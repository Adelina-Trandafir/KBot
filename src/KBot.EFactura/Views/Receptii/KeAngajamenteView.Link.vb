Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-KE21-01: adding an angajament to the table and taking its association off.
Partial Public Class KeAngajamenteView

    ' UI boundary (async Sub): finds the angajament the text names (an item of the list, a code, else the server's search) and adds it.
    Private Async Sub BeginAddAngajament(k_text As String)
        Try
            Dim k_found As EFacturaKeAngajamentAlegere = Nothing
            If Not _angByLabel.TryGetValue(k_text, k_found) Then
                k_found = _allAng.FirstOrDefault(Function(k_a) String.Equals(k_a.CodAngajament, k_text, StringComparison.OrdinalIgnoreCase))
            End If
            If k_found Is Nothing Then
                Dim k_list As List(Of EFacturaKeAngajamentAlegere) = Await _gate.RunAsync(
                    Function() _api.GetKeAngajamenteAsync(k_text, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_list.Count = 1 Then
                    k_found = k_list(0)
                ElseIf k_list.Count = 0 Then
                    _host.ShowNotice($"Niciun angajament nu se potrivește cu «{k_text}».", NoticeKind.Warning)
                Else
                    _host.ShowNotice($"{k_list.Count} angajamente se potrivesc cu «{k_text}»: scrieți mai mult din cod sau din descriere.", NoticeKind.Warning)
                End If
            End If
            If k_found IsNot Nothing Then
                If Not _items.Any(Function(k_i) String.Equals(k_i.Cod, k_found.CodAngajament, StringComparison.OrdinalIgnoreCase)) Then
                    ' Slice 0121-05: the pick is saved for the SUPPLIER (its tax code); it comes back from the server as a candidate.
                    Dim k_saved As Boolean
                    For Each k_cui As String In _host.SupplierTaxCodes()
                        If k_cui.Length = 0 Then
                            _host.ShowNotice("Furnizorul nu are cod fiscal pe factură: angajamentul nu se poate lega de el.", NoticeKind.Warning)
                            Continue For
                        End If
                        Dim k_cuiNow As String = k_cui
                        Dim k_codNow As String = k_found.CodAngajament
                        Await _gate.RunAsync(Function() _api.LinkSupplierAngajamentAsync(k_cuiNow, k_codNow, _cts.Token)).ConfigureAwait(True)
                        If IsDisposed Then Return
                        k_saved = True
                    Next
                    If k_saved Then Await _host.RefreshContextAsync().ConfigureAwait(True)
                    If IsDisposed Then Return
                End If
                _angCod = k_found.CodAngajament
            End If
            RaiseEvent RefillRequested(Me, EventArgs.Empty)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeAngajamenteView.BeginAddAngajament", ex)
            _host.ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.BeginAddAngajament", ex)
            _host.ShowNotice("Angajamentul nu a putut fi căutat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' UI boundary (async Sub). An angajament added here is simply dropped. One that comes from the server is an association of the invoices: the manual
    ' links the operator made with a DDF of that angajament are removed; one that comes from the supplier being a partner of the DDF, or from links with
    ' receptions, is not made here and cannot be removed here.
    Private Async Sub BeginRemoveAssociation(k_item As AngItem)
        Try
            If String.Equals(k_item.Sursa, "furnizor", StringComparison.Ordinal) Then
                ' Slice 0121-05: an angajament picked for the supplier is the supplier's link (with its indicators), removed for the suppliers of the invoices shown.
                If KBotMessage.Show(FindForm(), $"Scoateți legătura furnizorului cu angajamentul {k_item.Cod}? Dispar și indicatorii legați de ea. Recepțiile din FOREXE nu se ating.",
                                    "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
                For Each k_cui As String In _host.SupplierTaxCodes()
                    If k_cui.Length = 0 Then Continue For
                    Dim k_cuiNow As String = k_cui
                    Dim k_codNow As String = k_item.Cod
                    Await _gate.RunAsync(Function() _api.UnlinkSupplierAngajamentAsync(k_cuiNow, k_codNow, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                Next
                Await _host.RefreshContextAsync().ConfigureAwait(True)
                If IsDisposed Then Return
                RaiseEvent RefillRequested(Me, EventArgs.Empty)
                Return
            End If
            If KBotMessage.Show(FindForm(), $"Scoateți asocierea facturilor bifate cu angajamentul {k_item.Cod}? Recepțiile din FOREXE nu se ating.",
                                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim k_removed As Integer = Await _host.RemoveManualLinksAsync(k_item.Cod).ConfigureAwait(True)
            If IsDisposed Then Return
            If k_removed = 0 Then
                KBotMessage.Show(FindForm(), $"Asocierea cu {k_item.Cod} nu a fost făcută de dumneavoastră: vine din DDF-ul la care furnizorul este partener sau din legături cu recepții. " &
                                 "Se scoate din DDF, respectiv din vederea «Recepții» a facturii.", "Recepții", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            RaiseEvent RefillRequested(Me, EventArgs.Empty)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeAngajamenteView.BeginRemoveAssociation", ex)
            _host.ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.BeginRemoveAssociation", ex)
            _host.ShowNotice("Asocierea nu a putut fi scoasă. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

End Class
