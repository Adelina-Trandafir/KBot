Option Strict On
Imports System.Globalization

''' <summary>
''' One record followed through the saved copies of a database (slice ME02-07): which copies hold it, how its values changed from copy
''' to copy, and the SQL that would put one of its old versions back. Reads only the local store, never the server.
''' </summary>
Public Class RecordHistoryForm

    Private ReadOnly _store As BackupStore
    Private ReadOnly _snapshots As List(Of SnapshotManifest)
    Private ReadOnly _tableName As String
    Private ReadOnly _tableNow As BackupTable
    Private _versions As New List(Of BackupRecordVersion)()

    ''' <param name="k_snapshots">The copies of ONE database, oldest first.</param>
    Public Sub New(k_store As BackupStore, k_snapshots As List(Of SnapshotManifest), k_tableName As String, k_initialKey As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        _store = k_store
        _snapshots = k_snapshots
        _tableName = k_tableName
        _tableNow = Nothing
        For k_i As Integer = _snapshots.Count - 1 To 0 Step -1
            _tableNow = BackupReader.FindTable(_snapshots(k_i), _tableName)
            If _tableNow IsNot Nothing Then Exit For
        Next
        Text = "Istoricul unui rând — " & _snapshots(0).Db & "." & _tableName
        If _tableNow Is Nothing OrElse _tableNow.PrimaryKey.Count = 0 Then
            lblKey.Text = "Tabela nu are cheie primară."
            btnSearch.Enabled = False
        Else
            lblKey.Text = "Cheia (" & String.Join("; ", _tableNow.PrimaryKey) & "):"
        End If
        txtKey.Text = If(k_initialKey, String.Empty)
        btnCopySql.Enabled = False
    End Sub

    Private Sub TxtKey_FieldKeyDown(k_sender As Object, e As KeyEventArgs) Handles txtKey.FieldKeyDown
        Try
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                BtnSearch_Click(k_sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RecordHistoryForm.TxtKey_FieldKeyDown", ex)
        End Try
    End Sub

    Private Async Sub BtnSearch_Click(k_sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            If _tableNow Is Nothing Then Return
            Dim k_parts As New List(Of String)()
            For Each k_part As String In txtKey.Text.Split(";"c)
                k_parts.Add(k_part.Trim())
            Next
            If k_parts.Count <> _tableNow.PrimaryKey.Count OrElse k_parts.Any(Function(k_p) k_p.Length = 0) Then
                KBotMessage.Show("Scrieți toate părțile cheii (" & String.Join("; ", _tableNow.PrimaryKey) & "), separate prin ;.", "Istoricul unui rând", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            btnSearch.Enabled = False
            lblState.Text = "Se caută rândul în copii..."
            Dim k_say As Action(Of String) = Sub(k_text) SayLater(k_text)
            Dim k_found As List(Of BackupRecordVersion) = Await Task.Run(Function() BackupReader.RecordHistory(_store, _snapshots, _tableName, k_parts, k_say))
            _versions = k_found
            ShowVersions()
        Catch ex As BackupException
            lblState.Text = ex.Message
            KBotMessage.Show(ex.Message, "Istoricul unui rând", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("RecordHistoryForm.BtnSearch_Click", ex)
            lblState.Text = ex.Message
            KBotMessage.Show(ex.Message, "Istoricul unui rând", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnSearch.Enabled = _tableNow IsNot Nothing AndAlso _tableNow.PrimaryKey.Count > 0
        End Try
    End Sub

    Private Sub SayLater(k_text As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        BeginInvoke(New Action(Sub() lblState.Text = k_text))
    End Sub

    Private Shared Function StateOf(k_version As BackupRecordVersion) As String
        If k_version.Present Then
            If k_version.Appeared Then Return "apare"
            Return If(k_version.Changed.Count > 0, "modificat", "neschimbat")
        End If
        Return If(k_version.Disappeared, "a dispărut", "nu există")
    End Function

    Private Sub ShowVersions()
        gridVersions.BeginUpdate()
        Try
            gridVersions.ClearRows()
            For Each k_version As BackupRecordVersion In _versions
                Dim k_row As KBotDataRow = gridVersions.AddRow()
                k_row.Tag = k_version
                k_row("when") = k_version.Snapshot.FinishedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)
                k_row("state") = StateOf(k_version)
                k_row("kind") = k_version.Snapshot.Kind
                k_row("changed") = String.Join(", ", k_version.Changed.OrderBy(Function(k_n) k_n, StringComparer.OrdinalIgnoreCase))
            Next
        Finally
            gridVersions.EndUpdate()
        End Try
        Dim k_present As Integer = _versions.Where(Function(k_v) k_v.Present).Count()
        lblState.Text = "Rândul există în " & k_present.ToString(CultureInfo.InvariantCulture) & " din " & _versions.Count.ToString(CultureInfo.InvariantCulture) & " copii."
        If _versions.Count > 0 Then gridVersions.CurrentRowIndex = _versions.Count - 1
        ShowValues()
    End Sub

    Private Function SelectedVersion() As BackupRecordVersion
        Dim k_row As KBotDataRow = gridVersions.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, BackupRecordVersion))
    End Function

    Private Sub GridVersions_SelectionChanged(k_sender As Object, e As EventArgs) Handles gridVersions.SelectionChanged
        Try
            ShowValues()
        Catch ex As Exception
            GlobalErrorLog.Write("RecordHistoryForm.GridVersions_SelectionChanged", ex)
        End Try
    End Sub

    Private Shared Function CellText(k_value As String, k_column As BackupColumn) As String
        If k_value Is Nothing Then Return "[NULL]"
        If k_column.IsBinary Then Return "[binar, " & ((k_value.Length * 3) \ 4).ToString(CultureInfo.InvariantCulture) & " octeți]"
        Return If(k_value.Length > 300, k_value.Substring(0, 300) & "...", k_value)
    End Function

    Private Sub ShowValues()
        gridValues.BeginUpdate()
        Try
            gridValues.ClearRows()
            btnCopySql.Enabled = False
            Dim k_version As BackupRecordVersion = SelectedVersion()
            If k_version Is Nothing OrElse Not k_version.Present Then Return
            Dim k_table As BackupTable = BackupReader.FindTable(k_version.Snapshot, _tableName)
            If k_table Is Nothing Then Return
            Dim k_before As BackupRecordVersion = Nothing
            Dim k_at As Integer = _versions.IndexOf(k_version)
            For k_i As Integer = k_at - 1 To 0 Step -1
                If _versions(k_i).Present Then
                    k_before = _versions(k_i)
                    Exit For
                End If
            Next
            For k_c As Integer = 0 To k_table.Columns.Count - 1
                Dim k_col As BackupColumn = k_table.Columns(k_c)
                Dim k_row As KBotDataRow = gridValues.AddRow()
                k_row("column") = k_col.Name
                k_row("value") = CellText(If(k_c < k_version.Values.Length, k_version.Values(k_c), Nothing), k_col)
                If k_before IsNot Nothing AndAlso k_version.Changed.Contains(k_col.Name) AndAlso k_c < k_before.Values.Length Then
                    k_row("before") = CellText(k_before.Values(k_c), k_col)
                End If
            Next
            btnCopySql.Enabled = True
        Finally
            gridValues.EndUpdate()
        End Try
    End Sub

    Private Sub BtnCopySql_Click(k_sender As Object, e As EventArgs) Handles btnCopySql.Click
        Try
            Dim k_version As BackupRecordVersion = SelectedVersion()
            If k_version Is Nothing OrElse Not k_version.Present Then Return
            Dim k_table As BackupTable = BackupReader.FindTable(k_version.Snapshot, _tableName)
            Clipboard.SetText(BackupSql.UpsertStatement(k_table, k_version.Values))
            lblState.Text = "Comanda SQL este în clipboard. Citiți-o și rulați-o pe baza potrivită, dacă vreți să readuceți rândul."
        Catch ex As Exception
            GlobalErrorLog.Write("RecordHistoryForm.BtnCopySql_Click", ex)
            KBotMessage.Show(ex.Message, "Istoricul unui rând", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
