Option Strict On
Imports System
Imports System.Linq
Imports Xunit
Imports KBot.Updater

' Slice 0067: the command line between KBot.App and KBot.Updater. Both sides are
' in this repo, but the parser is what turns a typo into a message instead of a
' half-applied update, so every rule is pinned.
Public Class UpdaterArgsTests

    <Fact>
    Public Sub Parse_ReadsEveryOption()
        Dim a = UpdaterArgs.Parse({
            "--zip", "C:\Temp\KBot_1.0.31.0.zip",
            "--target", "C:\KBOT",
            "--wait", "4321",
            "--restart", "C:\KBOT\KBot.App.exe",
            "--sha256", New String("a"c, 64),
            "--version", "1.0.31.0",
            "--elevated"})

        Assert.Equal("C:\Temp\KBot_1.0.31.0.zip", a.ZipPath)
        Assert.Equal("C:\KBOT", a.TargetDir)
        Assert.Equal(4321, a.WaitPid)
        Assert.Equal("C:\KBOT\KBot.App.exe", a.RestartExe)
        Assert.Equal(New String("a"c, 64), a.Sha256)
        Assert.Equal("1.0.31.0", a.Version)
        Assert.True(a.Elevated)
    End Sub

    <Fact>
    Public Sub Parse_OnlyZipAndTargetAreRequired()
        Dim a = UpdaterArgs.Parse({"--zip", "C:\x\p.zip", "--target", "C:\KBOT"})
        Assert.Equal(0, a.WaitPid)
        Assert.Null(a.RestartExe)
        Assert.Null(a.Sha256)
        Assert.False(a.Elevated)
    End Sub

    <Theory>
    <InlineData(New String() {})>
    <InlineData(New String() {"--zip", "C:\x\p.zip"})>
    <InlineData(New String() {"--target", "C:\KBOT"})>
    <InlineData(New String() {"--zip", "p.zip", "--target", "C:\KBOT"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "KBOT"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--restart", "KBot.App.exe"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--wait", "abc"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--wait", "-5"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--sha256", "abc"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--wait"})>
    <InlineData(New String() {"--zip", "C:\x\p.zip", "--target", "C:\KBOT", "--bogus", "1"})>
    Public Sub Parse_RejectsBadCommandLines(argv As String())
        Assert.Throws(Of ArgumentException)(Sub() UpdaterArgs.Parse(argv))
    End Sub

    <Fact>
    Public Sub Parse_NullThrowsArgumentNull()
        Assert.Throws(Of ArgumentNullException)(Sub() UpdaterArgs.Parse(Nothing))
    End Sub

    <Fact>
    Public Sub ToArgumentList_RoundTrips_AndCanMarkElevated()
        Dim original = UpdaterArgs.Parse({
            "--zip", "C:\Temp\p.zip", "--target", "C:\KBOT", "--wait", "7",
            "--restart", "C:\KBOT\KBot.App.exe", "--sha256", New String("b"c, 64), "--version", "1.2.3.4"})
        Assert.False(original.Elevated)

        Dim again = UpdaterArgs.Parse(original.ToArgumentList(markElevated:=True).ToArray())

        Assert.Equal(original.ZipPath, again.ZipPath)
        Assert.Equal(original.TargetDir, again.TargetDir)
        Assert.Equal(original.WaitPid, again.WaitPid)
        Assert.Equal(original.RestartExe, again.RestartExe)
        Assert.Equal(original.Sha256, again.Sha256)
        Assert.Equal(original.Version, again.Version)
        Assert.True(again.Elevated)
    End Sub

    <Fact>
    Public Sub ToArgumentList_OmitsWhatWasNotGiven()
        Dim a = UpdaterArgs.Parse({"--zip", "C:\x\p.zip", "--target", "C:\KBOT"})
        Dim list = a.ToArgumentList()
        Assert.Equal({"--zip", "C:\x\p.zip", "--target", "C:\KBOT"}, list.ToArray())
    End Sub
End Class
