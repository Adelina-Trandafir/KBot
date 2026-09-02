Option Strict On

''' <summary>
''' Which page <see cref="KBotCalendar"/> is showing. The three are one zoom axis:
''' clicking the header title zooms OUT (days to months to years), picking a cell zooms
''' back IN, and a pick in <see cref="Days"/> is the only one that produces a value.
''' </summary>
Public Enum KBotCalendarView

    ''' <summary>One month: 7 columns x 6 week rows.</summary>
    Days = 0

    ''' <summary>One year: the twelve months, 4 x 3.</summary>
    Months = 1

    ''' <summary>One decade: the ten years plus the two neighbours, 4 x 3.</summary>
    Years = 2

End Enum
