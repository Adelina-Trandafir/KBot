# SLICE-AV02-05 - «Modificare operatii» (port of Access `ModificareNC1280`)

Plan: `docs/PLANS/PLAN_AV02_05_MODIFICARE_OPERATII.md`. Reading sheet: `docs/access-map/avacont/modificare-nc1280.md` (confirmed by the
operator 10.11.2026, answers in its section 0.1). Help: `SLICE-0000-177-ajutor-modificare-operatii.md`.

## What changed and why

The operator asked (09.11.2026) to port the Access window «Modificare / Stergere operatii contabile» with its ribbon. Decisions: page in
AvacontMain, list + delete only (no modify until the `Note` form is ported), prints to stage 6, ORD/PROP/ANG eliminated, CH and CF as ticks,
«Ord» = state of the ordonantare and «R» = reception, both as ICONS (operator 10.11.2026), several column filters at once, delete refused
with an ordonantare, partner balance with the opening balance, sort and panel remembered per user, closing / salaries / Centralizator
lists later, the button in a NEW ribbon tab «Introducere date» (group «Introducere Date» gets the moved «Operatii», group «Modificare Date»
the new button).

- **Server** `PYTHON/routes/avacont/modificare_date/operatii/`:
  - `citire.py` - one SELECT for the list and the delete checks (one row per `TZ_Oper`: every join by a key giving at most one row; the
    debit/credit side through its first line, the amount = sum of the side); `stare_ord` (FX_ORD_TBL / FX_ORD.Semnatura roles AB, CD,
    Ordonator), `stare_receptie` (all / some invoices with a reception), `motiv_refuz` (the delete rules).
  - `lista.py` - `GET /api/avacont/modificare-operatii/lista`, `GET .../info/<idoper>` (panel: invoice paid INTEGRAL/PARTIAL, OP, invoices
    paid, partner balance = opening balance + turnover before the opening + operations of the subunit and year, to the document date and annual).
  - `stergere.py` - `GET .../operatie|document/<id>/verifica`, `DELETE .../operatie|document/<id>`: one transaction, rows locked, the same
    checks again; a payment with an OP through `plati.sterge_plata`; other operations: their allocations as a payment, the link
    `EF_PrimiteOperatii` (the invoice becomes bookable again), `IdEvidentiereAnterior` cleared, debit/credit lines, operation; empty document
    and empty note removed. Every delete logged on the server (details) and in `Jurnal` (labels in `routes/actiuni.py`).
  - `introducere_date/operatii/plati.py`: `primite_cu_receptie` (the `are_receptie` rule for many invoices at once).
  - registered in `routes/avacont/__init__.py`. No DDL.
- **Client**: `KBot.Domain/Avacont/ModificareDate/Operatii/ModificareOperatiiInfo.vb`; `KBot.Api/Avacont/ModificareDate/Operatii/`
  (`IModificareOperatiiApi`, `ApiClient.ModificareOperatii.vb`); `KBot.App/Avacont/ModificareDate/Operatii/`: `KbotForm.ModificareOperatii.vb`
  (page, one instance, macheta `modificare_operatii`), `ModificareOperatiiIcons.vb` (Tip letter badge, Ord: hollow / half / three quarters /
  full circle with tick; R: full square with tick / square with one filled corner), `ModificareOperatiiExcel.vb`, `Views/ModificareOperatiiForm`
  (+ `.Lista`, `.Info`, `.Stergere`, `.Ribbon` partials, each in `KBot.App.vbproj` as code).
- **AvacontMain**: new tab «Introducere date» (after Configurari; `SelectedTabIndex` 1 -> 2 so «Angajamente» still opens selected), the
  eight view-group buttons of the page, keys in `ViewGroupKeys` and `Ribbon_ItemClicked`.
- **KBotDataView** (any grid can use it): `KBotCellFormattingEventArgs.CellImage` (a picture drawn instead of the text of a Text cell; sort,
  filter and tooltip keep the text) and `VisibleRows()` (rows as shown). The several-filters-at-once request was ALREADY in the grid (slice
  0028-03, one filter per column, all applied together); the page only turns the filter on for every column.
