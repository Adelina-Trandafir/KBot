Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>One markdown file of the project, with what the history page needs to place it: its slice, its dates and its links.</summary>
Public NotInheritable Class MdFile

    ''' <summary>Path from the repository root, forward slashes.</summary>
    Public Property RelPath As String = String.Empty

    Public Property Name As String = String.Empty

    ''' <summary>The first «# » heading, or the file name.</summary>
    Public Property Title As String = String.Empty

    ''' <summary>The family the operator filters by (K-BOT, VERCON, ADECHIT, AVACONT, Ajutor, ...).</summary>
    Public Property Family As String = String.Empty

    ''' <summary>The slice a worklog belongs to (<c>0000</c>, <c>AV01</c>, <c>VC06</c>), or empty for a file that is not a worklog.</summary>
    Public Property SliceKey As String = String.Empty

    ''' <summary>The sub-slice number of a worklog (<c>126</c>), or empty.</summary>
    Public Property SubKey As String = String.Empty

    ''' <summary>The lane / cluster of a file with no slice (a folder of the project).</summary>
    Public Property Area As String = String.Empty

    ''' <summary>What kind of file: Fisa (worklog), Registru (status), Plan, Ajutor, Document.</summary>
    Public Property Kind As String = String.Empty

    Public Property Bytes As Long

    ''' <summary>The start of the file as tooltip markup (see <see cref="MdPreview"/>).</summary>
    Public Property Preview As String = String.Empty

    ''' <summary>Every day this file was touched (committed, or changed on disk and not yet committed), oldest first.</summary>
    Public ReadOnly Property Touches As New List(Of Date)()

    Public ReadOnly Property FirstDate As Date
        Get
            Return If(Touches.Count = 0, Date.MinValue, Touches(0))
        End Get
    End Property

    Public ReadOnly Property LastDate As Date
        Get
            Return If(Touches.Count = 0, Date.MinValue, Touches(Touches.Count - 1))
        End Get
    End Property

    ''' <summary>Files this one points to (written links first, then the implied one to its registry).</summary>
    Public ReadOnly Property LinksOut As New List(Of MdFile)()

    ''' <summary>Files that point to this one.</summary>
    Public ReadOnly Property LinksIn As New List(Of MdFile)()

    ''' <summary>The lane key: the slice for a worklog, the folder for the rest.</summary>
    Public ReadOnly Property GroupKey As String
        Get
            Return If(SliceKey.Length > 0, "s:" & SliceKey, "a:" & Area)
        End Get
    End Property

    ''' <summary>«0000-126» for a sub-slice worklog, else the file name.</summary>
    Public ReadOnly Property ShortName As String
        Get
            If SliceKey.Length > 0 AndAlso SubKey.Length > 0 Then Return SliceKey & "-" & SubKey
            If SliceKey.Length > 0 Then Return SliceKey
            Return Name
        End Get
    End Property

End Class

''' <summary>A link from one md to another. <see cref="Implied"/> = not written in the file; derived from the registry index.</summary>
Public NotInheritable Class MdLink
    Public Property FromFile As MdFile
    Public Property ToFile As MdFile
    Public Property Implied As Boolean
End Class

