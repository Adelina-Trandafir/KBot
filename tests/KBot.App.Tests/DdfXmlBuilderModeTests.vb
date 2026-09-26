Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq
Imports Xunit
Imports KBot.Domain
Imports KBot.App

' Slice 0081: the two PDFs of a revision. INTERIM (0081-02) = header + section A only; FINAL =
' the whole document, and (0081-05) the FOREXE captures in SubformSectiuneaB/Subform51/Table4.
Public Class DdfXmlBuilderModeTests

    Private Shared Function Ctx() As DdfXmlBuilder.Context
        Return New DdfXmlBuilder.Context() With {
            .NumeUnitate = "Test Institutie", .CodFiscal = "21015411", .CodProgram = "0000002510"}
    End Function

    Private Shared Function Antet() As DdfAntet
        Return New DdfAntet() With {.Iddf = 1, .CodAngajament = "AAB2EF2MCP4", .Cual = 4,
                                    .Comp = "secretariat", .ObiectDDF = "Obiect"}
    End Function

    Private Shared Function Revizie() As RevizieRow
        Return New RevizieRow() With {.Idrev = 44, .Iddf = 1, .NumarRev = 0,
                                      .DataRev = New Date(2026, 9, 25), .DescScurta = "Scurt", .DescLunga = "Lung"}
    End Function

    Private Shared Function Linii() As List(Of LinieSaRow)
        Return New List(Of LinieSaRow) From {
            New LinieSaRow() With {.Idrev = 44, .Clsf = "65.04.02.20.01.01", .SS = "02A",
                                   .ElementFund = "Burse", .ValCur = 1380.0}}
    End Function

    Private Shared Function SbRows() As List(Of SectiuneBRow)
        Return New List(Of SectiuneBRow) From {
            New SectiuneBRow() With {.Idrev = 44, .CodAngajament = "AAB2EF2MCP4", .CodIndicator = "AAB",
                                     .CodSSI = "02A650402200101", .Inf1 = 1380.0, .Inf2 = 1380.0}}
    End Function

    Private Shared Function Att() As List(Of AtasamentRow)
        Return New List(Of AtasamentRow) From {
            New AtasamentRow() With {.Idrev = 44, .CaleFisier = "contract.pdf", .DateFisier = "QUJD"},
            New AtasamentRow() With {.Idrev = 44, .CaleFisier = "Poza_Final.png", .DateFisier = "iVBO", .PrtScr = True}}
    End Function

    <Fact>
    Public Sub Interim_HasNoSectionBRows_AndLeavesOption1Unticked()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), Att(),
                                                               DdfPdfMode.Interim)).Root
        Dim sectB = root.Element("SubformSectiuneaB")
        Assert.Equal("0", sectB.Element("CheckBox9").Value)
        ' Only the hidden template row.
        Assert.Single(sectB.Element("Table3").Elements("Row1"))
        ' Section A is complete.
        Assert.Equal(2, root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").Elements("Row1").Count())
    End Sub

    <Fact>
    Public Sub Interim_DropsTheForexeCaptures_KeepsTheOperatorFiles()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), Att(),
                                                               DdfPdfMode.Interim)).Root
        Dim nume = root.Element("Attachments").Elements("Attachment").Select(Function(a) a.Element("FileName").Value).ToList()
        Assert.Contains("NOTAFD.xml", nume)
        Assert.Contains("contract.pdf", nume)
        Assert.DoesNotContain("Poza_Final.png", nume)
    End Sub

    <Fact>
    Public Sub Interim_NotafdSectionB_IsEmptyAndUnticked()
        Dim notafd = XDocument.Parse(DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(),
                                                                  Enumerable.Empty(Of SectiuneBRow)(),
                                                                  sectiuneaB:=False)).Root
        Dim sectB = notafd.Elements().First(Function(e) e.Name.LocalName = "sectiuneaB")
        Assert.Equal("0", sectB.Attribute("ckbx_secta_inreg_ctrl_ang").Value)
        Assert.Empty(sectB.Elements())
    End Sub

    <Fact>
    Public Sub Final_IsTheWholeDocument()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), Att(),
                                                               DdfPdfMode.Final)).Root
        Dim sectB = root.Element("SubformSectiuneaB")
        Assert.Equal("1", sectB.Element("CheckBox9").Value)
        Assert.Equal(2, sectB.Element("Table3").Elements("Row1").Count())
    End Sub

    ' ── 0081-05: the captures in Table4 ──────────────────────────────────────

    Private Shared ReadOnly XfaNs As XNamespace = "http://www.xfa.org/schema/xfa-data/1.0/"

    <Fact>
    Public Sub Table4_NoCaptures_IsNothing()
        Assert.Null(DdfXmlBuilder.Table4Of(Nothing))
        Assert.Null(DdfXmlBuilder.Table4Of({"", "  "}))
    End Sub

    <Fact>
    Public Sub Table4_TemplateRowFirst_ThenOneImageRowPerCapture_InOrder()
        Dim t4 = DdfXmlBuilder.Table4Of({"AAAA", "BBBB"})
        Dim rows = t4.Elements("Row1").ToList()
        Assert.Equal(3, rows.Count)
        Assert.Equal(String.Empty, rows(0).Element("Cell1").Value)
        Assert.Null(rows(0).Element("Cell1").Attribute(XfaNs + "contentType"))
        Assert.Equal("AAAA", rows(1).Element("Cell1").Value)
        Assert.Equal("BBBB", rows(2).Element("Cell1").Value)
        Assert.Equal("image/png", rows(1).Element("Cell1").Attribute(XfaNs + "contentType").Value)
        Assert.Equal(String.Empty, rows(1).Element("Cell1").Attribute("href").Value)
    End Sub

    <Fact>
    Public Sub Final_WithCaptures_PutsTable4UnderSubform51_AndNotInAttachments()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), Att(),
                                                               DdfPdfMode.Final, {"iVBO"})).Root
        Dim t4 = root.Element("SubformSectiuneaB").Element("Subform51").Element("Table4")
        Assert.NotNull(t4)
        Assert.Equal(2, t4.Elements("Row1").Count())
        Dim nume = root.Element("Attachments").Elements("Attachment").Select(Function(a) a.Element("FileName").Value).ToList()
        Assert.DoesNotContain("Poza_Final.png", nume)
    End Sub

    <Fact>
    Public Sub Final_WithoutCaptures_HasNoSubform51()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), Att(),
                                                               DdfPdfMode.Final)).Root
        Assert.Null(root.Element("SubformSectiuneaB").Element("Subform51"))
    End Sub

End Class
