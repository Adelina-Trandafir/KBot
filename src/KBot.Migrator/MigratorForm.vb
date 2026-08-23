Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The migration console: pick the registry, the server, the units and the tables, verify,
''' then transfer.
''' </summary>
''' <remarks>
''' <para>
''' Every control is declared in the designer file. This class only wires behaviour.
''' </para>
''' <para>
''' The two long operations run OFF the UI thread and are cancellable; cancelling a
''' transfer rolls the transaction back, so the database is left exactly as it was.
''' «Transferă» stays disabled until a verification passes with no blocking finding.
''' </para>
''' <para>
''' Passwords are read from the boxes at the moment a request is built and are never
''' stored, logged or written to disk.
''' </para>
''' </remarks>
Public Class MigratorForm

    Private ReadOnly _settings As MigratorSettings = MigratorSettings.Load()
    Private _units As New List(Of CaiUnit)()
    Private _cancellation As CancellationTokenSource
    Private _busy As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ---- lifetime -------------------------------------------------------------------

    Private Sub MigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            txtRegistru.Text = _settings.RegistryPath
            txtJurnal.Text = _settings.JournalFolder
            txtGazda.Text = _settings.Host
            txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
            txtUtilizator.Text = _settings.User

            FillTableList()
            Say("Alegeți registrul «cale.accdb» și apăsați «Citește registrul».")
            Say("Anul transferului este fixat la " &
                TableMaps.TransferYear.ToString(CultureInfo.InvariantCulture) & ".")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.Load", ex)
        End Try
    End Sub

    Private Sub MigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If _busy Then
                Dim answer = MessageBox.Show(
                    "O operație este în curs. Închiderea o oprește și derulează tranzacția înapoi." &
                    Environment.NewLine & "Închideți oricum?",
                    "Migrare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If answer <> DialogResult.Yes Then
                    e.Cancel = True
                    Return
                End If
                _cancellation?.Cancel()
            End If
            SaveSettings()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.FormClosing", ex)
        End Try
    End Sub

    Private Sub SaveSettings()
        _settings.RegistryPath = txtRegistru.Text.Trim()
        _settings.JournalFolder = txtJurnal.Text.Trim()
        _settings.Host = txtGazda.Text.Trim()
        _settings.Port = ParsePort()
        _settings.User = txtUtilizator.Text.Trim()
        _settings.Dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        ' No password reaches this call - MigratorSettings has no field for one.
        _settings.Save()
    End Sub

    ' ---- browsing --------------------------------------------------------------------

    Private Sub btnRasfoireRegistru_Click(sender As Object, e As EventArgs) Handles btnRasfoireRegistru.Click
        Try
            Using dialog As New OpenFileDialog()
                dialog.Title = "Alegeți registrul AVACONT"
                dialog.Filter = "Baze Access (*.accdb)|*.accdb|Toate fișierele (*.*)|*.*"
                If File.Exists(txtRegistru.Text) Then dialog.FileName = txtRegistru.Text
                If dialog.ShowDialog(Me) = DialogResult.OK Then txtRegistru.Text = dialog.FileName
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnRasfoireRegistru_Click", ex)
        End Try
    End Sub

    Private Sub btnRasfoireJurnal_Click(sender As Object, e As EventArgs) Handles btnRasfoireJurnal.Click
        Try
            Using dialog As New FolderBrowserDialog()
                dialog.Description = "Alegeți dosarul jurnalului SQL"
                If Directory.Exists(txtJurnal.Text) Then dialog.SelectedPath = txtJurnal.Text
                If dialog.ShowDialog(Me) = DialogResult.OK Then txtJurnal.Text = dialog.SelectedPath
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnRasfoireJurnal_Click", ex)
        End Try
    End Sub

    ' ---- the registry -----------------------------------------------------------------

    Private Sub btnCitesteRegistru_Click(sender As Object, e As EventArgs) Handles btnCitesteRegistru.Click
        Try
            Dim path = txtRegistru.Text.Trim()
            If Not File.Exists(path) Then
                Warn($"Fișierul «{path}» nu există.")
                Return
            End If

            _units = CaiRegistry.Read(path, txtParolaUnitati.Text)
            Say($"Registrul citit: {_units.Count} unități.")

            Dim previous = _settings.Dc
            cboDc.Items.Clear()
            For Each dc In CaiRegistry.DistinctDcs(_units)
                cboDc.Items.Add(dc)
            Next

            If cboDc.Items.Count = 0 Then
                Warn("Registrul nu conține niciun DC.")
                Return
            End If

            Dim index = cboDc.Items.IndexOf(previous)
            cboDc.SelectedIndex = If(index >= 0, index, 0)

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnCitesteRegistru_Click", ex)
            Warn("Registrul nu a putut fi citit." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    Private Sub cboDc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDc.SelectedIndexChanged
        Try
            FillUnitList()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.cboDc_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub FillUnitList()
        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        lblBazaTinta.Text = $"Baza-țintă: {dc}"

        dgvUnitati.BeginUpdate()
        Try
            dgvUnitati.ClearRows()
            For Each unit In CaiRegistry.UnitsOf(_units, dc)
                Dim row = dgvUnitati.AddRow()
                row("bifa") = False
                row("id") = unit.IdUnitate.ToString(CultureInfo.InvariantCulture)
                row("nume") = unit.NumeUnitate
                row("sursa") = unit.Sursa
                row("nomenclator") = If(unit.HasUnitFile, unit.UnitFilePath, "(lipsește) " & unit.UnitFilePath)
                row("forexe") = If(unit.HasForexeFile, unit.ForexeFilePath, "(niciunul)")
            Next
        Finally
            dgvUnitati.EndUpdate()
        End Try

        Say($"DC «{dc}»: {dgvUnitati.RowCount} unități. Bifați-le pe cele de transferat.")
    End Sub

    Private Sub FillTableList()
        dgvTabele.BeginUpdate()
        Try
            dgvTabele.ClearRows()
            For Each map In TableMaps.All()
                Dim row = dgvTabele.AddRow()
                row("bifa") = True
                row("tabel") = map.TargetTable
                row("sursa") = SourceLabel(map.Source)
                row("randuri") = "—"
            Next
        Finally
            dgvTabele.EndUpdate()
        End Try
    End Sub

    Private Shared Function SourceLabel(source As SourceFile) As String
        Select Case source
            Case SourceFile.UnitFile : Return "nomenclatoare"
            Case SourceFile.ForexeFile : Return "FOREXE"
            Case Else : Return "registru"
        End Select
    End Function

    ' ---- the server ---------------------------------------------------------------------

    Private Async Sub btnTesteaza_Click(sender As Object, e As EventArgs) Handles btnTesteaza.Click
        Try
            Dim server = BuildServer()
            If server Is Nothing Then Return

            lblStareServer.Text = "Se încearcă…"
            SetBusy(True)
            Try
                Dim version = Await Task.Run(Function() server.TestConnection())
                lblStareServer.Text = $"Conectat. MariaDB {version}."
                Say($"Conexiune reușită: {server.Describe()} — MariaDB {version}.")
            Finally
                SetBusy(False)
            End Try

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnTesteaza_Click", ex)
            lblStareServer.Text = "Conexiune eșuată."
            Warn("Conexiunea la server a eșuat." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    ' ---- verify -------------------------------------------------------------------------

    Private Async Sub btnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            Dim request = BuildRequest()
            If request Is Nothing Then Return

            btnTransfera.Enabled = False
            dgvConstatari.ClearRows()
            SetBusy(True)
            _cancellation = New CancellationTokenSource()

            Try
                Dim token = _cancellation.Token
                Dim report = Await Task.Run(Function() New Verifier(request, AddressOf SayFromWorker).Run(token), token)
                ShowFindings(report)
                btnTransfera.Enabled = report.CanRun
                If report.CanRun Then
                    Say("«Transferă» este acum activ.")
                Else
                    Say("«Transferă» rămâne inactiv până când nu mai există constatări blocante.")
                End If
            Finally
                SetBusy(False)
                _cancellation?.Dispose()
                _cancellation = Nothing
            End Try

        Catch ex As OperationCanceledException
            Say("Verificare oprită.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnVerifica_Click", ex)
            Warn("Verificarea a eșuat." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    Private Sub ShowFindings(report As VerificationReport)
        dgvConstatari.BeginUpdate()
        Try
            dgvConstatari.ClearRows()
            ' Blocking first: the operator must not have to hunt for what stops the run.
            For Each finding In report.Findings.
                OrderByDescending(Function(f) CInt(f.Severity)).
                ThenBy(Function(f) f.Table, StringComparer.OrdinalIgnoreCase)
                Dim row = dgvConstatari.AddRow()
                row("clasa") = finding.Severity.ToString()
                row("fel") = finding.Kind
                row("tabel") = finding.Table
                row("coloana") = finding.Column
                row("mesaj") = finding.Message
            Next
        Finally
            dgvConstatari.EndUpdate()
        End Try

        Say(report.Summary())
        If report.WriteOrder IsNot Nothing AndAlso report.WriteOrder.Count > 0 Then
            Say("Ordinea de scriere: " & String.Join(" ▸ ", report.WriteOrder))
        End If
    End Sub

    ' ---- transfer -------------------------------------------------------------------------

    Private Async Sub btnTransfera_Click(sender As Object, e As EventArgs) Handles btnTransfera.Click
        Try
            Dim request = BuildRequest()
            If request Is Nothing Then Return

            Dim confirmation = MessageBox.Show(
                $"Se scriu {request.SelectedTables.Count} tabele pentru {request.Units.Count} unități " &
                $"în baza «{request.TargetDatabase}»." & Environment.NewLine & Environment.NewLine &
                "Totul într-o singură tranzacție: orice eșec derulează tot înapoi." & Environment.NewLine &
                "Continuați?",
                "Transferă", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirmation <> DialogResult.Yes Then Return

            SetBusy(True)
            prgTransfer.Style = ProgressBarStyle.Marquee
            _cancellation = New CancellationTokenSource()

            Try
                Dim token = _cancellation.Token
                Dim runner As New TransferRunner(request, AddressOf SayFromWorker, AddressOf ProgressFromWorker)
                Dim result = Await Task.Run(Function() runner.Run(token), token)
                ShowResult(result)
            Finally
                prgTransfer.Style = ProgressBarStyle.Blocks
                SetBusy(False)
                _cancellation?.Dispose()
                _cancellation = Nothing
            End Try

        Catch ex As OperationCanceledException
            Say("Transfer oprit. Tranzacția a fost derulată înapoi.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnTransfera_Click", ex)
            Warn("Transferul a eșuat." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    Private Sub ShowResult(result As TransferResult)
        For Each line In result.Totals()
            Say("   " & line)
        Next

        If result.Committed Then
            Say($"Transfer încheiat cu COMMIT. Jurnalul: {result.JournalFolder}")
            MessageBox.Show(
                $"Transfer încheiat: {result.TotalWritten} rânduri scrise." & Environment.NewLine &
                Environment.NewLine & "Jurnalul rulării: " & result.JournalFolder,
                "Transferă", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ' A committed run cannot be repeated on the insert-only tables, so the button
            ' goes back to needing a fresh verification.
            btnTransfera.Enabled = False
        ElseIf result.Cancelled Then
            Warn("Transferul a fost oprit. Baza a rămas exact cum era." & Environment.NewLine &
                 Environment.NewLine & "Jurnalul rulării: " & result.JournalFolder)
        Else
            Warn("Transferul a eșuat și a fost derulat înapoi." & Environment.NewLine & Environment.NewLine &
                 If(result.Error IsNot Nothing, result.Error.Message, "Motiv necunoscut.") &
                 Environment.NewLine & Environment.NewLine &
                 "Instrucțiunea care a picat este în " & result.JournalFolder & "\_99_final.txt.")
        End If
    End Sub

    Private Sub btnOpreste_Click(sender As Object, e As EventArgs) Handles btnOpreste.Click
        Try
            If _cancellation Is Nothing Then Return
            Say("Se oprește…")
            _cancellation.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnOpreste_Click", ex)
        End Try
    End Sub

    ' ---- building the request --------------------------------------------------------------

    Private Function ParsePort() As Integer
        Dim port As Integer
        If Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, port) AndAlso
           port > 0 AndAlso port <= 65535 Then
            Return port
        End If
        Return 3306
    End Function

    Private Function BuildServer() As TargetServer
        Dim host = txtGazda.Text.Trim()
        If host.Length = 0 Then
            Warn("Gazda serverului MariaDB lipsește.")
            Return Nothing
        End If
        Dim user = txtUtilizator.Text.Trim()
        If user.Length = 0 Then
            Warn("Utilizatorul administrator lipsește.")
            Return Nothing
        End If
        Return New TargetServer(New TargetConnection(host, CUInt(ParsePort()), user, txtParolaServer.Text))
    End Function

    ''' <summary>
    ''' Assembles everything the verifier or the runner needs, or Nothing after telling the
    ''' operator what is missing.
    ''' </summary>
    Private Function BuildRequest() As TransferRequest
        Dim server = BuildServer()
        If server Is Nothing Then Return Nothing

        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        If String.IsNullOrWhiteSpace(dc) Then
            Warn("Nu a fost ales niciun DC. Citiți întâi registrul.")
            Return Nothing
        End If

        Dim journal = txtJurnal.Text.Trim()
        If journal.Length = 0 Then
            Warn("Dosarul jurnalului SQL lipsește. Fără el transferul nu pornește, " &
                 "ca să nu existe o migrare nescrisă nicăieri.")
            Return Nothing
        End If

        Dim request As New TransferRequest(server, dc) With {
            .UnitFilePassword = txtParolaUnitati.Text,
            .ForexeFilePassword = txtParolaForexe.Text,
            .JournalFolder = journal,
            .CommonDatabase = _settings.CommonDatabase,
            .TemplateDatabase = _settings.TemplateDatabase,
            .OperatorName = Environment.UserName,
            .PopulateUnitati = True
        }

        Dim chosen = CaiRegistry.UnitsOf(_units, dc)
        For index = 0 To dgvUnitati.RowCount - 1
            If Not TrueAt(dgvUnitati, "bifa", index) Then Continue For
            If index < chosen.Count Then request.Units.Add(chosen(index))
        Next
        If request.Units.Count = 0 Then
            Warn("Nu a fost bifată nicio unitate.")
            Return Nothing
        End If

        For index = 0 To dgvTabele.RowCount - 1
            If Not TrueAt(dgvTabele, "bifa", index) Then Continue For
            request.SelectedTables.Add(Convert.ToString(dgvTabele("tabel", index), CultureInfo.InvariantCulture))
        Next
        If request.SelectedTables.Count = 0 Then
            Warn("Nu a fost bifat niciun tabel. Lista goală nu înseamnă «toate».")
            Return Nothing
        End If

        SaveSettings()
        Return request
    End Function

    Private Shared Function TrueAt(grid As KBot.Controls.KBotDataView, key As String, index As Integer) As Boolean
        Dim value = grid(key, index)
        If value Is Nothing Then Return False
        Return TypeOf value Is Boolean AndAlso CBool(value)
    End Function

    ' ---- log and busy state ------------------------------------------------------------------

    Private Sub SetBusy(busy As Boolean)
        _busy = busy
        btnVerifica.Enabled = Not busy
        btnTesteaza.Enabled = Not busy
        btnCitesteRegistru.Enabled = Not busy
        btnOpreste.Enabled = busy
        If busy Then btnTransfera.Enabled = False
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    ''' <summary>Log line from a worker thread. Marshals onto the UI thread.</summary>
    Private Sub SayFromWorker(message As String)
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(Of String)(AddressOf Say), message)
                Return
            End If
            Say(message)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.SayFromWorker", ex)
        End Try
    End Sub

    Private Sub ProgressFromWorker(table As String, written As Long, read As Long)
        SayFromWorker($"   {table}: {written} scrise din {read} citite…")
    End Sub

    Private Sub Say(message As String)
        Try
            rtbJurnal.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}")
            rtbJurnal.SelectionStart = rtbJurnal.TextLength
            rtbJurnal.ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.Say", ex)
        End Try
    End Sub

    Private Sub Warn(message As String)
        Say(message.Replace(Environment.NewLine, " "))
        MessageBox.Show(message, "Migrare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

End Class
