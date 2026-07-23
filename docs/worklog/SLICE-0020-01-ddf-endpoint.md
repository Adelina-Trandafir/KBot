# SLICE-0020-01 — server + client: `GET /api/forexe/ddf`

Pass 1 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §3–§4. Ships the endpoint, its
host-only tests, the `KBot.Domain` POCOs and `GetDdfAsync` on the API client. **No UI yet** —
`MainForm.CreateView("ddf")` still returns a `PlaceholderView`; that is pass 02.

> **Worklog naming.** The plan names a single `SLICE-0020-ddf-view.md`. `CODE_WORKFLOW.md` §3.2
> requires multi-pass slices to use sub-numbered files (`SLICE-0007-01-…`), which is also what
> 0015 and 0017 did. Followed the standing rule, not the plan's filename.

## What changed and why

One endpoint, three arrays, one round trip: `{cod, antet[], revizii[], linii[]}`. The client
filters locally (tree, grid, clsf combo) so a tree click never issues a request — decision 7.

- **`antet` is an array, not an object.** `FX_DDF`'s MariaDB PK is composite `(IDDF, CUAL)` and
  there is no unique constraint on `CodAngajament` (verified in `000_DEMO.sql:161`). Every
  Access query assumes one header; nothing enforces it. The server logs a warning when the
  count is > 1, and `DdfInfo.AntetDeLucru(iddfPreferat)` makes the client's pick **explicit**
  (match on `AngajamentTreeInfo.IDDF`, else first) rather than silent.
