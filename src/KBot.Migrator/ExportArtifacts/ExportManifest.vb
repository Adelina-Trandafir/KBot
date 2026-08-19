Imports System.Collections.Generic

''' <summary>
''' Un rând din <c>manifest.json</c>, secțiunea <c>tables</c>. POCO.
''' </summary>
Public NotInheritable Class ManifestTable
    Public Property Table As String = ""
    Public Property Columns As New List(Of String)()
    Public Property Rows As Integer = 0
    Public Property Files As New List(Of String)()
End Class

''' <summary>
''' <c>manifest.json</c>, scris de <c>mdl_FX_ExportSeed</c> DUPĂ ce toate tabelele au fost
''' parcurse (numărul de rânduri se știe abia atunci). POCO.
''' </summary>
Public NotInheritable Class ExportManifest
    Public Property Exported As String = ""
    Public Property Source As String = ""
    Public Property CaiSource As String = ""

    ''' <summary>Fișierul cu tabelul [Cai] — metadate de rutare, nu tabel migrat.</summary>
    Public Property CaiFile As String = "Cai.json"
    Public Property CaiColumns As New List(Of String)()
    Public Property CaiRows As Integer = 0

    ''' <summary>
    ''' Tabele declarate în afara domeniului care totuși EXISTĂ în Access. Raportate de
    ''' exportator, niciodată exportate (FX_PRT_EXPL, FX_CopacAngajamente).
    ''' </summary>
    Public Property UnexpectedTables As New List(Of String)()

    Public Property Tables As New List(Of ManifestTable)()

    ''' <summary>Rândul de manifest al unui tabel, sau Nothing dacă lipsește.</summary>
    Public Function FindTable(name As String) As ManifestTable
        For Each t As ManifestTable In Tables
            If String.Equals(t.Table, name, StringComparison.OrdinalIgnoreCase) Then
                Return t
            End If
        Next
        Return Nothing
    End Function
End Class
