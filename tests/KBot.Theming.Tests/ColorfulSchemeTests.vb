Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Comportamentul schemei «Colorful» prin motor, nu doar prin definiția ei: aplicăm o schemă
''' care SCRIE culori, apoi comutăm pe Colorful și verificăm că suprafața se întoarce la ce s-a
''' autorit. E cerința operatorului, verificată exact așa cum a fost formulată — «will preserve
''' exactly the colors i set in the designer».
''' </summary>
Public Class ColorfulSchemeTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_colorful_test_" & Guid.NewGuid().ToString("N"))
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
    Public Sub Colorful_RestoresTheDesignerColor_AfterAThemeOverwroteIt()
        Using f As New Form()
            Dim p As New Panel() With {.Name = "pnl", .BackColor = Color.Fuchsia}
            f.Controls.Add(p)

            ' Prima aplicare ia instantaneul ȘI scrie paleta peste el.
            ThemeManager.SetScheme(BuiltInSchemes.Dark())
            ThemeManager.Apply(f)
            Assert.NotEqual(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())

            ThemeManager.SetScheme(BuiltInSchemes.Colorful())
            ThemeManager.Apply(f)
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())
        End Using
    End Sub

    ''' <summary>
    ''' Panourile unui SplitContainer nu trec niciodată prin recursie (traversarea sare direct la
    ''' copiii lor), dar StylePalette LE SCRIE. Fără tratamentul explicit din PreserveDesigner ar
    ''' rămâne singurele suprafețe cu culoarea temei vechi sub Colorful.
    ''' </summary>
    <Fact>
    Public Sub Colorful_RestoresSplitContainerPanels_Too()
        Using f As New Form()
            Dim sc As New SplitContainer() With {.Name = "split"}
            sc.Panel1.BackColor = Color.Fuchsia
            sc.Panel2.BackColor = Color.LimeGreen
            f.Controls.Add(sc)

            ThemeManager.SetScheme(BuiltInSchemes.Dark())
            ThemeManager.Apply(f)
            Assert.NotEqual(Color.Fuchsia.ToArgb(), sc.Panel1.BackColor.ToArgb())

            ThemeManager.SetScheme(BuiltInSchemes.Colorful())
            ThemeManager.Apply(f)
            Assert.Equal(Color.Fuchsia.ToArgb(), sc.Panel1.BackColor.ToArgb())
            Assert.Equal(Color.LimeGreen.ToArgb(), sc.Panel2.BackColor.ToArgb())
        End Using
    End Sub

    ''' <summary>
    ''' Un control care NU a fost colorat în designer nu trebuie să rămână cu ce a scris tema
    ''' anterioară: sub Colorful redevine moștenitor (Reset), nu îngheață pe culoarea Dark.
    ''' </summary>
    <Fact>
    Public Sub Colorful_UnauthoredControl_FallsBackToInherited_NotToThePreviousTheme()
        Using f As New Form()
            Dim lbl As New Label() With {.Name = "lbl"}
            f.Controls.Add(lbl)
            Dim inherited As Integer = lbl.ForeColor.ToArgb()

            ThemeManager.SetScheme(BuiltInSchemes.Dark())
            ThemeManager.Apply(f)
            Dim darkFore As Integer = lbl.ForeColor.ToArgb()

            ThemeManager.SetScheme(BuiltInSchemes.Colorful())
            ThemeManager.Apply(f)

            Assert.NotEqual(darkFore, lbl.ForeColor.ToArgb())
            Assert.Equal(inherited, lbl.ForeColor.ToArgb())
        End Using
    End Sub

    ''' <summary>Comutarea înapoi pe o schemă care scrie culori trebuie să funcționeze la fel ca înainte.</summary>
    <Fact>
    Public Sub LeavingColorful_LetsTheThemeWriteAgain()
        Using f As New Form()
            Dim p As New Panel() With {.Name = "pnl", .BackColor = Color.Fuchsia}
            f.Controls.Add(p)

            ThemeManager.SetScheme(BuiltInSchemes.Colorful())
            ThemeManager.Apply(f)
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())

            ThemeManager.SetScheme(BuiltInSchemes.Dark())
            ThemeManager.Apply(f)
            Assert.Equal(BuiltInSchemes.Dark().Palette.SurfaceColor.ToArgb(), p.BackColor.ToArgb())
        End Using
    End Sub

End Class
