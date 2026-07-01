Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

Public NotInheritable Class DevHarnessForm

    Private ReadOnly _provider As IServiceProvider
    Private ReadOnly _progress As IProgress(Of HarnessProgressInfo)
    Private _allTests As List(Of IHarnessTest)
    Private _visibleTests As List(Of IHarnessTest)
    Private _cts As CancellationTokenSource
    Private _runLog As RunLogger

    ' Injectat din Program.vb (Debug). Deschide shell-ul real fără referință la KBot.App.
    Public Property OpenMainFormAction As Action

    ' Controalele sunt declarate în DevHarnessForm.Designer.vb (design-time).

    Public Sub New(provider As IServiceProvider)
        _provider = provider
        InitializeComponent()
        _progress = New Progress(Of HarnessProgressInfo)(AddressOf OnProgress)
        Try
            _allTests = HarnessTestDiscovery.Discover()
        Catch ex As Exception
            _allTests = New List(Of IHarnessTest)()
            GlobalErrorLog.Write("DevHarnessForm.ctor.Discover", ex)
            AppendLog("EROARE la descoperirea testelor (vezi harness_errors.log): " & ex.Message)
        End Try
        ApplyFilter()
    End Sub

    Private Sub txtFilter_TextChanged(sender As Object, e As EventArgs) Handles txtFilter.TextChanged
        ApplyFilter()
    End Sub

    Private Function DisplayName(t As IHarnessTest) As String
        Dim suffix As String = ""
        If t.IsDestructive Then
            suffix = " (DESTRUCTIVE)"
        ElseIf t.RequiresLiveConnection Then
            suffix = " (LIVE)"
        End If
        Return "[" & t.Category & "] " & t.Name & suffix
    End Function

    Private Sub ApplyFilter()
        Try
            Dim q As String = txtFilter.Text.Trim()
            If q.Length = 0 Then
                _visibleTests = _allTests.ToList()
            Else
                _visibleTests = _allTests.
                    Where(Function(t) DisplayName(t).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).
                    ToList()
            End If
            clbTests.BeginUpdate()
            clbTests.Items.Clear()
            For Each t As IHarnessTest In _visibleTests
                clbTests.Items.Add(DisplayName(t))
            Next
            clbTests.EndUpdate()
            lblStatus.Text = _visibleTests.Count.ToString() & " test(e) afișate."
        Catch ex As Exception
            HandleUiError("ApplyFilter", ex)
        End Try
    End Sub

    ' ---------- handlers (fiecare cu Try/Catch -> HandleUiError) ----------
    Private Async Sub btnRunChecked_Click(sender As Object, e As EventArgs) Handles btnRunChecked.Click
        Try
            Dim batch As New List(Of IHarnessTest)()
            For i As Integer = 0 To _visibleTests.Count - 1
                If clbTests.GetItemChecked(i) Then batch.Add(_visibleTests(i))
            Next
            If batch.Count = 0 Then
                AppendLog("Niciun test bifat.")
                Return
            End If
            Await RunTestsAsync(batch)
        Catch ex As Exception
            HandleUiError("btnRunChecked_Click", ex)
        End Try
    End Sub

    Private Async Sub btnRunSelected_Click(sender As Object, e As EventArgs) Handles btnRunSelected.Click
        Try
            Dim idx As Integer = clbTests.SelectedIndex
            If idx < 0 Then
                AppendLog("Niciun test selectat.")
                Return
            End If
            Await RunTestsAsync(New List(Of IHarnessTest) From {_visibleTests(idx)})
        Catch ex As Exception
            HandleUiError("btnRunSelected_Click", ex)
        End Try
    End Sub

    Private Async Sub btnRunAll_Click(sender As Object, e As EventArgs) Handles btnRunAll.Click
        Try
            Await RunTestsAsync(_visibleTests.ToList())
        Catch ex As Exception
            HandleUiError("btnRunAll_Click", ex)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Try
            If _cts IsNot Nothing Then _cts.Cancel()
        Catch ex As Exception
            HandleUiError("btnCancel_Click", ex)
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        Try
            rtbResults.Clear()
        Catch ex As Exception
            HandleUiError("btnClear_Click", ex)
        End Try
    End Sub

    Private Sub btnOpenMainForm_Click(sender As Object, e As EventArgs) Handles btnOpenMainForm.Click
        Try
            If OpenMainFormAction IsNot Nothing Then
                OpenMainFormAction.Invoke()
            Else
                AppendLog("OpenMainFormAction nu e cablat (vezi Program.vb).")
            End If
        Catch ex As Exception
            HandleUiError("btnOpenMainForm_Click", ex)
        End Try
    End Sub

    ' Tratează o eroare de UI: o scrie în harness_errors.log (+ runlog dacă rulează) și o afișează.
    Private Sub HandleUiError(source As String, ex As Exception)
        GlobalErrorLog.Write(source, ex)
        Try
            If _runLog IsNot Nothing Then _runLog.WriteLine("EROARE [" & source & "]: " & ex.ToString())
        Catch traceEx As Exception
            ' SINK TERMINAL: runlog indisponibil; eroarea principală e deja în harness_errors.log.
            Trace.WriteLine("HandleUiError: scrierea în runlog a eșuat pentru " & source & ": " & traceEx.Message)
        End Try
        MessageBox.Show(Me, ex.Message, "K-BOT — eroare (" & source & ")", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ' ---------- run loop ----------
    Private Async Function RunTestsAsync(tests As IList(Of IHarnessTest)) As Task
        ' --- precondiție OBLIGATORIE: fișierul de rezultate trebuie să se poată crea ---
        Dim logPath As String = Path.Combine(AppContext.BaseDirectory, "Logs",
                                             "test_" & DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") & ".log")
        Try
            _runLog = New RunLogger(logPath)
        Catch ex As Exception
            GlobalErrorLog.Write("RunTestsAsync.OpenLog", ex)
            MessageBox.Show(Me,
                "Nu pot crea fișierul de rezultate:" & Environment.NewLine & logPath & Environment.NewLine & Environment.NewLine &
                ex.Message & Environment.NewLine & Environment.NewLine &
                "Rularea NU pornește (rezultatele trebuie scrise în fișier). Verifică drepturile de scriere pe directorul de instalare.",
                "K-BOT — fișier de rezultate", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        _cts = New CancellationTokenSource()
        SetRunningState(True)
        pbProgress.Minimum = 0
        pbProgress.Maximum = tests.Count
        pbProgress.Value = 0

        Dim nPassed As Integer = 0
        Dim nFailed As Integer = 0
        Dim nSkipped As Integer = 0
        Dim nError As Integer = 0

        Dim ctx As New HarnessContext(_provider, AddressOf AppendLog, _progress)
        Try
            AppendLog("=== K-BOT Dev Harness — rulare teste ===")
            AppendLog("Data      : " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            AppendLog("Mașină    : " & Environment.MachineName & "   Utilizator: " & Environment.UserName)
            AppendLog("AppDir    : " & AppContext.BaseDirectory)
            AppendLog("Fișier log: " & _runLog.FilePath)
            AppendLog("Teste     : " & tests.Count.ToString())
            AppendLog("")

            For Each test As IHarnessTest In tests
                If _cts.IsCancellationRequested Then
                    AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "run cancelled", 0)
                    nSkipped += 1
                    Continue For
                End If

                If test.RequiresLiveConnection OrElse test.IsDestructive Then
                    Dim decision As DialogResult = ConfirmLiveOrDestructive(test)
                    If decision = DialogResult.Cancel Then
                        _cts.Cancel()
                        AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "run cancelled", 0)
                        nSkipped += 1
                        Continue For
                    ElseIf decision = DialogResult.No Then
                        AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "skipped by user", 0)
                        nSkipped += 1
                        pbProgress.Value += 1
                        Continue For
                    End If
                End If

                AppendLog("── RUN [" & test.Category & "] " & test.Name)
                Dim sw As Stopwatch = Stopwatch.StartNew()
                Dim res As HarnessTestResult
                Try
                    res = Await test.RunAsync(ctx, _cts.Token)
                    If res Is Nothing Then res = HarnessTestResult.Failed("testul a întors Nothing (contract încălcat)")
                Catch ocex As OperationCanceledException
                    res = HarnessTestResult.Skipped("cancelled")
                Catch ex As Exception
                    ' Surfacing complet (ex.ToString() = mesaj + stack + inner), NU înghițire.
                    res = HarnessTestResult.Errored(ex)
                End Try
                sw.Stop()

                AppendVerdict(test.Name, res.Outcome, res.Message, sw.ElapsedMilliseconds)
                If Not String.IsNullOrEmpty(res.Details) Then AppendLog(res.Details)

                Select Case res.Outcome
                    Case HarnessTestOutcome.Passed : nPassed += 1
                    Case HarnessTestOutcome.Failed : nFailed += 1
                    Case HarnessTestOutcome.[Error] : nError += 1
                    Case Else : nSkipped += 1
                End Select
                pbProgress.Value += 1
            Next

            AppendLog("")
            AppendLog("=== SUMAR: Passed=" & nPassed.ToString() & "  Failed=" & nFailed.ToString() &
                      "  Error=" & nError.ToString() & "  Skipped=" & nSkipped.ToString() & " ===")
        Finally
            SetRunningState(False)
            If _cts IsNot Nothing Then
                _cts.Dispose()
                _cts = Nothing
            End If
            If _runLog IsNot Nothing Then
                _runLog.Dispose()
                _runLog = Nothing
            End If
            lblStatus.Text = "Gata."
        End Try
    End Function

    Private Function ConfirmLiveOrDestructive(test As IHarnessTest) As DialogResult
        Dim kind As String = If(test.IsDestructive, "DESTRUCTIVE", "LIVE")
        Dim msg As String = "Testul '" & test.Name & "' este " & kind & "." & Environment.NewLine &
                            "Yes = rulează,  No = sari peste,  Cancel = oprește rularea."
        Return MessageBox.Show(Me, msg, "Confirmare test " & kind, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning)
    End Function

    Private Sub SetRunningState(running As Boolean)
        btnRunChecked.Enabled = Not running
        btnRunSelected.Enabled = Not running
        btnRunAll.Enabled = Not running
        btnClear.Enabled = Not running
        btnOpenMainForm.Enabled = Not running
        btnCancel.Enabled = running
        clbTests.Enabled = Not running
        txtFilter.Enabled = Not running
    End Sub

    Private Sub OnProgress(info As HarnessProgressInfo)
        Dim text As String = If(info.Percent.HasValue, info.Percent.Value.ToString() & "% ", "") & If(info.Message, "")
        lblStatus.Text = text
        If Not String.IsNullOrEmpty(info.Message) Then AppendLog("   · " & info.Message)
    End Sub

    ' ---------- output (Invoke-safe) ----------
    Private Sub AppendLog(message As String)
        If rtbResults.InvokeRequired Then
            rtbResults.Invoke(Sub() AppendLogCore(message, Color.Black))
        Else
            AppendLogCore(message, Color.Black)
        End If
    End Sub

    Private Sub AppendVerdict(name As String, outcome As HarnessTestOutcome, message As String, elapsedMs As Long)
        Dim c As Color
        Select Case outcome
            Case HarnessTestOutcome.Passed : c = Color.LimeGreen
            Case HarnessTestOutcome.Failed : c = Color.OrangeRed
            Case HarnessTestOutcome.[Error] : c = Color.Red
            Case Else : c = Color.Gray   ' Skipped
        End Select
        Dim line As String = "[" & outcome.ToString().ToUpperInvariant() & "] " & name &
                             "  (" & elapsedMs.ToString() & " ms)  " & message
        If rtbResults.InvokeRequired Then
            rtbResults.Invoke(Sub() AppendLogCore(line, c))
        Else
            AppendLogCore(line, c)
        End If
    End Sub

    Private Sub AppendLogCore(text As String, color As Color)
        rtbResults.SelectionStart = rtbResults.TextLength
        rtbResults.SelectionLength = 0
        rtbResults.SelectionColor = color
        rtbResults.AppendText(text & Environment.NewLine)
        rtbResults.SelectionColor = rtbResults.ForeColor
        rtbResults.ScrollToCaret()
        ' Tee în fișierul rulării. O eroare de scriere NU e înghițită — se propagă și e
        ' prinsă de bariera handlerului (HandleUiError -> harness_errors.log + dialog),
        ' rularea se oprește vizibil (rezultatele TREBUIE să ajungă în fișier).
        If _runLog IsNot Nothing Then _runLog.WriteLine(text)
    End Sub
End Class
