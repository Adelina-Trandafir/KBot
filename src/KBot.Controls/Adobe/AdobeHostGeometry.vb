Option Strict On
Imports System.Drawing

' Placing the HOSTED Adobe window inside a host panel.
'
' MOVED HERE IN SLICE 0024 from KBot.DevHarness (Internal\Adobe\MoveOutcome.vb), unchanged. The
' bench and the shipping preview now compute the same rectangle from the same code — the whole point
' of the extraction. The semantics are the ones the harness already implements and are NOT redefined
' here (see docs\SETARI_UTILIZATOR.md for the operator-facing description).
'
' The bench moves and resizes the window it HOSTS — the same window clip right/top already drive —
' NOT individual Adobe child windows. dx/dy/dw/dh are the general form of the clip: clip right is a
' dw, clip top is a dy plus a dh. Everything goes through one rectangle, so a delta survives
' resizes, splitter drags and relaunches without anything re-imposing it.
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

''' <summary>
''' The whole placement in one pure function: host client size + clip + deltas -&gt; the rectangle the
''' hosted window gets. This is the extracted body of the harness's <c>HostedBounds()</c>, which is
''' why the bench and the DDF preview cannot drift apart geometrically.
''' </summary>
Public NotInheritable Class AdobeHostGeometry

    Private Sub New()
    End Sub

    Public Shared Function Compute(hostWidth As Integer, hostHeight As Integer,
                                   clipEnabled As Boolean, clipRight As Integer, clipTop As Integer,
                                   dx As Integer, dy As Integer, dw As Integer, dh As Integer) As Rectangle
        Dim b As Rectangle
        If clipEnabled Then
            ' Oversized and offset, so the clipped bands (top toolbar strip / right pane strip)
            ' fall OUTSIDE the visible client area of the host panel.
            b = New Rectangle(0, -clipTop, hostWidth + clipRight, hostHeight + clipTop)
        Else
            b = New Rectangle(0, 0, hostWidth, hostHeight)
        End If
        Return HostedWindowGeometry.Offset(b, dx, dy, dw, dh)
    End Function

    ''' <summary>The rectangle a profile asks for, given the host panel's client size.</summary>
    Public Shared Function Compute(hostSize As Size, profile As AdobeViewerProfile) As Rectangle
        If profile Is Nothing Then Return New Rectangle(0, 0, Math.Max(1, hostSize.Width), Math.Max(1, hostSize.Height))
        Return Compute(hostSize.Width, hostSize.Height,
                       profile.ClipEnabled, profile.ClipRight, profile.ClipTop,
                       profile.Dx, profile.Dy, profile.Dw, profile.Dh)
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
