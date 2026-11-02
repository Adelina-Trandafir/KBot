Option Strict On

''' <summary>
''' Slice 01EF-02 (reworked 02.11.2026) -- the SUBUNIT of the application, as the main window holds it. K-BOT does not use it, but the accounting operations (and the other
''' Avacont operations) belong to a subunit, so the pages that book read <see cref="CurrentSubunitId"/> when they work and hear <see cref="SubunitPicked"/> when the
''' operator picks another one in the title bar (the selector stands before the source and is drawn at any time when two or more subunits have it).
''' </summary>
Public Interface ISubunitHost

    ''' <summary>The operator picked a subunit in the title bar; the argument is its key (the IdSubunitate as text). The selector has already moved.</summary>
    Event SubunitPicked As Action(Of String)

    ''' <summary>The subunit in force; Nothing = the unit has none for the source.</summary>
    ReadOnly Property CurrentSubunitId As Integer?

    ''' <summary>Moves the selector to a key without raising <see cref="SubunitPicked"/> (a page that could not follow a pick puts it back).</summary>
    Sub SetSubunitKey(k_key As String)

End Interface
