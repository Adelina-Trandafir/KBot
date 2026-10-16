Option Strict On
Imports System.Text.Json.Serialization

' The shapes admin_cli.py prints (PYTHON/routes/admin_cli). Keys are ASCII snake_case on the wire; the
' serializer maps them by naming policy (see AdminClient), the two keys with digits are named by hand.

Public Class DatabaseInfo
    Public Property Db As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Cf As String = String.Empty
    Public Property Exists As Boolean
    Public Property InCai As Boolean
    Public Property Accounts As Integer
    Public Property Tables As Integer
    Public Property Rows As Long
    Public Property Mb As Double
    Public Property LastWrite As String
    Public Property SyncPending As Integer
    Public Property SyncDestructive As Integer
    Public Property SyncErrors As Integer
    <JsonPropertyName("err24")> Public Property Err24 As Integer
    <JsonPropertyName("warn24")> Public Property Warn24 As Integer
End Class

Public Class DatabasesResult
    Public Property Databases As New List(Of DatabaseInfo)()
    Public Property ErrorsWithoutDb As Integer
    Public Property PendingRequests As Integer
    Public Property ServerTime As String = String.Empty
End Class

Public Class AccountLink
    Public Property Un As String = String.Empty
    Public Property Dc As String = String.Empty
    Public Property Rol As String = String.Empty
    Public Property LastSs As String
    Public Property Roles As New List(Of String)()
End Class

Public Class AccountsResult
    Public Property Accounts As New List(Of AccountLink)()
End Class

Public Class PasswordStateRow
    Public Property Un As String = String.Empty
    Public Property MustChange As Boolean
    Public Property ChangedAt As String
End Class

Public Class PasswordStateResult
    Public Property Accounts As New List(Of PasswordStateRow)()
End Class

Public Class RoleInfo
    Public Property Id As Integer
    Public Property Cod As String = String.Empty
    Public Property Section As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Operations As New List(Of String)()
End Class

Public Class RolesResult
    Public Property Roles As New List(Of RoleInfo)()
    Public Property KnownMissing As New List(Of String)()
End Class

Public Class DiffStatement
    Public Property Table As String = String.Empty
    <JsonPropertyName("object")> Public Property ObjectName As String = String.Empty
    <JsonPropertyName("type")> Public Property Kind As String = String.Empty
    Public Property Action As String = String.Empty
    Public Property Destructive As Boolean
    Public Property Priority As Integer
    <JsonPropertyName("error")> Public Property ErrorText As String = String.Empty
    Public Property Sql As String = String.Empty
End Class

Public Class DiffResult
    Public Property Db As String = String.Empty
    Public Property Mode As String = String.Empty
    Public Property Count As Integer
    Public Property Destructive As Integer
    Public Property Statements As New List(Of DiffStatement)()
End Class

Public Class CommandInfo
    Public Property Name As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property Writes As Boolean
    ''' <summary>Label of the one value the command needs typed (empty = none); such a command runs on ONE database only.</summary>
    Public Property Param As String = String.Empty
End Class

Public Class CommandsResult
    Public Property Commands As New List(Of CommandInfo)()
End Class

Public Class CommandBlock
    Public Property Db As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Columns As New List(Of String)()
    Public Property Rows As New List(Of List(Of String))()
    Public Property Note As String = String.Empty
    ''' <summary>True: print each row as one untruncated line instead of an aligned column table.</summary>
    Public Property Raw As Boolean
End Class

Public Class CommandResult
    Public Property Name As String = String.Empty
    Public Property Blocks As New List(Of CommandBlock)()
End Class

Public Class LogFileInfo
    Public Property Key As String = String.Empty
    Public Property Label As String = String.Empty
    Public Property Exists As Boolean
    Public Property Size As Long
    Public Property Modified As String
    Public Property Rotated As Integer
End Class

Public Class LogsMeta
    Public Property Files As New List(Of LogFileInfo)()
    Public Property ServerTime As String = String.Empty
End Class

Public Class ServerLogEntry
    Public Property Ts As String = String.Empty
    Public Property File As String = String.Empty
    Public Property Level As String = String.Empty
    Public Property Ip As String = String.Empty
    Public Property User As String = String.Empty
    Public Property Db As String = String.Empty
    Public Property Session As String = String.Empty
    Public Property Msg As String = String.Empty
    Public Property Detail As String = String.Empty
End Class

Public Class LogsResult
    Public Property Rows As New List(Of ServerLogEntry)()
    Public Property Total As Integer
    Public Property Truncated As Boolean
    Public Property Unreadable As New List(Of String)()
    Public Property ServerTime As String = String.Empty
End Class

Public Class AuditRow
    Public Property Id As Long
    Public Property Moment As String = String.Empty
    Public Property Un As String = String.Empty
    Public Property Dc As String = String.Empty
    Public Property Action As String = String.Empty
    Public Property Target As String = String.Empty
    Public Property Details As String = String.Empty
    Public Property Result As String = String.Empty
    Public Property Machine As String = String.Empty
    Public Property Ip As String = String.Empty
End Class

Public Class AuditResult
    Public Property Rows As New List(Of AuditRow)()
End Class

