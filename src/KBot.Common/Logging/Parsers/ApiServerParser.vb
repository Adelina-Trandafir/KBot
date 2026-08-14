Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Analizor pentru jurnalele serverului Flask. DOUĂ forme, fiindcă fișierul de pe VPS e azi cel
''' vechi, iar felia 0031-02 îl schimbă pe cel nou:
'''
''' <list type="bullet">
''' <item><b>vechi</b> (<c>utils/logger.py</c> de azi, patru câmpuri, fără unitate):
''' <c>2026-08-13 14:22:31,123 - ERROR - 86.120.4.11 - mesaj</c> — marcajul e cel implicit al
''' modulului <c>logging</c> din Python, cu VIRGULĂ înaintea milisecundelor;</item>
''' <item><b>nou</b> (felia 0031-02, cinci câmpuri, ISO 8601 cu decalaj real):
''' <c>2026-08-13T14:22:31.123+03:00 - ERROR - 000_DEMO - 86.120.4.11 - mesaj</c>.</item>
''' </list>
'''
''' Forma nouă poartă decalajul ei, deci NU are nevoie de corecția de ceas; forma veche nu poartă
''' nimic și o primește (vezi <c>ServerClock</c>). De asta se păstrează
''' <see cref="LogEntry.Timestamp"/> ca oră locală a serverului la forma veche și convertită la cea
''' nouă: <see cref="UtcOffset"/> spune apelantului care e cazul.
''' </summary>
Public NotInheritable Class ApiServerParser
    Implements ILogEntryParser

    ' Nou: ISO cu T și decalaj (+03:00 sau Z), cinci câmpuri.
    Private Shared ReadOnly _rxIso As New Regex(
        "^(?<ts>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:[.,]\d+)?(?:Z|[+-]\d{2}:?\d{2}))\s-\s(?<lvl>[A-Z]+)\s-\s(?<dc>[^-]*?)\s-\s(?<ip>[^-]*?)\s-\s(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    ' Vechi: marcaj implicit Python (spațiu + virgulă la milisecunde), patru câmpuri.
    Private Shared ReadOnly _rxLegacy As New Regex(
        "^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}[,.]\d{3})\s-\s(?<lvl>[A-Z]+)\s-\s(?<ip>[^-]*?)\s-\s(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "ApiServer"
        End Get
    End Property

    ''' <summary>
    ''' O intrare pe linie. Un <c>logger.exception</c> adaugă totuși stiva sub linia lui, deci
    ''' blocurile există și aici — dar sunt excepția, nu regula, iar proba are nevoie de un prag
    ''' care să spună ceva despre fișierul obișnuit.
    ''' </summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return True
        End Get
    End Property

    Public Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean Implements ILogEntryParser.TryParseHeader
        If String.IsNullOrEmpty(line) Then Return False

        Dim m As Match = _rxIso.Match(line)
        If m.Success Then
            Dim dto As DateTimeOffset
            If DateTimeOffset.TryParse(m.Groups("ts").Value, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, dto) Then
                ' Linia își poartă decalajul: o aducem în ora locală a CLIENTULUI aici, ca grila
                ' să nu mai aibă nimic de corectat.
                result = New LogEntry(dto.ToLocalTime().DateTime, MapLevel(m.Groups("lvl").Value),
                                      m.Groups("ip").Value.Trim(), m.Groups("msg").Value.Trim(), line)
                result.Origin = LogOrigin.Server
                Return True
            End If
        End If

        m = _rxLegacy.Match(line)
        If m.Success Then
            Dim stamp As Date
            Dim normalized As String = m.Groups("ts").Value.Replace(","c, "."c)
            If Date.TryParseExact(normalized, "yyyy-MM-dd HH:mm:ss.fff",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, stamp) Then
                ' Fără decalaj în linie: marcajul rămâne ora SERVERULUI, necorectată.
                ' ServerClock.ToClientLocal o corectează la afișare.
                result = New LogEntry(stamp, MapLevel(m.Groups("lvl").Value),
                                      m.Groups("ip").Value.Trim(), m.Groups("msg").Value.Trim(), line)
                result.Origin = LogOrigin.Server
                result.TimestampNeedsClockCorrection = True
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>Numele de nivel ale modulului <c>logging</c> din Python.</summary>
    Private Shared Function MapLevel(raw As String) As KBotLogLevel
        Select Case If(raw, String.Empty).Trim().ToUpperInvariant()
            Case "DEBUG" : Return KBotLogLevel.Debug
            Case "INFO" : Return KBotLogLevel.Info
            Case "WARNING", "WARN" : Return KBotLogLevel.Warn
            Case "ERROR", "CRITICAL", "FATAL" : Return KBotLogLevel.Error
            Case "NOTSET" : Return KBotLogLevel.Trace
            Case Else : Return KBotLogLevel.Unknown
        End Select
    End Function

End Class
