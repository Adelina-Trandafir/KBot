Imports System.Globalization
Imports KBot.Common
Imports Microsoft.Win32

''' <summary>
''' What the Access system left in the registry for one DC:
''' <c>HKCU\Software\VB and VBA Program Settings\AVACONT\&lt;DC&gt;\Tokens</c> (values <c>Token</c>, <c>RefreshKey</c>,
''' <c>TokenExpiry</c>, <c>Vercon</c>). Read only: this class never writes the registry and never returns a whole secret
''' to be shown (<see cref="Mask"/>).
''' </summary>
Public NotInheritable Class EfTokenRegistry

    Private Const BaseKey As String = "Software\VB and VBA Program Settings\AVACONT"

    Private Sub New()
    End Sub

    ''' <summary>The values found under one DC. <see cref="KeyFound"/> False = there is no Tokens key at all.</summary>
    Public NotInheritable Class Found
        Public Property KeyPath As String
        Public Property KeyFound As Boolean
        Public Property Token As String
        Public Property RefreshKey As String
        Public Property TokenExpiryText As String
        Public Property TokenExpiry As DateTime?
        Public Property Vercon As String
    End Class

    ''' <summary>Reads the Tokens key of <paramref name="k_dc"/>; never throws for a missing key or value.</summary>
    Public Shared Function Read(k_dc As String) As Found
        Try
            If String.IsNullOrWhiteSpace(k_dc) Then Throw New ArgumentException("DC is empty.", NameOf(k_dc))

            Dim found As New Found With {.KeyPath = $"HKCU\{BaseKey}\{k_dc}\Tokens"}
            Using key = Registry.CurrentUser.OpenSubKey($"{BaseKey}\{k_dc}\Tokens", False)
                If key Is Nothing Then Return found
                found.KeyFound = True
                found.Token = TextOf(key, "Token")
                found.RefreshKey = TextOf(key, "RefreshKey")
                found.Vercon = TextOf(key, "Vercon")
                found.TokenExpiryText = TextOf(key, "TokenExpiry")
            End Using

            Dim parsed As DateTime
            If DateTime.TryParse(found.TokenExpiryText, CultureInfo.InvariantCulture, DateTimeStyles.None, parsed) Then
                found.TokenExpiry = parsed
            End If
            Return found
        Catch ex As Exception
            GlobalErrorLog.Write("EfTokenRegistry.Read", ex)
            Throw
        End Try
    End Function

    ''' <summary>Length and the first characters only, so the log shows that a value exists without carrying it.</summary>
    Public Shared Function Mask(k_value As String) As String
        If String.IsNullOrEmpty(k_value) Then Return "(lipsă)"
        Dim head = k_value.Substring(0, Math.Min(6, k_value.Length))
        Return $"{head}... ({k_value.Length} caractere)"
    End Function

    Private Shared Function TextOf(k_key As RegistryKey, k_name As String) As String
        Dim raw = k_key.GetValue(k_name, Nothing)
        Return If(raw Is Nothing, Nothing, Convert.ToString(raw, CultureInfo.InvariantCulture))
    End Function

End Class
