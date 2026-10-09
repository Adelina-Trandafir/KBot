Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-08 / 00EF-09 -- «E-Factura — facturi emise»: the screen of the Access form EFACTURA_ADD, redrawn with K-BOT controls.
''' On the left a TREE: the clients are the roots, their invoices the leaves; the dot on the left of an invoice says where it is
''' (grey = not sent, orange = sent and not confirmed, green = accepted, red = refused) and the sign that shows on its right when
''' the mouse is over it opens its menu (send, check the state, show the ANAF error, the two PDFs, storno, modify). On the right
''' the chosen invoice, in VIEWS chosen from a horizontal bar (the pattern of the DDF view): Generale, Cumpărător,
''' Atașamente, Conținut and, by the state of the invoice, «Factură PDF», «Factură ANAF» and «Eroare ANAF» (the three are one
''' embedded PDF viewer). Everything the window shows about what may be done with an invoice comes from the server
''' (<see cref="EFacturaFactura.PoateModifica"/>, ...), never from a rule here.
'''
''' <para>Split by what it does: this file = the invoice as a whole, buttons, state; FacturiForm.Arbore = the tree and the invoice
''' menu; FacturiForm.Trimitere = send, check, storno; FacturiForm.Pdf = the views and the PDF viewer; FacturiForm.Client = the
''' customer view; FacturiForm.Furnizor = the issuer the window keeps and the title-bar menu «Conturi Unitate» / «Date Unitate»; FacturiForm.Linii = the lines grid and the units of measure.</para>
''' </summary>
Public Class FacturiForm

    ''' <summary>What the right side is doing: showing an invoice, typing a new one, or modifying a draft.</summary>
    Private Enum EditMode
        Viewing
        Creating
        Editing
    End Enum

    ' The views of the horizontal bar (the keys of navDetaliu's items, written in the designer).
    Private Const ViewGenerale As String = "generale"
    Private Const ViewCumparator As String = "cumparator"
    Private Const ViewAtasamente As String = "atasamente"
    Private Const ViewContinut As String = "continut"
    Private Const ViewPdf As String = "pdf"
    Private Const ViewAnaf As String = "anaf"
    Private Const ViewEroare As String = "eroare"

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _authorizer As TokenAuthorizer
    Private ReadOnly _unitName As String
    Private ReadOnly _cts As New CancellationTokenSource()

    Private _mode As EditMode = EditMode.Viewing
    Private _facturi As New List(Of EFacturaFactura)()
    ' The invoice shown (Nothing = nothing chosen, or a new one being typed).
    Private _current As EFacturaFactura
    ' The id of the invoice the right side shows or is loading (0 = none): the tree's selection goes back to it when a change is refused.
    Private _shownId As Integer
    Private _dirty As Boolean
    Private _loading As Boolean
    Private _busy As Boolean
    Private _treeLoading As Boolean
    Private _showSeq As Integer
    Private _closeConfirmed As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        BindPages()
        _gate = ReauthGate.Direct
        _unitName = String.Empty
    End Sub

    ''' <param name="k_api">The invoice routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net (a call that answers 401 is retried after a new login).</param>
    ''' <param name="k_authorizer">The token step, for the «Token ANAF» button.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the status line.</param>
    ''' <param name="k_viewerFactory">Makes the embedded PDF viewer (it lives in the shell's project); Nothing = the PDF views say there is none.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_authorizer As TokenAuthorizer, k_unitName As String,
                   Optional k_viewerFactory As Func(Of IFacturaPdfViewer) = Nothing)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        BindPages()
        _api = k_api
        _gate = k_gate
        _authorizer = k_authorizer
        _unitName = If(k_unitName, String.Empty)
        pgPdf.ViewerFactory = k_viewerFactory
        pgAnaf.ViewerFactory = k_viewerFactory
        pgEroare.ViewerFactory = k_viewerFactory
        ' While a document is still opening in Adobe no other row may be clicked (the same gate the DDF tree uses).
        AdobeOpenGate.LockWhileOpening(tree)
    End Sub

    Private Sub FacturiForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            InitCounties()
            InitSectors()
            SelectView(ViewGenerale)
            ShowInvoice(Nothing)
            ApplyMode()
            ApplyTreeLook()
            LoadAll()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("FacturiForm.FacturiForm_Load", ex)
        End Try
    End Sub

    Private Sub FacturiForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            _cts.Cancel()
            _cts.Dispose()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("FacturiForm.FacturiForm_FormClosed", ex)
        End Try
    End Sub

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadAll()
        Dim k_needUnit As Boolean = False
        Try
            SetBusy(True, "Se încarcă datele E-Factura…")
            ntfMesaj.Clear()
            Dim k_furnizor As EFacturaFurnizor = Await _gate.RunAsync(
                Function() _api.GetFurnizorAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            SetFurnizor(k_furnizor)
            Dim k_umNote As String = Await LoadUmAsync().ConfigureAwait(True)
            If IsDisposed Then Return
            Await LoadClientsAsync().ConfigureAwait(True)
            If IsDisposed Then Return
            Await LoadInvoicesAsync(Nothing).ConfigureAwait(True)
            If IsDisposed Then Return
            If k_furnizor Is Nothing Then
                k_needUnit = True
                ntfMesaj.Show("Datele unității care emite facturile nu sunt completate. Completați-le în fereastra «Date Unitate» " &
                              "și apăsați «Salvează»; fără ele nu se pot face facturi.", NoticeKind.Warning)
            ElseIf k_umNote IsNot Nothing Then
                ntfMesaj.Show(k_umNote, NoticeKind.Warning)
            End If
            SetStatus(StatusLine())
        Catch ex As OperationCanceledException
            ' The window was closed while the answer was on its way: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.LoadAll", ex)
            If Not IsDisposed Then
                ntfMesaj.Show(ex.Message, NoticeKind.Error)
                SetStatus(String.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.LoadAll", ex)
            If Not IsDisposed Then
                ntfMesaj.Show("Datele E-Factura nu au putut fi încărcate. Detalii în jurnalul de erori.", NoticeKind.Error)
                SetStatus(String.Empty)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
        ' The unit has no issuer data yet: its window opens by itself (nothing else can be done before it is filled in).
        If k_needUnit AndAlso Not IsDisposed Then OpenDateUnitate()
    End Sub

    Private Function StatusLine() As String
        Dim k_unit As String = If(_unitName.Length = 0, String.Empty, _unitName & " — ")
        Return $"{k_unit}{_facturi.Count} facturi."
    End Function

    ''' <summary>Reads the invoice headers again and rebuilds the tree; <paramref name="k_selectId"/> is chosen when it is there.</summary>
    Private Async Function LoadInvoicesAsync(k_selectId As Integer?) As Task
        Dim k_list As List(Of EFacturaFactura) = Await _gate.RunAsync(
            Function() _api.GetFacturiAsync(Nothing, Nothing, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return
        _facturi = k_list
        BuildTree(k_selectId)
        FillContPlata()
    End Function

    ''' <summary>
    ''' After a change on the server: the tree is read again and the invoice is shown again with what the server now says about it
    ''' (its state, what may be done with it). Called when nothing else is busy.
    ''' </summary>
    Private Async Function ReloadAndShowAsync(k_idFactura As Integer) As Task
        Await LoadInvoicesAsync(k_idFactura).ConfigureAwait(True)
        If IsDisposed Then Return
        _shownId = k_idFactura
        Await ShowInvoiceAsync(k_idFactura).ConfigureAwait(True)
    End Function

    Private Shared Function StateText(k_f As EFacturaFactura) As String
        Dim k_text As String
        Select Case k_f.Stare
            Case EFacturaStare.Incarcata
                k_text = "Trimisă (neconfirmată)"
            Case EFacturaStare.Refuzata
                k_text = "Refuzată de ANAF"
            Case EFacturaStare.Acceptata
                k_text = "Acceptată"
            Case Else
                k_text = "Ciornă"
        End Select
        If k_f.EsteStorno Then k_text &= " · stornare"
        If k_f.Corectata Then k_text &= " · corectată"
        Return k_text
    End Function

    ' UI boundary: asks about unsaved changes, then shows the invoice.
    Private Async Sub ChangeSelection(k_idFactura As Integer)
        Try
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then
                RestoreTreeSelection()
                Return
            End If
            _shownId = k_idFactura
            Await ShowInvoiceAsync(k_idFactura).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.ChangeSelection", ex)
        End Try
    End Sub

    ''' <summary>Reads one invoice with its customer and lines and shows it (a newer request wins over an older answer).</summary>
    Private Async Function ShowInvoiceAsync(k_idFactura As Integer) As Task
        Dim k_seq As Integer = Interlocked.Increment(_showSeq)
        Try
            SetBusy(True, "Se citește factura…")
            ntfMesaj.Clear()
            Dim k_f As EFacturaFactura = Await _gate.RunAsync(
                Function() _api.GetFacturaAsync(k_idFactura, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _showSeq Then Return
            _current = k_f
            _mode = EditMode.Viewing
            _dirty = False
            ShowInvoice(k_f)
            SetStatus(String.Empty)
        Catch ex As OperationCanceledException
            ' The window was closed while the answer was on its way: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.ShowInvoiceAsync", ex)
            If Not IsDisposed Then ntfMesaj.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.ShowInvoiceAsync", ex)
            If Not IsDisposed Then ntfMesaj.Show("Factura nu a putut fi citită. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed AndAlso k_seq = _showSeq Then SetBusy(False, Nothing)
        End Try
    End Function

    ' ── The invoice on the right ────────────────────────────────────────────────

    ''' <summary>Fills every view from an invoice (Nothing = empty, nothing chosen) and sets what may be edited.</summary>
    Private Sub ShowInvoice(k_f As EFacturaFactura)
        Dim k_was As Boolean = _loading
        _loading = True
        Try
            ' No earlier limit may clamp the date of the invoice about to be shown (a date from before a rule stays as it is).
            dtpData.ResetMinDate()
            If k_f Is Nothing Then
                'lblAntet.Text = "Nicio factură aleasă"
                lblNumar.Text = String.Empty
                lblTip.Text = String.Empty
                lblStareFactura.Text = String.Empty
                lblInfoFactura.Text = "Alegeți o factură din arbore sau apăsați «Adăugare»."
                dtpData.Value = Date.Today
                txtComentarii.Text = String.Empty
                txtRef.Text = String.Empty
                chkAtasament.Checked = False
                cmbContPlata.Text = String.Empty
                ShowClient(Nothing)
                FillLines(New List(Of EFacturaLinie)())
            Else
                'lblAntet.Text = $"Factura {k_f.Eticheta} — {StateText(k_f)}"
                lblNumar.Text = k_f.Eticheta
                lblTip.Text = TypeText(k_f.TipFactura)
                lblStareFactura.Text = StateText(k_f)
                lblInfoFactura.Text = InfoText(k_f)
                dtpData.Value = If(k_f.DataFactura.Year >= 2000, k_f.DataFactura, Date.Today)
                txtComentarii.Text = k_f.Comentarii
                txtRef.Text = k_f.BT_13
                chkAtasament.Checked = k_f.AtasamentOriginal
                cmbContPlata.Text = k_f.ContPlata
                ShowClient(k_f.Client)
                FillLines(k_f.Linii)
            End If
        Finally
            _loading = k_was
        End Try
        ApplyStateColor()
        ApplyMode()
        RefreshPdfView()
    End Sub

    Private Shared Function TypeText(k_type As String) As String
        Select Case k_type
            Case "380"
                Return "380 — Factură"
            Case "384"
                Return "384 — Factură corectată"
            Case Else
                Return k_type
        End Select
    End Function

    Private Shared Function InfoText(k_f As EFacturaFactura) As String
        Select Case k_f.Stare
            Case EFacturaStare.Acceptata
                Return "ANAF a acceptat factura." & If(k_f.EsteStorno, StornoSentence(k_f), String.Empty)
            Case EFacturaStare.Incarcata
                Return "Factura a fost trimisă la ANAF, dar rezultatul nu este încă cunoscut. Din meniul ei (semnul din dreapta, în arbore) " &
                       "o puteți valida la ANAF. Până atunci nu se modifică."
            Case EFacturaStare.Refuzata
                Return "ANAF a refuzat factura și ea nu se mai modifică." &
                       If(k_f.EroareAnaf.Length = 0, String.Empty, vbLf & "Motivul: " & k_f.EroareAnaf)
            Case Else
                If k_f.EsteStorno Then Return "Factură de stornare: nu se modifică, doar se trimite." & StornoSentence(k_f)
                Return "Ciornă: factura nu a fost trimisă la ANAF."
        End Select
    End Function

    Private Shared Function StornoSentence(k_f As EFacturaFactura) As String
        Dim k_other As String = If(k_f.SerieFacturaA.Length = 0, k_f.NumarFacturaA, k_f.SerieFacturaA & "_" & k_f.NumarFacturaA)
        Return If(k_f.EsteStorno AndAlso k_other.Length > 0, vbLf & "Stornează factura " & k_other & ".", String.Empty)
    End Function

    Private Sub ApplyStateColor()
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Dim k_color As Color = k_palette.TextColor
        If _current IsNot Nothing AndAlso _mode = EditMode.Viewing Then
            Select Case _current.Stare
                Case EFacturaStare.Acceptata
                    k_color = k_palette.SuccessColor
                Case EFacturaStare.Refuzata
                    k_color = k_palette.ErrorColor
                Case EFacturaStare.Incarcata
                    k_color = k_palette.WarningColor
            End Select
        End If
        lblStareFactura.ForeColor = k_color
    End Sub

    ' ── What may be edited, and which buttons work ──────────────────────────────

    ''' <summary>
    ''' The date of the invoice (slice 00EF-09, operator 07.10.2026): a new invoice takes it freely (never before the newest invoice); a
    ''' draft being modified changes it only while it is the LAST of its series, and never to before the invoice that comes just before it.
    ''' The server decides (<see cref="EFacturaFactura.PoateModificaData"/>) and checks again on save.
    ''' </summary>
    Private Function DateEditable() As Boolean
        If _mode = EditMode.Creating Then Return True
        Return _mode = EditMode.Editing AndAlso _current IsNot Nothing AndAlso _current.PoateModificaData
    End Function

    Private Sub ApplyMode()
        Dim k_edit As Boolean = _mode <> EditMode.Viewing
        For Each k_c As Control In New Control() {txtComentarii, txtRef, chkAtasament, cmbContPlata, btnLinieNoua}
            k_c.Enabled = k_edit
        Next
        dtpData.Enabled = DateEditable()
        gridLinii.ReadOnlyGrid = Not k_edit
        ApplyClientMode(k_edit)
        btnAdauga.Enabled = Not _busy AndAlso _furnizor IsNot Nothing
        btnModifica.Enabled = Not _busy AndAlso _mode = EditMode.Viewing AndAlso _current IsNot Nothing AndAlso _current.PoateModifica
        btnSalveaza.Enabled = Not _busy AndAlso k_edit
        btnRenunta.Enabled = Not _busy AndAlso k_edit
        'btnSterge.Enabled = Not _busy AndAlso _mode = EditMode.Viewing AndAlso _current IsNot Nothing AndAlso _current.PoateSterge
        'btnToken.Enabled = Not _busy AndAlso _authorizer IsNot Nothing
        If k_edit Then
            'lblAntet.Text = If(_mode = EditMode.Creating, "Factură nouă", $"Modificare — factura {_current?.Eticheta}")
        End If
        ApplyViewItems()
        ApplyStateColor()
    End Sub

    ''' <summary>
    ''' The views that belong to the state of the invoice: «Factură PDF» and «Factură ANAF» to an accepted one, «Eroare ANAF» to a refused
    ''' one; none while an invoice is typed. A view that goes away while it is on screen hands over to «Generale».
    ''' </summary>
    Private Sub ApplyViewItems()
        Dim k_saved As Boolean = _current IsNot Nothing AndAlso _mode = EditMode.Viewing
        Dim k_accepted As Boolean = k_saved AndAlso _current.Stare = EFacturaStare.Acceptata
        Dim k_refused As Boolean = k_saved AndAlso _current.Stare = EFacturaStare.Refuzata
        navDetaliu.SetItemVisible(ViewPdf, k_accepted)
        navDetaliu.SetItemVisible(ViewAnaf, k_accepted)
        navDetaliu.SetItemVisible(ViewEroare, k_refused)
        Dim k_key As String = navDetaliu.SelectedKey
        If (String.Equals(k_key, ViewPdf, StringComparison.Ordinal) AndAlso Not k_accepted) OrElse
           (String.Equals(k_key, ViewAnaf, StringComparison.Ordinal) AndAlso Not k_accepted) OrElse
           (String.Equals(k_key, ViewEroare, StringComparison.Ordinal) AndAlso Not k_refused) Then
            SelectView(ViewGenerale)
        End If
    End Sub

    Private Sub SetDirty(k_value As Boolean)
        _dirty = k_value
    End Sub

    Private Sub SetBusy(k_on As Boolean, k_text As String)
        _busy = k_on
        barBusy.Running = k_on
        UseWaitCursor = k_on
        ApplyMode()
        If k_text IsNot Nothing Then SetStatus(k_text)
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    ''' <summary>Marks the invoice as changed (only while it is open for editing and nothing is being filled in).</summary>
    Private Sub FieldChanged()
        If _loading OrElse _mode = EditMode.Viewing Then Return
        SetDirty(True)
    End Sub

    Private Sub Field_Changed(sender As Object, e As EventArgs) _
        Handles dtpData.ValueChanged, txtComentarii.TextChanged, txtRef.TextChanged, chkAtasament.CheckedChanged,
                cmbContPlata.TextChanged, cmbContPlata.SelectedIndexChanged
        Try
            FieldChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.Field_Changed", ex)
        End Try
    End Sub

    ' ── Buttons ─────────────────────────────────────────────────────────────────

    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _busy Then Return
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            Await BeginNewAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnAdauga_Click", ex)
        End Try
    End Sub

    ''' <summary>Opens an empty invoice: the date is today, the account is the last one used, the number is shown as provisional.</summary>
    Private Async Function BeginNewAsync() As Task
        Try
            If _furnizor Is Nothing Then
                KBotMessage.Show(Me, "Completați mai întâi datele unității emitente, în fereastra «Date Unitate».",
                                 "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Information)
                OpenDateUnitate()
                Return
            End If
            SetBusy(True, "Se pregătește factura nouă…")
            Dim k_next As EFacturaNumarUrmator = Await _gate.RunAsync(
                Function() _api.GetNextNumberAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            Interlocked.Increment(_showSeq)
            _current = Nothing
            _mode = EditMode.Creating
            ClearTreeSelection()
            _shownId = 0
            _loading = True
            Try
                ShowInvoice(Nothing)
                lblNumar.Text = If(k_next.Serie.Length = 0, k_next.Numar.ToString(CultureInfo.InvariantCulture),
                                   k_next.Serie & "_" & k_next.Numar.ToString(CultureInfo.InvariantCulture)) &
                                "  (provizoriu; numărul se dă la salvare)"
                lblTip.Text = TypeText("380")
                lblStareFactura.Text = "Factură nouă"
                lblInfoFactura.Text = "Alegeți clientul în vederea «Cumpărător» și completați conținutul în vederea «Conținut»."
                cmbContPlata.Text = DefaultContPlata()
                ' The date: today, but never before the newest invoice of the series.
                If k_next.DataMinima.HasValue Then
                    Dim k_min As Date = k_next.DataMinima.Value.Date
                    If dtpData.Value.Date < k_min Then dtpData.Value = k_min
                    dtpData.MinDate = k_min
                End If
            Finally
                _loading = False
            End Try
            SetDirty(False)
            ApplyMode()
            SelectView(ViewGenerale)
            cmbContPlata.Focus()
            SetStatus("Factură nouă: alegeți clientul, completați liniile și apăsați «Salvare».")
        Catch ex As OperationCanceledException
            ' The window was closed: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.BeginNewAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Factură nouă", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BeginNewAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Factura nouă nu a putut fi pregătită. Detalii în jurnalul de erori.", "Factură nouă",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    Private Sub BtnModifica_Click(sender As Object, e As EventArgs) Handles btnModifica.Click
        Try
            BeginEdit()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnModifica_Click", ex)
        End Try
    End Sub

    ''' <summary>Opens the shown draft for modification (the button and the invoice menu both come here).</summary>
    Private Sub BeginEdit()
        If _busy OrElse _mode <> EditMode.Viewing OrElse _current Is Nothing OrElse Not _current.PoateModifica Then Return
        _mode = EditMode.Editing
        SetDirty(False)
        ' A date that may move may not go before the invoice just before this one.
        If _current.PoateModificaData AndAlso _current.DataMinima.HasValue AndAlso dtpData.Value.Date >= _current.DataMinima.Value.Date Then
            Dim k_was As Boolean = _loading
            _loading = True
            Try
                dtpData.MinDate = _current.DataMinima.Value.Date
            Finally
                _loading = k_was
            End Try
        End If
        ApplyMode()
        SelectView(ViewGenerale)
        SetStatus(If(_current.PoateModificaData,
                     "Factura este deblocată pentru modificare; apăsați «Salvare» sau «Renunță».",
                     "Factura este deblocată pentru modificare. Data nu se mai schimbă: factura nu este ultima din serie. Apăsați «Salvare» sau «Renunță»."))
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            If _busy OrElse _mode = EditMode.Viewing Then Return
            DiscardEdit()
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnRenunta_Click", ex)
        End Try
    End Sub

    ''' <summary>Throws the typing away and shows the invoice as it was last read (or the empty state after a new one).</summary>
    Private Sub DiscardEdit()
        _mode = EditMode.Viewing
        SetDirty(False)
        ClientCleanState()
        ShowInvoice(_current)
    End Sub

    Private Async Sub BtnSterge_Click(sender As Object, e As EventArgs)
        Try
            If _busy OrElse _mode <> EditMode.Viewing OrElse _current Is Nothing OrElse Not _current.PoateSterge Then Return
            If KBotMessage.Show(Me, $"Ștergeți factura {_current.Eticheta} din {_current.DataFactura:dd.MM.yyyy} ({_current.ClientDenumire})?",
                                "Ștergere factură", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            Dim k_id = _current.IdFactura
            Dim k_label = _current.Eticheta
            SetBusy(True, "Se șterge factura…")
            Try
                Await _gate.RunAsync(Function() _api.DeleteFacturaAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                _current = Nothing
                _shownId = 0
                Await LoadInvoicesAsync(Nothing).ConfigureAwait(True)
                If IsDisposed Then Return
                ShowInvoice(Nothing)
                SetStatus($"Factura {k_label} a fost ștearsă.")
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
        Catch ex As OperationCanceledException
            ' The window was closed: nothing to show.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.BtnSterge_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Ștergere factură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnSterge_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Factura nu a putut fi ștearsă. Detalii în jurnalul de erori.", "Ștergere factură",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _busy Then Return
            Await SaveInvoiceAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>Writes the invoice and its lines. True = saved (or nothing to save). Every failure is shown here.</summary>
    Private Async Function SaveInvoiceAsync() As Task(Of Boolean)
        If _mode = EditMode.Viewing OrElse _api Is Nothing Then Return True
        Try
            If Not gridLinii.CommitPendingEdit() Then Return False
            Dim k_request As EFacturaFactura = ReadInvoice()
            If k_request Is Nothing Then Return False
            SetBusy(True, "Se salvează factura…")
            Dim k_saved As EFacturaFactura
            Try
                k_saved = Await _gate.RunAsync(Function() _api.SaveFacturaAsync(k_request, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return True
                _current = k_saved
                _shownId = k_saved.IdFactura
                _mode = EditMode.Viewing
                SetDirty(False)
                ClientCleanState()
                Await LoadInvoicesAsync(k_saved.IdFactura).ConfigureAwait(True)
                If IsDisposed Then Return True
                ShowInvoice(k_saved)
                SetStatus($"Factura {k_saved.Eticheta} a fost salvată.")
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            Return True
        Catch ex As OperationCanceledException
            Return False
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.SaveInvoiceAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Salvare factură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.SaveInvoiceAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Factura nu a putut fi salvată. Detalii în jurnalul de erori.", "Salvare factură",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The invoice as the views hold it, or Nothing after telling the operator what is missing (the view and the field
    ''' are opened). The server checks everything again; this only spares a round trip for the obvious.
    ''' </summary>
    Private Function ReadInvoice() As EFacturaFactura
        If _clientId <= 0 Then
            Problem("Alegeți clientul facturii (vederea «Cumpărător»).", ViewCumparator, cmbClient)
            Return Nothing
        End If
        If _clientDirty Then
            Problem("Datele clientului au fost modificate și nu sunt salvate. Apăsați «Salvează clientul» înainte de a salva factura.",
                    ViewCumparator, btnClientSalveaza)
            Return Nothing
        End If
        If Not dtpData.HasValue Then
            Problem("Data facturii este obligatorie.", ViewGenerale, dtpData)
            Return Nothing
        End If
        Dim k_account As String = cmbContPlata.Text.Trim()
        If k_account.Length = 0 Then
            Problem("Contul emitent (IBAN) este obligatoriu (vederea «Generale»).", ViewGenerale, cmbContPlata)
            Return Nothing
        End If
        Dim k_lines As List(Of EFacturaLinie) = ReadLines()
        If k_lines Is Nothing Then Return Nothing
        Return New EFacturaFactura() With {
            .IdFactura = If(_mode = EditMode.Editing AndAlso _current IsNot Nothing, _current.IdFactura, 0),
            .IdClient = _clientId,
            .DataFactura = dtpData.Value.Date,
            .Comentarii = txtComentarii.Text.Trim(),
            .BT_13 = txtRef.Text.Trim(),
            .ContPlata = k_account,
            .AtasamentOriginal = chkAtasament.Checked,
            .Linii = k_lines}
    End Function

    ''' <summary>Tells the operator, opens the view and puts the cursor in the field.</summary>
    Private Sub Problem(k_text As String, k_view As String, k_field As Control)
        SelectView(k_view)
        KBotMessage.Show(Me, k_text, "Salvare factură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        k_field.Focus()
    End Sub

    Private Sub BtnToken_Click(sender As Object, e As EventArgs)
        Try
            If _busy OrElse _authorizer Is Nothing Then Return
            Using k_form As New TokenForm(_authorizer, _unitName)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnToken_Click", ex)
        End Try
    End Sub

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.BtnIesire_Click", ex)
        End Try
    End Sub

    ' ── Unsaved changes ─────────────────────────────────────────────────────────

    Private Function InvoiceUnsaved() As Boolean
        Return _mode <> EditMode.Viewing AndAlso (_dirty OrElse _clientDirty)
    End Function

    ''' <summary>
    ''' Unsaved invoice: Yes saves (False when the save fails), No drops the changes, Cancel stays. True = the caller may go on.
    ''' </summary>
    Private Async Function ConfirmLeaveAsync() As Task(Of Boolean)
        If _busy Then Return False
        If Not InvoiceUnsaved() Then
            If _mode <> EditMode.Viewing Then DiscardEdit()
            Return True
        End If
        Dim k_answer As DialogResult = KBotMessage.Show(Me, "Factura are modificări nesalvate." & vbLf & "Le salvați?",
                                                        "E-Factura", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If k_answer = DialogResult.Cancel Then Return False
        If k_answer = DialogResult.No Then
            DiscardEdit()
            Return True
        End If
        Return Await SaveInvoiceAsync().ConfigureAwait(True)
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If Not _closeConfirmed AndAlso e.CloseReason = CloseReason.UserClosing AndAlso InvoiceUnsaved() Then
                e.Cancel = True
                ConfirmClose()
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.OnFormClosing", ex)
        End Try
    End Sub

    ' UI boundary: the questions and the saves cannot be awaited inside FormClosing; the window closes after them.
    Private Async Sub ConfirmClose()
        Try
            If _busy Then Return
            If InvoiceUnsaved() AndAlso Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            If IsDisposed Then Return
            _closeConfirmed = True
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.ConfirmClose", ex)
        End Try
    End Sub

    ' ── Theme ───────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            Dim k_palette As ThemePalette = k_scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(2)).
            BackColor = k_palette.BorderColor
            ' The pages of the views are controls of their own and theme themselves (Vanzare*Page.ApplyTheme).
            For Each k_c As Control In New Control() {tlyMain, pnlCard, tlyBody, pnlDetaliu, pnlPages, tlySubsol}
                k_c.BackColor = k_palette.SurfaceAltColor
            Next
            'lblAntet.ForeColor = k_palette.TextColor
            lblStare.ForeColor = k_palette.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            For Each k_b As Button In New Button() {btnAdauga, btnModifica, btnRenunta, btnIesire}
                ButtonStyles.ApplyTrans(k_b, k_scheme)
            Next
            ApplyStateColor()
            ApplyTreeLook()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class
