# PLAN_AutoProvisioning.md — self-service unit database creation (public web page)

**Slice 0075** (see `KBOT_STATUS.md`; the plan was drafted as 0073, which the Recorder slice
already holds). Worklogs: `docs/worklog/SLICE-0075-0N-*.md`, one per pass.
Read `CODE_WORKFLOW.md` and `CLAUDE.md` before starting.

Replaces the operator's manual Navicat work: a new unit fills in a public page, the operator
approves with one click, and the server creates the database, the registry rows, the
classifications and the MariaDB user.

Every fact below is marked **VERIFIED** (seen in the repo) or **UNVERIFIED** (to confirm in
Step 0). If this plan and the code disagree, the code wins — say so, do not follow the plan
into a false claim. **§0 (Step 0 findings, 22.09.2026) overrides every later section where
they differ.**

---

## 0.0 START HERE — state on 22.09.2026, end of day

**Pass 0075-00 is applied.** `Clasificatii.Sector`, `Sursa` and `SS` are written columns on
the K-BOT server, the six other generated columns are untouched, and the updated writers
(`routes/clasificatii_ss.py`, `clasificatii.py`, `nomenclatoare.py`) are on the VPS with
gunicorn restarted. Run by the operator; the developer has no server access.

⚠️ **One thing the script did not do by itself:** the three virtual columns on
`AVACONT_SURSA.Clasificatii` had to be removed **by hand** before the run would go through.
The reason was not captured. Before trusting `scripts/clasificatii_sursa.py` again — 0075-03
does not use it, but a future unit database will — find out whether the seven unit databases
went through cleanly or also needed hand work, and what exactly the template refused.
Everything else about the script (backup, snapshot, verification against it) is unexercised
on a table with rows, since the template is empty.

**Next pass: 0075-01** (§5.1–5.3) — the pre-auth registration store, the ANAF proxy, `/cod`
and `/verifica`, with pytest. Nothing blocks it: nginx needs no change (§F9 is closed, a
single `location /` proxies everything to Flask), the JS components are complete (§F2a), and
D24–D26 answer the role, CodProgram and account questions.

**Not started, and separate: 0075-06** (§13) — the rights of the accounts that already exist.
The grants were read on 22.09.2026 and are less bad than first reported; see §13.

## 0. Step 0 findings (22.09.2026) — read before anything else

Everything here was read from files in the repo or from the live server output the operator
pasted. Decisions D20–D23 were taken by the operator the same day.

