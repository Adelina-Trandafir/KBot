Option Strict On
Imports System.Drawing
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Slice 00EF-19 -- «E-Factura — facturi primite»: ALL the received invoices of the unit for the year chosen in K-BOT, in the same control the
''' DDF view uses (<see cref="FacturiCumparareView"/>): the tree by months and suppliers with the search in its header, the views on the right. Nothing
''' below it (slice 0114-21: the strip with «Ieșire» and the state line is gone; the ribbon closes the window and the tree footer
''' shows the state). The update from ANAF is the footer of the tree («Actualizează», slice 00EF-33; the work is
''' <see cref="FacturiCumparareView.SyncFromAnafAsync"/>) and the ribbon button (slice 00EF-30) calls the same method through
''' <see cref="SyncFromAnafAsync"/>. There is no «Reîmprospătează»: a re-read of the database is not wanted. The window holds no rules;
''' what is shown and what the buttons do come from the server (slice 00EF-17).
''' </summary>
Public Class FacturiCumparareForm
    Implements IPutAway

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _year As Integer

    ''' <summary>Slice 01EF-02: the main window's title bar, for the subunit selector of «Contari» (set by the shell after the window is made).</summary>
    Public Property SubunitHost As ISubunitHost
        Get
            Return facturiCumparare.SubunitHost
        End Get
        Set(k_value As ISubunitHost)
            facturiCumparare.SubunitHost = k_value
        End Set
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The received-invoice routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the title.</param>
    ''' <param name="k_year">The year chosen in K-BOT; only the invoices dated in it are listed (0 = all years).</param>
    ''' <param name="k_viewerFactory">Makes the embedded PDF viewer (it lives in the shell's project); Nothing = the PDF view says there is none.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_unitName As String, k_year As Integer,
                   k_viewerFactory As Func(Of IFacturaPdfViewer))
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _year = k_year
        facturiCumparare.Initialize(k_api, k_gate, k_viewerFactory, k_year)
        capBar.Text = "Facturi primite" & If(k_year > 0, $" {k_year}", String.Empty) &
                      If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; the control shows its own failures.
    Private Async Sub FacturiCumparareForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            Await facturiCumparare.LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareForm.FacturiCumparareForm_Load", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 00EF-30 / 00EF-33: brings the received invoices of the last <paramref name="k_days"/> days (1 to 60) from ANAF; the
    ''' ribbon button calls this. The work and what the end tells are the control's (<see cref="FacturiCumparareView.SyncFromAnafAsync"/>).
    ''' </summary>
    Public Async Function SyncFromAnafAsync(k_days As Integer) As Task
        If _api Is Nothing Then Return
        Await facturiCumparare.SyncFromAnafAsync(k_days).ConfigureAwait(True)
    End Function

    ''' <summary>
    ''' Slice 03EF-01: the ribbon button «Sincronizează facturi primite» opens the popup of the update (the one of the footer of the tree), just under the button:
    ''' <paramref name="k_anchor"/> is the ribbon and <paramref name="k_rect"/> the button in its client coordinates. UI boundary: the view logs and swallows.
    ''' </summary>
    Public Sub OpenSyncPopup(k_anchor As Control, k_rect As Rectangle)
        If _api Is Nothing Then Return
        facturiCumparare.OpenSyncPopup(k_anchor, k_rect)
    End Sub

    ''' <summary>Slice 00EF-43: «Preia din Access» (ribbon). The work and what the end tells are the control's (<see cref="FacturiCumparareView.PreiaDinAccessAsync"/>).</summary>
    Public Async Function PreiaDinAccessAsync() As Task
        If _api Is Nothing Then Return
        Await facturiCumparare.PreiaDinAccessAsync().ConfigureAwait(True)
    End Function

    ''' <summary>Slice 00EF-43: «Trimite in Access» (ribbon). The work and what the end tells are the control's (<see cref="FacturiCumparareView.TrimiteInAccessAsync"/>).</summary>
    Public Async Function TrimiteInAccessAsync() As Task
        If _api Is Nothing Then Return
        Await facturiCumparare.TrimiteInAccessAsync().ConfigureAwait(True)
    End Function

    ''' <summary>
    ''' The button of the reception work («Leagă» / «Creează recepția» / «Modifică recepția») stands in the ribbon's view group while the view «Recepții» is on screen:
    ''' the shell asks these four and hears <see cref="ReceptiiActionChanged"/> (sender = this window) when any answer changes.
    ''' </summary>
    Public Event ReceptiiActionChanged As EventHandler

    Public ReadOnly Property ReceptiiActionVisible As Boolean
        Get
            Return facturiCumparare.ReceptiiActionVisible
        End Get
    End Property

    Public ReadOnly Property ReceptiiActionEnabled As Boolean
        Get
            Return facturiCumparare.ReceptiiActionEnabled
        End Get
    End Property

    Public ReadOnly Property ReceptiiActionText As String
        Get
            Return facturiCumparare.ReceptiiActionText
        End Get
    End Property

    Public Sub RunReceptiiAction()
        facturiCumparare.RunReceptiiAction()
    End Sub

    ''' <summary>Slice 02EF-11 / KE21: the ribbon button «Genereaza ordonantare» is offered (a reception was saved by the view «Recepții» and nothing was changed since).</summary>
    Public ReadOnly Property OrdDisponibil As Boolean
        Get
            Return facturiCumparare.OrdDisponibil
        End Get
    End Property

    ''' <summary>The angajament of the reception saved last.</summary>
    Public ReadOnly Property OrdCod As String
        Get
            Return facturiCumparare.OrdCod
        End Get
    End Property

    ''' <summary>The snapshot (IDRH) of the reception saved last.</summary>
    Public ReadOnly Property OrdIdrh As Integer
        Get
            Return facturiCumparare.OrdIdrh
        End Get
    End Property

    ''' <summary>Slice 02EF-12 / KE21: the ribbon button «Creeaza rezervare» is offered.</summary>
    Public ReadOnly Property RezervareAvailable As Boolean
        Get
            Return facturiCumparare.RezervareAvailable
        End Get
    End Property

    ''' <summary>The ribbon button «Creeaza rezervare» can be pressed.</summary>
    Public ReadOnly Property RezervareEnabled As Boolean
        Get
            Return facturiCumparare.RezervareEnabled
        End Get
    End Property

    ''' <summary>The ribbon button «Creeaza rezervare» was pressed. UI boundary (the view logs and swallows).</summary>
    Public Sub RunRezervare()
        facturiCumparare.RunRezervare()
    End Sub

    ' ── Slice 03EF-02..05: the buttons of the view «Contari» in the ribbon (the shell asks these and hears ReceptiiActionChanged when an answer changes) ──

    ''' <summary>The view «Contari» is on screen: the view group of the ribbon is named after it and shows its buttons.</summary>
    Public ReadOnly Property ContareRibbonVisible As Boolean
        Get
            Return facturiCumparare.ContareRibbonVisible
        End Get
    End Property

    ''' <summary>The button is on screen (see <see cref="FacturiCumparareView.ContareRibbonShown"/>).</summary>
    Public Function ContareRibbonShown(k_key As String) As Boolean
        Return facturiCumparare.ContareRibbonShown(k_key)
    End Function

    ''' <summary>The button can be pressed (dimmed otherwise).</summary>
    Public Function ContareRibbonEnabled(k_key As String) As Boolean
        Return facturiCumparare.ContareRibbonEnabled(k_key)
    End Function

    ''' <summary>The text of the button when it follows the state; empty = the designer's text stays.</summary>
    Public Function ContareRibbonText(k_key As String) As String
        Return facturiCumparare.ContareRibbonText(k_key)
    End Function

    ''' <summary>The tooltip of the button when it carries news; empty = the designer's text stays.</summary>
    Public Function ContareRibbonTip(k_key As String) As String
        Return facturiCumparare.ContareRibbonTip(k_key)
    End Function

    ''' <summary>A button of «Contari» was pressed (<paramref name="k_anchor"/> = the ribbon, <paramref name="k_rect"/> = the button, for the popup of «Exportă...»). UI boundary: the view logs and swallows.</summary>
    Public Sub RunContareRibbon(k_key As String, k_anchor As Control, k_rect As Rectangle)
        facturiCumparare.RunContareRibbon(k_key, k_anchor, k_rect)
    End Sub

    Private Sub FacturiCumparare_ReceptiiActionChanged(sender As Object, e As EventArgs) Handles facturiCumparare.ReceptiiActionChanged
        Try
            RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareForm.FacturiCumparare_ReceptiiActionChanged", ex)
        End Try
    End Sub

    ''' <summary>Slice 0113-01: put away -- the control moves to a view without a PDF and lets the document go.</summary>
    Public Sub PutAway() Implements IPutAway.PutAway
        facturiCumparare.PutAway()
    End Sub

End Class
