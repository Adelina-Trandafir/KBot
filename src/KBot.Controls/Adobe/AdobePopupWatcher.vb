Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Watches for the floating <c>AVL_AVPopup</c> badge Adobe draws OVER its own window and hides it.
'''
''' WHY IT SHIPS WHEN THE REGISTRY SWITCH DOES NOT: hiding a floating badge inside OUR OWN hosted
''' window changes nothing outside K-BOT and lasts exactly as long as that Adobe process. Writing
''' <c>bEnableAv2</c>, by contrast, would change the operator's Adobe for every PDF they ever open,
''' everywhere — so the shipping code adapts to whichever UI it finds and never writes it.
'''
''' WHY A TIMER AND NOT A ONE-SHOT: the badge is created lazily and re-created after interactions,
''' so a single sweep right after embedding finds nothing. The same reason the bench's
''' <c>hideChildren</c> retries.
'''
''' The badge is a TOP-LEVEL window (not a child of the hosted window), which is why it is not
''' expressible in a saved bench state and why this watcher is the only thing that can remove it.
''' </summary>
Public NotInheritable Class AdobePopupWatcher
    Implements IDisposable

    ''' <summary>How often the desktop is swept for a badge (ms).</summary>
    Public Const DefaultIntervalMs As Integer = 500

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _timer As New Timer()
    ' Handles already hidden (or already rejected), so one badge does not produce a log line per tick.
    Private ReadOnly _seen As New HashSet(Of IntPtr)()
    Private _hostHandle As IntPtr = IntPtr.Zero
    Private _adobePids As New List(Of Integer)()

    Public Sub New(log As Action(Of String))
        _log = log
        _timer.Interval = DefaultIntervalMs
        AddHandler _timer.Tick, AddressOf OnTick
    End Sub

    ''' <summary>How many badges this watcher has actually hidden since it was started.</summary>
    Public ReadOnly Property HiddenCount As Integer

    ''' <summary>
    ''' Starts watching for badges belonging to <paramref name="adobePids"/> that overlap the host.
    ''' Returns False (and logs, in Romanian) when it could not attach — §6: a preview that silently
    ''' shows a grey rectangle is the worst possible outcome, so every failure path is visible.
    ''' </summary>
    Public Function Start(hostHandle As IntPtr, adobePids As IEnumerable(Of Integer)) As Boolean
        Try
            _hostHandle = hostHandle
            _adobePids = If(adobePids Is Nothing, New List(Of Integer)(), New List(Of Integer)(adobePids))
            _seen.Clear()
            _HiddenCount = 0
            If _hostHandle = IntPtr.Zero Then
                Report("Supraveghetorul de ferestre plutitoare Adobe NU a pornit: panoul-gazdă nu are handle.")
                Return False
            End If
            If _adobePids.Count = 0 Then
                Report("Supraveghetorul de ferestre plutitoare Adobe NU a pornit: niciun proces Adobe identificat.")
                Return False
            End If
            _timer.Start()
            Report($"Supraveghetor ferestre plutitoare Adobe pornit ({_adobePids.Count} proces(e), " &
                   $"la fiecare {DefaultIntervalMs} ms).")
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePopupWatcher.Start", ex)
            Report("Supraveghetorul de ferestre plutitoare Adobe nu a putut porni: " & ex.Message)
            Return False
        End Try
    End Function

    Public Sub [Stop]()
        Try
            _timer.Stop()
            _seen.Clear()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePopupWatcher.Stop", ex)
        End Try
    End Sub

    ' Timer tick = UI boundary: log and SWALLOW (a throw here would take the process down).
    Private Sub OnTick(sender As Object, e As EventArgs)
        Try
            Sweep()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePopupWatcher.OnTick", ex)
        End Try
    End Sub

    ''' <summary>
    ''' One sweep. Public so a caller can force it immediately after embedding instead of waiting a
    ''' whole interval for the first tick.
    ''' </summary>
    Public Sub Sweep()
        Try
            If _hostHandle = IntPtr.Zero Then Return
            Dim hostRect As Rectangle = AdobeNativeMethods.RectOnScreen(_hostHandle)

            AdobeNativeMethods.EnumWindows(
                Function(h, l)
                    Dim cls As String = AdobeNativeMethods.GetClass(h)
                    ' Pre-filter on class only: without it every window on the desktop would produce
                    ' a «RESPINS (altă clasă)» line, and the log that is supposed to explain a broken
                    ' preview would be unreadable.
                    If Not String.Equals(cls, AdobePopupFilter.PopupClass, StringComparison.OrdinalIgnoreCase) Then
                        Return True
                    End If

                    Dim rect As Rectangle = AdobeNativeMethods.RectOnScreen(h)
                    Dim verdict As AdobePopupVerdict = AdobePopupFilter.Evaluate(
                        cls, AdobeNativeMethods.OwnerPid(h), _adobePids, rect, hostRect,
                        AdobeNativeMethods.IsWindowVisible(h))

                    If verdict = AdobePopupVerdict.Accepted Then
                        AdobeNativeMethods.ShowWindow(h, AdobeNativeMethods.SW_HIDE)
                        _HiddenCount += 1
                        Report($"Fereastră plutitoare Adobe {AdobePopupFilter.Label(verdict)} și ascunsă: " &
                               $"0x{h.ToInt64():X} {rect.Width}x{rect.Height} la {rect.X},{rect.Y}.")
                        _seen.Add(h)
                    ElseIf _seen.Add(h) Then
                        ' Rejections are logged ONCE per handle, with the reason — the rule the
                        ' bench learned the hard way: a filter nobody can read is a filter nobody
                        ' can debug after the next Adobe update.
                        Report($"Fereastră plutitoare Adobe {AdobePopupFilter.Label(verdict)}: " &
                               $"0x{h.ToInt64():X} {rect.Width}x{rect.Height}, proces {AdobeNativeMethods.OwnerPid(h)}.")
                    End If
                    Return True
                End Function, IntPtr.Zero)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePopupWatcher.Sweep", ex)
            Throw
        End Try
    End Sub

    Private Sub Report(line As String)
        _log?.Invoke(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            RemoveHandler _timer.Tick, AddressOf OnTick
            _timer.Stop()
            _timer.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePopupWatcher.Dispose", ex)
        End Try
    End Sub

End Class
