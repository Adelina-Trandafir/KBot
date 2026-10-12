Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common

''' <summary>One row of the table <c>Cale</c> of cale.accdb: a source folder (or base) and the unit it belongs to.</summary>
Public NotInheritable Class VrcCaleRow

    Public Property Cale As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Dc As String = String.Empty
    ''' <summary>The year of the base; 2100 marks the special OI/MF base (not a year).</summary>
    Public Property AnDate As Integer

End Class

''' <summary>
''' The name of the subunit comes from <c>cale.accdb</c>, table <c>Cale</c> (operator, 12.10.2026): the row whose <c>Cale</c> ends
''' with the name of the chosen source folder. The special rows (AnDate 2100: OI, MF) carry their own names and never name the
''' subunit. The file is opened read-only; nothing in it is changed.
''' </summary>
Public NotInheritable Class VrcCale

    Private Sub New()
    End Sub

    Public Shared Function Read(k_path As String) As List(Of VrcCaleRow)
        Try
            Dim k_rows As New List(Of VrcCaleRow)()
            Dim k_provider As String = String.Empty
            Using k_cn As OleDbConnection = VrcAccess.OpenReadOnly(k_path, k_provider)
                Using k_cmd As New OleDbCommand("SELECT [Cale], [Denumire], [DC], [AnDate] FROM [Cale]", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            Dim k_year As Integer = 0
                            If Not k_reader.IsDBNull(3) Then Integer.TryParse(Convert.ToString(k_reader.GetValue(3), CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_year)
                            k_rows.Add(New VrcCaleRow() With {
                                .Cale = Text(k_reader, 0), .Denumire = Text(k_reader, 1), .Dc = Text(k_reader, 2), .AnDate = k_year})
                        End While
                    End Using
                End Using
            End Using
            Return k_rows
        Catch ex As Exception
            GlobalErrorLog.Write("VrcCale.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Function Text(k_reader As OleDbDataReader, k_index As Integer) As String
        Return If(k_reader.IsDBNull(k_index), String.Empty, Convert.ToString(k_reader.GetValue(k_index), CultureInfo.InvariantCulture).Trim())
    End Function

    ''' <summary>The rows (not the special 2100 ones) whose Cale ends with the last segment of <paramref name="k_folder"/>, compared without case.</summary>
    Public Shared Function Match(k_rows As IEnumerable(Of VrcCaleRow), k_folder As String) As List(Of VrcCaleRow)
        Dim k_segment As String = IO.Path.GetFileName(k_folder.TrimEnd("\"c, "/"c))
        If k_segment.Length = 0 Then Return New List(Of VrcCaleRow)()
        Return k_rows.Where(Function(k_r) k_r.AnDate <> VrcSourceFile.SpecialMarker AndAlso
                                           String.Equals(IO.Path.GetFileName(k_r.Cale.TrimEnd("\"c, "/"c)), k_segment, StringComparison.OrdinalIgnoreCase)).ToList()
    End Function

    ''' <summary>The distinct names among the matching rows, in file order.</summary>
    Public Shared Function Names(k_matches As IEnumerable(Of VrcCaleRow)) As List(Of String)
        Return k_matches.Select(Function(k_r) k_r.Denumire).Where(Function(k_n) k_n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
    End Function

End Class
