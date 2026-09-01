# SLICE-0048-07 — benzile de așezare, liniile în trepte, reperele plăților și retragerea lui F13

Continuarea directă a feliilor `SLICE-0048-04` … `SLICE-0048-06` (editorul de legături R ▸ H și
graficul lui). Plan: `docs/PLAN_AsociereBenzi.md`, scris de operator pe 31.08.2026.

**Stare:** cod verde — soluția întreagă build **0 erori**; `KBot.Controls` **0 erori / 0
avertismente**. Singurele avertismente pe soluție sunt cele **6 `MSB3825` preexistente** de pe
`AsociereForm.resx` (`Il_Receptii.ImageStream` prin `BinaryFormatter`), care apar doar când se
regenerează `.resx`-ul — aceleași pe care le consemna și worklogul feliei 0048-06.
`KBot.Controls.Tests` **953 trecute / 0 picate**, exact linia de plecare. `KBot.App.Tests`
**13 picate / 185 trecute**, exact linia de plecare (vezi §7). Python **465 trecute / 15 sărite**.

**NIMIC NU S-A VĂZUT PE ECRAN.** `AsociereForm` nu a fost niciodată randat, iar `KBotLaneView` și
`AsociereBenziForm` sunt scrise în felia asta și nu au fost deschise nici măcar în designerul
Visual Studio. Testarea și verificarea vizuală sunt ale operatorului, pe mașina lui, după publicare.

**Nimic nu a rulat pe MariaDB.** Fără migrare, fără schimbare de schemă, fără apel viu.

**Fără git.** Nimic nu s-a comis și nimic nu s-a împins — cerut explicit în plan. Worklogul rămâne
în arborele de lucru.

---

## 0. Ce s-a citit înainte de orice

`CODE_WORKFLOW.md` · `KBOT_STATUS.md` (toată familia 0048) · `docs/FUNDAMENT_Asociere_Receptii.md`
(părțile 1, 2, 4 integral) · worklogurile `0048-04`, `0048-05`, `0048-06` ·
`src/KBot.App/Forexe/AsociereForm.{vb,Designer.vb}` · toată familia `src/KBot.Controls/Chart/` +
`KBotChartView.md` · `src/KBot.Controls/Tree/AdvancedTreeControl.Drag.vb` ·
`src/KBot.Controls/CONTROLS.md` (C1..C9) · `docs/kbot-forms-ui-convention.md` ·
`PYTHON/routes/forexe/asociere.py` și `prelucrare_asociere.py` · `src/KBot.Domain/AsociereStare.vb` ·
`src/KBot.Api/ApiClient.vb` (partea de asociere) · `src/KBot.Theming/ThemeShapes.vb` și
`ThemePalette.vb`.

---

## 1. De ce există felia — premisa care s-a rupt

Operatorul a corectat pe 31.08.2026 un fapt pe care se sprijinea tot formularul:

> `FX_Receptii_R.DataR` **nu** e momentul creării. E un câmp obișnuit, pe care omul îl tastează pe
> site, și se poate schimba după creare.

Verificat în schemă: `FX_Receptii_R` e
`IDRR, NRCRT, CodAngajament, Tip, DataR, SumaAntet, Descriere, HASH, TipReceptie, Incarcat, Preluat`
(`MariaDB_Schema/000_DEMO.sql` și `FX_System_Export/TABLES/FX_Receptii_R.md`). **Nu există nicio
coloană cu momentul creării în tabelul ăla.** Rândurile din eșantion poartă `dd.MM.yyyy` fără
componentă de oră — adică exact cum arată o dată tastată.

Deci **F13 se sprijinea pe nimic** și a ieșit. Iar fiindcă F13 era cea mai tare constrângere a
formularului, ce a rămas — F14, F16, o singură ștergere per lanț, F15 ca semn — nu mai poate duce
designul. Consecința nu e o funcție mai mică, ci alta:

> Formularul nu mai poate **refuza** util o plasare. Trebuie în schimb să facă fiecare plasare
> **ieftin de desfăcut și imediat vizibilă în consecințe.**

Asta s-a construit.

---

## 2. Ce s-a livrat

### 2.1 `KBotChartView` — linii în trepte (pasul 1 din plan)

