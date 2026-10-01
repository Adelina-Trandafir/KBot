Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports KBot.Common

''' <summary>
''' Slice 0078-05, OPT-IN (Setări, off by default): the last release of a hosted Adobe window, when
''' the K-BOT window holding it closes, hands the window back to the desktop at the size of the
''' screen and closes it there.
'''
''' WHY. Adobe remembers the size of the window it closes. The hosted window is closed as a child of
''' the K-BOT panel (almost always through WM_CLOSE: Adobe hands the document to its running
''' instance, so the window is «foreign» and is never killed), and the operator's own Adobe then
''' opens at the panel's size. Closing it once more as a normal, maximized window of the work area
''' is meant to leave THAT size behind instead (operator, 29.09.2026: to be tested on their machine).
'''
''' This is the only place where a hosted window gets its style back and leaves the panel -- the
''' thing <see cref="AdobeWindowTeardown"/> must never do on a document change (it left a stray
''' window with a taskbar button). Here the window is closed right after, and a window that survives
''' the close (Adobe asking whether to save, for example) is left on screen on purpose: a visible
''' window the operator can answer, not a hidden one.
''' </summary>
Public NotInheritable Class AdobeScreenRelease

    ''' <summary>How long the window gets to close before a process K-BOT started is ended.</summary>
    Public Const GraceMs As Integer = 3000
    Private Const PollMs As Integer = 25
    Private Const SW_MAXIMIZE As Integer = 3

    ' The standalone frame given to a window whose original style is unknown.
    Private Const OverlappedWindow As Long =
        AdobeNativeMethods.WS_CAPTION Or AdobeNativeMethods.WS_SYSMENU Or AdobeNativeMethods.WS_THICKFRAME Or
        AdobeNativeMethods.WS_MINIMIZEBOX Or AdobeNativeMethods.WS_MAXIMIZEBOX

    Private ReadOnly _win As INativeWindows
    Private ReadOnly _launcher As IAdobeLauncher

    Public Sub New(Optional windows As INativeWindows = Nothing, Optional launcher As IAdobeLauncher = Nothing)
        _win = If(windows, Win32Windows.Instance)
        _launcher = If(launcher, ProcessAdobeLauncher.Instance)
    End Sub

    ''' <summary>
    ''' The top-level style to give back: the one the window had before it was hosted, without
    ''' WS_CHILD / WS_VISIBLE; a standard frame when it is unknown (0).
    ''' </summary>
    Public Shared Function TopLevelStyle(originalStyle As Long, currentStyle As Long) As Long
        Dim style As Long = If(originalStyle <> 0, originalStyle, currentStyle Or OverlappedWindow)
        Return style And Not AdobeNativeMethods.WS_CHILD And Not AdobeNativeMethods.WS_VISIBLE
    End Function

    ''' <summary>
    ''' Takes <paramref name="hwnd"/> out of its panel, maximizes it over <paramref name="workArea"/>
    ''' and closes it. Returns the Romanian sentence for the working log; <paramref name="killed"/>
    ''' says whether the process had to be ended (only ever one in <paramref name="launchedPids"/>).
    ''' </summary>
    Public Function Run(hwnd As IntPtr, hostedPid As Integer, originalStyle As Long, workArea As Rectangle,
                        launchedPids As ISet(Of Integer), ByRef killed As Boolean,
                        Optional whileClosing As Func(Of Boolean) = Nothing) As String
        killed = False
        _whileClosing = whileClosing
        Try
            If hwnd = IntPtr.Zero OrElse Not _win.IsWindow(hwnd) Then Return ""

            _win.ShowWindow(hwnd, AdobeNativeMethods.SW_HIDE)
            Dim current As Long = _win.GetWindowLongPtr(hwnd, AdobeNativeMethods.GWL_STYLE).ToInt64()
            _win.SetWindowLongPtr(hwnd, AdobeNativeMethods.GWL_STYLE, New IntPtr(TopLevelStyle(originalStyle, current)))
            _win.SetParent(hwnd, IntPtr.Zero)
            _win.SetWindowPos(hwnd, IntPtr.Zero, workArea.X, workArea.Y, workArea.Width, workArea.Height,
                              AdobeNativeMethods.SWP_NOZORDER Or AdobeNativeMethods.SWP_NOACTIVATE Or
                              AdobeNativeMethods.SWP_FRAMECHANGED)
            _win.ShowWindow(hwnd, SW_MAXIMIZE)
            _win.PostMessage(hwnd, AdobeNativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero)

            Dim area As String = $"{workArea.X},{workArea.Y} {workArea.Width}x{workArea.Height}"
            If WaitForWindowToDie(hwnd, GraceMs) Then
                Return $"La închidere: fereastra Adobe (PID {hostedPid}) a fost readusă pe tot ecranul " &
                       $"({area}, maximizată) și închisă — Adobe ține minte această dimensiune."
            End If

            Dim ours As Boolean = hostedPid > 0 AndAlso launchedPids IsNot Nothing AndAlso launchedPids.Contains(hostedPid)
            If ours Then
                _launcher.Kill(hostedPid)
                killed = True
                Return $"La închidere: fereastra Adobe a fost readusă pe tot ecranul ({area}), dar nu s-a închis " &
                       $"în {GraceMs} ms — am oprit procesul {hostedPid}, pornit de K-BOT (dimensiunea poate să nu fi rămas salvată)."
            End If
            Return $"La închidere: fereastra Adobe a fost readusă pe tot ecranul ({area}), dar nu s-a închis în " &
                   $"{GraceMs} ms (poate Adobe întreabă ceva) — rămâne deschisă pe ecran; procesul {hostedPid} nu e al K-BOT."
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeScreenRelease.Run", ex)
            Throw
        Finally
            _whileClosing = Nothing
        End Try
    End Function

    ' slice 0078-09: the poll of the Run in progress, as in AdobeWindowTeardown (the trap answers
    ' Adobe's «save changes before closing?» box; True = a save is going on, the wait starts over).
    Private _whileClosing As Func(Of Boolean)

    Private Function WaitForWindowToDie(hwnd As IntPtr, graceMs As Integer) As Boolean
        Dim started As DateTime = DateTime.UtcNow
        Dim deadline As DateTime = started.AddMilliseconds(Math.Max(0, graceMs))
        Dim limit As DateTime = started.AddMilliseconds(Math.Max(graceMs, AdobeWindowTeardown.MaxExtendedWaitMs))
        Do
            If Not _win.IsWindow(hwnd) Then Return True
            If _whileClosing IsNot Nothing AndAlso _whileClosing.Invoke() Then
                Dim extended As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(0, graceMs))
                If extended > deadline Then deadline = If(extended < limit, extended, limit)
            End If
            If DateTime.UtcNow >= deadline Then Return False
            Thread.Sleep(PollMs)
        Loop
    End Function

End Class
