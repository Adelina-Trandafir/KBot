Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports GeneralClasses

Public Class ResendForm

    Private Const ResendExtension As String = ".resend.json"

    ' =========================================================================
    ' CULORI TEMĂ KBOT
    ' =========================================================================
    Private Shared ReadOnly CLR_BG As Color = Color.FromArgb(28, 28, 28)
    Private Shared ReadOnly CLR_BG_PANEL As Color = Color.FromArgb(45, 45, 48)
    Private Shared ReadOnly CLR_FG As Color = Color.FromArgb(210, 210, 210)
    Private Shared ReadOnly CLR_FG_DIM As Color = Color.FromArgb(115, 115, 115)
    Private Shared ReadOnly CLR_BTN As Color = Color.FromArgb(62, 62, 66)
    Private Shared ReadOnly CLR_BTN_BORDER As Color = Color.FromArgb(85, 85, 88)

    ' Culori token tree
    Private Shared ReadOnly CLR_NODE_ROOT As Color = Color.FromArgb(86, 156, 214)
    Private Shared ReadOnly CLR_NODE_MSG_ACK As Color = Color.FromArgb(220, 185, 100)
    Private Shared ReadOnly CLR_NODE_MSG_NOACK As Color = Color.FromArgb(155, 210, 155)
    Private Shared ReadOnly CLR_KEY As Color = Color.FromArgb(156, 220, 254)
    Private Shared ReadOnly CLR_VAL_STR As Color = Color.FromArgb(206, 145, 120)
    Private Shared ReadOnly CLR_VAL_NUM As Color = Color.FromArgb(181, 206, 168)
    Private Shared ReadOnly CLR_VAL_BOOL As Color = Color.FromArgb(86, 156, 214)
    Private Shared ReadOnly CLR_VAL_NULL As Color = Color.FromArgb(115, 115, 115)
    Private Shared ReadOnly CLR_ARR_IDX As Color = Color.FromArgb(180, 180, 180)
    Private Shared ReadOnly CLR_ERR As Color = Color.FromArgb(252, 57, 57)
    Private Shared ReadOnly CLR_META As Color = Color.FromArgb(115, 115, 115)

    ' =========================================================================
    ' CÂMPURI PRIVATE
    ' =========================================================================
    Private _currentFolder As String
    Private _currentPkg As ResendPackage
    Private _allFiles As New List(Of String)   ' căi complete, sortate desc după dată

    ' Controale adăugate programatic în taburi
    Private _tvPackage As TreeView
    Private _rtbJson As RichTextBox

    ' =========================================================================
    ' EVENT — ridicat când utilizatorul confirmă retrimiterea
    ' =========================================================================
    Public Event ResendRequested(json As String, requiresAck As Boolean)

    ' =========================================================================
    ' CONSTRUCTOR
    ' =========================================================================
    Friend Sub New(folderPath As String)
        InitializeComponent()
        _currentFolder = folderPath
        BuildRightPanelControls()
        KBotTheme.ApplyTheme(Me)
        RefreshFileList()
    End Sub

    ' =========================================================================
    ' BUILD — TreeView și RichTextBox în taburi (programatic, temă neagră)
    ' =========================================================================
    Private Sub BuildRightPanelControls()

        ' Tab 1: TreeView structură pachet
        _tvPackage = New TreeView() With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Consolas", 9.5F),
            .BackColor = CLR_BG,
            .ForeColor = CLR_FG,
            .BorderStyle = BorderStyle.None,
            .ShowLines = True,
            .ShowRootLines = True,
            .FullRowSelect = True,
            .HotTracking = False,
            .ShowNodeToolTips = False,
            .Indent = 20
        }
        AddHandler _tvPackage.AfterSelect, AddressOf tvPackage_AfterSelect
        tpTree.Controls.Add(_tvPackage)

        ' Tab 2: RichTextBox JSON raw
        _rtbJson = New RichTextBox() With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Consolas", 9.5F),
            .BackColor = CLR_BG,
            .ForeColor = CLR_FG,
            .BorderStyle = BorderStyle.None,
            .ReadOnly = True,
            .WordWrap = False,
            .ScrollBars = RichTextBoxScrollBars.Both
        }
        tpJson.Controls.Add(_rtbJson)
    End Sub

    ' =========================================================================
    ' POPULARE LISTĂ FIȘIERE
    ' =========================================================================
    Private Sub RefreshFileList()
        _allFiles.Clear()
        lbxFiles.Items.Clear()
        ClearDetails()

        If String.IsNullOrEmpty(_currentFolder) OrElse Not Directory.Exists(_currentFolder) Then
            lblStatus.Text = $"Folderul nu există: {If(_currentFolder, "(neconfigurat)")}"
            Return
        End If

        Try
            Dim files() As String = Directory.GetFiles(
                _currentFolder, $"*{ResendExtension}", SearchOption.TopDirectoryOnly)

            ' Sortare descrescătoare — cel mai nou primul
            Array.Sort(files,
                Function(a, b) File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)))

            For Each f In files
                _allFiles.Add(f)
                lbxFiles.Items.Add(StripResendExtension(Path.GetFileName(f)))
            Next

            If _allFiles.Count = 0 Then
                lblStatus.Text = $"Niciun fișier în: {_currentFolder}"
            Else
                lblStatus.Text = $"{_allFiles.Count} fișier(e)  |  {_currentFolder}"
                lbxFiles.SelectedIndex = 0   ' auto-selectează cel mai nou
            End If

        Catch ex As Exception
            lblStatus.Text = $"Eroare citire folder: {ex.Message}"
        End Try
    End Sub

    ''' <summary>Elimină sufixul ".resend.json" din numele de fișier afișat în listă.</summary>
    Private Shared Function StripResendExtension(fileName As String) As String
        If fileName.EndsWith(ResendExtension, StringComparison.OrdinalIgnoreCase) Then
            Return fileName.Substring(0, fileName.Length - ResendExtension.Length)
        End If
        Return fileName
    End Function

    Private Sub ClearDetails()
        _currentPkg = Nothing
        If _tvPackage IsNot Nothing Then _tvPackage.Nodes.Clear()
        If _rtbJson IsNot Nothing Then _rtbJson.Clear()
        btnResend.Enabled = False
        lblStatus.Text = "Selectează un fișier din listă."
    End Sub

    ' =========================================================================
    ' SELECȚIE FIȘIER → încarcă pachetul
    ' =========================================================================
    Private Sub lbxFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lbxFiles.SelectedIndexChanged
        Dim idx = lbxFiles.SelectedIndex
        If idx < 0 OrElse idx >= _allFiles.Count Then Return
        LoadPackageFromFile(_allFiles(idx))
    End Sub

    Private Sub LoadPackageFromFile(filePath As String)
        _currentPkg = Nothing
        _tvPackage.Nodes.Clear()
        _rtbJson.Clear()
        btnResend.Enabled = False

        Try
            Dim jsonRaw As String = File.ReadAllText(filePath, System.Text.Encoding.UTF8)
            _currentPkg = JsonConvert.DeserializeObject(Of ResendPackage)(jsonRaw)

            If _currentPkg Is Nothing OrElse
               _currentPkg.Messages Is Nothing OrElse
               _currentPkg.Messages.Count = 0 Then
                lblStatus.Text = "Fișier invalid sau fără mesaje."
                Return
            End If

            ' ── Tab JSON raw ──────────────────────────────────────────
            Try
                _rtbJson.Text = JObject.Parse(jsonRaw).ToString(Formatting.Indented)
            Catch
                _rtbJson.Text = jsonRaw   ' fallback: raw text dacă parse-ul pică
            End Try

            ' ── Tab Structură ─────────────────────────────────────────
            PopulateTree(_currentPkg)

            lblStatus.Text = $"{_currentPkg.JobName}  |  {_currentPkg.Messages.Count} mesaj(e)  |  {_currentPkg.SavedAt:dd.MM.yyyy HH:mm}"
            Me.Text = $"📤  Resend Manager — {StripResendExtension(Path.GetFileName(filePath))}"

        Catch ex As Exception
            lblStatus.Text = $"Eroare încărcare: {ex.Message}"
        End Try
    End Sub

    ' =========================================================================
    ' POPULARE TREE
    ' =========================================================================
    Private Sub PopulateTree(pkg As ResendPackage)
        _tvPackage.BeginUpdate()
        _tvPackage.Nodes.Clear()

        Dim rootNode As New TreeNode(
            $"📦  {pkg.JobName}     [{pkg.SavedAt:dd.MM.yyyy HH:mm:ss}]") With {
            .ForeColor = CLR_NODE_ROOT,
            .NodeFont = New Font("Consolas", 9.5F, FontStyle.Bold),
            .Tag = Nothing
        }

        For i = 0 To pkg.Messages.Count - 1
            rootNode.Nodes.Add(BuildMessageNode(i, pkg.Messages(i)))
        Next

        _tvPackage.Nodes.Add(rootNode)
        rootNode.Expand()
        _tvPackage.EndUpdate()
    End Sub

    Private Function BuildMessageNode(index As Integer, msg As SentPipeMessage) As TreeNode
        Dim label As String = BuildMsgLabel(index, msg)
        Dim color As Color = If(msg.RequiresAck, CLR_NODE_MSG_ACK, CLR_NODE_MSG_NOACK)

        Dim msgNode As New TreeNode(label) With {
            .Tag = msg,
            .ForeColor = color,
            .NodeFont = New Font("Consolas", 9.5F, FontStyle.Bold)
        }

        ' Câmpurile JSON ale mesajului
        Try
            Dim obj = JObject.Parse(msg.Json)
            For Each prop As JProperty In obj.Properties()
                Dim child As New TreeNode(prop.Name) With {.ForeColor = CLR_KEY}
                AppendTokenToNode(prop.Value, child)
                msgNode.Nodes.Add(child)
            Next
        Catch
            msgNode.Nodes.Add(New TreeNode("⚠  JSON invalid") With {.ForeColor = CLR_ERR})
        End Try

        ' Metadate trimitere — SentAt ca ultim sub-nod, dim
        msgNode.Nodes.Add(New TreeNode($"🕒  {msg.SentAt:dd.MM.yyyy HH:mm:ss}") With {
            .ForeColor = CLR_META
        })

        msgNode.Expand()
        Return msgNode
    End Function

    ''' <summary>Construiește eticheta unui nod mesaj: index, cmd, taskId, info extra, ack.</summary>
    Private Shared Function BuildMsgLabel(index As Integer, msg As SentPipeMessage) As String
        Try
            Dim obj = JObject.Parse(msg.Json)
            Dim cmd As String = If(obj("cmd")?.ToString(), "?")
            Dim taskId As String = If(obj("taskid")?.ToString(), "0")
            Dim extra = obj("extra")
            Dim fieldInfo As String = ""
            If extra IsNot Nothing Then
                Select Case extra.Type
                    Case JTokenType.Object : fieldInfo = $"  ({DirectCast(extra, JObject).Count} câmpuri)"
                    Case JTokenType.Array : fieldInfo = $"  ({DirectCast(extra, JArray).Count} elem.)"
                End Select
            End If
            Dim ackMark As String = If(msg.RequiresAck, "", "  [no-ack]")
            Return $"[{index}]  {cmd}  taskId={taskId}{fieldInfo}{ackMark}"
        Catch
            Return $"[{index}]  {msg.Json.Substring(0, Math.Min(60, msg.Json.Length))}"
        End Try
    End Function

    ''' <summary>
    ''' Adaugă recursiv token-uri JSON ca noduri copil.
    ''' Urmează același pattern ca AppendTokenToNode din HistoryForm.
    ''' </summary>
    Private Sub AppendTokenToNode(token As JToken, parentNode As TreeNode)
        Select Case token.Type

            Case JTokenType.Object
                Dim obj = DirectCast(token, JObject)
                For Each prop As JProperty In obj.Properties()
                    Dim child As New TreeNode(prop.Name) With {.ForeColor = CLR_KEY}
                    AppendTokenToNode(prop.Value, child)
                    parentNode.Nodes.Add(child)
                Next

            Case JTokenType.Array
                Dim arr = DirectCast(token, JArray)
                For i = 0 To arr.Count - 1
                    Dim child As New TreeNode($"[{i}]") With {.ForeColor = CLR_ARR_IDX}
                    AppendTokenToNode(arr(i), child)
                    parentNode.Nodes.Add(child)
                Next

            Case JTokenType.Null
                parentNode.Text &= ": null"
                parentNode.ForeColor = CLR_VAL_NULL

            Case JTokenType.Boolean
                parentNode.Text &= $": {token.ToString().ToLower()}"
                parentNode.ForeColor = CLR_VAL_BOOL

            Case JTokenType.Integer, JTokenType.Float
                parentNode.Text &= $": {token}"
                parentNode.ForeColor = CLR_VAL_NUM

            Case Else   ' String, Date, etc.
                Dim val As String = token.ToString()
                If val.Length > 80 Then val = val.Substring(0, 77) & "…"
                parentNode.Text &= $": {val}"
                parentNode.ForeColor = CLR_VAL_STR

        End Select
    End Sub

    ' =========================================================================
    ' SELECȚIE NOD TREE → activare buton Send + update status
    ' =========================================================================
    Private Sub tvPackage_AfterSelect(sender As Object, e As TreeViewEventArgs)
        Dim msg = GetSelectedMessage()
        If msg IsNot Nothing Then
            btnResend.Enabled = True
            Try
                Dim obj = JObject.Parse(msg.Json)
                Dim cmd = If(obj("cmd")?.ToString(), "?")
                Dim taskId = If(obj("taskid")?.ToString(), "?")
                lblStatus.Text = $"►  {cmd}  taskId={taskId}  |  {msg.SentAt:HH:mm:ss}  |  Ack={msg.RequiresAck}"
            Catch
                lblStatus.Text = "Mesaj selectat."
            End Try
        Else
            btnResend.Enabled = False
            If _currentPkg IsNot Nothing Then
                lblStatus.Text = $"{_currentPkg.JobName}  |  {_currentPkg.Messages.Count} mesaj(e)  |  {_currentPkg.SavedAt:dd.MM.yyyy HH:mm}"
            End If
        End If
    End Sub

    ''' <summary>
    ''' Urcă în ierarhia nodului selectat până găsește un nod cu Tag = SentPipeMessage.
    ''' Permite click pe orice sub-nod al unui mesaj (câmpuri, valori) fără să pierzi selecția.
    ''' </summary>
    Private Function GetSelectedMessage() As SentPipeMessage
        Dim node = _tvPackage.SelectedNode
        While node IsNot Nothing
            If TypeOf node.Tag Is SentPipeMessage Then
                Return DirectCast(node.Tag, SentPipeMessage)
            End If
            node = node.Parent
        End While
        Return Nothing
    End Function

    ' =========================================================================
    ' SEND — confirmare + trimitere + minimizare
    ' =========================================================================
    Private Sub btnResend_Click(sender As Object, e As EventArgs) Handles btnResend.Click
        Dim msg = GetSelectedMessage()
        If msg Is Nothing Then
            lblStatus.Text = "Niciun mesaj selectat."
            Return
        End If

        Dim cmdLabel As String = TryGetCmd(msg.Json)

        Dim confirm = MessageBox.Show(
            $"Retrimitem mesajul:{Environment.NewLine}{Environment.NewLine}" &
            $"  {BuildMsgLabel(GetSelectedMessageIndex(msg), msg)}{Environment.NewLine}{Environment.NewLine}" &
            "Atenție: VBA trebuie să fie în așteptare!",
            "Confirmare Resend",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If confirm <> DialogResult.Yes Then Return

        lblStatus.Text = $"✓  Trimis: {cmdLabel}  |  {DateTime.Now:HH:mm:ss}"
        RaiseEvent ResendRequested(msg.Json, msg.RequiresAck)

        Me.WindowState = FormWindowState.Minimized
    End Sub

    ''' <summary>Returnează indexul unui mesaj în lista curentă a pachetului.</summary>
    Private Function GetSelectedMessageIndex(msg As SentPipeMessage) As Integer
        If _currentPkg Is Nothing Then Return 0
        Return _currentPkg.Messages.IndexOf(msg)
    End Function

    Private Shared Function TryGetCmd(json As String) As String
        Try
            Dim result As String = JObject.Parse(json)("cmd")?.ToString()
            Return If(result, "?")
        Catch
            Return "?"
        End Try
    End Function

    ' =========================================================================
    ' SCHIMBARE FOLDER
    ' =========================================================================
    Private Sub btnChangeFolder_Click(sender As Object, e As EventArgs) Handles btnChangeFolder.Click
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Selectează folderul cu fișierele Resend (.resend.json)"
            fbd.ShowNewFolderButton = True
            If Not String.IsNullOrEmpty(_currentFolder) AndAlso Directory.Exists(_currentFolder) Then
                fbd.SelectedPath = _currentFolder
            End If

            If fbd.ShowDialog() <> DialogResult.OK Then Return

            _currentFolder = fbd.SelectedPath
            RefreshFileList()
        End Using
    End Sub

    ' =========================================================================
    ' ÎNCHIDE
    ' =========================================================================
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
