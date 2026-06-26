Imports System
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks

Namespace KBot.Forexe
    ' Semnătura A3 (decizia #3): certificatul ales de utilizator este injectat în RunAsync.
    Public Interface IForexeRunner
        Function RunAsync(job As JobRequest,
                          certificate As X509Certificate2,
                          progress As IProgress(Of Integer),
                          ct As CancellationToken) As Task(Of JobResult)
    End Interface
End Namespace
