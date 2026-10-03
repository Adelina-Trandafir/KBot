Option Strict On
Imports System.Diagnostics
Imports System.Threading
Imports KBot.Common

''' <summary>
''' Slice 0078-11: the Adobe processes K-BOT brought into being, remembered for the day the
''' application closes. Acrobat serves an embedded document with a BROKER (started by K-BOT) and
''' RENDERER children; when K-BOT ends they were left in the Task Manager as ghosts.
'''
''' A process is "ours" only when it is named like Adobe AND its parent is K-BOT or a process
''' already known to be ours - an Acrobat the operator opened by hand is never touched. Each one
''' is remembered with its start time, so a process id reused later by something else is not
''' mistaken for it.
'''
''' <see cref="Shutdown"/> (called once, after the shell has closed): a final sweep, WM_CLOSE to
''' the windows of every process of ours, a short wait, then the direct kill of the tree for those
''' that did not leave (a ghost has no window to answer WM_CLOSE).
''' </summary>
Public NotInheritable Class AdobeProcessRegistry

    ''' <summary>How long the WM_CLOSE gets before the kill (ms).</summary>
    Private Const GraceMs As Integer = 1500
    Private Const PollMs As Integer = 100

    Private Shared ReadOnly _lock As New Object()
    ' pid -> start time (ticks) of the process as it was when remembered.
    Private Shared ReadOnly _tracked As New Dictionary(Of Integer, Long)()

    Private Sub New()
    End Sub

    ''' <summary>Remembers one process (one K-BOT started itself). Gone or unreadable = nothing to remember.</summary>
    Public Shared Sub Track(k_pid As Integer)
        If k_pid <= 0 OrElse k_pid = Environment.ProcessId Then Return
        Try
            Using k_proc As Process = Process.GetProcessById(k_pid)
                Dim k_start As Long = k_proc.StartTime.Ticks
                SyncLock _lock
                    _tracked(k_pid) = k_start
                End SyncLock
            End Using
        Catch ex As ArgumentException
            ' No such process any more: expected, nothing to remember.
        Catch ex As InvalidOperationException
            ' It exited while being read: same.
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProcessRegistry.Track", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Remembers every process of <paramref name="k_parents"/> (pid -> parent pid, Adobe-named ones
    ''' only) that descends from K-BOT or from a process already remembered.
    ''' </summary>
    Public Shared Sub TrackFamily(k_parents As IDictionary(Of Integer, Integer))
        Try
            If k_parents Is Nothing OrElse k_parents.Count = 0 Then Return
            Dim k_self As Integer = Environment.ProcessId
            Dim k_known As HashSet(Of Integer)
            SyncLock _lock
                k_known = New HashSet(Of Integer)(_tracked.Keys)
            End SyncLock
            Dim k_fresh As New List(Of Integer)()
            Dim k_grew As Boolean = True
            While k_grew
                k_grew = False
                For Each k_kv As KeyValuePair(Of Integer, Integer) In k_parents
                    If k_known.Contains(k_kv.Key) Then Continue For
                    If k_kv.Value = k_self OrElse k_known.Contains(k_kv.Value) Then
                        k_known.Add(k_kv.Key)
                        k_fresh.Add(k_kv.Key)
                        k_grew = True
                    End If
                Next
            End While
            For Each k_pid As Integer In k_fresh
                Track(k_pid)
            Next
        Catch ex As Exception
            ' Reached from the save trap's timer: a failed sweep only means "nothing new this time".
            GlobalErrorLog.Write("AdobeProcessRegistry.TrackFamily", ex)
        End Try
    End Sub

    ''' <summary>Looks at every Adobe process running now and remembers the ones that descend from K-BOT.</summary>
    Public Shared Sub Sweep()
        Try
            TrackFamily(AdobeParents())
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProcessRegistry.Sweep", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Ends what K-BOT started. Never throws (it runs while the application is going away).
    ''' Said in <c>adobe_preview.log</c>: what was found, what answered WM_CLOSE, what was killed, and
    ''' the Adobe processes left alone with their parent - the evidence for the next ghost.
    ''' </summary>
    Public Shared Sub Shutdown()
        Try
            Sweep()
            Dim k_snapshot As KeyValuePair(Of Integer, Long)()
            SyncLock _lock
                k_snapshot = _tracked.ToArray()
            End SyncLock

            Dim k_alive As New HashSet(Of Integer)()
            For Each k_kv As KeyValuePair(Of Integer, Long) In k_snapshot
                If IsSameProcessAlive(k_kv.Key, k_kv.Value) Then k_alive.Add(k_kv.Key)
            Next
            LogLeftAlone(k_alive)
            If k_alive.Count = 0 Then
                AdobeHostLog.Write("Shutdown: no Adobe process of K-BOT is left.")
                Return
            End If
            AdobeHostLog.Write($"Shutdown: {k_alive.Count} Adobe process(es) of K-BOT [{String.Join(", ", k_alive)}] - WM_CLOSE first.")

            PostCloseToWindowsOf(k_alive)
            Dim k_waited As Integer = 0
            While k_waited < GraceMs AndAlso k_snapshot.Any(Function(k_e) k_alive.Contains(k_e.Key) AndAlso IsSameProcessAlive(k_e.Key, k_e.Value))
                Thread.Sleep(PollMs)
                k_waited += PollMs
            End While

            For Each k_kv As KeyValuePair(Of Integer, Long) In k_snapshot
                If Not k_alive.Contains(k_kv.Key) Then Continue For
                If Not IsSameProcessAlive(k_kv.Key, k_kv.Value) Then
                    AdobeHostLog.Write($"Shutdown: {k_kv.Key} left by itself.")
                    Continue For
                End If
                KillTree(k_kv.Key)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProcessRegistry.Shutdown", ex)
        End Try
    End Sub

    ' ── helpers (all reached through Shutdown / Sweep, which are wrapped) ──────────

    ''' <summary>pid -> parent pid of every Adobe-named process running now.</summary>
    Private Shared Function AdobeParents() As Dictionary(Of Integer, Integer)
        Dim k_parents As New Dictionary(Of Integer, Integer)()
        For Each k_name As String In AdobeWindowHosting.ProcessNames
            For Each k_proc As Process In Process.GetProcessesByName(k_name)
                Using k_proc
                    k_parents(k_proc.Id) = AdobeNativeMethods.ParentPid(k_proc.Id)
                End Using
            Next
        Next
        Return k_parents
    End Function

    ''' <summary>True when the process is still running AND is the one remembered (same start time).</summary>
    Private Shared Function IsSameProcessAlive(k_pid As Integer, k_startTicks As Long) As Boolean
        Try
            Using k_proc As Process = Process.GetProcessById(k_pid)
                Return Not k_proc.HasExited AndAlso k_proc.StartTime.Ticks = k_startTicks
            End Using
        Catch ex As ArgumentException
            Return False
        Catch ex As InvalidOperationException
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProcessRegistry.IsSameProcessAlive", ex)
            Return False
        End Try
    End Function

    ''' <summary>Posts (never sends) WM_CLOSE to every top-level window owned by one of the processes.</summary>
    Private Shared Sub PostCloseToWindowsOf(k_pids As HashSet(Of Integer))
        Dim k_windows As New List(Of IntPtr)()
        ' The callback cannot throw across the interop boundary, so it only ever appends.
        AdobeNativeMethods.EnumWindows(
            Function(k_h, k_l)
                If k_pids.Contains(AdobeNativeMethods.OwnerPid(k_h)) Then k_windows.Add(k_h)
                Return True
            End Function, IntPtr.Zero)
        For Each k_h As IntPtr In k_windows
            AdobeNativeMethods.PostMessage(k_h, AdobeNativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero)
        Next
        AdobeHostLog.Write($"Shutdown: WM_CLOSE posted to {k_windows.Count} window(s).")
    End Sub

    Private Shared Sub KillTree(k_pid As Integer)
        Try
            Using k_proc As Process = Process.GetProcessById(k_pid)
                If k_proc.HasExited Then Return
                k_proc.Kill(True)
                AdobeHostLog.Write($"Shutdown: {k_pid} did not leave - killed with its tree.")
            End Using
        Catch ex As ArgumentException
            ' Gone between the check and the kill: what was wanted.
        Catch ex As InvalidOperationException
            ' Same.
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProcessRegistry.KillTree", ex)
        End Try
    End Sub

    ''' <summary>The Adobe processes NOT ours, with their parent: left alone, but written down.</summary>
    Private Shared Sub LogLeftAlone(k_ours As HashSet(Of Integer))
        Dim k_others As New List(Of String)()
        For Each k_kv As KeyValuePair(Of Integer, Integer) In AdobeParents()
            If Not k_ours.Contains(k_kv.Key) Then k_others.Add($"{k_kv.Key}<-parent {k_kv.Value}")
        Next
        If k_others.Count > 0 Then
            AdobeHostLog.Write($"Shutdown: Adobe process(es) not started by K-BOT, left alone: {String.Join(", ", k_others)}")
        End If
    End Sub

End Class
