Option Strict On

''' <summary>
''' Agregatul pe care o <see cref="KBotDataColumn"/> îl aduce în banda de SUBSOL
''' (<see cref="KBotDataView.FooterVisible"/> trebuie să fie True ca banda să apară).
''' O coloană cu <see cref="KBotAggregate.None"/> lasă celula de subsol goală — și, din slice
''' 0028, ÎI DISPAR ȘI LINIILE VERTICALE: în subsol se despart doar coloanele agregate.
'''
''' Agregatele se calculează peste TOATE rândurile din model (nu doar cele vizibile).
''' Ce agregate sunt permise depinde de <see cref="KBotDataColumn.ValueType"/>, nu de tipul de
''' pictare — vezi <see cref="KBotAggregateRules"/>; o pereche nepermisă e o EXCEPȚIE, nu o
''' celulă goală în tăcere.
''' </summary>
Public Enum KBotAggregate

    ''' <summary>Fără agregat — celula de subsol stă goală, fără separatoare verticale.</summary>
    None = 0

    ''' <summary>Suma celulelor numerice (cele ne-numerice / <c>Nothing</c> se sar). Doar <see cref="KBotValueType.Number"/>.</summary>
    Sum = 1

    ''' <summary>Numărul rândurilor care AU o valoare stocată (<see cref="KBotDataRow.HasValue"/>),
    ''' NU numărul celulelor numerice ne-vide. Se randează întotdeauna ca întreg.</summary>
    Count = 2

    ''' <summary>Media celulelor numerice. Zero celule numărabile => text vid (niciodată 0, niciodată NaN).
    ''' Doar <see cref="KBotValueType.Number"/>.</summary>
    Average = 3

    ''' <summary>Cea mai mică valoare (numerică sau calendaristică). Doar Number / DateTime.</summary>
    Min = 4

    ''' <summary>Cea mai mare valoare (numerică sau calendaristică). Doar Number / DateTime.</summary>
    Max = 5

    ''' <summary>Câte valori DISTINCTE apar în coloană (comparate pe textul afișat, deci în
    ''' aceeași cultură cu restul grilei). Celulele goale nu intră la socoteală.</summary>
    CountDistinct = 6

    ''' <summary>Câte rânduri au celula GOALĂ (fără valoare stocată, <c>Nothing</c>, sau text alb).</summary>
    CountEmpty = 7

    ''' <summary>Câte celule sunt bifate. Doar <see cref="KBotValueType.Boolean"/>.</summary>
    CountTrue = 8

    ''' <summary>Câte celule cu valoare stocată NU sunt bifate. Doar <see cref="KBotValueType.Boolean"/>.</summary>
    CountFalse = 9

    ''' <summary>Textul afișat al PRIMEI celule cu valoare (ordinea rândurilor din model).</summary>
    First = 10

    ''' <summary>Textul afișat al ULTIMEI celule cu valoare (ordinea rândurilor din model).</summary>
    Last = 11

End Enum
