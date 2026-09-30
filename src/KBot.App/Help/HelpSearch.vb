Option Strict On
Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' One search result (slice 0000-18): a section of a topic, with the words around the first
''' match. Used by the «?» popup, the help window and the question log.
''' </summary>
Public NotInheritable Class HelpHit

    Public Sub New(topicId As String, sectionAnchor As String, title As String, sectionTitle As String,
                   snippet As String, score As Integer)
        Me.TopicId = topicId
        Me.SectionAnchor = If(sectionAnchor, String.Empty)
        Me.Title = title
        Me.SectionTitle = If(sectionTitle, String.Empty)
        Me.Snippet = If(snippet, String.Empty)
        Me.Score = score
    End Sub

    Public ReadOnly Property TopicId As String

    ''' <summary>The section's anchor in the topic page; empty = the top of the page.</summary>
    Public ReadOnly Property SectionAnchor As String

    ''' <summary>The topic's title (Romanian).</summary>
    Public ReadOnly Property Title As String

    ''' <summary>The section heading (Romanian); empty for the text before the first heading.</summary>
    Public ReadOnly Property SectionTitle As String

    ''' <summary>A short piece of the section's text around the first match (Romanian).</summary>
    Public ReadOnly Property Snippet As String

    Public ReadOnly Property Score As Integer

    ''' <summary>Slice 0000-19: the topic's <c>open:</c> (<c>view:ddf</c>...); empty = no «Deschide» button.</summary>
    Public Property OpenTarget As String
        Get
            Return _openTarget
        End Get
        Friend Set(value As String)
            _openTarget = If(value, String.Empty)
        End Set
    End Property
    Private _openTarget As String = String.Empty

    ''' <summary>Slice 0000-19: the id of the topic's guided tour; empty = no «Tur ghidat» button.</summary>
    Public Property TourId As String
        Get
            Return _tourId
        End Get
        Friend Set(value As String)
            _tourId = If(value, String.Empty)
        End Set
    End Property
    Private _tourId As String = String.Empty

    ''' <summary>The tour's title (Romanian), for the button's tooltip.</summary>
    Public Property TourTitle As String
        Get
            Return _tourTitle
        End Get
        Friend Set(value As String)
            _tourTitle = If(value, String.Empty)
        End Set
    End Property
    Private _tourTitle As String = String.Empty

    ''' <summary><c>topicId#anchor</c>: what the question log stores (ids only, never text).</summary>
    Public ReadOnly Property Key As String
        Get
            Return If(SectionAnchor.Length = 0, TopicId, TopicId & "#" & SectionAnchor)
        End Get
    End Property

End Class

''' <summary>
''' The words of one piece of text, reduced to stems once when the library loads (slice 0000-18).
''' <see cref="Full"/> holds the stems; <see cref="Near"/> their first four letters, the looser
''' match that scores half.
''' </summary>
Friend NotInheritable Class HelpTermSet

    Public ReadOnly Full As New HashSet(Of String)(StringComparer.Ordinal)
    Public ReadOnly Near As New HashSet(Of String)(StringComparer.Ordinal)

    Public Sub New(foldedText As String)
        For Each w As String In HelpSearch.Words(foldedText)
            If HelpSearch.IsStopWord(w) Then Continue For
            Dim s As String = HelpSearch.Stem(w)
            Full.Add(s)
            Dim n As String = HelpSearch.NearStem(s)
            If n IsNot Nothing Then Near.Add(n)
        Next
    End Sub

    ''' <summary><paramref name="full"/> points for the same stem, <paramref name="half"/> for the same first four letters, else 0.</summary>
    Public Function Weight(term As HelpQueryTerm, full As Integer, half As Integer) As Integer
        If Me.Full.Contains(term.Stem) Then Return full
        If term.Near IsNot Nothing AndAlso Near.Contains(term.Near) Then Return half
        Return 0
    End Function

End Class

