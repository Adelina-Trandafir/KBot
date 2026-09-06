''' <summary>
''' One node of the ancestor chain captured for a recorded step, from the element
''' itself up to body (at most eight levels).
''' </summary>
''' <remarks>
''' Kept so that a better selector can be rebuilt later without replaying the
''' recording. Ids are never stored: Wicket regenerates them on every rerender.
''' </remarks>
Public Class RecordedAncestor

    Public Property Tag As String = String.Empty
    Public Property Classes As New List(Of String)
    Public Property NameAttr As String = String.Empty
    Public Property DataAttrs As New Dictionary(Of String, String)

    ''' <summary>Normalised inner text, truncated to 30 characters.</summary>
    Public Property Text As String = String.Empty

    ''' <summary>One based position among the parent's element children; 0 when unknown.</summary>
    Public Property IndexInParent As Integer

End Class
