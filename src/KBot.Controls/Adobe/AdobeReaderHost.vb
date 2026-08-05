Option Strict On
Imports System.Collections.Generic
Imports System.IO
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
''' Hosts an Adobe window inside a panel, under a <see cref="AdobeViewerProfile"/>.
'''
''' This is the shared engine of slice 0024: <c>ReaderHostPreview</c> (KBot.App) drives it, and
''' <c>AdobeReaderHarnessForm</c> (KBot.DevHarness) uses the same primitives underneath
''' (<see cref="AdobeWindowCapture"/>, <see cref="AdobeWindowTeardown"/>,
''' <see cref="AdobeWindowHosting"/>, <see cref="AdobeWindowProbe"/>,
''' <see cref="AdobeHostGeometry"/>) so the bench and the shipping preview cannot drift apart.
'''
''' PASS 03 CHANGED THE TWO THINGS THAT WERE ACTUALLY BROKEN:
'''  * the window is found by PROCESS ID and while still INVISIBLE, then hidden before anyone can
'''    see it — see <see cref="AdobeWindowCapture"/>;
'''  * teardown DROPS the window instead of handing it back. The old code restored the original
'''    style and re-parented to the desktop, which is how a stray Adobe window with a taskbar button
'''    survived every document change — see <see cref="AdobeWindowTeardown"/>.
'''
''' WHAT THIS CLASS DELIBERATELY DOES NOT DO: it never writes <c>bEnableAv2</c>, or any other Adobe
''' preference. That value changes the operator's Adobe everywhere, for every PDF they open, K-BOT
''' or not. Writing it silently on every DDF preview is unacceptable and prompting on every preview
''' is unusable, so the shipping code ADAPTS to whichever UI it finds — which is exactly what
''' <see cref="AdobeUiDetector"/> makes possible. The bench writes it because the bench is a bench.
''' </summary>
Public NotInheritable Class AdobeReaderHost
    Implements IDisposable

    Private ReadOnly _host As IHostSurface
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _watcher As AdobePopupWatcher
    Private ReadOnly _capture As AdobeWindowCapture
    Private ReadOnly _teardown As AdobeWindowTeardown
    Private ReadOnly _launcher As IAdobeLauncher
    Private ReadOnly _hook As AdobeCreationHook

    ' Every process id THIS host started. A PID outside this set is never killed — not here, not in
    ' the bench. With «/n» off the embedded window can belong to the operator's own Adobe.
    Private ReadOnly _launchedPids As New HashSet(Of Integer)()

    Private _hostedWindow As IntPtr = IntPtr.Zero
    Private _hostedPid As Integer = 0
    Private _startedPid As Integer = 0
    ' A newer ShowDocument invalidates an in-flight one.
    Private _generation As Integer = 0
    ' Cached across documents, so the SECOND document already launches with the right flags.
    Private _lastGeneration As AdobeUiGeneration = AdobeUiGeneration.Unknown
    Private _relaunchedForProfile As Boolean = False

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
        _watcher = New AdobePopupWatcher(AddressOf Report)
        _hook = New AdobeCreationHook(AddressOf Report)
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

    ''' <summary>The profile in force right now (Nothing before the first document).</summary>
    Public ReadOnly Property CurrentChoice As AdobeProfileChoice

    ''' <summary>The last detection, or Nothing if nothing has been probed yet.</summary>
    Public ReadOnly Property LastDetection As AdobeUiDetection

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
            _relaunchedForProfile = False

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

        Dim args As String = AdobeWindowHosting.BuildArguments(launchProfile, pdfPath, _options.ExtraArgs)
        Report($"Pornesc Adobe: {Path.GetFileName(adobePath)} {args}")
        Report("  " & launchProfile.Describe())
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
        ' is labelled «ours» or «foreign». Gating the search on «/n» — as this code briefly did — is
        ' wrong: the operator's log shows Adobe hands the document to a running instance on EVERY
        ' launch, «/n» included, so a PID-strict search finds nothing and leaves the real window
        ' floating on screen with a taskbar button.
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
        Report($"Fereastră găsită în {caught.ElapsedMs} ms " &
               $"({If(caught.Match = AdobeCaptureMatch.ByPid, "după PID", "după titlu")}), PID {_hostedPid}.")

        ' §1: the modern profile launches WITHOUT «/n», so Adobe may hand the document to an instance
        ' the operator already had open. Say so loudly — the window we are about to reparent then
        ' belongs to a process K-BOT did not create, and closing it later would close THEIR work.
        If caught.Match = AdobeCaptureMatch.ByTitle Then
            Report($"ATENȚIE: fereastra încorporată (PID {_hostedPid}) NU a fost creată de K-BOT " &
                   $"(am pornit PID {_startedPid}). Adobe a predat documentul unei instanțe existente — " &
                   "procesul acela NU va fi închis de K-BOT. Setează «Instanță nouă Adobe» pe «Da» " &
                   "dacă vrei o instanță separată.")
        End If

        ' The window is hidden at this point (AdobeWindowCapture hid it on sight). Everything from
        ' here to Reveal happens off screen.
        _capture.AttachAsChild(_hostedWindow, _host.Handle)

        ' Detection happens on the window tree we now own, never on the registry.
        Dim nodes As List(Of AdobeWindowNode) = AdobeWindowProbe.Walk(_hostedWindow, _host.Handle)
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
        ' Only now does the window become visible — placed, sized and inside the panel.
        _capture.Reveal(_hostedWindow)

        If PopupWatchEnabled AndAlso profile.HidePopups Then
            Dim pids As New List(Of Integer)()
            If _hostedPid <> 0 Then pids.Add(_hostedPid)
            If _startedPid <> 0 AndAlso Not pids.Contains(_startedPid) Then pids.Add(_startedPid)
            _watcher.Start(_host.Handle, pids)
            _watcher.Sweep()
        End If

        ' Adobe finishes its layout after the window appears; without this second pass the reparented
        ' window can stay blank.
        Await Task.Delay(_options.RedrawDelayMs).ConfigureAwait(True)
        If gen <> _generation Then Return New AdobeHostResult(AdobeHostStatus.Superseded, "", Nothing)
        ApplyGeometry(profile, "Poziție (a doua trecere)")

        Dim note As String = ""
        If detection.Generation = AdobeUiGeneration.Unknown Then note = AdobeUiDetector.UnrecognisedNote
        Return New AdobeHostResult(AdobeHostStatus.Hosted, note, choice, caught.ElapsedMs, caught.Match)
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
        Dim wanted As Rectangle = AdobeHostGeometry.Compute(_host.ClientSize, profile)
        If wanted.Width <= 0 OrElse wanted.Height <= 0 Then Return

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

    ''' <summary>
    ''' Lets the hosted window go, in the mode <see cref="Options"/> selects.
    '''
    ''' THE WINDOW IS NEVER HANDED BACK. Restoring its style and re-parenting it to the desktop is
    ''' what left a stray Adobe window — with a taskbar button, showing the previous document —
    ''' behind every document change before pass 03. See <see cref="AdobeWindowTeardown"/>.
    ''' </summary>
    Public Sub Detach()
        Try
            _watcher.Stop()
            _hook.Remove()

            Dim hwnd As IntPtr = _hostedWindow
            Dim pid As Integer = _hostedPid
            ' Cleared FIRST, so a teardown that fails cannot leave a stale handle behind to be
            ' re-used against a window that no longer exists.
            _hostedWindow = IntPtr.Zero
            _hostedPid = 0
            _startedPid = 0
            _CurrentChoice = Nothing

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

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            Detach()
            _watcher.Dispose()
            _hook.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReaderHost.Dispose", ex)
        End Try
    End Sub

End Class
