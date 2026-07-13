Imports System
Imports Xunit
Imports KBot.Common
Imports KBot.Domain

Public Class SessionContextTests
    <Fact>
    Public Sub IsLoaded_FalsCandCFGol()
        Dim s As New SessionContext()
        Assert.False(s.IsLoaded)
    End Sub

    <Fact>
    Public Sub IsAuthenticated_FalsFaraToken()
        Dim s As New SessionContext()
        Assert.False(s.IsAuthenticated)
    End Sub

    Private Shared Function SampleDto() As SessionContextDto
        Return New SessionContextDto With {
            .DbName = "000_DEMO",
            .CF = "123456",
            .NumeUnitate = "DEMO",
            .Role = "Contabil"
        }
    End Function

    <Fact>
    Public Sub Populate_MapeazaIdentitateaSiMarcheazaAutentificat()
        Dim s As New SessionContext()
        s.Populate("000_DEMO_Contabil", "tok-opaque-123", SampleDto())

        Assert.Equal("000_DEMO_Contabil", s.OperatorName)
        Assert.Equal("tok-opaque-123", s.Token)
        Assert.Equal("000_DEMO", s.DbName)
        Assert.Equal("123456", s.CF)
        Assert.Equal("DEMO", s.NumeUnitate)
        Assert.Equal("Contabil", s.Role)
        ' Anul / SS / CodProgram NU vin din login — sunt runtime (SetPeriod).
        Assert.Equal(0, s.An)
        Assert.Equal(String.Empty, s.SectorSursa)
        Assert.Equal(String.Empty, s.CodProgram)
        Assert.True(s.IsAuthenticated)           ' derivat din Token
        Assert.True(s.IsLoaded)                  ' derivat din CF
    End Sub

    <Fact>
    Public Sub SetPeriod_FixeazaAnSsCodProgram()
        Dim s As New SessionContext()
        s.Populate("000_DEMO_Contabil", "tok-opaque-123", SampleDto())

        s.SetPeriod(2026, "02A", "P01")

        Assert.Equal(2026, s.An)
        Assert.Equal("02A", s.SectorSursa)
        Assert.Equal("P01", s.CodProgram)
    End Sub

    <Fact>
    Public Sub Populate_TokenGol_Arunca()
        Dim s As New SessionContext()
        Assert.Throws(Of ArgumentException)(
            Sub() s.Populate("u", String.Empty, SampleDto()))
        Assert.Throws(Of ArgumentException)(
            Sub() s.Populate("u", Nothing, SampleDto()))
    End Sub

    <Fact>
    Public Sub Populate_CtxNothing_Arunca()
        Dim s As New SessionContext()
        Assert.Throws(Of ArgumentNullException)(
            Sub() s.Populate("u", "tok", Nothing))
    End Sub

    <Fact>
    Public Sub Clear_ReseteazaStareaSiCampurile()
        Dim s As New SessionContext()
        s.Populate("000_DEMO_Contabil", "tok-opaque-123", SampleDto())
        s.SetPeriod(2026, "02A", "P01")

        s.Clear()

        Assert.False(s.IsAuthenticated)
        Assert.Equal(String.Empty, s.Token)
        Assert.Equal(String.Empty, s.Role)
        Assert.Equal(String.Empty, s.DbName)
        Assert.Equal(0, s.An)
        Assert.Equal(String.Empty, s.SectorSursa)
        Assert.Equal(String.Empty, s.CodProgram)
        Assert.Equal(String.Empty, s.CF)
        Assert.False(s.IsLoaded)
    End Sub
End Class
