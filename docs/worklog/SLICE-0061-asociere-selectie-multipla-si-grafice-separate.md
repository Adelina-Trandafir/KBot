# SLICE 0061 — selecție multiplă în arbore, ecran de lucru curat, și locul dat de ceas

Cinci cereri ale operatorului din 10.09.2026, două pe control și trei pe `AsociereForm`:

1. în `AdvancedTreeControl`, **opțional**, să se poată alege mai multe noduri ale aceleiași
   rădăcini, cu Ctrl sau Shift, ca în Windows;
2. în coșul de neasociate, să se poată alege mai multe deodată și **să se tragă împreună**
   pe R-ul lor, iar **meniul să meargă pe toate**;
3. graficele (grafic + benzi) să plece într-un **formular complet separat**, redimensionabil,
   deschis cu un buton;
4. **fiecare arbore** să aibă sub el o grilă cu indicatorii lui;
5. la trecerea cu un H peste arborele recepțiilor, **R-ul să se aprindă întreg** și H-ul să-și
   ia, în el, **poziția dată de oră** — «nu am voie să o pun oriunde, deci asta e soluția».

---

## Ce s-a schimbat și de ce

### A. Grupul de rânduri (`KBot.Controls/Tree`)

`AdvancedTreeControl.MultiSelect.vb` — partială nouă. Implicit **stins**, deci cele nouă
vederi care folosesc deja arborele nu capătă niciun comportament nou.

- **O singură rădăcină.** Un grup nu trece peste două rădăcini. Arborele nu e o listă plată:
  rădăcinile lui sunt lucrurile însele (o recepție, coșul), iar un grup întins peste două ar
  fi un grup de rânduri care n-au nimic în comun — fiecare gazdă ar trebui să-l despartă
  înapoi înainte să facă ceva cu el. Ctrl/Shift pe rândul altei rădăcini **începe un grup
  nou acolo**.
- **`SelectedNode` rămâne ce era**: rândul cu focus, mereu parte din grup, singurul care
  primește CHENARUL selecției (restul iau doar umplerea). Scris din afară, strânge grupul la
  el — cine spune «rândul e ăsta» nu poate lăsa în urmă alte cinci rânduri nenumite.
- **Clicul simplu într-un grup îl strânge la ridicarea butonului, nu la apăsare.** Apăsarea
  pe un rând al grupului e ȘI felul în care începe tragerea întregului grup; strâns la
  apăsare, până se mișca mouse-ul ar fi rămas un singur rând. `_pendingSingleSelect` ține
  datoria, `MaybeBeginDrag` o șterge, `OnMouseUp` o plătește.
- Tastatura: Shift + săgeți/Home/End/PgUp/PgDn întind plaja de la ancoră; Ctrl+A ia toată
  rădăcina rândului curent, fără rândul-rădăcină. Amândouă inerte cu `MultiSelect` stins.
- API: `SelectedNodes` (în ordinea de pe ecran, niciodată `Nothing`), `SelectedNodeCount`,
  `IsNodeSelected`, `SelectNodes`, `ClearNodeSelection`, eveniment `SelectedNodesChanged`.
- Tragerea a devenit «unul sau mai multe» **fără să rupă nimic**: `TreeDragStartEventArgs`
  capătă `Items` (listă MODIFICABILĂ — gazda scoate din ea rândurile pe care nu le lasă să
  plece), iar `TreeDragOverEventArgs` / `TreeDropEventArgs` capătă `Sources`, care răspunde
  cu o listă de un element când s-a tras unul singur. **Constructorii vechi au rămas
  neatinși**, deci testele și gazdele de dinainte compilează și merg ca înainte.
- Grupul călătorește printr-un câmp `Shared` (`_draggedGroup`), nu prin obiectul de date:
  bucla de tragere a sistemului e modală și e una singură pe proces, iar arborele-ȚINTĂ (la
  o tragere între doi arbori) n-a pornit tragerea, deci câmpurile lui de instanță sunt goale.
  Obiectul de date a rămas ce era — rândul apăsat.

