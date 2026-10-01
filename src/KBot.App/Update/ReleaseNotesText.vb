Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>What one paragraph of the release notes is.</summary>
Friend Enum NoteBlockKind
    ''' <summary>The big bold line on top.</summary>
    Title
    ''' <summary>A bold line over a group of bullets (a version).</summary>
    Heading
    ''' <summary>One change: a bullet.</summary>
    Bullet
    ''' <summary>A plain paragraph.</summary>
    Plain
    ''' <summary>A little vertical space.</summary>
    Gap
End Enum

''' <summary>One paragraph of the release notes.</summary>
Friend Structure NoteBlock
    Public Kind As NoteBlockKind
    Public Text As String

    Public Sub New(kind As NoteBlockKind, text As String)
        Me.Kind = kind
        Me.Text = If(text, String.Empty)
    End Sub
End Structure

''' <summary>One version of <c>NOUTATI.md</c>: its number, its date and its changes.</summary>
Friend NotInheritable Class ReleaseSection
    Public Property Version As String = String.Empty
    Public Property Dated As String = String.Empty
    Public ReadOnly Property Changes As New List(Of String)()
End Class

''' <summary>
''' The release notes as the operator reads them (01.10.2026): the update offer, «Istoric versiuni»
''' in Setari and «Ce e nou?» in the help popup all write the same way -- a bold title or version,
''' one bullet per change -- into a <see cref="RichTextBox"/> that wraps the text to its width.
''' The history comes from <c>NOUTATI.md</c>, which ships next to the program.
''' </summary>
Friend NotInheritable Class ReleaseNotesText

    Private Sub New()
    End Sub

    ''' <summary>How many versions «Ce e nou?» shows.</summary>
    Friend Const RecentVersions As Integer = 3

    Private Shared ReadOnly HeadingPattern As New Regex("^##\s+(\S+)\s*(?:\((.*?)\))?\s*$", RegexOptions.Compiled)

    ' ── The text of an update offer ──────────────────────────────────────────

    ''' <summary>The server's notes (lines «- ...») as paragraphs.</summary>
    Friend Shared Function Parse(notes As String) As List(Of NoteBlock)
        Dim blocks As New List(Of NoteBlock)()
        For Each raw As String In If(notes, String.Empty).Split(ControlChars.Lf)
            Dim t As String = raw.Trim()
            If t.Length = 0 Then Continue For
            If t.StartsWith("- ", StringComparison.Ordinal) OrElse t.StartsWith("* ", StringComparison.Ordinal) Then
                blocks.Add(New NoteBlock(NoteBlockKind.Bullet, t.Substring(2).Trim()))
            ElseIf t.StartsWith("#", StringComparison.Ordinal) Then
                blocks.Add(New NoteBlock(NoteBlockKind.Heading, t.TrimStart("#"c).Trim()))
            Else
                blocks.Add(New NoteBlock(NoteBlockKind.Plain, t))
            End If
        Next
        Return blocks
    End Function

    ''' <summary>Headline (first line = title), the notes, then the closing lines.</summary>
    Friend Shared Function Offer(headline As String, notes As String, footer As String) As List(Of NoteBlock)
        Dim blocks As New List(Of NoteBlock)()
        Dim first As Boolean = True
        For Each line As String In If(headline, String.Empty).Split(ControlChars.Lf)
            If line.Trim().Length = 0 Then Continue For
            blocks.Add(New NoteBlock(If(first, NoteBlockKind.Title, NoteBlockKind.Plain), line.Trim()))
            first = False
        Next
        Dim changes As List(Of NoteBlock) = Parse(notes)
        If changes.Count > 0 Then
            blocks.Add(New NoteBlock(NoteBlockKind.Gap, String.Empty))
            blocks.Add(New NoteBlock(NoteBlockKind.Heading, "Ce s-a schimbat"))
            blocks.AddRange(changes)
        End If
        If Not String.IsNullOrWhiteSpace(footer) Then
            blocks.Add(New NoteBlock(NoteBlockKind.Gap, String.Empty))
            For Each line As String In footer.Split(ControlChars.Lf)
                If line.Trim().Length > 0 Then blocks.Add(New NoteBlock(NoteBlockKind.Plain, line.Trim()))
            Next
        End If
        Return blocks
    End Function

    ' ── The history (NOUTATI.md) ─────────────────────────────────────────────

    ''' <summary>Where the shipped history is.</summary>
    Friend Shared ReadOnly Property HistoryPath As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, "Noutati", "NOUTATI.md")
        End Get
    End Property

    ''' <summary>
    ''' The versions of <c>NOUTATI.md</c> that list changes, newest first. A version without a
    ''' list (the starting point of the journal) is left out. Throws when the file cannot be read.
    ''' </summary>
    Friend Shared Function LoadHistory(path As String) As List(Of ReleaseSection)
        Try
            Dim result As New List(Of ReleaseSection)()
            Dim current As ReleaseSection = Nothing
            For Each raw As String In File.ReadAllLines(path)
                Dim t As String = raw.Trim()
                Dim m As Match = HeadingPattern.Match(t)
                If m.Success Then
                    current = New ReleaseSection With {.Version = m.Groups(1).Value, .Dated = m.Groups(2).Value}
                    result.Add(current)
                ElseIf current IsNot Nothing AndAlso t.StartsWith("- ", StringComparison.Ordinal) Then
                    current.Changes.Add(t.Substring(2).Trim())
                End If
            Next
            result.RemoveAll(Function(s) s.Changes.Count = 0)
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("ReleaseNotesText.LoadHistory", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The paragraphs of the last <paramref name="count"/> versions (0 = all). A history that is
    ''' missing or unreadable becomes one plain line, not an exception: the page must still open.
    ''' </summary>
    Friend Shared Function HistoryBlocks(count As Integer) As List(Of NoteBlock)
        Dim blocks As New List(Of NoteBlock)()
        Try
            Dim path As String = HistoryPath
            If Not File.Exists(path) Then
                blocks.Add(New NoteBlock(NoteBlockKind.Plain, "Istoricul versiunilor nu a fost găsit lângă program."))
                Return blocks
            End If
            Dim sections As List(Of ReleaseSection) = LoadHistory(path)
            If count > 0 Then sections = sections.Take(count).ToList()
            If sections.Count = 0 Then
                blocks.Add(New NoteBlock(NoteBlockKind.Plain, "Nu există încă o listă de schimbări."))
                Return blocks
            End If
            For Each s As ReleaseSection In sections
                blocks.Add(New NoteBlock(NoteBlockKind.Heading,
                    "Versiunea " & s.Version & If(s.Dated.Length > 0, "  ·  " & s.Dated, String.Empty)))
                For Each c As String In s.Changes
                    blocks.Add(New NoteBlock(NoteBlockKind.Bullet, c))
                Next
                blocks.Add(New NoteBlock(NoteBlockKind.Gap, String.Empty))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ReleaseNotesText.HistoryBlocks", ex)
            blocks.Clear()
            blocks.Add(New NoteBlock(NoteBlockKind.Plain, "Istoricul versiunilor nu a putut fi citit."))
        End Try
        Return blocks
    End Function

    ' ── Writing into the box ─────────────────────────────────────────────────

    ''' <summary>
    ''' Replaces the content of <paramref name="box"/> with the paragraphs. Fonts come from the box's
    ''' own font and colours from its ForeColor, so the theme (dark or not) and the text size apply;
    ''' call it once the box has its final font (after Load). The box wraps, so no horizontal bar.
    ''' </summary>
    Friend Shared Sub Fill(box As RichTextBox, blocks As IEnumerable(Of NoteBlock))
        Try
            Dim zoom As Double = box.DeviceDpi / 96.0
            Dim baseFont As Font = box.Font
            box.WordWrap = True
            box.Clear()
            box.BulletIndent = CInt(Math.Round(16 * zoom))
            Using title As New Font(baseFont.FontFamily, baseFont.Size * 1.2F, FontStyle.Bold)
                Using bold As New Font(baseFont, FontStyle.Bold)
                    Using small As New Font(baseFont.FontFamily, Math.Max(4.0F, baseFont.Size * 0.5F))
                        For Each b As NoteBlock In blocks
                            Dim start As Integer = box.TextLength
                            Dim text As String = b.Text & ControlChars.Lf
                            box.AppendText(text)
                            box.Select(start, text.Length)
                            box.SelectionBullet = (b.Kind = NoteBlockKind.Bullet)
                            box.SelectionIndent = If(b.Kind = NoteBlockKind.Bullet, CInt(Math.Round(4 * zoom)), 0)
                            Select Case b.Kind
                                Case NoteBlockKind.Title : box.SelectionFont = title
                                Case NoteBlockKind.Heading : box.SelectionFont = bold
                                Case NoteBlockKind.Gap : box.SelectionFont = small
                                Case Else : box.SelectionFont = baseFont
                            End Select
                        Next
                    End Using
                End Using
            End Using
            box.Select(0, 0)
            box.ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("ReleaseNotesText.Fill", ex)
            Throw
        End Try
    End Sub

End Class
