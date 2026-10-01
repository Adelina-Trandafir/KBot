Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the tree as seen by the help.
''' <list type="bullet">
''' <item><see cref="IKBotHelpParts"/> -- where each painted button is, for the guided tours.
''' Parts: <c>header</c>, <c>header.search</c>, <c>header.right</c>, <c>columns</c>,
''' <c>node.icon</c> (the button at the right end of a row; the tour shows it on one row even
''' when it normally appears only under the mouse), <c>footer</c>, <c>footer.left</c>,
''' <c>footer.right</c>, <c>footer.collapse</c>.</item>
''' <item><see cref="IKBotCaptureRedaction"/> -- the visible rows whose caption or cells hold
''' sensitive text, for the help capture's blur.</item>
''' </list>
''' </summary>
Partial Public Class AdvancedTreeControl
    Implements IKBotHelpParts, IKBotHelpReveal, IKBotCaptureRedaction

    ' The row whose right icon the tour shows although the mouse is not on it (Nothing = none).
    Private _helpDemoItem As TreeItem

    ' Slice 0000-30: what the tour shows when the view hides the button (nothing to do yet): the
    ' view hands over the picture of its row button / footer-left button, the tree draws it for the
    ' step. Nothing = this tree has nothing to show there.
    Private _helpDemoRightIcon As Image
    Private _helpDemoFooterLeftIcon As Image
    Private _helpRevealedRow As TreeItem
    Private _helpRevealedFooterLeft As Boolean

    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property HelpDemoRightIcon As Image
        Get
            Return _helpDemoRightIcon
        End Get
        Set(value As Image)
            _helpDemoRightIcon = value
        End Set
    End Property

    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property HelpDemoFooterLeftIcon As Image
        Get
            Return _helpDemoFooterLeftIcon
        End Get
        Set(value As Image)
            _helpDemoFooterLeftIcon = value
        End Set
    End Property

    Public Function HelpPartBounds(part As String) As Rectangle Implements IKBotHelpParts.HelpPartBounds
        Select Case part
            Case "header"
                Return If(_headerVisible, New Rectangle(0, 0, Width, _headerHeight), Rectangle.Empty)
            Case "header.search"
                Return If(_headerVisible AndAlso _headerSearchIcon IsNot Nothing, _headerSearchIconRect, Rectangle.Empty)
            Case "header.right"
                Return HeaderRightIconRect
            Case "columns"
                Dim h As Integer = If(_treeListViewEnabled AndAlso _treeListView AndAlso GetVisibleColumnCount() > 0,
                                      ColumnHeaderHeightPx, 0)
                If h <= 0 Then Return Rectangle.Empty
                Return New Rectangle(0, TotalHeaderOffset - h, Width, h)
            Case "node.icon"
                If _helpDemoItem Is Nothing Then Return Rectangle.Empty
                Dim r As Rectangle = NodeRightIconRect(_helpDemoItem)
                Return If(RowOnScreen(r.Top, r.Height), r, Rectangle.Empty)
            Case "footer"
                If Not _footerVisible Then Return Rectangle.Empty
                Return New Rectangle(0, Math.Max(0, Height - _footerHeight), Width, _footerHeight)
            Case "footer.left"
                Return FooterLeftIconRect
            Case "footer.right"
                Return FooterRightIconRect
            Case "footer.collapse"
                Return FooterCollapseButtonRect
            Case Else
                Throw New ArgumentException("Unknown tree help part '" & part & "'.", NameOf(part))
        End Select
    End Function

    ''' <summary>
    ''' <c>node.icon</c>: the selected row when it has a right icon and is on screen, else the first
    ''' row on screen that has one. Every other known part is always drawn: nothing to do.
    ''' </summary>
    Public Sub SetHelpPartDemo(part As String, show As Boolean) Implements IKBotHelpParts.SetHelpPartDemo
        HelpPartBounds(part)   ' validates the name (unknown -> ArgumentException)
        If part <> "node.icon" Then Return
        If Not show Then
            _helpDemoItem = Nothing
        Else
            Dim candidates As List(Of TreeItem) = GetVisibleItems().
                Where(Function(it) it.RightIcon IsNot Nothing AndAlso RowOnScreen(GetItemY(it), _itemHeight)).ToList()
            _helpDemoItem = If(candidates.Contains(pSelectedItem), pSelectedItem, candidates.FirstOrDefault())
        End If
        Invalidate()
    End Sub

    ''' <summary>
    ''' Slice 0000-30: <c>node.icon</c> when no visible row has a button (the view shows it only
    ''' when there is something to do): the view's picture goes on the selected row, else the first
    ''' row without children, else the first row. <c>footer.left</c> when the view hides it: the
    ''' view's picture takes its place. Nothing when the view gave no picture.
    ''' </summary>
    Public Function HelpReveal(target As Control, part As String, ByRef area As Rectangle, ByRef note As String) As Boolean Implements IKBotHelpReveal.HelpReveal
        If Not ReferenceEquals(target, Me) Then Return False
        Select Case part
            Case "node.icon"
                If _helpDemoRightIcon Is Nothing OrElse _helpRevealedRow IsNot Nothing Then Return False
                Dim rows As List(Of TreeItem) = GetVisibleItems().Where(Function(it) RowOnScreen(GetItemY(it), _itemHeight)).ToList()
                If rows.Any(Function(it) it.RightIcon IsNot Nothing) Then Return False
                Dim pick As TreeItem = Nothing
                If pSelectedItem IsNot Nothing AndAlso rows.Contains(pSelectedItem) AndAlso pSelectedItem.Children.Count = 0 Then
                    pick = pSelectedItem
                Else
                    pick = rows.FirstOrDefault(Function(it) it.Children.Count = 0)
                    If pick Is Nothing Then pick = rows.FirstOrDefault()
                End If
                If pick Is Nothing Then Return False
                pick.RightIcon = _helpDemoRightIcon
                _helpRevealedRow = pick
                Invalidate()
                Return True
            Case "footer.left"
                If _helpDemoFooterLeftIcon Is Nothing OrElse FooterLeftIcon IsNot Nothing OrElse _helpRevealedFooterLeft Then Return False
                FooterLeftIcon = _helpDemoFooterLeftIcon
                _helpRevealedFooterLeft = True
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Sub HelpRevealEnd() Implements IKBotHelpReveal.HelpRevealEnd
        If _helpRevealedRow IsNot Nothing Then
            _helpRevealedRow.RightIcon = Nothing
            _helpRevealedRow = Nothing
            Invalidate()
        End If
        If _helpRevealedFooterLeft Then
            _helpRevealedFooterLeft = False
            FooterLeftIcon = Nothing
        End If
    End Sub

    ' A row band [top, top + height) lies inside the rows area (below the bands at the top,
    ' above the footer).
    Private Function RowOnScreen(top As Integer, height As Integer) As Boolean
        Dim bottom As Integer = Me.Height - If(_footerVisible, _footerHeight, 0)
        Return top >= TotalHeaderOffset AndAlso top + height <= bottom
    End Function

    ' What a cell is, for the capture's rules: its column's name and header («Cod fiscal», «CUI»).
    Private Shared Function CellContext(columnName As String, rowCols As List(Of ColumnDef)) As String
        Dim header As String = String.Empty
        If rowCols IsNot Nothing Then
            For Each cd As ColumnDef In rowCols
                If String.Equals(cd.Name, columnName, StringComparison.OrdinalIgnoreCase) Then header = If(cd.Header, String.Empty)
            Next
        End If
        Return If(columnName, String.Empty) & " " & header
    End Function

    ''' <summary>
    ''' TEXT only (operator, 30.09.2026): on a visible row, the caption's text when it is
    ''' sensitive, and the cells band when one of its cells is. Icons and lines stay sharp.
    ''' </summary>
    Public Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Rectangle) Implements IKBotCaptureRedaction.SensitiveRegions
        ArgumentNullException.ThrowIfNull(isSensitive)
        Dim result As New List(Of Rectangle)()
        Dim rightEdge As Integer = Width - ScrollBarWidth - PaddingTreeEndPx
        Using boldFont As New Font(Font, FontStyle.Bold)
        If Not _collapsed Then
            For Each it As TreeItem In GetVisibleItems()
                Dim y As Integer = GetItemY(it)
                If Not RowOnScreen(y, _itemHeight) Then Continue For
                Dim band As Integer = Math.Min(_itemHeight, Font.Height + 2)
                Dim top As Integer = y + (_itemHeight - band) \ 2
                Dim rowCols As List(Of ColumnDef) = Nothing
                If _treeListViewEnabled AndAlso _treeListView AndAlso RowHasColumns(it) Then rowCols = GetRowColumns(it)
                Dim colStart As Integer = If(rowCols IsNot Nothing AndAlso rowCols.Count > 0, GetColumnStartX(rowCols), rightEdge)

                If isSensitive(String.Empty, If(it.Caption, String.Empty)) Then
                    Dim x As Integer = Math.Max(0, GetContentStartX(it))
                    ' Bold captions draw wider than the regular-font measure kept in TextWidth.
                    Dim w As Integer = TextRenderer.MeasureText(it.Caption, If(it.Bold, boldFont, Font)).Width
                    w = Math.Min(w, colStart - x)
                    If w > 0 Then result.Add(New Rectangle(x, top, w, band))
                End If
                If colStart < rightEdge AndAlso
                   it.Cells.Any(Function(kv) kv.Value IsNot Nothing AndAlso isSensitive(CellContext(kv.Key, rowCols), If(kv.Value.Value, String.Empty))) Then
                    result.Add(New Rectangle(colStart, top, rightEdge - colStart, band))
                End If
            Next
        End If
        End Using
        ' The header caption's own strip: after the left icon, before the search / right icons.
        If _headerVisible AndAlso isSensitive(String.Empty, If(_headerCaption, String.Empty)) Then
            Dim left As Integer = PaddingHeaderLeftPx +
                                  If(_headerLeftIcon IsNot Nothing, _headerIconSize.Width + PaddingIconGapPx, 0)
            Dim right As Integer = Width - PaddingTreeEndPx
            If Not _headerSearchIconRect.IsEmpty Then right = Math.Min(right, _headerSearchIconRect.Left)
            If Not _headerRightIconRect.IsEmpty Then right = Math.Min(right, _headerRightIconRect.Left)
            Dim band As Integer = Math.Min(_headerHeight, HeaderFont.Height + 2)
            If right > left Then result.Add(New Rectangle(left, (_headerHeight - band) \ 2, right - left, band))
        End If
        Return result
    End Function

End Class
