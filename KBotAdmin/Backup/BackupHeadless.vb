Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading

''' <summary>
''' The periodic local backup (slice ME02-05): <c>KBotAdmin.exe --backup</c> copies every unit database (except 000_DEMO) into the
''' local store with no window, then applies the retention. It needs the key saved on this PC (the passphrase is never asked) and an
''' SSH key without passphrase. Exit code: 0 = all copied, 1 = some database failed, 2 = could not start.
''' </summary>
Friend Module BackupHeadless

    Friend Const Switch As String = "--backup"
    Private Const DemoDatabase As String = "000_DEMO"
    Private ReadOnly LogLock As New Object()

    Friend Function IsRequested(k_args As IEnumerable(Of String)) As Boolean
        For Each k_arg As String In k_args
            If String.Equals(k_arg, Switch, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Friend Function Run() As Integer
        Try
            Return RunAsync().GetAwaiter().GetResult()
        Catch ex As Exception
            GlobalErrorLog.Write("BackupHeadless.Run", ex)
            Log("EROARE: " & ex.Message)
            Return 2
        End Try
    End Function

    Private Async Function RunAsync() As Task(Of Integer)
        Log("Pornește copia locală periodică.")
        Dim k_settings As AdminSettings = AdminSettingsStore.Load()
        If String.IsNullOrWhiteSpace(k_settings.BackupRoot) OrElse Not BackupStore.Exists(k_settings.BackupRoot) Then
            Log("EROARE: dosarul copiilor locale nu e ales sau nu conține o copie. Deschideți KBotAdmin, pagina «Copii locale», și alegeți-l.")
            Return 2
        End If
        Dim k_keys As BackupKeys = BackupKeyring.TryLoad(BackupStore.ReadStoreId(k_settings.BackupRoot))
        If k_keys Is Nothing Then
            Log("EROARE: cheia copiilor nu e păstrată pe acest calculator (sau Windows nu o poate deschide). Deschideți pagina «Copii locale» și bifați păstrarea cheii.")
            Return 2
        End If
        Dim k_store As BackupStore = BackupStore.OpenWithKeys(k_settings.BackupRoot, k_keys)
        Using k_client As New AdminClient(k_settings)
            Await k_client.ConnectAsync().ConfigureAwait(False)
            Dim k_databases As DatabasesResult = Await k_client.CallAsync(Of DatabasesResult)("databases", "--no-counts").ConfigureAwait(False)
            Dim k_names As New List(Of String)()
            For Each k_info As DatabaseInfo In k_databases.Databases
                If k_info.Exists AndAlso Not String.Equals(k_info.Db, DemoDatabase, StringComparison.OrdinalIgnoreCase) Then k_names.Add(k_info.Db)
            Next
            Log("Baze de copiat: " & k_names.Count.ToString(CultureInfo.InvariantCulture))
            Dim k_failed As Integer = 0
            Dim k_snapshots As List(Of SnapshotManifest) = k_store.ListSnapshots().Items
            For Each k_db As String In k_names
                Dim k_mode As String = ChooseMode(k_snapshots, k_db, k_settings.BackupFullEveryDays)
                Try
                    Dim k_summary As BackupRunSummary = Await BackupRunner.RunAsync(k_client, k_store, k_db, "periodic", "periodic", k_mode, AddressOf Log, CancellationToken.None).ConfigureAwait(False)
                    Log(k_db & ": OK (" & k_mode & "), pagini noi " & k_summary.PagesNew.ToString(CultureInfo.InvariantCulture) & ", neschimbate " &
                        k_summary.PagesSame.ToString(CultureInfo.InvariantCulture) & ", " & k_summary.Seconds.ToString("0.#", CultureInfo.InvariantCulture) & " s.")
                Catch ex As Exception
                    k_failed += 1
                    Log(k_db & ": EȘUAT — " & ex.Message)
                End Try
            Next
            Try
                Dim k_plan As BackupRetentionPlan = BackupRetention.Plan(k_store, k_settings.BackupKeepDaily, k_settings.BackupKeepWeekly, k_settings.BackupKeepMonthly, DateTime.Now)
                Dim k_freed As Long = BackupRetention.Apply(k_store, k_plan, AddressOf Log)
                Log("Retenție: " & k_plan.Remove.Count.ToString(CultureInfo.InvariantCulture) & " copii vechi șterse, " & (k_freed \ 1048576L).ToString(CultureInfo.InvariantCulture) & " MB eliberați.")
            Catch ex As Exception
                k_failed += 1
                Log("Retenția a eșuat — " & ex.Message)
            End Try
            Log(If(k_failed = 0, "Gata, fără erori.", "Gata, cu " & k_failed.ToString(CultureInfo.InvariantCulture) & " eșec(uri)."))
            Return If(k_failed = 0, 0, 1)
        End Using
    End Function

    ''' <summary>"full" for a database that has no full copy yet or whose last full one is older than the setting; "fast" otherwise.</summary>
    Friend Function ChooseMode(k_snapshots As IEnumerable(Of SnapshotManifest), k_db As String, k_fullEveryDays As Integer) As String
        Dim k_lastFull As DateTime = DateTime.MinValue
        For Each k_item As SnapshotManifest In k_snapshots
            If String.Equals(k_item.Db, k_db, StringComparison.OrdinalIgnoreCase) AndAlso k_item.Mode = "full" AndAlso k_item.FinishedAt > k_lastFull Then k_lastFull = k_item.FinishedAt
        Next
        If k_lastFull = DateTime.MinValue Then Return "full"
        Return If((DateTime.Now - k_lastFull).TotalDays >= Math.Max(1, k_fullEveryDays), "full", "fast")
    End Function

    Friend Function LogPath() As String
        Return Path.Combine(AppContext.BaseDirectory, "Logs", "copii_locale.log")
    End Function

    Friend Sub Log(k_text As String)
        Try
            SyncLock LogLock
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath()))
                File.AppendAllText(LogPath(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) & "  " & k_text & vbLf, New UTF8Encoding(False))
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("BackupHeadless.Log", ex)
        End Try
    End Sub