| # | Finding | Consequence |
|---|---|---|
| F1 | Slice 0073 is the Recorder slice; 0074 the Browser view | **D20:** this work is slice **0075** |
| F2 | `JS_COMPONENTS/treeview` is a single-select dropdown (`onSelect` fires once and closes); no checkboxes, no tri-state. `combobox` is single-select (`readonly` + `staticData` fits **An** only) | **D21:** add an opt-in checkbox mode to the tree (`checkable: true`, tri-state, result = checked leaves); **SectorSursa** = plain checkbox list |
| F2a | The four pieces F2 first reported missing **arrived on 22.09.2026**: `JS_COMPONENTS/listener-tracker/` (5 files, itself importing `../event-bus/event-bus.js` — also present), `utils/z-max.js` (`window.ZIndexManager`), `utils/css.js` (`window.getClassNumericProperty`), and the stylesheets `css/treeview/*` + `css/combobox.css` | The components import `../../listener-tracker/…`, i.e. TWO levels up: they must be served from `static/js/components/<name>/` with the shared folders at `static/js/`. That layout is respected rather than the import paths edited (D1: use them as they are) |
| F3 | `schema_sync` **refuses** a missing DB (`schema_common.verify_targets` raises «Baze inexistente pe server»). The only creation path is `routes/admin.py::setup_database`: `CREATE DATABASE` + `SHOW CREATE TABLE`/`VIEW` clone of `AVACONT_SURSA`, X-Api-Key, **legacy** server | **D22:** the provisioning job ports that clone loop into its own module on the K-BOT server (`DB_CONFIG_NEW`); `schema_sync` is not used for creation |
| F0 | **The K-BOT server has 7 unit databases, not the 22 §2a lists** (dry run, 22.09.2026): `000_DEMO`, `006_GR35`, `014_SCSV`, `027_SCGM`, `030_SCTC`, `045_CTER`, `050_GRSA`. The 22-name list in §2a was read off the LEGACY server. `AVACONT_SURSA.Clasificatii` is empty (0 rows) | §5.4's free-name check scans `SCHEMATA` **and** `CAI` on the K-BOT server, so the shorter list is what counts. §2a's list is stale — do not use it to predict a free `nn` |
| F4 | Login (`routes/auth/auth.py`, gitignored) reads `AVACONT_COMUN.Unitati (DC PK, NumeUnitate NOT NULL, CF NOT NULL)`, `Unitati_Utilizatori (UN varchar(80), DC, Rol varchar(32) NOT NULL, LastSS)` PK `(UN, DC)` FK `DC → Unitati`, `Unitati_Ani (DC, AN, SS varchar(16), CodProgram varchar(64) NOT NULL)` PK `(DC, AN, SS)` FK `DC → Unitati`; audit in `Jurnal`. `CAI` is read only by `schema_sync` discovery. `Rol` holds `Contabil` on all 3 live rows | **D23:** the job writes `Unitati` + `Unitati_Utilizatori` (`Rol = 'CO'` per D4, one constant) + one `Unitati_Ani` row per SS × An, **plus** `CAI` for `schema_sync`. D17's `Utilizatori_Roluri` is dropped. Open: whether the 3 `Contabil` rows become `CO`, and the `CodProgram` value for a new unit (`000_DEMO` has `0000002510` / `0000000000`; default `0000000000` until told otherwise) |
| F5 | `session_store.put_note/get_note/delete_note` accept any token string and never touch the session keys; the guard validates only `kbot:sess:<token>` | The pre-auth store of §5.1 = notes on a registration token (`name = "register"`, TTL 30 min). No new backend code |
| F6 | Every data route uses the **service account** (`utils/database.py`: "the caller picks a SERVER, not an identity"); the operator's MariaDB login is used only by `_verify_operator`, with no default DB | D18's grants are not needed by K-BOT itself; they are still applied as decided |
| F7 | `Clasificatii` has a fifth, **local** FK `Clasificatii__Unitati (IdUnitate) → Unitati ON DELETE CASCADE` | `Unitati` rows precede `Clasificatii`; `DROP DATABASE` unwinds both |
| F8 | K-BOT server: no password-validation plugin loaded (`SHOW PLUGINS`); `strict_password_validation=ON` is inert without one | The random unusable password of §5.6 step 7 cannot be refused |
| F9 | `main.py` serves no static files; nginx proxies to gunicorn `127.0.0.1:5009`. The `location` rules for `kbot.avatarsoft.ro` are **UNVERIFIED** | The page is served by a Flask blueprint (`send_from_directory`); the nginx block must be confirmed before 0075-04 |
| F10 | `.venv` has no `requests` | ANAF proxy uses stdlib `urllib.request` |
| F11 | `LIMITER` API: `is_blocked(ip, key)`, `record_failure`, `record_success`; 5 per key / 15 min, 30 per IP / min, 15-min lockout | Reused as-is with key = e-mail (or CF) |
| F12 | `mailer.py` has one hard-coded message (`send_password_code`) | New `send_*` functions in the same file for the registration mails |
| F13 | Migrator: `ClasificatieDerived` replicates the nine generated columns; `TableMaps` notes «Nine target columns are GENERATED»; `Verifier` gates five dictionaries (incl. `DefaClsfE`, which has no FK on the server — harmless) | §12 applies as written |

Still to obtain from the operator: `SHOW GRANTS` for one e-mail account and one legacy
`_Contabil` account (step 7 of §5.6), the nginx `server` block, and the four missing JS pieces.

---

## 1. Decisions taken (operator, 22.09.2026)

