''' <summary>
''' One ranked selector proposal for a recorded step, produced by Recorder.js.
''' Ranking is by <see cref="Score"/> descending; the operator can override the
''' choice from the recorder form before the workflow is generated.
''' </summary>
''' <remarks>
''' Deliberately NOT part of the WorkflowModels namespace: the recorder must not
''' add anything to the workflow model classes.
''' </remarks>
Public Class SelectorCandidate

    ''' <summary>The selector itself, ready to be written into a .wfl file.</summary>
    Public Property Selector As String = String.Empty

    ''' <summary>
    ''' How the selector was built: name-strict, data-attr, has-text,
    ''' ancestor-anchor, class-path or nth-child.
    ''' </summary>
    Public Property Strategy As String = String.Empty

    ''' <summary>Higher is better. Reduced by 25 when the selector is not unique.</summary>
    Public Property Score As Integer

    ''' <summary>
    ''' How many elements the selector matched at capture time. -1 means the count
    ''' could not be established from JavaScript.
    ''' </summary>
    Public Property MatchCount As Integer

    ''' <summary>True for selectors that are not unique or that rely on position.</summary>
    Public Property Fragile As Boolean

End Class
