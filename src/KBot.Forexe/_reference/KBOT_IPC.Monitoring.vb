Imports System.IO
Imports System.Windows.Forms
Imports GeneralClasses

Partial Public Class KBOT_IPC
    Private Sub InitializeTrayComponents()
        If _notifyIcon IsNot Nothing Then Return

        ' Configurare Iconiță
        _notifyIcon = New NotifyIcon With {
            .Icon = Me.Icon,
            .Text = "ForexeBot - Inițializare...",
            .Visible = False
        }

        Dim ctxMenu As New ContextMenuStrip()

        Dim itemChangeTheme As New ToolStripMenuItem() With {.Text = If(KBotTheme.IsDark, "☀️", "🌙")}
        AddHandler itemChangeTheme.Click, Sub(s, e)
                                              KBotTheme.SetTheme(Not KBotTheme.IsDark)
                                              itemChangeTheme.Text = If(KBotTheme.IsDark, "☀️", "🌙")
                                          End Sub
        ctxMenu.Items.Add(itemChangeTheme)

        ctxMenu.Items.Add(New ToolStripSeparator())

        Dim itemOnTop As New ToolStripMenuItem("Afișează App Deasupra")
        AddHandler itemOnTop.Click, Sub(s, e)
                                        Me.TopMost = Not Me.TopMost
                                        ' Nu setăm .Checked aici manual, lăsăm evenimentul Opening să se ocupe
                                    End Sub
        ctxMenu.Items.Add(itemOnTop)

        ' --- B. BROWSER TOPMOST (Cerința 3) ---
        Dim itemBrowserTop As New ToolStripMenuItem("Browser Deasupra (TopMost)")
        AddHandler itemBrowserTop.Click, Async Sub(s, e)
                                             If _executor IsNot Nothing Then
                                                 Dim newState As Boolean = Not itemBrowserTop.Checked
                                                 Await _executor.SetBrowserTopMostAsync(newState)
                                                 ' Bifa se va actualiza la redeschiderea meniului, dar o putem forța și aici vizual dacă vrem feedback instant
                                                 itemBrowserTop.Checked = newState
                                             End If
                                         End Sub
        ctxMenu.Items.Add(itemBrowserTop)

        ' Separator
        ctxMenu.Items.Add(New ToolStripSeparator())

        ' --- C. ISTORIC (Cerința 4) ---
        Dim itemHistory As New ToolStripMenuItem("Istoric Acțiuni")
        AddHandler itemHistory.Click, Sub(s, e)
                                          Dim hForm As New HistoryForm()
                                          AddHandler hForm.ResendRequested,
                                      Sub(json As String, requiresAck As Boolean)
                                          ResendRawMessage(json, requiresAck)
                                      End Sub
                                          hForm.Show()
                                      End Sub
        ctxMenu.Items.Add(itemHistory)

        ' Separator
        ctxMenu.Items.Add(New ToolStripSeparator())

        ' Salvare mesaje Resend și încărcare pachet Resend
        Dim itemResendSave As New ToolStripMenuItem("💾 Salvează mesaje Resend")
        AddHandler itemResendSave.Click, Sub(s, e) ManualSaveResendPackage()
        ctxMenu.Items.Add(itemResendSave)

        Dim itemResendLoad As New ToolStripMenuItem("📂 Încarcă mesaje Resend...")
        AddHandler itemResendLoad.Click, Sub(s, e) LoadResendPackageFromDialog()
        ctxMenu.Items.Add(itemResendLoad)

        ' --- D. SUBMENIU WORKFLOW ---
        Dim subWorkflow As New ToolStripMenuItem("⚙ Workflow") With {.Name = "subWorkflow"}

        Dim itemEditWfl As New ToolStripMenuItem("Editează Workflow...") With {.Name = "itemEditWfl"}
        AddHandler itemEditWfl.Click, AddressOf OnEditWorkflowClick
        subWorkflow.DropDownItems.Add(itemEditWfl)

        subWorkflow.DropDownItems.Add(New ToolStripSeparator())

        Dim itemBringAll As New ToolStripMenuItem("Aduce toate în față") With {.Name = "itemBringAll"}
        AddHandler itemBringAll.Click, AddressOf OnBringAllEditorsClick
        subWorkflow.DropDownItems.Add(itemBringAll)

        Dim itemReloadAll As New ToolStripMenuItem("Reîncarcă toate din disc") With {.Name = "itemReloadAll"}
        AddHandler itemReloadAll.Click, AddressOf OnReloadAllEditorsClick
        subWorkflow.DropDownItems.Add(itemReloadAll)

        Dim itemCloseAll As New ToolStripMenuItem("Închide toate editorele") With {.Name = "itemCloseAll"}
        AddHandler itemCloseAll.Click, AddressOf OnCloseAllEditorsClick
        subWorkflow.DropDownItems.Add(itemCloseAll)

        ctxMenu.Items.Add(subWorkflow)

        ctxMenu.Items.Add(New ToolStripSeparator())

        ' Restaurare Consolă
        ctxMenu.Items.Add("Arată Consolă", Nothing, AddressOf RestoreWindowFromTray)

        ' Browser Show/Hide
        Dim itemBrowser As New ToolStripMenuItem("Afișează Browser")
        AddHandler itemBrowser.Click, Async Sub(s, e)
                                          If _executor IsNot Nothing Then
                                              If _executor.IsBrowserVisible Then
                                                  Await _executor.HideBrowserWindowAsync()
                                              Else
                                                  Await _executor.ShowBrowserWindowAsync()
                                              End If
                                          End If
                                      End Sub
        ctxMenu.Items.Add(itemBrowser)

        ' --- HANDLER OPENING (CRITIC PENTRU SYSTRAY FIX) ---
        AddHandler ctxMenu.Opening, Sub(s, e)
                                        ' 1. Sincronizare App TopMost
                                        itemOnTop.Checked = Me.TopMost

                                        ' 2. Sincronizare Browser / Executor — dezactivate total în light mode
                                        If _isLightMode Then
                                            itemBrowser.Enabled = False
                                            itemBrowser.Text = "Afișează Browser (N/A — Resend Only)"
                                            itemBrowser.Checked = False
                                            itemBrowserTop.Enabled = False
                                            _notifyIcon.Text = "ForexeBot — Resend Only"
                                        ElseIf _executor IsNot Nothing Then
                                            itemBrowser.Enabled = True
                                            itemBrowserTop.Enabled = True

                                            If _executor.IsBrowserVisible Then
                                                itemBrowser.Text = "Ascunde Browser"
                                                itemBrowser.Checked = True
                                            Else
                                                itemBrowser.Text = "Afișează Browser"
                                                itemBrowser.Checked = False
                                            End If
                                        Else
                                            itemBrowser.Enabled = False
                                            itemBrowser.Text = "Afișează Browser"
                                            itemBrowser.Checked = False
                                            itemBrowserTop.Enabled = False
                                        End If
                                    End Sub

        ' --- SALVARE LOG ---
        ctxMenu.Items.Add(New ToolStripSeparator())

        Dim itemSaveLog As New ToolStripMenuItem("💾 Salvează Log...")
        AddHandler itemSaveLog.Click, Sub(s, e)
                                          Using sfd As New SaveFileDialog With {
        .Title = "Salvează log curent",
        .Filter = "RTF files (*.rtf)|*.rtf",
        .DefaultExt = "rtf",
        .FileName = $"ForexeBot_Log_{DateTime.Now:yyyyMMdd_HHmmss}.rtf"
    }
                                              If sfd.ShowDialog() <> DialogResult.OK Then Return
                                              Try
                                                  rtbLog.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText)
                                                  _logger?.LogSuccess($"[LOG] Log salvat: {sfd.FileName}")
                                              Catch ex As Exception
                                                  MessageBox.Show($"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                              End Try
                                          End Using
                                      End Sub
        ctxMenu.Items.Add(itemSaveLog)

        AddWicketMonitorMenuItem(ctxMenu)

        ' Ieșire
        ctxMenu.Items.Add(New ToolStripSeparator())
        ctxMenu.Items.Add("Ieșire Robot", Nothing, Sub(s, e)
                                                       _forceExit = True
                                                       Application.Exit()
                                                   End Sub)

        _notifyIcon.ContextMenuStrip = ctxMenu

        ' Event dublu-click
        AddHandler _notifyIcon.DoubleClick, AddressOf RestoreWindowFromTray

        _trayTimer = New System.Windows.Forms.Timer With {.Interval = 1000}
        AddHandler _trayTimer.Tick, AddressOf OnTrayTimerTick
        _trayTimer.Start()
    End Sub

    Private Sub RestoreWindowFromTray(sender As Object, e As EventArgs)
        Me.Show()
        Me.BringToFront()
        Me.Activate()

        RestoreWindowFromTray_UIUpdate()
    End Sub

    Private Sub RestoreWindowFromTray_UIUpdate()
        Me.ShowInTaskbar = True
        Me.WindowState = FormWindowState.Normal

        Me.lblTitleLog.Visible = True
        Me.rtbLog.Visible = True
        Me.tlpLayout.RowStyles(1).SizeType = SizeType.AutoSize
        Me.tlpLayout.RowStyles(2).SizeType = SizeType.Percent
        Me.tlpLayout.RowStyles(2).Height = 70.0F
        Me.Height = 600

        _isConsoleVisible = True
    End Sub

    ''' <summary>
    ''' Salvează automat log-ul curent în _jobFolderPath, fără dialog.
    ''' Apelată la comanda CMD_SAVE_LOG din VBA.
    ''' </summary>
    Private Sub SaveLogAutomatic()
        Try
            ' Folder destinație: același cu job folder-ul, sau lângă exe ca fallback
            Dim folder As String = _jobFolderPath
            If String.IsNullOrEmpty(folder) OrElse Not Directory.Exists(folder) Then
                folder = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location)
            End If

            Dim fileName As String = $"ForexeBot_Log_{DateTime.Now:yyyyMMdd_HHmmss}.rtf"
            Dim filePath As String = Path.Combine(folder, fileName)

            ' Salvăm RTF direct din control (cu culori)
            rtbLog.SaveFile(filePath, RichTextBoxStreamType.RichText)

            _logger?.LogSuccess($"[LOG] Log salvat automat: {filePath}")

            ' Răspuns înapoi către VBA cu calea completă
            SendMessageToPipe(PipeCmd.LOG_SAVED, _currentTaskId, filePath)

        Catch ex As Exception
            _logger?.LogError($"[LOG] Eroare la salvarea automată: {ex.Message}")
            ' Trimitem eroarea înapoi ca JOB_ERROR fire-and-forget
            SendMessageToPipe(PipeCmd.LOG_SAVED, _currentTaskId, $"EROARE: {ex.Message}")
        End Try
    End Sub

    Private Sub EnterStandbyMode()
        If _isStandbyMode Then Return
        _isStandbyMode = True

        _logger.LogInfo("--- INTRARE ÎN MOD STANDBY ---")
        'SendMessageToPipe(PipeCmd.STANDBY, 0, "Intru în modul service ascuns.")

        If _executor IsNot Nothing AndAlso _executor.IsBrowserVisible Then
            Dim unused = _executor.HideBrowserWindowAsync()
        End If

        ' Asigurăm că componentele Tray există
        InitializeTrayComponents()

        ' Ascundem forma
        Me.ShowInTaskbar = False
        Me.WindowState = FormWindowState.Minimized
        Me.Hide()

        ' Activăm Tray
        _notifyIcon.Visible = True
        UpdateLightModeUI()
        _trayTimer.Start()

        ' Watchdog Timer (Monitorizează dacă fereastra Access mai există)
        _accessWatchdog = New System.Windows.Forms.Timer With {.Interval = 2000}
        AddHandler _accessWatchdog.Tick, AddressOf OnAccessWatchdogTick
        _accessWatchdog.Start()
    End Sub

    ''' <summary>
    ''' Actualizează tooltip-ul tray și starea elementelor de meniu legate de executor
    ''' în funcție de modul curent (light vs normal).
    ''' Poate fi apelat thread-safe.
    ''' </summary>
    Friend Sub UpdateLightModeUI()
        If InvokeRequired Then
            Invoke(Sub() UpdateLightModeUI())
            Return
        End If

        If _notifyIcon Is Nothing Then Return  ' Tray-ul nu e inițializat încă

        If _isLightMode Then
            _notifyIcon.Text = "ForexeBot — Resend Only"
        Else
            _notifyIcon.Text = "ForexeBot"
        End If
    End Sub

    Friend Sub AddWicketMonitorMenuItem(ctxMenu As ContextMenuStrip)
        ctxMenu.Items.Add(New ToolStripSeparator())

        Dim item As New ToolStripMenuItem("Wicket Monitor")
        AddHandler item.Click, Async Sub(s, e)
                                   Try
                                       Await ShowWicketMonitorAsync()
                                   Catch
                                       ' silently ignore — monitorizarea e opțională
                                   End Try
                               End Sub
        ctxMenu.Items.Add(item)
    End Sub

    ' =========================================================================
    '  ShowWicketMonitorAsync
    ' =========================================================================
    Private Async Function ShowWicketMonitorAsync() As Task
        If _executor Is Nothing Then
            MessageBox.Show(
                "Browserul nu este activ." & Environment.NewLine &
                "Pornește o sesiune înainte de a deschide monitorul.",
                "Wicket Monitor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If

        ' Activăm monitorizarea dacă nu e deja activă
        If Not _executor.WicketMonitoringActive Then
            Await _executor.StartWicketMonitoringAsync()
        End If

        ' Creare sau refolosire fereastră singleton
        If _wicketMonitorForm Is Nothing OrElse _wicketMonitorForm.IsDisposed Then
            _wicketMonitorForm = New WicketMonitorForm()
        End If

        ' Reataș executorul curent (gestionează și cazul de executor nou)
        _wicketMonitorForm.AttachExecutor(_executor)

        _wicketMonitorForm.Show()
        _wicketMonitorForm.BringToFront()
    End Function

    Private Sub OnAccessWatchdogTick(sender As Object, e As EventArgs)
#If DEBUG Then
        ' În modul de testare, nu avem o fereastră Access reală, deci ignorăm verificarea
        Return
#End If
        If Not IsWindow(_hwndWatcher) AndAlso _hwndWatcher <> 0 Then
            _accessWatchdog.Stop()
            _logger.LogWarning("Access-ul părinte a dispărut. Mă închid.")
            Application.Exit()
        End If
    End Sub

    Private Sub OnTrayTimerTick(sender As Object, e As EventArgs)
        If _notifyIcon Is Nothing OrElse Not _notifyIcon.Visible Then Return

        Dim statusText As String = "ForexeBot: Activ"

        If _cacheExpirationTime = DateTime.MaxValue Then
            statusText &= vbCrLf & "Cache: Permanent (Sesiune)"
        ElseIf DateTime.Now < _cacheExpirationTime Then
            Dim remaining = _cacheExpirationTime - DateTime.Now
            statusText &= vbCrLf & $"Cache: {remaining:mm\:ss}"
        Else
            statusText &= vbCrLf & "Cache: Expirat"
        End If

        ' Limitare text (Windows are o limită de 63 sau 127 caractere pentru tooltip tray)
        If statusText.Length > 63 Then statusText = String.Concat(statusText.AsSpan(0, 60), "...")
        _notifyIcon.Text = statusText
    End Sub

    Private Sub OnEditWorkflowClick(sender As Object, e As EventArgs)
        Using dlg As New OpenFileDialog()
            dlg.Title = "Selectează fișier workflow"
            dlg.Filter = "Workflow files (*.wfl)|*.wfl|Toate fișierele (*.*)|*.*"
            dlg.DefaultExt = "wfl"
            If Not String.IsNullOrEmpty(_jobFolderPath) AndAlso Directory.Exists(_jobFolderPath) Then
                dlg.InitialDirectory = _jobFolderPath
            End If
            If dlg.ShowDialog() <> DialogResult.OK Then Return

            Dim editor As New WorkflowEditorForm(dlg.FileName)
            _openEditors.Add(editor)
            AddHandler editor.FormClosed, Sub(s, ev)
                                              _openEditors.Remove(DirectCast(s, WorkflowEditorForm))
                                          End Sub
            editor.Show()
        End Using
    End Sub

    Private Sub OnBringAllEditorsClick(sender As Object, e As EventArgs)
        For Each ed In _openEditors.ToList()
            If Not ed.IsDisposed Then
                ed.BringToFront()
                ed.Activate()
            End If
        Next
    End Sub

    Private Sub OnReloadAllEditorsClick(sender As Object, e As EventArgs)
        For Each ed In _openEditors.ToList()
            If Not ed.IsDisposed Then
                ed.ReloadFromDisk()
            End If
        Next
    End Sub

    Private Sub OnCloseAllEditorsClick(sender As Object, e As EventArgs)
        For Each ed In _openEditors.ToList()
            If Not ed.IsDisposed Then
                ed.Close()
            End If
        Next
    End Sub
End Class
