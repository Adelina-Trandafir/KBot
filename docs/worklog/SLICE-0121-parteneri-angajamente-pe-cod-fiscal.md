# SLICE-0121 — Furnizori și angajamente după CodFiscal

Planul și șase etape implementate documentar sunt reunite. Dependențele sunt în tabelul planului: 02…07 cer schema 01 aplicată și sincronizată, iar 05 folosește ruta 04. 07 adaugă copierea datelor vechi după constatarea că Migratorul nu acoperă ce s-a introdus ulterior în K-BOT; limitele inițiale sunt checkpointuri. Corecția din 20.10 la 03 trimite id_clsf = 0 pentru clientul 1.1.2.4. Nicio schemă, interogare sau migrare nu este executată prin consolidare.

[0121 — plan și decizii](#slice-0121-plan) · [0121-01](#slice-0121-01) · [0121-02](#slice-0121-02) · [0121-03](#slice-0121-03) · [0121-04](#slice-0121-04) · [0121-05](#slice-0121-05) · [0121-07](#slice-0121-07)

## SLICE-0121 - Supplier <-> angajament links by CodFiscal (PLAN, 19.10.2026) <a id="slice-0121-plan"></a>

**Checkpoint 19.10.2026:** DDL 01 și interogarea 07 scrise, nerulate; 02…05 construite, nepornite/netestate. Închiderea 06 rămâne la cuvântul operatorului. Numărul 0121 a fost ales ca următorul liber (operator, 19.10.2026); fiecare punct are subfelia `0121-NN`.

### Why
Operator, 19.10.2026, «Recepții» (KE16): the chosen angajament belongs to the supplier, not the invoice. `_added` (`KeLegaForm`) keeps it only in memory per supplier; durable `Parteneri`/`Parteneri_Coduri` are per unit/SS (`IdUnitate`/`CodPartener`), requiring the partner in every affected unit. Decision: leave `Parteneri` and key NEW links by normalized **CodFiscal**, DC-wide across its units.

### Decisions (all from the operator, 19.10.2026)
- `Parteneri` is NOT altered (no ALTER, indexes untouched). Partners without a CodFiscal (Access has some) are never linked to angajamente, invoices or reports; they stay in `Parteneri` and just have no rows in the new tables. They are counted and listed by the Migrator, none is lost.
- Several angajamente of one supplier on the same classification must be possible (the old key `(IdPartener, IdClsf)` forbids it; the new indicator key is the `CodAI`).
- `ContBancar` goes on the indicator row, as in `Parteneri_Coduri` today.
- Same CodFiscal with different `CodAng` on the same classification in two units: the operator believes it does not happen; the Migrator's report must show whether that is true.
- CodFiscal normalization is the one of the DDF (`primite_ubl.normalize_cui`), written normalized, read normalized.
- No path back to Access for `ParteneriAng` (none exists, none wanted for now).
- The server DDL is run by the operator by hand; nothing is deployed by Claude (see memory `never-deploy-to-server`).

### Facts read (not assumed)
- `Parteneri`: unique `(IdUnitate, CodPartener)`; FKs by `IdPartener` from `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`, `Parteneri_Coduri`, `VC_NIR` (RESTRICT) -> the table is NOT touched.
- Already by CodFiscal: `FX_DDF`, `FX_DDF_Parteneri(IDDF, CodFiscal)`, `FX_ORD_PART`, `EF_*`.
- `Parteneri_Coduri` is read/written only by the Parteneri window (`ParteneriForm`, `ApiClient.Nomenclatoare`), `parteneri_edit.py` and the Migrator (`TableMaps`, `ResolutionMaps`, `Verifier`, `TransferRunner`); DDF/ORD editors do not use it.
- `FX_Indicatori` already gives `CodAI` (`CodAng-CodInd`) -> `IdClsf`, so the indicator link needs no per-unit `IdClsf`.

### Sub-slices
| Sub-slice | What | Depends on |
|-----------|------|-----------|
| 0121-01 | [Schema celor două tabele](#slice-0121-01) | - |
| 0121-02 | [Migrator și raportul fiecărui rând](#slice-0121-02) | 01 |
| 0121-03 | [Fereastra Parteneri și compatibilitatea 1.1.2.4](#slice-0121-03) | 01 |
| 0121-04 | [Hook-uri și tranzacția asocierii](#slice-0121-04) | 01 |
| 0121-05 | [Recepții: propuneri, salvare și eliminare](#slice-0121-05) | 01, 04 |
| 0121-07 | [Copie unică din Parteneri_Coduri](#slice-0121-07) | 01 |
| <a id="slice-0121-06"></a> 0121-06 | Închidere: numai după probe și acordul operatorului | 02, 03, 05 |

### Open threads
- Inițial DDL-ul nu era scris; etapa 01 de mai jos îl consemnează scris, încă nerulat. Conținutul real `Parteneri_Coduri` (`CodAng` completat?) rămâne nevăzut; 07 păstrează interogările de control.
- The exact normalization of CodFiscal for persons (CNP) and foreign VAT ids to be checked in the Migrator report (0121-02).

## SLICE-0121-01 - Schema: supplier <-> angajament links by CodFiscal (19.10.2026) <a id="slice-0121-01"></a>

### What changed and why
One new script, `sql/P121_01_parteneri_angajamente.sql`, with two new tables. `Parteneri` and `Parteneri_Coduri` are NOT touched (no ALTER, no index change); no code reads or writes the new tables yet.

| Table | Purpose | Key |
|-------|---------|-----|
| `Parteneri_Angajamente` | the link supplier <-> angajament | PK `IdLegAng`; unique `(CodFiscal, CodAng)`; index on `CodAng` |
| `Parteneri_Angajamente_Indicatori` | the indicators of a link, with `ContBancar` (NULL = none) | PK `IdLegInd`; unique `(IdLegAng, CodAI)`; index on `CodAI`; FK `IdLegAng` -> link, ON DELETE/UPDATE CASCADE |

Choices (all from the plan, plus the ones I made):
- The keys above implement the [plan decisions](#slice-0121-plan): normalized CUI by `primite_ubl.normalize_cui`, multiple angajamente per supplier/classification, unlike old `(IdPartener, IdClsf)`.
- No per-unit `IdClsf`: `CodAI` -> `FX_Indicatori.IdClsf` gives it.
- **No FK to `FX_Angajamente`/`FX_Indicatori`** (model choice, as `EF_PrimiteReceptii`): links may precede sync, deleting an angajament must not delete supplier links, and 0121-02 must report missing CodAI. Only indicator → link has FK.
- Text: `utf8mb3`/`utf8mb3_general_ci`; `CodFiscal`/`CodAng`/`CodAI` varchar(255) to avoid truncating odd legacy values. Largest unique key: 255×3 + 255×3 = 1530 bytes < InnoDB 3072.
- Date columns follow `Parteneri_Coduri` (`DataAdugare`, `DataModificare`).
- `CREATE TABLE IF NOT EXISTS`: safe to run twice.

### Files touched
`sql/P121_01_parteneri_angajamente.sql` (new); `docs/worklog/SLICE-0121-01-schema.md` (this file); `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md` (status)

### Test results
Nothing run: the DDL has not been executed anywhere (the operator runs it by hand; Claude deploys nothing). Not syntax-checked against a live MariaDB.

### Left unverified or deferred
- Operator: run the script on `AVACONT_SURSA`, then sync the structure to the unit databases with KBotAdmin, then refresh `MariaDB_Schema/` (gitignored, by hand).
- `sql/AVACONT_SURSA.sql` (hand-written, already drifted) was not edited.
- No help change: nothing the operator sees changed.
- Next: 0121-02 (Migrator). Operator, 19.10.2026: no way to test the Migrator side now - it is built only, tested when the time comes.

## SLICE-0121-02 - Migrator: `ParteneriAng` -> supplier <-> angajament links by CodFiscal (19.10.2026) <a id="slice-0121-02"></a>

### What changed and why
Access `ParteneriAng` now writes the two CodFiscal-keyed tables of [01](#slice-0121-01), replacing `Parteneri_Coduri`; the partner need not exist in the server unit.

- **Source of the CodFiscal:** the unit's own Access `Parteneri` (by `CodPartener`), normalized with `EfCui.Normalize` (the same rule as the DDF's `normalize_cui`). Not read from the target.
- **What is written:**
  - `Parteneri_Angajamente(CodFiscal, CodAng)` - one row per pair;
  - `Parteneri_Angajamente_Indicatori(IdLegAng, CodAI, CodInd, ContBancar)` - one row per `CodAI = CodAng-CodInd` (the key of `FX_Indicatori`); `ContBancar` from Access `ContBanca`.
  - The Access `IdClsf` / `Clsf` are not written (`CodAI` already gives the classification; `Clsf` is read only for the cross-unit report).
- **Upsert, safe to run twice:** the link finds its id back through `LAST_INSERT_ID(IdLegAng)`; an indicator fills `CodInd` / `ContBancar` only where still empty - what the operator edited after a first run is never overwritten.
- **Nothing is lost silently.** Every Access row ends in exactly one place: written, merged into a repeated pair, or listed in the report:
  - partners with no usable CodFiscal (count + list per unit) - no rows in the new tables, they stay in `Parteneri`;
  - partner codes of `ParteneriAng` missing in the unit's `Parteneri`;
  - rows with a partner but no `CodAng` (no link possible);
  - rows with `CodAng` but no `CodInd` (link written, no indicator);
  - repeats merged; two different non-empty bank accounts on the same link + `CodAI` (the first stays, listed);
  - same CodFiscal + same classification with different `CodAng` in two or more units (the operator believes it never happens - the report shows whether it does); all are written, the key is `(CodFiscal, CodAng)`;
  - CodFiscal values that are not all digits after normalization (CNP / foreign VAT ids) - listed, written as they come out;
  - `CodAI` missing in `FX_Indicatori` (the target's plus the ticked units' FOREXE files) - listed, still written (no foreign key).
  Only an unreadable `ParteneriAng` blocks.
- The ticked table is still «Parteneri_Angajamente» (Access source `ParteneriAng`); the indicators table is written with it. `Parteneri_Coduri` is not written any more and stays on the server untouched (0121-06 drops it, at the operator's word).

### Files touched
`src/KBot.Migrator/Transfer/PartnerLinkSet.vb` (new): `PartnerLinkSet`, `PartnerLink`, `PartnerLinkIndicator`, `PartnerRowCount`, `PartnerLinkReader` (reads Access, shapes the links, reads the known `CodAI`). `src/KBot.Migrator/Transfer/TableMap.vb`: `WritesPartnerLinks`, `CompanionTable`, `AsPartnerLinks`. `src/KBot.Migrator/Transfer/TableMaps.vb`: the `ParteneriAng` map now targets `Parteneri_Angajamente` (+ companion). `src/KBot.Migrator/Transfer/Finding.vb`: new kinds `LEGATURI_PARTENERI`, `PARTENER_FARA_COD_FISCAL`, `PARTENER_LIPSA_ACCESS`, `LEGATURA_FARA_ANGAJAMENT`, `LEGATURI_CONFLICT_UNITATI`, `CONT_BANCAR_DIFERIT`, `INDICATOR_LIPSA_FX`, `COD_FISCAL_NENUMERIC`. `src/KBot.Migrator/Transfer/Verifier.vb`: companion table existence, the 1364 / missing-column guard for both tables, and the new gate `CheckPartnerLinks` (in the existing step «Se rezolvă partenerii…»). `src/KBot.Migrator/Transfer/TransferRunner.vb`: `WritePartnerLinks`, called from the write loop for a `WritesPartnerLinks` map. `src/KBot.Migrator/KBot.Migrator.vbproj`: FileVersion 1.20.0.0 -> 1.21.0.0. Status: `state/KBOT_STATUS_0120-0129.md`, `KBOT_STATUS.md`, the plan file.

### Test results
`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: succeeded, 0 warnings, 0 errors. **Nothing else was run, on purpose** (operator, 19.10.2026: no way to test the Migrator now - built only; no tests written or run, no Access file opened, no database touched). The test project was not built.

### Left unverified or deferred
- Never run: the Access column names are taken from `qFX_Ang_Part` (`CodAng, IdClsf, CodPartener, CodInd, Clsf, ContBanca`) and from the old map; Access `Parteneri` is assumed to have `CodFiscal` (it travelled by name match to `Parteneri.CodFiscal`) and a name column (`DenumirePartener`, else `NumePartener`; only used to label the report).
- `LAST_INSERT_ID(IdLegAng)` through MySqlConnector's `LastInsertedId` is the expected behaviour but unproven here; a lookup by `(CodFiscal, CodAng)` is the fallback when it returns 0.
- The verifier and the runner each read the Access files (the runner does not take the verifier's `PartnerLinkSet`); both read the same files, so the figures agree unless a file changes in between.
- Not decided by me: a CodFiscal longer than 255 characters or a `CodAng` longer than 255 would fail the INSERT (loud, rolls back), not be cut.
- `tests/KBot.Migrator.Tests` was not checked for tests of the old `ParteneriAng` -> `Parteneri_Coduri` map (a grep found only a comment).
- No help change (the Migrator is an owner tool).

## SLICE-0121-03 - «Parteneri» window + `parteneri_edit.py` on the new link tables (19.10.2026) <a id="slice-0121-03"></a>

### What changed and why
The window's «Coduri angajament» no longer read or write `Parteneri_Coduri`; they belong to the supplier's normalized CodFiscal (`Parteneri_Angajamente` / `Parteneri_Angajamente_Indicatori`).

Server:
- New `PYTHON/routes/forexe/partner_links.py`: deliberately copies DDF `normalize_cui` to avoid importing the efactura blueprint; `read_links` uses one link LEFT JOIN indicators LEFT JOIN `FX_Indicatori` LEFT JOIN `Clasificatii` (classification display by CodAI); `save_codes` writes.
- `save_codes`, in the caller transaction: delete removed indicators/empty links after CodFiscal check; require Cod ang./unique CodAng+CodInd; remove changed identities BEFORE writing to allow code swaps; upsert link/indicator with CodAI = CodAng-CodInd, overwriting CodInd/ContBancar from the authoritative window. Delete an indicatorless link unless still named by a grid row. Foreign-supplier ids are refused in rows, ignored in deletes.
- `parteneri_edit.py` uses the service; wire adds `id_leg`/row and `legaturi_sterse`, permits null `id_clsf`. Removes old SQL/CodPartener refresh; 1062 names the new key. Codes without CodFiscal are refused; partner deletion preserves fiscal-code-owned links.

Client:
- Domain `PartenerCod` (`IdLeg`, nullable `IdClsf`), `PartenerSaveRequest.LegaturiSterse`; `ApiClient.Nomenclatoare` wire.
- `ParteneriForm`: Clasificație readonly text, no combo/`ClsfChoice`; hidden `id_leg`. `ReadCodes` requires Cod ang./unique pair. Deleted indicator → `CoduriSterse`; angajament without indicator → `LegaturiSterse`. `CodesAllowed`/`UpdateCodesState` disable the grid with title hint for missing fiscal code or an unsaved change to a saved partner code; deletion question says links remain.
- Help 0000-113 (separate worklog).

### Files touched
`PYTHON/routes/forexe/partner_links.py` (new), `PYTHON/routes/forexe/parteneri_edit.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/ParteneriForm.vb` + `.Designer.vb`, help files (see 0000-113), FileVersion: Domain 1.2.17.0, Api 1.0.25.0, App 1.1.2.5.

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Both Python files pass `py_compile`. **No test written or run, the app not started, no database touched** (operator: no tests). 

### Left unverified or deferred
- Never run against a database: the SQL of `partner_links.py`, the transaction behaviour and the grid (read-only column, hidden column, disabled state) are unverified. The Designer was edited by hand, never opened in Visual Studio.
- The server still sends the `clasificatii` list (the window no longer uses it): left so the wire does not change more than needed.
- A partner whose CodFiscal is edited keeps no link: the old links stay on the old CUI (by design: they belong to the supplier, not the partner row).
- Old rows of `Parteneri_Coduri` are read by nobody now; they reach the new tables only through the Migrator (0121-02). Until that runs, the window shows empty codes.
- Server files go to the VPS by the operator's hand (never by Claude).

### Corrective 20.10.2026 - compatibility with the deployed client (App 1.1.2.4)
Operator requires deployed App 1.1.2.4 compatibility. It reads `id_clsf` as Integer; null for indicatorless/unknown CodAI links would break the whole partner list. `partner_links.read_links` now sends `id_clsf: 0` (unknown), unused by the new client. In 1.1.2.4, a row without Cod ang. is still refused explicitly because CodAng keys the link. `py_compile` OK; no database run.

## SLICE-0121-04 - Hooks: partner <-> DDF / angajament association fills the supplier's link (19.10.2026) <a id="slice-0121-04"></a>

### What changed and why
Saving a supplier/document association inserts its missing angajament link and indicators, avoiding manual entry in Parteneri.

- `partner_links.py`:
  - `ensure_link(cursor, cod_fiscal, cod_angajament)`: find/insert normalized `(CodFiscal, CodAng)`; insert missing `FX_Indicatori.CodAngajament` indicators (`CodAI`, `CodInd = CodIndicator`, no bank account). **Insert only:** preserve existing links, CodInd/ContBancar; delete nothing.
  - `ensure_for_partners(cursor, db_name, cod_angajament, cods_fiscale)`: the DDF hook; skips silently (a warning in the log) when the database has no link tables, so a DDF save never depends on 0121-01 being deployed.
  - `tables_present`: positive-only cache, like `FX_DDF_Parteneri`'s probe.
  - New route `POST /api/forexe/parteneri/angajament {cod_fiscal, cod_angajament}` -> `{id_leg, legatura_noua, indicatori_adaugati}` (400 no fiscal code / tables missing - names the SQL file, 404 unknown angajament): the save the page «Recepții» will call in 0121-05. Journalled in `actiuni.py` («Legat un furnizor de un angajament»).
- `ddf_parteneri.py`: `sincronizeaza_parteneri` (the DDF editor's save) and `adauga_parteneri` (the Sumar button) take `cod_angajament` and call the hook for the partners they store / add. `parteneri is None` (an older client that sent no list) still touches nothing.
- `ddf_edit.py`: the save passes the document's `cod`; `post_ddf_parteneri_asociati` passes the code of the request.
- Runs inside the caller's transaction and only on a SAVE - a mere view/GET writes nothing. A database error in the hook is not swallowed (it fails the save, as any other write of it would).

### Files touched
`PYTHON/routes/forexe/partner_links.py`, `ddf_parteneri.py`, `ddf_edit.py`, `PYTHON/routes/actiuni.py`.

### Test results
`py_compile` of the four files: OK. **No test written or run, no database touched** (operator: no tests).

### Left unverified or deferred
- Never run: the SQL, the transaction behaviour and the link counts are unverified. The partners named by a save get the link; the plan said «partner <-> angajament» too - that is the new route, which nothing calls until 0121-05.
- Backfill: a DDF whose partners were associated before this slice gets its links the next time its partners are saved; the Migrator (0121-02) covers the old `ParteneriAng` rows.
- No client and no help change (nothing the operator sees). Server files go to the VPS by the operator's hand.

## SLICE-0121-05 - Page «Recepții» (KE16): the supplier's angajamente from the new tables, the pick saved (19.10.2026) <a id="slice-0121-05"></a>

### What changed and why
The Angajamente pick now persists by normalized supplier tax code, implementing the [19.10 operator decision](#slice-0121-plan).

Server:
- `primite_ke.py` `_angajamente_candidate` adds `Parteneri_Angajamente` WHERE `CodFiscal = invoice.CuiNormalizat`, source **furnizor**, after DDF (keeps ddf) and preserving legatura for a linked one. `_are_legaturi_furnizor` probes `DATABASE()` to tolerate absent tables. Bulk `_facturi_neasociate` already uses these candidates, so proposals gain supplier links without changing `primite_ke_propuneri.py`.
- `partner_links.py`: new `POST /api/forexe/parteneri/angajament/scoate {cod_fiscal, cod_angajament}` -> `{sterse}` (deletes the link, its indicators go by cascade); the save is the route of 0121-04. Journalled in `actiuni.py`.

Client:
- `IEFacturaApi` / `ApiClient.EFactura.Ke.vb`: `LinkSupplierAngajamentAsync`, `UnlinkSupplierAngajamentAsync` (paths under `/api/forexe/parteneri/angajament`).
- `KeLegaForm` retires `_added`/`AddedForContext`. `BeginAddAngajament` saves by `SupplierCui` for each context supplier (ticked invoices, otherwise clicked invoice); missing tax code gives notice/no link. `RefreshContextStatesAsync`/new `_browsedId` discard that supplier invoice states, reread visible ones and rebuild from server. Elimină asociere confirms/removes furnizor links for those suppliers. `KePropuneriForm` gets an empty picked-code list (server resolves); both KE16 routes retain `angajamente` for old clients.
- Help 0000-114 (separate worklog).

### Files touched
`PYTHON/routes/efactura/primite_ke.py`, `PYTHON/routes/forexe/partner_links.py`, `PYTHON/routes/actiuni.py`, `src/KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Ke.vb`, `src/KBot.EFactura/Forms/KeLegaForm.vb`; FileVersion: Api 1.0.26.0, EFactura 1.8.0.0; help `contabil/efactura/index.md`.

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. `py_compile` of the Python files: OK. **No test written or run, no database touched, nothing seen on screen.**

### Left unverified or deferred
- Never run: the `furnizor` candidates, the save/remove round trips and the redraw of the table after a pick are unverified. The scenario to check: supplier TRANSPORT CALATORI EXPRESS - pick an angajament, press another supplier and back, close and reopen the page.
- The route of the save requires a session only (like the «Parteneri» window), not the role «Contabil» that the KE writes check.
- Removing a «furnizor» link deletes its indicators and bank accounts too (asked in the question). A link made by the DDF hook (0121-04) reappears at the next DDF save of that partner.
- The embedded variants (a view of one invoice / one angajament) use the same table; only the pick path changed.
- Server files go to the VPS by the operator's hand.

## SLICE-0121-07 - One-time query: copy the old `Parteneri_Coduri` into the new link tables (19.10.2026) <a id="slice-0121-07"></a>

Operator (19.10.2026): only one unit is left to migrate, so the Migrator cannot carry the old data (and it would not carry what was typed in K-BOT after the migration); wanted: a run-once SQL, run from KBotAdmin like the earlier ones.

### What changed and why
New `sql/0121_07_interogare_unica.sql`, for KBotAdmin page «Interogari unice» (name `0121_07_parteneri_coduri_in_angajamente`). On AVACONT_SURSA and every ticked unit database it copies the old «Coduri angajament» into the new tables:
- `Parteneri_Coduri` JOIN `Parteneri` -> CodFiscal normalized in SQL like the DDF rule (letters and digits only, upper case, leading «RO» before a digit dropped; `REGEXP_REPLACE`), `CodAng`, `CodInd`, `ContBancar` into a temporary table;
- `Parteneri_Angajamente`: one link per `(CodFiscal, CodAng)`, inserted only when missing;
- `Parteneri_Angajamente_Indicatori`: `CodAI = CodAng-CodInd`; an existing indicator keeps what it has and is filled only where `CodInd` / `ContBancar` are empty, so data already typed in the new window is never overwritten.
- Left in the old table (listed by the check queries at the end of the file, as comments: a one-time query returns no result set): rows whose partner has no usable CodFiscal, rows with no `CodAng`. The old `IdClsf` is not copied (`CodAI` gives it). A row with `CodAng` and no `CodInd` makes the link only.
- Nothing is deleted; safe to run again; the five statements were split by the runner's own `split_statements` (checked, no database touched).

Order: `P121_01` on AVACONT_SURSA -> structure sync to the units -> the new Python files on the server -> this query. A database without the new tables fails and is left unmarked.

### Files touched
`sql/0121_07_interogare_unica.sql` (new); status + plan rows.

### Test results
Only the splitter of `routes/one_time/runner.py` was run on the file (5 statements). The SQL was not executed anywhere (no database touched).

### Left unverified or deferred
- Never run: `REGEXP_REPLACE` needs MariaDB 10.0.5+ (the server runs MariaDB; version not checked here); the `INSERT ... SELECT ... ON DUPLICATE KEY UPDATE` with `VALUES()` is the usual form but unrun.
- Normalization in SQL treats only ASCII letters and digits; the Python rule accepts any letter. For tax codes this makes no difference; a foreign id with an accented letter would be cut differently.
- Whether `CodAng` is filled in the old data is still unseen: run the first check query before the run.
- 0121-06 (dropping `Parteneri_Coduri`) waits for this query's result, for the last unit's Migrator run and for the operator's word.

## Reper ulterior — 23.10.2026

[0122-01](SLICE-0122-parteneri-fara-idpartener.md#slice-0122-01) menționează că operatorul rulase interogarea 0121_07, fără a preciza acolo pe câte baze. „07 scris, nerulat” de mai sus rămâne checkpointul din 19.10; nu îl extindem la toate bazele și nu închidem 06 prin această citare.
