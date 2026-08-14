Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Analizor pentru <c>log_{treeId}.txt</c>, scris de <c>TreeLogger.Write</c>:
''' <code>[14:22:31.123] [12.345s] [INFO ] [sursă] mesaj</code>
'''
''' Trei lucruri verificate în scriitor, nu presupuse:
''' <list type="bullet">
''' <item>nivelul e completat cu spații la 5 caractere (<c>INFO </c>, <c>WARN </c>, <c>ERR  </c>),
''' deci se compară TĂIAT, nu literal;</item>
''' <item>paranteza sursei LIPSEȘTE cu totul când sursa e goală — nu e o paranteză vidă;</item>
''' <item>marcajul e DOAR ORA. Data nu se scrie nicăieri în fișier.</item>
''' </list>
'''
''' <para>Data se ia din <c>LastWriteTime</c>-ul fișierului, dată constructorului. Asta înseamnă
''' că un fișier care traversează miezul nopții primește data ULTIMEI scrieri pentru toate
''' intrările lui: orele care «merg înapoi» rămân pe aceeași dată, în loc să fie ghicite. O
''' ghicire ar fi mai frumoasă și uneori greșită; asta e doar uneori greșită, dar declarat.</para>
''' </summary>
Public NotInheritable Class TreeLoggerParser
    Implements ILogEntryParser

    Private Shared ReadOnly _rx As New Regex(
        "^\[(?<ts>\d{2}:\d{2}:\d{2}\.\d{3})\]\s\[(?<el>[\d.,]+)s\]\s\[(?<lvl>[^\]]*)\]\s(?:\[(?<src>[^\]]*)\]\s)?(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Private ReadOnly _fileDate As Date

    ''' <summary>
    ''' <paramref name="fileDate"/> e data de care se leagă orele din fișier — în practică
    ''' <c>FileInfo.LastWriteTime</c>. Doar partea de dată se folosește.
    ''' </summary>
    Public Sub New(fileDate As Date)
        _fileDate = fileDate.Date
    End Sub

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "TreeLogger"
        End Get
    End Property

    ''' <summary>O intrare pe linie.</summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return True
        End Get
    End Property

    Public Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean Implements ILogEntryParser.TryParseHeader
        If String.IsNullOrEmpty(line) Then Return False

        Dim m As Match = _rx.Match(line)
        If Not m.Success Then Return False

        Dim timeOfDay As Date
        If Not Date.TryParseExact(m.Groups("ts").Value, "HH:mm:ss.fff",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, timeOfDay) Then
            Return False
        End If

        Dim stamp As Date = _fileDate.Add(timeOfDay.TimeOfDay)
        Dim level As KBotLogLevel = MapLevel(m.Groups("lvl").Value)

        result = New LogEntry(stamp, level, m.Groups("src").Value, m.Groups("msg").Value.Trim(), line)
        Return True
    End Function

    ''' <summary>Numele scrise de <c>TreeLogger.LogLevel</c>, completate cu spații la 5 caractere.</summary>
    Private Shared Function MapLevel(raw As String) As KBotLogLevel
        Select Case If(raw, String.Empty).Trim().ToUpperInvariant()
            Case "DEBUG" : Return KBotLogLevel.Debug
            Case "INFO" : Return KBotLogLevel.Info
            Case "WARN" : Return KBotLogLevel.Warn
            Case "ERR", "ERROR" : Return KBotLogLevel.Error
            Case Else : Return KBotLogLevel.Unknown
        End Select
    End Function

End Class
