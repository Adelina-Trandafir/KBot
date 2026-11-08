Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>What the release notes say about one version, as <c>docs\release-notes\NOUTATI.md</c> holds it.</summary>
Public NotInheritable Class ReleaseNotesSection

    Public Sub New(k_found As Boolean, k_heading As String, k_baseline As Boolean, k_lines As IReadOnlyList(Of String))
        Found = k_found
        Heading = k_heading
        IsBaseline = k_baseline
        Lines = k_lines
    End Sub

    ''' <summary>The file has a section for the version.</summary>
    Public ReadOnly Property Found As Boolean

    ''' <summary>The heading as written (<c>1.1.0.7 (01.10.2026)</c>); empty when there is no section.</summary>
    Public ReadOnly Property Heading As String

    ''' <summary>The starting point of the journal: it has no changes to list and counts as written.</summary>
    Public ReadOnly Property IsBaseline As Boolean

    ''' <summary>One entry per change (the <c>- </c> lines, a wrapped line joined to its entry).</summary>
    Public ReadOnly Property Lines As IReadOnlyList(Of String)

    ''' <summary>The section counts as written: it has at least one line, or it is the baseline (the rule of ReleaseNotes.ps1).</summary>
    Public ReadOnly Property IsWritten As Boolean
        Get
            Return Found AndAlso (IsBaseline OrElse Lines.Count > 0)
        End Get
    End Property

End Class

''' <summary>
''' The version a publish / push is about to make, and the notes written for it (KBotAdmin, «Scripturi»). It repeats two things the
''' scripts do, so the page can show them before anything runs: the version step of <c>publish-release.ps1</c> (Step-KBotVersion) and
''' the reading of <c>NOUTATI.md</c> of <c>ReleaseNotes.ps1</c> (a section is <c>## x.y.z.w</c> up to the next <c>## </c>).
''' </summary>
Public NotInheritable Class ReleasePreview

    Private Const ProjectFile As String = "src\KBot.App\KBot.App.vbproj"
    Friend Const NotesFile As String = "docs\release-notes\NOUTATI.md"

    Private Shared ReadOnly FileVersionTag As New Regex("<FileVersion>\s*([0-9.]+)\s*</FileVersion>", RegexOptions.CultureInvariant)
    Friend Shared ReadOnly SectionHeading As New Regex("^##\s+(\d+(?:\.\d+){1,3})(?![\d.])(.*)$", RegexOptions.CultureInvariant)
    Friend Shared ReadOnly BulletLine As New Regex("^\s*[-*]\s+(\S.*)$", RegexOptions.CultureInvariant)

    ' The highest value a part may hold (publish-release.ps1 $VersionCaps); one past it resets the part and carries into the one above.
    Private Shared ReadOnly Caps As Integer() = {Integer.MaxValue, 9, 99, 9}

    ''' <summary>The FileVersion of KBot.App now (four parts), read from its project file.</summary>
    Public Shared Function CurrentVersion(k_root As String) As Version
        Try
            Dim k_path As String = Path.Combine(k_root, ProjectFile)
            Dim k_match As Match = FileVersionTag.Match(File.ReadAllText(k_path, Encoding.UTF8))
            If Not k_match.Success Then Throw New InvalidDataException("Nu găsesc <FileVersion> în " & k_path)
            Return Normalize(Version.Parse(k_match.Groups(1).Value))
        Catch ex As Exception
            GlobalErrorLog.Write("ReleasePreview.CurrentVersion", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The version after raising one part: <paramref name="k_part"/> is 0 (Major) .. 3 (Revision). The parts below it reset to 0; one
    ''' past the cap of a part resets it and carries into the part above (1.9.99.9 + Revision = 2.0.0.0), as publish-release.ps1 does.
    ''' </summary>
    Public Shared Function Bumped(k_version As Version, k_part As Integer) As Version
        If k_part < 0 OrElse k_part > 3 Then Throw New ArgumentOutOfRangeException(NameOf(k_part))
        Dim k_n As Integer() = {k_version.Major, k_version.Minor, k_version.Build, k_version.Revision}
        For k_j As Integer = k_part + 1 To 3
            k_n(k_j) = 0
        Next
        Dim k_i As Integer = k_part
        Do
            k_n(k_i) += 1
            If k_i = 0 OrElse k_n(k_i) <= Caps(k_i) Then Exit Do
            k_n(k_i) = 0
            k_i -= 1
        Loop
        Return New Version(k_n(0), k_n(1), k_n(2), k_n(3))
    End Function

    ''' <summary>The version with its four parts (a missing part is 0), the way the scripts compare versions.</summary>
    Public Shared Function Normalize(k_version As Version) As Version
        Return New Version(Math.Max(k_version.Major, 0), Math.Max(k_version.Minor, 0), Math.Max(k_version.Build, 0), Math.Max(k_version.Revision, 0))
    End Function

    ''' <summary>The section of NOUTATI.md for this version (<see cref="ReleaseNotesSection.Found"/> False when the file has none).</summary>
    Public Shared Function NotesFor(k_root As String, k_version As Version) As ReleaseNotesSection
        Try
            Dim k_path As String = Path.Combine(k_root, NotesFile)
            If Not File.Exists(k_path) Then Return New ReleaseNotesSection(False, String.Empty, False, Array.Empty(Of String)())
            Dim k_all As String() = File.ReadAllText(k_path, Encoding.UTF8).Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
            Dim k_wanted As Version = Normalize(k_version)
            Dim k_i As Integer = 0
            Do While k_i < k_all.Length
                Dim k_match As Match = SectionHeading.Match(k_all(k_i))
                k_i += 1
                If Not k_match.Success OrElse Normalize(Version.Parse(k_match.Groups(1).Value)) <> k_wanted Then Continue Do
                Dim k_heading As String = (k_match.Groups(1).Value & k_match.Groups(2).Value).Trim()
                Dim k_baseline As Boolean = False
                Dim k_lines As New List(Of String)()
                Dim k_open As Boolean = False   ' the previous line was a bullet: an indented line continues it
                Do While k_i < k_all.Length AndAlso Not Regex.IsMatch(k_all(k_i), "^##(?!#)")
                    Dim k_line As String = k_all(k_i)
                    k_i += 1
                    If Regex.IsMatch(k_line, "^\s*<!--") Then
                        k_open = False
                        If Regex.IsMatch(k_line, "release:[^>]*\bbaseline\b") Then k_baseline = True
                        Continue Do
                    End If
                    Dim k_bullet As Match = BulletLine.Match(k_line)
                    If k_bullet.Success Then
                        k_lines.Add(k_bullet.Groups(1).Value.Trim())
                        k_open = True
                    ElseIf k_open AndAlso Regex.IsMatch(k_line, "^\s+\S") Then
                        k_lines(k_lines.Count - 1) = k_lines(k_lines.Count - 1) & " " & k_line.Trim()
                    Else
                        k_open = False
                    End If
                Loop
                Return New ReleaseNotesSection(True, k_heading, k_baseline, k_lines)
            Loop
            Return New ReleaseNotesSection(False, String.Empty, False, Array.Empty(Of String)())
        Catch ex As Exception
            GlobalErrorLog.Write("ReleasePreview.NotesFor", ex)
            Throw
        End Try
    End Function

    ''' <summary>The text the page shows for a section: one «•» line per change, as the update box shows them.</summary>
    Public Shared Function SectionText(k_section As ReleaseNotesSection) As String
        If k_section.Lines.Count = 0 Then Return String.Empty
        Return String.Join(vbCrLf, k_section.Lines.Select(Function(k_line) ChrW(&H2022) & " " & k_line))
    End Function


End Class
