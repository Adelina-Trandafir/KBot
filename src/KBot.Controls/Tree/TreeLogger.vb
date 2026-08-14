Imports System.Diagnostics
Imports System.IO

''' <summary>
''' Logger centralizat bazat pe treeId.
''' Fișierul se suprascrie la fiecare pornire.
''' Thread-safe prin SyncLock.
'''
''' <para>Felia 0031: fișierul s-a mutat din directorul executabilului în
''' <c>&lt;AppDir&gt;\Logs</c>, lângă celelalte jurnale, ca vizualizatorul să aibă un singur
''' director de citit. Numele (<c>log_{treeId}.txt</c>) și parametrul opțional de cale rămân
''' neschimbate.</para>
'''
''' <para><b>Sink TERMINAL, ca <c>GlobalErrorLog</c> și <c>AdobeHostLog</c>.</b> Scrierile de aici
''' NU pot rearunca: <c>AdvancedTreeControl</c> le cheamă din căi de desenare și de așezare
''' (<c>TooltipPopup.OnPaint</c>, <c>DrawContent</c>), iar o excepție ieșită dintr-o scriere de
''' jurnal în <c>OnPaint</c> omoară procesul. Eșecurile pleacă deci pe <see cref="Trace"/> — ceea
''' ce e altceva decât înghițirea tăcută dinainte, care nu lăsa NICIO urmă.</para>
''' </summary>
Public Class TreeLogger
    Private Shared _logPath As String = Nothing
    Private Shared ReadOnly _lock As New Object()
    Private Shared _initialized As Boolean = False
    Private Shared _startTime As DateTime
    Private Shared _minLevel As LogLevel = LogLevel.INFO

    Public Enum LogLevel
        DEBUG_ = 0
        INFO = 1
        WARN = 2
        ERR = 3
    End Enum

    ''' <summary>
    ''' Inițializează logger-ul. Apelat O SINGURĂ DATĂ la pornirea aplicației.
    ''' Suprascrie fișierul existent.
    ''' </summary>
    Public Shared Sub Init(treeId As String, minLevel As LogLevel, Optional logPath As String = "")
        _minLevel = minLevel

        SyncLock _lock
            If _initialized Then Return

            _startTime = DateTime.Now

            Dim safeName As String = If(String.IsNullOrEmpty(treeId), "unknown", SanitizeFileName(treeId))
            ' Felia 0031: <AppDir>\Logs, nu directorul executabilului. Calea explicită dată de
            ' apelant are în continuare prioritate.
            Dim folder As String
            If Not String.IsNullOrEmpty(logPath) Then
                folder = logPath
            Else
                Try
                    folder = LogPaths.EnsureLogsDirectory()
                Catch ex As Exception
                    ' Directorul Logs nu se poate crea: încercăm oricum lângă executabil, iar
                    ' blocul de mai jos are propria rezervă pe Temp. Motivul NU se pierde.
                    Trace.WriteLine("TreeLogger.Init: nu am putut crea directorul Logs: " & ex.Message)
                    folder = AppDomain.CurrentDomain.BaseDirectory
                End Try
            End If

            _logPath = Path.Combine(folder, $"log_{safeName}.txt")

            Try
                ' Suprascrie fișierul (creează nou la fiecare pornire)
                File.WriteAllText(_logPath,
                    $"========================================{Environment.NewLine}" &
                    $"  TREEVIEW_VBA Log - {treeId}{Environment.NewLine}" &
                    $"  Start: {_startTime:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}" &
                    $"  Machine: {Environment.MachineName}{Environment.NewLine}" &
                    $"========================================{Environment.NewLine}")

                _initialized = True
            Catch ex As Exception
                ' Fallback: dacă nu putem scrie în folderul ales, încercăm Temp.
                Trace.WriteLine($"TreeLogger.Init: scrierea în {_logPath} a eșuat ({ex.Message}); încerc Temp.")
                Try
                    folder = Path.GetTempPath()
                    _logPath = Path.Combine(folder, $"log_{safeName}.txt")
                    File.WriteAllText(_logPath, $"[FALLBACK] Log start: {_startTime:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}")
                    _initialized = True
                Catch fallbackEx As Exception
                    ' SINK TERMINAL: nu mai există unde scrie. NU rearuncăm (vezi nota de clasă),
                    ' dar nici nu tăcem — fără linia asta, un logger mort arăta exact ca unul viu.
                    Trace.WriteLine("TreeLogger.Init: nici rezerva pe Temp nu a mers: " & fallbackEx.Message)
                    _initialized = False
                End Try
            End Try
        End SyncLock
    End Sub

    ' ─── Metode publice de logare ───

    Public Shared Sub Debug(message As String, Optional source As String = "", Optional dummy1 As Object = Nothing, Optional dummy2 As Object = Nothing)
        Write(LogLevel.DEBUG_, message, source)
    End Sub

    Public Shared Sub Info(message As String, Optional source As String = "", Optional dummy1 As Object = Nothing, Optional dummy2 As Object = Nothing)
        Write(LogLevel.INFO, message, source)
    End Sub

    Public Shared Sub Warn(message As String, Optional source As String = "", Optional dummy1 As Object = Nothing, Optional dummy2 As Object = Nothing)
        Write(LogLevel.WARN, message, source)
    End Sub

    Public Shared Sub Err(message As String, Optional source As String = "", Optional dummy1 As Object = Nothing, Optional dummy2 As Object = Nothing)
        Write(LogLevel.ERR, message, source)
    End Sub

    ''' <summary>
    ''' Loghează o excepție cu stack trace.
    ''' </summary>
    Public Shared Sub Ex(ex As Exception, Optional source As String = "", Optional dummy1 As Object = Nothing, Optional dummy2 As Object = Nothing)
        If ex Is Nothing Then Return
        Dim msg As String = $"{ex.GetType().Name}: {ex.Message}{Environment.NewLine}  StackTrace: {ex.StackTrace}"
        If ex.InnerException IsNot Nothing Then
            msg &= $"{Environment.NewLine}  Inner: {ex.InnerException.Message}"
        End If
        Write(LogLevel.ERR, msg, source)
    End Sub

    ''' <summary>
    ''' Loghează durata unei operații (pentru profiling).
    ''' </summary>
    Public Shared Sub Perf(operation As String, elapsedMs As Long, Optional source As String = "")
        Write(LogLevel.DEBUG_, $"PERF [{operation}] {elapsedMs}ms", source)
    End Sub

    ' ─── Implementare internă ───

    Private Shared Sub Write(level As LogLevel, message As String, source As String)
        If Not _initialized Then Return
        If level < _minLevel Then Return

        Dim elapsed As TimeSpan = DateTime.Now - _startTime
        Dim levelStr As String = level.ToString().TrimEnd("_"c).PadRight(5)
        Dim srcStr As String = If(String.IsNullOrEmpty(source), "", $"[{source}] ")

        Dim line As String = $"[{DateTime.Now:HH:mm:ss.fff}] [{elapsed.TotalSeconds:F3}s] [{levelStr}] {srcStr}{message}"

        SyncLock _lock
            Try
                ' Rotația nu aruncă: dacă eșuează, linia se scrie oricum. Vezi LogRotation.
                LogRotation.Roll(_logPath)
                File.AppendAllText(_logPath, line & Environment.NewLine)
            Catch ex As Exception
                ' SINK TERMINAL: apelat din căi de desenare, deci NU poate rearunca (vezi nota de
                ' clasă). Înainte era înghițire tăcută; acum eșecul lasă o urmă pe Trace.
                Trace.WriteLine("TreeLogger.Write a eșuat: " & ex.Message)
            End Try
        End SyncLock
    End Sub

    Private Shared Function SanitizeFileName(name As String) As String
        Dim invalid As Char() = Path.GetInvalidFileNameChars()
        Dim result As String = name
        For Each c In invalid
            result = result.Replace(c, "_"c)
        Next
        Return result
    End Function

    ''' <summary>
    ''' Returnează calea completă a fișierului de log (pentru debug).
    ''' </summary>
    Public Shared ReadOnly Property LogFilePath As String
        Get
            Return If(_logPath, "(neinițializat)")
        End Get
    End Property
End Class