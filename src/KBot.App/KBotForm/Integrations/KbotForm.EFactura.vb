Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Theming

' Slice 00EF-05 / 00EF-08 -- E-Factura (KBot.EFactura), used directly by the shell, in the same process: the open
' unit and the re-login net are handed over as plain arguments, nothing is copied. Since the opening page it is a tab
' of the ribbon with three machete / groups of commands:
'   - «E-Factura Vanzare»  (key efactura_vanzare): the invoices the unit issues (FacturiForm);
'   - «E-Factura Cumparare» (key efactura_cumparare): the invoices received from ANAF (FacturiCumparareForm);
'   - the unit's commands that were in the title bar of the issued-invoices window (accounts, unit data), the
'     received-invoices synchronisation (slice 0114-01 / 00EF-30 / 03EF-01: a ribbon button, shown while the «Cumparare» page is on
'     screen, whose click opens the popup of the period under it; no dialog) and the ANAF token step.
' Each machete is a page of the main window, one at a time; asking again brings the open one to the front.
Partial Public Class KbotForm

    Friend Const MachetaEFacturaVanzare As String = "efactura_vanzare"
    Friend Const MachetaEFacturaCumparare As String = "efactura_cumparare"
    Friend Const MachetaEFacturaVercon As String = "efactura_vercon"

    Private _efacturaForm As FacturiForm
    Private _facturiCumparareForm As FacturiCumparareForm
    Private _verconForm As VerconForm

    ' SLICE-02EF-14: adapt the shared view to the shell's selection contract.
    Private NotInheritable Class FacturiCumparareAngajamentView
        Inherits FacturiCumparareView
        Implements IAngajamentView, IReleasesDocument, IViewRibbon

        Private ReadOnly _api As IApiClient
        Private ReadOnly _reauthDdf As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
        Private _contextSeq As Integer
        Private ReadOnly _ordonantare As Action(Of String, Integer)

        Public Sub New(k_api As IApiClient, k_reauthDdf As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo)), k_gate As ReauthGate, k_ordonantare As Action(Of String, Integer))
            MyBase.New(DirectCast(k_api, IEFacturaApi), k_gate, Function() New FacturaPdfViewer(True), 0)
            _api = k_api
            _reauthDdf = k_reauthDdf
            _ordonantare = k_ordonantare
            AngajamentView = True
            AddHandler ReceptiiActionChanged, Sub(k_sender As Object, k_args As EventArgs) RaiseEvent RibbonStateChanged(Me, EventArgs.Empty)
        End Sub

        ' Slice KE21: the work of the view «Recepții» (link / create / modify, the ordonantare and the reservation built on it) is reached from the ribbon here too.
        Public Event RibbonStateChanged As EventHandler Implements IViewRibbon.RibbonStateChanged

        Public ReadOnly Property RibbonCaption As String Implements IViewRibbon.RibbonCaption
            Get
                Return "Recepții"
            End Get
        End Property

        Public ReadOnly Property RibbonKeys As IReadOnlyList(Of String) Implements IViewRibbon.RibbonKeys
            Get
                Return {AvacontMain.KeyReceptieActiune, AvacontMain.KeyReceptieOrd, AvacontMain.KeyReceptieRezervare}
            End Get
        End Property

        Public Function RibbonButtonState(k_key As String) As ViewButtonState Implements IViewRibbon.RibbonButtonState
            Select Case k_key
                Case AvacontMain.KeyReceptieActiune
                    If Not ReceptiiActionVisible Then Return ViewButtonState.Hidden
                    Return If(ReceptiiActionEnabled, ViewButtonState.Enabled, ViewButtonState.Dimmed)
                Case AvacontMain.KeyReceptieOrd
                    Return If(OrdDisponibil, ViewButtonState.Enabled, ViewButtonState.Hidden)
                Case AvacontMain.KeyReceptieRezervare
                    If Not RezervareAvailable Then Return ViewButtonState.Hidden
                    Return If(RezervareEnabled, ViewButtonState.Enabled, ViewButtonState.Dimmed)
                Case Else
                    Return ViewButtonState.Hidden
            End Select
        End Function

        Public Function RibbonButtonText(k_key As String) As String Implements IViewRibbon.RibbonButtonText
            Return If(String.Equals(k_key, AvacontMain.KeyReceptieActiune, StringComparison.Ordinal), ReceptiiActionText, String.Empty)
        End Function

        ' UI boundary: logs and swallows (the view does, or this method).
        Public Sub RunRibbonButton(k_key As String) Implements IViewRibbon.RunRibbonButton
            Try
                Select Case k_key
                    Case AvacontMain.KeyReceptieActiune
                        RunReceptiiAction()
                    Case AvacontMain.KeyReceptieOrd
                        If OrdDisponibil Then _ordonantare?.Invoke(OrdCod, OrdIdrh)
                    Case AvacontMain.KeyReceptieRezervare
                        RunRezervare()
                End Select
            Catch ex As Exception
                GlobalErrorLog.Write("FacturiCumparareAngajamentView.RunRibbonButton", ex)
            End Try
        End Sub

        Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
            Get
                Return "efactura"
            End Get
        End Property

        Public Async Sub SetContext(k_info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
            Try
                Dim k_mySeq As Integer = Interlocked.Increment(_contextSeq)
                If k_info Is Nothing OrElse String.IsNullOrEmpty(k_info.CodAngajament) Then
                    ClearAll()
                    Return
                End If
                Dim k_cod As String = k_info.CodAngajament
                Dim k_ddf As DdfInfo = Await _reauthDdf(Function() _api.GetDdfAsync(k_cod, CancellationToken.None)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _contextSeq Then Return
                Dim k_antet As DdfAntet = k_ddf.AntetDeLucru(0)
                If k_antet Is Nothing Then
                    ClearAll()
                    Return
                End If
                If k_ddf.Antet.Count > 1 Then
                    GlobalErrorLog.Write("FacturiCumparareAngajamentView.SetContext",
                                         New InvalidOperationException($"Angajamentul {k_cod} are {k_ddf.Antet.Count} DDF-uri; facturile primite se arata pentru primul."))
                End If
                AngajamentCode = k_cod
                Await LoadAsync(k_antet.Iddf).ConfigureAwait(True)
            Catch ex As Exception
                GlobalErrorLog.Write("FacturiCumparareAngajamentView.SetContext", ex)
            End Try
        End Sub

        Private Sub ReleaseContextDocument() Implements IReleasesDocument.ReleaseDocument
            ReleaseDocument()
        End Sub
    End Class

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
            If _facturiCumparareForm IsNot Nothing AndAlso Not _facturiCumparareForm.IsDisposed Then
                ShowMachetaPage(_facturiCumparareForm, MachetaRef.Create(MachetaEFacturaCumparare))
                Return
            End If
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("E-Factura Cumpărare", k_api, k_gate) Then Return
            _facturiCumparareForm = New FacturiCumparareForm(k_api, k_gate, _session.NumeUnitate, _session.An, Function() New FacturaPdfViewer(True))
            _facturiCumparareForm.SubunitHost = Me   ' slice 01EF-02: the subunit selector of «Contari» is in the title bar
            AddHandler _facturiCumparareForm.FormClosed, Sub() _facturiCumparareForm = Nothing
            If Not ShowMachetaPage(_facturiCumparareForm, MachetaRef.Create(MachetaEFacturaCumparare)) Then
                _facturiCumparareForm.Dispose()
                _facturiCumparareForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideEFacturaCumparare", ex)
            KBotMessage.Show(Me, "Fereastra facturilor primite nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura Cumpărare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    ''' <summary>Slice 00EF-48, «E-Factura Trimite la Vercon»: the page where the accountant sends received invoices to VERCON (with the account proposed on each line).</summary>
    Private Sub DeschideEFacturaVercon()
        Try
            If _verconForm IsNot Nothing AndAlso Not _verconForm.IsDisposed Then
                ShowMachetaPage(_verconForm, MachetaRef.Create(MachetaEFacturaVercon))
                Return
            End If
            Dim k_api As IEFacturaApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not EFacturaContext("E-Factura Trimite la Vercon", k_api, k_gate) Then Return
            _verconForm = New VerconForm(k_api, k_gate, _session.NumeUnitate, _session.An)
            AddHandler _verconForm.FormClosed, Sub() _verconForm = Nothing
            If Not ShowMachetaPage(_verconForm, MachetaRef.Create(MachetaEFacturaVercon)) Then
                _verconForm.Dispose()
                _verconForm = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideEFacturaVercon", ex)
            KBotMessage.Show(Me, "Pagina de trimitere la Vercon nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "E-Factura Trimite la Vercon", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
                Case MachetaEFacturaVercon
                    DeschideEFacturaVercon()
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
    ''' Slice 00EF-43: «Trimite in Access» (<c>ef_access_trimite</c>) and «Preia din Access» (<c>ef_access_preia</c>) of the ribbon's form group, shown only for
    ''' a client that also runs the Access application while the received invoices window is on screen. The work and what the end tells are the window's.
    ''' UI boundary (async Sub): logs and tells the operator.
    ''' </summary>
    Friend Async Sub RunPrimiteAccess(k_key As String)
        Try
            If _facturiCumparareForm Is Nothing OrElse _facturiCumparareForm.IsDisposed Then Return
            Select Case k_key
                Case "ef_access_trimite"
                    Await _facturiCumparareForm.TrimiteInAccessAsync().ConfigureAwait(True)
                Case "ef_access_preia"
                    Await _facturiCumparareForm.PreiaDinAccessAsync().ConfigureAwait(True)
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
