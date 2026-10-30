# SLICE-0028 — Istoricul DataView, filtrării și temelor

Consolidare D001-10: baza și etapele 02…09 sunt reunite cronologic, cu contracte, fișiere, rezultate și restanțe păstrate. Probe și comenzi sunt istorice; nu se execută prin această consolidare.

Etapa 03 introduce meniul desenat și filtrarea pe coloană, 06 mută meniul în designer, 08 consemnează intervențiile operatorului. Acceptarea vizuală a operatorului din 08 nu se extinde la restul lucrării. Reguli istorice de sortare/validare nu înlocuiesc AGENTS actual.

[0028-baza](#slice-0028-baza) · [0028-02](#slice-0028-02) · [0028-03](#slice-0028-03) · [0028-04](#slice-0028-04) · [0028-05](#slice-0028-05) · [0028-06](#slice-0028-06) · [0028-07](#slice-0028-07) · [0028-08](#slice-0028-08) · [0028-09](#slice-0028-09)

## SLICE-0028 — `KBotDataView`: subsol cu agregate pe tip, benzi tematizate, etichetă de celulă, buton de strângere <a id="slice-0028-baza"></a>

Cerere de operator (5 puncte), nu un plan preexistent. Numărul de felie: **0028** — cel declarat liber în `KBOT_STATUS.md` (0027-03 era deja luat de „valorile de designer ale arborelui în cele patru vederi”).

### Ce s-a schimbat și de ce

#### 1. Agregate pe COLOANĂ, deschise după tipul VALORII

`KBotAggregate` avea Sum/Count/Average fără tipul valorii. `KBotColumnType` descrie numai pictarea (Text/Combo/CheckBox/…); sumele DdfView/PlatiView sunt „Text”. Astfel, `Sum` pe nume ar fi produs silențios 0.

- **`KBotValueType`** (nou): `Text` / `Number` / `DateTime` / `Boolean`.
- **`KBotAggregate`** lărgit la 12 valori: peste cele trei vechi — `Min`, `Max`, `CountDistinct`, `CountEmpty`, `CountTrue`, `CountFalse`, `First`, `Last`.
- **`KBotAggregateRules`** (nou) — SINGURUL loc unde scrie ce se poate aduna și ce se poate doar număra. O citesc trei consumatori: setterul agregatului, convertorul de designer și calculul.
- **`KBotAggregateConverter`** (nou) — grila de proprietăți din Visual Studio oferă DOAR agregatele valabile pentru `ValueType`-ul coloanei editate.
- O pereche nepermisă **ARUNCĂ** `ArgumentException` (regula casei: niciun no-op tăcut), pe toate cele trei drumuri: setterul `Aggregate`, setterul `ValueType` (n-ar orfaniza agregatul curent) și intrarea unei coloane construite liber în grilă (`OnColumnsChanged`).
- Verificarea se amână în designer/`BeginInit`…`EndInit`: designerul poate emite `Aggregate` înaintea lui `ValueType`, iar o excepție ar bloca deschiderea formularului. Perechea completă se verifică la `EndInit` (`ValidateColumns`).
- `Min`/`Max` compară în tipul coloanei (numeric vs. calendaristic), nu alfabetic. `CountDistinct` numără pe TEXTUL AFIȘAT, deci prin `FormatString` — două `DateTime` cu ore diferite, afișate `dd.MM.yyyy`, sunt o singură zi.
- `Min`/`Max` NU se oferă la `Text`: „cel mai mic alfabetic” s-ar confunda cu `First`.

**Redenumire (API vechi scos, cum s-a cerut):** `ShowTotalsRow` → **`FooterVisible`**, `TotalsRowHeight` → **`FooterHeight`**, `RecomputeTotals` → `RecomputeFooter`, `DebugTotalsText` → `DebugFooterText`, `KBotDataView.Totals.vb` → `KBotDataView.Footer.vb`. Vocabularul e acum același cu al subsolului arborelui (0027-02). Toate cele 5 locuri de apel au fost migrate.

#### 1b. `DecimalPlaces` pe coloanele numerice (cerere ulterioară, aceeași felie)

`KBotDataColumn.DecimalPlaces` — câte zecimale se AFIȘEAZĂ. Implicit `-1` (`NoDecimalPlaces`) = nefixat, adică exact comportamentul de dinainte; plafon 15 (`MaxDecimalPlaces`, limita lui `Math.Round`), peste el `ArgumentOutOfRangeException`. Orice negativ se normalizează la -1, ca starea „gol” să aibă o singură formă.

- **`MidpointRounding.AwayFromZero`:** 2,5 ⇒ 3 și −2,5 ⇒ −3, față de `Math.Round(2.5) = 2` implicit, la par. Proba mijlocului de interval este esențială: omiterea parametrului trece celelalte teste, dar produce rezultatul contabil neașteptat.
- `Decimal` se rotunjește CA `Decimal` (nu prin `Double`): altfel s-ar pierde exact precizia pentru care cineva a ales `Decimal` la o coloană de bani.
- Fără `FormatString`, numărul de zecimale devine ȘI formatul (`F2` ⇒ «2,50», nu «2,5»), altfel rotunjirea ar fi invizibilă. Cu `FormatString`, formatul rămâne al lui — dar vede valoarea deja rotunjită.
- **Subsolul adună valorile afișate:** Sum/Average/Min/Max folosesc valorile rotunjite și rotunjesc rezultatul. Trei «0,34» dau **1,02**, nu 1,01 din 0,338 × 3. `CountDistinct` folosește textul afișat: valori rotunjite la fel sunt una singură.
- **Valoarea STOCATĂ nu se atinge** — rotunjirea e o regulă de afișare, ca un commit spre server să nu trimită înapoi un număr ciuntit de grilă.
- Se poate fixa doar pe `ValueType = Number`; altfel ARUNCĂ, pe aceleași trei drumuri ca perechea tip × agregat (setterul zecimalelor, setterul tipului, intrarea unei coloane libere în grilă) și cu aceeași amânare în designer/`BeginInit`.
- **Niciuna dintre vederile existente nu a fost trecută pe `DecimalPlaces`** — proprietatea e opt-in, iar DdfView/PlatiView își păstrează `FormatString = "N2"`. Trecerea lor ar schimba semantica totalului (suma valorilor rotunjite), deci e o decizie de operator, nu una tehnică.

#### 2. Benzile (antet + subsol) se tematizează cu adevărat

- Fontul fix `New Font("Segoe UI Semibold", …)` este înlocuit pentru benzi cu `Style.BaseFontName`/`BaseFontSize` (Modern: «Segoe UI Variable Text» 9pt), semibold numai dacă este instalat. Se consultă lista familiilor: construirea fontului nu probează existența, GDI+ alegând silențios altă familie.
- Subsolul are **roluri proprii** de culoare (`_cFooter*`), nu mai împrumută sloturile antetului: fundalul lui e spălat spre accent, ca ochiul să vadă din prima că jos e altceva decât un rând.
- **Degradeul benzilor** se aprinde din STILUL schemei (`ButtonRender = ModernOwnerDrawn` sau `CornerRadius > 0`), nu dintr-un `if Modern` scris în control.
- Șase proprietăți de designer: `HeaderBackColor` / `HeaderForeColor` / `HeaderFont` și perechile de subsol. `Color.Empty` / `Nothing` = temă; `ShouldSerialize`/`Reset` evită serializarea valorii rezolvate în `.Designer.vb`, care ar bloca schimbarea temei.

#### 3. În subsol se despart doar coloanele agregate

`FooterDrawsRightSeparator` / `FooterDrawsLeftSeparator` — funcții PURE, tocmai ca regula să nu fie verificabilă doar cu ochiul. O coloană fără agregat nu primește nicio linie; una agregată rămâne închisă între două linii, iar muchia comună a două coloane agregate vecine se desenează o singură dată.

#### 4. Etichetă plutitoare pentru celulele al căror text nu încape

- `KBotCellTooltipOptions` (public, expus prin `KBotDataView.CellTooltip`, editabil în grila de proprietăți) + `KBotCellTooltipWindow` (fereastra, `Friend`).
- Ca `TreeNodeFlyout`: `WS_EX_NOACTIVATE` + `ShowWithoutActivation`, `WS_EX_TOOLWINDOW`, `HTTRANSPARENT` pe `WM_NCHITTEST`. Transparența la hit-test împiedică alternarea infinită hover/ascundere când eticheta acoperă celula cursorului.
- NU e un `ToolTip` WinForms: acela nu se poate tematiza, rotunji, și nu poate purta fontul celulei.
- Apare **doar când chiar nu încape**: se măsoară textul deja formatat (prin aceeași trecere de `CellFormatting` ca pictarea, pe argumentele de interogare) cu fontul lui, față de lățimea utilă a celulei — minus zona chevronului la coloanele Combo.
- Decizia e o funcție pură (`CellTooltipTextFor`), deci verificabilă fără ecran.

#### 5. Buton de strângere, ca la arbore și la bara de navigare — dar cu axă

Aceleași nume și aceleași reguli ca la `AdvancedTreeControl`/`KBotNavList`: `CollapseButton`, `CollapseButtonSize/Position`, imaginile celor două stări, `MinimumCollapsedWidth` (100), `Collapsed` (stare de RULARE, neserializată), `ToggleCollapse()`, `CollapsedChanged`, `HostOwnsWidth`, `ExpandedWidth`.

Ce e în plus: **`CollapseDirection`**. O grilă andocată `Fill` într-o vedere nu câștigă nimic dintr-o fâșie verticală de 100px, deci pe `Vertical` corpul dispare cu totul și rămân cele două benzi — adică agregatele, exact ce vrei să vezi când ai închis lista. `HostOwnsHeight` / `ExpandedHeight` / `CollapsedHeight` sunt perechile verticale.

Regula gazdei se păstrează întocmai: dacă dimensiunea de pe axa aleasă nu e a noastră (`Dock`, ori ancorare pe amândouă laturile), NU scriem nimic — schimbăm starea, ridicăm `CollapsedChanged`, iar gazda își mută splitter-ul. Butonul stă în banda de subsol și **latura pe care stă e a lui** (`FooterContentRect`): textul agregatelor se decupează înaintea lui, iar X-urile coloanelor rămân cele din antet, ca subsolul să nu se desprindă de coloanele lui.

### Fișiere atinse

**Noi (`src/KBot.Controls/DataView/`):** `KBotValueType.vb`, `KBotAggregateRules.vb` (+ convertorul), `KBotCollapseDirection.vb`, `KBotFooterButtonPosition.vb`, `KBotCellTooltip.vb`, `KBotDataView.Footer.vb` (înlocuiește `.Totals.vb`), `KBotDataView.Collapse.vb`, `KBotDataView.Tooltip.vb`.

**Modificate:** `KBotAggregate.vb`, `KBotDataColumn.vb`, `KBotDataView.vb`, `.Theming.vb`, `.Painting.vb`, `.Layout.vb`, `.Input.vb`, `.AutoSize.vb`, `.Designer.vb` · `DdfView.vb`, `PlatiView.vb`, `DdfView.Designer.vb`, `SumarView.Designer.vb`, `PlatiView.Designer.vb`, `DdfFileBrowser.Designer.vb`, `XfaXmlPreview.Designer.vb` · `DataViewHarnessForm(.Designer).vb`.

**Șters:** `KBotDataView.Totals.vb`.

**Teste:** `KBotDataViewTotalsTests.vb` → `KBotDataViewFooterTests.vb` (redenumit + `ValueType` adăugat unde se cere `Sum`/`Average`); noi: `KBotDataViewAggregateRulesTests.vb`, `KBotDataViewCollapseTests.vb`, `KBotDataViewCellTooltipTests.vb`, `KBotDataViewBandThemingTests.vb`, `KBotDataViewDecimalPlacesTests.vb`. Actualizat: `KBotDataColumnDesignerTests.vb`.

### Rezultatele testelor

- `dotnet build KBot.sln` — **0 erori**; 1 avertisment **PREEXISTENT**: `MSB3825` pe `DdfView.resx` (lista de imagini a operatorului, deserializată prin `BinaryFormatter` — notat deja în 0027-03). `KBot.Controls` singur compilează cu **0 avertismente**.
- `KBot.Controls.Tests` — **586 passed / 0 failed** (501 înainte; **85 de teste noi**, dintre care 26 pentru `DecimalPlaces`).
- Restul soluției, neschimbat față de starea de dinainte: `Theming` 61, `DevHarness` 170, `Xfa` 39, `Common` 14, `LocalStore` 1 — toate verzi.

**Eșecuri PREEXISTENTE, neatinse de felia asta** (12, aceleași înainte și după):
- `KBot.Domain.Tests` 3 + `KBot.Api.Tests` 1 — `EtichetaRevizie` (umplerea cu spații).
- `KBot.App.Tests` 8 — dintre care 2 pică și pe `HEAD` curat (`AdobeSettings_LiveOnTheDocumentPage`, `Leaf_CaptionPadsRevisionNumberWithSpaces`), iar 6 cad din redenumirea NECOMISĂ din arborele de lucru `tree_NodeMouseUp` → `Tree_NodeMouseUp`: testele caută metoda prin reflecție, iar `GetMethod` e case-sensitive. Verificat rulând suita `KBot.App.Tests` într-un worktree curat pe `HEAD` (2 eșecuri acolo, față de 8 în arborele de lucru).

### Rămas neverificat / amânat

- **NIMIC DIN FELIE N-A FOST VĂZUT PE ECRAN.** Tot ce e vizual (degradeul benzilor pe Modern, fontul de schemă, liniile verticale doar la coloanele agregate, eticheta de celulă, unghiul butonului) e verificat doar prin funcții pure și teste headless.
- Pentru proba vizuală, `DataViewHarnessForm` a primit: două comutatoare noi („Buton de strângere”, „Strânge pe verticală”), un jurnal pe `CollapsedChanged`, și o coloană «Denumire» cu `MaxWidth` deliberat prea mic (singurul fel în care se poate VEDEA eticheta) agregată pe `CountDistinct`. **Bancul nu a fost rulat.**
- Grila e andocată în harness, deci acolo `HostOwnsWidth`/`HostOwnsHeight` sunt amândouă True: strângerea se vede doar prin eveniment + benzi, nu prin redimensionare. Contractul „gazda mută splitter-ul” e verificat în teste, dar nu într-o gazdă reală (ca `MainForm.tree_CollapsedChanged` pentru arbore). **Nicio vedere nu folosește încă butonul de strângere al grilei.**
- `MinimumCollapsedWidth` se aplică pe orizontală; pe verticală înălțimea strânsă e chiar suma benzilor (`CollapsedHeight`) și nu e configurabilă — n-a fost cerută.
- Eticheta de celulă acoperă coloanele `Text` și `Combo` (cele care poartă text propriu). Antetul și subsolul NU primesc etichetă când textul lor se taie — nu s-a cerut.
- `DecimalPlaces` NU intervine la EDITARE: editorul flotant arată și comite valoarea tastată, cu toate zecimalele ei; abia afișarea o rotunjește. Dacă se dorește ca un commit să și ciuntească valoarea (data-entry pe bani), e o decizie separată — ar schimba datele, nu doar imaginea lor.
- Vederile existente nu au fost trecute pe `DecimalPlaces` (vezi §1b): păstrează `FormatString`, deci totalurile lor rămân sume ale valorilor BRUTE, ca până acum.

## SLICE-0028-02 — DataView: titlu de subsol, pictograme de coloană, `Format` în stil Access <a id="slice-0028-02"></a>

Al doilea pas al feliei 0028 (`KBotDataView`). Trei cereri ale operatorului, toate pe grilă.

### Ce s-a schimbat și de ce

#### 1. Titlul din subsol (`FooterCaption` + pictogramă)

Spațiul din stânga primei coloane agregate primește `FooterCaption`, `FooterLeftIcon`, `FooterIconSize`, ca la arbore, plus `FooterLeftIconClicked` și `FooterLeftIconHoverColor` (gol = temă).

**Zona se oprește la PRIMA coloană agregată**, evitând confundarea titlului cu eticheta unei sume; fără agregate ocupă toată banda. Pornește din `FooterContentRect`, care exclude deja colțul butonului de strângere, inclusiv când acesta este în stânga.

Pictograma se desenează doar dacă încape ÎNTREAGĂ în zonă, și tot atunci se și apasă: ce nu se vede nu se poate apăsa.

#### 2. Pictograme pe fiecare coloană, în antet (`HeaderLeftIcon` / `HeaderRightIcon`)

Coloana are `HeaderLeftIcon` / `HeaderRightIcon`, cu `HeaderLeftIconSize` / `HeaderRightIconSize` (implicit 16×16). Numai dreapta este buton: `HeaderRightIconClicked` trimite cheia coloanei și dreptunghiul în `KBotColumnEventArgs`, pentru meniul de sub pictogramă; `HeaderRightIconHoverColor` gol folosește tema. Stânga este semn inert, fără eveniment sau hover.

**Ordinea sacrificiului la îngustare** (cerută explicit): titlul se taie ÎNTÂI, cu elipsă, până la lățime zero; abia apoi cade pictograma din STÂNGA; cea din dreapta nu cade niciodată.

**Podeaua:** `KBotDataColumn.EffectiveMinWidth = max(MinWidth, HeaderIconsWidth)`, cu `HeaderIconsWidth = 2×Pad + stânga + Gap + dreapta` (Pad 8, Gap 4, px logici). `ClampWidth`, `MeasureColumnToContent` și `ShrinkToFit` o respectă; bate `MaxWidth` pentru a evita suprapunerile. Setarea pictogramei limitează lățimea imediat.

Așezarea celor trei piese stă într-o funcție pură, `KBotDataView.ComputeHeaderCellLayout`, pe care o cheamă și pictarea, și hit-testul, și testele — o a doua formulă ar însemna o pictogramă desenată unde nu se apasă.

#### 3. `Format` — proprietatea numită, în stil Access

`KBotDataColumn.Format` (`KBotFormat`) oferă lista Access: General Number, Currency, Euro, Fixed, Standard, Percent, Scientific, General/Long/Medium/Short Date, Long/Medium/Short Time, Yes/No, True/False, On/Off. `KBotColumnFormat` traduce unic pentru celule și agregate, păstrând același format în coloană și subsol.

Două lucruri deliberate:

- **Formatul numit CITEȘTE valoarea**, nu doar o formatează: o coloană `ValueType.Text` care poartă numere (cazul DdfView/PlatiView) se scrie „Standard” corect. O valoare care nu se poate citi în tipul cerut cade pe calea obișnuită și tot se vede — nu se stinge într-o celulă goală.
- **`Format` / `FormatString` sunt exclusive:** setarea ambelor aruncă `ArgumentException`, amânată până la `EndInit` în inițializarea designerului, ca la `ValueType × Aggregate`. Nicio proprietate nu rămâne aparent activă dar ignorată.

Câte zecimale scrie un format vine din `DecimalPlaces` când e fixat, altfel 2 (implicitul Access), deci cele două proprietăți nu se contrazic. `AggregateFormatString` rămâne deasupra tuturor.

`ValidateAggregatePair` s-a redenumit `ValidateSettled` — verifică acum trei perechi așezate, nu una.

### Fișiere atinse

Noi:

`src/KBot.Controls/DataView/KBotFormat.vb` — enum-ul formatelor numite. `src/KBot.Controls/DataView/KBotColumnFormat.vb` — traducerea lor în text (singurul loc). `src/KBot.Controls/DataView/KBotDataView.HeaderIcons.vb` — așezare, hover, hit-test, eveniment. `src/KBot.Controls/DataView/KBotDataView.FooterCaption.vb` — zona, pictograma, evenimentul. `src/KBot.Controls/DataView/Events/KBotColumnEventArgs.vb` — cheie + dreptunghiul pictogramei. `tests/KBot.Controls.Tests/KBotDataViewHeaderIconsTests.vb` (14 teste); `tests/KBot.Controls.Tests/KBotDataViewColumnFormatTests.vb` (14 teste); `tests/KBot.Controls.Tests/KBotDataViewFooterCaptionTests.vb` (14 teste)

Modificate:

`KBotDataColumn.vb` — pictogramele + mărimile + culoarea de hover, `ClampWidth`, `HeaderIconsWidth`/`EffectiveMinWidth`, `Format`, `FormatString` devenit proprietate întreagă, `ValidateSettled`. `KBotDataView.vb` — `OnColumnFormatChanged`, apelurile `ValidateSettled`. `KBotDataView.Painting.vb` — `DrawHeaderCell` prin `DrawHeaderIcons`, titlul de subsol, `FormatValue` trece prin formatul numit; `ToBool` devenit `Friend`. `KBotDataView.Footer.vb` — agregatele trec prin formatul numit; `TryNumeric`/`TryDate` `Friend`. `KBotDataView.AutoSize.vb` — măsurarea adună pictogramele; podeaua e `EffectiveMinWidth`. `KBotDataView.Input.vb` — apăsarea/hover-ul pictogramelor de antet, cursorul de mână. `KBotDataView.Collapse.vb` — banda de subsol trimite apăsarea/hover-ul mai departe la pictogramă.

### Rezultatele testelor

- `dotnet build KBot.sln` — succes, 0 erori. Singurul warning e cel preexistent, `MSB3825` (BinaryFormatter în `DdfView.resx`), nelegat de felia asta.
- `KBot.Controls.Tests` — **627 trecute, 0 căzute** (42 noi).
- `KBot.Theming.Tests` 61, `KBot.Common.Tests` 14, `KBot.Xfa.Tests` 39, `KBot.DevHarness.Tests` 170 — toate verzi.
- **Căzute, dar PREEXISTENTE în arborele de lucru, nelegate de felia asta:** 1 în `KBot.Api.Tests` (`GetDdf_FormatsRevisionLabel_…` — `KBot.Api` nici măcar nu referă `KBot.Controls`) și 8 în `KBot.App.Tests` (`MainFormNavItemsTests.Designer_WroteLiteralDiacritics_…` plus 7 `DdfViewTests`, care caută prin reflecție un `tree_NodeMouseUp` ce nu mai există ca metodă în `DdfView.vb` — a rămas doar în eticheta de log de la linia 650). Nu au fost atinse aici.

### Rămas neverificat / amânat

- **Nimic nu s-a văzut pe ecran.** Ca la 0028: totul e verificat prin compilare + teste headless (`DrawToBitmap`). `TreePlaygroundForm`/`DataViewHarnessForm` NU au fost extinse cu piesele noi — harness-ul nu arată încă nici titlul de subsol, nici pictogramele de coloană, nici lista de formate. Ăsta e pasul următor dacă se vrea verdict vizual.
- **DPI.** Mărimile pictogramelor sunt în pixeli, ne-scalate (ca la arbore: ce se vede în designer e ce se desenează), pe când spațiile se scalează. Pe un ecran la 150% podeaua rămâne cu câțiva pixeli sub necesarul real al pictării — se plătește din text, care oricum se taie primul, niciodată din pictograme, deci degradarea e cea documentată, nu una care suprapune desene. Neverificat pe un ecran real la DPI ridicat.
- **`ShouldSerialize`/`Reset` neverificate empiric în designerul Visual Studio.** Perechile există pentru toate proprietățile noi de tip `Image`/`Size`/`Color` (regula casei: `Size` nu poate purta `<DefaultValue>`), dar verificarea cerută de CLAUDE.md — o coloană proaspăt creată să producă ZERO linii de proprietăți, probat prin `TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)` — n-a fost rulată pentru ele.
- **`Format` nu e folosit încă de nicio vedere.** `SumarView`/`DdfView`/`PlatiView` etc. rămân pe `FormatString`; nimic nu s-a migrat, ca să nu se amestece o schimbare de contract cu o rescriere de vederi.

## SLICE-0028-03 — DataView: filtrul de coloană în stil Access + suprascrierea pe întuneric <a id="slice-0028-03"></a>

Al treilea pas al feliei 0028 (`KBotDataView`). Două cereri ale operatorului:

> 1. A property called `ShowColumnFilter` as an icon with exposed props: Size, Icon and HoverColor. It should replicate the Access Filtering system on the native DataSheets (different options for Number, String and Dates). When pressed a custom popup menu should appear which will allow the user to filter the dgv. Also, like in Access the first options should be for Sorting. Should use the Custom Theming System.
> 2. The DGV should also use the theming system, but only for DarkMode. If DarkMode, it should ignore the colors set by me manually in the designer and set the normal DarkMode colors for the DGV (header, footer and cells). Also the scrollbars should use the Theming System.

Trei decizii au fost puse operatorului înainte de a scrie cod (răspunsurile lui în paranteze): cine filtrează efectiv rândurile (**grila însăși**), cât de fidel e meniul față de Access (**paritate completă**, cu submeniu de condiții) și cum se colorează barele de derulare (**bare native + tema întunecată a Windows-ului**, nu un control desenat de noi).

### 1. Motorul de sortare și filtrare (pur)

Patru tipuri noi, fără nicio referință la pictare sau la vreun control — aici scrie ce înseamnă „mai mic” și „conține”, o singură dată, pentru meniu, pentru dialog și pentru potrivire:

| Fișier | Rol |
|---|---|
| `KBotSortDirection.vb` | `None` / `Ascending` / `Descending`. `None` **nu** e „ascendent implicit”: e ordinea de încărcare, la care o vedere care primește rândurile deja ordonate de server trebuie să se poată întoarce. |
| `KBotFilterOperator.vb` | Vocabularul submeniurilor Access: Egal / Diferit / Conține / Începe cu / Se termină cu / Mai mic / Mai mare / Între / Este (ne)completat. |
| `KBotFilterEngine.vb` | Comparația tipizată, potrivirea condiției, oferta pe tip (`AllowedOperators`), textele de meniu. |
| `KBotColumnFilter.vb` | Filtrul unei coloane: lista de valori bifate **ȘI** condiția, aplicate împreună. |

Reguli care nu sunt evidente și de aceea sunt scrise în cod:

- **TEXT compară textul afișat**, pentru căutarea a ceea ce se vede; condițiile de mărime compară valoarea brută în tipul coloanei, evitând ordinea alfabetică «9» > «10».
- **Operand ilizibil = condiție INERTĂ**, cu toate rândurile afișate: «Mai mic decât <text>» nu golește grila.
- **Goalele întâi, necitibilele la urmă.** `Nothing`, `""` și spațiile sunt echivalente. Pe coloana numerică, numerele preced valorile necitibile, evitând egalități și sortare instabilă între cele două categorii.
- **`SelectedValues = Nothing` ≠ mulțime goală.** Primul înseamnă «toate», al doilea «niciuna».
- **Coloanele logice n-au submeniu de condiții** (`AllowedOperators` întoarce gol): două căsuțe în listă spun deja tot ce se poate spune despre o bifă.

### 2. Harta vedere ↔ model (`KBotDataView.Filtering.vb`)

Indicii de model și pozițiile de vedere sunt numerotări distincte, care nu se amestecă:

- **API public = indici de MODEL:** `Item(cheie, index)`, `CurrentRowIndex`, `CellClick`, `EnsureVisible`, `IsRowEnabled` își păstrează sensul. Sumar/Istoric/Plăți păstrează acești indici; renumerotarea la filtrare i-ar corupe silențios. Există probă dedicată.
- **Geometria lucrează în poziții de VEDERE** — `RowTop`, `FirstVisibleRow`, virtualizarea, hit-testul. `RowAtPoint` traduce o dată, la intrare; `RowTopForModel` traduce în sens invers pentru celule/editare/etichetă.

Alte două alegeri:

- **Reconstrucție leneșă** (`_viewDirty` + `EnsureView`): în încărcarea `BeginUpdate`/`EndUpdate`, harta rămasă în urmă ar putea indexa în gol la o pictare intermediară. `RecomputeFooter` devine **`RecomputeDerived`** și marchează harta murdară înaintea gărzii `BeginUpdate`.
- **Sortare stabilă**: `List.Sort` nu e stabilă, deci comparatorul cade la egalitate pe indexul de model. Fără asta, două rânduri egale și-ar schimba locul la fiecare resortare.

Ce s-a mutat pe rândurile vizibile, și de ce:

| Loc | Înainte | Acum |
|---|---|---|
| Agregatele din subsol | toate rândurile | doar cele care trec de filtre — un total care nu iese la adunare pe ecran e, pentru cine citește pagina, o greșeală de calcul |
| Înălțimea de derulat | `_rows.Count` | `ViewCount()` |
| Eșantionul de auto-dimensionare | primele N rânduri din model | primele N **vizibile** |
| Dungile alternante | index de model | poziție de vedere (rămân alternante după filtrare) |
| Săgețile sus/jos | pas în model | pas în vedere („rândul desenat sub acesta”) |
| `KBotAggregate.Last` | ultimul din model | ultimul de pe ecran (sub o sortare nu mai e același) |

Selecția: dacă rândul curent e filtrat afară, se abandonează editarea deschisă și selecția cade (`DropSelectionIfHidden`) — o selecție pe un rând invizibil ar muta săgețile și editarea în gol.

### 3. Butonul de filtrare — **pe coloană** (`KBotDataColumn` + `KBotDataView.FilterIcon.vb`)

Prima variantă a feliei avea steagul pe GRILĂ (`KBotDataView.ShowColumnFilter`) plus un comutator de excludere pe coloană. **Operatorul a cerut invers**: se hotărăște COLOANĂ CU COLOANĂ, în designer. Cele patru proprietăți stau acum pe `KBotDataColumn`, lângă celelalte pictograme de antet ale coloanei — `ShowColumnFilter` (implicit False), `ColumnFilterIcon`, `ColumnFilterIconSize` (16×16), `ColumnFilterHoverColor` — toate cu perechea `ShouldSerialize`/`Reset`, verificată în teste **prin `TypeDescriptor`**, calea pe care o ia chiar Visual Studio. `Filterable` și `FilterIconReserve` au DISPĂRUT: coloana știe acum totul singură, deci grila n-are ce să-i mai împingă.

**Interzis pe `Button` și `ProgressBar`.** Acelea nu poartă o valoare pe care s-o cauți — o celulă buton arată o comandă, una de progres o fracțiune desenată; o listă de valori distincte peste ele ar fi o listă de nimic, iar «sortează A → Z» n-ar avea ce ordona. Aprinderea **ARUNCĂ**, nu se stinge tăcut: un buton care nu apare unde a fost cerut e chiar no-op-ul tăcut interzis de regula casei. Perechea se apără din **amândouă** direcțiile (inclusiv la schimbarea tipului sub un filtru deja aprins), cu aceeași amânare în `BeginInit`/designer ca perechea `ValueType × Aggregate` — designerul emite proprietățile în ordinea LUI, iar o excepție în `InitializeComponent` ar închide formularul cu totul — și cu verificarea finală în `ValidateSettled`, la `EndInit`. `FilterEnabled` (steag ȘI tip) e ce citesc pictarea și hit-testul, ca în designer să nu se deseneze un buton pe care `EndInit` apoi îl refuză.

- **Stă mereu în același loc** — capătul din dreapta al celulei de antet — iar `HeaderRightIcon` se mută la stânga lui. Niciunul dintre cele două nu se sacrifică la îngustare: amândouă se apasă.
- **Podeaua de lățime**: `HeaderIconsWidth` numără acum trei piese și spațiile dintre ele, direct din starea coloanei.
- **Fără imagine dată, se DESENEAZĂ**: o pâlnie trasată din culoarea temei, **plină și în accent cât timp coloana chiar e filtrată**, doar conturată altfel. Așa antetul spune dintr-o privire care coloane sunt filtrate — jumătate din rostul semnului — fără nicio resursă imagine.

`SumarView.Designer.vb` fusese deja autorat în designerul VS cu forma veche (`grid.ShowColumnFilter = True` + `grid.ColumnFilterIcon = My.Resources.Resources.filter`); a fost mutat pe cele șapte coloane ale vederii, păstrând exact comportamentul autorat.

### 4. Meniul (`DataView/Filter/`)

`KBotFilterPopup` (+ `.Painting`) e o FEREASTRĂ desenată de noi, ca `CustomPopup` și din același motiv: un `ContextMenuStrip` cu un `CheckedListBox` în el ar rămâne două dreptunghiuri albe sub o schemă întunecată. Cele patru etaje Access, în ordinea Access:

1. sortarea (crescător / descrescător), cu textul luat din tip — «A → Z» pe text, «de la mic la mare» pe numere, «de la vechi la nou» pe date — și cu un semn pe sensul activ;
2. «Șterge filtrul din «Coloană»», stins cât timp coloana n-are filtru;
3. submeniul de condiții («Filtre text / numerice / de dată»), care deschide `KBotFilterConditionDialog` (dialog modal, cu `.Designer.vb` propriu, conform regulii casei);
4. caseta de căutare + lista de valori bifate cu «(Selectează tot)», apoi OK / Anulează.

Alegeri deliberate:

- **Sortarea se aplică IMEDIAT și închide meniul; filtrul se predă abia la OK.** Sortarea e o comandă, nu o alegere de confirmat — exact ca în Access. Popup-ul lucrează pe o COPIE a filtrului (`Clone`), deci «Anulează» și Esc nu lasă nimic în urmă.
- **«Toate bifate» = nicio restricție** (`SelectedValues` rămâne `Nothing`), altfel filtrul ar rămâne activ pentru totdeauna și antetul ar arăta coloana ca filtrată degeaba.
- **Lista distinctă se calculează peste rândurile care trec de filtrele CELORLALTE coloane**, dar ignoră filtrul coloanei ÎNSEȘI: altfel, odată bifată o valoare, celelalte ar dispărea din listă și nimeni nu s-ar mai putea răzgândi.
- **Golul are eticheta lui**, «(Necompletate)», dar în model rămâne textul vid — o etichetă strecurată în model ar filtra la fel și o coloană care chiar conține textul acela.
- Singurul control-copil adevărat e caseta de căutare (`KBotTextField`, `IThemedControl`), fiindcă text tastat cere un control care știe să primească taste. Popup-ul e el însuși `IThemedControl`, deci traversarea temei nu-i calcă interiorul — regula care a mușcat deja de două ori.
- `_suppressDeactivate` cât timp submeniul e sus: pierderea activării atunci nu înseamnă «s-a dat clic în altă parte», înseamnă «se uită la copil».

Grila expune și `ColumnFilterOpening` (anulabil) plus `ShowColumnFilterMenu(cheie)`, pentru o gazdă care vrea să lege comanda de un buton propriu sau să oprească meniul (o vedere care filtrează pe server).

### 5. Întunericul bate designerul + barele de derulare

`KBotDataView.Theming.vb`:

- În `IsDark`, `HeaderBackResolved` / `HeaderForeResolved` / `FooterBackResolved` / `FooterForeResolved` folosesc paleta, ignorând culorile de designer (`DarkOverridesDesignerColors`) pentru lizibilitate. Pe scheme luminoase rămâne regula culorii explicite. Culorile sunt păstrate și revin la ieșirea din întuneric.
- Se aplică numai CULORILOR. **Un font fixat rămâne al operatorului în orice schemă** — un font nu devine ilizibil pe fundal închis.
- Celulele luau deja culorile din paletă în orice schemă (`_cRowBack` / `_cCellText`), deci acolo n-a fost nimic de schimbat.
- Barele de derulare: `SetWindowTheme` cu «DarkMode_Explorer» / «Explorer», prin `NativeMethods.ApplyWindowTheme` — același truc pe care ThemeManager îl folosește deja pentru liste și pentru `KBotComboBox`. Se re-aplică la `OnHandleCreated`, fiindcă apelul cere un handle.

### 6. Bug găsit pe drum: tema nu ajungea în NICIO vedere

Raportat de operator: «even though in the Harness Form the color scheme gets applied, in the SumarView it doesn't». Nu e o problemă a lui SumarView, e una a motorului de teme, și lovea toate cele șase vederi reale deodată.

`ThemeManager.Traverse` se OPREA la primul `IThemedControl`:

```vb
If themed IsNot Nothing Then
    themed.ApplyTheme(_current)
    Return          ' ← și copiii lui nu mai vedeau niciodată schema
End If
```

Oprirea proteja interiorul unui control auto-tematizat de reguli GENERICE (`Panel`, `TextBox` intern din `KBotTextField`). Dar toate cele șase vederi reale implementează `IThemedControl` și conțin direct `Controls.Add(grid)`: grilele nu mai primeau schema și rămâneau implicite. Pe banc, grila direct pe formular primea tema, mascând cauza.

Comentariul din `SumarView.ApplyTheme` chiar spunea premisa pe dos («grila NU se atinge aici: `ThemeManager.Traverse` ajunge la el»), ceea ce a ținut bug-ul ascuns.

**Reparație comună:** `ThemeManager.ApplyToNestedThemed` traversează subarborele și transmite schema numai copiilor `IThemedControl`, fără reguli generice pe tip. `TextBox` intern din `KBotTextField` rămâne neatins. Se evită apelurile duplicate în fiecare vedere și omiterea uneia viitoare.

Patru teste noi (`NestedThemedControlTests`) fixează ambele jumătăți: copilul și nepotul tematizat PRIMESC schema, copilul simplu NU e atins, și fiecare control e aplicat exact o dată.

### Fișiere atinse

**Noi (`src/KBot.Controls/DataView/`)**: `KBotSortDirection.vb`, `KBotFilterOperator.vb`, `KBotFilterEngine.vb`, `KBotColumnFilter.vb`, `KBotDataView.Filtering.vb`, `KBotDataView.FilterIcon.vb`, `Filter/KBotFilterPopup.vb`, `Filter/KBotFilterPopup.Painting.vb`, `Filter/KBotFilterConditionDialog.vb`, `Filter/KBotFilterConditionDialog.Designer.vb`.

**Modificate (`src/KBot.Controls/DataView/`)**: `KBotDataColumn.vb` (cele patru proprietăți de filtrare, `FilterEnabled`, interdicția pe Button/ProgressBar din ambele direcții, `HeaderIconsWidth` pe trei piese), `KBotDataView.vb`, `.HeaderIcons.vb` (slotul de filtru în `ComputeHeaderCellLayout`), `.Layout.vb` (poziții de vedere, `RowTopForModel`, `EnsureVisible`), `.Painting.vb` (`DrawRow` pe poziție de vedere), `.Input.vb` (`RowAtPoint`, `MoveRow`, hover/click de filtru), `.Footer.vb` (`RecomputeDerived`, agregate pe `ViewRows`), `.AutoSize.vb`, `.Editing.vb`, `.Tooltip.vb`, `.Theming.vb`.

**Motorul de teme**: `src/KBot.Theming/ThemeManager.vb` (`ApplyToNestedThemed`).

**Aplicația**: `src/KBot.App/Views/SumarView.vb` (comentariul care spunea premisa pe dos), `src/KBot.App/Views/SumarView.Designer.vb` (filtrul mutat de pe grilă pe cele șapte coloane).

**Bancul de probă**: `DataViewPlaygroundForm(.Designer).vb` — `ShowColumnFilter` în inspectorul de coloană (stins pentru Button/ProgressBar, ca bancul să arate regula, nu s-o descopere printr-o excepție) și butonul `ClearAllFilters()`.

**Versiuni**: `KBot.Controls` 1.13.0.0 → **1.14.0.0**; `KBot.Theming` 1.5.0.0 → **1.6.0.0**; `KBot.App` 1.0.12.0 → **1.0.13.0**; `KBot.DevHarness` 1.0.16.0 → **1.0.17.0**.

**Teste noi**: `KBotFilterEngineTests.vb`, `KBotDataViewFilteringTests.vb`, `KBotDataViewFilterIconTests.vb`, `KBotFilterPopupTests.vb`, `tests/KBot.Theming.Tests/NestedThemedControlTests.vb`. Modificate: `KBotDataViewBandThemingTests.vb` (vezi mai jos) și `tests/KBot.App.Tests/DdfViewTests.vb` (`RunSta` folosește acum `ExceptionDispatchInfo.Capture(...).Throw()` în loc de `Throw failure` — altfel fiecare eșec din firul STA se raporta ca o excepție fără loc, ceea ce a costat un drum în plus la diagnosticarea eșecurilor preexistente de mai jos).

### Rezultatele testelor

```
dotnet build KBot.sln          → 0 erori, 0 avertismente noi
                                 (1 avertisment MSB3825 pe DdfView.resx, preexistent)
KBot.Controls.Tests            → 685 / 685  (era 627 înainte de felie, +58)
KBot.DevHarness.Tests          → 170 / 170
KBot.Theming.Tests             →  65 /  65  (era 61, +4 pentru bug-ul de traversare)
KBot.Common.Tests / Xfa.Tests  →  14 / 14, 39 / 39
```

Meniul se probează headless prin porțile `Debug*` (convenția casei): construcția + așezarea (`DebugMeasure`) prind orice cădere în `Recalc`, iar `BuildFilter` face verificabilă regula «tot bifat ≠ filtru» fără să deschidă vreo fereastră.

#### Eșecuri PREEXISTENTE, neatinse de felie

`KBot.Domain.Tests` (3), `KBot.Api.Tests` (1) și `KBot.App.Tests` (8) au eșecuri care **nu vin de aici**. Verificat, nu presupus:

- Cele din `Domain`/`Api` (`EtichetaRevizie*`) eșuează identic pe arborele CURAT, la HEAD, cu felia scoasă cu `git stash`.
- Din cele 8 din `App.Tests`, 2 eșuează la HEAD, iar restul de 6 vin din rescrierea **în lucru** a lui `DdfView` din arborele de lucru: testele ajung la handler prin reflecție (`GetMethod("tree_NodeMouseUp")`), iar declarația aceea nu mai există în `DdfView.vb` (e la HEAD linia 622, în arborele de lucru a rămas doar șirul din `GlobalErrorLog.Write`) — `GetMethod` întoarce `Nothing` și `m.Invoke` dă `NullReferenceException`.

**Un test existent a fost REscris, nu reparat**: `KBotDataViewBandThemingTests.AColourSetByTheOperator_SurvivesAThemeSwitch` verifica exact regula pe care cererea 2 o răstoarnă — folosea schema **Dark** ca să demonstreze că o culoare de designer supraviețuiește. Acum se numește `…SurvivesASwitchBetweenLIGHTSchemes` și folosește Modern, iar regula nouă are trei teste proprii (suprascriere sub întuneric, culorile date înapoi la ieșire, fontul lăsat în pace).

### Rămase neverificate / amânate

- **NIMIC DIN FELIA ASTA N-A FOST VĂZUT PE ECRAN.** Meniul, pâlnia, bifele, dialogul de condiție și barele întunecate sunt verde la compilare și la teste headless, atât. Bancul de probă (`DataViewPlaygroundForm`, bifa «ShowColumnFilter») e locul unde se verifică — este singurul pas care rămâne, și e cel care a găsit bug-uri în fiecare felie de control de până acum.
- **Barele de derulare urmează doar perechea întuneric/lumină.** `SetWindowTheme` aduce griul Windows-ului, nu culorile paletei: sub o schemă colorată barele rămân cele de sistem, iar accentul nu ajunge niciodată pe ele. Varianta completă e un `KBotScrollBar` owner-drawn (folositor și arborelui, și popup-ului) — nefăcut aici, la alegerea operatorului.
- **O schemă personalizată cu `IsDark = True` ȘI `PreserveDesignerColors = True`** ar readuce culorile de designer după `ApplyTheme`, prin `DesignerBaseline.Restore` din `ThemeManager`. Niciuna dintre cele patru scheme compilate nu combină cele două steaguri (Colorful e singura cu `PreserveDesignerColors`, și e luminoasă), dar editorul de teme le poate combina.
- **Filtrarea nu s-a rulat peste date reale.** Toate testele merg pe rânduri construite în memorie; nicio vedere (Sumar / Istoric / Plăți / DDF) n-a fost încă trecută pe `ShowColumnFilter`.
- **Sortarea e pe O SINGURĂ coloană**, ca în foaia de date Access. Sortarea pe mai multe coloane n-a fost cerută și nu e implementată.
- Butoanele de sortare din Istoric și din arbore (`MsgBox` substituent) rămân neatinse — sunt un fir deschis mai vechi, nu al acestei felii.

### Fir deschis creat de felie

Vederile existente pot aprinde acum `ShowColumnFilter = True` fără altă modificare (grila filtrează singură, iar indicii de rând nu-și schimbă înțelesul). Care dintre ele o face, și dacă subsolul lor trebuie să arate «filtrat din N rânduri», e o decizie de operator — nu s-a luat aici.

## SLICE-0028-04 — DataView: `AutoSizeMode` PE COLOANĂ, care bate modul grilei <a id="slice-0028-04"></a>

Al patrulea pas al feliei 0028 (`KBotDataView`). Cerere de operator, începută de el în cod și lăsată neterminată, cu TODO-ul scris chiar în capul fișierului de auto-dimensionare:

> i started a new property per column (AutoSize) which takes precedence over the View's own AutoSize. you need to finish the implementation in KBotDataView.AutoSize

Exista `KBotDataColumn.AutoSizeMode` (câmp, proprietate, `Owner?.OnColumnAutoSizeModeChanged()`), cu `KBotDataView.OnColumnAutoSizeModeChanged` cerând layout, plus TODO în `KBotDataView.AutoSize.vb`. Dar `PerformAutoSize` citea numai `_autoSizeMode`, ignorând proprietatea coloanei.

### 1. Decizia care trebuia luată înainte de cod: ce înseamnă „precedență”

Implicitul inițial al coloanei, `KBotAutoSizeMode.None`, combinat cu precedența ar opri orice măsurare, deși grila are `AutoSizeColumnsMode = ToContent`. Cele cinci gazde care îl setează explicit (`DdfFileBrowser`, `XfaXmlPreview`, Plăți, Rezervări, Recepții) ar pierde dimensionarea fără altă schimbare.

**`KBotAutoSizeMode.Inherit = -1`** devine implicitul COLOANEI: hotărăște grila dacă nu există alegere explicită, ca sentinela `Color.Empty` pentru temă din 0027.

Consecințe, toate deliberate:

- **`Inherit` aparține numai coloanei.** Pe grilă (`AutoSizeColumnsMode`) aruncă `ArgumentException`, neavând sursă de moștenire. Ambele proprietăți resping valori din afara enum-ului prin `[Enum].IsDefined`.
- **Enum-ul păstrează `None = 0`, `ToContent = 1`:** bancul leagă `cboAutoSize.SelectedIndex` direct de valoare, de aceea `Inherit = -1`. Lista coloanei începe cu `Inherit` și traduce indicii prin `ColumnModeAt` / `IndexOfColumnMode`.
- `<DefaultValue(KBotAutoSizeMode.Inherit)>` pe proprietatea coloanei: designerul nu scrie nimic pentru o coloană neatinsă.

### 2. Pasul de măsurare (`KBotDataView.AutoSize.vb`)

Un singur loc citește precedența, și e numit ca atare:

```vb
Friend Function EffectiveAutoSizeMode(col As KBotDataColumn) As KBotAutoSizeMode
    If col Is Nothing Then Return _autoSizeMode
    If col.AutoSizeMode = KBotAutoSizeMode.Inherit Then Return _autoSizeMode
    Return col.AutoSizeMode
End Function
```

Două schimbări în pas:

- **Pasul 1 (măsurarea) întreabă per coloană**, nu o dată pe grilă. `If _autoSizeMode = ToContent` de dinaintea buclei a devenit un `If EffectiveAutoSizeMode(c) <> ToContent Then Continue For` ÎN buclă, lângă garda de `UserSized`.
- **Poarta de la intrarea în `PerformAutoSize` s-a mutat de pe grilă pe coloane.** Înainte era «grila e pe None ȘI n-avem umplere ȘI n-avem auto-hide ⇒ ieși». Acum se întreabă și `AnyColumnSizesToContent()`, altfel o coloană care cere singură măsurarea, pe o grilă manuală, n-ar fi fost măsurată niciodată — adică fix aranjamentul pentru care există proprietatea. (Poarta ignoră coloanele ascunse și pe cele trase de operator: pe acelea pasul oricum nu le atinge, deci o pornire pentru ele ar fi muncă degeaba.)

**Precedența acoperă DOAR măsurarea.** `ColumnFillMode` e alt buton și rămâne așa: umplerea / strâmtarea trec în continuare peste toate coloanele vizibile, inclusiv peste una fixată pe `None` — exact regula care se aplică deja unei coloane trase cu mouse-ul (`UserSized`), pe care măsurarea o sare, dar umplerea o mișcă. O coloană care nu are voie să se miște deloc se prinde în cuie cu `MinWidth = MaxWidth`, unde clamparea o ține. Regula asta e scrisă acum și în capul fișierului, în locul TODO-ului.

**Precedența e față de GRILĂ, nu față de operator**: `UserSized` bate în continuare tot, iar `ResetColumnSizing()` e cel care redă coloana măsurării.

### 3. Bancul de probă

Inspectorul de coloană din `DataViewPlaygroundForm` a primit lista `AutoSizeMode (bate modul grilei)`, cu cele trei valori. Tot acolo s-a corectat o activare care de acum minte: „Width” era editabil după steagul GRILEI (`toContent`), deși coloana selectată poate spune altceva — se întreabă modul EFECTIV al coloanei selectate (`SelectedColumnSizesToContent`).

### Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/KBotAutoSizeMode.vb` | membrul `Inherit = -1` + de ce e santinelă doar a coloanei |
| `src/KBot.Controls/DataView/KBotDataColumn.vb` | implicitul proprietății pe `Inherit`, `<DefaultValue>`, documentația precedenței, refuzul valorilor din afara enum-ului |
| `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` | `EffectiveAutoSizeMode`, `AnyColumnSizesToContent`, poarta și bucla de măsurare, refuzul lui `Inherit` pe grilă, capul de fișier (TODO-ul operatorului → explicație) |
| `src/KBot.DevHarness/Internal/DataViewPlaygroundForm(.Designer).vb` | lista `AutoSizeMode` în inspectorul de coloană + activarea corectă a „Width” |
| `tests/KBot.Controls.Tests/KBotDataViewColumnAutoSizeTests.vb` | **nou**, 9 teste |

**Versiuni**: `KBot.Controls` 1.14.0.0 → **1.15.0.0**; `KBot.DevHarness` 1.0.17.0 → **1.0.18.0**.

### Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori, 0 avertismente noi (MSB3825 pe DdfView.resx, preexistent)
KBot.Controls.Tests      → 694 / 694   (685 înainte de pas, +9)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
```

Cele 9 teste noi fixează: implicitul `Inherit`, coloana `None` care bate grila `ToContent`, coloana `ToContent` care bate grila `None`, pornirea pasului pe o grilă complet manuală, re-layout-ul din setter, `UserSized` care rămâne deasupra tuturor (și cade la `ResetColumnSizing`), umplerea care mișcă și o coloană `None`, plus cele două refuzuri (`Inherit` pe grilă, valoare din afara enum-ului).

#### Eșecuri PREEXISTENTE, neatinse de pas

Aceleași ca la 0028-03, verificate din nou **într-un worktree curat la HEAD**, nu presupuse: `KBot.Domain.Tests` 3 și `KBot.Api.Tests` 1 (`EtichetaRevizie*`) eșuează identic la HEAD; `KBot.App.Tests` are 10 în arborele de lucru, dintre care 2 eșuează și la HEAD, iar restul de 8 vin din rescrierea ÎN LUCRU a vederilor (`DdfView` — `GetMethod("tree_NodeMouseUp")` întoarce `Nothing`; `IstoricView` — coloana `obs`; captionul de navigare «Doc. Fundamentare»). Niciunul nu atinge auto-dimensionarea.

### Rămase neverificate / amânate

- **N-a fost văzut pe ecran**, ca tot restul feliei 0028. Locul unde se probează e lista nouă din inspectorul de coloană al bancului (`DataViewPlaygroundForm`).
- **Nicio vedere nu folosește încă proprietatea.** Cele cinci gazde care setează `AutoSizeColumnsMode = ToContent` au rămas exact cum erau; alegerea coloanelor care merită fixate (de obicei cele numerice, ca lățimea lor să nu sară de la o încărcare la alta) e a operatorului.
- **Umplerea rămâne grid-wide.** Un `FillWeight` pe coloană n-a fost cerut și nu s-a făcut; azi singurul mod de a scoate o coloană complet din mișcare e `MinWidth = MaxWidth`.

## SLICE-0028-05 — DataView: cine deține lățimea unei coloane (designer vs trecere de layout) <a id="slice-0028-05"></a>

Al cincilea pas al feliei 0028, pornit de la un raport de operator pe o vedere reală:

> look at the IstoricView. I tried to make the first three columns NO Autosize and only the last one to strech, but no matter what i do, the first column becomes huge. i want the first columns to be 300px, the second and third 200px and the last one to fill the difference

„No matter what I do” era exact adevărul: **două defecte diferite** îi anulau, pe rând, fiecare încercare. Niciunul nu se vede citind vederea — se văd doar din numere.

### 1. Ce spuneau numerele

Starea din `IstoricView.Designer.vb` la momentul raportului:

```vb
grid.AutoSizeColumnsMode = KBotAutoSizeMode.None       ' fără măsurare
' grid.ColumnFillMode : ABSENT  => None, deci nimeni nu se întinde
KBotDataColumn1.MaxWidth = 123456                       ' încercare de plafon
KBotDataColumn1.MinWidth = 300
KBotDataColumn1.Width    = 747                          ' ← „coloana uriașă”
KBotDataColumn2.Width    = 40                           ' exact MinWidth
KBotDataColumn3.Width    = 55
KBotDataColumn4.Width    = 144
```

`747 + 40 + 55 + 144 = 986 = grid.Size.Width` în designer. Suma exactă și `40 = MinWidth` identifică valorile drept ieșirea umplerii, serializată ca alegere a operatorului; constatarea este consemnată ca verificată.

### 2. Defectul 1 — trecerea de layout rula ÎN DESIGNER și-i autora fișierul

`PerformAutoSize` scrie `Width`, apoi designerul serializează rezultatul în `.Designer.vb`: lățimea calculată pentru acea suprafață devine persistentă ca alegere a operatorului. Este capcana `ShouldSerialize*` aplicată ieșirii layout-ului, care nu trebuie să autoreze formularul.

**Reparat:** `PerformAutoSize` iese imediat sub `KBotDesignTime.IsDesignTime(Me)`. În designer se văd lățimile **așa cum au fost autorate** (ceea ce, pentru autorare, e și mai onest); măsurarea și umplerea se întâmplă la rulare.

### 3. Defectul 2 — strâmtarea era DISTRUCTIVĂ

`IstoricView` se construiește într-un `SplitContainer` de ≈690 px, apoi se lărgește. `ShrinkToFit` suprascria `Width` (200 → 120); `DistributeLeftover` mărea numai ținta umplerii, lăsând cele două coloane strâmtate toată sesiunea. Trecerea își refolosea ieșirea, deci depindea de istoricul redimensionării, nu de cererea caller-ului.

**Reparat în model:** `KBotDataColumn` separă `_width` pictată de `_authoredWidth` cerută, scrisă numai prin setterul public `Width` (designer/cod/tragere). Layout-ul scrie prin `SetLayoutWidth` și începe cu `RestoreAuthoredWidths()`, ca `ClearAutoHiddenState()`: rezultatul depinde numai de lățimile cerute și spațiul disponibil.

O coloană trasă de operator e sărită de la restaurare: tragerea aceea E lățimea cerută de acum (setterul public), și ține până la `ResetColumnSizing()`.

### 4. Vederea

`IstoricView.Designer.vb`: `ColumnFillMode = LastColumn` (lipsea — de asta nu se întindea nimic), `MaxWidth = 123456` șters, iar lățimile puse pe ce a cerut operatorul: **300 / 200 / 200 / 250** (ultima e doar punctul de plecare — ea ia diferența). Cele patru `AutoSizeMode = None` autorate de operator rămân: acum chiar înseamnă ceva (felia 0028-04).

Rezultatul, măsurat pe vederea reală (diagnostic temporar, șters după):

```
la 690 px  (cât e la construcție): 300 / 120 / 120 / 150   → strâmtate, fără bară orizontală
la 1600 px (fereastra adevărată):  300 / 200 / 200 / 900   → cerute înapoi + ultima umple
```

### Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/KBotDataColumn.vb` | `_authoredWidth`, `SetLayoutWidth`, `RestoreAuthoredWidth`; `Width`/`MinWidth`/`MaxWidth` țin baseline-ul la zi |
| `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` | ieșire sub `IsDesignTime`, `RestoreAuthoredWidths()` la începutul trecerii, toate scrierile trecerii pe `SetLayoutWidth` |
| `src/KBot.App/Views/IstoricView.Designer.vb` | `ColumnFillMode = LastColumn`, `MaxWidth = 123456` scos, lățimi 300/200/200/250 |
| `tests/KBot.Controls.Tests/KBotDataViewAuthoredWidthTests.vb` | **nou**, 5 teste |

**Versiuni**: `KBot.Controls` 1.15.0.0 → **1.16.0.0**; `KBot.App` 1.0.13.0 → **1.0.14.0**.

### Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Controls.Tests      → 699 / 699   (694 înainte, +5)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
Common 14/14 · Xfa 39/39 · LocalStore 1/1
```

Cele 5 teste noi: în designer nu se strâmtează și nu se măsoară nimic (site fals cu `DesignMode = True`), strâmtarea nu mai e distructivă (îngust → lat readuce 300/200/200), trecerea e idempotentă la aceeași lățime disponibilă, iar o coloană trasă rămâne a operatorului.

Eșecurile din `Domain` (3), `Api` (1) și `App` (10) sunt aceleași dinainte de pas — preexistente, verificate la HEAD într-un worktree curat la comiterea precedentă.

### Rămase neverificate / amânate

- **Tot nevăzut pe ecran.** Numerele de mai sus vin dintr-un test headless pe `IstoricView`, nu dintr-o rulare a aplicației.
- **Celelalte vederi pot avea aceeași murdărie în `.Designer.vb`.** Defectul 1 a lucrat pentru oricare formular deschis în designer, deci `SumarView` / `PlatiView` / `Rezervari` / `Receptii` / `DdfView` pot conține lățimi calculate, înghețate. N-au fost atinse aici: fiecare cere privit ce voia operatorul de la coloanele ei, nu o corectură în masă. De acum înainte nu se mai pot murdări.
- **Strâmtarea rămâne grid-wide**: la spațiu insuficient se ia din TOATE coloanele vizibile, inclusiv din cele cu `AutoSizeMode = None` (regula scrisă în 0028-04: precedența acoperă doar măsurarea). Cine vrea o coloană complet nemișcată o prinde cu `MinWidth = MaxWidth`. Dacă operatorul vrea altă regulă — «strâmtarea ia doar din ținta umplerii» — e o schimbare de contract, cu două teste existente care o fixează pe cea de azi (`Overflow_WithFillMode_…`, `Overflow_MinWidthsExceedViewport_…`).

## SLICE-0028-06 — Meniul de filtrare, autorat în DESIGNER (nu desenat în cod) <a id="slice-0028-06"></a>

Cerere de operator:

> i want the filtering form to be configed in the designer, not in normal code. of course, except the dynamic "list" of values. that can also be configured in the designer, but the values in it should remain filled at runtime

### 1. De ce era desenat în cod, și de ce motivul acela a expirat

`KBotFilterPopup` din 0028-03 picta integral meniul: ~400 linii .Painting.vb și tot atâtea de geometrie/hit-test. Motivul inițial: ContextMenuStrip + CheckedListBox ar fi rămas două dreptunghiuri albe în dark.

Între timp ThemeManager a primit reguli pentru CheckedListBox/ListBox/CheckBox/Button/Panel și scrollbars native DarkMode_Explorer/Explorer. KBotThemedForm le aplică la încărcare/schimbarea schemei, eliminând lipsa care justificase pictura din 03.

Deci meniul a fost **rescris ca formular obișnuit al casei**, cu regula casei aplicată la literă: toate controalele în `KBotFilterPopup.Designer.vb`.

### 2. Ce s-a schimbat

| Înainte | Acum |
|---|---|
| `Inherits Form` + `Implements IThemedControl`, culorile luate manual într-un `ApplyTheme` propriu | `Inherits KBotThemedForm` — tema vine prin `ThemeManager.Apply`, ca la orice formular |
| rânduri de meniu desenate (`MenuRow`, `Bounds`, `HotNone/HotOk/HotCancel`), hit-test pe `OnMouseDown` | patru `Button`-e docate sus + `CheckBox` + `CheckedListBox` + `pnlButoane` cu OK/Anulează |
| lista de valori desenată rând cu rând, cu derulare proprie (`_listScroll`, `ClampScroll`, `DrawListScrollHint`) | `CheckedListBox` cu bara ei, tematizată nativ |
| «(Selectează tot)» desenat cu bifă în trei stări | `CheckBox` cu `ThreeState = True` |
| lățime și înălțime calculate din texte (`MeasureNaturalWidth`) | **lățimea și toate mărimile sunt ale designerului**; codul mai atinge doar ÎNĂLȚIMEA, ca lista să arate câte rânduri are (până la 10) |
| `KBotFilterPopup.Painting.vb` (408 linii) | **șters** |

Ce a rămas neapărat la rulare, și de ce:

- **conținutul listei** — valorile distincte ale unei coloane nu există la proiectare (chiar cererea operatorului); controlul, fontul, înălțimea rândului și locul lui sunt însă din designer;
- **textele care depind de tipul coloanei** — sortarea se numește «A → Z» pe text și «de la mic la mare» pe numere (`KBotFilterEngine.SortCaption`), iar «Șterge filtrul din «X»» poartă numele coloanei;
- **activarea lui «Șterge filtrul»** (stins fără filtru) și **existența butonului de condiții** (coloanele logice n-au submeniu — butonul se ASCUNDE, nu se stinge, ca să nu rămână un rând care nu duce nicăieri).

**Decizii păstrate:** OK transmite Clone; sortarea imediată închide meniul; tot bifat ≠ filtru; Selectează tot afectează numai rândurile afișate; Necompletate rămâne text vid în model; `_suppressDeactivate` păstrează meniul cât condițiile sunt deschise. KBotFilterPopupTests a trecut neschimbat, fixând aceleași decizii indiferent de randare.

### 3. Defect găsit pe drum: `KBotTextField` nu ridica niciodată `TextChanged`

`Handles txtCauta.TextChanged` nu primea evenimentul: KBotTextField.Text delega inputului intern fără ridicarea TextChanged pe cadru. Consumatorii anteriori foloseau InnerTextBox.TextChanged; legarea normală în designer eșua tăcut.

Reparat în control: `_inner.TextChanged` → `OnTextChanged(EventArgs.Empty)` + `Invalidate()` (placeholder-ul oricum se redesena la fel).

### 4. Ce poate face operatorul acum

`KBotFilterPopup.vb` se deschide în designerul Visual Studio (a plecat `<DesignerCategory("Code")>`) și se schimbă acolo fonturile, înălțimile rândurilor, lățimea meniului, ordinea butoanelor, `ItemHeight`-ul listei — fără să atingă cod. Chenarul de 1px al meniului nu e pictat: e `Padding`-ul formularului peste fundalul lui, iar culoarea vine din temă (`OnThemeChanged`), deci **nu există nicio culoare scrisă în designer sau în cod**.

### Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Designer.vb` | **nou** — toate controalele, cu ordinea de andocare inversă celei vizuale |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | rescris: doar comportament (630 → ~470 linii, fără geometrie și fără pictură) |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Painting.vb` | **șters** (408 linii) |
| `src/KBot.Controls/TextField/KBotTextField.vb` | `TextChanged` al cadrului se ridică la schimbarea textului intern |
| `tests/KBot.Controls.Tests/KBotFilterPopupDesignerTests.vb` | **nou**, 9 teste care trec prin CONTROALE, nu prin porțile `Debug*` |

**Versiuni**: `KBot.Controls` 1.16.0.0 → **1.17.0.0**.

### Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Controls.Tests      → 708 / 708   (699 înainte, +9)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
```

Cele 9 teste noi: lista e declarată în designer și umplută la rulare, căutarea reumple CONTROLUL, o bifă stinsă în `CheckedListBox` ajunge în filtru (prin `ItemCheck`, drumul mouse-ului), un filtru existent aprinde bifele potrivite, «Șterge filtrul» e stins fără filtru, «(Selectează tot)» arată starea mixtă și atinge doar rândurile arătate, coloana logică n-are rândul de condiții (verificat prin CONSECINȚĂ — ce urmează urcă — fiindcă `Visible` pe un formular nearătat răspunde despre lanțul de părinți, nu despre buton), iar titlurile de sortare vin din tipul valorii.

### Rămase neverificate / amânate

- **NEVĂZUT PE ECRAN**, ca tot restul feliei 0028. Meniul e verde la compilare și în teste headless; locul unde se probează rămâne bancul (`DataViewPlaygroundForm`, bifa `ShowColumnFilter` pe o coloană) — plus, de acum, chiar suprafața de proiectare din Visual Studio.
- **Culorile pe schema întunecată n-au fost văzute.** Regulile pe tip există și se aplică, dar bifele desenate de sistem într-un `CheckedListBox` sub o paletă închisă sunt exact genul de lucru care se judecă privind, nu citind.
- **Navigarea cu tastatura s-a schimbat**: nu mai există rândul „survolat” desenat de noi, ci ordinea de TAB a controalelor (Esc și Enter au rămas la fel, prin `KeyPreview`). E comportamentul standard Windows, dar e ALT comportament față de 0028-03.
- Meniul de condiții (`CustomPopup`) și dialogul de operanzi au rămas neatinse.

## SLICE-0028-07 — Schema «Modern»: butonul crește cât să încapă și umplutura, și textul <a id="slice-0028-07"></a>

Raport de operator, după ce a autorat singur meniul de filtrare în designer (felia 0028-06):

> i adjusted the design and it looks ok, but on modern theme, it adds a lot of padding to the buttons, so the text is not visible anymore. when the modern theme is applied, it should adjust the height of the buttons to fit the new padding AND the text

### 1. Ce se întâmpla

`BuiltInSchemes.Modern()` cere `ControlPadding = (12, 8, 12, 8)`, iar `ModernRenderer.ApplyButton` o scrie în `Button.Padding`. Umplutura NU e desenată în afara butonului — ea micșorează suprafața în care încape textul. Pe un buton autorat la 32px rămâneau `32 − 8 − 8 = 16px` pentru un rând de text care cere ~15–20px: textul se tăia sau dispărea de tot.

**Paddingul nu era restaurat:** la ieșirea din Modern, `ThemeManager.StylePalette` și `StyleSystem` chemau `DetachButton`, care elimina handler-ele/regiunea rotunjită, dar păstra `Padding = (12,8,12,8)`. Modificarea persista astfel și în Classic.

### 2. Regula pusă în loc

În `ModernRenderer`, aceeași formă ca lățimea autorată a unei coloane (felia 0028-05): **starea autorată se reține înainte de prima scriere a temei, iar tema calculează peste ea, nu în locul ei.**

`ButtonState` (deja exista, pentru handler-e) ține acum și `AuthoredPadding` + `AuthoredHeight`, luate o singură dată, înaintea oricărei scrieri. Apoi:

- **`ApplyButton`** — după ce pune umplutura schemei, cheamă `FitHeightToPaddingAndText`:

  ```
  nevoie   = Padding.Vertical + înălțimea unui rând de text (fontul butonului) + cele două chenare
  înălțime = Math.Max(înălțimea AUTORATĂ, nevoie)
  ```

  Se ia MAXIMUL, deci **o înălțime autorată mai mare rămâne neatinsă**: tema completează ce lipsește,
  nu rescrie ce a ales operatorul. Și, fiindcă baza e mereu valoarea autorată, comutarea temei de
  zece ori dă același rezultat ca prima — trecerea e o funcție de (autorat, schemă), nu de istoric.

- **`DetachButton`** — dă înapoi și marginea, și înălțimea autorate.

Două cazuri sunt sărite dinadins, fiindcă acolo înălțimea nu e a butonului:

- `AutoSize = True` — se ocupă WinForms;
- `Dock = Left / Right / Fill` — înălțimea e a PĂRINTELUI, iar o scriere aici ar fi ștearsă de următorul layout (în meniul de filtrare, exact cazul lui OK / Anulează, andocate în bara lor).

Un buton fără text primește totuși loc de un rând (se măsoară „Wg”): o pictogramă pusă mai târziu n-are cum să încapă într-un buton cât umplutura.

### 3. Fereastra care le ține

Butoanele de comandă ale meniului sunt andocate SUS, deci creșterea lor mușcă din lista de valori, care e `Fill` — meniul ar fi arătat, după comutare, mai puține valori decât înainte. `KBotFilterPopup.OnThemeChanged` face acum `PerformLayout()` + re-măsurarea înălțimii ferestrei, deci fereastra crește cu exact cât au crescut butoanele și numărul de rânduri arătate rămâne cel dinainte.

### Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Theming/ModernRenderer.vb` | `AuthoredPadding`/`AuthoredHeight` în `ButtonState`, `FitHeightToPaddingAndText`, restaurarea din `DetachButton` |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | `OnThemeChanged` re-măsoară fereastra după ce butoanele au crescut |
| `tests/KBot.Theming.Tests/ModernButtonHeightTests.vb` | **nou**, 6 teste |
| `tests/KBot.Controls.Tests/KBotFilterPopupDesignerTests.vb` | +1 test (fereastra crește odată cu butonul) |

**Versiuni**: `KBot.Theming` 1.6.0.0 → **1.7.0.0**; `KBot.Controls` 1.17.0.0 → **1.18.0.0**.

### Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Theming.Tests       →  71 /  71   (65 înainte, +6)
KBot.Controls.Tests      → 709 / 709   (708 înainte, +1)
KBot.DevHarness.Tests    → 170 / 170
```

Cele 6 teste noi: butonul scund crește cât umplutura + textul, cel destul de înalt e lăsat în pace, ieșirea din schemă dă înapoi marginea ȘI înălțimea, re-aplicarea nu compune creșterea, un buton andocat lateral e lăsat părintelui, iar unul fără text tot primește loc de un rând.

### Rămase neverificate / amânate

- **Nevăzut pe ecran**, ca toată felia 0028. Numerele vin din măsurători headless (`TextRenderer.MeasureText`), care sunt cele folosite și de pictare — dar verdictul «se citește bine» se dă privind.
- **Alte forme din aplicație vor arăta altfel pe Modern.** Regula e în MOTOR, nu în meniu, deci orice buton scund cu `Dock = Top/Bottom/None` va crește acum sub schema modernă — ceea ce e reparația cerută, dar e o schimbare vizibilă peste tot (LoginForm, MainForm, vederi). Nimic nu se micșorează, deci nimic nu se poate ascunde din cauza asta.
- **Umplutura orizontală n-a fost atinsă.** Cei 12px stânga/dreapta pot încă trunchia un text lung pe un buton îngust; lățimea nu s-a ajustat, fiindcă acolo layout-ul (andocare, ancore) o stăpânește mult mai des decât înălțimea, iar o creștere automată de lățime ar strica aranjamente întregi. Dacă apare cazul, se rezolvă cu `AutoSize` sau cu o lățime autorată mai mare.

## SLICE-0028-08 — Trecerea OPERATORULUI peste cele două ferestre de filtrare (+ ce a rupt designerul pe drum) <a id="slice-0028-08"></a>

**Livrare:** operatorul a modificat singur designerul VS după 0028-06/07 și a cerut includerea tuturor modificărilor necomise, «even the work you didn't do». Se consemnează proveniența celor incluse.

### 1. Meniul de filtrare — rândurile de meniu (autor: operatorul)

`KBotFilterPopup.vb` adaugă `AplicaRandDeMeniu`, apelată din OnThemeChanged pentru patru comenzi de sus: rânduri plate, fără chenar, culoarea suprafeței, relief numai la hover, ca meniul de sistem.

Modern desena orice Button cu Region de rază 8/fundal de buton; pe rândurile late, decupajele arătau suprafața de dedesubt ca patru pastile gri pe alb. `ModernRenderer.DetachButton` elimină Region și restaurează marginea/înălțimea autorate, pe calea din 07. Idempotent și în schemele fără rotunjire, păstrând rândul uniform.

Șterge filtrul folosește acum ErrorColor din paletă în locul Firebrick din designer, urmărind schema.

**Două defecte reparate în același drum**, amândouă în deschiderea submeniului de condiții:

1. `CustomPopup` ridică `ItemClicked` ÎNAINTE de `Close`, iar dialogul de condiție e modal — deci submeniul rămânea pe ecran, viu și inutil, până se închidea dialogul. Acum se cheamă `Hide()` pe el înainte de `AplicaConditia`.
2. `_suppressDeactivate = True` se pune ACUM ÎNAINTE de `Hide()`: ascunderea ferestrei active mută activarea pe altcineva, adică ridică `OnDeactivate` — care, cu garda pusă după, închidea chiar meniul pe care tocmai îl foloseam.

### 2. Dialogul de condiție — rescris pe `TableLayoutPanel` (autor: operatorul)

`KBotFilterConditionDialog` stătea pe `Location`/`Size` absolute. Acum totul e într-un `tlyMAIN` (două coloane, șase rânduri), deci se poate rearanja în designer fără să se recalculeze poziții.

A doua casetă, numai la Între…, se ascunde prin rânduri strânse la zero și scurtarea corespunzătoare a ferestrei, fără mutarea manuală a butoanelor. `StrangeRandul` citește tlyMAIN.GetRow, fără indice fix care ar ceda la reordonarea în designer. Constructorul primește Try/Catch cu re-aruncare: dialogul incomplet nu este acceptat.

### 3. Ce a rupt designerul pe drum: `PlatiView` (reparat aici)

VS a șters bucla celor nouă AutoSize din detailTable, nerecitibilă de designer. A rămas RowStyles.Add(Percent 100), aplicat pozițional rândului 0 (Nr. document), în loc de 9 (Explicații). Ar fi produs primul rând uriaș/restul înghesuite.

Reparat scriind cele nouă rânduri **unul câte unul** — pe acelea designerul le poate reciti, deci nu le mai pierde.

E același tipar cu defectul din 0028-05 (trecerea de auto-dimensionare care autora `.Designer.vb`) văzut din partea cealaltă: **ce nu poate designerul reciti, dispare la prima deschidere.**

### 4. Restul care aștepta comiterea

- `src/KBot.Controls/My Project/Resources.resx` + `Resources.Designer.vb` — proiectul `KBot.Controls` a primit propriul fișier de resurse (gol deocamdată, doar șablonul VS) plus legarea lui în `.vbproj`. **Trebuie comis**: fără el, `.vbproj` trimite către un fișier care nu există pentru oricine altcineva clonează.
- `PlatiView.resx`, `KBotFilterConditionDialog.resx` (+ `$this.Icon`) — resurse generate de VS la salvarea formularelor.
- `KBotFilterPopup.Designer.vb` — ajustările vizuale ale operatorului: cursor de mână pe cele două rânduri de comandă, «Operatori filtru» în loc de «Filtre...», înălțimi și umpluturi refăcute.

### Fișiere atinse

| Fișier | Autor | Ce s-a schimbat |
|---|---|---|
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | operator | `AplicaRandDeMeniu`, roșul din paletă, cele două defecte din deschiderea submeniului |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Designer.vb` | operator | ajustări vizuale (cursor, texte, înălțimi) |
| `src/KBot.Controls/DataView/Filter/KBotFilterConditionDialog(.Designer/.resx).vb` | operator | rescris pe `tlyMAIN`, `StrangeRandul`, `Try/Catch` în constructor |
| `src/KBot.Controls/My Project/Resources.resx` + `.Designer.vb`, `KBot.Controls.vbproj` | operator | fișierul de resurse al proiectului de controale |
| `src/KBot.App/Views/PlatiView.Designer.vb` | **reparat aici** | cele nouă rânduri `AutoSize`, scrise unul câte unul |
| `src/KBot.App/Views/PlatiView.resx` | VS | resursă generată la salvare |

**Versiuni**: `KBot.Controls` 1.18.0.0 → **1.19.0.0**; `KBot.App` 1.0.14.0 → **1.0.15.0**.

### Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori, 0 avertismente
KBot.Controls.Tests      → 709 / 709
KBot.Theming.Tests       →  71 /  71
KBot.DevHarness.Tests    → 170 / 170
Common 14/14 · Xfa 39/39 · LocalStore 1/1
```

`App` (10), `Domain` (3) și `Api` (1) — aceleași eșecuri preexistente, cu ACELAȘI număr ca înainte de felie (verificate la HEAD într-un worktree curat la comiterea din 10.08.2026).

### Rămase neverificate / amânate

- **Nimic din felia asta n-a fost probat de mine pe ecran.** Partea operatorului a fost VĂZUTĂ de el în designer și la rulare — de acolo vin cererile — dar rândurile de meniu, dialogul pe tabel și panoul `PlatiView` reparat aici n-au fost confirmate vizual după ultima modificare.
- **`PlatiView` e reparat pe hârtie, nu pe ecran.** Niciun test nu prinde ordinea stilurilor de rând (ele n-au consecință verificabilă headless, fiindcă panoul e `Visible = False` până la o selecție). Merită o privire la prima rulare a vederii Plăți.
- **Aceeași ștergere de buclă poate lovi orice `.Designer.vb` deschis în VS.** `PlatiView` a fost prins fiindcă diferența era în arborele de lucru; alte vederi cu cod scris de mână în designer (bucle, `For Each`, apeluri ajutătoare ca `InitDetailPair`) pot pierde același fel de linii la prima deschidere. `InitDetailPair` a supraviețuit aici, dar e o chestiune de noroc, nu de regulă.
- `My Project\Resources.resx` din `KBot.Controls` e GOL: legarea există, resursele nu.

## SLICE-0028-09 — Butonul de TEMĂ intră în bara de titlu <a id="slice-0028-09"></a>

Cerere de operator, după ce selectorul de teme a mers o felie întreagă din `MainForm`:

> add a new button ThemeButton, right-to-left: just after the minimize/maximize buttons (if available, else after the close button) and will be built in the captionbar. it will do what the mainform option button does now, but with functions built in the captionbar system. […] there will be only one option (true/false) to enable this button. […] there should be another new option: "Show Theme Editor" which will show / hide the last button in the popup (the theme editor)

### 1. Ce se întâmpla

MainForm ținea ~130 linii pentru ThemeManager, litere de acces, pictograme, CustomPopup.ClosedJustNow, ThemeEditorForm și capBar.OptionButtonBounds; captionbar avea numai buton/eveniment. Orice altă gazdă ar fi copiat logica.

Și chiar exista: `LoginForm` are `ShowOptionsButton = True` cu pictograma `switch_theme` și **niciun handler** — un buton care arată exact ca selectorul de teme și nu face nimic la clic.

### 2. Ce s-a construit

**`KBotCaptionBar.ThemeButton.vb`**, partial nou lângă KBotCaptionBar.vb: `ShowThemeMenu()` construiește CustomPopup, ancorat la ThemeButtonBounds, comută prin ThemeManager.SetScheme și deschide ThemeEditorForm la Stiluri…. Editorul exista deja în KBot.Controls, fără mutare nouă.

Proprietăți (toate cu `<DefaultValue>`, deci **niciun implicit nu ajunge scris în formularul gazdă**):

| Proprietate | Implicit | Ce face |
|---|---|---|
| `ShowThemeButton` | `False` | **Singurul comutator cerut.** Îl aprinzi și vine cu tot: pictogramă, meniu, litere de acces, comutarea schemei. |
| `ShowThemeEditor` | `True` | Al doilea comutator cerut: stinge ULTIMUL rând al meniului («Stiluri...») — și, cu el, separatorul care nu mai are ce despărți. |
| `ThemeButtonImage` | `Nothing` | Gol = pictograma implicită din resursele K-BOT (`switch_theme`). Pereche `ShouldSerialize`/`Reset`, ca implicitul să nu treacă drept alegere. |
| `ThemeButtonPadding` | `2` | Cât se strânge glifa în slot (valoarea aleasă de operator pe butonul vechi). |
| `TintThemeButtonImage` | `True` | Recolorează glifa cu culoarea celorlalte — altfel, pe schema întunecată, siluetă neagră pe fundal negru. |
| `ThemeButtonBounds` / `ThemeButtonActive` | — | Doar rulare: `<Browsable(False)>` + serializare ascunsă. |
| Evenimentul `ThemeSchemeChanged` | — | Ridicat DUPĂ ce s-a aplicat schema. Gazda **nu** trebuie să reaplice nimic (`SetScheme` difuzează peste toate formularele deschise) — evenimentul e pentru ce e în PLUS. |

**Pictogramele meniului** vin acum din `KBot.Controls\Resources.resx` (`ThemeClassic` / `ThemeDark` / `ThemeModern` / `ThemeColorful` / `ThemeEditor` / `switch_theme`), nu din resursele lui `KBot.App` — controlul nu poate depinde de gazdă.

**Cheie editor `@ThemeEditor`:** evită confundarea cu o schemă de utilizator numită Stiluri, care în vechiul MainForm ar fi deschis editorul în locul comutării.

### 3. Cele trei lucruri care s-au rupt pe drum și au fost reparate

**(a) Sloturi:** formula proprie a opțiunilor `1 + min + max` ar fi suprapus tema. Acum ThemeButtonSlot() numără o dată; opțiunile folosesc `ThemeButtonSlot() + If(ShowThemeButton, 1, 0)`. Dreapta→stânga: închidere, maximizare, minimizare, temă, opțiuni, inclusiv fără min/max conform cererii.

**(b) Titlul curgea pe sub butoane.** `TitleRightLimit()` nu se uita deloc la butonul de opțiuni (defect vechi, invizibil cât timp titlul era scurt). Acum limita e marginea celui mai din stânga buton VIZIBIL, oricare ar fi el.

**(c) IPopupAnchor** transmite numai bitul meniu deschis, fără buton. `_themeMenuOpening`, pus înainte de ShowBelow și consumat în SetPopupOpen, aprinde tema; alte deschideri de la gazdă aprind opțiunile. Închiderea le stinge pe ambele, evitând butonul aprins fără meniu.

Desenul celor două butoane a fost pus pe aceeași funcție (`DrawImageButton`): sunt același obiect vizual, iar două desene paralele s-ar fi despărțit la prima reglare de umplutură.

### 4. `MainForm` a rămas cu 9 rânduri

`capBar.ShowThemeButton = True` înlocuiește trei rânduri de opțiuni în designer. MainForm.vb elimină CapBar_OptionButtonClick/OnThemePicked/CuLiteraDeAcces/GetThemeIcon (~130 linii) și `_suppressThemeEvents`, verificat niciodată ridicat. Rămâne CapBar_ThemeSchemeChanged pentru jurnal; shellul se retematizează singur.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` | **nou** — butonul, meniul, comutarea schemei, «Stiluri...» |
| `src/KBot.Controls/CaptionBar/ThemeSchemeChangedEventArgs.vb` | **nou** — argumentele evenimentului |
| `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb` | `Partial`; sloturi dintr-o singură numărătoare; `TitleRightLimit` vede toate butoanele; desen comun `DrawImageButton`/`DrawGlyphImage`; hover + clic + `SetPopupOpen` pe două butoane |
| `src/KBot.App/MainForm.vb` | ~130 de rânduri de meniu de temă scoase; handler nou de 9 rânduri |
| `src/KBot.App/MainForm.Designer.vb` | `ShowThemeButton = True` în locul butonului de opțiuni |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.19 → **1.20** |
| `src/KBot.App/KBot.App.vbproj` | `FileVersion` 1.0.15 → **1.0.16** |
| `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb` | **nou** — 16 probe |

### Rezultate de test

- `dotnet build KBot.sln` — **succeeded, 0 erori, 0 avertismente** (rămâne avertismentul vechi `MSB3825` de la `DdfView.resx`, fără legătură).
- `KBot.Controls.Tests` — **725/725 verzi** (cele 16 noi + cele 9 vechi ale butonului de opțiuni, nemodificate).
- `KBot.Theming.Tests` — 71/71 verzi.
- Probele noi țin: geometria sloturilor (tema lângă cutia de control, opțiunile lipite la stânga ei, ambele urcă un slot când se stinge minimizarea), conținutul meniului (schema activă lipsește, fiecare rând are literă de acces TASTABILĂ și unică, fiecare rând are pictogramă), comutatorul `ShowThemeEditor` (scoate ultimul rând ȘI separatorul), aprinderea pe butonul corect și **zero rânduri de designer** pentru toate cele cinci proprietăți noi — verificate prin `TypeDescriptor`, calea pe care merge chiar Visual Studio.

### Nevăzut / amânat

- **Nimic din felia asta n-a fost văzut pe ecran.** Butonul, meniul, pictograma implicită și aprinderea sunt probate headless (`DrawToBitmap`, numărare de pixeli). Prima probă de operator trebuie să se uite la: poziția butonului față de minimizare, mărimea glifei la umplutura 2, și dacă meniul iese aliniat sub buton.
- **`KBot.App.Tests` are 10 probe roșii, TOATE dinainte** (`DdfViewTests` 7, `IstoricViewTests` 2, `MainFormNavItemsTests.Designer_WroteLiteralDiacritics` 1). Ultima e verificabilă direct: `git show HEAD:src/KBot.App/MainForm.Designer.vb` scrie deja `"Fundamentare"`, iar proba cere `"Doc. Fundamentare"`. Niciuna nu atinge bara de titlu sau tema.
- **`LoginForm` a rămas neatins**, deși acolo e chiar butonul mort descris la punctul 1: are `ShowOptionsButton = True` cu pictograma de temă și niciun handler. Un rând (`ShowThemeButton = True` în locul celor trei de opțiuni) îl face să meargă — dar `LoginForm` are lucru necomis în arbore, deci decizia rămâne a operatorului.
- Butonul de opțiuni NU a fost scos din control: rămâne liber pentru alt meniu, cu contractul lui neschimbat (`OptionButtonClick` + `OptionButtonBounds`).

## Completări și neconcordanțe din registrul istoric

- Etapa 02: vocabularul enumului păstrează numele Access, iar textul formatat este românesc («Da»/«Nu», «Adevărat»/«Fals»). Registrul scria «627 (585 înainte, +42)», în timp ce baza scria 586; numerele originale rămân consemnate, fără recalcularea sau inventarea unei probe.
- Etapa 03: registrul păstra referințe la varianta abandonată `FilterIconReserve`/`Filterable` în descrierea podelei/inspectorului; contractul corectat din aceeași etapă este per coloană, iar aceste proprietăți sunt eliminate. Registrul preciza SumarView ca singura vedere cu filtrul aprins (șapte coloane), celelalte cinci fără; fraza generală «nicio vedere» din finalul worklogului este neactualizată față de migrarea SumarView consemnată chiar în corpul lui.
- Etapa 09: formularea istorică «0 avertismente» coexistă cu mențiunea avertismentului vechi MSB3825. Nu devine dovadă că avertismentul a fost eliminat. LoginForm și probele lui rămân amânate, fără intervenție asupra codului.
