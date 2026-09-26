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
''' Slice 0081-04 -- THE SEND of a DDF revision to forexecab, and the three follow-up actions of
''' the Rezervari footer menu («Definitiveaza», «Deruleaza», «Genereaza PDF final»).
'''
''' <para><b>Order of a send</b> (plan 0081-04): stage 1 («interrupted») is written BEFORE forexecab
''' is touched -> the workflow runs -> whatever it left behind (the real angajament code, the row
''' codes, the captures) is written back AT ONCE, failed run or not -> forexecab's reservations are
''' downloaded and checked against section A -> the existing import runs, unchanged -> stage 2 ->
''' (not a new angajament) the final PDF, uploaded unsigned -> stage 3.</para>
'''
''' <para>Every stop before stage 2 leaves the revision at S1x («Trimitere intrerupta») and the
''' DDF menu offers «Reia trimiterea». A resume never runs «Creare Angajament» a second time once
''' the real code is known; it continues with «Incarca Rezervare» on the lines forexecab does not
''' have yet.</para>
''' </summary>
Partial Public Class KbotForm

    Private Const TitluTrimitere As String = "Trimite în FOREXE"

    ' ── The send ─────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' «Trimite in FOREXE» (S1) / «Reia trimiterea in FOREXE» (S1x) from the DDF view. Called
    ''' through <see cref="ExecutaComandaDdf"/>, which shows any exception.
    ''' </summary>
    Private Async Function TrimiteDdfAsync(cod As String, selectata As RevizieRow) As Task
        If selectata Is Nothing OrElse selectata.Idrev <= 0 OrElse selectata.Iddf <= 0 Then
            KBotMessage.Show(Me, "Selectați o revizie din arbore.", TitluTrimitere,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim sendApi As IDdfSendApi = CereApiulDeTrimitere()
        Dim idrev As Integer = selectata.Idrev
        Dim iddf As Integer = selectata.Iddf

        ' Fresh state: the tree may be minutes old, and a second K-BOT may have sent it meanwhile.
        Dim revizie As RevizieRow = Await RevizieProaspataAsync(cod, idrev)
        If revizie Is Nothing Then
            KBotMessage.Show(Me, "Revizia nu mai există pe server.", TitluTrimitere,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Not DdfRevisionStates.CanSend(revizie.Stare) Then
            KBotMessage.Show(Me, $"Revizia este «{DdfRevisionStates.Label(revizie.Stare)}» și nu se poate trimite." &
                            vbCrLf & vbCrLf & "Se trimite o revizie semnată A pe documentul intermediar " &
                            "(«Semnată A»), sau se reia o trimitere întreruptă.",
                            TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim reluare As Boolean = revizie.Stare = DdfRevisionState.SendInterrupted

        ' Built from the SAVED revision, never from the screen (plan step 1).
        Dim draft As DdfDraft
        busyBar.Running = True
        Try
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        Dim linii As List(Of DdfSendLine) = draft.LiniiA.Select(Function(a) DdfSendLine.FromDraft(a)).ToList()
        If linii.Count = 0 Then
            KBotMessage.Show(Me, "Revizia nu are niciun rând în secțiunea A: nu există nimic de trimis.",
                            TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim codCurent As String = If(draft.CodAngajament, String.Empty).Trim()
        Dim numarRev As Integer = draft.Revizie.NumarRev
        ' A new angajament = the Rev 0 of a document created in K-BOT. FX_DDF.Manual stays 1 after
        ' the «!» code is replaced, so this still holds on a resume.
        Dim angajamentNou As Boolean = draft.Manual AndAlso numarRev = 0
        Dim motiv As String = DdfSendInputs.Motiv(draft.Revizie.DescScurta, numarRev)
        Dim program As String = If(String.IsNullOrWhiteSpace(draft.Program), _session.CodProgram, draft.Program)

        Dim intrebare As String = If(reluare,
            $"Reiau trimiterea reviziei {numarRev} a angajamentului «{codCurent}» în FOREXE?" & vbCrLf & vbCrLf &
            "Se trimit doar rândurile pe care FOREXE nu le are încă.",
            $"Trimit revizia {numarRev} a angajamentului «{codCurent}» în FOREXE?" & vbCrLf & vbCrLf &
            "Din acest moment revizia nu se mai modifică și nu se mai șterge din K-BOT: " &
            "orice schimbare ulterioară cere o revizie nouă.")
        If KBotMessage.Show(Me, intrebare & ModeNote(), TitluTrimitere, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            Return
        End If

        ' The FOREXE session first: stage 1 is written only when a run can actually follow.
        If Not Await _controller.ConnectAsync().ConfigureAwait(True) Then
            AratEsecul("Conectarea la FOREXE")
            Return
        End If
        ' Slice 0081-07: a dry run saves nothing in forexecab, so the revision is not marked as sent.
        If Not _controller.DryRunMode Then
            Await ApelTrimitereAsync(Function() sendApi.IncepeTrimitereaDdfAsync(idrev, CancellationToken.None))
        End If

        ' ── Which workflow ────────────────────────────────────────────────────────
        Dim creare As Boolean = codCurent.StartsWith("!", StringComparison.Ordinal)
        If creare AndAlso reluare Then
            ' The first run stopped before K-BOT learned forexecab's code. Creating again could
            ' make a SECOND angajament -- the operator decides.
            Dim ales As String = Await CodulCreariiIntreruptelorAsync(draft)
            If ales Is Nothing Then Return
            If ales.Length > 0 Then
                codCurent = Await SalveazaCoduriAsync(sendApi, idrev, ales, Nothing, codCurent)
                creare = False
            End If
        End If

        Dim job As JobRequest = Nothing
        If creare Then
            job = JobBuilder.BuildCreareAngajament(draft.ObiectDdf,
                DdfSendInputs.ToJson(DdfSendInputs.CreareRows(linii, program)))
        Else
            Dim deTrimis As List(Of DdfSendLine) = linii
            If reluare Then
                ' What forexecab has already: codes saved, and only the rest is sent again.
                Dim inainte As PrelucrareRezultat = Await DescarcaRezervarileAsync(codCurent)
                If inainte Is Nothing Then
                    AratEsecul("Citirea angajamentului din FOREXE")
                    AnuntaTrimitereaIntrerupta(codCurent, idrev, "FOREXE nu a putut fi citit înainte de reluare.")
                    Return
                End If
                Dim grilaInainte As IReadOnlyList(Of RandTabel) = GrilaIndicatorilor(inainte)
                Await SalveazaCoduriAsync(sendApi, idrev, String.Empty, CoduriRandurilor(grilaInainte, linii), codCurent)
                deTrimis = If(angajamentNou,
                              linii.Where(Function(l) DdfSendInputs.GridRowFor(grilaInainte, l) Is Nothing).ToList(),
                              DdfSendInputs.LinesToResume(grilaInainte, linii))
            End If
            If deTrimis.Count > 0 Then
                ' «Informatii complete contract» is captured on the Rev 0 of an angajament that
                ' came from forexecab already running (plan 0081-03, G2).
                Dim infoComplete As Boolean = numarRev = 0 AndAlso Not draft.Manual AndAlso
                    RezervariMenu.ParseState(StareaAngajamentului(codCurent, draft), codCurent) = ForexeAngajamentState.InDerulare
                job = JobBuilder.BuildIncarcaRezervare(codCurent,
                    DdfSendInputs.ToJson(DdfSendInputs.IncarcaRows(deTrimis, program, motiv)), infoComplete)
            End If
        End If

        ' ── The run, and what it left behind ─────────────────────────────────────
        If job IsNot Nothing Then
            Dim rezultat As JobResult = Await _controller.RuleazaTrimitereAsync(job, codCurent).ConfigureAwait(True)
            If rezultat Is Nothing Then
                ' Nothing started: forexecab is untouched, the stage is 1 and a resume is safe.
                AratEsecul("Trimiterea")
                AnuntaTrimitereaIntrerupta(codCurent, idrev, "Robotul FOREXE nu a pornit.")
                Return
            End If
            If rezultat.StoppedBeforeSave Then
                ReportDryRunStop(rezultat)
                Return
            End If

            If creare Then
                Dim codReal As String = CodulDinRezultat(rezultat)
                If codReal.Length > 0 Then
                    codCurent = Await SalveazaCoduriAsync(sendApi, idrev, codReal,
                        CoduriRandurilor(GrilaDinRezultat(rezultat), linii), codCurent)
                End If
            End If
            Await UrcaCapturileAsync(sendApi, idrev, rezultat)

            If Not rezultat.Success Then
                Dim nota As String = "FOREXE: " & rezultat.Message
                If creare AndAlso codCurent.StartsWith("!", StringComparison.Ordinal) Then
                    nota &= vbCrLf & vbCrLf & "K-BOT nu a apucat să citească codul angajamentului. Verificați în " &
                            "FOREXE dacă angajamentul a fost creat înainte de a relua trimiterea."
                End If
                AnuntaTrimitereaIntrerupta(codCurent, idrev, nota)
                Return
            End If
            If creare AndAlso codCurent.StartsWith("!", StringComparison.Ordinal) Then
                AnuntaTrimitereaIntrerupta(codCurent, idrev,
                    "Angajamentul a fost creat, dar codul lui nu s-a putut citi din pagină. Deschideți " &
                    "angajamentul în vederea «Browser FOREXE» și reluați trimiterea: codul se citește din pagină.")
                Return
            End If
        End If

        ' ── Read forexecab back, check, import ───────────────────────────────────
        Dim pachet As PrelucrareRezultat = Await DescarcaRezervarileAsync(codCurent)
        If pachet Is Nothing Then
            AratEsecul("Citirea rezervărilor din FOREXE")
            AnuntaTrimitereaIntrerupta(codCurent, idrev, "Rezervările nu au putut fi citite înapoi din FOREXE.")
            Return
        End If
        Dim grila As IReadOnlyList(Of RandTabel) = GrilaIndicatorilor(pachet)
        Await SalveazaCoduriAsync(sendApi, idrev, String.Empty, CoduriRandurilor(grila, linii), codCurent)

        Dim diferente As List(Of String) = DdfSendInputs.Differences(grila, linii, angajamentNou)
        If diferente.Count > 0 Then
            AnuntaTrimitereaIntrerupta(codCurent, idrev,
                "FOREXE nu arată ce spune documentul de fundamentare:" & vbCrLf & "  " &
                String.Join(vbCrLf & "  ", diferente))
            Return
        End If

        If Not Await DuLaIngestieAsync(codCurent, pachet) Then
            AnuntaTrimitereaIntrerupta(codCurent, idrev,
                "Rezervările din FOREXE nu au fost salvate în K-BOT. «Reia trimiterea» le citește din nou.")
            Return
        End If
        Await ApelTrimitereAsync(Function() sendApi.SeteazaStareTrimitereDdfAsync(
            idrev, DdfSendStage.SentInProgress, CancellationToken.None))

        If angajamentNou Then
            KBotMessage.Show(Me, $"Angajamentul «{codCurent}» a fost creat în FOREXE." & vbCrLf & vbCrLf &
                            "Continuați din subsolul arborelui Rezervări: «Definitivează», apoi «Derulează», " &
                            "apoi «Generează PDF final».",
                            TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            Await GenereazaPdfFinalAsync(sendApi, codCurent, idrev)
        End If
        Await ArataReviziaAsync(codCurent, idrev)
    End Function

    ''' <summary>
    ''' A resume of a «Creare Angajament» that stopped before its code was read. Returns the code
    ''' to continue with, "" to create the angajament now, or Nothing to stop.
    ''' </summary>
    Private Async Function CodulCreariiIntreruptelorAsync(draft As DdfDraft) As Task(Of String)
        Dim codPagina As String = DdfSendInputs.CodeFromText(
            Await _controller.CitesteCodulPaginiiAsync().ConfigureAwait(True))
        If codPagina.Length > 0 Then
            Dim r As DialogResult = KBotMessage.Show(Me,
                $"Trimiterea anterioară s-a oprit înainte ca K-BOT să afle codul angajamentului." & vbCrLf & vbCrLf &
                $"Pagina FOREXE arată acum angajamentul «{codPagina}». Este cel creat pentru «{draft.ObiectDdf}»?" & vbCrLf & vbCrLf &
                "Da = continui pe el.  Nu = mă opresc.",
                TitluTrimitere, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            Return If(r = DialogResult.Yes, codPagina, Nothing)
        End If

        Dim raspuns As DialogResult = KBotMessage.Show(Me,
            "Trimiterea anterioară s-a oprit înainte ca K-BOT să afle codul angajamentului, deci nu se știe " &
            $"dacă angajamentul «{draft.ObiectDdf}» a apucat să fie creat în FOREXE." & vbCrLf & vbCrLf &
            "Verificați lista de angajamente din FOREXE." & vbCrLf &
            "Da = NU există, îl creez acum." & vbCrLf &
            "Nu = există: mă opresc. Deschideți-l în vederea «Browser FOREXE» și reluați trimiterea; " &
            "codul se citește atunci din pagină.",
            TitluTrimitere, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
        Return If(raspuns = DialogResult.Yes, String.Empty, Nothing)
    End Function

    ''' <summary>The send stopped before stage 2: the revision stays S1x. Says why and reloads.</summary>
    Private Sub AnuntaTrimitereaIntrerupta(cod As String, idrev As Integer, motiv As String)
        KBotMessage.Show(Me, "Trimiterea s-a oprit." & vbCrLf & vbCrLf & motiv & vbCrLf & vbCrLf &
                        "Revizia rămâne «" & DdfRevisionStates.Label(DdfRevisionState.SendInterrupted) &
                        "». Ce s-a apucat de făcut în FOREXE (codurile, capturile) s-a salvat. " &
                        "Din vederea DDF alegeți «Reia trimiterea în FOREXE».",
                        TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        ReincarcaDupaTrimitere(cod, idrev)
    End Sub

    ' Fire-and-forget reload after a stopped send: the message is already shown.
    Private Async Sub ReincarcaDupaTrimitere(cod As String, idrev As Integer)
        Try
            Await ArataReviziaAsync(cod, idrev)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReincarcaDupaTrimitere", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0081-07: the line added to a send question when FOREXE will not really be written --
    ''' the dry run, or answers loaded from «Rezultate_Forexe». Empty in the normal mode.
    ''' </summary>
    Private Function ModeNote() As String
        If _controller.ReplayMode Then
            Return vbCrLf & vbCrLf & "MOD REÎNCĂRCARE: robotul nu intră în FOREXE. Pentru fiecare pas alegeți " &
                   "răspunsul păstrat în «Rezultate_Forexe»; K-BOT îl tratează ca și cum FOREXE ar fi răspuns acum."
        End If
        If _controller.DryRunMode Then
            Return vbCrLf & vbCrLf & "MOD PROBĂ: robotul parcurge paginile FOREXE și se oprește înainte de primul pas " &
                   "care salvează. Nu se salvează nimic în FOREXE, iar revizia nu își schimbă starea."
        End If
        Return String.Empty
    End Function

    ''' <summary>
    ''' Slice 0081-07: a dry run reached a step that saves and stopped there. Nothing was saved in
    ''' forexecab, so nothing is written on the revision either (no codes, no captures, no stage).
    ''' </summary>
    Private Sub ReportDryRunStop(result As JobResult)
        KBotMessage.Show(Me, "Proba s-a încheiat." & vbCrLf & vbCrLf & result.Message & vbCrLf & vbCrLf &
                        "Tot ce era înainte de acest pas a mers pe paginile FOREXE reale. Revizia nu s-a schimbat. " &
                        "Răspunsul probei (cu o captură a paginii din momentul opririi) este în «Rezultate_Forexe».",
                        TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' ── The Rezervari footer menu: Definitiveaza / Deruleaza / Genereaza PDF final ─────

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

    ' ── Helpers ───────────────────────────────────────────────────────────────────

    Private Function CereApiulDeTrimitere() As IDdfSendApi
        Dim api As IDdfSendApi = TryCast(_apiClient, IDdfSendApi)
        If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IDdfSendApi.")
        Return api
    End Function

    ' The IDdfSendApi calls without a result, through the one 401 net.
    Private Function ApelTrimitereAsync(apel As Func(Of Task)) As Task(Of Boolean)
        Return WithReauth(Of Boolean)(
            Async Function()
                Await apel().ConfigureAwait(True)
                Return True
            End Function)
    End Function

    ''' <summary>Writes the codes forexecab gave; returns the angajament code now in force.</summary>
    Private Async Function SalveazaCoduriAsync(sendApi As IDdfSendApi, idrev As Integer, codReal As String,
                                               randuri As List(Of DdfCodRand), codCurent As String) As Task(Of String)
        If String.IsNullOrEmpty(codReal) AndAlso (randuri Is Nothing OrElse randuri.Count = 0) Then Return codCurent
        Dim lista As IReadOnlyList(Of DdfCodRand) = If(randuri, New List(Of DdfCodRand)())
        Dim cod As String = Await WithReauth(Of String)(
            Function() sendApi.SalveazaCoduriDdfAsync(idrev, codReal, lista, CancellationToken.None))
        Return If(String.IsNullOrWhiteSpace(cod), codCurent, cod.Trim())
    End Function

    ''' <summary>The forexecab row code of every line that has one and does not carry it yet.</summary>
    Private Shared Function CoduriRandurilor(grila As IReadOnlyList(Of RandTabel), linii As IEnumerable(Of DdfSendLine)) As List(Of DdfCodRand)
        Dim result As New List(Of DdfCodRand)()
        If grila Is Nothing OrElse grila.Count = 0 Then Return result
        For Each l As DdfSendLine In linii
            Dim code As String = DdfSendInputs.RowCode(grila, l)
            If code.Length = 0 OrElse code.StartsWith("!", StringComparison.Ordinal) Then Continue For
            If String.Equals(code, l.CodIndicator, StringComparison.Ordinal) Then Continue For
            result.Add(New DdfCodRand() With {.IdSecA = l.IdSecA, .CodIndicator = code})
        Next
        Return result
    End Function

    ''' <summary>Every <c>Poza_*</c> of a run, stored on the revision. A capture that fails to upload
    ''' is logged and the rest go on: the run already happened, losing all of them is worse.</summary>
    Private Async Function UrcaCapturileAsync(sendApi As IDdfSendApi, idrev As Integer, rezultat As JobResult) As Task
        Dim capturi As List(Of KeyValuePair(Of String, Byte())) = DdfSendInputs.Captures(rezultat?.Data)
        Dim esuate As Integer = 0
        For Each c As KeyValuePair(Of String, Byte()) In capturi
            Dim nume As String = DdfSendInputs.CaptureFileName(c.Key)
            Dim octeti As Byte() = c.Value
            Try
                Await WithReauth(Of Integer)(
                    Function() sendApi.UrcaCapturaDdfAsync(idrev, nume, octeti, CancellationToken.None))
            Catch ex As Exception
                GlobalErrorLog.Write("MainForm.UrcaCapturileAsync", ex)
                esuate += 1
            End Try
        Next
        If esuate > 0 Then
            KBotMessage.Show(Me, $"{esuate} din {capturi.Count} capturi FOREXE nu s-au putut salva pe revizie. " &
                            "Detalii în jurnalul de erori.", TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Function

    ''' <summary>The angajament code a «Creare Angajament» run read: CodAng_Final, else the last
    ''' CodAng_&lt;Cheie&gt; read after a row save. Empty when none.</summary>
    Private Shared Function CodulDinRezultat(rezultat As JobResult) As String
        If rezultat?.Data Is Nothing Then Return String.Empty
        Dim final As String = Nothing
        If rezultat.Data.TryGetValue(WorkflowCatalog.VarCodAngajamentFinal, final) Then
            Dim c As String = DdfSendInputs.CodeFromText(final)
            If c.Length > 0 Then Return c
        End If
        Dim ultim As String = String.Empty
        For Each kvp As KeyValuePair(Of String, String) In rezultat.Data
            If kvp.Key Is Nothing OrElse Not kvp.Key.StartsWith(WorkflowCatalog.VarCodAngajamentRandPrefix, StringComparison.Ordinal) Then Continue For
            Dim c As String = DdfSendInputs.CodeFromText(kvp.Value)
            If c.Length > 0 Then ultim = c
        Next
        Return ultim
    End Function

    Private Shared Function GrilaDinRezultat(rezultat As JobResult) As IReadOnlyList(Of RandTabel)
        Dim t As TabelRezultat = Nothing
        If rezultat?.Tables IsNot Nothing AndAlso rezultat.Tables.TryGetValue(WorkflowCatalog.TabelIndicatori, t) Then Return t
        Return New List(Of RandTabel)()
    End Function

    Private Shared Function GrilaIndicatorilor(pachet As PrelucrareRezultat) As IReadOnlyList(Of RandTabel)
        Dim t As TabelRezultat = Nothing
        If pachet?.Tabele IsNot Nothing AndAlso pachet.Tabele.TryGetValue(WorkflowCatalog.TabelIndicatori, t) AndAlso t IsNot Nothing Then Return t
        Return New List(Of RandTabel)()
    End Function

    ''' <summary>The reservations download (the same one as the Rezervari refresh icon).</summary>
    Private Async Function DescarcaRezervarileAsync(cod As String) As Task(Of PrelucrareRezultat)
        busyBar.Running = True
        Try
            Return Await _controller.DownloadRezervariAsync(cod).ConfigureAwait(True)
        Finally
            busyBar.Running = False
        End Try
    End Function

    Private Async Function RevizieProaspataAsync(cod As String, idrev As Integer) As Task(Of RevizieRow)
        Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
        Return ddf?.Revizii?.FirstOrDefault(Function(r) r.Idrev = idrev)
    End Function

    ''' <summary>The angajament's open revision (S0, S1, S1x, S2a) with the highest number, or Nothing.</summary>
    Private Async Function RevizieDeschisaAsync(cod As String) As Task(Of RevizieRow)
        Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
        Return ddf?.Revizii?.Where(Function(r) DdfRevisionStates.IsOpen(r.Stare)).
                             OrderByDescending(Function(r) r.NumarRev).FirstOrDefault()
    End Function

    ''' <summary>forexecab's state of the angajament: the tree's row when it has one, else the draft's.</summary>
    Private Function StareaAngajamentului(cod As String, draft As DdfDraft) As String
        Dim info As AngajamentTreeInfo = Nothing
        If _treeInfos.TryGetValue(cod, info) AndAlso info IsNot Nothing Then Return info.Stare
        Return If(draft?.Stare, String.Empty)
    End Function

    ''' <summary>
    ''' Lands the operator on the revision: the tree reloaded onto <paramref name="cod"/> (it may
    ''' be a new code), the DDF view opened and the revision selected.
    ''' </summary>
    Private Async Function ArataReviziaAsync(cod As String, idrev As Integer) As Task
        Await LoadTreeAsync(codDeSelectat:=cod).ConfigureAwait(True)
        If _currentInfo Is Nothing OrElse Not String.Equals(_currentInfo.CodAngajament, cod, StringComparison.OrdinalIgnoreCase) Then Return
        If Not IsViewEnabled("ddf", _currentInfo) Then Return
        If navViews.SelectedKey <> "ddf" Then navViews.SelectedKey = "ddf"
        TryCast(_activeView, DdfView)?.Reincarca(idrev)
    End Function

End Class