### B. Unde ar cădea rândul (`AdvancedTreeControl.DropPreview.vb`)

Cererea (5), și e o cerință despre adevăr: în lanțul unei recepții locul unui instantaneu
**nu se alege**, îl dă `DataH`. Chenarul de pe rândul de sub cursor spunea deci ceva fals.

`SetDropPreview(root, ghosts)` / `ClearDropPreview()`, cu `TreeDropGhost(index, caption)`.
Rădăcina se aprinde ÎNTREAGĂ (chenar punctat + văl subțire, în `DragHighlightColor`) și în ea
apar rânduri-fantomă pe pozițiile primite. **Pozițiile le calculează GAZDA**, în șirul
REZULTAT: ordinea aparține datelor, iar arborele nu știe nimic despre `DataH` — aceeași
împărțire ca la vetourile tragerii (arborele arată, gazda hotărăște).

Fantomele sunt `TreeItem`-uri adevărate (`IsDropGhost = True`) băgate în `Children`-ul
rădăcinii, ca să treacă prin aceeași pictură, aceeași derulare și aceeași măsurătoare ca
restul; o pictură separată «pe deasupra» s-ar fi despărțit de rânduri la prima schimbare de
înălțime sau de derulare. Ies toate pe un singur drum, `ClearDropPreview`, chemat din
`CancelDrag` (aruncare, ESC, pierderea ferestrei) și din `OnDragLeave`. Un `DragOver` care
cade pe o fantomă e raportat gazdei ca fiind pe părintele ei.

### C. `AsociereForm` — ecran de lucru, grafice în fereastra lor

- **Așezarea s-a rotit** (cererea 4): stânga = recepțiile cu lanțurile lor **+ grila lor**,
  dreapta = coșul de neașezate **+ grila lui**. `UmpleGrila(nod, tinta)` umple grila
  arborelui din care vine rândul; o singură grilă pentru amândoi ar fi însemnat că un clic în
  dreapta șterge indicatorii rândului ales în stânga — adică tocmai perechea pe care
  operatorul o compară. Coloanele au trecut pe `ToContent` cu minime mai mici: panourile sunt
  acum pe jumătate late.
- **Graficul și benzile** (cererea 3) stau în `pnlGrafice`, un panou ASCUNS al cardului, care
  se mută ÎNTREG în `GraficeAsociereForm` la apăsarea butonului «Grafice și benzi» și se
  întoarce acasă la închidere. Fereastra e redimensionabilă (`KBotShellForm`). **Nu are
  controale proprii, și nu din economie**: tabloul local trăiește într-un singur loc, iar
  tratatorii graficului și ai benzilor sunt scriși acolo, lângă el — o a doua pereche de
  suprafețe ar fi însemnat un al doilea set de culori, de repere și de reguli de tragere,
  care s-ar putea abate de la primul. Aceeași hotărâre ca la `AsociereBenziForm`.
- **Tragerea și meniul merg pe grup** (cererea 2). `Tree_NodeDragStarting` SCOATE din grup
  rândurile care nu se mută (rădăcini prinse din greșeală, legături înghețate) în loc să
  refuze tot gestul; `TreeLant_NodeDragOver` verifică vetourile pentru fiecare și, la refuz,
  spune DESPRE CARE e vorba; cele care stau deja pe recepția țintă se scot din socoteală, nu
  refuză mutarea celorlalte. Meniul de grup oferă doar comenzile care au același înțeles
  pentru toate: desprinderea, «(nu) consemnează o schimbare» și **«Începe o recepție nouă din
  toate N»** — un SINGUR IDRR pentru tot grupul, cerut o dată în afara buclei (`UrmatorulIdrrNou`
  citește `_pozitie`, deci chemat înăuntru ar fi făcut N recepții de câte unul). «Este rândul
  de ștergere» NU se oferă pe un grup: ștergerea e ultimul instantaneu al unui lanț, pusă pe
  cinci ar spune că lanțul are cinci sfârșituri.
- Meniul lucrează pe grup doar dacă grupul CONȚINE rândul pe care a căzut clicul; altfel e o
  selecție rămasă de la un gest de dinainte.

