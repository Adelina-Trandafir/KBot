Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions

Public Enum PushState
    NewOnServer
    Modified
    Identical
    AwaitingGit
End Enum

''' <summary>One local file that is allowed to be pushed, compared with the server.</summary>
Public NotInheritable Class PushFile
    Public Property RelativePath As String = String.Empty
    Public Property LocalFullPath As String = String.Empty
    Public Property RemotePath As String = String.Empty
    Public Property State As PushState
    Public Property LocalMtimeUtc As DateTime
    Public Property RemoteMtimeUtc As DateTime = DateTime.MinValue
    Public Property LocalSize As Long
    Public Property RemoteSize As Long
    Public Property RegisteredSlices As String = String.Empty
    Public Property RegisteredReasons As String = String.Empty
    Public Property DebugHash As String = String.Empty
    Public ReadOnly Property Extension As String
        Get
            Return Path.GetExtension(RelativePath).TrimStart("."c).ToLowerInvariant()
        End Get
    End Property
End Class

''' <summary>What the scan saw for one extension (not counting files an ignore rule skipped).</summary>
Public NotInheritable Class PushExtensionStat
    Public Property Extension As String = String.Empty
    ''' <summary>Local files with this extension that no ignore rule held back.</summary>
    Public Property Found As Integer
    Public Property Accepted As Boolean
    ''' <summary>New or modified ones among them (only counted for accepted extensions).</summary>
    Public Property ToSend As Integer
End Class

''' <summary>How many files one ignore rule held back.</summary>
Public NotInheritable Class PushRuleStat
    Public Property Pattern As String = String.Empty
    Public Property Skipped As Integer
End Class

Public NotInheritable Class PushScan
    Public ReadOnly Property Files As New List(Of PushFile)()
    Public ReadOnly Property Extensions As New List(Of PushExtensionStat)()
    Public ReadOnly Property Rules As New List(Of PushRuleStat)()
    ''' <summary>Files held back by .pushignore.</summary>
    Public Property PushIgnoreSkipped As Integer
    Public Property PushIgnoreRules As Integer
    Public Property LocalRoot As String = String.Empty
    Public Property ObservedFingerprint As String = String.Empty
End Class

