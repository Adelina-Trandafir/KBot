# SLICE-0108-01 - the credit bugetar belongs to the classification; budget on a day without quarters

Operator request, 03.10.2026. Branch `SLICE-0108-credit-pe-clasificatie`, commits `88d456f` (DDL),
`2f9f837` (server), `d7be2a3` (robot + client), `aa3209a` (Migrator), `d0b6f91` (check form).

## What changed and why

FOREXE's credit bugetar is PER CLASSIFICATION: every angajament that uses a classification has the same
figure. The old `FX_Indicatori.Credit_Bugetar` kept one copy per angajament, and «Verifica bugetul» picked
the "latest" with `ORDER BY DTQ DESC` - on 000_DEMO two angajamente had the same `DTQ` to the second
(it is the insert time, never updated), so the winner was arbitrary (it showed 48.030 against the real
128.030). The fix is structural:

1. **DDL (`sql/0108_01..03`).**
   - New table `FX_Indicatori_Buget`: ONE row per `IdClsf` (`CreditBugetar`, `CodAngajament` = the
     download that wrote it last, `DTI`, `DTQ`), data only from FOREXE.
   - `DTI` = created, `DTQ` = last change (`ON UPDATE current_timestamp()`) on the seven tables that had a
     `DTQ`: `Clasificatii_Venituri_Rectificari`, `FX_Angajamente`, `FX_Indicatori`, `FX_Istoric`, `FX_Plati`,
     `FX_Receptii`, `FX_Rezervari`. The one-time query copies the old `DTQ` into `DTI` with `DTQ = DTQ`
     written on purpose (a column the UPDATE names is not touched by the automatic update).
   - `FX_Angajamente.CreditInitialLa` (NULL = the initial credit was never read).
   - Last script, after everything is installed: drops `FX_Indicatori.Credit_Bugetar` and `FX_Indicatori.DTL`.
2. **Server.**
   - `budget_on_day.py`: the budget of a day is the TOTAL of the version in force (Trim1..4) + the total of
     its corrections; no quarter cut-off (operator: a version of 01.04 with 100/200/300/400 gives 1000 on
     every later day). Used by the DDF and now also by `R_CreditBug` of new reservations
     (`prelucrare_pasi.py`).
   - `prelucrare.py` writes `FX_Indicatori_Buget` (upsert per classification) on every download and no
     longer writes `Credit_Bugetar` / `Credit_Bugetar_Initial` of `FX_Indicatori`.
   - Sumar «Credit Bug.», «Verifica bugetul» and the DDF «Buget» fallback read `FX_Indicatori_Buget`
     (the check no longer has a quarter in its answer).
   - `ddf_edit.py` no longer writes `Credit_Bugetar_Initial` (a re-save of a revision must not overwrite it).
3. **Initial credit, read ONCE per angajament.** «Prelucrare Completa» and its «Reverse» twin got a
   section 5: when `{{CITESTE_CREDIT_INITIAL}}` = true AND the page shows «În derulare», it clicks
   «Afișează informații complete», scrapes the table as `InfoCompleteContract` and goes back with «Înapoi».
   The server step `_step2c_credit_initial` takes the cell «Prevedere bugetara / Credit bugetar / An curent»
   per `Cod ang-rând` (= `CodAI`), writes `FX_Indicatori.Credit_Bugetar_Initial` and sets `CreditInitialLa`.
   "Once" is enforced in THREE places: the client sends `true` only when `GET /api/forexe/istoric` says
   `credit_initial_citit = false`; the server refuses when `CreditInitialLa` is already set (checked before
   and in the UPDATE `... AND CreditInitialLa IS NULL`) or the state is not «În derulare»; and the date is set
   in the SAME transaction as the figures (a failed or unexpected page writes nothing, so the next download
   tries again, with a warning, never an error).
4. **Migrator.** The «Buget 1/12» run (button name kept, operators call it so) writes the WHOLE previous-year
   total in `Trim1` (Trim2..4 = 0), no division by 12, and an existing 01.01 version is REPLACED
   (`ON DUPLICATE KEY UPDATE`). `FX_Indicatori` is copied without Access `Prevedere_Bugetara_Initiala`.
