Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Vizualizatorul de jurnale (felia 0031-04): fișierele din stânga, intrările în grilă, blocul brut
''' al intrării selectate dedesubt, iar deasupra tuturor filtrele — jetoane de nivel, text, interval.
'''
''' <para>Tot ce ține de CITIT și de ÎNȚELES un jurnal stă în <c>KBot.Common\Logging</c> (felia
''' 0031-01) și e pur: <c>LogPaths</c>, <c>LogFileLoader</c>, <c>LogFilter</c>, <c>ServerClock</c>.
''' Formularul ăsta nu analizează nimic — doar leagă nucleul acela de controalele casei.</para>
'''
''' <para><b>Citirea nu stă niciodată pe firul UI.</b> Fișierele se citesc și se analizează pe un fir
''' de fundal (un jurnal poate avea 5 MB), iar FILTRAREA e în memorie și sincronă: nu recitește
''' niciodată un fișier și nu re-cere nimic de la server. De aceea căutarea are un cronometru de
''' 250 ms — altfel fiecare literă ar reface filtrarea peste zeci de mii de intrări.</para>
'''
''' <para><b>Serverul e opțional și se încarcă la cerere.</b> Fără <c>IApiClient</c> (bancul de
''' probă) grupul «Server» nici nu apare; cu el, lista de fișiere se cere abia când operatorul o
''' apasă, iar un eșec se scrie într-un <c>KBotNotice</c> și lasă fișierele locale să meargă mai
''' departe. Un API mort nu are voie să tragă vizualizatorul după el.</para>
'''
''' <para><b>Ce NU face:</b> nu urmărește fișierele în timp real (fără <c>FileSystemWatcher</c>), nu
''' scrie nimic pe server (rutele sunt doar de citire) și nu șterge nimic în afara drumului explicit
''' din <see cref="btnGoleste_Click"/>, care cere confirmare de două ori.</para>
''' </summary>
Public Class LogViewerForm

    ' ── Chei de navigare ──────────────────────────────────────────────────────
    Private Const KEY_TOATE As String = "toate"
    Private Const KEY_SERVER_LISTA As String = "srv__lista"
    Private Const PREFIX_LOCAL As String = "loc:"
    Private Const PREFIX_SERVER As String = "srv:"

    ' Cheile jetoanelor de nivel = numele nivelurilor, ca traducerea într-un set de KBotLogLevel
    ' să fie o singură căutare, nu o hartă ținută pe de rost în două locuri.
    Private Shared ReadOnly NIVELURI As (Key As String, Text As String, Level As KBotLogLevel)() = {
        ("err", "Erori", KBotLogLevel.[Error]),
        ("wrn", "Avertismente", KBotLogLevel.Warn),
        ("inf", "Informații", KBotLogLevel.Info),
        ("dbg", "Depanare", KBotLogLevel.Debug),
        ("trc", "Urmărire", KBotLogLevel.Trace),
        ("unk", "Necunoscut", KBotLogLevel.Unknown)}

    ''' <summary>Clientul API pentru jurnalele de server. Nothing = doar fișiere locale.</summary>
    Private ReadOnly _api As IApiClient

    ' Intrările încărcate pentru selecția curentă (înainte de filtrare).
    Private _incarcate As New List(Of LogEntry)()
    ' Numele fișierelor server aduse de la /api/logs/files (gol până se cere lista).
    Private _fisiereServer As New List(Of String)()
    ' Câte octeți au fost citiți și dacă vreun fișier a fost tăiat la fereastra de citire.
    Private _octetiCititi As Long
    Private _taiat As Boolean
    ' Selecția curentă din listă (cheia de nav).
    Private _selectie As String = KEY_TOATE
    ' Cât timp umplem controalele programatic, evenimentele lor nu trebuie să refiltreze.
    Private _suprimaEvenimente As Boolean
    ' O singură încărcare pe rând: a doua o anulează pe prima (operatorul a schimbat fișierul).
    Private _cts As CancellationTokenSource

    ''' <summary>Vizualizator fără server — bancul de probă și orice gazdă fără API.</summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Vizualizator cu jurnale de server. <paramref name="api"/> Nothing = grupul «Server» nu apare
    ''' deloc (nu apare gol și dezactivat: un grup care nu poate funcționa nu trebuie să existe).
    ''' </summary>
    Public Sub New(api As IApiClient)
        InitializeComponent()
        _api = api
        ' Jetoanele se construiesc AICI, nu în Load: filtrul își ia nivelurile din ele, iar o
        ' fereastră fără jetoane ar filtra pe mulțimea goală — adică n-ar arăta nimic (LogFilter:
        ' «mulțime goală = nimic»). Așa, orice apelant primește o fereastră deja coerentă.
        ConstruiesteJetoane()
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' Iconița e cosmetică — lipsa ei nu împiedică deschiderea ferestrei.
            GlobalErrorLog.Write("LogViewerForm.New", ex)
        End Try
    End Sub

    ' =====================================================================
    ' DESCHIDERE
    ' =====================================================================

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ConstruiesteListaFisiere()
            AplicaCulorileJetoanelor()
            navFisiere.SelectedKey = KEY_TOATE      ' ridică SelectionChanged -> încarcă
        Catch ex As Exception
            ' Frontieră de UI (Load): un throw ar dărâma deschiderea ferestrei.
            GlobalErrorLog.Write("LogViewerForm.OnLoad", ex)
            lblStare.Text = "Vizualizatorul nu a putut porni. Detalii în jurnalul de erori."
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        ' Culoarea jetoanelor ERORI / AVERTISMENTE vine din paletă și e dată de AICI (controlul nu
        ' numește nicio culoare), deci la comutarea schemei trebuie RE-dată. Grila se repictează
        ' singură prin RowFormatting, care citește paleta la fiecare rând.
        AplicaCulorileJetoanelor()
        grila.Invalidate()
    End Sub

    Private Sub AplicaCulorileJetoanelor()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            For Each c As KBotChip In chipNiveluri.Chips
                Select Case c.Key
                    Case "err" : c.AccentOverride = p.ErrorColor
                    Case "wrn" : c.AccentOverride = p.WarningColor
                    Case Else : c.AccentOverride = Color.Empty
                End Select
            Next
            chipNiveluri.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.AplicaCulorileJetoanelor", ex)
        End Try
    End Sub

    ' Toate nivelurile bifate la deschidere: vizualizatorul se deschide arătând TOT, iar operatorul
    ' scoate ce nu-l interesează. Invers ar fi o fereastră care se deschide goală.
    Private Sub ConstruiesteJetoane()
        _suprimaEvenimente = True
        Try
            chipNiveluri.Chips.Clear()
            For Each n In NIVELURI
                chipNiveluri.AddChip(n.Key, n.Text, True)
            Next
        Finally
            _suprimaEvenimente = False
        End Try
    End Sub

    ''' <summary>
    ''' Lista din stânga: «Toate fișierele» fixat primul, apoi grupul LOCAL (fișierele din
    ''' <c>LogPaths.LogsDirectory()</c>, cele mai proaspete primele, arhivele marcate ca atare) și,
    ''' dacă există client API, grupul SERVER — cu un singur rând care ADUCE lista la cerere.
    ''' </summary>
    Private Sub ConstruiesteListaFisiere()
        _suprimaEvenimente = True
        Try
            navFisiere.Items.Clear()
            navFisiere.AddItem(KEY_TOATE, "Toate fișierele")
            navFisiere.AddSeparator()

            For Each f As FileInfo In FisiereLocale()
                navFisiere.AddItem(PREFIX_LOCAL & f.Name, EtichetaFisier(f))
            Next

            If _api IsNot Nothing Then
                navFisiere.AddSeparator()
                navFisiere.AddItem(KEY_SERVER_LISTA, "Server: adu lista…")
                For Each nume As String In _fisiereServer
                    navFisiere.AddItem(PREFIX_SERVER & nume, "Server: " & nume)
                Next
            End If
        Finally
            _suprimaEvenimente = False
        End Try
    End Sub

    ''' <summary>
    ''' Fișierele de jurnal locale, cele mai recent scrise primele. Un director inexistent nu e o
    ''' eroare: e o instalare în care nu s-a scris încă niciun jurnal.
    ''' </summary>
    Private Shared Function FisiereLocale() As List(Of FileInfo)
        Try
            Dim dir As New DirectoryInfo(LogPaths.LogsDirectory())
            If Not dir.Exists Then Return New List(Of FileInfo)()
            Dim toate As New List(Of FileInfo)()
            For Each tipar As String In {"*.log", "*.log.1", "*.log.2", "*.log.3", "*.log.4", "*.log.5", "log_*.txt"}
                toate.AddRange(dir.GetFiles(tipar))
            Next
            Return toate.
                GroupBy(Function(f) f.Name, StringComparer.OrdinalIgnoreCase).
                Select(Function(g) g.First()).
                OrderByDescending(Function(f) f.LastWriteTime).
                ToList()
        Catch ex As Exception
            ' Frontieră de I/O chemată din construirea listei (o frontieră de UI deja împachetată):
            ' logăm și întoarcem gol, ca fereastra să se deschidă și să spună «niciun fișier».
            GlobalErrorLog.Write("LogViewerForm.FisiereLocale", ex)
            Return New List(Of FileInfo)()
        End Try
    End Function

    ' «harness_errors.log» / «harness_errors.log.2 (arhivă)» — arhiva se vede din etichetă.
    Private Shared Function EticheteazaArhiva(nume As String) As Boolean
        Dim ext As String = Path.GetExtension(nume)
        Dim gen As Integer
        Return ext.Length > 1 AndAlso Integer.TryParse(ext.Substring(1), gen) AndAlso gen >= 1 AndAlso gen <= 5
    End Function

    Private Shared Function EtichetaFisier(f As FileInfo) As String
        If EticheteazaArhiva(f.Name) Then Return f.Name & " (arhivă)"
        Return f.Name
    End Function

    ' =====================================================================
    ' ÎNCĂRCARE
    ' =====================================================================

    Private Sub navFisiere_SelectionChanged(key As String) Handles navFisiere.SelectionChanged
        If _suprimaEvenimente Then Return
        Try
            If String.Equals(key, KEY_SERVER_LISTA, StringComparison.Ordinal) Then
                ' Rândul ăsta nu e un fișier: e butonul care ADUCE lista de fișiere de pe server.
                Dim ignorat As Task = AduListaServerAsync()
                Return
            End If
            _selectie = key
            Dim ignorat2 As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.navFisiere_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub btnReimprospateaza_Click(sender As Object, e As EventArgs) Handles btnReimprospateaza.Click
        Try
            ' Reîmprospătarea reia și lista de fișiere: între timp rotația poate fi creat o arhivă nouă.
            Dim curenta As String = _selectie
            ConstruiesteListaFisiere()
            _suprimaEvenimente = True
            Try
                navFisiere.SelectedKey = If(navFisiere.Items.Any(Function(i) String.Equals(i.Key, curenta, StringComparison.Ordinal)),
                                            curenta, KEY_TOATE)
                _selectie = navFisiere.SelectedKey
            Finally
                _suprimaEvenimente = False
            End Try
            Dim ignorat As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.btnReimprospateaza_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Încarcă selecția curentă: citirea și analiza pe un fir de fundal, umplerea pe firul UI.
    ''' O nouă încărcare o anulează pe cea în curs — operatorul a schimbat fișierul, răspunsul
    ''' vechi n-are ce căuta în grilă.
    ''' </summary>
    Private Async Function IncarcaSelectiaAsync() As Task
        _cts?.Cancel()
        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim ct As CancellationToken = _cts.Token
        Dim cerut As String = _selectie

        busy.Running = True
        lblStare.Text = "Se încarcă…"
        noticeGol.Clear()
        noticeGol.Visible = False
        Try
            Dim rezultat As IncarcareRezultat
            If cerut.StartsWith(PREFIX_SERVER, StringComparison.Ordinal) Then
                rezultat = Await IncarcaServerAsync(cerut.Substring(PREFIX_SERVER.Length), ct)
            Else
                Dim cai As List(Of String) = CaiPentru(cerut)
                rezultat = Await Task.Run(Function() CitesteFisiere(cai, ct), ct)
            End If

            If ct.IsCancellationRequested OrElse Not String.Equals(cerut, _selectie, StringComparison.Ordinal) Then Return

            _incarcate = rezultat.Entries
            _octetiCititi = rezultat.Bytes
            _taiat = rezultat.Truncated
            ActualizeazaBadgeuri()
            AplicaFiltrul()
        Catch ex As OperationCanceledException
            ' Anularea e drum normal (a doua selecție), nu eroare.
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.IncarcaSelectiaAsync", ex)
            _incarcate = New List(Of LogEntry)()
            grila.ClearRows()
            AratăGol("Jurnalul nu a putut fi citit: " & ex.Message)
            lblStare.Text = "Încărcare eșuată."
        Finally
            busy.Running = False
        End Try
    End Function

    ''' <summary>Ce fișiere acoperă o cheie de nav: unul singur, sau toate cele locale.</summary>
    Private Function CaiPentru(key As String) As List(Of String)
        If key.StartsWith(PREFIX_LOCAL, StringComparison.Ordinal) Then
            Return New List(Of String) From {LogPaths.Combine(key.Substring(PREFIX_LOCAL.Length))}
        End If
        ' «Toate fișierele» = toate cele LOCALE. Jurnalele de server se cer la bucată, fiindcă
        ' fiecare e o cerere de rețea — a le aduce pe toate «ca să vedem tot» ar fi o surpriză.
        Return FisiereLocale().Select(Function(f) f.FullName).ToList()
    End Function

    ''' <summary>Ce a ieșit dintr-o încărcare: intrările plus numerele pentru bara de stare.</summary>
    Private NotInheritable Class IncarcareRezultat
        Public Property Entries As List(Of LogEntry)
        Public Property Bytes As Long
        Public Property Truncated As Boolean
    End Class

    ''' <summary>
    ''' Firul de fundal: citește și analizează fiecare fișier, apoi le coase într-o singură listă
    ''' sortată pe marcajul CORECTAT. Intrările fără marcaj rămân în ordinea din fișier, la coadă —
    ''' o dată inventată pentru ele ar fi o minciună sortabilă.
    ''' </summary>
    Private Shared Function CitesteFisiere(cai As List(Of String), ct As CancellationToken) As IncarcareRezultat
        Dim cuData As New List(Of LogEntry)()
        Dim faraData As New List(Of LogEntry)()
        Dim octeti As Long = 0
        Dim taiat As Boolean = False

        For Each cale As String In cai
            ct.ThrowIfCancellationRequested()
            Try
                Dim r As LogLoadResult = LogFileLoader.LoadFile(cale)
                octeti += r.FileLengthBytes
                taiat = taiat OrElse r.WasTruncated
                For Each en As LogEntry In r.Entries
                    If ServerClock.ToClientLocal(en).HasValue Then cuData.Add(en) Else faraData.Add(en)
                Next
            Catch ex As IOException
                ' Un fișier care nu se poate citi (șters între listare și citire, blocat exclusiv)
                ' NU oprește restul. Se loghează, ca lipsa lui să nu fie tăcută.
                GlobalErrorLog.Write("LogViewerForm.CitesteFisiere(" & cale & ")", ex)
            Catch ex As UnauthorizedAccessException
                GlobalErrorLog.Write("LogViewerForm.CitesteFisiere(" & cale & ")", ex)
            End Try
        Next

        cuData.Sort(Function(a, b) Nullable.Compare(ServerClock.ToClientLocal(a), ServerClock.ToClientLocal(b)))
        cuData.AddRange(faraData)
        Return New IncarcareRezultat With {.Entries = cuData, .Bytes = octeti, .Truncated = taiat}
    End Function

    ' =====================================================================
    ' SERVER (la cerere)
    ' =====================================================================

    ''' <summary>
    ''' Aduce lista de fișiere a unității curente (<c>GET /api/logs/files</c>) și o pune în listă.
    ''' Un eșec NU dărâmă vizualizatorul: se scrie în <c>noticeServer</c>, iar fișierele locale
    ''' rămân întregi.
    '''
    ''' <para><b>Ruta nu există încă</b> — e livrată de trecerea 0031-02, împreună cu metodele
    ''' tipizate <c>GetLogFilesAsync</c>/<c>GetLogTailAsync</c> de pe <c>ApiClient</c>. Până atunci
    ''' apelul cade pe 404 și operatorul vede exact asta scris, nu o listă goală fără explicație.</para>
    ''' </summary>
    Private Async Function AduListaServerAsync() As Task
        If _api Is Nothing Then Return
        busy.Running = True
        MarcheazaNoticeServer(False)
        Try
            Dim raspuns As LogFilesResponse =
                Await _api.GetAsync(Of LogFilesResponse)("/api/logs/files", CancellationToken.None)

            If raspuns?.ServerTime IsNot Nothing Then
                Dim st As DateTimeOffset
                If DateTimeOffset.TryParse(raspuns.ServerTime, st) Then ServerClock.Update(st)
            End If

            _fisiereServer = If(raspuns?.Files Is Nothing,
                                New List(Of String)(),
                                raspuns.Files.Where(Function(f) f IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(f.Name)).
                                              Select(Function(f) f.Name).ToList())
            ConstruiesteListaFisiere()
            If _fisiereServer.Count = 0 Then
                noticeServer.Show("Serverul nu are niciun fișier de jurnal pentru unitatea curentă.", NoticeKind.Warning)
                MarcheazaNoticeServer(True)
            End If
        Catch ex As NotImplementedException
            ' Astăzi ăsta e drumul REAL: ruta și metodele tipizate de pe ApiClient vin în trecerea
            ' 0031-02. Se spune pe șleau, nu se ascunde într-o listă goală.
            noticeServer.Show("Jurnalele de server nu sunt încă disponibile: ruta se livrează în " &
                              "trecerea 0031-02. Fișierele locale funcționează normal.", NoticeKind.Warning)
            MarcheazaNoticeServer(True)
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.AduListaServerAsync", ex)
            noticeServer.Show("Jurnalele de server nu s-au putut aduce: " & ex.Message &
                              " Fișierele locale funcționează mai departe.", NoticeKind.[Error])
            MarcheazaNoticeServer(True)
        Finally
            busy.Running = False
        End Try
    End Function

    ''' <summary>Aduce coada unui fișier de server și o trece prin același analizor ca pe unul local.</summary>
    Private Async Function IncarcaServerAsync(nume As String, ct As CancellationToken) As Task(Of IncarcareRezultat)
        If _api Is Nothing Then Return New IncarcareRezultat With {.Entries = New List(Of LogEntry)()}
        Try
            Dim raspuns As LogTailResponse =
                Await _api.GetAsync(Of LogTailResponse)("/api/logs/tail?generation=" & GeneratiaDin(nume), ct)

            If raspuns?.ServerTime IsNot Nothing Then
                Dim st As DateTimeOffset
                If DateTimeOffset.TryParse(raspuns.ServerTime, st) Then ServerClock.Update(st)
            End If

            Dim r As LogLoadResult = LogFileLoader.LoadText(If(raspuns?.Text, String.Empty), nume, Date.Today,
                                                            LogOrigin.Server,
                                                            If(raspuns IsNot Nothing, raspuns.Truncated, False),
                                                            If(raspuns IsNot Nothing, raspuns.SizeBytes, 0L))
            MarcheazaNoticeServer(False)
            Return New IncarcareRezultat With {.Entries = r.Entries.ToList(),
                                               .Bytes = r.FileLengthBytes,
                                               .Truncated = r.WasTruncated}
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.IncarcaServerAsync", ex)
            noticeServer.Show("Jurnalul de server «" & nume & "» nu s-a putut citi: " & ex.Message,
                              NoticeKind.[Error])
            MarcheazaNoticeServer(True)
            Return New IncarcareRezultat With {.Entries = New List(Of LogEntry)()}
        End Try
    End Function

    ' Generația din numele fișierului: «api_000_DEMO.log» = 0, «…log.3» = 3. Ruta cere un număr,
    ' nu un nume — clientul nu poate adresa fișierul altei unități (vezi planul §6.4).
    Private Shared Function GeneratiaDin(nume As String) As Integer
        Dim ext As String = Path.GetExtension(If(nume, String.Empty))
        Dim gen As Integer
        If ext.Length > 1 AndAlso Integer.TryParse(ext.Substring(1), gen) AndAlso gen >= 1 AndAlso gen <= 5 Then Return gen
        Return 0
    End Function

    ''' <summary>Corpul lui <c>GET /api/logs/files</c> (planul §6.4). Trecerea 0031-02 îl mută pe ApiClient.</summary>
    Private NotInheritable Class LogFilesResponse
        Public Property Files As List(Of LogFileInfoDto)
        <System.Text.Json.Serialization.JsonPropertyName("server_time")>
        Public Property ServerTime As String
    End Class

    Private NotInheritable Class LogFileInfoDto
        Public Property Name As String
        Public Property Generation As Integer
        <System.Text.Json.Serialization.JsonPropertyName("size_bytes")>
        Public Property SizeBytes As Long
        Public Property Modified As String
    End Class

    ''' <summary>Corpul lui <c>GET /api/logs/tail</c> (planul §6.4).</summary>
    Private NotInheritable Class LogTailResponse
        Public Property Text As String
        Public Property Truncated As Boolean
        <System.Text.Json.Serialization.JsonPropertyName("size_bytes")>
        Public Property SizeBytes As Long
        <System.Text.Json.Serialization.JsonPropertyName("server_time")>
        Public Property ServerTime As String
    End Class

    ' =====================================================================
    ' FILTRARE
    ' =====================================================================

    Private Sub chipNiveluri_CheckedChanged(chipKey As String) Handles chipNiveluri.CheckedChanged
        If _suprimaEvenimente Then Return
        Try
            AplicaFiltrul()
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.chipNiveluri_CheckedChanged", ex)
        End Try
    End Sub

    ' Căutarea NU refiltrează la fiecare tastă: cronometrul se reia, filtrarea vine după liniște.
    Private Sub txtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        If _suprimaEvenimente Then Return
        tmrCautare.Stop()
        tmrCautare.Start()
    End Sub

    Private Sub tmrCautare_Tick(sender As Object, e As EventArgs) Handles tmrCautare.Tick
        Try
            tmrCautare.Stop()
            AplicaFiltrul()
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.tmrCautare_Tick", ex)
        End Try
    End Sub

    Private Sub txtInterval_TextChanged(sender As Object, e As EventArgs) Handles txtDeLa.TextChanged, txtPanaLa.TextChanged
        If _suprimaEvenimente Then Return
        tmrCautare.Stop()
        tmrCautare.Start()
    End Sub

    ''' <summary>
    ''' Filtrează IN MEMORIE (niciodată o recitire) și umple grila. Rândul poartă intrarea în
    ''' <c>Tag</c>, ca selecția să umple panoul de detaliu fără nicio altă căutare.
    ''' </summary>
    Private Sub AplicaFiltrul()
        Dim filtru As New LogFilter() With {
            .Levels = NiveluriBifate(),
            .Text = txtCauta.Text,
            .FromDate = DataDin(txtDeLa.Text, False),
            .ToDate = DataDin(txtPanaLa.Text, True)}

        Dim rezultat As LogFilterResult = filtru.Apply(_incarcate)

        grila.BeginUpdate()
        Try
            grila.ClearRows()
            For Each en As LogEntry In rezultat.Entries
                Dim r As KBotDataRow = grila.AddRow()
                r.Tag = en
                Dim stamp As Date? = ServerClock.ToClientLocal(en)
                r("ora") = If(stamp.HasValue, stamp.Value.ToString("dd.MM HH:mm:ss.fff"), String.Empty)
                r("nivel") = TextNivel(en.Level)
                r("sursa") = If(en.Origin = LogOrigin.Server, "server", "local")
                r("fisier") = en.FileName
                r("detaliu") = en.Source
                r("mesaj") = en.Message
            Next
        Finally
            grila.EndUpdate()
        End Try

        ActualizeazaDetaliu(Nothing)   ' ClearRows nu ridică SelectionChanged
        ActualizeazaStare(rezultat)

        If rezultat.ShownCount = 0 Then
            AratăGol(If(_incarcate.Count = 0,
                        "Niciun jurnal de arătat. Fie nu s-a scris încă nimic, fie fișierul e gol.",
                        "Niciun rând nu trece de filtrele curente."))
        Else
            noticeGol.Visible = False
            noticeGol.Clear()
        End If
    End Sub

    Private Sub AratăGol(mesaj As String)
        noticeGol.Show(mesaj, NoticeKind.Warning)
        noticeGol.Visible = True
    End Sub

    ' Mulțimea de niveluri cerută de jetoane. Bara garantează cel puțin un jeton bifat
    ' (MinimumRequiredChecked = 1), deci mulțimea nu poate ieși goală din greșeală.
    Private Function NiveluriBifate() As ISet(Of KBotLogLevel)
        Dim nivele As New HashSet(Of KBotLogLevel)()
        For Each n In NIVELURI
            If chipNiveluri.ContainsChip(n.Key) AndAlso chipNiveluri.IsChecked(n.Key) Then nivele.Add(n.Level)
        Next
        Return nivele
    End Function

    ''' <summary>
    ''' Data dintr-un capăt de interval, în formatul operatorului (zz.ll.aaaa). Gol = fără capăt.
    ''' Capătul de SUS se duce la sfârșitul zilei: «până la 14.08» înseamnă toată ziua de 14, nu
    ''' miezul nopții — altfel filtrul ar tăia exact ziua pe care operatorul a cerut-o.
    ''' Un text nevalid se poartă tot ca «fără capăt» (se tastează literă cu literă).
    ''' </summary>
    Private Shared Function DataDin(text As String, sfarsitDeZi As Boolean) As Date?
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim d As Date
        If Not Date.TryParseExact(text.Trim(), "dd.MM.yyyy", Globalization.CultureInfo.InvariantCulture,
                                  Globalization.DateTimeStyles.None, d) Then Return Nothing
        Return If(sfarsitDeZi, d.Date.AddDays(1).AddTicks(-1), d.Date)
    End Function

    Private Shared Function TextNivel(level As KBotLogLevel) As String
        Select Case level
            Case KBotLogLevel.[Error] : Return "EROARE"
            Case KBotLogLevel.Warn : Return "AVERT."
            Case KBotLogLevel.Info : Return "INFO"
            Case KBotLogLevel.Debug : Return "DEPAN."
            Case KBotLogLevel.Trace : Return "URM."
            Case Else : Return "?"
        End Select
    End Function

    ' Badge-ul fiecărui jeton = câte intrări de nivelul ăla s-au ÎNCĂRCAT (nu câte se văd):
    ' numărul trebuie să spună ce e în fișier, altfel s-ar schimba sub degetul care filtrează.
    Private Sub ActualizeazaBadgeuri()
        For Each n In NIVELURI
            If Not chipNiveluri.ContainsChip(n.Key) Then Continue For
            ' Enumerable.Count(predicat), nu List.Count — proprietatea umbrește metoda în VB.
            Dim nivel As KBotLogLevel = n.Level
            chipNiveluri.SetBadge(n.Key, Enumerable.Count(_incarcate, Function(en) en.Level = nivel))
        Next
    End Sub

    ''' <summary>
    ''' Bara de stare: câte intrări, câte se văd, cât s-a citit, dacă s-a tăiat fereastra, decalajul
    ''' ceasului de server și — obligatoriu — câte intrări au fost excluse fiindcă n-au dată. O
    ''' excludere tăcută arată exact ca un defect.
    ''' </summary>
    Private Sub ActualizeazaStare(rezultat As LogFilterResult)
        Dim sb As New StringBuilder()
        sb.Append(rezultat.TotalCount.ToString("N0")).Append(" intrări · ")
        sb.Append(rezultat.ShownCount.ToString("N0")).Append(" afișate")
        If _octetiCititi > 0 Then
            sb.Append(" · ").Append((_octetiCititi / 1024.0 / 1024.0).ToString("N1")).Append(" MB")
        End If
        If _taiat Then sb.Append(" · doar coada fișierului")
        If ServerClock.HasReading AndAlso ServerClock.Offset <> TimeSpan.Zero Then
            sb.Append(" · ceas server ").Append(ServerClock.OffsetText())
        End If
        If rezultat.ExcludedWithoutTimestamp > 0 Then
            sb.Append(" · ").Append(rezultat.ExcludedWithoutTimestamp.ToString("N0")).
               Append(" fără dată, excluse de filtrul de timp")
        End If
        lblStare.Text = sb.ToString()
    End Sub

    ' =====================================================================
    ' GRILĂ ȘI DETALIU
    ' =====================================================================

    ''' <summary>
    ''' Culoarea rândului după nivel — DIN PALETĂ, niciodată o culoare scrisă aici (planul §1.1).
    ''' Info nu primește nimic: e rândul obișnuit, iar a-l vopsi ar face din normal o excepție.
    '''
    ''' Instanța argumentelor e REFOLOSITĂ de grilă pentru fiecare rând: nu se reține.
    ''' </summary>
    Private Sub grila_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles grila.RowFormatting
        Try
            Dim en As LogEntry = TryCast(e.Row?.Tag, LogEntry)
            If en Is Nothing Then Return
            Dim p As ThemePalette = ThemeManager.Current.Palette
            Select Case en.Level
                Case KBotLogLevel.[Error] : e.ForeColor = p.ErrorColor
                Case KBotLogLevel.Warn : e.ForeColor = p.WarningColor
                Case KBotLogLevel.Debug : e.ForeColor = p.TextDimColor
                Case KBotLogLevel.Trace : e.ForeColor = p.DisabledTextColor
                Case KBotLogLevel.Unknown : e.ForeColor = p.TextDimColor
            End Select
        Catch ex As Exception
            ' Frontieră de pictare: un throw de aici ar dărâma procesul.
            GlobalErrorLog.Write("LogViewerForm.grila_RowFormatting", ex)
        End Try
    End Sub

    Private Sub grila_SelectionChanged(sender As Object, e As EventArgs) Handles grila.SelectionChanged
        Try
            ActualizeazaDetaliu(TryCast(grila.CurrentRow?.Tag, LogEntry))
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.grila_SelectionChanged", ex)
        End Try
    End Sub

    ' Panoul de detaliu arată blocul BRUT, NECONVERTIT: ora din el e ora scrisă în fișier, chiar
    ' dacă în coloana «Ora» apare corectată. Cine citește o urmă de stivă trebuie să vadă exact ce
    ' s-a scris, nu o versiune ajutată de noi.
    Private Sub ActualizeazaDetaliu(en As LogEntry)
        txtDetaliu.Text = If(en Is Nothing, String.Empty, en.Raw)
    End Sub

    ' =====================================================================
    ' ACȚIUNI
    ' =====================================================================

    ''' <summary>Copiază rândul selectat, sau — fără selecție — tot ce e afișat acum.</summary>
    Private Sub btnCopiaza_Click(sender As Object, e As EventArgs) Handles btnCopiaza.Click
        Try
            Dim text As String = TextulAfisat()
            If String.IsNullOrEmpty(text) Then
                lblStare.Text = "Nimic de copiat."
                Return
            End If
            Clipboard.SetText(text)
            lblStare.Text = "Copiat în clipboard."
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.btnCopiaza_Click", ex)
            MessageBox.Show(Me, "Copierea nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Function TextulAfisat() As String
        Dim curent As LogEntry = TryCast(grila.CurrentRow?.Tag, LogEntry)
        If curent IsNot Nothing Then Return curent.Raw
        Dim sb As New StringBuilder()
        For i As Integer = 0 To grila.RowCount - 1
            Dim en As LogEntry = TryCast(grila.Rows(i).Tag, LogEntry)
            If en IsNot Nothing Then sb.AppendLine(en.Raw)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Exportă intrările FILTRATE, în UTF-8 cu BOM (Notepad-ul le vrea așa).</summary>
    Private Sub btnExporta_Click(sender As Object, e As EventArgs) Handles btnExporta.Click
        Try
            If grila.RowCount = 0 Then
                lblStare.Text = "Nimic de exportat."
                Return
            End If
            Using dlg As New SaveFileDialog()
                dlg.Filter = "Fișier text (*.txt)|*.txt|Toate fișierele (*.*)|*.*"
                dlg.FileName = "jurnal_" & Date.Now.ToString("yyyyMMdd_HHmmss") & ".txt"
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

                Dim sb As New StringBuilder()
                For i As Integer = 0 To grila.RowCount - 1
                    Dim en As LogEntry = TryCast(grila.Rows(i).Tag, LogEntry)
                    If en IsNot Nothing Then sb.AppendLine(en.Raw)
                Next
                File.WriteAllText(dlg.FileName, sb.ToString(), New UTF8Encoding(True))
                lblStare.Text = "Exportat în " & dlg.FileName
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.btnExporta_Click", ex)
            MessageBox.Show(Me, "Exportul nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub btnDeschideDosar_Click(sender As Object, e As EventArgs) Handles btnDeschideDosar.Click
        Try
            Dim dir As String = LogPaths.EnsureLogsDirectory()
            Diagnostics.Process.Start("explorer.exe", """" & dir & """")
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.btnDeschideDosar_Click", ex)
            MessageBox.Show(Me, "Dosarul nu s-a putut deschide: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Singurul drum care ȘTERGE. Doar fișiere LOCALE — jurnalele de server nu se șterg niciodată
    ''' din client, iar rutele sunt și rămân doar de citire. Doi pași, fiindcă nu există «înapoi».
    ''' </summary>
    Private Sub btnGoleste_Click(sender As Object, e As EventArgs) Handles btnGoleste.Click
        Try
            Using dlg As New LogClearDialog
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                lblStare.Text = dlg.Rezumat
            End Using
            ' Fișierele s-au schimbat sub noi: lista și conținutul se reiau.
            ConstruiesteListaFisiere
            _suprimaEvenimente = True
            Try
                navFisiere.SelectedKey = KEY_TOATE
                _selectie = KEY_TOATE
            Finally
                _suprimaEvenimente = False
            End Try
            Dim ignorat = IncarcaSelectiaAsync
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.btnGoleste_Click", ex)
            MessageBox.Show(Me, "Golirea nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' =====================================================================
    ' CÂRLIGE FRIEND PENTRU TESTE (headless, fără ecran și fără disc)
    ' =====================================================================
    ' Fereastra nu se arată în teste, deci Control.Visible al unui copil întoarce mereu False
    ' (nu are părinte vizibil) — de aceea starea notificării se ține și într-un steag propriu,
    ' singurul lucru pe care un test îl poate întreba cinstit.

    Private _noticeServerAfisat As Boolean

    ' Un singur loc care arată/ascunde notificarea de server, ca steagul să nu se poată despărți
    ' de control (două locuri care spun același lucru se contrazic la prima modificare).
    Private Sub MarcheazaNoticeServer(afisat As Boolean)
        _noticeServerAfisat = afisat
        noticeServer.Visible = afisat
    End Sub

    ''' <summary>Friend test hook: încarcă intrări gata făcute, sărind peste disc și rețea.</summary>
    Friend Sub DebugIncarcaIntrari(entries As IEnumerable(Of LogEntry))
        _incarcate = If(entries Is Nothing, New List(Of LogEntry)(), entries.ToList())
        ActualizeazaBadgeuri()
        AplicaFiltrul()
    End Sub

    ''' <summary>Friend test hook: trimite un rând pe drumul REAL de colorare.</summary>
    Friend Sub DebugFormateazaRand(e As KBotRowFormattingEventArgs)
        grila_RowFormatting(grila, e)
    End Sub

    ''' <summary>Friend test hook: textul din panoul de detaliu.</summary>
    Friend Function DebugTextDetaliu() As String
        Return txtDetaliu.Text
    End Function

    ''' <summary>Friend test hook: selectează un rând pe drumul real (grilă -> detaliu).</summary>
    Friend Sub DebugSelecteazaRand(index As Integer)
        grila.CurrentRowIndex = index
        grila_SelectionChanged(grila, EventArgs.Empty)
    End Sub

    ''' <summary>Friend test hook: câte rânduri trec de filtrele curente.</summary>
    Friend Function DebugNumarRanduri() As Integer
        Return grila.RowCount
    End Function

    ''' <summary>Friend test hook: aduce lista de server pe drumul real (inclusiv eșecul ei).</summary>
    Friend Function DebugAduListaServerAsync() As Task
        Return AduListaServerAsync()
    End Function

    ''' <summary>Friend test hook: e afișată notificarea de server?</summary>
    Friend Function DebugNoticeServerAfisat() As Boolean
        Return _noticeServerAfisat
    End Function

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            _cts?.Cancel()
            _cts?.Dispose()
            _cts = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

End Class
