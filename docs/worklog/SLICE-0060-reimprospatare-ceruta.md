# SLICE 0060 — Reîmprospătarea cerută de operator: macheta care nu se mai deschide degeaba, nodul care rămâne selectat, și cele două reîmprospătări parțiale

**Data:** 10.09.2026
**Cerute de operator** (patru propoziții, în ordinea în care le-a scris):

1. La salvarea asocierilor, dacă totul e ok, se închide macheta de asocieri și se dă refresh
   la datele pentru nodul selectat din MainForm — **dar nodul rămâne selectat**.
2. Dacă nu există nimic în coșul de neasociate, macheta **nu se mai afișează deloc**: se
   salvează direct și se dă refresh la arbore, tot cu nodul păstrat.
3. La refresh pe nod: un formular cu grila proprie (prima coloană bifabilă) care lasă
   operatorul să aleagă **ce recepții** vrea reîmprospătate, plus un `.wfl` care să suporte
   opțiunea. Macheta pornește cu toate bifate, dar cu un **parametru de aplicație**
   configurabil pe viitor.
4. Workflow-uri care fac refresh **doar pe rezervări** sau **doar pe recepții**, legate de
   butonul din dreapta subsolului arborelui din vederile lor. Cel de recepții deschide și el
   macheta de selecție.

**Cerut explicit:** «nu testa. nu git» — nicio suită nu s-a rulat și nimic nu s-a comis.

---

## 1. Macheta se închide după o salvare reușită (cererea 1)

`AsociereForm.SpuneSiInchide` — o singură cale, folosită de amândouă modurile (propunere și
oricând).

**Prin casetă, nu prin banda de mesaje.** Banda trăiește cât fereastra, iar fereastra tocmai
se închide: propoziția care spune CE s-a scris (tabelele și cifrele întoarse de server) ar
sclipi și ar dispărea. `KBotMessage.Show` o ține pe ecran până când omul apasă și o scrie în
`Logs\mesaje_operator.log`, deci nu se pierde nici după aceea.

**Închiderea se face DOAR pe drumul reușit.** La eroare fereastra rămâne deschisă cu tot ce a
așezat operatorul; altfel un 409 i-ar șterge munca de pe ecran și l-ar trimite să reia
descărcarea de la capăt.

**În modul «oricând» reîncărcarea de după salvare a RĂMAS**, deși fereastra se închide imediat
— și nu e risipă: după `ReincarcaAsync` lista `Comenzi()` e goală, deci întrebarea din
`AsociereForm_FormClosing` («ai schimbări nesalvate, le pierzi?») nu se mai pune. Fără ea,
închiderea ar cere operatorului să confirme pierderea a ceea ce tocmai salvase.

`TextDupaSalvare` a devenit `Friend Shared`: aceeași propoziție o arată acum și shell-ul, pe
drumul fără machetă (§2). Două formulări ale aceluiași lucru l-ar face pe operator să creadă
că s-au întâmplat două lucruri diferite.

## 2. Coșul gol = nicio întrebare (cererea 2)

`MainForm.DuLaIngestieAsync` întreabă ÎNAINTE de a deschide fereastra, prin **aceeași funcție**
pe care o folosește și formularul: `AsociereForm.NehotarateDin(instantanee, pozitie, ignorat)`,
nouă, `Friend Shared`. `NehotarateleCount` (instanța) o cheamă acum pe ea, deci există o
singură numărătoare. Două numărători scrise separat ar aluneca una față de alta, iar alunecarea
s-ar vedea ca o **salvare tăcută acolo unde omul trebuia întrebat** — exact ce nu-și poate
permite conducta asta.

Zero de așezat ▸ `SalveazaFaraMachetaAsync`: `DeciziiDin` peste ACELEAȘI trei dicționare goale
(«nicio mutare, niciun ignorat, nicio ștergere pusă de operator» e chiar starea formularului în
clipa în care se deschide), apoi `PrelucrareCoordinator.SalveazaAsync`. Nu e o scurtătură pe
lângă om: mulțimea de hotărât e goală, deci lista conține exact rândurile pe care serverul
însuși le-a așezat în faza întâi. Nu se inventează nicio alegere; se confirmă ce nu era de ales
(F18).

