Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Modelul de suprascrieri pe control: calea stabilă, contractul „neatins ⇒ nu se scrie”,
''' round-trip-ul JSON și aplicarea peste o ierarhie vie.
''' </summary>
Public Class ControlPathTests

    <Fact>
    Public Sub Build_UsesNames_AndExcludesTheRoot()
        Using f As New Form() With {.Name = "MainForm"}
            Dim outer As New Panel() With {.Name = "pnlRoot"}
            Dim inner As New Panel() With {.Name = "pnlHeader"}
            Dim leaf As New Button() With {.Name = "btnStil"}
            inner.Controls.Add(leaf)
            outer.Controls.Add(inner)
            f.Controls.Add(outer)

            Assert.Equal("pnlRoot/pnlHeader/btnStil", ControlPath.Build(f, leaf))
            Assert.Equal("pnlRoot", ControlPath.Build(f, outer))
            Assert.Equal(String.Empty, ControlPath.Build(f, f))
        End Using
    End Sub

    <Fact>
    Public Sub Build_ControlOutsideTheRoot_ReturnsNothing()
        Using f As New Form()
            Using other As New Form()
                Dim stray As New Button() With {.Name = "btn"}
                other.Controls.Add(stray)
                Assert.Null(ControlPath.Build(f, stray))
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub Resolve_IsTheInverseOfBuild()
        Using f As New Form()
            Dim outer As New Panel() With {.Name = "pnlRoot"}
            Dim leaf As New Button() With {.Name = "btnStil"}
            outer.Controls.Add(leaf)
            f.Controls.Add(outer)

            Dim path As String = ControlPath.Build(f, leaf)
            Assert.Same(leaf, ControlPath.Resolve(f, path))
            Assert.Same(f, ControlPath.Resolve(f, String.Empty))
            Assert.Null(ControlPath.Resolve(f, "pnlRoot/nuExista"))
        End Using
    End Sub

    ''' <summary>Fără nume, segmentul cade pe „{Tip}[{index}]” — imposibil de confundat cu un nume.</summary>
    <Fact>
    Public Sub SegmentOf_UnnamedControl_UsesTypeAndIndex()
        Using f As New Form()
            Dim first As New Button()
            Dim second As New Button()
            f.Controls.Add(first)
            f.Controls.Add(second)

            Assert.Equal("Button[0]", ControlPath.SegmentOf(first))
            Assert.Equal("Button[1]", ControlPath.SegmentOf(second))
            Assert.Same(second, ControlPath.Resolve(f, "Button[1]"))
        End Using
    End Sub

End Class

Public Class ControlStyleOverrideTests

    <Fact>
    Public Sub FreshEntry_IsEmpty_AndPruneDropsIt()
        Dim entry As New ControlStyleOverride With {.Path = "pnl"}
        Assert.True(entry.IsEmpty)

        Dim styleSet As New ThemeOverrideSet()
        styleSet.Entries.Add(entry)
        styleSet.Prune()
        Assert.Empty(styleSet.Entries)
    End Sub

    <Fact>
    Public Sub ColorRoundTrip_EmptyMeansUntouched()
        Assert.Null(ControlStyleOverride.FromColor(Color.Empty))
        Assert.Equal("#FF00FF", ControlStyleOverride.FromColor(Color.Fuchsia))
        Assert.Equal(Color.Empty, ControlStyleOverride.ToColor(Nothing))
        Assert.Equal(Color.Empty, ControlStyleOverride.ToColor("nu-i hex"))
        Assert.Equal(Color.Fuchsia.ToArgb(), ControlStyleOverride.ToColor("#FF00FF").ToArgb())
    End Sub

    <Fact>
    Public Sub FontRoundTrip_NeedsBothNameAndSize()
        Dim entry As New ControlStyleOverride()
        Assert.False(entry.HasFont)
        Assert.Null(entry.ToFont())

        entry.SetFont(New Font("Consolas", 11.0F, FontStyle.Bold))
        Assert.True(entry.HasFont)
        Using f As Font = entry.ToFont()
            Assert.Equal("Consolas", f.FontFamily.Name)
            Assert.Equal(11.0F, f.Size)
            Assert.True(f.Bold)
        End Using

        entry.SetFont(Nothing)
        Assert.False(entry.HasFont)
        Assert.True(entry.IsEmpty)
    End Sub

End Class

Public Class ThemeOverrideStoreTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_ovr_test_" & Guid.NewGuid().ToString("N"))
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
    Public Sub Save_ThenLoad_RoundTripsTheChoices()
        Dim styleSet As New ThemeOverrideSet With {.Name = "Test", .Scope = "MainForm"}
        Dim entry = styleSet.GetOrCreate("pnlHeader/cboTema", "KBot.Controls.KBotComboBox")
        entry.BackColor = "#112233"
        entry.HoverColor = "#445566"
        entry.SetFont(New Font("Consolas", 10.0F))

        Dim filePath As String = ThemeOverrideStore.DefaultPathFor("MainForm")
        ThemeOverrideStore.Save(styleSet, filePath)
        Assert.True(File.Exists(filePath))

        Dim loaded = ThemeOverrideStore.LoadFile(filePath)
        Assert.Equal("MainForm", loaded.Scope)
        Assert.Single(loaded.Entries)
        Assert.Equal("#112233", loaded.Entries(0).BackColor)
        Assert.Equal("#445566", loaded.Entries(0).HoverColor)
        Assert.Equal("Consolas", loaded.Entries(0).FontName)
        Assert.Null(loaded.Entries(0).ForeColor)   ' neatins ⇒ absent din fișier
    End Sub

    ''' <summary>Diacriticele trebuie să rămână LITERALE în fișier (regula casei).</summary>
    <Fact>
    Public Sub Save_KeepsRomanianDiacriticsLiteral()
        Dim styleSet As New ThemeOverrideSet With {.Name = "Recepții și plăți", .Scope = "ReceptiiView"}
        styleSet.GetOrCreate("pnl", "Panel").BackColor = "#000000"

        Dim filePath As String = Path.Combine(_tempRoot, "diacritice.json")
        ThemeOverrideStore.Save(styleSet, filePath)

        Dim raw As String = File.ReadAllText(filePath)
        Assert.Contains("Recepții și plăți", raw)
        Assert.DoesNotContain("\u", raw)
    End Sub

    <Fact>
    Public Sub LoadFile_Missing_ReturnsNothing()
        Assert.Null(ThemeOverrideStore.LoadFile(Path.Combine(_tempRoot, "nu_exista.json")))
    End Sub

    ''' <summary>Un fișier corupt e sărit + logat, nu face restul invizibil.</summary>
    <Fact>
    Public Sub LoadAll_SkipsCorruptFile_AndKeepsTheRest()
        Directory.CreateDirectory(ThemeOverrideStore.OverridesFolder)
        File.WriteAllText(Path.Combine(ThemeOverrideStore.OverridesFolder, "stricat.json"), "{ nu e json")

        Dim ok As New ThemeOverrideSet With {.Scope = "SumarView"}
        ok.GetOrCreate("pnl", "Panel").ForeColor = "#FFFFFF"
        ThemeOverrideStore.Save(ok, ThemeOverrideStore.DefaultPathFor("SumarView"))

        Dim all = ThemeOverrideStore.LoadAll()
        Assert.Single(all)
        Assert.Equal("SumarView", all(0).Scope)
    End Sub

    <Fact>
    Public Sub SanitizeFileName_ReplacesForbiddenCharacters()
        Assert.Equal("a_b", ThemeOverrideStore.SanitizeFileName("a/b"))
        Assert.Equal(String.Empty, ThemeOverrideStore.SanitizeFileName("   "))
    End Sub

End Class

Public Class ThemeOverrideApplierTests

    <Fact>
    Public Sub Apply_WritesOnlyTheTouchedSlots()
        Using f As New Form()
            Dim p As New Panel() With {.Name = "pnl", .BackColor = Color.White, .ForeColor = Color.Black}
            f.Controls.Add(p)

            Dim styleSet As New ThemeOverrideSet With {.Scope = "Form"}
            styleSet.GetOrCreate("pnl", "Panel").BackColor = "#FF00FF"

            Assert.Equal(1, ThemeOverrideApplier.Apply(f, styleSet))
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())
            Assert.Equal(Color.Black.ToArgb(), p.ForeColor.ToArgb())   ' neatins ⇒ nescris
        End Using
    End Sub

    <Fact>
    Public Sub Apply_SkipsEntriesWhosePathNoLongerResolves()
        Using f As New Form()
            Dim styleSet As New ThemeOverrideSet()
            styleSet.GetOrCreate("nu/mai/exista", "Panel").BackColor = "#FF00FF"
            Assert.Equal(0, ThemeOverrideApplier.Apply(f, styleSet))
        End Using
    End Sub

    ''' <summary>
    ''' Sloturile suplimentare se aplică doar controalelor care expun proprietatea. Pe un Panel
    ''' obișnuit nu există nimic de scris — și asta NU e o eroare, doar un slot fără efect.
    ''' </summary>
    <Fact>
    Public Sub ExtraSlots_AreAppliedOnlyWhereThePropertyExists()
        Using p As New Panel()
            Assert.Null(ThemeOverrideApplier.FindColorProperty(p, ThemeOverrideApplier.HoverColorNames))
            Assert.Null(ThemeOverrideApplier.TrySetColor(p, ThemeOverrideApplier.HoverColorNames, Color.Fuchsia))
            Assert.Equal(Color.Empty, ThemeOverrideApplier.ReadColor(p, ThemeOverrideApplier.HoverColorNames))
        End Using
    End Sub

    <Fact>
    Public Sub TrySetColor_EmptyColorMeansUntouched_AndWritesNothing()
        Using p As New Panel()
            Assert.Null(ThemeOverrideApplier.TrySetColor(p, {"BackColor"}, Color.Empty))
        End Using
    End Sub

End Class
