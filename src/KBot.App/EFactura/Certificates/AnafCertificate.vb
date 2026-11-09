Option Strict On
Imports System.Security.Cryptography.X509Certificates

''' <summary>
''' A certificate of the Windows store the operator may use for the ANAF login (slice 00EF-05): the
''' <see cref="X509Certificate2"/> itself plus what the picker shows. The private key stays on the token or
''' card: nothing here reads, copies or exports it.
''' <para>Owns the certificate: <see cref="Dispose"/> releases it. The finder hands out one object per
''' eligible certificate and the caller disposes every one of them, chosen or not.</para>
''' </summary>
Public NotInheritable Class AnafCertificate
    Implements IDisposable

    Private ReadOnly _certificate As X509Certificate2

    Public Sub New(k_certificate As X509Certificate2, k_commonName As String, k_issuerName As String,
                   k_providerName As String)
        ArgumentNullException.ThrowIfNull(k_certificate)
        _certificate = k_certificate
        CommonName = If(k_commonName, String.Empty)
        IssuerName = If(k_issuerName, String.Empty)
        ProviderName = If(k_providerName, String.Empty)
    End Sub

    Public ReadOnly Property Certificate As X509Certificate2
        Get
            Return _certificate
        End Get
    End Property

    ''' <summary>The CN of the subject (the person or company the certificate was issued to).</summary>
    Public ReadOnly Property CommonName As String

    ''' <summary>The CN of the issuer.</summary>
    Public ReadOnly Property IssuerName As String

    ''' <summary>The key store the private key lives in (the smart-card / token provider).</summary>
    Public ReadOnly Property ProviderName As String

    Public ReadOnly Property NotAfter As Date
        Get
            Return _certificate.NotAfter
        End Get
    End Property

    Public ReadOnly Property Thumbprint As String
        Get
            Return _certificate.Thumbprint
        End Get
    End Property

    Public Sub Dispose() Implements IDisposable.Dispose
        _certificate.Dispose()
    End Sub

End Class
