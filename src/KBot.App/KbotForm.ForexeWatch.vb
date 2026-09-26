Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' The shell's half of the in-page watcher (slice 0073). The floating K-BOT menu inside the
''' FOREXE browser reports the operator's own operations - a new angajament, a reservation
''' row, a reception - through <c>ForexeWatch.js</c> -> <c>WorkflowExecutor</c> ->
''' <c>ForexeRunner</c> -> <c>ForexeController.OperatiuneCapturata</c>. When one FINISHES
''' (its save was confirmed by the page) the shell does, by itself, what the node's download
''' icon does: downloads from FOREXE, takes the package through the two-phase ingest, and
''' then, for every operation but a reservation, opens the angajament's history cut to the
''' minutes the operator worked. WHAT is downloaded follows the operation (slice 0076,
''' operator 23.09.2026): a reception - new or edited - brings THAT reception and the history
''' read backwards, from the page the operator is on («Receptie Editata», no search); a
''' reservation is not downloaded at each save at all - the indicator row the page read is
''' kept, the operator is asked whether they are done, and «DA» brings the history once
''' («Rezervari Editate») with the kept rows put in the same package; a new angajament or a
''' manual operation brings the whole angajament (operator, 21.09.2026).
'''
''' <para><b>A new angajament</b> has no node yet, so the angajamente list is synchronised
''' first (the same road as the tree footer icon), the codes that were not in the tree before
''' are taken as the new ones, and each is downloaded in turn - one final save makes one
''' angajament, whether it had one indicator row or several.</para>
'''
''' <para><b>One at a time.</b> A captured operation that arrives while another is still
''' being downloaded is told so and dropped: the robot has one browser, and the operator can
''' always press the node icon later.</para>
''' </summary>
Partial Public Class KbotForm

    ' A captured operation is being downloaded / ingested right now.
    Private _urmarireInLucru As Boolean

    ''' <summary>Subscribes to the coordinator; called once from Load, after Bind.</summary>
    Private Sub LeagaUrmarirea()
        AddHandler _controller.OperatiuneCapturata, AddressOf Controller_OperatiuneCapturata
    End Sub

    ''' <summary>The coordinator is a singleton: a subscription left behind would keep the shell alive.</summary>
    Private Sub DezleagaUrmarirea()
        RemoveHandler _controller.OperatiuneCapturata, AddressOf Controller_OperatiuneCapturata
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            DezleagaUrmarirea()
            DezleagaOperatiunileNecorectate()   ' slice 0084 - KbotForm.UncorrectedOperations.vb
            DezleagaBrowserul()
            DezleagaOptiunileArborelui()   ' slice 0777 - KbotForm.TreeOptions.vb
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' Comes from the Playwright callback thread: onto the UI thread, then act.
    Private Sub Controller_OperatiuneCapturata(sender As Object, ev As ForexeWatchEvent)
        Try
            If ev Is Nothing Then Return
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            BeginInvoke(Sub() TrateazaOperatiuneaCapturata(ev))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Controller_OperatiuneCapturata", ex)
        End Try
    End Sub

    ' UI boundary (async Sub started from BeginInvoke): log and tell, never rethrow.
    Private Async Sub TrateazaOperatiuneaCapturata(ev As ForexeWatchEvent)
        Try
            Select Case ev.Kind
                Case ForexeWatchEventKind.Finished
                    Await PreiaOperatiuneaAsync(ev)
                Case ForexeWatchEventKind.PageOpened
                    ' Slice 0074: the page shows another angajament - the tree follows it,
                    ' the robot stays put (KbotForm.Browser.vb).
                    TrateazaPaginaDeschisa(ev.CodAngajament)
                Case Else
                    ' Started / Cancelled / Info are already on the console, written by the
                    ' executor; nothing for the shell to do with them.
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaOperatiuneaCapturata", ex)
            KBotMessage.Show(Me, "Preluarea operațiunii din FOREXE a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' A finished operation: find the angajament code(s), download and ingest each one, then
    ''' open its history for the interval. Throws to the caller on anything unexpected.
    ''' </summary>
    Private Async Function PreiaOperatiuneaAsync(ev As ForexeWatchEvent) As Task
        If _urmarireInLucru Then
            KBotMessage.Show(Me,
                $"«{ev.Label}» s-a salvat în FOREXE, dar o operațiune anterioară e încă în lucru." &
                Environment.NewLine &
                "Descărcați angajamentul din iconița nodului când se termină.",
                "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        _urmarireInLucru = True
        Try
            Dim panaLa As Date = If(ev.FinishedAt, Date.Now)
            Dim deLa As Date = If(ev.StartedAt, panaLa)

            Dim coduri As List(Of String)
            If ev.Operation = ForexeOperationKind.Angajament Then
                coduri = Await CoduriNoiDupaSincronizareAsync(ev.CodEfectiv)
            Else
                Dim cod As String = ev.CodEfectiv
                If String.IsNullOrEmpty(cod) AndAlso _currentInfo IsNot Nothing Then
                    ' The page header was not readable at either end; the node the operator has
                    ' selected in the tree is the next best fact, and it is said out loud.
                    cod = If(_currentInfo.CodAngajament, String.Empty)
                    If Not String.IsNullOrEmpty(cod) Then
                        _controller.SpuneStare($"Pagina FOREXE nu a arătat codul; folosesc nodul selectat «{cod}».")
                    End If
                End If
                coduri = New List(Of String)()
                If Not String.IsNullOrEmpty(cod) Then coduri.Add(cod)
            End If

            If coduri.Count = 0 Then
                KBotMessage.Show(Me,
                    $"«{ev.Label}» s-a salvat în FOREXE, dar nu am putut afla codul angajamentului." &
                    Environment.NewLine &
                    "Descărcați-l din iconița nodului din listă.",
                    "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Slice 0076: a reservation is not downloaded at every save. The page's row is kept,
            ' and the operator is asked whether the reservations are done (see there).
            If ev.Operation = ForexeOperationKind.Rezervare Then
                Await PastreazaRezervareaAsync(ev, coduri(0))
                Return
            End If

            For Each cod As String In coduri
                _controller.SpuneStare($"«{ev.Label}» salvată în FOREXE — descarc «{cod}»...")
                Dim pachet As PrelucrareRezultat = Await DescarcaPentruOperatiuneAsync(ev, cod)
                ' Nothing = the robot did not start or failed; it already said why on the console.
                If pachet Is Nothing Then
                    ShowForexeFailure("FOREXE")
                    Continue For
                End If
                Await DuLaIngestieAsync(cod, pachet)
                DeschideIstoricInterval(cod, deLa, panaLa, ev.Label)
            Next
        Finally
            _urmarireInLucru = False
        End Try
    End Function

    ''' <summary>
    ''' The download that fits the operation: reservations only, receptions only, or the
    ''' whole node. Never the «which receptions to skip» question - the operator just
    ''' finished working, everything is fresh, everything of that family is wanted.
    ''' </summary>
    Private Async Function DescarcaPentruOperatiuneAsync(ev As ForexeWatchEvent,
                                                          cod As String) As Task(Of PrelucrareRezultat)
        busyBar.Running = True
        Try
            Select Case ev.Operation
                Case ForexeOperationKind.Receptie
                    ' Slice 0076: a NEW reception - the flow starts on the page the operator is
                    ' on and reads the LAST row of the list (operator, 23.09.2026).
                    Return Await _controller.DownloadReceptieEditataAsync(cod, Nothing, CitesteIstoricul())
                Case ForexeOperationKind.ReceptieModificare
                    ' An EDITED reception: the one with the date the page kept (the form's, else
                    ' the row's whose eye was pressed). Without a date the reception cannot be
                    ' told from the others, so the whole family is refreshed - slower, never wrong.
                    Dim data As Date? = DataReceptieDin(ev.DetaliiJson)
                    If data.HasValue Then
                        Return Await _controller.DownloadReceptieEditataAsync(cod, data, CitesteIstoricul())
                    End If
                    _controller.SpuneStare("Pagina nu a dat data recepției modificate; reîmprospătez toate recepțiile.")
                    Return Await _controller.DownloadReceptiiAsync(cod, Nothing)
                Case Else
                    Return Await _controller.DownloadNodeAsync(cod, CitesteIstoricul(), Nothing)
            End Select
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>The local history read every download uses for its REVERSE stop.</summary>
    Private Function CitesteIstoricul() As Func(Of String, CancellationToken, Task(Of IstoricInfo))
        Return Function(c, ct) WithReauth(Of IstoricInfo)(Function() _apiClient.GetIstoricAsync(c, ct))
    End Function

    ' ── The reservations of the «Browser FOREXE» view (slice 0076) ───────────────────────

    ''' <summary>
    ''' What the page sent after each reservation save of ONE angajament, kept until the
    ''' operator says the reservations are done: one indicator row (with its nested
    ''' «BugetIndicator») per indicator, the LAST save of an indicator winning, in the order the
    ''' indicators were first touched.
    ''' </summary>
    Private NotInheritable Class RezervariInLucru
        Private ReadOnly _randuri As New Dictionary(Of String, RandTabel)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _ordine As New List(Of String)()

        ''' <summary>How many saves were taken, including those whose row could not be read.</summary>
        Public Property Salvari As Integer

        Public ReadOnly Property Count As Integer
            Get
                Return _ordine.Count
            End Get
        End Property

        Public Sub Pune(codIndicator As String, rand As RandTabel)
            If Not _randuri.ContainsKey(codIndicator) Then _ordine.Add(codIndicator)
            _randuri(codIndicator) = rand
        End Sub

        Public Function Randuri() As IReadOnlyList(Of RandTabel)
            Return _ordine.Select(Function(k) _randuri(k)).ToList()
        End Function

        Public Function Coduri() As String
            Return String.Join(", ", _ordine)
        End Function
    End Class

    ' Angajament code -> its reservation edits not yet taken into K-BOT. Lives as long as the
    ' shell: the saves are in FOREXE whatever happens here, and a full refresh of the node
    ' brings them in anyway.
    Private ReadOnly _rezervariInLucru As New Dictionary(Of String, RezervariInLucru)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>
    ''' One reservation save (operator, 23.09.2026): the indicator row the page read is KEPT,
    ''' and the operator is asked whether they are done. «NU» = carry on, nothing is
    ''' downloaded. «DA» = «adlop - Rezervari Editate.wfl» reads the header and the history
    ''' ONCE, backwards, the kept rows go into the same package, and the package goes to the
    ''' two-phase ingest. No history window: the Rezervari view already shows what was written
    ''' (operator, 22.09.2026).
    ''' </summary>
    Private Async Function PastreazaRezervareaAsync(ev As ForexeWatchEvent, cod As String) As Task
        Dim memorie As RezervariInLucru = Nothing
        If Not _rezervariInLucru.TryGetValue(cod, memorie) Then
            memorie = New RezervariInLucru()
            _rezervariInLucru(cod) = memorie
        End If
        memorie.Salvari += 1

        Dim codIndicator As String = String.Empty
        Dim rand As RandTabel = RandIndicatorDin(ev.DetaliiJson, codIndicator)
        If rand IsNot Nothing Then
            memorie.Pune(codIndicator, rand)
            _controller.SpuneStare($"Rezervarea indicatorului «{codIndicator}» ({cod}) e păstrată în K-BOT.")
        Else
            ' The save IS in FOREXE and its history rows will come with the download; only the
            ' indicator's own row is missing, which matters for a NEW indicator (the server
            ' would not know it). Said, not hidden.
            _controller.SpuneStare($"Rândul indicatorului salvat nu s-a putut citi din pagină ({cod}); " &
                                   "dacă e un indicator nou, descărcați angajamentul din iconița nodului.")
        End If

        Dim nl As String = Environment.NewLine
        Dim raspuns As DialogResult = KBotMessage.Show(Me,
            $"Rezervarea s-a salvat în FOREXE." & nl &
            $"Pentru «{cod}» am păstrat {memorie.Count} indicator(i) modificat(i)" &
            If(memorie.Count > 0, $": {memorie.Coduri()}.", ".") & nl & nl &
            "Ați terminat modificarea rezervărilor?" & nl & nl &
            "DA — le preiau acum în K-BOT (istoricul se citește o singură dată)." & nl &
            "NU — continuați; le păstrez și vă întreb din nou după următoarea salvare.",
            "Rezervări FOREXE", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If raspuns <> DialogResult.Yes Then
            _controller.SpuneStare($"Rezervările lui «{cod}» rămân păstrate ({memorie.Salvari} salvări); continuați în FOREXE.")
            Return
        End If

        _controller.SpuneStare($"Preiau rezervările editate ale lui «{cod}»...")
        Dim pachet As PrelucrareRezultat
        busyBar.Running = True
        Try
            pachet = Await _controller.DownloadRezervariEditateAsync(cod, memorie.Randuri(), CitesteIstoricul())
        Finally
            busyBar.Running = False
        End Try
        If pachet Is Nothing Then
            ' The kept rows stay: the next save asks again, and «DA» tries again.
            ShowForexeFailure("Rezervări FOREXE")
            Return
        End If
        ' The package now carries them (and it is on disk); the memory is done with.
        _rezervariInLucru.Remove(cod)
        Await DuLaIngestieAsync(cod, pachet)
    End Function

    ''' <summary>
    ''' The indicator row of a reservation save, as a package row: the tab0 table's cells
    ''' plus «BugetIndicator» (the budget table the page read at the save), exactly the shape
    ''' of a «TabelIndicatori_results» row. Nothing when the page could not find the row.
    ''' </summary>
    Private Shared Function RandIndicatorDin(detaliiJson As String, ByRef codIndicator As String) As RandTabel
        codIndicator = String.Empty
        Try
            If String.IsNullOrWhiteSpace(detaliiJson) Then Return Nothing
            Dim obj As JsonObject = TryCast(JsonNode.Parse(detaliiJson), JsonObject)
            If obj Is Nothing Then Return Nothing
            Dim indicator As JsonObject = TryCast(obj("indicator"), JsonObject)
            If indicator Is Nothing Then Return Nothing

            Dim rand As New RandTabel()
            For Each kvp As KeyValuePair(Of String, JsonNode) In indicator
                rand.Pune(kvp.Key, TabeleJson.DinCelula(kvp.Value))
            Next
            Dim buget As JsonNode = obj("buget")
            rand.Pune(WorkflowResultStore.COL_BUGET_INDICATOR,
                      If(TypeOf buget Is JsonArray, TabeleJson.DinCelula(buget),
                         CelulaTabel.DinLista(New List(Of CelulaTabel)())))

            Dim cod As JsonNode = obj("indicatorCod")
            codIndicator = If(cod Is Nothing, String.Empty, cod.ToString()).Trim()
            If String.IsNullOrEmpty(codIndicator) Then
                Dim celula As CelulaTabel = Nothing
                If rand.TryGetValue("Indicator_ang", celula) AndAlso celula IsNot Nothing Then
                    codIndicator = celula.TextSau(String.Empty).Trim()
                End If
            End If
            If String.IsNullOrEmpty(codIndicator) Then Return Nothing
            Return rand
        Catch ex As Exception
            ' Data boundary (JSON from the page): logged, and the save is treated as unread.
            GlobalErrorLog.Write("MainForm.RandIndicatorDin", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>The date of an edited reception, from the page's data (zz/ll/aaaa); Nothing when absent.</summary>
    Private Shared Function DataReceptieDin(detaliiJson As String) As Date?
        Try
            If String.IsNullOrWhiteSpace(detaliiJson) Then Return Nothing
            Dim obj As JsonObject = TryCast(JsonNode.Parse(detaliiJson), JsonObject)
            If obj Is Nothing Then Return Nothing
            Dim nod As JsonNode = obj("dataReceptie")
            Dim text As String = If(nod Is Nothing, String.Empty, nod.ToString()).Trim()
            Dim data As Date
            If Date.TryParseExact(text, WorkflowCatalog.DataReceptieFormat,
                                  Globalization.CultureInfo.InvariantCulture,
                                  Globalization.DateTimeStyles.None, data) Then
                Return data
            End If
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DataReceptieDin", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Synchronises the angajamente list (the tree footer's road, minus its message box) and
    ''' returns the codes that were NOT in the tree before. <paramref name="codDinPagina"/>,
    ''' the code the FOREXE page showed after the save, is always included when present: the
    ''' page is the primary witness, the list diff the second.
    ''' </summary>
    Private Async Function CoduriNoiDupaSincronizareAsync(codDinPagina As String) As Task(Of List(Of String))
        Dim inainte As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each info As AngajamentTreeInfo In _treeInfos.Values
            If Not String.IsNullOrEmpty(info.CodAngajament) Then inainte.Add(info.CodAngajament)
        Next

        Dim noi As New List(Of String)()
        If Not String.IsNullOrEmpty(codDinPagina) Then noi.Add(codDinPagina)

        Dim mapate As List(Of Angajament)
        busyBar.Running = True
        Try
            mapate = Await _controller.DownloadListaAsync()
        Finally
            busyBar.Running = False
        End Try
        If mapate Is Nothing Then
            ShowForexeFailure("Listă angajamente")
            Return noi
        End If

        ' With no DbName (no login -- possible only in the Debug harness) the list cannot be
        ' written, and an angajament without a header cannot be ingested either.
        If String.IsNullOrEmpty(_session.DbName) Then
            KBotMessage.Show(Me,
                "Lista a fost descărcată, dar nu poate fi trimisă pe server: sesiunea nu are baza " &
                "unității (necesită login). Angajamentul nou nu poate fi preluat.",
                "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return New List(Of String)()
        End If

        Dim rezultat As AngajamenteAdaugate
        busyBar.Running = True
        Try
            rezultat = Await WithReauth(Of AngajamenteAdaugate)(
                Function() _apiClient.AdaugaAngajamenteNoiAsync(_session.DbName, mapate, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If rezultat.Inserate > 0 Then Await LoadTreeAsync(pastreazaSelectia:=True)

        For Each a As Angajament In mapate
            Dim cod As String = If(a.CodAngajament, String.Empty).Trim()
            If String.IsNullOrEmpty(cod) OrElse inainte.Contains(cod) Then Continue For
            If Not noi.Contains(cod, StringComparer.OrdinalIgnoreCase) Then noi.Add(cod)
        Next
        Return noi
    End Function

    ''' <summary>
    ''' The history of the angajament, cut to the operator's minutes in the browser. Modeless,
    ''' owned by the shell, disposed on close - one window per captured operation.
    ''' </summary>
    Private Sub DeschideIstoricInterval(cod As String, deLa As Date, panaLa As Date, eticheta As String)
        Try
            Dim f As New IstoricIntervalForm(_apiClient,
                                             Function(op) WithReauth(Of IstoricInfo)(op),
                                             cod, deLa, panaLa, eticheta)
            AddHandler f.FormClosed, Sub(s, e) f.Dispose()
            f.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideIstoricInterval", ex)
            KBotMessage.Show(Me, "Fereastra de istoric nu s-a putut deschide: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
