# SLICE-0010-02 — KBotDataView: render + virtualizare + scrollbar-uri

A doua trecere din slice-ul 0010. Livrează motorul de randare virtualizat: benzile de
layout, matematica de virtualizare, barele de derulare, pictarea rândurilor (Text + CheckBox),
plumbing-ul evenimentelor de formatare și **poarta de verificare headless** a virtualizării.

## Ce s-a schimbat și de ce

### Restructurare pe parțiale
Controlul a crescut, așa că l-am împărțit exact ca `AdvancedTreeControl`:
- `KBotDataView.vb` — stare + API public + evenimente
- `KBotDataView.Theming.vb` — maparea paletă→roluri, cache GDI, `Blend`/`RoundedRect`
- `KBotDataView.Layout.vb` — benzi, geometrie, virtualizare, scrollbar-uri, `EnsureVisible`
- `KBotDataView.Painting.vb` — `OnPaint` + ajutoarele de desen

### Layout / benzi
Trei zone: **antet** (sus), **bandă înghețată** (stânga, primele `FrozenColumnCount` coloane
vizibile — nu derulează orizontal) și **corpul derulat**. Offset-urile X se recalculează în
`RecalcColumnLayout` (pur, fără atingere de controale, deci sigur de chemat și din pictare) în
două liste: `_frozenLayout` / `_scrollLayout`.

### Virtualizare (înălțime fixă => aritmetică întreagă)
- `firstVisibleRow = vScrollOffset \ RowHeight`
- `visibleCount = (ViewportHeight \ RowHeight) + 2`
- `RowTop(i) = HeaderHeight + i * RowHeight - vScrollOffset`
Se pictează DOAR intervalul vizibil; costul unei pictări nu depinde de `RowCount`.

### Scrollbar-uri
`UpdateScrollBars` evaluează în **două treceri** (bara verticală mănâncă lățime, cea orizontală
înălțime — se influențează reciproc). Semantica WinForms respectată: `Maximum = conținut - 1`,
`LargeChange = fereastra`, valoarea utilă maximă = `Maximum - LargeChange + 1`; `Value` se
clamp-ează la micșorarea conținutului. Bara orizontală derulează **doar banda ne-înghețată**.
`UpdateLayout` are gardă de reintrare (`_inLayout`), fiindcă schimbarea vizibilității unei bare
declanșează layout. Rotița mouse-ului: vertical; `Shift`+rotiță: orizontal.

### Pictare
Antet (bandă derulată decupată, apoi banda înghețată desenată peste ea, separatoare, linie de
bază + accent), rânduri (fundal normal/alternant/selectat), celule **Text** (text formatat prin
`Column.FormatString`, aliniere, elipsă) și **CheckBox** (geometria din `AdvancedTreeControl` —
dreptunghi rotunjit + bifă — dar cu **culori din paletă**, nu `DodgerBlue` hardcodat ca acolo).
Liniile de grilă se trag pe marginea de jos a rândului și pe marginea dreaptă a celulei.

### Evenimente de formatare (plumbing; handler-ele bogate vin în 0010-04)
`CellFormatting` / `RowFormatting` se ridică deja, cu argumente **REFOLOSITE** (câte o singură
instanță `_cellArgs` / `_rowArgs`, resetate înainte de fiecare ridicare) — zero alocări per
celulă la mii de rânduri. Fundalul per-celulă/rând se pictează cu pensula cache-uită pe calea
rapidă și cu o pensulă temporară doar când un handler chiar a suprascris culoarea.

### Maparea paletă → roluri (fără culori literale)
Implementată conform tabelului din plan: antet = `SurfaceAlt`/`Text`/`Border`/`Accent`,
rânduri = `InputBack` și `Blend(InputBack, Surface, 0.5)`, selecție =
`Blend(fundalul REAL al rândului, Accent, 0.18)` (două variante precalculate: rând normal /
alternant, ca textul să rămână lizibil), grilă = `Border`, bifă = `Border`/`Accent`/`AccentText`.
Editorii flotanți (`editText`/`editCombo`) se tematizează direct în `ApplyTheme`.

## Fișiere atinse

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
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **104 passed / 0 failed / 0 skipped** (20 în `KBot.Controls.Tests`).
- **Poarta de virtualizare (headless)** — `KBotDataViewVirtualizationTests`:
  - `Paints_OnlyVisibleRows_NotAllFiveThousand`: 5.000 rânduri × 20 coloane într-o fereastră
    de 800×400 => se pictează `> 0` și `<= 20` rânduri.
  - `PaintedRowCount_IsIndependentOfRowCount`: numărul de rânduri pictate la 5.000 este
    **identic** cu cel la 50.000 — dovada că pictarea e O(fereastră), nu O(rânduri).
  - `EmptyGrid_PaintsNoDataRows`.
  - Metoda: `DrawToBitmap` forțează o pictare sincronă (confirmat: trece prin `OnPaint`), iar
    `DebugLastPaintedDataRows` (Friend, expus prin `InternalsVisibleTo`) raportează rezultatul.

## Rămas neverificat / amânat

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
