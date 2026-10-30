Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice AV01-14 -- the «Sabloane» window (the operation templates, type M and type A, and the salary templates). Since AV01-16 a MODAL window of the main
' window, like «Jurnale»: answered and gone, not a page and not kept in the history. It follows the sector+source of the title bar as it was when it opened.
Partial Public Class KbotForm

    ''' <summary>Opens the modal templates window; returns (the code below resumes) when it closed. UI boundary: logs and shows.</summary>
    Friend Async Sub DeschideSabloane()
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "Șabloane", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As ISablonOperatiiApi = TryCast(_apiClient, ISablonOperatiiApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement ISablonOperatiiApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            Using k_form As New SabloaneForm(k_api, TryCast(_apiClient, ISablonSalariiApi), k_gate, _session)
                Await ShowModalAsync(k_form, MachetaRef.Create(MachetaSabloane)).ConfigureAwait(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideSabloane", ex)
            KBotMessage.Show(Me, "Fereastra șabloanelor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Șabloane", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
