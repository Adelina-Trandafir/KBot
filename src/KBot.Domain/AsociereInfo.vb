Option Strict On
Imports System.Collections.Generic

' ══════════════════════════════════════════════════════════════════════════════════════
' Felia 0048-03 — contractul in DOUA FAZE al ingestiei FOREXE.
'
' De ce exista: istoricul FOREXE nu numeste niciodata receptia (F4), iar trecerea automata
' poate aseza doar ULTIMUL instantaneu al unui lant (F9). Restul — aproximativ
' (instantanee − receptii) per angajament (F10) — ajung, prin constructie, la operator.
' Asta nu e o cale de exceptie, e rezultatul normal al fiecarei descarcari. Nimic nu are
' voie sa ajunga in baza inainte ca omul sa fi raspuns, fiindca o asociere gresita e
' TACUTA si PERMANENTA (F12): strica TotalReceptii / PlatiAnt / Ramas pentru fiecare plata
' de dupa acea data, si nimic nu compara cifrele cu nimic.
'
' Sursa regulilor: docs/FUNDAMENT_Asociere_Receptii.md. F* si D-* vin de acolo si nu se
' re-deduc aici.
'
' Perechea de pe fir: routes/forexe/prelucrare_asociere.py. Numele campurilor JSON sunt
' ASCII pe AMANDOUA laturile (regula 0); traducerea intre ele si tipurile de mai jos se
' opreste la frontiera, in KBot.Api.
' ══════════════════════════════════════════════════════════════════════════════════════

''' <summary>
''' Ce a facut un instantaneu in viata unei recepții. Cele PATRU actiuni si niciuna alta.
''' </summary>
Public Enum ActiuneAsociere
    ''' <summary>Instantaneul apartine acelei recepții. Se scrie <c>FX_Receptii_H.IDRR</c>.</summary>
    Asociat = 0

    ''' <summary>
    ''' O salvare care nu a consemnat nicio schimbare (F17). <c>Sters = True</c> pe
    ''' instantaneu, <c>IDRR</c> lasat gol.
    '''
    ''' A ignora nu pierde NIMIC — randul nu poarta nicio informatie. A-l forta pe o
    ''' receptie injecteaza o valoare falsa in cronologia ei la acea data, iar verificarea
    ''' de capat de lant NU o prinde daca aterizeaza la mijloc. De-asta «totul trebuie
    ''' asezat» e regula gresita pentru clasa asta de randuri.
    ''' </summary>
    Ignorat = 1

    ''' <summary>
    ''' Acest instantaneu ESTE randul de stergere (F21): ultimul din lantul recepției lui.
    ''' <c>IDRR</c> se scrie, <c>EsteStergere = 1</c> pe instantaneu, <c>Sters = 1</c> pe
    ''' receptie.
    ''' </summary>
    Stergere = 2

    ''' <summary>
    ''' Instantaneul PORNESTE o receptie care nu mai exista (F26): a fost creata SI stearsa
    ''' inainte ca K-BOT sa fi descarcat vreodata angajamentul, deci nu are rand in
    ''' <c>ListaReceptii</c>. Receptia se materializeaza la salvare, din lantul pe care
    ''' operatorul il construieste pe ea.
    ''' </summary>
    Reconstituire = 3

    ''' <summary>
    ''' Instantaneul se rupe de recepția pe care sta acum si ramane neasezat
    ''' (<c>IDRR = NULL</c>, <c>Sters = 0</c>).
    '''
    ''' EXISTA DOAR IN EDITORUL DE ORICAND (felia 0048-04, <c>ComandaAsociere</c>).
    ''' Contractul in doua faze nu o are, fiindca acolo nimic nu e inca atasat si deci nu
    ''' e nimic de desprins; <c>ApiClient.NumeActiune</c> o refuza pe calea de ingestie,
    ''' iar asta e purtarea corecta, nu o scapare. E <c>btnDel_Click</c> din
    ''' <c>frmFX_DUBII_LISTA_HA</c>.
    ''' </summary>
    Desprins = 4
