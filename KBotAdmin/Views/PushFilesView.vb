Option Strict On
Imports System.Globalization

''' <summary>
''' The file push of AvacontPush, inside KBotAdmin (slice ME01): scans the local PYTHON folder against the
''' server over SFTP, lists what is new or modified with ticks, sends the ticked files and can restart the
''' service. Beside the file list it shows, and lets the operator edit, the extensions that are accepted or
''' ignored and the ignore rules; they are saved at once in admin_settings.json and take effect on the next scan.
''' </summary>
Public Class PushFilesView
    Implements IAdminView

    Private _context As AdminContext
    Private ReadOnly _log As LogSink
    Private _scan As PushScan
    Private _filling As Boolean
    Private _running As Boolean

    Public Sub New()
        InitializeComponent()
        _log = New LogSink(logBox)
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowRoots()
        FillRules()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The push does not depend on the chosen database.
    End Sub

    Private ReadOnly Property Settings As AdminSettings
        Get
            Return _context.Client.Settings
        End Get
    End Property

    Private Sub ShowRoots()
        Dim k_rules As String = If(_scan Is Nothing, String.Empty, "   |   .pushignore: " & _scan.PushIgnoreRules.ToString(CultureInfo.InvariantCulture) & " reguli")
        lblRoot.Text = "Local: " & Settings.EffectiveLocalRoot() & "   ->   server: " & Settings.RemoteRoot & k_rules
    End Sub

    ' ------------------------------------------------------------------
    ' Scan
    ' ------------------------------------------------------------------

    Private Async Sub BtnScan_Click(k_sender As Object, e As EventArgs) Handles btnScan.Click
        If _running Then Return
        Try
            _running = True
            SetButtons(False)
            _log.Clear()
            Await ScanAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnScan_Click", ex)
            KBotMessage.Show("Scanarea a eșuat: " & ex.Message, "Publicare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _running = False
            SetButtons(True)
        End Try
    End Sub

    ''' <summary>Scans and refills the lists. The caller owns the busy flag and the log.</summary>
    Private Async Function ScanAsync() As Task
        _log.Add(KBotLogLevel.Info, "== Scanare " & Settings.EffectiveLocalRoot() & " ==")
        Dim k_settings As AdminSettings = Settings
        Dim k_result As PushScan = Nothing
        Await UiRun.Guard("PushFilesView.ScanAsync", _context,
            Async Function()
                _context.Say("Se scanează fișierele...")
                k_result = Await Task.Run(Function() PushService.Scan(k_settings, Sub(k_line) _log.Add(KBotLogLevel.Info, k_line)))
            End Function)
        If k_result Is Nothing Then Return
        _scan = k_result
        FillFiles()
        FillRules()
        ShowRoots()
        Dim k_send As Integer = _scan.Files.Where(Function(k_f) k_f.State <> PushState.Identical).Count()
        lblState.Text = _scan.Files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere permise, " &
                        k_send.ToString(CultureInfo.InvariantCulture) & " noi sau modificate; " &
                        _scan.PushIgnoreSkipped.ToString(CultureInfo.InvariantCulture) & " oprite de .pushignore."
        _log.Add(KBotLogLevel.Info, lblState.Text)
        _context.Say(lblState.Text)
    End Function

    Private Sub SetButtons(k_on As Boolean)
        btnScan.Enabled = k_on
        btnSend.Enabled = k_on
    End Sub

    Private Sub FillFiles()
        _filling = True
        gridFiles.BeginUpdate()
        Try
            gridFiles.ClearRows()
            If _scan Is Nothing Then Return
            For Each k_file As PushFile In _scan.Files
                If k_file.State = PushState.Identical AndAlso Not chkIdentical.Checked Then Continue For
                Dim k_row As KBotDataRow = gridFiles.AddRow()
                k_row.Tag = k_file
                k_row("send") = (k_file.State <> PushState.Identical)
                k_row("path") = k_file.RelativePath
                k_row("state") = StateText(k_file.State)
                k_row("ext") = k_file.Extension
                k_row("size") = UiRun.FormatBytes(k_file.LocalSize)
                k_row("srvsize") = If(k_file.State = PushState.NewOnServer, String.Empty, UiRun.FormatBytes(k_file.RemoteSize))
                k_row("local") = k_file.LocalMtimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                k_row("server") = If(k_file.RemoteMtimeUtc = DateTime.MinValue, String.Empty,
                                     k_file.RemoteMtimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            Next
        Finally
            gridFiles.EndUpdate()
            _filling = False
        End Try
    End Sub

    Private Shared Function StateText(k_state As PushState) As String
        Select Case k_state
            Case PushState.NewOnServer : Return "NOU"
            Case PushState.Modified : Return "MODIFICAT"
            Case Else : Return "IDENTIC"
        End Select
    End Function

    Private Sub GridFiles_RowFormatting(k_sender As Object, e As KBotRowFormattingEventArgs) Handles gridFiles.RowFormatting
        Try
            Dim k_file As PushFile = TryCast(e.Row.Tag, PushFile)
            If k_file Is Nothing Then Return
            Dim k_pal As ThemePalette = ThemeManager.Current.Palette
            Select Case k_file.State
                Case PushState.NewOnServer : e.ForeColor = k_pal.SuccessColor
                Case PushState.Modified : e.ForeColor = k_pal.WarningColor
                Case Else : e.ForeColor = k_pal.DisabledTextColor
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.GridFiles_RowFormatting", ex)
        End Try
    End Sub

    Private Sub ChkIdentical_CheckedChanged(k_sender As Object, e As EventArgs) Handles chkIdentical.CheckedChanged
        Try
            FillFiles()
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.ChkIdentical_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub BtnAll_Click(k_sender As Object, e As EventArgs) Handles btnAll.Click
        SetAllTicks(True)
    End Sub

    Private Sub BtnNone_Click(k_sender As Object, e As EventArgs) Handles btnNone.Click
        SetAllTicks(False)
    End Sub

    Private Sub SetAllTicks(k_value As Boolean)
        Try
            _filling = True
            For k_i As Integer = 0 To gridFiles.Rows.Count - 1
                gridFiles("send", k_i) = k_value
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.SetAllTicks", ex)
        Finally
            _filling = False
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Send
    ' ------------------------------------------------------------------

    Private Async Sub BtnSend_Click(k_sender As Object, e As EventArgs) Handles btnSend.Click
        RunLog.Write("Trimite: clic (running=" & _running.ToString() & ")")
        If _running Then
            _context.Say("O operație rulează încă; așteptați să se termine.")
            Return
        End If
        Try
            Dim k_files As New List(Of PushFile)()
            For k_i As Integer = 0 To gridFiles.Rows.Count - 1
                Dim k_file As PushFile = TryCast(gridFiles.Rows(k_i).Tag, PushFile)
                If k_file IsNot Nothing AndAlso Convert.ToBoolean(gridFiles("send", k_i), CultureInfo.InvariantCulture) Then k_files.Add(k_file)
            Next
            RunLog.Write("Trimite: " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere bifate din " & gridFiles.Rows.Count.ToString(CultureInfo.InvariantCulture) & " rânduri")
            If k_files.Count = 0 Then
                KBotMessage.Show("Nu este bifat niciun fișier pentru trimitere. Scanați întâi, apoi bifați fișierele.", "Publicare", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_restart As Boolean = chkRestart.Checked
            Dim k_yes As Boolean = UiRun.Confirm("Trimit " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere pe server" &
                                                 If(k_restart, " și repornesc serviciul avacont", String.Empty) & "?", "Publicare")
            RunLog.Write("Trimite: răspuns la confirmare = " & If(k_yes, "Da", "Nu"))
            If Not k_yes Then
                _context.Say("Trimiterea a fost anulată (ați răspuns Nu).")
                Return
            End If

            RunLog.Write("Trimite: confirmat, pornește încărcarea")
            _running = True
            SetButtons(False)
            _log.Clear()
            _log.Add(KBotLogLevel.Info, "== Trimitere " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere ==")
            _context.Say("Se trimit " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere...")
            Dim k_settings As AdminSettings = Settings
            Dim k_failed As Integer = -1
            Await UiRun.Guard("PushFilesView.BtnSend_Click", _context,
                Async Function()
                    k_failed = Await Task.Run(Function() PushService.Upload(k_settings, k_files,
                        Sub(k_done, k_file, k_error)
                            If k_error = String.Empty Then
                                _log.Add(KBotLogLevel.Info, "OK      " & k_file.RelativePath)
                            Else
                                _log.Add(KBotLogLevel.Error, "EROARE  " & k_file.RelativePath & " - " & k_error)
                            End If
                        End Sub))
                    If k_failed = 0 AndAlso k_restart Then
                        _log.Add(KBotLogLevel.Info, "== Repornire serviciu avacont ==")
                        Await RunRemote("systemctl daemon-reload")
                        Await RunRemote("systemctl restart avacont")
                        Await RunRemote("journalctl -u avacont -n 30 --no-pager")
                    ElseIf k_failed > 0 AndAlso k_restart Then
                        _log.Add(KBotLogLevel.Warn, "Repornirea nu s-a făcut: unele fișiere nu au ajuns.")
                    End If
                End Function)
            RunLog.Write("Trimite: încărcare terminată, erori=" & k_failed.ToString(CultureInfo.InvariantCulture))
            If k_failed < 0 Then Return   ' Guard already told the operator what went wrong

            Dim k_summary As String = If(k_failed = 0,
                "Trimise " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere, fără erori.",
                "Trimitere terminată cu " & k_failed.ToString(CultureInfo.InvariantCulture) & " erori din " & k_files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere.")
            _log.Add(If(k_failed = 0, KBotLogLevel.Info, KBotLogLevel.Error), k_summary)
            _context.Say(k_summary)
            ' Rescan at once: the list now shows what the server really holds (sent files disappear from it).
            Dim k_before As Integer = k_files.Count
            Await ScanAsync()
            RunLog.Write("Trimite: rescanare gata")
            Dim k_left As Integer = If(_scan Is Nothing, 0, _scan.Files.Where(Function(k_f) k_f.State <> PushState.Identical).Count())
            Dim k_after As String = If(k_left = 0, "Verificat pe server: toate fișierele sunt identice.",
                                       "Verificat pe server: au rămas " & k_left.ToString(CultureInfo.InvariantCulture) & " fișiere noi sau modificate.")
            _log.Add(If(k_left = 0, KBotLogLevel.Info, KBotLogLevel.Warn), k_after)
            lblState.Text = k_summary & " " & k_after
            KBotMessage.Show(k_summary & vbLf & k_after, "Publicare", MessageBoxButtons.OK, If(k_failed = 0 AndAlso k_left = 0, MessageBoxIcon.Information, MessageBoxIcon.Warning))
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnSend_Click", ex)
            KBotMessage.Show("Trimiterea a eșuat: " & ex.Message, "Publicare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _running = False
            SetButtons(True)
        End Try
    End Sub

    Private Async Function RunRemote(k_command As String) As Task
        _log.Add(KBotLogLevel.Info, "$ " & k_command)
        RunLog.Write("Remote: " & k_command)
        Dim k_result As SshResult = Await _context.Client.StreamAsync(k_command, Sub(k_line) _log.Add(k_line))
        If k_result.ExitStatus <> 0 Then
            _log.Add(KBotLogLevel.Warn, "AVERTISMENT: comanda a returnat codul " & k_result.ExitStatus.ToString(CultureInfo.InvariantCulture) & ".")
        End If
    End Function

    ' ------------------------------------------------------------------
    ' Extensions and ignore rules (edited here, saved at once)
    ' ------------------------------------------------------------------

    Private Sub FillRules()
        _filling = True
        gridExt.BeginUpdate()
        gridPat.BeginUpdate()
        Try
            gridExt.ClearRows()
            gridPat.ClearRows()
            Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            If _scan IsNot Nothing Then
                For Each k_stat As PushExtensionStat In _scan.Extensions
                    k_seen.Add(k_stat.Extension)
                    AddExtRow(k_stat.Extension, k_stat.Accepted, k_stat.Found.ToString(CultureInfo.InvariantCulture),
                              If(k_stat.Accepted, k_stat.ToSend.ToString(CultureInfo.InvariantCulture), String.Empty))
                Next
            End If
            For Each k_ext As String In Settings.IncludeExtensions.OrderBy(Function(k_e) k_e, StringComparer.OrdinalIgnoreCase)
                Dim k_clean As String = NormalizeExt(k_ext)
                If k_clean <> String.Empty AndAlso k_seen.Add(k_clean) Then AddExtRow(k_clean, True, String.Empty, String.Empty)
            Next
            Dim k_hits As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            If _scan IsNot Nothing Then
                For Each k_rule As PushRuleStat In _scan.Rules
                    k_hits(k_rule.Pattern) = k_rule.Skipped
                Next
            End If
            For Each k_pattern As String In Settings.IgnorePatterns
                If String.IsNullOrWhiteSpace(k_pattern) Then Continue For
                Dim k_row As KBotDataRow = gridPat.AddRow()
                k_row("pattern") = k_pattern.Trim()
                k_row("skipped") = If(_scan Is Nothing, String.Empty, k_hits.GetValueOrDefault(k_pattern.Trim()).ToString(CultureInfo.InvariantCulture))
            Next
        Finally
            gridExt.EndUpdate()
            gridPat.EndUpdate()
            _filling = False
        End Try
    End Sub

    Private Sub AddExtRow(k_ext As String, k_accepted As Boolean, k_found As String, k_toSend As String)
        Dim k_row As KBotDataRow = gridExt.AddRow()
        k_row("ext") = k_ext
        k_row("accepted") = k_accepted
        k_row("found") = k_found
        k_row("tosend") = k_toSend
    End Sub

    Private Shared Function NormalizeExt(k_text As String) As String
        Return If(k_text, String.Empty).Trim().TrimStart("."c).ToLowerInvariant()
    End Function

    ''' <summary>Writes what the two rule grids show back into the settings and saves them.</summary>
    Private Sub SaveRules()
        Try
            Dim k_ext As New List(Of String)()
            For k_i As Integer = 0 To gridExt.Rows.Count - 1
                Dim k_name As String = NormalizeExt(Convert.ToString(gridExt("ext", k_i), CultureInfo.InvariantCulture))
                If k_name <> String.Empty AndAlso Convert.ToBoolean(gridExt("accepted", k_i), CultureInfo.InvariantCulture) AndAlso Not k_ext.Contains(k_name) Then
                    k_ext.Add(k_name)
                End If
            Next
            Dim k_patterns As New List(Of String)()
            For k_i As Integer = 0 To gridPat.Rows.Count - 1
                Dim k_pattern As String = Convert.ToString(gridPat("pattern", k_i), CultureInfo.InvariantCulture).Trim()
                If k_pattern <> String.Empty AndAlso Not k_patterns.Contains(k_pattern, StringComparer.OrdinalIgnoreCase) Then k_patterns.Add(k_pattern)
            Next
            Settings.IncludeExtensions = k_ext
            Settings.IgnorePatterns = k_patterns
            AdminSettingsStore.Save(Settings)
            lblState.Text = "Regulile s-au salvat (" & k_ext.Count.ToString(CultureInfo.InvariantCulture) & " extensii acceptate, " &
                            k_patterns.Count.ToString(CultureInfo.InvariantCulture) & " reguli de ignorare). Scanați din nou ca să se aplice."
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.SaveRules", ex)
            KBotMessage.Show("Regulile nu au putut fi salvate: " & ex.Message, "Publicare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub GridExt_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles gridExt.CellValueChanged
        If Not _filling Then SaveRules()
    End Sub

    Private Sub GridPat_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles gridPat.CellValueChanged
        If Not _filling Then SaveRules()
    End Sub

    Private Sub BtnExtAdd_Click(k_sender As Object, e As EventArgs) Handles btnExtAdd.Click
        Try
            AddExtRow(String.Empty, True, String.Empty, String.Empty)
            gridExt.CurrentRowIndex = gridExt.Rows.Count - 1
            lblState.Text = "Rând nou: scrieți extensia (fără punct) în coloana «Extensie»."
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnExtAdd_Click", ex)
        End Try
    End Sub

    Private Sub BtnExtDel_Click(k_sender As Object, e As EventArgs) Handles btnExtDel.Click
        Try
            Dim k_index As Integer = gridExt.CurrentRowIndex
            If k_index < 0 OrElse k_index >= gridExt.Rows.Count Then Return
            gridExt.RemoveRowAt(k_index)
            SaveRules()
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnExtDel_Click", ex)
        End Try
    End Sub

    Private Sub BtnPatAdd_Click(k_sender As Object, e As EventArgs) Handles btnPatAdd.Click
        Try
            Dim k_row As KBotDataRow = gridPat.AddRow()
            k_row("pattern") = String.Empty
            k_row("skipped") = String.Empty
            gridPat.CurrentRowIndex = gridPat.Rows.Count - 1
            lblState.Text = "Rând nou: scrieți numele dosarului sau al fișierului, ori *.extensie, în coloana «Regulă de ignorare»."
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnPatAdd_Click", ex)
        End Try
    End Sub

    Private Sub BtnPatDel_Click(k_sender As Object, e As EventArgs) Handles btnPatDel.Click
        Try
            Dim k_index As Integer = gridPat.CurrentRowIndex
            If k_index < 0 OrElse k_index >= gridPat.Rows.Count Then Return
            gridPat.RemoveRowAt(k_index)
            SaveRules()
        Catch ex As Exception
            GlobalErrorLog.Write("PushFilesView.BtnPatDel_Click", ex)
        End Try
    End Sub

End Class
