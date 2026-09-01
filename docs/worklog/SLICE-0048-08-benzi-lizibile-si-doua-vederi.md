# SLICE-0048-08 — benzile citibile, eticheta care nu mai rămâne în urmă, și cele două vederi din dreapta sus

Continuarea directă a feliei `SLICE-0048-07`. Cerută punct cu punct de operator pe 01.09.2026,
după ce a privit prima oară benzile de așezare.

**Stare:** cod verde — `dotnet build KBot.sln` **0 erori**, cele **6 `MSB3825` preexistente** pe
`AsociereForm.resx` / `Views/*.resx` (`ImageStream` prin `BinaryFormatter`) fiind singurele
avertismente. `KBot.Controls.Tests` **953 trecute / 0 picate** (exact linia de plecare).
`KBot.App.Tests` **185 trecute / 13 picate** — aceleași 13 roșii **preexistente și străine** de
felia asta (`DdfViewTests` 3, `DdfXfaParserTests` 2, `IstoricViewTests` 6, `MainFormNavItemsTests` 1,
`XfaXmlPreviewTests` 1). **Fără teste noi.** Python neatins.

**S-A VĂZUT PE ECRAN** — prima felie din familia 0048 despre care se poate spune asta.
`KBotLaneView` a fost randat cu `DrawToBitmap` pe toate trei schemele (Classic / Modern / Dark), iar
`AsociereForm` a fost randat întreg, în amândouă vederile din dreapta sus, cu un tablou de probă
(4 recepții × 4 instantanee + 3 orfani + 2 plăți). Sondele de randare au fost **temporare și au fost
șterse** — nu a rămas niciun test nou în suită. Ce **NU** s-a văzut: designerul Visual Studio, un
server viu, o tragere adevărată cu mouse-ul.

**Nimic nu a rulat pe MariaDB.** Nicio rută Python atinsă, nicio schemă schimbată.

---

## 1. Ce a cerut operatorul, și ce s-a livrat pentru fiecare

### 1.1 «Fereastra mare a benzilor: NU arăta nici măcar valoarea»

`AsociereBenziForm` punea `MarkerLabelsVisible = True`, deci scria suma lângă fiecare marcaj.
Linia a ieșit din `.Designer.vb` (implicitul e False, deci nu se mai scrie deloc).

**De ce e corect, nu doar cerut.** Suma scrisă lângă un marcaj se întinde spre dreapta peste
BANDA DE DEDESUBT, exact pe bucata în care ochiul compară două lanțuri — adică singurul lucru
pentru care există suprafața. Întrebarea la care răspunde banda e UNDE stă un instantaneu; cât
valorează o spun arborele, graficul și eticheta plutitoare, fiecare într-un loc unde nu acoperă
nimic. Motivul e scris în doc-ul de clasă al lui `AsociereBenziForm`, nu în `.Designer.vb`
(convenția §7: fără comentarii noi acolo).

### 1.2 «Arată în culori diferite de unde pleacă până unde se termină și începe altul — sau sunt ultimul»

Proprietate nouă pe `KBotLaneView`: **`SegmentedRail`** (implicit **True**) + `SegmentWidth`
(0 = grosimea lui `LaneLineWidth`). Fiecare marcaj pictează bucata de bandă pe care o
**stăpânește**: de la el până la următorul, iar ultimul până la capătul din dreapta, în culoarea
lui.

**E o afirmație despre date, nu o podoabă** — aceeași pe care graficul o desenează de la felia
0048-07 ca linie în trepte: ce consemnează un instantaneu ține până îl schimbă următorul. O bandă
plată spunea contrariul, că lanțul e un singur lucru nedespărțit, și lăsa «de unde până unde ține
ăsta» pe seama socotelii din poziția marcajelor.

Trei hotărâri luate la fața locului:
- **Banda simplă rămâne dedesubt, pe toată lățimea.** O bandă fără niciun marcaj trebuie să se
  vadă în continuare ca loc unde se poate arunca, iar bucata dinaintea primului marcaj trebuie să
  se citească drept GOALĂ, nu drept lipsă.
- **Bucățile se ordonează după X, nu după ordinea din colecție.** Colecția e nesortată prin
  contract (scrie în `KBotLaneView.md`), iar o bucată desenată spre un marcaj aflat la STÂNGA ei ar
  merge înapoi peste cea dinainte.
- **Un marcaj `Loose` nu pictează nimic.** Nu stă pe nimic, deci nu stăpânește nimic; o bucată
  între doi orfani ar desena un lanț tocmai acolo unde nu există niciunul. Se vede în captura de
  ecran: banda «Neașezate» are romburi și nimic între ele.

