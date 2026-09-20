Option Strict On
Imports System
Imports Xunit
Imports KBot.Updater

' Slice 0067-01: the "Programs and Features" entry refresh. HKLM is never touched here;
' only the pure pieces are pinned (the name rewrite and the folder match). The registry
' write itself is seen on a real update, in Logs\updater.log.
Public Class InstallRegistryTests

    <Fact>
    Public Sub NewDisplayName_ReplacesTheTrailingVersion()
        Assert.Equal("K-BOT 1.0.31.0", InstallRegistry.NewDisplayName("K-BOT 1.0.30.0", "1.0.30.0", "1.0.31.0"))
    End Sub

    <Fact>
    Public Sub NewDisplayName_LeavesANameThatDoesNotEndWithTheOldVersion()
        Assert.Null(InstallRegistry.NewDisplayName("K-BOT", "1.0.30.0", "1.0.31.0"))
        Assert.Null(InstallRegistry.NewDisplayName("K-BOT 1.0.30.0 (beta)", "1.0.30.0", "1.0.31.0"))
    End Sub

    <Fact>
    Public Sub NewDisplayName_NothingWhenTheOldValuesAreMissing()
        Assert.Null(InstallRegistry.NewDisplayName(Nothing, "1.0.30.0", "1.0.31.0"))
        Assert.Null(InstallRegistry.NewDisplayName("K-BOT 1.0.30.0", Nothing, "1.0.31.0"))
    End Sub

    <Fact>
    Public Sub NewDisplayName_RefusesAnEmptyNewVersion()
        Assert.Throws(Of ArgumentException)(Function() InstallRegistry.NewDisplayName("K-BOT 1.0.30.0", "1.0.30.0", " "))
    End Sub

    <Fact>
    Public Sub SameFolder_IgnoresCaseAndTrailingSeparators()
        Assert.True(InstallRegistry.SameFolder("C:\KBOT\", "c:\kbot"))
        Assert.True(InstallRegistry.SameFolder("C:\KBOT", "C:\KBOT\"))
        Assert.False(InstallRegistry.SameFolder("C:\KBOT", "C:\KBOT2"))
    End Sub

    <Fact>
    Public Sub SameFolder_FalseWhenEitherIsEmpty()
        Assert.False(InstallRegistry.SameFolder(Nothing, "C:\KBOT"))
        Assert.False(InstallRegistry.SameFolder("C:\KBOT", ""))
    End Sub

    <Fact>
    Public Sub KeyPath_IsTheInnoUninstallKeyOfTheInstallerAppId()
        Assert.EndsWith("_is1", InstallRegistry.KEY_PATH)
        Assert.Contains("{A9840797-CE1E-4708-BDC0-73E4CCBC2D3A}", InstallRegistry.KEY_PATH)
    End Sub
End Class
