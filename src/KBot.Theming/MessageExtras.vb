Option Strict On

''' <summary>
''' What a message can carry beyond text, caption, icon and the standard buttons -- all of it supplied by
''' the message catalog (<c>Config\mesaje_catalog.json</c>), none of it by a plain <c>MessageBox.Show</c> call.
''' </summary>
Public NotInheritable Class MessageExtras

    ''' <summary>A heading above the message text (simple HTML allowed); empty = none.</summary>
    Public Property Header As String = String.Empty

    ''' <summary>The caption of one extra button; empty = none.</summary>
    Public Property ExtraButton As String = String.Empty

    ''' <summary>
    ''' The X of the title bar: <c>Auto</c> (shown when the message has a way out -- a Cancel button, or OK
    ''' alone), <c>Show</c> or <c>Hide</c>.
    ''' </summary>
    Public Property CloseButton As String = "Auto"

    ''' <summary>Slice 0112-04: <c>FileName.Method</c> of the call that showed the message (filled by <c>KBotMessage</c>).</summary>
    Public Property Source As String = String.Empty

    ''' <summary>Slice 0112-04: the line of that call.</summary>
    Public Property SourceLine As Integer

End Class
