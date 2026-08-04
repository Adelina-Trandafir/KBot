Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq

' What one move attempt actually DID (slice 0023, pass 6 — moving child windows).
'
' Same rule as HideOutcome: the log must distinguish a real change from a no-op. A move that lands
' on the rectangle the window already had proves nothing about whether the mechanism works, and
' after the call everything looks "moved" — so the before/after rectangles are captured and
' compared explicitly rather than assumed.
Public Enum MoveOutcome
    ' The window moved: the rectangle after the call differs from the one before.
    Moved
    ' No window carries that text.
    NotFound
    ' Deltas were all zero, or the window was already at the target rectangle.
    Unchanged
    ' SetWindowPos returned False (window died mid-call, or the parent refused it).
    Failed
End Enum

Public NotInheritable Class MoveOutcomeClassifier

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Compares the rectangles the caller actually read before and after the call. Both are in the
    ''' PARENT'S CLIENT coordinates — mixing those with screen coordinates is the one way this
    ''' whole feature goes wrong, so the caller converts once and passes converted values here.
    ''' </summary>
    Public Shared Function Classify(found As Boolean, apiSucceeded As Boolean,
                                    before As Rectangle, after As Rectangle) As MoveOutcome
        If Not found Then Return MoveOutcome.NotFound
        If Not apiSucceeded Then Return MoveOutcome.Failed
        If before = after Then Return MoveOutcome.Unchanged
        Return MoveOutcome.Moved
    End Function

    Public Shared Function Label(outcome As MoveOutcome) As String
        Select Case outcome
            Case MoveOutcome.Moved : Return "MUTAT"
            Case MoveOutcome.NotFound : Return "NEGĂSIT"
            Case MoveOutcome.Unchanged : Return "NESCHIMBAT"
            Case Else : Return "EȘUAT"
        End Select
    End Function

    ' «x,y wxh» — the form the probe log already uses, so the two are readable side by side.
    Public Shared Function Describe(r As Rectangle) As String
        Return $"{r.X},{r.Y} {r.Width}x{r.Height}"
    End Function

End Class

' One line's worth of result, kept as data so the runner can summarise without re-reading windows.
Public NotInheritable Class MoveAttempt

    Public ReadOnly Property Text As String
    Public ReadOnly Property Outcome As MoveOutcome
    Public ReadOnly Property Before As Rectangle
    Public ReadOnly Property After As Rectangle

    Public Sub New(text As String, outcome As MoveOutcome, before As Rectangle, after As Rectangle)
        Me.Text = text
        Me.Outcome = outcome
        Me.Before = before
        Me.After = after
    End Sub

    Public Function LogLine() As String
        Dim head As String = $"  {MoveOutcomeClassifier.Label(Outcome)}: «{Text}»"
        If Outcome = MoveOutcome.NotFound Then Return head
        Return head & $" {MoveOutcomeClassifier.Describe(Before)} -> {MoveOutcomeClassifier.Describe(After)}"
    End Function

End Class

Public NotInheritable Class MoveAttemptSummary

    Public ReadOnly Property Attempts As IReadOnlyList(Of MoveAttempt)

    Public Sub New(attempts As IEnumerable(Of MoveAttempt))
        Me.Attempts = If(attempts Is Nothing, New List(Of MoveAttempt)(), attempts.ToList())
    End Sub

    Public ReadOnly Property MovedCount As Integer
        Get
            ' Enumerable.Count(seq, predicate): IReadOnlyList.Count is a PROPERTY and shadows the
            ' LINQ extension, so the fluent form does not compile here.
            Return Enumerable.Count(Attempts, Function(a) a.Outcome = MoveOutcome.Moved)
        End Get
    End Property

    Public ReadOnly Property NotFoundCount As Integer
        Get
            Return Enumerable.Count(Attempts, Function(a) a.Outcome = MoveOutcome.NotFound)
        End Get
    End Property

    Public ReadOnly Property FailedCount As Integer
        Get
            Return Enumerable.Count(Attempts, Function(a) a.Outcome = MoveOutcome.Failed)
        End Get
    End Property

    Public ReadOnly Property ChangedNothing As Boolean
        Get
            Return Attempts.Count > 0 AndAlso MovedCount = 0
        End Get
    End Property

    Public Function SummaryLine() As String
        If Attempts.Count = 0 Then Return "moveChildren: nicio intrare — nimic de mutat."
        Dim s As String = $"moveChildren: {Attempts.Count} intrare(i) — {MovedCount} mutate, " &
                          $"{NotFoundCount} negăsite, {FailedCount} eșuate."
        If ChangedNothing Then s &= " ATENȚIE: nicio schimbare reală."
        Return s
    End Function

End Class

' The rectangle a window had BEFORE the bench first moved it, keyed by window TEXT.
'
' Keyed by text, not HWND, for the same reason the hide list is: handles change on every launch
' while «AVSplitterView» does not. Capture is IDEMPOTENT — the same rule as RegistrySnapshotSet:
' a second move must not overwrite the true original, or "readu la poziția inițială" would restore
' the machine to a state the operator produced rather than the one Adobe did.
Public NotInheritable Class MoveOriginStore

    Private ReadOnly _origins As New Dictionary(Of String, Rectangle)(StringComparer.OrdinalIgnoreCase)

    Public Sub Capture(text As String, rect As Rectangle)
        If String.IsNullOrWhiteSpace(text) Then Return
        If _origins.ContainsKey(text) Then Return
        _origins(text) = rect
    End Sub

    Public Function TryGet(text As String, ByRef rect As Rectangle) As Boolean
        If String.IsNullOrWhiteSpace(text) Then Return False
        Return _origins.TryGetValue(text, rect)
    End Function

    Public ReadOnly Property Count As Integer
        Get
            Return _origins.Count
        End Get
    End Property

    Public Function Texts() As IReadOnlyList(Of String)
        Return _origins.Keys.ToList()
    End Function

    Public Sub Clear()
        _origins.Clear()
    End Sub

End Class
