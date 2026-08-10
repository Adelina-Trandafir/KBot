Option Strict On

''' <summary>
''' Formatele de afișare NUMITE ale unei coloane <see cref="KBotDataView"/>, în vocabularul
''' proprietății <c>Format</c> a unui textbox Access (slice 0028-02). Operatorul alege din listă
''' „Standard” sau „Short Date”, nu scrie un format .NET — exact ca în Access.
'''
''' <para><b>Relația cu <see cref="KBotDataColumn.FormatString"/>.</b> Sunt DOUĂ fețe ale
''' aceluiași lucru, deci nu se folosesc împreună: formatul numit e lista, <c>FormatString</c> e
''' portița pentru un format .NET oarecare. Amândouă setate => excepție (vezi
''' <see cref="KBotDataColumn.Format"/>), niciodată una câștigând tăcut în fața celeilalte.</para>
'''
''' <para><b>De ce numele sunt englezești, dar textul e românesc.</b> Numele membrilor sunt
''' vocabularul Access, cel pe care îl caută cine portează un formular (<c>Yes/No</c>,
''' <c>Short Date</c>); ce se SCRIE în celulă e text de interfață, deci e românesc: „Da” / „Nu”.</para>
''' </summary>
Public Enum KBotFormat

    ''' <summary>Fără format numit — se folosește <see cref="KBotDataColumn.FormatString"/>.</summary>
    None = 0

    ''' <summary>Access «General Number»: numărul așa cum e, fără separatori de mii.</summary>
    GeneralNumber = 1

    ''' <summary>Access «Currency»: simbolul monetar al culturii curente + separatori de mii.</summary>
    Currency = 2

    ''' <summary>Access «Euro»: ca <see cref="Currency"/>, dar cu simbolul € impus.</summary>
    Euro = 3

    ''' <summary>Access «Fixed»: zecimale fixe, FĂRĂ separatori de mii (implicit 2).</summary>
    Fixed = 4

    ''' <summary>Access «Standard»: separatori de mii + zecimale fixe (implicit 2).</summary>
    Standard = 5

    ''' <summary>Access «Percent»: valoarea × 100, urmată de %.</summary>
    Percent = 6

    ''' <summary>Access «Scientific»: notație exponențială (1,23E+04).</summary>
    Scientific = 7

    ''' <summary>Access «General Date»: data, plus ora doar dacă valoarea chiar are una.</summary>
    GeneralDate = 8

    ''' <summary>Access «Long Date»: data lungă a culturii curente.</summary>
    LongDate = 9

    ''' <summary>Access «Medium Date»: zi-lună prescurtată-an (11-aug-26).</summary>
    MediumDate = 10

    ''' <summary>Access «Short Date»: data scurtă a culturii curente.</summary>
    ShortDate = 11

    ''' <summary>Access «Long Time»: ora completă, cu secunde.</summary>
    LongTime = 12

    ''' <summary>Access «Medium Time»: ore:minute cu indicator AM/PM.</summary>
    MediumTime = 13

    ''' <summary>Access «Short Time»: ore:minute, 24h.</summary>
    ShortTime = 14

    ''' <summary>Access «Yes/No»: „Da” / „Nu”.</summary>
    YesNo = 15

    ''' <summary>Access «True/False»: „Adevărat” / „Fals”.</summary>
    TrueFalse = 16

    ''' <summary>Access «On/Off»: „Pornit” / „Oprit”.</summary>
    OnOff = 17

End Enum
