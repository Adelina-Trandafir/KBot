Option Strict On
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Slice 00EF-19 -- «E-Factura — facturi primite»: ALL the received invoices of the unit for the year chosen in K-BOT, in the same control the
''' DDF view uses (<see cref="PrimiteView"/>): the tree by months and suppliers with the search in its header, the views on the right. At the
''' bottom, a line of state. The update from ANAF is the footer of the tree («Actualizare…», slice 00EF-33; the work is
''' <see cref="PrimiteView.SyncFromAnafAsync"/>) and the ribbon button (slice 00EF-30) calls the same method through
''' <see cref="SyncFromAnafAsync"/>. There is no «Reîmprospătează»: a re-read of the database is not wanted. The window holds no rules;
''' what is shown and what the buttons do come from the server (slice 00EF-17).
''' </summary>
Public Class PrimiteForm

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _year As Integer

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
        primite.Initialize(k_api, k_gate, k_viewerFactory, k_year)
        AddHandler primite.SyncStatus, AddressOf Primite_SyncStatus
        capBar.Text = "Facturi primite" & If(k_year > 0, $" {k_year}", String.Empty) &
                      If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; the control shows its own failures.
    Private Async Sub PrimiteForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            Await primite.LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.PrimiteForm_Load", ex)
        End Try
    End Sub

    ' The state line of the update (the control also shows it in the footer of its tree). UI boundary: logs and swallows.
    Private Sub Primite_SyncStatus(k_text As String)
        Try
            If Not IsDisposed Then lblStare.Text = k_text
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.Primite_SyncStatus", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 00EF-30 / 00EF-33: brings the received invoices of the last <paramref name="k_days"/> days (1 to 60) from ANAF; the
    ''' ribbon button calls this. The work and what the end tells are the control's (<see cref="PrimiteView.SyncFromAnafAsync"/>).
    ''' </summary>
    Public Async Function SyncFromAnafAsync(k_days As Integer) As Task
        If _api Is Nothing Then Return
        Await primite.SyncFromAnafAsync(k_days).ConfigureAwait(True)
    End Function

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.BtnIesire_Click", ex)
        End Try
    End Sub

End Class
