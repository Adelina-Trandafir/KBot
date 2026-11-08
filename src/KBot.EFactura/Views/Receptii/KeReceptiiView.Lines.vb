Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-KE21-02: KeReceptiiView.Lines - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' Slice KE12-06: an angajament with ONE indicator gives its classification to the lines by itself. k_all = every line (the angajament was just pressed: pressing another
    ' angajament changes them again); otherwise only the lines that have no classification yet (invoices ticked later).
    Private Sub ApplySingleIndicator(k_all As Boolean)
        If _data Is Nothing Then Return
        Dim k_ids As List(Of Integer) = _data.Indicatori.Where(Function(k_i) k_i.IdClsf.HasValue).Select(Function(k_i) k_i.IdClsf.Value).Distinct().ToList()
        Dim k_label As String = Nothing
        If k_ids.Count <> 1 OrElse Not _labelOf.TryGetValue(k_ids(0), k_label) Then Return
        _filling = True
        Try
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_current As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
                If k_all OrElse String.IsNullOrWhiteSpace(k_current) OrElse String.Equals(k_current, NeAleasa, StringComparison.Ordinal) Then k_row(ColLClsf) = k_label
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The indicators of the angajamente are the choices; a classification a line already has that is not one of them stays visible.
    Private Sub ApplyIndicators()
        _byLabel.Clear()
        _labelOf.Clear()
        Dim k_known As List(Of EFacturaKeReceptiiAngajament) = angajamente.KnownData()
        If k_known.Count = 0 Then Return
        Dim k_choices As New List(Of Object) From {NeAleasa}
        For Each k_i As EFacturaKeIndicator In k_known.SelectMany(Function(k_d) k_d.Indicatori)
            If Not k_i.IdClsf.HasValue OrElse _labelOf.ContainsKey(k_i.IdClsf.Value) Then Continue For
            ' Only the classification code, no name (operator, 18.10.2026); two indicators with the same code get the id added so the labels stay distinct.
            Dim k_label As String = k_i.Clsf
            If _byLabel.ContainsKey(k_label) Then k_label = $"{k_i.Clsf} ({k_i.IdClsf.Value})"
            _byLabel(k_label) = k_i.IdClsf.Value
            _labelOf(k_i.IdClsf.Value) = k_label
            k_choices.Add(k_label)
        Next
        For Each k_id As Integer In _checked
            For Each k_l As EFacturaKeLinie In _states(k_id).Linii
                If k_l.IdClsf.HasValue AndAlso Not _labelOf.ContainsKey(k_l.IdClsf.Value) Then
                    Dim k_label As String = $"{k_l.Clsf} (nu e indicator al angajamentului)"
                    _byLabel(k_label) = k_l.IdClsf.Value
                    _labelOf(k_l.IdClsf.Value) = k_label
                    k_choices.Add(k_label)
                End If
            Next
        Next
        _filling = True
        Try
            gridLinii.Column(ColLClsf).ComboItems = k_choices
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_init As Integer? = CType(k_row.Tag, LineTag).InitClsf
                Dim k_current As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
                If String.Equals(k_current, NeAleasa, StringComparison.Ordinal) OrElse _byLabel.ContainsKey(k_current) Then Continue For
                ' a choice made from another angajament's list (or the raw text before any angajament) is not one of these
                k_row(ColLClsf) = If(k_init.HasValue AndAlso _labelOf.ContainsKey(k_init.Value), _labelOf(k_init.Value), NeAleasa)
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
        QueueReshape()   ' slice KE20-04: a row whose classification is not on the new list became an empty row
    End Sub

    ' The classification the row stands for: the one picked from the list; before any angajament was opened the row keeps the one its invoice had.
    Private Function ChosenClass(k_row As KBotDataRow) As Integer?
        Dim k_init As Integer? = CType(k_row.Tag, LineTag).InitClsf
        Dim k_text As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
        Dim k_id As Integer = 0
        If Not String.IsNullOrWhiteSpace(k_text) AndAlso Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso
           _byLabel.TryGetValue(k_text, k_id) Then Return k_id
        If _byLabel.Count = 0 AndAlso k_init.HasValue AndAlso Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) Then Return k_init
        Return Nothing
    End Function

    Private Function RowsOf(k_idPrimita As Integer) As List(Of KBotDataRow)
        Return gridLinii.Rows.Where(Function(k_r) CType(k_r.Tag, LineTag).Invoice.IdPrimita = k_idPrimita).ToList()
    End Function

    ' Slice KE12-03: a line whose tick box is cleared takes no part in the reception (its «Recepționată» is 0 and cannot be typed).
    Private Shared Function IsIncluded(k_row As KBotDataRow) As Boolean
        Dim k_v As Object = k_row(KBotDataView.CheckColumnKey)
        Return Not (TypeOf k_v Is Boolean) OrElse CBool(k_v)
    End Function

    ' The empty row has no sum to type until it has a classification (its sum is then what is left of the invoice).
    Private Sub GridLinii_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridLinii.CellFormatting
        Try
            If e.Row Is Nothing OrElse _viewing IsNot Nothing OrElse Not String.Equals(e.ColumnKey, ColLRec, StringComparison.Ordinal) Then Return
            If Not (TypeOf e.Row.Tag Is LineTag) OrElse Not ChosenClass(e.Row).HasValue Then e.ReadOnlyCell = True
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridLinii_CellFormatting", ex)
        End Try
    End Sub

    ' Only a choice from the list is accepted in a cell; «Recepționată» takes a number of the sign of the invoice, not above what is left of it after the other rows of the invoice.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            If e.ColumnKey = ColLClsf Then
                Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
                If String.IsNullOrWhiteSpace(k_text) Then
                    e.ProposedValue = NeAleasa
                ElseIf Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso Not _byLabel.ContainsKey(k_text) Then
                    e.Cancel = True
                End If
            ElseIf e.ColumnKey = ColLRec Then
                Dim k_number As Decimal
                If Not ReadNumber(e.ProposedValue, k_number) Then
                    ShowNotice("Suma recepționată nu este un număr.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                ' Slice KE20-04: what is left of the invoice after its OTHER rows (the whole invoice when it has none).
                Dim k_row As KBotDataRow = gridLinii.Rows(e.RowIndex)
                Dim k_idInv As Integer = CType(k_row.Tag, LineTag).Invoice.IdPrimita
                Dim k_rest As Decimal = InvoiceRest(k_idInv)
                Dim k_cap As Decimal = k_rest - AllocatedOf(k_idInv, k_row)
                If k_number <> 0D AndAlso (Math.Sign(k_number) <> Math.Sign(k_rest) OrElse Math.Abs(k_number) > Math.Abs(k_cap) + Tolerance) Then
                    ShowNotice($"Suma nu poate depăși ce a mai rămas din factură ({Fmt(k_cap)}) și are semnul facturii.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                e.ProposedValue = CDbl(k_number)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridLinii_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' UI boundary: a classification picked in the header of an invoice is the one of its FIRST row (made, with the whole of what is left, when the invoice has none yet).
    Private Sub GridLinii_GroupHeaderValuePicked(sender As Object, e As KBotGroupHeaderValueEventArgs) Handles gridLinii.GroupHeaderValuePicked
        Try
            If Not String.Equals(e.ColumnKey, ColLClsf, StringComparison.Ordinal) Then Return
            Dim k_text As String = Convert.ToString(e.Value, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) OrElse String.Equals(k_text, NeAleasa, StringComparison.Ordinal) OrElse e.RowIndexes.Count = 0 Then Return
            Dim k_first As KBotDataRow = gridLinii.Rows(e.RowIndexes(0))
            _filling = True
            Try
                k_first(ColLClsf) = k_text
            Finally
                _filling = False
            End Try
            gridLinii.Invalidate()
            Dim k_pickedId As Integer = 0
            If _byLabel.TryGetValue(k_text, k_pickedId) Then angajamente.EnsureAngajamentFor(k_pickedId)
            RecomputeDefaults()
            FillAlocari()
            QueueReshape()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridLinii_GroupHeaderValuePicked", ex)
        End Try
    End Sub

    ' A classification picked in a row, or a sum typed in it. The EMPTY row that gets a classification becomes an allocation: the first one of the invoice is proposed (the whole of
    ' what is left); a later one takes what the others left and counts as typed. A classification taken off a row empties it. The amounts follow; the rows are reshaped a moment later.
    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridLinii.RowCount Then Return
            Dim k_row As KBotDataRow = gridLinii.Rows(e.RowIndex)
            Dim k_tag As LineTag = CType(k_row.Tag, LineTag)
            If e.ColumnKey = ColLClsf Then
                Dim k_picked As Integer? = ChosenClass(k_row)
                If k_picked.HasValue Then
                    angajamente.EnsureAngajamentFor(k_picked.Value)
                    Dim k_wasEmpty As Boolean = k_row(ColLRec) Is Nothing OrElse TypeOf k_row(ColLRec) Is DBNull
                    If k_wasEmpty Then
                        _filling = True
                        Try
                            k_row(ColLDenumire) = String.Empty   ' no longer the empty row: its «Rest de alocat» note goes
                        Finally
                            _filling = False
                        End Try
                        Dim k_id As Integer = k_tag.Invoice.IdPrimita
                        If RowsOf(k_id).Any(Function(k_r) Not ReferenceEquals(k_r, k_row) AndAlso ChosenClass(k_r).HasValue) Then
                            _filling = True
                            Try
                                k_row(ColLRec) = CDbl(InvoiceRest(k_id) - AllocatedOf(k_id, k_row))
                            Finally
                                _filling = False
                            End Try
                            k_tag.Edited = True
                        End If
                    End If
                Else
                    _filling = True
                    Try
                        k_row(ColLRec) = Nothing
                    Finally
                        _filling = False
                    End Try
                    k_tag.Edited = False
                End If
            ElseIf e.ColumnKey = ColLRec Then
                k_tag.Edited = True
            Else
                Return
            End If
            RecomputeDefaults()
            FillAlocari()
            QueueReshape()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

End Class
