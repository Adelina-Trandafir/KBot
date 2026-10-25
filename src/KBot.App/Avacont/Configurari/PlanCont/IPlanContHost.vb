Option Strict On

''' <summary>
''' What the «Plan de conturi» window needs from the main window's title bar (slice AV01-06): the subunit selector that stands
''' beside the sector/source one. The window fills it with the subunits of the chosen source (it is drawn only with two or more),
''' and hears the operator's pick; the selector does NOT move by itself, the window moves it once the pick went through.
''' </summary>
Public Interface IPlanContHost

    ''' <summary>The operator picked a subunit in the title bar; the argument is the key given to <see cref="SetSubunitChoices"/>.</summary>
    Event SubunitPicked As Action(Of String)

    ''' <summary>The choices (key, text) of the subunit selector and the one shown. No choices = the selector is not drawn.</summary>
    Sub SetSubunitChoices(k_items As IEnumerable(Of KeyValuePair(Of String, String)), k_selectedKey As String)

    ''' <summary>Moves the selector to a key without raising <see cref="SubunitPicked"/>.</summary>
    Sub SetSubunitKey(k_key As String)

    ''' <summary>Shows or hides the selector (it belongs to the window only while the window is on screen).</summary>
    Sub SetSubunitShown(k_shown As Boolean)

End Interface
