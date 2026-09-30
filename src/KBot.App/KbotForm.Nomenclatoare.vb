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

    ' Slice 0095-02: set at the first collapse of navViews; from then on btnMeniu's column follows
    ' the bar's width (also through a DPI change). Before it the designer's width stands.
    Private _menuFollowsNav As Boolean

    ''' <summary>
    ''' The bar of views collapsed or expanded (slice 0095-02): the header's menu button takes the
    ''' same width, and a collapsed button shows only its icon.
    ''' </summary>
    Private Sub NavViews_CollapseStateChanged(state As KBotNavCollapseState) Handles navViews.CollapseStateChanged
        Try
            _menuFollowsNav = True
            ApplyMenuButtonText()
            SyncMenuButtonWidth()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("MainForm.NavViews_CollapseStateChanged", ex)
        End Try
    End Sub

    Private Sub NavViews_SizeChanged(sender As Object, e As EventArgs) Handles navViews.SizeChanged
        Try
            If _menuFollowsNav Then SyncMenuButtonWidth()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("MainForm.NavViews_SizeChanged", ex)
        End Try
    End Sub

    ' Column 0 of the header holds btnMeniu with its margins; the button's left edge already lines
    ' up with the bar's (cell border + left margin = pnlWork's left padding), so the column is the
    ' bar's width plus the two margins. The table takes LOGICAL pixels: device / its scale.
    Private Sub SyncMenuButtonWidth()
        If IsDisposed OrElse Not IsHandleCreated Then Return
        Dim scale As Single = tlyHeader.DpiScale
        If scale <= 0F Then Return
        Dim device As Integer = navViews.Width + btnMeniu.Margin.Horizontal
        tlyHeader.SetColumnWidth(0, device / scale)
    End Sub

    ' The menu button's caption: none while the bar is collapsed (only the icon fits; the tooltip
    ' still tells about the «(!)» mark), otherwise «Meniu» with or without the mark.
    Private Sub ApplyMenuButtonText()
        If navViews.CollapseState <> KBotNavCollapseState.Expanded Then
            btnMeniu.Text = String.Empty
        Else
            btnMeniu.Text = If(_menuMarked, MenuButtonMarkedText, MenuButtonText)
        End If
    End Sub

    Private Sub MenuNou_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles menuNou.ItemClicked
        Try
            Select Case e.Key
                Case "angajament_nou"
                    DeschideAngajamentNou()
                Case "extrase"
                    ' Slice 0095-02: the «Extrase de cont» window (KbotForm.Extrase.vb).
                    DeschideExtrasele()
                Case "clasificatii"
                    DeschideClasificatiile()
                Case "parteneri"
                    DeschidePartenerii()
                Case HelpCaptureMenuKey
                    ' Slice 0000-02: the help screenshot list (KbotForm.HelpCapture.vb).
                    DeschideCapturileAjutorului()
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
