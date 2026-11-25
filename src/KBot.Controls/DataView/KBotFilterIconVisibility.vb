Option Strict On

''' <summary>
''' Slice DGV0-02: when the filter button (the funnel) of a column header is drawn
''' (<see cref="KBotDataView.FilterIconVisibility"/>). The room for the button is reserved in every mode,
''' so the title never moves when the button appears or disappears.
''' </summary>
Public Enum KBotFilterIconVisibility

    ''' <summary>The funnel is always drawn (default; the behaviour before DGV0-02).</summary>
    Always = 0

    ''' <summary>The funnel is drawn only on the column whose header is under the mouse (or whose filter menu is open).</summary>
    OnHover = 1

    ''' <summary>
    ''' Like <see cref="OnHover"/>, and the funnel also stays drawn on every column that has a filter set,
    ''' so the operator still sees at a glance what is filtered.
    ''' </summary>
    OnHoverOrFiltered = 2

End Enum
