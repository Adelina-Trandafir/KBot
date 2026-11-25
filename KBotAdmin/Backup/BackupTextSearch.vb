Option Strict On
Imports System.Globalization

''' <summary>
''' One version of a row found by a text search across the saved copies (slice ME04-01): the row with exactly these values exists in every copy
''' from <see cref="First"/> to <see cref="Last"/> (of the copies that hold it).
''' </summary>
Public NotInheritable Class BackupTextHit
    Public Property TableName As String = String.Empty
    Public Property Columns As List(Of BackupColumn)
    Public Property Values As String()
    ''' <summary>The primary key values (empty when the table has none).</summary>
    Public Property Key As List(Of String)
    Public Property First As SnapshotManifest
    Public Property Last As SnapshotManifest
    Public Property Copies As Integer
    ''' <summary>True when the newest copy of the database still holds this exact version.</summary>
    Public Property InNewest As Boolean

    Public ReadOnly Property KeyText As String
        Get
            Return String.Join("; ", Key)
        End Get
    End Property

    ''' <summary>The row as «column=value» pairs, empty cells left out, binary columns left out.</summary>
    Public ReadOnly Property Summary As String
        Get
            Dim k_parts As New List(Of String)()
            For k_i As Integer = 0 To Math.Min(Columns.Count, Values.Length) - 1
                If Columns(k_i).IsBinary OrElse String.IsNullOrEmpty(Values(k_i)) Then Continue For
                Dim k_text As String = Values(k_i).Replace(vbCr, " ").Replace(vbLf, " ")
                If k_text.Length > 80 Then k_text = k_text.Substring(0, 80) & "..."
                k_parts.Add(Columns(k_i).Name & "=" & k_text)
            Next
            Return String.Join("  ", k_parts)
        End Get
    End Property
End Class

''' <summary>Looks for a text in every row of the saved copies of one database (slice ME04-01); reads only the local store.</summary>
Public NotInheritable Class BackupTextSearch

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The versions of rows (in any text column) that contain <paramref name="k_needle"/>, over the copies of ONE database (oldest first). A page shared by
    ''' many copies is opened once. Each distinct version of a row is one hit, with the first and the last copy that holds it.
    ''' </summary>
    ''' <param name="k_tableName">Only this table; Nothing or empty = every table.</param>
    ''' <param name="k_progress">Called with a short status (any thread); may be Nothing.</param>
    ''' <param name="k_cancel">Checked between pages.</param>
    Public Shared Function Search(k_store As BackupStore, k_snapshots As IList(Of SnapshotManifest), k_tableName As String, k_needle As String,
                                  k_progress As Action(Of String), k_cancel As Threading.CancellationToken) As List(Of BackupTextHit)
        Try
            If String.IsNullOrWhiteSpace(k_needle) Then Throw New ArgumentException("Textul căutat e gol.", NameOf(k_needle))
            Dim k_hits As New Dictionary(Of String, BackupTextHit)(StringComparer.Ordinal)
            Dim k_order As New List(Of BackupTextHit)()
            Dim k_pageCache As New Dictionary(Of String, List(Of String()))(StringComparer.Ordinal)   ' page object -> its rows that contain the text
            Dim k_newest As SnapshotManifest = If(k_snapshots.Count = 0, Nothing, k_snapshots(k_snapshots.Count - 1))
            Dim k_step As Integer = 0
            For Each k_snapshot As SnapshotManifest In k_snapshots
                k_step += 1
                k_progress?.Invoke("Copia " & k_step.ToString(CultureInfo.InvariantCulture) & " din " & k_snapshots.Count.ToString(CultureInfo.InvariantCulture) &
                                   " (" & k_order.Count.ToString(CultureInfo.InvariantCulture) & " rezultate)...")
                For Each k_table As BackupTable In k_snapshot.Tables
                    If Not String.IsNullOrWhiteSpace(k_tableName) AndAlso Not String.Equals(k_table.Name, k_tableName.Trim(), StringComparison.OrdinalIgnoreCase) Then Continue For
                    Dim k_textColumns As New List(Of Integer)()
                    For k_c As Integer = 0 To k_table.Columns.Count - 1
                        If Not k_table.Columns(k_c).IsBinary Then k_textColumns.Add(k_c)
                    Next
                    Dim k_keyAt As New List(Of Integer)()
                    For Each k_keyName As String In k_table.PrimaryKey
                        Dim k_name As String = k_keyName
                        Dim k_at As Integer = k_table.Columns.FindIndex(Function(k_col) String.Equals(k_col.Name, k_name, StringComparison.OrdinalIgnoreCase))
                        If k_at >= 0 Then k_keyAt.Add(k_at)
                    Next
                    For Each k_page As BackupPage In k_table.Pages
                        k_cancel.ThrowIfCancellationRequested()
                        Dim k_cacheKey As String = k_page.ObjectName
                        Dim k_matches As List(Of String()) = Nothing
                        If Not k_pageCache.TryGetValue(k_cacheKey, k_matches) Then
                            k_matches = New List(Of String())()
                            For Each k_row As String() In BackupReader.ReadPage(k_store, k_page)
                                For Each k_c As Integer In k_textColumns
                                    If k_c < k_row.Length AndAlso k_row(k_c) IsNot Nothing AndAlso k_row(k_c).IndexOf(k_needle, StringComparison.CurrentCultureIgnoreCase) >= 0 Then
                                        k_matches.Add(k_row)
                                        Exit For
                                    End If
                                Next
                            Next
                            k_pageCache(k_cacheKey) = k_matches
                        End If
                        For Each k_row As String() In k_matches
                            Dim k_id As String = k_table.Name & ChrW(30) & String.Join(ChrW(31), k_row.Select(Function(k_v) If(k_v, ChrW(0))))
                            Dim k_hit As BackupTextHit = Nothing
                            If Not k_hits.TryGetValue(k_id, k_hit) Then
                                k_hit = New BackupTextHit With {.TableName = k_table.Name, .Columns = k_table.Columns, .Values = k_row, .First = k_snapshot,
                                                                .Key = k_keyAt.Select(Function(k_a) If(k_a < k_row.Length, k_row(k_a), String.Empty)).ToList()}
                                k_hits(k_id) = k_hit
                                k_order.Add(k_hit)
                            End If
                            If Not ReferenceEquals(k_hit.Last, k_snapshot) Then k_hit.Copies += 1
                            k_hit.Last = k_snapshot
                        Next
                    Next
                Next
            Next
            For Each k_hit As BackupTextHit In k_order
                k_hit.InNewest = k_newest IsNot Nothing AndAlso ReferenceEquals(k_hit.Last, k_newest)
            Next
            Return k_order
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("BackupTextSearch.Search", ex)
            Throw
        End Try
    End Function

End Class
