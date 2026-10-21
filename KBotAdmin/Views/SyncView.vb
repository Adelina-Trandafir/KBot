Option Strict On
Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Runs schema_sync on the server for the chosen database or all of them (slice ME01), the same command line
''' AvacontPush builds. Preview is the default; a real run asks, and a destructive one asks twice. The lines
''' arrive while the command runs and are shown in the log box, errors dark red.
''' </summary>
Public Class SyncView
    Implements IAdminView

    Private Shared ReadOnly ErrorWords As New Regex("\b(ERROR|CRITICAL|Traceback|E.EC)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly WarnWords As New Regex("\b(WARN|WARNING|DISTRUCTIV[AE]?|refuz\w*|MISSING)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    ' ME01-05: the server mentions a database the registry lists but the server does not have; it is not a target, so it is not shown.
    Private Shared ReadOnly MissingDbLine As New Regex("listeaz.+nu exist.+ignorat", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private _context As AdminContext
    Private ReadOnly _lines As New List(Of KBotLogLine)()
    Private ReadOnly _queue As New ConcurrentQueue(Of String)()
    Private _running As Boolean

    Public Sub New()
        InitializeComponent()
        cmbMode.Items.Add("SAFE - doar adăugări")
        cmbMode.Items.Add("FORCE - și modificări, ștergeri")
        cmbMode.SelectedIndex = 0
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowTarget()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowTarget()
    End Sub

    Private Sub ShowTarget()
        lblTarget.Text = "Țintă: " & If(_context.IsAll, "TOATE bazele de unitate (serverul le descoperă singur)", "baza " & _context.SelectedDb)
    End Sub

    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            If _running Then Return
            Dim k_viewOnly As Boolean = chkView.Checked
            Dim k_mode As String = If(cmbMode.SelectedIndex = 1, "FORCE", "SAFE")
            Dim k_targets As String = If(_context.IsAll, String.Empty, _context.SelectedDb)
            Dim k_targetText As String = If(_context.IsAll, "TOATE bazele", "baza " & _context.SelectedDb)

            If Not k_viewOnly Then
                If Not UiRun.Confirm("Execut sincronizarea de schemă (" & k_mode & ") pe " & k_targetText & "?" & vbLf &
                                     "Modificările se fac pe serverul real.", "Sincronizare") Then Return
                If _context.IsAll AndAlso Not UiRun.Confirm("Atenție: se execută pe TOATE bazele de unitate. Sigur?", "Sincronizare") Then Return
                If chkDestructive.Checked AndAlso Not UiRun.Confirm(
                    "Operațiile distructive sunt PERMISE: pot șterge sau modifica coloane și date." & vbLf &
                    "Serverul face întâi o copie a bazei, dar restaurarea e treaba dumneavoastră." & vbLf & vbLf & "Continuați?", "Sincronizare distructivă") Then Return
            End If

            _running = True
            btnRun.Enabled = False
            AddLine(KBotLogLevel.Info, "== " & If(k_viewOnly, "Previzualizare ", "Execuție ") & k_mode & " pe " & k_targetText & " ==")
            tmrFlush.Start()
            Dim k_command As String = _context.Client.SyncCommand(k_mode, k_targets, k_viewOnly, chkDestructive.Checked)
            Dim k_result As SshResult = Nothing
            Await UiRun.Guard("SyncView.BtnRun_Click", _context,
                Async Function()
                    _context.Say("Rulează sincronizarea pe " & k_targetText & "...")
                    k_result = Await _context.Client.StreamAsync(k_command, Sub(k_line) _queue.Enqueue(k_line))
                End Function)
            tmrFlush.Stop()
            FlushQueue()
            If k_result IsNot Nothing Then Conclude(k_result, k_viewOnly)
            _running = False
            btnRun.Enabled = True
            If k_result IsNot Nothing AndAlso Not k_viewOnly Then _context.RequestRefresh()
        Catch ex As Exception
            tmrFlush.Stop()
            _running = False
            btnRun.Enabled = True
            GlobalErrorLog.Write("SyncView.BtnRun_Click", ex)
        End Try
    End Sub

    Private Sub Conclude(k_result As SshResult, k_viewOnly As Boolean)
        Select Case k_result.ExitStatus
            Case 0
                AddLine(KBotLogLevel.Info, If(k_viewOnly, "Gata: planul e mai sus, nimic nu s-a executat.", "Gata: sincronizarea s-a încheiat fără erori."))
                lblState.Text = "Terminat cu succes."
            Case 2
                AddLine(KBotLogLevel.Warn, "Planul are operații distructive și NU a fost executat (nu ați permis distructivul).")
                lblState.Text = "Refuzat: plan distructiv fără permisiune."
            Case 3
                AddLine(KBotLogLevel.Warn, "Confirmarea distructivă nu a fost dată; nimic nu s-a executat.")
                lblState.Text = "Oprit la confirmare."
            Case Else
                AddLine(KBotLogLevel.Error, "Comanda s-a încheiat cu codul " & k_result.ExitStatus.ToString(CultureInfo.InvariantCulture) & ".")
                lblState.Text = "Terminat cu erori (cod " & k_result.ExitStatus.ToString(CultureInfo.InvariantCulture) & ")."
        End Select
        FlushRender()
        _context.Say(lblState.Text)
    End Sub

    ' ------------------------------------------------------------------
    ' Lines
    ' ------------------------------------------------------------------

    Private Sub TmrFlush_Tick(k_sender As Object, e As EventArgs) Handles tmrFlush.Tick
        Try
            FlushQueue()
        Catch ex As Exception
            GlobalErrorLog.Write("SyncView.TmrFlush_Tick", ex)
        End Try
    End Sub

    Private Sub FlushQueue()
        Dim k_any As Boolean = False
        Dim k_line As String = Nothing
        Do While _queue.TryDequeue(k_line)
            Dim k_text As String = k_line
            If k_text.StartsWith("[err] ", StringComparison.Ordinal) Then k_text = k_text.Substring(6)
            If k_text.Trim() = String.Empty OrElse MissingDbLine.IsMatch(k_text) Then Continue Do
            Dim k_level As KBotLogLevel = KBotLogLevel.Info
            If ErrorWords.IsMatch(k_text) Then
                k_level = KBotLogLevel.Error
            ElseIf WarnWords.IsMatch(k_text) Then
                k_level = KBotLogLevel.Warn
            End If
            _lines.Add(New KBotLogLine With {.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), .Level = k_level, .Text = k_text})
            k_any = True
        Loop
        If k_any Then FlushRender()
    End Sub

    Private Sub AddLine(k_level As KBotLogLevel, k_text As String)
        _lines.Add(New KBotLogLine With {.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), .Level = k_level, .Text = k_text})
        FlushRender()
    End Sub

    Private Sub FlushRender()
        logBox.SetLines(_lines, True)
    End Sub

    Private Sub BtnClear_Click(k_sender As Object, e As EventArgs) Handles btnClear.Click
        Try
            _lines.Clear()
            logBox.ClearLines()
            lblState.Text = String.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("SyncView.BtnClear_Click", ex)
        End Try
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If _lines.Count > 0 Then Clipboard.SetText(logBox.ToPlainText())
            _context.Say("Textul a fost copiat în clipboard.")
        Catch ex As Exception
            GlobalErrorLog.Write("SyncView.BtnCopy_Click", ex)
        End Try
    End Sub

End Class
