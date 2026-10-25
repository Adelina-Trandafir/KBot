# SLICE-0010-01 — KBotDataView (schelet)

Consolidare documentară D001-08 (25.10.2026): toate cele șapte intervenții 0010-01…07 sunt mai jos. Contractele, abaterile de la plan și regulile citate sunt istorice, nu instrucțiuni care înlocuiesc AGENTS actual. IsDirty din 01 este înlocuit explicit de 06; thumb-ul din 07 are corecția track/release. Probele rămân pe etapă, fără verdict vizual inventat. Repere: [0010-01](#slice-0010-01), [0010-02](#slice-0010-02), [0010-03](#slice-0010-03), [0010-04](#slice-0010-04), [0010-05](#slice-0010-05), [0010-06](#slice-0010-06), [0010-07](#slice-0010-07).

În etapele 0010-01…06, buildul istoric `dotnet build KBot.sln -c Debug` avea 0 warnings/0 errors la fiecare pas; rezultatele distincte ale testelor sunt în secțiunile lor. Toate etapele actualizau și docs/worklog/KBOT_STATUS.md.

<a id="slice-0010-01"></a>

Prima trecere din slice-ul 0010 (`KBotDataView`): grila owner-drawn, nelegată de date,
virtualizată — widget-ul de listă partajat pentru vederile K-BOT. Această trecere livrează
**scheletul** definit în planul pasului `0010-01`: proiect + bază de control double-buffered,
modelele coloană/rând, enum-ul de tip, cache-ul de temă + `ApplyTheme`, și pictarea antetului +
a corpului gol, tematizat. **Fără date randate** (rândurile vin în 0010-02).

## Ce s-a schimbat și de ce

- **Model nelegat (POCO):**
  - `KBotColumnType` — enum: Text, Combo, CheckBox, OptionButton, Button, ProgressBar.
  - `KBotDataColumn` — modelul de coloană (Key/ColumnType fixate la creare; Width clamp-at la
    MinWidth; Visible/Frozen/ReadOnly/Enabled/TextAlign/FormatString/ComboItems/Progress*/
    Resizable/Tag).
  - `KBotDataRow` — store de celule `Dictionary(Of String, Object)` (cheie lipsă => `Nothing`),
    `Enabled`, `Tag`, `IsDirty` + `MarkClean`, `HasValue`. Setter-ul `Item` ridică `IsDirty`.
- **Control `KBotDataView` (Inherits `Control`, Implements `IThemedControl`):**
  - Bază double-buffered (UserPaint | AllPaintingInWmPaint | OptimizedDoubleBuffer | ResizeRedraw
    | Selectable), aceeași familie ca `KBotNavList`.
  - Suprafață publică pentru acest pas: `AddColumn`, `Columns`, `Column(key)`, `FrozenColumnCount`,
    `AddRow`, `Rows`, `RowCount`, `ClearRows`, `GetDirtyRows`, `Item(colKey, rowIndex)`,
    `RowHeight`, `HeaderHeight`, `ShowHeader`, `AlternatingRows`, `ReadOnlyGrid`,
    `BeginUpdate`/`EndUpdate`, `InvalidateCell`/`InvalidateRow`, `ApplyTheme`.
  - `Designer.vb` declară TOATE cele 4 controale copil (regula casei): `editText`, `editCombo`,
    `vScroll`, `hScroll` — ascunse; poziționarea/afișarea lor vine în pașii următori.
  - Pictare: fundal corp + bandă de antet (text pe coloană, separatoare verticale, linie de bază),
    chenar exterior. Doar rânduri vizibile se vor picta (virtualizare în 0010-02).
  - `OnPaint` e boundary UI => `Try/Catch` care logează prin `GlobalErrorLog.Write` și
    **înghite** (un throw dintr-un body de paint ar prăbuși procesul). `DrawHeader` e acoperit
    tranzitiv de acel boundary.

## Decizii (abateri de la textul planului — confirmate cu codul real)

1. **Plasare + tematizare.** Planul spune „KBot.Controls + tematizare prin constante `KBotTheme`”.
   Realitatea: `KBotTheme` (în `KBot.Forexe`) e o **facadă de compatibilitate** cu doar ~9 `CLR_*`;
   nu are sloturile de grilă din plan. Modelul real de auto-tematizare (folosit de `KBotNavList`)
   este `IThemedControl.ApplyTheme(scheme As ThemeScheme)` care citește din `scheme.Palette`
   (`ThemePalette`). Am ales: **păstrez `KBotDataView` în `KBot.Controls`** (namespace-ul din plan,
   lângă `AdvancedTreeControl`) și **adaug o referință de proiect la `KBot.Theming`** (verificat:
   `KBot.Theming` NU referă `KBot.Controls`, deci fără ciclu). Controlul se auto-tematizează prin
   `IThemedControl`. Maparea sloturilor (0010-01): corp/rând = `InputBack`, antet = `ButtonBack`/
   `ButtonText`, chenar/separator = `Border`. Culorile derivate (selecție, linie de grilă, disabled)
   se adaugă în pașii care le folosesc — fără culori literale.
2. **`ThemeShapes` e `Friend`** în `KBot.Theming`, deci invizibil din `KBot.Controls`. Am replicat
   local doar ajutoarele geometrice pure (`ScaleDpi`, alinierea textului) — NU culori.
3. **Comentarii în română** (ca tot codul existent + regula CLAUDE.md), deși CODE_WORKFLOW zice
   „engleză”; 100% din codul din repo e cu comentarii românești, deci m-am aliniat la cod.

## Fișiere atinse

- `src/KBot.Controls/KBot.Controls.vbproj` — `ProjectReference` + `Import` pentru `KBot.Theming`.
- `src/KBot.Controls/KBotColumnType.vb` (nou)
- `src/KBot.Controls/KBotDataColumn.vb` (nou)
- `src/KBot.Controls/KBotDataRow.vb` (nou)
- `src/KBot.Controls/KBotDataView.vb` (nou)
- `src/KBot.Controls/KBotDataView.Designer.vb` (nou)
- `tests/KBot.Controls.Tests/` (nou proiect: `.vbproj` + `KBotDataModelTests.vb` + `KBotDataViewTests.vb`)
- `KBot.sln` — adăugat proiectul de teste.

## Rezultate teste

- `dotnet test KBot.sln` — **101 passed / 0 failed / 0 skipped** (din care 17 noi în
  `KBot.Controls.Tests`: clamp lățime coloană, cheie duplicată/necunoscută, `Item` prin control
  ridică `IsDirty` + `GetDirtyRows`, `ApplyTheme(Nothing)` no-op, re-aplicare temă fără scurgeri).
- Controlul se instanțiază fără handle în runner-ul de teste (fără STA/mesaje).

## Rămas neverificat / amânat (pentru pașii următori)

- **Pictarea NU a fost verificată vizual** (owner-drawn nu se poate testa headless). Se validează
  vizual în harness-ul din 0010-02 (eșantion sintetic 5.000×20).
- **Scrollbar-uri**: `vScroll`/`hScroll` declarate dar ascunse; poziționare/wiring în 0010-02.
  Randarea lor tematizată rămâne datorie tehnică asumată (v1 = scrollbar-uri WinForms standard).
- **Semantica `IsDirty` „încărcat vs. editat”** se finalizează în 0010-06 (editare). Azi
  `BeginUpdate/EndUpdate` doar suspendă/reia pictarea; nu ating starea „dirty”.
- `InvalidateCell`/`InvalidateRow` fac invalidare integrală (dreptunghiul exact vine la
  virtualizare, 0010-02).
- Restul pașilor: 02 render+virtualizare, 03 tipuri de coloană, 04 formatare+disable,
  05 input+selecție, 06 editare.

---

<a id="slice-0010-02"></a>

## SLICE-0010-02 — KBotDataView: render + virtualizare + scrollbar-uri

A doua trecere din slice-ul 0010. Livrează motorul de randare virtualizat: benzile de
layout, matematica de virtualizare, barele de derulare, pictarea rândurilor (Text + CheckBox),
plumbing-ul evenimentelor de formatare și **poarta de verificare headless** a virtualizării.

### Ce s-a schimbat și de ce

#### Restructurare pe parțiale
Controlul a crescut, așa că l-am împărțit exact ca `AdvancedTreeControl`:
- `KBotDataView.vb` — stare + API public + evenimente
- `KBotDataView.Theming.vb` — maparea paletă→roluri, cache GDI, `Blend`/`RoundedRect`
- `KBotDataView.Layout.vb` — benzi, geometrie, virtualizare, scrollbar-uri, `EnsureVisible`
- `KBotDataView.Painting.vb` — `OnPaint` + ajutoarele de desen

#### Layout / benzi
Trei zone: **antet** (sus), **bandă înghețată** (stânga, primele `FrozenColumnCount` coloane
vizibile — nu derulează orizontal) și **corpul derulat**. Offset-urile X se recalculează în
`RecalcColumnLayout` (pur, fără atingere de controale, deci sigur de chemat și din pictare) în
două liste: `_frozenLayout` / `_scrollLayout`.

#### Virtualizare (înălțime fixă => aritmetică întreagă)
- `firstVisibleRow = vScrollOffset \ RowHeight`
- `visibleCount = (ViewportHeight \ RowHeight) + 2`
- `RowTop(i) = HeaderHeight + i * RowHeight - vScrollOffset`
Se pictează DOAR intervalul vizibil; costul unei pictări nu depinde de `RowCount`.

#### Scrollbar-uri
`UpdateScrollBars` evaluează în **două treceri** (bara verticală mănâncă lățime, cea orizontală
înălțime — se influențează reciproc). Semantica WinForms respectată: `Maximum = conținut - 1`,
`LargeChange = fereastra`, valoarea utilă maximă = `Maximum - LargeChange + 1`; `Value` se
clamp-ează la micșorarea conținutului. Bara orizontală derulează **doar banda ne-înghețată**.
`UpdateLayout` are gardă de reintrare (`_inLayout`), fiindcă schimbarea vizibilității unei bare
declanșează layout. Rotița mouse-ului: vertical; `Shift`+rotiță: orizontal.

#### Pictare
Antet (bandă derulată decupată, apoi banda înghețată desenată peste ea, separatoare, linie de
bază + accent), rânduri (fundal normal/alternant/selectat), celule **Text** (text formatat prin
`Column.FormatString`, aliniere, elipsă) și **CheckBox** (geometria din `AdvancedTreeControl` —
dreptunghi rotunjit + bifă — dar cu **culori din paletă**, nu `DodgerBlue` hardcodat ca acolo).
Liniile de grilă se trag pe marginea de jos a rândului și pe marginea dreaptă a celulei.

#### Evenimente de formatare (plumbing; handler-ele bogate vin în 0010-04)
`CellFormatting` / `RowFormatting` se ridică deja, cu argumente **REFOLOSITE** (câte o singură
instanță `_cellArgs` / `_rowArgs`, resetate înainte de fiecare ridicare) — zero alocări per
celulă la mii de rânduri. Fundalul per-celulă/rând se pictează cu pensula cache-uită pe calea
rapidă și cu o pensulă temporară doar când un handler chiar a suprascris culoarea.

#### Maparea paletă → roluri (fără culori literale)
Implementată conform tabelului din plan: antet = `SurfaceAlt`/`Text`/`Border`/`Accent`,
rânduri = `InputBack` și `Blend(InputBack, Surface, 0.5)`, selecție =
`Blend(fundalul REAL al rândului, Accent, 0.18)` (două variante precalculate: rând normal /
alternant, ca textul să rămână lizibil), grilă = `Border`, bifă = `Border`/`Accent`/`AccentText`.
Editorii flotanți (`editText`/`editCombo`) se tematizează direct în `ApplyTheme`.

### Fișiere atinse

- `src/KBot.Controls/KBotDataView.vb` (restructurat: stare + API + evenimente)
- `src/KBot.Controls/KBotDataView.Theming.vb` (nou)
- `src/KBot.Controls/KBotDataView.Layout.vb` (nou)
- `src/KBot.Controls/KBotDataView.Painting.vb` (nou)
- `src/KBot.Controls/Events/KBotCellFormattingEventArgs.vb` (nou)
- `src/KBot.Controls/Events/KBotRowFormattingEventArgs.vb` (nou)
- `src/KBot.Controls/AssemblyInfo.vb` (nou — `InternalsVisibleTo("KBot.Controls.Tests")`)
- `tests/KBot.Controls.Tests/KBotDataViewVirtualizationTests.vb` (nou)
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` + `.Designer.vb` (nou)
- `src/KBot.DevHarness/Tests/KBotDataViewVisualTest.vb` (nou — auto-descoperit prin reflecție)

### Rezultate teste

- `dotnet test KBot.sln` — **104 passed / 0 failed / 0 skipped** (20 în `KBot.Controls.Tests`).
- **Poarta de virtualizare (headless)** — `KBotDataViewVirtualizationTests`:
  - `Paints_OnlyVisibleRows_NotAllFiveThousand`: 5.000 rânduri × 20 coloane într-o fereastră
    de 800×400 => se pictează `> 0` și `<= 20` rânduri.
  - `PaintedRowCount_IsIndependentOfRowCount`: numărul de rânduri pictate la 5.000 este
    **identic** cu cel la 50.000 — dovada că pictarea e O(fereastră), nu O(rânduri).
  - `EmptyGrid_PaintsNoDataRows`.
  - Metoda: `DrawToBitmap` forțează o pictare sincronă (confirmat: trece prin `OnPaint`), iar
    `DebugLastPaintedDataRows` (Friend, expus prin `InternalsVisibleTo`) raportează rezultatul.

### Rămas neverificat / amânat

- **Verdictul vizual uman NU a fost dat.** Harness-ul manual există și se lansează din
  DevHarness → categoria «Controls/UI» → «KBotDataView — virtualizare + temă (5.000 × 20)»
  (auto-descoperit prin reflecție, fără registru de modificat). Rulează DevHarness
  (pornire Debug → «Nu»), lansează proba și verifică: derulare fluidă pe ambele axe, prima
  coloană rămâne înghețată, rândurile alternante + bifele arată corect, comutarea
  Classic/Dark/Modern re-pictează grila. **Fluența derulării rămâne neconfirmată până când
  cineva chiar o rulează** — poarta headless dovedește doar că nu se pictează decât fereastra.
- `KBotDataColumn.Frozen` rămâne **metadata neutilizată**: mecanismul autoritar e
  `FrozenColumnCount`. De curățat (sau de cablat) într-un pas viitor.
- Selecția (`_currentRowIndex`) se **pictează** deja, dar rămâne -1 până la 0010-05, care
  aduce API-ul public + `SelectionChanged`.
- `InvalidateCell`/`InvalidateRow` fac tot invalidare integrală (suficient azi).
- Scrollbar-urile sunt cele standard WinForms — gaura de tematizare asumată pentru v1.
- Urmează: 03 restul tipurilor de coloană, 04 formatare+disable, 05 input+selecție, 06 editare.

---

<a id="slice-0010-03"></a>

## SLICE-0010-03 — KBotDataView: restul tipurilor de coloană

A treia trecere din slice-ul 0010. Completează cele șase tipuri de coloană: peste **Text** și
**CheckBox** (livrate în 0010-02) se adaugă **Combo**, **OptionButton**, **Button** și
**ProgressBar**, plus semantica de date a grupurilor de opțiuni.

### Ce s-a schimbat și de ce

#### Model
- `KBotDataColumn.OptionGroup As String` — grupul de exclusivitate pentru coloanele
  `OptionButton`. Grup vid => opțiune independentă.

#### Semantica grupurilor de opțiuni (date, nu input)
`KBotDataView.SetOptionValue(colKey, rowIndex, value)`: bifarea (True) stinge celelalte
opțiuni **din același rând** care au **același `OptionGroup`**. Nu atinge alte rânduri, nu
atinge alte grupuri, iar opțiunile fără grup rămân independente. Cheie necunoscută sau coloană
de alt tip => `ArgumentException` (fără no-op tăcut). Cablarea pe click/Space vine în 0010-05.

#### Pictare (stare de afișare)
- **Combo** — textul formatat + un chevron `TextDim` în dreapta; zona de text se micșorează
  cu lățimea chevronului. Editorul real (ComboBox flotant) apare abia la editare (0010-06).
- **OptionButton** — elipsă + punct central (geometria din `AdvancedTreeControl`, culorile din
  paletă: `Border` / `Accent` / `AccentText`).
- **Button** — față rotunjită + chenar + etichetă centrată (`ButtonBack`/`ButtonBorder`/
  `ButtonText`). Eticheta = textul celulei, iar dacă lipsește, `HeaderText`-ul coloanei (cazul
  uzual: o coloană «Detalii» cu același buton pe fiecare rând). Stările hover/pressed vin în
  0010-05, odată cu urmărirea mouse-ului.
- **ProgressBar** — șină + umplere proporțională (`Accent`), scalată pe `ProgressMin`/
  `ProgressMax`. Sub o lățime egală cu înălțimea, colțul rotunjit degenerează, deci se umple
  drept. Textul procentual (opțional în plan) NU s-a implementat.

Toate culorile vin din paletă — zero literale.

### Fișiere atinse

- `src/KBot.Controls/KBotDataColumn.vb` — `OptionGroup`.
- `src/KBot.Controls/KBotDataView.vb` — `SetOptionValue` + `ClearOptionSiblings`.
- `src/KBot.Controls/KBotDataView.Theming.vb` — culori + resurse GDI pentru cele patru tipuri noi.
- `src/KBot.Controls/KBotDataView.Painting.vb` — `DrawOptionCell`, `DrawButtonCell`,
  `DrawProgressCell`, `DrawComboCell`, `ProgressFraction`, `ToDouble` + dispatch-ul pe tip.
- `tests/KBot.Controls.Tests/KBotDataViewColumnTypeTests.vb` (nou, 9 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` — datele sintetice conțin acum câte o
  coloană din fiecare tip (acceptanța pasului).

### Rezultate teste

- `dotnet test KBot.sln` — **113 passed / 0 failed / 0 skipped** (29 în `KBot.Controls.Tests`).
- Acoperire nouă: exclusivitatea `OptionGroup` (același grup / alt grup / alt rând / fără grup /
  coloană de tip greșit), scalarea + limitarea `ProgressFraction` (inclusiv interval degenerat
  `Min = Max`, care NU împarte la zero), și o pictare cu **toate cele șase tipuri** simultan,
  care confirmă că `OnPaint` chiar a desenat rândul (`DebugLastPaintedDataRows = 1`).

### Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat.** Harness-ul (DevHarness → Controls/UI →
  «KBotDataView — virtualizare + temă») afișează acum toate cele șase tipuri; cum arată
  chevronul, butonul și bara de progres **rămâne neconfirmat până când cineva îl rulează**.
  Testele dovedesc doar că pictarea nu aruncă și că matematica e corectă.
