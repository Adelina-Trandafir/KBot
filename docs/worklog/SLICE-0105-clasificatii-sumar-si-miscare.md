# SLICE-0105 / 0107 — Clasificații: sumar, filtre și totaluri

Istoric reunit pe tema ferestrei de buget. Etapele din 03.10.2026 păstrează probele și limitele originale. Totalul pe rând din 0107-02 și rândul separat «Buget + rectificări» descriu suprafețe diferite; regulile de credit și total anual sunt amendate ulterior în [0108](SLICE-0102-buget-pe-versiuni.md#slice-0108-01).

[0105 — sumar și mișcare](#slice-0105) · [0107-02 — total pe rând și subsoluri](#slice-0107-02) · [0107 — aliniere, rând final și FOREXE](#slice-0107-buget)

## SLICE-0105 — Clasificatii: sumar pe noduri, filtru pe miscare, Verifica buget fara diferente zero <a id="slice-0105"></a>

Operator request, 03.10.2026. Help: 0000-42 (`contabil/nomenclatoare/clasificatii.md`).

### What changed and why

1. **Non-leaf summary, readonly:** chapter/subchapter/article selection shows both grids without Inceput/Nr. doc./Data/✕/footer + or editing, with a stretching first Clsf column. Budget: each classification below the node, latest yearly version (greatest DataInceput). Corrections: classifications with corrections, quarter sums and Total. Save/Trimite in Access disabled: no current classification.
2. **Movement filter on every opening:** any nonzero quarter in ANY yearly budget version/correction qualifies. Check quarters separately: +1000/-1000 still count despite total 0. «Arata toate clasificatiile» under the tree shows all; adding with + ticks it so new zero-movement classifications remain visible.
3. **Verifica bugetul:** zero differences never appear. BudgetCheck.Differences uses Diferenta<>0 instead of Not Egal (which included FOREXE 0 versus missing K-BOT budget). Removed redundant Doar diferentele checkbox; automatic post-download check opens only if such a difference exists.

Server: new `GET /api/forexe/nomenclatoare/clasificatii/sumar-buget?an=` in `routes/forexe/clasificatii_edit.py` (last version per classification, corrections totals, `activ`).

Assumptions (stated, not asked): slice number 0105; «last budget» = greatest start date of the year; the filter looks at all versions / corrections of the year, not only the last version.

### Files touched

`PYTHON/routes/forexe/clasificatii_edit.py`; `src/KBot.Domain/Nomenclatoare.vb` (`BudgetSummaryRow`, `BudgetCheck.Differences`); `src/KBot.Api/INomenclatoareApi.vb`, `ApiClient.Nomenclatoare.vb` (`GetBudgetSummaryAsync`); `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` + `.Designer.vb`; `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm.vb` + `.Designer.vb`; `src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. `py_compile` of the Python route: OK. No tests written or run (project rule). Nothing run live or looked at on screen.

### Unverified / deferred

- **Server not deployed**: until it is, selecting a classification still works but the tree load fails (it now also reads `sumar-buget`) and non-leaf nodes show an error in the status line.
- Not seen on screen: the stretching «Clsf» column, hidden columns toggling, the checkbox row under the tree (new `tlyBody` row, `RowSpan 2` on the right panel).
- A classification just saved to zero stays in the tree until it is rebuilt (checkbox / reopen).
- The grid tooltips (editing hints) are not switched for the summary mode.

---

## SLICE-0107-02 - «Clasificatii bugetare»: row Total on the budget grid, no footer total where it means nothing <a id="slice-0107-02"></a>

Operator request, 03.10.2026 (filed under slice 0107).

### What changed and why

- **gridBuget:** readonly Total (key total, between Trim. 4 and ✕) sums four quarters for every leaf version and every node-summary last budget; quarter edits recompute, + starts at 0. No footer aggregates for any node/leaf.
- **gridRectificari:** SetCorrectionsTotal(False) removes designer Aggregate=Sum on Trim. 1–4/Total for summaries/empty selection, where cross-classification sums are meaningless; a leaf restores them with True. Footer caption: «Rectificarile anului, pe clasificatie» in summary, empty without selection.
- Help (slice 0000-44): `contabil.nomenclatoare.clasificatii` and the tour steps «Bugetul» and «Rectificari > Total» now describe both changes; tag `0107-02` on each.

### Files touched

`src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.Designer.vb` (column `KBotDataColumn19`); `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` (`FillGrids`, `FillSummary`, `ShowNoSelection`, `ApplyMode`, new `SetCorrectionsTotal`, `Grid_CellValueChanged`, `GridBuget_FooterRightIconClicked`); Help: `HelpContent/contabil/nomenclatoare/clasificatii.md`, `HelpContent/tours/tur-clasificatii.md`

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no test runs / UI driving unless asked).

### Left unverified / deferred

- Not seen on screen: the new «Total» column (width 110, next to «Trim. 4»), the footer without sums in the summary and in the empty state, the footer sums coming back on a leaf after a summary.
- Assumption: `KBotDataColumn.Aggregate` can be changed at run time and the footer repaints (the setter calls `Owner.OnColumnAggregateChanged`); not seen.
- The «Total» of a budget row is shown for convenience only; it is not saved (the server never had one).
- The «Rectificari > Total» tour step still targets the footer; when the footer has no sums the step describes that.

---

### Also under slice 0107 (03.10.2026): «Clasificatii bugetare», aligned grids and a final row <a id="slice-0107-buget"></a>

Same slice, operator request: the two grids of `ClasificatiiForm` keep their columns on the same vertical lines, and a row under both adds the last budget and all the corrections.

- Stretching columns (budget Clsf, Nr. doc., other Clsf) authored at 150/MinWidth 150; all others 110. Equal authored totals, leaf 844/node 700, synchronize shrink/stretch. Old budget Clsf=200 shrank its whole grid on narrow panels while corrections did not.
- Leaf: the budget grid shows `Clsf` first (`Clsf | Inceput | Trim 1-4 | Total | x` over `Nr. doc. | Data | ...`).
- `KBotDataView`: `VScrollShown`, event `VScrollShownChanged` (raised after the layout pass), property `ReserveVScrollSpace`. `ClasificatiiForm.Grid_VScrollShownChanged` reserves the bar's strip on all three grids only while one of the two upper grids has its bar on.
- `gridTotal` (one row, no header / footer, read only, text «Buget + rectificari»): newest-dated budget + all corrections per quarter and Total; for a node, the last budget of every classification under it + their corrections totals. `UpdateTotalRow` follows every edit, new row and delete.
- Files: `KBotDataView.Layout.vb`, `KBotDataView.AutoSize.vb`, `ClasificatiiForm.Designer.vb`, `ClasificatiiForm.vb`; help `clasificatii.md`, `tur-clasificatii.md` (0000-46).
- `gridTotal` is not selectable (new `KBotDataView.Selectable`, default True) and its «Inceput» cell stays empty.
- chkForexe («Arata DOAR clasificatiile folosite in FOREXE»), right of Arata toate clasificatiile in tlyBife: rows existing in FX_Indicatori_Buget by IdClsf, overriding movement filtering. GET .../clasificatii adds in_forexe EXISTS/Clasificatie.InForexe; server not deployed. Verifica bugetul double click closes the window and GoToClassification follows tree-click loading, including unsaved-change prompts; a hidden leaf first shows all classifications. BudgetCheckForm.RunAsync(...,onPick), only from ClasificatiiForm; automatic post-download check passes no onPick, so double click does nothing. Id in KBotDataRow.Tag. Not seen on screen.
- check_budget (manual and automatic) always applies the same FX_Indicatori_Buget row test above, excluding K-BOT-only values without FOREXE credit (operator, 03.10.2026). Check and checkbox results stay empty until a slice-0108 download fills that table.
- Build `KBot.App`: 0 warnings, 0 errors. Not seen on screen: the alignment at the operator's DPI with and without the bar, the strip appearing on the other grids when one starts to scroll, the height of the last row.
