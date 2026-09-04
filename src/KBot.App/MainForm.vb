Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Controls
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming
' RichTextBoxLogger și CertificateSelectionForm sunt în namespace global (din KBot.Forexe).

''' <summary>
''' Shell-ul principal K-BOT — echivalentul Access frmFX_MAIN (DOAR el; Meniul rămâne
''' un concept separat). Trei coloane: navigația vederilor (stânga), arborele de
''' angajamente (mijloc) și vederea activă (dreapta). Vederile sunt UserControl-uri
''' create lazy (PlaceholderView în această felie); starea nodului selectat circulă
''' ca AngajamentTreeInfo, nu ca textbox-uri ascunse.
''' </summary>
Public Class MainForm

    Private ReadOnly _forexeRunner As IForexeRunner
    Private ReadOnly _session As SessionContext
    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _loginFactory As Func(Of LoginForm)
    ''' <summary>
    ''' Logger-ul FOREXE. Shell-ul îl CONSTRUIEȘTE și îl atașează runner-ului, dar NU scrie
    ''' niciodată în el: consola FOREXE arată exact ce spune robotul din <c>KBot.Forexe</c>,
    ''' nimic altceva. Până în felia 0040 shell-ul își punea aici și treburile lui (temă
    ''' comutată, vedere creată lazy, arbore încărcat, perioade, SS memorat) — un jurnal în
    ''' care nu mai găseai pașii robotului printre ele. Cine are de raportat o eroare de shell
    ''' cheamă <c>GlobalErrorLog.Write</c>; cine are de spus ceva operatorului o spune în UI.
    ''' </summary>
    Private _forexeLogger As RichTextBoxLogger
    Private _cts As CancellationTokenSource

    ' Catalogul an / SS / CodProgram al bazei curente (din /api/auth/periods).
    Private _periods As IReadOnlyList(Of PeriodInfo)
    ' Suprima logica din SelectedIndexChanged cat timp umplem combo-urile programatic
    ' (setarea DataSource / SelectedIndex declanseaza evenimentele).
    Private _suppressPeriodEvents As Boolean

    ' Vederile create lazy (cheie -> instanță); una singură e vizibilă.
    Private ReadOnly _views As New Dictionary(Of String, IAngajamentView)()
    Private _activeView As IAngajamentView
    ' Contextul selecției curente din arbore (Nothing = nimic selectat / nod de capitol).
    Private _currentInfo As AngajamentTreeInfo
    ' NodeKey -> info, reconstruit la fiecare LoadTree.
    Private ReadOnly _treeInfos As New Dictionary(Of String, AngajamentTreeInfo)()
    ' Opțiunea btnOpt: arată și angajamentele ASCUNS (implicit nu).
    Private _includeHidden As Boolean
    ' Fereastra nemodală «Informații interne» (flag-urile Are* ale nodului selectat).
    ' Nothing / IsDisposed = închisă; se re-deschide la nevoie.
    Private _infoForm As InternalInfoForm
    ' Fereastra nemodală de jurnale, deschisă din meniul butonului de opțiuni (felia 0031-04).
    ' Nothing / IsDisposed = închisă; se re-deschide la nevoie, ca _infoForm.
    Private _logViewer As LogViewerForm

    ' Coordonatorul FOREXE (felia 0034) — singurul care vorbește cu runner-ul. Banda din
    ' subsol și consola se leagă la el; shell-ul nu mai orchestrează nimic singur.
    Private ReadOnly _forexe As ForexeController
    ' Consola FOREXE: creată O SINGURĂ DATĂ și doar ascunsă la închidere, fiindcă rtbLog-ul
    ' ei e ținta logger-ului pe toată durata aplicației (vezi EnsureConsole).
    Private _console As ForexeConsoleForm
    ' Istoricul acțiunilor FOREXE (felia 0040): creat la prima cerere, ascuns la închidere.
    Private _istoricForexe As ForexeHistoryForm

    Public Sub New(forexeRunner As IForexeRunner, session As SessionContext,
                   apiClient As IApiClient, authApi As IAuthApi, loginFactory As Func(Of LoginForm),
                   forexe As ForexeController)
        InitializeComponent()
        _forexeRunner = forexeRunner
        _session = session
        _apiClient = apiClient
        _authApi = authApi
        _loginFactory = loginFactory
        _forexe = forexe
        Me.Text = "K-BOT"
    End Sub

    ''' <summary>
    ''' Rulează un apel autentificat. La 401 (sesiune expirată / plafon absolut)
    ''' redeschide LoginForm; dacă operatorul se re-autentifică, reia apelul O SINGURĂ
    ''' dată cu token-ul proaspăt din SessionContext (singleton — aceeași instanță pe
    ''' care o citește ApiClient). Orice alt eșec, sau un al doilea 401, se propagă.
    ''' Un CONTEXT_MISMATCH (403) se oprește scurt — vezi IsContextMismatch.
    ''' </summary>
    Private Async Function WithReauth(Of T)(action As Func(Of Task(Of T))) As Task(Of T)
        ' Fără plasă proprie: 401-ul e control-flow (re-login), iar orice alt eșec e deja
        ' logat (GlobalErrorLog) + arătat de apelant (LoadTreeAsync / SincronizeazaAsync).
        ' VB.NET nu permite Await într-un Catch: capturăm 401-ul și continuăm sub Try.
        Dim expired As ApiException
        Try
            Return Await action().ConfigureAwait(True)
        Catch ex As ApiException When IsContextMismatch(ex)
            Throw ContextMismatchError(ex)
        Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401
            expired = ex
        End Try

        Using login As LoginForm = _loginFactory()
            If login.ShowDialog(Me) <> DialogResult.OK Then
                Throw expired   ' operatorul a anulat re-login-ul; propagăm 401-ul original
            End If
        End Using

        ' Login-ul a repopulat _session.Token (aceeași instanță citită de ApiClient).
        ' O SINGURĂ reîncercare. Un al doilea 401 imediat după un login proaspăt NU e o
        ' expirare normală — e un defect de server (token respins de îndată). Nu mai
        ' trimitem operatorul iar în bucla de login: îi spunem clar ce s-a întâmplat.
        Try
            Return Await action().ConfigureAwait(True)
        Catch ex2 As ApiException When IsContextMismatch(ex2)
            Throw ContextMismatchError(ex2)
        Catch ex2 As ApiException When ex2.StatusCode.HasValue AndAlso ex2.StatusCode.Value = 401
            Dim reason As String = If(ex2.Reason, String.Empty)
            If reason = "TOKEN_UNKNOWN" Then
                Throw New ApiException(
                    "Autentificare reușită, dar serverul a respins imediat sesiunea (« " & reason & " »). " &
                    "Este un defect de server, nu o sesiune expirată — contactați administratorul.",
                    401, reason)
            End If
            Throw   ' alt motiv de 401 (ex. expirare reală) — propagăm neschimbat
        End Try
    End Function

    ''' <summary>
    ''' CONTEXT_MISMATCH = token VIU, dar folosit pe alt context decât cel al sesiunii
    ''' (ex. alt db_name). Serverul îl întoarce cu 403, NU cu 401 (vezi guard.reject /
    ''' auth_periods), tocmai fiindcă sesiunea e validă — deci re-login-ul nu repară
    ''' nimic și ar trimite operatorul într-o buclă inutilă. Îl tratăm separat de calea
    ''' de 401, la ORICE apel, nu doar după re-login.
    ''' </summary>
    Private Shared Function IsContextMismatch(ex As ApiException) As Boolean
        Return ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 403 AndAlso
               String.Equals(ex.Reason, "CONTEXT_MISMATCH", StringComparison.Ordinal)
    End Function

    ' Mesaj clar pentru operator: nu e sesiune expirată, e o nepotrivire de unitate.
    Private Shared Function ContextMismatchError(ex As ApiException) As ApiException
        Return New ApiException(
            "Cererea a fost respinsă: sesiunea este deschisă pe altă unitate decât cea cerută " &
            "(« CONTEXT_MISMATCH »). Este un defect, nu o sesiune expirată — contactați administratorul.",
            403, ex.Reason)
    End Function

    Private Async Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Logger FOREXE (felia 0034): ținta VIZIBILĂ e caseta din consola FOREXE, iar
            ' liniile merg ȘI în <AppDir>\Logs. Consola se construiește AICI, o singură dată,
            ' și rămâne ascunsă până o cere operatorul — RichTextBoxLogger cere controlul la
            ' construcție și îl ține cât trăiește aplicația, deci ținta trebuie să existe
            ' înainte de prima acțiune FOREXE, nu abia când se deschide fereastra.
            Dim logDir As String = LogPaths.LogsDirectory()
            Directory.CreateDirectory(logDir)
            Dim caleJurnal As String = Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt")

            Try
                EnsureConsole()
                _console.CaleJurnal = caleJurnal
                _forexeLogger = New RichTextBoxLogger(_console.LogBox) With {
                    .EnableUI = True,
                    .LogFilePath = caleJurnal
                }
            Catch ex As Exception
                MessageBox.Show(Me, "Nu s-a putut crea logger-ul FOREXE: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ' Logger-ul e esențial pentru a vedea progresul și erorile din fluxurile FOREXE; fără el, shell-ul nu poate funcționa.
                Close()
            End Try


            Try
                ' Atașează logger-ul FOREXE la runner (aceeași instanță singleton)
                DirectCast(_forexeRunner, ForexeRunner).AttachLogger(_forexeLogger)
            Catch ex As Exception
                MessageBox.Show(Me, "Nu s-a putut atașa logger-ul la runner: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Close()
            End Try

            ' Identitate: caption + bara de status (din SessionContext).
            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), "K-BOT", "K-BOT — " & _session.NumeUnitate)
            'lblUnit.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), String.Empty, _session.NumeUnitate)
            lblOperator.Text = If(String.IsNullOrEmpty(_session.OperatorName), String.Empty, _session.OperatorName)

            ' Banda FOREXE din subsol: dialogurile coordonatorului (alegerea certificatului)
            ' primesc shell-ul ca proprietar, iar banda se leagă la coordonator.
            _forexe.Owner = Me
            forexeFooter.Bind(_forexe)

            ' «Conectare» stă acum în antet, nu în bandă. Butonul e al shell-ului, dar starea lui
            ' vine tot de la coordonator: ne abonăm o singură dată aici și ne dezabonăm la
            ' închidere (coordonatorul e singleton și ar ține formularul în viață).
            AddHandler _forexe.StateChanged, AddressOf Forexe_StateChanged
            ActualizeazaButonConectare()

            ' Navigația vederilor — ordinea paginilor din Access, Sumar implicit.
            ' Cele opt intrări (cinci butoane Near, separator Far, DDF/ORD Far) sunt AUTORITE
            ' ÎN DESIGNER, în `navViews.Items` (felia 0025): vezi MainForm.Designer.vb. Nu se
            ' mai adaugă nimic din cod — un AddItem aici ar lovi aruncarea pe cheie duplicată
            ' la prima rulare.
            ' Fiecare cheie (mai puțin „sumar") e poarta unui flag Are* din arbore:
            ' vezi ApplyViewGating. Sumar rămâne mereu activ (nu are flag).
            ' Rămân de adăugat, când vederile lor vor exista: „indicatori", „revizii", „partener".
            ' Selecția inițială RĂMÂNE în cod, deliberat: atribuirea de aici e cea care ridică
            ' SelectionChanged și deci creează prima vedere. În designer ar fi o valoare moartă.
            navViews.SelectedKey = "sumar"   ' declanșează SelectionChanged -> creează vederea

            ' Fără nod selectat nu se știe ce date există: toate vederile cu flag pornesc
            ' închise, nu deschise-și-goale.
            ApplyViewGating(Nothing)

            ' Lista de angajamente: controlul „tree" e configurat ca listă plată cu coloane
            ' (caption = Descriere, coloană = CodAngajament, iconiță de status stânga,
            ' refresh la hover în dreapta). Datele reale vin din GET /api/forexe/tree.
            'ConfigureAngajamenteList()

            ' Combo-urile an / SS ȘI lista se umplu doar cu o sesiune autentificată (calea
            ' Release trece prin login; în harness-ul Debug fereastra se poate deschide fără
            ' login — atunci arătăm un eșantion mic ca shell-ul să fie testabil vizual).
            If _session.IsAuthenticated AndAlso Not String.IsNullOrEmpty(_session.DbName) Then
                Await LoadPeriodsAsync()
                Await LoadTreeAsync()
            Else
                ' Fără sesiune (posibil doar în harness-ul Debug, care poate deschide shell-ul
                ' fără login): fără date, fără eșantion tăcut — lista rămâne goală, onest.
                cboAn.Enabled = False
                cboSs.Enabled = False
                ' Combo-urile dezactivate SPUN deja povestea; consola FOREXE nu e jurnalul
                ' shell-ului, iar cazul e oricum doar al harness-ului Debug.
            End If
        Catch ex As Exception
            ' Boundary UI (Load): async Sub nu poate rearunca — logăm și înghițim.
            GlobalErrorLog.Write("MainForm.MainForm_Load", ex)
        End Try
    End Sub

    ' ---------------- perioade (an / SS / CodProgram) ----------------

    ''' <summary>
    ''' Aduce catalogul an / SS / CodProgram al bazei curente și umple combo-urile.
    ''' Anul implicit e cel mai mare; SS-ul pornește din LastSS (dacă e valabil în acel
    ''' an), altfel primul. Un eșec de citire nu blochează fereastra — doar dezactivează.
    ''' </summary>
    Private Async Function LoadPeriodsAsync() As Task
        Try
            Try
                _periods = Await _authApi.GetPeriodsAsync(_session.Token, _session.DbName, CancellationToken.None)
            Catch ex As Exception
                ' Nu blocăm fereastra, dar nu înghițim tăcut: arătăm operatorului DE CE sunt
                ' dezactivate combo-urile an/SS (mesajul român al serverului, nu JSON brut).
                ' Detaliul complet merge în jurnalul de erori al shell-ului, nu în consola FOREXE.
                GlobalErrorLog.Write("MainForm.LoadPeriodsAsync.GetPeriods", ex)
                cboAn.Enabled = False
                cboSs.Enabled = False
                MessageBox.Show(Me,
                    "Nu s-au putut citi perioadele (an/SS): " & ex.Message,
                    "Perioade", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try

            If _periods Is Nothing OrElse _periods.Count = 0 Then
                ' Unitatea nu are perioade configurate — nu e o eroare, e o configurare lipsă.
                ' Combo-urile dezactivate o arată; nu e treaba consolei FOREXE.
                cboAn.Enabled = False
                cboSs.Enabled = False
                Return
            End If

            _suppressPeriodEvents = True
            Dim years = _periods.Select(Function(p) p.AN).Distinct().OrderByDescending(Function(y) y).ToList()
            cboAn.DataSource = years
            cboAn.SelectedIndex = 0            ' cel mai mare an
            _suppressPeriodEvents = False

            LoadSsForSelectedYear()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadPeriodsAsync", ex)
            Throw
        End Try
    End Function

    ' Umple SS-urile anului selectat; preselectează LastSS dacă există în acel an.
    Private Sub LoadSsForSelectedYear()
        Try
            If _periods Is Nothing OrElse cboAn.SelectedItem Is Nothing Then Return
            Dim an As Integer = CInt(cboAn.SelectedItem)
            Dim ssList = _periods.Where(Function(p) p.AN = an).
                                  Select(Function(p) p.SS).Distinct().ToList()

            _suppressPeriodEvents = True
            cboSs.DataSource = ssList
            Dim idx As Integer = If(String.IsNullOrEmpty(_session.LastSS), -1, ssList.IndexOf(_session.LastSS))
            cboSs.SelectedIndex = If(idx >= 0, idx, If(ssList.Count > 0, 0, -1))
            _suppressPeriodEvents = False

            ApplySelectedPeriod(persist:=False)   ' nu re-salvam valoarea deja memorata
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadSsForSelectedYear", ex)
            Throw
        End Try
    End Sub

    ' Fixează perioada pe sesiune din selecția curentă; opțional o memorează pe server.
    Private Sub ApplySelectedPeriod(persist As Boolean)
        Try
            If _periods Is Nothing OrElse cboAn.SelectedItem Is Nothing OrElse cboSs.SelectedItem Is Nothing Then Return
            Dim an As Integer = CInt(cboAn.SelectedItem)
            Dim ss As String = CStr(cboSs.SelectedItem)
            Dim row As PeriodInfo = _periods.FirstOrDefault(Function(p) p.AN = an AndAlso p.SS = ss)
            If row Is Nothing Then Return

            ' CodProgram nu mai are etichetă proprie în subsol (locul lui l-a luat banda
            ' FOREXE, felia 0034) — rămâne pe sesiune, de unde îl citește JobBuilder.
            _session.SetPeriod(an, ss, row.CodProgram)
            If persist Then PersistLastSs(ss)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplySelectedPeriod", ex)
            Throw
        End Try
    End Sub

    ' Fire-and-forget: memorarea SS-ului nu trebuie să blocheze utilizatorul; un eșec
    ' se logează. Async Sub cu try/catch = nu poate dărâma aplicația.
    Private Async Sub PersistLastSs(ss As String)
        Try
            Await _authApi.SaveLastSsAsync(_session.Token, ss, CancellationToken.None)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PersistLastSs", ex)
        End Try
    End Sub

    ' Schimbarea anului reface SS-urile anului (care fixează perioada) și RE-CITEȘTE
    ' arborele: an-ul e filtru pe server, deci datele vechi nu mai sunt valabile.
    Private Async Sub CboAn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAn.SelectedIndexChanged
        Try
            If _suppressPeriodEvents Then Return
            LoadSsForSelectedYear()
            Await LoadTreeAsync()
        Catch ex As Exception
            ' Boundary UI: un handler nu poate rearunca (ar dărâma procesul) — logăm și înghițim.
            GlobalErrorLog.Write("MainForm.cboAn_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' Idem pentru SS (filtru pe server, prin EXISTS pe FX_Indicatori.SS).
    Private Async Sub CboSs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSs.SelectedIndexChanged
        Try
            If _suppressPeriodEvents Then Return
            ApplySelectedPeriod(persist:=True)
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.cboSs_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' ---------------- vederi (navigație stânga) ----------------

    Private Sub NavViews_SelectionChanged(key As String) Handles navViews.SelectionChanged
        Try
            ActivateView(key)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.navViews_SelectionChanged", ex)
        End Try
    End Sub

    ' Creează vederea la prima activare (lazy), o arată și îi împinge contextul curent.
    Private Sub ActivateView(key As String)
        Try
            Dim view As IAngajamentView = Nothing
            If Not _views.TryGetValue(key, view) Then
                view = CreateView(key)
                Dim ctrl As System.Windows.Forms.Control = DirectCast(view, System.Windows.Forms.Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                viewHost.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _views(key) = view
            End If

            Dim previous As IAngajamentView = _activeView
            _activeView = view
            DirectCast(view, System.Windows.Forms.Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, view) Then
                DirectCast(previous, System.Windows.Forms.Control).Visible = False
            End If
            ' Doar vederea ACTIVĂ primește contextul; celelalte îl primesc la activare.
            view.SetContext(_currentInfo)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ActivateView", ex)
            Throw
        End Try
    End Sub

    Private Function CreateView(key As String) As IAngajamentView
        Try
            Select Case key
                ' Prima vedere reală (felia 0011). Primește clientul API + plasa 401 a
                ' shell-ului, ca politica de re-login să rămână într-un singur loc.
                Case "sumar" : Return New SumarView(_apiClient, Function(op) WithReauth(Of SumarInfo)(op))
                Case "indicatori" : Return New PlaceholderView(key, "Indicatori")
                Case "istoric" : Return New IstoricView(_apiClient, Function(op) WithReauth(Of IstoricInfo)(op))
                Case "revizii" : Return New PlaceholderView(key, "Revizii")
                ' The "+" icon on the reservations tree asks for a DDF on that reservation --
                ' Access: fxRezervari_AdaugaRevizie in frmFX_MAIN. It goes through the SAME
                ' ExecutaComandaDdf as DdfView: one re-login policy, one place where the editor
                ' opens.
                Case "rezervari" : Return New RezervariView(_apiClient, Function(op) WithReauth(Of RezervariInfo)(op),
                                                            AddressOf ExecutaComandaDdf)
                Case "partener" : Return New PlaceholderView(key, "Partener")
                Case "receptii" : Return New ReceptiiView(_apiClient, Function(op) WithReauth(Of ReceptiiInfo)(op),
                                                         AddressOf DeschideLegaturileReceptiilor)
                ' «+» din arborele de plati cere ordonantarea zilei / lotul lunii — Access:
                ' fxPlati_AdaugareOrdonantare / fxPlati_AdaugareOrdonantari din frmFX_MAIN.
                ' Trece prin ACEEASI ExecutaComandaOrd ca OrdView: o singura politica de
                ' re-login, un singur loc unde se deschide editorul.
                Case "plati" : Return New PlatiView(_apiClient, Function(op) WithReauth(Of PlatiInfo)(op),
                                                    AddressOf ExecutaComandaOrd)
                Case "ddf" : Return New DdfView(_apiClient, Function(op) WithReauth(Of DdfInfo)(op), _session,
                                                AddressOf ExecutaComandaDdf)
                Case "ord" : Return New OrdView(_apiClient, Function(op) WithReauth(Of OrdInfo)(op), _session,
                                                AddressOf ExecutaComandaOrd)
                Case Else
                    Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CreateView", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deschide editorul de legături recepție ▸ instantaneu (felia 0048-04), MODAL.
    '''
    ''' <para>Trăiește aici, nu în vedere, dintr-un singur motiv: formularul are nevoie de plasa
    ''' de re-autentificare pe DOUĂ forme de răspuns, iar <c>WithReauth</c> e privat și generic
    ''' în shell. Vederea primește doar acțiunea asta, deci politica de re-login rămâne, ca peste
    ''' tot, într-un singur loc.</para>
    '''
    ''' <para>D-I: modal. După o salvare, recepțiile se reîncarcă — legăturile schimbate mută
    ''' anteturile între recepții, deci ce a rămas pe ecran nu mai e adevărat.</para>
    ''' </summary>
    Private Sub DeschideLegaturileReceptiilor(cod As String)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return
            Using f As New AsociereForm(_apiClient, cod,
                                        Function(op) WithReauth(Of AsociereStare)(op),
                                        Function(op) WithReauth(Of AsociereRezultat)(op))
                f.ShowDialog(Me)
                If f.SAuSalvatModificari Then
                    Dim vedere As ReceptiiView = TryCast(_activeView, ReceptiiView)
                    vedere?.Reincarca()
                End If
            End Using
        Catch ex As Exception
            ' Graniță de UI: se loghează și se arată; un throw de aici ar cădea pe firul de UI.
            GlobalErrorLog.Write("MainForm.DeschideLegaturileReceptiilor", ex)
            MessageBox.Show(Me, "Editorul de legături nu a putut fi deschis. Detalii în jurnalul de erori.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════════════
    ' EDITORUL DE ORDONANTARE (felia 0049) — cele patru puncte de intrare
    '
    ' Traiesc AICI, nu in vedere, dintr-un singur motiv: fiecare are nevoie de plasa de
    ' re-autentificare pe una sau mai multe forme de raspuns, iar `WithReauth` e privat si
    ' generic in shell. `OrdView` primeste o singura actiune si ramane read-only; politica
    ' de re-login ramane, ca peste tot, intr-un singur loc. Acelasi tipar ca
    ' `DeschideLegaturileReceptiilor` (felia 0048-04).
    ' ══════════════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Executa o comanda de scriere ceruta de <c>OrdView</c>. Granita de UI: se logheaza si
    ''' se arata; un throw de aici ar cadea pe firul de interfata.
    ''' </summary>
    Private Async Sub ExecutaComandaOrd(comanda As OrdComanda)
        Try
            If comanda Is Nothing OrElse String.IsNullOrWhiteSpace(comanda.Cod) Then Return

            Select Case comanda.Actiune
                Case OrdActiune.Adauga
                    Await AdaugaOrdonantareAsync(comanda.Cod, comanda.Ziua, comanda.IdPlataFx).ConfigureAwait(True)
                Case OrdActiune.Modifica : Await ModificaOrdonantareAsync(comanda.Ordonantare).ConfigureAwait(True)
                Case OrdActiune.Sterge : Await StergeOrdonantareAsync(comanda.Ordonantare).ConfigureAwait(True)
                Case OrdActiune.Lot
                    Await GenereazaInLotAsync(comanda.Cod, comanda.Luna, comanda.An).ConfigureAwait(True)
                Case Else
                    ' Fara no-op-uri tacute: o actiune necunoscuta e un defect de programare.
                    Throw New ArgumentException($"Acțiune ORD necunoscută: {comanda.Actiune}", NameOf(comanda))
            End Select
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaComandaOrd", ex)
            MessageBox.Show(Me, ex.Message, "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaComandaOrd", ex)
            MessageBox.Show(Me, "Comanda nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Adauga» — genereaza graful pe server (nimic scris) si deschide editorul. Portul lui
    ''' <c>FX_Adaugare_ORD_Din_Plati</c>. Avertismentul de peste 25 de parteneri vine de la
    ''' server, in <c>Avertismente</c>, si il arata formularul.
    '''
    ''' <para><paramref name="ziCeruta"/> <c>Nothing</c> = ziua NU e stiuta si se cere
    ''' operatorului; asa intra <c>OrdView</c>, care n-are din ce s-o deduca. Cand comanda vine
    ''' de pe «+»-ul arborelui de plati, ziua e chiar nodul apasat, deci nu se mai intreaba —
    ''' exact ca in Access, unde <c>fxPlati_AdaugareOrdonantare</c> primea <c>vDataPlata</c>
    ''' gata ales.</para>
    '''
    ''' <para><paramref name="idPlataFx"/> <c>Nothing</c> = toate platile neordonantate ale
    ''' zilei (VBA: <c>vIdPlataFX = -1</c> ▸ <c>sIdPlataFX = "*"</c>). Frunza arborelui de
    ''' plati e ZIUA, nu plata, deci azi nimeni nu trimite o plata anume; parametrul exista
    ''' fiindca ruta il accepta si fiindca nivelul de plata al Access-ului poate reveni.</para>
    ''' </summary>
    Private Async Function AdaugaOrdonantareAsync(cod As String,
                                                  Optional ziCeruta As Date? = Nothing,
                                                  Optional idPlataFx As Integer? = Nothing) As Task
        Dim zi As Date? = If(ziCeruta.HasValue, ziCeruta, CereZiua(cod))
        If Not zi.HasValue Then Return

        busyBar.Running = True
        Dim draft As OrdDraft
        Try
            draft = Await WithReauth(Of OrdDraft)(
                Function() _apiClient.GenereazaOrdAsync(cod, zi.Value, idPlataFx, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulOrd(draft)
    End Function

    ''' <summary>
    ''' «Modifica» — incarca ordonantarea selectata in forma editorului si o deschide.
    '''
    ''' <para>Se citeste prin <c>GET /api/forexe/ord/draft/{idordp}</c>, NU prin apelul
    ''' vederii: acela e al lui <c>OrdView</c> si isi alege coloanele deliberat (felia 0033) —
    ''' nu intoarce CodAI, CodIndicator, IdClsf, CodSSI, explicatia, partenerul liniei,
    ''' randurile de document una cate una sau legaturile cu platile, toate necesare editarii.
    ''' `routes/forexe/ord.py` ramane neatinsa.</para>
    ''' </summary>
    Private Async Function ModificaOrdonantareAsync(ordonantare As OrdHeaderRow) As Task
        If ordonantare Is Nothing OrElse ordonantare.Idordp <= 0 Then
            MessageBox.Show(Me, "Selectați o ordonanțare din arbore.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        busyBar.Running = True
        Dim draft As OrdDraft
        Try
            Dim idordp As Integer = ordonantare.Idordp
            draft = Await WithReauth(Of OrdDraft)(
                Function() _apiClient.GetOrdDraftAsync(idordp, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulOrd(draft)
    End Function

    ''' <summary>Deschide editorul MODAL si, la salvare, reincarca vederea pe documentul scris.</summary>
    Private Sub DeschideEditorulOrd(draft As OrdDraft)
        If draft Is Nothing Then Return

        Using f As New OrdEditForm(_apiClient, draft,
                                   Function(op) WithReauth(Of OrdSaveRezultat)(op),
                                   Function(op) WithReauth(Of PutAtasamentResponse)(op),
                                   Function(op) WithReauth(Of PdfDownloadResult)(op),
                                   Function(op) WithReauth(Of Integer)(op))
            f.ShowDialog(Me)
            If f.SAuSalvatModificari Then
                ' Ce a ramas pe ecran nu mai e adevarat: liniile s-au schimbat, platile
                ' acoperite s-au schimbat, iar o ordonantare noua nici macar nu era acolo.
                DupaScriereaOrdonantarii(f.IdordpSalvat)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' «Sterge» — confirmarea numeste numarul, data si totalul, si SPUNE ce mai pleaca odata
    ''' cu documentul: PDF-ul stocat pe server si legaturile cu platile (care se intorc astfel
    ''' in rezerva de neordonantate). Stergerea propriu-zisa e un singur DELETE pe antet;
    ''' cascadele bazei duc restul.
    ''' </summary>
    Private Async Function StergeOrdonantareAsync(ordonantare As OrdHeaderRow) As Task
        If ordonantare Is Nothing OrElse ordonantare.Idordp <= 0 Then
            MessageBox.Show(Me, "Selectați o ordonanțare din arbore.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim data As String = If(ordonantare.DataOrd.HasValue,
                                ordonantare.DataOrd.Value.ToString("dd.MM.yyyy"), "fără dată")
        Dim intrebare As String =
            $"Ștergeți ordonanțarea nr. {ordonantare.NrOrd} din {data}, în valoare de " &
            $"{ordonantare.TotalOrd.ToString("N2", ro)} lei?" & vbCrLf & vbCrLf &
            "Odată cu ea se șterg beneficiarii, rândurile de plată, documentele justificative, " &
            "atașamentele și PDF-ul semnat stocat pe server." & vbCrLf &
            "Plățile acoperite redevin neordonanțate."

        If MessageBox.Show(Me, intrebare, "Șterge ordonanțarea",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As OrdStergereRezultat
        Try
            Dim idordp As Integer = ordonantare.Idordp
            rez = Await WithReauth(Of OrdStergereRezultat)(
                Function() _apiClient.DeleteOrdAsync(idordp, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        MessageBox.Show(Me,
            $"Ordonanțarea nr. {rez.NrOrd} a fost ștearsă." & vbCrLf &
            $"Beneficiari: {rez.Parteneri} · rânduri de plată: {rez.Linii} · " &
            $"documente: {rez.Documente} · atașamente: {rez.Atasamente} · PDF: {rez.Pdf}." & vbCrLf &
            $"Plăți redevenite neordonanțate: {rez.PlatiEliberate}.",
            "Șterge ordonanțarea", MessageBoxButtons.OK, MessageBoxIcon.Information)

        ' Stergerea nu poate APRINDE AreORD (poate doar sa-l stinga, iar cate au mai ramas nu
        ' stim de aici) — deci poarta nu se atinge.
        DupaScriereaOrdonantarii(maiExistaOrdonantari:=False)
    End Function

    ''' <summary>
    ''' «Generare in lot» — portul lui <c>FX_Adaugare_ORD_Din_Plati_Batch</c>, restructurat.
    '''
    ''' <para>Bucla VBA reinteroga zilele cu plati neordonantate si se oprea cand lista se
    ''' golea, fiindca fiecare ORD salvat isi scotea platile din multimea candidata prin
    ''' <c>FX_ORD_TBL_REC</c>. Forma se pastreaza: se cer zilele, iar pentru fiecare se cheama
    ''' genereaza ▸ salveaza, fara formular si fara interactiune.</para>
    '''
    ''' <para><b>La prima eroare se OPRESTE</b>, se spune care zi a picat si cate au reusit —
    ''' exact ca VBA-ul, si pentru acelasi motiv: o bucla nesupravegheata care merge mai
    ''' departe dupa un esec produce o mizerie pe care nimeni n-o mai poate reconstitui.</para>
    '''
    ''' <para>NU exista o tranzactie uriasa care sa cuprinda tot lotul: o ordonantare, o
    ''' tranzactie.</para>
    '''
    ''' <para><paramref name="luna"/> / <paramref name="an"/> <c>Nothing</c> = tot
    ''' angajamentul; asa intra <c>OrdView</c>. Cand comanda vine de pe «+»-ul unei LUNI din
    ''' arborele de plati, lotul se margineste la luna aceea — VBA: <c>vLunaAn</c>, care in
    ''' Access era textul «luna/an» pus in <c>LIKE</c>. Aici sunt doi parametri numerici,
    ''' fiindca <c>*</c> nu e metacaracter in MariaDB (vezi felia 0049 §1.4).</para>
    ''' </summary>
    Private Async Function GenereazaInLotAsync(cod As String,
                                               Optional luna As Integer? = Nothing,
                                               Optional an As Integer? = Nothing) As Task
        busyBar.Running = True
        Dim zile As OrdZileInfo
        Try
            zile = Await WithReauth(Of OrdZileInfo)(
                Function() _apiClient.GetOrdZileAsync(cod, luna, an, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        ' Numele perioadei, pentru mesaje: fara luna/an e tot angajamentul.
        Dim perioada As String = If(luna.HasValue AndAlso an.HasValue,
                                    $" în {NumeLuna(luna.Value)} {an.Value}", String.Empty)

        If zile Is Nothing OrElse zile.Zile.Count = 0 Then
            MessageBox.Show(Me, $"Nu există plăți neordonanțate pentru {cod}{perioada}.",
                            "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            $"Se generează ordonanțări pentru {zile.Zile.Count} zile cu plăți neordonanțate{perioada} " &
            $"({zile.TotalEstimat} ordonanțări estimate)." & vbCrLf & vbCrLf &
            "Fiecare zi se salvează separat, fără să vă mai fie cerută confirmarea." & vbCrLf &
            "La prima eroare, generarea se oprește. Continuați?"
        If MessageBox.Show(Me, intrebare, "Generare în lot",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Dim reusite As Integer = 0
        Dim ziEsuata As String = Nothing
        Dim motiv As String = Nothing

        busyBar.Running = True
        Try
            For Each zi As OrdZiCandidat In zile.Zile
                Dim data As Date = zi.Data
                Try
                    Dim draft As OrdDraft = Await WithReauth(Of OrdDraft)(
                        Function() _apiClient.GenereazaOrdAsync(cod, data, Nothing, CancellationToken.None))
                    Await WithReauth(Of OrdSaveRezultat)(
                        Function() _apiClient.SaveOrdAsync(draft, CancellationToken.None))
                    reusite += 1
                Catch ex As Exception
                    GlobalErrorLog.Write("MainForm.GenereazaInLotAsync", ex)
                    ziEsuata = data.ToString("dd.MM.yyyy")
                    motiv = ex.Message
                    Exit For
                End Try
            Next
        Finally
            busyBar.Running = False
        End Try

        If ziEsuata Is Nothing Then
            MessageBox.Show(Me, $"{reusite} ordonanțări au fost generate și salvate.",
                            "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show(Me,
                $"Generarea s-a oprit la data {ziEsuata}." & vbCrLf &
                $"Motiv: {motiv}" & vbCrLf & vbCrLf &
                $"Până acolo s-au salvat {reusite} ordonanțări; ele RĂMÂN salvate. " &
                "Rezolvați cauza și reluați — zilele deja acoperite nu se mai propun.",
                "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

        ' Poarta se aprinde doar daca lotul chiar a scris ceva.
        DupaScriereaOrdonantarii(maiExistaOrdonantari:=reusite > 0)
    End Function

    ''' <summary>
    ''' Cere operatorului ziua pentru care se genereaza ordonantarea. Implicit: ziua de azi.
    ''' <c>Nothing</c> daca operatorul a renuntat.
    ''' </summary>
    Private Function CereZiua(cod As String) As Date?
        Using dlg As New OrdZiuaForm(cod)
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return Nothing
            Return dlg.Ziua
        End Using
    End Function

    ' Numele lunii in romana, cu prima litera mare — doar pentru mesajele lotului.
    Private Shared Function NumeLuna(luna As Integer) As String
        If luna < 1 OrElse luna > 12 Then Return CStr(luna)
        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim nume As String = ro.DateTimeFormat.GetMonthName(luna)
        If String.IsNullOrEmpty(nume) Then Return CStr(luna)
        Return Char.ToUpper(nume(0), ro) & nume.Substring(1)
    End Function

    ''' <summary>
    ''' Ce se reimprospateaza dupa orice SCRIERE de ordonantare (adaugare, modificare,
    ''' stergere, lot). Portul liniilor de dupa <c>FX_Adaugare_ORD_*</c> din
    ''' <c>frmFX_MAIN</c>: acolo urmau <c>RefreshTreeQuery</c> si
    ''' <c>fxPlati.RefreshPlati CodAngajament, True</c>.
    '''
    ''' <para>Trei lucruri, in ordinea asta:</para>
    ''' <list type="number">
    ''' <item>vederea ORD, daca e cea activa — documentul scris trebuie sa apara;</item>
    ''' <item>vederea PLATI, daca a fost deschisa — platile acoperite nu mai sunt
    ''' neordonantate, deci «+»-ul sta pe o zi gresita si starea nodurilor minte. Se
    ''' reimprospateaza chiar daca nu e vederea activa: altfel ar minti tacut pana la
    ''' urmatoarea schimbare de angajament;</item>
    ''' <item>poarta vederilor: prima ordonantare a unui angajament ii aprinde
    ''' <c>AreORD</c>. Se ridica LOCAL, nu prin re-citirea arborelui mare — un
    ''' <c>LoadTreeAsync</c> de aici ar goli selectia si ar arunca operatorul inapoi pe
    ''' «sumar» exact dupa ce si-a salvat documentul. Flagul se corecteaza oricum de la
    ''' server la urmatoarea schimbare de an/SS sau la urmatoarea descarcare.</item>
    ''' </list>
    ''' </summary>
    Private Sub DupaScriereaOrdonantarii(Optional idordpSalvat As Integer? = Nothing,
                                         Optional maiExistaOrdonantari As Boolean = True)
        Try
            TryCast(_activeView, OrdView)?.Reincarca(If(idordpSalvat, 0))

            Dim vederePlati As IAngajamentView = Nothing
            If _views.TryGetValue("plati", vederePlati) Then
                TryCast(vederePlati, PlatiView)?.Reincarca()
            End If

            If maiExistaOrdonantari AndAlso _currentInfo IsNot Nothing AndAlso Not _currentInfo.AreORD Then
                _currentInfo.AreORD = True
                ApplyViewGating(_currentInfo)
            End If
        Catch ex As Exception
            ' Granita de UI: reimprospatarea a esuat, dar SCRIEREA a reusit deja. Se
            ' logheaza si se merge mai departe — un throw de aici ar face sa para ca
            ' salvarea a picat.
            GlobalErrorLog.Write("MainForm.DupaScriereaOrdonantarii", ex)
        End Try
    End Sub


    ' ══════════════════════════════════════════════════════════════════════════════════
    ' THE DDF EDITOR (slice 0051)
    '
    ' The same shape as `ExecutaComandaOrd` above: the view asks, the shell executes. Every
    ' call goes through `WithReauth`, which is private and generic here, so the re-login
    ' policy stays in one place.
    ' ══════════════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Executes one write command of the fundamentation document.
    '''
    ''' <para><c>Adauga</c> and <c>AdaugaRevizieInitiala</c> are asked for by the "+" icon on
    ''' the RESERVATIONS tree (<c>RezervariView.Tree_RightIconClicked</c>) -- which is where
    ''' Access asked for them too: <c>fxRezervari_AdaugaRevizie</c> in <c>frmFX_MAIN</c>.
    ''' Which of the two is decided there, from the reservation's <c>EInitiala</c>, exactly as
    ''' <c>cNode.Value2</c> decided it in <c>frmFX_MAIN_REZ</c>. They had no caller when slice
    ''' 0051 shipped (decision D20); they have one now.</para>
    ''' </summary>
    Private Async Sub ExecutaComandaDdf(comanda As DdfComanda)
        Try
            If comanda Is Nothing OrElse String.IsNullOrWhiteSpace(comanda.Cod) Then Return

            Select Case comanda.Actiune
                Case DdfActiune.Adauga
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=False).ConfigureAwait(True)
                Case DdfActiune.AdaugaRevizieInitiala
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=True).ConfigureAwait(True)
                Case DdfActiune.Modifica
                    Await ModificaDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeRevizie
                    Await StergeRevizieDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.Sterge
                    Await StergeDocumentDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeLuna
                    Await StergeLunaDdfAsync(comanda).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an unknown action is a programming defect.
                    Throw New ArgumentException($"Acțiune DDF necunoscută: {comanda.Actiune}", NameOf(comanda))
            End Select
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            MessageBox.Show(Me, ex.Message, "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            MessageBox.Show(Me, "Comanda nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Adauga» -- generates the graph on the server (NOTHING is written) and opens the
    ''' editor. The port of <c>FX_Adaugare_DDF</c>.
    '''
    ''' <para><paramref name="rev0"/> selects the HEADER treatment: the initial revision, which
    ''' also creates the document, or a subsequent one on a document that exists. It does NOT
    ''' select the line source -- the server takes that from the data, and refuses loudly when
    ''' neither source has rows.</para>
    '''
    ''' <para>Called from the "+" icon on the reservations tree; see
    ''' <see cref="ExecutaComandaDdf"/>.</para>
    ''' </summary>
    Private Async Function AdaugaDdfAsync(cod As String, rev0 As Boolean) As Task
        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GenereazaDdfAsync(cod, rev0, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulDdf(draft)
    End Function

    ''' <summary>
    ''' «Modifica» -- loads the selected revision in the editor's shape and opens it.
    '''
    ''' <para>Read through <c>GET /api/forexe/ddf/draft/{iddf}/{idrev}</c>, NOT through the
    ''' view's call: that one is the read-only view's and chooses its columns deliberately --
    ''' it returns neither the section-A keys and classifications, nor the section-B rows, nor
    ''' the attachment rows, all of which editing needs.</para>
    ''' </summary>
    Private Async Function ModificaDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Idrev <= 0 OrElse revizie.Iddf <= 0 Then
            MessageBox.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            Dim iddf As Integer = revizie.Iddf
            Dim idrev As Integer = revizie.Idrev
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulDdf(draft)
    End Function

    ''' <summary>Opens the editor MODALLY and, on a save, reloads the view onto what was written.</summary>
    Private Sub DeschideEditorulDdf(draft As DdfDraft)
        If draft Is Nothing Then Return

        ' Seven specialisations of the 401 net, named rather than positional: `WithReauth` is
        ' private and generic, so the form needs one closure per response shape, and seven
        ' identical-looking delegates in a row is an argument order nobody gets right twice.
        Dim reauth As New DdfEditReauth(
            Function(op) WithReauth(Of DdfSaveRezultat)(op),
            Function(op) WithReauth(Of PutDdfFisierResponse)(op),
            Function(op) WithReauth(Of PdfDownloadResult)(op),
            Function(op) WithReauth(Of DdfNumarLock)(op),
            Function(op) WithReauth(Of List(Of String))(op),
            Function(op) WithReauth(Of List(Of DdfPartener))(op),
            Function(op) WithReauth(Of List(Of DdfClasificatie))(op))

        Using f As New DdfEditForm(_apiClient, draft, reauth)
            f.ShowDialog(Me)
            If f.SAuSalvatModificari Then
                ' What is still on screen is no longer true: the lines changed, the reservations
                ' consumed changed, and a new revision was not there at all.
                DupaScriereaDdf(f.IdrevSalvat)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' «Sterge revizia» -- the port of <c>FX_Stergere_Revizie</c>. The confirmation names the
    ''' revision, the way Access's <c>FX_Info_DDF</c> summary did.
    ''' </summary>
    Private Async Function StergeRevizieDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Idrev <= 0 Then
            MessageBox.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea Reviziei curente?" & vbCrLf & vbCrLf &
            RezumatRevizie(revizie) & vbCrLf & vbCrLf &
            "Odată cu ea se șterg rândurile din secțiunile A și B și fișierele atașate." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If MessageBox.Show(Me, intrebare, "Șterge revizia",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim idrev As Integer = revizie.Idrev
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfRevizieAsync(idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge revizia")
        DupaScriereaDdf()
    End Function

    ''' <summary>
    ''' «Sterge documentul» -- the port of <c>FX_Stergere_DDF</c>.
    '''
    ''' <para>Refused while any ordonantare points at the document. That refusal exists twice
    ''' over: <c>FX_ORD.IDDF</c> is a RESTRICT foreign key, so the database would stop it
    ''' anyway -- the server's guard is the readable version of the same "no", and it arrives
    ''' here as an <see cref="ApiException"/> with the Romanian message.</para>
    ''' </summary>
    Private Async Function StergeDocumentDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Iddf <= 0 Then
            MessageBox.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea Documentului curent?" & vbCrLf & vbCrLf &
            RezumatRevizie(revizie) & vbCrLf & vbCrLf &
            "Se șterg TOATE reviziile documentului, cu rândurile și fișierele lor." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If MessageBox.Show(Me, intrebare, "Șterge documentul",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim iddf As Integer = revizie.Iddf
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfAsync(iddf, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge documentul")
        DupaScriereaDdf()
    End Function

    ''' <summary>
    ''' «Sterge TOATE reviziile lunii» -- the port of <c>FX_Stergere_Revizii</c>.
    '''
    ''' <para>When the month holds EVERY revision the document has, the server deletes the
    ''' DOCUMENT instead of the last revision and says so in the result. That is Access's
    ''' behaviour, kept -- an empty document pointing at nothing is worse than none.</para>
    ''' </summary>
    Private Async Function StergeLunaDdfAsync(comanda As DdfComanda) As Task
        If comanda.Iddf <= 0 OrElse comanda.An <= 0 OrElse comanda.Luna < 1 OrElse comanda.Luna > 12 Then
            MessageBox.Show(Me, "Selectați o lună din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea tuturor Reviziilor din luna selectată?" & vbCrLf & vbCrLf &
            $"Luna {comanda.Luna:00}.{comanda.An}, angajamentul {comanda.Cod}." & vbCrLf & vbCrLf &
            "Dacă luna conține toate reviziile documentului, se șterge documentul întreg." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If MessageBox.Show(Me, intrebare, "Șterge reviziile lunii",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim iddf As Integer = comanda.Iddf
            Dim an As Integer = comanda.An
            Dim luna As Integer = comanda.Luna
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfLunaAsync(iddf, an, luna, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge reviziile lunii")
        DupaScriereaDdf()
    End Function

    ''' <summary>The <c>FX_Info_DDF</c> summary, built client-side from what the view already
    ''' holds rather than fetched: every field in it is already on the revision row.</summary>
    Private Shared Function RezumatRevizie(revizie As RevizieRow) As String
        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim data As String = If(revizie.DataRev.HasValue,
                                revizie.DataRev.Value.ToString("dd.MM.yyyy"), "fără dată")
        Dim descriere As String = If(String.IsNullOrWhiteSpace(revizie.DescScurta),
                                     "fără descriere", revizie.DescScurta)
        Return $"Revizia nr. {revizie.NumarRev} din {data}, în valoare de " &
               $"{revizie.TotalRevizie.ToString("N2", ro)} lei." & vbCrLf & descriere
    End Function

    ''' <summary>Reports what actually went, with real counts rather than a bare "done".</summary>
    Private Sub AratatRezultatulStergerii(rez As DdfStergereRezultat, titlu As String)
        If rez Is Nothing Then Return
        Dim ce As String = If(rez.DocumentSters, "Documentul de fundamentare a fost șters.",
                                                 "Reviziile au fost șterse.")
        MessageBox.Show(Me,
            ce & vbCrLf &
            $"Revizii: {rez.Revizii} · secțiunea A: {rez.LiniiA} · secțiunea B: {rez.LiniiB} · " &
            $"fișiere: {rez.Atasamente}." & vbCrLf &
            $"Rezervări redevenite fără DDF: {rez.RezervariEliberate}.",
            titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' Refreshes what a DDF write invalidated. The reservations view is refreshed too: a save
    ''' marks reservations as having a DDF and a delete releases them again, so what it shows
    ''' is no longer true either.
    ''' </summary>
    Private Sub DupaScriereaDdf(Optional idrevSalvat As Integer = 0)
        Try
            TryCast(_activeView, DdfView)?.Reincarca(idrevSalvat)

            ' The reservations view too: a save marks reservations as having a DDF and a delete
            ' releases them again, so the "+" icon that view draws -- now the trigger for the
            ' editor -- is no longer where it belongs. Only the ACTIVE view is refreshed:
            ' `ActivateView` calls `SetContext` on every activation, so an inactive one reloads
            ' by itself when the operator switches to it.
            TryCast(_activeView, RezervariView)?.Reincarca()
        Catch ex As Exception
            ' UI boundary: the refresh failed, but the WRITE already succeeded. Logged, and we
            ' carry on -- a throw here would make it look as though the save had failed.
            GlobalErrorLog.Write("MainForm.DupaScriereaDdf", ex)
        End Try
    End Sub

    ' ---------------- lista de angajamente ----------------

    ' Lățimea coloanei CodAngajament (px). Restul spațiului îl ia captionul (Descriere).
    Private Const COD_COLUMN_WIDTH As Integer = 140

    ''' <summary>
    ''' Încarcă arborele de la GET /api/forexe/tree pentru perioada selectată (an + SS),
    ''' via WithReauth — aceeași cale unică de re-login pe 401. Baza nu se trimite:
    ''' serverul o ia din sesiune (o bază = o unitate). Busy-bar pe durata apelului;
    ''' erorile se arată operatorului în română, niciodată înghițite și niciodată
    ''' mascate cu un arbore gol.
    ''' </summary>
    Private Async Function LoadTreeAsync() As Task
        ' Fără an/SS nu există interogare de făcut (combo-uri goale = perioade necitite).
        If cboAn.SelectedItem Is Nothing OrElse cboSs.SelectedItem Is Nothing Then
            Return
        End If

        Dim an As Integer = CInt(cboAn.SelectedItem)
        Dim ss As String = CStr(cboSs.SelectedItem)

        busyBar.Running = True
        Try
            Dim ct As CancellationToken = CancellationToken.None
            Dim rows As IReadOnlyList(Of AngajamentTreeInfo) =
                Await WithReauth(Of IReadOnlyList(Of AngajamentTreeInfo))(
                    Function() _apiClient.GetTreeAsync(an, ss, _includeHidden, ct))
            PopulateTree(rows)
        Catch ex As Exception
            ' Fără plasă tăcută: o eroare (server oprit / 401 sesiune moartă / defect de
            ' server după re-login) se arată operatorului cu motivul întors de server,
            ' nu se maschează cu un arbore gol — acela ar minți că unitatea n-are date.
            GlobalErrorLog.Write("MainForm.LoadTreeAsync", ex)
            MessageBox.Show(Me,
                "Nu s-a putut încărca arborele de angajamente: " & ex.Message,
                "Angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' Golește și repopulează lista + dicționarul Cod -> info. Fiecare rând: caption =
    ''' Descriere (upper), celulă CodAngajament, iconiță de status (din Stare), refresh la
    ''' hover, bold dacă are surse (comportament legacy), tooltip = Descriere, Tag = Cod.
    ''' </summary>
    Private Sub PopulateTree(rows As IReadOnlyList(Of AngajamentTreeInfo))
        Try
            ArgumentNullException.ThrowIfNull(rows)
            tree.Clear()
            _treeInfos.Clear()
            _currentInfo = Nothing
            ' Selecția veche a dispărut odată cu rândurile: nicio vedere nu rămâne
            ' deschisă pe un angajament care nu mai e în arbore.
            ApplyViewGating(Nothing)
            RefreshInfoForm()   ' selecția s-a golit -> fereastra de info reflectă asta

            For Each info As AngajamentTreeInfo In rows
                Dim cod As String = If(info.CodAngajament, String.Empty)
                Dim caption As String = If(info.Descriere, String.Empty).Trim().ToUpperInvariant()

                Dim node As AdvancedTreeControl.TreeItem =
                    tree.AddItem("D_" & cod, caption,
                                 pLeftIconClosed:=FxIcons.StatusIcon(info.Stare),
                                 pRightIcon:=FxIcons.RefreshIcon(),
                                 pTag:=cod)

                node.Cells("CodAngajament") = New AdvancedTreeControl.TreeItem.CellData With {.Value = cod}
                node.Bold = info.AreIndicatori   ' legacy: îngroșare = are surse (indicatori)
                node.Tooltip = If(info.Descriere, String.Empty)
                'node.ShowRightIconOnHover = True

                _treeInfos(cod) = info
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PopulateTree", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Poarta vederilor: fiecare flag Are* comandă exact o intrare din navigație.
    ''' Fără nod selectat (info = Nothing) rămâne activă doar «sumar».
    ''' Un flag FALSE ASCUNDE intrarea (SetItemVisible), nu doar o dezactivează — o
    ''' intrare ascunsă nu ocupă spațiu, nu se pictează, nu se poate selecta și e sărită
    ''' de navigarea cu tastatura.
    ''' </summary>
    Private Sub ApplyViewGating(info As AngajamentTreeInfo)
        Try
            'navViews.SetItemVisible("indicatori", info IsNot Nothing AndAlso info.AreIndicatori)
            navViews.SetItemVisible("istoric", info IsNot Nothing AndAlso info.AreIstoric)
            'navViews.SetItemVisible("revizii", info IsNot Nothing AndAlso info.AreRevizii)
            navViews.SetItemVisible("rezervari", info IsNot Nothing AndAlso info.AreRezervari)
            'navViews.SetItemVisible("partener", info IsNot Nothing AndAlso info.ArePartener)
            navViews.SetItemVisible("receptii", info IsNot Nothing AndAlso info.AreReceptii)
            navViews.SetItemVisible("plati", info IsNot Nothing AndAlso info.ArePlati)
            navViews.SetItemVisible("ddf", info IsNot Nothing AndAlso info.AreDDF)
            navViews.SetItemVisible("ord", info IsNot Nothing AndAlso info.AreORD)

            ' Dacă vederea activă tocmai s-a închis, cădem înapoi pe «sumar» (mereu activ)
            ' ca shell-ul să nu rămână pe o pagină pe care nu o mai poți părăsi.
            If Not IsViewEnabled(navViews.SelectedKey, info) Then
                navViews.SelectedKey = "sumar"
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplyViewGating", ex)
            Throw
        End Try
    End Sub

    ' Adevărat dacă vederea are dreptul să fie activă pentru contextul dat.
    Private Shared Function IsViewEnabled(key As String, info As AngajamentTreeInfo) As Boolean
        If String.IsNullOrEmpty(key) OrElse key = "sumar" Then Return True
        If info Is Nothing Then Return False
        Select Case key
            'Case "indicatori" : Return info.AreIndicatori
            Case "istoric" : Return info.AreIstoric
            'Case "revizii" : Return info.AreRevizii
            Case "rezervari" : Return info.AreRezervari
            'Case "partener" : Return info.ArePartener
            Case "receptii" : Return info.AreReceptii
            Case "plati" : Return info.ArePlati
            Case "ddf" : Return info.AreDDF
            Case "ord" : Return info.AreORD
            Case Else
                Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
        End Select
    End Function

    ' Selecția din listă împinge contextul (AngajamentTreeInfo) către vederea activă.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            Dim info As AngajamentTreeInfo = Nothing
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If cod IsNot Nothing Then
                _treeInfos.TryGetValue(cod, info)
            End If
            _currentInfo = info
            ' Flag-urile nodului comandă ce vederi sunt accesibile (poarta Are*).
            ApplyViewGating(info)
            _activeView?.SetContext(info)
            RefreshInfoForm()   ' fereastra «Informații interne», dacă e deschisă
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' NOTĂ (felia 0034): strângerea arborelui a DISPĂRUT din shell — butonul de subsol,
    ' handler-ul tree_CollapsedChanged și ClampSplitter au fost șterse, iar colțul din stânga
    ' al subsolului a devenit butonul de descărcare a listei. Arborii din VEDERI își păstrează
    ' strângerea; asta a fost doar a shell-ului.

    ''' <summary>
    ''' Iconița din STÂNGA subsolului arborelui = descarcă din FOREXE lista de angajamente
    ''' («adlop - Lista Angajamente Curente.wfl»). Rezultatul rămâne LOCAL (memorie + JSON);
    ''' scrierea pe server e rândul «Sincronizare (server)» din meniul de opțiuni.
    ''' </summary>
    Private Async Sub Tree_FooterLeftIconClicked(e As MouseEventArgs) Handles tree.FooterLeftIconClicked
        Try
            busyBar.Running = True
            Try
                Await _forexe.DownloadListaAsync()
            Finally
                busyBar.Running = False
            End Try
        Catch ex As Exception
            ' Frontieră de UI (async Sub): nu poate rearunca — logăm și spunem de ce.
            GlobalErrorLog.Write("MainForm.tree_FooterLeftIconClicked", ex)
            MessageBox.Show(Me, "Descărcarea listei de angajamente a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Iconița din dreapta unui NOD = descarcă din FOREXE angajamentul acela întreg
    ''' («Prelucrare Completa», sau varianta REVERSE dacă are deja istoric local). Rezultatul
    ''' rămâne LOCAL, brut — nu există încă mapper de ingestie pentru fluxul ăsta.
    ''' </summary>
    Private Async Sub Tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If String.IsNullOrEmpty(cod) Then Return

            busyBar.Running = True
            Try
                ' Istoricul LOCAL decide înainte/înapoi (Access FX_Angajament_InfoComplete):
                ' îl citim prin aceeași plasă de re-login ca restul shell-ului.
                Await _forexe.DownloadNodeAsync(
                    cod,
                    Function(c, ct) WithReauth(Of IstoricInfo)(Function() _apiClient.GetIstoricAsync(c, ct)))
            Finally
                busyBar.Running = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_RightIconClicked", ex)
            MessageBox.Show(Me, "Descărcarea angajamentului a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ---------------- FOREXE: consolă + sincronizare ----------------
    '
    ' Conectarea, alegerea certificatului, progresul, starea și anularea au plecat toate în
    ' ForexeController (felia 0034). Shell-ul păstrează doar: fereastra consolei (creată o
    ' dată, ascunsă la închidere) și treapta de upsert, care e a lui — coordonatorul aduce
    ' datele, serverul le primește pe calea deja existentă (WithReauth).

    ''' <summary>
    ''' Creează consola FOREXE dacă nu există. Se cheamă din <c>MainForm_Load</c>, ÎNAINTE de
    ''' construirea logger-ului: caseta ei e ținta acestuia pe toată durata aplicației.
    ''' Închiderea ferestrei o ascunde (vezi <c>ForexeConsoleForm.OnFormClosing</c>), deci
    ''' instanța rămâne validă și nu se re-creează niciodată.
    ''' </summary>
    Private Sub EnsureConsole()
        Try
            If _console IsNot Nothing AndAlso Not _console.IsDisposed Then Return
            _console = New ForexeConsoleForm()
            _console.Bind(_forexe)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.EnsureConsole", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Creează fereastra de istoric FOREXE la prima cerere (felia 0040). Ca și consola, se
    ''' construiește O SINGURĂ DATĂ și se ascunde la închidere — dar, altfel decât consola, nu e
    ''' nevoie de ea la pornire: nimeni nu ține o referință în ea, istoricul stă în
    ''' <c>JobHistoryManager</c>, iar fereastra doar îl citește.
    ''' </summary>
    Private Sub EnsureIstoricForexe()
        Try
            If _istoricForexe IsNot Nothing AndAlso Not _istoricForexe.IsDisposed Then Return
            _istoricForexe = New ForexeHistoryForm()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.EnsureIstoricForexe", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' «Conectare» (prima celulă din <c>tlyHeader</c>) — fostul buton al benzii de subsol,
    ''' mutat în antet. Face exact ce făcea acolo: cere coordonatorului o sesiune FOREXE.
    ''' </summary>
    Private Async Sub BtnConectare_Click(sender As Object, e As EventArgs) Handles btnConectare.Click
        Try
            Await _forexe.ConnectAsync()
        Catch ex As Exception
            ' Frontieră de UI (async Sub): nu poate rearunca — logăm și spunem de ce.
            GlobalErrorLog.Write("MainForm.btnConectare_Click", ex)
            MessageBox.Show(Me, "Conectarea la FOREXE a eșuat: " & ex.Message, "FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' Starea coordonatorului poate veni de pe firul robotului — trecem pe firul de UI.
    Private Sub Forexe_StateChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ActualizeazaButonConectare))
            Else
                ActualizeazaButonConectare()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Forexe_StateChanged", ex)
        End Try
    End Sub

    ' Activ doar cât nu e nimic în lucru ȘI nu există deja sesiune (regula benzii de subsol).
    Private Sub ActualizeazaButonConectare()
        Try
            btnConectare.Enabled = Not _forexe.IsBusy AndAlso Not _forexe.IsConnected
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ActualizeazaButonConectare", ex)
        End Try
    End Sub

    ' Coordonatorul e singleton: un abonament rămas ar ține shell-ul în viață după închidere.
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            RemoveHandler _forexe.StateChanged, AddressOf Forexe_StateChanged
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' Butonul de extindere din banda de subsol: arată consola (nemodal, deținută de shell).
    Private Sub ForexeFooter_ExpandRequested(sender As Object, e As EventArgs) Handles forexeFooter.ExpandRequested
        Try
            EnsureConsole()
            If Not _console.Visible Then _console.Show(Me)
            If _console.WindowState = FormWindowState.Minimized Then _console.WindowState = FormWindowState.Normal
            _console.BringToFront()
            _console.Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.forexeFooter_ExpandRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Butonul «Istoric» din banda de subsol (felia 0040): arată istoricul acțiunilor FOREXE,
    ''' nemodal, deținut de shell — exact ca consola. Reîncărcarea se face la FIECARE deschidere:
    ''' fereastra trăiește cât aplicația, iar între două deschideri au mai rulat lucrări.
    ''' </summary>
    Private Sub ForexeFooter_HistoryRequested(sender As Object, e As EventArgs) Handles forexeFooter.HistoryRequested
        Try
            EnsureIstoricForexe()
            If Not _istoricForexe.Visible Then _istoricForexe.Show(Me)
            If _istoricForexe.WindowState = FormWindowState.Minimized Then _istoricForexe.WindowState = FormWindowState.Normal
            _istoricForexe.Reincarca()
            _istoricForexe.BringToFront()
            _istoricForexe.Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.forexeFooter_HistoryRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Sincronizare = descărcarea listei (prin coordonator) + upsert la
    ''' <c>/api/forexe/angajamente/upsert</c>, cu <c>WithReauth</c> pe apelul HTTP. Este
    ''' fluxul vechiului <c>btnSinc</c>, mutat în meniul butonului de opțiuni din bara de
    ''' titlu. Fără DbName (fără login, posibil doar în harness-ul Debug) se oprește după
    ''' descărcare — datele rămân oricum salvate local de coordonator.
    ''' </summary>
    Private Async Function SincronizeazaAsync() As Task
        busyBar.Running = True
        _cts = New CancellationTokenSource()
        Try
            Dim mapate As List(Of Angajament) = Await _forexe.DownloadListaAsync()
            If mapate Is Nothing Then
                ' Motivul l-a spus deja robotul: coordonatorul a pus linia lui în starea din
                ' banda FOREXE, iar pașii descărcării sunt în consolă. Aici n-avem ce adăuga.
                Return
            End If

            ' Guard: fără DbName (populat la login) nu putem ținti baza unității.
            If String.IsNullOrEmpty(_session.DbName) Then
                MessageBox.Show(Me,
                    "Lista a fost descărcată și salvată local, dar nu poate fi trimisă pe server: " &
                    "sesiunea nu are baza unității (necesită login).",
                    "Sincronizare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Upsert-ul e HTTP, nu robot: rezultatul lui se spune operatorului, nu consolei FOREXE.
            Dim resp As String = Await WithReauth(Function() _apiClient.UpsertAngajamenteAsync(_session.DbName, mapate, _cts.Token))
            MessageBox.Show(Me,
                $"Sincronizare reușită: {mapate.Count} angajamente trimise în «{_session.DbName}».{Environment.NewLine}{resp}",
                "Sincronizare", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SincronizeazaAsync", ex)
            Throw
        Finally
            busyBar.Running = False
        End Try
    End Function

    ' ---------------- placeholder-e (felii viitoare) ----------------

    Private Sub BtnSort_Click(sender As Object, e As EventArgs) Handles btnSort.Click
        Try
            ' TODO felie: sortarea arborelui (Access btnSort / m_SortTree).
            MessageBox.Show(Me, "În lucru.", "Sortare", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnSort_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opțiunile arborelui (Access bOpt). În această felie: comută afișarea
    ''' angajamentelor ascunse (ASCUNS) și re-citește arborele — ASCUNS e filtru pe
    ''' server (include_hidden), nu unul local.
    ''' </summary>
    Private Async Sub BtnOpt_Click(sender As Object, e As EventArgs) Handles btnOpt.Click
        Try
            _includeHidden = Not _includeHidden
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnOpt_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deschide (sau readuce în față) fereastra nemodală «Informații interne», care
    ''' arată toate câmpurile + flag-urile Are* ale angajamentului selectat. Nemodală:
    ''' Show, nu ShowDialog — operatorul poate lucra în shell cu ea deschisă. Se
    ''' reîmprospătează singură la fiecare selecție din arbore (vezi RefreshInfoForm);
    ''' butonul ei «Reîmprospătează» re-citește selecția prin provider-ul _currentInfo.
    ''' </summary>
    Private Sub BtnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click
        Try
            If _infoForm Is Nothing OrElse _infoForm.IsDisposed Then
                _infoForm = New InternalInfoForm(Function() _currentInfo)
                ' Poziționare lângă cardul arborelui, în interiorul shell-ului.
                Dim anchor As Point = PointToScreen(New Point(pnlWork.Left + 8, pnlWork.Top + 8))
                _infoForm.StartPosition = FormStartPosition.Manual
                _infoForm.Location = anchor
                _infoForm.ShowInfo(_currentInfo)
                _infoForm.Show(Me)   ' nemodal, deținut de shell (se închide odată cu el)
            Else
                _infoForm.ShowInfo(_currentInfo)
                If _infoForm.WindowState = FormWindowState.Minimized Then _infoForm.WindowState = FormWindowState.Normal
                _infoForm.BringToFront()
                _infoForm.Activate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnInfo_Click", ex)
        End Try
    End Sub

    ' Împinge contextul curent către fereastra de informații, dacă e deschisă.
    Private Sub RefreshInfoForm()
        Try
            If _infoForm IsNot Nothing AndAlso Not _infoForm.IsDisposed Then
                _infoForm.ShowInfo(_currentInfo)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RefreshInfoForm", ex)
        End Try
    End Sub

    ' ---------------- temă ----------------

    ' Culorile semantice theme-aware (rulează după ThemeManager.Apply și la comutare).
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme = ThemeManager.Current
            Dim p = scheme.Palette

            ' Fundalul formularului ESTE conturul de 1px al ferestrei (vezi LoginForm).
            BackColor = p.BorderColor

            ' Etichetele secundare -> text dim; titlurile rămân pe TextColor plin.
            lblOperator.ForeColor = p.TextDimColor
            lblAn.ForeColor = p.TextDimColor
            lblSs.ForeColor = p.TextDimColor
            'lblUnit.ForeColor = p.TextColor
            lblTree.ForeColor = p.TextColor

            ' «Conectare» e butonul principal al antetului (stilul îl avea în banda de subsol).
            ButtonStyles.ApplyPrimary(btnConectare, scheme)


            ' Arborele ESTE acum IThemedControl: își ia singur paleta și, mai important,
            ' ThemeManager nu mai recurge în copiii lui. Împinsul de culori de aici era exact
            ' ce ștergea alegerile din designer, deci a dispărut — o culoare pusă în designer
            ' câștigă, una lăsată goală urmează tema.

            ' Banda FOREXE e IThemedControl: ThemeManager i-a cerut deja ApplyTheme și NU a
            ' recurs în copiii ei — aici nu mai împingem nicio culoare peste ea.
            pnlHeader.Invalidate()
            pnlStatus.Invalidate()
        Catch ex As Exception
            ' Boundary UI (rulează în cascada de temă/paint) — logăm și înghițim.
            GlobalErrorLog.Write("MainForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' Cele două benzi (header + status) citesc ca o singură bară cu caption-ul:
    ' o linie de 1px sub header, respectiv deasupra barei de status.
    Private Sub PnlHeader_Paint(sender As Object, e As PaintEventArgs) Handles pnlHeader.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlHeader_Paint", ex)
        End Try
    End Sub

    Private Sub PnlStatus_Paint(sender As Object, e As PaintEventArgs) Handles pnlStatus.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlStatus_Paint", ex)
        End Try
    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.Activate()
        Me.BringToFront()
    End Sub

    ' Comutarea schemei de temă NU mai are handler aici. Selectorul a intrat în bara de titlu
    ' (felia 0029, `capBar.ShowThemeButton`), iar `ThemeManager.SetScheme` difuzează schema peste
    ' toate formularele deschise — shell-ul n-are nimic de făcut pe urma lui. Singurul rând care
    ' rămăsese era o linie de jurnal scrisă în consola FOREXE, adică exact zgomotul pe care
    ' felia 0040 l-a scos de acolo (vezi `_forexeLogger`).

    ' Cheile rândurilor din meniul butonului de opțiuni.
    Private Const OPT_JURNAL As String = "jurnal"
    ' Felia 0034: vechiul btnSinc din subsol a devenit rând de meniu (compatibilitate).
    Private Const OPT_SINCRONIZARE As String = "sincronizare"

    ''' <summary>
    ''' Butonul de opțiuni din bara de titlu desfășoară meniul shell-ului — un <c>CustomPopup</c>
    ''' desenat de noi, deci tematizat, exact ca meniul de teme al aceleiași bare.
    '''
    ''' <para>Azi are un singur rând, «Arată jurnal». Poarta lui e
    ''' <c>FeatureSwitches.VizualizatorJurnaleActiv</c> — comutatorul intern, mereu aprins deocamdată.
    ''' Când e stins, meniul NU se deschide deloc: un meniu cu singurul lui rând stins ar fi o
    ''' fereastră goală agățată de buton.</para>
    ''' </summary>
    Private Sub CapBar_OptionButtonClick(sender As Object, e As EventArgs) Handles capBar.OptionButtonClick
        Try
            ' Al doilea clic pe buton ÎNCHIDE meniul: apăsarea l-a închis deja (a activat fereastra
            ' de dedesubt), deci fără garda asta l-am redeschide instantaneu.
            If CustomPopup.ClosedJustNow Then Return

            Dim ancora As Rectangle = capBar.OptionButtonBounds
            If ancora.IsEmpty Then Return

            Dim elemente As New List(Of CustomPopupItem)()
            If FeatureSwitches.VizualizatorJurnaleActiv Then
                ' «&A» = litera de acces, ca la orice meniu de sistem.
                elemente.Add(New CustomPopupItem(OPT_JURNAL, "&Arată jurnal"))
            End If
            elemente.Add(New CustomPopupItem(OPT_SINCRONIZARE, "&Sincronizare (server)"))
            If elemente.Count = 0 Then Return

            ' NU în «Using»: arătat nemodal, popup-ul se eliberează singur la închidere.
            Dim meniu As New CustomPopup(elemente)
            AddHandler meniu.ItemClicked, AddressOf MeniuOptiuni_ItemClicked
            meniu.ShowBelow(capBar, ancora)
        Catch ex As Exception
            ' Frontieră de UI (handler de eveniment): logăm și înghițim.
            GlobalErrorLog.Write("MainForm.CapBar_OptionButtonClick", ex)
        End Try
    End Sub

    Private Async Sub MeniuOptiuni_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Select Case e.Item.Key
                Case OPT_JURNAL
                    ShowLog()
                Case OPT_SINCRONIZARE
                    Await SincronizeazaAsync()
                Case Else
                    ' Fără no-op-uri tăcute: un rând adăugat în meniu și uitat aici trebuie să se vadă.
                    Throw New ArgumentException("Rând necunoscut în meniul de opțiuni: «" & e.Item.Key & "».")
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.MeniuOptiuni_ItemClicked", ex)
            MessageBox.Show(Me, "Comanda nu a putut fi executată: " & ex.Message, "K-BOT",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Deschide vizualizatorul de jurnale NEMODAL — operatorul trebuie să poată citi jurnalul și să
    ''' lucreze în shell în același timp. O singură fereastră: dacă e deja deschisă, se aduce în
    ''' față, ca <c>InternalInfoForm</c>.
    ''' </summary>
    Private Sub ShowLog()
        If _logViewer Is Nothing OrElse _logViewer.IsDisposed Then
            _logViewer = New LogViewerForm(_apiClient)
            AddHandler _logViewer.FormClosed, Sub() _logViewer = Nothing
        End If
        _logViewer.Show()
        _logViewer.BringToFront()
    End Sub
End Class
