Imports System.Globalization
Imports System.Text
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Sends converted rows to one MariaDB table in multi-row INSERT statements, inside the caller's transaction.
''' </summary>
''' <remarks>
''' <para>
''' The duplicate-key rule is the point. By default (<see cref="EfTableSpec.UpdateExisting"/> = False) a row whose
''' primary key is already in the table is left EXACTLY as it is: <c>ON DUPLICATE KEY UPDATE key = key</c>. Only that
''' one error is silenced; a value that is too long or a missing parent still fails the statement. A row the operator
''' has since edited in K-BOT is therefore never overwritten by a second import. With UpdateExisting (the unit-of-measure
''' list) the other columns are replaced.
''' </para>
''' <para>
''' One unique key besides the primary key can exist (<c>EF_Facturi (SerieFactura, NumarFactura)</c>, <c>EF_Mesaje.IdSol</c>):
''' a row that collides on THAT key is also skipped, silently. The run reports added rows by counting the table before
''' and after, so a row skipped this way shows up as «deja în țintă», never as written.
''' </para>
''' </remarks>
Friend NotInheritable Class EfBatchWriter

    ''' <summary>A batch is sent early when the binary values in it reach this many bytes.</summary>
    Private Const MaxBatchBytes As Long = 6L * 1024L * 1024L

    Private ReadOnly _cn As MySqlConnection
    Private ReadOnly _tx As MySqlTransaction
    Private ReadOnly _spec As EfTableSpec
    Private ReadOnly _rows As New List(Of Object())()
    Private _bytes As Long

    Public Sub New(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_spec As EfTableSpec)
        If k_cn Is Nothing Then Throw New ArgumentNullException(NameOf(k_cn))
        If k_tx Is Nothing Then Throw New ArgumentNullException(NameOf(k_tx))
        If k_spec Is Nothing Then Throw New ArgumentNullException(NameOf(k_spec))
        _cn = k_cn
        _tx = k_tx
        _spec = k_spec
    End Sub

    Public Property RowsSent As Long

    Public Sub Add(k_row As Object())
        _rows.Add(k_row)
        For Each value In k_row
            Dim bytes = TryCast(value, Byte())
            If bytes IsNot Nothing Then _bytes += bytes.Length
        Next
        If _rows.Count >= _spec.BatchSize OrElse _bytes >= MaxBatchBytes Then Flush()
    End Sub

    Public Sub Flush()
        If _rows.Count = 0 Then Return
        Try
            Using cmd = _cn.CreateCommand()
                cmd.Transaction = _tx
                cmd.CommandTimeout = 600
                cmd.CommandText = BuildSql(_rows.Count)
                Dim index As Integer = 0
                For Each row In _rows
                    For Each value In row
                        cmd.Parameters.AddWithValue("@p" & index.ToString(CultureInfo.InvariantCulture), value)
                        index += 1
                    Next
                Next
                cmd.ExecuteNonQuery()
            End Using
            RowsSent += _rows.Count
            _rows.Clear()
            _bytes = 0
        Catch ex As Exception
            GlobalErrorLog.Write("EfBatchWriter.Flush", ex)
            Throw
        End Try
    End Sub

    Private Function BuildSql(k_rowCount As Integer) As String
        Dim columns = _spec.Columns
        Dim sb As New StringBuilder()
        sb.Append("INSERT INTO ").Append(TargetServer.Quote(_spec.TargetTable)).Append(" (")
        sb.Append(String.Join(", ", columns.Select(Function(c) TargetServer.Quote(c.Target))))
        sb.Append(") VALUES ")

        Dim index As Integer = 0
        For r = 0 To k_rowCount - 1
            If r > 0 Then sb.Append(", ")
            sb.Append("(")
            For c = 0 To columns.Count - 1
                If c > 0 Then sb.Append(", ")
                sb.Append("@p").Append(index.ToString(CultureInfo.InvariantCulture))
                index += 1
            Next
            sb.Append(")")
        Next

        sb.Append(" ON DUPLICATE KEY UPDATE ")
        If _spec.UpdateExisting Then
            Dim others = columns.Where(Function(c) Not String.Equals(c.Target, _spec.KeyColumn, StringComparison.OrdinalIgnoreCase)).ToList()
            If others.Count = 0 Then
                sb.Append(KeepRule())
            Else
                sb.Append(String.Join(", ", others.Select(Function(c) $"{TargetServer.Quote(c.Target)} = VALUES({TargetServer.Quote(c.Target)})")))
            End If
        Else
            sb.Append(KeepRule())
        End If
        Return sb.ToString()
    End Function

    ''' <summary>The no-op that makes a duplicate key leave the row alone.</summary>
    Private Function KeepRule() As String
        Dim key = TargetServer.Quote(_spec.KeyColumn)
        Return $"{key} = {key}"
    End Function

End Class