- **`TotalRevizie` is a real `SUM(ValCur)`**, as a correlated scalar subquery. Access aliases
  `SA.ValCur AS TotalRevizie` — one section-A line wearing a total's name — and `Show_Revizii`
  then re-adds the same `"RC_" & IDREV` leaf once per line, displaying an arbitrary one. Same
  defect family as the `aggRev` fan-out in 0011-03. `COALESCE(…, 0)` keeps a revision with zero
  section-A lines visible with a 0 total instead of dropping it (Access's `INNER JOIN` drops it).
- **`Clsf` needs none of the 0011-03 `IdClsfAcc` + `IdUnitate` treatment.** Confirmed from
  `routes/ddf/sync_acc_mdb.py:7-8` (`Access.IdClsfPY → MariaDB.IdClsf`) and the DDL constraint
  `FX_DDF_REV_SA_ibfk_4 FOREIGN KEY (IdClsf) REFERENCES Clasificatii (IDClsf)`: on this table
  `IdClsf` already holds the nomenclator **primary key**, the reverse of `FX_Indicatori`. Keying
  on a PK is unique by definition → no fan-out, so **no `IdUnitate` predicate**. The denormalised
  `FX_DDF_REV_SA.Clsf` is preferred, falling back to the nomenclator only when blank.
- **`ParametriiFund` is on `linii` from the start** (plan §8 bis asked for this explicitly): the
  grid never shows it (decision 4), but the pass-05 XML builder writes it into section A's
  `Cell4`. Adding it now avoids reopening the endpoint later.

### Deviation from the plan: no join to the header

§3 specifies `revizii` as "`FX_DDF_REV` joined to its `FX_DDF` header". **I did not do that.**
Given the composite PK above, an `INNER JOIN … ON IDDF` fans every revision out by the number of
`CUAL` rows that `IDDF` carries. Both `revizii` and `linii` filter through `IN (SELECT …)`
instead, which makes "one row per `IDREV`" / "one row per `IdSecA`" a property of the query
*shape* — not of a `GROUP BY` someone can later edit. That is the stated lesson of 0011-03 and
§3 applies it to `TotalRevizie` but not to the join; this extends it consistently.

`test_second_cual_row_does_not_fan_out_revisions` is the regression pin: it inserts a genuine
second `CUAL` row and asserts `antet` grows to 2 while `revizii` stays at 3 and `linii` at 4.

### Access defects reproduced *deliberately differently*

| Access | Here |
|---|---|
| `SA.ValCur AS TotalRevizie` (one arbitrary line) | real `SUM(ValCur)` per revision |
| month root value is the literal `0` | client will show the real sum (pass 02) |
| revision with no section-A line vanishes (`INNER JOIN`) | survives with total `0.0` |

## Files touched

**Server**
- `PYTHON/routes/forexe/ddf.py` — **new**. Three parameterised queries, `@require_session`,
  `ensure_ascii=False`, base from `g.session.db_name` (never a query parameter).
- `PYTHON/routes/forexe/__init__.py` — registers `ddf` on `forexe_bp`.
- `PYTHON/tests/test_forexe_ddf.py` — **new**, 17 host-only tests (skip cleanly off-host).

**Client**
- `src/KBot.Domain/DdfInfo.vb` — **new**: `DdfAntet`, `RevizieRow`, `LinieSaRow`, `DdfInfo`.
  Carries three derived members read out of the Access source, not guessed:
  `DdfAntet.FolderPdf` / `NormalizeazaNume` (the partener vs `GENERAL` convention, `\W+` → `_`),
  `RevizieRow.EtichetaRevizie` (§2.6: `Format(NumarRev,"@@@")` is a **text** format — right-align
  in 3 chars padded with **spaces**, so `PadLeft(3)`, never `D3`/`000`), and `AntetDeLucru`.
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — wire DTOs `GetDdfResponse` / `GetDdfAntetRow` /
  `GetDdfRevizieRow` / `GetDdfLinieRow` (snake_case stops at the wire).
- `src/KBot.Api/IApiClient.vb`, `src/KBot.Api/ApiClient.vb` — `GetDdfAsync`.
- `tests/KBot.App.Tests/{Plati,Receptii,Rezervari,Sumar}ViewTests.vb` — the four `FakeApiClient`
  stubs gain `GetDdfAsync` (interface addition; they throw `NotSupportedException` as before).
- `tests/KBot.Api.Tests/ApiClientTests.vb` — 8 tests.
- `tests/KBot.Domain.Tests/DdfInfoTests.vb` — **new**, 13 tests on the derived members.

## Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors**, 16 warnings, all pre-existing `NU1701`
  (iTextSharp / BouncyCastle are .NET Framework packages on `net8.0-windows`, documented in 0019).
  `dotnet test KBot.sln`: **334 passed, 0 failed** across 8 projects
  (Api 61, App 50, Controls 134, Theming 27, Xfa 39, Domain 17, Common 7, LocalStore 1).
- **Python (offline, dev station)** — `pytest tests/`: **75 passed, 14 skipped, 0 failed**.
  `tests/test_forexe_ddf.py` is among the skips: it is host-only by design (`main` needs
  `config.py`), and skipping is the correct offline outcome, **not** evidence it works.

## Anything left unverified or deferred

1. **The endpoint has never run against a real database.** Every DDF test is host-only and was
   only observed *skipping*. The SQL is written against the verified DDL and the Access export,
   but no row has passed through it. This is the same standing caveat as slices 0008/0011/0014/
   0015/0017 and it is not discharged by this pass.
2. **§3 item 5 — the duplicate-header count is NOT yet known.**
   `test_report_duplicate_cod_angajament_count` is written as a *fact-finding* test (it never
   fails; it prints the count of `CodAngajament` with more than one `FX_DDF` row). It has not
   run, so **whether §2.7 is theoretical or real remains open.** Run it on the host and record
   the number here. Until then `antet` stays an array and the client logs — which is the safe
   behaviour either way.
3. **`data_creare` / `data_def` are truncated to a date.** They are `datetime` in MariaDB; the
   view groups and displays by day. If a later pass needs the time component, `_iso` must change.
4. **`Program` ambiguity is untouched** (§2.9): `FX_DDF.Program` is carried on `antet`, but which
   of it and the session's program code the Access XML builder actually wins with is a pass-05
   question. Nothing here depends on the answer.
5. **`Tip` and `Semnatura`** are carried and displayed nowhere, per §8 item 3. `Semnatura` becomes
   live only if pass 06 goes ahead.
