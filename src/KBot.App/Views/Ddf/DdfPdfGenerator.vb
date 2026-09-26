Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0081-04: builds the PDF of ONE revision into the work area (<c>TempPdf\</c>) -- the
''' steps the DDF view's «Genereaza» ran inline since slice 0020-05, moved here so the send's FINAL
''' PDF is built exactly the same way. Reads the generation data (<c>pentru_generare=1</c>), keeps
''' the revision's rows, builds the XFA XML in the requested <see cref="DdfPdfMode"/> and runs
''' <c>XfaWriter</c>. Nothing is written to the server here.
''' </summary>
Public NotInheritable Class DdfPdfGenerator

    ''' <summary>What a generation produced.</summary>
    Public NotInheritable Class Rezultat
        Public Property PdfPath As String = String.Empty
        Public Property Antet As DdfAntet
        Public Property Revizie As RevizieRow
    End Class

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Generates the PDF of revision <paramref name="idrev"/> of angajament <paramref name="cod"/>.
    ''' Risky boundary (HTTP, XML, iTextSharp, disk): logs and rethrows.
    ''' </summary>
    ''' <param name="citesteDdf">The DDF read through the caller's 401 net
    ''' (<c>GetDdfAsync(cod, pentruGenerare:=True)</c>).</param>
    ''' <param name="citesteFisier">Fetches one attachment's bytes by <c>IdRevAtt</c> (0081-05, the
    ''' captures of the final PDF); Nothing = captures are not drawn.</param>
    Public Shared Async Function GenereazaAsync(citesteDdf As Func(Of Task(Of DdfInfo)),
                                                citesteFisier As Func(Of Integer, Task(Of Byte())),
                                                session As SessionContext,
                                                idrev As Integer,
                                                mode As DdfPdfMode) As Task(Of Rezultat)
        Try
            ArgumentNullException.ThrowIfNull(citesteDdf)
            Dim data As DdfInfo = Await citesteDdf().ConfigureAwait(True)
            If data Is Nothing Then Throw New InvalidOperationException("The DDF read returned nothing.")

            Dim revizie As RevizieRow = data.Revizii.FirstOrDefault(Function(r) r.Idrev = idrev)
            If revizie Is Nothing Then Throw New InvalidOperationException($"Revision {idrev} is not in the DDF read.")
            Dim antet As DdfAntet = data.AntetDeLucru(revizie.Iddf)
            If antet Is Nothing Then Throw New InvalidOperationException($"Revision {idrev} has no FX_DDF header.")

            ' Only the target revision's rows (the generation is per revision, like Access's tmpFX_*).
            Dim linii = data.Linii.Where(Function(l) l.Idrev = idrev).ToList()
            Dim sb = data.SectiuneB.Where(Function(s) s.Idrev = idrev).ToList()
            Dim att = data.Atasamente.Where(Function(a) a.Idrev = idrev).ToList()

            Dim capturi As List(Of String) = Nothing
            If mode = DdfPdfMode.Final AndAlso citesteFisier IsNot Nothing Then
                capturi = Await CapturileAsync(att, citesteFisier).ConfigureAwait(True)
            End If

            Dim ctx As DdfXmlBuilder.Context = DdfXmlBuilder.Context.FromSession(session)
            Dim xml As String = DdfXmlBuilder.BuildComplete(ctx, antet, revizie, linii, sb, att, mode, capturi)

            ' The generated document is UNSIGNED, so a derived artefact: the work area, never the
            ' persistent cache of signed PDFs (slice 0041). Same file name as the convention.
            Dim numeFisier As String = Path.GetFileName(
                DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, antet, revizie.NumarRev))
            If String.IsNullOrEmpty(numeFisier) Then Throw New InvalidOperationException("No PDF file name for the revision.")
            TempPdfStore.EnsureRoot()
            Dim pdfPath As String = TempPdfStore.PathFor(numeFisier)
            Dim xmlPath As String = Path.ChangeExtension(pdfPath, ".xml")
            File.WriteAllText(xmlPath, xml, New Text.UTF8Encoding(False))

            Await Task.Run(Sub() Call Global.KBot.Xfa.XfaWriter.Genereaza(xmlPath, pdfPath, "DDF", deschidePdf:=False)).ConfigureAwait(True)

            Return New Rezultat() With {.PdfPath = pdfPath, .Antet = antet, .Revizie = revizie}
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPdfGenerator.GenereazaAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' 0081-05: the FOREXE captures of the revision (<c>PrtScr = 1</c>), as base64 PNG, in the order
    ''' they were stored (<c>IdRevAtt</c>, i.e. the order the workflows took them). The bytes come
    ''' from the row itself when the old column carries them, else from the attachment store.
    ''' A capture that cannot be read stops the generation: a final PDF missing a capture would be
    ''' signed as complete.
    ''' </summary>
    Private Shared Async Function CapturileAsync(att As IEnumerable(Of AtasamentRow),
                                                 citesteFisier As Func(Of Integer, Task(Of Byte()))) As Task(Of List(Of String))
        Dim result As New List(Of String)()
        For Each a As AtasamentRow In att.Where(Function(x) x.PrtScr).OrderBy(Function(x) x.IdRevAtt)
            If Not String.IsNullOrWhiteSpace(a.DateFisier) Then
                result.Add(a.DateFisier)
                Continue For
            End If
            Dim bytes As Byte() = Await citesteFisier(a.IdRevAtt).ConfigureAwait(True)
            If bytes Is Nothing OrElse bytes.Length = 0 Then
                Throw New InvalidOperationException($"Capture {a.IdRevAtt} ({a.CaleFisier}) has no bytes on the server.")
            End If
            result.Add(Convert.ToBase64String(bytes))
        Next
        Return result
    End Function

End Class
