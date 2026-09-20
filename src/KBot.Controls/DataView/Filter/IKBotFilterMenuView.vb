Option Strict On

''' <summary>
''' The contract of one tab of the column menu (<see cref="KBotFilterPopup"/>): a UserControl the
''' popup creates lazily at first activation and hosts one at a time, exactly as <c>KbotForm</c>
''' and <c>SetariForm</c> host their views. Three implementations, one per tab:
''' <see cref="KBotFilterPopupSortView"/>, <see cref="KBotFilterPopupFilterView"/> and
''' <see cref="KBotFilterPopupGroupView"/>.
''' </summary>
Friend Interface IKBotFilterMenuView

    ''' <summary>The nav key ("sortare", "filtrare", "grupare").</summary>
    ReadOnly Property ViewKey As String

    ''' <summary>
    ''' True when the popup must show its OK / «Anuleaza» bar under this tab. Only the filter
    ''' tab hands anything over at the end; sorting and grouping apply the moment the operator
    ''' clicks, so a command bar under them would look like something is still to confirm.
    ''' </summary>
    ReadOnly Property ShowsCommandBar As Boolean

    ''' <summary>The tab came on screen (focus goes where the operator would type first).</summary>
    Sub Activated()

    ''' <summary>
    ''' How tall the tab wants to be, in client pixels of the host: its fixed table rows at
    ''' the current theme plus whatever its elastic list asks for. The popup adds its own frame
    ''' (nav bar, separator, command bar) on top. Measured AFTER a layout pass, never from the
    ''' designer numbers.
    ''' </summary>
    Function RequiredHeight() As Integer

End Interface
