# SLICE-0101 — Receptions that do not close: red in the association form and in the main tree; month «+» without a success box (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (kept on this branch for now; nothing committed, files only staged).

## What changed and why

**1. Plati view, «+» on a month (batch ORD generation).** After the batch finished, a box said
«N ordonanțări au fost generate și salvate». The operator wants only the confirmation BEFORE the work.
`KbotForm.Ord.vb` `GenereazaInLotAsync`: the success box is gone, the count goes to `mesaje_operator.log`
(`OperatorLog`), as the single/group delete already do. Error boxes stay (the failure box says which day stopped and
that earlier ones stay saved), and so does «Nu există plăți neordonanțate…» (it shows before any work). The same
handler serves «Generare în lot» in the Ordonanțări view, which loses the success box too.

**2. Association form: a chain that does not close is red.** `AsociereForm.vb`, new
`ColoreazaRosuLanturileNeinchise`: when the LAST snapshot (H) of a reception (R) is not a deletion row and its
total, rounded to 2 decimals, differs from `SumaAntet`, BOTH the R row and that H row get `ErrorColor` as text
colour. It runs last in `SincronizeazaCulorile` and after the tree is built, so no other colour rule (chart line
colours, the dimmed «blocked» / «fără schimbare» grey) can override it. Same test as the existing tooltip line
«Lanțul nu se închide».

**3. Main tree (KbotForm): the same check for every angajament of the unit.**
- Server, `PYTHON/routes/forexe/tree.py`: new scalar column `LantNeinchis` (`_LANT_NEINCHIS`), a correlated
  `GROUP_CONCAT` over `FX_Receptii_R` / `FX_Receptii_H`: last snapshot of each chain (DataH, then IDRH), not a
  deletion row, has lines (`FX_Receptii`), totals differ at 2 decimals. NULL = every chain closes; otherwise
  `yyyy-MM-dd~total~suma` joined with `|` (fixed `DECIMAL(18,2)` casts, `CAST(DATE AS CHAR)` so no `%` has to cross
  the driver). The tuple unpack and the JSON got the new field.
- Client: `GetTreeRow.LantNeinchis` (wire), new POCO `KBot.Domain/ReceptieNeinchisa.vb`,
  `AngajamentTreeInfo.LantNeinchis`, `ApiClient.ParseLantNeinchis` (a piece that does not parse is skipped).
- `KbotForm.Tree.vb`: a flagged angajament's row is painted with `ErrorColor`; its tooltip gets a blank line, «⚠
  Lanțul nu se închide: ultimul instantaneu nu are valoarea recepției.» and one line per reception («Recepția din
  dd.MM.yyyy: ultimul instantaneu X, valoarea recepției Y», Romanian number format). `OnThemeChanged`
  (`KbotForm.Chrome.vb`) repaints the red rows, since the colour is explicit on the node.

## Files touched

`src/KBot.App/KbotForm.Ord.vb`, `src/KBot.App/Forexe/AsociereForm.vb`, `src/KBot.App/KbotForm.Tree.vb`,
`src/KBot.App/KbotForm.Chrome.vb`, `src/KBot.Api/ApiClient.vb`, `src/KBot.Api/UpsertAngajamenteRequest.vb`,
`src/KBot.Domain/AngajamentTreeInfo.vb`, `src/KBot.Domain/ReceptieNeinchisa.vb` (new),
`PYTHON/routes/forexe/tree.py`, `docs/release-notes/NOUTATI.md`, this file and the status files; help: see `SLICE-0000-37-ajutor-lanturi-neinchise.md`.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App` → 0 warnings, 0 errors. `tree.py` parsed
with `ast.parse` only. The SQL was checked by the operator on `014_SCSV` (standalone version of the same query):
`EXPLAIN` showed index lookups only; the result was 5 of 39 angajamente flagged (AAB2XDKG7DF 4 receptions,
AAB2D6TB4X5 2, AAB2NSMCNBR 2, AAB39RD2PHB 2, AAB2HEX3DEC 1) — real mismatches, not noise.

## Left unverified / deferred

- Nothing was run or seen on screen: the red rows, the new tooltip and the missing success box are unseen.
- `PYTHON/tests/test_forexe_tree.py` was NOT run and not updated; the tree route now returns one more column per
  row, so a fake cursor that returns 24-tuples will fail the unpack there. Fix or replace it when tests are asked for.
- The help was done afterwards, as `0000-37` (`SLICE-0000-37-ajutor-lanturi-neinchise.md`); no picture was taken.
- Deploy: `tree.py` only (no DDL). A client newer than the server shows no red (the field is missing → empty).
- The red on the main tree is a text colour; whether it stays readable on a SELECTED row was not looked at.
