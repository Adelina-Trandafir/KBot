Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Butonul de TEMĂ al barei de titlu (felia 0029): al doilea buton cu pictogramă, care își face
''' SINGUR meniul de scheme — până acum meniul îl construia MainForm, iar al doilea formular cu
''' bară de titlu ar fi trebuit să copieze o sută de rânduri.
'''
''' Contractele ținute aici:
''' <list type="number">
''' <item>UN SINGUR comutator îl aprinde (<c>ShowThemeButton</c>), iar restul (pictogramă, meniu,
''' litere de acces) vine cu el — deci niciunul dintre implicituri n-are voie să ajungă scris în
''' formularul gazdă;</item>
''' <item>stă imediat la stânga cutiei de control, iar butonul de opțiuni se mută cu un slot mai
''' la stânga când e aprins — două butoane nu pot împărți un slot;</item>
''' <item>meniul NU arată schema activă și își pierde ultimul rând («Stiluri...») când
''' <c>ShowThemeEditor</c> e stins;</item>
''' <item>butonul rămâne aprins cât e meniul lui deschis — și NU se aprinde când meniul l-a
''' desfășurat butonul de opțiuni (sinkul <c>IPopupAnchor</c> e comun pentru amândouă).</item>
''' </list>
''' </summary>
Public Class KBotCaptionBarThemeButtonTests

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

    Private Shared Function Bara() As KBotCaptionBar
        Return New KBotCaptionBar() With {
            .Width = 800,
            .Height = 40,
            .ShowMinimize = True,
            .ShowMaximize = True,
            .ShowThemeButton = True
        }
    End Function

    ' ── Vizibilitatea și geometria ───────────────────────────────────────────────

    <Fact>
    Public Sub Butonul_e_stins_implicit()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar()
                       Assert.False(bar.ShowThemeButton)
                       Assert.True(bar.ThemeButtonBounds.IsEmpty)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Butonul stă imediat la stânga cutiei de control și urcă un slot când unul se stinge.</summary>
    <Fact>
    Public Sub Butonul_sta_la_stanga_cutiei_de_control()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Dim cuTrei As Rectangle = bar.ThemeButtonBounds
                       Assert.False(cuTrei.IsEmpty)
                       Assert.True(cuTrei.Right <= bar.Width)

                       bar.ShowMinimize = False
                       Dim cuDoua As Rectangle = bar.ThemeButtonBounds
                       Assert.True(cuDoua.Left > cuTrei.Left, "fără minimizare butonul urcă un slot spre dreapta")
                       Assert.Equal(cuTrei.Width, cuDoua.Width)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cele două butoane cu pictogramă nu pot împărți un slot: tema stă lângă cutia de control,
    ''' opțiunile la stânga ei. Asta e chiar defectul pe care l-ar fi produs o a doua formulă de
    ''' sloturi scrisă separat.
    ''' </summary>
    <Fact>
    Public Sub Butonul_de_optiuni_se_da_la_stanga_celui_de_tema()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       bar.ShowOptionsButton = True
                       Dim tema As Rectangle = bar.ThemeButtonBounds
                       Dim optiuni As Rectangle = bar.OptionButtonBounds

                       Assert.False(tema.IsEmpty)
                       Assert.False(optiuni.IsEmpty)
                       Assert.Equal(tema.Left, optiuni.Right)          ' lipite, nu suprapuse
                       Assert.True(optiuni.Left < tema.Left)

                       ' Stins butonul de temă, opțiunile își iau slotul lui înapoi.
                       bar.ShowThemeButton = False
                       Assert.Equal(tema, bar.OptionButtonBounds)
                   End Using
               End Sub)
    End Sub

    ' ── Meniul ───────────────────────────────────────────────────────────────────

    ''' <summary>Schema ACTIVĂ nu intră în meniu: ar fi un rând care nu face nimic.</summary>
    <Fact>
    Public Sub Meniul_nu_arata_schema_activa()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Dim elemente = bar.ConstruiesteElementeleMeniului()
                       ' Nu sunt scheme: cele DOUĂ rânduri de unealtă («Opțiuni temă...» din felia
                       ' 0036 și «Stiluri...»), cursorul de mărime (0036-01) și comutatorul de
                       ' font (0052).
                       Dim scheme = elemente.Where(Function(i) Not i.IsSeparator AndAlso
                                                               Not i.IsSlider AndAlso
                                                               i.Key <> "@ThemeEditor" AndAlso
                                                               i.Key <> "@ThemeOptions" AndAlso
                                                               i.Key <> "@ThemeFont").ToList()

                       Assert.Equal(ThemeManager.AvailableSchemes.Count - 1, scheme.Count)
                       Assert.DoesNotContain(scheme,
                           Function(i) String.Equals(i.Key, ThemeManager.Current.Name,
                                                     StringComparison.OrdinalIgnoreCase))
                   End Using
               End Sub)
    End Sub

    ''' <summary>Fiecare rând poartă o literă de acces TASTABILĂ, deci temele se comută de la tastatură.</summary>
    <Fact>
    Public Sub Fiecare_rand_are_litera_de_acces_unica()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       ' Cursorul de mărime e sărit: n-are literă de acces prin contract — litera
                       ' ALEGE un rând, iar un cursor nu se alege, se trage (felia 0036-01).
                       Dim litere = bar.ConstruiesteElementeleMeniului().
                           Where(Function(i) Not i.IsSeparator AndAlso Not i.IsSlider).
                           Select(Function(i) i.Mnemonic).ToList()

                       Assert.DoesNotContain(PopupMnemonic.None, litere)
                       Assert.Equal(litere.Count, litere.Distinct().Count())
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' «Show Theme Editor» stinge ULTIMUL rând al meniului. Din felia 0036 separatorul NU mai
    ''' pleacă odată cu el: aparține grupului de unelte, iar «Opțiuni temă...» a rămas aprins.
    ''' </summary>
    <Fact>
    Public Sub Comutatorul_editorului_scoate_ultimul_rand()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Assert.True(bar.ShowThemeEditor, "implicit editorul e în meniu")

                       Dim cuEditor = bar.ConstruiesteElementeleMeniului()
                       Assert.True(cuEditor.Last().Text.Contains("Stiluri"))
                       Assert.True(cuEditor(cuEditor.Count - 2).Text.Contains("Opțiuni temă"))
                       Assert.True(cuEditor(cuEditor.Count - 3).IsSeparator)

                       bar.ShowThemeEditor = False
                       Dim faraEditor = bar.ConstruiesteElementeleMeniului()
                       Assert.DoesNotContain(faraEditor, Function(i) i.Text IsNot Nothing AndAlso
                                                                    i.Text.Contains("Stiluri"))
                       ' Separatorul rămâne: mai are ce despărți.
                       Assert.Contains(faraEditor, Function(i) i.IsSeparator)
                       Assert.Equal(cuEditor.Count - 1, faraEditor.Count)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cele două unelte au comutatoare SEPARATE (felia 0036): fac lucruri diferite — una schimbă
    ''' tema, cealaltă pune excepții pe controalele unei ferestre — deci o fereastră care o vrea
    ''' pe una n-are de ce s-o capete și pe cealaltă.
    ''' </summary>
    <Fact>
    Public Sub Comutatorul_optiunilor_e_separat_de_al_editorului()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Assert.True(bar.ShowThemeOptions, "implicit opțiunile sunt în meniu")

                       bar.ShowThemeOptions = False
                       Dim faraOptiuni = bar.ConstruiesteElementeleMeniului()
                       Assert.DoesNotContain(faraOptiuni, Function(i) i.Key = "@ThemeOptions")
                       Assert.Contains(faraOptiuni, Function(i) i.Key = "@ThemeEditor")
                       Assert.Contains(faraOptiuni, Function(i) i.IsSeparator)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Stins TOT ce se poate stinge, pleacă și separatorul UNELTELOR: o linie care nu mai desparte
    ''' nimic e o linie degeaba, iar meniul s-ar termina cu ea.
    '''
    ''' <para>Ce NU pleacă e rândul «Font din temă» și linia lui (felia 0052). Rândul acela e
    ''' singura cale prin care operatorul poate arăta cu degetul că fontul temei nu e cauza unei
    ''' ferestre care s-a redimensionat — un comutator care poate fi ascuns tocmai de formularul
    ''' pe care s-ar investiga problema n-ar fi bun la nimic. Deci meniul nu mai poate rămâne fără
    ''' niciun separator; poate rămâne cu exact unul.</para>
    ''' </summary>
    <Fact>
    Public Sub Fara_nicio_unealta_pleaca_separatorul_uneltelor()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Dim complet = bar.ConstruiesteElementeleMeniului()

                       bar.ShowTextScaleSlider = False
                       bar.ShowThemeOptions = False
                       bar.ShowThemeEditor = False
                       Dim doarScheme = bar.ConstruiesteElementeleMeniului()

                       ' Rămâne linia de sub «Font din temă», și doar ea.
                       ' .Where(...).Count(), nu .Count(...): pe un List(Of T), «Count» se rezolvă
                       ' la proprietatea listei, nu la extensia LINQ, iar compilatorul o respinge.
                       Assert.Equal(1, doarScheme.Where(Function(i) i.IsSeparator).Count())
                       Assert.False(doarScheme(doarScheme.Count - 1).IsSeparator,
                                    "meniul nu are voie să se termine cu o linie")
                       ' Au plecat: cursorul, separatorul uneltelor și cele două unelte.
                       Assert.Equal(complet.Count - 4, doarScheme.Count)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Fiecare rând ALEGIBIL are pictogramă: unul fără ar sări din coloana celorlalte. Cursorul de
    ''' mărime nu intră la socoteală — el nu se alege, iar coloana lui e șina, nu o glifă.
    ''' </summary>
    <Fact>
    Public Sub Fiecare_rand_are_pictograma()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       For Each i In bar.ConstruiesteElementeleMeniului()
                           If i.IsSeparator OrElse i.IsSlider Then Continue For
                           Assert.NotNull(i.Image)
                       Next
                   End Using
               End Sub)
    End Sub

    ' ── Aprinderea ───────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Sinkul <see cref="IPopupAnchor"/> e comun celor două butoane, iar interfața nu spune care
    ''' s-a desfășurat: o deschidere venită din afară (butonul de opțiuni al gazdei) aprinde
    ''' opțiunile, nu tema.
    ''' </summary>
    <Fact>
    Public Sub O_deschidere_din_afara_nu_aprinde_butonul_de_tema()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       bar.ShowOptionsButton = True
                       Dim ancora As IPopupAnchor = DirectCast(bar, IPopupAnchor)

                       ancora.SetPopupOpen(True)
                       Assert.True(bar.OptionButtonActive)
                       Assert.False(bar.ThemeButtonActive)

                       ancora.SetPopupOpen(False)
                       Assert.False(bar.OptionButtonActive)
                       Assert.False(bar.ThemeButtonActive)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Starea de aprindere e de rulare — designerul n-are voie s-o scrie în formular.</summary>
    <Fact>
    Public Sub Aprinderea_nu_se_serializeaza()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Dim pd As PropertyDescriptor =
                           TypeDescriptor.GetProperties(bar)(NameOf(KBotCaptionBar.ThemeButtonActive))
                       Assert.NotNull(pd)
                       Assert.False(pd.ShouldSerializeValue(bar))
                       Assert.False(pd.IsBrowsable)
                   End Using
               End Sub)
    End Sub

    ' ── Designer ─────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' O bară proaspăt pusă pe un formular trebuie să producă ZERO rânduri pentru butonul de
    ''' temă: implicitul înghețat în .Designer.vb se citește pentru totdeauna ca o alegere a
    ''' operatorului (vezi regula ShouldSerialize din CLAUDE.md). Calea verificată e chiar cea pe
    ''' care merge Visual Studio — <c>TypeDescriptor</c>, nu metoda proprie.
    ''' </summary>
    <Theory>
    <InlineData(NameOf(KBotCaptionBar.ShowThemeButton))>
    <InlineData(NameOf(KBotCaptionBar.ShowThemeEditor))>
    <InlineData(NameOf(KBotCaptionBar.ShowThemeOptions))>
    <InlineData(NameOf(KBotCaptionBar.ShowTextScaleSlider))>
    <InlineData(NameOf(KBotCaptionBar.ThemeButtonImage))>
    <InlineData(NameOf(KBotCaptionBar.ThemeButtonPadding))>
    <InlineData(NameOf(KBotCaptionBar.TintThemeButtonImage))>
    Public Sub Implicitele_nu_ajung_in_designer(numeProprietate As String)
        RunSta(Sub()
                   Using bar As New KBotCaptionBar()
                       Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(bar)(numeProprietate)
                       Assert.NotNull(pd)
                       Assert.False(pd.ShouldSerializeValue(bar))
                   End Using
               End Sub)
    End Sub

    ''' <summary>...dar o alegere explicită se scrie, altfel nu s-ar mai putea regla nimic.</summary>
    <Fact>
    Public Sub Alegerile_explicite_se_serializeaza()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar()
                       bar.ShowThemeButton = True
                       bar.ShowThemeEditor = False
                       bar.ThemeButtonPadding = 6
                       bar.TintThemeButtonImage = False

                       Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(bar)
                       Assert.True(props(NameOf(KBotCaptionBar.ShowThemeButton)).ShouldSerializeValue(bar))
                       Assert.True(props(NameOf(KBotCaptionBar.ShowThemeEditor)).ShouldSerializeValue(bar))
                       Assert.True(props(NameOf(KBotCaptionBar.ThemeButtonPadding)).ShouldSerializeValue(bar))
                       Assert.True(props(NameOf(KBotCaptionBar.TintThemeButtonImage)).ShouldSerializeValue(bar))

                       Using glifa As New Bitmap(16, 16)
                           bar.ThemeButtonImage = glifa
                           Assert.True(props(NameOf(KBotCaptionBar.ThemeButtonImage)).ShouldSerializeValue(bar))
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Butonul are pictogramă FĂRĂ nicio reglare — implicitul vine din resursele K-BOT, nu din
    ''' formularul gazdă. Proba fără ochi: se pictează bara și se numără pixelii din dreptunghiul
    ''' butonului care se desprind de fundal.
    ''' </summary>
    <Fact>
    Public Sub Butonul_are_pictograma_fara_nicio_reglare()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = Bara()
                       Dim scheme As ThemeScheme = BuiltInSchemes.Dark()
                       DirectCast(bar, IThemedControl).ApplyTheme(scheme)
                       Assert.Null(bar.ThemeButtonImage)   ' nimeni n-a pus nimic

                       Dim zona As Rectangle = bar.ThemeButtonBounds
                       Dim fundal As Single = scheme.Palette.SurfaceAltColor.GetBrightness()
                       Dim vizibili As Integer = 0
                       Using bmp As New Bitmap(bar.Width, bar.Height)
                           bar.DrawToBitmap(bmp, New Rectangle(0, 0, bar.Width, bar.Height))
                           For x As Integer = zona.Left To zona.Right - 1
                               For y As Integer = zona.Top To zona.Bottom - 1
                                   If Math.Abs(bmp.GetPixel(x, y).GetBrightness() - fundal) > 0.15F Then vizibili += 1
                               Next
                           Next
                       End Using
                       Assert.True(vizibili > 0, "pictograma implicită trebuie să se vadă pe schema întunecată")
                   End Using
               End Sub)
    End Sub

End Class
