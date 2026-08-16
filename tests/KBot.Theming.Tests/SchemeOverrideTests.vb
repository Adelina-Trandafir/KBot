Option Strict On
Imports System.IO
Imports KBot.Theming
Imports Xunit

''' <summary>
''' PERSONALIZAREA SCHEMELOR (felia 0036): un fișier din …\AVACONT\Themes cu numele unei scheme
''' built-in o ÎNLOCUIEȘTE, nu se adaugă lângă ea.
'''
''' Asta e regula pe care se sprijină tot «Salvează» din fereastra de opțiuni. Fără ea, editarea
''' lui «Modern» ar fi produs două rânduri «Modern» în meniul de teme, iar
''' <c>ResolveByName</c> — care întoarce primul potrivit — ar fi ales mereu pe cel NEeditat: adică
''' salvarea ar fi părut că nu face nimic.
''' </summary>
Public Class SchemeOverrideTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_override_test_" & Guid.NewGuid().ToString("N"))
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

    Private Shared Function SchemaDeUtilizator(nume As String, suprafata As String) As ThemeScheme
        Dim s As ThemeScheme = BuiltInSchemes.Modern()
        s.Name = nume
        s.Palette.Surface = suprafata
        Return s
    End Function

    ' ── Îmbinarea ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Un_fisier_omonim_inlocuieste_schema_built_in()
        Dim personalizata As ThemeScheme = SchemaDeUtilizator(BuiltInSchemes.ModernName, "#123456")
        Dim rezultat = ThemeManager.MergeSchemes(BuiltInSchemes.All(),
                                                 New ThemeScheme() {personalizata})

        Assert.Equal(BuiltInSchemes.All().Count, rezultat.Count)   ' nu s-a adăugat un al doilea «Modern»
        Assert.Single(rezultat, Function(s) String.Equals(s.Name, BuiltInSchemes.ModernName,
                                                          StringComparison.OrdinalIgnoreCase))

        Dim modern As ThemeScheme = rezultat.First(
            Function(s) String.Equals(s.Name, BuiltInSchemes.ModernName, StringComparison.OrdinalIgnoreCase))
        Assert.Equal("#123456", modern.Palette.Surface)
    End Sub

    ''' <summary>Locul din listă rămâne al schemei built-in: meniul nu-și rearanjează rândurile.</summary>
    <Fact>
    Public Sub Inlocuirea_pastreaza_pozitia_din_lista()
        Dim pozitiaModern As Integer = -1
        Dim builtIn = BuiltInSchemes.All()
        For i As Integer = 0 To builtIn.Count - 1
            If builtIn(i).Name = BuiltInSchemes.ModernName Then pozitiaModern = i
        Next

        Dim rezultat = ThemeManager.MergeSchemes(builtIn,
            New ThemeScheme() {SchemaDeUtilizator(BuiltInSchemes.ModernName, "#123456")})

        Assert.Equal(BuiltInSchemes.ModernName, rezultat(pozitiaModern).Name)
        Assert.Equal("#123456", rezultat(pozitiaModern).Palette.Surface)
    End Sub

    ''' <summary>O schemă cu nume propriu se ADAUGĂ — nu înlocuiește nimic.</summary>
    <Fact>
    Public Sub O_schema_cu_nume_propriu_se_adauga_la_coada()
        Dim rezultat = ThemeManager.MergeSchemes(BuiltInSchemes.All(),
            New ThemeScheme() {SchemaDeUtilizator("Al meu", "#123456")})

        Assert.Equal(BuiltInSchemes.All().Count + 1, rezultat.Count)
        Assert.Equal("Al meu", rezultat.Last().Name)
    End Sub

    ''' <summary>Potrivirea numelui ignoră registrul, ca peste tot în motorul de teme.</summary>
    <Fact>
    Public Sub Potrivirea_numelui_ignora_registrul()
        Dim rezultat = ThemeManager.MergeSchemes(BuiltInSchemes.All(),
            New ThemeScheme() {SchemaDeUtilizator("mOdErN", "#123456")})

        Assert.Equal(BuiltInSchemes.All().Count, rezultat.Count)
    End Sub

    ' ── Fișierul ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub SaveScheme_apoi_LoadUserSchemes_face_dus_intors()
        ThemeStore.SaveScheme(SchemaDeUtilizator(BuiltInSchemes.ModernName, "#ABCDEF"))

        Dim incarcate = ThemeStore.LoadUserSchemes()
        Assert.Single(incarcate)
        Assert.Equal(BuiltInSchemes.ModernName, incarcate(0).Name)
        Assert.Equal("#ABCDEF", incarcate(0).Palette.Surface)
        ' Opțiunile de stil merg și ele pe fir, nu doar culorile.
        Assert.Equal(BuiltInSchemes.Modern().Style.CornerRadius, incarcate(0).Style.CornerRadius)
    End Sub

    <Fact>
    Public Sub DeleteScheme_readuce_implicitul()
        ThemeStore.SaveScheme(SchemaDeUtilizator(BuiltInSchemes.ModernName, "#ABCDEF"))
        Assert.True(ThemeStore.DeleteScheme(BuiltInSchemes.ModernName))
        Assert.Empty(ThemeStore.LoadUserSchemes())
    End Sub

    ''' <summary>
    ''' «Restaurează implicit» pe o schemă neatinsă nu e o eroare, e un răspuns: nu era nimic de
    ''' șters. Un throw aici ar transforma un buton inofensiv într-un dialog de eroare.
    ''' </summary>
    <Fact>
    Public Sub DeleteScheme_fara_fisier_raspunde_False()
        Assert.False(ThemeStore.DeleteScheme(BuiltInSchemes.ModernName))
    End Sub

    <Fact>
    Public Sub SaveScheme_fara_nume_arunca()
        Dim faraNume As ThemeScheme = BuiltInSchemes.Modern()
        faraNume.Name = "   "
        Assert.Throws(Of ArgumentException)(Sub() ThemeStore.SaveScheme(faraNume))
    End Sub

    ''' <summary>Un nume cu caractere interzise nu poate produce o cale invalidă.</summary>
    <Fact>
    Public Sub Numele_cu_caractere_interzise_se_curata()
        Dim cale As String = ThemeStore.SchemeFilePath("a/b:c*d")
        Assert.Equal(ThemeStore.ThemesFolder, Path.GetDirectoryName(cale))
        Assert.Equal("a_b_c_d.json", Path.GetFileName(cale))
    End Sub

End Class
