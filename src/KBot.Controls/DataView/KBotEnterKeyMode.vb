Option Strict On

''' <summary>
''' Unde duce tasta Enter în <see cref="KBotDataView"/> — cele două obiceiuri de introducere a
''' datelor pe care le are un operator venit din Access.
'''
''' <para>Alegerea nu e cosmetică: ea decide dacă un tabel se completează pe COLOANE (același
''' câmp, rând după rând — liste de sume, de cantități) sau pe RÂNDURI (un rând întreg, câmp
''' după câmp, apoi rândul următor). Formularul care alege greșit îl pune pe operator să apese
''' săgeți între două apăsări de Enter.</para>
''' </summary>
Public Enum KBotEnterKeyMode

    ''' <summary>
    ''' Enter coboară pe RÂNDUL URMĂTOR, în aceeași coloană — formularul continuu clasic. Bun
    ''' pentru grilele completate pe verticală, o coloană odată.
    ''' </summary>
    NextRow = 0

    ''' <summary>
    ''' Enter trece pe URMĂTOAREA CELULĂ EDITABILĂ din același rând; când nu mai există niciuna
    ''' la dreapta, coboară pe primul câmp editabil al rândului următor. Bun pentru grilele
    ''' completate rând cu rând, unde Tab-ul ar cere o a doua mână.
    ''' </summary>
    NextEditableCell = 1

End Enum
