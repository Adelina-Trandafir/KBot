Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq

''' <summary>Why a candidate window was accepted or rejected as «the floating Adobe badge».</summary>
Public Enum AdobePopupVerdict
    ''' <summary>All four filters passed — this window may be hidden.</summary>
    Accepted = 0
    ''' <summary>Not an <c>AVL_AVPopup</c>.</summary>
    WrongClass = 1
    ''' <summary>Belongs to a process that is not the Adobe instance we host.</summary>
    ForeignProcess = 2
    ''' <summary>Does not overlap our host rectangle — it floats over someone else's screen.</summary>
    NoIntersection = 3
    ''' <summary>Degenerate rectangle: nothing to hide.</summary>
    TooSmall = 4
    ''' <summary>Bigger than the host: that is a window, not a badge.</summary>
    TooLarge = 5
    ''' <summary>Already invisible — hiding it would prove nothing.</summary>
    NotVisible = 6
End Enum

''' <summary>
''' Decides whether a top-level window is the floating <c>AVL_AVPopup</c> badge Adobe draws over its
''' own document window.
'''
''' FOUR FILTERS, AND ALL FOUR MATTER. This code calls <c>ShowWindow(SW_HIDE)</c> on a window owned
''' by ANOTHER PROCESS, so «probably the right one» is not good enough: a popup is hidden only when
''' it is the right class, owned by the Adobe process we are hosting, overlapping OUR host rectangle
''' and of plausible badge size. Everything rejected is logged with the reason — the same rule as
''' <see cref="HideOutcomeClassifier"/>: a filter that silently drops candidates is a filter nobody
''' can debug after the next Adobe update.
'''
''' Pure by construction (no Win32 in this file), so the whole rule is unit-tested from recorded
''' rectangles.
''' </summary>
Public NotInheritable Class AdobePopupFilter

    Private Sub New()
    End Sub

    ''' <summary>The class of Adobe's floating badge window.</summary>
    Public Const PopupClass As String = "AVL_AVPopup"

    ''' <summary>Anything thinner or shorter than this is a degenerate rectangle, not a badge.</summary>
    Public Const MinSide As Integer = 4

    ''' <summary>
    ''' Evaluates one candidate. All rectangles are in SCREEN coordinates — the popup is a top-level
    ''' window, so there is no shared parent to convert into and mixing spaces here would accept
    ''' windows that are nowhere near the host.
    ''' </summary>
    Public Shared Function Evaluate(className As String, ownerPid As Integer,
                                    adobePids As IEnumerable(Of Integer),
                                    popupOnScreen As Rectangle, hostOnScreen As Rectangle,
                                    visible As Boolean) As AdobePopupVerdict
        If Not String.Equals(className, PopupClass, StringComparison.OrdinalIgnoreCase) Then
            Return AdobePopupVerdict.WrongClass
        End If

        Dim pids As List(Of Integer) = If(adobePids Is Nothing, New List(Of Integer)(), adobePids.ToList())
        If ownerPid <= 0 OrElse Not pids.Contains(ownerPid) Then Return AdobePopupVerdict.ForeignProcess

        If Not visible Then Return AdobePopupVerdict.NotVisible
        If popupOnScreen.Width < MinSide OrElse popupOnScreen.Height < MinSide Then
            Return AdobePopupVerdict.TooSmall
        End If
        If hostOnScreen.Width > 0 AndAlso hostOnScreen.Height > 0 Then
            If popupOnScreen.Width > hostOnScreen.Width OrElse popupOnScreen.Height > hostOnScreen.Height Then
                Return AdobePopupVerdict.TooLarge
            End If
            If Not popupOnScreen.IntersectsWith(hostOnScreen) Then Return AdobePopupVerdict.NoIntersection
        End If
        Return AdobePopupVerdict.Accepted
    End Function

    ''' <summary>Romanian label for the log line, one per verdict.</summary>
    Public Shared Function Label(verdict As AdobePopupVerdict) As String
        Select Case verdict
            Case AdobePopupVerdict.Accepted : Return "ACCEPTAT"
            Case AdobePopupVerdict.WrongClass : Return "RESPINS (altă clasă)"
            Case AdobePopupVerdict.ForeignProcess : Return "RESPINS (alt proces)"
            Case AdobePopupVerdict.NoIntersection : Return "RESPINS (în afara gazdei)"
            Case AdobePopupVerdict.TooSmall : Return "RESPINS (prea mic)"
            Case AdobePopupVerdict.TooLarge : Return "RESPINS (mai mare decât gazda)"
            Case Else : Return "RESPINS (deja invizibil)"
        End Select
    End Function

End Class
