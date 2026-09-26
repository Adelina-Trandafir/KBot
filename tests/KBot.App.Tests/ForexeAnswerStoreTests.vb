Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports Xunit

' Slice 0081-07. An answer kept in «Rezultate_Forexe» must come back as the SAME result the
' runner returned: same verdict, same variables, same tables. If it does not, loading it "as if
' the workflow had run" would be a lie, and the whole point -- fixing how K-BOT reads FOREXE's
' answer without saving in FOREXE a second time -- is gone.
' Written, not run (house rule).
Public Class ForexeAnswerStoreTests

    Private Shared Function Job(Optional parameters As Dictionary(Of String, String) = Nothing) As JobRequest
        Return New JobRequest With {
            .WorkflowName = "CreareAngajament",
            .WflPath = "C:\KBOT\Workflows\Creare\adlop - Creare Angajament.wfl",
            .Parameters = If(parameters, New Dictionary(Of String, String) From {
                {"DESCRIERE_ANGAJAMENT", "Burse"},
                {"DATE_RECEPTIE", "[{""Cheie"":""IdSecA-1""}]"}
            })
        }
    End Function

    Private Shared Function Result() As JobResult
        Dim r As New JobResult With {.Success = False, .Message = "Mod probă: oprit.", .StoppedBeforeSave = True}
        r.Data("CodAng_Final") = "AAB2EF2MCP4"
        r.Data("Poza_IdSecA-1") = Convert.ToBase64String(New Byte() {1, 2, 3})
        r.Data("TabelIndicatori") = "[{""Indicator ang"":""AAB1"",""Suma"":""10,00""}]"
        Return r
    End Function

    Private Shared Function Session() As SessionContext
        Return New SessionContext With {.DbName = "000_DEMO", .An = 2026, .SectorSursa = "02A"}
    End Function

    <Fact>
    Public Sub FromRun_KeepsTheVerdictTheVariablesAndTheRequest()
        Dim job As JobRequest = Job()
        job.StopBeforeSave = True
        Dim a As ForexeAnswer = ForexeAnswerStore.FromRun(job, "!DDF7", Session(), Result(), New Date(2026, 9, 26, 14, 30, 12, 457))

        Assert.Equal(ForexeAnswerStore.CurrentVersion, a.Version)
        Assert.Equal("CreareAngajament", a.Workflow)
        Assert.Equal("adlop - Creare Angajament.wfl", a.WflFile)
        Assert.Equal("!DDF7", a.Code)
        Assert.Equal("000_DEMO", a.DbName)
        Assert.Equal(2026, a.FiscalYear)
        Assert.True(a.DryRun)
        Assert.False(a.Success)
        Assert.True(a.StoppedBeforeSave)
        Assert.Equal("Mod probă: oprit.", a.Message)
        Assert.Equal("Burse", a.Parameters("DESCRIERE_ANGAJAMENT"))
        Assert.Equal("AAB2EF2MCP4", a.Variables("CodAng_Final"))
        Assert.Equal(3, a.Variables.Count)
    End Sub

    <Fact>
    Public Sub ToJobResult_AfterAJsonRoundTrip_IsTheResultTheRunnerReturned()
        Dim original As JobResult = Result()
        Dim a As ForexeAnswer = ForexeAnswerStore.FromRun(Job(), "!DDF7", Session(), original, Date.Now)
        Dim back As ForexeAnswer = JsonSerializer.Deserialize(Of ForexeAnswer)(JsonSerializer.Serialize(a))

        Dim r As JobResult = ForexeAnswerStore.ToJobResult(back)

        Assert.Equal(original.Success, r.Success)
        Assert.Equal(original.Message, r.Message)
        Assert.Equal(original.StoppedBeforeSave, r.StoppedBeforeSave)
        Assert.Equal(original.Data, r.Data)
        ' The table is rebuilt from its variable, exactly as the runner builds it.
        Assert.True(r.Tables.ContainsKey("TabelIndicatori"))
        Assert.Equal("AAB1", r.Tables("TabelIndicatori")(0)("Indicator ang").TextSau(String.Empty))
        ' A plain value and a capture stay variables, not tables.
        Assert.False(r.Tables.ContainsKey("CodAng_Final"))
        Assert.False(r.Tables.ContainsKey("Poza_IdSecA-1"))
        ' The capture still decodes to the same bytes (the send stores it on the revision).
        Dim captures As List(Of KeyValuePair(Of String, Byte())) = DdfSendInputs.Captures(r.Data)
        Assert.Single(captures)
        Assert.Equal(New Byte() {1, 2, 3}, captures(0).Value)
    End Sub

    <Fact>
    Public Sub ParameterDifferences_SameRequest_IsEmpty()
        Dim a As ForexeAnswer = ForexeAnswerStore.FromRun(Job(), "!DDF7", Session(), Result(), Date.Now)
        Assert.Empty(ForexeAnswerStore.ParameterDifferences(a, Job()))
    End Sub

    <Fact>
    Public Sub ParameterDifferences_NamesChangedMissingAndExtraKeys()
        Dim a As ForexeAnswer = ForexeAnswerStore.FromRun(Job(), "!DDF7", Session(), Result(), Date.Now)
        Dim now As JobRequest = Job(New Dictionary(Of String, String) From {
            {"DESCRIERE_ANGAJAMENT", "Burse 2"},
            {"MOTIV", "(REV:1)"}
        })

        Dim d As List(Of String) = ForexeAnswerStore.ParameterDifferences(a, now)

        Assert.Equal(New List(Of String) From {"DATE_RECEPTIE", "DESCRIERE_ANGAJAMENT", "MOTIV"}, d)
    End Sub

    <Fact>
    Public Sub FileName_IsStampWorkflowCode_WithoutUnsafeCharacters()
        Dim a As New ForexeAnswer With {
            .RecordedAt = New Date(2026, 9, 26, 14, 30, 12, 457),
            .Workflow = "IncarcaRezervare",
            .Code = "AAB2/EF2 MCP4"
        }
        Assert.Equal("20260926_143012_457_IncarcaRezervare_AAB2EF2MCP4.json", ForexeAnswerStore.FileName(a))
    End Sub

    <Fact>
    Public Sub FileName_WithoutCode_EndsWithTheWorkflow()
        Dim a As New ForexeAnswer With {.RecordedAt = New Date(2026, 9, 26, 8, 0, 0), .Workflow = "ListaAngajamente"}
        Assert.Equal("20260926_080000_000_ListaAngajamente.json", ForexeAnswerStore.FileName(a))
    End Sub

    <Fact>
    Public Sub FilePattern_ListsOnlyThatWorkflow()
        Assert.Equal("*_PrelucrareCompleta_*.json;*_PrelucrareCompleta.json",
                     ForexeAnswerStore.FilePattern("PrelucrareCompleta"))
        Assert.Equal("*.json", ForexeAnswerStore.FilePattern(String.Empty))
    End Sub

    <Fact>
    Public Sub Load_RefusesAnotherVersion()
        Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"kbot_answer_{Guid.NewGuid():N}.json")
        Try
            File.WriteAllText(path, "{""Version"": 99, ""Workflow"": ""CreareAngajament""}")
            Assert.Throws(Of InvalidDataException)(Function() ForexeAnswerStore.Load(path))
        Finally
            File.Delete(path)
        End Try
    End Sub

    <Fact>
    Public Sub Load_ReadsWhatSaveWrote()
        Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"kbot_answer_{Guid.NewGuid():N}.json")
        Try
            Dim a As ForexeAnswer = ForexeAnswerStore.FromRun(Job(), "!DDF7", Session(), Result(), Date.Now)
            File.WriteAllText(path, JsonSerializer.Serialize(a))
            Dim back As ForexeAnswer = ForexeAnswerStore.Load(path)
            Assert.Equal("CreareAngajament", back.Workflow)
            Assert.Equal(a.Variables, back.Variables)
            Assert.Equal(a.Parameters, back.Parameters)
        Finally
            File.Delete(path)
        End Try
    End Sub

End Class
