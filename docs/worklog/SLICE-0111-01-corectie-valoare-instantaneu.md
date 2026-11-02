# SLICE 0111-01 — value correction of a reception snapshot

**Date:** 06.10.2026. **Asked by the operator.** Reasoning and rules: `docs/FUNDAMENT_Asociere_Receptii.md` §1.8 and Part 6 (F35).

## What changed and why

FOREXE's own history sometimes writes a wrong total on a reception header. Real case, found by the operator: reception of 12.06.2026 of `AAB3MEF2MG2` — history rows «Suma receptie: 1635 RON» (line) and «Receptie: plata salarii metodisti mai 2026, valoare: 0, (activ:true)» (header). K-BOT copied the 0 into `FX_Receptii_H.Total`: the snapshot could not be matched to its reception by value, the chain did not close, and `DIFH` / `DIF` (summed by the ordonantare) started from a false figure.

The operator may now correct the figure from the association window. Decisions, all the operator's:

- correction uses WORKING `FX_Receptii_H.Total` / `FX_Receptii.Valoare`, already used by all readers (none changed); existing `TotalOrig`/`ValoareOrig` retain FOREXE values. Difference marks «corrected». Replaces proposed `ValoareReala`, which required every reader/insert path to change, risking silent `NULL` → 0.
- `FX_Istoric` is never touched, and a correction never causes a new download;
- on save, a total that is not the sum of the lines is a BLOCKING error;
- who / when / why on the header: `CorectatDe`, `CorectatLa`, `CorectatMotiv` (reason required);
- the DDL runs on `AVACONT_SURSA` only; the other databases come through AvacontPush.

Counted by the operator on 06.10.2026 on the 18 databases: no row has an original different from its working value; `FX_Receptii.ValoareOrig` is filled everywhere; `FX_Receptii_H.TotalOrig` is `NULL` on all existing headers (K-BOT never wrote it) — the one-time query fills it.

### Server (`PYTHON/routes/forexe/`)

- `prelucrare_pasi.py`: `_H_INSERT_SQL` / `_H_INSERT_CU_ID_SQL` write `TotalOrig` at birth, equal to `Total`, as the expression `Total` in the value list (no caller's parameters changed, so the three callers and the tests' tuple shapes stay as they were). `receptii_refacere.py` imports the same SQL.
- `asociere.py`:
  - the read (`GET /api/forexe/asociere`) now also sends `total_orig`, `corectat_de`, `corectat_la`, `corectat_motiv` per snapshot and `idr`, `valoare_orig` per line;
  - new route `POST /api/forexe/asociere/corectie`: `normalize_correction` (shape), `plan_correction` (the rules, a pure function), `apply_correction` (the writes), `post_correction` (the route). Rules: reason required (≤ 500); total = sum of the lines to 2 decimals (400); a PLACED snapshot that the ordonantare rule freezes is refused (409 `INSTANTANEU_BLOCAT`), an unplaced one is not; the deletion row is refused; the old values the client saw must match the base (409 `STARE_MODIFICATA`); originals filled first, only where `NULL`; step 4d re-run in the same transaction for a placed snapshot (DIF is stored and the ordonantare sums DIF).
  - module docstring points to the fundament.

### SQL (`sql/`)

- `0111_01_sursa.sql` — the three columns on `AVACONT_SURSA.FX_Receptii_H` (run on the template only).
- `0111_02_interogare_unica.sql` — the one-time query for AvacontPush (names unqualified): `TotalOrig <- Total` and `ValoareOrig <- Valoare` where `NULL`; `DTQ = DTQ` on `FX_Receptii` so the automatic `ON UPDATE` does not move every row's date.

Order: DDL on the template › AvacontPush schema sync (SAFE) › push Python › AvacontPush one-time query `0111_total_orig_si_valoare_orig` › client.

### Client (`src/`)

- `KBot.Domain`: `InstantaneuLegat` (`TotalOrig`, `CorectatDe/La/Motiv`, `EsteCorectat()`), `LinieInstantaneu` (`Idr`, `ValoareOrig`, `EsteCorectata()`), new `CorectieValoare` / `CorectieLinie`.
- `KBot.Api`: DTOs, `IApiClient.CorecteazaValoareaAsync`, `ApiClient` (the 400 / 409 arrive as `ApiException` with `Reason`).
- `KBot.App\Forexe\CorectieValoareForm` (+ `.Designer.vb`): the grid (total row, then one row per line; «Din FOREXE» read-only, «Valoare corectă» editable), the live sum under it (error colour when the total is not the sum), the reason box, «Revino la valorile din FOREXE», «Salvează», «Renunță». The rules are mirrored as `Friend Shared` functions (`ProblemWith`, `SumMismatch`); the server decides. Outcome by `DialogResult`: `OK` saved, `Retry` nothing written because the picture was old, `Cancel`.
- `AsociereForm`: context-menu item «Corectează valoarea…» (anytime editor only; not on a frozen snapshot — it returns earlier with the reason —, not on the deletion row); `[corectat]` in the row caption; the tooltip says what FOREXE gave and who / when / why. The correction is saved at once and the window reloads; unsaved moves are asked about first.
- `tests/KBot.App.Tests`: the 9 fakes of `IApiClient` got the new member (stub), so the slice adds no compile error there.

