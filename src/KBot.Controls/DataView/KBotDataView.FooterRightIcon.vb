Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The RIGHT button of the footer band of <see cref="KBotDataView"/> (slice 0087): one clickable
''' icon at the right end of the band, the pair of the tree footer's <c>FooterRightIcon</c> (same
''' names, so a host learns one vocabulary). Typical use: «+» that adds a row the operator then
''' fills in the grid.
'''
''' <para><b>Its corner belongs to it.</b> Like the collapse button, the icon takes a square at
''' the end of the band and <see cref="FooterContentRect"/> gives the rest to the totals and the
''' caption, so a long sum is cut before the icon instead of running under it. When the collapse
''' button also sits on the right, the icon stands just left of it.</para>
'''
''' <para>The icon is drawn and clickable only when <see cref="FooterVisible"/> is on and an image
''' is set; nothing is painted or hit-tested otherwise.</para>
''' </summary>
Partial Class KBotDataView

    Private _footerRightIcon As Image = Nothing
    Private _footerRightIconTooltip As String = String.Empty
    Private _footerRightIconHover As Boolean = False

    ''' <summary>The icon at the right end of the footer band was clicked.</summary>
    Public Event FooterRightIconClicked As EventHandler

    ''' <summary>The icon at the right end of the footer band. Nothing (default) = no button.</summary>
    <Category("K-BOT: Footer")>
    <Description("Icon at the right end of the footer band. Clicking it raises FooterRightIconClicked. Needs FooterVisible.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterRightIcon As Image
        Get
            Return _footerRightIcon
        End Get
        Set(value As Image)
            If value Is _footerRightIcon Then Return
            _footerRightIcon = value
            _footerRightIconHover = False
            RecomputeDerived()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterRightIcon() As Boolean
        Return _footerRightIcon IsNot Nothing
    End Function

    Private Sub ResetFooterRightIcon()
        FooterRightIcon = Nothing
    End Sub

    ''' <summary>The hover label of the footer right icon (KBotToolTip markup). Empty = none.</summary>
    <Category("K-BOT: Footer")>
    <Description("Hover label of the footer right icon. Empty = no label.")>
    <DefaultValue("")>
    Public Property FooterRightIconTooltip As String
        Get
            Return _footerRightIconTooltip
        End Get
        Set(value As String)
            _footerRightIconTooltip = If(value, String.Empty)
        End Set
    End Property

    ''' <summary>The current rectangle of the footer right icon (empty = not shown). For hosts / tests.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FooterRightIconRect As Rectangle
        Get
            If Not _showFooter OrElse FooterBandHeight() <= 0 Then Return Rectangle.Empty
            Return ComputeFooterRightIconRect(CurrentFooterBandRect())
        End Get
    End Property

    ''' <summary>
    ''' The icon's square in a band: at the right end, or just left of a right-side collapse
    ''' button. Pure, shared by paint, hit-test and <see cref="FooterContentRect"/>.
    ''' </summary>
    Private Function ComputeFooterRightIconRect(bandRect As Rectangle) As Rectangle
        If _footerRightIcon Is Nothing OrElse bandRect.Height <= 0 Then Return Rectangle.Empty
        Dim s As Size = FooterIconSizePx
        Dim side As Integer = Math.Min(Math.Max(s.Width, s.Height), Math.Max(1, bandRect.Height - 4))
        Dim margin As Integer = ScaleDpi(6)
        Dim right As Integer = bandRect.Right - margin
        If _collapseButton AndAlso _collapseButtonPosition = KBotFooterButtonPosition.Right Then
            Dim collapse As Rectangle = ComputeCollapseButtonRect(bandRect)
            If Not collapse.IsEmpty Then right = collapse.Left - margin
        End If
        Dim left As Integer = right - side
        If left < bandRect.Left Then Return Rectangle.Empty
        Return New Rectangle(left, bandRect.Top + (bandRect.Height - side) \ 2, side, side)
    End Function

    ''' <summary>Paints the icon (covered by the Try of OnPaint).</summary>
    Private Sub DrawFooterRightIcon(g As Graphics, bandRect As Rectangle)
        Dim r As Rectangle = ComputeFooterRightIconRect(bandRect)
        If r.IsEmpty Then Return
        If _footerRightIconHover Then
            Using b As New SolidBrush(FooterIconHoverResolved())
                Using path As GraphicsPath = RoundedRect(Rectangle.Inflate(r, 3, 3), ScaleDpi(3))
                    g.FillPath(b, path)
                End Using
            End Using
        End If
        g.DrawImage(_footerRightIcon, r)
    End Sub

    ''' <summary>A press on the icon. True = consumed.</summary>
    Friend Function HandleFooterRightIconMouseDown(location As Point) As Boolean
        Dim r As Rectangle = FooterRightIconRect
        If r.IsEmpty OrElse Not r.Contains(location) Then Return False
        If Not KBotDesignTime.IsDesignTime(Me) Then
            RaiseEvent FooterRightIconClicked(Me, EventArgs.Empty)
        End If
        Return True
    End Function

    ''' <summary>Hover tracking over the icon, with its label. True = the cursor is on it.</summary>
    Friend Function UpdateFooterRightIconHover(location As Point) As Boolean
        Dim r As Rectangle = FooterRightIconRect
        Dim hover As Boolean = Not r.IsEmpty AndAlso r.Contains(location)
        If hover <> _footerRightIconHover Then
            _footerRightIconHover = hover
            If hover Then
                ShowButtonTip("frt", _footerRightIconTooltip)
            ElseIf String.Equals(_tipButonCurent, "frt", StringComparison.Ordinal) Then
                HideButtonTip()
            End If
            Invalidate()
        End If
        Return hover
    End Function

    ''' <summary>Turns the icon's hover off.</summary>
    Friend Sub ClearFooterRightIconHover()
        If Not _footerRightIconHover Then Return
        _footerRightIconHover = False
        Invalidate()
    End Sub

    ''' <summary>The cursor is over the footer right icon (read by .Input for the hand cursor).</summary>
    Friend ReadOnly Property FooterRightIconHovered As Boolean
        Get
            Return _footerRightIconHover
        End Get
    End Property

End Class
