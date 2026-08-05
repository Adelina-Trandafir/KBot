Option Strict On
Imports System.IO
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Controls

''' <summary>
''' The orchestration of slice 0024-03: what happens when there is no Adobe, and what happens when
''' the operator clicks a second document while the first is still being embedded.
'''
''' These run entirely on fakes. The real <see cref="AdobeWindowProbe"/> still walks a window tree
''' with a handle that does not exist, which is safe — user32 answers «not a window» — and yields an
''' empty tree, so detection is Unknown and the conservative classic profile applies. That is the
''' intended offline behaviour, not an accident of the test.
''' </summary>
Public Class AdobeReaderHostTests

    ' A file that exists, because ShowDocumentAsync refuses a path that does not.
    ' NOTE: the local is NOT called `path` — VB is case-insensitive, so `path` shadows System.IO.Path
    ' and every `Path.Combine` in the method silently becomes a String member lookup.
    Private Shared Function NewTempPdf() As String
        Dim full As String = Path.Combine(Path.GetTempPath(),
                                          "kbot_test_" & Guid.NewGuid().ToString("N") & ".pdf")
        File.WriteAllText(full, "%PDF-1.4 test")
        Return full
    End Function

    Private Shared Sub Discard(file As String)
        Try
            If IO.File.Exists(file) Then IO.File.Delete(file)
        Catch
            ' A temp file we could not remove is not a test failure.
        End Try
    End Sub

    Private Shared Function NewHost(win As FakeNativeWindows, launcher As FakeAdobeLauncher) As AdobeReaderHost
        Dim host As New AdobeReaderHost(New FakeHostSurface(), Nothing, win, launcher)
        host.Options.FindTimeoutMs = 500
        host.Options.FindPollMs = 5
        host.Options.RedrawDelayMs = 1
        host.Options.CloseGraceMs = 50
        Return host
    End Function

    <Fact>
    Public Async Function NoAdobeInstalled_ReportsItInRomanian_AndStartsNothing() As Task
        ' §9 test 1. There is deliberately NO fallback to the default handler: these are LiveCycle
        ' XFA documents and nothing else renders them, so a fallback would show a broken page and
        ' call it success.
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher() With {.PathToReturn = Nothing}
        Dim host = NewHost(win, launcher)
        Dim pdf As String = NewTempPdf()

        Try
            Dim result = Await host.ShowDocumentAsync(pdf)

            Assert.Equal(AdobeHostStatus.AdobeMissing, result.Status)
            Assert.Equal("Nu am găsit niciun produs Adobe instalat. Documentul nu poate fi afișat.",
                         result.Message)
            Assert.Empty(launcher.Started)
            Assert.False(host.IsHosting)
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

    <Fact>
    Public Async Function LaunchFailure_ReportsItInRomanian() As Task
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher() With {.ThrowOnStart = True}
        Dim host = NewHost(win, launcher)
        Dim pdf As String = NewTempPdf()

        Try
            Dim result = Await host.ShowDocumentAsync(pdf)

            Assert.Equal(AdobeHostStatus.LaunchFailed, result.Status)
            Assert.Equal("Adobe nu a putut fi pornit. Detalii în jurnalul de erori.", result.Message)
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

    <Fact>
    Public Async Function WindowNeverAppears_ReportsItInRomanian() As Task
        Dim win As New FakeNativeWindows()          ' an empty desktop
        Dim launcher As New FakeAdobeLauncher()
        Dim host = NewHost(win, launcher)
        Dim pdf As String = NewTempPdf()

        Try
            Dim result = Await host.ShowDocumentAsync(pdf)

            Assert.Equal(AdobeHostStatus.WindowNotFound, result.Status)
            Assert.Equal("Adobe a pornit, dar fereastra documentului nu a apărut.", result.Message)
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

    <Fact>
    Public Async Function ASuccessfulEmbed_HostsTheWindowAndRevealsItOnlyAfterPlacing() As Task
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher()
        Dim host = NewHost(win, launcher)
        Dim pdf As String = NewTempPdf()
        ' The window is identified by the DOCUMENT NAME, so the fake must carry it — a bare PID is
        ' deliberately not enough any more. The launcher hands out 1001 first.
        Dim w = win.Add(handle:=40, pid:=1001,
                        title:=Path.GetFileNameWithoutExtension(pdf) & " - Adobe Acrobat",
                        visible:=True)

        Try
            Dim result = Await host.ShowDocumentAsync(pdf)

            Assert.Equal(AdobeHostStatus.Hosted, result.Status)
            Assert.True(host.IsHosting)
            Assert.Equal(New IntPtr(40), host.HostedWindow)
            Assert.Equal(1001, host.HostedPid)
            Assert.Equal(AdobeCaptureMatch.ByPid, result.Match)

            ' Hidden on capture, revealed after it was made a child and placed. The order is the
            ' whole fix: hide -> restyle -> reparent -> move -> show.
            Dim hideAt As Integer = win.Calls.IndexOf("ShowWindow(40,0)")
            Dim parentAt As Integer = win.Calls.FindIndex(Function(c) c.StartsWith("SetParent(40,", StringComparison.Ordinal))
            Dim showAt As Integer = win.Calls.IndexOf("ShowWindow(40,5)")
            Assert.True(hideAt >= 0, "fereastra nu a fost ascunsă la captură")
            Assert.True(parentAt > hideAt, "reparentarea s-a făcut înainte de ascundere")
            Assert.True(showAt > parentAt, "fereastra a fost arătată înainte de a fi încorporată")
            Assert.Equal(New IntPtr(9000), w.Parent)
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

    <Fact>
    Public Async Function DetachAfterHosting_KillsOurProcess_AndNeverHandsTheWindowBack() As Task
        ' The end-to-end version of the taskbar defect: after a real embed, letting go must not
        ' produce a top-level window.
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher()
        Dim host = NewHost(win, launcher)
        Dim pdf As String = NewTempPdf()
        win.Add(handle:=41, pid:=1001, title:=Path.GetFileNameWithoutExtension(pdf))

        Try
            Await host.ShowDocumentAsync(pdf)
            Dim callsBeforeDetach As Integer = win.Calls.Count

            host.Detach()

            Assert.False(host.IsHosting)
            Assert.Contains(1001, launcher.Killed)
            ' Nothing after the embed may re-parent or re-style the window.
            Dim afterDetach = win.Calls.GetRange(callsBeforeDetach, win.Calls.Count - callsBeforeDetach)
            Assert.DoesNotContain(afterDetach, Function(c) c.StartsWith("SetParent", StringComparison.Ordinal))
            Assert.DoesNotContain(afterDetach, Function(c) c.StartsWith("SetWindowLongPtr", StringComparison.Ordinal))
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

    <Fact>
    Public Async Function ANewerDocumentSupersedesTheOlderOne_AndKillsOnlyItsOwnProcess() As Task
        ' §9 test 5. Two fast clicks in the tree: the first embed must not win a race against the
        ' second, and abandoning it must not leave its Adobe process running.
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher()
        Dim host = NewHost(win, launcher)
        Dim first As String = NewTempPdf()
        Dim second As String = NewTempPdf()

        Try
            ' Nothing on the desktop yet, so the first request sits in its poll loop.
            Dim firstTask As Task(Of AdobeHostResult) = host.ShowDocumentAsync(first)
            Assert.Single(launcher.Started)          ' pid 1001 is in flight

            ' The second request supersedes it, and this one does find a window (pid 1002). The
            ' title carries the SECOND document's name, so the first request can never match it.
            win.Add(handle:=42, pid:=1002, title:=Path.GetFileNameWithoutExtension(second))
            Dim secondResult = Await host.ShowDocumentAsync(second)
            Dim firstResult = Await firstTask

            Assert.Equal(AdobeHostStatus.Hosted, secondResult.Status)
            Assert.Equal(1002, host.HostedPid)
            Assert.Equal(AdobeHostStatus.Superseded, firstResult.Status)

            ' The abandoned launch was cleaned up — and only it.
            Assert.Contains(1001, launcher.Killed)
            Assert.DoesNotContain(1002, launcher.Killed)
        Finally
            host.Dispose()
            Discard(first)
            Discard(second)
        End Try
    End Function

    <Fact>
    Public Async Function AForeignWindow_IsHostedButItsProcessIsNeverKilled() As Task
        ' The single-instance handoff, end to end, and the case that MUST work whatever «/n» says —
        ' the operator's log shows Adobe does this on every launch. We embed a window belonging to
        ' another Adobe process, and letting go must not close it.
        Dim win As New FakeNativeWindows()
        Dim launcher As New FakeAdobeLauncher()
        Dim host = NewHost(win, launcher)
        ' «/n» explicitly ON — capture must STILL take the foreign window. Requiring our own PID here
        ' is what left the real window floating in the taskbar.
        host.NewInstanceMode = AdobeNewInstanceMode.Da

        Dim pdf As String = NewTempPdf()
        win.Add(handle:=43, pid:=7777,
                title:=Path.GetFileNameWithoutExtension(pdf) & " - Adobe Acrobat")

        Try
            Dim result = Await host.ShowDocumentAsync(pdf)

            Assert.Equal(AdobeHostStatus.Hosted, result.Status)
            Assert.Equal(AdobeCaptureMatch.ByTitle, result.Match)
            Assert.Equal(7777, host.HostedPid)

            host.Detach()
            Assert.DoesNotContain(7777, launcher.Killed)
        Finally
            host.Dispose()
            Discard(pdf)
        End Try
    End Function

End Class