| # | Decision |
|---|---|
| D1 | Public web page, built new, served by the Flask app. Uses the custom tree and combobox from `JS_COMPONENTS` (see D21) |
| D2 | Operator approval is required before anything is created on the server |
| D3 | DB name `1nn_SSSS`: `n` ∈ 1..9, `SSSS` = first 4 consonants of the unit name |
| D4 | Username = the e-mail, verified by a 6-digit code. Role suffix is deprecated. Roles do not exist yet; for now exactly one: **`Contabil`**. `Administrator` and `Director` come later — the design must take them without rework. *(Amended by D24: the values are the Romanian words already in the table, not the codes `CO`/`AD`/`DR` this row first carried.)* |
| D5 | The tree selection writes `Clasificatii`: one row per checked F leaf × checked E leaf × chosen SS. `Capitol = Left(ClsfF,2) & "." & Left(SS,2)`, except `SS = "02E"` → `Left(ClsfF,2) & ".10"` (dotted, as on real rows: `65.01`) |
| D6 | `IdUnitate = MAX(CAI.IdUnitate) + 1`. `An` defaults to the current year, allowed range `[current-1, current]`, never the future |
| D7 | **CF is the first input.** It drives an ANAF lookup (`PlatitorTvaRest/v9/tva`) that pre-fills the unit's data |
| D8 | Nothing is written on the legacy server |
| D9 | A leaf whose `xx00` parent is missing is ignored (superseded by D14 when the root exists) |
| D10 | Approval happens on an operator page that shows the request's details |
| D11 | A CF already present in `CAI` (and in `AVACONT_COMUN.Unitati`) is refused |
| D12 | `SSSS`: first 4 consonants; if fewer than 4, vowels fill the rest in name order |
| D13 | The password is set through a one-time link e-mailed after approval |
| D14 | A leaf whose `xx00` parent is missing hangs directly under its root `xx0000`; a leaf with no root either is ignored |
| D15 | All 14 `DefaSursaSector` values must be storable → `Sursa` becomes a written column (§11) |
| D16 | A `DefaClsfE` row with NULL/empty `Denumire` is not shown in the tree |
| D17 | ~~`Utilizatori_Roluri(Email, DbName, Rol)`~~ — replaced by D23 (`Unitati_Utilizatori.Rol` already exists) |
| D18 | New users get only what their work needs, on their own DB — never global rights. Provisioning uses its own account, not `AVACONT` |
| D19 | Q11 shape approved; migration per §11, verified row by row; Migrator updated per §12 |
| D20 | Slice number **0075** |
| D21 | Tree: opt-in checkbox mode added to `JS_COMPONENTS/treeview`; SectorSursa = checkbox list; combobox for An |
| D22 | DB creation: port of `admin.py::setup_database`'s clone loop, on the K-BOT server |
| D23 | Login rows: `Unitati`, `Unitati_Utilizatori (Rol)`, `Unitati_Ani` (per SS), plus `CAI` |
| D24 | **Role values are the Romanian words**, as already stored: `Contabil` (what D4 called `CO`), later `Administrator` (`AD`) and `Director` (`DR`). The 3 existing rows are **not** touched. One constant in the code, three values, no codes |
| D25 | **`Unitati_Ani.CodProgram` follows the sector**: `01` ▸ `0000002510`, `02` ▸ `0000000000`. Prefilled that way and **editable by the operator on the approval page**, per (An, SS) row, before the job runs. A sector outside 01/02 (now reachable, see D15) has no known value → prefilled `0000000000`, editable like the rest |
| D26 | **The `NNN_XXXX_Contabil` / `_Administrator` accounts disappear.** Every user, at every level, is a MariaDB account named by their e-mail; the role is `Unitati_Utilizatori.Rol`, not the account name. **Separately: every account on the server today carries SU rights, which is wrong and has to be fixed** — its own job, see §13 |

---

## 2. What already exists (VERIFIED)

- `AVACONT_SURSA` is the schema template (`sql/AVACONT_SURSA.sql`). ~~The server-side
  `schema_sync` job creates a unit DB from it~~ — **wrong, see F3.** `SchemaSyncClient`
  validates the DC with `^[0-9]{3}_[A-Za-z0-9]+$` — `1nn_SSSS` fits.
- `AVACONT_COMUN.CAI` (`sql/avacont_comun_login.sql`, stale — see §2a): `DbName`, `NumeUnitate`,
  `AlteDetalii`, `Sursa` (SectorSursa), `CF`, `CodProgram`, `AnDate`, `DC`.
- `AVACONT_COMUN` login tables: see F4.
- Unit DB `Unitati`: `IdUnitate` (not auto-increment), `Detalii` NOT NULL, `SursaSector` varchar(3)
  NOT NULL, `An` NOT NULL, `CodProgram`, `Ascuns`.
- Unit DB `Clasificatii`: writable `IdUnitate`, `Capitol` varchar(5), `Subcapitol` varchar(5),
  `Articol` varchar(5) (FK `AVACONT_COMUN.DefaArticol`), `Alineat` varchar(2), `Denumire`
  varchar(255) NOT NULL, `IdClsfAcc` (NOT NULL DEFAULT 0). Generated: `ClsfF = Left(Capitol,2) &
  Replace(Subcapitol,'.','')`, `ClsfE = Replace(Articol,'.','') & Alineat`, `SS` from
  `Right(Capitol,2)`: `01→01A`, `00→01A`, `02→02A`, `10→02E`. Four generated columns carry FKs into
  `AVACONT_COMUN` → a bad derivation is a `1452` at INSERT. Fifth FK is local (F7).
- 2FA building blocks from slice 0072: `routes/auth/mailer.py`, 6 digits from `secrets`, SHA-256
  hash stored, `hmac.compare_digest`, ≤ 5 attempts, 10-minute TTL, `ratelimit.py::LIMITER`.
  Codes are stored as **session notes** — a registrant has no session, so the note is keyed by
  the registration token (F5).
- Logins are real MariaDB accounts; the password is set with `SET PASSWORD` on the operator's own
  connection.

## 2a. Live server, read 22.09.2026 (VERIFIED — overrides §2 where they differ)

- MariaDB `10.11.14`, `lower_case_table_names = 0` → **DB names are case-sensitive**; always
  uppercase `SSSS`. `sql_mode` has `STRICT_TRANS_TABLES` → an over-long value is error `1406`, not a
  truncation.
- **`CAI` differs from the repo copy:** PK is `IdCai` AUTO_INCREMENT; `IdUnitate` is **not unique**
  (plain column). `MAX(IdUnitate) = 200`, 70 rows. `DbName = DC` on every row. D6 therefore needs
  its own lock (`GET_LOCK('cai_idunitate')`) — no key protects it. `sql/avacont_comun_login.sql`
  is stale; fix it in 0075-02.
