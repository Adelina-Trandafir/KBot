# SLICE-0028 — `KBotDataView`: subsol cu agregate pe tip, benzi tematizate, etichetă de celulă, buton de strângere

Cerere de operator (5 puncte), nu un plan preexistent. Numărul de felie: **0028** — cel declarat
liber în `KBOT_STATUS.md` (0027-03 era deja luat de „valorile de designer ale arborelui în cele
patru vederi”).

## Ce s-a schimbat și de ce

### 1. Agregate pe COLOANĂ, deschise după tipul VALORII

`KBotAggregate` avea trei valori (Sum / Count / Average) și nicio noțiune de „ce fel de date ține
coloana”: `KBotColumnType` spune doar cum se PICTEAZĂ celula (Text / Combo / CheckBox / …), iar
coloanele de sume din DdfView și PlatiView sunt „Text”. Deci nimic nu împiedica un `Sum` pe o
coloană de nume — s-ar fi calculat 0, tăcut.

- **`KBotValueType`** (nou): `Text` / `Number` / `DateTime` / `Boolean`.
- **`KBotAggregate`** lărgit la 12 valori: peste cele trei vechi — `Min`, `Max`, `CountDistinct`,
  `CountEmpty`, `CountTrue`, `CountFalse`, `First`, `Last`.
- **`KBotAggregateRules`** (nou) — SINGURUL loc unde scrie ce se poate aduna și ce se poate doar
  număra. O citesc trei consumatori: setterul agregatului, convertorul de designer și calculul.
- **`KBotAggregateConverter`** (nou) — grila de proprietăți din Visual Studio oferă DOAR agregatele
  valabile pentru `ValueType`-ul coloanei editate.
- O pereche nepermisă **ARUNCĂ** `ArgumentException` (regula casei: niciun no-op tăcut), pe toate
  cele trei drumuri: setterul `Aggregate`, setterul `ValueType` (n-ar orfaniza agregatul curent) și
  intrarea unei coloane construite liber în grilă (`OnColumnsChanged`).
- Verificarea se AMÂNĂ cât timp grila e în `BeginInit`/`EndInit` sau în designer: designerul emite
  proprietățile în ordinea lui, deci `Aggregate` poate ajunge înaintea lui `ValueType`, iar o
  excepție acolo ar însemna un formular care nu se mai deschide DELOC. Perechea așezată se verifică
  la `EndInit` (`ValidateColumns`).
- `Min`/`Max` compară în tipul coloanei (numeric vs. calendaristic), nu alfabetic.
  `CountDistinct` numără pe TEXTUL AFIȘAT, deci prin `FormatString` — două `DateTime` cu ore
  diferite, afișate `dd.MM.yyyy`, sunt o singură zi.
- `Min`/`Max` NU se oferă la `Text`: „cel mai mic alfabetic” s-ar confunda cu `First`.

**Redenumire (API vechi scos, cum s-a cerut):** `ShowTotalsRow` → **`FooterVisible`**,
`TotalsRowHeight` → **`FooterHeight`**, `RecomputeTotals` → `RecomputeFooter`, `DebugTotalsText` →
`DebugFooterText`, `KBotDataView.Totals.vb` → `KBotDataView.Footer.vb`. Vocabularul e acum același
cu al subsolului arborelui (0027-02). Toate cele 5 locuri de apel au fost migrate.

### 1b. `DecimalPlaces` pe coloanele numerice (cerere ulterioară, aceeași felie)

`KBotDataColumn.DecimalPlaces` — câte zecimale se AFIȘEAZĂ. Implicit `-1` (`NoDecimalPlaces`) =
nefixat, adică exact comportamentul de dinainte; plafon 15 (`MaxDecimalPlaces`, limita lui
`Math.Round`), peste el `ArgumentOutOfRangeException`. Orice negativ se normalizează la -1, ca
starea „gol” să aibă o singură formă.

