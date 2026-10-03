Option Strict On

''' <summary>
''' Which sides of a rectangle get a border line: the cell borders of a column
''' (<see cref="KBotDataColumn.CellBorders"/>, per-cell through <c>CellFormatting</c>) and the
''' button border of a Button column (<see cref="KBotDataColumn.ButtonBorders"/>).
''' A flags value: combine sides with <c>Or</c>, or pick the ready-made <see cref="All"/> /
''' <see cref="None"/>. (Slice 0085-03; first written as <c>KBotBorderSides</c> in 0085-02.)
''' </summary>
<Flags>
Public Enum KBotBorderSides

    ''' <summary>No border at all.</summary>
    None = 0

    ''' <summary>The left side.</summary>
    Left = 1

    ''' <summary>The top side.</summary>
    Top = 2

    ''' <summary>The right side.</summary>
    Right = 4

    ''' <summary>The bottom side.</summary>
    Bottom = 8

    ''' <summary>All four sides.</summary>
    All = Left Or Top Or Right Or Bottom

End Enum