- Existing DBs: `000_DEMO`, `001_GR23` … `053_LTTR`, `101_CCDP` (22). Pattern `NNN_XXXX`;
  `101_CCDP` predates D3 (has a 0). The free-name check must look at `SCHEMATA` **and** `CAI`.
- **`Clasificatii` has FOUR cross-database FKs, not five:** `Articol▸DefaArticol`,
  `ClsfF▸DefaClsfF`, `SS▸DefaSursaSector`, `Titlu▸DefaTitlu` (`Titlu = Left(Articol,2)`). No FK to
  `DefaClsfE`. `MAPARE_NOMENCLATOARE.md` §3.1 is stale on this.
- `DefaClsfF`: **no PK**, `ClsfF` nullable, non-unique index → duplicates/NULLs possible; the list
  endpoint uses `SELECT DISTINCT … WHERE ClsfF IS NOT NULL`.
- `DefaClsfF`/`DefaClsfE`/`DefaArticol`/`DefaTitlu`: caption column is **`Denumire`**
  (closes the 0022-01 `Explicatie` doubt). Types are `mediumtext`/`longtext`.
- All codes are 6 characters. F: 52 roots, **1** level-1, 531 leaves. E: 25 roots, 317 level-1,
  686 leaves.
- **Split, confirmed on real rows** (`650402` / `200104` → `65.02 · 04.02 · 20.01 · 04`):
  - `Capitol = Left(F,2) & "." & <sector>` (D5)
  - `Subcapitol = Mid(F,3,2) & "." & Mid(F,5,2)`
  - `Articol = Left(E,2) & "." & Mid(E,3,2)`, `Alineat = Right(E,2)`
  - A level-1 code is a legitimate row: real data has `650500` (`05.00`) and `590100` (alineat `00`).
- `Denumire` (NOT NULL, varchar 255) ← `DefaClsfE.Denumire`, trimmed and cut to 255 before INSERT
  (strict mode). NULL caption → D16.
- `DefaSursaSector` has 14 rows: `01A 01D 01E 01F 01G 02A 02C 02D 02E 02G 03A 04A 05A 08A`.
  **The generated `SS` column can only produce `01A`, `02A`, `02E`** (and `''`) → §11.
- Roles: `Unitati_Utilizatori.Rol` = `Contabil` on all 3 rows (F4).
- F orphans: **252**, all with an existing root → D14 places them. E orphans: 0.
- **15 E codes cannot be inserted** (FK would fail) and are therefore **not offered in the tree**,
  filtered server-side by joining `DefaArticol` / `DefaTitlu`:
  - missing in `DefaArticol`: `20.38`, `50.05`, `59.45`, `61.16`, `66.01`, `66.02`, `66.03`, `80.14`
  - missing in `DefaTitlu`: `66`, `91`, `92`, `93`
- `DefaClsfE.Denumire`: 0 empty, **9 longer than 255** → cut to 255.
- Accounts: all `mysql_native_password`, host `%`. Three generations coexist: legacy
  `NNN_XXXX_Contabil` / `_Administrator`, service accounts `AVACONT`, `Admin`, and e-mail users.
  New users: `CREATE USER '<email>'@'%' IDENTIFIED VIA mysql_native_password`, e-mail ≤ 80
  characters (`Unitati_Utilizatori.UN` is varchar(80) — refused on the page above that).
- No password-validation plugin (F8).

## 3. Step 0 — done 22.09.2026, see §0

---

## 4. The page — `https://kbot.avatarsoft.ro/inregistrare` (wizard, Romanian UI)

One HTML page + one JS module, served as static files by Flask, using the `JS_COMPONENTS` tree
(checkbox mode, D21) and combobox. No external CDN.

| Step | Screen | Server call |
|---|---|---|
| 1 | **Cod fiscal** → «Caută» → shows the ANAF data (name, address) for confirmation | `POST /api/inregistrare/anaf` |
| 2 | **E-mail** → «Trimite codul» → code → «Verifică» | `POST /api/inregistrare/cod`, `/verifica` |
| 3 | **Denumire unitate** (pre-filled from ANAF, editable) → the proposed DB name is shown read-only | `GET /api/inregistrare/nume?denumire=` |
| 4 | **SectorSursa** — multi-select (checkbox list) from `DefaSursaSector` | `GET /api/inregistrare/sursasector` |
| 5 | **ClsfF** tree and **ClsfE** tree, with checkboxes | `GET /api/inregistrare/clasificatii?tip=F\|E` |
| 6 | **An** (combobox: current year, current-1) + summary → «Trimite cererea» | `POST /api/inregistrare/cerere` |
| 7 | «Cererea a fost trimisă. Veți primi un e-mail după aprobare.» | — |

Steps 2–6 are only reachable with the registration token from step 2; every call after it carries
the token.

### 4.1 Tree building (client side, from the flat list)

For each code `c` (ClsfF or ClsfE):

