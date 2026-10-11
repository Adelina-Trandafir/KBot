Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' The one-time queries of AvacontPush, inside KBotAdmin (slice ME01): ONE query text run once on AVACONT_SURSA
''' and on every ticked database, remembered in the ledger table of each database so it never runs twice. The
''' runner (<c>routes/one_time/runner.py</c>) lives on the server; this page only builds its command line.
''' </summary>
Public Class OnceQueryView
    Implements IAdminView

    Private _context As AdminContext
    Private ReadOnly _log As LogSink
    Private ReadOnly _ticks As New Dictionary(Of String, Boolean)(StringComparer.OrdinalIgnoreCase)
    Private _running As Boolean

    Public Sub New()
        InitializeComponent()
        _log = New LogSink(logBox)
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        FillDatabases()
        Await LoadHistoryAsync()
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The databases to run on are ticked here, not taken from the tree.
    End Sub

    Private Sub FillDatabases()
        ' Keep the ticks the operator made; a database seen for the first time comes ticked.
        For k_i As Integer = 0 To gridDb.Rows.Count - 1
            _ticks(Convert.ToString(gridDb("db", k_i), CultureInfo.InvariantCulture)) = Convert.ToBoolean(gridDb("use", k_i), CultureInfo.InvariantCulture)
        Next
        gridDb.BeginUpdate()
        Try
            gridDb.ClearRows()
            For Each k_db As DatabaseInfo In _context.Databases
                Dim k_row As KBotDataRow = gridDb.AddRow()
                k_row("use") = _ticks.GetValueOrDefault(k_db.Db, True)
                k_row("db") = k_db.Db
                k_row("name") = k_db.Name
            Next
        Finally
            gridDb.EndUpdate()
        End Try
    End Sub

    Private Function TickedDatabases() As List(Of String)
        Dim k_list As New List(Of String)()
        For k_i As Integer = 0 To gridDb.Rows.Count - 1
            If Convert.ToBoolean(gridDb("use", k_i), CultureInfo.InvariantCulture) Then k_list.Add(Convert.ToString(gridDb("db", k_i), CultureInfo.InvariantCulture))
        Next
        Return k_list
    End Function

    Private Sub BtnAllDb_Click(k_sender As Object, e As EventArgs) Handles btnAllDb.Click
        SetAll(True)
    End Sub

    Private Sub BtnNoDb_Click(k_sender As Object, e As EventArgs) Handles btnNoDb.Click
        SetAll(False)
    End Sub

    Private Sub SetAll(k_value As Boolean)
        Try
            For k_i As Integer = 0 To gridDb.Rows.Count - 1
                gridDb("use", k_i) = k_value
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.SetAll", ex)
        End Try
    End Sub

    Private Sub BtnFile_Click(k_sender As Object, e As EventArgs) Handles btnFile.Click
        Try
            Using k_dialog As New OpenFileDialog With {.Filter = "Interogări SQL (*.sql)|*.sql|Toate fișierele (*.*)|*.*", .Title = "Interogare unică"}
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                txtSql.Text = File.ReadAllText(k_dialog.FileName, Encoding.UTF8)
                If txtName.Text.Trim() = String.Empty Then txtName.Text = Path.GetFileNameWithoutExtension(k_dialog.FileName)
                lblState.Text = "Încărcat: " & Path.GetFileName(k_dialog.FileName) & "."
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.BtnFile_Click", ex)
            KBotMessage.Show("Fișierul nu a putut fi citit: " & ex.Message, "Interogare unică", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub BtnView_Click(k_sender As Object, e As EventArgs) Handles btnView.Click
        Try
            Await RunOnce(True)
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.BtnView_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            Await RunOnce(False)
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.BtnRun_Click", ex)
        End Try
    End Sub

    Private Async Function RunOnce(k_view As Boolean) As Task
        If _running Then Return
        Dim k_name As String = txtName.Text.Trim()
        Dim k_units As List(Of String) = TickedDatabases()
        Dim k_problem As String = OnceQuery.CheckName(k_name)
        If k_problem = String.Empty Then k_problem = OnceQuery.CheckSize(txtSql.Text)
        If k_problem = String.Empty AndAlso k_units.Count = 0 Then k_problem = "Bifați cel puțin o bază."
        If k_problem <> String.Empty Then
            KBotMessage.Show(k_problem, "Interogare unică", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Not k_view Then
            If Not UiRun.Confirm("Interogarea «" & k_name & "» se rulează pe AVACONT_SURSA și pe " & k_units.Count.ToString(CultureInfo.InvariantCulture) & " unități." & vbLf &
                                 "O bază care a rulat deja textul exact este sărită; modificările nu se pot anula." & vbLf & vbLf &
                                 "Ați făcut întâi «Vezi (nu execută)»? Continuați?", "Interogare unică") Then Return
        End If
        Dim k_command As String = OnceQuery.RunCommand(_context.Client.Settings, k_name, txtSql.Text, k_view, k_units)
        Await Stream(k_command, "interogare unică «" & k_name & "» (" & If(k_view, "vezi", "execută") & ")", k_view)
    End Function

    Private Async Sub BtnStatus_Click(k_sender As Object, e As EventArgs) Handles btnStatus.Click
        Try
            Await LoadHistoryAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.BtnStatus_Click", ex)
        End Try
    End Sub

    ''' <summary>The history: every query the runner ran, from the template's ledger, with the databases still to do.</summary>
    Private Async Function LoadHistoryAsync() As Task
        Await UiRun.Guard("OnceQueryView.LoadHistoryAsync", _context,
            Async Function()
                _context.Say("Se citește istoricul interogărilor...")
                Dim k_result As OnceHistoryResult = Await _context.Client.CallAsync(Of OnceHistoryResult)("once-history")
                gridHist.BeginUpdate()
                Try
                    gridHist.ClearRows()
                    For Each k_item As OnceHistoryItem In k_result.Items
                        Dim k_row As KBotDataRow = gridHist.AddRow()
                        k_row.Tag = k_item
                        k_row("name") = k_item.Name
                        k_row("ranat") = k_item.RanAt.Replace("T", " ")
                        k_row("by") = k_item.RanBy
                        k_row("rows") = If(k_item.Rows.HasValue, k_item.Rows.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                        k_row("done") = k_item.Done.ToString(CultureInfo.InvariantCulture) & "/" & k_item.Total.ToString(CultureInfo.InvariantCulture)
                        k_row("missing") = If(k_item.Missing.Count = 0, "-", String.Join(", ", k_item.Missing))
                    Next
                Finally
                    gridHist.EndUpdate()
                End Try
                _context.Say(k_result.Items.Count.ToString(CultureInfo.InvariantCulture) & " interogări în istoric. Dublu clic pe un rând arată textul.")
            End Function)
    End Function

    Private Sub GridHist_CellDoubleClick(k_sender As Object, e As KBotCellEventArgs) Handles gridHist.CellDoubleClick
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= gridHist.Rows.Count Then Return
            Dim k_item As OnceHistoryItem = TryCast(gridHist.Rows(e.RowIndex).Tag, OnceHistoryItem)
            If k_item Is Nothing Then Return
            Dim k_size As Long = Encoding.UTF8.GetByteCount(k_item.Text)
            Using k_form As New SqlTextForm(k_item.Name, k_size, k_item.Text, k_size, False)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("OnceQueryView.GridHist_CellDoubleClick", ex)
        End Try
    End Sub

    Private Async Function Stream(k_command As String, k_title As String, k_view As Boolean) As Task
        _running = True
        SetButtons(False)
        _log.Clear()
        _log.Add(KBotLogLevel.Info, "== " & k_title & " ==")
        Dim k_result As SshResult = Nothing
        Await UiRun.Guard("OnceQueryView.Stream", _context,
            Async Function()
                _context.Say("Se rulează: " & k_title & "...")
                k_result = Await _context.Client.StreamAsync(k_command, Sub(k_line) _log.Add(k_line))
            End Function)
        If k_result IsNot Nothing Then
            lblState.Text = OnceQuery.Describe(k_result.ExitStatus, k_view)
            _log.Add(If(k_result.ExitStatus = 0, KBotLogLevel.Info, KBotLogLevel.Warn), lblState.Text)
            _context.Say(lblState.Text)
        End If
        _running = False
        SetButtons(True)
        If k_result IsNot Nothing AndAlso Not k_view Then Await LoadHistoryAsync()
    End Function

    Private Sub SetButtons(k_on As Boolean)
        btnView.Enabled = k_on
        btnRun.Enabled = k_on
        btnStatus.Enabled = k_on
    End Sub

End Class
