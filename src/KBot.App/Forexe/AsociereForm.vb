Option Strict On
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' EDITORUL DE LEGĂTURI RECEPȚIE ▸ INSTANTANEU (felia 0048-04) — se deschide ORICÂND, nu
''' doar când tocmai a sosit ceva.
'''
''' <para><b>Problema, în două propoziții.</b> <c>FX_Receptii_R</c> știe CARE recepție, dar
''' nu are axă a timpului. <c>FX_Receptii_H</c> are axa timpului, dar nu știe care recepție —
''' istoricul FOREXE nu numește niciodată recepția (F4). Legătura dintre ele nu există în
''' date și nu poate fi dedusă: valoarea nu e cheie (F5), data nu e cheie (F6), iar o salvare
''' care nu schimbă nimic produce oricum un instantaneu complet (F7). Vezi
''' <c>docs/FUNDAMENT_Asociere_Receptii.md</c>.</para>
'''
''' <para><b>De ce se deschide oricând.</b> Access avea exact gazda asta — cele patru
''' subformulare <c>frmFX_DUBII_LISTA*</c> se ramifică pe <c>isLoaded("frmFX_ASOC")</c> — iar
''' operatorul a cerut-o din nou pe 29.08.2026: legăturile trebuie corectate când se observă
''' greșeala, nu doar în minutul de după o descărcare.</para>
'''
''' <para><b>Ce nu se poate atinge.</b> O legătură pe care s-a construit o ordonanțare, sau
''' peste care s-au calculat plăți de la data ei încolo, rămâne <b>vizibilă, dar nu se mai
''' mută</b>. Serverul decide asta, nu formularul (<c>routes/forexe/asociere.py</c>); aici doar
''' se arată și se refuză din vreme, ca operatorul să nu ajungă la un mesaj de eroare după ce
''' a tras.</para>
'''
''' <para><b>O singură salvare, la sfârșit</b> (D-H). Tragerile schimbă doar tabloul local;
''' nimic nu pleacă spre server până la buton. Comenzile trimise sunt DOAR cele care diferă de
''' ce s-a citit — o legătură neatinsă nu se rescrie, iar tăcerea înseamnă «las-o cum e».</para>
''' </summary>
Public Class AsociereForm

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere.
    Private Const COL_INDICATOR As String = "indicator"
    Private Const COL_SSI As String = "ssi"
    Private Const COL_CREDIT As String = "credit"
    Private Const COL_VALOARE As String = "valoare"

    ' Prefixele cheilor de nod. Cheia spune și CE e nodul, deci nu mai trebuie ghicit din Tag.
    Private Const CHEIE_RECEPTIE As String = "R:"
    Private Const CHEIE_INSTANTANEU As String = "H:"
    Private Const CHEIE_LIBERE As String = "LIBERE"

    ' Cheile din meniul contextual.
    Private Const MENIU_IGNORA As String = "ignora"
    Private Const MENIU_NU_IGNORA As String = "nu_ignora"
    Private Const MENIU_STERGERE As String = "stergere"
    Private Const MENIU_NU_STERGERE As String = "nu_stergere"
    Private Const MENIU_DESPRINDE As String = "desprinde"
    Private Const MENIU_RECEPTIE_NOUA As String = "receptie_noua"
    Private Const MENIU_RENUNTA_RECEPTIE As String = "renunta_receptie"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _cod As String
    ' Plasa 401 a shell-ului, specializată pe fiecare dintre cele două forme de răspuns:
    ' politica de re-login rămâne într-un singur loc, formularul doar o folosește.
    Private ReadOnly _withReauthStare As Func(Of Func(Of Task(Of AsociereStare)), Task(Of AsociereStare))
    Private ReadOnly _withReauthSalvare As Func(Of Func(Of Task(Of AsociereRezultat)), Task(Of AsociereRezultat))

    ' ── Modul PROPUNERE (felia 0055) ─────────────────────────────────────────────────
    ' Câmpurile de mai jos sunt Nothing în modul de oricând și pline în modul propunere.
    Private ReadOnly _mod As ModAsociere
    Private ReadOnly _propunere As PrelucrarePropunere
    ' Payload-ul descărcării. Se RETRIMITE neschimbat la salvare: `rand_istoric` din decizii
    ' e indicele rândului în `TabelIstoric`, nu o cheie de bază, deci faza a doua trebuie să
    ' vadă exact ce a văzut faza întâi. O re-descărcare între faze cere o propunere nouă.
    Private ReadOnly _pachet As PrelucrareRezultat
    ' Alegerile de unitate deja făcute. Merg înapoi cu salvarea: bifa «nu mă mai întreba» s-a
    ' scris în FX_Alegeri_Unitate ÎNĂUNTRUL tranzacției propunerii, deci s-a derulat înapoi
    ' odată cu ea — fără ele, faza a doua ar primi din nou 409 pentru o întrebare la care
    ' operatorul a răspuns deja.
    Private ReadOnly _alegeri As List(Of AlegereUnitate)
    Private ReadOnly _withReauthPrelucrare As Func(Of Func(Of Task(Of PrelucrareRaspuns)), Task(Of PrelucrareRaspuns))

    ' Tabloul citit de la server. `_stare` e ADEVĂRUL DE PE SERVER și nu se modifică local —
    ' altfel n-am mai avea cu ce compara ca să știm ce s-a schimbat.
    Private _stare As AsociereStare

    ' Tabloul LOCAL, pe care îl mișcă operatorul. Cheia e IDRH.
    '   `_pozitie`   IDRH ▸ IDRR pe care stă acum (0 = neașezat)
    '   `_ignorat`   IDRH ▸ marcat «nu consemnează nicio schimbare» (F17)
    '   `_stergere`  IDRH ▸ este rândul de ștergere al lanțului (F21)
    Private ReadOnly _pozitie As New Dictionary(Of Integer, Integer)
    Private ReadOnly _ignorat As New Dictionary(Of Integer, Boolean)
    Private ReadOnly _stergere As New Dictionary(Of Integer, Boolean)

    ' Recepțiile pe care le PORNEȘTE operatorul aici, din coșul celor neașezate (F26): o
    ' recepție creată și ștearsă pe site înainte ca K-BOT să fi descărcat vreodată
    ' angajamentul nu are rând în `ListaReceptii`, deci nu există nicăieri de care lanțul ei
    ' să se agațe — dar instantaneele ei sunt toate în istoric.
    '
    ' PROIECȚIE, nu o a doua evidență. Adevărul rămâne `_pozitie`: o recepție nouă e un IDRR
    ' NEGATIV pus acolo, iar lista de mai jos se reface din el la fiecare reconstruire
    ' (`ActualizeazaReceptiileNoi`). Așa nu există un al doilea loc care să se contrazică cu
    ' primul, iar o recepție rămasă fără niciun instantaneu dispare de la sine în loc să
    ' trebuiască ștearsă de cineva.
    Private ReadOnly _receptiiNoi As New List(Of ReceptiePropusa)

    ' IDRH-urile care ÎNCHID un lanț pornit aici. Tot o proiecție a lui `_pozitie`, refăcută în
    ' aceeași trecere cu lista de mai sus.
    '
    ' Pe o recepție pornită aici rândul de ștergere NU e o alegere a operatorului, cum e pe una
    ' venită de la server: recepția asta există tocmai fiindcă a fost ștearsă (F26), deci ultimul
    ' instantaneu al lanțului ESTE ștergerea ei, oricare ar fi el. `_stergere` nu are niciun
    ' cuvânt aici — un steag rămas de pe o așezare anterioară ar arăta pe ecran un rând de
    ' ștergere pe care firul nu-l trimite.
    Private ReadOnly _stergereNoua As New HashSet(Of Integer)

    ''' <summary>Banda de mesaje arată ACUM un avertisment pus de verificarea recepțiilor noi.</summary>
    ''' <remarks>
    ''' Se ține minte fiindcă banda e comună: numai cine a pus un mesaj are voie să-l ia jos, altfel
    ''' o reconstruire ar șterge mesajul altcuiva.
    ''' </remarks>
    Private _avertismentReceptiiNoi As Boolean

    ' The receipt the selected row belongs to. The chart's per-receipt view is drawn from this, and
    ' the whole-commitment view emphasises its line, so the operator can still tell which chain is
    ' theirs among all the others.
    Private _receptieSelectata As ReceptiePropusa

    ' Chart tab keys — the same two strings the designer writes into `grafic.Tabs`.
    Private Const GRAFIC_RECEPTIE As String = "receptie"
    Private Const GRAFIC_ANGAJAMENT As String = "angajament"

    ' The key of the chart's total line. Not a receipt, so it deliberately cannot collide with
    ' CheiaSeriei, which always starts with "R".
    Private Const SERIA_TOTAL As String = "TOTAL"

    ' Tree rows by the thing they stand for, filled while the trees are built. Read twice: by the
    ' colouring pass, to paint each row in the colour of its own point or line, and by a click on a
    ' chart point, which has to find the row standing for the same snapshot.
    ' A dictionary rather than a search: the colouring pass runs on every selection change and on
    ' every drag, and walking two whole trees for each of a few hundred snapshots is a lot of work
    ' to redo for an answer we already had while building them.
    ' The receipts need no tree beside them — a root only ever exists in the left tree. A snapshot
    ' can be in either, so it carries its own (see RandDeArbore).
    Private ReadOnly _nodReceptie As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)
    Private ReadOnly _nodInstantaneu As New Dictionary(Of Integer, RandDeArbore)

    ' Aceleași două dicționare, pentru banda de așezare. Se umplu la construirea benzilor și se
    ' citesc de pasul de culori — un instantaneu are ACUM trei înfățișări pe ecran (rândul din
    ' arbore, punctul din grafic, marcajul de pe bandă) și toate trei trebuie să poarte aceeași
    ' culoare, altfel culoarea nu mai leagă nimic de nimic.
    Private ReadOnly _bandaReceptie As New Dictionary(Of Integer, KBotLane)
    Private ReadOnly _marcajInstantaneu As New Dictionary(Of Integer, KBotLaneMarker)

    ''' <summary>True dacă s-a salvat ceva — gazda reîncarcă recepțiile abia atunci.</summary>
    Public ReadOnly Property SAuSalvatModificari As Boolean

    Public Sub New(apiClient As IApiClient,
                   cod As String,
                   withReauthStare As Func(Of Func(Of Task(Of AsociereStare)), Task(Of AsociereStare)),
                   withReauthSalvare As Func(Of Func(Of Task(Of AsociereRezultat)), Task(Of AsociereRezultat)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
        If withReauthStare Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthStare))
        If withReauthSalvare Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthSalvare))
        InitializeComponent()
        _apiClient = apiClient
        _cod = cod.Trim()
        _withReauthStare = withReauthStare
        _withReauthSalvare = withReauthSalvare
        capBar.Text = $"K-BOT — Legăturile recepțiilor · {_cod}"
        ' Vederea implicită e graficul. Scrisă și aici, nu doar în designer: cheia din designer
        ' trece prin `EndInit`, care în procesul Visual Studio nu o aplică deloc — deci pe drumul
        ' ăla singura garanție e `benzi.Visible = False`, iar asta e o valoare, nu o alegere.
        AplicaVedereaDinDreaptaSus(VEDEREA_GRAFIC)
    End Sub

    ''' <summary>
    ''' MODUL PROPUNERE (felia 0055): același editor, dar peste tabloul pe care tocmai l-a
    ''' propus o descărcare, nu peste ce e scris în bază.
    '''
    ''' <para><b>Miza e alta, și de-asta se și poartă altfel.</b> În modul de oricând, o
    ''' închidere fără salvare pierde niște corecturi. Aici pierde ÎNTREAGA descărcare: faza
    ''' întâi a rulat toți pașii și a derulat tranzacția înapoi necondiționat, deci până la
    ''' butonul de salvare nu există în bază nici recepțiile, nici plățile, nici istoricul.
    ''' De aceea fiecare instantaneu are nevoie de o hotărâre înainte ca butonul să se
    ''' aprindă (serverul refuză cu 400 o acoperire incompletă: tăcerea nu are voie să
    ''' însemne «ignoră-l»), iar închiderea spune limpede ce se pierde.</para>
    ''' </summary>
    ''' <param name="propunere">Tabloul întors de faza întâi.</param>
    ''' <param name="pachet">Payload-ul descărcării, retrimis NESCHIMBAT la salvare.</param>
    ''' <param name="alegeri">Alegerile de unitate deja făcute; merg înapoi cu salvarea.</param>
    Public Sub New(apiClient As IApiClient,
                   cod As String,
                   propunere As PrelucrarePropunere,
                   pachet As PrelucrareRezultat,
                   alegeri As IReadOnlyList(Of AlegereUnitate),
                   withReauthPrelucrare As Func(Of Func(Of Task(Of PrelucrareRaspuns)), Task(Of PrelucrareRaspuns)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
        If propunere Is Nothing Then Throw New ArgumentNullException(NameOf(propunere))
        If pachet Is Nothing Then Throw New ArgumentNullException(NameOf(pachet))
        If withReauthPrelucrare Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthPrelucrare))
        InitializeComponent()
        _apiClient = apiClient
        _cod = cod.Trim()
        _mod = ModAsociere.Propunere
        _propunere = propunere
        _pachet = pachet
        _alegeri = If(alegeri Is Nothing, New List(Of AlegereUnitate)(), New List(Of AlegereUnitate)(alegeri))
        _withReauthPrelucrare = withReauthPrelucrare
        capBar.Text = $"K-BOT — Așezarea recepțiilor descărcate · {_cod}"
        ' Clearing the placements means something ONLY here. The anytime editor has no "current
        ' step": every link there is an old one, and a button that detached them all would be a
        ' tool for breaking, not for starting over. See btnReseteaza_Click.
        btnReseteaza.Visible = True
        AplicaVedereaDinDreaptaSus(VEDEREA_GRAFIC)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Încărcarea
    ' ══════════════════════════════════════════════════════════════════════════

    Private Async Sub AsociereForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' Graniță de UI: un throw dintr-un tratator async ar cădea pe firul de UI, deci se
        ' loghează și se arată, nu se re-aruncă.
        Await ReincarcaAsync()
    End Sub

    Private Async Function ReincarcaAsync() As Task
        Try
            Cursor = Cursors.WaitCursor
            btnSalveaza.Enabled = False
            Dim stare As AsociereStare
            If _mod = ModAsociere.Propunere Then
                ' Nimic de cerut: tabloul a venit odată cu propunerea, iar o a doua citire ar
                ' arăta baza AȘA CUM E ACUM — adică fără nimic din descărcarea asta, fiindcă
                ' faza întâi s-a derulat înapoi.
                stare = AsociereStare.DinPropunere(_propunere)
            Else
                stare = Await _withReauthStare(Function() _apiClient.GetAsociereAsync(_cod, CancellationToken.None))
            End If

            _stare = stare
            _receptieSelectata = Nothing
            _pozitie.Clear()
            _ignorat.Clear()
            _stergere.Clear()
            ' Recepțiile pornite aici s-au dus odată cu salvarea: ce s-a scris se întoarce din
            ' `stare` cu IDRR-ul lui adevărat, iar ce nu s-a scris nu mai are pe ce sta.
            _receptiiNoi.Clear()
            _stergereNoua.Clear()
            _avertismentReceptiiNoi = False
            For Each i As InstantaneuLegat In stare.Instantanee
                _pozitie(i.Idrh) = i.Idrr
                _ignorat(i.Idrh) = i.Ignorat
                _stergere(i.Idrh) = i.Stergere
            Next

            Reconstruieste()

            If stare.Instantanee.Count = 0 Then
                ntfMesaj.Show("Angajamentul nu are niciun instantaneu de istoric.", NoticeKind.Warning)
            Else
                ntfMesaj.Clear()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.ReincarcaAsync", ex)
            ntfMesaj.Show(TextDeEroare(ex, "Nu am putut citi legăturile"), NoticeKind.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Proiecția pe ecran
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Reconstruiește amândoi arborii din tabloul LOCAL.
    '''
    ''' <para>Se reconstruiește tot, nu se mută frunza. Un arbore mutat pe bucăți și un tablou
    ''' de date ajung, după câteva trageri, să spună lucruri diferite — iar aici cel care minte
    ''' e ecranul, adică exact ce nu-și poate permite un formular al cărui rost e să arate ce
    ''' s-a hotărât.</para>
    '''
    ''' <para><b>The scroll of both trees survives the rebuild</b> (operator, 09.09.2026).
    ''' <c>Clear</c> zeroes the scroll — rightly, it is an emptying — so every drag threw the
    ''' operator back to the first row, which is precisely away from the receipt they were
    ''' working on. It is read BEFORE and put back AFTER; <c>ScrollOffsetY</c> rebuilds the
    ''' bar's range first, or the value would clamp to zero because the emptying left it with no
    ''' content. What is NOT remembered is the SELECTION: it carries meaning here (the chosen
    ''' row feeds the grid and the chart) and a drag changes it anyway.</para>
    ''' </summary>
    Private Sub Reconstruieste()
        Try
            Dim derulareLant As Integer = treeLant.ScrollOffsetY
            Dim derulareLibere As Integer = treeLibere.ScrollOffsetY
            treeLant.Clear()
            treeLibere.Clear()
            grid.ClearRows()
            ' Cleared with the trees, never after: a stale entry here points at a TreeItem that is
            ' no longer on screen, and colouring it would look exactly like doing nothing.
            _nodReceptie.Clear()
            _nodInstantaneu.Clear()
            ' ÎNAINTE de benzi și de arbori, fiindcă amândouă le desenează: lista recepțiilor
            ' pornite aici se reface din `_pozitie`, deci trebuie să fie deja la zi când începe
            ' prima suprafață să citească.
            ActualizeazaReceptiileNoi()
            ' Benzile se golesc AICI, cu arborii, nu în `ReconstruiesteBenzi` — pe drumul de mai
            ' jos cu `_stare Is Nothing` metoda aia nici nu se mai cheamă, iar o suprafață rămasă
            ' plină lângă doi arbori goliți ar fi singurul lucru de pe ecran care mai susține că
            ' există date.
            ReconstruiesteBenzi()
            If _stare Is Nothing Then Return

            ' ── stânga: recepțiile, fiecare cu lanțul ei ordonat după DataH ──────────
            For Each rec As ReceptiePropusa In Receptiile().OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
                Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(rec)

                Dim nod As AdvancedTreeControl.TreeItem =
                    treeLant.AddItem(CHEIE_RECEPTIE & rec.Idrr, CaptionReceptie(rec, lant), pExpanded:=True, pLeftIconClosed:=Il_Receptii.Images.Item("Receptii"))
                nod.Tag = rec
                nod.Bold = True
                ' ITALIC = a receipt BORN by this download (operator, 09.09.2026). Every root is
                ' bold, so bold alone told nothing apart; italic on top of it reads at a glance
                ' and costs no new control. The sign is `RandReceptie`, not a list kept on the
                ' side: the server sets it on EXACTLY the receipts created in the current run
                ' (it is their index in `ListaReceptii`, slice 0056) and leaves it Nothing for
                ' every other one. The ones that came from the server stay merely bold.
                ' O recepție pornită de operator din coșul celor neașezate e tot una care încă
                ' nu există în bază, deci poartă același semn ca una născută de descărcare.
                nod.Italic = rec.RandReceptie.HasValue OrElse EsteReceptieNoua(rec)
                ColoreazaReceptia(nod, rec)
                nod.Tooltip = TooltipReceptie(rec, lant)
                _nodReceptie(rec.Idrr) = nod

                For Each inst As InstantaneuLegat In lant
                    Dim frunza As AdvancedTreeControl.TreeItem =
                        treeLant.AddItem(CHEIE_INSTANTANEU & inst.Idrh, CaptionInstantaneu(inst), nod, pLeftIconClosed:=Il_Receptii.Images.Item("Receptii_Link"))
                    frunza.Tag = inst
                    frunza.Tooltip = TooltipInstantaneu(inst)
                    If inst.Blocat Then frunza.RightIcon = Il_Receptii.Images.Item("Lock")
                    ColoreazaInstantaneu(frunza, inst)
                    _nodInstantaneu(inst.Idrh) = New RandDeArbore(treeLant, frunza)
                Next
            Next

            ' ── dreapta: instantaneele neașezate, sub o rădăcină care e ȘI ținta de
            ' desprindere. Fără rădăcină, un arbore gol n-ar avea pe ce să primească
            ' primul instantaneu desprins.
            Dim libere As List(Of InstantaneuLegat) =
                _stare.Instantanee.Where(Function(i) PozitiaLui(i) = 0).
                                   OrderBy(Function(i) i.DataH).ThenBy(Function(i) i.Idrh).ToList()

            Dim radacina As AdvancedTreeControl.TreeItem =
                treeLibere.AddItem(CHEIE_LIBERE, $"Neașezate ({libere.Count})", pExpanded:=True)
            radacina.Bold = True
            radacina.Tooltip = "Trage aici un instantaneu ca să-l desprinzi de recepția lui." &
                               Environment.NewLine &
                               "Clic dreapta pe un instantaneu de aici: «Începe o recepție nouă»."

            For Each inst As InstantaneuLegat In libere
                Dim frunza As AdvancedTreeControl.TreeItem =
                    treeLibere.AddItem(CHEIE_INSTANTANEU & inst.Idrh, CaptionInstantaneu(inst), radacina)
                frunza.Tag = inst
                frunza.Tooltip = TooltipInstantaneu(inst)
                ColoreazaInstantaneu(frunza, inst)
                _nodInstantaneu(inst.Idrh) = New RandDeArbore(treeLibere, frunza)
            Next

            ' O recepție pornită aici, dar încă nescriibilă, oprește salvarea ÎNTREAGĂ: serverul
            ' refuză tot pachetul, nu doar recepția cu pricina. Se spune înainte, și se spune ce
            ' anume lipsește — un buton stins fără motiv l-ar pune pe operator să caute.
            Dim problemaNoua As String = ProblemaReceptiilorNoi()

            If _mod = ModAsociere.Propunere Then
                ' Acoperire OBLIGATORIE: serverul refuză cu 400 o salvare căreia îi lipsește
                ' fie și o singură hotărâre. Butonul spune asta înainte, iar mesajul numără cât
                ' a mai rămas — altfel operatorul ar afla abia din eroare.
                Dim ramase As Integer = NehotarateleCount()
                btnSalveaza.Enabled = (ramase = 0 AndAlso problemaNoua = String.Empty)
                If ramase > 0 Then
                    ntfMesaj.Show($"Mai sunt {ramase} instantanee neașezate. Trage-le pe recepția lor " &
                                  "sau marchează-le «fără schimbare»; până atunci nimic nu se scrie.",
                                  NoticeKind.Warning)
                ElseIf problemaNoua <> String.Empty Then
                    ntfMesaj.Show(problemaNoua, NoticeKind.Warning)
                Else
                    ntfMesaj.Show("Toate instantaneele au primit o hotărâre — poți salva descărcarea.",
                                  NoticeKind.Success)
                End If
            Else
                btnSalveaza.Enabled = problemaNoua = String.Empty AndAlso Comenzi().Count > 0
                If problemaNoua <> String.Empty Then
                    ntfMesaj.Show(problemaNoua, NoticeKind.Warning)
                    _avertismentReceptiiNoi = True
                ElseIf _avertismentReceptiiNoi Then
                    ' Se ia jos ce am pus tot noi, și numai atunci. Fără steag, un `Clear` la
                    ' fiecare reconstruire ar șterge și mesajele altcuiva — de pildă «legăturile
                    ' au fost salvate», care vine tocmai după o reîncărcare.
                    ntfMesaj.Clear()
                    _avertismentReceptiiNoi = False
                End If
            End If
            ' The scroll, restored AFTER both trees are full: the setter rebuilds the bar's
            ' range first, so only now does it have anything to measure against.
            treeLant.ScrollOffsetY = derulareLant
            treeLibere.ScrollOffsetY = derulareLibere
            treeLant.Invalidate()
            treeLibere.Invalidate()
            ' The chart reads the LOCAL picture too, so a drag has to move it as well. Rebuilding
            ' it here and nowhere else keeps it from drifting away from the trees for exactly the
            ' reason the trees themselves are rebuilt whole.
            ReconstruiesteGrafic()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Reconstruieste", ex)
        End Try
    End Sub

    ''' <summary>Unde stă acum instantaneul, DUPĂ mutările locale.</summary>
    Private Function PozitiaLui(inst As InstantaneuLegat) As Integer
        Dim idrr As Integer
        If _pozitie.TryGetValue(inst.Idrh, idrr) Then Return idrr
        Return inst.Idrr
    End Function

    Private Function EsteIgnorat(idrh As Integer) As Boolean
        Dim v As Boolean
        If _ignorat.TryGetValue(idrh, v) Then Return v
        Return False
    End Function

    ''' <summary>
    ''' Instantaneul stă pe o recepție pornită AICI? Se citește direct din <c>_pozitie</c>, fiindcă
    ''' numai de acolo poate veni un IDRR negativ — serverul nu trimite niciodată unul.
    ''' </summary>
    Private Function EstePeReceptieNoua(idrh As Integer) As Boolean
        Dim idrr As Integer
        If _pozitie.TryGetValue(idrh, idrr) Then Return idrr < 0
        Return False
    End Function

    ''' <summary>
    ''' Instantaneul e rândul de ștergere al lanțului lui (F21).
    ''' </summary>
    ''' <remarks>
    ''' Două regimuri, fiindcă sunt două întrebări diferite. Pe o recepție venită de la server,
    ''' ștergerea e o HOTĂRÂRE a operatorului: recepția poate foarte bine să fie încă vie, iar
    ''' mașina nu poate ghici dacă ultimul instantaneu o închide sau e doar cel mai recent. Pe o
    ''' recepție pornită aici nu e nimic de hotărât — ea există tocmai fiindcă a fost ștearsă
    ''' (F26), altfel ar fi venit în `ListaReceptii` ca toate celelalte —, deci ultimul
    ''' instantaneu al lanțului e ștergerea ei și nu are cine să spună altceva.
    ''' </remarks>
    Private Function EsteStergere(idrh As Integer) As Boolean
        If EstePeReceptieNoua(idrh) Then Return _stergereNoua.Contains(idrh)
        Dim v As Boolean
        If _stergere.TryGetValue(idrh, v) Then Return v
        Return False
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Recepțiile pornite din coșul celor neașezate (F26)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' O recepție pe care nu o are baza, ci a pornit-o operatorul aici. Semnul e IDRR-ul
    ''' NEGATIV, și e negativ tocmai ca să nu se poată confunda niciodată cu unul adevărat.
    ''' </summary>
    Private Shared Function EsteReceptieNoua(rec As ReceptiePropusa) As Boolean
        Return rec IsNot Nothing AndAlso rec.Idrr < 0
    End Function

    ''' <summary>
    ''' Numele sub care pleacă pe fir o recepție pornită aici — o ETICHETĂ dată de client, nu
    ''' o cheie: recepția nu există încă, deci nu are un IDRR de numit. Serverul o
    ''' materializează la salvare și îi dă cheia adevărată.
    ''' </summary>
    Private Shared Function EtichetaTrimisa(idrr As Integer) As String
        Return "R" & (-idrr).ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Recepțiile de pe server PLUS cele pornite aici — tot ce se vede pe ecran.</summary>
    Private Function Receptiile() As IEnumerable(Of ReceptiePropusa)
        If _stare Is Nothing Then Return _receptiiNoi
        Return _stare.Receptii.Concat(_receptiiNoi)
    End Function

    ''' <summary>
    ''' Reface lista recepțiilor pornite aici din <c>_pozitie</c>, singura evidență.
    '''
    ''' <para>Nu se ține nimic pe lângă: o recepție nouă ESTE mulțimea instantaneelor așezate pe
    ''' IDRR-ul ei negativ. Când ultimul pleacă de pe ea, recepția dispare — nu «rămâne goală»,
    ''' fiindcă o recepție fără niciun instantaneu n-are cu ce fi scrisă și n-are ce arăta.</para>
    '''
    ''' <para><b>Cifrele se recalculează, nu se rețin de la creare.</b> Serverul le va scrie
    ''' chiar așa: <c>DataR</c> = momentul celui mai vechi instantaneu din lanț, <c>SumaAntet</c>
    ''' = totalul rândului de ștergere (cât valora când a plecat). Un rând care ar arăta altceva
    ''' decât ce se va scrie ar fi singura minciună de pe ecran. Din același motiv poartă și
    ''' <c>Sters</c>, și <c>Reconstituit</c>: <c>_R_INSERT_RECONST_SQL</c> le scrie pe amândouă
    ''' cu 1, deci o recepție pornită aici e ștearsă din clipa în care se naște.</para>
    ''' </summary>
    Private Sub ActualizeazaReceptiileNoi()
        _receptiiNoi.Clear()
        _stergereNoua.Clear()
        If _stare Is Nothing Then Return

        For Each idrr As Integer In _stare.Instantanee.Select(AddressOf PozitiaLui).
                                                       Where(Function(x) x < 0).
                                                       Distinct().OrderByDescending(Function(x) x)
            Dim lant As List(Of InstantaneuLegat) =
                _stare.Instantanee.Where(Function(i) PozitiaLui(i) = idrr).
                                   OrderBy(Function(i) i.DataH).ThenBy(Function(i) i.Idrh).ToList()
            If lant.Count = 0 Then Continue For

            Dim ultimul As InstantaneuLegat = lant.Last()
            ' Ultimul ÎNCHIDE lanțul — dar numai dintr-un lanț adevărat. Cât are un singur
            ' instantaneu, recepția e pe jumătate făcută: acela o pornește, și n-are ce închide
            ' încă. Marcat oricum, ecranul ar arăta o recepție gata (bandă terminată cu cruce,
            ' valoare căzută la zero după ea) exact acolo unde salvarea e oprită.
            If lant.Count > 1 Then _stergereNoua.Add(ultimul.Idrh)
            _receptiiNoi.Add(New ReceptiePropusa() With {
                .Idrr = idrr,
                .DataR = lant(0).DataH.Date,
                .SumaAntet = ultimul.Total,
                .Descriere = ultimul.Descriere,
                .Sters = True,
                .Reconstituit = True})
        Next

        ' Selecția putea sta pe o recepție care tocmai s-a golit. Se lasă goală în loc să
        ' rămână pe un obiect pe care nimeni nu-l mai desenează.
        If EsteReceptieNoua(_receptieSelectata) AndAlso
           Not _receptiiNoi.Any(Function(r) r.Idrr = _receptieSelectata.Idrr) Then
            _receptieSelectata = Nothing
        End If
    End Sub

    ''' <summary>Următorul IDRR local liber: -1, apoi -2, … Niciodată reciclat cât ține fereastra.</summary>
    ''' <remarks>
    ''' Nu se reciclează fiindcă eticheta trimisă se face din el: două recepții pornite una după
    ''' alta pe același număr ar ajunge la server cu aceeași etichetă, iar acolo o etichetă e
    ''' declarată o singură dată.
    ''' </remarks>
    Private Function UrmatorulIdrrNou() As Integer
        Dim minim As Integer = 0
        For Each idrr As Integer In _pozitie.Values
            If idrr < minim Then minim = idrr
        Next
        Return minim - 1
    End Function

    ''' <summary>
    ''' Ce mai lipsește ca recepțiile pornite aici să poată fi scrise. Șir gol = se poate salva.
    '''
    ''' <para>Se verifică ÎNAINTE de salvare fiindcă serverul refuză toată salvarea, nu doar
    ''' recepția cu pricina, iar operatorul ar afla abia din eroare ce anume îi lipsea.</para>
    '''
    ''' <para>A rămas o singură condiție, și e a serverului cuvânt cu cuvânt (§4c-bis): un lanț
    ''' reconstituit are exact o «reconstituire» și exact o «ștergere», iar ștergerea e ultima.
    ''' Celelalte două nu se mai pot încălca de când capetele lanțului sunt CITITE din el, nu
    ''' marcate: ultimul e ștergerea, primul o declară, deci singurul fel în care se pot ciocni e
    ''' să fie unul și același — adică lanțul să aibă un singur instantaneu.</para>
    ''' </summary>
    Private Function ProblemaReceptiilorNoi() As String
        For Each rec As ReceptiePropusa In _receptiiNoi
            Dim problema As String = ProblemaUneiReceptiiNoi(rec, LantulReceptiei(rec))
            If problema <> String.Empty Then Return problema
        Next
        Return String.Empty
    End Function

    ''' <summary>Aceeași verificare, pentru o singură recepție — și pentru tooltipul ei.</summary>
    Private Function ProblemaUneiReceptiiNoi(rec As ReceptiePropusa,
                                             lant As List(Of InstantaneuLegat)) As String
        If rec Is Nothing OrElse lant Is Nothing Then Return String.Empty
        If lant.Count < 2 Then
            Return $"Recepția nouă din {rec.DataR:dd.MM.yyyy} are un singur instantaneu. " &
                   "Mai trage cel puțin unul pe ea: cel dintâi o pornește, ultimul e ștergerea " &
                   "ei, iar același instantaneu nu poate fi și una, și alta."
        End If
        Return String.Empty
    End Function

    ''' <summary>Cele două capete ale unui lanț pornit aici, citite dintr-o singură ordonare.</summary>
    Private Structure CapeteLant
        ''' <summary>Primul după DataH — el DECLARĂ eticheta, prin «reconstituire».</summary>
        Public Declara As Integer
        ''' <summary>Ultimul după DataH — el e rândul de ȘTERGERE, fiindcă recepția a fost ștearsă.</summary>
        Public Inchide As Integer
    End Structure

    ''' <summary>
    ''' Capetele fiecărei recepții pornite aici.
    ''' </summary>
    ''' <remarks>
    ''' Se calculează o singură dată, aici, nu în bucla care scrie comenzile: serverul cere EXACT
    ''' o «reconstituire» și EXACT o «ștergere» per etichetă, iar o alegere luată rând cu rând ar
    ''' putea nimeri două — sau niciuna. Amândouă capetele ies din aceeași ordonare, ca să nu
    ''' existe două locuri în care se hotărăște ce înseamnă «primul» și «ultimul».
    ''' </remarks>
    Private Function CapeteleReceptiilorNoi() As Dictionary(Of Integer, CapeteLant)
        Dim out As New Dictionary(Of Integer, CapeteLant)()
        For Each rec As ReceptiePropusa In _receptiiNoi
            Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(rec)
            If lant.Count = 0 Then Continue For
            out(rec.Idrr) = New CapeteLant() With {
                .Declara = lant(0).Idrh,
                .Inchide = lant.Last().Idrh}
        Next
        Return out
    End Function

    ''' <summary>
    ''' Aceeași alegere, calculată din tabloul dat pe parametri — pentru <see cref="DeciziiDin"/>,
    ''' care e <c>Shared</c> și nu are voie să citească câmpurile formularului.
    ''' </summary>
    Private Shared Function CapeteleDin(instantanee As IEnumerable(Of InstantaneuLegat),
                                        pozitie As IReadOnlyDictionary(Of Integer, Integer)) _
                                        As Dictionary(Of Integer, CapeteLant)
        Dim primul As New Dictionary(Of Integer, InstantaneuLegat)()
        Dim ultimul As New Dictionary(Of Integer, InstantaneuLegat)()
        For Each inst As InstantaneuLegat In instantanee
            Dim idrr As Integer = inst.Idrr
            If pozitie IsNot Nothing AndAlso pozitie.ContainsKey(inst.Idrh) Then idrr = pozitie(inst.Idrh)
            If idrr >= 0 Then Continue For
            Dim decand As InstantaneuLegat = Nothing
            If Not primul.TryGetValue(idrr, decand) OrElse
               inst.DataH < decand.DataH OrElse
               (inst.DataH = decand.DataH AndAlso inst.Idrh < decand.Idrh) Then
                primul(idrr) = inst
            End If
            If Not ultimul.TryGetValue(idrr, decand) OrElse
               inst.DataH > decand.DataH OrElse
               (inst.DataH = decand.DataH AndAlso inst.Idrh > decand.Idrh) Then
                ultimul(idrr) = inst
            End If
        Next
        Dim out As New Dictionary(Of Integer, CapeteLant)()
        For Each kvp As KeyValuePair(Of Integer, InstantaneuLegat) In primul
            out(kvp.Key) = New CapeteLant() With {
                .Declara = kvp.Value.Idrh,
                .Inchide = ultimul(kvp.Key).Idrh}
        Next
        Return out
    End Function

    ''' <summary>
    ''' Ce acțiune poartă un instantaneu așezat pe o recepție pornită aici: primul o DECLARĂ,
    ''' ultimul o închide, restul se așază pe ea.
    ''' </summary>
    ''' <remarks>
    ''' Declarația se verifică prima, și de-asta nu mai trebuie nimic pentru lanțul de un singur
    ''' instantaneu: acolo cele două capete sunt același rând, iar el pornește recepția. Că nu o
    ''' și închide o spune <see cref="ProblemaUneiReceptiiNoi"/>, care oprește salvarea înainte
    ''' să se ajungă aici.
    ''' </remarks>
    Private Shared Function ActiuneaPeReceptieNoua(idrh As Integer, capete As CapeteLant) As ActiuneAsociere
        If idrh = capete.Declara Then Return ActiuneAsociere.Reconstituire
        If idrh = capete.Inchide Then Return ActiuneAsociere.Stergere
        Return ActiuneAsociere.Asociat
    End Function

    Private Function CaptionReceptie(rec As ReceptiePropusa, lant As List(Of InstantaneuLegat)) As String
        Dim semne As String = String.Empty
        If EsteReceptieNoua(rec) Then
            ' UN singur semn, deși recepția poartă și `Sters`, și `Reconstituit`: cele trei scrise
            ' unul după altul ar spune de trei ori același lucru și ar umple rândul. «Ștearsă» e
            ' partea care NU se subînțelege — o recepție pornită aici e ștearsă prin definiție, și
            ' e singurul fel în care poate exista (F26).
            '
            ' Și DOAR atât: ce anume îi mai lipsește se scrie în tooltip și în banda de mesaje,
            ' care au loc pentru o propoziție. Partea din dreapta a rândului e ALINIATĂ LA DREAPTA
            ' și, când nu încape, arborele o lasă nedesenată cu totul — verificat pe ecran, un
            ' semn mai lung a făcut să dispară și suma, și numărul de instantanee.
            semne = " [nouă, ștearsă]"
        Else
            ' TEXT, nu pictograme: cele două steaguri sunt fapte diferite și au nevoie fiecare de
            ' cuvântul lui, iar o pictogramă ar cere un control nou în designer.
            If rec.Sters Then semne &= " [ștearsă]"
            If rec.Reconstituit Then semne &= " [reconstituită]"
        End If
        Return $"{rec.DataR:dd.MM.yyyy}~~~{Bani(rec.SumaAntet)} ({lant.Count}){semne}"
    End Function

    ''' <summary>
    ''' Culoarea rândului-recepție: o recepție ȘTEARSĂ se scrie stins, oricare ar fi ea.
    ''' </summary>
    ''' <remarks>
    ''' <para>Stins, nu roșu, și nu scos din calcule: recepția ștearsă rămâne o recepție
    ''' întreagă. Ordonanțările dinaintea ștergerii îi citesc totalul așa cum stătea atunci
    ''' (§1.3), lanțul ei e în grafic și pe benzi ca oricare altul, iar un instantaneu ANTERIOR
    ''' ștergerii se poate așeza pe ea în continuare. Culoarea spune «asta nu mai e pe site», nu
    ''' «asta nu contează».</para>
    ''' <para>Aceeași culoare și pentru cele de la server, și pentru cele pornite aici. E același
    ''' fapt, iar două recepții amândouă șterse, dintre care numai una se vede că e, ar fi mai
    ''' rău decât nicio culoare.</para>
    ''' </remarks>
    ''' <remarks>
    ''' <para><b>FONDUL, nu culoarea textului.</b> Textul rândului-recepție e deja luat: pe fila
    ''' «Tot angajamentul» el poartă culoarea liniei din grafic, adică lucrul care leagă rândul de
    ''' linia lui (<see cref="SincronizeazaCulorile"/>). Scrisă tot acolo, ștergerea ar fi ori
    ''' ștearsă de identitate, ori ar șterge-o pe ea — și tocmai la recepția pornită aici
    ''' operatorul are mai multă nevoie să vadă care linie e a ei. Fondul nu e cerut de nimeni
    ''' altcineva, deci cele două fapte încap amândouă, pe amândouă filele.</para>
    ''' <para>O nuanță, nu o culoare tare: recepția ștearsă rămâne o recepție întreagă.
    ''' Ordonanțările dinaintea ștergerii îi citesc totalul așa cum stătea atunci (§1.3), lanțul ei
    ''' e în grafic și pe benzi ca oricare altul, iar un instantaneu ANTERIOR ștergerii se poate
    ''' așeza pe ea în continuare. Fondul spune «asta nu mai e pe site», nu «asta nu contează».</para>
    ''' <para>Aceeași nuanță și pentru cele de la server, și pentru cele pornite aici: e același
    ''' fapt. Două recepții amândouă șterse, dintre care numai una se vede că e, ar fi mai rău
    ''' decât nicio culoare.</para>
    ''' </remarks>
    Private Sub ColoreazaReceptia(nod As AdvancedTreeControl.TreeItem, rec As ReceptiePropusa)
        If nod Is Nothing Then Return
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta Is Nothing OrElse rec Is Nothing Then Return
        ' `SurfaceAlt` singur nu se vede: în temele deschise e chiar alb, adică fondul arborelui.
        ' Nuanța se AMESTECĂ, deci vine tot din paletă și se întoarce singură pe dos în tema
        ' întunecată — acolo textul stins e mai deschis decât fondul, deci banda iese mai
        ' deschisă, nu mai închisă.
        nod.NodeBackColor = If(rec.Sters,
                               Amesteca(paleta.SurfaceAltColor, paleta.TextDimColor, 0.14),
                               Color.Empty)
    End Sub

    ''' <summary>Două culori din paletă, amestecate. Nicio culoare scrisă în cod (regula casei).</summary>
    ''' <remarks>
    ''' Canalele se URCĂ la <c>Double</c> înainte de scădere. <c>Color.R</c> e un <c>Byte</c>, iar
    ''' în VB scăderea a doi Byte se face tot pe Byte: la prima recepție ștearsă cu fondul mai
    ''' deschis decât textul, diferența e negativă și aruncă <c>OverflowException</c>. Prins pe
    ''' ecran, nu de teste — arborele își înghite excepția (graniță de UI) și se oprea din desenat
    ''' la jumătate, cu rândul-recepție scris și fără niciun instantaneu sub el.
    ''' </remarks>
    Private Shared Function Amesteca(fond As Color, peste As Color, cat As Double) As Color
        Dim k As Double = Math.Max(0.0, Math.Min(1.0, cat))
        Return Color.FromArgb(
            CInt(Math.Round(CDbl(fond.R) + (CDbl(peste.R) - CDbl(fond.R)) * k)),
            CInt(Math.Round(CDbl(fond.G) + (CDbl(peste.G) - CDbl(fond.G)) * k)),
            CInt(Math.Round(CDbl(fond.B) + (CDbl(peste.B) - CDbl(fond.B)) * k)))
    End Function

    Private Function CaptionInstantaneu(inst As InstantaneuLegat) As String
        Dim semne As String = String.Empty
        'If inst.Blocat Then semne &= "  🔒"
        If EsteStergere(inst.Idrh) Then semne &= "  [ștergere]"
        If EsteIgnorat(inst.Idrh) Then semne &= "  [fără schimbare]"
        ' NOTHING about `DataR` here, and nothing in the two tooltips either (operator,
        ' 09.09.2026). This was the last remnant of the withdrawn F13: a sign that lit up on
        ' perfectly correct data, on row after row, because `DataR` is typed by hand on the
        ' site and says nothing about when the receipt appeared (F29). A warning that is
        ' almost always wrong is not an observation, it is noise on every line — so it is
        ' gone, veto and sign both. The `rec` parameter stays: the caller has it, and the two
        ' call sites read better naming the receipt the snapshot sits on.
        Return $"{inst.DataH:dd.MM.yyyy HH:mm}~~~{Bani(inst.Total)}{semne}"
    End Function

    ''' <summary>
    ''' Un instantaneu blocat se scrie cu culoarea textului stins, unul ignorat la fel.
    ''' Culorile vin din paletă, niciodată scrise în cod (regula casei).
    ''' </summary>
    Private Sub ColoreazaInstantaneu(nod As AdvancedTreeControl.TreeItem, inst As InstantaneuLegat)
        nod.NodeForeColor = CuloareDeBaza(inst)
        If EsteStergere(inst.Idrh) Then nod.Italic = True
    End Sub

    ''' <summary>
    ''' What a snapshot row is worth in colour BEFORE the chart has a say: the disabled grey for a
    ''' row that cannot be moved or that says nothing, otherwise <c>Color.Empty</c> — the tree's
    ''' own «take it from the theme».
    ''' </summary>
    ''' <remarks>
    ''' Pulled out of <see cref="ColoreazaInstantaneu"/> because the colouring pass has to be able
    ''' to put a row BACK where it started when the chart stops having an opinion about it — which
    ''' happens on every switch between the two chart views.
    ''' </remarks>
    Private Function CuloareDeBaza(inst As InstantaneuLegat) As Color
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta Is Nothing Then Return Color.Empty
        If inst.Blocat OrElse EsteIgnorat(inst.Idrh) Then Return paleta.DisabledTextColor
        Return Color.Empty
    End Function

    Private Function TooltipReceptie(rec As ReceptiePropusa, lant As List(Of InstantaneuLegat)) As String
        Dim sb As New Text.StringBuilder()
        ' The number is written ONLY when it is a real one. A receipt born by this download got
        ' its `IDRR` inside the transaction phase one rolled back, so the number it carries now
        ' is not the one it will have after the save — showing it would send the operator
        ' looking for something that does not exist (slice 0056, and the 09.09.2026 complaint).
        If EsteReceptieNoua(rec) Then
            ' Nici numărul, nici data nu sunt încă fapte: recepția se scrie abia la salvare, iar
            ' cifrele de aici se recalculează din lanț la fiecare mutare (vezi
            ' `ActualizeazaReceptiileNoi`).
            sb.AppendLine("Recepție pornită de dumneavoastră, încă nescrisă în bază.")
            sb.AppendLine($"Data ei va fi {rec.DataR:dd.MM.yyyy} — cel mai vechi instantaneu al lanțului.")
            ' Se spune, nu se subînțelege. O recepție pornită de aici există TOCMAI fiindcă a
            ' fost ștearsă înainte de prima descărcare (F26) — altfel ar fi venit în
            ' `ListaReceptii` ca toate celelalte. Nu e o stare pe care operatorul o poate alege,
            ' deci nici nu i se cere: se scrie ce se va întâmpla.
            sb.AppendLine("Se va scrie ca recepție ȘTEARSĂ și reconstituită — o recepție de " &
                          "aici există tocmai fiindcă a fost ștearsă, altfel n-ar fi lipsit " &
                          "din listă. Ultimul instantaneu al lanțului e ștergerea ei.")
            ' Ce îi mai lipsește se scrie AICI, nu pe rând: tooltipul are loc pentru o propoziție,
            ' iar rândul o pierde întreagă când nu încape (vezi CaptionReceptie).
            Dim lipsa As String = ProblemaUneiReceptiiNoi(rec, lant)
            If lipsa <> String.Empty Then sb.AppendLine("⚠ " & lipsa)
        ElseIf rec.RandReceptie.HasValue Then
            sb.AppendLine($"Recepție NOUĂ, din descărcarea curentă · creată {rec.DataR:dd.MM.yyyy}")
        Else
            sb.AppendLine($"Recepția {rec.Idrr} · creată {rec.DataR:dd.MM.yyyy}")
        End If
        ' «Acum» ar fi o minciună pentru o recepție pornită aici: suma ei e cât valora în clipa
        ' ștergerii, fiindcă exact aia se scrie în `SumaAntet` (vezi `ActualizeazaReceptiileNoi`).
        ' Pentru celelalte rămâne cum era — ce le-a trimis serverul, oricare ar fi povestea lor.
        sb.AppendLine(If(EsteReceptieNoua(rec),
                         $"Valoarea la ștergere: {Bani(rec.SumaAntet)}",
                         $"Valoare acum: {Bani(rec.SumaAntet)}"))
        If lant.Count > 0 Then
            Dim ultimul As InstantaneuLegat = lant.Last()
            sb.AppendLine($"Ultimul instantaneu: {ultimul.DataH:dd.MM.yyyy} · {Bani(ultimul.Total)}")
            ' F15 ca SEMN, nu ca refuz — exact cum îl descrie fundamentul §1.5. Nu se
            ' verifică pe un lanț terminat în ștergere: a-l compara cu starea de ACUM nu
            ' înseamnă nimic.
            If Not EsteStergere(ultimul.Idrh) AndAlso
               Math.Round(ultimul.Total, 2) <> Math.Round(rec.SumaAntet, 2) Then
                sb.AppendLine("⚠ Lanțul nu se închide: ultimul instantaneu nu are valoarea de acum.")
            End If
        Else
            sb.AppendLine("Nu are niciun instantaneu.")
        End If
        ' No "older than the receipt's date" count here any more - see CaptionInstantaneu.
        If rec.ReconstituitNesigur Then
            sb.AppendLine("⚠ Reconstituire nesigură: gruparea a fost o judecată, nu o verificare.")
        End If
        Return sb.ToString().TrimEnd()
    End Function

    ''' <summary>Lanțul se închide pe valoarea de acum (F15) — semnul de la capătul benzii.</summary>
    ''' <remarks>
    ''' Nu se aplică pe un lanț terminat în ștergere: a compara rândul de ștergere cu starea de
    ''' ACUM nu înseamnă nimic (F15 spune asta explicit). Nici pe un lanț gol — nu are capăt.
    ''' </remarks>
    Private Function SemnulCapatului(rec As ReceptiePropusa, lant As List(Of InstantaneuLegat)) As KBotLaneEndMark
        If rec Is Nothing OrElse lant Is Nothing OrElse lant.Count = 0 Then Return KBotLaneEndMark.None
        Dim ultimul As InstantaneuLegat = lant.Last()
        If EsteStergere(ultimul.Idrh) Then Return KBotLaneEndMark.None
        If Math.Round(ultimul.Total, 2) = Math.Round(rec.SumaAntet, 2) Then Return KBotLaneEndMark.Ok
        Return KBotLaneEndMark.Warning
    End Function

    Private Function TooltipInstantaneu(inst As InstantaneuLegat) As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine($"{inst.DataH:dd.MM.yyyy HH:mm:ss} · {Bani(inst.Total)}")
        If Not String.IsNullOrWhiteSpace(inst.Descriere) Then sb.AppendLine(inst.Descriere)
        Dim ind As String = String.Join(", ", inst.Indicatori().OrderBy(Function(x) x))
        If ind <> "" Then sb.AppendLine($"Indicatori: {ind}")
        ' No "older than the receipt's date" warning here any more — see CaptionInstantaneu.
        If inst.Blocat Then
            sb.AppendLine()
            sb.AppendLine("NU SE MAI POATE MUTA:")
            For Each m As String In inst.Motive
                sb.AppendLine("• " & m)
            Next
        End If
        Return sb.ToString().TrimEnd()
    End Function

    Private Shared Function Bani(v As Double) As String
        Return v.ToString("N2", _roCulture)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Tragerea
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub Tree_NodeDragStarting(sender As Object, e As TreeDragStartEventArgs) Handles treeLant.NodeDragStarting, treeLibere.NodeDragStarting
        Try
            Dim inst = TryCast(e.Item?.Tag, InstantaneuLegat)
            ' Rădăcinile de recepție NU se trag (D-J), și nici rădăcina «neașezate».
            If inst Is Nothing Then e.Cancel = True : Return
            ' Legătură înghețată: vizibilă, dar nu de mutat. Se oprește din pornire, ca operatorul
            ' să simtă refuzul înainte să facă gestul, nu după.
            If inst.Blocat Then e.Cancel = True : Return
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Tree_NodeDragStarting", ex)
            e.Cancel = True
        End Try
    End Sub

    Private Sub TreeLant_NodeDragOver(sender As Object, e As TreeDragOverEventArgs) Handles treeLant.NodeDragOver
        Try
            Dim inst = TryCast(e.Source?.Tag, InstantaneuLegat)
            If inst Is Nothing Then e.Allow = False : Return

            Dim rec = ReceptiaTintei(e.Target)
            If rec Is Nothing Then
                e.Allow = False
                e.Motiv = "Aruncă instantaneul pe o recepție."
                Return
            End If

            If PozitiaLui(inst) = rec.Idrr Then
                e.Allow = False
                e.Motiv = "Instantaneul este deja pe această recepție."
                Return
            End If

            Dim motiv = MotivulRefuzului(inst, rec)
            e.Allow = motiv = String.Empty
            e.Motiv = motiv
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.TreeLant_NodeDragOver", ex)
            e.Allow = False
        End Try
    End Sub

    Private Sub TreeLibere_NodeDragOver(sender As Object, e As TreeDragOverEventArgs) Handles treeLibere.NodeDragOver
        Try
            Dim inst = TryCast(e.Source?.Tag, InstantaneuLegat)
            If inst Is Nothing Then e.Allow = False : Return

            If PozitiaLui(inst) = 0 Then
                e.Allow = False
                e.Motiv = "Instantaneul este deja neașezat."
                Return
            End If
            e.Allow = True
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.TreeLibere_NodeDragOver", ex)
            e.Allow = False
        End Try
    End Sub

    ''' <summary>
    ''' Recepția pe care s-a aruncat: fie rândul-recepție însuși, fie recepția rândului pe care
    ''' a nimerit cursorul. A ținti un instantaneu al lanțului înseamnă tot «pe recepția asta» —
    ''' altfel operatorul ar trebui să nimerească exact antetul.
    ''' </summary>
    Private Function ReceptiaTintei(tinta As AdvancedTreeControl.TreeItem) As ReceptiePropusa
        If tinta Is Nothing Then Return Nothing
        Dim rec As ReceptiePropusa = TryCast(tinta.Tag, ReceptiePropusa)
        If rec IsNot Nothing Then Return rec
        Dim inst As InstantaneuLegat = TryCast(tinta.Tag, InstantaneuLegat)
        If inst Is Nothing Then Return Nothing
        Dim idrr As Integer = PozitiaLui(inst)
        If idrr = 0 Then Return Nothing
        Return Receptiile().FirstOrDefault(Function(r) r.Idrr = idrr)
    End Function

    ''' <summary>
    ''' Vetourile, aplicate ÎNAINTE de aruncare. Șir gol = se poate.
    '''
    ''' <para>F14 (indicatorii) și F16 (mulțimile doar cresc) — aceleași două pe care le
    ''' verifică și serverul, și tot ridicând, nu corectând. Se repetă aici nu din neîncredere,
    ''' ci ca refuzul să ajungă la operator în timpul gestului. F15 (capătul lanțului) NU e
    ''' aici: el e un semn, nu un veto, și trăiește în eticheta recepției.</para>
    '''
    ''' <para><b>F13 nu mai e aici deloc</b> — retras pe 31.08.2026. <c>FX_Receptii_R.DataR</c>
    ''' nu e momentul creării: e un câmp obișnuit, pe care operatorul îl scrie pe site și îl
    ''' poate schimba după aceea, iar <c>FX_Receptii_R</c> nu are NICIO coloană cu momentul
    ''' creării (F29). Un veto clădit pe un câmp tastat refuză plasări corecte. Comparația a mai
    ''' trăit un timp ca SEMN pe rând și în etichete; pe 09.09.2026 operatorul a cerut și acel
    ''' rest scos — se aprindea pe date corecte, deci nu spunea nimic. <b>Nu mai există nicăieri
    ''' în formular niciun cuvânt despre <c>DataR</c> ca avertisment.</b></para>
    ''' </summary>
    Private Function MotivulRefuzului(inst As InstantaneuLegat, rec As ReceptiePropusa) As String
        Dim indInst As HashSet(Of String) = inst.Indicatori()
        Dim indRec As New HashSet(Of String)(
            rec.Rhr.Where(Function(l) Not String.IsNullOrEmpty(l.CodIndicator)).Select(Function(l) l.CodIndicator),
            StringComparer.OrdinalIgnoreCase)

        ' F14 — submulțimea de indicatori. Slab (majoritatea angajamentelor au un singur
        ' indicator), dar corect.
        '
        ' NU se aplică unei recepții pornite aici: ea nu are linii pe indicator și nici nu poate
        ' avea. Serverul i le scrie la salvare CHIAR DIN instantaneele lanțului (ultimul dinainte
        ' de ștergere), deci mulțimea față de care s-ar compara e tocmai cea care se construiește
        ' acum. Verificat oricum, ar refuza primul instantaneu al fiecărei recepții noi — adică
        ' exact gestul care le creează.
        If Not EsteReceptieNoua(rec) AndAlso indInst.Count > 0 AndAlso Not indInst.IsSubsetOf(indRec) Then
            Dim lipsa As String = String.Join(", ", indInst.Except(indRec).OrderBy(Function(x) x))
            Return $"Instantaneul numește indicatorii {lipsa}, pe care recepția nu îi are."
        End If

        ' F16 — mulțimile doar cresc, de-a lungul lanțului ordonat după DataH. Se măsoară pe
        ' lanțul REZULTAT, adică pe cel de acum plus instantaneul care tocmai se așază.
        If indInst.Count > 0 Then
            Dim inainte As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each alt As InstantaneuLegat In _stare.Instantanee.
                    Where(Function(i) i.Idrh <> inst.Idrh AndAlso PozitiaLui(i) = rec.Idrr AndAlso i.DataH <= inst.DataH).
                    OrderBy(Function(i) i.DataH)
                inainte.UnionWith(alt.Indicatori())
            Next
            If Not inainte.IsSubsetOf(indInst) Then
                Dim pierduti As String = String.Join(", ", inainte.Except(indInst).OrderBy(Function(x) x))
                Return $"Instantaneul pierde indicatorii {pierduti}, prezenți mai devreme în lanțul recepției. " &
                       "Un indicator poate cădea la zero, dar nu poate dispărea."
            End If
        End If

        Return String.Empty
    End Function

    ' `EsteInainteDeDataReceptiei` USED TO BE HERE and was DELETED on 09.09.2026 at the
    ' operator's request: the last remnant of F13 (withdrawn as a veto on 31.08.2026) lived on
    ' as a sign on the row and in the two floating labels, and it lit up on perfectly correct
    ' data — `DataR` is typed on the site and says nothing about when the receipt appeared
    ' (F29). A rule that cannot be applied is not kept half-way; the fundament still describes
    ' it, and its history is in `SLICE-0058`.

    Private Sub TreeLant_NodeDropped(sender As Object, e As TreeDropEventArgs) Handles treeLant.NodeDropped
        Try
            Dim inst = TryCast(e.Source?.Tag, InstantaneuLegat)
            Dim rec = ReceptiaTintei(e.Target)
            If inst Is Nothing OrElse rec Is Nothing Then Return

            _pozitie(inst.Idrh) = rec.Idrr
            ' Un instantaneu așezat nu mai e «fără schimbare»: cele două se exclud, fiindcă
            ' `Sters = 1` înseamnă tocmai «lăsat deliberat neatașat».
            _ignorat(inst.Idrh) = False
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.TreeLant_NodeDropped", ex)
        End Try
    End Sub

    Private Sub TreeLibere_NodeDropped(sender As Object, e As TreeDropEventArgs) Handles treeLibere.NodeDropped
        Try
            Dim inst = TryCast(e.Source?.Tag, InstantaneuLegat)
            If inst Is Nothing Then Return
            _pozitie(inst.Idrh) = 0
            _stergere(inst.Idrh) = False
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.TreeLibere_NodeDropped", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Selecția și meniul contextual
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeLant.NodeMouseUp, treeLibere.NodeMouseUp
        Try
            ' The notice box is transient feedback about the row that was just refused, saved or
            ' loaded. Picking another row means the operator has moved on, so the message goes
            ' first — before anything below can put a new one up. Without this the refusal of one
            ' snapshot stayed on screen while the operator worked on the next one, and read as if
            ' it were about that one.
            ntfMesaj.Clear()
            _receptieSelectata = ReceptiaNodului(pNode)
            UmpleGrila(pNode)
            ReconstruiesteGrafic()
            If e.Button = MouseButtons.Right Then AratMeniul(pNode)
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Which receipt the selected row belongs to: the row itself when it is a receipt, the receipt
    ''' the snapshot currently sits on otherwise, and Nothing for an unplaced snapshot or for the
    ''' root of the unplaced list.
    ''' </summary>
    Private Function ReceptiaNodului(nod As AdvancedTreeControl.TreeItem) As ReceptiePropusa
        If nod Is Nothing OrElse _stare Is Nothing Then Return Nothing
        Dim rec As ReceptiePropusa = TryCast(nod.Tag, ReceptiePropusa)
        If rec IsNot Nothing Then Return rec
        Dim inst As InstantaneuLegat = TryCast(nod.Tag, InstantaneuLegat)
        If inst Is Nothing Then Return Nothing
        Dim idrr As Integer = PozitiaLui(inst)
        If idrr = 0 Then Return Nothing
        Return Receptiile().FirstOrDefault(Function(r) r.Idrr = idrr)
    End Function

    ''' <summary>Liniile pe indicator ale rândului selectat — recepție sau instantaneu.</summary>
    Private Sub UmpleGrila(nod As AdvancedTreeControl.TreeItem)
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            Dim rec As ReceptiePropusa = TryCast(nod?.Tag, ReceptiePropusa)
            If rec IsNot Nothing Then
                For Each l As LinieReceptie In rec.Rhr
                    Dim r As KBotDataRow = grid.AddRow()
                    r(COL_INDICATOR) = l.CodIndicator
                    r(COL_SSI) = l.CodSsi
                    r(COL_CREDIT) = Bani(l.CreditBugetar)
                    r(COL_VALOARE) = Bani(l.Valoare)
                Next
                Return
            End If
            Dim inst As InstantaneuLegat = TryCast(nod?.Tag, InstantaneuLegat)
            If inst Is Nothing Then Return
            For Each l As LinieInstantaneu In inst.Linii
                Dim r As KBotDataRow = grid.AddRow()
                r(COL_INDICATOR) = l.CodIndicator
                r(COL_SSI) = l.CodSsi
                ' Instantaneul nu poartă creditul bugetar — el e al INDICATORULUI, nu al
                ' momentului. Coloana rămâne goală, nu zero: zero ar fi citit ca o cifră.
                r(COL_CREDIT) = String.Empty
                r(COL_VALOARE) = Bani(l.Valoare)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The chart
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The band above the chart moved. The chart itself decides nothing about what a button means
    ''' — it hands over the key and the form refills the series.
    ''' </summary>
    ''' <summary>
    ''' The scheme changed, so every colour this form wrote down by hand is now the wrong one.
    '''
    ''' <para>The theme reaches the controls on its own; what it cannot reach is a colour COPIED
    ''' out of the old palette into a chart series, a chart point or a tree row — those are values,
    ''' not bindings, and nothing goes back to correct them. Rebuilding the chart re-asks for all
    ''' of them and the colouring pass carries the answers back to the trees.</para>
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            ReconstruiesteGrafic()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Cele două vederi din dreapta sus
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Cheile din <c>navGrafice</c> — aceleași două șiruri pe care le scrie designerul.</summary>
    Private Const VEDEREA_GRAFIC As String = "grafic"

    Private Const VEDEREA_BENZI As String = "benzi"

    ''' <summary>
    ''' Graficul și benzile împart același loc, iar <c>navGrafice</c> alege care se vede.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Una peste alta, nu una lângă alta.</b> Împărțite pe verticală, amândouă primeau
    ''' vreo sută cincizeci de pixeli: prea puțin pentru un grafic ca să i se citească scara și
    ''' prea puțin pentru benzi ca să încapă mai mult de câteva rânduri fără derulare. Sunt oricum
    ''' două întrebări diferite puse pe rând — «cum a evoluat» și «unde stă» — deci fiecare ia tot
    ''' locul cât e întrebată.</para>
    ''' <para><b>Nimic nu se reconstruiește aici.</b> Amândouă suprafețele sunt ținute la zi de
    ''' <c>Reconstruieste</c> chiar și cât sunt ascunse, deci comutarea e o schimbare de vizibilitate
    ''' și atât. O reconstrucție la fiecare apăsare ar face butonul să pară lent fără să adauge
    ''' nimic — ce arată e deja adevărat.</para>
    ''' </remarks>
    Private Sub NavGrafice_SelectionChanged(key As String) Handles navGrafice.SelectionChanged
        Try
            AplicaVedereaDinDreaptaSus(key)
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.NavGrafice_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arată vederea cerută. Orice altceva decât cheia benzilor înseamnă graficul — inclusiv
    ''' <c>Nothing</c>, fiindcă graficul e vederea implicită și un formular fără nimic ales trebuie
    ''' totuși să arate ceva.
    ''' </summary>
    Private Sub AplicaVedereaDinDreaptaSus(key As String)
        If grafic Is Nothing OrElse benzi Is Nothing Then Return
        Dim aratBenzi As Boolean = String.Equals(key, VEDEREA_BENZI, StringComparison.Ordinal)
        benzi.Visible = aratBenzi
        grafic.Visible = Not aratBenzi
    End Sub

    Private Sub Grafic_TabSelected(tabKey As String) Handles grafic.TabSelected
        Try
            ReconstruiesteGrafic()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Grafic_TabSelected", ex)
        End Try
    End Sub

    ''' <summary>
    ''' A click on a point selects the row that stands for the SAME snapshot, and brings it into
    ''' view.
    ''' </summary>
    ''' <remarks>
    ''' <para>This is the other half of the colouring: a point and a row already share a colour, so
    ''' the operator can pair them by eye — but on a long chain the row they have just found in the
    ''' chart may be scrolled out of sight, or under a collapsed receipt. Selecting it is what turns
    ''' «I can see which one it is» into «I can now work on it».</para>
    ''' <para>The snapshot is read off the point's <c>Tag</c>, which was put there when the point was
    ''' built — no re-derivation from the label, no matching on a moment that two snapshots can
    ''' share. A point WITHOUT one is the total line: an aggregate of several receipts, so there is
    ''' no single row behind it and the click is deliberately left to do nothing.</para>
    ''' <para>Nothing is rebuilt here. Selecting a row is not a change to the picture, and rebuilding
    ''' the chart from inside the chart's own click would be a fine way to invent a loop.</para>
    ''' </remarks>
    Private Sub Grafic_PointClicked(seriesKey As String, pointIndex As Integer) Handles grafic.PointClicked
        Try
            Dim serie As KBotChartSeries = grafic.FindSeries(seriesKey)
            If serie Is Nothing OrElse pointIndex < 0 OrElse pointIndex >= serie.Points.Count Then Return

            Dim inst As InstantaneuLegat = TryCast(serie.Points(pointIndex).Tag, InstantaneuLegat)
            If inst Is Nothing Then Return

            Dim rand As RandDeArbore = Nothing
            If Not _nodInstantaneu.TryGetValue(inst.Idrh, rand) Then Return
            rand.Arbore.SelectAndReveal(rand.Nod)
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Grafic_PointClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Rebuilds the chart from the LOCAL picture, whole, for the same reason the trees are rebuilt
    ''' whole: a chart patched point by point and a tree rebuilt from scratch end up telling
    ''' different stories after a few drags, and here the one that lies is the screen.
    ''' </summary>
    Private Sub ReconstruiesteGrafic()
        grafic.BeginUpdate()
        Try
            grafic.ClearSeries()
            If _stare Is Nothing Then Return
            If String.Equals(grafic.SelectedTabKey, GRAFIC_ANGAJAMENT, StringComparison.Ordinal) Then
                ConstruiesteGraficAngajament()
            Else
                ConstruiesteGraficReceptie()
            End If
        Finally
            grafic.EndUpdate()
            ' AFTER the chart, always, and nowhere else: the chart is what decides the colours, so
            ' the trees can only copy them once they exist. Outside the Try on purpose — a chart
            ' that failed half-way still has to leave the rows in a state that matches what is
            ' actually drawn, and that state is «whatever the chart ended up with».
            SincronizeazaCulorile()
        End Try
    End Sub

    ''' <summary>
    ''' Paints each tree row in the colour of the thing that stands for it on the chart.
    '''
    ''' <para>Which rows depends on what the chart is showing. On <b>Recepția</b> the chart is one
    ''' chain, so every POINT is a snapshot and every snapshot row of that chain takes its point's
    ''' colour. On <b>Tot angajamentul</b> the chart is one line per receipt, so every RECEIPT row
    ''' takes its line's colour and the snapshots underneath go back to plain text — colouring
    ''' them too would claim a distinction the chart is not drawing.</para>
    '''
    ''' <para>Everything is reset first and then repainted, rather than patched. The two views
    ''' colour different rows, so a switch between them always leaves rows behind; clearing first
    ''' is the only version of this that does not slowly accumulate colours from a view the
    ''' operator left minutes ago.</para>
    ''' </summary>
    Private Sub SincronizeazaCulorile()
        Try
            ' Doar CULOAREA TEXTULUI se ia de la zero — fondul nu, fiindcă nu e al graficului: el
            ' spune dacă recepția e ștearsă, iar asta nu se schimbă când operatorul trece de pe o
            ' filă pe alta (vezi `ColoreazaReceptia`).
            For Each nod As AdvancedTreeControl.TreeItem In _nodReceptie.Values
                nod.NodeForeColor = Color.Empty
            Next
            For Each rand As RandDeArbore In _nodInstantaneu.Values
                Dim inst As InstantaneuLegat = TryCast(rand.Nod.Tag, InstantaneuLegat)
                rand.Nod.NodeForeColor = If(inst Is Nothing, Color.Empty, CuloareDeBaza(inst))
            Next
            For Each serie As KBotChartSeries In grafic.Series
                If String.Equals(serie.Key, SERIA_TOTAL, StringComparison.Ordinal) Then Continue For

                ' The whole-commitment view: the line names the receipt, so the ROOT row takes it.
                ' `<> 0`, nu `> 0`: o recepție pornită aici are un IDRR NEGATIV, și are în grafic
                ' o linie ca oricare alta. Cu `> 0` rândul ei rămânea singurul care nu se lega de
                ' linia lui — adică exact recepția despre care operatorul are mai multe întrebări.
                Dim idrr As Integer = IdrrDinCheie(serie.Key)
                Dim radacina As AdvancedTreeControl.TreeItem = Nothing
                If idrr <> 0 AndAlso _nodReceptie.TryGetValue(idrr, radacina) AndAlso
                   serie.LineColor <> Color.Empty Then
                    radacina.NodeForeColor = serie.LineColor
                End If

                ' The per-receipt view: each point names a snapshot, so the LEAF rows take those.
                For Each punct As KBotChartPoint In serie.Points
                    If punct.PointColor = Color.Empty Then Continue For
                    Dim inst As InstantaneuLegat = TryCast(punct.Tag, InstantaneuLegat)
                    If inst Is Nothing Then Continue For
                    Dim frunza As RandDeArbore = Nothing
                    If _nodInstantaneu.TryGetValue(inst.Idrh, frunza) Then
                        frunza.Nod.NodeForeColor = punct.PointColor
                    End If
                Next
            Next

            AplicaCulorileBenzii(benzi, _bandaReceptie)

            treeLant.Invalidate()
            treeLibere.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.SincronizeazaCulorile", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aceeași trecere de culori, pentru o bandă oarecare — a formularului sau a ferestrei mari.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Banda citește graficul, marcajul nu.</b> O bandă întreagă înseamnă o recepție, iar
    ''' aceeași recepție are o linie în grafic și un rând în arbore — deci culoarea benzii vine de
    ''' acolo, ca și până acum, și leagă cele trei suprafețe între ele.</para>
    ''' <para><b>Marcajele nu mai spun «care lanț», ci «ce s-a întâmplat»</b> — verde dacă
    ''' instantaneul a urcat valoarea recepției, roșu dacă a coborât-o. Vezi
    ''' <see cref="ColoreazaMarcajeleDupaSchimbare"/>: banda întreagă e deja a unei recepții, deci
    ''' pe ea nu mai e nimic de deosebit între marcaje, iar culoarea lor rămâne liberă pentru
    ''' singurul lucru pe care nici forma marcajului, nici arborele nu-l arată — direcția
    ''' schimbării.</para>
    ''' <para><b>Graficul nu se schimbă cu nimic.</b> Punctele lui își păstrează culorile de până
    ''' acum și legătura punct–rând-de-arbore rămâne întreagă: aici se ia doar culoarea LINIEI unei
    ''' serii, adică exact ce se lua și înainte.</para>
    ''' </remarks>
    Private Sub AplicaCulorileBenzii(tinta As KBotLaneView,
                                     bandaDupaIdrr As Dictionary(Of Integer, KBotLane))
        If tinta Is Nothing OrElse bandaDupaIdrr Is Nothing Then Return

        ColoreazaMarcajeleDupaSchimbare(tinta)

        For Each serie As KBotChartSeries In grafic.Series
            If String.Equals(serie.Key, SERIA_TOTAL, StringComparison.Ordinal) Then Continue For

            ' `<> 0` din același motiv ca la arbore: banda unei recepții pornite aici trebuie să
            ' aibă culoarea liniei ei, altfel cele trei suprafețe nu mai spun același lucru.
            Dim idrr As Integer = IdrrDinCheie(serie.Key)
            Dim banda As KBotLane = Nothing
            If idrr <> 0 AndAlso bandaDupaIdrr.TryGetValue(idrr, banda) AndAlso
               serie.LineColor <> Color.Empty Then
                banda.LaneColor = serie.LineColor
            End If
        Next
    End Sub

    ''' <summary>
    ''' Culoarea marcajelor de pe benzi: verde dacă instantaneul a URCAT valoarea recepției, roșu
    ''' dacă a coborât-o.
    ''' </summary>
    ''' <remarks>
    ''' <para>Fiecare marcaj pictează și bucata de bandă dintre el și următorul, deci o bandă se
    ''' citește de la stânga la dreapta ca un șir de urcări și coborâri — chiar întrebarea pe care
    ''' operatorul o pune când se uită la lanțul unei recepții.</para>
    ''' <para><b>Un instantaneu care nu schimbă valoarea nu primește nicio culoare</b>, adică
    ''' rămâne pe culoarea benzii, care e culoarea recepției din grafic. Nici verde, nici roșu:
    ''' n-a fost nici urcare, nici coborâre, iar o culoare acolo ar fi o afirmație pe care datele
    ''' n-o susțin.</para>
    ''' <para><b>Primul marcaj al unui lanț se măsoară față de zero</b> — înainte de el recepția
    ''' nu avea nicio valoare, deci el o urcă.</para>
    ''' <para><b>Banda de jos rămâne pe culorile ei.</b> Instantaneele neașezate nu formează un
    ''' lanț, deci diferența dintre două marcaje vecine de acolo nu înseamnă nimic — «a crescut»
    ''' ar fi o minciună despre două lucruri care n-au nicio legătură între ele.</para>
    ''' </remarks>
    Private Sub ColoreazaMarcajeleDupaSchimbare(tinta As KBotLaneView)
        If tinta Is Nothing Then Return
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta Is Nothing Then Return

        For Each banda As KBotLane In tinta.Lanes
            If String.Equals(banda.Key, CHEIE_LIBERE, StringComparison.Ordinal) Then Continue For

            Dim anterior As Double = 0
            For j As Integer = 0 To banda.Markers.Count - 1
                Dim marcaj As KBotLaneMarker = banda.Markers(j)
                Dim inst As InstantaneuLegat = TryCast(marcaj.Tag, InstantaneuLegat)
                If inst Is Nothing Then Continue For
                If inst.Total > anterior Then
                    marcaj.MarkerColor = paleta.SuccessColor
                ElseIf inst.Total < anterior Then
                    marcaj.MarkerColor = paleta.ErrorColor
                Else
                    marcaj.MarkerColor = Color.Empty
                End If
                anterior = inst.Total
            Next
        Next
    End Sub

    ''' <summary>The receipt behind a series key, or 0 if the key is not one of ours.</summary>
    Private Shared Function IdrrDinCheie(cheie As String) As Integer
        If String.IsNullOrEmpty(cheie) OrElse Not cheie.StartsWith("R", StringComparison.Ordinal) Then Return 0
        Dim idrr As Integer
        If Integer.TryParse(cheie.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, idrr) Then Return idrr
        Return 0
    End Function

    ''' <summary>One line: the chain of the selected receipt, on the time axis of its snapshots.</summary>
    Private Sub ConstruiesteGraficReceptie()
        If _receptieSelectata Is Nothing Then
            grafic.EmptyText = "Alege o recepție în stânga ca să-i vezi evoluția."
            Return
        End If

        Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(_receptieSelectata)
        If lant.Count = 0 Then
            grafic.EmptyText = "Recepția aleasă nu are niciun instantaneu."
            Return
        End If

        Dim serie As KBotChartSeries = grafic.AddSeries(CheiaSeriei(_receptieSelectata), EtichetaReceptiei(_receptieSelectata))
        serie.Emphasis = True
        serie.FillArea = True
        serie.LineMode = KBotChartLineMode.Step

        ' Every point gets its OWN colour here — this is the view where a point and a tree row are
        ' the same snapshot, so the pair has to be findable by colour alone. The segment leaving a
        ' point carries that colour too, which the chart does on its own.
        '
        ' A blocked or unchanged snapshot is NOT dimmed here any more. The first version reused the
        ' disabled grey for those, and on a chain where most links are blocked that turned the whole
        ' line grey: the chart stopped saying anything, and the row it was meant to be paired with
        ' had nothing left to pair with. «Out of play» is already written on the row twice — by the
        ' padlock icon and by the mark appended to the caption — so the colour is free to do the one
        ' job nothing else here can do: tie a point to its row.
        Dim i As Integer = 0
        For Each inst As InstantaneuLegat In lant
            AdaugaPunct(serie, inst, _receptieSelectata).PointColor = grafic.AutoColor(i)
            i += 1
        Next
    End Sub

    ''' <summary>
    ''' One line per receipt plus the thicker total line.
    '''
    ''' <para>The selected receipt is NOT the emphasised one here — the total is. Two emphasised
    ''' lines would make the total just another chain, which is exactly what it is not. The chosen
    ''' receipt is marked by its tinted area instead, so it is still findable among the rest.</para>
    ''' </summary>
    Private Sub ConstruiesteGraficAngajament()
        grafic.EmptyText = "Angajamentul nu are niciun instantaneu așezat."

        Dim lanturi As New List(Of List(Of InstantaneuLegat))()
        For Each rec As ReceptiePropusa In Receptiile().OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
            Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(rec)
            If lant.Count = 0 Then Continue For
            lanturi.Add(lant)

            Dim serie As KBotChartSeries = grafic.AddSeries(CheiaSeriei(rec), EtichetaReceptiei(rec))
            ' Named rather than left automatic: the receipt's ROW is painted in this colour too,
            ' and a colour nobody wrote down is a colour the tree cannot be told about.
            serie.LineColor = grafic.AutoColor(lanturi.Count - 1)
            serie.FillArea = _receptieSelectata IsNot Nothing AndAlso rec.Idrr = _receptieSelectata.Idrr
            serie.LineMode = KBotChartLineMode.Step
            For Each inst As InstantaneuLegat In lant
                AdaugaPunct(serie, inst, rec)
            Next
        Next

        ConstruiesteSeriaTotal(lanturi)
    End Sub

    ''' <summary>
    ''' The total of the commitment, one point per distinct moment in the whole picture.
    '''
    ''' <para>At each moment a receipt contributes the value of its own last snapshot up to then:
    ''' a value stands until the next snapshot changes it, which is what "the receipt was worth
    ''' this much in between" means. A receipt whose last snapshot is the chain's DELETION row
    ''' contributes that value AT that moment and nothing after it — the deletion row records what
    ''' the receipt was worth when it left, not what it goes on being worth.</para>
    '''
    ''' <para>With a single chain there is no total to draw: it would be the same line twice, and a
    ''' second line saying nothing new is worse than no second line.</para>
    ''' </summary>
    Private Sub ConstruiesteSeriaTotal(lanturi As List(Of List(Of InstantaneuLegat)))
        If lanturi.Count < 2 Then Return

        Dim momente As List(Of Date) =
            lanturi.SelectMany(Function(l) l).Select(Function(i) i.DataH).Distinct().OrderBy(Function(d) d).ToList()
        If momente.Count = 0 Then Return

        Dim serie As KBotChartSeries = grafic.AddSeries(SERIA_TOTAL, "Total angajament")
        serie.Emphasis = True
        ' În TREPTE, ca toate celelalte, și aici e cel mai vizibil de ce: totalul de mai jos se
        ' calculează chiar așa — `ValoareaLa` ia valoarea celui mai nou instantaneu de la sau
        ' dinaintea momentului, adică «o valoare ține până o schimbă următorul instantaneu».
        ' Desenat drept, graficul ar contrazice aritmetica pe care tocmai a făcut-o.
        serie.LineMode = KBotChartLineMode.Step
        ' Deliberately NOT a colour from AutoColor: that set belongs to the receipts, and the
        ' total is not one more receipt. The theme's plain text colour reads as «the sum» and, not
        ' being any row's colour, cannot be mistaken for a chain.
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta IsNot Nothing Then serie.LineColor = paleta.TextColor
        For Each moment As Date In momente
            Dim total As Double = 0
            For Each lant As List(Of InstantaneuLegat) In lanturi
                total += ValoareaLa(lant, moment)
            Next
            Dim punct As KBotChartPoint = serie.AddPoint(moment, total)
            punct.TooltipHeader = "Total angajament"
            punct.TooltipText = $"{moment:dd.MM.yyyy HH:mm} · {Bani(total)}"
        Next
    End Sub

    ''' <summary>What one chain was worth at a given moment. See the note on the caller.</summary>
    Private Function ValoareaLa(lant As List(Of InstantaneuLegat), moment As Date) As Double
        Dim valoare As Double = 0
        ' The chain is already ordered by DataH, so the last assignment wins and it is the value of
        ' the newest snapshot at or before the moment asked about.
        For Each inst As InstantaneuLegat In lant
            If inst.DataH > moment Then Exit For
            valoare = If(EsteStergere(inst.Idrh) AndAlso inst.DataH < moment, 0.0, inst.Total)
        Next
        Return valoare
    End Function

    ''' <summary>The chain of a receipt after the LOCAL moves, ordered along its own time axis.</summary>
    Private Function LantulReceptiei(rec As ReceptiePropusa) As List(Of InstantaneuLegat)
        If _stare Is Nothing OrElse rec Is Nothing Then Return New List(Of InstantaneuLegat)()
        Return _stare.Instantanee.Where(Function(i) PozitiaLui(i) = rec.Idrr).
                                  OrderBy(Function(i) i.DataH).ThenBy(Function(i) i.Idrh).ToList()
    End Function

    Private Shared Function CheiaSeriei(rec As ReceptiePropusa) As String
        Return "R" & rec.Idrr.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Shared Function EtichetaReceptiei(rec As ReceptiePropusa) As String
        ' IDRR-ul unei recepții pornite aici e negativ și e o unealtă a formularului, nu un
        ' număr: scris pe grafic sau pe bandă, ar trimite operatorul să caute «recepția -1».
        If EsteReceptieNoua(rec) Then Return $"Recepție nouă · {rec.DataR:dd.MM.yyyy}"
        Return $"Recepția {rec.Idrr} · {rec.DataR:dd.MM.yyyy}"
    End Function

    ''' <summary>
    ''' One snapshot as a point, with the label the operator reads on hover. Same text as the tree
    ''' tooltip, on purpose: the point and the row are the same fact seen twice.
    ''' </summary>
    ''' <returns>The point, so the caller can still colour it. See <see cref="ConstruiesteGraficReceptie"/>.</returns>
    Private Function AdaugaPunct(serie As KBotChartSeries, inst As InstantaneuLegat, rec As ReceptiePropusa) As KBotChartPoint
        Dim punct As KBotChartPoint = serie.AddPoint(inst.DataH, inst.Total)
        punct.Tag = inst
        punct.TooltipHeader = EtichetaReceptiei(rec)

        Dim sb As New Text.StringBuilder()
        sb.AppendLine($"{inst.DataH:dd.MM.yyyy HH:mm:ss} · {Bani(inst.Total)}")
        If Not String.IsNullOrWhiteSpace(inst.Descriere) Then sb.AppendLine(inst.Descriere)
        Dim ind As String = String.Join(", ", inst.Indicatori().OrderBy(Function(x) x))
        If ind <> "" Then sb.AppendLine($"Indicatori: {ind}")
        punct.TooltipText = sb.ToString().TrimEnd()

        Dim semne As New List(Of String)()
        If EsteStergere(inst.Idrh) Then semne.Add("rândul de ștergere")
        If EsteIgnorat(inst.Idrh) Then semne.Add("fără schimbare")
        If inst.Blocat Then semne.Add("legătură blocată")
        If semne.Count > 0 Then punct.TooltipFooter = String.Join(" · ", semne)
        Return punct
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Benzile de așezare
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Reconstruiește banda de așezare din tabloul LOCAL: o bandă per recepție, un marcaj per
    ''' instantaneu, un separator, apoi banda instantaneelor neașezate.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Tot, niciodată peticit</b> — aceeași regulă ca la arbori și la grafic, și din
    ''' același motiv: trei suprafețe peticite fiecare pe cont propriu ajung să spună trei
    ''' povești, iar cea care minte e ecranul.</para>
    ''' <para><b>De ce mai există, dacă arborele arată aceleași date.</b> Arborele arată UN lanț
    ''' deodată, pe verticală, cu restul derulat afară din vedere. Aici se văd toate lanțurile în
    ''' aceeași privire, pe aceeași axă a timpului, cu liniile plăților traversându-le pe toate —
    ''' deci «instantaneul ăsta a căzut de partea greșită a plății» e o observație pe care o face
    ''' ochiul, în timpul tragerii, nu una la care trebuie să te gândești după.</para>
    ''' <para>Fără text, deliberat: douăzeci de benzi a câte douăzeci de marcaje e cazul obișnuit
    ''' după spusele operatorului, iar la scara aia denumirile nu mai sunt o citire. Ele sunt la o
    ''' plimbare de mouse distanță, iar fereastra mare (<see cref="AsociereBenziForm"/>) le
    ''' scrie pe toate.</para>
    ''' </remarks>
    Private Sub ReconstruiesteBenzi()
        ConstruiesteBenzi(benzi, _bandaReceptie, _marcajInstantaneu, stapanulReperelor:=True)
    End Sub

    ''' <summary>
    ''' Construcția propriu-zisă, pe o suprafață dată și cu dicționarele ei.
    ''' </summary>
    ''' <param name="stapanulReperelor">
    ''' True doar pentru banda din formular: ea e cea care scrie și reperele plăților în grafic,
    ''' deci tot ea are dreptul să le și șteargă.
    ''' </param>
    ''' <remarks>
    ''' Parametrizată, nu scrisă de două ori, fiindcă fereastra mare arată ACELEAȘI benzi la altă
    ''' mărime. Două construcții separate ar fi două locuri în care se hotărăște ce e un marcaj de
    ''' ștergere sau când se închide un lanț — și primul lucru care s-ar abate ar fi tocmai cel pe
    ''' care operatorul îl privește când vrea să fie sigur.
    ''' </remarks>
    Private Sub ConstruiesteBenzi(tinta As KBotLaneView,
                                  bandaDupaIdrr As Dictionary(Of Integer, KBotLane),
                                  marcajDupaIdrh As Dictionary(Of Integer, KBotLaneMarker),
                                  stapanulReperelor As Boolean)
        tinta.BeginUpdate()
        Try
            tinta.ClearLanes()
            tinta.ClearGuides()
            bandaDupaIdrr.Clear()
            marcajDupaIdrh.Clear()
            If stapanulReperelor Then grafic.ClearGuides()
            If _stare Is Nothing Then Return

            Dim i As Integer = 0
            For Each rec As ReceptiePropusa In Receptiile().OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
                Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(rec)

                Dim banda As KBotLane = tinta.AddLane(CheiaSeriei(rec), EtichetaReceptiei(rec))
                banda.Tag = rec
                banda.Tooltip = TooltipBanda(rec, lant)
                ' Culoarea se scrie EXPLICIT, nu se lasă pe seama controlului: aceeași recepție
                ' are o linie în grafic și un rând în arbore, iar o culoare pe care n-a scris-o
                ' nimeni e o culoare despre care celelalte două nu pot fi anunțate.
                banda.LaneColor = tinta.AutoColor(i)
                banda.EndMark = SemnulCapatului(rec, lant)
                bandaDupaIdrr(rec.Idrr) = banda

                For j As Integer = 0 To lant.Count - 1
                    Dim inst As InstantaneuLegat = lant(j)
                    Dim marcaj As KBotLaneMarker = banda.AddMarker(inst.DataH, Bani(inst.Total))
                    marcaj.Tag = inst
                    marcaj.Tooltip = TooltipInstantaneu(inst)
                    marcaj.Style = StilulMarcajului(inst, asezat:=True)
                    marcajDupaIdrh(inst.Idrh) = marcaj
                Next
                i += 1
            Next

            ' Banda de jos: instantaneele neașezate. E ȘI ținta de desprindere — a trage un marcaj
            ' în jos înseamnă exact ce înseamnă a-l trage în `treeLibere`.
            Dim libere As List(Of InstantaneuLegat) =
                _stare.Instantanee.Where(Function(x) PozitiaLui(x) = 0).
                                   OrderBy(Function(x) x.DataH).ThenBy(Function(x) x.Idrh).ToList()

            Dim bandaLibere As KBotLane = tinta.AddLane(CHEIE_LIBERE, $"Neașezate ({libere.Count})")
            bandaLibere.SeparatorAbove = True
            bandaLibere.Tooltip = "Instantaneele care nu stau pe nicio recepție." & Environment.NewLine &
                                  "Trage un marcaj aici ca să-l desprinzi de recepția lui."
            For j As Integer = 0 To libere.Count - 1
                Dim inst As InstantaneuLegat = libere(j)
                Dim marcaj As KBotLaneMarker = bandaLibere.AddMarker(inst.DataH, Bani(inst.Total))
                marcaj.Tag = inst
                marcaj.Tooltip = TooltipInstantaneu(inst)
                marcaj.Style = StilulMarcajului(inst, asezat:=False)
                marcaj.MarkerColor = tinta.AutoColor(j)
                marcajDupaIdrh(inst.Idrh) = marcaj
            Next

            ' Culorile marcajelor de pe benzile așezate — urcare verde, coborâre roșie. Aici, la
            ' construire, ca banda să spună asta și înainte ca trecerea de culori să apuce să
            ' treacă pe la ea.
            ColoreazaMarcajeleDupaSchimbare(tinta)

            ConstruiesteReperelePlatilor(tinta, stapanulReperelor)
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.ConstruiesteBenzi", ex)
        Finally
            tinta.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Ce spune forma unui marcaj. Ordinea contează: se răspunde întâi la întrebarea pe care
    ''' operatorul o pune prima.
    ''' </summary>
    ''' <remarks>
    ''' <para>Pe banda de jos, «neașezat» nu e o informație — TOATE marcajele de acolo sunt
    ''' neașezate. Ce se poate spune acolo e F17: «asta nu consemnează nicio schimbare», adică
    ''' motivul pentru care instantaneul e lăsat deoparte deliberat, nu unul încă neatins.</para>
    ''' <para>Pe benzile de sus, prima întrebare la un marcaj care nu se mișcă e «de ce nu se
    ''' mișcă», deci lacătul bate rândul de ștergere. Nu se pierde nimic: eticheta plutitoare le
    ''' spune pe amândouă, iar rândul din arbore poartă marcajul [ștergere] oricum.</para>
    ''' </remarks>
    Private Function StilulMarcajului(inst As InstantaneuLegat, asezat As Boolean) As KBotLaneMarkerStyle
        If Not asezat Then
            Return If(EsteIgnorat(inst.Idrh), KBotLaneMarkerStyle.NoChange, KBotLaneMarkerStyle.Loose)
        End If
        If inst.Blocat Then Return KBotLaneMarkerStyle.Locked
        If EsteStergere(inst.Idrh) Then Return KBotLaneMarkerStyle.Deletion
        Return KBotLaneMarkerStyle.Normal
    End Function

    ''' <summary>
    ''' Eticheta benzii unei recepții: ce spune arborele, plus câte marcaje sunt pe ea.
    ''' </summary>
    ''' <remarks>
    ''' Numărul e aici fiindcă banda nu-l poate arăta: mai multe salvări în același minut cad pe
    ''' aceeași coloană de pixeli și se desenează una peste alta. Toate se desenează, niciuna nu
    ''' se ascunde și niciuna nu se contopește — dar «sunt trei aici, nu unul» trebuie scris
    ''' undeva, și ăsta e locul.
    ''' </remarks>
    Private Function TooltipBanda(rec As ReceptiePropusa, lant As List(Of InstantaneuLegat)) As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine(TooltipReceptie(rec, lant))
        sb.AppendLine()
        sb.AppendLine($"{lant.Count} instantanee pe bandă.")
        Return sb.ToString().TrimEnd()
    End Function

    ''' <summary>
    ''' Plățile angajamentului, ca linii verticale, pe AMÂNDOUĂ suprafețele.
    ''' </summary>
    ''' <remarks>
    ''' <para>Astea sunt rostul întregii felii. §1.3 din fundament: fiecare ordonanțare citește
    ''' totalul recepției AȘA CUM STĂTEA la data plății. Deci partea pe care cade un instantaneu
    ''' față de o plată nu e un amănunt de aspect — e diferența dintre o cifră corectă și una
    ''' greșită, tăcut și pentru totdeauna (F12).</para>
    ''' <para><b>Câte un reper pentru fiecare suprafață, construite în ACEEAȘI buclă din aceeași
    ''' plată.</b> Prima variantă punea un singur obiect în amândouă colecțiile — «nu au cum să nu
    ''' fie de acord asupra unei date» — dar un reper are UN singur proprietar, cel care îl
    ''' repictează când i se schimbă culoarea, iar al doilea adăugat îl fura pe primul. Un obiect
    ''' cu doi stăpâni și un singur câmp de stăpân e o capcană pusă pentru mai târziu. Garanția
    ''' rămâne la fel de tare fără el: amândouă reperele primesc <c>plata.DataPlata</c>, aceeași
    ''' valoare, în același pas al aceleiași bucle.</para>
    ''' <para><b>Eticheta face socoteala §1.3, nu o descrie.</b> Vezi
    ''' <see cref="CorpulReperului"/>: acolo se scrie de ce.</para>
    ''' </remarks>
    Private Sub ConstruiesteReperelePlatilor(tinta As KBotLaneView, siInGrafic As Boolean)
        If _stare Is Nothing Then Return

        ' Lanțurile o SINGURĂ dată, nu unul pe plată: sunt aceleași pentru toate reperele, iar
        ' douăzeci de recepții × douăzeci de plăți înseamnă patru sute de treceri prin toate
        ' instantaneele pentru un răspuns pe care îl aveam deja de la prima.
        Dim lanturi As List(Of List(Of InstantaneuLegat)) =
            Receptiile().Select(Function(r) LantulReceptiei(r)).
                            Where(Function(l) l.Count > 0).ToList()

        ' Plățile deja făcute până la reperul curent. Se adună PE PARCURS, în ordinea datelor,
        ' fiindcă asta e chiar definiția lui `PlatiAnt` din §1.3 — plățile de dinaintea ăsteia.
        Dim platiAnterioare As Double = 0

        For Each plata As PlataAsociere In _stare.Plati.OrderBy(Function(p) p.DataPlata)
            Dim titlu As String = $"Plată {plata.DataPlata:dd.MM.yyyy} · {Bani(plata.Suma)}"
            Dim corp As String = CorpulReperului(plata, lanturi, platiAnterioare)
            platiAnterioare += plata.Suma

            Dim peBenzi As KBotChartGuide = tinta.AddGuide(plata.DataPlata, titlu)
            peBenzi.Tooltip = corp
            peBenzi.Tag = plata

            If siInGrafic Then
                Dim peGrafic As KBotChartGuide = grafic.AddGuide(plata.DataPlata, titlu)
                peGrafic.Tooltip = corp
                peGrafic.Tag = plata
            End If
        Next
    End Sub

    ''' <summary>
    ''' Eticheta unei plăți: socoteala din §1.3, făcută pe tabloul LOCAL, la data plății.
    ''' </summary>
    ''' <param name="platiAnterioare">Plățile de dinaintea ăsteia — <c>PlatiAnt</c> din §1.3.</param>
    ''' <remarks>
    ''' <para><b>De ce nu mai scrie doar «totalul a intrat în ordonanțare».</b> Propoziția aia
    ''' spunea CE se întâmplă, iar operatorul știa asta oricum; ce n-avea era CIFRA. Ordonanțarea
    ''' citește totalul recepțiilor așa cum stătea la data plății, adică suma ultimului instantaneu
    ''' al fiecărei recepții de la sau dinaintea acelei date — exact ce calculează
    ''' <see cref="ValoareaLa"/>, exact ce desenează linia «Total angajament». Scrisă lângă plată,
    ''' cifra asta răspunde direct la întrebarea pentru care există toată suprafața: instantaneele
    ''' astea sunt de partea bună a plății?</para>
    ''' <para><b>De ce apar și plățile anterioare, deși nu s-au cerut.</b> Fără ele diferența n-ar
    ''' avea niciun prag: prima plată ar ieși zero și toate celelalte n-ar ieși, la un angajament
    ''' plătit perfect. Cu ele, diferența e <c>Ramas</c> din tabelul §1.3 — ce mai are angajamentul
    ''' de plătit la momentul ăla — deci un număr care se poate citi ca număr.</para>
    ''' <para><b>Ce e greșit și ce e doar rest.</b> O diferență pozitivă e obișnuită: recepții
    ''' încă neplătite. O diferență NEGATIVĂ nu poate fi rest — s-ar fi plătit mai mult decât
    ''' arătau recepțiile atunci — deci ori un instantaneu care ar fi trebuit să cadă înaintea
    ''' plății stă acum după ea, ori stă pe recepția greșită. Doar aia se colorează, și de aia
    ''' numai ea: o culoare pusă pe un rest normal ar învăța ochiul să nu se mai uite la culoare.</para>
    ''' <para><b>Instantaneele neașezate se numără</b> fiindcă ele nu intră în niciun lanț, deci
    ''' nu intră nici în total. Cu ele pe jos, cifra de deasupra e provizorie, iar asta trebuie
    ''' spus acolo unde se citește cifra, nu ghicit de pe banda de jos.</para>
    ''' </remarks>
    Private Function CorpulReperului(plata As PlataAsociere,
                                     lanturi As List(Of List(Of InstantaneuLegat)),
                                     platiAnterioare As Double) As String
        Dim totalReceptii As Double = 0
        For Each lant As List(Of InstantaneuLegat) In lanturi
            totalReceptii += ValoareaLa(lant, plata.DataPlata)
        Next
        Dim ramas As Double = totalReceptii - platiAnterioare - plata.Suma

        Dim sb As New Text.StringBuilder()
        If Not String.IsNullOrWhiteSpace(plata.NrOp) Then sb.AppendLine($"OP {plata.NrOp}")
        sb.AppendLine($"Total recepții la data plății: <b>{Bani(totalReceptii)}</b>")
        If platiAnterioare <> 0 Then sb.AppendLine($"Plăți anterioare: {Bani(platiAnterioare)}")
        sb.AppendLine($"Plata asta: {Bani(plata.Suma)}")
        sb.AppendLine(LiniaDiferentei(ramas))

        If _stare IsNot Nothing Then
            Dim neasezate As Integer =
                _stare.Instantanee.Where(Function(i) PozitiaLui(i) = 0 AndAlso i.DataH <= plata.DataPlata).Count()
            If neasezate > 0 Then
                sb.AppendLine()
                sb.AppendLine($"{neasezate} instantanee neașezate până la data asta — totalul de mai sus nu le cuprinde.")
            End If
        End If
        Return sb.ToString().TrimEnd()
    End Function

    ''' <summary>
    ''' Linia diferenței, colorată doar când e imposibilă (vezi nota de la <see cref="CorpulReperului"/>).
    ''' </summary>
    ''' <remarks>
    ''' Culoarea se ia din paletă și se scrie în text ca marcaj — o valoare copiată, nu o legătură,
    ''' deci la schimbarea temei nu se corectează singură. Nu e o scăpare: eticheta se reface din
    ''' <c>OnThemeChanged</c> odată cu benzile, ca toate celelalte culori copiate de formularul ăsta.
    ''' </remarks>
    Private Shared Function LiniaDiferentei(ramas As Double) As String
        Dim text As String = $"Diferență (recepții - plăți): {Bani(ramas)}"
        ' Bani de la un Double: un rest de o miime de leu e zgomot de virgulă mobilă, nu o plată
        ' în plus, iar pragul e sub cel mai mic ban care se poate scrie.
        If ramas >= -0.005 Then Return text
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta Is Nothing Then Return text
        Dim c As Color = paleta.ErrorColor
        Return $"<color=#{c.R:X2}{c.G:X2}{c.B:X2}>{text}</color>"
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Tragerea pe benzi
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Recepția din spatele unei benzi. <c>Nothing</c> pentru banda «neașezate», care e o țintă
    ''' adevărată, dar nu o recepție.
    ''' </summary>
    Private Function ReceptiaBenzii(banda As KBotLane) As ReceptiePropusa
        Return TryCast(banda?.Tag, ReceptiePropusa)
    End Function

    Private Sub Benzi_MarkerDragStarting(sender As Object, e As LaneDragStartEventArgs) Handles benzi.MarkerDragStarting
        Try
            ntfMesaj.Clear()
            Dim inst = TryCast(e.Marker?.Tag, InstantaneuLegat)
            If inst Is Nothing Then e.Cancel = True : Return
            ' Legătură înghețată de o ordonanțare sau de o plată: vizibilă, dar nu de mutat. Se
            ' oprește din pornire, ca operatorul să simtă refuzul înainte de gest, nu după.
            If inst.Blocat Then
                e.Cancel = True
                ntfMesaj.Show("Această legătură nu se mai poate modifica. " &
                              String.Join(" ", inst.Motive), NoticeKind.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Benzi_MarkerDragStarting", ex)
            e.Cancel = True
        End Try
    End Sub

    ''' <summary>
    ''' Aceleași vetouri ca la arbore, pe aceeași funcție — <see cref="MotivulRefuzului"/>.
    ''' </summary>
    ''' <remarks>
    ''' <b>F13 nu se consultă</b> (retras 31.08.2026, iar semnul care îi rămăsese a fost șters
    ''' pe 09.09.2026). Dacă momentul cade înaintea lui <c>DataR</c> al recepției-țintă,
    ''' aruncarea se face oricum și nimic nu comentează asta.
    ''' </remarks>
    Private Sub Benzi_MarkerDragOver(sender As Object, e As LaneDragOverEventArgs) Handles benzi.MarkerDragOver
        Try
            Dim inst = TryCast(e.Marker?.Tag, InstantaneuLegat)
            If inst Is Nothing Then e.Allow = False : Return

            Dim rec As ReceptiePropusa = ReceptiaBenzii(e.Target)
            If rec Is Nothing Then
                ' Banda de jos = desprinderea.
                If PozitiaLui(inst) = 0 Then
                    e.Allow = False
                    e.Reason = "Instantaneul este deja neașezat."
                    Return
                End If
                e.Allow = True
                Return
            End If

            If PozitiaLui(inst) = rec.Idrr Then
                e.Allow = False
                e.Reason = "Instantaneul este deja pe această recepție."
                Return
            End If

            Dim motiv As String = MotivulRefuzului(inst, rec)
            e.Allow = motiv = String.Empty
            e.Reason = motiv
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Benzi_MarkerDragOver", ex)
            e.Allow = False
        End Try
    End Sub

    ''' <summary>
    ''' Aruncarea scrie în ACELAȘI <c>_pozitie</c> în care scrie și tragerea din arbore.
    ''' </summary>
    ''' <remarks>
    ''' Cele două suprafețe sunt două vederi ale unui singur tablou local, deci există exact un
    ''' loc în care se consemnează o așezare. Dacă ar exista două, s-ar putea contrazice.
    ''' </remarks>
    Private Sub Benzi_MarkerDropped(sender As Object, e As LaneDropEventArgs) Handles benzi.MarkerDropped
        Try
            ntfMesaj.Clear()
            Dim inst = TryCast(e.Marker?.Tag, InstantaneuLegat)
            If inst Is Nothing Then Return

            Dim rec As ReceptiePropusa = ReceptiaBenzii(e.Target)
            If rec Is Nothing Then
                _pozitie(inst.Idrh) = 0
                _stergere(inst.Idrh) = False
            Else
                _pozitie(inst.Idrh) = rec.Idrr
                ' Un instantaneu așezat nu mai e «fără schimbare»: cele două se exclud, fiindcă
                ' `Sters = 1` înseamnă tocmai «lăsat deliberat neatașat».
                _ignorat(inst.Idrh) = False
            End If
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Benzi_MarkerDropped", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deschide fereastra mare. Aceleași benzi, la altă mărime — nu o a doua funcție.
    ''' </summary>
    ''' <remarks>
    ''' Modal, și se reconstruiește la întoarcere: tabloul local e comun, deci fereastra mare a
    ''' putut muta ceva, iar strâmta trebuie să arate ce s-a hotărât. Nu e nimic de împăcat între
    ''' ele — amândouă citesc aceleași dicționare.
    ''' </remarks>
    Private Sub Benzi_EnlargeRequested() Handles benzi.EnlargeRequested
        Try
            If _stare Is Nothing Then Return
            Using f As New AsociereBenziForm(Me)
                f.ShowDialog(Me)
            End Using
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.Benzi_EnlargeRequested", ex)
            ntfMesaj.Show("Nu am putut deschide benzile mari. Vedeți jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Ce împrumută fereastra mare
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Umple banda unei alte ferestre din ACELAȘI tablou local, prin aceeași metodă.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="AsociereBenziForm"/> nu are date proprii și nu are voie să aibă: e aceeași
    ''' suprafață la altă mărime. Împrumută construcția și cei trei tratatori de tragere, deci nu
    ''' există o a doua regulă de așezare care s-ar putea abate de la prima.
    ''' </remarks>
    Friend Sub UmpleBenzile(tinta As KBotLaneView,
                            banda As Dictionary(Of Integer, KBotLane),
                            marcaj As Dictionary(Of Integer, KBotLaneMarker))
        If tinta Is Nothing Then Throw New ArgumentNullException(NameOf(tinta))
        If banda Is Nothing Then Throw New ArgumentNullException(NameOf(banda))
        If marcaj Is Nothing Then Throw New ArgumentNullException(NameOf(marcaj))
        ' `stapanulReperelor:=False`: reperele graficului aparțin benzii din formular. Fereastra
        ' mare își pune reperele ei pe suprafața ei și nu se atinge de grafic — altfel golirea
        ' lor de aici ar șterge, la deschiderea ferestrei, exact liniile pe care le desenează
        ' graficul de dedesubt.
        ConstruiesteBenzi(tinta, banda, marcaj, stapanulReperelor:=False)
        AplicaCulorileBenzii(tinta, banda)
    End Sub

    ''' <summary>Tratatorii de tragere ai formularului, pentru banda ferestrei mari.</summary>
    Friend Sub LeagaBanda(tinta As KBotLaneView)
        If tinta Is Nothing Then Throw New ArgumentNullException(NameOf(tinta))
        AddHandler tinta.MarkerDragStarting, AddressOf Benzi_MarkerDragStarting
        AddHandler tinta.MarkerDragOver, AddressOf Benzi_MarkerDragOver
        AddHandler tinta.MarkerDropped, AddressOf Benzi_MarkerDropped
    End Sub

    Friend Sub DezleagaBanda(tinta As KBotLaneView)
        If tinta Is Nothing Then Return
        RemoveHandler tinta.MarkerDragStarting, AddressOf Benzi_MarkerDragStarting
        RemoveHandler tinta.MarkerDragOver, AddressOf Benzi_MarkerDragOver
        RemoveHandler tinta.MarkerDropped, AddressOf Benzi_MarkerDropped
    End Sub

    ''' <summary>
    ''' Meniul unui instantaneu: cele două marcaje pe care tragerea nu le poate exprima.
    '''
    ''' <para><b>«Nu consemnează nicio schimbare»</b> e F17, și e o ACȚIUNE A OPERATORULUI, nu o
    ''' clasificare automată: două blocuri cu aceleași cifre pot fi o salvare goală SAU o
    ''' modificare reală pe altă recepție cu aceeași sumă, iar mașina nu le poate deosebi.</para>
    '''
    ''' <para><b>«Este rândul de ștergere»</b> e F21: ultimul instantaneu al lanțului, cel care
    ''' spune când a plecat recepția și cât valora atunci.</para>
    ''' </summary>
    Private Sub AratMeniul(nod As AdvancedTreeControl.TreeItem)
        ' Rândul-recepție al unei recepții pornite aici are meniul lui, cu o singură intrare:
        ' renunțarea. Fără ea, o recepție pornită din greșeală s-ar desface doar desprinzându-i
        ' instantaneele unul câte unul — adică un gest pentru fiecare, ca s-o anulezi pe cea
        ' făcută cu unul singur.
        Dim recNoua As ReceptiePropusa = TryCast(nod?.Tag, ReceptiePropusa)
        If EsteReceptieNoua(recNoua) Then
            AratMeniulReceptieiNoi(recNoua)
            Return
        End If

        Dim inst As InstantaneuLegat = TryCast(nod?.Tag, InstantaneuLegat)
        If inst Is Nothing Then Return
        If inst.Blocat Then
            ntfMesaj.Show("Această legătură nu se mai poate modifica. " &
                          String.Join(" ", inst.Motive), NoticeKind.Warning)
            Return
        End If

        ' `<> 0`, nu `> 0`: un instantaneu așezat pe o recepție pornită AICI poartă un IDRR
        ' negativ, și e tot așezat. Cu `> 0` cădea pe ramura coșului — i se ofereau «nu
        ' consemnează nicio schimbare» și «începe o recepție nouă» pe ceva care stă deja pe una,
        ' și nu i se oferea desprinderea.
        Dim asezat As Boolean = PozitiaLui(inst) <> 0
        Dim intrari As New List(Of CustomPopupItem)()
        If asezat Then
            intrari.Add(New CustomPopupItem(MENIU_DESPRINDE, "&Desprinde de recepție", Il_Receptii.Images.Item("link_break")))
            ' Ștergerea se oferă doar pe o recepție venită de la server. Pe una pornită aici nu e
            ' nimic de hotărât — ultimul instantaneu al lanțului E ștergerea ei (vezi
            ' `EsteStergere`) —, iar o intrare de meniu care nu schimbă nimic e mai rea decât
            ' niciuna: operatorul o apasă și ecranul îi răspunde că n-a apăsat.
            If Not EstePeReceptieNoua(inst.Idrh) Then
                If EsteStergere(inst.Idrh) Then
                    intrari.Add(New CustomPopupItem(MENIU_NU_STERGERE, "Nu mai e rândul de ș&tergere"))
                Else
                    intrari.Add(New CustomPopupItem(MENIU_STERGERE, "Este rândul de ș&tergere"))
                End If
            End If
        Else
            If EsteIgnorat(inst.Idrh) Then
                intrari.Add(New CustomPopupItem(MENIU_NU_IGNORA, "&Consemnează o schimbare"))
            Else
                intrari.Add(New CustomPopupItem(MENIU_IGNORA, "&Nu consemnează nicio schimbare"))
            End If
            ' F26 — a treia cale, pe lângă «pune-l pe o recepție de aici» și «nu consemnează
            ' nimic»: instantaneul PORNEȘTE o recepție care nu mai există nicăieri. Se oferă
            ' doar în coșul celor neașezate, fiindcă doar acolo întrebarea are sens: un
            ' instantaneu deja legat are recepția lui, iar aici tocmai lipsa uneia e problema.
            intrari.Add(CustomPopupItem.Separator())
            intrari.Add(New CustomPopupItem(MENIU_RECEPTIE_NOUA, "Începe o recepție no&uă",
                                            Il_Receptii.Images.Item("Receptii")))
        End If

        Dim meniu As New CustomPopup(intrari)
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs) AplicaComandaDeMeniu(inst, ev.Item.Key)
        meniu.ShowAtCursor(If(nod Is Nothing, CType(treeLant, Control), CType(treeLant, Control)))
    End Sub

    ''' <summary>
    ''' Meniul rândului unei recepții pornite aici: se poate doar renunța la ea.
    ''' </summary>
    ''' <remarks>
    ''' Renunțarea nu «șterge» nimic — pune înapoi în coș toate instantaneele ei, iar recepția
    ''' dispare fiindcă nu mai are niciunul (<see cref="ActualizeazaReceptiileNoi"/>). E singurul
    ''' fel în care poate dispărea, deci nu există două drumuri care s-ar putea abate unul de la
    ''' celălalt.
    ''' </remarks>
    Private Sub AratMeniulReceptieiNoi(rec As ReceptiePropusa)
        Dim intrari As New List(Of CustomPopupItem)() From {
            New CustomPopupItem(MENIU_RENUNTA_RECEPTIE, "&Renunță la recepția nouă",
                                Il_Receptii.Images.Item("link_break"))}
        Dim meniu As New CustomPopup(intrari)
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs) RenuntaLaReceptiaNoua(rec)
        meniu.ShowAtCursor(treeLant)
    End Sub

    ''' <summary>Instantaneele recepției pornite aici se întorc în coș; recepția piere cu ele.</summary>
    Private Sub RenuntaLaReceptiaNoua(rec As ReceptiePropusa)
        Try
            If rec Is Nothing OrElse _stare Is Nothing Then Return
            For Each inst As InstantaneuLegat In LantulReceptiei(rec)
                _pozitie(inst.Idrh) = 0
                _stergere(inst.Idrh) = False
            Next
            _receptieSelectata = Nothing
            Reconstruieste()
        Catch ex As Exception
            ' Graniță de UI (tratatorul meniului): se loghează și se arată, nu se re-aruncă.
            GlobalErrorLog.Write("AsociereForm.RenuntaLaReceptiaNoua", ex)
            ntfMesaj.Show("Nu am putut renunța la recepția nouă. Vedeți jurnalul de erori.",
                          NoticeKind.Error)
        End Try
    End Sub

    Private Sub AplicaComandaDeMeniu(inst As InstantaneuLegat, cheie As String)
        Try
            Select Case cheie
                Case MENIU_DESPRINDE
                    _pozitie(inst.Idrh) = 0
                    _stergere(inst.Idrh) = False
                Case MENIU_IGNORA
                    _ignorat(inst.Idrh) = True
                    _pozitie(inst.Idrh) = 0
                Case MENIU_NU_IGNORA
                    _ignorat(inst.Idrh) = False
                Case MENIU_STERGERE
                    _stergere(inst.Idrh) = True
                Case MENIU_NU_STERGERE
                    _stergere(inst.Idrh) = False
                Case MENIU_RECEPTIE_NOUA
                    ' Se scrie DOAR în `_pozitie`, cu un IDRR negativ. Recepția însăși se naște
                    ' din asta la reconstruire — nu se adaugă nimic nicăieri pe lângă, ca să nu
                    ' existe o a doua evidență care s-ar putea contrazice cu prima.
                    _pozitie(inst.Idrh) = UrmatorulIdrrNou()
                    _ignorat(inst.Idrh) = False
                    _stergere(inst.Idrh) = False
                Case Else
                    ' Fără implicit tăcut: o cheie necunoscută e o greșeală de programare, nu
                    ' o comandă pe care s-o ghicim.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {cheie}", NameOf(cheie))
            End Select
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.AplicaComandaDeMeniu", ex)
            ntfMesaj.Show("Comanda nu a putut fi aplicată. Vedeți jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Salvarea
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Ce s-a schimbat față de tabloul citit — și NIMIC în plus.
    '''
    ''' <para>O legătură neatinsă nu se retrimite. Aici tăcerea înseamnă «las-o cum e», ceea ce
    ''' e un răspuns adevărat; în ingestie ar fi fost o alegere ascunsă, de-aia acolo acoperirea
    ''' e obligatorie și lipsa unei decizii e 400.</para>
    ''' </summary>
    Private Function Comenzi() As List(Of ComandaAsociere)
        Dim out As New List(Of ComandaAsociere)()
        If _stare Is Nothing Then Return out

        Dim capete As Dictionary(Of Integer, CapeteLant) = CapeteleReceptiilorNoi()

        For Each inst As InstantaneuLegat In _stare.Instantanee
            Dim idrrNou As Integer = PozitiaLui(inst)
            Dim ignoratNou As Boolean = EsteIgnorat(inst.Idrh)
            Dim stergereNou As Boolean = EsteStergere(inst.Idrh)

            Dim sAschimbat As Boolean = (idrrNou <> inst.Idrr) OrElse
                                        (ignoratNou <> inst.Ignorat) OrElse
                                        (stergereNou <> inst.Stergere)
            If Not sAschimbat Then Continue For

            If idrrNou = 0 Then
                ' Neașezat: fie marcat «fără schimbare», fie pur și simplu desprins. Cele două
                ' scriu altceva în bază (`Sters = 1` față de `Sters = 0`), deci nu se confundă.
                out.Add(New ComandaAsociere() With {
                    .Idrh = inst.Idrh,
                    .Actiune = If(ignoratNou, ActiuneAsociere.Ignorat, ActiuneAsociere.Desprins)})
            ElseIf idrrNou < 0 Then
                ' Recepție pornită aici: nu are IDRR de trimis, ci o ETICHETĂ. Serverul o
                ' materializează la salvare din chiar lanțul de mai jos și abia atunci îi dă
                ' cheia. `Idrr` rămâne nescris — serverul cere exact una dintre cele două.
                ' Ștergerea NU se citește din `_stergere`: capetele lanțului o dau, ca și pe
                ' ecran (vezi `EsteStergere`).
                Dim capeteleLui As CapeteLant = Nothing
                capete.TryGetValue(idrrNou, capeteleLui)
                out.Add(New ComandaAsociere() With {
                    .Idrh = inst.Idrh,
                    .Actiune = ActiuneaPeReceptieNoua(inst.Idrh, capeteleLui),
                    .ReceptieNoua = EtichetaTrimisa(idrrNou)})
            Else
                out.Add(New ComandaAsociere() With {
                    .Idrh = inst.Idrh,
                    .Actiune = If(stergereNou, ActiuneAsociere.Stergere, ActiuneAsociere.Asociat),
                    .Idrr = idrrNou})
            End If
        Next
        Return out
    End Function

    ''' <summary>
    ''' Câte instantanee nu au încă o hotărâre: neașezate ȘI nemarcate «fără schimbare».
    ''' Zero e condiția ca butonul de salvare să se aprindă în modul propunere.
    ''' </summary>
    Private Function NehotarateleCount() As Integer
        If _stare Is Nothing Then Return 0
        ' `.Where(...).Count()`, nu `.Count(...)`: proprietatea `Count` a listei umbrește
        ' extensia LINQ cu predicat, iar compilatorul o citește ca indexare. Aceeași formă ca
        ' în restul formularului.
        ' `Not i.Blocat`: rândurile de CONTEXT (felia 0056) nu poartă hotărâri, deci nu au
        ' cum să lipsească din ele. Unul neașezat printre ele — cel al cărui rând de istoric
        ' nu e în descărcarea asta — ar stinge butonul pentru totdeauna.
        Return _stare.Instantanee.
            Where(Function(i) Not i.Blocat AndAlso PozitiaLui(i) = 0 AndAlso
                              Not EsteIgnorat(i.Idrh)).Count()
    End Function

    ''' <summary>
    ''' Hotărârea operatorului pentru FIECARE instantaneu — spre deosebire de
    ''' <see cref="Comenzi"/>, care trimite doar ce s-a schimbat.
    '''
    ''' <para>Acoperirea e totală fiindcă serverul o cere: un instantaneu fără decizie e 400,
    ''' nu o valoare implicită, tocmai ca tăcerea să nu poată însemna «ignoră-l». Un rând
    ''' rămas pe sugestia automată e tot o hotărâre — operatorul a văzut-o și a apăsat
    ''' «Salvează» peste ea (F18).</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>Shared și cu tabloul dat pe parametri, ca să poată fi verificată fără să se
    ''' deschidă fereastra: <c>ShowDialog</c> e modal și ar bloca o rulare de teste.</para>
    ''' <para><b>Recepțiile intră aici ca să se poată numi corect</b> (felia 0056). O recepție
    ''' pe care o naște CHIAR descărcarea asta nu poate fi numită prin <c>IDRR</c>: numărul ei
    ''' s-a dat înăuntrul tranzacției pe care faza întâi a derulat-o înapoi, iar contorul
    ''' AUTO_INCREMENT nu se derulează odată cu ea — la salvare primește altul. Numele care
    ''' ține e <see cref="ReceptiePropusa.RandReceptie"/>, indicele rândului ei în
    ''' <c>ListaReceptii</c>. Parametrul e OBLIGATORIU tocmai fiindcă aici s-a greșit o dată:
    ''' un implicit tăcut ar reînvia «Recepția N nu există pe acest angajament».</para>
    ''' </remarks>
    Friend Shared Function DeciziiDin(instantanee As IEnumerable(Of InstantaneuLegat),
                                      receptii As IEnumerable(Of ReceptiePropusa),
                                      pozitie As IReadOnlyDictionary(Of Integer, Integer),
                                      ignorat As IReadOnlyDictionary(Of Integer, Boolean),
                                      stergere As IReadOnlyDictionary(Of Integer, Boolean)) As List(Of DecizieAsociere)
        If instantanee Is Nothing Then Throw New ArgumentNullException(NameOf(instantanee))
        If receptii Is Nothing Then Throw New ArgumentNullException(NameOf(receptii))

        ' IDRR-ul local ▸ numele care supraviețuiește. Doar recepțiile născute de rularea
        ' asta au unul; restul se numesc prin IDRR, care e real și nu se mișcă.
        Dim ancora As New Dictionary(Of Integer, Integer)()
        For Each r As ReceptiePropusa In receptii
            If r.RandReceptie.HasValue Then ancora(r.Idrr) = r.RandReceptie.Value
        Next

        ' Capetele fiecărei recepții pornite de operator (IDRR negativ). Se calculează o dată,
        ' din același tablou, ca acoperirea de mai jos să nu poată nimeri două «reconstituiri»
        ' — sau două «ștergeri» — pentru aceeași etichetă.
        Dim capete As Dictionary(Of Integer, CapeteLant) = CapeteleDin(instantanee, pozitie)

        Dim out As New List(Of DecizieAsociere)()

        For Each inst As InstantaneuLegat In instantanee
            ' Rândurile de CONTEXT nu poartă hotărâri: acoperirea cerută de server e exact
            ' mulțimea de așezat, iar o decizie pentru un rând din afara ei e respinsă cu 400
            ' — și pe drept, ar rescrie tăcut legături vechi la fiecare descărcare.
            If inst.Blocat Then Continue For

            Dim idrr As Integer = inst.Idrr
            If pozitie IsNot Nothing AndAlso pozitie.ContainsKey(inst.Idrh) Then idrr = pozitie(inst.Idrh)
            Dim eIgnorat As Boolean = ignorat IsNot Nothing AndAlso
                                      ignorat.ContainsKey(inst.Idrh) AndAlso ignorat(inst.Idrh)
            Dim eStergere As Boolean = stergere IsNot Nothing AndAlso
                                       stergere.ContainsKey(inst.Idrh) AndAlso stergere(inst.Idrh)

            Dim d As New DecizieAsociere() With {
                .RandIstoric = inst.Idrh,      ' în modul propunere ancora ESTE indicele rândului
                .DataH = inst.DataH
            }
            If eIgnorat Then
                d.Actiune = ActiuneAsociere.Ignorat
            ElseIf idrr < 0 Then
                ' Recepție pornită de operator din coșul celor neașezate (F26): se numește
                ' printr-o etichetă, fiindcă nu există nici măcar local — nici IDRR, nici rând
                ' în `ListaReceptii`. Serverul o creează din lanțul ăsta. `eStergere` nu se
                ' citește aici: pe o recepție care există TOCMAI fiindcă a fost ștearsă,
                ' ștergerea e ultimul rând al lanțului, nu un steag pus de cineva.
                Dim capeteleLui As CapeteLant = Nothing
                capete.TryGetValue(idrr, capeteleLui)
                d.Actiune = ActiuneaPeReceptieNoua(inst.Idrh, capeteleLui)
                d.ReceptieNoua = EtichetaTrimisa(idrr)
            ElseIf idrr = 0 Then
                ' Nu se inventează o hotărâre pentru un rând pe care operatorul nu l-a atins.
                ' Butonul e stins tocmai ca drumul ăsta să nu se poată parcurge.
                Throw New InvalidOperationException(
                    $"Instantaneul de la rândul {inst.Idrh} nu are nicio hotărâre.")
            Else
                d.Actiune = If(eStergere, ActiuneAsociere.Stergere, ActiuneAsociere.Asociat)
                ' UNA dintre cele două, niciodată amândouă: serverul cere exact o țintă.
                If ancora.ContainsKey(idrr) Then
                    d.RandReceptie = ancora(idrr)
                Else
                    d.Idrr = idrr
                End If
            End If
            out.Add(d)
        Next
        Return out
    End Function

    ''' <summary>
    ''' FAZA A DOUA: retrimite ACELAȘI payload, cu amprenta și hotărârile, iar serverul
    ''' comite. Abia acum ajunge descărcarea în tabele — toată, nu doar legăturile.
    ''' </summary>
    Private Async Function SalveazaPropunereaAsync() As Task
        Try
            Dim decizii As List(Of DecizieAsociere) =
                DeciziiDin(_stare.Instantanee, _stare.Receptii, _pozitie, _ignorat, _stergere)

            Cursor = Cursors.WaitCursor
            btnSalveaza.Enabled = False
            ' Prin coordonator, nu direct pe client: 409 ALEGERE_UNITATE se poate declanșa ȘI
            ' la salvare — pașii rulează din nou —, iar un răspuns de întrebare luat drept
            ' succes ar fi exact minciuna pentru care s-a retras `TrimiteAsync`.
            Dim coordonator As New PrelucrareCoordinator(_apiClient)
            Dim raspuns As PrelucrareRaspuns =
                Await _withReauthPrelucrare(Function() coordonator.SalveazaAsync(
                    _pachet, _propunere.Amprenta, decizii, _alegeri, CancellationToken.None))

            If raspuns Is Nothing Then
                ' Operatorul a renunțat la o alegere de unitate: serverul a derulat înapoi.
                ntfMesaj.Show("Salvarea s-a oprit la o alegere de unitate — nu s-a scris nimic. " &
                              "Apasă din nou «Salvează» ca să reiei.", NoticeKind.Warning)
                btnSalveaza.Enabled = (NehotarateleCount() = 0)
                Return
            End If

            _SAuSalvatModificari = True
            ' NU se reîncarcă: propunerea e consumată, iar o a doua citire ar cere celălalt mod.
            ' Corecturile de după se fac în editorul de oricând, care e la un clic distanță.
            ntfMesaj.Show(TextDupaSalvare(raspuns),
                          If(raspuns IsNot Nothing AndAlso raspuns.Avertismente.Count > 0,
                             NoticeKind.Warning, NoticeKind.Success))
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.SalveazaPropunereaAsync", ex)
            ntfMesaj.Show(TextDeEroare(ex, "Nu am putut salva descărcarea"), NoticeKind.Error)
            btnSalveaza.Enabled = (NehotarateleCount() = 0)
        Finally
            Cursor = Cursors.Default
        End Try
    End Function

    ''' <summary>Ce s-a scris, pe tabele — cifrele serverului, nu o repovestire.</summary>
    Private Shared Function TextDupaSalvare(raspuns As PrelucrareRaspuns) As String
        If raspuns Is Nothing Then Return "Descărcarea a fost salvată."
        Dim scrise As String = String.Join(", ",
            raspuns.Scrise.Where(Function(kvp) kvp.Value > 0).
                           OrderBy(Function(kvp) kvp.Key).
                           Select(Function(kvp) $"{kvp.Key}: {kvp.Value}"))
        Dim mesaj As String = If(scrise = "",
                                 "Descărcarea a fost salvată (nimic nou de scris).",
                                 "Descărcarea a fost salvată — " & scrise & ".")
        If raspuns.Avertismente.Count > 0 Then mesaj &= " " & String.Join(" ", raspuns.Avertismente)
        Return mesaj
    End Function

    Private Async Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _mod = ModAsociere.Propunere Then
                Await SalveazaPropunereaAsync()
                Return
            End If
            ' `deTrimis`, nu `comenzi`: VB e insensibil la litere mari/mici, deci o variabilă
            ' numită `comenzi` ar umbri metoda `Comenzi()` pentru tot restul procedurii — și
            ' apelul din Catch ar deveni o indexare de listă. Capcana e consemnată în notele
            ' proiectului tocmai fiindcă a mai mușcat o dată.
            Dim deTrimis As List(Of ComandaAsociere) = Comenzi()
            If deTrimis.Count = 0 Then
                ntfMesaj.Show("Nu ai schimbat nicio legătură.", NoticeKind.Warning)
                Return
            End If

            Cursor = Cursors.WaitCursor
            btnSalveaza.Enabled = False
            Dim rezultat As AsociereRezultat =
                Await _withReauthSalvare(Function() _apiClient.SalveazaLegaturiAsync(
                    _cod, _stare.Amprenta, deTrimis, CancellationToken.None))

            _SAuSalvatModificari = True
            ' Se reîncarcă de la server, nu se peticește tabloul local: după salvare, `Final` /
            ' `Partial` s-au recalculat, iar blocajele se pot fi schimbat — o proiecție locală
            ' ar arăta o stare pe care nimeni n-a citit-o.
            Await ReincarcaAsync()

            Dim mesaj As String = "Legăturile au fost salvate."
            If rezultat.Avertismente.Count > 0 Then
                ntfMesaj.Show(mesaj & " " & String.Join(" ", rezultat.Avertismente), NoticeKind.Warning)
            Else
                ntfMesaj.Show(mesaj, NoticeKind.Success)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.btnSalveaza_Click", ex)
            ntfMesaj.Show(TextDeEroare(ex, "Nu am putut salva legăturile"), NoticeKind.Error)
            btnSalveaza.Enabled = ProblemaReceptiilorNoi() = String.Empty AndAlso Comenzi().Count > 0
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Mesajul pentru operator. Cele două coduri-motiv ale rutei capătă text propriu: fără el,
    ''' operatorul ar citi o eroare tehnică acolo unde de fapt trebuie doar să reîncarce.
    ''' </summary>
    Private Shared Function TextDeEroare(ex As Exception, prefix As String) As String
        Dim api As ApiException = TryCast(ex, ApiException)
        If api IsNot Nothing Then
            Select Case api.Reason
                Case PrelucrarePropunere.MotivStareModificata
                    Return "Altcineva a modificat între timp recepțiile acestui angajament. " &
                           "Nu s-a scris nimic — închideți și deschideți din nou fereastra."
                Case AsociereStare.MotivInstantaneuBlocat
                    Return "Una dintre legături a fost înghețată între timp de o ordonanțare sau " &
                           "de o plată. Nu s-a scris nimic. " & api.Message
            End Select
            Return $"{prefix}: {api.Message}"
        End If
        Return $"{prefix}: {ex.Message}"
    End Function

    Private Sub btnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Close()
    End Sub

    ''' <summary>
    ''' EMPTIES THE CURRENT STEP'S PLACEMENTS (operator, 09.09.2026): everything this download
    ''' brought goes back to the "unplaced" basket, and whatever was already written on the
    ''' server is left untouched.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>The dividing line is not a list kept on the side, it is <c>Blocat</c>.</b> In
    ''' proposal mode the server marks <c>Blocat = True</c> on EVERY context row — that is, on
    ''' everything already linked in the database — and <c>False</c> on exactly the set to be
    ''' decided in this run (see <c>AsociereStare.DinPropunere</c>). The same line is what
    ''' <see cref="NehotarateleCount"/> and <see cref="DeciziiDin"/> read, so this button cannot
    ''' end up believing something different from the rest of the form.</para>
    ''' <para><b>It empties to UNPLACED, it does not restore the automatic suggestion.</b> The
    ''' button exists to wipe everything decided since the window opened; going back to the
    ''' suggestion would leave behind decisions the operator never took, which is exactly what
    ''' they were trying to get out of. The save button switches itself off on the rebuild —
    ''' there is no decided row left.</para>
    ''' <para>It asks for confirmation: this deletes work, and there is no Undo out of it.</para>
    ''' </remarks>
    Private Sub btnReseteaza_Click(sender As Object, e As EventArgs) Handles btnReseteaza.Click
        Try
            If _stare Is Nothing Then Return
            Dim deGolit As List(Of InstantaneuLegat) =
                _stare.Instantanee.Where(Function(i) Not i.Blocat).ToList()
            If deGolit.Count = 0 Then
                ntfMesaj.Show("Descărcarea asta n-a adus niciun instantaneu de așezat — " &
                              "nu e nimic de golit.", NoticeKind.Warning)
                Return
            End If

            Dim raspuns As DialogResult = KBotMessage.Show(
                Me,
                $"Se pun înapoi în «Neașezate» toate cele {deGolit.Count} instantanee aduse de " &
                "descărcarea curentă, iar ștergerile și marcajele «fără schimbare» puse de " &
                "dumneavoastră se anulează." & Environment.NewLine &
                "Legăturile care erau deja pe server NU se ating." & Environment.NewLine &
                "Goliți așezările?",
                "K-BOT — Așezarea recepțiilor descărcate",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If raspuns <> DialogResult.Yes Then Return

            For Each inst As InstantaneuLegat In deGolit
                _pozitie(inst.Idrh) = 0
                _ignorat(inst.Idrh) = False
                _stergere(inst.Idrh) = False
            Next
            _receptieSelectata = Nothing
            grid.ClearRows()
            Reconstruieste()
            ' `Reconstruieste` has already put up the "how many are left to decide" notice, and
            ' that is the right one now, so nothing is written over it.
        Catch ex As Exception
            ' UI boundary: cannot re-throw.
            GlobalErrorLog.Write("AsociereForm.btnReseteaza_Click", ex)
            ntfMesaj.Show(TextDeEroare(ex, "Nu am putut goli așezările"), NoticeKind.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Închiderea cu modificări nesalvate cere confirmare (D-D). Nu e nimic de derulat înapoi —
    ''' nimic nu a plecat spre server — dar munca de pe ecran s-ar pierde tăcut.
    ''' </summary>
    Private Sub AsociereForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If _mod = ModAsociere.Propunere Then
                ' Aici nu se pierd niște corecturi, ci TOATĂ descărcarea: faza întâi a derulat
                ' tranzacția înapoi, deci fără salvare nu rămâne în bază nici recepțiile, nici
                ' plățile, nici istoricul. Merită spus pe litere, nu cu formula obișnuită.
                If SAuSalvatModificari Then Return
                Dim raspunsul As DialogResult = KBotMessage.Show(
                    Me,
                    "Închizi fără să salvezi. NIMIC din descărcare nu ajunge în tabele — nici " &
                    "recepțiile, nici plățile, nici istoricul —, fiindcă serverul a derulat " &
                    "înapoi tot ce a pregătit." & Environment.NewLine &
                    "Va trebui reluată descărcarea. Închizi oricum?",
                    "K-BOT — Așezarea recepțiilor descărcate",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                If raspunsul = DialogResult.No Then e.Cancel = True
                Return
            End If

            If Comenzi().Count = 0 Then Return
            Dim raspuns As DialogResult = KBotMessage.Show(
                Me,
                "Ai schimbat legături care nu au fost salvate. Se pierd dacă închizi acum." & Environment.NewLine &
                "Închizi oricum?",
                "K-BOT — Legăturile recepțiilor",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If raspuns = DialogResult.No Then e.Cancel = True
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereForm.AsociereForm_FormClosing", ex)
        End Try
    End Sub

    ''' <summary>
    ''' A tree row together with the tree it lives in.
    ''' </summary>
    ''' <remarks>
    ''' A <c>TreeItem</c> does not know its own control, and the snapshots are split over TWO trees
    ''' — the linked ones on the left, the loose ones on the right. Selecting a row therefore takes
    ''' both halves, and keeping them together is the only version of this that cannot end up
    ''' asking one tree to select a row belonging to the other.
    ''' </remarks>
    Private NotInheritable Class RandDeArbore
        Public Sub New(arbore As AdvancedTreeControl, nod As AdvancedTreeControl.TreeItem)
            ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of
            ' the same name and an unqualified assignment would write the parameter into itself.
            Me.Arbore = arbore
            Me.Nod = nod
        End Sub

        Public ReadOnly Property Arbore As AdvancedTreeControl
        Public ReadOnly Property Nod As AdvancedTreeControl.TreeItem
    End Class
End Class

''' <summary>
''' Peste ce lucrează editorul: peste legăturile DIN BAZĂ, sau peste tabloul pe care tocmai
''' l-a propus o descărcare (felia 0055).
''' </summary>
''' <remarks>
''' Aceeași treabă și același ecran; ce diferă e de unde vine tabloul, unde pleacă hotărârea
''' și cât costă o închidere fără salvare. Vezi al doilea constructor al formularului.
''' </remarks>
Public Enum ModAsociere
    ''' <summary>Editorul de oricând: citește <c>GET /api/forexe/asociere</c>, trimite comenzi.</summary>
    Oricand = 0

    ''' <summary>Faza a doua a ingestiei: tabloul vine din propunere, hotărârile pleacă cu payload-ul.</summary>
    Propunere = 1
End Enum