- `Right(c,4) = "0000"` → root
- else `Right(c,2) = "00"` → level 1, parent = `Left(c, Len-4) & "0000"`
- else → leaf, parent = `Left(c, Len-2) & "00"`; parent missing → under the root (D14); root
  missing too → ignored

A level-1 node without children is valid and **selectable as a leaf**. Checking a parent checks its
children (tri-state). What is sent to the server = the checked **leaves only** (level-1 nodes with no
children count as leaves).

---

## 5. Server — new blueprint `routes/inregistrare/`

All pre-auth routes: `LIMITER` per IP, parameterized SQL only, reason-coded JSON errors with literal
diacritics, no swallowed exceptions.

### 5.1 Pre-auth store

Notes on the session store (F5): key = opaque registration token, `name = "register"`, value =
`{email, cf, code_hash, attempts, verified, anaf}`, TTL 30 min. Memory and Redis behave the same.

### 5.2 ANAF proxy — `POST /api/inregistrare/anaf {cf}`

- Server-side call (a browser cannot call ANAF: CORS). Body
  `[{"cui": <cf>, "data": "<yyyy-mm-dd>"}]` to `https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva`,
  exactly as `InformatiiFirmaOnline2`. Stdlib `urllib` (F10).
- `cf`: digits only (strip `RO`, spaces), validated before the call.
- Response starts with `<html>` → «Serverul ANAF a generat o eroare.» (502). `cod <> 200` or CF not
  found → «Codul fiscal nu a fost găsit în baza de date ANAF.» (404).
- Returns only the fields the page shows (name, address). UNVERIFIED: ANAF's current rate limit and
  field names in v9 — confirm on the first real call and write them in the worklog.
- CF already in `CAI` or `AVACONT_COMUN.Unitati` → refused (D11): «Pentru acest cod fiscal există
  deja o bază de date. Contactați-ne pentru acces.» Checked here AND again at `/cerere` and at
  approval.

### 5.3 E-mail verification — `/cod`, `/verifica`

Same rules as slice 0072 (6 digits, hash only, 10 min, ≤ 5 attempts, `compare_digest`). `/cod`
also refuses an e-mail that already exists as a MariaDB user (`mysql.user`) or in
`Unitati_Utilizatori`, with a message that does not reveal more than «adresa nu poate fi folosită».

### 5.4 DB name — `GET /api/inregistrare/nume`

`SSSS`: uppercase, diacritics folded (`Ș→S`, `Ț→T`, `Ă/Â→A`, `Î→I`), letters only, vowels
`AEIOU` removed, first 4 of what remains. Fewer than 4 consonants (D12) → the missing positions
are filled with the vowels, in the order they appear in the name. Fewer than 4 letters in total →
refused with a Romanian message. `nn`: the lowest free pair in `11..99` with no zero digit,
free meaning absent from both `information_schema.SCHEMATA` and `CAI.DbName`. The name is
**re-computed at approval time**; the page value is only a preview.

### 5.5 Request — `POST /api/inregistrare/cerere`

Validates everything again server-side (token verified, SS values exist in `DefaSursaSector`, every
code exists in `DefaClsfF`/`DefaClsfE` and is a leaf, An in range). Writes one row in a new table
`AVACONT_COMUN.FX_Inregistrari`:

`IdCerere` PK AI · `Email` · `CF` · `DenumireAnaf` · `Denumire` · `An` · `Payload` JSON
(SS list, F leaves, E leaves) · `Stare` (`InAsteptare` / `Aprobata` / `Respinsa` / `Esuata`) ·
`DbName` (filled at approval) · `Motiv` · `DataCerere` · `DataDecizie` · `Decis` (operator) ·
`IpAddress`.

Then e-mails the operator that a request is waiting (address from `config.py`).

### 5.6 Approval and provisioning (job + poll, like `schema_sync`)

Runs under a **separate provisioning account** in `config.py` (`CREATE`, `CREATE USER`,
`GRANT OPTION` — never the normal service account). Every step registers its compensation; any
failure unwinds in reverse order, sets `Stare = Esuata` + `Motiv`, and reports it.

| # | Step | Compensation |
|---|---|---|
| 1 | Re-compute `DbName`; refuse if taken | — |
| 2 | Create the DB: `CREATE DATABASE` + clone every table and view of `AVACONT_SURSA` (D22) | `DROP DATABASE` |
| 3 | `IdUnitate` = `MAX(CAI.IdUnitate)+1`, one per SS, under `GET_LOCK('cai_idunitate')` | — |
| 4 | `INSERT` `CAI` rows (`DbName` = `DC` = new name, `Sursa` = SS, `CF`, `NumeUnitate`, `AnDate`) | `DELETE` those `IdUnitate` |
| 4a | `INSERT` `AVACONT_COMUN.Unitati (DC, NumeUnitate, CF)` and `Unitati_Ani (DC, AN, SS, CodProgram)` per SS (D23). `CodProgram` = what the operator left in the approval page's field, prefilled per sector (D25) | `DELETE` those rows |
| 5 | `INSERT` unit-DB `Unitati` rows (same `IdUnitate`, `SursaSector`, `Detalii`, `An`) | dropped with the DB |
| 6 | `INSERT` `Clasificatii` rows (§6) in one transaction | dropped with the DB |
| 7 | `CREATE USER '<email>'@'%' IDENTIFIED VIA mysql_native_password` with a random unusable password; `GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON \`<db>\`.*` only (D18) — never the SU rights today's accounts carry (D26, §13); row in `Unitati_Utilizatori (UN, DC, Rol='Contabil')` (D23/D24) | `DROP USER` + delete the row |
| 8 | E-mail the user a one-time link (24 h) to set the password → `SET PASSWORD` | — |