- Hover/pressed pe celulele `Button` — în 0010-05 (are nevoie de urmărirea mouse-ului).
- Textul procentual pe `ProgressBar` — neimplementat (opțional în plan).
- `KBotDataColumn.Frozen` rămâne metadata neutilizată (autoritar e `FrozenColumnCount`).
- Urmează: 04 formatare+disable, 05 input+selecție, 06 editare.

---

<a id="slice-0010-04"></a>

## SLICE-0010-04 — KBotDataView: formatare condiționată + dezactivare pe trei niveluri

A patra trecere din slice-ul 0010. Plumbing-ul evenimentelor de formatare exista din 0010-02;
acum capătă **efect vizual** și, mai important, **contractul de activare efectivă** pe care se
va sprijini blocarea input-ului și a editării în 0010-05/06.

### Ce s-a schimbat și de ce

#### Activarea EFECTIVĂ (coloană × rând × celulă)
Regula, identică la pictare și la interogare:

```
efectiv = Column.Enabled AndAlso <Row.Enabled după RowFormatting> AndAlso <cellEnabled după CellFormatting>
```

Handler-ul de celulă poate **coborî** pe False, dar **nu poate ridica** peste o coloană sau un
rând dezactivat (verificat prin test). Două metode publice expun rezoluția, ca input-ul din 05
să nu re-implementeze regula:
- `IsRowEnabled(rowIndex)` — `Row.Enabled` + eventualul veto din `RowFormatting`.
- `IsCellEnabled(colKey, rowIndex)` — regula completă de mai sus.