End Enum

''' <summary>
''' Tabloul pe care serverul l-a construit si apoi l-a derulat inapoi — faza «propunere».
''' POCO, fara logica.
''' </summary>
''' <remarks>
''' <see cref="Scrise"/> raporteaza ce S-AR FI scris. Tranzactia a fost anulata imediat
''' dupa; contorul arata a rezultat, dar descrie o rulare care nu a lasat nimic in urma.
''' </remarks>
Public NotInheritable Class PrelucrarePropunere

    ''' <summary>
    ''' Codul-motiv pe care serverul il pune in campul «reason» cand baza s-a schimbat intre
    ''' cele doua faze. Stabil, ca si celelalte coduri-motiv (TOKEN_UNKNOWN, ALEGERE_UNITATE).
    ''' </summary>
    Public Const MotivStareModificata As String = "STARE_MODIFICATA"

    Public Property CodAngajament As String = String.Empty

    ''' <summary>
    ''' Amprenta starii angajamentului la momentul propunerii. Se trimite INAPOI la salvare;
    ''' daca intre timp baza s-a miscat, serverul raspunde 409 si nu scrie nimic.
    ''' </summary>
    Public Property Amprenta As String = String.Empty

    ''' <summary>
    ''' TOATE recepțiile angajamentului, inclusiv cele neatinse de rulare si cele sterse —
    ''' formularul are nevoie de toate ca tinte de plasare. O receptie stearsa poate primi
    ''' in continuare un instantaneu ANTERIOR stergerii ei.
    ''' </summary>
    Public Property Receptii As New List(Of ReceptiePropusa)

    ''' <summary>
    ''' Fiecare instantaneu inca neasezat, asezat sau nu de trecerea automata (D-F) — si din
    ''' rularea asta, si din oricare anterioara.
    ''' </summary>
    Public Property Instantanee As New List(Of InstantaneuPropus)

    ''' <summary>
    ''' RESTUL instantaneelor angajamentului: cele deja legate si cele marcate «fara nicio
    ''' schimbare». CONTEXT — se arata, nu se decid, si toate vin cu
    ''' <see cref="InstantaneuLegat.Blocat"/> pus.
    '''
    ''' <para><b>De ce sunt aici</b> (08.09.2026). Fara ele, o recepție al carei lant era
    ''' deja legat sosea pe ecran goala: nicio linie in grafic, niciun marcaj pe banda,
    ''' nimic sub ea in arbore. Vazut de la locul operatorului, «recepțiile vechi nu mai
    ''' vin». Erau acolo; povestea lor nu era. Si nu e doar aspect: serverul isi da deja
    ''' vetourile F15 / F16 pe lantul INTREG, deci pana acum formularul era singurul care
    ''' nu vedea ce vede vetoul.</para>
    '''
    ''' <para>Ancora lor e <c>IDRH</c>, cheia reala — nu un indice de rand. In formular
    ''' primesc chei NEGATIVE (<c>-IDRH</c>), ca sa spuna dintr-o privire ca nu poarta o
    ''' hotarare si sa nu se ciocneasca cu cheile pozitive ale celor din
    ''' <see cref="Instantanee"/>; vezi <c>AsociereStare.DinPropunere</c>.</para>
    ''' </summary>
    Public Property InstantaneeAsezate As New List(Of InstantaneuLegat)

    ''' <summary>
    ''' Plățile angajamentului, contextul in care se citeste orice lant.
    '''
    ''' <para>§1.3 din fundament: fiecare ordonanțare citeste totalul recepției AȘA CUM
    ''' STĂTEA la data plății. Partea pe care cade un instantaneu față de o plată nu e un
    ''' amănunt de aspect — e diferența dintre o cifră corectă și una greșită, tăcut și
    ''' pentru totdeauna (F12). Deci reperele trebuie sa fie pe ecran CAND se așază, nu
    ''' abia după.</para>
    ''' </summary>
    Public Property Plati As New List(Of PlataAsociere)

    ''' <summary>Steagurile <c>FX_Angajament_Are</c>, per pas.</summary>
    Public Property Are As New Dictionary(Of String, Boolean)

    ''' <summary>Cate randuri s-ar fi scris, per tabel. Vezi nota din remarks.</summary>
    Public Property Scrise As New Dictionary(Of String, Integer)

    ''' <summary>Avertismente pentru operator (romana, diacritice literale).</summary>
    Public Property Avertismente As New List(Of String)
