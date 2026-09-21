Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports KBot.Domain

''' <summary>
''' XFA XML builder for the ORD (ordonantare de plata) document, ported from
''' mdl_FX_ORD_PDF: <c>GenereazaXML_ORD</c> (the "form1" tree) and
''' <c>InsereazaAtasamente_ORD</c> (the "Attachments" node). Sibling of
''' <see cref="DdfXmlBuilder"/>: pure (no I/O, no WinForms).
'''
''' Access read the tmpFX_ORD* tables filled for ONE ordonantare; here the same graph
''' arrives as an <see cref="OrdDraft"/> (GET /api/forexe/ord/draft/{idordp}). The image
''' bytes of FX_ORD_ATT no longer travel with the graph (they live in FX_ORD_ATT_IMG), so
''' the caller downloads them and hands them in as base64 keyed by IDORDATTP.
'''
''' Node ORDER is what the XFA template and XfaWriter depend on (comment block of the
''' VBA source):
'''   MainForm: SubformAntet, SubformInf[0], SubformBtnInf, SubformSemnaturaAB,
'''             SubformSemnaturaCD, SubformSemnaturaOrdonator, SubformInf[1..] (overflow)
'''   Table1:   HeaderRow1, HeaderRow2, Row1 (dummy), FooterRow, Row1... (real)
'''   Table2 / Table3: Row1 (dummy), FooterRow, Row1... (real)
'''
''' Fidelities kept from the source:
'''   * <c>ToXmlNum_ORD</c> forces two decimals with a dot ("0.00") -- unlike the DDF one;
'''   * <c>AdaugaSubnod_ORD</c> writes no text node for an empty value;
'''   * Cell3 is the session program code AS IS (no default: it cannot be missing);
'''   * the first text document goes to "DocumenteJustificative", the rest to Table3;
'''   * attachments (images) and text documents with no partner are GLOBAL: they repeat
'''     under every partner (Access: <c>IDORDPART=... OR IDORDPART IS NULL</c>);
'''   * file documents (NumeDoc set) become "Attachment" nodes once, whatever the partner.
''' </summary>
Public NotInheritable Class OrdXmlBuilder

    Private Sub New()
    End Sub

    Private Const XmlDecl As String = "<?xml version=""1.0"" encoding=""UTF-8""?>"
    Private Const UniversalCode As String = "A1.0.08"
    Private Shared ReadOnly XfaNs As XNamespace = "http://www.xfa.org/schema/xfa-data/1.0/"

    ''' <summary>Session globals the builder needs (globNumeUnit / globCF / globCodProgram).</summary>
    Public NotInheritable Class Context
        Public Property NumeUnitate As String = String.Empty
        Public Property CodFiscal As String = String.Empty
        Public Property CodProgram As String = String.Empty

        Public Shared Function FromSession(s As Global.KBot.Common.SessionContext) As Context
            If s Is Nothing Then Return New Context()
            Return New Context() With {
                .NumeUnitate = If(s.NumeUnitate, String.Empty),
                .CodFiscal = If(s.CF, String.Empty),
                .CodProgram = If(s.CodProgram, String.Empty)}
        End Function
    End Class

    ' -- Complete document ----------------------------------------------------

    ''' <summary>
    ''' Final XML (form1 + Attachments), ready to be written to disk and handed to
    ''' XfaWriter. Mirrors GenereazaPDF_ORD: GenereazaXML_ORD -> InsereazaAtasamente_ORD.
    ''' </summary>
    ''' <param name="imagini">Base64 image content keyed by IDORDATTP; a missing key
    ''' yields an empty image row, as a NULL <c>Imagine</c> did in Access.</param>
    Public Shared Function BuildComplete(ctx As Context, draft As OrdDraft,
                                         imagini As IReadOnlyDictionary(Of Integer, String)) As String
        Dim formXml As String = BuildFormXml(ctx, draft, imagini)
        Return InsertAttachments(formXml, If(draft Is Nothing, Nothing, draft.Documente))
    End Function

    ' -- form1 (GenereazaXML_ORD) ---------------------------------------------

    Public Shared Function BuildFormXml(ctx As Context, draft As OrdDraft,
                                        imagini As IReadOnlyDictionary(Of Integer, String)) As String
        If ctx Is Nothing Then ctx = New Context()
        If draft Is Nothing Then draft = New OrdDraft()

        Dim mainForm As New XElement("MainForm")

        ' SubformAntet
        Dim antet As New XElement("SubformAntet")
        AddNode(antet, "DenInstPb", UCaseSafe(ctx.NumeUnitate))
        AddNode(antet, "cif", ctx.CodFiscal)
        AddNode(antet, "NrOpl", draft.NrOrd.ToString(CultureInfo.InvariantCulture))
        AddNode(antet, "DataOpl", DateForm(draft.DataOrd))
        AddNode(antet, "universalCode", UniversalCode)
        mainForm.Add(antet)

        Dim comp As String = If(draft.Comp, String.Empty)
        Dim cual As String = If(draft.Cual, String.Empty)

        ' SubformInf -- one block per partner, in IDORDPART order (Access ORDER BY IDORDPART).
        Dim firstPart As Boolean = True
        For Each part As OrdDraftPart In draft.Parteneri.OrderBy(Function(p) p.Idordpartp).ThenBy(Function(p) p.TempId)
            Dim partKey As Integer = part.Cheie
            Dim inf As New XElement("SubformInf")

            ' Subform1
            Dim sub1 As New XElement("Subform1")
            AddNode(sub1, "NrUnicInreg", cual)
            inf.Add(sub1)

            ' Table1: HeaderRow1, HeaderRow2, dummy Row1, FooterRow (sums), real Row1s.
            Dim linii As List(Of OrdDraftLinie) = draft.LiniiPentru(partKey)
            Dim table1 As New XElement("Table1")
            table1.Add(DataGroup("HeaderRow1"))
            table1.Add(DataGroup("HeaderRow2"))

            Dim dummy As New XElement("Row1")
            For i As Integer = 1 To 7
                AddNode(dummy, "Cell" & i.ToString(CultureInfo.InvariantCulture), String.Empty)
            Next
            AddNode(dummy, "Cell8", "0.00")
            table1.Add(dummy)

            ' Pass 1: sums + the joined explanation.
            Dim sumRec As Double = 0, sumPlatiAnt As Double = 0, sumVal As Double = 0, sumRamas As Double = 0
            Dim expl As New List(Of String)()
            For Each l As OrdDraftLinie In linii
                sumRec += l.TotalReceptii
                sumPlatiAnt += l.PlatiAnt
                sumVal += l.Valoare
                sumRamas += l.Ramas
                If Not String.IsNullOrEmpty(l.Explicatie) Then expl.Add(l.Explicatie)
            Next
            Dim sExpl As String = String.Join("; ", expl)

            Dim footer As New XElement("FooterRow")
            AddNode(footer, "Cell5", ToXmlNum(sumRec))
            AddNode(footer, "Cell6", ToXmlNum(sumPlatiAnt))
            AddNode(footer, "Cell7", ToXmlNum(sumVal))
            AddNode(footer, "Cell8", ToXmlNum(sumRamas))
            table1.Add(footer)

            ' Pass 2: the real rows.
            For Each l As OrdDraftLinie In linii
                Dim row As New XElement("Row1")
                AddNode(row, "Cell1", l.CodAngajament)
                AddNode(row, "Cell2", l.CodIndicator)
                AddNode(row, "Cell3", ctx.CodProgram)
                AddNode(row, "Cell4", l.CodSsi)
                AddNode(row, "Cell5", ToXmlNum(l.TotalReceptii))
                AddNode(row, "Cell6", ToXmlNum(l.PlatiAnt))
                AddNode(row, "Cell7", ToXmlNum(l.Valoare))
                AddNode(row, "Cell8", ToXmlNum(l.Ramas))
                table1.Add(row)
            Next
            inf.Add(table1)

            ' SubformCaptura / Table2: dummy Row1, FooterRow, one Row1 per image
            ' (this partner's or global).
            Dim table2 As New XElement("Table2")
            table2.Add(New XElement("Row1", New XElement("Cell1")))
            table2.Add(DataGroup("FooterRow"))
            For Each att As OrdDraftAtt In draft.Atasamente
                Dim attPart As Integer = If(att.Idordpartp > 0, att.Idordpartp, att.PartTempId)
                If attPart <> partKey AndAlso attPart <> 0 Then Continue For
                Dim row As New XElement("Row1")
                AddNode(row, "Cell1", ImageOf(att, imagini))
                table2.Add(row)
            Next
            inf.Add(New XElement("SubformCaptura", table2))

            ' SubformBeneficiar
            Dim benGroup As New XElement("SubformBeneficiar")
            Dim ben As New XElement("SubformBen")
            AddNode(ben, "Beneficiar", part.DenBene)

            ' Text documents (no file name), this partner's or global: first one goes to
            ' DocumenteJustificative, the rest to Table3.
            Dim docText As New List(Of String)()
            For Each d As OrdDraftDoc In draft.Documente
                If Not String.IsNullOrWhiteSpace(d.NumeDoc) Then Continue For
                Dim docPart As Integer = If(d.Idordpartp > 0, d.Idordpartp, d.PartTempId)
                If docPart <> partKey AndAlso docPart <> 0 Then Continue For
                docText.Add(If(d.DocJust, String.Empty))
            Next
            AddNode(ben, "DocumenteJustificative", If(docText.Count > 0, docText(0), String.Empty))

            Dim table3 As New XElement("Table3")
            table3.Add(New XElement("Row1", New XElement("Cell1")))
            table3.Add(DataGroup("FooterRow"))
            For i As Integer = 1 To docText.Count - 1
                Dim row As New XElement("Row1")
                AddNode(row, "Cell1", docText(i))
                table3.Add(row)
            Next
            ben.Add(table3)
            ben.Add(DataGroup("AtasFis"))
            benGroup.Add(ben)

            ' SubformBen1: bank data + payment info.
            Dim ben1 As New XElement("SubformBen1")
            AddNode(ben1, "CifBeneficiar", part.CodFiscal)
            AddNode(ben1, "IbanBeneficiar", part.ContIban)
            AddNode(ben1, "BancaBeneficiar", part.Banca)
            AddNode(ben1, "InfPvPlata", sExpl)
            AddNode(ben1, "InfPvPlata1", String.Empty)
            benGroup.Add(ben1)
            inf.Add(benGroup)

            mainForm.Add(inf)

            ' Structural blocks go right AFTER the first SubformInf, before the overflow ones.
            If firstPart Then
                firstPart = False
                mainForm.Add(DataGroup("SubformBtnInf"))
                Dim semnAB As New XElement("SubformSemnaturaAB")
                AddNode(semnAB, "compartimentSpecialitate", UCaseSafe(comp))
                mainForm.Add(semnAB)
                mainForm.Add(DataGroup("SubformSemnaturaCD"))
                mainForm.Add(DataGroup("SubformSemnaturaOrdonator"))
            End If
        Next

        Return XmlDecl & New XElement("form1", mainForm).ToString()
    End Function

    ' -- Attachments (InsereazaAtasamente_ORD) --------------------------------

    ''' <summary>
    ''' Adds the "Attachments" node on the "form1" root: one "Attachment" per file document
    ''' (NumeDoc set, DocJust = base64 content). Returns the final XML.
    ''' </summary>
    Public Shared Function InsertAttachments(formXml As String, documente As IEnumerable(Of OrdDraftDoc)) As String
        Dim doc As XDocument = XDocument.Parse(formXml)
        Dim root As XElement = doc.Root      ' form1

        Dim attachments As New XElement("Attachments")
        For Each d As OrdDraftDoc In If(documente, Enumerable.Empty(Of OrdDraftDoc)())
            If String.IsNullOrWhiteSpace(d.NumeDoc) OrElse String.IsNullOrEmpty(d.DocJust) Then Continue For
            attachments.Add(New XElement("Attachment",
                                         New XElement("FileName", d.NumeDoc),
                                         New XElement("FileData", d.DocJust)))
        Next
        root.Add(attachments)

        Return XmlDecl & root.ToString()
    End Function

    ' -- Helpers ---------------------------------------------------------------

    ' AdaugaSubnod_ORD: text node only when the value is non-empty.
    Private Shared Sub AddNode(parent As XElement, name As String, value As String)
        If String.IsNullOrEmpty(value) Then
            parent.Add(New XElement(name))
        Else
            parent.Add(New XElement(name, value))
        End If
    End Sub

    ' An empty structural node carrying xfa:dataNode="dataGroup".
    Private Shared Function DataGroup(name As String) As XElement
        Return New XElement(name,
                            New XAttribute(XNamespace.Xmlns + "xfa", XfaNs.NamespaceName),
                            New XAttribute(XfaNs + "dataNode", "dataGroup"))
    End Function

    ' ToXmlNum_ORD: Format$(v, "0.00") with the decimal comma turned into a dot.
    Private Shared Function ToXmlNum(value As Double) As String
        Return value.ToString("0.00", CultureInfo.InvariantCulture)
    End Function

    ' Base64 of the image: the downloaded content first, then whatever the draft carries.
    Private Shared Function ImageOf(att As OrdDraftAtt, imagini As IReadOnlyDictionary(Of Integer, String)) As String
        Dim b64 As String = Nothing
        If imagini IsNot Nothing AndAlso att.Idordattp > 0 AndAlso imagini.TryGetValue(att.Idordattp, b64) Then
            Return If(b64, String.Empty)
        End If
        If att.Continut IsNot Nothing AndAlso att.Continut.Length > 0 Then
            Return Convert.ToBase64String(att.Continut)
        End If
        Return String.Empty
    End Function

    Private Shared Function DateForm(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                  Date.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
    End Function

    Private Shared Function UCaseSafe(s As String) As String
        Return If(s, String.Empty).ToUpperInvariant()
    End Function

End Class
