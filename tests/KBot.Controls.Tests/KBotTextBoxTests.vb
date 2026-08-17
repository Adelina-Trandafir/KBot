Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Contractul lui <see cref="KBotTextBox"/>: chenarul reglabil (culoare + grosime), aerul pe care
''' îl rezervă, delegarea către caseta internă, barele proprii (când apar, când se ascund, ce
''' interval iau) și curățenia la serializare.
'''
''' Ce NU pot dovedi testele astea: derularea REALĂ prin mesajele <c>EM_*</c>. Ele răspund cinstit
''' doar cu un handle de fereastră viu și cu text care chiar depășește caseta — o probă vizuală,
''' nerulată. Aici se verifică ce se poate: intervalele puse pe bare și vizibilitatea lor.
''' </summary>
Public Class KBotTextBoxTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    ' ── Chenar ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Chenarul_ia_culoarea_temei_pana_o_pune_operatorul()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       Dim intunecata As ThemeScheme = BuiltInSchemes.Dark()
                       c.ApplyTheme(intunecata)
                       Assert.Equal(intunecata.Palette.InputBorderColor, c.BorderColor)
                       Assert.Equal(intunecata.Palette.AccentColor, c.FocusBorderColor)

                       c.BorderColor = Color.Magenta
                       c.ApplyTheme(intunecata)
                       Assert.Equal(Color.Magenta, c.BorderColor)
                       c.ResetBorderColor()
                       Assert.Equal(intunecata.Palette.InputBorderColor, c.BorderColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Grosimea_chenarului_se_prinde_la_zero_nu_merge_negativ()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.BorderWidth = -3
                       Assert.Equal(0, c.BorderWidth)
                       c.BorderWidth = 4
                       Assert.Equal(4, c.BorderWidth)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Chenarul_si_aerul_scot_zona_de_continut_din_marginile_controlului()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 100)
                       c.BorderWidth = 3
                       c.FocusBorderWidth = 3
                       c.TextPadding = 5
                       Dim zona As Rectangle = c.ContentBounds
                       Assert.Equal(8, zona.X)
                       Assert.Equal(8, zona.Y)
                       Assert.Equal(200 - 16, zona.Width)
                       Assert.Equal(100 - 16, zona.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Ingrosarea_la_focus_e_rezervata_din_start_ca_textul_sa_nu_sara()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 100)
                       c.TextPadding = 0
                       c.BorderWidth = 1
                       c.FocusBorderWidth = 3
                       ' Zona ține maximul dintre cele două grosimi, chiar dacă acum e desenată cea subțire.
                       Assert.Equal(3, c.ContentBounds.X)
                   End Using
               End Sub)
    End Sub

    ' ── Delegare către caseta internă ────────────────────────────────────────

    <Fact>
    Public Sub Textul_si_evenimentul_lui_trec_prin_cadru()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       Dim numar As Integer = 0
                       AddHandler c.TextChanged, Sub(s, e) numar += 1
                       c.Text = "primul rând"
                       Assert.Equal("primul rând", c.InnerTextBox.Text)
                       Assert.Equal(1, numar)

                       ' …și invers: ce se scrie în caseta internă se vede pe cadru.
                       c.InnerTextBox.Text = "alt text"
                       Assert.Equal("alt text", c.Text)
                       Assert.Equal(2, numar)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Caseta_interna_nu_are_bare_native()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       ' Toată povestea controlului: Windows nu desenează nicio bandă, noi le punem.
                       Assert.Equal(System.Windows.Forms.ScrollBars.None, c.InnerTextBox.ScrollBars)
                       Assert.Equal(BorderStyle.None, c.InnerTextBox.BorderStyle)
                   End Using
               End Sub)
    End Sub

    ' ── Bare ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Fara_bare_cerute_nu_apare_niciuna()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 60)
                       c.ScrollBars = System.Windows.Forms.ScrollBars.None
                       c.Text = String.Join(Environment.NewLine, Enumerable.Repeat("rând", 200))
                       c.SincronizeazaBare()
                       Assert.False(c.VerticalScrollBar.Visible)
                       Assert.False(c.HorizontalScrollBar.Visible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bara_verticala_ia_intervalul_in_LINII()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 100)
                       c.Text = String.Join(Environment.NewLine, Enumerable.Repeat("rând", 40))
                       c.SincronizeazaBare()
                       ' Fără handle, numărul de linii vine din Lines — 40 de rânduri, deci 0..39.
                       Assert.Equal(0, c.VerticalScrollBar.Minimum)
                       Assert.Equal(39, c.VerticalScrollBar.Maximum)
                       Assert.Equal(1, c.VerticalScrollBar.SmallChange)
                       Assert.True(c.VerticalScrollBar.IsScrollable)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bara_orizontala_cere_randuri_nerupte()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(120, 100)
                       c.ScrollBars = System.Windows.Forms.ScrollBars.Both
                       c.Text = New String("x"c, 400)

                       ' Cu ruperea rândurilor nu există derulare pe orizontală — nu e ce vede ochiul.
                       c.WordWrap = True
                       c.SincronizeazaBare()
                       Assert.False(c.HorizontalScrollBar.Visible)

                       c.WordWrap = False
                       c.SincronizeazaBare()
                       Assert.True(c.HorizontalScrollBar.Visible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AutoHide_stins_tine_bara_pe_ecran_si_cand_n_are_ce_derula()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 200)
                       c.Text = "un rând"
                       c.SincronizeazaBare()
                       Assert.False(c.VerticalScrollBar.Visible)

                       c.AutoHideScrollBars = False
                       Assert.True(c.VerticalScrollBar.Visible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bara_vizibila_ingusteaza_caseta_de_text()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.Size = New Size(200, 100)
                       c.ScrollBarThickness = 14
                       c.AutoHideScrollBars = False
                       Dim zona As Rectangle = c.ContentBounds
                       Assert.Equal(zona.Width - 14, c.InnerTextBox.Width)
                       Assert.Equal(zona.Right - 14, c.VerticalScrollBar.Left)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tema_ajunge_si_pe_barele_dinauntru()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       Dim intunecata As ThemeScheme = BuiltInSchemes.Dark()
                       c.ApplyTheme(intunecata)
                       ' Barele sunt copii auto-tematizați: ThemeManager nu coboară în ele decât
                       ' prin ApplyToNestedThemed, deci cadrul le dă schema el însuși.
                       Assert.Equal(intunecata.Palette.AccentColor, c.VerticalScrollBar.ThumbHoverColor)
                       Assert.Equal(intunecata.Palette.AccentColor, c.HorizontalScrollBar.ThumbHoverColor)
                       Assert.Equal(intunecata.Palette.InputBackColor, c.InnerTextBox.BackColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Fundalul_pus_de_operator_bate_tema()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       c.BackColor = Color.LightYellow
                       c.ApplyTheme(BuiltInSchemes.Dark())
                       Assert.Equal(Color.LightYellow, c.BackColor)
                       Assert.Equal(Color.LightYellow, c.InnerTextBox.BackColor)
                   End Using
               End Sub)
    End Sub

    ' ── Designer ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub O_caseta_proaspata_nu_are_nimic_de_serializat()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       For Each nume As String In New String() {"BorderColor", "FocusBorderColor",
                                                                "Font", "Size", "BackColor", "ForeColor"}
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(c)(nume)
                           Assert.False(pd.ShouldSerializeValue(c), $"«{nume}» ar fi scris în .Designer.vb")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tema_singura_nu_pinuieste_culorile_ambientale()
        RunSta(Sub()
                   Using c As New KBotTextBox()
                       ' Capcana casei: ApplyTheme SCRIE BackColor/ForeColor, iar
                       ' Control.ShouldSerializeX răspunde True de la prima scriere — de aceea
                       ' cadrul răspunde din steagul lui, nu din al bazei.
                       c.ApplyTheme(BuiltInSchemes.Dark())
                       Assert.False(TypeDescriptor.GetProperties(c)("BackColor").ShouldSerializeValue(c))
                       Assert.False(TypeDescriptor.GetProperties(c)("ForeColor").ShouldSerializeValue(c))
                   End Using
               End Sub)
    End Sub

End Class
