Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
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
            Return New AdeSettings()
        End Try
    End Function

    Public Sub Save()
        Try
            File.WriteAllText(FilePath(), JsonSerializer.Serialize(Me), Encoding.UTF8)
        Catch ex As Exception
            GlobalErrorLog.Write("AdeSettings.Save", ex)
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

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ---- lifetime ------------------------------------------------------------------------------------------------

    Private Sub AdeMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            txtGazda.Text = _settings.Host
            txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
            txtUtilizator.Text = _settings.User
            txtFisier.Text = If(_settings.LastFile.Length > 0 AndAlso File.Exists(_settings.LastFile), _settings.LastFile, GuessFile())
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.Load", ex)
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
        btnRasfoire.Enabled = Not k_busy
        btnCiteste.Enabled = Not k_busy
        btnTesteaza.Enabled = Not k_busy
        dgvEducatori.Enabled = Not k_busy
        If k_busy Then
            btnMigreaza.Enabled = False
        Else
            RefreshMigrateButton()
        End If
        UseWaitCursor = k_busy
    End Sub

    Private Sub Say(k_text As String)
        lblStare.Text = k_text
    End Sub

    ' ---- the file ------------------------------------------------------------------------------------------------

    Private Sub btnRasfoire_Click(sender As Object, e As EventArgs) Handles btnRasfoire.Click
        Try
            Using k_dialog As New OpenFileDialog() With {
                .Title = "Alegeți baza Access ADECHIT",
                .Filter = "Baze Access (*.mdb)|*.mdb|Toate fișierele (*.*)|*.*",
                .InitialDirectory = BrowseFolder(),
                .CheckFileExists = True}
                If k_dialog.ShowDialog(Me) = DialogResult.OK Then txtFisier.Text = k_dialog.FileName
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.btnRasfoire_Click", ex)
        End Try
    End Sub

    Private Async Sub btnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
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
                _settings.Save()
                lblDc.Text = $"DC: {_source.Dc}"
                FillEducators()
                RebuildPlan()
                Say("Gata. Verificați educatorii, apoi testați serverul.")
            Catch ex As Exception
                GlobalErrorLog.Write("AdeMigratorForm.btnCiteste_Click.read", ex)
                Say("Citirea a eșuat.")
                KBotMessage.Show("Fișierul nu a putut fi citit:" & Environment.NewLine & Environment.NewLine & ex.Message,
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.btnCiteste_Click", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---- educators and the plan ----------------------------------------------------------------------------------

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

    Private Sub dgvEducatori_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvEducatori.CellValueChanged
        Try
            If _busy OrElse _source Is Nothing Then Return
            RebuildPlan()
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.dgvEducatori_CellValueChanged", ex)
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
        _plan = AdePlan.Build(_source, ReadEducators())
        _targetProblems = Nothing
        ShowPlan()
        RefreshMigrateButton()
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

    Private Async Sub btnTesteaza_Click(sender As Object, e As EventArgs) Handles btnTesteaza.Click
        Try
            SetBusy(True)
            lblStareServer.Text = "Se testează..."
            Try
                Dim k_server = BuildServer()
                Dim k_version = Await Task.Run(Function() k_server.TestConnection())
                lblStareServer.Text = $"Conectat — MariaDB {k_version}"
                _settings.Host = txtGazda.Text.Trim()
                _settings.Port = Integer.Parse(txtPort.Text.Trim(), CultureInfo.InvariantCulture)
                _settings.User = txtUtilizator.Text.Trim()
                _settings.Save()
                If _plan IsNot Nothing AndAlso _source IsNot Nothing Then
                    Dim k_dc = _source.Dc
                    Dim k_planNow = _plan
                    Dim k_problems = Await Task.Run(Function() AdeWriter.Problems(k_server, k_dc, k_planNow))
                    _targetProblems = k_problems
                    _targetCounts = If(k_problems.Any(Function(p) p.StartsWith("Pe server nu există baza", StringComparison.Ordinal)),
                                       Nothing,
                                       Await Task.Run(Function() AdeWriter.CountRows(k_server, k_dc, k_planNow.Tables.Select(Function(t) t.Table))))
                    ShowPlan()
                    Say(If(k_problems.Count = 0, $"Baza «{k_dc}» este gata pentru migrare.", $"Baza «{k_dc}» nu este gata — vezi «Conversii și constatări»."))
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("AdeMigratorForm.btnTesteaza_Click.test", ex)
                lblStareServer.Text = "Conectare eșuată"
                KBotMessage.Show("Nu m-am putut conecta la server:" & Environment.NewLine & Environment.NewLine & ex.Message,
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.btnTesteaza_Click", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---- migrate -------------------------------------------------------------------------------------------------

    Private Sub RefreshMigrateButton()
        btnMigreaza.Enabled = Not _busy AndAlso _plan IsNot Nothing AndAlso Not _plan.HasBlocking AndAlso
                              _targetProblems IsNot Nothing AndAlso _targetProblems.Count = 0
    End Sub

    Private Async Sub btnMigreaza_Click(sender As Object, e As EventArgs) Handles btnMigreaza.Click
        Try
            If _plan Is Nothing OrElse _source Is Nothing Then Return
            Dim k_total = _plan.Tables.Sum(Function(t) t.Rows.Count)
            Dim k_answer = KBotMessage.Show(
                $"Se scriu {k_total} rânduri în baza «{_source.Dc}» de pe {txtGazda.Text.Trim()}." & Environment.NewLine &
                "Totul se face într-o singură tranzacție: dacă ceva eșuează, baza rămâne neschimbată." & Environment.NewLine & Environment.NewLine &
                "Continuați?", "Migrare ADE", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If k_answer <> DialogResult.Yes Then Return
            SetBusy(True)
            Try
                Dim k_server = BuildServer()
                Dim k_dc = _source.Dc
                Dim k_sourceNow = _source
                Dim k_planNow = _plan
                Dim k_progress As IProgress(Of String) = New Progress(Of String)(AddressOf Say)
                Dim k_written = Await Task.Run(Function() AdeWriter.Write(k_server, k_dc, k_sourceNow, k_planNow, k_progress))
                _targetCounts = Await Task.Run(Function() AdeWriter.CountRows(k_server, k_dc, k_planNow.Tables.Select(Function(t) t.Table)))
                _targetProblems = New List(Of String) From {"Migrarea s-a făcut deja; tabelele nu mai sunt goale."}
                ShowPlan()
                Say("Migrare terminată.")
                KBotMessage.Show($"Migrare terminată: {k_written.Values.Sum()} rânduri în {k_written.Count} tabele.",
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                GlobalErrorLog.Write("AdeMigratorForm.btnMigreaza_Click.write", ex)
                Say("Migrarea a eșuat; baza a rămas neschimbată.")
                KBotMessage.Show("Migrarea a eșuat; baza a rămas neschimbată." & Environment.NewLine & Environment.NewLine & ex.Message,
                                 "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("AdeMigratorForm.btnMigreaza_Click", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

End Class
