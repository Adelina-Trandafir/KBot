Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

' Whether the machine is fit to draw conclusions from (slice 0023).
'
' WHY: on 03.08 an HKLM policy was applied with no matching revert. It stayed active, suppressed
' Adobe's document services, and by 04.08 the tools pane was 0×0 — so four scenario runs "passed"
' while testing nothing. A contaminated baseline must be surfaced BEFORE the first step, not
' reconstructed from logs afterwards.
'
' PASS 5: the HKLM policy is not the only contaminant. A baseline read on 04.08 came back "curată"
' while HKCU still held bRHPSticky = 1 and aDefaultRHPViewMode_L = "Collapsed" — Adobe remembering
' that it should keep the pane collapsed, left over from an earlier Shift+F4. Those decide how the
' pane behaves just as surely as a policy does, so they belong in the same verdict.
Public Enum BaselineVerdict
    ' Nothing set — results mean something.
    Clean
    ' Something is set; warn and let the operator decide.
    Warn
    ' Something is set and the scenario demands a clean baseline — refuse to run.
    Block
End Enum

' Where a contaminating value lives. Only used for wording and for telling the operator which of
' the two cleanup routes applies (elevated revert vs «șterge» + «Aplică valori HKCU»).
Public Enum BaselineOrigin
    ' HKLM\SOFTWARE\Policies — machine-wide, needs elevation to change.
    MachinePolicy
    ' HKCU\…\AVGeneral — per user, the bench can clear it itself.
    UserPreference
End Enum

' One value as read from the registry (read-only; reading HKLM\SOFTWARE\Policies needs no rights).
Public NotInheritable Class PolicyReading

    Public ReadOnly Property Path As String
    Public ReadOnly Property Name As String
    Public ReadOnly Property Present As Boolean
    Public ReadOnly Property Value As Object
    Public ReadOnly Property Origin As BaselineOrigin

    Public Sub New(path As String, name As String, present As Boolean, value As Object,
                   Optional origin As BaselineOrigin = BaselineOrigin.MachinePolicy)
        Me.Path = path
        Me.Name = name
        Me.Present = present
        Me.Value = value
        Me.Origin = origin
    End Sub

    Public ReadOnly Property OriginLabel As String
        Get
            Return If(Origin = BaselineOrigin.UserPreference, "[HKCU preferință]", "[HKLM politică]")
        End Get
    End Property

    Public Overrides Function ToString() As String
        If Not Present Then Return $"{Path}\{Name} = (absent)"
        Return $"{Path}\{Name} = {Value}"
    End Function

End Class

Public NotInheritable Class BaselineAssessment

    Public ReadOnly Property Verdict As BaselineVerdict
    Public ReadOnly Property Active As IReadOnlyList(Of PolicyReading)

    Public Sub New(verdict As BaselineVerdict, active As IEnumerable(Of PolicyReading))
        Me.Verdict = verdict
        Me.Active = If(active Is Nothing, New List(Of PolicyReading)(), active.ToList())
    End Sub

    Public ReadOnly Property IsClean As Boolean
        Get
            Return Verdict = BaselineVerdict.Clean
        End Get
    End Property

    Public ReadOnly Property Policies As IReadOnlyList(Of PolicyReading)
        Get
            Return Active.Where(Function(r) r.Origin = BaselineOrigin.MachinePolicy).ToList()
        End Get
    End Property

    Public ReadOnly Property Preferences As IReadOnlyList(Of PolicyReading)
        Get
            Return Active.Where(Function(r) r.Origin = BaselineOrigin.UserPreference).ToList()
        End Get
    End Property

    ' Operator-facing text naming every value that is set, tagged with where it lives.
    Public Function Describe() As String
        If Active.Count = 0 Then Return "Mașina este neutră: nicio politică HKLM și nicio preferință RHP în HKCU."
        Dim sb As New StringBuilder()
        For Each r As PolicyReading In Active
            sb.AppendLine("  " & r.OriginLabel & " " & r.ToString())
        Next
        Return sb.ToString().TrimEnd()
    End Function

    ' Why each kind matters, said only when that kind is actually present.
    Private Function Consequences() As String
        Dim sb As New StringBuilder()
        If Policies.Count > 0 Then
            sb.AppendLine("Politica HKLM suprimă serviciile Adobe și poate face ca panoul de instrumente " &
                          "să fie deja gol sau de dimensiune zero.")
        End If
        If Preferences.Count > 0 Then
            sb.AppendLine("Preferințele HKCU înseamnă că Adobe își amintește deja o anumită stare a " &
                          "panoului (colapsat / lipit), deci proba nu pornește de la zero.")
        End If
        Return sb.ToString().TrimEnd()
    End Function

    ' How to get back to neutral — different route per origin.
    Private Function Cleanup() As String
        Dim sb As New StringBuilder()
        If Policies.Count > 0 Then
            sb.AppendLine("Revocă politica din secțiunea «Politici Adobe» («Revocă (cere elevare)»).")
        End If
        If Preferences.Count > 0 Then
            sb.AppendLine("Pune rândurile HKCU implicate pe «șterge» și apasă «Aplică și repornește Adobe».")
        End If
        Return sb.ToString().TrimEnd()
    End Function

    Public Function WarningText() As String
        Return "Starea de pornire NU este neutră:" & Environment.NewLine &
               Describe() & Environment.NewLine & Environment.NewLine &
               Consequences() & Environment.NewLine &
               "Rezultatele acestui scenariu nu vor fi concludente." &
               Environment.NewLine & Environment.NewLine & "Continui?"
    End Function

    Public Function BlockedText() As String
        Return "Scenariul cere o bază curată (requireCleanBaseline), dar mașina nu este neutră:" &
               Environment.NewLine & Describe() & Environment.NewLine & Environment.NewLine &
               Consequences() & Environment.NewLine & Environment.NewLine &
               Cleanup() & Environment.NewLine & "Apoi reia scenariul."
    End Function

End Class

Public NotInheritable Class BaselineEvaluator

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Clean when no reading is present; otherwise Block if the scenario demands a clean baseline,
    ''' else Warn.
    ''' </summary>
    Public Shared Function Evaluate(readings As IEnumerable(Of PolicyReading),
                                    requireCleanBaseline As Boolean) As BaselineAssessment
        Dim active As List(Of PolicyReading) =
            If(readings Is Nothing, New List(Of PolicyReading)(), readings.Where(Function(r) r.Present).ToList())
        If active.Count = 0 Then Return New BaselineAssessment(BaselineVerdict.Clean, active)
        If requireCleanBaseline Then Return New BaselineAssessment(BaselineVerdict.Block, active)
        Return New BaselineAssessment(BaselineVerdict.Warn, active)
    End Function

End Class
