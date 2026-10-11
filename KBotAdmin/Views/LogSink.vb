Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Threading

''' <summary>
''' Fills a <see cref="KBotLogBox"/> from any thread (slice ME01): lines are collected under a lock and the
''' box is redrawn at most once per pending batch, on the UI thread. Errors come out dark red, warnings in
''' the warning colour, by the words they contain.
''' </summary>
Public NotInheritable Class LogSink

    Private Shared ReadOnly ErrorWords As New Regex("\b(ERROR|ERR|CRITICAL|Traceback|EROARE|E.EC)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly WarnWords As New Regex("\b(WARN|WARNING|AVERTISMENT|DISTRUCTIV[AE]?|refuz\w*|MISSING|CONFLICT)\b", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private ReadOnly _box As KBotLogBox
    Private ReadOnly _lines As New List(Of KBotLogLine)()
    Private ReadOnly _lock As New Object()
    Private _pending As Integer

    Public Sub New(k_box As KBotLogBox)
        _box = k_box
    End Sub

    Public Sub Add(k_text As String)
        Add(GuessLevel(k_text), k_text)
    End Sub

    Public Sub Add(k_level As KBotLogLevel, k_text As String)
        Dim k_clean As String = If(k_text, String.Empty)
        If k_clean.StartsWith("[err] ", StringComparison.Ordinal) Then k_clean = k_clean.Substring(6)
        SyncLock _lock
            _lines.Add(New KBotLogLine With {.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), .Level = k_level, .Text = k_clean})
        End SyncLock
        If Not _box.IsHandleCreated Then Return   ' no window yet: the line is kept and shows with the next one
        If _box.InvokeRequired Then
            If Interlocked.Exchange(_pending, 1) = 0 Then _box.BeginInvoke(New Action(AddressOf Render))
        Else
            Render()
        End If
    End Sub

    Public Sub Clear()
        SyncLock _lock
            _lines.Clear()
        End SyncLock
        _box.ClearLines()
    End Sub

    Public Function ToText() As String
        Return _box.ToPlainText()
    End Function

    Public ReadOnly Property Count As Integer
        Get
            SyncLock _lock
                Return _lines.Count
            End SyncLock
        End Get
    End Property

    Private Sub Render()
        Try
            Interlocked.Exchange(_pending, 0)
            Dim k_copy As List(Of KBotLogLine)
            SyncLock _lock
                k_copy = New List(Of KBotLogLine)(_lines)
            End SyncLock
            _box.SetLines(k_copy, True)
        Catch ex As Exception
            GlobalErrorLog.Write("LogSink.Render", ex)
        End Try
    End Sub

    Public Shared Function GuessLevel(k_text As String) As KBotLogLevel
        If k_text Is Nothing Then Return KBotLogLevel.Info
        If ErrorWords.IsMatch(k_text) Then Return KBotLogLevel.Error
        If WarnWords.IsMatch(k_text) Then Return KBotLogLevel.Warn
        Return KBotLogLevel.Info
    End Function

End Class
