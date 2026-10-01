Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0099: builds the PDF of ONE ordonantare into the work area (<c>TempPdf\</c>) without
''' showing it -- the steps <c>OrdView.OnGenerateRequested</c> runs for the «Generează documentul» button
''' (the whole graph, the image bytes, the XML, <c>XfaWriter</c>), for the print list, which makes
''' several documents in a row. Nothing is written to the server here.
''' </summary>
Friend NotInheritable Class OrdPdfGenerator

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Generates the document of <paramref name="ordonantare"/> and returns its path. Risky boundary
    ''' (HTTP, XML, iTextSharp, disk): logs and re-throws.
    ''' </summary>
    Public Shared Async Function GenereazaAsync(api As IApiClient,
                                                session As SessionContext,
                                                ordonantare As OrdHeaderRow,
                                                cod As String) As Task(Of String)
        Try
            ArgumentNullException.ThrowIfNull(api)
            ArgumentNullException.ThrowIfNull(ordonantare)
            Dim idordp As Integer = ordonantare.Idordp

            Dim draft As OrdDraft = Await api.GetOrdDraftAsync(idordp, CancellationToken.None).ConfigureAwait(True)
            If draft Is Nothing Then Throw New InvalidOperationException($"Ordonanțarea {idordp} nu a putut fi citită de pe server.")

            ' The image bytes, one download per attachment. 404 = no image, a normal state.
            Dim imagini As New Dictionary(Of Integer, String)()
            For Each att As OrdDraftAtt In draft.Atasamente
                If att.Idordattp <= 0 Then Continue For
                Dim img As PdfDownloadResult = Await api.GetOrdAtasamentAsync(
                    att.Idordattp, Nothing, CancellationToken.None).ConfigureAwait(True)
                If img IsNot Nothing AndAlso img.Status = PdfDownloadStatus.Content AndAlso img.Bytes IsNot Nothing Then
                    imagini(att.Idordattp) = Convert.ToBase64String(img.Bytes)
                End If
            Next

            Dim ctx As OrdXmlBuilder.Context = OrdXmlBuilder.Context.FromSession(session)
            Dim xml As String = OrdXmlBuilder.BuildComplete(ctx, draft, imagini)

            ' Unsigned -> a derived artefact: the work area, never the signed cache.
            Dim numeFisier As String = IO.Path.GetFileName(
                OrdPdfLocator.ExpectedPath(KBotPaths.Current.OrdPdfRoot, ordonantare, cod))
            If String.IsNullOrEmpty(numeFisier) Then Throw New InvalidOperationException("No PDF file name for the ordonantare.")
            TempPdfStore.EnsureRoot()
            Dim pdfPath As String = TempPdfStore.PathFor(numeFisier)
            Dim xmlPath As String = IO.Path.ChangeExtension(pdfPath, ".xml")
            IO.File.WriteAllText(xmlPath, xml, New Text.UTF8Encoding(False))

            Await Task.Run(Sub() Call Global.KBot.Xfa.XfaWriter.Genereaza(xmlPath, pdfPath, "ORD", deschidePdf:=False)).ConfigureAwait(True)
            Return pdfPath
        Catch ex As Exception
            GlobalErrorLog.Write("OrdPdfGenerator.GenereazaAsync", ex)
            Throw
        End Try
    End Function

End Class
