Option Strict On

''' <summary>
''' Contractul unei sub-pagini ORD găzduite de <c>OrdView</c> (felia 0033). Fratele lui
''' <see cref="IDdfPage"/>, cu aceeași înțelegere: pagina e PROASTĂ prin construcție —
''' primește un context de la părinte și îl redă. NU are client API, NU are sesiune, NU face
''' nicio cerere de rețea. Datele ORD se încarcă O SINGURĂ DATĂ pe CodAngajament, iar un
''' click în arbore doar filtrează local; dacă fiecare pagină și-ar aduce singură datele,
''' acea decizie s-ar rupe.
'''
''' Efect secundar util: fără injecție de dependențe, fiecare pagină are un constructor FĂRĂ
''' parametri, deci se instanțiază în designerul Visual Studio.
'''
''' MAI SUBȚIRE decât <c>IDdfPage</c>: ORD e read-only în felia asta — nu există generare și
''' nu există listă de fișiere, deci nicio pagină nu are ce ridica spre părinte. Când
''' generarea ORD va veni (felie ulterioară), aici se adaugă un <c>GenerateRequested</c>, la
''' fel ca la DDF.
''' </summary>
Public Interface IOrdPage

    ''' <summary>
    ''' Cheia paginii: «vizualizare» sau «document». Trebuie să fie IDENTICĂ cu cheia
    ''' intrării din <c>navSub</c> (designerul le scrie ca literale).
    ''' </summary>
    ReadOnly Property PageKey As String

    ''' <summary>
    ''' Redă contextul dat. <c>Nothing</c> = nicio ordonanțare selectată -&gt; pagina își arată
    ''' starea goală.
    ''' </summary>
    Sub SetContext(ctx As OrdPageContext)

    ''' <summary>
    ''' Ridicat de butonul «Generează documentul» de pe suprafața „document lipsă". Îl ridică
    ''' doar paginile «Vizualizare» și «Document»; celelalte niciodată.
    ''' </summary>
    Event GenerateRequested As EventHandler

    ''' <summary>
    ''' Ridicat când operatorul alege un fișier din listă. Îl ridică doar pagina «Fișiere»;
    ''' părintele calculează ținta PDF și comută pe pagina «Document».
    ''' </summary>
    Event FileActivated As EventHandler(Of String)

End Interface
