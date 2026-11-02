Option Strict On

''' <summary>
''' Which button a <see cref="KBotColumnType.Button"/> column shows (slice 0010-09).
''' </summary>
Public Enum KBotButtonKind

    ''' <summary>The button as the other Button properties of the column describe it (default).</summary>
    Custom = 0

    ''' <summary>
    ''' The standard delete button of K-BOT, the same in every grid: the glyph «✕», filling the whole cell (no margin, no padding, no
    ''' border), flat so it shows the colour of the row it sits on, a wash of the theme's error colour and the hand cursor while the mouse
    ''' is over it. The other Button properties of the column are ignored.
    ''' </summary>
    Delete = 1

End Enum
