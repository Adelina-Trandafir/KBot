Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Theming

' Slice AV02-05 -- the «Modificare operatii» page (the Access form ModificareNC1280): a modeless page of the main window, one at a time; asking
' again brings the open one to the front. Same shape as «Plan de conturi» (KbotForm.PlanCont.vb). Opened from the ribbon tab «Introducere date»,
' group «Modificare Date».
Partial Public Class KbotForm

    Friend Const MachetaModificareOperatii As String = "modificare_operatii"

    Private _modificareOperatiiForm As ModificareOperatiiForm

    Friend Sub DeschideModificareOperatii()
        Try
            If _modificareOperatiiForm IsNot Nothing AndAlso Not _modificareOperatiiForm.IsDisposed Then
                ShowMachetaPage(_modificareOperatiiForm, MachetaRef.Create(MachetaModificareOperatii))
                Return
            End If
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 ModificareOperatiiForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IModificareOperatiiApi = TryCast(_apiClient, IModificareOperatiiApi)
            If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IModificareOperatiiApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            _modificareOperatiiForm = New ModificareOperatiiForm(k_api, k_gate, _session, Me)
            AddHandler _modificareOperatiiForm.FormClosed, Sub() _modificareOperatiiForm = Nothing
            If Not ShowMachetaPage(_modificareOperatiiForm, MachetaRef.Create(MachetaModificareOperatii)) Then
                _modificareOperatiiForm.Dispose()
                _modificareOperatiiForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideModificareOperatii", ex)
            KBotMessage.Show(Me, "Fereastra «Modificare operații» nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             ModificareOperatiiForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
