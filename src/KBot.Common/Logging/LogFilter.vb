Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>Ce a rămas după filtrare, plus numerele pe care bara de stare le arată.</summary>
Public NotInheritable Class LogFilterResult

    Public Sub New(entries As IReadOnlyList(Of LogEntry),
                   totalCount As Integer,
                   excludedWithoutTimestamp As Integer)
        Me.Entries = entries
        Me.TotalCount = totalCount
        Me.ExcludedWithoutTimestamp = excludedWithoutTimestamp
    End Sub

    ''' <summary>Intrările păstrate, în ordinea de intrare.</summary>
    Public ReadOnly Property Entries As IReadOnlyList(Of LogEntry)

    ''' <summary>Câte intrări au INTRAT în filtru.</summary>
    Public ReadOnly Property TotalCount As Integer

    ''' <summary>
    ''' Câte intrări au fost scoase DOAR fiindcă nu au marcaj de timp, cu un capăt de interval
    ''' pus. Se raportează ca bara de stare să poată spune «3 intrări fără dată, excluse de filtrul
    ''' de timp»: o excludere tăcută arată exact ca un defect.
    ''' </summary>
    Public ReadOnly Property ExcludedWithoutTimestamp As Integer

    ''' <summary>Câte intrări se afișează.</summary>
    Public ReadOnly Property ShownCount As Integer
        Get
            Return Entries.Count
        End Get
    End Property

End Class

''' <summary>
''' Filtrul vizualizatorului de jurnale — clasă PURĂ, fără niciun tip de UI, aceeași formă ca
''' <c>IstoricFilter</c>: criterii independente, ȘI-ul lor, un <see cref="Apply"/> care întoarce
''' rezultatul. Fără I/O, deci fără Try/Catch (regula casei).
'''
''' Patru axe: fișiere, niveluri, text, interval.
'''
''' <para><b>Mulțime goală = nimic.</b> <see cref="Files"/> sau <see cref="Levels"/> goale NU
''' înseamnă «toate», înseamnă «niciuna» — o selecție validă, dar fără potriviri. Interfața
''' garantează că nu se ajunge acolo din greșeală: bara de jetoane ține cel puțin un jeton bifat
''' (<c>MinimumRequiredChecked = 1</c>). Regula e fixată cu test în ambele sensuri, fiindcă un
''' filtru care golește grila fără explicație e o reclamație care așteaptă să fie scrisă.</para>
'''
''' <para><b>Fără pliere de diacritice</b> — «sters» NU găsește «șters». Limitare cunoscută,
''' scrisă ca atare în worklog, nu reparată pe tăcute.</para>
''' </summary>
Public NotInheritable Class LogFilter

    ''' <summary>Numele de fișier acceptate. Nothing = axa inactivă; goală = nicio intrare.</summary>
    Public Property Files As ISet(Of String)

    ''' <summary>Nivelurile acceptate. Nothing = axa inactivă; goală = nicio intrare.</summary>
    Public Property Levels As ISet(Of KBotLogLevel)

    ''' <summary>Textul căutat, potrivit CASE-INSENSITIVE în blocul brut. Gol = orice.</summary>
    Public Property Text As String

    ''' <summary>Capătul de jos al intervalului, INCLUSIV. Nothing = fără capăt.</summary>
    Public Property FromDate As Date?

    ''' <summary>Capătul de sus al intervalului, INCLUSIV. Nothing = fără capăt.</summary>
    Public Property ToDate As Date?

    ''' <summary>
    ''' Aplică filtrul. Nu are efecte secundare și nu atinge intrările.
    '''
    ''' Intervalul se compară pe marcajul CORECTAT (<c>ServerClock.ToClientLocal</c>), ca o linie
    ''' de server veche să nu cadă în afara intervalului doar fiindcă ceasul serverului merge cu
    ''' trei ore înainte.
    ''' </summary>
    Public Function Apply(entries As IEnumerable(Of LogEntry)) As LogFilterResult
        Dim source As List(Of LogEntry) = If(entries Is Nothing,
                                             New List(Of LogEntry)(),
                                             entries.Where(Function(e) e IsNot Nothing).ToList())

        Dim kept As New List(Of LogEntry)()
        Dim excludedNoStamp As Integer = 0
        Dim hasRange As Boolean = FromDate.HasValue OrElse ToDate.HasValue
        Dim needle As String = If(Text, String.Empty)
        Dim hasText As Boolean = Not String.IsNullOrEmpty(needle)

        For Each e As LogEntry In source
            If Files IsNot Nothing AndAlso Not Files.Contains(e.FileName) Then Continue For
            If Levels IsNot Nothing AndAlso Not Levels.Contains(e.Level) Then Continue For

            If hasText AndAlso e.Raw.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0 Then
                Continue For
            End If

            If hasRange Then
                Dim stamp As Date? = ServerClock.ToClientLocal(e)
                If Not stamp.HasValue Then
                    ' Fără dată și cu interval pus: iese, dar se NUMĂRĂ.
                    excludedNoStamp += 1
                    Continue For
                End If
                If FromDate.HasValue AndAlso stamp.Value < FromDate.Value Then Continue For
                If ToDate.HasValue AndAlso stamp.Value > ToDate.Value Then Continue For
            End If

            kept.Add(e)
        Next

        Return New LogFilterResult(kept, source.Count, excludedNoStamp)
    End Function

End Class
