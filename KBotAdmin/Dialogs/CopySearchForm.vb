Option Strict On
Imports System.Globalization
Imports System.Threading

''' <summary>
''' A text looked for in every saved copy of one database (slice ME04-01): shows each version of the rows that contain it, with the first and the
''' last copy that holds it and whether the newest copy still has it, so a deleted row shows the copy it was last seen in. Reads only the local store.
''' </summary>
Public Class CopySearchForm

    Private ReadOnly _store As BackupStore
    Private ReadOnly _snapshots As List(Of SnapshotManifest)
    Private _cancel As CancellationTokenSource

    ''' <param name="k_snapshots">The copies of ONE database, oldest first.</param>
    Public Sub New(k_store As BackupStore, k_snapshots As List(Of SnapshotManifest), k_tableName As String)
        InitializeComponent()
        Icon = AdminIcon.Value
        _store = k_store
        _snapshots = k_snapshots
        Text = "Căutare în copiile locale — " & If(_snapshots.Count > 0, _snapshots(0).Db, String.Empty)
        txtTable.Text = If(k_tableName, String.Empty)
        btnHistory.Enabled = False
        lblState.Text = _snapshots.Count.ToString(CultureInfo.InvariantCulture) & " copii de căutat."
    End Sub

    Private Sub TxtText_FieldKeyDown(k_sender As Object, e As KeyEventArgs) Handles txtText.FieldKeyDown, txtTable.FieldKeyDown
        Try
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                BtnSearch_Click(k_sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CopySearchForm.TxtText_FieldKeyDown", ex)
        End Try
    End Sub

    Private Async Sub BtnSearch_Click(k_sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Dim k_needle As String = txtText.Text.Trim()
            If k_needle.Length = 0 Then
                KBotMessage.Show("Scrieți textul căutat.", "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If _cancel IsNot Nothing Then _cancel.Cancel()
            Dim k_mine As New CancellationTokenSource()
            _cancel = k_mine
            btnSearch.Enabled = False
            btnHistory.Enabled = False
            lblState.Text = "Se caută în copii..."
            Dim k_table As String = txtTable.Text.Trim()
            Dim k_say As Action(Of String) = Sub(k_text) SayLater(k_text)
            Dim k_hits As List(Of BackupTextHit) = Await Task.Run(Function() BackupTextSearch.Search(_store, _snapshots, k_table, k_needle, k_say, k_mine.Token))
            If k_mine.IsCancellationRequested OrElse IsDisposed Then Return
            ShowHits(k_hits)
        Catch ex As OperationCanceledException
            ' a newer search or closing the window stopped this one
        Catch ex As BackupException
            lblState.Text = ex.Message
            KBotMessage.Show(ex.Message, "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("CopySearchForm.BtnSearch_Click", ex)
            lblState.Text = ex.Message
            KBotMessage.Show(ex.Message, "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not IsDisposed Then btnSearch.Enabled = True
        End Try
    End Sub

    Private Sub SayLater(k_text As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        BeginInvoke(New Action(Sub() lblState.Text = k_text))
    End Sub

    Private Shared Function StateOf(k_hit As BackupTextHit) As String
        If k_hit.InNewest Then Return "există și acum"
        Return "dispărut/schimbat"
    End Function

    Private Sub ShowHits(k_hits As List(Of BackupTextHit))
        Dim k_ordered As List(Of BackupTextHit) = k_hits.OrderByDescending(Function(k_h) k_h.Last.FinishedAt).ThenBy(Function(k_h) k_h.TableName, StringComparer.OrdinalIgnoreCase).ToList()
        gridHits.BeginUpdate()
        Try
            gridHits.ClearRows()
            For Each k_hit As BackupTextHit In k_ordered
                Dim k_row As KBotDataRow = gridHits.AddRow()
                k_row.Tag = k_hit
                k_row("first") = k_hit.First.FinishedAt
                k_row("last") = k_hit.Last.FinishedAt
                k_row("copies") = k_hit.Copies
                k_row("state") = StateOf(k_hit)
                k_row("table") = k_hit.TableName
                k_row("row") = If(k_hit.Key.Count > 0, "[" & k_hit.KeyText & "]  ", String.Empty) & k_hit.Summary
            Next
        Finally
            gridHits.EndUpdate()
        End Try
        If k_ordered.Count > 0 Then gridHits.CurrentRowIndex = 0
        lblState.Text = k_ordered.Count.ToString("N0", CultureInfo.CurrentCulture) & " versiuni de rânduri găsite în " &
                        _snapshots.Count.ToString(CultureInfo.InvariantCulture) & " copii. «Ultima copie» mai veche decât cea mai nouă înseamnă că rândul s-a schimbat sau a dispărut după ea."
        btnHistory.Enabled = k_ordered.Count > 0
    End Sub

    Private Function CurrentHit() As BackupTextHit
        Dim k_row As KBotDataRow = gridHits.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, BackupTextHit))
    End Function

    Private Sub BtnHistory_Click(k_sender As Object, e As EventArgs) Handles btnHistory.Click
        Try
            OpenHistory()
        Catch ex As Exception
            GlobalErrorLog.Write("CopySearchForm.BtnHistory_Click", ex)
            KBotMessage.Show(ex.Message, "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GridHits_CellDoubleClick(k_sender As Object, e As KBotCellEventArgs) Handles gridHits.CellDoubleClick
        Try
            OpenHistory()
        Catch ex As Exception
            GlobalErrorLog.Write("CopySearchForm.GridHits_CellDoubleClick", ex)
            KBotMessage.Show(ex.Message, "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OpenHistory()
        Dim k_hit As BackupTextHit = CurrentHit()
        If k_hit Is Nothing Then Return
        If k_hit.Key.Count = 0 Then
            KBotMessage.Show("Tabela " & k_hit.TableName & " nu are cheie primară: istoricul unui rând nu se poate urmări.", "Căutare în copii", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using k_form As New RecordHistoryForm(_store, _snapshots, k_hit.TableName, String.Join("; ", k_hit.Key))
            k_form.ShowDialog(Me)
        End Using
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _cancel IsNot Nothing Then _cancel.Cancel()
        MyBase.OnFormClosing(e)
    End Sub

End Class
