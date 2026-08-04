Option Strict On
Imports System.Collections.Generic
Imports System.Text.Json
Imports Microsoft.Win32

' What a scenario ASKED FOR, for one HKCU value (slice 0023, "make the bench tell the truth" pass).
'
' WHY THIS TYPE EXISTS: userPrefs values used to be funnelled through the four checkboxes, each of
' which means "set to the one value the panel knows / leave alone". A scenario asking for
' `"bEnableAv2": 1` came out of that funnel as 0, the confirmation dialog offered `0 -> 0`, and the
' log recorded `0 (DWord) -> 0 (DWord)`. Half the schema was decorative. Intents carry the value
' LITERALLY from the file to the registry, and the value NAMES are open — any value under the
' resolved AVGeneral hive can be driven from a file without a code change.
Public Enum UserPrefAction
    ' Integer in JSON -> REG_DWORD.
    WriteDword
    ' String in JSON -> REG_SZ.
    WriteString
    ' JSON null -> delete the value. Distinct from "set it to 0", and distinct again from an
    ' absent key, which means "leave alone" and produces no intent at all.
    Delete
End Enum

Public NotInheritable Class UserPrefIntent

    Public ReadOnly Property Name As String
    Public ReadOnly Property Action As UserPrefAction
    Public ReadOnly Property Value As Object

    Public Sub New(name As String, action As UserPrefAction, value As Object)
        Me.Name = name
        Me.Action = action
        Me.Value = value
    End Sub

    Public ReadOnly Property Kind As RegistryValueKind
        Get
            Select Case Action
                Case UserPrefAction.WriteDword : Return RegistryValueKind.DWord
                Case UserPrefAction.WriteString : Return RegistryValueKind.String
                Case Else : Return RegistryValueKind.Unknown
            End Select
        End Get
    End Property

    ' What the operator is shown in the confirmation dialog and the «Cerut» column.
    Public Function RequestedText() As String
        If Action = UserPrefAction.Delete Then Return "(șters)"
        Return Convert.ToString(Value, Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Overrides Function ToString() As String
        Return $"{Name} -> {RequestedText()}"
    End Function

End Class

' Turns a scenario's `userPrefs.values` dictionary into literal intents. Pure: no registry.
Public NotInheritable Class UserPrefIntentFactory

    Private Sub New()
    End Sub

    ''' <summary>
    ''' One intent per entry. Unsupported JSON kinds (arrays/objects/booleans) are reported through
    ''' <paramref name="rejected"/> instead of being guessed at — the type is never invented.
    ''' </summary>
    Public Shared Function FromValues(values As Dictionary(Of String, JsonElement),
                                      Optional ByRef rejected As List(Of String) = Nothing) As List(Of UserPrefIntent)
        Dim intents As New List(Of UserPrefIntent)()
        If rejected Is Nothing Then rejected = New List(Of String)()
        If values Is Nothing Then Return intents

        For Each kv As KeyValuePair(Of String, JsonElement) In values
            Select Case kv.Value.ValueKind
                Case JsonValueKind.Number
                    intents.Add(New UserPrefIntent(kv.Key, UserPrefAction.WriteDword, kv.Value.GetInt32()))
                Case JsonValueKind.String
                    intents.Add(New UserPrefIntent(kv.Key, UserPrefAction.WriteString, kv.Value.GetString()))
                Case JsonValueKind.Null
                    intents.Add(New UserPrefIntent(kv.Key, UserPrefAction.Delete, Nothing))
                Case Else
                    rejected.Add($"userPrefs.values.{kv.Key}: tip JSON nesuportat ({kv.Value.ValueKind}) — ignorat.")
            End Select
        Next
        Return intents
    End Function

    ''' <summary>
    ''' Merges the manual checkbox shortcuts UNDER the scenario: a name the scenario mentions keeps
    ''' the scenario's literal intent; the checkbox only contributes names the file is silent about
    ''' (and, with no scenario loaded, all of them).
    ''' </summary>
    Public Shared Function Merge(fromScenario As IEnumerable(Of UserPrefIntent),
                                 fromCheckboxes As IEnumerable(Of UserPrefIntent)) As List(Of UserPrefIntent)
        Dim merged As New List(Of UserPrefIntent)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If fromScenario IsNot Nothing Then
            For Each i As UserPrefIntent In fromScenario
                If seen.Add(i.Name) Then merged.Add(i)
            Next
        End If
        If fromCheckboxes IsNot Nothing Then
            For Each i As UserPrefIntent In fromCheckboxes
                If seen.Add(i.Name) Then merged.Add(i)
            Next
        End If
        Return merged
    End Function

End Class
