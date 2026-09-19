Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO

''' <summary>
''' The command line KBot.App hands to the updater (slice 0067). Pure: parses, validates,
''' never touches the file system beyond <c>Path</c> checks -- the tests drive it directly.
'''
''' <code>
''' KBot.Updater.exe --zip "C:\...\KBot_1.0.31.0.zip" --target "C:\KBOT" --wait 1234
'''                  --restart "C:\KBOT\KBot.App.exe" [--sha256 hex] [--version 1.0.31.0] [--elevated]
''' </code>
''' </summary>
Friend NotInheritable Class UpdaterArgs

    ''' <summary>The package to apply. Required.</summary>
    Public Property ZipPath As String

    ''' <summary>The installation folder to write into. Required.</summary>
    Public Property TargetDir As String

    ''' <summary>Process id to wait for before touching files (the app that launched us). 0 = none.</summary>
    Public Property WaitPid As Integer

    ''' <summary>Executable to start once the package is applied. Optional.</summary>
    Public Property RestartExe As String

    ''' <summary>Expected SHA-256 of the zip, hex. Optional; when given the zip is verified before anything is written.</summary>
    Public Property Sha256 As String

    ''' <summary>Version text for the window title. Optional, cosmetic.</summary>
    Public Property Version As String

    ''' <summary>Set by the updater itself when it relaunches elevated, so it never loops.</summary>
    Public Property Elevated As Boolean

    Public Shared Function Parse(argv As String()) As UpdaterArgs
        If argv Is Nothing Then Throw New ArgumentNullException(NameOf(argv))
        Dim result As New UpdaterArgs()
        Dim i As Integer = 0
        While i < argv.Length
            Dim key As String = argv(i)
            Select Case key
                Case "--elevated"
                    result.Elevated = True
                    i += 1
                Case "--zip", "--target", "--wait", "--restart", "--sha256", "--version"
                    If i + 1 >= argv.Length Then
                        Throw New ArgumentException("Argumentul " & key & " nu are valoare.")
                    End If
                    Dim value As String = argv(i + 1)
                    Select Case key
                        Case "--zip" : result.ZipPath = value
                        Case "--target" : result.TargetDir = value
                        Case "--restart" : result.RestartExe = value
                        Case "--sha256" : result.Sha256 = value
                        Case "--version" : result.Version = value
                        Case "--wait"
                            Dim pid As Integer
                            If Not Integer.TryParse(value, pid) OrElse pid < 0 Then
                                Throw New ArgumentException("Argumentul --wait trebuie să fie un număr de proces: «" & value & "».")
                            End If
                            result.WaitPid = pid
                    End Select
                    i += 2
                Case Else
                    Throw New ArgumentException("Argument necunoscut: «" & key & "».")
            End Select
        End While

        If String.IsNullOrWhiteSpace(result.ZipPath) Then Throw New ArgumentException("Lipsește --zip.")
        If String.IsNullOrWhiteSpace(result.TargetDir) Then Throw New ArgumentException("Lipsește --target.")
        If Not Path.IsPathRooted(result.ZipPath) Then Throw New ArgumentException("--zip trebuie să fie o cale absolută.")
        If Not Path.IsPathRooted(result.TargetDir) Then Throw New ArgumentException("--target trebuie să fie o cale absolută.")
        If Not String.IsNullOrEmpty(result.RestartExe) AndAlso Not Path.IsPathRooted(result.RestartExe) Then
            Throw New ArgumentException("--restart trebuie să fie o cale absolută.")
        End If
        If Not String.IsNullOrEmpty(result.Sha256) AndAlso result.Sha256.Trim().Length <> 64 Then
            Throw New ArgumentException("--sha256 trebuie să aibă 64 de caractere hex.")
        End If
        Return result
    End Function

    ''' <summary>The same arguments back as a command line (for the elevated relaunch).</summary>
    Public Function ToArgumentList(Optional markElevated As Boolean = False) As IReadOnlyList(Of String)
        Dim list As New List(Of String) From {"--zip", ZipPath, "--target", TargetDir}
        If WaitPid > 0 Then list.AddRange({"--wait", WaitPid.ToString()})
        If Not String.IsNullOrEmpty(RestartExe) Then list.AddRange({"--restart", RestartExe})
        If Not String.IsNullOrEmpty(Sha256) Then list.AddRange({"--sha256", Sha256})
        If Not String.IsNullOrEmpty(Version) Then list.AddRange({"--version", Version})
        If Elevated OrElse markElevated Then list.Add("--elevated")
        Return list
    End Function
End Class
