Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports KBot.Common

''' <summary>
''' The tray runs from a COPY of its exe, never from the installation folder. The updater of K-BOT overwrites every file of
''' <c>C:\KBOT</c>; a tray that stayed there from the start of Windows would hold <c>KBot.Tray.exe</c> open and make every
''' update fail on it. The exe in the installation folder is only the launcher: it copies itself to
''' <c>%LOCALAPPDATA%\AVACONT\KBot\Tray</c>, starts the copy with <c>--app-dir</c> (where K-BOT lives) and ends.
''' </summary>
Friend NotInheritable Class ShadowCopy

    Private Sub New()
    End Sub

    Friend Const ExeName As String = "KBot.Tray.exe"
    Friend Const AppDirArg As String = "--app-dir"

    ''' <summary>The folder the copy runs from.</summary>
    Friend Shared Function ShadowDirectory() As String
        Return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AVACONT", "KBot", "Tray")
    End Function

    ''' <summary>Is this process the copy (not the exe of the installation folder)?</summary>
    Friend Shared Function IsShadow() As Boolean
        Dim k_self As String = Path.GetDirectoryName(Environment.ProcessPath)
        Return String.Equals(Path.GetFullPath(k_self).TrimEnd("\"c), Path.GetFullPath(ShadowDirectory()).TrimEnd("\"c),
                             StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' The folder where <c>KBot.App.exe</c> lives: the one given to the copy, or the folder of this exe when it was started by
    ''' hand from the installation folder.
    ''' </summary>
    Friend Shared Function AppDirectory(k_args As String()) As String
        If k_args IsNot Nothing Then
            For k_i As Integer = 0 To k_args.Length - 2
                If String.Equals(k_args(k_i), AppDirArg, StringComparison.OrdinalIgnoreCase) Then Return k_args(k_i + 1)
            Next
        End If
        Return Path.GetDirectoryName(Environment.ProcessPath)
    End Function

    ''' <summary>
    ''' Copies this exe over the one in the shadow folder when it differs and starts that one. I/O boundary: logs and rethrows
    ''' (the caller ends the process either way: without the copy there is no tray, and the log says why).
    ''' </summary>
    Friend Shared Sub StartCopy(k_appDir As String)
        Try
            Dim k_dir As String = ShadowDirectory()
            Directory.CreateDirectory(k_dir)
            Dim k_source As String = Environment.ProcessPath
            Dim k_target As String = Path.Combine(k_dir, ExeName)
            If NeedsCopy(k_source, k_target) Then File.Copy(k_source, k_target, overwrite:=True)
            Dim k_psi As New ProcessStartInfo(k_target) With {
                .UseShellExecute = False,
                .WorkingDirectory = k_dir
            }
            k_psi.ArgumentList.Add(AppDirArg)
            k_psi.ArgumentList.Add(k_appDir)
            Process.Start(k_psi)?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ShadowCopy.StartCopy", ex)
            Throw
        End Try
    End Sub

    Private Shared Function NeedsCopy(k_source As String, k_target As String) As Boolean
        If Not File.Exists(k_target) Then Return True
        Dim k_a As New FileInfo(k_source)
        Dim k_b As New FileInfo(k_target)
        Return k_a.Length <> k_b.Length OrElse k_a.LastWriteTimeUtc <> k_b.LastWriteTimeUtc
    End Function
End Class