## Fișiere atinse

**KBot.Controls**
- `Tree/AdvancedTreeControl.MultiSelect.vb` — NOU (grupul, ancora, datoria de strângere)
- `Tree/AdvancedTreeControl.DropPreview.vb` — NOU (fantomele + rădăcina aprinsă, `TreeDropGhost`)
- `Tree/AdvancedTreeControl.Drag.vb` — grup în tragere (`_draggedGroup`, `ItemsDinDate`),
  `Items`/`Sources` pe cele trei EventArgs, `ClearDropPreview` din `CancelDrag`/`OnDragLeave`
- `Tree/AdvancedTreeControl.Overrides.vb` — `ApplyMouseSelection` / `SelectSingle` /
  `SettlePendingSelection` în locul scrierilor directe pe `pSelectedItem`; `DrawDropPreviewRoot`
- `Tree/AdvancedTreeControl.Keyboard.vb` — `ApplyKeySelection` după fiecare mutare, Ctrl+A
- `Tree/AdvancedTreeControl.Painting.vb` — `IsRowSelected` în cele trei locuri; chenar doar pe focus
- `Tree/AdvancedTreeControl.Properties.vb` — `SelectedNode` sincronizează grupul
- `Tree/AdvancedTreeControl.API.vb` — `Clear` uită grupul și previzualizarea
- `Tree/AdvancedTreeControl.TreeItem.vb` — `IsDropGhost`
- `Tree/AdvancedTreeControl.md` — două secțiuni noi + limitele lor

**KBot.App**
- `Forexe/GraficeAsociereForm.vb` + `.Designer.vb` — NOI
- `Forexe/AsociereForm.Designer.vb` — arbore+grilă pe fiecare parte, `pnlGrafice` ascuns,
  `btnGrafice` (4 coloane în `tlyAsociere`), `MultiSelect = True` pe amândoi arborii,
  `gridLant`/`gridLibere` pe `ToContent`, antetul fișierului rescris
- `Forexe/AsociereForm.vb` — `UmpleGrila(nod, tinta)`, doi tratatori de clic, grup în cele
  cinci metode de tragere, `AratMeniulGrupului` + `AplicaComandaPeGrup`, `AratLocul` /
  `AratLoculInCos` / `Fantomele`, `btnGrafice_Click`, `_nodLibere`
- `Forexe/AsociereForm.resx` — textul de sus spune de acum și despre Ctrl/Shift și despre
  locul dat de oră
- `Views/Ddf/DdfXmlBuilder.vb` — două «machetа» cu «а» CHIRILIC în comentarii, scrise cândva
  din greșeală; măturate (RULE 0)

## Rezultate

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** — cerere explicită: «no tests. no git».
- **Văzut pe ecran**, prin `DrawToBitmap`, dintr-o gazdă de unică folosință din scratchpad:
  1. grupul de trei rânduri ale aceleiași recepții — umplere pe toate, chenar doar pe cel cu
     focus;
  2. previzualizarea aruncării — R-ul aprins întreg, cu chenar punctat, și rândul-fantomă
     așezat între 02.02 și 04.02, adică pe locul dat de oră;
  3. arborele DUPĂ `ClearDropPreview` — identic la octet cu (1), deci fantomele nu lasă urmă;
  4. `AsociereForm` întreg în noua așezare (doi arbori, două grile, butonul nou);
  5. `GraficeAsociereForm` deschis, cu banda de nume și graficul pe toată fereastra;
  6. `AsociereForm` după închiderea ei — panoul întors acasă, așezarea neatinsă.

## Neverificat / de făcut

- ⚠ **Nimic nu s-a salvat pe o bază vie**, și niciun test n-a rulat. Testele existente de
  asociere cheamă prin reflexie `TreeLant_NodeDropped` / `TreeLibere_NodeDropped` cu
  `TreeDropEventArgs` construit de ele; forma aceea încă merge (`Sources` cade înapoi pe
  `Source`), dar **nimeni n-a rulat suita ca s-o confirme**.