`KBotChartLineMode = Straight | Step`, enum nou în `KBotChartEnums.vb`. Proprietate nouă
`LineMode` pe **`KBotChartSeries`** (implicit `Straight`, cu perechea `ShouldSerialize`/`Reset`,
categoria `K-BOT Chart Series`).

Pe SERIE și nu pe control fiindcă e o afirmație despre ce SUNT datele, nu despre cum arată
controlul. `DrawOneSeries` mergea deja segment cu segment (un segment ia culoarea punctului din
**stânga** lui, 0048-05 §9.3); în trepte fiecare segment devine două — orizontal, apoi vertical —
și **amândouă păstrează culoarea punctului din stânga**, fiindcă amândouă aparțin bucății pe care
acel punct a început-o. `FillArea` umple sub traseul în trepte (dreptunghi la înălțimea punctului
din stânga, nu trapez).

**E o corectură de adevăr, nu o alegere de stil.** Valoarea unei recepții între două instantanee
e **constantă, apoi sare**. `FUNDAMENT` §1.3 citește exact valoarea-treaptă la data plății, iar
`ConstruiesteSeriaTotal` implementa deja citirea aia în cod («o valoare ține până o schimbă
următorul instantaneu»). Linia dreaptă desenată până acum contrazicea totalul desenat lângă ea.

Colțul dintre orizontală și verticală **nu primește marcaj**: e singurul vârf pe care datele nu-l
conțin, iar un marcaj acolo ar pretinde o măsurătoare pe care n-a făcut-o nimeni.

`AsociereForm` pune `LineMode = Step` pe fiecare serie, în amândouă vederile, totalul inclusiv.

### 2.2 `KBotChartView` — reperele (pasul 2)

`KBotChartGuide` + `KBotChartGuideCollection`, fișier nou `Chart/KBotChartGuide.vb`. Membri:
`Moment`, `Text` (titlul etichetei), `Tooltip`, `LineColor` (`Color.Empty` = culoarea de text
stinsă a temei), `DashStyle` (implicit `Dot`), `Visible`, `Tag`. Desenate **în spatele** seriilor,
pe toată înălțimea zonei, subțiri și punctate. **Niciodată roșu** — regula permanentă a
operatorului: roșul înseamnă doar «ceva e rău». Se vânează la trecerea mouse-ului (**doar pe
distanță orizontală** — un reper e o coloană întreagă, nu un punct) și se numesc prin
`PointTooltip.ShowAt`. **Nu se pot apăsa și nu ridică niciun eveniment.** Nu întind axa timpului:
unul din afara intervalului punctelor pur și simplu nu se desenează.

### 2.3 `KBot.Controls/Lane/` — `KBotLaneView`, familie nouă de control (pașii 3–4)

Suprafața de așezare: câte o bandă orizontală per recepție, câte un marcaj per instantaneu, un
separator, apoi banda instantaneelor neașezate. Compactă implicit — **fără niciun text** — fiindcă
douăzeci de benzi a câte douăzeci de marcaje e scara reală pe care a numit-o operatorul, iar
textul nu supraviețuiește acolo.

Fișiere: `KBotLaneView.vb`, `.Painting.vb`, `.Drag.vb`, `KBotLane.vb`, `KBotLaneMarker.vb` (cu
colecțiile lor), `KBotLaneEnums.vb`, `KBotLaneView.md`.
`Control` · sealed · partial · Toolbox · `IThemedControl`, `ISupportInitialize`.

`KBotLaneMarkerStyle = Normal | Deletion | NoChange | Locked | Loose` și
`KBotLaneEndMark = None | Ok | Warning`, fiecare cu motivul lui scris în cod. **Nimic nu se
stinge în gri** — `Locked` se desenează în culoare PLINĂ cu lacăt, fiindcă 0048-06 a stabilit că
gri-ul distruge împerecherea prin culoare, singura treabă pe care culoarea o face aici.

Restul e regula familiei, literal: zero culori scrise în cod; `ShouldSerialize*`/`Reset*` pe tot,
inclusiv pe `BackColor`/`ForeColor`/`Font` cu steaguri de «pinuit»; toate măsurile publice în
pixeli logici la 96 dpi, scalate la desenare prin `ThemeShapes.ScaleDpi`, o singură sursă;
etichetele prin `KBotToolTip.ShowAt`; chei validate în `EndInit` și în metodele de rulare, sărite
la design-time cu chenar roșu în loc; `Try/Catch` loghează-și-înghite pe `OnPaint`, mouse,
tastatură și derulare, loghează-și-re-aruncă în mutatorii de colecție.

