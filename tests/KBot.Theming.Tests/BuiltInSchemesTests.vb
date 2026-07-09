Imports System.Drawing
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

    <Fact>
    Public Sub Modern_HasRoundedOwnerDrawnButtons_AndVariableFont()
        Dim s = BuiltInSchemes.Modern()
        Assert.False(s.IsDark)
        Assert.False(s.Style.UseSystemColors)
        Assert.True(s.Style.CornerRadius > 0)
        Assert.Equal(ButtonRenderStyle.ModernOwnerDrawn, s.Style.ButtonRender)
        Assert.Equal("Segoe UI Variable Text", s.Style.BaseFontName)
        Assert.True(s.Style.FocusAccent)
    End Sub

    <Fact>
    Public Sub All_ReturnsThreeDistinctSchemes()
        Dim all = BuiltInSchemes.All()
        Assert.Equal(3, all.Count)
        Assert.Equal(BuiltInSchemes.ClassicName, all(0).Name)
        Assert.Equal(BuiltInSchemes.DarkName, all(1).Name)
        Assert.Equal(BuiltInSchemes.ModernName, all(2).Name)
    End Sub

End Class
