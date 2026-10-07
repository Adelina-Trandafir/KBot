Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common

' The Save As trap (slice 0078-15, operator 07.10.2026), ported unchanged in what it does from the old ActiveX viewer
' (AcroPdfSurface, removed 07.10.2026): the same AdobeSaveTrap the hosted window uses. Signing in Adobe forces a Save As; the trap moves the
' dialog off screen, writes the document's own path into it, reads it back, presses Save and answers «replace?» --
' any doubt cancels and raises SaveTrapFailed. It also answers Adobe's «save changes before closing?» with «No» when
' K-BOT closes or replaces the document, and closes the script alerts that match the operator's trapped-alert list
' (Setări > «Mesaje de script Adobe»); the others stay on screen.
'
' With AcroPDF nobody hands us Adobe's process id: the trap asks OwnerPids on every sweep. Measured 23.09.2026: the
' control is served by a BROKER that K-BOT's own process starts (it owns the Save As dialog) and its RENDERER child (it
' owns the AVL_* windows inside the control); the family is the renderers seen in the control, their parents, every
' Adobe process K-BOT started, and every child of those.
'
' Off screen (panel or a parent hidden, form minimised) the trap is paused, and resumed when the viewer is back.
Partial Public NotInheritable Class AcroPdfViewer

    ' The alerts block K-BOT's thread while they last, so the wait after them is shorter than AdobeSaveTrap's default.
    Private Const ActiveXBurstQuietMs As Integer = 1200
    Private Const ActiveXBurstIntervalMs As Integer = 50
    Private Const AdobeViewClassPrefix As String = "AVL_"
    ' After a close / replace: how long at least the «save changes?» answer is waited for.
    Private Const ClosePromptMinMs As Integer = 300

    Private _saveTrap As AdobeSaveTrap
    Private _saveTrapEnabled As Boolean
    ' The form whose minimise also takes the viewer off screen (found lazily: at construction the panel may not be on a
    ' form yet).
    Private _screenForm As Form

    ''' <summary>Raised on the UI thread after a trapped save. Argument = the document path.</summary>
    Public Event DocumentSaved As Action(Of String)
    ''' <summary>Raised on the UI thread when a save had to be cancelled. Argument = Romanian reason.</summary>
    Public Event SaveTrapFailed As Action(Of String)

    ' Called by the constructor.
    Private Sub InitSaveTrap()
        _saveTrap = New AdobeSaveTrap(AddressOf Report) With {
            .PidSource = AddressOf OwnerPids, .IsOnScreen = AddressOf IsOnScreen, .Traced = True,
            .BurstQuietMs = ActiveXBurstQuietMs, .BurstIntervalMs = ActiveXBurstIntervalMs}
        AddHandler _saveTrap.Saved, AddressOf OnTrapSaved
        AddHandler _saveTrap.Failed, AddressOf OnTrapFailed
        AddHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
        AddHandler _panel.VisibleChanged, AddressOf OnScreenStateChanged
        AddHandler _panel.ParentChanged, AddressOf OnScreenStateChanged
    End Sub

    ' Called by Dispose (wrapped there).
    Private Sub DisposeSaveTrap()
        If _saveTrap Is Nothing Then Return
        RemoveHandler _saveTrap.Saved, AddressOf OnTrapSaved
        RemoveHandler _saveTrap.Failed, AddressOf OnTrapFailed
        RemoveHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
        RemoveHandler _panel.VisibleChanged, AddressOf OnScreenStateChanged
        RemoveHandler _panel.ParentChanged, AddressOf OnScreenStateChanged
        If _screenForm IsNot Nothing Then RemoveHandler _screenForm.Resize, AddressOf OnScreenStateChanged
        _screenForm = Nothing
        _saveTrap.Dispose()
    End Sub

    ''' <summary>
    ''' True = Adobe's Save As (forced by signing) is taken over for the document on screen; see the file notes. Takes
    ''' effect on the document already loaded.
    ''' </summary>
    Public Property SaveTrapEnabled As Boolean
        Get
            Return _saveTrapEnabled
        End Get
        Set(value As Boolean)
            ' Unchanged -> nothing (set before every load: it must not restart the trap on the document being replaced).
            If value = _saveTrapEnabled Then Return
            _saveTrapEnabled = value
            If value Then
                StartSaveTrap()
            Else
                _saveTrap.Stop()
            End If
        End Set
    End Property

    ' Before a load: a save of the previous document finishes first, and replacing it is closing it (Adobe may ask
    ' «save changes?»). Returns True when a document is being replaced. Reached from ShowDocument (wrapped).
    Private Function BeforeLoadSaveTrap() As Boolean
        If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
        Dim k_replacing As Boolean = _loadedPath IsNot Nothing
        If k_replacing Then _saveTrap.BeginClose()
        Return k_replacing
    End Function

    ' After a load: the close prompt of the replaced document answered, then the trap re-armed on the new one -- right
    ' after the load, not after the page is laid out: the operator may sign while Adobe still lays out, and the pids are
    ' re-read on every sweep. Reached from ShowDocument (wrapped).
    Private Sub AfterLoadSaveTrap(k_replacing As Boolean)
        If k_replacing Then PumpClosePrompt()
        _saveTrap.Stop()
        StartSaveTrap()
    End Sub

    ' Before the control is destroyed: a pressed Save still writing the file must not lose its control halfway; the trap
    ' stays on while the control goes, so «save changes before closing?» is answered. Returns True when a document was
    ' open. Reached from Clear (wrapped).
    Private Function BeforeClearSaveTrap() As Boolean
        If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
        Dim k_had As Boolean = _loadedPath IsNot Nothing
        If k_had Then _saveTrap.BeginClose()
        Return k_had
    End Function

    ' After the control is destroyed. Reached from Clear (wrapped).
    Private Sub AfterClearSaveTrap(k_had As Boolean)
        If k_had Then PumpClosePrompt()
        _saveTrap.Stop()
    End Sub

    ' Lets the trap answer «save changes before closing?» (and the Save As that may follow): at least ClosePromptMinMs,
    ' longer while a save runs, never past AdobeWindowTeardown.MaxExtendedWaitMs. Blocking on purpose: the document
    ' being replaced is gone only when it is done.
    Private Sub PumpClosePrompt()
        Dim k_clock As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
        Do
            Dim k_going As Boolean = _saveTrap.ClosePoll()
            If Not k_going AndAlso k_clock.ElapsedMilliseconds >= ClosePromptMinMs Then Exit Do
            If k_clock.ElapsedMilliseconds >= AdobeWindowTeardown.MaxExtendedWaitMs Then Exit Do
            Threading.Thread.Sleep(50)
        Loop
    End Sub

    ' Wrapped: called from the setter and after a load.
    Private Sub StartSaveTrap()
        Try
            If Not _saveTrapEnabled OrElse _host Is Nothing OrElse String.IsNullOrEmpty(_loadedPath) Then Return
            AttachScreenForm()
            If ViewerShown() Then _saveTrap.Resume() Else _saveTrap.Pause()
            _saveTrap.Start(_loadedPath, OwnerPids())
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.StartSaveTrap", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The processes whose Save As belongs to our document: the renderers that own Adobe's views inside this control,
    ''' their parents, every Adobe process K-BOT started, and every child of those. Empty before Adobe has built its
    ''' windows. Asked by the trap on every sweep.
    ''' </summary>
    Public Function OwnerPids() As IEnumerable(Of Integer)
        Dim k_pids As New List(Of Integer)()
        Try
            If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return k_pids
            Dim k_renderers As New HashSet(Of Integer)()
            For Each k_h As IntPtr In AdobeNativeMethods.Descendants(_host.Handle)
                If Not AdobeNativeMethods.GetClass(k_h).StartsWith(AdobeViewClassPrefix, StringComparison.Ordinal) Then Continue For
                Dim k_pid As Integer = AdobeNativeMethods.OwnerPid(k_h)
                If k_pid > 0 Then k_renderers.Add(k_pid)
            Next
            k_pids.AddRange(k_renderers)

            ' The rest of the family, among the Adobe processes only (name + parent).
            Dim k_self As Integer = Environment.ProcessId
            Dim k_parents As New Dictionary(Of Integer, Integer)()
            For Each k_name As String In AdobeProcessNames
                For Each k_proc As Diagnostics.Process In Diagnostics.Process.GetProcessesByName(k_name)
                    Using k_proc
                        k_parents(k_proc.Id) = AdobeNativeMethods.ParentPid(k_proc.Id)
                    End Using
                Next
            Next
            ' Slice 0078-11: the family K-BOT started is remembered for the closing of the application.
            AdobeProcessRegistry.TrackFamily(k_parents)
            Dim k_brokers As New HashSet(Of Integer)()
            For Each k_pair As KeyValuePair(Of Integer, Integer) In k_parents
                If k_pair.Value = k_self Then k_brokers.Add(k_pair.Key)                       ' started by K-BOT
                If k_renderers.Contains(k_pair.Key) AndAlso k_parents.ContainsKey(k_pair.Value) Then k_brokers.Add(k_pair.Value)
            Next
            For Each k_pair As KeyValuePair(Of Integer, Integer) In k_parents
                If k_brokers.Contains(k_pair.Key) OrElse k_brokers.Contains(k_pair.Value) Then
                    If Not k_pids.Contains(k_pair.Key) Then k_pids.Add(k_pair.Key)
                End If
            Next
        Catch ex As Exception
            ' Called from the trap's timer: a failed walk only means «no pids this time».
            GlobalErrorLog.Write("AcroPdfViewer.OwnerPids", ex)
        End Try
        Return k_pids
    End Function

    ''' <summary>
    ''' True when this viewer is on screen. Two viewers (DDF and ORD) share one Acrobat, so a Save As whose offered name
    ''' matches neither document goes to the one the operator is looking at.
    ''' </summary>
    Public Function IsOnScreen() As Boolean
        If _host Is Nothing OrElse Not _host.IsHandleCreated OrElse Not ViewerShown() Then Return False
        Return AdobeNativeMethods.IsWindowVisible(_host.Handle)
    End Function

    Private Function ViewerShown() As Boolean
        If Not _panel.Visible Then Return False
        Return _screenForm Is Nothing OrElse _screenForm.WindowState <> FormWindowState.Minimized
    End Function

    Private Sub AttachScreenForm()
        Dim k_form As Form = _panel.FindForm()
        If k_form Is _screenForm Then Return
        If _screenForm IsNot Nothing Then RemoveHandler _screenForm.Resize, AddressOf OnScreenStateChanged
        _screenForm = k_form
        If _screenForm IsNot Nothing Then AddHandler _screenForm.Resize, AddressOf OnScreenStateChanged
    End Sub

    ' Panel shown / hidden / moved, form minimised / restored: the trap pauses or resumes. UI boundary: log and swallow.
    Private Sub OnScreenStateChanged(sender As Object, e As EventArgs)
        Try
            AttachScreenForm()
            If ViewerShown() Then
                If _saveTrap.IsPaused Then _saveTrap.Resume()
            Else
                _saveTrap.Pause()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnScreenStateChanged", ex)
        End Try
    End Sub

    ' Trap events arrive on the UI thread (hook + WinForms timer); forwarded as they are.
    Private Sub OnTrapSaved(k_path As String)
        Try
            Check($"+{Elapsed()} ms >>>>> SAVE TRAP: saved «{k_path}» <<<<<")             ' ACTIVEX-CHECK
            RaiseEvent DocumentSaved(k_path)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnTrapSaved", ex)
        End Try
    End Sub

    Private Sub OnTrapFailed(k_reason As String)
        Try
            Check($"+{Elapsed()} ms >>>>> SAVE TRAP: save cancelled: {k_reason} <<<<<")   ' ACTIVEX-CHECK
            RaiseEvent SaveTrapFailed(k_reason)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnTrapFailed", ex)
        End Try
    End Sub

    ' A burst of script alerts is over: Read Mode, which waits for it, gets another chance (Adobe may raise no window
    ' event after the last alert). UI boundary: log and swallow.
    Private Sub OnScriptBurstEnded()
        Try
            ReadModeOnEvent()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnScriptBurstEnded", ex)
        End Try
    End Sub

End Class