Tragerea: `MarkerDragStarting` / `MarkerDragOver` / `MarkerDropped`, aceeași formă ca la arbore.
`Allow` implicit **False**. Marcajul tras se citește **din obiectul de date**, niciodată din
câmpul propriu (capcana prinsă în 0048-04 la tragerea între doi arbori). `DoDragDrop` din
WinForms. **Controlul nu mută niciodată marcajul** — ridică evenimentul și se oprește. Chenarul
de aruncare se desenează **și la refuz**.

Derulare **doar pe verticală**, `VScrollBar` standard: axa orizontală e TIMPUL, iar o axă a
timpului care iese din ecran a încetat să mai fie o comparație între benzi, adică singurul lucru
pentru care există suprafața asta.

### 2.4 `AsociereForm` — banda compactă (pasul 5)

`splitDreapta.Panel1` ținea `grafic` singur; acum ține un `SplitContainer` orizontal imbricat
(`splitSus`): `Panel1` = `grafic`, `Panel2` = `benzi`, `Panel2MinSize = 60`, `SplitterDistance`
180 (graficul sus, benzile jos). Scris **direct în `AsociereForm.Designer.vb`** — vezi §6.2.

Legarea: `ReconstruiesteBenzi()` din `Reconstruieste()`; `SincronizeazaCulorile()` extinsă la
benzi (culoarea benzii = culoarea seriei recepției, culoarea marcajului = culoarea punctului), cei
trei tratatori de tragere, `Benzi_EnlargeRequested`, `ntfMesaj.Clear()` la orice atingere.
Aruncarea scrie în **același** `_pozitie` în care scrie și tragerea din arbore: două vederi ale
unui singur tablou local, un singur loc în care se consemnează o așezare.

### 2.5 F13 coborât la semn — client și server (pasul 7)

**Client.** Vetoul a ieșit din `MotivulRefuzului`. În locul lui, `EsteInainteDeDataReceptiei` —
un semn: `[înainte de data recepției]` adăugat la denumirea rândului, un paragraf în eticheta
instantaneului, și un număr în eticheta recepției («N din M instantanee sunt mai vechi decât data
recepției»). Numărul contează mai mult decât faptul: unul singur e o ciudățenie, tot lanțul
înseamnă că data recepției e greșită.

**Server.** În `valideaza_plasarile` (`prelucrare_asociere.py`) F13 nu mai ridică `DecizieInvalida`
ci scrie în `avertismente`. **Pe amândouă căile**, nu doar în editor: pe calea de ingestie un veto
fals e paguba adevărată, fiindcă înfundă operatorul pe o recepție pe care nu o poate repara — exact
ce interzice F10. `aplica_decizii` primește acum `avertismente=warnings`, deci semnul ajunge în
răspuns și pe calea de ingestie. F14, F16 și o-singură-ștergere-per-lanț rămân vetouri pe amândouă
căile, neatinse.

**Comparația se face pe ZI, nu pe timestamp complet** — și asta e o schimbare față de litera
planului, făcută deliberat. Formularea veche («timestamp complet, nu granularitate de zi») pleca de
la ideea că amândouă capetele sunt momente. Nu sunt: `DataR` e o dată TASTATĂ, deci sosește la
miezul nopții, iar `DataH` e ceasul sistemului. Comparate ca momente, ORICE instantaneu din chiar
ziua recepției ar ieși «înainte de ea», și semnul s-ar aprinde pe date perfect corecte. Consemnat
și în F13 în fundament.

### 2.6 `AsociereBenziForm` — fereastra mare (pasul 6)

`src/KBot.App/Forexe/AsociereBenziForm.{vb,Designer.vb}`, `KBotThemedForm`, modală, maximizată.
Un `KBotLaneView` docked `Fill`, configurat pentru citirea largă: denumiri de bandă, sume lângă
marcaje, date pe axă, `LaneHeight = 26`. Fără buton de salvare — D-H stă.

**Nu are date proprii și nu are voie să aibă.** Împrumută construcția (`UmpleBenzile`) și cei trei
tratatori (`LeagaBanda`), deci nu există o a doua regulă de așezare care s-ar putea abate de la
prima. La închidere nu e nimic de împăcat: n-au fost niciodată două tablouri.