End Class

''' <summary>O receptie asa cum sta acum, cu liniile ei pe indicator. POCO.</summary>
Public NotInheritable Class ReceptiePropusa
    Public Property Idrr As Integer

    ''' <summary>
    ''' INDICELE randului ei in <c>ListaReceptii</c>, si NUMAI daca receptia s-a nascut in
    ''' rularea curenta. Nothing pentru toate celelalte.
    '''
    ''' <para><b>De ce exista</b> (08.09.2026, dupa prima rulare adevarata). Faza intai
    ''' deruleaza tranzactia inapoi neconditionat, dar contorul AUTO_INCREMENT NU se
    ''' deruleaza cu ea: o recepție nascuta in propunere primeste ALT <c>IDRR</c> la
    ''' salvare. O decizie care o numea prin <see cref="Idrr"/> cadea deci cu «Recepția N
    ''' nu există pe acest angajament» — si asa a si cazut. Indicele e stabil prin
    ''' constructie, fiindca amandoua fazele poarta acelasi payload; acelasi rationament ca
    ''' <see cref="InstantaneuPropus.RandIstoric"/> (F24).</para>
    '''
    ''' <para><see cref="Idrr"/> RAMANE, si ramane cheia pe care formularul isi tine
    ''' dictionarele: e unica in tabloul primit. Doar ca, pentru recepțiile astea, nu e un
    ''' nume care se poate trimite inapoi.</para>
    ''' </summary>
    Public Property RandReceptie As Integer?
    ''' <summary>Data CREARII recepției. Nu are nimic de-a face cu <c>DataH</c> (F6).</summary>
    Public Property DataR As Date
    Public Property SumaAntet As Double
    Public Property Descriere As String = String.Empty

    ''' <summary>
    ''' Stearsa pe site (F22). Ramane tinta valida pentru un instantaneu ANTERIOR stergerii:
    ''' vetoul e pe data, nu pe steag.
    ''' </summary>
    Public Property Sters As Boolean

    ''' <summary>
    ''' Construita din propriile instantanee fiindca nu mai exista pe site (F26). Mereu
    ''' impreuna cu <see cref="Sters"/>, dar alt fapt: <c>Sters</c> spune ce s-a intamplat
    ''' cu ea, <c>Reconstituit</c> spune de unde stim ca a existat.
    ''' </summary>
    Public Property Reconstituit As Boolean

    ''' <summary>
    ''' F28: in clipa reconstituirii, o ALTA reconstituire pe acelasi angajament facea gruparea
    ''' imposibil de verificat (F27), deci ea a fost o judecata, nu o verificare.
    '''
    ''' Al TREILEA fapt, si nu se colapseaza in celelalte doua: <c>Sters</c> = stearsa pe site,
    ''' <c>Reconstituit</c> = reconstruita din propriile instantanee, asta = gruparea nu a putut
    ''' fi probata. Nu se sterge niciodata — o rulare de mai tarziu care vede una singura nu
    ''' face gruparea de atunci mai verificabila decat era in clipa in care s-a facut.
    ''' </summary>
    Public Property ReconstituitNesigur As Boolean

    Public Property Rhr As New List(Of LinieReceptie)
End Class

