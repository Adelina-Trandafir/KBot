Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Text.RegularExpressions

' SLICE-ME01-07. Immutable records let agents register tasks while a publish is running.
Public NotInheritable Class DeploymentRecord
    Public Property id As String = String.Empty
    Public Property file As String = String.Empty
    Public Property slice As String = String.Empty
    Public Property reason As String = String.Empty
    Public Property sha256 As String = String.Empty
    Public Property created_utc As String = String.Empty
    <System.Text.Json.Serialization.JsonIgnore>
    Public Property SourcePath As String = String.Empty
End Class

Public NotInheritable Class DeploymentItem
    Public Property PushFile As PushFile
    Public Property Bytes As Byte()
    Public Property Hash As String = String.Empty
    Public Property MtimeUtc As DateTime
    Public Property Records As New List(Of DeploymentRecord)
End Class

Public NotInheritable Class DeploymentPlan
    Public ReadOnly Property Items As New List(Of DeploymentItem)
    Public Property Message As String = String.Empty
    Public Property GitOnly As Boolean
    Public ReadOnly Property GitCount As Integer
        Get
            Return Items.Where(Function(k_item) k_item.Records.Count > 0).Count()
        End Get
    End Property
End Class

Public NotInheritable Class DebugDeployment
    Public Property path As String = String.Empty
    Public Property sha256 As String = String.Empty
    Public Property current_sha256 As String = String.Empty
    Public Property deployed_utc As String = String.Empty
End Class