Drumul e complet: 409 ALEGERE_UNITATE trece tot prin coordonator, iar o renunțare la alegere
întoarce False și **nu** reîncarcă arborele.

## 3. Nodul rămâne selectat (cererile 1 și 2)

`LoadTreeAsync(Optional pastreazaSelectia As Boolean = False)` ▸
`PopulateTree(rows, Optional codSelectat As String = Nothing)`.

Codul se citește **înainte** de cerere: `PopulateTree` golește `_currentInfo`, deci după el nu
mai există de unde afla ce era selectat. Selecția se pune înapoi la sfârșit, cu tot ce atârnă
de ea — poarta vederilor (`ApplyViewGating`), contextul vederii active, fereastra de informații
—, adică exact ce face un clic pe nod (`Tree_NodeMouseUp`).

`SelectAndReveal`, nu `SelectedNode`: scrisă singură, selecția ar fi reală și INVIZIBILĂ, iar
operatorul ar vedea o listă întoarsă la primul rând.

Informația nodului se ia din bucla de construcție, **nu** prin `_treeInfos(codSelectat)`:
dicționarul e pe comparație exactă, iar potrivirea e fără litere mari/mici, deci o cheie care
diferă doar prin caz ar fi aruncat `KeyNotFoundException`.

`DeschideLegaturileReceptiilor` reîncarcă acum arborele cu nodul păstrat în loc să cheme
`ReceptiiView.Reincarca()`: o legătură mutată poate aprinde sau stinge steagurile `Are*` ale
nodului, iar reîncărcarea arborelui împinge singură contextul nou în vederea deschisă.
⚠ `ReceptiiView.Reincarca` a rămas în cod, dar **nu mai e chemată de nicăieri**; e păstrată
pentru simetrie cu `PlatiView` / `RezervariView`, care încă o folosesc.

## 4. Macheta de alegere a recepțiilor (cererea 3)

`src/KBot.App/Forexe/SelectieReceptiiForm.vb` + `.Designer.vb` — `KBotThemedForm` +
`KBotDataView` cu prima coloană `KBotColumnType.CheckBox`, toate controalele declarate în
designer (convenția casei). Coloane: bifă · Nr. · Data · Valoare · Instantanee · Stare.
Butoane: «Bifează tot» / «Debifează tot» / «Descarcă» / «Renunță».

**Lista e cea LOCALĂ, și nu poate fi altfel.** Întrebarea se pune ÎNAINTE de descărcare, deci
singurele recepții care se pot arăta sunt cele pe care K-BOT le are deja
(`GET /api/forexe/receptii`). O recepție care există în FOREXE dar nu și aici NU e în listă și
se descarcă întotdeauna — nu poți sări peste ceva ce nu știi că există. Scris pe ecran, în
antetul machetei.

**Recepția se numește prin DATA ei**, fiindcă exact așa o numește tot restul conductei:
`step4b_receptii_prelucrare` potrivește un rând de sarcină utilă cu o recepție stocată pe
`DATE(DataR) = %s AND Sters = 0` și ia **primul candidat**, iar felia 0058 o numește
operatorului tot prin dată și valoare. De aici singura ciudățenie a machetei: **două recepții
din aceeași zi nu se pot deosebi pe fir.** Dacă una e bifată și cealaltă nu, ziua se descarcă
ÎNTREAGĂ, iar rândul de jos spune asta cu voce tare (`ZileAmestecate`). Direcția în care se
greșește e aleasă: cealaltă ar sări peste o recepție pe care operatorul CHIAR a cerut-o, iar
asta n-ar apărea nicăieri — ar arăta exact ca o descărcare reușită.

**Parametrul de aplicație:** `FeatureSwitches.ReceptiiBifateLaDeschidere`, azi mereu `True`.
Comutatorul stă acolo unde stau toate celelalte, cu de ce-ul scris lângă el; când se decide de
unde vin cu adevărat, se schimbă implementarea proprietății, nu apelantul.

