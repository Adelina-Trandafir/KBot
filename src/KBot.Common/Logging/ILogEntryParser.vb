Option Strict On

''' <summary>
''' Un analizor răspunde la O SINGURĂ întrebare: linia asta începe o intrare nouă, și dacă da, ce
''' câmpuri are? Continuarea (liniile de stivă, liniile fără antet) e treaba lui
''' <c>LogFileLoader</c>, nu a analizorului — altfel fiecare analizor ar reimplementa aceeași
''' mașină de stări.
''' </summary>
Public Interface ILogEntryParser

    ''' <summary>Numele analizorului, pentru raportarea «cine a câștigat» din încărcător.</summary>
    ReadOnly Property Name As String

    ''' <summary>
    ''' True dacă formatul pune o intrare pe FIECARE linie (adobe, tree, api), False dacă o
    ''' intrare se întinde normal pe mai multe linii (<c>harness_errors.log</c>: un antet și toată
    ''' stiva de sub el).
    '''
    ''' <para>Există pentru proba de potrivire din <c>LogFileLoader</c>. Regula «sub 30% anteturi
    ''' recunoscute înseamnă ghicire greșită» e corectă pentru formatele cu o intrare pe linie și
    ''' GREȘITĂ pentru cele pe blocuri: un <c>harness_errors.log</c> perfect sănătos, cu un antet
    ''' și douăzeci de linii de stivă, are 5% anteturi și ar fi declarat ghicire greșită. Pentru
    ''' formatele pe blocuri proba cere doar să existe MĂCAR UN antet — ceea ce prinde exact
    ''' defectul căutat (conținut de alt format într-un fișier cu numele ăsta), fără să declare
    ''' greșit un fișier normal.</para>
    ''' </summary>
    ReadOnly Property ExpectsHeaderOnEveryLine As Boolean

    ''' <summary>
    ''' True dacă <paramref name="line"/> e o linie de ANTET, caz în care
    ''' <paramref name="result"/> primește intrarea nouă. False lasă
    ''' <paramref name="result"/> nedefinit și înseamnă «linie de continuare».
    ''' </summary>
    Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean

End Interface
