Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.Json

''' <summary>
''' Remembers WHO logged in last (user name and the unit they picked), so the login form
''' can offer them again instead of making the operator type the e-mail every morning
''' (operator request, 15.09.2026 -- slice 0063).
'''
''' <para>Only the two identifiers are kept. NEVER the password, NEVER the token: the file
''' is plain JSON next to the operator's other per-user settings, and a stored secret there
''' would be a stored secret in clear text.</para>
'''
''' <para>Where: <c>%APPDATA%\AVACONT\KBot\last_login.json</c> -- the same per-user folder
''' as <see cref="SetariFoldere"/> (<c>settings.json</c>). Per user by construction, which is
''' exactly the scope of «the last user on this Windows account».</para>
'''
''' <para>A missing, empty or broken file means «nothing remembered»: <see cref="Load"/>
''' never throws and never logs (same reasoning as <see cref="SetariFoldere.Incarca"/>:
''' this runs before anything else is up). <see cref="Save"/> is an I/O boundary: it logs
''' and rethrows, so a caller that cares can tell, and one that does not (the login form)
''' swallows it at its UI boundary -- a login must not fail because a convenience file
''' could not be written.</para>
''' </summary>
Public NotInheritable Class LastLoginStore

    ''' <summary>File name inside <see cref="SetariFoldere.DirectorSetari"/>.</summary>
    Public Const FileName As String = "last_login.json"

    ''' <summary>User name (e-mail) of the last successful login; Nothing when unknown.</summary>
    Public Property Username As String

    ''' <summary>DC of the unit picked at the last successful login; Nothing when unknown.</summary>
    Public Property UnitDc As String

    ''' <summary>Full path of the store file, in the given folder or the per-user default.</summary>
    Public Shared Function FilePath(Optional dir As String = Nothing) As String
        Return Path.Combine(If(String.IsNullOrEmpty(dir), SetariFoldere.DirectorSetari(), dir), FileName)
    End Function

    ''' <summary>
    ''' Reads what was remembered. Missing / empty / broken file = an empty store. Never throws.
    ''' </summary>
    Public Shared Function Load(Optional dir As String = Nothing) As LastLoginStore
        Dim result As New LastLoginStore()
        Try
            Dim cale As String = FilePath(dir)
            If Not File.Exists(cale) Then Return result
            Dim json As String = File.ReadAllText(cale)
            If String.IsNullOrWhiteSpace(json) Then Return result

            Using doc As JsonDocument = JsonDocument.Parse(json)
                If doc.RootElement.ValueKind <> JsonValueKind.Object Then Return result
                result.Username = ReadString(doc.RootElement, "Username")
                result.UnitDc = ReadString(doc.RootElement, "UnitDc")
            End Using
            Return result
        Catch
            ' Broken JSON or no read permission: the operator types the name once more.
            ' Deliberately not logged -- see the class remarks.
            Return New LastLoginStore()
        End Try
    End Function

    Private Shared Function ReadString(root As JsonElement, name As String) As String
        Dim prop As JsonElement
        If Not root.TryGetProperty(name, prop) Then Return Nothing
        If prop.ValueKind <> JsonValueKind.String Then Return Nothing
        Dim v As String = prop.GetString()
        Return If(String.IsNullOrWhiteSpace(v), Nothing, v.Trim())
    End Function

    ''' <summary>
    ''' Writes the user name and unit of a login that SUCCEEDED. Call it only after the
    ''' server said yes -- a mistyped name must not become tomorrow's suggestion.
    ''' </summary>
    ''' <remarks>I/O boundary: logs and rethrows (house rule).</remarks>
    Public Shared Sub Save(username As String, unitDc As String, Optional dir As String = Nothing)
        Try
            If String.IsNullOrWhiteSpace(username) Then
                Throw New ArgumentException("User name is empty.", NameOf(username))
            End If
            Dim payload As New LastLoginStore() With {
                .Username = username.Trim(),
                .UnitDc = If(String.IsNullOrWhiteSpace(unitDc), Nothing, unitDc.Trim())
            }
            Dim target As String = FilePath(dir)
            Directory.CreateDirectory(Path.GetDirectoryName(target))
            File.WriteAllText(
                target,
                JsonSerializer.Serialize(payload, New JsonSerializerOptions With {.WriteIndented = True}),
                New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("LastLoginStore.Save", ex)
            Throw
        End Try
    End Sub
End Class
