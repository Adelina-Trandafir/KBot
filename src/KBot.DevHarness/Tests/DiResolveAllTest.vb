Imports System
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Forexe
Imports KBot.LocalStore

' Smoke, safe: verifică faptul că serviciile cheie se rezolvă din containerul DI al shell-ului.
Public NotInheritable Class DiResolveAllTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Resolve all DI services"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Smoke"
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
        Dim sb As New StringBuilder()
        Dim ok As Boolean = True
        ok = ResolveOne(Of IApiClient)(context, sb) AndAlso ok
        ok = ResolveOne(Of IAuthApi)(context, sb) AndAlso ok
        ok = ResolveOne(Of IForexeRunner)(context, sb) AndAlso ok
        ok = ResolveOne(Of ITempStore)(context, sb) AndAlso ok
        ok = ResolveOne(Of SessionContext)(context, sb) AndAlso ok

        If ok Then
            Return Task.FromResult(HarnessTestResult.Passed("all services resolved"))
        Else
            Return Task.FromResult(HarnessTestResult.Failed("one or more services failed to resolve", sb.ToString()))
        End If
    End Function

    Private Shared Function ResolveOne(Of T)(context As HarnessContext, sb As StringBuilder) As Boolean
        Try
            Dim svc As T = context.GetService(Of T)()
            sb.AppendLine("OK   " & GetType(T).Name)
            Return svc IsNot Nothing
        Catch ex As Exception
            sb.AppendLine("FAIL " & GetType(T).Name & ": " & ex.Message)
            Return False
        End Try
    End Function
End Class
