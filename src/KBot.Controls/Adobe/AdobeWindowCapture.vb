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
''' THE ONE CONCESSION, AND WHY. <see cref="AdobeViewerProfiles.Modern"/> launches WITHOUT «/n»,
''' which is a measured value, not a preference: Adobe then hands the document to an instance the
''' operator already had open and OUR process owns no window at all. A strict PID filter would
''' report «window not found» for every modern-profile document. So when — and only when — the
''' launch profile omitted «/n», a title match in a foreign process is accepted as a SECOND phase,
''' and the result says <see cref="AdobeCaptureMatch.ByTitle"/> so the caller can log it and refuse
''' to kill that process later.
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
    ''' <param name="launchedPid">The PID we started; 0 disables the (preferred) PID phase.</param>
    ''' <param name="baseName">The document's file name, used only by the title fallback.</param>
    ''' <param name="allowForeignTitleMatch">
    ''' True only when the launch profile omitted «/n». See the class remarks — this is the modern
    ''' profile's concession, not a general loosening.
    ''' </param>
    Public Function Find(launchedPid As Integer, baseName As String, allowForeignTitleMatch As Boolean,
                         options As AdobeHostOptions) As AdobeCaptureResult
        Try
            Dim opts As AdobeHostOptions = If(options, New AdobeHostOptions())
            Dim started As DateTime = DateTime.UtcNow

            ' A deliberate head start: see AdobeHostOptions.CaptureDelayMs. Default 0.
            If opts.CaptureDelayMs > 0 Then Thread.Sleep(opts.CaptureDelayMs)

            Dim deadline As DateTime = started.AddMilliseconds(Math.Max(1, opts.FindTimeoutMs))
            Do
                ' Phase 1 — ours by process id. No visibility test, no title test.
                If launchedPid > 0 Then
                    Dim mine As IntPtr = FirstAdobeWindow(launchedPid, Nothing)
                    If mine <> IntPtr.Zero Then
                        Return Captured(mine, launchedPid, AdobeCaptureMatch.ByPid, started)
                    End If
                End If

                ' Phase 2 — the modern profile's foreign instance, matched on the document name.
                If allowForeignTitleMatch AndAlso Not String.IsNullOrEmpty(baseName) Then
                    Dim foreign As IntPtr = FirstAdobeWindow(0, baseName)
                    If foreign <> IntPtr.Zero Then
                        Return Captured(foreign, _win.OwnerPid(foreign), AdobeCaptureMatch.ByTitle, started)
                    End If
                End If

                Thread.Sleep(Math.Max(1, opts.FindPollMs))
            Loop While DateTime.UtcNow < deadline

            Return New AdobeCaptureResult(IntPtr.Zero, 0, AdobeCaptureMatch.None, Elapsed(started))
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowCapture.Find", ex)
            Throw
        End Try
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

    ' One sweep. pid > 0 matches on owner; otherwise titleContains must match. Never tests visibility.
    Private Function FirstAdobeWindow(pid As Integer, titleContains As String) As IntPtr
        Dim all As IReadOnlyList(Of IntPtr) = _win.EnumTopLevelWindows()
        If all Is Nothing Then Return IntPtr.Zero
        For Each h As IntPtr In all
            If pid > 0 AndAlso _win.OwnerPid(h) <> pid Then Continue For
            If Not AdobeWindowHosting.IsAdobeWindowClass(_win.GetClass(h)) Then Continue For
            If Not String.IsNullOrEmpty(titleContains) Then
                Dim title As String = If(_win.GetTitle(h), "")
                If title.IndexOf(titleContains, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For
            End If
            Return h
        Next
        Return IntPtr.Zero
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
