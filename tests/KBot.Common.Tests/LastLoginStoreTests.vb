Option Strict On
Imports System
Imports System.IO
Imports Xunit
Imports KBot.Common

''' <summary>
''' The last-login memory (slice 0063): what the login form gets back, and what must never
''' end up in the file. Every test works on a temporary folder passed explicitly, so
''' nothing touches the machine's %APPDATA%.
''' </summary>
Public Class LastLoginStoreTests

    Private Shared Function TempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "kbot_login_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(d)
        Return d
    End Function

    <Fact>
    Public Sub MissingFile_IsAnEmptyStore()
        Dim s As LastLoginStore = LastLoginStore.Load(TempDir())
        Assert.Null(s.Username)
        Assert.Null(s.UnitDc)
    End Sub

    <Fact>
    Public Sub Save_ThenLoad_RoundTripsUserAndUnit()
        Dim dir As String = TempDir()
        LastLoginStore.Save("  op@example.com ", " 1234 ", dir)
        Dim s As LastLoginStore = LastLoginStore.Load(dir)
        Assert.Equal("op@example.com", s.Username)
        Assert.Equal("1234", s.UnitDc)
    End Sub

    <Fact>
    Public Sub Save_WithoutUnit_KeepsOnlyTheUser()
        Dim dir As String = TempDir()
        LastLoginStore.Save("op@example.com", Nothing, dir)
        Dim s As LastLoginStore = LastLoginStore.Load(dir)
        Assert.Equal("op@example.com", s.Username)
        Assert.Null(s.UnitDc)
    End Sub

    <Fact>
    Public Sub Save_OverwritesThePreviousUser()
        Dim dir As String = TempDir()
        LastLoginStore.Save("first@example.com", "1", dir)
        LastLoginStore.Save("second@example.com", "2", dir)
        Dim s As LastLoginStore = LastLoginStore.Load(dir)
        Assert.Equal("second@example.com", s.Username)
        Assert.Equal("2", s.UnitDc)
    End Sub

    <Fact>
    Public Sub Save_EmptyUser_Throws()
        Assert.Throws(Of ArgumentException)(Sub() LastLoginStore.Save("  ", "1", TempDir()))
    End Sub

    <Fact>
    Public Sub BrokenJson_IsAnEmptyStore_NotAnException()
        Dim dir As String = TempDir()
        File.WriteAllText(LastLoginStore.FilePath(dir), "{ not json")
        Dim s As LastLoginStore = LastLoginStore.Load(dir)
        Assert.Null(s.Username)
    End Sub

    <Fact>
    Public Sub WrongShape_IsAnEmptyStore()
        Dim dir As String = TempDir()
        File.WriteAllText(LastLoginStore.FilePath(dir), "[1, 2]")
        Assert.Null(LastLoginStore.Load(dir).Username)
        File.WriteAllText(LastLoginStore.FilePath(dir), "{ ""Username"": 7 }")
        Assert.Null(LastLoginStore.Load(dir).Username)
    End Sub

    <Fact>
    Public Sub TheFile_NeverContainsAPassword()
        ' The API takes only the two identifiers, so the file can only ever hold those.
        Dim dir As String = TempDir()
        LastLoginStore.Save("op@example.com", "1", dir)
        Dim text As String = File.ReadAllText(LastLoginStore.FilePath(dir))
        Assert.Contains("Username", text)
        Assert.Contains("UnitDc", text)
        Assert.DoesNotContain("assword", text)
        Assert.DoesNotContain("oken", text)
    End Sub

    <Fact>
    Public Sub DefaultFolder_IsThePerUserSettingsFolder()
        Assert.Equal(Path.Combine(SetariFoldere.DirectorSetari(), LastLoginStore.FileName),
                     LastLoginStore.FilePath())
    End Sub
End Class
