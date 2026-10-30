Option Strict On
Imports System.Threading.Tasks

''' <summary>
''' A page of the «Sabloane» window (slice AV01-14): the operations of type M, the operations of type A (and later the salary templates).
''' The window shows one at a time and asks the page in front to settle what it has pending before the other page takes over or the window closes.
''' </summary>
Public Interface ISablonPagina

    ''' <summary>True when something was typed, added or deleted and is not saved yet.</summary>
    ReadOnly Property HasPending As Boolean

    ''' <summary>Asks about the unsaved changes (Yes saves, No drops them, Cancel stays). True = the caller may go on.</summary>
    Function CommitAsync() As Task(Of Boolean)

End Interface
