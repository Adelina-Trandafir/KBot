Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports KBot.Common

''' <summary>What teardown actually did. Logged verbatim — a refusal must not read like a success.</summary>
Public Enum AdobeTeardownAction
    ''' <summary>There was nothing hosted.</summary>
    None = 0
    ''' <summary>Mode A: the process K-BOT started was ended.</summary>
    Killed = 1
    ''' <summary>Mode B: the window closed on request; the process was left running.</summary>
    Closed = 2
    ''' <summary>Mode B: the window outlived the grace period, so mode A finished the job.</summary>
    ClosedThenKilled = 3
    ''' <summary>
    ''' Mode A was asked for but the window belongs to a process K-BOT did not start, so only the
    ''' window was closed. Killing it would take the operator's other documents with it.
    ''' </summary>
    ForeignClosedInstead = 4
    ''' <summary>
    ''' The window belongs to a foreign process AND survived WM_CLOSE. Nothing further is permitted;
    ''' the window is still a child of the host panel and dies with it.
    ''' </summary>
    ForeignLeftAlive = 5
End Enum

''' <summary>The action plus the Romanian sentence for the log.</summary>
Public NotInheritable Class AdobeTeardownOutcome

    Public ReadOnly Property Action As AdobeTeardownAction
    Public ReadOnly Property Message As String

    Public Sub New(action As AdobeTeardownAction, message As String)
        Me.Action = action
        Me.Message = If(message, "")
    End Sub

End Class

''' <summary>
''' Lets a hosted Adobe window go, in one of the two modes of <see cref="AdobeDetachMode"/>.
'''
''' WHAT THIS CLASS EXISTS TO NOT DO (slice 0024-03 §0, symptom 1). The teardown it replaces called
''' <c>SetWindowLongPtr</c> to put the original style back — caption, frame, WS_POPUP and all — and
''' then <c>SetParent(hwnd, originalParent)</c> to hand the window back to the desktop. That is the
''' textbook way to CREATE a top-level window, and a top-level window is a taskbar button. The close
''' that was supposed to follow could not fire, because the shell-execute launch it relied on had
''' returned an already-exited process. Result: every second document left a live Adobe window
''' behind, with a taskbar entry, showing the previous PDF.
'''
''' So: no style restore, no re-parent, ever. There is deliberately no code path here that calls
''' either, and a test asserts their absence.
'''
''' A PROCESS IS ONLY EVER KILLED IF K-BOT STARTED IT. The modern profile launches without «/n», so
''' the embedded window can belong to an Adobe the operator opened themselves; ending that process
''' would close their unrelated documents. Mode A therefore degrades to mode B for a foreign window
''' rather than proceeding.
''' </summary>
Public NotInheritable Class AdobeWindowTeardown

    Private Const GracePollMs As Integer = 25

    Private ReadOnly _win As INativeWindows
    Private ReadOnly _launcher As IAdobeLauncher

    Public Sub New(Optional windows As INativeWindows = Nothing, Optional launcher As IAdobeLauncher = Nothing)
        _win = If(windows, Win32Windows.Instance)
        _launcher = If(launcher, ProcessAdobeLauncher.Instance)
    End Sub

    ''' <summary>
    ''' Drops the window. <paramref name="launchedPids"/> is the set of process ids THIS host
    ''' started; a PID outside it is never killed, in the bench or in the app.
    ''' </summary>
    Public Function Run(hwnd As IntPtr, hostedPid As Integer, launchedPids As ISet(Of Integer),
                        mode As AdobeDetachMode, graceMs As Integer) As AdobeTeardownOutcome
        Try
            If hwnd = IntPtr.Zero AndAlso hostedPid <= 0 Then
                Return New AdobeTeardownOutcome(AdobeTeardownAction.None, "")
            End If

            Dim ours As Boolean = hostedPid > 0 AndAlso
                                  launchedPids IsNot Nothing AndAlso
                                  launchedPids.Contains(hostedPid)

            If mode = AdobeDetachMode.KillProcess Then
                If ours Then
                    _launcher.Kill(hostedPid)
                    Return New AdobeTeardownOutcome(
                        AdobeTeardownAction.Killed,
                        $"Închidere (A): am oprit procesul {hostedPid}, pornit de K-BOT.")
                End If

                ' Refusal, stated as a refusal.
                Dim degraded As AdobeTeardownOutcome = CloseWindowOnly(hwnd, hostedPid, ours:=False, graceMs:=graceMs)
                Dim reason As String =
                    $"Închidere (A) refuzată: fereastra aparține procesului {hostedPid}, pe care nu " &
                    "l-am pornit noi — închiderea lui i-ar lua operatorului și celelalte documente. "
                If degraded.Action = AdobeTeardownAction.Closed Then
                    Return New AdobeTeardownOutcome(AdobeTeardownAction.ForeignClosedInstead,
                                                    reason & "Am închis doar fereastra.")
                End If
                Return New AdobeTeardownOutcome(AdobeTeardownAction.ForeignLeftAlive,
                                                reason & "Fereastra nu s-a închis la cerere; rămâne copil " &
                                                "al panoului și dispare odată cu el.")
            End If

            Return CloseWindowOnly(hwnd, hostedPid, ours, graceMs)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowTeardown.Run", ex)
            Throw
        End Try
    End Function

    ' Mode B. Posts WM_CLOSE (never sends: a busy foreign UI thread must not block ours), waits for
    ' the handle to die, and escalates to mode A only for a process we started.
    Private Function CloseWindowOnly(hwnd As IntPtr, hostedPid As Integer, ours As Boolean,
                                     graceMs As Integer) As AdobeTeardownOutcome
        If hwnd <> IntPtr.Zero AndAlso _win.IsWindow(hwnd) Then
            _win.PostMessage(hwnd, AdobeNativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero)
        End If

        If WaitForWindowToDie(hwnd, graceMs) Then
            Return New AdobeTeardownOutcome(
                AdobeTeardownAction.Closed,
                $"Închidere (B): fereastra s-a închis; procesul {hostedPid} rămâne pornit.")
        End If

        If ours Then
            _launcher.Kill(hostedPid)
            Return New AdobeTeardownOutcome(
                AdobeTeardownAction.ClosedThenKilled,
                $"Închidere (B): fereastra nu s-a închis în {graceMs} ms — am oprit procesul {hostedPid}.")
        End If

        Return New AdobeTeardownOutcome(
            AdobeTeardownAction.ForeignLeftAlive,
            $"Închidere (B): fereastra nu s-a închis în {graceMs} ms și procesul {hostedPid} nu e al " &
            "nostru — nu îl opresc.")
    End Function

    ' True when the window is gone. Blocking, but only ever on the mode-B path: mode A (the shipped
    ' default) kills and returns without waiting, so the normal document change never pays this.
    Private Function WaitForWindowToDie(hwnd As IntPtr, graceMs As Integer) As Boolean
        If hwnd = IntPtr.Zero Then Return True
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(0, graceMs))
        Do
            If Not _win.IsWindow(hwnd) Then Return True
            If DateTime.UtcNow >= deadline Then Return False
            Thread.Sleep(GracePollMs)
        Loop
    End Function

End Class
