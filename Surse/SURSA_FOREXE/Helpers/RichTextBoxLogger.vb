Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports GeneralClasses

''' <summary>
''' Log levels with associated colors
''' </summary>
Public Enum LogLevel
    Info        ' Dark Gray
    Action      ' Blue
    Success     ' Green
    Warning     ' Orange
    [Error]     ' Red
    Debug       ' Gray
    Normal      ' Black
End Enum

''' <summary>
''' Thread-safe logger for RichTextBox with colored output, File Support AND History Memory
''' </summary>
Public Class RichTextBoxLogger
    Private ReadOnly _richTextBox As RichTextBox

    ' Obiect pentru blocarea accesului la fișier între thread-uri
    Private Shared ReadOnly _fileLock As New Object()

    ' --- PROPRIETĂȚI ---
    ''' <summary>
    ''' Dacă este True, scrie în fereastra aplicației. Dacă False, ignoră UI-ul.
    ''' </summary>
    Public Property EnableUI As Boolean = True

    ''' <summary>
    ''' Calea completă către fișierul de log (ex: C:\Logs\Log.txt). 
    ''' Dacă este Nothing sau gol, nu se scrie în fișier.
    ''' </summary>
    Public Property LogFilePath As String = Nothing
    ' ---------------------------------------

    ' Colors for dark mode (bright, visible on dark background)
    Private Shared ReadOnly LogColorsDark As New Dictionary(Of LogLevel, Color) From {
        {LogLevel.Info, Color.FromArgb(180, 180, 180)},
        {LogLevel.Action, Color.FromArgb(100, 160, 255)},
        {LogLevel.Success, Color.FromArgb(80, 210, 80)},
        {LogLevel.Warning, Color.FromArgb(255, 185, 60)},
        {LogLevel.Error, Color.FromArgb(255, 100, 100)},
        {LogLevel.Debug, Color.FromArgb(130, 130, 130)},
        {LogLevel.Normal, Color.FromArgb(210, 210, 210)}
    }

    ' Colors for light mode (high contrast on white background)
    Private Shared ReadOnly LogColorsLight As New Dictionary(Of LogLevel, Color) From {
        {LogLevel.Info, Color.FromArgb(60, 60, 60)},
        {LogLevel.Action, Color.FromArgb(0, 80, 190)},
        {LogLevel.Success, Color.FromArgb(0, 130, 0)},
        {LogLevel.Warning, Color.FromArgb(180, 90, 0)},
        {LogLevel.Error, Color.FromArgb(180, 0, 0)},
        {LogLevel.Debug, Color.FromArgb(110, 110, 110)},
        {LogLevel.Normal, Color.FromArgb(10, 10, 10)}
    }

    ' Active colors — initialized to light mode, switched via SetColorScheme()
    Private Shared LogColors As New Dictionary(Of LogLevel, Color) From {
        {LogLevel.Info, Color.FromArgb(60, 60, 60)},
        {LogLevel.Action, Color.FromArgb(0, 80, 190)},
        {LogLevel.Success, Color.FromArgb(0, 130, 0)},
        {LogLevel.Warning, Color.FromArgb(180, 90, 0)},
        {LogLevel.Error, Color.FromArgb(180, 0, 0)},
        {LogLevel.Debug, Color.FromArgb(110, 110, 110)},
        {LogLevel.Normal, Color.FromArgb(10, 10, 10)}
    }

    ''' <summary>Comută paleta de culori pentru log între dark și light
    ''' și re-randează imediat toate intrările existente în toate instanțele active.</summary>
    Public Shared Sub SetColorScheme(dark As Boolean)
        ' 1. Actualizează paleta activă
        Dim src = If(dark, LogColorsDark, LogColorsLight)
        For Each kvp In src
            LogColors(kvp.Key) = kvp.Value
        Next
        ' 2. Re-randează toate instanțele active (trimite pe UI thread)
        Dim dead As New List(Of WeakReference(Of RichTextBoxLogger))
        SyncLock _instances
            For Each wr In _instances
                Dim inst As RichTextBoxLogger = Nothing
                If wr.TryGetTarget(inst) Then
                    inst.RefreshDisplay()
                Else
                    dead.Add(wr)
                End If
            Next
            For Each d In dead
                _instances.Remove(d)
            Next
        End SyncLock
    End Sub

    ' Prefixes for each log level
    Private Shared ReadOnly LogPrefixes As New Dictionary(Of LogLevel, String) From {
        {LogLevel.Info, "  "},
        {LogLevel.Action, "► "},
        {LogLevel.Success, "✓ "},
        {LogLevel.Warning, "⚠ "},
        {LogLevel.Error, "✗ "},
        {LogLevel.Debug, "  "},
        {LogLevel.Normal, ""}
    }

    ' ── Registry global de instanțe active (pentru re-render la switch temă) ──
    Private Shared ReadOnly _instances As New List(Of WeakReference(Of RichTextBoxLogger))

    ' ── Buffer per instanță: (nivel, mesaj formatat) — maxim 5000 intrări ──
    Private ReadOnly _buffer As New List(Of (Level As LogLevel, Msg As String))
    Private Const MaxBuffer As Integer = 5000

    Public Sub New(richTextBox As RichTextBox)
        _richTextBox = richTextBox
        SyncLock _instances
            _instances.Add(New WeakReference(Of RichTextBoxLogger)(Me))
        End SyncLock
    End Sub

    ''' <summary>
    ''' Log a message with specified level (Thread-Safe)
    ''' </summary>
    Public Sub Log(message As String, Optional level As LogLevel = LogLevel.Info)
        Dim timestamp = DateTime.Now.ToString("HH:mm:ss")
        Dim prefix = LogPrefixes(level)
        Dim color = LogColors(level)

        ' Mesajul formatat care va apărea în log
        Dim fullMessage = $"[{timestamp}] {prefix}{message}"

        ' Bufferizăm pentru re-render la switch temă
        SyncLock _buffer
            If _buffer.Count >= MaxBuffer Then _buffer.RemoveAt(0)
            _buffer.Add((level, fullMessage))
        End SyncLock

        ' Apelăm funcția internă care decide unde se scrie (UI, Fișier, Istoric)
        WriteInternal(fullMessage, color)
    End Sub

    ''' <summary>
    ''' Log normal message (black)
    ''' </summary>
    Public Sub LogNormal(message As String)
        Log(message, LogLevel.Normal)
    End Sub

    ''' <summary>
    ''' Log info message (black/gray)
    ''' </summary>
    Public Sub LogInfo(message As String)
        Log(message, LogLevel.Info)
    End Sub

    ''' <summary>
    ''' Log action message (blue)
    ''' </summary>
    Public Sub LogAction(message As String)
        Log(message, LogLevel.Action)
    End Sub

    ''' <summary>
    ''' Log success message (green)
    ''' </summary>
    Public Sub LogSuccess(message As String)
        Log(message, LogLevel.Success)
    End Sub

    ''' <summary>
    ''' Log warning message (orange)
    ''' </summary>
    Public Sub LogWarning(message As String)
        Log(message, LogLevel.Warning)
    End Sub

    ''' <summary>
    ''' Log error message (red)
    ''' </summary>
    Public Sub LogError(message As String)
        Log(message, LogLevel.Error)
    End Sub

    ''' <summary>
    ''' Log debug message (gray)
    ''' </summary>
    Public Sub LogDebug(message As String)
        Log(message, LogLevel.Debug)
    End Sub

    ''' <summary>
    ''' Log exception with full details
    ''' </summary>
    Public Sub LogException(ex As Exception, Optional context As String = "")
        Dim message As String
        If String.IsNullOrEmpty(context) Then
            message = $"Excepție: {ex.Message}"
        Else
            message = $"{context}: {ex.Message}"
        End If

        LogError(message)

