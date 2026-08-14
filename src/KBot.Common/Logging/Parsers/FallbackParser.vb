Option Strict On

''' <summary>
''' Ultima plasă: fiecare linie neagolă e propria ei intrare, <c>Info</c>, fără marcaj de timp.
''' NU eșuează niciodată.
'''
''' Există ca vizualizatorul să arate ORICE fișier, inclusiv unul într-un format pe care nimeni
''' nu l-a prevăzut. Un fișier nerecunoscut trebuie să se vadă ca text, nu să dispară.
''' </summary>
Public NotInheritable Class FallbackParser
    Implements ILogEntryParser

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "Fallback"
        End Get
    End Property

    ''' <summary>O intrare pe linie, prin definiție.</summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return True
        End Get
    End Property

    Public Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean Implements ILogEntryParser.TryParseHeader
        ' Liniile goale rămân continuare, ca peste tot: o linie goală nu începe o intrare.
        If String.IsNullOrWhiteSpace(line) Then Return False

        result = New LogEntry(Nothing, KBotLogLevel.Info, String.Empty, line.Trim(), line)
        Return True
    End Function

End Class
