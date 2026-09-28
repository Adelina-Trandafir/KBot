Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Note de corecție CAB» (slice 0088). Opened by the shell right after a FOREXE login when the
''' «Operațiuni necorectate» table brought NEW «ERRRRRRRRRR» operations, and from the menu
''' «Operațiuni necorelate» for every stored one that no note covers yet.
'''
''' <para><b>How it is filled</b> (operator, 28.09.2026): the operator selects EACH row of the grid
''' and fills the bottom part -- the angajament (those with an indicator on the operation's source
''' and classification come first) and its indicator (chosen by itself when the angajament has only
''' one). A complete row gets its tick in the first column. «Salvează tot» is enabled only when
''' EVERY row is ticked, and saves them ALL at once, all or nothing.</para>
'''
''' <para><b>Note numbers</b> (operator, slice 0088-05): the number is per row. Choosing an
''' angajament already chosen on another row brings that row's number; a new angajament gets the
''' next number free on the server and in this window. One number = one note = one PDF. A number
''' given to two angajamente, or already used on the server this year, stops the save: nothing is
''' stored. Each note is then offered for upload into CAB on its own.</para>
'''
''' <para>«Ieșire» saves nothing: the operations are already in <c>FX_Operatiuni</c> (the login
''' stored them) and come back through the menu.</para>
''' </summary>
Public Class CabNoteForm

    ' The keys of `grdOperatii`'s columns -- identical to the designer's.
    Private Const ColDone As String = "gata"
    Private Const ColReference As String = "referinta"
    Private Const ColDocument As String = "nr_doc"
    Private Const ColDate As String = "data"
    Private Const ColKind As String = "tip"
    Private Const ColSsi As String = "ssi"
    Private Const ColAmount As String = "suma"
    Private Const ColState As String = "stare"

    Private ReadOnly _operations As List(Of UncorrectedOperation)
    Private ReadOnly _preparation As CabNotePreparation
    Private ReadOnly _api As ICabNotesApi
    Private ReadOnly _uploadToCab As Func(Of IWin32Window, CabCorrectionNote, String, Task(Of Boolean))
    Private ReadOnly _unitName As String
    Private ReadOnly _unitTaxCode As String

    ' Operation index -> what the operator filled for it so far.
    Private ReadOnly _drafts As New Dictionary(Of Integer, CabNoteCorrection)()
    ' Slice 0088-05: operation index -> the note number typed for it (text, as typed). One note per
    ' number: rows on the same angajament share it, each new angajament gets its own.
    Private ReadOnly _numberTexts As New Dictionary(Of Integer, String)()
    ' What the two combos hold, in their order.
    Private ReadOnly _offeredCommitments As New List(Of CabCommitment)()
    Private ReadOnly _offeredIndicators As New List(Of CabCommitmentIndicator)()
    Private _current As Integer = -1
    Private _loading As Boolean
    Private _saving As Boolean
    Private _saved As List(Of CabCorrectionNote)

    ''' <summary>The notes saved by this window (one per number); empty when closed without saving.</summary>
    Public ReadOnly Property SavedNotes As IReadOnlyList(Of CabCorrectionNote)
        Get
            Return If(_saved, New List(Of CabCorrectionNote)())
        End Get
    End Property

    ''' <param name="operations">The ERR operations to correlate.</param>
    ''' <param name="preparation">Next number and the unit's angajamente with their indicators.</param>
    ''' <param name="uploadToCab">The shell's «upload into CAB» step (asks, uploads, records).</param>
    Public Sub New(operations As IEnumerable(Of UncorrectedOperation), preparation As CabNotePreparation,
                   api As ICabNotesApi, unitName As String, unitTaxCode As String,
                   uploadToCab As Func(Of IWin32Window, CabCorrectionNote, String, Task(Of Boolean)))
        ArgumentNullException.ThrowIfNull(preparation)
        ArgumentNullException.ThrowIfNull(api)
        InitializeComponent()
        _operations = If(operations, Enumerable.Empty(Of UncorrectedOperation)()).Where(Function(o) o IsNot Nothing).ToList()
        _preparation = preparation
        _api = api
        _uploadToCab = uploadToCab
        _unitName = If(unitName, String.Empty)
        _unitTaxCode = If(unitTaxCode, String.Empty)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Opening
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            _loading = True
            Try
                capBar.Text = $"K-BOT — Operațiuni necorelate ({_operations.Count})"
                Text = capBar.Text
                txtDenumire.Text = CabCorrectionNoteRules.EntityName(_unitName)
                txtCif.Text = CabCorrectionNoteRules.TaxCode(_unitTaxCode)

                grdOperatii.ClearRows()
                For i As Integer = 0 To _operations.Count - 1
                    Dim op As UncorrectedOperation = _operations(i)
                    _drafts(i) = NewDraft(op)
                    _numberTexts(i) = String.Empty
                    Dim row As KBotDataRow = grdOperatii.AddRow()
                    row(ColDone) = False
                    row(ColReference) = op.TreasuryReference
                    row(ColDocument) = op.DocumentNumber
                    row(ColDate) = If(op.PaymentDate.HasValue, CabCorrectionNoteRules.FormDate(op.PaymentDate.Value), String.Empty)
                    row(ColKind) = op.Kind
                    row(ColSsi) = DisplaySsi(op)
                    If op.Amount.HasValue Then row(ColAmount) = CDbl(op.Amount.Value)
                    row(ColState) = "De completat"
                    row.Tag = op
                Next
            Finally
                _loading = False
            End Try
            If _operations.Count > 0 Then
                grdOperatii.CurrentRowIndex = 0
                ShowOperation(0)
            Else
                lblStare.Text = "Nu există operațiuni «ERRRRRRRRRR» necorelate."
            End If
            UpdateSaveEnabled()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.OnLoad", ex)
            lblStare.Text = "Fereastra nu s-a putut pregăti. Detalii în jurnalul de erori."
        End Try
    End Sub

    ' What a row starts with: everything FOREXE told about the operation; angajament and
    ' indicator empty.
    Private Shared Function NewDraft(op As UncorrectedOperation) As CabNoteCorrection
        Return New CabNoteCorrection With {
            .IdFxp = op.IdFxp,
            .TreasuryReference = op.TreasuryReference,
            .DocumentNumber = op.DocumentNumber,
            .AccountSymbol = CabCorrectionNoteRules.AccountSymbol(op),
            .ProgramCode = op.Program,
            .OriginalOperationDate = op.PaymentDate.GetValueOrDefault(Date.MinValue),
            .AmountColumn = CabCorrectionNoteRules.AmountColumnFor(op.Kind),
            .Amount = CabCorrectionNoteRules.StornoAmount(op.Amount.GetValueOrDefault()),
            .Explanation = CabCorrectionNoteRules.DefaultExplanation(op)}
    End Function

    ' «02A-650401200103» (a stored row) -> «02A-65.04.01.20.01.03», as FOREXE prints it.
    Private Shared Function DisplaySsi(op As UncorrectedOperation) As String
        Dim c As String = op.Classification
        If c.Length = 12 AndAlso c.All(AddressOf Char.IsDigit) Then
            c = String.Join(".", Enumerable.Range(0, 6).Select(Function(k) c.Substring(k * 2, 2)))
            Return If(op.SectorSource.Length > 0, op.SectorSource & "-" & c, c)
        End If
        Return op.Ssi
    End Function

    Private Sub grdOperatii_SelectionChanged(sender As Object, e As EventArgs) Handles grdOperatii.SelectionChanged
        Try
            If _loading OrElse _saving Then Return
            Dim i As Integer = grdOperatii.CurrentRowIndex
            If i <> _current Then ShowOperation(i)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.grdOperatii_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>Fills the bottom part from row <paramref name="index"/>'s draft.</summary>
    Private Sub ShowOperation(index As Integer)
        _current = index
        If index < 0 OrElse index >= _operations.Count Then Return
        Dim op As UncorrectedOperation = _operations(index)
        Dim d As CabNoteCorrection = _drafts(index)
        _loading = True
        Try
            FillCommitments(op)
            cmbAngajament.SelectedIndex = _offeredCommitments.FindIndex(
                Function(c) String.Equals(c.Code, d.CommitmentCode, StringComparison.OrdinalIgnoreCase))
            If cmbAngajament.SelectedIndex < 0 Then cmbAngajament.Text = String.Empty
            FillIndicators(op)
            cmbIndicator.SelectedIndex = _offeredIndicators.FindIndex(
                Function(i) String.Equals(i.Code, d.IndicatorCode, StringComparison.OrdinalIgnoreCase))
            txtSimbolCont.Text = d.AccountSymbol
            txtSimbolContCorectie.Text = d.CorrectionAccountSymbol
            txtCodProgram.Text = d.ProgramCode
            txtCodProgramCorectie.Text = d.CorrectionProgramCode
            txtDataOper.Text = If(d.OriginalOperationDate = Date.MinValue, String.Empty,
                                  CabCorrectionNoteRules.FormDate(d.OriginalOperationDate))
            txtExplicatii.Text = d.Explanation
            txtNrNota.Text = _numberTexts(index)
        Finally
            _loading = False
        End Try
        ShowRowState(index)
        If _saved Is Nothing AndAlso String.IsNullOrEmpty(d.CommitmentCode) Then ActiveControl = cmbAngajament
    End Sub

    ' The unit's angajamente, those with an indicator on the operation's SS + classification first.
    Private Sub FillCommitments(op As UncorrectedOperation)
        Dim matching As List(Of CabCommitment) = _preparation.Commitments.
            Where(Function(c) c.Indicators.Any(Function(i) CabCorrectionNoteRules.Matches(i, op))).ToList()
        _offeredCommitments.Clear()
        _offeredCommitments.AddRange(matching)
        _offeredCommitments.AddRange(_preparation.Commitments.Where(Function(c) Not matching.Contains(c)))
        cmbAngajament.Items.Clear()
        For Each c As CabCommitment In _offeredCommitments
            cmbAngajament.Items.Add(If(String.IsNullOrWhiteSpace(c.Description), c.Code, c.Code & " — " & c.Description))
        Next
        cmbAngajament.FindFirstGroupCount = matching.Count
    End Sub

    ' The chosen angajament's indicators, those on the operation's SS + classification first.
    Private Sub FillIndicators(op As UncorrectedOperation)
        _offeredIndicators.Clear()
        cmbIndicator.Items.Clear()
        Dim c As CabCommitment = ChosenCommitment()
        If c Is Nothing Then Return
        _offeredIndicators.AddRange(c.Indicators.Where(Function(i) CabCorrectionNoteRules.Matches(i, op)))
        _offeredIndicators.AddRange(c.Indicators.Where(Function(i) Not CabCorrectionNoteRules.Matches(i, op)))
        For Each i As CabCommitmentIndicator In _offeredIndicators
            Dim label As String = i.Code & "  ·  " & i.Ss & "-" & i.Clsf
            If Not String.IsNullOrWhiteSpace(i.Name) Then label &= "  ·  " & i.Name
            cmbIndicator.Items.Add(label)
        Next
    End Sub

    Private Function ChosenCommitment() As CabCommitment
        Dim i As Integer = cmbAngajament.SelectedIndex
        If i < 0 OrElse i >= _offeredCommitments.Count Then Return Nothing
        Return _offeredCommitments(i)
    End Function

    Private Function ChosenIndicator() As CabCommitmentIndicator
        Dim i As Integer = cmbIndicator.SelectedIndex
        If i < 0 OrElse i >= _offeredIndicators.Count Then Return Nothing
        Return _offeredIndicators(i)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Editing -> the row's draft -> its tick
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub cmbAngajament_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAngajament.SelectedIndexChanged
        Try
            If _loading OrElse _current < 0 Then Return
            Dim op As UncorrectedOperation = _operations(_current)
            _loading = True
            Try
                FillIndicators(op)
                txtSimbolContCorectie.Text = String.Empty
                txtCodProgramCorectie.Text = String.Empty
            Finally
                _loading = False
            End Try
            ' Slice 0088-05 (operator): an angajament already used on another row brings that row's
            ' note number; a new one gets the next free number (not on the server, not in this window).
            Dim chosen As CabCommitment = ChosenCommitment()
            If chosen IsNot Nothing Then
                _loading = True
                Try
                    txtNrNota.Text = NumberFor(chosen.Code, _current).ToString(CultureInfo.InvariantCulture)
                    _numberTexts(_current) = txtNrNota.Text
                Finally
                    _loading = False
                End Try
            End If
            ' The operator's rule: an angajament with a single indicator needs no second choice.
            If _offeredIndicators.Count = 1 Then
                cmbIndicator.SelectedIndex = 0
            ElseIf _offeredIndicators.Count > 1 Then
                ActiveControl = cmbIndicator
            End If
            EditorsChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.cmbAngajament_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub cmbIndicator_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbIndicator.SelectedIndexChanged
        Try
            If _loading OrElse _current < 0 Then Return
            Dim ind As CabCommitmentIndicator = ChosenIndicator()
            If ind IsNot Nothing Then
                _loading = True
                Try
                    Dim symbol As String = CabCorrectionNoteRules.AccountSymbol(ind.Ss, ind.ClsfSal)
                    ' A sector K-BOT has no account prefix for yet: the storno row's symbol (as on the
                    ' sample note), left for the operator to check.
                    txtSimbolContCorectie.Text = If(symbol.Length > 0, symbol, txtSimbolCont.Text)
                    txtCodProgramCorectie.Text = CabCorrectionNoteRules.CorrectionProgram(ind, txtCodProgram.Text)
                Finally
                    _loading = False
                End Try
            End If
            EditorsChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.cmbIndicator_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The note number a row on <paramref name="commitmentCode"/> gets: the number of another row of
    ''' this window on the same angajament, else the next number free both on the server and here.
    ''' </summary>
    Private Function NumberFor(commitmentCode As String, row As Integer) As Integer
        For i As Integer = 0 To _operations.Count - 1
            If i = row Then Continue For
            Dim n As Integer = RowNumber(i)
            If n > 0 AndAlso String.Equals(_drafts(i).CommitmentCode, commitmentCode, StringComparison.OrdinalIgnoreCase) Then Return n
        Next
        Dim taken As New HashSet(Of Integer)(_preparation.UsedNumbers)
        For i As Integer = 0 To _operations.Count - 1
            If i <> row AndAlso RowNumber(i) > 0 Then taken.Add(RowNumber(i))
        Next
        Dim candidate As Integer = Math.Max(1, _preparation.NextNumber)
        While taken.Contains(candidate)
            candidate += 1
        End While
        Return candidate
    End Function

    ' The row's note number; 0 when it is empty or not a number.
    Private Function RowNumber(row As Integer) As Integer
        Dim n As Integer
        If Integer.TryParse(If(_numberTexts(row), String.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, n) Then Return n
        Return 0
    End Function

    ' Every editor of the row: the draft follows what is typed.
    Private Sub RowEditor_TextChanged(sender As Object, e As EventArgs) _
        Handles txtSimbolCont.TextChanged, txtSimbolContCorectie.TextChanged, txtCodProgram.TextChanged,
                txtCodProgramCorectie.TextChanged, txtDataOper.TextChanged, txtExplicatii.TextChanged, txtNrNota.TextChanged
        Try
            If _loading OrElse _current < 0 Then Return
            EditorsChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.RowEditor_TextChanged", ex)
        End Try
    End Sub

    ' The notes' header (entity): only the save button depends on it.
    Private Sub HeaderEditor_TextChanged(sender As Object, e As EventArgs) _
        Handles txtDenumire.TextChanged, txtCif.TextChanged
        Try
            If _loading Then Return
            UpdateSaveEnabled()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.HeaderEditor_TextChanged", ex)
        End Try
    End Sub

    Private Sub EditorsChanged()
        If _saved IsNot Nothing Then Return
        Dim d As CabNoteCorrection = _drafts(_current)
        Dim commitment As CabCommitment = ChosenCommitment()
        Dim ind As CabCommitmentIndicator = ChosenIndicator()
        d.CommitmentCode = If(commitment Is Nothing, String.Empty, commitment.Code)
        d.IndicatorCode = If(ind Is Nothing, String.Empty, ind.Code.Trim().ToUpperInvariant())
        d.CodAi = If(ind Is Nothing, String.Empty, ind.CodAi)
        d.AccountSymbol = txtSimbolCont.Text.Trim().ToUpperInvariant()
        d.CorrectionAccountSymbol = txtSimbolContCorectie.Text.Trim().ToUpperInvariant()
        d.ProgramCode = txtCodProgram.Text.Trim()
        d.CorrectionProgramCode = txtCodProgramCorectie.Text.Trim()
        Dim operationDate As Date
        d.OriginalOperationDate = If(Date.TryParseExact(txtDataOper.Text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None, operationDate), operationDate, Date.MinValue)
        d.Explanation = CabCorrectionNoteRules.CleanText(txtExplicatii.Text, CabCorrectionNoteRules.MaxExplanation, allowPunctuation:=True)
        _numberTexts(_current) = txtNrNota.Text.Trim()
        ShowRowState(_current)
    End Sub

    ''' <summary>The row's tick, its «Stare» cell and the line under the editors.</summary>
    Private Sub ShowRowState(index As Integer)
        Dim problems As List(Of String) = RowProblems(index)
        Dim done As Boolean = problems.Count = 0
        If _saved Is Nothing Then
            grdOperatii.Item(ColDone, index) = done
            grdOperatii.Item(ColState, index) = If(done, $"Completă — nota nr. {RowNumber(index)}", "De completat")
            grdOperatii.InvalidateRow(index)
        End If
        If index = _current AndAlso _saved Is Nothing Then
            Dim open As Integer = _operations.Count - CompleteCount()
            lblStare.Text = If(done,
                               If(open = 0, "Toate rândurile sunt complete: «Salvează tot».",
                                            $"Rând complet. Mai sunt de completat {open}."),
                               problems(0))
        End If
        UpdateSaveEnabled()
    End Sub

    ' What keeps a row from its tick: the correction's own rules and a note number.
    Private Function RowProblems(index As Integer) As List(Of String)
        Dim problems As List(Of String) = CabCorrectionNoteRules.ValidateCorrection(_drafts(index), Date.Today)
        If RowNumber(index) <= 0 Then problems.Add("Numărul notei trebuie să fie un număr mai mare ca 0.")
        Return problems
    End Function

    Private Function CompleteCount() As Integer
        Return Enumerable.Range(0, _operations.Count).Where(Function(i) RowProblems(i).Count = 0).Count()
    End Function

    ' Trivial: enabled only when EVERY row is complete (operator, 28.09.2026).
    Private Sub UpdateSaveEnabled()
        btnSalveaza.Enabled = _saved Is Nothing AndAlso Not _saving AndAlso _operations.Count > 0 AndAlso
                              CompleteCount() = _operations.Count
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Saving -- ALL rows, one note per number (slice 0088-05)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The rows grouped into notes, one per number, in number order. The operator's rules (28.09.2026):
    ''' a number belongs to ONE angajament -- given to two in this window, or already used on the
    ''' server this year, is an error and nothing is saved.
    ''' </summary>
    Private Function BuildNotes(errors As List(Of String)) As List(Of CabCorrectionNote)
        Dim today As Date = Date.Today
        Dim entityName As String = CabCorrectionNoteRules.EntityName(txtDenumire.Text)
        Dim taxCode As String = CabCorrectionNoteRules.TaxCode(txtCif.Text)
        Dim notes As New List(Of CabCorrectionNote)()
        For Each g In Enumerable.Range(0, _operations.Count).GroupBy(AddressOf RowNumber).OrderBy(Function(x) x.Key)
            Dim number As Integer = g.Key
            If number <= 0 Then
                errors.Add("Fiecare rând trebuie să aibă numărul notei (un număr mai mare ca 0).")
                Continue For
            End If
            Dim commitments As List(Of String) = g.Select(Function(i) _drafts(i).CommitmentCode).
                Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            If commitments.Count > 1 Then
                errors.Add($"Numărul {number} este dat la angajamente diferite ({String.Join(", ", commitments)}): " &
                           "fiecare angajament nou are numărul lui.")
            End If
            If _preparation.UsedNumbers.Contains(number) Then
                errors.Add($"Numărul {number} este deja folosit de o notă de corecție din {today.Year} pe server.")
            End If
            Dim note As New CabCorrectionNote With {
                .NoteNumber = number, .Year = today.Year, .NoteDate = today,
                .EntityName = entityName, .EntityTaxCode = taxCode}
            For Each i As Integer In g
                note.Corrections.Add(_drafts(i))
            Next
            errors.AddRange(CabCorrectionNoteRules.Validate(note).Select(Function(x) $"Nota nr. {number}: {x}"))
            notes.Add(note)
        Next
        Return notes
    End Function

    Private Async Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        ' UI boundary (async Sub): log and tell, never rethrow.
        Try
            If _saving OrElse _saved IsNot Nothing Then Return
            Dim errors As New List(Of String)()
            Dim notes As List(Of CabCorrectionNote) = BuildNotes(errors)
            If errors.Count > 0 Then
                lblStare.Text = "Nu s-a salvat nimic."
                KBotMessage.Show(Me, "Nu s-a salvat nimic:" & Environment.NewLine & Environment.NewLine &
                                 String.Join(Environment.NewLine, errors.Distinct().Select(Function(x) "• " & x)),
                                 "Nota de corecție", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _saving = True
            UpdateSaveEnabled()
            UseWaitCursor = True
            Dim pdfPaths As New Dictionary(Of CabCorrectionNote, String)()
            Dim pdfWarnings As New List(Of String)()
            Try
                lblStare.Text = If(notes.Count = 1, "Se salvează nota...", $"Se salvează {notes.Count} note...")
                Try
                    ' All or nothing: the server stores every note in one transaction.
                    Await _api.SaveCabNotesAsync(notes, CancellationToken.None)
                Catch ex As ApiException
                    GlobalErrorLog.Write("CabNoteForm.btnSalveaza_Click.Save", ex)
                    lblStare.Text = "Nu s-a salvat nimic."
                    KBotMessage.Show(Me, "Nu s-a salvat nimic: " & ex.Message, "Nota de corecție",
                                     MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End Try

                ' One PDF per note (operator: two numbers = two documents).
                For Each note As CabCorrectionNote In notes
                    lblStare.Text = $"Se face PDF-ul notei nr. {note.NoteNumber}..."
                    Dim current As CabCorrectionNote = note
                    Dim pdfPath As String = Await Task.Run(Function() CabNoteFiles.Generate(current))
                    pdfPaths(note) = pdfPath
                    Try
                        ' Stored unsigned («-»): the note has its document on the server from the start;
                        ' the signed copy replaces it (PdfSigningSession, precedent = this sha).
                        Dim resp As PutPdfResponse = Await _api.UploadCabNotePdfAsync(
                            note.IdNc, File.ReadAllBytes(pdfPath), ApiClient.ShaFaraRand, ApiClient.ShaFaraRand,
                            Nothing, CancellationToken.None)
                        note.PdfSha256 = resp.sha256
                    Catch ex As Exception
                        GlobalErrorLog.Write("CabNoteForm.btnSalveaza_Click.Pdf", ex)
                        pdfWarnings.Add($"Nota nr. {note.NoteNumber}: PDF-ul NU a ajuns pe server ({ex.Message}); " &
                                        "se poate genera din nou din vederea «Note corecție».")
                    End Try
                Next
            Finally
                _saving = False
                UseWaitCursor = False
            End Try

            _saved = notes
            For i As Integer = 0 To _operations.Count - 1
                grdOperatii.Item(ColState, i) = $"Salvată — nota nr. {RowNumber(i)}"
                grdOperatii.InvalidateRow(i)
            Next
            SetEditorsEnabled(False)
            Dim summary As String = If(notes.Count = 1,
                $"Nota nr. {notes(0).NoteNumber} a fost salvată ({notes(0).Corrections.Count} operațiuni).",
                $"Au fost salvate {notes.Count} note: nr. {String.Join(", ", notes.Select(Function(n) n.NoteNumber))}.")
            lblStare.Text = summary
            If pdfWarnings.Count > 0 Then
                KBotMessage.Show(Me, summary & Environment.NewLine & Environment.NewLine & String.Join(Environment.NewLine, pdfWarnings),
                                 "Nota de corecție", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            ' Each note is its own document in CAB: asked (and uploaded) one by one.
            If _uploadToCab IsNot Nothing Then
                For Each note As CabCorrectionNote In notes
                    Dim pdfPath As String = Nothing
                    If pdfPaths.TryGetValue(note, pdfPath) AndAlso pdfPath IsNot Nothing Then
                        Await _uploadToCab(Me, note, pdfPath)
                    End If
                Next
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteForm.btnSalveaza_Click", ex)
            lblStare.Text = "Salvarea s-a oprit. Detalii în jurnalul de erori."
            KBotMessage.Show(Me, "Salvarea notelor s-a oprit: " & ex.Message, "Nota de corecție",
                             MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            UpdateSaveEnabled()
        End Try
    End Sub

    Private Sub SetEditorsEnabled(enabled As Boolean)
        For Each c As Control In {CType(cmbAngajament, Control), cmbIndicator, txtSimbolCont, txtSimbolContCorectie,
                                  txtCodProgram, txtCodProgramCorectie, txtNrNota, txtDataOper, txtExplicatii,
                                  txtDenumire, txtCif}
            c.Enabled = enabled
        Next
    End Sub

End Class
