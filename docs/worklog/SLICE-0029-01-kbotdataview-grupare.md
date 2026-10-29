# SLICE-0029-01 — `KBotDataView`: grupare ca la raportul Access

**Cerere operator:** «grila trebuie să suporte grupare EXACT ca raportul Access: agregare, culori diferite și fonturi diferite. Atenție la sortare / filtrare pe o grilă grupată.»

**Trei hotărâri confirmate de operator înainte de a scrie o linie** (grupurile se pot STRÂNGE; agregatele merg pe AMÂNDOUĂ benzile, alese pe nivel; o sortare pe o coloană negrupată ordonează ÎNĂUNTRUL grupurilor). Toate trei erau recomandarea, dar niciuna nu se putea deduce din cerere.

---

## Ce s-a schimbat și de ce

### 1. A treia numerotare: benzile

Peste indicii de MODEL (ordinea încărcării, folosită de API) și pozițiile de VEDERE (după filtrare/sortare) din 0028-03, gruparea adaugă indicii de BANDĂ: rândurile desenate, inclusiv antetele/subsolurile de grup fără rând de model.

Regula veche NU s-a mișcat: `Item(cheie, index)`, `CurrentRowIndex`, `CellClick`, `EnsureVisible` înseamnă azi exact ce însemnau înainte de felie. Ce s-a mutat e geometria, care lucrează acum în benzi.

`_view` păstrează rândurile de DATE care trec filtrele, inclusiv cele ascunse de grupuri strânse. Subsolul general și măsurarea coloanelor lucrează astfel neschimbat.

### 2. Înălțimi variabile fără să se piardă virtualizarea

Virtualizarea se sprijinea pe o înmulțire: `poziție × RowHeight`. O bandă de grup poate avea înălțimea ei (`HeaderHeight`/`FooterHeight` pe nivel), deci înmulțirea nu mai ține.

Soluția: **offset-uri cumulate + căutare binară** (`BandIndexAtOffset`). Costul unei pictări rămâne al FERESTREI, nu al modelului — proba e în test: 5.000 de rânduri și 50 de grupuri pictează sub 40 de rânduri de date.

**Calea negrupată rămâne neschimbată:** fără nivel activ, `IsGrouped = False`, nu se alocă benzi, geometria folosește împărțire/înmulțire. Cele șase vederi negrupate evită astfel costul O(rânduri vizibile) al tabloului de benzi.

### 3. Sortarea nu poate rupe gruparea — miezul feliei

Comparatorul compară întâi CHEILE DE GRUPARE, în ordinea nivelurilor, și abia apoi coloana cerută de operator din antet. E precedența din fereastra «Sorting and Grouping» a unui raport Access, și nu e o preferință de stil:

> Un grup înseamnă rândurile aceleiași chei, **lipite**. Dacă sortarea operatorului ar putea trece
> înaintea cheii de grupare, aceleași chei s-ar împrăștia prin listă și fiecare bucată și-ar primi
> propriul antet și propriul total — o grilă în care «Total ianuarie» apare de patru ori, cu patru
> sume diferite.

Clicul pe antetul unei chei de grupare schimbă SENSUL nivelului, fără sortare secundară. Sortarea de rând se elimină, evitând două semne de sortare în antet.

### 4. Filtrarea schimbă totalurile, strângerea NU

Distincția asta e cea mai ușor de stricat și cea mai vizibilă când e stricată:

- un **filtru** scoate rânduri din pagină → grupul rămas fără niciun rând dispare cu antet și subsol cu tot (nu rămâne un antet gol), iar totalurile scad, fiindcă pagina chiar s-a schimbat;
- o **strângere** doar ascunde → totalul grupului și cel general rămân NESCHIMBATE. Dacă cele două s-ar purta la fel, un operator care închide o lună ar vedea totalul general mișcându-se singur sub ochii lui, ceea ce se citește ca o greșeală de calcul, nu ca o comoditate de afișare.

**Rândul filtrat pierde selecția; cel ascuns într-un grup strâns o păstrează.** Astfel `Ctrl+Dreapta` poate redeschide grupul închis cu `Ctrl+Stânga`. Rândul fără bandă se ancorează pe antet prin `AnchorBandOfRow`; săgeata în jos pleacă de acolo și sare peste grup.

### 5. Agregatele: o singură definiție a lui «sumă»

`ComputeAggregateText` primește un INTERVAL de poziții de vedere: întregul `_view` pentru subsolul general, intervalul contiguu al grupului pentru subsolul acestuia. Ambele folosesc aceeași formulă.

Agregatul coloanei rămâne `KBotDataColumn.Aggregate`, fără `GroupAggregate` separat. PE NIVEL se alege numai afișarea agregatelor, ca aceeași expresie `=Sum([x])` din subsolurile de grup și raport Access.

Cache-ul e leneș, pe nod: o grilă cu zece mii de grupuri și cinci coloane agregate ar fi plătit cincizeci de mii de treceri la fiecare resortare, ca să arate douăzeci de benzi.

### 6. Culori și fonturi

Roluri PROPRII în temă (`_cGroupHeaderBack` / `_cGroupFooterBack` / …), nu împrumutate de la antetul sau subsolul grilei — o schemă trebuie să poată distinge «titlul unei secțiuni» de «titlurile coloanelor». Antetul de grup e spălat spre accent mai tare (0,28) decât subsolul grilei (0,1): el desparte secțiuni, nu încheie o pagină.

