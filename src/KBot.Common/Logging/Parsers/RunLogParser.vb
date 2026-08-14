Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Analizor pentru fișierul unei rulări de banc de probă, <c>test_{yyyyMMdd_HHmmss_fff}.log</c>.
'''
''' Marcajele de mai jos sunt CITITE din <c>DevHarnessForm</c> (<c>RunTestsAsync</c>,
''' <c>AppendVerdict</c>, <c>HandleUiError</c>), nu presupuse:
''' <list type="bullet">
''' <item><c>=== K-BOT Dev Harness — rulare teste ===</c> și <c>=== SUMAR: … ===</c>;</item>
''' <item>liniile de antet <c>Data      : …</c>, <c>Mașină    : …</c>, <c>AppDir    : …</c>,
''' <c>Fișier log: …</c>, <c>Teste     : …</c>;</item>
''' <item><c>── RUN [categorie] nume</c>;</item>
''' <item><c>[PASSED] nume  (12 ms)  mesaj</c>, la fel <c>FAILED</c>, <c>ERROR</c>, <c>SKIPPED</c>;</item>
''' <item><c>   · mesaj de progres</c>;</item>
''' <item><c>EROARE [sursă]: &lt;ex.ToString()&gt;</c>, ale cărui linii de stivă sunt continuare.</item>
''' </list>
'''
''' <para><b>Fișierul NU poartă marcaj de timp pe linie.</b> Singura dată din tot fișierul e pe
''' linia <c>Data      : …</c>. Ea se analizează ca marcaj, iar restul intrărilor îl moștenesc
''' prin regula de moștenire din <c>LogFileLoader</c> — altfel o rulare întreagă ar rămâne fără
''' dată și ar dispărea sub orice filtru de interval.</para>
''' </summary>
Public NotInheritable Class RunLogParser
    Implements ILogEntryParser

    Private Shared ReadOnly _rxVerdict As New Regex(
        "^\[(?<outcome>PASSED|FAILED|ERROR|SKIPPED)\]\s(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Private Shared ReadOnly _rxEroare As New Regex(
        "^EROARE\s\[(?<src>[^\]]*)\]:\s(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    ' "Data      : 2026-08-13 14:22:31"  (AppendLog din RunTestsAsync)
    Private Shared ReadOnly _rxData As New Regex(
        "^Data\s*:\s*(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s*$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "RunLog"
        End Get
    End Property

    ''' <summary>
    ''' O intrare pe linie: blocurile <c>EROARE […]</c> există, dar restul fișierului — anteturi,
    ''' linii RUN, verdicte, sumar — e linie cu linie.
    ''' </summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return True
        End Get
    End Property

    Public Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean Implements ILogEntryParser.TryParseHeader
        If String.IsNullOrEmpty(line) Then Return False

        ' Eroare de UI: antet de bloc, restul lui ex.ToString() vine ca linii de continuare.
        Dim m As Match = _rxEroare.Match(line)
        If m.Success Then
            result = New LogEntry(Nothing, KBotLogLevel.Error, m.Groups("src").Value,
                                  m.Groups("msg").Value.Trim(), line)
            Return True
        End If

        ' Verdictul unui test.
        m = _rxVerdict.Match(line)
        If m.Success Then
            result = New LogEntry(Nothing, MapOutcome(m.Groups("outcome").Value), String.Empty,
                                  m.Groups("msg").Value.Trim(), line)
            Return True
        End If

        ' Singura dată din fișier.
        m = _rxData.Match(line)
        If m.Success Then
            Dim stamp As Date
            If Date.TryParseExact(m.Groups("ts").Value, "yyyy-MM-dd HH:mm:ss",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, stamp) Then
                result = New LogEntry(stamp, KBotLogLevel.Info, String.Empty, line.Trim(), line)
                Return True
            End If
        End If

        ' Linie de stivă a unei excepții: continuare, NU antet. ex.ToString() indentează
        ' cadrele cu spații ("   at …" / "   la …") și pune "--- End of stack trace …".
        If line.StartsWith("   at ", StringComparison.Ordinal) OrElse
           line.StartsWith("   la ", StringComparison.Ordinal) OrElse
           line.TrimStart().StartsWith("--- ", StringComparison.Ordinal) Then
            Return False
        End If

        ' Progresul unui test: eveniment propriu al bancului, nu continuarea unei excepții.
        If line.StartsWith("   · ", StringComparison.Ordinal) Then
            result = New LogEntry(Nothing, KBotLogLevel.Info, String.Empty, line.Trim(), line)
            Return True
        End If

        ' Orice altă linie indentată e continuare (restul lui ex.ToString()).
        If line.StartsWith(" ", StringComparison.Ordinal) OrElse
           line.StartsWith(ControlChars.Tab, StringComparison.Ordinal) Then
            Return False
        End If

        ' Anteturi, linia RUN, sumarul, câmpurile de antet — toate intrări de sine stătătoare.
        result = New LogEntry(Nothing, KBotLogLevel.Info, String.Empty, line.Trim(), line)
        Return True
    End Function

    ''' <summary>
    ''' <c>HarnessTestOutcome</c> -&gt; nivel. <c>FAILED</c> ajunge la <c>Error</c>, nu la
    ''' <c>Warn</c>: un test picat e un rezultat pe care cineva trebuie să îl vadă, iar
    ''' diferența dintre «aserțiune picată» și «excepție» se citește oricum pe linie.
    ''' <c>SKIPPED</c> e <c>Warn</c> — nu e o eroare, dar un test care nu a rulat NU e o reușită.
    ''' </summary>
    Private Shared Function MapOutcome(outcome As String) As KBotLogLevel
        Select Case If(outcome, String.Empty).Trim().ToUpperInvariant()
            Case "PASSED" : Return KBotLogLevel.Info
            Case "SKIPPED" : Return KBotLogLevel.Warn
            Case "FAILED", "ERROR" : Return KBotLogLevel.Error
            Case Else : Return KBotLogLevel.Unknown
        End Select
    End Function

End Class
