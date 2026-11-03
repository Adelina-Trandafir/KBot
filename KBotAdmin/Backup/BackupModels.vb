Option Strict On
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>One column of a saved table (docs/ME02_FORMAT.md).</summary>
Public Class BackupColumn
    <JsonPropertyName("n")> Public Property Name As String = String.Empty
    <JsonPropertyName("dt")> Public Property DataType As String = String.Empty
    <JsonPropertyName("bin")> Public Property IsBinary As Boolean
End Class

''' <summary>
''' One page: the rows of a table whose keys lie in (Lo, Hi], kept as one object of the store. Lo / Hi are Nothing at the
''' two ends of the table. Sum / Full are the check values the database computed for the interval.
''' </summary>
Public Class BackupPage
    <JsonPropertyName("lo")> Public Property Lo As List(Of String)
    <JsonPropertyName("hi")> Public Property Hi As List(Of String)
    <JsonPropertyName("n")> Public Property Rows As Integer
    <JsonPropertyName("sum")> Public Property Sum As String = String.Empty
    <JsonPropertyName("full")> Public Property Full As String = String.Empty
    <JsonPropertyName("obj")> Public Property ObjectName As String = String.Empty
    <JsonPropertyName("bytes")> Public Property Bytes As Long
End Class

Public Class BackupTable
    <JsonPropertyName("name")> Public Property Name As String = String.Empty
    <JsonPropertyName("ddl")> Public Property DdlObject As String = String.Empty
    <JsonPropertyName("cols")> Public Property Columns As New List(Of BackupColumn)()
    <JsonPropertyName("pk")> Public Property PrimaryKey As New List(Of String)()
    <JsonPropertyName("modcol")> Public Property ModifiedColumn As String
    <JsonPropertyName("rows")> Public Property Rows As Long
    <JsonPropertyName("pages")> Public Property Pages As New List(Of BackupPage)()
End Class

''' <summary>A view, trigger, routine or event of the database, kept as the text the server gave.</summary>
Public Class BackupExtra
    <JsonPropertyName("kind")> Public Property Kind As String = String.Empty
    <JsonPropertyName("name")> Public Property Name As String = String.Empty
    <JsonPropertyName("ddl")> Public Property DdlObject As String = String.Empty
End Class

Public Class BackupStats
    <JsonPropertyName("pages_new")> Public Property PagesNew As Integer
    <JsonPropertyName("pages_same")> Public Property PagesSame As Integer
    <JsonPropertyName("bytes_new")> Public Property BytesNew As Long
    <JsonPropertyName("rows")> Public Property Rows As Long
    <JsonPropertyName("seconds")> Public Property Seconds As Double
End Class

''' <summary>One copy of one database at one moment: the list of pages of every table, nothing more.</summary>
Public Class SnapshotManifest
    <JsonPropertyName("v")> Public Property Version As Integer = 1
    <JsonPropertyName("id")> Public Property Id As String = String.Empty
    <JsonPropertyName("db")> Public Property Db As String = String.Empty
    <JsonPropertyName("kind")> Public Property Kind As String = "manual"
    <JsonPropertyName("reason")> Public Property Reason As String = String.Empty
    <JsonPropertyName("mode")> Public Property Mode As String = "fast"
    <JsonPropertyName("started")> Public Property Started As String = String.Empty
    <JsonPropertyName("finished")> Public Property Finished As String = String.Empty
    <JsonPropertyName("server_time")> Public Property ServerTime As String = String.Empty
    <JsonPropertyName("server_version")> Public Property ServerVersion As String = String.Empty
    <JsonPropertyName("prev")> Public Property PreviousId As String
    <JsonPropertyName("tables")> Public Property Tables As New List(Of BackupTable)()
    <JsonPropertyName("extras")> Public Property Extras As New List(Of BackupExtra)()
    <JsonPropertyName("stats")> Public Property Stats As New BackupStats()

    Private Shared ReadOnly Options As New JsonSerializerOptions With {
        .DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        .PropertyNameCaseInsensitive = False
    }

    Public Function ToJsonBytes() As Byte()
        Return JsonSerializer.SerializeToUtf8Bytes(Me, Options)
    End Function

    Public Shared Function FromJsonBytes(k_bytes As Byte()) As SnapshotManifest
        Dim k_manifest As SnapshotManifest = JsonSerializer.Deserialize(Of SnapshotManifest)(k_bytes, Options)
        If k_manifest Is Nothing OrElse k_manifest.Id = String.Empty Then Throw New BackupException("Un instantaneu al copiei locale este gol.")
        If k_manifest.Version <> 1 Then Throw New BackupException("Un instantaneu are o versiune de format necunoscută (" & k_manifest.Version & ").")
        Return k_manifest
    End Function

    ''' <summary>The moment the copy finished, as a date (local time); MinValue when the text cannot be read.</summary>
    <JsonIgnore>
    Public ReadOnly Property FinishedAt As DateTime
        Get
            Dim k_value As DateTimeOffset
            If DateTimeOffset.TryParse(Finished, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind, k_value) Then
                Return k_value.LocalDateTime
            End If
            Return DateTime.MinValue
        End Get
    End Property
End Class
