Imports System.Collections.Generic

' What POST /api/forexe/extrase/import did (slice 0057).
'
' `Avertismente` is not decoration. The import resolves a statement's unit and
' classification through nomenclatoare that a given server may not have yet: the
' shared pair AVACONT_COMUN.DefaSS / DefaSSS, and Clasificatii_Venituri, which only
' entered AVACONT_SURSA on 09.09.2026. When one is missing the column stays NULL and
' the server says so here, instead of leaving the operator to discover months later
' that every statement header has no unit. The money rows are written either way.
Public Class ImportExtraseRezultat
    ''' <summary>How many statements were sent.</summary>
    Public Property Primite As Integer

    ''' <summary>How many statement FILES were written (FX_Extrase_F rows).</summary>
    Public Property Importate As Integer

    ''' <summary>How many were skipped: already imported, no XML, or unreadable XML.</summary>
    Public Property Sarite As Integer

    ''' <summary>How many money rows were written (FX_Extrase rows).</summary>
    Public Property Randuri As Integer

    ''' <summary>What the server could not resolve. Empty when everything lined up.</summary>
    Public ReadOnly Property Avertismente As New List(Of String)()
End Class
