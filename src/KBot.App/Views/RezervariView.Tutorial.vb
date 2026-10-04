Imports KBot.Common

''' <summary>
''' Slice 000T: the places of this view a tutorial points at. The «+» is a button painted on ONE leaf
''' of the tree (the first reservation not yet turned into a document), so no control name can say
''' where it is: the tree reports the row buttons on screen.
''' </summary>
Partial Public Class RezervariView

    ''' <summary>Anchor: the row button («+») that makes a document out of a reservation.</summary>
    Friend Const AnchorPlus As String = "rezervari.plus"

    ''' <summary>The screen rectangles of the named anchor; empty when it is not on screen or not ours.</summary>
    Friend Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle)
        Dim k_none As New List(Of Rectangle)()
        Try
            If Not String.Equals(k_name, AnchorPlus, StringComparison.OrdinalIgnoreCase) Then Return k_none
            Return tree.RightIconRectsOnScreen().Select(Function(r) tree.RectangleToScreen(r)).ToList()
        Catch ex As Exception
            ' UI boundary (called by the tutorial's timer): not readable = not on screen.
            GlobalErrorLog.Write("RezervariView.TutorialAnchor", ex)
            Return k_none
        End Try
    End Function

End Class
