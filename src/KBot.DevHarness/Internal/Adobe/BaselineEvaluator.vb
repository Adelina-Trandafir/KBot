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
Public Enum BaselineVerdict
    ' No HKLM policy value present — results mean something.
    Clean
    ' Policy present; warn and let the operator decide.
    Warn
    ' Policy present and the scenario demands a clean baseline — refuse to run.
    Block
End Enum

' One policy value as read from HKLM (read-only; reading needs no elevation).
Public NotInheritable Class PolicyReading

    Public ReadOnly Property Path As String
    Public ReadOnly Property Name As String
    Public ReadOnly Property Present As Boolean
    Public ReadOnly Property Value As Object

    Public Sub New(path As String, name As String, present As Boolean, value As Object)
        Me.Path = path
        Me.Name = name
        Me.Present = present
        Me.Value = value
    End Sub

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

    ' Operator-facing text naming every active policy value.
    Public Function Describe() As String
        If Active.Count = 0 Then Return "Nicio politică HKLM activă."
        Dim sb As New StringBuilder()
        For Each r As PolicyReading In Active
            sb.AppendLine("  " & r.ToString())
        Next
        Return sb.ToString().TrimEnd()
    End Function

    Public Function WarningText() As String
        Return "Politica HKLM este activă pe această mașină:" & Environment.NewLine &
               Describe() & Environment.NewLine & Environment.NewLine &
               "Ea suprimă serviciile Adobe și poate face ca panoul de instrumente să fie deja gol " &
               "sau de dimensiune zero. Rezultatele acestui scenariu nu vor fi concludente." &
               Environment.NewLine & Environment.NewLine & "Continui?"
    End Function

    Public Function BlockedText() As String
        Return "Scenariul cere o bază curată (requireCleanBaseline), dar politica HKLM este activă:" &
               Environment.NewLine & Describe() & Environment.NewLine & Environment.NewLine &
               "Revocă politica (secțiunea «Politici Adobe») și reia scenariul."
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
