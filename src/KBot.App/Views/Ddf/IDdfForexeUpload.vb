Option Strict On

''' <summary>
''' The seam for uploading a saved fundamentation document to FOREXE (plan section 10.4).
'''
''' <para><b>Nothing implements this against the portal yet, and that is deliberate.</b> In
''' Access the upload was a manual step: the operator saved the document, then drove the
''' FOREXE portal by hand. Slice 0051 ports the SAVE, not the upload -- the Playwright work
''' belongs to <c>KBot.Forexe</c> and to a slice that does not exist yet.</para>
'''
''' <para>The interface exists now so the call site has a shape to bind to when that slice
''' arrives, instead of the upload being retro-fitted into the save handler. The only
''' implementation in the tree, <see cref="DdfForexeUploadNeimplementat"/>, throws: an upload
''' that silently did nothing would let an operator believe the document reached the portal.
''' </para>
''' </summary>
Public Interface IDdfForexeUpload

    ''' <summary>
    ''' Sends one saved revision to the FOREXE portal.
    ''' </summary>
    ''' <param name="iddf">The document key returned by the save.</param>
    ''' <param name="idrev">The revision key returned by the save.</param>
    ''' <param name="ct">Cancellation for the browser run.</param>
    Function UrcaRevizieAsync(iddf As Integer, idrev As Integer,
                              ct As Threading.CancellationToken) As Task

End Interface

''' <summary>
''' The placeholder implementation. It refuses loudly rather than pretending to have uploaded
''' anything -- see <see cref="IDdfForexeUpload"/> for why the real one is not here yet.
''' </summary>
Public NotInheritable Class DdfForexeUploadNeimplementat
    Implements IDdfForexeUpload

    Public Function UrcaRevizieAsync(iddf As Integer, idrev As Integer,
                                     ct As Threading.CancellationToken) As Task _
                                     Implements IDdfForexeUpload.UrcaRevizieAsync
        Throw New NotImplementedException(
            "The FOREXE upload of a fundamentation document is not implemented. " &
            "Slice 0051 ports the save only; the portal run belongs to a later slice.")
    End Function

End Class
