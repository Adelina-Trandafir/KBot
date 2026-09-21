Imports System
Imports System.Collections.Generic
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace KBot.Forexe
    ' Semnătura A3 (decizia #3): certificatul ales de utilizator este injectat în RunAsync.
    Public Interface IForexeRunner
        ' Conectare: forțează o sesiune nouă (lansează browserul, autentifică).
        Function RunAsync(job As JobRequest,
                          certificate As X509Certificate2,
                          progress As IProgress(Of Integer),
                          ct As CancellationToken) As Task(Of JobResult)

        ' Job pe sesiunea EXISTENTĂ (fără relansare browser). Injectează
        ' job.Parameters, execută .wfl-ul și întoarce variabilele în JobResult
        ' (Data plat + Tables pentru rezultate tabelare). Cere o sesiune vie.
        Function RunJobAsync(job As JobRequest,
                             progress As IProgress(Of Integer),
                             ct As CancellationToken) As Task(Of JobResult)

        ' Downloading the SNM bank statements, slice 0057. Not a workflow: FOREXE answers
        ' this one in JSON, and the old system called the path SNM_INTERNAL. It goes through
        ' the runner because the runner is the only thing holding a live page. Needs a live
        ' session and THROWS on failure (it has no JobResult to hand back).
        Function DescarcaExtraseAsync(folderDescarcare As String,
                                      dataDeLa As Date?,
                                      progres As Action(Of Integer, Integer, String),
                                      ct As CancellationToken) As Task(Of List(Of ExtrasDescarcat))

        ' Linia de stare a executorului, împinsă mai departe (felia 0034). Până acum se
        ' termina în logger; suprafețele de UI (banda de subsol + consola) au nevoie de ea
        ' ca text scurt, nu ca fișier de log.
        Event StatusUpdated As EventHandler(Of String)

        ' True dacă există o sesiune vie (executor cu browser deschis). Era doar pe clasă,
        ' iar gazdele făceau DirectCast la ForexeRunner ca s-o citească.
        ReadOnly Property HasLiveSession As Boolean

        ' Arată pagina browserului — ANDOCATĂ în fereastra recorderului, deschisă doar pentru
        ' privit (felia 0070). Fereastra Chromium nu apare niciodată singură pe ecran: are
        ' buton de închidere, iar o apăsare pe el omoară sesiunea. Proprietarul e fereastra
        ' peste care se deschide recorderul (poate fi Nothing).
        Function ShowBrowserAsync(owner As IWin32Window) As Task

        ' Ascunde la loc browserul: îl detașează din formularul-gazdă și îl parchează în afara
        ' ecranului (stealth). Perechea lui ShowBrowserAsync: de la felia 0034-02 browserul
        ' PORNEȘTE ascuns, deci fără asta o dată arătat nu mai putea fi ascuns înapoi.
        Function HideBrowserAsync() As Task

        ' Browserul e la vedere acum, adică andocat într-un formular? (pentru butonul care comută)
        ReadOnly Property IsBrowserVisible As Boolean

        ' Browserul tocmai s-a andocat sau s-a ascuns — inclusiv când operatorul a închis
        ' fereastra care îl găzduia, caz în care butonul din consolă trebuie să-și schimbe
        ' eticheta fără să fi fost apăsat. Poate veni de pe orice fir.
        Event BrowserVisibilityChanged As EventHandler

        ' Slice 0074 - the shell's «Browser FOREXE» view hosts the docked browser itself, in a
        ' panel of its own, with no recorder window in between. The three calls below are the
        ' recorder's docking toolbar reduced to what a plain host needs.
        '
        ' DockBrowserAsync: puts the live browser into `host` (and the floating K-BOT menu into
        ' the page). If it is docked somewhere else it is taken over; a recorder opened only
        ' for looking is closed, one opened for recording stays and shows itself undocked.
        ' Throws without a live session or when the host has no handle yet.
        Function DockBrowserAsync(host As Control) As Task

        ' ReleaseBrowserAsync: undocks and hides the browser IF it is docked in `host`. Quiet
        ' when it is elsewhere or there is no session: a view that is being hidden must not
        ' pull the browser out from under another host.
        Function ReleaseBrowserAsync(host As Control) As Task

        ' SyncBrowserBoundsAsync: re-fits the docked window to its host after a resize.
        Function SyncBrowserBoundsAsync() As Task

        ' The control the browser is docked into now, or Nothing (hidden, or no session).
        ReadOnly Property BrowserHost As Control

        ' The angajament code the page shows right now (its header); empty when none, when the
        ' menu is not in the page yet, or without a session. Never throws.
        Function ReadPageAngajamentAsync() As Task(Of String)

        ' Deschide bancul de înregistrare (felia 0053) peste sesiunea vie. Formularul
        ' trăiește în KBot.Forexe fiindcă are nevoie de WorkflowExecutor, care rămâne
        ' privat în runner; gazdele cer doar «arată-l», nu executorul.
        Sub ShowRecorder(owner As IWin32Window)

        ' The floating K-BOT menu inside the FOREXE page (slice 0073) saw the operator start,
        ' finish or abandon an operation: a new angajament, a reservation row, a reception.
        ' Kind = Finished carries the angajament code read from the page header and the
        ' interval [StartedAt, FinishedAt]; the shell downloads that angajament and opens its
        ' history for the interval. Raised on the Playwright callback thread - marshal first.
        Event OperationCaptured As EventHandler(Of ForexeWatchEvent)
    End Interface
End Namespace
