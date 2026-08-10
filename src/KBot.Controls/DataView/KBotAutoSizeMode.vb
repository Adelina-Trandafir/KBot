Option Strict On

''' <summary>
''' How <see cref="KBotDataView"/> measures its columns before any fill mode is applied.
'''
''' English (slice 0028-04): the same vocabulary is used by BOTH knobs — the grid-wide
''' <see cref="KBotDataView.AutoSizeColumnsMode"/> and the per-column
''' <see cref="KBotDataColumn.AutoSizeMode"/>, which wins wherever it is set. Only the column
''' may say <see cref="Inherit"/>.
''' </summary>
Public Enum KBotAutoSizeMode

    ''' <summary>
    ''' English (slice 0028-04): COLUMN-ONLY sentinel — «no opinion of my own, follow the grid».
    ''' It is the default of <see cref="KBotDataColumn.AutoSizeMode"/>, which is what keeps the
    ''' per-column knob backwards compatible: a column says nothing until somebody sets it, and
    ''' the grid-wide <see cref="KBotDataView.AutoSizeColumnsMode"/> keeps deciding for everybody.
    ''' Assigning it to the GRID property is an error (there is nothing above the grid to inherit
    ''' from) and raises <c>ArgumentException</c> rather than silently meaning None.
    ''' </summary>
    Inherit = -1

    ''' <summary>Widths stay exactly as set by the caller or by a manual drag-resize.</summary>
    None = 0

    ''' <summary>
    ''' Each visible column is measured to its content (widest of header text and sampled
    ''' cell text) and clamped to [MinWidth, MaxWidth]. This is the default.
    ''' </summary>
    ToContent = 1

End Enum
