Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice AV02-03 -- the «Operatii diverse» page (the Access form OP9): the templates of type A on the left (a tree by journal, collapsed, with its own search), the header (note, document number, kind and
''' date of the document, the partner where a line needs one, the amount) and the lines of the chosen template on the right. The amount of the header is copied on every
''' line and can be changed line by line; «Salveaza» posts the lines with an amount (a document and an operation each) through the server. The accounts come from the
''' template on the server: the page sends only the ids of the lines and their amounts.
''' </summary>
Public Class OperatiiDiversePage
    Implements IThemedControl, ISablonPagina

    Private Shared ReadOnly _felDocumente As String() = {"Notă contabilă", "Factură", "Stat de plată", "Extras de cont", "Ordin de plată", "Altele"}

    Private _api As IOperatiiApi
    Private _sablonApi As ISablonOperatiiApi
    Private _gate As ReauthGate
    Private _session As SessionContext
    Private _idSubunitate As Integer
    Private _subunitSs As String = String.Empty
    Private _bound As Boolean
    Private _loaded As Boolean
    Private _busy As Boolean
    Private _salvareIncerta As Boolean
    Private _filling As Boolean
    Private _dirty As Boolean
    Private _catalog As SabloaneOperatiiCatalog
    Private _context As OperatiiContext
    Private _templates As New List(Of SablonOperatieInfo)()
    Private _current As SablonOperatieInfo
    Private _journals As New Dictionary(Of Integer, JurnalInfo)()
    Private _classifications As New Dictionary(Of Integer, SablonClsfInfo)()
    Private _partners As New Dictionary(Of Integer, SablonPartenerInfo)()
    ' The node of every template of the tree (the key is the id of the template).
    Private ReadOnly _nodeOf As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    Public Sub New()
        InitializeComponent()
        cmbPartener.CaptionSelector = Function(k_o) CaptionOfPartner(k_o)
    End Sub

    ''' <summary>Hands the page what it needs and reads the templates and the notes. UI boundary: a failure is shown in the status line.</summary>
    ''' <param name="k_idSubunitate">The subunit in force; Nothing = none (the page stays locked and says so).</param>
    Public Async Sub Bind(k_api As IOperatiiApi, k_sablonApi As ISablonOperatiiApi, k_gate As ReauthGate, k_session As SessionContext,
                          k_idSubunitate As Integer?, k_subunitSs As String)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_sablonApi)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            _api = k_api
            _sablonApi = k_sablonApi
            _gate = k_gate
            _session = k_session
            _subunitSs = If(k_subunitSs, String.Empty)
            ResetAll()
            If Not k_idSubunitate.HasValue Then
                SetStatus("Sursa/Sectorul ales nu are nicio subunitate: operațiile aparțin unei subunități. Creați-o în «Plan de conturi».")
                Return
            End If
            _idSubunitate = k_idSubunitate.Value
            _bound = True
            Await LoadAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.Bind", ex)
        End Try
    End Sub

    ' Bind is also called when the sector+source of the window changes: what the page held for the previous one goes away first.
    Private Sub ResetAll()
        _bound = False
        _loaded = False
        _templates = New List(Of SablonOperatieInfo)()
        _filling = True
        Try
            treeSabloane.Clear()
            _nodeOf.Clear()
            cmbNota.Items.Clear()
            cmbPartener.Items.Clear()
        Finally
            _filling = False
        End Try
        ResetHeader()
    End Sub

    Private Async Function LoadAsync() As Task(Of Boolean)
        Try
            SetBusy(True)
            SetStatus("Se încarcă…")
            Dim k_catalog As SabloaneOperatiiCatalog = Await _gate.RunAsync(
                Function() _sablonApi.GetSabloaneOperatiiAsync(CancellationToken.None)).ConfigureAwait(True)
            Dim k_context As OperatiiContext = Await _gate.RunAsync(
                Function() _api.GetOperatiiContextAsync(_idSubunitate, _session.An, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return False
            _catalog = k_catalog
            _context = k_context
            _journals = k_catalog.Jurnale.ToDictionary(Function(k_j) k_j.IdJurnal)
            _classifications = k_catalog.ClasificatiiLinii.ToDictionary(Function(k_c) k_c.IdClsf)
            _partners = k_catalog.Parteneri.ToDictionary(Function(k_p) k_p.IdPartener)
            _templates = k_catalog.Sabloane.Where(Function(k_s) String.Equals(k_s.Tip, "A", StringComparison.Ordinal) AndAlso
                                                      (k_s.IdSubunitate = 0 OrElse k_s.IdSubunitate = _idSubunitate) AndAlso
                                                      InSource(k_s)).OrderBy(Function(k_s) k_s.Explicatie, StringComparer.CurrentCultureIgnoreCase).ToList()
            _filling = True
            Try
                cmbNota.Items.Clear()
                For Each k_n As OperatiiNotaInfo In k_context.Note
                    cmbNota.Items.Add(CType(k_n.NumarNC.ToString(CultureInfo.InvariantCulture), Object))
                Next
                cmbFelDoc.Items.Clear()
                For Each k_f As String In _felDocumente
                    cmbFelDoc.Items.Add(CType(k_f, Object))
                Next
                cmbPartener.Items.Clear()
                ' The partner of the operation: only the ones of the unit of this sector+source and year (the catalog holds every unit's, so a partner showed up several times).
                For Each k_p As FacturiPlatiPartener In k_context.Parteneri
                    cmbPartener.Items.Add(CType(New SablonPartenerInfo() With {.IdPartener = k_p.IdPartener, .Cod = k_p.Cod, .Denumire = k_p.Denumire}, Object))
                Next
            Finally
                _filling = False
            End Try
            _loaded = True
            BuildTemplates()
            ResetHeader()
            SetStatus(If(_templates.Count = 0, "Nu există șabloane de operații diverse pentru această subunitate. Adăugați-le în «Șabloane».", String.Empty))
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("OperatiiDiversePage.LoadAsync", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.LoadAsync", ex)
            If Not IsDisposed Then SetStatus("Datele nu au putut fi citite. Detalii în jurnalul de erori.")
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    ' The journal of the template has the sector+source of the subunit (when the window knows it).
    Private Function InSource(k_template As SablonOperatieInfo) As Boolean
        If _subunitSs.Length = 0 Then Return True
        Dim k_journal As JurnalInfo = Nothing
        Return _journals.TryGetValue(k_template.IdJurnal, k_journal) AndAlso String.Equals(k_journal.SursaSector, _subunitSs, StringComparison.Ordinal)
    End Function

    Private Shared Function CaptionOfPartner(k_item As Object) As String
        Dim k_p As SablonPartenerInfo = TryCast(k_item, SablonPartenerInfo)
        Return If(k_p Is Nothing, Convert.ToString(k_item, CultureInfo.InvariantCulture), $"{k_p.Denumire} ({k_p.Cod})")
    End Function

    ' ── The tree of templates ───────────────────────────────────────────────────

    ' Built once per read: a folder per journal (closed), the operations of that journal under it. A pick never builds it again, so the folders the operator
    ' opened stay open; the search of the tree finds an operation by its explanation and opens the folder it is in.
    Private Sub BuildTemplates()
        _filling = True
        Try
            treeSabloane.Clear()
            _nodeOf.Clear()
            Dim k_folder As Image = My.Resources.Resources.folder_open
            Dim k_groups As IEnumerable(Of IGrouping(Of Integer, SablonOperatieInfo)) = _templates.GroupBy(Function(k_t) k_t.IdJurnal).
                OrderBy(Function(k_g) JournalCaption(k_g.Key), StringComparer.CurrentCultureIgnoreCase)
            For Each k_group As IGrouping(Of Integer, SablonOperatieInfo) In k_groups
                Dim k_parent As AdvancedTreeControl.TreeItem = treeSabloane.AddItem(
                    $"J|{k_group.Key.ToString(CultureInfo.InvariantCulture)}", Escape(JournalCaption(k_group.Key)), pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                k_parent.Bold = True
                For Each k_t As SablonOperatieInfo In k_group
                    Dim k_node As AdvancedTreeControl.TreeItem = treeSabloane.AddItem(
                        $"O|{k_t.IdSablon.ToString(CultureInfo.InvariantCulture)}", Escape(If(String.IsNullOrWhiteSpace(k_t.Explicatie), "(fără explicație)", k_t.Explicatie)), k_parent)
                    k_node.Tag = k_t
                    _nodeOf(k_t.IdSablon) = k_node
                Next
            Next
        Finally
            _filling = False
        End Try
        treeSabloane.Invalidate()
        treeSabloane.Enabled = Not _busy
    End Sub

    Private Function JournalCaption(k_idJournal As Integer) As String
        Dim k_journal As JurnalInfo = Nothing
        If Not _journals.TryGetValue(k_idJournal, k_journal) Then Return "Fără jurnal"
        Return If(String.IsNullOrWhiteSpace(k_journal.Denumire), k_journal.Cod, $"{k_journal.Cod} - {k_journal.Denumire}")
    End Function

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    ' The selected node follows the operation in work (none in work = none selected); the folder of it is opened.
    Private Sub SyncSelection()
        If _current Is Nothing Then
            treeSabloane.SelectedNode = Nothing
            Return
        End If
        Dim k_node As AdvancedTreeControl.TreeItem = Nothing
        If _nodeOf.TryGetValue(_current.IdSablon, k_node) Then treeSabloane.SelectAndReveal(k_node)
    End Sub

    ' UI boundary (event handler). A click on a folder only opens or closes it (the tree does that); a click on an operation chooses it.
    Private Sub TreeSabloane_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeSabloane.NodeMouseUp
        Try
            If _filling OrElse _busy OrElse Not _loaded OrElse k_node Is Nothing OrElse e.Button <> MouseButtons.Left Then Return
            Dim k_template As SablonOperatieInfo = TryCast(k_node.Tag, SablonOperatieInfo)
            If k_template Is Nothing OrElse (_current IsNot Nothing AndAlso k_template.IdSablon = _current.IdSablon) Then Return
            If _dirty AndAlso KBotMessage.Show(Me, "Operația în lucru are date necomplet salvate." & vbLf & "Renunțați la ea și alegeți alta?", "Operații diverse",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                SyncSelection()
                Return
            End If
            Choose(k_template)
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.TreeSabloane_NodeMouseUp", ex)
        End Try
    End Sub

    ' ── The header and the lines ────────────────────────────────────────────────

    ' The template is chosen: the header wakes up (the partner only when a line needs it) and the lines show with the amount of the header.
    Private Sub Choose(k_template As SablonOperatieInfo)
        _filling = True
        Try
            _current = k_template
            _dirty = False
            txtSuma.Text = String.Empty
            txtNrDoc.Text = String.Empty
            cmbPartener.SelectedIndex = -1
            cmbPartener.Text = String.Empty
            FillLines(0D)
            Dim k_needsPartner As Boolean = NeedsPartner()
            cmbPartener.Enabled = k_needsPartner
            cmbPartener.PlaceholderText = If(k_needsPartner, "Alegeți partenerul", "Nu se cere partener")
        Finally
            _filling = False
        End Try
        SetHeaderEnabled(True)
        SetStatus(String.Empty)
        SyncSelection()
        RefreshButtons()
    End Sub

    Private Function NeedsPartner() As Boolean
        Return _current IsNot Nothing AndAlso _current.Linii.Any(Function(k_l) String.Equals(k_l.PartDebitMod, SablonOperatiiModuri.Operatie, StringComparison.Ordinal) OrElse
                                                                  String.Equals(k_l.PartCreditMod, SablonOperatiiModuri.Operatie, StringComparison.Ordinal))
    End Function

    Private Sub FillLines(k_amount As Decimal)
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            If _current Is Nothing Then Return
            For Each k_l As SablonLinieInfo In _current.Linii.OrderBy(Function(k_x) k_x.IdLinie)
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = k_l
                Dim k_clsf As SablonClsfInfo = Nothing
                k_row("clsf") = If(k_l.IdClsf > 0 AndAlso _classifications.TryGetValue(k_l.IdClsf, k_clsf), k_clsf.Clsf, String.Empty)
                k_row("explicatie") = _current.Explicatie
                k_row("contd") = Symbol(k_l.ContDebit, k_l.AnaliticDebit)
                k_row("contc") = Symbol(k_l.ContCredit, k_l.AnaliticCredit)
                k_row("suma") = k_amount
            Next
            ShowPartners()
        Finally
            gridLinii.EndUpdate()
        End Try
    End Sub

    Private Shared Function Symbol(k_account As Integer, k_analytic As String) As String
        Return If(String.IsNullOrEmpty(k_analytic), k_account.ToString(CultureInfo.InvariantCulture), $"{k_account.ToString(CultureInfo.InvariantCulture)}.{k_analytic}")
    End Function

    ' The partner columns: the partner of the operation (the one chosen in the header, else a reminder), the fixed one by name, none by blank.
    Private Sub ShowPartners()
        Dim k_chosen As SablonPartenerInfo = TryCast(cmbPartener.SelectedItem, SablonPartenerInfo)
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_l As SablonLinieInfo = TryCast(k_row.Tag, SablonLinieInfo)
            If k_l Is Nothing Then Continue For
            k_row("partd") = PartnerText(k_l.PartDebitMod, k_l.IdPartDebit, k_chosen)
            k_row("partc") = PartnerText(k_l.PartCreditMod, k_l.IdPartCredit, k_chosen)
        Next
        gridLinii.Invalidate()
    End Sub

    Private Function PartnerText(k_mode As String, k_idFixed As Integer, k_chosen As SablonPartenerInfo) As String
        If String.Equals(k_mode, SablonOperatiiModuri.Operatie, StringComparison.Ordinal) Then
            Return If(k_chosen Is Nothing, "(partenerul operației)", k_chosen.Denumire)
        End If
        Dim k_fixed As SablonPartenerInfo = Nothing
        If String.Equals(k_mode, SablonOperatiiModuri.Fix, StringComparison.Ordinal) AndAlso _partners.TryGetValue(k_idFixed, k_fixed) Then Return k_fixed.Denumire
        Return String.Empty
    End Function

    Private Sub SetHeaderEnabled(k_on As Boolean)
        cmbNota.Enabled = k_on
        txtNrDoc.Enabled = k_on
        cmbFelDoc.Enabled = k_on
        dtpData.Enabled = k_on
        txtSuma.Enabled = k_on
        If Not k_on Then cmbPartener.Enabled = False
    End Sub

    ' Nothing chosen and nothing typed: the header goes back to its resting state (next note, today's date inside the work year).
    Private Sub ResetHeader()
        _filling = True
        Try
            _current = Nothing
            _dirty = False
            gridLinii.ClearRows()
            txtNrDoc.Text = String.Empty
            txtSuma.Text = String.Empty
            cmbFelDoc.Text = String.Empty
            cmbPartener.SelectedIndex = -1
            cmbPartener.Text = String.Empty
            cmbPartener.PlaceholderText = "Partener"
            cmbNota.Text = If(_context Is Nothing OrElse _context.Urmatoare <= 0, String.Empty, _context.Urmatoare.ToString(CultureInfo.InvariantCulture))
            Dim k_year As Integer = If(_session Is Nothing OrElse _session.An = 0, Date.Today.Year, _session.An)
            dtpData.Value = If(Date.Today.Year = k_year, Date.Today, New Date(k_year, 1, 1))
        Finally
            _filling = False
        End Try
        SetHeaderEnabled(False)
        RefreshButtons()
    End Sub

    Private Sub Header_Changed(sender As Object, e As EventArgs) Handles txtNrDoc.TextChanged, dtpData.ValueChanged
        Try
            If _filling OrElse _current Is Nothing Then Return
            _dirty = True
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.Header_Changed", ex)
        End Try
    End Sub

    Private Sub CmbPartener_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPartener.SelectedIndexChanged
        Try
            If _filling OrElse _current Is Nothing Then Return
            _dirty = True
            ShowPartners()
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.CmbPartener_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The amount of the header goes on every line.
    Private Sub TxtSuma_TextChanged(sender As Object, e As EventArgs) Handles txtSuma.TextChanged
        Try
            If _filling OrElse _current Is Nothing Then Return
            Dim k_amount As Decimal = ParseAmount(txtSuma.Text)
            _filling = True
            Try
                For Each k_row As KBotDataRow In gridLinii.Rows
                    k_row("suma") = k_amount
                Next
            Finally
                _filling = False
            End Try
            gridLinii.Invalidate()
            _dirty = True
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.TxtSuma_TextChanged", ex)
        End Try
    End Sub

    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If _filling OrElse Not String.Equals(e.ColumnKey, "suma", StringComparison.Ordinal) Then Return
            _dirty = True
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ' A number typed with a comma or a dot; anything else is zero.
    Private Shared Function ParseAmount(k_value As Object) As Decimal
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then Return 0D
        If TypeOf k_value Is Decimal Then Return CDec(k_value)
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim().Replace(" "c, String.Empty)
        Dim k_result As Decimal
        If Decimal.TryParse(k_text, NumberStyles.Number, CultureInfo.InvariantCulture, k_result) Then Return k_result
        If Decimal.TryParse(k_text.Replace(","c, "."c), NumberStyles.Number, CultureInfo.InvariantCulture, k_result) Then Return k_result
        If Decimal.TryParse(k_text, NumberStyles.Number, New CultureInfo("ro-RO"), k_result) Then Return k_result
        Return 0D
    End Function

    ' ── Save and give up ────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.BtnSalveaza_Click", ex)
        End Try
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            If _current Is Nothing Then Return
            If _dirty AndAlso KBotMessage.Show(Me, "Renunțați la operația în lucru?", "Operații diverse", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            ResetHeader()
            SyncSelection()
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.BtnRenunta_Click", ex)
        End Try
    End Sub

    ' Checks what the server also checks, so the operator is told at once; the server stays the judge.
    Private Function ValidateRequest(ByRef k_request As OperatiiDiverseCerere) As String
        If _current Is Nothing Then Return "Alegeți o operație din listă."
        Dim k_note As Integer
        If Not Integer.TryParse(cmbNota.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_note) OrElse k_note <= 0 Then Return "Nota contabilă trebuie să fie un număr întreg pozitiv."
        If txtNrDoc.Text.Trim().Length = 0 Then Return "Scrieți numărul documentului."
        If cmbFelDoc.Text.Trim().Length = 0 Then Return "Scrieți sau alegeți felul documentului."
        Dim k_date As Date = dtpData.Value.Date
        If _session IsNot Nothing AndAlso _session.An <> 0 AndAlso k_date.Year <> _session.An Then Return $"Data documentului trebuie să fie în anul de lucru ({_session.An})."
        If _context IsNot Nothing AndAlso _context.Inchidere.HasValue AndAlso k_date <= _context.Inchidere.Value Then
            Return $"Luna este închisă: data documentului trebuie să fie după {_context.Inchidere.Value:dd.MM.yyyy}."
        End If
        Dim k_partner As SablonPartenerInfo = TryCast(cmbPartener.SelectedItem, SablonPartenerInfo)
        If NeedsPartner() AndAlso k_partner Is Nothing Then Return "Alegeți partenerul: operația are linii cu «partenerul operației»."
        Dim k_lines As New List(Of OperatiiDiverseLinie)()
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_l As SablonLinieInfo = TryCast(k_row.Tag, SablonLinieInfo)
            Dim k_amount As Decimal = ParseAmount(k_row("suma"))
            If k_l IsNot Nothing AndAlso k_amount <> 0D Then k_lines.Add(New OperatiiDiverseLinie() With {.IdLinie = k_l.IdLinie, .Suma = k_amount})
        Next
        If k_lines.Count = 0 Then Return "Nicio linie nu are sumă: nu este nimic de înregistrat."
        Dim k_nota As OperatiiNotaInfo = _context.Note.FirstOrDefault(Function(nota) nota.NumarNC = k_note)
        k_request = New OperatiiDiverseCerere() With {
            .IdSubunitate = _idSubunitate, .An = If(_session Is Nothing OrElse _session.An = 0, k_date.Year, _session.An), .IdSablon = _current.IdSablon, .NumarNC = k_note,
            .IdNota = If(k_nota IsNot Nothing AndAlso k_nota.IdNota > 0, k_nota.IdNota, CType(Nothing, Integer?)),
            .NotaNoua = If(k_nota Is Nothing OrElse k_nota.IdNota > 0, CType(k_nota Is Nothing, Boolean?), Nothing),
            .NumarDocument = txtNrDoc.Text.Trim(), .FelDocument = cmbFelDoc.Text.Trim(), .DataDocument = k_date,
            .IdPartener = If(k_partner Is Nothing, 0, k_partner.IdPartener), .Linii = k_lines}
        Return String.Empty
    End Function

    Private Async Function SaveAsync() As Task(Of Boolean)
        If Not _bound OrElse _busy OrElse _salvareIncerta Then Return False
        Dim k_request As OperatiiDiverseCerere = Nothing
        Dim k_problem As String = ValidateRequest(k_request)
        If k_problem.Length > 0 Then
            SetStatus(k_problem)
            KBotMessage.Show(Me, k_problem, "Operații diverse", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Dim k_asteaptaRaspuns As Boolean = False
        Try
            SetBusy(True)
            k_asteaptaRaspuns = True
            Dim k_result As OperatiiDiverseRezultat = Await _gate.RunAsync(
                Function() _api.SalveazaOperatiiDiverseAsync(k_request, CancellationToken.None)).ConfigureAwait(True)
            k_asteaptaRaspuns = False
            If IsDisposed Then Return True
            _dirty = False
            KBotMessage.Show(Me, $"Au fost înregistrate {k_result.Operatii} operații în nota contabilă {k_result.NumarNC}.", "Operații diverse", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Dim k_template As SablonOperatieInfo = _current
            Await ReloadContextAsync().ConfigureAwait(True)
            ResetHeader()
            If k_template IsNot Nothing Then Choose(k_template) Else SyncSelection()
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("OperatiiDiversePage.SaveAsync", ex)
            _salvareIncerta = k_asteaptaRaspuns AndAlso (Not ex.StatusCode.HasValue OrElse ex.StatusCode.Value >= 500)
            Dim mesaj As String = If(_salvareIncerta, "Rezultatul salvării este incert. Verificați lista operațiilor înainte de a relua.", ex.Message)
            SetStatus(mesaj)
            KBotMessage.Show(Me, mesaj, "Operații diverse", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.SaveAsync", ex)
            _salvareIncerta = k_asteaptaRaspuns
            SetStatus(If(_salvareIncerta, "Rezultatul salvării este incert. Verificați lista operațiilor înainte de a relua.",
                         "Datele operațiilor nu au putut fi reîncărcate. Detalii în jurnalul de erori."))
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    ' After a save the notes (and the next number) are read again.
    Private Async Function ReloadContextAsync() As Task
        _context = Await _gate.RunAsync(Function() _api.GetOperatiiContextAsync(_idSubunitate, _session.An, CancellationToken.None)).ConfigureAwait(True)
        _filling = True
        Try
            cmbNota.Items.Clear()
            For Each k_n As OperatiiNotaInfo In _context.Note
                cmbNota.Items.Add(CType(k_n.NumarNC.ToString(CultureInfo.InvariantCulture), Object))
            Next
        Finally
            _filling = False
        End Try
    End Function

    ' ── The window's questions (ISablonPagina) ──────────────────────────────────

    Public ReadOnly Property HasPending As Boolean Implements ISablonPagina.HasPending
        Get
            Return _current IsNot Nothing AndAlso _dirty AndAlso Not _salvareIncerta
        End Get
    End Property

    Public Async Function CommitAsync() As Task(Of Boolean) Implements ISablonPagina.CommitAsync
        Try
            If Not HasPending Then Return True
            Dim k_answer As DialogResult = KBotMessage.Show(Me, "Operația în lucru nu este salvată." & vbLf & "O salvați?", "Operații diverse",
                                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            _dirty = False
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.CommitAsync", ex)
            Return False
        End Try
    End Function

    ' ── State of the controls ───────────────────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        treeSabloane.Enabled = Not k_busy
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        btnSalveaza.Enabled = _loaded AndAlso Not (_busy OrElse _salvareIncerta) AndAlso _current IsNot Nothing
        btnRenunta.Enabled = _loaded AndAlso Not _busy AndAlso _current IsNot Nothing
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.PanelBack()
            For Each k_c As Control In {tlyPagina, tlyStanga, tlyDreapta, tlyAntet, tlyJos}
                k_c.BackColor = k_scheme.PanelBack()
            Next
            lblStare.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiDiversePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
