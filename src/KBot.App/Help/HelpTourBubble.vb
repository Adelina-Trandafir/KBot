Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The bubble of a guided tour (slice 0000-04): step «n din m», the step's title and text, a note
''' when K-BOT could not do something by itself, and «Înapoi» / «Înainte» / «Închide». Top-most,
''' placed next to the control the step talks about. Keys: → or Enter = next, ← = back, Esc = close.
''' <see cref="HelpTourRunner"/> drives it.
''' </summary>
Public Class HelpTourBubble

    Public Event NextRequested()
    Public Event BackRequested()
    Public Event CloseRequested()

    Private _closingByRunner As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Fills the bubble for one step and sizes it to its text.</summary>
    Public Sub ShowStep(tourTitle As String, stepTitle As String, text As String, note As String,
                        index As Integer, count As Integer)
        Try
            capBar.Text = tourTitle
            lblPas.Text = "Pasul " & (index + 1) & " din " & count
            lblTitlu.Text = stepTitle
            lblText.Text = text
            lblNota.Text = If(note, String.Empty)
            lblNota.Visible = Not String.IsNullOrEmpty(note)
            btnInapoi.Enabled = index > 0
            btnInainte.Text = If(index = count - 1, "Gata", "Înainte ►")
            FitToText()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.ShowStep", ex)
            Throw
        End Try
    End Sub

    ' Height = the chrome + what the labels need at the bubble's width. Device pixels throughout.
    Private Sub FitToText()
        Dim w As Integer = tlyCorp.ClientSize.Width - tlyCorp.Padding.Horizontal
        Dim h As Integer = capBar.Height + tlyCorp.Padding.Vertical
        h += lblPas.GetPreferredSize(New Size(w, 0)).Height + lblPas.Margin.Vertical
        h += lblTitlu.GetPreferredSize(New Size(w, 0)).Height + lblTitlu.Margin.Vertical
        h += lblText.GetPreferredSize(New Size(w, 0)).Height + lblText.Margin.Vertical
        If lblNota.Visible Then h += lblNota.GetPreferredSize(New Size(w, 0)).Height + lblNota.Margin.Vertical
        h += CInt(Math.Round(40 * DeviceDpi / 96.0)) + tlyButoane.Margin.Vertical
        ClientSize = New Size(ClientSize.Width, h + Padding.Vertical + 4)
    End Sub

    ''' <summary>
    ''' Places the bubble next to <paramref name="target"/> (screen coordinates): right, else left,
    ''' else below, else above; always inside the working area. Empty target = centre of the screen
    ''' the cursor is on.
    ''' </summary>
    Public Sub PlaceNear(target As Rectangle)
        Try
            Dim gap As Integer = CInt(Math.Round(14 * DeviceDpi / 96.0))
            Dim area As Rectangle = If(target.IsEmpty, Screen.FromPoint(Cursor.Position), Screen.FromRectangle(target)).WorkingArea
            Dim s As Size = Size
            Dim candidates As New List(Of Point)()
            If Not target.IsEmpty Then
                Dim cy As Integer = target.Top + (target.Height - s.Height) \ 2
                Dim cx As Integer = target.Left + (target.Width - s.Width) \ 2
                candidates.Add(New Point(target.Right + gap, cy))
                candidates.Add(New Point(target.Left - gap - s.Width, cy))
                candidates.Add(New Point(cx, target.Bottom + gap))
                candidates.Add(New Point(cx, target.Top - gap - s.Height))
            End If
            Dim chosen As Point? = Nothing
            For Each p As Point In candidates
                Dim r As New Rectangle(p, s)
                If r.Left >= area.Left AndAlso r.Right <= area.Right Then
                    ' Horizontal fit is what matters; the vertical position is clamped below.
                    If (p.Y + s.Height > area.Top) AndAlso (p.Y < area.Bottom) Then
                        chosen = p
                        Exit For
                    End If
                End If
            Next
            Dim final As Point = If(chosen, New Point(area.Left + (area.Width - s.Width) \ 2, area.Top + (area.Height - s.Height) \ 2))
            final.X = Math.Max(area.Left, Math.Min(final.X, area.Right - s.Width))
            final.Y = Math.Max(area.Top, Math.Min(final.Y, area.Bottom - s.Height))
            Location = final
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.PlaceNear", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Closes without raising <see cref="CloseRequested"/> (the runner is already ending the tour).</summary>
    Public Sub CloseByRunner()
        _closingByRunner = True
        Close()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.AccentColor   ' the 1px outline in the accent colour: the bubble belongs to the ring
            lblPas.ForeColor = p.TextDimColor
            lblNota.ForeColor = p.WarningColor
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnThemeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Escape : RaiseEvent CloseRequested() : e.Handled = True
                Case Keys.Right, Keys.Enter : RaiseEvent NextRequested() : e.Handled = True
                Case Keys.Left : If btnInapoi.Enabled Then RaiseEvent BackRequested()
                    e.Handled = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnKeyDown", ex)
        End Try
    End Sub

    Private Sub BtnInainte_Click(sender As Object, e As EventArgs) Handles btnInainte.Click
        RaiseEvent NextRequested()
    End Sub

    Private Sub BtnInapoi_Click(sender As Object, e As EventArgs) Handles btnInapoi.Click
        RaiseEvent BackRequested()
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        RaiseEvent CloseRequested()
    End Sub

    ' The caption bar's close is «Închide» too.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            If Not _closingByRunner Then
                e.Cancel = True
                RaiseEvent CloseRequested()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnFormClosing", ex)
        End Try
    End Sub

End Class
