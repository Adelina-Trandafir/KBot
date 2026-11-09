Option Strict On

''' <summary>
''' Which certificates of the store are offered for the ANAF login (slice 00EF-05). Ported rule for rule from
''' the old <c>EF.EXE</c> (<c>Surse\EF_SURSA\Program.vb</c>, <c>LoadCertificates</c>, read 06.10.2026): a
''' qualified certificate on a hardware token or card, never a software key and never a test certificate.
''' Pure: it takes the facts about one certificate and answers yes or no.
''' </summary>
Public NotInheritable Class CertificateRules

    ''' <summary>Extended Key Usage «Client Authentication».</summary>
    Public Const ClientAuthenticationOid As String = "1.3.6.1.5.5.7.3.2"

    ' Key stores that are software, so the key can be copied: refused outright.
    Private Shared ReadOnly MicrosoftSoftwareProviders As String() = {
        "Microsoft Strong Cryptographic Provider",
        "Microsoft Enhanced Cryptographic Provider",
        "Microsoft Base Cryptographic Provider",
        "Microsoft Software Key Storage Provider"}

    ' Parts of the provider name of the known token / card makers.
    Private Shared ReadOnly HardwareProviderMarks As String() = {
        "Smart Card", "Token", "Athena", "SafeNet", "eToken",
        "Aladdin", "Gemalto", "Feitian", "JaCarta", "Oberthur",
        "ePass", "Certum", "Cryptotech", "OpenSC", "Siemens CardOS"}

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The key cannot be exported, is not in a Microsoft software store, the store is a known hardware maker or
    ''' the certificate carries Client Authentication, and the name is not a test one (<c>test</c> /
    ''' <c>localhost</c> anywhere in the CN).
    ''' </summary>
    Public Shared Function IsEligible(k_providerName As String, k_nonExportable As Boolean,
                                      k_hasClientAuth As Boolean, k_commonName As String) As Boolean
        If Not k_nonExportable Then Return False
        If IsMicrosoftSoftware(k_providerName) Then Return False
        If Not (IsKnownHardware(k_providerName) OrElse k_hasClientAuth) Then Return False
        Dim cn As String = If(k_commonName, String.Empty)
        If cn.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0 Then Return False
        If cn.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 Then Return False
        Return True
    End Function

    Public Shared Function IsMicrosoftSoftware(k_providerName As String) As Boolean
        For Each k_known As String In MicrosoftSoftwareProviders
            If String.Equals(k_providerName, k_known, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Public Shared Function IsKnownHardware(k_providerName As String) As Boolean
        If String.IsNullOrEmpty(k_providerName) Then Return False
        For Each k_mark As String In HardwareProviderMarks
            If k_providerName.IndexOf(k_mark, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next
        Return False
    End Function

End Class
