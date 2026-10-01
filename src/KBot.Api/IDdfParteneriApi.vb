Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0084-02: the partners associated with an angajament's DDF (<c>FX_DDF_Parteneri</c>), for
''' the Sumar button «Asociaza parteneri». Kept out of <see cref="IApiClient"/> like
''' <see cref="IDdfProgramApi"/>, so the test fakes of the big interface need not learn it; the
''' view asks for it with <c>TryCast</c>. Throws <see cref="ApiException"/> on a non-2xx,
''' carrying the server's Romanian text (404 = the angajament has no document).
''' </summary>
Public Interface IDdfParteneriApi
    ''' <summary>GET /api/forexe/ddf/parteneri-asociati: the partners the document has now.</summary>
    Function GetDdfParteneriAsociatiAsync(cod As String, ct As CancellationToken) As Task(Of DdfParteneriAsociati)

    ''' <summary>POST /api/forexe/ddf/parteneri-asociati: ADDS the partners the document does not
    ''' have yet; nothing is ever removed by it.</summary>
    Function AdaugaDdfParteneriAsync(cod As String, parteneri As IEnumerable(Of DdfPartenerAsociat),
                                     ct As CancellationToken) As Task(Of DdfParteneriAsociati)
End Interface