---

## 6. `Clasificatii` rows

### 6.1 Which rows

Every checked F leaf × every checked E leaf × every chosen SS (D5). The count is shown on the
summary screen before «Trimite cererea», and on the approval page — it can be large
(e.g. 20 × 40 × 2 = 1 600 rows). Inserted with `executemany` in one transaction.

### 6.2 `Capitol` (D5)

`Left(ClsfF,2) & "." & Left(SS,2)`, except `SS = "02E"` → `Left(ClsfF,2) & ".10"`. After §11
`Sursa` is written explicitly (`Right(SS,1)`), so any of the 14 SS values round-trips.

### 6.3 `Subcapitol`, `Articol`, `Alineat`, `Denumire`

Split per §2a. Before the INSERT, every derived row is checked in memory against
`DefaArticol`, `DefaClsfF`, `DefaTitlu`, `DefaSursaSector` (the same gate as the Migrator's
`Verifier.CheckDictionary`), so a mismatch fails with a Romanian message, not a raw `1452`.
`Denumire` = `DefaClsfE.Denumire`, trimmed, cut to 255. `IdClsfAcc` = 0.

---

## 7. Operator side

An operator page (D10), behind operator login, on the same site:

- **List** of requests, filterable by `Stare`, newest first.
- **Detail** of one request: CF + the ANAF data as fetched, the typed name, e-mail, An, the
  proposed DB name (recomputed live), the SS list, the checked F and E leaves with captions, the
  resulting `Clasificatii` row count, IP and dates.
- «Aprobă» → starts the §5.6 job, shows its progress live, then the final state.
- «Respinge» → reason required, e-mailed to the requester.
- A failed job (`Esuata`) shows `Motiv` and can be retried after the cause is fixed.

UNVERIFIED: how operator login is gated for a web page — `auth.py` offers bearer sessions only;
an operator allow-list in `config.py` (e-mails) checked on top of `require_session` is the
candidate. Decide in 0075-05.

## 8. Passes

| Pass | Contents |
|---|---|
| 0075-00 | `Clasificatii.Sursa` migration script (§11) + Migrator flow (§12) — before anything else |
| 0075-01 | Pre-auth notes, ANAF proxy, `/cod`, `/verifica`, pytest |
| 0075-02 | Nomenclator endpoints, name algorithm, `FX_Inregistrari` DDL, `/cerere`, pytest; refresh `sql/avacont_comun_login.sql` |
| 0075-03 | Provisioning job with compensation; one pytest per failing step proving the unwind |
| 0075-04 | Public page with `JS_COMPONENTS` (tree checkbox mode) |
| 0075-05 | Operator approval UI (incl. the editable `CodProgram` fields, D25); runbook for the provisioning account + SMTP on the VPS |
| 0075-06 | **Least privilege for the accounts that already exist** (§13, D26) — separate, one account at a time, after the grants are read |

## 9. Verification checklist

1. ANAF: valid CF, unknown CF, ANAF html error — three distinct messages.
2. Code: wrong ×5 blocks; expired refused; reused refused.
3. Name: diacritics fold, collision increments `nn`, `nn` never contains 0.
4. Tree: roots, level-1 without children selectable, orphan under its root, orphan without root ignored.
5. Server refuses a tampered request (non-leaf code, unknown SS, An out of range).
6. A failure injected at each of steps 2–7 leaves **nothing** behind (no DB, no user, no `CAI` /
   `Unitati` / `Unitati_Ani` / `Unitati_Utilizatori` rows).
7. The new user logs in through `LoginForm` and sees exactly the new unit(s), role `Contabil`,
   and `SHOW GRANTS` for them names **only** the new database.
8. Build clean; pytest all green or cleanly skipped.

## 10. Closed questions

Q1–Q11 closed 22.09.2026 → D14–D19 above.

## 11. Pass 0075-00 — `Sector`, `Sursa` and `SS` become written columns (runs FIRST)

