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

' SLICE-KE21-02: KeReceptiiView.Amounts - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' How much of what is left a line takes: the whole value while there is enough, what is left when it is less, nothing when nothing is.
    Private Shared Function TakeFrom(k_value As Decimal, k_left As Decimal) As Decimal
        If k_left = 0D OrElse Math.Sign(k_left) <> Math.Sign(k_value) Then Return 0D
        Return If(Math.Abs(k_left) < Math.Abs(k_value), k_left, k_value)
    End Function

    ' The amounts proposed on the lines the accountant did not type: the value of the line, cut to what the invoice still has on its classification
    ' (partly linked before) and, for an existing reception, to what that reception still has. Typed amounts are taken out first.
    Private Sub RecomputeDefaults()
        Dim k_rec As EFacturaKeReceptie = If(_mode = EFacturaKeTip.Existenta, ChosenReceptie(), Nothing)
        Dim k_recLeft As New Dictionary(Of Integer, Decimal)()
        If k_rec IsNot Nothing Then
            For Each k_pc As EFacturaKeReceptieClasificatie In k_rec.PeClasificatie
                k_recLeft(k_pc.IdClsf) = k_pc.Rest
            Next
        End If
        _filling = True
        gridLinii.BeginUpdate()
        Try
            ' Slice KE20-04: the whole of what is left of the invoice (its total less its links and the sums typed on its rows) goes to the classified rows not typed, in order -- in
            ' practice the first one; for an existing reception cut to what that reception still has on the classification. A row without a classification has no sum.
            For Each k_id As Integer In _checked
                Dim k_rows As List(Of KBotDataRow) = RowsOf(k_id)
                Dim k_whole As Decimal = InvoiceRest(k_id)
                Dim k_left As Decimal = k_whole
                For Each k_row As KBotDataRow In k_rows.Where(Function(k_r) CType(k_r.Tag, LineTag).Edited AndAlso ChosenClass(k_r).HasValue)
                    Dim k_typed As Decimal = 0D
                    ReadNumber(k_row(ColLRec), k_typed)
                    k_left -= k_typed
                    Dim k_typedCls As Integer = ChosenClass(k_row).Value
                    If k_recLeft.ContainsKey(k_typedCls) Then k_recLeft(k_typedCls) -= k_typed
                Next
                For Each k_row As KBotDataRow In k_rows.Where(Function(k_r) Not CType(k_r.Tag, LineTag).Edited OrElse Not ChosenClass(k_r).HasValue)
                    Dim k_cls As Integer? = ChosenClass(k_row)
                    If Not k_cls.HasValue Then
                        k_row(ColLRec) = Nothing
                        Continue For
                    End If
                    Dim k_take As Decimal = TakeFrom(k_whole, k_left)
                    If k_rec IsNot Nothing Then k_take = TakeFrom(k_take, k_recLeft.GetValueOrDefault(k_cls.Value))
                    k_left -= k_take
                    If k_rec IsNot Nothing Then k_recLeft(k_cls.Value) = k_recLeft.GetValueOrDefault(k_cls.Value) - k_take
                    k_row(ColLRec) = CDbl(k_take)
                Next
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The sum of the amounts of the lines of one invoice, per classification (zero ones left out).
    Private Function AmountsOf(k_idPrimita As Integer) As Dictionary(Of Integer, Decimal)
        Dim k_sums As New Dictionary(Of Integer, Decimal)()
        For Each k_row As KBotDataRow In RowsOf(k_idPrimita)
            If Not IsIncluded(k_row) Then Continue For
            Dim k_cls As Integer? = ChosenClass(k_row)
            Dim k_number As Decimal = 0D
            If Not k_cls.HasValue OrElse Not ReadNumber(k_row(ColLRec), k_number) Then Continue For
            k_sums(k_cls.Value) = k_sums.GetValueOrDefault(k_cls.Value) + k_number
        Next
        Return k_sums.Where(Function(k_p) Math.Abs(k_p.Value) >= Tolerance).ToDictionary(Function(k_p) k_p.Key, Function(k_p) Math.Round(k_p.Value, 2))
    End Function

    ' SLICE-KE20-07: use the availability snapshot actually displayed in this row.
    Private Function AvailabilityTooltip(k_column As String, k_row As KBotDataRow) As String
        Dim k_data As EFacturaKeDisponibil = Nothing
        If Not _alocDisponibil.TryGetValue(k_row, k_data) OrElse k_data.Detalii Is Nothing OrElse
            k_row(k_column) Is Nothing Then Return Nothing

        Dim k_rows As IEnumerable(Of EFacturaKeContributieDisponibil)
        Dim k_labelHeader As String
        Dim k_hasDate As Boolean = k_column <> ColAReceptii
        Dim k_total As Decimal
        Select Case k_column
            Case ColAReceptii
                k_rows = k_data.Detalii.ReceptiiPeAngajamente
                k_labelHeader = "Cod angajament"
                k_total = k_data.Receptii
            Case ColAAngRezervari
                If Not k_data.RezervariAng.HasValue Then Return Nothing
                k_rows = k_data.Detalii.Rezervari
                k_labelHeader = "Tip rezervare"
                k_total = k_data.RezervariAng.Value
            Case ColAAngReceptii
                If Not k_data.ReceptiiAng.HasValue Then Return Nothing
                k_rows = k_data.Detalii.Receptii
                k_labelHeader = "Recepție"
                k_total = k_data.ReceptiiAng.Value
            Case Else
                Return Nothing
        End Select

        Dim k_header As New XElement("header")
        If k_hasDate Then k_header.Add(New XElement("cell", "Data"))
        k_header.Add(New XElement("cell", k_labelHeader), New XElement("cell", New XAttribute("Align", "right"), "Valoare"))
        Dim k_table As New XElement("table", New XAttribute("MaxWidth", 520), k_header)
        Dim k_order = StringComparer.Create(_ro, True)
        For Each k_item As EFacturaKeContributieDisponibil In k_rows.OrderBy(Function(k_entry) k_entry.Data).
                ThenBy(Function(k_entry) k_entry.Eticheta, k_order).ThenBy(Function(k_entry) k_entry.Suma)
            Dim k_detail As New XElement("row")
            If k_hasDate Then
                k_detail.Add(New XElement("cell", If(k_item.Data.HasValue, k_item.Data.Value.ToString("dd.MM.yyyy", _ro), String.Empty)))
            End If
            k_detail.Add(New XElement("cell", k_item.Eticheta),
                         TooltipAmountCell(k_item.Suma.ToString("#,0.00######", _ro)))
            k_table.Add(k_detail)
        Next
        Dim k_footer As New XElement("footer", New XElement("cell", New XAttribute("Bold", 1), "Total"))
        If k_hasDate Then k_footer.Add(New XElement("cell"))
        k_footer.Add(TooltipAmountCell(Fmt(k_total)))
        k_table.Add(k_footer)
        Return k_table.ToString(SaveOptions.DisableFormatting)
    End Function

    Private Shared Function TooltipAmountCell(k_text As String) As XElement
        Return New XElement("cell", New XAttribute("Align", "right"), k_text)
    End Function

    ' One row per classification of the ticked invoices: Buget, Receptii (before the day), Disponibil, Valoare (the sum of «Recepționată») and Ramas.
    Private Sub FillAlocari()
        _filling = True
        gridAloc.BeginUpdate()
        Try
            gridAloc.ClearRows()
            _alocDisponibil.Clear()
            Dim k_valRecep As New Dictionary(Of Integer, Decimal)()
            Dim k_name As New Dictionary(Of Integer, String)()
            For Each k_row As KBotDataRow In gridLinii.Rows
                If Not IsIncluded(k_row) Then Continue For
                Dim k_cls As Integer? = ChosenClass(k_row)
                If Not k_cls.HasValue Then Continue For
                Dim k_number As Decimal = 0D
                ReadNumber(k_row(ColLRec), k_number)
                k_valRecep(k_cls.Value) = k_valRecep.GetValueOrDefault(k_cls.Value) + k_number
                If Not k_name.ContainsKey(k_cls.Value) Then
                    k_name(k_cls.Value) = If(_labelOf.ContainsKey(k_cls.Value), _labelOf(k_cls.Value), Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture))
                End If
            Next
            ' Slice KE20-01: a new / modified reception may take a value on ANY indicator of the angajament, also on one no ticked invoice stands on (its invoice may be missing).
            If AllowsExtra() Then
                For Each k_ind As EFacturaKeIndicator In _data.Indicatori
                    If Not k_ind.IdClsf.HasValue OrElse k_name.ContainsKey(k_ind.IdClsf.Value) Then Continue For
                    k_name(k_ind.IdClsf.Value) = If(_labelOf.ContainsKey(k_ind.IdClsf.Value), _labelOf(k_ind.IdClsf.Value), k_ind.Clsf)
                Next
            End If
            Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
            For Each k_pair As KeyValuePair(Of Integer, String) In k_name.OrderBy(Function(k_p) k_p.Value, StringComparer.CurrentCultureIgnoreCase)
                Dim k_row As KBotDataRow = gridAloc.AddRow()
                Dim k_val As Decimal = k_valRecep.GetValueOrDefault(k_pair.Key) + ExtraOf(k_pair.Key)
                Dim k_label As String = k_pair.Value
                If _data IsNot Nothing AndAlso IndicatorOf(k_pair.Key) Is Nothing Then k_label &= "  (nu e indicator al angajamentului)"
                k_row(ColAClsf) = k_label
                ' As in the fundamentation document: Buget (of the classification on the day), Receptii (before the day), Disponibil = Buget - Receptii,
                ' Valoare (this reception) and Ramas = Disponibil - Valoare.
                Dim k_d As EFacturaKeDisponibil = DispoOf(k_pair.Key)
                If k_d IsNot Nothing Then
                    _alocDisponibil(k_row) = k_d
                    k_row(ColAReceptii) = CDbl(k_d.Receptii)
                    If k_d.Buget.HasValue Then
                        Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
                        k_row(ColABuget) = CDbl(k_d.Buget.Value)
                        k_row(ColADisponibil) = CDbl(k_available)
                    End If
                End If
                k_row(ColAValRecep) = CDbl(k_val)
                ' Slice 02EF-02: the row stands for this classification (the edit of «Valoare» finds the lines by it); the columns per angajament and «Ramas» are the angajament's.
                k_row.Tag = k_pair.Key
                If k_d IsNot Nothing AndAlso k_d.RezervariAng.HasValue AndAlso k_d.ReceptiiAng.HasValue Then
                    k_row(ColAAngRezervari) = CDbl(k_d.RezervariAng.Value)
                    k_row(ColAAngReceptii) = CDbl(k_d.ReceptiiAng.Value)
                    k_row(ColAAngDisponibil) = CDbl(k_d.RezervariAng.Value - k_d.ReceptiiAng.Value)
                    k_row(ColARamas) = CDbl(k_d.RezervariAng.Value - k_d.ReceptiiAng.Value - k_val)
                End If
                If k_rec IsNot Nothing Then
                    Dim k_pc As EFacturaKeReceptieClasificatie = k_rec.PeClasificatie.FirstOrDefault(Function(k_x) k_x.IdClsf = k_pair.Key)
                    ' SLICE-KE20-07: the balance after applying the proposed allocation.
                    k_row(ColARestRecep) = CDbl(If(k_pc IsNot Nothing, k_pc.Rest, 0D) - k_val)
                End If
            Next
            ' Keep the unused balance visible on classifications with no proposed allocation.
            If k_rec IsNot Nothing Then
                For Each k_other As EFacturaKeReceptieClasificatie In k_rec.PeClasificatie.Where(
                        Function(k_x) Math.Abs(k_x.Rest) >= Tolerance AndAlso Not k_name.ContainsKey(k_x.IdClsf)).OrderBy(Function(k_x) k_x.Clsf, StringComparer.Ordinal)
                    Dim k_extra As KBotDataRow = gridAloc.AddRow()
                    k_extra(ColAClsf) = $"{If(_labelOf.ContainsKey(k_other.IdClsf), _labelOf(k_other.IdClsf), k_other.Clsf)}  (recepția are rest aici; nicio linie nu e pe ea)"
                    k_extra(ColARestRecep) = CDbl(k_other.Rest)
                Next
            End If
        Finally
            gridAloc.EndUpdate()
            _filling = False
        End Try
        RefreshOk()
    End Sub

    ' What the angajament in use still has on a classification ON THE DAY of the reception: its reservations minus its receptions taken before that day (both from the
    ' server's answer for the angajament and day, like Buget / Total recepții). Nothing = not an indicator of the angajament, or the answer is not in yet / does not say.
    Private Function AngDisponibil(k_idClsf As Integer) As Decimal?
        Dim k_d As EFacturaKeDisponibil = DispoOf(k_idClsf)
        If k_d Is Nothing OrElse Not k_d.RezervariAng.HasValue OrElse Not k_d.ReceptiiAng.HasValue Then Return Nothing
        Return k_d.RezervariAng.Value - k_d.ReceptiiAng.Value
    End Function

    ' Slice KE20-01: a reception that is made or modified may take a value above the invoices; a link to an existing reception never (its value does not change).
    Private Function AllowsExtra() As Boolean
        Return _data IsNot Nothing AndAlso _viewing Is Nothing AndAlso _mode <> EFacturaKeTip.Existenta
    End Function

    ' The value above the lines on one classification (0 when none, or when the mode takes none).
    Private Function ExtraOf(k_idClsf As Integer) As Decimal
        If Not AllowsExtra() Then Return 0D
        Return _extra.GetValueOrDefault(k_idClsf)
    End Function

    ' What the reception takes on each classification: the lines of the ticked invoices plus the value above them (zero ones left out by the callers).
    Private Function ReceptionTotals() As Dictionary(Of Integer, Decimal)
        Dim k_total As New Dictionary(Of Integer, Decimal)()
        For Each k_id As Integer In _checked
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In AmountsOf(k_id)
                k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
            Next
        Next
        If AllowsExtra() Then
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In _extra
                k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
            Next
        End If
        Return k_total
    End Function

    ' The amount of every classification, from the lines (as FillAlocari sums it).
    Private Function ClassSums() As Dictionary(Of Integer, Decimal)
        Dim k_sums As New Dictionary(Of Integer, Decimal)()
        For Each k_row As KBotDataRow In gridLinii.Rows
            If Not IsIncluded(k_row) Then Continue For
            Dim k_cls As Integer? = ChosenClass(k_row)
            If Not k_cls.HasValue Then Continue For
            Dim k_number As Decimal = 0D
            ReadNumber(k_row(ColLRec), k_number)
            k_sums(k_cls.Value) = k_sums.GetValueOrDefault(k_cls.Value) + k_number
        Next
        Return k_sums
    End Function

    ' The rows of the grid of classifications keep their place; only «Valoare» and «Ramas» are written again from the lines (a rebuild inside the edit of one of its own cells
    ' would take the row away from under the operator).
    Private Sub UpdateAlocValues()
        Dim k_sums As Dictionary(Of Integer, Decimal) = ClassSums()
        _filling = True
        Try
            For Each k_row As KBotDataRow In gridAloc.Rows
                If Not (TypeOf k_row.Tag Is Integer) Then Continue For
                Dim k_cls As Integer = CInt(k_row.Tag)
                Dim k_value As Decimal = k_sums.GetValueOrDefault(k_cls) + ExtraOf(k_cls)
                k_row(ColAValRecep) = CDbl(k_value)
                Dim k_ang As Decimal? = AngDisponibil(k_cls)
                If k_ang.HasValue Then k_row(ColARamas) = CDbl(k_ang.Value - k_value)
            Next
        Finally
            _filling = False
        End Try
        gridAloc.Invalidate()
        RefreshOk()
    End Sub

    ' The sum typed in a classification can be written only on a row that stands for one (never while an invoice already covered is only looked at).
    Private Sub GridAloc_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridAloc.CellFormatting
        Try
            If e.Row Is Nothing OrElse Not String.Equals(e.ColumnKey, ColAValRecep, StringComparison.Ordinal) Then Return
            ' Slice KE20-04: for «Leagă» the reception's value does not change, so nothing is typed here (the sums are those of the table of invoices).
            If _viewing IsNot Nothing OrElse _mode = EFacturaKeTip.Existenta OrElse Not (TypeOf e.Row.Tag Is Integer) Then e.ReadOnlyCell = True
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridAloc_CellFormatting", ex)
        End Try
    End Sub

    ' «Valoare» takes a number the lines of the classification can hold and, for a reception that is made or modified, one the angajament still has (the rest may not go below 0).
    Private Sub GridAloc_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridAloc.CellValidating
        Try
            If _filling OrElse Not String.Equals(e.ColumnKey, ColAValRecep, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAloc.RowCount OrElse Not (TypeOf gridAloc.Rows(e.RowIndex).Tag Is Integer) Then
                e.Cancel = True
                Return
            End If
            Dim k_cls As Integer = CInt(gridAloc.Rows(e.RowIndex).Tag)
            Dim k_number As Decimal
            If Not ReadNumber(e.ProposedValue, k_number) Then
                ShowNotice("Valoarea nu este un număr.", NoticeKind.Warning)
                e.Cancel = True
                Return
            End If
            If _mode = EFacturaKeTip.Existenta Then
                e.Cancel = True   ' read-only there (CellFormatting); never reached by the editor
                Return
            End If
            ' Slice KE20-01: a new / modified reception may take MORE than the ticked invoices (the surplus stays on the reception), never less than their sums:
            ' lower the sums in the table of invoices instead. A credit note (negative sums) takes no surplus.
            Dim k_lines As Decimal = ClassSums().GetValueOrDefault(k_cls)
            If k_lines < -Tolerance Then
                If Math.Abs(k_number - k_lines) >= Tolerance Then
                    ShowNotice($"Pe această clasificație facturile bifate sunt o stornare ({Fmt(k_lines)}): valoarea nu se schimbă de aici.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
            ElseIf k_number < k_lines - Tolerance Then
                ShowNotice($"Valoarea nu poate fi sub totalul facturilor bifate pe această clasificație ({Fmt(k_lines)}). Scădeți în tabelul cu facturi, la «Recepționată».", NoticeKind.Warning)
                e.Cancel = True
                Return
            End If
            Dim k_ang As Decimal? = AngDisponibil(k_cls)
            If _mode <> EFacturaKeTip.Existenta AndAlso k_ang.HasValue AndAlso k_number > Tolerance AndAlso k_number > k_ang.Value + Tolerance Then
                ShowNotice($"Valoarea depășește disponibilul angajamentului ({Fmt(k_ang.Value)}): rămasul nu poate fi sub 0.", NoticeKind.Error)
                e.Cancel = True
                Return
            End If
            e.ProposedValue = CDbl(k_number)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridAloc_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' UI boundary: «Valoare» of a classification was typed: it goes on the lines, and «Recepționată» (and the rest of this grid) follow.
    Private Sub GridAloc_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAloc.CellValueChanged
        Try
            If _filling OrElse Not String.Equals(e.ColumnKey, ColAValRecep, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAloc.RowCount OrElse Not (TypeOf gridAloc.Rows(e.RowIndex).Tag Is Integer) Then Return
            Dim k_number As Decimal = 0D
            ReadNumber(gridAloc.Rows(e.RowIndex)(ColAValRecep), k_number)
            Dim k_cls As Integer = CInt(gridAloc.Rows(e.RowIndex).Tag)
            If _mode = EFacturaKeTip.Existenta Then Return
            ' Slice KE20-01: what is above the sums of the invoices is kept as the surplus; the invoices are not touched.
            Dim k_above As Decimal = k_number - ClassSums().GetValueOrDefault(k_cls)
            If k_above > Tolerance Then _extra(k_cls) = Math.Round(k_above, 2) Else _extra.Remove(k_cls)
            UpdateAlocValues()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridAloc_CellValueChanged", ex)
        End Try
    End Sub

End Class
