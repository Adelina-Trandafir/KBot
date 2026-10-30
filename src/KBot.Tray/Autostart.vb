Option Strict On
Imports System.IO
Imports KBot.Common
Imports Microsoft.Win32

''' <summary>
''' «Pornește cu Windows»: the tray starts with the user's Windows session through the standard
''' <c>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</c> key -- the launcher in the installation folder, never the copy
''' (see <see cref="ShadowCopy"/>), so a new version of K-BOT is the one that starts next time.
'''
''' <para>The first run switches it on by itself. When the user switches it off from the tray menu that is remembered
''' (<c>HKCU\Software\AVACONT\KBot\Tray</c>, <c>AutostartOff</c>) and the first-run rule leaves it alone from then on.</para>
''' </summary>
Friend NotInheritable Class Autostart

    Private Sub New()
    End Sub

    Private Const RunKeyPath As String = "Software\Microsoft\Windows\CurrentVersion\Run"
    Private Const RunValueName As String = "KBotTray"
    Private Const StateKeyPath As String = "Software\AVACONT\KBot\Tray"
    Private Const StateValueOff As String = "AutostartOff"

    ''' <summary>Is the Run entry in place?</summary>
    Friend Shared Function IsOn() As Boolean
        Try
            Using k_key As RegistryKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable:=False)
                Return k_key IsNot Nothing AndAlso k_key.GetValue(RunValueName) IsNot Nothing
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("Autostart.IsOn", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Is K-BOT here a build folder of the developer (<c>...\bin\Debug\...</c> or <c>...\bin\Release\...</c>)? Such a tray never
    ''' writes the Windows Run key: it would point Windows at a folder that is rebuilt and deleted. An installed copy (Debug
    ''' published to a test PC included) does not live under a <c>bin</c> folder.
    ''' </summary>
    Friend Shared Function IsDevelopmentFolder(k_appDir As String) As Boolean
        Dim k_path As String = (If(k_appDir, String.Empty)).Replace("/"c, "\"c) & "\"
        Return k_path.IndexOf("\bin\Debug\", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               k_path.IndexOf("\bin\Release\", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>Puts the entry in on the first run, unless the user switched it off before. Never throws (a convenience).</summary>
    Friend Shared Sub EnsureOnFirstRun(k_appDir As String)
        Try
            If IsDevelopmentFolder(k_appDir) Then Return
            If UserSwitchedOff() OrElse IsOn() Then Return
            SetOn(True, k_appDir)
        Catch ex As Exception
            GlobalErrorLog.Write("Autostart.EnsureOnFirstRun", ex)
        End Try
    End Sub

    ''' <summary>Switches the entry on or off and remembers the user's choice. I/O boundary: logs and rethrows.</summary>
    Friend Shared Sub SetOn(k_on As Boolean, k_appDir As String)
        Try
            If IsDevelopmentFolder(k_appDir) Then Return
            Using k_run As RegistryKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable:=True)
                If k_on Then
                    Dim k_exe As String = Path.Combine(k_appDir, ShadowCopy.ExeName)
                    k_run.SetValue(RunValueName, """" & k_exe & """", RegistryValueKind.String)
                Else
                    k_run.DeleteValue(RunValueName, throwOnMissingValue:=False)
                End If
            End Using
            Using k_state As RegistryKey = Registry.CurrentUser.CreateSubKey(StateKeyPath, writable:=True)
                k_state.SetValue(StateValueOff, If(k_on, 0, 1), RegistryValueKind.DWord)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("Autostart.SetOn", ex)
            Throw
        End Try
    End Sub

    Private Shared Function UserSwitchedOff() As Boolean
        Using k_key As RegistryKey = Registry.CurrentUser.OpenSubKey(StateKeyPath, writable:=False)
            If k_key Is Nothing Then Return False
            Dim k_value As Object = k_key.GetValue(StateValueOff)
            Return k_value IsNot Nothing AndAlso Convert.ToInt32(k_value) = 1
        End Using
    End Function
End Class
