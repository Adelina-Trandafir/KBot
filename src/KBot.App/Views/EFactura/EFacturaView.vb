Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 -- the view «E-Factura» of the angajament shell: the received e-invoices of the DDF of the chosen angajament. The view is
''' a thin shell around <see cref="PrimiteView"/> (KBot.EFactura): it finds the DDF of the angajament (the same read the DDF view does) and
''' hands its id over; the list, the views and the links are the control's. An angajament without a DDF shows nothing.
''' </summary>
Public Class EFacturaView
    Implements IAngajamentView, IReleasesDocument, IThemedContainer

    Private ReadOnly _api As IApiClient
    Private ReadOnly _reauthDdf As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
    Private _seq As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_api">The shell's API client (the DDF read and, through <paramref name="k_eFactura"/>, the invoice routes).</param>
    ''' <param name="k_reauthDdf">The shell's re-login net for the DDF read.</param>
    ''' <param name="k_gate">The same net for the invoice calls.</param>
    Public Sub New(k_api As IApiClient, k_reauthDdf As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo)), k_gate As ReauthGate)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_reauthDdf)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _reauthDdf = k_reauthDdf
        Dim k_eFactura As IEFacturaApi = TryCast(k_api, IEFacturaApi)
        If k_eFactura Is Nothing Then Throw New InvalidOperationException("The API client does not implement IEFacturaApi.")
        primite.Initialize(k_eFactura, k_gate, Function() New FacturaPdfViewer(), 0)
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "efactura"
        End Get
    End Property

    ''' <summary>The selection changed: the invoices of the DDF of the new angajament. UI boundary (async Sub): logs and swallows.</summary>
    Public Async Sub SetContext(k_info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            If k_info Is Nothing OrElse String.IsNullOrEmpty(k_info.CodAngajament) OrElse _api Is Nothing Then
                primite.ClearAll()
                Return
            End If
            Dim k_cod As String = k_info.CodAngajament
            Dim k_ddf As DdfInfo = Await _reauthDdf(Function() _api.GetDdfAsync(k_cod, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _seq Then Return
            Dim k_antet As DdfAntet = k_ddf.AntetDeLucru(0)
            If k_antet Is Nothing Then
                primite.ClearAll()
                Return
            End If
            If k_ddf.Antet.Count > 1 Then
                ' Several headers on one angajament: the first one is used and the choice is on record (it must not be silent).
                GlobalErrorLog.Write("EFacturaView.SetContext",
                                     New InvalidOperationException($"Angajamentul {k_cod} are {k_ddf.Antet.Count} DDF-uri; facturile primite se arata pentru primul."))
            End If
            Await primite.LoadAsync(k_antet.Iddf).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("EFacturaView.SetContext", ex)
        End Try
    End Sub

    Public Sub ReleaseDocument() Implements IReleasesDocument.ReleaseDocument
        primite.ReleaseDocument()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("EFacturaView.ApplyTheme", ex)
        End Try
    End Sub

End Class
