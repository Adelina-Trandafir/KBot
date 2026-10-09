Option Strict On
Imports System.Collections.Generic

' Slice 0008-02: groups of angajamente (FX_Angajamente_Grupe) and the alias of an angajament.
' Plain data: the server rules are in PYTHON/routes/forexe/grupe.py.

''' <summary>One indicator of an angajament, as the tooltip of the groups window lists it.</summary>
Public NotInheritable Class GrupaIndicator
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Ss As String = String.Empty
End Class

''' <summary>An angajament as the groups window shows it: code, name, alias and its indicators.</summary>
Public NotInheritable Class GrupaAngajament
    Public Property Cod As String = String.Empty
    Public Property Descriere As String = String.Empty
    ''' <summary>The operator's name for it; empty = none.</summary>
    Public Property AliasAng As String = String.Empty
    Public Property Indicatori As IReadOnlyList(Of GrupaIndicator) = Array.Empty(Of GrupaIndicator)()
End Class

''' <summary>A group of angajamente: its number, name, colour (#RRGGBB) and the codes in it.</summary>
Public NotInheritable Class GrupaInfo
    Public Property IdGr As Integer
    Public Property Denumire As String = String.Empty
    ''' <summary>#RRGGBB; black when the group has none.</summary>
    Public Property Culoare As String = "#000000"
    Public Property Coduri As IReadOnlyList(Of String) = Array.Empty(Of String)()
End Class

''' <summary>What GET /api/forexe/grupe returns: every group and every visible angajament.</summary>
Public NotInheritable Class GrupeCatalog
    Public Property Grupe As IReadOnlyList(Of GrupaInfo) = Array.Empty(Of GrupaInfo)()
    Public Property Angajamente As IReadOnlyList(Of GrupaAngajament) = Array.Empty(Of GrupaAngajament)()
End Class
