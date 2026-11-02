Option Strict On

''' <summary>
''' What the «Plan de conturi» window needs from the main window (slice AV01-06, reworked 02.11.2026): the SUBUNIT in force. The title bar's selector is the application's
''' (it is drawn at any time, before the source, when two or more subunits have it); the window reads the subunit in force and hears the operator's pick, and puts the
''' selector back (<see cref="SetSubunitKey"/>) when it could not follow a pick.
''' </summary>
Public Interface IPlanContHost

    ''' <summary>The operator picked a subunit in the title bar; the argument is its key (the IdSubunitate as text). The selector has already moved.</summary>
    Event SubunitPicked As Action(Of String)

    ''' <summary>The subunit in force; Nothing = the unit has none for the source.</summary>
    ReadOnly Property CurrentSubunitId As Integer?

    ''' <summary>Moves the selector to a key without raising <see cref="SubunitPicked"/>.</summary>
    Sub SetSubunitKey(k_key As String)

End Interface
