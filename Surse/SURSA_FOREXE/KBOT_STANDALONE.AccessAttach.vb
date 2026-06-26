Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.IO.Pipes
Imports System.Text
Imports System.Windows.Forms
Imports GeneralClasses
Imports Newtonsoft.Json

Partial Public Class KBOT_STANDALONE

    ' =========================================================================
    ' CONSTANTE
    ' =========================================================================
    Private Const SA_PIPE_NAME As String = "ForexeBotPipe"
    Private Const SA_PIPE_CONNECT_TIMEOUT_MS As Integer = 2000
    Private Const SA_RESEND_EXTENSION As String = ".resend.json"
    Private Const SA_RESEND_SUBFOLDER As String = "Resend"

    ' =========================================================================
    ' CÂMPURI — Access Attach
    ' =========================================================================
    Private _saPipeClient As NamedPipeClientStream = Nothing
    Private _saIsConnected As Boolean = False
    Private _saAttachedProcessId As Integer = 0

    Private _saOutgoingQueue As New Queue(Of SaQueuedMessage)()
    Private ReadOnly _saQueueLock As New Object()

    ' Listă mesaje trimise în sesiunea curentă — pentru SA_SaveResendPackage
    Private _saSentMessages As New List(Of SentPipeMessage)()

    Private Class SaQueuedMessage
        Public Property JsonContent As String
        Public Property RequiresAck As Boolean   ' stocat pentru referință, nu blochează coada (no ACK receive)
    End Class

    ' =========================================================================
    ' CLICK btnConectAccess
    ' (butonul e adăugat manual de user în Designer — declarat acolo ca Friend WithEvents)
    ' =========================================================================
    Private Sub btnConectAccess_Click(sender As Object, e As EventArgs) Handles btnConectAccess.Click

        ' ── Dacă suntem deja conectați → meniu deconectare ──────────
        If _saIsConnected Then
            Dim ctxDc As New ContextMenuStrip()
            ctxDc.Font = New Font("Segoe UI", 9.5F)

            Dim itemDc As New ToolStripMenuItem(
                $"❌  Deconectează (PID {_saAttachedProcessId})")
            AddHandler itemDc.Click, Sub(s2, e2) SA_Disconnect()
            ctxDc.Items.Add(itemDc)

            ctxDc.Show(btnConectAccess, New Point(0, btnConectAccess.Height))
            Return
        End If

        ' ── Enumerăm toate instanțele MSACCESS ──────────────────────
        Dim procs() As Process = Process.GetProcessesByName("MSACCESS")

        If procs.Length = 0 Then
            MessageBox.Show(
                "Nicio instanță Microsoft Access găsită în execuție.",
                "Access Attach",
                MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' ── ContextMenuStrip inline — fără form separat ──────────────
        Dim ctx As New ContextMenuStrip()
        ctx.Font = New Font("Segoe UI", 9.5F)

        For Each proc In procs
            Dim title As String = If(
                String.IsNullOrWhiteSpace(proc.MainWindowTitle),
                $"Access PID {proc.Id}",
                proc.MainWindowTitle)

            Dim item As New ToolStripMenuItem($"[PID {proc.Id}]   {title}")
            Dim captured As Process = proc   ' capturăm pentru closure — altfel toți items au ultimul proc

            AddHandler item.Click, Sub(s2, e2) SA_AttachToProcess(captured)
            ctx.Items.Add(item)
        Next

        ctx.Show(btnConectAccess, New Point(0, btnConectAccess.Height))
    End Sub

    ' =========================================================================
    ' ATAŞARE — conectare pipe + handshake
    ' =========================================================================
    Private Sub SA_AttachToProcess(proc As Process)
        ' Curăță orice conexiune anterioară
        SA_Disconnect(silent:=True)

        _logger?.LogInfo($"[SA] Încerc atașare la Access PID {proc.Id} — «{proc.MainWindowTitle}»")
        lblStatus.Text = $"Conectare la Access PID {proc.Id}..."

        ' ── 1. Conectare pipe ───────────────────────────────────────
        Try
            _saPipeClient = New NamedPipeClientStream(".", SA_PIPE_NAME, PipeDirection.Out)
            _saPipeClient.Connect(SA_PIPE_CONNECT_TIMEOUT_MS)
            _saIsConnected = True
            _saAttachedProcessId = proc.Id

        Catch ex As TimeoutException
            SA_CleanupPipe()
            _logger?.LogWarning($"[SA] Timeout conectare pipe (>{SA_PIPE_CONNECT_TIMEOUT_MS} ms).")
            lblStatus.Text = "Conectare eșuată — timeout."
            MessageBox.Show(
                $"Pipe-ul «{SA_PIPE_NAME}» nu răspunde.{Environment.NewLine}" &
                "Asigurați-vă că VBA ascultă (modulul pipe din Access este activ).",
                "Access Attach — Timeout",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return

        Catch ex As Exception
            SA_CleanupPipe()
            _logger?.LogWarning($"[SA] Eroare conectare pipe: {ex.Message}")
            lblStatus.Text = "Conectare eșuată."
            MessageBox.Show(
                $"Nu am putut conecta la pipe-ul Access (PID {proc.Id}).{Environment.NewLine}{Environment.NewLine}" &
                ex.Message,
                "Access Attach — Eroare",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try

        _logger?.LogSuccess($"[SA] Pipe «{SA_PIPE_NAME}» conectat la PID {proc.Id}.")

        ' ── 2. Handshake — trimitem HWND-ul formularului nostru ─────
        '    Identic cu KBOT_IPC: PipeCmd.VBNET_HWND, taskId=0, payload=hwnd ca string.
        '    VBA stochează HWND-ul pentru a putea trimite WM dacă dorește ulterior.
        SA_SendMessageToPipe(PipeCmd.VBNET_HWND, 0, Me.Handle.ToString(), Nothing)

        ' ── 3. Update UI ─────────────────────────────────────────────
        SA_UpdateButtonState()
        lblStatus.Text = $"✓ Conectat la Access PID {proc.Id} — «{proc.MainWindowTitle}»"
        _logger?.LogSuccess("[SA] Handshake trimis. STANDALONE atașat la Access.")
    End Sub

    ' =========================================================================
    ' DECONECTARE
    ' =========================================================================

    ''' <summary>
    ''' Deconectează pipe-ul și resetează starea.
    ''' silent=True → fără log și fără update lblStatus (pentru cleanup intern).
    ''' </summary>
    Public Sub SA_Disconnect(Optional silent As Boolean = False)
        Dim oldPid As Integer = _saAttachedProcessId

        SA_CleanupPipe()

        SyncLock _saQueueLock
            _saOutgoingQueue.Clear()
            _saSentMessages.Clear()   ' resetăm istoricul sesiunii la deconectare
        End SyncLock

        _saIsConnected = False
        _saAttachedProcessId = 0
        SA_UpdateButtonState()

        If Not silent Then
            _logger?.LogInfo($"[SA] Deconectat de la Access PID {oldPid}.")
            lblStatus.Text = "Deconectat de la Access."
        End If
    End Sub

    Private Sub SA_CleanupPipe()
        If _saPipeClient IsNot Nothing Then
            Try
                If _saPipeClient.IsConnected Then _saPipeClient.Close()
                _saPipeClient.Dispose()
            Catch
                ' Ignorăm — oricum aruncăm referința
            End Try
            _saPipeClient = Nothing
        End If
    End Sub

    ' =========================================================================
    ' API PUBLIC — apelabil din orice altă parte a STANDALONE
    ' =========================================================================

    ''' <summary>
    ''' Trimite un mesaj pipe la Access prin coada ordonată.
    ''' Coada garantează trimitere secvențială fără înghesuire în VBA.
    ''' Fără blocare pe ACK — STANDALONE nu ascultă WM înapoi.
    ''' </summary>
    Friend Sub SA_SendMessageToPipe(cmd As RobotCommand,
                                    taskId As Integer,
                                    msg As String,
                                    Optional extraData As Object = Nothing)
        If Not _saIsConnected Then
            _logger?.LogWarning("[SA] SendMessage ignorat — nu suntem conectați la Access.")
            Return
        End If

        Dim json As String = SA_CreateJson(cmd.ToString(), taskId, msg, extraData)

        Dim qMsg As New SaQueuedMessage With {
            .JsonContent = json,
            .RequiresAck = cmd.RequiresAck
        }

        SyncLock _saQueueLock
            _saOutgoingQueue.Enqueue(qMsg)
        End SyncLock

        If Me.InvokeRequired Then
            Me.BeginInvoke(Sub() SA_TrySendNext())
        Else
            SA_TrySendNext()
        End If
    End Sub

    ' =========================================================================
    ' PROCESARE COADĂ — drenare completă la fiecare apel
    ' =========================================================================
    Private Sub SA_TrySendNext()
        ' Nu blocăm pe ACK — golim coada secvențial, sincron pe UI thread.
        ' Fiecare Write pe NamedPipeClientStream e sincron → ordinea e garantată.
        Do
            Dim current As SaQueuedMessage = Nothing
            SyncLock _saQueueLock
                If _saOutgoingQueue.Count > 0 Then
                    current = _saOutgoingQueue.Peek()
                End If
            End SyncLock

            If current Is Nothing Then Return

            If Not _saIsConnected OrElse _saPipeClient Is Nothing Then
                _logger?.LogWarning("[SA] Pipe indisponibil. Coadă golită.")
                SyncLock _saQueueLock
                    _saOutgoingQueue.Clear()
                End SyncLock
                SA_Disconnect(silent:=True)
                Return
            End If

            Try
                Dim buffer() As Byte = Encoding.Unicode.GetBytes(current.JsonContent)
                _saPipeClient.Write(buffer, 0, buffer.Length)

                SyncLock _saQueueLock
                    _saOutgoingQueue.Dequeue()
                    ' Înregistrăm în istoricul sesiunii — pentru SA_SaveResendPackage
                    _saSentMessages.Add(New SentPipeMessage With {
                        .Json = current.JsonContent,
                        .RequiresAck = current.RequiresAck,
                        .SentAt = DateTime.Now
                    })
                End SyncLock

                _logger?.LogInfo($"[SA] ► {current.JsonContent.Substring(0, Math.Min(120, current.JsonContent.Length))}")

            Catch ex As IOException
                ' Pipe rupt — deconectare forțată, coadă golită
                _logger?.LogWarning($"[SA] Pipe rupt: {ex.Message}. Deconectare forțată.")
                SyncLock _saQueueLock
                    _saOutgoingQueue.Clear()
                End SyncLock
                SA_Disconnect(silent:=True)
                lblStatus.Text = "Pipe deconectat (eroare IO)."
                SA_UpdateButtonState()
                Return

            Catch ex As Exception
                ' Eroare pe mesajul curent — eliminăm și continuăm cu restul cozii
                _logger?.LogWarning($"[SA] Eroare trimitere mesaj: {ex.Message}. Continuu cu restul cozii.")
                SyncLock _saQueueLock
                    _saOutgoingQueue.Dequeue()
                End SyncLock
            End Try
        Loop
    End Sub

    ' =========================================================================
    ' JSON BUILDER
    ' Structură identică cu CreateVbaJson din KBOT_IPC.Pipe.vb
    ' =========================================================================
    Private Shared Function SA_CreateJson(commandName As String,
                                          taskId As Integer,
                                          Optional msg As String = "",
                                          Optional extraData As Object = Nothing) As String
        Dim root As New Dictionary(Of String, Object) From {
            {"cmd", commandName},
            {"taskid", taskId},
            {"msg", msg},
            {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
        }

        If extraData IsNot Nothing Then
            Dim t As Type = extraData.GetType()
            If t.IsPrimitive OrElse
               t Is GetType(String) OrElse
               t Is GetType(Decimal) OrElse
               t Is GetType(DateTime) OrElse
               t Is GetType(DateTimeOffset) OrElse
               t Is GetType(TimeSpan) OrElse
               t.IsEnum Then
                root.Add("extrastring", extraData)
            Else
                root.Add("extra", extraData)
            End If
        End If

        Return JsonConvert.SerializeObject(root)
    End Function

    ' =========================================================================
    ' UI — starea butonului reflectă conexiunea
    ' =========================================================================
    Private Sub SA_UpdateButtonState()
        If Not Me.IsHandleCreated Then Return
        If Me.InvokeRequired Then
            Me.Invoke(Sub() SA_UpdateButtonState())
            Return
        End If

        If _saIsConnected Then
            btnConectAccess.Text = $"🟢"
            btnConectAccess.BackColor = Color.FromArgb(30, 80, 30)
            btnConectAccess.ForeColor = Color.FromArgb(180, 255, 180)
            btnConectAccess.FlatStyle = FlatStyle.Flat

            btnSendMessage.Enabled = True

        Else
            btnConectAccess.Text = "🔗"
            btnConectAccess.UseVisualStyleBackColor = True
            btnConectAccess.ForeColor = SystemColors.ControlText
            btnConectAccess.FlatStyle = FlatStyle.Standard

            btnSendMessage.Enabled = False
        End If
    End Sub

    ' =========================================================================
    ' SAVE RESEND — buton btnSaveSendAction
    ' Comportament identic cu ManualSaveResendPackage din KBOT_IPC.Resend.vb,
    ' cu SaveFileDialog în loc de folder fix (STANDALONE nu are _jobFolderPath).
    ' =========================================================================
    Private Sub btnSaveSendAction_Click(sender As Object, e As EventArgs) Handles btnSaveSendAction.Click
        SA_SaveResendPackage()
    End Sub

    Friend Sub SA_SaveResendPackage()
        Dim snapshot As List(Of SentPipeMessage)
        SyncLock _saQueueLock
            snapshot = New List(Of SentPipeMessage)(_saSentMessages)
        End SyncLock

        If snapshot.Count = 0 Then
            MessageBox.Show(
                "Nu există mesaje pipe trimise în sesiunea curentă.",
                "Resend — Nimic de salvat",
                MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Sugerăm numele fișierului bazat pe PID-ul atașat sau timestamp
        Dim suggestedName As String
        If _saAttachedProcessId > 0 Then
            suggestedName = $"STANDALONE_PID{_saAttachedProcessId}_{DateTime.Now:yyyyMMdd_HHmmss}{SA_RESEND_EXTENSION}"
        Else
            suggestedName = $"STANDALONE_{DateTime.Now:yyyyMMdd_HHmmss}{SA_RESEND_EXTENSION}"
        End If

        ' Director implicit: subfolder Resend lângă folderul curent de workflow-uri
        Dim defaultDir As String
        If Not String.IsNullOrEmpty(_currentWflFolder) Then
            defaultDir = Path.Combine(Path.GetDirectoryName(_currentWflFolder), SA_RESEND_SUBFOLDER)
        Else
            defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        End If

        Try
            If Not Directory.Exists(defaultDir) Then Directory.CreateDirectory(defaultDir)
        Catch
            defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        End Try

        Using sfd As New SaveFileDialog()
            sfd.Title = "Salvează pachet Resend"
            sfd.Filter = $"Resend Package (*{SA_RESEND_EXTENSION})|*{SA_RESEND_EXTENSION}|Toate fișierele (*.*)|*.*"
            sfd.FilterIndex = 1
            sfd.FileName = suggestedName
            sfd.InitialDirectory = defaultDir

            If sfd.ShowDialog() <> DialogResult.OK Then Return

            Try
                Dim pkg As New ResendPackage With {
                    .JobName = $"STANDALONE (PID {_saAttachedProcessId})",
                    .SavedAt = DateTime.Now,
                    .Messages = snapshot
                }

                File.WriteAllText(
                    sfd.FileName,
                    JsonConvert.SerializeObject(pkg, Formatting.Indented),
                    System.Text.Encoding.UTF8)

                _logger?.LogSuccess($"[SA] Pachet Resend salvat: {Path.GetFileName(sfd.FileName)}")

                MessageBox.Show(
                    $"Salvat în:{Environment.NewLine}{sfd.FileName}",
                    "Resend — Salvat cu succes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)

            Catch ex As Exception
                _logger?.LogWarning($"[SA] Eroare la salvare pachet Resend: {ex.Message}")
                MessageBox.Show(
                    $"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    ' =========================================================================
    ' RESEND — buton btnSendMessage
    ' Deschide ResendForm cu folderul Resend curent.
    ' ResendRequested → SA_ResendRawMessage (cu resend=true injectat, ca în KBOT_IPC)
    ' =========================================================================
    Private Sub btnSendMessage_Click(sender As Object, e As EventArgs) Handles btnSendMessage.Click
        If Not _saIsConnected Then
            MessageBox.Show(
                "Nu ești conectat la Access. Apasă «🔌 Conectare Access» mai întâi.",
                "Resend — Neconectat",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Același folder ca la salvare
        Dim resendFolder As String
        If Not String.IsNullOrEmpty(_currentWflFolder) Then
            resendFolder = Path.Combine(Path.GetDirectoryName(_currentWflFolder), SA_RESEND_SUBFOLDER)
        Else
            resendFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        End If

        Dim rForm As New ResendForm(resendFolder)
        AddHandler rForm.ResendRequested,
            Sub(json As String, requiresAck As Boolean)
                SA_ResendRawMessage(json, requiresAck)
            End Sub
        rForm.Show()
    End Sub

    ''' <summary>
    ''' Retrimite un mesaj stocat prin pipe-ul SA.
    ''' Injectează "resend":true în JSON — identic cu ResendRawMessage din KBOT_IPC.Pipe.vb.
    ''' Nu înregistrează în _saSentMessages (să nu se acumuleze resend-urile).
    ''' </summary>
    Private Sub SA_ResendRawMessage(json As String, requiresAck As Boolean)
        If String.IsNullOrEmpty(json) Then Return

        Dim finalJson As String = json
        Try
            Dim obj = Newtonsoft.Json.Linq.JObject.Parse(json)
            obj("resend") = True
            finalJson = obj.ToString(Newtonsoft.Json.Formatting.None)
        Catch
            ' Dacă parse-ul pică, trimitem JSON-ul original fără flag
            finalJson = json
        End Try

        Dim qMsg As New SaQueuedMessage With {
            .JsonContent = finalJson,
            .RequiresAck = requiresAck
        }

        SyncLock _saQueueLock
            _saOutgoingQueue.Enqueue(qMsg)
        End SyncLock

        _logger?.LogInfo($"[SA] Resend enqueued (resend=true, requiresAck={requiresAck})")

        If Me.InvokeRequired Then
            Me.BeginInvoke(Sub() SA_TrySendNext())
        Else
            SA_TrySendNext()
        End If
    End Sub

    ' =========================================================================
    ' CLEANUP LA ÎNCHIDERE FORM
    ' VB.NET permite mai mulți handleri pentru același eveniment pe partial class-uri
    ' =========================================================================
    Private Sub SA_OnFormClosing(sender As Object, e As FormClosingEventArgs) _
        Handles MyBase.FormClosing
        SA_Disconnect(silent:=True)
    End Sub

End Class