Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The floating bar of a help capture (slice 0000-02): which picture, what to set up by hand, and
''' what K-BOT could not do on its own. It stays on top, bottom-right of the working area, while the
''' operator uses K-BOT underneath; «Capturează» or «Renunță» ends it (<see cref="Finished"/>).
''' </summary>
Public Class HelpCapturePromptForm

    ''' <summary>Raised once: True = «Capturează», False = «Renunță» / the bar was closed.</summary>
    Public Event Finished(capture As Boolean)

    Private _done As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(capture As HelpCapture, note As String)
        Me.New()
        lblImagine.Text = capture.Caption
        lblPregatire.Text = If(String.IsNullOrWhiteSpace(capture.Prepare),
                               "Nimic de pregătit: ecranul e cel potrivit. Apasă «Capturează».",
                               capture.Prepare)
        lblNota.Text = If(note, String.Empty)
        lblNota.Visible = Not String.IsNullOrEmpty(note)
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ' Bottom-right of the screen the cursor is on, out of the way of what gets photographed.
            Dim area As Rectangle = Screen.FromPoint(Cursor.Position).WorkingArea
            Location = New Point(area.Right - Width - 16, area.Bottom - Height - 16)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor
            lblNota.ForeColor = p.WarningColor
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub BtnCaptureaza_Click(sender As Object, e As EventArgs) Handles btnCaptureaza.Click
        Finish(True)
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Finish(False)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Finish(False)   ' the caption bar's close = give up
    End Sub

    Private Sub Finish(capture As Boolean)
        Try
            If _done Then Return
            _done = True
            RaiseEvent Finished(capture)
            If Not IsDisposed Then Close()
        Catch ex As Exception
            ' UI boundary (click / close handlers).
            GlobalErrorLog.Write("HelpCapturePromptForm.Finish", ex)
        End Try
    End Sub

End Class
