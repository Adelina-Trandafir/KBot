Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>SLICE-ME01-23. Edits only the selected Markdown entries in the local release journal.</summary>
Friend NotInheritable Class ReleaseNotesFile
    Friend NotInheritable Class Block
        Friend Sub New(k_markdown As String, Optional k_version As Version = Nothing,
                       Optional k_noteText As String = Nothing, Optional k_isHeading As Boolean = False,
                       Optional k_lineEnding As String = Nothing)
            Markdown = k_markdown
            ReleaseVersion = k_version
            NoteText = k_noteText
            IsHeading = k_isHeading
            LineEnding = k_lineEnding
        End Sub

        Friend ReadOnly Property Markdown As String
        Friend ReadOnly Property ReleaseVersion As Version
        Friend ReadOnly Property NoteText As String
        Friend ReadOnly Property IsHeading As Boolean
        Friend ReadOnly Property LineEnding As String
    End Class

    Private Shared ReadOnly Utf8 As New UTF8Encoding(False, True)
    Private ReadOnly _path As String
    Private ReadOnly _newLine As String
    Private ReadOnly _bom As String
    Private _snapshot As String
    Private _blocks As List(Of Block)

    Friend Sub New(k_root As String)
        _path = Path.Combine(k_root, ReleasePreview.NotesFile)
        _snapshot = Utf8.GetString(File.ReadAllBytes(_path))
        _bom = If(_snapshot.StartsWith(ChrW(&HFEFF)), ChrW(&HFEFF).ToString(), String.Empty)
        Dim k_break As Match = Regex.Match(_snapshot, "\r\n|\n|\r")
        _newLine = If(k_break.Success, k_break.Value, vbCrLf)
        _blocks = ReadBlocks(Regex.Split(_snapshot.Substring(_bom.Length), "(\r\n|\n|\r)"))
    End Sub

    Friend ReadOnly Property Versions As IEnumerable(Of Version)
        Get
            Return _blocks.Where(Function(k_block) k_block.IsHeading AndAlso k_block.ReleaseVersion IsNot Nothing).
                Select(Function(k_block) k_block.ReleaseVersion).Distinct()
        End Get
    End Property

    Friend ReadOnly Property Notes As IEnumerable(Of Block)
        Get
            Return _blocks.Where(Function(k_block) k_block.NoteText IsNot Nothing)
        End Get
    End Property

    Friend Function Add(k_version As Version, k_text As String) As Block
        Dim k_note As New Block("- " & k_text, k_version, k_text)
        Dim k_updated As New List(Of Block)(_blocks)
        InsertNotes(k_updated, k_version, {k_note})
        Save(k_updated)
        Return k_note
    End Function

    Friend Function Move(k_selected As IEnumerable(Of Block), k_target As Version) As Integer
        Dim k_selection As New HashSet(Of Block)(k_selected)
        Dim k_moving As List(Of Block) = Notes.Where(Function(k_note) k_selection.Contains(k_note) AndAlso
                                                               k_note.ReleaseVersion <> k_target).ToList()
        If k_moving.Count = 0 Then Return 0
        Dim k_removed As New HashSet(Of Block)(k_moving)
        Dim k_updated As List(Of Block) = _blocks.Where(Function(k_block) Not k_removed.Contains(k_block)).ToList()
        InsertNotes(k_updated, k_target, k_moving.Select(Function(k_note) New Block(k_note.Markdown, k_target, k_note.NoteText,
                                                                                  k_lineEnding:=k_note.LineEnding)))
        Save(k_updated)
        Return k_moving.Count
    End Function

    Private Shared Function ReadBlocks(k_lines As String()) As List(Of Block)
        Dim k_blocks As New List(Of Block)()
        Dim k_version As Version = Nothing
        Dim k_inComment As Boolean = False
        Dim k_i As Integer = 0
        While k_i < k_lines.Length
            Dim k_line As String = k_lines(k_i)
            Dim k_ending As String = If(k_i + 1 < k_lines.Length, k_lines(k_i + 1), Nothing)
            k_i += 2
            If k_inComment OrElse k_line.TrimStart().StartsWith("<!--", StringComparison.Ordinal) Then
                k_inComment = Not k_line.Contains("-->", StringComparison.Ordinal)
                k_blocks.Add(New Block(k_line, k_lineEnding:=k_ending))
            ElseIf Regex.IsMatch(k_line, "^##(?!#)") Then
                Dim k_heading As Match = ReleasePreview.SectionHeading.Match(k_line)
                k_version = If(k_heading.Success, ReleasePreview.Normalize(Version.Parse(k_heading.Groups(1).Value)), Nothing)
                k_blocks.Add(New Block(k_line, k_version, k_isHeading:=True, k_lineEnding:=k_ending))
            Else
                Dim k_bullet As Match = ReleasePreview.BulletLine.Match(k_line)
                If k_version Is Nothing OrElse Not k_bullet.Success Then
                    k_blocks.Add(New Block(k_line, k_lineEnding:=k_ending))
                    Continue While
                End If
                Dim k_markdown As New StringBuilder(k_line)
                Dim k_text As New StringBuilder(k_bullet.Groups(1).Value.Trim())
                While k_i < k_lines.Length AndAlso Regex.IsMatch(k_lines(k_i), "^\s+\S") AndAlso
                      Not k_lines(k_i).TrimStart().StartsWith("<!--", StringComparison.Ordinal) AndAlso
                      Not ReleasePreview.BulletLine.IsMatch(k_lines(k_i))
                    k_markdown.Append(k_ending).Append(k_lines(k_i))
                    k_text.Append(" ").Append(k_lines(k_i).Trim())
                    k_ending = If(k_i + 1 < k_lines.Length, k_lines(k_i + 1), Nothing)
                    k_i += 2
                End While
                k_blocks.Add(New Block(k_markdown.ToString(), k_version, k_text.ToString(), k_lineEnding:=k_ending))
            End If
        End While
        Return k_blocks
    End Function

    Private Shared Sub InsertNotes(k_blocks As List(Of Block), k_version As Version, k_notes As IEnumerable(Of Block))
        ' The release scripts read the first section when a version has duplicate headings.
        Dim k_heading As Integer = k_blocks.FindIndex(Function(k_block) k_block.IsHeading AndAlso k_block.ReleaseVersion = k_version)
        Dim k_insert As Integer
        Dim k_added As New List(Of Block)()
        If k_heading < 0 Then
            k_insert = k_blocks.FindIndex(Function(k_block) k_block.IsHeading AndAlso k_block.ReleaseVersion IsNot Nothing AndAlso
                                                                               k_block.ReleaseVersion < k_version)
            If k_insert < 0 Then k_insert = k_blocks.Count
            k_added.Add(New Block($"## {k_version} ({Date.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)})",
                                  k_version, k_isHeading:=True))
            k_added.Add(New Block("<!-- felii: -->"))
            k_added.Add(New Block(String.Empty))
        Else
            k_insert = k_blocks.FindIndex(k_heading + 1, Function(k_block) k_block.IsHeading)
            If k_insert < 0 Then k_insert = k_blocks.Count
            For k_i As Integer = k_insert - 1 To k_heading + 1 Step -1
                If k_blocks(k_i).NoteText Is Nothing Then Continue For
                k_insert = k_i + 1
                Exit For
            Next
        End If

        ' Reuse the existing blank separator at the end of the section, if present.
        While k_insert > k_heading + 1 AndAlso String.IsNullOrWhiteSpace(k_blocks(k_insert - 1).Markdown)
            k_insert -= 1
        End While
        If k_insert > 0 AndAlso k_blocks(k_insert - 1).NoteText Is Nothing Then k_added.Insert(0, New Block(String.Empty))
        k_added.AddRange(k_notes)
        If k_insert = k_blocks.Count OrElse Not String.IsNullOrWhiteSpace(k_blocks(k_insert).Markdown) Then
            k_added.Add(New Block(String.Empty))
        End If
        k_blocks.InsertRange(k_insert, k_added)
    End Sub

    Private Sub Save(k_updated As List(Of Block))
        Dim k_content As New StringBuilder(_bom)
        For k_i As Integer = 0 To k_updated.Count - 1
            Dim k_block As Block = k_updated(k_i)
            k_content.Append(k_block.Markdown)
            If k_i < k_updated.Count - 1 Then k_content.Append(If(k_block.LineEnding, _newLine))
        Next
        Dim k_text As String = k_content.ToString()
        Dim k_temp As String = _path & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            File.WriteAllText(k_temp, k_text, Utf8)
            ' Keep the source protected from in-place writes between the comparison and replacement.
            Using k_stream As New FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read Or FileShare.Delete)
                Using k_reader As New StreamReader(k_stream, Utf8, detectEncodingFromByteOrderMarks:=False)
                    If Not String.Equals(k_reader.ReadToEnd(), _snapshot, StringComparison.Ordinal) Then
                        Throw New IOException("Fișierul a fost modificat din altă parte. Redeschideți pagina NOUTATI pentru reîncărcare.")
                    End If
                    File.Replace(k_temp, _path, Nothing)
                End Using
            End Using
        Finally
            File.Delete(k_temp)
        End Try
        _snapshot = k_text
        _blocks = k_updated
    End Sub
End Class
