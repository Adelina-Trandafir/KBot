Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>How an attempt to host a document ended.</summary>
Public Enum AdobeHostStatus
    ''' <summary>The window was found and is now a child of the host panel.</summary>
    Hosted = 0
    ''' <summary>No Adobe executable on this machine.</summary>
    AdobeMissing = 1
    ''' <summary>Adobe could not be started at all.</summary>
    LaunchFailed = 2
    ''' <summary>Adobe started but no matching window appeared before the timeout.</summary>
    WindowNotFound = 3
    ''' <summary>The document was cancelled/replaced while we were waiting.</summary>
    Superseded = 4
    ''' <summary>Something else went wrong; the message carries the detail.</summary>
    Failed = 5
End Enum

''' <summary>The status plus the Romanian sentence the operator should see.</summary>
Public NotInheritable Class AdobeHostResult

    Public ReadOnly Property Status As AdobeHostStatus
    ''' <summary>Operator-facing, Romanian, already usable as a label. Empty on success.</summary>
    Public ReadOnly Property Message As String
    ''' <summary>The profile that was actually applied (Nothing when nothing was hosted).</summary>
    Public ReadOnly Property Choice As AdobeProfileChoice
    ''' <summary>Milliseconds from launch to embedded — 0 when nothing was hosted.</summary>
    Public ReadOnly Property ElapsedMs As Integer
    ''' <summary>How the window was identified. <see cref="AdobeCaptureMatch.ByTitle"/> means foreign.</summary>
    Public ReadOnly Property Match As AdobeCaptureMatch

    Public Sub New(status As AdobeHostStatus, message As String, choice As AdobeProfileChoice,
                   Optional elapsedMs As Integer = 0,
                   Optional match As AdobeCaptureMatch = AdobeCaptureMatch.None)
        Me.Status = status
        Me.Message = If(message, "")
        Me.Choice = choice
        Me.ElapsedMs = elapsedMs
        Me.Match = match
    End Sub

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return Status = AdobeHostStatus.Hosted
        End Get
    End Property

End Class

''' <summary>
''' Hosts an Adobe window inside a panel.
'''
''' This is the shared engine of slice 0024: <c>ReaderHostPreview</c> and <c>DdfFisierPreview</c>
''' (KBot.App) drive it.
'''
''' PASS 03 CHANGED THE TWO THINGS THAT WERE ACTUALLY BROKEN:
'''  * the window is found by PROCESS ID and while still INVISIBLE, then hidden before anyone can
'''    see it — see <see cref="AdobeWindowCapture"/>;
'''  * teardown DROPS the window instead of handing it back. The old code restored the original
'''    style and re-parented to the desktop, which is how a stray Adobe window with a taskbar button
'''    survived every document change — see <see cref="AdobeWindowTeardown"/>.
'''
''' NO POSITIONING, NO HIDING (slice 0078-05, operator 29.09.2026). The window simply fills the
''' panel. The measured profiles (clipping the toolbar band off the top, pulling the window left,
''' «/A toolbar=0&amp;navpanes=0», hiding the floating badge) are gone from this class: Adobe
''' remembers the size and chrome of the window it closes, so the operator's own Adobe kept the
''' K-BOT layout after K-BOT was closed. The toolbars are hidden by Adobe's own Read Mode instead
''' (Ctrl+H, sent once when the page is laid out -- see <see cref="ArmReadMode"/>), which is a
''' state of the open document only and dies with it.
'''
''' WHAT THIS CLASS DELIBERATELY DOES NOT DO: it never writes <c>bEnableAv2</c>, or any other Adobe
''' preference. The single, documented exception (slice 0078) is <see cref="AdobePrefs"/>: the
''' standard Windows «Save As» dialog, written only when a SIGNING session starts, never by this class.
''' </summary>
Public NotInheritable Class AdobeReaderHost
    Implements IDisposable

    Private ReadOnly _host As IHostSurface
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _capture As AdobeWindowCapture
    Private ReadOnly _teardown As AdobeWindowTeardown
    ' Slice 0078-05: the opt-in last release at the size of the screen (RestoreScreenSizeOnExit).
    Private ReadOnly _screenRelease As AdobeScreenRelease
    ' The hosted window's style before it became a child -- only the screen release writes it back.
    Private _originalStyle As Long
    ' The K-BOT window holding the panel, watched for its closing (RestoreScreenSizeOnExit).
    Private _closingForm As Form
    Private ReadOnly _launcher As IAdobeLauncher
    Private ReadOnly _hook As AdobeCreationHook
    ' Slice 0078-02: forces Adobe's «Save As» onto the hosted document's own path.
    Private ReadOnly _saveTrap As AdobeSaveTrap
    ' The document hosted right now -- the ONLY path the save trap may ever force.
    Private _hostedPath As String

    ' Every process id THIS host started. A PID outside this set is never killed.
    Private ReadOnly _launchedPids As New HashSet(Of Integer)()

    Private _hostedWindow As IntPtr = IntPtr.Zero
    Private _hostedPid As Integer = 0
    Private _startedPid As Integer = 0
    ' A newer ShowDocument invalidates an in-flight one.
    Private _generation As Integer = 0

    Public Sub New(hostPanel As Control, log As Action(Of String))
        Me.New(New ControlHostSurface(hostPanel), log, Nothing, Nothing)
    End Sub

    ''' <summary>Full constructor — the seams default to the real ones. Tests pass fakes.</summary>
    Public Sub New(host As IHostSurface, log As Action(Of String),
                   Optional windows As INativeWindows = Nothing,
                   Optional launcher As IAdobeLauncher = Nothing)
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        _host = host
        _log = log
        _launcher = If(launcher, ProcessAdobeLauncher.Instance)
        _capture = New AdobeWindowCapture(windows)
        _teardown = New AdobeWindowTeardown(windows, _launcher)
        _screenRelease = New AdobeScreenRelease(windows, _launcher)
        _hook = New AdobeCreationHook(AddressOf Report)
        _saveTrap = New AdobeSaveTrap(AddressOf Report)
        AddHandler _saveTrap.Saved, AddressOf OnTrapSaved
        AddHandler _saveTrap.Failed, AddressOf OnTrapFailed
        AddHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
        _readModeTimer.Interval = ReadModeTickMs
        AddHandler _readModeTimer.Tick, AddressOf OnReadModeTick
    End Sub

    ''' <summary>
    ''' Slice 0078: raised (UI thread) after Adobe saved the hosted document through a trapped
    ''' «Save As». Argument = the document path. The file may still be settling on disk -- with a
    ''' signature, Adobe writes it only AFTER the dialog (and the token's PIN) is done.
    ''' </summary>
    Public Event DocumentSaved As Action(Of String)

    ''' <summary>Slice 0078: raised (UI thread) when a «Save As» had to be cancelled. Argument = Romanian reason.</summary>
    Public Event SaveTrapFailed As Action(Of String)

    ''' <summary>
    ''' Slice 0078: while True, every file dialog of the hosted Adobe is filled with the HOSTED
    ''' document's own path and confirmed, off screen (see <see cref="AdobeSaveTrap"/>). The target
    ''' is never a separate property: it cannot differ from the file actually on screen. Takes
    ''' effect immediately when a document is hosted, otherwise with the next one.
    ''' </summary>
    Public Property SaveTrapEnabled As Boolean
        Get
            Return _saveTrapEnabled
        End Get
        Set(value As Boolean)
            _saveTrapEnabled = value
            If value Then
                StartSaveTrap()
            Else
                _saveTrap.Stop()
            End If
        End Set
    End Property
    Private _saveTrapEnabled As Boolean

    ''' <summary>
    ''' Slice 0078-05: send Adobe's Read Mode (Ctrl+H) once the document is laid out, so the toolbars
    ''' are hidden by Adobe itself. True by default; the bench may switch it off.
    ''' </summary>
    Public Property ReadModeEnabled As Boolean = True

    ''' <summary>The document hosted right now, or Nothing.</summary>
    Public ReadOnly Property HostedPath As String
        Get
            Return _hostedPath
        End Get
    End Property

    ''' <summary>
    ''' True while a Save pressed by the trap has not finished (slice 0078-05, for the bench: the
    ''' document must not be released while Adobe may still be writing it).
    ''' </summary>
    Public ReadOnly Property IsSaving As Boolean
        Get
            Return _saveTrap.IsBusy
        End Get
    End Property

    ' Starts the trap on the current document, when there is one. Wrapped: called from a setter.
    Private Sub StartSaveTrap()
        Try
            If Not _saveTrapEnabled OrElse Not IsHosting OrElse String.IsNullOrEmpty(_hostedPath) Then Return
            _saveTrap.Start(_hostedPath, HostedPids())
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.StartSaveTrap", ex)
        End Try
    End Sub

    Private Function HostedPids() As List(Of Integer)
        Dim pids As New List(Of Integer)()
        If _hostedPid <> 0 Then pids.Add(_hostedPid)
        If _startedPid <> 0 AndAlso Not pids.Contains(_startedPid) Then pids.Add(_startedPid)
        Return pids
    End Function

    ' Trap events arrive on the UI thread (hook + WinForms timer); forwarded as they are.
    Private Sub OnTrapSaved(path As String)
        Try
            RaiseEvent DocumentSaved(path)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.OnTrapSaved", ex)
        End Try
    End Sub

    Private Sub OnTrapFailed(reason As String)
        Try
            RaiseEvent SaveTrapFailed(reason)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.OnTrapFailed", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Whether «/n» is used. <see cref="AdobeNewInstanceMode.Auto"/> = yes: a process of K-BOT's own,
    ''' so the window reparented is never one the operator opened.
    ''' </summary>
    Public Property NewInstanceMode As AdobeNewInstanceMode = AdobeNewInstanceMode.Auto

    ''' <summary>Capture and teardown knobs. Never Nothing; assigning Nothing restores the defaults.</summary>
    Public Property Options As AdobeHostOptions
        Get
            Return _options
        End Get
        Set(value As AdobeHostOptions)
            _options = If(value, New AdobeHostOptions())
        End Set
    End Property
    Private _options As New AdobeHostOptions()

    ''' <summary>The window hosted right now, or IntPtr.Zero.</summary>
    Public ReadOnly Property HostedWindow As IntPtr
        Get
            Return _hostedWindow
        End Get
    End Property

    ''' <summary>The process that owns the hosted window, or 0.</summary>
    Public ReadOnly Property HostedPid As Integer
        Get
            Return _hostedPid
        End Get
    End Property

    Public ReadOnly Property IsHosting As Boolean
        Get
            Return _hostedWindow <> IntPtr.Zero
        End Get
    End Property

    ''' <summary>The Adobe executable, resolved once per call (Nothing when not installed).</summary>
    Public Shared Function ResolveAdobePath() As String
        Return AdobeWindowHosting.ResolveAdobePath()
    End Function

    ''' <summary>
    ''' Opens <paramref name="pdfPath"/> and embeds its window. Every failure path returns a result
    ''' carrying a Romanian sentence the caller can put on screen — never an exception, never a
    ''' silent grey rectangle (§6 of the slice brief).
    ''' </summary>
    Public Async Function ShowDocumentAsync(pdfPath As String) As Task(Of AdobeHostResult)
        Try
            Detach()
            _generation += 1
            Dim gen As Integer = _generation

            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Return New AdobeHostResult(AdobeHostStatus.Failed,
                                           "Documentul nu există pe disc.", Nothing)
            End If

            Dim adobePath As String = _launcher.ResolvePath()
            If String.IsNullOrEmpty(adobePath) Then
                Report("Adobe Reader/Acrobat nu a fost găsit pe această mașină.")
                ' Deliberately NO fallback to the default handler: these are LiveCycle/XFA documents
                ' and no other product renders them — a "helpful" fallback would show a broken page.
                Return New AdobeHostResult(AdobeHostStatus.AdobeMissing,
                                           "Nu am găsit niciun produs Adobe instalat. Documentul nu poate fi afișat.",
                                           Nothing)
            End If

            Return Await LaunchAndHostAsync(adobePath, pdfPath, gen)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.ShowDocumentAsync", ex)
            Return New AdobeHostResult(AdobeHostStatus.Failed,
                                       "Documentul nu a putut fi afișat. Detalii în jurnalul de erori.", Nothing)
        End Try
    End Function

    ' One launch + embed cycle. Reached only from ShowDocumentAsync (wrapped) -- transitive coverage.
    Private Async Function LaunchAndHostAsync(adobePath As String, pdfPath As String, gen As Integer) As Task(Of AdobeHostResult)
        ' «/n» + «/s» and the file, nothing else: no /A open parameters (they hid toolbars and panes).
        Dim newInstance As Boolean = NewInstanceMode <> AdobeNewInstanceMode.Nu
        ' Extra switches go BEFORE the file name, like every other one -- Adobe ignores anything after it.
        Dim extras As String = If(String.IsNullOrWhiteSpace(_options.ExtraArgs), "", _options.ExtraArgs.Trim() & " ")
        Dim args As String = If(newInstance, "/n ", "") & "/s " & extras & """" & pdfPath & """"
        Report($"Pornesc Adobe: {Path.GetFileName(adobePath)} {args}")
        Report("  " & _options.Describe())

        Dim pid As Integer
        Try
            pid = _launcher.Start(adobePath, args)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.LaunchAndHostAsync.Start", ex)
            Return New AdobeHostResult(AdobeHostStatus.LaunchFailed,
                                       "Adobe nu a putut fi pornit. Detalii în jurnalul de erori.", Nothing)
        End Try

        _startedPid = pid
        If pid > 0 Then _launchedPids.Add(pid)

        ' The early catch, when it is switched on. Installed from THIS (UI) thread by contract.
        If _options.UseCreationHook Then _hook.Install(pid)

        ' The window is identified by the DOCUMENT NAME, and the PID only decides whether the match
        ' is labelled «ours» or «foreign»: Adobe hands the document to a running instance on EVERY
        ' launch, «/n» included, so a PID-strict search finds nothing.
        Dim opts As AdobeHostOptions = _options.Clone()
        Dim baseName As String = Path.GetFileNameWithoutExtension(pdfPath)
        Dim caught As AdobeCaptureResult =
            Await Task.Run(Function() _capture.Find(pid, baseName, opts)).ConfigureAwait(True)

        If _options.UseCreationHook Then _hook.Remove()

        If gen <> _generation Then
            ' A newer document took over while we waited. Ours must not be left running.
            AbandonLaunched(pid)
            Return New AdobeHostResult(AdobeHostStatus.Superseded, "", Nothing)
        End If

        If Not caught.Found Then
            Report($"Fereastra Adobe nu a apărut în {opts.FindTimeoutMs \ 1000} secunde.")
            Return New AdobeHostResult(AdobeHostStatus.WindowNotFound,
                                       "Adobe a pornit, dar fereastra documentului nu a apărut.",
                                       Nothing, caught.ElapsedMs, caught.Match)
        End If

        _hostedWindow = caught.Window
        _hostedPid = caught.OwnerPid
        _hostedPath = pdfPath
        Report($"Fereastră găsită în {caught.ElapsedMs} ms " &
               $"({If(caught.Match = AdobeCaptureMatch.ByPid, "după PID", "după titlu")}), PID {_hostedPid}.")

        ' The window we are about to reparent may belong to a process K-BOT did not create; closing
        ' it later would close THEIR work. Said loudly.
        If caught.Match = AdobeCaptureMatch.ByTitle Then
            Report($"ATENȚIE: fereastra încorporată (PID {_hostedPid}) NU a fost creată de K-BOT " &
                   $"(am pornit PID {_startedPid}). Adobe a predat documentul unei instanțe existente — " &
                   "procesul acela NU va fi închis de K-BOT.")
        End If

        ' The window is hidden at this point (AdobeWindowCapture hid it on sight). Everything from
        ' here to Reveal happens off screen.
        _originalStyle = _capture.AttachAsChild(_hostedWindow, _host.Handle).ToInt64()
        WatchClosingForm()
        Fill("Poziție")
        ' Only now does the window become visible — inside the panel, filling it.
        _capture.Reveal(_hostedWindow)

        ' Slice 0078: armed as soon as the window is visible -- the operator can sign from now on.
        StartSaveTrap()

        ' Adobe finishes its layout after the window appears; without this second pass the reparented
        ' window can stay blank.
        Await Task.Delay(_options.RedrawDelayMs).ConfigureAwait(True)
        If gen <> _generation Then Return New AdobeHostResult(AdobeHostStatus.Superseded, "", Nothing)
        Fill("Poziție (a doua trecere)")

        If ReadModeEnabled Then ArmReadMode()
        Return New AdobeHostResult(AdobeHostStatus.Hosted, "", Nothing, caught.ElapsedMs, caught.Match)
    End Function

    ' A superseded launch must not leak a process. Only ever applied to a PID we started ourselves.
    Private Sub AbandonLaunched(pid As Integer)
        Try
            If pid <= 0 OrElse Not _launchedPids.Contains(pid) Then Return
            Report($"Cerere depășită: opresc procesul {pid}, pornit pentru documentul abandonat.")
            _launcher.Kill(pid)
            _launchedPids.Remove(pid)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.AbandonLaunched", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Makes the hosted window fill the panel again. Called on every host resize; safe (and silent)
    ''' when nothing is hosted.
    ''' </summary>
    Public Sub Relayout()
        Try
            If Not IsHosting Then Return
            Fill(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Relayout", ex)
        End Try
    End Sub

    ' The window gets exactly the panel's client area -- no offset, no oversize. REPORTS WHAT
    ' ACTUALLY HAPPENED: Adobe can refuse or clamp a size. Nothing as `what` = the silent resize path.
    Private Sub Fill(what As String)
        If Not IsHosting Then Return
        Dim size As Size = _host.ClientSize
        If size.Width <= 0 OrElse size.Height <= 0 Then Return
        Dim wanted As New Rectangle(0, 0, size.Width, size.Height)

        Dim before As Rectangle = AdobeWindowHosting.RectInParent(_hostedWindow)
        AdobeWindowHosting.Place(_hostedWindow, wanted)
        AdobeWindowHosting.NudgeRedraw(_hostedWindow, wanted)
        _host.Invalidate()
        If String.IsNullOrEmpty(what) Then Return

        Dim after As Rectangle = AdobeWindowHosting.RectInParent(_hostedWindow)
        Dim outcome As MoveOutcome = MoveOutcomeClassifier.Classify(True, True, before, after)
        Report($"{what}: cerut {MoveOutcomeClassifier.Describe(wanted)} — " &
               $"{MoveOutcomeClassifier.Label(outcome)} {MoveOutcomeClassifier.Describe(before)} -> " &
               MoveOutcomeClassifier.Describe(after))
        If after <> wanted Then
            Report("  ATENȚIE: fereastra nu a ajuns la dreptunghiul cerut (Adobe a refuzat sau a limitat).")
        End If
    End Sub

    ' ── Read Mode (Ctrl+H) ──────────────────────────────────────────────────────
    '
    ' Same conditions as the ActiveX surface's Read Mode, checked on a short timer (the hosted window
    ' has no load event of its own):
    '  1. the panel is on screen;
    '  2. the page view («AVPageView») inside the hosted window is visible and has a size;
    '  3. no burst of script alerts is running (AdobeSaveTrap.InScriptBurst) -- Ctrl+H sent while
    '     Adobe recovers from them did not hold on the ActiveX engine (24.09.2026);
    '  4. the K-BOT form holding the panel is ENABLED (no modal alert) and is the FOREGROUND window
    '     -- SendInput types into whatever has the keyboard;
    '  5. after SetFocus on the page view, the keyboard focus really is inside the hosted window.
    ' Sent ONCE per document. When the conditions never hold, the operator is told to press Ctrl+H.
    ' Read Mode is a state of the open document: nothing of it is saved in Adobe's preferences.

    Private Const ReadModeTickMs As Integer = 150
    Private Const ReadModeMaxMs As Integer = 30000
    Private Const PageViewTitle As String = "AVPageView"
    Private Const VK_H As UShort = &H48US
    Private Const VK_S As UShort = &H53US
    Private ReadOnly _readModeTimer As New System.Windows.Forms.Timer()
    ' Slice 0078-05: the Ctrl+<key> combinations waiting for the conditions above (H = Read Mode,
    ' S = the save K-BOT asks for after a signature, see RequestSave). Sent in order, each once.
    Private ReadOnly _pendingKeys As New List(Of UShort)()
    Private _readModeUntil As DateTime
    Private _readModeWait As String
    ' Measured 29.09.2026: right after the Token Logon closed, SetFocus on the page did not take and
    ' Ctrl+S was dropped. The focus is tried again (the Adobe window first, then the page) a few
    ' times, a little later each time, before giving up.
    Private Const FocusAttempts As Integer = 3
    Private Const FocusRetryMs As Integer = 500
    Private _focusFailures As Integer
    Private _retryNotBefore As DateTime = DateTime.MinValue

    ''' <summary>
    ''' Slice 0078-05: raised (UI thread) when the save K-BOT asked for (<see cref="RequestSave"/>)
    ''' could not be sent to Adobe. Argument = the Romanian sentence for the operator, who must save
    ''' by hand or the change the form made after the signature is lost.
    ''' </summary>
    Public Event SaveNotSent As Action(Of String)

    ''' <summary>Slice 0078-05: raised (UI thread) when the Ctrl+S asked for by <see cref="RequestSave"/> went out.</summary>
    Public Event SaveKeysSent As Action

    ' Reached from LaunchAndHostAsync (wrapped).
    Private Sub ArmReadMode()
        QueueKey(VK_H)
    End Sub

    ''' <summary>
    ''' Slice 0078-05: asks the hosted Adobe to SAVE the document (Ctrl+S, under the same conditions
    ''' as Read Mode; a «Save As» it may show goes through the trap as usual). Used after a signature:
    ''' the form's postSign scripts change the document after it was saved, and that change -- which
    ''' unlocks the next signer -- is lost unless it is saved too. False when nothing is hosted.
    ''' </summary>
    Public Function RequestSave() As Boolean
        Try
            If Not IsHosting Then Return False
            Report("Salvare cerută de K-BOT după semnătură: Ctrl+S în așteptare.")
            QueueKey(VK_S)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.RequestSave", ex)
            Return False
        End Try
    End Function

    Private Sub QueueKey(vk As UShort)
        If Not _pendingKeys.Contains(vk) Then _pendingKeys.Add(vk)
        _focusFailures = 0
        _retryNotBefore = DateTime.MinValue
        _readModeWait = Nothing
        _readModeUntil = DateTime.UtcNow.AddMilliseconds(ReadModeMaxMs)
        _readModeTimer.Start()
    End Sub

    Private Sub DisarmReadMode()
        _readModeTimer.Stop()
        _pendingKeys.Clear()
    End Sub

    ' Trap event, UI thread. Boundary: log and swallow.
    Private Sub OnScriptBurstEnded()
        Try
            If _readModeTimer.Enabled Then TrySendPendingKeys()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.OnScriptBurstEnded", ex)
        End Try
    End Sub

    ' Timer: log and swallow.
    Private Sub OnReadModeTick(sender As Object, e As EventArgs)
        Try
            TrySendPendingKeys()
        Catch ex As Exception
            DisarmReadMode()
            GlobalErrorLog.Write("AdobeReaderHost.OnReadModeTick", ex)
        End Try
    End Sub

    ' A combination that will not be sent: logged; for the save, the operator is told as well.
    Private Sub GiveUp(vk As UShort, reason As String)
        Report($"{reason} {ManualHint(vk)}")
        If vk = VK_S Then
            RaiseEvent SaveNotSent("K-BOT nu a putut salva documentul după semnătură (Adobe nu a primit tastele)." &
                                   Environment.NewLine & Environment.NewLine &
                                   "Faceți clic în document și apăsați Ctrl+S, ca schimbările făcute de formular după " &
                                   "semnare să fie salvate. Fără ele, semnătura următoare nu se poate aplica.")
        End If
    End Sub

    Private Shared Function KeyName(vk As UShort) As String
        Return "Ctrl+" & ChrW(vk)
    End Function

    ' What the operator must do by hand when a combination could not be sent.
    Private Shared Function ManualHint(vk As UShort) As String
        If vk = VK_S Then Return "Salvați documentul (Ctrl+S) ca să nu se piardă schimbarea de după semnătură."
        Return "Apăsați Ctrl+H în document pentru a ascunde barele."
    End Function

    Private Sub TrySendPendingKeys()
        If Not IsHosting OrElse _pendingKeys.Count = 0 Then
            DisarmReadMode()
            Return
        End If
        If DateTime.UtcNow < _retryNotBefore Then Return
        Dim names As String = String.Join(", ", _pendingKeys.Select(Function(k) KeyName(k)))
        Dim page As IntPtr
        Dim wait As String = ReadModeBlocker(page)
        If wait IsNot Nothing Then
            If DateTime.UtcNow > _readModeUntil Then
                Dim failed As List(Of UShort) = _pendingKeys.ToList()
                DisarmReadMode()
                For Each vk As UShort In failed
                    GiveUp(vk, $"{KeyName(vk)} NU a fost trimis în {ReadModeMaxMs \ 1000} s ({wait}).")
                Next
            ElseIf wait <> _readModeWait Then
                _readModeWait = wait
                Report($"{names}: aștept — {wait}")
            End If
            Return
        End If

        ' After a failed attempt the Adobe window itself is focused first, then the page.
        If _focusFailures > 0 Then AdobeNativeMethods.SetFocus(_hostedWindow)
        AdobeNativeMethods.SetFocus(page)
        Dim focus As IntPtr = AdobeNativeMethods.GetFocus()
        If focus <> page AndAlso focus <> _hostedWindow AndAlso Not AdobeNativeMethods.IsChild(_hostedWindow, focus) Then
            _focusFailures += 1
            If _focusFailures < FocusAttempts Then
                ' Keys stay queued; the timer tries again after a pause.
                Report($"{names}: documentul nu a primit focusul tastaturii (focus la 0x{focus.ToInt64():X}) — " &
                       $"reîncerc peste {FocusRetryMs} ms (încercarea {_focusFailures + 1}/{FocusAttempts}).")
                _retryNotBefore = DateTime.UtcNow.AddMilliseconds(FocusRetryMs)
                _readModeUntil = DateTime.UtcNow.AddMilliseconds(ReadModeMaxMs)
                Return
            End If
            Dim failed As List(Of UShort) = _pendingKeys.ToList()
            DisarmReadMode()
            For Each vk As UShort In failed
                GiveUp(vk, $"ATENȚIE — documentul nu a primit focusul tastaturii după {FocusAttempts} încercări; " &
                           $"{KeyName(vk)} NU a fost trimis (ar fi ajuns în K-BOT).")
            Next
            Return
        End If

        Dim keysToSend As List(Of UShort) = _pendingKeys.ToList()
        DisarmReadMode()

        For Each vk As UShort In keysToSend
            Dim keys As AdobeNativeMethods.INPUT() = {
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(vk, False),
                AdobeNativeMethods.KeyInput(vk, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)}
            Dim sent As UInteger = AdobeNativeMethods.SendInput(CUInt(keys.Length), keys,
                Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
            If sent = keys.Length Then
                Report($"{KeyName(vk)} trimis documentului.")
                If vk = VK_S Then RaiseEvent SaveKeysSent()
            Else
                Dim err As Integer = Runtime.InteropServices.Marshal.GetLastWin32Error()
                GiveUp(vk, $"ATENȚIE — {KeyName(vk)} nu a putut fi trimis ({sent}/{keys.Length} taste, eroarea {err}).")
            End If
        Next
    End Sub

    ' Nothing when the pending keys may be sent now; otherwise why not (Romanian, for the log).
    Private Function ReadModeBlocker(ByRef page As IntPtr) As String
        page = IntPtr.Zero
        If _host.Handle = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindowVisible(_host.Handle) Then Return "panoul nu e pe ecran"
        For Each h As IntPtr In AdobeNativeMethods.Descendants(_hostedWindow)
            If Not String.Equals(AdobeNativeMethods.GetTitle(h), PageViewTitle, StringComparison.Ordinal) Then Continue For
            If Not AdobeNativeMethods.IsWindowVisible(h) Then Continue For
            Dim r As Rectangle = AdobeNativeMethods.RectInParent(h)
            If r.Width > 0 AndAlso r.Height > 0 Then
                page = h
                Exit For
            End If
        Next
        If page = IntPtr.Zero Then Return "pagina nu e încă așezată"
        If _saveTrap.InScriptBurst Then Return "mesaje de script în curs"
        Dim form As IntPtr = AdobeNativeMethods.GetAncestor(_host.Handle, AdobeNativeMethods.GA_ROOT)
        If form = IntPtr.Zero Then Return "panoul nu e într-o fereastră"
        If Not AdobeNativeMethods.IsWindowEnabled(form) Then Return "fereastra K-BOT e blocată de un mesaj"
        Dim fg As IntPtr = AdobeNativeMethods.GetForegroundWindow()
        If fg <> form Then
            ' Measured 29.09.2026: ~4.5 s of every open were lost here -- the foreground is taken by
            ' the Adobe process launched with «/n» (it hands the document over and lingers). When the
            ' foreground belongs to an Adobe process, K-BOT takes it back instead of waiting.
            Dim fgPid As Integer = AdobeNativeMethods.OwnerPid(fg)
            Dim fgText As String = $"0x{fg.ToInt64():X} «{AdobeNativeMethods.GetTitle(fg)}» clasă={AdobeNativeMethods.GetClass(fg)} proces={fgPid}"
            If IsAdobeProcess(fgPid) AndAlso AdobeNativeMethods.SetForegroundWindow(form) AndAlso
               AdobeNativeMethods.GetForegroundWindow() = form Then
                Report($"Prim-planul era la Adobe ({fgText}) — readus la K-BOT.")
                Return Nothing
            End If
            Return $"fereastra K-BOT nu e în prim-plan (prim-plan: {fgText})"
        End If
        Return Nothing
    End Function

    ' The foreground belongs to the hosted / launched Adobe, or to any Adobe viewer process.
    Private Function IsAdobeProcess(pid As Integer) As Boolean
        If pid <= 0 Then Return False
        If pid = _hostedPid OrElse pid = _startedPid OrElse _launchedPids.Contains(pid) Then Return True
        Return AdobeWindowHosting.AdobeProcessIds().Contains(pid)
    End Function

    ''' <summary>
    ''' Lets the hosted window go, in the mode <see cref="Options"/> selects.
    '''
    ''' THE WINDOW IS NEVER HANDED BACK. Restoring its style and re-parenting it to the desktop is
    ''' what left a stray Adobe window — with a taskbar button, showing the previous document —
    ''' behind every document change before pass 03. See <see cref="AdobeWindowTeardown"/>.
    ''' </summary>
    Public Sub Detach()
        Try
            DisarmReadMode()
            _hook.Remove()
            ' Slice 0078: a Save we pressed must finish before Adobe is closed or killed, or the
            ' signed file could be cut in half.
            If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
            _saveTrap.Stop()
            _hostedPath = Nothing

            Dim hwnd As IntPtr = _hostedWindow
            Dim pid As Integer = _hostedPid
            ' Cleared FIRST, so a teardown that fails cannot leave a stale handle behind to be
            ' re-used against a window that no longer exists.
            _hostedWindow = IntPtr.Zero
            _hostedPid = 0
            _startedPid = 0

            If hwnd = IntPtr.Zero AndAlso pid <= 0 Then Return

            Dim outcome As AdobeTeardownOutcome =
                _teardown.Run(hwnd, pid, _launchedPids, _options.DetachMode, _options.CloseGraceMs)
            If outcome.Message.Length > 0 Then Report(outcome.Message)
            If outcome.Action = AdobeTeardownAction.Killed OrElse
               outcome.Action = AdobeTeardownAction.ClosedThenKilled Then
                _launchedPids.Remove(pid)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Detach", ex)
        End Try
    End Sub

    ' ── Last release at the size of the screen (slice 0078-05, opt-in) ────────────────────────

    ' Watches the form that holds the panel: its closing is the last release of the window.
    ' Reached from LaunchAndHostAsync (wrapped).
    Private Sub WatchClosingForm()
        Dim surface As ControlHostSurface = TryCast(_host, ControlHostSurface)
        Dim form As Form = surface?.Control.FindForm()
        If form Is _closingForm Then Return
        UnwatchClosingForm()
        _closingForm = form
        If _closingForm IsNot Nothing Then AddHandler _closingForm.FormClosing, AddressOf OnClosingFormClosing
    End Sub

    Private Sub UnwatchClosingForm()
        If _closingForm Is Nothing Then Return
        RemoveHandler _closingForm.FormClosing, AddressOf OnClosingFormClosing
        _closingForm = Nothing
    End Sub

    ' UI boundary (event handler): log and swallow.
    Private Sub OnClosingFormClosing(sender As Object, e As FormClosingEventArgs)
        Try
            If e.Cancel Then Return
            ReleaseAtScreenSize()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.OnClosingFormClosing", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The last release, when <see cref="AdobeHostOptions.RestoreScreenSizeOnExit"/> is on: the
    ''' window leaves the panel, is maximized over the screen it is on and closed there, so Adobe
    ''' keeps that size instead of the panel's (see <see cref="AdobeScreenRelease"/>). Does nothing
    ''' when the option is off or nothing is hosted -- <see cref="Detach"/> stays the release then.
    ''' </summary>
    Private Sub ReleaseAtScreenSize()
        Try
            If Not _options.RestoreScreenSizeOnExit OrElse Not IsHosting Then Return
            DisarmReadMode()
            _hook.Remove()
            If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
            _saveTrap.Stop()
            _hostedPath = Nothing

            Dim hwnd As IntPtr = _hostedWindow
            Dim pid As Integer = _hostedPid
            _hostedWindow = IntPtr.Zero
            _hostedPid = 0
            _startedPid = 0

            Dim area As Rectangle = Screen.FromHandle(hwnd).WorkingArea
            Dim killed As Boolean
            Dim message As String = _screenRelease.Run(hwnd, pid, _originalStyle, area, _launchedPids, killed)
            If message.Length > 0 Then Report(message)
            If killed Then _launchedPids.Remove(pid)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.ReleaseAtScreenSize", ex)
        End Try
    End Sub

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            UnwatchClosingForm()
            ReleaseAtScreenSize()
            Detach()
            _readModeTimer.Dispose()
            _hook.Dispose()
            RemoveHandler _saveTrap.Saved, AddressOf OnTrapSaved
            RemoveHandler _saveTrap.Failed, AddressOf OnTrapFailed
            RemoveHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
            _saveTrap.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Dispose", ex)
        End Try
    End Sub

End Class
