Imports System.Globalization
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The side panel of the main window, beside every page: recent machete, recent actions, session
''' information and the errors of this run. Nothing polls: the robot's state arrives through the
''' controller's events, the errors through <c>GlobalErrorLog.Written</c>, the machete through the
''' history's <c>Changed</c> (all three may come from another thread), and the E-Factura token is asked
''' for each time the panel is refreshed.
''' </summary>
Partial Public Class AvacontMain

    Private Const ErrorsShown As Integer = 10

    Private ReadOnly _session As SessionContext
    Private ReadOnly _controller As ForexeController
    Private ReadOnly _tokenState As Func(Of Task(Of EFacturaTokenState))
    Private _tokenSeq As Integer
    Private _tokenTone As Tone = Tone.Dim
    Private _forexeTone As Tone = Tone.Dim
    Private _errorsPending As Boolean
    Private _fillingErrors As Boolean
    Private _actionsSeq As Integer
    Private WithEvents _actionsTimer As New System.Windows.Forms.Timer() With {.Interval = 2000}
    Private WithEvents _midnightTimer As New System.Windows.Forms.Timer()   ' one shot per day: the list of recent actions starts over (slice 0115-01)
    Private Const ActionsShown As Integer = 10

    ' The seven information rows, created once and updated in place.
    Private ReadOnly _rowOperator As New KBotSidePanelRow() With {.DetailText = "Operator"}
    Private ReadOnly _rowRole As New KBotSidePanelRow() With {.DetailText = "Rol"}
    Private ReadOnly _rowPeriod As New KBotSidePanelRow() With {.DetailText = "Baza și perioada"}
    Private ReadOnly _rowForexe As New KBotSidePanelRow() With {.DetailText = "FOREXE"}
    Private ReadOnly _rowActivity As New KBotSidePanelRow() With {.DetailText = "Activitate robot"}
    Private ReadOnly _rowToken As New KBotSidePanelRow() With {.DetailText = "Token E-Factura"}
    Private ReadOnly _rowVersion As New KBotSidePanelRow() With {.DetailText = "Versiunea K-BOT"}

    Private Enum Tone
        [Dim]
        Good
        Warn
        Bad
    End Enum

    ' Called once by the constructor, after InitializeComponent.
    Private Sub InitSidebar()
        Dim k_info As KBotSidePanelSection = sidebar.FindSection("info")
        For Each k_row As KBotSidePanelRow In New KBotSidePanelRow() {_rowOperator, _rowRole, _rowPeriod, _rowForexe, _rowActivity, _rowToken, _rowVersion}
            k_row.Text = "—"
            k_info.Rows.Add(k_row)
        Next
        sidebar.Collapsed = AppSettings.Current.HomeSidebarCollapsed
        AddHandler _controller.StateChanged, AddressOf Controller_Changed
        AddHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
        AddHandler GlobalErrorLog.Written, AddressOf ErrorWritten
        AddHandler _registry.History.Changed, AddressOf History_Changed
        AddHandler ServerGateHandler.WriteSucceeded, AddressOf Server_WriteSucceeded
        ArmMidnightTimer()
    End Sub

    ' The singletons outlive this window: a subscription left behind would keep it alive.
    Private Sub TearDownSidebar()
        RemoveHandler _controller.StateChanged, AddressOf Controller_Changed
        RemoveHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
        RemoveHandler GlobalErrorLog.Written, AddressOf ErrorWritten
        RemoveHandler _registry.History.Changed, AddressOf History_Changed
        RemoveHandler ServerGateHandler.WriteSucceeded, AddressOf Server_WriteSucceeded
        _actionsTimer.Stop()
        _midnightTimer.Stop()
        DisposeSnapshots()
    End Sub

    ''' <summary>Reads everything the panel shows again.</summary>
    Friend Sub RefreshSidebar()
        Try
            _rowOperator.Text = If(String.IsNullOrEmpty(_session.OperatorName), "—", _session.OperatorName)
            _rowRole.Text = If(String.IsNullOrEmpty(_session.Role), "—", _session.Role)
            _rowPeriod.Text = DescribePeriod()
            _rowVersion.Text = CurrentVersionText()
            ApplyForexe()
            FillErrors()
            FillMachete()
            sidebar.InvalidateLayout()
            LoadTokenAsync()
            LoadActionsAsync()
        Catch ex As Exception
            ' UI boundary (called when a page is shown): log and swallow.
            GlobalErrorLog.Write("AvacontMain.RefreshSidebar", ex)
        End Try
    End Sub

    Private Function DescribePeriod() As String
        If String.IsNullOrEmpty(_session.DbName) Then Return "—"
        Dim k_text As String = _session.DbName
        If _session.An > 0 Then k_text &= " · " & _session.An.ToString(CultureInfo.InvariantCulture)
        If Not String.IsNullOrEmpty(_session.SectorSursa) Then k_text &= " / " & _session.SectorSursa
        Return k_text
    End Function

    Private Shared Function CurrentVersionText() As String
        Try
            Return AppUpdateService.CurrentVersion.ToString()
        Catch ex As Exception
            ' Already logged by the service.
            Return "necunoscută"
        End Try
    End Function

    ' ---- Events, possibly from another thread -----------------------------------------------------

    Private Sub PostToUi(k_action As Action)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(k_action)
            Else
                k_action()
            End If
        Catch ex As Exception
            ' UI boundary (event from another thread): log and swallow.
            GlobalErrorLog.Write("AvacontMain.PostToUi", ex)
        End Try
    End Sub

    Private Sub Controller_Changed(sender As Object, e As EventArgs)
        PostToUi(AddressOf ApplyForexe)
    End Sub

    Private Sub Controller_StatusChanged(sender As Object, e As String)
        PostToUi(AddressOf ApplyForexe)
    End Sub

    Private Sub History_Changed(sender As Object, e As EventArgs)
        PostToUi(AddressOf FillMachete)
    End Sub

    ' ---- FOREXE --------------------------------------------------------------------------------

    Private Sub ApplyForexe()
        Try
            If IsDisposed Then Return
            Dim k_connected As Boolean = _controller.IsConnected
            Dim k_certificate As String = _controller.CertificateName
            _rowForexe.Text = If(k_connected,
                                 "Conectat" & If(String.IsNullOrEmpty(k_certificate), String.Empty, " · " & k_certificate),
                                 "Neconectat")
            _forexeTone = If(k_connected, Tone.Good, Tone.Warn)
            _rowForexe.ForeColor = ToneColor(_forexeTone)
            _rowActivity.Text = If(_controller.IsBusy,
                                   If(String.IsNullOrWhiteSpace(_controller.LastStatus), "În lucru…", _controller.LastStatus),
                                   "În așteptare")
            sidebar.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.ApplyForexe", ex)
        End Try
    End Sub

    ' ---- E-Factura token --------------------------------------------------------------------------

    ' UI boundary (async Sub, started without await): logs and shows a plain line.
    Private Async Sub LoadTokenAsync()
        Dim k_seq As Integer = Interlocked.Increment(_tokenSeq)
        Try
            ShowToken("Se verifică…", Tone.Dim)
            If Not _session.IsAuthenticated OrElse String.IsNullOrWhiteSpace(_session.DbName) Then
                ShowToken("—", Tone.Dim)
                Return
            End If
            Dim k_state As EFacturaTokenState = Await _tokenState().ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _tokenSeq Then Return
            If k_state Is Nothing Then
                ShowToken("—", Tone.Dim)
            ElseIf Not k_state.Configured Then
                ShowToken("Neconfigurat pe server", Tone.Dim)
            ElseIf Not k_state.Exists Then
                ShowToken("Nu există token — autorizați din E-Factura", Tone.Warn)
            ElseIf k_state.MustRenew Then
                ShowToken("Expirat — necesită reînnoire", Tone.Bad)
            ElseIf k_state.ShouldWarn Then
                ShowToken("Valid încă " & DaysText(k_state) & " — reînnoiți curând", Tone.Warn)
            Else
                ShowToken("Valid până la " & UntilText(k_state) & " (" & DaysText(k_state) & ")", Tone.Good)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.LoadTokenAsync", ex)
            If Not IsDisposed AndAlso k_seq = _tokenSeq Then ShowToken("Nu s-a putut verifica", Tone.Dim)
        End Try
    End Sub

    Private Shared Function DaysText(k_state As EFacturaTokenState) As String
        If Not k_state.DaysLeft.HasValue Then Return "—"
        Dim k_days As Integer = k_state.DaysLeft.Value
        Return If(k_days = 1, "1 zi", k_days.ToString(CultureInfo.InvariantCulture) & " zile")
    End Function

    Private Shared Function UntilText(k_state As EFacturaTokenState) As String
        If Not k_state.ValidUntilUtc.HasValue Then Return "—"
        Return k_state.ValidUntilUtc.Value.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
    End Function

    Private Sub ShowToken(k_text As String, k_tone As Tone)
        _tokenTone = k_tone
        _rowToken.Text = k_text
        _rowToken.ForeColor = ToneColor(k_tone)
        sidebar.InvalidateLayout()
    End Sub

    Private Shared Function ToneColor(k_tone As Tone) As Color
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Select Case k_tone
            Case Tone.Good : Return k_palette.SuccessColor
            Case Tone.Warn : Return k_palette.WarningColor
            Case Tone.Bad : Return k_palette.ErrorColor
            Case Else : Return Color.Empty   ' the panel's own text colour
        End Select
    End Function

    ' The tones are colours of the theme: they follow it.
    Private Sub ApplySidebarTones()
        _rowForexe.ForeColor = ToneColor(_forexeTone)
        _rowToken.ForeColor = ToneColor(_tokenTone)
        sidebar.InvalidateLayout()
    End Sub

    ' ---- Errors of this run -----------------------------------------------------------------------

    ' May run on any thread, and while an error is being written: one refresh is posted, however many arrive.
    Private Sub ErrorWritten(sender As Object, e As SessionErrorEntry)
        Try
            If _fillingErrors OrElse IsDisposed OrElse Not IsHandleCreated OrElse _errorsPending Then Return
            _errorsPending = True
            BeginInvoke(New Action(AddressOf FillErrors))
        Catch ex As Exception
            ' Terminal path: this runs inside GlobalErrorLog.Write; it must not write again.
            Trace.WriteLine("AvacontMain.ErrorWritten: " & ex.Message)
        End Try
    End Sub

    Private Sub FillErrors()
        _errorsPending = False
        If _fillingErrors OrElse IsDisposed Then Return
        _fillingErrors = True
        Try
            Dim k_section As KBotSidePanelSection = sidebar.FindSection("erori")
            k_section.Rows.Clear()
            For Each k_entry As SessionErrorEntry In GlobalErrorLog.RecentErrors(ErrorsShown)
                k_section.Rows.Add(New KBotSidePanelRow(FirstLine(k_entry.Message), k_entry.Moment.ToString("HH:mm:ss", CultureInfo.InvariantCulture), "eroare") With {
                    .Tag = k_entry,
                    .ToolTipText = k_entry.Source & vbLf & FirstLine(k_entry.Message) & vbLf & "Clic pentru detalii."})
            Next
        Catch ex As Exception
            ' Not written to the log here: that would raise Written again and refill this very list.
            Trace.WriteLine("AvacontMain.FillErrors: " & ex.Message)
        Finally
            _fillingErrors = False
        End Try
    End Sub

    Private Shared Function FirstLine(k_text As String) As String
        If String.IsNullOrEmpty(k_text) Then Return "(fără mesaj)"
        Dim k_cut As Integer = k_text.IndexOfAny(New Char() {ControlChars.Cr, ControlChars.Lf})
        Return If(k_cut > 0, k_text.Substring(0, k_cut), k_text)
    End Function

    ' ---- Recent actions (the data writes the server recorded for this operator) ---------------------

    ' A write succeeded (any thread): the list is read again once, 2 s after the LAST write of a burst -- one small GET
    ' however many writes a save makes.
    Private Sub Server_WriteSucceeded(sender As Object, e As EventArgs)
        PostToUi(Sub()
                     _actionsTimer.Stop()
                     _actionsTimer.Start()
                 End Sub)
    End Sub

    Private Sub ActionsTimer_Tick(sender As Object, e As EventArgs) Handles _actionsTimer.Tick
        _actionsTimer.Stop()
        LoadActionsAsync()
    End Sub

    ' The list shows only today: a few seconds after the next midnight it is read again (and comes back empty) and the timer is set for the following one.
    Private Sub ArmMidnightTimer()
        _midnightTimer.Stop()
        _midnightTimer.Interval = CInt(Math.Min(Integer.MaxValue, (Date.Today.AddDays(1) - Date.Now).TotalMilliseconds + 5000.0))
        _midnightTimer.Start()
    End Sub

    Private Sub MidnightTimer_Tick(sender As Object, e As EventArgs) Handles _midnightTimer.Tick
        Try
            ArmMidnightTimer()
            LoadActionsAsync()
        Catch ex As Exception
            ' UI boundary (timer): log and swallow.
            GlobalErrorLog.Write("AvacontMain.MidnightTimer_Tick", ex)
        End Try
    End Sub

    ' UI boundary (async Sub, started without await): logs and leaves the list as it was.
    Private Async Sub LoadActionsAsync()
        Dim k_seq As Integer = Interlocked.Increment(_actionsSeq)
        Try
            Dim k_actions As IReadOnlyList(Of RecentAction) = Await _kbot.GetRecentActionsAsync(ActionsShown).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _actionsSeq Then Return
            Dim k_section As KBotSidePanelSection = sidebar.FindSection("actiuni")
            k_section.Rows.Clear()
            For Each k_action As RecentAction In k_actions
                ' Slice 0115-01: only today's actions; yesterday's and older are not shown (the list starts over at midnight).
                If k_action.Moment.Date <> Date.Today Then Continue For
                Dim k_when As String = k_action.Moment.ToString("HH:mm", CultureInfo.InvariantCulture)
                k_section.Rows.Add(New KBotSidePanelRow(k_action.Text, k_when, "actiune") With {
                    .ToolTipText = k_action.Text & vbLf & k_action.Moment.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)})
            Next
            sidebar.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.LoadActionsAsync", ex)
        End Try
    End Sub

    ' ---- Recent machete (the history of the registry) ---------------------------------------------

    Private Sub FillMachete()
        Try
            If IsDisposed Then Return
            Dim k_section As KBotSidePanelSection = sidebar.FindSection("machete")
            k_section.Rows.Clear()
            ' Slice 0113-01: the open machete first, in the order they were opened; the closed ones after them, in italics.
            ' Slice 0115-01: over an OPEN macheta the tooltip shows a picture of it as it was left; over a closed one there is no tooltip.
            For Each k_entry As MachetaEntry In _registry.History.Recent()
                Dim k_this As MachetaEntry = k_entry
                Dim k_row As New KBotSidePanelRow(k_entry.Title, String.Empty, "macheta") With {
                    .Tag = k_entry,
                    .Italic = Not k_entry.IsOpen}
                If k_entry.IsOpen Then
                    k_row.ToolTipHeader = k_entry.Title
                    k_row.ToolTipImageProvider = Function() PreviewFor(k_this)
                End If
                k_section.Rows.Add(k_row)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.FillMachete", ex)
        End Try
    End Sub

    ' ---- Pictures of the open machete (the tooltip of «Machete deschise») ---------------------------
    ' A picture is taken of the macheta that is on screen when something else takes its place, and of the one on screen when
    ' the mouse is over its row; a macheta put away shows the picture taken then. Small (see SnapshotMaxSize), kept by
    ' MachetaRef.Id, thrown away when the macheta closes. Not live: it is a still picture.

    Private ReadOnly _snapshots As New Dictionary(Of String, Bitmap)(StringComparer.Ordinal)
    Private Shared ReadOnly SnapshotMaxSize As New Size(640, 420)

    ' The picture of the open macheta of this entry, or Nothing. UI thread.
    Private Function PreviewFor(k_entry As MachetaEntry) As Image
        Try
            If k_entry Is Nothing OrElse Not k_entry.IsOpen Then Return Nothing
            Dim k_front As Control = FrontMacheta(k_entry.Id)
            If k_front IsNot Nothing Then StoreSnapshot(k_entry.Id, k_front)   ' on screen now: take it as it is
            Dim k_bitmap As Bitmap = Nothing
            Return If(_snapshots.TryGetValue(k_entry.Id, k_bitmap), k_bitmap, Nothing)
        Catch ex As Exception
            ' UI boundary (tooltip): log and swallow; the tooltip shows without a picture.
            GlobalErrorLog.Write("AvacontMain.PreviewFor", ex)
            Return Nothing
        End Try
    End Function

    ' The control on screen when it is the macheta with this id (a page, or the shell); Nothing otherwise.
    Private Function FrontMacheta(k_id As String) As Control
        If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
            Dim k_ref As MachetaRef = Nothing
            Return If(_pageRefs.TryGetValue(_page, k_ref) AndAlso String.Equals(k_ref.Id, k_id, StringComparison.Ordinal), _page, Nothing)
        End If
        Return If(_baseIsShell AndAlso String.Equals(ShellRef.Id, k_id, StringComparison.Ordinal), _kbot, Nothing)
    End Function

    ''' <summary>The macheta on screen is about to be put away: its picture is taken now, as the operator leaves it.</summary>
    Private Sub CaptureFront()
        Try
            If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
                Dim k_ref As MachetaRef = Nothing
                If _pageRefs.TryGetValue(_page, k_ref) Then StoreSnapshot(k_ref.Id, _page)
            ElseIf _baseIsShell Then
                StoreSnapshot(ShellRef.Id, _kbot)
            End If
        Catch ex As Exception
            ' UI boundary: a missing picture must not stop the operator from moving on.
            GlobalErrorLog.Write("AvacontMain.CaptureFront", ex)
        End Try
    End Sub

    Private Sub StoreSnapshot(k_id As String, k_control As Control)
        Dim k_new As Bitmap = TakeSnapshot(k_control)
        If k_new Is Nothing Then Return
        DropSnapshot(k_id)
        _snapshots(k_id) = k_new
    End Sub

    ' The control drawn into a bitmap, then reduced to at most SnapshotMaxSize. Nothing when it cannot be drawn now.
    Private Shared Function TakeSnapshot(k_control As Control) As Bitmap
        If k_control Is Nothing OrElse k_control.IsDisposed OrElse Not k_control.IsHandleCreated OrElse Not k_control.Visible Then Return Nothing
        If k_control.Width < 8 OrElse k_control.Height < 8 Then Return Nothing
        Using k_full As New Bitmap(k_control.Width, k_control.Height)
            k_control.DrawToBitmap(k_full, New Rectangle(Point.Empty, k_control.Size))
            Dim k_scale As Double = Math.Min(1.0, Math.Min(SnapshotMaxSize.Width / CDbl(k_full.Width), SnapshotMaxSize.Height / CDbl(k_full.Height)))
            Dim k_small As New Bitmap(Math.Max(1, CInt(Math.Round(k_full.Width * k_scale))), Math.Max(1, CInt(Math.Round(k_full.Height * k_scale))))
            Using k_g As Graphics = Graphics.FromImage(k_small)
                k_g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                k_g.DrawImage(k_full, New Rectangle(Point.Empty, k_small.Size))
            End Using
            Return k_small
        End Using
    End Function

    ''' <summary>The macheta closed (or is not a macheta any more): its picture goes.</summary>
    Private Sub DropSnapshot(k_id As String)
        Dim k_old As Bitmap = Nothing
        If _snapshots.TryGetValue(k_id, k_old) Then
            _snapshots.Remove(k_id)
            k_old.Dispose()
        End If
    End Sub

    Private Sub DisposeSnapshots()
        For Each k_bitmap As Bitmap In _snapshots.Values
            k_bitmap.Dispose()
        Next
        _snapshots.Clear()
    End Sub

    ' UI boundary (async Sub from a click): logs and tells the operator.
    Private Async Sub OpenMachetaAsync(k_entry As MachetaEntry)
        Try
            Await _registry.OpenAsync(k_entry.ToRef()).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.OpenMachetaAsync", ex)
            KBotMessage.Show(Me, "Macheta nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Machete deschise", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ---- Clicks ---------------------------------------------------------------------------------

    Private Sub Sidebar_RowClicked(sender As Object, e As KBotSidePanelRowClickedEventArgs) Handles sidebar.RowClicked
        Try
            Dim k_macheta As MachetaEntry = TryCast(e.Row.Tag, MachetaEntry)
            If k_macheta IsNot Nothing Then
                OpenMachetaAsync(k_macheta)
                Return
            End If
            Dim k_entry As SessionErrorEntry = TryCast(e.Row.Tag, SessionErrorEntry)
            If k_entry Is Nothing Then Return
            Dim k_detail As String = k_entry.Detail
            If k_detail.Length > 1500 Then k_detail = k_detail.Substring(0, 1500) & "…"
            KBotMessage.Show(Me, k_entry.Moment.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) & "  [" & k_entry.Source & "]" &
                             vbLf & vbLf & k_detail & vbLf & vbLf & "Textul întreg e în jurnalul de erori.",
                             "Eroare din această sesiune", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Sidebar_RowClicked", ex)
        End Try
    End Sub

    Private Sub Sidebar_CollapsedChanged(sender As Object, e As EventArgs) Handles sidebar.CollapsedChanged
        Try
            AppSettings.Current.HomeSidebarCollapsed = sidebar.Collapsed
            AppSettings.Current.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Sidebar_CollapsedChanged", ex)
        End Try
    End Sub
End Class
