Option Strict On

''' <summary>
''' The contract of one page of the settings window (slice 0072): a UserControl the shell
''' creates lazily at first activation, exactly as <c>KbotForm</c> creates its views.
''' </summary>
Public Interface ISetariView

    ''' <summary>The nav key ("info", "aplicatie", "forexe", "tema", "autentificare").</summary>
    ReadOnly Property ViewKey As String

    ''' <summary>
    ''' The page came on screen. Pages re-read what they show from the stores here, so a
    ''' value changed elsewhere (another page, the DDF view's Adobe combos) is current.
    ''' </summary>
    Sub Activated()

    ''' <summary>
    ''' The window is about to close. A page with unsaved work asks the operator here and
    ''' returns False to keep the window open; every other page returns True.
    ''' </summary>
    Function CanClose() As Boolean

    ''' <summary>One line for the status band of the window.</summary>
    Event StatusChanged(text As String)

    ''' <summary>The page is waiting on the server; the band shows the busy bar.</summary>
    Event BusyChanged(busy As Boolean)

End Interface