Public Class VisitRow
    Public Property First As String = String.Empty
    Public Property Last As String = String.Empty
    Public Property Ip As String = String.Empty
    Public Property Country As String = String.Empty
    Public Property Mobile As Boolean
    Public Property Bot As Boolean
    Public Property Page As String = String.Empty
    Public Property Referrer As String = String.Empty
    Public Property Seconds As Integer
    Public Property Sections As String = String.Empty
End Class

Public Class VisitsResult
    Public Property Rows As New List(Of VisitRow)()
End Class

Public Class BackupFile
    Public Property Path As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Size As Long
    Public Property Modified As String = String.Empty
    Public Property Kind As String = String.Empty
End Class

Public Class BackupsResult
    Public Property Files As New List(Of BackupFile)()
End Class

Public Class BackupText
    Public Property Name As String = String.Empty
    Public Property Size As Long
    Public Property Text As String = String.Empty
    Public Property Truncated As Boolean
    Public Property ShownBytes As Long
End Class

Public Class OnceHistoryItem
    Public Property Name As String = String.Empty
    Public Property Hash As String = String.Empty
    Public Property RanAt As String = String.Empty
    Public Property RanBy As String = String.Empty
    Public Property Rows As Integer?
    Public Property Text As String = String.Empty
    Public Property Done As Integer
    Public Property Total As Integer
    Public Property Missing As New List(Of String)()
End Class

Public Class OnceHistoryResult
    Public Property Items As New List(Of OnceHistoryItem)()
    Public Property Units As Integer
End Class

Public Class BackupMade
    Public Property Db As String = String.Empty
    Public Property Path As String = String.Empty
    Public Property Size As Long
End Class

Public Class DiskInfo
    Public Property Label As String = String.Empty
    Public Property Path As String = String.Empty
    Public Property Total As Long
    Public Property Used As Long
    Public Property Free As Long
End Class

Public Class CertInfo
    Public Property Name As String = String.Empty
    Public Property Until As String = String.Empty
    Public Property Days As Integer?
End Class

Public Class StatusInfo
    Public Property Time As String = String.Empty
    Public Property UptimeS As Double?
    Public Property Load As New List(Of String)()
    Public Property MemTotal As Long?
    Public Property MemFree As Long?
    Public Property SwapTotal As Long?
    Public Property SwapFree As Long?
    Public Property Disks As New List(Of DiskInfo)()
    Public Property Processes As New List(Of String)()
    Public Property Mariadb As New Dictionary(Of String, String)()
    Public Property Redis As String = String.Empty
    Public Property Certificates As New List(Of CertInfo)()
    Public Property Logs As New List(Of LogFileInfo)()
End Class

Public Class RoleChange
    Public Property Changed As Integer
    Public Property Cod As String = String.Empty
End Class

Public Class RequestItem
    Public Property Id As Integer
    <JsonPropertyName("date")> Public Property DateText As String = String.Empty
    Public Property State As String = String.Empty
    Public Property Cf As String = String.Empty
    Public Property Email As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Year As Integer
    Public Property DbName As String = String.Empty
    Public Property DecidedOn As String = String.Empty
    Public Property DecidedBy As String = String.Empty
    Public Property Rows As Integer
End Class

Public Class RequestsResult
    Public Property Requests As New List(Of RequestItem)()
    Public Property Counts As New Dictionary(Of String, Integer)()
    Public Property Limit As Integer
End Class

Public Class CodeName
    Public Property Cod As String = String.Empty
    Public Property Name As String = String.Empty
End Class

Public Class PlanResult
    Public Property DbName As String = String.Empty
    Public Property DbNamePropus As String = String.Empty
    Public Property IdUnitate As New Dictionary(Of String, Integer)()
    Public Property CodProgram As New Dictionary(Of String, String)()
    Public Property Randuri As Integer
    Public Property Tabele As Integer
    Public Property Viewuri As Integer
End Class

Public Class PlanBlock
    Public Property Ok As Boolean
    Public Property Result As PlanResult
    Public Property Lines As New List(Of String)()
    Public Property Problem As String = String.Empty
End Class

Public Class RequestDetail
    Public Property Id As Integer
    Public Property Email As String = String.Empty
    Public Property Cf As String = String.Empty
    Public Property AnafName As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Year As Integer
    Public Property State As String = String.Empty
    Public Property DbName As String = String.Empty
    Public Property Reason As String = String.Empty
    Public Property RequestedOn As String = String.Empty
    Public Property DecidedOn As String = String.Empty
    Public Property DecidedBy As String = String.Empty
    Public Property Ip As String = String.Empty
    Public Property LinkActive As Boolean
    Public Property LinkValid As Boolean
    Public Property Ss As New List(Of CodeName)()
    Public Property F As New List(Of CodeName)()
    Public Property E As New List(Of CodeName)()
    Public Property Rows As Integer
    Public Property Approvable As Boolean
    Public Property Plan As PlanBlock
End Class

Public Class RejectResult
    Public Property Email As String = String.Empty
    Public Property Announced As Boolean
End Class

Public Class LinkResult
    Public Property LinkTrimis As Boolean
    Public Property Link As String
    Public Property Lines As New List(Of String)()
End Class