Ambele ridică evenimentele de formatare cu **instanțe de argumente SEPARATE**
(`_probeCellArgs`/`_probeRowArgs`), ca o interogare să nu calce peste argumentele refolosite
ale unei pictări în curs — capcana evidentă a refolosirii.

#### Randarea „dezactivat”
- Textul dezactivat trece pe `DisabledTextColor` **indiferent** ce a cerut handler-ul: „inert”
  trebuie să se și VADĂ inert.
- Spălarea faintă (`Blend(rowBack, Surface, 0.4)`) se aplică **doar** când handler-ul n-a impus
  alt fundal — altfel am călca peste o regulă de formatare condiționată a caller-ului.
- Toate cele șase tipuri respectă starea: bifa/opțiunea se desenează cu marca ștearsă, chenarul
  butonului și eticheta lui trec pe culoarea dezactivată, umplerea barei de progres la fel,
  chevronul combo la fel.

#### Reguli demonstrative în harness (acceptanța pasului)
- `RowFormatting`: `Stare = «Anulat»` => `e.Enabled = False` => tot rândul șters + inert.
- `CellFormatting`: valoare numerică negativă => text pe `Palette.ErrorColor` (roșul PALETEI,
  nu o culoare literală — regula «zero literale» se aplică și în bancul de test).
