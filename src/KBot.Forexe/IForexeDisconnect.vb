Imports System.Threading.Tasks

''' <summary>
''' Slice 0097 -- closing the live FOREXE session on request (the unit switch in the caption bar:
''' the browser session belongs to the unit that was open, and what it downloads is written into
''' the session's database). Kept apart from <c>IForexeRunner</c>, like
''' <see cref="IForexeDocumentUpload"/>, so the test doubles stay as they are; <c>ForexeRunner</c>
''' implements it and the coordinator asks for it with TryCast.
''' </summary>
Public Interface IForexeDisconnect

    ''' <summary>Closes the browser and forgets the session. Nothing to do without one.</summary>
    Function DisconnectAsync() As Task

End Interface
