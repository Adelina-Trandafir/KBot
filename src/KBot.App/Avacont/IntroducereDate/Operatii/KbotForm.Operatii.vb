Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slices AV02-03 / AV02-04 -- the «OperatiiSabloane» window (the pages «Operatii diverse» and «Facturi si plati»). Like «Sabloane», a MODAL window of the main window:
' answered and gone, not a page and not kept in the history. It follows the year and the subunit of the title bar as they were when it opened.
Partial Public Class KbotForm

    Friend Const MachetaOperatiiSabloane As String = "operatii_sabloane"

    ''' <summary>Opens the modal window of the accounting operations; returns (the code below resumes) when it closed. UI boundary: logs and shows.</summary>
    Friend Async Sub DeschideOperatiiSabloane()
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "Operații contabile", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IOperatiiApi = TryCast(_apiClient, IOperatiiApi)
            Dim k_sablonApi As ISablonOperatiiApi = TryCast(_apiClient, ISablonOperatiiApi)
            If k_api Is Nothing OrElse k_sablonApi Is Nothing Then Throw New InvalidOperationException("The API client does not implement IOperatiiApi / ISablonOperatiiApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            Using k_form As New OperatiiSabloaneForm(k_api, k_sablonApi, k_gate, _session, CurrentSubunitId, CurrentSubunitSs)
                Await ShowModalAsync(k_form, MachetaRef.Create(MachetaOperatiiSabloane)).ConfigureAwait(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideOperatiiSabloane", ex)
            KBotMessage.Show(Me, "Fereastra operațiilor contabile nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Operații contabile", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
