Option Strict On
Imports System
Imports System.IO
Imports System.Linq
Imports Xunit
Imports KBot.Domain
Imports KBot.App

' Pure-logic tests for DdfPdfLocator (slice 0020-04). The path convention (§2.5) and the
' "_{CodAngajament}.PDF" filter are argued from mdl_FX_DDF_PDF; these pin them against a real
' temp directory tree so GENERAL + per-partener folders, the .xml exclusion, and the name
' parsing are all exercised.
Public Class DdfPdfLocatorTests

    Private Shared Function MakeTree() As String
        ' <root>\GENERAL\...  and  <root>\TERMO_PLOIESTI\...  plus an .xml sibling and a
        ' foreign angajament's PDF that must NOT be picked up.
        Dim root As String = Path.Combine(Path.GetTempPath(), "kbot_pdf_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(Path.Combine(root, "GENERAL"))
        Directory.CreateDirectory(Path.Combine(root, "TERMO_PLOIESTI"))

        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_0_AAB2EF2MCP4.PDF"), "x")
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_2_AAB2EF2MCP4.PDF"), "xx")
        ' Sibling .xml — trebuie IGNORAT.
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_0_AAB2EF2MCP4.xml"), "<form1/>")
        ' Alt angajament — NU trebuie prins.
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_3_REV_0_AAB2HGH6FMA.PDF"), "y")
        ' Același angajament, sub folder de partener.
        File.WriteAllText(Path.Combine(root, "TERMO_PLOIESTI", "DDF_NR_1_REV_0_AAB2EF2MCP4.PDF"), "z")
        Return root
    End Function

    <Fact>
    Public Sub Enumerate_PicksOnlyMatchingCod_AcrossFolders_ExcludesXml()
        Dim root = MakeTree()
        Try
            Dim files = DdfPdfLocator.Enumerate(root, "AAB2EF2MCP4")
            ' Trei PDF-uri ale angajamentului (2 în GENERAL + 1 la partener); .xml și celălalt
            ' angajament excluse.
            Assert.Equal(3, files.Count)
            Assert.All(files, Sub(f) Assert.EndsWith("_AAB2EF2MCP4.PDF", f.FileName, StringComparison.OrdinalIgnoreCase))
            Assert.DoesNotContain(files, Function(f) f.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            Assert.Contains(files, Function(f) f.Folder = "GENERAL")
            Assert.Contains(files, Function(f) f.Folder = "TERMO_PLOIESTI")
        Finally
            Directory.Delete(root, True)
        End Try
    End Sub

    <Fact>
    Public Sub Enumerate_ParsesCualAndRev_FromFileName()
        Dim root = MakeTree()
        Try
            Dim files = DdfPdfLocator.Enumerate(root, "AAB2EF2MCP4")
            Dim atPartener = files.Single(Function(f) f.Folder = "TERMO_PLOIESTI")
            Assert.Equal(1, atPartener.Cual)
            Assert.Equal(0, atPartener.NumarRev)
            Dim rev2 = files.Single(Function(f) f.NumarRev = 2)
            Assert.Equal(4, rev2.Cual)
        Finally
            Directory.Delete(root, True)
        End Try
    End Sub

    <Fact>
    Public Sub Enumerate_OrdersByModifiedDescending()
        Dim root = MakeTree()
        Try
            ' Facem un fișier clar cel mai recent.
            Dim newest As String = Path.Combine(root, "GENERAL", "DDF_NR_4_REV_2_AAB2EF2MCP4.PDF")
            File.SetLastWriteTime(newest, DateTime.Now.AddMinutes(10))
            Dim files = DdfPdfLocator.Enumerate(root, "AAB2EF2MCP4")
            Assert.Equal(newest, files.First().FullPath)
        Finally
            Directory.Delete(root, True)
        End Try
    End Sub

    <Fact>
    Public Sub Enumerate_MissingRoot_ReturnsEmpty_DoesNotThrowOrCreate()
        Dim ghost As String = Path.Combine(Path.GetTempPath(), "kbot_ghost_" & Guid.NewGuid().ToString("N"))
        Dim files = DdfPdfLocator.Enumerate(ghost, "AAB2EF2MCP4")
        Assert.Empty(files)
        Assert.False(Directory.Exists(ghost))      ' NU a creat folderul
    End Sub

    <Fact>
    Public Sub Enumerate_BlankInputs_ReturnEmpty()
        Assert.Empty(DdfPdfLocator.Enumerate("", "COD"))
        Assert.Empty(DdfPdfLocator.Enumerate("C:\x", ""))
    End Sub

    <Theory>
    <InlineData("DDF_NR_4_REV_2_AAB2EF2MCP4.PDF", 4, 2)>
    <InlineData("DDF_NR_12_REV_0_X.pdf", 12, 0)>
    Public Sub ParseName_ExtractsCualAndRev(name As String, expCual As Integer, expRev As Integer)
        Dim cual As Integer, rev As Integer
        Assert.True(DdfPdfLocator.ParseName(name, cual, rev))
        Assert.Equal(expCual, cual)
        Assert.Equal(expRev, rev)
    End Sub

    <Fact>
    Public Sub ParseName_NonMatching_ReturnsFalse()
        Dim cual As Integer, rev As Integer
        Assert.False(DdfPdfLocator.ParseName("altceva.pdf", cual, rev))
    End Sub

    <Fact>
    Public Sub ExpectedPath_UsesPartenerFolder_WhenPartAng()
        Dim antet As New DdfAntet() With {
            .Cual = 3, .CodAngajament = "AAB2EF2MCP4", .PartAng = True, .NumePartener = "TERMO PLOIESTI"}
        Dim path = DdfPdfLocator.ExpectedPath("C:\AVACONT\FOREXE\PDF\DDF\", antet, 2)
        Assert.Equal("C:\AVACONT\FOREXE\PDF\DDF\TERMO_PLOIESTI\DDF_NR_3_REV_2_AAB2EF2MCP4.PDF", path)
    End Sub

    <Fact>
    Public Sub ExpectedPath_UsesGeneralFolder_WhenNotPartAng()
        Dim antet As New DdfAntet() With {
            .Cual = 4, .CodAngajament = "AAB2EF2MCP4", .PartAng = False, .NumePartener = ""}
        Dim path = DdfPdfLocator.ExpectedPath("C:\AVACONT\FOREXE\PDF\DDF\", antet, 0)
        Assert.Equal("C:\AVACONT\FOREXE\PDF\DDF\GENERAL\DDF_NR_4_REV_0_AAB2EF2MCP4.PDF", path)
    End Sub

    <Fact>
    Public Sub ExpectedPath_NullAntet_ReturnsNothing()
        Assert.Null(DdfPdfLocator.ExpectedPath("C:\x", Nothing, 0))
    End Sub

End Class
