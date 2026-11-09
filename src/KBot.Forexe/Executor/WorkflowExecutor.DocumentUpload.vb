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

            ' SLICE-0088-10: the portal's frame can still be reloading when it first shows the form (the page's
            ' Load fires before the frame settles): a query that lands in that moment fails with
            ' «Execution context was destroyed». Nothing has been sent yet, so just look again.
            Dim frame As IFrame = Nothing
            Dim send As ILocator = Nothing
            For k_attempt As Integer = 1 To 3
                frame = Await FindFrameWithAsync(LinkDocSelector, FormWaitSeconds)
                If frame Is Nothing Then
                    Throw New TimeoutException("Pagina «Transmitere documente electronice» nu a arătat câmpul de alegere a fișierului.")
                End If
                Try
                    Await frame.WaitForLoadStateAsync(LoadState.Load,
                        New FrameWaitForLoadStateOptions With {.Timeout = FormWaitSeconds * 1000})
                    Await frame.Locator(LinkDocSelector).First.SetInputFilesAsync(pdfPath)
                    send = frame.Locator(SendButtonSelector).First
                    If Await send.CountAsync() = 0 Then Throw New InvalidOperationException("Butonul «Trimite» nu există în pagină.")
                    Exit For
                Catch ex As PlaywrightException When IsNavigationRace(ex) AndAlso k_attempt < 3
                    GlobalErrorLog.Write("WorkflowExecutor.UploadElectronicDocumentAsync.FormReloaded", ex)
                End Try
            Next

            ' A mark on the form's document: once it is gone the frame holds FOREXE's answer.
            Await frame.EvaluateAsync(Of Boolean)("() => { window.__kbotFormMark = true; return true; }")

            _logger?.LogOperator("Apăs «Trimite».")
            ' The button posts the form and the frame navigates to FOREXE's answer: wait for that
            ' POST's response, then read the answer once the new document has loaded.
            ' From the click on, the document may already be in FOREXE: any failure below must say so,
            ' otherwise the operator is told «failed» and sends it a second time.
            Dim answer As String
            Try
                Try
                    Await _page.RunAndWaitForResponseAsync(
                        Function() send.ClickAsync(),
                        Function(r As IResponse) r.Request.IsNavigationRequest AndAlso
                                                 String.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
                        New PageRunAndWaitForResponseOptions With {.Timeout = AnswerWaitSeconds * 1000})
                Catch ex As PlaywrightException When IsNavigationRace(ex)
                    ' The click itself ran into the frame navigating = the form was posted.
                    GlobalErrorLog.Write("WorkflowExecutor.UploadElectronicDocumentAsync.ClickNavigation", ex)
                End Try
                answer = Await ReadAnswerAsync(frame)
            Catch ex As Exception
                GlobalErrorLog.Write("WorkflowExecutor.UploadElectronicDocumentAsync.AfterSend", ex)
                Throw New InvalidOperationException(
                    "Documentul a fost probabil trimis în FOREXE, dar răspunsul nu a putut fi citit (" & ex.Message &
                    "). Verifică în FOREXE înainte să-l trimiți din nou.", ex)
            End Try
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
                Try
                    If Await f.Locator(selector).CountAsync() > 0 Then Return f
                Catch ex As PlaywrightException When IsNavigationRace(ex)
                    ' The frame is navigating right now: ask again on the next round.
                End Try
            Next
            If DateTime.UtcNow > deadline Then Return Nothing
            Await Task.Delay(500)
        Loop
    End Function

    ' The text of the document that replaced the form in the frame (FOREXE's answer). Polled:
    ' the POST's response arrives before the frame commits the new document, so reading at once
    ' would hit the old document, which is then destroyed under the read.
    Private Async Function ReadAnswerAsync(k_formFrame As IFrame) As Task(Of String)
        Dim k_deadline As DateTime = DateTime.UtcNow.AddSeconds(AnswerWaitSeconds)
        Do
            ' The form's frame, or - when the portal threw it away and built the answer in a new
            ' frame (or in the page itself) - the live frames, child frames first.
            Dim k_candidates As New List(Of IFrame)
            If Not k_formFrame.IsDetached Then
                k_candidates.Add(k_formFrame)
            Else
                For Each k_f As IFrame In _page.Frames
                    If k_f.IsDetached OrElse k_f Is _page.MainFrame Then Continue For
                    k_candidates.Add(k_f)
                Next
                k_candidates.Add(_page.MainFrame)
            End If
            For Each k_target As IFrame In k_candidates
                Try
                    Dim k_text As String = Await k_target.EvaluateAsync(Of String)(
                        "() => (document.readyState !== 'complete' || window.__kbotFormMark) ? null : " &
                        "(document.body ? document.body.innerText : '').replace(/\s+\n/g, '\n').trim()")
                    If k_text IsNot Nothing Then Return k_text
                Catch ex As PlaywrightException When IsNavigationRace(ex)
                    ' Still navigating: look again on the next round.
                End Try
            Next
            If DateTime.UtcNow > k_deadline Then
                Throw New TimeoutException("FOREXE nu a răspuns la încărcarea documentului în timp util (pagina: " &
                                           _page.Url & ", cadrul formularului " &
                                           If(k_formFrame.IsDetached, "a fost înlocuit", "este încă formularul") & ").")
            End If
            Await Task.Delay(300)
        Loop
    End Function

    ' Playwright's errors for «the page moved under the query» (as opposed to a real failure).
    Private Shared Function IsNavigationRace(k_ex As Exception) As Boolean
        If k_ex Is Nothing Then Return False
        Dim k_message As String = k_ex.Message
        Return k_message.IndexOf("Execution context was destroyed", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               k_message.IndexOf("Frame was detached", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               k_message.IndexOf("Cannot find context with specified id", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

End Class
