Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice AV01-04 -- the «Plan de conturi» window (subunits + the chart of accounts of one subunit). A modeless page of the
' main window, one at a time; asking again brings the open one to the front. Same shape as «Parteneri»
' (KbotForm.Nomenclatoare.vb).
Partial Public Class KbotForm
    Implements IPlanContHost

    Private _planContForm As PlanContForm

    ' ── The subunit selector of the title bar (slice AV01-06) ───────────────────

    Public Event SubunitPicked As Action(Of String) Implements IPlanContHost.SubunitPicked

    Public Sub SetSubunitChoices(k_items As IEnumerable(Of KeyValuePair(Of String, String)), k_selectedKey As String) Implements IPlanContHost.SetSubunitChoices
        Try
            capBar.SetSelectorItems(KBotCaptionBar.SelectorSubunit, k_items, k_selectedKey)
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.SetSubunitChoices", ex)
            Throw
        End Try
    End Sub

    Public Sub SetSubunitKey(k_key As String) Implements IPlanContHost.SetSubunitKey
        capBar.SetSelectorKey(KBotCaptionBar.SelectorSubunit, k_key)
    End Sub

    Public Sub SetSubunitShown(k_shown As Boolean) Implements IPlanContHost.SetSubunitShown
        capBar.SetSelectorShown(KBotCaptionBar.SelectorSubunit, k_shown)
    End Sub

    ' The operator picked a subunit in the title bar (called from CapBar_PeriodChanged); the window decides.
    Private Sub OnSubunitPicked(k_key As String)
        RaiseEvent SubunitPicked(k_key)
    End Sub

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
