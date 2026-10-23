Partial Public Class AdvancedTreeControl
    Public Event NodeMouseDown(pNode As TreeItem, e As MouseEventArgs)
    Public Event NodeMouseUp(pNode As TreeItem, e As MouseEventArgs)
    Public Event NodeDoubleClicked(pNode As TreeItem, e As MouseEventArgs)
    Public Event NodeChecked(pNode As TreeItem)
    Public Event NodeRadioSelected(nodeOn As TreeItem, nodeOff As TreeItem)
    Public Event RequestLazyLoad(sender As Object, item As TreeItem)
    Public Event RightIconClicked(pNode As TreeItem, e As MouseEventArgs)
    Public Event HeaderRightIconClicked(e As MouseEventArgs)
    Public Event FooterRightIconClicked(e As MouseEventArgs)
    Public Event FooterLeftIconClicked(e As MouseEventArgs)

    ''' <summary>Slice 00EF-33: a click in the footer band, outside the collapse button and the two icons (on the caption).</summary>
    Public Event FooterCaptionClicked(e As MouseEventArgs)
    Public Event SearchFinished(matchingItems As List(Of TreeItem), searchText As String)

    ''' <summary>
    ''' Slice 0123-02: the pause after typing is over and the search is about to run on the rows loaded so far. A host whose branches load on
    ''' demand reads the rest here and calls <see cref="RerunSearch"/> when they are in.
    ''' </summary>
    Public Event SearchStarting(searchText As String)
End Class
