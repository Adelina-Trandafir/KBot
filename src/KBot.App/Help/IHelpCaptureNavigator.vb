Option Strict On

''' <summary>
''' A window that can put itself on the screen a help capture asks for (slice 0000-02).
''' Implemented by the main window; <see cref="HelpCaptureForm"/> finds it among the open forms.
''' </summary>
Public Interface IHelpCaptureNavigator

    ''' <summary>
    ''' Goes to <paramref name="target"/> (the part of a capture's <c>goto</c> this window knows:
    ''' <c>view:</c>, <c>menu:</c>, <c>setari:</c>). Returns Nothing when it got there, or a Romanian
    ''' sentence for the operator saying what to do by hand (a view that is off for the selected
    ''' angajament, no unit open...). An unknown target throws <see cref="ArgumentException"/>.
    ''' </summary>
    Function NavigateForCapture(target As String) As String

    ''' <summary>
    ''' Slice 0000-19: the caption the operator reads for <paramref name="target"/> (the view's
    ''' button, the menu row, the settings window), for the «Deschide ...» button of a search hit. Nothing
    ''' when this window has no such screen.
    ''' </summary>
    Function TargetCaption(target As String) As String

End Interface
