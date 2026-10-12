Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' VRC.Migrator, the whole flow of ONE transfer per DC + subunit: (1) choose the folder and read the Access files read-only
''' (VC05-01); (2) associate each file with a gestiune / type / exercise and check the plan (VC05-02); (3) test the server
''' and transfer everything in one transaction (VC05-03); (4) stop before COMMIT, journal, diagnose a lost answer (VC05-04);
''' (5) the reconciliation report (VC05-05). Nothing is written to Access. After a confirmed COMMIT the subunit can never be
''' imported again: there is no re-import.
''' </summary>
Public Class VrcMigratorForm

    Private _busy As Boolean
    Private _files As New List(Of VrcSourceFile)()
    Private _profiles As New List(Of VrcFileProfile)()
    Private _lastReport As String = String.Empty
    Private _state As VrcRunState = VrcRunState.Unread
    Private _testedHash As String = String.Empty
    Private _cts As CancellationTokenSource = Nothing
    Private _logPath As String = String.Empty
    Private _committed As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub VrcMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim k_tip = dgvPlan.Columns.First(Function(k_c) k_c.Key = "tip")
            k_tip.ComboItems = VrcPlan.GestiuneTypes.Cast(Of Object).ToList()
            txtPort.Text = "3306"
            SetState(VrcRunState.Unread, "Alegeți folderul cu bazele subunității.")
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.Load", ex)
        End Try
    End Sub

    ' ---- state and buttons -------------------------------------------------------------------------------------------------
    Private Sub SetState(k_state As VrcRunState, k_text As String)
        _state = k_state
        lblStare.Text = $"[{StateText(k_state)}] {k_text}"
        UpdateButtons()
    End Sub

    Private Shared Function StateText(k_state As VrcRunState) As String
        Select Case k_state
            Case VrcRunState.Unread : Return "NECITIT"
            Case VrcRunState.PlanRead : Return "PLAN_CITIT"
            Case VrcRunState.Blocked : Return "BLOCAT"
            Case VrcRunState.Tested : Return "TESTAT"
            Case VrcRunState.InImport : Return "IN_IMPORT"
            Case VrcRunState.StopRequested : Return "OPRIRE_SOLICITATA"
            Case VrcRunState.RollbackConfirmed : Return "ROLLBACK_CONFIRMAT"
            Case VrcRunState.CommitConfirmed : Return "COMMIT_CONFIRMAT"
            Case VrcRunState.ResultUnknown : Return "REZULTAT_NECUNOSCUT"
            Case Else : Return "RECONCILIAT"
        End Select
    End Function

    Private Sub UpdateButtons()
        Dim k_idle As Boolean = Not _busy
        Dim k_hasPlan As Boolean = _profiles.Count > 0
        btnFolder.Enabled = k_idle AndAlso Not _committed
        btnToate.Enabled = k_idle AndAlso _files.Count > 0
        btnNiciunul.Enabled = k_idle AndAlso _files.Count > 0
        btnCiteste.Enabled = k_idle AndAlso _files.Count > 0 AndAlso Not _committed
        btnSalveaza.Enabled = k_idle AndAlso _lastReport.Length > 0
        btnVerifica.Enabled = k_idle AndAlso k_hasPlan AndAlso Not _committed
        btnExportaPlan.Enabled = k_idle AndAlso k_hasPlan
        btnTesteaza.Enabled = k_idle AndAlso k_hasPlan AndAlso Not _committed
        btnMigreaza.Enabled = k_idle AndAlso _state = VrcRunState.Tested AndAlso Not _committed
        btnOpreste.Enabled = _busy AndAlso _cts IsNot Nothing AndAlso _state = VrcRunState.InImport
        btnDiagnostic.Enabled = k_idle AndAlso k_hasPlan
    End Sub

    ' ---- step 1: folder and reading (VC05-01) -------------------------------------------------------------------------------
    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        Try
            If fbdFolder.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_ignored As New List(Of String)()
            _files = VrcSourceFile.Scan(fbdFolder.SelectedPath, k_ignored)
            _profiles.Clear()
            dgvPlan.ClearRows()
            lblFolder.Text = fbdFolder.SelectedPath
            _lastReport = String.Empty
            rtbRaport.Clear()
            ShowFiles()
            Dim k_text As New StringBuilder($"{_files.Count} fișiere găsite.")
            If k_ignored.Count > 0 Then k_text.Append($" Ignorate: {String.Join(", ", k_ignored)}.")
            SetState(VrcRunState.Unread, k_text.ToString())
        Catch ex As Exception
            Fail("VrcMigratorForm.Folder", "Folderul nu a putut fi citit.", ex)
        End Try
    End Sub

    Private Sub ShowFiles()
        dgvFisiere.BeginUpdate()
        Try
            dgvFisiere.ClearRows()
            Dim k_yearsSeen As New Dictionary(Of Integer, Integer)()
            For Each k_f In _files.Where(Function(k_x) k_x.Kind = VrcFileKind.Year)
                k_yearsSeen(k_f.Year) = If(k_yearsSeen.ContainsKey(k_f.Year), k_yearsSeen(k_f.Year) + 1, 1)
            Next
            For Each k_f In _files
                Dim k_row = dgvFisiere.AddRow()
                k_row("sel") = k_f.Selected
                k_row("fisier") = k_f.FileName
                k_row("format") = k_f.Format
                k_row("rol") = RoleText(k_f)
                k_row("an") = If(k_f.Kind = VrcFileKind.Year, k_f.Year.ToString(CultureInfo.InvariantCulture), "—")
                k_row("dimensiune") = (k_f.SizeBytes / 1024.0).ToString("N0", CultureInfo.InvariantCulture) & " KB"
                k_row("nota") = If(k_f.Kind = VrcFileKind.Year AndAlso k_yearsSeen(k_f.Year) > 1, "același an în două formate (.accdb și .mdb)",
                                   If(k_f.Kind = VrcFileKind.Other, "nume neobișnuit: nebifat implicit", String.Empty))
            Next
        Finally
            dgvFisiere.EndUpdate()
        End Try
    End Sub

    Private Shared Function RoleText(k_f As VrcSourceFile) As String
        Select Case k_f.Kind
            Case VrcFileKind.Special
                Return "Bază specială OI / MF (2100 nu e an)"
            Case VrcFileKind.Year
                Return "Exercițiu Materiale / Alimente"
            Case Else
                Return "Alt nume"
        End Select
    End Function

    Private Sub SetAll(k_value As Boolean)
        For k_i As Integer = 0 To dgvFisiere.RowCount - 1
            dgvFisiere.Rows(k_i)("sel") = k_value
        Next
        dgvFisiere.Invalidate()
    End Sub

    Private Sub btnToate_Click(sender As Object, e As EventArgs) Handles btnToate.Click
        SetAll(True)
    End Sub

    Private Sub btnNiciunul_Click(sender As Object, e As EventArgs) Handles btnNiciunul.Click
        SetAll(False)
    End Sub

    Private Async Sub btnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
        Try
            Dim k_chosen As New List(Of VrcSourceFile)()
            For k_i As Integer = 0 To dgvFisiere.RowCount - 1
                If Convert.ToBoolean(dgvFisiere.Rows(k_i)("sel"), CultureInfo.InvariantCulture) Then k_chosen.Add(_files(k_i))
            Next
            If k_chosen.Count = 0 Then
                SetState(_state, "Bifați cel puțin un fișier.")
                Return
            End If
            _busy = True
            UpdateButtons()
            rtbRaport.Clear()
            Dim k_progress As Action(Of String) = Sub(k_text) BeginInvoke(Sub() lblStare.Text = k_text)
            _profiles = Await Task.Run(Function() k_chosen.Select(Function(k_f) VrcProfiler.Profile(k_f, k_progress)).ToList())
            _chosen = k_chosen
            _lastReport = VrcReport.ToMarkdown(_profiles)
            rtbRaport.Text = _lastReport
            FillPlanGrid()
            Dim k_bad As Integer = _profiles.Where(Function(k_p) k_p.ReadError.Length > 0 OrElse Not k_p.Stable).Count()
            _busy = False
            SetState(VrcRunState.PlanRead, $"{_profiles.Count} fișiere citite" & If(k_bad > 0, $"; {k_bad} cu probleme (vezi raportul).", ". Completați planul și apăsați «Verifică planul»."))
        Catch ex As Exception
            _busy = False
            Fail("VrcMigratorForm.Citeste", "Citirea a eșuat.", ex)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    Private _chosen As New List(Of VrcSourceFile)()

    Private Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If sfdRaport.ShowDialog(Me) <> DialogResult.OK Then Return
            File.WriteAllText(sfdRaport.FileName, _lastReport, New UTF8Encoding(False))
            lblStare.Text = "Raport salvat: " & sfdRaport.FileName
        Catch ex As Exception
            Fail("VrcMigratorForm.Salveaza", "Raportul nu a putut fi salvat.", ex)
        End Try
    End Sub

    ' ---- step 2: the plan (VC05-02) ------------------------------------------------------------------------------------------
    ''' <summary>One row per file read, with the proposal from the name and the flags. The operator changes it; nothing is approved automatically.</summary>
    Private Sub FillPlanGrid()
        dgvPlan.BeginUpdate()
        Try
            dgvPlan.ClearRows()
            _planFile.Clear()
            For k_i As Integer = 0 To _profiles.Count - 1
                Dim k_profile = _profiles(k_i)
                Dim k_source = _chosen(k_i)
                Dim k_types As List(Of String) = If(k_profile.ReadError.Length > 0, New List(Of String) From {String.Empty}, VrcPlan.SuggestTypes(k_profile))
                For Each k_type In k_types
                    Dim k_row = dgvPlan.AddRow()
                    _planFile.Add(k_i)
                    k_row("fisier") = k_profile.FileName
                    k_row("an") = VrcPlan.SuggestYear(k_profile, k_source).ToString(CultureInfo.InvariantCulture)
                    k_row("gestiune") = VrcPlan.SuggestName(k_type)
                    k_row("tip") = k_type
                    k_row("nota") = PlanNote(k_profile, k_source, k_type, k_types.Count)
                Next
            Next
        Finally
            dgvPlan.EndUpdate()
        End Try
        If txtSubunitate.Text.Trim().Length = 0 Then
            Dim k_dc As String = Nothing
            If _profiles.Count > 0 AndAlso _profiles(0).Config.TryGetValue("UNIT.DC", k_dc) Then txtSubunitate.Text = String.Empty
        End If
    End Sub

    Private ReadOnly _planFile As New List(Of Integer)()

    Private Shared Function PlanNote(k_profile As VrcFileProfile, k_source As VrcSourceFile, k_type As String, k_parts As Integer) As String
        If k_source.Kind = VrcFileKind.Special Then
            Return If(k_profile.MinNirYear > 0, $"bază specială; anul propus = cel mai mic an din NIR ({k_profile.MinNirYear})", "bază specială: nu are date NIR, alegeți anul")
        End If
        If k_parts > 1 AndAlso k_profile.Split IsNot Nothing Then
            Return If(k_type = "ALIMENTE", $"conturi de tip A: {k_profile.Split.AccountsAlimente} conturi, {k_profile.Split.NirAlimente} NIR, {k_profile.Split.BonAlimente} bonuri", $"conturi M / fără tip: {k_profile.Split.AccountsMateriale} conturi, {k_profile.Split.NirMateriale} NIR, {k_profile.Split.BonMateriale} bonuri")
        End If
        Return "fișier cu un singur fel de conturi (propunere din conturile Masini)"
    End Function

    ''' <summary>The plan as the grid and the fields say it now.</summary>
    Private Function CurrentPlan() As VrcPlan
        Dim k_plan As New VrcPlan() With {.SubunitName = txtSubunitate.Text}
        For k_i As Integer = 0 To dgvPlan.RowCount - 1
            Dim k_row = dgvPlan.Rows(k_i)
            Dim k_year As Integer
            Integer.TryParse(Convert.ToString(k_row("an"), CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_year)
            k_plan.Sources.Add(New VrcPlanSource() With {
                .Source = _chosen(_planFile(k_i)), .Profile = _profiles(_planFile(k_i)), .Year = k_year,
                .GestiuneName = Convert.ToString(k_row("gestiune"), CultureInfo.InvariantCulture),
                .GestiuneType = Convert.ToString(k_row("tip"), CultureInfo.InvariantCulture)})
        Next
        k_plan.Evaluate()
        Return k_plan
    End Function

    Private Function PlanText(k_plan As VrcPlan) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine($"PLAN — DC {If(k_plan.Dc.Length = 0, "(necunoscut)", k_plan.Dc)}, subunitate «{k_plan.SubunitName.Trim()}», ZEC {k_plan.Zec}, scară {k_plan.Scale}, hash {k_plan.Hash().Substring(0, 12)}…")
        k_sb.AppendLine()
        If k_plan.Blockers.Count = 0 Then
            k_sb.AppendLine("Planul este complet: nu există motive de blocare.")
        Else
            k_sb.AppendLine($"BLOCAJE ({k_plan.Blockers.Count}):")
            For Each k_b In k_plan.Blockers
                k_sb.AppendLine("  - " & k_b)
            Next
        End If
        If k_plan.Warnings.Count > 0 Then
            k_sb.AppendLine()
            k_sb.AppendLine($"AVERTIZĂRI ({k_plan.Warnings.Count}, nu blochează):")
            For Each k_w In k_plan.Warnings
                k_sb.AppendLine("  - " & k_w)
            Next
        End If
        Return k_sb.ToString()
    End Function

    Private Sub btnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            Dim k_plan = CurrentPlan()
            rtbRaport.Text = PlanText(k_plan)
            _testedHash = String.Empty   ' any change of the plan invalidates the test
            If k_plan.IsComplete Then
                SetState(VrcRunState.PlanRead, "Planul este complet. Introduceți serverul și apăsați «Testează».")
            Else
                SetState(VrcRunState.Blocked, $"{k_plan.Blockers.Count} motive de blocare (vezi jos).")
            End If
        Catch ex As Exception
            Fail("VrcMigratorForm.Verifica", "Planul nu a putut fi verificat.", ex)
        End Try
    End Sub

    Private Sub btnExportaPlan_Click(sender As Object, e As EventArgs) Handles btnExportaPlan.Click
        Try
            Dim k_plan = CurrentPlan()
            If sfdPlan.ShowDialog(Me) <> DialogResult.OK Then Return
            File.WriteAllText(sfdPlan.FileName, k_plan.ToJson(), New UTF8Encoding(False))
            lblStare.Text = "Plan exportat: " & sfdPlan.FileName
        Catch ex As Exception
            Fail("VrcMigratorForm.ExportaPlan", "Planul nu a putut fi exportat.", ex)
        End Try
    End Sub

    ' ---- the name of the subunit comes from cale.accdb ------------------------------------------------------------------------
    Private Sub btnCale_Click(sender As Object, e As EventArgs) Handles btnCale.Click
        Try
            If lblFolder.Text.Length = 0 OrElse lblFolder.Text.StartsWith("Niciun", StringComparison.Ordinal) Then
                SetState(_state, "Alegeți întâi folderul cu bazele: denumirea se caută după numele lui.")
                Return
            End If
            If ofdCale.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_rows = VrcCale.Read(ofdCale.FileName)
            Dim k_matches = VrcCale.Match(k_rows, lblFolder.Text)
            Dim k_names = VrcCale.Names(k_matches)
            If k_names.Count = 0 Then
                SetState(_state, $"În tabelul Cale nu există niciun rând (în afara celor 2100) cu drumul care se termină cu «{IO.Path.GetFileName(lblFolder.Text.TrimEnd("\"c))}». Scrieți denumirea.")
                Return
            End If
            txtSubunitate.Text = k_names(0)
            _testedHash = String.Empty
            Dim k_dcs = k_matches.Select(Function(k_r) k_r.Dc).Where(Function(k_d) k_d.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            Dim k_note As String = If(k_names.Count > 1, $" Atenție: mai multe denumiri pentru acest drum ({String.Join(" | ", k_names)}); verificați.", String.Empty)
            SetState(_state, $"Denumirea subunității: «{k_names(0)}» (din Cale; DC în Cale: {If(k_dcs.Count = 0, "—", String.Join(", ", k_dcs))}).{k_note}")
        Catch ex As Exception
            Fail("VrcMigratorForm.Cale", "cale.accdb nu a putut fi citit.", ex)
        End Try
    End Sub

    ' ---- server ------------------------------------------------------------------------------------------------------------------
    Private Function BuildServer() As TargetServer
        Dim k_port As Integer
        If txtGazda.Text.Trim().Length = 0 OrElse txtUtilizator.Text.Trim().Length = 0 OrElse
           Not Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_port) OrElse k_port < 1 OrElse k_port > 65535 Then
            Throw New InvalidOperationException("Completați gazda, portul (1–65535) și utilizatorul serverului.")
        End If
        Return New TargetServer(New TargetConnection(txtGazda.Text.Trim(), CUInt(k_port), txtUtilizator.Text.Trim(), txtParola.Text))
    End Function

    ' ---- step 3a: Testează ---------------------------------------------------------------------------------------------------------
    Private Async Sub btnTesteaza_Click(sender As Object, e As EventArgs) Handles btnTesteaza.Click
        Try
            Dim k_plan = CurrentPlan()
            rtbRaport.Text = PlanText(k_plan)
            _testedHash = String.Empty
            If Not k_plan.IsComplete Then
                SetState(VrcRunState.Blocked, "Planul are motive de blocare; serverul nu a fost verificat.")
                Return
            End If
            Dim k_server = BuildServer()
            _busy = True
            UpdateButtons()
            lblStare.Text = "Se verifică serverul…"
            Dim k_problems = Await Task.Run(Function() VrcWriter.Preflight(k_server, k_plan))
            Dim k_suppliers As List(Of String) = Nothing
            If k_problems.Count = 0 Then k_suppliers = Await Task.Run(Function() VrcWriter.UnmatchedSuppliers(k_server, k_plan))
            _busy = False
            Dim k_sb As New StringBuilder(PlanText(k_plan))
            k_sb.AppendLine()
            If k_problems.Count > 0 Then
                k_sb.AppendLine($"SERVER — {k_problems.Count} probleme:")
                For Each k_p In k_problems
                    k_sb.AppendLine("  - " & k_p)
                Next
                rtbRaport.Text = k_sb.ToString()
                SetState(VrcRunState.Blocked, "Serverul nu este gata (vezi jos).")
                Return
            End If
            k_sb.AppendLine("SERVER — gata: baza, tabelele VC_, subunitatea nouă.")
            If k_suppliers IsNot Nothing AndAlso k_suppliers.Count > 0 Then
                k_sb.AppendLine($"AVERTIZARE: {k_suppliers.Count} furnizori fără partener KBOT cu același cod fiscal (NIR-urile lor se importă fără partener): {String.Join(", ", k_suppliers.Take(20))}{If(k_suppliers.Count > 20, "…", "")}")
            End If
            rtbRaport.Text = k_sb.ToString()
            _testedHash = k_plan.Hash()
            SetState(VrcRunState.Tested, "Testat. «Migrează» face transferul unic; orice modificare a planului cere retestare.")
        Catch ex As Exception
            _busy = False
            Fail("VrcMigratorForm.Testeaza", "Testul a eșuat.", ex)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    ' ---- step 3b/4: Migrează, Oprește ---------------------------------------------------------------------------------------------------
    Private Async Sub btnMigreaza_Click(sender As Object, e As EventArgs) Handles btnMigreaza.Click
        Try
            Dim k_plan = CurrentPlan()
            If Not k_plan.IsComplete OrElse k_plan.Hash() <> _testedHash Then
                _testedHash = String.Empty
                SetState(VrcRunState.PlanRead, "Planul s-a schimbat de la ultimul test; apăsați din nou «Testează».")
                Return
            End If
            Dim k_ask = KBotMessage.Show(Me,
                $"Se face transferul UNIC al subunității «{k_plan.SubunitName.Trim()}» în baza {k_plan.Dc}: {k_plan.Sources.Count} fișiere, într-o singură tranzacție." & Environment.NewLine & Environment.NewLine &
                "După COMMIT subunitatea nu mai poate fi importată (nu există reimport). Continuați?",
                "Migrează", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If k_ask <> DialogResult.Yes Then Return
            Dim k_server = BuildServer()
            _cts = New CancellationTokenSource()
            _busy = True
            _logPath = NewLogPath()
            rtbRaport.Clear()
            SetState(VrcRunState.InImport, "Transfer în curs. «Oprește» anulează înainte de COMMIT.")
            Dim k_token = _cts.Token
            Dim k_user As String = Environment.UserName
            Dim k_log As Action(Of String) = Sub(k_line) BeginInvoke(Sub() AppendLog(k_line))
            Dim k_result = Await Task.Run(Function() VrcWriter.Run(k_server, k_plan, k_user, k_token, k_log))
            _busy = False
            _cts.Dispose()
            _cts = Nothing
            Dim k_report = k_result.ToMarkdown(k_plan)
            _lastReport = k_report
            rtbRaport.Text = k_report
            SaveRunFiles(k_report)
            If k_result.State = VrcRunState.CommitConfirmed Then _committed = True
            SetState(k_result.State, k_result.Message)
            If k_result.State = VrcRunState.ResultUnknown Then
                KBotMessage.Show(Me, k_result.Message, "Rezultat necunoscut", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            _busy = False
            _cts?.Dispose()
            _cts = Nothing
            Fail("VrcMigratorForm.Migreaza", "Transferul a eșuat.", ex)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    Private Sub btnOpreste_Click(sender As Object, e As EventArgs) Handles btnOpreste.Click
        Try
            If _cts Is Nothing Then Return
            _cts.Cancel()
            SetState(VrcRunState.StopRequested, "Oprire solicitată: se oprește la următoarea verificare, înainte de COMMIT.")
            btnOpreste.Enabled = False
        Catch ex As Exception
            Fail("VrcMigratorForm.Opreste", "Oprirea nu a putut fi cerută.", ex)
        End Try
    End Sub

    ' ---- step 4: diagnostic ----------------------------------------------------------------------------------------------------------
    Private Async Sub btnDiagnostic_Click(sender As Object, e As EventArgs) Handles btnDiagnostic.Click
        Try
            Dim k_plan = CurrentPlan()
            If k_plan.Dc.Length = 0 OrElse k_plan.SubunitName.Trim().Length = 0 Then
                SetState(_state, "Pentru diagnostic trebuie să cunoaștem DC-ul (din fișiere) și denumirea subunității.")
                Return
            End If
            Dim k_server = BuildServer()
            _busy = True
            UpdateButtons()
            Dim k_text = Await Task.Run(Function() VrcWriter.Diagnose(k_server, k_plan.Dc, k_plan.SubunitName))
            _busy = False
            _lastReport = k_text
            rtbRaport.Text = k_text
            lblStare.Text = "Diagnostic citit de pe server (doar citire)."
        Catch ex As Exception
            _busy = False
            Fail("VrcMigratorForm.Diagnostic", "Diagnosticul a eșuat.", ex)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    ' ---- journal ----------------------------------------------------------------------------------------------------------------------
    Private Shared Function NewLogPath() As String
        Dim k_dir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
        Directory.CreateDirectory(k_dir)
        Return Path.Combine(k_dir, $"vrc_migrare_{DateTime.Now:yyyyMMdd_HHmmss}.log")
    End Function

    ''' <summary>One line on screen and in the file. Never a password; a logging failure does not change the result of the transfer.</summary>
    Private Sub AppendLog(k_line As String)
        Try
            Dim k_text As String = $"{DateTime.Now:HH:mm:ss} {k_line}"
            rtbRaport.AppendText(k_text & Environment.NewLine)
            If _logPath.Length > 0 Then File.AppendAllText(_logPath, k_text & Environment.NewLine, New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.AppendLog", ex)
        End Try
    End Sub

    Private Sub SaveRunFiles(k_report As String)
        Try
            If _logPath.Length > 0 Then File.WriteAllText(Path.ChangeExtension(_logPath, ".md"), k_report, New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.SaveRunFiles", ex)
        End Try
    End Sub

    Private Sub Fail(k_source As String, k_message As String, k_ex As Exception)
        GlobalErrorLog.Write(k_source, k_ex)
        lblStare.Text = k_message
        KBotMessage.Show(k_message & Environment.NewLine & k_ex.Message, "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>Closing waits for a running read or transfer: the files, the Access driver and the transaction must be released first.</summary>
    Private Sub VrcMigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _busy Then
            e.Cancel = True
            KBotMessage.Show("O operație este în curs. Așteptați să se termine (sau apăsați «Oprește» în timpul transferului) înainte de a închide.", "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

End Class
