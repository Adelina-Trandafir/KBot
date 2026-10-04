Option Strict On
Imports System.Drawing

''' <summary>
''' Slice 000T: what the tutorial runner tells a window when a tutorial starts or ends in it.
''' <see cref="FlowId"/> = the tutorial; <see cref="Key"/> = the free ASCII string of the flow's
''' <c>host-key:</c> header, so one window can serve several tutorials and decide per key.
''' </summary>
Public NotInheritable Class KBotTutorialRequest

    Public Sub New(k_flowId As String, k_key As String)
        FlowId = k_flowId
        Key = k_key
    End Sub

    Public ReadOnly Property FlowId As String
    Public ReadOnly Property Key As String

End Class

''' <summary>
''' Slice 000T: a window (every modal one that a tutorial walks into, and the main window) that
''' knows a tutorial state. The runner tells it «start in tutorial mode» with a key, asks it for the
''' places only it knows (named anchors) and lets it say «this step is done» for things that have
''' no clean event. The runner builds the ring / dim / bubble for that window AFTER it is shown and
''' owned by it, so a modal window never disables them.
''' </summary>
Public Interface IKBotTutorialHost

    ''' <summary>True when this window can serve the tutorial that carries <paramref name="k_key"/>.</summary>
    Function TutorialSupports(k_key As String) As Boolean

    ''' <summary>The tutorial starts (or walks into) this window. Raise nothing from here.</summary>
    Sub TutorialBegin(k_request As KBotTutorialRequest)

    ''' <summary>The tutorial left this window (finished, stopped by the user, or the window closes).</summary>
    Sub TutorialEnd()

    ''' <summary>
    ''' The screen rectangles of the named anchor (the visible rows that have reservations, the
    ''' row that carries the «+», ...). Empty = the anchor is not on screen now or unknown here.
    ''' </summary>
    Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle)

    ''' <summary>The window says a named step is done (a file was attached, a partner associated...).</summary>
    Event TutorialSignal(k_name As String)

End Interface
