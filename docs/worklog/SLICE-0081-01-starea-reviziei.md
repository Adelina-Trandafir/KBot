# SLICE 0081-01 — the revision state (G0a)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-01. **Handoff:** `docs/HANDOFF_0081_DDF_Trimitere.md`.

## What changed and why

The plan said the state S0–S4 (and S1x) would be derived from `FX_DDF_REV.Incarcat` + `Semnatura`,
no new column. Reading the code showed that does not hold, and the operator chose a new column
(question asked in this thread, 25.09.2026):

1. The import sets `FX_DDF_REV.Incarcat = 1` by itself — `prelucrare_pasi.py`, step 3e, case 1:
   any reservation carrying an `IDREV` (from the `(REV:n)` tag the send writes, D9) marks its
   revision `Incarcat = 1`. So `Incarcat` cannot hold the send stage: the import, which is off limits,
   overwrites it right after the send.
2. «A signed on the final PDF» and «A signed on the interim PDF» both read `Semnatura = 'A'`, so S2a
   and S2b could not be told apart; the Rezervări menu would have offered «Generează PDF final» again
   and wiped the signature.
3. S1x had no trace in either column.

**New column `FX_DDF_REV.StareTrimitere` (tinyint, default 0)** — `sql/0081_ddf_rev_stare_trimitere.sql`:
0 = not sent by K-BOT (and every older revision), 1 = interrupted, 2 = sent / in progress (S2a),
3 = final PDF generated. `Incarcat` keeps its old meaning and is not written by this slice.

**The derivation** (`KBot.Domain/DdfRevisionState.vb`, pure, one place):

| Stage | Extra | State |
|------:|-------|-------|
| 0 | forexecab already has it (`Incarcat` or `Preluat` or a linked `FX_Rezervari` row) | by signature: `Ordonator` → S4, `A`+`B` → S3, else S2b |
| 0 | otherwise | `A` → S1, else S0 |
| 1 | — | S1x |
| 2 | — | S2a |
| 3 | — | by signature, as the first row |

Plus `IsOpen` (S0, S1, S1x, S2a — «one open revision at a time»), `CanEdit` (S0, S1), `CanSend`
(S1, S1x), `IsSent` (S2a and after), `Label` (the Romanian text), `HasRole`.

**The check the plan left for this step — settled:** an empty `X-Semnatura` is REFUSED (400, «Antetul
X-Semnatura este gol»), not treated as a reset. Added: `X-Semnatura: -` (the same «nothing» marker as
`X-Sha-Precedent`) writes `''`. The final PDF (0081-04) is uploaded with it. An empty header stays
refused — empty is a client mistake, `-` is a statement.

**Read route** (`routes/forexe/ddf.py`, read-only additions): `stare_trimitere` (the literal 0 when
the column is not there yet — probed once per database, `routes/forexe/ddf_stare.py`) and
`are_rezervari` (scalar `EXISTS` on `FX_Rezervari.IDREV`, no fan-out).

**DDF view:** each revision leaf gets a RIGHT icon for its state (the left arrow keeps meaning the sign
of the total) and a second tooltip line «Stare: …». Icons: `image_list` key `stare_<state>` first
(e.g. `stare_signeda`), else a GDI shape per state from `DdfIcons.StateIcon`, coloured from the
palette — a different SHAPE per state, so it reads without colour.

## Files touched

- `sql/0081_ddf_rev_stare_trimitere.sql` (new)
- `src/KBot.Domain/DdfRevisionState.vb` (new) — `DdfSendStage`, `DdfRevisionState`, `DdfRevisionStates`
- `src/KBot.Domain/DdfInfo.vb` — `RevizieRow.StareTrimitere`, `.AreRezervari`, `.DejaInForexe`, `.Stare`
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `GetDdfRevizieRow.stare_trimitere`, `.are_rezervari`
- `src/KBot.Api/ApiClient.vb` — mapping; `ApiClient.SemnaturaNiciuna = "-"`
- `src/KBot.App/DdfIcons.vb` — `StateIcon`
- `src/KBot.App/Views/DdfView.vb` — right icon + tooltip per revision, `StateIconFor`
- `PYTHON/routes/forexe/ddf_stare.py` (new) — stage constants + column probe
- `PYTHON/routes/forexe/ddf.py` — the two read-only fields
- `PYTHON/routes/forexe/pdf.py` — `X-Semnatura: -`
- `tests/KBot.Domain.Tests/DdfRevisionStateTests.vb` (new)
- `PYTHON/tests/test_pdf_semnaturi.py` — `TestParseRolesNoSignature`
- `PYTHON/tests/test_forexe_ddf.py` — `REVIZIE_KEYS` (+ the three `pdf_*` keys of slice 0041, which
  the set-equality assertion was already missing)

## Test results

`dotnet build src\KBot.App` — **0 errors, 0 warnings**. `py_compile` green on the three Python files.
Tests written, **not run** (house rule).

## Left unverified / deferred

- `sql/0081_ddf_rev_stare_trimitere.sql` not run anywhere. Until it is, every revision reads stage 0
  and the send routes of 0081-04 refuse with a message naming the script.
- The tree was not drawn on screen (no `DrawToBitmap` pass) — the icons are unseen.
- Assumption: a stage-0 revision with `Incarcat`, `Preluat` or a linked reservation is «already in
  forexecab» and is read as a final document (its PDF, generated the old way, has Section B).
