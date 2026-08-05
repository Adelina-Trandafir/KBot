Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
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

    Public Sub New(status As AdobeHostStatus, message As String, choice As AdobeProfileChoice)
        Me.Status = status
        Me.Message = If(message, "")
        Me.Choice = choice
    End Sub

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return Status = AdobeHostStatus.Hosted
        End Get
    End Property

End Class

''' <summary>
''' Hosts an Adobe window inside a panel, under a <see cref="AdobeViewerProfile"/>.
'''
''' This is the shared engine of slice 0024: <c>ReaderHostPreview</c> (KBot.App) drives it, and
''' <c>AdobeReaderHarnessForm</c> (KBot.DevHarness) uses the same primitives underneath
''' (<see cref="AdobeWindowHosting"/>, <see cref="AdobeWindowProbe"/>,
''' <see cref="AdobeHostGeometry"/>) so the bench and the shipping preview cannot drift apart.
'''
''' WHAT THIS CLASS DELIBERATELY DOES NOT DO: it never writes <c>bEnableAv2</c>, or any other Adobe
''' preference. That value changes the operator's Adobe everywhere, for every PDF they open, K-BOT
''' or not. Writing it silently on every DDF preview is unacceptable and prompting on every preview
''' is unusable, so the shipping code ADAPTS to whichever UI it finds — which is exactly what
''' <see cref="AdobeUiDetector"/> makes possible. The bench writes it because the bench is a bench.
''' </summary>
Public NotInheritable Class AdobeReaderHost
    Implements IDisposable

    Private ReadOnly _hostPanel As Control
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _watcher As AdobePopupWatcher

    ' The window we host now, plus what it takes to give it back.
    Private _hostedWindow As IntPtr = IntPtr.Zero
    Private _originalParent As IntPtr = IntPtr.Zero
    Private _originalStyle As IntPtr = IntPtr.Zero
    ' The process WE started (may be Nothing, and may not own the hosted window — see §1 of the
    ' slice brief: the modern profile launches without «/n»).
    Private _readerProcess As Process
    Private _startedPid As Integer = 0
    Private _hostedPid As Integer = 0
    ' A newer ShowDocument invalidates an in-flight one.
    Private _generation As Integer = 0
    ' Cached across documents, so the SECOND document already launches with the right flags.
    Private _lastGeneration As AdobeUiGeneration = AdobeUiGeneration.Unknown
    Private _relaunchedForProfile As Boolean = False

    Public Sub New(hostPanel As Control, log As Action(Of String))
        If hostPanel Is Nothing Then Throw New ArgumentNullException(NameOf(hostPanel))
        _hostPanel = hostPanel
        _log = log
        _watcher = New AdobePopupWatcher(AddressOf Report)
    End Sub

    ''' <summary>Which profile the operator asked for. Changing it takes effect on the next document.</summary>
    Public Property Mode As AdobeViewerMode = AdobeViewerMode.Auto

    ''' <summary>Whether «/n» is forced on or off, whatever the profile says.</summary>
    Public Property NewInstanceMode As AdobeNewInstanceMode = AdobeNewInstanceMode.Auto

    ''' <summary>
    ''' Whether the floating-badge watcher may run. FALSE by default so the bench, which shares this
    ''' code, keeps behaving exactly as it did before the extraction; the DDF preview turns it on.
    ''' </summary>
    Public Property PopupWatchEnabled As Boolean = False

    ''' <summary>The profile in force right now (Nothing before the first document).</summary>
    Public ReadOnly Property CurrentChoice As AdobeProfileChoice

    ''' <summary>The last detection, or Nothing if nothing has been probed yet.</summary>
    Public ReadOnly Property LastDetection As AdobeUiDetection

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
            _relaunchedForProfile = False

            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Return New AdobeHostResult(AdobeHostStatus.Failed,
                                           "Documentul nu există pe disc.", Nothing)
            End If

            Dim adobePath As String = AdobeWindowHosting.ResolveAdobePath()
            If String.IsNullOrEmpty(adobePath) Then
                Report("Adobe Reader/Acrobat nu a fost găsit pe această mașină.")
                Return New AdobeHostResult(AdobeHostStatus.AdobeMissing,
                                           "Adobe Reader/Acrobat nu este instalat — documentul nu poate fi afișat aici.",
                                           Nothing)
            End If

            Return Await LaunchAndHostAsync(adobePath, pdfPath, gen, allowRelaunch:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.ShowDocumentAsync", ex)
            Return New AdobeHostResult(AdobeHostStatus.Failed,
                                       "Documentul nu a putut fi afișat. Detalii în jurnalul de erori.", Nothing)
        End Try
    End Function

    ' One launch + embed cycle. Reached only from ShowDocumentAsync (wrapped) and from itself for the
    ' single profile relaunch — transitive coverage, house rule.
    Private Async Function LaunchAndHostAsync(adobePath As String, pdfPath As String, gen As Integer,
                                              allowRelaunch As Boolean) As Task(Of AdobeHostResult)
        ' The launch flags must be decided BEFORE we can see the window, so the first document uses
        ' the last generation we detected (Unknown -> classic, the conservative profile). If the
        ' detection afterwards disagrees, we relaunch ONCE with the right flags — see below.
        Dim launchChoice As AdobeProfileChoice =
            AdobeViewerProfiles.Resolve(Mode, New AdobeUiDetection(_lastGeneration, "din rularea anterioară", False))
        Dim launchProfile As AdobeViewerProfile = launchChoice.Profile.WithNewInstance(NewInstanceMode)

        Dim args As String = AdobeWindowHosting.BuildArguments(launchProfile, pdfPath)
        Report($"Pornesc Adobe: {Path.GetFileName(adobePath)} {args}")
        Report("  " & launchProfile.Describe())

        Dim proc As Process
        Try
            proc = Process.Start(New ProcessStartInfo(adobePath, args) With {.UseShellExecute = False})
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.LaunchAndHostAsync.Start", ex)
            Return New AdobeHostResult(AdobeHostStatus.LaunchFailed,
                                       "Adobe nu a putut fi pornit. Detalii în jurnalul de erori.", Nothing)
        End Try

        _readerProcess = proc
        _startedPid = SafePid(proc)

        Dim baseName As String = Path.GetFileNameWithoutExtension(pdfPath)
        Dim hwnd As IntPtr = Await Task.Run(Function() AdobeWindowHosting.FindReaderWindow(baseName)).ConfigureAwait(True)

        If gen <> _generation Then
            Return New AdobeHostResult(AdobeHostStatus.Superseded, "", Nothing)
        End If

        If hwnd = IntPtr.Zero Then
            Report($"Fereastra Adobe nu a apărut în {AdobeWindowHosting.FindTimeoutMs \ 1000} secunde.")
            Return New AdobeHostResult(AdobeHostStatus.WindowNotFound,
                                       "Documentul s-a deschis în Adobe, dar fereastra nu a putut fi încorporată.",
                                       Nothing)
        End If

        ' §1: the modern profile launches WITHOUT «/n», so Adobe may hand the document to an instance
        ' the operator already had open. Say so loudly — the window we are about to reparent then
        ' belongs to a process K-BOT did not create, and closing it later would close THEIR work.
        _hostedPid = AdobeWindowHosting.OwnerPid(hwnd)
        If _startedPid <> 0 AndAlso _hostedPid <> 0 AndAlso _hostedPid <> _startedPid Then
            Report($"ATENȚIE: fereastra încorporată (PID {_hostedPid}) NU a fost creată de K-BOT " &
                   $"(am pornit PID {_startedPid}). Adobe a predat documentul unei instanțe existente — " &
                   "procesul acela NU va fi închis de K-BOT. Setează «Instanță nouă Adobe» pe «Da» " &
                   "dacă vrei o instanță separată.")
        End If

        _hostedWindow = hwnd
        _originalParent = AdobeNativeMethods.GetParent(hwnd)
        _originalStyle = AdobeWindowHosting.AttachAsChild(hwnd, _hostPanel.Handle)

        ' Detection happens on the window tree we now own, never on the registry.
        Dim nodes As List(Of AdobeWindowNode) = AdobeWindowProbe.Walk(hwnd, _hostPanel.Handle)
        Dim detection As AdobeUiDetection = AdobeUiDetector.Detect(nodes)
        _LastDetection = detection
        If detection.Generation <> AdobeUiGeneration.Unknown Then _lastGeneration = detection.Generation
        Report(detection.Describe())
        If detection.Generation = AdobeUiGeneration.Unknown Then
            ' The full tree, so the next person has the evidence rather than a verdict.
            For Each n As AdobeWindowNode In nodes
                Report(AdobeWindowProbe.DescribeNode(n))
            Next
        End If

        Dim choice As AdobeProfileChoice = AdobeViewerProfiles.Resolve(Mode, detection)
        Dim profile As AdobeViewerProfile = choice.Profile.WithNewInstance(NewInstanceMode)
        _CurrentChoice = choice
        If choice.Mismatch Then
            Report($"ATENȚIE: setarea forțează profilul «{choice.Profile.Name}», dar arborele de ferestre " &
                   $"arată o interfață {If(choice.Detected = AdobeUiGeneration.Modern, "modernă", "clasică")}. " &
                   "Previzualizarea va arăta greșit — schimbă «Mod vizualizator Adobe» pe «Automat».")
        End If

        ' Auto only: if the detected profile needs DIFFERENT LAUNCH FLAGS from the ones we just used,
        ' relaunch once. Geometry can be re-applied in place; «/n», «/s» and the /A parameters cannot.
        If allowRelaunch AndAlso Not _relaunchedForProfile AndAlso NeedsRelaunch(launchProfile, profile) Then
            _relaunchedForProfile = True
            Report($"Profil detectat «{profile.Name}» diferă de cel de pornire «{launchProfile.Name}» " &
                   "la parametrii de lansare — repornesc Adobe o singură dată cu profilul corect.")
            Detach()
            _generation += 1
            Return Await LaunchAndHostAsync(adobePath, pdfPath, _generation, allowRelaunch:=False).ConfigureAwait(True)
        End If

        ApplyGeometry(profile, "Poziție")

        If PopupWatchEnabled AndAlso profile.HidePopups Then
            Dim pids As New List(Of Integer)()
            If _hostedPid <> 0 Then pids.Add(_hostedPid)
            If _startedPid <> 0 AndAlso Not pids.Contains(_startedPid) Then pids.Add(_startedPid)
            _watcher.Start(_hostPanel.Handle, pids)
            _watcher.Sweep()
        End If

        ' Adobe finishes its layout after the window appears; without this second pass the reparented
        ' window can stay blank.
        Await Task.Delay(AdobeWindowHosting.RedrawDelayMs).ConfigureAwait(True)
        If gen <> _generation Then Return New AdobeHostResult(AdobeHostStatus.Superseded, "", Nothing)
        ApplyGeometry(profile, "Poziție (a doua trecere)")

        Dim note As String = ""
        If detection.Generation = AdobeUiGeneration.Unknown Then note = AdobeUiDetector.UnrecognisedNote
        Return New AdobeHostResult(AdobeHostStatus.Hosted, note, choice)
    End Function

    ' Only LAUNCH-time differences justify a relaunch; geometry is re-applied in place.
    Private Shared Function NeedsRelaunch(used As AdobeViewerProfile, wanted As AdobeViewerProfile) As Boolean
        If used Is Nothing OrElse wanted Is Nothing Then Return False
        Return used.NewInstance <> wanted.NewInstance OrElse
               used.NoSplash <> wanted.NoSplash OrElse
               Not String.Equals(used.OpenParametersText(), wanted.OpenParametersText(), StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Re-places the hosted window under the current profile. Called on every host resize; safe (and
    ''' silent) when nothing is hosted.
    ''' </summary>
    Public Sub Relayout()
        Try
            If Not IsHosting OrElse _CurrentChoice Is Nothing Then Return
            ApplyGeometry(_CurrentChoice.Profile.WithNewInstance(NewInstanceMode), Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Relayout", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Re-applies a (possibly new) profile to the window already hosted, without relaunching. This
    ''' is what makes the setting take effect on the CURRENT document: geometry, yes — launch flags
    ''' need the next document.
    ''' </summary>
    Public Sub ReapplyProfile()
        Try
            If Not IsHosting Then Return
            Dim choice As AdobeProfileChoice = AdobeViewerProfiles.Resolve(Mode, _LastDetection)
            _CurrentChoice = choice
            Report("Profil reaplicat pe documentul curent: " & choice.Profile.Describe())
            If choice.Mismatch Then
                Report("ATENȚIE: profilul forțat nu se potrivește cu interfața Adobe detectată.")
            End If
            ApplyGeometry(choice.Profile.WithNewInstance(NewInstanceMode), "Poziție (profil reaplicat)")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.ReapplyProfile", ex)
        End Try
    End Sub

    ' Places the window and REPORTS WHAT ACTUALLY HAPPENED — Adobe can refuse or clamp a size, and a
    ' request it ignored must not read like a success (the lesson pass 4 of slice 0023 paid for).
    ' Pass Nothing as `what` for the silent resize path, which would otherwise flood the log.
    Private Sub ApplyGeometry(profile As AdobeViewerProfile, what As String)
        If Not IsHosting Then Return
        Dim wanted As Rectangle = AdobeHostGeometry.Compute(_hostPanel.ClientSize, profile)
        If wanted.Width <= 0 OrElse wanted.Height <= 0 Then Return

        Dim before As Rectangle = AdobeWindowHosting.RectInParent(_hostedWindow)
        AdobeWindowHosting.Place(_hostedWindow, wanted)
        AdobeWindowHosting.NudgeRedraw(_hostedWindow, wanted)
        _hostPanel.Invalidate(True)
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

    ''' <summary>
    ''' Gives the window back and stops watching. The process is closed ONLY when K-BOT created it
    ''' AND it owns the hosted window: with «/n» off Adobe may have handed the document to the
    ''' operator's own instance, and closing that would take their other documents with it.
    ''' Best-effort by construction — every step here can legitimately meet a dead handle.
    ''' </summary>
    Public Sub Detach()
        Try
            _watcher.Stop()

            If _hostedWindow <> IntPtr.Zero Then
                AdobeWindowHosting.RestoreStandalone(_hostedWindow, _originalStyle, _originalParent)
                _hostedWindow = IntPtr.Zero
                _originalParent = IntPtr.Zero
                _originalStyle = IntPtr.Zero
            End If

            Dim proc As Process = _readerProcess
            _readerProcess = Nothing
            If proc IsNot Nothing Then
                Try
                    If _hostedPid <> 0 AndAlso _startedPid <> 0 AndAlso _hostedPid <> _startedPid Then
                        Report($"Nu închid Adobe: fereastra aparținea procesului {_hostedPid}, " &
                               "pe care nu l-am pornit noi.")
                    ElseIf Not proc.HasExited Then
                        proc.CloseMainWindow()
                    End If
                Catch
                    ' Best-effort: the process may already be gone.
                End Try
                Try
                    proc.Dispose()
                Catch
                End Try
            End If
            _startedPid = 0
            _hostedPid = 0
            _CurrentChoice = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Detach", ex)
        End Try
    End Sub

    Private Shared Function SafePid(proc As Process) As Integer
        Try
            Return If(proc Is Nothing, 0, proc.Id)
        Catch
            Return 0
        End Try
    End Function

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            Detach()
            _watcher.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Dispose", ex)
        End Try
    End Sub

End Class
