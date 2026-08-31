Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports KBot.Common

''' <summary>
''' One measured moment of a <see cref="KBotChartSeries"/>: when it happened and what the value
''' was then.
''' </summary>
''' <remarks>
''' <para>The horizontal axis is a real time axis, not a slot index, so two points one minute
''' apart sit one minute apart. That matters for the thing this control was written for: several
''' snapshots of the same receipt can share a day, and drawing them evenly spaced would invent a
''' rhythm the data does not have.</para>
''' <para>The three tooltip fields are what the floating label shows while the pointer rests on
''' the point. All three are optional; a point with none of them still gets a label built from its
''' moment and value, so a host that fills nothing still says something true.</para>
''' </remarks>
Public NotInheritable Class KBotChartPoint

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a point at a moment, with a value.</summary>
    Public Sub New(moment As Date, value As Double)
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Moment = moment
        Me.Value = value
    End Sub

    <Category("K-BOT Chart Point")>
    <Description("When this measurement happened. This is the horizontal axis — real time, not a slot index.")>
    Public Property Moment As Date

    <Category("K-BOT Chart Point")>
    <Description("The measured value at that moment. This is the vertical axis.")>
    Public Property Value As Double

    ''' <summary>
    ''' The colour of this point's marker AND of the line segment that leaves it towards the next
    ''' point — the segment takes the colour of the point on its LEFT.
    ''' </summary>
    ''' <remarks>
    ''' <para>Named <c>PointColor</c> rather than <c>Color</c> on purpose. VB is case-insensitive
    ''' and a member shadows a type of the same name, so a property called <c>Color</c> would make
    ''' every <c>Color.Empty</c> written inside this class resolve to the property instead of the
    ''' type — a silent breakage this codebase has already been bitten by once.</para>
    ''' <para>Colouring points individually is what lets a host tie a chart to a list beside it:
    ''' the row and the point that mean the same thing carry the same colour, so the eye pairs
    ''' them without a legend. Left at <see cref="Color.Empty"/> the point simply follows its
    ''' series, which is what a chart that has nothing to pair with wants.</para>
    ''' </remarks>
    <Category("K-BOT Chart Point")>
    <Description("Colour of this point's marker and of the segment leaving it towards the next point. Empty = the colour of the series.")>
    Public Property PointColor As Color = Color.Empty

    Public Function ShouldSerializePointColor() As Boolean
        Return PointColor <> Color.Empty
    End Function

    Public Sub ResetPointColor()
        PointColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Point")>
    <Description("Title line of the floating label shown while the pointer rests on this point. Empty = the series name.")>
    Public Property TooltipHeader As String = String.Empty

    <Category("K-BOT Chart Point")>
    <Description("Body of the floating label (multiple lines; accepts the rich-text markup of KBotToolTip). Empty = moment and value.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    Public Property TooltipText As String = String.Empty

    <Category("K-BOT Chart Point")>
    <Description("Footer line of the floating label. Empty = no footer.")>
    Public Property TooltipFooter As String = String.Empty

    ''' <summary>
    ''' Whatever the host wants to find again when this point is hovered or clicked. Never
    ''' serialized: it is a runtime handle, not a designer setting.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>
    ''' Where the last layout pass put this point, in client pixels. Device pixels on purpose —
    ''' this is a painting result, not an operator setting, so nothing unscales it on the way back.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property PlotLocation As Point = Point.Empty

    ''' <summary>False while the point falls outside the plotted range and carries no location.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Plotted As Boolean

    Public Overrides Function ToString() As String
        Return $"{Moment:g} = {Value}"
    End Function
End Class

''' <summary>
''' The ordered points of one series. Every mutation invalidates the owner's layout, so an edit
''' made in the designer collection dialog repaints itself instead of waiting for a resize.
''' </summary>
''' <remarks>
''' The points are NOT sorted here. A host that feeds them out of order gets a line that walks
''' backwards, which is a visible defect and therefore an honest one; silently sorting would hide
''' a bug in the caller's query behind a chart that always looks plausible.
''' </remarks>
Public NotInheritable Class KBotChartPointCollection
    Inherits Collection(Of KBotChartPoint)

    ''' <summary>The series that owns this collection (Nothing for a free-standing instance).</summary>
    Friend Property Owner As KBotChartSeries

    ' The four mutators carry their own Try/Catch because they are ENTRY POINTS — the designer
    ' calls them from InitializeComponent and code calls them directly, so there is no already
    ' wrapped boundary above them. Boundary classification: log and RE-THROW, never swallow.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotChartPoint)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            Owner?.InvalidateOwnerLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartPointCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotChartPoint)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.SetItem(index, item)
            Owner?.InvalidateOwnerLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartPointCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.InvalidateOwnerLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartPointCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.InvalidateOwnerLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartPointCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner?.OwnerChart) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
