# SLICE-0017-01 — `KBotDataView`: pinned totals row

Pass 1 of slice 0017 (Plăți). A reusable control feature in `KBot.Controls`, deliberately
**not** special-cased for Plăți — Plăți (pass 03) just switches it on and tags its columns.

## What changed and why

A pinned totals band at the bottom of `KBotDataView`, painted with the header's band styling,
that aggregates each column independently. Plăți's LISTA needs a Sum on the money column; the
control gains the general feature (Sum / Count / Average) so any view can use it.

### Public surface

- New enum `KBotAggregate { None, Sum, Count, Average }` (`KBotAggregate.vb`).
- `KBotDataColumn.Aggregate As KBotAggregate` (default `None`) and
  `KBotDataColumn.AggregateFormatString As String` (optional).
- `KBotDataView.ShowTotalsRow As Boolean` (default `False`).
- `KBotDataView.TotalsRowHeight As Integer` (defaults to `HeaderHeight`; a non-positive value
  restores that default via a `<= 0` sentinel, so it tracks HeaderHeight until set).
- `Friend KBotDataView.DebugTotalsText(colKey)` — headless test hook, mirrors
  `DebugLastPaintedDataRows`.

### Behaviour (as built)

- **Pinned band, not a row.** The band sits between the scrollable body and the horizontal
  scrollbar. It is never in `_rows`, so it is automatically excluded from `RowCount`, `Rows`,
  virtualization, selection, keyboard navigation, hit-testing (a click in the band returns row
  `-1` because it is below `ViewportHeight()`), `GetDirtyRows`, and conditional formatting.
- **Header band styling, reused not duplicated.** `DrawTotalsRow` reuses `_bHeaderBack`,
  `_pHeaderSep`, `_pHeaderBaseline` and `_cHeaderText` — the same colour reads `DrawHeader`
  uses — plus the semibold `HeaderFont()`. The separating rule + accent are drawn on the band's
  **top** edge (between body and totals), mirroring the header's bottom baseline.
- **Frozen columns + horizontal scroll.** `DrawTotalsRow` mirrors `DrawHeader`'s
  frozen-over-scroll layering exactly (scroll band clipped, frozen band repainted opaquely
  over it), so a totals cell always sits under its column, including with `ScrollByColumn`
  engaged.
- **Aggregates over ALL rows**, not just visible ones. `Sum`/`Average` skip `Nothing` and
  non-numeric cells (real numeric primitives always count; a numeric *string* parses;
  Boolean / non-numeric string / object are skipped). `Average` over zero countable cells
  renders **empty** — never `0`, never `NaN`. `Count` counts **rows whose cell `HasValue`**
  (state present, even a stored `Nothing`), NOT the count of non-empty numeric cells — stated
  and tested explicitly.
- **Formatting.** `Count` always a plain integer (ignores every format string). `Sum`/`Average`
  use `AggregateFormatString` if set, else the column's `FormatString`, else `ToString` — all
  in `CultureInfo.CurrentCulture`, matching the body's `FormatValue`.
- **Recompute triggers.** Cached formatted text per column, recomputed (guarded against
  BeginUpdate storms — runs once at `EndUpdate`) on: `AddColumn`, `AddRow`, `ClearRows`,
  `EndUpdate`, a committed edit (`CommitEdit`), the control's per-cell `Item` setter
  (`dv(col,row)=`), and `ShowTotalsRow` turning on. Paint never re-aggregates.
- **Layout.** `ViewportHeight()` and `UpdateScrollBars` both subtract `TotalsBandHeight()`, and
  `AutoSize.WillVScrollBeVisible` subtracts it too, so the auto-size vscroll prediction and the
  real scrollbar agree (slice-0013 takes the same measurement).

## Decisions taken / deviations from the plan

- **`AddRow` recompute is "one step behind", so the per-cell `Item` setter recomputes too.**
  The plan lists `AddRow` as a recompute trigger, but a caller sets the new row's cells *after*
  `AddRow` returns, so a recompute at `AddRow` sees the row still empty. Rather than drop the
  `AddRow` trigger (kept, harmless), I added a recompute in the control's `Item` setter
  (`dv(col,row)=value`) — the real per-cell write API — and rely on `EndUpdate` for bulk loads.
  Views load through `KBotDataRow`'s own indexer inside `BeginUpdate`/`EndUpdate` (Rezervări,
  Recepții both do), which the control cannot observe per-cell but recomputes once at
  `EndUpdate`. This is a deviation from the plan's literal trigger list, in the plan's spirit
  ("totals stay correct as the model changes"). A recompute in the `Item` setter does **not**
  dirty the row — totals reflect current data regardless of the load-vs-edit `IsDirty` contract.
- **Auto-size: the totals text DOES participate in measurement.** Stated as a required decision
  in the plan. `MeasureColumnToContent` includes the formatted aggregate (measured with the
  header font + header padding, since that is how the band paints it), so a wide total cannot
  ellipsize. This is the chosen behaviour, not a limitation.
- **`Sum` of nothing renders `0`** (formatted), not empty — only `Average` of nothing is empty
  (per the plan). A money totals cell reading `0,00` on an empty grid is the natural total.

## Files touched

- `src/KBot.Controls/KBotAggregate.vb` — **new** enum.
- `src/KBot.Controls/KBotDataColumn.vb` — `Aggregate`, `AggregateFormatString`.
- `src/KBot.Controls/KBotDataView.vb` — state, `ShowTotalsRow`/`TotalsRowHeight`, recompute
  hooks in `AddColumn`/`AddRow`/`ClearRows`/`EndUpdate`/`Item` setter.
- `src/KBot.Controls/KBotDataView.Totals.vb` — **new** partial: compute + format + `TryNumeric`
  + `DebugTotalsText`.
- `src/KBot.Controls/KBotDataView.Layout.vb` — `TotalsBandHeight()`, `ViewportHeight()` and
  `UpdateScrollBars` subtract it.
- `src/KBot.Controls/KBotDataView.Painting.vb` — `DrawTotalsRow`/`DrawTotalsCell` + `OnPaint`.
- `src/KBot.Controls/KBotDataView.AutoSize.vb` — measure totals text; subtract band in
  `WillVScrollBeVisible`.
- `src/KBot.Controls/KBotDataView.Editing.vb` — recompute after `CommitEdit`.
- `src/KBot.DevHarness/Internal/DataViewHarnessForm{.vb,.Designer.vb}` — «Rând de totaluri»
  checkbox; `nr`=Count, numeric columns=Sum, first numeric=Average.
- `tests/KBot.Controls.Tests/KBotDataViewTotalsTests.vb` — **new**, 14 tests.

## Test results

- `KBotDataViewTotalsTests`: 14/14 green.
- Full `KBot.Controls.Tests`: **134** green, 0 failures.
- `KBot.Controls` + `KBot.DevHarness` build: 0 warnings, 0 errors.

## Left unverified / deferred

- **The totals band has never been rendered on screen.** It is a new painted band — the
  frozen-column *alignment*, the band styling, the baseline rule and the actual colours are all
  eyeball concerns. `KBotDataView`'s visual harness is **still unrun** (open since 0010). Run
  DevHarness → the `KBotDataView` probe → tick «Rând de totaluri» before trusting the paint.
  The headless tests cover computation, exclusion and layout math; frozen alignment is only
  smoke-tested (a frozen column's total computes and a full paint with H-scroll does not throw).
