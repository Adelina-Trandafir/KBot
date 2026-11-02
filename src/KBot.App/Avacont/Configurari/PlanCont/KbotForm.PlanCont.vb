Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice AV01-04 -- the «Plan de conturi» window (subunits + the chart of accounts of one subunit). A modeless page of the
' main window, one at a time; asking again brings the open one to the front. Same shape as «Parteneri»
' (KbotForm.Nomenclatoare.vb).
Partial Public Class KbotForm
    Implements IPlanContHost, Global.KBot.EFactura.ISubunitHost

    Private _planContForm As PlanContForm

    ' The subunit selector of the title bar is the application's (KbotForm.Subunit.vb): this window reads the subunit in force and hears the picks.

    Friend Sub DeschidePlanCont()
        Try
            If _planContForm IsNot Nothing AndAlso Not _planContForm.IsDisposed Then
                ShowMachetaPage(_planContForm, MachetaRef.Create(MachetaPlanCont))
                Return
            End If
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "Plan de conturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IConturiApi = TryCast(_apiClient, IConturiApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IConturiApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            _planContForm = New PlanContForm(k_api, k_gate, _session, Me)
            AddHandler _planContForm.FormClosed, Sub() _planContForm = Nothing
            If Not ShowMachetaPage(_planContForm, MachetaRef.Create(MachetaPlanCont)) Then
                _planContForm.Dispose()
                _planContForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschidePlanCont", ex)
            KBotMessage.Show(Me, "Fereastra planului de conturi nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Plan de conturi", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
