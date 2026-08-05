Option Strict On
Imports System.Diagnostics
Imports KBot.Common

''' <summary>
''' Finding Adobe, starting it, and ending it — behind an interface so the orchestration can be
''' tested without an Adobe on the machine (slice 0024-03).
'''
''' Processes are identified by PID, never by a <see cref="Process"/> object. That is not a style
''' choice: the shell-execute launch this code replaced handed back a <c>Process</c> that had
''' ALREADY EXITED, because Adobe is effectively single-instance and the second document is passed
''' to the running copy. A PID read from the window we actually embedded is the only identity that
''' survives that.
''' </summary>
Public Interface IAdobeLauncher

    ''' <summary>The Adobe executable, or Nothing when no Adobe product is installed.</summary>
    Function ResolvePath() As String

    ''' <summary>
    ''' Starts Adobe and returns its process id. Throws when the process cannot be started at all —
    ''' the caller turns that into a Romanian message, never a crash.
    ''' </summary>
    Function Start(exePath As String, arguments As String) As Integer

    ''' <summary>True when the process is gone (or was never there).</summary>
    Function HasExited(pid As Integer) As Boolean

    ''' <summary>Ends a process tree. Best-effort: a process that is already gone is not an error.</summary>
    Sub Kill(pid As Integer)

End Interface

''' <summary>The real launcher, over <see cref="Process"/> and <see cref="AdobeWindowHosting"/>.</summary>
Public NotInheritable Class ProcessAdobeLauncher
    Implements IAdobeLauncher

    Private Shared ReadOnly _instance As New ProcessAdobeLauncher()

    Public Shared ReadOnly Property Instance As IAdobeLauncher
        Get
            Return _instance
        End Get
    End Property

    Public Function ResolvePath() As String Implements IAdobeLauncher.ResolvePath
        Return AdobeWindowHosting.ResolveAdobePath()
    End Function

    ''' <summary>
    ''' UseShellExecute = False deliberately. Shell-executing the PDF asks the SHELL to open the
    ''' document, which hands it to whatever Adobe is already running and returns a process that has
    ''' already exited — so nothing can be tracked or closed afterwards. Starting the EXECUTABLE with
    ''' the document as an argument gives us a process we own.
    ''' </summary>
    Public Function Start(exePath As String, arguments As String) As Integer Implements IAdobeLauncher.Start
        Try
            Dim proc As Process = Process.Start(
                New ProcessStartInfo(exePath, arguments) With {.UseShellExecute = False})
            If proc Is Nothing Then Return 0
            Try
                Return proc.Id
            Finally
                ' The PID is the handle we keep; the Process object itself is not needed again.
                proc.Dispose()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ProcessAdobeLauncher.Start", ex)
            Throw
        End Try
    End Function

    Public Function HasExited(pid As Integer) As Boolean Implements IAdobeLauncher.HasExited
        If pid <= 0 Then Return True
        Try
            Dim p As Process = Process.GetProcessById(pid)
            Try
                Return p.HasExited
            Finally
                p.Dispose()
            End Try
        Catch
            ' GetProcessById throws when there is no such process — which is precisely "has exited".
            Return True
        End Try
    End Function

    Public Sub Kill(pid As Integer) Implements IAdobeLauncher.Kill
        AdobeWindowHosting.KillPid(pid)
    End Sub

End Class