- `AppSettings`: `ModificareOperatiiSortKey`, `ModificareOperatiiSortDesc`, `ModificareOperatiiInfo`.
- Versions: KBot.Controls 1.85.2.0, KBot.Common 1.5.15.0, KBot.Api 1.0.31.0, KBot.Domain 1.2.22.0 (KBot.App is bumped by `push-update.ps1`).

## Files touched

- `PYTHON/routes/avacont/modificare_date/__init__.py`, `.../operatii/__init__.py`, `citire.py`, `lista.py`, `stergere.py` (new);
  `PYTHON/routes/avacont/__init__.py`, `PYTHON/routes/actiuni.py`, `PYTHON/routes/avacont/introducere_date/operatii/plati.py`
- `src/KBot.Domain/Avacont/ModificareDate/Operatii/ModificareOperatiiInfo.vb` (new), `src/KBot.Domain/KBot.Domain.vbproj`
- `src/KBot.Api/Avacont/ModificareDate/Operatii/IModificareOperatiiApi.vb`, `ApiClient.ModificareOperatii.vb` (new), `src/KBot.Api/KBot.Api.vbproj`
- `src/KBot.App/Avacont/ModificareDate/Operatii/*` (new), `src/KBot.App/Avacont/AvacontMain.vb`, `AvacontMain.Designer.vb`,
  `src/KBot.App/KBotForm/Documents/KbotForm.Machete.vb`, `src/KBot.App/KBot.App.vbproj`
- `src/KBot.Controls/DataView/Events/KBotCellFormattingEventArgs.vb`, `KBotDataView.Painting.vb`, `KBotDataView.Filtering.vb`, `KBot.Controls.vbproj`
- `src/KBot.Common/AppSettings.vb`, `KBot.Common.vbproj`
- Help: see `SLICE-0000-177`. Docs: the plan, the reading sheet, this worklog, `KBOT_STATUS_AV.md`, `KBOT_STATUS.md`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings (also KBot.Api and KBot.Controls alone: 0 / 0).
- Python: the five files byte-compiled with `PYTHON\.venv` (no import / no run).
- Nothing run: no test, no server call, the page was never opened on screen.

## Left unverified or deferred

- **Never run.** The SQL of `citire.py` was written against `MariaDB_Schema/` and the AV02-01/02 DDL, not run on a database: the row counts
  around the joins are NOT checked. To check: the list of a subunit/year has as many rows as `TZ_Oper` of it.
- The Python files must be published to the server by the operator; the page answers 404 until then.
- «Ord» signature roles read as AB, CD, Ordonator (`routes/forexe/pdf.py:125`), shown in that order as the stages; an ORD signed by CD but not
  AB shows «Semnata CD».
- «R» of an operation = its invoices: an evidence through `EF_PrimiteOperatii`, a payment through `TZ_PlatiFacturi.IdPrimita` (an invoice
  typed by hand has none, so no icon).
- Partner balance: opening balance counted as `SoldDebitor + RulajDebitor` / `SoldCreditor + RulajCreditor` of the accounts of the subunit
  (my reading of «turnover before the opening»); the partner is the debit side's, else the credit side's.
- The year rule of Access (`EInchis`: an earlier year is closed once the current month is after January) is kept with TODAY's date, so
  operations of the previous year cannot be deleted from February on.
- The delete buttons stay ACTIVE on a blocked row: a click shows the reason (no server call); planned «dimmed with a tip» was replaced because
  a ribbon button cannot carry a per-row tip.
- Access keys Shift+Up/Down (page) are not added: the grid's own PageUp/PageDown do it. Enter / double-click do nothing (no modify yet).
- Not ported (sheet): modify, «Sterge nota», prints, ORD/PROP/ANG, FX/PA columns, lists OA2/OA3/OA4.
- Access exports still missing (only for the later lists): `USysRibbons` row «MNC», `[~SQL]` 40000-40003 / 3403, table `B`.
- Help captures: none taken (page never on screen).