---

## 3. Fișiere atinse

**Noi**
- `src/KBot.Controls/Lane/KBotLaneEnums.vb`, `KBotLaneMarker.vb`, `KBotLane.vb`,
  `KBotLaneView.vb`, `KBotLaneView.Painting.vb`, `KBotLaneView.Drag.vb`, `KBotLaneView.md`
- `src/KBot.Controls/Chart/KBotChartGuide.vb`, `KBotAutoPalette.vb`
- `src/KBot.App/Forexe/AsociereBenziForm.vb`, `AsociereBenziForm.Designer.vb`
- `docs/worklog/SLICE-0048-07-benzi-si-retragerea-f13.md` (fișierul ăsta)

**Modificate**
- `src/KBot.Controls/Chart/KBotChartEnums.vb` — `KBotChartLineMode`
- `src/KBot.Controls/Chart/KBotChartSeries.vb` — `LineMode` + perechea lui
- `src/KBot.Controls/Chart/KBotChartView.vb` — `Guides`, `AddGuide`, `ClearGuides`,
  `IKBotGuideHost`, `_hoverGuideIndex`
- `src/KBot.Controls/Chart/KBotChartView.Painting.vb` — desen în trepte, `ProjectGuides`,
  `DrawGuides`, `HitTestGuide`, eticheta reperului, `AutoColor` delegat la `KBotAutoPalette`
