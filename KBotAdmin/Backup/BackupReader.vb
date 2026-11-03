Option Strict On
Imports System.IO
Imports System.Text.Json

''' <summary>One version of a record: its values in one snapshot (or the fact that it did not exist yet / any more).</summary>
Public NotInheritable Class BackupRecordVersion
    Public Property Snapshot As SnapshotManifest
    Public Property Present As Boolean
    ''' <summary>The values in the order of the table's columns (Nothing = NULL); binary columns are base64.</summary>
    Public Property Values As String()
    ''' <summary>The columns whose value differs from the previous version that existed.</summary>
    Public ReadOnly Property Changed As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>True for the first snapshot in which the record appears and for the one in which it disappears.</summary>
    Public Property Appeared As Boolean
    Public Property Disappeared As Boolean
End Class

''' <summary>Reads pages back out of the store: the rows of a table at a moment, and the history of one record across moments (ME02-07).</summary>
Public NotInheritable Class BackupReader

    Private Sub New()
    End Sub

    ''' <summary>The rows of one page (values as text, Nothing = NULL).</summary>
    Public Shared Function ReadPage(k_store As BackupStore, k_page As BackupPage) As List(Of String())
        Dim k_bytes As Byte() = k_store.GetObject(k_page.ObjectName)
        Dim k_rows As New List(Of String())()
        Using k_doc As JsonDocument = JsonDocument.Parse(k_bytes)
            For Each k_row As JsonElement In k_doc.RootElement.EnumerateArray()
                Dim k_values As New List(Of String)()
                For Each k_cell As JsonElement In k_row.EnumerateArray()
                    k_values.Add(If(k_cell.ValueKind = JsonValueKind.Null, Nothing, k_cell.GetString()))
                Next
                k_rows.Add(k_values.ToArray())
            Next
        End Using
        Return k_rows
    End Function

    Public Shared Function FindTable(k_manifest As SnapshotManifest, k_name As String) As BackupTable
        For Each k_table As BackupTable In k_manifest.Tables
            If String.Equals(k_table.Name, k_name, StringComparison.OrdinalIgnoreCase) Then Return k_table
        Next
        Return Nothing
    End Function

    Private Shared Function KeyIndexes(k_table As BackupTable) As Integer()
        If k_table.PrimaryKey.Count = 0 Then Throw New BackupException("Tabela " & k_table.Name & " nu are cheie primară: istoricul unui rând nu se poate urmări.")
        Dim k_idx(k_table.PrimaryKey.Count - 1) As Integer
        For k_i As Integer = 0 To k_idx.Length - 1
            Dim k_keyName As String = k_table.PrimaryKey(k_i)
            k_idx(k_i) = k_table.Columns.FindIndex(Function(k_c) String.Equals(k_c.Name, k_keyName, StringComparison.OrdinalIgnoreCase))
            If k_idx(k_i) < 0 Then Throw New BackupException("Cheia primară a tabelei " & k_table.Name & " nu se găsește printre coloane.")
        Next
        Return k_idx
    End Function

    ''' <summary>
    ''' The record with this key in each of the given snapshots (one database, oldest first). Every distinct page is opened once even if many
    ''' snapshots share it (an unchanged page is the same object).
    ''' </summary>
    Public Shared Function RecordHistory(k_store As BackupStore, k_snapshots As IList(Of SnapshotManifest), k_tableName As String,
                                         k_key As IList(Of String), k_progress As Action(Of String)) As List(Of BackupRecordVersion)
        Try
            Dim k_result As New List(Of BackupRecordVersion)()
            Dim k_cache As New Dictionary(Of String, String())(StringComparer.Ordinal)   ' page object -> the record's values, or Nothing when the page does not hold it
            Dim k_previous As BackupRecordVersion = Nothing
            Dim k_step As Integer = 0
            For Each k_snapshot As SnapshotManifest In k_snapshots
                k_step += 1
                k_progress?.Invoke("Copia " & k_step.ToString(Globalization.CultureInfo.InvariantCulture) & " din " & k_snapshots.Count.ToString(Globalization.CultureInfo.InvariantCulture) & "...")
                Dim k_version As New BackupRecordVersion With {.Snapshot = k_snapshot}
                Dim k_table As BackupTable = FindTable(k_snapshot, k_tableName)
                If k_table IsNot Nothing Then
                    Dim k_idx As Integer() = KeyIndexes(k_table)
                    If k_idx.Length <> k_key.Count Then Throw New BackupException("Cheia are " & k_idx.Length & " părți; ați dat " & k_key.Count & ".")
                    For Each k_page As BackupPage In k_table.Pages
                        Dim k_found As String() = Nothing
                        If Not k_cache.TryGetValue(k_page.ObjectName & "|" & String.Join(ChrW(31), k_idx), k_found) Then
                            k_found = SearchPage(k_store, k_page, k_idx, k_key)
                            k_cache(k_page.ObjectName & "|" & String.Join(ChrW(31), k_idx)) = k_found
                        End If
                        If k_found IsNot Nothing Then
                            k_version.Present = True
                            k_version.Values = k_found
                            Exit For
                        End If
                    Next
                End If
                If k_version.Present AndAlso (k_previous Is Nothing OrElse Not k_previous.Present) Then k_version.Appeared = True
                If Not k_version.Present AndAlso k_previous IsNot Nothing AndAlso k_previous.Present Then k_version.Disappeared = True
                If k_version.Present AndAlso k_previous IsNot Nothing AndAlso k_previous.Present Then
                    Dim k_columns As List(Of BackupColumn) = FindTable(k_snapshot, k_tableName).Columns
                    Dim k_old As String() = k_previous.Values
                    For k_c As Integer = 0 To Math.Min(k_columns.Count, Math.Min(k_old.Length, k_version.Values.Length)) - 1
                        If Not String.Equals(k_old(k_c), k_version.Values(k_c), StringComparison.Ordinal) Then k_version.Changed.Add(k_columns(k_c).Name)
                    Next
                End If
                k_result.Add(k_version)
                k_previous = k_version
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("BackupReader.RecordHistory", ex)
            Throw
        End Try
    End Function

    Private Shared Function SearchPage(k_store As BackupStore, k_page As BackupPage, k_idx As Integer(), k_key As IList(Of String)) As String()
        For Each k_row As String() In ReadPage(k_store, k_page)
            Dim k_match As Boolean = True
            For k_i As Integer = 0 To k_idx.Length - 1
                If Not String.Equals(k_row(k_idx(k_i)), k_key(k_i), StringComparison.OrdinalIgnoreCase) Then
                    k_match = False
                    Exit For
                End If
            Next
            If k_match Then Return k_row
        Next
        Return Nothing
    End Function

End Class