- ⚠ **Zero teste noi** pentru: grupul (ancoră, Ctrl, Shift, o singură rădăcină, datoria de
  strângere), `Fantomele` (pură, cu tabloul pe parametri — cea mai ușor de acoperit),
  `InstantaneeleAlese`, `AplicaComandaPeGrup`.
- ⚠ **Gesturile n-au fost făcute cu mâna**: Ctrl/Shift, tragerea unui grup dintr-un arbore în
  celălalt și meniul pe grup au fost văzute doar prin API, nu cu mouse-ul pe ecran.
- ⚠ `KBotNavList` taie eticheta elementului «Distribuție» (aliniat Far) și în fereastra nouă,
  la lățime mare. E felul în care se desena și înainte, în panoul îngust — nu s-a atins nimic
  acolo, dar acum se vede pe o suprafață mare.
- ⚠ Fereastra graficelor se deschide **modal**. Cât e deschisă nu se poate trage în arbori.
  Dacă operatorul o vrea alături, deschisă în timp ce lucrează, e o schimbare mică
  (`Show(owner)` + reîmprospătare la fiecare `Reconstruieste`), dar nu s-a cerut așa.
- ⚠ Previzualizarea NU derulează arborele ca să aducă fantoma în dreptul ochiului: dacă lanțul
  e lung și locul cade sub marginea de jos, se vede doar rădăcina aprinsă.
---

# 0061-02 — urmarea din 10.09.2026: arborele în fereastra graficelor, și graficul rezervărilor

Doua cereri, amandoua despre acelasi lucru — un grafic fara arborele lui nu e de niciun folos:

6. în fereastra graficelor asocierii **să apară copacul cu R-uri**, ca să se poată alege pentru
   grafic; **la benzi nu e nevoie** de el;
7. în arborele **rezervărilor**, în antet, **un buton ca la recepții**, care să arate graficul
   rezervărilor.

## A. `GraficeAsociereForm` împrumută acum ȘI arborele

Fereastra deschisă de « Grafice și benzi » avea graficul, dar graficul se desface pe recepția
ALEASĂ — iar alegerea se face în arbore, care rămăsese în formularul de dedesubt, sub o
fereastră modală. Se vedea deci ce fusese ales înainte de apăsarea butonului, și atât; scria
chiar pe suprafața goală «Alege o recepție în stânga», și nu exista nicio stânga.

- `pnlCard` are acum un `SplitContainer` (`splitGrafice`): **stânga = `treeLant`, dreapta =
  `pnlGrafice`**. Amandoua panourile sunt GOALE în designer — amandoua controalele vin
  împrumutate de la `AsociereForm` la deschidere și se întorc acasă la închidere, panoul ascuns
  (cum era) și arborele la vedere (cum a plecat).
- **Nu s-a făcut un al doilea arbore.** Tratatorii, cele două dicționare de rânduri și pasul de
  culori sunt scrise pentru `treeLant`; o copie ar fi însemnat un al doilea set de toate. Împreună
  cu arborele se întorc și cele două legături care nu aveau unde să lucreze: **culoarea** (fiecare
  punct poartă culoarea rândului lui) și **clicul pe punct**, care selectează rândul.
- **La benzi arborele se STRÂNGE** (`Panel1Collapsed`), nu se ascunde: strâns, benzile primesc
  toată fereastra; ascuns, ar fi rămas o coloană goală și o dungă de tras. Lățimea aleasă de
  operator nu se pierde — `SplitterDistance` rămâne ce era.
- Cine spune ferestrei că s-a schimbat vederea: `AsociereForm.AplicaVedereaDinDreaptaSus`, prin
  `_fereastraGrafice?.AratArborele(...)`. Banda de nume (`navGrafice`) e tot a formularului, deci
  tot el află primul; câmpul se șterge în `Finally`, ca o fereastră căzută să nu lase în urmă o
  referință la un formular aruncat. Starea de pornire o citește fereastra din
  `AsociereForm.VedereaEsteBenzi`, ca să n-o ghicească.

## B. `GraficRezervariForm` — graficul rezervărilor, din antetul arborelui

