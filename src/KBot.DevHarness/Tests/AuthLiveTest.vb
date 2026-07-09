Option Strict On
Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Domain

' LIVE: proba fluxul de login end-to-end contra API-ului real si a contului operator
' de test. GetUnits -> Login (000_DEMO, emite token bearer) -> Logout (revoca token).
' Scrie un rezumat in <AppDir>\Logs\test_auth.log. Se skip-uieste daca API-ul nu e
' configurat sau daca lipsesc credentialele de test (TEST_OP_USER / TEST_OP_PASS).
' Destructiv: creeaza si revoca o sesiune reala pe server.
Public NotInheritable Class AuthLiveTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Auth — login LIVE (units -> login -> logout)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Auth"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return True
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return True
        End Get
    End Property

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        Dim user As String = Environment.GetEnvironmentVariable("TEST_OP_USER")
        Dim pass As String = Environment.GetEnvironmentVariable("TEST_OP_PASS")
        If String.IsNullOrWhiteSpace(user) OrElse String.IsNullOrWhiteSpace(pass) Then
            Return HarnessTestResult.Skipped("Set TEST_OP_USER / TEST_OP_PASS, then relaunch KBOT.")
        End If

        Dim opt As ApiOptions = context.GetService(Of ApiOptions)()
        If String.IsNullOrWhiteSpace(opt.BaseUrl) Then
            Return HarnessTestResult.Skipped(
                "API address missing — ApiOptions.BaseUrl (built-in constant) is empty; check the build.")
        End If

        Dim auth As IAuthApi = context.GetService(Of IAuthApi)()

        ' 1) Faza 1: unitati accesibile.
        Dim units As IReadOnlyList(Of UnitInfo) = Await auth.GetUnitsAsync(user, pass, Nothing, ct)
        Dim demo As UnitInfo = units.FirstOrDefault(Function(u) u.DbName = "000_DEMO")
        If demo Is Nothing Then
            Return LogAndReturn(HarnessTestResult.Failed(
                $"Operatorul '{user}' nu vede 000_DEMO ({units.Count} unitati)."), user)
        End If

        ' 2) Faza 2: login pe 000_DEMO.
        Dim result As LoginResult = Await auth.LoginAsync(user, pass, demo.IdUnitate, "DEVHARNESS", ct)
        If result.SessionContext.DbName <> "000_DEMO" Then
            Return LogAndReturn(HarnessTestResult.Failed(
                $"SessionContext.DbName='{result.SessionContext.DbName}', asteptat 000_DEMO."), user)
        End If

        ' Rol store-only: verifica doar daca username-ul poarta un sufix cunoscut.
        Dim roleOk As Boolean = True
        If user.EndsWith("_Contabil", StringComparison.OrdinalIgnoreCase) Then
            roleOk = (result.SessionContext.Role = "Contabil")
        ElseIf user.EndsWith("_Administrator", StringComparison.OrdinalIgnoreCase) Then
            roleOk = (result.SessionContext.Role = "Administrator")
        End If
        If Not roleOk Then
            Return LogAndReturn(HarnessTestResult.Failed(
                $"Rol nepotrivit: '{result.SessionContext.Role}' pentru '{user}'."), user)
        End If

        ' 3) Logout (revoca token-ul emis).
        Await auth.LogoutAsync(result.Token, ct)

        ' SECURITY: token-ul e credential bearer — nu se logheaza valoarea, doar lungimea.
        Return LogAndReturn(HarnessTestResult.Passed(
            $"Login OK: token emis (len={result.Token.Length}), DbName=000_DEMO, Role='{result.SessionContext.Role}', logout (revocare) OK."), user)
    End Function

    ' Scrie o linie de rezumat in <AppDir>\Logs\test_auth.log si intoarce rezultatul.
    Private Shared Function LogAndReturn(result As HarnessTestResult, user As String) As HarnessTestResult
        Try
            Dim dir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
            Directory.CreateDirectory(dir)
            Dim line As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  [{user}]  {result.Outcome}  {result.Message}" & Environment.NewLine
            File.AppendAllText(Path.Combine(dir, "test_auth.log"), line)
        Catch
            ' Log-ul e diagnostic; esecul lui nu schimba rezultatul testului.
        End Try
        Return result
    End Function
End Class
