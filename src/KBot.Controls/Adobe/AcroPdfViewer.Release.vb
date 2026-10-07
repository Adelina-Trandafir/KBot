Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports KBot.Common

' ACTIVEX-CHECK (slice 0078-15): what is left of Adobe after the control is released (operator, 06.10.2026: two Adobe
' processes stayed in memory after the bench was closed). Before the control goes, Clear notes Adobe's windows inside it
' and Adobe's top-level windows; then a background thread checks them -- and lists Adobe's processes -- right after
' the release and again 1 / 3 / 10 / 30 s later. Background, so it neither blocks the UI nor stops when the bench closes
' (Dispose lets the log writer finish only after it). It dies with K-BOT itself: close only the bench, not K-BOT.
' Part of AcroPdfViewer. Kept for a future investigation (operator, 07.10.2026): used only while DetailedWatch is True.
Partial Public NotInheritable Class AcroPdfViewer

    Private Shared ReadOnly ReleaseSnapshotsMs As Integer() = {0, 1000, 3000, 10000, 30000}
    Private Shared ReadOnly AdobeProcessPrefixes As String() = {"Acro", "Adobe", "RdrCEF"}
    Private _releaseWatch As Thread

    ' What was there just before the release. The handles outlive the control, so they can be checked afterwards.
    Private NotInheritable Class ReleaseState
        Public Tag As String
        Public ControlHandle As IntPtr
        Public Inside As New List(Of IntPtr)()
        Public TopLevel As New List(Of IntPtr)()
    End Class

    Private Function BeforeRelease(k_host As AcroPdfHost) As ReleaseState
        Dim k_state As New ReleaseState With {.Tag = ViewerTag()}
        Dim k_pids As New HashSet(Of Integer)()
        If k_host.IsHandleCreated Then
            k_state.ControlHandle = k_host.Handle
            For Each k_h As IntPtr In AdobeNativeMethods.Descendants(k_host.Handle)
                Dim k_pid As Integer = AdobeNativeMethods.OwnerPid(k_h)
                If k_pid = Environment.ProcessId Then Continue For
                k_state.Inside.Add(k_h)
                k_pids.Add(k_pid)
            Next
        End If
        For Each k_h As IntPtr In _adobeTopWindows.Keys
            If AdobeNativeMethods.IsWindow(k_h) Then k_state.TopLevel.Add(k_h)
        Next
        Check($"RELEASE: about to dispose control {HexOf(k_state.ControlHandle)}: {k_state.Inside.Count} Adobe window(s) inside " &
              $"(pid {String.Join(", ", k_pids)}), {k_state.TopLevel.Count} Adobe top-level window(s) alive, form={DescribeForm()}")
        Return k_state
    End Function

    Private Sub StartReleaseWatch(k_state As ReleaseState)
        Dim k_clock As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim k_thread As New Thread(
            Sub()
                RunReleaseWatch(k_state, k_clock)
            End Sub) With {.IsBackground = True, .Name = "AcroPdfViewer release watch"}
        _releaseWatch = k_thread
        k_thread.Start()
    End Sub

    ' Background thread. Must not throw.
    Private Sub RunReleaseWatch(k_state As ReleaseState, k_clock As System.Diagnostics.Stopwatch)
        Try
            For Each k_ms As Integer In ReleaseSnapshotsMs
                Dim k_wait As Long = k_ms - k_clock.ElapsedMilliseconds
                If k_wait > 0 Then Thread.Sleep(CInt(k_wait))
                Check($"RELEASE +{k_clock.ElapsedMilliseconds} ms: control window " &
                      $"{If(AdobeNativeMethods.IsWindow(k_state.ControlHandle), "STILL ALIVE", "gone")}; " &
                      $"Adobe windows inside: {AliveCount(k_state.Inside)} of {k_state.Inside.Count} still alive; " &
                      $"Adobe top-level: {AliveCount(k_state.TopLevel)} of {k_state.TopLevel.Count} still alive; " &
                      $"processes: {AdobeProcesses()}", k_state.Tag)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.RunReleaseWatch", ex)
        End Try
    End Sub

    Private Shared Function AliveCount(k_handles As List(Of IntPtr)) As Integer
        Dim k_alive As Integer = 0
        For Each k_h As IntPtr In k_handles
            If AdobeNativeMethods.IsWindow(k_h) Then k_alive += 1
        Next
        Return k_alive
    End Function

    ' Every running Adobe process: name#pid (start time). Read-only; a process that ends while being read is skipped.
    Private Shared Function AdobeProcesses() As String
        Dim k_parts As New List(Of String)()
        For Each k_proc As Process In Process.GetProcesses()
            Try
                Dim k_name As String = k_proc.ProcessName
                Dim k_isAdobe As Boolean = False
                For Each k_prefix As String In AdobeProcessPrefixes
                    If k_name.StartsWith(k_prefix, StringComparison.OrdinalIgnoreCase) Then k_isAdobe = True
                Next
                If Not k_isAdobe Then Continue For
                Dim k_start As String = "?"
                Try
                    k_start = k_proc.StartTime.ToString("HH:mm:ss.fff")
                Catch ex As ComponentModel.Win32Exception
                    ' No access to that process: the name and pid are enough.
                End Try
                k_parts.Add($"{k_name}#{k_proc.Id} (started {k_start})")
            Catch ex As InvalidOperationException
                ' Ended while being read.
            Finally
                k_proc.Dispose()
            End Try
        Next
        Return If(k_parts.Count = 0, "none", String.Join(", ", k_parts))
    End Function

End Class
