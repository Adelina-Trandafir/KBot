Imports System.IO
Imports KBot.Theming
Imports Xunit

' Persistența SetScheme scrie în AVACONT — redirijăm către temp ca să nu atingem %AppData%.
Public Class ThemeManagerTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_thememgr_test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_tempRoot)
        ThemeStore.OverrideRootForTests = _tempRoot
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Revenim la o schemă neutră ca să nu contaminăm alte clase de test.
        ThemeManager.SetScheme(BuiltInSchemes.Classic())
        ThemeStore.OverrideRootForTests = Nothing
        Try
            If Directory.Exists(_tempRoot) Then Directory.Delete(_tempRoot, True)
        Catch
        End Try
    End Sub

    <Fact>
    Public Sub SetScheme_UpdatesCurrent()
        ThemeManager.SetScheme(BuiltInSchemes.Dark())
        Assert.Equal(BuiltInSchemes.DarkName, ThemeManager.Current.Name)
        Assert.True(ThemeManager.Current.IsDark)

        ThemeManager.SetScheme(BuiltInSchemes.Modern())
        Assert.Equal(BuiltInSchemes.ModernName, ThemeManager.Current.Name)
        Assert.False(ThemeManager.Current.IsDark)
    End Sub

    <Fact>
    Public Sub SetScheme_RaisesThemeChangedExactlyOnce()
        Dim count As Integer = 0
        Dim handler As EventHandler = Sub(s, e) count += 1
        AddHandler ThemeManager.ThemeChanged, handler
        Try
            ThemeManager.SetScheme(BuiltInSchemes.Dark())
        Finally
            RemoveHandler ThemeManager.ThemeChanged, handler
        End Try
        Assert.Equal(1, count)
    End Sub

    <Fact>
    Public Sub SetScheme_Null_Throws()
        Assert.Throws(Of ArgumentNullException)(Sub() ThemeManager.SetScheme(Nothing))
    End Sub

    <Fact>
    Public Sub ResolveByName_KnownReturnsScheme_UnknownReturnsNothing()
        Assert.Equal(BuiltInSchemes.DarkName, ThemeManager.ResolveByName("Dark").Name)
        Assert.Equal(BuiltInSchemes.ClassicName, ThemeManager.ResolveByName("classic").Name)   ' case-insensitive
        Assert.Null(ThemeManager.ResolveByName("NoSuchScheme"))   ' → apelantul cade pe Classic
        Assert.Null(ThemeManager.ResolveByName(Nothing))
    End Sub

End Class
