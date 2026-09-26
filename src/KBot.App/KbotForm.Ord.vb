Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' THE ORDONANTARE EDITOR (slice 0049) -- its four entry points (slice 0086 split out of
''' KbotForm.vb).
''' </summary>
''' <remarks>
''' They live HERE, not in the view, for one reason: each needs the re-login net on one or more
''' response shapes, and <c>WithReauth</c> is private and generic in the shell. <c>OrdView</c>
''' gets a single action and stays read-only; the re-login policy stays, as everywhere, in one
''' place. The same pattern as <c>DeschideLegaturileReceptiilor</c> (slice 0048-04).
''' </remarks>
Partial Public Class KbotForm

    ''' <summary>
    ''' Executes a write command asked for by <c>OrdView</c>. UI boundary: logged and shown; a
    ''' throw from here would land on the UI thread.
    ''' </summary>
    Private Async Sub ExecutaComandaOrd(comanda As OrdComanda)
        Try
            If comanda Is Nothing OrElse String.IsNullOrWhiteSpace(comanda.Cod) Then Return

            Select Case comanda.Actiune
                Case OrdActiune.Adauga
                    Await AdaugaOrdonantareAsync(comanda.Cod, comanda.Ziua, comanda.IdPlataFx).ConfigureAwait(True)
                Case OrdActiune.Modifica : Await ModificaOrdonantareAsync(comanda.Ordonantare).ConfigureAwait(True)
                Case OrdActiune.Sterge : Await StergeOrdonantareAsync(comanda.Ordonantare).ConfigureAwait(True)
                Case OrdActiune.Lot
                    Await GenereazaInLotAsync(comanda.Cod, comanda.Luna, comanda.An).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an unknown action is a programming defect.
                    Throw New ArgumentException($"Acțiune ORD necunoscută: {comanda.Actiune}", NameOf(comanda))
            End Select
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaComandaOrd", ex)
            KBotMessage.Show(Me, ex.Message, "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaComandaOrd", ex)
            KBotMessage.Show(Me, "Comanda nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Adauga» -- generates the graph on the server (nothing written) and opens the editor.
    ''' The port of <c>FX_Adaugare_ORD_Din_Plati</c>. The warning about more than 25 partners
    ''' comes from the server, in <c>Avertismente</c>, and the form shows it.
    '''
    ''' <para><paramref name="ziCeruta"/> <c>Nothing</c> = the day is NOT known and the
    ''' operator is asked for it; that is how <c>OrdView</c> comes in, having nothing to derive
    ''' it from. When the command comes from the "+" on the payments tree, the day is the very
    ''' node pressed, so nobody is asked -- exactly as in Access, where
    ''' <c>fxPlati_AdaugareOrdonantare</c> received <c>vDataPlata</c> already chosen.</para>
    '''
    ''' <para><paramref name="idPlataFx"/> <c>Nothing</c> = every non-ordonantat payment of the
    ''' day (VBA: <c>vIdPlataFX = -1</c> -> <c>sIdPlataFX = "*"</c>). The leaf of the payments
    ''' tree is the DAY, not the payment, so today nobody sends a specific payment; the
    ''' parameter exists because the route accepts it and because Access's payment level may
    ''' come back.</para>
    ''' </summary>
    Private Async Function AdaugaOrdonantareAsync(cod As String,
                                                  Optional ziCeruta As Date? = Nothing,
                                                  Optional idPlataFx As Integer? = Nothing) As Task
        Dim zi As Date? = If(ziCeruta.HasValue, ziCeruta, CereZiua(cod))
        If Not zi.HasValue Then Return

        busyBar.Running = True
        Dim draft As OrdDraft
        Try
            draft = Await WithReauth(Of OrdDraft)(
                Function() _apiClient.GenereazaOrdAsync(cod, zi.Value, idPlataFx, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulOrd(draft)
    End Function

    ''' <summary>
    ''' «Modifica» -- loads the selected ordonantare in the editor's shape and opens it.
    '''
    ''' <para>Read through <c>GET /api/forexe/ord/draft/{idordp}</c>, NOT through the view's
    ''' call: that one is <c>OrdView</c>'s and chooses its columns deliberately (slice 0033) --
    ''' it does not return CodAI, CodIndicator, IdClsf, CodSSI, the explanation, the line's
    ''' partner, the document rows one by one or the links with the payments, all needed for
    ''' editing. `routes/forexe/ord.py` stays untouched.</para>
    ''' </summary>
    Private Async Function ModificaOrdonantareAsync(ordonantare As OrdHeaderRow) As Task
        If ordonantare Is Nothing OrElse ordonantare.Idordp <= 0 Then
            KBotMessage.Show(Me, "Selectați o ordonanțare din arbore.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        busyBar.Running = True
        Dim draft As OrdDraft
        Try
            Dim idordp As Integer = ordonantare.Idordp
            draft = Await WithReauth(Of OrdDraft)(
                Function() _apiClient.GetOrdDraftAsync(idordp, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulOrd(draft)
    End Function

    ''' <summary>Opens the editor MODALLY and, on a save, reloads the view onto the written document.</summary>
    Private Sub DeschideEditorulOrd(draft As OrdDraft)
        If draft Is Nothing Then Return

        Using f As New OrdEditForm(_apiClient, draft,
                                   Function(op) WithReauth(Of OrdSaveRezultat)(op),
                                   Function(op) WithReauth(Of PutAtasamentResponse)(op),
                                   Function(op) WithReauth(Of PdfDownloadResult)(op),
                                   Function(op) WithReauth(Of Integer)(op))
            f.ShowDialog(Me)
            If f.SAuSalvatModificari Then
                ' What stayed on screen is no longer true: the lines changed, the covered
                ' payments changed, and a new ordonantare was not even there.
                DupaScriereaOrdonantarii(f.IdordpSalvat)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' «Sterge» -- the confirmation names the number, the date and the total, and SAYS what
    ''' goes along with the document: the PDF stored on the server and the links with the
    ''' payments (which thus return to the pool of non-ordonantate). The delete itself is a
    ''' single DELETE on the header; the database cascades take the rest.
    ''' </summary>
    Private Async Function StergeOrdonantareAsync(ordonantare As OrdHeaderRow) As Task
        If ordonantare Is Nothing OrElse ordonantare.Idordp <= 0 Then
            KBotMessage.Show(Me, "Selectați o ordonanțare din arbore.",
                            "Ordonanțare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim data As String = If(ordonantare.DataOrd.HasValue,
                                ordonantare.DataOrd.Value.ToString("dd.MM.yyyy"), "fără dată")
        Dim intrebare As String =
            $"Ștergeți ordonanțarea nr. {ordonantare.NrOrd} din {data}, în valoare de " &
            $"{ordonantare.TotalOrd.ToString("N2", ro)} lei?" & vbCrLf & vbCrLf &
            "Odată cu ea se șterg beneficiarii, rândurile de plată, documentele justificative, " &
            "atașamentele și PDF-ul semnat stocat pe server." & vbCrLf &
            "Plățile acoperite redevin neordonanțate."

        If KBotMessage.Show(Me, intrebare, "Șterge ordonanțarea",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As OrdStergereRezultat
        Try
            Dim idordp As Integer = ordonantare.Idordp
            rez = Await WithReauth(Of OrdStergereRezultat)(
                Function() _apiClient.DeleteOrdAsync(idordp, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        KBotMessage.Show(Me,
            $"Ordonanțarea nr. {rez.NrOrd} a fost ștearsă." & vbCrLf &
            $"Beneficiari: {rez.Parteneri} · rânduri de plată: {rez.Linii} · " &
            $"documente: {rez.Documente} · atașamente: {rez.Atasamente} · PDF: {rez.Pdf}." & vbCrLf &
            $"Plăți redevenite neordonanțate: {rez.PlatiEliberate}.",
            "Șterge ordonanțarea", MessageBoxButtons.OK, MessageBoxIcon.Information)

        ' A delete cannot turn AreORD ON (it can only turn it off, and how many are left is
        ' not known from here) -- so the gate is not touched.
        DupaScriereaOrdonantarii(maiExistaOrdonantari:=False)
    End Function

    ''' <summary>
    ''' «Generare in lot» -- the port of <c>FX_Adaugare_ORD_Din_Plati_Batch</c>, restructured.
    '''
    ''' <para>The VBA loop re-queried the days with non-ordonantate payments and stopped when
    ''' the list emptied, because every saved ORD took its payments out of the candidate set
    ''' through <c>FX_ORD_TBL_REC</c>. The shape is kept: the days are asked for, and for each
    ''' one generate -> save is called, with no form and no interaction.</para>
    '''
    ''' <para><b>It STOPS at the first error</b>, saying which day failed and how many
    ''' succeeded -- exactly like the VBA, and for the same reason: an unsupervised loop that
    ''' goes on after a failure makes a mess nobody can reconstruct.</para>
    '''
    ''' <para>There is NO huge transaction spanning the whole batch: one ordonantare, one
    ''' transaction.</para>
    '''
    ''' <para><paramref name="luna"/> / <paramref name="an"/> <c>Nothing</c> = the whole
    ''' angajament; that is how <c>OrdView</c> comes in. When the command comes from the "+" of
    ''' a MONTH in the payments tree, the batch is bounded to that month -- VBA:
    ''' <c>vLunaAn</c>, which in Access was the text «luna/an» put into a <c>LIKE</c>. Here they
    ''' are two numeric parameters, because <c>*</c> is not a wildcard in MariaDB (see slice
    ''' 0049 section 1.4).</para>
    ''' </summary>
    Private Async Function GenereazaInLotAsync(cod As String,
                                               Optional luna As Integer? = Nothing,
                                               Optional an As Integer? = Nothing) As Task
        busyBar.Running = True
        Dim zile As OrdZileInfo
        Try
            zile = Await WithReauth(Of OrdZileInfo)(
                Function() _apiClient.GetOrdZileAsync(cod, luna, an, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        ' The period name, for the messages: without month/year it is the whole angajament.
        Dim perioada As String = If(luna.HasValue AndAlso an.HasValue,
                                    $" în {NumeLuna(luna.Value)} {an.Value}", String.Empty)

        If zile Is Nothing OrElse zile.Zile.Count = 0 Then
            KBotMessage.Show(Me, $"Nu există plăți neordonanțate pentru {cod}{perioada}.",
                            "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            $"Se generează ordonanțări pentru {zile.Zile.Count} zile cu plăți neordonanțate{perioada} " &
            $"({zile.TotalEstimat} ordonanțări estimate)." & vbCrLf & vbCrLf &
            "Fiecare zi se salvează separat, fără să vă mai fie cerută confirmarea." & vbCrLf &
            "La prima eroare, generarea se oprește. Continuați?"
        If KBotMessage.Show(Me, intrebare, "Generare în lot",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Dim reusite As Integer = 0
        Dim ziEsuata As String = Nothing
        Dim motiv As String = Nothing

        busyBar.Running = True
        Try
            For Each zi As OrdZiCandidat In zile.Zile
                Dim data As Date = zi.Data
                Try
                    Dim draft As OrdDraft = Await WithReauth(Of OrdDraft)(
                        Function() _apiClient.GenereazaOrdAsync(cod, data, Nothing, CancellationToken.None))
                    Await WithReauth(Of OrdSaveRezultat)(
                        Function() _apiClient.SaveOrdAsync(draft, CancellationToken.None))
                    reusite += 1
                Catch ex As Exception
                    GlobalErrorLog.Write("MainForm.GenereazaInLotAsync", ex)
                    ziEsuata = data.ToString("dd.MM.yyyy")
                    motiv = ex.Message
                    Exit For
                End Try
            Next
        Finally
            busyBar.Running = False
        End Try

        If ziEsuata Is Nothing Then
            KBotMessage.Show(Me, $"{reusite} ordonanțări au fost generate și salvate.",
                            "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            KBotMessage.Show(Me,
                $"Generarea s-a oprit la data {ziEsuata}." & vbCrLf &
                $"Motiv: {motiv}" & vbCrLf & vbCrLf &
                $"Până acolo s-au salvat {reusite} ordonanțări; ele RĂMÂN salvate. " &
                "Rezolvați cauza și reluați — zilele deja acoperite nu se mai propun.",
                "Generare în lot", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

        ' The gate turns on only when the batch actually wrote something.
        DupaScriereaOrdonantarii(maiExistaOrdonantari:=reusite > 0)
    End Function

    ''' <summary>
    ''' Asks the operator for the day the ordonantare is generated for. Default: today.
    ''' <c>Nothing</c> when the operator gave up.
    ''' </summary>
    Private Function CereZiua(cod As String) As Date?
        Using dlg As New OrdZiuaForm(cod)
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return Nothing
            Return dlg.Ziua
        End Using
    End Function

    ' The month name in Romanian, capitalised -- only for the batch messages.
    Private Shared Function NumeLuna(luna As Integer) As String
        If luna < 1 OrElse luna > 12 Then Return CStr(luna)
        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim nume As String = ro.DateTimeFormat.GetMonthName(luna)
        If String.IsNullOrEmpty(nume) Then Return CStr(luna)
        Return Char.ToUpper(nume(0), ro) & nume.Substring(1)
    End Function

    ''' <summary>
    ''' What is refreshed after any ordonantare WRITE (add, modify, delete, batch). The port of
    ''' the lines after <c>FX_Adaugare_ORD_*</c> in <c>frmFX_MAIN</c>: there came
    ''' <c>RefreshTreeQuery</c> and <c>fxPlati.RefreshPlati CodAngajament, True</c>.
    '''
    ''' <para>Three things, in this order:</para>
    ''' <list type="number">
    ''' <item>the ORD view, when it is the active one -- the written document must appear;</item>
    ''' <item>the PLATI view, when it was opened -- the covered payments are no longer
    ''' non-ordonantate, so the "+" sits on the wrong day and the node states lie. It is
    ''' refreshed even when it is not the active view: otherwise it would lie silently until
    ''' the next angajament change;</item>
    ''' <item>the view gate: an angajament's first ordonantare turns its <c>AreORD</c> on. It is
    ''' raised LOCALLY, not by re-reading the big tree -- a <c>LoadTreeAsync</c> from here would
    ''' clear the selection and throw the operator back onto «sumar» right after saving their
    ''' document. The flag is corrected by the server anyway on the next year/SS change or the
    ''' next download.</item>
    ''' </list>
    ''' </summary>
    Private Sub DupaScriereaOrdonantarii(Optional idordpSalvat As Integer? = Nothing,
                                         Optional maiExistaOrdonantari As Boolean = True)
        Try
            TryCast(_activeView, OrdView)?.Reincarca(If(idordpSalvat, 0))

            Dim vederePlati As IAngajamentView = Nothing
            If _views.TryGetValue("plati", vederePlati) Then
                TryCast(vederePlati, PlatiView)?.Reincarca()
            End If

            If maiExistaOrdonantari AndAlso _currentInfo IsNot Nothing AndAlso Not _currentInfo.AreORD Then
                _currentInfo.AreORD = True
                ApplyViewGating(_currentInfo)
            End If
        Catch ex As Exception
            ' UI boundary: the refresh failed, but the WRITE already succeeded. Logged, and we
            ' carry on -- a throw here would make it look as though the save had failed.
            GlobalErrorLog.Write("MainForm.DupaScriereaOrdonantarii", ex)
        End Try
    End Sub
End Class
