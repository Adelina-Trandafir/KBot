Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Slice 00EF-05 -- the operator picks the qualified certificate for the ANAF login. The list comes from
''' <see cref="CertificateFinder"/>; a single certificate is still shown (the operator confirms what is about to
''' be used). Shows only labels: name, issuer, expiry, device. The private key is never touched here.
''' </summary>
Public Class CertificatePickerForm

    Private Const ColName As String = "nume"
    Private Const ColIssuer As String = "emis"
    Private Const ColExpires As String = "expira"
    Private Const ColProvider As String = "furnizor"

    Private ReadOnly _certs As IReadOnlyList(Of AnafCertificate)

    Private _selected As AnafCertificate

    ''' <summary>The certificate chosen, set when the form closes with OK; Nothing otherwise.</summary>
    Public ReadOnly Property Selected As AnafCertificate
        Get
            Return _selected
        End Get
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _certs = New List(Of AnafCertificate)()
    End Sub

    Public Sub New(k_certs As IReadOnlyList(Of AnafCertificate))
        ArgumentNullException.ThrowIfNull(k_certs)
        InitializeComponent()
        _certs = k_certs
    End Sub

    Private Sub CertificatePickerForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            FillGrid()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow; the list stays empty and «Folosește» stays off.
            GlobalErrorLog.Write("CertificatePickerForm.CertificatePickerForm_Load", ex)
        End Try
    End Sub

    Private Sub FillGrid()
        grilaCertificate.BeginUpdate()
        Try
            grilaCertificate.ClearRows()
            For Each k_cert As AnafCertificate In _certs
                Dim k_row As KBotDataRow = grilaCertificate.AddRow()
                k_row.Tag = k_cert
                k_row(ColName) = k_cert.CommonName
                k_row(ColIssuer) = k_cert.IssuerName
                k_row(ColExpires) = k_cert.NotAfter.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                k_row(ColProvider) = k_cert.ProviderName
            Next
        Finally
            grilaCertificate.EndUpdate()
        End Try
        If grilaCertificate.RowCount > 0 Then grilaCertificate.CurrentRowIndex = 0
        btnAlege.Enabled = grilaCertificate.RowCount > 0
    End Sub

    Private Sub btnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            ' The button already carries DialogResult.OK: the form closes after this handler.
            _selected = CurrentCertificate()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("CertificatePickerForm.btnAlege_Click", ex)
        End Try
    End Sub

    Private Sub grilaCertificate_CellDoubleClick(sender As Object, e As KBotCellEventArgs) Handles grilaCertificate.CellDoubleClick
        Try
            If CurrentCertificate() Is Nothing Then Return
            btnAlege.PerformClick()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("CertificatePickerForm.grilaCertificate_CellDoubleClick", ex)
        End Try
    End Sub

    Private Function CurrentCertificate() As AnafCertificate
        Dim k_row As KBotDataRow = grilaCertificate.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, AnafCertificate))
    End Function

End Class
