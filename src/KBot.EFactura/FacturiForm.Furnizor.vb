Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-13 -- the unit as the issuer of the invoices. Its data (name, tax code, address, series...) and its accounts are edited in
' their own window, «Date Unitate» (DateUnitateForm), opened from the menu of the extra button of the title bar («Conturi Unitate» / «Date Unitate»); they are no longer a
' view of this window. What stays here: the issuer the window keeps (the invoice list is blocked without it and the PDF prints it) and
' the invoice's own payment account, the combo «Cont emitent» of the «Generale» view.
Partial Public Class FacturiForm

    ' Nothing = the unit has not filled its issuer data yet (no invoice can be made until it does).
    Private _furnizor As EFacturaFurnizor

    ' The unit's own IBANs (AVACONT_COMUN.Unitati_Conturi), offered first by the «Cont emitent» combo.
    Private _conturi As New List(Of EFacturaCont)()

    Private Sub SetFurnizor(k_furnizor As EFacturaFurnizor)
        _furnizor = k_furnizor
    End Sub

    ' The extra button of the title bar drops the menu of the unit's windows.
    Private Sub CapBar_OptionButtonClick(sender As Object, e As EventArgs) Handles capBar.OptionButtonClick
        Try
            If _busy OrElse _api Is Nothing Then Return
            Dim k_area As Rectangle = capBar.RectangleToScreen(capBar.OptionButtonBounds)
            mnuUnitate.ShowAt(capBar, New Point(k_area.Left, k_area.Bottom))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.CapBar_OptionButtonClick", ex)
        End Try
    End Sub

    Private Sub MnuUnitate_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles mnuUnitate.ItemClicked
        Try
            Select Case e.Item.Key
                Case "conturi"
                    OpenConturi()
                Case "date"
                    OpenDateUnitate()
                Case "primite"
                    OpenSincronizarePrimite()
                Case "listaprimite"
                    OpenPrimite()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.MnuUnitate_ItemClicked", ex)
        End Try
    End Sub

    ' Slice 00EF-19: the window with all the received invoices of the year; one at a time, asking again brings it forward.
    Private Sub OpenPrimite()
        If _api Is Nothing Then Return
        If _primiteForm IsNot Nothing AndAlso Not _primiteForm.IsDisposed Then
            _primiteForm.Activate()
            Return
        End If
        _primiteForm = New PrimiteForm(_api, _gate, _unitName, _year, _viewerFactory)
        AddHandler _primiteForm.FormClosed, Sub() _primiteForm = Nothing
        _primiteForm.Show(Me)
    End Sub

    ' Slice 00EF-18: brings the received invoices from ANAF (the dialog does the work, a batch at a time).
    Private Sub OpenSincronizarePrimite()
        If _busy OrElse _api Is Nothing Then Return
        Using k_dialog As New SincronizarePrimiteForm(_api, _gate)
            k_dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenConturi()
        If _busy OrElse _api Is Nothing Then Return
        Using k_dialog As New ConturiForm(_api, _gate, _unitName)
            k_dialog.ShowDialog(Me)
        End Using
        ' The list may have changed: the combo offers the new one.
        Dim k_task As Task = ReloadConturiAsync()
    End Sub

    ''' <summary>Reads the unit's own accounts again and refreshes the «Cont emitent» combo.</summary>
    Private Async Function ReloadConturiAsync() As Task
        Try
            Await LoadConturiAsync().ConfigureAwait(True)
            If Not IsDisposed Then FillContPlata()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.ReloadConturiAsync", ex)
        End Try
    End Function

    Private Async Function LoadConturiAsync() As Task
        Try
            Dim k_list As List(Of EFacturaCont) = Await _gate.RunAsync(
                Function() _api.GetConturiAsync(_cts.Token)).ConfigureAwait(True)
            If Not IsDisposed Then _conturi = k_list
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.LoadConturiAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Opens «Date Unitate» as a dialog; what it saved becomes the issuer this window keeps.</summary>
    Private Sub OpenDateUnitate()
        If _busy OrElse _api Is Nothing Then Return
        Using k_dialog As New DateUnitateForm(_api, _gate, _furnizor)
            k_dialog.ShowDialog(Me)
            If k_dialog.Furnizor IsNot Nothing Then SetFurnizor(k_dialog.Furnizor)
        End Using
        ApplyMode()
    End Sub

    ' ── The invoice's payment account ───────────────────────────────────────────

    ''' <summary>The accounts already used on the unit's invoices, newest first, offered by the «Cont emitent» combo.</summary>
    Private Sub FillContPlata()
        Dim k_was As Boolean = _loading
        _loading = True
        Try
            Dim k_text As String = cmbContPlata.Text
            cmbContPlata.Items.Clear()
            Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each k_c As EFacturaCont In _conturi
                Dim k_own As String = k_c.Cont.Trim()
                If k_own.Length > 0 AndAlso k_seen.Add(k_own) Then cmbContPlata.Items.Add(k_own)
            Next
            For Each k_f As EFacturaFactura In _facturi
                Dim k_account As String = k_f.ContPlata.Trim()
                If k_account.Length > 0 AndAlso k_seen.Add(k_account) Then cmbContPlata.Items.Add(k_account)
            Next
            cmbContPlata.Text = k_text
        Finally
            _loading = k_was
        End Try
    End Sub

    ''' <summary>The account of the newest invoice: a new invoice starts from it (the operator may change it).</summary>
    Private Function DefaultContPlata() As String
        If _conturi.Count = 1 Then Return _conturi(0).Cont.Trim()
        Dim k_last As EFacturaFactura = _facturi.FirstOrDefault(Function(x) x.ContPlata.Trim().Length > 0)
        Return If(k_last Is Nothing, String.Empty, k_last.ContPlata.Trim())
    End Function

End Class
