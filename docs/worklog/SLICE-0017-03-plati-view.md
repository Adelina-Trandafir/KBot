# SLICE-0017-03 — client: `PlatiView`

Pass 3 of slice 0017 (Plăți). The fourth real view — after Sumar (0011), Rezervări (0014),
Recepții (0015) — replacing `MainForm.CreateView("plati")` → `PlaceholderView`.

## What changed and why

A master/detail view: an `AdvancedTreeControl` on the left, a `KBotDataView` LISTA top-right,
and a bank-statement detail pane bottom-right. Read-only. Data comes from `GET /api/forexe/plati`
through the shell's 401 net (`WithReauth(Of PlatiInfo)`) with the `_requestedCod` stale-guard.

- **Tree — three levels.** `« TOATE PLĂȚILE »` root (Σ over all rows, collapsed, first) →
  month roots (one per year/month, chronological, Σ month) → day leaves (one per calendar day,
  **all** rows of that day merged into one node, Σ day) → payment nodes (one per `IdPlataFX`,
  caption = `NrOP` falling back to `ReferintaTrezor`, value = `Suma`). This revives the dormant
  Access `Level = 2` handlers (`Show_Plati` only built two levels).
- **LISTA — filter, not aggregate.** Selecting any node shows exactly its rows (ALL → every
  row; month → month rows; day → day rows; payment → that one row). Columns: Clasificație
  (Count), Plătitor, Nr. doc, Data plății, Suma (Sum, `ro-RO` money, right-aligned). Totals row
  on (`ShowTotalsRow = True` from pass 01). `clsf` falls back to `clsf_plata` (`ClsfEfectiv`);
  Plătitor comes from the row's extras (`platitor_nume`), matching the Access LISTA.
- **Detail pane — bank statement.** Driven by the grid's row-selection event, from data already
  on the row (no second network call). Ten read-only labelled fields. No extras (`Idfxe` null)
  → `Fără extras bancar asociat.`; no grid row selected → `Selectați o plată.`
- **The «+» — exactly one day.** The earliest `DataPlata` day containing a row with
  `AreOrd = False` is marked, and only: that day leaf, its parent month root (Access:
  `cLeaf.ParentNode.IconRight = "Plus"`), and each level-2 node under that day whose row has
  `AreOrd = False`. Nothing else, ever. Right-icon clicks raise dormant events
  (`AdaugaOrdonantariCerut` level 0, `AdaugaOrdonantareCerut` levels 1/2), no subscriber.

## Decisions / assumptions (flagged as the plan requires)

- **Day/month state icon (no Access source — assumption):** *any* row `Incarcat` → up; else
  *any* row `Preluat` → down; else neutral. Per-row level-2 nodes use the Access rule directly.
- **Day/month INCASARE colouring (assumption):** green (from `palette.SuccessColor`, not
  `RGB(0,100,0)`) only when *every* row of the day/month is INCASARE. Per-row level-2 nodes are
  green iff that row is INCASARE (Access per-row). The Access source only colours the per-row
  leaf; month/day merged colouring is the operator interpretation, stated here.
- **Months get a state icon and can be green** — the plan's icon assumption names "month roots",
  so months carry the merged state icon; I extended the same all-INCASARE colour rule to months
  for consistency (Access never colours roots, so this is additive — stated).
- **Plăți «+» has no green variant** (unlike Rezervări) — Access uses only `"Plus"` for Plăți.
  `PlatiIcons.PlusIcon` is single-tint (accent).

## Files touched

- `src/KBot.Domain/PlatiInfo.vb` — **new**: `PlataRow`, `ExtrasBancar`, `PlatiInfo`, with
  `ClsfEfectiv` / `EtichetaPlata` / `EsteIncasare` helpers.
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `GetPlatiResponse` / `GetPlataRow` (flat extras).
- `src/KBot.Api/IApiClient.vb` — `GetPlatiAsync`.
- `src/KBot.Api/ApiClient.vb` — implementation (escaped URL; flat extras folded into a nested
  `ExtrasBancar`, Nothing when `idfxe` is null).
- `src/KBot.App/PlatiIcons.vb` — **new** GDI icons (up / down / neutral / «+» / «toate»).
- `src/KBot.App/Views/PlatiView.Designer.vb` — **new** (outer split + inner split + detail
  TableLayoutPanel; all controls declared here).
- `src/KBot.App/Views/PlatiView.vb` — **new** view logic + `LunaAnEventArgs` / `PlataOrdEventArgs`.
- `src/KBot.App/MainForm.vb` — `CreateView("plati")` → `PlatiView`.
- Tests: `GetPlatiAsync` stub added to the three App `FakeApiClient`s;
  `tests/KBot.Api.Tests/ApiClientTests.vb` (+5 GetPlati); `tests/KBot.App.Tests/PlatiViewTests.vb`
  (**new**, 11).

## Test results

- Full solution: **build 0 warnings / 0 errors**; all test projects green — Api 48→53 (+5),
  App 39→50 (+11), Controls 134, Theming 27, Common 7, Domain 1, LocalStore 1.

## Left unverified / deferred

- **`PlatiView` has never been rendered on screen**, like every view before it, and
  `KBotDataView`'s visual harness (incl. the new pass-01 totals band) is **still unrun**. Run
  DevHarness before trusting the paint, especially the totals band and the tree icons/colours.
- The endpoint (pass 02) has not touched a live database; if `clsf` comes back blank the fallback
  `clsf_plata` fills the column, but the join key is the 0011-03 trap to check.
- The ordonanțare workflow itself is out of scope — the `«+»` events are raised and nothing
  listens (dormant, as in Access this slice).
