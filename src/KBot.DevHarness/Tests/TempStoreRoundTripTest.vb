Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.LocalStore

' LocalStore, safe.
' ITempStore (SqliteTempStore) este încă un stub: Open()/Reset() aruncă NotImplementedException
' (conexiunea/schema reală vin în Felia 1). Round-trip-ul write→read→verify nu există încă,
' deci acest test oglindește exact ceea ce verifică suita xUnit LocalStore.Tests
' (SqliteTempStoreTests): faptul că Open() aruncă NotImplementedException — contractul curent
' al stub-ului. Se rescrie ca round-trip propriu-zis când SqliteTempStore capătă schema reală.
Public NotInheritable Class TempStoreRoundTripTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "TempStore round-trip (stub contract)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "LocalStore"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Dim store As ITempStore = context.GetService(Of ITempStore)()
        context.Log("ITempStore rezolvat: " & store.GetType().Name)

        Try
            store.Open()
            ' Dacă Open() nu mai aruncă, schema reală a fost implementată: stub-ul nu mai e contractul curent.
            Return Task.FromResult(HarnessTestResult.Failed(
                "Open() nu a mai aruncat NotImplementedException — stub-ul a fost implementat. " &
                "Rescrie acest test ca round-trip real write→read→verify (Felia 1)."))
        Catch nie As NotImplementedException
            context.Log("Open() a aruncat NotImplementedException — contractul stub curent, conform LocalStore.Tests.")
            Return Task.FromResult(HarnessTestResult.Passed(
                "stub contract OK (Open() aruncă NotImplementedException); round-trip real pending Felia 1"))
        Catch ex As Exception
            Return Task.FromResult(HarnessTestResult.Failed(
                "Open() a aruncat o excepție neașteptată: " & ex.GetType().Name, ex.ToString()))
        End Try
    End Function
End Class
