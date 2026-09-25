Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports Xunit

' Which Adobe script alerts the save trap closes (operator, 24.09.2026): only those matching the
' operator's list. Texts below are the real ones from the working logs.
Public Class AdobeScriptAlertFilterTests

    Private Shared Function Defaults() As IReadOnlyList(Of System.Text.RegularExpressions.Regex)
        Return AdobeScriptAlertFilter.Compile(AppSettings.DefaultAdobeTrappedAlerts())
    End Function

    <Theory>
    <InlineData("GeneralErrorOperation failed.")>
    <InlineData("GeneralError" & vbCrLf & "Operation failed.")>
    <InlineData("TypeErrorsumCell5.toFixed is not a function")>
    Public Sub ScriptErrors_SeenInTheLogs_AreTrappedByTheDefaults(message As String)
        Assert.NotNull(AdobeScriptAlertFilter.FirstMatch(message, Defaults()))
    End Sub

    <Theory>
    <InlineData("Validarea s-a terminat cu succes! Semnati formularul pentru a completa coloanele 1-3 din tabel apoi validati formularul pentru a putea aplica urmatoarea semnatura ")>
    <InlineData("Validarea s-a terminat cu succes! A fost atasat fisierul NOTAFD.xml.")>
    <InlineData("Nu sunt erori la sectiunea A")>
    <InlineData("Completati sectiunea B, validati_o pentru a genera fisierul xml, apoi semnati formularul")>
    Public Sub OperatorMessages_AreLeftOnScreenByTheDefaults(message As String)
        Assert.Null(AdobeScriptAlertFilter.FirstMatch(message, Defaults()))
    End Sub

    <Fact>
    Public Sub Patterns_AreCaseInsensitive_AndMatchAnywhere()
        Dim rules = AdobeScriptAlertFilter.Compile({"operation FAILED"})
        Assert.Equal("operation FAILED", AdobeScriptAlertFilter.FirstMatch("xx Operation failed. yy", rules))
    End Sub

    <Fact>
    Public Sub RegularExpressions_Work()
        Dim rules = AdobeScriptAlertFilter.Compile({"^Nu sunt erori( la sectiunea [AB])?$"})
        Assert.NotNull(AdobeScriptAlertFilter.FirstMatch("Nu sunt erori la sectiunea B", rules))
        Assert.Null(AdobeScriptAlertFilter.FirstMatch("Atentie: Nu sunt erori", rules))
    End Sub

    <Fact>
    Public Sub LineBreaksInTheMessage_CountAsOneSpace()
        Dim rules = AdobeScriptAlertFilter.Compile({"succes! Semnati"})
        Assert.NotNull(AdobeScriptAlertFilter.FirstMatch("Validarea s-a terminat cu succes!" & vbCrLf & vbCrLf & "Semnati", rules))
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("   ")>
    <InlineData("(unclosed")>
    <InlineData(".*")>
    Public Sub BrokenOrMatchEverythingPatterns_AreRefused(pattern As String)
        Assert.NotEqual(String.Empty, AdobeScriptAlertFilter.CheckPattern(pattern))
    End Sub

    <Fact>
    Public Sub BrokenPatterns_AreSkippedWhenCompiled()
        Dim rules = AdobeScriptAlertFilter.Compile({"(unclosed", "TypeError", ""})
        Assert.Single(rules)
    End Sub

    <Fact>
    Public Sub EmptyList_TrapsNothing()
        Assert.Null(AdobeScriptAlertFilter.FirstMatch("GeneralErrorOperation failed.", AdobeScriptAlertFilter.Compile({})))
    End Sub

    <Fact>
    Public Sub EmptyMessage_IsNeverTrapped()
        Assert.Null(AdobeScriptAlertFilter.FirstMatch("", Defaults()))
    End Sub

End Class
