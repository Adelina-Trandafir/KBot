Imports System.Drawing
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Text
Imports GeneralClasses

Public Class HistoryForm

    ' =========================================================
    ' CÂMPURI PRIVATE
    ' =========================================================
    Private _tvOutput As TreeView

    Friend WithEvents pnlOutputActions As Panel
    Friend WithEvents btnExportOutput As Button

    ' Tab Resend — creat programatic în BuildResendTab()
    Private _tpResend As TabPage
    Private _lbxMessages As ListBox
    Private _btnResend As Button
    Private _pnlResendBottom As Panel
    Private _lblResendStatus As Label

    ' =========================================================
    ' EVENT — ridicat când utilizatorul vrea să retrimită un mesaj
    ' =========================================================
    Public Event ResendRequested(json As String, requiresAck As Boolean)

    ' =========================================================
    ' CONSTRUCTOR
    ' =========================================================
    Public Sub New()
        InitializeComponent()

        ' Tab Output — TreeView dinamic
        _tvOutput = New TreeView() With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Consolas", 10),
            .BorderStyle = BorderStyle.None,
            .ShowLines = True,
            .ShowPlusMinus = True
        }
        tpOutput.Controls.Add(_tvOutput)

        pnlOutputActions = New Panel() With {
            .Dock = DockStyle.Bottom,
            .Height = 40,
            .Padding = New Padding(4)
        }
        btnExportOutput = New Button() With {
            .Text = "💾 Exportă Output...",
            .Dock = DockStyle.Right,
            .Width = 160,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .UseVisualStyleBackColor = True
        }
        pnlOutputActions.Controls.Add(btnExportOutput)
        tpOutput.Controls.Add(pnlOutputActions)
        _tvOutput.BringToFront()

        ' Tab Resend — nou
        BuildResendTab()

        LoadData()
        KBotTheme.ApplyTheme(Me)
    End Sub

    ' =========================================================
    ' BUILD TAB RESEND
    ' =========================================================
    Private Sub BuildResendTab()
        _tpResend = New TabPage() With {
            .Text = "📤 Retrimite",
            .UseVisualStyleBackColor = True,
            .Padding = New Padding(3, 4, 3, 4)
        }

        _pnlResendBottom = New Panel() With {
            .Dock = DockStyle.Bottom,
            .Height = 44,
            .Padding = New Padding(6, 6, 6, 6)
        }

        _btnResend = New Button() With {
            .Text = "🔁 Retrimite la VBA",
            .Dock = DockStyle.Right,
            .Width = 200,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .BackColor = Color.FromArgb(46, 125, 50),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Enabled = False
        }
        _btnResend.FlatAppearance.BorderSize = 0
        AddHandler _btnResend.Click, AddressOf BtnResend_Click

        _lblResendStatus = New Label() With {
            .Dock = DockStyle.Fill,
            .Text = "Selectează un mesaj din listă pentru a-l retrimite la VBA.",
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.DimGray
        }

        _pnlResendBottom.Controls.Add(_lblResendStatus)
        _pnlResendBottom.Controls.Add(_btnResend)

        _lbxMessages = New ListBox() With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Consolas", 9.0F),
            .BorderStyle = BorderStyle.None,
            .IntegralHeight = False
        }
        AddHandler _lbxMessages.SelectedIndexChanged, AddressOf LbxMessages_SelectedIndexChanged

        ' Ordinea de adăugare contează: Bottom se adaugă primul, Fill umple ce rămâne
        _tpResend.Controls.Add(_pnlResendBottom)
        _tpResend.Controls.Add(_lbxMessages)
        _lbxMessages.BringToFront()

        tcDetails.TabPages.Add(_tpResend)
    End Sub

    ' =========================================================
    ' ÎNCĂRCARE DATE — TreeView stânga
    ' =========================================================
    Private Sub LoadData()
        tvHistory.Nodes.Clear()
        If JobHistoryManager.History Is Nothing Then Return

        For Each job In JobHistoryManager.History.AsEnumerable().Reverse()
            Dim title As String = $"[{job.Timestamp:HH:mm:ss}] {job.JobName}"
            Dim node As TreeNode = tvHistory.Nodes.Add(title)
            node.Tag = job

            Dim st As String = If(job.Status, "").ToLower()
            If st.Contains("eroare") OrElse st.Contains("error") Then
                node.ForeColor = Color.Red
            ElseIf st.Contains("succes") Then
                node.ForeColor = Color.Green
            Else
                node.ForeColor = Color.Blue
            End If
        Next

        If tvHistory.Nodes.Count > 0 Then tvHistory.SelectedNode = tvHistory.Nodes(0)
    End Sub

    ' =========================================================
    ' SELECȚIE JOB → actualizare toate tab-urile
    ' =========================================================
    Private Sub tvHistory_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvHistory.AfterSelect
        If e.Node.Tag Is Nothing Then Return
        Dim job = DirectCast(e.Node.Tag, JobHistoryItem)

        ' Tab Log
        rtbLog.Clear()
        rtbLog.Text = job.FullLog.ToString()
        ReapplyLogColors()

        ' Tab Input
        rtbInput.Clear()
        rtbInput.Text = FormatJsonText(job.InputData)

        ' Tab Output
        PopulateOutputTree(job.OutputData)

        ' Tab Resend — NOU
        PopulateResendList(job)
    End Sub

    ' =========================================================
    ' RESEND — populare listă mesaje pipe
    ' =========================================================
    Private Sub PopulateResendList(job As JobHistoryItem)
        _lbxMessages.Items.Clear()
        _btnResend.Enabled = False
        _lblResendStatus.Text = "Selectează un mesaj din listă pentru a-l retrimite la VBA."
        _lblResendStatus.ForeColor = Color.DimGray

        If job.SentPipeMessages Is Nothing OrElse job.SentPipeMessages.Count = 0 Then
            _lbxMessages.Items.Add("(Niciun mesaj pipe înregistrat pentru acest job)")
            Return
        End If

        For Each msg In job.SentPipeMessages
            _lbxMessages.Items.Add(BuildMessageLabel(msg.Json, msg.SentAt))
        Next
    End Sub

    ''' <summary>
    ''' Construiește eticheta afișată în listă pentru un mesaj pipe stocat.
    ''' Format: [HH:mm:ss]  CMD  taskId=N  (X câmpuri)
    ''' </summary>
    Private Function BuildMessageLabel(json As String, sentAt As DateTime) As String
        Try
            Dim obj = JObject.Parse(json)
            Dim cmd As String = If(obj("cmd")?.ToString(), "?")
            Dim taskId As String = If(obj("taskid")?.ToString(), "0")
            Dim extra = obj("extra")
            Dim fieldInfo As String = ""
            If extra IsNot Nothing Then
                If extra.Type = JTokenType.Object Then
                    fieldInfo = $"  ({DirectCast(extra, JObject).Count} câmpuri)"
                ElseIf extra.Type = JTokenType.Array Then
                    fieldInfo = $"  ({DirectCast(extra, JArray).Count} elemente)"
                End If
            End If
            Return $"[{sentAt:HH:mm:ss}]  {cmd}  taskId={taskId}{fieldInfo}"
        Catch
            Return $"[{sentAt:HH:mm:ss}]  {json.Substring(0, Math.Min(80, json.Length))}"
        End Try
    End Function

    ' =========================================================
    ' RESEND — selecție item în listă
    ' =========================================================
    Private Sub LbxMessages_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim idx = _lbxMessages.SelectedIndex
        If idx < 0 Then
            _btnResend.Enabled = False
            Return
        End If

        If tvHistory.SelectedNode Is Nothing OrElse tvHistory.SelectedNode.Tag Is Nothing Then
            _btnResend.Enabled = False
            Return
        End If

        Dim job = DirectCast(tvHistory.SelectedNode.Tag, JobHistoryItem)
        If idx >= job.SentPipeMessages.Count Then
            _btnResend.Enabled = False
            Return
        End If

        _btnResend.Enabled = True
        _lblResendStatus.ForeColor = Color.DimGray
        _lblResendStatus.Text = $"Selectat: {_lbxMessages.SelectedItem}"
    End Sub

    ' =========================================================
    ' RESEND — click buton Retrimite
    ' =========================================================
    Private Sub BtnResend_Click(sender As Object, e As EventArgs)
        If tvHistory.SelectedNode Is Nothing OrElse tvHistory.SelectedNode.Tag Is Nothing Then Return
        Dim idx = _lbxMessages.SelectedIndex
        If idx < 0 Then Return

        Dim job = DirectCast(tvHistory.SelectedNode.Tag, JobHistoryItem)
        If idx >= job.SentPipeMessages.Count Then Return

        Dim msg = job.SentPipeMessages(idx)
        Dim label = _lbxMessages.SelectedItem?.ToString()

        Dim confirm = MessageBox.Show(
            $"Retrimitem mesajul:{Environment.NewLine}{Environment.NewLine}" &
            $"{label}{Environment.NewLine}{Environment.NewLine}" &
            $"Atenție: VBA trebuie să fie în așteptare pentru a procesa răspunsul!",
            "Confirmare Resend",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If confirm <> DialogResult.Yes Then Return

        RaiseEvent ResendRequested(msg.Json, msg.RequiresAck)

        _lblResendStatus.Text = $"✓ Retrimis: {label}"
        _lblResendStatus.ForeColor = Color.FromArgb(46, 125, 50)
        _btnResend.Enabled = False ' dezactivăm după trimitere să nu dea dublu-click
    End Sub

    ' =========================================================
    ' SALVARE TOT ISTORICUL CA RTF
    ' =========================================================
    Private Sub btnSaveAllHistory_Click(sender As Object, e As EventArgs) Handles btnSaveAllHistory.Click
        Using sfd As New SaveFileDialog()
            sfd.Title = "Salvează Istoricul Complet"
            sfd.Filter = "RTF (*.rtf)|*.rtf"
            sfd.FileName = $"IstoricKBOT_{DateTime.Now:yyyyMMdd_HHmmss}"
            If sfd.ShowDialog() <> DialogResult.OK Then Return

            Dim headerFont As New Font("Consolas", 11.0F, FontStyle.Bold)
            Dim normalFont As New Font("Consolas", 9.0F)
            Using tempRtb As New RichTextBox()
                Try
                    For Each job In JobHistoryManager.History
                        tempRtb.SelectionFont = headerFont
                        tempRtb.SelectionColor = Color.FromArgb(30, 80, 140)
                        tempRtb.AppendText($"═══ [{job.Timestamp:HH:mm:ss}] {job.JobName} — {job.Status} ═══")
                        tempRtb.AppendText(Environment.NewLine)

                        tempRtb.SelectionFont = normalFont
                        tempRtb.SelectionColor = Color.Black
                        tempRtb.AppendText(job.FullLog.ToString())
                        tempRtb.AppendText(Environment.NewLine)

                        tempRtb.SelectionColor = Color.LightGray
                        tempRtb.SelectionFont = normalFont
                        tempRtb.AppendText(New String("─"c, 80))
                        tempRtb.AppendText(Environment.NewLine)
                    Next

                    tempRtb.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText)
                    MessageBox.Show($"Istoricul a fost salvat:{Environment.NewLine}{sfd.FileName}",
                                    "Salvare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Catch ex As Exception
                    MessageBox.Show($"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    headerFont.Dispose()
                    normalFont.Dispose()
                End Try
            End Using
        End Using
    End Sub

    ' =========================================================
    ' RECOLORARE LOG
    ' =========================================================
    Private Sub ReapplyLogColors()
        rtbLog.Visible = False

        Dim index As Integer = 0
        While index < rtbLog.Lines.Length
            Dim lineStart As Integer = rtbLog.GetFirstCharIndexFromLine(index)
            Dim lineLength As Integer = rtbLog.Lines(index).Length
            If lineLength > 0 Then
                rtbLog.Select(lineStart, lineLength)
                Dim lineText As String = rtbLog.SelectedText
                rtbLog.SelectionColor = DetermineLineColor(lineText)
            End If
            index += 1
        End While

        rtbLog.Select(0, 0)
        rtbLog.Visible = True
    End Sub

    Private Function DetermineLineColor(lineText As String) As Color
        If lineText.Contains("✗") OrElse lineText.Contains("[Error]") Then
            Return Color.Red
        ElseIf lineText.Contains("✓") OrElse lineText.Contains("[Success]") Then
            Return Color.FromArgb(0, 128, 0)
        ElseIf lineText.Contains("⚠") OrElse lineText.Contains("[Warning]") Then
            Return Color.FromArgb(255, 140, 0)
        ElseIf lineText.Contains("►") OrElse lineText.Contains("[Action]") Then
            Return Color.Blue
        ElseIf lineText.Contains("[Info]") Then
            Return Color.DarkGray
        Else
            Return Color.Black
        End If
    End Function

    ' =========================================================
    ' OUTPUT TREEVIEW — populare
    ' =========================================================
    Private Sub PopulateOutputTree(data As Object)
        _tvOutput.Nodes.Clear()
        If data Is Nothing Then
            _tvOutput.Nodes.Add("No data.")
            Return
        End If

        Try
            If TypeOf data Is Dictionary(Of String, Object) Then
                Dim workflowDict = DirectCast(data, Dictionary(Of String, Object))
                If workflowDict.Count = 0 Then
                    _tvOutput.Nodes.Add("No workflows executed.")
                    Return
                End If
                For Each kvp In workflowDict
                    Dim wfNode As TreeNode = _tvOutput.Nodes.Add(kvp.Key)
                    wfNode.ForeColor = Color.FromArgb(30, 80, 140)
                    wfNode.NodeFont = New Font("Consolas", 10, FontStyle.Bold)
                    If kvp.Value IsNot Nothing Then
                        Dim token As JToken = TryCast(kvp.Value, JToken)
                        If token Is Nothing Then
                            token = JToken.FromObject(kvp.Value)
                        End If
                        AppendTokenToNode(token, wfNode)
                    End If
                    wfNode.Expand()
                Next
            Else
                Dim token As JToken = TryCast(data, JToken)
                If token Is Nothing Then
                    token = JToken.FromObject(data)
                End If
                Dim root As TreeNode = _tvOutput.Nodes.Add("Output")
                AppendTokenToNode(token, root)
                root.Expand()
            End If
        Catch ex As Exception
            _tvOutput.Nodes.Add($"Eroare la afișare: {ex.Message}")
        End Try
    End Sub

    Private Sub AppendTokenToNode(token As JToken, parentNode As TreeNode)
        Select Case token.Type
            Case JTokenType.Object
                Dim obj = DirectCast(token, JObject)
                For Each prop As JProperty In obj.Properties()
                    Dim childNode As TreeNode = parentNode.Nodes.Add(prop.Name)
                    childNode.ForeColor = Color.DarkSlateBlue
                    AppendTokenToNode(prop.Value, childNode)
                Next

            Case JTokenType.Array
                Dim arr = DirectCast(token, JArray)
                Dim index As Integer = 0
                For Each item As JToken In arr
                    Dim childNode As TreeNode = parentNode.Nodes.Add($"[{index}]")
                    childNode.ForeColor = Color.DarkSlateGray
                    AppendTokenToNode(item, childNode)
                    index += 1
                Next

            Case JTokenType.Null
                parentNode.Text &= ": null"
                parentNode.ForeColor = Color.Gray

            Case JTokenType.Boolean
                parentNode.Text &= $": {token.ToString().ToLower()}"
                parentNode.ForeColor = Color.Purple

            Case JTokenType.Integer, JTokenType.Float
                parentNode.Text &= $": {token}"
                parentNode.ForeColor = Color.DarkCyan

            Case Else
                parentNode.Text &= $": {token}"
                parentNode.ForeColor = Color.DarkSlateGray
        End Select
    End Sub

    ' =========================================================
    ' FORMAT JSON
    ' =========================================================
    Private Function FormatJsonText(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then Return ""
        Try
            Return JToken.Parse(input).ToString(Formatting.Indented)
        Catch
            Return input
        End Try
    End Function

    ' =========================================================
    ' EXPORT OUTPUT — handler buton
    ' =========================================================
    Private Sub BtnExportOutput_Click(sender As Object, e As EventArgs) Handles btnExportOutput.Click
        If tvHistory.SelectedNode Is Nothing OrElse tvHistory.SelectedNode.Tag Is Nothing Then
            MessageBox.Show("Selectează un job din listă înainte de export.",
                            "Niciun job selectat", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim job = DirectCast(tvHistory.SelectedNode.Tag, JobHistoryItem)

        Using sfd As New SaveFileDialog()
            sfd.Title = "Exportă Output"
            sfd.Filter = "RTF (*.rtf)|*.rtf|JSON (*.json)|*.json|XML (*.xml)|*.xml|RTF + JSON + XML|*.*"
            sfd.FilterIndex = 1
            sfd.FileName = $"Output_{job.JobName}_{job.Timestamp:yyyyMMdd_HHmmss}"

            If sfd.ShowDialog() <> DialogResult.OK Then Return

            Try
                Dim basePath = IO.Path.Combine(
                    IO.Path.GetDirectoryName(sfd.FileName),
                    IO.Path.GetFileNameWithoutExtension(sfd.FileName))

                Select Case sfd.FilterIndex
                    Case 1
                        ExportOutputRtf(basePath & ".rtf", job)
                        MessageBox.Show($"Exportat RTF:{Environment.NewLine}{basePath}.rtf",
                                        "Export reușit", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case 2
                        ExportOutputJson(basePath & ".json", job)
                        MessageBox.Show($"Exportat JSON:{Environment.NewLine}{basePath}.json",
                                        "Export reușit", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case 3
                        ExportOutputXml(job, sfd.FileName)
                        MessageBox.Show($"Exportat XML:{Environment.NewLine}{sfd.FileName}",
                                        "Export reușit", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Case 4
                        ExportOutputRtf(basePath & ".rtf", job)
                        ExportOutputJson(basePath & ".json", job)
                        ExportOutputXml(job, basePath & ".xml")
                        MessageBox.Show($"Exportate:{Environment.NewLine}{basePath}.rtf{Environment.NewLine}{basePath}.json{Environment.NewLine}{basePath}.xml",
                                        "Export reușit", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Select

            Catch ex As Exception
                MessageBox.Show($"Eroare la export:{Environment.NewLine}{ex.Message}",
                                "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    ' =========================================================
    ' EXPORT RTF
    ' =========================================================
    Private Sub ExportOutputRtf(filePath As String, job As JobHistoryItem)
        Dim headerFont As New Font("Consolas", 11.0F, FontStyle.Bold)
        Dim nodeFont As New Font("Consolas", 9.5F)
        Dim leafFont As New Font("Consolas", 9.0F)

        Using tempRtb As New RichTextBox()
            tempRtb.Font = nodeFont

            tempRtb.SelectionFont = headerFont
            tempRtb.SelectionColor = Color.FromArgb(30, 80, 140)
            tempRtb.AppendText($"OUTPUT — {job.JobName}")
            tempRtb.AppendText(Environment.NewLine)

            tempRtb.SelectionFont = leafFont
            tempRtb.SelectionColor = Color.Gray
            tempRtb.AppendText($"Timestamp: {job.Timestamp:yyyy-MM-dd HH:mm:ss}   Status: {job.Status}")
            tempRtb.AppendText(Environment.NewLine)
            tempRtb.AppendText(New String("─"c, 60))
            tempRtb.AppendText(Environment.NewLine & Environment.NewLine)

            AppendTreeToRtb(tempRtb, _tvOutput.Nodes, 0, nodeFont, leafFont)

            tempRtb.SaveFile(filePath, RichTextBoxStreamType.RichText)
        End Using

        headerFont.Dispose()
        nodeFont.Dispose()
        leafFont.Dispose()
    End Sub

    Private Sub AppendTreeToRtb(rtb As RichTextBox, nodes As TreeNodeCollection,
                                  depth As Integer, nodeFont As Font, leafFont As Font)
        For Each node As TreeNode In nodes
            Dim indent As String = New String(" "c, depth * 3)
            Dim prefix As String = If(node.Nodes.Count > 0, "▸ ", "  ")

            rtb.SelectionFont = If(node.Nodes.Count > 0, nodeFont, leafFont)
            rtb.SelectionColor = If(depth = 0, Color.FromArgb(30, 80, 140),
                                    If(node.Nodes.Count > 0, Color.DarkSlateBlue, Color.FromArgb(50, 50, 50)))
            rtb.AppendText($"{indent}{prefix}{node.Text}")
            rtb.AppendText(Environment.NewLine)

            If node.Nodes.Count > 0 Then
                AppendTreeToRtb(rtb, node.Nodes, depth + 1, nodeFont, leafFont)
            End If
        Next
    End Sub

    ' =========================================================
    ' EXPORT JSON
    ' =========================================================
    Private Sub ExportOutputJson(filePath As String, job As JobHistoryItem)
        Dim root As New JObject()
        root("job") = job.JobName
        root("timestamp") = job.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
        root("status") = job.Status

        Dim variablesNode As New JObject()
        TreeNodesToJson(_tvOutput.Nodes, variablesNode)
        root("output") = variablesNode

        IO.File.WriteAllText(filePath, root.ToString(Formatting.Indented), System.Text.Encoding.UTF8)
    End Sub

    Private Sub TreeNodesToJson(nodes As TreeNodeCollection, parent As JObject)
        For Each node As TreeNode In nodes
            If node.Nodes.Count = 0 Then
                Dim colonIdx As Integer = node.Text.IndexOf(": ")
                If colonIdx > 0 Then
                    Dim k = SanitizeJsonKey(node.Text.Substring(0, colonIdx))
                    Dim v = node.Text.Substring(colonIdx + 2)
                    SafeAdd(parent, k, New JValue(v))
                Else
                    SafeAdd(parent, SanitizeJsonKey(node.Text), New JValue(node.Text))
                End If
            Else
                Dim childObj As New JObject()
                TreeNodesToJson(node.Nodes, childObj)
                SafeAdd(parent, SanitizeJsonKey(node.Text), childObj)
            End If
        Next
    End Sub

    Private Sub SafeAdd(obj As JObject, key As String, value As JToken)
        If Not obj.ContainsKey(key) Then
            obj.Add(key, value)
        Else
            Dim i As Integer = 2
            While obj.ContainsKey($"{key}_{i}") : i += 1 : End While
            obj.Add($"{key}_{i}", value)
        End If
    End Sub

    Private Function SanitizeJsonKey(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then Return "_"
        Dim clean = System.Text.RegularExpressions.Regex.Replace(input.Trim(), "[^\w\-\.\[\]]", "_")
        If clean.Length > 60 Then clean = clean.Substring(0, 60)
        Return If(String.IsNullOrWhiteSpace(clean), "_", clean)
    End Function

    ' =========================================================
    ' EXPORT XML
    ' =========================================================
    Private Sub ExportOutputXml(job As JobHistoryItem, filePath As String)
        Dim doc As New Xml.XmlDocument()
        Dim root = doc.CreateElement("JobOutput")
        doc.AppendChild(root)
        root.SetAttribute("jobName", job.JobName)
        root.SetAttribute("timestamp", job.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))
        root.SetAttribute("status", job.Status)

        For Each kvp In job.OutputData
            Dim key = SanitizeXmlName(kvp.Key)
            Dim elem = doc.CreateElement(key)
            Dim valStr = If(kvp.Value Is Nothing, "", JsonConvert.SerializeObject(kvp.Value))
            If valStr.TrimStart().StartsWith("{"c) OrElse valStr.TrimStart().StartsWith("["c) Then
                Dim cdataSection = doc.CreateCDataSection(valStr)
                elem.AppendChild(cdataSection)
            Else
                elem.InnerText = valStr
            End If
            root.AppendChild(elem)
        Next

        doc.Save(filePath)
    End Sub

    Private Function SanitizeXmlName(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then Return "_"
        Dim clean = System.Text.RegularExpressions.Regex.Replace(input.Trim(), "[^a-zA-Z0-9_\-\.]", "_")
        If clean.Length > 0 AndAlso Char.IsDigit(clean(0)) Then clean = "_" & clean
        Return If(String.IsNullOrWhiteSpace(clean), "_", clean)
    End Function

End Class