Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
' CertificateSelectionForm e în namespace global (din KBot.Forexe).

''' <summary>
''' Coordonatorul FOREXE (felia 0034) — SINGURUL loc care vorbește cu <see cref="IForexeRunner"/>.
''' Ține ciclul de viață al sesiunii, progresul, starea, anularea, certificatul ales și depozitul
''' local de rezultate. Cele două suprafețe de UI (banda din subsolul shell-ului și fereastra de
''' consolă) sunt PROSTE: se leagă la evenimentele de aici și cheamă intențiile de aici.
'''
''' <para>Singleton, ca runner-ul: starea unei sesiuni de browser nu are ce căuta într-un
''' formular care se poate închide.</para>
''' </summary>
Public NotInheritable Class ForexeController

    Private ReadOnly _runner As IForexeRunner
    Private ReadOnly _session As SessionContext
    Private ReadOnly _store As New WorkflowResultStore()

    ' Un CancellationTokenSource per operație (butonul «Anulează» din consolă îl folosește).
    Private _cts As CancellationTokenSource
    Private _certificat As X509Certificate2
    Private _busy As Boolean
    Private _ultimulProcent As Integer
    Private _ultimaStare As String = String.Empty

    ' Why the LAST intent came back empty, or String.Empty when nothing went wrong.
    ' Slice 0057. A download can return Nothing for two very different reasons: the robot
    ' failed (or the workflow stopped itself because the angajament is not in the FOREXE
    ' list), or the operator simply cancelled the certificate dialog. The shell shows a
    ' message box for the first and stays quiet for the second, so the two must be told
    ' apart -- and a cancel deliberately leaves this EMPTY.
    Private _ultimulEsec As String = String.Empty

    ''' <summary>
    ''' Fereastra-părinte pentru dialogurile modale (alegerea certificatului). O pune
    ''' shell-ul după creare; fără ea dialogul s-ar deschide fără proprietar.
    ''' </summary>
    Public Property Owner As IWin32Window

    Public Sub New(runner As IForexeRunner, session As SessionContext)
        ArgumentNullException.ThrowIfNull(runner)
        ArgumentNullException.ThrowIfNull(session)
        _runner = runner
        _session = session
        AddHandler _runner.StatusUpdated, AddressOf Runner_StatusUpdated
    End Sub

    ' ── Stare ────────────────────────────────────────────────────────────

    ''' <summary>Există o sesiune FOREXE vie (browser deschis + autentificat)?</summary>
    Public ReadOnly Property IsConnected As Boolean
        Get
            Try
                Return _runner.HasLiveSession
            Catch ex As Exception
                GlobalErrorLog.Write("ForexeController.IsConnected", ex)
                Throw
            End Try
        End Get
    End Property

    ''' <summary>O operație e în curs (conectare sau descărcare)?</summary>
    Public ReadOnly Property IsBusy As Boolean
        Get
            Return _busy
        End Get
    End Property

    ''' <summary>Numele simplu al certificatului ales; gol dacă nu s-a ales niciunul.</summary>
    Public ReadOnly Property CertificateName As String
        Get
            Try
                If _certificat Is Nothing Then Return String.Empty
                Return _certificat.GetNameInfo(X509NameType.SimpleName, False)
            Catch ex As Exception
                GlobalErrorLog.Write("ForexeController.CertificateName", ex)
                Throw
            End Try
        End Get
    End Property

    Public ReadOnly Property LastPercent As Integer
        Get
            Return _ultimulProcent
        End Get
    End Property

    Public ReadOnly Property LastStatus As String
        Get
            Return _ultimaStare
        End Get
    End Property

    ''' <summary>
    ''' Why the last intent came back empty, in Romanian, ready to be shown to the operator;
    ''' String.Empty when it succeeded or when the operator cancelled it themselves.
    ''' Set fresh by every intent, so it always describes the most recent one.
    ''' </summary>
    Public ReadOnly Property LastFailure As String
        Get
            Return _ultimulEsec
        End Get
    End Property

    ''' <summary>Depozitul local (memorie + JSON) al rezultatelor descărcate.</summary>
    Public ReadOnly Property Rezultate As WorkflowResultStore
        Get
            Return _store
        End Get
    End Property

    ' ── Evenimente (suprafețele de UI se leagă la ele) ───────────────────

    Public Event StateChanged As EventHandler
    Public Event ProgressChanged As EventHandler(Of Integer)
    Public Event StatusChanged As EventHandler(Of String)

    ' ── Intenții ─────────────────────────────────────────────────────────

    ''' <summary>
    ''' Deschide o sesiune FOREXE: alege certificatul (dialogul existent, PIN manual) și
    ''' rulează workflow-ul «Conectare». Dacă sesiunea e deja vie, nu face nimic.
    ''' Întoarce False dacă operatorul a anulat alegerea certificatului sau conectarea a eșuat.
    ''' </summary>
    Public Async Function ConnectAsync() As Task(Of Boolean)
        Try
            If IsConnected Then Return True
            If _busy Then Return False
            _ultimulEsec = String.Empty

            Dim cert As X509Certificate2 = Nothing
            If NoTokenInDebug() Then
                ' With no token the certificate dialog is pointless: it opens empty and says
                ' so itself. Go straight on, without a certificate.
                RaporteazaStare("Fără token — pornesc sesiunea NEAUTENTIFICATĂ (doar Debug).")
            Else
                cert = SelectCertificate()
                If cert Is Nothing Then Return False   ' anulat / fără certificat
            End If

            IntraInLucru()
            Try
                Dim job As New JobRequest With {
                    .WorkflowName = "Conectare",
                    .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.ConectareFile)
                }
                RaporteazaStare("Conectare la FOREXE...")
                Dim rezultat As JobResult = Await _runner.RunAsync(job, cert, Progres(), _cts.Token)
                If rezultat.Success Then
                    _certificat = cert
                    RaporteazaStare("Conectat.")
                Else
                    RaporteazaEsec("Conectare eșuată: " & rezultat.Message)
                End If
                Return rezultat.Success
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.ConnectAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Descarcă lista de angajamente («adlop - Lista Angajamente Curente.wfl»), o mapează în
    ''' forma de domeniu și o păstrează local (memorie + JSON). NU scrie nimic pe server —
    ''' upsert-ul e o treaptă separată, chemată din meniul de opțiuni al shell-ului.
    ''' Întoarce rândurile mapate, sau Nothing dacă fluxul nu a putut porni ori a eșuat.
    ''' </summary>
    Public Async Function DownloadListaAsync() As Task(Of List(Of Angajament))
        ' Cutia neagră a descărcării (felia 0054): se deschide ÎNAINTE de orice ieșire, ca
        ' până și o cerere respinsă din start să lase o urmă pe disc.
        Dim jurnal As New ForexeRunDump("ListaAngajamente", String.Empty, _session)
        Try
            _ultimulEsec = String.Empty
            If _busy Then
                jurnal.Note("motiv", "O alta operatie FOREXE era deja in curs.")
                ScrieJurnal(jurnal, "ocupat", Nothing)
                RaporteazaEsec("Rulează deja o operație FOREXE — cererea a fost ignorată.")
                Return Nothing
            End If
            If Not Await AsiguraSesiuneAsync() Then
                jurnal.Note("motiv", "Sesiunea FOREXE nu s-a deschis (anulat sau esuat).")
                ScrieJurnal(jurnal, "fara-sesiune", Nothing)
                Return Nothing
            End If

            IntraInLucru()
            Try
                RaporteazaStare("Descarc lista de angajamente...")
                Dim job As JobRequest = JobBuilder.BuildListaAngajamente(_session)
                jurnal.NoteRequest(job)
                Dim rezultat As JobResult = Await _runner.RunJobAsync(job, Progres(), _cts.Token)
                If Not rezultat.Success Then
                    ScrieJurnal(jurnal, "esuat", rezultat)
                    RaporteazaEsec("Lista de angajamente a eșuat: " & rezultat.Message)
                    Return Nothing
                End If

                Dim randuri As TabelRezultat = Nothing
                If Not rezultat.Tables.TryGetValue(WorkflowCatalog.ListaAngajamenteTable, randuri) Then
                    jurnal.Note("tabel_asteptat", WorkflowCatalog.ListaAngajamenteTable)
                    ScrieJurnal(jurnal, "tabel-lipsa", rezultat)
                    RaporteazaEsec($"Tabelul «{WorkflowCatalog.ListaAngajamenteTable}» lipsește din rezultat (0 rânduri).")
                    Return Nothing
                End If

                ' Cheile BRUTE, ca o redenumire în FOREXE să se vadă în jurnal chiar și
                ' atunci când maparea trece — la fel ca pe calea veche a lui btnSinc.
                If randuri.Count > 0 Then
                    RaporteazaStare("Coloane citite: " & String.Join(",", randuri(0).Keys))
                End If

                Dim mapate As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(randuri)
                Dim cale As String = _store.SalveazaLista(mapate)
                ScrieJurnal(jurnal, "ok", rezultat, mapate)
                RaporteazaStare($"{mapate.Count} angajamente mapate (din {randuri.Count} brute) → {Path.GetFileName(cale)}")
                Return mapate
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            jurnal.Note("exceptie", ex.ToString())
            ScrieJurnal(jurnal, "exceptie", Nothing)
            GlobalErrorLog.Write("ForexeController.DownloadListaAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Descarcă un angajament întreg. Oglindește Access <c>FX_Angajament_InfoComplete</c>:
    ''' fără istoric local rulează «Prelucrare Completa», iar cu istoric rulează varianta
    ''' REVERSE pornind de la cea mai recentă <c>DataFX</c> cunoscută. Rezultatul (cinci
    ''' tabele + scalari) se păstrează BRUT — nu există încă mapper de ingestie.
    ''' </summary>
    Public Async Function DownloadNodeAsync(cod As String,
                                            citesteIstoric As Func(Of String, CancellationToken, Task(Of IstoricInfo))) As Task(Of PrelucrareRezultat)
        ' Cutia neagră a descărcării (felia 0054) — vezi DownloadListaAsync.
        Dim jurnal As New ForexeRunDump("PrelucrareCompleta", cod, _session)
        Try
            _ultimulEsec = String.Empty
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If
            If _busy Then
                jurnal.Note("motiv", "O alta operatie FOREXE era deja in curs.")
                ScrieJurnal(jurnal, "ocupat", Nothing)
                RaporteazaEsec($"Rulează deja o operație FOREXE — cererea pentru «{cod}» a fost ignorată.")
                Return Nothing
            End If
            If Not Await AsiguraSesiuneAsync() Then
                jurnal.Note("motiv", "Sesiunea FOREXE nu s-a deschis (anulat sau esuat).")
                ScrieJurnal(jurnal, "fara-sesiune", Nothing)
                Return Nothing
            End If

            IntraInLucru()
            Try
                Dim ultimaData As Date? = Await UltimaDataIstoric(cod, citesteIstoric)
                jurnal.Note("istoric_local",
                            If(ultimaData.HasValue,
                               $"REVERSE de la {ultimaData.Value:yyyy-MM-dd HH:mm:ss}",
                               "fara istoric local -> prelucrare completa"))

                Dim job As JobRequest
                If ultimaData.HasValue Then
                    RaporteazaStare($"Descarc «{cod}» (REVERSE, de la {ultimaData.Value:dd.MM.yyyy HH:mm:ss})...")
                    job = JobBuilder.BuildPrelucrareCompletaReverse(cod, ultimaData.Value)
                Else
                    RaporteazaStare($"Descarc «{cod}» (prelucrare completă)...")
                    job = JobBuilder.BuildPrelucrareCompleta(cod)
                End If
                jurnal.NoteRequest(job)

                Dim rezultat As JobResult = Await _runner.RunJobAsync(job, Progres(), _cts.Token)
                If Not rezultat.Success Then
                    ' Slice 0057. This branch now ALSO catches the flow that stopped
                    ' itself because the angajament is not in the FOREXE list: RunJobAsync
                    ' turns an <Exit> into a failed job, so the download ends HERE, with
                    ' nothing sent to the server, and the shell shows the reason instead of
                    ' the operator reading it in the console while an empty package
                    ' travels on.
                    ScrieJurnal(jurnal, "esuat", rezultat)
                    RaporteazaEsec($"Prelucrarea lui «{cod}» a eșuat: " & rezultat.Message)
                    Return Nothing
                End If

                Dim pachet As PrelucrareRezultat = WorkflowResultStore.DinJobResult(cod, rezultat)
                Dim cale As String = _store.SalveazaNod(cod, pachet)
                Dim total As Integer = pachet.Tabele.Values.Sum(Function(t) t.Count)
                ScrieJurnal(jurnal, "ok", rezultat, pachet)
                RaporteazaStare($"«{cod}»: {pachet.Tabele.Count} tabele, {total} rânduri → {Path.GetFileName(cale)}")
                ' Aici se termină treaba ROBOTULUI: pachetul e pe disc, nimic n-a plecat încă
                ' spre server. Ingestia (propunere ▸ așezare ▸ salvare, felia 0055) pornește din
                ' shell, cu pachetul întors mai jos — coordonatorul aduce datele, shell-ul le
                ' duce mai departe. Se spune pe consolă fiindcă între cele două trepte poate sta
                ' un dialog: dacă operatorul îl închide, descărcarea rămâne doar locală.
                RaporteazaStare($"«{cod}»: descărcarea s-a încheiat — pachetul e local; urmează ingestia.")
                Return pachet
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            jurnal.Note("exceptie", ex.ToString())
            ScrieJurnal(jurnal, "exceptie", Nothing)
            GlobalErrorLog.Write("ForexeController.DownloadNodeAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Downloads the SNM bank statements from FOREXE and keeps them locally -- slice 0057.
    ''' The LEFT icon of the tree footer. Like the list, it writes NOTHING to the server:
    ''' the import is a separate step, started by the shell with what comes back from here.
    ''' </summary>
    ''' <param name="citesteUltimaData">
    ''' The most recent statement date already imported, so a press does not re-download the
    ''' whole FOREXE inbox. Nothing (or a read that failed) means «take everything», which is
    ''' correct, only slower -- the server rejects the duplicates anyway.
    ''' Same shape as <c>citesteIstoric</c> in <see cref="DownloadNodeAsync"/>.
    ''' </param>
    ''' <returns>
    ''' The statements downloaded (an empty list is a real answer: there were no new ones),
    ''' or Nothing when the flow could not start or failed -- in which case
    ''' <see cref="LastFailure"/> says why.
    ''' </returns>
    Public Async Function DownloadExtraseAsync(
            citesteUltimaData As Func(Of CancellationToken, Task(Of Date?))) As Task(Of List(Of ExtrasDescarcat))
        Try
            _ultimulEsec = String.Empty
            If _busy Then
                RaporteazaEsec("Rulează deja o operație FOREXE — cererea de extrase a fost ignorată.")
                Return Nothing
            End If
            If Not Await AsiguraSesiuneAsync() Then Return Nothing

            IntraInLucru()
            Try
                Dim deLa As Date? = Await UltimaDataExtras(citesteUltimaData)
                If deLa.HasValue Then
                    RaporteazaStare($"Descarc extrasele de cont, de la {deLa.Value:dd.MM.yyyy}...")
                Else
                    RaporteazaStare("Descarc extrasele de cont (toată cutia de mesaje)...")
                End If

                Dim folder As String = KBotPaths.FolderExtrase
                Dim extrase As List(Of ExtrasDescarcat) =
                    Await _runner.DescarcaExtraseAsync(folder, deLa, ProgresExtrase(), _cts.Token)

                RaporteazaStare($"{extrase.Count} extrase descărcate în «{folder}».")
                Return extrase
            Finally
                IesDinLucru()
            End Try
        Catch ex As OperationCanceledException
            ' The operator's own cancel: said on the console, but NOT a failure, so the
            ' shell stays quiet about it -- same rule as a cancelled certificate dialog.
            GlobalErrorLog.Write("ForexeController.DownloadExtraseAsync", ex)
            RaporteazaStare("Descărcarea extraselor a fost anulată.")
            Return Nothing
        Catch ex As Exception
            ' Boundary: the robot throws (it has no JobResult to hand back) and it stops
            ' here -- the shell reads the reason from LastFailure and shows it.
            GlobalErrorLog.Write("ForexeController.DownloadExtraseAsync", ex)
            RaporteazaEsec("Descărcarea extraselor a eșuat: " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' The most recent statement date already imported, or Nothing. A read that fails does
    ''' NOT stop the download: it takes the whole inbox (more work, but correct) and says so
    ''' on the console -- never a made-up date. The twin of <c>UltimaDataIstoric</c>.
    ''' </summary>
    Private Async Function UltimaDataExtras(
            citeste As Func(Of CancellationToken, Task(Of Date?))) As Task(Of Date?)
        If citeste Is Nothing Then Return Nothing
        Try
            Return Await citeste(_cts.Token)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.UltimaDataExtras", ex)
            RaporteazaStare($"Nu s-a putut citi data ultimului extras ({ex.Message}) — descarc toată cutia.")
            Return Nothing
        End Try
    End Function

    ' The statements' progress bridge: the robot's counter becomes a percentage for both UI
    ' surfaces, and its status line passes through unchanged. With no total (0) the bar does
    ' not move: a percentage worked out by dividing by zero would be an invented figure.
    Private Function ProgresExtrase() As Action(Of Integer, Integer, String)
        Return Sub(facute As Integer, total As Integer, mesaj As String)
                   If total > 0 Then
                       Dim procent As Integer = CInt(Math.Min(100L, 100L * facute \ total))
                       _ultimulProcent = procent
                       RaiseEvent ProgressChanged(Me, procent)
                   End If
                   RaporteazaStare(mesaj)
               End Sub
    End Function

    ''' <summary>Anulează operația în curs (butonul «Anulează» din consolă).</summary>
    Public Sub Cancel()
        Try
            _cts?.Cancel()
            RaporteazaStare("Anulare cerută...")
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.Cancel", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Browserul FOREXE e la vedere acum?</summary>
    Public ReadOnly Property IsBrowserVisible As Boolean
        Get
            Try
                Return _runner.IsBrowserVisible
            Catch ex As Exception
                GlobalErrorLog.Write("ForexeController.IsBrowserVisible", ex)
                Throw
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Comută vizibilitatea browserului FOREXE. De la felia 0034-02 el PORNEȘTE ascuns
    ''' (stealth, ca în KBOT_IPC), deci butonul din consolă trebuie să meargă în ambele sensuri —
    ''' altfel, o dată arătat, n-ar mai putea fi ascuns la loc.
    ''' </summary>
    Public Async Function ToggleBrowserAsync() As Task
        Try
            If _runner.IsBrowserVisible Then
                Await _runner.HideBrowserAsync()
            Else
                Await _runner.ShowBrowserAsync()
            End If
            RaiseEvent StateChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.ToggleBrowserAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Aduce fereastra browserului FOREXE în față.</summary>
    Public Async Function ShowBrowserAsync() As Task
        Try
            Await _runner.ShowBrowserAsync()
            RaiseEvent StateChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.ShowBrowserAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deschide bancul de înregistrare (K-BOT Recorder, felia 0053) peste sesiunea FOREXE.
    ''' Fereastra e modeless și trăiește în KBot.Forexe, lângă executorul pe care îl andochează;
    ''' aici trece doar intenția și proprietarul dialogului.
    ''' </summary>
    Public Sub ShowRecorder()
        Try
            _runner.ShowRecorder(Owner)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.ShowRecorder", ex)
            Throw
        End Try
    End Sub

    ' ── Interne ──────────────────────────────────────────────────────────

    ''' <summary>
    ''' Cea mai recentă <c>DataFX</c> din istoricul LOCAL al angajamentului, sau Nothing.
    ''' Un eșec de citire NU oprește descărcarea: cade pe fluxul complet (mai mult de lucru,
    ''' dar corect) și spune asta în starea afișată — niciodată o dată inventată.
    ''' </summary>
    Private Async Function UltimaDataIstoric(cod As String,
                                             citesteIstoric As Func(Of String, CancellationToken, Task(Of IstoricInfo))) As Task(Of Date?)
        If citesteIstoric Is Nothing Then Return Nothing
        Try
            Dim info As IstoricInfo = Await citesteIstoric(cod, _cts.Token)
            If info Is Nothing OrElse info.Randuri Is Nothing OrElse info.Randuri.Count = 0 Then Return Nothing
            Dim date_ = info.Randuri.Where(Function(r) r.DataFx.HasValue).Select(Function(r) r.DataFx.Value).ToList()
            If date_.Count = 0 Then Return Nothing
            Return date_.Max()
        Catch ex As Exception
            ' Frontieră de decizie, nu de date: logăm, anunțăm și mergem pe fluxul complet.
            GlobalErrorLog.Write("ForexeController.UltimaDataIstoric", ex)
            RaporteazaStare($"Istoricul local pentru «{cod}» nu s-a putut citi ({ex.Message}) — rulez prelucrarea completă.")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Scrie cutia neagră a unei descărcări și SPUNE unde a ajuns. Calea se anunță pe consolă
    ''' pentru că folderul e tot rostul feliei: operatorul trebuie să-l poată trimite mai
    ''' departe fără să-l caute. Dacă scrierea însăși a eșuat, se spune și asta — un jurnal
    ''' lipsă nu are voie să treacă neobservat.
    ''' </summary>
    Private Sub ScrieJurnal(jurnal As ForexeRunDump, rezultatFinal As String,
                            rezultat As JobResult, Optional mapat As Object = Nothing)
        Dim cale As String = jurnal.Save(rezultatFinal, rezultat, mapat)
        If String.IsNullOrEmpty(cale) Then
            RaporteazaStare("Jurnalul descărcării nu s-a putut scrie — vezi Logs\harness_errors.log.")
        Else
            RaporteazaStare("Jurnalul descărcării: " & cale)
        End If
    End Sub

    ' Deschide sesiunea dacă nu există; False = operatorul a anulat sau conectarea a eșuat.
    Private Async Function AsiguraSesiuneAsync() As Task(Of Boolean)
        If IsConnected Then Return True
        Return Await ConnectAsync()
    End Function

    ''' <summary>
    ''' Debug AND no certificate on the token/smartcard: Connect goes ahead WITHOUT one.
    ''' Authenticating to FOREXE will fail, but the browser stays open (decision A3 in
    ''' <c>ForexeRunner.RunAsync</c>), so docking, the recorder and the Wicket monitor can be
    ''' tried on a machine that does not have the token at hand.
    '''
    ''' <para>Release has no such door: there a missing token stops the connection, as before.
    ''' Neither does Debug when the token IS present - the certificate dialog opens normally
    ''' and an operator cancel stays a cancel.</para>
    ''' </summary>
    Private Shared Function NoTokenInDebug() As Boolean
