Option Strict On
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' The captures K-BOT takes out of the FOREXE page while the OPERATOR works there
''' (operator, 28.09.2026): <c>PUT /api/forexe/capturi/rezervare/{idrev}</c> and
''' <c>PUT /api/forexe/capturi/receptie/{idrh}</c>.
'''
''' <para>Kept out of <see cref="IApiClient"/> for the same reason as
''' <see cref="IMarcajApi"/>: one call, one caller (the shell, after the ingest), and every
''' fake of the big interface would otherwise have to learn it.</para>
'''
''' <para>The number in the address is the one the page's marker named - the DDF revision to
''' come for a reservation, the reception snapshot for a reception - and the server turns it
''' into the record the picture hangs off (<c>FX_Rezervarii_IMG</c> / <c>FX_Receptii_IMG</c>).
''' A number the server cannot place is refused, never guessed: the picture stays on disk.</para>
''' </summary>
Public Interface IForexeCapturiApi

    ''' <summary>
    ''' One picture (JPEG bytes) of a reservation session. <paramref name="idrev"/> is the
    ''' revision the page's «(IDREV: n)» named, <paramref name="cod"/> the angajament, and
    ''' <paramref name="moment"/> "inainte" or "dupa". Returns the row's key.
    ''' Throws <see cref="ApiException"/> on any non-2xx.
    ''' </summary>
    Function UrcaCapturaRezervareAsync(idrev As Integer, cod As String, nume As String,
                                       moment As String, octeti As Byte(),
                                       ct As CancellationToken) As Task(Of Integer)

    ''' <summary>
    ''' One picture of a reception. <paramref name="idrh"/> is the snapshot the page's
    ''' «(IDRH: n; IDR: m)» named, <paramref name="moment"/> "receptii" or "info-complete".
    ''' </summary>
    Function UrcaCapturaReceptieAsync(idrh As Integer, cod As String, nume As String,
                                      moment As String, octeti As Byte(),
                                      ct As CancellationToken) As Task(Of Integer)

End Interface