Public NotInheritable Class DeploymentRegistry
    Private Shared ReadOnly SlicePattern As New Regex("^SLICE-[A-Z0-9]{4}-[0-9]{2}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly Romanian As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)

    Private Sub New()
    End Sub

    Public Shared Function RepositoryRoot(k_localRoot As String) As String
        Dim k_dir As New DirectoryInfo(Path.GetFullPath(k_localRoot))
        While k_dir IsNot Nothing
            If File.Exists(Path.Combine(k_dir.FullName, "KBot.sln")) Then Return k_dir.FullName
            k_dir = k_dir.Parent
        End While
        Throw New ApplicationException("Nu găsesc KBot.sln deasupra folderului local; nomenclatorul nu poate fi identificat.")
    End Function

    Public Shared Function Read(k_localRoot As String) As List(Of DeploymentRecord)
        Try
            Dim k_dir As String = Path.Combine(RepositoryRoot(k_localRoot), "docs", "deployment", "pending")
            Dim k_result As New List(Of DeploymentRecord)
            If Not Directory.Exists(k_dir) Then Return k_result
            For Each k_path As String In Directory.EnumerateFiles(k_dir, "*.json")
                Dim k_record As DeploymentRecord = JsonSerializer.Deserialize(Of DeploymentRecord)(File.ReadAllText(k_path))
                If k_record Is Nothing OrElse Not Regex.IsMatch(If(k_record.id, ""), "^[a-f0-9]{32}$") OrElse
                   Not String.Equals(Path.GetFileNameWithoutExtension(k_path), k_record.id, StringComparison.Ordinal) OrElse
                   Not SlicePattern.IsMatch(If(k_record.slice, "")) OrElse String.IsNullOrWhiteSpace(k_record.reason) OrElse
                   Not Regex.IsMatch(If(k_record.sha256, ""), "^[a-f0-9]{64}$") OrElse Not SafePath(k_record.file) Then
                    Throw New ApplicationException("Înregistrare invalidă în nomenclator: " & k_path)
                End If
                k_record.SourcePath = k_path
                k_result.Add(k_record)
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentRegistry.Read", ex)
            Throw
        End Try
    End Function

    Public Shared Function SafePath(k_path As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(k_path) AndAlso Not Path.IsPathRooted(k_path) AndAlso
            Not k_path.Contains("\") AndAlso Not k_path.Contains(ChrW(0)) AndAlso
            k_path.Split("/"c).All(Function(k_part) k_part <> "" AndAlso k_part <> "." AndAlso k_part <> ".." AndAlso
                Not k_part.Equals(".git", StringComparison.OrdinalIgnoreCase) AndAlso
                Not k_part.Equals(".kbotadmin-deployment", StringComparison.OrdinalIgnoreCase))
    End Function

    Public Shared Function ForFile(k_records As IEnumerable(Of DeploymentRecord), k_repo As String, k_file As PushFile) As List(Of DeploymentRecord)
        Dim k_rel As String = Path.GetRelativePath(k_repo, k_file.LocalFullPath).Replace("\"c, "/"c)
        If Not SafePath(k_rel) Then Throw New ApplicationException("Fișier în afara proiectului: " & k_file.LocalFullPath)
        Return k_records.Where(Function(k_record) String.Equals(k_record.file, k_rel, StringComparison.OrdinalIgnoreCase)).ToList()
    End Function

    Public Shared Function Prepare(k_settings As AdminSettings, k_files As IReadOnlyList(Of PushFile), Optional k_debugging As Boolean = False) As DeploymentPlan
        Try
            Dim k_root As String = Path.GetFullPath(k_settings.EffectiveLocalRoot()).TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
            Dim k_repo As String = RepositoryRoot(k_root)
            Dim k_records As List(Of DeploymentRecord) = If(k_debugging, New List(Of DeploymentRecord), Read(k_root))
            Dim k_plan As New DeploymentPlan
            Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_file As PushFile In k_files
                If Not SafePath(k_file.RelativePath) OrElse Not k_seen.Add(k_file.RelativePath) OrElse
                   Not String.Equals(Path.GetFullPath(Path.Combine(k_root, k_file.RelativePath)), Path.GetFullPath(k_file.LocalFullPath), StringComparison.OrdinalIgnoreCase) Then
                    Throw New ApplicationException("Cale de publicare invalidă: " & k_file.RelativePath)
                End If
                Dim k_walk As String = Path.GetFullPath(k_file.LocalFullPath)
                While k_walk.Length >= k_root.Length
                    If (File.GetAttributes(k_walk) And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then Throw New ApplicationException("Legătură simbolică neacceptată: " & k_file.RelativePath)
                    k_walk = Path.GetDirectoryName(k_walk)
                End While
                Dim k_entries As List(Of DeploymentRecord) = ForFile(k_records, k_repo, k_file)
                Dim k_mtime As DateTime = File.GetLastWriteTimeUtc(k_file.LocalFullPath)
                Dim k_bytes As Byte() = File.ReadAllBytes(k_file.LocalFullPath)
                Dim k_hash As String = Convert.ToHexString(SHA256.HashData(k_bytes)).ToLowerInvariant()
                If k_entries.Count > 0 AndAlso Not k_entries.Any(Function(k_entry) k_entry.sha256 = k_hash) Then
                    Throw New ApplicationException("Fișier modificat după confirmarea Git: " & k_file.RelativePath & ". Pentru probe, bifați Publicare debugging; pentru Git, confirmați versiunea actuală.")
                End If
                k_plan.Items.Add(New DeploymentItem With {.PushFile = k_file, .Bytes = k_bytes, .Hash = k_hash,
                    .MtimeUtc = k_mtime, .Records = k_entries})
            Next
            If k_plan.Items.Count = 0 Then Throw New ApplicationException("Nu există fișiere de publicat.")
            k_plan.Message = CommitMessage(k_plan.Items.SelectMany(Function(k_item) k_item.Records))
            Return k_plan
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentRegistry.Prepare", ex)
            Throw
        End Try
    End Function

    Public Shared Function PrepareDebugFinalization(k_settings As AdminSettings, k_files As IReadOnlyList(Of PushFile)) As DeploymentPlan
        Try
            Dim k_root As String = k_settings.EffectiveLocalRoot()
            Dim k_repo As String = RepositoryRoot(k_root)
            Dim k_existing As List(Of DeploymentRecord) = Read(k_root)
            Dim k_plan As New DeploymentPlan With {.GitOnly = True}
            Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
            Dim k_missing As New List(Of String)
            For Each k_file As PushFile In k_files
                If Not SafePath(k_file.RelativePath) OrElse Not Regex.IsMatch(k_file.DebugHash, "^[a-f0-9]{64}$") OrElse Not k_seen.Add(k_file.RelativePath) Then
                    Throw New ApplicationException("Selectați numai versiuni publicate debugging: " & k_file.RelativePath)
                End If
                ' Use each file's approved records; never ask for or create replacement metadata here.
                Dim k_entries As List(Of DeploymentRecord) = ForFile(k_existing, k_repo,
                    New PushFile With {.LocalFullPath = Path.Combine(k_root, k_file.RelativePath)})
                If Not k_entries.Any(Function(k_entry) k_entry.sha256 = k_file.DebugHash) Then
                    k_missing.Add(k_file.RelativePath)
                    Continue For
                End If
                k_plan.Items.Add(New DeploymentItem With {.PushFile = k_file, .Hash = k_file.DebugHash, .Records = k_entries})
            Next
            If k_missing.Count > 0 Then
                Throw New ApplicationException("Finalizare oprită: lipsesc informațiile Git aprobate pentru versiunile debugging ale fișierelor:" &
                    vbCrLf & String.Join(vbCrLf, k_missing.OrderBy(Function(k_path) k_path, Romanian)))
            End If
            If k_plan.Items.Count = 0 Then Throw New ApplicationException("Nu sunt bifate fișiere debugging de finalizat.")
            k_plan.Message = CommitMessage(k_plan.Items.SelectMany(Function(k_item) k_item.Records))
            Return k_plan
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentRegistry.PrepareDebugFinalization", ex)
            Throw
        End Try
    End Function

    Public Shared Function CommitMessage(k_records As IEnumerable(Of DeploymentRecord)) As String
        Dim k_all As List(Of DeploymentRecord) = k_records.ToList()
        If k_all.Count = 0 Then Return String.Empty
        Dim k_titles As New List(Of String)
        For Each k_group In k_all.Select(Function(k_record) k_record.slice).Distinct(StringComparer.Ordinal).
            GroupBy(Function(k_slice) k_slice.Substring(0, 10)).OrderBy(Function(k_family) k_family.Key, Romanian)
            Dim k_numbers As List(Of Integer) = k_group.Select(Function(k_slice) Integer.Parse(k_slice.Substring(11), CultureInfo.InvariantCulture)).OrderBy(Function(k_n) k_n).ToList()
            Dim k_title As String = k_group.Key & "-" & k_numbers.First().ToString("00", CultureInfo.InvariantCulture)
            If k_numbers.Count > 1 Then k_title &= "/" & k_numbers.Last().ToString("00", CultureInfo.InvariantCulture)
            k_titles.Add(k_title)
        Next
        Dim k_reasons As List(Of String) = k_all.SelectMany(Function(k_record) k_record.reason.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)).
            Select(Function(k_line) k_line.Trim()).Where(Function(k_line) k_line <> String.Empty).Distinct(StringComparer.Ordinal).
            OrderBy(Function(k_line) k_line, Romanian).ToList()
        Return String.Join("; ", k_titles) & vbCrLf & vbCrLf & String.Join(vbCrLf, k_reasons.Select(Function(k_reason) "- " & k_reason))
    End Function

    Public Shared Sub RemoveCompleted(k_plan As DeploymentPlan)
        Try
            ' Only the immutable records used in this plan; later registrations always have new ids.
            For Each k_record As DeploymentRecord In k_plan.Items.SelectMany(Function(k_item) k_item.Records)
                If File.Exists(k_record.SourcePath) Then File.Delete(k_record.SourcePath)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentRegistry.RemoveCompleted", ex)
            Throw New ApplicationException("Publicarea și commitul au reușit, dar nomenclatorul nu a putut fi curățat: " & ex.Message, ex)
        End Try
    End Sub
End Class
