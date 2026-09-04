Option Strict On
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' RÂNDUL-CURSOR al meniului (felia 0036-01): al treilea rol al lui <c>CustomPopupItem</c>, după
''' rândul obișnuit și separator.
'''
''' Contractele ținute aici:
''' <list type="number">
''' <item>valoarea se LIMITEAZĂ singură, nu aruncă — sursa e o tragere de mouse, iar o excepție la
''' marginea șinei ar fi o cădere produsă de folosirea normală;</item>
''' <item>un cursor NU închide meniul și nu trece prin <c>ItemClicked</c> — altfel n-ar exista
''' previzualizare, adică chiar rostul lui;</item>
''' <item>n-are literă de acces: litera ALEGE un rând, iar un cursor nu se alege;</item>
''' <item>evenimentul se ridică O SINGURĂ DATĂ pe schimbare reală — tragerea produce zeci de
''' mesaje pe același pixel, iar gazda rescrie fonturile întregii aplicații în handler;</item>
''' <item>săgețile și Home/End mută valoarea când evidențierea e pe cursor, și meniul altfel.</item>
''' </list>
''' </summary>
Public Class CustomPopupSliderTests

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

    Private Shared Function Meniu() As CustomPopup
        Return New CustomPopup(New List(Of CustomPopupItem) From {
            CustomPopupItem.Slider("zoom", "Mărime text", 75, 200, 100),
            CustomPopupItem.Separator(),
            New CustomPopupItem("a", "&Alfa"),
            New CustomPopupItem("b", "&Beta")
        })
    End Function

    ' ── Modelul ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Valoarea_se_limiteaza_intre_capete()
        Dim it As CustomPopupItem = CustomPopupItem.Slider("z", "Zoom", 75, 200, 100)

        it.SliderValue = 500
        Assert.Equal(200, it.SliderValue)

        it.SliderValue = -20
        Assert.Equal(75, it.SliderValue)

        it.SliderValue = 125
        Assert.Equal(125, it.SliderValue)
    End Sub

    ''' <summary>Valoarea din constructor trece tot prin limitare — altfel ar intra nelimitată.</summary>
    <Fact>
    Public Sub Valoarea_din_constructor_se_limiteaza()
        Assert.Equal(200, CustomPopupItem.Slider("z", "Zoom", 75, 200, 999).SliderValue)
    End Sub

    <Fact>
    Public Sub Un_interval_intors_pe_dos_arunca()
        Assert.Throws(Of ArgumentException)(Function() CustomPopupItem.Slider("z", "Zoom", 200, 75, 100))
    End Sub

    <Theory>
    <InlineData(75, 0.0)>
    <InlineData(200, 1.0)>
    <InlineData(137, 0.496)>
    Public Sub Fractia_urmeaza_valoarea(valoare As Integer, asteptat As Double)
        Dim it As CustomPopupItem = CustomPopupItem.Slider("z", "Zoom", 75, 200, valoare)
        Assert.Equal(asteptat, it.SliderFraction, 2)
    End Sub

    ''' <summary>Un cursor n-are literă de acces — litera alege un rând, iar cursorul nu se alege.</summary>
    <Fact>
    Public Sub Cursorul_nu_are_litera_de_acces()
        Dim it As CustomPopupItem = CustomPopupItem.Slider("z", "&Mărime", 75, 200, 100)
        Assert.Equal(PopupMnemonic.None, it.Mnemonic)
    End Sub

    ' ── Comportamentul în meniu ──────────────────────────────────────────────────

    ''' <summary>
    ''' Cursorul se poate EVIDENȚIA (altfel săgețile n-ar putea ajunge la el) dar activarea lui
    ''' NU închide meniul și nu ridică <c>ItemClicked</c>.
    ''' </summary>
    <Fact>
    Public Sub Activarea_cursorului_nu_inchide_meniul()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim alese As Integer = 0
                       AddHandler m.ItemClicked, Sub() alese += 1

                       Assert.True(m.IsSliderRow(0))
                       m.ActivateItem(0)

                       Assert.Equal(0, alese)
                       Assert.Equal(0, m.SelectedIndex)   ' s-a evidențiat, atât
                       Assert.False(m.IsDisposed)
                   End Using
               End Sub)
    End Sub

    ''' <summary>…spre deosebire de un rând obișnuit, care ridică evenimentul.</summary>
    <Fact>
    Public Sub Activarea_unui_rand_obisnuit_ridica_evenimentul()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim cheie As String = Nothing
                       AddHandler m.ItemClicked, Sub(s, e) cheie = e.Item.Key
                       m.ActivateItem(2)
                       Assert.Equal("a", cheie)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Evenimentul se ridică DOAR la o schimbare reală. Tragerea produce zeci de mesaje pe același
    ''' pixel; fără garda asta, gazda ar rescrie fonturile aplicației de zeci de ori pe secundă.
    ''' </summary>
    <Fact>
    Public Sub Evenimentul_se_ridica_doar_la_schimbare_reala()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim treceri As Integer = 0
                       AddHandler m.SliderValueChanged, Sub() treceri += 1

                       m.SetSliderValue(0, 120)
                       Assert.Equal(1, treceri)

                       m.SetSliderValue(0, 120)          ' aceeași valoare
                       Assert.Equal(1, treceri)

                       m.SetSliderValue(0, 5000)         ' limitat la 200 => chiar se schimbă
                       Assert.Equal(2, treceri)

                       m.SetSliderValue(0, 9999)         ' tot 200 => nimic nou
                       Assert.Equal(2, treceri)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Evenimentul_poarta_elementul_si_pozitia()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim cheie As String = Nothing
                       Dim pozitie As Integer = -1
                       AddHandler m.SliderValueChanged,
                           Sub(s, e)
                               cheie = e.Item.Key
                               pozitie = e.Index
                           End Sub

                       m.SetSliderValue(0, 150)
                       Assert.Equal("zoom", cheie)
                       Assert.Equal(0, pozitie)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Săgețile mută valoarea cu un pas — dar numai când evidențierea e pe cursor.</summary>
    <Fact>
    Public Sub Sagetile_muta_valoarea_doar_pe_cursor()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       m.SelectedIndex = 0
                       Assert.True(m.NudgeSelectedSlider(CustomPopup.SliderKeyStep))
                       Assert.Equal(100 + CustomPopup.SliderKeyStep, m.Items(0).SliderValue)

                       ' Evidențierea pe un rând obișnuit ⇒ săgeata nu e a nimănui.
                       m.SelectedIndex = 2
                       Assert.False(m.NudgeSelectedSlider(CustomPopup.SliderKeyStep))
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un cursor DEZACTIVAT nu se mișcă — dar nici nu aruncă.</summary>
    <Fact>
    Public Sub Un_cursor_dezactivat_ramane_pe_loc()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       m.Items(0).Enabled = False
                       m.SelectedIndex = 0
                       ' IsSelectable e False ⇒ evidențierea nici nu rămâne pe el.
                       Assert.False(m.IsSelectable(0))
                   End Using
               End Sub)
    End Sub

    ' ── Predarea la sfârșitul gestului (0036-02) ─────────────────────────────────

    ''' <summary>
    ''' Tragerea ridică <c>SliderValueChanged</c> la fiecare pas (previzualizare ieftină), dar
    ''' <c>SliderValueCommitted</c> O SINGURĂ DATĂ, la ridicarea butonului. Asta e chiar defectul
    ''' raportat de operator: cu lucrul greu legat de fiecare pas, aplicația se reașeza continuu,
    ''' iar meniul se închidea singur.
    ''' </summary>
    <Fact>
    Public Sub Tragerea_preda_o_singura_data_la_ridicarea_butonului()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim pasi As Integer = 0
                       Dim predari As Integer = 0
                       AddHandler m.SliderValueChanged, Sub() pasi += 1
                       AddHandler m.SliderValueCommitted, Sub() predari += 1

                       Dim sina As Rectangle = m.SliderTrackBounds(0)
                       m.BeginSliderDrag(0, sina.Left)
                       m.UpdateSliderDrag(sina.Left + sina.Width \ 4)
                       m.UpdateSliderDrag(sina.Left + sina.Width \ 2)
                       m.UpdateSliderDrag(sina.Right)

                       Assert.True(pasi >= 2, "fiecare pas al tragerii trebuie previzualizat")
                       Assert.Equal(0, predari)

                       m.EndSliderDrag()
                       Assert.Equal(1, predari)
                       Assert.Equal(200, m.Items(0).SliderValue)
                   End Using
               End Sub)
    End Sub

    ''' <summary>O apăsare care n-a mișcat nimic NU e o comandă, deci nu se predă nimic.</summary>
    <Fact>
    Public Sub O_apasare_fara_miscare_nu_preda()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim predari As Integer = 0
                       AddHandler m.SliderValueCommitted, Sub() predari += 1

                       ' Apăsare CHIAR pe poziția valorii curente (100 din 75..200).
                       Dim sina As Rectangle = m.SliderTrackBounds(0)
                       Dim x As Integer = sina.Left + CInt(m.Items(0).SliderFraction * sina.Width)
                       m.BeginSliderDrag(0, x)
                       Dim dupaApasare As Integer = m.Items(0).SliderValue
                       m.EndSliderDrag()

                       If dupaApasare = 100 Then Assert.Equal(0, predari)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Săgețile se repetă cât ții tasta apăsată, deci gestul de tastatură se încheie la RIDICAREA
    ''' tastei — o singură predare, oricâte apăsări.
    ''' </summary>
    <Fact>
    Public Sub Sagetile_predau_o_singura_data_la_ridicarea_tastei()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim predari As Integer = 0
                       AddHandler m.SliderValueCommitted, Sub() predari += 1

                       m.SelectedIndex = 0
                       m.NudgeSelectedSlider(CustomPopup.SliderKeyStep)
                       m.NudgeSelectedSlider(CustomPopup.SliderKeyStep)
                       m.NudgeSelectedSlider(CustomPopup.SliderKeyStep)
                       Assert.Equal(0, predari)

                       m.CommitKeyboardSlider()
                       Assert.Equal(1, predari)
                       Assert.Equal(100 + 3 * CustomPopup.SliderKeyStep, m.Items(0).SliderValue)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Predarea fără niciun gest de tastatură în curs nu face nimic (și nu aruncă).</summary>
    <Fact>
    Public Sub Predarea_fara_gest_nu_face_nimic()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim predari As Integer = 0
                       AddHandler m.SliderValueCommitted, Sub() predari += 1
                       m.CommitKeyboardSlider()
                       m.CommitKeyboardSlider()
                       Assert.Equal(0, predari)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cât ține predarea, meniul NU se închide pe <c>Deactivate</c>: pierderea activării vine din
    ''' propria noastră comandă (gazda reașază toate ferestrele), nu dintr-un clic în afară. Fără
    ''' garda asta, meniul dispărea exact în clipa în care era de folos.
    ''' </summary>
    <Fact>
    Public Sub Meniul_nu_se_inchide_cand_gazda_reaseaza_ferestrele()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       ' Gazda face în handler ce face și cea reală: fură activarea.
                       AddHandler m.SliderValueCommitted,
                           Sub()
                               Assert.True(m.IsCommittingSlider)
                               m.TestDeactivate()
                           End Sub

                       Dim sina As Rectangle = m.SliderTrackBounds(0)
                       m.BeginSliderDrag(0, sina.Right)
                       m.EndSliderDrag()

                       ' Un meniu care n-a ajuns pe ecran se ÎNCHIDE prin Dispose, fără FormClosed
                       ' (vezi nota din CustomPopup.OnFormClosed) — deci ăsta e semnalul de citit.
                       Assert.False(m.IsDisposed, "meniul nu are voie să se închidă din propria comandă")
                       Assert.False(m.IsCommittingSlider, "steagul se coboară după predare")
                   End Using
               End Sub)
    End Sub

    ''' <summary>…dar o dezactivare OBIȘNUITĂ (clic în afară) închide meniul, ca la orice meniu.</summary>
    <Fact>
    Public Sub O_dezactivare_obisnuita_inchide_meniul()
        RunSta(Sub()
                   Dim m As New CustomPopup(New List(Of CustomPopupItem) From {
                                                New CustomPopupItem("a", "&Alfa")})
                   m.TestDeactivate()
                   Assert.True(m.IsDisposed)
               End Sub)
    End Sub

    ' ── Geometria ────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Meniul se croiește destul de lat cât să încapă eticheta, șina ȘI valoarea. Fără asta, un
    ''' meniu cu etichete scurte ar fi lăsat șinei aproape nimic.
    ''' </summary>
    <Fact>
    Public Sub Meniul_face_loc_sinei()
        RunSta(Sub()
                   Using cuCursor As CustomPopup = Meniu()
                       Using faraCursor As New CustomPopup(New List(Of CustomPopupItem) From {
                                                               New CustomPopupItem("a", "&Alfa"),
                                                               New CustomPopupItem("b", "&Beta")})
                           Assert.True(cuCursor.NaturalSize.Width > faraCursor.NaturalSize.Width,
                                       "rândul-cursor trebuie să lățească meniul")
                       End Using

                       Dim sina As Rectangle = cuCursor.SliderTrackBounds(0)
                       Assert.False(sina.IsEmpty)
                       Assert.True(sina.Width > 0)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un rând care nu e cursor n-are șină — răspunsul e gol, nu o excepție.</summary>
    <Fact>
    Public Sub Un_rand_obisnuit_nu_are_sina()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Assert.True(m.SliderTrackBounds(2).IsEmpty)
                       Assert.True(m.SliderTrackBounds(1).IsEmpty)   ' separator
                       Assert.True(m.SliderTrackBounds(99).IsEmpty)  ' în afara colecției
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Capetele șinei chiar se pot atinge. Contează fiindcă degetul se desenează CENTRAT pe
    ''' poziție: măsurată pe toată lățimea, valoarea maximă ar cere un X din afara șinei, deci
    ''' 200% n-ar putea fi ales niciodată cu mouse-ul.
    ''' </summary>
    <Fact>
    Public Sub Capetele_sinei_se_pot_atinge_cu_mouse_ul()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Dim sina As Rectangle = m.SliderTrackBounds(0)
                       Assert.False(sina.IsEmpty)

                       Assert.Equal(75, m.SliderValueAt(0, sina.Left))
                       Assert.Equal(200, m.SliderValueAt(0, sina.Right))
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un X mult în afara șinei se limitează la capăt, nu produce o valoare aiurea.</summary>
    <Fact>
    Public Sub Un_X_din_afara_sinei_se_limiteaza()
        RunSta(Sub()
                   Using m As CustomPopup = Meniu()
                       Assert.Equal(75, m.SliderValueAt(0, -5000))
                       Assert.Equal(200, m.SliderValueAt(0, 5000))
                   End Using
               End Sub)
    End Sub

    ' ── Meniul de temă ───────────────────────────────────────────────────────────

    ''' <summary>Cursorul de mărime stă în CAPUL meniului butonului de temă și poartă valoarea reală.</summary>
    <Fact>
    Public Sub Meniul_de_tema_incepe_cu_cursorul_de_marime()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar() With {.ShowThemeButton = True}
                       Dim elemente = bar.ConstruiesteElementeleMeniului()

                       Assert.True(elemente(0).IsSlider)
                       Assert.Equal("@TextScale", elemente(0).Key)
                       Assert.Equal(CInt(Math.Round(AppScaling.TextScale * 100)), elemente(0).SliderValue)
                       ' Felia 0052 a pus comutatorul de font între cursor și separator: amândouă
                       ' rândurile de sus reglează același lucru — fontul cu care se măsoară
                       ' fereastra — deci stau împreună, deasupra liniei care le desparte de scheme.
                       Assert.Equal("@ThemeFont", elemente(1).Key)
                       Assert.True(elemente(2).IsSeparator)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Stins comutatorul, pleacă și cursorul, și separatorul lui.</summary>
    <Fact>
    Public Sub Comutatorul_scoate_cursorul_din_meniu()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar() With {.ShowThemeButton = True}
                       bar.ShowTextScaleSlider = False
                       Dim elemente = bar.ConstruiesteElementeleMeniului()

                       Assert.DoesNotContain(elemente, Function(i) i.IsSlider)
                       Assert.False(elemente(0).IsSeparator, "meniul nu are voie să înceapă cu o linie")
                   End Using
               End Sub)
    End Sub

    ''' <summary>Niciodată doi separatori la rând, oricâte grupuri s-ar stinge.</summary>
    <Fact>
    Public Sub Nu_exista_doi_separatori_alaturati()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar() With {.ShowThemeButton = True}
                       Dim elemente = bar.ConstruiesteElementeleMeniului()
                       For i As Integer = 1 To elemente.Count - 1
                           Assert.False(elemente(i).IsSeparator AndAlso elemente(i - 1).IsSeparator)
                       Next
                   End Using
               End Sub)
    End Sub

End Class
