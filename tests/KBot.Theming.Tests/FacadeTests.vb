Imports System.IO
Imports KBot.Theming
Imports Xunit

' KBotTheme (facada, în KBot.Forexe, namespace global) trebuie să delege corect la
' ThemeManager fără nicio editare la call-site-uri. Verificăm contractul CLR_* → slot.
Public Class FacadeTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_facade_test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_tempRoot)
        ThemeStore.OverrideRootForTests = _tempRoot
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ThemeManager.SetScheme(BuiltInSchemes.Classic())
        ThemeStore.OverrideRootForTests = Nothing
        Try
            If Directory.Exists(_tempRoot) Then Directory.Delete(_tempRoot, True)
        Catch
        End Try
    End Sub

    <Fact>
    Public Sub AfterSetSchemeDark_FacadeReportsDark_AndClrConstantsMatchLegacy()
        ThemeManager.SetScheme(BuiltInSchemes.Dark())
        Dim p = BuiltInSchemes.Dark().Palette

        Assert.True(KBotTheme.IsDark)
        ' CLR_BG (legacy #1C1C1C) == SurfaceAlt; CLR_BG_PANEL (#2D2D30) == Surface.
        Assert.Equal(p.SurfaceAltColor.ToArgb(), KBotTheme.CLR_BG.ToArgb())
        Assert.Equal(p.SurfaceColor.ToArgb(), KBotTheme.CLR_BG_PANEL.ToArgb())
        Assert.Equal(p.TextColor.ToArgb(), KBotTheme.CLR_FG.ToArgb())
        Assert.Equal(p.ButtonBackColor.ToArgb(), KBotTheme.CLR_BTN.ToArgb())
        Assert.Equal(p.TabAccentColor.ToArgb(), KBotTheme.CLR_TAB_ACCENT.ToArgb())
    End Sub

    <Fact>
    Public Sub SetTheme_False_MapsToClassic_NotDark()
        KBotTheme.SetTheme(False)
        Assert.False(KBotTheme.IsDark)
        Assert.Equal(BuiltInSchemes.ClassicName, ThemeManager.Current.Name)
    End Sub

    <Fact>
    Public Sub SetTheme_True_MapsToDark()
        KBotTheme.SetTheme(True)
        Assert.True(KBotTheme.IsDark)
        Assert.Equal(BuiltInSchemes.DarkName, ThemeManager.Current.Name)
    End Sub

End Class
