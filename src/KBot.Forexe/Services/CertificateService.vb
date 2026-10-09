Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.IO
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' Service for smartcard certificate operations
''' </summary>
Public Class CertificateService

    ''' <summary>
    ''' File name of the last certificate the operator confirmed in CertificateSelectionForm,
    ''' under %APPDATA%\AVACONT\KBot (next to settings.json). Public part only (DER), so
    ''' it holds nothing secret and needs no token to be read back.
    ''' </summary>
    Public Const LastUsedCertificateFileName As String = "last_certificate.cer"

    ''' <summary>Full path of <see cref="LastUsedCertificateFileName"/>.</summary>
    Public Shared Function LastUsedCertificatePath() As String
        Return Path.Combine(SetariFoldere.DirectorSetari(), LastUsedCertificateFileName)
    End Function

    ''' <summary>
    ''' Remembers <paramref name="cert"/> as the last used one. Only the public certificate is
    ''' written; the private key stays on the token.
    ''' </summary>
    Public Shared Sub SaveLastUsedCertificate(cert As X509Certificate2)
        If cert Is Nothing Then Throw New ArgumentNullException(NameOf(cert))
        Try
            Dim filePath As String = LastUsedCertificatePath()
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllBytes(filePath, cert.Export(X509ContentType.Cert))
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.SaveLastUsedCertificate", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Forgets the remembered certificate (slice 0072, «Setări» -> FOREXE): the next
    ''' connection asks again. Returns False when there was nothing to forget.
    ''' </summary>
    ''' <remarks>I/O boundary: logs and rethrows.</remarks>
    Public Shared Function ForgetLastUsedCertificate() As Boolean
        Try
            Dim forgotten As Boolean = False
            Dim filePath As String = LastUsedCertificatePath()
            If File.Exists(filePath) Then
                File.Delete(filePath)
                forgotten = True
            End If
            ' The browser's auto-select rule (written by WorkflowExecutor) names the old certificate.
            If RemoveAutoSelectPolicy() Then forgotten = True
            Return forgotten
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.ForgetLastUsedCertificate", ex)
            Throw
        End Try
    End Function

    Private Const AutoSelectPolicyKey As String = "Software\Policies\Chromium\AutoSelectCertificateForUrls"
    Private Const AutoSelectPolicyUrlMarker As String = "forexe.mfinante.gov.ro"

    ''' <summary>
    ''' Deletes the FOREXE rules from HKCU\Software\Policies\Chromium\AutoSelectCertificateForUrls.
    ''' That key is normally writable only by an administrator (which is why the rule is written
    ''' through an elevated reg.exe), so when the direct delete is refused the same elevated
    ''' route is used. True when at least one rule was removed.
    ''' </summary>
    ''' <remarks>Registry / process boundary: logs and rethrows.</remarks>
    Private Shared Function RemoveAutoSelectPolicy() As Boolean
        Try
            Dim k_names As New List(Of String)
            Using k_key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(AutoSelectPolicyKey, False)
                If k_key Is Nothing Then Return False
                For Each k_name As String In k_key.GetValueNames()
                    Dim k_value As String = k_key.GetValue(k_name)?.ToString()
                    If k_value IsNot Nothing AndAlso k_value.IndexOf(AutoSelectPolicyUrlMarker, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        k_names.Add(k_name)
                    End If
                Next
            End Using
            If k_names.Count = 0 Then Return False

            Dim removed As Boolean = False
            For Each k_name As String In k_names
                Try
                    Using k_key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(AutoSelectPolicyKey, True)
                        If k_key Is Nothing Then Throw New UnauthorizedAccessException()
                        k_key.DeleteValue(k_name, False)
                    End Using
                    removed = True
                Catch ex As Exception When TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is System.Security.SecurityException
                    Dim k_info As New ProcessStartInfo With {
                        .FileName = "reg.exe",
                        .Arguments = $"delete ""HKCU\{AutoSelectPolicyKey}"" /v ""{k_name}"" /f",
                        .Verb = "runas",
                        .UseShellExecute = True,
                        .WindowStyle = ProcessWindowStyle.Hidden
                    }
                    Using k_proc As Process = Process.Start(k_info)
                        k_proc.WaitForExit()
                        If k_proc.ExitCode = 0 Then removed = True
                    End Using
                End Try
            Next
            Return removed
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.RemoveAutoSelectPolicy", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The certificate saved by <see cref="SaveLastUsedCertificate"/>, or Nothing when none was
    ''' saved yet (or the file is unreadable). Built straight from the file: no certificate store,
    ''' no token, no validation of any kind. Whether the token is actually plugged in is found
    ''' out later, when the certificate is used. The instance carries no private key; see
    ''' <see cref="ResolveFromStore"/> when one is needed.
    ''' </summary>
    Public Shared Function LoadLastUsedCertificate() As X509Certificate2
        Try
            Dim filePath As String = LastUsedCertificatePath()
            If Not File.Exists(filePath) Then Return Nothing
            Return New X509Certificate2(File.ReadAllBytes(filePath))
        Catch ex As Exception
            ' A damaged file must not stop the picker from opening: log it, behave as "none saved".
            GlobalErrorLog.Write("CertificateService.LoadLastUsedCertificate", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' The store copy of <paramref name="cert"/> (same thumbprint), which is the one bound to
    ''' the token's private key. Nothing when the store has no such certificate, i.e. the token
    ''' (or its driver) is not present. A single lookup by thumbprint, no key access.
    ''' </summary>
    Public Shared Function ResolveFromStore(cert As X509Certificate2) As X509Certificate2
        If cert Is Nothing Then Throw New ArgumentNullException(NameOf(cert))
        Try
            If cert.HasPrivateKey Then Return cert
            For Each location In {StoreLocation.CurrentUser, StoreLocation.LocalMachine}
                Using store As New X509Store(StoreName.My, location)
                    store.Open(OpenFlags.ReadOnly)
                    Dim found = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly:=False)
                    If found.Count > 0 Then Return found(0)
                End Using
            Next
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.ResolveFromStore", ex)
            Throw
        End Try
    End Function

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
                    ' Only the thumbprint makes two certificates "the same": two certificates with
                    ' one holder name (e.g. an old and a new token) are both listed.
                    Dim reason As String = RejectReason(cert)
                    If reason Is Nothing AndAlso certificates.Any(Function(c) c.Thumbprint = cert.Thumbprint) Then
                        reason = "already listed (same thumbprint)"
                    End If
                    CertificateLog.Write($"{location} | {GetCommonName(cert)} | SN {cert.SerialNumber} | " &
                                         $"{cert.NotBefore:dd.MM.yyyy}-{cert.NotAfter:dd.MM.yyyy} | " &
                                         If(reason Is Nothing, "ACCEPTED", "REJECTED: " & reason))
                    If reason Is Nothing Then certificates.Add(cert)
                Next
            End Using
        Catch ex As Exception
            ' Deliberate swallow: an unreadable store must not stop the other store from being
            ' enumerated; logged, then carry on.
            GlobalErrorLog.Write("CertificateService.AddCertificatesFromStore", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Why <paramref name="cert"/> does not belong in the picker, or Nothing when it does.
    ''' Every rule returns its own reason, so certificate_filter.log says which one fired.
    ''' Public for the DevHarness certificate inventory, which prints the picker's verdict per certificate.
    ''' </summary>
    Public Shared Function RejectReason(cert As X509Certificate2) As String
        If Not cert.HasPrivateKey Then Return "no private key"
        If DateTime.Now > cert.NotAfter Then Return "expired"
        If DateTime.Now < cert.NotBefore Then Return "not valid yet"
        Return HardwareRejectReason(cert)
    End Function

    ''' <summary>CryptoAPI NTE_BAD_KEYSET: the key container named by the certificate does not exist.</summary>
    Private Const NTE_BAD_KEYSET As Integer = &H80090016

    ''' <summary>
    ''' The hardware rules: the key lives on a token/smart card (or carries Client Authentication),
    ''' is not exportable and is not in a Microsoft software provider. Nothing = accepted.
    ''' USING releases the smart card handle right away, so the PIN is not locked.
    ''' </summary>
    Private Shared Function HardwareRejectReason(cert As X509Certificate2) As String
        Try
            Using rsaPrivateKey As RSA = cert.GetRSAPrivateKey()
                If rsaPrivateKey Is Nothing Then Return "private key is not RSA (" & cert.PublicKey.Oid.FriendlyName & ")"

                Dim providerName As String = ""
                Dim isNonExportable As Boolean = False

                ' A. Provider and export policy.
                Dim cngKey As RSACng = TryCast(rsaPrivateKey, RSACng)
                If cngKey IsNot Nothing Then
                    providerName = cngKey.Key.Provider.Provider
                    ' AllowPlaintextExport bit = 0 means the key cannot leave the device.
                    isNonExportable = (cngKey.Key.ExportPolicy And CngExportPolicies.AllowPlaintextExport) = 0
                Else
                    Dim capiKey As RSACryptoServiceProvider = TryCast(rsaPrivateKey, RSACryptoServiceProvider)
                    If capiKey IsNot Nothing Then
                        providerName = capiKey.CspKeyContainerInfo.ProviderName
                        isNonExportable = Not capiKey.CspKeyContainerInfo.Exportable
                    End If
                End If

                Return ClassifyProvider(cert, providerName, isNonExportable)

            End Using ' <-- the connection to the card closes here for this certificate

        Catch ex As CryptographicException When ex.HResult = NTE_BAD_KEYSET
            ' "Keyset does not exist" = a STALE store copy: certutil shows it as "Missing stored keyset".
            ' The store entry names a key container the token no longer offers (token not plugged in,
            ' or an old copy left by an earlier insertion). Not selectable; say so, do not list it.
            GlobalErrorLog.Write("CertificateService.HardwareRejectReason", ex)
            Dim k_provider As String = StoredKeyProviderName(cert)
            Return "stale store copy: key container not found" &
                   If(String.IsNullOrEmpty(k_provider), "", " (provider '" & k_provider & "')") &
                   " - token not plugged in or certificate not propagated from it"
        Catch ex As Exception
            ' Deliberate swallow: an error here (e.g. token not plugged in, missing driver) makes
            ' the certificate ineligible instead of crashing the picker; still logged globally.
            GlobalErrorLog.Write("CertificateService.HardwareRejectReason", ex)
            Return "error reading the private key: " & ex.Message
        End Try
    End Function

    ''' <summary>
    ''' Rules B-F of the picker: software-provider blacklist, hardware whitelist / Client
    ''' Authentication, name filter. Nothing = accepted.
    ''' </summary>
    Private Shared Function ClassifyProvider(cert As X509Certificate2, providerName As String, isNonExportable As Boolean) As String
        Try
            Dim isHardwareProvider As Boolean = False
            If providerName Is Nothing Then providerName = ""
                ' B. Blacklist: the standard Microsoft software providers.
                Dim microsoftSoftwareProviders As String() = {
                    "Microsoft Strong Cryptographic Provider",
                    "Microsoft Enhanced Cryptographic Provider",
                    "Microsoft Base Cryptographic Provider",
                    "Microsoft Software Key Storage Provider"
                }

                For Each msProvider In microsoftSoftwareProviders
                    If providerName.Equals(msProvider, StringComparison.OrdinalIgnoreCase) Then
                        Return "software provider '" & providerName & "'"
                    End If
                Next

                ' C. Whitelist: known hardware providers.
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

                ' D. Extended Key Usage: Client Authentication (1.3.6.1.5.5.7.3.2). Some certificates
                ' (e.g. cloud ones) do not show a classic hardware provider but carry this flag.
                Dim hasClientAuth As Boolean = False
                For Each extension In cert.Extensions
                    If TypeOf extension Is X509EnhancedKeyUsageExtension Then
                        Dim ekuExt As X509EnhancedKeyUsageExtension = DirectCast(extension, X509EnhancedKeyUsageExtension)
                        For Each oid In ekuExt.EnhancedKeyUsages
                            If oid.Value = "1.3.6.1.5.5.7.3.2" Then
                                hasClientAuth = True
                                Exit For
                            End If
                        Next
                    End If
                Next

                ' E. Final rule: not exportable AND (known hardware provider OR Client Auth).
                If Not isNonExportable Then Return "exportable key (provider '" & providerName & "')"
                If Not (isHardwareProvider OrElse hasClientAuth) Then
                    Return "provider '" & providerName & "' is not a known token and there is no Client Authentication"
                End If

                ' F. Name filter: no localhost / test certificates.
                Dim cnMatch As Match = Regex.Match(cert.Subject, "CN=([^,]+)")
                Dim cn As String = If(cnMatch.Success, cnMatch.Groups(1).Value, "")

                If cn.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   cn.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return "name contains localhost/test"
                End If

                Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateService.ClassifyProvider", ex)
            Return "error reading the private key: " & ex.Message
        End Try
    End Function

    <System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError:=True)>
    Private Shared Function CertGetCertificateContextProperty(pCertContext As IntPtr, dwPropId As UInteger, pvData As IntPtr, ByRef pcbData As UInteger) As Boolean
    End Function

    ''' <summary>
    ''' Name of the key provider recorded on the certificate (CERT_KEY_PROV_INFO_PROP_ID), read from
    ''' the certificate context without opening the key. Nothing when none is recorded.
    ''' </summary>
    Private Shared Function StoredKeyProviderName(cert As X509Certificate2) As String
        Const CERT_KEY_PROV_INFO_PROP_ID As UInteger = 2UI
        Dim k_size As UInteger = 0UI
        If Not CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, IntPtr.Zero, k_size) OrElse k_size = 0UI Then Return Nothing
        Dim k_buffer As IntPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(CInt(k_size))
        Try
            If Not CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, k_buffer, k_size) Then Return Nothing
            ' CRYPT_KEY_PROV_INFO: pwszContainerName, then pwszProvName.
            Dim k_namePtr As IntPtr = System.Runtime.InteropServices.Marshal.ReadIntPtr(k_buffer, IntPtr.Size)
            Return System.Runtime.InteropServices.Marshal.PtrToStringUni(k_namePtr)
        Finally
            System.Runtime.InteropServices.Marshal.FreeHGlobal(k_buffer)
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