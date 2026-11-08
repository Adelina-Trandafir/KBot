Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Theming

' Slice 0123-01 -- the ADECHIT tab of the ribbon: «Rapoarte» (key adechit_rapoarte) opens the page of the six reports of the evidence, a page of the main
' window like the E-Factura ones; asking again brings the open one to the front.
Partial Public Class KbotForm

    Friend Const MachetaAdechitRapoarte As String = "adechit_rapoarte"

    Private _adechitRapoarte As AdechitRapoarteForm

    ''' <summary>«ADECHIT Rapoarte»: the page of the evidence's reports.</summary>
    Friend Sub DeschideAdechitRapoarte()
        Try
            If _adechitRapoarte IsNot Nothing AndAlso Not _adechitRapoarte.IsDisposed Then
                ShowMachetaPage(_adechitRapoarte, MachetaRef.Create(MachetaAdechitRapoarte))   ' already the page: nothing changes
                Return
            End If
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "ADECHIT Rapoarte", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IAdechitApi = TryCast(_apiClient, IAdechitApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IAdechitApi.")
            Dim k_gate As New ReauthGate(Function(action) WithReauth(Of Object)(action))
            _adechitRapoarte = New AdechitRapoarteForm(k_api, k_gate, _session.NumeUnitate)
            AddHandler _adechitRapoarte.FormClosed, Sub() _adechitRapoarte = Nothing
            If Not ShowMachetaPage(_adechitRapoarte, MachetaRef.Create(MachetaAdechitRapoarte)) Then
                _adechitRapoarte.Dispose()   ' another page refused to close: stay where we are
                _adechitRapoarte = Nothing
            End If
        Catch ex As Exception
            ' UI boundary (ribbon click): log and tell the operator.
            GlobalErrorLog.Write("KbotForm.DeschideAdechitRapoarte", ex)
            KBotMessage.Show(Me, "Pagina de rapoarte ADECHIT nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "ADECHIT Rapoarte", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
