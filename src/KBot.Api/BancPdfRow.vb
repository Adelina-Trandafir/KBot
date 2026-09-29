Option Strict On
Imports System.Collections.Generic

''' <summary>
''' Slice 0078-05, BENCH ONLY: one row of <c>KBOT_BANC_PDF</c> (000_DEMO) as
''' GET /api/forexe/banc/pdf lists it -- everything but the content. POCO; property names are the
''' JSON keys (ApiClient keeps DTO names unchanged).
''' </summary>
Public NotInheritable Class BancPdfRow
    Public Property id As Long
    ''' <summary>When the server stored it, «yyyy-MM-dd HH:mm:ss.fff».</summary>
    Public Property primit As String = String.Empty
    Public Property tip As String = String.Empty
    Public Property id_doc As Integer
    Public Property nume_fisier As String = String.Empty
    Public Property pas As String = String.Empty
    Public Property semnatura As String = String.Empty
    Public Property sha256 As String = String.Empty
    Public Property dimensiune As Integer
    Public Property [operator] As String = String.Empty
End Class

''' <summary>The answer of GET /api/forexe/banc/pdf. POCO.</summary>
Public NotInheritable Class BancPdfList
    Public Property fisiere As List(Of BancPdfRow) = New List(Of BancPdfRow)()
End Class
