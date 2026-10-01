Option Strict On
Imports System.Collections.Generic
Imports KBot.Domain

''' <summary>
''' A page of <see cref="NoteCabView"/> (slice 0088) -- the same contract as <c>IOrdPage</c>: the
''' page is given a context and renders it; it has no API client and makes no network call.
''' </summary>
Public Interface ICabNotePage

    ''' <summary>«vizualizare», «document» or «recipisa» -- identical to the key of its <c>navSub</c> entry.</summary>
    ReadOnly Property PageKey As String

    ''' <summary>Renders the context; Nothing = nothing selected, the page shows its empty state.</summary>
    Sub SetContext(ctx As CabNotePageContext)

    ''' <summary>
    ''' The button of the missing-document surface: «Generează» on «Document», «Validează documentul»
    ''' on «Recipisă» (slice 0088-04). «Vizualizare» never raises it.
    ''' </summary>
    Event GenerateRequested As EventHandler

End Interface

''' <summary>What the pages of <see cref="NoteCabView"/> render for the node selected now. POCO.</summary>
Public NotInheritable Class CabNotePageContext

    ''' <summary>The notes the node covers: one on a note, every note of the month on a month.</summary>
    Public ReadOnly Property Notes As List(Of CabCorrectionNote)

    ''' <summary>The selected note; Nothing on a month (a month has no single document).</summary>
    Public ReadOnly Property Note As CabCorrectionNote

    ''' <summary>The document to show; empty on a month.</summary>
    Public ReadOnly Property PdfPath As String

    ''' <summary>Is that file on this computer right now?</summary>
    Public ReadOnly Property PdfExists As Boolean

    ''' <summary>The signing session of the document at <see cref="PdfPath"/> (Nothing on a month).</summary>
    Public Property Signing As PdfSigningSession

    ''' <summary>
    ''' Slice 0088-04: the note's FOREXE receipt on this computer; empty on a month, or when the note
    ''' has no receipt on the server (the «Recipisă» page then offers «Validează documentul»).
    ''' </summary>
    Public Property ReceiptPath As String = String.Empty

    ''' <summary>Is <see cref="ReceiptPath"/> on this computer right now?</summary>
    Public Property ReceiptExists As Boolean

    ''' <summary>Slice 0099: the note the document at <see cref="PdfPath"/> is, for the print counter (Nothing on a month).</summary>
    Public Property PrintTarget As PdfPrintTarget

    ''' <summary>Slice 0099: the receipt at <see cref="ReceiptPath"/>, for the print counter (Nothing without a receipt).</summary>
    Public Property ReceiptPrintTarget As PdfPrintTarget

    Public Sub New(notes As List(Of CabCorrectionNote), note As CabCorrectionNote, pdfPath As String, pdfExists As Boolean)
        Me.Notes = If(notes, New List(Of CabCorrectionNote)())
        Me.Note = note
        Me.PdfPath = If(pdfPath, String.Empty)
        Me.PdfExists = pdfExists
    End Sub

End Class
