Imports System.Threading
Imports System.Threading.Tasks

' Contractul unui test din banc de probă. O clasă care îl implementează (cu
' constructor fără parametri) apare singură în lista harness-ului (vezi HarnessTestDiscovery).
Public Interface IHarnessTest
    ReadOnly Property Name As String
    ReadOnly Property Category As String
    ReadOnly Property RequiresLiveConnection As Boolean
    ReadOnly Property IsDestructive As Boolean
    Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult)
End Interface
