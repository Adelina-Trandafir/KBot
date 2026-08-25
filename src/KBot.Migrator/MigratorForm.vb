Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports KBot.Common
Imports System.Data.OleDb
Imports System.Runtime.InteropServices
''' <summary>
''' The migration console: pick the registry, the server, the units and the tables, verify,
''' then transfer.
''' </summary>
''' <remarks>
''' <para>
''' Every control is declared in the designer file, which the operator owns. This class only
''' wires behaviour and must follow the designer, never the other way round.
''' </para>
''' <para>
''' The long operations run OFF the UI thread and are cancellable; cancelling a transfer
''' rolls the transaction back, so the database is left exactly as it was.
''' «Transferă» stays disabled until a verification passes with no blocking finding.
''' </para>
''' <para>
''' Passwords are read from the boxes at the moment a request is built and are never stored,
''' logged or written to disk. There is ONE Access password box - «Parolă fișiere» - and it
''' covers both the unit files and the Forexe files, which is the shape of this estate.
''' </para>
''' </remarks>
Public Class MigratorForm
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function TerminateProcess(hProcess As IntPtr, uExitCode As UInteger) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function GetCurrentProcess() As IntPtr
    End Function

    Private ReadOnly _settings As MigratorSettings = MigratorSettings.Load()
    Private _units As New List(Of CaiUnit)()
    ''' <summary>
    ''' The ownership plan the last passing verification produced, handed on to the
    ''' transfer so the selection cannot shift between measuring and writing. Cleared
    ''' whenever «Transferă» is disabled, so a stale plan can never outlive the
    ''' verification that justified it.
    ''' </summary>
    Private _ownership As OwnershipPlan
    Private _cancellation As CancellationTokenSource
    Private _busy As Boolean
    ''' <summary>Resolved by SetBusy(False), i.e. only once the running operation's own
    ''' Finally has disposed every Access/MariaDB connection it opened. FormClosing awaits
    ''' this instead of closing the window - and the process behind it - while ACE is still
    ''' mid-call, which used to crash on exit.</summary>
    Private _operationFinished As TaskCompletionSource(Of Boolean)
    Private _boldFont As Font
    Private _normalFont As Font
    Private _journalPath As String

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ---- lifetime -------------------------------------------------------------------

    Private Sub MigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            txtRegistru.Text = _settings.RegistryPath
            _journalPath = _settings.JournalFolder
            txtGazda.Text = _settings.Host
            txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
            txtUtilizator.Text = _settings.User
            txtServerUrl.Text = _settings.ServerUrl

            FillTableList()
            ResetProgress()

            Say("Alegeți registrul «cale.accdb» și apăsați «Citește registrul».")
            Say("Anul transferului este fixat la " &
                TableMaps.TransferYear.ToString(CultureInfo.InvariantCulture) & ".")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.Load", ex)
        End Try
    End Sub

    'Private Async Sub MigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
    '    Try
    '        If _busy Then
    '            Dim answer = MessageBox.Show(
    '                "O operație este în curs. Închiderea o oprește și derulează tranzacția înapoi." &
    '                Environment.NewLine & "Închideți oricum?",
    '                "Migrare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
    '            If answer <> DialogResult.Yes Then
    '                e.Cancel = True
    '                Return
    '            End If

    '            ' Cancelling only asks the background operation to stop - it still has to unwind
    '            ' through its Using blocks and close its Access/MariaDB connections. Closing the
    '            ' window right away closed the process out from under it instead, mid ACE call,
    '            ' which is what showed up as a crash-on-exit dialog. Block this close, wait for
    '            ' SetBusy(False) (set only after that unwind finishes), then close for real.
    '            e.Cancel = True
    '            _cancellation?.Cancel()
    '            Dim pending = _operationFinished
    '            If pending IsNot Nothing Then Await pending.Task
    '            Close()
    '            Return
    '        End If
    '        SaveSettings()
    '    Catch ex As Exception
    '        GlobalErrorLog.Write("MigratorForm.FormClosing", ex)
    '    End Try
    'End Sub

    ''' <summary>
    ''' Kills the process without DLL_PROCESS_DETACH running for any loaded module.
    ''' </summary>
    ''' <remarks>
    ''' The WinDbg trace pins this down exactly: Environment.Exit -&gt; the OS's
    ''' ExitProcess -&gt; ntdll!LdrShutdownProcess -&gt; DLL_PROCESS_DETACH to every loaded
    ''' DLL -&gt; mso98win32client.dll's own static destructor for its Floodgate telemetry
    ''' client crashes on a bad pointer. That is a bug in Office's shared component, and
    ''' it fires on ANY exit through ExitProcess - Environment.Exit included, which is why
    ''' that "fix" changed nothing, and a Sleep beforehand changed nothing either, because
    ''' there is no race to win: DLL_PROCESS_DETACH runs every time, not sometimes.
    ''' TerminateProcess is the one documented way to skip DLL notification entirely.
    ''' </remarks>
    Private Shared Sub KillProcessNow()
        TerminateProcess(GetCurrentProcess(), 0)
    End Sub

    Private Sub MigratorForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            _boldFont?.Dispose()
            _normalFont?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.FormClosed", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Copies every box the operator can edit into the settings and writes them to disk.
    ''' </summary>
    ''' <remarks>
    ''' The server's ADDRESS belongs to the «Server Python» group and lives here between runs.
    ''' Its API KEY does not: a key is a secret, and this class holds none - see the note at
    ''' the top of <see cref="MigratorSettings"/>. It is read out of its box at the moment a
    ''' request is built, exactly like the two passwords.
    ''' <para>
    ''' Called from <see cref="BuildRequest"/> too, before anything is launched, so every
    ''' step reads what the operator typed and not what was on disk when the form opened.
    ''' </para>
    ''' </remarks>
    Private Sub SaveSettings()
        _settings.RegistryPath = txtRegistru.Text.Trim()
        _settings.JournalFolder = If(_journalPath, String.Empty).Trim()
        _settings.Host = txtGazda.Text.Trim()
        _settings.Port = ParsePort()
        _settings.User = txtUtilizator.Text.Trim()
        _settings.Dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        _settings.ServerUrl = txtServerUrl.Text.Trim()
        ' No secret reaches this call - MigratorSettings has no field for one. The API key
        ' stays in its box and dies with the window.
        '
        ' txtCodFiscal is deliberately NOT here, and MigratorSettings has no field for it
        ' either (D16). It is an override for ONE run: persisting it would let a value
        ' typed to test something quietly select the wrong statements in a real migration
        ' weeks later, and the box would look empty-by-default while not being it. The
        ' journal header records both the registry value and the value used, which is where
        ' that history belongs.
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

    Private Sub btnRasfoireJurnal_Click(sender As Object, e As EventArgs)
        Try
            Using dialog As New FolderBrowserDialog
                dialog.Description = "Alegeți dosarul jurnalului SQL"
                If Directory.Exists(_journalPath) Then dialog.SelectedPath = _journalPath
                If dialog.ShowDialog(Me) = DialogResult.OK Then _journalPath = dialog.SelectedPath
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

            _units = CaiRegistry.Read(path, AccessPassword())
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
            ShowCodFiscal()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.cboDc_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Shows the selected DC's CodFiscal as it stands in the Windows registry, so the
    ''' operator SEES the value a run would use instead of inferring it from an empty box.
    ''' </summary>
    ''' <remarks>
    ''' The box stays editable: what is shown is a starting point, not a lock. Typing over
    ''' it is still the D16 override, and because a shown value equals the registry value,
    ''' <see cref="TransferRequest.ResolvedCodFiscal"/> answers the same either way - the
    ''' displayed text only becomes an override once the operator changes it.
    ''' <para>
    ''' The value comes from <see cref="CodFiscalRegistry"/>, i.e. from
    ''' HKCU\Software\VB and VBA Program Settings\AVACONT\&lt;DC&gt;, NOT from cai.accdb -
    ''' the registry file knows the paths, the Windows store knows the fiscal code.
    ''' </para>
    ''' <para>
    ''' Re-run on every DC change, overwriting whatever is in the box. A code typed while
    ''' another DC was selected belonged to that DC; carrying it over to the next one is
    ''' precisely the silent mismatch D16 exists to avoid.
    ''' </para>
    ''' </remarks>
    Private Sub ShowCodFiscal()
        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        Dim code = CodFiscalRegistry.ForDc(dc)
        txtCodFiscal.Text = code

        If code.Length > 0 Then
            Say($"Cod fiscal din registrul Windows pentru «{dc}»: {code}.")
        Else
            Say($"Registrul Windows nu are cod fiscal pentru «{dc}». Completați-l manual.")
        End If
    End Sub

    ''' <summary>
    ''' Fills the unit grid, binding each <see cref="CaiUnit"/> to its row's
    ''' <see cref="KBot.Controls.KBotDataRow.Tag"/>.
    ''' </summary>
    ''' <remarks>
    ''' The Tag is the point. Reading the ticks back by ROW INDEX would silently assume the
    ''' grid's order still matches the source list - true today, because KBotDataView does
    ''' not reorder itself, but it is an invisible coupling that breaks the day anyone adds
    ''' sorting, and it breaks by migrating the WRONG UNIT rather than by failing. The Tag
    ''' carries the object itself, so order stops mattering entirely.
    ''' </remarks>
    Private Sub FillUnitList()
        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        lblBazaTinta.Text = $"Baza-țintă: {dc}"

        dgvUnitati.BeginUpdate()
        Try
            dgvUnitati.ClearRows()
            For Each unit In CaiRegistry.UnitsOf(_units, dc)
                Dim row = dgvUnitati.AddRow()
                row.Tag = unit
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

    ''' <summary>Fills the table grid, binding each <see cref="TableMap"/> to its row.</summary>
    Private Sub FillTableList()
        dgvTabele.BeginUpdate()
        Try
            dgvTabele.ClearRows()
            For Each map In TableMaps.All()
                Dim row = dgvTabele.AddRow()
                row.Tag = map
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
            Await VerifyAsync(offerSchemaSync:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnVerifica_Click", ex)
            Warn("Verificarea a eșuat." & Environment.NewLine & Environment.NewLine & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Runs every gate, shows the findings, and enables «Transferă» if nothing blocks.
    ''' </summary>
    ''' <param name="offerSchemaSync">
    ''' When the target turns out to hold no tables at all, offer to build its structure from
    ''' the template and then verify again. False on that second pass, so a copy that fails
    ''' to create anything cannot start an endless offer-run-offer loop.
    ''' </param>
    Private Async Function VerifyAsync(offerSchemaSync As Boolean) As Task
        Dim request = BuildRequest()
        If request Is Nothing Then Return

        btnTransfera.Enabled = False
        _ownership = Nothing
        dgvConstatari.ClearRows()
        ClearFindingDetail()
        SetBusy(True)
        _cancellation = New CancellationTokenSource()

        Dim report As VerificationReport = Nothing
        Try
            Dim token = _cancellation.Token
            BeginProgress(Verifier.StepCount)
            report = Await Task.Run(
                Function() New Verifier(request, AddressOf SayFromWorker, AddressOf StepFromWorker).Run(token),
                token)
            ShowFindings(report)
            btnTransfera.Enabled = report.CanRun
            ' Kept only while it is allowed to be used. A plan from a verification that
            ' found something blocking must not sit around waiting to be picked up.
            _ownership = If(report.CanRun, report.Ownership, Nothing)
            Say(If(report.CanRun,
                   "«Transferă» este acum activ.",
                   "«Transferă» rămâne inactiv până când nu mai există constatări blocante."))
        Catch ex As OperationCanceledException
            Say("Verificare oprită.")
        Finally
            EndProgress()
            SetBusy(False)
            _cancellation?.Dispose()
            _cancellation = Nothing
        End Try

        If report Is Nothing OrElse Not offerSchemaSync Then Return
        If Not report.Findings.Any(Function(f) f.Kind = Finding.BAZA_FARA_TABELE) Then Return

        Await OfferBuildStructureAsync(request)
    End Function

    ' ---- structure for an empty database ----------------------------------------------------

    ''' <summary>
    ''' Offers to build an empty target database's structure directly - the same MariaDB
    ''' connection the migrator already has open, no HTTP and no separate server involved.
    ''' </summary>
    ''' <remarks>
    ''' Replaces the schema_sync/SSH route (see remarks on
    ''' <see cref="TargetServer.BuildStructureInEmptyDatabase"/> for why it broke).
    ''' Deliberately a prompt rather than a button, same as before: the form's layout is the
    ''' operator's, and this is a remedy for one specific finding, not a step of the normal
    ''' flow. It appears exactly when it applies.
    ''' </remarks>
    Private Async Function OfferBuildStructureAsync(request As TransferRequest) As Task
        Dim dc = request.TargetDatabase
        Dim answer = MessageBox.Show(
            $"Baza «{dc}» există, dar nu are niciun tabel." & Environment.NewLine & Environment.NewLine &
            "Structura poate fi construită acum după «" & request.TemplateDatabase & "», " &
            "pe serverul MariaDB de mai sus." &
            Environment.NewLine & Environment.NewLine &
            "După aceea verificarea se reia automat." & Environment.NewLine & Environment.NewLine &
            "Construiți structura acum?",
            "Bază fără tabele", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If answer <> DialogResult.Yes Then
            Say("Structura nu a fost construită. Baza rămâne fără tabele.")
            Return
        End If

        SetBusy(True)
        BeginProgress(0)
        _cancellation = New CancellationTokenSource()

        Try
            Dim token = _cancellation.Token
            Dim created = Await Task.Run(
                Function() request.Server.BuildStructureInEmptyDatabase(
                    dc, request.TemplateDatabase, AddressOf SayFromWorker),
                token)
            Say($"Structură construită: {created} tabele.")
        Catch ex As OperationCanceledException
            Say("Construirea a fost oprită.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.OfferBuildStructureAsync", ex)
            Warn("Structura nu a putut fi construită." &
                 Environment.NewLine & Environment.NewLine & ex.Message)
            Return
        Finally
            EndProgress()
            SetBusy(False)
            _cancellation?.Dispose()
            _cancellation = Nothing
        End Try

        ' offerSchemaSync:=False - if the copy reported success but created nothing, the
        ' second pass reports it once and stops, rather than offering the same run again.
        Await VerifyAsync(offerSchemaSync:=False)
    End Function

    ' ---- findings ---------------------------------------------------------------------------

    Private Sub ShowFindings(report As VerificationReport)
        dgvConstatari.BeginUpdate()
        Try
            dgvConstatari.ClearRows()
            ' Blocking first: the operator must not have to hunt for what stops the run.
            For Each finding In report.Findings.
                OrderByDescending(Function(f) CInt(f.Severity)).
                ThenBy(Function(f) f.Table, StringComparer.OrdinalIgnoreCase)
                Dim row = dgvConstatari.AddRow()
                ' The finding itself rides on the row, so the detail pane reads the object
                ' rather than re-parsing the cells it just wrote.
                row.Tag = finding
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

    Private Sub dgvConstatari_CellClick(sender As Object, e As KBot.Controls.KBotCellEventArgs) _
        Handles dgvConstatari.CellClick
        Try
            ShowFindingDetail(FindingAt(e.RowIndex))
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvConstatari_CellClick", ex)
        End Try
    End Sub

    Private Function FindingAt(rowIndex As Integer) As Finding
        If rowIndex < 0 OrElse rowIndex >= dgvConstatari.Rows.Count Then Return Nothing
        Return TryCast(dgvConstatari.Rows(rowIndex).Tag, Finding)
    End Function

    ''' <summary>
    ''' Writes the clicked finding into the detail pane, each label in bold.
    ''' </summary>
    Private Sub ShowFindingDetail(finding As Finding)
        If finding Is Nothing Then
            ClearFindingDetail()
            Return
        End If

        rtbInfoRowConstatari.Clear()
        AppendLabelled("COLOANĂ", If(finding.Column.Length > 0, finding.Column, "—"))
        AppendLabelled("MESAJ", finding.Message)
        rtbInfoRowConstatari.SelectionStart = 0
        rtbInfoRowConstatari.ScrollToCaret()
    End Sub

    Private Sub ClearFindingDetail()
        rtbInfoRowConstatari.Clear()
    End Sub

    ''' <summary>Appends «<b>LABEL</b> - value» plus a newline.</summary>
    Private Sub AppendLabelled(label As String, value As String)
        EnsureDetailFonts()

        rtbInfoRowConstatari.SelectionStart = rtbInfoRowConstatari.TextLength
        rtbInfoRowConstatari.SelectionLength = 0
        rtbInfoRowConstatari.SelectionFont = _boldFont
        rtbInfoRowConstatari.AppendText(label)

        rtbInfoRowConstatari.SelectionStart = rtbInfoRowConstatari.TextLength
        rtbInfoRowConstatari.SelectionLength = 0
        rtbInfoRowConstatari.SelectionFont = _normalFont
        rtbInfoRowConstatari.AppendText(" - " & If(value, String.Empty) & Environment.NewLine)
    End Sub

    ''' <summary>
    ''' Builds the two fonts the detail pane needs, rebuilding them if the theme changed the
    ''' control's font since the last time.
    ''' </summary>
    ''' <remarks>
    ''' Cached rather than made per click: a new Font on every click leaks a GDI handle each
    ''' time, and this pane is clicked once per finding.
    ''' </remarks>
    Private Sub EnsureDetailFonts()
        Dim current = rtbInfoRowConstatari.Font
        If _normalFont IsNot Nothing AndAlso _normalFont.FontFamily.Equals(current.FontFamily) AndAlso
           Math.Abs(_normalFont.Size - current.Size) < 0.01F Then
            Return
        End If

        _boldFont?.Dispose()
        _normalFont?.Dispose()
        _normalFont = New Font(current, FontStyle.Regular)
        _boldFont = New Font(current, FontStyle.Bold)
    End Sub

    ' ---- transfer -------------------------------------------------------------------------

    Private Async Sub btnTransfera_Click(sender As Object, e As EventArgs) Handles btnTransfera.Click
        Try
            Dim request = BuildRequest()
            If request Is Nothing Then Return

            ' The plan the VERIFICATION built, not a fresh one. «Transferă» is only enabled
            ' by a verification that has just built one, so this is never Nothing in
            ' practice - and reusing it is the point: what the operator was shown is what
            ' gets written, with no chance of the selection shifting in between.
            If _ownership Is Nothing Then
                Warn("Rulați întâi «Verifică»: proprietatea rândurilor se stabilește acolo, " &
                     "iar transferul scrie exact ce s-a verificat.")
                Return
            End If

            Dim confirmation = MessageBox.Show(
                $"Se scriu {request.SelectedTables.Count} tabele pentru {request.Units.Count} unități " &
                $"în baza «{request.TargetDatabase}»." & Environment.NewLine & Environment.NewLine &
                "Totul într-o singură tranzacție: orice eșec derulează tot înapoi." & Environment.NewLine &
                "Continuați?",
                "Transferă", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirmation <> DialogResult.Yes Then Return

            SetBusy(True)
            BeginProgress(0)
            _cancellation = New CancellationTokenSource()

            Try
                Dim token = _cancellation.Token
                Dim runner As New TransferRunner(request, _ownership,
                                                 AddressOf SayFromWorker, AddressOf ProgressFromWorker)
                Dim result = Await Task.Run(Function() runner.Run(token), token)

                ' The LAST thing that happens to a unit database — see
                ' docs/PLAN_ForexeIngest.md §3. It runs ONLY after a COMMIT, and never on
                ' AVACONT_SURSA; AutoIncrementStep refuses both cases itself, so the guard
                ' lives in one place rather than being restated here.
                '
                ' Its failure is NOT the transfer's failure: the rows are already committed
                ' and stay committed. Reported separately so the operator is never told a
                ' successful transfer was rolled back.
                Dim autoIncrement As AutoIncrementReport = Nothing
                If result.Committed Then
                    Try
                        Dim finalStep As New AutoIncrementStep(
                            request.Server, request.TargetDatabase,
                            request.TemplateDatabase, request.CommonDatabase,
                            AddressOf SayFromWorker)
                        autoIncrement = Await Task.Run(Function() finalStep.Run(True, token), token)
                    Catch stepError As Exception
                        GlobalErrorLog.Write("MigratorForm.AutoIncrementStep", stepError)
                        Warn("Transferul a reușit și rândurile sunt scrise, dar pasul final " &
                             "(AUTO_INCREMENT pe cheile primare) a eșuat:" & Environment.NewLine &
                             Environment.NewLine & stepError.Message & Environment.NewLine &
                             Environment.NewLine &
                             "Baza NU este completă până când pasul acesta nu trece. " &
                             "Poate fi rulat din nou — tabelele deja convertite sunt sărite.")
                    End Try
                End If

                ShowResult(result, autoIncrement)
            Finally
                EndProgress()
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

    Private Sub ShowResult(result As TransferResult,
                           Optional autoIncrement As AutoIncrementReport = Nothing)
        For Each line In result.Totals()
            Say("   " & line)
        Next

        If result.Committed Then
            Say($"Transfer încheiat cu COMMIT. Jurnalul: {result.JournalFolder}")
            MessageBox.Show(
                $"Transfer încheiat: {result.TotalWritten} rânduri scrise." & Environment.NewLine &
                AutoIncrementSummary(autoIncrement) &
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

    ''' <summary>
    ''' One line about the final AUTO_INCREMENT step for the transfer's message box.
    ''' Empty when the step did not run at all (a transfer that did not commit).
    ''' </summary>
    Private Shared Function AutoIncrementSummary(report As AutoIncrementReport) As String
        If report Is Nothing Then Return String.Empty

        If report.RefusedBecause IsNot Nothing Then
            Return Environment.NewLine &
                   "Pasul AUTO_INCREMENT nu a rulat: " & report.RefusedBecause & Environment.NewLine
        End If

        If Not report.Succeeded Then
            Return Environment.NewLine & "Pasul AUTO_INCREMENT NU s-a încheiat." & Environment.NewLine
        End If

        Dim converted = report.Tables.Where(Function(t) Not t.Missing AndAlso Not t.AlreadyDone).Count()
        Dim already = report.Tables.Where(Function(t) t.AlreadyDone).Count()
        Dim missing = report.Tables.Where(Function(t) t.Missing).Count()

        Dim text = Environment.NewLine &
                   $"AUTO_INCREMENT: {converted} chei convertite"
        If already > 0 Then text &= $", {already} deja gata"
        If missing > 0 Then text &= $", {missing} tabele absente"
        Return text & "." & Environment.NewLine
    End Function

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

    ''' <summary>
    ''' The one Access password. The form has a single «Parolă fișiere» box, and it covers
    ''' the unit files and the Forexe files alike.
    ''' </summary>
    Private Function AccessPassword() As String
        Return txtParolaUnitati.Text
    End Function

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
    ''' <remarks>
    ''' The ticked units and tables are read from each row's Tag - the object itself - never
    ''' from a row index matched against a separate list. Order cannot desynchronise a thing
    ''' that carries its own identity.
    ''' </remarks>
    Private Function BuildRequest() As TransferRequest
        Dim server = BuildServer()
        If server Is Nothing Then Return Nothing

        Dim dc = Convert.ToString(cboDc.SelectedItem, CultureInfo.InvariantCulture)
        If String.IsNullOrWhiteSpace(dc) Then
            Warn("Nu a fost ales niciun DC. Citiți întâi registrul.")
            Return Nothing
        End If

        Dim journal = If(_journalPath, String.Empty).Trim()
        If journal.Length = 0 Then
            Warn("Dosarul jurnalului SQL lipsește. Fără el transferul nu pornește, " &
                 "ca să nu existe o migrare nescrisă nicăieri.")
            Return Nothing
        End If

        Dim password = AccessPassword()
        Dim request As New TransferRequest(server, dc) With {
            .UnitFilePassword = password,
            .ForexeFilePassword = password,
            .JournalFolder = journal,
            .CommonDatabase = _settings.CommonDatabase,
            .TemplateDatabase = _settings.TemplateDatabase,
            .OperatorName = Environment.UserName,
            .PopulateUnitati = True,
            .CodFiscalOverride = txtCodFiscal.Text
        }

        ' The WHOLE registry, not the selection. Decision D7 needs three answers: a unit of
        ' another DC is a normal shape of a shared Forexe file and is skipped in silence,
        ' while a unit in no cai row at all means the file and the registry disagree and
        ' stops the run. Only the full list can tell those two apart.
        request.RegistryUnits.AddRange(_units)

        For Each row In dgvUnitati.Rows
            If Not IsTicked(row) Then Continue For
            Dim unit = TryCast(row.Tag, CaiUnit)
            If unit IsNot Nothing Then request.Units.Add(unit)
        Next
        If request.Units.Count = 0 Then
            Warn("Nu a fost bifată nicio unitate.")
            Return Nothing
        End If

        For Each row In dgvTabele.Rows
            If Not IsTicked(row) Then Continue For
            Dim map = TryCast(row.Tag, TableMap)
            If map IsNot Nothing Then request.SelectedTables.Add(map.TargetTable)
        Next
        If request.SelectedTables.Count = 0 Then
            Warn("Nu a fost bifat niciun tabel. Lista goală nu înseamnă «toate».")
            Return Nothing
        End If

        SaveSettings()
        Return request
    End Function

    Private Shared Function IsTicked(row As KBot.Controls.KBotDataRow) As Boolean
        Dim value = row("bifa")
        Return TypeOf value Is Boolean AndAlso CBool(value)
    End Function

    ' ---- progress, log and busy state ----------------------------------------------------------

    ''' <summary>
    ''' Puts the bar into a known state. <paramref name="steps"/> of 0 means "unknown length",
    ''' which is the marquee.
    ''' </summary>
    Private Sub BeginProgress(steps As Integer)
        If steps > 0 Then
            prgTransfer.Style = ProgressBarStyle.Blocks
            prgTransfer.Minimum = 0
            prgTransfer.Maximum = steps
            prgTransfer.Value = 0
        Else
            prgTransfer.Style = ProgressBarStyle.Marquee
        End If
    End Sub

    Private Sub EndProgress()
        prgTransfer.Style = ProgressBarStyle.Blocks
        prgTransfer.Value = prgTransfer.Maximum
    End Sub

    Private Sub ResetProgress()
        prgTransfer.Style = ProgressBarStyle.Blocks
        prgTransfer.Minimum = 0
        prgTransfer.Maximum = 100
        prgTransfer.Value = 0
    End Sub

    ''' <summary>Verification step, from the worker thread.</summary>
    Private Sub StepFromWorker(done As Integer, total As Integer, label As String)
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(Of Integer, Integer, String)(AddressOf ShowStep), done, total, label)
                Return
            End If
            ShowStep(done, total, label)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.StepFromWorker", ex)
        End Try
    End Sub

    Private Sub ShowStep(done As Integer, total As Integer, label As String)
        Try
            If total > 0 Then
                prgTransfer.Maximum = total
                prgTransfer.Value = Math.Max(0, Math.Min(done, total))
            End If
            If Not String.IsNullOrEmpty(label) Then Say(label)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.ShowStep", ex)
        End Try
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

    Private Sub SetBusy(busy As Boolean)
        _busy = busy
        btnVerifica.Enabled = Not busy
        btnTesteaza.Enabled = Not busy
        btnCitesteRegistru.Enabled = Not busy
        btnOpreste.Enabled = busy
        If busy Then btnTransfera.Enabled = False
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)

        If busy Then
            _operationFinished = New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
        Else
            _operationFinished?.TrySetResult(True)
        End If
    End Sub

    Private Sub Warn(message As String)
        Say(message.Replace(Environment.NewLine, " "))
        MessageBox.Show(message, "Migrare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Async Sub MigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
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

                e.Cancel = True
                _cancellation?.Cancel()
                Dim pending = _operationFinished
                If pending IsNot Nothing Then Await pending.Task
                Close()
                Return
            End If

            SaveSettings()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.FormClosing", ex)
        End Try

        KillProcessNow()
    End Sub
End Class
