Option Strict On
Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Security
Imports Microsoft.Win32

''' <summary>
''' The "Programs and Features" entry the Inno installer registers (slice 0067-01).
'''
''' <para>The installer writes <c>DisplayVersion</c> once; an automatic update rewrites the
''' files without going through Setup, so the entry would fall behind after the first one.
''' The installer itself reads the exe's FileVersion, not this entry, so nothing breaks --
''' but the operator reads this list, and it must tell the truth.</para>
'''
''' <para>Only the entry that belongs to the folder being updated is touched: its
''' <c>InstallLocation</c> must be that folder. HKLM needs administrator rights, which is
''' why <see cref="NeedsElevation"/> exists: the form asks for them before applying, and
''' when the operator declines the files are still updated and only this entry is skipped.</para>
''' </summary>
Friend NotInheritable Class InstallRegistry

    ''' <summary>Same AppId as tools\KBotInstaller\KBot.iss; Inno appends "_is1".</summary>
    Public Const APP_ID As String = "{A9840797-CE1E-4708-BDC0-73E4CCBC2D3A}"
    Public Const KEY_PATH As String = "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" & APP_ID & "_is1"
    Private Const APP_EXE As String = "KBot.App.exe"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The registry view holding the entry for <paramref name="targetDir"/>, or <c>Nothing</c>
    ''' when there is none (a hand-copied install, or the SFX-era one) or the entry belongs to
    ''' a different folder.
    ''' </summary>
    Public Shared Function FindView(targetDir As String) As RegistryView?
        If String.IsNullOrWhiteSpace(targetDir) Then Throw New ArgumentException("targetDir")
        For Each view As RegistryView In {RegistryView.Registry64, RegistryView.Registry32}
            Using base As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)
                Using key As RegistryKey = base.OpenSubKey(KEY_PATH, writable:=False)
                    If key Is Nothing Then Continue For
                    Dim location As String = TryCast(key.GetValue("InstallLocation"), String)
                    If SameFolder(location, targetDir) Then Return view
                End Using
            End Using
        Next
        Return Nothing
    End Function

    ''' <summary>True when the entry exists for this folder and this process cannot write it.</summary>
    Public Shared Function NeedsElevation(targetDir As String) As Boolean
        Dim view As RegistryView? = FindView(targetDir)
        If Not view.HasValue Then Return False
        Try
            Using base As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view.Value)
                Using key As RegistryKey = base.OpenSubKey(KEY_PATH, writable:=True)
                    Return key Is Nothing
                End Using
            End Using
        Catch ex As SecurityException
            Return True
        Catch ex As UnauthorizedAccessException
            Return True
        End Try
    End Function

    ''' <summary>
    ''' Writes <paramref name="newVersion"/> into the entry for <paramref name="targetDir"/>:
    ''' <c>DisplayVersion</c>, <c>VersionMajor</c>/<c>VersionMinor</c> and the version suffix of
    ''' <c>DisplayName</c> ("K-BOT 1.0.30.0" becomes "K-BOT 1.0.31.0"). Returns False when
    ''' there is no entry for that folder. Throws on a denied write -- the caller decides
    ''' whether that is fatal.
    ''' </summary>
    Public Shared Function WriteVersion(targetDir As String, newVersion As String, log As Action(Of String)) As Boolean
        If String.IsNullOrWhiteSpace(newVersion) Then Throw New ArgumentException("newVersion")
        Dim view As RegistryView? = FindView(targetDir)
        If Not view.HasValue Then Return False
        Using base As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view.Value)
            Using key As RegistryKey = base.OpenSubKey(KEY_PATH, writable:=True)
                If key Is Nothing Then Return False
                Dim oldVersion As String = TryCast(key.GetValue("DisplayVersion"), String)
                Dim oldName As String = TryCast(key.GetValue("DisplayName"), String)

                key.SetValue("DisplayVersion", newVersion, RegistryValueKind.String)
                Dim parsed As System.Version = Nothing
                If System.Version.TryParse(newVersion, parsed) Then
                    key.SetValue("VersionMajor", Math.Max(parsed.Major, 0), RegistryValueKind.DWord)
                    key.SetValue("VersionMinor", Math.Max(parsed.Minor, 0), RegistryValueKind.DWord)
                End If
                Dim newName As String = NewDisplayName(oldName, oldVersion, newVersion)
                If newName IsNot Nothing AndAlso Not String.Equals(newName, oldName, StringComparison.Ordinal) Then
                    key.SetValue("DisplayName", newName, RegistryValueKind.String)
                End If
                log?.Invoke("registry: DisplayVersion " & If(oldVersion, "<none>") & " -> " & newVersion &
                            If(newName IsNot Nothing AndAlso newName <> oldName, ", DisplayName «" & newName & "»", "") &
                            " (" & view.Value.ToString() & ")")
                Return True
            End Using
        End Using
    End Function

    ''' <summary>
    ''' The FileVersion of the KBot.App.exe now in <paramref name="targetDir"/> -- the same
    ''' number the installer compares against -- or <c>Nothing</c> when the exe or its version
    ''' resource is missing.
    ''' </summary>
    Public Shared Function ReadInstalledVersion(targetDir As String) As String
        Dim exe As String = Path.Combine(targetDir, APP_EXE)
        If Not File.Exists(exe) Then Return Nothing
        Dim info As FileVersionInfo = FileVersionInfo.GetVersionInfo(exe)
        If String.IsNullOrWhiteSpace(info.FileVersion) Then Return Nothing
        Return info.FileVersion.Trim()
    End Function

    ''' <summary>
    ''' Inno sets <c>DisplayName</c> to AppVerName, i.e. "K-BOT &lt;version&gt;". Replaces that
    ''' trailing old version with the new one; returns <c>Nothing</c> when the name does not
    ''' end with the old version (then it is left alone -- never guess at a name).
    ''' </summary>
    Public Shared Function NewDisplayName(oldName As String, oldVersion As String, newVersion As String) As String
        If String.IsNullOrEmpty(oldName) OrElse String.IsNullOrEmpty(oldVersion) Then Return Nothing
        If String.IsNullOrWhiteSpace(newVersion) Then Throw New ArgumentException("newVersion")
        If Not oldName.EndsWith(oldVersion, StringComparison.Ordinal) Then Return Nothing
        Return oldName.Substring(0, oldName.Length - oldVersion.Length) & newVersion.Trim()
    End Function

    ''' <summary>Same folder regardless of case and trailing separators; False when either is empty.</summary>
    Public Shared Function SameFolder(a As String, b As String) As Boolean
        If String.IsNullOrWhiteSpace(a) OrElse String.IsNullOrWhiteSpace(b) Then Return False
        Dim fa As String = Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim fb As String = Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Return String.Equals(fa, fb, StringComparison.OrdinalIgnoreCase)
    End Function
End Class
