Option Strict On
Imports System.IO
Imports System.IO.Compression
Imports System.Security.Cryptography
Imports System.Text

''' <summary>A failure of the local backup that the operator can read (wrong passphrase, damaged file, store in use...).</summary>
Public Class BackupException
    Inherits Exception

    Public Sub New(k_message As String)
        MyBase.New(k_message)
    End Sub

    Public Sub New(k_message As String, k_inner As Exception)
        MyBase.New(k_message, k_inner)
    End Sub
End Class

''' <summary>The two keys derived from the passphrase: one encrypts, one names objects (docs/ME02_FORMAT.md).</summary>
Public NotInheritable Class BackupKeys

    Public Sub New(k_enc As Byte(), k_nameKey As Byte())
        If k_enc Is Nothing OrElse k_enc.Length <> 32 Then Throw New ArgumentException("Cheia de criptare trebuie să aibă 32 de octeți.", NameOf(k_enc))
        If k_nameKey Is Nothing OrElse k_nameKey.Length <> 32 Then Throw New ArgumentException("Cheia de denumire trebuie să aibă 32 de octeți.", NameOf(k_nameKey))
        Enc = k_enc
        NameKey = k_nameKey
    End Sub

    Public ReadOnly Property Enc As Byte()
    Public ReadOnly Property NameKey As Byte()

    ''' <summary>The 64 bytes, for the key file protected by Windows (BackupKeyring).</summary>
    Public Function ToBytes() As Byte()
        Dim k_all(63) As Byte
        Buffer.BlockCopy(Enc, 0, k_all, 0, 32)
        Buffer.BlockCopy(NameKey, 0, k_all, 32, 32)
        Return k_all
    End Function

    Public Shared Function FromBytes(k_bytes As Byte()) As BackupKeys
        If k_bytes Is Nothing OrElse k_bytes.Length <> 64 Then Throw New BackupException("Fișierul cu cheia este stricat.")
        Dim k_enc(31) As Byte
        Dim k_name(31) As Byte
        Buffer.BlockCopy(k_bytes, 0, k_enc, 0, 32)
        Buffer.BlockCopy(k_bytes, 32, k_name, 0, 32)
        Return New BackupKeys(k_enc, k_name)
    End Function
End Class

''' <summary>
''' The cryptography of the local backup (slice ME02-03), all of it standard .NET: PBKDF2-SHA256 and HKDF for the keys,
''' AES-256-GCM for sealing, HMAC-SHA256 for names, Brotli for size. Nothing here is invented.
''' </summary>
Public NotInheritable Class BackupCrypto

    Public Const KindObject As Byte = 1
    Public Const KindSnapshot As Byte = 2
    Public Const KindCheck As Byte = 3
    Public Const DefaultIterations As Integer = 600000
    Private Const NonceSize As Integer = 12
    Private Const TagSize As Integer = 16

    Private Sub New()
    End Sub

    Public Shared Function NewSalt() As Byte()
        Dim k_salt(15) As Byte
        RandomNumberGenerator.Fill(k_salt)
        Return k_salt
    End Function

    Public Shared Function DeriveKeys(k_passphrase As String, k_salt As Byte(), k_iterations As Integer) As BackupKeys
        If String.IsNullOrEmpty(k_passphrase) Then Throw New BackupException("Scrieți parola copiilor locale.")
        If k_iterations < 10000 Then Throw New BackupException("Antetul copiei locale are un număr de iterații prea mic.")
        Dim k_master As Byte() = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(k_passphrase), k_salt, k_iterations, HashAlgorithmName.SHA256, 32)
        Dim k_enc As Byte() = HKDF.DeriveKey(HashAlgorithmName.SHA256, k_master, 32, Nothing, Encoding.ASCII.GetBytes("kbk1-enc"))
        Dim k_name As Byte() = HKDF.DeriveKey(HashAlgorithmName.SHA256, k_master, 32, Nothing, Encoding.ASCII.GetBytes("kbk1-name"))
        CryptographicOperations.ZeroMemory(k_master)
        Return New BackupKeys(k_enc, k_name)
    End Function

    ''' <summary>The name of an object: HMAC-SHA256 of its (uncompressed) content, 64 hex characters.</summary>
    Public Shared Function ObjectName(k_plain As Byte(), k_keys As BackupKeys) As String
        Return Convert.ToHexString(HMACSHA256.HashData(k_keys.NameKey, k_plain)).ToLowerInvariant()
    End Function

    ''' <summary>Compresses (Brotli, fast) unless that does not help; the first byte says which: 0 = raw, 1 = Brotli.</summary>
    Public Shared Function Pack(k_plain As Byte()) As Byte()
        Using k_out As New MemoryStream(k_plain.Length \ 2 + 16)
            k_out.WriteByte(1)
            Using k_brotli As New BrotliStream(k_out, CompressionLevel.Fastest, True)
                k_brotli.Write(k_plain, 0, k_plain.Length)
            End Using
            If k_out.Length < k_plain.Length + 1 Then Return k_out.ToArray()
        End Using
        Dim k_raw(k_plain.Length) As Byte
        k_raw(0) = 0
        Buffer.BlockCopy(k_plain, 0, k_raw, 1, k_plain.Length)
        Return k_raw
    End Function

    Public Shared Function Unpack(k_packed As Byte()) As Byte()
        If k_packed Is Nothing OrElse k_packed.Length = 0 Then Throw New BackupException("Un obiect al copiei locale este gol.")
        If k_packed(0) = 0 Then
            Dim k_raw(k_packed.Length - 2) As Byte
            Buffer.BlockCopy(k_packed, 1, k_raw, 0, k_raw.Length)
            Return k_raw
        End If
        If k_packed(0) <> 1 Then Throw New BackupException("Un obiect al copiei locale are un format necunoscut.")
        Using k_in As New MemoryStream(k_packed, 1, k_packed.Length - 1)
            Using k_brotli As New BrotliStream(k_in, CompressionMode.Decompress)
                Using k_out As New MemoryStream()
                    k_brotli.CopyTo(k_out)
                    Return k_out.ToArray()
                End Using
            End Using
        End Using
    End Function

    ''' <summary>kind | nonce | ciphertext | tag. The label (the file's own name) is authenticated with the content.</summary>
    Public Shared Function Seal(k_kind As Byte, k_plain As Byte(), k_label As String, k_keys As BackupKeys) As Byte()
        Dim k_nonce(NonceSize - 1) As Byte
        RandomNumberGenerator.Fill(k_nonce)
        Dim k_cipher(k_plain.Length - 1) As Byte
        Dim k_tag(TagSize - 1) As Byte
        Using k_aes As New AesGcm(k_keys.Enc, TagSize)
            k_aes.Encrypt(k_nonce, k_plain, k_cipher, k_tag, Associated(k_kind, k_label))
        End Using
        Dim k_file(1 + NonceSize + k_cipher.Length + TagSize - 1) As Byte
        k_file(0) = k_kind
        Buffer.BlockCopy(k_nonce, 0, k_file, 1, NonceSize)
        Buffer.BlockCopy(k_cipher, 0, k_file, 1 + NonceSize, k_cipher.Length)
        Buffer.BlockCopy(k_tag, 0, k_file, 1 + NonceSize + k_cipher.Length, TagSize)
        Return k_file
    End Function

    ''' <summary>Opens a sealed file; a wrong key, a moved file or any changed byte raises a BackupException.</summary>
    Public Shared Function Open(k_expectedKind As Byte, k_file As Byte(), k_label As String, k_keys As BackupKeys) As Byte()
        If k_file Is Nothing OrElse k_file.Length < 1 + NonceSize + TagSize Then Throw New BackupException("Un fișier al copiei locale este prea scurt.")
        If k_file(0) <> k_expectedKind Then Throw New BackupException("Un fișier al copiei locale este de alt fel decât se aștepta.")
        Dim k_nonce(NonceSize - 1) As Byte
        Buffer.BlockCopy(k_file, 1, k_nonce, 0, NonceSize)
        Dim k_length As Integer = k_file.Length - 1 - NonceSize - TagSize
        Dim k_cipher(k_length - 1) As Byte
        Buffer.BlockCopy(k_file, 1 + NonceSize, k_cipher, 0, k_length)
        Dim k_tag(TagSize - 1) As Byte
        Buffer.BlockCopy(k_file, 1 + NonceSize + k_length, k_tag, 0, TagSize)
        Dim k_plain(k_length - 1) As Byte
        Try
            Using k_aes As New AesGcm(k_keys.Enc, TagSize)
                k_aes.Decrypt(k_nonce, k_cipher, k_tag, k_plain, Associated(k_expectedKind, k_label))
            End Using
        Catch ex As CryptographicException
            Throw New BackupException("Un fișier al copiei locale nu se poate deschide (parolă greșită sau fișier stricat).", ex)
        End Try
        Return k_plain
    End Function

    Private Shared Function Associated(k_kind As Byte, k_label As String) As Byte()
        Dim k_text As Byte() = Encoding.UTF8.GetBytes(If(k_label, String.Empty))
        Dim k_all(k_text.Length) As Byte
        k_all(0) = k_kind
        Buffer.BlockCopy(k_text, 0, k_all, 1, k_text.Length)
        Return k_all
    End Function

End Class
