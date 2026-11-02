Option Strict On
Imports System.Collections.Generic
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

' Slice 01EF-02 -- the first view of the bar, «Contari» (window with all the invoices only): while it is the shown view the tree has tick boxes (on the invoices and on the
' suppliers); the invoices of ONE supplier can be ticked at a time (a tick on another supplier takes the others off); every change of the ticks, of the classification, of the
' templates or of a value to pay asks the server to calculate again and the page shows the answer. Nothing is saved yet (01EF-04/05).
Partial Public Class PrimiteView

    Private Const ViewContare As String = "contare"

    Private Const ContareGoala As String = "Bifați în arbore facturile aceluiași furnizor."
    Private Const ContareUnFurnizor As String = "Se pot alege facturi ale unui singur furnizor odată: bifele celorlalți furnizori au fost scoase."

    Private _contare As Boolean
    Private _contareSeq As Integer
    Private _contareSupplier As String = String.Empty
    Private _contareClsf As Integer?
    Private _contareE As Integer?
    Private _contareP As Integer?
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
            GlobalErrorLog.Write("PrimiteView.HostSubunitPicked", ex)
        End Try
    End Sub

    ' The shown view changed: the tick boxes appear with «Contari» and go with it. UI boundary (called from ShowPage): logs and swallows.
    Private Sub SetContareMode(k_active As Boolean)
        Try
            If k_active = _contare Then Return
            _contare = k_active AndAlso Not _angajamentView
            If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
            If _contare Then
                Dim k_ignored As Task = RefreshContareAsync()   ' never throws: it reports into the page
            Else
                _contareSeq += 1
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.SetContareMode", ex)
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

    Private Function SubunitInForce() As Integer?
        Return If(_subunitHost Is Nothing, CType(Nothing, Integer?), _subunitHost.CurrentSubunitId)
    End Function

    Private Sub ResetContare()
        _contareSeq += 1
        _contareUltim = Nothing
        _tickedIds.Clear()
        _contareSupplier = String.Empty
        ResetContareChoices()
        pgContare.ShowEmpty(ContareGoala)
    End Sub

    Private Sub ResetContareChoices()
        _contareClsf = Nothing
        _contareE = Nothing
        _contareP = Nothing
        _contarePlatit.Clear()
    End Sub

    Private Function TickedNow() As List(Of Integer)
        Return _nodes.Where(Function(k_pair) k_pair.Value.CheckState = AdvancedTreeControl.TreeCheckState.Checked).Select(Function(k_pair) k_pair.Key).ToList()
    End Function

    Private Function SupplierOfInvoice(k_id As Integer) As String
        Dim k_p As EFacturaPrimita = TryCast(_nodes(k_id).Tag, EFacturaPrimita)
        Return If(k_p Is Nothing, String.Empty, SupplierKey(k_p))
    End Function

    ' Only one supplier can be ticked: when a tick brought a second one, the NEWLY ticked supplier stays and the ticks of the others are taken off.
    ' Returns True when ticks were taken off.
    Private Function KeepOneSupplier(k_ticked As List(Of Integer)) As Boolean
        Dim k_keys As List(Of String) = k_ticked.Select(AddressOf SupplierOfInvoice).Distinct(StringComparer.Ordinal).ToList()
        If k_keys.Count <= 1 Then Return False
        Dim k_keep As String = k_keys.FirstOrDefault(Function(k_x) Not String.Equals(k_x, _contareSupplier, StringComparison.Ordinal))
        If k_keep Is Nothing Then k_keep = k_keys(0)
        _treeLoading = True
        Try
            For Each k_id As Integer In k_ticked
                If Not String.Equals(SupplierOfInvoice(k_id), k_keep, StringComparison.Ordinal) Then
                    tree.SetItemCheckState(_nodes(k_id), AdvancedTreeControl.TreeCheckState.Unchecked)
                End If
            Next
        Finally
            _treeLoading = False
        End Try
        Return True
    End Function

    ' UI boundary (async Sub): a tick changed (on an invoice, or on a supplier that ticks all its invoices).
    Private Async Sub Tree_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles tree.NodeChecked
        Try
            If _treeLoading OrElse Not _contare Then Return
            Dim k_ticked As List(Of Integer) = TickedNow()
            Dim k_removed As Boolean = KeepOneSupplier(k_ticked)
            If k_removed Then k_ticked = TickedNow()
            _tickedIds.Clear()
            For Each k_id As Integer In k_ticked
                _tickedIds.Add(k_id)
            Next
            Await RefreshContareAsync(If(k_removed, ContareUnFurnizor, String.Empty)).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.Tree_NodeChecked", ex)
        End Try
    End Sub

    ''' <summary>Asks the server for the calculation of the ticked invoices and puts it on the page; a failure becomes a line on the page and in the notice. Never throws.</summary>
    Private Async Function RefreshContareAsync(Optional k_note As String = "") As Task
        Try
            Dim k_ids As List(Of Integer) = _tickedIds.Where(Function(k_id) _nodes.ContainsKey(k_id)).OrderBy(Function(k_id) k_id).ToList()
            If k_ids.Count = 0 Then
                _contareSeq += 1
                _contareSupplier = String.Empty
                ResetContareChoices()
                pgContare.ShowEmpty(ContareGoala)
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
            pgContare.ShowResult(k_answer)
            If k_note.Length > 0 Then
                ntfMesaj.Show(k_note, NoticeKind.Warning)
                ntfMesaj.Visible = True
            End If
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.RefreshContareAsync", ex)
            pgContare.ShowEmpty(ex.Message)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.RefreshContareAsync", ex)
            pgContare.ShowEmpty("Calculul contării nu a putut fi făcut. Detalii în jurnalul de erori.")
            ShowProblem("Calculul contării nu a putut fi făcut. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Async Function AskContareAsync(k_ids As List(Of Integer)) As Task(Of EFacturaContariRaspuns)
        Dim k_request As New EFacturaContariCerere() With {.IdClsf = _contareClsf, .IdSablonE = _contareE, .IdSablonP = _contareP, .An = YearFilter(), .IdSubunitate = SubunitInForce()}
        k_request.Ids.AddRange(k_ids)
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In _contarePlatit
            If k_ids.Contains(k_pair.Key) Then k_request.Platit(k_pair.Key) = k_pair.Value
        Next
        Return Await _gate.RunAsync(Function() _api.CalculateContariAsync(k_request, _cts.Token)).ConfigureAwait(True)
    End Function

    ' ── The page's events ───────────────────────────────────────────────────────

    Private Async Sub PgContare_ClasificatieAleasa(k_idClsf As Integer) Handles pgContare.ClasificatieAleasa
        Try
            _contareClsf = k_idClsf
            _contareE = Nothing
            _contareP = Nothing
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgContare_ClasificatieAleasa", ex)
        End Try
    End Sub

    Private Async Sub PgContare_SablonAles(k_fel As String, k_idSablon As Integer) Handles pgContare.SablonAles
        Try
            If String.Equals(k_fel, "E", StringComparison.Ordinal) Then _contareE = k_idSablon Else _contareP = k_idSablon
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgContare_SablonAles", ex)
        End Try
    End Sub

    Private Async Sub PgContare_PlatitSchimbat(k_idPrimita As Integer, k_valoare As Decimal) Handles pgContare.PlatitSchimbat
        Try
            _contarePlatit(k_idPrimita) = k_valoare
            Await RefreshContareAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgContare_PlatitSchimbat", ex)
        End Try
    End Sub

    Private Sub PgContare_RenuntaCerut() Handles pgContare.RenuntaCerut
        Try
            ClearTicks()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgContare_RenuntaCerut", ex)
        End Try
    End Sub

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
    ' (the server checks them again). UI boundary (async Sub): logs and swallows; a failure becomes a line in the notice.
    Private Async Sub PgContare_SalveazaCerut() Handles pgContare.SalveazaCerut
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
            Dim k_noteValid As Boolean
            Dim k_note As Integer? = pgContare.NotaAleasa(k_noteValid)
            If Not k_noteValid Then
                KBotMessage.Show(FindForm(), "Numărul notei contabile trebuie să fie un număr întreg pozitiv.", "Contări", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim k_request As New EFacturaContariCerere() With {
                .IdClsf = If(k_answer.Clasificatie Is Nothing, CType(Nothing, Integer?), k_answer.Clasificatie.IdClsf),
                .IdSablonE = k_answer.IdSablonE, .An = YearFilter(), .IdSubunitate = SubunitInForce(),
                .NumarNC = k_note}
            k_request.Ids.AddRange(_tickedIds.Where(Function(k_id) _nodes.ContainsKey(k_id)).OrderBy(Function(k_id) k_id))
            k_request.Confirmari.AddRange(k_confirm.Select(Function(k_a) k_a.Cod))
            Dim k_saved As EFacturaContariSalvat = Await _gate.RunAsync(Function() _api.SaveContariAsync(k_request, _cts.Token)).ConfigureAwait(True)
            KBotMessage.Show(FindForm(), $"Salvare efectuată: nota contabilă {k_saved.NumarNC.ToString(CultureInfo.CurrentCulture)} ({k_saved.Subunitate}), " &
                             $"{k_saved.NrOperatii.ToString(CultureInfo.CurrentCulture)} operații.", "Contări", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ClearTicks()
            Await LoadAsync(_idDdf).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PgContare_SalveazaCerut", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgContare_SalveazaCerut", ex)
            ShowProblem("Salvarea nu a putut fi dusă la capăt. Detalii în jurnalul de erori.")
        Finally
            _contareSalveaza = False
        End Try
    End Sub

End Class
