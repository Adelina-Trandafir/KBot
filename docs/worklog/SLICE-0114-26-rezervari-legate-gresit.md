# SLICE-0114-26 - Reservations linked to a revision that does not cover them; no new FOREXE reservation while one waits; readable tree tooltip (23.10.2026)

## What changed and why
Operator, 23.10.2026, on angajament AAB2FTFNE46: the reservation of 05.10.2026 (+3000) had no plus sign in the Rezervari tree although no revision exists for it, and several other angajamente are in the same state. Requests: (2) stop in-app reservations while reservations without a revision exist, (3) a repair button (inside "Repara rezervari", with its own route) and a second red in the main tree; (4) the main tree tooltip must be readable (bigger font, like the Receptii tree's).

**Cause (read in the code and checked against the pasted dump; not run on MariaDB).** On 05.10 the in-app browser wrote the marker "(IDREV: 114)" into the reservation motive (`marcaj.py`: one revision number held per angajament edit session). Reservations of 30.06, 08.09 and 19.09 already existed without a revision. On 20.10 the revisions were generated oldest day first, so the 30.06 revision took number 114. Step 8.1b of the revision save (`ddf_edit.py`) then linked every reservation whose history row carried 114 and had no IDREV yet, without checking that the revision covers it. `grp_idrz` of revision 114 held only IDRZ 201, the SA lines of revision 114 sum to -31136 = reservation 201 alone, and IDRZ 400 (+3000, 05.10) was linked by 8.1b only. AreDDF = 1 on it means the tree shows no plus.

### 1. Detection and repair (server)
- NEW `PYTHON/routes/forexe/rezervari_legaturi.py`:
  - `gaseste_legaturi_gresite`: per revision, linked reservations are grouped by (day, operation type); the smallest key is the revision's own. A foreign reservation is reported ONLY when the SA sums disagree with all linked reservations AND agree once the foreign ones are left out.
  - `POST /api/forexe/rezervari/repara-legaturi {cod, aplica}`: dry run or write. Per reservation: `FX_Rezervari.IDREV = NULL, AreDDF = FALSE`; `FX_Istoric.IDREV = NULL`; the revision's copies of pictures that belong only to that reservation are removed (made again by the right revision). Sums and revisions are not touched.
- `tree.py`: tree rows get a `LegaturiGresite` column (tree rows read to the end first; a second query on the same connection would fail).
- `ddf_edit.py` step 8.1b: the marker link now applies only to reservations with the same (day, type) as the ones the revision was generated from; when the save had no generated reservation the old behaviour stays. A left-out reservation is logged.

### 2. Rule: no reservation begins while one waits for its revision
- `marcaj.py`: new tip `verifica-rezervare` (reserves nothing): 409 + reason when the angajament has reservations with AreDDF = 0 and non-zero value, except those of the edit session in progress (history row IDREV = held lock); 200 otherwise.
- `ForexeWatch.js`: the click that starts a reservation is held until K-BOT answers (`onPoartaClick` / `cerePoarta`); a refusal shows the reason in the blocking box and the click is not replayed.
- `WorkflowExecutor.Watch.vb` (`RaspundeLaPoarta`, `TrimiteRaspunsInPagina`), `Program.vb`, `ApiClient.Marcaj.vb`, `MarcajeRecente.TipVerificaRezervare`. Only the server's refusal stops the operator; a failed check lets the reservation begin and is logged.

### 3. Client: repair + second red
- Domain `RezervareLegataGresit`, `RezervariLegaturiResult`; `AngajamentTreeInfo.LegaturiGresite`; `GetTreeRow.LegaturiGresite`; `IRezervariLegaturiApi` + `ApiClient.RezervariLegaturi.vb` (outside `IApiClient`, registered in `Program.vb`).
- `KbotForm.Tree.vb`: second red = `RosuAlternativ` (theme error colour, hue -22 degrees); a row with the older errors keeps the first red.
- `KbotForm.Ddf.vb`: `ReparaRezervari` first runs `ReparaLegaturiAsync` (dry run, Yes/No, apply, tree reloaded), then the old repair if still needed. `RezervariView` shows the ribbon button for this error too; button tooltip updated.

### 4. Main tree tooltip
`KbotForm.Tree.vb` `TooltipTabel`: XML table tooltip like the Receptii tree, 10 pt (11 first, then 1 pt smaller at the operator's request; was plain text 8.5 pt). Old `TooltipFor` kept, no longer called.
The tooltip no longer disappears after a time: `tree.AutoHideTooltipMs = 0` in the `KbotForm` constructor, and `AdvancedTreeControl.Popup.vb` starts no timer when the value is 0 or less (it was 5000). It goes away only when the hovered row changes or the mouse leaves the tree (existing `HideAllTooltips` paths). This holds for every row tooltip of the main tree, not only those with errors; no other tree sets the value, so they keep 5 s. Build 0/0; not seen on screen.

## Files touched
Server: `PYTHON/routes/forexe/rezervari_legaturi.py` (new), `__init__.py`, `tree.py`, `marcaj.py`, `ddf_edit.py`.
Client: `src/KBot.Domain/RezervareLegataGresit.vb`, `RezervariLegaturiResult.vb` (new), `AngajamentTreeInfo.vb`; `src/KBot.Api/IRezervariLegaturiApi.vb`, `ApiClient.RezervariLegaturi.vb` (new), `ApiClient.vb`, `ApiClient.Marcaj.vb`, `UpsertAngajamenteRequest.vb`; `src/KBot.Common/MarcajeRecente.vb`; `src/KBot.Forexe/Executor/WorkflowExecutor.Watch.vb`, `Services/JavaScripts/ForexeWatch.js`; `src/KBot.App/Program.vb`, `KbotForm.Tree.vb`, `KbotForm.Ddf.vb`, `Views/RezervariView.vb`, `AvacontMain.Designer.vb`.
Help (0000-120): `contabil/vederi/rezervari.md`, `contabil/fereastra.md`, `contabil/forexe/browser.md`.

## Test results
- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors (before the bookkeeping; re-run at the end).
- `py_compile` of the Python files: ok.
- No tests written or run. Nothing run on MariaDB, nothing seen on screen, server not touched (operator puts the files on it by hand; `rezervari_legaturi.py` is NEW and goes together with `tree.py`, `marcaj.py`, `ddf_edit.py`, `__init__.py`).

## Unverified / deferred
- Detection rule designed on AAB2FTFNE46 only; reported only when removing a reservation makes the revision sums agree. Check a second angajament before pressing the repair button.
- State of IDRZ 400 BEFORE the 20.10 save is not in the dump; the cause rests on code and sums.
- Existing tests (`test_forexe_tree.py`, `test_forexe_marcaj*`) may compare exact row keys or the old unknown-tip message; not run.
- `ForexeWatch.js` gate not exercised in a real FOREXE page.
- Gate counts only AreDDF = 0 with non-zero value; an edit session in progress does not block.
- Existing wrongly linked data is NOT repaired by this slice; the button does it per angajament.