''' <summary>O linie de receptie (<c>FX_Receptii_RHR</c>). POCO.</summary>
Public NotInheritable Class LinieReceptie
    Public Property CodIndicator As String = String.Empty
    Public Property CodAi As String = String.Empty
    Public Property CodSsi As String = String.Empty
    ''' <summary>Creditul bugetar AL INDICATORULUI — constant pe indicator, nu per receptie.</summary>
    Public Property CreditBugetar As Double
    Public Property Valoare As Double
    Public Property ValoareN As Double
End Class

''' <summary>
''' Numele sub care un instantaneu de asezat calatoreste de la propunere la salvare —
''' ANCORA (F24 / F34). Doua forme, si numai doua, iar cele doua nu se confunda niciodata:
''' <list type="bullet">
''' <item><c>rand:N</c> — indicele randului lui de istoric in <c>TabelIstoric</c> (F24),
''' cand randul E in sarcina utila. Id-urile date in propunere dispar la derularea inapoi;
''' indicele e stabil prin constructie, fiindca amandoua fazele poarta acelasi payload.</item>
''' <item><c>idh:N</c> — <c>FX_Istoric.ID</c> (F34, 18.09.2026), cand randul NU e in sarcina
''' utila. Fluxul REVERSE aduce doar istoricul mai nou decat ultimul de acasa, deci un
''' instantaneu ramas neasezat dintr-o rulare mai veche nu-si mai gaseste indicele; ID-ul
''' randului lui e insa stabil intre faze, fiindca randul exista dinaintea rularii si
''' amprenta garanteaza ca tabelul nu s-a miscat.</item>
''' </list>
''' Perechea de pe fir: <c>prelucrare_asociere.ancora()</c>.
''' </summary>
Public NotInheritable Class AncoraAsociere
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Textul ancorei, sau sirul gol cand instantaneul nu are niciun nume (nici indice, nici
    ''' id de istoric) — un astfel de rand nu poate primi hotarare din descarcare.
    ''' </summary>
    Public Shared Function Cheie(randIstoric As Integer?, idh As Integer?) As String
        If randIstoric.HasValue Then Return "rand:" & randIstoric.Value.ToString(Globalization.CultureInfo.InvariantCulture)
        If idh.HasValue AndAlso idh.Value > 0 Then Return "idh:" & idh.Value.ToString(Globalization.CultureInfo.InvariantCulture)
        Return String.Empty
    End Function

    ''' <summary>Cum se numeste ancora in mesajele catre operator: «rândul 3» / «istoric 5786».</summary>
    Public Shared Function Text(randIstoric As Integer?, idh As Integer?) As String
        If randIstoric.HasValue Then Return "rândul " & randIstoric.Value.ToString(Globalization.CultureInfo.InvariantCulture)
        If idh.HasValue AndAlso idh.Value > 0 Then Return "istoric " & idh.Value.ToString(Globalization.CultureInfo.InvariantCulture)
        Return "(fără nume)"
    End Function
End Class

