Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports WorkflowModels
Imports System.Collections.Generic

Public Class GeneralClasses
    Public Class PdfExtractResult
        Public Property Success As Boolean
        Public Property XmlContent As String
        Public Property FileName As String
        Public Property FileDate As DateTime
        Public Property ErrorMessage As String
    End Class

    ''' <summary>
    ''' Reprezintă un pachet de mesaje pentru retrimitere.
    ''' </summary>
    Public Class ResendPackage
        Public Property JobName As String
        Public Property SavedAt As DateTime
        Public Property Messages As New List(Of SentPipeMessage)
    End Class

    ''' <summary>
    ''' Stochează un mesaj pipe trimis efectiv, pentru posibilă retrimitere din HistoryForm.
    ''' </summary>
    Public Class SentPipeMessage
        Public Property Json As String
        Public Property RequiresAck As Boolean
        Public Property SentAt As DateTime = DateTime.Now
    End Class

    Public NotInheritable Class PipeCmd
        ' =========================================================================
        ' 1. COMENZI FIRE-AND-FORGET (Fără ACK - RequiresAck = False)
        ' =========================================================================
        ' Se trimit instant, nu blochează coada, nu așteaptă răspuns.

        Public Shared ReadOnly HELLO As New RobotCommand("HELLO", False)

        ' Progress e critic sa fie False. Altfel, la un upload cu 100 de pasi, 
        ' ai sta dupa 100 de confirmari din Access.
        Public Shared ReadOnly PROGRESS As New RobotCommand("PROGRESS", False)


        ' =========================================================================
        ' 2. COMENZI STOP-AND-WAIT (Cu ACK - RequiresAck = True)
        ' =========================================================================
        ' Se trimit doar după ce comanda anterioară a fost confirmată de VBA.
        ' Garantează ordinea și că VBA a terminat procesarea datelor.

        Public Shared ReadOnly VBNET_HWND As New RobotCommand("VBNET_HWND", True)
        Public Shared ReadOnly CONNECTED As New RobotCommand("CONNECTED", True)
        Public Shared ReadOnly JOB_STARTED As New RobotCommand("JOB_STARTED", True)
        Public Shared ReadOnly JOB_SUCCESS As New RobotCommand("JOB_SUCCESS", True)
        Public Shared ReadOnly JOB_ERROR As New RobotCommand("JOB_ERROR", True)
        Public Shared ReadOnly JOB_STOPPED As New RobotCommand("JOB_STOPPED", True)
        Public Shared ReadOnly WAITING As New RobotCommand("WAITING", True)
        Public Shared ReadOnly CLOSING As New RobotCommand("CLOSING", True)
        Public Shared ReadOnly RECONNECTED As New RobotCommand("RECONNECTED", True)

        Public Shared ReadOnly BUSY As New RobotCommand("BUSY", True)
        Public Shared ReadOnly INFO As New RobotCommand("INFO", True) ' De obicei vrem sa stim ca s-a logat
        Public Shared ReadOnly STANDBY As New RobotCommand("STANDBY", True)

        Public Shared ReadOnly WORKFLOW_SUCCESS As New RobotCommand("WORKFLOW_SUCCESS", True)
        Public Shared ReadOnly WORKFLOW_ERROR As New RobotCommand("WORKFLOW_ERROR", True)

        Public Shared ReadOnly LOG_SAVED As New RobotCommand("LOG_SAVED", False)
    End Class

    Public Class RobotCommand
        ' Numele comenzii (care ajunge in JSON la VBA)
        Public ReadOnly Property Name As String

        ' Daca trebuie sa astepte confirmare (ACK) de la VBA
        Public ReadOnly Property RequiresAck As Boolean

        Public Sub New(name As String, requiresAck As Boolean)
            Me.Name = name
            Me.RequiresAck = requiresAck
        End Sub

        ' Override ToString pentru a pastra compatibilitatea cu codul vechi care facea .ToString()
        Public Overrides Function ToString() As String
            Return Me.Name
        End Function
    End Class
    ''' <summary>
    ''' How the application was launched
    ''' </summary>
    Public Enum LaunchMode
        Manual      ' No arguments - use default folder
        Auto        ' --auto argument - use temp folder
        Specific    ' Specific file path provided
    End Enum

    ''' <summary>
    ''' Startup configuration based on command line arguments
    ''' </summary>
    Public Class StartupConfig
        Public Property WorkflowPath As String = String.Empty
        Public Property DeleteAfterRead As Boolean = False
        Public Property LaunchMode As LaunchMode = LaunchMode.Manual
    End Class

    ''' <summary>
    ''' Element personalizat pentru CheckedListBox
    ''' </summary>
    Public Class CustomCheckedItem
        Public Property Index As Integer = 0
        Public Property Text As String
        Public Property Tag As Object
        Public Property Variables As New Dictionary(Of String, WorkflowVariable)
        Public Overrides Function ToString() As String
            Return Text   ' ce se afișează în listă
        End Function
    End Class

    ''' <summary>
    ''' Configurația unui job pentru robot
    ''' </summary>
    Public Class RobotJob
        Public Property Certificat As String
        Public Property OutputFile As String

        Public Property ShowBrowser As Boolean = True

        Public Property ShowLogs As Boolean = True      ' Dacă e false, ascundem RTB
        Public Property LogToFolder As String = Nothing ' Calea folderului pentru log text
        Public Property JobFolder As String = Nothing  ' Folderul pe care il monitorizează robotul pentru joburi noi
        ' -----------------------

        Public Property Tasks As List(Of RobotTask)
        Public Property ManualPin As Boolean = True
        Public Property JobName As String = ""
    End Class

    ''' <summary>
    ''' O sarcină individuală din cadrul unui job
    ''' </summary>
    Public Class RobotTask
        Public Property Path As String
        Public Property Vars As Dictionary(Of String, String)
        Public Property TaskId As String
        Public Property Receive As Boolean
        Public Property Name As String

    End Class

    ''' <summary>
    ''' Deschide un dialog tip InputBox dar cu mască de parolă.
    ''' </summary>
    Public Shared Function InputBoxPassword(prompt As String, title As String) As String
        Dim form As New Form()
        Dim lbl As New Label()
        Dim txt As New TextBox()
        Dim btnOk As New Button()
        Dim btnCancel As New Button()
        Dim dRes As DialogResult

        ' --- Configurare Fereastră ---
        form.Text = title
        form.ClientSize = New Size(396, 107) ' Dimensiune fixă similară cu InputBox clasic
        form.Controls.AddRange(New Control() {lbl, txt, btnOk, btnCancel})
        form.FormBorderStyle = FormBorderStyle.FixedDialog
        form.StartPosition = FormStartPosition.CenterScreen
        form.MinimizeBox = False
        form.MaximizeBox = False
        form.AcceptButton = btnOk
        form.CancelButton = btnCancel
        form.TopMost = True
        ' --- Configurare Label (Textul întrebării) ---
        lbl.Text = prompt
        lbl.SetBounds(9, 20, 372, 13)
        lbl.AutoSize = True

        ' --- Configurare TextBox (Câmpul de parolă) ---
        txt.SetBounds(12, 36, 372, 20)
        txt.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        ' AICI E SECRETUL:
        txt.UseSystemPasswordChar = True ' Folosește bulinele standard de Windows
        ' Sau poți folosi: txt.PasswordChar = "*"c

        ' --- Configurare Butoane ---
        btnOk.Text = "OK"
        btnOk.DialogResult = DialogResult.OK
        btnOk.SetBounds(228, 72, 75, 23)
        btnOk.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        btnCancel.Text = "Anulează"
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.SetBounds(309, 72, 75, 23)
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        ' --- Afișare ---
        dRes = form.ShowDialog()

        If dRes = DialogResult.OK Then
            Return txt.Text
        Else
            Return String.Empty
        End If
    End Function

    ''' <summary>
    ''' Clasă pentru ascultarea mesajelor Windows (pentru IPC)
    ''' </summary>
    Public Class IPCListener
        Inherits NativeWindow
        Implements IDisposable

        ' Eveniment pentru a notifica Form-ul principal
        Public Event OnMessageReceived(msg As Message)

        Public Sub New()
            ' Cream o fereastra "Message-Only" (invizibila, fara parinte vizual)
            Dim cp As New CreateParams With {
                .Caption = "ForexeBotListener"
            }
            Me.CreateHandle(cp)
        End Sub

        ' Aici interceptam mesajele (exact ca in WndProc din Form)
        Protected Overrides Sub WndProc(ByRef m As Message)
            ' Pasam mesajul mai departe catre KBOT_STANDALONE prin eveniment
            RaiseEvent OnMessageReceived(m)

            ' Lasam Windows-ul sa proceseze restul
            MyBase.WndProc(m)
        End Sub

        Public ReadOnly Property HandleValue As Long
            Get
                Return Me.Handle.ToInt64()
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.DestroyHandle()
        End Sub
    End Class

    Public Class JobHistoryItem
        Public Property Timestamp As DateTime
        Public Property JobName As String
        Public Property InputData As String           ' JSON-ul sau textul primit de la VBA
        Public Property OutputData As New Dictionary(Of String, Object) ' Ce a răspuns robotul
        Public Property Status As String
        Public Property FullLog As New StringBuilder()
        Public Property SentPipeMessages As New List(Of SentPipeMessage) ' ← NOU: replay fidel
    End Class

    ' Un Manager simplu (Singleton) pentru a ține datele în memorie
    ' Manager Singleton pentru a ține datele în memorie
    Public Class JobHistoryManager
        Public Shared ReadOnly History As New List(Of JobHistoryItem)
        Private Shared _currentJob As JobHistoryItem

        ''' <summary>Apelată la începutul unui job nou.</summary>
        Public Shared Sub StartJob(name As String, inputJson As String)
            Dim newItem As New JobHistoryItem With {
            .Timestamp = DateTime.Now,
            .JobName = name,
            .InputData = inputJson,
            .Status = "In Execuție..."
        }
            _currentJob = newItem
            History.Add(newItem)
        End Sub

        ''' <summary>Apelată din RichTextBoxLogger pentru fiecare linie de log.</summary>
        Public Shared Sub AppendLog(message As String)
            If _currentJob IsNot Nothing Then
                SyncLock _currentJob
                    _currentJob.FullLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}")
                End SyncLock
            End If
        End Sub

        ''' <summary>Apelată după fiecare workflow finalizat.</summary>
        Public Shared Sub FinishWorkflow(status As String, outputData As Object)
            _currentJob?.OutputData.TryAdd(status, outputData)
        End Sub

        ''' <summary>
        ''' Înregistrează exact JSON-ul trimis prin pipe în istoricul job-ului curent.
        ''' Apelată din SendMessageToPipe — permite replay fidel din HistoryForm.
        ''' NU se apelează din ResendRawMessage (să nu se acumuleze resend-urile).
        ''' </summary>
        Public Shared Sub RecordSentMessage(json As String, requiresAck As Boolean)
            If _currentJob Is Nothing Then Return
            Try
                Dim msg As New SentPipeMessage With {
                .Json = json,
                .RequiresAck = requiresAck,
                .SentAt = DateTime.Now
            }
                SyncLock _currentJob
                    _currentJob.SentPipeMessages.Add(msg)
                End SyncLock
            Catch
                ' Fail silently — nu blocăm execuția pentru logging
            End Try
        End Sub

        ''' <summary>Marchează job-ul curent ca terminat cu succes.</summary>
        Public Shared Sub FinishJob(status As String)
            If _currentJob IsNot Nothing Then
                _currentJob.Status = status
                _currentJob = Nothing
            End If
        End Sub

        ''' <summary>Marchează job-ul curent ca eșuat.</summary>
        Public Shared Sub FailJob(errorMessage As String)
            If _currentJob IsNot Nothing Then
                _currentJob.Status = "Eroare"
                _currentJob.OutputData.TryAdd("EROARE", errorMessage)
                _currentJob = Nothing
            End If
        End Sub

        Public Shared Sub SaveOutputVariables(vars As Dictionary(Of String, String))
            If _currentJob Is Nothing Then Return
            For Each kvp In vars
                _currentJob.OutputData.TryAdd(kvp.Key, kvp.Value)
            Next
        End Sub
    End Class
End Class
