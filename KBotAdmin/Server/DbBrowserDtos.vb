Option Strict On
Imports System.Text.Json
Imports System.Text.Json.Serialization

' The shapes admin_dbview.py prints (PYTHON/routes/admin_cli): the page «Tabele». Keys are ASCII snake_case on the wire.

Public Class DbTableSummary
    Public Property Name As String = String.Empty
    Public Property Kind As String = String.Empty
    Public Property Engine As String
    Public Property RowsEst As Long
    Public Property DataBytes As Long
    Public Property IndexBytes As Long
    Public Property ColumnsN As Integer
    Public Property Comment As String = String.Empty
End Class

Public Class DbTablesResult
    Public Property Db As String = String.Empty
    Public Property Tables As New List(Of DbTableSummary)()
End Class

Public Class DbTableInfo
    Public Property Name As String = String.Empty
    Public Property Kind As String = String.Empty
    Public Property Engine As String
    Public Property RowsEst As Long
    Public Property DataBytes As Long
    Public Property IndexBytes As Long
    Public Property AutoIncrement As Long?
    Public Property Created As String
    Public Property Updated As String
    Public Property Collation As String
    Public Property Comment As String = String.Empty
End Class

Public Class DbColumnInfo
    Public Property Name As String = String.Empty
    Public Property Pos As Integer
    Public Property ColumnType As String = String.Empty
    Public Property DataType As String = String.Empty
    Public Property CharLen As Long?
    Public Property NumPrec As Integer?
    Public Property NumScale As Integer?
    Public Property DtPrec As Integer?
    Public Property Nullable As Boolean
    Public Property DefaultValue As String
    Public Property Extra As String = String.Empty
    Public Property ColKey As String = String.Empty
    Public Property Collation As String
    Public Property Comment As String = String.Empty
End Class

Public Class DbForeignKey
    Public Property Name As String = String.Empty
    Public Property Columns As New List(Of String)()
    Public Property RefDb As String = String.Empty
    Public Property RefTable As String = String.Empty
    Public Property RefColumns As New List(Of String)()
    Public Property OnUpdate As String
    Public Property OnDelete As String
End Class

Public Class DbReference
    Public Property Name As String = String.Empty
    Public Property FromDb As String = String.Empty
    Public Property Table As String = String.Empty
    Public Property Columns As New List(Of String)()
    Public Property RefColumns As New List(Of String)()
End Class

Public Class DbIndexInfo
    Public Property Name As String = String.Empty
    Public Property Unique As Boolean
    Public Property Type As String = String.Empty
    Public Property Columns As New List(Of String)()
End Class

''' <summary>One page of rows: each cell is a JSON value (null, a number, a string, <c>{"b": n}</c> for a blob, <c>{"t": ..., "more": true}</c> for a cut text).</summary>
Public Class DbRowsPage
    Public Property Offset As Integer
    Public Property Limit As Integer
    Public Property Rows As New List(Of List(Of JsonElement))()
    Public Property HasMore As Boolean
End Class

Public Class DbTableResult
    Public Property Db As String = String.Empty
    Public Property Info As New DbTableInfo()
    Public Property Columns As New List(Of DbColumnInfo)()
    Public Property PrimaryKey As New List(Of String)()
    Public Property ForeignKeys As New List(Of DbForeignKey)()
    Public Property ReferencedBy As New List(Of DbReference)()
    Public Property Indexes As New List(Of DbIndexInfo)()
    Public Property Page As New DbRowsPage()
    ''' <summary>Only the answer of <c>query-run</c> has it: how long the query took on the server.</summary>
    Public Property ElapsedMs As Long
End Class

Public Class DbRowsResult
    Public Property Page As New DbRowsPage()
End Class

''' <summary>The values a column holds (no NULL, sorted), each as a JSON value like a cell of a page; <c>HasMore</c> = the list was cut at the server's limit.</summary>
Public Class DbDistinctResult
    Public Property Column As String = String.Empty
    Public Property Values As New List(Of JsonElement)()
    Public Property HasMore As Boolean
End Class

''' <summary>The whole content of one cell. <c>Kind</c> is null, text or blob; a blob comes as base64 in <c>B64</c>.</summary>
Public Class DbCellResult
    Public Property Kind As String = String.Empty
    Public Property Length As Long
    Public Property Truncated As Boolean
    Public Property Text As String
    <JsonPropertyName("b64")> Public Property B64 As String
End Class
