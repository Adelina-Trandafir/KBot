Option Strict On

''' <summary>
''' Unde duce tasta Tab dintr-un editor deschis al <see cref="KBotDataView"/> (slice 00EF-51).
''' Fără editor deschis, Tab rămâne mutarea de coloană.
''' </summary>
Public Enum KBotTabKeyMode

    ''' <summary>Tab trece pe următoarea coloană ACTIVĂ a rândului, editabilă sau nu — cum a fost mereu.</summary>
    NextColumn = 0

    ''' <summary>
    ''' Tab trece pe URMĂTOAREA CELULĂ EDITABILĂ a rândului (sare peste cele doar de citit și peste butoane); după ultima coboară
    ''' pe primul câmp editabil al rândului următor, ca Enter în <see cref="KBotEnterKeyMode.NextEditableCell"/>. Shift+Tab
    ''' merge înapoi pe celula editabilă dinainte.
    ''' </summary>
    NextEditableCell = 1

End Enum
