Option Strict On

''' <summary>
''' How a hosted Adobe window is let go when the document changes or the preview closes.
'''
''' NEITHER MODE RESTORES THE WINDOW. Handing the window back — putting its original style back and
''' re-parenting it to the desktop — is exactly what left a stray Adobe window with a taskbar button
''' after every second document (slice 0024-03 §0). Both modes DROP the window instead of releasing
''' it; they differ only in how much they take with it.
''' </summary>
Public Enum AdobeDetachMode
    ''' <summary>
    ''' A — end the process we started. Only ever applied to a PID this host launched; a window that
    ''' belongs to the operator's own Adobe degrades to <see cref="CloseWindow"/>.
    ''' </summary>
    KillProcess = 0
    ''' <summary>
    ''' B — post WM_CLOSE to the hosted window only and leave the process warm, so the next document
    ''' opens without paying for an Adobe start. Falls back to A when the window outlives the grace
    ''' period.
    ''' </summary>
    CloseWindow = 1
End Enum

''' <summary>
''' The tunable parts of hosting an Adobe window. Every value has a default that matches what the
''' shipping preview wants; the bench exposes them as controls so a threshold can be FOUND on the
''' operator's machine rather than guessed here.
''' </summary>
Public NotInheritable Class AdobeHostOptions

    Public Property DetachMode As AdobeDetachMode = AdobeDetachMode.KillProcess

    ''' <summary>
    ''' Install a <c>SetWinEventHook</c> to hide the window the moment it is created, instead of on
    ''' the next poll tick. OFF by default: out-of-context WinEvents are delivered asynchronously
    ''' across the process boundary, so this NARROWS the window in which Adobe can be seen — it
    ''' cannot be promised to close it (slice 0024-03 §4).
    ''' </summary>
    Public Property UseCreationHook As Boolean = False

    ''' <summary>
    ''' Wait this long before looking for the window at all. A deliberate knob, not a fix: catching
    ''' the window too early may mean Adobe has not finished building its document view, and
    ''' reparenting a half-initialised main window is a RISK TO OBSERVE, not a settled fact. Default
    ''' 0 — the bench spinner is how the threshold gets measured.
    ''' </summary>
    Public Property CaptureDelayMs As Integer = 0

    ''' <summary>How long to keep looking for the window before giving up.</summary>
    Public Property FindTimeoutMs As Integer = AdobeWindowHosting.FindTimeoutMs

    ''' <summary>
    ''' Polling step while looking. 30 ms rather than the old 150: the window is hidden the instant
    ''' it is matched, so every tick of latency is a tick in which Adobe can be seen.
    ''' </summary>
    Public Property FindPollMs As Integer = 30

    ''' <summary>Delay before the second redraw pass — Adobe lays out after the window appears.</summary>
    Public Property RedrawDelayMs As Integer = AdobeWindowHosting.RedrawDelayMs

    ''' <summary>How long mode B waits for the window to die before falling back to mode A.</summary>
    Public Property CloseGraceMs As Integer = 1500

    ''' <summary>Extra command-line switches, appended after the profile's own. The bench uses this.</summary>
    Public Property ExtraArgs As String = Nothing

    ''' <summary>A copy, so a caller can hand options to the host and keep editing its own.</summary>
    Public Function Clone() As AdobeHostOptions
        Return New AdobeHostOptions() With {
            .DetachMode = DetachMode,
            .UseCreationHook = UseCreationHook,
            .CaptureDelayMs = CaptureDelayMs,
            .FindTimeoutMs = FindTimeoutMs,
            .FindPollMs = FindPollMs,
            .RedrawDelayMs = RedrawDelayMs,
            .CloseGraceMs = CloseGraceMs,
            .ExtraArgs = ExtraArgs}
    End Function

    ''' <summary>One line for the log — the settings in force for this document.</summary>
    Public Function Describe() As String
        Return $"închidere={If(DetachMode = AdobeDetachMode.KillProcess, "omoară procesul (A)", "închide fereastra (B)")} · " &
               $"cârlig de creare={If(UseCreationHook, "da", "nu")} · " &
               $"întârziere captură={CaptureDelayMs} ms · pas căutare={FindPollMs} ms · " &
               $"răgaz închidere={CloseGraceMs} ms"
    End Function

End Class
