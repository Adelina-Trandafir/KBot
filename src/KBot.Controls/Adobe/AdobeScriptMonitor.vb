Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common

''' <summary>
''' Counts and keeps the script windows Adobe raised for ONE document (slice 0078-12): the
''' hosted-window engine's answer to «how many JS errors did this document give, and which».
'''
''' <para><b>Why.</b> <see cref="AdobeSaveTrap"/> already pressed OK on script errors and logged each
''' as a line of text, on both engines. On the ActiveX engine that was always watched through its
''' trace; on the hosted window nobody tallied it, and an older Adobe (before 2024: Acrobat DC / 2020)
''' shows the form's scripts failing there again. This class is the tally. <see cref="AdobeReaderHost"/>
''' feeds it from the trap, resets it at every document, and writes <see cref="Summary"/> to the
''' working log when the document is open and when it is let go.</para>
'''
''' <para>UI thread only (the trap's timer and hook both run there), so no lock.</para>
''' </summary>
Public NotInheritable Class AdobeScriptMonitor

    Private ReadOnly _alerts As New List(Of AdobeScriptAlert)()

    ''' <summary>Raised (UI thread) after each recorded window. Argument = the window just recorded.</summary>
    Public Event Recorded As Action(Of AdobeScriptAlert)

    ''' <summary>Every window recorded since the last <see cref="Reset"/>, in the order they were seen.</summary>
    Public ReadOnly Property Alerts As IReadOnlyList(Of AdobeScriptAlert)
        Get
            Return _alerts
        End Get
    End Property

    ''' <summary>Script ERRORS seen: alerts whose text is on the list (pressed, or hidden for lack of an OK).</summary>
    Public ReadOnly Property ErrorCount As Integer
        Get
            Return _alerts.Where(Function(k_a) k_a.Action = AdobeScriptAlertAction.Pressed OrElse k_a.Action = AdobeScriptAlertAction.Hidden).Count()
        End Get
    End Property

    ''' <summary>
    ''' Errors that were still on screen after every OK press. Each one was also counted in
    ''' <see cref="ErrorCount"/> when it was first pressed: this is a part of it, not an addition.
    ''' </summary>
    Public ReadOnly Property StuckCount As Integer
        Get
            Return _alerts.Where(Function(k_a) k_a.Action = AdobeScriptAlertAction.Stuck).Count()
        End Get
    End Property

    ''' <summary>The form's own messages, left to the operator.</summary>
    Public ReadOnly Property MessageCount As Integer
        Get
            Return _alerts.Where(Function(k_a) k_a.Action = AdobeScriptAlertAction.LeftToOperator).Count()
        End Get
    End Property

    ''' <summary>The JavaScript Debugger console showing up (hidden, or left to the operator).</summary>
    Public ReadOnly Property ConsoleCount As Integer
        Get
            Return _alerts.Where(Function(k_a) k_a.IsConsole).Count()
        End Get
    End Property

    ''' <summary>True when nothing was recorded since the last reset.</summary>
    Public ReadOnly Property IsEmpty As Boolean
        Get
            Return _alerts.Count = 0
        End Get
    End Property

    ''' <summary>Records one window and tells the listeners. A failing listener never stops the trap.</summary>
    Public Sub Record(k_alert As AdobeScriptAlert)
        If k_alert Is Nothing Then Throw New ArgumentNullException(NameOf(k_alert))
        _alerts.Add(k_alert)
        Try
            RaiseEvent Recorded(k_alert)
        Catch ex As Exception
            ' Called from the trap's timer / hook: one broken listener must not break the sweep.
            GlobalErrorLog.Write("AdobeScriptMonitor.Record", ex)
        End Try
    End Sub

    ''' <summary>Forgets everything (a new document). Does not raise <see cref="Recorded"/>.</summary>
    Public Sub Reset()
        _alerts.Clear()
    End Sub

    ''' <summary>
    ''' One Romanian sentence for the working log and the bench: «12 erori de script (OK apăsat de
    ''' K-BOT; 1 nu s-a închis); 4 mesaje lăsate operatorului; consola de script ascunsă de 1 ori».
    ''' «niciun mesaj de script» when nothing was seen. No trailing full stop.
    ''' </summary>
    Public Function Summary() As String
        If _alerts.Count = 0 Then Return "niciun mesaj de script"
        Dim parts As New List(Of String)()
        Dim errors As Integer = ErrorCount
        Dim stuck As Integer = StuckCount
        If errors > 0 OrElse stuck > 0 Then
            parts.Add($"{errors} erori de script (OK apăsat de K-BOT" &
                      If(stuck > 0, $"; {stuck} nu s-a închis", "") & ")")
        End If
        Dim messages As Integer = MessageCount
        If messages > 0 Then parts.Add($"{messages} mesaje lăsate operatorului")
        Dim hidden As Integer = _alerts.Where(Function(k_a) k_a.Action = AdobeScriptAlertAction.ConsoleHidden).Count()
        If hidden > 0 Then parts.Add($"consola de script ascunsă de {hidden} ori")
        Dim k_left As Integer = _alerts.Where(Function(k_a) k_a.Action = AdobeScriptAlertAction.ConsoleLeftAlone).Count()
        If k_left > 0 Then parts.Add($"consola deschisă de operator, lăsată în pace ({k_left})")
        Return String.Join("; ", parts)
    End Function

End Class
