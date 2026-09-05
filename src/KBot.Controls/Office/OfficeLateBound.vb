Option Strict On
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices

''' <summary>
''' The IDispatch calls the Office host needs, spelled out through reflection.
'''
''' <para><b>Why reflection and not a COM reference.</b> A <c>COMReference</c> on
''' <c>Microsoft.Office.Interop.Excel</c> makes Office a BUILD dependency: the type library has to be
''' registered on whatever machine compiles K-BOT, and the resulting assembly then insists on an
''' Office generation at run time as well. The preview is a convenience, not a feature K-BOT can be
''' held hostage by, so the calls go out through <see cref="Type.InvokeMember"/> instead: K-BOT
''' builds and runs on a machine with no Office at all, and the operator gets a Romanian sentence
''' saying the program is missing rather than a load failure.</para>
'''
''' <para><b>Why not <c>Option Strict Off</c> and plain late binding.</b> House rule: the whole
''' solution is <c>Option Strict On</c>. Reflection is the way to keep it that way, and it buys one
''' thing plain late binding cannot — the CULTURE argument below.</para>
'''
''' <para><b>The culture argument is not decoration.</b> Office's IDispatch is locale-sensitive.
''' Driven from a process running under a Romanian locale, a member call made with the ambient
''' culture can come back as <c>0x80020005</c> («Type mismatch») or the notorious «Old format or
''' invalid type library». Every call here goes out under <c>en-US</c>, which is the locale Office's
''' own type information is written in.</para>
''' </summary>
Friend NotInheritable Class OfficeLateBound

    Private Sub New()
    End Sub

    ''' <summary>The locale every member call is made under. See the class remarks.</summary>
    Private Shared ReadOnly _comCulture As New CultureInfo("en-US")

    ''' <summary>
    ''' Creates the automation server behind <paramref name="progId"/>, or <c>Nothing</c> when the
    ''' ProgID is not registered — which is what "the program is not installed" looks like from here.
    ''' </summary>
    Public Shared Function CreateFromProgId(progId As String) As Object
        Try
            Dim t As Type = Type.GetTypeFromProgID(progId, throwOnError:=False)
            If t Is Nothing Then Return Nothing
            Return Activator.CreateInstance(t)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeLateBound.CreateFromProgId", ex)
            Throw
        End Try
    End Function

    ''' <summary>Reads a property (or an indexed one, when <paramref name="args"/> is given).</summary>
    Public Shared Function GetProp(target As Object, name As String, ParamArray args As Object()) As Object
        Try
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            Return target.GetType().InvokeMember(name, BindingFlags.GetProperty, Nothing, target, args, _comCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeLateBound.GetProp:" & name, ex)
            Throw
        End Try
    End Function

    ''' <summary>Writes a property.</summary>
    Public Shared Sub SetProp(target As Object, name As String, value As Object)
        Try
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            target.GetType().InvokeMember(name, BindingFlags.SetProperty, Nothing, target, {value}, _comCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeLateBound.SetProp:" & name, ex)
            Throw
        End Try
    End Sub

    ''' <summary>Calls a method.</summary>
    Public Shared Function Invoke(target As Object, name As String, ParamArray args As Object()) As Object
        Try
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            Return target.GetType().InvokeMember(name, BindingFlags.InvokeMethod, Nothing, target, args, _comCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficeLateBound.Invoke:" & name, ex)
            Throw
        End Try
    End Function

    ' ── The forgiving trio ──────────────────────────────────────────────────────
    ' Everything the host does to STRIP CHROME goes through these. A property that one Office
    ' generation exposes and the next one renamed, removed or refuses in the current view must not
    ' abort a preview that is otherwise working: the ribbon staying up is a blemish, an exception
    ' here would be a blank pane. Each failure is written to the working log, so a blemish that
    ' turns out to matter is still traceable.

    ''' <summary>Reads a property; on any failure returns <c>Nothing</c> and logs one line.</summary>
    Public Shared Function TryGetProp(target As Object, name As String, ParamArray args As Object()) As Object
        Try
            If target Is Nothing Then Return Nothing
            Return target.GetType().InvokeMember(name, BindingFlags.GetProperty, Nothing, target, args, _comCulture)
        Catch ex As Exception
            OfficeHostLog.Write($"Property {name} could not be read: {OfficeHostLog.Describe(ex)}")
            Return Nothing
        End Try
    End Function

    ''' <summary>Writes a property; on any failure returns False and logs one line.</summary>
    Public Shared Function TrySetProp(target As Object, name As String, value As Object) As Boolean
        Try
            If target Is Nothing Then Return False
            target.GetType().InvokeMember(name, BindingFlags.SetProperty, Nothing, target, {value}, _comCulture)
            Return True
        Catch ex As Exception
            OfficeHostLog.Write($"Property {name} could not be written: {OfficeHostLog.Describe(ex)}")
            Return False
        End Try
    End Function

    ''' <summary>Calls a method; on any failure returns False and logs one line.</summary>
    Public Shared Function TryInvoke(target As Object, name As String, ParamArray args As Object()) As Boolean
        Try
            If target Is Nothing Then Return False
            target.GetType().InvokeMember(name, BindingFlags.InvokeMethod, Nothing, target, args, _comCulture)
            Return True
        Catch ex As Exception
            OfficeHostLog.Write($"Method {name} could not be called: {OfficeHostLog.Describe(ex)}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Releases one COM reference and clears the variable. Never throws: teardown runs on paths
    ''' that are already failing, and a release that fails must not hide the original problem.
    ''' </summary>
    Public Shared Sub Release(ByRef instance As Object)
        Try
            If instance Is Nothing Then Return
            If Marshal.IsComObject(instance) Then Marshal.FinalReleaseComObject(instance)
        Catch ex As Exception
            OfficeHostLog.Write("Releasing a COM reference failed: " & OfficeHostLog.Describe(ex))
        Finally
            instance = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' An integer read out of a boxed COM value. Office hands window handles back as VT_I4 or
    ''' VT_I8 depending on the member and the bitness; both have to arrive as the same number.
    ''' </summary>
    Public Shared Function AsInt64(value As Object) As Long
        Try
            If value Is Nothing Then Return 0L
            Return Convert.ToInt64(value, CultureInfo.InvariantCulture)
        Catch ex As Exception
            OfficeHostLog.Write("A COM value could not be converted to a number: " & OfficeHostLog.Describe(ex))
            Return 0L
        End Try
    End Function

End Class
