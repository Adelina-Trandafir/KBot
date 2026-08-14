Option Strict On
Imports System.Collections.Generic
Imports KBot.Common
Imports Xunit

''' <summary>
''' Testele 10–11 din planul feliei 0031: filtrul și ceasul serverului.
'''
''' <c>ServerClock</c> e o stare de PROCES, deci fiecare test care îl atinge îl duce înapoi la zero
''' — altfel un test ar contamina ordinea celorlalte.
''' </summary>
Public Class LogFilterTests
    Implements IDisposable

    Public Sub New()
        ServerClock.Reset()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ServerClock.Reset()
    End Sub

    ' ── Testul 10: fiecare axă separat, două combinate, mulțimile goale ──────

    <Fact>
    Public Sub Filtrul_AxaFișier_Singură()
        Dim entries = BuildSet()
        Dim f As New LogFilter() With {.Files = New HashSet(Of String)({"a.log"})}

        Dim r As LogFilterResult = f.Apply(entries)

        Assert.Equal(2, r.ShownCount)
        Assert.All(r.Entries, Sub(e) Assert.Equal("a.log", e.FileName))
        Assert.Equal(4, r.TotalCount)
    End Sub

    <Fact>
    Public Sub Filtrul_AxaNivel_Singură()
        Dim entries = BuildSet()
        Dim f As New LogFilter() With {.Levels = New HashSet(Of KBotLogLevel)({KBotLogLevel.Error})}

        Dim r As LogFilterResult = f.Apply(entries)

        Assert.Equal(2, r.ShownCount)
        Assert.All(r.Entries, Sub(e) Assert.Equal(KBotLogLevel.Error, e.Level))
    End Sub

    <Fact>
    Public Sub Filtrul_AxaText_CautăȘiÎnInteriorulBloculuiBrut()
        Dim entries As New List(Of LogEntry) From {
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Error, "a.log", "eroare"),
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "a.log", "altceva")}
        ' Blocul brut al primei intrări primește o linie de stivă.
        entries(0).AppendRawLine("   at KBot.App.MetodaCăutată()")

        Dim f As New LogFilter() With {.Text = "metodacăutată"}
        Dim r As LogFilterResult = f.Apply(entries)

        ' Potrivire case-insensitive ÎN STIVĂ, nu doar în mesaj.
        Assert.Single(r.Entries)
        Assert.Equal("eroare", r.Entries(0).Message)
    End Sub

    <Fact>
    Public Sub Filtrul_TextGol_PotriveșteTot()
        Dim entries = BuildSet()
        Dim f As New LogFilter() With {.Text = String.Empty}

        Assert.Equal(4, f.Apply(entries).ShownCount)
    End Sub

    <Fact>
    Public Sub Filtrul_FărăDiacritice_NuGăseșteCuvântulCuDiacritice_LimitareCunoscută()
        ' Limitare DECLARATĂ, fixată cu test ca să nu fie «reparată» din greșeală fără să se
        ' observe: nu se pliază diacriticele.
        Dim entries As New List(Of LogEntry) From {
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "a.log", "fișier șters")}

        Assert.Empty(New LogFilter() With {.Text = "sters"}.Apply(entries).Entries)
        Assert.Single(New LogFilter() With {.Text = "șters"}.Apply(entries).Entries)
    End Sub

    <Fact>
    Public Sub Filtrul_DouăAxeCombinate_SeAplicăCaȘI()
        Dim entries = BuildSet()
        Dim f As New LogFilter() With {
            .Files = New HashSet(Of String)({"a.log"}),
            .Levels = New HashSet(Of KBotLogLevel)({KBotLogLevel.Error})}

        Dim r As LogFilterResult = f.Apply(entries)

        Assert.Single(r.Entries)
        Assert.Equal("a.log", r.Entries(0).FileName)
        Assert.Equal(KBotLogLevel.Error, r.Entries(0).Level)
    End Sub

    <Fact>
    Public Sub Filtrul_MulțimeGoală_ÎnseamnăNIMIC_NuTOATE()
        ' Regula e fixată în AMBELE sensuri: goală = nimic, Nothing = axa inactivă.
        Dim entries = BuildSet()

        Assert.Empty(New LogFilter() With {.Levels = New HashSet(Of KBotLogLevel)()}.Apply(entries).Entries)
        Assert.Empty(New LogFilter() With {.Files = New HashSet(Of String)()}.Apply(entries).Entries)
        Assert.Equal(4, New LogFilter().Apply(entries).ShownCount)
    End Sub

    <Fact>
    Public Sub Filtrul_IntervalInclusivLaAmbeleCapete()
        Dim entries As New List(Of LogEntry) From {
            NewEntry(New Date(2026, 8, 12), KBotLogLevel.Info, "a.log", "înainte"),
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "a.log", "chiar la capătul de jos"),
            NewEntry(New Date(2026, 8, 14), KBotLogLevel.Info, "a.log", "chiar la capătul de sus"),
            NewEntry(New Date(2026, 8, 15), KBotLogLevel.Info, "a.log", "după")}

        Dim f As New LogFilter() With {
            .FromDate = New Date(2026, 8, 13),
            .ToDate = New Date(2026, 8, 14)}

        Assert.Equal(2, f.Apply(entries).ShownCount)
    End Sub

    <Fact>
    Public Sub Filtrul_IntrărileFărăDată_SuntExcluseDeInterval_ȘiNUMĂRATE()
        ' Excluderea tăcută nu e acceptabilă: bara de stare trebuie să poată spune câte au ieșit.
        Dim entries As New List(Of LogEntry) From {
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "a.log", "cu dată"),
            NewEntry(Nothing, KBotLogLevel.Info, "a.log", "fără dată 1"),
            NewEntry(Nothing, KBotLogLevel.Info, "a.log", "fără dată 2")}

        Dim f As New LogFilter() With {.FromDate = New Date(2026, 8, 1)}
        Dim r As LogFilterResult = f.Apply(entries)

        Assert.Single(r.Entries)
        Assert.Equal(2, r.ExcludedWithoutTimestamp)
    End Sub

    <Fact>
    Public Sub Filtrul_FărăInterval_IntrărileFărăDatăRămân()
        Dim entries As New List(Of LogEntry) From {
            NewEntry(Nothing, KBotLogLevel.Info, "a.log", "fără dată")}

        Dim r As LogFilterResult = New LogFilter().Apply(entries)

        Assert.Single(r.Entries)
        Assert.Equal(0, r.ExcludedWithoutTimestamp)
    End Sub

    ' ── Testul 11: ceasul serverului ─────────────────────────────────────────

    <Fact>
    Public Sub Ceasul_FărăCitire_NuCorecteazăNimic()
        Assert.False(ServerClock.HasReading)
        Assert.Equal(TimeSpan.Zero, ServerClock.Offset)
        Assert.Equal(String.Empty, ServerClock.OffsetText())
    End Sub

    <Fact>
    Public Sub Ceasul_LinieVeche_FolseșteDecalajul()
        ' Server cu trei ore înaintea noastră.
        ServerClock.Update(DateTimeOffset.Now.AddHours(3))

        Dim e As LogEntry = NewEntry(New Date(2026, 8, 13, 14, 0, 0), KBotLogLevel.Info, "api.log", "veche")
        e.TimestampNeedsClockCorrection = True

        Dim corrected As Date? = ServerClock.ToClientLocal(e)

        ' 14:00 la server = 11:00 la noi. Comparație CU TOLERANȚĂ, nu la egalitate: decalajul se
        ' calculează din două citiri de ceas diferite, deci poartă zgomot de ordinul microsecundelor
        ' prin construcție. Exact motivul pentru care dus-întorsul nu se compensează — o egalitate
        ' strictă aici ar pretinde o precizie pe care mecanismul nu o are.
        Assert.True(Math.Abs((corrected.Value - New Date(2026, 8, 13, 11, 0, 0)).TotalSeconds) < 1.0,
                    "Marcajul corectat ar trebui să fie 11:00 ± 1s, dar e " & corrected.Value.ToString("O"))
    End Sub

    <Fact>
    Public Sub Ceasul_LinieCareÎșiPoartăDecalajul_ESTEIgnoratDeCeas()
        ServerClock.Update(DateTimeOffset.Now.AddHours(3))

        Dim e As LogEntry = NewEntry(New Date(2026, 8, 13, 14, 0, 0), KBotLogLevel.Info, "api.log", "nouă")
        ' TimestampNeedsClockCorrection rămâne False: analizorul a convertit deja.

        Assert.Equal(New Date(2026, 8, 13, 14, 0, 0), ServerClock.ToClientLocal(e).Value)
    End Sub

    <Fact>
    Public Sub Ceasul_TextulDecalajului_SeFormateazăCuSemn()
        ServerClock.Update(DateTimeOffset.Now.AddHours(3))
        Assert.StartsWith("+", ServerClock.OffsetText())

        ServerClock.Update(DateTimeOffset.Now.AddHours(-2))
        Assert.StartsWith("-", ServerClock.OffsetText())
    End Sub

    <Fact>
    Public Sub Ceasul_DecalajSubOSecundă_NuSeAfișează()
        ' Un «+00:00» permanent în bara de stare e zgomot.
        ServerClock.Update(DateTimeOffset.Now)
        Assert.Equal(String.Empty, ServerClock.OffsetText())
    End Sub

    <Fact>
    Public Sub Filtrul_IntervalulSeComparăPeMarcajulCORECTAT()
        ' Server cu trei ore înainte. O linie veche stampilată 14:00 ora serverului e 11:00 la noi,
        ' deci trebuie să intre într-un interval 10:00–12:00 LOCAL.
        ServerClock.Update(DateTimeOffset.Now.AddHours(3))

        Dim e As LogEntry = NewEntry(New Date(2026, 8, 13, 14, 0, 0), KBotLogLevel.Info, "api.log", "veche")
        e.TimestampNeedsClockCorrection = True

        Dim f As New LogFilter() With {
            .FromDate = New Date(2026, 8, 13, 10, 0, 0),
            .ToDate = New Date(2026, 8, 13, 12, 0, 0)}

        Assert.Single(f.Apply(New List(Of LogEntry) From {e}).Entries)
    End Sub

    ' ── ajutoare ─────────────────────────────────────────────────────────────

    Private Shared Function NewEntry(stamp As Date?, level As KBotLogLevel, fileName As String, message As String) As LogEntry
        Dim e As New LogEntry(stamp, level, "src", message, message)
        e.FileName = fileName
        Return e
    End Function

    Private Shared Function BuildSet() As List(Of LogEntry)
        Return New List(Of LogEntry) From {
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Error, "a.log", "eroare în a"),
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "a.log", "info în a"),
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Error, "b.log", "eroare în b"),
            NewEntry(New Date(2026, 8, 13), KBotLogLevel.Info, "b.log", "info în b")}
    End Function

End Class
