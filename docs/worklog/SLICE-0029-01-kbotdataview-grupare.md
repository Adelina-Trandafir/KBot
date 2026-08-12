# SLICE-0029-01 — `KBotDataView`: grupare ca la raportul Access

**Cerere operator:** «grila trebuie să suporte grupare EXACT ca raportul Access: agregare,
culori diferite și fonturi diferite. Atenție la sortare / filtrare pe o grilă grupată.»

**Trei hotărâri confirmate de operator înainte de a scrie o linie** (grupurile se pot STRÂNGE;
agregatele merg pe AMÂNDOUĂ benzile, alese pe nivel; o sortare pe o coloană negrupată ordonează
ÎNĂUNTRUL grupurilor). Toate trei erau recomandarea, dar niciuna nu se putea deduce din cerere.

---

## Ce s-a schimbat și de ce

### 1. A treia numerotare: benzile

Grila avea deja două numerotări, ținute separat cu grijă (slice 0028-03): *indici de MODEL*
(ordinea de încărcare, pe care o vorbește tot API-ul public) și *poziții de VEDERE* (ordinea de pe
ecran, după filtrare și sortare). Gruparea aduce a treia — *indici de BANDĂ*, adică rândurile
DESENATE, printre care se numără și antetele/subsolurile de grup, care nu sunt rânduri de model
deloc.

Regula veche NU s-a mișcat: `Item(cheie, index)`, `CurrentRowIndex`, `CellClick`, `EnsureVisible`
înseamnă azi exact ce însemnau înainte de felie. Ce s-a mutat e geometria, care lucrează acum în
benzi.

**`_view` a rămas neatinsă ca înțeles** — lista rândurilor de DATE care trec de filtre. Gruparea nu
scoate nimic din ea, nici măcar rândurile unui grup strâns. De aici vine cea mai importantă
proprietate a feliei: subsolul grilei și măsurarea coloanelor au continuat să lucreze pe același
lucru ca înainte, fără o singură modificare.

### 2. Înălțimi variabile fără să se piardă virtualizarea

Virtualizarea se sprijinea pe o înmulțire: `poziție × RowHeight`. O bandă de grup poate avea
înălțimea ei (`HeaderHeight`/`FooterHeight` pe nivel), deci înmulțirea nu mai ține.

Soluția: **offset-uri cumulate + căutare binară** (`BandIndexAtOffset`). Costul unei pictări rămâne
al FERESTREI, nu al modelului — proba e în test: 5.000 de rânduri și 50 de grupuri pictează sub 40
de rânduri de date.

**Calea negrupată n-a fost atinsă.** Fără niciun nivel activ, `IsGrouped` e False, tabloul de benzi
nu se alocă deloc, iar geometria rămâne pe împărțire/înmulțire. Asta a fost o hotărâre, nu o
optimizare de final: tabloul de benzi e O(rânduri vizibile), adică exact ce evita virtualizarea, și
n-avea voie să fie plătit de cele șase vederi care nu grupează.

### 3. Sortarea nu poate rupe gruparea — miezul feliei

Comparatorul compară întâi CHEILE DE GRUPARE, în ordinea nivelurilor, și abia apoi coloana cerută
de operator din antet. E precedența din fereastra «Sorting and Grouping» a unui raport Access, și nu
e o preferință de stil:

> Un grup înseamnă rândurile aceleiași chei, **lipite**. Dacă sortarea operatorului ar putea trece
> înaintea cheii de grupare, aceleași chei s-ar împrăștia prin listă și fiecare bucată și-ar primi
> propriul antet și propriul total — o grilă în care «Total ianuarie» apare de patru ori, cu patru
> sume diferite.

**Al doilea caz, cel care ar fi fost ușor de ratat:** un click pe antetul unei coloane care e ea
însăși cheie de grupare. Acolo nu se adaugă o sortare secundară — se schimbă SENSUL nivelului, iar
sortarea de rând se ridică. E singurul lucru pe care îl poate însemna «sortează după lună» într-o
grilă grupată pe lună: lunile sunt deja adunate, tot ce mai rămâne de ales e dacă merg în sus sau în
jos. Ridicarea sortării de rând e ca antetul să nu poarte două semne de sortare deodată.

