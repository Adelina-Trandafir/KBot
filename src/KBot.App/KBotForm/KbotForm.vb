Imports System.IO
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe
' RichTextBoxLogger and CertificateSelectionForm live in the global namespace (from KBot.Forexe).

''' <summary>
''' The main K-BOT shell -- the equivalent of Access frmFX_MAIN (ONLY that one; the Menu stays
''' a separate concept). Three columns: the view navigation (left), the angajamente tree
''' (middle) and the active view (right). Views are UserControls created lazily; the state of
''' the selected node travels as AngajamentTreeInfo, not as hidden textboxes.
''' </summary>
''' <remarks>
''' Slices 0086 / 0086-01: the class is split into partial files by concern, kept in folders
''' under <c>KBotForm\</c> (none in the project root). This file holds the fields, the constructor,
''' the re-login net and Load; the state the pieces share is <see cref="ShellState"/>
''' (<c>_state</c>). The rest, by folder:
''' <list type="bullet">
''' <item><c>KBotForm\</c> -- Periods (year / SS / CodProgram combos), Units, Subunit, Chrome (theme,
''' header / status painting, the options menu), Host, Pagini, MessageCatalog; nested under this file;</item>
''' <item><c>KBotForm\Navigation\</c> -- Views (left navigation, lazy views, the Are* gate), Tree (the
''' angajamente list and its info window), TreeOptions, Grupe, Nomenclatoare;</item>
''' <item><c>KBotForm\Robot\</c> -- (renamed from <c>Forexe\</c>, slice 0086-06) Console, Browser, ForexeWatch, Download, Parallel, RobotQueue, KeRobot,
''' and the flows that already left the form as classes of their own (slice 0086-02): <c>CaptureFlow</c>
''' (the FOREXE page pictures) and <c>IngestFlow</c> (the two-phase ingest and the partial refreshes);</item>
''' <item><c>KBotForm\Documents\</c> -- Ddf, DdfDelete, DdfSend, DdfSendMenu, DdfSendHelpers, Ord,
''' Receptii (link editor, rebuild from istoric), Machete;</item>
''' <item><c>KBotForm\Operations\</c> -- CabNotes, UncorrectedOperations, AsociereGuard, Extrase;</item>
''' <item><c>KBotForm\Integrations\</c> -- EFactura, Vercon, Adechit, Director;</item>
''' <item><c>KBotForm\Help\</c> -- HelpCapture, HelpReveal, Tutorial;</item>
''' <item><c>KBotForm\Views\</c> -- what is UNIQUE to this form (slice 0086-03): the views Sumar, Istoric, Rezervari,
''' Receptii, Plati, Extrase, Ord, NoteCab, Browser, EFactura and the placeholder, and the windows and pages only they
''' open (<c>Ord</c>, <c>NoteCab</c>, <c>Nomenclatoare</c>, <c>Adechit</c>, <c>Vercon</c>, <c>EFactura</c>, groups, reservations
''' repair). The screens another part of the program also uses are in <c>Views\Comune\</c> (the whole DDF view and its
''' pages -- <c>Director</c> embeds it --, <c>ExtraseForm</c>, <c>ParteneriForm</c>, <c>IViewRibbon</c>, <c>LogClearDialog</c>,
''' <c>OrdTextForm</c>, the print list). What is not a screen lives outside <c>Views\</c> (slice 0086-04): the DDF builders in
''' <c>Documents\Ddf</c>, the PDF cache / print target in <c>Documents\Pdf</c>, the signing helpers in <c>Documents\Signing</c>,
''' <c>Adobe\</c>, <c>LogViewer\</c>; the window only this form opens, <c>InternalInfoForm</c>, is in <c>KBotForm\Views\</c>. Slice 0086-06: the FOREXE windows only this form opens
''' (<c>ForexeConsoleForm</c>, <c>ForexeFooterView</c>, <c>ForexeHistoryForm</c>, <c>RobotQueueForm</c>, <c>ActualizareMultiplaForm</c>,
''' <c>SelectieReceptiiForm</c>) are there too; the robot side (<c>ForexeController</c>, the stores, the queue) is in <c>Forexe\Robot\</c>
''' and the association windows in <c>Forexe\Asociere\</c>.</item>
''' </list>
''' </remarks>
Partial Public Class KbotForm

    Private ReadOnly _forexeRunner As IForexeRunner
    Private ReadOnly _session As SessionContext
    Private ReadOnly _apiClient As IApiClient
    ''' <summary>
    ''' The upload of the FOREXE page captures (operator, 28.09.2026). Its own interface, like
    ''' the markers: the shell is the only caller, right after the ingest that wrote the
    ''' records the pictures hang off.
    ''' </summary>
    Private ReadOnly _capturiApi As IForexeCapturiApi
    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _loginFactory As Func(Of LoginForm)
    ''' <summary>
    ''' The FOREXE logger. The shell BUILDS it and attaches it to the runner, but NEVER writes
    ''' to it: the FOREXE console shows exactly what the robot in <c>KBot.Forexe</c> says,
    ''' nothing else. Until slice 0040 the shell also put its own business here (theme switched,
    ''' lazy view created, tree loaded, periods, SS remembered) -- a log in which the robot's
    ''' steps got lost. Whoever has a shell error to report calls <c>GlobalErrorLog.Write</c>;
    ''' whoever has something to tell the operator says it in the UI.
    ''' </summary>
    Private _forexeLogger As RichTextBoxLogger
    ' WARNING: this source belongs to the SYNCHRONISATION, not to the form -- it is born in
    ' SincronizeazaAsync, right before its only use, and stays Nothing until then. Do not read
    ' it from another flow (slice 0055 did and hit a NullReference before any request left).
    ' The rest of the shell uses CancellationToken.None.
    Private _cts As CancellationTokenSource

    ' Slice 0086-01: the state the pieces of the shell share (periods, views, the selected node,
    ' the tree rows, the download marks) is ONE object -- see ShellState.vb. The order and the
    ' columns of the tree live in AppSettings (slice 0777, TreeOptions).
    Private ReadOnly _state As New ShellState()
    ' The modeless «Informatii interne» window (the Are* flags of the selected node).
    ' Nothing / IsDisposed = closed; reopened on demand.
    Private _infoForm As InternalInfoForm
    ' The FOREXE coordinator (slice 0034) -- the only one that talks to the runner. The footer
    ' band and the console bind to it; the shell no longer orchestrates anything on its own.
    Private ReadOnly _controller As ForexeController
    ' The FOREXE console: created ONCE and only hidden on close, because its rtbLog is the
    ' logger's target for the whole life of the application (see EnsureConsole).
    Private _console As ForexeConsoleForm
    ' The FOREXE action history (slice 0040): created on first request, hidden on close.
    Private _istoricForexe As ForexeHistoryForm
    ' Slice 0098: the one line of work for the robot (KbotForm.RobotQueue.vb) and its window.
    Private ReadOnly _robotQueue As RobotQueue
    Private _queueForm As RobotQueueForm

    ' The settings window (slice 0072): built by DI on demand, shown modeless and owned by
    ' the shell, one instance at a time (SetariForm.ShowFor).
    Private ReadOnly _setariFactory As Func(Of SetariForm)

    Public Sub New(forexeRunner As IForexeRunner, session As SessionContext,
                   apiClient As IApiClient, authApi As IAuthApi, loginFactory As Func(Of LoginForm),
                   forexe As ForexeController, setariFactory As Func(Of SetariForm),
                   capturiApi As IForexeCapturiApi)
        InitializeComponent()
        ' Slice 0078-08: no new row while a document is still opening in Adobe (AdobeOpenGate).
        AdobeOpenGate.LockWhileOpening(tree)
        ' Slice 0114-26: the row tooltip (it carries the error warnings) stays until the mouse moves to another
        ' row or leaves the tree; 0 = no timer. It is a public field, so the designer never writes it.
        tree.AutoHideTooltipMs = 0
        _forexeRunner = forexeRunner
        _session = session
        AddHandler _session.PeriodChanged, AddressOf SubunitSessionPeriodChanged   ' slice 01EF-02: the subunit selector follows the source
        _apiClient = apiClient
        _authApi = authApi
        _loginFactory = loginFactory
        _controller = forexe
        _robotQueue = New RobotQueue(Function() _controller.WaitUntilIdleAsync(),
                                     Sub(text) _controller.SpuneStare(text))
        _setariFactory = setariFactory
        _capturiApi = capturiApi
        ' Slice 0086-02: the flows that left the form (KBotForm\Robot\*Flow.vb), built once.
        _captures = New CaptureFlow(Me, Me, _controller, _capturiApi,
                                    Function(k_cod) _rezervariInLucru.ContainsKey(k_cod))
        _ingest = New IngestFlow(Me, Me, _apiClient, _controller, _robotQueue, _state, _captures)
        Me.Text = "K-BOT"
    End Sub

    ''' <summary>
    ''' Runs an authenticated call. On a 401 (session expired / absolute cap) it reopens
    ''' LoginForm; if the operator re-authenticates, the call is retried ONCE with the fresh
    ''' token from SessionContext (singleton -- the same instance ApiClient reads). Any other
    ''' failure, or a second 401, propagates. A CONTEXT_MISMATCH (403) stops short -- see
    ''' IsContextMismatch.
    ''' </summary>
    Private Async Function WithReauth(Of T)(action As Func(Of Task(Of T))) As Task(Of T)
        ' No net of its own: the 401 is control flow (re-login), and any other failure is
        ' already logged (GlobalErrorLog) and shown by the caller (LoadTreeAsync / SincronizeazaAsync).
        ' VB.NET does not allow Await inside a Catch: the 401 is captured and handled below.
        Dim expired As ApiException
        Dim tokenFolosit As String = _session.Token
        Try
            Return Await action().ConfigureAwait(True)
        Catch ex As ApiException When IsContextMismatch(ex)
            Throw ContextMismatchError(ex)
        Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401
            expired = ex
        End Try

        ' Slice 0097: the login window only when the settings call for it; otherwise K-BOT logs
        ' back in by itself (see ReautentificaAsync). A failed silent login falls back to the window.
        If Not Await ReautentificaAsync(tokenFolosit).ConfigureAwait(True) Then
            Using login As LoginForm = _loginFactory()
                If login.ShowDialog(Me) <> DialogResult.OK Then
                    Throw expired   ' the operator cancelled the re-login; propagate the original 401
                End If
            End Using
            ' The window lets the operator pick a unit; the caption follows the session.
            ArataUnitateaInTitlu()
        End If

        ' The login refilled _session.Token (the same instance ApiClient reads).
        ' ONE retry. A second 401 right after a fresh login is NOT a normal expiry -- it is a
        ' server defect (token rejected at once). The operator is not sent back into the login
        ' loop: they are told plainly what happened.
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
            Throw   ' another 401 reason (e.g. a real expiry) -- propagate unchanged
        End Try
    End Function

    ''' <summary>
    ''' For code that talks to the server outside ApiClient (the PDF template download): makes sure the
    ''' session token is alive, re-signing the operator in when it is not (the same net as every other call).
    ''' Must run on the UI thread. True = the session has a usable token now.
    ''' </summary>
    Public Async Function EnsureSessionAsync() As Task(Of Boolean)
        Try
            Await WithReauth(Function() _authApi.GetMyUnitsAsync(_session.Token, CancellationToken.None)).ConfigureAwait(True)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("KbotForm.EnsureSessionAsync", ex)
            Return False
        End Try
    End Function

    ' One silent re-login at a time: several calls can hit the same expiry together.
    Private ReadOnly _reloginGate As New SemaphoreSlim(1, 1)

    ''' <summary>
    ''' Slice 0097 -- the re-login WITHOUT the window. True = the session has a fresh token and the
    ''' call can be retried; False = the window must be shown.
    '''
    ''' <para>Silent when: the password of this process's login is known
    ''' (<see cref="SessionCredentials"/>), AND either the operator (advanced options) turned the
    ''' window off, or the last login typed in the window is younger than the interval
    ''' («Setari → Autentificare», 10..60 minutes without the advanced options). Always on the
    ''' SAME unit (<c>_session.DbName</c>). A token already replaced by a concurrent re-login is
    ''' simply reused. A server refusal (the password was changed elsewhere) drops the kept
    ''' password and returns False; any other failure also returns False, logged.</para>
    ''' </summary>
    Private Async Function ReautentificaAsync(tokenFolosit As String) As Task(Of Boolean)
        Await _reloginGate.WaitAsync().ConfigureAwait(True)
        Try
            ' Somebody else already logged back in while this call waited.
            If Not String.IsNullOrEmpty(_session.Token) AndAlso
               Not String.Equals(_session.Token, tokenFolosit, StringComparison.Ordinal) Then Return True

            Dim s As AppSettings = AppSettings.Current
            If s.ReloginPromptInEffect Then
                Dim last As DateTime = SessionCredentials.LastInteractiveLoginUtc
                If last = DateTime.MinValue Then Return False
                If DateTime.UtcNow - last >= TimeSpan.FromMinutes(s.ReloginMinutesInEffect) Then Return False
            End If

            Dim user As String = Nothing, pass As String = Nothing
            If Not SessionCredentials.TryRecall(user, pass) Then Return False
            If String.IsNullOrWhiteSpace(_session.DbName) Then Return False

            Dim dc As String = _session.DbName
            Dim lastSs As String = _session.LastSS
            Try
                Dim result As LoginResult = Await _authApi.LoginAsync(
                    user, pass, dc, Environment.MachineName, CancellationToken.None).ConfigureAwait(True)
                _session.Populate(user, result.Token, result.SessionContext)
                _session.LastSS = lastSs
                SessionCredentials.Remember(user, pass, interactive:=False)
                Return True
            Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401
                ' The kept password is no longer the account's: the window, and forget it.
                GlobalErrorLog.Write("MainForm.ReautentificaAsync", ex)
                SessionCredentials.ForgetInProcess()
                Return False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReautentificaAsync", ex)
            Return False
        Finally
            _reloginGate.Release()
        End Try
    End Function

    ''' <summary>
    ''' CONTEXT_MISMATCH = a LIVE token used on a context other than the session's (e.g. another
    ''' db_name). The server returns it with 403, NOT 401 (see guard.reject / auth_periods),
    ''' precisely because the session is valid -- so a re-login fixes nothing and would send the
    ''' operator into a pointless loop. Handled apart from the 401 path, on EVERY call, not only
    ''' after a re-login.
    ''' </summary>
    Private Shared Function IsContextMismatch(ex As ApiException) As Boolean
        Return ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 403 AndAlso
               String.Equals(ex.Reason, "CONTEXT_MISMATCH", StringComparison.Ordinal)
    End Function

    ' A clear message for the operator: not an expired session, a unit mismatch.
    Private Shared Function ContextMismatchError(ex As ApiException) As ApiException
        Return New ApiException(
            "Cererea a fost respinsă: sesiunea este deschisă pe altă unitate decât cea cerută " &
            "(« CONTEXT_MISMATCH »). Este un defect, nu o sesiune expirată — contactați administratorul.",
            403, ex.Reason)
    End Function

    Private Async Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' FOREXE logger (slice 0034): the VISIBLE target is the box in the FOREXE console,
            ' and the lines ALSO go to <AppDir>\Logs. The console is built HERE, once, and stays
            ' hidden until the operator asks for it -- RichTextBoxLogger takes the control at
            ' construction and keeps it for the life of the application, so the target must
            ' exist before the first FOREXE action, not only when the window opens.
            Dim logDir As String = LogPaths.LogsDirectory()
            Directory.CreateDirectory(logDir)
            Dim logPath As String = Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt")

            Try
                EnsureConsole()
                _console.CaleJurnal = logPath
                _forexeLogger = New RichTextBoxLogger(_console.LogBox) With {
                    .EnableUI = True,
                    .LogFilePath = logPath
                }
            Catch ex As Exception
                KBotMessage.Show(Me, "Nu s-a putut crea logger-ul FOREXE: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ' The logger is essential to see the progress and errors of the FOREXE flows;
                ' without it the shell cannot work.
                Close()
            End Try


            Try
                ' Attach the FOREXE logger to the runner (the same singleton instance).
                ' Slice 0125: the simulated runner (Debug) has no logger to take.
                Dim k_realRunner As ForexeRunner = TryCast(_forexeRunner, ForexeRunner)
                If k_realRunner IsNot Nothing Then k_realRunner.AttachLogger(_forexeLogger)
            Catch ex As Exception
                KBotMessage.Show(Me, "Nu s-a putut atașa logger-ul la runner: " & ex.Message,
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Close()
            End Try

            ' Identity: caption + status bar (from SessionContext).
            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), "K-BOT", "K-BOT — " & _session.NumeUnitate)
            'lblUnit.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), String.Empty, _session.NumeUnitate)
            lblOperator.Text = If(String.IsNullOrEmpty(_session.OperatorName), String.Empty, _session.OperatorName)

            ' The FOREXE footer band: the coordinator's dialogs (certificate choice) get the
            ' shell as owner, and the band binds to the coordinator.
            _controller.Owner = Me
            forexeFooter.Bind(_controller)
            ' Slice 0098: the robot queue - its footer button and its window (KbotForm.RobotQueue.vb).
            BindRobotQueue()
            ' The in-page watcher's finished operations (slice 0073) - see KbotForm.ForexeWatch.vb.
            LeagaUrmarirea()
            ' The login's «Operatiuni necorectate» warning (slice 0084) - see KbotForm.UncorrectedOperations.vb.
            LeagaOperatiunileNecorectate()
            ' Slice 0100: the on-connect update of the old angajamente - see KbotForm.Parallel.vb.
            LeagaActualizareaMultipla()
            ' The «Browser FOREXE» view's gate on the session (slice 0074) - see KbotForm.Browser.vb.
            LeagaBrowserul()

            ' View navigation -- the page order from Access, Sumar by default.
            ' The entries are AUTHORED IN THE DESIGNER, in `navViews.Items` (slice 0025): see
            ' KbotForm.Designer.vb. Nothing is added from code -- an AddItem here would hit the
            ' duplicate-key throw on the first run.
            ' Every key (except "sumar") is gated by an Are* flag from the tree: see
            ' ApplyViewGating. Sumar is always enabled (it has no flag).
            ' The initial selection STAYS in code, deliberately: this assignment is what raises
            ' SelectionChanged and so creates the first view. In the designer it would be dead.
            ' A director has no «sumar» (DirectorMode): ApplyViewGating below puts the empty page up.
            If Not ModDirector Then navViews.SelectedKey = "sumar"   ' raises SelectionChanged -> creates the view

            ' With no node selected nobody knows what data exists: every flagged view starts
            ' closed, not open-and-empty.
            ApplyViewGating(Nothing)

            ' A director does not work with FOREXE: the band with the connection and the robot queue is not shown.
            If ModDirector Then forexeFooter.Visible = False

            ' The angajamente list: a flat list whose columns (CODANGAJAMENT / SURSE) and order
            ' come from AppSettings and follow its changes (slice 0777, KbotForm.TreeOptions.vb).
            LeagaOptiunileArborelui()

            ' The year / SS selectors (caption bar) AND the list are filled only with an authenticated session
            ' (the Release path goes through login; the Debug harness can open the window
            ' without one).
            If _session.IsAuthenticated AndAlso Not String.IsNullOrEmpty(_session.DbName) Then
                Await LoadPeriodsAsync()
                ' Slice 0100-02: what the server allows (multi-thread) is known before the tree is read.
                Await LoadServerSettingsAsync()
                Await LoadTreeAsync()
                ' Slice 0088: the menu's «Operatiuni necorelate» entry and its «!» mark.
                Await RefreshUncorrelatedMarkAsync()
                ' Slice 0097: the unit selector in the caption (two or more units only).
                Await IncarcaUnitatileAsync()
                ' The advice about the installed Adobe (free Reader 2025+ recommended); the
                ' operator switches it off from its own window. Before the tour, which is posted.
                AdobeAdvice.ShowIfDue(Me)
                ' Slice 000T-13: the tutorial of the start, by itself, until it was seen to the end
                ' or switched off. Last, so it walks through a window that already has its data.
                PornesteTutorialulDePornire()
            Else
                ' No session (possible only in the Debug harness): no data, no silent sample --
                ' the list stays empty, honestly. The bar with no year / SS selector tells the story.
            End If

        Catch ex As Exception
            ' UI boundary (Load): an async Sub cannot re-throw -- log and swallow.
            GlobalErrorLog.Write("MainForm.MainForm_Load", ex)
        End Try
    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.Activate()
        Me.BringToFront()
    End Sub

    ''' <summary>
    ''' Slice 0097-02: with «Setari → Aplicatie → Fereastra principala porneste marita» the shell
    ''' opens maximized. After the base (theme, fit, centring on the normal size), so «Restore»
    ''' brings back the window the designer and the theme agreed on; still before the first paint.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ' Hosted (AvacontMain): the window state belongs to the host, which applies it itself.
            If HostForm Is Nothing AndAlso AppSettings.Current.StartMaximized Then WindowState = FormWindowState.Maximized
        Catch ex As Exception
            ' UI boundary (Load): log and swallow -- the window opens at its normal size.
            GlobalErrorLog.Write("MainForm.OnLoad", ex)
        End Try
    End Sub

    ' Slice 000T-13: the tutorial of the start (HelpService decides whether it is due). It belongs to the main window
    ' (AvacontMain: the ribbon is there), so a shell on its own (the Debug bench) never starts it.
    ' Posted, not called: Load has just finished its last request and the window must paint the
    ' data first. UI boundary: a tutorial that cannot start is logged, never told -- nobody asked for it.
    Private Sub PornesteTutorialulDePornire()
        Try
            ' Not for a director: the tour walks through ribbon buttons and views his window does not have.
            If IsDisposed OrElse Not IsHandleCreated OrElse HostForm Is Nothing OrElse ModDirector Then Return
            BeginInvoke(New Action(
                Sub()
                    Try
                        Dim k_main As Form = HostForm
                        If IsDisposed OrElse k_main Is Nothing OrElse k_main.IsDisposed OrElse Not k_main.Visible OrElse
                           k_main.WindowState = FormWindowState.Minimized Then Return
                        TryCast(KBotHelp.Provider, HelpService)?.StartOpeningTutorial()
                    Catch ex As Exception
                        GlobalErrorLog.Write("MainForm.PornesteTutorialulDePornire", ex)
                    End Try
                End Sub))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PornesteTutorialulDePornire", ex)
        End Try
    End Sub
End Class
