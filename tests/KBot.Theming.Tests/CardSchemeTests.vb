Option Strict On
Imports System.Drawing
Imports System.Text.Json
Imports Xunit
Imports KBot.Theming

''' <summary>
''' The contract of slice 0049: the card slots and the card geometry.
'''
''' <para>Two things are worth proving here and nowhere else. First, that <b>Classic, Dark and
''' Colorful are untouched</b> — every new colour slot on them resolves to the slot the scheme was
''' already using, and every new geometry value is 0, which the painting code reads as «take the
''' flat path». Second, that a scheme file <b>saved before this slice</b> still loads.</para>
'''
''' <para>What these tests deliberately do NOT claim: that the Modern numbers look right. Radius
''' 14, a 10px shadow and 34px rows are judgements, and only an operator in front of the running
''' application can settle them. See the worklog.</para>
''' </summary>
Public Class CardSchemeTests

    Private Shared Function Neutral() As IEnumerable(Of ThemeScheme)
        Return New ThemeScheme() {BuiltInSchemes.Classic(), BuiltInSchemes.Dark(), BuiltInSchemes.Colorful()}
    End Function

    <Fact>
    Public Sub NeutralSchemes_PointCardSlotsAtWhatTheyAlreadyUsed()
        For Each s As ThemeScheme In Neutral()
            Dim p As ThemePalette = s.Palette
            Assert.Equal(p.SurfaceColor, p.CanvasColor)
            Assert.Equal(p.SurfaceAltColor, p.CardColor)
            Assert.Equal(p.BorderColor, p.CardBorderColor)
            Assert.Equal(p.SurfaceAltColor, p.HeaderBandColor)
            Assert.Equal(p.BorderColor, p.RowSeparatorColor)
            Assert.Equal(p.SuccessColor, p.DotOkColor)
            Assert.Equal(p.TextDimColor, p.DotIdleColor)
        Next
    End Sub

    ''' <summary>
    ''' The one slot that is not a straight alias. Before this slice KBotNavList computed its
    ''' selection fill as <c>Blend(SurfaceAlt, Accent, 0.14)</c> inline; the slot has to reproduce
    ''' that expression EXACTLY, or every neutral scheme's selected nav item shifts by a unit or
    ''' two — a change nobody asked for and nobody would spot by eye.
    ''' </summary>
    <Fact>
    Public Sub NeutralSchemes_NavSelectedBack_EqualsTheOldInlineBlend()
        For Each s As ThemeScheme In Neutral()
            Dim p As ThemePalette = s.Palette
            Dim asBefore As Color = ThemeShapes.Blend(p.SurfaceAltColor, p.AccentColor, 0.14)
            Assert.Equal(asBefore, p.NavSelectedBackColor)
        Next
    End Sub

    ''' <summary>
    ''' Geometry zero everywhere but Modern. Every consumer reads 0 as «leave it alone», so this
    ''' single assertion is what keeps the other three schemes laying out exactly as before.
    ''' </summary>
    <Fact>
    Public Sub NeutralSchemes_HaveNoCardGeometry()
        For Each s As ThemeScheme In Neutral()
            Dim st As ThemeStyleOptions = s.Style
            Assert.Equal(0, st.CardRadius)
            Assert.Equal(0, st.CardShadow)
            Assert.Equal(0, st.CardShadowOpacity)
            Assert.Equal(0, st.CardGutter)
            Assert.Equal(0, st.NavItemHeight)
            Assert.Equal(0, st.ListRowHeight)
            Assert.Equal(0, st.GridRowHeight)
            Assert.Equal(0, st.GridHeaderHeight)
            Assert.False(st.TintIcons)
        Next
    End Sub

    <Fact>
    Public Sub Modern_AsksForCards()
        Dim st As ThemeStyleOptions = BuiltInSchemes.Modern().Style
        Assert.True(st.CardRadius > 0)
        Assert.True(st.CardShadow > 0)
        Assert.InRange(st.CardShadowOpacity, 1, 100)
        Assert.True(st.CardGutter > 0)
        Assert.True(st.NavItemHeight > 0)
        Assert.True(st.ListRowHeight > 0)
        Assert.True(st.GridRowHeight > 0)
        Assert.True(st.GridHeaderHeight > 0)
        Assert.True(st.TintIcons)
    End Sub

    ''' <summary>
    ''' A scheme file written by the previous build knows nothing about any of the new keys. It has
    ''' to load, and it has to land on the neutral defaults — not throw, and not come back with a
    ''' half-built palette that the painting code would then read as «draw a card».
    ''' </summary>
    <Fact>
    Public Sub SchemeSavedBeforeThisSlice_LoadsOntoDefaults()
        Const oldJson As String = "{""Name"":""Vechi"",""IsDark"":false," &
            "``Palette``:{""Surface"":""#FAFAFA"",""SurfaceAlt"":""#FFFFFF"",""Accent"":""#185FA5""}," &
            "``Style``:{""CornerRadius"":8,""BaseFontName"":""Segoe UI""}}"
        Dim json As String = oldJson.Replace("``", """")

        Dim s As ThemeScheme = JsonSerializer.Deserialize(Of ThemeScheme)(
            json, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})

        Assert.NotNull(s)
        Assert.Equal("Vechi", s.Name)
        ' The keys it never had come back as the class defaults …
        Assert.Equal(0, s.Style.CardRadius)
        Assert.Equal(0, s.Style.CardShadow)
        Assert.Equal(0, s.Style.CardGutter)
        Assert.False(s.Style.TintIcons)
        ' … and every new colour still parses, so nothing throws on first paint.
        Assert.NotEqual(Color.Empty, s.Palette.CanvasColor)
        Assert.NotEqual(Color.Empty, s.Palette.CardColor)
        Assert.NotEqual(Color.Empty, s.Palette.RowSeparatorColor)
    End Sub

    ''' <summary>Round trip: everything new survives being written and read back.</summary>
    <Fact>
    Public Sub Modern_RoundTripsThroughJson()
        Dim before As ThemeScheme = BuiltInSchemes.Modern()
        Dim json As String = JsonSerializer.Serialize(before)
        Dim after As ThemeScheme = JsonSerializer.Deserialize(Of ThemeScheme)(json)

        Assert.Equal(before.Style.CardRadius, after.Style.CardRadius)
        Assert.Equal(before.Style.CardShadow, after.Style.CardShadow)
        Assert.Equal(before.Style.CardShadowOpacity, after.Style.CardShadowOpacity)
        Assert.Equal(before.Style.CardGutter, after.Style.CardGutter)
        Assert.Equal(before.Style.NavItemHeight, after.Style.NavItemHeight)
        Assert.Equal(before.Style.ListRowHeight, after.Style.ListRowHeight)
        Assert.Equal(before.Style.GridRowHeight, after.Style.GridRowHeight)
        Assert.Equal(before.Style.GridHeaderHeight, after.Style.GridHeaderHeight)
        Assert.Equal(before.Style.TintIcons, after.Style.TintIcons)
        Assert.Equal(before.Palette.CanvasColor, after.Palette.CanvasColor)
        Assert.Equal(before.Palette.CardColor, after.Palette.CardColor)
        Assert.Equal(before.Palette.CardBorderColor, after.Palette.CardBorderColor)
        Assert.Equal(before.Palette.ShadowColor, after.Palette.ShadowColor)
        Assert.Equal(before.Palette.NavSelectedBackColor, after.Palette.NavSelectedBackColor)
        Assert.Equal(before.Palette.DotOkColor, after.Palette.DotOkColor)
    End Sub

End Class
