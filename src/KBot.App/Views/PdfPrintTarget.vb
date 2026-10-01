Option Strict On
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Which server document the file on screen is, for the print counter (slice 0099). Built by the
''' view that owns the selection (it knows the id), carried by the page context and handed to
''' <see cref="ReaderHostPreview"/> before the document is shown -- the same road as
''' <see cref="PdfSigningSession"/>.
'''
''' <para>When the print queue shows a job of that file (<see cref="AdobePrintWatcher"/>), the
''' preview calls <see cref="Record"/>: one POST, and the server raises <c>PrintCount</c> on the row
''' the document has now (its stored PDF, or the document row of an unsigned DDF / ORD).</para>
'''
''' <para>The operator is never interrupted for a counter: the outcome goes to
''' <c>adobe_preview.log</c>, a failure also to the error log. The call goes straight to the client,
''' like the other secondary calls of the views (the 401 net of a view is typed on its own list): a
''' print made after the session expired is logged as not counted.</para>
''' </summary>
Public NotInheritable Class PdfPrintTarget

    Private ReadOnly _api As IPrintCountApi

    Public ReadOnly Property Kind As PrintedDocumentKind
    ''' <summary>IDREV, IDORDP, IDNC or IDRCP, by <see cref="Kind"/>.</summary>
    Public ReadOnly Property Id As Integer

    Public Sub New(kind As PrintedDocumentKind, id As Integer, api As IPrintCountApi)
        ArgumentNullException.ThrowIfNull(api)
        If id <= 0 Then Throw New ArgumentException("Document id must be positive.", NameOf(id))
        Me.Kind = kind
        Me.Id = id
        _api = api
    End Sub

    ''' <summary>
    ''' The target of a document, or Nothing when there is nothing to count against: no id, or a
    ''' client that has no print route (a test double). The preview then only logs the print.
    ''' </summary>
    Public Shared Function Create(kind As PrintedDocumentKind, id As Integer, api As IApiClient) As PdfPrintTarget
        Dim counter As IPrintCountApi = TryCast(api, IPrintCountApi)
        If counter Is Nothing OrElse id <= 0 Then Return Nothing
        Return New PdfPrintTarget(kind, id, counter)
    End Function

    ''' <summary>Romanian, for the log: «DDF IDREV 12».</summary>
    Public Function Describe() As String
        Select Case Kind
            Case PrintedDocumentKind.Ddf : Return $"DDF IDREV {Id}"
            Case PrintedDocumentKind.Ord : Return $"ORD IDORDP {Id}"
            Case PrintedDocumentKind.CabNote : Return $"nota CAB IDNC {Id}"
            Case PrintedDocumentKind.CabNoteReceipt : Return $"recipisa IDRCP {Id}"
            Case Else : Return $"{Kind} {Id}"
        End Select
    End Function

    ''' <summary>
    ''' Sends one print to the server. UI boundary (async Sub, started by the preview without
    ''' await): log and swallow.
    ''' </summary>
    Public Async Sub Record(job As AdobePrintJob)
        Dim name As String = If(job Is Nothing, String.Empty, IO.Path.GetFileName(job.DocumentPath))
        Try
            Dim result As PrintCountResult =
                Await _api.RecordPrintAsync(Kind, Id, CancellationToken.None).ConfigureAwait(True)
            If result IsNot Nothing AndAlso result.Counted Then
                AdobeHostLog.Write($"Tipărire numărată pe server: «{name}» ({Describe()}) — {RowLabel(result.Target)}, " &
                                   $"total {result.PrintCount}.")
            Else
                AdobeHostLog.Write($"Tipărire NEnumărată: «{name}» ({Describe()}) nu are un PDF păstrat pe server.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PdfPrintTarget.Record", ex)
            AdobeHostLog.Write($"ATENȚIE: tipărirea «{name}» ({Describe()}) nu a putut fi numărată pe server: {ex.Message}")
        End Try
    End Sub

    ' Which row took the count, as the server names it (routes/forexe/print_count.py).
    Private Shared Function RowLabel(target As String) As String
        Select Case If(target, String.Empty)
            Case "pdf" : Return "pe PDF-ul păstrat"
            Case "document" : Return "pe document (nesemnat, fără PDF păstrat)"
            Case "recipisa" : Return "pe recipisă"
            Case Else : Return "rând «" & target & "»"
        End Select
    End Function

End Class
