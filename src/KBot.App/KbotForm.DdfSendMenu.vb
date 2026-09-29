Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' Slice 0081-04 -- the three follow-up actions of the Rezervari footer menu after a send
''' («Definitiveaza», «Deruleaza», «Genereaza PDF final») and the final PDF itself. Split out of
''' KbotForm.DdfSend.vb in slice 0086; the send is there, the shared helpers in
''' KbotForm.DdfSendHelpers.vb.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' «Definitiveaza» / «Deruleaza»: the new angajament's Rev 0 (S2a) moves one state on in
    ''' forexecab. The reason carries «(REV:0)»; the captures join Rev 0 (plan 0081-04).
    ''' </summary>
    Private Async Function SchimbaStareaAngajamentuluiAsync(info As AngajamentTreeInfo, definitivare As Boolean) As Task
        If info Is Nothing OrElse String.IsNullOrWhiteSpace(info.CodAngajament) Then Return
        Dim sendApi As IDdfSendApi = CereApiulDeTrimitere()
        Dim cod As String = info.CodAngajament
        Dim titlu As String = RezervariMenu.Label(If(definitivare, RezervariMenuOption.Definitiveaza, RezervariMenuOption.Deruleaza))

        Dim rev0 As RevizieRow = Await RevizieDeschisaAsync(cod)
        If rev0 Is Nothing OrElse rev0.NumarRev <> 0 OrElse rev0.Stare <> DdfRevisionState.SentInProgress Then
            KBotMessage.Show(Me, "Acțiunea există doar pentru revizia 0 a unui angajament creat în K-BOT, " &
                            "trimisă în FOREXE și fără PDF final.", titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String = If(definitivare,
            $"Definitivez angajamentul «{cod}» în FOREXE?",
            $"Trec angajamentul «{cod}» în derulare în FOREXE?")
        If KBotMessage.Show(Me, intrebare & ModeNote(), titlu, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            Return
        End If

        Dim motiv As String = DdfSendInputs.Motiv(rev0.DescScurta, 0)
        Dim job As JobRequest = If(definitivare, JobBuilder.BuildDefinitivare(cod, motiv), JobBuilder.BuildDerulare(cod, motiv))
        Dim rezultat As JobResult = Await _controller.RuleazaTrimitereAsync(job, cod).ConfigureAwait(True)
        If rezultat Is Nothing Then
            AratEsecul(titlu)
            Return
        End If
        If rezultat.StoppedBeforeSave Then
            ReportDryRunStop(rezultat)
            Return
        End If
        Await UrcaCapturileAsync(sendApi, rev0.Idrev, rezultat)
        If Not rezultat.Success Then
            KBotMessage.Show(Me, $"«{titlu}» s-a oprit: " & rezultat.Message & vbCrLf & vbCrLf &
                            "Verificați starea angajamentului în FOREXE. Capturile făcute s-au salvat pe revizia 0.",
                            titlu, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' The new state reaches K-BOT through the existing import (FX_Angajamente.Stare), and the
        ' tree reload it ends with re-draws the footer menu.
        Dim pachet As PrelucrareRezultat = Await DescarcaRezervarileAsync(cod)
        If pachet Is Nothing Then
            AratEsecul("Citirea angajamentului din FOREXE")
            Return
        End If
        Await DuLaIngestieAsync(cod, pachet)
    End Function

    ''' <summary>«Genereaza PDF final» from the Rezervari menu: the open S2a revision gets its final PDF.</summary>
    Private Async Function GenereazaPdfFinalDinMeniuAsync(info As AngajamentTreeInfo) As Task
        If info Is Nothing OrElse String.IsNullOrWhiteSpace(info.CodAngajament) Then Return
        Dim sendApi As IDdfSendApi = CereApiulDeTrimitere()
        Dim cod As String = info.CodAngajament
        Dim revizie As RevizieRow = Await RevizieDeschisaAsync(cod)
        If revizie Is Nothing OrElse revizie.Stare <> DdfRevisionState.SentInProgress Then
            KBotMessage.Show(Me, "Nu există o revizie trimisă care să aștepte PDF-ul final.",
                            RezervariMenu.Label(RezervariMenuOption.GenereazaPdfFinal),
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Await GenereazaPdfFinalAsync(sendApi, cod, revizie.Idrev) Then
            Await ArataReviziaAsync(cod, revizie.Idrev)
        End If
    End Function

    ''' <summary>
    ''' The FINAL PDF (section B + the captures, 0081-05), uploaded with no signature
    ''' (<see cref="ApiClient.SemnaturaNiciuna"/>) over the signed interim one, then stage 3
    ''' (S2b, waiting for A, B and the director). False, with the message shown, when it failed:
    ''' the revision stays at stage 2 and the Rezervari menu offers «Genereaza PDF final» again.
    ''' </summary>
    Private Async Function GenereazaPdfFinalAsync(sendApi As IDdfSendApi, cod As String, idrev As Integer) As Task(Of Boolean)
        Dim titlu As String = RezervariMenu.Label(RezervariMenuOption.GenereazaPdfFinal)
        busyBar.Running = True
        Try
            ' Slice 0078-06: a PDF signed on section A is NEVER regenerated (the signature would be
            ' lost). Signed A and B already -> it is the final PDF, only the stage moves. Signed A
            ' only -> the DDF view the caller opens next writes section B into it; the stage moves
            ' when B's signature is uploaded (FinalizeazaPdfDupaSemnareBAsync).
            Dim actuala As RevizieRow = Await RevizieProaspataAsync(cod, idrev)
            If actuala IsNot Nothing AndAlso DdfRevisionStates.HasRole(actuala.Semnatura, DdfRevisionStates.RoleA) Then
                If DdfRevisionStates.HasRole(actuala.Semnatura, DdfRevisionStates.RoleB) Then
                    Await ApelTrimitereAsync(Function() sendApi.SeteazaStareTrimitereDdfAsync(
                        idrev, DdfSendStage.FinalPdf, CancellationToken.None))
                    Return True
                End If
                KBotMessage.Show(Me, "Documentul de pe server este semnat pe Secțiunea A, deci nu se mai generează din nou " &
                                "(semnătura s-ar pierde)." & vbCrLf & vbCrLf &
                                "Secțiunea B, cu codurile din FOREXE, se pune în documentul semnat și se arată acum în " &
                                "vederea documentului de fundamentare. Semnați Secțiunea B: când semnătura ajunge pe server, " &
                                "revizia trece la «PDF final». Fără semnătură nu se salvează nimic.",
                                titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return True
            End If

            Dim generat As DdfPdfGenerator.Rezultat = Await DdfPdfGenerator.GenereazaAsync(
                Function() WithReauth(Of DdfInfo)(
                    Function() _apiClient.GetDdfAsync(cod, CancellationToken.None, pentruGenerare:=True)),
                AddressOf CitesteFisierulDdfAsync, _session, idrev, DdfPdfMode.Final).ConfigureAwait(True)

            Dim continut As Byte() = File.ReadAllBytes(generat.PdfPath)
            ' The sha the server holds now (the signed interim PDF), or «no row».
            Dim shaPrecedent As String = If(String.IsNullOrWhiteSpace(generat.Revizie?.PdfSha256),
                                            ApiClient.ShaFaraRand, generat.Revizie.PdfSha256)
            Await WithReauth(Of PutPdfResponse)(
                Function() _apiClient.UploadDdfPdfAsync(idrev, continut, shaPrecedent, ApiClient.SemnaturaNiciuna,
                                                        Nothing, CancellationToken.None))
            ' Slice 0078-04: the cache still holds the signed interim PDF; left there, the next
            ' open finds it differs from the server and warns «nu era identic». Not fatal if it
            ' fails (Adobe may hold the file): the next open downloads the server copy anyway.
            Try
                SignedPdfFiles.WriteCache(
                    DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, generat.Antet, generat.Revizie.NumarRev),
                    continut)
            Catch ex As Exception
                GlobalErrorLog.Write("MainForm.GenereazaPdfFinalAsync.Cache", ex)
            End Try
            Await ApelTrimitereAsync(Function() sendApi.SeteazaStareTrimitereDdfAsync(
                idrev, DdfSendStage.FinalPdf, CancellationToken.None))
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.GenereazaPdfFinalAsync", ex)
            KBotMessage.Show(Me, "PDF-ul final nu a putut fi generat sau încărcat: " & ex.Message & vbCrLf & vbCrLf &
                            "Trimiterea în FOREXE s-a încheiat; PDF-ul final se poate genera din nou din " &
                            "subsolul arborelui Rezervări («Generează PDF final»).",
                            titlu, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' Slice 0078-06: section B was signed on the document the DDF view built from the A-signed PDF,
    ''' and uploaded. That upload IS the final PDF, so the revision moves to stage 3 -- except for a
    ''' new angajament's Rev 0 whose forexecab state is not «In derulare» yet: there «Definitiveaza»
    ''' / «Deruleaza» still come first (variant a), and «Genereaza PDF final», finding A and B signed,
    ''' only moves the stage. Called through <see cref="ExecutaComandaDdf"/>, which shows any exception.
    ''' </summary>
    Private Async Function FinalizeazaPdfDupaSemnareBAsync(cod As String, selectata As RevizieRow) As Task
        If selectata Is Nothing OrElse String.IsNullOrWhiteSpace(cod) Then Return
        Dim sendApi As IDdfSendApi = CereApiulDeTrimitere()
        Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
        Dim revizie As RevizieRow = ddf?.Revizii?.FirstOrDefault(Function(r) r.Idrev = selectata.Idrev)
        If revizie Is Nothing OrElse revizie.StareTrimitere <> DdfSendStage.SentInProgress Then Return
        If Not DdfRevisionStates.HasRole(revizie.Semnatura, DdfRevisionStates.RoleA) OrElse
           Not DdfRevisionStates.HasRole(revizie.Semnatura, DdfRevisionStates.RoleB) Then Return
        Dim antet As DdfAntet = ddf.AntetDeLucru(revizie.Iddf)
        If antet IsNot Nothing AndAlso antet.Manual AndAlso revizie.NumarRev = 0 AndAlso
           RezervariMenu.ParseState(StareaAngajamentului(cod, Nothing), cod) <> ForexeAngajamentState.InDerulare Then
            Return
        End If
        Await ApelTrimitereAsync(Function() sendApi.SeteazaStareTrimitereDdfAsync(
            revizie.Idrev, DdfSendStage.FinalPdf, CancellationToken.None))
        Await ArataReviziaAsync(cod, revizie.Idrev)
    End Function

    ''' <summary>One attachment's bytes (a capture of the final PDF). Risky boundary: logs and rethrows.</summary>
    Private Async Function CitesteFisierulDdfAsync(idRevAtt As Integer) As Task(Of Byte())
        Try
            Dim r As PdfDownloadResult = Await WithReauth(Of PdfDownloadResult)(
                Function() _apiClient.GetDdfFisierAsync(idRevAtt, String.Empty, CancellationToken.None))
            Return If(r IsNot Nothing AndAlso r.Status = PdfDownloadStatus.Content, r.Bytes, Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CitesteFisierulDdfAsync", ex)
            Throw
        End Try
    End Function

End Class