Slotul din dreapta antetului era liber în `RezervariView` (în `ReceptiiView` acolo stă editorul
de legături). Acum ține iconița de grafic și deschide o fereastră redimensionabilă
(`KBotShellForm`) cu două file:

- **Evoluția** — cât era rezervat, zi de zi: suma alergătoare a lui `R_Valoare`, în TREPTE
  (`LineMode.Step`), fiindcă o rezervare ține cât ține și sare când vine operația următoare;
  desenată în pantă, linia ar fi spus despre fiecare zi dintre ele o sumă care n-a existat.
- **Pe luni** — chiar totalurile scrise pe folderele de lună din arbore, luna cu luna.

Amandoua se desfac din ACELEAȘI rânduri pe care le arată arborele — nicio a treia cifră care să
nu se poată regăsi acolo. Rândurile intră O DATĂ, prin constructor: o a doua cerere de rețea ar fi
putut răspunde altceva decât scrie în arborele de sub fereastră. Fără angajament ales și fără
rezervări sunt două motive diferite de a nu deschide nimic, și se spun separat, prin
`KBotMessage`.

**Graficul e al ferestrei**, nu împrumutat ca la asociere: acolo suprafața exista deja în
formularul de lucru, aici nu exista niciun grafic al rezervărilor de împrumutat.

⚠ **Lunile negative NU se înroșesc**, deși rândul lor din arbore se înroșește. Încercat și scos:
`PointColor` vopsește și SEGMENTUL CARE PLEACĂ din punct, deci o lună negativă făcea roșie
urcarea către luna următoare — spunea roșu exact despre partea care creștea.

## Fișiere atinse

**KBot.App**
- `Views/GraficRezervariForm.vb` + `.Designer.vb` — NOI
- `Views/RezervariView.Designer.vb` — `HeaderRightIcon` + tooltipul lui
- `Views/RezervariView.vb` — `tree_HeaderRightIconClicked`
- `Forexe/GraficeAsociereForm.Designer.vb` — `splitGrafice`, antetul rescris
- `Forexe/GraficeAsociereForm.vb` — constructor cu arbore + acasă-arbore, `AratArborele`
- `Forexe/AsociereForm.vb` — `_fereastraGrafice`, `VedereaEsteBenzi`, `btnGrafice_Click`

## Rezultate

- `dotnet build src\KBot.App\KBot.App.vbproj` — **0 erori, 0 avertismente**.
- **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** — cerere explicită: «fără teste, fără
  git».
- **Văzut pe ecran**, prin `DrawToBitmap`, dintr-o gazdă de unică folosință din scratchpad (un
  `DispatchProxy` peste `IApiClient` răspunde doar la `GetAsociereAsync`, cu un tablou născocit):
  1. fereastra graficelor cu arborele recepțiilor în stânga și graficul în dreapta;
  2. aceeași fereastră pe «Distribuție» — arborele strâns, benzile pe toată lățimea;
  3. `AsociereForm` după închiderea ferestrei — arborele întors acasă, cele două grile la locul lor;
  4. `GraficRezervariForm` pe amandoua filele.

## Neverificat / de făcut

- ⚠ **Niciun gest n-a fost făcut cu mâna**: nici apăsarea butonului «Grafice și benzi», nici
  trecerea grafic ↔ benzi din banda de nume, nici clicul pe iconița din antetul rezervărilor.
- ⚠ **Zero teste noi**, și cele două metode de construcție a graficului rezervărilor sunt tocmai
  genul care se acoperă ușor (rânduri pe parametri, rezultat numărabil).
- ⚠ **Nimic nu s-a citit de pe o bază vie**: rândurile de rezervări din gazdă sunt născocite, deci
  n-a fost verificat cum arată graficul pe un angajament cu sute de operații în aceeași zi.
- ⚠ Fereastra graficelor asocierii a rămas **MODALĂ** (nota din 0061 stă în picioare): cât e
  deschisă, în arborele împrumutat se poate ALEGE, dar nu se poate trage nimic în celălalt
  arbore — el a rămas dincolo, în formular.
