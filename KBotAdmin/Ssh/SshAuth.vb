Option Strict On
Imports System.IO
Imports Renci.SshNet

''' <summary>
''' How KBotAdmin logs in to the server: with the operator's SSH KEY when there is one (the same key
''' the editor uses), and with the typed password only when the key is missing or one was typed anyway.
''' Same lookup order as AvacontPush:
'''   1. PrivateKeyPath in admin_settings.json, when set and the file exists;
'''   2. an IdentityFile line of the ~/.ssh/config block that names this host (or "Host *");
'''   3. the default files of ~/.ssh: id_ed25519, id_ecdsa, id_rsa.
''' When the key is protected by a passphrase, what is typed in the password box is tried as that
''' passphrase and, if the key is refused, as the account password. SSH.NET does not talk to
''' ssh-agent, so a key that lives only in the agent needs a key file.
''' </summary>
Public NotInheritable Class SshAuth

    Private Shared ReadOnly DefaultKeyNames As String() = {"id_ed25519", "id_ecdsa", "id_rsa"}

    Private Sub New()
    End Sub

    ''' <summary>The connection info for SshClient. Throws (Romanian) when there is nothing to log in with.</summary>
    Public Shared Function Build(k_settings As AdminSettings) As ConnectionInfo
        Dim k_methods As New List(Of AuthenticationMethod)()
        Dim k_typed As String = If(k_settings.Password, String.Empty)
        Dim k_keyPath As String = FindKeyFile(k_settings)
        Dim k_keyProblem As String = String.Empty

        If k_keyPath <> String.Empty Then
            Try
                Dim k_phrase As String = If(k_typed = String.Empty, Nothing, k_typed)
                k_methods.Add(New PrivateKeyAuthenticationMethod(k_settings.User, New PrivateKeyFile(k_keyPath, k_phrase)))
            Catch ex As Exception When TypeOf ex Is Renci.SshNet.Common.SshException OrElse
                                       TypeOf ex Is IOException OrElse
                                       TypeOf ex Is ArgumentException OrElse
                                       TypeOf ex Is NotSupportedException
                k_keyProblem = $"Cheia SSH {k_keyPath} nu a putut fi folosită: {ex.Message}. " &
                               "Dacă e protejată cu parolă, scrieți parola cheii în câmpul Parolă."
            End Try
        End If

        If k_typed <> String.Empty Then k_methods.Add(New PasswordAuthenticationMethod(k_settings.User, k_typed))

        If k_methods.Count = 0 Then
            If k_keyProblem <> String.Empty Then Throw New ApplicationException(k_keyProblem)
            Throw New ApplicationException(
                "Nu există cheie SSH în ~/.ssh (id_ed25519, id_ecdsa, id_rsa) și nici parolă tastată. " &
                "Puneți cheia în ~/.ssh sau tastați parola.")
        End If

        Return New ConnectionInfo(k_settings.Host, k_settings.Port, k_settings.User, k_methods.ToArray())
    End Function

    ''' <summary>One line for the status bar: how the next connection will log in.</summary>
    Public Shared Function Describe(k_settings As AdminSettings) As String
        Dim k_keyPath As String = FindKeyFile(k_settings)
        If k_keyPath <> String.Empty Then
            Return $"Autentificare: cheia SSH {Path.GetFileName(k_keyPath)}" &
                   If(String.IsNullOrEmpty(k_settings.Password), " (fără parolă).", " (și parola tastată).")
        End If
        Return If(String.IsNullOrEmpty(k_settings.Password),
                  "Autentificare: nicio cheie SSH găsită; tastați parola.",
                  "Autentificare: cu parola tastată (nicio cheie SSH găsită).")
    End Function

    Public Shared Function FindKeyFile(k_settings As AdminSettings) As String
        Dim k_explicit As String = ExpandHome(If(k_settings.PrivateKeyPath, String.Empty).Trim())
        If k_explicit <> String.Empty AndAlso File.Exists(k_explicit) Then Return k_explicit

        Dim k_fromConfig As String = IdentityFromSshConfig(k_settings.Host)
        If k_fromConfig <> String.Empty AndAlso File.Exists(k_fromConfig) Then Return k_fromConfig

        Dim k_dir As String = SshFolder()
        If k_dir = String.Empty Then Return String.Empty
        For Each k_name As String In DefaultKeyNames
            Dim k_candidate As String = Path.Combine(k_dir, k_name)
            If File.Exists(k_candidate) Then Return k_candidate
        Next
        Return String.Empty
    End Function

    Private Shared Function SshFolder() As String
        Dim k_home As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Return If(String.IsNullOrEmpty(k_home), String.Empty, Path.Combine(k_home, ".ssh"))
    End Function

    Private Shared Function ExpandHome(k_path As String) As String
        If k_path = String.Empty OrElse Not k_path.StartsWith("~", StringComparison.Ordinal) Then Return k_path
        Dim k_home As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Return k_home & k_path.Substring(1).Replace("/"c, Path.DirectorySeparatorChar)
    End Function

    ' The first IdentityFile of the ~/.ssh/config blocks that name this host (or "Host *").
    Private Shared Function IdentityFromSshConfig(k_host As String) As String
        Dim k_dir As String = SshFolder()
        If k_dir = String.Empty Then Return String.Empty
        Dim k_config As String = Path.Combine(k_dir, "config")
        If Not File.Exists(k_config) Then Return String.Empty

        Dim k_applies As Boolean = False
        For Each k_raw As String In File.ReadAllLines(k_config)
            Dim k_line As String = k_raw.Trim()
            If k_line = String.Empty OrElse k_line.StartsWith("#", StringComparison.Ordinal) Then Continue For

            Dim k_parts As String() = k_line.Split({" "c, vbTab, "="c}, 2, StringSplitOptions.RemoveEmptyEntries)
            If k_parts.Length < 2 Then Continue For
            Dim k_keyword As String = k_parts(0)
            Dim k_value As String = k_parts(1).Trim().Trim(""""c)

            If String.Equals(k_keyword, "Host", StringComparison.OrdinalIgnoreCase) Then
                k_applies = k_value.Split({" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries).Any(
                    Function(k_p) k_p = "*" OrElse String.Equals(k_p, k_host, StringComparison.OrdinalIgnoreCase))
            ElseIf k_applies AndAlso String.Equals(k_keyword, "IdentityFile", StringComparison.OrdinalIgnoreCase) Then
                Return ExpandHome(k_value)
            End If
        Next
        Return String.Empty
    End Function

End Class
