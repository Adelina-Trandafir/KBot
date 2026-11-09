Option Strict On
Imports System.Threading.Tasks

''' <summary>
''' A page of the «Clasificatii» window (slice AV01-11): «Buget» (<see cref="ClasificatiiBugetView"/>) or «Venituri» (<see cref="ClasificatiiVenituriView"/>).
''' The window shows one at a time and asks the page in front to settle what it has pending before the other page takes over or the window closes.
''' </summary>
Public Interface IClasificatiiPage

    ''' <summary>True when something typed is not saved yet.</summary>
    ReadOnly Property HasPending As Boolean

    ''' <summary>Asks about the unsaved changes (Yes saves, No drops them, Cancel stays). True = the caller may go on.</summary>
    Function CommitAsync() As Task(Of Boolean)

End Interface
