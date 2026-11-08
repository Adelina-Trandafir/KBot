Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 02EF-11: the ordonantare made from a RECEPTION (<c>POST /api/forexe/ord/genereaza-din-receptie</c>). Kept out of <see cref="IApiClient"/> on purpose
''' (one call, one caller; every fake of the big interface would otherwise have to learn it). <see cref="ApiClient"/> implements both.
''' </summary>
Public Interface IOrdReceptieApi

    ''' <summary>
    ''' The PROPOSED graph of a new ordonantare based on the reception <paramref name="idrh"/> (the snapshot K-BOT reserved when it saved the reception), not on
    ''' payments: the beneficiary comes from Parteneri (the supplier of the invoices linked to the reception), the lines are the reception's values, the pictures
    ''' are the reception's. Nothing is written. A refusal (no DDF, reception not in K-BOT yet, no linked invoice) arrives as <see cref="ApiException"/>.
    ''' </summary>
    Function GenereazaOrdDinReceptieAsync(cod As String, idrh As Integer, dataOrd As Date,
                                          ct As CancellationToken) As Task(Of OrdDraft)

    ''' <summary>
    ''' Slice 02EF-13: the rows of ordonantari made from a reception that a free payment may be attached to by hand
    ''' (<c>GET /api/forexe/ord/plata/{id}/linii-candidate</c>): same angajament and indicator, still pending, with room for the sum.
    ''' </summary>
    Function GetLiniiCandidateAsync(idPlataFx As Integer, ct As CancellationToken) As Task(Of OrdLiniiCandidateResponse)

    ''' <summary>Slice 02EF-13: attaches ONE payment to ONE pending row (<c>POST /api/forexe/ord/leaga-plata</c>). A refusal arrives as <see cref="ApiException"/>.</summary>
    Function LeagaPlataAsync(idPlataFx As Integer, idordrecp As Integer, ct As CancellationToken) As Task

End Interface