- **Rotunjire NORMALĂ, nu cea implicită din .NET.** `Math.Round(2.5)` dă **2**, fiindcă rotunjește
  „la par”. Aici se folosește `MidpointRounding.AwayFromZero`, deci 2,5 ⇒ 3 și −2,5 ⇒ −3 — ce
  învață omul la școală și ce se așteaptă într-o notă contabilă. E și motivul pentru care testul de
  mijloc de interval e cel mai important din suită: o implementare scrisă fără paramentrul acela
  trece toate celelalte teste și cade abia în fața operatorului.
- `Decimal` se rotunjește CA `Decimal` (nu prin `Double`): altfel s-ar pierde exact precizia pentru
  care cineva a ales `Decimal` la o coloană de bani.
- Fără `FormatString`, numărul de zecimale devine ȘI formatul (`F2` ⇒ «2,50», nu «2,5»), altfel
  rotunjirea ar fi invizibilă. Cu `FormatString`, formatul rămâne al lui — dar vede valoarea deja
  rotunjită.
- **Subsolul adună ce se VEDE.** Sum/Average/Min/Max lucrează pe valorile rotunjite, iar rezultatul
  se rotunjește și el. Trei celule afișate «0,34» dau în subsol **1,02**, nu 1,01 (suma brută a lui
  0,338 × 3): un total care nu iese la adunare pe pagină e, pentru cine citește pagina, o greșeală
  de calcul — degeaba e „mai exact”. `CountDistinct` numără tot pe textul afișat, deci două valori
  care se rotunjesc la fel sunt una singură.
- **Valoarea STOCATĂ nu se atinge** — rotunjirea e o regulă de afișare, ca un commit spre server să
  nu trimită înapoi un număr ciuntit de grilă.
- Se poate fixa doar pe `ValueType = Number`; altfel ARUNCĂ, pe aceleași trei drumuri ca perechea
  tip × agregat (setterul zecimalelor, setterul tipului, intrarea unei coloane libere în grilă) și
  cu aceeași amânare în designer/`BeginInit`.
- **Niciuna dintre vederile existente nu a fost trecută pe `DecimalPlaces`** — proprietatea e
  opt-in, iar DdfView/PlatiView își păstrează `FormatString = "N2"`. Trecerea lor ar schimba
  semantica totalului (suma valorilor rotunjite), deci e o decizie de operator, nu una tehnică.

### 2. Benzile (antet + subsol) se tematizează cu adevărat

- Fontul antetului era **scris în control** — `New Font("Segoe UI Semibold", …)` — deci schimbarea
  schemei nu ajungea la el. Acum fontul benzilor vine din `Style.BaseFontName`/`BaseFontSize` ale
  schemei active (Modern ⇒ «Segoe UI Variable Text» 9pt), în varianta semibold dacă familia are una
  INSTALATĂ. Verificarea se face în lista familiilor instalate, nu construind fontul și prinzând
  excepția: GDI+ nu aruncă pentru o familie necunoscută, ci cade tăcut pe alta.
- Subsolul are **roluri proprii** de culoare (`_cFooter*`), nu mai împrumută sloturile antetului:
  fundalul lui e spălat spre accent, ca ochiul să vadă din prima că jos e altceva decât un rând.
- **Degradeul benzilor** se aprinde din STILUL schemei (`ButtonRender = ModernOwnerDrawn` sau
  `CornerRadius > 0`), nu dintr-un `if Modern` scris în control.
- Șase proprietăți noi de designer: `HeaderBackColor` / `HeaderForeColor` / `HeaderFont` și
  perechile lor de subsol. `Color.Empty` / `Nothing` = „din temă”; fiecare are perechea
  `ShouldSerialize`/`Reset`, altfel Visual Studio ar îngheța valoarea REZOLVATĂ în `.Designer.vb`
  și tema n-ar mai ajunge niciodată la bandă (capcana din regulile casei).

### 3. În subsol se despart doar coloanele agregate

`FooterDrawsRightSeparator` / `FooterDrawsLeftSeparator` — funcții PURE, tocmai ca regula să nu fie
verificabilă doar cu ochiul. O coloană fără agregat nu primește nicio linie; una agregată rămâne
închisă între două linii, iar muchia comună a două coloane agregate vecine se desenează o
singură dată.

