Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Slice 0087 -- the header menu (menuNou, opened by btnMeniu) and the two windows it opens besides
' «Angajament nou»: «Clasificatii bugetare» and «Parteneri». Both are standalone (modeless) windows,
' one of each at a time; asking again brings the open one to the front.
Partial Public Class KbotForm

    Private _clasificatiiForm As ClasificatiiForm
    Private _parteneriForm As ParteneriForm

    Private Sub MenuNou_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles menuNou.ItemClicked
        Try
            Select Case e.Key
                Case "angajament_nou"
                    DeschideAngajamentNou()
                Case "clasificatii"
                    DeschideClasificatiile()
                Case "parteneri"
                    DeschidePartenerii()
                Case UncorrelatedMenuKey
                    ' Slice 0088: every stored ERR operation no note covers yet.
                    OpenUncorrelatedFromMenu()
                Case Else
                    Throw New ArgumentException($"Unknown menu key '{e.Key}'.", NameOf(e))
            End Select
        Catch ex As Exception
            ' UI boundary (menu handler): log and swallow; each branch already told the operator.
            GlobalErrorLog.Write("MainForm.MenuNou_ItemClicked", ex)
        End Try
    End Sub

    ' The API and the re-login net the two windows share. Nothing (and a message) when no unit is open.
    Private Function NomenclatoareContext(caption As String, ByRef api As INomenclatoareApi,
                                          ByRef gate As ReauthGate) As Boolean
        If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
            KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                             caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        api = TryCast(_apiClient, INomenclatoareApi)
        If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement INomenclatoareApi.")
        gate = New ReauthGate(Function(action) WithReauth(Of Object)(action))
        Return True
    End Function

    Private Sub DeschideClasificatiile()
        Try
            If _clasificatiiForm IsNot Nothing AndAlso Not _clasificatiiForm.IsDisposed Then
                _clasificatiiForm.Activate()
                Return
            End If
            Dim api As INomenclatoareApi = Nothing
            Dim gate As ReauthGate = Nothing
            If Not NomenclatoareContext("Clasificații bugetare", api, gate) Then Return
            _clasificatiiForm = New ClasificatiiForm(api, gate, _session.An)
            AddHandler _clasificatiiForm.FormClosed, Sub() _clasificatiiForm = Nothing
            _clasificatiiForm.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideClasificatiile", ex)
            KBotMessage.Show(Me, "Fereastra clasificațiilor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Clasificații bugetare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DeschidePartenerii()
        Try
            If _parteneriForm IsNot Nothing AndAlso Not _parteneriForm.IsDisposed Then
                _parteneriForm.Activate()
                Return
            End If
            Dim api As INomenclatoareApi = Nothing
            Dim gate As ReauthGate = Nothing
            If Not NomenclatoareContext("Parteneri", api, gate) Then Return
            _parteneriForm = New ParteneriForm(api, gate, _session.SectorSursa)
            AddHandler _parteneriForm.FormClosed, Sub() _parteneriForm = Nothing
            _parteneriForm.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschidePartenerii", ex)
            KBotMessage.Show(Me, "Fereastra partenerilor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Parteneri", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
