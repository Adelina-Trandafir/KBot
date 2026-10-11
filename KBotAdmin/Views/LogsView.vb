Option Strict On
Imports System.Globalization

''' <summary>
''' The server logs for the chosen database (slice ME01), in the coloured log box: time in the accent colour,
''' errors dark red, warnings in the warning colour, a traceback hanging under its line. The server reads and
''' parses its own files (admin_cli logs); the level filter and the search text work on what came back.
''' </summary>
Public Class LogsView
    Implements IAdminView

    Private ReadOnly _levelCaptions As String() = {"Toate nivelurile", "Avertismente și erori", "Doar erori"}
    Private ReadOnly _gensCaptions As String() = {"Doar fișierul curent", "+ ultima copie", "+ 3 copii", "+ toate copiile"}
    Private ReadOnly _gensValues As Integer() = {0, 1, 3, 5}

    Private _context As AdminContext
    Private _files As New List(Of LogFileInfo)()
    Private _entries As New List(Of ServerLogEntry)()
    Private _loading As Boolean
    Private _metaLoaded As Boolean
    Private _fillingCombos As Boolean

    Public Sub New()
        InitializeComponent()
        _fillingCombos = True
        For Each k_caption As String In _levelCaptions
            cmbLevel.Items.Add(k_caption)
        Next
        cmbLevel.SelectedIndex = 0
        For Each k_caption As String In _gensCaptions
            cmbGens.Items.Add(k_caption)
        Next
        cmbGens.SelectedIndex = 1
        cmbFile.Items.Add("Toate fișierele")
        cmbFile.SelectedIndex = 0
        _fillingCombos = False
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await EnsureMetaAsync()
        Await LoadAsync(True)
        tmrFollow.Enabled = chkFollow.Checked
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        BeginLoad()
    End Sub

    Private Async Sub BeginLoad()
        Try
            Await LoadAsync(True)
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.BeginLoad", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Loading
    ' ------------------------------------------------------------------

    Private Async Function EnsureMetaAsync() As Task
        If _metaLoaded Then Return
        Await UiRun.Guard("LogsView.EnsureMetaAsync", _context,
            Async Function()
                Dim k_meta As LogsMeta = Await _context.Client.CallAsync(Of LogsMeta)("logs-meta")
                _files = k_meta.Files
                _fillingCombos = True
                Try
                    cmbFile.Items.Clear()
                    cmbFile.Items.Add("Toate fișierele")
                    For Each k_file As LogFileInfo In _files
                        cmbFile.Items.Add(k_file.Label & If(k_file.Exists, String.Empty, " (lipsește)"))
                    Next
                    cmbFile.SelectedIndex = 0
                Finally
                    _fillingCombos = False
                End Try
                _metaLoaded = True
            End Function)
    End Function

    ''' <summary>Reads the entries from the server. <paramref name="k_loud"/> False (the follow timer): no message boxes.</summary>
    Private Async Function LoadAsync(k_loud As Boolean) As Task
        If _loading Then Return
        _loading = True
        Try
            Dim k_args As New List(Of String) From {
                "--gens", _gensValues(Math.Max(0, cmbGens.SelectedIndex)).ToString(CultureInfo.InvariantCulture),
                "--limit", "2500"
            }
            If cmbFile.SelectedIndex > 0 AndAlso cmbFile.SelectedIndex - 1 < _files.Count Then k_args.AddRange({"--file", _files(cmbFile.SelectedIndex - 1).Key})
            If Not _context.IsAll Then k_args.AddRange({"--db", _context.SelectedDb})
            Dim k_work As Func(Of Task) =
                Async Function()
                    _context.Say("Se citesc logurile...")
                    Dim k_result As LogsResult = Await _context.Client.CallAsync(Of LogsResult)("logs", k_args.ToArray())
                    _entries = k_result.Rows
                    Render(k_result)
                    _context.Say("Gata.")
                End Function
            If k_loud Then
                Await UiRun.Guard("LogsView.LoadAsync", _context, k_work)
            Else
                Try
                    Await k_work().ConfigureAwait(True)
                Catch ex As Exception
                    ' The follow timer must not bury the operator in boxes: stop following and say why.
                    GlobalErrorLog.Write("LogsView.LoadAsync(follow)", ex)
                    chkFollow.Checked = False
                    _context.Say("Urmărirea s-a oprit: " & ex.Message)
                End Try
            End If
        Finally
            _loading = False
        End Try
    End Function

    ' ------------------------------------------------------------------
    ' Showing
    ' ------------------------------------------------------------------

    Private Sub Render(k_result As LogsResult)
        Dim k_minimum As Integer = cmbLevel.SelectedIndex
        Dim k_text As String = txtSearch.Text.Trim()
        Dim k_lines As New List(Of KBotLogLine)()
        For Each k_entry As ServerLogEntry In _entries
            Dim k_level As KBotLogLevel = LevelOf(k_entry.Level)
            If k_minimum = 1 AndAlso k_level < KBotLogLevel.Warn Then Continue For
            If k_minimum = 2 AndAlso k_level < KBotLogLevel.Error Then Continue For
            If k_text <> String.Empty AndAlso Not Matches(k_entry, k_text) Then Continue For
            k_lines.Add(New KBotLogLine With {
                .Time = k_entry.Ts.Replace("T", " "),
                .Level = k_level,
                .Source = SourceOf(k_entry),
                .Text = k_entry.Msg,
                .Detail = k_entry.Detail
            })
        Next
        logBox.SetLines(k_lines, True)
        Dim k_errors As Integer = k_lines.Where(Function(k_l) k_l.Level = KBotLogLevel.Error).Count()
        Dim k_warnings As Integer = k_lines.Where(Function(k_l) k_l.Level = KBotLogLevel.Warn).Count()
        lblState.Text = k_lines.Count.ToString(CultureInfo.InvariantCulture) & " linii afișate (" & k_errors.ToString(CultureInfo.InvariantCulture) & " erori, " &
                        k_warnings.ToString(CultureInfo.InvariantCulture) & " avertismente)" &
                        If(k_result.Truncated, "; sunt " & k_result.Total.ToString(CultureInfo.InvariantCulture) & " în total, se arată cele mai noi", String.Empty) &
                        If(k_result.Unreadable.Count > 0, ". Nu s-au putut citi: " & String.Join(", ", k_result.Unreadable), ".") &
                        "  Baza: " & If(_context.IsAll, AdminContext.AllDatabasesCaption, _context.SelectedDb)
    End Sub

    Private Shared Function LevelOf(k_text As String) As KBotLogLevel
        Select Case If(k_text, String.Empty).ToUpperInvariant()
            Case "ERROR", "CRITICAL", "FATAL" : Return KBotLogLevel.Error
            Case "WARNING", "WARN" : Return KBotLogLevel.Warn
            Case "DEBUG" : Return KBotLogLevel.Debug
            Case Else : Return KBotLogLevel.Info
        End Select
    End Function

    Private Shared Function SourceOf(k_entry As ServerLogEntry) As String
        Dim k_parts As New List(Of String)()
        k_parts.Add(k_entry.File)
        If k_entry.User <> String.Empty Then k_parts.Add(k_entry.User)
        If k_entry.Db <> String.Empty Then k_parts.Add(k_entry.Db)
        Return "[" & String.Join(" | ", k_parts) & "]"
    End Function

    Private Shared Function Matches(k_entry As ServerLogEntry, k_text As String) As Boolean
        Return k_entry.Msg.Contains(k_text, StringComparison.OrdinalIgnoreCase) OrElse
               k_entry.User.Contains(k_text, StringComparison.OrdinalIgnoreCase) OrElse
               k_entry.Db.Contains(k_text, StringComparison.OrdinalIgnoreCase) OrElse
               k_entry.Detail.Contains(k_text, StringComparison.OrdinalIgnoreCase)
    End Function

    ' ------------------------------------------------------------------
    ' Handlers
    ' ------------------------------------------------------------------

    Private Sub Filter_Changed(k_sender As Object, e As EventArgs) Handles cmbLevel.SelectedIndexChanged
        Try
            If _fillingCombos OrElse _context Is Nothing OrElse _entries.Count = 0 Then Return
            Render(New LogsResult With {.Total = _entries.Count})
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.Filter_Changed", ex)
        End Try
    End Sub

    Private Async Sub Source_Changed(k_sender As Object, e As EventArgs) Handles cmbFile.SelectedIndexChanged, cmbGens.SelectedIndexChanged
        Try
            If _fillingCombos OrElse _context Is Nothing Then Return
            Await LoadAsync(True)
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.Source_Changed", ex)
        End Try
    End Sub

    Private Sub TxtSearch_TextChanged(k_sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        tmrSearch.Stop()
        tmrSearch.Start()
    End Sub

    Private Sub TmrSearch_Tick(k_sender As Object, e As EventArgs) Handles tmrSearch.Tick
        Try
            tmrSearch.Stop()
            If _context Is Nothing OrElse _entries.Count = 0 Then Return
            Render(New LogsResult With {.Total = _entries.Count})
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.TmrSearch_Tick", ex)
        End Try
    End Sub

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync(True)
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.BtnRefresh_Click", ex)
        End Try
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If logBox.LogLines.Count > 0 Then Clipboard.SetText(logBox.ToPlainText())
            _context.Say("Liniile afișate au fost copiate în clipboard.")
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.BtnCopy_Click", ex)
        End Try
    End Sub

    Private Sub ChkFollow_CheckedChanged(k_sender As Object, e As EventArgs) Handles chkFollow.CheckedChanged
        tmrFollow.Enabled = chkFollow.Checked AndAlso Visible
    End Sub

    Private Async Sub TmrFollow_Tick(k_sender As Object, e As EventArgs) Handles tmrFollow.Tick
        Try
            If Not Visible Then
                tmrFollow.Stop()
                Return
            End If
            Await LoadAsync(False)
        Catch ex As Exception
            GlobalErrorLog.Write("LogsView.TmrFollow_Tick", ex)
        End Try
    End Sub

End Class
