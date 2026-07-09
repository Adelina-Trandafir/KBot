Imports System.IO
Imports KBot.Theming
Imports Xunit

' Redirijează rădăcina AVACONT către un director temporar unic; curăță la Dispose.
Public Class ThemeStoreTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_theme_test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_tempRoot)
        ThemeStore.OverrideRootForTests = _tempRoot
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ThemeStore.OverrideRootForTests = Nothing
        Try
            If Directory.Exists(_tempRoot) Then Directory.Delete(_tempRoot, True)
        Catch
        End Try
    End Sub

    <Fact>
    Public Sub SaveActive_ThenLoadActiveName_RoundTrips()
        ThemeStore.SaveActive("Modern")
        Assert.Equal("Modern", ThemeStore.LoadActiveName())
        Assert.True(File.Exists(ThemeStore.ActiveFilePath))
    End Sub

    <Fact>
    Public Sub LoadActiveName_MissingFile_ReturnsNothing()
        ' Nimic salvat încă => Nothing (apelantul cade pe Classic).
        Assert.Null(ThemeStore.LoadActiveName())
    End Sub

    <Fact>
    Public Sub LoadUserSchemes_SkipsMalformed_KeepsValid()
        Directory.CreateDirectory(ThemeStore.ThemesFolder)
        File.WriteAllText(Path.Combine(ThemeStore.ThemesFolder, "good.json"), "{""Name"":""MyTheme"",""IsDark"":false}")
        File.WriteAllText(Path.Combine(ThemeStore.ThemesFolder, "broken.json"), "{ this is not valid json ")

        Dim schemes = ThemeStore.LoadUserSchemes()   ' nu trebuie să arunce
        Assert.Single(schemes)
        Assert.Equal("MyTheme", schemes(0).Name)
    End Sub

    <Fact>
    Public Sub LoadUserSchemes_NoFolder_ReturnsEmpty()
        Assert.Empty(ThemeStore.LoadUserSchemes())
    End Sub

End Class