''' <summary>Un instantaneu de asezat, cu sugestia automata daca a fost una. POCO.</summary>
Public NotInheritable Class InstantaneuPropus

    ''' <summary>
    ''' INDICELE de la zero al randului in <c>TabelIstoric</c> (F24) — NU o cheie de baza de
    ''' date. Nothing cand randul lui de istoric NU e in aceasta descarcare; atunci ancora e
    ''' <see cref="Idh"/> (F34). Vezi <see cref="AncoraAsociere"/>.
    ''' </summary>
    Public Property RandIstoric As Integer?

    ''' <summary>
    ''' <c>FX_Istoric.ID</c> al randului lui de istoric. Ancora cand <see cref="RandIstoric"/>
    ''' e Nothing (F34); altfel doar informativ. 0 = necunoscut.
    ''' </summary>
    Public Property Idh As Integer

    ''' <summary>
    ''' <c>FX_Receptii_H.IDRH</c> AL PROPUNERII. NU e un nume care supravietuieste: se atribuie
    ''' din nou la salvare, dupa derularea inapoi. E unic in tabloul de fata si atat — cheia
    ''' pe care formularul isi tine dictionarele cat traieste propunerea
    ''' (<c>AsociereStare.DinPropunere</c>). Nu pleaca niciodata inapoi spre server.
    ''' </summary>
    Public Property Idrh As Integer

    ''' <summary>Ancora, ca text: vezi <see cref="AncoraAsociere.Cheie"/>.</summary>
    Public Function Ancora() As String
        Return AncoraAsociere.Cheie(RandIstoric, Idh)
    End Function

    ''' <summary>Momentul editarii. ESTE axa timpului lantului (F2).</summary>
    Public Property DataH As Date

    Public Property Descriere As String = String.Empty

    ''' <summary>Valoarea INTREGII recepții la acel moment, nu marimea schimbarii (F3).</summary>
    Public Property Total As Double

    ''' <summary>Randul de stergere al lantului (F21). Nu are linii pe indicator.</summary>
    Public Property Stergere As Boolean

    ''' <summary>Ce a propus trecerea automata; 0 daca nu a avut raspuns.</summary>
    Public Property SugestieIdrr As Integer

    ''' <summary>
    ''' True cand valoarea de mai sus vine de la masina. Se ARATA ca sugestie, nu ca fapt
    ''' (F18): sub F11 trecerea automata poate fi GRESITA, nu doar incompleta.
    ''' </summary>
    Public Property SugestieAutomata As Boolean

    Public Property Linii As New List(Of LinieInstantaneu)
End Class

''' <summary>O linie de instantaneu (<c>FX_Receptii</c>). POCO.</summary>
Public NotInheritable Class LinieInstantaneu
    Public Property CodIndicator As String = String.Empty
    Public Property CodAi As String = String.Empty
    Public Property CodSsi As String = String.Empty
    Public Property IdClsf As Integer
    Public Property Valoare As Double
End Class

