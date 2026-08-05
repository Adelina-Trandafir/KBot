Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports KBot.Common

''' <summary>How the hosted window was identified. Recorded because the two are NOT equally safe.</summary>
Public Enum AdobeCaptureMatch
    ''' <summary>Nothing was found.</summary>
    None = 0
    ''' <summary>Matched on the process id K-BOT launched — the window is unambiguously ours.</summary>
    ByPid = 1
    ''' <summary>
    ''' Matched on the document title in a process K-BOT did NOT launch. Legitimate only when the
    ''' profile launched without «/n» and Adobe handed the document to an existing instance.
    ''' </summary>
    ByTitle = 2
End Enum

''' <summary>What the search found.</summary>
Public NotInheritable Class AdobeCaptureResult

    Public ReadOnly Property Window As IntPtr
    Public ReadOnly Property OwnerPid As Integer
    Public ReadOnly Property Match As AdobeCaptureMatch
    ''' <summary>Milliseconds from the start of the search to the match — the bench displays this.</summary>
    Public ReadOnly Property ElapsedMs As Integer

    Public Sub New(window As IntPtr, ownerPid As Integer, match As AdobeCaptureMatch, elapsedMs As Integer)
        Me.Window = window
        Me.OwnerPid = ownerPid
        Me.Match = match
        Me.ElapsedMs = elapsedMs
    End Sub

    Public ReadOnly Property Found As Boolean
        Get
            Return Window <> IntPtr.Zero
        End Get
    End Property

End Class

''' <summary>
''' Finds the Adobe window and turns it into a child of the host panel.
'''
''' THIS CLASS IS THE FIX FOR THE FLASH (slice 0024-03 §0, symptom 2). The search it replaces
''' rejected any candidate that was not already VISIBLE, which by construction meant the window —
''' caption, frame and all — had been drawn on screen before the code was allowed to touch it. No
''' amount of hiding afterwards can remove a frame that has already been presented. Here:
'''
'''  * visibility is never tested. An invisible window is a BETTER catch, not a worse one;
'''  * the window is hidden THE INSTANT it is matched, inside the search loop, before the caller
'''    even learns it exists;
'''  * identity comes from the process id, so a window the operator opened by hand cannot be
'''    grabbed by accident — the old search matched a title substring and would happily take one.
'''
''' THE DOCUMENT TITLE IS REQUIRED, AND THE PROCESS ID IS ONLY A PREFERENCE. This was learned the
''' hard way, from the operator's own `adobe_preview.log`: on a real machine EVERY launch logs
''' «fereastra încorporată (PID 25168) NU a fost creată de K-BOT (am pornit PID 27152)». Adobe is
''' effectively single-instance and hands the document to an already-running copy — even when the
''' profile passes «/n», which it ignores. So our launched process very often owns NO window at all,
''' and a capture that REQUIRES the PID to match reports «window not found» while the real Adobe
''' window sits on screen as a floating, taskbar-listed window. That is exactly the regression this
''' class shipped with once.
'''
''' Therefore: a candidate must carry the document name in its title (which is what makes it the
''' right window, and what stops a helper window of the same class from being grabbed), and the PID
''' is used only to PREFER our own window and to LABEL the match. A
''' <see cref="AdobeCaptureMatch.ByTitle"/> result tells the caller the window is foreign, which is
''' what stops teardown from ending someone else's process — that protection lives in
''' <see cref="AdobeWindowTeardown"/> and its launched-PID set, and never needed capture to be
''' strict.
''' </summary>
Public NotInheritable Class AdobeWindowCapture

    Private ReadOnly _win As INativeWindows

    Public Sub New(Optional windows As INativeWindows = Nothing)
        _win = If(windows, Win32Windows.Instance)
    End Sub

    ''' <summary>
    ''' Polls for the window, hides it on sight, and returns it. Blocking by design — call it from a
    ''' background thread.
    ''' </summary>
    ''' <param name="launchedPid">
    ''' The PID we started. Used to PREFER our own window and to label the match; a window belonging
    ''' to another process is still accepted, because Adobe routinely hands the document to one.
    ''' </param>
    ''' <param name="baseName">The document's file name. Required — see the class remarks.</param>
    Public Function Find(launchedPid As Integer, baseName As String,
                         options As AdobeHostOptions) As AdobeCaptureResult
        Try
            Dim opts As AdobeHostOptions = If(options, New AdobeHostOptions())
            Dim started As DateTime = DateTime.UtcNow

            ' A deliberate head start: see AdobeHostOptions.CaptureDelayMs. Default 0.
            If opts.CaptureDelayMs > 0 Then Thread.Sleep(opts.CaptureDelayMs)

            Dim deadline As DateTime = started.AddMilliseconds(Math.Max(1, opts.FindTimeoutMs))
            Do
                Dim hit As AdobeCaptureResult = SweepOnce(launchedPid, baseName, started)
                If hit IsNot Nothing Then Return hit
                Thread.Sleep(Math.Max(1, opts.FindPollMs))
            Loop While DateTime.UtcNow < deadline

            Return New AdobeCaptureResult(IntPtr.Zero, 0, AdobeCaptureMatch.None, Elapsed(started))
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowCapture.Find", ex)
            Throw
        End Try
    End Function

    ' One pass over the desktop. Ours wins over a stranger's, but a stranger's is still taken —
    ' refusing it is what left the real window floating in the taskbar.
    Private Function SweepOnce(launchedPid As Integer, baseName As String,
                               started As DateTime) As AdobeCaptureResult
        If String.IsNullOrEmpty(baseName) Then Return Nothing
        Dim all As IReadOnlyList(Of IntPtr) = _win.EnumTopLevelWindows()
        If all Is Nothing Then Return Nothing

        Dim ours As IntPtr = IntPtr.Zero
        Dim foreign As IntPtr = IntPtr.Zero

        For Each h As IntPtr In all
            ' Visibility is deliberately NOT tested: catching the window while it is still hidden is
            ' the whole point. The title is what identifies it.
            If Not AdobeWindowHosting.IsAdobeWindowClass(_win.GetClass(h)) Then Continue For
            Dim title As String = If(_win.GetTitle(h), "")
            If title.IndexOf(baseName, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For

            If launchedPid > 0 AndAlso _win.OwnerPid(h) = launchedPid Then
                If ours = IntPtr.Zero Then ours = h
            ElseIf foreign = IntPtr.Zero Then
                foreign = h
            End If
        Next

        If ours <> IntPtr.Zero Then Return Captured(ours, launchedPid, AdobeCaptureMatch.ByPid, started)
        If foreign <> IntPtr.Zero Then
            Return Captured(foreign, _win.OwnerPid(foreign), AdobeCaptureMatch.ByTitle, started)
        End If
        Return Nothing
    End Function

    ' Hide FIRST, report second. Between the match and this call the window may be on screen; there
    ' is nothing closer to the creation moment available from a polling loop.
    Private Function Captured(hwnd As IntPtr, pid As Integer, match As AdobeCaptureMatch,
                              started As DateTime) As AdobeCaptureResult
        _win.ShowWindow(hwnd, AdobeNativeMethods.SW_HIDE)
        Return New AdobeCaptureResult(hwnd, pid, match, Elapsed(started))
    End Function

    Private Shared Function Elapsed(started As DateTime) As Integer
        Return CInt(Math.Min(Integer.MaxValue, (DateTime.UtcNow - started).TotalMilliseconds))
    End Function

    ''' <summary>
    ''' The style a top-level window must carry to behave as a hosted child: the six standalone bits
    ''' cleared, WS_CHILD set. Pure, so the mask is pinned by a test instead of being re-derived by
    ''' eye every time someone reads the reparenting code.
    ''' </summary>
    Public Shared Function ChildStyle(originalStyle As Long) As Long
        Return (originalStyle And Not AdobeNativeMethods.StandaloneStyles) Or AdobeNativeMethods.WS_CHILD
    End Function

    ''' <summary>
    ''' Makes the window a child of <paramref name="hostHandle"/> and returns its ORIGINAL style.
    '''
    ''' The original style is returned for the LOG and for diagnostics only. It is deliberately never
    ''' written back: see <see cref="AdobeDetachMode"/> — restoring it is what produced the stray
    ''' taskbar window this slice removes.
    ''' </summary>
    Public Function AttachAsChild(hwnd As IntPtr, hostHandle As IntPtr) As IntPtr
        Try
            Dim original As IntPtr = _win.GetWindowLongPtr(hwnd, AdobeNativeMethods.GWL_STYLE)
            _win.SetWindowLongPtr(hwnd, AdobeNativeMethods.GWL_STYLE,
                                  New IntPtr(ChildStyle(original.ToInt64())))
            ' A child window has no taskbar button, whatever its styles say.
            _win.SetParent(hwnd, hostHandle)
            Return original
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowCapture.AttachAsChild", ex)
            Throw
        End Try
    End Function

    ''' <summary>Shows the embedded window again once it is placed (§3.4 step 5).</summary>
    Public Sub Reveal(hwnd As IntPtr)
        If hwnd = IntPtr.Zero Then Return
        _win.ShowWindow(hwnd, AdobeNativeMethods.SW_SHOW)
    End Sub

End Class
