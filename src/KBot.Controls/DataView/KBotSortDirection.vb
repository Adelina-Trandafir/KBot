Option Strict On

''' <summary>
''' Sensul de sortare al unei coloane <see cref="KBotDataView"/> (slice 0028-03).
'''
''' <see cref="None"/> nu e „ascendent implicit”: e ORDINEA DE ÎNCĂRCARE, adică exact ordinea în
''' care apelantul a adăugat rândurile. Distincția contează — o vedere care aduce rândurile deja
''' ordonate de server (Istoric, Plăți) trebuie să poată reveni la acea ordine, iar «ascendent pe
''' prima coloană» nu e același lucru.
''' </summary>
Public Enum KBotSortDirection

    ''' <summary>Nesortat — rândurile rămân în ordinea de inserare.</summary>
    None = 0

    ''' <summary>Crescător (A→Z, 0→9, cel mai vechi întâi).</summary>
    Ascending = 1

    ''' <summary>Descrescător (Z→A, 9→0, cel mai nou întâi).</summary>
    Descending = 2

End Enum
