Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Security
Imports System.Text

' Slice 0088 -- «Nota contabila corectie CAB» (MF form F1135). FOREXE lists under «Operatiuni
' necorectate» the treasury operations it could not attach to an angajament («ERRRRRRRRRR»); the
' operator attaches each one to an angajament + indicator and K-BOT makes the note: row 1 stornos
' the operation, row 2 puts the same amount on the chosen angajament.
'
' Everything here is pure (no I/O) -> no Try/Catch (house rule). The rules are the form's own
' (csValidareNC / csUtile in the XDP, Surse/CAB+ERRRRRRR/full.txt), and the two XML shapes are
' copied from the sample note «NOTA CAB 23.pdf» (its datasets and its embedded f1135.xml).

''' <summary>
''' One stored correction note (FX_NoteCAB), or one being made in the note window: the header and
''' EVERY operation it corrects (<see cref="Corrections"/>) -- one pair of printed rows each.
''' </summary>
Public NotInheritable Class CabCorrectionNote
    ''' <summary>FX_NoteCAB.IDNC; 0 before the save.</summary>
    Public Property IdNc As Integer
    Public Property NoteNumber As Integer
    Public Property Year As Integer
    Public Property NoteDate As Date
    ''' <summary>«Denumire Entitate Publica»: at most 30 characters, letters, digits, spaces.</summary>
    Public Property EntityName As String = String.Empty
    ''' <summary>«Cod Fiscal Entitate Publica»: digits only.</summary>
    Public Property EntityTaxCode As String = String.Empty
    ''' <summary>Signer roles in the stored PDF («S1,S2»); empty = unsigned.</summary>
    Public Property Signature As String = String.Empty
    ''' <summary>Uploaded into FOREXE («Transmitere documente electronice»).</summary>
    Public Property Sent As Boolean
    Public Property SentAt As DateTime?
    Public Property SentAnswer As String = String.Empty
    ''' <summary>Sha-256 of the PDF stored on the server; empty = no PDF there.</summary>
    Public Property PdfSha256 As String = String.Empty
    ''' <summary>Slice 0088-04: FOREXE's registration index of the last upload («1230450081»); empty = unknown.</summary>
    Public Property RegistrationIndex As String = String.Empty
    ''' <summary>Slice 0088-04: the FOREXE receipt kept on the server; Nothing = none yet.</summary>
    Public Property Receipt As CabNoteReceipt
    ''' <summary>The operations the note corrects, in print order.</summary>
    Public Property Corrections As New List(Of CabNoteCorrection)()

    Public ReadOnly Property IsSigned As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(Signature)
        End Get
    End Property

    ''' <summary>The amount moved onto angajamente (the corrections' total, positive).</summary>
    Public ReadOnly Property Total As Decimal
        Get
            Return Corrections.Sum(Function(c) -c.Amount)
        End Get
    End Property

    ''' <summary>«0000000023» -- the ten digits the form wants.</summary>
    Public ReadOnly Property NoteNumberText As String
        Get
            Return NoteNumber.ToString("D10", CultureInfo.InvariantCulture)
        End Get
    End Property

    ''' <summary>The file name, the same the server gives it (routes/forexe/pdf.py, _nume_fisier_nc).</summary>
    Public ReadOnly Property FileName As String
        Get
            Return $"NOTA_CAB_{NoteNumber}_{Year}.PDF"
        End Get
    End Property
End Class

''' <summary>
''' One ERR operation corrected by a note (FX_NoteCAB_Corectii): its storno row and the row that
''' puts the amount on the chosen angajament.
''' </summary>
Public NotInheritable Class CabNoteCorrection
    ''' <summary>FX_Operatiuni.IDFXP of the operation; 0 when unknown.</summary>
    Public Property IdFxp As Integer
    ''' <summary>«Referință TREZOR» (storno row, «Numar referinta operatiune initiala»).</summary>
    Public Property TreasuryReference As String = String.Empty
    ''' <summary>«Nr. document»; with the reference, the key of the operation.</summary>
    Public Property DocumentNumber As String = String.Empty
    ''' <summary>Storno «Simbol cont», «24A650401200103».</summary>
    Public Property AccountSymbol As String = String.Empty
    ''' <summary>Storno «Cod program», the program FOREXE shows for the operation.</summary>
    Public Property ProgramCode As String = String.Empty
    ''' <summary>Storno «Data operatiune initiala».</summary>
    Public Property OriginalOperationDate As Date
    ''' <summary>"C" = the amounts are in «Suma credit» (an «Încasare»), "D" = in «Suma debit».</summary>
    Public Property AmountColumn As String = "C"
    ''' <summary>The storno amount: always negative. The correction row carries its opposite.</summary>
    Public Property Amount As Decimal
    ''' <summary>The angajament the operator chose.</summary>
    Public Property CommitmentCode As String = String.Empty
    ''' <summary>Its indicator (three characters, «AAB»).</summary>
    Public Property IndicatorCode As String = String.Empty
    ''' <summary>FX_Indicatori.CodAI of that indicator; may be empty.</summary>
    Public Property CodAi As String = String.Empty
    Public Property CorrectionAccountSymbol As String = String.Empty
    Public Property CorrectionProgramCode As String = String.Empty
    ''' <summary>«Explicatii» (the same on both rows): at most 70 characters, no diacritics.</summary>
    Public Property Explanation As String = String.Empty
End Class

''' <summary>One printed row of a note (the form's «rowNC»).</summary>
Public NotInheritable Class CabNoteRow
    Public Property RowNumber As Integer
    Public Property AccountSymbol As String = String.Empty
    Public Property ProgramCode As String = String.Empty
    Public Property CommitmentCode As String = String.Empty
    Public Property IndicatorCode As String = String.Empty
    Public Property OriginalReference As String = String.Empty
    Public Property OriginalDate As Date?
    Public Property Debit As Decimal
    Public Property Credit As Decimal
    Public Property Explanation As String = String.Empty
End Class

''' <summary>An indicator of an angajament, as the note window offers it.</summary>
Public NotInheritable Class CabCommitmentIndicator
    ''' <summary>FX_Indicatori.CodIndicator, «AAB».</summary>
    Public Property Code As String = String.Empty
    Public Property CodAi As String = String.Empty
    ''' <summary>Sector + source, «02A».</summary>
    Public Property Ss As String = String.Empty
    ''' <summary>Classification without dots, «650401200103».</summary>
    Public Property ClsfSal As String = String.Empty
    ''' <summary>Classification with dots, «65.04.01.20.01.03».</summary>
    Public Property Clsf As String = String.Empty
    Public Property Name As String = String.Empty
    ''' <summary>The programs of its SS (AVACONT_COMUN.DefaProgram).</summary>
    Public Property Programs As New List(Of String)()
End Class

''' <summary>An angajament of the unit with its indicators.</summary>
Public NotInheritable Class CabCommitment
    Public Property Code As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property Indicators As New List(Of CabCommitmentIndicator)()
End Class

''' <summary>What the note window loads once (GET /api/forexe/note-cab/pregatire).</summary>
Public NotInheritable Class CabNotePreparation
    Public Property NextNumber As Integer
    Public Property Year As Integer
    ''' <summary>Slice 0088-05: the note numbers already used on the server in <see cref="Year"/>.</summary>
    Public Property UsedNumbers As New HashSet(Of Integer)()
    Public Property Commitments As New List(Of CabCommitment)()
End Class

''' <summary>
''' Slice 0088-04 -- the FOREXE receipt («recipisa») of an uploaded note (FX_NoteCAB_Recipisa): the
''' file FOREXE puts in its SNM inbox for the registration index of the upload. POCO.
''' </summary>
Public NotInheritable Class CabNoteReceipt
    Public Property IdReceipt As Integer
    ''' <summary>«1230450081».</summary>
    Public Property RegistrationIndex As String = String.Empty
    ''' <summary>«INTERNT-1230450081-2026/28-09-2026».</summary>
    Public Property RegistrationNumber As String = String.Empty
    ''' <summary>FOREXE's id of the SNM message; 0 = unknown.</summary>
    Public Property MessageId As Long
    Public Property MessageText As String = String.Empty
    Public Property MessageDate As DateTime?
    Public Property FileName As String = String.Empty
    Public Property Size As Integer
    Public Property Sha256 As String = String.Empty
End Class

''' <summary>The F1135 rules and the two XML shapes of the form.</summary>
Public NotInheritable Class CabCorrectionNoteRules

    Private Sub New()
    End Sub

    ''' <summary>The code FOREXE puts on an operation it could not attach to an angajament.</summary>
    Public Const ErrCommitment As String = "ERRRRRRRRRR"

    ''' <summary>The form's number and version («universalCode»), read from the template.</summary>
    Public Const FormNumber As String = "1135"
    Public Const UniversalCode As String = "F1135_A1.0.05"

    ''' <summary>The note's total is always 0 (storno + correction); the form writes it in words so.</summary>
    Public Const ZeroInWords As String = "(zero lei si zero bani)"

    Public Const MaxExplanation As Integer = 70

    ' Slice 0088-04: FOREXE's registration number, «INTERNT-1230450081-2026/28-09-2026»; the index
    ' is the digits after «INTERNT-». FOREXE's answer to «Trimite» was never seen, so the plain
    ' «index ... 1230450081» wording is accepted too.
    Private Shared ReadOnly RegistrationPattern As New RegularExpressions.Regex(
        "INTERNT-(?<index>[0-9]{4,20})-[0-9]{4}/[0-9]{2}-[0-9]{2}-[0-9]{4}", RegularExpressions.RegexOptions.IgnoreCase)
    Private Shared ReadOnly IndexPattern As New RegularExpressions.Regex(
        "(?:INTERNT-|index\D{0,30})(?<index>[0-9]{6,20})", RegularExpressions.RegexOptions.IgnoreCase)

    ''' <summary>
    ''' The registration index in a FOREXE text (the answer to «Trimite», an SNM message); empty when
    ''' there is none.
    ''' </summary>
    Public Shared Function RegistrationIndexFrom(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Dim m As RegularExpressions.Match = IndexPattern.Match(text)
        Return If(m.Success, m.Groups("index").Value, String.Empty)
    End Function

    ''' <summary>The full registration number («INTERNT-...-2026/28-09-2026») in a FOREXE text; empty when none.</summary>
    Public Shared Function RegistrationNumberFrom(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Dim m As RegularExpressions.Match = RegistrationPattern.Match(text)
        Return If(m.Success, m.Value, String.Empty)
    End Function

    ''' <summary>Is <paramref name="index"/> a registration index (digits only, 4..20)?</summary>
    Public Shared Function IsRegistrationIndex(index As String) As Boolean
        If String.IsNullOrEmpty(index) OrElse index.Length < 4 OrElse index.Length > 20 Then Return False
        Return index.All(Function(ch) ch >= "0"c AndAlso ch <= "9"c)
    End Function
    Public Const MaxEntityName As Integer = 30
    Public Const MaxTaxCode As Integer = 10

    ' Budget sector -> the two digits the account symbol starts with. ONLY what a real note shows:
    ' sector 02 -> «24» («24A650401200103» on NOTA CAB 23). Any other sector gives no symbol and
    ' the operator types it; add a line here once a real note of that sector has been seen.
    Private Shared ReadOnly AccountPrefixBySector As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"02", "24"}
    }

    Private Shared ReadOnly RoCulture As CultureInfo = CultureInfo.GetCultureInfo("ro-RO")

    ''' <summary>True for an operation FOREXE could not attach («ERRRRRRRRRR»).</summary>
    Public Shared Function IsErr(op As UncorrectedOperation) As Boolean
        If op Is Nothing Then Return False
        Return String.Equals(op.Commitment.Trim(), ErrCommitment, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>«02A» + «650401200103» -> «24A650401200103»; empty when the sector is not known.</summary>
    Public Shared Function AccountSymbol(ss As String, clsfSal As String) As String
        Dim s As String = If(ss, String.Empty).Trim().ToUpperInvariant()
        Dim c As String = If(clsfSal, String.Empty).Replace(".", String.Empty).Trim()
        If s.Length <> 3 OrElse c.Length <> 12 OrElse Not c.All(AddressOf Char.IsDigit) Then Return String.Empty
        Dim prefix As String = Nothing
        If Not AccountPrefixBySector.TryGetValue(s.Substring(0, 2), prefix) Then Return String.Empty
        Return prefix & s.Substring(2, 1) & c
    End Function

    ''' <summary>The symbol of an operation as FOREXE writes its SSI, «02A-65.04.01.20.01.03».</summary>
    Public Shared Function AccountSymbol(op As UncorrectedOperation) As String
        If op Is Nothing Then Return String.Empty
        Return AccountSymbol(op.SectorSource, op.Classification)
    End Function

    ''' <summary>
    ''' The form's own account symbol check (csValidareNC.validareSimbolCont): 23/24/25/27/29 +
    ''' source letter + 12 digits, 20/21/22/26/28 + letter + 6 digits, or 5 + 3/5/7 digits.
    ''' </summary>
    Public Shared Function IsValidAccountSymbol(symbol As String) As Boolean
        Dim s As String = If(symbol, String.Empty).Trim().ToUpperInvariant()
        If s.Length = 0 Then Return False
        If s(0) = "5"c Then
            Return (s.Length = 4 OrElse s.Length = 6 OrElse s.Length = 8) AndAlso s.All(AddressOf Char.IsDigit)
        End If
        If s.Length < 3 OrElse s(0) <> "2"c Then Return False
        Dim longForm As Boolean = "23 24 25 27 29".Contains(s.Substring(0, 2), StringComparison.Ordinal)
        Dim shortForm As Boolean = "20 21 22 26 28".Contains(s.Substring(0, 2), StringComparison.Ordinal)
        If Not longForm AndAlso Not shortForm Then Return False
        If s.Length <> If(longForm, 15, 9) Then Return False
        If s(2) < "A"c OrElse s(2) > "M"c Then Return False
        Return s.Substring(3).All(AddressOf Char.IsDigit)
    End Function

    ''' <summary>
    ''' "C" for an «Încasare» (the sample note puts it under «Suma credit»), "D" for anything else.
    ''' </summary>
    Public Shared Function AmountColumnFor(kind As String) As String
        Return If(Plain(kind).Contains("INCASARE", StringComparison.Ordinal), "C", "D")
    End Function

    ''' <summary>The storno amount: the operation's amount made negative.</summary>
    Public Shared Function StornoAmount(amount As Decimal) As Decimal
        Return -Math.Abs(Decimal.Round(amount, 2))
    End Function

    ''' <summary>
    ''' The explanation offered for an operation: «INCASARE REGLARE OP 136». The operator may
    ''' rewrite it; it goes through <see cref="CleanText"/> either way.
    ''' </summary>
    Public Shared Function DefaultExplanation(op As UncorrectedOperation) As String
        If op Is Nothing Then Return String.Empty
        Return CleanText($"{op.Kind} REGLARE OP {op.DocumentNumber}", MaxExplanation, allowPunctuation:=True)
    End Function

    ''' <summary>The unit's name as the form accepts it: no diacritics, letters/digits/spaces, 30.</summary>
    Public Shared Function EntityName(unitName As String) As String
        Return CleanText(unitName, MaxEntityName, allowPunctuation:=False)
    End Function

    ''' <summary>«RO29164800» -> «29164800»: digits only, at most 10.</summary>
    Public Shared Function TaxCode(cf As String) As String
        Dim d As String = New String(If(cf, String.Empty).Where(AddressOf Char.IsDigit).ToArray())
        Return If(d.Length > MaxTaxCode, d.Substring(0, MaxTaxCode), d)
    End Function

    ''' <summary>Upper case, without diacritics.</summary>
    Public Shared Function Plain(text As String) As String
        Dim t As String = If(text, String.Empty).Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(t.Length)
        For Each ch As Char In t
            If CharUnicodeInfo.GetUnicodeCategory(ch) <> UnicodeCategory.NonSpacingMark Then sb.Append(ch)
        Next
        Return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant()
    End Function

    ''' <summary>
    ''' Text the form accepts: upper case, no diacritics, only A-Z 0-9 and spaces (plus . , - / when
    ''' <paramref name="allowPunctuation"/>), spaces collapsed, cut to <paramref name="maxLength"/>.
    ''' </summary>
    Public Shared Function CleanText(text As String, maxLength As Integer, allowPunctuation As Boolean) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In Plain(text)
            If (ch >= "A"c AndAlso ch <= "Z"c) OrElse (ch >= "0"c AndAlso ch <= "9"c) Then
                sb.Append(ch)
            ElseIf allowPunctuation AndAlso ".,-/".IndexOf(ch) >= 0 Then
                sb.Append(ch)
            Else
                sb.Append(" "c)
            End If
        Next
        Dim collapsed As String = String.Join(" ", sb.ToString().Split(" "c, StringSplitOptions.RemoveEmptyEntries))
        Return If(collapsed.Length > maxLength, collapsed.Substring(0, maxLength).TrimEnd(), collapsed)
    End Function

    ''' <summary>The indicators of an angajament that sit on the operation's SS + classification.</summary>
    Public Shared Function Matches(ind As CabCommitmentIndicator, op As UncorrectedOperation) As Boolean
        If ind Is Nothing OrElse op Is Nothing Then Return False
        Return String.Equals(ind.Ss.Trim(), op.SectorSource.Trim(), StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(ind.ClsfSal.Trim(), op.Classification.Replace(".", String.Empty).Trim(), StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' The program of the correction row: the operation's own when the indicator's SS has it,
    ''' else the SS's only (or first) program, else the operation's.
    ''' </summary>
    Public Shared Function CorrectionProgram(ind As CabCommitmentIndicator, stornoProgram As String) As String
        Dim own As String = If(stornoProgram, String.Empty).Trim()
        If ind Is Nothing OrElse ind.Programs.Count = 0 Then Return own
        If ind.Programs.Contains(own) Then Return own
        Return ind.Programs(0)
    End Function

    ''' <summary>
    ''' The printed rows: for every correction, the storno then the correction (2k-1, 2k). The form
    ''' wants row 1 to be a storno and every other correction row without reference / date -- this
    ''' order gives both.
    ''' </summary>
    Public Shared Function Rows(note As CabCorrectionNote) As List(Of CabNoteRow)
        ArgumentNullException.ThrowIfNull(note)
        Dim result As New List(Of CabNoteRow)()
        For Each c As CabNoteCorrection In note.Corrections
            Dim credit As Boolean = String.Equals(c.AmountColumn, "C", StringComparison.OrdinalIgnoreCase)
            Dim storno As Decimal = c.Amount
            result.Add(New CabNoteRow With {
                .RowNumber = result.Count + 1, .AccountSymbol = c.AccountSymbol, .ProgramCode = c.ProgramCode,
                .CommitmentCode = ErrCommitment, .IndicatorCode = String.Empty,
                .OriginalReference = c.TreasuryReference, .OriginalDate = c.OriginalOperationDate,
                .Debit = If(credit, 0D, storno), .Credit = If(credit, storno, 0D),
                .Explanation = c.Explanation})
            result.Add(New CabNoteRow With {
                .RowNumber = result.Count + 1, .AccountSymbol = c.CorrectionAccountSymbol,
                .ProgramCode = c.CorrectionProgramCode, .CommitmentCode = c.CommitmentCode,
                .IndicatorCode = c.IndicatorCode, .OriginalReference = String.Empty,
                .OriginalDate = Nothing,
                .Debit = If(credit, 0D, -storno), .Credit = If(credit, -storno, 0D),
                .Explanation = c.Explanation})
        Next
        Return result
    End Function

    ''' <summary>
    ''' What is wrong with ONE correction, in Romanian, one line each; empty = complete (the tick in
    ''' the note window). The same rules the server (routes/forexe/note_cab.py) and the form check.
    ''' </summary>
    Public Shared Function ValidateCorrection(c As CabNoteCorrection, noteDate As Date) As List(Of String)
        ArgumentNullException.ThrowIfNull(c)
        Dim errors As New List(Of String)()
        If String.IsNullOrWhiteSpace(c.CommitmentCode) Then errors.Add("Alegeți angajamentul.")
        If If(c.IndicatorCode, String.Empty).Length <> 3 Then errors.Add("Alegeți indicatorul angajamentului.")
        If c.OriginalOperationDate = Date.MinValue Then
            errors.Add("Data operațiunii inițiale trebuie scrisă zz.ll.aaaa.")
        ElseIf c.OriginalOperationDate > noteDate Then
            errors.Add("Data operațiunii inițiale nu poate fi după data notei.")
        End If
        If Not IsValidAccountSymbol(c.AccountSymbol) Then errors.Add("Simbolul contului de stornare nu are forma cerută de formular (ex. 24A650401200103).")
        If Not IsValidAccountSymbol(c.CorrectionAccountSymbol) Then errors.Add("Simbolul contului de corecție nu are forma cerută de formular.")
        If Not IsProgram(c.ProgramCode) Then errors.Add("Codul programului de stornare trebuie să aibă 10 cifre.")
        If Not IsProgram(c.CorrectionProgramCode) Then errors.Add("Codul programului de corecție trebuie să aibă 10 cifre.")
        If c.Amount >= 0D Then errors.Add("Operațiunea nu are o sumă citită din FOREXE.")
        If c.TreasuryReference.Length < 6 OrElse c.TreasuryReference.Length > 15 Then errors.Add("Referința TREZOR trebuie să aibă între 6 și 15 caractere.")
        If String.IsNullOrWhiteSpace(c.Explanation) Then errors.Add("Explicațiile sunt obligatorii.")
        Return errors
    End Function

    ''' <summary>The note's header mistakes plus every correction's, each prefixed with its reference.</summary>
    Public Shared Function Validate(note As CabCorrectionNote) As List(Of String)
        ArgumentNullException.ThrowIfNull(note)
        Dim errors As New List(Of String)()
        If note.NoteNumber <= 0 Then errors.Add("Numărul notei trebuie să fie mai mare decât 0.")
        If note.EntityName.Length = 0 Then errors.Add("Denumirea entității lipsește.")
        If note.EntityTaxCode.Length = 0 Then errors.Add("Codul fiscal al entității lipsește.")
        If note.Corrections.Count = 0 Then errors.Add("Nota nu are nicio operațiune.")
        For Each c As CabNoteCorrection In note.Corrections
            For Each e As String In ValidateCorrection(c, note.NoteDate)
                errors.Add(c.TreasuryReference & ": " & e)
            Next
        Next
        Return errors
    End Function

    Private Shared Function IsProgram(code As String) As Boolean
        Dim c As String = If(code, String.Empty)
        Return c.Length = 10 AndAlso c.All(AddressOf Char.IsDigit)
    End Function

    ''' <summary>
    ''' «Suma control» (csDataTool.getSumaControlExt + csUtile.criptareSumaControl): CIF + the note
    ''' date as yyyyMMdd + the number of rows, then every digit +2 mod 10, a leading 0 becoming 9.
    ''' NOTA CAB 23: 29164800 + 20260928 + 2 = 49425730 -> 61647952.
    ''' </summary>
    Public Shared Function ControlSum(taxCode As String, noteDate As Date, rowCount As Integer) As Long
        Dim cif As Long = 0
        Long.TryParse(If(taxCode, String.Empty), NumberStyles.None, CultureInfo.InvariantCulture, cif)
        Dim sum As Long = cif + Long.Parse(noteDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) + rowCount
        Dim digits As Char() = sum.ToString(CultureInfo.InvariantCulture).ToCharArray()
        For i As Integer = 0 To digits.Length - 1
            digits(i) = ChrW(AscW("0"c) + ((AscW(digits(i)) - AscW("0"c) + 2) Mod 10))
        Next
        If digits.Length > 0 AndAlso digits(0) = "0"c Then digits(0) = "9"c
        Return Long.Parse(New String(digits), CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' The form's data (the XFA datasets «form1» node), the same shape Adobe saved in NOTA CAB 23:
    ''' amounts as «-111,00», the two totals as «0.00». Adobe lays the rows out from it.
    ''' </summary>
    Public Shared Function FormDataXml(note As CabCorrectionNote) As String
        ArgumentNullException.ThrowIfNull(note)
        Dim noteRows As List(Of CabNoteRow) = Rows(note)
        Dim control As Long = ControlSum(note.EntityTaxCode, note.NoteDate, noteRows.Count)
        Dim sb As New StringBuilder()
        sb.Append("<form1><main><antet>")
        Element(sb, "numarNota", note.NoteNumberText)
        Element(sb, "dataNota", FormDate(note.NoteDate))
        Element(sb, "sumaNota", FormAmount(0D))
        Element(sb, "denumireEP", note.EntityName)
        Element(sb, "cifEP", note.EntityTaxCode)
        Element(sb, "universalCode", UniversalCode)
        Element(sb, "txtSumaNota", ZeroInWords)
        Element(sb, "cif", note.EntityTaxCode)
        Element(sb, "an_r", note.NoteDate.ToString("yyyy", CultureInfo.InvariantCulture))
        Element(sb, "luna_r", note.NoteDate.ToString("MM", CultureInfo.InvariantCulture))
        Element(sb, "d_rec", "0")
        Element(sb, "totalPlata_A", control.ToString(CultureInfo.InvariantCulture))
        Element(sb, "numarInregistrari", noteRows.Count.ToString(CultureInfo.InvariantCulture))
        Element(sb, "sumaControl", control.ToString(CultureInfo.InvariantCulture))
        Element(sb, "totalDebit", "0.00")
        Element(sb, "totalCredit", "0.00")
        sb.Append("</antet><nc><tableNC>")
        sb.Append("<headerRowNC xfa:dataNode=""dataGroup"" xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/"" />")
        For Each r As CabNoteRow In noteRows
            sb.Append("<rowNC>")
            Element(sb, "nrRand", r.RowNumber.ToString(CultureInfo.InvariantCulture))
            Element(sb, "simbolCont", r.AccountSymbol)
            Element(sb, "codProgram", r.ProgramCode)
            Element(sb, "codAngajament", r.CommitmentCode)
            Element(sb, "indicatorAngajament", r.IndicatorCode)
            Element(sb, "nrRefOperInitiala", r.OriginalReference)
            Element(sb, "dataOperInitiala", If(r.OriginalDate.HasValue, FormDate(r.OriginalDate.Value), String.Empty))
            Element(sb, "sumaDebit", FormAmount(r.Debit))
            Element(sb, "sumaCredit", FormAmount(r.Credit))
            Element(sb, "explicatii", r.Explanation)
            sb.Append("</rowNC>")
        Next
        sb.Append("</tableNC></nc></main>")
        sb.Append("<sbfrmFooter xfa:dataNode=""dataGroup"" xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/"" />")
        sb.Append("</form1>")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' The «f1135.xml» the form attaches to itself on «VALIDARE SI GENERARE XML» -- the file
    ''' FOREXE reads. Byte for byte the shape of NOTA CAB 23's attachment: CRLF line ends, a space
    ''' before each «>», numbers written as JavaScript writes them («0», «-111», «111.5»), empty
    ''' attributes left out.
    ''' </summary>
    Public Shared Function ExportXml(note As CabCorrectionNote) As String
        ArgumentNullException.ThrowIfNull(note)
        Dim noteRows As List(Of CabNoteRow) = Rows(note)
        Dim control As Long = ControlSum(note.EntityTaxCode, note.NoteDate, noteRows.Count)
        Dim ns As String = $"mfp:anaf:dgti:f{FormNumber}:declaratie:v1"
        Dim sb As New StringBuilder()
        sb.Append("<?xml version=""1.0"" encoding=""UTF-8""?>").Append(vbCrLf)
        sb.Append("<f").Append(FormNumber)
        sb.Append(" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""")
        sb.Append(" xsi:schemaLocation=""").Append(ns).Append("""")
        sb.Append(" xmlns=""").Append(ns).Append("""")
        Attr(sb, "luna", note.NoteDate.ToString("MM", CultureInfo.InvariantCulture))
        Attr(sb, "an", note.NoteDate.ToString("yyyy", CultureInfo.InvariantCulture))
        Attr(sb, "d_rec", "0")
        Attr(sb, "cif", note.EntityTaxCode)
        Attr(sb, "suma_control", control.ToString(CultureInfo.InvariantCulture))
        Attr(sb, "numar_inregistrari", noteRows.Count.ToString(CultureInfo.InvariantCulture))
        Attr(sb, "numar_nota", note.NoteNumberText)
        Attr(sb, "data_nota", FormDate(note.NoteDate))
        Attr(sb, "suma_nota", JsNumber(0D))
        Attr(sb, "cif_ep", note.EntityTaxCode)
        Attr(sb, "denumire_ep", note.EntityName)
        sb.Append(" >").Append(vbCrLf)
        For Each r As CabNoteRow In noteRows
            sb.Append("<nota_rand")
            Attr(sb, "nr_rand", r.RowNumber.ToString(CultureInfo.InvariantCulture))
            Attr(sb, "simbol_cont", r.AccountSymbol)
            Attr(sb, "cod_program", r.ProgramCode)
            Attr(sb, "cod_angajament", r.CommitmentCode)
            Attr(sb, "indicator_angajament", r.IndicatorCode)
            Attr(sb, "nr_ref_oper_initiala", r.OriginalReference)
            If r.OriginalDate.HasValue Then Attr(sb, "data_oper_initiala", FormDate(r.OriginalDate.Value))
            Attr(sb, "suma_debit", JsNumber(r.Debit))
            Attr(sb, "suma_credit", JsNumber(r.Credit))
            Attr(sb, "explicatii", r.Explanation)
            sb.Append(" /> ").Append(vbCrLf)
        Next
        sb.Append("</f").Append(FormNumber).Append(">")
        Return sb.ToString()
    End Function

    ''' <summary>dd.MM.yyyy, the form's date format.</summary>
    Public Shared Function FormDate(value As Date) As String
        Return value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>«-111,00»: two decimals, comma, no thousands separator (the datasets' shape).</summary>
    Public Shared Function FormAmount(value As Decimal) As String
        Return value.ToString("0.00", RoCulture)
    End Function

    ''' <summary>How JavaScript prints a number: «0», «-111», «111.5».</summary>
    Public Shared Function JsNumber(value As Decimal) As String
        Return Decimal.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture)
    End Function

    ' Empty values are written as an empty element, as Adobe saved them («<indicatorAngajament />»).
    Private Shared Sub Element(sb As StringBuilder, name As String, value As String)
        If String.IsNullOrEmpty(value) Then
            sb.Append("<"c).Append(name).Append(" />")
        Else
            sb.Append("<"c).Append(name).Append(">"c).Append(SecurityElement.Escape(value)).
               Append("</").Append(name).Append(">"c)
        End If
    End Sub

    ' The form leaves an attribute out when the field is empty (csDataTool.__generateXMLForTable).
    Private Shared Sub Attr(sb As StringBuilder, name As String, value As String)
        If String.IsNullOrEmpty(value) Then Return
        sb.Append(" "c).Append(name).Append("=""").Append(SecurityElement.Escape(value)).Append(""""c)
    End Sub

End Class
