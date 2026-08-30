Option Strict On
Imports System.IO
Imports System.Text.Json
Imports Xunit
Imports KBot.Theming

''' <summary>
''' The defect this file exists for, written down so it is never rediscovered the hard way.
'''
''' <para>Editing a built-in scheme in the options window writes
''' <c>…\AVACONT\Themes\&lt;Name&gt;.json</c>, and from the next start THAT file IS the scheme — it
''' replaces the compiled one. Which is correct, and is how «edit Modern and keep it» works.</para>
'''
''' <para><b>What it also did, until slice 0049.</b> A file saved by an older build knows nothing
''' about any key added since, so deserializing it handed every new key the TYPE's default. A real
''' `Modern.json` saved on 17 August therefore switched off every card, the shipped font and all
''' four row heights on a build that had just introduced them — the application looked exactly as
''' it had before, and nothing was logged, because from the code's point of view nothing had gone
''' wrong. A whole slice of visible work was invisible, and the cause was a file nobody thought to
''' look at.</para>
'''
''' <para>The fix is an OVERLAY: the stored file is written over the compiled scheme key by key,
''' so what the operator actually chose still wins and everything else keeps the compiled value.
''' The JSON below is the real file, verbatim.</para>
''' </summary>
Public Class StaleUserSchemeTests

    ''' <summary>The Modern.json found in %AppData% on 2026-08-30, saved 2026-08-17.</summary>
    Private Const StoredModern As String = "
{
  ""Name"": ""Modern"",
  ""IsDark"": false,
  ""Palette"": {
    ""Surface"": ""#FAFAFA"",
    ""SurfaceAlt"": ""#FFFFFF"",
    ""Text"": ""#1E1E1E"",
    ""Accent"": ""#185FA5"",
    ""Border"": ""#E2E2E2""
  },
  ""Style"": {
    ""UseSystemColors"": false,
    ""FlatControls"": true,
    ""ButtonRender"": 2,
    ""CornerRadius"": 8,
    ""BaseFontName"": ""Segoe UI Variable Text"",
    ""BaseFontSize"": 9,
    ""ControlPadding"": { ""Left"": 2, ""Top"": 2, ""Right"": 2, ""Bottom"": 2 },
    ""FocusAccent"": true,
    ""DarkTitleBar"": false,
    ""OwnerDrawTabs"": false,
    ""PreserveDesignerColors"": false
  }
}"

    ''' <summary>
    ''' The bare deserialize — what the code did before the fix. Kept as a test so the failure mode
    ''' stays visible: every card value is 0, which every consumer reads as «draw nothing».
    ''' </summary>
    <Fact>
    Public Sub PlainDeserialize_LosesEveryKeyTheFileNeverHad()
        Dim s As ThemeScheme = JsonSerializer.Deserialize(Of ThemeScheme)(
            StoredModern, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})

        Assert.Equal(0, s.Style.CardRadius)
        Assert.Equal(0, s.Style.CardShadow)
        Assert.Equal(0, s.Style.ListRowHeight)
        Assert.False(s.Style.TintIcons)
        Assert.Equal("Segoe UI Variable Text", s.Style.BaseFontName)
    End Sub

    ''' <summary>Overlaid on the compiled scheme, the same file gets every new key.</summary>
    <Fact>
    Public Sub Overlay_BringsTheNewKeysBack()
        Dim compiled As ThemeScheme = BuiltInSchemes.Modern()
        Dim merged As ThemeScheme = ThemeStore.OverlayOnto(compiled, StoredModern)

        Assert.NotNull(merged)
        Assert.Equal("Modern", merged.Name)
        Assert.Equal(compiled.Style.CardRadius, merged.Style.CardRadius)
        Assert.Equal(compiled.Style.CardShadow, merged.Style.CardShadow)
        Assert.Equal(compiled.Style.CardShadowOpacity, merged.Style.CardShadowOpacity)
        Assert.Equal(compiled.Style.CardGutter, merged.Style.CardGutter)
        Assert.Equal(compiled.Style.ListRowHeight, merged.Style.ListRowHeight)
        Assert.Equal(compiled.Style.GridRowHeight, merged.Style.GridRowHeight)
        Assert.Equal(compiled.Style.NavItemHeight, merged.Style.NavItemHeight)
        Assert.True(merged.Style.TintIcons)
        Assert.Equal(compiled.Palette.CanvasColor, merged.Palette.CanvasColor)
        Assert.Equal(compiled.Palette.CardColor, merged.Palette.CardColor)
    End Sub

    ''' <summary>
    ''' …and the operator's own edits still win. This is the half that makes the overlay safe:
    ''' were it the other way round, «Salvează» in the options window would stop meaning anything.
    ''' </summary>
    <Fact>
    Public Sub Overlay_KeepsWhatTheOperatorActuallyChose()
        Dim merged As ThemeScheme = ThemeStore.OverlayOnto(BuiltInSchemes.Modern(), StoredModern)

        ' Present in the file, and different from the compiled scheme — the file wins.
        Assert.Equal("#185FA5", merged.Palette.Accent)
        Assert.Equal("#FAFAFA", merged.Palette.Surface)
        Assert.Equal("Segoe UI Variable Text", merged.Style.BaseFontName)
        Assert.Equal(9.0F, merged.Style.BaseFontSize)
        Assert.Equal(2, merged.Style.ControlPadding.Left)
        Assert.Equal(2, merged.Style.ControlPadding.Bottom)
    End Sub

    ''' <summary>A value the file sets to False must not be read as «absent» and overwritten.</summary>
    <Fact>
    Public Sub Overlay_RespectsFalseAndZero()
        Const json As String = "{""Name"":""Modern"",""Style"":{""CardRadius"":0,""TintIcons"":false,""FocusAccent"":false}}"
        Dim merged As ThemeScheme = ThemeStore.OverlayOnto(BuiltInSchemes.Modern(), json)

        Assert.Equal(0, merged.Style.CardRadius)
        Assert.False(merged.Style.TintIcons)
        Assert.False(merged.Style.FocusAccent)
        ' …while a key the file is silent about still comes from the compiled scheme.
        Assert.Equal(BuiltInSchemes.Modern().Style.CardShadow, merged.Style.CardShadow)
    End Sub

    ''' <summary>End to end, through the folder the application actually reads.</summary>
    <Fact>
    Public Sub LoadUserSchemes_OverlaysAStoredBuiltIn()
        Dim root As String = Path.Combine(Path.GetTempPath(), "kbot-theme-" & Guid.NewGuid().ToString("N"))
        Dim previous As String = ThemeStore.OverrideRootForTests
        Try
            ThemeStore.OverrideRootForTests = root
            Directory.CreateDirectory(ThemeStore.ThemesFolder)
            File.WriteAllText(Path.Combine(ThemeStore.ThemesFolder, "Modern.json"), StoredModern)

            Dim loaded As List(Of ThemeScheme) = ThemeStore.LoadUserSchemes(
                Function(n) If(String.Equals(n, "Modern", StringComparison.OrdinalIgnoreCase),
                               BuiltInSchemes.Modern(), Nothing))

            Dim modern As ThemeScheme = loaded.Single()
            Assert.True(modern.Style.CardRadius > 0, "Cardurile s-au pierdut la incarcarea din folder.")
            Assert.True(modern.Style.CardShadow > 0)
            Assert.Equal("#185FA5", modern.Palette.Accent)     ' operator's choice survives
        Finally
            ThemeStore.OverrideRootForTests = previous
            Try
                If Directory.Exists(root) Then Directory.Delete(root, True)
            Catch
                ' A temp folder that will not delete is not a test failure.
            End Try
        End Try
    End Sub

    ''' <summary>A scheme the operator invented has no built-in to overlay on, and is read as it stands.</summary>
    <Fact>
    Public Sub Overlay_IsSkippedForASchemeThatIsNotBuiltIn()
        Dim root As String = Path.Combine(Path.GetTempPath(), "kbot-theme-" & Guid.NewGuid().ToString("N"))
        Dim previous As String = ThemeStore.OverrideRootForTests
        Try
            ThemeStore.OverrideRootForTests = root
            Directory.CreateDirectory(ThemeStore.ThemesFolder)
            File.WriteAllText(Path.Combine(ThemeStore.ThemesFolder, "AlMeu.json"),
                              StoredModern.Replace("""Modern""", """AlMeu"""))

            Dim loaded As List(Of ThemeScheme) = ThemeStore.LoadUserSchemes(Function(n) Nothing)

            Dim mine As ThemeScheme = loaded.Single()
            Assert.Equal("AlMeu", mine.Name)
            Assert.Equal(0, mine.Style.CardRadius)
        Finally
            ThemeStore.OverrideRootForTests = previous
            Try
                If Directory.Exists(root) Then Directory.Delete(root, True)
            Catch
            End Try
        End Try
    End Sub

End Class
