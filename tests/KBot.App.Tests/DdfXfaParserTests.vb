Option Strict On
Imports System.Linq
Imports Xunit
Imports KBot.App

' Pure-logic tests for DdfXfaParser (slice 0020-03). No STA / WinForms needed — the parser is
' the ONE testable part of the preview pass (window hosting and rendering can't be verified
' headless). The sample below mirrors mdl_FX_DDF_PDF.GenereazaXML_PentruPython EXACTLY: a bare
' <form1> root, SubformAntet fields, SubformSectiuneaA/Subform4/Table1 with a DUMMY first Row1
' (only Cell1, index 0, skipped) followed by real rows Cell1..Cell6.
'
' IMPORTANT context recorded here because it bit the plan: the repo's `ddf_demo.xml` is
' BYTE-IDENTICAL to `ord_demo.xml` (same MD5) — it is NOT a real DDF sample. The element set
' below was therefore derived from the Access module source, not from that file.
Public Class DdfXfaParserTests

    ' <form1> tree exactly as the DDF XML builder writes it (two real section-A lines).
    Private Const BareForm1 As String =
        "<?xml version=""1.0"" encoding=""UTF-8""?>" &
        "<form1>" &
        "  <SubformAntet>" &
        "    <DenInstPb>TEST INSTITUTIE PUBLICA</DenInstPb>" &
        "    <cif>21015411</cif>" &
        "    <NrUnicInreg>3</NrUnicInreg>" &
        "    <SubtitluDF>ISJ 2025 - Burse decembrie</SubtitluDF>" &
        "    <DataRevizuirii>2026-01-18</DataRevizuirii>" &
        "    <Revizuirea>0</Revizuirea>" &
        "    <CheckBox1>0</CheckBox1>" &
        "    <universalCode>A1.0.07</universalCode>" &
        "  </SubformAntet>" &
        "  <SubformSectiuneaA>" &
        "    <Subform123>" &
        "      <ComparimentSpecialitate>SECRETARIAT</ComparimentSpecialitate>" &
        "      <DescrieObFundRevizuireScurt>Descriere scurtă ăâîșț</DescrieObFundRevizuireScurt>" &
        "      <DescrieObFundRevizuireLung>Descriere lungă</DescrieObFundRevizuireLung>" &
        "    </Subform123>" &
        "    <Subform4>" &
        "      <Subform41><CheckBox2>1</CheckBox2></Subform41>" &
        "      <Table1>" &
        "        <Row1><Cell1 /></Row1>" &
        "        <Row1>" &
        "          <Cell1>Burse</Cell1><Cell2>0000002510</Cell2>" &
        "          <Cell3>01A6501040259010</Cell3><Cell4>parametru</Cell4>" &
        "          <Cell5>0.00</Cell5><Cell6>129705.00</Cell6>" &
        "        </Row1>" &
        "        <Row1>" &
        "          <Cell1>Salarii</Cell1><Cell2>0000002510</Cell2>" &
        "          <Cell3>01A6501040210010</Cell3><Cell4 /><Cell5>100.00</Cell5><Cell6>200.00</Cell6>" &
        "        </Row1>" &
        "      </Table1>" &
        "    </Subform4>" &
        "    <Subform5><CheckBox5>1</CheckBox5><CheckBox6>1</CheckBox6></Subform5>" &
        "  </SubformSectiuneaA>" &
        "</form1>"

    ' Same tree but wrapped as it appears when embedded in a PDF (xdp:xdp / xfa:data).
    Private Const WrappedForm1 As String =
        "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/"">" &
        "  <xfa:datasets xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/"">" &
        "    <xfa:data>" &
        "      <form1>" &
        "        <SubformAntet><DenInstPb>INST</DenInstPb><cif>123</cif></SubformAntet>" &
        "        <SubformSectiuneaA><Subform4><Table1>" &
        "          <Row1><Cell1 /></Row1>" &
        "          <Row1><Cell1>El</Cell1><Cell3>cod</Cell3><Cell5>1.00</Cell5><Cell6>2.00</Cell6></Row1>" &
        "        </Table1></Subform4></SubformSectiuneaA>" &
        "      </form1>" &
        "    </xfa:data>" &
        "  </xfa:datasets>" &
        "</xdp:xdp>"

    <Fact>
    Public Sub Parse_BareForm1_ReadsAntetFieldsInOrder()
        Dim m = DdfXfaParser.Parse(BareForm1)
        Assert.NotNull(m)
        ' Șase câmpuri cunoscute, în ordinea de afișare (CheckBox1/universalCode NU se afișează).
        Assert.Equal(6, m.AntetFields.Count)
        Assert.Equal("Instituția publică", m.AntetFields(0).Key)
        Assert.Equal("TEST INSTITUTIE PUBLICA", m.AntetFields(0).Value)
        Assert.Equal("Cod fiscal", m.AntetFields(1).Key)
        Assert.Equal("21015411", m.AntetFields(1).Value)
        Assert.Equal("Obiectul documentului", m.AntetFields(3).Key)
        Assert.Equal("Revizuirea", m.AntetFields(5).Key)
        Assert.Equal("0", m.AntetFields(5).Value)
    End Sub

    <Fact>
    Public Sub Parse_ReadsCompartimentAndDescriptions()
        Dim m = DdfXfaParser.Parse(BareForm1)
        Assert.Equal("SECRETARIAT", m.Compartiment)
        Assert.Equal("Descriere scurtă ăâîșț", m.DescScurt)      ' diacritice literale
        Assert.Equal("Descriere lungă", m.DescLung)
    End Sub

    <Fact>
    Public Sub Parse_SkipsTheDummyFirstRow_AndReadsRealLines()
        Dim m = DdfXfaParser.Parse(BareForm1)
        ' Rândul fictiv (doar Cell1 gol, fără Cell6) NU trebuie numărat.
        Assert.Equal(2, m.Linii.Count)
        Assert.Equal("Burse", m.Linii(0).ElementFund)
        Assert.Equal("01A6501040259010", m.Linii(0).Clsf)
        Assert.Equal("0.00", m.Linii(0).ValPrec)
        Assert.Equal("129705.00", m.Linii(0).ValCur)
        Assert.Equal("Salarii", m.Linii(1).ElementFund)
        Assert.Equal("200.00", m.Linii(1).ValCur)
    End Sub

    <Fact>
    Public Sub Parse_WrappedForm1_FindsForm1UnderXdpAndXfaData()
        ' Namespace-agnostic: același arbore embedat în PDF (xdp:xdp/xfa:data) se citește la fel.
        Dim m = DdfXfaParser.Parse(WrappedForm1)
        Assert.NotNull(m)
        Assert.Equal(2, m.AntetFields.Count)      ' DenInstPb + cif
        Assert.Single(m.Linii)                    ' un singur rând real (fictivul sărit)
        Assert.Equal("El", m.Linii(0).ElementFund)
        Assert.Equal("cod", m.Linii(0).Clsf)
    End Sub

    <Fact>
    Public Sub Parse_EmptyOrNull_ReturnsNothing()
        Assert.Null(DdfXfaParser.Parse(Nothing))
        Assert.Null(DdfXfaParser.Parse(""))
        Assert.Null(DdfXfaParser.Parse("   "))
    End Sub

    <Fact>
    Public Sub Parse_InvalidXml_ReturnsNothing_DoesNotThrow()
        ' Un XML rupt NU trebuie să arunce — apelantul își arată starea de eroare.
        Assert.Null(DdfXfaParser.Parse("<form1><SubformAntet>"))
    End Sub

    <Fact>
    Public Sub Parse_XmlWithoutForm1_ReturnsNothing()
        Assert.Null(DdfXfaParser.Parse("<altceva><x>1</x></altceva>"))
    End Sub

    <Fact>
    Public Sub Parse_AntetWithBlankFields_OmitsThem()
        Dim xml =
            "<form1><SubformAntet><DenInstPb>INST</DenInstPb><cif></cif>" &
            "<NrUnicInreg>  </NrUnicInreg></SubformAntet></form1>"
        Dim m = DdfXfaParser.Parse(xml)
        ' Doar câmpurile ne-goale intră; cif/NrUnicInreg (goale/spații) sunt omise.
        Assert.Single(m.AntetFields)
        Assert.Equal("Instituția publică", m.AntetFields(0).Key)
    End Sub

    <Fact>
    Public Sub Parse_NoLines_ModelIsNotGolIfAntetPresent()
        Dim xml = "<form1><SubformAntet><DenInstPb>INST</DenInstPb></SubformAntet></form1>"
        Dim m = DdfXfaParser.Parse(xml)
        Assert.False(m.EsteGol)
        Assert.Empty(m.Linii)
    End Sub

End Class
