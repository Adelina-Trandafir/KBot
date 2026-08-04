Option Strict On
Imports System.IO
Imports System.Linq
Imports KBot.DevHarness
Imports Xunit

' The scenario files shipped in Config\ must actually load — comments, diacritics, trailing commas
' and all. A file that fails to parse is discovered at the worst possible moment otherwise.
Public Class ShippedScenarioFilesTests

    Private Shared Function ConfigFiles() As IEnumerable(Of String)
        Dim dir As String = Path.Combine(AppContext.BaseDirectory, "Config")
        If Not Directory.Exists(dir) Then Return Enumerable.Empty(Of String)()
        Return Directory.GetFiles(dir, "*.json")
    End Function

    <Fact>
    Public Sub EveryShippedScenario_ParsesWithoutErrors()
        Dim files = ConfigFiles().ToList()
        Assert.NotEmpty(files)
        For Each f As String In files
            Dim r = HarnessScenarioReader.Read(File.ReadAllText(f))
            Assert.True(r.IsValid,
                        $"{Path.GetFileName(f)}: " & String.Join(" / ", r.Errors))
        Next
    End Sub

    <Fact>
    Public Sub EveryShippedScenario_DemandsACleanBaseline()
        ' After 04.08 every shipped probe refuses to run on a contaminated machine.
        For Each f As String In ConfigFiles()
            Dim r = HarnessScenarioReader.Read(File.ReadAllText(f))
            Assert.True(r.Scenario.RequireCleanBaseline,
                        $"{Path.GetFileName(f)} ar trebui să ceară o bază curată")
        Next
    End Sub

    <Fact>
    Public Sub NoShippedScenario_CarriesADocumentPath()
        ' The PDF always comes from «Deschide PDF…».
        For Each f As String In ConfigFiles()
            Dim r = HarnessScenarioReader.Read(File.ReadAllText(f))
            Assert.DoesNotContain(r.Warnings, Function(w) w.Contains("document"))
        Next
    End Sub

End Class
