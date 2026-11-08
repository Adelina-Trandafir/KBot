Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Theming

' Slice 0123-02 -- the VERCON tab of the ribbon: «Rapoarte» (key vercon_rapoarte) opens the page of the VERCON reports of the gestiune, a page of the main
' window like the ADECHIT and E-Factura ones; asking again brings the open one to the front.
Partial Public Class KbotForm

    Friend Const MachetaVerconRapoarte As String = "vercon_rapoarte"

    Private _verconRapoarte As VerconRapoarteForm

    ''' <summary>«VERCON Rapoarte»: the page of the gestiune's reports.</summary>
    Friend Sub DeschideVerconRapoarte()
        Try
            If _verconRapoarte IsNot Nothing AndAlso Not _verconRapoarte.IsDisposed Then
                ShowMachetaPage(_verconRapoarte, MachetaRef.Create(MachetaVerconRapoarte))   ' already the page: nothing changes
                Return
            End If
            If String.IsNullOrWhiteSpace(_session.DbName) Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "VERCON Rapoarte", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IVerconApi = TryCast(_apiClient, IVerconApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IVerconApi.")
            Dim k_gate As New ReauthGate(Function(action) WithReauth(Of Object)(action))
            _verconRapoarte = New VerconRapoarteForm(k_api, k_gate, _session.NumeUnitate)
            AddHandler _verconRapoarte.FormClosed, Sub() _verconRapoarte = Nothing
            If Not ShowMachetaPage(_verconRapoarte, MachetaRef.Create(MachetaVerconRapoarte)) Then
                _verconRapoarte.Dispose()   ' another page refused to close: stay where we are
                _verconRapoarte = Nothing
            End If
        Catch ex As Exception
            ' UI boundary (ribbon click): log and tell the operator.
            GlobalErrorLog.Write("KbotForm.DeschideVerconRapoarte", ex)
            KBotMessage.Show(Me, "Pagina de rapoarte VERCON nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "VERCON Rapoarte", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