#If DEBUG Then
        Try
            Return CertificateService.GetSmartcardCertificates().Count = 0
        Catch ex As Exception
            ' The reader itself may be missing from the machine - still "no token".
            GlobalErrorLog.Write("ForexeController.NoTokenInDebug", ex)
            Return True
        End Try
#Else
        Return False
#End If
    End Function

    ''' <summary>Picker de certificat în mod manual de PIN (utilizatorul tastează PIN-ul în dialogul Windows).</summary>
    Private Function SelectCertificate() As X509Certificate2
        Try
            Using dlg As New CertificateSelectionForm(manualPin:=True)
                Dim rezultat As DialogResult = If(Owner Is Nothing, dlg.ShowDialog(), dlg.ShowDialog(Owner))
                If rezultat = DialogResult.OK Then Return dlg.SelectedCertificate
            End Using
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.SelectCertificate", ex)
            Throw
        End Try
    End Function

    ' Puntea de progres dată runner-ului: procentul 0..100 ajunge la ambele suprafețe.
    Private Function Progres() As IProgress(Of Integer)
        Return New Progress(Of Integer)(Sub(p)
                                            _ultimulProcent = p
                                            RaiseEvent ProgressChanged(Me, p)
                                        End Sub)
    End Function

    Private Sub IntraInLucru()
        _cts = New CancellationTokenSource()
        _busy = True
        _ultimulProcent = 0
        RaiseEvent StateChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub IesDinLucru()
        _busy = False
        _cts?.Dispose()
        _cts = Nothing
        RaiseEvent StateChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub RaporteazaStare(mesaj As String)
        _ultimaStare = If(mesaj, String.Empty)
        RaiseEvent StatusChanged(Me, _ultimaStare)
    End Sub

    ''' <summary>
    ''' Says it on the console AND remembers it as the reason the intent came back empty,
    ''' so the shell can put it in front of the operator. Only for things that actually went
    ''' wrong -- an operator cancel goes through RaporteazaStare and leaves LastFailure empty.
    ''' </summary>
    Private Sub RaporteazaEsec(mesaj As String)
        _ultimulEsec = If(mesaj, String.Empty)
        RaporteazaStare(mesaj)
    End Sub

    ' Starea venită din executor (prin runner) merge mai departe neschimbată.
    Private Sub Runner_StatusUpdated(sender As Object, status As String)
        Try
            RaporteazaStare(status)
        Catch ex As Exception
            ' Frontieră de eveniment: un abonat care aruncă nu are voie să oprească robotul.
            GlobalErrorLog.Write("ForexeController.Runner_StatusUpdated", ex)
        End Try
    End Sub

End Class
