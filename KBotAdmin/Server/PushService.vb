Option Strict On
Imports System.IO

Public Enum PushState
    NewOnServer
    Modified
    Identical
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

            Using k_sftp As New SftpSession(k_settings)
                k_sftp.Connect()
                For Each k_localFile As String In Directory.EnumerateFiles(k_localRoot, "*", SearchOption.AllDirectories)
                    Dim k_rel As String = k_localFile.Substring(k_localRoot.Length + 1)
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
                    If k_ext = String.Empty OrElse Not k_accepted.Contains(k_ext) Then Continue For

                    Dim k_relRemote As String = k_rel.Replace("\"c, "/"c)
                    Dim k_remotePath As String = k_remoteRoot & "/" & k_relRemote
                    Dim k_localUtc As DateTime = File.GetLastWriteTimeUtc(k_localFile)
                    Dim k_remoteUtc As DateTime
                    Dim k_remoteSize As Long
                    Dim k_exists As Boolean = k_sftp.TryGetRemote(k_remotePath, k_remoteUtc, k_remoteSize)
                    Dim k_state As PushState
                    If Not k_exists Then
                        k_state = PushState.NewOnServer
                    ElseIf k_localUtc > k_remoteUtc.AddSeconds(MtimeToleranceSeconds) Then
                        k_state = PushState.Modified
                    Else
                        k_state = PushState.Identical
                    End If
                    If k_state <> PushState.Identical Then k_extSend(k_ext) = k_extSend.GetValueOrDefault(k_ext) + 1
                    k_result.Files.Add(New PushFile With {
                        .RelativePath = k_relRemote, .LocalFullPath = k_localFile, .RemotePath = k_remotePath,
                        .State = k_state, .LocalMtimeUtc = k_localUtc, .RemoteMtimeUtc = If(k_exists, k_remoteUtc, DateTime.MinValue),
                        .LocalSize = New FileInfo(k_localFile).Length, .RemoteSize = k_remoteSize
                    })
                Next
                k_result.ObservedFingerprint = String.Empty
            End Using

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
            k_result.Files.Sort(Function(k_a, k_b) String.Compare(k_a.RelativePath, k_b.RelativePath, StringComparison.OrdinalIgnoreCase))
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("PushService.Scan", ex)
            Throw
        End Try
    End Function

    ''' <summary>Uploads the files; returns how many failed. <paramref name="k_onFile"/> gets (done, file, error or empty).</summary>
    Public Shared Function Upload(k_settings As AdminSettings, k_files As IReadOnlyList(Of PushFile),
                                  k_onFile As Action(Of Integer, PushFile, String)) As Integer
        Try
            Dim k_failed As Integer = 0
            RunLog.Write("Upload: conectare SFTP")
            Using k_sftp As New SftpSession(k_settings)
                k_sftp.Connect()
                RunLog.Write("Upload: conectat, " & k_files.Count.ToString(Globalization.CultureInfo.InvariantCulture) & " fișiere")
                Dim k_dirs As New HashSet(Of String)(StringComparer.Ordinal)
                Dim k_done As Integer = 0
                For Each k_file As PushFile In k_files
                    Dim k_error As String = String.Empty
                    RunLog.Write("Upload: " & k_file.RelativePath & " -> " & k_file.RemotePath)
                    Try
                        k_sftp.UploadFile(k_file.LocalFullPath, k_file.RemotePath, k_settings.PreserveMTime, k_dirs)
                    Catch ex As Exception
                        GlobalErrorLog.Write("PushService.Upload", ex)
                        k_failed += 1
                        k_error = ex.Message
                    End Try
                    k_done += 1
                    RunLog.Write("Upload: gata " & k_file.RelativePath & If(k_error = String.Empty, " OK", " EROARE " & k_error))
                    k_onFile?.Invoke(k_done, k_file, k_error)
                Next
            End Using
            Return k_failed
        Catch ex As Exception
            GlobalErrorLog.Write("PushService.Upload", ex)
            Throw
        End Try
    End Function

    ''' <summary>The first plain pattern (a path segment, or *.ext) that holds the file back, or empty.</summary>
    Private Shared Function FirstMatchingPattern(k_relativePath As String, k_patterns As List(Of String)) As String
        If k_patterns Is Nothing Then Return String.Empty
        Dim k_rel As String = k_relativePath.Replace("\"c, "/"c)
        Dim k_segments As String() = k_rel.Split("/"c)
        For Each k_pattern As String In k_patterns
            Dim k_p As String = If(k_pattern, String.Empty).Trim()
            If k_p = String.Empty Then Continue For
            If k_p.StartsWith("*.", StringComparison.Ordinal) Then
                If k_rel.EndsWith(k_p.Substring(1), StringComparison.OrdinalIgnoreCase) Then Return k_p
            Else
                For Each k_segment As String In k_segments
                    If String.Equals(k_segment, k_p, StringComparison.OrdinalIgnoreCase) Then Return k_p
                Next
            End If
        Next
        Return String.Empty
    End Function

End Class