5. **Client.** «Verifica bugetul»: no quarter in the caption, new tooltip. Help: slice 0000-45.

## Order of installation (important)

1. `sql/0108_01_sursa.sql` on `AVACONT_SURSA`; 2. AvacontPush «Sincronizare schemă» (SAFE) on every unit;
3. AvacontPush «Interogări unice»: `sql/0108_02_interogare_unica.sql`, name `0108_dti_si_credit_pe_clasificatie`;
4. push the Python files, restart; 5. the K-BOT client and the Migrator; 6. LAST `sql/0108_03_...` on the
template and on every unit (a SAFE sync never drops a column). Until step 6 the old column stays and is harmless.
If the Python files reach a unit before step 2/3, the refresh fails on the missing `FX_Indicatori_Buget`
(deliberately loud); only the initial-credit read tolerates a missing `CreditInitialLa` (warning).

## Files touched

`sql/0108_01_sursa.sql`, `sql/0108_02_interogare_unica.sql`, `sql/0108_03_sursa_scoate_credit_bugetar.sql`;
`PYTHON/routes/forexe/{budget_on_day,prelucrare,prelucrare_pasi,ddf_edit,sumar,clasificatii_edit,istoric}.py`;
`src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl` and `... Reverse.wfl`, `WorkflowCatalog.vb`, `JobBuilder.vb`;
`src/KBot.App/Forexe/ForexeController.vb` + `.Parallel.vb`; `src/KBot.Domain/IstoricInfo.vb`, `Nomenclatoare.vb`;
`src/KBot.Api/ApiClient.vb`, `ApiClient.Nomenclatoare.vb`, `UpsertAngajamenteRequest.vb`;
`src/KBot.App/Views/Nomenclatoare/BudgetCheckForm*.vb`; `src/KBot.Migrator/Transfer/{BudgetOpeningRunner,TableMaps}.vb`,
`MigratorForm*.vb`; help (see 0000-45).

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj` and `src\KBot.Migrator\KBot.Migrator.vbproj`: 0 warnings, 0 errors.
`py_compile` on the changed Python files: clean. The two `.wfl` files parse as XML. Nothing else run (operator rule).

## Left unverified / deferred

- **Nothing ran against FOREXE or MariaDB.** The robot section is written against the page the operator pasted
  (button text «Afișează informații complete», heading «Informații complete contract», the 3-row header). The
  scraper joins header rows with «!» and keeps letters/digits/«_», so the credit cell key is assumed to be
  `Prevedere_bugetara!Credit_bugetar!An_curent` and the angajament-row key `Cod_ang_rand`; if the real keys differ
  the server answers with a warning and writes nothing (the date stays empty, the next download retries).
  Also unverified: that the button exists on the «Modificare» page the flow ends on.
- `Credit_Bugetar_Initial` for angajamente that are already «În derulare» will be read at their NEXT download
  (their `CreditInitialLa` is NULL) and is then "credit at first reading", not truly initial, as discussed.
- The one-time seed of `FX_Indicatori_Buget` picks, per classification, the angajament with the latest
  «Dată început derulare» (`DataDefinitivare`), then `DataCreare`, then the higher credit; the next download
  of any angajament on the classification overwrites it with what FOREXE reports.
- A hand-typed 01.01 budget version is overwritten by the Migrator run (the run counts how many and asks first).
- Tests (`tests\`) were not touched and may still assert the old `Credit_Bugetar` columns (`test_forexe_prelucrare_route.py`
  and fixtures) - not run, not fixed.
- The old `Credit_Bugetar` of `FX_Indicatori` is still on the units until `0108_03` runs.
- **REMINDER (operator, 03.10.2026): the `stg_` staging tables have no purpose any more and must be removed
  entirely - separate slice, used in `PYTHON/routes/ddf/{core,staging}.py`, `forexe/ddf_edit.py` and every file of
  `PYTHON/routes/ord/`.**
