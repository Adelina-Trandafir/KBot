Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The opening page of the main window: a foldable «Ce este nou» card at the top right (opened again
''' after every update, until the operator folds it; the last versions of <c>NOUTATI.md</c>, the same text as «Setari -> Istoric
''' versiuni»; read only, no selection, no focus) and the K-BOT mark in the corner. The side panel
''' (recent machete, recent actions, session information, session errors) belongs to the main
''' window, so it stays beside every page, this one included. A page of <c>AvacontMain</c>: it stays
''' loaded and is only shown or hidden.
''' </summary>
Public Class AcasaPage

    Private ReadOnly _session As SessionContext
    Private _newsFilter As NoInteractionFilter
    Private _newsFullHeight As Integer
    Private _newsSignature As String   ' what the news box was last filled with (see RefreshState)
    Private _arrowDown As Bitmap
    Private _arrowUp As Bitmap

    Public Sub New(k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _session = k_session
        edNoutati.Editabil = False
        MakeNewsReadOnly()
        ' After an update the news open by themselves, once; from then on the button decides.
        Dim k_version As String = CurrentVersionText()
        If Not String.Equals(AppSettings.Current.NewsSeenVersion, k_version, StringComparison.Ordinal) Then
            AppSettings.Current.NewsSeenVersion = k_version
            AppSettings.Current.HomeNewsCollapsed = False
            AppSettings.Current.Save()
        End If
        ApplyNewsFolding(AppSettings.Current.HomeNewsCollapsed)
    End Sub

    Private Shared Function CurrentVersionText() As String
        Try
            Return AppUpdateService.CurrentVersion.ToString()
        Catch ex As Exception
            ' Already logged by the service.
            Return String.Empty
        End Try
    End Function

    ''' <summary>
    ''' Reads everything again. Called each time the page is brought back on screen. The news box is written only when what it
    ''' would show differs from what it shows now (text, font, colour, dpi): refilling the same text made it flash and jump back
    ''' to the top every time the operator came back to this page.
    ''' </summary>
    Public Sub RefreshState()
        Try
            Dim k_blocks As List(Of NoteBlock) = ReleaseNotesText.HistoryBlocks(ReleaseNotesText.RecentVersions)
            Dim k_box As RichTextBox = edNoutati.TextBox
            Dim k_sign As String = NewsSignature(k_box, k_blocks)
            If String.Equals(k_sign, _newsSignature, StringComparison.Ordinal) Then Return
            ReleaseNotesText.Fill(k_box, k_blocks)
            _newsSignature = k_sign
        Catch ex As Exception
            ' UI boundary (called from the host when the page is shown): log and swallow.
            GlobalErrorLog.Write("AcasaPage.RefreshState", ex)
        End Try
    End Sub

    ' Everything that decides how the news box looks: the paragraphs, and the font / colour / dpi they are drawn with.
    Private Shared Function NewsSignature(k_box As RichTextBox, k_blocks As IEnumerable(Of NoteBlock)) As String
        Dim k_sb As New System.Text.StringBuilder()
        k_sb.Append(k_box.Font.ToString()).Append("|"c).Append(k_box.ForeColor.ToArgb()).Append("|"c).Append(k_box.DeviceDpi).Append("|"c)
        For Each k_block As NoteBlock In k_blocks
            k_sb.Append(CInt(k_block.Kind)).Append(":"c).Append(k_block.Text).Append(ControlChars.Lf)
        Next
        Return k_sb.ToString()
    End Function

    ' ---- «Ce este nou»: folding, and a text that can only be looked at -------------------------------

    Private Sub BtnNoutati_Click(sender As Object, e As EventArgs) Handles btnNoutati.Click
        Try
            Dim k_fold As Boolean = edNoutati.Visible
            ApplyNewsFolding(k_fold)
            AppSettings.Current.HomeNewsCollapsed = k_fold
            AppSettings.Current.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.BtnNoutati_Click", ex)
        End Try
    End Sub

    ' Folded = the title bar only; unfolded = the text box under it.
    Private Sub ApplyNewsFolding(k_folded As Boolean)
        If _newsFullHeight = 0 Then _newsFullHeight = pnlNoutati.Height
        edNoutati.Visible = Not k_folded
        pnlNoutati.Height = If(k_folded, pnlNoutatiHead.Height, _newsFullHeight)
        ShowNewsArrow(k_folded)
    End Sub

    ' The fold button carries an icon: the "down" arrow of the resources, painted in the text colour of the theme
    ' (the stock one is blue) and turned over when the card is open (click = fold).
    Private Sub BuildNewsArrows()
        _arrowDown?.Dispose()
        _arrowUp?.Dispose()
        Dim k_color As Color = ThemeManager.Current.Palette.TextColor
        Dim k_side As Integer = ThemeShapes.ScaleDpi(Me, 18)
        _arrowDown = Tint(My.Resources.Resources.down, k_color, k_side)
        _arrowUp = DirectCast(_arrowDown.Clone(), Bitmap)
        _arrowUp.RotateFlip(RotateFlipType.Rotate180FlipNone)
    End Sub

    ' Same alpha as the source, every pixel in one colour.
    Private Shared Function Tint(k_source As Image, k_color As Color, k_side As Integer) As Bitmap
        Dim k_result As New Bitmap(k_side, k_side)
        Using k_g As Graphics = Graphics.FromImage(k_result)
            Dim k_matrix As New Imaging.ColorMatrix(New Single()() {
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 1, 0},
                New Single() {k_color.R / 255.0F, k_color.G / 255.0F, k_color.B / 255.0F, 0, 1}})
            Using k_attrs As New Imaging.ImageAttributes()
                k_attrs.SetColorMatrix(k_matrix)
                k_g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                k_g.DrawImage(k_source, New Rectangle(0, 0, k_side, k_side), 0, 0, k_source.Width, k_source.Height, GraphicsUnit.Pixel, k_attrs)
            End Using
        End Using
        Return k_result
    End Function

    Private Sub ShowNewsArrow(k_folded As Boolean)
        If _arrowDown Is Nothing Then BuildNewsArrows()
        btnNoutati.Image = If(k_folded, _arrowDown, _arrowUp)
    End Sub

    ' No caret, no selection, no focus: the text is only read (the wheel still scrolls it).
    Private Sub MakeNewsReadOnly()
        Dim k_box As RichTextBox = edNoutati.TextBox
        k_box.TabStop = False
        k_box.Cursor = Cursors.Default
        k_box.ShortcutsEnabled = False
        _newsFilter = New NoInteractionFilter()
        If k_box.IsHandleCreated Then _newsFilter.AssignHandle(k_box.Handle)
        AddHandler k_box.HandleCreated, Sub(k_s, k_e) _newsFilter.AssignHandle(k_box.Handle)
        AddHandler k_box.HandleDestroyed, Sub(k_s, k_e) _newsFilter.ReleaseHandle()
    End Sub

    ''' <summary>Swallows the mouse buttons, the context menu and the focus message of a control: it can be scrolled, nothing more.</summary>
    Private NotInheritable Class NoInteractionFilter
        Inherits NativeWindow

        Private Const WM_SETFOCUS As Integer = &H7
        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_LBUTTONDBLCLK As Integer = &H203
        Private Const WM_RBUTTONDOWN As Integer = &H204
        Private Const WM_RBUTTONDBLCLK As Integer = &H206
        Private Const WM_MBUTTONDOWN As Integer = &H207
        Private Const WM_CONTEXTMENU As Integer = &H7B

        Protected Overrides Sub WndProc(ByRef k_m As Message)
            Select Case k_m.Msg
                Case WM_SETFOCUS, WM_LBUTTONDOWN, WM_LBUTTONDBLCLK, WM_RBUTTONDOWN, WM_RBUTTONDBLCLK, WM_MBUTTONDOWN, WM_CONTEXTMENU
                    k_m.Result = IntPtr.Zero
                Case Else
                    MyBase.WndProc(k_m)
            End Select
        End Sub
    End Class

    ' ---- Theme -------------------------------------------------------------------------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_palette As ThemePalette = ThemeManager.Current.Palette
            pnlNoutati.BackColor = k_palette.SurfaceColor
            pnlNoutatiHead.BackColor = k_palette.SurfaceColor
            lblNoutati.BackColor = k_palette.SurfaceColor
            lblNoutati.ForeColor = k_palette.TextColor
            ButtonStyles.ApplyTransparent(btnNoutati, ThemeManager.Current, k_palette.SurfaceColor, k_palette.TextColor)
            BuildNewsArrows()
            ShowNewsArrow(Not edNoutati.Visible)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.OnThemeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _arrowDown?.Dispose()
        _arrowUp?.Dispose()
        MyBase.OnFormClosed(e)
    End Sub
End Class