**Culorile diferite** vin din `AsociereForm`: fiecare marcaj primește `AutoColor(j)` după POZIȚIA
lui în lanț. Nu e o culoare inventată — e ACEEAȘI pe care graficul o dă punctului al j-lea al unui
lanț pe fila «Recepția», deci pasul de culori rescrie exact aceleași valori acolo, iar pe fila
«Tot angajamentul» — unde graficul n-are nicio părere despre instantanee — marcajele rămân totuși
deosebite între ele. Ca urmare, `AplicaCulorileBenzii` nu mai golește marcajele la `Color.Empty`
(un marcaj fără culoare și-ar pierde bucata în banda de dedesubt) ci le întoarce la culoarea
poziției; primește pentru asta suprafața ca prim parametru.

### 1.3 «Mai mult loc în dreapta pentru ultimul»

Proprietate nouă: **`TrailingSpace`** (px logici, implicit 0). Fără ea, ultimul marcaj cade exact
pe marginea din dreapta și bucata lui are lățime zero — **singura bucată încă deschisă e tocmai cea
care dispare**.

Locul se ia din **AXA TIMPULUI, nu din suprafață**: `MomentToX` împarte intervalul pe
`_plotRect.Width - trailing`, deci marcajele și reperele își păstrează datele relative, iar
etichetele de axă se mută cu ele (altfel data din dreapta ar fi stat sub loc gol, numind un moment
la care nu stă nimic). Mărginit la un sfert din suprafață: un `TrailingSpace` destul de mare cât să
strivească axa a încetat să mai fie loc pentru ultima bucată.

Pus în designer: `benzi.TrailingSpace = 50`, `benziMari.TrailingSpace = 60`.

### 1.4 «Când pleci de pe un punct, eticheta trebuie să se reseteze»

**Defectul era în `KBotToolTip`, nu în benzi**, și lovea la fel graficul, grila și arborele.

`Schedule` avea garda «aceeași țintă, aceeași etichetă deja pe ecran: n-o mai clipim», scrisă ca
`ReferenceEquals(_continutAsteptat, content)`. Dar fiecare control desenat de noi are **UN SINGUR**
obiect `KBotToolTipContent`, pe care îl REscrie înainte de fiecare cerere. Deci referința e mereu
aceeași, garda răspundea «e deja pe ecran» pentru orice al doilea lucru arătat de acel control, iar
eticheta rămânea înțepenită pe primul până când cursorul pleca de pe **tot controlul** — exact
purtarea pe care a descris-o operatorul («trebuie să ies din rândul recepției»).

Reparat la rădăcină, o dată pentru toate cele patru controale:
- garda compară acum și **AMPRENTA** textului (antet ␁ corp ␁ subsol), nu doar referința;
- când eticheta e **deja deschisă**, schimbarea se face **imediat**, sărind întârzierea inițială —
  o a doua întârziere ar lăsa numele unui lucru stând peste altul o jumătate de secundă, adică
  singura minciună pe care o etichetă nu are voie s-o spună;
- afișarea propriu-zisă s-a strâns într-un singur loc, `ShowScheduled`, chemat și din tick-ul
  întârzierii, și din schimbarea la cald. Verificarea «cursorul mai e pe control?» rămâne pe
  amândouă drumurile.

`HideNow` curăță și amprentele, altfel prima etichetă de după o stingere ar fi fost înghițită.

### 1.5 «Nu folosi culori prea deschise pe Clasic / Modern»

`KBotAutoPalette.ColorAt` mergea pe HSL: hue-ul se plimba cu fracția de aur, iar lightness-ul
alterna în trei trepte. Problema e că **lightness-ul HSL nu spune cât de tare se vede ceva**: la
una și aceeași valoare, un galben și un albastru sunt la doi pași de contrast unul de altul. Pe
schemele deschise, o parte din set ieșea de-a dreptul de negăsit — iar culoarea e singurul lucru
care leagă un marcaj de rândul lui, deci o culoare pierdută e o legătură pierdută.

Adăugat `Readable(...)`: fiecare culoare e împinsă pe **propriul** lightness (în pași de 0.04, cel
mult 12, mărginit la 0.16..0.88) până trece de **3:1** față de fundalul schemei — pragul WCAG
pentru grafică. **Hue-ul nu se mișcă niciodată**, fiindcă hue-ul e ce deosebește două serii între
ele; saturația la fel. O culoare care tot nu ajunge la prag rămâne unde a ajuns: un set împins până
la negru ar strica exact deosebirea pentru care există.

