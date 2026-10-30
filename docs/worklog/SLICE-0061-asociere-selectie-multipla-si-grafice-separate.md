<a id="slice-0061"></a>

# SLICE 0061 — selecție multiplă în arbore, ecran de lucru curat, și locul dat de ceas

Cinci cereri ale operatorului din 10.09.2026, două pe control și trei pe `AsociereForm`:

1. în `AdvancedTreeControl`, **opțional**, să se poată alege mai multe noduri ale aceleiași rădăcini, cu Ctrl sau Shift, ca în Windows;
2. în coșul de neasociate, să se poată alege mai multe deodată și **să se tragă împreună** pe R-ul lor, iar **meniul să meargă pe toate**;
3. graficele (grafic + benzi) să plece într-un **formular complet separat**, redimensionabil, deschis cu un buton;
4. **fiecare arbore** să aibă sub el o grilă cu indicatorii lui;
5. la trecerea cu un H peste arborele recepțiilor, **R-ul să se aprindă întreg** și H-ul să-și ia, în el, **poziția dată de oră** — «nu am voie să o pun oriunde, deci asta e soluția».

---

## Ce s-a schimbat și de ce

### A. Grupul de rânduri (`KBot.Controls/Tree`)

`AdvancedTreeControl.MultiSelect.vb` — partială nouă. Implicit **stins**, deci cele nouă vederi care folosesc deja arborele nu capătă niciun comportament nou.

- **O singură rădăcină:** un grup aparține aceleiași recepții/coș; gazdele nu trebuie să despartă rânduri fără legătură. Ctrl/Shift pe altă rădăcină începe acolo un grup nou.
- **SelectedNode rămâne focusul**, inclus în grup și singurul cu chenar; restul au numai umplere. Setterul extern reduce grupul la acel rând, fără selecții nenumite rămase.
- **Click simplu în grup:** reduce selecția la MouseUp, fără a o pierde înaintea tragerii. `_pendingSingleSelect` amână; MaybeBeginDrag o anulează, OnMouseUp o aplică.
- Tastatura: Shift + săgeți/Home/End/PgUp/PgDn întind plaja de la ancoră; Ctrl+A ia toată rădăcina rândului curent, fără rândul-rădăcină. Amândouă inerte cu `MultiSelect` stins.
- API: `SelectedNodes` (în ordinea de pe ecran, niciodată `Nothing`), `SelectedNodeCount`, `IsNodeSelected`, `SelectNodes`, `ClearNodeSelection`, eveniment `SelectedNodesChanged`.
- **Drag compatibil:** TreeDragStartEventArgs.Items este lista modificabilă din care gazda poate elimina rânduri; TreeDragOverEventArgs/TreeDropEventArgs.Sources întoarce un element pentru drag simplu. Constructorii vechi rămân, cu gazdele/testele anterioare compatibile.
- Grupul folosește Shared `_draggedGroup`: bucla drag a sistemului este unică/modală pe proces, iar arborele țintă nu are câmpurile instanței sursă. Data object păstrează rândul apăsat.

### B. Unde ar cădea rândul (`AdvancedTreeControl.DropPreview.vb`)

Cererea (5), și e o cerință despre adevăr: în lanțul unei recepții locul unui instantaneu **nu se alege**, îl dă `DataH`. Chenarul de pe rândul de sub cursor spunea deci ceva fals.

`SetDropPreview(root, ghosts)`/`ClearDropPreview()`, cu TreeDropGhost(index, caption): rădăcină întreagă cu chenar punctat/văl DragHighlightColor și fantome pe pozițiile calculate de GAZDĂ în șirul REZULTAT. Arborele nu cunoaște DataH, ca la vetouri: desenează decizia gazdei.

Fantomele sunt `TreeItem` în `Children` (`IsDropGhost = True`), deci folosesc pictura, derularea și măsurarea obișnuite. `ClearDropPreview` le elimină din `CancelDrag` (aruncare, ESC, pierderea ferestrei) și `OnDragLeave`; `DragOver` pe fantomă este raportat pe părintele ei.

