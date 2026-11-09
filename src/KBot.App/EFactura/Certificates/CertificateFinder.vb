Option Strict On
Imports System.Collections.Generic
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' Lists the certificates of the current user's Windows store that may be used for the ANAF login (slice
''' 00EF-05): valid today, with a private key that cannot be exported, in a hardware key store or with Client
''' Authentication, and not a test certificate (<see cref="CertificateRules"/>).
''' <para>It only READS the store and the key's properties (provider name, export policy). It never exports,
''' copies or signs with a key: the key stays on the token, and the PIN is asked by the token's own software
''' later, when the certificate is used for the call to ANAF.</para>
''' </summary>
Public NotInheritable Class CertificateFinder

    Private Shared ReadOnly CommonNamePattern As New Regex("CN=([^,]+)", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Every eligible certificate, best (latest expiry) first. The caller disposes ALL of them, chosen or not.
    ''' A certificate whose key cannot be inspected is skipped and logged; it does not stop the list.
    ''' </summary>
    Public Shared Function FindEligible() As IReadOnlyList(Of AnafCertificate)
        Dim k_found As New List(Of AnafCertificate)()
        Try
            Using k_store As New X509Store(StoreName.My, StoreLocation.CurrentUser)
                k_store.Open(OpenFlags.ReadOnly Or OpenFlags.OpenExistingOnly)
                For Each k_cert As X509Certificate2 In k_store.Certificates
                    Dim k_keep As AnafCertificate = Nothing
                    Try
                        k_keep = Inspect(k_cert)
                    Catch ex As Exception
                        ' One broken certificate (a card that was pulled out, a provider that refuses) is not a
                        ' reason to hide the others; it is recorded and skipped.
                        GlobalErrorLog.Write("CertificateFinder.Inspect", ex)
                    End Try
                    If k_keep Is Nothing Then
                        k_cert.Dispose()
                    Else
                        k_found.Add(k_keep)
                    End If
                Next
            End Using
            k_found.Sort(Function(a, b) b.NotAfter.CompareTo(a.NotAfter))
            Return k_found
        Catch ex As Exception
            ' Risky / boundary (the certificate store): log and rethrow, releasing what was already taken.
            GlobalErrorLog.Write("CertificateFinder.FindEligible", ex)
            For Each k_item As AnafCertificate In k_found
                k_item.Dispose()
            Next
            Throw
        End Try
    End Function

    ' The wrapper when the certificate qualifies, Nothing when it does not. Does not dispose k_cert.
    Private Shared Function Inspect(k_cert As X509Certificate2) As AnafCertificate
        If Not k_cert.HasPrivateKey Then Return Nothing
        If Date.Now > k_cert.NotAfter Then Return Nothing

        Dim k_provider As String = String.Empty
        Dim k_nonExportable As Boolean = False
        Using k_rsa As RSA = k_cert.GetRSAPrivateKey()
            If k_rsa Is Nothing Then Return Nothing
            Dim k_cng As RSACng = TryCast(k_rsa, RSACng)
            If k_cng IsNot Nothing Then
                k_provider = k_cng.Key.Provider.Provider
                k_nonExportable = (k_cng.Key.ExportPolicy And CngExportPolicies.AllowPlaintextExport) = CngExportPolicies.None
            Else
                Dim k_capi As RSACryptoServiceProvider = TryCast(k_rsa, RSACryptoServiceProvider)
                If k_capi IsNot Nothing Then
                    k_provider = k_capi.CspKeyContainerInfo.ProviderName
                    k_nonExportable = Not k_capi.CspKeyContainerInfo.Exportable
                End If
            End If
        End Using

        Dim k_cn As String = CommonNameOf(k_cert.Subject)
        If Not CertificateRules.IsEligible(k_provider, k_nonExportable, HasClientAuth(k_cert), k_cn) Then Return Nothing
        Return New AnafCertificate(k_cert, If(k_cn.Length = 0, "N/A", k_cn), CommonNameOf(k_cert.Issuer), k_provider)
    End Function

    Private Shared Function CommonNameOf(k_distinguishedName As String) As String
        Dim k_match As Match = CommonNamePattern.Match(If(k_distinguishedName, String.Empty))
        Return If(k_match.Success, k_match.Groups(1).Value.Trim(), String.Empty)
    End Function

    Private Shared Function HasClientAuth(k_cert As X509Certificate2) As Boolean
        For Each k_ext As X509Extension In k_cert.Extensions
            Dim k_eku As X509EnhancedKeyUsageExtension = TryCast(k_ext, X509EnhancedKeyUsageExtension)
            If k_eku Is Nothing Then Continue For
            For Each k_oid As Oid In k_eku.EnhancedKeyUsages
                If k_oid.Value = CertificateRules.ClientAuthenticationOid Then Return True
            Next
        Next
        Return False
    End Function

End Class
