Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

' Persistent record that the harness left an HKLM policy applied (slice 0023).
'
' WHY A FILE AND NOT A FIELD: the contamination on 03.08 survived the bench being closed. In-memory
' state cannot warn the next session, so the marker lives in AppDir\Config\harness_machine_state.json
' and is read on open, written on apply, and cleared only after a verified revert.
Public NotInheritable Class MachineStateMarker

    Public Const FileName As String = "harness_machine_state.json"

    <JsonPropertyName("policyApplied")>
    Public Property PolicyApplied As Boolean

    <JsonPropertyName("product")>
    Public Property Product As String

    <JsonPropertyName("appliedAt")>
    Public Property AppliedAt As String

    <JsonPropertyName("revertRegFile")>
    Public Property RevertRegFile As String

    ' Pre-apply snapshot, as "path\name = value" / "path\name = (absent)" lines, so a revert can be
    ' reconstructed by hand if the .reg file is gone.
    <JsonPropertyName("preApply")>
    Public Property PreApply As List(Of String)

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)

End Class

' Outcome of reading the marker: a corrupt file is "unknown, warn" — never an exception, and never
' silently treated as "nothing outstanding".
Public Enum MarkerReadStatus
    None
    Present
    Corrupt
End Enum

Public NotInheritable Class MachineStateMarkerResult

    Public ReadOnly Property Status As MarkerReadStatus
    Public ReadOnly Property Marker As MachineStateMarker
    Public ReadOnly Property [Error] As String

    Public Sub New(status As MarkerReadStatus, marker As MachineStateMarker, [error] As String)
        Me.Status = status
        Me.Marker = marker
        Me.[Error] = [error]
    End Sub

    ''' <summary>True when the operator must be warned: an outstanding policy, or a file we could
    ''' not read (which might be hiding one).</summary>
    Public ReadOnly Property NeedsWarning As Boolean
        Get
            If Status = MarkerReadStatus.Corrupt Then Return True
            Return Status = MarkerReadStatus.Present AndAlso Marker IsNot Nothing AndAlso Marker.PolicyApplied
        End Get
    End Property

End Class

' Serialization only — the caller supplies and stores the text, so the round trip is testable
' without touching the disk.
Public NotInheritable Class MachineStateMarkerStore

    Private Sub New()
    End Sub

    Private Shared ReadOnly Options As New JsonSerializerOptions() With {
        .PropertyNameCaseInsensitive = True,
        .ReadCommentHandling = JsonCommentHandling.Skip,
        .AllowTrailingCommas = True,
        .WriteIndented = True,
        .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    }

    Public Shared Function Serialize(marker As MachineStateMarker) As String
        If marker Is Nothing Then Throw New ArgumentNullException(NameOf(marker))
        Return JsonSerializer.Serialize(marker, Options)
    End Function

    Public Shared Function Parse(json As String) As MachineStateMarkerResult
        If String.IsNullOrWhiteSpace(json) Then
            Return New MachineStateMarkerResult(MarkerReadStatus.None, Nothing, Nothing)
        End If
        Try
            Dim m As MachineStateMarker = JsonSerializer.Deserialize(Of MachineStateMarker)(json, Options)
            If m Is Nothing Then
                Return New MachineStateMarkerResult(MarkerReadStatus.Corrupt, Nothing,
                                                    "Fișierul de stare nu conține un obiect JSON.")
            End If
            Return New MachineStateMarkerResult(MarkerReadStatus.Present, m, Nothing)
        Catch ex As JsonException
            ' Corrupt is NOT "nothing outstanding" — it may be hiding an applied policy.
            Return New MachineStateMarkerResult(MarkerReadStatus.Corrupt, Nothing,
                                                "Fișier de stare corupt: " & ex.Message)
        End Try
    End Function

    Public Shared Function PathFor(configDir As String) As String
        Return IO.Path.Combine(configDir, MachineStateMarker.FileName)
    End Function

End Class
