Option Strict On

''' <summary>
''' Slice 0113-01: implemented by whatever can hold a document in an embedded Adobe (a view of the shell, a page of the
''' main window). The main window calls <see cref="PutAway"/> BEFORE it puts the thing on screen away (another page
''' takes the work area, or a modal window opens over it): it moves to a tab that shows no PDF -- the navigation bar
''' stays where it is -- and lets its document go, so Adobe is closed while nobody looks at it. Coming back, the
''' operator is on that tab and the document opens again when the PDF tab is chosen.
''' </summary>
Public Interface IPutAway

    ''' <summary>Moves to a tab without a PDF and closes the documents shown in Adobe. Does nothing when no document is open.</summary>
    Sub PutAway()

End Interface
