Option Strict On

''' <summary>The kind of a message: picks the glyph, the colour and the system sound.</summary>
Public Enum KBotMsgKind
    ''' <summary>No glyph (the native <c>MessageBoxIcon.None</c>).</summary>
    None = 0
    Info = 1
    Warning = 2
    [Error] = 3
    ''' <summary>A confirmation (<c>MessageBoxIcon.Question</c>).</summary>
    Question = 4
End Enum
