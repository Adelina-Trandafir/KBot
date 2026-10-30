Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The tray's message above the clock: the documents waiting for the signature, one row each. A click on a row asks for
''' that document (<see cref="DocumentChosen"/>); a click anywhere else on the window asks for the list with no document
''' picked (<see cref="ListChosen"/>). It never takes the focus when it appears (the user may be typing in another program),
''' goes away by itself after a while, and also when the user clicks outside it after having used it.
''' </summary>
Friend Class DocumentsToast

    Friend Event DocumentChosen(k_document As DdfDeSemnat)
    Friend Event ListChosen()

    Private Const MaxVisibleRows As Integer = 6
    Private Const MarginFromEdge As Integer = 12
    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    Private _documents As IReadOnlyList(Of DdfDeSemnat) = New List(Of DdfDeSemnat)()
    Private _hot As Integer = -1
    Private _wasActivated As Boolean
    Private _loaded As Boolean
    Private _headerFont As Font

    Friend Sub New()
        InitializeComponent()
        CenterOnScreen = False
        AutoFitToTheme = False
        _headerFont = New Font(lblHeader.Font.FontFamily, lblHeader.Font.Size + 1.0F, FontStyle.Bold)
        lblHeader.Font = _headerFont
    End Sub

    ''' <summary>No focus when it appears, no entry in Alt+Tab.</summary>
    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim k_params As CreateParams = MyBase.CreateParams
            k_params.ExStyle = k_params.ExStyle Or WS_EX_TOOLWINDOW
            Return k_params
        End Get
    End Property

    ''' <summary>Shows the rows and puts the window above the clock (the corner of the work area of the main screen).</summary>
    ''' <param name="k_unreadUnits">How many units' lists the server could not read: the user is told the list may be incomplete.</param>
    Friend Sub ShowDocuments(k_documents As IReadOnlyList(Of DdfDeSemnat), k_unreadUnits As Integer)
        Try
            _documents = If(k_documents, New List(Of DdfDeSemnat)())
            _hot = -1
            _wasActivated = False
            lstDocs.BeginUpdate()
            Try
                lstDocs.Items.Clear()
                For Each k_document As DdfDeSemnat In _documents
                    lstDocs.Items.Add(k_document)
                Next
            Finally
                lstDocs.EndUpdate()
            End Try

            lblHeader.Text = If(_documents.Count = 0, "Nu aveți documente de semnat.",
                                If(_documents.Count = 1, "Aveți 1 document de semnat.",
                                   $"Aveți {_documents.Count} documente de semnat."))
            lblFooter.Text = If(k_unreadUnits > 0,
                                "Lista unei unități nu a putut fi citită acum. Clic pe fereastră deschide K-BOT.",
                                If(_documents.Count = 0, "Clic pe fereastră deschide K-BOT.", "Clic pe un document îl deschide în K-BOT."))

            ' Before the first Show the form is not scaled to the screen yet: OnLoad places it once it is.
            If _loaded Then Place()
            tmrHide.Stop()
            tmrHide.Start()
            If Not Visible Then Show()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.ShowDocuments", ex)
            Throw
        End Try
    End Sub

    Private Sub Place()
        Dim k_rowHeight As Integer = lstDocs.ItemHeight
        Dim k_rows As Integer = Math.Min(_documents.Count, MaxVisibleRows)
        Dim k_chrome As Integer = lblHeader.Height + lblFooter.Height + Padding.Vertical
        Dim k_height As Integer = k_chrome + k_rows * k_rowHeight
        Dim k_width As Integer = LogicalToDeviceUnits(420)
        Dim k_area As Rectangle = Screen.PrimaryScreen.WorkingArea
        Dim k_margin As Integer = LogicalToDeviceUnits(MarginFromEdge)
        Bounds = New Rectangle(k_area.Right - k_width - k_margin, k_area.Bottom - k_height - k_margin, k_width, k_height)
        lstDocs.Visible = _documents.Count > 0
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ' The designer scaled the sizes; the height of a row of an owner-drawn list is not one of them.
            lstDocs.ItemHeight = LogicalToDeviceUnits(46)
            _loaded = True
            Place()
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_palette As ThemePalette = ThemeManager.Current.Palette
            ' The form background IS the 1 px outline of the window (Padding(1)).
            BackColor = k_palette.BorderColor
            lblHeader.BackColor = k_palette.SurfaceAltColor
            lblHeader.ForeColor = k_palette.TextColor
            lblFooter.BackColor = k_palette.SurfaceAltColor
            lblFooter.ForeColor = k_palette.TextDimColor
            lstDocs.BackColor = k_palette.SurfaceColor
            lstDocs.ForeColor = k_palette.TextColor
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.OnThemeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        _headerFont?.Dispose()
        _headerFont = Nothing
    End Sub

    ' ── Rows ─────────────────────────────────────────────────────────────────────

    Private Sub LstDocs_DrawItem(k_sender As Object, e As DrawItemEventArgs) Handles lstDocs.DrawItem
        Try
            If e.Index < 0 OrElse e.Index >= _documents.Count Then Return
            Dim k_palette As ThemePalette = ThemeManager.Current.Palette
            Dim k_document As DdfDeSemnat = _documents(e.Index)
            Dim k_back As Color = If(e.Index = _hot, k_palette.ButtonHoverColor, k_palette.SurfaceColor)
            Using k_brush As New SolidBrush(k_back)
                e.Graphics.FillRectangle(k_brush, e.Bounds)
            End Using
            Using k_pen As New Pen(k_palette.BorderColor)
                e.Graphics.DrawLine(k_pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
            End Using

            Dim k_pad As Integer = LogicalToDeviceUnits(10)
            Dim k_half As Integer = e.Bounds.Height \ 2
            Dim k_flags As TextFormatFlags = TextFormatFlags.EndEllipsis Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
            Dim k_ro As New CultureInfo("ro-RO")
            Dim k_date As String = If(k_document.DataRev.HasValue, k_document.DataRev.Value.ToString("dd.MM.yyyy", k_ro), String.Empty)

            Dim k_top As String = $"{k_document.CodAngajament}  ·  rev. {k_document.NumarRev}  ·  {k_document.Total.ToString("#,##0.00", k_ro)}"
            Dim k_bottom As String = $"{k_document.NumeUnitate}  ·  {k_document.ObiectDdf}"
            If k_date.Length > 0 Then k_bottom = k_date & "  ·  " & k_bottom

            Dim k_topRect As New Rectangle(e.Bounds.Left + k_pad, e.Bounds.Top + 2, e.Bounds.Width - 2 * k_pad, k_half - 2)
            Dim k_bottomRect As New Rectangle(e.Bounds.Left + k_pad, e.Bounds.Top + k_half, e.Bounds.Width - 2 * k_pad, k_half - 2)
            Using k_bold As New Font(e.Font, FontStyle.Bold)
                TextRenderer.DrawText(e.Graphics, k_top, k_bold, k_topRect, k_palette.TextColor, k_flags)
            End Using
            TextRenderer.DrawText(e.Graphics, k_bottom, e.Font, k_bottomRect, k_palette.TextDimColor, k_flags)
        Catch ex As Exception
            ' UI boundary (paint): log and swallow.
            GlobalErrorLog.Write("DocumentsToast.LstDocs_DrawItem", ex)
        End Try
    End Sub

    Private Sub LstDocs_MouseMove(k_sender As Object, e As MouseEventArgs) Handles lstDocs.MouseMove
        Try
            Dim k_index As Integer = RowAt(e.Location)
            If k_index = _hot Then Return
            _hot = k_index
            lstDocs.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.LstDocs_MouseMove", ex)
        End Try
    End Sub

    Private Sub LstDocs_MouseLeave(k_sender As Object, e As EventArgs) Handles lstDocs.MouseLeave
        If _hot = -1 Then Return
        _hot = -1
        lstDocs.Invalidate()
    End Sub

    ''' <summary>The row under a point, -1 when the point is on empty space below the rows.</summary>
    Private Function RowAt(k_point As Point) As Integer
        Dim k_index As Integer = lstDocs.IndexFromPoint(k_point)
        If k_index < 0 OrElse k_index >= lstDocs.Items.Count Then Return -1
        Return If(lstDocs.GetItemRectangle(k_index).Contains(k_point), k_index, -1)
    End Function

    Private Sub LstDocs_MouseUp(k_sender As Object, e As MouseEventArgs) Handles lstDocs.MouseUp
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim k_index As Integer = RowAt(e.Location)
            Hide()
            If k_index >= 0 Then
                RaiseEvent DocumentChosen(_documents(k_index))
            Else
                RaiseEvent ListChosen()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.LstDocs_MouseUp", ex)
        End Try
    End Sub

    ' ── Everything that is not a row: the list without a selection ────────────────

    Private Sub Background_Click(k_sender As Object, e As EventArgs) Handles lblHeader.Click, lblFooter.Click, Me.Click
        Try
            Hide()
            RaiseEvent ListChosen()
        Catch ex As Exception
            GlobalErrorLog.Write("DocumentsToast.Background_Click", ex)
        End Try
    End Sub

    ' ── Going away ───────────────────────────────────────────────────────────────

    Private Sub TmrHide_Tick(k_sender As Object, e As EventArgs) Handles tmrHide.Tick
        tmrHide.Stop()
        Hide()
    End Sub

    Private Sub DocumentsToast_Activated(k_sender As Object, e As EventArgs) Handles Me.Activated
        _wasActivated = True
    End Sub

    ' It was used (clicked into), now the user went elsewhere: the message has done its job.
    Private Sub DocumentsToast_Deactivate(k_sender As Object, e As EventArgs) Handles Me.Deactivate
        If _wasActivated Then Hide()
    End Sub

    Private Sub DocumentsToast_KeyDown(k_sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then Hide()
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        If Not Visible Then tmrHide.Stop()
    End Sub
End Class
