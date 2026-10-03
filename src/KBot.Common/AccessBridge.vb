Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.Loader
Imports KBot.Domain

''' <summary>What <see cref="IAccessBridge.SendBudget"/> did in the Access file.</summary>
Public NotInheritable Class AccessSendResult
    Public Property File As String = String.Empty
    ''' <summary>Rows of <c>Clasificatii</c> updated (0 = the classification is not in that file).</summary>
    Public Property BudgetUpdated As Integer
    Public Property RectificariInserted As Integer
    Public Property RectificariUpdated As Integer
End Class

''' <summary>
''' Slice 0104 -- the ONE door between the application and everything that talks to the Access files.
''' The code behind it lives in its own assembly, <c>KBot.Access.dll</c>, that only the package for
''' Access clients contains; the application never references that assembly, it finds it at run time
''' (<see cref="AccessBridge"/>). A package without the file simply has no bridge.
''' </summary>
Public Interface IAccessBridge

    ''' <summary>
    ''' The rows of the registry table <c>cai</c> of <paramref name="registryPath"/> (the path is the
    ''' operator's, <see cref="AppSettings.AccessRegistryPath"/>) for year <paramref name="an"/> (<c>AnDate</c>)
    ''' and source <paramref name="sursa"/> (<c>SURSA</c>), every column as stored; a year of 0 or less, or an empty
    ''' source, means «any». Throws
    ''' <see cref="InvalidOperationException"/> (Romanian message) when the registry cannot be read.
    ''' </summary>
    Function ReadRegistry(registryPath As String, an As Integer, sursa As String) As DataTable

    ''' <summary>
    ''' The Access file of unit <paramref name="idUnitate"/>, as the registry <paramref name="registryPath"/>
    ''' names it. Throws <see cref="InvalidOperationException"/> (Romanian message) when the registry, the
    ''' unit row or the file is missing.
    ''' </summary>
    Function ResolveUnitFile(registryPath As String, idUnitate As Integer) As String

    ''' <summary>
    ''' Writes the budget version and the rectifications of the classification whose Access id is
    ''' <paramref name="idClsfAcc"/> into <paramref name="unitFile"/>, in one transaction.
    ''' </summary>
    Function SendBudget(unitFile As String, idClsfAcc As Integer, budget As BudgetVersion,
                        corrections As IEnumerable(Of RectificareBugetara)) As AccessSendResult
End Interface

''' <summary>
''' Finds <c>KBot.Access.dll</c> next to the application and hands out its <see cref="IAccessBridge"/>.
''' The assembly is loaded into its own load context: its own dependencies (the OLE DB provider
''' package) are resolved from ITS folder through its <c>.deps.json</c>, while every <c>KBot.*</c>
''' assembly -- <c>KBot.Common</c> above all, because <see cref="IAccessBridge"/> must be the SAME type
''' on both sides -- comes from the application.
''' </summary>
Public NotInheritable Class AccessBridge

    ''' <summary>File name of the component, in the application folder.</summary>
    Public Const ComponentFileName As String = "KBot.Access.dll"

    Private Shared ReadOnly _sync As New Object()
    Private Shared _tried As Boolean
    Private Shared _instance As IAccessBridge

    Private Sub New()
    End Sub

    ''' <summary>The component is in the application folder (a package without Access does not have it).</summary>
    Public Shared ReadOnly Property IsInstalled As Boolean
        Get
            Return File.Exists(ComponentPath())
        End Get
    End Property

    ''' <summary>
    ''' The bridge, or Nothing when the component is not installed OR could not be loaded (the cause is in
    ''' the error log, written once by <see cref="Load"/>). Loaded on first use, kept for the process.
    ''' </summary>
    Public Shared ReadOnly Property Instance As IAccessBridge
        Get
            SyncLock _sync
                If Not _tried Then
                    _tried = True
                    Try
                        If IsInstalled Then _instance = Load(ComponentPath())
                    Catch
                        ' Already logged by Load; the Access features simply stay off.
                        _instance = Nothing
                    End Try
                End If
                Return _instance
            End SyncLock
        End Get
    End Property

    Private Shared Function ComponentPath() As String
        Return Path.Combine(AppContext.BaseDirectory, ComponentFileName)
    End Function

    ' Reflection / assembly loading: a risky boundary -- logs and rethrows (the caller above decides).
    Private Shared Function Load(path As String) As IAccessBridge
        Try
            Dim context As New ComponentLoadContext(path)
            Dim asm As Assembly = context.LoadFromAssemblyPath(path)
            Dim bridgeType As Type = asm.GetTypes().FirstOrDefault(
                Function(t) t.IsClass AndAlso Not t.IsAbstract AndAlso GetType(IAccessBridge).IsAssignableFrom(t))
            If bridgeType Is Nothing Then
                Throw New InvalidOperationException($"{ComponentFileName} nu conține nicio clasă {NameOf(IAccessBridge)}.")
            End If
            Return DirectCast(Activator.CreateInstance(bridgeType), IAccessBridge)
        Catch ex As Exception
            GlobalErrorLog.Write("AccessBridge.Load", ex)
            Throw
        End Try
    End Function

    Private NotInheritable Class ComponentLoadContext
        Inherits AssemblyLoadContext

        Private ReadOnly _resolver As AssemblyDependencyResolver

        Public Sub New(componentPath As String)
            MyBase.New("KBot.Access", isCollectible:=False)
            _resolver = New AssemblyDependencyResolver(componentPath)
        End Sub

        ' Nothing = "not mine": the runtime then asks the default context (the application's own).
        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
            If assemblyName.Name IsNot Nothing AndAlso assemblyName.Name.StartsWith("KBot.", StringComparison.Ordinal) Then
                Return Nothing
            End If
            Dim found As String = _resolver.ResolveAssemblyToPath(assemblyName)
            Return If(found Is Nothing, Nothing, LoadFromAssemblyPath(found))
        End Function
    End Class

End Class

''' <summary>
''' Slice 0104 -- may the Access features be shown and used right now? Both must hold: the connected
''' unit's <c>Setari.Access</c> is on (<see cref="ServerSettings.AccessEnabled"/>, read after login) AND
''' the component is in this installation (<see cref="AccessBridge.Instance"/>). Anything that talks to
''' Access asks here first; a client without Access never even sees the buttons.
''' </summary>
Public NotInheritable Class AccessFeature

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property Enabled As Boolean
        Get
            Return ServerSettings.AccessEnabled AndAlso AccessBridge.Instance IsNot Nothing
        End Get
    End Property

End Class
