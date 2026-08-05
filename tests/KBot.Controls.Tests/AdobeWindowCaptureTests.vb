Option Strict On
Imports Xunit
Imports KBot.Controls

''' <summary>
''' The capture half of slice 0024-03 — the fix for «the Adobe window flashes on screen before it is
''' placed», and for «the search can grab a window the operator opened by hand».
''' </summary>
Public Class AdobeWindowCaptureTests

    ' The six Win32 style bits a hosted window must lose, spelled out here rather than read from the
    ' production constant — a test that imports the value it is checking proves nothing.
    Private Const WS_CHILD As Long = &H40000000L
    Private Const WS_POPUP As Long = &H80000000L
    Private Const WS_CAPTION As Long = &HC00000L
    Private Const WS_THICKFRAME As Long = &H40000L
    Private Const WS_SYSMENU As Long = &H80000L
    Private Const WS_MINIMIZEBOX As Long = &H20000L
    Private Const WS_MAXIMIZEBOX As Long = &H10000L
    Private Const WS_VISIBLE As Long = &H10000000L

    Private Shared Function FastOptions() As AdobeHostOptions
        ' Short enough that a miss fails the test in milliseconds instead of eight seconds.
        Return New AdobeHostOptions() With {.FindTimeoutMs = 300, .FindPollMs = 1}
    End Function

    <Fact>
    Public Sub ChildStyle_ClearsAllSixStandaloneBits_AndSetsChild()
        ' §9 test 2. A window carrying every standalone bit plus something unrelated (WS_VISIBLE)
        ' must come back with the six cleared, WS_CHILD set, and the unrelated bit untouched.
        Dim original As Long = WS_CAPTION Or WS_THICKFRAME Or WS_POPUP Or
                               WS_MINIMIZEBOX Or WS_MAXIMIZEBOX Or WS_SYSMENU Or WS_VISIBLE

        Dim result As Long = AdobeWindowCapture.ChildStyle(original)

        Assert.Equal(0L, result And WS_CAPTION)
        Assert.Equal(0L, result And WS_THICKFRAME)
        Assert.Equal(0L, result And WS_POPUP)
        Assert.Equal(0L, result And WS_MINIMIZEBOX)
        Assert.Equal(0L, result And WS_MAXIMIZEBOX)
        Assert.Equal(0L, result And WS_SYSMENU)
        Assert.Equal(WS_CHILD, result And WS_CHILD)
        ' Bits that are none of our business survive.
        Assert.Equal(WS_VISIBLE, result And WS_VISIBLE)
    End Sub

    <Fact>
    Public Sub Find_TakesAForeignWindow_BecauseAdobeHandsTheDocumentToAnExistingInstance()
        ' THE REGRESSION THIS FILE SHIPPED ONCE, and the reason this test exists.
        '
        ' Capture used to refuse a window belonging to a process K-BOT had not started, unless the
        ' launch profile omitted «/n». The operator's own adobe_preview.log shows that gate is
        ' simply wrong: EVERY launch logs «fereastra încorporată (PID 25168) NU a fost creată de
        ' K-BOT (am pornit PID 27152)». Adobe is effectively single-instance and hands the document
        ' to a running copy even when «/n» is passed. With the gate on, capture found nothing,
        ' reported «window not found», and left the real Adobe window on screen — floating, with a
        ' taskbar button. Exactly what the operator saw.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=13, pid:=7777, title:="raport.pdf - Adobe Acrobat")   ' a running instance
        Dim capture As New AdobeWindowCapture(win)

        ' We started 1001 and it owns nothing. The window must still be taken.
        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.True(result.Found, "fereastra străină TREBUIE luată — altfel rămâne plutind în taskbar")
        Assert.Equal(New IntPtr(13), result.Window)
        ' Reported as foreign, which is what stops teardown from killing someone else's process.
        Assert.Equal(AdobeCaptureMatch.ByTitle, result.Match)
        Assert.Equal(7777, result.OwnerPid)
    End Sub

    <Fact>
    Public Sub Find_IgnoresAWindowOfAnotherDocument_EvenInOurOwnProcess()
        ' The title is what identifies the window. A different document open in the same Adobe must
        ' not be grabbed.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=10, pid:=1001, title:="altceva.pdf - Adobe Acrobat")
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.False(result.Found)
        Assert.Equal(AdobeCaptureMatch.None, result.Match)
    End Sub

    <Fact>
    Public Sub Find_IgnoresAHelperWindowOfOurOwnProcess()
        ' Adobe creates several top-level windows in one process sharing the «Acrobat» class prefix,
        ' and EnumWindows returns them in Z-order. Matching on PID + class alone therefore grabbed a
        ' helper window, hid it, reparented it — and left the real document window floating. The
        ' document name in the title is what tells them apart.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=16, pid:=1001, title:="", visible:=False)              ' helper, listed FIRST
        win.Add(handle:=17, pid:=1001, title:="raport.pdf - Adobe Acrobat")    ' the real frame
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.Equal(New IntPtr(17), result.Window)
    End Sub

    <Fact>
    Public Sub Find_AcceptsAWindowThatIsNotVisibleYet()
        ' §9 test 4 — the flash fix. The original search skipped any window failing IsWindowVisible,
        ' which meant it could only ever match a window ALREADY DRAWN on screen, caption and all.
        ' Catching it while still invisible is the point; the title, not the visibility, identifies it.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=11, pid:=1001, title:="raport.pdf", visible:=False)
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.True(result.Found)
        Assert.Equal(New IntPtr(11), result.Window)
        Assert.Equal(AdobeCaptureMatch.ByPid, result.Match)
    End Sub

    <Fact>
    Public Sub Find_HidesTheWindowTheInstantItMatches()
        ' Hiding must happen inside the search, before the caller is even told the window exists —
        ' every millisecond between «it exists» and «it is hidden» is a millisecond it can be seen.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=12, pid:=1001, title:="raport.pdf", visible:=True)
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.True(result.Found)
        Assert.False(w.Visible)
        Assert.True(win.Called("ShowWindow(12,0)"))     ' SW_HIDE
    End Sub

    <Fact>
    Public Sub Find_PrefersOurOwnWindowOverAForeignOneWithTheSameTitle()
        ' Both can match at once. Ours must win, or a second K-BOT document would embed the
        ' operator's window while our own sat there unused.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=20, pid:=7777, title:="raport.pdf - Adobe Acrobat")   ' theirs, listed first
        win.Add(handle:=21, pid:=1001, title:="raport.pdf - Adobe Acrobat")   ' ours
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.Equal(New IntPtr(21), result.Window)
        Assert.Equal(AdobeCaptureMatch.ByPid, result.Match)
    End Sub

    <Fact>
    Public Sub Find_KeepsPollingUntilTheWindowTurnsUp()
        ' Adobe does not have a window the instant Process.Start returns.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=14, pid:=1001, title:="raport.pdf")
        w.AppearsAfterSweeps = 3
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.True(result.Found)
        Assert.Equal(New IntPtr(14), result.Window)
    End Sub

    <Fact>
    Public Sub Find_RejectsANonAdobeWindowEvenWithAMatchingTitle()
        Dim win As New FakeNativeWindows()
        win.Add(handle:=15, pid:=1001, title:="raport.pdf", className:="SplashScreenClass")
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport", options:=FastOptions())

        Assert.False(result.Found)
    End Sub

    <Fact>
    Public Sub AttachAsChild_SetsTheChildStyleAndReparents_ReturningTheOriginal()
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=16, pid:=1001)
        w.Style = WS_CAPTION Or WS_POPUP Or WS_SYSMENU
        Dim capture As New AdobeWindowCapture(win)

        Dim original As IntPtr = capture.AttachAsChild(New IntPtr(16), New IntPtr(9000))

        Assert.Equal(WS_CAPTION Or WS_POPUP Or WS_SYSMENU, original.ToInt64())
        Assert.Equal(WS_CHILD, w.Style And WS_CHILD)
        Assert.Equal(0L, w.Style And WS_CAPTION)
        Assert.Equal(New IntPtr(9000), w.Parent)
    End Sub

End Class