### C. `AsociereForm` — ecran de lucru, grafice în fereastra lor

- **Layout (4):** stânga recepții/lanțuri + grila lor, dreapta neașezate + grila lor. UmpleGrila(nod, tinta) ține indicatorii fiecăruia: grila unică ar fi șters comparația stângii la click dreapta. Coloane ToContent cu minime reduse pentru panouri înjumătățite.
- **Grafice/benzi (3):** pnlGrafice ascuns în card se mută întreg în GraficeAsociereForm la «Grafice și benzi» și revine la închidere; fereastră redimensionabilă KBotShellForm. Fără controale duplicate: tabloul/tratatorii existenți păstrează un singur set de culori/repere/drag, ca la AsociereBenziForm.
- **Drag/meniu pe grup (2):** Tree_NodeDragStarting elimină rădăcini/înghețate, fără refuzul întregului gest; TreeLant_NodeDragOver verifică fiecare și numește rândul refuzat. Rândurile deja pe țintă nu blochează celelalte. Comenzi comune: desprindere, (nu) consemnează schimbare, Începe o recepție nouă din toate N. Un singur IDRR cerut înaintea buclei: UrmatorulIdrrNou citește `_pozitie` și în buclă ar crea N recepții separate. Fără Este rândul de ștergere pe grup: ar declara mai multe sfârșituri ale lanțului.
- Meniul lucrează pe grup doar dacă grupul CONȚINE rândul pe care a căzut clicul; altfel e o selecție rămasă de la un gest de dinainte.

## Fișiere atinse

**KBot.Controls**
- `Tree/AdvancedTreeControl.MultiSelect.vb` — NOU (grupul, ancora, datoria de strângere)
- `Tree/AdvancedTreeControl.DropPreview.vb` — NOU (fantomele + rădăcina aprinsă, `TreeDropGhost`)
- `Tree/AdvancedTreeControl.Drag.vb` — grup în tragere (`_draggedGroup`, `ItemsDinDate`), `Items`/`Sources` pe cele trei EventArgs, `ClearDropPreview` din `CancelDrag`/`OnDragLeave`
- `Tree/AdvancedTreeControl.Overrides.vb` — `ApplyMouseSelection` / `SelectSingle` / `SettlePendingSelection` în locul scrierilor directe pe `pSelectedItem`; `DrawDropPreviewRoot`
- `Tree/AdvancedTreeControl.Keyboard.vb` — `ApplyKeySelection` după fiecare mutare, Ctrl+A
- `Tree/AdvancedTreeControl.Painting.vb` — `IsRowSelected` în cele trei locuri; chenar doar pe focus
- `Tree/AdvancedTreeControl.Properties.vb` — `SelectedNode` sincronizează grupul
- `Tree/AdvancedTreeControl.API.vb` — `Clear` uită grupul și previzualizarea
- `Tree/AdvancedTreeControl.TreeItem.vb` — `IsDropGhost`
- `Tree/AdvancedTreeControl.md` — două secțiuni noi + limitele lor

**KBot.App**
- `Forexe/GraficeAsociereForm.vb` + `.Designer.vb` — NOI
- `Forexe/AsociereForm.Designer.vb` — arbore+grilă pe fiecare parte, `pnlGrafice` ascuns, `btnGrafice` (4 coloane în `tlyAsociere`), `MultiSelect = True` pe amândoi arborii, `gridLant`/`gridLibere` pe `ToContent`, antetul fișierului rescris
- `Forexe/AsociereForm.vb` — `UmpleGrila(nod, tinta)`, doi tratatori de clic, grup în cele cinci metode de tragere, `AratMeniulGrupului` + `AplicaComandaPeGrup`, `AratLocul` / `AratLoculInCos` / `Fantomele`, `btnGrafice_Click`, `_nodLibere`
- `Forexe/AsociereForm.resx` — textul de sus spune de acum și despre Ctrl/Shift și despre locul dat de oră
- `Views/Ddf/DdfXmlBuilder.vb` — două «machetа» cu «а» CHIRILIC în comentarii, scrise cândva din greșeală; măturate (RULE 0)

