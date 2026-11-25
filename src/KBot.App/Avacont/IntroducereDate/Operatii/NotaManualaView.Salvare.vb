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

' Slice AV08-05 - the save of the accounting editor:
' - the boxes are checked before sending, and a refusal of the server names the box / line to correct: it is marked in the error colour of the theme;
' - a warning of the server (new month, Access 217; document number already in the note, Access 347) is answered Da / Nu and the save is sent again;
' - a conflict (the operation changed meanwhile) keeps what the operator typed and offers «Reîncarcă»;
' - the answer of the server (ids, numbers, dates) is kept in Rezultat for the list to select the saved operation.
Partial Public Class NotaManualaView

    Private Const ReasonConfirmare As String = "CONFIRMARE"
    Private Const ReasonVersiune As String = "VERSIUNE"
    Private Const ConfirmareDocumentDublat As String = "DOCUMENT_DUBLAT"

    Private ReadOnly _marcate As New List(Of Control)()
    Private _gridEroare As KBotDataView
    Private _randEroare As Integer = -1

    Private Async Function SalveazaAsync() As Task
        If Not gridDebit.CommitPendingEdit() OrElse Not gridCredit.CommitPendingEdit() Then Return
        For Each k_selector As KBotComboBox In {cmbNota, cmbDocument, cmbFelDocument, cmbTipOperatie, cmbJurnal, cmbClsf, cmbCompartiment, cmbExplicatie}
            If k_selector.Enabled Then k_selector.CommitText()
        Next
        CurataMarcaje()
        Dim k_cerere As NotaManualaCerere = ConstruiesteCerere()
        If k_cerere Is Nothing Then Return
        Dim k_detalii As NotaManualaDetalii = _context.Detalii
        If chkDocument.Checked AndAlso KBotMessage.Show(FindForm(), $"Modificați documentul comun pentru {k_detalii.NrOperatiiDocument} operații?",
            Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        _saving = True
        ActualizeazaCampuri()
        Try
            Dim k_continua As Boolean = True
            Do While k_continua AndAlso _rezultat Is Nothing
                Try
                    _rezultat = Await _gate.RunAsync(Function() _api.SalveazaNotaManualaAsync(k_cerere, CancellationToken.None))
                Catch ex As NotaManualaException When ex.Refuz.Reason = ReasonConfirmare AndAlso Not String.IsNullOrEmpty(ex.Refuz.Confirmare)
                    ' A warning the operator answers (no Await in a Catch: the box is synchronous).
                    MarcheazaRefuz(ex.Refuz)
                    If KBotMessage.Show(FindForm(), ex.Message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        k_cerere.Confirmari.Add(ex.Refuz.Confirmare)
                        CurataMarcaje()
                    Else
                        k_continua = False
                    End If
                End Try
            Loop
        Catch ex As NotaManualaException
            _incerta = ex.Refuz.StatusCode >= 500
            MarcheazaRefuz(ex.Refuz)
            If ex.Refuz.Reason = ReasonVersiune Then
                _conflict = True
                _reincarcareDisponibila = True
                GlobalErrorLog.Write("NotaManualaView.SalveazaAsync", ex)
                ArataAvertisment(ex.Message & " Datele introduse au rămas în fereastră. «Reîncarcă» aduce operația salvată.")
            Else
                ArataEroare(ex, _incerta)
            End If
        Catch ex As ApiException
            _incerta = Not ex.StatusCode.HasValue OrElse ex.StatusCode.Value >= 500
            ArataEroare(ex, _incerta)
        Catch ex As Exception
            _incerta = True
            ArataEroare(ex, _incerta)
        Finally
            _saving = False
        End Try
        If Salvat Then
            RaiseEvent SalvareFinalizata(Me, EventArgs.Empty)
        Else
            ActualizeazaCampuri()
        End If
    End Function

    ' The request from the window; Nothing when a validation message is shown.
    Private Function ConstruiesteCerere() As NotaManualaCerere
        If cmbClsf.Enabled AndAlso cmbClsf.SelectedItem Is Nothing Then Return Lipsa(cmbClsf, "Alegeți o clasificație sau «Fără clasificație» din listă.")
        If cmbJurnal.Enabled AndAlso cmbJurnal.SelectedItem Is Nothing Then Return Lipsa(cmbJurnal, "Alegeți jurnalul din listă.")
        Dim k_detalii As NotaManualaDetalii = _context.Detalii
        Dim numarNota As Integer? = Nothing
        If cmbNota.Enabled AndAlso Not String.IsNullOrWhiteSpace(cmbNota.Text) Then
            Dim valoare As Integer
            If Not Integer.TryParse(cmbNota.Text.Trim(), NumberStyles.None, Ro, valoare) OrElse valoare <= 0 Then
                Return Lipsa(cmbNota, "Numărul notei trebuie să fie întreg și pozitiv.")
            End If
            numarNota = valoare
        End If
        If chkDocument.Checked AndAlso Not numarNota.HasValue Then Return Lipsa(cmbNota, "Completați «Notă contabilă».")
        Dim k_nota As NotaManualaNota = NotaAleasa()
        Dim k_doc As NotaManualaDocument = If(k_detalii?.Document, DocumentAles())
        If _adaugaLaDocument Then
            If k_nota Is Nothing Then Return Lipsa(cmbNota, "Alegeți nota contabilă existentă.")
            If k_doc Is Nothing OrElse k_doc.IdNota <> k_nota.Id Then Return Lipsa(cmbDocument, "Alegeți documentul în care adăugați operația.")
        End If
        Dim tip = TryCast(cmbTipOperatie.SelectedItem, NotaManualaTipOperatie)
        If cmbTipOperatie.Enabled AndAlso tip Is Nothing Then Return Lipsa(cmbTipOperatie, "Alegeți tipul operației din listă.")
        If cmbTipOperatie.Enabled AndAlso tip.Cod = "I" Then Return Lipsa(cmbTipOperatie, "Închiderea automată se generează din fluxul de închidere.")
        Dim codTip As String = If(cmbTipOperatie.Enabled, tip?.Cod, Nothing)
        If k_detalii IsNot Nothing AndAlso k_detalii.Document.Fel = "V" AndAlso codTip = String.Empty Then codTip = Nothing
        Dim numarDocument As String = cmbDocument.Text.Trim()
        If cmbDocument.Enabled AndAlso Not _adaugaLaDocument AndAlso numarDocument.Length = 0 Then Return Lipsa(cmbDocument, "Completați «Număr document».")
        If cmbFelDocument.Enabled AndAlso String.IsNullOrWhiteSpace(cmbFelDocument.Text) Then Return Lipsa(cmbFelDocument, "Completați «Fel document».")
        If cmbExplicatie.Enabled AndAlso String.IsNullOrWhiteSpace(cmbExplicatie.Text) Then Return Lipsa(cmbExplicatie, "Completați «Explicație».")
        If Not gridDebit.ReadOnlyGrid Then
            For Each k_pereche As (Grid As KBotDataView, Parte As String) In {(gridDebit, "Debit"), (gridCredit, "Credit")}
                If Not k_pereche.Grid.Rows.Any(Function(row) Not RandGol(row)) Then Return Lipsa(k_pereche.Grid, $"Introduceți cel puțin o linie pe {k_pereche.Parte}.")
                For k_index As Integer = 0 To k_pereche.Grid.RowCount - 1
                    Dim k_row As KBotDataRow = k_pereche.Grid.Rows(k_index)
                    If RandGol(k_row) Then Continue For
                    If DinGrup(k_row("cont")) Is Nothing Then
                        MarcheazaLinia(k_pereche.Grid, k_index)
                        ArataAvertisment($"Alegeți contul pe linia {k_index + 1} de la {k_pereche.Parte}.")
                        Return Nothing
                    End If
                    If TypeOf k_row("suma") IsNot Decimal OrElse CDec(k_row("suma")) = 0D Then
                        MarcheazaLinia(k_pereche.Grid, k_index)
                        ArataAvertisment($"Introduceți suma pe linia {k_index + 1} de la {k_pereche.Parte}.")
                        Return Nothing
                    End If
                Next
            Next
        End If
        If cmbDocument.Enabled AndAlso Not _adaugaLaDocument AndAlso Not ConfirmaNumarulDocumentului() Then Return Nothing
        Dim notaNoua As Boolean? = Nothing
        If k_nota IsNot Nothing Then
            notaNoua = False
        ElseIf Not numarNota.HasValue Then
            notaNoua = True
        End If
        Dim cerere As New NotaManualaCerere With {
            .IdSubunitate = _sid, .An = _an, .IdOper = _idOper, .IdDocument = k_doc?.IdDocument, .IdNota = k_nota?.Id,
            .NotaNoua = notaNoua, .NumarNC = numarNota, .NumarDocument = numarDocument,
            .FelDocument = TextIstoric(cmbFelDocument, k_doc?.FelDocument), .TipOperatie = codTip,
            .DataDocument = DateOnly.FromDateTime(dtpDocument.Value), .DataOperatie = DateOnly.FromDateTime(dtpOperatie.Value),
            .IdJurnal = Cheie(TryCast(cmbJurnal.SelectedItem, NotaManualaAlegere)), .IdClsf = Cheie(ClasificatiaAleasa()),
            .Explicatie = TextIstoric(cmbExplicatie, k_detalii?.Operatie.Explicatie), .Compartiment = TextIstoric(cmbCompartiment, k_detalii?.Operatie.Compartiment),
            .Cfpp = chkCfpp.Checked, .Chelt = chkChelt.Checked,
            .ModificaDocument = chkDocument.Checked, .Versiune = k_detalii?.Versiune, .Debit = Linii(gridDebit), .Credit = Linii(gridCredit),
            .Protectie = _protectie}
        If Not _adaugaLaDocument AndAlso NumarDocumentConfirmat(k_nota, numarDocument) Then cerere.Confirmari.Add(ConfirmareDocumentDublat)
        Return cerere
    End Function

    Private Function Lipsa(k_control As Control, k_mesaj As String) As NotaManualaCerere
        Marcheaza(k_control)
        ArataAvertisment(k_mesaj)
        k_control.Focus()
        Return Nothing
    End Function

    Private Shared Function Linii(grid As KBotDataView) As List(Of NotaManualaLinie)
        Return grid.Rows.Where(Function(row) Not RandGol(row)).Select(Function(row) New NotaManualaLinie With {
            .IdLinie = If(TypeOf row.Tag Is Integer, CType(CInt(row.Tag), Integer?), Nothing),
            .IdCont = If(DinGrup(row("cont"))?.Id, 0),
            .IdPartener = Cheie(TryCast(row("partener"), NotaManualaAlegere)),
            .Suma = If(TypeOf row("suma") Is Decimal, CDec(row("suma")), 0D)}).ToList()
    End Function

    Private Shared Function Cheie(alegere As NotaManualaAlegere) As Integer?
        If alegere Is Nothing OrElse alegere.Id <= 0 Then Return Nothing
        Return alegere.Id
    End Function

    ' ── marking the boxes in error ───────────────────────────────────────────

    ' The box the server named (camp), or the line (parte + linie).
    Private Sub MarcheazaRefuz(k_refuz As NotaManualaRefuz)
        Dim k_grid As KBotDataView = If(k_refuz.Parte = "Debit", gridDebit, If(k_refuz.Parte = "Credit", gridCredit, Nothing))
        If k_grid IsNot Nothing AndAlso k_refuz.Linie.HasValue AndAlso k_refuz.Linie.Value >= 0 AndAlso k_refuz.Linie.Value < k_grid.RowCount Then
            MarcheazaLinia(k_grid, k_refuz.Linie.Value)
        End If
        Dim k_control As Control = ControlPentru(k_refuz.Camp)
        If k_control IsNot Nothing Then Marcheaza(k_control)
    End Sub

    Private Function ControlPentru(k_camp As String) As Control
        Select Case k_camp
            Case "IdJurnal" : Return cmbJurnal
            Case "IdClsf" : Return cmbClsf
            Case "IdNota", "NumarNC" : Return cmbNota
            Case "IdDocument", "NumarDocument" : Return cmbDocument
            Case "FelDocument" : Return cmbFelDocument
            Case "TipOperatie" : Return cmbTipOperatie
            Case "DataDocument" : Return dtpDocument
            Case "DataOperatie" : Return dtpOperatie
            Case "Explicatie" : Return cmbExplicatie
            Case "Compartiment" : Return cmbCompartiment
            Case "Debit" : Return gridDebit
            Case "Credit" : Return gridCredit
            Case Else : Return Nothing
        End Select
    End Function

    Private Sub Marcheaza(k_control As Control)
        SeteazaChenar(k_control, ThemeManager.Current.Palette.ErrorColor)
        If Not _marcate.Contains(k_control) Then _marcate.Add(k_control)
    End Sub

    Private Sub MarcheazaLinia(k_grid As KBotDataView, k_index As Integer)
        If _gridEroare IsNot Nothing AndAlso Not ReferenceEquals(_gridEroare, k_grid) Then _gridEroare.Invalidate()
        _gridEroare = k_grid
        _randEroare = k_index
        k_grid.Invalidate()
    End Sub

    Private Sub CurataMarcaje()
        For Each k_control As Control In _marcate
            SeteazaChenar(k_control, Color.Empty)
        Next
        _marcate.Clear()
        If _gridEroare IsNot Nothing Then _gridEroare.Invalidate()
        _gridEroare = Nothing
        _randEroare = -1
    End Sub

    ' Color.Empty = the colour of the theme again.
    Private Shared Sub SeteazaChenar(k_control As Control, k_color As Color)
        If TypeOf k_control Is KBotTextField Then
            DirectCast(k_control, KBotTextField).BorderColor = k_color
        ElseIf TypeOf k_control Is KBotComboBox Then
            DirectCast(k_control, KBotComboBox).BorderColor = k_color
        ElseIf TypeOf k_control Is KBotDatePicker Then
            DirectCast(k_control, KBotDatePicker).BorderColor = k_color
        ElseIf TypeOf k_control Is KBotDataView Then
            DirectCast(k_control, KBotDataView).BorderColor = k_color
        End If
    End Sub

    Private Sub Grid_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridDebit.CellFormatting, gridCredit.CellFormatting
        Select Case e.ColumnKey
            Case "cont", "partener"
                Dim alegere As NotaManualaAlegere = DinGrup(e.Value)
                If alegere IsNot Nothing AndAlso Not ReferenceEquals(alegere, FaraPartener) Then
                    e.Text = If(alegere.Cod, String.Empty)
                End If
            Case "sterge"
                If e.Row IsNot Nothing AndAlso RandGol(e.Row) Then
                    e.HideButton = True
                    e.Enabled = False
                End If
        End Select
        If ReferenceEquals(sender, _gridEroare) AndAlso e.RowIndex = _randEroare Then e.BackColor = Color.FromArgb(60, ThemeManager.Current.Palette.ErrorColor)
    End Sub

    ' A marked box loses its mark when the operator changes it.
    Private Sub Camp_Changed(sender As Object, e As EventArgs) Handles cmbNota.TextChanged, cmbDocument.TextChanged, cmbFelDocument.TextChanged,
        cmbTipOperatie.TextChanged, cmbExplicatie.TextChanged, cmbCompartiment.TextChanged, cmbJurnal.SelectedIndexChanged,
        dtpDocument.ValueChanged, dtpOperatie.ValueChanged
        Dim k_control As Control = TryCast(sender, Control)
        If k_control Is Nothing OrElse Not _marcate.Remove(k_control) Then Return
        SeteazaChenar(k_control, Color.Empty)
    End Sub

    Private Sub Clasificatie_Marcaj(sender As Object, e As EventArgs) Handles cmbClsf.SelectedIndexChanged
        If _marcate.Remove(cmbClsf) Then SeteazaChenar(cmbClsf, Color.Empty)
    End Sub

    Private Sub Grid_ValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridDebit.CellValueChanged, gridCredit.CellValueChanged
        Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
        If Not _loading AndAlso Not grid.ReadOnlyGrid Then
            PastreazaRandDeAdaugare(grid)
            grid.InvalidateRow(e.RowIndex)
        End If
        If Not ReferenceEquals(sender, _gridEroare) Then Return
        _gridEroare.Invalidate()
        _gridEroare = Nothing
        _randEroare = -1
    End Sub

End Class
