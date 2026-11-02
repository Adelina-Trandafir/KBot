Option Strict On

''' <summary>
''' Which part of a date a <see cref="KBotGroupLevel"/> groups on (KBotAdmin "Tabele", 02.11.2026). Only read for a column whose
''' <see cref="KBotValueType"/> is DateTime and whose cell holds a real date; for any other column the level groups on the displayed text,
''' as before. The time of day never takes part: a date-time shown as dd.MM.yyyy HH:mm:ss and grouped by <see cref="Day"/> is one group per
''' day, not one per second.
''' </summary>
Public Enum KBotGroupDatePart

    ''' <summary>The displayed text is the key (the behaviour before this enum, and for every column that is not a date).</summary>
    None = 0

    ''' <summary>One group per calendar day, titled dd.MM.yyyy.</summary>
    Day = 1

    ''' <summary>One group per month of a year, titled MM.yyyy.</summary>
    Month = 2

    ''' <summary>One group per year, titled yyyy.</summary>
    Year = 3

End Enum
