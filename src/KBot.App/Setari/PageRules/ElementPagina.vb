Option Strict On
Imports System.Collections.Generic
Imports System.Text.Json
Imports KBot.Common

''' <summary>
''' One element of the FOREXE page as the in-page script lists it (<c>listElements()</c> in
''' <c>ForexeWatch.js</c>): where it sits, what it is, a selector without Wicket ids, its
''' own text and its inline style. Read from JSON; never written back.
''' </summary>
Public NotInheritable Class ElementPagina

    ''' <summary>Position in the listing: what the page's highlightElement(i) expects back.</summary>
    Public Property Index As Integer = -1
    Public Property Depth As Integer
    Public Property Tag As String = String.Empty
    Public Property Id As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Classes As String = String.Empty
    Public Property Selector As String = String.Empty
    Public Property Text As String = String.Empty
    Public Property Style As String = String.Empty

    ''' <summary>The node caption in the tree of the page: tag, id, classes, name, then the text.</summary>
    Public ReadOnly Property Eticheta As String
        Get
            Dim s As String = Tag
            If Not String.IsNullOrEmpty(Id) Then s &= "#" & Id
            If Not String.IsNullOrEmpty(Classes) Then s &= " ." & Classes.Replace(" ", " .")
            If Not String.IsNullOrEmpty(Name) Then s &= $" [name='{Name}']"
            If Not String.IsNullOrEmpty(Text) Then s &= "  «" & Text & "»"
            Return s
        End Get
    End Property

    ''' <summary>The note a rule made from this element starts with.</summary>
    Public ReadOnly Property Descriere As String
        Get
            Dim s As String = Tag
            If Not String.IsNullOrEmpty(Id) Then s &= "#" & Id
            If Not String.IsNullOrEmpty(Text) Then s &= " «" & Text & "»"
            Return s
        End Get
    End Property

    ''' <summary>Parses the script's JSON array; a broken text gives an empty list and a log line.</summary>
    Public Shared Function DinJson(json As String) As List(Of ElementPagina)
        Dim lista As New List(Of ElementPagina)()
        If String.IsNullOrWhiteSpace(json) Then Return lista
        Try
            Using doc As JsonDocument = JsonDocument.Parse(json)
                If doc.RootElement.ValueKind <> JsonValueKind.Array Then Return lista
                For Each e As JsonElement In doc.RootElement.EnumerateArray()
                    If e.ValueKind <> JsonValueKind.Object Then Continue For
                    lista.Add(New ElementPagina With {
                        .Index = lista.Count,
                        .Depth = IntOf(e, "depth"),
                        .Tag = StrOf(e, "tag"),
                        .Id = StrOf(e, "id"),
                        .Name = StrOf(e, "name"),
                        .Classes = StrOf(e, "classes"),
                        .Selector = StrOf(e, "selector"),
                        .Text = StrOf(e, "text"),
                        .Style = StrOf(e, "style")})
                Next
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ElementPagina.DinJson", ex)
            Throw
        End Try
        Return lista
    End Function

    Private Shared Function StrOf(e As JsonElement, key As String) As String
        Dim v As JsonElement
        If e.TryGetProperty(key, v) AndAlso v.ValueKind = JsonValueKind.String Then Return If(v.GetString(), String.Empty)
        Return String.Empty
    End Function

    Private Shared Function IntOf(e As JsonElement, key As String) As Integer
        Dim v As JsonElement
        Dim n As Integer
        If e.TryGetProperty(key, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then Return n
        Return 0
    End Function

End Class
