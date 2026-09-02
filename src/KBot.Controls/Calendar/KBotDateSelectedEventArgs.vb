Option Strict On

''' <summary>
''' A date the operator actually CHOSE — a click on a day cell, the "today" row, or Enter on
''' the focused cell. Distinct from <c>ValueChanged</c>, which also fires while the operator
''' is only walking the grid with the arrow keys: a drop-down closes on this event, not on
''' every step through the month.
''' </summary>
Public NotInheritable Class KBotDateSelectedEventArgs
    Inherits EventArgs

    Public Sub New(value As Date)
        Me.Value = value.Date
    End Sub

    ''' <summary>The chosen day (time component stripped).</summary>
    Public ReadOnly Property Value As Date

End Class
