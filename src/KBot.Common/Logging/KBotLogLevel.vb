Option Strict On

''' <summary>
''' Nivelul unei intrări de jurnal, în ORDINE CRESCĂTOARE a gravității, ca o comparație de tip
''' «cel puțin acest nivel» să fie un simplu <c>&gt;=</c> dacă va fi vreodată nevoie.
'''
''' <c>Unknown</c> stă primul fiindcă e cel mai puțin grav lucru pe care îl putem spune despre o
''' linie: că nu am recunoscut-o. NU e o eroare — fișierele de rulare ale bancului de probă sunt
''' pline de linii fără nivel (anteturi, linii de progres), și ele sunt perfect normale.
''' </summary>
Public Enum KBotLogLevel
    Unknown = 0
    Trace = 1
    Debug = 2
    Info = 3
    Warn = 4
    [Error] = 5
End Enum

''' <summary>
''' De unde vine intrarea. Decide coloana «Sursă» din grilă și dacă marcajul de timp are nevoie
''' de corecția de ceas față de server.
''' </summary>
Public Enum LogOrigin
    Client = 0
    Server = 1
End Enum
