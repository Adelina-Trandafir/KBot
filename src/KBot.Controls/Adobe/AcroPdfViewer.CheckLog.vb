Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common

' ACTIVEX-CHECK (slice 0078-15): the operator mark, the tree dumps and the writer of activex_check.log.
' Part of AcroPdfViewer. Kept for a future investigation (operator, 07.10.2026); it writes only while DetailedWatch is
' True (AcroPdfViewer.LightWatch.vb).
Partial Public NotInheritable Class AcroPdfViewer

    ''' <summary>
    ''' ACTIVEX-CHECK: the operator pressed «I see it» on the bench -- the moment the document LOOKS loaded, written with
    ''' the time since the load and the whole tree at that moment (what Ctrl+H will wait for).
    ''' </summary>
    Public Sub MarkOperatorSeen()
        Try
            Check($"+{Elapsed()} ms >>>>> OPERATOR: the document is seen loaded <<<<< form={DescribeForm()} " &
                  $"foreground={Describe(AdobeNativeMethods.GetForegroundWindow())}")
            DumpTree("operator mark")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.MarkOperatorSeen", ex)
        End Try
    End Sub

    Private Sub Milestone(k_what As String)
        Check($"+{Elapsed()} ms MILESTONE: {k_what}")
        DumpTree(k_what)
    End Sub

    ' The whole tree under the control, one line per window, indented by depth.
    Private Sub DumpTree(k_reason As String)
        If Not DetailedWatch Then Return   ' the small watch writes nothing: no tree walk either
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
        Dim k_all As List(Of IntPtr) = AdobeNativeMethods.Descendants(_host.Handle)
        Check($"tree ({k_reason}): {k_all.Count} window(s) under control {HexOf(_host.Handle)}, focus={Describe(AdobeNativeMethods.GetFocus())}")
        For Each k_h As IntPtr In k_all
            Dim k_depth As Integer = 0
            Dim k_p As IntPtr = AdobeNativeMethods.GetParent(k_h)
            While k_p <> IntPtr.Zero AndAlso k_p <> _host.Handle AndAlso k_depth < 30
                k_depth += 1
                k_p = AdobeNativeMethods.GetParent(k_p)
            End While
            Check("   " & New String(" "c, k_depth * 2) & DescribeChild(k_h, AdobeNativeMethods.RectInParent(k_h)))
        Next
    End Sub

    ' The child windows of an Adobe box (its text and buttons), so the log says WHAT it said.
    Private Sub DumpBox(hwnd As IntPtr)
        For Each k_h As IntPtr In AdobeNativeMethods.Descendants(hwnd)
            Dim k_title As String = AdobeNativeMethods.GetTitle(k_h)
            If String.IsNullOrEmpty(k_title) Then Continue For
            Check($"   box child {HexOf(k_h)} class={AdobeNativeMethods.GetClass(k_h)} text=«{k_title}»")
        Next
    End Sub

    Private Function DescribeChild(hwnd As IntPtr, k_rect As Rectangle) As String
        Return $"{HexOf(hwnd)} class={AdobeNativeMethods.GetClass(hwnd)} title=«{AdobeNativeMethods.GetTitle(hwnd)}» " &
               $"rect={k_rect} visible={AdobeNativeMethods.IsWindowVisible(hwnd)} pid={AdobeNativeMethods.OwnerPid(hwnd)}"
    End Function

    ' The host panel: its size, and which sibling is on top of it (the «loading» cover or the panel itself).
    Private Function DescribePanel() As String
        Dim k_parent As Control = _panel.Parent
        Dim k_top As String = "(no parent)"
        If k_parent IsNot Nothing AndAlso k_parent.Controls.Count > 0 Then
            Dim k_first As Control = k_parent.Controls(0)
            k_top = $"{k_first.Name} visible={k_first.Visible}"
        End If
        Return $"panel={_panel.Name} {_panel.ClientSize.Width}x{_panel.ClientSize.Height} visible={_panel.Visible} topSibling={k_top}"
    End Function

    Private Function DescribeForm() As String
        Dim k_form As Form = _panel.FindForm()
        If k_form Is Nothing OrElse Not k_form.IsHandleCreated Then Return "(none)"
        Return $"{HexOf(k_form.Handle)} enabled={AdobeNativeMethods.IsWindowEnabled(k_form.Handle)} state={k_form.WindowState}"
    End Function

    Private Shared Function Describe(hwnd As IntPtr) As String
        If hwnd = IntPtr.Zero Then Return "(none)"
        Return $"{HexOf(hwnd)} class={AdobeNativeMethods.GetClass(hwnd)} title=«{AdobeNativeMethods.GetTitle(hwnd)}» pid={AdobeNativeMethods.OwnerPid(hwnd)}"
    End Function

    Private Function Elapsed() As Long
        Return If(_clock Is Nothing, 0L, _clock.ElapsedMilliseconds)
    End Function

    ' +N = when the event HAPPENED (now minus the time it waited in our queue), relative to the load.
    Private Function Stamp(k_lag As Long) As String
        Return $"+{Math.Max(0L, Elapsed() - k_lag)} ms (handled {k_lag} ms later)"
    End Function

    ' How long ago the event happened, by the system tick count it carries (32-bit, wraps).
    Private Shared Function LagMs(timestamp As UInteger) As Long
        Dim k_now As Long = Environment.TickCount64 And &HFFFFFFFFL
        Dim k_lag As Long = k_now - CLng(timestamp)
        If k_lag < 0 Then k_lag += &H100000000L
        Return k_lag
    End Function

    Private Shared Function EventName(eventType As UInteger) As String
        Select Case eventType
            Case AdobeNativeMethods.EVENT_OBJECT_CREATE : Return "CREATE"
            Case AdobeNativeMethods.EVENT_OBJECT_DESTROY : Return "DESTROY"
            Case AdobeNativeMethods.EVENT_OBJECT_SHOW : Return "SHOW"
            Case EVENT_OBJECT_HIDE : Return "HIDE"
            Case EVENT_OBJECT_FOCUS : Return "FOCUS"
            Case AdobeNativeMethods.EVENT_OBJECT_STATECHANGE : Return "STATECHANGE"
            Case AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE : Return "LOCATIONCHANGE"
            Case Else : Return "0x" & eventType.ToString("X")
        End Select
    End Function

    Private Shared Function HexOf(hwnd As IntPtr) As String
        Return AcroPdfTraceLog.Hex(hwnd)
    End Function

    ' The check lines go to their OWN file, <AppDir>\Logs\activex_check.log, written by a background thread -- never
    ' through AdobeHostLog: its LineWritten listeners (the benches' live log boxes) append every line on the UI thread,
    ' and ~1300 lines into an ever longer TextBox froze the bench (measured 06.10.2026: Adobe's events handled 1.2-1.7 s
    ' late, the operator's clicks slowed down).
    Private Const CheckFileName As String = "activex_check.log"
    Private ReadOnly _checkQueue As New System.Collections.Concurrent.BlockingCollection(Of String)()
    Private _checkWriter As Threading.Thread

    ' Written only while the big watch is the one in use (DetailedWatch); otherwise every check line is dropped here.
    Private Sub Check(k_line As String)
        If Not DetailedWatch Then Return
        Check(k_line, ViewerTag())
    End Sub

    Private Sub CompleteChecks()
        SyncLock _checkQueue
            _checkQueue.CompleteAdding()
        End SyncLock
    End Sub

    ' With the tag given: for the background threads, which must not walk the control tree.
    Private Sub Check(k_line As String, k_tag As String)
        If Not DetailedWatch Then Return
        SyncLock _checkQueue
            If _checkQueue.IsAddingCompleted Then Return
            If _checkWriter Is Nothing Then
                _checkWriter = New Threading.Thread(AddressOf DrainChecks) With {.IsBackground = True, .Name = "AcroPdfViewer check log"}
                _checkWriter.Start()
            End If
            _checkQueue.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  [" & k_tag & "] " & k_line)
        End SyncLock
    End Sub

    ' Background thread: appends the queued lines (in batches) until the viewer is disposed. Must not throw.
    Private Sub DrainChecks()
        Try
            LogPaths.EnsureLogsDirectory()
            Dim k_file As String = LogPaths.Combine(CheckFileName)
            Dim k_batch As New System.Text.StringBuilder()
            For Each k_line As String In _checkQueue.GetConsumingEnumerable()
                k_batch.Clear()
                k_batch.AppendLine(k_line)
                Dim k_more As String = Nothing
                While k_batch.Length < 200000 AndAlso _checkQueue.TryTake(k_more)
                    k_batch.AppendLine(k_more)
                End While
                Try
                    File.AppendAllText(k_file, k_batch.ToString(), New System.Text.UTF8Encoding(True))
                Catch ex As IOException
                    GlobalErrorLog.Write("AcroPdfViewer.DrainChecks", ex)
                End Try
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.DrainChecks", ex)
        End Try
    End Sub

    ' Which view this viewer belongs to (the first ancestor whose type ends in «View»: DdfView, OrdView, NoteCabView).
    Private Function ViewerTag() As String
        Dim k_c As Control = _panel
        While k_c IsNot Nothing
            If k_c.GetType().Name.EndsWith("View", StringComparison.Ordinal) Then Return k_c.GetType().Name
            k_c = k_c.Parent
        End While
        Return "?"
    End Function


End Class
