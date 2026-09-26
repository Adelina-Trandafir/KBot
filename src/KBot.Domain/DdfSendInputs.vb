Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

' Slice 0081-04 -- the pure half of THE SEND: the workflow inputs, reading forexecab's indicator
' table back, the check of that table against section A, and what a run leaves behind (codes,
' captures). No I/O -> no Try/Catch (house rule).

''' <summary>
''' One section-A line as the send sees it -- built from the SAVED revision, never from the screen.
''' </summary>
Public NotInheritable Class DdfSendLine
    Public Property IdSecA As Integer
    ''' <summary>Dotted classification, «65.04.02.20.01.01».</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Sector + source, three characters, «02A».</summary>
    Public Property Ss As String = String.Empty
    ''' <summary>forexecab's row code («AAB»), or K-BOT's «!xyz» while forexecab has no row.</summary>
    Public Property CodIndicator As String = String.Empty
    Public Property ValPrec As Double
    Public Property ValCur As Double
    Public Property ValTot As Double

    ''' <summary>The workflows' row key (<c>Cheie</c>): «IdSecA-&lt;id&gt;».</summary>
    Public ReadOnly Property Cheie As String
        Get
            Return "IdSecA-" & IdSecA.ToString(CultureInfo.InvariantCulture)
        End Get
    End Property

    ''' <summary>The classification without separators, «650402200101» (<c>ClsfSal</c>).</summary>
    Public ReadOnly Property ClsfSal As String
        Get
            Return DdfSendInputs.DigitsAndLetters(Clsf)
        End Get
    End Property

    ''' <summary>Has forexecab no row for this line yet?</summary>
    Public ReadOnly Property EsteRandNou As Boolean
        Get
            Return String.IsNullOrWhiteSpace(CodIndicator) OrElse CodIndicator.StartsWith("!", StringComparison.Ordinal)
        End Get
    End Property

    Public Shared Function FromDraft(a As DdfDraftLinieA) As DdfSendLine
        ArgumentNullException.ThrowIfNull(a)
        Return New DdfSendLine() With {
            .IdSecA = a.IdSecA, .Clsf = DdfSendInputs.ForexeClsf(a.Clsf), .Ss = If(a.Ss, String.Empty),
            .CodIndicator = If(a.CodIndicator, String.Empty),
            .ValPrec = a.ValPrec, .ValCur = a.ValCur, .ValTot = a.ValTot}
    End Function
End Class

