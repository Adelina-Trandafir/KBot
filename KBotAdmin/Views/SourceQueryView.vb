Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' The «Interogare pe SURSA» page (sliceless): the owner gives the path of a .sql file; the page reads what its statements touch, runs it on
''' AVACONT_SURSA alone (the text stays in the server's ledger <c>Interogari_Unice</c>, which the graph and the lanes of «Schema bazelor» and
''' «Evoluția schemei» read), checks every object against the real structure of SURSA, and, on confirmation, carries the structure to the units
''' with the schema sync and marks the query as run in each unit's ledger without executing it. The runner (<c>routes/one_time/runner.py</c>) and
''' the reader/checker (<c>sql_objects.py</c>) live on the server; this page only builds their command lines and shows their answers.
''' </summary>
Public Class SourceQueryView
    Implements IAdminView

    Private Shared ReadOnly BadNameChars As New Regex("[^A-Za-z0-9_.\-]", RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Private _context As AdminContext
    Private ReadOnly _log As LogSink
    Private _items As New List(Of SourceQueryItem)()
    Private _sqlText As String = String.Empty
    Private _running As Boolean
    Private _filling As Boolean
    Private _planDestructive As Integer
    Private _planHasRows As Boolean

    Public Sub New()
        InitializeComponent()
        _log = New LogSink(logBox)
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadHistoryAsync(String.Empty)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The query works on AVACONT_SURSA and on all units; the tree does not choose anything here.
    End Sub

    ' ------------------------------------------------------------------
    ' The file
    ' ------------------------------------------------------------------

    ''' <summary>Reads the file named in the box into <see cref="_sqlText"/>; False (after telling the operator) when it cannot be used.</summary>
    Private Function LoadFile() As Boolean
        Dim k_path As String = txtPath.Text.Trim().Trim(""""c)
        If k_path = String.Empty Then
            KBotMessage.Show("Scrieți sau alegeți calea unui fișier .sql.", "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        If Not File.Exists(k_path) Then
            KBotMessage.Show("Fișierul nu există: " & k_path, "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Dim k_text As String = File.ReadAllText(k_path, Encoding.UTF8)
        Dim k_problem As String = OnceQuery.CheckSize(k_text)
        If k_problem <> String.Empty Then
            KBotMessage.Show(k_problem, "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        _sqlText = k_text
        txtPath.Text = k_path
        If txtName.Text.Trim() = String.Empty Then txtName.Text = BadNameChars.Replace(Path.GetFileNameWithoutExtension(k_path), "_")
        Return True
    End Function

    Private Async Sub BtnBrowse_Click(k_sender As Object, e As EventArgs) Handles btnBrowse.Click
        Try
            Using k_dialog As New OpenFileDialog With {.Filter = "Interogări SQL (*.sql)|*.sql|Toate fișierele (*.*)|*.*", .Title = "Interogare pe SURSA"}
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                txtPath.Text = k_dialog.FileName
            End Using
            Await ReadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnBrowse_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRead_Click(k_sender As Object, e As EventArgs) Handles btnRead.Click
        Try
            Await ReadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnRead_Click", ex)
        End Try
    End Sub

    ''' <summary>«Citește»: what each statement of the file touches. Runs nothing.</summary>
    Private Async Function ReadAsync() As Task
        If _running Then Return
        Await UiRun.Guard("SourceQueryView.ReadAsync", _context,
            Async Function()
                If Not LoadFile() Then Return
                _context.Say("Se citește fișierul...")
                Dim k_b64 As String = Convert.ToBase64String(Encoding.UTF8.GetBytes(_sqlText))
                Dim k_result As SourceReadResult = Await _context.Client.CallAsync(Of SourceReadResult)("source-read", "--sql-b64", k_b64)
                FillObjects(k_result.Objects)
                Dim k_data As Integer = k_result.Objects.Where(Function(k_o) IsData(k_o)).Count()
                Dim k_structure As Integer = k_result.Objects.Count - k_data
                _log.Clear()
                _log.Add(KBotLogLevel.Info, "Citit " & Path.GetFileName(txtPath.Text) & ": " & k_result.Statements.ToString(CultureInfo.InvariantCulture) & " instrucțiuni, " &
                         k_structure.ToString(CultureInfo.InvariantCulture) & " obiecte de structură.")
                If k_data > 0 Then
                    _log.Add(KBotLogLevel.Warn, k_data.ToString(CultureInfo.InvariantCulture) & " instrucțiuni schimbă date, nu structură: sincronizarea de schemă nu le duce în unități.")
                End If
                lblState.Text = "Citit. Nu s-a rulat nimic."
                _context.Say(lblState.Text)
            End Function)
    End Function

    ' ------------------------------------------------------------------
    ' Running on SURSA
    ' ------------------------------------------------------------------

    Private Async Sub BtnView_Click(k_sender As Object, e As EventArgs) Handles btnView.Click
        Try
            Await RunSourceAsync(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnView_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            Await RunSourceAsync(False)
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnRun_Click", ex)
        End Try
    End Sub

    Private Async Function RunSourceAsync(k_view As Boolean) As Task
        If _running Then Return
        Dim k_command As String = Nothing
        Dim k_name As String = txtName.Text.Trim()
        Await UiRun.Guard("SourceQueryView.RunSourceAsync", _context,
            Function()
                If Not LoadFile() Then Return Task.CompletedTask
                k_name = txtName.Text.Trim()
                Dim k_problem As String = OnceQuery.CheckName(k_name)
                If k_problem <> String.Empty Then
                    KBotMessage.Show(k_problem, "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return Task.CompletedTask
                End If
                If Not k_view AndAlso Not UiRun.Confirm("Interogarea «" & k_name & "» se rulează pe AVACONT_SURSA, doar acolo." & vbLf &
                                                        "O bază care a rulat deja textul exact este sărită; modificările nu se pot anula." & vbLf & vbLf &
                                                        "Ați făcut întâi «Vezi (nu execută)»? Continuați?", "Interogare pe SURSA") Then
                    Return Task.CompletedTask
                End If
                k_command = OnceQuery.SourceCommand(_context.Client.Settings, k_name, _sqlText, k_view)
                Return Task.CompletedTask
            End Function)
        If k_command Is Nothing Then Return

        Dim k_exit As Integer = Await StreamAsync(k_command, "interogare pe SURSA «" & k_name & "» (" & If(k_view, "vezi", "execută") & ")")
        If k_exit < 0 Then Return
        lblState.Text = OnceQuery.Describe(k_exit, k_view)
        _log.Add(If(k_exit = 0, KBotLogLevel.Info, KBotLogLevel.Warn), lblState.Text)
        _context.Say(lblState.Text)
        If k_view OrElse k_exit <> 0 Then Return
        Await LoadHistoryAsync(k_name)
        Await CheckAsync(k_name)
        Await DumpAsync()
    End Function

    ''' <summary>
    ''' After a run: the structure of AVACONT_SURSA is dumped from the server over <c>MariaDB_Schema\AVACONT_SURSA.sql</c>, the file «Schema bazelor» reads,
    ''' so the graph shows what the query made (press «Reîncarcă» on that page).
    ''' </summary>
    Private Async Function DumpAsync() As Task
        Await UiRun.Guard("SourceQueryView.DumpAsync", _context,
            Async Function()
                Dim k_folder As String = SchemaScanner.FindFolder(AdminSettingsStore.Load().EffectiveLocalRoot())
                If k_folder.Length = 0 Then Throw New AdminException("Interogarea a rulat, dar nu găsesc folderul MariaDB_Schema lângă KBot.sln, deci dump-ul AVACONT_SURSA nu s-a refăcut.")
                _context.Say("Se face dump-ul structurii AVACONT_SURSA...")
                Dim k_dump As SourceDumpResult = Await _context.Client.CallAsync(Of SourceDumpResult)("source-dump")
                If k_dump.Tables = 0 Then Throw New AdminException("Serverul n-a întors niciun tabel din AVACONT_SURSA; dump-ul de pe disc a rămas cum era.")
                Dim k_text As String = k_dump.Text
                Dim k_path As String = Await Task.Run(Function() SourceDumpFile.Save(k_folder, k_text))
                _log.Add(KBotLogLevel.Info, "Dump AVACONT_SURSA refăcut în " & k_path & " (" & k_dump.Tables.ToString(CultureInfo.InvariantCulture) & " tabele). Cel de dinainte: " & SourceDumpFile.PreviousFileName & ".")
                If k_dump.Views.Count > 0 Then
                    _log.Add(KBotLogLevel.Warn, k_dump.Views.Count.ToString(CultureInfo.InvariantCulture) & " vederi nu sunt în dump: " & String.Join(", ", k_dump.Views))
                End If
                lblState.Text = "Gata: rulat pe SURSA, verificat, dump refăcut. «Schema bazelor» îl citește de acolo."
                _context.Say(lblState.Text)
            End Function)
    End Function

    ''' <summary>Runs a command line on the server and shows its lines; the exit status, or -1 when the command could not run.</summary>
    Private Async Function StreamAsync(k_command As String, k_title As String) As Task(Of Integer)
        Dim k_result As SshResult = Nothing
        _running = True
        SetButtons(False)
        Try
            _log.Add(KBotLogLevel.Info, "== " & k_title & " ==")
            Await UiRun.Guard("SourceQueryView.StreamAsync", _context,
                Async Function()
                    _context.Say("Se rulează: " & k_title & "...")
                    k_result = Await _context.Client.StreamAsync(k_command, AddressOf ReadLine)
                End Function)
        Finally
            _running = False
            SetButtons(True)
        End Try
        Return If(k_result Is Nothing, -1, k_result.ExitStatus)
    End Function

    ''' <summary>Runs on the SSH reader thread: shows the line and notes what the schema sync plan says.</summary>
    Private Sub ReadLine(k_line As String)
        Dim k_text As String = If(k_line, String.Empty)
        If k_text.StartsWith("[err] ", StringComparison.Ordinal) Then k_text = k_text.Substring(6)
        If k_text.Trim() = String.Empty OrElse SyncView.MissingDbLine.IsMatch(k_text) Then Return
        _log.Add(k_text)
        Dim k_match As Match = SyncView.DestructiveLine.Match(k_text)
        If k_match.Success Then _planDestructive = Integer.Parse(k_match.Groups(1).Value, CultureInfo.InvariantCulture)
        If SyncView.PlanWrittenLine.IsMatch(k_text) Then _planHasRows = True
    End Sub

    ' ------------------------------------------------------------------
    ' Checking in SURSA
    ' ------------------------------------------------------------------

    Private Async Sub BtnCheck_Click(k_sender As Object, e As EventArgs) Handles btnCheck.Click
        Try
            If _running Then Return
            Dim k_item As SourceQueryItem = SelectedItem()
            If k_item Is Nothing Then
                KBotMessage.Show("Alegeți din istoric interogarea de verificat.", "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Await CheckAsync(k_item.Name)
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnCheck_Click", ex)
        End Try
    End Sub

    ''' <summary>Asks the server whether AVACONT_SURSA really has what the query says, fills the object grid with the answer; Nothing when it could not be asked.</summary>
    Private Async Function CheckAsync(k_name As String) As Task(Of SourceCheckResult)
        Dim k_result As SourceCheckResult = Nothing
        Await UiRun.Guard("SourceQueryView.CheckAsync", _context,
            Async Function()
                _context.Say("Se verifică «" & k_name & "» în structura SURSEI...")
                k_result = Await _context.Client.CallAsync(Of SourceCheckResult)("source-check", "--name", k_name)
            End Function)
        If k_result Is Nothing Then Return Nothing

        FillChecks(k_result.Checks)
        Dim k_ok As Integer = k_result.Checks.Where(Function(k_c) String.Equals(k_c.State, "ok", StringComparison.Ordinal)).Count()
        Dim k_text As String = "Verificare «" & k_name & "» în AVACONT_SURSA: " & k_result.Checks.Count.ToString(CultureInfo.InvariantCulture) & " obiecte, " &
                               k_ok.ToString(CultureInfo.InvariantCulture) & " în regulă, " & k_result.Bad.ToString(CultureInfo.InvariantCulture) & " cu probleme, " &
                               k_result.UncheckedCount.ToString(CultureInfo.InvariantCulture) & " care nu se pot verifica."
        _log.Add(If(k_result.Bad = 0, KBotLogLevel.Info, KBotLogLevel.Warn), k_text)
        If k_result.DataStatements > 0 Then
            _log.Add(KBotLogLevel.Warn, k_result.DataStatements.ToString(CultureInfo.InvariantCulture) & " instrucțiuni de date nu se verifică (nu sunt structură).")
        End If
        lblState.Text = If(k_result.Bad = 0, "Verificat: structura SURSEI corespunde interogării.", "Verificat: " & k_result.Bad.ToString(CultureInfo.InvariantCulture) & " obiecte nu corespund. Vezi lista.")
        _context.Say(lblState.Text)
        Return k_result
    End Function

    ' ------------------------------------------------------------------
    ' Carrying the structure to the units
    ' ------------------------------------------------------------------

    Private Async Sub BtnSync_Click(k_sender As Object, e As EventArgs) Handles btnSync.Click
        Try
            Await SyncAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnSync_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The chosen query: verify it in SURSA, show the schema sync plan (nothing executed), ask, run it on all the units, then mark the query as run in
    ''' each unit's ledger. A query with data statements is synchronized for its structure only and is NOT marked, so the data part can still be run
    ''' on the units from «Interogări unice».
    ''' </summary>
    Private Async Function SyncAsync() As Task
        If _running Then Return
        Dim k_item As SourceQueryItem = SelectedItem()
        If k_item Is Nothing Then
            KBotMessage.Show("Alegeți din istoric interogarea de dus în unități.", "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If k_item.Total > 0 AndAlso k_item.Missing.Count = 0 Then
            KBotMessage.Show("Toate cele " & k_item.Total.ToString(CultureInfo.InvariantCulture) & " unități au deja interogarea «" & k_item.Name & "». Nu e nimic de sincronizat.",
                             "Interogare pe SURSA", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim k_check As SourceCheckResult = Await CheckAsync(k_item.Name)
        If k_check Is Nothing Then Return
        If k_check.Bad > 0 AndAlso Not UiRun.Confirm(
            "În AVACONT_SURSA " & k_check.Bad.ToString(CultureInfo.InvariantCulture) & " obiecte nu corespund interogării (lipsesc sau mai există)." & vbLf &
            "Sincronizarea duce în unități structura SURSEI așa cum este acum, nu cea din fișier." & vbLf & vbLf & "Continuați?", "Interogare pe SURSA") Then
            Return
        End If
        If k_item.HasData AndAlso Not UiRun.Confirm(
            "«" & k_item.Name & "» are și instrucțiuni de date (INSERT, UPDATE...)." & vbLf &
            "Sincronizarea duce în unități doar structura. Interogarea NU va fi marcată ca rulată pe unități; partea de date o rulați pe ele din «Interogări unice»." & vbLf & vbLf &
            "Continuați cu structura?", "Interogare pe SURSA") Then
            Return
        End If

        ' 1. The plan: nothing is executed. The lines stay in the log, so the confirmation below is about what was seen.
        _planDestructive = 0
        _planHasRows = False
        Dim k_planExit As Integer = Await StreamAsync(_context.Client.SyncCommand("FORCE", String.Empty, True, False), "plan de sincronizare (nu se execută nimic)")
        If k_planExit < 0 Then Return
        If k_planExit <> 0 Then
            lblState.Text = "Planul nu s-a putut calcula (cod " & k_planExit.ToString(CultureInfo.InvariantCulture) & "). Nu s-a schimbat nimic."
            _log.Add(KBotLogLevel.Error, lblState.Text)
            _context.Say(lblState.Text)
            Return
        End If

        ' 2. The confirmation (and a second one when the plan deletes or changes data).
        If _planHasRows Then
            If Not UiRun.Confirm("Planul de mai sus se execută pe TOATE bazele de unitate, pe serverul real." & vbLf & "Continuați?", "Sincronizare") Then
                lblState.Text = "Oprit: sincronizarea nu a fost confirmată."
                _context.Say(lblState.Text)
                Return
            End If
            Dim k_destructive As Boolean = _planDestructive > 0
            If k_destructive AndAlso Not UiRun.Confirm(
                "Planul conține " & _planDestructive.ToString(CultureInfo.InvariantCulture) & " operații DISTRUCTIVE: pot șterge sau modifica coloane, indexuri și date." & vbLf &
                "Serverul face întâi o copie a bazei, dar restaurarea e treaba dumneavoastră." & vbLf & vbLf & "Continuați?", "Sincronizare distructivă") Then
                _log.Add(KBotLogLevel.Warn, "Oprit: operațiile distructive nu au fost permise; nu s-a executat nimic.")
                lblState.Text = "Oprit: operațiile distructive nu au fost permise."
                _context.Say(lblState.Text)
                Return
            End If

            ' 3. The run.
            Dim k_runExit As Integer = Await StreamAsync(_context.Client.SyncCommand("FORCE", String.Empty, False, k_destructive), "sincronizare pe toate unitățile" & If(k_destructive, " (cu operații distructive)", String.Empty))
            If k_runExit < 0 Then Return
            If k_runExit <> 2 Then _context.RequestRefresh()
            If k_runExit <> 0 Then
                lblState.Text = "Sincronizarea s-a încheiat cu codul " & k_runExit.ToString(CultureInfo.InvariantCulture) & "; interogarea NU a fost marcată pe unități."
                _log.Add(KBotLogLevel.Error, lblState.Text)
                _context.Say(lblState.Text)
                Return
            End If
        Else
            If k_item.HasData Then
                lblState.Text = "Unitățile au deja structura. Interogarea are date: nu o marchez; rulați partea de date din «Interogări unice»."
                _log.Add(KBotLogLevel.Warn, lblState.Text)
                _context.Say(lblState.Text)
                Return
            End If
            If Not UiRun.Confirm("Planul e gol: unitățile au deja structura." & vbLf & "Marchez interogarea «" & k_item.Name & "» ca rulată pe unități, fără s-o execut?", "Interogare pe SURSA") Then Return
        End If

        ' 4. The mark: the structure came from the sync, so the units must not run the text again.
        If k_item.HasData Then
            lblState.Text = "Structura a ajuns în unități. Interogarea are date: nu o marchez; rulați partea de date din «Interogări unice»."
            _log.Add(KBotLogLevel.Warn, lblState.Text)
            _context.Say(lblState.Text)
            Await LoadHistoryAsync(k_item.Name)
            Return
        End If
        Dim k_markExit As Integer = Await StreamAsync(OnceQuery.MarkCommand(_context.Client.Settings, k_item.Name), "marcare «" & k_item.Name & "» pe unități (nu se execută)")
        If k_markExit < 0 Then Return
        lblState.Text = If(k_markExit = 0, "Gata: structura e în unități și interogarea «" & k_item.Name & "» e marcată ca rulată pe ele.",
                           "Structura a ajuns în unități, dar marcarea a avut erori (cod " & k_markExit.ToString(CultureInfo.InvariantCulture) & "). Citiți rezultatul.")
        _log.Add(If(k_markExit = 0, KBotLogLevel.Info, KBotLogLevel.Warn), lblState.Text)
        _context.Say(lblState.Text)
        Await LoadHistoryAsync(k_item.Name)
    End Function

    ' ------------------------------------------------------------------
    ' History
    ' ------------------------------------------------------------------

    Private Async Sub BtnReload_Click(k_sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            Dim k_item As SourceQueryItem = SelectedItem()
            Await LoadHistoryAsync(If(k_item Is Nothing, String.Empty, k_item.Name))
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.BtnReload_Click", ex)
        End Try
    End Sub

    ''' <summary>The queries that ran on AVACONT_SURSA, from its ledger; <paramref name="k_select"/> is the one to choose afterwards.</summary>
    Private Async Function LoadHistoryAsync(k_select As String) As Task
        Await UiRun.Guard("SourceQueryView.LoadHistoryAsync", _context,
            Async Function()
                _context.Say("Se citește istoricul interogărilor de pe SURSA...")
                Dim k_result As SourceQueriesResult = Await _context.Client.CallAsync(Of SourceQueriesResult)("source-queries")
                _items = k_result.Items
                _filling = True
                Try
                    gridHist.BeginUpdate()
                    Try
                        gridHist.ClearRows()
                        For Each k_item As SourceQueryItem In _items
                            Dim k_row As KBotDataRow = gridHist.AddRow()
                            k_row.Tag = k_item
                            Dim k_data As Integer = k_item.Objects.Where(Function(k_o) IsData(k_o)).Count()
                            k_row("name") = k_item.Name
                            k_row("ranat") = k_item.RanAt.Replace("T", " ")
                            k_row("objects") = (k_item.Objects.Count - k_data).ToString(CultureInfo.InvariantCulture) & " / " & k_data.ToString(CultureInfo.InvariantCulture)
                            k_row("done") = k_item.Done.ToString(CultureInfo.InvariantCulture) & "/" & k_item.Total.ToString(CultureInfo.InvariantCulture)
                            k_row("missing") = If(k_item.Missing.Count = 0, "-", String.Join(", ", k_item.Missing))
                        Next
                    Finally
                        gridHist.EndUpdate()
                    End Try
                    For k_i As Integer = 0 To _items.Count - 1
                        If String.Equals(_items(k_i).Name, k_select, StringComparison.Ordinal) Then gridHist.CurrentRowIndex = k_i
                    Next
                Finally
                    _filling = False
                End Try
                _context.Say(_items.Count.ToString(CultureInfo.InvariantCulture) & " interogări rulate pe SURSA. Dublu clic pe un rând arată textul.")
            End Function)
    End Function

    Private Function SelectedItem() As SourceQueryItem
        Dim k_row As KBotDataRow = gridHist.CurrentRow
        If k_row Is Nothing Then Return Nothing
        Return TryCast(k_row.Tag, SourceQueryItem)
    End Function

    ''' <summary>Choosing a query shows what it touches (without the state, which «Verifică» adds).</summary>
    Private Sub GridHist_SelectionChanged(k_sender As Object, e As EventArgs) Handles gridHist.SelectionChanged
        Try
            If _filling Then Return
            Dim k_item As SourceQueryItem = SelectedItem()
            If k_item IsNot Nothing Then FillObjects(k_item.Objects)
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.GridHist_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridHist_CellDoubleClick(k_sender As Object, e As KBotCellEventArgs) Handles gridHist.CellDoubleClick
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= gridHist.Rows.Count Then Return
            Dim k_item As SourceQueryItem = TryCast(gridHist.Rows(e.RowIndex).Tag, SourceQueryItem)
            If k_item Is Nothing Then Return
            Dim k_size As Long = Encoding.UTF8.GetByteCount(k_item.Text)
            Using k_form As New SqlTextForm(k_item.Name, k_size, k_item.Text, k_size, False)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SourceQueryView.GridHist_CellDoubleClick", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The object grid
    ' ------------------------------------------------------------------

    Private Sub FillObjects(k_objects As IEnumerable(Of SourceObject))
        gridObj.BeginUpdate()
        Try
            gridObj.ClearRows()
            For Each k_object As SourceObject In k_objects
                Dim k_row As KBotDataRow = gridObj.AddRow()
                k_row("stmt") = k_object.Stmt.ToString(CultureInfo.InvariantCulture)
                k_row("action") = ActionText(k_object)
                k_row("kind") = KindText(k_object.Kind)
                k_row("table") = k_object.Table
                k_row("name") = If(k_object.Old.Length > 0, k_object.Old & " → " & k_object.Name, k_object.Name)
                k_row("state") = "-"
            Next
        Finally
            gridObj.EndUpdate()
        End Try
    End Sub

    Private Sub FillChecks(k_checks As IEnumerable(Of SourceCheck))
        gridObj.BeginUpdate()
        Try
            gridObj.ClearRows()
            For Each k_check As SourceCheck In k_checks
                Dim k_row As KBotDataRow = gridObj.AddRow()
                k_row("stmt") = k_check.Stmt.ToString(CultureInfo.InvariantCulture)
                k_row("action") = If(String.Equals(k_check.Expect, "present", StringComparison.Ordinal), "Trebuie să existe", "Trebuie să lipsească")
                k_row("kind") = KindText(k_check.Kind)
                k_row("table") = k_check.Table
                k_row("name") = If(k_check.Name.Length = 0, "-", k_check.Name)
                k_row("state") = StateText(k_check)
            Next
        Finally
            gridObj.EndUpdate()
        End Try
    End Sub

    Private Shared Function IsData(k_object As SourceObject) As Boolean
        Return String.Equals(k_object.Kind, "DATA", StringComparison.Ordinal)
    End Function

    Private Shared Function KindText(k_kind As String) As String
        Select Case k_kind
            Case "TABLE" : Return "Tabel"
            Case "COLUMN" : Return "Coloană"
            Case "INDEX" : Return "Index"
            Case "PK" : Return "Cheie primară"
            Case "FK" : Return "Cheie străină"
            Case "DATA" : Return "Date"
            Case Else : Return k_kind
        End Select
    End Function

    Private Shared Function ActionText(k_object As SourceObject) As String
        If IsData(k_object) Then Return k_object.Action
        Select Case k_object.Action
            Case "CREATE" : Return "Creează"
            Case "ADD" : Return "Adaugă"
            Case "ALTER", "MODIFY" : Return "Modifică"
            Case "DROP" : Return "Șterge"
            Case "RENAME" : Return "Redenumește"
            Case Else : Return k_object.Action
        End Select
    End Function

    Private Shared Function StateText(k_check As SourceCheck) As String
        Select Case k_check.State
            Case "ok" : Return If(String.Equals(k_check.Expect, "present", StringComparison.Ordinal), "Există", "Nu mai există")
            Case "missing" : Return "LIPSEȘTE"
            Case "still_there" : Return "MAI EXISTĂ"
            Case Else : Return "Nu se poate verifica"
        End Select
    End Function

    Private Sub SetButtons(k_on As Boolean)
        btnBrowse.Enabled = k_on
        btnRead.Enabled = k_on
        btnView.Enabled = k_on
        btnRun.Enabled = k_on
        btnCheck.Enabled = k_on
        btnSync.Enabled = k_on
        btnReload.Enabled = k_on
    End Sub

End Class
