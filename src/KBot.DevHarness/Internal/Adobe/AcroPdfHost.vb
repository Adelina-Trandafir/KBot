Option Strict On
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
