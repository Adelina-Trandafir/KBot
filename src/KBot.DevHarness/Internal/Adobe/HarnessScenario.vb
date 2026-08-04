Option Strict On
Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Text.Json.Serialization

' Scenario file model (slice 0023, config+layout pass). Plain classes with System.Text.Json
' attributes: NO I/O and NO Windows API calls in this file — serialization only, so the whole
' model is unit-testable without a registry, a window or Adobe.
'
' Three rules the shape encodes deliberately:
'   * A scenario carries SETTINGS, never a document. The PDF is always chosen by the operator with
'     «Deschide PDF…»; loading a scenario pre-sets the left panel, and the file picked afterwards
'     opens under those settings. So there is no `document` section — a file path in a scenario
'     would be meaningless on anyone else's machine anyway. (Files written against the older
'     schema still load: `document` simply lands in Extra and is reported as an unknown property.)
'   * An ABSENT section is Nothing and means "leave that alone". It is NOT the same as a present
'     section that turns something off (e.g. `"clip": { "enabled": false }`). Every section is a
'     reference type, never default-constructed, and every scalar inside is Nullable, so both
'     states stay distinguishable after parsing.
'   * Child windows are addressed by window TEXT, never by HWND: the probe log proves handles
'     change on every launch (0x5083E -> 0x20B66) while the text (AVTaskPaneHostView) does not.
'     Class names are useless here too — nearly everything is AVL_AVView.

Public NotInheritable Class HarnessScenario
    <JsonPropertyName("schema")>
    Public Property Schema As Integer

    <JsonPropertyName("name")>
    Public Property Name As String

    <JsonPropertyName("note")>
    Public Property Note As String

    <JsonPropertyName("launch")>
    Public Property Launch As LaunchConfig

    <JsonPropertyName("openParameters")>
    Public Property OpenParameters As OpenParametersConfig

    <JsonPropertyName("clip")>
    Public Property Clip As ClipConfig

    <JsonPropertyName("hideChildren")>
    Public Property HideChildren As HideChildrenConfig

    <JsonPropertyName("keys")>
    Public Property Keys As List(Of KeyStepConfig)

    <JsonPropertyName("userPrefs")>
    Public Property UserPrefs As UserPrefsConfig

    <JsonPropertyName("machinePolicy")>
    Public Property MachinePolicy As MachinePolicyConfig

    ' When true the run REFUSES to start if any HKLM policy value is present, instead of only
    ' warning. An outstanding policy suppresses Adobe's services and can leave the tools pane empty
    ' or zero-sized, which silently invalidates every result — that is what happened on 04.08.
    <JsonPropertyName("requireCleanBaseline")>
    Public Property RequireCleanBaseline As Boolean

    <JsonPropertyName("scenario")>
    Public Property Scenario As List(Of String)

    ' Unknown properties are captured (not rejected) so the reader can warn about each one.
    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

Public NotInheritable Class LaunchConfig
    <JsonPropertyName("newInstance")>
    Public Property NewInstance As Boolean?

    <JsonPropertyName("noSplash")>
    Public Property NoSplash As Boolean?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

' The /A open parameters. Adobe's own syntax: integers except pagemode, which is a string.
Public NotInheritable Class OpenParametersConfig
    <JsonPropertyName("toolbar")>
    Public Property Toolbar As Integer?

    <JsonPropertyName("navpanes")>
    Public Property Navpanes As Integer?

    <JsonPropertyName("statusbar")>
    Public Property Statusbar As Integer?

    <JsonPropertyName("messages")>
    Public Property Messages As Integer?

    <JsonPropertyName("scrollbar")>
    Public Property Scrollbar As Integer?

    <JsonPropertyName("pagemode")>
    Public Property Pagemode As String

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

Public NotInheritable Class ClipConfig
    <JsonPropertyName("enabled")>
    Public Property Enabled As Boolean?

    <JsonPropertyName("right")>
    Public Property Right As Integer?

    <JsonPropertyName("top")>
    Public Property Top As Integer?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

' Hides are re-resolved after every embed because HWNDs change on every launch; Adobe also
' creates the task pane host AFTER the main view, so a single attempt right after embed often
' finds nothing — hence the retry knobs.
Public NotInheritable Class HideChildrenConfig
    Public Const DefaultReapplyAttempts As Integer = 10
    Public Const DefaultReapplyIntervalMs As Integer = 400

    <JsonPropertyName("byText")>
    Public Property ByText As List(Of String)

    <JsonPropertyName("reapplyOnRelaunch")>
    Public Property ReapplyOnRelaunch As Boolean?

    <JsonPropertyName("reapplyAttempts")>
    Public Property ReapplyAttempts As Integer?

    <JsonPropertyName("reapplyIntervalMs")>
    Public Property ReapplyIntervalMs As Integer?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)

    Public Function EffectiveAttempts() As Integer
        If ReapplyAttempts.HasValue AndAlso ReapplyAttempts.Value > 0 Then Return ReapplyAttempts.Value
        Return DefaultReapplyAttempts
    End Function

    Public Function EffectiveIntervalMs() As Integer
        If ReapplyIntervalMs.HasValue AndAlso ReapplyIntervalMs.Value > 0 Then Return ReapplyIntervalMs.Value
        Return DefaultReapplyIntervalMs
    End Function