## Rezultate

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** — cerere explicită: «no tests. no git».
- **Văzut pe ecran**, prin `DrawToBitmap`, dintr-o gazdă de unică folosință din scratchpad:
  1. grupul de trei rânduri ale aceleiași recepții — umplere pe toate, chenar doar pe cel cu focus;
  2. previzualizarea aruncării — R-ul aprins întreg, cu chenar punctat, și rândul-fantomă așezat între 02.02 și 04.02, adică pe locul dat de oră;
  3. arborele DUPĂ `ClearDropPreview` — identic la octet cu (1), deci fantomele nu lasă urmă;
  4. `AsociereForm` întreg în noua așezare (doi arbori, două grile, butonul nou);
  5. `GraficeAsociereForm` deschis, cu banda de nume și graficul pe toată fereastra;
  6. `AsociereForm` după închiderea ei — panoul întors acasă, așezarea neatinsă.

## Neverificat / de făcut

- ⚠ **Nimic nu s-a salvat pe o bază vie**, și niciun test n-a rulat. Testele existente de asociere cheamă prin reflexie `TreeLant_NodeDropped` / `TreeLibere_NodeDropped` cu `TreeDropEventArgs` construit de ele; forma aceea încă merge (`Sources` cade înapoi pe `Source`), dar **nimeni n-a rulat suita ca s-o confirme**.
- ⚠ **Zero teste noi** pentru: grupul (ancoră, Ctrl, Shift, o singură rădăcină, datoria de strângere), `Fantomele` (pură, cu tabloul pe parametri — cea mai ușor de acoperit), `InstantaneeleAlese`, `AplicaComandaPeGrup`.
- ⚠ **Gesturile n-au fost făcute cu mâna**: Ctrl/Shift, tragerea unui grup dintr-un arbore în celălalt și meniul pe grup au fost văzute doar prin API, nu cu mouse-ul pe ecran.
- ⚠ `KBotNavList` taie eticheta elementului «Distribuție» (aliniat Far) și în fereastra nouă, la lățime mare. E felul în care se desena și înainte, în panoul îngust — nu s-a atins nimic acolo, dar acum se vede pe o suprafață mare.
- ⚠ Fereastra graficelor se deschide **modal**. Cât e deschisă nu se poate trage în arbori. Dacă operatorul o vrea alături, deschisă în timp ce lucrează, e o schimbare mică (`Show(owner)` + reîmprospătare la fiecare `Reconstruieste`), dar nu s-a cerut așa.
- ⚠ Previzualizarea NU derulează arborele ca să aducă fantoma în dreptul ochiului: dacă lanțul e lung și locul cade sub marginea de jos, se vede doar rădăcina aprinsă.
---

<a id="slice-0061-02"></a>

# 0061-02 — urmarea din 10.09.2026: arborele în fereastra graficelor, și graficul rezervărilor

Doua cereri, amandoua despre acelasi lucru — un grafic fara arborele lui nu e de niciun folos:

6. în fereastra graficelor asocierii **să apară copacul cu R-uri**, ca să se poată alege pentru grafic; **la benzi nu e nevoie** de el;
7. în arborele **rezervărilor**, în antet, **un buton ca la recepții**, care să arate graficul rezervărilor.

## A. `GraficeAsociereForm` împrumută acum ȘI arborele

Modalul Grafice și benzi afișa recepția aleasă anterior; arborele pentru altă alegere rămânea inaccesibil dedesubt. Suprafața goală spunea „Alege o recepție în stânga”, fără arbore acolo.

