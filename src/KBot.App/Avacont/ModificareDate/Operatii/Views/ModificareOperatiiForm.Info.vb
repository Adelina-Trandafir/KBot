Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

' Slice AV02-05 - «Modificare operatii»: the info panel under the list (the Access popup MNotaC11, «Informatii suplimentare»). It follows the selected
' row; shown / hidden from the ribbon, the choice remembered per user (the Access setting MNC_C6).
Partial Public Class ModificareOperatiiForm

    ' Bumped by every read of the panel: an answer that finds a newer read started is dropped.
    Private _infoSeq As Integer

    Private ReadOnly Property InfoVisible As Boolean
        Get
            Return tlyInfo.Visible
        End Get
    End Property

    Private Sub SetInfoVisible(k_visible As Boolean, k_save As Boolean)
        tlyInfo.Visible = k_visible
        If k_save Then
            AppSettings.Current.ModificareOperatiiInfo = k_visible
            AppSettings.Current.Save()
        End If
        RefreshButtons()
        If k_visible Then LoadInfo()
    End Sub

    Private Sub ToggleInfo()
        Try
            SetInfoVisible(Not InfoVisible, k_save:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.ToggleInfo", ex)
        End Try
    End Sub

    ' UI boundary: started without await.
    Private Async Sub LoadInfo()
        Dim k_seq As Integer = Interlocked.Increment(_infoSeq)
        Try
            If Not InfoVisible OrElse _api Is Nothing Then Return
            Dim k_op As OperatieContabila = SelectedOperatie()
            If k_op Is Nothing Then
                ShowInfo(Nothing, Nothing)
                Return
            End If
            Dim k_info As OperatieInfo = Await _gate.RunAsync(
                Function() _api.GetOperatieInfoAsync(k_op.IdOper, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _infoSeq Then Return
            ShowInfo(k_op, k_info)
        Catch ex As ApiException
            GlobalErrorLog.Write("ModificareOperatiiForm.LoadInfo", ex)
            If Not IsDisposed AndAlso k_seq = _infoSeq Then lblInfo.Text = ex.Message
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.LoadInfo", ex)
            If Not IsDisposed AndAlso k_seq = _infoSeq Then lblInfo.Text = "Informațiile operației nu au putut fi citite. Detalii în jurnalul de erori."
        End Try
    End Sub

    Private Shared Function Ro(k_date As Date?) As String
        Return If(k_date.HasValue, k_date.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
    End Function

    Private Shared Function SoldText(k_sold As OperatieSold) As String
        Dim k_value As Decimal = k_sold.Sold
        If k_value = 0D Then Return "0,00"
        Return Math.Abs(k_value).ToString("N2", CultureInfo.GetCultureInfo("ro-RO")) & If(k_value > 0D, " D", " C")
    End Function

    Private Sub ShowInfo(k_op As OperatieContabila, k_info As OperatieInfo)
        gridFacturi.BeginUpdate()
        gridSold.BeginUpdate()
        Try
            gridFacturi.ClearRows()
            gridSold.ClearRows()
            If k_op Is Nothing OrElse k_info Is Nothing Then
                lblInfo.Text = "Alegeți o operație din listă."
                Return
            End If

            Dim k_text As New StringBuilder()
            k_text.AppendLine($"Tip operație: {TipText(k_info.Fel)}")
            k_text.AppendLine($"Document: {k_op.NumarDocument} din {Ro(k_op.DataDocument)} · nota contabilă {k_op.NumarNC.ToString(CultureInfo.InvariantCulture)}")
            If Not String.IsNullOrEmpty(k_info.Factura) Then k_text.AppendLine($"Factură plătită: {k_info.Factura}")
            If k_info.NumarOP.HasValue Then
                k_text.AppendLine($"Ordin de plată: {k_info.NumarOP.Value.ToString(CultureInfo.InvariantCulture)} din {Ro(k_info.DataOP)}")
            End If
            If Not String.IsNullOrEmpty(k_info.DenumirePartener) OrElse Not String.IsNullOrEmpty(k_info.CodPartener) Then
                k_text.AppendLine($"Partener: {k_info.CodPartener} — {k_info.DenumirePartener}")
            End If
            If k_op.Ord <> OperatieStareOrd.Niciuna Then k_text.AppendLine($"Ordonanțare: {OrdText(k_op.Ord)}")
            If k_op.Receptie <> OperatieStareReceptie.Niciuna Then k_text.AppendLine($"Recepție: {ReceptieText(k_op.Receptie)}")
            If Not String.IsNullOrEmpty(k_op.Refuz) Then k_text.AppendLine($"Nu se poate șterge: {k_op.Refuz}")
            lblInfo.Text = k_text.ToString().TrimEnd()

            For Each k_f As OperatieFacturaPlatita In k_info.Facturi
                Dim k_row As KBotDataRow = gridFacturi.AddRow()
                k_row("numar") = k_f.Numar
                k_row("data") = If(k_f.Data.HasValue, CObj(k_f.Data.Value), Nothing)
                k_row("platit") = k_f.Platit
            Next
            AddSold($"La {Ro(k_info.DataDocument)}", k_info.SoldLaData)
            AddSold("Anual", k_info.SoldAnual)
        Finally
            gridFacturi.EndUpdate()
            gridSold.EndUpdate()
        End Try
    End Sub

    Private Sub AddSold(k_label As String, k_sold As OperatieSold)
        If k_sold Is Nothing Then Return
        Dim k_row As KBotDataRow = gridSold.AddRow()
        k_row("eticheta") = k_label
        k_row("debit") = k_sold.Debit
        k_row("credit") = k_sold.Credit
        k_row("sold") = SoldText(k_sold)
    End Sub

End Class
