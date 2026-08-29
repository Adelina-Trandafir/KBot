Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.App
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Testele 21–24 din <c>docs/PLAN_LogViewer.md</c> §10, pentru <c>LogViewerForm</c> (felia 0031-04):
''' fereastra se construiește cu toate controalele cerute; culorile rândurilor vin din PALETĂ, nu
''' din constante; selecția pune blocul BRUT în panoul de detaliu; iar un apel de server picat lasă
''' fișierele locale întregi și scoate o notificare.
'''
''' Totul rulează pe un fir STA dedicat și FĂRĂ disc: intrările intră prin cârligul
''' <c>DebugIncarcaIntrari</c>, deci niciun test nu depinde de ce se află în <c>Logs\</c>.
'''
''' Ce NU pot dovedi: cum ARATĂ fereastra. Verdictul vizual e proba din banc
''' (<c>LogViewerTest</c>), consemnată ca NERULATĂ în worklog.
''' </summary>
Public Class LogViewerFormTests

    ' Client API care pică pe orice cerere — cazul din testul 24.
    Private NotInheritable Class FailingApiClient
        Implements IApiClient

        ' Felia 0048-02: ingestia FOREXE. Nefolosita de vederile astea.
        Public Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat,
                                               alegeri As IReadOnlyList(Of AlegereUnitate),
                                               ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.TrimitePrelucrareAsync
            Throw New NotSupportedException()
        End Function

        Public Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                           alegeri As IReadOnlyList(Of AlegereUnitate),
                                           ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.CerePropunereAsync
            Throw New NotSupportedException()
        End Function

        Public Function SalveazaAsociereaAsync(rezultat As PrelucrareRezultat,
                                               amprenta As String,
                                               decizii As IReadOnlyList(Of DecizieAsociere),
                                               alegeri As IReadOnlyList(Of AlegereUnitate),
                                               ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.SalveazaAsociereaAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAsociereAsync(cod As String, ct As CancellationToken) _
            As Task(Of AsociereStare) Implements IApiClient.GetAsociereAsync
            Throw New NotSupportedException()
        End Function

        Public Function SalveazaLegaturiAsync(cod As String,
                                              amprenta As String,
                                              comenzi As IReadOnlyList(Of ComandaAsociere),
                                              ct As CancellationToken) As Task(Of AsociereRezultat) _
            Implements IApiClient.SalveazaLegaturiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) _
            Implements IApiClient.GetAsync
            Return Task.FromException(Of T)(New ApiException("Serverul nu răspunde.", 503, "SERVER_DOWN"))
        End Function

        ' --- restul contractului: nefolosit aici ---
        Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest,
                                                          ct As CancellationToken) As Task(Of TResponse) _
            Implements IApiClient.PostAsync
            Throw New NotSupportedException()
        End Function

        Public Function UpsertAngajamenteAsync(dbName As String, rows As IReadOnlyList(Of Angajament),
                                               ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.UpsertAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                            ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) _
            Implements IApiClient.GetAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                     ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) _
            Implements IApiClient.GetTreeAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
            Implements IApiClient.GetSumarAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
            Implements IApiClient.GetIstoricAsync
            Throw New NotSupportedException()
        End Function

        ' Felia 0033: vederea ORD nu e exercitată de acest dublu — contractul cere metoda,
        ' deci o refuzăm zgomotos, ca pe celelalte neatinse.
        Public Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo) _
            Implements IApiClient.GetOrdAsync
            Throw New NotSupportedException()
        End Function

        ' Felia 0041: rutele de PDF semnat nu sunt exercitate de acest dublu — contractul cere
        ' metodele, deci le refuzăm zgomotos, ca pe celelalte neatinse.
        Public Function DownloadDdfPdfAsync(idrev As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadDdfPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function DownloadOrdPdfAsync(idordp As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadOrdPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function UploadDdfPdfAsync(idrev As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadDdfPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function UploadOrdPdfAsync(idordp As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadOrdPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.ProcessExcelAsync
            Throw New NotSupportedException()
        End Function
    End Class

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    Private Shared Function FindControl(Of T As Class)(root As Control) As T
        For Each c As Control In root.Controls
            Dim hit As T = TryCast(c, T)
            If hit IsNot Nothing Then Return hit
            Dim nested As T = FindControl(Of T)(c)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function Intrare(level As KBotLogLevel, mesaj As String, brut As String) As LogEntry
        ' FileName / LineNumber / Origin au setter FRIEND: le scrie doar LogFileLoader, din
        ' KBot.Common. Testele de aici nu au nevoie de ele — filtrarea pe fișier nu e pe drumul
        ' testat, iar Origin implicit e Client, exact cazul unui jurnal local.
        Return New LogEntry(New Date(2026, 8, 14, 10, 0, 0), level, "sursa", mesaj, brut)
    End Function

    ' ── 21. Fereastra se construiește cu toate controalele cerute ─────────────

    <Fact>
    Public Sub Fereastra_MonteazaJetoaneleListaGrilaSiDetaliul()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       Assert.NotNull(FindControl(Of KBotChipBar)(f))
                       Assert.NotNull(FindControl(Of KBotNavList)(f))
                       Assert.NotNull(FindControl(Of KBotDataView)(f))
                       Assert.NotNull(FindControl(Of KBotCaptionBar)(f))
                       Assert.NotNull(FindControl(Of KBotBusyBar)(f))
                       Assert.NotNull(FindByName(f, "txtDetaliu"))
                       Assert.NotNull(FindByName(f, "txtCauta"))
                       Assert.NotNull(FindByName(f, "noticeServer"))
                       Assert.NotNull(FindByName(f, "noticeGol"))

                       ' Cele șase coloane cerute de plan, în ordine, cu «Ora» înghețată.
                       Dim g As KBotDataView = FindControl(Of KBotDataView)(f)
                       Dim chei As String() = g.Columns.Select(Function(c) c.Key).ToArray()
                       Assert.Equal(New String() {"ora", "nivel", "sursa", "fisier", "detaliu", "mesaj"}, chei)
                       Assert.Equal(1, g.FrozenColumnCount)
                       Assert.True(g.ReadOnlyGrid)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ToateButoaneleDeActiune_SuntMontateInFereastra()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       ' Regresie reală: «Golește jurnale…» a fost la un moment dat DECLARAT în
                       ' designer fără să fie construit și adăugat nicăieri. Formularul compila,
                       ' se deschidea, arăta bine — și singurul drum de ștergere pur și simplu nu
                       ' exista. Un câmp declarat nu e un control montat.
                       For Each nume As String In {"btnCopiaza", "btnExporta", "btnDeschideDosar",
                                                   "btnGoleste", "btnReimprospateaza"}
                           Assert.True(FindByName(f, nume) IsNot Nothing,
                                       "Butonul «" & nume & "» nu e montat în fereastră.")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Jetoanele_AuCeleSaseNiveluri_SiCelPutinUnulRamaneBifat()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       Dim bar As KBotChipBar = FindControl(Of KBotChipBar)(f)
                       ' Jetoanele se construiesc în CONSTRUCTOR, nu în Load: filtrul citește
                       ' nivelurile bifate, iar o fereastră fără jetoane ar filtra pe mulțimea
                       ' goală — adică n-ar arăta nimic (vezi LogFilter).
                       Assert.Equal(New String() {"err", "wrn", "inf", "dbg", "trc", "unk"},
                                    bar.Chips.Select(Function(c) c.Key).ToArray())
                       Assert.All(bar.Chips, Sub(c) Assert.True(c.Checked))
                       ' Pragul e autorat în designer și e regula care ține grila plină de sens:
                       ' cu zero niveluri bifate, LogFilter nu arată NIMIC.
                       Assert.Equal(1, bar.MinimumRequiredChecked)
                   End Using
               End Sub)
    End Sub

    ' ── 22. Culorile rândurilor vin din PALETĂ ────────────────────────────────

    <Fact>
    Public Sub Colorarea_RandurilorUrmeazaPaletaActiva()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       Dim p As ThemePalette = ThemeManager.Current.Palette
                       Dim asteptat As New Dictionary(Of KBotLogLevel, Color) From {
                           {KBotLogLevel.[Error], p.ErrorColor},
                           {KBotLogLevel.Warn, p.WarningColor},
                           {KBotLogLevel.Debug, p.TextDimColor},
                           {KBotLogLevel.Trace, p.DisabledTextColor},
                           {KBotLogLevel.Unknown, p.TextDimColor}}

                       For Each kv In asteptat
                           Dim rand As New KBotDataRow()
                           rand.Tag = Intrare(kv.Key, "m", "brut")
                           Dim e As New KBotRowFormattingEventArgs() With {
                               .Row = rand, .ForeColor = Color.HotPink, .BackColor = Color.White, .Enabled = True}
                           f.DebugFormateazaRand(e)
                           Assert.Equal(kv.Value, e.ForeColor)
                       Next

                       ' Info NU se vopsește: e rândul obișnuit, iar a-l colora ar face din normal o
                       ' excepție. Culoarea pre-umplută de grilă rămâne neatinsă.
                       Dim randInfo As New KBotDataRow()
                       randInfo.Tag = Intrare(KBotLogLevel.Info, "m", "brut")
                       Dim eInfo As New KBotRowFormattingEventArgs() With {
                           .Row = randInfo, .ForeColor = Color.HotPink, .BackColor = Color.White, .Enabled = True}
                       f.DebugFormateazaRand(eInfo)
                       Assert.Equal(Color.HotPink, eInfo.ForeColor)
                   End Using
               End Sub)
    End Sub

    ' ── 23. Selecția pune blocul BRUT în panoul de detaliu ────────────────────

    <Fact>
    Public Sub Selectia_PuneBlocul_BRUT_InPanoulDeDetaliu()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       Dim brut As String = "==== 14.08.2026 10:00:00  [MainForm.X] ====" & Environment.NewLine &
                                            "System.InvalidOperationException: ceva" & Environment.NewLine &
                                            "   la K-BOT.MainForm.X()"
                       f.DebugIncarcaIntrari(New List(Of LogEntry) From {
                           Intrare(KBotLogLevel.[Error], "System.InvalidOperationException: ceva", brut)})

                       Assert.Equal(1, f.DebugNumarRanduri())
                       f.DebugSelecteazaRand(0)
                       ' NECONVERTIT: exact ce s-a scris în fișier, cu tot cu urma de stivă.
                       Assert.Equal(brut, f.DebugTextDetaliu())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Filtrarea_DupaNivel_ScoateRandurileNebifate()
        RunSta(Sub()
                   Using f As New LogViewerForm()
                       f.DebugIncarcaIntrari(New List(Of LogEntry) From {
                           Intrare(KBotLogLevel.[Error], "e", "e"),
                           Intrare(KBotLogLevel.Info, "i", "i")})
                       Assert.Equal(2, f.DebugNumarRanduri())

                       Dim bar As KBotChipBar = FindControl(Of KBotChipBar)(f)
                       bar.SetChecked("inf", False)
                       Assert.Equal(1, f.DebugNumarRanduri())
                   End Using
               End Sub)
    End Sub

    ' ── 24. Un server picat nu dărâmă vizualizatorul ──────────────────────────

    <Fact>
    Public Sub ServerPicat_LasaFisiereleLocaleIntregi_SiScoateONotificare()
        RunSta(Sub()
                   Using f As New LogViewerForm(New FailingApiClient())
                       f.DebugIncarcaIntrari(New List(Of LogEntry) From {
                           Intrare(KBotLogLevel.[Error], "local", "local")})
                       Assert.Equal(1, f.DebugNumarRanduri())
                       Assert.False(f.DebugNoticeServerAfisat())

                       f.DebugAduListaServerAsync().GetAwaiter().GetResult()

                       Assert.True(f.DebugNoticeServerAfisat())
                       ' Intrările locale sunt neatinse: un API mort nu golește grila.
                       Assert.Equal(1, f.DebugNumarRanduri())
                       f.DebugSelecteazaRand(0)
                       Assert.Equal("local", f.DebugTextDetaliu())
                   End Using
               End Sub)
    End Sub

End Class
