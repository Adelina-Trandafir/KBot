Imports System.IO
Imports KBot.Common
Imports Microsoft.Playwright

' =============================================================================
'  DocumentUpload - slice 0088: uploading a signed PDF into FOREXE through
'  «Transmitere documente electronice» (the CAB correction note, F1135).
'
'  The page (Surse/CAB+ERRRRRRR/trimitere_page.txt) is a portal page that holds
'  the real form in an <iframe src=".../WAS6DUS/">: the file input
'  <input type="file" name="linkdoc"> and the button <input type="submit"
'  value="Trimite"> live in that frame, not in the page. So every frame is
'  searched for the input rather than one frame being assumed.
' =============================================================================
Partial Public Class WorkflowExecutor

    Public Const ElectronicDocumentsUrl As String = "https://forexe.mfinante.gov.ro/transmitere-documente-electronice"

    Private Const LinkDocSelector As String = "input[type=file][name=linkdoc]"
    Private Const SendButtonSelector As String = "input[type=submit][value='Trimite']"

    ' How long the portal may take to show the form, and FOREXE to answer the upload.
    Private Const FormWaitSeconds As Integer = 45
    Private Const AnswerWaitSeconds As Integer = 180
    ' The answer is kept for the operator and the database; it is cut to this.
    Private Const MaxAnswerLength As Integer = 2000

    ''' <summary>
    ''' Opens «Transmitere documente electronice», puts <paramref name="pdfPath"/> into «linkdoc»,
    ''' presses «Trimite» and returns the text the form's frame shows afterwards (FOREXE's answer,
    ''' cut to <see cref="MaxAnswerLength"/> characters). The browser is sent back to the page it
    ''' was on. Throws when there is no session, the file is missing, the form does not appear or
    ''' FOREXE does not answer in time.
    ''' </summary>
    Public Async Function UploadElectronicDocumentAsync(pdfPath As String) As Task(Of String)
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException("Nu există o sesiune FOREXE deschisă.")
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Throw New FileNotFoundException("Fișierul de încărcat nu există.", pdfPath)
            End If

            Dim returnUrl As String = _page.Url
            _logger?.LogOperator($"Deschid «Transmitere documente electronice» pentru {Path.GetFileName(pdfPath)}.")
            Await _page.GotoAsync(ElectronicDocumentsUrl, New PageGotoOptions With {
                .WaitUntil = WaitUntilState.Load, .Timeout = FormWaitSeconds * 1000})

            Dim frame As IFrame = Await FindFrameWithAsync(LinkDocSelector, FormWaitSeconds)
            If frame Is Nothing Then
                Throw New TimeoutException("Pagina «Transmitere documente electronice» nu a arătat câmpul de alegere a fișierului.")
            End If

            Await frame.Locator(LinkDocSelector).First.SetInputFilesAsync(pdfPath)
            Dim send As ILocator = frame.Locator(SendButtonSelector).First
            If Await send.CountAsync() = 0 Then Throw New InvalidOperationException("Butonul «Trimite» nu există în pagină.")

            _logger?.LogOperator("Apăs «Trimite».")
            ' The button posts the form and the frame navigates to FOREXE's answer: wait for that
            ' POST's response, then for the answer page to finish loading.
            Await _page.RunAndWaitForResponseAsync(
                Function() send.ClickAsync(),
                Function(r As IResponse) r.Request.IsNavigationRequest AndAlso
                                         String.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
                New PageRunAndWaitForResponseOptions With {.Timeout = AnswerWaitSeconds * 1000})
            Await frame.WaitForLoadStateAsync(LoadState.Load,
                New FrameWaitForLoadStateOptions With {.Timeout = AnswerWaitSeconds * 1000})

            Dim answer As String = Await frame.EvaluateAsync(Of String)(
                "() => (document.body ? document.body.innerText : '').replace(/\s+\n/g, '\n').trim()")
            answer = If(answer, String.Empty).Trim()
            If answer.Length > MaxAnswerLength Then answer = answer.Substring(0, MaxAnswerLength)
            _logger?.LogOperator("Răspunsul FOREXE: " & If(answer.Length > 200, answer.Substring(0, 200) & "…", answer))

            ' Back where the operator was; a failure here does not undo the upload.
            Try
                If Not String.IsNullOrWhiteSpace(returnUrl) AndAlso
                   Not returnUrl.StartsWith(ElectronicDocumentsUrl, StringComparison.OrdinalIgnoreCase) Then
                    Await _page.GotoAsync(returnUrl)
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("WorkflowExecutor.UploadElectronicDocumentAsync.Return", ex)
            End Try
            Return answer
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.UploadElectronicDocumentAsync", ex)
            Throw
        End Try
    End Function

    ' The first frame (the page itself included) that has an element matching the selector,
    ' polled until the timeout; Nothing when none shows up.
    Private Async Function FindFrameWithAsync(selector As String, timeoutSeconds As Integer) As Task(Of IFrame)
        Dim deadline As DateTime = DateTime.UtcNow.AddSeconds(timeoutSeconds)
        Do
            For Each f As IFrame In _page.Frames
                ' A frame the portal already threw away cannot be asked.
                If f.IsDetached Then Continue For
                If Await f.Locator(selector).CountAsync() > 0 Then Return f
            Next
            If DateTime.UtcNow > deadline Then Return Nothing
            Await Task.Delay(500)
        Loop
    End Function

End Class
