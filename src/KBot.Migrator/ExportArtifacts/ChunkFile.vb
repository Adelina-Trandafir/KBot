Imports System.Collections.Generic
Imports System.Text.Json

''' <summary>
''' Un fișier de chunk: <c>{ "table", "chunk", "columns", "rows" }</c>, cu rândurile ca
''' tablouri poziționale. Exact forma pe care <c>POST /api/forexe/seed/rows</c> o acceptă
''' deja, deci valorile nu se re-tipizează niciodată pe drum.
'''
''' Valorile se păstrează ca <see cref="JsonElement"/> CLONATE, nu convertite la tipuri
''' .NET: un Decimal(28,6) trecut prin Double s-ar rotunji tăcut, iar migrarea nu are voie
''' să schimbe nicio valoare. Elementele clonate sunt detașate de JsonDocument, deci
''' documentul poate fi eliberat imediat.
''' </summary>
Public NotInheritable Class ChunkFile

    Public Property Table As String = ""
    Public Property Chunk As Integer = 0
    Public Property FileName As String = ""
    Public Property Columns As New List(Of String)()
    Public Property Rows As New List(Of JsonElement())()

    ''' <summary>
    ''' Indexul unei coloane, sau -1 dacă lipsește. Case-insensitive, ca peste tot în
    ''' proiect (Access nu e case-sensitive la nume de câmp).
    ''' </summary>
    Public Function IndexOfColumn(name As String) As Integer
        For i As Integer = 0 To Columns.Count - 1
            If String.Equals(Columns(i), name, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        Return -1
    End Function

End Class
