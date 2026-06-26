Imports System.IO
Imports System.IO.Pipes
Imports System.Text
Imports GeneralClasses
Imports Newtonsoft.Json

Partial Public Class KBOT_IPC

    Private Function ConnectToPipe() As Boolean
        If _isConnected Then Return True

        Try
            _pipeClient = New NamedPipeClientStream(".", "ForexeBotPipe", PipeDirection.Out)
            _pipeClient.Connect(2000)
            _isConnected = True
            _logger?.LogSuccess("[PIPE] Conectat!")
            Return True

        Catch ex As Exception
            _isConnected = False
            _logger?.LogError($"[PIPE] Eroare Handshake: {ex.Message}")
            If _pipeClient IsNot Nothing Then _pipeClient.Dispose() : _pipeClient = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Trimite un mesaj nou generat prin pipe (calea normală de execuție).
    ''' Înregistrează automat JSON-ul în istoricul job-ului curent pentru Resend.
    ''' </summary>
    Private Sub SendMessageToPipe(cmd As RobotCommand, cTaskId As Integer, msg As String, Optional extraData As Object = Nothing)
        If _logger Is Nothing Then
            Stop
        End If

        _vbaVariables = Nothing

        ' 1. Generăm JSON
        Dim jsonToSend As String = CreateVbaJson(cmd.ToString(), cTaskId, msg, extraData)

        ' 2. Înregistrăm în istoricul job-ului (pentru funcționalitatea Resend din HistoryForm)
        '    NU se face în ResendRawMessage — să nu se acumuleze resend-urile în istoric
        JobHistoryManager.RecordSentMessage(jsonToSend, cmd.RequiresAck)

        ' 3. Cream obiectul de coadă
        Dim qMsg As New QueuedMessage With {
            .JsonContent = jsonToSend,
            .RequiresAck = cmd.RequiresAck
        }

        ' 4. Îl punem în coadă
        SyncLock _queueLock
            _outgoingQueue.Enqueue(qMsg)
        End SyncLock

        ' 5. Trigger procesare
        If Me.InvokeRequired Then
            Me.BeginInvoke(Sub() TrySendNextMessage())
        Else
            TrySendNextMessage()
        End If
    End Sub

    ''' <summary>
    ''' Retrimite un mesaj pipe exact ca stocat (replay fidel), fără a-l înregistra din nou în istoric.
    ''' Apelat din HistoryForm.ResendRequested — utilizatorul a ales manual mesajul.
    ''' </summary>
    Public Sub ResendRawMessage(json As String, requiresAck As Boolean)
        If String.IsNullOrEmpty(json) Then Return

        ' Injectăm flag resend în JSON înainte de trimitere
        Dim finalJson As String = json
        Try
            Dim obj = Newtonsoft.Json.Linq.JObject.Parse(json)
            obj("resend") = True
            finalJson = obj.ToString(Newtonsoft.Json.Formatting.None)
        Catch
            ' Dacă parse-ul pică, trimitem JSON-ul original fără flag
            finalJson = json
        End Try

        Dim qMsg As New QueuedMessage With {
        .JsonContent = finalJson,
        .RequiresAck = requiresAck
    }

        SyncLock _queueLock
            _outgoingQueue.Enqueue(qMsg)
        End SyncLock

        _logger?.LogInfo($"[PIPE] Resend manual enqueued (resend=true, requiresAck={requiresAck})")

        If Me.InvokeRequired Then
            Me.BeginInvoke(Sub() TrySendNextMessage())
        Else
            TrySendNextMessage()
        End If
    End Sub

    Private Sub TrySendNextMessage()
        If _waitingForVbaAck Then Return

        Dim currentMsg As QueuedMessage = Nothing

        SyncLock _queueLock
            If _outgoingQueue.Count > 0 Then
                currentMsg = _outgoingQueue.Peek()
            End If
        End SyncLock

        If currentMsg Is Nothing Then Return

        If Not _isServer Then Return
        If Not _isConnected OrElse _pipeClient Is Nothing Then
            ConnectToPipe()
            If Not _isConnected Then Return
        End If

        Try
            Dim buffer() As Byte = Encoding.Unicode.GetBytes(currentMsg.JsonContent)
            _pipeClient.Write(buffer, 0, buffer.Length)

            SyncLock _queueLock
                _outgoingQueue.Dequeue()
            End SyncLock

            If currentMsg.RequiresAck Then
                _waitingForVbaAck = True
            Else
                _waitingForVbaAck = False
                TrySendNextMessage()
            End If

        Catch ex As IOException
            _logger?.LogWarning($"[PIPE] Broken Pipe ({ex.Message}).")
            _isConnected = False
            _pipeClient.Dispose() : _pipeClient = Nothing
            _waitingForVbaAck = False
        Catch ex As Exception
            _logger?.LogError($"[PIPE] Eroare trimitere: {ex.Message}")
            _waitingForVbaAck = False
        End Try
    End Sub

    Public Function CreateVbaJson(commandName As String, cTaskId As Integer, Optional message As String = "", Optional extraData As Object = Nothing) As String

        ' 1. Structura standard
        Dim root As New Dictionary(Of String, Object) From {
        {"cmd", commandName},
        {"taskid", cTaskId},
        {"msg", message},
        {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
    }

        ' 2. LOGICA DE TIPIZARE (Primitive vs Complex)
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

        ' 3. Salvare pentru istoric (_vbaVariables folosit de JobHistoryManager.FinishWorkflow)
        _vbaVariables = root

        ' 4. Serializare
        Return JsonConvert.SerializeObject(root)
    End Function

End Class