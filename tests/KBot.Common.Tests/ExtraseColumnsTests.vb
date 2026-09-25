Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Xunit
Imports KBot.Common

''' <summary>
''' The statement grids' column choice (slice 0080-02): the catalogue, the defaults the operator
''' asked for, the cleaning of a stored list, and the round trip through app_settings.json.
''' Every store test works on a temporary folder passed explicitly.
''' </summary>
Public Class ExtraseColumnsTests

    Private Shared Function TempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "kbot_extrase_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(d)
        Return d
    End Function

    <Fact>
    Public Sub Defaults_AreTheColumnsTheOperatorAskedFor()
        Assert.Equal({ExtraseColumns.HData, ExtraseColumns.HClsf, ExtraseColumns.HSid, ExtraseColumns.HSic,
                      ExtraseColumns.HTsd, ExtraseColumns.HTsc, ExtraseColumns.HSfd, ExtraseColumns.HSfc},
                     ExtraseColumns.Defaults(ExtraseGrid.ViewHeaders))
        ' DataBanca first: it can differ from DataDoc.
        Assert.Equal(ExtraseColumns.ODataBanca, ExtraseColumns.Defaults(ExtraseGrid.ViewOperations)(0))
        ' The window adds CodAngajament / Indicator and ends on Explicatii.
        Dim w As List(Of String) = ExtraseColumns.Defaults(ExtraseGrid.WindowOperations)
        Assert.Contains(ExtraseColumns.OCodAngajament, w)
        Assert.Contains(ExtraseColumns.OIndicator, w)
        Assert.Equal(ExtraseColumns.OExplicatii, w.Last())
    End Sub

    <Fact>
    Public Sub EveryDefault_IsInItsCatalogue()
        For Each g As ExtraseGrid In [Enum].GetValues(GetType(ExtraseGrid))
            Dim keys As HashSet(Of String) = ExtraseColumns.Catalog(g).Select(Function(c) c.Key).ToHashSet()
            Assert.All(ExtraseColumns.Defaults(g), Sub(k) Assert.Contains(k, keys))
        Next
    End Sub

    <Fact>
    Public Sub CatalogueKeys_AreAsciiOnly()
        For Each g As ExtraseGrid In [Enum].GetValues(GetType(ExtraseGrid))
            Assert.All(ExtraseColumns.Catalog(g), Sub(c) Assert.True(c.Key.All(Function(ch) AscW(ch) < 128), c.Key))
        Next
    End Sub

    <Fact>
    Public Sub Normalize_DropsUnknownAndDuplicateKeys_KeepsOrder()
        Dim got As List(Of String) = ExtraseColumns.Normalize(ExtraseGrid.ViewOperations,
            {ExtraseColumns.OCredit, "nu_exista", ExtraseColumns.ODataBanca, ExtraseColumns.OCredit})
        Assert.Equal({ExtraseColumns.OCredit, ExtraseColumns.ODataBanca}, got)
    End Sub

    <Fact>
    Public Sub Normalize_NothingOrEmpty_IsTheDefaults()
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.WindowHeaders),
                     ExtraseColumns.Normalize(ExtraseGrid.WindowHeaders, Nothing))
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.WindowHeaders),
                     ExtraseColumns.Normalize(ExtraseGrid.WindowHeaders, {"nu_exista"}))
    End Sub

    <Fact>
    Public Sub HeaderKeys_AreNotAcceptedByTheOperationsGrid()
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.ViewOperations),
                     ExtraseColumns.Normalize(ExtraseGrid.ViewOperations, {ExtraseColumns.HSid}))
    End Sub

    <Fact>
    Public Sub MissingFromTheFile_ReadsAsTheDefaults()
        Dim s As AppSettings = AppSettings.Load(TempDir())
        For Each g As ExtraseGrid In [Enum].GetValues(GetType(ExtraseGrid))
            Assert.Equal(ExtraseColumns.Defaults(g), s.ExtraseColumnsFor(g))
        Next
    End Sub

    <Fact>
    Public Sub Save_ThenLoad_KeepsEachGridSeparately()
        Dim dir As String = TempDir()
        Dim s As New AppSettings()
        s.SetExtraseColumns(ExtraseGrid.ViewOperations, {ExtraseColumns.OCredit, ExtraseColumns.ODebit})
        s.SetExtraseColumns(ExtraseGrid.WindowHeaders, {ExtraseColumns.HClsf})
        s.Save(dir)

        Dim back As AppSettings = AppSettings.Load(dir)
        Assert.Equal({ExtraseColumns.OCredit, ExtraseColumns.ODebit}, back.ExtraseColumnsFor(ExtraseGrid.ViewOperations))
        Assert.Equal({ExtraseColumns.HClsf}, back.ExtraseColumnsFor(ExtraseGrid.WindowHeaders))
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.ViewHeaders), back.ExtraseColumnsFor(ExtraseGrid.ViewHeaders))
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.WindowOperations), back.ExtraseColumnsFor(ExtraseGrid.WindowOperations))
    End Sub

    <Fact>
    Public Sub SetNothing_GoesBackToTheDefaults()
        Dim s As New AppSettings()
        s.SetExtraseColumns(ExtraseGrid.ViewHeaders, {ExtraseColumns.HSid})
        s.SetExtraseColumns(ExtraseGrid.ViewHeaders, Nothing)
        Assert.Null(s.ExtraseViewHeaderColumns)
        Assert.Equal(ExtraseColumns.Defaults(ExtraseGrid.ViewHeaders), s.ExtraseColumnsFor(ExtraseGrid.ViewHeaders))
    End Sub

End Class
