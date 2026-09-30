#If DEBUG Then
Option Strict On
Imports System.Diagnostics
Imports System.Linq
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' Slice 0098 bench: the REAL robot queue, the REAL server gate and the REAL queue window, driven
''' by a real <see cref="ForexeController"/> over a simulated robot (<see cref="FakeForexeRunner"/>)
''' and a simulated server (<see cref="FakeServerHandler"/>). Nothing leaves the PC; FOREXE and the
''' server are never touched.
''' </summary>
''' <remarks>
''' <para><b>Own instances, not the application's.</b> The gate, the HttpClient, the controller and
''' the queue are built here, so the bench can neither hold nor release the shell's real requests.</para>
''' <para><b>One bench task has the shape of a real node refresh</b> (<c>KbotForm.DescarcaNodulAsync</c>):
''' a read (GET, as the receptions question), the robot download (through the controller, so the
''' gate closes exactly as in the application), the proposal (POST), the «Asociere» window
''' (simulated with a box; the queue shows that it waits for it) and the save (POST).</para>
''' <para><b>What to watch in the journal:</b> a GET is answered at once even while the robot
''' works; a POST sent during a run reaches the SERVER only after «poarta: deschisă»; the robot's
''' own mid-run request (marked Bypass, like the Excel step) passes the closed gate; tasks start in
''' the order they were asked; a duplicate is refused; the FOREXE operation jumps the waiting ones;
''' a robot started outside the queue is waited for.</para>
''' <para><b>Disk:</b> the controller writes its usual files for each simulated run (the answer in
''' «Rezultate_Forexe», the package in «WorkflowResults», the run log), under the PROBA-* codes.</para>
''' </remarks>
Public Class RobotQueueHarnessForm

    Private Const BaseUrl As String = "https://kbot-proba.invalid"

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _watch As Stopwatch = Stopwatch.StartNew()
    Private ReadOnly _gate As ServerGate
    Private ReadOnly _server As FakeServerHandler
    Private ReadOnly _http As HttpClient
    Private ReadOnly _runner As FakeForexeRunner
    Private ReadOnly _controller As ForexeController
    Private ReadOnly _queue As RobotQueue
    Private _queueForm As RobotQueueForm

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="log">The harness run log (every journal line is copied there too).</param>
    Public Sub New(log As Action(Of String))
        InitializeComponent()
        _log = log
        _gate = New ServerGate()
        _server = New FakeServerHandler(AddressOf Scrie)
        _http = New HttpClient(New ServerGateHandler(_gate, TimeSpan.FromSeconds(100), _server)) With {
            .BaseAddress = New Uri(BaseUrl),
            .Timeout = System.Threading.Timeout.InfiniteTimeSpan
        }
        _runner = New FakeForexeRunner() With {.MidRunCall = AddressOf CerereaRobotuluiAsync}
        _controller = New ForexeController(_runner, New SessionContext(), Nothing, _gate) With {.Owner = Me}
        _queue = New RobotQueue(Function() _controller.WaitUntilIdleAsync(),
                                Sub(text) Scrie("COADĂ  " & text))

        AddHandler _gate.StateChanged, AddressOf Gate_StateChanged
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
        AddHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        Try
            MyBase.OnShown(e)
            If _queue Is Nothing Then Return
            AplicaSetarile()
            Scrie("Banc pornit. FOREXE și serverul sunt SIMULATE; nimic nu pleacă de pe PC.")
            Scrie("Fișierele obișnuite ale descărcării se scriu totuși pe disc, pe codurile PROBA-*.")
            DeschideCoada(activate:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.OnShown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            ' The bench's own queue is dropped with it: nothing waiting survives the window.
            If _queue IsNot Nothing Then
                _queue.SetPaused(True)
                _queue.CancelAll()
                If _controller.IsBusy Then _controller.Cancel()
            End If
            If _queueForm IsNot Nothing AndAlso Not _queueForm.IsDisposed Then _queueForm.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.OnFormClosing", ex)
        End Try
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            If _gate IsNot Nothing Then RemoveHandler _gate.StateChanged, AddressOf Gate_StateChanged
            If _controller IsNot Nothing Then
                RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged
                RemoveHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
            End If
            _http?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' ── The simulated tasks ─────────────────────────────────────────────────────────

    ' The shape of KbotForm.DescarcaNodulAsync, over the simulated robot and server.
    Private Async Function ReimprospatareaAsync(cod As String) As Task
        Scrie($"[{cod}] SARCINA pornește — întrebarea recepțiilor citește lista (GET)")
        Await TrimiteAsync(HttpMethod.Get, $"/api/forexe/receptii?cod={cod}", cod)

        Dim pachet As PrelucrareRezultat = Await _controller.DownloadNodeAsync(
            cod, Function(c, ct) IstoriculAsync(c, ct), Nothing)
        If pachet Is Nothing Then
            Scrie($"[{cod}] robotul n-a adus nimic: {If(_controller.LastFailure.Length > 0, _controller.LastFailure, "(anulat)")}")
            Return
        End If
        Dim randuri As Integer = pachet.Tabele.Values.Sum(Function(t) t.Count)
        Scrie($"[{cod}] pachet: {pachet.Tabele.Count} tabele, {randuri} rânduri — urmează ingestia")

        Await TrimiteAsync(HttpMethod.Post, "/api/forexe/prelucrare/propunere", cod)
        If chkAsociere.Checked Then
            _queue.SetNote($"«{cod}»: așteaptă fereastra de asociere")
            Try
                Scrie($"[{cod}] «Asociere» deschisă — coada așteaptă operatorul")
                KBotMessage.Show(Me,
                    $"[SIMULARE] Fereastra «Asociere» pentru «{cod}»." & Environment.NewLine & Environment.NewLine &
                    "Cât stă deschisă, coada nu pornește sarcina următoare." & Environment.NewLine &
                    "OK = salvez.",
                    "Asociere (simulată)", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Finally
                _queue.SetNote(String.Empty)
            End Try
            Scrie($"[{cod}] «Asociere» închisă")
        End If
        Await TrimiteAsync(HttpMethod.Post, "/api/forexe/prelucrare/salveaza", cod)
        Scrie($"[{cod}] SARCINA gata")
    End Function

    ' The shape of KbotForm.PreiaOperatiuneaAsync: pictures first, then the download and the save.
    Private Async Function OperatiuneaAsync(cod As String) As Task
        Scrie($"[{cod}] OPERAȚIUNE FOREXE — capturile paginii (simulat), apoi descărcarea")
        Await Task.Delay(300).ConfigureAwait(True)
        Dim pachet As PrelucrareRezultat = Await _controller.DownloadNodeAsync(
            cod, Function(c, ct) IstoriculAsync(c, ct), Nothing)
        If pachet Is Nothing Then
            Scrie($"[{cod}] robotul n-a adus nimic: {_controller.LastFailure}")
            Return
        End If
        Await TrimiteAsync(HttpMethod.Post, "/api/forexe/prelucrare/salveaza", cod)
        Scrie($"[{cod}] OPERAȚIUNE gata")
    End Function

    ' The local history read that decides forward / REVERSE: a GET, made BEFORE the run. Returns
    ' Nothing (no local history), so the controller runs the complete processing.
    Private Async Function IstoriculAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo)
        Await TrimiteAsync(HttpMethod.Get, $"/api/forexe/istoric?cod={cod}", cod)
        Return Nothing
    End Function

    ' The robot's own request in the middle of a run (as the Excel step): marked Bypass.
    Private Async Function CerereaRobotuluiAsync() As Task
        Await TrimiteAsync(HttpMethod.Post, "/api/tools/process_excel", "robot", bypass:=True)
    End Function

    ' One request through the bench's gated HttpClient, with the client-side journal lines.
    Private Async Function TrimiteAsync(method As HttpMethod, path As String, who As String,
                                        Optional bypass As Boolean = False) As Task
        Dim start As Long = _watch.ElapsedMilliseconds
        Dim label As String = $"{method} {path.Split("?"c)(0)}"
        Scrie($"CLIENT → [{who}] trimite {label}" &
              If(bypass, " (Bypass)", If(_gate.IsClosed AndAlso Not ServerGate.IsRead(method), "  — poarta e ÎNCHISĂ, așteaptă", "")))
        Using msg As New HttpRequestMessage(method, path)
            If bypass Then msg.Options.Set(ServerGate.Bypass, True)
            If method <> HttpMethod.Get Then msg.Content = New StringContent("{""proba"":true}", Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg).ConfigureAwait(True)
                Await resp.Content.ReadAsStringAsync().ConfigureAwait(True)
                Scrie($"CLIENT ← [{who}] {label}: {CInt(resp.StatusCode)} după {_watch.ElapsedMilliseconds - start} ms")
            End Using
        End Using
    End Function

    ' ── Scenario buttons ───────────────────────────────────────────────────────────

    Private Sub BtnReimprospatare_Click(sender As Object, e As EventArgs) Handles btnReimprospatare.Click
        Try
            AplicaSetarile()
            If lstCoduri.SelectedItems.Count = 0 Then
                Scrie("Selectați cel puțin un nod din listă.")
                Return
            End If
            For Each item As Object In lstCoduri.SelectedItems
                PuneInCoada(CStr(item))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnReimprospatare_Click", ex)
        End Try
    End Sub

    Private Sub BtnRafala_Click(sender As Object, e As EventArgs) Handles btnRafala.Click
        Try
            AplicaSetarile()
            For Each item As Object In lstCoduri.Items
                PuneInCoada(CStr(item))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnRafala_Click", ex)
        End Try
    End Sub

    Private Sub BtnDublura_Click(sender As Object, e As EventArgs) Handles btnDublura.Click
        Try
            AplicaSetarile()
            Dim cod As String = If(lstCoduri.SelectedItem Is Nothing, CStr(lstCoduri.Items(0)), CStr(lstCoduri.SelectedItem))
            Scrie($"Cer «{cod}» de două ori — a doua cerere trebuie refuzată.")
            PuneInCoada(cod)
            PuneInCoada(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnDublura_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnOperatiune_Click(sender As Object, e As EventArgs) Handles btnOperatiune.Click
        Try
            AplicaSetarile()
            Dim cod As String = "PROBA-OPERATIUNE"
            Scrie($"Operațiune salvată în pagina FOREXE pe «{cod}» — intră în fața cozii.")
            Await _queue.RunAsync(Nothing, $"Operațiune FOREXE «{cod}»", Function() OperatiuneaAsync(cod), inFront:=True)
        Catch ex As RobotTaskDroppedException
            Scrie("Operațiunea a fost scoasă din coadă.")
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnOperatiune_Click", ex)
            Scrie("EROARE operațiune: " & ex.Message)
        End Try
    End Sub

    ' A robot run started OUTSIDE the queue (as the Conectare button or the Browser view): the
    ' queue must wait for it before starting its next task.
    Private Async Sub BtnDirect_Click(sender As Object, e As EventArgs) Handles btnDirect.Click
        Try
            AplicaSetarile()
            Scrie("ÎN AFARA COZII: robotul deschide «PROBA-DIRECT» (ca vederea «Browser FOREXE»).")
            Dim ok As Boolean = Await _controller.DeschideAngajamentAsync("PROBA-DIRECT")
            Scrie($"ÎN AFARA COZII: {If(ok, "gata", "refuzat / eșuat — " & _controller.LastFailure)}")
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnDirect_Click", ex)
            Scrie("EROARE în afara cozii: " & ex.Message)
        End Try
    End Sub

    Private Async Sub BtnCitire_Click(sender As Object, e As EventArgs) Handles btnCitire.Click
        Try
            AplicaSetarile()
            Await TrimiteAsync(HttpMethod.Get, "/api/forexe/tree?an=2026&ss=02", "manual")
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnCitire_Click", ex)
            Scrie("EROARE citire: " & ex.Message)
        End Try
    End Sub

    Private Async Sub BtnScriere_Click(sender As Object, e As EventArgs) Handles btnScriere.Click
        Try
            AplicaSetarile()
            Await TrimiteAsync(HttpMethod.Post, "/api/forexe/angajamente/upsert", "manual")
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnScriere_Click", ex)
            Scrie("EROARE scriere: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnCoada_Click(sender As Object, e As EventArgs) Handles btnCoada.Click
        Try
            DeschideCoada(activate:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.btnCoada_Click", ex)
        End Try
    End Sub

    Private Sub BtnGolesteJurnal_Click(sender As Object, e As EventArgs) Handles btnGolesteJurnal.Click
        txtJurnal.Clear()
    End Sub

    Private Sub BtnReusit_Click(sender As Object, e As EventArgs) Handles btnReusit.Click
        DialogResult = DialogResult.Yes
        Close()
    End Sub

    Private Sub BtnEsuat_Click(sender As Object, e As EventArgs) Handles btnEsuat.Click
        DialogResult = DialogResult.No
        Close()
    End Sub

    ' ── Helpers ────────────────────────────────────────────────────────────────────

    ' A node refresh, exactly as the shell queues it (same key, same label shape).
    Private Async Sub PuneInCoada(cod As String)
        Try
            Await _queue.RunAsync("nod|" & cod, $"Descărcare completă «{cod}»", Function() ReimprospatareaAsync(cod))
        Catch ex As RobotTaskDroppedException
            Scrie($"[{cod}] nu a rulat: {ex.Message}")
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.PuneInCoada", ex)
            Scrie($"[{cod}] EROARE: {ex.Message}")
        End Try
    End Sub

    ' The simulation settings are read at every command, so they can change between tasks.
    Private Sub AplicaSetarile()
        _runner.RunSeconds = CDbl(numDurata.Value)
        _runner.FailRuns = chkEsec.Checked
        _server.DelayMs = CInt(numServer.Value)
    End Sub

    Private Sub DeschideCoada(activate As Boolean)
        If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then
            _queueForm = New RobotQueueForm(_queue, _controller)
            AddHandler _queueForm.FormClosed, Sub() _queueForm = Nothing
            Dim r As Rectangle = RectangleToScreen(ClientRectangle)
            _queueForm.Location = New Point(Math.Max(0, r.Right - _queueForm.Width - 16), r.Top + 90)
            _queueForm.Show(Me)
        End If
        If activate Then _queueForm.Activate()
    End Sub

    Private Sub Gate_StateChanged(sender As Object, e As EventArgs)
        Try
            PeUi(Sub()
                     Dim inchisa As Boolean = _gate.IsClosed
                     lblPoarta.Text = If(inchisa, "Poarta: ÎNCHISĂ (scrierile așteaptă)", "Poarta: deschisă")
                     Dim p = ThemeManager.Current.Palette
                     lblPoarta.ForeColor = If(inchisa, p.WarningColor, p.TextColor)
                     Scrie(If(inchisa, "POARTA: închisă — robotul lucrează", "POARTA: deschisă"))
                 End Sub)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.Gate_StateChanged", ex)
        End Try
    End Sub

    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            PeUi(Sub() lblRobot.Text = If(_controller.IsBusy, "Robot: LUCREAZĂ", "Robot: liber"))
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.Controller_StateChanged", ex)
        End Try
    End Sub

    Private Sub Controller_StatusChanged(sender As Object, stare As String)
        Try
            ' Only the controller's own lines; the ten progress steps would drown the journal.
            If stare Is Nothing OrElse stare.Contains("pasul ") Then Return
            Scrie("ROBOT  " & stare)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.Controller_StatusChanged", ex)
        End Try
    End Sub

    ' One journal line, from any thread: time since the bench opened, then the text.
    Private Sub Scrie(text As String)
        Dim line As String = $"{_watch.Elapsed:mm\:ss\.fff}  {text}"
        Try
            _log?.Invoke(line)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.Scrie", ex)
        End Try
        PeUi(Sub() txtJurnal.AppendText(line & Environment.NewLine))
    End Sub

    Private Sub PeUi(action As Action)
        If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(action)
        Else
            action()
        End If
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme = ThemeManager.Current
            Dim p = scheme.Palette
            txtJurnal.BackColor = p.SurfaceAltColor
            txtJurnal.ForeColor = p.TextColor
            For Each b As Button In flpComenzi.Controls.OfType(Of Button)()
                ButtonStyles.ApplySecondary(b, scheme)
            Next
            ButtonStyles.ApplyPrimary(btnReusit, scheme)
            ButtonStyles.ApplySecondary(btnEsuat, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class
#End If
