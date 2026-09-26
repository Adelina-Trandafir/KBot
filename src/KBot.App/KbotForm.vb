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
''' Slice 0086: the class is split into partial files by concern, each kept around 500 lines.
''' This file holds the fields, the constructor, the re-login net and Load. The rest:
''' <list type="bullet">
''' <item><c>KbotForm.Periods.vb</c> -- year / SS / CodProgram combos;</item>
''' <item><c>KbotForm.Views.vb</c> -- left navigation, lazy views, the Are* gate;</item>
''' <item><c>KbotForm.Tree.vb</c> -- the angajamente list, its options and the info window;</item>
''' <item><c>KbotForm.Receptii.vb</c> -- receptie links editor and rebuild from istoric;</item>
''' <item><c>KbotForm.Ord.vb</c> -- the ordonantare editor entry points;</item>
''' <item><c>KbotForm.Ddf.vb</c> / <c>KbotForm.DdfDelete.vb</c> -- the DDF editor;</item>
''' <item><c>KbotForm.DdfSend.vb</c> / <c>KbotForm.DdfSendMenu.vb</c> / <c>KbotForm.DdfSendHelpers.vb</c> --
''' the send to forexecab and the Rezervari menu follow-ups;</item>
''' <item><c>KbotForm.Extrase.vb</c> -- bank statements download and import;</item>
''' <item><c>KbotForm.Download.vb</c> -- FOREXE downloads from the tree (list and node);</item>
''' <item><c>KbotForm.Ingest.vb</c> -- the two-phase ingest and the partial refreshes;</item>
''' <item><c>KbotForm.Console.vb</c> -- FOREXE console, history, connect, synchronise;</item>
''' <item><c>KbotForm.Chrome.vb</c> -- theme, header/status painting, the options menu;</item>
''' <item><c>KbotForm.Browser.vb</c>, <c>KbotForm.ForexeWatch.vb</c>, <c>KbotForm.TreeOptions.vb</c>,
''' <c>KbotForm.UncorrectedOperations.vb</c> -- as before.</item>
''' </list>
''' </remarks>
Partial Public Class KbotForm

    Private ReadOnly _forexeRunner As IForexeRunner
    Private ReadOnly _session As SessionContext
    Private ReadOnly _apiClient As IApiClient
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

    ' The year / SS / CodProgram catalogue of the current database (from /api/auth/periods).
    Private _periods As IReadOnlyList(Of PeriodInfo)
    ' Suppresses the SelectedIndexChanged logic while the combos are filled from code
    ' (setting DataSource / SelectedIndex raises the events).
    Private _suppressPeriodEvents As Boolean

    ' Lazily created views (key -> instance); only one is visible.
    Private ReadOnly _views As New Dictionary(Of String, IAngajamentView)()
    Private _activeView As IAngajamentView
    ' Context of the current tree selection (Nothing = nothing selected / chapter node).
    Private _currentInfo As AngajamentTreeInfo
    ' NodeKey -> info, rebuilt on every LoadTree.
    Private ReadOnly _treeInfos As New Dictionary(Of String, AngajamentTreeInfo)()
    ' The btnOpt option: also show the HIDDEN (ASCUNS) angajamente (off by default).
    Private _includeHidden As Boolean
    ' The last rows GET /api/forexe/tree handed back, in the server's own order. Kept so a
    ' change of sort re-lays the tree without asking for them again. The order and the
    ' columns themselves live in AppSettings (slice 0777, KbotForm.TreeOptions.vb).
    Private _treeRows As IReadOnlyList(Of AngajamentTreeInfo)
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

    ' The settings window (slice 0072): built by DI on demand, shown modeless and owned by
    ' the shell, one instance at a time (SetariForm.ShowFor).
    Private ReadOnly _setariFactory As Func(Of SetariForm)

    Public Sub New(forexeRunner As IForexeRunner, session As SessionContext,
                   apiClient As IApiClient, authApi As IAuthApi, loginFactory As Func(Of LoginForm),
                   forexe As ForexeController, setariFactory As Func(Of SetariForm))
        InitializeComponent()
        _forexeRunner = forexeRunner
        _session = session
        _apiClient = apiClient
        _authApi = authApi
        _loginFactory = loginFactory
        _controller = forexe
        _setariFactory = setariFactory
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
        Try
            Return Await action().ConfigureAwait(True)
        Catch ex As ApiException When IsContextMismatch(ex)
            Throw ContextMismatchError(ex)
        Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401
            expired = ex
        End Try

        Using login As LoginForm = _loginFactory()
            If login.ShowDialog(Me) <> DialogResult.OK Then
                Throw expired   ' the operator cancelled the re-login; propagate the original 401
            End If
        End Using

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
                DirectCast(_forexeRunner, ForexeRunner).AttachLogger(_forexeLogger)
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
            ' The in-page watcher's finished operations (slice 0073) - see KbotForm.ForexeWatch.vb.
            LeagaUrmarirea()
            ' The login's «Operatiuni necorectate» warning (slice 0084) - see KbotForm.UncorrectedOperations.vb.
            LeagaOperatiunileNecorectate()
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
            navViews.SelectedKey = "sumar"   ' raises SelectionChanged -> creates the view

            ' With no node selected nobody knows what data exists: every flagged view starts
            ' closed, not open-and-empty.
            ApplyViewGating(Nothing)

            ' The angajamente list: a flat list whose columns (CODANGAJAMENT / SURSE) and order
            ' come from AppSettings and follow its changes (slice 0777, KbotForm.TreeOptions.vb).
            LeagaOptiunileArborelui()

            ' The year / SS combos AND the list are filled only with an authenticated session
            ' (the Release path goes through login; the Debug harness can open the window
            ' without one).
            If _session.IsAuthenticated AndAlso Not String.IsNullOrEmpty(_session.DbName) Then
                Await LoadPeriodsAsync()
                Await LoadTreeAsync()
            Else
                ' No session (possible only in the Debug harness): no data, no silent sample --
                ' the list stays empty, honestly. The disabled combos already tell the story.
                cboAn.Enabled = False
                cboSs.Enabled = False
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
End Class
