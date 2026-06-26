Imports System.IO
Imports System.Windows.Forms
Imports GeneralClasses
Imports Newtonsoft.Json

Partial Public Class KBOT_IPC

    Private Const ResendSubfolderName As String = "Resend"
    Private Const ResendExtension As String = ".resend.json"

    ' Singleton — refolosim aceeași fereastră dacă e deja deschisă
    Private _resendFormInstance As ResendForm = Nothing

    ' =========================================================
    ' SAVE — manual din systray (neschimbat)
    ' =========================================================

    ''' <summary>
    ''' Salvează mesajele pipe ale ultimului job finalizat în folderul fix Resend.
    ''' Apelat manual din systray.
    ''' </summary>
    Friend Sub ManualSaveResendPackage(Optional AutoSave As Boolean = False)
        Dim lastJob As JobHistoryItem = JobHistoryManager.History.
            AsEnumerable().Reverse().
            FirstOrDefault(Function(j) j.SentPipeMessages IsNot Nothing AndAlso
                                       j.SentPipeMessages.Count > 0)

        If Not AutoSave Then
            If lastJob Is Nothing Then
                MessageBox.Show("Nu există mesaje pipe salvate în sesiunea curentă.",
                                "Resend — Nimic de salvat",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If String.IsNullOrEmpty(_jobFolderPath) Then
                MessageBox.Show("Nu este configurat un folder pentru job. Nu pot salva.",
                                "Resend — Folder lipsă",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
        Else
            If lastJob Is Nothing Or String.IsNullOrEmpty(_jobFolderPath) Then
                _logger.LogError("Nu s-a putut salva ultimul mesaj resend.")
                Return
            End If
        End If

        Try
            Dim resendDir As String = Path.Combine(_jobFolderPath, ResendSubfolderName)
            If Not Directory.Exists(resendDir) Then Directory.CreateDirectory(resendDir)

            Dim defaultName As String = SanitizeFileName(lastJob.JobName)
            Dim fileName As String

            If AutoSave Then
                ' Salvare automată — timestamp, fără dialog
                fileName = $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmmss}{ResendExtension}"
            Else
                ' Salvare manuală — user tastează numele
                Using box As New CustomInputBox("Salvare Resend", "Nume fișier (fără extensie):", defaultName)
                    If box.ShowDialog() <> DialogResult.OK Then Return
                    Dim userInput As String = box.UserInput.Trim()
                    If String.IsNullOrEmpty(userInput) Then Return
                    fileName = $"{SanitizeFileName(userInput)}{ResendExtension}"
                End Using
            End If

            Dim filePath As String = Path.Combine(resendDir, fileName)

            Dim pkg As New ResendPackage With {
                .JobName = lastJob.JobName,
                .SavedAt = DateTime.Now,
                .Messages = lastJob.SentPipeMessages
            }

            File.WriteAllText(filePath,
                              JsonConvert.SerializeObject(pkg, Formatting.Indented),
                              System.Text.Encoding.UTF8)

            _logger?.LogSuccess($"[RESEND] Pachet salvat: {fileName}")

            If Not AutoSave Then
                MessageBox.Show($"Salvat în:{Environment.NewLine}{filePath}",
                            "Resend — Salvat cu succes",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            _logger?.LogWarning($"[RESEND] Eroare la salvare: {ex.Message}")
            MessageBox.Show($"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' =========================================================
    ' LOAD — deschide ResendForm standalone (nou, folder-based)
    ' =========================================================

    ''' <summary>
    ''' Deschide Resend Manager — formular standalone cu lista fișierelor din folderul Resend.
    ''' Dacă fereastra e deja deschisă, o aduce în față (singleton).
    ''' Apelat din systray (fostul LoadResendPackageFromDialog).
    ''' </summary>
    Friend Sub LoadResendPackageFromDialog()
        ' Singleton: reutilizăm instanța dacă există și nu a fost închisă
        If _resendFormInstance IsNot Nothing AndAlso Not _resendFormInstance.IsDisposed Then
            _resendFormInstance.WindowState = FormWindowState.Normal
            _resendFormInstance.BringToFront()
            Return
        End If

        Dim resendFolder As String = If(
            Not String.IsNullOrEmpty(_jobFolderPath),
            Path.Combine(_jobFolderPath, ResendSubfolderName),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop))

        _resendFormInstance = New ResendForm(resendFolder)

        AddHandler _resendFormInstance.ResendRequested,
            Sub(json As String, requiresAck As Boolean)
                ResendRawMessage(json, requiresAck)
            End Sub

        ' Curățăm referința la închidere pentru a permite redeschiderea
        AddHandler _resendFormInstance.FormClosed,
            Sub(s As Object, ev As FormClosedEventArgs)
                _resendFormInstance = Nothing
            End Sub

        _logger?.LogInfo($"[RESEND] Resend Manager deschis — folder: {resendFolder}")
        _resendFormInstance.Show()
    End Sub

    ' =========================================================
    ' HELPER
    ' =========================================================
    Private Shared Function SanitizeFileName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "job"
        Dim invalid As Char() = Path.GetInvalidFileNameChars()
        Return New String(name.Select(
            Function(c) If(Array.IndexOf(invalid, c) >= 0, "_"c, c)).ToArray())
    End Function

End Class