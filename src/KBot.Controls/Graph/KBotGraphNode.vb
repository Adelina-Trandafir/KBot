Option Strict On
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' One dot of a <see cref="KBotGraphView"/>: a thing that can be linked to other things.
''' </summary>
''' <remarks>
''' <para>Nodes are RUNTIME data, never serialized by the designer: a graph is a picture of some
''' other data set, rebuilt by the host every time it is loaded.</para>
''' <para>The position (<see cref="X"/>, <see cref="Y"/>) is in WORLD units, not pixels. The view
''' maps them to the surface with its own zoom and pan, so a host that fills the positions once
''' (see <see cref="KBotGraphLayout"/>) keeps the picture still while it shows and hides nodes - the
''' point of showing a graph over time.</para>
''' </remarks>
Public NotInheritable Class KBotGraphNode

    Private ReadOnly _key As String

    ''' <summary>Created through <see cref="KBotGraphView.AddNode"/>, which validates the key.</summary>
    Friend Sub New(k_key As String, k_text As String)
        _key = k_key
        Text = If(k_text, String.Empty)
    End Sub

    ''' <summary>Unique, non-empty. How edges and events name this node.</summary>
    Public ReadOnly Property Key As String
        Get
            Return _key
        End Get
    End Property

    ''' <summary>The title of the floating label, and the label painted beside the node when labels are on.</summary>
    Public Property Text As String

    ''' <summary>The body of the floating label (accepts <c>KBotToolTip</c> markup). Empty = the title only.</summary>
    Public Property Tooltip As String = String.Empty

    ''' <summary>
    ''' Nodes with the same group are laid out together by <see cref="KBotGraphLayout"/>. Empty = alone
    ''' in a group of its own.
    ''' </summary>
    Public Property Group As String = String.Empty

    ''' <summary>The fill. <c>Color.Empty</c> = the accent of the active scheme (C1).</summary>
    Public Property NodeColor As Color = Color.Empty

    ''' <summary>Radius in LOGICAL pixels. 0 = the control's <c>NodeRadius</c>.</summary>
    Public Property Radius As Integer

    ''' <summary>False = not drawn, no edge to it is drawn, no hover. The position is kept.</summary>
    Public Property Visible As Boolean = True

    ''' <summary>A ring around the node: "look here" (for example, touched recently).</summary>
    Public Property Highlighted As Boolean

    ''' <summary>World position. Filled by the host, usually from <see cref="KBotGraphLayout.Compute"/>.</summary>
    Public Property X As Double

    ''' <summary>World position. See <see cref="X"/>.</summary>
    Public Property Y As Double

    ''' <summary>Anything the host wants back from an event.</summary>
    Public Property Tag As Object

    ''' <summary>Position of this node inside the control's list; maintained by the control.</summary>
    Friend Property Index As Integer

    Public Overrides Function ToString() As String
        Return _key
    End Function

End Class

''' <summary>A line between two nodes of a <see cref="KBotGraphView"/>.</summary>
Public NotInheritable Class KBotGraphEdge

    Friend Sub New(k_from As KBotGraphNode, k_to As KBotGraphNode)
        FromNode = k_from
        ToNode = k_to
    End Sub

    ''' <summary>One end.</summary>
    Public ReadOnly Property FromNode As KBotGraphNode

    ''' <summary>The other end. Edges have no direction on the surface; the order only says how the host stated them.</summary>
    Public ReadOnly Property ToNode As KBotGraphNode

    ''' <summary>The line colour. <c>Color.Empty</c> = a dimmed text colour from the scheme (C1).</summary>
    Public Property EdgeColor As Color = Color.Empty

    ''' <summary>True = drawn fainter. For links that are implied rather than written down.</summary>
    Public Property Faint As Boolean

    ''' <summary>Anything the host wants back.</summary>
    Public Property Tag As Object

End Class
