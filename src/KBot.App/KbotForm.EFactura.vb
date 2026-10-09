Option Strict On
Imports System.Collections.Generic
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Theming

' Slice 00EF-05 / 00EF-08 -- «Meniu → E-Factura»: opens the E-Factura window (KBot.EFactura). The project is used directly by
' the shell, in the same process: the open unit and the re-login net are handed over as plain arguments, nothing is
' copied. Since 00EF-08 the window is the list of issued invoices (FacturiForm); the ANAF token window of 00EF-05 is
' reached from its «Token ANAF» button. Modeless, one at a time; asking again brings the open one to the front.
Partial Public Class KbotForm

    Private _efacturaForm As FacturiForm

    ' Slice 00EF-21: the tab «E-Factura» of the angajament shell shows only when the angajament's DDF has received invoices. The last
    ' answer is kept for the angajament it was asked for.
    Private _eFacturaCod As String = String.Empty
    Private _eFacturaAre As Boolean
    Private _eFacturaSeq As Integer

    ''' <summary>
    ''' Asks the server whether the DDF of <paramref name="k_cod"/> has received invoices and shows or hides the tab accordingly. UI boundary
    ''' (async Sub, started by the view gating): logs and swallows; a failure leaves the tab as it was. A newer question cancels the older answer.
    ''' </summary>
    Private Async Sub VerificaFacturiPrimite(k_cod As String)
        Try
            Dim k_api As IEFacturaApi = TryCast(_apiClient, IEFacturaApi)
            If k_api Is Nothing OrElse String.IsNullOrEmpty(k_cod) Then Return
            Dim k_mySeq As Integer = Threading.Interlocked.Increment(_eFacturaSeq)
            Dim k_ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(k_cod, Threading.CancellationToken.None)).ConfigureAwait(True)
            Dim k_antet As DdfAntet = k_ddf.AntetDeLucru(0)
            Dim k_are As Boolean
            If k_antet IsNot Nothing Then
                Dim k_list As List(Of EFacturaPrimita) = Await WithReauth(Of List(Of EFacturaPrimita))(
                    Function() k_api.GetPrimiteAsync(Nothing, Nothing, Nothing, k_antet.Iddf, Threading.CancellationToken.None)).ConfigureAwait(True)
                k_are = k_list.Count > 0
            End If
            If IsDisposed OrElse k_mySeq <> _eFacturaSeq Then Return
            If _currentInfo Is Nothing OrElse Not String.Equals(_currentInfo.CodAngajament, k_cod, StringComparison.Ordinal) Then Return
            _eFacturaCod = k_cod
            _eFacturaAre = k_are
            navViews.SetItemVisible("efactura", k_are)
            ' The operator is on the tab and it has just gone: back to «Sumar», like any view that closes.
            If Not k_are AndAlso navViews.SelectedKey = "efactura" Then navViews.SelectedKey = "sumar"
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.VerificaFacturiPrimite", ex)
        End Try
    End Sub

    Private Sub DeschideEFactura()
        Try
            If _efacturaForm IsNot Nothing AndAlso Not _efacturaForm.IsDisposed Then
                _efacturaForm.Activate()
                Return
            End If
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IEFacturaApi = TryCast(_apiClient, IEFacturaApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IEFacturaApi.")
            Dim k_gate As New ReauthGate(Function(action) WithReauth(Of Object)(action))
            _efacturaForm = New FacturiForm(k_api, k_gate, New TokenAuthorizer(k_api, k_gate), _session.NumeUnitate,
                                            _session.An, Function() New FacturaPdfViewer())
            AddHandler _efacturaForm.FormClosed, Sub() _efacturaForm = Nothing
            _efacturaForm.Show(Me)
        Catch ex As Exception
            ' UI boundary (menu handler): log and tell the operator.
            GlobalErrorLog.Write("MainForm.DeschideEFactura", ex)
            KBotMessage.Show(Me, "Fereastra E-Factura nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
