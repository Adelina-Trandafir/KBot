Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the caption bar as seen by the help.
''' <list type="bullet">
''' <item><see cref="IKBotHelpParts"/> -- parts <c>icon</c>, <c>title</c>, <c>unit</c> (the unit
''' selector), <c>options</c>, <c>theme</c>, <c>help</c>, <c>minimize</c>, <c>maximize</c>,
''' <c>close</c>. All are drawn without the mouse: no demo.</item>
''' <item><see cref="IKBotCaptureRedaction"/> -- the unit selector always (it shows a unit's name),
''' and the part of the title that is sensitive.</item>
''' </list>
''' </summary>
Partial Public NotInheritable Class KBotCaptionBar
    Implements IKBotHelpParts, IKBotCaptureRedaction

    Private Const TitleSeparator As String = " — "

    ''' <summary>The texts of the selector's choices (the operator's units), in order.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectorTexts As IReadOnlyList(Of String)
        Get
            Return _selectorItems.Select(Function(p) If(p.Value, String.Empty)).ToList()
        End Get
    End Property

    Public Function HelpPartBounds(part As String) As Rectangle Implements IKBotHelpParts.HelpPartBounds
        Select Case part
            Case "icon" : Return IconRect()
            Case "title" : Return TitleRect()
            Case "unit" : Return If(SelectorVisible, _selectorRect, Rectangle.Empty)
            Case "options" : Return If(_showOptionsButton, OptionButtonRect(), Rectangle.Empty)
            Case "theme" : Return If(_showThemeButton, ThemeButtonRect(), Rectangle.Empty)
            Case "help" : Return HelpButtonBounds
            Case "minimize" : Return If(_showMinimize, MinRect(), Rectangle.Empty)
            Case "maximize" : Return If(_showMaximize, MaxRect(), Rectangle.Empty)
            Case "close" : Return CloseRect()
            Case Else
                Throw New ArgumentException("Unknown caption bar help part '" & part & "'.", NameOf(part))
        End Select
    End Function

    Public Sub SetHelpPartDemo(part As String, show As Boolean) Implements IKBotHelpParts.SetHelpPartDemo
        HelpPartBounds(part)   ' validates the name; every part is always drawn
    End Sub

    ' The title as painted: its measured width, cut at the first button (and, with a selector,
    ' the «title — » that precedes it). Empty = no title.
    Private Function TitleRect() As Rectangle
        Dim title As String = If(Text, String.Empty) & If(SelectorVisible, TitleSeparator, String.Empty)
        If title.Length = 0 Then Return Rectangle.Empty
        Dim x As Integer = TitleLeft()
        Dim limit As Integer = TitleRightLimit() - ThemeShapes.ScaleDpi(Me, 12)
        Dim w As Integer = Math.Min(TextWidth(title), limit - x)
        Return If(w > 0, New Rectangle(x, 0, w, Height), Rectangle.Empty)
    End Function

    ' The line of text inside a full-height box: the font's height, vertically centred.
    Private Function TextBand(box As Rectangle) As Rectangle
        Dim h As Integer = Math.Min(box.Height, Font.Height + 2)
        Return New Rectangle(box.Left, box.Top + (box.Height - h) \ 2, box.Width, h)
    End Function

    Private Function TextWidth(s As String) As Integer
        Const flags As TextFormatFlags = TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
        Return TextRenderer.MeasureText(s, Font, New Size(Integer.MaxValue, Height), flags).Width
    End Function

    ''' <summary>
    ''' TEXT only, never the bar or its buttons (operator, 30.09.2026): the unit's name inside the
    ''' selector whenever it shows, and the title when sensitive -- only the part after «K-BOT — »
    ''' when that first part is not sensitive itself, so the picture still says «K-BOT».
    ''' </summary>
    Public Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Rectangle) Implements IKBotCaptureRedaction.SensitiveRegions
        ArgumentNullException.ThrowIfNull(isSensitive)
        Dim result As New List(Of Rectangle)()
        If SelectorVisible AndAlso Not _selectorTextRect.IsEmpty Then result.Add(TextBand(_selectorTextRect))
        Dim text As String = If(Me.Text, String.Empty)
        Dim title As Rectangle = TitleRect()
        If Not title.IsEmpty Then title = TextBand(title)
        If title.IsEmpty OrElse Not isSensitive(String.Empty, text) Then Return result
        Dim cut As Integer = text.IndexOf(TitleSeparator, StringComparison.Ordinal)
        If cut > 0 AndAlso Not isSensitive(String.Empty, text.Substring(0, cut)) Then
            Dim skip As Integer = TextWidth(text.Substring(0, cut + TitleSeparator.Length))
            title = Rectangle.FromLTRB(Math.Min(title.Right, title.Left + skip), title.Top, title.Right, title.Bottom)
            If title.Width <= 0 Then Return result
        End If
        result.Add(title)
        Return result
    End Function

End Class
