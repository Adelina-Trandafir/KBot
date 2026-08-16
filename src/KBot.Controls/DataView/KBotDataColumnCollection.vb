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

    ' English (slice 0025-03): the four mutators carry their own Try/Catch because they are ENTRY
    ' POINTS — the designer calls them from InitializeComponent, AddColumn calls them, and callers
    ' call them directly, so there is no already-wrapped boundary above them to log at (the house
    ' rule's transitive coverage does not apply). They also reach real work: OnColumnsChanged runs
    ' the index rebuild AND a full layout pass (scrollbars, auto-size, fill), which is the part
    ' that can genuinely fail. Classification is «boundary»: log and RE-THROW, never swallow — a
    ' grid that silently loses a column would paint blank cells and look like a data bug.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotDataColumn)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            item.Owner = Owner
            ' Lățimea cerută e logică; cea pictată se derivă acum, când coloana știe pe ce grilă
            ' stă și, prin ea, la ce DPI (felia 0035-01).
            item.RefreshWidthScale()
            Owner?.OnColumnsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotDataColumnCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotDataColumn)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            Dim replaced As KBotDataColumn = Me(index)
            MyBase.SetItem(index, item)
            If replaced IsNot Nothing Then replaced.Owner = Nothing
            item.Owner = Owner
            ' Lățimea cerută e logică; cea pictată se derivă acum, când coloana știe pe ce grilă
            ' stă și, prin ea, la ce DPI (felia 0035-01).
            item.RefreshWidthScale()
            Owner?.OnColumnsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotDataColumnCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            Dim removed As KBotDataColumn = Me(index)
            MyBase.RemoveItem(index)
            If removed IsNot Nothing Then removed.Owner = Nothing
            Owner?.OnColumnsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotDataColumnCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each col As KBotDataColumn In Me
                col.Owner = Nothing
            Next
            MyBase.ClearItems()
            Owner?.OnColumnsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotDataColumnCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' Writing a log file from inside devenv.exe is noise at best; see KBotDesignTime.
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub

End Class
