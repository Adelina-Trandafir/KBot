Option Strict On

''' <summary>
''' Decalajul dintre ceasul serverului și ceasul acestei mașini.
'''
''' Se calculează la autentificare din câmpul <c>server_time</c> al răspunsului și se REÎMPROSPĂTEAZĂ
''' din fiecare răspuns de jurnale, ca o sesiune veche de trei ore să nu afișeze pe un decalaj
''' învechit.
'''
''' <para><b>Dus-întorsul NU se compensează.</b> Jumătate din timpul de rundă e marginea de eroare;
''' pe legătura asta e sub o secundă, iar o corecție care pretinde mai multă precizie decât atât ar
''' fi o minciună politicoasă. Decalajul e o UNEALTĂ DE AFIȘARE, nu o sursă de adevăr.</para>
'''
''' <para>Liniile de server în forma nouă își poartă propriul decalaj în marcaj, deci corecția asta
''' e o REZERVĂ pentru liniile vechi — vezi <see cref="LogEntry.TimestampNeedsClockCorrection"/>.</para>
''' </summary>
Public Module ServerClock

    Private ReadOnly _gate As New Object()
    Private _offset As TimeSpan = TimeSpan.Zero
    Private _hasReading As Boolean

    ''' <summary>
    ''' Decalajul curent (ceas server minus ceas local). <see cref="TimeSpan.Zero"/> cât timp nu
    ''' s-a citit niciun <c>server_time</c> — adică «nu știm», tratat ca «nicio corecție».
    ''' </summary>
    Public ReadOnly Property Offset As TimeSpan
        Get
            SyncLock _gate
                Return _offset
            End SyncLock
        End Get
    End Property

    ''' <summary>True după prima citire reușită de <c>server_time</c>.</summary>
    Public ReadOnly Property HasReading As Boolean
        Get
            SyncLock _gate
                Return _hasReading
            End SyncLock
        End Get
    End Property

    ''' <summary>Înregistrează un <c>server_time</c> proaspăt și recalculează decalajul.</summary>
    Public Sub Update(serverTime As DateTimeOffset)
        SyncLock _gate
            _offset = serverTime - DateTimeOffset.Now
            _hasReading = True
        End SyncLock
    End Sub

    ''' <summary>Uită decalajul (deconectare / test). Revine la «nu știm».</summary>
    Public Sub Reset()
        SyncLock _gate
            _offset = TimeSpan.Zero
            _hasReading = False
        End SyncLock
    End Sub

    ''' <summary>
    ''' Marcajul unei intrări, adus în ora locală a clientului.
    '''
    ''' Corecția se aplică DOAR intrărilor care chiar au nevoie de ea: o linie de server veche,
    ''' fără decalaj propriu. O linie de client, sau una nouă pe care analizorul a convertit-o deja,
    ''' se întoarce neatinsă.
    ''' </summary>
    Public Function ToClientLocal(entry As LogEntry) As Date?
        If entry Is Nothing Then Return Nothing
        If Not entry.Timestamp.HasValue Then Return Nothing
        If Not entry.TimestampNeedsClockCorrection Then Return entry.Timestamp
        Return entry.Timestamp.Value - Offset
    End Function

    ''' <summary>
    ''' Decalajul ca text pentru bara de stare: <c>+03:00</c> / <c>-01:30</c>.
    ''' Gol când nu s-a citit nimic sau când decalajul e sub o secundă — un «+00:00» permanent
    ''' într-o bară de stare e zgomot.
    ''' </summary>
    Public Function OffsetText() As String
        Dim value As TimeSpan = Offset
        If Not HasReading Then Return String.Empty
        If Math.Abs(value.TotalSeconds) < 1.0 Then Return String.Empty
        Dim sign As String = If(value.Ticks < 0L, "-", "+")
        Dim abs As TimeSpan = If(value.Ticks < 0L, value.Negate(), value)
        Return sign & abs.Hours.ToString("00", Globalization.CultureInfo.InvariantCulture) &
               ":" & abs.Minutes.ToString("00", Globalization.CultureInfo.InvariantCulture)
    End Function

End Module
