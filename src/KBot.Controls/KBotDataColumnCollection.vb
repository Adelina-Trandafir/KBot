Option Strict On
Imports System.Collections.ObjectModel

''' <summary>
''' English (slice 0025): the ordered collection behind <see cref="KBotDataView.Columns"/>.
''' Adding, replacing, removing or clearing a column now rebuilds the grid's key index and
''' triggers a layout by itself — which is half of the limitation recorded in slice 0013 ("no
''' column→grid back-reference"). Editing a column's PROPERTIES after load still behaves as
''' before and still needs an explicit <c>AutoSizeColumns()</c>.
'''
''' Duplicate keys are SKIPPED SILENTLY during the index rebuild, deliberately: the designer's
''' collection dialog inserts an empty column the moment you press «Add», long before it has a
''' key. Validation belongs to <c>AddColumn</c> (runtime) and to <c>EndInit</c> (after the
''' designer block), not to every intermediate keystroke.
''' </summary>
Public NotInheritable Class KBotDataColumnCollection
    Inherits Collection(Of KBotDataColumn)

    ''' <summary>The grid that owns this collection (Nothing for a free-floating instance).</summary>
    Friend Property Owner As KBotDataView

    Protected Overrides Sub InsertItem(index As Integer, item As KBotDataColumn)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        MyBase.InsertItem(index, item)
        item.Owner = Owner
        Owner?.OnColumnsChanged()
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotDataColumn)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
        Dim replaced As KBotDataColumn = Me(index)
        MyBase.SetItem(index, item)
        If replaced IsNot Nothing Then replaced.Owner = Nothing
        item.Owner = Owner
        Owner?.OnColumnsChanged()
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Dim removed As KBotDataColumn = Me(index)
        MyBase.RemoveItem(index)
        If removed IsNot Nothing Then removed.Owner = Nothing
        Owner?.OnColumnsChanged()
    End Sub

    Protected Overrides Sub ClearItems()
        For Each col As KBotDataColumn In Me
            col.Owner = Nothing
        Next
        MyBase.ClearItems()
        Owner?.OnColumnsChanged()
    End Sub

End Class
