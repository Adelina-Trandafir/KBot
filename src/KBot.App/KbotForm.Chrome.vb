Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' The shell's chrome (slice 0086 split out of KbotForm.vb): theme accents, the 1 px lines of
''' the header and status bands.
''' </summary>
''' <remarks>
''' Switching the theme scheme has no handler here. The selector lives in the caption bar
''' (slice 0029, <c>capBar.ShowThemeButton</c>), and <c>ThemeManager.SetScheme</c> broadcasts the
''' scheme over every open form -- the shell has nothing to do after it.
''' </remarks>
Partial Public Class KbotForm

    ' The theme-aware semantic colours (run after ThemeManager.Apply and on every switch).
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme = ThemeManager.Current
            Dim p = scheme.Palette

            ' The form's background IS the window's 1 px outline (see LoginForm).
            BackColor = p.BorderColor

            ' Secondary labels -> dim text; titles stay on the full TextColor.
            lblOperator.ForeColor = p.TextDimColor
            lblTree.ForeColor = p.TextColor

            ' Slice 0086: «Angajament nou» is the header's primary action.
            ButtonStyles.ApplyPrimary(btnMeniu, scheme)

            ' The tree IS an IThemedControl: it takes its own palette and, more importantly,
            ' ThemeManager no longer recurses into its children. Pushing colours from here was
            ' exactly what wiped the designer's choices, so it is gone -- a colour set in the
            ' designer wins, one left empty follows the theme.

            ' The FOREXE band is an IThemedControl: ThemeManager already asked it to ApplyTheme
            ' and did NOT recurse into its children -- no colour is pushed over it here.
            pnlHeader.Invalidate()
            pnlStatus.Invalidate()
        Catch ex As Exception
            ' UI boundary (runs in the theme / paint cascade) -- log and swallow.
            GlobalErrorLog.Write("MainForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' The two bands (header + status) read as one bar with the caption:
    ' a 1 px line under the header, and one above the status bar.
    Private Sub PnlHeader_Paint(sender As Object, e As PaintEventArgs) Handles pnlHeader.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlHeader_Paint", ex)
        End Try
    End Sub

    Private Sub PnlStatus_Paint(sender As Object, e As PaintEventArgs) Handles pnlStatus.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlStatus_Paint", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the log viewer: since slice 0072-01 it is the «Jurnal» page of the settings
    ''' window, so the same modeless, one-instance window is shown (or brought to the front)
    ''' and switched to that page. The operator can read the log and work in the shell at the
    ''' same time, as before.
    ''' </summary>
    Private Sub ShowLog()
        Try
            SetariForm.ShowFor(Me, _setariFactory).ShowPage("jurnal")
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowLog", ex)
            KBotMessage.Show(Me, "Jurnalul nu a putut fi deschis. Detalii în jurnalul de erori.",
                             "Jurnal activitate", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Opens the settings window (modeless, one instance): the header menu row «Configurare K-BOT».</summary>
    Private Sub ShowSettings()
        Try
            SetariForm.ShowFor(Me, _setariFactory)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowSettings", ex)
            KBotMessage.Show(Me, "Fereastra de setări nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Configurare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
