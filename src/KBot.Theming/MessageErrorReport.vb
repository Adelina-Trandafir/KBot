Option Strict On

''' <summary>
''' An error message as the operator saw it, handed to <see cref="KBotMessage.ErrorReporter"/> when the
''' operator presses the «send the error» button in the title bar of the message window (slice 0112-04).
''' Only what the window itself knows; the application adds the rest (version, machine, session, logs).
''' </summary>
Public NotInheritable Class MessageErrorReport

    ''' <summary>A new id for this report; the server stores a report once however many times it is sent.</summary>
    Public Property Rid As String = Guid.NewGuid().ToString("D")

    ''' <summary>When the message was shown, UTC.</summary>
    Public Property ShownUtc As DateTime = DateTime.UtcNow

    ''' <summary><c>FileName.Method</c> of the call that showed it; empty when unknown.</summary>
    Public Property Source As String = String.Empty

    Public Property SourceLine As Integer
    Public Property Caption As String = String.Empty
    Public Property Header As String = String.Empty

    ''' <summary>The text as shown (simple HTML, as the window received it).</summary>
    Public Property Text As String = String.Empty

    ''' <summary>The answers offered, e.g. <c>OK</c> or <c>YesNo</c>.</summary>
    Public Property Buttons As String = String.Empty

    ''' <summary>Type and title of the window the message was shown over; empty when there was none.</summary>
    Public Property OwnerForm As String = String.Empty

End Class
