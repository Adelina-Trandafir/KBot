# SLICE-0056-02 — old links can be corrected while placing a download

Operator, 01.10.2026: «I need to be able to edit ANY reception when new data is downloaded, as
long as there is no ordonantare for it. Now, when I download new data, it does not let me change
the old receptions — the ones that already exist — and I do not want that.»

Slice number: recorded as the second pass of 0056 («Tabloul întreg al propunerii»), the slice
that brought the already written snapshots (the "context") into the proposal and froze all of
them. **Assumed, not asked** — renumber if it belongs elsewhere.

## What was wrong (read from the code and from the operator's `asociere.log`)

In proposal mode every snapshot written before the download arrived with `blocat = True`,
whatever the ordonantari said (`citeste_instantanee_context`, and again in
`AsociereStare.DinPropunere`). The reason given at the time: the server asks for a decision on
exactly the rows to place, and a decision for any other row was refused.

The pasted log (014_SCSV, AAB2HFBEEAF, run 9) shows what that costs. The download moved the
value 1723.58 onto reception 33, where snapshot 63 (398.39, placed earlier) is the newest. The
two snapshots to place (61, 62, 1723.58) belong on 33, but with 63 still on it the chain ends on
398.39, F15 refuses the save, and 63 could not be moved from this window. The download could not
be saved at all without closing the window and going through the anytime editor first.

## What changed

**The rule is now the same in both modes: a link is frozen only by an ordonantare** (the
existing rule of `asociere.citeste_blocaje`: an ordonantare line on the angajament dated on or
after the snapshot's day).

Server (`PYTHON/routes/forexe/`):

- `prelucrare_asociere.citeste_instantanee_context`: `blocat` is no longer True for all. A
  placed row is blocked only by `blocaje`; an unplaced one is free; a row born in the current
  run that is not in the set to place (no history row, or a duplicate) stays blocked with
  `MOTIV_FARA_ISTORIC`, because its IDRH does not survive the rollback of phase one.
  `MOTIV_CONTEXT` is removed.
- `citeste_idrh_scrise` (new): the IDRHs of the angajament BEFORE the steps write anything,
  read in both phases. Only these can be named by IDRH.
- A third anchor, `("idrh", n)`, next to `rand_istoric` (F24) and `idh` (F34): a decision that
  carries `idrh` is a CORRECTION of a snapshot written before this run.
  `normalizeaza_decizii` asks for exactly one of the three and allows `desprins` only on a
  correction (`ACTIUNE_DESPRINS` moved here; `asociere.py` imports it).
- `verifica_corecturile` (new): the row must be a context row, once, with a matching `data_h`,
  and not blocked — checked again at save, since an ordonantare may appear after the proposal.
  No coverage: for an old link silence means «leave it as it is».
- `aplica_decizii`: coverage is asked only for the rows to place; labels, chains and writes
  treat both kinds alike. The reception a corrected row LEAVES is touched too (chain checked,
  `Final`/`Partial` redone, `Sters` taken off when it lost its deletion row). `desprins` and
  `ignorat` on a correction also clear `EsteStergere`. New counter `asocieri.desprins`.
- `valideaza_plasarile(f15_doar_semn=...)`: on a reception that only LOSES snapshots in this
  save F15 warns instead of refusing — the anytime editor's reason: a chain whose last snapshot
  was taken off does not close by definition. A reception that gains a snapshot keeps the veto.
- `prelucrare.py`: reads `idrh_scrise` before the steps; in the save phase reads the context
  only when a correction came.
- Journal (`asociere.log`): the context table has a `blocat` column and the reasons; the save
  writes «corecturi: N», and for each one where the row was.

Client:

- `KBot.Domain`: `InstantaneuLegat.Context` (row written before the download; key `-IDRH`);
  `DinPropunere` keeps the server's `Blocat`; `DecizieAsociere.Idrh` (the third anchor).
- `KBot.Api`: `PostPrelucrareDecizie.idrh`; `CatreFir` sends exactly one anchor and allows
  `Desprins` only with it. **`IApiClient` is unchanged** (the corrections travel in the same
  `decizii` list).
- `AsociereForm`: `DeciziiDin` sends a correction for every context row the operator changed
  and nothing for an untouched one; `NehotarateDin` skips context rows (no decision owed);
  «Golește așezările» puts moved old links back to the server's state and its question says so.
  Nothing else: every drag / menu / lock check already read `Blocat`.

Compatibility: an old client against the new server still marks all context rows blocked (its
own `DinPropunere`); a new client against the old server gets them all blocked from the server.

## Files touched

- `PYTHON/routes/forexe/prelucrare_asociere.py`, `prelucrare.py`, `asociere.py`
- `src/KBot.Domain/AsociereStare.vb`, `AsociereInfo.vb`, `KBot.Domain.vbproj` (1.2.7.0 → 1.2.8.0)
- `src/KBot.Api/ApiClient.vb`, `UpsertAngajamenteRequest.vb`, `KBot.Api.vbproj` (1.0.15.0 → 1.0.16.0)
- `src/KBot.App/Forexe/AsociereForm.vb`
- help: see `SLICE-0000-26-ajutor-pentru-0056-02.md`

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj` (output to a scratch folder, K-BOT was running and
  locked `bin\Debug`): **0 warnings, 0 errors**.
- The three Python files parse (`ast.parse`); nothing was imported or run.
- **No tests were run and none were written** (operator: «nu testa»). The app was not started.

## Left unverified or deferred

- **Never run**: no proposal, no save, no window. The whole change is read from the code.
- **Tests that describe the old behaviour and will fail until updated** (not touched):
  - `PYTHON/tests/test_forexe_prelucrare_asociere.py` — the three `citeste_instantanee_context`
    calls (new required argument `idrh_scrise`; «all blocked» no longer true);
  - `PYTHON/tests/test_forexe_prelucrare_route.py` — the scripted cursor does not know the new
    `SELECT IDRH FROM FX_Receptii_H WHERE CodAngajament` read;
  - `tests/KBot.Domain.Tests/AsociereStareDinPropunereTests.vb` and the `Blocat` assertions in
    `tests/KBot.App.Tests/AsociereFormTests.vb` / `AsociereDeciziiTests.vb`, where they expect
    every context row blocked.
- **The server files must reach the VPS** together with the client; until then the new client
  behaves as before (all old links blocked).
- The fingerprint (`amprenta`) does not see a link MOVED by another session between the proposal
  and the save (it counts rows, not targets). Same limit as in the anytime editor; the blocking
  rule itself is re-checked at save.
- The two warnings of `citeste_instantanee` («... nu pot fi așezate din descărcare. Se rezolvă în
  editorul de asociere») are unchanged, although such rows written before the run can now be
  placed in this window. Wording left for the operator to decide.
- Ties on `DataH` between an old and a new snapshot on a reception started in the window are
  ordered by the form on `-IDRH`, by the server on `IDRH`. Not expected in real data.
