Option Strict On

''' <summary>
''' The three answers a <see cref="KBotDataColumn"/> can give to «are you on screen?». The first
''' two are the old Boolean; the third is the one that makes a grid grow INTO its extra room
''' instead of leaving it empty or stretching a single fill column forever.
'''
''' <para><b>Why a third value and not a second flag.</b> <see cref="KBotDataColumn.AutoHide"/>
''' already answers the opposite question (a column that MAY go away when room is short). The
''' mirror of it — a column that MAY come back when room is plenty — is not a flag next to
''' <c>Visible</c>: a hidden column with «may appear» is exactly a value of visibility, the same
''' way «may go away» sits on a shown one. Keeping it on <c>Visible</c> means the designer shows
''' one property with three choices, not two properties that contradict each other.</para>
''' </summary>
Public Enum KBotColumnVisibility

    ''' <summary>Painted and taking space (the old <c>True</c>). Default.</summary>
    Visible = 0

    ''' <summary>Never painted, takes no space (the old <c>False</c>).</summary>
    Hidden = 1

    ''' <summary>
    ''' Hidden by default, but the layout pass SHOWS it when the visible columns leave enough room
    ''' for at least its <see cref="KBotDataColumn.MinWidth"/> — see
    ''' <see cref="KBotDataView.ShowColumnsWhenRoom"/>. Revealed columns come first, in column
    ''' order; whatever is left afterwards goes to the fill column
    ''' (<see cref="KBotDataView.ColumnFillMode"/>). Narrowing the grid hides them again.
    ''' </summary>
    WhenRoom = 2

End Enum
