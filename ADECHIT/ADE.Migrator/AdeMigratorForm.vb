Imports System.Globalization
Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' Host, port and user of the last run. Never a password.
''' </summary>
Public NotInheritable Class AdeSettings

    Private Const FileName As String = "ade-migrator-settings.json"

    Public Property Host As String = "127.0.0.1"
    Public Property Port As Integer = 3306
    Public Property User As String = "root"
    Public Property LastFile As String = String.Empty

    Private Shared Function FilePath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

    Public Shared Function Load() As AdeSettings
        Try
            If Not File.Exists(FilePath()) Then Return New AdeSettings()
            Return If(JsonSerializer.Deserialize(Of AdeSettings)(File.ReadAllText(FilePath(), Encoding.UTF8)), New AdeSettings())
        Catch ex As Exception
            ' A damaged settings file must not stop the tool: start from the defaults.
            GlobalErrorLog.Write("AdeSettings.Load", ex)
            KBotMessage.Show("Setările nu au putut fi citite. Se folosesc valorile implicite." & Environment.NewLine & ex.Message,
                             "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return New AdeSettings()
        End Try
    End Function

    Public Sub Save()
        Try
            File.WriteAllText(FilePath(), JsonSerializer.Serialize(Me), Encoding.UTF8)
        Catch ex As Exception
            ' The event handler records and displays this failure.
            Throw
        End Try
    End Sub

End Class

''' <summary>
''' The migration console: pick the Access file, read it, settle the educators' names, test the server, migrate.
''' Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AdeMigratorForm

    Private ReadOnly _settings As AdeSettings = AdeSettings.Load()
    Private _source As AdeSource
    Private _plan As AdePlan
    ''' <summary>Problems the server check found; Nothing = the server has not been checked for this plan.</summary>
    Private _targetProblems As List(Of String)
    Private _targetCounts As Dictionary(Of String, Integer)
    Private _busy As Boolean
    Private _cancellation As CancellationTokenSource
    Private _operationFinished As TaskCompletionSource(Of Boolean)
    Private _lastMigrationState As String = String.Empty
    Private _targetCheckError As String = String.Empty
    Private _initializing As Boolean = True

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ---- lifetime ------------------------------------------------------------------------------------------------

    Private Sub AdeMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            lblLogPath.Text = LogPaths.Combine(GlobalErrorLog.FileNameOnly)
            txtGazda.Text = _settings.Host
            txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
            txtUtilizator.Text = _settings.User
            txtFisier.Text = If(_settings.LastFile.Length > 0 AndAlso File.Exists(_settings.LastFile), _settings.LastFile, GuessFile())
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Load", "Inițializarea ferestrei a eșuat.", ex)
        Finally
            _initializing = False
        End Try
    End Sub

    ''' <summary>The usual folder is C:\adechit; when it holds exactly one .mdb, that is the file.</summary>
    Private Shared Function GuessFile() As String
        For Each k_folder In {"C:\adechit", "C:\adchit"}
            If Not Directory.Exists(k_folder) Then Continue For
            Dim k_files = Directory.GetFiles(k_folder, "*.mdb")
            If k_files.Length = 1 Then Return k_files(0)
        Next
        Return String.Empty
    End Function

    Private Function BrowseFolder() As String
        For Each k_folder In {"C:\adechit", "C:\adchit"}
            If Directory.Exists(k_folder) Then Return k_folder
        Next
        Return String.Empty
    End Function

    ' ---- busy ----------------------------------------------------------------------------------------------------

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        grpSursa.Enabled = Not k_busy
        grpServer.Enabled = Not k_busy
        btnStop.Enabled = k_busy AndAlso _cancellation IsNot Nothing
        If k_busy Then
            _operationFinished = New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
        Else
            _cancellation?.Dispose()
            _cancellation = Nothing
            _operationFinished?.TrySetResult(True)
        End If
        btnRasfoire.Enabled = Not k_busy
        btnCiteste.Enabled = Not k_busy
        btnTesteaza.Enabled = Not k_busy
        dgvEducatori.Enabled = Not k_busy
        dgvGrupe.Enabled = Not k_busy
        If k_busy Then
            btnMigreaza.Enabled = False
        Else
            RefreshMigrateButton()
        End If
        RefreshMigrateButton()
        UseWaitCursor = k_busy
    End Sub

    ''' <summary>Changing any input invalidates the verification that enabled a write.</summary>
    Private Sub InputsChanged(sender As Object, e As EventArgs) Handles txtFisier.TextChanged, txtGazda.TextChanged, txtPort.TextChanged, txtUtilizator.TextChanged, txtParola.TextChanged, txtTargetDc.TextChanged
        Try
            _targetCheckError = String.Empty
            _targetProblems = Nothing
            _targetCounts = Nothing
            If sender Is txtFisier Then
                _source = Nothing
                _plan = Nothing
                txtTargetDc.Text = String.Empty
                dgvTabele.ClearRows()
                dgvEducatori.ClearRows()
                dgvGrupe.ClearRows()
                rtbConversii.Clear()
                lblRezumat.Text = "Fișier schimbat. Apăsați «Citește» înainte de verificare."
            End If
            lblStareServer.Text = "Netestat"
            RefreshMigrateButton()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.InputsChanged", "Actualizarea selecției a eșuat.", ex)
        End Try
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        Try
            _cancellation?.Cancel()
            btnStop.Enabled = False
            Say("Oprire solicitată. Așteptați terminarea instrucțiunii curente și rollback-ul.")
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Stop", "Oprirea nu a putut fi solicitată.", ex)
        End Try
    End Sub

    ''' <summary>Keep the process alive until the worker has released its connections.</summary>
    Private Async Sub AdeMigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If Not _busy Then Return
            e.Cancel = True
            If KBotMessage.Show("O operație este în curs. Așteptați terminarea ei înainte de închidere. Închideți după terminare?",
                                "Migrare ADE", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            _cancellation?.Cancel()
            Dim k_pending = _operationFinished
            If k_pending IsNot Nothing Then Await k_pending.Task
            Close()
        Catch ex As Exception
            e.Cancel = True
            ShowOperationError("AdeMigratorForm.FormClosing", "Închiderea a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>UI errors stay visible; the shared log preserves their technical details.</summary>
    Private Sub ShowOperationError(k_context As String, k_message As String, k_error As Exception)
        GlobalErrorLog.Write(k_context, k_error)
        AppendLog("EROARE", k_message & Environment.NewLine & k_error.ToString())
        Say(k_message)
        KBotMessage.Show(k_message & Environment.NewLine & Environment.NewLine & k_error.Message,
                         "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub Say(k_text As String)
        lblStare.Text = k_text
        AppendLog("INFO", k_text)
    End Sub

    ''' <summary>Keep session diagnostics visible without replacing the shared error logger.</summary>
    Private Sub AppendLog(level As String, message As String)
        If _initializing AndAlso level = "STARE MIGRARE" Then Return
        If rtbLog Is Nothing OrElse rtbLog.IsDisposed Then Return
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {level}: {message}" & Environment.NewLine)
        rtbLog.SelectionStart = rtbLog.TextLength
        rtbLog.ScrollToCaret()
    End Sub

    Private Sub BtnLoadErrors_Click(sender As Object, e As EventArgs) Handles btnLoadErrors.Click
        Try
            LoadErrorLog()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.LoadErrorLog", "Citirea jurnalului de erori a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>Read a bounded tail of the common log, allowing the logger to keep writing.</summary>
    Private Sub LoadErrorLog()
        Dim logPath = LogPaths.Combine(GlobalErrorLog.FileNameOnly)
        lblLogPath.Text = logPath
        If Not File.Exists(logPath) Then
            AppendLog("INFO", "Nu există încă un jurnal comun de erori: " & logPath)
            Return
        End If
        Using stream As New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim truncated = stream.Length > 262144
            If truncated Then stream.Seek(-262144, SeekOrigin.End)
            Using reader As New StreamReader(stream, Encoding.UTF8)
                ' Discard a partial first line when the byte window starts inside an entry.
                If truncated Then reader.ReadLine()
                AppendLog("JURNAL COMUN", reader.ReadToEnd())
            End Using
        End Using
    End Sub

    ''' <summary>Preference persistence is separate from validating a source or destination.</summary>
    Private Sub SavePreferences()
        Try
            _settings.Save()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.SavePreferences", "Setările locale nu au putut fi salvate. Verificarea poate continua.", ex)
        End Try
    End Sub

    ' ---- the file ------------------------------------------------------------------------------------------------

    Private Sub BtnRasfoire_Click(sender As Object, e As EventArgs) Handles btnRasfoire.Click
        Try
            Using k_dialog As New OpenFileDialog() With {
                .Title = "Alegeți baza Access ADECHIT",
                .Filter = "Baze Access (*.mdb)|*.mdb|Toate fișierele (*.*)|*.*",
                .InitialDirectory = BrowseFolder(),
                .CheckFileExists = True}
                If k_dialog.ShowDialog(Me) = DialogResult.OK Then txtFisier.Text = k_dialog.FileName
            End Using
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Browse", "Alegerea fișierului a eșuat.", ex)
        End Try
    End Sub

    Private Async Sub BtnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
        If _busy Then Return
        Try
            Dim k_path = txtFisier.Text.Trim()
            If k_path.Length = 0 OrElse Not File.Exists(k_path) Then
                KBotMessage.Show("Alegeți un fișier .mdb existent.", "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            SetBusy(True)
            Say("Citesc fișierul Access...")
            Try
                _source = Nothing
                _plan = Nothing
                _targetProblems = Nothing
                _targetCounts = Nothing
                _source = Await Task.Run(Function() AdeSource.Read(k_path))
                _settings.LastFile = k_path
                SavePreferences()
                txtTargetDc.Text = _source.Dc
                Say($"DC descoperit în Access: {_source.Dc}. Puteți schimba DC-ul destinației înainte de verificare.")
                FillGroups()
                FillEducators()
                RebuildPlan()
                Say("Gata. Verificați grupa de plecați și educatorii, apoi testați serverul.")
            Catch ex As Exception
                ShowOperationError("AdeMigratorForm.ReadSource", "Fișierul nu a putut fi citit.", ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Read", "Citirea fișierului a eșuat.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---- educators and the plan ----------------------------------------------------------------------------------

    Private Sub FillGroups()
        dgvGrupe.BeginUpdate()
        Try
            dgvGrupe.ClearRows()
            For Each k_sourceRow As DataRow In _source.Tables("Grupe").Select(String.Empty, "Grupa ASC")
                Dim k_id = AdeValue.ToInt(k_sourceRow("IDG"), Nothing)
                If k_id Is Nothing Then Continue For
                Dim k_name = Convert.ToString(k_sourceRow("Grupa"), CultureInfo.InvariantCulture)
                Dim k_row = dgvGrupe.AddRow()
                k_row.Tag = CInt(k_id)
                k_row("grupa") = k_name
                k_row("plecati") = AdePlan.IsDepartedGroup(k_name)
            Next
        Finally
            dgvGrupe.EndUpdate()
        End Try
    End Sub

    Private Sub DgvGrupe_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvGrupe.CellValueChanged
        Try
            If _busy OrElse _source Is Nothing Then Return
            RebuildPlan()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.GroupTypes", "Refacerea planului a eșuat. Verificați bifele grupelor de plecați.", ex)
        End Try
    End Sub

    Private Function ReadGroupTypes() As Dictionary(Of Integer, Boolean)
        Dim k_types As New Dictionary(Of Integer, Boolean)()
        For Each k_row In dgvGrupe.Rows
            k_types(CInt(k_row.Tag)) = Convert.ToBoolean(k_row("plecati"), CultureInfo.InvariantCulture)
        Next
        Return k_types
    End Function

    Private Sub FillEducators()
        dgvEducatori.BeginUpdate()
        Try
            dgvEducatori.ClearRows()
            For Each k_entry In AdeEducators.Candidates(_source)
                Dim k_row = dgvEducatori.AddRow()
                k_row.Tag = k_entry
                k_row("grupa") = k_entry.GroupName
                k_row("brut") = k_entry.Raw
                k_row("nume") = k_entry.Names
            Next
        Finally
            dgvEducatori.EndUpdate()
        End Try
    End Sub

    Private Sub DgvEducatori_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvEducatori.CellValueChanged
        Try
            If _busy OrElse _source Is Nothing Then Return
            RebuildPlan()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Educators", "Refacerea planului a eșuat. Corectați educatorii și verificați din nou.", ex)
        End Try
    End Sub

    ''' <summary>The names the operator settled on, per (group, text in Access).</summary>
    Private Function ReadEducators() As Dictionary(Of (Integer, String), String)
        Dim k_map As New Dictionary(Of (Integer, String), String)()
        For Each k_row In dgvEducatori.Rows
            Dim k_entry = TryCast(k_row.Tag, AdeEducatorEntry)
            If k_entry Is Nothing Then Continue For
            k_map((k_entry.IdG, k_entry.Raw)) = Convert.ToString(k_row("nume"), CultureInfo.InvariantCulture)
        Next
        Return k_map
    End Function

    Private Sub RebuildPlan()
        ' Clear the previous plan before rebuilding so a failed edit cannot leave a writable stale plan.
        _plan = Nothing
        _targetCheckError = String.Empty
        _targetProblems = Nothing
        _targetCounts = Nothing
        RefreshMigrateButton()
        _plan = AdePlan.Build(_source, ReadEducators(), ReadGroupTypes(), chkChildCnpIsParent.Checked)
        _targetProblems = Nothing
        ShowPlan()
        For Each finding In _plan.Findings
            AppendLog(If(finding.Blocking, "BLOCAJ ACCESS", "OBSERVAȚIE"), finding.Text)
        Next
        RefreshMigrateButton()
    End Sub

    Private Sub ChildCnpIsParent_CheckedChanged(sender As Object, e As EventArgs) Handles chkChildCnpIsParent.CheckedChanged
        Try
            If _busy OrElse _source Is Nothing Then Return
            RebuildPlan()
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.ParentCnp", "Refacerea planului pentru CNP-ul părintelui a eșuat.", ex)
        End Try
    End Sub

    Private Sub ShowPlan()
        dgvTabele.BeginUpdate()
        Try
            dgvTabele.ClearRows()
            For Each k_planTable In _plan.Tables
                Dim k_row = dgvTabele.AddRow()
                k_row("tabel") = If(k_planTable.SourceRows >= 0, k_planTable.Table.Name, "(construit)")
                k_row("randuriAccess") = If(k_planTable.SourceRows >= 0, k_planTable.SourceRows.ToString(CultureInfo.InvariantCulture), "—")
                k_row("tinta") = k_planTable.Table.Target
                k_row("deScris") = k_planTable.Rows.Count.ToString(CultureInfo.InvariantCulture)
                k_row("inBaza") = CountText(k_planTable.Table.Name)
            Next
            If _plan.ReceiptConfig IsNot Nothing Then
                Dim k_receipt = dgvTabele.AddRow()
                k_receipt("tabel") = "CFGs (CH)"
                k_receipt("randuriAccess") = "—"
                k_receipt("tinta") = "Unitati_Chitante"
                k_receipt("deScris") = "1"
                k_receipt("inBaza") = "—"
            End If
        Finally
            dgvTabele.EndUpdate()
        End Try
        ShowNotes()
        Dim k_total = _plan.Tables.Sum(Function(t) t.Rows.Count)
        Dim k_blocking = _plan.Findings.Where(Function(f) f.Blocking).Count()
        lblRezumat.Text = If(k_blocking > 0,
            $"{k_total} rânduri de scris, dar {k_blocking} blocaje — vezi «Conversii și constatări».",
            $"{k_total} rânduri de scris în {_plan.Tables.Count} tabele.")
    End Sub

    Private Function CountText(k_tableName As String) As String
        If _targetCounts Is Nothing Then Return "—"
        Dim k_n As Integer
        If Not _targetCounts.TryGetValue(k_tableName, k_n) Then Return "—"
        Return If(k_n < 0, "lipsă", k_n.ToString(CultureInfo.InvariantCulture))
    End Function

    Private Sub ShowNotes()
        rtbConversii.Clear()
        Dim k_bold As New Font(rtbConversii.Font, FontStyle.Bold)
        Try
            If _targetProblems IsNot Nothing AndAlso _targetProblems.Count > 0 Then
                AppendLine("Serverul nu e gata:", k_bold)
                For Each k_problem In _targetProblems
                    AppendLine("  • " & k_problem, Nothing)
                Next
                AppendLine(String.Empty, Nothing)
            End If
            If _plan.Findings.Count > 0 Then
                AppendLine("Constatări:", k_bold)
                For Each k_finding In _plan.Findings
                    AppendLine((If(k_finding.Blocking, "  • BLOCAT: ", "  • ")) & k_finding.Text, Nothing)
                Next
                AppendLine(String.Empty, Nothing)
            End If
            AppendLine("Conversii aplicate:", k_bold)
            For Each k_note In _plan.Notes
                AppendLine("  • " & k_note, Nothing)
            Next
            AppendLine("  • Nu se citesc din Access: " & AdePlan.NotMigrated & ".", Nothing)
        Finally
            k_bold.Dispose()
        End Try
        rtbConversii.SelectionStart = 0
        rtbConversii.ScrollToCaret()
    End Sub

    Private Sub AppendLine(k_text As String, k_font As Font)
        If k_font IsNot Nothing Then rtbConversii.SelectionFont = k_font
        rtbConversii.AppendText(k_text & Environment.NewLine)
        If k_font IsNot Nothing Then rtbConversii.SelectionFont = rtbConversii.Font
    End Sub

    ' ---- the server ----------------------------------------------------------------------------------------------

    Private Function BuildServer() As TargetServer
        Dim k_port As Integer
        If Not Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_port) OrElse k_port < 1 OrElse k_port > 65535 Then
            Throw New InvalidOperationException("Portul nu este un număr între 1 și 65535.")
        End If
        If txtGazda.Text.Trim().Length = 0 Then Throw New InvalidOperationException("Completați gazda serverului.")
        Return New TargetServer(New TargetConnection(txtGazda.Text.Trim(), CUInt(k_port), txtUtilizator.Text.Trim(), txtParola.Text))
    End Function

    ''' <summary>The operator's destination is separate from the unchanged unit identity read from Access.</summary>
    Private Function ReadTargetDc() As String
        Dim targetDc = txtTargetDc.Text.Trim()
        If targetDc.Length = 0 Then Throw New InvalidOperationException("Completați DC-ul destinației.")
        Return targetDc
    End Function

    Private Async Sub BtnTesteaza_Click(sender As Object, e As EventArgs) Handles btnTesteaza.Click
        If _busy Then Return
        Try
            _targetCheckError = String.Empty
            _targetProblems = Nothing
            _targetCounts = Nothing
            SetBusy(True)
            lblStareServer.Text = "Se testează..."
            Say($"Verific conexiunea MariaDB către {txtGazda.Text.Trim()}:{txtPort.Text.Trim()}...")
            Try
                Dim k_server = BuildServer()
                Dim k_version = Await Task.Run(Function() k_server.TestConnection())
                lblStareServer.Text = $"Conectat — MariaDB {k_version}"
                _settings.Host = txtGazda.Text.Trim()
                _settings.Port = Integer.Parse(txtPort.Text.Trim(), CultureInfo.InvariantCulture)
                _settings.User = txtUtilizator.Text.Trim()
                SavePreferences()
                If _plan IsNot Nothing AndAlso _source IsNot Nothing Then
                    Dim k_dc = ReadTargetDc()
                    Dim k_planNow = _plan
                    Dim k_problems = Await Task.Run(Function() AdeWriter.Problems(k_server, k_dc, k_planNow))
                    _targetProblems = k_problems
                    For Each problem In k_problems
                        AppendLog("BLOCAJ SERVER", problem)
                    Next
                    _targetCounts = If(k_problems.Any(Function(p) p.StartsWith("Pe server nu există baza", StringComparison.Ordinal)),
                                       Nothing,
                                       Await Task.Run(Function() AdeWriter.CountRows(k_server, k_dc, k_planNow.Tables.Select(Function(t) t.Table))))
                    ShowPlan()
                    Say(If(k_problems.Count = 0, $"Baza «{k_dc}» este gata pentru migrare.", $"Baza «{k_dc}» nu este gata — vezi «Conversii și constatări»."))
                Else
                    Say("Conexiunea a reușit, dar nu există un plan citit. Apăsați «Citește», apoi «Testează» pentru verificarea destinației.")
                End If
            Catch ex As Exception
                _targetCheckError = "Verificarea serverului a eșuat: " & ex.Message
                _targetProblems = Nothing
                _targetCounts = Nothing
                lblStareServer.Text = "Verificare eșuată"
                ShowOperationError("AdeMigratorForm.VerifyTarget", "Verificarea serverului a eșuat.", ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Test", "Verificarea serverului a eșuat.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---- migrate -------------------------------------------------------------------------------------------------

    Private Sub RefreshMigrateButton()
        Dim reasons As New List(Of String)
        If _busy Then reasons.Add("O operație este în curs; așteptați terminarea ei.")
        If String.IsNullOrWhiteSpace(txtTargetDc.Text) Then reasons.Add("Completați DC-ul destinației.")
        If _source Is Nothing OrElse _plan Is Nothing Then
            reasons.Add("Apăsați «Citește» pentru încărcarea fișierului și construirea planului.")
        Else
            reasons.AddRange(_plan.Findings.Where(Function(f) f.Blocking).Select(Function(f) f.Text))
        End If
        If _targetProblems Is Nothing Then
            reasons.Add(If(_targetCheckError.Length > 0, _targetCheckError,
                           "Apăsați «Testează» după citire și după ultima modificare pentru verificarea destinației."))
        Else
            reasons.AddRange(_targetProblems)
        End If
        btnMigreaza.Enabled = reasons.Count = 0
        Dim state = If(reasons.Count = 0, "Migrarea poate porni.", "Migrare blocată: " & reasons(0))
        If reasons.Count > 1 Then state &= $" Mai sunt {reasons.Count - 1} motive; vedeți jurnalul de mai jos."
        lblMigrationState.Text = state
        If state <> _lastMigrationState Then
            _lastMigrationState = state
            AppendLog("STARE MIGRARE", If(reasons.Count = 0, state, String.Join(Environment.NewLine, reasons)))
        End If
    End Sub

    Private Async Sub BtnMigreaza_Click(sender As Object, e As EventArgs) Handles btnMigreaza.Click
        If _busy Then Return
        Dim k_committed = False
        Try
            If _busy OrElse _plan Is Nothing OrElse _source Is Nothing OrElse _plan.HasBlocking OrElse
                _targetProblems Is Nothing OrElse _targetProblems.Count > 0 Then Return
            Dim k_total = _plan.Tables.Sum(Function(t) t.Rows.Count)
            Dim targetDc = ReadTargetDc()
            Dim k_answer = KBotMessage.Show(
                $"Se scriu {k_total} rânduri în baza «{targetDc}» de pe {txtGazda.Text.Trim()}." & Environment.NewLine &
                $"DC sursă Access: {_source.Dc}. DC destinație: {targetDc}." & Environment.NewLine &
                "Scrierea folosește o singură tranzacție. Dacă rezultatul nu poate fi confirmat, verificați serverul înainte de reluare." & Environment.NewLine & Environment.NewLine &
                "Continuați?", "Migrare ADE", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If k_answer <> DialogResult.Yes Then Return
            _cancellation = New CancellationTokenSource()
            Dim k_cancel = _cancellation.Token
            SetBusy(True)
            Try
                Dim k_server = BuildServer()
                Dim k_dc = targetDc
                Dim k_sourceNow = _source
                Dim k_planNow = _plan
                Dim k_run = _operationFinished
                Dim k_progress As IProgress(Of String) = New Progress(Of String)(
                    Sub(message)
                        Try
                            If _busy AndAlso _operationFinished Is k_run AndAlso Not k_cancel.IsCancellationRequested Then Say(message)
                        Catch ex As Exception
                            ShowOperationError("AdeMigratorForm.Progress", "Afișarea progresului a eșuat.", ex)
                        End Try
                    End Sub)
                Dim k_journalRoot = Path.Combine(AppContext.BaseDirectory, "Jurnale", "ADE")
                Dim k_written = Await Task.Run(Function() AdeWriter.Write(k_server, k_dc, k_sourceNow, k_planNow, k_progress, k_cancel, k_journalRoot))
                k_committed = True
                ' These are committed counts returned by the writer. A later query must not turn a committed run into a failure.
                _targetCounts = k_written
                _targetProblems = New List(Of String) From {"Migrarea s-a făcut deja; tabelele nu mai sunt goale."}
                ShowPlan()
                Say("Migrare terminată.")
                KBotMessage.Show($"Migrare terminată: {k_written.Values.Sum()} rânduri în {k_written.Count} tabele." & Environment.NewLine & "Jurnalele SQL sunt în: " & k_journalRoot,
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As OperationCanceledException
                _targetProblems = Nothing
                Say("Migrare oprită. Verificați din nou serverul înainte de reluare.")
                KBotMessage.Show("Migrarea a fost oprită înainte de confirmarea tranzacției. Verificați din nou serverul înainte de reluare.",
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                _targetProblems = Nothing
                Dim k_message = If(k_committed, "Migrarea este confirmată, dar afișarea rezultatului a eșuat. Verificați jurnalul.",
                                   "Migrarea nu a fost confirmată. Verificați serverul și jurnalul înainte de reluare.")
                ShowOperationError("AdeMigratorForm.Write", k_message, ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AdeMigratorForm.Migrate", "Operația de migrare nu a putut fi finalizată.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

End Class