**Citirea listei care eșuează NU oprește descărcarea** (`AlegeReceptiileDeSaritAsync`): fără
listă nu se poate întreba, iar întrebarea e o economie de timp, nu o condiție — se descarcă tot
și se spune de ce pe consolă. Niciodată invers: o sărire tăcută după o citire eșuată ar fi exact
hotărârea pe care mașina nu are voie s-o ia. Un angajament fără nicio recepție locală nu
deschide nimic.

**Renunțarea la machetă e renunțarea la descărcare**, nu o descărcare implicită
(`Nothing` ▸ `Return`).

## 5. Selecția în workflow — și de ce nu e destulă singură

`{{RECEPTII_SARITE}}` = datele nebifate, `zz/ll/aaaa` separate prin virgulă. GOL = nu se sare
peste niciuna, adică **exact comportamentul dinainte**. Parametrul se trimite ÎNTOTDEAUNA, chiar
gol (`JobBuilder.PuneReceptiileSarite`): un placeholder nesubstituit ar ajunge la `IfVar` ca
text literal `{{RECEPTII_SARITE}}`, ceea ce se întâmplă să fie sigur (nu se potrivește cu nicio
dată, deci se descarcă tot) — dar din întâmplare.

În `.wfl`, în bucla recepțiilor:

```xml
<IfVar value="{{RECEPTII_SARITE}}" compare="regex:(^|,)[[R.Data]](,|$)">
  <Log .../>
  <SetInternalVar name="Detaliu" value="[]" />
  <SetInternalVar name="TipReceptie" value="" />
  <SetInternalVar name="DescriereReceptie" value="" />
  <SetInternalVar name="CodIndicator" value="" />
  <Else> …citirea detaliului, neatinsă… </Else>
</IfVar>
```

**Cele patru golirii sunt obligatorii, nu igienă.** `BuildCollectedRow` citește un câmp lipsă
din variabila cu ACELAȘI nume, iar `SetVariable` adaugă într-o listă din care se ia ULTIMA
valoare — deci fără golire, recepția sărită ar pleca spre server cu **`Detaliu`-ul vecinei ei**.
`"[]"` pentru `Detaliu`, nu `""`: serverul cere o listă acolo (`cere_lista`), iar un șir gol e
refuzat.

**Și tot nu e destul — de aceea taie și clientul.** `WorkflowResultStore.FaraReceptiileSarite`
scoate rândurile sărite din `ListaReceptii` înainte de trimitere. Workflow-ul sare peste
DETALIU (acolo sunt minutele), dar rândul fusese deja citit de `ScrapeTable` și ar pleca cu suma
proaspătă de pe site și cu detaliul gol; serverul ar actualiza atunci antetul
(`_R_UPDATE_SUMA_SQL`) fără să-i atingă liniile, adică ar lăsa în bază **o recepție a cărei sumă
nu mai dă suma liniilor ei**. Un rând SCOS nu e atins de nimeni: pasul 4b lucrează rând cu rând.

**Un rând a cărui dată NU se poate citi RĂMÂNE.** «Nu știu care e» nu are voie să devină «sigur
nu e bifată».

Tăierea se face ÎNAINTE de salvarea locală, ca fișierul din `WorkflowResults` să arate exact ce
pleacă spre server.

## 6. Cele două reîmprospătări parțiale (cererea 4)

Două fișiere `.wfl` NOI, compuse din secțiunile fișierului complet — aceleași selectoare,
aceiași pași, aceleași nume de tabele (D11: când se schimbă o secțiune comună, se schimbă în
toate fișierele):

| Fișier | Secțiuni | De ce |
|---|---|---|
| `adlop - Receptii Angajament.wfl` | 0 (antet) + 2 (recepții, cu selecție) | butonul din subsolul arborelui de recepții cere recepțiile |
| `adlop - Rezervari Angajament.wfl` | 0 + 1 (indicatori + buget) + 4 (istoric) | `FX_Rezervari` se scrie pe server **din `FX_Istoric`** (pașii 3c/3d), deci istoricul E sursa rezervărilor |

**Serverul acceptă pachete parțiale fără nicio modificare** — verificat citind
`routes/forexe/prelucrare.py`: fiecare tabel se citește cu `tabele.get(...) or []`, deci pasul
lui nu scrie nimic; iar pasul 4b are nevoie de indicatori **din BAZĂ** (`read_indicatori`), nu
din sarcina utilă. Nicio linie de Python atinsă în felia asta.

