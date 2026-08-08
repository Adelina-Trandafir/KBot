Imports System.Linq

Partial Public Class AdvancedTreeControl

    ''' <summary>
    ''' Configures the control as a flat list with columns (no expanders, no indent).
    ''' Public entry point for callers outside KBot.Controls; wraps the Friend
    ''' SetTreeListView. Uses STATIC columns (DynamicColumns = False, ColumnsLevel = 0):
    ''' every root row gets the same column band, and clicking a node does NOT rebuild
    ''' the columns from ColHeaderText. The caption occupies the remaining left space;
    ''' columns are drawn right-aligned. The column filter popup works out of the box
    ''' (header click path is gated on TreeListView + columns, verified).
    ''' </summary>
    Public Sub ConfigureListMode(columns As IEnumerable(Of ColumnDef))
        If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))

        Dim cols As New List(Of ColumnDef)(columns)

        ' Static-column mode: all rows are level 0 and receive the same band.
        DynamicColumns = False
        ColumnsLevel = 0

        ' Flat list: no expanders, no indentation.
        RootExpander = False
        Indent = 0

        ' Master switch on + install the column definitions.
        TreeListView = True
        SetTreeListView(True, cols)

        Me.Invalidate()
    End Sub

End Class
