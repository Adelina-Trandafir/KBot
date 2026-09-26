Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Forexe

' FOREXE, read-only: dumps EVERY certificate of EVERY store on this PC (CurrentUser and
' LocalMachine, all store names), token or not, to <AppDir>\Logs\certificate_inventory_<stamp>.txt.
' For each one: identity, validity, EKU / key usage, where its private key is said to live
' (read from the certificate property, without opening the key), then the key itself
' (algorithm, provider, exportable, hardware flags) and the verdict of the KBot picker's filter.
' Built to explain why a certificate Chrome offers is missing from CertificateSelectionForm.
Public NotInheritable Class CertificateInventoryTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — Inventar certificate (toate, în fișier)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "FOREXE"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Dim logsDir As String = LogPaths.LogsDirectory()
        Directory.CreateDirectory(logsDir)
        Dim filePath As String = Path.Combine(logsDir, $"certificate_inventory_{DateTime.Now:yyyyMMdd_HHmmss}.txt")

        Dim counts As (Total As Integer, WithKey As Integer, Accepted As Integer) =
            Await Task.Run(Function() WriteInventory(filePath, context, ct), ct)

        context.Log($"Inventar scris: {filePath}")
        Return HarnessTestResult.Passed(
            $"{counts.Total} certificate, {counts.WithKey} cu cheie privată, {counts.Accepted} acceptate de selector. Fișier: {filePath}")
    End Function

    Private Shared Function WriteInventory(filePath As String, context As HarnessContext, ct As CancellationToken) As (Integer, Integer, Integer)
        Dim sb As New StringBuilder()
        sb.AppendLine($"KBot certificate inventory  {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        sb.AppendLine($"Machine: {Environment.MachineName}   User: {Environment.UserDomainName}\{Environment.UserName}")
        sb.AppendLine("Picker filter = CertificateService.RejectReason (the same rules CertificateSelectionForm uses).")
        sb.AppendLine("NOTE: the picker only reads the My (Personal) store of CurrentUser and LocalMachine.")
        sb.AppendLine()

        Dim total, withKey, accepted As Integer
        Dim storeNames As StoreName() = DirectCast([Enum].GetValues(GetType(StoreName)), StoreName())

        For Each location In {StoreLocation.CurrentUser, StoreLocation.LocalMachine}
            For Each storeName In storeNames
                ct.ThrowIfCancellationRequested()
                Dim header As String = $"{location}\{storeName}"
                Try
                    Using store As New X509Store(storeName, location)
                        store.Open(OpenFlags.ReadOnly Or OpenFlags.OpenExistingOnly)
                        sb.AppendLine(New String("="c, 100))
                        sb.AppendLine($"STORE {header}   ({store.Certificates.Count} certificates)")
                        sb.AppendLine(New String("="c, 100))
                        Dim index As Integer = 0
                        For Each cert As X509Certificate2 In store.Certificates
                            index += 1
                            total += 1
                            If cert.HasPrivateKey Then withKey += 1
                            Dim isAccepted As Boolean = AppendCertificate(sb, cert, index, storeName = StoreName.My)
                            If isAccepted Then accepted += 1
                        Next
                        sb.AppendLine()
                    End Using
                Catch ex As Exception
                    ' A store that does not exist or cannot be opened is a line in the report, not a stop.
                    sb.AppendLine($"STORE {header}: cannot open ({ex.GetType().Name}: {ex.Message})")
                    sb.AppendLine()
                End Try
            Next
            context.Progress.Report(New HarnessProgressInfo(If(location = StoreLocation.CurrentUser, 50, 100), $"{location} gata"))
        Next

        sb.AppendLine($"TOTAL: {total} certificates, {withKey} with a private key, {accepted} accepted by the picker filter (My store only).")
        File.WriteAllText(filePath, sb.ToString(), New UTF8Encoding(True))
        Return (total, withKey, accepted)
    End Function

    ''' <summary>One certificate block. Returns True when the picker filter accepts it (My store only).</summary>
    Private Shared Function AppendCertificate(sb As StringBuilder, cert As X509Certificate2, index As Integer, isMyStore As Boolean) As Boolean
        sb.AppendLine($"--- #{index} {CertificateService.GetCommonName(cert)}")
        Line(sb, "Subject", Function() cert.Subject)
        Line(sb, "Issuer", Function() cert.Issuer)
        Line(sb, "Serial number", Function() cert.SerialNumber)
        Line(sb, "Thumbprint", Function() cert.Thumbprint)
        Line(sb, "Valid", Function() $"{cert.NotBefore:dd.MM.yyyy HH:mm} -> {cert.NotAfter:dd.MM.yyyy HH:mm}" &
                                        If(DateTime.Now > cert.NotAfter, "  [EXPIRED]", If(DateTime.Now < cert.NotBefore, "  [NOT YET VALID]", "")))
        Line(sb, "Friendly name", Function() cert.FriendlyName)
        Line(sb, "Version", Function() cert.Version.ToString())
        Line(sb, "Signature algorithm", Function() $"{cert.SignatureAlgorithm.FriendlyName} ({cert.SignatureAlgorithm.Value})")
        Line(sb, "Public key", Function() $"{cert.PublicKey.Oid.FriendlyName} ({cert.PublicKey.Oid.Value}), {PublicKeySize(cert)} bits")
        Line(sb, "Enhanced key usage", Function() Eku(cert))
        Line(sb, "Key usage", Function() KeyUsage(cert))
        Line(sb, "Chain builds", Function() ChainStatus(cert))
        Line(sb, "Has private key", Function() cert.HasPrivateKey.ToString())
        Line(sb, "Key location (property)", Function() KeyProviderInfo(cert))

        If cert.HasPrivateKey Then
            Line(sb, "Private key (opened)", Function() PrivateKeyDetails(cert))
        End If

        Dim verdict As String = Nothing
        If isMyStore Then
            Line(sb, "PICKER VERDICT", Function()
                                           verdict = CertificateService.RejectReason(cert)
                                           Return If(verdict Is Nothing, "ACCEPTED", "REJECTED: " & verdict)
                                       End Function)
        End If
        sb.AppendLine()
        Return isMyStore AndAlso verdict Is Nothing
    End Function

    ''' <summary>One "name: value" line; a failing read becomes the error text, never a stop.</summary>
    Private Shared Sub Line(sb As StringBuilder, label As String, value As Func(Of String))
        Dim text As String
        Try
            text = value()
        Catch ex As Exception
            text = $"<error {ex.GetType().Name}: {ex.Message}>"
        End Try
        sb.AppendLine($"    {label,-26}: {text}")
    End Sub

    Private Shared Function PublicKeySize(cert As X509Certificate2) As Integer
        Using rsa As RSA = cert.GetRSAPublicKey()
            If rsa IsNot Nothing Then Return rsa.KeySize
        End Using
        Using ec As ECDsa = cert.GetECDsaPublicKey()
            If ec IsNot Nothing Then Return ec.KeySize
        End Using
        Return 0
    End Function

    Private Shared Function Eku(cert As X509Certificate2) As String
        Dim parts As New List(Of String)()
        For Each ext In cert.Extensions
            Dim e As X509EnhancedKeyUsageExtension = TryCast(ext, X509EnhancedKeyUsageExtension)
            If e Is Nothing Then Continue For
            For Each oid In e.EnhancedKeyUsages
                parts.Add($"{oid.FriendlyName} ({oid.Value})")
            Next
        Next
        Return If(parts.Count = 0, "<none: any purpose>", String.Join("; ", parts))
    End Function

    Private Shared Function KeyUsage(cert As X509Certificate2) As String
        For Each ext In cert.Extensions
            Dim k As X509KeyUsageExtension = TryCast(ext, X509KeyUsageExtension)
            If k IsNot Nothing Then Return k.KeyUsages.ToString()
        Next
        Return "<none>"
    End Function

    Private Shared Function ChainStatus(cert As X509Certificate2) As String
        Using chain As New X509Chain()
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck
            Dim ok As Boolean = chain.Build(cert)
            If ok Then Return $"yes ({chain.ChainElements.Count} elements)"
            Dim problems As New List(Of String)()
            For Each s In chain.ChainStatus
                problems.Add(s.Status.ToString())
            Next
            Return "no: " & String.Join(", ", problems)
        End Using
    End Function

    ' ---------------------------------------------------------------------------------------
    ' Key location read from CERT_KEY_PROV_INFO_PROP_ID: provider and container as Windows
    ' recorded them, WITHOUT opening the key (no token access, no PIN, no "insert card" prompt).
    ' A cloud / remote-signing certificate shows its own provider name here.
    ' ---------------------------------------------------------------------------------------

    Private Const CERT_KEY_PROV_INFO_PROP_ID As Integer = 2

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure CRYPT_KEY_PROV_INFO
        <MarshalAs(UnmanagedType.LPWStr)> Public pwszContainerName As String
        <MarshalAs(UnmanagedType.LPWStr)> Public pwszProvName As String
        Public dwProvType As Integer
        Public dwFlags As Integer
        Public cProvParam As Integer
        Public rgProvParam As IntPtr
        Public dwKeySpec As Integer
    End Structure

    <DllImport("crypt32.dll", SetLastError:=True)>
    Private Shared Function CertGetCertificateContextProperty(pCertContext As IntPtr, dwPropId As Integer, pvData As IntPtr, ByRef pcbData As Integer) As Boolean
    End Function

    Private Shared Function KeyProviderInfo(cert As X509Certificate2) As String
        Dim size As Integer = 0
        If Not CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, IntPtr.Zero, size) Then
            Return $"<no key-provider property, Win32 error {Marshal.GetLastWin32Error()}>"
        End If
        Dim buffer As IntPtr = Marshal.AllocHGlobal(size)
        Try
            If Not CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, buffer, size) Then
                Return $"<read failed, Win32 error {Marshal.GetLastWin32Error()}>"
            End If
            Dim info As CRYPT_KEY_PROV_INFO = Marshal.PtrToStructure(Of CRYPT_KEY_PROV_INFO)(buffer)
            ' dwProvType 0 = CNG key storage provider; anything else = legacy CAPI CSP type.
            Dim kind As String = If(info.dwProvType = 0, "CNG KSP", $"CAPI CSP type {info.dwProvType}")
            Return $"provider '{info.pwszProvName}' [{kind}], container '{info.pwszContainerName}', keySpec {info.dwKeySpec}, flags 0x{info.dwFlags:X}"
        Finally
            Marshal.FreeHGlobal(buffer)
        End Try
    End Function

    ''' <summary>
    ''' Opens the private key (RSA or ECDSA) and reads what the filter reads. This CAN talk to
    ''' the token; with the token absent the driver may show its own "insert card" window.
    ''' </summary>
    Private Shared Function PrivateKeyDetails(cert As X509Certificate2) As String
        Using rsa As RSA = cert.GetRSAPrivateKey()
            If rsa IsNot Nothing Then Return "RSA " & DescribeKey(rsa)
        End Using
        Using ec As ECDsa = cert.GetECDsaPrivateKey()
            If ec IsNot Nothing Then Return "ECDSA " & DescribeKey(ec)
        End Using
        Return "<neither RSA nor ECDSA>"
    End Function

    Private Shared Function DescribeKey(key As AsymmetricAlgorithm) As String
        Dim cng As CngKey = Nothing
        If TypeOf key Is RSACng Then cng = DirectCast(key, RSACng).Key
        If TypeOf key Is ECDsaCng Then cng = DirectCast(key, ECDsaCng).Key
        If cng IsNot Nothing Then
            Dim flags As New List(Of String)()
            Try
                If cng.IsMachineKey Then flags.Add("machine key")
            Catch
            End Try
            Dim uiPolicy As String
            Try
                uiPolicy = cng.UIPolicy.ProtectionLevel.ToString()
            Catch ex As Exception
                uiPolicy = "?"
            End Try
            Return $"CNG {key.KeySize} bits, provider '{cng.Provider.Provider}', exportPolicy {cng.ExportPolicy}, " &
                   $"UI protection {uiPolicy}, uniqueName '{cng.UniqueName}'" &
                   If(flags.Count > 0, ", " & String.Join(", ", flags), "")
        End If

        Dim capi As RSACryptoServiceProvider = TryCast(key, RSACryptoServiceProvider)
        If capi IsNot Nothing Then
            Dim i As CspKeyContainerInfo = capi.CspKeyContainerInfo
            Return $"CAPI {key.KeySize} bits, provider '{i.ProviderName}', exportable {i.Exportable}, " &
                   $"hardware {i.HardwareDevice}, removable {i.Removable}, accessible {i.Accessible}, container '{i.KeyContainerName}'"
        End If

        Return $"{key.GetType().FullName} {key.KeySize} bits"
    End Function

End Class
