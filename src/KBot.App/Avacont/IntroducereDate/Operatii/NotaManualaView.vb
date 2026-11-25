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
''' «Notă manuală» / «Modifică operație» (slice AV02-07, the Access form Note): one operation with its debit and credit lines, saved by the shared posting service.
''' Partials: .Protectie (slice AV08-01, the accounts kept to the classification), .Salvare (slice AV08-05, the save, the warnings, the boxes marked in error).
''' </summary>
Public Class NotaManualaView
    Friend Const KeyDocumentNou As String = "document_nou"
    Friend Const KeyOperatieNoua As String = "operatie_noua"
    Friend Const KeyReincarca As String = "reincarca"
    Friend Const KeySalveaza As String = "salveaza"

    Friend Event StareActiuniSchimbata As EventHandler
    Friend Event SalvareFinalizata As EventHandler

    Private Shared ReadOnly Ro As CultureInfo = CultureInfo.GetCultureInfo("ro-RO")
    Private Shared ReadOnly Alfabetic As StringComparer = StringComparer.Create(Ro, True)
    Private Shared ReadOnly FaraPartener As New NotaManualaAlegere With {.Denumire = "Fără partener"}
    Private Shared ReadOnly FaraClsf As New NotaManualaAlegere With {.Denumire = "Fără clasificație"}

    Private _api As INotaManualaApi
    Private _gate As ReauthGate
    Private _sid As Integer
    Private _an As Integer
    Private _idOper As Integer?
    Private _context As NotaManualaContext
    Private _conturi As Dictionary(Of Integer, NotaManualaAlegere)
    Private _conturiGrupate As Dictionary(Of Integer, KBotComboGroupedItem)
    Private _clasificatiiGrupate As Dictionary(Of Integer, KBotComboGroupedItem)
    Private _parteneri As Dictionary(Of Integer, NotaManualaAlegere)
    Private _optiuniCont As List(Of Object)
    Private _optiuniPartener As List(Of Object)
    Private _alegereTooltip As NotaManualaAlegere
    Private _loading As Boolean = True
    Private _saving As Boolean
    Private _incerta As Boolean
    Private _conflict As Boolean
    Private _reincarcareDisponibila As Boolean
    Private _reincarcareInCurs As Boolean
    Private _avertismentValidareInCurs As Boolean
    Private _rezultat As NotaManualaRezultat

    ''' <summary>True once the server saved the operation (see <see cref="Rezultat"/>).</summary>
    Public ReadOnly Property Salvat As Boolean
        Get
            Return _rezultat IsNot Nothing
        End Get
    End Property

    ''' <summary>Slice AV08-05: what the server saved (ids, numbers, dates); Nothing while nothing was saved.</summary>
    Public ReadOnly Property Rezultat As NotaManualaRezultat
        Get
            Return _rezultat
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
    End Sub

    Friend Sub Configureaza(api As INotaManualaApi, gate As ReauthGate, sid As Integer, an As Integer, idOper As Integer?)
        _api = api
        _gate = gate
        _sid = sid
        _an = an
        _idOper = idOper
        Text = If(_idOper.HasValue, "Modifică operație", "Notă manuală")
        For Each k_grid As KBotDataView In {gridDebit, gridCredit}
            k_grid.Column("suma").ValueFormatter = Function(k_value) CDec(k_value).ToString("F2", Ro)
        Next
        RaiseEvent StareActiuniSchimbata(Me, EventArgs.Empty)
    End Sub

    Friend ReadOnly Property SalvareInCurs As Boolean
        Get
            Return _saving
        End Get
    End Property

    Private ReadOnly Property EditarePermisa As Boolean
        Get
            Return _context IsNot Nothing AndAlso Not (_loading OrElse _saving) AndAlso String.IsNullOrEmpty(_context.Detalii?.Refuz)
        End Get
    End Property

    Private ReadOnly Property EditareCompleta As Boolean
        Get
            Return EditarePermisa AndAlso (_context.Detalii Is Nothing OrElse _context.Detalii.EditareCompleta)
        End Get
    End Property

    Friend Function StareActiune(cheie As String) As ViewButtonState
        Dim activa As Boolean
        Select Case cheie
            Case KeyDocumentNou, KeyOperatieNoua
                If _idOper.HasValue Then Return ViewButtonState.Hidden
                activa = EditareCompleta AndAlso (_adaugaLaDocument = (cheie = KeyDocumentNou))
            Case KeyReincarca
                If Not _reincarcareDisponibila Then Return ViewButtonState.Hidden
                activa = Not (_reincarcareInCurs OrElse _saving)
            Case KeySalveaza
                activa = EditarePermisa AndAlso Not (_incerta OrElse _conflict)
            Case Else
                Return ViewButtonState.Hidden
        End Select
        Return If(activa, ViewButtonState.Enabled, ViewButtonState.Dimmed)
    End Function

    Friend Async Function ExecutaActiuneAsync(cheie As String) As Task
        If _confirmareInCurs OrElse _avertismentValidareInCurs OrElse StareActiune(cheie) <> ViewButtonState.Enabled Then Return
        Select Case cheie
            Case KeyDocumentNou, KeyOperatieNoua
                Await SchimbaModDocumentAsync(cheie = KeyOperatieNoua)
            Case KeyReincarca
                Await IncarcaDateAsync()
            Case KeySalveaza
                Await SalveazaAsync()
        End Select
    End Function

    Friend Async Function IncarcaDateAsync() As Task
        If _api Is Nothing OrElse _reincarcareInCurs OrElse _saving Then Return
        _reincarcareInCurs = True
        Try
            Await IncarcaAsync()
        Catch ex As Exception
            _reincarcareDisponibila = True
            ArataEroare(ex)
        Finally
            _reincarcareInCurs = False
            If Not IsDisposed Then RaiseEvent StareActiuniSchimbata(Me, EventArgs.Empty)
        End Try
    End Function

    ''' <summary>Reads the lists and, when editing, the operation; fills the window. Called at opening and by «Reîncarcă» after a conflict (slice AV08-05).</summary>
    Private Async Function IncarcaAsync() As Task
        _loading = True
        ActualizeazaCampuri()
        Dim k_context As NotaManualaContext = Await _gate.RunAsync(Function() _api.GetNotaManualaContextAsync(_sid, _an, _idOper, CancellationToken.None))
        If IsDisposed Then Return
        _context = k_context
        ' Slice AV08-01: the tick of the user; never on the last closing document (Access: Protectie off for Fel I).
        _protectie = AppSettings.Current.AvacontProtectieOperatii AndAlso Not (_context.Detalii IsNot Nothing AndAlso _context.Detalii.Inchidere)
        Text = $"{If(_idOper.HasValue, "Modifică operație", "Notă manuală")} · {_context.Subunitate} · {_an}"
        cmbNota.Items.Clear()
        cmbNota.CaptionSelector = Function(item) DirectCast(item, NotaManualaNota).NumarNC.ToString(Ro)
        cmbNota.Items.AddRange(_context.Note.OrderByDescending(Function(n) n.NumarNC).Cast(Of Object)().ToList())
        IncarcaAntet()
        cmbClsf.Items.Clear()
        cmbClsf.Items.Add(FaraClsf)
        Dim clasificatii = _context.Clasificatii.OrderBy(Function(c) c.Articol, Alfabetic).ThenBy(Function(c) c.Cod, Alfabetic).
            ThenBy(Function(c) c.Denumire, Alfabetic).
            Select(Function(c) New KBotComboGroupedItem(c.ToString(), If(String.IsNullOrWhiteSpace(c.Articol), "Fără articol", c.Articol), c.ToString(), c)).ToList()
        _clasificatiiGrupate = clasificatii.ToDictionary(Function(c) DirectCast(c.Tag, NotaManualaAlegere).Id)
        cmbClsf.Items.AddRange(clasificatii.Cast(Of Object)())
        cmbClsf.SelectedItem = FaraClsf
        cmbJurnal.Items.Clear()
        cmbJurnal.Items.AddRange(SorteazaAlegere(_context.Jurnale))
        _conturi = _context.Conturi.ToDictionary(Function(c) c.Id)
        _parteneri = _context.Parteneri.ToDictionary(Function(p) p.Id)
        Dim conturi = _context.Conturi.OrderBy(Function(c) c.Cont).ThenBy(Function(c) c.Analitic, Alfabetic).
            ThenBy(Function(c) c.Cod, Alfabetic).ThenBy(Function(c) c.Denumire, Alfabetic).
            Select(Function(c) New KBotComboGroupedItem(c.ToString(),
                If(c.Cont.HasValue, c.Cont.GetValueOrDefault().ToString(Ro), "Fără cont de bază"), c.ToString(), c)).ToList()
        _conturiGrupate = conturi.ToDictionary(Function(c) DirectCast(c.Tag, NotaManualaAlegere).Id)
        _optiuniCont = conturi.Cast(Of Object)().ToList()
        _optiuniPartener = SorteazaAlegere(_context.Parteneri)
        _optiuniPartener.Insert(0, FaraPartener)
        gridDebit.ClearRows()
        gridCredit.ClearRows()
        chkDocument.Checked = False
        Dim zi As Date = If(Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
        dtpDocument.Value = zi
        dtpOperatie.Value = zi
        If _context.Detalii IsNot Nothing Then
            CompleteazaEditare(_context.Detalii)
        End If
        CurataMarcaje()
        _conflict = False
        _reincarcareDisponibila = False
        _loading = False
        ActualizeazaCampuri()
        Dim mesaj As String = _context.Detalii?.Refuz
        If String.IsNullOrEmpty(mesaj) Then mesaj = _context.Detalii?.Avertisment
        If Not String.IsNullOrEmpty(mesaj) Then ArataAvertisment(mesaj)
        Await IncarcaDocumenteleNoteiAsync()
    End Function

    Private Shared Function SorteazaAlegere(items As IEnumerable(Of NotaManualaAlegere)) As List(Of Object)
        Return items.OrderBy(Function(item) item.Cod, Alfabetic).ThenBy(Function(item) item.Denumire, Alfabetic).Cast(Of Object)().ToList()
    End Function

    Private Shared Function DinGrup(item As Object) As NotaManualaAlegere
        Return TryCast(If(TryCast(item, KBotComboGroupedItem)?.Tag, item), NotaManualaAlegere)
    End Function

    Private Sub CompleteazaEditare(detalii As NotaManualaDetalii)
        Dim operatie As NotaManualaOperatie = detalii.Operatie
        Dim doc As NotaManualaDocument = detalii.Document
        CompleteazaDocument(doc, actualizeazaNumar:=True)
        dtpOperatie.Value = operatie.DataOperatie.ToDateTime(TimeOnly.MinValue)
        cmbExplicatie.Text = If(operatie.Explicatie, String.Empty).ToUpper(Ro)
        cmbCompartiment.Text = If(operatie.Compartiment, String.Empty).ToUpper(Ro)
        chkChelt.Checked = operatie.Chelt <> 0
        chkCfpp.Checked = operatie.Cfpp <> 0
        cmbJurnal.SelectedItem = _context.Jurnale.FirstOrDefault(Function(j) j.Id = operatie.IdJurnal)
        Dim clsf As KBotComboGroupedItem = Nothing
        _clasificatiiGrupate.TryGetValue(operatie.IdClsf.GetValueOrDefault(), clsf)
        cmbClsf.SelectedItem = If(CObj(clsf), FaraClsf)
        IncarcaLinii(gridDebit, detalii.Debit)
        IncarcaLinii(gridCredit, detalii.Credit)
    End Sub

    Private Sub IncarcaLinii(grid As KBotDataView, linii As IEnumerable(Of NotaManualaLinie))
        grid.BeginUpdate()
        Try
            For Each linie As NotaManualaLinie In linii.OrderBy(Function(l) _conturi(l.IdCont).Cont).
                    ThenBy(Function(l) _conturi(l.IdCont).Analitic, Alfabetic).
                    ThenBy(Function(l) _conturi(l.IdCont).Cod, Alfabetic).
                    ThenBy(Function(l) _conturi(l.IdCont).Denumire, Alfabetic)
                AdaugaRand(grid, linie)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Sub AdaugaRand(grid As KBotDataView, linie As NotaManualaLinie)
        Dim row As KBotDataRow = grid.AddRow()
        row("partener") = FaraPartener
        If linie Is Nothing Then Return
        row.Tag = linie.IdLinie
        row("cont") = _conturiGrupate(linie.IdCont)
        If linie.IdPartener.HasValue Then row("partener") = _parteneri(linie.IdPartener.Value)
        row("suma") = linie.Suma
    End Sub

    ' Slice AV08-05, Access DataDoc_AfterUpdate (DataOp = DataDoc): the operation takes the date of the document whenever the operator sets it, and for a new
    ' operation also when it goes into an existing document. The original date of the edited document put back by «Modifică documentul» unticked is left alone.
    Private Sub DataDocument_Changed(sender As Object, e As EventArgs) Handles dtpDocument.ValueChanged
        If _loading Then Return
        If Not _idOper.HasValue OrElse chkDocument.Checked Then dtpOperatie.Value = dtpDocument.Value
    End Sub

    Private Sub ActualizeazaCampuri()
        Dim detalii As NotaManualaDetalii = _context?.Detalii
        Dim gata As Boolean = _context IsNot Nothing AndAlso Not _loading AndAlso Not _saving
        Dim permite As Boolean = EditarePermisa
        Dim completa As Boolean = EditareCompleta
        Dim antet As Boolean = completa AndAlso If(detalii Is Nothing, Not _adaugaLaDocument, chkDocument.Checked)
        pnlAntet.Enabled = gata
        cmbNota.Enabled = completa AndAlso (detalii Is Nothing OrElse chkDocument.Checked)
        chkDocument.Visible = detalii IsNot Nothing AndAlso permite AndAlso detalii.EditareDocument
        cmbDocument.Enabled = antet OrElse (completa AndAlso _adaugaLaDocument AndAlso Not _incarcaDocumente AndAlso _idNotaDocumente.HasValue)
        cmbFelDocument.Enabled = antet
        cmbTipOperatie.Enabled = antet AndAlso (detalii Is Nothing OrElse detalii.SchimbareTip)
        dtpDocument.Enabled = antet
        dtpOperatie.Enabled = completa
        cmbJurnal.Enabled = completa
        cmbClsf.Enabled = completa
        chkCfpp.Enabled = completa
        chkChelt.Enabled = completa
        cmbExplicatie.Enabled = permite
        cmbCompartiment.Enabled = permite
        gridDebit.ReadOnlyGrid = Not completa
        gridCredit.ReadOnlyGrid = Not completa
        Dim vizibilitate = If(completa, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
        gridDebit.Column("sterge").Visible = vizibilitate
        gridCredit.Column("sterge").Visible = vizibilitate
        If completa Then
            PastreazaRandDeAdaugare(gridDebit)
            PastreazaRandDeAdaugare(gridCredit)
        End If
        RaiseEvent StareActiuniSchimbata(Me, EventArgs.Empty)
    End Sub

    Private Sub Grid_ComboItems(sender As Object, e As KBotComboItemsEventArgs) Handles gridDebit.ComboItemsRequested, gridCredit.ComboItemsRequested
        e.Items = If(e.ColumnKey = "cont", OptiuniCont(DirectCast(sender, KBotDataView).Rows(e.RowIndex)("cont")), _optiuniPartener)
    End Sub

    ' SLICE-AV08-11: contul și partenerul folosesc tooltipul tematic al view-ului.
    Private Sub Grid_MouseMove(sender As Object, e As MouseEventArgs) Handles gridDebit.MouseMove, gridCredit.MouseMove
        Try
            Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
            Dim coloana As String = grid.ColumnKeyAt(e.Location)
            Dim rand As Integer = grid.RowIndexAt(e.Location)
            Dim esteAlegere As Boolean = rand >= 0 AndAlso (coloana = "cont" OrElse coloana = "partener")
            grid.CellTooltip.Enabled = Not esteAlegere

            Dim alegere As NotaManualaAlegere = If(esteAlegere, DinGrup(grid(coloana, rand)), Nothing)
            If e.Button <> MouseButtons.None OrElse alegere Is Nothing OrElse ReferenceEquals(alegere, FaraPartener) OrElse
                String.IsNullOrWhiteSpace(alegere.Denumire) Then
                AscundeTooltipAlegere()
                Return
            End If
            If ReferenceEquals(_alegereTooltip, alegere) Then Return

            _alegereTooltip = alegere
            Dim continut As New KBotToolTipContent With {.Text = alegere.Denumire.Replace("<", "<" & ChrW(&H200B))}
            KBotToolTip1.ShowAt(grid, continut, grid.PointToScreen(e.Location))
        Catch ex As Exception
            GlobalErrorLog.Write("NotaManualaView.Grid_MouseMove", ex)
        End Try
    End Sub

    Private Sub AscundeTooltipAlegere() Handles gridDebit.MouseLeave, gridCredit.MouseLeave,
        gridDebit.MouseDown, gridCredit.MouseDown, gridDebit.MouseWheel, gridCredit.MouseWheel,
        gridDebit.KeyDown, gridCredit.KeyDown, gridDebit.VisibleChanged, gridCredit.VisibleChanged
        If _alegereTooltip Is Nothing Then Return
        _alegereTooltip = Nothing
        KBotToolTip1.HideNow()
    End Sub

    Private Sub Grid_Validating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridDebit.CellValidating, gridCredit.CellValidating
        Dim mesaj As String
        If e.ColumnKey = "suma" Then
            Dim suma As Decimal
            Dim text As String = Convert.ToString(e.ProposedValue, Ro).Trim()
            Dim format As CultureInfo = If(text.Contains(","c), Ro, CultureInfo.InvariantCulture)
            Dim styles As NumberStyles = NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint
            If Decimal.TryParse(text, styles, format, suma) AndAlso suma <> 0D AndAlso Decimal.Round(suma, 2) = suma Then
                e.ProposedValue = suma
                Return
            End If
            mesaj = "Introduceți o sumă nenulă, cu cel mult două zecimale."
        ElseIf DinGrup(e.ProposedValue) IsNot Nothing Then
            Return
        Else
            mesaj = "Alegeți un rând din listă."
        End If
        e.Cancel = True
        If _avertismentValidareInCurs OrElse Not IsHandleCreated Then Return
        _avertismentValidareInCurs = True
        ' Lăsăm grila să termine validarea înainte ca dialogul să preia focusul.
        BeginInvoke(New Action(Sub()
                                   Try
                                       ArataAvertisment(mesaj)
                                   Finally
                                       _avertismentValidareInCurs = False
                                   End Try
                               End Sub))
    End Sub

    Private Shared Function RandGol(row As KBotDataRow) As Boolean
        Return row.Tag Is Nothing AndAlso row("cont") Is Nothing AndAlso row("suma") Is Nothing AndAlso
            Cheie(DinGrup(row("partener"))) Is Nothing
    End Function

    Private Sub PastreazaRandDeAdaugare(grid As KBotDataView)
        If grid.RowCount = 0 OrElse Not RandGol(grid.Rows(grid.RowCount - 1)) Then AdaugaRand(grid, Nothing)
    End Sub

    Private Sub Grid_Sterge(sender As Object, e As KBotButtonClickEventArgs) Handles gridDebit.ButtonClick, gridCredit.ButtonClick
        Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
        If e.ColumnKey <> "sterge" OrElse grid.ReadOnlyGrid OrElse e.RowIndex < 0 OrElse e.RowIndex >= grid.RowCount OrElse
            RandGol(grid.Rows(e.RowIndex)) Then Return
        If ReferenceEquals(grid, _gridEroare) Then CurataMarcaje()
        grid.RemoveRowAt(e.RowIndex)
        PastreazaRandDeAdaugare(grid)
    End Sub

    Private Sub ArataAvertisment(mesaj As String)
        If IsDisposed OrElse Disposing Then Return
        Dim fereastra As Form = FindForm()
        If fereastra IsNot Nothing AndAlso fereastra.Visible Then
            KBotMessage.Show(fereastra, mesaj, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Friend Sub ArataEroare(ex As Exception, Optional rezultatIncert As Boolean = False)
        GlobalErrorLog.Write("NotaManualaView", ex)
        Dim mesaj As String = If(TypeOf ex Is ApiException OrElse TypeOf ex Is NotaManualaException, ex.Message,
                                 "Acțiunea nu a putut fi terminată. Detalii în jurnalul de erori.")
        If rezultatIncert Then mesaj &= vbCrLf & "Rezultat incert. Închideți editorul și reîncărcați lista înainte de a relua salvarea."
        ArataAvertisment(mesaj)
    End Sub
End Class
