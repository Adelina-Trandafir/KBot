Option Strict On
Imports KBot.Controls
Imports Xunit

' Slice 0078-12: which side of 2024 the installed Adobe is on, from its file version. The numbers
' are the real ones: the operator's Acrobat DC (19.12.20035.332343) and the 26.1 line of the bench.
Public Class AdobeProductInfoTests

    <Theory>
    <InlineData("19.12.20035.332343", 19)>
    <InlineData("24.2.20999.0", 24)>
    <InlineData("26.1.21771.0", 26)>
    <InlineData("7", 7)>
    <InlineData(" 20.005.30000 ", 20)>
    <InlineData("", 0)>
    <InlineData(Nothing, 0)>
    <InlineData("abc.1.2", 0)>
    <InlineData("0.1.2", 0)>
    <InlineData(".5", 0)>
    Public Sub ParseMajor_ReadsTheNumberBeforeTheFirstDot(k_version As String, k_expected As Integer)
        Assert.Equal(k_expected, AdobeProductInfo.ParseMajor(k_version))
    End Sub

    <Theory>
    <InlineData("19.12.20035.332343", True)>
    <InlineData("20.5.30000.0", True)>
    <InlineData("23.8.20533.0", True)>
    <InlineData("24.1.20604.0", False)>
    <InlineData("26.1.21771.0", False)>
    Public Sub IsBefore2024_DrawsTheLineAtMajor24(k_version As String, k_expected As Boolean)
        Dim info As New AdobeProductInfo("C:\Adobe\Acrobat.exe", k_version, "Adobe Acrobat")
        Assert.True(info.IsKnown)
        Assert.Equal(k_expected, info.IsBefore2024)
    End Sub

    <Fact>
    Public Sub UnknownVersion_IsNeitherOldNorNew()
        Dim info As New AdobeProductInfo("C:\Adobe\Acrobat.exe", "", "")
        Assert.False(info.IsKnown)
        Assert.False(info.IsBefore2024)
        Assert.Contains("nu a putut fi citită", info.Describe())
    End Sub

    <Fact>
    Public Sub NoAdobe_SaysNoneIsInstalled()
        Dim info As New AdobeProductInfo(Nothing, Nothing, Nothing)
        Assert.False(info.IsKnown)
        Assert.Equal("", info.ExeName)
        Assert.Contains("niciun produs Adobe", info.Describe())
    End Sub

    <Fact>
    Public Sub Describe_NamesTheExeTheVersionAndTheSide()
        Dim old As New AdobeProductInfo("C:\Program Files (x86)\Adobe\Acrobat DC\Acrobat\Acrobat.exe", "19.12.20035.332343", "Adobe Acrobat")
        Assert.Equal("Acrobat.exe", old.ExeName)
        Assert.Contains("Acrobat.exe 19.12.20035.332343", old.Describe())
        Assert.Contains("ANTERIOR lui 2024", old.Describe())

        Dim current As New AdobeProductInfo("C:\Adobe\Reader\AcroRd32.exe", "26.1.21771.0", "Adobe Acrobat Reader")
        Assert.Contains("2024 sau mai nouă", current.Describe())
    End Sub

    <Fact>
    Public Sub Read_OfAMissingFile_IsUnknown_AndDoesNotThrow()
        Dim info As AdobeProductInfo = AdobeProductInfo.Read("C:\this\does\not\exist\Acrobat.exe")
        Assert.False(info.IsKnown)
    End Sub

End Class
