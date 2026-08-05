Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Threading
Imports KBot.Common
Imports Microsoft.Win32

''' <summary>
''' The Win32 primitives for hosting an Adobe window in a panel: find the exe, build the command
''' line, find the window, reparent it, place it, force it to paint, and let it go again.
'''
''' EXTRACTED IN SLICE 0024. Both <c>AdobeReaderHarnessForm</c> (KBot.DevHarness) and
''' <c>ReaderHostPreview</c> (KBot.App) used to carry their own copy of every one of these, and the
''' copies had ALREADY diverged — the bench stripped WS_SYSMENU / WS_MINIMIZEBOX / WS_MAXIMIZEBOX
''' and nudged the window into painting, the preview did neither, so the same PDF behaved
''' differently in the two places. One implementation means the next Adobe update is diagnosed once.
''' </summary>
Public NotInheritable Class AdobeWindowHosting

    Private Sub New()
    End Sub

    ''' <summary>Timeout when looking for the launched Adobe window (ms).</summary>
    Public Const FindTimeoutMs As Integer = 8000
    ''' <summary>Polling step while looking for it (ms).</summary>
    Public Const FindPollMs As Integer = 150
    ''' <summary>Delay before the second redraw — Adobe finishes its layout after the window appears.</summary>
    Public Const RedrawDelayMs As Integer = 250

    ''' <summary>The process names an Adobe viewer runs under.</summary>
    Public Shared ReadOnly ProcessNames As String() = {"Acrobat", "AcroRd32"}

    ' ── Locating Adobe ──────────────────────────────────────────────────────────
    ''' <summary>
    ''' The Adobe executable: the App Paths registry entry first, then the usual install folders.
    ''' Nothing when Adobe is not installed — the caller must say so in Romanian, never crash.
    '''
    ''' NOTE: <c>AdobeUtils.GetAdobeReaderPath</c> (KBot.Xfa) resolves the .pdf HANDLER instead, but
    ''' it is Private and KBot.Controls does not reference KBot.Xfa; unifying them would mean opening
    ''' the signing engine, which is out of scope here.
    ''' </summary>
    Public Shared Function ResolveAdobePath() As String
        Try
            For Each exe As String In New String() {"Acrobat.exe", "AcroRd32.exe"}
                Dim p As String = ReadAppPath(exe)
                If Not String.IsNullOrEmpty(p) AndAlso File.Exists(p) Then Return p
            Next

            Dim candidates As String() = {
                "Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                "Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                "Adobe\Acrobat\Acrobat.exe"
            }
            For Each pf As String In New String() {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}
                If String.IsNullOrEmpty(pf) Then Continue For
                For Each c As String In candidates
                    Dim full As String = Path.Combine(pf, c)
                    If File.Exists(full) Then Return full
                Next
            Next
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowHosting.ResolveAdobePath", ex)
            Return Nothing
        End Try
    End Function

    Private Shared Function ReadAppPath(exeName As String) As String
        Try
            Using k As RegistryKey = Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" & exeName)
                If k IsNot Nothing Then Return TryCast(k.GetValue(Nothing), String)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowHosting.ReadAppPath", ex)
        End Try
        Return Nothing
    End Function

    ' ── Command line ────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Adobe's syntax: <c>[/n] [/s] [/A "param1&amp;param2&amp;…"] "cale.pdf"</c>. The /A parameters
    ''' MUST come before the file name — Adobe ignores them otherwise. Pure, so the exact command
    ''' line each profile produces is pinned by tests.
    ''' </summary>
    Public Shared Function BuildArguments(newInstance As Boolean, noSplash As Boolean,
                                          openParameters As String, pdfPath As String) As String
        Dim sb As New StringBuilder()
        If newInstance Then sb.Append("/n ")
        If noSplash Then sb.Append("/s ")
        If Not String.IsNullOrEmpty(openParameters) Then
            sb.Append("/A """).Append(openParameters).Append(""" ")
        End If
        sb.Append(""""c).Append(pdfPath).Append(""""c)
        Return sb.ToString()
    End Function

    ''' <summary>The command line a profile produces for a document.</summary>
    Public Shared Function BuildArguments(profile As AdobeViewerProfile, pdfPath As String) As String
        Return BuildArguments(profile, pdfPath, Nothing)
    End Function

    ''' <summary>
    ''' The command line a profile produces, plus any extra switches the caller wants appended. The
    ''' extras go BEFORE the file name, like every other switch — Adobe ignores anything after it.
    ''' </summary>
    Public Shared Function BuildArguments(profile As AdobeViewerProfile, pdfPath As String,
                                          extraArgs As String) As String
        Dim head As String
        If profile Is Nothing Then
            head = ""
        Else
            head = BuildArguments(profile.NewInstance, profile.NoSplash, profile.OpenParametersText(), "")
            ' Strip the empty quoted file name the shared builder appends; it is re-added below.
            head = head.Substring(0, Math.Max(0, head.Length - 2))
        End If
        Dim extras As String = If(String.IsNullOrWhiteSpace(extraArgs), "", extraArgs.Trim() & " ")
        Return head & extras & """"c & pdfPath & """"c
    End Function

    ' ── Finding the window ──────────────────────────────────────────────────────
    ''' <summary>
    ''' A top-level Adobe window whose title contains <paramref name="baseName"/>, polled until the
    ''' timeout. IntPtr.Zero when it never appears — the caller then SAYS so rather than grabbing the
    ''' wrong window.
    '''
    ''' THE PID-LESS PATH. Prefer <see cref="AdobeWindowCapture.Find"/> with a process id, which can
    ''' at least PREFER our own window; here every match is a title match. It remains for the bench,
    ''' which starts Adobe in a separate scenario step and has no PID at this point.
    '''
    ''' It no longer requires the window to be VISIBLE, and it hides the window the instant it
    ''' matches — that visibility gate was the reason Adobe was seen on screen, with its caption,
    ''' before it could be embedded.
    '''
    ''' Blocking by design: call it from a background thread.
    ''' </summary>
    Public Shared Function FindReaderWindow(baseName As String,
                                            Optional timeoutMs As Integer = FindTimeoutMs,
                                            Optional pollMs As Integer = 30) As IntPtr
        Try
            Dim capture As New AdobeWindowCapture()
            Dim opts As New AdobeHostOptions() With {.FindTimeoutMs = timeoutMs, .FindPollMs = pollMs}
            Return capture.Find(0, baseName, opts).Window
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowHosting.FindReaderWindow", ex)
            Throw
        End Try
    End Function

    ''' <summary>The main-window classes Adobe uses across versions (Reader and Acrobat).</summary>
    Public Shared Function IsAdobeWindowClass(className As String) As Boolean
        If String.IsNullOrEmpty(className) Then Return False
        Return className.IndexOf("Acrobat", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               className.IndexOf("AdobeAcrobat", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>Every live Adobe process id on this machine.</summary>
    Public Shared Function AdobeProcessIds() As List(Of Integer)
        Dim ids As New List(Of Integer)()
        Try
            For Each name As String In ProcessNames
                For Each p As Process In Process.GetProcessesByName(name)
                    Try
                        ids.Add(p.Id)
                    Finally
                        p.Dispose()
                    End Try
                Next
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowHosting.AdobeProcessIds", ex)
        End Try
        Return ids
    End Function

    ' ── Reparenting ─────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Turns a top-level Adobe window into a child of <paramref name="hostHandle"/>, stripping the
    ''' styles that only make sense on a standalone window. Returns the ORIGINAL style, for the log
    ''' and for diagnostics.
    '''
    ''' THE ORIGINAL STYLE IS NEVER WRITTEN BACK. There used to be a <c>RestoreStandalone</c> here
    ''' that put the style and the parent back on teardown; it was deleted in slice 0024-03 because
    ''' it WAS the defect. Restoring WS_CAPTION/WS_POPUP and re-parenting to the desktop is the
    ''' textbook way to create a top-level window, and a top-level window is a taskbar button — so
    ''' every document change left a live Adobe window behind, showing the previous PDF. Teardown
    ''' now drops the window instead: see <see cref="AdobeWindowTeardown"/>.
    ''' </summary>
    Public Shared Function AttachAsChild(hwnd As IntPtr, hostHandle As IntPtr) As IntPtr
        ' One implementation of the mask and the call order, shared with the capture path.
        Return New AdobeWindowCapture().AttachAsChild(hwnd, hostHandle)
    End Function

    ' ── Placing / painting ──────────────────────────────────────────────────────
    ''' <summary>Moves the hosted window to <paramref name="bounds"/> (host client coordinates).</summary>
    Public Shared Sub Place(hwnd As IntPtr, bounds As Rectangle)
        If hwnd = IntPtr.Zero OrElse bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return
        AdobeNativeMethods.MoveWindow(hwnd, bounds.X, bounds.Y, bounds.Width, bounds.Height, True)
    End Sub

    ''' <summary>
    ''' Forces the reparented window to show and repaint. WITHOUT THIS IT STAYS INVISIBLE until some
    ''' unrelated layout change happens — the single most confusing symptom of this whole mechanism,
    ''' and the one difference the preview copy was missing.
    ''' </summary>
    Public Shared Sub NudgeRedraw(hwnd As IntPtr, bounds As Rectangle)
        If hwnd = IntPtr.Zero OrElse bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return
        AdobeNativeMethods.SetWindowPos(hwnd, IntPtr.Zero, bounds.X, bounds.Y, bounds.Width, bounds.Height,
                                        AdobeNativeMethods.SWP_NOZORDER Or AdobeNativeMethods.SWP_NOACTIVATE Or
                                        AdobeNativeMethods.SWP_FRAMECHANGED Or AdobeNativeMethods.SWP_SHOWWINDOW)
        ' One pixel narrower and back: Adobe recomputes its layout on a size CHANGE, not on a repaint.
        AdobeNativeMethods.MoveWindow(hwnd, bounds.X, bounds.Y, Math.Max(1, bounds.Width - 1), bounds.Height, True)
        AdobeNativeMethods.MoveWindow(hwnd, bounds.X, bounds.Y, bounds.Width, bounds.Height, True)
        AdobeNativeMethods.RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero,
                                        AdobeNativeMethods.RDW_INVALIDATE Or AdobeNativeMethods.RDW_ALLCHILDREN Or
                                        AdobeNativeMethods.RDW_UPDATENOW Or AdobeNativeMethods.RDW_FRAME)
    End Sub

    ''' <summary>The hosted window's rectangle in its parent's CLIENT coordinates.</summary>
    Public Shared Function RectInParent(hwnd As IntPtr) As Rectangle
        Return AdobeNativeMethods.RectInParent(hwnd)
    End Function

    ''' <summary>Hides one window and says what that actually achieved.</summary>
    Public Shared Function Hide(hwnd As IntPtr) As HideOutcome
        If hwnd = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then Return HideOutcome.NotFound
        Dim r As Rectangle = AdobeNativeMethods.RectOnScreen(hwnd)
        Dim outcome As HideOutcome = HideOutcomeClassifier.Classify(
            found:=True, visible:=AdobeNativeMethods.IsWindowVisible(hwnd), width:=r.Width, height:=r.Height)
        If outcome = HideOutcome.Hidden Then AdobeNativeMethods.ShowWindow(hwnd, AdobeNativeMethods.SW_HIDE)
        Return outcome
    End Function

    ''' <summary>
    ''' Posts a left click at the centre of a window's client area.
    '''
    ''' EXISTS BECAUSE HIDING IS NOT COLLAPSING (measured 05.08.2026). Calling ShowWindow(SW_HIDE) on
    ''' Adobe's tab strip leaves its WIDTH intact, so Adobe never re-lays-out and the document view
    ''' stays inset — the probe showed it still at x=67 with 792px instead of x=0 with 859px. Adobe's
    ''' own collapse button sets the width to zero AND reflows the siblings, so the reliable way to
    ''' reach that state is to press Adobe's button and let Adobe do the layout.
    '''
    ''' Posted, not sent: a foreign UI thread must never block ours. Returns False when the window is
    ''' gone. EXPERIMENTAL in the same sense as the keyboard path of slice 0023 — a synthetic click
    ''' into another process's window is not guaranteed to be honoured, and «nothing happened» is a
    ''' valid result to record.
    ''' </summary>
    Public Shared Function ClickCentre(hwnd As IntPtr) As Boolean
        If hwnd = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then Return False
        Dim r As Rectangle = AdobeNativeMethods.RectOnScreen(hwnd)
        ' Client coordinates, so the centre is half the size — not the screen position.
        Dim lp As IntPtr = AdobeNativeMethods.MakeLParam(Math.Max(0, r.Width \ 2), Math.Max(0, r.Height \ 2))
        AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.WM_LBUTTONDOWN, New IntPtr(1), lp)
        AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.WM_LBUTTONUP, IntPtr.Zero, lp)
        Return True
    End Function

    ''' <summary>Shows one window again (the bench's «rearată»).</summary>
    Public Shared Sub Show(hwnd As IntPtr)
        If hwnd = IntPtr.Zero OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then Return
        AdobeNativeMethods.ShowWindow(hwnd, AdobeNativeMethods.SW_SHOW)
    End Sub

    ''' <summary>The process that owns a window, or 0.</summary>
    Public Shared Function OwnerPid(hwnd As IntPtr) As Integer
        Return AdobeNativeMethods.OwnerPid(hwnd)
    End Function

    ''' <summary>
    ''' True while the handle still names a window. Adobe destroys and recreates its panels, so a
    ''' handle recorded a second ago can already be dead — every caller checks before using one.
    ''' </summary>
    Public Shared Function IsAlive(hwnd As IntPtr) As Boolean
        Return hwnd <> IntPtr.Zero AndAlso AdobeNativeMethods.IsWindow(hwnd)
    End Function

    ''' <summary>True when the window is currently visible.</summary>
    Public Shared Function IsVisible(hwnd As IntPtr) As Boolean
        Return hwnd <> IntPtr.Zero AndAlso AdobeNativeMethods.IsWindowVisible(hwnd)
    End Function

    ''' <summary>
    ''' Gives the keyboard focus to the hosted window. EXPERIMENTAL across a process boundary — a
    ''' reparented foreign window does not behave like a native child, and "nothing happened" is a
    ''' valid result to record.
    ''' </summary>
    Public Shared Sub FocusWindow(hwnd As IntPtr)
        If hwnd = IntPtr.Zero Then Return
        AdobeNativeMethods.SetFocus(hwnd)
    End Sub

    ''' <summary>Kills a process tree by id. Best-effort: the process may already be gone.</summary>
    Public Shared Sub KillPid(pid As Integer)
        If pid <= 0 Then Return
        Try
            Dim p As Process = Process.GetProcessById(pid)
            Try
                If Not p.HasExited Then p.Kill(True)
            Finally
                p.Dispose()
            End Try
        Catch
            ' Best-effort: the process may not exist any more. Nothing to report.
        End Try
    End Sub

End Class
