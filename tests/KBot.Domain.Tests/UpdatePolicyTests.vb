Option Strict On
Imports System
Imports Xunit
Imports KBot.Domain

' Slice 0067: the version comparison behind every update prompt. Pure function,
' so every branch is pinned here -- including the System.Version trap where a
' three-part "1.0.30" compares BELOW "1.0.30.0" unless padded.
Public Class UpdatePolicyTests

    <Theory>
    <InlineData("1.0.30.0", "1.0.30.0", "1.0.0.0", UpdateDecision.UpToDate)>
    <InlineData("1.0.31.0", "1.0.30.0", "1.0.0.0", UpdateDecision.UpToDate)>
    <InlineData("1.0.30.0", "1.0.31.0", "1.0.0.0", UpdateDecision.Available)>
    <InlineData("1.0.30.0", "1.0.31.0", "1.0.30.0", UpdateDecision.Available)>
    <InlineData("1.0.29.0", "1.0.31.0", "1.0.30.0", UpdateDecision.Required)>
    <InlineData("1.0.30.0", "1.0.31.0", "1.0.31.0", UpdateDecision.Required)>
    <InlineData("0.9.99.9", "1.0.0.0", "1.0.0.0", UpdateDecision.Required)>
    Public Sub Decide_CoversTheThreeOutcomes(current As String, latest As String, minimum As String,
                                             expected As UpdateDecision)
        Assert.Equal(expected, UpdatePolicy.Decide(Version.Parse(current), latest, minimum))
    End Sub

    <Fact>
    Public Sub Decide_MinimumOutranksLatest_WhenClientSkippedReleases()
        ' 1.0.28 skipped optional 1.0.29 and 1.0.30; 1.0.31 was pushed with -Mandatory
        ' (minimum = 1.0.31). The skipped releases do not matter: below minimum = Required.
        Assert.Equal(UpdateDecision.Required,
                     UpdatePolicy.Decide(New Version(1, 0, 28, 0), "1.0.31.0", "1.0.31.0"))
    End Sub

    <Theory>
    <InlineData("1.0.30", "1.0.30.0")>
    <InlineData("1.0", "1.0.0.0")>
    <InlineData("1.0.30.0", "1.0.30")>
    Public Sub Decide_TreatsMissingPartsAsZero(current As String, latest As String)
        ' Without Normalize, "1.0.30" < "1.0.30.0" and the client would be told to
        ' update to the version it already runs.
        Assert.Equal(UpdateDecision.UpToDate,
                     UpdatePolicy.Decide(Version.Parse(current), latest, "0.0.0.0"))
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("   ")>
    <InlineData("abc")>
    <InlineData("1")>
    <InlineData("1.0.30.0.5")>
    Public Sub Decide_ThrowsOnUnparsableServerVersion(bad As String)
        Assert.Throws(Of ArgumentException)(
            Sub() UpdatePolicy.Decide(New Version(1, 0, 30, 0), bad, "1.0.0.0"))
        Assert.Throws(Of ArgumentException)(
            Sub() UpdatePolicy.Decide(New Version(1, 0, 30, 0), "1.0.31.0", bad))
    End Sub

    <Fact>
    Public Sub Decide_ThrowsOnNullServerVersion()
        Assert.Throws(Of ArgumentException)(
            Sub() UpdatePolicy.Decide(New Version(1, 0, 30, 0), CStr(Nothing), "1.0.0.0"))
    End Sub

    <Fact>
    Public Sub Decide_ThrowsOnNullCurrent()
        Assert.Throws(Of ArgumentNullException)(
            Sub() UpdatePolicy.Decide(CType(Nothing, Version), "1.0.31.0", "1.0.0.0"))
    End Sub

    <Fact>
    Public Sub Normalize_PadsToFourParts()
        Assert.Equal(New Version(1, 2, 0, 0), UpdatePolicy.Normalize(New Version(1, 2)))
        Assert.Equal(New Version(1, 2, 3, 0), UpdatePolicy.Normalize(New Version(1, 2, 3)))
        Assert.Equal(New Version(1, 2, 3, 4), UpdatePolicy.Normalize(New Version(1, 2, 3, 4)))
    End Sub

    <Fact>
    Public Sub ParseVersion_TrimsWhitespace()
        Assert.Equal(New Version(1, 0, 31, 0), UpdatePolicy.ParseVersion("  1.0.31.0 ", "x"))
    End Sub
End Class
