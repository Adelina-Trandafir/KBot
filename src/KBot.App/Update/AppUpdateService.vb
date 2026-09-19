Option Strict On
Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The whole update conversation with the operator (slice 0067): ask the server, decide,
''' offer, download, hand over to <c>KBot.Updater.exe</c>, and tell the caller to exit.
'''
''' <para><b>Two entrances, one flow.</b> <see cref="RunStartupCheck"/> runs before the login
''' window (Release only, see Program) and is quiet when there is nothing to do; a server
''' that cannot be reached is logged and the app goes on -- an offline operator must still
''' be able to log in. <see cref="RunManualCheckAsync"/> is the "Caută actualizări" button:
''' the same flow, but every outcome is said out loud, including "sunteți la zi".</para>
'''
''' <para><b>Mandatory vs optional</b> is <see cref="UpdatePolicy"/>'s call: below the
''' server's <c>minimum</c> the only choices are update or close.</para>
'''
''' <para><b>Handover.</b> The updater is copied out of the installation folder into a fresh
''' <c>%TEMP%</c> folder (it is itself part of the package being replaced), started with the
''' zip path and this process id, and then this process must exit -- the caller does that,
''' because how to exit differs between the startup path (return from Main) and a running
''' shell (close the form).</para>
''' </summary>
Public NotInheritable Class AppUpdateService

    Private Const UPDATER_EXE As String = "KBot.Updater.exe"
    Private Const CAPTION As String = "Actualizare K-BOT"

    Private ReadOnly _api As IUpdateApi

    Public Sub New(api As IUpdateApi)
        If api Is Nothing Then Throw New ArgumentNullException(NameOf(api))
        _api = api
    End Sub

    ''' <summary>The running KBot.App FileVersion -- the number the server's <c>version</c> is compared with.</summary>
    Public Shared ReadOnly Property CurrentVersion As Version
        Get
            Return ReadCurrentVersion()
        End Get
    End Property

    Private Shared Function ReadCurrentVersion() As Version
        Try
            Dim asm As Assembly = If(Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly())
            Dim attr As AssemblyFileVersionAttribute = asm.GetCustomAttribute(Of AssemblyFileVersionAttribute)()
            Dim parsed As Version = Nothing
            If attr IsNot Nothing AndAlso Version.TryParse(attr.Version, parsed) Then Return UpdatePolicy.Normalize(parsed)
            Throw New InvalidOperationException("KBot.App nu are AssemblyFileVersion; comparația de versiune nu se poate face.")
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.ReadCurrentVersion", ex)
            Throw
        End Try
    End Function

    ''' <summary>The outcome of one check: what to do and, when there is something, with what.</summary>
    Public NotInheritable Class CheckResult
        Public Property Decision As UpdateDecision
        Public Property Info As UpdateInfo
        Public Property Current As Version
    End Class

    ''' <summary>
    ''' Asks the server and decides. <c>Info</c> is <c>Nothing</c> when the server has never
    ''' published anything (then the decision is UpToDate). Throws on a server that cannot be
    ''' reached or answers garbage -- the two entrances decide what to tell the operator.
    ''' </summary>
    Public Async Function CheckAsync(ct As CancellationToken) As Task(Of CheckResult)
        Try
            Dim current As Version = CurrentVersion
            Dim info As UpdateInfo = Await _api.GetLatestAsync(ct).ConfigureAwait(False)
            If info Is Nothing Then
                Return New CheckResult With {.Decision = UpdateDecision.UpToDate, .Info = Nothing, .Current = current}
            End If
            Return New CheckResult With {
                .Decision = UpdatePolicy.Decide(current, info.Version, info.Minimum),
                .Info = info,
                .Current = current}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.CheckAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The startup entrance (before login, no message loop yet). Returns <c>True</c> when the
    ''' process must exit now: the updater has been started, or a mandatory update was refused.
    ''' A failed check is logged and yields <c>False</c> -- never blocks an offline login.
    ''' </summary>
    Public Function RunStartupCheck() As Boolean
        Try
            Dim check As CheckResult
            Try
                Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(20))
                    check = Task.Run(Function() CheckAsync(cts.Token)).GetAwaiter().GetResult()
                End Using
            Catch ex As Exception
                ' Server down, DNS, timeout, bad JSON: all the same at startup -- note it, go on.
                GlobalErrorLog.Write("AppUpdateService.RunStartupCheck (verificarea a eșuat, aplicația continuă)", ex)
                Return False
            End Try
            Return Offer(Nothing, check, manual:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.RunStartupCheck", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The "Caută actualizări" entrance. Every outcome is shown. Returns <c>True</c> when the
    ''' caller must close the application (updater started / mandatory update refused).
    ''' </summary>
    Public Async Function RunManualCheckAsync(owner As IWin32Window) As Task(Of Boolean)
        Try
            Dim check As CheckResult
            Try
                Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(20))
                    check = Await CheckAsync(cts.Token)
                End Using
            Catch ex As ApiException
                KBotMessage.Show(owner, "Verificarea actualizărilor a eșuat:" & Environment.NewLine & ex.Message,
                                 CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            Catch ex As Exception
                KBotMessage.Show(owner, "Serverul de actualizări nu a putut fi contactat:" & Environment.NewLine & ex.Message,
                                 CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End Try
            Return Offer(owner, check, manual:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.RunManualCheckAsync", ex)
            Return False
        End Try
    End Function

    ' The shared middle: from a decision to "the process must exit" or not.
    Private Function Offer(owner As IWin32Window, check As CheckResult, manual As Boolean) As Boolean
        Select Case check.Decision
            Case UpdateDecision.UpToDate
                If manual Then
                    Dim text As String = If(check.Info Is Nothing,
                        "Serverul nu a publicat nicio actualizare. Aveți versiunea " & check.Current.ToString() & ".",
                        "Aveți ultima versiune (" & check.Current.ToString() & ").")
                    KBotMessage.Show(owner, text, CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                Return False

            Case UpdateDecision.Available
                Dim answer As DialogResult = KBotMessage.Show(owner, OfferText(check, mandatory:=False), CAPTION,
                                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                             MessageBoxDefaultButton.Button1)
                If answer <> DialogResult.Yes Then Return False
                Return DownloadAndHandOver(owner, check, mandatory:=False)

            Case UpdateDecision.Required
                Dim answer As DialogResult = KBotMessage.Show(owner, OfferText(check, mandatory:=True), CAPTION,
                                                             MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                                                             MessageBoxDefaultButton.Button1)
                If answer <> DialogResult.OK Then Return True      ' refused a mandatory update: the app closes
                If DownloadAndHandOver(owner, check, mandatory:=True) Then Return True
                ' Download cancelled or failed on a mandatory update: nothing else can run.
                KBotMessage.Show(owner, "Fără această actualizare aplicația nu poate continua. Se închide.",
                                 CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return True

            Case Else
                Throw New ArgumentException("Decizie de actualizare necunoscută: " & check.Decision.ToString())
        End Select
    End Function

    Private Shared Function OfferText(check As CheckResult, mandatory As Boolean) As String
        Dim sb As New Text.StringBuilder()
        If mandatory Then
            sb.AppendLine("Versiunea pe care o aveți (" & check.Current.ToString() & ") nu mai poate fi folosită.")
            sb.AppendLine("Este necesară actualizarea la versiunea " & check.Info.Version & ".")
        Else
            sb.AppendLine("Este disponibilă versiunea " & check.Info.Version & " a K-BOT (aveți " & check.Current.ToString() & ").")
        End If
        If Not String.IsNullOrWhiteSpace(check.Info.Notes) Then
            sb.AppendLine()
            sb.AppendLine(check.Info.Notes.Trim())
        End If
        sb.AppendLine()
        sb.AppendLine("Mărime: " & UpdateProgressForm.FormatBytes(check.Info.Size) & ".")
        sb.AppendLine()
        If mandatory Then
            sb.Append("OK = se descarcă, aplicația se închide, se actualizează și pornește din nou. Anulare = aplicația se închide.")
        Else
            sb.Append("Actualizați acum? Aplicația se închide, se actualizează și pornește din nou. «Nu» = mai târziu.")
        End If
        Return sb.ToString()
    End Function

    ' Download into a fresh temp folder, then start the updater from a copy in that same
    ' folder. True = the updater is running and this process must exit.
    Private Function DownloadAndHandOver(owner As IWin32Window, check As CheckResult, mandatory As Boolean) As Boolean
        Dim workDir As String = Nothing
        Try
            Dim updaterSource As String = Path.Combine(AppContext.BaseDirectory, UPDATER_EXE)
            If Not File.Exists(updaterSource) Then
                ' An installation older than this slice, or a hand-copied folder. Say exactly what is missing.
                KBotMessage.Show(owner,
                    "Lipsește " & UPDATER_EXE & " din folderul aplicației (" & AppContext.BaseDirectory & ")." & Environment.NewLine &
                    "Instalați o dată versiunea nouă din pachetul de instalare; de acolo încolo actualizările se fac singure.",
                    CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            workDir = Path.Combine(Path.GetTempPath(), "KBot", "update", Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(workDir)
            Dim zipPath As String = Path.Combine(workDir, If(String.IsNullOrWhiteSpace(check.Info.File), "KBot_" & check.Info.Version & ".zip", check.Info.File))

            Using dlg As New UpdateProgressForm(_api, check.Info, zipPath)
                Dim result As DialogResult = If(owner Is Nothing, dlg.ShowDialog(), dlg.ShowDialog(owner))
                If result <> DialogResult.OK Then
                    TryDeleteDir(workDir)
                    Return False
                End If
            End Using

            Dim updaterCopy As String = Path.Combine(workDir, UPDATER_EXE)
            File.Copy(updaterSource, updaterCopy, overwrite:=True)

            Dim restartExe As String = Environment.ProcessPath
            If String.IsNullOrEmpty(restartExe) Then restartExe = Application.ExecutablePath

            Dim psi As New ProcessStartInfo(updaterCopy) With {.UseShellExecute = False, .WorkingDirectory = workDir}
            psi.ArgumentList.Add("--zip") : psi.ArgumentList.Add(zipPath)
            psi.ArgumentList.Add("--target") : psi.ArgumentList.Add(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))
            psi.ArgumentList.Add("--wait") : psi.ArgumentList.Add(Environment.ProcessId.ToString())
            psi.ArgumentList.Add("--restart") : psi.ArgumentList.Add(restartExe)
            psi.ArgumentList.Add("--sha256") : psi.ArgumentList.Add(check.Info.Sha256)
            psi.ArgumentList.Add("--version") : psi.ArgumentList.Add(check.Info.Version)
            Process.Start(psi)
            OperatorLog.Write("AppUpdateService", CAPTION,
                              "Actualizare pornită: " & check.Current.ToString() & " -> " & check.Info.Version &
                              If(mandatory, " (obligatorie)", "") & "; pachet " & zipPath)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.DownloadAndHandOver", ex)
            KBotMessage.Show(owner, "Actualizarea nu a putut fi pornită:" & Environment.NewLine & ex.Message,
                             CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error)
            If workDir IsNot Nothing Then TryDeleteDir(workDir)
            Return False
        End Try
    End Function

    Private Shared Sub TryDeleteDir(dir As String)
        Try
            If Directory.Exists(dir) Then Directory.Delete(dir, recursive:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("AppUpdateService.TryDeleteDir", ex)
        End Try
    End Sub
End Class