### 4. Filtrarea schimbă totalurile, strângerea NU

Distincția asta e cea mai ușor de stricat și cea mai vizibilă când e stricată:

- un **filtru** scoate rânduri din pagină → grupul rămas fără niciun rând dispare cu antet și subsol
  cu tot (nu rămâne un antet gol), iar totalurile scad, fiindcă pagina chiar s-a schimbat;
- o **strângere** doar ascunde → totalul grupului și cel general rămân NESCHIMBATE. Dacă cele două
  s-ar purta la fel, un operator care închide o lună ar vedea totalul general mișcându-se singur sub
  ochii lui, ceea ce se citește ca o greșeală de calcul, nu ca o comoditate de afișare.

Aceeași distincție se propagă în selecție, și acolo a cerut o hotărâre explicită: **un rând filtrat
afară pierde selecția; unul închis într-un grup strâns NU o pierde.** Un filtru spune «rândul nu mai
e în pagină» — n-ai la ce te întoarce. O strângere spune «nu acum», și e făcută ca să fie desfăcută:
dacă selecția ar cădea, `Ctrl+Dreapta` n-ar mai putea redeschide ce tocmai a închis `Ctrl+Stânga`,
fiindcă n-ar mai exista niciun rând despre care să întrebe. Rândul păstrează selecția, nu desenează
nimic (n-are bandă) și se sprijină pe antetul grupului — `AnchorBandOfRow`. De acolo vine și
purtarea corectă a săgeții în jos: pleacă de la antet, deci sare peste tot grupul, exact ca ochiul.

### 5. Agregatele: o singură definiție a lui «sumă»

`ComputeAggregateText` a fost parametrizată pe un INTERVAL de poziții de vedere. Subsolul grilei o
cheamă pe toată vederea, subsolul unui grup pe intervalul lui. Un grup e prin construcție un
interval contiguu, deci agregatul de grup e chiar agregatul grilei calculat peste o felie — **nu o a
doua formulă**, care s-ar fi putut contrazice cu prima.

Din același motiv nu există o proprietate `GroupAggregate` pe coloană: ce agregat aduce o coloană e
`KBotDataColumn.Aggregate`, cel care alimentează deja subsolul. Un raport Access scrie `=Sum([x])` o
dată și îl pune și în subsolul de grup, și în cel de raport. Ce se alege PE NIVEL e dacă agregatele
se ARATĂ, nu care sunt.

Cache-ul e leneș, pe nod: o grilă cu zece mii de grupuri și cinci coloane agregate ar fi plătit
cincizeci de mii de treceri la fiecare resortare, ca să arate douăzeci de benzi.

### 6. Culori și fonturi

Roluri PROPRII în temă (`_cGroupHeaderBack` / `_cGroupFooterBack` / …), nu împrumutate de la antetul
sau subsolul grilei — o schemă trebuie să poată distinge «titlul unei secțiuni» de «titlurile
coloanelor». Antetul de grup e spălat spre accent mai tare (0,28) decât subsolul grilei (0,1): el
desparte secțiuni, nu încheie o pagină.

**Nuanța pe adâncime se calculează, nu se configurează:** nivelul 0 e cel mai apăsat, fiecare nivel
de sub el se apropie de fundalul rândurilor. Așa ierarhia se citește fără ca nimeni să aleagă cinci
culori, iar un nivel adăugat nu cere un slot nou de paletă.

Peste asta, trei straturi de control, în ordinea puterii: culoarea/fontul fixate PE NIVEL bat tema;
evenimentul `GroupFormatting` bate nivelul (acolo se colorează grupul care iese din tipar — luna cu
depășire pe roșu); iar sub o schemă ÎNTUNECATĂ culorile fixate se ignoră, exact ca la benzile grilei
(regula din 0028-03: la întuneric contrastul bate preferința). **Fontul fixat rămâne al operatorului
în orice schemă** — un font nu devine ilizibil pe fundal închis.

