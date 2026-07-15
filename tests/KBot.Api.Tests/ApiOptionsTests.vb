Imports Xunit
Imports KBot.Api

Public Class ApiOptionsTests
    <Fact>
    Public Sub Defaults_TimeoutSiRetry()
        Dim o As New ApiOptions()
        Assert.Equal(100, o.TimeoutSeconds)
        Assert.Equal(3, o.MaxRetries)
    End Sub

    ' Adresa de producție e singura sursă de adevăr — și e https.
    <Fact>
    Public Sub DefaultBaseUrl_EProductieHttps()
        Assert.Equal("https://kbot.avatarsoft.ro", ApiOptions.DefaultBaseUrl)
        Call New ApiOptions().EnsureHttpsBaseUrl()
    End Sub

    ' Garda de pornire: o adresă http necriptată oprește aplicația, nu o lasă să ruleze.
    <Theory>
    <InlineData("http://kbot.avatarsoft.ro")>
    <InlineData("HTTP://kbot.avatarsoft.ro")>
    <InlineData("ftp://kbot.avatarsoft.ro")>
    <InlineData("kbot.avatarsoft.ro")>
    Public Sub EnsureHttpsBaseUrl_AruncaPeAdresaNeHttps(url As String)
        Dim o As New ApiOptions() With {.BaseUrl = url}
        Dim ex = Assert.Throws(Of InvalidOperationException)(Sub() o.EnsureHttpsBaseUrl())
        Assert.Contains(url, ex.Message)
    End Sub

    <Theory>
    <InlineData(CStr(Nothing))>
    <InlineData("")>
    <InlineData("   ")>
    Public Sub EnsureHttpsBaseUrl_AruncaPeAdresaLipsa(url As String)
        Dim o As New ApiOptions() With {.BaseUrl = url}
        Assert.Throws(Of InvalidOperationException)(Sub() o.EnsureHttpsBaseUrl())
    End Sub

    ' Majusculele din schemă sunt acceptate (comparație OrdinalIgnoreCase).
    <Fact>
    Public Sub EnsureHttpsBaseUrl_AcceptaHttpsIndiferentDeMajuscule()
        Call New ApiOptions() With {.BaseUrl = "HTTPS://kbot.avatarsoft.ro"}.EnsureHttpsBaseUrl()
    End Sub
End Class