- pnlCard primește splitGrafice: stânga treeLant, dreapta pnlGrafice. Ambele panouri sunt goale în Designer; controalele împrumutate revin în AsociereForm la închidere, graficul ascuns și arborele vizibil ca inițial.
- **Arbore unic:** se păstrează tratatorii, cele două dicționare și colorarea treeLant. Revin astfel culoarea punctului/rândului și selectarea rândului prin click pe punct, fără al doilea set de logică.
- **La benzi arborele se STRÂNGE** (`Panel1Collapsed`), nu se ascunde: strâns, benzile primesc toată fereastra; ascuns, ar fi rămas o coloană goală și o dungă de tras. Lățimea aleasă de operator nu se pierde — `SplitterDistance` rămâne ce era.
- AsociereForm.AplicaVedereaDinDreaptaSus, proprietarul navGrafice, notifică prin `_fereastraGrafice?.AratArborele(...)`. Finally golește referința chiar la eșec; starea inițială vine din AsociereForm.VedereaEsteBenzi, fără presupunere.

## B. `GraficRezervariForm` — graficul rezervărilor, din antetul arborelui

Slotul din dreapta antetului era liber în `RezervariView` (în `ReceptiiView` acolo stă editorul de legături). Acum ține iconița de grafic și deschide o fereastră redimensionabilă (`KBotShellForm`) cu două file:

- **Evoluția** — cât era rezervat, zi de zi: suma alergătoare a lui `R_Valoare`, în TREPTE (`LineMode.Step`), fiindcă o rezervare ține cât ține și sare când vine operația următoare; desenată în pantă, linia ar fi spus despre fiecare zi dintre ele o sumă care n-a existat.
- **Pe luni** — chiar totalurile scrise pe folderele de lună din arbore, luna cu luna.

Ambele file primesc aceleași rânduri ca arborele, o dată prin constructor, fără a doua cerere care ar putea schimba cifrele. Fără angajament și fără rezervări sunt refuzuri distincte explicate prin KBotMessage.

**Graficul e al ferestrei**, nu împrumutat ca la asociere: acolo suprafața exista deja în formularul de lucru, aici nu exista niciun grafic al rezervărilor de împrumutat.

⚠ **Lunile negative nu sunt roșii**, deși în arbore sunt: încercarea a fost retrasă deoarece PointColor colorează și segmentul următor, înroșind fals urcarea către luna următoare.

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
- **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** — cerere explicită: «fără teste, fără git».
- **Văzut pe ecran**, prin `DrawToBitmap`, dintr-o gazdă de unică folosință din scratchpad (un `DispatchProxy` peste `IApiClient` răspunde doar la `GetAsociereAsync`, cu un tablou născocit):
  1. fereastra graficelor cu arborele recepțiilor în stânga și graficul în dreapta;
  2. aceeași fereastră pe «Distribuție» — arborele strâns, benzile pe toată lățimea;
  3. `AsociereForm` după închiderea ferestrei — arborele întors acasă, cele două grile la locul lor;
  4. `GraficRezervariForm` pe amandoua filele.

## Neverificat / de făcut

- ⚠ **Niciun gest n-a fost făcut cu mâna**: nici apăsarea butonului «Grafice și benzi», nici trecerea grafic ↔ benzi din banda de nume, nici clicul pe iconița din antetul rezervărilor.
- ⚠ **Zero teste noi**, și cele două metode de construcție a graficului rezervărilor sunt tocmai genul care se acoperă ușor (rânduri pe parametri, rezultat numărabil).
- ⚠ **Nimic nu s-a citit de pe o bază vie**: rândurile de rezervări din gazdă sunt născocite, deci n-a fost verificat cum arată graficul pe un angajament cu sute de operații în aceeași zi.
- ⚠ Fereastra graficelor asocierii a rămas **MODALĂ** (nota din 0061 stă în picioare): cât e deschisă, în arborele împrumutat se poate ALEGE, dar nu se poate trage nimic în celălalt arbore — el a rămas dincolo, în formular.

## Reper de trasabilitate din registru

Registrul caracteriza grupul și `Fantomele` drept primele două funcții pure ușor de acoperit, alături de `InstantaneeleAlese` și `AplicaComandaPeGrup` în lista fără teste noi. Aceasta este evaluarea istorică a registrului, nu o suită scrisă/rulată acum.
