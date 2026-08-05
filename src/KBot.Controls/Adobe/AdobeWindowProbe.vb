Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq

''' <summary>
''' One node of Adobe's window tree, as the probe read it. Text first, because the TEXT is the
''' durable identity across relaunches: handles change on every launch (0x5083E -&gt; 0x20B66) while
''' the text (AVTaskPaneHostView, AV2DocumentTabView) does not, and class names are useless because
''' nearly everything is <c>AVL_AVView</c>.
'''
''' POCO -&gt; no Try/Catch (house rule).
''' </summary>
Public NotInheritable Class AdobeWindowNode

    Public ReadOnly Property Hwnd As IntPtr
    Public ReadOnly Property ClassName As String
    Public ReadOnly Property Text As String
    ''' <summary>Rectangle in the HOST PANEL's client coordinates.</summary>
    Public ReadOnly Property Bounds As Rectangle
    Public ReadOnly Property Visible As Boolean
    Public ReadOnly Property Depth As Integer

    Public Sub New(hwnd As IntPtr, className As String, text As String,
                   bounds As Rectangle, visible As Boolean, depth As Integer)
        Me.Hwnd = hwnd
        Me.ClassName = If(className, "")
        Me.Text = If(text, "")
        Me.Bounds = bounds
        Me.Visible = visible
        Me.Depth = depth
    End Sub

    Public ReadOnly Property Width As Integer
        Get
            Return Bounds.Width
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return Bounds.Height
        End Get
    End Property

    ''' <summary>True when this node carries <paramref name="marker"/> as its text OR its class.</summary>
    Public Function Matches(marker As String) As Boolean
        If String.IsNullOrEmpty(marker) Then Return False
        Return String.Equals(Text, marker, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(ClassName, marker, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Overrides Function ToString() As String
        Dim t As String = If(String.IsNullOrEmpty(Text), "(fără text)", Text)
        Return $"{t} — {ClassName} ({Width}x{Height})"
    End Function

End Class

''' <summary>
''' Walks the hosted window's descendants. Extracted in slice 0024 from the harness's
''' <c>WalkChildren</c> so the bench and the shipping preview read the SAME tree the same way —
''' the detection in <see cref="AdobeUiDetector"/> is only as trustworthy as this.
''' </summary>
Public NotInheritable Class AdobeWindowProbe

    Private Sub New()
    End Sub

    ''' <summary>Depth limit of the recursion (the harness's PROBE_MAX_DEPTH).</summary>
    Public Const DefaultMaxDepth As Integer = 4
    ''' <summary>Right-edge tolerance of the right-hand-pane heuristic, in pixels.</summary>
    Public Const RhpEdgeTolerance As Integer = 8

    ''' <summary>
    ''' Every descendant of <paramref name="root"/>, depth-first, with rectangles converted into
    ''' <paramref name="hostHandle"/>'s client coordinates. Returns an empty list for a dead handle
    ''' rather than throwing — a probe is a diagnostic, never a failure path.
    ''' </summary>
    Public Shared Function Walk(root As IntPtr, hostHandle As IntPtr,
                                Optional maxDepth As Integer = DefaultMaxDepth) As List(Of AdobeWindowNode)
        Dim nodes As New List(Of AdobeWindowNode)()
        If root = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(root) Then Return nodes
        WalkInto(root, hostHandle, 1, maxDepth, nodes)
        Return nodes
    End Function

    ' Private helper reached ONLY through Walk (itself only called from wrapped boundaries) —
    ' transitive coverage, house rule.
    Private Shared Sub WalkInto(parent As IntPtr, hostHandle As IntPtr, depth As Integer,
                                maxDepth As Integer, nodes As List(Of AdobeWindowNode))
        Dim child As IntPtr = AdobeNativeMethods.GetWindow(parent, AdobeNativeMethods.GW_CHILD)
        While child <> IntPtr.Zero
            Dim r As AdobeNativeMethods.RECT
            Dim bounds As Rectangle = Rectangle.Empty
            If AdobeNativeMethods.GetWindowRect(child, r) Then
                If hostHandle <> IntPtr.Zero Then
                    AdobeNativeMethods.MapWindowPoints(IntPtr.Zero, hostHandle, r, 2)
                End If
                bounds = r.ToRectangle()
            End If
            nodes.Add(New AdobeWindowNode(child,
                                          AdobeNativeMethods.GetClass(child),
                                          AdobeNativeMethods.GetTitle(child),
                                          bounds,
                                          AdobeNativeMethods.IsWindowVisible(child),
                                          depth))
            If depth < maxDepth Then WalkInto(child, hostHandle, depth + 1, maxDepth, nodes)
            child = AdobeNativeMethods.GetWindow(child, AdobeNativeMethods.GW_HWNDNEXT)
        End While
    End Sub

    ''' <summary>
    ''' The right-hand-pane candidate, as a HEURISTIC (that word is in the log too): visible, flush
    ''' against the host's right edge (± tolerance) and narrower than half the host. The widest such
    ''' child wins. Pure, so it can be tested against a recorded tree.
    ''' </summary>
    Public Shared Function RhpCandidate(nodes As IEnumerable(Of AdobeWindowNode), hostWidth As Integer) As AdobeWindowNode
        If nodes Is Nothing OrElse hostWidth <= 0 Then Return Nothing
        Dim best As AdobeWindowNode = Nothing
        For Each n As AdobeWindowNode In nodes
            If Not n.Visible OrElse n.Width <= 0 Then Continue For
            If n.Width >= hostWidth \ 2 Then Continue For
            If Math.Abs((n.Bounds.X + n.Width) - hostWidth) > RhpEdgeTolerance Then Continue For
            If best Is Nothing OrElse n.Width > best.Width Then best = n
        Next
        Return best
    End Function

    ''' <summary>One log line per node, in the format the bench log already uses.</summary>
    Public Shared Function DescribeNode(n As AdobeWindowNode) As String
        Dim txt As String = n.Text
        If txt.Length > 40 Then txt = txt.Substring(0, 40) & "…"
        Return $"  d={n.Depth} hwnd=0x{n.Hwnd.ToInt64():X} cls={n.ClassName} text=""{txt}"" " &
               $"x={n.Bounds.X} y={n.Bounds.Y} {n.Width}x{n.Height} vis={If(n.Visible, 1, 0)}"
    End Function

End Class
