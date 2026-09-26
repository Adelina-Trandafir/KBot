Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' The shell's chrome (slice 0086 split out of KbotForm.vb): theme accents, the 1 px lines of
''' the header and status bands, and the caption bar's options menu.
''' </summary>
''' <remarks>
''' Switching the theme scheme has no handler here. The selector lives in the caption bar
''' (slice 0029, <c>capBar.ShowThemeButton</c>), and <c>ThemeManager.SetScheme</c> broadcasts the
''' scheme over every open form -- the shell has nothing to do after it.
''' </remarks>
Partial Public Class KbotForm

    ' The row keys of the options button menu.
    Private Const OPT_JURNAL As String = "jurnal"
    ' Slice 0072: the settings window.
    Private Const OPT_SETARI As String = "setari"

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
            lblAn.ForeColor = p.TextDimColor
            lblSs.ForeColor = p.TextDimColor
            lblTree.ForeColor = p.TextColor

            ' Slice 0086: «Angajament nou» is the header's primary action.
            ButtonStyles.ApplyPrimary(btnAngajamentNou, scheme)

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
    ''' The options button of the caption bar: TWO rows at most, «Arata jurnal» and «Setari...»,
    ''' each with its icon and a separator between them -- the operator's request of
    ''' 20.09.2026 («the setari button will only show the Jurnal (if enabled from setari) and
    ''' the Setari option»). The menu is a <c>CustomPopup</c>, painted by us, so it is themed
    ''' exactly like the theme menu of the same bar.
    '''
    ''' <para>«Arata jurnal» is gated by <c>FeatureSwitches.VizualizatorJurnaleActiv</c> (the
    ''' operator's own switch on the «Aplicatie» page). When it is off the menu has one row
    ''' left, and a one-row menu is a detour: the click opens the settings window directly.</para>
    '''
    ''' <para>«Sincronizare (server)» left the menu with this request; <see cref="SincronizeazaAsync"/>
    ''' stays, unreachable from the shell until the operator asks for a new home for it.</para>
    ''' </summary>
    Private Sub CapBar_OptionButtonClick(sender As Object, e As EventArgs) Handles capBar.OptionButtonClick
        Try
            ' A second click on the button CLOSES the menu: the press already closed it (it
            ' activated the window underneath), so without this guard it would reopen at once.
            If CustomPopup.ClosedJustNow Then Return

            If Not FeatureSwitches.VizualizatorJurnaleActiv Then
                SetariForm.ShowFor(Me, _setariFactory)
                Return
            End If

            Dim ancora As Rectangle = capBar.OptionButtonBounds
            If ancora.IsEmpty Then Return

            ' «&A» = the access letter, as in any system menu. The icons are the same two
            ' the settings window uses for its nav rows, so the menu and the window agree.
            Dim elemente As New List(Of CustomPopupItem) From {
                New CustomPopupItem(OPT_JURNAL, "&Arată jurnal",
                                    My.Resources.Resources.Papirus_Team_Papirus_Apps_Accessories_text_editor_512_resized),
                CustomPopupItem.Separator(),
                New CustomPopupItem(OPT_SETARI, "S&etări…", My.Resources.Resources.settings__1_)
            }

            ' NOT in «Using»: shown modeless, the popup disposes itself on close.
            Dim meniu As New CustomPopup(elemente)
            AddHandler meniu.ItemClicked, AddressOf MeniuOptiuni_ItemClicked
            meniu.ShowBelow(capBar, ancora)
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("MainForm.CapBar_OptionButtonClick", ex)
        End Try
    End Sub

    Private Sub MeniuOptiuni_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Select Case e.Item.Key
                Case OPT_JURNAL
                    ShowLog()
                Case OPT_SETARI
                    SetariForm.ShowFor(Me, _setariFactory)
                Case Else
                    ' No silent no-ops: a row added to the menu and forgotten here must show.
                    Throw New ArgumentException("Rând necunoscut în meniul de opțiuni: «" & e.Item.Key & "».")
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.MeniuOptiuni_ItemClicked", ex)
            KBotMessage.Show(Me, "Comanda nu a putut fi executată: " & ex.Message, "K-BOT",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the log viewer: since slice 0072-01 it is the «Jurnal» page of the settings
    ''' window, so the same modeless, one-instance window is shown (or brought to the front)
    ''' and switched to that page. The operator can read the log and work in the shell at the
    ''' same time, as before.
    ''' </summary>
    Private Sub ShowLog()
        SetariForm.ShowFor(Me, _setariFactory).ShowPage("jurnal")
    End Sub
End Class
