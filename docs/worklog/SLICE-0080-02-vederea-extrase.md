# SLICE 0080-02 — the Extrase view + «Setări → Extrase» (24.09.2026)

## Request (operator, 24.09.2026, answers in session)

A new angajament view in the style of Rezervări / Recepții / Plăți: a tree on the left
(Toate → month → day, days from `FX_Extrase.DataBanca`); month / Toate selected → the
FX_Extrase_H rows (Clsf, SID, SIC, TSD, TSC, SFD, SFC, plus a date column; one row per header,
no summing) and under them a second grid with the operations of the selected header; day
selected → that day's FX_Extrase rows (DataBanca first, NrDoc, platitor, CUI, IBAN, debit,
credit) and under them the selected one in full, like Plăți. Only the selected angajament's rows
(`CodContract` = CodAngajament; `RandContract` = CodIndicator). TOTALURI on debit / credit only.
The tree footer's right icon downloads the statements (what the main tree's left icon did) and
the view reloads. A new «Extrase» page in Setări lets the operator choose the columns of the
grids and their order, separately for the view and the window, with «Revino la implicit»;
DataBanca, Clsf and Plătitor get the filter icon, Clsf and DataBanca also grouping. The
defaults are authored in the designer.

Placement decided by the operator: a header sits under every day on which one of its
operations has a DataBanca; one without operations sits on its statement's DataExtras; the
«Data» column shows DataExtras. One operations layout serves both the day grid and the lower
grid of a window.

## What changed

- **Server** `routes/forexe/extrase_lista.py` (new): `GET /api/forexe/extrase/lista[?cod=]` →
  `{antete, operatiuni}`; with `cod` only operations with that CodContract and their headers.
  `routes/forexe/tree.py`: new flag `AreExtrase` (EXISTS FX_Extrase with CodContract).
- **Client**: `IApiClient.GetExtraseListaAsync`, wire DTOs, `KBot.Domain/ExtraseInfo.vb`
  (`ExtraseInfo`, `ExtrasAntet`, `ExtrasOperatiune`); `AreExtrase` through DTO → domain →
  gating. The nine test fakes got a `NotSupportedException` stub.
- **`KBot.Common/ExtraseColumns.vb`** (new): `ExtraseGrid` (4 grids), the catalogue (ASCII keys,
  Romanian captions), the defaults, `Normalize`. **`AppSettings`**: four lists + DTO keys,
  `ExtraseColumnsFor` / `SetExtraseColumns` (Nothing = defaults).
- **`Views/Extrase/ExtrasePanel`** (new, Designer + code): the shared body. Every catalogue
  column declared in the designer, defaults visible; `ExtraseLayout.Apply` reorders / hides at
  runtime and again on `AppSettings.Changed`. Mode `Angajament` / `Toate` (window: four more
  detail rows — Cod angajament, Indicator, Referință destinatar, Cod program).
- **`Views/ExtraseView`** (new): `IAngajamentView` key `extrase`; loads with `cod`; footer icon
  → shell download → reload when something was imported.
- **`KBot.Controls`**: `KBotDataColumn.AllowGrouping` (default True) + the filter popup shows
  the «Grupare» tab only where it is True — needed to give Plătitor a filter without grouping.
- **Shell**: nav item «Extrase» (after Plăți, icon `binvoice`), `CreateView("extrase")`, gating
  on `AreExtrase`; the download moved into `DescarcaExtraseAsync(owner)` (returns True when the
  import wrote something), shared with the window (0080-03).
- **Setări**: `SetariExtraseView` (new, Designer + code), nav key `extrase` (everyday page):
  combo for the grid, checklist grid, Sus / Jos / Revino la implicit / Salvează; refuses a grid
  with no column ticked.

## Files touched
PYTHON: `routes/forexe/extrase_lista.py` (new), `routes/forexe/__init__.py`,
`routes/forexe/tree.py`, `tests/test_forexe_tree.py`, `tests/test_clsf_pair.py` (row mappers).
src: `KBot.Api/{IApiClient,ApiClient,UpsertAngajamenteRequest}.vb`,
`KBot.Domain/{ExtraseInfo(new),AngajamentTreeInfo}.vb`, `KBot.Common/{ExtraseColumns(new),
AppSettings}.vb`, `KBot.Controls/DataView/{KBotDataColumn,Filter/KBotFilterPopup}.vb`,
`KBot.App/Views/Extrase/{ExtrasePanel(.Designer),ExtraseLayout}.vb` (new),
`KBot.App/Views/ExtraseView(.Designer).vb` (new), `KBot.App/KbotForm(.Designer).vb`,
`KBot.App/Setari/SetariExtraseView(.Designer).vb` (new), `KBot.App/Setari/SetariForm(.Designer).vb`.
tests: `KBot.Common.Tests/ExtraseColumnsTests.vb` (new), nine `KBot.App.Tests` fakes.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj --no-incremental`: **0 errors, 0 warnings**
(Api / Controls / Common also 0 / 0). No test run (operator rule). Seen on screen via
`DrawToBitmap` with invented data (scratch renderer, not in the repo): the panel at the root
(headers + the first header's operations), a day (operations + detail, window mode with the
four extra rows) and the Setări page. Date columns widened after the first render.

## Unverified / deferred
- Never run against a server; the route never ran.
- Operations with no DataBanca get no day node (still listed under their header).
- Tree nodes carry no amount on the right (the other views show money there) — not asked, not
  guessed.
- The view needs 0080-01 deployed (the route reads `DataDoc` as a date and `IdClsf` as the PK;
  on an unconverted database `Clsf` stays empty).