### 7. Regula `ShouldSerialize` — verificată, nu presupusă

Fiecare proprietate de nivel care se poate seta din designer și nu poate purta `DefaultValue`
(`Font`, `Color`, cele două înălțimi) are perechea `ShouldSerialize`/`Reset`, iar testele o verifică
prin `TypeDescriptor`, adică pe drumul pe care merge chiar Visual Studio.

**O asumpție a mea a picat aici, și e bine că a picat printr-un test:** scrisesem că `Groups` nu
trebuie să se serializeze pe o grilă neatinsă. Fals — o colecție `Content` raportează ÎNTOTDEAUNA
`True`, fiindcă designerul nu scrie proprietatea, ci coboară în ea și scrie elementele; goală, nu
produce nicio linie oricum. Codul era corect (identic cu `Columns`, care face asta din 0025), testul
era greșit. Înlocuit cu verificarea onestă: `Groups` se poartă exact ca `Columns`.

---

## Fișiere atinse

**Noi — `src/KBot.Controls/DataView/`**

| Fișier | Rol |
|---|---|
| `KBotGroupBandKind.vb` | enum: `Data` / `GroupHeader` / `GroupFooter` |
| `KBotGroupLevel.vb` | un nivel de grupare (coloană, sens, benzi, șabloane de titlu, culori, fonturi, strângere) |
| `KBotGroupLevelCollection.vb` | colecția ordonată din spatele `Groups` — ordinea E ierarhia |
| `KBotDataView.Grouping.vb` | benzi, arborele de grupuri, strângere pe CALE, geometrie, porți de verificare |
| `KBotDataView.Grouping.Painting.vb` | pictarea benzilor: fundal pe adâncime, retragere, triunghi, titlu, agregate |
| `Events/KBotGroupFormattingEventArgs.vb` | formatarea UNUI grup (argumente refolosite) |

**Modificate**

| Fișier | Ce |
|---|---|
| `KBotDataView.vb` | `Groups.Owner`, validare la `EndInit`, `RefreshActiveLevels` la schimbarea coloanelor, `RowHeight` invalidează benzile |
| `.Filtering.vb` | comparator cu cheile de grupare PRIMELE; `ApplySort` pe o cheie de grupare întoarce nivelul; `GroupLevelFor`; `InvalidateView` → benzi |
| `.Layout.vb` | `FirstVisibleBand`/`LastVisibleBand`/`BandTop`, `RowTop` pe benzi, `ContentHeight()` în bara de derulare, `EnsureVisible` pe ancoră |
| `.Painting.vb` | `DrawRows` iterează BENZI; `DebugLastPaintedDataRows` numără în continuare doar rândurile de date |
| `.Footer.vb` | `ComputeAggregateText(col, primaPoziție, ultimaPoziție)` + `ViewRowsRange` |
| `.Input.vb` | `BandAtPoint`, `RowAtPoint` refuză benzile de grup, click pe antet = strângere, `MoveRow` peste benzi, `Ctrl+Stânga/Dreapta`, cursor de mână |
| `.AutoSize.vb` | predicția barei verticale citește `ContentHeight()` |
| `.Theming.vb` | cinci roluri noi + creionul de despărțire (create, resetate și eliberate) |

**Bancul** — `DataViewPlaygroundForm(.Designer).vb`: secțiune «Grupare», două combo-uri de nivel,
agregate în antet/subsol, `CollapsedByDefault`, `CollapseAllGroups()`/`ExpandAllGroups()`,
`FooterVisible`, numărul de grupuri în bara de info; coloanele `val*` au primit `ValueType = Number`
+ `Aggregate = Sum`, ca subsolurile să aibă ce arăta.

**Teste** — `KBotDataViewGroupingTests.vb` (33) + `KBotGroupLevelDesignerTests.vb` (15).

