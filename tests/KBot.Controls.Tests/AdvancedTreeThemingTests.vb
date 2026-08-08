Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' English: colour precedence on <see cref="AdvancedTreeControl"/> — the operator's report that
''' "the colours set up in the designer are not applied".
'''
''' Two independent causes, one test class. (1) <c>ThemeManager.Traverse</c> recursed INTO the tree
''' and restyled its internal search TextBox/Label with the generic per-type rules, wiping
''' <c>SearchBoxBackColor</c> on every theme apply. Implementing <see cref="IThemedControl"/> stops
''' the traversal at the tree. (2) the shell re-pushed palette colours over the designer's.
'''
''' The contract these tests pin: a colour SET explicitly wins over the theme, forever; a colour
''' left <c>Color.Empty</c> follows the theme; and <c>ShouldSerialize*</c> tells the designer to
''' write a line only for the former — otherwise every fresh tree would freeze the light palette
''' into its .Designer.vb, which is exactly how the bug was serialised into five files.
''' </summary>
Public Class AdvancedTreeThemingTests

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

    <Fact>
    Public Sub Culoarea_din_designer_supravietuieste_temei()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Dim verde As Color = Color.FromArgb(192, 255, 192)
                       tree.SearchBoxBackColor = verde
                       tree.HeaderBackColor = Color.Gainsboro

                       DirectCast(tree, IThemedControl).ApplyTheme(BuiltInSchemes.Dark())

                       Assert.Equal(verde, tree.SearchBoxBackColor)
                       Assert.Equal(Color.Gainsboro, tree.HeaderBackColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Culoarea_lasata_goala_urmeaza_tema()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Dim dark As ThemeScheme = BuiltInSchemes.Dark()
                       DirectCast(tree, IThemedControl).ApplyTheme(dark)

                       Assert.Equal(dark.Palette.SurfaceAltColor, tree.HeaderBackColor)
                       Assert.Equal(dark.Palette.TextColor, tree.HeaderForeColor)
                       Assert.Equal(dark.Palette.InputBackColor, tree.SearchBoxBackColor)
                       Assert.Equal(dark.Palette.BorderColor, tree.LineColor)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Suprafața nodurilor e Control.BackColor. Albul pus de constructor NU e o alegere a
    ''' operatorului, deci tema are voie să-l schimbe; una pusă din afară o blochează.
    ''' </summary>
    <Fact>
    Public Sub Suprafata_nodurilor_urmeaza_tema_pana_o_fixeaza_cineva()
        RunSta(Sub()
                   Dim dark As ThemeScheme = BuiltInSchemes.Dark()
                   Using tree As New AdvancedTreeControl()
                       Assert.Equal(Color.White, tree.BackColor)          ' implicitul din ctor
                       DirectCast(tree, IThemedControl).ApplyTheme(dark)
                       Assert.Equal(dark.Palette.SurfaceAltColor, tree.BackColor)
                   End Using

                   Using fixat As New AdvancedTreeControl()
                       fixat.BackColor = Color.Ivory                      ' alegere explicită
                       DirectCast(fixat, IThemedControl).ApplyTheme(dark)
                       Assert.Equal(Color.Ivory, fixat.BackColor)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Întrebarea se pune prin <see cref="TypeDescriptor"/> — exact calea pe care o folosește
    ''' Visual Studio ca să decidă dacă scrie o linie în `.Designer.vb`. Fără suprascrierea
    ''' `ShouldSerializeBackColor`, `Control` răspunde True de îndată ce proprietatea a fost
    ''' scrisă vreodată (constructorul și `ApplyTheme` o scriu), iar culoarea din temă s-ar
    ''' scurge în designerul formularului gazdă și ar deveni «alegere» la următoarea încărcare.
    ''' </summary>
    <Fact>
    Public Sub Tema_nu_se_scurge_in_designerul_gazdei()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Assert.False(SerializeazaProprietatea(tree, "BackColor"))
                       Assert.False(SerializeazaProprietatea(tree, "ForeColor"))

                       DirectCast(tree, IThemedControl).ApplyTheme(BuiltInSchemes.Dark())

                       ' Tema a schimbat culorile, dar nu le-a transformat în alegeri.
                       Assert.False(SerializeazaProprietatea(tree, "BackColor"))
                       Assert.False(SerializeazaProprietatea(tree, "ForeColor"))

                       ' O alegere reală se serializează, și se poate anula din «Reset».
                       tree.BackColor = Color.Ivory
                       Assert.True(SerializeazaProprietatea(tree, "BackColor"))
                       tree.ResetBackColor()
                       Assert.False(SerializeazaProprietatea(tree, "BackColor"))
                   End Using
               End Sub)
    End Sub

    ' Ce ar întreba designerul înainte să scrie linia.
    Private Shared Function SerializeazaProprietatea(c As Component, nume As String) As Boolean
        Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(c)(nume)
        Assert.NotNull(pd)
        Return pd.ShouldSerializeValue(c)
    End Function

    ''' <summary>
    ''' Un arbore proaspăt, neatins, nu trebuie să producă NICIO linie de proprietate în
    ''' formularul gazdă. Lista de mai jos e exact zgomotul care se scria înainte în toate cele
    ''' cinci designere: fontul arborelui, cele trei dimensiuni de iconițe, fontul ambiant și
    ''' perechea de derulare (aceasta din urmă e `Shadows`, deci nu moștenea atributele bazei).
    ''' </summary>
    <Fact>
    Public Sub Un_arbore_neatins_nu_scrie_nimic_in_designer()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       For Each nume As String In {"TreeFont", "Font", "BackColor", "ForeColor",
                                                   "LeftIconSize", "RightIconSize", "HeaderIconSize",
                                                   "SearchClearButtonPadding", "SearchBarFont"}
                           Assert.False(SerializeazaProprietatea(tree, nume),
                                        $"«{nume}» nu ar trebui serializat pe un arbore neatins")
                       Next

                       ' Starea de derulare e ascunsă cu totul din designer.
                       For Each nume As String In {"AutoScrollMinSize", "AutoScrollPosition"}
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)(nume)
                           Assert.True(pd Is Nothing OrElse Not pd.IsBrowsable,
                                       $"«{nume}» ar trebui ascuns din grila de proprietăți")
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>Iar o alegere reală se scrie — altfel «curățenia» ar fi mers prea departe.</summary>
    <Fact>
    Public Sub O_alegere_reala_se_serializeaza()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.TreeFont = New Font("Segoe UI", 11.0F)
                       tree.LeftIconSize = New Size(24, 24)
                       tree.Font = New Font("Arial", 12.0F)

                       Assert.True(SerializeazaProprietatea(tree, "TreeFont"))
                       Assert.True(SerializeazaProprietatea(tree, "LeftIconSize"))
                       Assert.True(SerializeazaProprietatea(tree, "Font"))

                       tree.ResetTreeFont()
                       tree.ResetLeftIconSize()
                       Assert.False(SerializeazaProprietatea(tree, "TreeFont"))
                       Assert.False(SerializeazaProprietatea(tree, "LeftIconSize"))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Ce face de fapt fontul, ca nimeni să nu presupună mai mult decât e adevărat.
    '''
    ''' Constructorul atribuie «Segoe UI, 9» EXPLICIT, deci arborele **nu moștenește** fontul
    ''' ambiant al formularului — nici acum, nici înainte de felia 0027. Steagul de fixare
    ''' rezolvă exclusiv SERIALIZAREA (designerul nu mai scrie linia); nu transformă implicitul
    ''' în font moștenit. `ResetFont()` e cel care redă moștenirea. Consecință de reținut:
    ''' `ThemeManager.ApplyBaseFont` pune fontul schemei pe FORMULAR și se bazează pe moștenire,
    ''' deci arborele rămâne surd la el cât timp nu i se cheamă `ResetFont()` — fir deschis în
    ''' worklog-ul feliei, NU ceva ce testul de față pretinde că e reparat.
    ''' </summary>
    <Fact>
    Public Sub Fontul_implicit_e_explicit_iar_ResetFont_reda_mostenirea()
        RunSta(Sub()
                   Using gazda As New Form()
                       Using tree As New AdvancedTreeControl()
                           gazda.Controls.Add(tree)

                           gazda.Font = New Font("Tahoma", 14.0F)
                           Assert.Equal("Segoe UI", tree.Font.Name)    ' implicitul din ctor ține
                           Assert.False(SerializeazaProprietatea(tree, "Font"))

                           tree.ResetFont()
                           Assert.Equal("Tahoma", tree.Font.Name)      ' abia acum moștenește

                           tree.Font = New Font("Arial", 10.0F)        ' alegere explicită
                           gazda.Font = New Font("Verdana", 16.0F)
                           Assert.Equal("Arial", tree.Font.Name)
                           Assert.True(SerializeazaProprietatea(tree, "Font"))
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Fără temă aplicată (bancul de probă, calea FOREXE/VBA), «auto» trebuie să dea exact
    ''' culorile hardcodate dinainte de tematizare — altfel arborele s-ar decolora acolo.
    ''' </summary>
    <Fact>
    Public Sub Fara_tema_auto_da_culorile_istorice()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Assert.Equal(Color.FromArgb(222, 222, 222), tree.HeaderBackColor)
                       Assert.Equal(Color.FromArgb(50, 50, 60), tree.HeaderForeColor)
                       Assert.Equal(Color.FromArgb(222, 222, 222), tree.SearchBackColor)
                       Assert.Equal(Color.FromArgb(230, 240, 255), tree.HoverBackColor)
                       Assert.Equal(Color.FromArgb(200, 220, 255), tree.SelectedBackColor)
                       Assert.Equal(Color.FromArgb(160, 160, 160), tree.LineColor)
                       Assert.Equal(Color.Transparent, tree.BorderColor)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Designerul serializează o culoare DOAR dacă a fost aleasă. Fără asta, VS scrie implicitul
    ''' rezolvat în .Designer.vb, iar acela devine «alegere» — cum s-a și întâmplat.
    ''' </summary>
    <Fact>
    Public Sub ShouldSerialize_e_False_pana_la_o_alegere_reala()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Assert.False(tree.ShouldSerializeHeaderBackColor())
                       Assert.False(tree.ShouldSerializeSearchBoxBackColor())
                       Assert.False(tree.ShouldSerializeLineColor())

                       tree.HeaderBackColor = Color.Red
                       Assert.True(tree.ShouldSerializeHeaderBackColor())

                       tree.ResetHeaderBackColor()
                       Assert.False(tree.ShouldSerializeHeaderBackColor())
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cauza structurală: traversarea temei nu mai are voie să intre în copiii arborelui.
    ''' <c>ThemeManager.Apply</c> pe formularul gazdă trebuie să lase caseta de căutare verde.
    ''' </summary>
    ''' <summary>
    ''' Deliberat FĂRĂ <c>ThemeManager.SetScheme</c>: acela difuzează schema peste
    ''' <c>Application.OpenForms</c> — adică peste formularele altor teste — și mută stare
    ''' statică globală. <c>Apply</c> pe formularul propriu traversează exact ce ne interesează.
    ''' </summary>
    <Fact>
    Public Sub ThemeManager_nu_mai_repicteaza_caseta_de_cautare()
        RunSta(Sub()
                   Using gazda As New Form()
                       Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                           gazda.Controls.Add(tree)
                           Dim verde As Color = Color.FromArgb(192, 255, 192)
                           tree.SearchBoxBackColor = verde
                           tree.SearchShow = True

                           Dim caseta As TextBox = tree.Controls.OfType(Of TextBox)().Single()
                           Assert.Equal(verde, caseta.BackColor)

                           ThemeManager.Apply(gazda)

                           Assert.Equal(verde, caseta.BackColor)
                           Assert.Equal(verde, tree.SearchBoxBackColor)
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Mecanismul din spatele testului de mai sus: <c>Traverse</c> se oprește la controalele
    ''' care implementează <see cref="IThemedControl"/>. Fără asta, regulile generice pe tip ar
    ''' repicta TextBox-ul și eticheta interne ale benzii de căutare.
    ''' </summary>
    <Fact>
    Public Sub Arborele_implementeaza_IThemedControl()
        Assert.True(GetType(IThemedControl).IsAssignableFrom(GetType(AdvancedTreeControl)))
    End Sub

    ''' <summary>
    ''' Capătul implicit al degradeului: spre alb dacă baza e deschisă, spre negru dacă e închisă
    ''' — «alb pe temă luminoasă, negru pe temă întunecată», dedus din luminanță.
    ''' </summary>
    <Fact>
    Public Sub Degradeul_automat_merge_spre_alb_sau_negru_dupa_luminanta()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.HeaderBackColor = Color.FromArgb(230, 230, 230)      ' bază deschisă
                       Dim capatDeschis As Color = tree.HeaderGradientEndColor
                       Assert.True(AdvancedTreeControl.Luminance(capatDeschis) >
                                   AdvancedTreeControl.Luminance(tree.HeaderBackColor))

                       tree.HeaderBackColor = Color.FromArgb(32, 32, 36)         ' bază închisă
                       Dim capatInchis As Color = tree.HeaderGradientEndColor
                       Assert.True(AdvancedTreeControl.Luminance(capatInchis) <
                                   AdvancedTreeControl.Luminance(tree.HeaderBackColor))

                       ' O alegere explicită bate calculul automat.
                       tree.HeaderGradientEndColor = Color.Magenta
                       Assert.Equal(Color.Magenta, tree.HeaderGradientEndColor)
                   End Using
               End Sub)
    End Sub
End Class
