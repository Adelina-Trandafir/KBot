Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports KBot.Domain

''' <summary>
''' Constructorul XML-ului XFA pentru documentul DDF (felia 0020-05), portat FIDEL din
''' mdl_FX_DDF_PDF: <c>GenereazaXML_PentruPython</c> (arborele «form1»),
''' <c>GenereazaXML_NOTAFD</c> (nota FD) și <c>InsereazaAtasamente</c> (nodul «Attachments»).
''' Pur (fără I/O, fără WinForms) -> se poate testa fără STA. Generarea e PER REVIZIE (tabelele
''' Access <c>tmpFX_*</c> țin datele unei singure revizii); aici primim antetul, revizia și
''' liniile/secțiunea B ale acelei revizii.
'''
''' Fidelități portate din sursă (fiecare cu test):
'''   * <c>ToXmlNum</c> — înlocuiește virgula zecimală cu punct (cultură invariantă), FĂRĂ
'''     zerouri forțate: 129705 -> «129705», nu «129705.00»;
'''   * <c>AdaugaSubnod</c> — omite nodul-text când valoarea e goală (element gol «&lt;Cell4/&gt;»,
'''     nu «&lt;Cell4&gt;&lt;/Cell4&gt;»);
'''   * primul <c>Row1</c> e FICTIV (index 0, doar Cell1 gol) — machetа îl sare;
'''   * <c>Int()</c> din NOTAFD e trunchiere spre −∞ (floor), nu spre zero;
'''   * lățimile de câmp <c>Left(…, N)</c> din NOTAFD sunt intenționate;
'''   * <c>Cell7</c> e sărit intenționat în secțiunea B;
'''   * sumele NOTAFD (<c>sum_rezv_crdt_ang_act</c> etc.) se CALCULEAZĂ (anterior+influență),
'''     nu se citesc dintr-o coloană.
'''
''' Decizie §2.9 (rezolvată din sursă): codul de program vine din globalul de sesiune
''' (<c>globCodProgram</c> -> <see cref="SessionContext.CodProgram"/>), NU din
''' <c>FX_DDF.Program</c> — macheta Access scrie <c>Nz(globCodProgram, "0000000000")</c> în
''' Cell2/program, ignorând antetul.
'''
''' RISC DE FIDELITATE NEVERIFICAT: <c>EncodeBase64</c> din VBA folosea
''' <c>StrConv(…, vbFromUnicode)</c> = octeți ANSI (codul de pagină al sistemului). Aici
''' NOTAFD se codează UTF-8. Fără o machetă DDF reală de comparat (vezi firul deschis din
''' STATUS — <c>ddf_demo.xml</c> e de fapt ORD), nu se poate decide care e corect; UTF-8 e
''' fără pierderi pentru diacritice, ANSI ar tăia caracterele din afara codului de pagină.
''' </summary>
Public NotInheritable Class DdfXmlBuilder

    Private Sub New()
    End Sub

    Private Const XmlDeclForm As String = "<?xml version=""1.0"" encoding=""UTF-8""?>"
    Private Const XmlDeclNotafd As String = "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>"
    Private Const NotafdNs As String = "mfp:anaf:dgti:notafd:declaratie:v1"
    Private Const DefaultProgram As String = "0000000000"

    ''' <summary>Globalii de sesiune de care are nevoie constructorul (§2.9). POCO pur.</summary>
    Public NotInheritable Class Context
        Public Property NumeUnitate As String = String.Empty
        Public Property CodFiscal As String = String.Empty
        ''' <summary>Codul de program (globCodProgram). Gol -> «0000000000».</summary>
        Public Property CodProgram As String = String.Empty

        Public Shared Function FromSession(s As KBot.Common.SessionContext) As Context
            If s Is Nothing Then Return New Context()
            Return New Context() With {
                .NumeUnitate = If(s.NumeUnitate, String.Empty),
                .CodFiscal = If(s.CF, String.Empty),
                .CodProgram = If(s.CodProgram, String.Empty)}
        End Function
    End Class

    ' ── Documentul complet ────────────────────────────────────────────────────

    ''' <summary>
    ''' XML-ul final (form1 + Attachments cu NOTAFD.xml și fișierele), gata de scris pe disc
    ''' și dat lui XfaWriter. Oglindește GenereazaPDF: form1 -> NOTAFD -> InsereazaAtasamente.
    ''' </summary>
    Public Shared Function BuildComplete(ctx As Context, antet As DdfAntet, revizie As RevizieRow,
                                         linii As IEnumerable(Of LinieSaRow),
                                         sbRows As IEnumerable(Of SectiuneBRow),
                                         attRows As IEnumerable(Of AtasamentRow)) As String
        Dim formXml As String = BuildFormXml(ctx, antet, revizie, linii, sbRows)
        Dim notafdXml As String = BuildNotafdXml(ctx, antet, revizie, linii, sbRows)
        Return InsertAttachments(formXml, notafdXml, attRows)
    End Function

    ' ── form1 (GenereazaXML_PentruPython) ─────────────────────────────────────

    Public Shared Function BuildFormXml(ctx As Context, antet As DdfAntet, revizie As RevizieRow,
                                        linii As IEnumerable(Of LinieSaRow),
                                        sbRows As IEnumerable(Of SectiuneBRow)) As String
        If ctx Is Nothing Then ctx = New Context()
        Dim program As String = ProgramOf(ctx)

        Dim antetNode As New XElement("SubformAntet")
        If antet IsNot Nothing Then
            AddNode(antetNode, "DenInstPb", UCaseSafe(ctx.NumeUnitate))
            AddNode(antetNode, "cif", ctx.CodFiscal)
            AddNode(antetNode, "NrUnicInreg", CualText(antet))
            AddNode(antetNode, "SubtitluDF", antet.ObiectDDF)
        End If
        If revizie IsNot Nothing Then
            AddNode(antetNode, "DataRevizuirii", DateForm(revizie.DataRev))
            AddNode(antetNode, "Revizuirea", revizie.NumarRev.ToString(CultureInfo.InvariantCulture))
        End If
        AddNode(antetNode, "CheckBox1", "0")
        AddNode(antetNode, "universalCode", "A1.0.07")

        ' Secțiunea A
        Dim sub123 As New XElement("Subform123")
        If antet IsNot Nothing Then AddNode(sub123, "ComparimentSpecialitate", UCaseSafe(antet.Comp))
        If revizie IsNot Nothing Then
            AddNode(sub123, "DescrieObFundRevizuireScurt", revizie.DescScurta)
            AddNode(sub123, "DescrieObFundRevizuireLung", revizie.DescLunga)
        End If

        Dim table1 As New XElement("Table1")
        ' Row1 FICTIV (index 0) — machetа îl sare.
        table1.Add(New XElement("Row1", New XElement("Cell1")))
        For Each l As LinieSaRow In SafeEnum(linii)
            Dim row As New XElement("Row1")
            AddNode(row, "Cell1", l.ElementFund)
            AddNode(row, "Cell2", program)
            AddNode(row, "Cell3", Cell3Of(l))
            AddNode(row, "Cell4", l.ParametriiFund)
            AddNode(row, "Cell5", ToXmlNum(l.ValPrec))
            AddNode(row, "Cell6", ToXmlNum(l.ValCur))
            table1.Add(row)
        Next

        Dim sub4 As New XElement("Subform4",
            New XElement("Subform41", New XElement("CheckBox2", "1")),
            table1)
        Dim sub5 As New XElement("Subform5",
            New XElement("CheckBox5", "1"), New XElement("CheckBox6", "1"))
        Dim sectA As New XElement("SubformSectiuneaA", sub123, sub4, sub5)

        ' Secțiunea B (Table3): Row1 fictiv + un rând per linie SB (Cell7 sărit).
        Dim table3 As New XElement("Table3")
        table3.Add(New XElement("Row1", New XElement("Cell1")))
        For Each sb As SectiuneBRow In SafeEnum(sbRows)
            Dim row As New XElement("Row1")
            AddNode(row, "Cell1", sb.CodAngajament)
            AddNode(row, "Cell2", sb.CodIndicator)
            AddNode(row, "Cell3", program)
            AddNode(row, "Cell4", sb.CodSSI)
            AddNode(row, "Cell5", ToXmlNum(sb.CaAnterior))
            AddNode(row, "Cell6", ToXmlNum(sb.Inf1))
            AddNode(row, "Cell8", ToXmlNum(sb.CbAnterior))
            AddNode(row, "Cell9", ToXmlNum(sb.Inf2))
            table3.Add(row)
        Next
        Dim sectB As New XElement("SubformSectiuneaB", New XElement("CheckBox9", "1"), table3)

        Dim form1 As New XElement("form1", antetNode, sectA, sectB)
        Return XmlDeclForm & form1.ToString()
    End Function

    ' ── NOTAFD (GenereazaXML_NOTAFD) ──────────────────────────────────────────

    Public Shared Function BuildNotafdXml(ctx As Context, antet As DdfAntet, revizie As RevizieRow,
                                          linii As IEnumerable(Of LinieSaRow),
                                          sbRows As IEnumerable(Of SectiuneBRow)) As String
        If ctx Is Nothing Then ctx = New Context()
        Dim ns As XNamespace = NotafdNs
        Dim program As String = Left(ProgramOf(ctx), 10)

        Dim root As New XElement(ns + "NOTAFD")
        If antet IsNot Nothing Then
            root.Add(New XAttribute("Cif", ctx.CodFiscal))
            root.Add(New XAttribute("DenInstPb", Left(UCaseSafe(ctx.NumeUnitate), 150)))
            root.Add(New XAttribute("SubtitluDF", Left(antet.ObiectDDF, 150)))
            root.Add(New XAttribute("NrUnicInreg", Left(CualText(antet), 20)))
        End If
        If revizie IsNot Nothing Then
            root.Add(New XAttribute("Revizuirea", Left(revizie.NumarRev.ToString(CultureInfo.InvariantCulture), 3)))
            root.Add(New XAttribute("DataRevizuirii", DateNotafd(revizie.DataRev)))
        End If

        ' Secțiunea A. ABATERE DELIBERATĂ de la VBA: macheta folosea `createElement` (fără
        ' namespace) pentru copii, ceea ce produce elemente în NICIUN namespace — INVALID față
        ' de XSD-ul NOTAFD, care e `elementFormDefault="qualified"` (toate elementele trebuie în
        ' `mfp:anaf:dgti:notafd:declaratie:v1`). Aici TOATE elementele poartă namespace-ul,
        ' pentru un XML valid. (Atributele rămân ne-calificate — `attributeFormDefault="unqualified"`.)
        Dim sectA As New XElement(ns + "sectiuneaA")
        If antet IsNot Nothing Then sectA.Add(New XAttribute("compartiment_specialitate", Left(UCaseSafe(antet.Comp), 150)))
        If revizie IsNot Nothing Then
            sectA.Add(New XAttribute("obiect_fd_reviz_scurt", Left(revizie.DescScurta, 250)))
            sectA.Add(New XAttribute("obiect_fd_reviz_lung", Left(revizie.DescLunga, 500)))
        End If

        Dim angVal As New XElement(ns + "ang_legale_val", New XAttribute("ckbx_stab_tin_cont", "1"))
        For Each l As LinieSaRow In SafeEnum(linii)
            Dim row As New XElement(ns + "rowT_ang_pl_val")
            row.Add(New XAttribute("element_fd", Left(l.ElementFund, 150)))
            row.Add(New XAttribute("program", program))
            row.Add(New XAttribute("codSSI", If(l.SS, String.Empty)))
            row.Add(New XAttribute("param_fd", Left(l.ParametriiFund, 500)))
            row.Add(New XAttribute("valt_rev_prec", IntTrunc(l.ValPrec)))
            row.Add(New XAttribute("influente", IntTrunc(l.ValCur)))
            row.Add(New XAttribute("valt_actualiz", IntTrunc(l.ValPrec + l.ValCur)))
            angVal.Add(row)
        Next
        sectA.Add(angVal)
        sectA.Add(New XElement(ns + "ang_legale_plati", New XAttribute("ckbx_fara_ang_emis_ancrt", "1")))
        root.Add(sectA)

        ' Secțiunea B
        Dim sectB As New XElement(ns + "sectiuneaB", New XAttribute("ckbx_secta_inreg_ctrl_ang", "1"))
        For Each sb As SectiuneBRow In SafeEnum(sbRows)
            Dim row As New XElement(ns + "rowT_ang_ctrl_ang")
            row.Add(New XAttribute("cod_angajament", Left(sb.CodAngajament, 11)))
            row.Add(New XAttribute("indicator_angajament", Left(sb.CodIndicator, 3)))
            row.Add(New XAttribute("program", program))
            row.Add(New XAttribute("cod_SSI", Left(sb.CodSSI, 15)))
            row.Add(New XAttribute("sum_rezv_crdt_ang_af_rvz_prc", IntTrunc(sb.CaAnterior)))
            row.Add(New XAttribute("influente_c6", IntTrunc(sb.Inf1)))
            row.Add(New XAttribute("sum_rezv_crdt_ang_act", IntTrunc(sb.CaAnterior + sb.Inf1)))
            row.Add(New XAttribute("sum_rezv_crdt_bug_af_rvz_prc", IntTrunc(sb.CbAnterior)))
            row.Add(New XAttribute("influente_c9", IntTrunc(sb.Inf2)))
            row.Add(New XAttribute("sum_rezv_crdt_bug_act", IntTrunc(sb.CbAnterior + sb.Inf2)))
            sectB.Add(row)
        Next
        root.Add(sectB)

        Return XmlDeclNotafd & root.ToString()
    End Function

    ' ── InsereazaAtasamente ───────────────────────────────────────────────────

    ''' <summary>
    ''' Adaugă nodul «Attachments» pe rădăcina «form1»: NOTAFD.xml (base64) + fiecare fișier
    ''' din <paramref name="attRows"/> (DateFisier e deja base64). Întoarce XML-ul final.
    ''' </summary>
    Public Shared Function InsertAttachments(formXml As String, notafdXml As String,
                                             attRows As IEnumerable(Of AtasamentRow)) As String
        Dim doc As XDocument = XDocument.Parse(formXml)
        Dim root As XElement = doc.Root      ' form1

        Dim attachments As New XElement("Attachments")
        If Not String.IsNullOrEmpty(notafdXml) Then
            AddAttachment(attachments, "NOTAFD.xml", EncodeBase64(notafdXml))
        End If
        For Each a As AtasamentRow In SafeEnum(attRows)
            If Not String.IsNullOrEmpty(a.CaleFisier) AndAlso Not String.IsNullOrEmpty(a.DateFisier) Then
                AddAttachment(attachments, a.CaleFisier, a.DateFisier)   ' deja base64
            End If
        Next
        root.Add(attachments)

        Return XmlDeclForm & root.ToString()
    End Function

    Private Shared Sub AddAttachment(parent As XElement, fileName As String, base64Data As String)
        parent.Add(New XElement("Attachment",
                                New XElement("FileName", fileName),
                                New XElement("FileData", base64Data)))
    End Sub

    ''' <summary>
    ''' Base64 al textului NOTAFD. VEZI riscul de fidelitate din antetul clasei: VBA folosea
    ''' octeți ANSI (StrConv vbFromUnicode); aici folosim UTF-8.
    ''' </summary>
    Public Shared Function EncodeBase64(text As String) As String
        If text Is Nothing Then Return String.Empty
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
    End Function

    ' ── Helperi ───────────────────────────────────────────────────────────────

    ' AdaugaSubnod: adaugă text DOAR când e ne-gol (element gol altfel, ca în VBA).
    Private Shared Sub AddNode(parent As XElement, name As String, value As String)
        If String.IsNullOrEmpty(value) Then
            parent.Add(New XElement(name))
        Else
            parent.Add(New XElement(name, value))
        End If
    End Sub

    ' ToXmlNum: virgulă zecimală -> punct, fără zerouri forțate (cultură invariantă).
    Private Shared Function ToXmlNum(value As Double) As String
        Return value.ToString(CultureInfo.InvariantCulture)
    End Function

    ' Int() din VBA = trunchiere spre −∞ (floor), NU spre zero. Rezultat ca întreg-șir.
    Private Shared Function IntTrunc(value As Double) As String
        Return CLng(Math.Floor(value)).ToString(CultureInfo.InvariantCulture)
    End Function

    ' Cell3 = SS + Left(clsf,2) + Replace(Mid(clsf,7),".","") — cu gărzi pentru șiruri scurte.
    Private Shared Function Cell3Of(l As LinieSaRow) As String
        Dim clsf As String = If(l.Clsf, String.Empty)
        Dim primele2 As String = If(clsf.Length >= 2, clsf.Substring(0, 2), clsf)
        Dim delaPoz7 As String = If(clsf.Length >= 7, clsf.Substring(6), String.Empty)
        Return If(l.SS, String.Empty) & primele2 & delaPoz7.Replace(".", String.Empty)
    End Function

    Private Shared Function ProgramOf(ctx As Context) As String
        Return If(String.IsNullOrEmpty(ctx.CodProgram), DefaultProgram, ctx.CodProgram)
    End Function

    Private Shared Function CualText(antet As DdfAntet) As String
        Return antet.Cual.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Shared Function DateForm(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                  Date.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
    End Function

    Private Shared Function DateNotafd(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                  Date.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))
    End Function

    Private Shared Function UCaseSafe(s As String) As String
        Return If(s, String.Empty).ToUpperInvariant()
    End Function

    Private Shared Function Left(s As String, n As Integer) As String
        Dim v As String = If(s, String.Empty)
        Return If(v.Length <= n, v, v.Substring(0, n))
    End Function

    Private Shared Function SafeEnum(Of T)(items As IEnumerable(Of T)) As IEnumerable(Of T)
        Return If(items, Enumerable.Empty(Of T)())
    End Function

End Class
