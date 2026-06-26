Imports Xunit
Imports KBot.Common

Public Class SessionContextTests
    <Fact>
    Public Sub IsLoaded_FalsCandCFGol()
        Dim s As New SessionContext()
        Assert.False(s.IsLoaded)
    End Sub
End Class
