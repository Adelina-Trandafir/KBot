Option Strict On

''' <summary>
''' Ce fel de BANDĂ e un rând de pe ecran, într-o grilă grupată (slice 0029).
'''
''' <para>Fără grupare există un singur fel — <see cref="Data"/> — și grila lucrează, ca până
''' acum, direct în poziții de vedere. Cu grupare, între rândurile de date se intercalează benzi
''' care NU sunt rânduri de model: nu au index de model, nu se pot selecta ca celule, nu intră în
''' <c>RowCount</c> și nu se scriu niciodată înapoi.</para>
''' </summary>
Public Enum KBotGroupBandKind

    ''' <summary>Un rând de date obișnuit (are index de model și poziție de vedere).</summary>
    Data = 0

    ''' <summary>Antetul unui grup — titlul lui, și singurul loc de unde se poate strânge.</summary>
    GroupHeader = 1

    ''' <summary>Subsolul unui grup — linia lui de totaluri, sora benzii de subsol a grilei.</summary>
    GroupFooter = 2

End Enum
