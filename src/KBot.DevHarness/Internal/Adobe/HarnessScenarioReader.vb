Option Strict On
Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Text.Json.Serialization

' Result of parsing a scenario file (slice 0023). Errors and warnings are Romanian operator-facing
' strings; the reader itself NEVER shows a message box and never lets a parse exception escape.
Public NotInheritable Class HarnessScenarioReadResult

    Public ReadOnly Property Scenario As HarnessScenario
    Public ReadOnly Property Errors As New List(Of String)()
    Public ReadOnly Property Warnings As New List(Of String)()

    Public Sub New(scenario As HarnessScenario)
        Me.Scenario = scenario
    End Sub

    Public ReadOnly Property IsValid As Boolean
        Get
            Return Errors.Count = 0 AndAlso Scenario IsNot Nothing
        End Get
    End Property

End Class

' Serialization + validation for scenario files. Pure: no file I/O (the caller supplies the text),
' no registry, no windows. Comments and trailing commas are allowed because these files are
' written by hand.
Public NotInheritable Class HarnessScenarioReader

    Public Const SupportedSchema As Integer = 1

    Private Sub New()
    End Sub

    Friend Shared ReadOnly Options As JsonSerializerOptions = BuildOptions()

    Private Shared Function BuildOptions() As JsonSerializerOptions
        Dim o As New JsonSerializerOptions() With {
            .PropertyNameCaseInsensitive = True,
            .ReadCommentHandling = JsonCommentHandling.Skip,
            .AllowTrailingCommas = True,
            .WriteIndented = True,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }
        Return o
    End Function

    ''' <summary>
    ''' Parses and validates scenario JSON. Malformed JSON yields IsValid = False with a Romanian
    ''' error — no exception escapes.
    ''' </summary>
    Public Shared Function Read(json As String) As HarnessScenarioReadResult
        If String.IsNullOrWhiteSpace(json) Then
            Dim empty As New HarnessScenarioReadResult(Nothing)
            empty.Errors.Add("Fișierul de scenariu este gol.")
            Return empty
        End If

        Dim parsed As HarnessScenario
        Try
            parsed = JsonSerializer.Deserialize(Of HarnessScenario)(json, Options)
        Catch ex As JsonException
            Dim bad As New HarnessScenarioReadResult(Nothing)
            bad.Errors.Add("JSON invalid: " & ex.Message)
            Return bad
        End Try

        If parsed Is Nothing Then
            Dim nul As New HarnessScenarioReadResult(Nothing)
            nul.Errors.Add("Fișierul de scenariu nu conține un obiect JSON.")
            Return nul
        End If

        Dim result As New HarnessScenarioReadResult(parsed)
        ValidateSchema(parsed, result)
        ValidateSteps(parsed, result)
        ValidateMoves(parsed, result)
        CollectUnknownProperties(parsed, result)
        Return result
    End Function

    Private Shared Sub ValidateSchema(s As HarnessScenario, result As HarnessScenarioReadResult)
        If s.Schema = 0 Then
            result.Warnings.Add($"Câmpul «schema» lipsește — presupun versiunea {SupportedSchema}.")
            s.Schema = SupportedSchema
        ElseIf s.Schema <> SupportedSchema Then
            result.Errors.Add($"Versiune de schemă nesuportată: {s.Schema} (suportată: {SupportedSchema}).")
        End If
    End Sub

    ' An unrecognised step name is an ERROR, never a silent skip.
    Private Shared Sub ValidateSteps(s As HarnessScenario, result As HarnessScenarioReadResult)
        If s.Scenario Is Nothing OrElse s.Scenario.Count = 0 Then
            result.Warnings.Add("Lista «scenario» este goală — nu se va executa niciun pas.")
            Return
        End If
        For Each stepName As String In s.Scenario
            If Not HarnessScenarioSteps.IsKnown(stepName) Then
                result.Errors.Add($"Pas necunoscut «{stepName}». Pași valizi: {HarnessScenarioSteps.AllAsText()}.")
            End If
        Next
    End Sub

    ' A present-but-all-zero `move` is legible ("leave the window where the panel puts it") but does
    ' nothing, so it is worth a warning — a file that meant to shift the window and lost its numbers
    ' would otherwise look like it worked.
    Private Shared Sub ValidateMoves(s As HarnessScenario, result As HarnessScenarioReadResult)
        If s.Move Is Nothing Then Return
        If s.Move.IsNoOp() Then
            result.Warnings.Add("move: toate deplasările sunt 0 — secțiunea nu schimbă nimic.")
        End If
    End Sub

    ' Unknown properties are captured by JsonExtensionData and reported as warnings, never as
    ' exceptions — a newer file must still run on an older bench.
    Private Shared Sub CollectUnknownProperties(s As HarnessScenario, result As HarnessScenarioReadResult)
        WarnExtras(result, "(rădăcină)", s.Extra)
        If s.Launch IsNot Nothing Then WarnExtras(result, "launch", s.Launch.Extra)
        If s.OpenParameters IsNot Nothing Then WarnExtras(result, "openParameters", s.OpenParameters.Extra)
        If s.Clip IsNot Nothing Then WarnExtras(result, "clip", s.Clip.Extra)
        If s.HideChildren IsNot Nothing Then WarnExtras(result, "hideChildren", s.HideChildren.Extra)
        If s.Move IsNot Nothing Then WarnExtras(result, "move", s.Move.Extra)
        If s.UserPrefs IsNot Nothing Then WarnExtras(result, "userPrefs", s.UserPrefs.Extra)
        If s.MachinePolicy IsNot Nothing Then WarnExtras(result, "machinePolicy", s.MachinePolicy.Extra)
        If s.Keys IsNot Nothing Then
            For i As Integer = 0 To s.Keys.Count - 1
                If s.Keys(i) IsNot Nothing Then WarnExtras(result, $"keys[{i}]", s.Keys(i).Extra)
            Next
        End If
    End Sub

    Private Shared Sub WarnExtras(result As HarnessScenarioReadResult, section As String,
                                  extra As Dictionary(Of String, JsonElement))
        If extra Is Nothing Then Return
        For Each k As String In extra.Keys
            result.Warnings.Add($"Proprietate necunoscută ignorată: {section}.{k}")
        Next
    End Sub

End Class

' Writes a scenario back out in the same schema. Separate from the reader so the round trip is
' explicit and testable; WriteIndented keeps saved files readable and diffable.
Public NotInheritable Class HarnessScenarioWriter

    Private Sub New()
    End Sub

    Public Shared Function Write(scenario As HarnessScenario) As String
        If scenario Is Nothing Then Throw New ArgumentNullException(NameOf(scenario))
        Return JsonSerializer.Serialize(scenario, HarnessScenarioReader.Options)
    End Function

End Class
