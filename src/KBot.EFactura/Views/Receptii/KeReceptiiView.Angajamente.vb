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

' SLICE-KE21-02: KeReceptiiView.Angajamente - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    Private Property _data As EFacturaKeReceptiiAngajament
        Get
            Return angajamente.CurrentData
        End Get
        Set(k_value As EFacturaKeReceptiiAngajament)
            angajamente.CurrentData = k_value
        End Set
    End Property

    Private Property _angCod As String
        Get
            Return angajamente.CurrentCod
        End Get
        Set(k_value As String)
            angajamente.CurrentCod = k_value
        End Set
    End Property

    ' The states of the ticked invoices (else the one pressed) go to the table, which keeps the angajament in use when it is still offered.
    Friend Sub FillAngajamente()
        If angajamente Is Nothing Then Return   ' the host has not given its table yet
        If _viewing IsNot Nothing Then
            angajamente.FillViewed(_viewState)
            Return
        End If
        Dim k_sources As New List(Of EFacturaKeStare)()
        For Each k_id As Integer In _checked
            Dim k_state As EFacturaKeStare = Nothing
            If _states.TryGetValue(k_id, k_state) Then k_sources.Add(k_state)
        Next
        angajamente.Fill(k_sources, _checked.Count > 0)
    End Sub

    ' UI boundary: the angajament in use is about to change; what was kept for the former one is forgotten.
    Private Sub Angajamente_AngajamentChanging(sender As Object, e As EventArgs) Handles angajamente.AngajamentChanging
        Try
            _dispo.Clear()
            _extra.Clear()
            _dispoKey = String.Empty
            ntfMesaj.Visible = False
            btnOk.Enabled = False
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Angajamente_AngajamentChanging", ex)
        End Try
    End Sub

    ' UI boundary: the data of the angajament in use are here: the receptions, the classifications, the amounts and the budget follow it.
    Private Sub Angajamente_AngajamentReady(sender As Object, e As EventArgs) Handles angajamente.AngajamentReady
        Try
            ApplyIndicators()
            ApplySingleIndicator(True)
            ReshapeInvoices()
            FillReceptii()
            BeginLoadReceptiiAt()
            RecomputeDefaults()
            FillAlocari()
            UpdateTitles()
            BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Angajamente_AngajamentReady", ex)
        End Try
    End Sub

    ' UI boundary: no angajament is in use any more.
    Private Sub Angajamente_AngajamentCleared(sender As Object, e As EventArgs) Handles angajamente.AngajamentCleared
        Try
            ClearAngajamentData()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Angajamente_AngajamentCleared", ex)
        End Try
    End Sub

    ' UI boundary: the indicators of the other angajamente of the table were read.
    Private Sub Angajamente_OthersLoaded(sender As Object, e As EventArgs) Handles angajamente.OthersLoaded
        Try
            ApplyIndicators()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Angajamente_OthersLoaded", ex)
        End Try
    End Sub

    ' UI boundary: an angajament was added or a link taken off.
    Private Sub Angajamente_RefillRequested(sender As Object, e As EventArgs) Handles angajamente.RefillRequested
        Try
            FillAngajamente()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Angajamente_RefillRequested", ex)
        End Try
    End Sub

    Private Function SupplierTaxCodes() As List(Of String) Implements IKeAngajamenteHost.SupplierTaxCodes
        Return ContextKeys().Select(Function(k_key) SupplierCui(k_key)).ToList()
    End Function

    Private Function RefreshContextAsync() As Task Implements IKeAngajamenteHost.RefreshContextAsync
        Return RefreshContextStatesAsync()
    End Function

    ' The manual links of the ticked invoices with a DDF of this angajament are removed; the state of each invoice (its candidate angajamente) is read again.
    Private Async Function RemoveManualLinksAsync(k_cod As String) As Task(Of Integer) Implements IKeAngajamenteHost.RemoveManualLinksAsync
        Dim k_removed As Integer = 0
        For Each k_id As Integer In _checked.ToList()
            Dim k_idNow As Integer = k_id
            Dim k_detail As EFacturaPrimitaDetaliu = Await _gate.RunAsync(Function() _api.GetPrimitaAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return k_removed
            For Each k_link As EFacturaPrimitaDdf In k_detail.LegaturiManuale.Where(
                    Function(k_l) String.Equals(k_l.CodAngajament, k_cod, StringComparison.OrdinalIgnoreCase)).ToList()
                Dim k_idDdf As Integer = k_link.IdDdf
                Await _gate.RunAsync(Function() _api.UnlinkPrimitaAsync(k_idNow, k_idDdf, _cts.Token)).ConfigureAwait(True)
                k_removed += 1
            Next
            _states(k_idNow) = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
        Next
        Return k_removed
    End Function

    Private Sub ClearAngajamentData()
        _byLabel.Clear()
        _labelOf.Clear()
        pgExistente.gridReceptii.ClearRows()
        pgAsociate.gridReceptii.ClearRows()
        gridLinii.Column(ColLClsf).ComboItems = New List(Of Object) From {NeAleasa}
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
    End Sub

End Class
