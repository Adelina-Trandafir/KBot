Option Strict On
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0088 -- uploading a PDF into FOREXE («Transmitere documente electronice»): the CAB
' correction note. The coordinator is the only one that talks to the runner (slice 0034), so
' the note window and the notes view come through here.
Partial Public NotInheritable Class ForexeController

    ''' <summary>
    ''' Uploads <paramref name="pdfPath"/> into FOREXE on the live session (opening one when there
    ''' is none) and returns FOREXE's answer. Nothing = the upload could not start (busy, replay
    ''' mode, session cancelled or failed): <see cref="LastFailure"/> says why. Throws when the
    ''' upload started and failed; the message is Romanian and says so.
    ''' </summary>
    Public Async Function TrimiteDocumentAsync(pdfPath As String) As Task(Of String)
        Try
            _ultimulEsec = String.Empty
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Throw New FileNotFoundException("Fișierul de încărcat în FOREXE nu există.", pdfPath)
            End If
            If _replayMode Then
                RaporteazaEsec("Mod reîncărcare: FOREXE nu este deschis, documentul nu a fost încărcat.")
                Return Nothing
            End If
            Dim upload As IForexeDocumentUpload = TryCast(_runner, IForexeDocumentUpload)
            If upload Is Nothing Then
                RaporteazaEsec("Această versiune a robotului FOREXE nu poate încărca documente.")
                Return Nothing
            End If
            If _busy Then
                RaporteazaEsec("Rulează deja o operație FOREXE — documentul nu a fost încărcat.")
                Return Nothing
            End If
            If Not Await AsiguraSesiuneAsync() Then
                If String.IsNullOrEmpty(_ultimulEsec) Then _ultimulEsec = "Sesiunea FOREXE nu s-a deschis."
                Return Nothing
            End If

            IntraInLucru()
            Try
                RaporteazaStare($"Încarc «{Path.GetFileName(pdfPath)}» în FOREXE...")
                Dim answer As String = Await upload.UploadElectronicDocumentAsync(pdfPath)
                RaporteazaStare($"«{Path.GetFileName(pdfPath)}» a fost trimis în FOREXE.")
                Return If(answer, String.Empty)
            Catch ex As Exception
                RaporteazaEsec("Încărcarea în FOREXE a eșuat: " & ex.Message)
                Throw New InvalidOperationException("Încărcarea în FOREXE a eșuat: " & ex.Message, ex)
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.TrimiteDocumentAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Slice 0088-04: looks for the FOREXE receipt of the upload registered under
    ''' <paramref name="registrationIndex"/> (opening a session when there is none). Nothing = the
    ''' search could not start (<see cref="LastFailure"/> says why); <c>Found = False</c> = not in the
    ''' inbox yet. Throws when the search started and failed; the message is Romanian.
    ''' </summary>
    Public Async Function CautaRecipisaAsync(registrationIndex As String) As Task(Of ForexeReceipt)
        Try
            _ultimulEsec = String.Empty
            If _replayMode Then
                RaporteazaEsec("Mod reîncărcare: FOREXE nu este deschis, recipisa nu poate fi căutată.")
                Return Nothing
            End If
            Dim upload As IForexeDocumentUpload = TryCast(_runner, IForexeDocumentUpload)
            If upload Is Nothing Then
                RaporteazaEsec("Această versiune a robotului FOREXE nu poate căuta recipise.")
                Return Nothing
            End If
            If _busy Then
                RaporteazaEsec("Rulează deja o operație FOREXE — recipisa nu a fost căutată.")
                Return Nothing
            End If
            If Not Await AsiguraSesiuneAsync() Then
                If String.IsNullOrEmpty(_ultimulEsec) Then _ultimulEsec = "Sesiunea FOREXE nu s-a deschis."
                Return Nothing
            End If

            IntraInLucru()
            Try
                RaporteazaStare($"Caut recipisa pentru indexul {registrationIndex}...")
                Dim receipt As ForexeReceipt = Await upload.FindReceiptAsync(registrationIndex)
                RaporteazaStare(If(receipt IsNot Nothing AndAlso receipt.Found,
                                   $"Recipisa pentru indexul {registrationIndex} a fost descărcată.",
                                   $"Recipisa pentru indexul {registrationIndex} nu este încă în FOREXE."))
                Return receipt
            Catch ex As Exception
                RaporteazaEsec("Căutarea recipisei a eșuat: " & ex.Message)
                Throw New InvalidOperationException("Căutarea recipisei în FOREXE a eșuat: " & ex.Message, ex)
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.CautaRecipisaAsync", ex)
            Throw
        End Try
    End Function

End Class
