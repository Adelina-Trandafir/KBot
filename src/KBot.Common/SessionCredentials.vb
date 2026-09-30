Option Strict On
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning
Imports System.Text
Imports Microsoft.Win32

''' <summary>
''' The operator's password, kept for two conveniences the operator asked for (slice 0097):
''' <list type="number">
''' <item><b>Silent re-login during work.</b> The server session expires (20 minutes idle, 30
''' at most); instead of the login window every time, K-BOT logs back in by itself while the
''' last login typed by hand is younger than the interval chosen in «Setari → Autentificare».
''' The password lives in THIS process only, encrypted with DPAPI, from the login that
''' succeeded until the process ends.</item>
''' <item><b>«Tine minte parola» on the login window.</b> The password survives a restart of
''' K-BOT but not of Windows: it sits in a VOLATILE registry key under HKCU, which Windows
''' itself discards at sign-out and at restart, encrypted with DPAPI for the current Windows
''' user. Nothing is written to disk in clear, nothing outlives the Windows session.</item>
''' </list>
''' <para>DPAPI is called directly (crypt32) so no package is added. Reads never throw: a key
''' that cannot be read or decrypted means «nothing remembered». Writes are boundaries: log +
''' rethrow.</para>
''' </summary>
<SupportedOSPlatform("windows")>
Public NotInheritable Class SessionCredentials

    Private Sub New()
    End Sub

    ' HKCU\Software\AVACONT\KBot\Session -- created volatile, so gone with the Windows session.
    Private Const VolatileKeyPath As String = "Software\AVACONT\KBot\Session"
    Private Const ValueUser As String = "User"
    Private Const ValueSecret As String = "Secret"

    ' Extra entropy: a blob lifted from this key cannot be decrypted by another DPAPI caller
    ' that does not know it.
    Private Shared ReadOnly Entropy As Byte() = Encoding.UTF8.GetBytes("KBOT.SessionCredentials.0097")

    Private Shared ReadOnly _gate As New Object()
    Private Shared _user As String
    Private Shared _secret As Byte()
    Private Shared _lastInteractiveUtc As DateTime = DateTime.MinValue

    ' ── In-process memory (silent re-login) ─────────────────────────────────

    ''' <summary>
    ''' Keeps the password of a login that just SUCCEEDED, encrypted, for this process.
    ''' <paramref name="interactive"/> = the operator typed it in the login window (or confirmed a
    ''' remembered one there): it restarts the interval after which the window is shown again.
    ''' </summary>
    Public Shared Sub Remember(user As String, password As String, interactive As Boolean)
        If String.IsNullOrEmpty(user) OrElse String.IsNullOrEmpty(password) Then Return
        Try
            Dim blob As Byte() = Protect(password)
            SyncLock _gate
                _user = user
                _secret = blob
                If interactive Then _lastInteractiveUtc = DateTime.UtcNow
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("SessionCredentials.Remember", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The password of this process's last login, if any. Never throws.</summary>
    Public Shared Function TryRecall(ByRef user As String, ByRef password As String) As Boolean
        user = Nothing
        password = Nothing
        Dim u As String
        Dim blob As Byte()
        SyncLock _gate
            u = _user
            blob = _secret
        End SyncLock
        If String.IsNullOrEmpty(u) OrElse blob Is Nothing Then Return False
        Try
            password = Unprotect(blob)
            user = u
            Return Not String.IsNullOrEmpty(password)
        Catch ex As Exception
            GlobalErrorLog.Write("SessionCredentials.TryRecall", ex)
            Return False
        End Try
    End Function

    ''' <summary>When the operator last logged in through the window (UTC); MinValue = never.</summary>
    Public Shared ReadOnly Property LastInteractiveLoginUtc As DateTime
        Get
            SyncLock _gate
                Return _lastInteractiveUtc
            End SyncLock
        End Get
    End Property

    ''' <summary>Drops the in-process copy (a password that the server refused).</summary>
    Public Shared Sub ForgetInProcess()
        SyncLock _gate
            _user = Nothing
            _secret = Nothing
        End SyncLock
    End Sub

    ' ── Windows session (the login window's «Tine minte parola») ──────────

    ''' <summary>
    ''' Stores the pair in the volatile key. Replaces what was there. I/O boundary: log + rethrow.
    ''' </summary>
    Public Shared Sub SaveForWindowsSession(user As String, password As String)
        Try
            If String.IsNullOrEmpty(user) OrElse String.IsNullOrEmpty(password) Then Return
            ' A key created non-volatile by anything earlier would outlive the Windows session;
            ' volatility is decided at creation, so the key is always re-created.
            Registry.CurrentUser.DeleteSubKeyTree(VolatileKeyPath, throwOnMissingSubKey:=False)
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(
                    VolatileKeyPath, writable:=True, options:=RegistryOptions.Volatile)
                key.SetValue(ValueUser, user, RegistryValueKind.String)
                key.SetValue(ValueSecret, Protect(password), RegistryValueKind.Binary)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SessionCredentials.SaveForWindowsSession", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The pair stored for this Windows session, if any. Never throws.</summary>
    Public Shared Function TryLoadForWindowsSession(ByRef user As String, ByRef password As String) As Boolean
        user = Nothing
        password = Nothing
        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(VolatileKeyPath, writable:=False)
                If key Is Nothing Then Return False
                Dim u As String = TryCast(key.GetValue(ValueUser), String)
                Dim blob As Byte() = TryCast(key.GetValue(ValueSecret), Byte())
                If String.IsNullOrEmpty(u) OrElse blob Is Nothing OrElse blob.Length = 0 Then Return False
                Dim p As String = Unprotect(blob)
                If String.IsNullOrEmpty(p) Then Return False
                user = u
                password = p
                Return True
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SessionCredentials.TryLoadForWindowsSession", ex)
            Return False
        End Try
    End Function

    ''' <summary>True when a pair is stored for this Windows session. Never throws.</summary>
    Public Shared Function HasWindowsSessionPassword() As Boolean
        Dim u As String = Nothing, p As String = Nothing
        Return TryLoadForWindowsSession(u, p)
    End Function

    ''' <summary>Deletes the stored pair. Returns whether there was one. I/O boundary.</summary>
    Public Shared Function ForgetWindowsSession() As Boolean
        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(VolatileKeyPath, writable:=False)
                If key Is Nothing Then Return False
            End Using
            Registry.CurrentUser.DeleteSubKeyTree(VolatileKeyPath, throwOnMissingSubKey:=False)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("SessionCredentials.ForgetWindowsSession", ex)
            Throw
        End Try
    End Function

    ' ── DPAPI (crypt32), current Windows user ───────────────────────────────

    <StructLayout(LayoutKind.Sequential)>
    Private Structure DataBlob
        Public cbData As Integer
        Public pbData As IntPtr
    End Structure

    Private Const CRYPTPROTECT_UI_FORBIDDEN As Integer = &H1

    <DllImport("crypt32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Private Shared Function CryptProtectData(ByRef pDataIn As DataBlob, szDataDescr As String,
                                             ByRef pOptionalEntropy As DataBlob, pvReserved As IntPtr,
                                             pPromptStruct As IntPtr, dwFlags As Integer,
                                             ByRef pDataOut As DataBlob) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("crypt32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Private Shared Function CryptUnprotectData(ByRef pDataIn As DataBlob, ppszDataDescr As IntPtr,
                                               ByRef pOptionalEntropy As DataBlob, pvReserved As IntPtr,
                                               pPromptStruct As IntPtr, dwFlags As Integer,
                                               ByRef pDataOut As DataBlob) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function LocalFree(hMem As IntPtr) As IntPtr
    End Function

    Private Shared Function Protect(text As String) As Byte()
        Return Transform(Encoding.UTF8.GetBytes(text), protectData:=True)
    End Function

    Private Shared Function Unprotect(blob As Byte()) As String
        Dim clear As Byte() = Transform(blob, protectData:=False)
        Try
            Return Encoding.UTF8.GetString(clear)
        Finally
            Array.Clear(clear, 0, clear.Length)
        End Try
    End Function

    ' One DPAPI call; the unmanaged buffers are always released.
    Private Shared Function Transform(input As Byte(), protectData As Boolean) As Byte()
        Dim inPtr As IntPtr = Marshal.AllocHGlobal(Math.Max(1, input.Length))
        Dim entPtr As IntPtr = Marshal.AllocHGlobal(Entropy.Length)
        Dim outBlob As New DataBlob()
        Try
            Marshal.Copy(input, 0, inPtr, input.Length)
            Marshal.Copy(Entropy, 0, entPtr, Entropy.Length)
            Dim inBlob As New DataBlob With {.cbData = input.Length, .pbData = inPtr}
            Dim entBlob As New DataBlob With {.cbData = Entropy.Length, .pbData = entPtr}
            Dim ok As Boolean
            If protectData Then
                ok = CryptProtectData(inBlob, "KBOT", entBlob, IntPtr.Zero, IntPtr.Zero,
                                      CRYPTPROTECT_UI_FORBIDDEN, outBlob)
            Else
                ok = CryptUnprotectData(inBlob, IntPtr.Zero, entBlob, IntPtr.Zero, IntPtr.Zero,
                                        CRYPTPROTECT_UI_FORBIDDEN, outBlob)
            End If
            If Not ok Then Throw New Win32Exception(Marshal.GetLastWin32Error())
            Dim result(outBlob.cbData - 1) As Byte
            Marshal.Copy(outBlob.pbData, result, 0, outBlob.cbData)
            Return result
        Finally
            Marshal.FreeHGlobal(inPtr)
            Marshal.FreeHGlobal(entPtr)
            If outBlob.pbData <> IntPtr.Zero Then LocalFree(outBlob.pbData)
        End Try
    End Function

End Class
