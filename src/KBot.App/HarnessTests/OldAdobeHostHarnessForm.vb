#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The bench for slice 0078-12: ONLY the hosted Adobe window, on an OLDER Adobe (before 2024:
''' Acrobat DC / 2020), with the script-error monitor in view. Drives <see cref="AdobeReaderHost"/>
''' directly -- the engine the DDF / ORD pages use when «Fereastră găzduită» is chosen -- without the
''' signing session, the server or the print watch, so what is seen here is the Adobe window alone.
'''
''' <para><b>Why a bench of its own.</b> The hosted window is correct for Reader 2024 and newer; on an
''' older Acrobat (the operator's local Acrobat Pro 2020) it behaves differently, and the form's
''' scripts raise errors there that only the ActiveX engine showed before. The signing bench mixes
''' the engine with the server round trip; this one is the engine and nothing else.</para>
'''
''' <para><b>What to watch.</b> Top line: which Adobe was found and whether it is before 2024. Right,
''' top: one row per script window the document gave (an error K-BOT pressed OK on, a message of the
''' form it left to the operator, the JavaScript Debugger console it hid) and the running tally.
''' Right, bottom: the live working log (<c>adobe_preview.log</c>, marked «[Adobe]»). «Jurnal detaliat»
''' also writes <c>acropdf_trace.log</c> (every trap sweep, the window tree, the keys). «Copiază
''' raportul» puts everything on the clipboard to be pasted into a report.</para>
'''
''' <para><b>Same preface as the application.</b> Before a document: the classic-interface option, the
''' standard Save As dialog preference, the operator's «new instance» and detach settings. «Lista
''' mesajelor închise automat…» is the REAL list in «Setări»: changing it changes the application's.
''' Always on a copy: the PDF is copied into <c>Temp\PDF\Banc\</c>, because the Save As trap may
''' overwrite the hosted file.</para>
''' </summary>
Public NotInheritable Class OldAdobeHostHarnessForm

    Private Const BenchFolder As String = "Banc"
    Private Const ReportLogLines As Integer = 250

    Private ReadOnly _host As AdobeReaderHost
    Private ReadOnly _log As Action(Of String)
    ' The trace switch as the bench found it: put back when the bench closes (the switch is in memory only).
    Private ReadOnly _traceBefore As Boolean
    Private _path As String
    Private _clock As Stopwatch
    Private _hostedMs As Integer = -1
    Private _readyMs As Integer = -1
    Private _savePrefDone As Boolean
    Private _logHooked As Boolean

    Public Sub New(k_log As Action(Of String))
        InitializeComponent()
        _log = k_log
        _traceBefore = AcroPdfTraceLog.SwitchedOn
        chkTrace.Checked = _traceBefore
        ' The same log callback as ReaderHostPreview: lines reach adobe_preview.log AND the bench.
        _host = New AdobeReaderHost(pnlHost, AddressOf AdobeHostLog.Write)
        AddHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
        _logHooked = True
        AddHandler _host.ScriptAlertSeen, AddressOf OnScriptAlert
        AddHandler _host.DocumentReady, AddressOf OnDocumentReady
        AddHandler _host.HostedWindowClosed, AddressOf OnHostedWindowClosed
        AddHandler _host.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler _host.SaveTrapFailed, AddressOf OnSaveTrapFailed
        ShowAdobeInfo()
        UpdateCounters()
        Write("Banc pornit: doar fereastra găzduită, fără sesiune de semnare și fără server. " &
              "Setările din «Setări» nu se schimbă (în afară de lista mesajelor, dacă o editezi de aici).")
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Try
            If dlgOpen.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_target As String = BenchPath(dlgOpen.FileName)
            ReleaseDocument()
            File.Copy(dlgOpen.FileName, k_target, overwrite:=True)
            Write($"Copiat «{dlgOpen.FileName}» în «{k_target}» (originalul nu se atinge).")
            OpenAsync(k_target)
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnOpen_Click", ex)
            Write("Nu s-a putut deschide: " & ex.Message)
        End Try
    End Sub

    Private Sub btnReopen_Click(sender As Object, e As EventArgs) Handles btnReopen.Click
        Try
            If String.IsNullOrEmpty(_path) OrElse Not File.Exists(_path) Then
                Write("Nu e niciun document de redeschis.")
                Return
            End If
            OpenAsync(_path)
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnReopen_Click", ex)
            Write("Redeschiderea a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnRelease_Click(sender As Object, e As EventArgs) Handles btnRelease.Click
        Try
            ReleaseDocument()
            Write("Documentul a fost eliberat.")
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnRelease_Click", ex)
        End Try
    End Sub

    Private Sub chkReadMode_CheckedChanged(sender As Object, e As EventArgs) Handles chkReadMode.CheckedChanged
        If _host Is Nothing Then Return     ' raised inside InitializeComponent / the constructor, before the host exists
        Write("Ctrl+H / F8 / Ctrl+2 " & If(chkReadMode.Checked, "se trimit", "NU se trimit") & " de la documentul următor.")
    End Sub

    Private Sub chkSaveTrap_CheckedChanged(sender As Object, e As EventArgs) Handles chkSaveTrap.CheckedChanged
        If _host Is Nothing Then Return     ' raised inside InitializeComponent, before the host exists
        Try
            _host.SaveTrapEnabled = chkSaveTrap.Checked
            Write("Capcana este " & If(chkSaveTrap.Checked, "pornită.", "OPRITĂ: nici «Salvare ca», nici mesajele de script nu mai sunt tratate de K-BOT."))
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.chkSaveTrap_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub chkTrace_CheckedChanged(sender As Object, e As EventArgs) Handles chkTrace.CheckedChanged
        If _host Is Nothing Then Return     ' raised by the constructor (chkTrace.Checked = ...) before the host exists
        AcroPdfTraceLog.SwitchedOn = chkTrace.Checked
        Write(If(chkTrace.Checked, "Jurnalul detaliat e pornit: se scrie în Logs\" & AcroPdfTraceLog.FileNameOnly & " de la documentul următor.",
                 "Jurnalul detaliat e oprit."))
    End Sub

    Private Sub btnAlertList_Click(sender As Object, e As EventArgs) Handles btnAlertList.Click
        Try
            Using k_form As New AdobeMesajeForm()
                If k_form.ShowDialog(Me) = DialogResult.OK Then Write(k_form.Rezumat)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnAlertList_Click", ex)
            Write("Lista mesajelor nu a putut fi deschisă: " & ex.Message)
        End Try
    End Sub

    ' The hosted window's own tree, and every top-level window of the Adobe processes (a script alert
    ' or a Save As lives there, not in the tree): what to read when a dialog stays on screen.
    Private Sub btnTree_Click(sender As Object, e As EventArgs) Handles btnTree.Click
        Try
            If Not _host.IsHosting Then
                Write("Niciun document găzduit — nu e ce să arăt.")
                Return
            End If
            Write($"── Arborele ferestrei găzduite 0x{_host.HostedWindow.ToInt64():X} (PID {_host.HostedPid}) ──")
            For Each k_node As AdobeWindowNode In AdobeWindowProbe.Walk(_host.HostedWindow, pnlHost.Handle, 8)
                Write(AdobeWindowProbe.DescribeNode(k_node))
            Next
            Dim k_pids As List(Of Integer) = AdobeWindowHosting.AdobeProcessIds()
            Dim k_win As INativeWindows = Win32Windows.Instance
            Write($"── Ferestrele de sus ale proceselor Adobe (PID: {String.Join(", ", k_pids)}) ──")
            For Each k_h As IntPtr In k_win.EnumTopLevelWindows()
                Dim k_pid As Integer = k_win.OwnerPid(k_h)
                If Not k_pids.Contains(k_pid) Then Continue For
                Write($"  0x{k_h.ToInt64():X} pid={k_pid} clasă={k_win.GetClass(k_h)} vizibilă={k_win.IsWindowVisible(k_h)} " &
                      $"titlu=«{k_win.GetTitle(k_h)}»")
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnTree_Click", ex)
            Write("Arborele nu a putut fi citit: " & ex.Message)
        End Try
    End Sub

    Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            Dim k_report As String = BuildReport()
            Clipboard.SetText(k_report)
            Write($"Raportul a fost copiat în clipboard ({k_report.Split(ChrW(10)).Length} linii).")
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.btnCopy_Click", ex)
            Write("Raportul nu a putut fi copiat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnClearLog_Click(sender As Object, e As EventArgs) Handles btnClearLog.Click
        txtLog.Clear()
    End Sub

    Private Sub pnlHost_SizeChanged(sender As Object, e As EventArgs) Handles pnlHost.SizeChanged
        Try
            ' Raised inside InitializeComponent, before _host exists: nothing hosted yet.
            _host?.Relayout()
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.pnlHost_SizeChanged", ex)
        End Try
    End Sub

    ' ── Opening a document ──────────────────────────────────────────────────────
    ' UI boundary (async void): log and swallow. The host never throws; it answers with a status.
    Private Async Sub OpenAsync(k_path As String)
        Try
            _path = k_path
            btnReopen.Enabled = True
            lblFile.Text = Path.GetFileName(k_path)
            lvAlerts.Items.Clear()
            _hostedMs = -1
            _readyMs = -1
            ApplyHostSettings()
            ShowAdobeInfo()
            lblStatus.Text = "Se deschide…"
            UpdateCounters()

            _clock = Stopwatch.StartNew()
            Dim k_result As AdobeHostResult = Await _host.ShowDocumentAsync(k_path).ConfigureAwait(True)
            If Not String.Equals(_path, k_path, StringComparison.Ordinal) Then Return

            Select Case k_result.Status
                Case AdobeHostStatus.Hosted
                    _hostedMs = CInt(_clock.ElapsedMilliseconds)
                    Write($"Fereastra Adobe a fost găzduită ({_hostedMs} ms de la clic; fereastra găsită în {k_result.ElapsedMs} ms, " &
                          $"{If(k_result.Match = AdobeCaptureMatch.ByPid, "după PID", "după titlu — instanță străină")}).")
                    ' DocumentReady may have come while this continuation waited.
                    If _host.IsDocumentReady AndAlso _readyMs < 0 Then _readyMs = CInt(_clock.ElapsedMilliseconds)
                    UpdateStatus()
                Case AdobeHostStatus.Superseded
                    ' A newer document took over: nothing to show.
                Case Else
                    lblStatus.Text = "Nu s-a deschis: " & k_result.Message
                    Write($"NU S-A DESCHIS ({k_result.Status}): {k_result.Message}")
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.OpenAsync", ex)
            Write("Deschiderea a eșuat: " & ex.Message)
        End Try
    End Sub

    ' Everything the application does before a document reaches the hosted window, so the bench opens
    ' it under the same conditions (same code as ReaderHostPreview.ApplySettings / ShowDocument).
    Private Sub ApplyHostSettings()
        Dim k_newInstance As AdobeSettingRead(Of AdobeNewInstanceMode) = AdobeViewerSettings.CurrentNewInstance()
        If k_newInstance.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & k_newInstance.Warning)
        _host.NewInstanceMode = k_newInstance.Value
        AdobeHostSettings.ApplyTo(_host, AddressOf AdobeHostLog.Write)
        _host.ReadModeEnabled = chkReadMode.Checked
        _host.SaveTrapEnabled = chkSaveTrap.Checked
        AcroPdfTraceLog.SwitchedOn = chkTrace.Checked
        AdobeHostLog.Write($"Setări gazdă Adobe (banc): instanță nouă={AdobeViewerSettings.NewInstanceLabel(k_newInstance.Value)}, " &
                           $"închidere={AdobeHostSettings.DetachModeLabel(_host.Options.DetachMode)}, " &
                           $"Ctrl+H/F8/Ctrl+2={If(chkReadMode.Checked, "da", "nu")}, capcana={If(chkSaveTrap.Checked, "da", "nu")}, " &
                           $"jurnal detaliat={If(chkTrace.Checked, "da", "nu")}.")

        AdobeUiPreference.EnsureApplied(AddressOf AdobeHostLog.Write)
        If Not _savePrefDone Then
            _savePrefDone = True
            AdobeHostLog.Write(AdobePrefs.EnsureStandardSaveDialog())
        End If
    End Sub

    Private Sub ReleaseDocument()
        _host.Detach()
        _path = Nothing
        btnReopen.Enabled = False
        lblFile.Text = "Niciun document"
        lblStatus.Text = ""
    End Sub

    ' ── Host events (UI thread; UI boundaries: log and swallow) ──────────────────
    Private Sub OnScriptAlert(k_alert As AdobeScriptAlert)
        Try
            Dim k_item As New ListViewItem(k_alert.Time.ToString("HH:mm:ss.fff"))
            k_item.SubItems.Add(k_alert.KindLabel)
            k_item.SubItems.Add(k_alert.ActionLabel)
            k_item.SubItems.Add(k_alert.Text)
            k_item.ForeColor = AlertColor(k_alert)
            lvAlerts.Items.Add(k_item)
            k_item.EnsureVisible()
            UpdateCounters()
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.OnScriptAlert", ex)
        End Try
    End Sub

    ' An error that would not close is the one to look at; the errors K-BOT closed are the noise this
    ' bench counts; the form's own messages and the console are plain.
    Private Function AlertColor(k_alert As AdobeScriptAlert) As Color
        Dim k_palette As ThemePalette = ThemeManager.Current?.Palette
        If k_palette Is Nothing Then Return lvAlerts.ForeColor
        If k_alert.Action = AdobeScriptAlertAction.Stuck Then Return k_palette.ErrorColor
        If k_alert.IsError Then Return k_palette.WarningColor
        If k_alert.IsConsole Then Return k_palette.TextDimColor
        Return lvAlerts.ForeColor
    End Function

    Private Sub OnDocumentReady()
        Try
            If _clock IsNot Nothing Then _readyMs = CInt(_clock.ElapsedMilliseconds)
            UpdateStatus()
            UpdateCounters()
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.OnDocumentReady", ex)
        End Try
    End Sub

    Private Sub OnHostedWindowClosed()
        Try
            lblStatus.Text = "Fereastra Adobe a fost închisă din Adobe."
            Write("Fereastra Adobe a fost închisă din Adobe (nu din bancul K-BOT).")
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.OnHostedWindowClosed", ex)
        End Try
    End Sub

    Private Sub OnDocumentSaved(k_path As String)
        Write("SALVAT de capcană (dialogul s-a închis): " & k_path)
    End Sub

    Private Sub OnSaveTrapFailed(k_reason As String)
        Write("SALVARE ANULATĂ de capcană: " & k_reason)
    End Sub

    ' Raised on the writer's thread (the trap and the host write from the UI thread, but not every
    ' writer does). Never throws: AdobeHostLog swallows a failing listener anyway.
    Private Sub OnAdobeLogLine(k_line As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf AppendLine), "[Adobe] " & k_line)
        Else
            AppendLine("[Adobe] " & k_line)
        End If
    End Sub

    ' ── Display helpers ─────────────────────────────────────────────────────────
    Private Sub ShowAdobeInfo()
        Dim k_info As AdobeProductInfo = AdobeProductInfo.Read(AdobeReaderHost.ResolveAdobePath())
        lblAdobe.Text = "Adobe: " & k_info.Describe()
    End Sub

    Private Sub UpdateStatus()
        If _hostedMs < 0 Then Return
        lblStatus.Text = $"Găzduit după {_hostedMs} ms; " &
                         If(_readyMs >= 0, $"document gata după {_readyMs} ms.", "se așteaptă ca Adobe să termine…")
    End Sub

    Private Sub UpdateCounters()
        lblCounters.Text = "Mesaje de script Adobe: " & _host.ScriptMonitor.Summary() & "."
    End Sub

    ' ── Report ──────────────────────────────────────────────────────────────────
    ' Everything needed to understand a run from a pasted message: the Adobe, the switches, the
    ' timings, the tally, each script window, and the tail of the log.
    Private Function BuildReport() As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine($"Banc fereastră găzduită — Adobe vechi — {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        k_sb.AppendLine(lblAdobe.Text)
        k_sb.AppendLine("Fișier: " & If(_path, "(niciunul)"))
        k_sb.AppendLine($"Ctrl+H/F8/Ctrl+2={If(chkReadMode.Checked, "da", "nu")}; capcana={If(chkSaveTrap.Checked, "da", "nu")}; " &
                        $"jurnal detaliat={If(chkTrace.Checked, "da", "nu")}")
        k_sb.AppendLine($"Găzduit după: {If(_hostedMs >= 0, _hostedMs & " ms", "—")}; document gata după: {If(_readyMs >= 0, _readyMs & " ms", "—")}")
        k_sb.AppendLine(lblCounters.Text)
        For Each k_alert As AdobeScriptAlert In _host.ScriptMonitor.Alerts
            k_sb.AppendLine($"  {k_alert.Time:HH:mm:ss.fff}  {k_alert.Describe()}")
        Next
        k_sb.AppendLine()
        k_sb.AppendLine($"── Jurnalul (ultimele {ReportLogLines} linii) ──")
        Dim k_lines As String() = txtLog.Lines
        For i As Integer = Math.Max(0, k_lines.Length - ReportLogLines) To k_lines.Length - 1
            k_sb.AppendLine(k_lines(i))
        Next
        Return k_sb.ToString()
    End Function

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    ' Always on a copy: the Save As trap may overwrite the hosted file.
    Private Shared Function BenchPath(k_source As String) As String
        Dim k_folder As String = Path.Combine(TempPdfStore.EnsureRoot(), BenchFolder)
        Directory.CreateDirectory(k_folder)
        Return Path.Combine(k_folder, "ADOBE_VECHI_" & Path.GetFileName(k_source))
    End Function

    Private Sub Write(k_line As String)
        AppendLine(k_line)
        _log?.Invoke(k_line)
    End Sub

    Private Sub AppendLine(k_line As String)
        If IsDisposed Then Return
        txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") & "  " & k_line & Environment.NewLine)
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub ShutDownBench()
        Try
            If _logHooked Then
                RemoveHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
                _logHooked = False
            End If
            ' Lets go of the hosted window and, per the operator's detach setting, ends the Adobe K-BOT started.
            _host?.Dispose()
            AcroPdfTraceLog.SwitchedOn = _traceBefore
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessForm.ShutDownBench", ex)
        End Try
    End Sub

End Class
#End If
