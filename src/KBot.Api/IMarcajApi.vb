Option Strict On
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' The K-BOT id markers (slice 0076): <c>POST /api/forexe/marcaj/rezerva</c>. Kept out of
''' <see cref="IApiClient"/> on purpose - one call, used by one caller (the FOREXE page, through
''' the runner's marker bridge), and every fake of the big interface would otherwise have to
''' learn it. <see cref="ApiClient"/> implements both.
''' </summary>
Public Interface IMarcajApi

    ''' <summary>
    ''' Reserves the ids the FOREXE save will become and returns the marker text the page
    ''' appends: <paramref name="tip"/> = "rezervare" -> "(IDREV: n)" (the SAME number for
    ''' every save while the angajament's reservation session is open); "receptie" ->
    ''' "(IDRH: n; IDR: m)" (a new pair every time). Throws <see cref="ApiException"/> on any
    ''' non-2xx.
    ''' </summary>
    Function RezervaMarcajAsync(tip As String, cod As String, ct As CancellationToken) As Task(Of String)

End Interface
