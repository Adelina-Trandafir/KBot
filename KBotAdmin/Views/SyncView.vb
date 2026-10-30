Option Strict On
Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Runs schema_sync on the server for the chosen database or all of them (slice ME01), the same command line
''' AvacontPush builds. «Verifică diferențe» is a preview (asks once, default «Da»); «Sincronizează» asks once, calculates the plan with a preview, asks again when it is destructive (both default «Nu»), then runs once. The lines
''' arrive while the command runs and are shown in the log box, errors dark red.
''' </summary>
Public Class SyncView
    Implements IAdminView

    Private Shared ReadOnly ErrorWords As New Regex("\b(ERROR|CRITICAL|Traceback|E.EC)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly WarnWords As New Regex("\b(WARN|WARNING|DISTRUCTIV[AE]?|refuz\w*|MISSING)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    ' ME01-05: the server mentions a database the registry lists but the server does not have; it is not a target, so it is not shown.
    Friend Shared ReadOnly MissingDbLine As New Regex("listeaz.+nu exist.+ignorat", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    ' The preview's summary: «<n> DISTRUCTIVE, pe: ...» when the plan deletes or changes data, «Instructiunile scrise in: <file>» when there is any plan at all.
    Friend Shared ReadOnly DestructiveLine As New Regex("(\d+) DISTRUCTIVE", RegexOptions.CultureInvariant)
    Friend Shared ReadOnly PlanWrittenLine As New Regex("Instruc.iunile scrise", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private _context As AdminContext
    Private ReadOnly _lines As New List(Of KBotLogLine)()
    Private ReadOnly _queue As New ConcurrentQueue(Of String)()
    Private _running As Boolean
    Private _planDestructive As Integer
    Private _planHasRows As Boolean

    ' The plan run is silent: its lines wait here and reach the log only when the run stops there (error, nothing to do, destructive refused).
    Private ReadOnly _held As New ConcurrentQueue(Of String)()
    Private _holdLines As Boolean

    Public Sub New()
        InitializeComponent()
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

    ''' <summary>«Verifică diferențe»: the full plan (FORCE, preview), nothing is executed.</summary>
    Private Async Sub BtnCheck_Click(k_sender As Object, e As EventArgs) Handles btnCheck.Click
        Try
            If _running Then Return
            If Not UiRun.Confirm("Verific diferențele de schemă pe " & TargetText() & "?" & vbLf &
                                 "Nu se modifică nimic pe server.", "Verifică diferențe", True) Then Return
            Await RunSyncAsync(True, False)
        Catch ex As Exception
            GlobalErrorLog.Write("SyncView.BtnCheck_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' «Sincronizează»: asks first, then calculates the plan with a preview (nothing executed) and reads from it
    ''' whether deletions or data changes follow. Only then the second question is asked, and the real run happens
    ''' once, with or without the destructive permission, so the server never has to refuse a run halfway.
    ''' </summary>
    Private Async Sub BtnRun_Click(k_sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            If _running Then Return
            If Not UiRun.Confirm("Execut sincronizarea de schemă pe " & TargetText() & "?" & vbLf &
                                 "Modificările se fac pe serverul real.", "Sincronizare") Then Return
            If _context.IsAll AndAlso Not UiRun.Confirm("Atenție: se execută pe TOATE bazele de unitate. Sigur?", "Sincronizare") Then Return

            Dim k_plan As SshResult = Await RunSyncAsync(True, False, True)
            If k_plan Is Nothing OrElse k_plan.ExitStatus <> 0 Then Return
            If Not _planHasRows Then
                ReleaseHeldLines()
                lblState.Text = "Nimic de sincronizat."
                _context.Say(lblState.Text)
                Return
            End If

            Dim k_destructive As Boolean = _planDestructive > 0
            If k_destructive AndAlso Not UiRun.Confirm(
                "Planul conține " & _planDestructive.ToString(CultureInfo.InvariantCulture) & " operații DISTRUCTIVE: pot șterge sau modifica coloane, indexuri și date." & vbLf &
                "Serverul face întâi o copie a bazei, dar restaurarea e treaba dumneavoastră." & vbLf & vbLf & "Continuați?",
                "Sincronizare distructivă") Then
                ReleaseHeldLines()
                AddLine(KBotLogLevel.Warn, "Oprit: operațiile distructive nu au fost permise; nu s-a executat nimic.")
                lblState.Text = "Oprit: operațiile distructive nu au fost permise."
                _context.Say(lblState.Text)
                Return
            End If
            _held.Clear()
            Await RunSyncAsync(False, k_destructive)
        Catch ex As Exception
            GlobalErrorLog.Write("SyncView.BtnRun_Click", ex)
        End Try
    End Sub

    ''' <summary>Moves the lines the silent plan run kept into the log.</summary>
    Private Sub ReleaseHeldLines()
        Dim k_line As String = Nothing
        Do While _held.TryDequeue(k_line)
            _queue.Enqueue(k_line)
        Loop
        FlushQueue()
    End Sub

    Private Function TargetText() As String
        Return If(_context.IsAll, "TOATE bazele", "baza " & _context.SelectedDb)
    End Function

    ''' <summary>Runs schema_sync (always FORCE) and shows the lines; Nothing when the command could not run.</summary>
    Private Async Function RunSyncAsync(k_viewOnly As Boolean, k_allowDestructive As Boolean, Optional k_planning As Boolean = False) As Task(Of SshResult)
        Dim k_result As SshResult = Nothing
        Try
            _running = True
            _planDestructive = 0
            _planHasRows = False
            SetButtons(False)
            _held.Clear()
            _holdLines = k_planning
            Dim k_targets As String = If(_context.IsAll, String.Empty, _context.SelectedDb)
            Dim k_targetText As String = TargetText()
            If k_planning Then
                lblState.Text = "Calculez planul (nu se execută nimic)..."
            Else
                AddLine(KBotLogLevel.Info, "== " & If(k_viewOnly, "Verificare diferențe", "Execuție") & If(k_allowDestructive, " (cu operații distructive)", String.Empty) & " pe " & k_targetText & " ==")
            End If
            tmrFlush.Start()
            Dim k_command As String = _context.Client.SyncCommand("FORCE", k_targets, k_viewOnly, k_allowDestructive)
            Await UiRun.Guard("SyncView.RunSyncAsync", _context,
                Async Function()
                    _context.Say("Rulează sincronizarea pe " & k_targetText & "...")
                    k_result = Await _context.Client.StreamAsync(k_command, Sub(k_line) ReadLine(k_line))
                End Function)
            tmrFlush.Stop()
            _holdLines = False
            If k_planning AndAlso (k_result Is Nothing OrElse k_result.ExitStatus <> 0) Then ReleaseHeldLines()
            FlushQueue()
            If k_result IsNot Nothing AndAlso Not (k_planning AndAlso k_result.ExitStatus = 0) Then Conclude(k_result, k_viewOnly)
            If k_result IsNot Nothing AndAlso Not k_viewOnly AndAlso k_result.ExitStatus <> 2 Then _context.RequestRefresh()
        Catch ex As Exception
            tmrFlush.Stop()
            GlobalErrorLog.Write("SyncView.RunSyncAsync", ex)
        Finally
            _running = False
            SetButtons(True)
        End Try
        Return k_result
    End Function

    ''' <summary>Runs on the SSH reader thread: queues the line for the log and notes what the plan summary says.</summary>
    Private Sub ReadLine(k_line As String)
        If _holdLines Then _held.Enqueue(k_line) Else _queue.Enqueue(k_line)
        Dim k_match As Match = DestructiveLine.Match(k_line)
        If k_match.Success Then _planDestructive = Integer.Parse(k_match.Groups(1).Value, CultureInfo.InvariantCulture)
        If PlanWrittenLine.IsMatch(k_line) Then _planHasRows = True
    End Sub

    Private Sub SetButtons(k_enabled As Boolean)
        btnCheck.Enabled = k_enabled
        btnRun.Enabled = k_enabled
    End Sub

    Private Sub Conclude(k_result As SshResult, k_viewOnly As Boolean)
        Select Case k_result.ExitStatus
            Case 0
                AddLine(KBotLogLevel.Info, If(k_viewOnly, "Gata: planul e mai sus, nimic nu s-a executat.", "Gata: sincronizarea s-a încheiat fără erori."))
                lblState.Text = "Terminat cu succes."
            Case 2
                AddLine(KBotLogLevel.Warn, "Planul are operații distructive și NU a fost executat (încă nu ați permis distructivul).")
                lblState.Text = "Plan distructiv: se așteaptă confirmarea."
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
