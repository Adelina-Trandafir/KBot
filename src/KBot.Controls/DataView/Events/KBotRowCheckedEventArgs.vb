Option Strict On

''' <summary>
''' Slice KE12-03 -- raised by <c>KBotDataView.RowChecked</c> when the OPERATOR ticks or clears the tick box of a row (directly, or through the tick box of a
''' group header, which raises it once per row that changed). Writing the state from code (<c>SetRowChecked</c>) raises nothing.
''' </summary>
Public NotInheritable Class KBotRowCheckedEventArgs
    Inherits EventArgs

    ''' <summary>The model index of the row (<c>Rows(i)</c>).</summary>
    Public ReadOnly Property RowIndex As Integer

    ''' <summary>The new state of the tick box.</summary>
    Public ReadOnly Property Checked As Boolean

    ''' <summary>
    ''' How many more rows of the same gesture still come after this one (0 = this is the last). A header tick box raises one event per row; a host that does heavy
    ''' work after each can wait for <c>Pending = 0</c>.
    ''' </summary>
    Public ReadOnly Property Pending As Integer

    Public Sub New(k_rowIndex As Integer, k_checked As Boolean, k_pending As Integer)
        RowIndex = k_rowIndex
        Checked = k_checked
        Pending = k_pending
    End Sub

End Class
