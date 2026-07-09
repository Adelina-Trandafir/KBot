Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports WorkflowModels

' Gardă de regresie pentru contractul "niciun secret / nicio adresă de server în client":
' DownloadAction NU mai are ApiKey (vechea cheie X-Api-Key compilată în binar) și NICI
' ApiUrl (vechea adresă http necriptată). Tot HTTP-ul parseExcel stă acum în ApiClient,
' iar executorul primește conversia prin seam-ul SetExcelProcessor (fără token/adresă
' în FOREXE). Pandant al aserției "no X-Api-Key" din ListaAngajamenteApiClientUpsertTest.
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

        If actionType.GetProperty("ApiUrl", BindingFlags.Public Or BindingFlags.Instance) IsNot Nothing Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "DownloadAction.ApiUrl există — adresa serverului nu are voie să stea în model (tot HTTP-ul e în ApiClient)."))
        End If

        Dim seam As MethodInfo = GetType(WorkflowExecutor).GetMethod(
            "SetExcelProcessor", BindingFlags.Public Or BindingFlags.Instance)
        If seam Is Nothing Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "WorkflowExecutor.SetExcelProcessor lipsește — acțiunea Download nu are prin ce trimite Excel-ul la ApiClient."))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            "DownloadAction fără ApiKey/ApiUrl; seam-ul procesorului Excel (SetExcelProcessor) prezent."))
    End Function
End Class