''' <summary>Raspunsul operatorului pentru UN instantaneu. POCO.</summary>
''' <remarks>
''' <see cref="Idrr"/> si <see cref="ReceptieNoua"/> se exclud reciproc: exact una dintre
''' ele pentru <see cref="ActiuneAsociere.Asociat"/> si <see cref="ActiuneAsociere.Stergere"/>,
''' doar eticheta pentru <see cref="ActiuneAsociere.Reconstituire"/>, niciuna pentru
''' <see cref="ActiuneAsociere.Ignorat"/>. Serverul respinge orice alta combinatie cu 400 —
''' nu ghiceste.
'''
''' <see cref="ReceptieNoua"/> e o ETICHETA data de client, nu o cheie: receptia pe care o
''' numeste inca nu exista si isi primeste <c>IDRR</c> abia la salvare.
''' </remarks>
Public NotInheritable Class DecizieAsociere
    ''' <summary>
    ''' Ancora F24: indicele randului de istoric in <c>TabelIstoric</c>. EXACT una dintre
    ''' <see cref="RandIstoric"/> si <see cref="Idh"/> — serverul respinge cu 400 si lipsa
    ''' amandurora, si prezenta amandurora. Se trimite inapoi exact ce a dat propunerea
    ''' (<see cref="InstantaneuPropus.RandIstoric"/> / <see cref="InstantaneuPropus.Idh"/>).
    ''' </summary>
    Public Property RandIstoric As Integer?

    ''' <summary>Ancora F34: <c>FX_Istoric.ID</c>, cand randul nu e in descarcare. Vezi <see cref="AncoraAsociere"/>.</summary>
    Public Property Idh As Integer?

    ''' <summary>Ancora, ca text: vezi <see cref="AncoraAsociere.Cheie"/>.</summary>
    Public Function Ancora() As String
        Return AncoraAsociere.Cheie(RandIstoric, Idh)
    End Function

    ''' <summary>
    ''' Data instantaneului, calatorind alaturi de ancora. Serverul o compara cu randul aflat
    ''' la acea ancora: daca nu se potriveste, fisierul de decizii e invechit si cererea
    ''' cade ZGOMOTOS in loc sa asocieze tacut alt rand.
    ''' </summary>
    Public Property DataH As Date

    Public Property Actiune As ActiuneAsociere

    ''' <summary>Receptia existenta pe care se aseaza. 0 = niciuna.</summary>
    Public Property Idrr As Integer

    ''' <summary>
    ''' Indicele randului in <c>ListaReceptii</c> al unei recepții NASCUTE de rularea
    ''' curenta. Nothing = niciuna. Vezi <see cref="ReceptiePropusa.RandReceptie"/>: pentru
    ''' recepțiile astea <see cref="Idrr"/> nu supravietuieste pana la salvare.
    ''' </summary>
    Public Property RandReceptie As Integer?

    ''' <summary>Eticheta unei recepții reconstituite. Nothing = niciuna.</summary>
    Public Property ReceptieNoua As String
End Class


' ── Dosarul local (felia 0048-03, decizia D-C) ───────────────────────────────────────
' POCO-ul sta in KBot.Domain, iar magazinul care il scrie in KBot.Common (AsociereStore),
' langa KBotPaths — celalalt magazin-fisier de langa executabil, si chiar forma pe care
' planul a cerut sa o urmam.
'
' DE CE NU IN KBot.App, unde a fost scris intai: KBot.App REFERA KBot.DevHarness (numai pe
' Debug), deci sagetile merg App -> DevHarness. Un tip aflat in App nu poate fi vazut de
' harness, iar planul cere ca dosarul sa fie exercitat TOCMAI din harness. Common si
' Domain sunt vazute de amandoua.

''' <summary>
''' Ce se pastreaza pe disc pentru o asociere in curs. POCO — fara logica, deci fara
''' Try/Catch (regula casei).
''' </summary>
Public NotInheritable Class AsociereDosar
    Public Property CodAngajament As String = String.Empty

    ''' <summary>Cand s-a cerut propunerea.</summary>
    Public Property Creat As DateTime

    ''' <summary>Cand s-a atins ultima oara dosarul.</summary>
    Public Property Modificat As DateTime

    ''' <summary>
    ''' Amprenta starii bazei la momentul propunerii. Se trimite inapoi la salvare; daca
    ''' nu se mai potriveste, serverul raspunde 409 STARE_MODIFICATA si nu scrie nimic.
    ''' </summary>
    Public Property Amprenta As String = String.Empty

    ''' <summary>
    ''' Sarcina utila EXACT cum a fost trimisa. Obligatorie — vezi nota clasei despre F24.
    ''' </summary>
    Public Property Payload As PrelucrareRezultat

    ''' <summary>Tabloul pe care l-a intors propunerea.</summary>
    Public Property Propunere As PrelucrarePropunere

    ''' <summary>
    ''' Alegerile de unitate deja facute. Se retrimit la salvare: bifa «nu ma mai intreba»
    ''' s-a derulat inapoi impreuna cu propunerea, deci serverul nu si-o aminteste.
    ''' </summary>
    Public Property Alegeri As New List(Of AlegereUnitate)

    ''' <summary>Deciziile luate pana acum. Partiale pana cand operatorul termina.</summary>
    Public Property Decizii As New List(Of DecizieAsociere)

    ''' <summary>
    ''' True cand fiecare instantaneu din propunere are o decizie. Serverul cere acoperire
    ''' COMPLETA (400 altfel): tacerea nu are voie sa insemne «ignora-l».
    ''' </summary>
    Public ReadOnly Property EsteComplet As Boolean
        Get
            If Propunere Is Nothing Then Return False
            ' Pe ANCORA, nu pe indice: un instantaneu ancorat pe id-ul de istoric (F34) are
            ' indicele Nothing, iar unul fara niciun nume nu poate fi niciodata «decis».
            Dim decise As New HashSet(Of String)(Decizii.Select(Function(d) d.Ancora()))
            decise.Remove(String.Empty)
            Return Propunere.Instantanee.All(Function(i) decise.Contains(i.Ancora()))
        End Get
    End Property
End Class
