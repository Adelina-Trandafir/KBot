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

''' <summary>Un instantaneu de asezat, cu sugestia automata daca a fost una. POCO.</summary>
Public NotInheritable Class InstantaneuPropus

    ''' <summary>
    ''' INDICELE de la zero al randului in <c>TabelIstoric</c> (F24) — NU o cheie de baza de
    ''' date.
    '''
    ''' Id-urile atribuite in timpul propunerii dispar la derularea inapoi si nu se intorc
    ''' identice. Indicele e stabil PRIN CONSTRUCTIE, fiindca amandoua fazele poarta acelasi
    ''' payload — de-asta fisierul local pastreaza sarcina utila exact cum a fost trimisa.
    ''' </summary>
    Public Property RandIstoric As Integer

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
    Public Property RandIstoric As Integer

    ''' <summary>
    ''' Data instantaneului, calatorind alaturi de indice. Serverul o compara cu randul aflat
    ''' la acel indice in payload: daca nu se potriveste, fisierul de decizii e invechit si
    ''' cererea cade ZGOMOTOS in loc sa asocieze tacut alt rand.
    ''' </summary>
    Public Property DataH As Date

    Public Property Actiune As ActiuneAsociere

    ''' <summary>Receptia existenta pe care se aseaza. 0 = niciuna.</summary>
    Public Property Idrr As Integer

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
            Dim decise As New HashSet(Of Integer)(Decizii.Select(Function(d) d.RandIstoric))
            Return Propunere.Instantanee.All(Function(i) decise.Contains(i.RandIstoric))
        End Get
    End Property
End Class
