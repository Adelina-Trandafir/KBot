Imports System.Linq
Imports System.Text.RegularExpressions

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WflLine — reprezintă o linie parsată dintr-un fișier .wfl             │
' │                                                                         │
' │  Instanțiată prin WflLine.Parse(rawText, lineIndex, inComment, ...)    │
' │  Stocată în _lineCache As List(Of WflLine) în form.                    │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm
    Public Class WflLine

        ' ── Identitate ──────────────────────────────────────────────────────────
        Public Property LineIndex As Integer
        Public Property RawText As String = ""

        ' ── Tip linie ───────────────────────────────────────────────────────────
        Public Property IsEmpty As Boolean = False
        Public Property IsComment As Boolean = False              ' <!-- ... -->
        Public Property IsInsideMultilineComment As Boolean = False
        Public Property IsXmlDeclaration As Boolean = False       ' <?xml ... ?>
        Public Property IsClosingTag As Boolean = False           ' </Tag>
        Public Property IsSelfClosing As Boolean = False          ' <Tag ... />
        Public Property IsOpeningTag As Boolean = False           ' <Tag ...>

        ' ── Conținut ────────────────────────────────────────────────────────────
        ''' <summary>Numele tag-ului (Nothing dacă linia nu e tag).</summary>
        Public Property TagName As String = Nothing

        ''' <summary>Atributele găsite pe linie: cheie=numeAttr, valoare=valoarea (poate fi "").</summary>
        Public Property Attributes As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        ' ── Validare ────────────────────────────────────────────────────────────
        ''' <summary>True dacă tag-ul există în dicționarul TagMap.</summary>
        Public Property TagExistsInMap As Boolean = False

        ''' <summary>Atribute prezente pe linie dar inexistente în TagMap pentru tag-ul respectiv.</summary>
        Public Property InvalidAttributes As New List(Of String)

        ''' <summary>Atribute REQUIRED (din TagMap) care lipsesc de pe linie.</summary>
        Public Property MissingRequiredAttributes As New List(Of String)

        ''' <summary>True dacă există ghilimele deschise fără pereche (valoare neterminată).</summary>
        Public Property HasUnclosedQuotes As Boolean = False

        ' ── Stare aparte (pentru multiline comment tracking) ────────────────────
        ''' <summary>True dacă linia DESCHIDE un comentariu multiline fără să-l și închidă.</summary>
        Public Property StartsMultilineComment As Boolean = False
        ''' <summary>True dacă linia ÎNCHIDE un comentariu multiline.</summary>
        Public Property EndsMultilineComment As Boolean = False

        ''' <summary>True dacă tag-ul s-a închis pe această linie (are > sau />).</summary>
        Public Property TagIsClosed As Boolean = True

        ''' <summary>True dacă linia e continuare de atribute pentru un tag deschis pe linia anterioară.</summary>
        Public Property IsTagContinuation As Boolean = False

        ' =========================================================================
        ' IsLineOK — toate validările la un loc
        ' =========================================================================
        ''' <summary>
        ''' True dacă linia este corectă sintactic și semantic față de TagMap.
        ''' Liniile goale, comentariile și tag-urile de închidere sunt mereu OK.
        ''' </summary>
        Public ReadOnly Property IsLineOK As Boolean
            Get
                If IsEmpty OrElse IsComment OrElse IsInsideMultilineComment OrElse
           IsXmlDeclaration OrElse IsClosingTag OrElse TagName Is Nothing Then
                    Return True
                End If
                ' Tag neînchis (atribute continuă pe liniile următoare):
                ' verificăm doar că tag-ul există și nu are atribute invalide
                If Not TagIsClosed Then
                    Return TagExistsInMap AndAlso InvalidAttributes.Count = 0
                End If
                ' Tag complet — verificare completă
                Return TagExistsInMap AndAlso
               InvalidAttributes.Count = 0 AndAlso
               MissingRequiredAttributes.Count = 0 AndAlso
               Not HasUnclosedQuotes
            End Get
        End Property
        ''' <summary>Mesaj descriptiv al problemei (pentru status bar / tooltip).</summary>
        Public ReadOnly Property ErrorSummary As String
            Get
                If IsLineOK Then Return ""
                Dim parts As New List(Of String)
                If Not TagExistsInMap Then parts.Add($"tag necunoscut: <{TagName}>")
                If InvalidAttributes.Count > 0 Then parts.Add($"atribute invalide: {String.Join(", ", InvalidAttributes)}")
                If MissingRequiredAttributes.Count > 0 Then parts.Add($"atribute lipsă: {String.Join(", ", MissingRequiredAttributes)}")
                If HasUnclosedQuotes Then parts.Add("ghilimele neterminat")
                Return String.Join(" | ", parts)
            End Get
        End Property

        ' =========================================================================
        ' GetAvailableAttributes — pentru autocomplete Space
        ' =========================================================================
        ''' <summary>
        ''' Atributele din TagMap care NU sunt încă prezente pe această linie,
        ''' sortate alfabetic. Folosit de autocomplete când utilizatorul apasă Space.
        ''' </summary>
        Public Function GetAvailableAttributes(
            tagAttrs As Dictionary(Of String, String())) As String()
            Dim value As String() = Nothing
            If TagName Is Nothing OrElse Not tagAttrs.TryGetValue(TagName, value) Then Return Array.Empty(Of String)()
            Return value.
            Where(Function(a) Not Attributes.ContainsKey(a)).
            OrderBy(Function(a) a).
            ToArray()
        End Function

        ' =========================================================================
        ' PARSE — factory method static
        ' =========================================================================
        ''' <summary>
        ''' Parsează o linie crudă și returnează un WflLine complet populat.
        ''' </summary>
        ''' <param name="rawText">Textul liniei (fără newline).</param>
        ''' <param name="lineIndex">Indexul 0-based în document.</param>
        ''' <param name="isInsideMultilineComment">True dacă un <!-- anterior nu a fost închis.</param>
        ''' <param name="tagAttrs">Dicționarul Tag→Atribute (din TagMap).</param>
        ''' <param name="tagRequired">Dicționarul Tag→AtributeRequired (din TagMap).</param>
        ''' <param name="openTagName">Opțional: numele tag-ului deschis pe linia anterioară, dacă această linie e continuare de atribute.</param>
        Public Shared Function Parse(rawText As String,
                             lineIndex As Integer,
                             isInsideMultilineComment As Boolean,
                             tagAttrs As Dictionary(Of String, String()),
                             tagRequired As Dictionary(Of String, String()),
                             Optional openTagName As String = Nothing) As WflLine

            Dim line As New WflLine With {.LineIndex = lineIndex, .RawText = rawText}
            Dim trimmed = rawText.Trim()

            ' ── Gol ──
            If String.IsNullOrWhiteSpace(trimmed) Then
                line.IsEmpty = True : Return line
            End If

            ' ── Continuare comentariu multiline ──
            If isInsideMultilineComment Then
                line.IsInsideMultilineComment = True
                line.IsComment = True
                If trimmed.Contains("-->") Then line.EndsMultilineComment = True
                Return line
            End If

            ' ── Declarație XML ──
            If trimmed.StartsWith("<?") Then
                line.IsXmlDeclaration = True : Return line
            End If

            ' ── Comentariu (posibil multiline) ──
            If trimmed.StartsWith("<!--") Then
                line.IsComment = True
                Dim hasOpen = trimmed.Contains("<!--")
                Dim hasClose = trimmed.Contains("-->")
                If hasOpen AndAlso Not hasClose Then line.StartsMultilineComment = True
                If hasClose Then line.EndsMultilineComment = True
                Return line
            End If

            ' ── Continuare atribute pentru un tag deschis pe linia anterioară ──
            If openTagName IsNot Nothing Then
                line.TagName = openTagName
                line.IsTagContinuation = True
                line.TagExistsInMap = tagAttrs.ContainsKey(openTagName)
                line.TagIsClosed = trimmed.Contains(">"c)   ' s-a închis pe această linie?
                line.IsOpeningTag = Not line.TagIsClosed
                line.IsSelfClosing = trimmed.TrimEnd().EndsWith("/>")

                ' Parsăm atributele de pe această linie de continuare
                ParseAttributeSection(trimmed, line, tagAttrs, tagRequired)
                Return line
            End If

            ' ── Tag de închidere </Tag> ──
            Dim closingM = Regex.Match(trimmed, "^</(\w+)\s*>")
            If closingM.Success Then
                line.IsClosingTag = True
                line.TagName = closingM.Groups(1).Value
                line.TagExistsInMap = tagAttrs.ContainsKey(line.TagName)
                Return line
            End If

            ' ── Tag deschis / self-closing ──
            ' Capturăm: <TagName [atribute...] /> sau <TagName [atribute...]>
            ' Notă: linia poate să NU fie completă (tag multiline) — atributele sunt oricum extrase
            Dim openM = Regex.Match(trimmed, "^<(\w+)(.*?)(/?>)?\s*$", RegexOptions.Singleline)
            If Not openM.Success Then Return line   ' linie de text simplu / altceva

            line.TagName = openM.Groups(1).Value
            line.IsSelfClosing = trimmed.TrimEnd().EndsWith("/>")
            line.IsOpeningTag = Not line.IsSelfClosing

            line.TagExistsInMap = tagAttrs.ContainsKey(line.TagName)
            line.TagIsClosed = trimmed.Contains(">"c)   ' ← are > sau /> = s-a închis

            ' ── Parsare atribute ──
            Dim attrSection = openM.Groups(2).Value
            ParseAttributeSection(attrSection, line, tagAttrs, tagRequired)

            Return line
        End Function

        ' =========================================================================
        ' PARSE ATTRIBUTES — intern
        ' =========================================================================
        Private Shared Sub ParseAttributeSection(attrSection As String,
                                              line As WflLine,
                                              tagAttrs As Dictionary(Of String, String()),
                                              tagRequired As Dictionary(Of String, String()))
            If String.IsNullOrWhiteSpace(attrSection) Then
                ' Nicio secțiune de atribute — verificăm doar required
                ValidateRequired(line, tagRequired)
                Return
            End If

            ' Detectare ghilimele neterminat
            Dim quoteCount = attrSection.Count(Function(c) c = """"c)
            line.HasUnclosedQuotes = (quoteCount Mod 2 <> 0)

            ' Extrage perechi attr="value" (cu sau fără ghilimele)
            Dim attrRx = New Regex("(\w[\w\-]*)\s*=\s*(?:""([^""]*)""|'([^']*)'|(\S+))",
                               RegexOptions.IgnoreCase)
            For Each m As Match In attrRx.Matches(attrSection)
                Dim attrName = m.Groups(1).Value
                Dim attrValue = If(m.Groups(2).Success, m.Groups(2).Value,
                           If(m.Groups(3).Success, m.Groups(3).Value,
                              m.Groups(4).Value))
                If Not line.Attributes.ContainsKey(attrName) Then
                    line.Attributes(attrName) = attrValue
                End If
            Next

            ' Validare față de TagMap
            If line.TagExistsInMap Then
                Dim validAttrs = tagAttrs(line.TagName)
                For Each attrName In line.Attributes.Keys
                    If Not validAttrs.Any(Function(a) String.Equals(a, attrName, StringComparison.OrdinalIgnoreCase)) Then
                        line.InvalidAttributes.Add(attrName)
                    End If
                Next
            End If

            ValidateRequired(line, tagRequired)
        End Sub

        Private Shared Sub ValidateRequired(line As WflLine,
                                        tagRequired As Dictionary(Of String, String()))
            Dim value As String() = Nothing
            If line.TagName Is Nothing OrElse Not tagRequired.TryGetValue(line.TagName, value) Then Return
            For Each req In value

                If Not line.Attributes.Keys.Any(Function(a) String.Equals(a, req, StringComparison.OrdinalIgnoreCase)) Then
                    line.MissingRequiredAttributes.Add(req)
                End If
            Next
        End Sub

    End Class
End Class