- Datele sintetice conțin acum și valori negative, ca regula să aibă ce colora.

### Fișiere atinse

- `src/KBot.Controls/KBotDataView.vb` — `IsRowEnabled`, `IsCellEnabled`, instanțele „probe”.
- `src/KBot.Controls/KBotDataView.Theming.vb` — `_cDisabledText`/`_cDisabledWash` + resurse GDI.
- `src/KBot.Controls/KBotDataView.Painting.vb` — aplicarea stării dezactivate în `DrawCell` și
  în cei cinci pictori de tip.
- `tests/KBot.Controls.Tests/KBotDataViewFormattingTests.vb` (nou, 10 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` — cele două reguli demonstrative.

### Rezultate teste

- `dotnet test KBot.sln` — **123 passed / 0 failed / 0 skipped** (39 în `KBot.Controls.Tests`).
- Acoperire nouă: dezactivare pe coloană (afectează doar coloana), pe rând (doar rândul), pe
  celulă via eveniment (doar celula), veto de rând din `RowFormatting`, imposibilitatea de a
  ridica activarea dintr-un handler de celulă, coloană necunoscută => `ArgumentException`,
  numărul de ridicări ale evenimentelor la o pictare (2 rânduri × 2 coloane = 2 + 4), faptul că
  suprascrierea `e.Text` schimbă doar AFIȘAREA nu și valoarea din model, și că celulele
  dezactivate se pictează fără excepție.

### Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat.** Culoarea efectiv pictată nu se poate citi
  headless — testele verifică CONTRACTUL care o determină, nu pixelii. Cum arată rândul gri
  «Anulat» și textul roșu pe negative **rămâne neconfirmat** până rulează cineva harness-ul
  (DevHarness → Controls/UI → «KBotDataView — virtualizare + temă»).
- Dezactivarea **blochează deocamdată doar randarea**; blocarea click-ului/tastelor/editării
  vine în 0010-05/06, care vor consuma `IsCellEnabled`.
- Hover/pressed pe `Button`, textul procentual pe `ProgressBar` — tot amânate.
- Urmează: 05 input+selecție, 06 editare.

---

<a id="slice-0010-05"></a>

## SLICE-0010-05 — KBotDataView: input + selecție

A cincea trecere din slice-ul 0010. Grila devine INTERACTIVĂ: celulă curentă, navigație de la
tastatură în stil Access, click/dublu-click, comutarea bifelor/opțiunilor, acționarea butoanelor
și redimensionarea coloanelor din antet. Totul respectă activarea efectivă definită în 0010-04.

### Ce s-a schimbat și de ce

#### Selecție
`CurrentRowIndex`, `CurrentColumnKey`, `CurrentRow` + evenimentul `SelectionChanged`. Selecția
trece printr-un punct UNIC (`SetCurrentCell`), care derulează la rând (`EnsureVisible`),
repictează și ridică evenimentul **o singură dată și doar la o schimbare reală** (același
contract ca la `KBotNavList`). Index de rând în afara intervalului => „fără selecție” (-1);
cheie de coloană necunoscută => `ArgumentException` (fără no-op tăcut). Selecție simplă în v1;
rândul curent se pictează cu spălarea de accent introdusă în 0010-02.

#### Tastatură (stil formular continuu Access)
`IsInputKey` revendică săgețile / Tab / Enter / F2 / PageUp / PageDown / Home / End / Space —
altfel WinForms le-ar folosi pentru schimbarea focusului. Apoi:
Sus/Jos = rând, Stânga/Dreapta = coloană (**sar peste coloanele ascunse și dezactivate**, fără
wrap), **Enter = un rând mai jos** (senzația Access), Tab/Shift+Tab = coloană dreapta/stânga,
PageUp/PageDown = o pagină vizibilă, Home/End = prima/ultima coloană activă,
Ctrl+Home/Ctrl+End = primul/ultimul rând, Space = comută/acționează celula curentă.

#### Mouse
`OnMouseDown` ia focusul (ca `KBotNavList`) și selectează celula; `OnMouseUp` comută/acționează
și ridică `CellClick`; `OnMouseDoubleClick` ridică `CellDoubleClick` (intrarea în editare se
adaugă în 0010-06). Rotița era deja cablată din 0010-02.

#### Comutare / acționare (`ActivateCell`)
Punct comun pentru click și Space, care **verifică întâi `IsCellEnabled`** — o celulă inertă nu
face nimic (nici valoare schimbată, nici eveniment). Apoi:
- **CheckBox** — comută și ridică `CellValueChanged`.
- **OptionButton** — selectează prin `SetOptionValue` (stinge surorile din grup) și ridică
  `CellValueChanged`. Un radio deja bifat **nu se de-bifează** (no-op, fără eveniment) — așa se
  comportă un grup radio.
- **Button** — ridică doar `ButtonClick`; **nu** ține valoare, deci **nu murdărește rândul**.

`ActivateCell` e `Friend`, ca testele să-l poată chema headless (nu se pot trimite taste reale
fără buclă de mesaje) — același tipar pe care planul îl cere pentru editare în 0010-06.

#### Redimensionare de coloană
Drag pe marginea dreaptă a unei coloane din antet (toleranță ~4px, DPI-scalată), doar dacă
`Column.Resizable`. Cursorul devine `SizeWE` deasupra unei margini. Lățimea nu coboară sub
`MinWidth` (clamp-ul e în model, din 0010-01). Reordonarea prin drag rămâne în afara scopului.

#### Evenimente noi
`KBotCellEventArgs` (bază), `KBotButtonClickEventArgs`, `KBotCellValueEventArgs` — **NU** se
refolosesc, spre deosebire de argumentele de formatare: evenimentele de interacțiune sunt rare
(o acțiune a operatorului), deci o alocare per eveniment e irelevantă și scapă handler-ele de
capcana „nu reține instanța”.

### Fișiere atinse

- `src/KBot.Controls/KBotDataView.Input.vb` (nou — selecție, hit-testing, tastatură, mouse, resize)
- `src/KBot.Controls/KBotDataView.vb` — `_currentColumnKey`, cele cinci evenimente de interacțiune
- `src/KBot.Controls/Events/KBotCellEventArgs.vb`, `KBotButtonClickEventArgs.vb`,
  `KBotCellValueEventArgs.vb` (noi)
- `tests/KBot.Controls.Tests/KBotDataViewInputTests.vb` (nou, 18 teste)

### Rezultate teste

- `dotnet test KBot.sln` — **141 passed / 0 failed / 0 skipped** (57 în `KBot.Controls.Tests`).
- Acoperire nouă: `NextEnabledColumn` (un pas, sare peste dezactivate ȘI ascunse, fără wrap,
  fără punct de plecare => prima/ultima activă), selecția (eveniment doar la schimbare reală,
  index invalid => -1, `CurrentRow` urmează indexul, cheie necunoscută => excepție),
  `EnsureVisible` (derulează în jos ca rândul să încapă COMPLET în fereastră, derulează înapoi
  sus, ignoră un index invalid), și comutarea (bifă dus-întors, opțiune care stinge sora,
  radio deja bifat = no-op, buton care nu murdărește rândul, celulă/rând dezactivat = nimic).

### Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat**, iar acum lipsa lui acoperă și interacțiunea:
  navigația cu tastatura, drag-ul de redimensionare și senzația de derulare **nu pot fi testate
  headless** (nu există buclă de mesaje). Testele acoperă LOGICA din spatele handler-ilor, nu
  traseul WinForms tastă→handler. Se validează rulând harness-ul (DevHarness → Controls/UI).
- Hover/pressed pe celulele `Button` — **tot neimplementat**; acum ar fi posibil (există
  urmărire de mouse), dar nu era cerut de acceptanța pasului. Rămâne datorie mică.
- F2 e revendicat de `IsInputKey`, dar încă nu face nimic — intrarea în editare vine în 0010-06.
- Selecție multiplă, reordonare de coloane — în afara scopului v1.
- Urmează: **0010-06** (editare: editorii flotanți, commit/veto/cancel, `IsDirty` real).

---

<a id="slice-0010-06"></a>

## SLICE-0010-06 — KBotDataView: editare in-place (ultima trecere)

Ultima trecere din slice-ul 0010 — motivul pentru care grila e NELEGATĂ de date. Peste celula
activă plutește UN SINGUR editor real (TextBox sau ComboBox, declarați în Designer încă din
0010-01), deci numărul de handle-uri rămâne constant indiferent de câte mii de rânduri sunt.

### Ce s-a schimbat și de ce

#### Ciclul de editare
`BeginEdit` → (Enter / Tab / pierdere de focus / mutare de celulă / derulare) → `CommitEdit`,
sau Esc → `CancelEdit`. Cele trei sunt `Friend`, ca testele să le conducă headless (editorii
reali au nevoie de buclă de mesaje pentru focus și taste).

- **Editabilitate** (`CanEdit`, public): `Not ReadOnlyGrid AndAlso Not Column.ReadOnly AndAlso
  IsCellEnabled(...) AndAlso (tipul e Text sau Combo)`. Refolosește rezoluția de activare
  efectivă din 0010-04 — regula nu e reimplementată.
- **Intrare în editare**: F2, dublu-click. Editorul se poziționează pe dreptunghiul real al
  celulei (`CellRect`, care ține cont de banda înghețată, de ambele derulări și de antet), se
  tematizează, primește valoarea formatată și select-all.
- **Commit**: ridică `CellValidating`, care poate **respinge** (`Cancel = True` => editorul
  rămâne deschis și focalizat, valoarea din rând neatinsă) sau **corecta** valoarea (handler-ul
  rescrie `ProposedValue` — trim/normalizare/conversie de tip). Dacă trece: scrie în rând,
  marchează rândul editat, ascunde editorul, ridică `CellValueChanged`, apoi aplică navigația
  în așteptare (Enter => un rând mai jos, Tab => o coloană la dreapta).
- **Un singur editor viu**: `BeginEdit` pe altă celulă, mutarea celulei curente și derularea
  comit întâi. Dacă acel commit e respins, **mutarea nu are loc** — altfel editorul ar rămâne
  agățat peste o celulă care nu mai e a lui.

#### Semantica `IsDirty` — CONTRACT SCHIMBAT față de 0010-01
Planul cere explicit: „murdar” = **editat de operator**, nu „scris programatic”. Prin urmare
`KBotDataRow.Item` **nu mai** ridică `IsDirty` (nici `KBotDataView.Item`) — scrierea prin API e
ÎNCĂRCARE de date. Steagul se ridică doar la: commit de editare, comutare de bifă, selectare de
opțiune. `GetDirtyRows()` întoarce deci exact rândurile atinse de operator, iar `ClearDirty()`
(nou, la nivel de grilă) + `KBotDataRow.MarkClean()` (per rând) resetează baseline-ul după
încărcare sau după trimiterea modificărilor la server.

**Două teste din 0010-01 au fost rescrise** ca să reflecte noul contract
(`Row_SetStoresWithoutMarkingDirty`, `Item_GetSetThroughControl_DoesNotDirtyTheRow`) — schimbare
deliberată, nu o „reparare” a testelor ca să treacă.

### Două bug-uri REALE prinse aici (aceeași capcană VB)

VB.NET e **case-insensitive**, deci un parametru ascunde o proprietate cu același nume, iar o
atribuire nekalificată nimerește parametrul, nu proprietatea. Compilatorul nu spune nimic.

1. `KBotCellValidatingEventArgs.New`: `ProposedValue = proposedValue` era un **no-op** =>
   commit-ul scria mereu `Nothing` în celulă. Prins de testele de editare.
2. `KBotDataColumn.New`: `HeaderText = If(headerText, String.Empty)` era un **no-op** =>
   **TOATE antetele de coloană erau `Nothing`** încă din 0010-01. Nicio pictare nu arunca, deci
   nici testele de virtualizare/tipuri nu-l puteau vedea — ar fi apărut ca un rând de antet gol
   la prima rulare a harness-ului.

Ambele sunt reparate cu `Me.` explicit (+ comentariu de avertizare) și au acum **teste de
regresie**. A treia instanță a aceleiași capcane a fost prinsă la compilare mai devreme, în
bucla `For Each item In col.ComboItems` (`item` se lega la proprietatea `Item` a clasei).

### Fișiere atinse

- `src/KBot.Controls/KBotDataView.Editing.vb` (nou — ciclul complet + `CellRect` + editorii)
- `src/KBot.Controls/Events/KBotCellValidatingEventArgs.vb` (nou; bug reparat)
- `src/KBot.Controls/KBotDataColumn.vb` — bug `HeaderText` reparat
- `src/KBot.Controls/KBotDataRow.vb` — `Item` nu mai murdărește; documentație de contract
- `src/KBot.Controls/KBotDataView.vb` — `ClearDirty`, `GetDirtyRows` redocumentat, `WireEditors`
- `src/KBot.Controls/KBotDataView.Input.vb` — F2, dublu-click => editare; auto-commit la mutare
- `src/KBot.Controls/KBotDataView.Layout.vb` — auto-commit la derulare
- `tests/KBot.Controls.Tests/KBotDataViewEditingTests.vb` (nou, 15 teste) + 2 teste rescrise
  + 1 test nou de regresie pe `HeaderText`

### Rezultate teste

- `dotnet test KBot.sln` — **158 passed / 0 failed / 0 skipped** (74 în `KBot.Controls.Tests`).
- Acoperire nouă: `CanEdit` (doar Text/Combo; fals pe `ReadOnlyGrid`, coloană read-only sau
  dezactivată, rând dezactivat, celulă dezactivată din eveniment), commit (scrie + murdărește +
  ridică `CellValueChanged` cu old/new corecte), veto (editor deschis, valoare neatinsă, nimic
  murdar), coerciția valorii din handler, Esc (nimic scris, niciun eveniment), un singur editor
  viu, auto-commit la mutare, **veto care blochează mutarea selecției**, refuzul unei celule
  needitabile, commit/cancel fără editare deschisă, `ClearDirty`, și `BeginUpdate/EndUpdate`
  care lasă totul curat.

### Rămas neverificat / amânat (starea finală a slice-ului)

- **Verdictul vizual/interactiv uman NU a fost dat — pentru NICIUNA dintre cele șase treceri.**
  Nimeni n-a rulat încă harness-ul. Ce NU poate fi verificat headless: fluența derulării,
  traseul WinForms tastă→handler, drag-ul de redimensionare, poziționarea reală a editorului
  flotant peste celulă, focusul și select-all-ul, și cum arată efectiv culorile. Bug-ul cu
  antetele gol arată exact cât de necesară e proba vizuală: testele nu-l puteau prinde.
  **Rulează:** DevHarness (pornire Debug → «Nu») → Controls/UI → «KBotDataView — virtualizare
  + temă (5.000 × 20)».
- **Neimplementat din plan** (asumat, nu uitat): intrarea în editare prin simpla TASTARE a unui
  caracter (F2 și dublu-click funcționează); hover/pressed pe celulele `Button`; textul
  procentual pe `ProgressBar`.
- `KBotDataColumn.Frozen` rămâne metadata neutilizată — mecanismul autoritar e
  `FrozenColumnCount`. De curățat sau de cablat.
- În afara scopului v1 (conform planului): scrollbar-uri tematizate (rămân cele WinForms
  standard — singura gaură de tematizare), sortare/filtrare în antet, selecție multiplă,
  reordonare de coloane prin drag, orice fel de data binding.
- Urmează: slice-ul **Sumar**, care consumă grila în mod read-only (`ReadOnlyGrid = True`).

---

<a id="slice-0010-07"></a>

## SLICE-0010-07 — KBotDataView: proprietatea ScrollByColumn

Adăugare cerută după închiderea slice-ului 0010: o proprietate `ScrollByColumn` care face
derularea ORIZONTALĂ să avanseze o coloană întreagă odată, în loc de pixel cu pixel.

### Ce s-a schimbat și de ce

- **`Public Property ScrollByColumn As Boolean`** (implicit False), în partiala `.Layout`. Când
  e True, valoarea barei orizontale se **aliniază la marginile coloanelor** din banda derulată.
  Nu atinge derularea verticală — aceea e deja „pe rând”, prin virtualizare.
- **Aliniere în DIRECȚIA mișcării** (`SnapHScrollToColumn` / `SnappedHValue`): la creștere se
  urcă la marginea următoare (`CeilToColumnStart`), la scădere se coboară la cea precedentă
  (`FloorToColumnStart`). Așa un pas mic — o săgeată, o rotiță — tot avansează o coloană
  întreagă, în loc să se lipească de aceeași margine (capcana unei alinieri „la cea mai
  apropiată”). Valoarea aliniată e limitată la maximul util al barei (`Maximum − LargeChange
  + 1`), deci ultimele coloane rămân accesibile chiar dacă capătul nu cade pe o margine.
- **Aliniere pe două căi, după cum se derulează:**
  - *Săgeți / șină / rotiță* — trec prin `ValueChanged` și se aliniază DIRECȚIONAL (ceil la
    creștere, floor la scădere), cu gardă de reintrare `_snappingHScroll`.
  - *Tragerea thumb-ului cu mouse-ul* — se ascultă evenimentul `ScrollBar.Scroll` (care spune
    CUM se derulează, spre deosebire de `ValueChanged`). Cât timp `ThumbTrack`, derularea e
    **liberă** (pixel cu pixel), altfel alinierea ar smuci thumb-ul înapoi la fiecare mișcare
    a mouse-ului („se refresh-uia oribil, de parcă nu se decidea ce coloană să arate”).
    Alinierea se face DOAR la `EndScroll` (eliberare), la marginea cea mai APROPIATĂ.
  - **Bug de comportament reparat:** prima versiune alinia și în timpul tragerii (tot prin
    `ValueChanged`) => thumb-ul sărea. Corecția e distincția track-vs-release prin `Scroll`.
- **Activarea aliniază pe loc**: setter-ul, când trece pe True, aliniază imediat poziția
  curentă (care putea fi la mijloc de coloană).
- **Re-aliniere după relayout**: `UpdateScrollBars` re-aliniază la final, fiindcă lățimile se
  pot schimba sub auto-size-ul din slice 0013.

### Fișiere atinse

- `src/KBot.Controls/KBotDataView.Layout.vb` — proprietatea + mecanica de aliniere.
- `tests/KBot.Controls.Tests/KBotDataViewScrollByColumnTests.vb` (nou, 7 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` + `.Designer.vb` — checkbox
  «Derulare pe coloană» care comută proprietatea, ca proba vizuală să o poată exercita.

### Rezultate teste

- `dotnet build src/KBot.Controls` și `src/KBot.DevHarness` — **0 warnings / 0 errors**.
- `KBot.Controls.Tests` — **111 passed / 0 failed** (12 pentru `ScrollByColumn`: 7 direcționale
  + 5 pentru tragerea thumb-ului — liberă în timpul tragerii, „nearest” la eliberare, inertă
  când proprietatea e off, iar săgețile rămân direcționale după eliberare). Tragerea se
  simulează prin `BeginHorizontalThumbDrag`/`EndHorizontalThumbDrag` (Friend) — evenimentul
  `Scroll` nu se poate ridica headless.
- Grila de test e fixată pe dimensionare MANUALĂ (`AutoSizeColumnsMode = None`,
  `ColumnFillMode = None`) ca marginile de coloană să fie deterministe (5 × 100px => marginile
  0/100/200/300/400, maximul util 300). Acoperit: implicit = pixel; creștere => marginea
  următoare; scădere => marginea precedentă; un pas mic tot avansează o coloană; alinierea nu
  depășește maximul util; activarea aliniază offset-ul curent; o poziție deja pe margine nu se
  mișcă.

### Notă de mediu

Build-ul întregii soluții a raportat `MSB3026` (file-lock) la `KBot.App`/`KBot.App.Tests`
fiindcă Visual Studio ținea aplicația deschisă — contenție de COPIERE, nu eroare de cod.
Proiectele atinse (`KBot.Controls`, `KBot.DevHarness`) compilează curat izolat. `KBot.App` NU a
fost atins (proprietatea e pur aditivă pe control).

### Rămas neverificat

- La fel ca tot slice-ul 0010: **verdictul vizual uman NU a fost dat.** Cum se simte alinierea
  la derulare (thumb/săgeți/rotiță) se validează rulând harness-ul (DevHarness → Controls/UI →
  «KBotDataView — virtualizare + temă») și bifând «Derulare pe coloană». Testele acoperă
  matematica alinierii, nu senzația.
