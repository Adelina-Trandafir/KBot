Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' DRAGGING A MARKER FROM ONE LANE TO ANOTHER — the gesture this whole surface exists for.
'''
''' <para><b>The same three events, in the same order, as
''' <c>AdvancedTreeControl.Drag.vb</c></b>, and for the same reasons. The one that matters is
''' <see cref="KBotLaneView.MarkerDragOver"/>: the host says "yes" or "no, here is why" BEFORE the
''' operator lets go, and the surface shows the answer under the pointer. A veto discovered after
''' the drop is an error message; a veto discovered during it is a cursor saying no.</para>
'''
''' <para><b><c>DoDragDrop</c> from WinForms</b>, not a hand-rolled mouse chase. The reason is the
''' cursor: the system's modal loop gives the "cannot drop here" cursor for free and ends the drag
''' correctly on ESC or on losing the window. The welcome side effect is that <c>MouseUp</c> no
''' longer reaches the control after a drag, so a drop cannot also be read as a click.</para>
'''
''' <para><b>Nothing moves on its own.</b> The control does NOT move the marker: it raises
''' <see cref="KBotLaneView.MarkerDropped"/> and stops. The lanes are a projection of the host's
''' picture — a marker moved locally would show a placement nobody has recorded yet, which is
''' exactly the lie a surface built to make placements visible cannot afford.</para>
'''
''' <para><b>The dragged marker is read from the DATA OBJECT, never from a private field.</b> That
''' is what makes dragging BETWEEN two of these work: the field is only filled on the control that
''' STARTED the drag, and the target is a different instance that would see it empty. The data
''' object is the one thing both of them can see, and it answers the same way when the source and
''' the target are one control. This trap was caught once already, in slice 0048-04, between two
''' trees.</para>
'''
''' <para><b>The outline is drawn on refusal too</b>, in the error colour. A lane that does not
''' react at all reads as "the surface did not see me"; one with a red outline says "I saw you, and
''' not here" — and the label beside it says why.</para>
''' </summary>
Partial Public NotInheritable Class KBotLaneView

    ' ── Arming ───────────────────────────────────────────────────────────────
    ' Where the button went down, so we know when the system's drag threshold is passed. Without
    ' it, any click with a pixel of tremor would start a drag.
    Private _dragOrigin As Point = Point.Empty
    Private _dragCandidate As KBotLaneMarker = Nothing

    ' ── While the drag lasts ─────────────────────────────────────────────────
    Private _dragSource As KBotLaneMarker = Nothing
    Private _dropTarget As KBotLane = Nothing
    Private _dropAllowed As Boolean = False
    Private _dropReason As String = String.Empty

    ' Which target is "in the label" now. Without this guard every pixel of movement over the same
    ' lane would reschedule the label and the reason would never actually appear.
    Private _dropTipTarget As KBotLane = Nothing

    Private _dragHighlightColor As Color = Color.Empty
    Private _dragForbiddenColor As Color = Color.Empty

    ' ══════════════════════════════════════════════════════════════════════════
    ' Properties
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Outline colour of the lane that can receive the marker. <c>Color.Empty</c> = the accent of
    ''' the theme.
    ''' </summary>
    <Category("K-BOT Lane Drag")>
    <Description("Outline of the lane that accepts the marker. Empty = the accent colour of the theme.")>
    Public Property DragHighlightColor As Color
        Get
            Return _dragHighlightColor
        End Get
        Set(value As Color)
            _dragHighlightColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeDragHighlightColor() As Boolean
        Return _dragHighlightColor <> Color.Empty
    End Function

    Public Sub ResetDragHighlightColor()
        DragHighlightColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Outline colour of the lane that refuses the marker. <c>Color.Empty</c> = the error colour
    ''' of the theme — the one place in this control where red is the right answer, because here it
    ''' really does mean "no".
    ''' </summary>
    <Category("K-BOT Lane Drag")>
    <Description("Outline of the lane that refuses the marker. Empty = the error colour of the theme.")>
    Public Property DragForbiddenColor As Color
        Get
            Return _dragForbiddenColor
        End Get
        Set(value As Color)
            _dragForbiddenColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeDragForbiddenColor() As Boolean
        Return _dragForbiddenColor <> Color.Empty
    End Function

    Public Sub ResetDragForbiddenColor()
        DragForbiddenColor = Color.Empty
    End Sub

    ''' <summary>
    ''' <c>AllowDrop</c> is OURS, not the host's, so it never reaches the host's <c>.Designer.vb</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para>The constructor turns it on, because this control exists in order to be dropped on.
    ''' The trouble is that the inherited <c>AllowDrop</c> carries <c>DefaultValue(False)</c>, so
    ''' the designer sees a value differing from the default and writes <c>x.AllowDrop = True</c>
    ''' into every host form — a line nobody chose, which then reads as a deliberate operator
    ''' setting forever. That is the trap CONTROLS.md C4 is about, in a property we inherited
    ''' rather than one we declared. Caught by measuring: a fresh lane view serialized
    ''' <c>AllowDrop</c> where a fresh chart serialized nothing but the three lines Visual Studio
    ''' writes for any control at all.</para>
    ''' <para><b>A <c>ShouldSerializeAllowDrop</c> here does NOT work</b>, and that was tried
    ''' first. <c>TypeDescriptor</c> builds an inherited property's descriptor against the type
    ''' that DECLARES it, so it looks for the method on <c>Control</c> and never sees ours. The
    ''' answer is the one the project notes already record for this case: shadow the property and
    ''' mark the shadow, because shadowing does not inherit the base's serialization
    ''' attributes.</para>
    ''' </remarks>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property AllowDrop As Boolean
        Get
            Return MyBase.AllowDrop
        End Get
        Set(value As Boolean)
            MyBase.AllowDrop = value
        End Set
    End Property

    ''' <summary>The marker being dragged right now, or <c>Nothing</c>. Read-only.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DraggedMarker As KBotLaneMarker
        Get
            Return _dragSource
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' Events
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' A marker is about to be dragged. The host says whether it may be.
    '''
    ''' <para>This is where a marker that must not move is stopped — one the server would refuse —
    ''' so the operator feels the refusal before making the gesture rather than after.</para>
    ''' </summary>
    Public Event MarkerDragStarting(sender As Object, e As LaneDragStartEventArgs)

    ''' <summary>
    ''' The pointer is over a lane during a drag. The host says whether the drop is allowed and, if
    ''' not, WHY — the reason appears as a floating label, in the operator's language.
    ''' </summary>
    Public Event MarkerDragOver(sender As Object, e As LaneDragOverEventArgs)

    ''' <summary>
    ''' The marker was dropped on a lane that answered "yes". The control moves NOTHING — the host
    ''' decides what the placement means and rebuilds the surface.
    ''' </summary>
    Public Event MarkerDropped(sender As Object, e As LaneDropEventArgs)

    ' ══════════════════════════════════════════════════════════════════════════
    ' Starting — called from OnMouseDown / OnMouseMove
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Remembers where a drag COULD start from. Starts nothing yet.</summary>
    Private Sub ArmDrag(m As KBotLaneMarker, p As Point, button As MouseButtons)
        If button <> MouseButtons.Left Then
            _dragCandidate = Nothing
            Return
        End If
        _dragCandidate = m
        _dragOrigin = p
    End Sub

    ''' <summary>Forgets an armed drag that never became one (the button came up first).</summary>
    Private Sub CancelDragArming()
        _dragCandidate = Nothing
    End Sub

    ''' <summary>
    ''' Starts the drag if the pointer has moved far enough. Returns True when it did — and the
    ''' rest of <c>OnMouseMove</c> then has nothing left to do, because the system's modal loop has
    ''' only just returned.
    ''' </summary>
    Private Function MaybeBeginDrag(p As Point, button As MouseButtons) As Boolean
        If _dragCandidate Is Nothing Then Return False
        If (button And MouseButtons.Left) <> MouseButtons.Left Then
            _dragCandidate = Nothing
            Return False
        End If

        Dim threshold As Size = SystemInformation.DragSize
        If Math.Abs(p.X - _dragOrigin.X) < threshold.Width AndAlso
           Math.Abs(p.Y - _dragOrigin.Y) < threshold.Height Then Return False

        Dim m As KBotLaneMarker = _dragCandidate
        _dragCandidate = Nothing

        Dim start As New LaneDragStartEventArgs(m, m.OwnerLane)
        RaiseEvent MarkerDragStarting(Me, start)
        If start.Cancel Then Return False

        _dragSource = m
        _dropTarget = Nothing
        _dropAllowed = False
        _dropReason = String.Empty
        HideLaneTip()
        Try
            ' The system's modal loop. It returns only on the drop, on ESC or on losing the
            ' window — all three leave by the same road, through CancelDrag below.
            DoDragDrop(m, DragDropEffects.Move)
        Finally
            CancelDrag()
        End Try
        Return True
    End Function

    ''' <summary>Puts out every trace of a drag. Safe to call at any time.</summary>
    Private Sub CancelDrag()
        Dim wasSomething As Boolean = _dragSource IsNot Nothing OrElse _dropTarget IsNot Nothing
        _dragSource = Nothing
        _dragCandidate = Nothing
        _dropTarget = Nothing
        _dropAllowed = False
        _dropReason = String.Empty
        _dropTipTarget = Nothing
        _markerTooltip?.HideNow()
        _currentTipKey = Nothing
        If wasSomething Then Invalidate()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The target — the drop overrides
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The dragged marker, read FROM THE DATA OBJECT rather than from our own field.
    ''' </summary>
    ''' <remarks>
    ''' See the note on this file: the field is only filled on the control that started the drag,
    ''' so a second lane view would see it empty. The drag never leaves the process, so the marker
    ''' travels as a reference and does not have to be serializable.
    ''' </remarks>
    Private Shared Function MarkerFromData(data As IDataObject) As KBotLaneMarker
        If data Is Nothing Then Return Nothing
        If Not data.GetDataPresent(GetType(KBotLaneMarker)) Then Return Nothing
        Return TryCast(data.GetData(GetType(KBotLaneMarker)), KBotLaneMarker)
    End Function

    Protected Overrides Sub OnDragEnter(drgevent As DragEventArgs)
        Try
            MyBase.OnDragEnter(drgevent)
            drgevent.Effect = DragDropEffects.None
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.OnDragEnter", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDragOver(drgevent As DragEventArgs)
        Try
            MyBase.OnDragOver(drgevent)
            drgevent.Effect = DragDropEffects.None

            Dim source As KBotLaneMarker = MarkerFromData(drgevent.Data)
            If source Is Nothing Then Return

            EnsureLayout()
            Dim p As Point = PointToClient(New Point(drgevent.X, drgevent.Y))
            Dim idx As Integer = LaneIndexAt(p)
            Dim target As KBotLane = If(idx >= 0, _lanes(idx), Nothing)

            ' A lane that is not a target is not offered at all — the host's veto never sees it,
            ' because "this lane is a heading, not a destination" is a fact about the lane rather
            ' than a judgement about this marker.
            If target IsNot Nothing AndAlso Not target.IsTarget Then target = Nothing

            Dim allow As Boolean = False
            Dim reason As String = String.Empty
            If target IsNot Nothing Then
                Dim args As New LaneDragOverEventArgs(source, source.OwnerLane, target)
                RaiseEvent MarkerDragOver(Me, args)
                allow = args.Allow
                reason = If(args.Reason, String.Empty)
            End If

            If Not ReferenceEquals(target, _dropTarget) OrElse allow <> _dropAllowed Then
                _dropTarget = target
                _dropAllowed = allow
                _dropReason = reason
                Invalidate()
            Else
                _dropReason = reason
            End If

            drgevent.Effect = If(allow, DragDropEffects.Move, DragDropEffects.None)
            ShowRefusalReason(target, allow)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.OnDragOver", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDragLeave(e As EventArgs)
        Try
            MyBase.OnDragLeave(e)
            If _dropTarget IsNot Nothing Then
                _dropTarget = Nothing
                _dropAllowed = False
                Invalidate()
            End If
            _dropTipTarget = Nothing
            _markerTooltip?.HideNow()
            _currentTipKey = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.OnDragLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDragDrop(drgevent As DragEventArgs)
        Try
            MyBase.OnDragDrop(drgevent)
            ' From the data object, not from the field: on a drag between two lane views the
            ' target did not start the drag and has nothing in `_dragSource`.
            Dim source As KBotLaneMarker = MarkerFromData(drgevent.Data)
            Dim target As KBotLane = _dropTarget
            Dim allowed As Boolean = _dropAllowed
            Dim from As KBotLane = If(source Is Nothing, Nothing, source.OwnerLane)
            CancelDrag()

            If Not allowed OrElse source Is Nothing OrElse target Is Nothing Then Return
            RaiseEvent MarkerDropped(Me, New LaneDropEventArgs(source, from, target))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.OnDragDrop", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The label carrying the reason
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Shows, beside the lane, WHY the marker cannot go there. Only on refusal, and only once per
    ''' lane: without the guard on <c>_dropTipTarget</c> every pixel of movement over the same lane
    ''' would reschedule the label and it would never get as far as appearing.
    ''' </summary>
    Private Sub ShowRefusalReason(target As KBotLane, allowed As Boolean)
        If allowed OrElse target Is Nothing OrElse String.IsNullOrWhiteSpace(_dropReason) Then
            If _dropTipTarget IsNot Nothing Then
                _dropTipTarget = Nothing
                _markerTooltip?.HideNow()
                _currentTipKey = Nothing
            End If
            Return
        End If
        If ReferenceEquals(target, _dropTipTarget) Then Return

        _dropTipTarget = target
        ' Through the ordinary label road, so the drag reason and the hover labels cannot end up
        ' fighting over the same window. The key is the lane, so it changes when the lane does.
        ShowLaneTip($"no:{If(target.Key, String.Empty)}", "Nu se poate aici", _dropReason, Nothing)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Painting — called from OnPaint, inside the surface clip
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The outline on the lane under the pointer: the accent when it accepts, the error colour
    ''' when it refuses.
    ''' </summary>
    ''' <remarks>
    ''' It reads <c>_dropTarget</c>, NOT <c>_dragSource</c>: on a drag between two lane views the
    ''' one drawing the target is the one that did not start the drag. Drawn on refusal too — see
    ''' the note at the top of this file.
    ''' </remarks>
    Private Sub DrawDropTarget(g As Graphics)
        If _dropTarget Is Nothing OrElse _dropTarget.Bounds.Height <= 0 Then Return

        Dim c As Color
        If _dropAllowed Then
            c = If(_dragHighlightColor = Color.Empty, Palette().AccentColor, _dragHighlightColor)
        Else
            c = If(_dragForbiddenColor = Color.Empty, Palette().ErrorColor, _dragForbiddenColor)
        End If

        Dim r As Rectangle = _dropTarget.Bounds
        r.Width = Math.Max(1, r.Width - 1)
        r.Height = Math.Max(1, r.Height - 1)

        ' A thin wash under the outline: at 150% a one-pixel frame is lost on a tall row.
        Using fillBrush As New SolidBrush(Color.FromArgb(40, c))
            g.FillRectangle(fillBrush, r)
        End Using
        Using pen As New Pen(c, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
            pen.Alignment = PenAlignment.Inset
            g.DrawRectangle(pen, r)
        End Using
    End Sub
End Class

''' <summary>Arguments of <see cref="KBotLaneView.MarkerDragStarting"/>.</summary>
Public NotInheritable Class LaneDragStartEventArgs
    Inherits EventArgs

    Public Sub New(marker As KBotLaneMarker, lane As KBotLane)
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Marker = marker
        Me.Lane = lane
    End Sub

    ''' <summary>The marker the operator started to drag.</summary>
    Public ReadOnly Property Marker As KBotLaneMarker

    ''' <summary>The lane it is leaving.</summary>
    Public ReadOnly Property Lane As KBotLane

    ''' <summary>Set it True and the marker cannot be dragged.</summary>
    Public Property Cancel As Boolean
End Class

''' <summary>Arguments of <see cref="KBotLaneView.MarkerDragOver"/>.</summary>
Public NotInheritable Class LaneDragOverEventArgs
    Inherits EventArgs

    Public Sub New(marker As KBotLaneMarker, from As KBotLane, target As KBotLane)
        Me.Marker = marker
        Me.From = from
        Me.Target = target
    End Sub

    ''' <summary>The marker being dragged.</summary>
    Public ReadOnly Property Marker As KBotLaneMarker

    ''' <summary>The lane it is on now. <c>Nothing</c> for a free-standing marker.</summary>
    Public ReadOnly Property From As KBotLane

    ''' <summary>The lane under the pointer.</summary>
    Public ReadOnly Property Target As KBotLane

    ''' <summary>
    ''' Defaults to False: <b>refusal is the default</b>. A host that forgets to answer lets
    ''' nothing through, instead of letting everything through.
    ''' </summary>
    Public Property Allow As Boolean

    ''' <summary>
    ''' Why not — in the operator's language, ready to show. Ignored when <see cref="Allow"/> is
    ''' True.
    ''' </summary>
    Public Property Reason As String = String.Empty
End Class

''' <summary>Arguments of <see cref="KBotLaneView.MarkerDropped"/>.</summary>
Public NotInheritable Class LaneDropEventArgs
    Inherits EventArgs

    Public Sub New(marker As KBotLaneMarker, from As KBotLane, target As KBotLane)
        Me.Marker = marker
        Me.From = from
        Me.Target = target
    End Sub

    ''' <summary>The marker that was dragged.</summary>
    Public ReadOnly Property Marker As KBotLaneMarker

    ''' <summary>The lane it came from. <c>Nothing</c> for a free-standing marker.</summary>
    Public ReadOnly Property From As KBotLane

    ''' <summary>The lane it was dropped on.</summary>
    Public ReadOnly Property Target As KBotLane
End Class