''' <summary>One word of a question, as a stem.</summary>
Friend NotInheritable Class HelpQueryTerm
    Public Sub New(stem As String)
        Me.Stem = stem
        Near = HelpSearch.NearStem(stem)
    End Sub
    Public ReadOnly Stem As String
    Public ReadOnly Near As String
End Class

''' <summary>
''' One section of a topic (slice 0000-18): the text before the first <c>## </c> heading, or one
''' <c>## </c> heading with the text under it. Folded and split once, when the library loads.
''' </summary>
Friend NotInheritable Class HelpSection

    ''' <summary>Empty for the text before the first heading; else the heading's anchor.</summary>
    Public Property Anchor As String = String.Empty

    ''' <summary>The heading as the operator reads it; empty for the text before the first heading.</summary>
    Public Property Heading As String = String.Empty

    ''' <summary>The section's text without Markdown marks, whitespace collapsed.</summary>
    Public Property PlainText As String = String.Empty

    ''' <summary><see cref="PlainText"/> folded, one character for one character (the snippet finds its place in it).</summary>
    Public Property FoldedText As String = String.Empty

    Public Property HeadingTerms As HelpTermSet
    Public Property BodyTerms As HelpTermSet

End Class

''' <summary>
''' The search behind the help (slice 0000-18). No model: a question is split into words, the
''' filler words go, every word becomes a stem («trimit» and «trimiterea» meet at «trimi»), and
''' each SECTION of each visible topic is scored by how many words it holds — title above
''' keywords above heading above text. The best sections come back as <see cref="HelpHit"/>.
''' </summary>
Friend NotInheritable Class HelpSearch

    Private Sub New()
    End Sub

    ''' <summary>Hits returned when the caller does not say.</summary>
    Public Const DefaultMaxHits As Integer = 20

    ' A topic may fill at most this many rows, so one long topic does not push out the rest.
    Private Const MaxHitsPerTopic As Integer = 3

    Private Const SnippetLength As Integer = 170
    Private Const SnippetLead As Integer = 50

    ' Weights: same stem / same first four letters.
    Private Const TitleFull As Integer = 10, TitleHalf As Integer = 5
    Private Const KeywordFull As Integer = 6, KeywordHalf As Integer = 3
    Private Const HeadingFull As Integer = 8, HeadingHalf As Integer = 4
    Private Const BodyFull As Integer = 2, BodyHalf As Integer = 1

    ' Stems are cut to this many letters: «trimiterea» and «trimit» both become «trimi».
    Private Const StemLength As Integer = 5
    Private Const NearLength As Integer = 4

    ''' <summary>
    ''' Filler words of a question, folded (no diacritics). The one list: add a word here and
    ''' nowhere else.
    ''' </summary>
    Private Shared ReadOnly StopWords As New HashSet(Of String)(StringComparer.Ordinal) From {
        "a", "al", "ale", "ai", "am", "ar", "as", "are", "au", "asta", "acest", "acesta", "aceasta", "aceste",
        "acum", "aici", "atunci", "avea", "ca", "cand", "care", "cat", "cate", "catre", "ce", "cel", "cea", "cei",
        "cele", "cu", "cum", "da", "daca", "dar", "de", "deci", "despre", "din", "dintr", "doar", "doresc", "dupa",
        "e", "ea", "ei", "el", "este", "eu", "face", "fac", "faci", "facut", "fara", "fi", "fie", "fost", "i", "ii",
        "il", "in", "intr", "intre", "iar", "imi", "la", "le", "li", "lor", "lui", "ma", "mai", "mea", "meu", "mele",
        "mi", "mie", "mod", "ne", "nici", "noi", "nu", "o", "ori", "pe", "pentru", "pot", "poate", "poti", "prin",
        "sa", "sau", "se", "si", "sunt", "spre", "sub", "ta", "tau", "te", "ti", "tu", "un", "una", "unde", "unei",
        "unor", "unui", "va", "vei", "voi", "vor", "vreau", "trebuie", "putea", "niste", "foarte", "asa", "ok"}

    ' Word endings dropped before the cut, longest first (folded Romanian endings).
    Private Shared ReadOnly Endings As String() = {
        "urilor", "ilor", "elor", "ului", "urile", "uri", "ele", "ile", "iei", "ii", "ie", "ea", "ei", "ul",
        "le", "lor", "a", "e", "i", "u"}

    Private Shared ReadOnly HtmlComment As New Regex("<!--.*?-->", RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared ReadOnly MarkdownImage As New Regex("!\[[^\]]*\]\([^)]*\)", RegexOptions.Compiled)
    Private Shared ReadOnly MarkdownLink As New Regex("\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled)
    Private Shared ReadOnly TableRule As New Regex("(?m)^[ \t]*\|?[ \t:|-]*-{3,}[ \t:|-]*\|?[ \t]*$", RegexOptions.Compiled)
    Private Shared ReadOnly LineMarks As New Regex("(?m)^[ \t]*(#{1,6}[ \t]+|>[ \t]?|[-*+][ \t]+|\d+\.[ \t]+)", RegexOptions.Compiled)
    Private Shared ReadOnly Spaces As New Regex("\s+", RegexOptions.Compiled)

    ' ── Text helpers ──────────────────────────────────────────────────────────────

    ''' <summary>Lower case and no diacritics, one character for one character.</summary>
    Public Shared Function Fold(s As String) As String
        If String.IsNullOrEmpty(s) Then Return String.Empty
        Dim sb As New StringBuilder(s.Length)
        For Each c As Char In s
            sb.Append(FoldChar(c))
        Next
        Return sb.ToString()
    End Function

    Private Shared Function FoldChar(c As Char) As Char
        Dim l As Char = Char.ToLowerInvariant(c)
        If AscW(l) < 128 Then Return l
        ' A precomposed letter decomposes to its base letter plus marks: keep the base letter.
        Dim d As String = l.ToString().Normalize(NormalizationForm.FormD)
        If d.Length > 1 AndAlso CharUnicodeInfo.GetUnicodeCategory(d(1)) = UnicodeCategory.NonSpacingMark Then Return d(0)
        Return l
    End Function

    ''' <summary>The runs of letters and digits of a folded text.</summary>
    Public Shared Iterator Function Words(folded As String) As IEnumerable(Of String)
        If String.IsNullOrEmpty(folded) Then Return
        Dim start As Integer = -1
        For i As Integer = 0 To folded.Length
            Dim inWord As Boolean = i < folded.Length AndAlso Char.IsLetterOrDigit(folded(i))
            If inWord Then
                If start < 0 Then start = i
            ElseIf start >= 0 Then
                Yield folded.Substring(start, i - start)
                start = -1
            End If
        Next
    End Function

    Public Shared Function IsStopWord(foldedWord As String) As Boolean
        Return foldedWord.Length < 2 OrElse StopWords.Contains(foldedWord)
    End Function

    ''' <summary>The word without a common ending, cut to <see cref="StemLength"/> letters.</summary>
    Public Shared Function Stem(foldedWord As String) As String
        Dim w As String = foldedWord
        If w.Length > 4 AndAlso Not Char.IsDigit(w(0)) Then
            For Each e As String In Endings
                If w.Length - e.Length >= 3 AndAlso w.EndsWith(e, StringComparison.Ordinal) Then
                    w = w.Substring(0, w.Length - e.Length)
                    Exit For
                End If
            Next
        End If
        Return If(w.Length > StemLength, w.Substring(0, StemLength), w)
    End Function

    ''' <summary>The looser key of a stem (its first four letters), or Nothing for a short stem (exact only).</summary>
    Public Shared Function NearStem(stem As String) As String
        If stem.Length < NearLength Then Return Nothing
        Return stem.Substring(0, NearLength)
    End Function

    ''' <summary>Markdown to the words a reader sees: no tags, no link targets, no marks, one line.</summary>
    Public Shared Function PlainText(markdown As String) As String
        If String.IsNullOrEmpty(markdown) Then Return String.Empty
        Dim t As String = HtmlComment.Replace(markdown, " ")
        t = MarkdownImage.Replace(t, " ")
        t = MarkdownLink.Replace(t, "$1")
        t = TableRule.Replace(t, " ")
        t = LineMarks.Replace(t, String.Empty)
        t = t.Replace("**", String.Empty).Replace("__", String.Empty).Replace("`", String.Empty).Replace("|", " ")
        Return Spaces.Replace(t, " ").Trim()
    End Function

    ''' <summary>
    ''' The anchor of a heading: folded, letters and digits joined by dashes, unique within the
    ''' topic (<paramref name="used"/>). The topic page gives its headings the same ids.
    ''' </summary>
    Public Shared Function AnchorFor(headingPlainText As String, used As HashSet(Of String)) As String
        Dim sb As New StringBuilder()
        For Each w As String In Words(Fold(headingPlainText))
            If sb.Length > 0 Then sb.Append("-"c)
            sb.Append(w)
            If sb.Length >= 48 Then Exit For
        Next
        Dim root As String = If(sb.Length = 0, "sectiune", sb.ToString())
        Dim a As String = root
        Dim n As Integer = 2
        While Not used.Add(a)
            a = root & "-" & n.ToString(CultureInfo.InvariantCulture)
            n += 1
        End While
        Return a
    End Function

    ' ── Index ─────────────────────────────────────────────────────────────────────

    ''' <summary>Splits the topic into sections and reduces title, keywords and sections to stems.</summary>
    Public Shared Sub Index(topic As HelpTopic)
        topic.TitleTerms = New HelpTermSet(Fold(topic.Title))
        topic.KeywordTerms = New HelpTermSet(Fold(String.Join(" ", topic.Keywords)))
        topic.Sections.Clear()

        Dim used As New HashSet(Of String)(StringComparer.Ordinal)
        Dim heading As String = String.Empty
        Dim anchor As String = String.Empty
        Dim text As New StringBuilder()
        For Each raw As String In topic.Body.Replace(vbCrLf, vbLf).Split(ChrW(10))
            If raw.StartsWith("## ", StringComparison.Ordinal) Then
                AddSection(topic, anchor, heading, text.ToString())
                heading = PlainText(raw.Substring(3))
                anchor = AnchorFor(heading, used)
                text.Clear()
            Else
                text.Append(raw).Append(ChrW(10))
            End If
        Next
        AddSection(topic, anchor, heading, text.ToString())
    End Sub

    Private Shared Sub AddSection(topic As HelpTopic, anchor As String, heading As String, markdown As String)
        Dim plain As String = PlainText(markdown)
        ' The text before the first heading is kept even when empty: the title still matches there.
        If anchor.Length > 0 OrElse plain.Length > 0 OrElse topic.Sections.Count = 0 Then
            Dim folded As String = Fold(plain)
            topic.Sections.Add(New HelpSection With {
                .Anchor = anchor, .Heading = heading, .PlainText = plain, .FoldedText = folded,
                .HeadingTerms = New HelpTermSet(Fold(heading)), .BodyTerms = New HelpTermSet(folded)})
        End If
    End Sub

    ' ── Query ─────────────────────────────────────────────────────────────────────

    ''' <summary>The stems of a question, filler words dropped, each once.</summary>
    Public Shared Function QueryTerms(query As String) As List(Of HelpQueryTerm)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim terms As New List(Of HelpQueryTerm)()
        For Each w As String In Words(Fold(If(query, String.Empty)))
            If IsStopWord(w) Then Continue For
            Dim s As String = Stem(w)
            If seen.Add(s) Then terms.Add(New HelpQueryTerm(s))
        Next
        Return terms
    End Function

    Private NotInheritable Class Candidate
        Public Topic As HelpTopic
        Public Section As HelpSection
        Public Score As Integer
        Public OwnWeight As Integer
        Public Matched As List(Of HelpQueryTerm)
        Public Order As Integer
    End Class

    ''' <summary>
    ''' The best sections of the topics in <paramref name="parts"/> for <paramref name="query"/>.
    ''' Score = words matched (first) and their weight (then). With three words or more, a
    ''' section must hold at least half of them.
    ''' </summary>
    Public Shared Function Search(topics As IEnumerable(Of HelpTopic), query As String,
                                  parts As IReadOnlyCollection(Of HelpPart), maxHits As Integer) As List(Of HelpHit)
        Dim result As New List(Of HelpHit)()
        Dim terms As List(Of HelpQueryTerm) = QueryTerms(query)
        If terms.Count = 0 OrElse maxHits <= 0 Then Return result
        Dim minMatched As Integer = If(terms.Count <= 2, 1, (terms.Count + 1) \ 2)

        Dim candidates As New List(Of Candidate)()
        Dim order As Integer = 0
        For Each t As HelpTopic In topics
            If Not parts.Contains(t.Part) OrElse t.TitleTerms Is Nothing Then Continue For
            Dim topicWeight(terms.Count - 1) As Integer
            For i As Integer = 0 To terms.Count - 1
                topicWeight(i) = t.TitleTerms.Weight(terms(i), TitleFull, TitleHalf) +
                                 t.KeywordTerms.Weight(terms(i), KeywordFull, KeywordHalf)
            Next
            For Each s As HelpSection In t.Sections
                order += 1
                Dim matched As New List(Of HelpQueryTerm)()
                Dim weight As Integer = 0
                Dim own As Integer = 0
                For i As Integer = 0 To terms.Count - 1
                    Dim w As Integer = s.HeadingTerms.Weight(terms(i), HeadingFull, HeadingHalf) +
                                       s.BodyTerms.Weight(terms(i), BodyFull, BodyHalf)
                    own += w
                    w += topicWeight(i)
                    If w > 0 Then
                        matched.Add(terms(i))
                        weight += w
                    End If
                Next
                If matched.Count < minMatched Then Continue For
                candidates.Add(New Candidate With {
                    .Topic = t, .Section = s, .OwnWeight = own, .Matched = matched, .Order = order,
                    .Score = matched.Count * 100 + Math.Min(99, weight)})
            Next
        Next

        ' Best first; a topic's extra sections only when they hold a match of their own.
        Dim perTopic As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For Each c As Candidate In candidates.OrderByDescending(Function(x) x.Score).ThenBy(Function(x) x.Order)
            Dim n As Integer = 0
            perTopic.TryGetValue(c.Topic.Id, n)
            If n >= MaxHitsPerTopic OrElse (n > 0 AndAlso c.OwnWeight = 0) Then Continue For
            perTopic(c.Topic.Id) = n + 1
            result.Add(New HelpHit(c.Topic.Id, c.Section.Anchor, c.Topic.Title, c.Section.Heading,
                                   Snippet(c.Topic, c.Section, c.Matched), c.Score))
            If result.Count >= maxHits Then Exit For
        Next
        Return result
    End Function

    ' The words around the first place a matched word starts; the start of the text otherwise.
    Private Shared Function Snippet(topic As HelpTopic, section As HelpSection, matched As List(Of HelpQueryTerm)) As String
        Dim plain As String = section.PlainText
        Dim folded As String = section.FoldedText
        If plain.Length = 0 AndAlso topic.Sections.Count > 1 Then
            plain = topic.Sections(1).PlainText
            folded = topic.Sections(1).FoldedText
        End If
        If plain.Length = 0 Then Return String.Empty

        Dim pos As Integer = -1
        For Each term As HelpQueryTerm In matched
            Dim key As String = If(term.Near, term.Stem)
            Dim p As Integer = WordStart(folded, key)
            If p >= 0 AndAlso (pos < 0 OrElse p < pos) Then pos = p
        Next
        Dim start As Integer = If(pos < 0, 0, Math.Max(0, pos - SnippetLead))
        If start > 0 Then
            Dim space As Integer = plain.IndexOf(" "c, start)
            If space >= 0 AndAlso space < pos Then start = space + 1
        End If
        Dim length As Integer = Math.Min(SnippetLength, plain.Length - start)
        Dim piece As String = plain.Substring(start, length)
        If start + length < plain.Length Then
            Dim cut As Integer = piece.LastIndexOf(" "c)
            If cut > SnippetLength \ 2 Then piece = piece.Substring(0, cut)
            piece &= ChrW(&H2026)
        End If
        If start > 0 Then piece = ChrW(&H2026) & piece
        Return piece
    End Function

    ' The first place key starts a word in folded, or -1.
    Private Shared Function WordStart(folded As String, key As String) As Integer
        Dim from As Integer = 0
        While from < folded.Length
            Dim p As Integer = folded.IndexOf(key, from, StringComparison.Ordinal)
            If p < 0 Then Return -1
            If p = 0 OrElse Not Char.IsLetterOrDigit(folded(p - 1)) Then Return p
            from = p + 1
        End While
        Return -1
    End Function

End Class
