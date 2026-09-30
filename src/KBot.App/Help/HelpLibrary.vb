Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' Every help topic, read from the <c>Help\</c> folder next to the exe (slice 0000-01).
'''
''' <para>A file that cannot be read (no header, unknown key, duplicate id, parent that does not
''' exist) is logged with its path and left out; the rest of the help still works. Nothing is
''' silently dropped: the log line names the file and the reason.</para>
''' </summary>
Public NotInheritable Class HelpLibrary

    Private Const HeaderFence As String = "---"

    Private ReadOnly _topics As New List(Of HelpTopic)()
    Private ReadOnly _byId As New Dictionary(Of String, HelpTopic)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>The folder the topics were read from.</summary>
    Public ReadOnly Property Root As String

    ''' <summary>Problems found while loading (also written to the error log), one line each.</summary>
    Public ReadOnly Property Problems As New List(Of String)()

    ''' <summary>The guided tours (slice 0000-04), in file order.</summary>
    Public ReadOnly Property Tours As New List(Of HelpTour)()

    Friend Const ToursFolderName As String = "tours"

    ''' <summary>
    ''' Slice 0000-13: the source tag, one line <c>&lt;!-- slice: 0072, 0097 --&gt;</c> under every
    ''' section, naming the slices that decided what the section says. It is for the maintainer
    ''' only: it is taken out when the file is read, so no page, tour bubble, search or exported
    ''' manual ever carries it.
    ''' </summary>
    Friend Shared ReadOnly SourceTagPattern As New Regex("(?m)^[ \t]*<!--\s*slice:[^\n]*?-->[ \t]*(\n|$)", RegexOptions.Compiled)

    ''' <summary>The text without its source tags (see <see cref="SourceTagPattern"/>).</summary>
    Friend Shared Function StripSourceTags(text As String) As String
        Return SourceTagPattern.Replace(text, String.Empty)
    End Function

    Private Shared Function IsTourFile(root As String, file As String) As Boolean
        Dim folder As String = Path.GetFullPath(Path.Combine(root, ToursFolderName)) & Path.DirectorySeparatorChar
        Return Path.GetFullPath(file).StartsWith(folder, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>A tour by id, or Nothing.</summary>
    Public Function FindTour(id As String) As HelpTour
        Return Tours.FirstOrDefault(Function(t) String.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>Every capture tag of every topic, all parts, in manual order (slice 0000-02).</summary>
    Public ReadOnly Property Captures As New List(Of HelpCapture)()

    Private Sub New(root As String)
        Me.Root = root
    End Sub

    ''' <summary>The default folder: <c>&lt;AppDir&gt;\Help</c>.</summary>
    Public Shared Function DefaultRoot() As String
        Return Path.Combine(AppContext.BaseDirectory, "Help")
    End Function

    ''' <summary>Reads every <c>*.md</c> under <paramref name="root"/> except README files.</summary>
    Public Shared Function Load(root As String) As HelpLibrary
        Try
            Dim library As New HelpLibrary(root)
            If Not Directory.Exists(root) Then
                library.Report(root, "the help folder does not exist")
                Return library
            End If

            For Each file As String In Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories).OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase)
                If String.Equals(Path.GetFileName(file), "README.md", StringComparison.OrdinalIgnoreCase) Then Continue For
                ' Slice 0000-04: tours have their own format and are read below.
                If IsTourFile(root, file) Then Continue For
                Try
                    Dim topic As HelpTopic = ParseFile(file)
                    If library._byId.ContainsKey(topic.Id) Then
                        library.Report(file, "duplicate id '" & topic.Id & "' (first in " & library._byId(topic.Id).SourcePath & ")")
                        Continue For
                    End If
                    library._topics.Add(topic)
                    library._byId(topic.Id) = topic
                Catch ex As ArgumentException
                    library.Report(file, ex.Message)
                End Try
            Next

            ' A topic whose parent is missing or in another part would hang nowhere in the tree.
            For Each t As HelpTopic In library._topics.ToList()
                If t.Parent.Length = 0 Then Continue For
                Dim parent As HelpTopic = Nothing
                If Not library._byId.TryGetValue(t.Parent, parent) Then
                    library.Report(t.SourcePath, "parent '" & t.Parent & "' does not exist; shown under the part")
                    t.Parent = String.Empty
                ElseIf parent.Part <> t.Part Then
                    library.Report(t.SourcePath, "parent '" & t.Parent & "' is in another part; shown under the part")
                    t.Parent = String.Empty
                End If
            Next

            ' Slice 0000-04: the guided tours, from <root>\tours\.
            Dim toursFolder As String = Path.Combine(root, ToursFolderName)
            If Directory.Exists(toursFolder) Then
                For Each file As String In Directory.EnumerateFiles(toursFolder, "*.md").OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase)
                    Try
                        Dim tour As HelpTour = HelpTour.ParseFile(file)
                        If library.Tours.Any(Function(x) String.Equals(x.Id, tour.Id, StringComparison.OrdinalIgnoreCase)) Then
                            library.Report(file, "duplicate tour id '" & tour.Id & "'")
                            Continue For
                        End If
                        If tour.TopicId.Length > 0 AndAlso library.Find(tour.TopicId) Is Nothing Then
                            library.Report(file, "tour topic '" & tour.TopicId & "' does not exist; shown only on the start page")
                            tour.TopicId = String.Empty
                        End If
                        library.Tours.Add(tour)
                    Catch ex As ArgumentException
                        library.Report(file, ex.Message)
                    End Try
                Next
            End If

            ' Slice 0000-02: every screenshot the topics ask for, in manual order.
            For Each part As HelpPart In [Enum].GetValues(Of HelpPart)()
                For Each t As HelpTopic In library.InReadingOrder(part)
                    For Each m As Match In HelpCapture.TagPattern.Matches(t.Body)
                        Try
                            Dim c As HelpCapture = HelpCapture.Parse(m.Groups("body").Value)
                            If library.Captures.Any(Function(x) String.Equals(x.Id, c.Id, StringComparison.OrdinalIgnoreCase)) Then
                                library.Report(t.SourcePath, "capture id '" & c.Id & "' is used twice")
                                Continue For
                            End If
                            c.TopicId = t.Id
                            c.TopicTitle = t.Title
                            c.Part = t.Part
                            library.Captures.Add(c)
                        Catch ex As ArgumentException
                            library.Report(t.SourcePath, ex.Message)
                        End Try
                    Next
                Next
            Next
            Return library
        Catch ex As Exception
            GlobalErrorLog.Write("HelpLibrary.Load", ex)
            Throw
        End Try
    End Function

    Private Sub Report(file As String, reason As String)
        Dim line As String = file & ": " & reason
        Problems.Add(line)
        GlobalErrorLog.Write("HelpLibrary.Load", New InvalidDataException(line))
    End Sub

    ''' <summary>
    ''' Splits the header block from the body and reads the keys. Throws
    ''' <see cref="ArgumentException"/> for an unknown key or a missing required one.
    ''' </summary>
    Friend Shared Function ParseFile(file As String) As HelpTopic
        Dim text As String = System.IO.File.ReadAllText(file, Encoding.UTF8)
        Dim topic As HelpTopic = Parse(text)
        topic.SourcePath = file
        Return topic
    End Function

    Friend Shared Function Parse(text As String) As HelpTopic
        Dim lines As String() = text.Replace(vbCrLf, vbLf).Split(ChrW(10))
        If lines.Length = 0 OrElse lines(0).Trim() <> HeaderFence Then
            Throw New ArgumentException("the file does not start with a '---' header block")
        End If

        Dim topic As New HelpTopic()
        Dim i As Integer = 1
        Dim closed As Boolean = False
        While i < lines.Length
            Dim line As String = lines(i).Trim()
            i += 1
            If line = HeaderFence Then
                closed = True
                Exit While
            End If
            If line.Length = 0 OrElse line.StartsWith("#", StringComparison.Ordinal) Then Continue While
            Dim colon As Integer = line.IndexOf(":"c)
            If colon <= 0 Then Throw New ArgumentException("header line without 'key:' -> " & line)
            Dim key As String = line.Substring(0, colon).Trim().ToLowerInvariant()
            Dim value As String = line.Substring(colon + 1).Trim()
            Select Case key
                Case "id" : topic.Id = value
                Case "title" : topic.Title = value
                Case "part" : topic.Part = ParsePart(value)
                Case "order"
                    Dim n As Integer
                    If Not Integer.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
                        Throw New ArgumentException("order is not a number: " & value)
                    End If
                    topic.Order = n
                Case "parent" : topic.Parent = value
                Case "screens" : topic.Screens.AddRange(SplitList(value))
                Case "keywords" : topic.Keywords.AddRange(SplitList(value))
                Case Else : Throw New ArgumentException("unknown header key '" & key & "'")
            End Select
        End While
        If Not closed Then Throw New ArgumentException("the header block is not closed with '---'")
        If topic.Id.Length = 0 Then Throw New ArgumentException("header has no 'id'")
        If topic.Title.Length = 0 Then Throw New ArgumentException("header has no 'title'")

        topic.Body = StripSourceTags(String.Join(vbLf, lines, i, lines.Length - i)).Trim()
        Return topic
    End Function

    Friend Shared Function ParsePart(value As String) As HelpPart
        Select Case value.Trim().ToLowerInvariant()
            Case "contabil" : Return HelpPart.Contabil
            Case "avansat" : Return HelpPart.Avansat
            Case "director" : Return HelpPart.Director
            Case Else : Throw New ArgumentException("unknown part '" & value & "' (contabil / avansat / director)")
        End Select
    End Function

    Private Shared Function SplitList(value As String) As IEnumerable(Of String)
        Return value.Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s.Length > 0)
    End Function

    ' ── Queries ─────────────────────────────────────────────────────────────────

    Public ReadOnly Property Topics As IReadOnlyList(Of HelpTopic)
        Get
            Return _topics
        End Get
    End Property

    ''' <summary>The topic with this id, or Nothing.</summary>
    Public Function Find(id As String) As HelpTopic
        If String.IsNullOrEmpty(id) Then Return Nothing
        Dim t As HelpTopic = Nothing
        _byId.TryGetValue(id, t)
        Return t
    End Function

    ''' <summary>Children of <paramref name="parentId"/> in <paramref name="part"/> (empty id = the part's roots), in order.</summary>
    Public Function Children(part As HelpPart, parentId As String) As List(Of HelpTopic)
        Dim p As String = If(parentId, String.Empty)
        Return _topics.Where(Function(t) t.Part = part AndAlso String.Equals(t.Parent, p, StringComparison.OrdinalIgnoreCase)) _
                      .OrderBy(Function(t) t.Order).ThenBy(Function(t) t.Title, StringComparer.CurrentCulture).ToList()
    End Function

    ''' <summary>Every topic of a part, depth first, in contents order (the manual's order).</summary>
    Public Function InReadingOrder(part As HelpPart) As List(Of HelpTopic)
        Dim result As New List(Of HelpTopic)()
        AddDepthFirst(part, String.Empty, result)
        Return result
    End Function

    Private Sub AddDepthFirst(part As HelpPart, parentId As String, into As List(Of HelpTopic))
        For Each t As HelpTopic In Children(part, parentId)
            into.Add(t)
            AddDepthFirst(part, t.Id, into)
        Next
    End Sub

    ''' <summary>Depth of a topic under its part (0 = root).</summary>
    Public Function Depth(topic As HelpTopic) As Integer
        Dim d As Integer = 0
        Dim cur As HelpTopic = Find(topic.Parent)
        While cur IsNot Nothing AndAlso d < 32
            d += 1
            cur = Find(cur.Parent)
        End While
        Return d
    End Function

    ''' <summary>The first topic, among <paramref name="parts"/>, that lists <paramref name="screenKey"/>.</summary>
    Public Function FindByScreen(screenKey As String, parts As IReadOnlyCollection(Of HelpPart)) As HelpTopic
        For Each part As HelpPart In parts
            For Each t As HelpTopic In InReadingOrder(part)
                If t.Screens.Any(Function(s) String.Equals(s, screenKey, StringComparison.OrdinalIgnoreCase)) Then Return t
            Next
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Topics matching every word of <paramref name="query"/>, best first. Case and diacritics
    ''' are ignored, so «plati» finds «Plăți». A title hit weighs more than a body hit.
    ''' </summary>
    Public Function Search(query As String, parts As IReadOnlyCollection(Of HelpPart)) As List(Of HelpTopic)
        Dim words As String() = Fold(If(query, String.Empty)).Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
        If words.Length = 0 Then Return New List(Of HelpTopic)()
        Dim scored As New List(Of KeyValuePair(Of HelpTopic, Integer))()
        For Each t As HelpTopic In _topics
            If Not parts.Contains(t.Part) Then Continue For
            Dim title As String = Fold(t.Title)
            Dim keys As String = Fold(String.Join(" ", t.Keywords))
            Dim body As String = Fold(t.Body)
            Dim score As Integer = 0
            Dim all As Boolean = True
            For Each w As String In words
                Dim s As Integer = 0
                If title.Contains(w) Then s += 10
                If keys.Contains(w) Then s += 5
                If body.Contains(w) Then s += 1
                If s = 0 Then all = False : Exit For
                score += s
            Next
            If all Then scored.Add(New KeyValuePair(Of HelpTopic, Integer)(t, score))
        Next
        Return scored.OrderByDescending(Function(kv) kv.Value).ThenBy(Function(kv) kv.Key.Title, StringComparer.CurrentCulture) _
                     .Select(Function(kv) kv.Key).ToList()
    End Function

    ''' <summary>Lower case, diacritics removed (ă→a, ș→s, ț→t, î→i, â→a).</summary>
    Friend Shared Function Fold(s As String) As String
        Dim d As String = s.ToLowerInvariant().Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(d.Length)
        For Each c As Char In d
            If CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark Then sb.Append(c)
        Next
        Return sb.ToString().Normalize(NormalizationForm.FormC)
    End Function

End Class
