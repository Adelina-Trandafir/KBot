Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' What a control must be able to do in order to own a <see cref="KBotChartGuideCollection"/>.
''' </summary>
''' <remarks>
''' <para>TWO controls draw guides: <see cref="KBotChartView"/> and <c>KBotLaneView</c>. They are
''' the two halves of one reading — the value over time above, where that value was placed below —
''' and a payment line has to fall on the same date in both. That is only true if both are drawing
''' the SAME guide objects, so the collection cannot be typed to either control.</para>
''' <para><c>Friend</c>, and implemented privately on both: a host never holds one of these. It
''' holds a chart or a lane view, and puts guides into the collection that control exposes.</para>
''' </remarks>
Friend Interface IKBotGuideHost

    ''' <summary>Something about the guides changed — lay out again and repaint.</summary>
    Sub InvalidateGuides()

    ''' <summary>The control itself, for the design-time check. Never Nothing.</summary>
    ReadOnly Property GuideHostControl As Control
End Interface

''' <summary>
''' A dated marker drawn straight down the plot of a <see cref="KBotChartView"/> — a moment that
''' matters, which is not itself a measurement.
''' </summary>
''' <remarks>
''' <para>The case this exists for: a payment. A payment is not a point on any series — it does
''' not say what a reception was worth — but it reads the value that stood on its date, so which
''' SIDE of it a snapshot falls on decides whether that payment was computed from the right
''' figure. Drawn as a line across the plot, that question answers itself by eye.</para>
'''
''' <para>A guide is <b>not a series</b>. It has no value, no marker, no legend entry, no key, and
''' no click: it is context the operator reads against the lines, never something they act on.
''' Hovering names it, and that is the whole of its behaviour.</para>
'''
''' <para><b>Never red.</b> Red is what this application spends on something being wrong, and a
''' payment is not wrong. Left at <see cref="Color.Empty"/> the guide takes the dimmed text colour
''' of the active scheme, which is the reading of "quiet background fact" that the plot already
''' uses for its axis labels.</para>
''' </remarks>
Public NotInheritable Class KBotChartGuide

    Private _lineColor As Color = Color.Empty

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a guide at a moment, with the label it shows on hover.</summary>
    Public Sub New(moment As Date, text As String)
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Moment = moment
        Me.Text = If(text, String.Empty)
    End Sub

    ''' <summary>The control that draws this guide (Nothing for a free-standing instance).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property OwnerHost As IKBotGuideHost

    <Category("K-BOT Chart Guide")>
    <Description("Where on the time axis the line stands. The same axis as the points, so a guide and a point at the same moment line up exactly.")>
    Public Property Moment As Date

    <Category("K-BOT Chart Guide")>
    <Description("Title line of the floating label shown while the pointer rests on the line. This is read by the operator. Empty = no label at all.")>
    <DefaultValue("")>
    Public Property Text As String = String.Empty

    <Category("K-BOT Chart Guide")>
    <Description("Body of the floating label (multiple lines; accepts the rich-text markup of KBotToolTip). Empty = the moment on its own.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property Tooltip As String = String.Empty

    ''' <summary>
    ''' The colour of the line. <c>Color.Empty</c> = the dimmed text colour of the active scheme.
    ''' </summary>
    <Category("K-BOT Chart Guide")>
    <Description("Colour of the line. Empty = the dimmed text colour of the theme. Never use red here — in this application red means something is wrong.")>
    Public Property LineColor As Color
        Get
            Return _lineColor
        End Get
        Set(value As Color)
            _lineColor = value
            OwnerHost?.InvalidateGuides()
        End Set
    End Property

    ''' <summary>
    ''' Keeps a colour nobody chose out of the host's .Designer.vb (see the twin on the series).
    ''' </summary>
    Public Function ShouldSerializeLineColor() As Boolean
        Return _lineColor <> Color.Empty
    End Function

    Public Sub ResetLineColor()
        LineColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Guide")>
    <Description("Dash pattern of the line. Dotted by default, so it reads as background rather than as one more series.")>
    <DefaultValue(Drawing2D.DashStyle.Dot)>
    Public Property DashStyle As Drawing2D.DashStyle = Drawing2D.DashStyle.Dot

    <Category("K-BOT Chart Guide")>
    <Description("False => the guide keeps its place in the collection but is neither drawn nor hit-tested.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    ''' <summary>Whatever the host wants to find again. Runtime handle, never serialized.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>
    ''' Where the last layout pass put this line, in client pixels. -1 while the guide falls
    ''' outside the plotted time range and is therefore not drawn.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property PlotX As Integer = -1

    Public Overrides Function ToString() As String
        Return $"{Moment:g} {If(Text, String.Empty)}".TrimEnd()
    End Function
End Class

''' <summary>
''' The guides of a chart or of a lane view. Every mutation lays the owner out again.
''' </summary>
''' <remarks>
''' <b>Nothing is validated here.</b> Unlike series and tabs, a guide has no key — there is
''' nothing for the host to look one up by, because a guide is never acted on. Two guides at the
''' same moment are legal and are both drawn: two payments made on one day are two payments.
''' </remarks>
Public NotInheritable Class KBotChartGuideCollection
    Inherits Collection(Of KBotChartGuide)

    ''' <summary>The control that draws these guides (Nothing for a free-standing instance).</summary>
    Friend Property Owner As IKBotGuideHost

    ' Entry points: log and RE-THROW (see the twin note in KBotChartSeriesCollection).
    Protected Overrides Sub InsertItem(index As Integer, item As KBotChartGuide)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerHost = Owner
            MyBase.InsertItem(index, item)
            Owner?.InvalidateGuides()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartGuideCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotChartGuide)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            item.OwnerHost = Owner
            MyBase.SetItem(index, item)
            Owner?.InvalidateGuides()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartGuideCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            If index >= 0 AndAlso index < Count Then Me(index).OwnerHost = Nothing
            MyBase.RemoveItem(index)
            Owner?.InvalidateGuides()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartGuideCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each gd As KBotChartGuide In Me
                gd.OwnerHost = Nothing
            Next
            MyBase.ClearItems()
            Owner?.InvalidateGuides()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartGuideCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If Owner IsNot Nothing AndAlso KBotDesignTime.IsDesignTime(Owner.GuideHostControl) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
