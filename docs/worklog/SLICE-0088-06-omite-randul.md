# SLICE-0088-06 - «Omite rândul»: rows made by hand in FOREXE, kept on the server (29.10.2026)

Sub-slice of [SLICE-0088](SLICE-0088-note-corectie-cab.md) (correction notes F1135).

## What changed and why
Operator: a new ERR note arrived that could not be saved, because the window asks EVERY row to be ticked and some rows are operations made by hand in FOREXE, with no documents in K-BOT. First built window-only; the operator then asked for it to be KEPT: an omitted row appears neither in the form nor in the ribbon until brought back by hand.
- **DDL** `sql/0088_06_fx_operatiuni_omis.sql` (new): `FX_Operatiuni.Omis tinyint(1) NOT NULL DEFAULT 0`. Run on every unit database (and AVACONT_SURSA) BEFORE the new `operatiuni.py` is relied on; until then the server keeps working (nothing hidden) and answers 409 to «Omite rândul» (column checked in information_schema).
- **Server** `routes/forexe/operatiuni.py`: `GET .../necorelate` leaves out `Omis = 1`; `?omise=1` returns ONLY those; new `POST /api/forexe/operatiuni/omitere` `{idfxp:[…], omis:bool}` → `{actualizate}` (ERR rows only, ≤ 500 ids). The login insert is unchanged: a row already there (same ReferintaTrezor + NrDoc) is never re-inserted, so an omitted row stays omitted.
- **Api**: `IUncorrectedOperationsApi.GetSkippedOperationsAsync` + `SetOperationsSkippedAsync`; the reading shared in `ReadUncorrelatedAsync`.
- **Window** `CabNoteForm`: `btnOmite` («Omite rândul», next to «Ieșire»): Yes/No question → server → the row leaves the grid (`PopulateGrid` rebuilds it; what was filled on the other rows is kept by IDFXP in `_stash`). Same window with `skippedMode:=True` lists ONLY the omitted rows, read-only, button «Reia rândul», no save. «Salvează tot» is the 0088-05 rule again, over the rows left in the grid.
- **Shell**: new menu / ribbon entry `operatiuni_omise` («Operațiuni omise (N)», visible only while some exist, no «(!)», no red); `KbotForm.ShowSkippedAsync`, machete type `cabnote.omise`; `RefreshUncorrelatedMarkAsync` also reads the omitted count. The «(!)» count and the entry «Operațiuni necorelate» count ONLY the rows not omitted.
- **Help:** `contabil/notecab.md` («Un rând care nu se corectează de aici», tag `0088-06`).

## Files touched
`sql/0088_06_fx_operatiuni_omis.sql` (new); `PYTHON/routes/forexe/operatiuni.py`; `src/KBot.Api/IUncorrectedOperationsApi.vb`, `ApiClient.UncorrectedOperations.vb`; `CabNoteForm` and `KbotForm` (shell entry, `ShowSkippedAsync`) in `KBot.App`; `HelpContent/contabil/notecab.md`.

## Test results
Build KBot.App 0 warnings / 0 errors; `py_compile` of `operatiuni.py` green. No test written or run. Nothing run against the server; window and entry never seen on screen (operator's rule).

## Left unverified or deferred
- ⚠ Run `sql/0088_06_fx_operatiuni_omis.sql` on every unit database + AVACONT_SURSA before relying on the new `operatiuni.py`.
- A disabled `KBotDataRow` still takes the selection click.
- Button width / bar layout at 144 dpi (`lblStare` is narrower now); the new ribbon button's room in «Verificări».
- The capture «operatiuni-necorelate» is old (no button).
