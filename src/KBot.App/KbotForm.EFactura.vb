Option Strict On
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Theming

' Slice 00EF-05 / 00EF-08 -- E-Factura (KBot.EFactura), used directly by the shell, in the same process: the open
' unit and the re-login net are handed over as plain arguments, nothing is copied. Since the opening page it is a tab
' of the ribbon with three machete / groups of commands:
'   - «E-Factura Vanzare»  (key efactura_vanzare): the invoices the unit issues (FacturiForm);
'   - «E-Factura Cumparare» (key efactura_cumparare): the invoices received from ANAF (PrimiteForm);
'   - the unit's commands that were in the title bar of the issued-invoices window (accounts, unit data), the
'     received-invoices synchronisation (slice 0114-01 / 00EF-30: a ribbon button and two inputs, shown while the «Cumparare» page is on
'     screen; no dialog) and the ANAF token step.
' Each machete is a page of the main window, one at a time; asking again brings the open one to the front.
Partial Public Class KbotForm

    Friend Const MachetaEFacturaVanzare As String = "efactura_vanzare"
    Friend Const MachetaEFacturaCumparare As String = "efactura_cumparare"
    Friend Const MachetaEFacturaReceptii As String = "efactura_receptii"

    Private _efacturaForm As FacturiForm
    Private _primiteForm As PrimiteForm
    Private _receptiiForm As KeLegaForm

    ' The API and the re-login net of E-Factura; False (after telling the operator) when no unit is open.
    Private Function EFacturaContext(k_caption As String, ByRef k_api As IEFacturaApi, ByRef k_gate As ReauthGate) As Boolean
        If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
            KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                             k_caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        k_api = TryCast(_apiClient, IEFacturaApi)
        If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IEFacturaApi.")
        k_gate = New ReauthGate(Function(action) WithReauth(Of Object)(action))
        Return True
    End Function

    ''' <summary>«E-Factura Vanzare»: the invoices the unit issues.</summary>
    Private Sub DeschideEFactura()
        Try
            If _efacturaForm IsNot Nothing AndAlso Not _efacturaForm.IsDisposed Then
                ShowMachetaPage(_efacturaForm, MachetaRef.Create(MachetaEFacturaVanzare))   ' already the page: nothing changes
                Return
            End If
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("E-Factura Vânzare", k_api, k_gate) Then Return
            _efacturaForm = New FacturiForm(k_api, k_gate, New TokenAuthorizer(k_api, k_gate), _session.NumeUnitate,
                                            _session.An, Function() New FacturaPdfViewer())
            AddHandler _efacturaForm.FormClosed, Sub() _efacturaForm = Nothing
            If Not ShowMachetaPage(_efacturaForm, MachetaRef.Create(MachetaEFacturaVanzare)) Then
                _efacturaForm.Dispose()   ' another page refused to close: stay where we are
                _efacturaForm = Nothing
            End If
        Catch ex As Exception
            ' UI boundary (menu handler): log and tell the operator.
            GlobalErrorLog.Write("MainForm.DeschideEFactura", ex)
            KBotMessage.Show(Me, "Fereastra E-Factura nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>«E-Factura Cumparare»: the invoices received from ANAF, all the year's.</summary>
    Private Sub DeschideEFacturaCumparare()
        Try
            If _primiteForm IsNot Nothing AndAlso Not _primiteForm.IsDisposed Then
                ShowMachetaPage(_primiteForm, MachetaRef.Create(MachetaEFacturaCumparare))
                Return
            End If
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("E-Factura Cumpărare", k_api, k_gate) Then Return
            _primiteForm = New PrimiteForm(k_api, k_gate, _session.NumeUnitate, _session.An, Function() New FacturaPdfViewer(True))
            AddHandler _primiteForm.FormClosed, Sub() _primiteForm = Nothing
            If Not ShowMachetaPage(_primiteForm, MachetaRef.Create(MachetaEFacturaCumparare)) Then
                _primiteForm.Dispose()
                _primiteForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideEFacturaCumparare", ex)
            KBotMessage.Show(Me, "Fereastra facturilor primite nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura Cumpărare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Slice KE12, «E-Factura Receptii»: the page where received invoices are linked to receptions (existing, new, modified).</summary>
    Private Sub DeschideEFacturaReceptii()
        Try
            If _receptiiForm IsNot Nothing AndAlso Not _receptiiForm.IsDisposed Then
                ShowMachetaPage(_receptiiForm, MachetaRef.Create(MachetaEFacturaReceptii))
                Return
            End If
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("E-Factura Recepții", k_api, k_gate) Then Return
            _receptiiForm = New KeLegaForm(k_api, k_gate, _session.NumeUnitate, _session.An)
            AddHandler _receptiiForm.FormClosed, Sub() _receptiiForm = Nothing
            If Not ShowMachetaPage(_receptiiForm, MachetaRef.Create(MachetaEFacturaReceptii)) Then
                _receptiiForm.Dispose()
                _receptiiForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideEFacturaReceptii", ex)
            KBotMessage.Show(Me, "Pagina recepțiilor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura Recepții", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' A button of the E-Factura tab of the ribbon. The two unit commands (accounts, unit data) work on the issued
    ''' invoices window, which comes to the front first (its issuer follows what «Date Unitate» saves).
    ''' </summary>
    Friend Sub RunEFacturaCommand(k_key As String)
        Try
            Select Case k_key
                Case MachetaEFacturaVanzare
                    DeschideEFactura()
                Case MachetaEFacturaCumparare
                    DeschideEFacturaCumparare()
                Case MachetaEFacturaReceptii
                    DeschideEFacturaReceptii()
                Case "ef_token"
                    OpenTokenAnaf()
                Case Else
                    Throw New ArgumentException($"Unknown E-Factura command '{k_key}'.", NameOf(k_key))
            End Select
        Catch ex As Exception
            ' UI boundary (ribbon click): log and tell the operator.
            GlobalErrorLog.Write("MainForm.RunEFacturaCommand", ex)
            KBotMessage.Show(Me, "Comanda E-Factura nu a putut fi executată. Detalii în jurnalul de erori.",
                             "E-Factura", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Conturi Unitate» and «Date Unitate» (the tab «Configurari» of the ribbon): the unit's accounts and its data as the
    ''' issuer. They need only the unit, not the issued-invoices window: when that window is open it hears what changed.
    ''' </summary>
    Friend Async Sub RunUnitCommand(k_key As String)
        Try
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("Unitate", k_api, k_gate) Then Return
            Select Case k_key
                Case "unitate_conturi"
                    Using k_dialog As New ConturiForm(k_api, k_gate, _session.NumeUnitate)
                        k_dialog.ShowDialog(Me)
                    End Using
                    If _efacturaForm IsNot Nothing AndAlso Not _efacturaForm.IsDisposed Then _efacturaForm.UnitAccountsChangedElsewhere()
                Case "unitate_date"
                    ' Nothing = the unit has not filled its issuer data yet; the dialog starts empty.
                    Dim k_furnizor As EFacturaFurnizor = Await k_gate.RunAsync(
                        Function() k_api.GetFurnizorAsync(CancellationToken.None)).ConfigureAwait(True)
                    Using k_dialog As New DateUnitateForm(k_api, k_gate, k_furnizor)
                        k_dialog.ShowDialog(Me)
                        If k_dialog.Furnizor IsNot Nothing AndAlso _efacturaForm IsNot Nothing AndAlso Not _efacturaForm.IsDisposed Then
                            _efacturaForm.UnitDataChangedElsewhere(k_dialog.Furnizor)
                        End If
                    End Using
                Case Else
                    Throw New ArgumentException($"Unknown unit command '{k_key}'.", NameOf(k_key))
            End Select
        Catch ex As Exception
            ' UI boundary (async Sub from a ribbon click): log and tell the operator.
            GlobalErrorLog.Write("MainForm.RunUnitCommand", ex)
            KBotMessage.Show(Me, "Fereastra unității nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Unitate", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 00EF-30: brings the received invoices from ANAF for the last <paramref name="k_days"/> days, from the ribbon (no dialog
    ''' any more): the «Cumparare» page shows the progress on its bottom line and reads its list again. Nothing happens when that
    ''' page is not open (the ribbon shows the button only then). UI boundary (async Sub): logs and tells the operator.
    ''' </summary>
    Friend Async Sub SyncPrimite(k_days As Integer)
        Try
            If _primiteForm Is Nothing OrElse _primiteForm.IsDisposed Then Return
            Await _primiteForm.SyncFromAnafAsync(k_days).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SyncPrimite", ex)
            KBotMessage.Show(Me, "Sincronizarea facturilor primite nu a putut fi dusă la capăt. Detalii în jurnalul de erori.",
                             "Sincronizează facturi primite", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 00EF-43: «Trimite in Access» (<c>ef_access_trimite</c>) and «Preia din Access» (<c>ef_access_preia</c>) of the ribbon's form group, shown only for
    ''' a client that also runs the Access application while the received invoices window is on screen. The work and what the end tells are the window's.
    ''' UI boundary (async Sub): logs and tells the operator.
    ''' </summary>
    Friend Async Sub RunPrimiteAccess(k_key As String)
        Try
            If _primiteForm Is Nothing OrElse _primiteForm.IsDisposed Then Return
            Select Case k_key
                Case "ef_access_trimite"
                    Await _primiteForm.TrimiteInAccessAsync().ConfigureAwait(True)
                Case "ef_access_preia"
                    Await _primiteForm.PreiaDinAccessAsync().ConfigureAwait(True)
                Case Else
                    Throw New ArgumentException($"Unknown Access command '{k_key}'.", NameOf(k_key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RunPrimiteAccess", ex)
            KBotMessage.Show(Me, "Comanda Access nu a putut fi dusă la capăt. Detalii în jurnalul de erori.",
                             "Facturi primite", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Slice 00EF-05: the ANAF token step (the certificate, the signature that authorises the unit).
    Private Sub OpenTokenAnaf()
        Dim k_api As IEFacturaApi = Nothing
        Dim k_gate As ReauthGate = Nothing
        If Not EFacturaContext("Token ANAF", k_api, k_gate) Then Return
        Using k_form As New TokenForm(New TokenAuthorizer(k_api, k_gate), _session.NumeUnitate)
            k_form.ShowDialog(Me)
        End Using
    End Sub

End Class
