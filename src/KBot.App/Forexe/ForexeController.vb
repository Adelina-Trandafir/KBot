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

            Dim cert As X509Certificate2 = SelectCertificate()
            If cert Is Nothing Then Return False   ' anulat / fără certificat

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
                    RaporteazaStare("Conectare eșuată: " & rezultat.Message)
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
        Try
            If _busy Then Return Nothing
            If Not Await AsiguraSesiuneAsync() Then Return Nothing

            IntraInLucru()
            Try
                RaporteazaStare("Descarc lista de angajamente...")
                Dim job As JobRequest = JobBuilder.BuildListaAngajamente(_session)
                Dim rezultat As JobResult = Await _runner.RunJobAsync(job, Progres(), _cts.Token)
                If Not rezultat.Success Then
                    RaporteazaStare("Lista de angajamente a eșuat: " & rezultat.Message)
                    Return Nothing
                End If

                Dim randuri As TabelRezultat = Nothing
                If Not rezultat.Tables.TryGetValue(WorkflowCatalog.ListaAngajamenteTable, randuri) Then
                    RaporteazaStare($"Tabelul «{WorkflowCatalog.ListaAngajamenteTable}» lipsește din rezultat (0 rânduri).")
                    Return Nothing
                End If

                ' Cheile BRUTE, ca o redenumire în FOREXE să se vadă în jurnal chiar și
                ' atunci când maparea trece — la fel ca pe calea veche a lui btnSinc.
                If randuri.Count > 0 Then
                    RaporteazaStare("Coloane citite: " & String.Join(",", randuri(0).Keys))
                End If

                Dim mapate As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(randuri)
                Dim cale As String = _store.SalveazaLista(mapate)
                RaporteazaStare($"{mapate.Count} angajamente mapate (din {randuri.Count} brute) → {Path.GetFileName(cale)}")
                Return mapate
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
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
        Try
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If
            If _busy Then Return Nothing
            If Not Await AsiguraSesiuneAsync() Then Return Nothing

            IntraInLucru()
            Try
                Dim ultimaData As Date? = Await UltimaDataIstoric(cod, citesteIstoric)

                Dim job As JobRequest
                If ultimaData.HasValue Then
                    RaporteazaStare($"Descarc «{cod}» (REVERSE, de la {ultimaData.Value:dd.MM.yyyy HH:mm:ss})...")
                    job = JobBuilder.BuildPrelucrareCompletaReverse(cod, ultimaData.Value)
                Else
                    RaporteazaStare($"Descarc «{cod}» (prelucrare completă)...")
                    job = JobBuilder.BuildPrelucrareCompleta(cod)
                End If

                Dim rezultat As JobResult = Await _runner.RunJobAsync(job, Progres(), _cts.Token)
                If Not rezultat.Success Then
                    RaporteazaStare($"Prelucrarea lui «{cod}» a eșuat: " & rezultat.Message)
                    Return Nothing
                End If

                Dim pachet As PrelucrareRezultat = WorkflowResultStore.DinJobResult(cod, rezultat)
                Dim cale As String = _store.SalveazaNod(cod, pachet)
                Dim total As Integer = pachet.Tabele.Values.Sum(Function(t) t.Count)
                RaporteazaStare($"«{cod}»: {pachet.Tabele.Count} tabele, {total} rânduri → {Path.GetFileName(cale)}")
                Return pachet
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.DownloadNodeAsync", ex)
            Throw
        End Try
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

    ' Deschide sesiunea dacă nu există; False = operatorul a anulat sau conectarea a eșuat.
    Private Async Function AsiguraSesiuneAsync() As Task(Of Boolean)
        If IsConnected Then Return True
        Return Await ConnectAsync()
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
