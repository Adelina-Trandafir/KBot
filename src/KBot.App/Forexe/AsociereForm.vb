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

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _cod As String
    ' Plasa 401 a shell-ului, specializată pe fiecare dintre cele două forme de răspuns:
    ' politica de re-login rămâne într-un singur loc, formularul doar o folosește.
    Private ReadOnly _withReauthStare As Func(Of Func(Of Task(Of AsociereStare)), Task(Of AsociereStare))
    Private ReadOnly _withReauthSalvare As Func(Of Func(Of Task(Of AsociereRezultat)), Task(Of AsociereRezultat))

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
            Dim stare As AsociereStare =
                Await _withReauthStare(Function() _apiClient.GetAsociereAsync(_cod, CancellationToken.None))

            _stare = stare
            _receptieSelectata = Nothing
            _pozitie.Clear()
            _ignorat.Clear()
            _stergere.Clear()
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
    ''' </summary>
    Private Sub Reconstruieste()
        Try
            treeLant.Clear()
            treeLibere.Clear()
            grid.ClearRows()
            ' Cleared with the trees, never after: a stale entry here points at a TreeItem that is
            ' no longer on screen, and colouring it would look exactly like doing nothing.
            _nodReceptie.Clear()
            _nodInstantaneu.Clear()
            ' Benzile se golesc AICI, cu arborii, nu în `ReconstruiesteBenzi` — pe drumul de mai
            ' jos cu `_stare Is Nothing` metoda aia nici nu se mai cheamă, iar o suprafață rămasă
            ' plină lângă doi arbori goliți ar fi singurul lucru de pe ecran care mai susține că
            ' există date.
            ReconstruiesteBenzi()
            If _stare Is Nothing Then Return

            ' ── stânga: recepțiile, fiecare cu lanțul ei ordonat după DataH ──────────
            For Each rec As ReceptiePropusa In _stare.Receptii.OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
                Dim lant As List(Of InstantaneuLegat) = LantulReceptiei(rec)

                Dim nod As AdvancedTreeControl.TreeItem =
                    treeLant.AddItem(CHEIE_RECEPTIE & rec.Idrr, CaptionReceptie(rec, lant), pExpanded:=True, pLeftIconClosed:=Il_Receptii.Images.Item("Receptii"))
                nod.Tag = rec
                nod.Bold = True
                nod.Tooltip = TooltipReceptie(rec, lant)
                _nodReceptie(rec.Idrr) = nod

                For Each inst As InstantaneuLegat In lant
                    Dim frunza As AdvancedTreeControl.TreeItem =
                        treeLant.AddItem(CHEIE_INSTANTANEU & inst.Idrh, CaptionInstantaneu(inst, rec), nod, pLeftIconClosed:=Il_Receptii.Images.Item("Receptii_Link"))
                    frunza.Tag = inst
                    frunza.Tooltip = TooltipInstantaneu(inst, rec)
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
            radacina.Tooltip = "Trage aici un instantaneu ca să-l desprinzi de recepția lui."

            For Each inst As InstantaneuLegat In libere
                Dim frunza As AdvancedTreeControl.TreeItem =
                    treeLibere.AddItem(CHEIE_INSTANTANEU & inst.Idrh, CaptionInstantaneu(inst), radacina)
                frunza.Tag = inst
                frunza.Tooltip = TooltipInstantaneu(inst)
                ColoreazaInstantaneu(frunza, inst)
                _nodInstantaneu(inst.Idrh) = New RandDeArbore(treeLibere, frunza)
            Next

            btnSalveaza.Enabled = Comenzi().Count > 0
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

    Private Function EsteStergere(idrh As Integer) As Boolean
        Dim v As Boolean
        If _stergere.TryGetValue(idrh, v) Then Return v
        Return False
    End Function

    Private Function CaptionReceptie(rec As ReceptiePropusa, lant As List(Of InstantaneuLegat)) As String
        Dim semne As String = String.Empty
        ' TEXT, nu pictograme: cele trei steaguri sunt fapte diferite și au nevoie fiecare de
        ' cuvântul lui, iar o pictogramă ar cere un control nou în designer.
        If rec.Sters Then semne &= " [ștearsă]"
        If rec.Reconstituit Then semne &= " [reconstituită]"
        Return $"{rec.DataR:dd.MM.yyyy}~~~{Bani(rec.SumaAntet)} ({lant.Count}){semne}"
    End Function

    Private Function CaptionInstantaneu(inst As InstantaneuLegat, Optional rec As ReceptiePropusa = Nothing) As String
        Dim semne As String = String.Empty
        'If inst.Blocat Then semne &= "  🔒"
        If EsteStergere(inst.Idrh) Then semne &= "  [ștergere]"
        If EsteIgnorat(inst.Idrh) Then semne &= "  [fără schimbare]"
        ' Semnul care a rămas din F13 retras. Scris pe rând, nu ridicat ca refuz: plasarea e
        ' permisă, iar operatorul decide dacă data recepției e greșită sau instantaneul e al
        ' altei recepții.
        If EsteInainteDeDataReceptiei(inst, rec) Then semne &= "  [înainte de data recepției]"
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
        sb.AppendLine($"Recepția {rec.Idrr} · creată {rec.DataR:dd.MM.yyyy}")
        sb.AppendLine($"Valoare acum: {Bani(rec.SumaAntet)}")
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
        ' Câte instantanee cad înaintea datei scrise pe recepție. Numărul contează mai mult decât
        ' faptul: unul singur e o ciudățenie, tot lanțul înseamnă că data recepției e greșită.
        ' `.Where(...).Count()`, nu `.Count(...)`: pe un `List(Of T)`, `Count` e PROPRIETATE și
        ' umbrește supraîncărcarea LINQ cu predicat, deci varianta scurtă nici nu compilează.
        Dim inainte As Integer = lant.Where(Function(x) EsteInainteDeDataReceptiei(x, rec)).Count()
        If inainte > 0 Then
            sb.AppendLine($"⚠ {inainte} din {lant.Count} instantanee sunt mai vechi decât data recepției " &
                          $"({rec.DataR:dd.MM.yyyy}). Data se scrie de mână pe site, deci nu e o piedică — " &
                          "dar merită privită.")
        End If
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

    Private Function TooltipInstantaneu(inst As InstantaneuLegat, Optional rec As ReceptiePropusa = Nothing) As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine($"{inst.DataH:dd.MM.yyyy HH:mm:ss} · {Bani(inst.Total)}")
        If Not String.IsNullOrWhiteSpace(inst.Descriere) Then sb.AppendLine(inst.Descriere)
        Dim ind As String = String.Join(", ", inst.Indicatori().OrderBy(Function(x) x))
        If ind <> "" Then sb.AppendLine($"Indicatori: {ind}")
        If EsteInainteDeDataReceptiei(inst, rec) Then
            sb.AppendLine()
            sb.AppendLine($"⚠ Instantaneul e mai vechi decât data recepției ({rec.DataR:dd.MM.yyyy}). " &
                          "Data recepției se scrie de mână pe site și se poate schimba, deci asta NU " &
                          "împiedică așezarea — dar ori data e greșită, ori instantaneul e al altei recepții.")
        End If
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
        Return _stare.Receptii.FirstOrDefault(Function(r) r.Idrr = idrr)
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
    ''' creării (F29). Un veto clădit pe un câmp tastat refuză plasări corecte. Comparația
    ''' supraviețuiește ca SEMN, în <see cref="EsteInainteDeDataReceptiei"/>, și atât.</para>
    ''' </summary>
    Private Function MotivulRefuzului(inst As InstantaneuLegat, rec As ReceptiePropusa) As String
        Dim indInst As HashSet(Of String) = inst.Indicatori()
        Dim indRec As New HashSet(Of String)(
            rec.Rhr.Where(Function(l) Not String.IsNullOrEmpty(l.CodIndicator)).Select(Function(l) l.CodIndicator),
            StringComparer.OrdinalIgnoreCase)

        ' F14 — submulțimea de indicatori. Slab (majoritatea angajamentelor au un singur
        ' indicator), dar corect.
        If indInst.Count > 0 AndAlso Not indInst.IsSubsetOf(indRec) Then
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

    ''' <summary>
    ''' Instantaneul este mai vechi decât data scrisă pe recepție — un SEMN, niciodată un refuz.
    ''' </summary>
    ''' <remarks>
    ''' <para>Asta a fost F13 până pe 31.08.2026, când operatorul a corectat premisa: <c>DataR</c>
    ''' nu spune când a apărut recepția, ci ce a tastat cineva în câmpul ăla pe site — și îl poate
    ''' retasta oricând (F29). Ca veto, regula putea refuza o plasare corectă; pe calea de
    ''' ingestie asta însemna un operator blocat pe o recepție pe care nu avea cum s-o repare,
    ''' adică exact înfundarea pe care F10 spune că nu are voie să existe.</para>
    ''' <para>Ca semn rămâne utilă: dacă instantaneele unei recepții cad înaintea datei ei, ori
    ''' data e greșită, ori instantaneele sunt ale altei recepții. Care dintre ele — vede
    ''' operatorul, nu mașina.</para>
    ''' </remarks>
    Private Shared Function EsteInainteDeDataReceptiei(inst As InstantaneuLegat, rec As ReceptiePropusa) As Boolean
        If inst Is Nothing OrElse rec Is Nothing Then Return False
        ' Pe ZI, nu pe timestamp complet: `DataR` e o dată tastată, deci sosește la miezul nopții,
        ' iar `DataH` e ceasul sistemului. Comparate ca momente, ORICE instantaneu din chiar ziua
        ' recepției ar ieși «înainte de ea» — un semn care s-ar aprinde pe date perfect corecte.
        Return rec.DataR.Date > inst.DataH.Date
    End Function

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
        Return _stare.Receptii.FirstOrDefault(Function(r) r.Idrr = idrr)
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
                Dim idrr As Integer = IdrrDinCheie(serie.Key)
                Dim radacina As AdvancedTreeControl.TreeItem = Nothing
                If idrr > 0 AndAlso _nodReceptie.TryGetValue(idrr, radacina) AndAlso
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

            AplicaCulorileBenzii(_bandaReceptie, _marcajInstantaneu)

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
    ''' <para>Un instantaneu are acum TREI înfățișări pe ecran: rândul din arbore, punctul din
    ''' grafic și marcajul de pe bandă. Rostul culorii e să spună că sunt același lucru, deci ea
    ''' vine dintr-un singur loc — graficul — și e purtată de aici către celelalte.</para>
    ''' <para><b>Benzile nu se golesc de culoare, doar marcajele.</b> O bandă își păstrează
    ''' culoarea scrisă la construire fiindcă pe fila «Recepția» graficul are o părere doar despre
    ''' un singur lanț; dacă s-ar goli tot, restul benzilor ar rămâne incolore exact atunci când
    ''' suprafața e cea mai încărcată, iar culoarea n-ar mai lega nimic de nimic.</para>
    ''' </remarks>
    Private Sub AplicaCulorileBenzii(bandaDupaIdrr As Dictionary(Of Integer, KBotLane),
                                     marcajDupaIdrh As Dictionary(Of Integer, KBotLaneMarker))
        If bandaDupaIdrr Is Nothing OrElse marcajDupaIdrh Is Nothing Then Return

        For Each marcaj As KBotLaneMarker In marcajDupaIdrh.Values
            marcaj.MarkerColor = Color.Empty
        Next

        For Each serie As KBotChartSeries In grafic.Series
            If String.Equals(serie.Key, SERIA_TOTAL, StringComparison.Ordinal) Then Continue For

            Dim idrr As Integer = IdrrDinCheie(serie.Key)
            Dim banda As KBotLane = Nothing
            If idrr > 0 AndAlso bandaDupaIdrr.TryGetValue(idrr, banda) AndAlso
               serie.LineColor <> Color.Empty Then
                banda.LaneColor = serie.LineColor
            End If

            For Each punct As KBotChartPoint In serie.Points
                If punct.PointColor = Color.Empty Then Continue For
                Dim inst As InstantaneuLegat = TryCast(punct.Tag, InstantaneuLegat)
                If inst Is Nothing Then Continue For
                Dim marcaj As KBotLaneMarker = Nothing
                If marcajDupaIdrh.TryGetValue(inst.Idrh, marcaj) Then
                    marcaj.MarkerColor = punct.PointColor
                End If
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
        For Each rec As ReceptiePropusa In _stare.Receptii.OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
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
            For Each rec As ReceptiePropusa In _stare.Receptii.OrderBy(Function(r) r.DataR).ThenBy(Function(r) r.Idrr)
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

                For Each inst As InstantaneuLegat In lant
                    Dim marcaj As KBotLaneMarker = banda.AddMarker(inst.DataH, Bani(inst.Total))
                    marcaj.Tag = inst
                    marcaj.Tooltip = TooltipInstantaneu(inst, rec)
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
            For Each inst As InstantaneuLegat In libere
                Dim marcaj As KBotLaneMarker = bandaLibere.AddMarker(inst.DataH, Bani(inst.Total))
                marcaj.Tag = inst
                marcaj.Tooltip = TooltipInstantaneu(inst)
                marcaj.Style = StilulMarcajului(inst, asezat:=False)
                marcajDupaIdrh(inst.Idrh) = marcaj
            Next

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
    ''' </remarks>
    Private Sub ConstruiesteReperelePlatilor(tinta As KBotLaneView, siInGrafic As Boolean)
        If _stare Is Nothing Then Return

        For Each plata As PlataAsociere In _stare.Plati.OrderBy(Function(p) p.DataPlata)
            Dim titlu As String = $"Plată {plata.DataPlata:dd.MM.yyyy} · {Bani(plata.Suma)}"
            Dim corp As String = If(String.IsNullOrWhiteSpace(plata.NrOp),
                                    "Totalul recepțiilor la data asta a intrat în ordonanțare.",
                                    $"OP {plata.NrOp}" & Environment.NewLine &
                                    "Totalul recepțiilor la data asta a intrat în ordonanțare.")

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
    ''' <b>F13 nu se consultă</b> (retras 31.08.2026). Dacă momentul cade înaintea lui
    ''' <c>DataR</c> al recepției-țintă, aruncarea se face oricum, iar observația o poartă semnul
    ''' de pe rând și eticheta plutitoare.
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
        AplicaCulorileBenzii(banda, marcaj)
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
        Dim inst As InstantaneuLegat = TryCast(nod?.Tag, InstantaneuLegat)
        If inst Is Nothing Then Return
        If inst.Blocat Then
            ntfMesaj.Show("Această legătură nu se mai poate modifica. " &
                          String.Join(" ", inst.Motive), NoticeKind.Warning)
            Return
        End If

        Dim asezat As Boolean = PozitiaLui(inst) > 0
        Dim intrari As New List(Of CustomPopupItem)()
        If asezat Then
            intrari.Add(New CustomPopupItem(MENIU_DESPRINDE, "&Desprinde de recepție", Il_Receptii.Images.Item("link_break")))
            If EsteStergere(inst.Idrh) Then
                intrari.Add(New CustomPopupItem(MENIU_NU_STERGERE, "Nu mai e rândul de ș&tergere"))
            Else
                intrari.Add(New CustomPopupItem(MENIU_STERGERE, "Este rândul de ș&tergere"))
            End If
        Else
            If EsteIgnorat(inst.Idrh) Then
                intrari.Add(New CustomPopupItem(MENIU_NU_IGNORA, "&Consemnează o schimbare"))
            Else
                intrari.Add(New CustomPopupItem(MENIU_IGNORA, "&Nu consemnează nicio schimbare"))
            End If
        End If

        Dim meniu As New CustomPopup(intrari)
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs) AplicaComandaDeMeniu(inst, ev.Item.Key)
        meniu.ShowAtCursor(If(nod Is Nothing, CType(treeLant, Control), CType(treeLant, Control)))
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
            Else
                out.Add(New ComandaAsociere() With {
                    .Idrh = inst.Idrh,
                    .Actiune = If(stergereNou, ActiuneAsociere.Stergere, ActiuneAsociere.Asociat),
                    .Idrr = idrrNou})
            End If
        Next
        Return out
    End Function

    Private Async Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
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
            btnSalveaza.Enabled = Comenzi().Count > 0
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
    ''' Închiderea cu modificări nesalvate cere confirmare (D-D). Nu e nimic de derulat înapoi —
    ''' nimic nu a plecat spre server — dar munca de pe ecran s-ar pierde tăcut.
    ''' </summary>
    Private Sub AsociereForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If Comenzi().Count = 0 Then Return
            Dim raspuns As DialogResult = MessageBox.Show(
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
