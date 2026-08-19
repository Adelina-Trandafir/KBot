Imports System.Collections.Generic

''' <summary>
''' Contorii unui tabel într-un DC. POCO — se adună, nu calculează nimic singur.
''' </summary>
Public NotInheritable Class DcTableStats
    Public Property Dc As String = ""
    Public Property Table As String = ""
    ''' <summary>Rânduri rutate către DC-ul ăsta.</summary>
    Public Property Routed As Integer = 0
    ''' <summary>Rânduri chiar inserate (rowcount 1).</summary>
    Public Property Inserted As Integer = 0
    ''' <summary>Rânduri deja prezente pe MariaDB, lăsate neatinse.</summary>
    Public Property Skipped As Integer = 0
End Class

''' <summary>Contorii unui tabel peste toată rularea.</summary>
Public NotInheritable Class TableStats

    Public Sub New(table As String)
        Me.Table = table
    End Sub

    Public ReadOnly Property Table As String
    ''' <summary>Rânduri citite din artefacte.</summary>
    Public Property Read As Integer = 0
    ''' <summary>Rânduri fără DC — nescrise, dar consemnate în fișierul de respinse.</summary>
    Public Property Rejected As Integer = 0
    ''' <summary>
    ''' Copii SUPLIMENTARE produse de multiplicare (FX_Extrase_F). Un rând trimis în două
    ''' DC-uri contribuie 2 la rutate și 1 aici, ca identitatea de la pasul 7 să închidă.
    ''' </summary>
    Public Property Duplicated As Integer = 0
    ''' <summary>Rânduri rutate către un DC care NU e selectat în rularea curentă.</summary>
    Public Property OutOfScope As Integer = 0

    Public ReadOnly Property PerDc As New Dictionary(Of String, DcTableStats)(StringComparer.OrdinalIgnoreCase)

    Public Function For1(dc As String) As DcTableStats
        Dim s As DcTableStats = Nothing
        If Not PerDc.TryGetValue(dc, s) Then
            s = New DcTableStats() With {.Dc = dc, .Table = Table}
            PerDc.Add(dc, s)
        End If
        Return s
    End Function

    Public ReadOnly Property RoutedTotal As Integer
        Get
            Dim n As Integer = 0
            For Each s As DcTableStats In PerDc.Values
                n += s.Routed
            Next
            Return n
        End Get
    End Property

End Class

''' <summary>
''' Ce a produs pasul de verificare și de ce are nevoie pasul de transfer.
''' </summary>
Public NotInheritable Class VerificationResult
    Public Property Manifest As ExportManifest
    Public Property Maps As RoutingMaps
    ''' <summary>Contorii de citire/rutare per tabel, din pasul 4.</summary>
    Public ReadOnly Property Stats As New Dictionary(Of String, TableStats)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>DC-urile care au trecut verificarea și pot primi scrieri.</summary>
    Public ReadOnly Property CleanDcs As New List(Of String)()
    ''' <summary>DC-urile oprite, cu motivul.</summary>
    Public ReadOnly Property BlockedDcs As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>„DC|Tabel" oprite (coloane care lipsesc pe țintă, id-uri DDF absente).</summary>
    Public ReadOnly Property BlockedTables As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>True dacă nimic nu a fost oprit și transferul poate porni fără forțare.</summary>
    Public Property IsClean As Boolean = False

    Public Function StatsFor(table As String) As TableStats
        Dim s As TableStats = Nothing
        If Not Stats.TryGetValue(table, s) Then
            s = New TableStats(table)
            Stats.Add(table, s)
        End If
        Return s
    End Function

    Public Shared Function BlockKey(dc As String, table As String) As String
        Return dc & "|" & table
    End Function

    Public Function IsBlocked(dc As String, table As String) As Boolean
        Return BlockedDcs.ContainsKey(dc) OrElse BlockedTables.ContainsKey(BlockKey(dc, table))
    End Function
End Class
