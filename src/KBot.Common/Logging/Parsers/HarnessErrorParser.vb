Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Analizor pentru <c>harness_errors.log</c>, scris de <c>GlobalErrorLog.Write</c>:
''' <code>==== 2026-08-13 14:22:31.123  [Tip.Metodă] ====</code>
''' urmat de <c>ex.ToString()</c> pe câte linii are nevoie și de o linie goală.
'''
''' Nivelul e MEREU <c>Error</c>: în fișierul ăsta nu ajunge nimic altceva decât excepții.
''' Liniile de stivă de după antet sunt continuare, deci analizorul le respinge — le adună
''' încărcătorul în același bloc.
''' </summary>
Public NotInheritable Class HarnessErrorParser
    Implements ILogEntryParser

    ' "==== " + marcaj + DOUĂ spații + "[sursă] ====" (vezi GlobalErrorLog.Write).
    Private Shared ReadOnly _rx As New Regex(
        "^====\s(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s+\[(?<src>[^\]]*)\]\s====\s*$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "HarnessError"
        End Get
    End Property

    ''' <summary>Format pe BLOCURI: un antet, apoi toată stiva. Vezi nota din interfață.</summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return False
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

        ' Mesajul intrării e prima linie a lui ex.ToString(), care vine ABIA pe linia următoare;
        ' încărcătorul o pune la Message fiindcă aici îl lăsăm gol.
        result = New LogEntry(stamp, KBotLogLevel.Error, m.Groups("src").Value, String.Empty, line)
        Return True
    End Function

End Class
