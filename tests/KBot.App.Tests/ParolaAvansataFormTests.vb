Option Strict On
Imports Xunit
Imports KBot.App

' The advanced options password (operator, 24.09.2026): exact and case-sensitive.
Public Class ParolaAvansataFormTests

    <Fact>
    Public Sub ThePassword_IsAccepted()
        Assert.True(ParolaAvansataForm.IsPassword("andreI"))
    End Sub

    <Theory>
    <InlineData("andrei")>
    <InlineData("ANDREI")>
    <InlineData("andreI ")>
    <InlineData(" andreI")>
    <InlineData("")>
    <InlineData(Nothing)>
    Public Sub AnythingElse_IsRefused(text As String)
        Assert.False(ParolaAvansataForm.IsPassword(text))
    End Sub

End Class
