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
    ''' <summary>Partners prepared from ANAF; inserted by «Migrează» inside the transfer transaction. Kept between runs (per DC); dropped only after a confirmed COMMIT.</summary>
    Private _prepared As New List(Of VrcNewPartner)()
    ''' <summary>The DC the list in <see cref="_prepared"/> belongs to.</summary>
    Private _preparedDc As String = String.Empty
    ''' <summary>What is remembered from one run to the next (server, files, DC, Subunitate, plan rows); never the password.</summary>
    Private _settings As New VrcSettings()
    ''' <summary>The rows of the VERCON table «cale» (subunit proposals); empty until «Încarcă cale…» or a remembered cale.accdb is loaded.</summary>
    Private _cale As New List(Of VrcCaleRow)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub VrcMigratorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim k_tip = dgvPlan.Columns.First(Function(k_c) k_c.Key = "tip")
            k_tip.ComboItems = VrcPlan.GestiuneTypes.Cast(Of Object).ToList()
            _settings = VrcSettings.Load()
            txtGazda.Text = _settings.Host
            txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
            txtUtilizator.Text = _settings.User
            txtDc.Text = _settings.Dc
            If _settings.LastFolder.Length > 0 AndAlso Directory.Exists(_settings.LastFolder) Then
                fbdFolder.SelectedPath = _settings.LastFolder
                ofdBaze.InitialDirectory = _settings.LastFolder
            End If
            LoadRememberedCale()
            UsePreparedOf(_settings.Dc)
            RestoreFiles()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.Load", ex)
        End Try
    End Sub

    ' ---- state and buttons -------------------------------------------------------------------------------------------------
    Private Sub SetState(k_state As VrcRunState, k_text As String)
        _state = k_state
        lblStare.Text = $"[{StateText(k_state)}] {k_text}"
        ApplyStateLook(k_state)
        UpdateButtons()
    End Sub

    ''' <summary>Dark green = success, orange = failed with rollback, red = critical (unknown result, blocked); the theme colour otherwise.</summary>
    Private Sub ApplyStateLook(k_state As VrcRunState)
        Dim k_palette = KBot.Theming.ThemeManager.Current.Palette
        Select Case k_state
            Case VrcRunState.CommitConfirmed, VrcRunState.Reconciled
                lblStare.ForeColor = ControlPaint.Dark(k_palette.SuccessColor)
                lblStare.Font = New Font(lblStare.Font, FontStyle.Bold)
            Case VrcRunState.RollbackConfirmed
                lblStare.ForeColor = k_palette.WarningColor
                lblStare.Font = New Font(lblStare.Font, FontStyle.Bold)
            Case VrcRunState.ResultUnknown, VrcRunState.Blocked
                lblStare.ForeColor = k_palette.ErrorColor
                lblStare.Font = New Font(lblStare.Font, FontStyle.Bold)
            Case Else
                lblStare.ResetForeColor()
                lblStare.Font = New Font(lblStare.Font, FontStyle.Regular)
        End Select
    End Sub

    ''' <summary>The report, with its first line (the verdict) in the colour of the outcome.</summary>
    Private Sub ShowReport(k_report As String, k_state As VrcRunState)
        rtbRaport.Text = k_report
        Dim k_end As Integer = rtbRaport.Text.IndexOf(vbLf, StringComparison.Ordinal)
        If k_end <= 0 Then Return
        Dim k_palette = KBot.Theming.ThemeManager.Current.Palette
        Dim k_color As Color
        Select Case k_state
            Case VrcRunState.CommitConfirmed, VrcRunState.Reconciled : k_color = ControlPaint.Dark(k_palette.SuccessColor)
            Case VrcRunState.RollbackConfirmed : k_color = k_palette.WarningColor
            Case Else : k_color = k_palette.ErrorColor
        End Select
        rtbRaport.Select(0, k_end)
        rtbRaport.SelectionColor = k_color
        rtbRaport.SelectionFont = New Font(rtbRaport.Font, FontStyle.Bold)
        rtbRaport.Select(0, 0)
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
        btnAdauga.Enabled = k_idle AndAlso Not _committed
        btnSterge.Enabled = k_idle AndAlso _files.Count > 0 AndAlso Not _committed
        btnGoleste.Enabled = k_idle AndAlso _files.Count > 0 AndAlso Not _committed
        btnCiteste.Enabled = k_idle AndAlso _files.Count > 0 AndAlso Not _committed
        btnRaportHtml.Enabled = k_idle AndAlso Not String.IsNullOrWhiteSpace(rtbRaport.Text)
        btnVerifica.Enabled = k_idle AndAlso k_hasPlan AndAlso Not _committed
        btnExportaPlan.Enabled = k_idle AndAlso k_hasPlan
        btnTesteaza.Enabled = k_idle AndAlso k_hasPlan AndAlso Not _committed
        btnParteneri.Enabled = k_idle AndAlso k_hasPlan AndAlso Not _committed
        btnMigreaza.Enabled = k_idle AndAlso _state = VrcRunState.Tested AndAlso Not _committed
        btnOpreste.Enabled = _busy AndAlso _cts IsNot Nothing AndAlso _state = VrcRunState.InImport
        btnDiagnostic.Enabled = k_idle AndAlso k_hasPlan
    End Sub

    Private Sub ReportTextChanged(sender As Object, e As EventArgs) Handles rtbRaport.TextChanged
        btnRaportHtml.Enabled = Not _busy AndAlso Not String.IsNullOrWhiteSpace(rtbRaport.Text)
    End Sub

    ''' <summary>A «deschide folderul» link of the report text (file:///…) opens Explorer with the file selected.</summary>
    Private Sub RtbRaport_LinkClicked(sender As Object, e As LinkClickedEventArgs) Handles rtbRaport.LinkClicked
        Try
            Dim k_uri As Uri = Nothing
            If Uri.TryCreate(e.LinkText.Trim().TrimEnd(")"c), UriKind.Absolute, k_uri) AndAlso k_uri.IsFile Then VrcShell.RevealInExplorer(k_uri.LocalPath)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.LinkClicked", ex)
        End Try
    End Sub

    ''' <summary>DC is remembered as soon as the operator leaves the field, not only on closing.</summary>
    Private Sub UnitFieldLeft(sender As Object, e As EventArgs) Handles txtDc.Leave
        SaveWorkState()
    End Sub

    Private Sub BtnRaportHtml_Click(sender As Object, e As EventArgs) Handles btnRaportHtml.Click
        Try
            If _busy OrElse String.IsNullOrWhiteSpace(rtbRaport.Text) Then Return
            Dim k_report As String = rtbRaport.Text
            If _profiles.Count > 0 AndAlso Not k_report.Contains("# VRC.Migrator — raport de profilare", StringComparison.Ordinal) Then
                k_report &= Environment.NewLine & Environment.NewLine & "---" & Environment.NewLine & Environment.NewLine &
                            VrcReport.ToMarkdown(_profiles)
            End If
            Using k_form As New VrcReportForm(k_report, lblStare.Text)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.RaportHtml", ex)
            KBotMessage.Show("Raportul HTML nu a putut fi deschis." & Environment.NewLine & ex.Message,
                             "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---- step 1: folder and reading (VC05-01) -------------------------------------------------------------------------------
    ''' <summary>«Alege folder…»: the list becomes the files of that one folder.</summary>
    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        LoadFolder()
    End Sub

    ''' <summary>The plan, profiles and report belong to the files read; a changed file list drops them.</summary>
    Private Sub ResetAfterFileListChange()
        ' The prepared partners and the plan choices stay: they are saved by the caller (before the list changes) and by SaveWorkState.
        _profiles.Clear()
        _chosen.Clear()
        dgvPlan.ClearRows()
        _lastReport = String.Empty
        rtbRaport.Clear()
        ShowFiles()
        SaveWorkState()
    End Sub

    ''' <summary>The cale.accdb of the last run is read again at start (read-only). A problem is only noted; the tool works without it.</summary>
    Private Sub LoadRememberedCale()
        Try
            If _settings.CalePath.Length = 0 OrElse Not File.Exists(_settings.CalePath) Then Return
            _cale = VrcCale.Load(_settings.CalePath)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.LoadRememberedCale", ex)   ' the proposals just fall back to the unit name of the files
            _cale = New List(Of VrcCaleRow)()
        End Try
    End Sub

    ''' <summary>«Încarcă cale…»: reads the table cale of cale.accdb; the subunit of each plan row is proposed from it (never over a value the operator changed).</summary>
    Private Sub btnCale_Click(sender As Object, e As EventArgs) Handles btnCale.Click
        Try
            If _settings.CalePath.Length > 0 AndAlso File.Exists(_settings.CalePath) Then ofdCale.FileName = _settings.CalePath
            If ofdCale.ShowDialog(Me) <> DialogResult.OK Then Return
            _cale = VrcCale.Load(ofdCale.FileName)
            _settings.CalePath = ofdCale.FileName
            Dim k_changed As Integer = 0
            For k_i As Integer = 0 To Math.Min(dgvPlan.RowCount, _planFile.Count) - 1
                Dim k_row = dgvPlan.Rows(k_i)
                Dim k_profile = _profiles(_planFile(k_i))
                Dim k_current As String = Convert.ToString(k_row("subunit"), CultureInfo.InvariantCulture)
                Dim k_ownFile As Integer = _planFile(k_i)
                Dim k_siblings As Integer = _planFile.Where(Function(k_f) k_f = k_ownFile).Count()
                k_row("nota") = PlanNote(k_profile, _chosen(_planFile(k_i)), _planKey(k_i).Proposed, k_siblings) & CaleNote(_chosen(_planFile(k_i)))
                If k_current.Trim().Length > 0 AndAlso Not String.Equals(k_current.Trim(), UnitNameOf(k_profile), StringComparison.OrdinalIgnoreCase) Then Continue For   ' changed by the operator
                Dim k_proposal As String = ProposeSubunit(_chosen(_planFile(k_i)), k_profile)
                If k_proposal.Length > 0 AndAlso k_proposal <> k_current Then
                    k_row("subunit") = k_proposal
                    k_changed += 1
                End If
            Next
            dgvPlan.Invalidate()
            SaveWorkState()
            SetState(_state, $"cale.accdb încărcat: {_cale.Count} rânduri." & If(dgvPlan.RowCount > 0, $" Subunitatea s-a propus pentru {k_changed} rânduri din plan.", " Se folosește la «Citește bifatele»."))
        Catch ex As Exception
            Fail("VrcMigratorForm.Cale", "cale.accdb nu a putut fi citit.", ex)
        End Try
    End Sub

    ''' <summary>The subunit proposed for a file: Detalii of cale.accdb when the folder matches, else the unit name stored in the file.</summary>
    Private Function ProposeSubunit(k_source As VrcSourceFile, k_profile As VrcFileProfile) As String
        Dim k_fromCale As String = VrcCale.Find(_cale, k_source.Path, k_source.Kind, k_source.Year)
        Return If(k_fromCale.Length > 0, k_fromCale, UnitNameOf(k_profile))
    End Function

    ''' <summary>The files of the last run come back as they were (ticks included); a file that no longer exists is dropped and said so. They are NOT read again: «Citește bifatele» does that.</summary>
    Private Sub RestoreFiles()
        Dim k_missing As New List(Of String)()
        _files = New List(Of VrcSourceFile)()
        For Each k_saved In _settings.Files
            Dim k_file As VrcSourceFile = Nothing
            If File.Exists(k_saved.Path) Then k_file = VrcSourceFile.FromChosenPath(k_saved.Path)
            If k_file Is Nothing Then
                k_missing.Add(k_saved.Path)
            Else
                k_file.Selected = k_saved.Selected
                _files.Add(k_file)
            End If
        Next
        ShowFiles()
        If _files.Count = 0 Then
            SetState(VrcRunState.Unread, "Alegeți folderul cu bazele subunității sau adăugați baze de date.")
        Else
            Dim k_text As String = $"{_files.Count} fișiere reluate de la ultima rulare. Apăsați «Citește bifatele»."
            If k_missing.Count > 0 Then k_text &= $" Lipsesc și au fost scoase: {String.Join(", ", k_missing)}."
            SetState(VrcRunState.Unread, k_text)
        End If
    End Sub

    ''' <summary>Writes everything worth remembering to the settings file: files and ticks, DC, Subunitate, the plan rows. Called after each step and on closing.</summary>
    Private Sub SaveWorkState()
        Try
            SyncSelection()
            SyncPreparedWithDc()
            StorePrepared()
            _settings.Files = _files.Select(Function(k_f) New VrcSavedFile() With {.Path = k_f.Path, .Selected = k_f.Selected}).ToList()
            _settings.Dc = txtDc.Text.Trim()
            ' Plan rows are merged by (file, proposed type): rows of files not in the grid now stay, so a file taken out and put back keeps its choices.
            For k_i As Integer = 0 To Math.Min(dgvPlan.RowCount, _planKey.Count) - 1
                Dim k_row = dgvPlan.Rows(k_i)
                Dim k_key = _planKey(k_i)
                _settings.PlanRows.RemoveAll(Function(k_r) String.Equals(k_r.Path, k_key.SourcePath, StringComparison.OrdinalIgnoreCase) AndAlso k_r.ProposedType = k_key.Proposed)
                _settings.PlanRows.Add(New VrcSavedPlanRow() With {
                    .Path = k_key.SourcePath, .ProposedType = k_key.Proposed,
                    .Subunit = Convert.ToString(k_row("subunit"), CultureInfo.InvariantCulture),
                    .Year = Convert.ToString(k_row("an"), CultureInfo.InvariantCulture),
                    .Gestiune = Convert.ToString(k_row("gestiune"), CultureInfo.InvariantCulture),
                    .GestiuneType = Convert.ToString(k_row("tip"), CultureInfo.InvariantCulture)})
            Next
            _settings.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.SaveWorkState", ex)   ' remembering is a convenience
        End Try
    End Sub

    ''' <summary>The prepared partners of a DC come back from the settings (the same list object: the plan and the partners window share it).</summary>
    Private Sub UsePreparedOf(k_dc As String)
        Dim k_saved As List(Of VrcNewPartner) = Nothing
        _prepared.Clear()
        If _settings.PreparedPartners.TryGetValue(VrcSettings.DcKey(k_dc), k_saved) Then _prepared.AddRange(k_saved)
        _preparedDc = k_dc.Trim()
    End Sub

    ''' <summary>When the DC field changed, the list goes to its old DC and the list of the new DC is taken (a list made before any DC was known joins the first DC).</summary>
    Private Sub SyncPreparedWithDc()
        Dim k_dc As String = txtDc.Text.Trim()
        If VrcSettings.DcKey(k_dc) = VrcSettings.DcKey(_preparedDc) Then Return
        If VrcSettings.DcKey(_preparedDc).Length = 0 Then
            _preparedDc = k_dc
        Else
            StorePrepared()
            UsePreparedOf(k_dc)
        End If
    End Sub

    Private Sub StorePrepared()
        Dim k_key As String = VrcSettings.DcKey(_preparedDc)
        If k_key.Length = 0 Then Return
        If _prepared.Count > 0 Then
            _settings.PreparedPartners(k_key) = New List(Of VrcNewPartner)(_prepared)
        Else
            _settings.PreparedPartners.Remove(k_key)
        End If
    End Sub

    Private Sub LoadFolder()
        Try
            If fbdFolder.ShowDialog(Me) <> DialogResult.OK Then Return
            SaveWorkState()   ' the subunit names typed in the plan are kept before the list changes
            Dim k_ignored As New List(Of String)()
            _files = VrcSourceFile.Scan(fbdFolder.SelectedPath, k_ignored)
            _settings.LastFolder = fbdFolder.SelectedPath
            ResetAfterFileListChange()
            Dim k_text As New StringBuilder($"{_files.Count} fișiere găsite.")
            If k_ignored.Count > 0 Then k_text.Append($" Ignorate: {String.Join(", ", k_ignored)}.")
            SetState(VrcRunState.Unread, k_text.ToString())
        Catch ex As Exception
            Fail("VrcMigratorForm.Folder", "Folderul nu a putut fi citit.", ex)
        End Try
    End Sub

    ''' <summary>
    ''' «+ Adaugă Baza Date…»: the operator picks Access FILES (one or several, from any folder) and they are added to the list.
    ''' Alimente, Materiale, OI and MF usually live in different folders. A file already in the list is skipped.
    ''' </summary>
    Private Sub btnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If ofdBaze.ShowDialog(Me) <> DialogResult.OK Then Return
            SaveWorkState()   ' keep the ticks and the plan choices the operator already changed
            Dim k_known As New HashSet(Of String)(_files.Select(Function(k_f) k_f.Path), StringComparer.OrdinalIgnoreCase)
            Dim k_skipped As New List(Of String)()
            Dim k_added As Integer = 0
            For Each k_path In ofdBaze.FileNames
                If k_known.Contains(k_path) Then
                    k_skipped.Add(Path.GetFileName(k_path) & " (deja în listă)")
                    Continue For
                End If
                Dim k_file = VrcSourceFile.FromChosenPath(k_path)
                If k_file Is Nothing Then
                    k_skipped.Add(Path.GetFileName(k_path))
                    Continue For
                End If
                _files.Add(k_file)
                k_known.Add(k_path)
                k_added += 1
            Next
            If ofdBaze.FileNames.Length > 0 Then _settings.LastFolder = Path.GetDirectoryName(ofdBaze.FileNames(0))
            ResetAfterFileListChange()
            Dim k_text As New StringBuilder($"{k_added} fișiere adăugate ({_files.Count} în total).")
            If k_skipped.Count > 0 Then k_text.Append($" Ignorate: {String.Join(", ", k_skipped)}.")
            If k_added > 0 Then k_text.Append(" Apăsați «Citește bifatele» din nou.")
            SetState(VrcRunState.Unread, k_text.ToString())
        Catch ex As Exception
            Fail("VrcMigratorForm.Adauga", "Fișierele nu au putut fi adăugate.", ex)
        End Try
    End Sub

    ''' <summary>Copies the ticks of the grid back into the files, so rebuilding the grid does not undo the operator's choice.</summary>
    Private Sub SyncSelection()
        For k_i As Integer = 0 To Math.Min(dgvFisiere.RowCount, _files.Count) - 1
            _files(k_i).Selected = Convert.ToBoolean(dgvFisiere.Rows(k_i)("sel"), CultureInfo.InvariantCulture)
        Next
    End Sub

    Private Sub ShowFiles()
        dgvFisiere.BeginUpdate()
        Try
            dgvFisiere.ClearRows()
            ' The same year in .accdb and .mdb counts only inside ONE folder: another folder with the same year is another gestiune.
            Dim k_yearsSeen As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For Each k_f In _files.Where(Function(k_x) k_x.Kind = VrcFileKind.Year)
                Dim k_key As String = YearKey(k_f)
                k_yearsSeen(k_key) = If(k_yearsSeen.ContainsKey(k_key), k_yearsSeen(k_key) + 1, 1)
            Next
            For Each k_f In _files
                Dim k_row = dgvFisiere.AddRow()
                k_row("sel") = k_f.Selected
                k_row("fisier") = k_f.FileName
                k_row("folder") = Path.GetDirectoryName(k_f.Path)
                k_row("format") = k_f.Format
                k_row("rol") = RoleText(k_f)
                k_row("an") = If(k_f.Kind = VrcFileKind.Year, k_f.Year.ToString(CultureInfo.InvariantCulture), "—")
                k_row("dimensiune") = (k_f.SizeBytes / 1024.0).ToString("N0", CultureInfo.InvariantCulture) & " KB"
                k_row("nota") = If(k_f.Kind = VrcFileKind.Year AndAlso k_yearsSeen(YearKey(k_f)) > 1, "același an în două formate (.accdb și .mdb)",
                                   If(k_f.Kind = VrcFileKind.Other, "nume neobișnuit: nebifat implicit", String.Empty))
            Next
        Finally
            dgvFisiere.EndUpdate()
        End Try
        Dim k_folders = _files.Select(Function(k_f) Path.GetDirectoryName(k_f.Path)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        lblFolder.Text = If(k_folders.Count = 0, "Niciun folder ales.",
                            If(k_folders.Count = 1, k_folders(0), $"{k_folders.Count} foldere: {String.Join(" | ", k_folders)}"))
    End Sub

    Private Shared Function YearKey(k_f As VrcSourceFile) As String
        Return Path.GetDirectoryName(k_f.Path) & "|" & k_f.Year.ToString(CultureInfo.InvariantCulture)
    End Function

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

    ''' <summary>«Șterge rândul»: the file of the current row leaves the list (nothing is deleted from disk). The reading and the plan on screen belonged to the old list: they are dropped.</summary>
    Private Sub btnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            Dim k_index As Integer = dgvFisiere.CurrentRowIndex
            If k_index < 0 OrElse k_index >= _files.Count Then
                SetState(_state, "Alegeți întâi rândul de scos din listă.")
                Return
            End If
            SaveWorkState()   ' keep the ticks of the other rows and the plan choices typed so far
            Dim k_name As String = _files(k_index).FileName
            _files.RemoveAt(k_index)
            ResetAfterFileListChange()
            If dgvFisiere.RowCount > 0 Then dgvFisiere.CurrentRowIndex = Math.Min(k_index, dgvFisiere.RowCount - 1)
            SetState(VrcRunState.Unread, $"«{k_name}» a fost scos din listă ({_files.Count} fișiere rămase)." & If(_files.Count > 0, " Apăsați «Citește bifatele».", String.Empty))
        Catch ex As Exception
            Fail("VrcMigratorForm.Sterge", "Rândul nu a putut fi scos.", ex)
        End Try
    End Sub

    ''' <summary>«Golește lista»: the file list, the reading, the plan and the report on screen are dropped. DC, Subunitate, the server, the remembered plan choices and the prepared partners stay.</summary>
    Private Sub btnGoleste_Click(sender As Object, e As EventArgs) Handles btnGoleste.Click
        Try
            ' Saved first: the plan grid is emptied below, and what the operator typed in it (subunit names) must reach the settings file before.
            SaveWorkState()
            _files = New List(Of VrcSourceFile)()
            ResetAfterFileListChange()
            SetState(VrcRunState.Unread, "Lista de baze este goală. Alegeți un folder sau adăugați baze de date.")
        Catch ex As Exception
            Fail("VrcMigratorForm.Goleste", "Lista nu a putut fi golită.", ex)
        End Try
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
            SaveWorkState()   ' the plan grid is rebuilt below: what was typed in it is saved first
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
            SaveWorkState()
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

    ' ---- step 2: the plan (VC05-02) ------------------------------------------------------------------------------------------
    ''' <summary>One row per file read, with the proposal from the name and the flags. The operator changes it; nothing is approved automatically.</summary>
    Private Sub FillPlanGrid()
        dgvPlan.BeginUpdate()
        Try
            dgvPlan.ClearRows()
            _planFile.Clear()
            _planKey.Clear()
            For k_i As Integer = 0 To _profiles.Count - 1
                Dim k_profile = _profiles(k_i)
                Dim k_source = _chosen(k_i)
                Dim k_types As List(Of String) = If(k_profile.ReadError.Length > 0, New List(Of String) From {String.Empty}, VrcPlan.SuggestTypes(k_profile))
                For Each k_type In k_types
                    Dim k_row = dgvPlan.AddRow()
                    _planFile.Add(k_i)
                    _planKey.Add((k_source.Path, k_type))
                    k_row("fisier") = k_profile.FileName
                    k_row("folder") = Path.GetDirectoryName(k_source.Path)
                    k_row("subunit") = ProposeSubunit(k_source, k_profile)
                    k_row("an") = VrcPlan.SuggestYear(k_profile, k_source).ToString(CultureInfo.InvariantCulture)
                    k_row("gestiune") = VrcPlan.SuggestName(k_type)
                    k_row("tip") = k_type
                    k_row("nota") = PlanNote(k_profile, k_source, k_type, k_types.Count) & CaleNote(k_source)
                    ' What the operator typed in the same row last time wins over the proposal.
                    Dim k_remembered = _settings.PlanRows.FirstOrDefault(Function(k_r) String.Equals(k_r.Path, k_source.Path, StringComparison.OrdinalIgnoreCase) AndAlso k_r.ProposedType = k_type)
                    If k_remembered IsNot Nothing Then
                        If k_remembered.Year.Length > 0 Then k_row("an") = k_remembered.Year
                        ' A remembered subunit equal to the file's own unit name is only an old default, not a choice: cale.accdb may know better.
                        If k_remembered.Subunit.Length > 0 AndAlso Not String.Equals(k_remembered.Subunit, UnitNameOf(k_profile), StringComparison.OrdinalIgnoreCase) Then k_row("subunit") = k_remembered.Subunit
                        If k_remembered.Gestiune.Length > 0 Then k_row("gestiune") = k_remembered.Gestiune
                        If k_remembered.GestiuneType.Length > 0 Then k_row("tip") = k_remembered.GestiuneType
                    End If
                Next
            Next
        Finally
            dgvPlan.EndUpdate()
        End Try
        Dim k_fileDc = _profiles.Select(Function(k_p) If(k_p.Config.ContainsKey("UNIT.DC"), k_p.Config("UNIT.DC"), "")).Where(Function(k_d) k_d.Length > 0 AndAlso k_d <> "NULL").LastOrDefault()
        ' A value the operator already has in DC is never overwritten by a new reading (decided 18.10.2026).
        If txtDc.Text.Trim().Length = 0 Then txtDc.Text = If(k_fileDc, String.Empty)
    End Sub

    ''' <summary>The subunit proposed for a file: the unit name stored in the file itself (UNIT.NumeUnitate); empty when it has none. Per database, the operator can change it.</summary>
    Private Shared Function UnitNameOf(k_profile As VrcFileProfile) As String
        Dim k_name As String = Nothing
        If k_profile.Config.TryGetValue("UNIT.NumeUnitate", k_name) AndAlso k_name <> "NULL" AndAlso k_name.Trim().Length > 0 Then Return k_name.Trim()
        Return String.Empty
    End Function

    Private ReadOnly _planFile As New List(Of Integer)()
    ''' <summary>Parallel to the plan grid rows: the file and the type the tool proposed, the key of a remembered row.</summary>
    Private ReadOnly _planKey As New List(Of (SourcePath As String, Proposed As String))()

    ''' <summary>« | cale: …»: where the proposed subunit came from (or why cale did not match); empty while no cale is loaded.</summary>
    Private Function CaleNote(k_source As VrcSourceFile) As String
        Dim k_text As String = VrcCale.Explain(_cale, k_source.Path, k_source.Kind, k_source.Year)
        Return If(k_text.Length > 0, " | " & k_text, String.Empty)
    End Function

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
        Dim k_plan As New VrcPlan() With {.TargetDc = txtDc.Text, .NewPartners = _prepared}
        For k_i As Integer = 0 To dgvPlan.RowCount - 1
            Dim k_row = dgvPlan.Rows(k_i)
            Dim k_year As Integer
            Integer.TryParse(Convert.ToString(k_row("an"), CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_year)
            k_plan.Sources.Add(New VrcPlanSource() With {
                .Source = _chosen(_planFile(k_i)), .Profile = _profiles(_planFile(k_i)), .Year = k_year,
                .SubunitName = Convert.ToString(k_row("subunit"), CultureInfo.InvariantCulture),
                .GestiuneName = Convert.ToString(k_row("gestiune"), CultureInfo.InvariantCulture),
                .GestiuneType = Convert.ToString(k_row("tip"), CultureInfo.InvariantCulture)})
        Next
        k_plan.Evaluate()
        Return k_plan
    End Function

    Private Function PlanText(k_plan As VrcPlan) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine($"PLAN — DC {If(k_plan.Dc.Length = 0, "(necunoscut)", k_plan.Dc)}, {k_plan.SubunitNames().Count} subunități ({String.Join(", ", k_plan.SubunitNames().Select(Function(k_n) $"«{k_n}» ZEC {k_plan.ZecOf(k_n)}"))}), scară {k_plan.Scale}, hash {k_plan.Hash().Substring(0, 12)}…")
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

    ' ---- server ------------------------------------------------------------------------------------------------------------------
    ''' <summary>After the server answered: host, port and user (never the password) are the defaults of the next run.</summary>
    Private Sub RememberServer()
        Dim k_port As Integer
        If Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_port) Then
            _settings.Host = txtGazda.Text.Trim()
            _settings.Port = k_port
            _settings.User = txtUtilizator.Text.Trim()
            SaveWorkState()
        End If
    End Sub

    Private Function BuildServer() As TargetServer
        Dim k_port As Integer
        If txtGazda.Text.Trim().Length = 0 OrElse txtUtilizator.Text.Trim().Length = 0 OrElse
           Not Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_port) OrElse k_port < 1 OrElse k_port > 65535 Then
            Throw New InvalidOperationException("Completați gazda, portul (1–65535) și utilizatorul serverului.")
        End If
        Return New TargetServer(New TargetConnection(txtGazda.Text.Trim(), CUInt(k_port), txtUtilizator.Text.Trim(), txtParola.Text))
    End Function

    ''' <summary>Like ADE.Migrator: any change of the server, or the destination base drops the test; Testează must be pressed again.</summary>
    Private Sub InputsChanged(sender As Object, e As EventArgs) Handles txtGazda.TextChanged, txtPort.TextChanged, txtUtilizator.TextChanged, txtParola.TextChanged, txtDc.TextChanged
        Try
            If _busy OrElse _state <> VrcRunState.Tested Then Return
            _testedHash = String.Empty
            SetState(VrcRunState.PlanRead, "Datele serverului sau DC-ul s-au schimbat: apăsați din nou «Testează».")
        Catch ex As Exception
            GlobalErrorLog.Write("VrcMigratorForm.InputsChanged", ex)
        End Try
    End Sub

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
            RememberServer()
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
            k_sb.AppendLine("SERVER — gata: baza, tabelele VC_, subunitățile noi.")
            If _prepared.Count > 0 Then k_sb.AppendLine($"{_prepared.Count} parteneri pregătiți din ANAF vor fi adăugați în Parteneri în aceeași tranzacție cu transferul (cad odată cu el dacă transferul eșuează).")
            If k_suppliers IsNot Nothing AndAlso k_suppliers.Count > 0 Then
                k_sb.AppendLine($"AVERTIZARE: {k_suppliers.Count} coduri fiscale lipsă sau fără înregistrare în Parteneri: {String.Join(", ", k_suppliers.Take(20))}{If(k_suppliers.Count > 20, "…", "")}. Codurile fiscale disponibile se salvează direct pe NIR și factură; cele absente rămân NULL.")
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

    ' ---- step 3a bis: partners from ANAF -------------------------------------------------------------------------------------------
    Private Sub btnParteneri_Click(sender As Object, e As EventArgs) Handles btnParteneri.Click
        Try
            Dim k_plan = CurrentPlan()
            If Not k_plan.IsComplete Then
                SetState(_state, "Planul trebuie să fie complet (vezi «Verifică planul») ca să se știe baza DC și fișierele cu furnizori.")
                Return
            End If
            Dim k_server = BuildServer()
            Using k_form As New VrcPartnersForm(k_server, k_plan)
                k_form.ShowDialog(Me)
                SaveWorkState()   ' the prepared partners are remembered for the next run
                RememberServer()
                lblStare.Text = $"{k_form.PreparedCount} parteneri pregătiți din ANAF: se scriu în {k_plan.Dc} la «Migrează», în aceeași tranzacție cu transferul. Apăsați «Testează» ca să vedeți avertismentul actualizat."
            End Using
        Catch ex As Exception
            Fail("VrcMigratorForm.Parteneri", "Fereastra de parteneri nu a putut fi deschisă.", ex)
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
                $"Se face transferul UNIC al {k_plan.SubunitNames().Count} subunități ({String.Join(", ", k_plan.SubunitNames().Select(Function(k_n) $"«{k_n}»"))}) în baza {k_plan.Dc}: {k_plan.Sources.Count} fișiere, într-o singură tranzacție." & Environment.NewLine & Environment.NewLine &
                "După COMMIT subunitățile nu mai pot fi importate (nu există reimport). Continuați?",
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
            ShowReport(k_report, k_result.State)
            SaveRunFiles(k_report)
            If k_result.State = VrcRunState.CommitConfirmed Then
                _committed = True
                _prepared.Clear()   ' they are in Parteneri now
                SaveWorkState()
            End If
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
            Dim k_unitNames = k_plan.SubunitNames()
            If k_plan.Dc.Length = 0 OrElse k_unitNames.Count = 0 Then
                SetState(_state, "Pentru diagnostic trebuie să cunoaștem DC-ul (din fișiere) și subunitatea de pe fiecare rând din plan.")
                Return
            End If
            Dim k_server = BuildServer()
            _busy = True
            UpdateButtons()
            Dim k_text = Await Task.Run(Function() String.Join(Environment.NewLine & Environment.NewLine, k_unitNames.Select(Function(k_n) VrcWriter.Diagnose(k_server, k_plan.Dc, k_n))))
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
        ApplyStateLook(VrcRunState.Blocked)
        KBotMessage.Show(k_message & Environment.NewLine & k_ex.Message, "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>Closing waits for a running read or transfer: the files, the Access driver and the transaction must be released first.</summary>
    Private Sub VrcMigratorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _busy Then
            e.Cancel = True
            KBotMessage.Show("O operație este în curs. Așteptați să se termine (sau apăsați «Oprește» în timpul transferului) înainte de a închide.", "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            SaveWorkState()
        End If
    End Sub

End Class