**Amândouă trec prin ACELEAȘI două faze** ca descărcarea unui nod întreg. Un flux parțial nu are
voie să ocolească asta. La recepții istoricul lipsește, deci de obicei nu rămâne nimic de
hotărât și salvarea se face singură (§2); la rezervări istoricul proaspăt POATE aduce
instantanee de așezat, și atunci macheta se deschide, ca după orice descărcare.

**Pachetele parțiale NU intră în memoria de reutilizare** (`WorkflowResultStore.SalveazaPartial`
în loc de `SalveazaNod`). Acolo stă răspunsul la «angajamentul ăsta a fost deja descărcat?», iar
`PachetBunDeRefolosit` l-ar oferi la următoarea apăsare a iconiței de nod ca pe o descărcare
ÎNTREAGĂ — o reîmprospătare doar pe recepții n-are nici indicatori, nici istoric, deci ar duce
ingestia să creadă că angajamentul nu mai are istoric deloc. Se scrie totuși pe disc, cu familia
în nume.

**Cele două butoane erau moarte.** `ReceptiiView` și `RezervariView` aveau iconița din dreapta
subsolului cu tot cu eticheta ei («Reîncarcă … de la server»), dar **niciun handler** — exact
defectul (1) din felia 0057, pe alt arbore. Acum au `Tree_FooterRightIconClicked` și un
`Action(Of String)` primit de la shell; fără el iconița **se stinge**, nu rămâne un buton care
nu face nimic.

---

## Fișiere atinse

**Nou**
- `src/KBot.App/Forexe/SelectieReceptiiForm.vb`, `.Designer.vb`
- `src/KBot.Forexe/Workflows/adlop - Receptii Angajament.wfl`
- `src/KBot.Forexe/Workflows/adlop - Rezervari Angajament.wfl`
- `docs/worklog/SLICE-0060-reimprospatare-ceruta.md` (acest fișier)

**Modificat**
- `src/KBot.Common/FeatureSwitches.vb` — `ReceptiiBifateLaDeschidere`
- `src/KBot.Common/KBot.Common.vbproj` — FileVersion 1.5.0.0 ▸ 1.5.1.0
- `src/KBot.Forexe/WorkflowCatalog.vb` — cele două fișiere noi, `VarReceptiiSarite`,
  `DataReceptieFormat`, `ListaDatelorSarite`, cele două liste de tabele
- `src/KBot.Forexe/JobBuilder.vb` — `receptiiSarite` pe cele două constructoare existente,
  `BuildReceptiiAngajament`, `BuildRezervariAngajament`, `PuneReceptiileSarite`
- `src/KBot.Forexe/KBot.Forexe.vbproj` — cele două `.wfl` noi în `CopyToOutputDirectory`;
  FileVersion 1.0.9.0 ▸ 1.0.10.0
- `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl` și `… Reverse.wfl` — garda
- `src/KBot.App/Forexe/AsociereForm.vb` — `SpuneSiInchide`, `NehotarateDin`,
  `TextDupaSalvare` ▸ Friend
- `src/KBot.App/Forexe/WorkflowResultStore.vb` — `SalveazaPartial`, `FaraReceptiileSarite`,
  `EDeSarit`
- `src/KBot.App/Forexe/ForexeController.vb` — `receptiiSarite` pe `DownloadNodeAsync`,
  `DownloadReceptiiAsync`, `DownloadRezervariAsync`, `DescarcaPartialAsync`
- `src/KBot.App/KBOT.vb` — `LoadTreeAsync`/`PopulateTree` cu selecția păstrată,
  `SalveazaFaraMachetaAsync`, `AlegeReceptiileDeSaritAsync`, `ReimprospateazaReceptii`,
  `ReimprospateazaRezervari`, `AratEsecul`, cele două vederi primesc callback-ul
- `src/KBot.App/Views/ReceptiiView.vb` / `.Designer.vb`,
  `src/KBot.App/Views/RezervariView.vb` / `.Designer.vb`

