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

        ' Aduce fereastra browserului în față (delegare către WorkflowExecutor).
        Function ShowBrowserAsync() As Task

        ' Ascunde la loc fereastra browserului (stealth). Perechea lui ShowBrowserAsync:
        ' de la felia 0034-02 browserul PORNEȘTE ascuns, deci fără asta o dată arătat nu
        ' mai putea fi ascuns înapoi.
        Function HideBrowserAsync() As Task

        ' Browserul e la vedere acum? (pentru butonul care comută)
        ReadOnly Property IsBrowserVisible As Boolean

        ' Deschide bancul de înregistrare (felia 0053) peste sesiunea vie. Formularul
        ' trăiește în KBot.Forexe fiindcă are nevoie de WorkflowExecutor, care rămâne
        ' privat în runner; gazdele cer doar «arată-l», nu executorul.
        Sub ShowRecorder(owner As IWin32Window)
    End Interface
End Namespace
