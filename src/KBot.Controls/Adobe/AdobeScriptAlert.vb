Option Strict On

''' <summary>What <see cref="AdobeSaveTrap"/> did with one script window of Adobe (slice 0078-12).</summary>
Public Enum AdobeScriptAlertAction
    ''' <summary>
    ''' A «Warning: JavaScript Window» whose text is on the trapped-alert list (a script ERROR):
    ''' K-BOT pressed its OK. Raised on the first press only.
    ''' </summary>
    Pressed = 0
    ''' <summary>
    ''' An alert whose text is NOT on the list: the form talking to the operator («Nu sunt erori la
    ''' sectiunea A»). Left on screen.
    ''' </summary>
    LeftToOperator = 1
    ''' <summary>
    ''' An error alert that was still showing after every OK press. Left on screen (hidden, it would
    ''' keep Adobe blocked): the operator must press OK. Always follows a <see cref="Pressed"/>.
    ''' </summary>
    Stuck = 2
    ''' <summary>An error alert with no OK button, hidden.</summary>
    Hidden = 3
    ''' <summary>The «JavaScript Debugger» console that opened itself on an error, hidden.</summary>
    ConsoleHidden = 4
    ''' <summary>A console the operator opened: left alone.</summary>
    ConsoleLeftAlone = 5
End Enum

''' <summary>
''' One script window of Adobe the trap met, as <see cref="AdobeSaveTrap.ScriptAlertSeen"/> reports
''' it (slice 0078-12). The same facts the working log already carried as text, now structured, so a
''' bench or a counter can use them. POCO: no Try/Catch.
''' </summary>
Public NotInheritable Class AdobeScriptAlert

    Public ReadOnly Property Time As DateTime
    Public ReadOnly Property Handle As IntPtr
    ''' <summary>The window title («Warning: JavaScript Window», «JavaScript Debugger»).</summary>
    Public ReadOnly Property Title As String
    ''' <summary>The readable text of the window, its Static children joined by « | ».</summary>
    Public ReadOnly Property Text As String
    Public ReadOnly Property Action As AdobeScriptAlertAction
    ''' <summary>The trapped-alert pattern that matched, or Nothing.</summary>
    Public ReadOnly Property Rule As String
    ''' <summary>OK presses so far (0 when no press was made).</summary>
    Public ReadOnly Property Attempts As Integer

    Public Sub New(k_time As DateTime, k_handle As IntPtr, k_title As String, k_text As String,
                   k_action As AdobeScriptAlertAction, k_rule As String, k_attempts As Integer)
        Time = k_time
        Handle = k_handle
        Title = If(k_title, "")
        Text = If(k_text, "")
        Action = k_action
        Rule = k_rule
        Attempts = k_attempts
    End Sub

    ''' <summary>The JavaScript Debugger console, not an alert box.</summary>
    Public ReadOnly Property IsConsole As Boolean
        Get
            Return Action = AdobeScriptAlertAction.ConsoleHidden OrElse Action = AdobeScriptAlertAction.ConsoleLeftAlone
        End Get
    End Property

    ''' <summary>
    ''' A script ERROR: the alert text is on the trapped-alert list (the action says what became of
    ''' it). The form's own messages and the console are not errors.
    ''' </summary>
    Public ReadOnly Property IsError As Boolean
        Get
            Return Action = AdobeScriptAlertAction.Pressed OrElse Action = AdobeScriptAlertAction.Stuck OrElse
                   Action = AdobeScriptAlertAction.Hidden
        End Get
    End Property

    ''' <summary>Romanian label of what the window is.</summary>
    Public ReadOnly Property KindLabel As String
        Get
            If IsConsole Then Return "Consolă"
            If IsError Then Return "Eroare"
            Return "Mesaj"
        End Get
    End Property

    ''' <summary>Romanian label of what K-BOT did.</summary>
    Public ReadOnly Property ActionLabel As String
        Get
            Select Case Action
                Case AdobeScriptAlertAction.Pressed : Return "OK apăsat"
                Case AdobeScriptAlertAction.LeftToOperator : Return "lăsat operatorului"
                Case AdobeScriptAlertAction.Stuck : Return "NU s-a închis (rămâne pe ecran)"
                Case AdobeScriptAlertAction.Hidden : Return "ascuns (fără OK)"
                Case AdobeScriptAlertAction.ConsoleHidden : Return "consolă ascunsă"
                Case Else : Return "consolă lăsată în pace"
            End Select
        End Get
    End Property

    Public Function Describe() As String
        Return $"{KindLabel} 0x{Handle.ToInt64():X} «{Title}» — {ActionLabel}" &
               If(Attempts > 0, $" (apăsări: {Attempts})", "") &
               If(Text.Length > 0, $": {Text}", "")
    End Function

End Class
