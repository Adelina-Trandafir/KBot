Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Capturi pentru ajutor» (slice 0000-02): every screenshot the help topics ask for, each with its
''' own «Fă poza» / «Refă» button. One press:
''' <list type="number">
''' <item>this window steps aside and K-BOT goes to the capture's <c>goto</c> screen;</item>
''' <item>a floating bar says what to set up by hand and waits for «Capturează»;</item>
''' <item>the screen freezes (<see cref="HelpCaptureOverlay"/>) and the operator marks the area;</item>
''' <item>the PNG is written where the tag says (<see cref="HelpCaptureStore"/>), the row turns
''' done and an open help window shows the new picture.</item>
''' </list>
''' Reached only in capture mode (Setări › Aplicație, under the advanced options).
''' </summary>
Public Class HelpCaptureForm

    Private Const ColPart As String = "parte"
    Private Const ColTopic As String = "pagina"
    Private Const ColCaption As String = "imagine"
    Private Const ColPrepare As String = "pregatire"
    Private Const ColState As String = "stare"
    Private Const ColShoot As String = "poza"
    Private Const ColLoad As String = "incarca"
    Private Const ColView As String = "vezi"

    Private Shared _instance As HelpCaptureForm

    Private ReadOnly _help As HelpService
    Private _store As HelpCaptureStore
    Private ReadOnly _rows As New List(Of HelpCapture)()
    Private _busy As Boolean
    ' Slice 0000-23: brings the help window back after a capture (it is always on top, so it
    ' steps aside -- minimized -- while the screen is photographed).
    Private _restoreHelp As Action

    ''' <summary>Designer only.</summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    Public Sub New(help As HelpService)
        InitializeComponent()
        _help = help
    End Sub

    ''' <summary>Opens the window, or brings the open one to the front.</summary>
    Public Shared Sub ShowFor(owner As IWin32Window, help As HelpService)
        Try
            If _instance Is Nothing OrElse _instance.IsDisposed Then
                _instance = New HelpCaptureForm(help)
                _instance.Show(owner)
            Else
                If _instance.WindowState = FormWindowState.Minimized Then _instance.WindowState = FormWindowState.Normal
                _instance.Activate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.ShowFor", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            If _help Is Nothing Then Return
            Fill()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor
            lblDosar.ForeColor = p.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ── List ──────────────────────────────────────────────────────────────────────

    Private Sub Fill()
        Dim library As HelpLibrary = _help.Library
        _store = New HelpCaptureStore(library)
        Dim all As List(Of HelpCapture) = library.Captures
        Dim done As Integer = all.Where(Function(c) _store.Exists(c)).Count()
        lblSumar.Text = done & " din " & all.Count & " imagini făcute" &
                        If(library.Problems.Count > 0, "   ·   " & library.Problems.Count & " probleme în paginile de ajutor (vezi jurnalul de erori)", String.Empty)
        lblDosar.Text = "Se salvează în: " & _store.RuntimeFolder &
                        If(_store.SourceFolder IsNot Nothing, "   și în sursă: " & _store.SourceFolder, String.Empty)

        _rows.Clear()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each c As HelpCapture In all
                Dim taken As DateTime? = _store.TakenAt(c)
                If chkDoarLipsa.Checked AndAlso taken.HasValue Then Continue For
                Dim row As KBotDataRow = grid.AddRow()
                row(ColPart) = PartShort(c.Part)
                row(ColTopic) = c.TopicTitle
                row(ColCaption) = c.Caption
                row(ColPrepare) = If(c.Prepare, String.Empty)
                row(ColState) = If(taken.HasValue, "făcută " & taken.Value.ToString("dd.MM HH:mm"), "lipsă")
                row(ColShoot) = If(taken.HasValue, "Refă", "Fă poza")
                row(ColView) = "Vezi"
                row(ColLoad) = "Încarcă"
                _rows.Add(c)
            Next
        Finally
            grid.EndUpdate()
        End Try
        ShowDetail()
    End Sub

    Private Shared Function PartShort(part As HelpPart) As String
        Select Case part
            Case HelpPart.Contabil : Return "Contabil"
            Case HelpPart.Avansat : Return "Avansat"
            Case HelpPart.Director : Return "Director"
            Case Else : Throw New ArgumentException("Unknown help part: " & part.ToString(), NameOf(part))
        End Select
    End Function

    ' ── Detail footer ─────────────────────────────────────────────────────────────

    Private Sub Grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            ShowDetail()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.Grid_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>Fills the footer from the current row: its caption, what to prepare, and the saved picture.</summary>
    Private Sub ShowDetail()
        ClearPreview()
        Dim idx As Integer = grid.CurrentRowIndex
        If _store Is Nothing OrElse idx < 0 OrElse idx >= _rows.Count Then
            txtImagine.Text = String.Empty
            txtPregatire.Text = String.Empty
            Return
        End If
        Dim capture As HelpCapture = _rows(idx)
        txtImagine.Text = capture.Caption
        txtPregatire.Text = If(capture.Prepare, String.Empty)
        If Not _store.Exists(capture) Then Return
        Try
            ' Copied into a fresh bitmap so the file stays free (GDI+ holds a file open).
            Using source As Image = Image.FromFile(_store.PathFor(capture))
                picPreview.Image = New Bitmap(source)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.ShowDetail", ex)
        End Try
    End Sub

    Private Sub ClearPreview()
        Dim old As Image = picPreview.Image
        picPreview.Image = Nothing
        old?.Dispose()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        ClearPreview()
    End Sub

    Private Sub ChkDoarLipsa_CheckedChanged(sender As Object, e As EventArgs) Handles chkDoarLipsa.CheckedChanged
        Try
            If _help IsNot Nothing AndAlso IsHandleCreated Then Fill()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.ChkDoarLipsa_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub BtnReincarca_Click(sender As Object, e As EventArgs) Handles btnReincarca.Click
        Try
            _help.ReloadLibrary()
            Fill()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.BtnReincarca_Click", ex)
        End Try
    End Sub

    Private Sub BtnDosar_Click(sender As Object, e As EventArgs) Handles btnDosar.Click
        Try
            Directory.CreateDirectory(_store.RuntimeFolder)
            Process.Start(New ProcessStartInfo(_store.RuntimeFolder) With {.UseShellExecute = True})?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.BtnDosar_Click", ex)
        End Try
    End Sub

    ' ── One capture ───────────────────────────────────────────────────────────────

    Private Sub Grid_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles grid.ButtonClick
        Try
            If _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= _rows.Count Then Return
            If e.ColumnKey = ColShoot Then
                StartCapture(_rows(e.RowIndex))
            ElseIf e.ColumnKey = ColLoad Then
                LoadFromDisk(_rows(e.RowIndex))
            ElseIf e.ColumnKey = ColView Then
                ViewSaved(_rows(e.RowIndex))
            End If
        Catch ex As Exception
            _busy = False
            _restoreHelp?.Invoke()
            _restoreHelp = Nothing
            If Not Visible Then Show()
            GlobalErrorLog.Write("HelpCaptureForm.Grid_ButtonClick", ex)
            KBotMessage.Show(Me, "Captura nu a putut porni: " & ex.Message, "Capturi pentru ajutor",
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>«Vezi»: opens the saved picture of this row, so the operator sees what was stored.</summary>
    Private Sub ViewSaved(capture As HelpCapture)
        If Not _store.Exists(capture) Then
            KBotMessage.Show(Me, "Imaginea «" & capture.Caption & "» nu a fost făcută încă.", "Capturi pentru ajutor",
                             MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using dlg As New HelpCaptureViewForm(capture.Caption, _store.PathFor(capture))
            dlg.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>
    ''' «Încarcă»: a picture already on disk (taken elsewhere, e.g. where K-BOT cannot reach the
    ''' screen) becomes this capture. Any PNG / JPG / BMP; it is always stored as <c>&lt;id&gt;.png</c>.
    ''' </summary>
    Private Sub LoadFromDisk(capture As HelpCapture)
        Using dlg As New OpenFileDialog()
            dlg.Title = "Încarcă imaginea «" & capture.Caption & "»"
            dlg.Filter = "Imagini (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
            ' Copied into a fresh bitmap so the source file is not kept locked (GDI+ holds it open).
            Using source As Image = Image.FromFile(dlg.FileName)
                Using picture As New Bitmap(source)
                    _store.Save(capture, picture)
                End Using
            End Using
        End Using
        _help.RefreshOpenPage()
        Fill()
    End Sub

    Private Sub StartCapture(capture As HelpCapture)
        _busy = True
        Hide()
        _restoreHelp = _help.StepAside()
        Dim note As String = GoToTarget(capture)
        Dim prompt As New HelpCapturePromptForm(capture, note)
        AddHandler prompt.Finished, Sub(ok) OnPromptFinished(capture, ok)
        ' Slice 0000-31: from here to the end of the capture (taken or given up) the popups the operator
        ' opens (menus, column filters, right-click menus) stay open when the focus moves to this tool.
        ' Released in OnPromptFinished; if the bar cannot even be shown, here.
        KBotPopupGuard.Hold()
        Try
            prompt.Show()
        Catch
            KBotPopupGuard.Release()
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Puts K-BOT on the capture's screen. Returns what the operator must do by hand, or Nothing.
    ''' Never throws: a screen K-BOT cannot open is not a reason to lose the capture.
    ''' </summary>
    Private Function GoToTarget(capture As HelpCapture) As String
        Return _help.Navigate(capture.Target, capture.TopicId)
    End Function

    ' UI boundary (async void, from the prompt's buttons): log and swallow, always come back.
    Private Async Sub OnPromptFinished(capture As HelpCapture, shoot As Boolean)
        Try
            If shoot Then
                ' Let the bar close and the screen underneath repaint before it is frozen.
                Await Task.Delay(400).ConfigureAwait(True)
                Dim screen As Screen = Screen.FromPoint(Cursor.Position)
                ' Slice 0000-23: user, unit, RO... accounts and CNPs are blurred before the picture is shown.
                Dim redaction As New HelpCaptureRedaction(_help.Session)
                Using picture As Bitmap = HelpCaptureOverlay.Choose(Nothing, screen, New List(Of IntPtr) From {Handle}, redaction)
                    If picture IsNot Nothing Then
                        _store.Save(capture, picture)
                        _help.RefreshOpenPage()
                    End If
                End Using
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureForm.OnPromptFinished", ex)
            KBotMessage.Show(Me, "Poza nu a putut fi salvată: " & ex.Message, "Capturi pentru ajutor",
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            _busy = False
            ' Slice 0000-31: popups behave normally again; the ones kept open close now.
            KBotPopupGuard.Release()
            Try
                Dim restore As Action = _restoreHelp
                _restoreHelp = Nothing
                restore?.Invoke()
                If Not IsDisposed Then
                    Show()
                    Fill()
                    Activate()
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("HelpCaptureForm.OnPromptFinished", ex)
            End Try
        End Try
    End Sub

End Class