**Nuanța pe adâncime se calculează, nu se configurează:** nivelul 0 e cel mai apăsat, fiecare nivel de sub el se apropie de fundalul rândurilor. Așa ierarhia se citește fără ca nimeni să aleagă cinci culori, iar un nivel adăugat nu cere un slot nou de paletă.

Peste asta, trei straturi de control, în ordinea puterii: culoarea/fontul fixate PE NIVEL bat tema; evenimentul `GroupFormatting` bate nivelul (acolo se colorează grupul care iese din tipar — luna cu depășire pe roșu); iar sub o schemă ÎNTUNECATĂ culorile fixate se ignoră, exact ca la benzile grilei (regula din 0028-03: la întuneric contrastul bate preferința). **Fontul fixat rămâne al operatorului în orice schemă** — un font nu devine ilizibil pe fundal închis.

### 7. Regula `ShouldSerialize` — verificată, nu presupusă

Fiecare proprietate de nivel care se poate seta din designer și nu poate purta `DefaultValue` (`Font`, `Color`, cele două înălțimi) are perechea `ShouldSerialize`/`Reset`, iar testele o verifică prin `TypeDescriptor`, adică pe drumul pe care merge chiar Visual Studio.

**Corecție probată:** o colecție `Content` raportează ÎNTOTDEAUNA `True` la serializare; designerul scrie elementele, iar colecția goală nu produce linii. Codul era corect; testul care cerea neserializarea lui `Groups` pe grila neatinsă era greșit. Înlocuit cu verificarea că `Groups` se comportă ca `Columns` (0025).

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

**Bancul** — `DataViewPlaygroundForm(.Designer).vb`: secțiune «Grupare», două combo-uri de nivel, agregate în antet/subsol, `CollapsedByDefault`, `CollapseAllGroups()`/`ExpandAllGroups()`, `FooterVisible`, numărul de grupuri în bara de info; coloanele `val*` au primit `ValueType = Number`
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

**Eșecuri PREEXISTENTE, verificate pe arbore curat** (`git stash`, aceleași cifre cu și fără felie — NU sunt ale acestei felii și nu au fost atinse): `KBot.App.Tests` 10, `KBot.Domain.Tests` 3, `KBot.Api.Tests` 1. Toate în jurul etichetei de revizie DDF și al diacriticelor din designer.

---

## Nerulat / rămas deschis

1. **NIMIC DIN FELIE N-A FOST VĂZUT PE ECRAN.** Aceeași situație ca tot slice-ul 0028. Bancul are acum comutatoarele, dar nu a fost pornit. Lucrurile care se judecă DOAR cu ochiul: nuanța pe adâncime a nivelurilor (dacă nivelul 2 chiar se distinge de nivelul 1), retragerea cumulativă, triunghiul de strângere, și cum arată benzile de grup sub schema Modern (degrade) și sub Dark.
2. **Dus-întorsul prin designerul Visual Studio nu a fost făcut.** Colecția `Groups` ar trebui să se editeze cu dialogul standard, iar un nivel neatins să nu scrie nicio linie în `.Designer.vb`. Testele verifică asta prin `TypeDescriptor` — drumul pe care merge designerul — dar nu ÎNSUȘI designerul. E aceeași limită consemnată la slice 0025.
3. **Niciun ecran real nu grupează încă.** Felia dă mecanismul; `IstoricView` și `PlatiView` sunt candidații evidenți (clasificații, respectiv luni), dar nu au fost atinse deliberat — sunt hotărâri de vedere, nu de control.
4. **Limită cunoscută, de la tastatură:** `Ctrl+Stânga` strânge grupul rândului curent și rândul rămâne selectat (ascuns), deci `Ctrl+Dreapta` îl redeschide. Corect. Dar nu există o cale de a MUTA selecția pe o bandă de grup — ea nu are index de model — deci nu se poate «naviga prin antete» de la tastatură. Nu e o scăpare, e limita modelului de selecție al grilei; ar cere o noțiune de «bandă curentă» separată de «celulă curentă».
5. **Costul de memorie al tabloului de benzi**, spus pe față: ~20 de octeți per rând vizibil, DOAR când grila e grupată. La 500.000 de rânduri grupate și complet desfăcute înseamnă ~10 MB. Pe o grilă negrupată e zero. Nu s-a măsurat pe date reale.
6. **Ruperea grupurilor se face pe TEXTUL AFIȘAT**, ordinea pe VALOAREA BRUTĂ. Pentru toate formatele obișnuite cele două sunt de acord (afișarea e monotonă), deci grupurile ies contigue. Un format NUMIT ne-monoton (ex. «Yes/No» peste o coloană `Number` cu valori negative) ar putea da două grupuri cu același titlu. Nu s-a întâlnit; se rezolvă punând `ValueType = Boolean`.
7. **Nu există un raport Access de referință.** Exportul de la `C:\AVACONT\FX_System_Export` are doar `FORMS / QUERIES / TABLES / MODULES` — **nicio secțiune `REPORTS`**. Felia implementează SEMANTICA de grupare a rapoartelor Access (bandă de antet, bandă de subsol cu `=Sum()`, precedența cheilor de grupare la sortare), nu un raport anume. Dacă operatorul are în minte un raport concret, el trebuie exportat ca să se poată potrivi la literă.