Se aplică la AMÂNDOUĂ suprafețele deodată, fiindcă modulul e comun de la 0048-07.

### 1.6 «Graficul și benzile: fiecare cu vederea lui, `navGrafice` alege, implicit graficul»

Operatorul pusese deja `navGrafice` (un `KBotNavList` cu «Grafic» / «Distribuție») în designer, dar
cele două suprafețe stăteau tot într-un `splitSus` orizontal, împărțite pe verticală.

`splitSus` a fost **scos cu totul**. `splitDreapta.Panel1` ține acum, în ordinea inversă de andocare
cerută de regula casei (Fill întâi, Top ultimul): `benzi` (Fill), `grafic` (Fill), `navGrafice`
(Top). `benzi.Visible = False` în designer, `navGrafice.SelectedKey = "grafic"`.

**De ce una peste alta și nu una lângă alta.** Împărțite, amândouă primeau vreo 150 de pixeli: prea
puțin pentru un grafic ca să i se citească scara, prea puțin pentru benzi ca să încapă mai mult de
câteva rânduri fără derulare. Sunt oricum două întrebări puse pe rând — «cum a evoluat» și «unde
stă» — deci fiecare ia tot locul cât e întrebată.

**Nu se reconstruiește nimic la comutare.** Amândouă suprafețele sunt ținute la zi de
`Reconstruieste` chiar și cât sunt ascunse, deci comutarea e o schimbare de vizibilitate și atât.
Vederea implicită e scrisă **și în constructor**, nu doar în designer: cheia din designer trece prin
`EndInit`, care în procesul Visual Studio nu o aplică deloc (`KBotNavList` sare validarea acolo,
deliberat), deci pe drumul ăla singura garanție ar fi fost `benzi.Visible = False`, adică o valoare,
nu o alegere.

Fiindcă `benzi` a moștenit tot panoul, a primit și mărimile care încap în el:
`LaneCaptionsVisible = True`, `LaneCaptionWidth = 150`, `LaneHeight = 18`, `LaneSpacing = 3`,
`MarkerSize = 9`, `AxisVisible = True`, `SegmentWidth = 4`. **`MarkerLabelsVisible` rămâne False și
aici** — regula de la §1.1 nu e despre fereastra mare, e despre ce e o bandă.

### 1.7 «`KBotLane` și `KBotChart` să expună și bordercolor și borderwidth»

`BorderColor` exista deja pe amândouă controalele; lipsea **`BorderWidth`**, adăugat acum pe
`KBotLaneView` și pe `KBotChartView` (px logici, implicit 1, `DefaultValue` deci nu se serializează
degeaba, `0` = fără chenar, la fel ca `BorderVisible = False`).

Două lucruri prinse pe drum, amândouă reparate:
- **`KBotChartView.BorderColor` nu reconstruia penița.** Setterul doar invalida, iar penița e
  CACHED — deci o culoare scrisă din cod nu se vedea până la următoarea schimbare de temă. Acum
  cheamă `RebuildThemeResources()`, ca omologul lui de pe benzi.
- **Grosimea trebuie recalculată când apare fereastra.** Înainte de handle, `DeviceDpi` răspunde 96
  oricare ar fi ecranul, deci penița construită mai devreme poartă grosimea greșită. `OnHandleCreated`
  și `OnDpiChangedAfterParent` reconstruiesc acum peniile pe amândouă controalele.

---

## 2. Fișiere atinse

**Modificate**
- `src/KBot.Controls/ToolTip/KBotToolTip.vb` — amprenta textului, `ShowScheduled`, schimbarea la
  cald; `KBotToolTip.md`
- `src/KBot.Controls/Chart/KBotAutoPalette.vb` — `Readable` + luminanța relativă WCAG
- `src/KBot.Controls/Chart/KBotChartView.vb` — `BorderWidth`, `BorderColor` reconstruiește penița,
  penițe rezidite la handle/DPI; `KBotChartView.Painting.vb` (`BorderWidth = 0`);
  `KBotChartView.md`
- `src/KBot.Controls/Lane/KBotLaneView.vb` — `SegmentedRail`, `SegmentWidth`, `TrailingSpace`,
  `BorderWidth`, penițe rezidite la handle/DPI; `KBotLaneView.Painting.vb` (`AxisRun`,
  `DrawLaneSegments`, axa pe intervalul timpului); `KBotLaneView.md`
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.38.0.0 ▸ **1.39.0.0**
- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `splitSus` scos, cele trei controale în
  `splitDreapta.Panel1`, `benzi` la mărimea panoului, `navGrafice.SelectedKey = "grafic"`