`src/KBot.App/Program.vb`, `src/KBot.App/KBot.App.vbproj`, `src/KBot.App/SingleInstance.vb`,
`My Project/*` și `Surse/SURSA_XFA_WRITTER/` erau deja modificate/neurmărite la începutul
sesiunii — **nu sunt ale acestei felii**.

## Rezultate

- `dotnet build KBot.sln` — **0 erori**, 7 avertismente `MSB3825` PREEXISTENTE (`.resx` /
  `BinaryFormatter`).
- XML-ul tuturor celor 8 `.wfl` — bine format (`ElementTree.parse`).

## Ce NU s-a făcut, și trebuie spus

- **Nicio suită de teste n-a fost rulată** (cerere explicită: «nu testa»). Nu există niciun test
  nou pentru `FaraReceptiileSarite`, `NehotarateDin`, `SelectieReceptiiForm.Grupeaza` /
  `ZileDeSarit` / `ZileAmestecate` — toate cinci sunt scrise anume ca funcții pure sau `Shared`,
  cu tabloul pe parametri, tocmai ca să se poată verifica fără să se deschidă o fereastră.
  **Sunt de scris.**
- **Nimic nu s-a comis** (cerere explicită: «nu git»).
- **NIMIC NU S-A VĂZUT PE ECRAN.** `SelectieReceptiiForm` n-a fost desenată nici măcar cu
  `DrawToBitmap`; lățimile de coloană, textul antetului și așezarea celor patru butoane sunt
  **presupuse**, nu verificate.
- **NICIUN WORKFLOW NOU N-A RULAT.** Cele două fișiere parțiale și garda `IfVar` n-au atins
  niciodată FOREXE. În particular sunt NEVERIFICATE: că `[[R.Data]]` sosește chiar în formatul
  `zz/ll/aaaa` pe care îl construiește `ListaDatelorSarite` (dedus din
  `fx_receptii_parse_ro_date`, care sparge după «/» și cere trei bucăți — dar nimeni n-a citit
  un `R.Data` real); că `SetInternalVar` chiar acoperă valoarea din iterația precedentă în
  `BuildCollectedRow`; și că secțiunile 1 și 2 pornesc corect una fără cealaltă (fiecare începe
  cu propriul `Click` pe tab, dar asta e citit din fișier, nu văzut pe site).
- **NICIO CERERE N-A AJUNS PE O BAZĂ VIE.** Că un pachet fără `TabelIstoric` (recepții) sau fără
  `ListaReceptii` (rezervări) trece prin ingestie e **dedus din codul rutei**
  (`tabele.get(...) or []`), nu observat. La fel salvarea cu listă de decizii GOALĂ: din
  `verifica_acoperirea` reiese că o mulțime goală e acoperită, dar n-a plecat niciodată una.
- **Reîmprospătarea rezervărilor citește istoricul ÎNAINTE, nu în REVERSE**, deci e mai lentă
  decât ar putea fi pe un angajament cu istoric lung. Deliberat: varianta REVERSE are nevoie de
  `DATA_IESIRE`, iar o dată lipsă ar face oprirea (`exitIfCellEquals`) să se potrivească cu
  PRIMUL rând — iar fluxul se poate cere și pe un angajament fără istoric local. O a doua
  variantă, ca perechea prelucrării complete, e **de făcut**.
- **Fișierele complete au fost MODIFICATE, nu duplicate.** Cererea spunea «un wfl nou care să
  suporte această nouă opțiune». Un al treilea și un al patrulea fișier de 14 KB care trebuie
  ținute în pas cu primele două e chiar felul în care se rupe D11; garda e inertă când variabila
  e goală, deci comportamentul de dinainte e neschimbat. Cele două fișiere NOI cerute la punctul
  4 există. **De confirmat cu operatorul** dacă voia totuși variante separate.
- `ReceptiiView.Reincarca` nu mai e chemată de nicăieri (vezi §3).
- **Două recepții în aceeași zi** nu se pot deosebi pe fir (§4). Se descarcă amândouă, și
  macheta spune asta — dar e o limită reală a identității «recepție = dată», nu un lucru de
  reparat aici.
