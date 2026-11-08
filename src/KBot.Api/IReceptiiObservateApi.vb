Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0076-01: <c>POST /api/forexe/receptii/data-schimbata</c>. Kept out of <see cref="IApiClient"/> on
''' purpose, like <see cref="IMarcajApi"/> - one call, two callers (the shell when the FOREXE page confirms the
''' save, the ingest flow when it replays what is still pending), and every fake of the big interface would
''' otherwise have to learn it. <see cref="ApiClient"/> implements both.
''' </summary>
Public Interface IReceptiiObservateApi

    ''' <summary>
    ''' Tells the server that the reception described by <paramref name="obs"/> now has another date, so the stored
    ''' reception is moved BEFORE the next download is ingested. Returns normally when the server did it or had
    ''' already done it. Throws <see cref="ApiException"/> on any non-2xx; status 409 = the server cannot tell which
    ''' reception it is (nothing was written).
    ''' </summary>
    Function SpuneDataSchimbataAsync(obs As ObservatieReceptie, ct As CancellationToken) As Task

    ''' <summary>
    ''' Slice 0076-02: tells the server that the reception described by <paramref name="obs"/> (its old date, sum and
    ''' rank) was DELETED in FOREXE, so it is marked deleted at once. Same contract as
    ''' <see cref="SpuneDataSchimbataAsync"/>: returns normally when done or already done; 409 = the server cannot
    ''' tell which reception it is (nothing was written).
    ''' </summary>
    Function SpuneStearsaAsync(obs As ObservatieReceptie, ct As CancellationToken) As Task

End Interface
