Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.EFactura
Imports KBot.Theming

' Slice 00EF-05 / 00EF-08 -- «Meniu → E-Factura»: opens the E-Factura window (KBot.EFactura). The project is used directly by
' the shell, in the same process: the open unit and the re-login net are handed over as plain arguments, nothing is
' copied. Since 00EF-08 the window is the list of issued invoices (FacturiForm); the ANAF token window of 00EF-05 is
' reached from its «Token ANAF» button. Modeless, one at a time; asking again brings the open one to the front.
Partial Public Class KbotForm

    Private _efacturaForm As FacturiForm

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
                                            Function() New FacturaPdfViewer())
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
