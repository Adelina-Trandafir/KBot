Imports System.Threading
Imports System.Threading.Tasks

' Api, LIVE (când va fi implementat): slot vizibil. Se completează când apare primul
' endpoint routes/forexe/ + metoda corespunzătoare pe IApiClient (Felia 1, Status §4.5).
Public NotInheritable Class ApiReferenceDataTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "API — date de referință"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Api"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return True
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Return Task.FromResult(HarnessTestResult.Skipped("Endpoint-ul de date de referință nu există încă (Felia 1, Status §4.5)."))
    End Function
End Class
