Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' The key of a local backup kept ON THIS PC, protected by the Windows account of the operator (DPAPI, current-user scope):
''' no passphrase has to be typed for a run that starts by itself. The file is useless on another PC or under another account;
''' the passphrase (in the password manager) is what recovers the copies there. Slice ME02-03.
''' </summary>
Public NotInheritable Class BackupKeyring

    Private Shared ReadOnly Entropy As Byte() = Encoding.UTF8.GetBytes("KBotAdmin.ME02.key")

    Private Sub New()
    End Sub

    Private Shared Function KeyFile(k_storeId As String) As String
        Return Path.Combine(AppContext.BaseDirectory, "backup_key_" & k_storeId & ".dat")
    End Function

    Public Shared Function Exists(k_storeId As String) As Boolean
        Return k_storeId <> String.Empty AndAlso File.Exists(KeyFile(k_storeId))
    End Function

    Public Shared Sub Save(k_store As BackupStore)
        Try
            Dim k_protected As Byte() = ProtectedData.Protect(k_store.Keys.ToBytes(), Entropy, DataProtectionScope.CurrentUser)
            File.WriteAllBytes(KeyFile(k_store.StoreId), k_protected)
        Catch ex As Exception
            GlobalErrorLog.Write("BackupKeyring.Save", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The keys saved for this store, or Nothing when there is no file or Windows cannot open it (another account, another PC).</summary>
    Public Shared Function TryLoad(k_storeId As String) As BackupKeys
        Try
            If Not Exists(k_storeId) Then Return Nothing
            Dim k_bytes As Byte() = ProtectedData.Unprotect(File.ReadAllBytes(KeyFile(k_storeId)), Entropy, DataProtectionScope.CurrentUser)
            Return BackupKeys.FromBytes(k_bytes)
        Catch ex As CryptographicException
            GlobalErrorLog.Write("BackupKeyring.TryLoad", ex)
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("BackupKeyring.TryLoad", ex)
            Throw
        End Try
    End Function

    Public Shared Sub Forget(k_storeId As String)
        Try
            If Exists(k_storeId) Then File.Delete(KeyFile(k_storeId))
        Catch ex As Exception
            GlobalErrorLog.Write("BackupKeyring.Forget", ex)
            Throw
        End Try
    End Sub

End Class
