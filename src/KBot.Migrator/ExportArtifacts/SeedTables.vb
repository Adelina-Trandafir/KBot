Imports System.Collections.Generic

''' <summary>
''' Cele 16 tabele migrate, în ordinea în care se scriu (părinți înaintea copiilor), plus
''' regula de rutare a fiecăruia. Lista e FIXĂ: nu se descoperă nimic din relații și nu se
''' potrivește niciun prefix — descoperirea e exact ce cade azi (vezi mdl_FX_ExportMD).
'''
''' Setul e identic cu ALLOWED_TABLES din PYTHON/routes/forexe/seed.py. Fără FX_ORD*,
''' fără FX_DDF*, fără FX_Parteneri.
''' </summary>
Public Enum RoutingKind
    ''' <summary>Coloana DC proprie dacă există și e nevidă, altfel IdUnitate propriu.</summary>
    OwnDcThenUnit = 0
    ''' <summary>IdUnitate propriu, prin Cai.json.</summary>
    OwnUnit = 1
    ''' <summary>CodAngajament, prin harta A.</summary>
    ByAngajament = 2
    ''' <summary>IDRZ, prin harta B.</summary>
    ByRezervare = 3
    ''' <summary>Doi părinți candidați: primul, cu retragere pe al doilea.</summary>
    TwoParents = 4
    ''' <summary>IDEXF, prin harta E — un rând poate ajunge în MAI MULTE DC-uri.</summary>
    FanOutByExtrasFile = 5
End Enum

''' <summary>Descrierea unui tabel migrat. POCO — fără logică, deci fără Try/Catch.</summary>
Public NotInheritable Class SeedTable

    Public Sub New(name As String, primaryKey As String, routing As RoutingKind)
        Me.Name = name
        Me.PrimaryKey = primaryKey
        Me.Routing = routing
        Me.DdfColumns = New List(Of String)()
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property PrimaryKey As String
    Public ReadOnly Property Routing As RoutingKind

    ''' <summary>Coloana pe care se face rutarea (prima, la TwoParents).</summary>
    Public Property RouteColumn As String = Nothing

    ''' <summary>A doua coloană candidată, doar la TwoParents.</summary>
    Public Property RouteColumn2 As String = Nothing

    ''' <summary>
    ''' Coloanele care arată spre familia DDF (IDDF / IDREV) și care se VERIFICĂ pe server
    ''' înainte de scriere. Nu se traduce niciodată nimic — vezi <c>SeedApiClient.CheckIds</c>.
    ''' </summary>
    Public ReadOnly Property DdfColumns As List(Of String)

End Class

Public Module SeedTables

    ''' <summary>Tabelul MariaDB al lui IDDF.</summary>
    Public Const DdfTable As String = "FX_DDF"
    ''' <summary>Tabelul MariaDB al lui IDREV.</summary>
    Public Const DdfRevTable As String = "FX_DDF_REV"

    Private ReadOnly _all As List(Of SeedTable) = BuildAll()

    ''' <summary>Cele 16 tabele, în ordinea de scriere.</summary>
    Public Function All() As IReadOnlyList(Of SeedTable)
        Return _all
    End Function

    ''' <summary>Descrierea unui tabel. Cheie necunoscută → ArgumentException (fără no-op tăcut).</summary>
    Public Function ByName(name As String) As SeedTable
        For Each t As SeedTable In _all
            If String.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) Then
                Return t
            End If
        Next
        Throw New ArgumentException("Tabelul «" & If(name, "<null>") & "» nu face parte din setul migrat.", NameOf(name))
    End Function

    Private Function BuildAll() As List(Of SeedTable)
        Dim list As New List(Of SeedTable)()

        ' --- FX_Angajamente: rădăcina. DC propriu dacă e completat, altfel IdUnitate. -----
        ' Exportul Access arată o coloană DC reală pe tabelul ăsta (FX_System_Export/TABLES/
        ' FX_Angajamente.md), deci ramura de retragere e o plasă, nu drumul obișnuit.
        Dim ang As New SeedTable("FX_Angajamente", "CodAngajament", RoutingKind.OwnDcThenUnit)
        ang.DdfColumns.Add("IDDF")
        list.Add(ang)

        list.Add(New SeedTable("FX_Indicatori", "CodAI", RoutingKind.OwnUnit))

        Dim ist As New SeedTable("FX_Istoric", "ID", RoutingKind.ByAngajament)
        ist.RouteColumn = "CodAngajament"
        ist.DdfColumns.Add("IDREV")
        list.Add(ist)

        Dim sal As New SeedTable("FX_Salarii", "IDFXS", RoutingKind.ByAngajament)
        sal.RouteColumn = "CodAngajament"
        sal.DdfColumns.Add("IDDF")
        sal.DdfColumns.Add("IDREV")
        list.Add(sal)

        Dim rez As New SeedTable("FX_Rezervari", "IDRZ", RoutingKind.ByAngajament)
        rez.RouteColumn = "CodAngajament"
        rez.DdfColumns.Add("IDREV")
        list.Add(rez)

        Dim rezImg As New SeedTable("FX_Rezervarii_IMG", "IDRZC", RoutingKind.ByRezervare)
        rezImg.RouteColumn = "IDRZ"
        list.Add(rezImg)

        Dim recR As New SeedTable("FX_Receptii_R", "IDRR", RoutingKind.ByAngajament)
        recR.RouteColumn = "CodAngajament"
        list.Add(recR)

        Dim recH As New SeedTable("FX_Receptii_H", "IDRH", RoutingKind.ByAngajament)
        recH.RouteColumn = "CodAngajament"
        list.Add(recH)

        Dim rec As New SeedTable("FX_Receptii", "IDR", RoutingKind.OwnUnit)
        rec.DdfColumns.Add("IDREV")
        list.Add(rec)

        list.Add(New SeedTable("FX_Receptii_RHR", "IDRHR", RoutingKind.OwnUnit))

        ' Doi părinți: IDRR (harta C) întâi, apoi IDRH (harta D).
        Dim recImg As New SeedTable("FX_Receptii_IMG", "IDRDC", RoutingKind.TwoParents)
        recImg.RouteColumn = "IDRR"
        recImg.RouteColumn2 = "IDRH"
        list.Add(recImg)

        Dim plati As New SeedTable("FX_Plati", "IdPlataFX", RoutingKind.OwnUnit)
        plati.DdfColumns.Add("IDREV")
        list.Add(plati)

        ' Doi părinți, în ordinea inversă celui de mai sus: IDRH (harta D) întâi, apoi IDRR.
        Dim recPlati As New SeedTable("FX_Receptii_Plati", "IDRP", RoutingKind.TwoParents)
        recPlati.RouteColumn = "IDRH"
        recPlati.RouteColumn2 = "IDRR"
        list.Add(recPlati)

        ' FX_Extrase_F se MULTIPLICĂ intenționat: un fișier de extras poate purta linii
        ' pentru mai multe unități, deci același rând aparține legitim mai multor baze.
        Dim exF As New SeedTable("FX_Extrase_F", "IDEXF", RoutingKind.FanOutByExtrasFile)
        exF.RouteColumn = "IDEXF"
        list.Add(exF)

        list.Add(New SeedTable("FX_Extrase_H", "IDEXH", RoutingKind.OwnUnit))
        list.Add(New SeedTable("FX_Extrase", "IDFXE", RoutingKind.OwnUnit))

        Return list
    End Function

End Module
