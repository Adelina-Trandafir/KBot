Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports KBot.Common

''' <summary>
''' One horizontal row of a <see cref="KBotLaneView"/>: the thing that markers belong TO.
''' </summary>
''' <remarks>
''' <para>A lane is a destination. Its markers are on it because somebody put them there, and the
''' whole point of the surface is that moving one from lane to lane is a single short drag whose
''' consequences are visible in the same glance.</para>
''' <para><see cref="EndMark"/> is the lane's own verdict on itself, shown at its closed end —
''' F15 of <c>docs/FUNDAMENT_Asociere_Receptii.md</c> as a SIGN, which is the only thing F15 has
''' ever been. The control computes nothing: the host knows what "closes correctly" means for its
''' data and says so.</para>
''' </remarks>
Public NotInheritable Class KBotLane

    Private ReadOnly _markers As New KBotLaneMarkerCollection()
    Private _laneColor As Color = Color.Empty

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
        _markers.Owner = Me
    End Sub

    ''' <summary>Convenience for code: a named lane with no markers yet.</summary>
    Public Sub New(key As String, text As String)
        Me.New()
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Key = key
        Me.Text = If(text, String.Empty)
    End Sub

    ''' <summary>The view that draws this lane (Nothing for a free-standing instance).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property OwnerView As KBotLaneView

    <Category("K-BOT Lane")>
    <Description("Identifier reported by the drag events and accepted by FindLane. Must be non-empty and unique.")>
    Public Property Key As String

    <Category("K-BOT Lane")>
    <Description("Short name of the lane. Title of the floating label, and painted at the left when the view has LaneCaptionsVisible. Read by the operator.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT Lane")>
    <Description("Body of the floating label (multiple lines; accepts the rich-text markup of KBotToolTip). Empty = the lane name on its own.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property Tooltip As String = String.Empty

    ''' <summary>
    ''' The colour of the lane and, unless a marker overrides it, of its markers.
    ''' </summary>
    ''' <remarks>
    ''' <c>Color.Empty</c> = derived from the palette, like everything else in this codebase — but
    ''' a host that has the SAME facts on a second surface should write the colour explicitly
    ''' (from <c>KBotLaneView.AutoColor</c>), because a colour nobody wrote down is a colour the
    ''' other surface cannot be told about.
    ''' </remarks>
    <Category("K-BOT Lane")>
    <Description("Colour of the lane and its markers. Empty = derived from the active theme.")>
    Public Property LaneColor As Color
        Get
            Return _laneColor
        End Get
        Set(value As Color)
            _laneColor = value
            InvalidateOwnerView()
        End Set
    End Property

    Public Function ShouldSerializeLaneColor() As Boolean
        Return _laneColor <> Color.Empty
    End Function

    Public Sub ResetLaneColor()
        LaneColor = Color.Empty
    End Sub

    ''' <summary>
    ''' False => nothing may be dropped here, and the lane is never offered as a target.
    ''' </summary>
    ''' <remarks>
    ''' Distinct from the <c>Allow</c> of <c>MarkerDragOver</c>: this one is about the LANE ("a
    ''' heading, not a destination"), the other about this marker on this lane right now. A lane
    ''' that is not a target never reaches the host's veto at all.
    ''' </remarks>
    <Category("K-BOT Lane")>
    <Description("False => the lane is drawn but can never receive a drop, and is never offered as a target.")>
    <DefaultValue(True)>
    Public Property IsTarget As Boolean = True

    <Category("K-BOT Lane")>
    <Description("The mark at the closed end of the lane: does it finish where it should? A sign, never a refusal — the view computes nothing, the host says so.")>
    <DefaultValue(KBotLaneEndMark.None)>
    Public Property EndMark As KBotLaneEndMark = KBotLaneEndMark.None

    ''' <summary>
    ''' True => a separator line is drawn ABOVE this lane.
    ''' </summary>
    ''' <remarks>
    ''' A general mechanism rather than a hard-wired "the last lane is special": the surface this
    ''' was written for puts the unplaced markers under a line, but nothing about a lane view says
    ''' there can only be one such division, and a control that decided which lane is the odd one
    ''' out would be guessing at the host's meaning.
    ''' </remarks>
    <Category("K-BOT Lane")>
    <Description("True => a separator line is drawn above this lane, cutting the surface in two.")>
    <DefaultValue(False)>
    Public Property SeparatorAbove As Boolean

    <Category("K-BOT Lane")>
    <Description("False => the lane takes no place at all and its markers are not drawn.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT Lane")>
    <Description("The markers on this lane. They are NOT sorted for you: markers out of order are an honest sign that the caller's query is.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Markers As KBotLaneMarkerCollection
        Get
            Return _markers
        End Get
    End Property

    ''' <summary>Whatever the host wants to find again. Runtime handle, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>Convenience for code: append a marker and return it.</summary>
    Public Function AddMarker(moment As Date, text As String) As KBotLaneMarker
        Dim m As New KBotLaneMarker(moment, text)
        _markers.Add(m)
        Return m
    End Function

    ''' <summary>Where the last layout pass put this lane, in client pixels.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Bounds As Rectangle = Rectangle.Empty

    ''' <summary>Ask the owning view for a fresh layout. Called by the marker collection.</summary>
    Friend Sub InvalidateOwnerView()
        OwnerView?.InvalidateLaneLayout()
    End Sub

    Public Overrides Function ToString() As String
        Return $"{If(Key, String.Empty)} ({_markers.Count})"
    End Function
End Class

''' <summary>
''' The lanes of a <see cref="KBotLaneView"/>, top to bottom. Every mutation lays the view out
''' again, so an edit made in the designer collection dialog repaints itself.
''' </summary>
''' <remarks>
''' <b>Key validation does NOT live here</b>, deliberately: the collection dialog inserts an empty
''' lane the moment Add is pressed, long before anything has been typed into it. The contract
''' (non-empty, unique key) is enforced in <c>KBotLaneView.EndInit</c> and in the runtime methods.
''' </remarks>
Public NotInheritable Class KBotLaneCollection
    Inherits Collection(Of KBotLane)

    ''' <summary>The view that owns this collection (Nothing for a free-standing instance).</summary>
    Friend Property Owner As KBotLaneView

    ' Entry points: log and RE-THROW, never swallow — a surface that silently loses a lane is
    ' exactly the failure the house rule bans.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotLane)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerView = Owner
            MyBase.InsertItem(index, item)
            Owner?.InvalidateLaneLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotLane)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerView = Owner
            MyBase.SetItem(index, item)
            Owner?.InvalidateLaneLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            If index >= 0 AndAlso index < Count Then Me(index).OwnerView = Nothing
            MyBase.RemoveItem(index)
            Owner?.InvalidateLaneLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each ln As KBotLane In Me
                ln.OwnerView = Nothing
            Next
            MyBase.ClearItems()
            Owner?.InvalidateLaneLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
