Imports Xunit
Imports KBot.Api

Public Class ApiOptionsTests
    <Fact>
    Public Sub Defaults_TimeoutSiRetry()
        Dim o As New ApiOptions()
        Assert.Equal(100, o.TimeoutSeconds)
        Assert.Equal(3, o.MaxRetries)
    End Sub
End Class