#If DEBUG Then
        'LogDebug($"Stack trace: {ex.StackTrace}")
#End If
    End Sub

    ''' <summary>
    ''' Clear all log content (Thread-Safe). Clears UI, buffer and optionally deletes file.
    ''' </summary>
    Public Sub Clear()
        ' 1. Curățare UI (Dacă e activat)
        If EnableUI Then
            If _richTextBox.IsDisposed OrElse _richTextBox.Disposing Then Return

            If _richTextBox.InvokeRequired Then
                _richTextBox.Invoke(Sub() ClearUI())
            Else
                ClearUI()
            End If
        End If

        ' 2. Curățare Fișier (Opțional - ștergem fișierul vechi dacă există)
        If Not String.IsNullOrEmpty(LogFilePath) Then
            Try
                SyncLock _fileLock
                    If File.Exists(LogFilePath) Then
                        File.Delete(LogFilePath)
                    End If
                End SyncLock
            Catch
                ' Ignorăm erorile de fișier la ștergere
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Emergency dump: Salvează log-ul curent în folder-ul executabilului (suprascriere).
    ''' Folosit când programul se închide anormal sau logging-ul normal eșuează.
    ''' </summary>
    Public Sub EmergencyDump()
        Try
            ' Extragem tot textul din RichTextBox
            Dim logContent As String = String.Empty

            If _richTextBox IsNot Nothing AndAlso Not _richTextBox.IsDisposed Then
                If _richTextBox.InvokeRequired Then
                    _richTextBox.Invoke(Sub()
                                            logContent = _richTextBox.Text
                                        End Sub)
                Else
                    logContent = _richTextBox.Text
                End If
            End If

            ' Dacă nu avem conținut, încercăm să salvăm măcar ultima eroare
            If String.IsNullOrEmpty(logContent) Then
                logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Emergency dump - No log content available"
            End If

            ' Folder executabil
            Dim exePath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
            Dim exeFolder As String = Path.GetDirectoryName(exePath)

            ' Fișier fix cu suprascriere
            Dim emergencyFile As String = Path.Combine(exeFolder, "EMERGENCY_LOG.txt")

            ' Scriem cu suprascriere
            SyncLock _fileLock
                File.WriteAllText(emergencyFile, logContent)
            End SyncLock

            ' Încercăm să adăugăm și timestamp în nume (pentru debugging ulterior)
            Try
                Dim timestampedFile As String = Path.Combine(exeFolder, $"EMERGENCY_LOG_{DateTime.Now:yyyyMMdd_HHmmss}.txt")
                File.WriteAllText(timestampedFile, logContent)
            Catch
                ' Ignorăm dacă nu putem crea fișierul cu timestamp
            End Try

        Catch ex As Exception
            ' Ultimul resort: încercăm să scriem măcar eroarea în Temp
            Try
                Dim tempFile As String = Path.Combine(Path.GetTempPath(), $"ForexeBot_CRASH_{DateTime.Now:yyyyMMdd_HHmmss}.txt")
                File.WriteAllText(tempFile, $"EMERGENCY DUMP FAILED!{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}")
            Catch
                ' Nu mai putem face nimic
            End Try
        End Try
    End Sub

    ''' <summary>
    ''' Salvează conținutul RichTextBox curent ca fișier RTF (cu culori păstrate).
    ''' Thread-safe.
    ''' </summary>
    Public Sub SaveAsRtf(filePath As String)
        Try
            If _richTextBox Is Nothing OrElse _richTextBox.IsDisposed Then Return

            If _richTextBox.InvokeRequired Then
                _richTextBox.Invoke(Sub() _richTextBox.SaveFile(filePath, RichTextBoxStreamType.RichText))
            Else
                _richTextBox.SaveFile(filePath, RichTextBoxStreamType.RichText)
            End If
        Catch ex As Exception
            ' Fail silently — apelantul gestionează eroarea
            Throw
        End Try
    End Sub

    Private Sub ClearUI()
        _richTextBox.Clear()
        SyncLock _buffer
            _buffer.Clear()
        End SyncLock
    End Sub

    ''' <summary>Re-randează toate intrările din buffer cu paleta activă curentă (async UI).</summary>
    Private Sub RefreshDisplay()
        If _richTextBox.IsDisposed OrElse _richTextBox.Disposing Then Return
        Dim snapshot As List(Of (Level As LogLevel, Msg As String))
        SyncLock _buffer
            snapshot = New List(Of (Level As LogLevel, Msg As String))(_buffer)
        End SyncLock
        If snapshot.Count = 0 Then Return
        Dim action As Action = Sub() RedrawAll(snapshot)
        If _richTextBox.InvokeRequired Then
            _richTextBox.BeginInvoke(action)
        Else
            action()
        End If
    End Sub

    Private Sub RedrawAll(entries As List(Of (Level As LogLevel, Msg As String)))
        Try
            _richTextBox.SuspendLayout()
            _richTextBox.Clear()
            For Each entry In entries
                AppendTextToUI(entry.Msg, LogColors(entry.Level))
            Next
            _richTextBox.SelectionStart = _richTextBox.TextLength
            _richTextBox.ScrollToCaret()
        Catch
            ' Fail silently — control poate fi dispus sau thread incorect
        Finally
            _richTextBox.ResumeLayout()
        End Try
    End Sub

    ''' <summary>
    ''' Internal method to handle writing to History, File and UI
    ''' </summary>
    Private Sub WriteInternal(text As String, color As Color)

        ' =========================================================
        ' 1. ISTORIC MEMORIE (NOU) - Cerința 4
        ' =========================================================
        Try
            ' Trimitem mesajul către managerul de istoric (dacă există un job activ)
            ' Notă: 'text' are deja timestamp și prefix, așa că va arăta bine în istoric.
            ' Asigură-te că JobHistoryManager este accesibil (Namespace corect)
            JobHistoryManager.AppendLog(text)
        Catch
            ' Fail silently - nu vrem să crăpăm loggingul dacă istoricul are probleme
        End Try

        ' =========================================================
        ' 2. SCRIERE ÎN FIȘIER (Dacă este configurată calea)
        ' =========================================================
        If Not String.IsNullOrEmpty(LogFilePath) Then
            Try
                SyncLock _fileLock
                    ' Adăugăm Environment.NewLine pentru că text nu îl are inclus la final
                    File.AppendAllText(LogFilePath, text & Environment.NewLine)
                End SyncLock
            Catch ex As Exception
                ' Fail silently la scrierea pe disc pentru a nu bloca execuția robotului
            End Try
        End If

        ' =========================================================
        ' 3. SCRIERE ÎN UI (Dacă este activată)
        ' =========================================================
        If EnableUI Then
            If _richTextBox.IsDisposed OrElse _richTextBox.Disposing Then Return

            If _richTextBox.InvokeRequired Then
                _richTextBox.Invoke(Sub() AppendTextToUI(text, color))
            Else
                AppendTextToUI(text, color)
            End If

            ' Păstrăm DoEvents pentru refresh vizual rapid
            Application.DoEvents()
        End If
    End Sub

    ''' <summary>
    ''' Helper method to manipulate RichTextBox (Must be called on UI thread)
    ''' </summary>
    Private Sub AppendTextToUI(text As String, color As Color)
        Try
            _richTextBox.SelectionStart = _richTextBox.TextLength
            _richTextBox.SelectionLength = 0
            _richTextBox.SelectionColor = color
            _richTextBox.AppendText(text & Environment.NewLine)
            _richTextBox.SelectionColor = _richTextBox.ForeColor

            ' Auto-scroll to bottom
            _richTextBox.SelectionStart = _richTextBox.TextLength
            _richTextBox.ScrollToCaret()
        Catch ex As Exception
            ' Fail silently if UI update fails
            System.Diagnostics.Debug.WriteLine($"Logger UI Error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Add a separator line
    ''' </summary>
    Public Sub Separator()
        WriteInternal("─────────────────────────────────────────", Color.LightGray)
    End Sub
End Class