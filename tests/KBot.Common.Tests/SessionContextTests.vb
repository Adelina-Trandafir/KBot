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

    Private Shared Function SampleDto() As SessionContextDto
        Return New SessionContextDto With {
            .DbName = "000_DEMO",
            .IdUnitate = 136,
            .ANL = 2026,
            .CodProgram = "P01",
            .SectorSursa = "02A",
            .CF = "123456",
            .NumeUnitate = "DEMO",
            .Role = "Contabil"
        }
    End Function

    <Fact>
    Public Sub Populate_MapeazaTotSiMarcheazaAutentificat()
        Dim s As New SessionContext()
        s.Populate("000_DEMO_Contabil", 42, SampleDto())

        Assert.Equal("000_DEMO_Contabil", s.OperatorName)
        Assert.Equal(42, s.SessionId)
        Assert.Equal("000_DEMO", s.DbName)
        Assert.Equal(136, s.IdUnitate)
        Assert.Equal(2026, s.An)                 ' DTO.ANL -> An
        Assert.Equal("P01", s.CodProgram)
        Assert.Equal("02A", s.SectorSursa)
        Assert.Equal("123456", s.CF)
        Assert.Equal("DEMO", s.NumeUnitate)
        Assert.Equal("Contabil", s.Role)
        Assert.True(s.IsAuthenticated)
        Assert.True(s.IsLoaded)                  ' derivat din CF
    End Sub

    <Fact>
    Public Sub Populate_SessionIdInvalid_Arunca()
        Dim s As New SessionContext()
        Assert.Throws(Of ArgumentOutOfRangeException)(
            Sub() s.Populate("u", 0, SampleDto()))
    End Sub

    <Fact>
    Public Sub Populate_CtxNothing_Arunca()
        Dim s As New SessionContext()
        Assert.Throws(Of ArgumentNullException)(
            Sub() s.Populate("u", 1, Nothing))
    End Sub

    <Fact>
    Public Sub Clear_ReseteazaStareaSiCampurile()
        Dim s As New SessionContext()
        s.Populate("000_DEMO_Contabil", 42, SampleDto())

        s.Clear()

        Assert.False(s.IsAuthenticated)
        Assert.Equal(0, s.SessionId)
        Assert.Equal(String.Empty, s.Role)
        Assert.Equal(String.Empty, s.DbName)
        Assert.Equal(0, s.IdUnitate)
        Assert.Equal(0, s.An)
        Assert.Equal(String.Empty, s.CF)
        Assert.False(s.IsLoaded)
    End Sub
End Class
