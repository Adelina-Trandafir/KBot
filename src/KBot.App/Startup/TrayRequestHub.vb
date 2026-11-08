Option Strict On
Imports System.IO
Imports System.Diagnostics
Imports System.Threading
Imports KBot.Common

''' <summary>
''' The application's side of the tray application (slice TRAY-01). KBot.Tray writes <see cref="TrayRequest"/> and starts
''' K-BOT; here the request is read, the main window is brought forward and the shell is told what to open
''' (<see cref="KbotForm.RaspundeCereriiDinTray"/>).
'''
''' <list type="bullet">
''' <item>K-BOT was not running: the request is waiting in the file when the main window appears (<see cref="Start"/>).</item>
''' <item>K-BOT was running: the second process only brings the window forward and ends (<c>SingleInstance</c>); the file
''' changes and the watcher wakes this class (an event, not a timer).</item>
''' </list>
''' A request older than <see cref="MaxAge"/> is dropped: nobody answered it, and a window must not open by surprise later.
''' </summary>
Friend NotInheritable Class TrayRequestHub
    Implements IDisposable

    Friend Shared ReadOnly MaxAge As TimeSpan = TimeSpan.FromMinutes(10)

    Private ReadOnly _main As AvacontMain
    Private _watcher As FileSystemWatcher

    Friend Sub New(k_main As AvacontMain)
        ArgumentNullException.ThrowIfNull(k_main)
        _main = k_main
    End Sub

    ''' <summary>Answers the request that is waiting and starts watching for the next ones. UI boundary: logs and swallows.</summary>
    Friend Sub Start()
        Try
            Dim k_dir As String = Path.GetDirectoryName(TrayRequest.FilePath())
            Directory.CreateDirectory(k_dir)
            _watcher = New FileSystemWatcher(k_dir, TrayRequest.FileName) With {
                .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.LastWrite Or NotifyFilters.CreationTime,
                .EnableRaisingEvents = True
            }
            AddHandler _watcher.Created, AddressOf Watcher_Changed
            AddHandler _watcher.Changed, AddressOf Watcher_Changed
            AddHandler _watcher.Renamed, AddressOf Watcher_Changed
            Answer()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequestHub.Start", ex)
        End Try
    End Sub

    ' Raised on a worker thread: handed to the UI thread.
    Private Sub Watcher_Changed(k_sender As Object, e As FileSystemEventArgs)
        Try
            If _main.IsDisposed OrElse Not _main.IsHandleCreated Then Return
            _main.BeginInvoke(New Action(AddressOf Answer))
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequestHub.Watcher_Changed", ex)
        End Try
    End Sub

    ' UI thread.
    Private Sub Answer()
        Try
            Dim k_request As TrayRequest = TrayRequest.Consume(MaxAge)
            If k_request Is Nothing Then Return
            If _main.WindowState = FormWindowState.Minimized Then _main.WindowState = FormWindowState.Normal
            _main.Activate()
            _main.Shell.RaspundeCereriiDinTray(k_request)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequestHub.Answer", ex)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _watcher?.Dispose()
        _watcher = Nothing
    End Sub

    ''' <summary>
    ''' Ends the tray application before an update: sets the event the tray waits on, waits for the process to leave and, if it
    ''' does not within five seconds, ends it. No tray running = nothing to do. The new tray starts with the next sign-in (the
    ''' updater restarts K-BOT). Never throws.
    ''' </summary>
    Friend Shared Sub StopTray()
        Try
            Dim k_event As EventWaitHandle = Nothing
            If Not EventWaitHandle.TryOpenExisting(TrayRequest.ExitEventName, k_event) Then Return
            Using k_event
                k_event.Set()
            End Using
            For Each k_process As Process In Process.GetProcessesByName("KBot.Tray")
                Using k_process
                    Try
                        If Not k_process.WaitForExit(5000) Then
                            GlobalErrorLog.Write("TrayRequestHub.StopTray", New TimeoutException("The tray did not end by itself; ended."))
                            k_process.Kill()
                        End If
                    Catch ex As Exception
                        GlobalErrorLog.Write("TrayRequestHub.StopTray", ex)
                    End Try
                End Using
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequestHub.StopTray", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Starts the tray application (<c>KBot.Tray.exe</c>, next to this program) after the sign-in. It keeps itself single: a
    ''' second start finds the first and ends. Not there (a developer's build folder) = nothing to start. Never throws.
    ''' </summary>
    Friend Shared Sub StartTray()
        Try
            Dim k_exe As String = Path.Combine(AppContext.BaseDirectory, "KBot.Tray.exe")
            If Not File.Exists(k_exe) Then Return
            Process.Start(New ProcessStartInfo(k_exe) With {.UseShellExecute = False, .WorkingDirectory = AppContext.BaseDirectory})?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequestHub.StartTray", ex)
        End Try
    End Sub
End Class