''' <summary>A slice (or a folder cluster) with the files that belong to it.</summary>
Public NotInheritable Class MdGroup
    Public Property Key As String = String.Empty
    Public Property Caption As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Family As String = String.Empty
    Public Property IsSlice As Boolean
    Public ReadOnly Property Files As New List(Of MdFile)()

    Public ReadOnly Property FirstDate As Date
        Get
            Dim k_min As Date = Date.MaxValue
            For Each k_file As MdFile In Files
                If k_file.Touches.Count > 0 AndAlso k_file.FirstDate < k_min Then k_min = k_file.FirstDate
            Next
            Return If(k_min = Date.MaxValue, Date.MinValue, k_min)
        End Get
    End Property

    Public ReadOnly Property LastDate As Date
        Get
            Dim k_max As Date = Date.MinValue
            For Each k_file As MdFile In Files
                If k_file.LastDate > k_max Then k_max = k_file.LastDate
            Next
            Return k_max
        End Get
    End Property
End Class

''' <summary>
''' Finds the registry (state) file a slice is recorded in. The index of KBOT_STATUS.md names some slices exactly and others only by family
''' («AV» for AV01), so the state files are also matched by their own names: <c>KBOT_STATUS_AV.md</c>, <c>KBOT_STATUS_0120-0129.md</c>,
''' <c>ADECHIT_STATUS_ADE0-ADE9.md</c>.
''' </summary>
Friend NotInheritable Class StateResolver

    Private ReadOnly _indexed As Dictionary(Of String, MdFile)
    Private ReadOnly _singles As New Dictionary(Of String, MdFile)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _ranges As New List(Of Tuple(Of String, Integer, Integer, MdFile))()
    Private ReadOnly _vercon As MdFile
    Private ReadOnly _adechit As MdFile

    Private Shared ReadOnly StateName As New Regex("^(?:KBOT|ADECHIT)_STATUS_(?:([A-Za-z]*)(\d+)-([A-Za-z]*)(\d+)|([A-Za-z0-9]+))\.md$", RegexOptions.Compiled)

    Friend Sub New(k_corpus As MdCorpus, k_byPath As Dictionary(Of String, MdFile), k_indexed As Dictionary(Of String, MdFile))
        _indexed = k_indexed
        For Each k_file As MdFile In k_corpus.Files
            If Not k_file.RelPath.StartsWith("docs/worklog/state/", StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim k_match As Match = StateName.Match(k_file.Name)
            If Not k_match.Success Then Continue For
            If k_match.Groups(5).Success Then
                _singles(k_match.Groups(5).Value) = k_file
            ElseIf String.Equals(k_match.Groups(1).Value, k_match.Groups(3).Value, StringComparison.OrdinalIgnoreCase) Then
                _ranges.Add(Tuple.Create(k_match.Groups(1).Value, Integer.Parse(k_match.Groups(2).Value, CultureInfo.InvariantCulture), Integer.Parse(k_match.Groups(4).Value, CultureInfo.InvariantCulture), k_file))
            End If
        Next
        k_byPath.TryGetValue("VERCON/VERCON_STATUS.md", _vercon)
        k_byPath.TryGetValue("ADECHIT_STATUS.md", _adechit)
    End Sub

    Friend Function Find(k_key As String) As MdFile
        Dim k_file As MdFile = Nothing
        If _indexed.TryGetValue(k_key, k_file) OrElse _singles.TryGetValue(k_key, k_file) Then Return k_file

        Dim k_letters As String = New String(k_key.TakeWhile(Function(k_c) Char.IsLetter(k_c)).ToArray())
        Dim k_digits As String = k_key.Substring(k_letters.Length)
        If k_letters.Length > 0 AndAlso k_digits.All(Function(k_c) Char.IsDigit(k_c)) AndAlso k_digits.Length > 0 Then
            If _singles.TryGetValue(k_letters, k_file) Then Return k_file
            Dim k_number As Integer = Integer.Parse(k_digits, CultureInfo.InvariantCulture)
            For Each k_range In _ranges
                If String.Equals(k_range.Item1, k_letters, StringComparison.OrdinalIgnoreCase) AndAlso k_number >= k_range.Item2 AndAlso k_number <= k_range.Item3 Then Return k_range.Item4
            Next
            If k_letters = "VC" Then Return _vercon
            If k_letters = "AD" Then Return _adechit
        ElseIf k_letters.Length = 0 AndAlso k_digits.Length > 0 AndAlso k_digits.All(Function(k_c) Char.IsDigit(k_c)) Then
            Dim k_number As Integer = Integer.Parse(k_digits, CultureInfo.InvariantCulture)
            For Each k_range In _ranges
                If k_range.Item1.Length = 0 AndAlso k_number >= k_range.Item2 AndAlso k_number <= k_range.Item3 Then Return k_range.Item4
            Next
        End If
        Return Nothing
    End Function

End Class

''' <summary>Everything the scan found: the files, the links between them and the groups.</summary>
Public NotInheritable Class MdCorpus
    Public Property Root As String = String.Empty
    Public Property Source As String = String.Empty
    Public ReadOnly Property Files As New List(Of MdFile)()
    Public ReadOnly Property Links As New List(Of MdLink)()
    Public ReadOnly Property Groups As New List(Of MdGroup)()
    Public Property MinDate As Date = Date.MinValue
    Public Property MaxDate As Date = Date.MinValue
End Class

''' <summary>
''' Reads every markdown file of the repository and works out, from the files alone, when each one appeared and changed, which slice it
''' belongs to and what it links to. The dates come from git (one <c>git log</c> over the md files); with no git the file times are used.
''' Nothing is written anywhere.
''' </summary>
Public Module MdScanner

    Private ReadOnly WorklogName As New Regex("^SLICE-([A-Za-z0-9]+)(?:-(\d{1,3}[a-z]?))?(?:-|\.md)", RegexOptions.Compiled)
    Private ReadOnly LinkPattern As New Regex("\]\(\s*<?([^)\s>#]+?\.md)(?:#[^)\s>]*)?>?\s*\)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
    Private ReadOnly IndexRow As New Regex("^\|\s*([A-Za-z0-9-]+)\s*\|\s*(.*?)\s*\|.*\]\((state/[^)\s]+\.md)\)\s*\|?\s*$", RegexOptions.Compiled)
    Private ReadOnly SkipFolders As String() = {"/bin/", "/obj/", "/node_modules/", "/.venv/", "/_Admin/", "/.git/"}

    ''' <summary>The repository root: the folder holding KBot.sln, looked for upwards from the program folder and from the folder pushed to the server.</summary>
    Public Function FindRoot(k_hint As String) As String
        Try
            For Each k_start As String In New String() {AppContext.BaseDirectory, k_hint}
                If String.IsNullOrWhiteSpace(k_start) Then Continue For
                Dim k_dir As DirectoryInfo = Nothing
                Try
                    k_dir = New DirectoryInfo(k_start)
                Catch ex As ArgumentException
                    Continue For
                End Try
                While k_dir IsNot Nothing
                    If File.Exists(Path.Combine(k_dir.FullName, "KBot.sln")) Then Return k_dir.FullName
                    k_dir = k_dir.Parent
                End While
            Next
            Return String.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("MdScanner.FindRoot", ex)
            Throw
        End Try
    End Function

    ''' <summary>Scans the repository at <paramref name="k_root"/>. Slow (git and a few hundred files): run it off the UI thread.</summary>
    Public Function Scan(k_root As String) As MdCorpus
        Try
            If String.IsNullOrWhiteSpace(k_root) OrElse Not Directory.Exists(k_root) Then
                Throw New AdminException("Nu găsesc dosarul proiectului (KBot.sln).")
            End If
            Dim k_corpus As New MdCorpus With {.Root = k_root}
            Dim k_byPath As New Dictionary(Of String, MdFile)(StringComparer.OrdinalIgnoreCase)

            Dim k_paths As List(Of String) = ListPaths(k_root, k_corpus)
            For Each k_rel As String In k_paths
                Dim k_full As String = Path.Combine(k_root, k_rel.Replace("/"c, Path.DirectorySeparatorChar))
                If Not File.Exists(k_full) Then Continue For
                Dim k_file As New MdFile With {.RelPath = k_rel, .Name = Path.GetFileName(k_rel)}
                Classify(k_file)
                k_file.Bytes = New FileInfo(k_full).Length
                k_corpus.Files.Add(k_file)
                k_byPath(k_rel) = k_file
            Next

            AddDates(k_root, k_corpus, k_byPath)
            ReadContents(k_root, k_corpus, k_byPath)
            AddImpliedLinks(k_corpus, k_byPath)
            BuildGroups(k_corpus, k_byPath)

            For Each k_file As MdFile In k_corpus.Files
                If k_file.Touches.Count = 0 Then Continue For
                If k_corpus.MinDate = Date.MinValue OrElse k_file.FirstDate < k_corpus.MinDate Then k_corpus.MinDate = k_file.FirstDate
                If k_file.LastDate > k_corpus.MaxDate Then k_corpus.MaxDate = k_file.LastDate
            Next
            Return k_corpus
        Catch ex As Exception
            GlobalErrorLog.Write("MdScanner.Scan", ex)
            Throw
        End Try
    End Function

    ' ------------------------------------------------------------------
    ' Which files
    ' ------------------------------------------------------------------

    Private Function ListPaths(k_root As String, k_corpus As MdCorpus) As List(Of String)
        Dim k_output As String = RunGit(k_root, "ls-files", "--cached", "--others", "--exclude-standard", "--", "*.md")
        If k_output IsNot Nothing Then
            k_corpus.Source = "git"
            Return k_output.Split({ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries).
                Select(Function(k_line) k_line.Trim()).Where(Function(k_line) k_line.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        End If
        k_corpus.Source = "fișierele de pe disc, fără git"
        Dim k_result As New List(Of String)()
        For Each k_full As String In Directory.EnumerateFiles(k_root, "*.md", SearchOption.AllDirectories)
            Dim k_rel As String = Path.GetRelativePath(k_root, k_full).Replace("\"c, "/"c)
            Dim k_probe As String = "/" & k_rel & "/"
            If SkipFolders.Any(Function(k_skip) k_probe.Contains(k_skip, StringComparison.OrdinalIgnoreCase)) Then Continue For
            k_result.Add(k_rel)
        Next
        Return k_result
    End Function

    ''' <summary>Runs git in the repository and returns its standard output, or <c>Nothing</c> when git is missing or fails.</summary>
    Private Function RunGit(k_root As String, ParamArray k_args As String()) As String
        Try
            Dim k_info As New ProcessStartInfo("git") With {
                .WorkingDirectory = k_root,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .StandardOutputEncoding = Encoding.UTF8
            }
            k_info.ArgumentList.Add("-c")
            k_info.ArgumentList.Add("core.quotepath=false")
            For Each k_arg As String In k_args
                k_info.ArgumentList.Add(k_arg)
            Next
            Using k_process As Process = Process.Start(k_info)
                Dim k_error As Task(Of String) = k_process.StandardError.ReadToEndAsync()
                Dim k_text As String = k_process.StandardOutput.ReadToEnd()
                k_process.WaitForExit()
                k_error.Wait()
                If k_process.ExitCode <> 0 Then Return Nothing
                Return k_text
            End Using
        Catch ex As ComponentModel.Win32Exception
            ' git is not installed / not on PATH: the caller falls back to file times.
            Return Nothing
        End Try
    End Function

    ' ------------------------------------------------------------------
    ' Slice, family, area
    ' ------------------------------------------------------------------

    Private Sub Classify(k_file As MdFile)
        Dim k_match As Match = WorklogName.Match(k_file.Name)
        Dim k_inWorklog As Boolean = k_file.RelPath.StartsWith("docs/worklog/", StringComparison.OrdinalIgnoreCase)
        If k_inWorklog AndAlso k_match.Success Then
            k_file.SliceKey = k_match.Groups(1).Value.ToUpperInvariant()
            k_file.SubKey = k_match.Groups(2).Value
            k_file.Kind = "Fisa"
            k_file.Family = FamilyOfSlice(k_file.SliceKey)
            Return
        End If

        Dim k_segments As String() = k_file.RelPath.Split("/"c)
        k_file.Area = AreaOf(k_segments)
        If k_file.Name.Contains("_STATUS", StringComparison.OrdinalIgnoreCase) Then
            k_file.Kind = "Registru"
        ElseIf k_file.RelPath.Contains("/HelpContent/", StringComparison.OrdinalIgnoreCase) Then
            k_file.Kind = "Ajutor"
        ElseIf k_file.Name.StartsWith("PLAN", StringComparison.OrdinalIgnoreCase) OrElse k_file.Name.EndsWith("_Plan.md", StringComparison.OrdinalIgnoreCase) Then
            k_file.Kind = "Plan"
        Else
            k_file.Kind = "Document"
        End If
        k_file.Family = FamilyOfPath(k_file.RelPath)
    End Sub

    Private Function FamilyOfSlice(k_key As String) As String
        If k_key.StartsWith("VC", StringComparison.Ordinal) Then Return "VERCON"
        If k_key.StartsWith("AD", StringComparison.Ordinal) Then Return "ADECHIT"
        If k_key.StartsWith("AV", StringComparison.Ordinal) Then Return "AVACONT"
        If k_key.StartsWith("ME", StringComparison.Ordinal) Then Return "KBotAdmin"
        If k_key = "00EF" OrElse k_key.StartsWith("EV", StringComparison.Ordinal) Then Return "E-Factura"
        If k_key = "000T" Then Return "Tutoriale"
        If k_key = "0000" Then Return "Ajutor"
        If k_key.StartsWith("D", StringComparison.Ordinal) AndAlso k_key.Length = 4 Then Return "DDF"
        If k_key.All(Function(k_c) Char.IsDigit(k_c)) OrElse k_key = "KE" Then Return "K-BOT"
        Return "Altele"
    End Function

    Private Function FamilyOfPath(k_rel As String) As String
        If k_rel.Contains("/HelpContent/", StringComparison.OrdinalIgnoreCase) Then Return "Ajutor"
        If k_rel.StartsWith("VERCON/", StringComparison.OrdinalIgnoreCase) Then Return "VERCON"
        If k_rel.StartsWith("ADECHIT", StringComparison.OrdinalIgnoreCase) Then Return "ADECHIT"
        If k_rel.StartsWith("KBotAdmin/", StringComparison.OrdinalIgnoreCase) Then Return "KBotAdmin"
        If k_rel.StartsWith("docs/access-map/avacont", StringComparison.OrdinalIgnoreCase) Then Return "AVACONT"
        If k_rel.StartsWith("docs/access-map/efactura", StringComparison.OrdinalIgnoreCase) Then Return "E-Factura"
        If k_rel.StartsWith("PYTHON/", StringComparison.OrdinalIgnoreCase) Then Return "Server"
        If k_rel.StartsWith("docs/", StringComparison.OrdinalIgnoreCase) Then Return "K-BOT"
        If k_rel.StartsWith("src/", StringComparison.OrdinalIgnoreCase) Then Return "K-BOT"
        Return "Altele"
    End Function

    Private Function AreaOf(k_segments As String()) As String
        If k_segments.Length = 1 Then Return "(rădăcină)"
        Dim k_joined As String = String.Join("/", k_segments)
        Dim k_help As Integer = Array.FindIndex(k_segments, Function(k_s) String.Equals(k_s, "HelpContent", StringComparison.OrdinalIgnoreCase))
        If k_help >= 0 Then
            Return If(k_segments.Length > k_help + 2, "Ajutor/" & k_segments(k_help + 1), "Ajutor")
        End If
        Return String.Join("/", k_segments.Take(Math.Min(2, k_segments.Length - 1)))
    End Function

    ' ------------------------------------------------------------------
    ' Dates
    ' ------------------------------------------------------------------

    Private Sub AddDates(k_root As String, k_corpus As MdCorpus, k_byPath As Dictionary(Of String, MdFile))
        Dim k_log As String = If(k_corpus.Source = "git", RunGit(k_root, "log", "--name-only", "--no-renames", "--format=@@%ad", "--date=short", "--", "*.md"), Nothing)
        If k_log IsNot Nothing Then
            Dim k_day As Date = Date.MinValue
            For Each k_raw As String In k_log.Split({ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
                Dim k_line As String = k_raw.Trim()
                If k_line.Length = 0 Then Continue For
                If k_line.StartsWith("@@", StringComparison.Ordinal) Then
                    If Not Date.TryParseExact(k_line.Substring(2), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, k_day) Then k_day = Date.MinValue
                    Continue For
                End If
                Dim k_file As MdFile = Nothing
                If k_day <> Date.MinValue AndAlso k_byPath.TryGetValue(k_line, k_file) AndAlso Not k_file.Touches.Contains(k_day) Then k_file.Touches.Add(k_day)
            Next

            ' Changed on disk and not committed yet: the day of the change counts too.
            Dim k_status As String = RunGit(k_root, "status", "--porcelain", "--", "*.md")
            If k_status IsNot Nothing Then
                For Each k_raw As String In k_status.Split({ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
                    If k_raw.Length < 4 Then Continue For
                    Dim k_rel As String = k_raw.Substring(3).Trim()
                    Dim k_arrow As Integer = k_rel.IndexOf(" -> ", StringComparison.Ordinal)
                    If k_arrow >= 0 Then k_rel = k_rel.Substring(k_arrow + 4)
                    Dim k_file As MdFile = Nothing
                    If k_byPath.TryGetValue(k_rel.Trim(""""c), k_file) Then
                        Dim k_changed As Date = File.GetLastWriteTime(Path.Combine(k_root, k_file.RelPath.Replace("/"c, Path.DirectorySeparatorChar))).Date
                        If Not k_file.Touches.Contains(k_changed) Then k_file.Touches.Add(k_changed)
                    End If
                Next
            End If
        End If

        For Each k_file As MdFile In k_corpus.Files
            If k_file.Touches.Count = 0 Then
                Dim k_full As String = Path.Combine(k_root, k_file.RelPath.Replace("/"c, Path.DirectorySeparatorChar))
                k_file.Touches.Add(File.GetCreationTime(k_full).Date)
                Dim k_written As Date = File.GetLastWriteTime(k_full).Date
                If Not k_file.Touches.Contains(k_written) Then k_file.Touches.Add(k_written)
            End If
            k_file.Touches.Sort()
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' Titles and written links
    ' ------------------------------------------------------------------

    Private Sub ReadContents(k_root As String, k_corpus As MdCorpus, k_byPath As Dictionary(Of String, MdFile))
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_file As MdFile In k_corpus.Files
            Dim k_full As String = Path.Combine(k_root, k_file.RelPath.Replace("/"c, Path.DirectorySeparatorChar))
            Dim k_text As String
            Try
                k_text = File.ReadAllText(k_full, Encoding.UTF8)
            Catch ex As IOException
                ' A file locked by an editor right now: it keeps its name as title and has no links this time.
                GlobalErrorLog.Write("MdScanner.ReadContents", ex)
                k_file.Title = k_file.Name
                Continue For
            End Try

            k_file.Title = k_file.Name
            k_file.Preview = MdPreview.Build(k_text)
            Dim k_lines As Integer = 0
            For Each k_line As String In k_text.Split({ControlChars.Lf}, 40, StringSplitOptions.None)
                k_lines += 1
                If k_line.StartsWith("# ", StringComparison.Ordinal) Then
                    k_file.Title = k_line.Substring(2).Trim()
                    Exit For
                End If
                If k_lines >= 30 Then Exit For
            Next

            Dim k_dir As String = Path.GetDirectoryName(k_file.RelPath.Replace("/"c, Path.DirectorySeparatorChar))
            For Each k_match As Match In LinkPattern.Matches(k_text)
                Dim k_target As String = k_match.Groups(1).Value
                If k_target.Contains("://", StringComparison.Ordinal) OrElse k_target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) Then Continue For
                k_target = Uri.UnescapeDataString(k_target)
                Dim k_resolved As String
                Try
                    k_resolved = Path.GetFullPath(Path.Combine(k_root, If(k_dir, String.Empty), k_target.Replace("/"c, Path.DirectorySeparatorChar)))
                Catch ex As ArgumentException
                    Continue For
                End Try
                Dim k_rel As String = Path.GetRelativePath(k_root, k_resolved).Replace("\"c, "/"c)
                Dim k_other As MdFile = Nothing
                If Not k_byPath.TryGetValue(k_rel, k_other) OrElse k_other Is k_file Then Continue For
                If Not k_seen.Add(k_file.RelPath & ">" & k_other.RelPath) Then Continue For
                k_file.LinksOut.Add(k_other)
                k_other.LinksIn.Add(k_file)
                k_corpus.Links.Add(New MdLink With {.FromFile = k_file, .ToFile = k_other, .Implied = False})
            Next
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' Registry index: slice -> its state file
    ' ------------------------------------------------------------------

    Private Sub AddImpliedLinks(k_corpus As MdCorpus, k_byPath As Dictionary(Of String, MdFile))
        Dim k_index As MdFile = Nothing
        If Not k_byPath.TryGetValue("docs/worklog/KBOT_STATUS.md", k_index) Then Return
        Dim k_indexed As Dictionary(Of String, MdFile) = SliceRegistry(k_corpus.Root, k_byPath).Item1
        Dim k_resolver As New StateResolver(k_corpus, k_byPath, k_indexed)
        Dim k_seen As New HashSet(Of String)(k_corpus.Links.Select(Function(k_l) k_l.FromFile.RelPath & ">" & k_l.ToFile.RelPath), StringComparer.OrdinalIgnoreCase)
        For Each k_file As MdFile In k_corpus.Files
            If k_file.Kind <> "Fisa" Then Continue For
            Dim k_state As MdFile = k_resolver.Find(k_file.SliceKey)
            If k_state Is Nothing OrElse k_state Is k_file Then Continue For
            If Not k_seen.Add(k_file.RelPath & ">" & k_state.RelPath) Then Continue For
            k_file.LinksOut.Add(k_state)
            k_state.LinksIn.Add(k_file)
            k_corpus.Links.Add(New MdLink With {.FromFile = k_file, .ToFile = k_state, .Implied = True})
        Next
    End Sub

    ''' <summary>Reads the slice index of KBOT_STATUS.md: for each slice, its state file and its name.</summary>
    Private Function SliceRegistry(k_root As String, k_byPath As Dictionary(Of String, MdFile)) As Tuple(Of Dictionary(Of String, MdFile), Dictionary(Of String, String))
        Dim k_files As New Dictionary(Of String, MdFile)(StringComparer.OrdinalIgnoreCase)
        Dim k_names As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim k_path As String = Path.Combine(k_root, "docs", "worklog", "KBOT_STATUS.md")
        If Not File.Exists(k_path) Then Return Tuple.Create(k_files, k_names)
        For Each k_line As String In File.ReadLines(k_path, Encoding.UTF8)
            If Not k_line.StartsWith("|", StringComparison.Ordinal) Then Continue For
            Dim k_match As Match = IndexRow.Match(k_line)
            If Not k_match.Success Then Continue For
            Dim k_slice As String = k_match.Groups(1).Value.ToUpperInvariant()
            Dim k_state As MdFile = Nothing
            If k_byPath.TryGetValue("docs/worklog/" & k_match.Groups(3).Value, k_state) Then k_files(k_slice) = k_state
            k_names(k_slice) = CleanName(k_match.Groups(2).Value)
        Next
        Return Tuple.Create(k_files, k_names)
    End Function

    Private Function CleanName(k_raw As String) As String
        Dim k_text As String = Regex.Replace(k_raw, "[*_`]", String.Empty).Trim()
        Dim k_colon As Integer = k_text.IndexOf(":"c)
        If k_colon >= 3 AndAlso k_colon <= 60 Then k_text = k_text.Substring(0, k_colon)
        Return If(k_text.Length > 60, k_text.Substring(0, 60).TrimEnd() & "...", k_text)
    End Function

    ' ------------------------------------------------------------------
    ' Groups
    ' ------------------------------------------------------------------

    Private Sub BuildGroups(k_corpus As MdCorpus, k_byPath As Dictionary(Of String, MdFile))
        Dim k_names As Dictionary(Of String, String) = SliceRegistry(k_corpus.Root, k_byPath).Item2
        Dim k_groups As New Dictionary(Of String, MdGroup)(StringComparer.Ordinal)
        For Each k_file As MdFile In k_corpus.Files
            Dim k_key As String = k_file.GroupKey
            Dim k_group As MdGroup = Nothing
            If Not k_groups.TryGetValue(k_key, k_group) Then
                k_group = New MdGroup With {.Key = k_key, .Family = k_file.Family, .IsSlice = k_file.SliceKey.Length > 0}
                If k_group.IsSlice Then
                    Dim k_name As String = Nothing
                    If Not k_names.TryGetValue(k_file.SliceKey, k_name) Then
                        ' The index lists «AV» where the worklogs say «AV01»: the letters alone name the family of slices.
                        Dim k_letters As String = New String(k_file.SliceKey.TakeWhile(Function(k_c) Char.IsLetter(k_c)).ToArray())
                        If k_letters.Length > 0 Then k_names.TryGetValue(k_letters, k_name)
                    End If
                    k_group.Caption = k_file.SliceKey
                    k_group.Title = If(String.IsNullOrEmpty(k_name), "Felia " & k_file.SliceKey, "Felia " & k_file.SliceKey & " - " & k_name)
                Else
                    k_group.Caption = k_file.Area
                    k_group.Title = "Folder " & k_file.Area
                End If
                k_groups(k_key) = k_group
                k_corpus.Groups.Add(k_group)
            End If
            k_group.Files.Add(k_file)
        Next
        k_corpus.Groups.Sort(Function(k_a, k_b)
                                 If k_a.IsSlice <> k_b.IsSlice Then Return If(k_a.IsSlice, -1, 1)
                                 Dim k_cmp As Integer = k_a.FirstDate.CompareTo(k_b.FirstDate)
                                 Return If(k_cmp <> 0, k_cmp, String.Compare(k_a.Caption, k_b.Caption, StringComparison.Ordinal))
                             End Function)
    End Sub

End Module
