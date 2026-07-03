Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms

' RootNamespace = AvacontPush, so no Namespace block here.
Partial Public Class Form1

    ' Tolerance (seconds) to absorb filesystem mtime rounding when comparing.
    Private Const MtimeToleranceSeconds As Integer = 2

    Private _settings As PushSettings

    ' Guards tvFiles.AfterCheck while we cascade a tick down to child nodes,
    ' so the programmatic child changes do not re-enter the handler.
    Private _suppressAfterCheck As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _settings = AppConfigStore.Load()

            If String.IsNullOrWhiteSpace(_settings.LocalRoot) Then
                ' Default: parent of the app folder. The EXE is deployed to PYTHON\_push\,
                ' so the parent is the PYTHON folder itself.
                Dim baseDir = New DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))
                _settings.LocalRoot = If(baseDir.Parent IsNot Nothing, baseDir.Parent.FullName, baseDir.FullName)
            End If

            LoadSettingsToUi()
            lblStatus.Text = "Pregătit."
        Catch ex As Exception
            MessageBox.Show("Nu s-a putut încărca configurația: " & ex.Message,
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            _settings = New PushSettings()
            LoadSettingsToUi()
        End Try
    End Sub

    Private Sub LoadSettingsToUi()
        txtHost.Text = _settings.Host
        txtPort.Text = _settings.Port.ToString(CultureInfo.InvariantCulture)
        txtUser.Text = _settings.User
        txtPassword.Text = _settings.Password
        txtLocalRoot.Text = _settings.LocalRoot
        txtRemoteRoot.Text = _settings.RemoteRoot
    End Sub

    ' Copies the editable UI fields back into _settings. Throws (Romanian) on bad input.
    Private Sub ApplyUiToSettings()
        Dim port As Integer
        If Not Integer.TryParse(txtPort.Text.Trim(), port) OrElse port <= 0 OrElse port > 65535 Then
            Throw New ApplicationException("Portul introdus nu este valid.")
        End If

        _settings.Host = txtHost.Text.Trim()
        _settings.Port = port
        _settings.User = txtUser.Text.Trim()
        _settings.Password = txtPassword.Text
        _settings.LocalRoot = txtLocalRoot.Text.Trim()
        _settings.RemoteRoot = txtRemoteRoot.Text.Trim().TrimEnd("/"c)

        If _settings.Host = "" Then Throw New ApplicationException("Hostul nu poate fi gol.")
        If _settings.User = "" Then Throw New ApplicationException("Utilizatorul nu poate fi gol.")
        If _settings.RemoteRoot = "" Then Throw New ApplicationException("Rădăcina server nu poate fi goală.")
    End Sub

    Private Sub btnBrowseLocal_Click(sender As Object, e As EventArgs) Handles btnBrowseLocal.Click
        If Directory.Exists(txtLocalRoot.Text.Trim()) Then
            dlgFolder.SelectedPath = txtLocalRoot.Text.Trim()
        End If
        If dlgFolder.ShowDialog(Me) = DialogResult.OK Then
            txtLocalRoot.Text = dlgFolder.SelectedPath
        End If
    End Sub

    ' ---------------------------------------------------------------- SCAN

    Private Async Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        Try
            ApplyUiToSettings()

            If Not Directory.Exists(_settings.LocalRoot) Then
                MessageBox.Show("Folderul local nu există. Verificați rădăcina locală.",
                                "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            SetBusy(True, "Se scanează...")

            Dim results As List(Of FileCompareResult) = Nothing
            Dim observedFp As String = ""

            Await Task.Run(
                Sub()
                    Using log As New RunLogger()
                        Using svc As New SftpService(_settings, log)
                            svc.Connect()
                            observedFp = svc.ObservedFingerprint
                            results = CompareFiles(svc, log)
                        End Using
                    End Using
                End Sub)

            ' Pin the host key on first successful connect, then persist.
            If _settings.HostKeyFingerprint.Trim() = "" AndAlso observedFp <> "" Then
                _settings.HostKeyFingerprint = observedFp
            End If
            AppConfigStore.Save(_settings)

            PopulateTree(results)

            ' Use Where(...).Count() so this binds to the LINQ extension, not List.Count.
            Dim toSend = results.Where(Function(r) r.State <> FileState.Identical).Count()
            SetBusy(False, $"Scanare completă: {results.Count} fișiere, {toSend} bifate pentru trimitere.")
        Catch ex As Exception
            SetBusy(False, "Scanare eșuată.")
            MessageBox.Show(ex.Message, "Eroare la scanare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Enumerates local files recursively, skips ignored ones, and compares each to the server.
    Private Function CompareFiles(svc As SftpService, log As RunLogger) As List(Of FileCompareResult)
        Dim list As New List(Of FileCompareResult)()
        Dim localRoot = _settings.LocalRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim remoteRoot = _settings.RemoteRoot.TrimEnd("/"c)

        For Each localFile In Directory.EnumerateFiles(localRoot, "*", SearchOption.AllDirectories)
            Dim rel = localFile.Substring(localRoot.Length + 1)
            If IsIgnored(rel, _settings.IgnorePatterns) Then Continue For
            ' Push only allowed source/config extensions (py, json, xml, ...); skip the rest.
            If Not IsIncludedExtension(rel, _settings.IncludeExtensions) Then Continue For

            Dim relRemote = rel.Replace("\"c, "/"c)
            Dim remotePath = remoteRoot & "/" & relRemote
            Dim localUtc = File.GetLastWriteTimeUtc(localFile)

            Dim remoteUtc As DateTime
            Dim existsRemote = svc.TryGetRemoteMtimeUtc(remotePath, remoteUtc)

            Dim state As FileState
            If Not existsRemote Then
                state = FileState.NewOnServer
            ElseIf localUtc > remoteUtc.AddSeconds(MtimeToleranceSeconds) Then
                state = FileState.Modified
            Else
                state = FileState.Identical
            End If

            list.Add(New FileCompareResult With {
                .RelativePath = relRemote,
                .LocalFullPath = localFile,
                .RemotePath = remotePath,
                .State = state,
                .LocalMtimeUtc = localUtc,
                .RemoteMtimeUtc = If(existsRemote, remoteUtc, DateTime.MinValue)
            })

            log.Write($"SCAN {StateText(state)}  {relRemote}")
        Next

        Return list.OrderBy(Function(r) r.RelativePath, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    ' True when any path segment equals a plain pattern, or the file matches a "*.ext" pattern.
    Private Shared Function IsIgnored(relativePath As String, patterns As List(Of String)) As Boolean
        If patterns Is Nothing Then Return False
        Dim rel = relativePath.Replace("\"c, "/"c)
        Dim segments = rel.Split("/"c)

        For Each pat In patterns
            Dim p = If(pat, "").Trim()
            If p = "" Then Continue For

            If p.StartsWith("*.", StringComparison.Ordinal) Then
                Dim ext = p.Substring(1) ' ".pyc"
                If rel.EndsWith(ext, StringComparison.OrdinalIgnoreCase) Then Return True
            Else
                For Each seg In segments
                    If String.Equals(seg, p, StringComparison.OrdinalIgnoreCase) Then Return True
                Next
            End If
        Next
        Return False
    End Function

    ' True when the file's extension is in the allow-list (compared without the dot).
    ' Files with no extension, or an extension not listed, are excluded.
    Private Shared Function IsIncludedExtension(relativePath As String, extensions As List(Of String)) As Boolean
        If extensions Is Nothing OrElse extensions.Count = 0 Then Return False
        Dim ext = Path.GetExtension(relativePath).TrimStart("."c)
        If ext = "" Then Return False

        For Each allowed In extensions
            Dim a = If(allowed, "").Trim().TrimStart("."c)
            If a <> "" AndAlso String.Equals(a, ext, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    ' Builds the folder/file tree from the flat scan results. Leaf nodes carry the
    ' FileCompareResult in .Tag; folder nodes have no tag.
    Private Sub PopulateTree(results As List(Of FileCompareResult))
        _suppressAfterCheck = True
        tvFiles.BeginUpdate()
        Try
            tvFiles.Nodes.Clear()
            For Each r In results
                AddResultNode(r)
            Next
            ' Set each folder's initial check from its descendants (all checked -> checked).
            For Each root As TreeNode In tvFiles.Nodes
                UpdateFolderCheckState(root)
            Next
            tvFiles.ExpandAll()
            If tvFiles.Nodes.Count > 0 Then tvFiles.Nodes(0).EnsureVisible()
        Finally
            tvFiles.EndUpdate()
            _suppressAfterCheck = False
        End Try
    End Sub

    ' Walks the relative path, creating folder nodes as needed, then adds the file leaf.
    Private Sub AddResultNode(r As FileCompareResult)
        Dim parts = r.RelativePath.Split("/"c)
        Dim nodes = tvFiles.Nodes

        ' Every segment except the last is a folder.
        For i As Integer = 0 To parts.Length - 2
            nodes = FindOrAddFolder(nodes, parts(i)).Nodes
        Next

        Dim leaf As New TreeNode(FileNodeText(parts(parts.Length - 1), r.State)) With {
            .Tag = r,
            .ToolTipText = TooltipFor(r),
            .ForeColor = ColorFor(r.State),
            .Checked = (r.State <> FileState.Identical)
        }
        nodes.Add(leaf)
    End Sub

    ' Finds an existing folder node by name (folder nodes have no tag) or creates one.
    Private Shared Function FindOrAddFolder(nodes As TreeNodeCollection, name As String) As TreeNode
        For Each n As TreeNode In nodes
            If n.Tag Is Nothing AndAlso String.Equals(n.Text, name, StringComparison.Ordinal) Then
                Return n
            End If
        Next
        Dim folder As New TreeNode(name)
        nodes.Add(folder)
        Return folder
    End Function

    ' Bottom-up: a folder is checked only when every descendant leaf is checked.
    ' Returns whether the node (leaf or folder) is fully checked.
    Private Shared Function UpdateFolderCheckState(node As TreeNode) As Boolean
        If node.Nodes.Count = 0 Then Return node.Checked

        Dim allChecked = True
        For Each child As TreeNode In node.Nodes
            If Not UpdateFolderCheckState(child) Then allChecked = False
        Next
        node.Checked = allChecked
        Return allChecked
    End Function

    ' Cascade a tick/untick to every child node, per the requested behavior.
    Private Sub tvFiles_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvFiles.AfterCheck
        If _suppressAfterCheck Then Return
        _suppressAfterCheck = True
        Try
            SetChildrenChecked(e.Node, e.Node.Checked)
        Finally
            _suppressAfterCheck = False
        End Try
    End Sub

    Private Shared Sub SetChildrenChecked(node As TreeNode, value As Boolean)
        For Each child As TreeNode In node.Nodes
            child.Checked = value
            SetChildrenChecked(child, value)
        Next
    End Sub

    ' Collects checked file leaves (those carrying a FileCompareResult) across the tree.
    Private Shared Sub CollectCheckedFiles(nodes As TreeNodeCollection, into As List(Of FileCompareResult))
        For Each n As TreeNode In nodes
            Dim r = TryCast(n.Tag, FileCompareResult)
            If r IsNot Nothing Then
                If n.Checked Then into.Add(r)
            Else
                CollectCheckedFiles(n.Nodes, into)
            End If
        Next
    End Sub

    Private Shared Function FileNodeText(fileName As String, state As FileState) As String
        Return $"{fileName}   [{StateText(state)}]"
    End Function

    Private Shared Function TooltipFor(r As FileCompareResult) As String
        Dim loc = r.LocalMtimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        Dim srv = If(r.RemoteMtimeUtc = DateTime.MinValue, "-",
                     r.RemoteMtimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
        Return $"Local (UTC): {loc}    Server (UTC): {srv}"
    End Function

    Private Shared Function ColorFor(state As FileState) As Color
        Select Case state
            Case FileState.NewOnServer : Return Color.Green
            Case FileState.Modified : Return Color.FromArgb(0, 0, 160)
            Case Else : Return Color.Gray
        End Select
    End Function

    Private Shared Function StateText(s As FileState) As String
        Select Case s
            Case FileState.NewOnServer : Return "NOU pe server"
            Case FileState.Modified : Return "MODIFICAT"
            Case Else : Return "IDENTIC"
        End Select
    End Function

    ' ---------------------------------------------------------------- PUSH

    Private Async Sub btnPush_Click(sender As Object, e As EventArgs) Handles btnPush.Click
        Try
            Dim files As New List(Of FileCompareResult)()
            CollectCheckedFiles(tvFiles.Nodes, files)
            If files.Count = 0 Then
                MessageBox.Show("Nu este bifat niciun fișier pentru trimitere.",
                                "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ApplyUiToSettings()
            AppConfigStore.Save(_settings)

            SetBusy(True, "Se trimit fișierele...")
            rtbOutput.Clear()
            pbProgress.Minimum = 0
            pbProgress.Value = 0
            pbProgress.Maximum = files.Count

            Dim restart = chkRestart.Checked

            Dim progress As New Progress(Of String)(Sub(msg) AppendOutput(msg))
            Dim progressCount As New Progress(Of Integer)(Sub(n) pbProgress.Value = Math.Min(n, pbProgress.Maximum))

            Dim failed As Integer = 0

            Await Task.Run(
                Sub()
                    Using log As New RunLogger()
                        Using svc As New SftpService(_settings, log)
                            svc.Connect()
                            Dim createdDirs As New HashSet(Of String)(StringComparer.Ordinal)
                            Dim done As Integer = 0

                            For Each f In files
                                Try
                                    svc.UploadFile(f.LocalFullPath, f.RemotePath, _settings.PreserveMTime, createdDirs)
                                    log.Write($"PUSH OK      {f.RelativePath}")
                                    DirectCast(progress, IProgress(Of String)).Report($"OK      {f.RelativePath}")
                                Catch ex As Exception
                                    failed += 1
                                    log.Write($"PUSH EROARE  {f.RelativePath} :: {ex.Message}")
                                    DirectCast(progress, IProgress(Of String)).Report($"EROARE  {f.RelativePath} — {ex.Message}")
                                End Try
                                done += 1
                                DirectCast(progressCount, IProgress(Of Integer)).Report(done)
                            Next
                        End Using

                        If restart Then
                            DirectCast(progress, IProgress(Of String)).Report("")
                            DirectCast(progress, IProgress(Of String)).Report("=== Repornire serviciu avacont ===")
                            RunRestart(progress, log)
                        End If
                    End Using
                End Sub)

            Dim summary = If(failed = 0,
                             $"Trimitere completă: {files.Count} fișiere, fără erori.",
                             $"Trimitere terminată cu {failed} erori din {files.Count} fișiere.")
            SetBusy(False, summary)
        Catch ex As Exception
            SetBusy(False, "Trimitere eșuată.")
            MessageBox.Show(ex.Message, "Eroare la trimitere", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Runs daemon-reload, restart, then journalctl. journalctl always runs so a
    ' failed restart is still visible. Runs on the worker thread; reports via IProgress.
    Private Sub RunRestart(progress As IProgress(Of String), log As RunLogger)
        Using ssh As New SshRestartService(_settings)
            ssh.Connect()

            ' Loop var is cmdText, not "command": VB resolves "command" to the built-in Command() function.
            For Each cmdText In New String() {"systemctl daemon-reload", "systemctl restart avacont"}
                Dim outText As String = "", errText As String = "", status As Integer = 0
                ssh.RunCommand(cmdText, status, outText, errText)
                log.Write($"CMD {cmdText} -> exit {status}")
                progress.Report($"$ {cmdText}   (exit {status})")
                If outText.Trim() <> "" Then progress.Report(outText.TrimEnd())
                If errText.Trim() <> "" Then progress.Report(errText.TrimEnd())
                If status <> 0 Then progress.Report($"AVERTISMENT: comanda a returnat codul {status}.")
            Next

            Dim jOut As String = "", jErr As String = "", jStatus As Integer = 0
            ssh.RunCommand("journalctl -u avacont -n 30 --no-pager", jStatus, jOut, jErr)
            log.Write($"CMD journalctl -> exit {jStatus}")
            progress.Report("")
            progress.Report("$ journalctl -u avacont -n 30 --no-pager")
            If jOut.Trim() <> "" Then progress.Report(jOut.TrimEnd())
            If jErr.Trim() <> "" Then progress.Report(jErr.TrimEnd())
        End Using
    End Sub

    ' ---------------------------------------------------------------- UI helpers

    Private Sub AppendOutput(msg As String)
        rtbOutput.AppendText(msg & Environment.NewLine)
        rtbOutput.SelectionStart = rtbOutput.TextLength
        rtbOutput.ScrollToCaret()
    End Sub

    Private Sub SetBusy(busy As Boolean, status As String)
        btnScan.Enabled = Not busy
        btnPush.Enabled = Not busy
        btnBrowseLocal.Enabled = Not busy
        lblStatus.Text = status
        Me.UseWaitCursor = busy
    End Sub

    Private Sub tlpRoot_Paint(sender As Object, e As PaintEventArgs) Handles tlpRoot.Paint

    End Sub
End Class
