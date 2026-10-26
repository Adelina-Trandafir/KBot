Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Threading

''' <summary>
''' The «Copii locale» page (slice ME02): the encrypted copies of the databases on this PC. New copy (with a reason), check, SQL export
''' of a copy, the history of one row across copies, clean-up by retention, and the daily Windows task. The server is only read.
''' </summary>
Public Class LocalBackupView
    Implements IAdminView

    Private Const DemoDatabase As String = "000_DEMO"

    Private _context As AdminContext
    Private _all As New List(Of SnapshotManifest)()          ' every copy that opens, oldest first
    Private _shown As New List(Of SnapshotManifest)()        ' the rows of gridSnaps, newest first

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await ReloadAsync(False)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The database chosen in the tree filters the list of copies.
        Try
            ShowSnapshots()
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.SelectionChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Helpers
    ' ------------------------------------------------------------------

    ''' <summary>Writes to the status line from any thread.</summary>
    Private Sub Say(k_text As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        BeginInvoke(New Action(Sub() lblState.Text = k_text))
    End Sub

    Private Function Settings() As AdminSettings
        Return _context.Client.Settings
    End Function

    ''' <summary>The store when the saved key opens it without asking anything; Nothing otherwise.</summary>
    Private Function OpenSilently() As BackupStore
        If _context.LocalStore IsNot Nothing Then Return _context.LocalStore
        Dim k_root As String = Settings().BackupRoot
        If Not BackupStore.Exists(k_root) Then Return Nothing
        Dim k_keys As BackupKeys = BackupKeyring.TryLoad(BackupStore.ReadStoreId(k_root))
        If k_keys Is Nothing Then Return Nothing
        Try
            _context.LocalStore = BackupStore.OpenWithKeys(k_root, k_keys)
        Catch ex As BackupException
            GlobalErrorLog.Write("LocalBackupView.OpenSilently", ex)
            Return Nothing
        End Try
        Return _context.LocalStore
    End Function

    Private Function CopyableDatabases() As List(Of String)
        Dim k_list As New List(Of String)()
        For Each k_info As DatabaseInfo In _context.Databases
            If k_info.Exists AndAlso Not String.Equals(k_info.Db, DemoDatabase, StringComparison.OrdinalIgnoreCase) Then k_list.Add(k_info.Db)
        Next
        Return k_list
    End Function

    Private Function Mb(k_bytes As Long) As String
        Return UiRun.FormatBytes(k_bytes)
    End Function

    ' ------------------------------------------------------------------
    ' Loading and showing
    ' ------------------------------------------------------------------

    Private Async Function ReloadAsync(k_mayAsk As Boolean) As Task
        Await UiRun.Guard("LocalBackupView.ReloadAsync", _context,
            Async Function()
                Dim k_root As String = Settings().BackupRoot
                If String.IsNullOrWhiteSpace(k_root) Then
                    ShowEmpty("Nu e ales încă dosarul copiilor locale. Apăsați «Dosar...».")
                    Return
                End If
                If Not BackupStore.Exists(k_root) Then
                    ShowEmpty("În " & k_root & " nu există încă o copie locală. Prima «Salvează acum» o creează (cere o parolă nouă).")
                    Return
                End If
                Dim k_store As BackupStore = If(k_mayAsk, BackupAccess.EnsureStore(_context, Me), OpenSilently())
                If k_store Is Nothing Then
                    ShowEmpty("Copiile din " & k_root & " sunt criptate. Apăsați «Deschide» și scrieți parola (sau bifați păstrarea cheii pe acest calculator).")
                    Return
                End If
                _context.Say("Se citește lista copiilor locale...")
                Dim k_listing As BackupSnapshotListing = Await Task.Run(Function() k_store.ListSnapshots())
                Dim k_bytes As Long = Await Task.Run(Function() k_store.TotalBytes())
                _all = k_listing.Items
                ShowSnapshots()
                Dim k_text As String = "Dosar: " & k_root & "  ·  " & _all.Count.ToString(CultureInfo.InvariantCulture) & " copii  ·  " & Mb(k_bytes) & " pe disc"
                If k_listing.Problems.Count > 0 Then k_text &= "  ·  ATENȚIE: " & k_listing.Problems.Count.ToString(CultureInfo.InvariantCulture) & " copii nu se pot deschide (rulați «Verifică»)."
                lblState.Text = k_text
                _context.Say("Gata.")
            End Function)
    End Function

    Private Sub ShowEmpty(k_text As String)
        _all = New List(Of SnapshotManifest)()
        gridSnaps.ClearRows()
        gridTables.ClearRows()
        lblState.Text = k_text
        _context.Say("Gata.")
    End Sub

    Private Sub ShowSnapshots()
        _shown = _all.Where(Function(k_m) _context.IsAll OrElse String.Equals(k_m.Db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase)).
                      OrderByDescending(Function(k_m) k_m.FinishedAt).ToList()
        gridSnaps.BeginUpdate()
        Try
            gridSnaps.ClearRows()
            For Each k_item As SnapshotManifest In _shown
                Dim k_row As KBotDataRow = gridSnaps.AddRow()
                k_row.Tag = k_item
                k_row("db") = k_item.Db
                k_row("when") = k_item.FinishedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)
                k_row("kind") = k_item.Kind
                k_row("mode") = k_item.Mode
                k_row("reason") = k_item.Reason
                k_row("tables") = k_item.Tables.Count.ToString(CultureInfo.InvariantCulture)
                k_row("pagesNew") = k_item.Stats.PagesNew.ToString(CultureInfo.InvariantCulture)
                k_row("pagesSame") = k_item.Stats.PagesSame.ToString(CultureInfo.InvariantCulture)
                k_row("rows") = k_item.Stats.Rows.ToString("N0", CultureInfo.CurrentCulture)
                k_row("secs") = k_item.Stats.Seconds.ToString("0.#", CultureInfo.CurrentCulture)
            Next
        Finally
            gridSnaps.EndUpdate()
        End Try
        If _shown.Count > 0 Then gridSnaps.CurrentRowIndex = 0
        ShowTables()
    End Sub

    Private Function SelectedSnapshot() As SnapshotManifest
        Dim k_row As KBotDataRow = gridSnaps.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, SnapshotManifest))
    End Function

    Private Function PreviousOf(k_item As SnapshotManifest) As SnapshotManifest
        Dim k_before As SnapshotManifest = Nothing
        For Each k_other As SnapshotManifest In _all
            If k_other.Id = k_item.Id Then Return k_before
            If String.Equals(k_other.Db, k_item.Db, StringComparison.OrdinalIgnoreCase) Then k_before = k_other
        Next
        Return Nothing
    End Function

    Private Sub ShowTables()
        gridTables.BeginUpdate()
        Try
            gridTables.ClearRows()
            Dim k_item As SnapshotManifest = SelectedSnapshot()
            If k_item Is Nothing Then Return
            Dim k_previous As SnapshotManifest = PreviousOf(k_item)
            For Each k_table As BackupTable In k_item.Tables
                Dim k_changed As Integer = k_table.Pages.Count
                If k_previous IsNot Nothing Then
                    Dim k_old As BackupTable = BackupReader.FindTable(k_previous, k_table.Name)
                    If k_old IsNot Nothing Then
                        Dim k_names As New HashSet(Of String)(k_old.Pages.Select(Function(k_p) k_p.ObjectName), StringComparer.Ordinal)
                        k_changed = k_table.Pages.Where(Function(k_p) Not k_names.Contains(k_p.ObjectName)).Count()
                    End If
                End If
                Dim k_row As KBotDataRow = gridTables.AddRow()
                k_row.Tag = k_table
                k_row("table") = k_table.Name
                k_row("rows") = k_table.Rows.ToString("N0", CultureInfo.CurrentCulture)
                k_row("pages") = k_table.Pages.Count.ToString(CultureInfo.InvariantCulture)
                k_row("changed") = k_changed.ToString(CultureInfo.InvariantCulture)
            Next
        Finally
            gridTables.EndUpdate()
        End Try
    End Sub

    Private Sub GridSnaps_SelectionChanged(k_sender As Object, e As EventArgs) Handles gridSnaps.SelectionChanged
        Try
            ShowTables()
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.GridSnaps_SelectionChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Buttons
    ' ------------------------------------------------------------------

    Private Async Sub BtnFolder_Click(k_sender As Object, e As EventArgs) Handles btnFolder.Click
        Try
            If Not BackupAccess.ChooseFolder(_context, Me) Then Return
            Await ReloadAsync(False)
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnFolder_Click", ex)
            KBotMessage.Show(ex.Message, "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Async Sub BtnOpen_Click(k_sender As Object, e As EventArgs) Handles btnOpen.Click
        Try
            Await ReloadAsync(True)
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnOpen_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            Await RunNowAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnRun_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The same as pressing «Salvează acum» (the main window calls it from the reminder shown at start-up). Without a fixed reason the reason is asked and required. With <paramref name="k_fixedReason"/>
    ''' the reason is not asked: that text is used for every database.
    ''' </summary>
    Public Async Function RunNowAsync(Optional k_fixedReason As String = Nothing) As Task
        Try
            Dim k_dbs As List(Of String) = If(_context.IsAll, CopyableDatabases(), New List(Of String) From {_context.SelectedDb})
            If k_dbs.Count = 0 Then
                KBotMessage.Show("Nu există nicio bază de unitate de copiat.", "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            Dim k_prompt As String = If(_context.IsAll,
                "Citesc de pe server cele " & k_dbs.Count.ToString(CultureInfo.InvariantCulture) & " baze de unitate (fără " & DemoDatabase & ") și scriu în copiile locale doar ce s-a schimbat. Scrieți motivul copiei, același pentru toate:",
                "Citesc de pe server baza " & k_dbs(0) & " și scriu în copiile locale doar ce s-a schimbat. Scrieți motivul copiei:")
            Dim k_reason As String = String.Empty
            If k_fixedReason IsNot Nothing Then
                k_reason = k_fixedReason
            Else
                Using k_form As New ReasonForm(k_prompt, "Copie locală", "Fă copia",
                                               "Pornește copia locală acum. Motivul se păstrează în copie, lângă dată.",
                                               "Între 5 și 2000 de caractere. Apare în coloana Motiv.",
                                               "Scrieți motivul copiei (cel puțin 5 caractere).")
                    If k_form.ShowDialog(Me) <> DialogResult.OK Then Return
                    k_reason = k_form.Reason
                End Using
            End If
            Dim k_results As List(Of BackupRunResult) = Nothing
            Await UiRun.Guard("LocalBackupView.BtnRun_Click", _context,
                Async Function()
                    k_results = Await BackupRunner.RunManyAsync(_context.Client, k_store, k_dbs, "manual", k_reason, "fast", AddressOf Say, CancellationToken.None)
                End Function)
            Await ReloadAsync(False)
            If k_results IsNot Nothing Then ReportRun(k_results)
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.RunNowAsync", ex)
        End Try
    End Function

    Private Sub ReportRun(k_results As List(Of BackupRunResult))
        Try
            Dim k_failed As List(Of BackupRunResult) = k_results.Where(Function(k_r) Not k_r.Ok).ToList()
            Dim k_done As Integer = k_results.Count - k_failed.Count
            Dim k_new As Long = k_results.Where(Function(k_r) k_r.Ok).Sum(Function(k_r) k_r.Summary.BytesNew)
            _context.Say("Copii locale făcute: " & k_done.ToString(CultureInfo.InvariantCulture) & " din " & k_results.Count.ToString(CultureInfo.InvariantCulture) & ", " & Mb(k_new) & " noi în dosar.")
            If k_failed.Count = 0 Then Return
            KBotMessage.Show("Copii făcute: " & k_done.ToString(CultureInfo.InvariantCulture) & " din " & k_results.Count.ToString(CultureInfo.InvariantCulture) & "." & vbLf & vbLf &
                             String.Join(vbLf, k_failed.Select(Function(k_r) k_r.Db & ": " & k_r.Error)),
                             "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.ReportRun", ex)
        End Try
    End Sub

    Private Async Sub BtnVerify_Click(k_sender As Object, e As EventArgs) Handles btnVerify.Click
        Try
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            Dim k_report As BackupVerifyReport = Nothing
            Await UiRun.Guard("LocalBackupView.BtnVerify_Click", _context,
                Async Function()
                    k_report = Await Task.Run(Function() k_store.Verify(AddressOf Say))
                End Function)
            If k_report Is Nothing Then Return
            Dim k_text As String = "Fișiere verificate: " & k_report.Objects.ToString(CultureInfo.InvariantCulture) & " (" & Mb(k_report.Bytes) & "), copii: " & k_report.Snapshots.ToString(CultureInfo.InvariantCulture) & "." & vbLf &
                                   "Stricate: " & k_report.Damaged.Count.ToString(CultureInfo.InvariantCulture) & "  ·  Lipsă: " & k_report.Missing.Count.ToString(CultureInfo.InvariantCulture) &
                                   "  ·  Fără stăpân (se șterg la «Curăță»): " & k_report.Orphans.ToString(CultureInfo.InvariantCulture)
            If Not k_report.IsClean Then
                Dim k_all As New List(Of String)(k_report.Damaged)
                k_all.AddRange(k_report.Missing.Select(Function(k_n) "Lipsește fișierul " & k_n & "..."))
                k_text &= vbLf & vbLf & String.Join(vbLf, k_all.Take(12))
                If k_all.Count > 12 Then k_text &= vbLf & "... și încă " & (k_all.Count - 12).ToString(CultureInfo.InvariantCulture)
            End If
            _context.Say(If(k_report.IsClean, "Copiile locale sunt întregi.", "Copiile locale au probleme."))
            KBotMessage.Show(k_text, "Verificarea copiilor locale", MessageBoxButtons.OK, If(k_report.IsClean, MessageBoxIcon.Information, MessageBoxIcon.Warning))
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnVerify_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnExport_Click(k_sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            Dim k_item As SnapshotManifest = SelectedSnapshot()
            If k_item Is Nothing Then
                KBotMessage.Show("Alegeți mai întâi o copie din listă.", "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            Dim k_target As String = BackupSql.DefaultTarget(k_item)
            Dim k_path As String
            Using k_dialog As New SaveFileDialog()
                k_dialog.Title = "Exportă copia ca SQL"
                k_dialog.Filter = "SQL comprimat (*.sql.gz)|*.sql.gz|SQL (*.sql)|*.sql"
                k_dialog.FileName = k_target & ".sql.gz"
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                k_path = k_dialog.FileName
            End Using
            If Not UiRun.Confirm("Scriu copia din " & k_item.FinishedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) & " a bazei " & k_item.Db &
                                 " ca fișier SQL." & vbLf & "Fișierul creează baza de probă " & k_target & " și nu atinge baza vie. Poate dura câteva minute.", "Export SQL") Then Return
            Dim k_rows As Long = 0
            Await UiRun.Guard("LocalBackupView.BtnExport_Click", _context,
                Async Function()
                    k_rows = Await Task.Run(Function() BackupSql.Export(k_store, k_item, k_path, k_target, Nothing, AddressOf Say))
                End Function)
            Dim k_size As Long = If(File.Exists(k_path), New FileInfo(k_path).Length, 0L)
            lblState.Text = "Export gata: " & k_rows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri, " & Mb(k_size) & " în " & k_path
            _context.Say("Gata.")
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnExport_Click", ex)
        End Try
    End Sub

    Private Sub BtnHistory_Click(k_sender As Object, e As EventArgs) Handles btnHistory.Click
        Try
            Dim k_item As SnapshotManifest = SelectedSnapshot()
            Dim k_row As KBotDataRow = gridTables.CurrentRow
            Dim k_table As BackupTable = If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, BackupTable))
            If k_item Is Nothing OrElse k_table Is Nothing Then
                KBotMessage.Show("Alegeți o copie din listă și, în dreapta, tabela al cărei rând vreți să-l urmăriți.", "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            Dim k_ofDb As List(Of SnapshotManifest) = _all.Where(Function(k_m) String.Equals(k_m.Db, k_item.Db, StringComparison.OrdinalIgnoreCase)).ToList()
            Using k_form As New RecordHistoryForm(k_store, k_ofDb, k_table.Name, String.Empty)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnHistory_Click", ex)
            KBotMessage.Show(ex.Message, "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Async Sub BtnPurge_Click(k_sender As Object, e As EventArgs) Handles btnPurge.Click
        Try
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            Dim k_cfg As AdminSettings = Settings()
            Dim k_plan As BackupRetentionPlan = Nothing
            Await UiRun.Guard("LocalBackupView.BtnPurge_Click.Plan", _context,
                Async Function()
                    k_plan = Await Task.Run(Function() BackupRetention.Plan(k_store, k_cfg.BackupKeepDaily, k_cfg.BackupKeepWeekly, k_cfg.BackupKeepMonthly, DateTime.Now))
                End Function)
            If k_plan Is Nothing Then Return
            If k_plan.Remove.Count = 0 AndAlso k_plan.Garbage.Count = 0 Then
                KBotMessage.Show("Nu e nimic de curățat: toate copiile intră în regulile de păstrare.", "Curăță copiile", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Se șterg " & k_plan.Remove.Count.ToString(CultureInfo.InvariantCulture) & " copii vechi și " & k_plan.Garbage.Count.ToString(CultureInfo.InvariantCulture) &
                                 " fișiere la care nu mai trimite nimic (în total cam " & Mb(k_plan.FreedBytes) & ")." & vbLf &
                                 "Rămân: ultimele " & k_cfg.BackupKeepDaily.ToString(CultureInfo.InvariantCulture) & " zile, " & k_cfg.BackupKeepWeekly.ToString(CultureInfo.InvariantCulture) & " săptămâni, " &
                                 k_cfg.BackupKeepMonthly.ToString(CultureInfo.InvariantCulture) & " luni, ultima copie a fiecărei baze și TOATE copiile făcute de mână (acelea se șterg doar cu «Șterge»)." & vbLf & "Continui?", "Curăță copiile") Then Return
            Dim k_freed As Long = 0
            Await UiRun.Guard("LocalBackupView.BtnPurge_Click.Apply", _context,
                Async Function()
                    k_freed = Await Task.Run(Function() BackupRetention.Apply(k_store, k_plan, AddressOf Say))
                End Function)
            Await ReloadAsync(False)
            _context.Say("Curățare gata: " & Mb(k_freed) & " eliberați.")
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnPurge_Click", ex)
        End Try
    End Sub

    ''' <summary>Removes the chosen copy. This is the ONLY way a manual copy ever disappears; the retention never touches it.</summary>
    Private Async Sub BtnDelete_Click(k_sender As Object, e As EventArgs) Handles btnDelete.Click
        Try
            Dim k_item As SnapshotManifest = SelectedSnapshot()
            If k_item Is Nothing Then
                KBotMessage.Show("Alegeți mai întâi o copie din listă.", "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_store As BackupStore = BackupAccess.EnsureStore(_context, Me)
            If k_store Is Nothing Then Return
            If Not UiRun.Confirm("Șterg definitiv copia bazei " & k_item.Db & " din " & k_item.FinishedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) &
                                 If(k_item.Reason = String.Empty, String.Empty, " (motiv: " & k_item.Reason & ")") & "?" & vbLf &
                                 "Nu se mai poate readuce. Celelalte copii rămân întregi.", "Șterge copia") Then Return
            Dim k_freed As Long = 0
            Await UiRun.Guard("LocalBackupView.BtnDelete_Click", _context,
                Async Function()
                    k_freed = Await Task.Run(Function() BackupRetention.DeleteSnapshot(k_store, k_item.Id))
                End Function)
            Await ReloadAsync(False)
            _context.Say("Copia a fost ștearsă; " & Mb(k_freed) & " eliberați.")
        Catch ex As Exception
            GlobalErrorLog.Write("LocalBackupView.BtnDelete_Click", ex)
        End Try
    End Sub

End Class
