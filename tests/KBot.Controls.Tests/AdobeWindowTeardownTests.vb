Option Strict On
Imports System.Collections.Generic
Imports Xunit
Imports KBot.Controls

''' <summary>
''' The teardown half of slice 0024-03 — the fix for «opening a second document leaves the first
''' Adobe window alive, with a taskbar button».
'''
''' The most important assertions in this file are NEGATIVE. The defect was not a missing call; it
''' was two calls that should never have been there: putting the original window style back, and
''' re-parenting the window to the desktop. Together those re-create a top-level window, and a
''' top-level window is a taskbar button.
''' </summary>
Public Class AdobeWindowTeardownTests

    Private Shared Function Ours(ParamArray pids As Integer()) As ISet(Of Integer)
        Return New HashSet(Of Integer)(pids)
    End Function

    <Fact>
    Public Sub Teardown_NeverRestoresTheStyle_AndNeverReparents()
        ' §9 test 8 — the exact defect. Whichever mode runs, neither call may appear.
        For Each mode As AdobeDetachMode In New AdobeDetachMode() {
            AdobeDetachMode.KillProcess, AdobeDetachMode.CloseWindow}

            Dim win As New FakeNativeWindows()
            Dim w = win.Add(handle:=30, pid:=1001)
            w.Parent = New IntPtr(9000)          ' currently hosted in our panel
            Dim launcher As New FakeAdobeLauncher()
            Dim teardown As New AdobeWindowTeardown(win, launcher)

            teardown.Run(New IntPtr(30), 1001, Ours(1001), mode, graceMs:=50)

            Assert.False(win.Called("SetParent"),
                         $"modul {mode} a re-parentat fereastra — exact defectul reparat în 0024-03")
            Assert.False(win.Called("SetWindowLongPtr"),
                         $"modul {mode} a restaurat stilul original — fereastra redevine top-level")
        Next
    End Sub

    <Fact>
    Public Sub ModeA_KillsTheProcessWeStarted()
        Dim win As New FakeNativeWindows()
        win.Add(handle:=31, pid:=1001)
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        Dim outcome = teardown.Run(New IntPtr(31), 1001, Ours(1001),
                                   AdobeDetachMode.KillProcess, graceMs:=50)

        Assert.Equal(AdobeTeardownAction.Killed, outcome.Action)
        Assert.Equal(New Integer() {1001}, launcher.Killed.ToArray())
    End Sub

    <Fact>
    Public Sub ModeA_NeverKillsAProcessWeDidNotStart()
        ' §9 test 6. The modern profile launches without «/n», so the embedded window can belong to
        ' the operator's own Adobe. Killing it would close their unrelated documents.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=32, pid:=7777)
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        ' We launched 1001; the window we ended up hosting belongs to 7777.
        Dim outcome = teardown.Run(New IntPtr(32), 7777, Ours(1001),
                                   AdobeDetachMode.KillProcess, graceMs:=50)

        Assert.Empty(launcher.Killed)
        Assert.Equal(AdobeTeardownAction.ForeignClosedInstead, outcome.Action)
        ' It degraded to closing the window only — and said so.
        Assert.True(win.Called("PostMessage(32,16)"))          ' WM_CLOSE
        Assert.Contains("nu l-am pornit noi", outcome.Message)
    End Sub

    <Fact>
    Public Sub ModeB_ClosesTheWindowAndLeavesTheProcessRunning()
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=33, pid:=1001)
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        Dim outcome = teardown.Run(New IntPtr(33), 1001, Ours(1001),
                                   AdobeDetachMode.CloseWindow, graceMs:=200)

        Assert.Equal(AdobeTeardownAction.Closed, outcome.Action)
        Assert.False(w.Alive)
        Assert.Empty(launcher.Killed)      ' the whole point of mode B: the process stays warm
    End Sub

    <Fact>
    Public Sub ModeB_FallsBackToKillingWhenTheWindowOutlivesTheGrace()
        ' §9 test 7. A window that ignores WM_CLOSE must not be allowed to strand the panel.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=34, pid:=1001)
        w.ClosesOnRequest = False
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        Dim outcome = teardown.Run(New IntPtr(34), 1001, Ours(1001),
                                   AdobeDetachMode.CloseWindow, graceMs:=60)

        Assert.Equal(AdobeTeardownAction.ClosedThenKilled, outcome.Action)
        Assert.Equal(New Integer() {1001}, launcher.Killed.ToArray())
        Assert.True(w.Alive)               ' the fake process does not really die; the PID kill did
    End Sub

    <Fact>
    Public Sub ModeB_LeavesAStubbornForeignWindowAlone()
        ' Foreign AND it ignored WM_CLOSE: there is nothing further we are permitted to do.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=35, pid:=7777)
        w.ClosesOnRequest = False
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        Dim outcome = teardown.Run(New IntPtr(35), 7777, Ours(1001),
                                   AdobeDetachMode.CloseWindow, graceMs:=60)

        Assert.Equal(AdobeTeardownAction.ForeignLeftAlive, outcome.Action)
        Assert.Empty(launcher.Killed)
    End Sub

    <Fact>
    Public Sub Teardown_OnNothingHosted_DoesNothing()
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher()
        Dim teardown As New AdobeWindowTeardown(win, launcher)

        Dim outcome = teardown.Run(IntPtr.Zero, 0, Ours(), AdobeDetachMode.KillProcess, graceMs:=50)

        Assert.Equal(AdobeTeardownAction.None, outcome.Action)
        Assert.Empty(launcher.Killed)
        Assert.Empty(win.Calls)
    End Sub

End Class
