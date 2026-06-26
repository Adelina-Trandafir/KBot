Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public NotInheritable Class DevHarnessForm
    Inherits Form

    Private ReadOnly _provider As IServiceProvider
    Private ReadOnly _progress As IProgress(Of HarnessProgressInfo)
    Private _allTests As List(Of IHarnessTest)
    Private _visibleTests As List(Of IHarnessTest)
    Private _cts As CancellationTokenSource

    ' Injectat din Program.vb (Debug). Deschide shell-ul real fără referință la KBot.App.
    Public Property OpenMainFormAction As Action

    ' Controale (instanțiate în BuildUi)
    Private txtFilter As TextBox
    Private clbTests As CheckedListBox
    Private rtbResults As RichTextBox
    Private pbProgress As ProgressBar
    Private lblStatus As Label
    Private btnRunChecked As Button
    Private btnRunSelected As Button
    Private btnRunAll As Button
    Private btnCancel As Button
    Private btnClear As Button
    Private btnOpenMainForm As Button

    Public Sub New(provider As IServiceProvider)
        _provider = provider
        BuildUi()
        _progress = New Progress(Of HarnessProgressInfo)(AddressOf OnProgress)
        _allTests = HarnessTestDiscovery.Discover()
        ApplyFilter()
    End Sub

    Private Sub BuildUi()
        Me.Text = "K-BOT — Dev Harness (DEBUG)"
        Me.Width = 1000
        Me.Height = 660
        Me.StartPosition = FormStartPosition.CenterScreen

        ' --- bara de sus (butoane) ---
        Dim top As New FlowLayoutPanel() With {.Dock = DockStyle.Top, .Height = 38, .Padding = New Padding(6), .WrapContents = False}
        btnRunChecked = NewButton("Run Checked", AddressOf btnRunChecked_Click)
        btnRunSelected = NewButton("Run Selected", AddressOf btnRunSelected_Click)
        btnRunAll = NewButton("Run All", AddressOf btnRunAll_Click)
        btnCancel = NewButton("Cancel", AddressOf btnCancel_Click)
        btnClear = NewButton("Clear", AddressOf btnClear_Click)
        btnOpenMainForm = NewButton("Deschide MainForm", AddressOf btnOpenMainForm_Click)
        btnCancel.Enabled = False
        top.Controls.AddRange(New Control() {btnRunChecked, btnRunSelected, btnRunAll, btnCancel, btnClear, btnOpenMainForm})

        ' --- bara de filtru ---
        Dim filterBar As New Panel() With {.Dock = DockStyle.Top, .Height = 28, .Padding = New Padding(6, 2, 6, 2)}
        Dim lblFilter As New Label() With {.Text = "Filtru:", .Dock = DockStyle.Left, .AutoSize = True, .Padding = New Padding(0, 6, 4, 0)}
        txtFilter = New TextBox() With {.Dock = DockStyle.Fill}
        AddHandler txtFilter.TextChanged, Sub() ApplyFilter()
        filterBar.Controls.Add(txtFilter)
        filterBar.Controls.Add(lblFilter)

        ' --- split: stânga listă, dreapta rezultate ---
        Dim split As New SplitContainer() With {.Dock = DockStyle.Fill, .SplitterDistance = 360}
        clbTests = New CheckedListBox() With {.Dock = DockStyle.Fill, .CheckOnClick = True, .IntegralHeight = False}
        split.Panel1.Controls.Add(clbTests)
        rtbResults = New RichTextBox() With {.Dock = DockStyle.Fill, .ReadOnly = True, .Font = New Font("Consolas", 9.0F)}
        split.Panel2.Controls.Add(rtbResults)

        ' --- jos: progres + status ---
        Dim bottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 42, .Padding = New Padding(6)}
        pbProgress = New ProgressBar() With {.Dock = DockStyle.Top, .Height = 16}
        lblStatus = New Label() With {.Dock = DockStyle.Bottom, .AutoSize = False, .Height = 18, .Text = "Gata."}
        bottom.Controls.Add(pbProgress)
        bottom.Controls.Add(lblStatus)

        Me.Controls.Add(split)
        Me.Controls.Add(bottom)
        Me.Controls.Add(filterBar)
        Me.Controls.Add(top)
    End Sub

    Private Function NewButton(text As String, handler As EventHandler) As Button
        Dim b As New Button() With {.Text = text, .AutoSize = True, .Margin = New Padding(3, 2, 3, 2)}
        AddHandler b.Click, handler
        Return b
    End Function

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
    End Sub

    ' ---------- handlers ----------
    Private Async Sub btnRunChecked_Click(sender As Object, e As EventArgs)
        Dim batch As New List(Of IHarnessTest)()
        For i As Integer = 0 To _visibleTests.Count - 1
            If clbTests.GetItemChecked(i) Then batch.Add(_visibleTests(i))
        Next
        If batch.Count = 0 Then
            AppendLog("Niciun test bifat.")
            Return
        End If
        Await RunTestsAsync(batch)
    End Sub

    Private Async Sub btnRunSelected_Click(sender As Object, e As EventArgs)
        Dim idx As Integer = clbTests.SelectedIndex
        If idx < 0 Then
            AppendLog("Niciun test selectat.")
            Return
        End If
        Await RunTestsAsync(New List(Of IHarnessTest) From {_visibleTests(idx)})
    End Sub

    Private Async Sub btnRunAll_Click(sender As Object, e As EventArgs)
        Await RunTestsAsync(_visibleTests.ToList())
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs)
        If _cts IsNot Nothing Then _cts.Cancel()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs)
        rtbResults.Clear()
    End Sub

    Private Sub btnOpenMainForm_Click(sender As Object, e As EventArgs)
        If OpenMainFormAction IsNot Nothing Then
            OpenMainFormAction.Invoke()
        Else
            AppendLog("OpenMainFormAction nu e cablat (vezi Program.vb).")
        End If
    End Sub

    ' ---------- run loop ----------
    Private Async Function RunTestsAsync(tests As IList(Of IHarnessTest)) As Task
        _cts = New CancellationTokenSource()
        SetRunningState(True)
        pbProgress.Minimum = 0
        pbProgress.Maximum = tests.Count
        pbProgress.Value = 0

        Dim ctx As New HarnessContext(_provider, AddressOf AppendLog, _progress)
        Try
            For Each test As IHarnessTest In tests
                If _cts.IsCancellationRequested Then
                    AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "run cancelled", 0)
                    Continue For
                End If

                If test.RequiresLiveConnection OrElse test.IsDestructive Then
                    Dim decision As DialogResult = ConfirmLiveOrDestructive(test)
                    If decision = DialogResult.Cancel Then
                        _cts.Cancel()
                        AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "run cancelled", 0)
                        Continue For
                    ElseIf decision = DialogResult.No Then
                        AppendVerdict(test.Name, HarnessTestOutcome.Skipped, "skipped by user", 0)
                        pbProgress.Value += 1
                        Continue For
                    End If
                End If

                AppendLog("── RUN [" & test.Category & "] " & test.Name)
                Dim sw As Stopwatch = Stopwatch.StartNew()
                Dim res As HarnessTestResult
                Try
                    res = Await test.RunAsync(ctx, _cts.Token)
                Catch ocex As OperationCanceledException
                    res = HarnessTestResult.Skipped("cancelled")
                Catch ex As Exception
                    res = HarnessTestResult.Errored(ex)
                End Try
                sw.Stop()
                AppendVerdict(test.Name, res.Outcome, res.Message, sw.ElapsedMilliseconds)
                If Not String.IsNullOrEmpty(res.Details) Then AppendLog(res.Details)
                pbProgress.Value += 1
            Next
        Finally
            SetRunningState(False)
            _cts.Dispose()
            _cts = Nothing
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
            rtbResults.Invoke(Sub() AppendLogCore(message, Color.Gainsboro))
        Else
            AppendLogCore(message, Color.Gainsboro)
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
    End Sub
End Class