- `src/KBot.App/Forexe/AsociereForm.vb` — `NavGrafice_SelectionChanged` +
  `AplicaVedereaDinDreaptaSus`, culoarea de poziție pe marcaje, `AplicaCulorileBenzii` primește
  suprafața
- `src/KBot.App/Forexe/AsociereBenziForm.Designer.vb` — fără sume, `SegmentWidth`, `TrailingSpace`;
  `AsociereBenziForm.vb` (doc-ul de clasă)
- `src/KBot.App/KBot.App.vbproj` — `FileVersion` 1.0.22.0 ▸ **1.0.23.0**
- `docs/worklog/KBOT_STATUS.md`, fișierul ăsta

`AssemblyVersion` rămâne `1.0.0.0` peste tot.

**Fără fișiere noi.** **Fără teste noi** (regula permanentă). **Nimic Python.**

---

## 3. Ce s-a văzut pe ecran, și cum

Două sonde temporare, șterse după ce au răspuns:
1. `KBotLaneView` singur, 900×320, cu 5 benzi × 4..8 marcaje, o bandă de orfani sub separator și
   două repere de plată, randat prin `DrawToBitmap` pe Classic, Modern și Dark. Confirmat: bucățile
   colorate, orfanii fără bucăți, spațiul din dreapta, datele de pe axă la capetele intervalului
   TIMPULUI (nu ale dreptunghiului), semnele de capăt, niciun ton spălăcit pe schemele deschise.
2. `AsociereForm` întreg, 1400×900, cu un `AsociereFakeApi` (4 recepții × 4 instantanee, 3 orfani,
   2 plăți), randat în amândouă vederile prin `navGrafice.SelectedKey`. Confirmat: banda de
   navigare sus, graficul implicit, «Distribuție» care aduce benzile pe tot panoul, denumirile
   benzilor, reperul plății traversând toate benzile.

Prima sondă a scos și un defect al MĂSURĂTORII, nu al codului: `Form.DrawToBitmap` peste
`ClientSize` desenează și rama și bara de titlu, deci partea de jos a controlului cădea în afara
imaginii și axa părea că lipsește. Randarea CONTROLULUI, nu a formularului, e drumul corect.

---

## 4. Ce NU s-a făcut, și de ce

- **Reperele din afara intervalului tot nu se desenează.** O plată căzută în `TrailingSpace` (adică
  la puțin după ultimul instantaneu) e nedesenată, ca înainte. Regula «un reper nu întinde axa
  timpului» e din 0048-07 și e deliberată; se vede în captură, unde plata din 18.02 nu apare fiindcă
  ultimul instantaneu e din 14.02. **De hotărât de operator** dacă spațiul din dreapta ar trebui să
  primească și reperele care cad în el — argumentul PENTRU e că exact acolo întrebarea «a căzut
  instantaneul înainte sau după plată» e cea mai ascuțită.
- **Alinierea la pixel bandă ▸ grafic** rămâne cum a lăsat-o 0048-07 (adevărată doar când amândouă
  arată același interval), și acum contează mai puțin: cele două nu mai sunt niciodată pe ecran în
  același timp.
- **Nicio schimbare pe server, în fundament sau în reguli.** Felia asta e numai despre ce se vede.
- **Tragerea nu a fost încercată cu mouse-ul.** Codul ei e neatins de felia asta, dar niciun gest
  adevărat n-a trecut prin el, nici acum, nici în 0048-07.

## 5. Rămâne neverificat

- **Designerul Visual Studio.** Nici `AsociereForm`, nici `AsociereBenziForm`, nici `KBotLaneView`
  n-au fost deschise acolo. `benzi.Visible = False` scris în designer face suprafața invizibilă și
  pe suprafața de proiectare — de știut înainte de a o căuta.
- **Scara reală.** Toate capturile sunt cu 4-5 benzi. Cazul numit de operator — 20 de benzi × 20 de
  marcaje — n-a fost desenat niciodată, iar acolo `LaneHeight = 18` cere derulare.
- **150% DPI.** Peniile se reconstruiesc acum la handle și la schimbarea de DPI, dar asta e
  argumentat, nu măsurat pe un ecran adevărat.
- **Ruta și clientul nu au rulat viu**, ca toată familia 0048.
