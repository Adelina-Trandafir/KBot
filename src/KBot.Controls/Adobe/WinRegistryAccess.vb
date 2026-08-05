Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Win32
Imports KBot.Common

' Real registry I/O behind IRegistryAccess (slice 0023). This is the risky boundary — every
' method logs to GlobalErrorLog and RE-THROWS (house rule), EXCEPT that a missing key/value in
' Read is a normal ABSENT result, not an error. Only HKCU is ever written here; HKLM policy
' writes go through reg.exe with elevation, never this type.
<System.Runtime.Versioning.SupportedOSPlatform("windows")>
Public NotInheritable Class WinRegistryAccess
    Implements IRegistryAccess

    Public Function Read(path As String, name As String) As RegistryValueSnapshot Implements IRegistryAccess.Read
        Try
            Dim hive As RegistryKey = Nothing
            Dim subPath As String = Nothing
            SplitPath(path, hive, subPath)
            Using k As RegistryKey = hive.OpenSubKey(subPath, writable:=False)
                If k Is Nothing Then Return RegistryValueSnapshot.AbsentSnap(path, name)
                Dim present As Boolean = k.GetValueNames().Any(
                    Function(n) String.Equals(n, name, StringComparison.OrdinalIgnoreCase))
                If Not present Then Return RegistryValueSnapshot.AbsentSnap(path, name)
                Dim kind As RegistryValueKind = k.GetValueKind(name)
                Dim val As Object = k.GetValue(name, Nothing, RegistryValueOptions.DoNotExpandEnvironmentNames)
                Return RegistryValueSnapshot.PresentSnap(path, name, kind, val)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("WinRegistryAccess.Read", ex)
            Throw
        End Try
    End Function

    Public Sub Write(path As String, name As String, kind As RegistryValueKind, value As Object) Implements IRegistryAccess.Write
        Try
            Dim hive As RegistryKey = Nothing
            Dim subPath As String = Nothing
            SplitPath(path, hive, subPath)
            Using k As RegistryKey = hive.CreateSubKey(subPath, writable:=True)
                If k Is Nothing Then Throw New InvalidOperationException($"Nu pot deschide/crea cheia: {path}")
                k.SetValue(name, value, kind)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("WinRegistryAccess.Write", ex)
            Throw
        End Try
    End Sub

    Public Sub DeleteValue(path As String, name As String) Implements IRegistryAccess.DeleteValue
        Try
            Dim hive As RegistryKey = Nothing
            Dim subPath As String = Nothing
            SplitPath(path, hive, subPath)
            Using k As RegistryKey = hive.OpenSubKey(subPath, writable:=True)
                If k IsNot Nothing Then k.DeleteValue(name, throwOnMissingValue:=False)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("WinRegistryAccess.DeleteValue", ex)
            Throw
        End Try
    End Sub

    Public Function KeyExists(path As String) As Boolean Implements IRegistryAccess.KeyExists
        Try
            Dim hive As RegistryKey = Nothing
            Dim subPath As String = Nothing
            SplitPath(path, hive, subPath)
            Using k As RegistryKey = hive.OpenSubKey(subPath, writable:=False)
                Return k IsNot Nothing
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("WinRegistryAccess.KeyExists", ex)
            Throw
        End Try
    End Function

    Public Function ValueNames(path As String) As IReadOnlyList(Of String) Implements IRegistryAccess.ValueNames
        Try
            Dim hive As RegistryKey = Nothing
            Dim subPath As String = Nothing
            SplitPath(path, hive, subPath)
            Using k As RegistryKey = hive.OpenSubKey(subPath, writable:=False)
                ' A missing key is «nothing there», not a failure — same rule as Read.
                If k Is Nothing Then Return New List(Of String)()
                Return k.GetValueNames().OrderBy(Function(n) n, StringComparer.OrdinalIgnoreCase).ToList()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("WinRegistryAccess.ValueNames", ex)
            Throw
        End Try
    End Function

    ' Splits a hive-prefixed path into its root key and sub-path.
    Private Shared Sub SplitPath(path As String, ByRef hive As RegistryKey, ByRef subPath As String)
        If String.IsNullOrEmpty(path) Then Throw New ArgumentException("Cale de registry goală.")
        Dim sep As Integer = path.IndexOf("\"c)
        Dim prefix As String = If(sep < 0, path, path.Substring(0, sep))
        subPath = If(sep < 0, "", path.Substring(sep + 1))
        Select Case prefix.ToUpperInvariant()
            Case AdobeRegistryConstants.HkcuPrefix
                hive = Registry.CurrentUser
            Case AdobeRegistryConstants.HklmPrefix
                hive = Registry.LocalMachine
            Case Else
                Throw New ArgumentException($"Prefix de registry nesuportat: {prefix}")
        End Select
    End Sub

End Class
