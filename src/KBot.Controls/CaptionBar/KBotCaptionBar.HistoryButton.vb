Option Strict On
Imports System.ComponentModel
Imports System.Drawing

' Slice 0097-09: optional history button, leftmost in the caption-button group.
Partial Public NotInheritable Class KBotCaptionBar

    Private _showHistoryButton As Boolean
    Private _historyButtonHover As Boolean
    Private _historyButtonActive As Boolean
    Private _historyMenuOpening As Boolean

    <Category("K-BOT"), DefaultValue(False)>
    <Description("Arată istoricul navigării primul din stânga în grupul butoanelor barei de titlu.")>
    Public Property ShowHistoryButton As Boolean
        Get
            Return _showHistoryButton
        End Get
        Set(value As Boolean)
            If value = _showHistoryButton Then Return
            _showHistoryButton = value
            _historyButtonHover = False
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HistoryButtonBounds As Rectangle
        Get
            Return If(_showHistoryButton, HistoryButtonRect(), Rectangle.Empty)
        End Get
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HistoryButtonHot As Boolean
        Get
            Return _historyButtonHover
        End Get
    End Property

    Public Event HistoryButtonClick As EventHandler
    Public Event HistoryButtonHoverChanged As EventHandler

    Private Function HistoryButtonRect() As Rectangle
        Return SlotRect(ThemeButtonSlot() + If(_showThemeButton, 1, 0) + If(_showOptionsButton, 1, 0))
    End Function

    ' A clock with a return arrow, drawn in the same colour and DPI as the control box.
    Private Sub DrawHistoryButton(g As Graphics)
        Dim k_bounds As Rectangle = HistoryButtonRect()
        If _historyButtonHover OrElse _historyButtonActive Then
            Using k_brush As New SolidBrush(_btnHoverColor)
                g.FillRectangle(k_brush, k_bounds)
            End Using
        End If
        Dim k_radius As Integer = Math.Max(3, Math.Min(ThemeShapes.ScaleDpi(Me, 8), Height \ 3))
        Dim k_x As Integer = k_bounds.Left + k_bounds.Width \ 2
        Dim k_y As Integer = k_bounds.Height \ 2
        Using k_pen As New Pen(_glyphColor, ThemeShapes.ScaleDpi(Me, 1))
            g.DrawArc(k_pen, k_x - k_radius, k_y - k_radius, 2 * k_radius, 2 * k_radius, 210, 310)
            Dim k_tip As New Point(k_x - k_radius, k_y - k_radius \ 2)
            g.DrawLines(k_pen, {New Point(k_tip.X, k_tip.Y - k_radius \ 2), k_tip,
                               New Point(k_tip.X + k_radius \ 2, k_tip.Y)})
            g.DrawLines(k_pen, {New Point(k_x, k_y - k_radius \ 2), New Point(k_x, k_y),
                               New Point(k_x + k_radius \ 2, k_y + k_radius \ 3)})
        End Using
    End Sub
End Class
