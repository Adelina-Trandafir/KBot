Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' THE DDF EDITOR (slice 0051) -- the command dispatch and the ways into the editor (slice 0086
''' split out of KbotForm.vb). The deletes and the refresh after a write are in
''' KbotForm.DdfDelete.vb; the send to forexecab and the Rezervari footer follow-ups are in
''' KbotForm.DdfSend.vb.
''' </summary>
''' <remarks>
''' The same shape as <c>ExecutaComandaOrd</c>: the view asks, the shell executes. Every call
''' goes through <c>WithReauth</c>, which is private and generic here, so the re-login policy
''' stays in one place.
''' </remarks>
Partial Public Class KbotForm

    ''' <summary>
    ''' Executes one write command of the fundamentation document.
    '''
    ''' <para><c>Adauga</c> and <c>AdaugaRevizieInitiala</c> are asked for by the "+" icon on
    ''' the RESERVATIONS tree (<c>RezervariView.Tree_RightIconClicked</c>) -- which is where
    ''' Access asked for them too: <c>fxRezervari_AdaugaRevizie</c> in <c>frmFX_MAIN</c>.
    ''' Which of the two is decided there, from the reservation's <c>EInitiala</c>, exactly as
    ''' <c>cNode.Value2</c> decided it in <c>frmFX_MAIN_REZ</c>. They had no caller when slice
    ''' 0051 shipped (decision D20); they have one now.</para>
    ''' </summary>
    Private Async Sub ExecutaComandaDdf(comanda As DdfComanda)
        Try
            If comanda Is Nothing OrElse String.IsNullOrWhiteSpace(comanda.Cod) Then Return

            Select Case comanda.Actiune
                Case DdfActiune.Adauga
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=False).ConfigureAwait(True)
                Case DdfActiune.AdaugaRevizieInitiala
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=True).ConfigureAwait(True)
                Case DdfActiune.Modifica
                    Await ModificaDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeRevizie
                    Await StergeRevizieDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.Sterge
                    Await StergeDocumentDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeLuna
                    Await StergeLunaDdfAsync(comanda).ConfigureAwait(True)
                Case DdfActiune.Trimite
                    Await TrimiteDdfAsync(comanda.Cod, comanda.Revizie).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an unknown action is a programming defect.
                    Throw New ArgumentException($"Acțiune DDF necunoscută: {comanda.Actiune}", NameOf(comanda))
            End Select
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            KBotMessage.Show(Me, ex.Message, "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            KBotMessage.Show(Me, "Comanda nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Adauga» -- generates the graph on the server (NOTHING is written) and opens the
    ''' editor. The port of <c>FX_Adaugare_DDF</c>.
    '''
    ''' <para><paramref name="rev0"/> selects the HEADER treatment: the initial revision, which
    ''' also creates the document, or a subsequent one on a document that exists. It does NOT
    ''' select the line source -- the server takes that from the data, and refuses loudly when
    ''' neither source has rows.</para>
    '''
    ''' <para>Called from the "+" icon on the reservations tree; see
    ''' <see cref="ExecutaComandaDdf"/>.</para>
    ''' </summary>
    Private Async Function AdaugaDdfAsync(cod As String, rev0 As Boolean) As Task
        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GenereazaDdfAsync(cod, rev0, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        ' Operator, 26.09.2026: the program of a revision added from the Rezervari tree is the
        ' angajament's -- FX_Indicatori.SS -> DefaProgram -- not the one /genereaza copied (the
        ' last revision's, or the fixed default for revision 0). The SSs are the selected node's.
        Dim surse As String = String.Empty
        If _currentInfo IsNot Nothing AndAlso
           String.Equals(_currentInfo.CodAngajament, cod, StringComparison.OrdinalIgnoreCase) Then
            surse = If(_currentInfo.Surse, String.Empty)
        End If
        DeschideEditorulDdf(draft, programDinIndicatori:=True, surseIndicatori:=surse)
    End Function

    ''' <summary>
    ''' «Modifica» -- loads the selected revision in the editor's shape and opens it.
    '''
    ''' <para>Read through <c>GET /api/forexe/ddf/draft/{iddf}/{idrev}</c>, NOT through the
    ''' view's call: that one is the read-only view's and chooses its columns deliberately --
    ''' it returns neither the section-A keys and classifications, nor the section-B rows, nor
    ''' the attachment rows, all of which editing needs.</para>
    ''' </summary>
    Private Async Function ModificaDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Idrev <= 0 OrElse revizie.Iddf <= 0 Then
            KBotMessage.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Slice 0081-02: only a revision not sent yet (S0 / S1) is edited (D4). Once forexecab has
        ' it, any change is a NEW revision -- «Adauga rezervare» in the Rezervari tree.
        Dim stare As DdfRevisionState = revizie.Stare
        If Not DdfRevisionStates.CanEdit(stare) Then
            KBotMessage.Show(Me, $"Revizia este «{DdfRevisionStates.Label(stare)}» și nu se mai modifică." &
                            vbCrLf & vbCrLf & "Orice schimbare de valori cere o revizie nouă " &
                            "(«Adaugă rezervare» din subsolul arborelui Rezervări).",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            Dim iddf As Integer = revizie.Iddf
            Dim idrev As Integer = revizie.Idrev
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        DeschideEditorulDdf(draft, stare)
    End Function

    ''' <summary>
    ''' Slice 0081-02 -- «Angajament nou» on the header: a new, K-BOT-only angajament («!» code),
    ''' revision 0, section A empty. The port of Access's <c>FX_Adaugare_ANG</c>; forexecab hears
    ''' of it only at «Trimite in FOREXE».
    ''' </summary>
    Private Sub BtnAngajamentNou_Click(sender As Object, e As EventArgs) Handles btnAngajamentNou.Click
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                "Angajament nou", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim draft As DdfDraft = DdfDraftFactory.ForNewAngajament(
                DdfDraftFactory.NewManualCode(), _session.DbName, _session.CodProgram, Date.Today)
            DeschideEditorulDdf(draft, DdfRevisionState.Draft)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.BtnAngajamentNou_Click", ex)
            KBotMessage.Show(Me, "Documentul pentru angajamentul nou nu a putut fi deschis. Detalii în jurnalul de erori.",
                            "Angajament nou", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0081-02 -- the option picked in the Rezervari footer menu. UI boundary (Async Sub
    ''' handed to the view): every failure is logged and shown, never thrown.
    ''' </summary>
    Private Async Sub ExecutaMeniulRezervari(optiune As RezervariMenuOption, info As AngajamentTreeInfo)
        Try
            Select Case optiune
                Case RezervariMenuOption.AdaugaRezervare
                    Await AdaugaRezervareDdfAsync(info, cuIndicatori:=False).ConfigureAwait(True)
                Case RezervariMenuOption.AdaugaRezervareCuIndicatori
                    Await AdaugaRezervareDdfAsync(info, cuIndicatori:=True).ConfigureAwait(True)
                Case RezervariMenuOption.Definitiveaza
                    Await SchimbaStareaAngajamentuluiAsync(info, definitivare:=True).ConfigureAwait(True)
                Case RezervariMenuOption.Deruleaza
                    Await SchimbaStareaAngajamentuluiAsync(info, definitivare:=False).ConfigureAwait(True)
                Case RezervariMenuOption.GenereazaPdfFinal
                    Await GenereazaPdfFinalDinMeniuAsync(info).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an option with no handler is a programming defect.
                    Throw New ArgumentException($"Opțiune de meniu fără acțiune: {optiune}", NameOf(optiune))
            End Select
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaMeniulRezervari", ex)
            KBotMessage.Show(Me, ex.Message, "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaMeniulRezervari", ex)
            KBotMessage.Show(Me, "Acțiunea nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0081-02 -- «Adauga rezervare» from the Rezervari footer menu: a NEW revision in
    ''' manual mode, section A empty. On an angajament with a document it continues that document
    ''' (header of the last revision); without one it opens its revision 0 (the carried-over case).
    ''' The menu offers it only when no revision is open (plan 0081-04); the check is repeated
    ''' here against fresh data, because the tree may be minutes old.
    ''' <para><paramref name="cuIndicatori"/> (operator, 26.09.2026, «2. Foloseste indicatorii
    ''' existenti»): section A starts with a line, value 0, for every indicator the angajament
    ''' already has; the editor builds them once the classification list is here.</para>
    ''' </summary>
    Private Async Function AdaugaRezervareDdfAsync(info As AngajamentTreeInfo, cuIndicatori As Boolean) As Task
        If info Is Nothing OrElse String.IsNullOrWhiteSpace(info.CodAngajament) Then Return
        Dim cod As String = info.CodAngajament

        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(
                Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
            Dim revizii As List(Of RevizieRow) = If(ddf?.Revizii, New List(Of RevizieRow)())
            Dim deschisa As RevizieRow = revizii.FirstOrDefault(Function(r) DdfRevisionStates.IsOpen(r.Stare))
            If deschisa IsNot Nothing Then
                KBotMessage.Show(Me, $"Revizia {deschisa.NumarRev} este încă «{DdfRevisionStates.Label(deschisa.Stare)}»." &
                                vbCrLf & vbCrLf & "Se lucrează la o singură revizie odată: termin-o întâi din vederea DDF.",
                                "Adaugă rezervare", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If revizii.Count = 0 Then
                draft = DdfDraftFactory.ForFirstRevisionOfExisting(
                    cod, _session.DbName, _session.CodProgram, info.Descriere, info.DataCreare, info.Stare, Date.Today)
            Else
                Dim ultima As RevizieRow = revizii.OrderByDescending(Function(r) r.NumarRev).First()
                Dim iddf As Integer = ultima.Iddf
                Dim idrev As Integer = ultima.Idrev
                Dim sursa As DdfDraft = Await WithReauth(Of DdfDraft)(
                    Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
                draft = DdfDraftFactory.ForAddedReservation(sursa, Date.Today)
            End If
        Finally
            busyBar.Running = False
        End Try

        ' Both entries (empty revision and prefilled one): the program comes from the indicators
        ' (FX_Indicatori.SS -> DefaProgram), never from the last revision or the session -- operator,
        ' 26.09.2026. The editor maps the node's sources once it has the DefaProgram map.
        DeschideEditorulDdf(draft, DdfRevisionState.Draft, cuIndicatori, If(info.Surse, String.Empty),
                            programDinIndicatori:=True)
    End Function

    ''' <summary>Opens the editor MODALLY and, on a save, reloads the view onto what was written.</summary>
    Private Sub DeschideEditorulDdf(draft As DdfDraft, Optional stare As DdfRevisionState = DdfRevisionState.Draft,
                                    Optional cuIndicatori As Boolean = False,
                                    Optional surseIndicatori As String = "",
                                    Optional programDinIndicatori As Boolean = False)
        If draft Is Nothing Then Return
        ' Slice 0081-02: a brand-new K-BOT angajament is not in the tree yet; after its save the
        ' tree is reloaded ONTO it, so the operator lands on what they just created.
        Dim angajamentNou As Boolean = draft.Nou AndAlso draft.Manual

        ' Seven specialisations of the 401 net, named rather than positional: `WithReauth` is
        ' private and generic, so the form needs one closure per response shape, and seven
        ' identical-looking delegates in a row is an argument order nobody gets right twice.
        Dim reauth As New DdfEditReauth(
            Function(op) WithReauth(Of DdfSaveRezultat)(op),
            Function(op) WithReauth(Of PutDdfFisierResponse)(op),
            Function(op) WithReauth(Of PdfDownloadResult)(op),
            Function(op) WithReauth(Of DdfNumarLock)(op),
            Function(op) WithReauth(Of List(Of String))(op),
            Function(op) WithReauth(Of List(Of DdfPartener))(op),
            Function(op) WithReauth(Of List(Of DdfClasificatie))(op),
            Function(op) WithReauth(Of List(Of DdfSursaProgram))(op))

        ' Slice 0081-09: a section-A line picks its SS among those of the document's program
        ' (AVACONT_COMUN.DefaProgram, read by the editor); the SS chosen here is the one proposed.
        Using f As New DdfEditForm(_apiClient, draft, reauth, stare, TryCast(_apiClient, IDdfSendApi)) With {
                .SursaSectorSesiune = If(_session.SectorSursa, String.Empty),
                .PornesteCuIndicatorii = cuIndicatori,
                .SurseIndicatori = If(surseIndicatori, String.Empty),
                .ProgramDinIndicatori = programDinIndicatori}
            f.ShowDialog(Me)
            If f.SAuSalvatModificari AndAlso angajamentNou Then
                ReincarcaArborelePe(draft.CodAngajament)
            ElseIf f.SAuSalvatModificari Then
                ' What is still on screen is no longer true: the lines changed, the reservations
                ' consumed changed, and a new revision was not there at all.
                ' `IddfSalvat` is the key the server returned, so it also answers "does this
                ' angajament have a document now?" -- which is what the «ddf» nav entry asks.
                DupaScriereaDdf(f.IdrevSalvat, draft.CodAngajament,
                                iddfDocument:=f.IddfSalvat)
            End If
        End Using
    End Sub
End Class
