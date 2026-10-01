Option Strict On
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the grid as seen by the help.
''' <list type="bullet">
''' <item><see cref="IKBotHelpParts"/> -- where each painted piece is, for the guided tours.
''' Parts: <c>header</c> (the column titles), <c>header.filter</c> (the first column menu icon
''' on screen), <c>rows</c>, <c>footer</c> (the totals band), <c>footer.left</c>,
''' <c>footer.right</c>, <c>footer.collapse</c>. All are always drawn: no demo.</item>
''' <item><see cref="IKBotCaptureRedaction"/> -- the visible cells whose text is sensitive, for
''' the help capture's blur.</item>
''' </list>
''' </summary>
Partial Class KBotDataView
    Implements IKBotHelpParts, IKBotCaptureRedaction

    Public Function HelpPartBounds(part As String) As Rectangle Implements IKBotHelpParts.HelpPartBounds
        RecalcColumnLayout()
        Select Case part
            Case "header"
                Dim h As Integer = HeaderBandHeight()
                Return If(h > 0, New Rectangle(0, 0, ViewportWidth(), h), Rectangle.Empty)
            Case "header.filter"
                Return FirstFilterIconOnScreen()
            Case "rows"
                Dim h As Integer = ViewportHeight()
                Return If(h > 0, New Rectangle(0, HeaderBandHeight(), ViewportWidth(), h), Rectangle.Empty)
            Case "footer"
                If Not _showFooter OrElse FooterBandHeight() <= 0 Then Return Rectangle.Empty
                Return CurrentFooterBandRect()
            Case "footer.left"
                Return FooterLeftIconRect
            Case "footer.right"
                Return FooterRightIconRect
            Case "footer.collapse"
                Return CollapseButtonRect
            Case Else
                Throw New ArgumentException("Unknown grid help part '" & part & "'.", NameOf(part))
        End Select
    End Function

    Public Sub SetHelpPartDemo(part As String, show As Boolean) Implements IKBotHelpParts.SetHelpPartDemo
        HelpPartBounds(part)   ' validates the name; every grid part is always drawn
    End Sub

    ' The column menu icon of the first column (frozen band first, then the scrolled one) whose
    ' icon is shown and lies inside the viewport. Empty = none.
    Private Function FirstFilterIconOnScreen() As Rectangle
        Dim viewport As Integer = ViewportWidth()
        For Each cl In _frozenLayout.Concat(_scrollLayout)
            Dim r As Rectangle = DebugFilterIconRect(cl.Column.Key)
            If Not r.IsEmpty AndAlso r.Left >= 0 AndAlso r.Right <= viewport Then Return r
        Next
        Return Rectangle.Empty
    End Function

    ''' <summary>The text box of every visible cell whose shown text (or raw value) is sensitive, cut to the viewport.</summary>
    Public Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Rectangle) Implements IKBotCaptureRedaction.SensitiveRegions
        ArgumentNullException.ThrowIfNull(isSensitive)
        Dim result As New List(Of Rectangle)()
        Dim body As New Rectangle(0, HeaderBandHeight(), ViewportWidth(), ViewportHeight())
        If body.Width <= 0 OrElse body.Height <= 0 Then Return result
        RecalcColumnLayout()
        Dim columns As List(Of KBotDataColumn) = _frozenLayout.Concat(_scrollLayout).Select(Function(cl) cl.Column).ToList()
        For vp As Integer = 0 To ViewCount() - 1
            Dim y As Integer = RowTop(vp)
            If y = Integer.MinValue OrElse y + _rowHeight <= body.Top OrElse y >= body.Bottom Then Continue For
            Dim mi As Integer = ModelIndexAt(vp)
            If mi < 0 Then Continue For
            Dim row As KBotDataRow = _rows(mi)
            For Each col As KBotDataColumn In columns
                Dim value As Object = row(col.Key)
                If value Is Nothing Then Continue For
                Dim context As String = col.Key & " " & If(col.HeaderText, String.Empty)
                If Not isSensitive(context, FormatValue(value, col)) AndAlso Not isSensitive(context, value.ToString()) Then Continue For
                ' Text only (operator, 30.09.2026): the cell's content box, not its padding or grid lines.
                Dim cellBox As Rectangle = CellRect(col, mi)
                If cellBox.IsEmpty Then Continue For
                Dim r As Rectangle = CellContentRect(col, cellBox)
                r.Intersect(body)
                If Not r.IsEmpty Then result.Add(r)
            Next
        Next
        Return result
    End Function

End Class
