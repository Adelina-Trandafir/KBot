Option Strict On

''' <summary>
''' Condiția unui filtru de coloană (slice 0028-03) — vocabularul submeniurilor «Text Filters» /
''' «Number Filters» / «Date Filters» din foaia de date Access.
'''
''' <para>Nu toate condițiile au sens pe orice tip de valoare: <see cref="Contains"/> pe o coloană
''' de sume e o întrebare fără răspuns, iar <see cref="Between"/> pe una de nume la fel. Ce se
''' oferă pe fiecare tip decide <see cref="KBotFilterEngine.AllowedOperators"/> — o singură listă,
''' citită și de meniu, și de validare, ca oferta să nu poată ajunge să difere de ce se acceptă.</para>
'''
''' <para><b>Operanzii sunt TEXT.</b> Îi tastează operatorul într-o casetă, iar citirea lor în tipul
''' coloanei se face la potrivire (vezi <see cref="KBotFilterEngine.MatchesCondition"/>), nu la
''' tastare: aceeași regulă de coerciție ca la formatare înseamnă că «1.234,5» se citește la fel în
''' celulă și în filtru.</para>
''' </summary>
Public Enum KBotFilterOperator

    ''' <summary>Fără condiție — coloana e filtrată doar prin lista de valori bifate.</summary>
    None = 0

    ''' <summary>Egal cu operandul.</summary>
    [Equals] = 1

    ''' <summary>Diferit de operand.</summary>
    NotEquals = 2

    ''' <summary>Conține textul (doar coloane de text).</summary>
    Contains = 3

    ''' <summary>Nu conține textul (doar coloane de text).</summary>
    NotContains = 4

    ''' <summary>Începe cu textul (doar coloane de text).</summary>
    BeginsWith = 5

    ''' <summary>Nu începe cu textul (doar coloane de text).</summary>
    NotBeginsWith = 6

    ''' <summary>Se termină cu textul (doar coloane de text).</summary>
    EndsWith = 7

    ''' <summary>Nu se termină cu textul (doar coloane de text).</summary>
    NotEndsWith = 8

    ''' <summary>Mai mic decât operandul (numere) / înainte de el (date).</summary>
    LessThan = 9

    ''' <summary>Mai mare decât operandul (numere) / după el (date).</summary>
    GreaterThan = 10

    ''' <summary>Între cei DOI operanzi, capetele incluse (numere și date).</summary>
    Between = 11

    ''' <summary>Celulă goală (Nothing sau text vid).</summary>
    IsEmpty = 12

    ''' <summary>Celulă necompletată — negația lui <see cref="IsEmpty"/>.</summary>
    IsNotEmpty = 13

End Enum
