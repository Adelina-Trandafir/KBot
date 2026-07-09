Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports WorkflowModels

' Gardă de regresie pentru contractul "niciun secret de flotă în client":
' DownloadAction NU mai are proprietatea ApiKey (vechea cheie X-Api-Key compilată
' în binar), iar executorul expune seam-ul de token bearer (SetSessionTokenProvider)
' prin care acțiunea Download se autentifică pe sesiunea K-BOT.
' Pandant al aserțiunii "no X-Api-Key" din ListaAngajamenteApiClientUpsertTest.
Public NotInheritable Class DownloadActionNoClientSecretTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Download — fără secret compilat (bearer, nu ApiKey)"
        End Get
    End Property

    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Forexe"
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
        Dim actionType As Type = GetType(DownloadAction)

        If actionType.GetProperty("ApiKey", BindingFlags.Public Or BindingFlags.Instance) IsNot Nothing Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "DownloadAction.ApiKey există — cheia API nu are voie să reapară în client (folosește token-ul bearer al sesiunii)."))
        End If

        Dim seam As MethodInfo = GetType(WorkflowExecutor).GetMethod(
            "SetSessionTokenProvider", BindingFlags.Public Or BindingFlags.Instance)
        If seam Is Nothing Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "WorkflowExecutor.SetSessionTokenProvider lipsește — acțiunea Download nu are de unde lua token-ul bearer."))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            "DownloadAction fără ApiKey; seam-ul bearer (SetSessionTokenProvider) prezent."))
    End Function
End Class
