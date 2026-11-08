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
        LeagaRobotulKe()   ' slices KE05 / KE07 - KbotForm.KeRobot.vb
    End Sub

    ''' <summary>The coordinator is a singleton: a subscription left behind would keep the shell alive.</summary>
    Private Sub DezleagaUrmarirea()
        RemoveHandler _controller.OperatiuneCapturata, AddressOf Controller_OperatiuneCapturata
        DezleagaRobotulKe()   ' slices KE05 / KE07 - KbotForm.KeRobot.vb
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            DezleagaUrmarirea()
            UnbindRobotQueue()   ' slice 0098 - KbotForm.RobotQueue.vb
            DezleagaOperatiunileNecorectate()   ' slice 0084 - KbotForm.UncorrectedOperations.vb
            DezleagaActualizareaMultipla()   ' slice 0100 - KbotForm.Parallel.vb
            DezleagaBrowserul()
            DezleagaOptiunileArborelui()   ' slice 0777 - KbotForm.TreeOptions.vb
            AdobeUiPreference.Restore(AddressOf KBot.Controls.AdobeHostLog.Write)   ' slice 0078-10 - Adobe's own interface value back
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
                    ' Slice 0076-01: a reception whose DATE changed is written to disk NOW, before the
                    ' queue: the task below can be dropped (a download in progress) or fail, and the
                    ' fact is true on the site whatever happens to it. IngestFlow delivers it to the
                    ' server before any ingest of this angajament.
                    PastreazaSchimbareaDeData(ev)
                    ' Slice 0098: a robot queue task like every other, but IN FRONT of the waiting ones:
                    ' its pictures are of the page as the operator left it, and the next queued
                    ' download would move the page away.
                    Await _robotQueue.RunAsync(Nothing, $"Operațiune FOREXE «{ev.Label}»",
                                               Function() PreiaOperatiuneaAsync(ev), inFront:=True)
                Case ForexeWatchEventKind.PageOpened
                    ' Slice 0074: the page shows another angajament - the tree follows it,
                    ' the robot stays put (KbotForm.Browser.vb).
                    TrateazaPaginaDeschisa(ev.CodAngajament)
                Case Else
                    ' Started / Cancelled / Info are already on the console, written by the
                    ' executor; nothing for the shell to do with them.
            End Select
        Catch ex As RobotTaskDroppedException
            ' Taken out of the robot queue by the operator: the console already said it.
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaOperatiuneaCapturata", ex)
            KBotMessage.Show(Me, "Preluarea operațiunii din FOREXE a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0076-01: an EDITED reception saved with another date than the one its row had when the
    ''' operator opened it. The page reports both (<c>rowDate</c> / <c>formDate</c>) and the row's
    ''' identity; the fact is kept on disk (<see cref="ObservatiiReceptiiStore"/>) and reaches the
    ''' server through <c>IngestFlow</c> before the next ingest of the angajament. UI boundary: logs
    ''' and swallows - a note that could not be kept is said, the download still goes on.
    ''' </summary>
    Private Sub PastreazaSchimbareaDeData(ev As ForexeWatchEvent)
        Try
            Dim k_stearsa As Boolean = ev.Operation = ForexeOperationKind.ReceptieStergere
            If ev.Operation <> ForexeOperationKind.ReceptieModificare AndAlso Not k_stearsa Then Return
            If String.IsNullOrWhiteSpace(ev.DetaliiJson) Then Return
            Dim k_obj As JsonObject = TryCast(JsonNode.Parse(ev.DetaliiJson), JsonObject)
            If k_obj Is Nothing Then Return

            Dim k_veche As String = TextDinJson(k_obj, "rowDate")
            Dim k_noua As String = If(k_stearsa, String.Empty, TextDinJson(k_obj, "formDate"))
            If k_veche.Length = 0 Then Return
            If Not k_stearsa Then
                If k_noua.Length = 0 Then Return
                If String.Equals(k_veche, k_noua, StringComparison.Ordinal) Then Return
            End If

            Dim k_cod As String = ev.CodEfectiv
            If String.IsNullOrEmpty(k_cod) AndAlso _state.CurrentInfo IsNot Nothing Then
                k_cod = If(_state.CurrentInfo.CodAngajament, String.Empty)
            End If
            If String.IsNullOrEmpty(k_cod) Then
                CapturiLog.Write("MainForm.PastreazaSchimbareaDeData",
                                 $"Data unei recepții s-a schimbat ({k_veche} -> {k_noua}), dar codul angajamentului " &
                                 "nu se cunoaște; schimbarea nu se poate păstra.", KBotLogLevel.Warn)
                Return
            End If

            Dim k_rang As Integer
            Dim k_pozitie As Integer
            Integer.TryParse(TextDinJson(k_obj, "rowRang"), k_rang)
            Integer.TryParse(TextDinJson(k_obj, "rowPozitie"), k_pozitie)

            ObservatiiReceptiiStore.Adauga(New ObservatieReceptie() With {
                .Fel = If(k_stearsa, FelObservatieReceptie.Stearsa, FelObservatieReceptie.DataSchimbata),
                .CodAngajament = k_cod, .DataVeche = k_veche, .DataNoua = k_noua,
                .SumaVeche = TextDinJson(k_obj, "rowSuma"), .Rang = k_rang, .Pozitie = k_pozitie,
                .Moment = If(ev.FinishedAt, DateTime.Now)})
            CapturiLog.Write("MainForm.PastreazaSchimbareaDeData",
                             If(k_stearsa,
                                $"Recepția din {k_veche} (rang {k_rang}) a fost ștearsă în FOREXE «{k_cod}»; ",
                                $"Recepția din {k_veche} (rang {k_rang}) a primit data {k_noua} în FOREXE «{k_cod}»; ") &
                             "schimbarea e păstrată și se trimite la server înaintea următoarei preluări.")
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PastreazaSchimbareaDeData", ex)
            KBotMessage.Show(Me,
                "Data recepției s-a schimbat în FOREXE, dar K-BOT n-a putut păstra schimbarea: " & ex.Message &
                Environment.NewLine & "Descărcarea care urmează poate crea o recepție nouă în loc să o mute pe cea veche.",
                "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' A property of the page's data object as trimmed text; empty when absent or null.
    Private Shared Function TextDinJson(k_obj As JsonObject, k_nume As String) As String
        Dim k_nod As JsonNode = Nothing
        If Not k_obj.TryGetPropertyValue(k_nume, k_nod) OrElse k_nod Is Nothing Then Return String.Empty
        Return k_nod.ToString().Trim()
    End Function

    ''' <summary>
    ''' A finished operation: find the angajament code(s), download and ingest each one.
    ''' Throws to the caller on anything unexpected.
    ''' </summary>
    Private Async Function PreiaOperatiuneaAsync(ev As ForexeWatchEvent) As Task
        If _urmarireInLucru Then
            CapturiLog.Write("MainForm.PreiaOperatiuneaAsync",
                             $"«{ev.Label}» ({ev.Operation}, cod «{ev.CodEfectiv}») NU se preia: o operațiune " &
                             "anterioară e încă în lucru; nicio captură nu se face acum.", KBotLogLevel.Warn)
            KBotMessage.Show(Me,
                $"«{ev.Label}» s-a salvat în FOREXE, dar o operațiune anterioară e încă în lucru." &
                Environment.NewLine &
                "Descărcați angajamentul din iconița nodului când se termină.",
                "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        _urmarireInLucru = True
        Try
            Dim coduri As List(Of String)
            If ev.Operation = ForexeOperationKind.Angajament Then
                ' Slice 0097-07: a NEW angajament is known from the page that follows the save (its
                ' header). The angajamente list is no longer downloaded for it: the ingest creates
                ' the header row from the package (CoduriNoiDupaSincronizareAsync stays for the
                ' tree footer's road, off this one).
                coduri = New List(Of String)()
                If Not String.IsNullOrWhiteSpace(ev.CodEfectiv) Then coduri.Add(ev.CodEfectiv.Trim())
            Else
                Dim cod As String = ev.CodEfectiv
                If String.IsNullOrEmpty(cod) AndAlso _state.CurrentInfo IsNot Nothing Then
                    ' The page header was not readable at either end; the node the operator has
                    ' selected in the tree is the next best fact, and it is said out loud.
                    cod = If(_state.CurrentInfo.CodAngajament, String.Empty)
                    If Not String.IsNullOrEmpty(cod) Then
                        _controller.SpuneStare($"Pagina FOREXE nu a arătat codul; folosesc nodul selectat «{cod}».")
                    End If
                End If
                coduri = New List(Of String)()
                If Not String.IsNullOrEmpty(cod) Then coduri.Add(cod)
            End If

            If coduri.Count = 0 Then
                CapturiLog.Write("MainForm.PreiaOperatiuneaAsync",
                                 $"«{ev.Label}» ({ev.Operation}) NU se preia: codul angajamentului nu se cunoaște; " &
                                 "nicio captură nu se face.", KBotLogLevel.Warn)
                KBotMessage.Show(Me,
                    $"«{ev.Label}» s-a salvat în FOREXE, dar nu am putut afla codul angajamentului." &
                    Environment.NewLine &
                    "Descărcați-l din iconița nodului din listă.",
                    "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Slice 0076: a reservation is not downloaded at every save. The page's row is kept,
            ' and the operator is asked whether the reservations are done (see there).
            CapturiLog.Write("MainForm.PreiaOperatiuneaAsync",
                $"Operațiune «{ev.Label}» ({ev.Operation}) salvată în FOREXE; cod la început «{ev.CodLaStart}», " &
                $"cod efectiv «{ev.CodEfectiv}» -> se lucrează pe «{String.Join(", ", coduri)}».")

            If ev.Operation = ForexeOperationKind.Rezervare Then
                Await PastreazaRezervareaAsync(ev, coduri(0))
                Return
            End If

            ' Operator, 28.09.2026: the two pictures the ALOP guide asks for a reception
            ' (p.21-22) are taken NOW - the page is on the Receptii tab, as the guide's own
            ' example shows, and the download that follows would navigate away from it.
            If ev.Operation = ForexeOperationKind.Receptie OrElse
               ev.Operation = ForexeOperationKind.ReceptieModificare Then
                Await _captures.TakeReceptionPicturesAsync(coduri(0))
            End If

            For Each cod As String In coduri
                _controller.SpuneStare($"«{ev.Label}» salvată în FOREXE — descarc «{cod}»...")
                Dim pachet As PrelucrareRezultat = Await DescarcaPentruOperatiuneAsync(ev, cod)
                ' Nothing = the robot did not start or failed; it already said why on the console.
                If pachet Is Nothing Then
                    ShowForexeFailure("FOREXE")
                    _captures.SayNotSent(cod, "descărcarea din FOREXE nu a reușit")
                    Continue For
                End If
                ' A new angajament: the tree selects it and scrolls to it after the save (slice 0097-07).
                Dim preluat As Boolean = Await _ingest.IngestAsync(
                    cod, pachet, ev.Operation = ForexeOperationKind.Angajament)
                ' The pictures can only be filed once the ingest has written the reception
                ' they hang off; until then they wait on disk.
                If Not preluat Then
                    _captures.SayNotSent(cod, "descărcarea nu s-a salvat în K-BOT")
                ElseIf ev.Operation = ForexeOperationKind.Receptie OrElse
                       ev.Operation = ForexeOperationKind.ReceptieModificare Then
                    Await _captures.SendAsync(cod, CapturaStore.FelReceptie)
                End If
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
                Case ForexeOperationKind.Angajament
                    ' Slice 0097-07: a NEW angajament (state «Initial»): its own short flow.
                    Return Await _controller.DownloadAngajamentNouAsync(cod)
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

        Await PastreazaMotivulAsync(ev, cod, codIndicator)

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
            CapturiLog.Write("MainForm.PastreazaRezervareaAsync",
                             $"«{cod}»: operatorul a ales «NU» la «Ați terminat modificarea rezervărilor?» " &
                             $"({memorie.Salvari} salvări); capturile de rezervare așteaptă «DA».")
            Return
        End If
        CapturiLog.Write("MainForm.PastreazaRezervareaAsync",
                         $"«{cod}»: operatorul a ales «DA» ({memorie.Salvari} salvări, {memorie.Count} indicator(i)).")

        ' Operator, 28.09.2026: the second picture of the session, taken while the page is
        ' still where the operator's last save left it - the Buget tab, with the new figures
        ' and the confirmation, which is the picture the guide shows (p.41). The download
        ' below drives the page away, so it cannot wait.
        Await _controller.IaCapturaAsync(cod, CapturaStore.FelRezervare, CapturaStore.MomentDupa, unaSingura:=True)

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
            _captures.SayNotSent(cod, "descărcarea rezervărilor din FOREXE nu a reușit")
            Return
        End If
        ' The package now carries them (and it is on disk); the memory is done with.
        _rezervariInLucru.Remove(cod)
        Dim preluat As Boolean = Await _ingest.IngestAsync(cod, pachet)
        ' Only now do the reservation rows exist in K-BOT, so only now can the two pictures
        ' be filed. A failed ingest leaves them on disk: «DA» tried again files them.
        If preluat Then
            Await _captures.SendAsync(cod, CapturaStore.FelRezervare)
            Await _controller.IncheieSesiuneaDeCapturiAsync(cod)
        Else
            _captures.SayNotSent(cod, "descărcarea rezervărilor nu s-a salvat în K-BOT")
        End If
    End Function

    ''' <summary>
    ''' Slice 0127 (operator, 04.11.2026): the reason typed in the FOREXE motive window is kept on the server, under
    ''' the revision number held for the angajament and the indicator that was saved, so the revision generated
    ''' later offers it as its short description. Only the work mode of the in-app browser gets here (the page
    ''' reports the save). A reason that cannot be kept is said on the console and never stops the save, which is
    ''' already in FOREXE.
    ''' </summary>
    Private Async Function PastreazaMotivulAsync(ev As ForexeWatchEvent, cod As String,
                                                 codIndicatorDinRand As String) As Task
        Try
            Dim k_indicator As String = String.Empty
            Dim k_motiv As String = MotivDin(ev.DetaliiJson, k_indicator)
            If String.IsNullOrWhiteSpace(k_motiv) Then Return
            If String.IsNullOrWhiteSpace(k_indicator) Then k_indicator = codIndicatorDinRand
            If String.IsNullOrWhiteSpace(k_indicator) Then
                _controller.SpuneStare($"Motivul rezervării lui «{cod}» nu s-a păstrat: pagina nu a dat indicatorul salvat.")
                Return
            End If
            Dim k_api As IMarcajApi = TryCast(_apiClient, IMarcajApi)
            If k_api Is Nothing Then
                Throw New InvalidOperationException("The API client does not implement IMarcajApi.")
            End If
            Await WithReauth(Of Boolean)(
                Async Function()
                    Await k_api.SalveazaMotivRezervareAsync(cod, k_indicator, k_motiv, CancellationToken.None)
                    Return True
                End Function)
            _controller.SpuneStare($"Motivul rezervării indicatorului «{k_indicator}» ({cod}) e păstrat pentru revizie.")
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PastreazaMotivulAsync", ex)
            _controller.SpuneStare($"Motivul rezervării lui «{cod}» nu s-a putut păstra pentru revizie: {ex.Message}")
        End Try
    End Function

    ''' <summary>The reason the page read from the motive window ("motiv" of a finished reservation) and the
    ''' indicator code it names; both empty when the page did not send them.</summary>
    Private Shared Function MotivDin(detaliiJson As String, ByRef codIndicator As String) As String
        codIndicator = String.Empty
        Try
            If String.IsNullOrWhiteSpace(detaliiJson) Then Return String.Empty
            Dim k_obj As JsonObject = TryCast(JsonNode.Parse(detaliiJson), JsonObject)
            If k_obj Is Nothing Then Return String.Empty
            Dim k_cod As JsonNode = k_obj("indicatorCod")
            codIndicator = If(k_cod Is Nothing, String.Empty, k_cod.ToString()).Trim()
            Dim k_motiv As JsonNode = k_obj("motiv")
            Return If(k_motiv Is Nothing, String.Empty, k_motiv.ToString()).Trim()
        Catch ex As Exception
            ' Data boundary (JSON from the page): logged, the reason is treated as absent.
            GlobalErrorLog.Write("MainForm.MotivDin", ex)
            codIndicator = String.Empty
            Return String.Empty
        End Try
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
        For Each info As AngajamentTreeInfo In _state.TreeInfos.Values
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

End Class