### Second pass (06.10.2026, same day): the correction in the DOWNLOAD window

Initially correction was anytime-editor only. After-download 0,00 versus reception 1.635,00 blocked placement/save; phase-one rollback leaves no persisted snapshot for that route to correct.

- Client: `AsociereForm` offers «Corectează valoarea…» only on downloaded snapshots, not older `Context` ones. Reused `CorectieValoareForm` saves locally via `PastreazaCorectia`: replaces lines with COPIES (proposal shares originals), retains FOREXE's `TotalOrig`/`ValoareOrig`, remembers `_corectii`. `DeciziiDin` attaches `DecizieAsociere.Corectie` / `PostPrelucrareDecizie.corectie` (total, reason, lines by INDICATOR). Restoring FOREXE figures sends no correction; no window reload.
- Server: `normalizeaza_decizii` carries `corectie` (`normalizeaza_corectie`: shape only). Phase-two `prelucrare.py` FIRST writes through `asociere.apply_download_corrections`: same `plan_correction` rules (reason, total = sum of lines, no deletion row), without frozen-link/stale-value checks (unplaced snapshot, fingerprint already verified). RE-READS snapshots before placement checks F14/F15/F16 and decisions; step 4d stays last.
- Not done on purpose: correcting an OLDER snapshot (`instantanee_asezate`) from the download window — that needs a decision of its own to carry it; it is done in the anytime editor.
- Giving up the download gives up the correction.
- Never exercised: the phase-two path. The first real download with a correction is its test.

### Help

Recorded as `0000-53` (`SLICE-0000-53-ajutor-corectare-valoare.md`).

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: 0 warnings, 0 errors.
- `PYTHON/.venv` `py_compile` of `asociere.py` and `prelucrare_pasi.py`: clean.
- `Check-Help.ps1 -Coverage`: the `0111` messages were the only ones about this slice and go away with the registry row; the remaining errors are the old `tutorials\ordonantare-din-plata.md` ones.
- NO test was written or run (operator's rule).
- `dotnet build tests/KBot.App.Tests`: does NOT build, for reasons that are not this slice (`ForexeAnswerStoreTests.vb(43)`: `JobRequest` cannot be indexed; `MainFormNavItemsTests` / `MainFormPoartaDdfTests`: missing `capturiApi` argument). No error mentions `IApiClient`.

## Left unverified or deferred

- **Nothing ran against a live MariaDB**: not the DDL, not the one-time query, not the route. That a value list may read a column set earlier in it (`TotalOrig` ← `Total`) is MariaDB's documented behaviour (the same as MySQL's), not something run here. If it were wrong every download would fail on its header insert — the first download after the deploy is the test.
- **The form was never seen on screen.** The designer file was written by hand (96 dpi, `AutoScaleMode.Dpi`, a `KBotTableLayoutPanel`, a `KBotDataView` with one editable number column), on the pattern of `DdfEditLinieAForm` / `SelectieReceptiiForm`. Sizes, the intro label's height and the focus into the first cell on opening are guesses.
- **ASSUMPTION to confirm:** the ordonantare freeze applies to a value correction too (same rule as for the link). Lifting it = the `blocks` check in `plan_correction`.
- The `HASH` of the lines is not recomputed after a correction: verified that nothing reads it back (no `SELECT` of it in `PYTHON/routes` or `src/**/*.vb`, no unique index on `FX_Receptii` / `FX_Receptii_H`). Scripts outside those folders were not searched.
- A header deleted and rebuilt from history (`receptii_refacere`) is born again with the history's figure and without the correction (it writes only what is missing).
- The tutorial `legaturile-receptiilor` was not changed (it teaches moving, not correcting).
- Not committed (the working tree holds other work; commits are the operator's).

## Files touched

New: `sql/0111_01_sursa.sql`, `sql/0111_02_interogare_unica.sql`, `src/KBot.App/Forexe/CorectieValoareForm.vb`, `src/KBot.App/Forexe/CorectieValoareForm.Designer.vb`, `docs/worklog/SLICE-0111-01-corectie-valoare-instantaneu.md`.

Modified: `PYTHON/routes/forexe/prelucrare_pasi.py`, `PYTHON/routes/forexe/asociere.py`, `PYTHON/routes/forexe/prelucrare.py`, `PYTHON/routes/forexe/prelucrare_asociere.py` (second pass), `src/KBot.Domain/AsociereInfo.vb`, `src/KBot.Domain/AsociereStare.vb`, `src/KBot.Api/UpsertAngajamenteRequest.vb`, `src/KBot.Api/IApiClient.vb`, `src/KBot.Api/ApiClient.vb`, `src/KBot.App/Forexe/AsociereForm.vb`, the 9 fakes in `tests/KBot.App.Tests` (`AsociereFakeApi`, `DdfViewTests`, `IstoricViewTests`, `LogViewerFormTests`, `PlatiViewTests`, `PrelucrareCoordinatorTests`, `ReceptiiViewTests`, `RezervariViewTests`, `SumarViewTests`), `docs/FUNDAMENT_Asociere_Receptii.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0110-0119.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, and the help files listed in `SLICE-0000-53-ajutor-corectare-valoare.md`.
