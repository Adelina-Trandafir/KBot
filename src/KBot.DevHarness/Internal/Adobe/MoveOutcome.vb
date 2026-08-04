Option Strict On
Imports System.Drawing

' Placing the HOSTED Adobe window inside pnlHost (slice 0023, pass 6).
'
' The bench moves and resizes the window it hosts — the same window clip right/top already drive —
' NOT individual Adobe child windows. dx/dy/dw/dh are the general form of the clip: clip right is a
' dw, clip top is a dy plus a dh. Everything the harness places goes through one rectangle, so a
' delta survives resizes, splitter drags and relaunches without anything re-imposing it.
Public NotInheritable Class HostedWindowGeometry

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Applies the deltas to a base rectangle. Negative dx/dy pull the window left/up so the band
    ''' at that edge leaves the visible area; dw/dh grow it past the opposite edge. Width and height
    ''' are floored at 1: a zero-sized window would vanish with no way back except a relaunch.
    ''' </summary>
    Public Shared Function Offset(base As Rectangle, dx As Integer, dy As Integer,
                                  dw As Integer, dh As Integer) As Rectangle
        Return New Rectangle(base.X + dx, base.Y + dy,
                             Math.Max(1, base.Width + dw), Math.Max(1, base.Height + dh))
    End Function

    Public Shared Function IsNeutral(dx As Integer, dy As Integer, dw As Integer, dh As Integer) As Boolean
        Return dx = 0 AndAlso dy = 0 AndAlso dw = 0 AndAlso dh = 0
    End Function

End Class

' What a placement actually DID.
'
' Same rule as HideOutcome: the log must distinguish a real change from a no-op. Adobe can refuse or
' clamp a size, and after the call the window looks placed either way — so the rectangles are read
' before and after and compared explicitly rather than assumed.
Public Enum MoveOutcome
    ' The rectangle after the call differs from the one before.
    Moved
    ' There is no window to place.
    NotFound
    ' Deltas were all zero, or the window was already at that rectangle.
    Unchanged
    ' The placement call itself failed.
    Failed
End Enum

Public NotInheritable Class MoveOutcomeClassifier

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Compares the rectangles the caller actually read before and after. Both must be in the
    ''' PARENT'S CLIENT coordinates — mixing those with screen coordinates is the one way this
    ''' produces plausible numbers that mean nothing, so the caller converts once and passes
    ''' converted values here.
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

    ' «x,y wxh» — the form the probe log already uses, so the two read side by side.
    Public Shared Function Describe(r As Rectangle) As String
        Return $"{r.X},{r.Y} {r.Width}x{r.Height}"
    End Function

End Class
