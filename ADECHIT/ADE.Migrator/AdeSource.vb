Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports KBot.Common
Imports KBot.Migrator

''' <summary>The receipt series of the unit (Access CFGs, family CH).</summary>
Public NotInheritable Class AdeReceiptConfig

    Public Sub New(k_serie As String, k_numar As Integer, k_explicatie As String)
        Serie = k_serie
        Numar = k_numar
        Explicatie = k_explicatie
    End Sub

    Public ReadOnly Property Serie As String
    Public ReadOnly Property Numar As Integer
    Public ReadOnly Property Explicatie As String

End Class

''' <summary>
''' Everything read from the ADECHIT Access file: the unit code (<c>Unitati!DC</c>, which is also the name of the
''' MariaDB database), the rows of the tables that are migrated, and the receipt series. Read-only; the file is never
''' changed.
''' </summary>
Public NotInheritable Class AdeSource

    Private Sub New()
    End Sub

    Public ReadOnly Property FilePath As String
    ''' <summary>Unit code = name of the unit database on the MariaDB server.</summary>
    Public ReadOnly Property Dc As String
    ''' <summary>Unit name from Access (Unitati.Denumire); only a suggestion for the subunit name.</summary>
    Public ReadOnly Property UnitName As String = String.Empty
    ''' <summary>SHA-256 of the file, recorded in AD_Imports.</summary>
    Public ReadOnly Property FileHash As String
    Public ReadOnly Property Tables As New Dictionary(Of String, DataTable)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>Nothing when the file has no CH configuration.</summary>
    Public ReadOnly Property ReceiptConfig As AdeReceiptConfig

    ''' <summary>
    ''' Opens <paramref name="k_path"/> (the shared AVACONT password is tried by <see cref="AccessProvider"/>) and reads
    ''' the migrated tables.
    ''' </summary>
    Public Shared Function Read(k_path As String) As AdeSource
        Try
            Dim k_source As New AdeSource()
            k_source._filePath = k_path
            k_source._fileHash = HashOf(k_path)
            Using k_cn As OleDbConnection = AccessProvider.Open(k_path, String.Empty)
                k_source._dc = ReadDc(k_cn)
                k_source._unitName = ReadUnitName(k_cn)
                For Each k_table In AdeSchema.Tables
                    k_source.Tables(k_table.Name) = Load(k_cn, k_table.Name)
                Next
                k_source._receiptConfig = ReadReceiptConfig(k_cn)
            End Using
            Return k_source
        Catch ex As Exception
            GlobalErrorLog.Write("AdeSource.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Function Load(k_cn As OleDbConnection, k_table As String) As DataTable
        Dim k_result As New DataTable(k_table) With {.Locale = CultureInfo.InvariantCulture}
        Using k_adapter As New OleDbDataAdapter($"SELECT * FROM [{k_table}]", k_cn)
            k_adapter.Fill(k_result)
        End Using
        Return k_result
    End Function

    Private Shared Function ReadDc(k_cn As OleDbConnection) As String
        Using k_cmd As New OleDbCommand("SELECT DC FROM Unitati", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_value = Convert.ToString(k_reader(0), CultureInfo.InvariantCulture)
                    If Not String.IsNullOrWhiteSpace(k_value) Then Return k_value.Trim()
                End While
            End Using
        End Using
        Throw New InvalidOperationException("Tabelul Unitati nu are nicio valoare în coloana DC.")
    End Function

    Private Shared Function ReadUnitName(k_cn As OleDbConnection) As String
        Using k_cmd As New OleDbCommand("SELECT Denumire FROM Unitati", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_value = Convert.ToString(k_reader(0), CultureInfo.InvariantCulture)
                    If Not String.IsNullOrWhiteSpace(k_value) Then Return k_value.Trim()
                End While
            End Using
        End Using
        Return String.Empty
    End Function

    Private Shared Function ReadReceiptConfig(k_cn As OleDbConnection) As AdeReceiptConfig
        Dim k_values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New OleDbCommand("SELECT [CFG], [VL] FROM [CFGs] WHERE [f]='CH'", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_values(Convert.ToString(k_reader(0), CultureInfo.InvariantCulture)) =
                        Convert.ToString(k_reader(1), CultureInfo.InvariantCulture)
                End While
            End Using
        End Using
        Dim k_serie As String = Nothing
        Dim k_numarText As String = Nothing
        Dim k_numar As Integer
        If Not k_values.TryGetValue("SERIE", k_serie) OrElse String.IsNullOrWhiteSpace(k_serie) Then Return Nothing
        If Not k_values.TryGetValue("NUMAR", k_numarText) OrElse
           Not Integer.TryParse(k_numarText, NumberStyles.Integer, CultureInfo.InvariantCulture, k_numar) Then Return Nothing
        Dim k_explicatie As String = Nothing
        k_values.TryGetValue("EXPLICATIE", k_explicatie)
        Return New AdeReceiptConfig(k_serie.Trim(), k_numar, If(k_explicatie, String.Empty))
    End Function

    Private Shared Function HashOf(k_path As String) As String
        ' Shared read: the file may be open in Access on the same machine.
        Using k_stream As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using k_sha As SHA256 = SHA256.Create()
                Return Convert.ToHexString(k_sha.ComputeHash(k_stream)).ToLowerInvariant()
            End Using
        End Using
    End Function

End Class