- `src/KBot.Controls/Chart/KBotChartView.md`, `src/KBot.Controls/CONTROLS.md`
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.37.0.0 ▸ **1.38.0.0**, convenția de
  organizare actualizată cu `Lane\`
- `src/KBot.App/Forexe/AsociereForm.vb`, `AsociereForm.Designer.vb`
- `PYTHON/routes/forexe/prelucrare_asociere.py`, `asociere.py`
- `PYTHON/tests/test_forexe_prelucrare_asociere.py`, `test_forexe_asociere.py`
- `tests/KBot.App.Tests/AsociereFormTests.vb`
- `docs/FUNDAMENT_Asociere_Receptii.md`
- `docs/worklog/KBOT_STATUS.md`

`AssemblyVersion` rămâne `1.0.0.0`.

---

## 4. Corecturile din fundament (§2 din plan)

- **F13 — RETRAS.** Textul e tăiat, nu șters, cu motivul complet și cu nota că a rămas SEMN pe
  amândouă căile și că se măsoară pe zi.
- **F25 — AMENDAT, și cheia numită corect.** Documentul (și citirea din care venea) spunea că
  pasul 4b se potrivește pe hash-ul de antet. **Nu.** Verificat în `Receptii_Prelucrare` real:
  `rsTmpR_Snap.FindFirst "CLng(DataR)=" & CLng(dtDataR)`. `sHashIdent` se CALCULEAZĂ în
  `ObtineDateHeader` și se SCRIE în `tmpFX_Receptii_R!HASH` la inserare, dar **nu se citește
  niciodată pentru potrivire**. Regula lui F25 rămâne întreagă.
- **F29 — NOU.** `DataR` e editabil de operator, inclusiv după creare, și tabelul nu are momentul
  creării. Nicio regulă nu are voie să-l trateze ca pe un fapt despre când a apărut recepția.
- **F30 — NOU, consemnat ca HAZARD, NEREPARAT aici.** Fiindcă 4b se potrivește pe `CLng(DataR)` cu
  `FindFirst`, două recepții cu același `DataR` se ciocnesc la fiecare descărcare de după prima:
  a doua o suprascrie pe prima și niciun rând nou nu se inserează. La PRIMA descărcare amândouă se
  inserează — de-aia perechi din astea ajung să existe (`AAB2HFBEEAF`, rândurile 268 și 269, ambele
  16.01.2026, cu `HASH` identic, deci nici hash-ul n-ar discrimina). Editarea lui `DataR` pe site
  (F29) dă defectul oglindă: cheia se mută, 4b nu mai recunoaște recepția, se inserează un duplicat.
  Operatorul spune că n-a văzut niciodată editarea în practică și că purtarea pe aceeași zi «nu e
  bună tot timpul, dar merge în mare». **Consemnat, deliberat nereparat.**
- **§1.5 rescris** — «regula datei» e tăiată și înlocuită cu semnul; propoziția-cheie a designului
  («nu mai poate refuza util, deci trebuie să facă plasarea ieftin de desfăcut») e scrisă acolo.
- **Partea 5** — **O2 închis** (F23 a fost retras pe 26.08 și F24 l-a înlocuit; contractul în două
  faze e construit pe F24, nu se datorează nimic). **O7 adăugat**, arătând spre F30 ca hazard
  acceptat, nu ca decizie deschisă.

---

## 5. Ce s-a hotărât altfel decât în literă, și de ce

**5.1 Ordinea lui `ReconstruiesteBenzi`.** Planul cerea «imediat după `ReconstruiesteGrafic()` și
înainte de `SincronizeazaCulorile()`». Imposibil ca atare: `SincronizeazaCulorile` nu se cheamă din
`Reconstruieste`, ci din coada lui `ReconstruiesteGrafic` — ca să prindă și schimbarea de filă, și
cea de selecție, care nu ating benzile. Intenția (benzile există înainte ca pasul de culori să
ruleze) s-a obținut punând benzile **înaintea** graficului. Comentat la fața locului.

**5.2 Reperele: două obiecte, nu unul împărțit.** Planul spune «aceeași colecție `Guides`». Tipul
CHIAR e împărțit — `KBotChartGuide` e desenat și de grafic, și de benzi, prin `IKBotGuideHost` — dar
prima variantă punea același OBIECT în amândouă colecțiile, și acolo e o capcană: un reper are un
singur câmp de proprietar (cel care îl repictează la schimbarea culorii), iar al doilea adăugat îl
fura pe primul. Acum se construiesc două repere în ACEEAȘI buclă din aceeași plată. Garanția e la
fel de tare — amândouă primesc `plata.DataPlata`, aceeași valoare, în același pas — fără obiect cu
doi stăpâni.

**5.3 `RangeStart`/`RangeEnd` există, dar `AsociereForm` NU le folosește.** Planul cere «margini
comune cu graficul, ca un reper de plată să stea în aceeași poziție orizontală în amândouă
controalele». Controlul are cu ce (`RangeStart`/`RangeEnd`, doar la rulare), dar formularul nu le
fixează, **deliberat**: benzile arată și instantaneele NEAȘEZATE, iar graficul nu; fixate pe
intervalul graficului, marcajele din afara lui n-ar mai fi desenate — adică suprafața al cărei rost
e tocmai să arate instantaneele neașezate le-ar ascunde. Alinierea la pixel între cele două rămâne
deci adevărată doar când amândouă arată același interval. **De verificat pe ecran de operator**;
dacă vrea alinierea cu orice preț, unealta e deja acolo.

**5.4 `KBotAutoPalette`.** `AutoColor` era o metodă privată a graficului. Fiindcă acum și benzile
trebuie să dea aceeași culoare aceluiași fapt, setul s-a mutat într-un modul `Friend` împărțit, iar
`KBotChartView.AutoColor` doar deleagă. Două implementări s-ar fi potrivit în ziua în care au fost
scrise și s-ar fi despărțit la prima schimbare de temă. Culorile ies bit cu bit aceleași — e
aceeași aritmetică mutată, nu rescrisă.

**5.5 `AllowDrop` — capcana C4 pe o proprietate MOȘTENITĂ.** Prinsă măsurând, nu ghicind: un
`KBotLaneView` proaspăt serializa `AllowDrop`, unde un `KBotChartView` proaspăt nu serializa nimic
peste cele trei linii pe care Visual Studio le scrie pentru orice control (`Location`, `Name`,
`TabIndex`). Constructorul îl aprinde fiindcă tot rostul controlului e să primească aruncări, dar
`AllowDrop` poartă `DefaultValue(False)`, deci designerul ar fi scris `benzi.AllowDrop = True` în
fiecare formular-gazdă. **Un `ShouldSerializeAllowDrop` NU merge** — și s-a încercat întâi:
`TypeDescriptor` construiește descriptorul unei proprietăți moștenite pe tipul care o DECLARĂ, deci
caută metoda pe `Control` și n-o vede niciodată pe a noastră. Soluția e cea deja consemnată în
notele proiectului pentru cazul ăsta: se umbrește proprietatea și se marchează umbra, fiindcă
umbrirea NU moștenește atributele de serializare ale bazei. Verificat empiric prin
`TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)` — drumul pe care chiar merge Visual
Studio — cu o sondă temporară, ștearsă după (§7).

**5.6 Testele care codificau F13 au fost rescrise, nu șterse.** Trei bucăți (două în Python, una în
`KBot.App.Tests`) verificau vetoul. Regula n-a dispărut, a coborât — deci testele au coborât cu ea
și verifică acum semnul. Nu e «test nou»: e același test, pe regula corectată. Un test lăsat pe
regula retrasă ar fi însemnat livrarea unei suite roșii.

---

## 6. Punctele de «oprește-te și raportează» (§4 din plan) — ce a ieșit

**6.1 Plățile în sarcina utilă — NU s-a declanșat.** `GET /api/forexe/asociere` întoarce deja
`plati: [{data_plata, suma, nr_op}]` (`asociere.py`, `_PLATI_SQL`), `AsociereStare.Plati` există în
`KBot.Domain/AsociereStare.vb` ca `List(Of PlataAsociere)`, iar `ApiClient.CitesteAsociere` o umple
deja. Nu s-a atins nicio rută și niciun tabel.

**6.2 Aspectul din designer — NU s-a declanșat.** `docs/kbot-forms-ui-convention.md` §5, §6 și §8
**nu** rezervă aspectul operatorului: rezervă aspectul **fișierului `.Designer.vb`**, spre deosebire
de codul de rulare. §6 e explicită — «dacă o schimbare aparține Designerului — mutare/re-părintare/
re-docare, redimensionare, adăugare de control — fă-o **direct în `*.Designer.vb`**». Deci
restructurarea s-a făcut acolo, direct. §7 respectată: **niciun comentariu nou** în `.Designer.vb`
(cele existente în `AsociereForm.Designer.vb` sunt dinainte, neatinse).

**6.3 Canalul de avertismente al serverului — NU s-a declanșat.** Există deja: `aplica_comenzi`
primește `avertismente: list`, `POST /api/forexe/asociere` îl întoarce ca `"avertismente"`, iar
clientul îl citește deja în `AsociereRezultat.Avertismente`. Pe calea de ingestie echivalentul e
`warnings`, pe care `aplica_decizii` îl primea deja pentru altceva. Nu s-a inventat nicio a doua
formă de răspuns.

**6.4 Unde trăia F13 — NU s-a declanșat.** F13 era un bloc `if` distinct în `valideaza_plasarile`,
nu împletit cu F14/F16. Coborârea lui nu atinge purtarea celorlalte două, iar testele lor trec
neschimbate.

---

## 7. Teste

| Suită | Înainte | După |
|---|---|---|
| `tests/KBot.Controls.Tests` | 953 trecute / 0 picate | **953 trecute / 0 picate** |
| `tests/KBot.App.Tests` | 185 trecute / **13 picate** | 185 trecute / **13 picate** |
| `PYTHON/tests` (via `.venv`) | 465 trecute / 15 sărite | **465 trecute / 15 sărite** |

Linia de plecare pentru `KBot.App.Tests` s-a luat **prin `git stash` pe modificările feliei**, nu
din memorie — tocmai fiindcă lucrul ăsta a mers prost o dată (`KBOT_STATUS` zicea șapte, erau
șaptesprezece). Cele 13 picate sunt **preexistente și străine de felia asta**:
`DdfViewTests` (3), `DdfXfaParserTests` (2), `IstoricViewTests` (6), `MainFormNavItemsTests` (1),
`XfaXmlPreviewTests` (1). Cu modificările aplicate au fost o clipă 14 — a paisprezecea fiind
`AsociereFormTests.F13_ORecepțieMaiNouaDecatInstantaneul_ERefuzata`, adică chiar testul regulii
retrase; rescris ca `F13Retras_...ETotusiPrimita`, suita s-a întors la 13.

**Fără teste noi**, conform regulii permanente. Singura excepție a fost o **sondă temporară**
(`ZZTempLaneSerializationProbe.vb`) folosită ca să măsor ce serializează un `KBotLaneView` proaspăt
față de un `KBotChartView` proaspăt; a găsit `AllowDrop` (§5.5), și a fost **ștearsă** după ce
măsurătoarea a ieșit `CHART: [Location, Name, TabIndex]` = `LANE: [Location, Name, TabIndex]`. Nu a
rămas în suită.

**Build:** `dotnet build KBot.sln` — **0 erori**. `KBot.Controls` singur: **0 erori / 0
avertismente**. Pe soluție apar 6 avertismente `MSB3825` (`AsociereForm.resx`,
`Il_Receptii.ImageStream` prin `BinaryFormatter`) — **preexistente**, consemnate ca atare și în
worklogul feliei 0048-06, și vizibile doar când se regenerează `.resx`-ul. Nu s-a atins `.resx`-ul
în felia asta pentru controalele noi: `benzi` nu are nicio imagine (butonul de mărire își desenează
singur pictograma când gazda nu-i dă una).

---

## 8. Observații pe `Receptii_Prelucrare`, consemnate nereparate

Operatorul a dat VBA-ul real pe 31.08.2026 și l-a descris ca «nu e bun tot timpul, dar în mare
merge». **Niciun VBA nu s-a modificat în felia asta** — VBA-ul e referința, portarea trăiește
altundeva.

1. **`CLng(DataR)` e cheia de identitate** (§4). Granularitate de zi, `FindFirst`, prima potrivire
   câștigă. Consecințele sunt F30.
2. **`rsTmpR_Snap` e un instantaneu luat ÎNAINTE de buclă.** Recepțiile inserate în timpul rulării
   sunt invizibile iterațiilor următoare ale aceleiași rulări. De-aia două recepții din aceeași zi
   se inserează amândouă la prima descărcare și se ciocnesc la următoarele.
3. **Un `cReceptii` gol întoarce `False`.** `If cReceptii Is Nothing Then Exit Function` și
   `If cReceptii.Count <= 0 Then Exit Function` cad amândouă pe valoarea implicită a funcției. Dacă
   apelantul citește `False` ca eroare, un angajament fără recepții e raportat ca eșec. De verificat
   pe partea apelantului; nu e evident greșit, doar ambiguu.
4. **`Nz(DMax("NRCRT", "tmpFX_Receptii_R"), 0)`** — `Nz` e interzis în cod K-BOT nou. Consemnat
   pentru portare, nu pentru original.
5. **`NRCRT` e atribuit de K-BOT**, nu de FOREXE (`DMax + 1`). Confirmat de operator: nu înseamnă
   nimic dinspre site. Nicio regulă nu are voie să-l trateze ca ordine de creare — de-aia
   constrângerea de ordonare avută în vedere mai devreme în design a fost lăsată deoparte.

---

## 9. Ce NU s-a făcut, și de ce

Din §5 al planului, explicit în afara ariei:

- **Nicio propunere automată.** Formularul arată; nu ghicește cărei recepții îi aparține un orfan.
- **Niciun teanc de anulare.** Cu o singură salvare la sfârșit și cu `Comenzi()` trimițând doar ce
  diferă, o tragere greșită se desface trăgând înapoi. Aia e reversibilitatea.
- **Nicio plasare în masă.** Operatorul raportează ~10 instantanee neașezate obișnuit, 25 la cel mai
  rău văzut. Câte una pe rând e primitiva potrivită la scara aia.
- **F30 nu e reparat.** Consemnat în fundament, niciun rând de cod.
- **Fără teste noi** (vezi §7).
- **Fără randare și fără capturi de ecran.** Ale operatorului, pe mașina lui, după publicare.
- **Nimic nu a rulat pe MariaDB.**

## 10. Rămâne neverificat

- **Totul, vizual.** `KBotLaneView` nu a fost desenat niciodată — nici la rulare, nici în designerul
  Visual Studio. Mărimile compacte (bandă de 13 px logici, marcaj de 7) sunt alese pe hârtie din
  scara pe care a numit-o operatorul; dacă nu se citesc, `LaneHeight`/`MarkerSize` sunt exact
  manetele de învârtit.
- **Lacătul la mărimea compactă** e două-trei pixeli de detaliu. Consemnat ca limită în
  `KBotLaneView.md`, nu ca defect.
- **Alinierea orizontală bandă ▸ grafic** e adevărată doar când amândouă arată același interval
  (§5.3). De privit pe ecran.
- **Semnul F13 pe date reale.** Comparația pe zi e argumentată, nu măsurată: nimeni n-a rulat-o
  peste un angajament viu ca să vadă câte semne se aprind.
- **Ruta și clientul nu au rulat viu.** La fel ca toată familia 0048.
