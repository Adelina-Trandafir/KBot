Option Strict On
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Runtime.InteropServices

''' <summary>
''' Slice 0104-02 -- ends the process without running the unloading code of the libraries inside it.
''' </summary>
''' <remarks>
''' <para>
''' <b>Why.</b> The Office copy of the Access driver (<c>ACEOLEDB.DLL</c>, loaded as soon as a window reads or writes an
''' Access file) pulls in <c>mso98win32client.dll</c>. When the process exits normally, Windows unloads that library and
''' its own shutdown code reads freed memory (the destructor of its telemetry client): an access violation, reported as a
''' crash and answered by any post-mortem debugger registered on the PC. The stack is Office's alone -- no K-BOT frame
''' (cdb log of 03.10.2026, worklog of slice 0104).
''' </para>
''' <para>
''' <b>What it does.</b> <see cref="TerminateIfOfficeDriverLoaded"/> is the last thing <c>Program.Main</c> does: when the
''' driver is in the process, <c>TerminateProcess</c> ends it at once with exit code 0, so the library is never asked to
''' unload. Everything K-BOT owns has already been closed by then (windows, the service container, the logout). Without
''' the driver loaded nothing changes: the process exits the usual way.
''' </para>
''' </remarks>
Public NotInheritable Class FastExit

    ' The libraries whose presence means «the Access driver was used in this process».
    Private Shared ReadOnly OfficeDriverModules As String() = {"ACEOLEDB.DLL", "mso98win32client.dll"}

    <DllImport("kernel32.dll")>
    Private Shared Function GetCurrentProcess() As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function TerminateProcess(hProcess As IntPtr, uExitCode As UInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Sub New()
    End Sub

    ''' <summary>The Office Access driver (or the Office library it brings) is loaded in this process.</summary>
    Public Shared Function OfficeDriverLoaded() As Boolean
        Try
            Using current As Process = Process.GetCurrentProcess()
                For Each m As ProcessModule In current.Modules
                    For Each wanted As String In OfficeDriverModules
                        If String.Equals(m.ModuleName, wanted, StringComparison.OrdinalIgnoreCase) Then Return True
                    Next
                Next
            End Using
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("FastExit.OfficeDriverLoaded", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Ends this process now, exit code 0, with no unloading code run. Nothing after the call executes: the caller
    ''' must have closed everything it owns. Throws <see cref="Win32Exception"/> if Windows refuses.
    ''' </summary>
    Public Shared Sub TerminateNow()
        Try
            If Not TerminateProcess(GetCurrentProcess(), 0UI) Then
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("FastExit.TerminateNow", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The exit-path call: <see cref="TerminateNow"/> when <see cref="OfficeDriverLoaded"/>, otherwise nothing. Never
    ''' throws -- a failure is logged and the process exits the normal way (which may then be reported as a crash).
    ''' </summary>
    Public Shared Sub TerminateIfOfficeDriverLoaded()
        Try
            If OfficeDriverLoaded() Then TerminateNow()
        Catch ex As Exception
            ' Already logged by the two calls above; the normal exit follows.
            Debug.WriteLine("FastExit: " & ex.Message)
        End Try
    End Sub

End Class
