Option Strict On
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports KBot.Common

''' <summary>
''' Finds the AcroPDF ActiveX control without hardcoding anything.
'''
''' THE GUID IS READ AT RUNTIME, from <c>HKCR\AcroPDF.PDF.1\CLSID</c>. A GUID pasted from a web page
''' is a fact about SOMEBODY ELSE's machine; Adobe has shipped more than one, and a control that
''' silently fails to create is indistinguishable from one that renders a blank page. If the ProgID
''' is not registered here, that is itself the answer the bench is meant to record.
''' </summary>
Friend NotInheritable Class AcroPdfDetector

    Private Sub New()
    End Sub

    ''' <summary>The ProgID Adobe registers for the browser control.</summary>
    Public Const ProgId As String = "AcroPDF.PDF.1"

    ''' <summary>The CLSID as a string, or Nothing when the control is not registered.</summary>
    Public Shared Function ResolveClsid() As String
        Try
            Using root As RegistryKey = Registry.ClassesRoot.OpenSubKey(ProgId & "\CLSID")
                If root Is Nothing Then Return Nothing
                Dim value As String = TryCast(root.GetValue(Nothing), String)
                If String.IsNullOrWhiteSpace(value) Then Return Nothing
                Return value.Trim()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfDetector.ResolveClsid", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Strips the braces AxHost does not want. AxHost takes the GUID WITHOUT braces; passing the
    ''' registry form verbatim throws at construction.
    ''' </summary>
    Public Shared Function NormaliseClsid(clsid As String) As String
        If String.IsNullOrWhiteSpace(clsid) Then Return Nothing
        Return clsid.Trim().Trim("{"c, "}"c)
    End Function

End Class

''' <summary>
''' A minimal <see cref="AxHost"/> over whatever CLSID it is handed.
'''
''' NO <c>aximp</c>, NO generated interop assembly, NO COM reference in the .vbproj — the whole point
''' of this evaluation is to learn whether the control renders an XFA document at all, and adding a
''' build-time dependency to answer that would be paying for the answer before hearing it.
''' Consequently every call into the control goes through reflection: <c>Option Strict On</c> forbids
''' late binding, so <c>CallByName</c> is not available and <see cref="Type.InvokeMember"/> is.
''' </summary>
Friend NotInheritable Class AcroPdfHost
    Inherits AxHost

    Public Sub New(clsidWithoutBraces As String)
        MyBase.New(clsidWithoutBraces)
    End Sub

    ''' <summary>
    ''' Calls <c>LoadFile(path)</c> on the underlying control. Returns the control's own answer when
    ''' it gives one; False plus a logged exception otherwise.
    ''' </summary>
    Public Function LoadFile(pdfPath As String) As Boolean
        Dim ocx As Object = GetOcx()
        If ocx Is Nothing Then Throw New InvalidOperationException(
            "Controlul AcroPDF nu a fost creat (GetOcx a întors Nothing).")
        Dim result As Object = ocx.GetType().InvokeMember(
            "LoadFile", BindingFlags.InvokeMethod, Nothing, ocx, New Object() {pdfPath})
        ' LoadFile is documented as returning a boolean, but a control that returns nothing at all
        ' must not be reported as a failure it did not report.
        If TypeOf result Is Boolean Then Return CBool(result)
        Return True
    End Function

    ''' <summary>
    ''' Turns off every piece of Adobe chrome the control exposes a method for, and reports what
    ''' each call did.
    '''
    ''' WHY THIS MATTERS MORE THAN IT LOOKS. Hiding Adobe's toolbars is the problem slice 0023 spent
    ''' five passes on and never solved cleanly — clip geometry, hiding child windows by text,
    ''' HKCU preferences, HKLM policies, keyboard toggles. The ActiveX control has a DOCUMENTED API
    ''' for the same thing. If these four calls work, they replace that entire apparatus.
    '''
    ''' Each call is made and reported INDEPENDENTLY, because the interesting answer is *which* ones
    ''' this Adobe build honours — a single pass/fail would hide that. A member the control does not
    ''' expose is a fact to record, not an error.
    ''' </summary>
    Public Function ApplyChrome() As List(Of String)
        Dim results As New List(Of String)()
        Dim ocx As Object = GetOcx()
        If ocx Is Nothing Then
            results.Add("AcroPDF: controlul nu e creat — nu pot aplica setările de chrome.")
            Return results
        End If

        ' Adobe's own names and argument shapes. Order follows their documentation.
        results.Add(CallMember(ocx, "setShowToolbar", False))
        results.Add(CallMember(ocx, "setShowScrollbars", False))
        results.Add(CallMember(ocx, "setPageMode", "none"))
        results.Add(CallMember(ocx, "setLayoutMode", "SinglePage"))
        results.Add(CallMember(ocx, "setView", "Fit"))
        Return results
    End Function

    ' One reflection call, reduced to one log line. Never throws: a member this build does not have
    ' is exactly the kind of thing the bench exists to discover.
    Private Shared Function CallMember(ocx As Object, memberName As String, arg As Object) As String
        Try
            ocx.GetType().InvokeMember(memberName, BindingFlags.InvokeMethod, Nothing, ocx,
                                       New Object() {arg})
            Return $"  {memberName}({arg}) — OK"
        Catch ex As MissingMethodException
            Return $"  {memberName}({arg}) — NU EXISTĂ pe acest build"
        Catch ex As Exception
            ' TargetInvocationException wraps whatever the control itself threw; the inner message is
            ' the informative one.
            Dim detail As String = If(ex.InnerException Is Nothing, ex.Message, ex.InnerException.Message)
            Return $"  {memberName}({arg}) — EȘEC: {detail}"
        End Try
    End Function

    ''' <summary>Calls <c>src = ""</c> / <c>LoadFile("")</c> to blank the control, best-effort.</summary>
    Public Sub Clear()
        Dim ocx As Object = GetOcx()
        If ocx Is Nothing Then Return
        ocx.GetType().InvokeMember("src", BindingFlags.SetProperty, Nothing, ocx, New Object() {""})
    End Sub

    ''' <summary>
    ''' The control's version string, when it exposes one. Returns Nothing rather than throwing —
    ''' a control without the property is a fact to log, not an error.
    ''' </summary>
    Public Function TryReadVersion() As String
        Try
            Dim ocx As Object = GetOcx()
            If ocx Is Nothing Then Return Nothing
            Dim v As Object = ocx.GetType().InvokeMember(
                "GetVersions", BindingFlags.InvokeMethod, Nothing, ocx, Nothing)
            Return Convert.ToString(v)
        Catch
            ' No such member on this build — expected on some versions, not worth an error entry.
            Return Nothing
        End Try
    End Function

End Class
