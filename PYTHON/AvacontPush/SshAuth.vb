Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Renci.SshNet

' How AvacontPush logs in to the server (slice 0103-02): with the operator's SSH KEY when there is
' one - the same key VS Code uses to open the server without asking anything - and with the typed
' password only when the key is missing or the operator typed one anyway.
'
' Where the key comes from, in this order:
'   1. PrivateKeyPath in push_settings.json, when set and the file exists;
'   2. an IdentityFile line of the ~/.ssh/config block that names this host (or "Host *");
'   3. the default files of ~/.ssh: id_ed25519, id_ecdsa, id_rsa.
'
' The Password box has a second meaning here: when the key is protected by a passphrase, what is
' typed in the box is tried as that passphrase (and as the password, if the key is refused).
' SSH.NET does not talk to ssh-agent, so a key that lives only in the agent cannot be used: it
' needs a key file.
Public NotInheritable Class SshAuth

    Private Shared ReadOnly DefaultKeyNames As String() = {"id_ed25519", "id_ecdsa", "id_rsa"}

    ' The connection info for SshClient / SftpClient. Throws (Romanian) when there is nothing to
    ' log in with.
    Public Shared Function Build(settings As PushSettings) As ConnectionInfo
        Dim methods As New List(Of AuthenticationMethod)()
        Dim typed = If(settings.Password, "")
        Dim keyPath = FindKeyFile(settings)
        Dim keyProblem = ""

        If keyPath <> "" Then
            Try
                Dim passphrase As String = If(typed = "", Nothing, typed)
                methods.Add(New PrivateKeyAuthenticationMethod(settings.User, New PrivateKeyFile(keyPath, passphrase)))
            Catch ex As Exception When TypeOf ex Is Renci.SshNet.Common.SshException OrElse
                                       TypeOf ex Is IOException OrElse
                                       TypeOf ex Is ArgumentException OrElse
                                       TypeOf ex Is NotSupportedException
                keyProblem = $"Cheia SSH {keyPath} nu a putut fi folosită: {ex.Message}. " &
                             "Dacă e protejată cu parolă, scrieți parola cheii în câmpul Parolă."
            End Try
        End If

        If typed <> "" Then methods.Add(New PasswordAuthenticationMethod(settings.User, typed))

        If methods.Count = 0 Then
            If keyProblem <> "" Then Throw New ApplicationException(keyProblem)
            Throw New ApplicationException(
                "Nu există cheie SSH în ~/.ssh (id_ed25519, id_ecdsa, id_rsa) și nici parolă tastată. " &
                "Puneți cheia în ~/.ssh sau tastați parola.")
        End If

        Return New ConnectionInfo(settings.Host, settings.Port, settings.User, methods.ToArray())
    End Function

    ' One line for the status bar: how the next connection will log in.
    Public Shared Function Describe(settings As PushSettings) As String
        Dim keyPath = FindKeyFile(settings)
        If keyPath <> "" Then
            Return $"Autentificare: cheia SSH {Path.GetFileName(keyPath)}" &
                   If(String.IsNullOrEmpty(settings.Password), " (fără parolă).", " (și parola tastată).")
        End If
        Return If(String.IsNullOrEmpty(settings.Password),
                  "Autentificare: nicio cheie SSH găsită; tastați parola.",
                  "Autentificare: cu parola tastată (nicio cheie SSH găsită).")
    End Function

    Public Shared Function FindKeyFile(settings As PushSettings) As String
        Dim explicitPath = ExpandHome(If(settings.PrivateKeyPath, "").Trim())
        If explicitPath <> "" AndAlso File.Exists(explicitPath) Then Return explicitPath

        Dim fromConfig = IdentityFromSshConfig(settings.Host)
        If fromConfig <> "" AndAlso File.Exists(fromConfig) Then Return fromConfig

        Dim sshDir = SshFolder()
        If sshDir = "" Then Return ""
        For Each name In DefaultKeyNames
            Dim candidate = Path.Combine(sshDir, name)
            If File.Exists(candidate) Then Return candidate
        Next
        Return ""
    End Function

    Private Shared Function SshFolder() As String
        Dim home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Return If(String.IsNullOrEmpty(home), "", Path.Combine(home, ".ssh"))
    End Function

    ' "~" or "~/x" -> the user's folder.
    Private Shared Function ExpandHome(path As String) As String
        If path = "" OrElse Not path.StartsWith("~", StringComparison.Ordinal) Then Return path
        Dim home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Return home & path.Substring(1).Replace("/"c, System.IO.Path.DirectorySeparatorChar)
    End Function

    ' The first IdentityFile of the ~/.ssh/config blocks that name this host (or "Host *").
    ' Only the exact host name is matched (no wildcard patterns other than a lone "*"): the config of
    ' this machine names the server by its address.
    Private Shared Function IdentityFromSshConfig(host As String) As String
        Dim dir = SshFolder()
        If dir = "" Then Return ""
        Dim configPath = Path.Combine(dir, "config")
        If Not File.Exists(configPath) Then Return ""

        Dim applies As Boolean = False
        For Each rawLine In File.ReadAllLines(configPath)
            Dim line = rawLine.Trim()
            If line = "" OrElse line.StartsWith("#", StringComparison.Ordinal) Then Continue For

            Dim parts = line.Split({" "c, vbTab, "="c}, 2, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length < 2 Then Continue For
            Dim keyword = parts(0)
            Dim value = parts(1).Trim().Trim(""""c)

            If String.Equals(keyword, "Host", StringComparison.OrdinalIgnoreCase) Then
                applies = value.Split({" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries).Any(
                    Function(p) p = "*" OrElse String.Equals(p, host, StringComparison.OrdinalIgnoreCase))
            ElseIf applies AndAlso String.Equals(keyword, "IdentityFile", StringComparison.OrdinalIgnoreCase) Then
                Return ExpandHome(value)
            End If
        Next
        Return ""
    End Function

End Class