End Class

Public NotInheritable Class KeyStepConfig
    <JsonPropertyName("send")>
    Public Property Send As String

    <JsonPropertyName("delayMsBefore")>
    Public Property DelayMsBefore As Integer?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

' hive accepts "auto" / "Acrobat Reader" / "Adobe Acrobat" — the same three values as cboHive.
' Values are mixed-type by design (dword 0/1 and the REG_SZ "Collapsed"), so they stay as raw
' JsonElement and are converted at the write boundary, never guessed here.
Public NotInheritable Class UserPrefsConfig
    <JsonPropertyName("hive")>
    Public Property Hive As String

    <JsonPropertyName("values")>
    Public Property Values As Dictionary(Of String, JsonElement)

    <JsonPropertyName("restoreOnClose")>
    Public Property RestoreOnClose As Boolean?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)
End Class

' apply defaults to FALSE: a scenario file may arrive from outside, and a machine-wide policy
' write must never happen just because a step name appears in the list.
Public NotInheritable Class MachinePolicyConfig
    <JsonPropertyName("product")>
    Public Property Product As String

    <JsonPropertyName("apply")>
    Public Property Apply As Boolean

    <JsonPropertyName("values")>
    Public Property Values As Dictionary(Of String, JsonElement)

    ' Default TRUE: an elevated apply with no matching revert is how the machine got contaminated
    ' on 03.08 and stayed that way for a day.
    <JsonPropertyName("revertOnClose")>
    Public Property RevertOnClose As Boolean?

    <JsonExtensionData>
    Public Property Extra As Dictionary(Of String, JsonElement)

    Public Function ShouldRevertOnClose() As Boolean
        Return RevertOnClose.GetValueOrDefault(True)
    End Function
End Class

' The recognised step names. An unrecognised name aborts the run — a typo in a file sent from
' outside must be loud, never silently skipped.
Public NotInheritable Class HarnessScenarioSteps

    Private Sub New()
    End Sub

    Public Const ApplyUserPrefs As String = "applyUserPrefs"
    Public Const ApplyMachinePolicy As String = "applyMachinePolicy"
    Public Const Launch As String = "launch"
    Public Const WaitForEmbed As String = "waitForEmbed"
    Public Const SendKeys As String = "sendKeys"
    Public Const Probe As String = "probe"
    Public Const HideChildren As String = "hideChildren"
    Public Const ApplyClip As String = "applyClip"
    Public Const RestoreUserPrefs As String = "restoreUserPrefs"
    Public Const RevertMachinePolicy As String = "revertMachinePolicy"

    Public Shared ReadOnly All As String() = {
        ApplyUserPrefs, ApplyMachinePolicy, Launch, WaitForEmbed, SendKeys,
        Probe, HideChildren, ApplyClip, RestoreUserPrefs, RevertMachinePolicy}

    Public Shared Function IsKnown(stepName As String) As Boolean
        If String.IsNullOrWhiteSpace(stepName) Then Return False
        For Each s As String In All
            If String.Equals(s, stepName, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    Public Shared Function AllAsText() As String
        Return String.Join(", ", All)
    End Function

End Class
