Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' The EXHAUSTIVE trace of the ActiveX (AcroPDF) viewer: <c>&lt;AppDir&gt;\Logs\acropdf_trace.log</c>.
'''
''' <para><b>Why a file of its own.</b> <see cref="AdobeHostLog"/> holds what was DECIDED, one line
''' per decision, and is read by people. This file holds EVERYTHING the ActiveX surface and its
''' timers see and do -- every tick, every window walk, every state change, every Win32 call and
''' its answer -- so a problem on a machine nobody can debug can be replayed from the file alone.
''' That volume would drown the working log, so it goes here.</para>
'''
''' <para><b>Recordings.</b> Lines are written only between <see cref="BeginSession"/> (a load
''' starts) and <see cref="EndSession"/> (the document is open, or a blocking error happened).
''' The timers keep running afterwards, but what they do then is noise for this file.</para>
'''
''' <para><b>Off by default.</b> Switched on from the settings window, Aplicatie -&gt; Documente
''' (<see cref="SwitchedOn"/>, in memory only: never saved, off at every start of the application),
''' and takes effect at once. Callers that build an expensive line
''' (a window tree dump) check <see cref="Enabled"/> first.</para>
'''
''' <para>Only the ActiveX engine writes here: the hosted-window engine shares
''' <see cref="AdobeSaveTrap"/>, but its trap has no trace sink attached.</para>
'''
''' <para>Terminal sink, like <see cref="AdobeHostLog"/>: a failed write goes to <see cref="Trace"/>
''' and is NEVER thrown. Same line format (timestamp, two spaces, text), so the log viewer reads it
''' with the same parser. Rotated by <see cref="LogRotation"/> (10 MB x 5).</para>
''' </summary>
Public Module AcroPdfTraceLog

    Private ReadOnly _gate As New Object()

    ''' <summary>The file name, next to the executable under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "acropdf_trace.log"

    ' A recording runs from the start of one document load to the moment that document is open, or
    ' a blocking error ends it (operator, 24.09.2026: after that it is only noise). Nothing is
    ' written outside a recording. UI thread only (load, timers, hooks), so no lock.
    Private _recording As Boolean
    Private _session As Integer

    ''' <summary>
    ''' The operator's switch (the checkbox in the settings window). Held in MEMORY ONLY, never saved: every start of
    ''' the application begins with it OFF (operator, 24.09.2026 -- never on by default, reset on
    ''' close).
    ''' </summary>
    Public Property SwitchedOn As Boolean

    ''' <summary>True when the switch is on AND a recording is running.</summary>
    Public ReadOnly Property Enabled As Boolean
        Get
            Return _recording AndAlso SwitchedOn
        End Get
    End Property

    ''' <summary>True while a recording runs (whatever the switch says).</summary>
    Public ReadOnly Property Recording As Boolean
        Get
            Return _recording
        End Get
    End Property

    ''' <summary>
    ''' Starts a recording (a new document load). A recording still running is closed first.
    ''' Never throws.
    ''' </summary>
    Public Sub BeginSession(source As String, what As String)
        If _recording Then EndSession(source, "superseded by a new load")
        _recording = True
        _session += 1
        Write(source, $"===== RECORDING #{_session} START: {what}")
    End Sub

    ''' <summary>
    ''' Ends the recording with its reason (document open, or the blocking error). Later lines are
    ''' dropped until the next <see cref="BeginSession"/>. Never throws.
    ''' </summary>
    Public Sub EndSession(source As String, reason As String)
        If Not _recording Then Return
        Write(source, $"===== RECORDING #{_session} END: {reason}")
        _recording = False
    End Sub

    ''' <summary>
    ''' One line: timestamp, UI-thread marker, source, text. Does nothing when the trace is off.
    ''' Never throws.
    ''' </summary>
    Public Sub Write(source As String, line As String)
        If Not Enabled Then Return
        Try
            Dim thread As String = $"T{Environment.CurrentManagedThreadId}"
            WriteRaw($"[{thread}] [{source}] {line}")
        Catch ex As Exception
            Trace.WriteLine("AcroPdfTraceLog.Write failure: " & ex.Message)
        End Try
    End Sub

    ''' <summary>A block of lines under one header (a window tree, a process list). Never throws.</summary>
    Public Sub WriteBlock(source As String, header As String, lines As IEnumerable(Of String))
        If Not Enabled Then Return
        Try
            Dim sb As New StringBuilder()
            sb.Append(header)
            If lines IsNot Nothing Then
                For Each l As String In lines
                    sb.Append(Environment.NewLine).Append("        ").Append(l)
                Next
            End If
            Write(source, sb.ToString())
        Catch ex As Exception
            Trace.WriteLine("AcroPdfTraceLog.WriteBlock failure: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Hex form of a window handle, as every trace line prints it.</summary>
    Public Function Hex(hwnd As IntPtr) As String
        Return "0x" & hwnd.ToInt64().ToString("X")
    End Function

    Private Sub WriteRaw(text As String)
        Try
            LogPaths.EnsureLogsDirectory()
            Dim filePath As String = LogPaths.Combine(FileNameOnly)
            SyncLock _gate
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath,
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & text & Environment.NewLine,
                                   New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            Trace.WriteLine("AcroPdfTraceLog terminal failure: " & terminalEx.Message)
        End Try
    End Sub

End Module