End Module

''' <summary>The Windows scheduled task that starts the periodic copy (slice ME02-05). Created and removed only when the operator asks.</summary>
Friend Module BackupScheduler

    Friend Const TaskName As String = "KBotAdmin copii locale"

    Private Function RunTool(k_file As String, k_arguments As String, ByRef k_output As String) As Integer
        Dim k_info As New ProcessStartInfo(k_file, k_arguments) With {
            .UseShellExecute = False, .CreateNoWindow = True, .RedirectStandardOutput = True, .RedirectStandardError = True
        }
        Using k_process As Process = Process.Start(k_info)
            Dim k_out As String = k_process.StandardOutput.ReadToEnd() & k_process.StandardError.ReadToEnd()
            If Not k_process.WaitForExit(60000) Then
                k_process.Kill()
                Throw New BackupException("Programul de planificare Windows nu a răspuns în 60 de secunde.")
            End If
            k_output = k_out
            Return k_process.ExitCode
        End Using
    End Function

    Friend Function IsRegistered() As Boolean
        Try
            Dim k_out As String = String.Empty
            Return RunTool("schtasks.exe", "/Query /TN """ & TaskName & """", k_out) = 0
        Catch ex As Exception
            GlobalErrorLog.Write("BackupScheduler.IsRegistered", ex)
            Return False
        End Try
    End Function

    ''' <summary>Daily at the given time (HH:mm), runs as soon as possible after a missed start, only while this Windows user is signed in.</summary>
    Friend Sub Register(k_time As String)
        Try
            Dim k_exe As String = Application.ExecutablePath.Replace("'", "''")
            Dim k_script As String = "$a = New-ScheduledTaskAction -Execute '" & k_exe & "' -Argument '" & BackupHeadless.Switch & "'; " &
                                     "$t = New-ScheduledTaskTrigger -Daily -At '" & k_time & "'; " &
                                     "$s = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Hours 8); " &
                                     "Register-ScheduledTask -TaskName '" & TaskName & "' -Action $a -Trigger $t -Settings $s -Force | Out-Null"
            Dim k_out As String = String.Empty
            Dim k_code As Integer = RunTool("powershell.exe", "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command """ & k_script.Replace("""", "\""") & """", k_out)
            If k_code <> 0 Then Throw New BackupException("Sarcina nu a putut fi programată în Windows: " & k_out.Trim())
        Catch ex As Exception
            GlobalErrorLog.Write("BackupScheduler.Register", ex)
            Throw
        End Try
    End Sub

    Friend Sub Unregister()
        Try
            Dim k_out As String = String.Empty
            Dim k_code As Integer = RunTool("schtasks.exe", "/Delete /TN """ & TaskName & """ /F", k_out)
            If k_code <> 0 Then Throw New BackupException("Sarcina nu a putut fi ștearsă: " & k_out.Trim())
        Catch ex As Exception
            GlobalErrorLog.Write("BackupScheduler.Unregister", ex)
            Throw
        End Try
    End Sub

End Module
