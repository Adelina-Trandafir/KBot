Option Strict On

''' <summary>
''' What <see cref="KBotDataView"/> does with the leftover (or missing) horizontal space
''' after column auto-sizing.
''' </summary>
Public Enum KBotFillMode

    ''' <summary>Leave the leftover empty; on overflow show the horizontal scrollbar. Default.</summary>
    None = 0

    ''' <summary>The first visible column absorbs the whole leftover.</summary>
    FirstColumn = 1

    ''' <summary>The last visible column absorbs the whole leftover.</summary>
    LastColumn = 2

    ''' <summary>
    ''' The leftover is split among all visible columns in proportion to their current width
    ''' (a wider column receives a bigger share).
    ''' </summary>
    Proportional = 3

    ''' <summary>
    ''' O coloană ALEASĂ de operator absoarbe tot spațiul rămas — cea numită de
    ''' <see cref="KBotDataView.FillColumnKey"/>. Există fiindcă întinderea nu cade întotdeauna
    ''' pe un capăt: într-o grilă cu sume la dreapta și un cod la stânga, coloana care trebuie să
    ''' crească e descrierea din MIJLOC, iar <c>FirstColumn</c>/<c>LastColumn</c> n-o pot numi.
    ''' </summary>
    SpecificColumn = 4

End Enum
