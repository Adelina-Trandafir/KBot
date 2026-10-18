Option Strict On

''' <summary>
''' Slice KE12-03: how a <see cref="KBotDataView"/> offers a tick box on its rows (<see cref="KBotDataView.CheckBoxes"/>).
''' </summary>
Public Enum KBotCheckBoxMode

    ''' <summary>No tick boxes (default).</summary>
    None = 0

    ''' <summary>
    ''' A tick box on every data row AND on every group header (tri-state: all / some / none of the rows of the group are ticked; pressing it
    ''' ticks every row of the group, or clears them when all are ticked).
    ''' </summary>
    All = 1

    ''' <summary>A tick box on every data row only; group headers have none.</summary>
    RowsOnly = 2

End Enum
