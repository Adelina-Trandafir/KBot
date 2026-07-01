Imports System
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks

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
    End Interface
End Namespace
