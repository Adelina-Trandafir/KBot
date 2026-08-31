Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' One line of a <see cref="KBotChartView"/>: a key, a name for the legend and the label, and the
''' ordered points it walks through.
''' </summary>
''' <remarks>
''' <para><see cref="LineColor"/> follows the house convention: <c>Color.Empty</c> means "from the
''' theme", and the chart then hands out a colour derived from the active palette, so a series
''' the host never coloured still changes with the scheme. Anything set explicitly wins and keeps
''' winning.</para>
''' <para><see cref="Emphasis"/> exists for the one case that made this control necessary: a
''' whole-commitment view draws one line per receipt plus a thicker total line, and the total is
''' not a fourth receipt — it is a different kind of statement and has to read as one.</para>
''' </remarks>
Public NotInheritable Class KBotChartSeries

    Private ReadOnly _points As New KBotChartPointCollection()
    Private _lineColor As Color = Color.Empty

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
        _points.Owner = Me
    End Sub

    ''' <summary>Convenience for code: a named series with no points yet.</summary>
    Public Sub New(key As String, text As String)
        Me.New()
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Key = key
        Me.Text = If(text, String.Empty)
    End Sub

    ''' <summary>The chart that owns this series (Nothing for a free-standing instance).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property OwnerChart As KBotChartView

    <Category("K-BOT Chart Series")>
    <Description("Identifier used by SetSeriesVisible / FindSeries and reported by the PointClicked event. Must be non-empty and unique.")>
    Public Property Key As String

    <Category("K-BOT Chart Series")>
    <Description("The name shown in the legend and, unless the point overrides it, in the title of the floating label.")>
    Public Property Text As String = String.Empty

    <Category("K-BOT Chart Series")>
    <Description("Colour of the line and its markers. Empty = derived from the active theme, so the series follows the scheme.")>
    Public Property LineColor As Color
        Get
            Return _lineColor
        End Get
        Set(value As Color)
            _lineColor = value
            OwnerChart?.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Keeps a colour nobody chose out of the host's .Designer.vb. Without it Visual Studio would
    ''' write the resolved theme colour into the form, and that frozen value would then read as a
    ''' deliberate choice forever.
    ''' </summary>
    Public Function ShouldSerializeLineColor() As Boolean
        Return _lineColor <> Color.Empty
    End Function

    Public Sub ResetLineColor()
        LineColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Series")>
    <Description("False => the series keeps its data but is neither drawn, nor hit-tested, nor listed in the legend.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT Chart Series")>
    <Description("True => drawn with EmphasisLineWidth and painted last, on top of the others. Meant for a total, or for the row the operator has selected.")>
    <DefaultValue(False)>
    Public Property Emphasis As Boolean

    <Category("K-BOT Chart Series")>
    <Description("True => the area between the line and the baseline is tinted with the line colour (see AreaFillOpacity on the chart).")>
    <DefaultValue(False)>
    Public Property FillArea As Boolean

    <Category("K-BOT Chart Series")>
    <Description("The points, in the order they are walked. They are NOT sorted for you: a line that walks backwards is an honest sign that the caller's query is out of order.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Points As KBotChartPointCollection
        Get
            Return _points
        End Get
    End Property

    ''' <summary>Whatever the host wants to find again. Runtime handle, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>Convenience for code: append a point and return it.</summary>
    Public Function AddPoint(moment As Date, value As Double) As KBotChartPoint
        Dim p As New KBotChartPoint(moment, value)
        _points.Add(p)
        Return p
    End Function

    ''' <summary>Ask the owning chart for a fresh layout. Called by the point collection.</summary>
    Friend Sub InvalidateOwnerLayout()
        OwnerChart?.InvalidateChartLayout()
    End Sub

    Public Overrides Function ToString() As String
        Return $"{If(Key, String.Empty)} ({_points.Count})"
    End Function
End Class

''' <summary>
''' The ordered series of a <see cref="KBotChartView"/>. Every mutation invalidates the chart's
''' layout, so an edit made in the designer collection dialog repaints itself instead of waiting
''' for a resize.
''' </summary>
''' <remarks>
''' <b>Key validation does NOT live here</b>, deliberately: the collection dialog inserts an empty
''' series the moment Add is pressed, long before anything has been typed into it. The contract
''' (non-empty, unique key) is enforced in <c>KBotChartView.EndInit</c> and in the runtime methods.
''' </remarks>
Public NotInheritable Class KBotChartSeriesCollection
    Inherits Collection(Of KBotChartSeries)

    ''' <summary>The chart that owns this collection (Nothing for a free-standing instance).</summary>
    Friend Property Owner As KBotChartView

    ' The four mutators carry their own Try/Catch because they are ENTRY POINTS — the designer
    ' calls them from InitializeComponent and code calls them directly, so there is no already
    ' wrapped boundary above them to log at. Boundary classification: log and RE-THROW, never
    ' swallow — a chart that silently loses a series is exactly the failure the house rule bans.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotChartSeries)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerChart = Owner
            MyBase.InsertItem(index, item)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartSeriesCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotChartSeries)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerChart = Owner
            MyBase.SetItem(index, item)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartSeriesCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            If index >= 0 AndAlso index < Count Then Me(index).OwnerChart = Nothing
            MyBase.RemoveItem(index)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartSeriesCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each s As KBotChartSeries In Me
                s.OwnerChart = Nothing
            Next
            MyBase.ClearItems()
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartSeriesCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
