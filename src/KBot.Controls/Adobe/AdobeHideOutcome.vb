Option Strict On
Imports System.Collections.Generic
Imports System.Linq

' What actually happened to one window we asked to hide.
'
' MOVED HERE IN SLICE 0024 from KBot.DevHarness (Internal\Adobe\HideOutcome.vb), unchanged — the
' popup watcher in the shipping preview reports with exactly the same vocabulary as the bench.
'
' WHY: the old code logged «1 din 1 texte găsite» and called it a success. On 04.08 the task pane
' host was already 0×0 and invisible BEFORE the scenario started, so every run hid a window that
' was already hidden and reported clean. "Found" is true and useless — only "changed" is evidence.
Public Enum HideOutcome
    ' Was visible with a non-zero rectangle; ShowWindow(SW_HIDE) applied. The only real result.
    Hidden
    ' Matched but vis=0 already — nothing was changed.
    AlreadyHidden
    ' Matched but the rectangle is 0×0 — there is nothing to hide.
    ZeroSize
    ' The text is not in the window tree at all.
    NotFound
End Enum

Public NotInheritable Class HideOutcomeClassifier

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Classification order matters: a not-found window is not a zero-size one, and a window that
    ''' is both invisible AND zero-sized is reported as ZeroSize, because the size is the stronger
    ''' statement about why hiding it proves nothing.
    ''' </summary>
    Public Shared Function Classify(found As Boolean, visible As Boolean,
                                    width As Integer, height As Integer) As HideOutcome
        If Not found Then Return HideOutcome.NotFound
        If width <= 0 OrElse height <= 0 Then Return HideOutcome.ZeroSize
        If Not visible Then Return HideOutcome.AlreadyHidden
        Return HideOutcome.Hidden
    End Function

    ' Romanian label used in the per-window log lines.
    Public Shared Function Label(outcome As HideOutcome) As String
        Select Case outcome
            Case HideOutcome.Hidden : Return "ASCUNS"
            Case HideOutcome.AlreadyHidden : Return "DEJA ASCUNS"
            Case HideOutcome.ZeroSize : Return "DIMENSIUNE ZERO"
            Case Else : Return "NEGĂSIT"
        End Select
    End Function

End Class

' Aggregates one attempt's outcomes into the summary line and the "did anything change?" verdict.
Public NotInheritable Class HideAttemptSummary

    Public ReadOnly Property Outcomes As IReadOnlyList(Of HideOutcome)
    Public ReadOnly Property Requested As Integer

    Public Sub New(requested As Integer, outcomes As IEnumerable(Of HideOutcome))
        Me.Requested = requested
        Me.Outcomes = If(outcomes Is Nothing, New List(Of HideOutcome)(), outcomes.ToList())
    End Sub

    ' Enumerable.Count(...) spelled out: IReadOnlyList already has a parameterless Count PROPERTY,
    ' which shadows the LINQ extension in VB.
    Public ReadOnly Property HiddenCount As Integer
        Get
            Return Enumerable.Count(Outcomes, Function(o) o = HideOutcome.Hidden)
        End Get
    End Property

    Public ReadOnly Property FoundCount As Integer
        Get
            Return Enumerable.Count(Outcomes, Function(o) o <> HideOutcome.NotFound)
        End Get
    End Property

    Public ReadOnly Property InertCount As Integer
        Get
            Return Enumerable.Count(Outcomes,
                Function(o) o = HideOutcome.AlreadyHidden OrElse o = HideOutcome.ZeroSize)
        End Get
    End Property

    ''' <summary>True when the step matched windows but changed nothing — the signature of a
    ''' contaminated baseline, and the case the old summary line hid.</summary>
    Public ReadOnly Property ChangedNothing As Boolean
        Get
            Return HiddenCount = 0
        End Get
    End Property

    Public Function SummaryLine(attempt As Integer, attempts As Integer) As String
        Dim s As String = $"hideChildren: încercarea {attempt}/{attempts} — {FoundCount} găsit(e): " &
                          $"{HiddenCount} ascunse, {InertCount} deja ascuns/zero."
        If ChangedNothing Then s &= " ATENȚIE: nicio schimbare reală."
        Return s
    End Function

End Class
