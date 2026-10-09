Option Strict On
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Slice 00EF-19 -- «E-Factura — facturi primite»: ALL the received invoices of the unit for the year chosen in K-BOT, in the same control the
''' DDF view uses (<see cref="PrimiteView"/>): the tree by months with the search box above it, the views on the right. At the bottom,
''' «Sincronizează cu ANAF» (the same dialog as the menu of the issued-invoice window) and «Reîmprospătează». The window holds no rules;
''' what is shown and what the buttons do come from the server (slice 00EF-17).
''' </summary>
Public Class PrimiteForm

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _year As Integer
    Private _busy As Boolean

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

    Private Async Sub BtnReimprospateaza_Click(sender As Object, e As EventArgs) Handles btnReimprospateaza.Click
        Try
            If _busy OrElse _api Is Nothing Then Return
            _busy = True
            Await primite.LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.BtnReimprospateaza_Click", ex)
        Finally
            _busy = False
        End Try
    End Sub

    Private Async Sub BtnSincronizeaza_Click(sender As Object, e As EventArgs) Handles btnSincronizeaza.Click
        Try
            If _busy OrElse _api Is Nothing Then Return
            _busy = True
            Dim k_added As Boolean
            Using k_dialog As New SincronizarePrimiteForm(_api, _gate)
                k_dialog.ShowDialog(Me)
                k_added = k_dialog.AdaugatCeva
            End Using
            If k_added AndAlso Not IsDisposed Then Await primite.LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.BtnSincronizeaza_Click", ex)
        Finally
            _busy = False
        End Try
    End Sub

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteForm.BtnIesire_Click", ex)
        End Try
    End Sub

End Class
