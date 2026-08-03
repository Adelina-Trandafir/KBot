Option Strict On
Imports KBot.DevHarness
Imports Xunit

' Slice 0023, plan §6: the pure .reg text builder — header, dword: formatting, deletion lines,
' escaping, and the FeatureLockDown section paths for both products.
Public Class RegFileBuilderTests

    <Fact>
    Public Sub Build_StartsWithVersion5Header()
        Dim text As String = New RegFileBuilder().Build()
        Assert.StartsWith("Windows Registry Editor Version 5.00", text)
    End Sub

    <Fact>
    Public Sub AddDword_FormatsEightHexDigitsLowercase()
        Dim b As New RegFileBuilder()
        b.AddDword("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Adobe\Acrobat Reader\DC\FeatureLockDown",
                   "bAcroSuppressUpsell", 1UI)
        Dim text As String = b.Build()
        Assert.Contains("""bAcroSuppressUpsell""=dword:00000001", text)
    End Sub

    <Fact>
    Public Sub AddDword_LargeValue_StaysUnsigned()
        Dim b As New RegFileBuilder()
        b.AddDword("HKEY_LOCAL_MACHINE\X", "v", &HFFFFFFFFUI)
        Assert.Contains("""v""=dword:ffffffff", b.Build())
    End Sub

    <Fact>
    Public Sub DeleteValue_EmitsDeletionLine()
        Dim b As New RegFileBuilder()
        b.DeleteValue("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Adobe\Acrobat Reader\DC\FeatureLockDown\cServices",
                      "bToggleAdobeDocumentServices")
        Assert.Contains("""bToggleAdobeDocumentServices""=-", b.Build())
    End Sub

    <Fact>
    Public Sub AddString_EmitsQuotedAssignment()
        Dim b As New RegFileBuilder()
        b.AddString("HKEY_CURRENT_USER\X", "aDefaultRHPViewMode_L", "Collapsed")
        Assert.Contains("""aDefaultRHPViewMode_L""=""Collapsed""", b.Build())
    End Sub

    <Fact>
    Public Sub EscapesBackslashAndQuoteInNames()
        Dim b As New RegFileBuilder()
        b.AddDword("HKEY_LOCAL_MACHINE\X", "a\b""c", 0UI)
        Assert.Contains("""a\\b\""c""=dword:00000000", b.Build())
    End Sub

    <Fact>
    Public Sub SectionHeaders_AreBracketedFullPaths_ForBothProducts()
        Dim reader As String = AdobeRegistryConstants.FeatureLockDownPath(AdobeRegistryConstants.ProductReader)
        Dim acrobat As String = AdobeRegistryConstants.FeatureLockDownPath(AdobeRegistryConstants.ProductAcrobat)
        Assert.Equal("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Adobe\Acrobat Reader\DC\FeatureLockDown", reader)
        Assert.Equal("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Adobe\Adobe Acrobat\DC\FeatureLockDown", acrobat)

        Dim b As New RegFileBuilder()
        b.AddDword(reader, "bAcroSuppressUpsell", 1UI)
        b.AddDword(AdobeRegistryConstants.CServicesPath(AdobeRegistryConstants.ProductReader),
                   "bToggleAdobeDocumentServices", 1UI)
        Dim text As String = b.Build()
        Assert.Contains("[" & reader & "]", text)
        Assert.Contains("[" & reader & "\cServices]", text)
    End Sub

    <Fact>
    Public Sub SectionsKeepInsertionOrder_AndGroupTheirValues()
        Dim b As New RegFileBuilder()
        b.AddDword("HKEY_LOCAL_MACHINE\A", "v1", 1UI)
        b.AddDword("HKEY_LOCAL_MACHINE\B", "v2", 2UI)
        b.AddDword("HKEY_LOCAL_MACHINE\A", "v3", 3UI)
        Dim text As String = b.Build()
        Dim posA As Integer = text.IndexOf("[HKEY_LOCAL_MACHINE\A]", StringComparison.Ordinal)
        Dim posB As Integer = text.IndexOf("[HKEY_LOCAL_MACHINE\B]", StringComparison.Ordinal)
        Dim posV3 As Integer = text.IndexOf("""v3""", StringComparison.Ordinal)
        Assert.True(posA >= 0 AndAlso posB > posA)
        ' v3 belongs to section A even though it was added after B was opened.
        Assert.True(posV3 > posA AndAlso posV3 < posB)
    End Sub

End Class