---

## Rezultatele testelor

```
KBot.Controls.Tests    800 passed / 0 failed   (752 înainte de felie + 48 noi)
KBot.DevHarness.Tests  170 passed / 0 failed
KBot.Theming.Tests      71 passed / 0 failed
KBot.Common.Tests       14 passed / 0 failed
dotnet build KBot.sln  0 erori, 1 avertisment (MSB3825 BinaryFormatter în DdfView.resx — preexistent)
```

**Eșecuri PREEXISTENTE, verificate pe arbore curat** (`git stash`, aceleași cifre cu și fără felie
— NU sunt ale acestei felii și nu au fost atinse): `KBot.App.Tests` 10, `KBot.Domain.Tests` 3,
`KBot.Api.Tests` 1. Toate în jurul etichetei de revizie DDF și al diacriticelor din designer.

---

## Nerulat / rămas deschis

1. **NIMIC DIN FELIE N-A FOST VĂZUT PE ECRAN.** Aceeași situație ca tot slice-ul 0028. Bancul are
   acum comutatoarele, dar nu a fost pornit. Lucrurile care se judecă DOAR cu ochiul: nuanța pe
   adâncime a nivelurilor (dacă nivelul 2 chiar se distinge de nivelul 1), retragerea cumulativă,
   triunghiul de strângere, și cum arată benzile de grup sub schema Modern (degrade) și sub Dark.
2. **Dus-întorsul prin designerul Visual Studio nu a fost făcut.** Colecția `Groups` ar trebui să se
   editeze cu dialogul standard, iar un nivel neatins să nu scrie nicio linie în `.Designer.vb`.
   Testele verifică asta prin `TypeDescriptor` — drumul pe care merge designerul — dar nu ÎNSUȘI
   designerul. E aceeași limită consemnată la slice 0025.
3. **Niciun ecran real nu grupează încă.** Felia dă mecanismul; `IstoricView` și `PlatiView` sunt
   candidații evidenți (clasificații, respectiv luni), dar nu au fost atinse deliberat — sunt
   hotărâri de vedere, nu de control.
4. **Limită cunoscută, de la tastatură:** `Ctrl+Stânga` strânge grupul rândului curent și rândul
   rămâne selectat (ascuns), deci `Ctrl+Dreapta` îl redeschide. Corect. Dar nu există o cale de a
   MUTA selecția pe o bandă de grup — ea nu are index de model — deci nu se poate «naviga prin
   antete» de la tastatură. Nu e o scăpare, e limita modelului de selecție al grilei; ar cere o
   noțiune de «bandă curentă» separată de «celulă curentă».
5. **Costul de memorie al tabloului de benzi**, spus pe față: ~20 de octeți per rând vizibil, DOAR
   când grila e grupată. La 500.000 de rânduri grupate și complet desfăcute înseamnă ~10 MB. Pe o
   grilă negrupată e zero. Nu s-a măsurat pe date reale.
6. **Ruperea grupurilor se face pe TEXTUL AFIȘAT**, ordinea pe VALOAREA BRUTĂ. Pentru toate
   formatele obișnuite cele două sunt de acord (afișarea e monotonă), deci grupurile ies contigue.
   Un format NUMIT ne-monoton (ex. «Yes/No» peste o coloană `Number` cu valori negative) ar putea da
   două grupuri cu același titlu. Nu s-a întâlnit; se rezolvă punând `ValueType = Boolean`.
7. **Nu există un raport Access de referință.** Exportul de la `C:\AVACONT\FX_System_Export` are
   doar `FORMS / QUERIES / TABLES / MODULES` — **nicio secțiune `REPORTS`**. Felia implementează
   SEMANTICA de grupare a rapoartelor Access (bandă de antet, bandă de subsol cu `=Sum()`,
   precedența cheilor de grupare la sortare), nu un raport anume. Dacă operatorul are în minte un
   raport concret, el trebuie exportat ca să se poată potrivi la literă.
