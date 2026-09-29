Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Xfa

''' <summary>
''' Slice 0078-06: the REAL section B written INTO a DDF that is already signed on section A -- the
''' path proven on the bench in 0078-04/05 (<c>DdfSectiuneaBHarnessForm</c>), used by the DDF view.
'''
''' <para>The interim document is signed on A with a placeholder section B. After the send forexecab
''' has given the real Cod angajament and indicators, and they are in <c>FX_DDF_REV_SB</c>. They are
''' written as an incremental update (only the <c>datasets</c> packet + NOTAFD.xml, see
''' <see cref="XfaSignedDocument.FillIncremental"/>) into a NEW file of the work area: the signed
''' bytes stay as they are, and nothing reaches the server until B is signed.</para>
''' </summary>
Public NotInheritable Class DdfSectionBInsert

    ''' <summary>What one insert produced. POCO.</summary>
    Public NotInheritable Class Rezultat
        Public Property PdfPath As String = String.Empty
        Public Property Rows As Integer
        ''' <summary>Captures sent for Table4.</summary>
        Public Property Captures As Integer
        ''' <summary>Captures found in the written document's data (Table4 rows with an image).</summary>
        Public Property CapturesInDocument As Integer
        ''' <summary>What happened to NOTAFD.xml (Romanian, for the log).</summary>
        Public Property NotafdNote As String = String.Empty
    End Class

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The server's row says section B is due: sent (stage 2 or 3), the stored PDF signed on A and
    ''' not on B, and the angajament has forexecab's code (no longer «!…»). Pure.
    ''' </summary>
    Public Shared Function IsCandidate(revizie As RevizieRow, antet As DdfAntet) As Boolean
        If revizie Is Nothing OrElse antet Is Nothing OrElse Not revizie.ArePdfSemnat Then Return False
        If revizie.StareTrimitere <> DdfSendStage.SentInProgress AndAlso revizie.StareTrimitere <> DdfSendStage.FinalPdf Then Return False
        If Not DdfRevisionStates.HasRole(revizie.Semnatura, DdfRevisionStates.RoleA) Then Return False
        If DdfRevisionStates.HasRole(revizie.Semnatura, DdfRevisionStates.RoleB) Then Return False
        Dim cod As String = If(antet.CodAngajament, String.Empty).Trim()
        Return cod.Length > 0 AndAlso Not cod.StartsWith("!", StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Every section B row carries forexecab's values: an 11-character code and a 3-character
    ''' indicator (the lengths the form's «Valideaza» demands), none of them K-BOT's «!…». Pure.
    ''' </summary>
    Public Shared Function ServerRowsReady(sb As IEnumerable(Of SectiuneBRow)) As Boolean
        Dim lista As List(Of SectiuneBRow) = If(sb, Enumerable.Empty(Of SectiuneBRow)()).ToList()
        If lista.Count = 0 Then Return False
        Return lista.All(Function(r) IsReal(r.CodAngajament, DdfXmlBuilder.InterimCodAngajament.Length) AndAlso
                                     IsReal(r.CodIndicator, DdfXmlBuilder.InterimIndicator.Length))
    End Function

    Private Shared Function IsReal(value As String, length As Integer) As Boolean
        Dim v As String = If(value, String.Empty).Trim()
        Return v.Length = length AndAlso Not v.StartsWith("!", StringComparison.Ordinal) AndAlso Not v.Contains("_"c)
    End Function

    ''' <summary>
    ''' The fill XML: section B exactly as the final generation writes it (<see cref="DdfXmlBuilder.BuildFormXml"/>,
    ''' captures in Table4 included), cut down to <c>SubformSectiuneaB</c> so nothing else of the
    ''' signed form is rewritten. <paramref name="program"/> goes in every row's Cell3. Pure.
    ''' </summary>
    Public Shared Function BuildFillXml(ctx As DdfXmlBuilder.Context, antet As DdfAntet, revizie As RevizieRow,
                                        sb As IEnumerable(Of SectiuneBRow), capturi As IEnumerable(Of String),
                                        program As String) As String
        Dim src As DdfXmlBuilder.Context = If(ctx, New DdfXmlBuilder.Context())
        Dim c As New DdfXmlBuilder.Context() With {
            .NumeUnitate = src.NumeUnitate, .CodFiscal = src.CodFiscal, .CodProgram = If(program, String.Empty)}
        Dim full As String = DdfXmlBuilder.BuildFormXml(c, antet, revizie, Enumerable.Empty(Of LinieSaRow)(), sb,
                                                        sectiuneaB:=True, capturi:=capturi)
        Dim sectB As XElement = XDocument.Parse(full).Root.Element("SubformSectiuneaB")
        Return "<?xml version=""1.0"" encoding=""UTF-8""?>" & New XElement("form1", sectB).ToString()
    End Function

    ''' <summary>
    ''' Cell2 (program) of the first real section A row in the form data; "" when there is none.
    ''' Section B takes the program the SIGNED section A carries (the interim's placeholder), as on
    ''' the bench. Risky boundary (XML): logs and rethrows.
    ''' </summary>
    Public Shared Function SectionAProgram(formXml As String) As String
        Try
            If String.IsNullOrWhiteSpace(formXml) Then Return String.Empty
            Dim table1 As XElement = XDocument.Parse(formXml).Descendants().FirstOrDefault(Function(x) x.Name.LocalName = "Table1")
            If table1 Is Nothing Then Return String.Empty
            For Each r As XElement In table1.Elements().Where(Function(x) x.Name.LocalName = "Row1")
                Dim program As String = CellText(r, "Cell2")
                If program.Length > 0 Then Return program
            Next
            Return String.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectionBInsert.SectionAProgram", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Writes section B into <paramref name="signedPath"/> (read shared, never changed) and returns
    ''' the new file in the work area, <c>&lt;name&gt;_B_HHmmss.pdf</c> -- a new name every time,
    ''' because Acrobat locks and serves from memory a name it already has open. Refuses (throws)
    ''' when the signed bytes did not survive as the prefix of the result. Risky boundary (disk,
    ''' iText, XML): logs and rethrows.
    ''' </summary>
    Public Shared Async Function InsertAsync(signedPath As String, ctx As DdfXmlBuilder.Context,
                                             antet As DdfAntet, revizie As RevizieRow,
                                             sb As List(Of SectiuneBRow), capturi As List(Of String),
                                             fallbackProgram As String) As Task(Of Rezultat)
        Try
            Dim before As Byte() = SignedPdfFiles.ReadShared(signedPath)
            Dim formXml As String = Await Task.Run(Function() XfaSignedDocument.ReadFormData(before)).ConfigureAwait(True)
            Dim program As String = SectionAProgram(formXml)
            If program.Length = 0 Then program = If(fallbackProgram, String.Empty)
            Dim dataXml As String = BuildFillXml(ctx, antet, revizie, sb, capturi, program)

            TempPdfStore.EnsureRoot()
            Dim target As String = TargetPath(signedPath)
            Dim inPath As String = Path.ChangeExtension(target, ".inainte.pdf")
            Dim xmlPath As String = Path.ChangeExtension(target, ".xml")
            File.WriteAllBytes(inPath, before)
            File.WriteAllText(xmlPath, dataXml, New Text.UTF8Encoding(False))
            Dim note As String = Await Task.Run(Function() XfaSignedDocument.FillIncremental(inPath, target, xmlPath)).ConfigureAwait(True)
            Try
                File.Delete(inPath)
            Catch ex As Exception
                ' Only a copy in the work area (wiped at start-up); the insert itself succeeded.
                GlobalErrorLog.Write("DdfSectionBInsert.InsertAsync.Cleanup", ex)
            End Try

            Dim after As Byte() = SignedPdfFiles.ReadShared(target)
            If after.Length <= before.Length OrElse Not after.AsSpan(0, before.Length).SequenceEqual(before) Then
                Throw New InvalidOperationException("The signed bytes of section A are not the prefix of the written file.")
            End If
            Dim afterXml As String = Await Task.Run(Function() XfaSignedDocument.ReadFormData(after)).ConfigureAwait(True)
            Return New Rezultat() With {
                .PdfPath = target, .Rows = sb.Count, .Captures = If(capturi, New List(Of String)()).Count,
                .CapturesInDocument = CountCaptures(afterXml), .NotafdNote = If(note, String.Empty)}
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectionBInsert.InsertAsync", ex)
            Throw
        End Try
    End Function

    ' «<work area>\<name>_B_HHmmss.pdf»; «_2», «_3»... in the same second.
    Private Shared Function TargetPath(signedPath As String) As String
        Dim baseName As String = Path.GetFileNameWithoutExtension(signedPath) & "_B_" & DateTime.Now.ToString("HHmmss")
        Dim candidate As String = TempPdfStore.PathFor(baseName & ".pdf")
        Dim n As Integer = 2
        While File.Exists(candidate)
            candidate = TempPdfStore.PathFor($"{baseName}_{n}.pdf")
            n += 1
        End While
        Return candidate
    End Function

    ' Table4 rows of the form data whose cell holds something (the template's empty row is skipped).
    Private Shared Function CountCaptures(formXml As String) As Integer
        If String.IsNullOrWhiteSpace(formXml) Then Return 0
        Dim table4 As XElement = XDocument.Parse(formXml).Descendants().FirstOrDefault(Function(x) x.Name.LocalName = "Table4")
        If table4 Is Nothing Then Return 0
        Return table4.Elements().Count(Function(r) r.Name.LocalName = "Row1" AndAlso CellText(r, "Cell1").Length > 0)
    End Function

    Private Shared Function CellText(row As XElement, name As String) As String
        Dim cell As XElement = row.Elements().FirstOrDefault(Function(x) x.Name.LocalName = name)
        Return If(cell Is Nothing, String.Empty, cell.Value.Trim())
    End Function

End Class
