Imports System.Collections.Generic
Imports System.Text.Json
Imports KBot.Common

''' <summary>Rezultatul rutării unui rând. POCO.</summary>
Public NotInheritable Class RouteResult
    ''' <summary>DC-urile în care rândul trebuie scris. Mai multe DOAR la FX_Extrase_F.</summary>
    Public ReadOnly Property Dcs As New List(Of String)()
    ''' <summary>Motivul respingerii, sau Nothing dacă rândul s-a rutat.</summary>
    Public Property Reject As String = Nothing
    Public ReadOnly Property IsRejected As Boolean
        Get
            Return Reject IsNot Nothing
        End Get
    End Property
End Class

''' <summary>
''' Rutează rândurile unui tabel, după regula din §5.1. Indexurile de coloană se rezolvă o
''' singură dată per chunk (<see cref="Prepare"/>), nu o dată per rând.
'''
''' Un rând a cărui cheie de rutare nu se rezolvă nu se scrie ȘI nu se pierde tăcut: pleacă
''' în fișierul de respinse, cu cheia primară și motivul, iar rularea continuă.
''' </summary>
Public NotInheritable Class RowRouter

    Private ReadOnly _table As SeedTable
    Private ReadOnly _maps As RoutingMaps

    Private _iPk As Integer = -1
    Private _iDc As Integer = -1
    Private _iUnit As Integer = -1
    Private _iRoute As Integer = -1
    Private _iRoute2 As Integer = -1

    Public Sub New(table As SeedTable, maps As RoutingMaps)
        _table = table
        _maps = maps
    End Sub

    ''' <summary>
    ''' Rezolvă indexurile de coloană pentru chunk-ul curent. Aruncă dacă lipsește o coloană
    ''' de care rutarea depinde — asta nu e un orfan, e un artefact care nu se potrivește cu
    ''' regula, deci se oprește.
    ''' </summary>
    Public Sub Prepare(chunk As ChunkFile)
        Try
            _iPk = chunk.IndexOfColumn(_table.PrimaryKey)

            Select Case _table.Routing
                Case RoutingKind.OwnDcThenUnit
                    _iDc = chunk.IndexOfColumn("DC")
                    _iUnit = chunk.IndexOfColumn("IdUnitate")
                    If _iDc < 0 AndAlso _iUnit < 0 Then
                        Throw New InvalidOperationException(
                            "«" & chunk.FileName & "» nu are nici DC, nici IdUnitate — " &
                            _table.Name & " nu poate fi rutat.")
                    End If

                Case RoutingKind.OwnUnit
                    _iUnit = chunk.IndexOfColumn("IdUnitate")
                    If _iUnit < 0 Then
                        Throw New InvalidOperationException(
                            "«" & chunk.FileName & "» nu are coloana IdUnitate, pe care se rutează " & _table.Name & ".")
                    End If

                Case RoutingKind.TwoParents
                    _iRoute = chunk.IndexOfColumn(_table.RouteColumn)
                    _iRoute2 = chunk.IndexOfColumn(_table.RouteColumn2)
                    If _iRoute < 0 AndAlso _iRoute2 < 0 Then
                        Throw New InvalidOperationException(
                            "«" & chunk.FileName & "» nu are nici " & _table.RouteColumn & ", nici " &
                            _table.RouteColumn2 & " — " & _table.Name & " nu poate fi rutat.")
                    End If

                Case Else
                    _iRoute = chunk.IndexOfColumn(_table.RouteColumn)
                    If _iRoute < 0 Then
                        Throw New InvalidOperationException(
                            "«" & chunk.FileName & "» nu are coloana " & _table.RouteColumn &
                            ", pe care se rutează " & _table.Name & ".")
                    End If
            End Select

        Catch ex As Exception
            GlobalErrorLog.Write("RowRouter.Prepare", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Cheia primară a rândului, ca text — pentru fișierul de respinse.</summary>
    Public Function PrimaryKeyOf(row As JsonElement()) As String
        If _iPk < 0 OrElse _iPk >= row.Length Then Return "?"
        Return If(JsonValues.AsText(row(_iPk)), "")
    End Function

    Public Function Route(row As JsonElement()) As RouteResult
        Try
            Dim r As New RouteResult()

            Select Case _table.Routing

                Case RoutingKind.OwnDcThenUnit
                    ' DC propriu dacă e completat, altfel IdUnitate prin [Cai].
                    Dim dc As String = Nothing
                    If _iDc >= 0 Then dc = JsonValues.AsText(row(_iDc))
                    If String.IsNullOrWhiteSpace(dc) Then dc = DcFromUnit(row, r)
                    AddDc(r, dc, "DC/IdUnitate")

                Case RoutingKind.OwnUnit
                    AddDc(r, DcFromUnit(row, r), "IdUnitate")

                Case RoutingKind.ByAngajament
                    Dim cod As String = JsonValues.AsText(row(_iRoute))
                    If String.IsNullOrWhiteSpace(cod) Then
                        r.Reject = _table.RouteColumn & " lipsește"
                    Else
                        Dim dc As String = Nothing
                        If _maps.Angajament.TryGetValue(cod, dc) Then
                            r.Dcs.Add(dc)
                        Else
                            r.Reject = "CodAngajament «" & cod & "» nu există în FX_Angajamente"
                        End If
                    End If

                Case RoutingKind.ByRezervare
                    RouteByLongMap(row, _iRoute, _table.RouteColumn, _maps.Rezervare, "FX_Rezervari", r)

                Case RoutingKind.TwoParents
                    RouteTwoParents(row, r)

                Case RoutingKind.FanOutByExtrasFile
                    ' Multiplicare INTENȚIONATĂ: un fișier de extras poate purta linii pentru
                    ' mai multe unități, deci același rând aparține legitim mai multor baze.
                    Dim idexf As Long
                    If Not JsonValues.TryAsLong(row(_iRoute), idexf) Then
                        r.Reject = "IDEXF lipsește"
                    Else
                        Dim set1 As HashSet(Of String) = Nothing
                        If _maps.ExtrasFile.TryGetValue(idexf, set1) AndAlso set1.Count > 0 Then
                            Dim sorted As New List(Of String)(set1)
                            sorted.Sort(StringComparer.OrdinalIgnoreCase)
                            r.Dcs.AddRange(sorted)
                        Else
                            r.Reject = "IDEXF " & idexf.ToString() & " nu apare în FX_Extrase_H"
                        End If
                    End If

            End Select

            If r.Dcs.Count = 0 AndAlso Not r.IsRejected Then
                r.Reject = "cheia de rutare nu se rezolvă în niciun DC"
            End If
            Return r

        Catch ex As Exception
            GlobalErrorLog.Write("RowRouter.Route", ex)
            Throw
        End Try
    End Function

    ' --- ajutoare private, atinse doar prin Route (deja învelit) --------------------

    Private Function DcFromUnit(row As JsonElement(), r As RouteResult) As String
        If _iUnit < 0 Then
            r.Reject = "coloana IdUnitate lipsește"
            Return Nothing
        End If
        Dim unit As Long
        If Not JsonValues.TryAsLong(row(_iUnit), unit) Then
            r.Reject = "IdUnitate lipsește"
            Return Nothing
        End If
        Dim dc As String = Nothing
        If Not _maps.UnitToDc.TryGetValue(unit, dc) Then
            r.Reject = "IdUnitate " & unit.ToString() & " nu există în [Cai]"
            Return Nothing
        End If
        Return dc
    End Function

    Private Shared Sub AddDc(r As RouteResult, dc As String, what As String)
        If r.IsRejected Then Return
        If String.IsNullOrWhiteSpace(dc) Then
            r.Reject = what & " nu se rezolvă în niciun DC"
        Else
            r.Dcs.Add(dc)
        End If
    End Sub

    Private Sub RouteByLongMap(row As JsonElement(), idx As Integer, columnName As String,
                               map As Dictionary(Of Long, String), parentTable As String, r As RouteResult)
        Dim key As Long
        If Not JsonValues.TryAsLong(row(idx), key) Then
            r.Reject = columnName & " lipsește"
            Return
        End If
        Dim dc As String = Nothing
        If map.TryGetValue(key, dc) Then
            r.Dcs.Add(dc)
        Else
            r.Reject = columnName & " " & key.ToString() & " nu apare în " & parentTable
        End If
    End Sub

    ''' <summary>
    ''' Doi părinți candidați: se încearcă primul, cu retragere pe al doilea. Dacă AMÂNDOI
    ''' sunt prezenți și NU sunt de acord asupra DC-ului, asta e eroare dură — nu retragere:
    ''' înseamnă că legăturile din Access se contrazic, iar alegerea unuia dintre ele ar fi
    ''' o ghiceală care mută date în baza greșită.
    ''' </summary>
    Private Sub RouteTwoParents(row As JsonElement(), r As RouteResult)
        Dim first As String = LookupParent(row, _iRoute, _table.RouteColumn)
        Dim second As String = LookupParent(row, _iRoute2, _table.RouteColumn2)

        If first IsNot Nothing AndAlso second IsNot Nothing AndAlso
           Not String.Equals(first, second, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                _table.Name & ", rândul cu cheia «" & PrimaryKeyOf(row) & "»: cei doi părinți nu sunt de acord — " &
                _table.RouteColumn & " duce la «" & first & "», iar " & _table.RouteColumn2 &
                " la «" & second & "». Migrarea se oprește; nu ghicim care are dreptate.")
        End If

        Dim dc As String = If(first, second)
        If dc Is Nothing Then
            r.Reject = "nici " & _table.RouteColumn & ", nici " & _table.RouteColumn2 & " nu se rezolvă"
        Else
            r.Dcs.Add(dc)
        End If
    End Sub

    Private Function LookupParent(row As JsonElement(), idx As Integer, columnName As String) As String
        If idx < 0 Then Return Nothing
        Dim key As Long
        If Not JsonValues.TryAsLong(row(idx), key) Then Return Nothing
        Dim dc As String = Nothing
        Dim ok As Boolean
        If String.Equals(columnName, "IDRR", StringComparison.OrdinalIgnoreCase) Then
            ok = _maps.ReceptieR.TryGetValue(key, dc)      ' harta C
        Else
            ok = _maps.ReceptieH.TryGetValue(key, dc)      ' harta D
        End If
        Return If(ok, dc, Nothing)
    End Function

End Class
