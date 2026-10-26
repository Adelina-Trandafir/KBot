Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice AV01-09 -- the «Jurnale» window (the accounting journals of the unit). A small MODAL window of the main window (like «Grupe»):
' answered and gone, not a page and not kept in the history.
Partial Public Class KbotForm

    Friend Const MachetaJurnaleContabile As String = "jurnale"

    ''' <summary>Opens the modal journals window; returns (the code below resumes) when it closed. UI boundary: logs and shows.</summary>
    Friend Async Sub DeschideJurnale()
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "Jurnale", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IJurnaleApi = TryCast(_apiClient, IJurnaleApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IJurnaleApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            Using k_form As New JurnaleForm(k_api, k_gate, _session)
                Await ShowModalAsync(k_form, MachetaRef.Create(MachetaJurnaleContabile)).ConfigureAwait(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideJurnale", ex)
            KBotMessage.Show(Me, "Fereastra jurnalelor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Jurnale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
