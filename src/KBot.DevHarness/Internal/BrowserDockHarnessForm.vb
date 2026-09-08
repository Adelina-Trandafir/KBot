Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports Microsoft.Playwright

' =============================================================================
'  BrowserDockHarnessForm - the bench for docking.
'
'  One button does the whole sequence the recorder does on a real session:
'    launch Chromium -> install the Wicket/click monitor -> navigate to the FOREXE
'    public page -> reparent the browser window into pnlBrowser.
'
'  Nothing here is a copy of the monitor: pnlMonitor hosts the REAL WicketMonitorForm
'  (TopLevel = False), attached to the same executor, so whatever the bench shows is
'  exactly what the monitor window shows in the application.
'
'  Order matters at start-up. StartWicketMonitoringAsync installs its script with
'  AddInitScriptAsync, and an init script only runs on the NEXT navigation - so the
'  monitor goes in BEFORE the page is loaded, never after. "Reincarca pagina" exists
'  for the same reason: it is the way to make a late started monitor live.
'
'  The docked window is a child of pnlBrowser and dies with it, so FormClosing undocks
'  before anything is disposed.
'
'  Controls are declared in BrowserDockHarnessForm.Designer.vb.
' =============================================================================
Friend NotInheritable Class BrowserDockHarnessForm

    ''' <summary>The page the operator asked the bench to open.</summary>
    Private Const TargetUrl As String = "https://mfinante.gov.ro/web/forexepublic"

    Private ReadOnly _log As Action(Of String)
    Private _logger As RichTextBoxLogger = Nothing
    Private _executor As WorkflowExecutor = Nothing
    Private _monitor As WicketMonitorForm = Nothing
    Private _busy As Boolean = False
    Private _closingDown As Boolean = False

    ' =========================================================================
    '  Constructor / Load
    ' =========================================================================
    Public Sub New(log As Action(Of String))
        InitializeComponent()
        _log = If(log, Sub(m As String)
                       End Sub)
        KBotTheme.ApplyTheme(Me)
    End Sub

    Private Sub BrowserDockHarnessForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            KBotTheme.ApplyTheme(Me)

            ' The real monitor window, hosted instead of copied.
            _monitor = New WicketMonitorForm() With {
                .TopLevel = False,
                .FormBorderStyle = FormBorderStyle.None,
                .Dock = DockStyle.Fill
            }
            pnlMonitor.Controls.Add(_monitor)
            _monitor.Show()

            UpdateButtons()
            AppendLog("Banc de probă pornit. Apasă «Pornește + navighează».")
            AppendLog("Fluxul [WICKET] rămâne tăcut în afara FOREXE: #statlogo/#animlogo " &
                      "nu există pe pagina publică. [CLICK] și [KEY] funcționează oriunde.")
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.Load", ex)
            AppendLog("Eroare la pornirea bancului: " & ex.Message)
        End Try
    End Sub

    ' =========================================================================
    '  Pornire: lansare + monitor + navigare + andocare
    ' =========================================================================
    Private Async Sub BtnPornire_Click(sender As Object, e As EventArgs) Handles btnPornire.Click
        If _busy Then Return
        _busy = True
        UpdateButtons()
        Try
            If _executor Is Nothing Then
                _logger = New RichTextBoxLogger(rtbLog) With {
                    .EnableUI = True,
                    .LogFilePath = Path.Combine(LogPaths.EnsureLogsDirectory(),
                                                $"harness_dock_{DateTime.Now:yyyyMMdd_HHmmss}.log")
                }
                ' No certificate and no stealth: this bench never authenticates, it only
                ' has to put a real browser window on the screen.
                _executor = New WorkflowExecutor(_logger, Nothing, False)
                AddHandler _executor.OnBrowserClosed, AddressOf HandleBrowserClosed
                _monitor?.AttachExecutor(_executor)
            End If

            _executor.HideChromeWhenDocked = chkBaraBrowser.Checked

            If Not _executor.IsBrowserOpen Then
                SetStare("Pornesc browserul...")
                Await _executor.LaunchAndPositionBrowserAsync()
            End If

            ' Before the navigation, always: the monitor's script is an init script.
            If Not _executor.WicketMonitoringActive Then
                SetStare("Instalez monitorul...")
                Await _executor.StartWicketMonitoringAsync()
            End If

            SetStare("Navighez la " & TargetUrl & " ...")
            Await _executor.CurrentPage.GotoAsync(TargetUrl, New PageGotoOptions With {
                .WaitUntil = WaitUntilState.DOMContentLoaded,
                .Timeout = 60000.0F
            })

            If Not _executor.IsDocked Then
                SetStare("Andochez browserul...")
                Await _executor.DockBrowserToAsync(pnlBrowser)
            End If

            SetStare("Browser andocat pe pagina publică FOREXE.")
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnPornire_Click", ex)
            AppendLog("EROARE: " & ex.Message)
            SetStare("Pornire eșuată: " & ex.Message)
        Finally
            _busy = False
            UpdateButtons()
        End Try
    End Sub

    ' =========================================================================
    '  Andocare / detașare / resincronizare
    ' =========================================================================

    ''' <summary>
    ''' Toolbar of the browser itself - tabs and address bar - in or out of the panel. Takes
    ''' effect at once on a docked browser, so the bench shows both states without a restart.
    ''' </summary>
    Private Sub ChkBaraBrowser_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkBaraBrowser.CheckedChanged
        Try
            If _executor Is Nothing Then Return
            _executor.HideChromeWhenDocked = chkBaraBrowser.Checked
            AppendLog(If(chkBaraBrowser.Checked,
                         "Bara browserului: ascunsă (se vede doar pagina).",
                         "Bara browserului: vizibilă (file + bară de adrese)."))
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.ChkBaraBrowser_CheckedChanged", ex)
            AppendLog("EROARE la comutarea barei browserului: " & ex.Message)
        End Try
    End Sub

    Private Async Sub BtnAndocare_Click(sender As Object, e As EventArgs) Handles btnAndocare.Click
        Try
            If _executor Is Nothing Then Return
            Await _executor.DockBrowserToAsync(pnlBrowser)
            SetStare("Andocat.")
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnAndocare_Click", ex)
            AppendLog("EROARE la andocare: " & ex.Message)
            SetStare("Andocare eșuată: " & ex.Message)
        Finally
            UpdateButtons()
        End Try
    End Sub

    Private Async Sub BtnDetasare_Click(sender As Object, e As EventArgs) Handles btnDetasare.Click
        Try
            If _executor Is Nothing Then Return
            Await _executor.UndockBrowserAsync()
            SetStare("Detașat.")
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnDetasare_Click", ex)
            AppendLog("EROARE la detașare: " & ex.Message)
        Finally
            UpdateButtons()
        End Try
    End Sub

    Private Async Sub BtnSincronizare_Click(sender As Object, e As EventArgs) _
        Handles btnSincronizare.Click
        Try
            If _executor Is Nothing Then Return
            Await _executor.SyncDockedBoundsAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnSincronizare_Click", ex)
            AppendLog("EROARE la resincronizare: " & ex.Message)
        End Try
    End Sub

    Private Async Sub BtnReincarca_Click(sender As Object, e As EventArgs) Handles btnReincarca.Click
        Try
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then Return
            Await _executor.CurrentPage.ReloadAsync(New PageReloadOptions With {
                .WaitUntil = WaitUntilState.DOMContentLoaded,
                .Timeout = 60000.0F
            })
            SetStare("Pagină reîncărcată.")
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnReincarca_Click", ex)
            AppendLog("EROARE la reîncărcare: " & ex.Message)
        End Try
    End Sub

    Private Async Sub BtnMonitor_Click(sender As Object, e As EventArgs) Handles btnMonitor.Click
        Try
            If _executor Is Nothing Then Return
            If _executor.WicketMonitoringActive Then
                _executor.StopWicketMonitoring()
                AppendLog("Monitor oprit.")
            Else
                Await _executor.StartWicketMonitoringAsync()
                AppendLog("Monitor pornit. Reîncarcă pagina ca scriptul să intre în ea.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.BtnMonitor_Click", ex)
            AppendLog("EROARE la monitor: " & ex.Message)
        Finally
            UpdateButtons()
        End Try
    End Sub

    ' =========================================================================
    '  Urmărirea panoului
    ' =========================================================================
    Private Sub PnlBrowser_Resize(sender As Object, e As EventArgs) Handles pnlBrowser.Resize
        ScheduleResync()
    End Sub

    Private Sub SplitMain_SplitterMoved(sender As Object, e As SplitterEventArgs) _
        Handles splitMain.SplitterMoved
        ScheduleResync()
    End Sub

    ''' <summary>Debounce: a drag fires hundreds of resize events.</summary>
    Private Sub ScheduleResync()
        Try
            If _executor Is Nothing OrElse Not _executor.IsDocked Then Return
            tmrResync.Stop()
            tmrResync.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.ScheduleResync", ex)
        End Try
    End Sub

    Private Async Sub TmrResync_Tick(sender As Object, e As EventArgs) Handles tmrResync.Tick
        tmrResync.Stop()
        Try
            If _executor Is Nothing OrElse Not _executor.IsDocked Then Return
            Await _executor.SyncDockedBoundsAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.TmrResync_Tick", ex)
        End Try
    End Sub

    ' =========================================================================
    '  Verdict
    ' =========================================================================
    Private Sub BtnPass_Click(sender As Object, e As EventArgs) Handles btnPass.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnFail_Click(sender As Object, e As EventArgs) Handles btnFail.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ' =========================================================================
    '  Stare / log
    ' =========================================================================
    Private Sub HandleBrowserClosed(message As String)
        If Me.IsDisposed Then Return
        If Me.InvokeRequired Then
            Me.BeginInvoke(Sub() HandleBrowserClosed(message))
            Return
        End If
        AppendLog("Browserul s-a închis: " & message)
        SetStare("Browser închis.")
        UpdateButtons()
    End Sub

    Private Sub SetStare(text As String)
        lblStare.Text = text
        _log("[banc andocare] " & text)
    End Sub

    Private Sub AppendLog(text As String)
        rtbLog.AppendText(text & Environment.NewLine)
        rtbLog.SelectionStart = rtbLog.TextLength
        rtbLog.ScrollToCaret()
        _log("[banc andocare] " & text)
    End Sub

    Private Sub UpdateButtons()
        Dim hasExecutor As Boolean = _executor IsNot Nothing
        Dim browserOpen As Boolean = hasExecutor AndAlso _executor.IsBrowserOpen
        Dim docked As Boolean = hasExecutor AndAlso _executor.IsDocked

        btnPornire.Enabled = Not _busy
        btnAndocare.Enabled = browserOpen AndAlso Not docked AndAlso Not _busy
        btnDetasare.Enabled = docked AndAlso Not _busy
        btnSincronizare.Enabled = docked AndAlso Not _busy
        btnReincarca.Enabled = browserOpen AndAlso Not _busy
        btnMonitor.Enabled = browserOpen AndAlso Not _busy
        btnMonitor.Text = If(hasExecutor AndAlso _executor.WicketMonitoringActive,
                             "Oprește monitorul", "Pornește monitorul")
    End Sub

    ' =========================================================================
    '  Teardown - undock FIRST: the docked window is a child of pnlBrowser and
    '  would be destroyed with it, taking the Chromium session along.
    ' =========================================================================
    Private Async Sub BrowserDockHarnessForm_FormClosing(sender As Object, e As FormClosingEventArgs) _
        Handles Me.FormClosing
        If _closingDown Then Return

        e.Cancel = True
        _closingDown = True
        Try
            tmrResync.Stop()
            If _executor IsNot Nothing Then
                _executor.StopWicketMonitoring()
                If _executor.IsDocked Then Await _executor.UndockBrowserAsync()
                Await _executor.CloseAsync()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("BrowserDockHarnessForm.FormClosing", ex)
        End Try

        Me.Close()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            Try
                If _executor IsNot Nothing Then
                    RemoveHandler _executor.OnBrowserClosed, AddressOf HandleBrowserClosed
                    _executor = Nothing
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("BrowserDockHarnessForm.Dispose", ex)
            End Try
            If _monitor IsNot Nothing Then
                _monitor.Dispose()
                _monitor = Nothing
            End If
            If components IsNot Nothing Then components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
