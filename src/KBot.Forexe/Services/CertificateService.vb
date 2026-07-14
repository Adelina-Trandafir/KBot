Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' Service for smartcard certificate operations
''' </summary>
Public Class CertificateService

    ''' <summary>
    ''' Get all valid certificates with private keys from smartcard/token
    ''' </summary>
    Public Shared Function GetSmartcardCertificates() As List(Of X509Certificate2)
        Try
            Dim certificates As New List(Of X509Certificate2)
            AddCertificatesFromStore(certificates, StoreLocation.CurrentUser)
            AddCertificatesFromStore(certificates, StoreLocation.LocalMachine)
            Return certificates
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.GetSmartcardCertificates", ex)
            Throw
        End Try
    End Function

    Private Shared Sub AddCertificatesFromStore(certificates As List(Of X509Certificate2), location As StoreLocation)
        Try
            Using store As New X509Store(StoreName.My, location)
                store.Open(OpenFlags.ReadOnly)
                For Each cert As X509Certificate2 In store.Certificates
                    If Not cert.HasPrivateKey Then Continue For
                    If DateTime.Now > cert.NotAfter Then Continue For
                    If DateTime.Now < cert.NotBefore Then Continue For
                    If Not IsValidHardwareCertificate(cert) Then Continue For
                    If Not certificates.Any(Function(c) c.Thumbprint = cert.Thumbprint) Then
                        certificates.Add(cert)
                    End If
                Next
            End Using
        Catch ex As Exception
            ' Înghițire intenționată: un magazin inaccesibil nu trebuie să pice enumerarea
            ' celuilalt magazin; logăm și continuăm.
            GlobalErrorLog.Write("CertificateService.AddCertificatesFromStore", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Verifică dacă certificatul este stocat pe un dispozitiv hardware sau este eligibil.
    ''' Implementează logica strictă de validare și folosește USING pentru a nu bloca PIN-ul.
    ''' </summary>
    Private Shared Function IsValidHardwareCertificate(cert As X509Certificate2) As Boolean
        Try
            ' !!! USING este critic aici pentru a elibera handle-ul către Smart Card imediat !!!
            Using rsaPrivateKey As RSA = cert.GetRSAPrivateKey()
                If rsaPrivateKey Is Nothing Then Return False

                Dim providerName As String = ""
                Dim isNonExportable As Boolean = False
                Dim isHardwareProvider As Boolean = False

                ' --- A. Identificare Provider și Export Policy ---
                Dim cngKey As RSACng = TryCast(rsaPrivateKey, RSACng)
                If cngKey IsNot Nothing Then
                    providerName = cngKey.Key.Provider.Provider
                    ' Bitul AllowPlaintextExport = 0 înseamnă că nu se poate exporta (deci e sigur/hardware)
                    isNonExportable = (cngKey.Key.ExportPolicy And CngExportPolicies.AllowPlaintextExport) = 0
                Else
                    Dim capiKey As RSACryptoServiceProvider = TryCast(rsaPrivateKey, RSACryptoServiceProvider)
                    If capiKey IsNot Nothing Then
                        providerName = capiKey.CspKeyContainerInfo.ProviderName
                        isNonExportable = Not capiKey.CspKeyContainerInfo.Exportable
                    End If
                End If

                ' --- B. BLACKLIST: Excludem explicit providerii software Microsoft ---
                Dim microsoftSoftwareProviders As String() = {
                    "Microsoft Strong Cryptographic Provider",
                    "Microsoft Enhanced Cryptographic Provider",
                    "Microsoft Base Cryptographic Provider",
                    "Microsoft Software Key Storage Provider"
                }

                Dim isMicrosoftSoftware As Boolean = False
                For Each msProvider In microsoftSoftwareProviders
                    If providerName.Equals(msProvider, StringComparison.OrdinalIgnoreCase) Then
                        isMicrosoftSoftware = True
                        Return False
                    End If
                Next

                ' --- C. WHITELIST: Acceptăm providerii hardware cunoscuți ---
                Dim hardwareProviders As String() = {
                    "Smart Card", "Token", "Athena", "SafeNet", "eToken",
                    "Aladdin", "Gemalto", "Feitian", "JaCarta", "Oberthur",
                    "ePass", "Certum", "Cryptotech", "OpenSC", "Siemens CardOS"
                }

                For Each hwProvider In hardwareProviders
                    If providerName.IndexOf(hwProvider, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        isHardwareProvider = True
                        Exit For
                    End If
                Next

                ' --- D. Verificăm Extended Key Usage (EKU) pentru Client Authentication ---
                ' Unele certificate (ex: Cloud) nu apar ca hardware provider clasic, dar au acest flag.
                Dim hasClientAuth As Boolean = False
                For Each extension In cert.Extensions
                    If TypeOf extension Is X509EnhancedKeyUsageExtension Then
                        Dim ekuExt As X509EnhancedKeyUsageExtension = DirectCast(extension, X509EnhancedKeyUsageExtension)
                        For Each oid In ekuExt.EnhancedKeyUsages
                            ' OID pentru Client Authentication: 1.3.6.1.5.5.7.3.2
                            If oid.Value = "1.3.6.1.5.5.7.3.2" Then
                                hasClientAuth = True
                                Exit For
                            End If
                        Next
                    End If
                Next

                ' --- E. LOGICA FINALĂ DE VALIDARE ---
                ' 1. Trebuie să nu fie exportabil.
                ' 2. Trebuie să NU fie un provider software Microsoft standard.
                ' 3. Trebuie să fie (Hardware Provider Cunoscut) SAU (Să aibă Client Auth).
                Dim isValid As Boolean = isNonExportable AndAlso
                                         Not isMicrosoftSoftware AndAlso
                                         (isHardwareProvider OrElse hasClientAuth)

                ' --- F. Filtrare suplimentară pe nume (Excludem Localhost/Test) ---
                If isValid Then
                    Dim cnMatch As Match = Regex.Match(cert.Subject, "CN=([^,]+)")
                    Dim cn As String = If(cnMatch.Success, cnMatch.Groups(1).Value, "")

                    If cn.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       cn.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        isValid = False
                    End If
                End If

                Return isValid

            End Using ' <--- Aici se închide conexiunea cu cardul pentru acest certificat

        Catch ex As Exception
            ' Înghițire intenționată: la eroare (ex: driver lipsă) considerăm certificatul
            ' invalid ca să nu crăpăm aplicația; logăm totuși în sink-ul global.
            GlobalErrorLog.Write("CertificateService.IsValidHardwareCertificate", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Extrage Common Name (CN) din subiectul certificatului
    ''' </summary>
    Public Shared Function GetCommonName(cert As X509Certificate2) As String
        Try
            Dim match As Match = Regex.Match(cert.Subject, "CN=([^,]+)")
            If match.Success Then
                Return match.Groups(1).Value
            End If
            Return cert.Subject
        Catch ex As Exception
            ' Înghițire intenționată: dacă regex-ul pică, întoarcem subiectul brut.
            GlobalErrorLog.Write("CertificateService.GetCommonName", ex)
            Return cert.Subject
        End Try
    End Function

    ''' <summary>
    ''' Generează numele de afișat în ComboBox
    ''' </summary>
    Public Shared Function GetDisplayName(cert As X509Certificate2) As String
        Try
            Dim cn = GetCommonName(cert)
            Dim expiry = cert.NotAfter.ToString("dd.MM.yyyy")
            Dim issuerMatch As Match = Regex.Match(cert.Issuer, "CN=([^,]+)")
            Dim issuer As String = If(issuerMatch.Success, issuerMatch.Groups(1).Value, "N/A")

            Return $"{cn} (exp: {expiry}) - Emis de: {issuer}"
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.GetDisplayName", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Validează PIN-ul încercând o semnare digitală de test.
    ''' Folosește USING pentru a nu bloca token-ul după validare.
    ''' </summary>
    Public Shared Function ValidatePin(cert As X509Certificate2) As (Success As Boolean, Message As String)
        Try
            ' !!! USING este critic aici. Deschide conexiunea, cere PIN, semnează, apoi ÎNCHIDE conexiunea !!!
            Using privateKey As RSA = cert.GetRSAPrivateKey()
                If privateKey Is Nothing Then
                    Return (False, "Nu s-a putut accesa cheia privată RSA.")
                End If

                ' Semnăm date random pentru a declanșa dialogul de PIN
                Dim testData = New Byte() {1, 2, 3, 4, 5, 6, 7, 8}
                privateKey.SignData(testData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)

            End Using ' <--- Sesiunea de PIN se resetează mai repede deoarece eliberăm resursa aici.

            Return (True, "PIN valid")

        Catch ex As CryptographicException
            ' Coduri de eroare specifice Smart Card
            Const PIN_INCORRECT As Integer = &H8010006B
            Const PIN_BLOCKED As Integer = &H8010006C
            Const CARD_REMOVED As Integer = &H80100068

            Select Case ex.HResult
                Case PIN_INCORRECT
                    Return (False, "PIN incorect")
                Case PIN_BLOCKED
                    Return (False, "PIN blocat! Prea multe încercări greșite. Contactați administratorul.")
                Case CARD_REMOVED
                    Return (False, "Token-ul a fost scos sau nu poate fi accesat.")
                Case Else
                    Return (False, $"Eroare criptografică (HResult: {ex.HResult:X}): {ex.Message}")
            End Select
        Catch ex As Exception
            ' Boundary spre UI: raportăm rezultatul, nu rearuncăm; logăm în sink-ul global.
            GlobalErrorLog.Write("CertificateService.ValidatePin", ex)
            Return (False, $"Eroare la validare PIN: {ex.Message}")
        End Try
    End Function

    ''' <summary>
    ''' Returnează detalii complete pentru afișare text
    ''' </summary>
    Public Shared Function GetCertificateDetails(cert As X509Certificate2) As String
        Try
            Return $"Subiect (CN): {GetCommonName(cert)}{vbCrLf}" &
                   $"Emitent: {cert.IssuerName.Name}{vbCrLf}" &
                   $"Valabil de la: {cert.NotBefore:dd.MM.yyyy HH:mm:ss}{vbCrLf}" &
                   $"Valabil până la: {cert.NotAfter:dd.MM.yyyy HH:mm:ss}{vbCrLf}" &
                   $"Amprentă: {cert.Thumbprint}"
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.GetCertificateDetails", ex)
            Throw
        End Try
    End Function

End Class