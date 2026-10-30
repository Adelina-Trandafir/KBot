# SLICE-0102/108 — Istoric buget: versiuni datate și credit pe clasificație

02–03.10.2026: 0102 introduce versiunile cu DataInceput și calculul cumulat până la trimestrul zilei; 0108-01 înlocuiește explicit acea regulă cu totalul versiunii plus totalul rectificărilor, fără trimestre, și mută creditul FOREXE pe clasificație. Etapele nu sunt două formule care se aplică simultan. Fallback-ul către coloana veche și bugetul de deschidere 1/12 sunt checkpointuri inițiale, urmate de 0108. Fiecare ordine de instalare, fișier, commit, presupunere și probă este păstrată la etapa sa. Exemplele de schemă și seed sunt istoric, neexecutat. Ferestrele și regulile nu sunt validate din nou prin consolidare.

[0102-buget](#slice-0102) · [0108-01](#slice-0108-01)

<a id="slice-0102"></a>

## SLICE-0102 — The budget of a classification with dates: the DDF uses the budget it had on the revision's day (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

### What changed and why

**The problem (operator).** Revision 0 for AAB2ES88K5F / AA2 showed negative «Valoare ramasă» after a budget reduction. DDF used today's `FX_Rezervari.R_CreditBug` / `FX_Indicatori.Credit_Bugetar`: when reservations are written, `R_CreditBug` copies the current credit, even for January history downloaded after the rectification. No earlier budget was stored.

**The rule (operator answers, 02.10.2026).**
1. `Clasificatii_Buget` gets `DataInceput` (date, required): one row = one VERSION of the budget of a classification for a year, starting on that day. The existing rows get **01.04.2026** (a row of another year, if any, 01.01 of its year).
2. Rectifications (`Clasificatii_Rectificari`: `Data`, `Document`, `Trim1..4`) are NOT budget but change it, in the quarter whose `TrimN` they carry. A rectification counts from the version's `DataInceput` up to the revision day (older than the version = already inside it).
3. «Buget» on day D = version with the greatest `DataInceput <= D` (same `IdClsf`, `An = YEAR(D)`) + its rectifications, **cumulative to the quarter of D** (Trim1..Trimq). **No yearly total** anywhere (operator: a total there is not correct).
4. No version on D → the DDF keeps today's `Credit_Bugetar` and says which classifications fell back (notice in the editor).
5. D = the angajament's creation date for revision 0, the reservation day for the others, the revision's own date for a document opened for editing.

Pieces:
- **DDL**, split in two by slice 0103 (the first draft was one all-bases loop, never run, now deleted): `sql/0102_01_sursa.sql` runs on `AVACONT_SURSA` only (the column, the new unique key, and the ledger table of 0103); schema sync then carries the structure to the units. `sql/0102_02_interogare_unica.sql` is the DATA part (start date for the existing rows, drop the old unique key), pasted into AvacontPush's «Interogări unice» tab and run on every database (`SLICE-0103-interogari-unice.md`). `sql/AVACONT_SURSA.sql` (the template dump) updated the same way.
- **`PYTHON/routes/forexe/budget_on_day.py`** (new): `budget_on_day`, `BudgetByDay` (memory per (classification, day) + the fallback notice text).
- **`ddf_edit.py`**: `POST /api/forexe/ddf/genereaza` replaces `Buget` of every generated line (both sources: reservations and indicators); `GET /api/forexe/ddf/draft/...` fills `buget` (was always 0.0) from the revision's `DataRev`.
- **`clasificatii_edit.py`** (the «Clasificații bugetare» window): the budget is now a LIST of versions. GET returns `budgets: [{id, data_inceput, trim1..4}]` (replaces `budget`); POST takes `budgets` + `deleted_budgets` (removals first, then updates/inserts; start date required, inside the year, one per day). 409 text for a repeated start date.
- **Legacy Access routes** (`routes/clasificatii.py` insert + upsert, `routes/nomenclatoare.py`): the plain inserts write `DataInceput = MAKEDATE(An, 1)`. **The upsert no longer overwrites a budget**: a classification that already has any version for the year is skipped (response `buget.skipped` replaces `buget.updated`), because Access's current figures written over a version would erase the budget the DDF needs as it was.
- **Window** (`ClasificatiiForm` + Designer): `gridBuget` is a multi-row grid — «Început», Trim. 1–4, «✕», hidden id; footer «+» adds a version; no «Total» column. `KBot.Domain.Nomenclatoare`: new `BudgetVersion`, `BugetClasificatie.Budgets` (replaces `Budget`); `INomenclatoareApi.SaveBugetClasificatieAsync` takes the versions and the deleted version ids.
- **Migrator** (`TableMaps.vb`): `DataInceput` constant 01.04.2026 for `Clasificatii_Buget` (required on the target now).
- **Help**: 0000-40 (see `SLICE-0000-40-ajutor-buget-pe-versiuni.md`).

### Files touched

`sql/0102_01_sursa.sql` + `sql/0102_02_interogare_unica.sql` (new), `sql/AVACONT_SURSA.sql`, `PYTHON/routes/forexe/budget_on_day.py` (new), `PYTHON/routes/forexe/ddf_edit.py`, `PYTHON/routes/forexe/clasificatii_edit.py`, `PYTHON/routes/clasificatii.py`, `PYTHON/routes/nomenclatoare.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/INomenclatoareApi.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` + `.Designer.vb`, `src/KBot.Migrator/Transfer/TableMaps.vb`; help files (see the 0000-40 worklog).

### Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App/KBot.App.vbproj` and the Migrator project → 0 warnings, 0 errors. The Python files compile (`py_compile`); nothing was run against a database. The app was not run.

### Left unverified / deferred

- **Deploy order:** `0102_01_sursa.sql` on `AVACONT_SURSA`; push the server files (`budget_on_day.py`, `ddf_edit.py`, `clasificatii_edit.py`, `clasificatii.py`, `nomenclatoare.py`, `routes/one_time/`, `provizionare.py`) and restart; schema sync SAFE on the units; the one-time query `0102_buget_data_inceput`; then the client. Until the DDL runs, DDF generation and the draft read fail (the query names `DataInceput`), and so does the window's save.
- **The existing rows start on 01.04.2026**, so a revision dated before that (the AA2 case, 12.01.2026) finds no version and keeps today's credit (with the notice). To get January–March right the operator adds, per classification, a version starting 01.01.2026 in «Clasificații bugetare» — the operator's stated next step. Nothing is added automatically.
- **The classification combo of the line dialog** (a line added by hand with «Adaugă rând») still shows today's `Credit_Bugetar` as «Buget». Doing it needs the revision date on `IApiClient.GetDdfClasificatiiAsync`, whose signature is implemented by ten test fakes (`tests/KBot.App.Tests/*`); left for a decision. The help says so.
- **Draft read**: a classification with no version on `DataRev` keeps the old 0.0 «Buget» (no fallback, no notice).
- Nothing seen on screen: the multi-row budget grid, «+» / «✕», the date cell, the save with a repeated or out-of-year date.
- The DB column `TOTAL` (generated) stays in the table; the window no longer shows or uses it.
- `PYTHON/tests/test_schema_diff_columns.py` describes a `Clasificatii_Buget` shape; not touched and not run.
- Cumulative-to-the-quarter is the operator's choice (answer «cumulat»); a rectification dated before its version's `DataInceput` is ignored (operator: «da»).

---

<a id="slice-0108-01"></a>

## SLICE-0108-01 - the credit bugetar belongs to the classification; budget on a day without quarters

Operator request, 03.10.2026. Branch `SLICE-0108-credit-pe-clasificatie`, commits `88d456f` (DDL), `2f9f837` (server), `d7be2a3` (robot + client), `aa3209a` (Migrator), `d0b6f91` (check form).

### What changed and why

FOREXE credit is per classification, shared by its angajamente. Old FX_Indicatori.Credit_Bugetar duplicated it; «Verifica bugetul» chose ORDER BY DTQ DESC. On 000_DEMO two angajamente shared the second-resolution insert DTQ (never updated), producing an arbitrary 48.030 instead of 128.030. Structural fix:

1. **DDL (`sql/0108_01..03`).**
   - New table `FX_Indicatori_Buget`: ONE row per `IdClsf` (`CreditBugetar`, `IdUnitate`, `DTI`, `DTQ`), data only from FOREXE; no angajament column (it is per classification).
   - `DTI` = created, `DTQ` = last change (`ON UPDATE current_timestamp()`) on the seven tables that had a `DTQ`: `Clasificatii_Venituri_Rectificari`, `FX_Angajamente`, `FX_Indicatori`, `FX_Istoric`, `FX_Plati`, `FX_Receptii`, `FX_Rezervari`. The one-time query copies the old `DTQ` into `DTI` with `DTQ = DTQ` written on purpose (a column the UPDATE names is not touched by the automatic update).
   - `FX_Angajamente.CreditInitialLa` (NULL = the initial credit was never read).
   - Last script, after everything is installed: drops `FX_Indicatori.Credit_Bugetar` and `FX_Indicatori.DTL`.
2. **Server.**
   - `budget_on_day.py`: the budget of a day is the TOTAL of the version in force (Trim1..4) + the total of its corrections; no quarter cut-off (operator: a version of 01.04 with 100/200/300/400 gives 1000 on every later day). Used by the DDF and now also by `R_CreditBug` of new reservations (`prelucrare_pasi.py`).
   - `prelucrare.py` writes `FX_Indicatori_Buget` (upsert per classification) on every download and no longer writes `Credit_Bugetar` / `Credit_Bugetar_Initial` of `FX_Indicatori`.
   - Sumar «Credit Bug.», «Verifica bugetul» and the DDF «Buget» fallback read `FX_Indicatori_Buget` (the check no longer has a quarter in its answer).
   - `ddf_edit.py` no longer writes `Credit_Bugetar_Initial` (a re-save of a revision must not overwrite it).
3. **Initial credit, once per angajament.** Prelucrare Completa/Reverse section 5: with CITESTE_CREDIT_INITIAL=true AND «În derulare», click «Afișează informații complete», scrape InfoCompleteContract, return via «Înapoi». _step2c_credit_initial maps «Prevedere bugetara / Credit bugetar / An curent» per Cod ang-rând (=CodAI) to FX_Indicatori.Credit_Bugetar_Initial and sets CreditInitialLa. Three guards: client true only for GET /api/forexe/istoric credit_initial_citit=false; server requires state «În derulare» and unset date, checked before and inside UPDATE … AND CreditInitialLa IS NULL; date and figures commit together. Failed/unexpected page writes nothing, warns (never error) and retries next download.
4. **Migrator.** The «Buget 1/12» run (button name kept, operators call it so) writes the WHOLE previous-year total in `Trim1` (Trim2..4 = 0), no division by 12, and an existing 01.01 version is REPLACED (`ON DUPLICATE KEY UPDATE`). `FX_Indicatori` is copied without Access `Prevedere_Bugetara_Initiala`.
5. **Client.** «Verifica bugetul»: no quarter in the caption, new tooltip. Help: slice 0000-45.

### Order of installation (important)

1. `sql/0108_01_sursa.sql` on `AVACONT_SURSA`; 2. AvacontPush «Sincronizare schemă» (SAFE) on every unit;
3. AvacontPush «Interogări unice»: `sql/0108_02_interogare_unica.sql`, name `0108_dti_si_credit_pe_clasificatie`;
4. push the Python files, restart; 5. the K-BOT client and the Migrator; 6. LAST `sql/0108_03_...` on the template and on every unit (a SAFE sync never drops a column). Until step 6 the old column stays and is harmless. If the Python files reach a unit before step 2/3, the refresh fails on the missing `FX_Indicatori_Buget` (deliberately loud); only the initial-credit read tolerates a missing `CreditInitialLa` (warning).

### Files touched

`sql/0108_01_sursa.sql`, `sql/0108_02_interogare_unica.sql`, `sql/0108_03_sursa_scoate_credit_bugetar.sql`; `PYTHON/routes/forexe/{budget_on_day,prelucrare,prelucrare_pasi,ddf_edit,sumar,clasificatii_edit,istoric}.py`; `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl` and `... Reverse.wfl`, `WorkflowCatalog.vb`, `JobBuilder.vb`; `src/KBot.App/Forexe/ForexeController.vb` + `.Parallel.vb`; `src/KBot.Domain/IstoricInfo.vb`, `Nomenclatoare.vb`; `src/KBot.Api/ApiClient.vb`, `ApiClient.Nomenclatoare.vb`, `UpsertAngajamenteRequest.vb`; `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm*.vb`; `src/KBot.Migrator/Transfer/{BudgetOpeningRunner,TableMaps}.vb`, `MigratorForm*.vb`; help (see 0000-45).

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj` and `src\KBot.Migrator\KBot.Migrator.vbproj`: 0 warnings, 0 errors. `py_compile` on the changed Python files: clean. The two `.wfl` files parse as XML. Nothing else run (operator rule).

### Left unverified / deferred

- **Nothing ran against FOREXE or MariaDB.** The robot section is written against the page the operator pasted (button text «Afișează informații complete», heading «Informații complete contract», the 3-row header). The scraper joins header rows with «!» and keeps letters/digits/«_», so the credit cell key is assumed to be `Prevedere_bugetara!Credit_bugetar!An_curent` and the angajament-row key `Cod_ang_rand`; if the real keys differ the server answers with a warning and writes nothing (the date stays empty, the next download retries). Also unverified: that the button exists on the «Modificare» page the flow ends on.
- `Credit_Bugetar_Initial` for angajamente that are already «În derulare» will be read at their NEXT download (their `CreditInitialLa` is NULL) and is then "credit at first reading", not truly initial, as discussed.
- The one-time seed of `FX_Indicatori_Buget` picks, per classification, the angajament with the latest «Dată început derulare» (`DataDefinitivare`), then `DataCreare`, then the higher credit; the next download of any angajament on the classification overwrites it with what FOREXE reports.
- A hand-typed 01.01 budget version is overwritten by the Migrator run (the run counts how many and asks first).
- Tests (`tests\`) were not touched and may still assert the old `Credit_Bugetar` columns (`test_forexe_prelucrare_route.py` and fixtures) - not run, not fixed.
- The old `Credit_Bugetar` of `FX_Indicatori` is still on the units until `0108_03` runs.
- **REMINDER (operator, 03.10.2026): the `stg_` staging tables have no purpose any more and must be removed entirely - separate slice, used in `PYTHON/routes/ddf/{core,staging}.py`, `forexe/ddf_edit.py` and every file of `PYTHON/routes/ord/`.**

## Repere unice din registru

- Registrul 0102 numea explicit cheia unică `(IdClsf, An, DataInceput)` și captura `clasificatii` de refăcut în ajutorul 0000-40. Acestea sunt reperele istorice, nu o modificare de schemă sau captură efectuată acum.
