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
    Public Sub Find_IgnoresAWindowOfAnotherProcess_EvenWhenClassAndTitleMatch()
        ' §9 test 3. THE REGRESSION THIS PREVENTS: the old search matched on class + title only, so
        ' an Adobe the operator had open on the same document would be embedded into K-BOT's panel —
        ' taking their window away from them.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=10, pid:=4242, title:="raport.pdf - Adobe Acrobat")   ' theirs
        Dim capture As New AdobeWindowCapture(win)

        ' allowForeignTitleMatch:=False — this is the «/n» case, where our PID must own the window.
        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=False, options:=FastOptions())

        Assert.False(result.Found)
        Assert.Equal(AdobeCaptureMatch.None, result.Match)
    End Sub

    <Fact>
    Public Sub Find_AcceptsAWindowThatIsNotVisibleYet()
        ' §9 test 4 — the flash regression itself. The previous search skipped any window failing
        ' IsWindowVisible, which meant it could only ever match a window ALREADY DRAWN on screen,
        ' caption and all. Catching it while invisible is the entire fix.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=11, pid:=1001, title:="", visible:=False)
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=False, options:=FastOptions())

        Assert.True(result.Found)
        Assert.Equal(New IntPtr(11), result.Window)
        Assert.Equal(AdobeCaptureMatch.ByPid, result.Match)
    End Sub

    <Fact>
    Public Sub Find_HidesTheWindowTheInstantItMatches()
        ' Hiding must happen inside the search, before the caller is even told the window exists —
        ' every millisecond between «it exists» and «it is hidden» is a millisecond it can be seen.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=12, pid:=1001, visible:=True)
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=False, options:=FastOptions())

        Assert.True(result.Found)
        Assert.False(w.Visible)
        Assert.True(win.Called("ShowWindow(12,0)"))     ' SW_HIDE
    End Sub

    <Fact>
    Public Sub Find_MatchesByTitleInAForeignProcess_OnlyWhenTheProfileOmittedNewInstance()
        ' The modern profile launches WITHOUT «/n», so Adobe hands the document to an instance the
        ' operator already had open and OUR pid owns nothing. That case must still work — and must
        ' be REPORTED as a title match, so the caller knows never to kill that process.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=13, pid:=7777, title:="raport.pdf - Adobe Acrobat")
        Dim capture As New AdobeWindowCapture(win)

        Dim allowed = capture.Find(launchedPid:=1001, baseName:="raport",
                                   allowForeignTitleMatch:=True, options:=FastOptions())
        Assert.True(allowed.Found)
        Assert.Equal(AdobeCaptureMatch.ByTitle, allowed.Match)
        Assert.Equal(7777, allowed.OwnerPid)

        ' Same desktop, «/n» on: the foreign window must stay untouched.
        Dim win2 As New FakeNativeWindows()
        win2.Add(handle:=13, pid:=7777, title:="raport.pdf - Adobe Acrobat")
        Dim refused = New AdobeWindowCapture(win2).Find(
            launchedPid:=1001, baseName:="raport",
            allowForeignTitleMatch:=False, options:=FastOptions())
        Assert.False(refused.Found)
    End Sub

    <Fact>
    Public Sub Find_PrefersOurOwnWindowOverAForeignOneWithTheSameTitle()
        ' Both phases can match at once. Ours must win, or a second K-BOT document would embed the
        ' operator's window while our own sat there unused.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=20, pid:=7777, title:="raport.pdf - Adobe Acrobat")   ' theirs, listed first
        win.Add(handle:=21, pid:=1001, title:="raport.pdf - Adobe Acrobat")   ' ours
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=True, options:=FastOptions())

        Assert.Equal(New IntPtr(21), result.Window)
        Assert.Equal(AdobeCaptureMatch.ByPid, result.Match)
    End Sub

    <Fact>
    Public Sub Find_KeepsPollingUntilTheWindowTurnsUp()
        ' Adobe does not have a window the instant Process.Start returns.
        Dim win As New FakeNativeWindows()
        Dim w = win.Add(handle:=14, pid:=1001)
        w.AppearsAfterSweeps = 3
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=False, options:=FastOptions())

        Assert.True(result.Found)
        Assert.Equal(New IntPtr(14), result.Window)
    End Sub

    <Fact>
    Public Sub Find_RejectsANonAdobeWindowOfOurOwnProcess()
        ' Our own process can own windows that are not the document frame.
        Dim win As New FakeNativeWindows()
        win.Add(handle:=15, pid:=1001, className:="SplashScreenClass")
        Dim capture As New AdobeWindowCapture(win)

        Dim result = capture.Find(launchedPid:=1001, baseName:="raport",
                                  allowForeignTitleMatch:=False, options:=FastOptions())

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
