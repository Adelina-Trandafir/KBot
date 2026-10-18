Imports System.Diagnostics
Imports System.IO
Imports KBot.Common

''' <summary>Opens Windows Explorer on a source file of the report.</summary>
Friend NotInheritable Class VrcShell

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Explorer with the file selected; when the file is gone, its folder. Only Access bases (.accdb / .mdb) and folders are opened:
    ''' the path comes from report text, so nothing else may be started from it.
    ''' </summary>
    Friend Shared Sub RevealInExplorer(k_path As String)
        Try
            If String.IsNullOrWhiteSpace(k_path) Then Return
            Dim k_extension As String = Path.GetExtension(k_path).ToLowerInvariant()
            Dim k_arguments As String
            If File.Exists(k_path) AndAlso (k_extension = ".accdb" OrElse k_extension = ".mdb") Then
                k_arguments = $"/select,""{k_path}"""
            ElseIf Directory.Exists(Path.GetDirectoryName(k_path)) Then
                k_arguments = $"""{Path.GetDirectoryName(k_path)}"""
            Else
                Return
            End If
            Process.Start(New ProcessStartInfo("explorer.exe", k_arguments) With {.UseShellExecute = False})
        Catch ex As Exception
            GlobalErrorLog.Write("VrcShell.RevealInExplorer", ex)
        End Try
    End Sub

End Class
