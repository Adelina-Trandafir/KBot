Option Strict On
Imports System.Text

''' <summary>
''' O intrare de jurnal — adică un BLOC, nu o linie. O excepție scrisă de
''' <c>GlobalErrorLog</c> ocupă antetul plus zeci de linii de stivă, și toate sunt aceeași intrare.
'''
''' Analizoarele construiesc intrarea din linia de ANTET. Liniile de continuare le adaugă
''' <c>LogFileLoader</c>, singurul care are voie să modifice o intrare după construcție — de aceea
''' mutatoarele sunt <c>Friend</c>, nu <c>Public</c>: din afara ansamblului intrarea e imuabilă.
''' </summary>
Public NotInheritable Class LogEntry

    Private ReadOnly _raw As StringBuilder

    ''' <summary>
    ''' Construiește o intrare dintr-o linie de antet. <paramref name="timestamp"/> e
    ''' <c>Nothing</c> pentru formatele care nu poartă dată (fișierul de rulare al bancului de
    ''' probă nu pune marcaj de timp pe fiecare linie, de exemplu).
    ''' </summary>
    Public Sub New(timestamp As Date?,
                   level As KBotLogLevel,
                   source As String,
                   message As String,
                   rawFirstLine As String)
        Me.Timestamp = timestamp
        Me.Level = level
        Me.Source = If(source, String.Empty)
        Me.Message = If(message, String.Empty)
        _raw = New StringBuilder(If(rawFirstLine, String.Empty))
    End Sub

    ''' <summary>Marcajul de timp al intrării, sau <c>Nothing</c> dacă formatul nu poartă unul.</summary>
    Public Property Timestamp As Date?
        Get
            Return _timestamp
        End Get
        Friend Set(value As Date?)
            _timestamp = value
        End Set
    End Property
    Private _timestamp As Date?

    ''' <summary>Nivelul intrării.</summary>
    Public Property Level As KBotLogLevel
        Get
            Return _level
        End Get
        Friend Set(value As KBotLogLevel)
            _level = value
        End Set
    End Property
    Private _level As KBotLogLevel

    ''' <summary>Paranteza <c>[sursă]</c> a liniei, sau IP-ul clientului la intrările de server.</summary>
    Public Property Source As String
        Get
            Return _source
        End Get
        Friend Set(value As String)
            _source = If(value, String.Empty)
        End Set
    End Property
    Private _source As String = String.Empty

    ''' <summary>Prima linie a blocului, curățată de antet.</summary>
    Public Property Message As String
        Get
            Return _message
        End Get
        Friend Set(value As String)
            _message = If(value, String.Empty)
        End Set
    End Property
    Private _message As String = String.Empty

    ''' <summary>
    ''' Blocul ORIGINAL complet, cu tot cu liniile de continuare — ce se arată în panoul de
    ''' detaliu și ce caută filtrul de text, ca o căutare să nimerească și în interiorul unei stive.
    ''' </summary>
    Public ReadOnly Property Raw As String
        Get
            Return _raw.ToString()
        End Get
    End Property

    ''' <summary>Doar numele fișierului, fără cale.</summary>
    Public Property FileName As String
        Get
            Return _fileName
        End Get
        Friend Set(value As String)
            _fileName = If(value, String.Empty)
        End Set
    End Property
    Private _fileName As String = String.Empty

    ''' <summary>Linia de început a blocului, numerotată de la 1 ÎN FEREASTRA CITITĂ.</summary>
    Public Property LineNumber As Integer
        Get
            Return _lineNumber
        End Get
        Friend Set(value As Integer)
            _lineNumber = value
        End Set
    End Property
    Private _lineNumber As Integer

    ''' <summary>Client sau server — decide corecția de ceas și coloana «Sursă».</summary>
    Public Property Origin As LogOrigin
        Get
            Return _origin
        End Get
        Friend Set(value As LogOrigin)
            _origin = value
        End Set
    End Property
    Private _origin As LogOrigin = LogOrigin.Client

    ''' <summary>
    ''' True dacă marcajul de timp NU e al intrării, ci moștenit de la intrarea dinainte
    ''' (vezi <c>LogFileLoader</c>). Filtrul de timp trebuie să poată spune diferența, iar bara de
    ''' stare trebuie să poată număra intrările rămase fără dată.
    ''' </summary>
    Public Property TimestampInherited As Boolean
        Get
            Return _timestampInherited
        End Get
        Friend Set(value As Boolean)
            _timestampInherited = value
        End Set
    End Property
    Private _timestampInherited As Boolean

    ''' <summary>
    ''' True dacă marcajul NU poartă decalaj propriu și are deci nevoie de corecția de ceas față
    ''' de server (vezi <c>ServerClock</c>). Adevărat DOAR pentru liniile de server în forma
    ''' veche: forma nouă, ISO cu decalaj, e deja adusă în ora clientului de analizor, iar
    ''' intrările de client sunt scrise chiar de ceasul care le afișează.
    ''' </summary>
    Public Property TimestampNeedsClockCorrection As Boolean
        Get
            Return _needsClockCorrection
        End Get
        Friend Set(value As Boolean)
            _needsClockCorrection = value
        End Set
    End Property
    Private _needsClockCorrection As Boolean

    ''' <summary>Adaugă o linie de continuare la blocul brut. Doar încărcătorul cheamă asta.</summary>
    Friend Sub AppendRawLine(line As String)
        _raw.Append(Environment.NewLine).Append(If(line, String.Empty))
    End Sub

    Public Overrides Function ToString() As String
        Dim stamp As String = If(Timestamp.HasValue,
                                 Timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss.fff", Globalization.CultureInfo.InvariantCulture),
                                 "(fără dată)")
        Return stamp & " [" & Level.ToString() & "] " & Message
    End Function

End Class
