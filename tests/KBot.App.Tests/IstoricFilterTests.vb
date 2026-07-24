Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Xunit
Imports KBot.Domain
Imports KBot.App

' Teste pure (fără STA) pentru IstoricFilter — portul lui mdl_FX_Popups.ApplyColumnFilter.
' Acoperă exact semantica cerută (§8): înlocuire în segment, acumulare între segmente,
' «TOATE» golește doar un segment, gruparea Rez_/Plata_ case-insensitive, distincția
' Rez_Initiala vs Rez_Initiala+, și segmentul lună vs zi.
Public Class IstoricFilterTests

    Private Shared Function Row(id As Integer, idClsf As Integer, tip As String, d As Date) As IstoricRand
        Return New IstoricRand() With {.Id = id, .IdClsf = idClsf, .TipRand = tip, .DataFx = d}
    End Function

    ' Set standard: 4 rânduri pe două clasificații, tipuri și zile diferite.
    Private Shared Function Data() As List(Of IstoricRand)
        Return New List(Of IstoricRand) From {
            Row(1, 10, "Rez_Initiala", New Date(2026, 1, 17, 8, 0, 0)),
            Row(2, 10, "Rez_Initiala+", New Date(2026, 1, 18, 9, 0, 0)),
            Row(3, 20, "PLATA_PLATA", New Date(2026, 2, 4, 19, 0, 0)),
            Row(4, 20, "Receptie", New Date(2026, 2, 4, 20, 0, 0))
        }
    End Function

    <Fact>
    Public Sub SetSegment_ReplacesWithinSegment_LastWins()
        Dim f As New IstoricFilter()
        f.SetTipRandExact("Rez_Initiala")
        f.SetTipRandExact("Receptie")           ' înlocuiește, nu acumulează
        Dim result = f.Apply(Data())
        Assert.Single(result)
        Assert.Equal(4, result(0).Id)
    End Sub

    <Fact>
    Public Sub Segments_AccumulateAcrossKinds_AsAnd()
        Dim f As New IstoricFilter()
        f.SetClsf({20}, "20")
        f.SetTipRandExact("PLATA_PLATA")
        Dim result = f.Apply(Data())
        Assert.Single(result)                    ' clsf 20 ȘI PLATA_PLATA -> doar rândul 3
        Assert.Equal(3, result(0).Id)
    End Sub

    <Fact>
    Public Sub Toate_ClearsOnlyOneSegment_LeavesOthers()
        Dim f As New IstoricFilter()
        f.SetClsf({10}, "10")
        f.SetTipRandExact("Rez_Initiala")
        f.ClearTipRand()                         ' «TOATE» pe TipRand — clsf rămâne
        Dim result = f.Apply(Data())
        Assert.True(f.ClsfActive)
        Assert.False(f.TipRandActive)
        Assert.Equal(New Integer() {1, 2}, result.Select(Function(r) r.Id).ToArray())
    End Sub

    <Fact>
    Public Sub TipRandPrefix_GroupsCaseInsensitive_PlataMatchesUpper()
        Dim f As New IstoricFilter()
        f.SetTipRandPrefix("Plata_", "PLĂȚI")    ' «Plata_» prinde «PLATA_PLATA» (case-insensitive)
        Dim result = f.Apply(Data())
        Assert.Single(result)
        Assert.Equal(3, result(0).Id)
    End Sub

    <Fact>
    Public Sub TipRandExact_DistinguishesPlusSuffix()
        Dim f As New IstoricFilter()
        f.SetTipRandExact("Rez_Initiala")        ' NU trebuie să prindă «Rez_Initiala+»
        Dim result = f.Apply(Data())
        Assert.Single(result)
        Assert.Equal(1, result(0).Id)
    End Sub

    <Fact>
    Public Sub DataFx_MonthSegment_MatchesWholeMonth()
        Dim f As New IstoricFilter()
        f.SetDataFxMonth(2026, 1, "Ianuarie/2026")
        Dim result = f.Apply(Data())
        Assert.Equal(New Integer() {1, 2}, result.Select(Function(r) r.Id).ToArray())
    End Sub

    <Fact>
    Public Sub DataFx_DaySegment_MatchesOneDay_IgnoringTime()
        Dim f As New IstoricFilter()
        f.SetDataFxDay(New Date(2026, 2, 4), "04.02.2026")
        Dim result = f.Apply(Data())
        Assert.Equal(New Integer() {3, 4}, result.Select(Function(r) r.Id).ToArray())
    End Sub

    <Fact>
    Public Sub ClearAll_EmptiesEverySegment()
        Dim f As New IstoricFilter()
        f.SetClsf({10}, "10")
        f.SetTipRandExact("Rez_Initiala")
        f.SetDataFxDay(New Date(2026, 1, 17), "17.01.2026")
        f.ClearAll()
        Assert.False(f.AnySegmentActive)
        Assert.Equal(4, f.Apply(Data()).Count)   ' fără filtru -> toate rândurile
    End Sub

End Class
