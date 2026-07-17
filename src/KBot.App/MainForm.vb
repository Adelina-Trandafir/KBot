Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
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
    Private _logger As RichTextBoxLogger
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

    Public Sub New(forexeRunner As IForexeRunner, session As SessionContext,
                   apiClient As IApiClient, authApi As IAuthApi, loginFactory As Func(Of LoginForm))
        InitializeComponent()
        _forexeRunner = forexeRunner
        _session = session
        _apiClient = apiClient
        _authApi = authApi
        _loginFactory = loginFactory
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
        ' logat + arătat de apelant (LoadAngajamenteAsync / btnSinc_Click via _logger).
        ' VB.NET nu permite Await într-un Catch: capturăm 401-ul și continuăm sub Try.
        Dim expired As ApiException = Nothing
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
            ' Logger FOREXE doar-fișier: shell-ul nu mai are panou de log (rtbLog a dispărut);
            ' liniile merg în <AppDir>\Logs, progresul vizual e busyBar. RichTextBox real,
            ' neafișat (nu Nothing): SetColorScheme→RefreshDisplay dereferențiază controlul
            ' (același pattern ca ForexeConnectTest din harness).
            Dim logDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
            Directory.CreateDirectory(logDir)

            Try
                _logger = New RichTextBoxLogger(New RichTextBox()) With {
                    .EnableUI = False,
                    .LogFilePath = Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt")
                }
            Catch ex As Exception
                MessageBox.Show(Me, "Nu s-a putut crea logger-ul FOREXE: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ' Logger-ul e esențial pentru a vedea progresul și erorile din fluxurile FOREXE; fără el, shell-ul nu poate funcționa.
                Close()
            End Try


            Try
                ' Atașează logger-ul FOREXE la runner (aceeași instanță singleton)
                DirectCast(_forexeRunner, ForexeRunner).AttachLogger(_logger)
            Catch ex As Exception
                MessageBox.Show(Me, "Nu s-a putut atașa logger-ul la runner: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Close()
            End Try

            ' Identitate: caption + bara de status (din SessionContext).
            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), "K-BOT", "K-BOT — " & _session.NumeUnitate)
            lblUnit.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), String.Empty, _session.NumeUnitate)
            lblOperator.Text = If(String.IsNullOrEmpty(_session.OperatorName), String.Empty, _session.OperatorName)
            lblProgram.Text = String.Empty   ' se completează după alegerea perioadei (SetPeriod)

            ' Navigația vederilor — ordinea paginilor din Access, Sumar implicit.
            ' Fiecare cheie (mai puțin „sumar") e poarta unui flag Are* din arbore:
            ' vezi ApplyViewGating. Sumar rămâne mereu activ (nu are flag).
            navViews.AddItem("sumar", "Sumar")
            navViews.AddItem("indicatori", "Indicatori")
            navViews.AddItem("istoric", "Istoric")
            navViews.AddItem("revizii", "Revizii")
            navViews.AddItem("rezervari", "Rezervări")
            navViews.AddItem("partener", "Partener")
            navViews.AddItem("receptii", "Recepții")
            navViews.AddItem("plati", "Plăți")
            navViews.AddItem("ddf", "DDF")
            navViews.AddItem("ord", "ORD")
            navViews.SelectedKey = "sumar"   ' declanșează SelectionChanged -> creează vederea

            ' Fără nod selectat nu se știe ce date există: toate vederile cu flag pornesc
            ' închise, nu deschise-și-goale.
            ApplyViewGating(Nothing)

            ' Lista de angajamente: controlul „tree" e configurat ca listă plată cu coloane
            ' (caption = Descriere, coloană = CodAngajament, iconiță de status stânga,
            ' refresh la hover în dreapta). Datele reale vin din GET /api/forexe/tree.
            ConfigureAngajamenteList()

            UpdateForexeStatus()

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
                _logger?.LogWarning("Fără sesiune autentificată — lista de angajamente rămâne goală (deschidere din harness).")
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
                _logger.LogException(ex, "Eroare la citirea perioadelor")
                cboAn.Enabled = False
                cboSs.Enabled = False
                MessageBox.Show(Me,
                    "Nu s-au putut citi perioadele (an/SS): " & ex.Message,
                    "Perioade", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try

            If _periods Is Nothing OrElse _periods.Count = 0 Then
                _logger.LogWarning("Nicio perioadă (an/SS) configurată pentru această unitate.")
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

            _session.SetPeriod(an, ss, row.CodProgram)
            lblProgram.Text = If(String.IsNullOrEmpty(row.CodProgram), String.Empty, "Program: " & row.CodProgram)
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
            _logger?.LogWarning($"Nu s-a putut memora SS '{ss}': {ex.Message}")
        End Try
    End Sub

    ' Schimbarea anului reface SS-urile anului (care fixează perioada) și RE-CITEȘTE
    ' arborele: an-ul e filtru pe server, deci datele vechi nu mai sunt valabile.
    Private Async Sub cboAn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAn.SelectedIndexChanged
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
    Private Async Sub cboSs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSs.SelectedIndexChanged
        Try
            If _suppressPeriodEvents Then Return
            ApplySelectedPeriod(persist:=True)
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.cboSs_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' ---------------- vederi (navigație stânga) ----------------

    Private Sub navViews_SelectionChanged(key As String) Handles navViews.SelectionChanged
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
                Dim ctrl As Control = DirectCast(view, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                viewHost.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _views(key) = view
                _logger?.LogDebug($"Vedere '{key}' creată (lazy).")
            End If

            Dim previous As IAngajamentView = _activeView
            _activeView = view
            DirectCast(view, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, view) Then
                DirectCast(previous, Control).Visible = False
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
                Case "sumar" : Return New PlaceholderView(key, "Sumar")
                Case "indicatori" : Return New PlaceholderView(key, "Indicatori")
                Case "istoric" : Return New PlaceholderView(key, "Istoric")
                Case "revizii" : Return New PlaceholderView(key, "Revizii")
                Case "rezervari" : Return New PlaceholderView(key, "Rezervări")
                Case "partener" : Return New PlaceholderView(key, "Partener")
                Case "receptii" : Return New PlaceholderView(key, "Recepții")
                Case "plati" : Return New PlaceholderView(key, "Plăți")
                Case "ddf" : Return New PlaceholderView(key, "DDF")
                Case "ord" : Return New PlaceholderView(key, "ORD")
                Case Else
                    Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CreateView", ex)
            Throw
        End Try
    End Function

    ' ---------------- lista de angajamente ----------------

    ' Lățimea coloanei CodAngajament (px). Restul spațiului îl ia captionul (Descriere).
    Private Const COD_COLUMN_WIDTH As Integer = 140

    ''' <summary>
    ''' Configurează controlul „tree" ca listă plată cu o coloană CodAngajament
    ''' (caption = Descriere). Aspect: rând 32px, Segoe UI 9.75, iconiță status 24×24
    ''' stânga, refresh 24×24 dreapta doar la hover. Filtrul pe coloană merge din start.
    ''' </summary>
    Private Sub ConfigureAngajamenteList()
        Try
            tree.Font = New Font("Segoe UI", 9.75F)
            tree.LeftIconSize = New Size(24, 24)
            tree.RightIconSize = New Size(24, 24)
            tree.ItemHeight = 32
            tree.HasNodeIcons = True
            tree.ShowRightIconOnHover = True
            ' Rezervă locul iconiței de refresh la dreapta, ca banda de coloane
            ' (CodAngajament) să nu se suprapună peste ea la hover.
            tree.ReserveRightIconSpace = True

            Dim cols As New List(Of ColumnDef) From {
                New ColumnDef With {
                    .Name = "CodAngajament", .Header = "CodAngajament", .Width = COD_COLUMN_WIDTH,
                    .ColType = En_ColType.ColType_Text, .Align = En_ColAlign.ColAlign_Left,
                    .Format = "", .HeaderBackColor = Color.Empty, .HeaderForeColor = Color.Empty,
                    .HeaderAlign = En_ColAlign.ColAlign_Inherit}
            }
            tree.ConfigureListMode(cols)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ConfigureAngajamenteList", ex)
            Throw
        End Try
    End Sub

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
            _logger?.LogWarning("Perioadă neselectată (an/SS) — arborele rămâne gol.")
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
            _logger?.LogInfo($"Arbore încărcat: {rows.Count} angajamente (an {an}, SS «{ss}»" &
                             If(_includeHidden, ", inclusiv ascunse", String.Empty) & ").")
        Catch ex As Exception
            ' Fără plasă tăcută: o eroare (server oprit / 401 sesiune moartă / defect de
            ' server după re-login) se arată operatorului cu motivul întors de server,
            ' nu se maschează cu un arbore gol — acela ar minți că unitatea n-are date.
            _logger?.LogException(ex, "Eroare la încărcarea arborelui de angajamente")
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
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
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
                node.ShowRightIconOnHover = True

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
    ''' Fără nod selectat (info = Nothing) rămâne activ doar «sumar».
    ''' NOTĂ: KBotNavList nu are conceptul de vizibilitate, doar Enabled — o intrare
    ''' dezactivată nu se poate selecta, nu are hover și e sărită de navigarea cu
    ''' tastatura, deci vederea e efectiv inaccesibilă (decizie felia 0008; ascunderea
    ''' propriu-zisă ar cere SetItemVisible în KBot.Theming).
    ''' </summary>
    Private Sub ApplyViewGating(info As AngajamentTreeInfo)
        Try
            navViews.SetItemEnabled("indicatori", info IsNot Nothing AndAlso info.AreIndicatori)
            navViews.SetItemEnabled("istoric", info IsNot Nothing AndAlso info.AreIstoric)
            navViews.SetItemEnabled("revizii", info IsNot Nothing AndAlso info.AreRevizii)
            navViews.SetItemEnabled("rezervari", info IsNot Nothing AndAlso info.AreRezervari)
            navViews.SetItemEnabled("partener", info IsNot Nothing AndAlso info.ArePartener)
            navViews.SetItemEnabled("receptii", info IsNot Nothing AndAlso info.AreReceptii)
            navViews.SetItemEnabled("plati", info IsNot Nothing AndAlso info.ArePlati)
            navViews.SetItemEnabled("ddf", info IsNot Nothing AndAlso info.AreDDF)
            navViews.SetItemEnabled("ord", info IsNot Nothing AndAlso info.AreORD)

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
            Case "indicatori" : Return info.AreIndicatori
            Case "istoric" : Return info.AreIstoric
            Case "revizii" : Return info.AreRevizii
            Case "rezervari" : Return info.AreRezervari
            Case "partener" : Return info.ArePartener
            Case "receptii" : Return info.AreReceptii
            Case "plati" : Return info.ArePlati
            Case "ddf" : Return info.AreDDF
            Case "ord" : Return info.AreORD
            Case Else
                Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
        End Select
    End Function

    ' Selecția din listă împinge contextul (AngajamentTreeInfo) către vederea activă.
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
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

    ' Click pe iconița de refresh (dreapta, la hover) — reîmprospătarea din FOREXE a
    ' angajamentului e o felie separată; aici doar semnalăm, fără no-op tăcut.
    Private Sub tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If String.IsNullOrEmpty(cod) Then Return
            RefreshAngajament(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_RightIconClicked", ex)
        End Try
    End Sub

    ' Stub pentru reîmprospătarea unui angajament din FOREXE (felie viitoare).
    Private Sub RefreshAngajament(cod As String)
        Try
            _logger?.LogInfo($"Refresh angajament «{cod}» cerut (neimplementat în această felie).")
            MessageBox.Show(Me,
                $"Reîmprospătarea angajamentului «{cod}» din FOREXE va fi disponibilă într-o felie viitoare.",
                "Refresh angajament", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RefreshAngajament", ex)
            Throw
        End Try
    End Sub

    ' ---------------- FOREXE: conectare + sincronizare ----------------

    Private Function HasLiveForexeSession() As Boolean
        Try
            Return DirectCast(_forexeRunner, ForexeRunner).HasLiveSession
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.HasLiveForexeSession", ex)
            Throw
        End Try
    End Function

    ' Actualizare cosmetică a indicatorului de status; e chemată și din Finally-ul lui
    ' btnSinc_Click și din OnThemeChanged, deci NU rearuncă (un throw ar scăpa din
    ' handler / Finally și ar dărâma procesul) — logăm și înghițim.
    Private Sub UpdateForexeStatus()
        Try
            Dim connected As Boolean = HasLiveForexeSession()
            lblForexe.Text = If(connected, "● Forexe: conectat", "● Forexe: neconectat")
            Dim p = ThemeManager.Current.Palette
            lblForexe.ForeColor = If(connected, p.SuccessColor, p.TextDimColor)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.UpdateForexeStatus", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Asigură o sesiune FOREXE vie: dacă nu există, rulează fluxul de conectare
    ''' (certificat + workflow „Conectare"). Întoarce False dacă operatorul anulează
    ''' sau conectarea eșuează. Ownership-ul conexiunii rămâne la IForexeRunner —
    ''' shell-ul doar o cere la nevoie (sensul Access al lui btnSinc).
    ''' </summary>
    Private Async Function EnsureForexeSessionAsync(progress As IProgress(Of Integer), ct As CancellationToken) As Task(Of Boolean)
        Try
            If HasLiveForexeSession() Then Return True

            Dim cert As X509Certificate2 = SelectCertificate()
            If cert Is Nothing Then Return False   ' anulat / fără certificat

            Dim job As New JobRequest With {
                .WorkflowName = "Conectare",
                .WflPath = Path.Combine(AppContext.BaseDirectory, "Workflows", "adlop - Conectare.wfl")
            }
            Dim result As JobResult = Await _forexeRunner.RunAsync(job, cert, progress, ct)
            UpdateForexeStatus()
            Return result.Success
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.EnsureForexeSessionAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Sincronizare = fluxul ListaAngajamente (felia completă existentă), pe sesiunea
    ''' FOREXE — deschisă la nevoie. Scrape -> mapare -> upsert la /api/forexe/
    ''' angajamente/upsert, cu WithReauth pe apelul HTTP. Fără DbName (fără login,
    ''' posibil doar în harness-ul Debug) se oprește după mapare.
    ''' </summary>
    Private Async Sub btnSinc_Click(sender As Object, e As EventArgs) Handles btnSinc.Click
        btnSinc.Enabled = False
        busyBar.Running = True
        _cts = New CancellationTokenSource()

        ' Procentul de progres nu are țintă vizuală în shell (busyBar e indeterminată);
        ' liniile detaliate merg în logger-ul fișier.
        Dim progress As New Progress(Of Integer)(Sub(p)
                                                 End Sub)

        Try
            If Not Await EnsureForexeSessionAsync(progress, _cts.Token) Then
                _logger.LogWarning("Sincronizare oprită: nu s-a putut deschide sesiunea FOREXE.")
                Return
            End If

            Dim job As JobRequest = JobBuilder.BuildListaAngajamente(_session)
            Dim result As JobResult = Await _forexeRunner.RunJobAsync(job, progress, _cts.Token)
            If Not result.Success Then
                _logger.LogError($"ListaAngajamente eșuat: {result.Message}")
                Return
            End If

            Dim rows As List(Of Dictionary(Of String, String)) = Nothing
            If Not result.Tables.TryGetValue(WorkflowCatalog.ListaAngajamenteTable, rows) Then
                _logger.LogWarning($"Nu s-a găsit tabelul '{WorkflowCatalog.ListaAngajamenteTable}' în rezultat (0 rânduri scrape).")
                Return
            End If

            ' De-risk: log the real scraped column keys before mapping, so a FOREXE
            ' rename is visible in the log even when the upsert is skipped (no DbName).
            If rows.Count > 0 Then
                _logger.LogInfo($"ListaAngajamente scraped keys: {String.Join(",", rows(0).Keys)}")
            End If

            Dim mapped As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(rows)
            _logger.LogInfo($"ListaAngajamente: {mapped.Count} rânduri mapate (din {rows.Count} brute).")

            ' Guard: fără DbName (populat la login) nu putem ținti baza unității.
            If String.IsNullOrEmpty(_session.DbName) Then
                _logger.LogWarning("DbName nesetat pe sesiune — sar peste upsert (necesită login).")
                Return
            End If

            Dim resp As String = Await WithReauth(Function() _apiClient.UpsertAngajamenteAsync(_session.DbName, mapped, _cts.Token))
            _logger.LogSuccess($"Upsert reușit: {mapped.Count} angajamente în '{_session.DbName}'. Răspuns server: {resp}")

        Catch ex As Exception
            _logger.LogException(ex, "Eroare Sincronizare (UI)")
        Finally
            busyBar.Running = False
            btnSinc.Enabled = True
            UpdateForexeStatus()
        End Try
    End Sub

    ''' <summary>Picker de certificat în mod manual de PIN (utilizatorul tastează PIN-ul în dialogul Windows).</summary>
    Private Function SelectCertificate() As X509Certificate2
        Try
            Using dlg As New CertificateSelectionForm(manualPin:=True)
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    Return dlg.SelectedCertificate
                End If
            End Using
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SelectCertificate", ex)
            Throw
        End Try
    End Function

    ' ---------------- placeholder-e (felii viitoare) ----------------

    Private Sub btnIstoric_Click(sender As Object, e As EventArgs) Handles btnIstoric.Click
        Try
            ' TODO felie: istoricul job-urilor (Access btnIstoric).
            MessageBox.Show(Me, "În lucru.", "Istoric", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnIstoric_Click", ex)
        End Try
    End Sub

    Private Sub btnSort_Click(sender As Object, e As EventArgs) Handles btnSort.Click
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
    Private Async Sub btnOpt_Click(sender As Object, e As EventArgs) Handles btnOpt.Click
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
    Private Sub btnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click
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

            ButtonStyles.ApplyPrimary(btnSinc, scheme)
            ButtonStyles.ApplySecondary(btnIstoric, scheme)

            ' Etichetele secundare -> text dim; titlurile rămân pe TextColor plin.
            lblOperator.ForeColor = p.TextDimColor
            lblProgram.ForeColor = p.TextDimColor
            lblAn.ForeColor = p.TextDimColor
            lblSs.ForeColor = p.TextDimColor
            lblUnit.ForeColor = p.TextColor
            lblTree.ForeColor = p.TextColor

            ' Arborele nu e IThemedControl (nu se retrofitează în această felie) —
            ' i se împing culorile paletei prin proprietățile lui publice.
            tree.BackColor = p.SurfaceAltColor
            tree.ForeColor = p.TextColor
            tree.HoverBackColor = p.ButtonHoverColor
            tree.SelectedBackColor = p.ButtonPressedColor
            tree.SelectedBorderColor = p.AccentColor
            tree.LineColor = p.BorderColor
            tree.HeaderBackColor = p.SurfaceAltColor
            tree.HeaderForeColor = p.TextColor

            UpdateForexeStatus()
            pnlHeader.Invalidate()
            pnlStatus.Invalidate()
        Catch ex As Exception
            ' Boundary UI (rulează în cascada de temă/paint) — logăm și înghițim.
            GlobalErrorLog.Write("MainForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' Cele două benzi (header + status) citesc ca o singură bară cu caption-ul:
    ' o linie de 1px sub header, respectiv deasupra barei de status.
    Private Sub pnlHeader_Paint(sender As Object, e As PaintEventArgs) Handles pnlHeader.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlHeader_Paint", ex)
        End Try
    End Sub

    Private Sub pnlStatus_Paint(sender As Object, e As PaintEventArgs) Handles pnlStatus.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.pnlStatus_Paint", ex)
        End Try
    End Sub

End Class