> **Reworked 22.09.2026, after the server refused the first design.** It kept `SS` generated
> as `concat(<sector CASE>, Sursa)`. MariaDB 10.11 rejects that with **error 1901** —
> reproduced in a FRESH table, so it is the expression shape, not the `ALTER`: *«Function or
> expression 'concat(case right(coalesce(`Capitol`,''),2) … ,`Sursa`)' cannot be used in the
> GENERATED ALWAYS AS clause of `SS`».* A stored generated column here cannot be built out of
> another column, so `SS` must be written — and `Sector`/`Sursa`, its two halves, follow.

**Which columns, and which stay.** The eight generated columns are two different kinds of
thing. Six (`Clsf`, `Titlu`, `ClsfSal`, `ClsfF`, `ClsfE`, `ClsfX`) are pure functions of
`Capitol/Subcapitol/Articol/Alineat`, which every writer already supplies, and two of them
carry foreign keys of their own. Generated, they **cannot** disagree with the base columns;
written, an `UPDATE` that changes `Capitol` and forgets `ClsfF` yields a row that passes every
foreign key and still lies. They stay generated. Only the three that genuinely cannot be
computed — the source letter was never in the capitol — become written (decision 22.09.2026).

**Why not a plain `MODIFY`:** MySQL documents that a STORED generated column turned into a
normal one keeps its values; for MariaDB 10.11 this is **UNVERIFIED**. The pass does not rely
on it: it copies the values into plain columns first, so nothing depends on how the engine
treats the conversion.

Per database — `AVACONT_SURSA` first (the template), then every `NNN_*` in `SCHEMATA`:

1. `mysqldump` of `Clasificatii` to a dated file (backup, per DB).
2. Snapshot: `CREATE TABLE _snap_clsf AS SELECT IDClsf, Sector, Sursa, SS FROM Clasificatii`.
3. `ADD COLUMN Sector_w / Sursa_w / SS_w` (nullable) → `UPDATE … SET Sector_w = Sector, …`.
4. One `ALTER TABLE`: `DROP FOREIGN KEY Clasificatii__DefaSS`, `DROP KEY idx_SS`,
   `DROP COLUMN SS / Sursa / Sector`, then `CHANGE` each `*_w` into its real name —
   `Sector varchar(2) NOT NULL DEFAULT ''`, `Sursa char(1) NOT NULL DEFAULT 'A'`,
   `SS varchar(3) NOT NULL` — each `AFTER` the column it sat behind, then `ADD KEY idx_SS`
   and `ADD CONSTRAINT Clasificatii__DefaSS`. **Nothing generated is created**, so 1901
   cannot recur.
5. Verify against the snapshot: row count equal, and **zero rows** where `Sector`, `Sursa` or
   `SS` differ (`<=>`). Any difference → stop, report, restore that DB from step 1. Drop the
   snapshot only after it passes.

**`SS` has no default, on purpose.** A writer that omits it fails with `1364`; one that
invents a value fails with `1452`. Both loud. A default would let a wrong sector-source
through quietly, which is the failure this pass exists to prevent.

**The deployment window.** After step 4 an `INSERT` without `SS` fails; before it, an `INSERT`
*with* `SS` fails, because writing a generated column is an error. So the updated writers and
this migration go out in the same maintenance window; between them, writes to `Clasificatii`
fail and reads are unaffected. Those routes serve the Access/VBA sync, not continuous traffic
— pick a quiet moment.

Delivered as a standalone script with a dry-run mode that prints the DB list, row counts and
per-database state, **not** through `proc_SchemaDiff_DDL`. `--clean-leftovers` clears what a
failed run left behind, but only where all three columns are still generated — i.e. where
nothing structural happened. `sql/AVACONT_SURSA.sql` updated to match.

### 11.1 The writers

