Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' Decides which Adobe script alerts the save trap closes by itself (operator, 24.09.2026).
'''
''' WHY: the DDF / ORD forms raise two kinds of «Warning: JavaScript Window» boxes. Script
''' errors («GeneralError / Operation failed.») change nothing and are noise; but the forms ALSO
''' use the same box to talk to the operator («Validarea s-a terminat cu succes! Semnati
''' formularul...»). Pressing OK on those left the operator not knowing what happened. So an
''' alert is closed only when its message -- the text of its «Static» children, the same text
''' Window Detective shows -- matches one of the operator's patterns
''' (<see cref="AppSettings.AdobeTrappedAlerts"/>, edited in Setari). Everything else stays.
'''
''' Patterns are regular expressions, case-insensitive, matched ANYWHERE in the text, so a plain
''' word works as written. A broken pattern never throws here: it is skipped (the settings dialog
''' refuses to save one, so it only happens in a hand-edited file).
''' </summary>
Public NotInheritable Class AdobeScriptAlertFilter

    ''' <summary>A pattern that takes longer than this on one message counts as «no match».</summary>
    Public Shared ReadOnly MatchTimeout As TimeSpan = TimeSpan.FromMilliseconds(200)

    Private Const Options As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant

    ' The compiled list, rebuilt when AppSettings.Current (or its list) is replaced by a save.
    Private Shared _cachedFor As List(Of String)
    Private Shared _cached As IReadOnlyList(Of Regex) = New List(Of Regex)()
    Private Shared ReadOnly _gate As New Object()

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Empty when <paramref name="pattern"/> is a valid regular expression, else the Romanian
    ''' reason (for the settings dialog). A blank pattern is invalid: it would match everything.
    ''' </summary>
    Public Shared Function CheckPattern(pattern As String) As String
        If String.IsNullOrWhiteSpace(pattern) Then Return "rândul este gol"
        Try
            Dim r As New Regex(pattern.Trim(), Options, MatchTimeout)
            If r.IsMatch(String.Empty) Then Return "expresia se potrivește cu orice text (și cu unul gol)"
            Return String.Empty
        Catch ex As ArgumentException
            Return ex.Message
        End Try
    End Function

    ''' <summary>The valid patterns, compiled. Blank and broken ones are skipped.</summary>
    Public Shared Function Compile(patterns As IEnumerable(Of String)) As IReadOnlyList(Of Regex)
        Dim list As New List(Of Regex)()
        If patterns Is Nothing Then Return list
        For Each p As String In patterns
            If CheckPattern(p).Length > 0 Then Continue For
            list.Add(New Regex(p.Trim(), Options, MatchTimeout))
        Next
        Return list
    End Function

    ''' <summary>
    ''' The first pattern that matches <paramref name="message"/>, or Nothing (= leave the alert to
    ''' the operator). Line breaks and runs of spaces in the message count as one space, so a
    ''' pattern copied from the log (where the lines are joined) still matches.
    ''' </summary>
    Public Shared Function FirstMatch(message As String, rules As IEnumerable(Of Regex)) As String
        If String.IsNullOrWhiteSpace(message) OrElse rules Is Nothing Then Return Nothing
        Dim text As String = Normalize(message)
        For Each r As Regex In rules
            Try
                If r.IsMatch(text) Then Return r.ToString()
            Catch ex As RegexMatchTimeoutException
                ' Too slow on this text: not a match. Logged, the other patterns still run.
                GlobalErrorLog.Write("AdobeScriptAlertFilter.FirstMatch", ex)
            End Try
        Next
        Return Nothing
    End Function

    ''' <summary>Whitespace (line breaks included) collapsed to single spaces, trimmed.</summary>
    Public Shared Function Normalize(message As String) As String
        If message Is Nothing Then Return String.Empty
        Return Regex.Replace(message, "\s+", " ").Trim()
    End Function

    ''' <summary>The patterns in force now (<see cref="AppSettings.Current"/>), compiled once per save.</summary>
    Public Shared Function Current() As IReadOnlyList(Of Regex)
        Try
            Dim source As List(Of String) = AppSettings.Current.AdobeTrappedAlerts
            SyncLock _gate
                If Not ReferenceEquals(source, _cachedFor) Then
                    _cached = Compile(source)
                    _cachedFor = source
                End If
                Return _cached
            End SyncLock
        Catch ex As Exception
            ' A broken store must not stop the trap: nothing matches, every alert stays on screen.
            GlobalErrorLog.Write("AdobeScriptAlertFilter.Current", ex)
            Return New List(Of Regex)()
        End Try
    End Function

End Class
