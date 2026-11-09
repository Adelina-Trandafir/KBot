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

' SLICE-KE21-02: KeReceptiiView.Receptii - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' The day the budget and the receptions are taken on: the date of the new reception, or the date of the chosen one.
    Private Function DispoDay() As Date
        If _mode = EFacturaKeTip.Noua Then Return pgNoua.dtpData.Value.Date
        Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
        Return If(k_rec IsNot Nothing AndAlso k_rec.DataR.HasValue, k_rec.DataR.Value.Date, Date.Today)
    End Function

    Private Function DispoOf(k_idClsf As Integer) As EFacturaKeDisponibil
        Dim k_d As EFacturaKeDisponibil = Nothing
        Return If(_dispo.TryGetValue(k_idClsf, k_d), k_d, Nothing)
    End Function

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub BeginRefreshDispo()
        Try
            Await RefreshDispoAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeReceptiiView.BeginRefreshDispo", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.BeginRefreshDispo", ex)
            ShowNotice("Bugetul și recepțiile pe indicatori nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Private Async Function RefreshDispoAsync() As Task
        If _api Is Nothing OrElse _data Is Nothing OrElse String.IsNullOrWhiteSpace(_angCod) Then Return
        Dim k_day As Date = DispoDay()
        Dim k_key As String = _angCod & "|" & k_day.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
        If String.Equals(k_key, _dispoKey, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_mySeq As Integer = Interlocked.Increment(_dispoSeq)
        Dim k_answer As EFacturaKeDisponibilZi = Nothing
        If Not _dispoCache.TryGetValue(k_key, k_answer) Then
            Dim k_cod As String = _angCod
            k_answer = Await _gate.RunAsync(Function() _api.GetKeDisponibilAsync(k_cod, k_day, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _dispoSeq Then Return
            _dispoCache(k_key) = k_answer
        End If
        If IsDisposed OrElse k_mySeq <> _dispoSeq Then Return
        _dispo.Clear()
        For Each k_d As EFacturaKeDisponibil In k_answer.Indicatori
            If k_d.IdClsf.HasValue AndAlso Not _dispo.ContainsKey(k_d.IdClsf.Value) Then _dispo(k_d.IdClsf.Value) = k_d
        Next
        _dispoKey = k_key
        _dispoWarning = String.Join(" ", k_answer.Avertismente)
        FillAlocari()
    End Function

    Friend Sub FillReceptii()
        If _viewing IsNot Nothing Then
            FillViewedReceptii()
            Return
        End If
        If _assocTab Then
            FillAssociatedReceptii()
            Return
        End If
        pgExistente.gridReceptii.BeginUpdate()
        Try
            pgExistente.gridReceptii.ClearRows()
            If _data Is Nothing Then Return
            _recShownKey = CurrentRecKey()
            For Each k_r As EFacturaKeReceptie In ReceptiiList().OrderBy(Function(kr) kr.DataR).ThenBy(Function(kr) kr.Descriere, StringComparer.Create(_ro, True))
                ' Linking to an existing reception: one whose rest is 0 on every classification can take no invoice, so it is not listed (operator, 18.10.2026).
                If _mode = EFacturaKeTip.Existenta AndAlso k_r.PeClasificatie.All(Function(k_c) Math.Abs(k_c.Rest) < Tolerance) Then Continue For
                Dim k_row As KBotDataRow = pgExistente.gridReceptii.AddRow()
                k_row.Tag = k_r
                k_row(ColRData) = If(k_r.DataR.HasValue, CObj(k_r.DataR.Value), Nothing)
                k_row(ColRDescriere) = k_r.Descriere
                k_row(ColRTotal) = CDbl(k_r.Total)
                k_row(ColRRest) = CDbl(k_r.PeClasificatie.Sum(Function(k_c) k_c.Rest))
            Next
        Finally
            pgExistente.gridReceptii.EndUpdate()
        End Try
    End Sub

    ' The day the receptions are measured on: the date of the oldest ticked invoice, only for «Leagă» and only for a day before today; Nothing = the receptions as they are.
    Private Function ReceptionDay() As Date?
        If _mode <> EFacturaKeTip.Existenta OrElse _viewing IsNot Nothing Then Return Nothing
        Dim k_days As List(Of Date) = _checked.Where(Function(k_id) _invoices.ContainsKey(k_id) AndAlso _invoices(k_id).DataFact.HasValue).
            Select(Function(k_id) _invoices(k_id).DataFact.Value.Date).ToList()
        If k_days.Count = 0 Then Return Nothing
        Dim k_oldest As Date = k_days.Min()
        Return If(k_oldest >= Date.Today, Nothing, CType(k_oldest, Date?))
    End Function

    ' What the grid of receptions should be showing: the angajament and the day ("-" = as they are).
    Private Function CurrentRecKey() As String
        Dim k_day As Date? = ReceptionDay()
        Return _angCod & "|" & If(k_day.HasValue, k_day.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "-")
    End Function

    ' The receptions of the grid: as they are, or as they stood on the day; none while the answer for the day is on its way (so nothing is chosen on today's figures by mistake).
    Private Function ReceptiiList() As List(Of EFacturaKeReceptie)
        If _data Is Nothing Then Return New List(Of EFacturaKeReceptie)()
        Dim k_key As String = CurrentRecKey()
        If k_key.EndsWith("|-", StringComparison.Ordinal) Then Return _data.Receptii
        If _recAt IsNot Nothing AndAlso String.Equals(k_key, _recAtKey, StringComparison.OrdinalIgnoreCase) Then Return _recAt.Receptii
        Return New List(Of EFacturaKeReceptie)()
    End Function

    ' What was read for a day is forgotten when the invoices or their links may have changed.
    Private Sub ForgetReceptiiAt()
        Interlocked.Increment(_recAtSeq)
        _recAtCache.Clear()
        _recAt = Nothing
        _recAtKey = String.Empty
        _recShownKey = String.Empty
    End Sub

    ' UI boundary (async Sub): reads the receptions for the day when the grid needs them; logs and swallows, a failure becomes a notice.
    Private Async Sub BeginLoadReceptiiAt()
        Try
            Await LoadReceptiiAtAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeReceptiiView.BeginLoadReceptiiAt", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.BeginLoadReceptiiAt", ex)
            ShowNotice("Recepțiile de la data facturii nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Private Async Function LoadReceptiiAtAsync() As Task
        If _api Is Nothing OrElse _data Is Nothing OrElse String.IsNullOrWhiteSpace(_angCod) Then Return
        Dim k_day As Date? = ReceptionDay()
        If Not k_day.HasValue Then Return
        Dim k_key As String = CurrentRecKey()
        If _recAt IsNot Nothing AndAlso String.Equals(k_key, _recAtKey, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_mySeq As Integer = Interlocked.Increment(_recAtSeq)
        Dim k_answer As EFacturaKeReceptiiAngajament = Nothing
        If Not _recAtCache.TryGetValue(k_key, k_answer) Then
            Dim k_cod As String = _angCod
            Dim k_dayNow As Date = k_day.Value
            k_answer = Await _gate.RunAsync(Function() _api.GetKeReceptiiLaDataAsync(k_cod, k_dayNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _recAtSeq Then Return
            _recAtCache(k_key) = k_answer
        End If
        If IsDisposed OrElse k_mySeq <> _recAtSeq OrElse Not String.Equals(k_key, CurrentRecKey(), StringComparison.OrdinalIgnoreCase) Then Return
        _recAt = k_answer
        _recAtKey = k_key
        FillReceptii()
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
        BeginRefreshDispo()
    End Function

    Private Function ChosenReceptie() As EFacturaKeReceptie
        If _mode = EFacturaKeTip.Noua Then Return Nothing
        Return TryCast(pgExistente.gridReceptii.CurrentRow?.Tag, EFacturaKeReceptie)
    End Function

    Private Sub GridReceptii_SelectionChanged(sender As Object, e As EventArgs) Handles pgExistente.ReceptionSelectionChanged
        Try
            If _viewing IsNot Nothing OrElse _assocTab Then Return   ' slice 02EF-07: on «Asociate» a row is only looked at
            RecomputeDefaults()
            FillAlocari()
            UpdateTitles()
            BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.GridReceptii_SelectionChanged", ex)
        End Try
    End Sub

    Private Function IndicatorOf(k_idClsf As Integer) As EFacturaKeIndicator
        If _data Is Nothing Then Return Nothing
        Return _data.Indicatori.FirstOrDefault(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf)
    End Function

End Class
