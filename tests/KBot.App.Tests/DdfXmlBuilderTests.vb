Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq
Imports Xunit
Imports KBot.Domain
Imports KBot.App

' Pure-logic tests for DdfXmlBuilder (slice 0020-05). This is the ONE fully-verifiable part of
' the generation pass (the XfaWriter call itself needs the live machete + Adobe). Every
' assertion below pins a fidelity detail read out of mdl_FX_DDF_PDF, so a future "cleanup" that
' breaks the port fails a test.
'
' NB (recorded in the worklog): the repo's ddf_demo.xml is byte-identical to ord_demo.xml, so
' the element structure was taken from GenereazaXML_PentruPython / GenereazaXML_NOTAFD, not from
' that file. These tests are structural (parse back + assert), not a byte diff.
Public Class DdfXmlBuilderTests

    Private Shared Function Ctx() As DdfXmlBuilder.Context
        Return New DdfXmlBuilder.Context() With {
            .NumeUnitate = "Test Institutie", .CodFiscal = "21015411", .CodProgram = "0000002510"}
    End Function

    Private Shared Function Antet() As DdfAntet
        Return New DdfAntet() With {
            .Iddf = 1, .CodAngajament = "AAB2EF2MCP4", .Cual = 4, .Comp = "secretariat",
            .ObiectDDF = "ISJ 2025 - Burse", .PartAng = False}
    End Function

    Private Shared Function Revizie() As RevizieRow
        Return New RevizieRow() With {
            .Idrev = 44, .Iddf = 1, .NumarRev = 0, .DataRev = New Date(2026, 1, 18),
            .DescScurta = "Descriere scurtă", .DescLunga = "Descriere lungă"}
    End Function

    Private Shared Function Linii() As List(Of LinieSaRow)
        Return New List(Of LinieSaRow) From {
            New LinieSaRow() With {
                .Idrev = 44, .Clsf = "65.01.04.02.10.01.01", .SS = "01A",
                .ElementFund = "Salarii", .ParametriiFund = "param", .ValPrec = 0.0, .ValCur = 918446.0},
            New LinieSaRow() With {
                .Idrev = 44, .Clsf = "65.01.04.02.10.01.05", .SS = "01A",
                .ElementFund = "Sporuri", .ParametriiFund = "", .ValPrec = 1002.0, .ValCur = -50.5}}
    End Function

    Private Shared Function SbRows() As List(Of SectiuneBRow)
        Return New List(Of SectiuneBRow) From {
            New SectiuneBRow() With {
                .Idrev = 44, .CodAngajament = "AAB2EF2MCP4", .CodIndicator = "AAB", .CodSSI = "01A",
                .CaAnterior = 1000.0, .Inf1 = 200.0, .CbAnterior = 3000.0, .Inf2 = 400.0}}
    End Function

    Private Shared Function ParseForm(xml As String) As XElement
        Return XDocument.Parse(xml).Root
    End Function

    ' ── form1 ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Form_HasDeclaration_AndForm1Root()
        Dim xml = DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())
        Assert.StartsWith("<?xml version=""1.0"" encoding=""UTF-8""?>", xml)
        Assert.Equal("form1", ParseForm(xml).Name.LocalName)
    End Sub

    <Fact>
    Public Sub Form_Antet_MapsGlobalsAndHeader()
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim antetNode = root.Element("SubformAntet")
        Assert.Equal("TEST INSTITUTIE", antetNode.Element("DenInstPb").Value)   ' UCase
        Assert.Equal("21015411", antetNode.Element("cif").Value)
        Assert.Equal("4", antetNode.Element("NrUnicInreg").Value)              ' CUAL
        Assert.Equal("ISJ 2025 - Burse", antetNode.Element("SubtitluDF").Value)
        Assert.Equal("2026-01-18", antetNode.Element("DataRevizuirii").Value)  ' yyyy-MM-dd
        Assert.Equal("0", antetNode.Element("Revizuirea").Value)
        Assert.Equal("A1.0.07", antetNode.Element("universalCode").Value)
    End Sub

    <Fact>
    Public Sub Form_Table1_HasDummyRowFirst_ThenRealRows()
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim table1 = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1")
        Dim rows = table1.Elements("Row1").ToList()
        Assert.Equal(3, rows.Count)                 ' 1 fictiv + 2 reale
        ' Rândul fictiv are DOAR Cell1 (gol), fără Cell6.
        Assert.Null(rows(0).Element("Cell6"))
        Assert.Equal("Salarii", rows(1).Element("Cell1").Value)
    End Sub

    <Fact>
    Public Sub Form_Cell3_IsSsPlusFirstTwoPlusFromChar7WithoutDots()
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim firstReal = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").
                            Elements("Row1").ElementAt(1)
        ' SS «01A» + Left(clsf,2) «65» + Replace(Mid(clsf,7),".","") de la «04.02.10.01.01» -> «0402100101»
        Assert.Equal("01A650402100101", firstReal.Element("Cell3").Value)
    End Sub

    <Fact>
    Public Sub Form_Cell2_IsSessionProgram_NotHeaderProgram()
        ' §2.9: programul vine din globalul de sesiune, nu din FX_DDF.Program.
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim firstReal = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").
                            Elements("Row1").ElementAt(1)
        Assert.Equal("0000002510", firstReal.Element("Cell2").Value)
    End Sub

    <Fact>
    Public Sub Form_EmptyValue_ProducesEmptyElement_NoTextNode()
        ' AdaugaSubnod omite textul când valoarea e goală. Linia 2 are ParametriiFund gol -> <Cell4/>.
        Dim xml = DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())
        Dim root = ParseForm(xml)
        Dim secondReal = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").
                             Elements("Row1").ElementAt(2)
        Assert.Equal("", secondReal.Element("Cell4").Value)
        Assert.False(secondReal.Element("Cell4").Nodes().Any())   ' fără nod-text
    End Sub

    <Fact>
    Public Sub Form_ToXmlNum_HasNoForcedDecimals()
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim firstReal = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").
                            Elements("Row1").ElementAt(1)
        Assert.Equal("918446", firstReal.Element("Cell6").Value)     ' nu «918446.00»
        Assert.Equal("0", firstReal.Element("Cell5").Value)
        ' Negativ zecimal: virgula -> punct.
        Dim secondReal = root.Element("SubformSectiuneaA").Element("Subform4").Element("Table1").
                             Elements("Row1").ElementAt(2)
        Assert.Equal("-50.5", secondReal.Element("Cell6").Value)
    End Sub

    <Fact>
    Public Sub Form_SectiuneB_Table3_SkipsCell7_UsesToXmlNum()
        Dim root = ParseForm(DdfXmlBuilder.BuildFormXml(Ctx(), Antet(), Revizie(), Linii(), SbRows()))
        Dim table3 = root.Element("SubformSectiuneaB").Element("Table3")
        Dim rows = table3.Elements("Row1").ToList()
        Assert.Equal(2, rows.Count)                 ' 1 fictiv + 1 real
        Dim real = rows(1)
        Assert.Equal("AAB2EF2MCP4", real.Element("Cell1").Value)
        Assert.Equal("AAB", real.Element("Cell2").Value)
        Assert.Equal("0000002510", real.Element("Cell3").Value)
        Assert.Equal("01A", real.Element("Cell4").Value)
        Assert.Equal("1000", real.Element("Cell5").Value)
        Assert.Equal("200", real.Element("Cell6").Value)
        Assert.Null(real.Element("Cell7"))          ' Cell7 sărit intenționat
        Assert.Equal("3000", real.Element("Cell8").Value)
        Assert.Equal("400", real.Element("Cell9").Value)
    End Sub

    ' ── NOTAFD ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Notafd_HasNamespaceRoot_AndDeclaration()
        Dim xml = DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())
        Assert.Contains("standalone=""yes""", xml)
        Dim root = XDocument.Parse(xml).Root
        Assert.Equal("NOTAFD", root.Name.LocalName)
        Assert.Equal("mfp:anaf:dgti:notafd:declaratie:v1", root.Name.NamespaceName)
    End Sub

    <Fact>
    Public Sub Notafd_RootAttributes_UseNotafdDateFormat()
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())).Root
        Assert.Equal("21015411", root.Attribute("Cif").Value)
        Assert.Equal("18.01.2026", root.Attribute("DataRevizuirii").Value)   ' dd.MM.yyyy (nu ISO)
        Assert.Equal("0", root.Attribute("Revizuirea").Value)
    End Sub

    <Fact>
    Public Sub Notafd_SectionA_Rows_UseIntTruncation_AndComputeActualiz()
        Dim ns As XNamespace = "mfp:anaf:dgti:notafd:declaratie:v1"
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())).Root
        Dim rows = root.Element(ns + "sectiuneaA").Element(ns + "ang_legale_val").
                       Elements(ns + "rowT_ang_pl_val").ToList()
        Assert.Equal(2, rows.Count)
        ' Linia 2: ValPrec 1002, ValCur -50.5 -> Int(-50.5) = -51 (floor), actualiz Int(951.5)=951.
        Dim r2 = rows(1)
        Assert.Equal("1002", r2.Attribute("valt_rev_prec").Value)
        Assert.Equal("-51", r2.Attribute("influente").Value)              ' floor, nu -50
        Assert.Equal("951", r2.Attribute("valt_actualiz").Value)          ' Int(1002 + (-50.5))
        Assert.Equal("01A", r2.Attribute("codSSI").Value)
    End Sub

    <Fact>
    Public Sub Notafd_SectionB_ComputesSums()
        Dim ns As XNamespace = "mfp:anaf:dgti:notafd:declaratie:v1"
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(), SbRows())).Root
        Dim row = root.Element(ns + "sectiuneaB").Elements(ns + "rowT_ang_ctrl_ang").First()
        ' sum_rezv_crdt_ang_act = Int(CA_Anterior + Inf1) = Int(1200) = 1200 (calculat, nu citit).
        Assert.Equal("1200", row.Attribute("sum_rezv_crdt_ang_act").Value)
        Assert.Equal("3400", row.Attribute("sum_rezv_crdt_bug_act").Value)  ' 3000 + 400
        Assert.Equal("AAB2EF2MCP4", row.Attribute("cod_angajament").Value)
    End Sub

    <Fact>
    Public Sub Notafd_LeftTruncations_AreApplied()
        ' indicator_angajament e Left(...,3): un cod indicator lung se taie la 3.
        Dim sb = New List(Of SectiuneBRow) From {
            New SectiuneBRow() With {.Idrev = 44, .CodIndicator = "ABCDEF", .CodSSI = "01A"}}
        Dim ns As XNamespace = "mfp:anaf:dgti:notafd:declaratie:v1"
        Dim root = XDocument.Parse(DdfXmlBuilder.BuildNotafdXml(Ctx(), Antet(), Revizie(), Linii(), sb)).Root
        Dim row = root.Element(ns + "sectiuneaB").Elements(ns + "rowT_ang_ctrl_ang").First()
        Assert.Equal("ABC", row.Attribute("indicator_angajament").Value)
    End Sub

    ' ── InsereazaAtasamente ───────────────────────────────────────────────────

    <Fact>
    Public Sub Attachments_EmbedsNotafdBase64_AndAttRows()
        Dim att = New List(Of AtasamentRow) From {
            New AtasamentRow() With {.Idrev = 44, .CaleFisier = "dovada.pdf", .DateFisier = "QUJD"}}
        Dim complete = DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), att)
        Dim root = ParseForm(complete)
        Dim attachments = root.Element("Attachments")
        Assert.NotNull(attachments)
        Dim items = attachments.Elements("Attachment").ToList()
        Assert.Equal(2, items.Count)                ' NOTAFD.xml + dovada.pdf
        Assert.Equal("NOTAFD.xml", items(0).Element("FileName").Value)
        ' NOTAFD.xml FileData e base64 valid care se decodează într-un XML NOTAFD.
        Dim decoded = Text.Encoding.UTF8.GetString(Convert.FromBase64String(items(0).Element("FileData").Value))
        Assert.Contains("NOTAFD", decoded)
        Assert.Equal("dovada.pdf", items(1).Element("FileName").Value)
        Assert.Equal("QUJD", items(1).Element("FileData").Value)   ' base64 brut, ne-recodat
    End Sub

    <Fact>
    Public Sub Attachments_SkipsRowsWithBlankNameOrData()
        Dim att = New List(Of AtasamentRow) From {
            New AtasamentRow() With {.Idrev = 44, .CaleFisier = "", .DateFisier = "QUJD"},
            New AtasamentRow() With {.Idrev = 44, .CaleFisier = "x.pdf", .DateFisier = ""}}
        Dim complete = DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(), SbRows(), att)
        Dim attachments = ParseForm(complete).Element("Attachments")
        ' Doar NOTAFD.xml — ambele rânduri incomplete sunt sărite.
        Assert.Single(attachments.Elements("Attachment"))
    End Sub

    <Fact>
    Public Sub EncodeBase64_RoundTripsUtf8()
        Dim s = "Descriere ăâîșț"
        Dim decoded = Text.Encoding.UTF8.GetString(Convert.FromBase64String(DdfXmlBuilder.EncodeBase64(s)))
        Assert.Equal(s, decoded)
    End Sub

    <Fact>
    Public Sub BuildComplete_NoAttachmentsOrSb_StillProducesValidForm1()
        Dim complete = DdfXmlBuilder.BuildComplete(Ctx(), Antet(), Revizie(), Linii(),
                                                   Enumerable.Empty(Of SectiuneBRow)(),
                                                   Enumerable.Empty(Of AtasamentRow)())
        Dim root = ParseForm(complete)
        Assert.Equal("form1", root.Name.LocalName)
        ' Attachments conține doar NOTAFD.xml; Table3 doar rândul fictiv.
        Assert.Single(root.Element("Attachments").Elements("Attachment"))
        Assert.Single(root.Element("SubformSectiuneaB").Element("Table3").Elements("Row1"))
    End Sub

End Class
