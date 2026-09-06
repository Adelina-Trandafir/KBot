''' <summary>
''' One raw event captured from the browser while recording: a click, a keystroke,
''' a value change, a blur or an Enter/Tab press.
''' </summary>
''' <remarks>
''' The raw trail is never edited in place by the compactor - the operator's edits
''' (chosen candidate, per step WaitFor, LogValue, logical delete) live here and the
''' compactor reads them. Deleting is logical so that indexes stay stable.
''' </remarks>
Public Class RecordedStep

    ''' <summary>Zero based position in the raw trail. Never renumbered.</summary>
    Public Property Index As Integer

    ''' <summary>click | input | change | key | blur</summary>
    Public Property Kind As String = String.Empty

    ''' <summary>Enter or Tab for <see cref="Kind"/> = "key"; empty otherwise.</summary>
    Public Property KeyName As String = String.Empty

    Public Property Timestamp As DateTime
    Public Property Url As String = String.Empty

    Public Property Tag As String = String.Empty
    Public Property TypeAttr As String = String.Empty
    Public Property NameAttr As String = String.Empty
    Public Property Value As String = String.Empty
    Public Property Text As String = String.Empty

    ''' <summary>
    ''' select2-mask | select2-open | select2-search | select2-pick | wicket-select |
    ''' paginator | row-action | none
    ''' </summary>
    Public Property Widget As String = "none"

    Public Property Candidates As New List(Of SelectorCandidate)

    ''' <summary>Index into <see cref="Candidates"/>. Editable from the recorder form.</summary>
    Public Property SelectedCandidateIndex As Integer = 0

    Public Property Path As New List(Of RecordedAncestor)

    ''' <summary>True when the step was followed by a Wicket AJAX round trip.</summary>
    Public Property TriggeredAjax As Boolean

    ''' <summary>Milliseconds between the step and the confirmed Wicket idle; 0 when unknown.</summary>
    Public Property IdleAfterMs As Integer

    ''' <summary>Per step override for the automatic WaitFor insertion.</summary>
    Public Property InsertWaitFor As Boolean

    ''' <summary>Operator visible label written as LogValue in the generated workflow.</summary>
    Public Property LogValue As String = String.Empty

    ''' <summary>Logical delete: the step stays in the list but is skipped on generation.</summary>
    Public Property Deleted As Boolean

    ''' <summary>The candidate currently selected, or an empty string when there is none.</summary>
    Public ReadOnly Property ChosenSelector As String
        Get
            If Candidates Is Nothing OrElse Candidates.Count = 0 Then Return String.Empty
            Dim i As Integer = SelectedCandidateIndex
            If i < 0 OrElse i >= Candidates.Count Then i = 0
            Return If(Candidates(i).Selector, String.Empty)
        End Get
    End Property

End Class
