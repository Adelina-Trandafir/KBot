Option Strict On
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Which document was printed (slice 0099) -- one value per route of routes/forexe/print_count.py.</summary>
Public Enum PrintedDocumentKind
    ''' <summary>A DDF revision; id = IDREV.</summary>
    Ddf = 0
    ''' <summary>An ordonantare; id = IDORDP.</summary>
    Ord = 1
    ''' <summary>A CAB correction note; id = IDNC.</summary>
    CabNote = 2
    ''' <summary>The FOREXE receipt of a CAB correction note; id = IDRCP.</summary>
    CabNoteReceipt = 3
End Enum

''' <summary>The server's answer to one counted print (slice 0099). POCO.</summary>
Public NotInheritable Class PrintCountResult
    ''' <summary>False when the document has no row that can hold the count (a note with no stored PDF).</summary>
    Public Property Counted As Boolean
    ''' <summary>Which row took the count: «pdf», «document» (an unsigned DDF / ORD), «recipisa», or empty.</summary>
    Public Property Target As String = String.Empty
    ''' <summary>The count after this print; Nothing when <see cref="Counted"/> is False.</summary>
    Public Property PrintCount As Integer?
End Class

''' <summary>
''' Slice 0099 -- PrintCount (routes/forexe/print_count.py). Kept out of <see cref="IApiClient"/>
''' on purpose, like <see cref="ICabNotesApi"/>: the only caller is the print counter of the
''' document views; <see cref="ApiClient"/> implements it. Throws <see cref="ApiException"/> on a
''' non-2xx, with the server's Romanian «error» text.
''' </summary>
Public Interface IPrintCountApi

    ''' <summary>
    ''' POST .../print -- one more print of the document. The server decides which row takes the
    ''' count (the stored PDF, or the document row of an unsigned DDF / ORD). A write: while the
    ''' FOREXE robot runs it waits at the server gate and leaves when the robot is done.
    ''' </summary>
    Function RecordPrintAsync(kind As PrintedDocumentKind, id As Integer, ct As CancellationToken) As Task(Of PrintCountResult)

End Interface
