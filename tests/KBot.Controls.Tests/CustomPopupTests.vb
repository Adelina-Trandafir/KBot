Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' <c>CustomPopup</c> — meniul contextual tematizat. Tot ce se poate ține fix FĂRĂ ecran se ține
''' aici, ca la <c>KBotNavList</c>: fereastra e doar randare, iar deciziile («ce rând urmează»,
''' «ce face litera de acces», «cât de lat iese meniul», «unde încape pe ecran») stau în funcții
''' pe care testele le pot chema direct.
'''
''' Cele patru contracte apărate:
''' <list type="number">
''' <item>litera de acces urmează regula Windows — o potrivire alege, mai multe doar mută;</item>
''' <item>tastatura e drum egal cu mouse-ul și sare separatorii și rândurile dezactivate;</item>
''' <item>selecția din constructor e chiar cerința pentru care există popup-ul — o cheie
''' necunoscută ARUNCĂ, nu se deschide tăcut pe nimic;</item>
''' <item>contractul de culoare al casei: gol = din temă, explicit = pentru totdeauna.</item>
''' </list>
''' </summary>
Public Class CustomPopupTests

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

    ' Meniul de probă: două rânduri, un separator, un rând dezactivat, unul normal.
    ' Indici: 0 = &Salvează, 1 = &Deschide, 2 = separator, 3 = &Ascunde (dezactivat), 4 = &Renunță.
    Private Shared Function MeniuDeProba() As List(Of CustomPopupItem)
        Return New List(Of CustomPopupItem) From {
            New CustomPopupItem("save", "&Salvează"),
            New CustomPopupItem("open", "&Deschide"),
            CustomPopupItem.Separator(),
            New CustomPopupItem("hide", "&Ascunde") With {.Enabled = False},
            New CustomPopupItem("cancel", "&Renunță")
        }
    End Function

    ' ── Litera de acces (funcție pură — fără fir STA) ────────────────────────────

    <Theory>
    <InlineData("&Salvează", "S"c)>
    <InlineData("&salvează", "S"c)>
    <InlineData("Sal&vează", "V"c)>
    <InlineData("Salvează &1", "1"c)>
    Public Sub Litera_de_acces_e_cea_marcata(text As String, asteptat As Char)
        Assert.Equal(asteptat, PopupMnemonic.Extract(text))
    End Sub

    <Theory>
    <InlineData("Salvează")>
    <InlineData("Profit && pierdere")>
    <InlineData("Termină cu ampersand &")>
    <InlineData("")>
    Public Sub Fara_marcaj_valid_nu_exista_litera_de_acces(text As String)
        Assert.Equal(PopupMnemonic.None, PopupMnemonic.Extract(text))
    End Sub

    <Fact>
    Public Sub Dublul_ampersand_e_literal_iar_marcajul_de_dupa_el_conteaza()
        ' «&&» nu marchează nimic, dar nu oprește căutarea: al doilea «&» e marcajul adevărat.
        Assert.Equal("P"c, PopupMnemonic.Extract("Profit && &pierdere"))
        Assert.Equal("Profit & pierdere", PopupMnemonic.Strip("Profit && &pierdere"))
    End Sub

    <Fact>
    Public Sub Textul_curat_pierde_marcajul_dar_pastreaza_ampersandul_literal()
        Assert.Equal("Salvează", PopupMnemonic.Strip("&Salvează"))
        Assert.Equal("A & B", PopupMnemonic.Strip("A && B"))
        Assert.Equal("fără ampersand", PopupMnemonic.Strip("fără ampersand"))
    End Sub

    <Theory>
    <InlineData("A"c, True)>
    <InlineData("z"c, True)>
    <InlineData("7"c, True)>
    <InlineData("Î"c, False)>
    <InlineData("ș"c, False)>
    <InlineData("-"c, False)>
    Public Sub Doar_literele_ASCII_pot_fi_litere_de_acces(ch As Char, asteptat As Boolean)
        Assert.Equal(asteptat, PopupMnemonic.IsTypable(ch))
    End Sub

    <Fact>
    Public Sub Tot_ce_e_tastabil_se_intoarce_prin_KeyToChar()
        ' Cele două fețe ale aceleiași reguli: IsTypable spune ce POATE fi marcat, KeyToChar spune
        ' ce AJUNGE de la tastatură. Dacă se despart, o subliniere ar promite o tastă inexistentă.
        For code As Keys = Keys.A To Keys.Z
            Assert.True(PopupMnemonic.IsTypable(CustomPopup.KeyToChar(code)))
        Next
        For code As Keys = Keys.D0 To Keys.D9
            Assert.True(PopupMnemonic.IsTypable(CustomPopup.KeyToChar(code)))
        Next
    End Sub

    <Fact>
    Public Sub O_litera_netastabila_nu_trece_drept_litera_de_acces()
        ' «&Închide» marchează «Î» — nicio tastă. Elementul raportează cinstit «n-am literă».
        Assert.Equal(PopupMnemonic.None, New CustomPopupItem("x", "&Închide").Mnemonic)
        Assert.Equal("N"c, New CustomPopupItem("x", "Î&nchide").Mnemonic)
    End Sub

    <Fact>
    Public Sub Un_element_dezactivat_sau_separator_nu_are_litera_de_acces()
        ' Altfel tasta ar «răspunde» fără să se întâmple nimic — chiar no-op-ul tăcut interzis.
        Dim dezactivat As New CustomPopupItem("hide", "&Ascunde") With {.Enabled = False}
        Assert.Equal(PopupMnemonic.None, dezactivat.Mnemonic)
        Assert.Equal(PopupMnemonic.None, CustomPopupItem.Separator().Mnemonic)
        Assert.Equal("A"c, New CustomPopupItem("hide", "&Ascunde").Mnemonic)
    End Sub

    ' ── Selecția din constructor ─────────────────────────────────────────────────

    <Fact>
    Public Sub Constructorul_deschide_meniul_pe_cheia_ceruta()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba(), "cancel")
                       Assert.Equal(4, p.SelectedIndex)
                       Assert.Equal("cancel", p.SelectedKey)
                       Assert.Same(p.Items(4), p.SelectedItem)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Fara_cheie_meniul_se_deschide_fara_nicio_evidentiere()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.Equal(-1, p.SelectedIndex)
                       Assert.Null(p.SelectedItem)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub O_cheie_necunoscuta_arunca_nu_se_deschide_tacut_pe_nimic()
        RunSta(Sub()
                   Assert.Throws(Of ArgumentException)(
                       Function() New CustomPopup(MeniuDeProba(), "inexistent"))
               End Sub)
    End Sub

    <Fact>
    Public Sub Cheia_se_cauta_si_dupa_deschidere_iar_ContainsKey_nu_arunca()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.Same(p.Items(1), p.ItemByKey("open"))
                       Assert.True(p.ContainsKey("open"))
                       Assert.False(p.ContainsKey("inexistent"))
                       Assert.Throws(Of ArgumentException)(Function() p.ItemByKey("inexistent"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_separator_sau_un_rand_dezactivat_nu_se_pot_evidentia()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       p.SelectedIndex = 2      ' separator
                       Assert.Equal(-1, p.SelectedIndex)
                       p.SelectedIndex = 3      ' dezactivat
                       Assert.Equal(-1, p.SelectedIndex)
                       p.SelectedIndex = 99     ' în afara colecției
                       Assert.Equal(-1, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    ' ── Tastatura ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Sagetile_sar_separatorul_si_randul_dezactivat()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba(), "open")
                       p.MoveSelection(1)       ' 1 → sare 2 (separator) și 3 (dezactivat)
                       Assert.Equal(4, p.SelectedIndex)
                       p.MoveSelection(-1)
                       Assert.Equal(1, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Sagetile_se_invart_in_cerc()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba(), "cancel")
                       p.MoveSelection(1)       ' ultimul → primul
                       Assert.Equal(0, p.SelectedIndex)
                       p.MoveSelection(-1)      ' primul → ultimul
                       Assert.Equal(4, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Fara_evidentiere_jos_ia_primul_rand_iar_sus_pe_ultimul()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       p.MoveSelection(1)
                       Assert.Equal(0, p.SelectedIndex)
                   End Using
                   Using p As New CustomPopup(MeniuDeProba())
                       p.MoveSelection(-1)
                       Assert.Equal(4, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Home_si_End_prind_capetele_selectabile()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       p.SelectEdge(False)
                       Assert.Equal(4, p.SelectedIndex)
                       p.SelectEdge(True)
                       Assert.Equal(0, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tastele_se_traduc_in_litera_de_acces()
        Assert.Equal("S"c, CustomPopup.KeyToChar(Keys.S))
        Assert.Equal("1"c, CustomPopup.KeyToChar(Keys.D1))
        Assert.Equal("1"c, CustomPopup.KeyToChar(Keys.NumPad1))
        Assert.Equal(PopupMnemonic.None, CustomPopup.KeyToChar(Keys.F5))
    End Sub

    <Fact>
    Public Sub O_singura_potrivire_alege_pe_loc()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.True(p.HandleMnemonic("D"c))          ' «&Deschide»
                       Assert.NotNull(p.ClickedItem)
                       Assert.Equal("open", p.ClickedItem.Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Mai_multe_potriviri_doar_muta_evidentierea_ciclic()
        RunSta(Sub()
                   Dim elemente As New List(Of CustomPopupItem) From {
                       New CustomPopupItem("s1", "&Salvează"),
                       New CustomPopupItem("s2", "&Salvează ca…"),
                       New CustomPopupItem("q", "&Renunță")
                   }
                   Using p As New CustomPopup(elemente)
                       Assert.True(p.HandleMnemonic("S"c))
                       Assert.Equal(0, p.SelectedIndex)
                       Assert.Null(p.ClickedItem)                   ' nu a ales nimic

                       Assert.True(p.HandleMnemonic("S"c))
                       Assert.Equal(1, p.SelectedIndex)
                       Assert.Null(p.ClickedItem)

                       Assert.True(p.HandleMnemonic("S"c))          ' se învârte înapoi
                       Assert.Equal(0, p.SelectedIndex)
                       Assert.Null(p.ClickedItem)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Litera_unui_rand_dezactivat_nu_raspunde()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.False(p.HandleMnemonic("A"c))         ' «&Ascunde», dezactivat
                       Assert.Null(p.ClickedItem)
                       Assert.Equal(-1, p.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Dupa_inchidere_al_doilea_clic_pe_buton_nu_redeschide()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       p.ActivateItem(0)
                       ' Gazda întreabă exact asta la începutul handler-ului de buton.
                       Assert.True(CustomPopup.ClosedJustNow)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Sublinierile_se_aprind_la_prima_tasta()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       p.RevealMnemonics()
                       Assert.True(p.MnemonicsVisible)
                   End Using
               End Sub)
    End Sub

    ' ── Geometrie ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Banda_de_pictograme_se_rezerva_doar_daca_are_cine_s_o_umple()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.Equal(0, p.IconGutter())
                   End Using

                   Dim cuIcoana As List(Of CustomPopupItem) = MeniuDeProba()
                   cuIcoana(0).Image = New Bitmap(16, 16)
                   Using p As New CustomPopup(cuIcoana)
                       Assert.True(p.IconGutter() > 0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Pictograma_largeste_meniul_cu_exact_banda_ei()
        RunSta(Sub()
                   ' Lățimea minimă e scoasă din calcul în ambele părți: altfel amândouă meniurile
                   ' ar fi strânse la ea și proba n-ar mai măsura nimic.
                   Dim fara As Integer
                   Using p As New CustomPopup(MeniuDeProba())
                       p.MinimumPopupWidth = 0
                       fara = p.NaturalSize.Width
                   End Using

                   Dim cuIcoana As List(Of CustomPopupItem) = MeniuDeProba()
                   cuIcoana(0).Image = New Bitmap(16, 16)
                   Using p As New CustomPopup(cuIcoana)
                       p.MinimumPopupWidth = 0
                       Assert.Equal(fara + p.IconGutter(), p.NaturalSize.Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Separatorul_e_mai_scund_decat_un_rand_si_nu_e_zona_de_ales()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Assert.Equal(p.EffectiveRowHeight(), p.RowBounds(0).Height)
                       Assert.True(p.RowBounds(2).Height < p.RowBounds(0).Height)
                       ' Rândurile stau unul sub altul, fără goluri și fără suprapuneri.
                       For i As Integer = 1 To p.Items.Count - 1
                           Assert.Equal(p.RowBounds(i - 1).Bottom, p.RowBounds(i).Top)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_text_mai_lung_lateste_meniul_iar_maximul_il_opreste()
        RunSta(Sub()
                   Dim scurt As Integer
                   Using p As New CustomPopup(MeniuDeProba())
                       scurt = p.NaturalSize.Width
                   End Using

                   Dim lung As List(Of CustomPopupItem) = MeniuDeProba()
                   lung(0).Text = "&Salvează angajamentul curent în baza de date a unității"
                   Using p As New CustomPopup(lung)
                       Assert.True(p.NaturalSize.Width > scurt)
                       p.MaximumPopupWidth = 150
                       Assert.True(p.NaturalSize.Width <= 150)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Adaugarea_unui_element_recalculeaza_geometria_pe_loc()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Dim inainte As Integer = p.NaturalSize.Height
                       p.Items.Add(New CustomPopupItem("new", "&Nou"))
                       Assert.Equal(inainte + p.EffectiveRowHeight(), p.NaturalSize.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_meniu_fara_elemente_nu_se_deschide()
        RunSta(Sub()
                   Using host As New Form()
                       Using p As New CustomPopup()
                           Assert.Throws(Of InvalidOperationException)(Sub() p.ShowAt(host, Point.Empty))
                       End Using
                   End Using
               End Sub)
    End Sub

    ' ── Așezarea pe ecran (funcție pură) ─────────────────────────────────────────

    <Fact>
    Public Sub Ancorat_intr_un_punct_meniul_creste_spre_stanga_din_punct()
        ' ShowAt: alternativele sunt chiar coordonatele punctului.
        Dim wa As New Rectangle(0, 0, 1000, 800)
        Dim r As Rectangle = CustomPopup.FitToWorkArea(New Size(200, 100), New Point(950, 10), 950, 10, wa)
        Assert.Equal(750, r.X)                      ' 950 - 200
        Assert.Equal(10, r.Y)
    End Sub

    <Fact>
    Public Sub Ancorat_pe_un_buton_meniul_se_aliniaza_la_dreapta_lui_nu_langa_el()
        ' ShowBelow pe un buton de 46px lipit de marginea din dreapta: «at» e colțul lui
        ' stânga-jos, «altRight» e marginea lui dreaptă. Meniul trebuie să se termine ODATĂ cu
        ' butonul — dacă ar folosi «at.X» ca alternativă, s-ar muta cu 46px mai la stânga,
        ' adică lângă buton în loc de sub el.
        Dim wa As New Rectangle(0, 0, 1000, 800)
        Dim r As Rectangle = CustomPopup.FitToWorkArea(New Size(200, 100), New Point(954, 32), 1000, 0, wa)
        Assert.Equal(800, r.X)                      ' 1000 - 200, adică aliniat la dreapta butonului
        Assert.Equal(1000, r.Right)
        Assert.Equal(32, r.Y)
    End Sub

    <Fact>
    Public Sub Meniul_se_rastoarna_in_sus_pe_marginea_alternativa()
        ' ShowBelow: «at» e sub buton, «altBottom» e vârful butonului — răsturnat, meniul stă
        ' DEASUPRA butonului, nu peste el.
        Dim wa As New Rectangle(0, 0, 1000, 800)
        Dim r As Rectangle = CustomPopup.FitToWorkArea(New Size(200, 100), New Point(10, 780), 210, 750, wa)
        Assert.Equal(650, r.Y)                      ' 750 - 100
        Assert.Equal(10, r.X)
    End Sub

    <Fact>
    Public Sub Meniul_mai_mare_decat_ecranul_se_strange_in_zona_de_lucru()
        Dim wa As New Rectangle(100, 50, 400, 300)
        Dim r As Rectangle = CustomPopup.FitToWorkArea(New Size(900, 900), New Point(200, 100), 200, 100, wa)
        Assert.Equal(wa, r)
    End Sub

    <Fact>
    Public Sub Ancorarea_pe_un_dreptunghi_gol_arunca()
        ' Butonul desenat e ascuns => OptionButtonBounds e gol. Un meniu care s-ar deschide
        ' oricum, în colțul din stânga-sus al gazdei, e chiar no-op-ul tăcut interzis.
        RunSta(Sub()
                   Using host As New Form()
                       Using p As New CustomPopup(MeniuDeProba())
                           Assert.Throws(Of ArgumentException)(Sub() p.ShowBelow(host, Rectangle.Empty))
                       End Using
                   End Using
               End Sub)
    End Sub

    ' ── Ancora rămâne aprinsă cât e meniul deschis ───────────────────────────────

    ''' <summary>Ancoră de probă: reține câte aprinderi și stingeri a primit.</summary>
    Private NotInheritable Class AncoraDeProba
        Inherits Control
        Implements IPopupAnchor

        Public Property Deschis As Boolean
        Public Property Aprinderi As Integer
        Public Property Stingeri As Integer

        Private Sub SetPopupOpen(open As Boolean) Implements IPopupAnchor.SetPopupOpen
            Deschis = open
            If open Then Aprinderi += 1 Else Stingeri += 1
        End Sub
    End Class

    <Fact>
    Public Sub Ancora_se_aprinde_la_deschidere_si_se_stinge_pe_ORICE_drum_de_inchidere()
        ' Cele trei drumuri (rând ales, Esc, clic în afară) trec toate prin OnFormClosed, care e
        ' motivul pentru care stingerea NU e treaba gazdei — una singură uitată ar lăsa butonul
        ' aprins pentru totdeauna.
        RunSta(Sub()
                   Using host As New Form()
                       Dim ancora As New AncoraDeProba() With {.Width = 40, .Height = 40}
                       host.Controls.Add(ancora)
                       host.Show()
                       Try
                           Dim p As New CustomPopup(MeniuDeProba())
                           p.ShowBelow(ancora)
                           Assert.True(ancora.Deschis)
                           Assert.Equal(1, ancora.Aprinderi)

                           p.ActivateItem(0)                    ' rând ales
                           Assert.False(ancora.Deschis)
                           Assert.Equal(1, ancora.Stingeri)
                       Finally
                           host.Hide()
                       End Try
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Ancora_se_stinge_si_cand_meniul_e_respins()
        RunSta(Sub()
                   Using host As New Form()
                       Dim ancora As New AncoraDeProba() With {.Width = 40, .Height = 40}
                       host.Controls.Add(ancora)
                       host.Show()
                       Try
                           Dim p As New CustomPopup(MeniuDeProba())
                           p.ShowBelow(ancora)
                           Assert.True(ancora.Deschis)

                           p.CloseWith(Nothing, -1)             ' Esc / clic în afară
                           Assert.False(ancora.Deschis)
                       Finally
                           host.Hide()
                       End Try
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_control_care_nu_e_ancora_nu_deranjeaza_pe_nimeni()
        RunSta(Sub()
                   Using host As New Form()
                       Using p As New CustomPopup(MeniuDeProba())
                           ' Un Button obișnuit nu implementează IPopupAnchor — meniul trebuie
                           ' să se deschidă la fel, fără să caute pe cineva de aprins.
                           Dim btn As New Button() With {.Width = 40, .Height = 24}
                           host.Controls.Add(btn)
                           host.Show()
                           Try
                               p.ShowBelow(btn)
                               Assert.True(p.Visible)
                           Finally
                               host.Hide()
                           End Try
                       End Using
                   End Using
               End Sub)
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Culoarea_lasata_goala_urmeaza_tema()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Dim dark As ThemeScheme = BuiltInSchemes.Dark()
                       DirectCast(p, IThemedControl).ApplyTheme(dark)

                       Assert.Equal(dark.Palette.SurfaceAltColor, p.EffectiveBackColor)
                       Assert.Equal(dark.Palette.BorderColor, p.EffectiveBorderColor)
                       Assert.Equal(dark.Palette.TextColor, p.EffectiveItemForeColor)
                       Assert.Equal(dark.Palette.DisabledTextColor, p.EffectiveDisabledForeColor)
                       Assert.Equal(dark.Palette.AccentColor, p.EffectiveHighlightBackColor)
                       Assert.Equal(dark.Palette.AccentTextColor, p.EffectiveHighlightForeColor)
                       ' Fundalul ferestrei urmează culoarea efectivă, altfel colțurile rotunjite
                       ' ar lăsa să se vadă o altă culoare pe margini.
                       Assert.Equal(dark.Palette.SurfaceAltColor, p.BackColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Culoarea_pusa_explicit_supravietuieste_temei()
        RunSta(Sub()
                   Using p As New CustomPopup(MeniuDeProba())
                       Dim verde As Color = Color.FromArgb(192, 255, 192)
                       p.PopupBackColor = verde
                       p.HighlightBackColor = Color.Gainsboro

                       DirectCast(p, IThemedControl).ApplyTheme(BuiltInSchemes.Dark())

                       Assert.Equal(verde, p.EffectiveBackColor)
                       Assert.Equal(verde, p.BackColor)
                       Assert.Equal(Color.Gainsboro, p.EffectiveHighlightBackColor)
                       ' …iar ce n-a fost ales explicit s-a dus după temă.
                       Assert.Equal(BuiltInSchemes.Dark().Palette.TextColor, p.EffectiveItemForeColor)
                   End Using
               End Sub)
    End Sub

End Class