`SS` written means six existing write sites must pass it or break. They all have only the
capitol, so `PYTHON/routes/clasificatii_ss.py` holds the one rule they share —
`ss_values(capitol, sursa=None) → (Sector, Sursa, SS)` — which **reproduces the old generated
expression exactly** when called with a capitol alone. Those routes therefore behave as they
always did; only new code (this slice's provisioning, the Migrator reading Access `Sursa`)
passes a real source letter and reaches the other eleven `DefaSursaSector` values.

Touched: `routes/clasificatii.py` (two INSERTs and the upsert — which also gains the three
columns in its `ON DUPLICATE KEY UPDATE` list, because it can change `Capitol`) and
`routes/nomenclatoare.py` (one INSERT).

## 12. Migrator — `Clasificatii` flow

- `ColumnPlan` builds from `information_schema`, so once `Sursa` is writable it becomes a **name
  match** by itself (Access `Clasificatii` has a `Sursa` column). That is correct — but it must be
  deliberate, not accidental. And `SS` is worse than accidental: `NOT NULL`, with a foreign key,
  and **no Access column of that name at all**, so without an explicit mapping every INSERT would
  die with `1364`. All three are declared on the `Clasificatii` `TableMap`.
- Value rule: Access `Sursa` trimmed and uppercased (first character — the column is `char(1)`);
  empty/NULL → `A`, what the old generated column produced for every non-`10` capitol;
  `Capitol xx10` → `E` regardless of the file.
- `ClasificatieDerived`: `Sector` gets the extended CASE; `Sursa` returns the written value by the
  rule above; `SS = Sector & Sursa`. For these three the class stops being a *prediction* of the
  DDL and becomes the **source** of what gets written. The `Verifier` gate against
  `DefaSursaSector` keeps working unchanged on the new `SS`.
- One new `ColumnSourceKind.ClasificatieSursaSector` serves all three; the target column name
  picks which value. Its `AccessColumn` is «Sursa» for all of them, so `ColumnPlan` counts that
  Access column as consumed and the plain name match cannot claim it a second time.
- `TargetColumn.IsGenerated` docs and `MAPARE_NOMENCLATOARE.md` §3: nine generated columns → six.
- Tests: an Access row with Sursa `F` on capitol `xx01` lands as `01F`; a NULL Sursa lands as `A`;
  a `xx10` capitol lands as `E`; and the `TableMap` carries a mapping for each of the three.

## 13. Existing accounts carry SU rights — a job of its own

Stated by the operator on 22.09.2026: **every account on the K-BOT server today has SU
rights**, granted in a hurry. That is a live security problem, not a detail of this slice,
and it is deliberately NOT folded into the provisioning job:

- Provisioning (§5.6 step 7) must not copy today's grants. It gives exactly what §D18 says,
  on the new database only. That rule holds whatever the old accounts turn out to have.
- Fixing the old accounts is a **separate pass** (proposed **0075-06**), because it can lock
  a working operator out of a live system and must therefore be done with eyes on it, one
  account at a time, with the old grant recorded before it is replaced.

What it needs before a line is written, all from the server:

```sql
SELECT user, host, plugin FROM mysql.user ORDER BY user;
SHOW GRANTS FOR '<each account>'@'%';
SELECT UN, DC, Rol FROM AVACONT_COMUN.Unitati_Utilizatori ORDER BY UN;
```

### 13.1 What the grants actually say (read 22.09.2026 — VERIFIED)

The e-mail accounts are **not** SU, contrary to the first report. Each has `USAGE ON *.*`
globally — that is "may log in", no rights — plus privileges on exactly its own database:
`scavatarsoft`▸`000_DEMO`, `gradipp35`▸`006_GR35`, `dorraaa1977`▸`014_SCSV`,
`grigore_moisil2003`▸`027_SCGM`, `radubogdangeorge`▸`045_CTER`. That scoping is already right.

Three real problems remain, all narrower than "SU":

1. **`WITH GRANT OPTION` on each user's own database** — any of the five can hand their rights
   to any other account. Nothing needs it.
2. **DDL inside their database**: `CREATE`, `DROP`, `ALTER`, `REFERENCES`, `INDEX`,
   `CREATE VIEW`, `CREATE ROUTINE`, `ALTER ROUTINE`, `EVENT`, `TRIGGER`,
   `CREATE TEMPORARY TABLES`, `LOCK TABLES`. An accountant can drop their unit's tables.
   D18 wants `SELECT, INSERT, UPDATE, DELETE, EXECUTE`.
3. **The two real SU accounts**: `Admin`@`%` has `ALL PRIVILEGES ON *.* WITH GRANT OPTION`;
   `AVACONT`@`%` has `SUPER`, `FILE`, `SHUTDOWN`, `CREATE USER`, `RELOAD`, `PROCESS` and
   `GRANT OPTION` globally. `AVACONT` is the Flask service account and needs some of that —
   not `SHUTDOWN` or `FILE`.

Two findings worth their own line:

- **`scavatarsoft@gmail.com` and `AVACONT` share the same password hash**
  (`*DE9102A3…`). The operator's own login and the service account are one password.
- **`030_SCTC` and `050_GRSA` have no account at all** — nobody can log into those two units.
  Intended or an oversight, unanswered.

Shape of the fix: for each e-mail account, `REVOKE ALL PRIVILEGES, GRANT OPTION`, then
`GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE` on each database that account has a
`Unitati_Utilizatori` row for — plus whatever login itself needs, which `auth.py` decides (it
reads `AVACONT_COMUN` through the SERVICE account, so probably nothing). The provisioning
account of §5.6 is new and narrow. The legacy `NNN_XXXX_Contabil` / `_Administrator` accounts
are dropped once nothing uses them (D26) — check `Jurnal` and the FOREXE fleet first.

Every step is reversible from what was recorded, and one account is done and verified by a
real login before the next is touched.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- No swallowed exceptions — every catch/except surfaces or rethrows.
- Parameterized SQL only; identifiers (DB name) validated against the regex, then quoted.
- Code and comments in English; UI and operator messages in Romanian with literal diacritics.
- Commit each self-contained change on its own, with its worklog and `KBOT_STATUS.md` line.
- Never invent a fact. Mark VERIFIED vs UNVERIFIED.
- The developer has no access to the MariaDB server or the VPS; the operator runs every live test.
