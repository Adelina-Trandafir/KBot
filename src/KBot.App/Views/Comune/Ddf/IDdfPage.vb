Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Contractul unei sub-pagini DDF găzduite de <c>DdfView</c> (felia 0032).
'''
''' O pagină e PROASTĂ prin construcție: primește un context de la părinte și îl redă. NU are
''' client API, NU are sesiune, NU are logică de afaceri și NU face nicio cerere de rețea —
''' toate rămân la <c>DdfView</c>, fiindcă datele DDF se încarcă O SINGURĂ DATĂ pe
''' CodAngajament, iar un click în arbore doar filtrează local (decizia 7 a feliei 0020). Dacă
''' fiecare pagină și-ar aduce singură datele, acea decizie s-ar rupe.
'''
''' Efect secundar util: fără injecție de dependențe, fiecare pagină are un constructor FĂRĂ
''' parametri, deci se instanțiază în designerul Visual Studio — care e chiar scopul feliei.
''' </summary>
Public Interface IDdfPage

    ''' <summary>
    ''' Cheia paginii: «valori», «previzualizare», «document», «fisiere». Trebuie să fie
    ''' IDENTICĂ cu cheia intrării din <c>navSub</c> (designerul le scrie ca literale).
    ''' </summary>
    ReadOnly Property PageKey As String

    ''' <summary>
    ''' Redă contextul dat. <c>Nothing</c> = niciun nod selectat -&gt; pagina își arată starea goală.
    ''' </summary>
    Sub SetContext(ctx As DdfPageContext)

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
