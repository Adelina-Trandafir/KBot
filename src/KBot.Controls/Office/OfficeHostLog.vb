Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text

''' <summary>
''' The WORKING log of the Office host: <c>&lt;AppDir&gt;\Logs\office_preview.log</c>.
'''
''' Separate from <see cref="KBot.Common.GlobalErrorLog"/>, which receives exceptions. What lands
''' here is what was DECIDED and why: which ProgID answered, how long the workbook took to open,
''' which window class chain was found, the requested rectangle against the one obtained, and how
''' the process was let go. The same reasoning as <see cref="AdobeHostLog"/>: a preview that breaks
''' after an Office update has to leave a trace someone can read.
'''
''' Terminal sink, like <c>GlobalErrorLog</c>: when even this file cannot be written the line goes
''' to <see cref="Trace"/> and nothing is thrown on.
''' </summary>
Public Module OfficeHostLog

    Private ReadOnly _gate As New Object()

    ''' <summary>The file name, next to the executable, under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "office_preview.log"

    ''' <summary>Writes one time-stamped line. Never throws.</summary>
    Public Sub Write(line As String)
        Try
            LogPaths.EnsureLogsDirectory()
            Dim filePath As String = LogPaths.Combine(FileNameOnly)
            SyncLock _gate
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath,
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & line & Environment.NewLine,
                                   New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            Trace.WriteLine("OfficeHostLog terminal failure: " & terminalEx.Message)
        End Try
    End Sub

    ''' <summary>
    ''' What an exception ACTUALLY says, on one line.
    '''
    ''' <para>Every call into Office goes out through <c>Type.InvokeMember</c>, and reflection wraps
    ''' whatever Office threw in a <see cref="Reflection.TargetInvocationException"/> whose own
    ''' message is the useless «Exception has been thrown by the target of an invocation». The thing
    ''' worth reading is underneath it: «COMException (0x800A03EC): Open method of Workbooks class
    ''' failed» names the member that failed and the HRESULT to look up. So the whole chain is walked
    ''' and every link printed, innermost last, with the HRESULT spelled out wherever there is one —
    ''' a COM error number is the difference between a diagnosis and a shrug.</para>
    ''' </summary>
    Public Function Describe(ex As Exception) As String
        If ex Is Nothing Then Return "<no exception>"
        Try
            Dim sb As New StringBuilder()
            Dim current As Exception = ex
            Dim depth As Integer = 0
            ' Bounded: a corrupt or circular chain must not spin here.
            Do While current IsNot Nothing AndAlso depth < 8
                If depth > 0 Then sb.Append(" -> ")
                sb.Append(current.GetType().Name).Append(": ").Append(current.Message)
                ' HResult is on Exception itself, so a plain COM failure surfaced as something else
                ' still gets its number. 0 and the generic COR_E_EXCEPTION say nothing; skip those.
                If current.HResult <> 0 AndAlso current.HResult <> &H80131500 Then
                    sb.Append(" [0x").Append(current.HResult.ToString("X8")).Append("]")
                End If
                current = current.InnerException
                depth += 1
            Loop
            Return sb.ToString()
        Catch describeEx As Exception
            ' A formatter that throws would hide the error it was asked to explain.
            Trace.WriteLine("OfficeHostLog.Describe failed: " & describeEx.Message)
            Return ex.Message
        End Try
    End Function

End Module
