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

' Slice 01EF-02 / 01EF-07 -- the first view of the bar, «Contari» (window with all the invoices only). It starts from the menu of an invoice, «Adaugă operații contabile»: that
' invoice is ticked, the page is filled with it and ONLY the other invoices of the same supplier get a tick box (the suppliers have none). At least one invoice stays ticked
' («Renunță» takes all off). Every change of the ticks, of the classification, of the templates or of a value to pay asks the server to calculate again and the page shows the answer.
Partial Public Class FacturiCumparareView

    Private Const ContareUnaMacar As String = "Trebuie să rămână bifată cel puțin o factură. Pentru a renunța la tot, folosiți «Renunță»."

    Private _contare As Boolean
    Private _contareSeq As Integer
    Private _contareSupplier As String = String.Empty
    Private _contareClsf As Integer?
    Private _contareE As Integer?
    Private _contareP As Integer?
    Private _contarePartner As Integer?
    Private _contarePartnerAsked As Boolean
    Private ReadOnly _contarePlatit As New Dictionary(Of Integer, Decimal)()
    Private ReadOnly _tickedIds As New HashSet(Of Integer)()
    Private _contareUltim As EFacturaContariRaspuns
    Private _subunitHost As ISubunitHost
    Private _contareSalveaza As Boolean

    ''' <summary>Slice 01EF-02: the main window, which holds the subunit in force (the selector of the title bar is the application's). «Contari» books in that subunit and recalculates when it changes.</summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property SubunitHost As ISubunitHost
        Get
            Return _subunitHost
        End Get
        Set(k_value As ISubunitHost)
            If _subunitHost IsNot Nothing Then RemoveHandler _subunitHost.SubunitPicked, AddressOf HostSubunitPicked
            _subunitHost = k_value
            If _subunitHost IsNot Nothing Then AddHandler _subunitHost.SubunitPicked, AddressOf HostSubunitPicked
        End Set
    End Property

    ' The operator picked a subunit in the title bar (the selector has already moved). The classifications offered are those of the source of the subunit, so the choices
    ' made so far are dropped and the page is calculated again. UI boundary (async Sub): logs and swallows.
    Private Async Sub HostSubunitPicked(k_key As String)
        Try
            If Not Visible OrElse Not _contare Then Return
            _contareClsf = Nothing
            _contareE = Nothing
            _contareP = Nothing
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.HostSubunitPicked", ex)
        End Try
    End Sub

    ' The shown view changed: the tick boxes appear with «Contari» and go with it. UI boundary (called from ShowPage): logs and swallows.
    Private Sub SetContareMode(k_active As Boolean)
        Try
            If k_active = _contare Then Return
            _contare = k_active AndAlso Not _angajamentView
            If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
            If _contare Then
                ' Nothing ticked: when the invoice shown already has operations they are shown (slice 03EF-13).
                Dim k_ignored As Task = If(_tickedIds.Count > 0, RefreshContareAsync(), ShowBookedAsync())   ' never throws: it reports into the page
            Else
                _contareSeq += 1
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.SetContareMode", ex)
        End Try
    End Sub

    ' The ticks that BuildTree gives back to the leaves after a rebuild (it runs with _treeLoading, so no event comes out of it).
    Private Sub RestoreTicks()
        If Not _contare Then Return
        For Each k_id As Integer In _tickedIds.ToList()
            Dim k_leaf As AdvancedTreeControl.TreeItem = Nothing
            If _nodes.TryGetValue(k_id, k_leaf) Then
                tree.SetItemCheckState(k_leaf, AdvancedTreeControl.TreeCheckState.Checked)
            Else
                _tickedIds.Remove(k_id)
            End If
        Next
    End Sub

    ' The tick box of an invoice is drawn only while «Contari» is the shown view and a booking is on (an invoice is ticked): then EVERY invoice has one (slice 03EF-09),
    ' but only the invoices of one supplier stay ticked: ticking another supplier's invoice unticks the others (Tree_NodeChecked).
    Private Function ContareBox(k_p As EFacturaPrimita) As Boolean
        Return _contare AndAlso _contareSupplier.Length > 0
    End Function

    ' Puts the tick boxes (back) on the leaves without rebuilding the tree: the supplier changed or went away.
    Private Sub ApplyContareBoxes()
        For Each k_pair As KeyValuePair(Of Integer, AdvancedTreeControl.TreeItem) In _nodes
            Dim k_p As EFacturaPrimita = TryCast(k_pair.Value.Tag, EFacturaPrimita)
            k_pair.Value.HasCheckBox = k_p IsNot Nothing AndAlso (ContareBox(k_p) OrElse _receptiiWide)
        Next
        tree.Invalidate()
    End Sub

    ''' <summary>
    ''' The menu «Adaugă operații contabile» of an invoice (slice 01EF-07): anything ticked before is dropped, this invoice becomes the only tick, its supplier is the one being
    ''' booked, the page «Contări» is shown and filled, and the other invoices of the supplier get their tick boxes. UI boundary: logs and swallows.
    ''' </summary>
    Private Async Function StartContareAsync(k_p As EFacturaPrimita) As Task
        Try
            ClearTicks()
            _tickedIds.Add(k_p.IdPrimita)
            _contareSupplier = SupplierKey(k_p)
            If Not _contare Then
                ' The page comes on screen: the tree is rebuilt with the boxes and the tick, and the page asks for the calculation (SetContareMode).
                detalii.navDetaliu.SelectedKey = FacturiCumparareDetaliiView.ViewContare
                ShowPage(FacturiCumparareDetaliiView.ViewContare)
                Return
            End If
            ApplyContareBoxes()
            RestoreTicks()
            tree.Invalidate()
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.StartContareAsync", ex)
        End Try
    End Function

    Private Function SubunitInForce() As Integer?
        Return If(_subunitHost Is Nothing, CType(Nothing, Integer?), _subunitHost.CurrentSubunitId)
    End Function

    Private Sub ResetContare()
        _contareSeq += 1
        _contareUltim = Nothing
        _tickedIds.Clear()
        _contareSupplier = String.Empty
        ResetContareChoices()
        detalii.pgContare.ShowEmpty()
        ApplyContareBoxes()
        NotifyContare()
    End Sub

    Private Sub ResetContareChoices()
        _contareClsf = Nothing
        _contareE = Nothing
        _contareP = Nothing
        _contarePartner = Nothing
        _contarePartnerAsked = False
        _contarePlatit.Clear()
    End Sub

    Private Function TickedNow() As List(Of Integer)
        Return _nodes.Where(Function(k_pair) k_pair.Value.CheckState = AdvancedTreeControl.TreeCheckState.Checked).Select(Function(k_pair) k_pair.Key).ToList()
    End Function

    Private Function SupplierOfInvoice(k_id As Integer) As String
        Dim k_p As EFacturaPrimita = TryCast(_nodes(k_id).Tag, EFacturaPrimita)
        Return If(k_p Is Nothing, String.Empty, SupplierKey(k_p))
    End Function

    ' UI boundary (async Sub): a tick changed (an invoice was ticked or unticked; a tick on another supplier's invoice replaces the ticks of the old one): the evidence grid gets (or loses) its row.
    Private Async Sub Tree_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles tree.NodeChecked
        Try
            If _treeLoading Then Return
            If _receptiiWide Then
                ReceptiiTickChanged()   ' slice KE21: several invoices on one reception
                Return
            End If
            If Not _contare Then Return
            Dim k_ticked As List(Of Integer) = TickedNow()
            If k_ticked.Count = 0 Then
                ' At least one invoice stays ticked: the tick that was just taken off is put back.
                _treeLoading = True
                Try
                    tree.SetItemCheckState(k_node, AdvancedTreeControl.TreeCheckState.Checked)
                Finally
                    _treeLoading = False
                End Try
                ntfMesaj.Show(ContareUnaMacar, NoticeKind.Warning)
                ntfMesaj.Visible = True
                Return
            End If
            ' Slice 03EF-09: an invoice of ANOTHER supplier was ticked: the ones ticked for the old supplier are taken off, the booking moves to the new supplier
            ' and the page is calculated again for it (the page drops what was typed for the old one).
            Dim k_ticked1 As EFacturaPrimita = TryCast(k_node.Tag, EFacturaPrimita)
            If k_ticked1 IsNot Nothing AndAlso k_node.CheckState = AdvancedTreeControl.TreeCheckState.Checked AndAlso
               Not String.Equals(SupplierKey(k_ticked1), _contareSupplier, StringComparison.Ordinal) Then
                _treeLoading = True
                Try
                    For Each k_id As Integer In k_ticked
                        Dim k_leaf As AdvancedTreeControl.TreeItem = Nothing
                        If k_id <> k_ticked1.IdPrimita AndAlso _nodes.TryGetValue(k_id, k_leaf) Then
                            tree.SetItemCheckState(k_leaf, AdvancedTreeControl.TreeCheckState.Unchecked)
                        End If
                    Next
                Finally
                    _treeLoading = False
                End Try
                k_ticked = New List(Of Integer) From {k_ticked1.IdPrimita}
                _contareSupplier = SupplierKey(k_ticked1)
                ResetContareChoices()
                tree.Invalidate()
            End If
            _tickedIds.Clear()
            For Each k_id As Integer In k_ticked
                _tickedIds.Add(k_id)
            Next
            _contarePartnerAsked = False   ' a new set of ticks: an unresolved partner is asked again
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.Tree_NodeChecked", ex)
        End Try
    End Sub

    ''' <summary>Asks the server for the calculation of the ticked invoices and puts it on the page; a failure becomes a line on the page and in the notice. Never throws.</summary>
    Private Async Function RefreshContareAsync() As Task
        Try
            Dim k_ids As List(Of Integer) = _tickedIds.Where(Function(k_id) _nodes.ContainsKey(k_id)).OrderBy(Function(k_id) k_id).ToList()
            If k_ids.Count = 0 Then
                _contareSeq += 1
                _contareSupplier = String.Empty
                ResetContareChoices()
                detalii.pgContare.ShowEmpty()
                ApplyContareBoxes()
                Return
            End If
            Dim k_supplier As String = SupplierOfInvoice(k_ids(0))
            If Not String.Equals(k_supplier, _contareSupplier, StringComparison.Ordinal) Then
                ResetContareChoices()
                _contareSupplier = k_supplier
            End If
            Dim k_mySeq As Integer = Interlocked.Increment(_contareSeq)
            Dim k_answer As EFacturaContariRaspuns = Await AskContareAsync(k_ids).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _contareSeq Then Return
            ' The partner is found by tax code only; when several have it the operator picks (once per set of ticks). Closing the box leaves the page with the blocking warning.
            If Not k_answer.PartenerGasit AndAlso k_answer.Parteneri.Count > 1 AndAlso Not _contarePartnerAsked Then
                _contarePartnerAsked = True
                Using k_dialog As New AlegePartenerForm("Contări", $"facturile furnizorului {k_answer.FurnizorDenumire}", k_answer.FurnizorCui, k_answer.Parteneri)
                    If k_dialog.ShowDialog(FindForm()) = DialogResult.OK AndAlso k_dialog.Selected IsNot Nothing Then
                        _contarePartner = k_dialog.Selected.IdPartener
                        k_answer = Await AskContareAsync(k_ids).ConfigureAwait(True)
                    End If
                End Using
                If IsDisposed OrElse k_mySeq <> _contareSeq Then Return
            End If
            ' The first answer for a supplier may suggest a classification (the only one fitting): it is taken and the page is calculated once more.
            If Not _contareClsf.HasValue AndAlso k_answer.ClasificatieSugerata.HasValue Then
                _contareClsf = k_answer.ClasificatieSugerata
                k_answer = Await AskContareAsync(k_ids).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _contareSeq Then Return
            End If
            _contareE = k_answer.IdSablonE
            _contareP = k_answer.IdSablonP
            For Each k_row As EFacturaContariRand In k_answer.Evidenta
                If _contarePlatit.ContainsKey(k_row.IdPrimita) Then _contarePlatit(k_row.IdPrimita) = k_row.Platit
            Next
            _contareUltim = k_answer
            detalii.pgContare.ShowResult(k_answer)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.RefreshContareAsync", ex)
            detalii.pgContare.ShowEmpty()
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.RefreshContareAsync", ex)
            detalii.pgContare.ShowEmpty()
            ShowProblem("Calculul contării nu a putut fi făcut. Detalii în jurnalul de erori.")
        Finally
            NotifyContare()
        End Try
    End Function

    Private Async Function AskContareAsync(k_ids As List(Of Integer)) As Task(Of EFacturaContariRaspuns)
        Dim k_request As New EFacturaContariCerere() With {.IdClsf = _contareClsf, .IdSablonE = _contareE, .IdSablonP = _contareP, .An = YearFilter(), .IdSubunitate = SubunitInForce(), .IdPartener = _contarePartner}
        k_request.Ids.AddRange(k_ids)
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In _contarePlatit
            If k_ids.Contains(k_pair.Key) Then k_request.Platit(k_pair.Key) = k_pair.Value
        Next
        Return Await _gate.RunAsync(Function() _api.CalculateContariAsync(k_request, _cts.Token)).ConfigureAwait(True)
    End Function

    ' ── The page's events ───────────────────────────────────────────────────────

    Private Async Sub PgContare_ClasificatieAleasa(k_idClsf As Integer)
        Try
            _contareClsf = k_idClsf
            _contareE = Nothing
            _contareP = Nothing
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.PgContare_ClasificatieAleasa", ex)
        End Try
    End Sub

    Private Async Sub PgContare_SablonAles(k_fel As String, k_idSablon As Integer)
        Try
            If String.Equals(k_fel, "E", StringComparison.Ordinal) Then
                _contareE = k_idSablon
            Else
                _contareP = If(k_idSablon > 0, CType(k_idSablon, Integer?), Nothing)   ' 0 = the payment template was taken off: no order of payment
            End If
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.PgContare_SablonAles", ex)
        End Try
    End Sub

    Private Async Sub PgContare_PlatitSchimbat(k_idPrimita As Integer, k_valoare As Decimal)
        Try
            _contarePlatit(k_idPrimita) = k_valoare
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.PgContare_PlatitSchimbat", ex)
        End Try
    End Sub

    ' The page told that what decides «Salvează» changed (the explanation became valid, a blocking warning appeared ...): the ribbon button follows.
    Private Sub PgContare_SalvareStareSchimbata()
        NotifyContare()
    End Sub

    ' The shown invoice as the LIST holds it: only the list carries NrOperatii (the detail read of an invoice does not, it is always 0 there).
    Private Function ShownListed() As EFacturaPrimita
        Return _items.FirstOrDefault(Function(k_p) k_p.IdPrimita = _shownId)
    End Function

    ' Slice 03EF-13: the invoice shown (clicked in the tree) already has accounting operations and no booking is in progress: the page shows them, read only (the
    ' template used is chosen in the lists, the operations are written in the evidence and payment grids). An invoice without operations leaves the page empty.
    ' Never throws: a failure becomes a line in the notice.
    Private Async Function ShowBookedAsync() As Task
        Try
            If Not _contare OrElse _tickedIds.Count > 0 Then Return
            Dim k_p As EFacturaPrimita = ShownListed()
            If k_p Is Nothing OrElse k_p.NrOperatii = 0 Then
                _contareSeq += 1
                detalii.pgContare.ShowEmpty()
                NotifyContare()
                Return
            End If
            Dim k_id As Integer = k_p.IdPrimita
            Dim k_mySeq As Integer = Interlocked.Increment(_contareSeq)
            Dim k_booked As EFacturaContariExistente = Await _gate.RunAsync(Function() _api.GetContariExistenteAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _contareSeq OrElse _tickedIds.Count > 0 OrElse k_id <> _shownId Then Return
            detalii.pgContare.ShowBooked(k_booked)
            NotifyContare()
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.ShowBookedAsync", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.ShowBookedAsync", ex)
            ShowProblem("Operațiile contabile ale facturii nu au putut fi citite. Detalii în jurnalul de erori.")
        End Try
    End Function

    ' Takes every tick off the tree and empties the page. Raises no tick event.
    Private Sub ClearTicks()
        _treeLoading = True
        Try
            For Each k_id As Integer In _tickedIds.ToList()
                Dim k_leaf As AdvancedTreeControl.TreeItem = Nothing
                If _nodes.TryGetValue(k_id, k_leaf) Then tree.SetItemCheckState(k_leaf, AdvancedTreeControl.TreeCheckState.Unchecked)
            Next
        Finally
            _treeLoading = False
        End Try
        ResetContare()
    End Sub

    ' «Salvează evidențierea» (slice 01EF-04). The warnings that need a confirmation are shown together and, when the operator agrees, their codes go with the request
    ' (the server checks them again). Started by the ribbon button «Salvează» (slice 03EF-04). UI boundary (async Sub): logs and swallows; a failure becomes a line in the notice.
    Private Async Sub SaveContare()
        Try
            If _contareSalveaza OrElse _contareUltim Is Nothing Then Return
            _contareSalveaza = True
            Dim k_answer As EFacturaContariRaspuns = _contareUltim
            Dim k_confirm As List(Of EFacturaContariAvertisment) = k_answer.Avertismente.Where(
                Function(k_a) String.Equals(k_a.Grav, "confirmare", StringComparison.Ordinal)).ToList()
            If k_confirm.Count > 0 Then
                Dim k_text As String = String.Join(Environment.NewLine & Environment.NewLine, k_confirm.Select(Function(k_a) k_a.Text)) &
                                       Environment.NewLine & Environment.NewLine & "Continuați cu salvarea?"
                If KBotMessage.Show(FindForm(), k_text, "Contări", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            End If
            If detalii.pgContare.PlataActiva Then
                If KBotMessage.Show(FindForm(), "Plata nu se înregistrează încă: se salvează doar evidențierea facturilor. Continuați?", "Contări",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            End If
            Dim k_noteValid As Boolean
            Dim k_note As Integer? = detalii.pgContare.NotaAleasa(k_noteValid)
            If Not k_noteValid Then
                KBotMessage.Show(FindForm(), "Numărul notei contabile trebuie să fie un număr întreg pozitiv.", "Contări", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim k_request As New EFacturaContariCerere() With {
                .IdClsf = If(k_answer.Clasificatie Is Nothing, CType(Nothing, Integer?), k_answer.Clasificatie.IdClsf),
                .IdSablonE = k_answer.IdSablonE, .An = YearFilter(), .IdSubunitate = SubunitInForce(),
                .NumarNC = k_note, .IdPartener = _contarePartner}
            k_request.Ids.AddRange(_tickedIds.Where(Function(k_id) _nodes.ContainsKey(k_id)).OrderBy(Function(k_id) k_id))
            k_request.Confirmari.AddRange(k_confirm.Select(Function(k_a) k_a.Cod))
            Dim k_saved As EFacturaContariSalvat = Await _gate.RunAsync(Function() _api.SaveContariAsync(k_request, _cts.Token)).ConfigureAwait(True)
            KBotMessage.Show(FindForm(), $"Salvare efectuată: nota contabilă {k_saved.NumarNC.ToString(CultureInfo.CurrentCulture)} ({k_saved.Subunitate}), " &
                             $"{k_saved.NrOperatii.ToString(CultureInfo.CurrentCulture)} operații.", "Contări", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ClearTicks()
            Await LoadAsync(_idDdf).ConfigureAwait(True)
            Await ShowBookedAsync().ConfigureAwait(True)   ' slice 03EF-13: what was just saved
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.SaveContare", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.SaveContare", ex)
            ShowProblem("Salvarea nu a putut fi dusă la capăt. Detalii în jurnalul de erori.")
        Finally
            _contareSalveaza = False
        End Try
    End Sub

    ' ── The buttons of the ribbon (slice 03EF-02..05): the view group of the window shows them while «Contari» is on screen ──
    ' KBot.EFactura cannot see the ribbon (it lives in the shell): the window asks these functions and hears ReceptiiActionChanged when an answer changes.

    Public Const RibbonContareAdauga As String = "ef_contare_adauga"
    Public Const RibbonContareSalveaza As String = "ef_contare_salveaza"
    Public Const RibbonContareRenunta As String = "ef_contare_renunta"
    Public Const RibbonContareSalveazaCa As String = "ef_contare_salveaza_ca"
    Public Const RibbonContareAscunde As String = "ef_contare_ascunde"

    Private Const SalveazaTip As String = "Înregistrează facturile bifate cu șablonul de evidențiere ales: o notă contabilă, un document și o operație pentru fiecare factură. Plata se înregistrează separat (încă nu este disponibilă)."

    ' Tells the window that the buttons of «Contari» may have changed (it asks them again). Safe before the handle exists: only an event.
    Private Sub NotifyContare()
        If IsDisposed Then Return
        RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
    End Sub

    ' Booking is in progress while at least one invoice is ticked.
    Private Function ContareEditing() As Boolean
        Return _contare AndAlso _tickedIds.Count > 0
    End Function

    ''' <summary>The view «Contari» is on screen (the group of the ribbon takes its name).</summary>
    Public ReadOnly Property ContareRibbonVisible As Boolean
        Get
            Return _contare
        End Get
    End Property

    ''' <summary>The button <paramref name="k_key"/> of the ribbon is on screen: «Adaugă» before the booking, «Salvează» and «Renunță» in its place while it lasts, the other two always.</summary>
    Public Function ContareRibbonShown(k_key As String) As Boolean
        If Not _contare Then Return False
        Select Case k_key
            Case RibbonContareAdauga
                Return Not ContareEditing()
            Case RibbonContareSalveaza, RibbonContareRenunta
                Return ContareEditing()
            Case RibbonContareSalveazaCa, RibbonContareAscunde
                Return True
            Case Else
                Throw New ArgumentException($"Buton necunoscut al vederii Contări: {k_key}", NameOf(k_key))
        End Select
    End Function

    ''' <summary>The button can be pressed (shown but dimmed otherwise).</summary>
    Public Function ContareRibbonEnabled(k_key As String) As Boolean
        Dim k_p As EFacturaPrimita = ShownInvoice()
        Dim k_listed As EFacturaPrimita = ShownListed()
        Select Case k_key
            Case RibbonContareAdauga, RibbonContareSalveazaCa
                Return k_p IsNot Nothing
            Case RibbonContareSalveaza
                Return Not _contareSalveaza AndAlso detalii.pgContare.PoateSalva
            Case RibbonContareRenunta
                Return True
            Case RibbonContareAscunde
                ' An invoice with accounting operations cannot be archived (the server refuses); one already archived can be taken out.
                Return k_p IsNot Nothing AndAlso (_archived.ContainsKey(k_p.IdPrimita) OrElse (k_listed IsNot Nothing AndAlso k_listed.NrOperatii = 0))
            Case Else
                Throw New ArgumentException($"Buton necunoscut al vederii Contări: {k_key}", NameOf(k_key))
        End Select
    End Function

    ''' <summary>The text of the button when it changes with the state («Ascunde...» becomes «Scoate din arhivă» for an archived invoice); empty = the designer's text stays.</summary>
    Public Function ContareRibbonText(k_key As String) As String
        If Not String.Equals(k_key, RibbonContareAscunde, StringComparison.Ordinal) Then Return String.Empty
        Dim k_p As EFacturaPrimita = ShownInvoice()
        Return If(k_p IsNot Nothing AndAlso _archived.ContainsKey(k_p.IdPrimita), "Scoate din arhivă", "Ascunde...")
    End Function

    ''' <summary>The tooltip of the button when it carries news: «Salvează» adds what the foot of the page used to say (the supplier, the warnings, the bookings). Empty = the designer's text stays.</summary>
    Public Function ContareRibbonTip(k_key As String) As String
        If Not String.Equals(k_key, RibbonContareSalveaza, StringComparison.Ordinal) Then Return String.Empty
        Dim k_details As String = detalii.pgContare.Detalii
        Return If(k_details.Length = 0, SalveazaTip, SalveazaTip & vbLf & vbLf & k_details)
    End Function

    ''' <summary>
    ''' A button of the ribbon was pressed. <paramref name="k_anchor"/> is the ribbon and <paramref name="k_rect"/> the button in its client coordinates (for the popup of
    ''' «Exportă...»). UI boundary: logs and swallows.
    ''' </summary>
    Public Sub RunContareRibbon(k_key As String, k_anchor As Control, k_rect As Rectangle)
        Try
            Dim k_p As EFacturaPrimita = ShownInvoice()
            Select Case k_key
                Case RibbonContareAdauga
                    If k_p Is Nothing Then Return
                    Dim k_ignored As Task = StartContareAsync(k_p)   ' never throws: it logs
                Case RibbonContareSalveaza
                    If ContareRibbonEnabled(k_key) Then SaveContare()
                Case RibbonContareRenunta
                    ClearTicks()
                    Dim k_ignoredBooked As Task = ShowBookedAsync()   ' slice 03EF-13: back to what the shown invoice has booked
                Case RibbonContareSalveazaCa
                    If k_p Is Nothing OrElse k_anchor Is Nothing OrElse k_rect.IsEmpty OrElse CustomPopup.ClosedJustNow Then Return
                    Dim k_items As New List(Of CustomPopupItem) From {
                        New CustomPopupItem(MenuZip, "Arhivă &ZIP"),
                        New CustomPopupItem(MenuXml, "Fișier &XML")}
                    Dim k_menu As New CustomPopup(k_items)
                    Dim k_invoice As EFacturaPrimita = k_p
                    AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) RunMenuAction(k_args.Item.Key, k_invoice)
                    k_menu.ShowBelow(k_anchor, k_rect)
                Case RibbonContareAscunde
                    If k_p Is Nothing Then Return
                    RunMenuAction(If(_archived.ContainsKey(k_p.IdPrimita), MenuScoateArhiva, MenuArhiveaza), k_p)
                Case Else
                    Throw New ArgumentException($"Buton necunoscut al vederii Contări: {k_key}", NameOf(k_key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.RunContareRibbon", ex)
        End Try
    End Sub

End Class
