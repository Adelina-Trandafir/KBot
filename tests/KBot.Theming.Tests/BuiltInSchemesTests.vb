Imports System.Drawing
Imports System.Linq
Imports KBot.Theming
Imports Xunit

Public Class BuiltInSchemesTests

    <Fact>
    Public Sub Classic_UsesSystemColors_NoCustomPaint()
        Dim s = BuiltInSchemes.Classic()
        Assert.True(s.Style.UseSystemColors)
        Assert.False(s.IsDark)
        Assert.Equal(ButtonRenderStyle.System, s.Style.ButtonRender)
        Assert.Equal(0, s.Style.CornerRadius)
        Assert.False(s.Style.OwnerDrawTabs)
    End Sub

    <Fact>
    Public Sub Dark_IsDark_AndPaletteEqualsLegacyClrConstants()
        Dim p = BuiltInSchemes.Dark().Palette
        Assert.True(BuiltInSchemes.Dark().IsDark)
        ' Sloturile Dark trebuie să reproducă EXACT constantele CLR_* istorice (baseline regresie).
        Assert.Equal(Color.FromArgb(45, 45, 48).ToArgb(), p.SurfaceColor.ToArgb())      ' CLR_BG_PANEL
        Assert.Equal(Color.FromArgb(28, 28, 28).ToArgb(), p.SurfaceAltColor.ToArgb())    ' CLR_BG
        Assert.Equal(Color.FromArgb(210, 210, 210).ToArgb(), p.TextColor.ToArgb())        ' CLR_FG
        Assert.Equal(Color.FromArgb(115, 115, 115).ToArgb(), p.TextDimColor.ToArgb())     ' CLR_FG_DIM
        Assert.Equal(Color.FromArgb(62, 62, 66).ToArgb(), p.ButtonBackColor.ToArgb())     ' CLR_BTN
        Assert.Equal(Color.FromArgb(85, 85, 88).ToArgb(), p.ButtonBorderColor.ToArgb())   ' CLR_BTN_BORDER
        Assert.Equal(Color.FromArgb(75, 75, 80).ToArgb(), p.ButtonHoverColor.ToArgb())    ' CLR_BTN_HOVER
        Assert.Equal(Color.FromArgb(37, 37, 38).ToArgb(), p.TabInactiveColor.ToArgb())    ' CLR_TAB_INACTIVE
        Assert.Equal(Color.FromArgb(0, 122, 204).ToArgb(), p.TabAccentColor.ToArgb())     ' CLR_TAB_ACCENT
    End Sub

    ''' <summary>
    ''' Felia 0052 a scos «Segoe UI Variable Text» de aici. Nu era o preferință de aspect: fontul
    ''' acela se măsoară altfel decât cel cu care se proiectează în designer, iar schema îl scria
    ''' peste el la rulare, deci fiecare fereastră se redimensiona la deschidere. Fontul de bază e
    ''' acum unul singur pentru toată aplicația — <see cref="KBotFonts.BaseFontName"/> — și tocmai
    ''' de aceea se verifică față de constantă, nu față de un literal: un literal aici ar permite
    ''' schemei să se despartă tăcut de restul aplicației, adică exact defectul reparat.
    ''' </summary>
    <Fact>
    Public Sub Modern_HasRoundedOwnerDrawnButtons_AndBaseFont()
        Dim s = BuiltInSchemes.Modern()
        Assert.False(s.IsDark)
        Assert.False(s.Style.UseSystemColors)
        Assert.True(s.Style.CornerRadius > 0)
        Assert.Equal(ButtonRenderStyle.ModernOwnerDrawn, s.Style.ButtonRender)
        Assert.Equal(KBotFonts.BaseFontName, s.Style.BaseFontName)
        Assert.Equal(KBotFonts.BaseFontSize, s.Style.BaseFontSize)
        Assert.True(s.Style.FocusAccent)
    End Sub

    ''' <summary>
    ''' Colorful e schema care NU rescrie culorile. Cele trei opțiuni verificate aici sunt exact
    ''' cele care ar picta peste alegerile din designer dacă ar fi pornite — de aceea sunt parte
    ''' din contract, nu detaliu de implementare.
    ''' </summary>
    <Fact>
    Public Sub Colorful_PreservesDesignerColors_AndPaintsNothingOverThem()
        Dim s = BuiltInSchemes.Colorful()
        Assert.True(s.Style.PreserveDesignerColors)
        Assert.False(s.IsDark)
        Assert.False(s.Style.UseSystemColors)
        Assert.Equal(ButtonRenderStyle.System, s.Style.ButtonRender)   ' n-ar mai fi culoarea aleasă
        Assert.False(s.Style.FocusAccent)                              ' inelul ar picta peste input

        ' Fontul NU se mai apără printr-un 0 (felia 0052). Schema poartă acum fontul de bază ca
        ' oricare alta, iar «nu atinge fontul» vine din DRUM, nu din valoare: PreserveDesignerColors
        ' o trimite prin PreserveDesigner, care repune fontul din designer și nu ajunge niciodată
        ' la ApplyBaseFont. Steagul verificat mai sus e deci ȘI garanția pentru font.
        Assert.Equal(KBotFonts.BaseFontName, s.Style.BaseFontName)
        Assert.Equal(KBotFonts.BaseFontSize, s.Style.BaseFontSize)
    End Sub

    ''' <summary>Doar Colorful ridică steagul — celelalte trei rămân scriitoare de culori.</summary>
    <Fact>
    Public Sub OnlyColorful_SetsPreserveDesignerColors()
        Assert.False(BuiltInSchemes.Classic().Style.PreserveDesignerColors)
        Assert.False(BuiltInSchemes.Dark().Style.PreserveDesignerColors)
        Assert.False(BuiltInSchemes.Modern().Style.PreserveDesignerColors)
        Assert.True(BuiltInSchemes.Colorful().Style.PreserveDesignerColors)
    End Sub

    ''' <summary>
    ''' Eticheta e în română, CHEIA rămâne în engleză. Despărțirea nu e cosmetică: numele schemei
    ''' e persistat de <c>ThemeStore.SaveActive</c> și rezolvat înapoi de <c>ResolveByName</c>,
    ''' deci tradus la sursă ar face ca prima pornire după actualizare să nu-și mai găsească
    ''' schema salvată și să cadă pe Classic.
    ''' </summary>
    <Theory>
    <InlineData("Classic", "Clasic")>
    <InlineData("Dark", "Întunecat")>
    <InlineData("Modern", "Modern")>
    <InlineData("Colorful", "Colorat")>
    Public Sub DisplayName_TranslatesTheFourBuiltIns(key As String, expected As String)
        Assert.Equal(expected, BuiltInSchemes.DisplayName(key))
        ' …iar cheia nu s-a clintit.
        Assert.NotNull(BuiltInSchemes.All().FirstOrDefault(Function(s) s.Name = key))
    End Sub

    ''' <summary>O schemă de utilizator își păstrează numele — e ales de operator, deci deja al lui.</summary>
    <Fact>
    Public Sub DisplayName_LeavesUserSchemesAlone()
        Assert.Equal("Schema mea", BuiltInSchemes.DisplayName("Schema mea"))
        Assert.Equal(String.Empty, BuiltInSchemes.DisplayName(Nothing))
    End Sub

    <Fact>
    Public Sub All_ReturnsFourDistinctSchemes()
        Dim all = BuiltInSchemes.All()
        Assert.Equal(4, all.Count)
        Assert.Equal(BuiltInSchemes.ClassicName, all(0).Name)
        Assert.Equal(BuiltInSchemes.DarkName, all(1).Name)
        Assert.Equal(BuiltInSchemes.ModernName, all(2).Name)
        Assert.Equal(BuiltInSchemes.ColorfulName, all(3).Name)
    End Sub

End Class
