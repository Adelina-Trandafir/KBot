Option Strict On
Imports KBot.Common

''' <summary>
''' Hides the Adobe window at the moment Windows reports it was created, instead of on the next tick
''' of the capture poll (slice 0024-03 §4).
'''
''' WHAT IT HONESTLY ACHIEVES. Out-of-context WinEvents are queued and delivered asynchronously
''' across the process boundary — Adobe creates and shows its window on ITS thread, and our callback
''' runs later, on ours. This NARROWS the interval in which the window can be seen. It cannot be
''' promised to close it, and no measurement in this repository says how much it helps. That is
''' precisely why <see cref="AdobeHostOptions.UseCreationHook"/> defaults to False: it is an
''' experiment with a switch, not a fix.
'''
''' TWO RULES THAT ARE NOT NEGOTIABLE.
'''  * The delegate is held in an instance field. A delegate collected while the hook is live is a
'''    hard crash inside user32, not a soft failure — this is the single most common way to get this
'''    API wrong.
'''  * WINEVENT_OUTOFCONTEXT delivers on a thread with a running message pump. Install from the UI
'''    thread only.
''' </summary>
Public NotInheritable Class AdobeCreationHook
    Implements IDisposable

    ' Held for the lifetime of the hook. See the class remarks — do not make this a local.
    Private ReadOnly _callback As AdobeNativeMethods.WinEventProc
    Private ReadOnly _log As Action(Of String)
    Private _hook As IntPtr = IntPtr.Zero
    Private _hidden As Integer = 0

    Public Sub New(log As Action(Of String))
        _log = log
        _callback = AddressOf OnWinEvent
    End Sub

    ''' <summary>True while a hook is installed.</summary>
    Public ReadOnly Property IsInstalled As Boolean
        Get
            Return _hook <> IntPtr.Zero
        End Get
    End Property

    ''' <summary>How many windows this hook has hidden — the bench reports it.</summary>
    Public ReadOnly Property HiddenCount As Integer
        Get
            Return _hidden
        End Get
    End Property

    ''' <summary>
    ''' Installs the hook, filtered to one process. Call from the UI thread. Installing twice is a
    ''' no-op on the second call rather than a leak.
    ''' </summary>
    Public Sub Install(pid As Integer)
        Try
            If _hook <> IntPtr.Zero Then Return
            If pid <= 0 Then Return
            _hook = AdobeNativeMethods.SetWinEventHook(
                AdobeNativeMethods.EVENT_OBJECT_CREATE,
                AdobeNativeMethods.EVENT_OBJECT_SHOW,
                IntPtr.Zero, _callback, CUInt(pid), 0UI,
                AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
            If _hook = IntPtr.Zero Then
                Report($"Cârligul de creare NU a putut fi instalat pentru procesul {pid} " &
                       "— captura se face doar prin sondare.")
            Else
                Report($"Cârlig de creare instalat pentru procesul {pid}.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeCreationHook.Install", ex)
            _hook = IntPtr.Zero
        End Try
    End Sub

    ''' <summary>Removes the hook. Safe to call when none is installed, and safe to call twice.</summary>
    Public Sub Remove()
        Try
            If _hook = IntPtr.Zero Then Return
            AdobeNativeMethods.UnhookWinEvent(_hook)
            _hook = IntPtr.Zero
            Report($"Cârlig de creare eliberat (a ascuns {_hidden} ferestre).")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeCreationHook.Remove", ex)
            _hook = IntPtr.Zero
        End Try
    End Sub

    ' Callback boundary: called from user32 across a process boundary. It PHYSICALLY cannot throw —
    ' an exception here unwinds into unmanaged code and takes the process with it. Log and swallow.
    Private Sub OnWinEvent(hook As IntPtr, eventType As UInteger, hwnd As IntPtr,
                           idObject As Integer, idChild As Integer,
                           threadId As UInteger, timestamp As UInteger)
        Try
            If hwnd = IntPtr.Zero Then Return
            ' The window itself, not an accessibility child of it.
            If idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0 Then Return
            ' Top-level only: Adobe creates hundreds of child windows and none of them is the frame.
            If AdobeNativeMethods.GetParent(hwnd) <> IntPtr.Zero Then Return
            If Not AdobeWindowHosting.IsAdobeWindowClass(AdobeNativeMethods.GetClass(hwnd)) Then Return

            AdobeNativeMethods.ShowWindow(hwnd, AdobeNativeMethods.SW_HIDE)
            _hidden += 1
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeCreationHook.OnWinEvent", ex)
        End Try
    End Sub

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Remove()
    End Sub

End Class
