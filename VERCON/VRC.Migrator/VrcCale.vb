Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports KBot.Common

''' <summary>One row of the VERCON table «cale»: the folder of a database (relative to the VERCON folder), the subunit name (Detalii) and the year (AnDate).</summary>
Friend NotInheritable Class VrcCaleRow

    Public Property Cale As String = String.Empty
    Public Property Detalii As String = String.Empty
    Public Property AnDate As Integer

End Class

''' <summary>
''' The table «cale» of VERCON (file cale.accdb) as the PROPOSAL for the subunit of each source file (operator, 18.10.2026): the folder of the
''' file is matched with Cale, and Detalii is the subunit name. It only proposes; every value stays editable and nothing is written to Access.
''' </summary>
Friend NotInheritable Class VrcCale

    Private Sub New()
    End Sub

    ''' <summary>Reads the table «cale» (or the only table) of the Access file, read-only.</summary>
    Friend Shared Function Load(k_path As String) As List(Of VrcCaleRow)
        Try
            Dim k_provider As String = String.Empty
            Dim k_rows As New List(Of VrcCaleRow)()
            Using k_cn = VrcAccess.OpenReadOnly(k_path, k_provider)
                Dim k_table As String = FindTable(k_cn)
                Using k_cmd As New OleDbCommand($"SELECT * FROM [{k_table}]", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        Dim k_cale As Integer = Ordinal(k_reader, "Cale")
                        Dim k_detalii As Integer = Ordinal(k_reader, "Detalii")
                        Dim k_year As Integer = Ordinal(k_reader, "AnDate")
                        While k_reader.Read()
                            Dim k_row As New VrcCaleRow() With {
                                .Cale = If(k_reader.IsDBNull(k_cale), String.Empty, Convert.ToString(k_reader.GetValue(k_cale), CultureInfo.InvariantCulture)),
                                .Detalii = If(k_reader.IsDBNull(k_detalii), String.Empty, Convert.ToString(k_reader.GetValue(k_detalii), CultureInfo.InvariantCulture))}
                            Dim k_number As Decimal? = If(k_reader.IsDBNull(k_year), Nothing, VrcConvert.ToDecimal(k_reader.GetValue(k_year)))
                            If k_number.HasValue Then k_row.AnDate = CInt(Math.Round(k_number.Value))
                            k_rows.Add(k_row)
                        End While
                    End Using
                End Using
            End Using
            Return k_rows
        Catch ex As Exception
            GlobalErrorLog.Write("VrcCale.Load", ex)
            Throw
        End Try
    End Function

    Private Shared Function Ordinal(k_reader As OleDbDataReader, k_name As String) As Integer
        For k_i As Integer = 0 To k_reader.FieldCount - 1
            If String.Equals(k_reader.GetName(k_i), k_name, StringComparison.OrdinalIgnoreCase) Then Return k_i
        Next
        Throw New InvalidOperationException($"Tabelul cale nu are coloana «{k_name}».")
    End Function

    ''' <summary>The table named «cale» (any case); else the only user table; else an error that names the tables found.</summary>
    Private Shared Function FindTable(k_cn As OleDbConnection) As String
        Dim k_names As New List(Of String)()
        Using k_schema = k_cn.GetSchema("Tables")
            For Each k_row As DataRow In k_schema.Rows
                If String.Equals(Convert.ToString(k_row("TABLE_TYPE"), CultureInfo.InvariantCulture), "TABLE", StringComparison.OrdinalIgnoreCase) Then
                    k_names.Add(Convert.ToString(k_row("TABLE_NAME"), CultureInfo.InvariantCulture))
                End If
            Next
        End Using
        Dim k_exact = k_names.FirstOrDefault(Function(k_n) String.Equals(k_n, "cale", StringComparison.OrdinalIgnoreCase))
        If k_exact IsNot Nothing Then Return k_exact
        If k_names.Count = 1 Then Return k_names(0)
        Throw New InvalidOperationException("Nu găsesc tabelul «cale» în fișier. Tabele găsite: " & If(k_names.Count = 0, "niciunul", String.Join(", ", k_names)) & ".")
    End Function

    Private Shared Function Normalize(k_text As String) As String
        If k_text Is Nothing Then Return String.Empty
        Return k_text.Replace("/"c, "\"c).Trim().Trim("\"c).ToLowerInvariant()
    End Function

    ''' <summary>
    ''' The subunit (Detalii) proposed for a file, or an empty text. The folder of the file must END with Cale (the VERCON folder can be anywhere).
    ''' For a year file the row of that year (AnDate) is preferred; a special base (tz_2100: OI, MF) has no year, it is matched by folder only.
    ''' Several rows of the folder are accepted only when they all give the same name; otherwise nothing is proposed.
    ''' </summary>
    Friend Shared Function Find(k_rows As IList(Of VrcCaleRow), k_filePath As String, k_kind As VrcFileKind, k_year As Integer) As String
        Return Lookup(k_rows, k_filePath, k_kind, k_year).Name
    End Function

    ''' <summary>Why the subunit of a file was (not) taken from cale: the Cale that matched, or the reason it did not. Empty when no cale is loaded.</summary>
    Friend Shared Function Explain(k_rows As IList(Of VrcCaleRow), k_filePath As String, k_kind As VrcFileKind, k_year As Integer) As String
        Return Lookup(k_rows, k_filePath, k_kind, k_year).Info
    End Function

    Private Shared Function Lookup(k_rows As IList(Of VrcCaleRow), k_filePath As String, k_kind As VrcFileKind, k_year As Integer) As (Name As String, Info As String)
        If k_rows Is Nothing OrElse k_rows.Count = 0 OrElse String.IsNullOrWhiteSpace(k_filePath) Then Return (String.Empty, String.Empty)
        Dim k_folder As String = "\" & Normalize(Path.GetDirectoryName(k_filePath))
        Dim k_candidates = k_rows.Where(Function(k_r) Normalize(k_r.Cale).Length > 0 AndAlso k_r.Detalii.Trim().Length > 0 AndAlso
                                                      k_folder.EndsWith("\" & Normalize(k_r.Cale), StringComparison.Ordinal)).ToList()
        If k_candidates.Count = 0 Then Return (String.Empty, "cale: folderul fișierului nu se potrivește cu nicio Cale")
        Dim k_longest As Integer = k_candidates.Max(Function(k_r) Normalize(k_r.Cale).Length)   ' \ELIE\47 beats \47
        k_candidates = k_candidates.Where(Function(k_r) Normalize(k_r.Cale).Length = k_longest).ToList()
        Dim k_wanted As Integer = If(k_kind = VrcFileKind.Year, k_year, VrcSourceFile.SpecialMarker)
        Dim k_exact = k_candidates.Where(Function(k_r) k_r.AnDate = k_wanted).ToList()
        Dim k_pool = If(k_exact.Count > 0, k_exact, k_candidates)
        Dim k_names = k_pool.Select(Function(k_r) k_r.Detalii.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        Dim k_cale As String = k_pool(0).Cale.Trim()
        If k_names.Count <> 1 Then Return (String.Empty, $"cale: Cale «{k_cale}» are mai multe nume ({String.Join(" / ", k_names)}); alegeți subunitatea")
        Return (k_names(0), $"cale: «{k_names(0)}» din Cale «{k_cale}»")
    End Function

End Class