### 4. Etichetă plutitoare pentru celulele al căror text nu încape

- `KBotCellTooltipOptions` (public, expus prin `KBotDataView.CellTooltip`, editabil în grila de
  proprietăți) + `KBotCellTooltipWindow` (fereastra, `Friend`).
- Fereastra e sora lui `TreeNodeFlyout`: `WS_EX_NOACTIVATE` + `ShowWithoutActivation`,
  `WS_EX_TOOLWINDOW`, `HTTRANSPARENT` pe `WM_NCHITTEST`. Ultima parte nu e cosmetică: eticheta poate
  atinge chiar celula peste care stă cursorul, deci fără ea hover-ul s-ar pierde când apare
  fereastra, ceea ce ar ascunde-o, ceea ce ar readuce hover-ul… la infinit.
- NU e un `ToolTip` WinForms: acela nu se poate tematiza, rotunji, și nu poate purta fontul celulei.
- Apare **doar când chiar nu încape**: se măsoară textul deja formatat (prin aceeași trecere de
  `CellFormatting` ca pictarea, pe argumentele de interogare) cu fontul lui, față de lățimea utilă
  a celulei — minus zona chevronului la coloanele Combo.
- Decizia e o funcție pură (`CellTooltipTextFor`), deci verificabilă fără ecran.

### 5. Buton de strângere, ca la arbore și la bara de navigare — dar cu axă

Aceleași nume și aceleași reguli ca la `AdvancedTreeControl`/`KBotNavList`: `CollapseButton`,
`CollapseButtonSize/Position`, imaginile celor două stări, `MinimumCollapsedWidth` (100),
`Collapsed` (stare de RULARE, neserializată), `ToggleCollapse()`, `CollapsedChanged`,
`HostOwnsWidth`, `ExpandedWidth`.

Ce e în plus: **`CollapseDirection`**. O grilă andocată `Fill` într-o vedere nu câștigă nimic
dintr-o fâșie verticală de 100px, deci pe `Vertical` corpul dispare cu totul și rămân cele două
benzi — adică agregatele, exact ce vrei să vezi când ai închis lista. `HostOwnsHeight` /
`ExpandedHeight` / `CollapsedHeight` sunt perechile verticale.

Regula gazdei se păstrează întocmai: dacă dimensiunea de pe axa aleasă nu e a noastră (`Dock`, ori
ancorare pe amândouă laturile), NU scriem nimic — schimbăm starea, ridicăm `CollapsedChanged`, iar
gazda își mută splitter-ul. Butonul stă în banda de subsol și **latura pe care stă e a lui**
(`FooterContentRect`): textul agregatelor se decupează înaintea lui, iar X-urile coloanelor rămân
cele din antet, ca subsolul să nu se desprindă de coloanele lui.

## Fișiere atinse

**Noi (`src/KBot.Controls/DataView/`):** `KBotValueType.vb`, `KBotAggregateRules.vb` (+ convertorul),
`KBotCollapseDirection.vb`, `KBotFooterButtonPosition.vb`, `KBotCellTooltip.vb`,
`KBotDataView.Footer.vb` (înlocuiește `.Totals.vb`), `KBotDataView.Collapse.vb`,
`KBotDataView.Tooltip.vb`.

**Modificate:** `KBotAggregate.vb`, `KBotDataColumn.vb`, `KBotDataView.vb`, `.Theming.vb`,
`.Painting.vb`, `.Layout.vb`, `.Input.vb`, `.AutoSize.vb`, `.Designer.vb` · `DdfView.vb`,
`PlatiView.vb`, `DdfView.Designer.vb`, `SumarView.Designer.vb`, `PlatiView.Designer.vb`,
`DdfFileBrowser.Designer.vb`, `XfaXmlPreview.Designer.vb` · `DataViewHarnessForm(.Designer).vb`.

**Șters:** `KBotDataView.Totals.vb`.

