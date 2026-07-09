Imports System.Drawing
Imports KBot.Theming
Imports Xunit

Public Class ColorHexTests

    <Theory>
    <InlineData(28, 28, 28)>
    <InlineData(0, 122, 204)>
    <InlineData(255, 255, 255)>
    <InlineData(210, 210, 210)>
    Public Sub RoundTrip_PreservesRgb(r As Integer, g As Integer, b As Integer)
        Dim original As Color = Color.FromArgb(r, g, b)
        Dim hex As String = ColorHex.ToHex(original)
        Dim back As Color = ColorHex.FromHex(hex)
        Assert.Equal(original.R, back.R)
        Assert.Equal(original.G, back.G)
        Assert.Equal(original.B, back.B)
    End Sub

    <Fact>
    Public Sub ToHex_ProducesUppercaseHashPrefixed()
        Assert.Equal("#007ACC", ColorHex.ToHex(Color.FromArgb(0, 122, 204)))
    End Sub

    <Fact>
    Public Sub FromHex_AcceptsWithAndWithoutHash()
        Assert.Equal(ColorHex.FromHex("#1C1C1C").ToArgb(), ColorHex.FromHex("1C1C1C").ToArgb())
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("   ")>
    <InlineData("#12345")>
    <InlineData("#GGGGGG")>
    <InlineData("#1234567")>
    <InlineData("notacolor")>
    Public Sub FromHex_InvalidThrows(bad As String)
        Assert.Throws(Of FormatException)(Function() ColorHex.FromHex(bad))
    End Sub

End Class
