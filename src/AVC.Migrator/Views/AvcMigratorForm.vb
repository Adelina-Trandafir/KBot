Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' The migration console of Avacont, built like KBot.Migrator's: read the registry <c>cale.accdb</c>, pick the DC, tick the
''' units, read their Access files, verify against the server, migrate. The steps (chart of accounts, journals, templates, ...)
''' come from <see cref="AvcCatalog"/> and run in that order inside one transaction.
''' Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AvcMigratorForm

    Private Const AppTitle As String = "Migrare Avacont"

    Private ReadOnly _settings As AvcSettings = AvcSettings.Load()
    Private _units As New List(Of CaiUnit)()
    ''' <summary>IdUnitate ▸ IdUnitate of the parent, from <c>cai.IdUnitateParinte</c> (only the units that have one).</summary>
    Private _parents As New Dictionary(Of Integer, Integer)()
    Private _steps As List(Of AvcStep) = AvcCatalog.Create()
    ''' <summary>The units as they were read; Nothing = nothing read yet, or the ticks / the DC changed since.</summary>
    Private _context As AvcContext
    Private _readFindings As List(Of Finding) = New List(Of Finding)()
    ''' <summary>The last verification; Nothing = none yet, or an input changed since.</summary>
    Private _report As VerificationReport
    Private _busy As Boolean
    Private _cancellation As CancellationTokenSource
    Private _operationFinished As TaskCompletionSource(Of Boolean)
    Private _initializing As Boolean = True
    ''' <summary>True while «Citește registrul» refills the DC list: the selection handler waits for the final pick.</summary>
    Private _fillingDcs As Boolean
    ''' <summary>The server reached with the default host / port / user and the Admin password asked at «Citește registrul»; Nothing = not asked yet.</summary>
    Private _server As TargetServer
    ''' <summary>The subunits the DC database holds, as read when the DC was picked (and after a run).</summary>
    Private _existing As New List(Of AvcSubunit)()
    ' What the HTML report shows: the findings of the last reading / verification (kept after a migration, which clears _report) and the
    ' outcome of the last migration. Nothing = nothing read yet; all cleared when the units or the DC change.
    Private _reportFindings As List(Of Finding)
    Private _reportFindingsAt As DateTime
    Private _reportVerified As Boolean
    Private _reportCanRun As Boolean
    Private _wasRun As Boolean
    Private _runOutcome As String = String.Empty
    Private _runClass As String = "success"
    Private _runAt As DateTime
    Private _runWritten As New List(Of AvcWritten)()
    ''' <summary>False when the subunits of the DC could not be read (database or table missing): the «Stare» column then says so.</summary>
    Private _subunitsKnown As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ---- lifetime ------------------------------------------------------------------------------------------------

    Private Sub AvcMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            txtRegistru.Text = _settings.RegistryPath
            FillSteps()
            RefreshMigrateButton()
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.Load", "Inițializarea ferestrei a eșuat.", ex)
        Finally
            _initializing = False
        End Try
    End Sub

    ''' <summary>Keep the process alive until the worker has released its connections.</summary>
    Private Async Sub AvcMigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If Not _busy Then Return
            e.Cancel = True
            If KBotMessage.Show("O operație este în curs. Închideți după terminarea ei?",
                                AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            _cancellation?.Cancel()
            Dim k_pending = _operationFinished
            If k_pending IsNot Nothing Then Await k_pending.Task
            Close()
        Catch ex As Exception
            e.Cancel = True
            ShowOperationError("AvcMigratorForm.FormClosing", "Închiderea a eșuat.", ex)
        End Try
    End Sub

    ' ---- busy, messages, log -------------------------------------------------------------------------------------

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        grpUnitate.Enabled = Not k_busy
        dgvPasi.Enabled = Not k_busy
        btnCiteste.Enabled = Not k_busy
        btnVerifica.Enabled = Not k_busy
        btnStop.Enabled = k_busy AndAlso _cancellation IsNot Nothing
        If k_busy Then
            _operationFinished = New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            btnMigreaza.Enabled = False
        Else
            _cancellation?.Dispose()
            _cancellation = Nothing
            _operationFinished?.TrySetResult(True)
            RefreshMigrateButton()
        End If
        RefreshReportButton()
        UseWaitCursor = k_busy
    End Sub

    ''' <summary>UI errors stay visible; the shared log preserves their technical details.</summary>
    Private Sub ShowOperationError(k_context As String, k_message As String, k_error As Exception)
        GlobalErrorLog.Write(k_context, k_error)
        AppendLog("EROARE", k_message & Environment.NewLine & k_error.ToString())
        Say(k_message)
        KBotMessage.Show(k_message & Environment.NewLine & Environment.NewLine & k_error.Message,
                         AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub Say(k_text As String)
        lblStare.Text = k_text
        AppendLog("INFO", k_text)
    End Sub

    Private Sub AppendLog(k_level As String, k_message As String)
        If rtbLog Is Nothing OrElse rtbLog.IsDisposed Then Return
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {k_level}: {k_message}" & Environment.NewLine)
        rtbLog.SelectionStart = rtbLog.TextLength
        rtbLog.ScrollToCaret()
    End Sub

    Private Sub SavePreferences()
        Try
            _settings.Save()
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.SavePreferences", "Setările locale nu au putut fi salvate. Lucrul poate continua.", ex)
        End Try
    End Sub

    ' ---- the registry --------------------------------------------------------------------------------------------

    Private Sub BtnRasfoireRegistru_Click(sender As Object, e As EventArgs) Handles btnRasfoireRegistru.Click
        Try
            Using k_dialog As New OpenFileDialog With {
                .Title = "Alegeți registrul AVACONT (cale.accdb)",
                .Filter = "Baze Access (*.accdb;*.mdb)|*.accdb;*.mdb|Toate fișierele (*.*)|*.*",
                .CheckFileExists = True}
                If File.Exists(txtRegistru.Text) Then k_dialog.FileName = txtRegistru.Text
                If k_dialog.ShowDialog(Me) = DialogResult.OK Then txtRegistru.Text = k_dialog.FileName
            End Using
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.BrowseRegistry", "Alegerea fișierului a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Asks for the Admin password, connects with it and the default host / port / user, and only then reads the registry: the grid
    ''' shows what the DC database already holds as subunits, so the server must be reachable before the units are listed.
    ''' </summary>
    Private Async Sub BtnCitesteRegistru_Click(sender As Object, e As EventArgs) Handles BtnCitesteRegistru.Click
        If _busy Then Return
        Try
            Dim k_path = txtRegistru.Text.Trim
            If Not File.Exists(k_path) Then
                KBotMessage.Show($"Fișierul «{k_path}» nu există.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim k_password As String
            Using k_ask As New AvcAdminPasswordForm(_settings.User, _settings.Host)
                If k_ask.ShowDialog(Me) <> DialogResult.OK Then Return
                k_password = k_ask.Password
            End Using
            Dim k_server As New TargetServer(New TargetConnection(_settings.Host, CUInt(_settings.Port), _settings.User, k_password))

            SetBusy(True)
            Try
                Say($"Mă conectez la {k_server.Describe()}...")
                Try
                    Dim k_version = Await Task.Run(Function() k_server.TestConnection())
                    Say($"Conectat la {k_server.Describe()} (MariaDB {k_version}).")
                Catch ex As Exception
                    ShowOperationError("AvcMigratorForm.Connect", "Conectarea la server a eșuat. Verificați parola de Admin.", ex)
                    Return
                End Try
                _server = k_server
                ResetRead()

                _units = CaiRegistry.Read(k_path, String.Empty)
                _parents = AvcCaiParents.Read(k_path, String.Empty)
                _settings.RegistryPath = k_path
                SavePreferences()
                Say($"Registrul citit: {_units.Count} unități.")

                Dim k_previous = _settings.Dc
                _fillingDcs = True
                Try
                    cboDc.BeginUpdate()
                    cboDc.Items.Clear()
                    For Each k_dc In CaiRegistry.DistinctDcs(_units)
                        cboDc.Items.Add(k_dc)
                    Next
                    cboDc.EndUpdate()
                    If cboDc.Items.Count = 0 Then
                        KBotMessage.Show("Registrul nu conține niciun DC.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                    Dim k_index = cboDc.Items.IndexOf(k_previous)
                    cboDc.SelectedIndex = If(k_index >= 0, k_index, 0)
                Finally
                    _fillingDcs = False
                End Try
                ' The same DC chosen again does not raise SelectedIndexChanged, but the units were re-read.
                ApplyDc()
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.ReadRegistry", "Registrul nu a putut fi citit.", ex)
        End Try
    End Sub

    Private Sub CboDc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDc.SelectedIndexChanged
        Try
            If _initializing OrElse _fillingDcs Then Return
            ApplyDc()
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.DcChanged", "Schimbarea DC-ului a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>The DC picked (or re-read): its units, its CodFiscal, and the choice remembered for the next launch.</summary>
    Private Sub ApplyDc()
        FillUnitList()
        ShowCodFiscal()
        Dim k_dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        If k_dc.Length > 0 Then
            _settings.Dc = k_dc
            SavePreferences()
        End If
    End Sub

    ''' <summary>Proposes the CodFiscal of the DC as the old application keeps it in the Windows registry; the operator may correct it.</summary>
    Private Sub ShowCodFiscal()
        Dim k_dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        txtCodFiscal.Text = CodFiscalRegistry.ForDc(k_dc)
    End Sub

    ''' <summary>
    ''' Fills the unit grid, binding each <see cref="CaiUnit"/> to its row's Tag (the ticks are read back from the Tag, never by
    ''' row index, so a re-ordered grid cannot migrate the wrong unit).
    ''' </summary>
    Private Sub FillUnitList()
        Dim k_dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        ResetRead()
        LoadSubunitState(k_dc)

        dgvUnitati.BeginUpdate()
        Try
            dgvUnitati.ClearRows()
            For Each k_unit In CaiRegistry.UnitsOf(_units, k_dc)
                Dim k_row = dgvUnitati.AddRow()
                k_row.Tag = k_unit
                k_row("bifa") = False
                k_row("id") = k_unit.IdUnitate.ToString(CultureInfo.InvariantCulture)
                k_row("nume") = k_unit.NumeUnitate
                k_row("sursa") = k_unit.Sursa
                k_row("parinte") = If(_parents.ContainsKey(k_unit.IdUnitate), _parents(k_unit.IdUnitate).ToString(CultureInfo.InvariantCulture), String.Empty)
                k_row("anDate") = k_unit.AnDate

                Dim k_done = PortedSubunit(k_unit)
                If k_done IsNot Nothing Then
                    ShowPorted(k_row, k_done)
                Else
                    ' Every unit of a source gets the chapter of that source; a repeat is the operator's to keep or correct.
                    Dim k_capitol = AvcSubunitStore.SuggestedCapitol(k_unit.Sursa)
                    k_row("denumire") = If(String.IsNullOrWhiteSpace(k_unit.AlteDetalii), k_unit.NumeUnitate, k_unit.AlteDetalii.Trim())
                    k_row("capitol") = k_capitol
                    k_row("zec") = "0"
                    k_row("stare") = If(Not _subunitsKnown, "Stare necunoscută", If(k_capitol.Length = 0, "De portat (completați capitolul)", "De portat"))
                End If
            Next
        Finally
            dgvUnitati.EndUpdate()
        End Try
        If k_dc.Length > 0 Then Say($"DC «{k_dc}»: {dgvUnitati.RowCount} unități. Bifați-le pe cele de transferat.")
    End Sub

    ''' <summary>The subunit the server holds for this registry unit, or Nothing when it is not ported (or the server was not read).</summary>
    Private Function PortedSubunit(k_unit As CaiUnit) As AvcSubunit
        Return _existing.FirstOrDefault(Function(k_s) k_s.IdUnitateAccess = k_unit.IdUnitate)
    End Function

    ''' <summary>A ported unit shows the subunit as it is on the server; its denomination, chapter and decimals are no longer editable.</summary>
    Private Shared Sub ShowPorted(k_row As KBot.Controls.KBotDataRow, k_done As AvcSubunit)
        k_row("denumire") = k_done.Denumire
        k_row("capitol") = k_done.Capitol
        k_row("zec") = k_done.Zecimale.ToString(CultureInfo.InvariantCulture)
        k_row("stare") = $"Portată (id {k_done.IdSubunitate.ToString(CultureInfo.InvariantCulture)})"
    End Sub

    ''' <summary>
    ''' Reads the sectors of the common database and the subunits the DC database holds. A database or table that cannot be read leaves
    ''' the state unknown and is written to the log, not raised: the units are still listed and the operator is told in «Stare».
    ''' </summary>
    Private Sub LoadSubunitState(k_dc As String)
        _existing = New List(Of AvcSubunit)()
        _subunitsKnown = False
        If _server Is Nothing OrElse k_dc.Length = 0 Then Return
        Try
            Using k_cn = _server.Open(k_dc)
                If Not AvcSql.TableExists(k_cn, k_dc, "TZ_Subunitati") Then
                    AppendLog("ATENȚIE", $"Tabelul TZ_Subunitati lipsește din «{k_dc}»: rulați scriptele sql/AVACONT/Configurari/PlanCont/AV01_04*.sql, apoi «Sincronizare schemă».")
                    Return
                End If
                Dim k_missing = AvcSql.MissingColumns(k_cn, k_dc, "TZ_Subunitati", AvcSubunitStore.LinkColumns)
                If k_missing.Count > 0 Then
                    AppendLog("ATENȚIE", $"«TZ_Subunitati» din «{k_dc}» nu are coloanele: {String.Join(", ", k_missing)} (script AV01_04d).")
                    Return
                End If
                _existing = AvcSubunitStore.List(k_cn)
                _subunitsKnown = True
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AvcMigratorForm.LoadSubunitState", ex)
            AppendLog("ATENȚIE", $"Subunitățile din «{k_dc}» nu au putut fi citite: {ex.Message}")
        End Try
    End Sub

    ''' <summary>A ported unit's denomination, chapter and decimals show normally but cannot be edited.</summary>
    Private Sub DgvUnitati_CellFormatting(sender As Object, e As KBot.Controls.KBotCellFormattingEventArgs) Handles dgvUnitati.CellFormatting
        Try
            If e.Row Is Nothing Then Return
            If e.ColumnKey <> "denumire" AndAlso e.ColumnKey <> "capitol" AndAlso e.ColumnKey <> "zec" Then Return
            Dim k_unit = TryCast(e.Row.Tag, CaiUnit)
            If k_unit IsNot Nothing AndAlso PortedSubunit(k_unit) IsNot Nothing Then e.ReadOnlyCell = True
        Catch ex As Exception
            GlobalErrorLog.Write("AvcMigratorForm.UnitCellFormatting", ex)
        End Try
    End Sub

    ''' <summary>The salary decimals are 0 or 2: anything else is refused where it is typed.</summary>
    Private Sub DgvUnitati_CellValidating(sender As Object, e As KBot.Controls.KBotCellValidatingEventArgs) Handles dgvUnitati.CellValidating
        Try
            If e.ColumnKey <> "zec" Then Return
            Dim k_text = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
            If k_text IsNot Nothing Then k_text = k_text.Trim()
            If k_text = "0" OrElse k_text = "2" Then
                e.ProposedValue = k_text
            Else
                e.Cancel = True
                lblStare.Text = "Zecimalele salariilor pot fi 0 sau 2."
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvcMigratorForm.CellValidating", ex)
        End Try
    End Sub

    Private Function TickedUnits() As List(Of CaiUnit)
        Dim k_units As New List(Of CaiUnit)()
        For Each k_row In dgvUnitati.Rows
            Dim k_unit = TryCast(k_row.Tag, CaiUnit)
            If k_unit IsNot Nothing AndAlso Convert.ToBoolean(k_row("bifa"), CultureInfo.InvariantCulture) Then k_units.Add(k_unit)
        Next
        Return k_units
    End Function

    Private Sub DgvUnitati_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvUnitati.CellValueChanged
        Try
            If _busy OrElse _initializing Then Return
            If e.ColumnKey = "denumire" OrElse e.ColumnKey = "capitol" OrElse e.ColumnKey = "zec" Then
                ' The subunit the step will create changed: what was verified no longer applies, what was read still does.
                _report = Nothing
                RefreshMigrateButton()
                Return
            End If
            If e.ColumnKey <> "bifa" Then Return
            ResetRead()
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.UnitTicked", "Schimbarea bifei a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>The units, the DC or the file changed: what was read and verified no longer applies.</summary>
    Private Sub ResetRead()
        _context = Nothing
        _report = Nothing
        _readFindings = New List(Of Finding)()
        _reportFindings = Nothing
        _reportVerified = False
        _reportCanRun = False
        _wasRun = False
        _runWritten = New List(Of AvcWritten)()
        _steps = AvcCatalog.Create()
        FillSteps()
        rtbConstatari.Clear()
        RefreshMigrateButton()
        RefreshReportButton()
    End Sub

    ''' <summary>Adds the server reached at «Citește registrul» and the CodFiscal to the context built at «Citește».</summary>
    Private Sub FillContextFromServer()
        If _server Is Nothing Then Throw New InvalidOperationException("Citiți întâi registrul: se cere parola de Admin a serverului.")
        _context.Server = _server
        _context.CommonDatabase = _settings.CommonDatabase
        _context.DcUnits = CaiRegistry.UnitsOf(_units, _context.Dc)
        _context.SubunitDrafts = BuildSubunitDrafts()
        _context.CodFiscal = txtCodFiscal.Text.Trim()
        _context.JournalRoot = Path.Combine(AppContext.BaseDirectory, "Jurnale", "AVC")
    End Sub

    ' ---- reading the unit files ----------------------------------------------------------------------------------

    Private Async Sub BtnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
        If _busy Then Return
        Try
            Dim k_dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
            If k_dc.Length = 0 Then
                KBotMessage.Show("Citiți registrul și alegeți un DC.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_ticked = TickedUnits()
            If k_ticked.Count = 0 Then
                KBotMessage.Show("Bifați cel puțin o unitate de transferat.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            SetBusy(True)
            Say("Citesc fișierele Access ale unităților bifate...")
            Try
                _report = Nothing
                _steps = AvcCatalog.Create()
                _context = New AvcContext(k_dc, k_ticked)
                Dim k_contextNow = _context
                Dim k_stepsNow = _steps
                _readFindings = Await Task.Run(Function() AvcRunner.ReadAll(k_contextNow, k_stepsNow))
                FillSteps()
                ShowFindings()
                For Each k_finding In _readFindings
                    AppendLog("EROARE CITIRE", k_finding.Message)
                Next
                Say(If(_readFindings.Count = 0, "Unitățile au fost citite. Completați serverul și apăsați «Verifică».",
                       "Citirea nu a reușit complet — vezi constatările."))
            Catch ex As Exception
                _context = Nothing
                ShowOperationError("AvcMigratorForm.ReadSource", "Fișierele nu au putut fi citite.", ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.Read", "Citirea unităților a eșuat.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---- the grid and the findings -------------------------------------------------------------------------------

    Private Sub FillSteps()
        dgvPasi.BeginUpdate()
        Try
            dgvPasi.ClearRows()
            For Each k_step In _steps
                Dim k_row = dgvPasi.AddRow()
                k_row.Tag = k_step
                k_row("bifat") = k_step.Selected
                k_row("etapa") = k_step.Stage
                k_row("pas") = k_step.Title
                k_row("tabel") = k_step.AccessTable
                k_row("randuriAccess") = If(k_step.AccessRows >= 0, k_step.AccessRows.ToString(CultureInfo.InvariantCulture), "—")
                k_row("deScris") = If(k_step.RowsToWrite >= 0, k_step.RowsToWrite.ToString(CultureInfo.InvariantCulture), "—")
                k_row("inBaza") = ExistingText(k_step)
            Next
        Finally
            dgvPasi.EndUpdate()
        End Try
    End Sub

    Private Shared Function ExistingText(k_step As AvcStep) As String
        If k_step.ExistingRows.Count = 0 Then Return "—"
        Dim k_first As Long
        If Not k_step.ExistingRows.TryGetValue(k_step.TargetTables(0), k_first) Then Return "—"
        Return If(k_first < 0, "lipsă", k_first.ToString(CultureInfo.InvariantCulture))
    End Function

    Private Sub DgvPasi_CellValueChanged(sender As Object, e As KBot.Controls.KBotCellValueEventArgs) Handles dgvPasi.CellValueChanged
        Try
            If _busy OrElse _initializing Then Return
            If e.ColumnKey <> "bifat" Then Return
            For Each k_row In dgvPasi.Rows
                Dim k_step = TryCast(k_row.Tag, AvcStep)
                If k_step IsNot Nothing Then k_step.Selected = Convert.ToBoolean(k_row("bifat"), CultureInfo.InvariantCulture)
            Next
            _report = Nothing
            RefreshMigrateButton()
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.StepTicked", "Schimbarea bifei a eșuat.", ex)
        End Try
    End Sub

    ''' <summary>The read errors and the verification, worst first, with the detail under the message.</summary>
    Private Sub ShowFindings()
        rtbConstatari.Clear()
        Dim k_all As New List(Of Finding)(_readFindings)
        If _report IsNot Nothing Then k_all.AddRange(_report.Findings)
        _reportFindings = k_all
        _reportFindingsAt = DateTime.Now
        _reportVerified = _report IsNot Nothing
        _reportCanRun = _report IsNot Nothing AndAlso _report.CanRun
        RefreshReportButton()
        If k_all.Count = 0 Then Return

        Dim k_bold As New Font(rtbConstatari.Font, FontStyle.Bold)
        Try
            For Each k_group In {FindingClass.Blocant, FindingClass.Atentie, FindingClass.Informativ}
                Dim k_inGroup = k_all.Where(Function(f) f.Severity = k_group).ToList()
                If k_inGroup.Count = 0 Then Continue For
                AppendLine(GroupTitle(k_group), k_bold)
                For Each k_finding In k_inGroup
                    AppendLine("  • " & k_finding.Message, Nothing)
                    If k_finding.Detail.Length > 0 Then
                        For Each k_line In k_finding.Detail.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                            AppendLine("      " & k_line, Nothing)
                        Next
                    End If
                Next
                AppendLine(String.Empty, Nothing)
            Next
        Finally
            k_bold.Dispose()
        End Try
        rtbConstatari.SelectionStart = 0
        rtbConstatari.ScrollToCaret()
    End Sub

    Private Shared Function GroupTitle(k_group As FindingClass) As String
        Select Case k_group
            Case FindingClass.Blocant : Return "Blocaje (opresc migrarea):"
            Case FindingClass.Atentie : Return "Atenționări (de citit înainte):"
            Case Else : Return "Informații:"
        End Select
    End Function

    Private Sub AppendLine(k_text As String, k_font As Font)
        If k_font IsNot Nothing Then rtbConstatari.SelectionFont = k_font
        rtbConstatari.AppendText(k_text & Environment.NewLine)
        If k_font IsNot Nothing Then rtbConstatari.SelectionFont = rtbConstatari.Font
    End Sub

    ' ---- subunits ------------------------------------------------------------------------------------------------

    ''' <summary>
    ''' One subunit draft per ticked unit, with the denomination, chapter and salary decimals as the grid shows them. The subunits step
    ''' of the run writes the ones the server does not hold yet, together with the rest of the migration (the chapter is in no Access table).
    ''' </summary>
    Private Function BuildSubunitDrafts() As List(Of AvcSubunit)
        dgvUnitati.CommitPendingEdit()
        Dim k_drafts As New List(Of AvcSubunit)()
        For Each k_row In dgvUnitati.Rows
            Dim k_unit = TryCast(k_row.Tag, CaiUnit)
            If k_unit Is Nothing OrElse Not Convert.ToBoolean(k_row("bifa"), CultureInfo.InvariantCulture) Then Continue For
            Dim k_zec As Integer
            If Not Integer.TryParse(Convert.ToString(k_row("zec"), CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_zec) Then k_zec = -1
            Dim k_parent As Integer = 0
            _parents.TryGetValue(k_unit.IdUnitate, k_parent)
            k_drafts.Add(New AvcSubunit() With {
                .Denumire = Convert.ToString(k_row("denumire"), CultureInfo.InvariantCulture).Trim(),
                .Capitol = Convert.ToString(k_row("capitol"), CultureInfo.InvariantCulture).Trim(),
                .SursaSector = k_unit.Sursa.Trim().ToUpperInvariant(),
                .Zecimale = k_zec,
                .IdUnitateAccess = k_unit.IdUnitate,
                .ParinteUnitateAccess = k_parent})
        Next
        Return k_drafts
    End Function

    ''' <summary>Re-reads the subunits and shows them on the lines that are now ported, without rebuilding the grid (the ticks stay).</summary>
    Private Sub RefreshPortedRows(k_dc As String)
        LoadSubunitState(k_dc)
        dgvUnitati.BeginUpdate()
        Try
            For Each k_row In dgvUnitati.Rows
                Dim k_unit = TryCast(k_row.Tag, CaiUnit)
                If k_unit Is Nothing Then Continue For
                Dim k_done = PortedSubunit(k_unit)
                If k_done IsNot Nothing Then ShowPorted(k_row, k_done)
            Next
        Finally
            dgvUnitati.EndUpdate()
        End Try
    End Sub

    ' ---- verify --------------------------------------------------------------------------------------------------

    Private Async Sub BtnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        If _busy Then Return
        Try
            If _context Is Nothing Then
                KBotMessage.Show("Apăsați întâi «Citește unitățile bifate», ca să se încarce fișierele Access.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            FillContextFromServer()

            SetBusy(True)
            Say("Verific serverul și datele citite...")
            Try
                _report = Nothing
                Dim k_contextNow = _context
                Dim k_stepsNow = _steps
                _report = Await Task.Run(Function() AvcRunner.Verify(k_contextNow, k_stepsNow))
                FillSteps()
                ShowFindings()
                Say(_report.Summary())
            Catch ex As Exception
                _report = Nothing
                ShowOperationError("AvcMigratorForm.Verify", "Verificarea nu a putut fi făcută. Citiți din nou registrul (cu parola de Admin) și verificați numele bazelor.", ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.VerifyClick", "Verificarea a eșuat.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    ''' <summary>«Migrează» is enabled only after a verification without blocks, on the units that were read, while nothing runs.</summary>
    Private Sub RefreshMigrateButton()
        btnMigreaza.Enabled = Not _busy AndAlso _context IsNot Nothing AndAlso _report IsNot Nothing AndAlso _report.CanRun
    End Sub

    ' ---- the HTML report -----------------------------------------------------------------------------------------

    ''' <summary>«Raport HTML» is available once something was read (it shows the verification) or a migration ran, and not while one runs.</summary>
    Private Sub RefreshReportButton()
        BtnRaportHTML.Enabled = Not _busy AndAlso (_reportFindings IsNot Nothing OrElse _wasRun)
    End Sub

    ''' <summary>Everything the report shows, from what the form already holds: no Access, no server.</summary>
    Private Function BuildReportData() As AvcReportData
        Dim k_data As New AvcReportData() With {
            .Dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture),
            .Server = If(_server Is Nothing, String.Empty, _server.Describe()),
            .CodFiscal = txtCodFiscal.Text.Trim(),
            .WasRead = _reportFindings IsNot Nothing,
            .WasVerified = _reportVerified,
            .CanRun = _reportCanRun,
            .WasRun = _wasRun,
            .Outcome = _runOutcome,
            .OutcomeClass = _runClass,
            .JournalFolder = If(_wasRun AndAlso _context IsNot Nothing, _context.JournalRoot, String.Empty)}
        If _reportFindings IsNot Nothing Then
            k_data.Findings.AddRange(_reportFindings)
            k_data.FindingsAt = _reportFindingsAt
        End If
        If _wasRun Then
            k_data.RunAt = _runAt
            k_data.Written.AddRange(_runWritten)
        End If
        For Each k_row In dgvUnitati.Rows
            k_data.Units.Add(New AvcReportUnit() With {
                .Ticked = Convert.ToBoolean(k_row("bifa"), CultureInfo.InvariantCulture),
                .Id = Convert.ToString(k_row("id"), CultureInfo.InvariantCulture),
                .Name = Convert.ToString(k_row("nume"), CultureInfo.InvariantCulture),
                .Denomination = Convert.ToString(k_row("denumire"), CultureInfo.InvariantCulture),
                .Chapter = Convert.ToString(k_row("capitol"), CultureInfo.InvariantCulture),
                .Source = Convert.ToString(k_row("sursa"), CultureInfo.InvariantCulture),
                .Decimals = Convert.ToString(k_row("zec"), CultureInfo.InvariantCulture),
                .Parent = Convert.ToString(k_row("parinte"), CultureInfo.InvariantCulture),
                .AnDate = Convert.ToString(k_row("anDate"), CultureInfo.InvariantCulture),
                .State = Convert.ToString(k_row("stare"), CultureInfo.InvariantCulture)})
        Next
        For Each k_step In _steps
            k_data.Steps.Add(New AvcReportStep() With {
                .Selected = k_step.Selected,
                .Stage = k_step.Stage,
                .Title = k_step.Title,
                .AccessTable = k_step.AccessTable,
                .AccessRows = If(k_step.AccessRows >= 0, k_step.AccessRows.ToString(CultureInfo.InvariantCulture), "—"),
                .ToWrite = If(k_step.RowsToWrite >= 0, k_step.RowsToWrite.ToString(CultureInfo.InvariantCulture), "—"),
                .InDatabase = ExistingText(k_step)})
        Next
        Return k_data
    End Function

    Private Sub BtnRaportHtml_Click(sender As Object, e As EventArgs) Handles BtnRaportHTML.Click
        Try
            If _busy OrElse Not (_reportFindings IsNot Nothing OrElse _wasRun) Then Return
            Dim k_data = BuildReportData()
            Using k_form As New AvcReportForm(AvcReportHtml.Create(k_data), k_data.Dc)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.ReportHtml", "Raportul HTML nu a putut fi deschis.", ex)
        End Try
    End Sub

    ' ---- migrate -------------------------------------------------------------------------------------------------

    Private Sub BtnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        Try
            _cancellation?.Cancel()
            btnStop.Enabled = False
            Say("Oprire solicitată. Se așteaptă terminarea instrucțiunii curente și rollback-ul.")
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.Stop", "Oprirea nu a putut fi solicitată.", ex)
        End Try
    End Sub

    ''' <summary>Keeps how the last migration ended, for the HTML report (it survives the verification being cleared).</summary>
    Private Sub RecordRun(k_outcome As String, k_class As String, k_written As IEnumerable(Of AvcWritten))
        _wasRun = True
        _runOutcome = k_outcome
        _runClass = k_class
        _runAt = DateTime.Now
        _runWritten = If(k_written Is Nothing, New List(Of AvcWritten)(), New List(Of AvcWritten)(k_written))
        RefreshReportButton()
    End Sub

    Private Async Sub BtnMigreaza_Click(sender As Object, e As EventArgs) Handles btnMigreaza.Click
        If _busy Then Return
        Dim k_committed = False
        Try
            If _context Is Nothing OrElse _report Is Nothing OrElse Not _report.CanRun Then Return
            FillContextFromServer()
            Dim k_selected = _steps.Where(Function(s) s.Selected).ToList()
            Dim k_total = k_selected.Sum(Function(s) Math.Max(s.RowsToWrite, 0))
            Dim k_answer = KBotMessage.Show(
                $"Se scriu {k_total} rânduri (pașii: {String.Join(", ", k_selected.Select(Function(s) s.Title))}) în baza «{_context.TargetDatabase}» de pe {_settings.Host}."& Environment.NewLine &
                $"Unități: {String.Join(", ", _context.Units.Select(Function(u) u.NumeUnitate))}. An: {_context.Year}." & Environment.NewLine &
                "Scrierea folosește o singură tranzacție. Dacă rezultatul nu poate fi confirmat, verificați serverul înainte de reluare." & Environment.NewLine & Environment.NewLine &
                "Continuați?", AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If k_answer <> DialogResult.Yes Then Return

            _cancellation = New CancellationTokenSource()
            Dim k_cancel = _cancellation.Token
            SetBusy(True)
            Try
                Dim k_contextNow = _context
                Dim k_stepsNow = _steps
                Dim k_run = _operationFinished
                Dim k_progress As IProgress(Of String) = New Progress(Of String)(
                    Sub(k_message)
                        Try
                            If _busy AndAlso _operationFinished Is k_run AndAlso Not k_cancel.IsCancellationRequested Then Say(k_message)
                        Catch ex As Exception
                            ShowOperationError("AvcMigratorForm.Progress", "Afișarea progresului a eșuat.", ex)
                        End Try
                    End Sub)
                Dim k_written = Await Task.Run(Function() AvcRunner.Run(k_contextNow, k_stepsNow, k_progress, k_cancel))
                If k_written Is Nothing Then
                    _report = Nothing
                    RecordRun("Migrare oprită înainte de COMMIT: nu s-a scris nimic (rollback confirmat).", "warning", Nothing)
                    Say("Migrare oprită. Verificați din nou înainte de reluare.")
                    KBotMessage.Show("Migrarea a fost oprită; nu s-a scris nimic. Verificați din nou înainte de reluare.",
                                     AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
                k_committed = True
                ' Committed counts come from the writer. A later failure to show them must not turn a committed run into a failure.
                _report = Nothing
                RecordRun("Migrare terminată: modificările sunt confirmate (COMMIT).", "success", k_written)
                Dim k_lines = k_written.Select(Function(w) $"{w.Table}: {w.Rows} rânduri")
                Say("Migrare terminată: " & String.Join("; ", k_lines))
                RefreshPortedRows(k_contextNow.Dc)
                KBotMessage.Show("Migrare terminată:" & Environment.NewLine & String.Join(Environment.NewLine, k_lines) & Environment.NewLine & Environment.NewLine &
                                 "Jurnalele SQL sunt în: " & _context.JournalRoot & Environment.NewLine &
                                 "Apăsați «Verifică» ca să vedeți starea bazei.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                _report = Nothing
                Dim k_message = If(k_committed, "Migrarea este confirmată, dar afișarea rezultatului a eșuat. Verificați jurnalul.",
                                   "Migrarea nu a fost confirmată. Verificați serverul și jurnalul înainte de reluare.")
                RecordRun(k_message & " " & ex.Message, If(k_committed, "warning", "blocking"), Nothing)
                ShowOperationError("AvcMigratorForm.Write", k_message, ex)
            End Try
        Catch ex As Exception
            ShowOperationError("AvcMigratorForm.Migrate", "Operația de migrare nu a putut fi finalizată.", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub
End Class
