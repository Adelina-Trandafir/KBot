# SLICE-0010-01 — KBotDataView (schelet)

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
- `docs/worklog/KBOT_STATUS.md` — rând slice 0010 + „Next free slice number”.

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
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
