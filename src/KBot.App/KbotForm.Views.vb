Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The left navigation of the shell (slice 0086 split out of KbotForm.vb): lazy creation of
''' the views, switching between them, and the Are* gate that decides which entries show.
''' </summary>
Partial Public Class KbotForm

    Private Sub NavViews_SelectionChanged(key As String) Handles navViews.SelectionChanged
        Try
            ActivateView(key)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.navViews_SelectionChanged", ex)
        End Try
    End Sub

    ' Creates the view on first activation (lazy), shows it and pushes the current context to it.
    Private Sub ActivateView(key As String)
        Try
            Dim view As IAngajamentView = Nothing
            If Not _views.TryGetValue(key, view) Then
                view = CreateView(key)
                Dim ctrl As System.Windows.Forms.Control = DirectCast(view, System.Windows.Forms.Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                viewHost.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _views(key) = view
            End If

            Dim previous As IAngajamentView = _activeView
            _activeView = view
            DirectCast(view, System.Windows.Forms.Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, view) Then
                DirectCast(previous, System.Windows.Forms.Control).Visible = False
            End If
            ' Only the ACTIVE view gets the context; the others get it when activated.
            view.SetContext(_currentInfo)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ActivateView", ex)
            Throw
        End Try
    End Sub

    Private Function CreateView(key As String) As IAngajamentView
        Try
            Select Case key
                ' The first real view (slice 0011). It gets the API client + the shell's 401
                ' net, so the re-login policy stays in one place.
                Case "sumar" : Return New SumarView(_apiClient, Function(op) WithReauth(Of SumarInfo)(op))
                Case "indicatori" : Return New PlaceholderView(key, "Indicatori")
                Case "istoric" : Return New IstoricView(_apiClient, Function(op) WithReauth(Of IstoricInfo)(op))
                Case "revizii" : Return New PlaceholderView(key, "Revizii")
                ' The "+" icon on the reservations tree asks for a DDF on that reservation --
                ' Access: fxRezervari_AdaugaRevizie in frmFX_MAIN. It goes through the SAME
                ' ExecutaComandaDdf as DdfView: one re-login policy, one place where the editor
                ' opens.
                ' Slice 0081-02: the footer LEFT icon (the DDF sending actions) reads the DDF
                ' through the same 401 net and hands the chosen option back here.
                Case "rezervari" : Return New RezervariView(_apiClient, Function(op) WithReauth(Of RezervariInfo)(op),
                                                            AddressOf ExecutaComandaDdf,
                                                            AddressOf ReimprospateazaRezervari,
                                                            Function(c) WithReauth(Of DdfInfo)(
                                                                Function() _apiClient.GetDdfAsync(c, CancellationToken.None)),
                                                            AddressOf ExecutaMeniulRezervari)
                Case "partener" : Return New PlaceholderView(key, "Partener")
                Case "receptii" : Return New ReceptiiView(_apiClient, Function(op) WithReauth(Of ReceptiiInfo)(op),
                                                         AddressOf DeschideLegaturileReceptiilor,
                                                         AddressOf ReimprospateazaReceptii,
                                                         AddressOf RebuildReceptiiFromIstoric)
                ' The "+" on the payments tree asks for the day's ordonantare / the month's
                ' batch -- Access: fxPlati_AdaugareOrdonantare / fxPlati_AdaugareOrdonantari in
                ' frmFX_MAIN. It goes through the SAME ExecutaComandaOrd as OrdView: one re-login
                ' policy, one place where the editor opens.
                Case "plati" : Return New PlatiView(_apiClient, Function(op) WithReauth(Of PlatiInfo)(op),
                                                    AddressOf ExecutaComandaOrd)
                ' Slice 0080-02: the angajament's bank statements. Its footer icon downloads them
                ' through the SAME DescarcaExtraseAsync as the «Extrase de cont» window.
                Case "extrase" : Return New ExtraseView(_apiClient, Function(op) WithReauth(Of ExtraseInfo)(op),
                                                        Function() DescarcaExtraseAsync(Me))
                Case "ddf" : Return New DdfView(_apiClient, Function(op) WithReauth(Of DdfInfo)(op), _session,
                                                AddressOf ExecutaComandaDdf)
                Case "ord" : Return New OrdView(_apiClient, Function(op) WithReauth(Of OrdInfo)(op), _session,
                                                AddressOf ExecutaComandaOrd)
                ' The live FOREXE page, docked into the shell (slice 0074). Only the
                ' coordinator: the view docks, opens angajamente and follows the page
                ' through it, and the shell keeps the reference for page-to-tree selection.
                Case "browser" : Return CreeazaVedereaBrowser()
                Case Else
                    Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CreateView", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The view gate: every Are* flag drives exactly one navigation entry.
    ''' With no node selected (info = Nothing) only «sumar» stays enabled.
    ''' A FALSE flag HIDES the entry (SetItemVisible), it does not merely disable it -- a
    ''' hidden entry takes no space, is not painted, cannot be selected and is skipped by
    ''' keyboard navigation.
    ''' </summary>
    Private Sub ApplyViewGating(info As AngajamentTreeInfo)
        Try
            'navViews.SetItemVisible("indicatori", info IsNot Nothing AndAlso info.AreIndicatori)
            navViews.SetItemVisible("istoric", info IsNot Nothing AndAlso info.AreIstoric)
            'navViews.SetItemVisible("revizii", info IsNot Nothing AndAlso info.AreRevizii)
            navViews.SetItemVisible("rezervari", info IsNot Nothing AndAlso info.AreRezervari)
            'navViews.SetItemVisible("partener", info IsNot Nothing AndAlso info.ArePartener)
            navViews.SetItemVisible("receptii", info IsNot Nothing AndAlso info.AreReceptii)
            navViews.SetItemVisible("plati", info IsNot Nothing AndAlso info.ArePlati)
            navViews.SetItemVisible("extrase", info IsNot Nothing AndAlso info.AreExtrase)
            navViews.SetItemVisible("ddf", info IsNot Nothing AndAlso info.AreDDF)
            navViews.SetItemVisible("ord", info IsNot Nothing AndAlso info.AreORD)
            ' «Browser FOREXE» (slice 0074) hangs on the SESSION, not on the node: with no
            ' selection the operator can still browse; without a session there is no page.
            navViews.SetItemVisible("browser", BrowserDisponibil())

            ' If the active view has just closed, fall back to «sumar» (always enabled) so the
            ' shell does not stay on a page the operator can no longer leave.
            If Not IsViewEnabled(navViews.SelectedKey, info) Then
                navViews.SelectedKey = "sumar"
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplyViewGating", ex)
            Throw
        End Try
    End Sub

    ' True when the view is allowed to be active for the given context.
    Private Shared Function IsViewEnabled(key As String, info As AngajamentTreeInfo) As Boolean
        If String.IsNullOrEmpty(key) OrElse key = "sumar" OrElse key = "browser" Then Return True
        If info Is Nothing Then Return False
        Select Case key
            'Case "indicatori" : Return info.AreIndicatori
            Case "istoric" : Return info.AreIstoric
            'Case "revizii" : Return info.AreRevizii
            Case "rezervari" : Return info.AreRezervari
            'Case "partener" : Return info.ArePartener
            Case "receptii" : Return info.AreReceptii
            Case "plati" : Return info.ArePlati
            Case "extrase" : Return info.AreExtrase
            Case "ddf" : Return info.AreDDF
            Case "ord" : Return info.AreORD
            ' Gated by the session in ApplyViewGating / the coordinator's StateChanged
            ' (KbotForm.Browser.vb), never by the node.
            Case "browser" : Return True
            Case Else
                Throw New ArgumentException($"Vedere necunoscută: '{key}'.", NameOf(key))
        End Select
    End Function
End Class