''' <summary>
''' The file push of AvacontPush, inside KBotAdmin (slice ME01): compares the local PYTHON folder with the
''' server over SFTP and uploads what the operator ticked. Both methods block; call them from a worker thread.
''' </summary>
Public NotInheritable Class PushService

    Public Const MtimeToleranceSeconds As Integer = 2

    Private Sub New()
    End Sub

    Public Shared Function Scan(k_settings As AdminSettings, k_onLine As Action(Of String)) As PushScan
        Try
            Dim k_localRoot As String = k_settings.EffectiveLocalRoot().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            If k_localRoot = String.Empty OrElse Not Directory.Exists(k_localRoot) Then
                Throw New ApplicationException("Folderul local nu există: " & k_localRoot & ". Setați LocalRoot în admin_settings.json.")
            End If
            Dim k_remoteRoot As String = k_settings.RemoteRoot.TrimEnd("/"c)
            Dim k_result As New PushScan With {.LocalRoot = k_localRoot}
            Dim k_repo As String = DeploymentRegistry.RepositoryRoot(k_localRoot)
            Dim k_records As List(Of DeploymentRecord) = DeploymentRegistry.Read(k_localRoot)
            Dim k_debug As New Dictionary(Of String, DebugDeployment)(StringComparer.Ordinal)
            For Each k_entry As DebugDeployment In DeploymentPublisher.ListDebug(k_settings, k_onLine)
                If Not DeploymentRegistry.SafePath(k_entry.path) OrElse Not Regex.IsMatch(k_entry.sha256, "^[a-f0-9]{64}$") OrElse
                    (k_entry.current_sha256 <> String.Empty AndAlso Not Regex.IsMatch(k_entry.current_sha256, "^[a-f0-9]{64}$")) Then
                    Throw New ApplicationException("Evidență debugging invalidă pe server.")
                End If
                k_debug.Add(k_entry.path, k_entry)
            Next
            Dim k_ignoreFile As PushIgnore = PushIgnore.Load(k_localRoot)
            k_result.PushIgnoreRules = k_ignoreFile.Count
            If k_ignoreFile.Count > 0 Then k_onLine?.Invoke(PushIgnore.FileNameOnly & ": " & k_ignoreFile.Count & " reguli din " & k_ignoreFile.SourcePath)

            Dim k_ruleHits As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For Each k_pattern As String In k_settings.IgnorePatterns
                If Not String.IsNullOrWhiteSpace(k_pattern) Then k_ruleHits(k_pattern.Trim()) = 0
            Next
            Dim k_extFound As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim k_extSend As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim k_accepted As New HashSet(Of String)(
                k_settings.IncludeExtensions.Select(Function(k_e) If(k_e, String.Empty).Trim().TrimStart("."c).ToLowerInvariant()),
                StringComparer.OrdinalIgnoreCase)

            ' One listing of the whole server tree instead of one SFTP stat per file; Nothing = fall back to stat.
            Dim k_listing As Dictionary(Of String, RemoteEntry) = ListRemote(k_settings, k_remoteRoot, k_onLine)

            Using k_sftp As New SftpSession(k_settings)
                k_sftp.Connect()
                For Each k_localFile As String In Directory.EnumerateFiles(k_localRoot, "*", SearchOption.AllDirectories)
                    Dim k_rel As String = k_localFile.Substring(k_localRoot.Length + 1)
                    If k_rel.Replace("\"c, "/"c).Split("/"c).Any(Function(k_part) k_part.Equals(".kbotadmin-deployment", StringComparison.OrdinalIgnoreCase)) Then Continue For
                    ' .pushignore has the last word: a "!" line there can put back a file the fixed list skips.
                    Dim k_ruled As Boolean? = k_ignoreFile.Match(k_rel)
                    If k_ruled.HasValue Then
                        If k_ruled.Value Then
                            k_result.PushIgnoreSkipped += 1
                            Continue For
                        End If
                    Else
                        Dim k_hit As String = FirstMatchingPattern(k_rel, k_settings.IgnorePatterns)
                        If k_hit <> String.Empty Then
                            k_ruleHits(k_hit) = k_ruleHits.GetValueOrDefault(k_hit) + 1
                            Continue For
                        End If
                    End If

                    Dim k_ext As String = Path.GetExtension(k_rel).TrimStart("."c).ToLowerInvariant()
                    k_extFound(k_ext) = k_extFound.GetValueOrDefault(k_ext) + 1
                    ' An explicit !path in .pushignore also includes a runtime asset whose extension is unchecked.
                    Dim k_explicit As Boolean = k_ruled.HasValue AndAlso Not k_ruled.Value
                    If k_ext = String.Empty OrElse (Not k_accepted.Contains(k_ext) AndAlso Not k_explicit) Then Continue For

                    Dim k_relRemote As String = k_rel.Replace("\"c, "/"c)
                    Dim k_remotePath As String = k_remoteRoot & "/" & k_relRemote
                    Dim k_localUtc As DateTime = File.GetLastWriteTimeUtc(k_localFile)
                    Dim k_remoteUtc As DateTime
                    Dim k_remoteSize As Long
                    Dim k_exists As Boolean
                    If k_listing IsNot Nothing Then
                        Dim k_entry As RemoteEntry = Nothing
                        k_exists = k_listing.TryGetValue(k_relRemote, k_entry)
                        If k_exists Then
                            k_remoteUtc = k_entry.MtimeUtc
                            k_remoteSize = k_entry.Size
                        End If
                    Else
                        k_exists = k_sftp.TryGetRemote(k_remotePath, k_remoteUtc, k_remoteSize)
                    End If
                    Dim k_state As PushState
                    If Not k_exists Then
                        k_state = PushState.NewOnServer
                    ElseIf k_localUtc > k_remoteUtc.AddSeconds(MtimeToleranceSeconds) Then
                        k_state = PushState.Modified
                    Else
                        k_state = PushState.Identical
                    End If
                    Dim k_file As New PushFile With {
                        .RelativePath = k_relRemote, .LocalFullPath = k_localFile, .RemotePath = k_remotePath,
                        .State = k_state, .LocalMtimeUtc = k_localUtc, .RemoteMtimeUtc = If(k_exists, k_remoteUtc, DateTime.MinValue),
                        .LocalSize = New FileInfo(k_localFile).Length, .RemoteSize = k_remoteSize
                    }
                    Dim k_debugEntry As DebugDeployment = Nothing
                    If k_debug.TryGetValue(k_relRemote, k_debugEntry) Then
                        k_file.DebugHash = k_debugEntry.sha256
                        ' Pending Git is not a content change. Compare actual bytes, regardless of mtimes.
                        If k_debugEntry.current_sha256 = String.Empty Then
                            k_file.State = PushState.NewOnServer
                        Else
                            Using k_stream As FileStream = File.OpenRead(k_localFile)
                                Dim k_hash As String = Convert.ToHexString(SHA256.HashData(k_stream)).ToLowerInvariant()
                                k_file.State = If(k_hash = k_debugEntry.current_sha256, PushState.AwaitingGit, PushState.Modified)
                            End Using
                        End If
                    End If
                    Dim k_entries As List(Of DeploymentRecord) = DeploymentRegistry.ForFile(k_records, k_repo, k_file)
                    If k_entries.Count > 0 Then
                        Dim k_message As String = DeploymentRegistry.CommitMessage(k_entries)
                        k_file.RegisteredSlices = k_message.Split(ControlChars.Cr)(0)
                        k_file.RegisteredReasons = String.Join(" | ", k_entries.Select(Function(k_entry) k_entry.reason).Distinct().
                            OrderBy(Function(k_reason) k_reason, StringComparer.Create(Globalization.CultureInfo.GetCultureInfo("ro-RO"), True)))
                        ' A failed or interrupted publish can leave matching mtimes but pending records.
                        If k_file.State = PushState.Identical Then k_file.State = PushState.Modified
                    End If
                    If k_file.State = PushState.NewOnServer OrElse k_file.State = PushState.Modified Then
                        k_extSend(k_ext) = k_extSend.GetValueOrDefault(k_ext) + 1
                    End If
                    k_result.Files.Add(k_file)
                Next
                k_result.ObservedFingerprint = String.Empty
            End Using

            ' A server debugging version can be finalized even if the local file is gone or excluded now.
            Dim k_scanned As New HashSet(Of String)(k_result.Files.Select(Function(k_file) k_file.RelativePath), StringComparer.Ordinal)
            For Each k_debugEntry As DebugDeployment In k_debug.Values
                If k_scanned.Contains(k_debugEntry.path) Then Continue For
                Dim k_remoteEntry As RemoteEntry = Nothing
                Dim k_hasRemote As Boolean = k_listing IsNot Nothing AndAlso k_listing.TryGetValue(k_debugEntry.path, k_remoteEntry)
                k_result.Files.Add(New PushFile With {.RelativePath = k_debugEntry.path,
                    .LocalFullPath = Path.Combine(k_localRoot, k_debugEntry.path), .RemotePath = k_remoteRoot & "/" & k_debugEntry.path,
                    .DebugHash = k_debugEntry.sha256, .State = PushState.AwaitingGit,
                    .RemoteSize = If(k_hasRemote, k_remoteEntry.Size, 0),
                    .RemoteMtimeUtc = If(k_hasRemote, k_remoteEntry.MtimeUtc, DateTime.MinValue)})
            Next

            ' Every extension the settings know, plus every one the scan met.
            Dim k_all As New SortedSet(Of String)(k_accepted, StringComparer.OrdinalIgnoreCase)
            For Each k_key As String In k_extFound.Keys
                k_all.Add(k_key)
            Next
            For Each k_ext As String In k_all
                If k_ext = String.Empty Then Continue For
                k_result.Extensions.Add(New PushExtensionStat With {
                    .Extension = k_ext, .Found = k_extFound.GetValueOrDefault(k_ext),
                    .Accepted = k_accepted.Contains(k_ext), .ToSend = k_extSend.GetValueOrDefault(k_ext)})
            Next
            If k_extFound.ContainsKey(String.Empty) Then
                k_result.Extensions.Add(New PushExtensionStat With {.Extension = String.Empty, .Found = k_extFound(String.Empty), .Accepted = False})
            End If
            For Each k_pair As KeyValuePair(Of String, Integer) In k_ruleHits
                k_result.Rules.Add(New PushRuleStat With {.Pattern = k_pair.Key, .Skipped = k_pair.Value})
            Next
            Dim k_sort As StringComparer = StringComparer.Create(Globalization.CultureInfo.GetCultureInfo("ro-RO"), True)
            k_result.Files.Sort(Function(k_a, k_b) k_sort.Compare(k_a.RelativePath, k_b.RelativePath))
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("PushService.Scan", ex)
            Throw
        End Try
    End Function

    Private Structure RemoteEntry
        Public Size As Long
        Public MtimeUtc As DateTime
    End Structure

    ''' <summary>
    ''' Every file under the remote root in ONE ssh command (path relative to the root, size, mtime).
    ''' Nothing when the command cannot run, so the caller falls back to one SFTP stat per file.
    ''' </summary>
    Private Shared Function ListRemote(k_settings As AdminSettings, k_remoteRoot As String, k_onLine As Action(Of String)) As Dictionary(Of String, RemoteEntry)
        Try
            Dim k_root As String = If(k_remoteRoot = String.Empty, "/", k_remoteRoot)
            Dim k_quoted As String = "'" & k_root.Replace("'", "'\''") & "'"
            Dim k_result As SshResult
            Using k_ssh As New SshSession(k_settings)
                k_ssh.Connect()
                k_result = k_ssh.Run("LC_ALL=C find " & k_quoted & " -type f -printf '%P\t%s\t%T@\n'")
            End Using
            If k_result.ExitStatus <> 0 AndAlso k_result.StdOut = String.Empty Then
                k_onLine?.Invoke("Listare server indisponibilă (cod " & k_result.ExitStatus.ToString(Globalization.CultureInfo.InvariantCulture) & "), verificare fișier cu fișier.")
                Return Nothing
            End If
            Dim k_map As New Dictionary(Of String, RemoteEntry)(StringComparer.Ordinal)
            For Each k_line As String In k_result.StdOut.Split(ControlChars.Lf)
                If k_line = String.Empty Then Continue For
                Dim k_parts As String() = k_line.TrimEnd(ControlChars.Cr).Split(ControlChars.Tab)
                If k_parts.Length < 3 Then Continue For
                Dim k_size As Long
                Dim k_epoch As Double
                If Not Long.TryParse(k_parts(1), Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, k_size) Then Continue For
                If Not Double.TryParse(k_parts(2), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, k_epoch) Then Continue For
                k_map(k_parts(0)) = New RemoteEntry With {
                    .Size = k_size,
                    .MtimeUtc = DateTime.SpecifyKind(DateTime.UnixEpoch.AddSeconds(Math.Floor(k_epoch)), DateTimeKind.Utc)}
            Next
            k_onLine?.Invoke("Server: " & k_map.Count.ToString(Globalization.CultureInfo.InvariantCulture) & " fișiere listate.")
            Return k_map
        Catch ex As Exception
            GlobalErrorLog.Write("PushService.ListRemote", ex)
            k_onLine?.Invoke("Listare server eșuată, verificare fișier cu fișier.")
            Return Nothing
        End Try
    End Function

    ''' <summary>Uploads the files; returns how many failed. <paramref name="k_onFile"/> gets (done, file, error or empty).</summary>
    Public Shared Function Upload(k_settings As AdminSettings, k_plan As DeploymentPlan,
                                  k_onFile As Action(Of Integer, PushFile, String), k_onLine As Action(Of String)) As Integer
        Try
            DeploymentPublisher.Publish(k_settings, k_plan,
                Sub(k_line)
                    RunLog.Write(k_line)
                    k_onLine?.Invoke(k_line)
                End Sub)
            For k_i As Integer = 0 To k_plan.Items.Count - 1
                k_onFile?.Invoke(k_i + 1, k_plan.Items(k_i).PushFile, String.Empty)
            Next
            Return 0
        Catch ex As Exception
            GlobalErrorLog.Write("PushService.Upload", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The first pattern that holds the file back, or empty. A pattern is a name or a wildcard
    ''' (<c>*</c> any run of characters, <c>?</c> one character) matched against every path segment, so
    ''' <c>bin</c>, <c>*.pyc</c> and <c>schema_diff*.sql</c> all work; one with a slash is matched against the whole path.
    ''' </summary>
    Private Shared Function FirstMatchingPattern(k_relativePath As String, k_patterns As List(Of String)) As String
        If k_patterns Is Nothing Then Return String.Empty
        Dim k_rel As String = k_relativePath.Replace("\"c, "/"c)
        Dim k_segments As String() = k_rel.Split("/"c)
        For Each k_pattern As String In k_patterns
            Dim k_p As String = If(k_pattern, String.Empty).Trim()
            If k_p = String.Empty Then Continue For
            Dim k_matcher As Regex = WildcardToRegex(k_p.Replace("\"c, "/"c).Trim("/"c))
            If k_p.Contains("/") OrElse k_p.Contains("\") Then
                If k_matcher.IsMatch(k_rel) Then Return k_p
            Else
                For Each k_segment As String In k_segments
                    If k_matcher.IsMatch(k_segment) Then Return k_p
                Next
            End If
        Next
        Return String.Empty
    End Function

    Private Shared ReadOnly WildcardCache As New Dictionary(Of String, Regex)(StringComparer.Ordinal)

    Private Shared Function WildcardToRegex(k_glob As String) As Regex
        SyncLock WildcardCache
            Dim k_found As Regex = Nothing
            If WildcardCache.TryGetValue(k_glob, k_found) Then Return k_found
            Dim k_body As String = "^" & Regex.Escape(k_glob).Replace("\*", ".*").Replace("\?", ".") & "$"
            Dim k_regex As New Regex(k_body, RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
            WildcardCache(k_glob) = k_regex
            Return k_regex
        End SyncLock
    End Function

End Class
