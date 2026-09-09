Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming
Imports WorkflowModels

' =============================================================================
'  RecorderForm - the K-BOT recorder bench.
'
'  The browser is reparented INTO pnlBrowser (WorkflowExecutor.Docking.vb): while docked
'  the Chromium window is a child control of that panel, so it moves and clips with the
'  form and cannot fall behind it. It also dies with the panel, which is why FormClosing
'  undocks before anything else is disposed.
'
'  Every operator edit (a different candidate, a per step WaitFor, a deletion, a
'  reorder, a global option) regenerates the preview through RecorderCompactor and
'  WflWriter. Deleting is logical, so the raw trail keeps its numbering.
'
'  Controls are declared in RecorderForm.Designer.vb.
' =============================================================================
Public Class RecorderForm

    Private _executor As WorkflowExecutor = Nothing
    ' The Wicket monitor window. Its own FormClosing hides instead of closing, so it is
    ' created once and only ever disposed from here.
    Private _monitor As WicketMonitorForm = Nothing
    Private ReadOnly _steps As New List(Of RecordedStep)
    Private _updatingDetail As Boolean = False
    Private _closingDown As Boolean = False
    Private _themeHooked As Boolean = False
    Private _strikeFont As Font = Nothing

    ' =========================================================================
    '  Theme aware semantic colours
    ' =========================================================================
    Private ReadOnly Property ClrSters As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(120, 120, 120),
                      Color.FromArgb(150, 150, 150))
        End Get
    End Property

    Private ReadOnly Property ClrFragil As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(230, 170, 70),
                      Color.FromArgb(170, 95, 0))
        End Get
    End Property

    Private ReadOnly Property ClrEroare As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(240, 110, 110),
                      Color.FromArgb(180, 0, 0))
        End Get
    End Property

    ' =========================================================================
    '  Constructor / Load
    ' =========================================================================
    Public Sub New()
        InitializeComponent()
        KBotTheme.ApplyTheme(Me)
    End Sub

    Private Sub RecorderForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            KBotTheme.ApplyTheme(Me)
            _strikeFont = New Font(lvPasi.Font, FontStyle.Strikeout)
            AddHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
            _themeHooked = True
            ApplyListColors()
            UpdateButtons()
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.RecorderForm_Load", ex)
        End Try
    End Sub

    ' =========================================================================
    '  ListView theming - ThemeManager does not cover ListView, so the rows and
    '  the header are painted here from the active palette. No literal colours.
    ' =========================================================================
    Private Sub HandleThemeChanged(sender As Object, e As EventArgs)
        Try
            If Me.IsDisposed Then Return
            ApplyListColors()
            RefreshPreview()
            lvPasi.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.HandleThemeChanged", ex)
        End Try
    End Sub

    Private Sub ApplyListColors()
        Dim palette = ThemeManager.Current.Palette
        lvPasi.BackColor = palette.InputBackColor
        lvPasi.ForeColor = palette.InputTextColor
        For k As Integer = 0 To Math.Min(lvPasi.Items.Count, _steps.Count) - 1
            ApplyRowLook(lvPasi.Items(k), _steps(k))
        Next
    End Sub

    Private Sub LvPasi_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs) _
        Handles lvPasi.DrawColumnHeader
        Try
            Dim palette = ThemeManager.Current.Palette
            Using back As New SolidBrush(palette.ButtonBackColor)
                e.Graphics.FillRectangle(back, e.Bounds)
            End Using
            Using border As New Pen(palette.BorderColor)
                e.Graphics.DrawLine(border, e.Bounds.Right - 1, e.Bounds.Top,
                                    e.Bounds.Right - 1, e.Bounds.Bottom - 1)
                e.Graphics.DrawLine(border, e.Bounds.Left, e.Bounds.Bottom - 1,
                                    e.Bounds.Right - 1, e.Bounds.Bottom - 1)
            End Using
            TextRenderer.DrawText(e.Graphics, e.Header.Text, lvPasi.Font, e.Bounds,
                                  palette.ButtonTextColor,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.LeftAndRightPadding)
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.LvPasi_DrawColumnHeader", ex)
        End Try
    End Sub

    Private Sub LvPasi_DrawItem(sender As Object, e As DrawListViewItemEventArgs) Handles lvPasi.DrawItem
        ' Details view: every cell is painted by DrawSubItem.
        e.DrawDefault = False
    End Sub

    Private Sub LvPasi_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs) _
        Handles lvPasi.DrawSubItem
        Try
            Dim palette = ThemeManager.Current.Palette
            Dim selected As Boolean = e.Item.Selected
            Dim backColor As Color = If(selected, palette.AccentColor, lvPasi.BackColor)
            Dim foreColor As Color = If(selected, palette.AccentTextColor, e.Item.ForeColor)

            Using back As New SolidBrush(backColor)
                e.Graphics.FillRectangle(back, e.Bounds)
            End Using

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, foreColor,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.LeftAndRightPadding)
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.LvPasi_DrawSubItem", ex)
        End Try
    End Sub

    ' =========================================================================
    '  AttachExecutor / DetachExecutor
    ' =========================================================================
    Public Sub AttachExecutor(executor As WorkflowExecutor)
        DetachExecutor()
        _executor = executor
        If executor Is Nothing Then Return
        AddHandler executor.OnRecordedStep, AddressOf HandleRecordedStep
        ' The monitor window listens to the same executor - it must follow the swap.
        _monitor?.AttachExecutor(executor)
        UpdateButtons()
    End Sub

    Public Sub DetachExecutor()
        If _executor Is Nothing Then Return
        RemoveHandler _executor.OnRecordedStep, AddressOf HandleRecordedStep
        _monitor?.DetachExecutor()
        _executor = Nothing
        UpdateButtons()
    End Sub

    ' =========================================================================
    '  Step arrival - raised on the Playwright thread
    ' =========================================================================
    Private Sub HandleRecordedStep(recordedStep As RecordedStep)
        If Me.IsDisposed OrElse recordedStep Is Nothing Then Return
        If Me.InvokeRequired Then
            Me.Invoke(Sub() HandleRecordedStep(recordedStep))
            Return
        End If

        Try
            _steps.Add(recordedStep)
            lvPasi.Items.Add(BuildRow(recordedStep, _steps.Count - 1))
            lvPasi.EnsureVisible(lvPasi.Items.Count - 1)
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.HandleRecordedStep", ex)
        End Try
    End Sub

    Private Function BuildRow(s As RecordedStep, position As Integer) As ListViewItem
        Dim row As New ListViewItem((position + 1).ToString(Globalization.CultureInfo.InvariantCulture))
        row.SubItems.Add(DescribeKind(s))
        row.SubItems.Add(s.ChosenSelector)
        row.SubItems.Add(If(String.IsNullOrEmpty(s.Value), s.Text, s.Value))
        row.SubItems.Add(If(s.TriggeredAjax, "da", ""))
        ApplyRowLook(row, s)
        Return row
    End Function

    Private Sub ApplyRowLook(row As ListViewItem, s As RecordedStep)
        If s.Deleted Then
            row.ForeColor = ClrSters
            If _strikeFont IsNot Nothing Then row.Font = _strikeFont
            Return
        End If

        row.Font = lvPasi.Font
        Dim chosen As SelectorCandidate = CurrentCandidate(s)
        If chosen IsNot Nothing AndAlso chosen.Fragile Then
            row.ForeColor = ClrFragil
        Else
            row.ForeColor = lvPasi.ForeColor
        End If
    End Sub

    Private Shared Function DescribeKind(s As RecordedStep) As String
        If String.Equals(s.Widget, "none", StringComparison.Ordinal) Then Return s.Kind
        Return $"{s.Kind}/{s.Widget}"
    End Function

    Private Shared Function CurrentCandidate(s As RecordedStep) As SelectorCandidate
        If s.Candidates Is Nothing OrElse s.Candidates.Count = 0 Then Return Nothing
        Dim i As Integer = s.SelectedCandidateIndex
        If i < 0 OrElse i >= s.Candidates.Count Then i = 0
        Return s.Candidates(i)
    End Function

    ' =========================================================================
    '  Docking toolbar
    ' =========================================================================
    Private Async Sub BtnAndocheaza_Click(sender As Object, e As EventArgs) Handles btnAndocheaza.Click
        Try
            If _executor Is Nothing Then
                KBotMessage.Show("Nu există o sesiune de browser atașată.", MsgBoxStyle.Exclamation, "K-BOT Recorder")
                Return
            End If
            Await _executor.DockBrowserToAsync(pnlBrowser)
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnAndocheaza_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    Private Async Sub BtnDetaseaza_Click(sender As Object, e As EventArgs) Handles btnDetaseaza.Click
        Try
            If _executor Is Nothing Then Return
            Await _executor.UndockBrowserAsync()
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnDetaseaza_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    Private Async Sub BtnResincronizeaza_Click(sender As Object, e As EventArgs) Handles btnResincronizeaza.Click
        Try
            If _executor Is Nothing Then Return
            Await _executor.SyncDockedBoundsAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnResincronizeaza_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    ' =========================================================================
    '  Recording toolbar
    ' =========================================================================
    Private Async Sub BtnPornesteInreg_Click(sender As Object, e As EventArgs) Handles btnPornesteInreg.Click
        Try
            If _executor Is Nothing Then
                KBotMessage.Show("Nu există o sesiune de browser atașată.", MsgBoxStyle.Exclamation, "K-BOT Recorder")
                Return
            End If
            ' Recording a browser the operator cannot see is pointless.
            If Not _executor.IsDocked Then
                KBotMessage.Show("Andochează browserul înainte de a începe înregistrarea.",
                       MsgBoxStyle.Exclamation, "K-BOT Recorder")
                Return
            End If
            Await _executor.StartRecordingAsync()
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnPornesteInreg_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    Private Sub BtnOpresteInreg_Click(sender As Object, e As EventArgs) Handles btnOpresteInreg.Click
        Try
            _executor?.StopRecording()
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnOpresteInreg_Click", ex)
        End Try
    End Sub

    Private Sub BtnCurata_Click(sender As Object, e As EventArgs) Handles btnCurata.Click
        Try
            _steps.Clear()
            _executor?.ClearRecordedSteps()
            lvPasi.Items.Clear()
            pnlDetaliu.Visible = False
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnCurata_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the Wicket monitor beside the recorder. The monitor is fed by
    ''' OnWicketStateChange, so the executor side monitor has to be running first -
    ''' otherwise the window would sit there empty and look broken.
    ''' </summary>
    Private Async Sub BtnMonitor_Click(sender As Object, e As EventArgs) Handles btnMonitor.Click
        Try
            If _executor Is Nothing Then
                KBotMessage.Show("Nu există o sesiune de browser atașată.", MsgBoxStyle.Exclamation, "K-BOT Recorder")
                Return
            End If

            If Not _executor.WicketMonitoringActive Then
                Await _executor.StartWicketMonitoringAsync()
            End If

            If _monitor Is Nothing OrElse _monitor.IsDisposed Then
                _monitor = New WicketMonitorForm()
                _monitor.AttachExecutor(_executor)
            End If

            If _monitor.Visible Then
                If _monitor.WindowState = FormWindowState.Minimized Then
                    _monitor.WindowState = FormWindowState.Normal
                End If
                _monitor.Activate()
            Else
                _monitor.Show(Me)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnMonitor_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    ' =========================================================================
    '  Options
    ' =========================================================================
    Private Sub Optiuni_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkWaitForAutomat.CheckedChanged, chkBlocReset.CheckedChanged
        Try
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.Optiuni_CheckedChanged", ex)
        End Try
    End Sub

    ' =========================================================================
    '  Step list and detail panel
    ' =========================================================================
    Private Sub LvPasi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvPasi.SelectedIndexChanged
        Try
            LoadDetail()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.LvPasi_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Function SelectedIndex() As Integer
        If lvPasi.SelectedIndices.Count = 0 Then Return -1
        Return lvPasi.SelectedIndices(0)
    End Function

    Private Function SelectedStep() As RecordedStep
        Dim i As Integer = SelectedIndex()
        If i < 0 OrElse i >= _steps.Count Then Return Nothing
        Return _steps(i)
    End Function

    Private Sub LoadDetail()
        Dim s As RecordedStep = SelectedStep()
        If s Is Nothing Then
            pnlDetaliu.Visible = False
            Return
        End If

        _updatingDetail = True
        Try
            cmbCandidati.Items.Clear()
            If s.Candidates IsNot Nothing Then
                For Each c As SelectorCandidate In s.Candidates
                    Dim prefix As String = If(c.Fragile, "⚠ ", "")
                    cmbCandidati.Items.Add(
                        $"{prefix}{c.Selector}  ({c.Strategy}, {c.MatchCount} potriviri)")
                Next
            End If
            If cmbCandidati.Items.Count > 0 Then
                Dim i As Integer = s.SelectedCandidateIndex
                If i < 0 OrElse i >= cmbCandidati.Items.Count Then i = 0
                cmbCandidati.SelectedIndex = i
            End If

            chkWaitForPas.Checked = s.InsertWaitFor
            txtLogValue.Text = s.LogValue
            btnStergePas.Text = If(s.Deleted, "Reactivează pasul", "Șterge pasul")
            pnlDetaliu.Visible = True
        Finally
            _updatingDetail = False
        End Try
    End Sub

    Private Sub CmbCandidati_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbCandidati.SelectedIndexChanged
        Try
            If _updatingDetail Then Return
            Dim s As RecordedStep = SelectedStep()
            If s Is Nothing OrElse cmbCandidati.SelectedIndex < 0 Then Return
            s.SelectedCandidateIndex = cmbCandidati.SelectedIndex
            RefreshRow(SelectedIndex())
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.CmbCandidati_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub ChkWaitForPas_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkWaitForPas.CheckedChanged
        Try
            If _updatingDetail Then Return
            Dim s As RecordedStep = SelectedStep()
            If s Is Nothing Then Return
            s.InsertWaitFor = chkWaitForPas.Checked
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.ChkWaitForPas_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub TxtLogValue_TextChanged(sender As Object, e As EventArgs) Handles txtLogValue.TextChanged
        Try
            If _updatingDetail Then Return
            Dim s As RecordedStep = SelectedStep()
            If s Is Nothing Then Return
            s.LogValue = txtLogValue.Text
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.TxtLogValue_TextChanged", ex)
        End Try
    End Sub

    Private Sub BtnStergePas_Click(sender As Object, e As EventArgs) Handles btnStergePas.Click
        Try
            Dim s As RecordedStep = SelectedStep()
            If s Is Nothing Then Return
            ' Logical delete: the row stays so the numbering of the raw trail holds.
            s.Deleted = Not s.Deleted
            btnStergePas.Text = If(s.Deleted, "Reactivează pasul", "Șterge pasul")
            RefreshRow(SelectedIndex())
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnStergePas_Click", ex)
        End Try
    End Sub

    Private Sub BtnSus_Click(sender As Object, e As EventArgs) Handles btnSus.Click
        MoveSelected(-1)
    End Sub

    Private Sub BtnJos_Click(sender As Object, e As EventArgs) Handles btnJos.Click
        MoveSelected(1)
    End Sub

    Private Sub MoveSelected(delta As Integer)
        Try
            Dim i As Integer = SelectedIndex()
            Dim target As Integer = i + delta
            If i < 0 OrElse target < 0 OrElse target >= _steps.Count Then Return

            Dim moved As RecordedStep = _steps(i)
            _steps.RemoveAt(i)
            _steps.Insert(target, moved)

            RebuildList()
            lvPasi.Items(target).Selected = True
            lvPasi.EnsureVisible(target)
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.MoveSelected", ex)
        End Try
    End Sub

    Private Sub RefreshRow(position As Integer)
        If position < 0 OrElse position >= lvPasi.Items.Count Then Return
        Dim s As RecordedStep = _steps(position)
        Dim row As ListViewItem = lvPasi.Items(position)
        row.SubItems(1).Text = DescribeKind(s)
        row.SubItems(2).Text = s.ChosenSelector
        row.SubItems(3).Text = If(String.IsNullOrEmpty(s.Value), s.Text, s.Value)
        row.SubItems(4).Text = If(s.TriggeredAjax, "da", "")
        ApplyRowLook(row, s)
    End Sub

    Private Sub RebuildList()
        lvPasi.BeginUpdate()
        Try
            lvPasi.Items.Clear()
            For k As Integer = 0 To _steps.Count - 1
                lvPasi.Items.Add(BuildRow(_steps(k), k))
            Next
        Finally
            lvPasi.EndUpdate()
        End Try
    End Sub

    ' =========================================================================
    '  Generation toolbar
    ' =========================================================================
    Private Sub BtnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        Try
            RefreshPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnGenereaza_Click", ex)
        End Try
    End Sub

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _steps.Count = 0 Then
                KBotMessage.Show("Nu există niciun pas înregistrat.", MsgBoxStyle.Information, "K-BOT Recorder")
                Return
            End If

            Using dialog As New SaveFileDialog()
                dialog.Filter = "Fișiere workflow (*.wfl)|*.wfl"
                dialog.DefaultExt = "wfl"
                dialog.FileName = "inregistrare.wfl"
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

                Dim numeWorkflow As String = Path.GetFileNameWithoutExtension(dialog.FileName)
                Dim xml As String = BuildWfl(numeWorkflow)
                File.WriteAllText(dialog.FileName, xml, New UTF8Encoding(True))
                KBotMessage.Show($"Fișierul a fost salvat: {dialog.FileName}",
                       MsgBoxStyle.Information, "K-BOT Recorder")
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.BtnSalveaza_Click", ex)
            KBotMessage.Show(ex.Message, MsgBoxStyle.Critical, "K-BOT Recorder")
        End Try
    End Sub

    ' =========================================================================
    '  Preview
    ' =========================================================================
    Private Function BuildWfl(numeWorkflow As String) As String
        Dim options As New CompactorOptions With {
            .InsertWaitForAutomatically = chkWaitForAutomat.Checked,
            .AddResetBlock = chkBlocReset.Checked
        }
        Dim actions As List(Of IWorkflowAction) = RecorderCompactor.Compact(_steps, options)
        Return WflWriter.Write(actions, numeWorkflow, "current", FirstUrl())
    End Function

    Private Function FirstUrl() As String
        For Each s As RecordedStep In _steps
            If Not String.IsNullOrEmpty(s.Url) Then Return s.Url
        Next
        Return String.Empty
    End Function

    Private Sub RefreshPreview()
        Try
            rtbPreview.ForeColor = If(KBotTheme.IsDark, KBotTheme.CLR_FG, SystemColors.WindowText)
            rtbPreview.Text = BuildWfl("Înregistrare K-BOT")
        Catch ex As Exception
            ' A broken selector must be visible in the preview, not only in the log.
            GlobalErrorLog.Write("RecorderForm.RefreshPreview", ex)
            rtbPreview.ForeColor = ClrEroare
            rtbPreview.Text = "Nu pot genera fișierul: " & ex.Message
        End Try
    End Sub

    Private Sub UpdateButtons()
        If Me.IsDisposed Then Return
        Dim hasExecutor As Boolean = _executor IsNot Nothing
        Dim docked As Boolean = hasExecutor AndAlso _executor.IsDocked
        Dim recording As Boolean = hasExecutor AndAlso _executor.RecordingActive

        btnAndocheaza.Enabled = hasExecutor AndAlso Not docked
        btnDetaseaza.Enabled = docked
        btnResincronizeaza.Enabled = docked
        btnPornesteInreg.Enabled = docked AndAlso Not recording
        btnOpresteInreg.Enabled = recording
        ' The monitor only needs a live executor - it works undocked too.
        btnMonitor.Enabled = hasExecutor
    End Sub

    ' =========================================================================
    '  Keeping the docked browser glued to the panel
    ' =========================================================================
    Private Sub RecorderForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        ScheduleResync()
    End Sub

    Private Sub RecorderForm_Move(sender As Object, e As EventArgs) Handles Me.Move
        ScheduleResync()
    End Sub

    Private Sub RecorderForm_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        ScheduleResync()
    End Sub

    Private Sub SplitMain_SplitterMoved(sender As Object, e As SplitterEventArgs) _
        Handles splitMain.SplitterMoved
        ScheduleResync()
    End Sub

    ''' <summary>Debounce: a drag fires hundreds of events, CDP must not see them all.</summary>
    Private Sub ScheduleResync()
        Try
            If _executor Is Nothing Then Return
            ' Undocked the timer still has work to do - it pushes the browser back above this
            ' form. Only a session with no browser at all leaves it idle.
            If Not _executor.IsDocked AndAlso Not _executor.IsBrowserOpen Then Return
            tmrResync.Stop()
            tmrResync.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.ScheduleResync", ex)
        End Try
    End Sub

    Private Async Sub TmrResync_Tick(sender As Object, e As EventArgs) Handles tmrResync.Tick
        tmrResync.Stop()
        Try
            If _executor Is Nothing Then Return
            If _executor.IsDocked Then
                Await _executor.SyncDockedBoundsAsync()
            ElseIf _executor.IsBrowserOpen Then
                ' Not docked, so nothing moves the browser - but activating this form would
                ' bury it. Put it back just above us, without stealing the focus.
                Await _executor.RaiseBrowserAboveAsync(Me)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.TmrResync_Tick", ex)
        End Try
    End Sub

    ' =========================================================================
    '  Teardown - stop recording first, then undock
    ' =========================================================================
    Private Async Sub RecorderForm_FormClosing(sender As Object, e As FormClosingEventArgs) _
        Handles Me.FormClosing
        If _closingDown Then Return

        e.Cancel = True
        _closingDown = True
        Try
            tmrResync.Stop()
            _executor?.StopRecording()
            If _executor IsNot Nothing AndAlso _executor.IsDocked Then
                Await _executor.UndockBrowserAsync()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RecorderForm.RecorderForm_FormClosing", ex)
        End Try

        Me.Close()
    End Sub

    ' =========================================================================
    '  Dispose - same order as FormClosing
    ' =========================================================================
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            Try
                _executor?.StopRecording()
            Catch ex As Exception
                GlobalErrorLog.Write("RecorderForm.Dispose", ex)
            End Try
            DetachExecutor()
            ' The monitor cancels a user close and hides instead, so nothing but this
            ' disposes it. Dispose bypasses FormClosing, which is exactly what is wanted.
            If _monitor IsNot Nothing Then
                _monitor.Dispose()
                _monitor = Nothing
            End If
            If _themeHooked Then
                RemoveHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
                _themeHooked = False
            End If
            If _strikeFont IsNot Nothing Then
                _strikeFont.Dispose()
                _strikeFont = Nothing
            End If
            If components IsNot Nothing Then components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
