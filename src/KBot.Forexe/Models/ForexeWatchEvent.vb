''' <summary>
''' Which FOREXE operation the in-page watcher (ForexeWatch.js, slice 0073) saw the operator
''' perform. The string values are the ones the script sends - see <c>OPS</c> there.
''' </summary>
Public Enum ForexeOperationKind
    ''' <summary>The script sent a name this build does not know.</summary>
    Unknown = 0
    ''' <summary>«Angajament nou» ... final save (one or several indicator rows).</summary>
    Angajament = 1
    ''' <summary>A reservation row added or changed on tab0 of an open angajament.</summary>
    Rezervare = 2
    ''' <summary>A reception added on tab1 of an open angajament.</summary>
    Receptie = 3
    ''' <summary>
    ''' A reception CHANGED: armed by the eye of its row on tab1 (slice 0076), saved with the
    ''' same «Salveaza» as a new one.
    ''' </summary>
    ReceptieModificare = 4
    ''' <summary>The operator pressed Start / Gata in the floating menu themselves.</summary>
    Manual = 5
End Enum

''' <summary>What happened: the operation started, finished (saved), was cancelled, or a note.</summary>
Public Enum ForexeWatchEventKind
    Info = 0
    Started = 1
    Finished = 2
    Cancelled = 3
    ''' <summary>
    ''' Slice 0074: the page shows a different angajament now (or none). Not an operation -
    ''' <see cref="ForexeWatchEvent.CodAngajament"/> is the header code, empty when the page
    ''' has no open angajament. The shell selects that node in its tree, nothing more.
    ''' </summary>
    PageOpened = 4
End Enum

''' <summary>
''' One message from the floating K-BOT menu inside the FOREXE page. <see cref="Kind"/> =
''' Finished is the one the shell acts on: the angajament was saved between
''' <see cref="StartedAt"/> and <see cref="FinishedAt"/>, and <see cref="CodAngajament"/> is
''' what the page showed in its header (<c>.well.well-small h4 span:nth-child(2)</c>) at the
''' end - or, when that was empty, what it showed when the operation started.
''' </summary>
Public Class ForexeWatchEvent

    Public Property Kind As ForexeWatchEventKind
    Public Property Operation As ForexeOperationKind
    ''' <summary>The operation name exactly as the script sent it (for the log).</summary>
    Public Property OperationName As String = String.Empty
    ''' <summary>Operator-facing label of the operation, from the script (Romanian).</summary>
    Public Property Label As String = String.Empty
    ''' <summary>Code read from the page header when the event was raised; empty when absent.</summary>
    Public Property CodAngajament As String = String.Empty
    ''' <summary>Code read from the page header when the operation STARTED; empty when absent.</summary>
    Public Property CodLaStart As String = String.Empty
    ''' <summary>Local clock of the machine running the browser, not the FOREXE server's.</summary>
    Public Property StartedAt As Date?
    Public Property FinishedAt As Date?
    Public Property Url As String = String.Empty
    Public Property Message As String = String.Empty

    ''' <summary>
    ''' Slice 0076: what the save was about, as the page sent it (its "data" object, JSON
    ''' text); empty when there is none. A reception: <c>{dataReceptie, rowDate, formDate}</c>
    ''' (dates as zz/ll/aaaa). A reservation: <c>{indicator, indicatorCod, buget}</c> - the
    ''' indicator's row of the tab0 table and its budget table, read the way ScrapeTable reads
    ''' them. The shell parses it; this layer only carries it.
    ''' </summary>
    Public Property DetaliiJson As String = String.Empty

    ''' <summary>The code to work with: the one at the end, else the one at the start.</summary>
    Public ReadOnly Property CodEfectiv As String
        Get
            If Not String.IsNullOrWhiteSpace(CodAngajament) Then Return CodAngajament.Trim()
            If Not String.IsNullOrWhiteSpace(CodLaStart) Then Return CodLaStart.Trim()
            Return String.Empty
        End Get
    End Property

    ''' <summary>Maps the script's operation name onto the enum; anything else is Unknown.</summary>
    Public Shared Function ParseOperation(name As String) As ForexeOperationKind
        Select Case If(name, String.Empty).Trim().ToLowerInvariant()
            Case "angajament" : Return ForexeOperationKind.Angajament
            Case "rezervare" : Return ForexeOperationKind.Rezervare
            Case "receptie" : Return ForexeOperationKind.Receptie
            Case "receptie-modificare" : Return ForexeOperationKind.ReceptieModificare
            Case "manual" : Return ForexeOperationKind.Manual
            Case Else : Return ForexeOperationKind.Unknown
        End Select
    End Function

    Public Shared Function ParseKind(name As String) As ForexeWatchEventKind
        Select Case If(name, String.Empty).Trim().ToLowerInvariant()
            Case "started" : Return ForexeWatchEventKind.Started
            Case "finished" : Return ForexeWatchEventKind.Finished
            Case "cancelled" : Return ForexeWatchEventKind.Cancelled
            Case "page" : Return ForexeWatchEventKind.PageOpened
            Case Else : Return ForexeWatchEventKind.Info
        End Select
    End Function

End Class
