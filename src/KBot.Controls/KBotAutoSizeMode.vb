Option Strict On

''' <summary>
''' How <see cref="KBotDataView"/> measures its columns before any fill mode is applied.
''' </summary>
Public Enum KBotAutoSizeMode

    ''' <summary>Widths stay exactly as set by the caller or by a manual drag-resize.</summary>
    None = 0

    ''' <summary>
    ''' Each visible column is measured to its content (widest of header text and sampled
    ''' cell text) and clamped to [MinWidth, MaxWidth]. This is the default.
    ''' </summary>
    ToContent = 1

End Enum
