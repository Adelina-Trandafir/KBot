Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0114-26: <c>POST /api/forexe/rezervari/repara-legaturi</c>. Kept out of <see cref="IApiClient"/> on
''' purpose, like <see cref="IMarcajApi"/> - one call, one caller (the shell, from the Rezervari view), and every
''' fake of the big interface would otherwise have to learn it. <see cref="ApiClient"/> implements both.
''' </summary>
Public Interface IRezervariLegaturiApi

    ''' <summary>
    ''' Frees the reservations of <paramref name="cod"/> that are linked to a revision which does not cover them, so
    ''' they get their own revision. <paramref name="apply"/> = False is the dry run (nothing is written); True
    ''' writes. Throws <see cref="ApiException"/> on any non-2xx.
    ''' </summary>
    Function RepairReservationLinksAsync(cod As String, apply As Boolean, ct As CancellationToken) _
        As Task(Of RezervariLegaturiResult)

End Interface