**Teste:** `KBotDataViewTotalsTests.vb` → `KBotDataViewFooterTests.vb` (redenumit + `ValueType`
adăugat unde se cere `Sum`/`Average`); noi: `KBotDataViewAggregateRulesTests.vb`,
`KBotDataViewCollapseTests.vb`, `KBotDataViewCellTooltipTests.vb`,
`KBotDataViewBandThemingTests.vb`, `KBotDataViewDecimalPlacesTests.vb`. Actualizat:
`KBotDataColumnDesignerTests.vb`.

## Rezultatele testelor

- `dotnet build KBot.sln` — **0 erori**; 1 avertisment **PREEXISTENT**: `MSB3825` pe
  `DdfView.resx` (lista de imagini a operatorului, deserializată prin `BinaryFormatter` — notat deja
  în 0027-03). `KBot.Controls` singur compilează cu **0 avertismente**.
- `KBot.Controls.Tests` — **586 passed / 0 failed** (501 înainte; **85 de teste noi**, dintre care
  26 pentru `DecimalPlaces`).
- Restul soluției, neschimbat față de starea de dinainte: `Theming` 61, `DevHarness` 170, `Xfa` 39,
  `Common` 14, `LocalStore` 1 — toate verzi.

**Eșecuri PREEXISTENTE, neatinse de felia asta** (12, aceleași înainte și după):
- `KBot.Domain.Tests` 3 + `KBot.Api.Tests` 1 — `EtichetaRevizie` (umplerea cu spații).
- `KBot.App.Tests` 8 — dintre care 2 pică și pe `HEAD` curat (`AdobeSettings_LiveOnTheDocumentPage`,
  `Leaf_CaptionPadsRevisionNumberWithSpaces`), iar 6 cad din redenumirea NECOMISĂ din arborele de
  lucru `tree_NodeMouseUp` → `Tree_NodeMouseUp`: testele caută metoda prin reflecție, iar
  `GetMethod` e case-sensitive. Verificat rulând suita `KBot.App.Tests` într-un worktree curat pe
  `HEAD` (2 eșecuri acolo, față de 8 în arborele de lucru).

## Rămas neverificat / amânat

- **NIMIC DIN FELIE N-A FOST VĂZUT PE ECRAN.** Tot ce e vizual (degradeul benzilor pe Modern,
  fontul de schemă, liniile verticale doar la coloanele agregate, eticheta de celulă, unghiul
  butonului) e verificat doar prin funcții pure și teste headless.
- Pentru proba vizuală, `DataViewHarnessForm` a primit: două comutatoare noi („Buton de strângere”,
  „Strânge pe verticală”), un jurnal pe `CollapsedChanged`, și o coloană «Denumire» cu `MaxWidth`
  deliberat prea mic (singurul fel în care se poate VEDEA eticheta) agregată pe `CountDistinct`.
  **Bancul nu a fost rulat.**
- Grila e andocată în harness, deci acolo `HostOwnsWidth`/`HostOwnsHeight` sunt amândouă True:
  strângerea se vede doar prin eveniment + benzi, nu prin redimensionare. Contractul „gazda mută
  splitter-ul” e verificat în teste, dar nu într-o gazdă reală (ca `MainForm.tree_CollapsedChanged`
  pentru arbore). **Nicio vedere nu folosește încă butonul de strângere al grilei.**
- `MinimumCollapsedWidth` se aplică pe orizontală; pe verticală înălțimea strânsă e chiar suma
  benzilor (`CollapsedHeight`) și nu e configurabilă — n-a fost cerută.
- Eticheta de celulă acoperă coloanele `Text` și `Combo` (cele care poartă text propriu). Antetul și
  subsolul NU primesc etichetă când textul lor se taie — nu s-a cerut.
- `DecimalPlaces` NU intervine la EDITARE: editorul flotant arată și comite valoarea tastată, cu
  toate zecimalele ei; abia afișarea o rotunjește. Dacă se dorește ca un commit să și ciuntească
  valoarea (data-entry pe bani), e o decizie separată — ar schimba datele, nu doar imaginea lor.
- Vederile existente nu au fost trecute pe `DecimalPlaces` (vezi §1b): păstrează `FormatString`,
  deci totalurile lor rămân sume ale valorilor BRUTE, ca până acum.
