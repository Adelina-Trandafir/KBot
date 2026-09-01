Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports KBot.Common

''' <summary>
''' One dated thing sitting in a <see cref="KBotLane"/> — the unit an operator drags.
''' </summary>
''' <remarks>
''' <para>A marker has a MOMENT and no value. That is the difference between this surface and the
''' chart above it: the chart answers "what was it worth", the lanes answer "which one does it
''' belong to". Squeezing a value in here would make twenty lanes of markers into twenty tiny
''' unreadable charts.</para>
''' <para><see cref="Text"/> is the marker's short name. It is the title of the floating label
''' always, and it is PAINTED beside the marker only when the host asks for labels
''' (<c>KBotLaneView.MarkerLabelsVisible</c>) — which is what the enlarged window does and the
''' compact strip does not.</para>
''' </remarks>
Public NotInheritable Class KBotLaneMarker

    Private _markerColor As Color = Color.Empty

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a marker at a moment, with the name it shows.</summary>
    Public Sub New(moment As Date, text As String)
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Moment = moment
        Me.Text = If(text, String.Empty)
    End Sub

    ''' <summary>The lane this marker sits in (Nothing for a free-standing instance).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property OwnerLane As KBotLane

    <Category("K-BOT Lane Marker")>
    <Description("When this happened. This is the horizontal axis — real time, not a slot index.")>
    Public Property Moment As Date

    <Category("K-BOT Lane Marker")>
    <Description("Short name of the marker. Title of the floating label, and painted beside the marker when the view has MarkerLabelsVisible. Read by the operator.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT Lane Marker")>
    <Description("Body of the floating label (multiple lines; accepts the rich-text markup of KBotToolTip). Empty = the moment on its own.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property Tooltip As String = String.Empty

    <Category("K-BOT Lane Marker")>
    <Description("What this marker stands for, and therefore how it is drawn. A fact about the marker, not decoration.")>
    <DefaultValue(KBotLaneMarkerStyle.Normal)>
    Public Property Style As KBotLaneMarkerStyle = KBotLaneMarkerStyle.Normal

    ''' <summary>
    ''' The colour of this marker. <c>Color.Empty</c> = the colour of its lane.
    ''' </summary>
    ''' <remarks>
    ''' Named <c>MarkerColor</c> rather than <c>Color</c> for the same reason
    ''' <c>KBotChartPoint.PointColor</c> is: VB is case-insensitive and a member shadows a type of
    ''' the same name, so a property called <c>Color</c> would make every <c>Color.Empty</c>
    ''' written inside this class resolve to the property instead of the type.
    ''' </remarks>
    <Category("K-BOT Lane Marker")>
    <Description("Colour of this marker. Empty = the colour of its lane. Set it to the colour the same fact carries on the chart, so the eye pairs the two.")>
    Public Property MarkerColor As Color
        Get
            Return _markerColor
        End Get
        Set(value As Color)
            _markerColor = value
            OwnerLane?.InvalidateOwnerView()
        End Set
    End Property

    Public Function ShouldSerializeMarkerColor() As Boolean
        Return _markerColor <> Color.Empty
    End Function

    Public Sub ResetMarkerColor()
        MarkerColor = Color.Empty
    End Sub

    <Category("K-BOT Lane Marker")>
    <Description("False => the marker keeps its place in the lane but is neither drawn, nor hit-tested, nor draggable.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    ''' <summary>Whatever the host wants to find again. Runtime handle, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>
    ''' Where the last layout pass put this marker, in client pixels. Device pixels on purpose —
    ''' this is a painting result, not an operator setting, so nothing unscales it on the way back.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property PlotLocation As Point = Point.Empty

    ''' <summary>False while the marker is off the drawn surface and carries no location.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Plotted As Boolean

    Public Overrides Function ToString() As String
        Return $"{Moment:g} {If(Text, String.Empty)}".TrimEnd()
    End Function
End Class

''' <summary>
''' The markers of one lane, in the order the host added them.
''' </summary>
''' <remarks>
''' They are NOT sorted here, for the same reason the chart does not sort its points: a host that
''' feeds them out of order gets a visible defect rather than a chart that always looks plausible.
''' Two markers at the same moment are legal and both drawn — several saves inside one minute is
''' exactly the case this surface was built for.
''' </remarks>
Public NotInheritable Class KBotLaneMarkerCollection
    Inherits Collection(Of KBotLaneMarker)

    ''' <summary>The lane that owns this collection (Nothing for a free-standing instance).</summary>
    Friend Property Owner As KBotLane

    ' The four mutators carry their own Try/Catch because they are ENTRY POINTS — the designer
    ' calls them from InitializeComponent and code calls them directly, so there is no already
    ' wrapped boundary above them. Boundary classification: log and RE-THROW, never swallow.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotLaneMarker)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerLane = Owner
            MyBase.InsertItem(index, item)
            Owner?.InvalidateOwnerView()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneMarkerCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotLaneMarker)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerLane = Owner
            MyBase.SetItem(index, item)
            Owner?.InvalidateOwnerView()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneMarkerCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            If index >= 0 AndAlso index < Count Then Me(index).OwnerLane = Nothing
            MyBase.RemoveItem(index)
            Owner?.InvalidateOwnerView()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneMarkerCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each m As KBotLaneMarker In Me
                m.OwnerLane = Nothing
            Next
            MyBase.ClearItems()
            Owner?.InvalidateOwnerView()
        Catch ex As Exception
            LogUnlessDesignTime("KBotLaneMarkerCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner?.OwnerView) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
