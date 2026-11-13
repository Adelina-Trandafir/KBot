Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' The signer edition updates itself (slice TRAY-02). It is not part of K-BOT's updates: its package and its version are its own
''' (<c>push-tray.ps1</c> -> <c>GET /api/update/tray/latest</c>, <c>GET /api/update/tray</c>).
'''
''' <para>The tray runs from its own folder, so it cannot overwrite the files it is using. Windows lets a program <b>rename</b> a file
''' that is loaded, though: every file of the new version is unpacked beside the old ones, each old file is renamed to
''' <c>*.old</c> and the new one takes its name; then the new tray is started (it waits for this process to end first, see
''' <see cref="WaitForPreviousInstance"/>). The <c>*.old</c> files are deleted at the next start.</para>
''' </summary>
Friend NotInheritable Class TrayUpdater
    Implements IDisposable

    Friend Const WaitPidArg As String = "--wait-pid"

    Private Const StagingFolder As String = "_update"
    Private Const OldSuffix As String = ".old"
    Private Shared ReadOnly CheckEvery As TimeSpan = TimeSpan.FromHours(6)
    Private Shared ReadOnly RetryAfterFailure As TimeSpan = TimeSpan.FromMinutes(30)

    Private ReadOnly _http As HttpClient
    Private ReadOnly _dir As String
    Private _nextCheckUtc As DateTime = DateTime.MinValue

    Friend Sub New(k_dir As String)
        ArgumentException.ThrowIfNullOrWhiteSpace(k_dir)
        _dir = k_dir
        Dim k_options As New ApiOptions()
        k_options.EnsureHttpsBaseUrl()
        _http = New HttpClient() With {.BaseAddress = New Uri(k_options.BaseUrl), .Timeout = TimeSpan.FromMinutes(10)}
    End Sub

    ''' <summary>
    ''' When this process was started by the previous tray after an update, waits for that one to end (it still owns the single-instance
    ''' lock). Gives up after 30 seconds. Never throws.
    ''' </summary>
    Friend Shared Sub WaitForPreviousInstance(k_args As String())
        Try
            If k_args Is Nothing Then Return
            For k_i As Integer = 0 To k_args.Length - 2
                If Not String.Equals(k_args(k_i), WaitPidArg, StringComparison.OrdinalIgnoreCase) Then Continue For
                Dim k_pid As Integer
                If Not Integer.TryParse(k_args(k_i + 1), k_pid) Then Return
                Try
                    Using k_previous As Process = Process.GetProcessById(k_pid)
                        k_previous.WaitForExit(30000)
                    End Using
                Catch ex As ArgumentException
                    ' Already gone: nothing to wait for.
                End Try
                Return
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TrayUpdater.WaitForPreviousInstance", ex)
        End Try
    End Sub

    ''' <summary>Deletes what the last update left behind (<c>*.old</c> files and the unpacking folder). Best effort; never throws.</summary>
    Friend Sub CleanUp()
        Try
            For Each k_file As String In Directory.EnumerateFiles(_dir, "*" & OldSuffix, SearchOption.AllDirectories)
                Try
                    File.Delete(k_file)
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                    ' Still held by the previous process: the next start deletes it.
                End Try
            Next
            Dim k_staging As String = Path.Combine(_dir, StagingFolder)
            If Directory.Exists(k_staging) Then Directory.Delete(k_staging, recursive:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayUpdater.CleanUp", ex)
        End Try
    End Sub

    ''' <summary>
    ''' At most every six hours: asks the server for the published tray and, when it is newer than this one, downloads it and puts it in
    ''' place. True = the new files are in place and the caller must <see cref="Restart"/>. A tray started from a developer's build folder
    ''' never updates. Never throws: a failure is logged and tried again in half an hour.
    ''' </summary>
    Friend Async Function TryUpdateAsync(k_ct As CancellationToken) As Task(Of Boolean)
        Try
            If DateTime.UtcNow < _nextCheckUtc Then Return False
            _nextCheckUtc = DateTime.UtcNow + CheckEvery
            If Autostart.IsDevelopmentFolder(_dir) Then
                TrayLog.Write("update", "skipped: this tray runs from a developer's build folder")
                Return False
            End If

            Dim k_latest As PublishedTray = Await ReadLatestAsync(k_ct).ConfigureAwait(True)
            If k_latest Is Nothing Then
                TrayLog.Write("update", "the server has no published tray (404)")
                Return False
            End If
            If k_latest.Version <= OwnVersion() Then
                TrayLog.Write("update", $"up to date: this {OwnVersion()}, server {k_latest.Version}")
                Return False
            End If
            TrayLog.Write("update", $"new version on the server: {k_latest.Version} (this {OwnVersion()}); downloading")

            Dim k_zip As String = Path.Combine(Path.GetTempPath(), "KBot_Tray_" & k_latest.Version.ToString() & ".zip")
            Try
                Await DownloadAsync(k_zip, k_latest, k_ct).ConfigureAwait(True)
                Dim k_staging As String = Path.Combine(_dir, StagingFolder)
                If Directory.Exists(k_staging) Then Directory.Delete(k_staging, recursive:=True)
                ZipFile.ExtractToDirectory(k_zip, k_staging)
                Dim k_newExe As String = Path.Combine(k_staging, "KBot.Tray.exe")
                If Not File.Exists(k_newExe) Then Throw New InvalidDataException("The tray package has no KBot.Tray.exe.")
                Dim k_inside As Version = Version.Parse(FileVersionInfo.GetVersionInfo(k_newExe).FileVersion)
                If k_inside <> k_latest.Version Then
                    Throw New InvalidDataException($"The tray package says {k_latest.Version} but its KBot.Tray.exe is {k_inside}.")
                End If
                Swap(k_staging)
                TrayLog.Write("update", $"version {k_latest.Version} is in place; restarting")
            Finally
                Try
                    File.Delete(k_zip)
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                    ' A leftover in the temp folder is harmless.
                End Try
            End Try
            Return True
        Catch ex As OperationCanceledException
            Return False
        Catch ex As Exception
            _nextCheckUtc = DateTime.UtcNow + RetryAfterFailure
            GlobalErrorLog.Write("TrayUpdater.TryUpdateAsync", ex)
            Return False
        End Try
    End Function

    ''' <summary>Starts the new tray (which waits for this process to end) . I/O boundary: logs and rethrows.</summary>
    Friend Sub Restart()
        Try
            Dim k_psi As New ProcessStartInfo(Environment.ProcessPath) With {.UseShellExecute = False, .WorkingDirectory = _dir}
            k_psi.ArgumentList.Add(WaitPidArg)
            k_psi.ArgumentList.Add(Environment.ProcessId.ToString(Globalization.CultureInfo.InvariantCulture))
            Process.Start(k_psi)?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayUpdater.Restart", ex)
            Throw
        End Try
    End Sub

    ' ── The server ───────────────────────────────────────────────────────────────

    Private NotInheritable Class PublishedTray
        Friend Version As Version
        Friend Sha256 As String
    End Class

    Private Async Function ReadLatestAsync(k_ct As CancellationToken) As Task(Of PublishedTray)
        Using k_response As HttpResponseMessage = Await _http.GetAsync("/api/update/tray/latest", k_ct).ConfigureAwait(True)
            ' 404 = the tray was never published: nothing to do.
            If k_response.StatusCode = HttpStatusCode.NotFound Then Return Nothing
            k_response.EnsureSuccessStatusCode()
            Dim k_json As String = Await k_response.Content.ReadAsStringAsync(k_ct).ConfigureAwait(True)
            Using k_doc As JsonDocument = JsonDocument.Parse(k_json)
                Dim k_root As JsonElement = k_doc.RootElement
                Dim k_result As New PublishedTray() With {
                    .Version = Version.Parse(k_root.GetProperty("version").GetString()),
                    .Sha256 = k_root.GetProperty("sha256").GetString()}
                Return k_result
            End Using
        End Using
    End Function

    Private Async Function DownloadAsync(k_target As String, k_latest As PublishedTray, k_ct As CancellationToken) As Task
        Using k_response As HttpResponseMessage = Await _http.GetAsync("/api/update/tray", HttpCompletionOption.ResponseHeadersRead, k_ct).ConfigureAwait(True)
            k_response.EnsureSuccessStatusCode()
            Using k_source As Stream = Await k_response.Content.ReadAsStreamAsync(k_ct).ConfigureAwait(True)
                Using k_file As New FileStream(k_target, FileMode.Create, FileAccess.Write, FileShare.None)
                    Await k_source.CopyToAsync(k_file, k_ct).ConfigureAwait(True)
                End Using
            End Using
        End Using
        Using k_read As FileStream = File.OpenRead(k_target)
            Dim k_hash As String = Convert.ToHexString(Await SHA256.HashDataAsync(k_read, k_ct).ConfigureAwait(True))
            If Not String.Equals(k_hash, k_latest.Sha256, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("The downloaded tray package does not match the checksum the server announced.")
            End If
        End Using
    End Function

    ' ── Putting the files in place ───────────────────────────────────────────────

    Private Shared Function OwnVersion() As Version
        Return Version.Parse(FileVersionInfo.GetVersionInfo(Environment.ProcessPath).FileVersion)
    End Function

    ' Every file of the new version takes the name of the old one, which is renamed to *.old. If anything fails half way, the files already
    ' swapped are put back, so the folder is never left with a mix of versions. I/O boundary: the caller logs.
    Private Sub Swap(k_staging As String)
        Dim k_done As New List(Of KeyValuePair(Of String, String))()
        Try
            Dim k_stamp As String = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", Globalization.CultureInfo.InvariantCulture)
            For Each k_new As String In Directory.EnumerateFiles(k_staging, "*", SearchOption.AllDirectories)
                Dim k_relative As String = Path.GetRelativePath(k_staging, k_new)
                Dim k_target As String = Path.Combine(_dir, k_relative)
                Directory.CreateDirectory(Path.GetDirectoryName(k_target))
                Dim k_old As String = Nothing
                If File.Exists(k_target) Then
                    k_old = k_target & "." & k_stamp & OldSuffix
                    File.Move(k_target, k_old)
                End If
                k_done.Add(New KeyValuePair(Of String, String)(k_target, k_old))
                File.Move(k_new, k_target)
            Next
        Catch
            For k_i As Integer = k_done.Count - 1 To 0 Step -1
                Dim k_pair As KeyValuePair(Of String, String) = k_done(k_i)
                Try
                    If File.Exists(k_pair.Key) Then File.Delete(k_pair.Key)
                    If k_pair.Value IsNot Nothing Then File.Move(k_pair.Value, k_pair.Key)
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                    GlobalErrorLog.Write("TrayUpdater.Swap.Rollback", ex)
                End Try
            Next
            Throw
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _http.Dispose()
    End Sub
End Class