''' <summary>
''' Everything the send computes without I/O.
''' </summary>
Public NotInheritable Class DdfSendInputs

    ''' <summary><c>Indicator_ang</c> of a row forexecab does not have yet
    ''' (<c>Incarca Rezervare</c>: a «!» prefix = add a row).</summary>
    Public Const NewRowMarker As String = "!NOU"
    ''' <summary>Variable prefix of every capture the sending workflows take.</summary>
    Public Const CapturePrefix As String = "Poza_"

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The reason text sent to forexecab: A.2 of the revision + «(REV:n)» (D9). The import reads
    ''' the tag back (<c>extract_numar_rev</c>) and links the new reservations to the revision.
    ''' </summary>
    Public Shared Function Motiv(descScurta As String, numarRev As Integer) As String
        Dim text As String = If(descScurta, String.Empty).Trim()
        Dim tag As String = $"(REV:{numarRev.ToString(CultureInfo.InvariantCulture)})"
        If text.IndexOf("(REV:", StringComparison.OrdinalIgnoreCase) >= 0 Then Return text
        Return If(text.Length = 0, tag, text & " " & tag)
    End Function

    ''' <summary>An amount as the forexecab inputs take it: «1.380,00» (the example in
    ''' <c>Incarca Rezervare</c>'s header).</summary>
    Public Shared Function FormatSuma(value As Double) As String
        Return value.ToString("#,##0.00", _ro)
    End Function

    ''' <summary>An amount read from a forexecab page («1.380,00»); Nothing when it is not one.</summary>
    Public Shared Function ParseSuma(text As String) As Double?
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim v As Double
        If Double.TryParse(text.Trim(), NumberStyles.Number, _ro, v) Then Return v
        Return Nothing
    End Function

    ''' <summary>
    ''' A classification as forexecab writes it: «65.02.04.02.20.01.01» -&gt; «65.04.02.20.01.01».
    '''
    ''' <para><c>Clasificatii.Clsf</c> is <c>concat_ws('.', Capitol, Subcapitol, Articol,
    ''' Alineat)</c>, and <c>Capitol</c> still carries Access's «NN.NN» («65.02»); the second
    ''' half is a relic forexecab does not know (the table's own <c>ClsfSal</c> already keeps
    ''' only <c>left(Capitol, 2)</c>). Every column is NOT NULL, so the Access form always has
    ''' seven dotted parts: only then is the second part dropped. A text already in forexecab's
    ''' form (six parts) or any other shape comes back trimmed, unchanged.</para>
    ''' </summary>
    Public Shared Function ForexeClsf(clsf As String) As String
        Dim t As String = If(clsf, String.Empty).Trim()
        Dim parti As String() = t.Split("."c)
        If parti.Length <> 7 OrElse parti(0).Length <> 2 Then Return t
        Return parti(0) & "." & String.Join(".", parti, 2, parti.Length - 2)
    End Function

    ''' <summary>Only letters and digits, upper case.</summary>
    Public Shared Function DigitsAndLetters(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Dim sb As New StringBuilder(text.Length)
        For Each ch As Char In text
            If Char.IsLetterOrDigit(ch) Then sb.Append(Char.ToUpperInvariant(ch))
        Next
        Return sb.ToString()
    End Function

    ''' <summary><c>DATE_RECEPTIE</c> of <c>Creare Angajament</c>: one row per line, CB initial = A.col6.</summary>
    Public Shared Function CreareRows(linii As IEnumerable(Of DdfSendLine), codProgram As String) _
        As List(Of Dictionary(Of String, String))
        Dim rows As New List(Of Dictionary(Of String, String))()
        For Each l As DdfSendLine In If(linii, Enumerable.Empty(Of DdfSendLine)())
            rows.Add(New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                {"Cheie", l.Cheie},
                {"Clasificatia", l.Clsf},
                {"CodProgram", If(codProgram, String.Empty)},
                {"Sursa", l.Ss},
                {"CB_INITIAL", FormatSuma(l.ValCur)}})
        Next
        Return rows
    End Function

    ''' <summary>
    ''' <c>DATE_MODIFICARE</c> of <c>Incarca Rezervare</c>: one row per line, value = A.col7.
    ''' A line forexecab has no row for gets <see cref="NewRowMarker"/>.
    ''' </summary>
    Public Shared Function IncarcaRows(linii As IEnumerable(Of DdfSendLine), codProgram As String,
                                       motiv As String) As List(Of Dictionary(Of String, String))
        Dim rows As New List(Of Dictionary(Of String, String))()
        For Each l As DdfSendLine In If(linii, Enumerable.Empty(Of DdfSendLine)())
            rows.Add(New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                {"Cheie", l.Cheie},
                {"Indicator_ang", If(l.EsteRandNou, NewRowMarker, l.CodIndicator.Trim())},
                {"ClsfSal", l.ClsfSal},
                {"Clasificatia", l.Clsf},
                {"CodProgram", If(codProgram, String.Empty)},
                {"Sursa", l.Ss},
                {"ValoareNoua", FormatSuma(l.ValTot)},
                {"Motiv", If(motiv, String.Empty)}})
        Next
        Return rows
    End Function

    ''' <summary>A workflow JSON variable from rows.</summary>
    Public Shared Function ToJson(rows As IEnumerable(Of Dictionary(Of String, String))) As String
        Return System.Text.Json.JsonSerializer.Serialize(If(rows, Enumerable.Empty(Of Dictionary(Of String, String))()))
    End Function

    ' ── Reading forexecab's indicator table back ─────────────────────────────

    ' Column names come normalised by ScrapeTable («Indicator_ang») or raw from FindInTable
    ' («Indicator ang»): compared on letters and digits only.
    Private Shared Function Cell(row As RandTabel, column As String) As String
        If row Is Nothing Then Return String.Empty
        Dim wanted As String = DigitsAndLetters(column)
        For Each kvp As KeyValuePair(Of String, CelulaTabel) In row
            If DigitsAndLetters(kvp.Key) = wanted Then
                Return If(kvp.Value Is Nothing, String.Empty, kvp.Value.TextSau(String.Empty))
            End If
        Next
        Return String.Empty
    End Function

    ''' <summary>
    ''' The forexecab row of a line. «Sector - Sursa - Indicator» starts with the SS («02A-») and
    ''' then carries the classification: compared as <c>FindInTable</c> does (prefix off, letters
    ''' and digits only, «includes»), and the SS must match when the cell carries one.
    ''' </summary>
    Public Shared Function GridRowFor(grid As IEnumerable(Of RandTabel), line As DdfSendLine) As RandTabel
        If grid Is Nothing OrElse line Is Nothing Then Return Nothing
        Dim clsfSal As String = line.ClsfSal
        If clsfSal.Length = 0 Then Return Nothing
        For Each row As RandTabel In grid
            Dim ssi As String = Cell(row, "Sector_Sursa_Indicator").Trim()
            If ssi.Length = 0 Then Continue For
            Dim m As Match = Regex.Match(ssi, "^([0-9]{2}[A-Za-z])\s*-\s*")
            Dim rest As String = ssi
            If m.Success Then
                If line.Ss.Length > 0 AndAlso Not String.Equals(m.Groups(1).Value, line.Ss, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If
                rest = ssi.Substring(m.Length)
            End If
            If DigitsAndLetters(rest).Contains(clsfSal) Then Return row
        Next
        Return Nothing
    End Function

    ''' <summary>forexecab's row code («Indicator ang») of a line, or empty.</summary>
    Public Shared Function RowCode(grid As IEnumerable(Of RandTabel), line As DdfSendLine) As String
        Return Cell(GridRowFor(grid, line), "Indicator_ang").Trim()
    End Function

    ''' <summary>
    ''' Step 4 of the plan: forexecab's reserved CB per line against section A. A new angajament
    ''' (<paramref name="initial"/>) is compared on «CB rezervat initial» = A.col6; any other send
    ''' on «CB rezervat definitiv an curent» = A.col7. One Romanian line per difference; empty =
    ''' in sync.
    ''' </summary>
    Public Shared Function Differences(grid As IEnumerable(Of RandTabel), linii As IEnumerable(Of DdfSendLine),
                                       initial As Boolean) As List(Of String)
        Dim diffs As New List(Of String)()
        Dim column As String = If(initial, "Credit_bugetar_rezervat_initial", "Credit_bugetar_rezervat_definitiv_an_curent")
        For Each l As DdfSendLine In If(linii, Enumerable.Empty(Of DdfSendLine)())
            Dim expected As Double = If(initial, l.ValCur, l.ValTot)
            Dim row As RandTabel = GridRowFor(grid, l)
            If row Is Nothing Then
                diffs.Add($"{l.Ss} {l.Clsf}: rândul nu există în FOREXE (așteptat {FormatSuma(expected)}).")
                Continue For
            End If
            Dim actual As Double? = ParseSuma(Cell(row, column))
            If Not actual.HasValue OrElse Math.Abs(actual.Value - expected) > 0.005R Then
                diffs.Add($"{l.Ss} {l.Clsf}: în FOREXE {If(actual.HasValue, FormatSuma(actual.Value), "(necitit)")}, " &
                          $"în documentul de fundamentare {FormatSuma(expected)}.")
            End If
        Next
        Return diffs
    End Function

    ''' <summary>
    ''' A resume (S1x) through <c>Incarca Rezervare</c>: the lines still to send. A line whose
    ''' forexecab value already equals the target is left out -- the workflow waits for the
    ''' «motiv» dialog, which forexecab shows only when the value changes. A line forexecab already
    ''' has gets that row's code even when the first run could not save it, so it is SET, never
    ''' added a second time.
    ''' </summary>
    Public Shared Function LinesToResume(grid As IEnumerable(Of RandTabel), linii As IEnumerable(Of DdfSendLine)) _
        As List(Of DdfSendLine)
        Dim rest As New List(Of DdfSendLine)()
        For Each l As DdfSendLine In If(linii, Enumerable.Empty(Of DdfSendLine)())
            Dim row As RandTabel = GridRowFor(grid, l)
            If row Is Nothing Then
                rest.Add(l)
                Continue For
            End If
            Dim code As String = Cell(row, "Indicator_ang").Trim()
            Dim actual As Double? = ParseSuma(Cell(row, "Credit_bugetar_rezervat_definitiv_an_curent"))
            If actual.HasValue AndAlso Math.Abs(actual.Value - l.ValTot) <= 0.005R Then Continue For
            rest.Add(New DdfSendLine() With {
                .IdSecA = l.IdSecA, .Clsf = l.Clsf, .Ss = l.Ss,
                .CodIndicator = If(code.Length > 0, code, l.CodIndicator),
                .ValPrec = l.ValPrec, .ValCur = l.ValCur, .ValTot = l.ValTot})
        Next
        Return rest
    End Function

    ' ── What a run leaves behind ─────────────────────────────────────────────

    ''' <summary>
    ''' The captures of a run, in the order the executor kept its variables: every
    ''' <c>Poza_*</c> variable holding one PNG (base64). A value that is a JSON list (a name
    ''' reused inside a loop) is skipped -- the sending workflows give each capture its own name.
    ''' </summary>
    Public Shared Function Captures(data As IEnumerable(Of KeyValuePair(Of String, String))) _
        As List(Of KeyValuePair(Of String, Byte()))
        Dim result As New List(Of KeyValuePair(Of String, Byte()))()
        If data Is Nothing Then Return result
        For Each kvp As KeyValuePair(Of String, String) In data
            If kvp.Key Is Nothing OrElse Not kvp.Key.StartsWith(CapturePrefix, StringComparison.Ordinal) Then Continue For
            Dim v As String = If(kvp.Value, String.Empty).Trim()
            If v.Length = 0 OrElse v.StartsWith("[", StringComparison.Ordinal) Then Continue For
            Dim bytes As Byte() = Nothing
            Try
                bytes = Convert.FromBase64String(v)
            Catch ex As FormatException
                ' Not base64, so not a capture: left out rather than stored broken.
                bytes = Nothing
            End Try
            If bytes IsNot Nothing AndAlso bytes.Length > 0 Then
                result.Add(New KeyValuePair(Of String, Byte())(kvp.Key, bytes))
            End If
        Next
        Return result
    End Function

    ''' <summary>The file name of a capture: the variable name, ASCII letters/digits/«_»/«-», «.png».</summary>
    Public Shared Function CaptureFileName(variable As String) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In If(variable, "Poza")
            Dim ok As Boolean = AscW(ch) < 128 AndAlso (Char.IsLetterOrDigit(ch) OrElse ch = "_"c OrElse ch = "-"c)
            sb.Append(If(ok, ch, "_"c))
        Next
        Return sb.ToString() & ".png"
    End Function

    ''' <summary>
    ''' The angajament code from a text read off the page: the value itself (CodAng_Final reads the
    ''' code's own span), or the one after «Cod:» / «Numar angajament:» in the header block that
    ''' CodAng_&lt;Cheie&gt; reads whole. Empty when none is there.
    ''' </summary>
    Public Shared Function CodeFromText(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then Return String.Empty
        Dim t As String = text.Trim()
        If Regex.IsMatch(t, "^[A-Z0-9]{6,20}$") Then Return t
        ' \u0103 = the a-breve of the label; written as a regex escape so the source stays ASCII (rule 0).
        Dim m As Match = Regex.Match(t, "(?:Cod|Num[a\u0103]r\s+angajament)\s*:\s*([A-Z0-9]{6,20})", RegexOptions.IgnoreCase)
        Return If(m.Success, m.Groups(1).Value.ToUpperInvariant(), String.Empty)
    End Function
End Class
