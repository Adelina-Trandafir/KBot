Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0088 -- the «Nota contabila corectie CAB» routes (routes/forexe/note_cab.py) and the
''' note's PDF (routes/forexe/pdf.py, family «nc»). Kept out of <see cref="IApiClient"/> on purpose,
''' like <see cref="IUncorrectedOperationsApi"/>: the callers are the note window and the notes
''' view; <see cref="ApiClient"/> implements both. Every call throws <see cref="ApiException"/> on a
''' non-2xx, with the server's Romanian «error» text.
''' </summary>
Public Interface ICabNotesApi

    ''' <summary>Next free number, the notes already made, the angajamente with their indicators.</summary>
    Function GetCabNotePreparationAsync(ct As CancellationToken) As Task(Of CabNotePreparation)

    ''' <summary>Stores the note; returns its IDNC. 409 when the number or the operation already has one.</summary>
    Function SaveCabNoteAsync(note As CabCorrectionNote, ct As CancellationToken) As Task(Of Integer)

    ''' <summary>
    ''' Slice 0088-05: every note of one save of the window, ALL or NOTHING (POST
    ''' /api/forexe/note-cab/lot). One note per number, one angajament per note. Sets each note's
    ''' <c>IdNc</c>. 400/409 (nothing saved) on a number given twice or already used this year.
    ''' </summary>
    Function SaveCabNotesAsync(notes As IReadOnlyList(Of CabCorrectionNote), ct As CancellationToken) As Task

    ''' <summary>The notes of one angajament, with the sha of the stored PDF.</summary>
    Function GetCabNotesAsync(codAngajament As String, ct As CancellationToken) As Task(Of List(Of CabCorrectionNote))

    ''' <summary>
    ''' Records that the note was uploaded into FOREXE, with the page's answer and (slice 0088-04) the
    ''' registration index read from it (empty = unknown, the stored one is kept).
    ''' </summary>
    Function MarkCabNoteSentAsync(idNc As Integer, answer As String, registrationIndex As String, ct As CancellationToken) As Task

    ''' <summary>
    ''' Slice 0088-04: POST /api/forexe/note-cab/{idnc}/recipisa -- stores the FOREXE receipt on the
    ''' note's PDF and returns it as stored. 409 when the note has no PDF on the server.
    ''' </summary>
    Function SaveCabNoteReceiptAsync(idNc As Integer, receipt As CabNoteReceipt, content As Byte(), ct As CancellationToken) As Task(Of CabNoteReceipt)

    ''' <summary>Slice 0088-04: GET /api/forexe/note-cab/recipisa/{idrcp} -- the receipt file.</summary>
    Function DownloadCabNoteReceiptAsync(idReceipt As Integer, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>GET /api/forexe/nc/pdf/{idnc} -- same contract as <see cref="IApiClient.DownloadOrdPdfAsync"/>.</summary>
    Function DownloadCabNotePdfAsync(idNc As Integer, cachedSha As String, ct As CancellationToken) As Task(Of PdfDownloadResult)

    ''' <summary>
    ''' PUT /api/forexe/nc/pdf/{idnc} -- same contract as <see cref="IApiClient.UploadOrdPdfAsync"/>.
    ''' The note's first PDF goes up unsigned, with <paramref name="semnatura"/> = «-».
    ''' </summary>
    Function UploadCabNotePdfAsync(idNc As Integer, continut As Byte(), shaPrecedent As String,
                                   semnatura As String, semnaturi As IReadOnlyList(Of PdfSignatureRecord),
                                   ct As CancellationToken) As Task(Of PutPdfResponse)

End Interface
