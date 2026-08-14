Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Analizor pentru <c>adobe_preview.log</c>, scris de <c>AdobeHostLog.Write</c>:
''' <code>2026-08-13 14:22:31.123  mesaj</code>
''' — marcaj de timp, DOUĂ spații (separatorul verificat în scriitor), apoi mesajul, o linie pe
''' intrare. Jurnal de LUCRU, nu de erori, deci nivelul e <c>Info</c>: aici ajunge ce s-a DECIS,
''' nu ce a eșuat.
''' </summary>
Public NotInheritable Class AdobeHostParser
    Implements ILogEntryParser

    Private Shared ReadOnly _rx As New Regex(
        "^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s{2}(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "AdobeHost"
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

        Dim stamp As Date
        If Not Date.TryParseExact(m.Groups("ts").Value, "yyyy-MM-dd HH:mm:ss.fff",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, stamp) Then
            Return False
        End If

        result = New LogEntry(stamp, KBotLogLevel.Info, String.Empty, m.Groups("msg").Value.Trim(), line)
        Return True
    End Function

End Class
