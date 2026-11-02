Option Strict On
Imports System.IO
Imports System.Text.Json

' SLICE-ME01-07. All Git commands live in the embedded server helper, never on this PC.
Public NotInheritable Class DeploymentPublisher
    Private Sub New()
    End Sub

    Public Shared Sub Publish(k_settings As AdminSettings, k_plan As DeploymentPlan, k_onLine As Action(Of String))
        Try
            Dim k_data As JsonElement = Execute(k_settings, k_plan, If(k_plan.GitOnly, "commit_debug", "publish"), k_onLine)
            Dim k_commit As String = k_data.GetProperty("commit").GetString()
            If k_data.GetProperty("count").GetInt32() <> k_plan.Items.Count OrElse
                k_data.GetProperty("git_count").GetInt32() <> k_plan.GitCount OrElse
                k_data.GetProperty("action").GetString() <> If(k_plan.GitOnly, "commit_debug", "publish") OrElse
                (k_plan.GitCount > 0 AndAlso String.IsNullOrWhiteSpace(k_commit)) Then
                Throw New ApplicationException("Confirmare incompletă de la server; nomenclatorul este păstrat.")
            End If
            If k_plan.GitCount > 0 Then
                k_onLine?.Invoke(If(k_plan.GitOnly, "Debugging finalizat fără reîncărcare. Commit server: ", "Publicare reușită. Commit server: ") & k_commit)
                DeploymentRegistry.RemoveCompleted(k_plan)
            Else
                k_onLine?.Invoke("Publicare debugging reușită, fără Git. Fișierele rămân în evidența serverului pentru finalizare.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentPublisher.Publish", ex)
            Throw
        End Try
    End Sub

    Public Shared Function ListDebug(k_settings As AdminSettings, k_onLine As Action(Of String)) As List(Of DebugDeployment)
        Try
            Dim k_data As JsonElement = Execute(k_settings, Nothing, "list_debug", k_onLine)
            Return JsonSerializer.Deserialize(Of List(Of DebugDeployment))(k_data.GetProperty("debug_files").GetRawText())
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentPublisher.ListDebug", ex)
            Throw
        End Try
    End Function

    Private Shared Function Execute(k_settings As AdminSettings, k_plan As DeploymentPlan, k_action As String,
                                    k_onLine As Action(Of String)) As JsonElement
        Dim k_stage As String = String.Empty
        Try
            Using k_ssh As New SshSession(k_settings)
                k_ssh.Connect()
                ' mktemp creates an owner-only directory, outside the published tree.
                Dim k_created As SshResult = k_ssh.Run("umask 077; mktemp -d /tmp/kbot-publish-XXXXXXXXXXXX")
                If k_created.ExitStatus <> 0 Then Throw New ApplicationException("Nu pot crea zona temporară pe server: " & k_created.StdErr)
                k_stage = k_created.StdOut.Trim()
                If Not System.Text.RegularExpressions.Regex.IsMatch(k_stage, "^/tmp/kbot-publish-[A-Za-z0-9]{12}$") Then Throw New ApplicationException("Director temporar neașteptat pe server.")
                RunLog.Write("Publicare: zona temporară " & k_stage)
                Dim k_manifestFiles As New List(Of Object)
                Using k_sftp As New SftpSession(k_settings)
                    k_sftp.Connect()
                    If k_plan IsNot Nothing Then
                        For k_i As Integer = 0 To k_plan.Items.Count - 1
                            Dim k_item As DeploymentItem = k_plan.Items(k_i)
                            If Not k_plan.GitOnly Then
                                k_onLine?.Invoke("Pregătire: " & k_item.PushFile.RelativePath)
                                k_sftp.UploadBytes(k_item.Bytes, k_stage & "/" & k_i.ToString(Globalization.CultureInfo.InvariantCulture) & ".data")
                            End If
                            k_manifestFiles.Add(New With {.path = k_item.PushFile.RelativePath, .sha256 = k_item.Hash,
                                .include_git = k_item.Records.Count > 0,
                                .mtime = If(k_plan.GitOnly, 0.0, New DateTimeOffset(k_item.MtimeUtc).ToUnixTimeMilliseconds() / 1000.0)})
                        Next
                    End If
                    Dim k_manifest As New With {.action = k_action, .files = k_manifestFiles,
                        .message = If(k_plan Is Nothing, String.Empty, k_plan.Message), .preserve_mtime = k_settings.PreserveMTime}
                    k_sftp.UploadBytes(JsonSerializer.SerializeToUtf8Bytes(k_manifest), k_stage & "/manifest.json")
                    Using k_stream As Stream = GetType(DeploymentPublisher).Assembly.GetManifestResourceStream("publish.server.py")
                        If k_stream Is Nothing Then Throw New ApplicationException("Lipsește componenta de publicare pe server.")
                        Using k_buffer As New MemoryStream()
                            k_stream.CopyTo(k_buffer)
                            k_sftp.UploadBytes(k_buffer.ToArray(), k_stage & "/publish.py")
                        End Using
                    End Using
                End Using
                If k_plan IsNot Nothing Then
                    k_onLine?.Invoke(If(k_plan.GitOnly, "Finalizare debugging: numai Git pe server...",
                        If(k_plan.GitCount = 0, "Publicare debugging fără Git...", "Salvare Git pe server pentru fișierele cu informații Git...")))
                End If
                Dim k_result As SshResult = k_ssh.Run("PYTHONIOENCODING=utf-8 PYTHONUNBUFFERED=1 " & AdminClient.Quote(k_settings.RemotePython) &
                    " " & AdminClient.Quote(k_stage & "/publish.py") & " " & AdminClient.Quote(k_settings.RemoteRoot))
                Dim k_receipt As String = Nothing
                For Each k_line As String In k_result.StdOut.Split(ControlChars.Lf)
                    If k_line.StartsWith("KBOT_PUBLISH_RESULT=", StringComparison.Ordinal) Then
                        k_receipt = k_line.Substring("KBOT_PUBLISH_RESULT=".Length)
                    ElseIf Not String.IsNullOrWhiteSpace(k_line) Then
                        k_onLine?.Invoke(k_line.TrimEnd(ControlChars.Cr))
                    End If
                Next
                If k_result.ExitStatus <> 0 OrElse k_receipt Is Nothing Then
                    Throw New ApplicationException("Publicare oprită. Nomenclatorul este păstrat. " & k_result.StdErr & vbCrLf & "Zona temporară: " & k_stage)
                End If
                Dim k_response As JsonElement
                Using k_json As JsonDocument = JsonDocument.Parse(k_receipt)
                    Dim k_data As JsonElement = k_json.RootElement
                    If Not k_data.GetProperty("ok").GetBoolean() Then
                        Throw New ApplicationException("Confirmare incompletă de la server; nomenclatorul este păstrat.")
                    End If
                    k_response = k_data.Clone()
                End Using
                ' Only our mktemp directory, after a positive server receipt. Failed batches remain for recovery.
                Try
                    Dim k_cleanup As SshResult = k_ssh.Run("rm -rf -- " & AdminClient.Quote(k_stage))
                    If k_cleanup.ExitStatus <> 0 Then k_onLine?.Invoke("Publicare reușită; zona temporară nu a putut fi ștearsă: " & k_stage)
                Catch ex As Exception
                    GlobalErrorLog.Write("DeploymentPublisher.Cleanup", ex)
                    k_onLine?.Invoke("Publicare reușită; curățarea zonei temporare a eșuat: " & k_stage & " — " & ex.Message)
                End Try
                Return k_response
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("DeploymentPublisher.Execute", ex)
            Throw New ApplicationException(ex.Message & If(k_stage = String.Empty, String.Empty, vbCrLf & "Zona temporară: " & k_stage), ex)
        End Try
    End Function
End Class
