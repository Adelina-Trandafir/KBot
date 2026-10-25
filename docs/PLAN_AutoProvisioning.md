# PLAN_AutoProvisioning.md — self-service unit database creation (public web page)

**Slice 0075** (see `KBOT_STATUS.md`; the plan was drafted as 0073, which the Recorder slice already holds). Worklogs: `docs/worklog/SLICE-0075-0N-*.md`, one per pass. Read `CODE_WORKFLOW.md` and `CLAUDE.md` before starting.

Replaces the operator's manual Navicat work: a new unit fills in a public page, the operator approves with one click, and the server creates the database, the registry rows, the classifications and the MariaDB user.

Every fact below is marked **VERIFIED** (seen in the repo) or **UNVERIFIED** (to confirm in Step 0). If this plan and the code disagree, the code wins — say so, do not follow the plan into a false claim. **§0 (Step 0 findings, 22.09.2026) overrides every later section where they differ.**

---

## 0.0 START HERE — state on 22.09.2026, end of day

Historical checkpoint, updated 23.09.2026: passes 0075-00…05 were delivered. The provisioning job ran live on request 1 → 111_TRND after two fully unwound failures; the operator subsequently confirmed deployment of /operator, §9 check 7 and 0075-06. Individual rights changes and every possible undo path are not documented as exercised. The command line remains the fallback. The earlier “never run” and “stub only” statements in the stage records precede these confirmations.

Implementation history and its limits: [00 — classification columns](worklog/SLICE-0075-00-clasificatii-sursa-scrisa.md), [01 — registration/ANAF/e-mail](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-01), [02 — names/dictionaries/request](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-02), [03 — provisioning and live attempts](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-03), [04 — public page](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-04), [05 — operator approval](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-05). The journals also hold the former handoff details, examples, findings, deployment notes and unresolved questions.

Schema reference at this checkpoint: the operator dumped AVACONT_COMUN (23 tables), AVACONT_SURSA (44), 000_DEMO (43), on 22.09.2026 at 19:21 from 89.33.25.34:3306 / MariaDB 10.11.14. MariaDB_Schema is gitignored (.gitignore:507), available only on the operator’s disk; it describes shapes, whereas the old sql/*.sql files explain meanings and can have drifted. The settled shapes and template-only _w scaffolding are recorded in the journals, without reopening the dumps here. Why the template first refused remains unrecorded.

Before the first live run, the prerequisites were sql/0075_03_provizionare.sql and DB_CONFIG_PROVIZIONARE; config also needed ANAF_TVA_URL, ANAF_TIMEOUT and OPERATOR_EMAIL (defaults or nonfatal absence). URL v9 in §5.2 was confirmed by the operator on 22.09.2026. The table below keeps initial findings; later decisions D24–D29 and the linked stages override superseded role, tree, source-pair and execution descriptions.

---

## 0. Step 0 findings (22.09.2026) — read before anything else

Everything here was read from files in the repo or from the live server output the operator pasted. Decisions D20–D23 were taken by the operator the same day.

| # | Finding | Consequence |
|---|---|---|
| F1 | Slice 0073 is the Recorder slice; 0074 the Browser view | **D20:** this work is slice **0075** |
| F2 | `JS_COMPONENTS/treeview` is a single-select dropdown (`onSelect` fires once and closes); no checkboxes, no tri-state. `combobox` is single-select (`readonly` + `staticData` fits **An** only) | **D21:** add an opt-in checkbox mode to the tree (`checkable: true`, tri-state, result = checked leaves); **SectorSursa** = two single-select comboboxes (Sursa, then Sectorul) |
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

Still to obtain from the operator: `SHOW GRANTS` for one e-mail account and one legacy `_Contabil` account (step 7 of §5.6), the nginx `server` block, and the four missing JS pieces.

---

## 1. Decisions taken (operator, 22.09.2026)

| # | Decision |
|---|---|
| D1 | Public web page, built new, served by the Flask app. Uses the custom tree and combobox, vendored from `JS_COMPONENTS` into `PYTHON/static/js/` (see D21) |
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
| D14 | ~~A leaf whose `xx00` parent is missing hangs directly under its root `xx0000`~~ ▸ superseded by D28 (the middle level is always there); a leaf with no root name either is ignored — **still in force** |
| D15 | All 14 `DefaSursaSector` values must be storable → `Sursa` becomes a written column (§11) |
| D16 | A `DefaClsfE` row with NULL/empty `Denumire` is not shown in the tree |
| D17 | ~~`Utilizatori_Roluri(Email, DbName, Rol)`~~ — replaced by D23 (`Unitati_Utilizatori.Rol` already exists) |
| D18 | New users get only what their work needs, on their own DB — never global rights. Provisioning uses its own account, not `AVACONT` |
| D19 | Q11 shape approved; migration per §11, verified row by row; Migrator updated per §12 |
| D20 | Slice number **0075** |
| D21 | Tree: opt-in checkbox mode added to the vendored `PYTHON/static/js/components/treeview`; combobox for An. ~~SectorSursa = checkbox list~~ ▸ ~~two comboboxes, one pair~~ ▸ **two comboboxes + «Adaugă» onto a list, any number of pairs** (operator, 22.09.2026, D29) |
| D22 | DB creation: port of `admin.py::setup_database`'s clone loop, on the K-BOT server |
| D23 | Login rows: `Unitati`, `Unitati_Utilizatori (Rol)`, `Unitati_Ani` (per SS), plus `CAI` |
| D24 | **Role values are the Romanian words**, as already stored: `Contabil` (what D4 called `CO`), later `Administrator` (`AD`) and `Director` (`DR`). The 3 existing rows are **not** touched. One constant in the code, three values, no codes |
| D25 | **`Unitati_Ani.CodProgram` follows the sector**: `01` ▸ `0000002510`, `02` ▸ `0000000000`. Prefilled that way and **editable by the operator on the approval page**, per (An, SS) row, before the job runs. A sector outside 01/02 (now reachable, see D15) has no known value → prefilled `0000000000`, editable like the rest |
| D27 | **Unit name characters** (operator, 22.09.2026): only `[\w\s,]` — letters (diacritics included), digits, `_`, whitespace, commas; whitespace runs become one space. The page **removes** anything else, from the ANAF name too, before it is shown; `/nume` and `/cerere` **refuse** it (`DENUMIRE_CARACTERE_INTERZISE`) |
| D28 | **Trees are three levels of two digits** (operator, 22.09.2026), ClsfF and ClsfE alike: `650100` = `65` › `01`, `650101` = `65` › `01` › `01`. Any number of leaves can be ticked. **ClsfF: only leaves have a checkbox.** **ClsfE: the top level (title) has none; a level-2 node with children (article) has a tri-state box that ticks its whole branch.** What is sent is always the ticked leaves. Replaces the §4.1 rules below |
| D29 | **Many sector-sources per request** (operator, 22.09.2026): the pair picked in the two comboboxes is added to a list with «Adaugă»; each entry can be removed. A pair with no `DefaSursaSector` row is refused (at «Adaugă», and at «Continuă» for a pair picked but not yet added) |
| D26 | **The `NNN_XXXX_Contabil` / `_Administrator` accounts disappear.** Every user, at every level, is a MariaDB account named by their e-mail; the role is `Unitati_Utilizatori.Rol`, not the account name. **Separately: every account on the server today carries SU rights, which is wrong and has to be fixed** — its own job, see §13 |

---

## 2. What already exists (VERIFIED)

> **Both §2 and §2a are now second-hand.** `MariaDB_Schema/` (22.09.2026, 19:21) holds the
> real `SHOW CREATE TABLE` for all three schemas — read it instead of trusting any shape
> below. The lists here are kept because they explain what the columns MEAN; the dumps say
> what they ARE. Known divergences are marked inline.

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
  is stale; fix it in 0075-02. *(Done in 0075-02, and then corrected again against the dump:
  the server has **no index at all** on `IdUnitate` — the `ix_CAI_IdUnitate` that 0075-02 added
  to the repo DDL does not exist. `IdCai` AUTO_INCREMENT is at 96.)*
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

One HTML page + one JS module, served as static files by Flask, using the vendored tree (checkbox mode, D21) and combobox in `PYTHON/static/js/`. No external CDN.

Six screens, as built in 0075-04 — the operator moved **Denumire** onto screen 1 and turned **SectorSursa** into two comboboxes on 22.09.2026, after the first version was on screen:

| Step | Screen | Server call |
|---|---|---|
| 1 | **Date Unitate**: cod fiscal → «Caută» → ANAF data for confirmation, then **Denumire** (pre-filled from ANAF, editable). The proposed DB name is **not** shown; `/nume` is called on «Continuă» only for its verdict (the four-letter rule) | `POST /api/inregistrare/anaf`, `GET /api/inregistrare/nume?denumire=` |
| 2 | **E-mail** → «Trimite codul» → code → «Verifică» | `POST /api/inregistrare/cod`, `/verifica` |
| 3 | **Sursa** combobox, then **Sectorul** combobox filtered by it, «Adaugă» → the pair (one `SursaSector` code) goes on a list; any number of pairs (D29). A pair with no row in `DefaSursaSector` is refused | `GET /api/inregistrare/sursasector` |
| 4 | **ClsfF** tree and **ClsfE** tree, three two-digit levels, checkboxes on leaves only (D28) | `GET /api/inregistrare/clasificatii?tip=F\|E` |
| 5 | **An** (combobox: current year, current-1) + summary → «Trimite cererea» | `POST /api/inregistrare/cerere` |
| 6 | «Cererea a fost trimisă. Veți primi un e-mail după aprobare.» | — |

Steps 2–5 are only reachable with the registration token, which **step 1 hands out** on a successful ANAF lookup; every call after it carries the token in the `X-Registration-Token` header. *(This line used to say "from step 2" and contradicted §5.1, whose note holds `cf` and `anaf` — step-1 data. Settled by the operator, 22.09.2026, and implemented in 0075-01: the server must own the ANAF name, because the applicant may edit `Denumire` and §7 shows the two side by side.)*

### 4.1 Tree building (client side, from the flat list)

**Rewritten 22.09.2026 (D28).** Three levels of two digits, for ClsfF and ClsfE alike:

- `xx0000` → the **name** of level 1 `xx`; never a choice
- `xxyy00` → a **leaf** at level 2 when no `xxyyzz` exists; otherwise the **name** of group `xxyy`
- `xxyyzz` → a leaf at level 3, under `xx` › `yy`, always — the middle level is created even when
  no `xxyy00` row exists (it then shows its two digits alone)
- a level-1 group with no name at all is dropped with everything under it (D14, second half)

Group names come from the rows above or, for ClsfE, from `DefaTitlu` / `DefaArticol` (`grupuri` in the `/clasificatii` answer): the ClsfE query's join on `DefaArticol` drops every `xx0000` row, because `xx.00` is not an article — so under the old rules ClsfE had no roots and showed nothing.

**ClsfF: only leaves have a checkbox**; a node with children opens and closes. **ClsfE: a title (level 1) has no box and only opens; an article with children (level 2) has a tri-state box that ticks or clears its whole branch** (tree option `branchChecks`). What is sent = the ticked leaves — the same thing `cerere.py` calls selectable.

---

## 5. Server — new blueprint `routes/inregistrare/`

All pre-auth routes: `LIMITER` per IP, parameterized SQL only, reason-coded JSON errors with literal diacritics, no swallowed exceptions.

### 5.1 Pre-auth store

Notes on the session store (F5), name = register, key = the opaque token minted by /anaf (the correction under §4), value = {email, cf, code_hash, attempts, verified, anaf}, plain JSON types for Redis json.dumps. Memory and Redis share the contract. The 30-minute TTL is absolute from the ANAF lookup: expires_at is set once, every write restores only the remaining time. Implementation: [0075-01](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-01), routes/inregistrare/store.py.

### 5.2 ANAF proxy — `POST /api/inregistrare/anaf {cf}`

- Server-side call (a browser cannot call ANAF: CORS). Body
  `[{"cui": <cf>, "data": "<yyyy-mm-dd>"}]` to `https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva`,
  exactly as `InformatiiFirmaOnline2`. Stdlib `urllib` (F10).
- `cf`: digits only (strip `RO`, spaces), validated before the call.
- Response starts with `<html>` → «Serverul ANAF a generat o eroare.» (502). ~~`cod <> 200`~~ →
  **v9 has no `cod` field**; "not found" is `found` being empty (VERIFIED against a real answer,
  22.09.2026: the body carried exactly `found` and `notFound`). → «Codul fiscal nu a fost găsit în
  baza de date ANAF.» (404).
- Returns only the fields the page shows, read from `found[0].date_generale`: `denumire`, `adresa`,
  `cui`, `nrRegCom` (VERIFIED on the same answer). ANAF sends **cedilla** diacritics (`PLOIEŞTI`),
  not comma-below; nothing rewrites them. The URL above is CONFIRMED (operator, 22.09.2026).
  Still UNVERIFIED: ANAF's rate limit on v9.
- CF already in `CAI` or `AVACONT_COMUN.Unitati` → refused (D11): «Pentru acest cod fiscal există
  deja o bază de date. Contactați-ne pentru acces.» Checked here AND again at `/cerere` and at
  approval.

### 5.3 E-mail verification — /cod, /verifica

The 0072 rules apply: 6 digits, hash only, 10 minutes, ≤ 5 attempts, compare_digest. /cod refuses an address over 80 characters or already present in mysql.user or Unitati_Utilizatori, without revealing which list matched. A new code replaces the previous one; it cannot outlive the registration (expires_in reports the true duration), and changing the address clears verified. Machine reading on 22.09 confirmed the service account could read mysql.user (10 accounts); [0075-01](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-01) retains the proof and limits.

### 5.4 DB name — GET /api/inregistrare/nume

SSSS: uppercase letters, diacritics folded through unicodedata NFD (comma-below and cedilla), first four consonants; fewer consonants are filled by vowels AEIOU in name order, fewer than four letters total are refused in Romanian. nn is the lowest free pair in 11..99 with no zero digit, absent from both SCHEMATA and CAI.DbName. The implemented reading compares the number: 1nn is free only when neither list contains a name starting 1nn_, even with different letters. This interpretation was made in 0075-02; it was not an explicit operator decision.

/nume returns previzualizare: true. The original plan recomputed the final name on approval; 0075-05 subsequently allows the operator to edit it and submits the confirmed name, validated by the same shape/free-number rules (§7). [0075-02](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-02) retains the direct-call examples and interpretation details.

### 5.5 Request — `POST /api/inregistrare/cerere`

Validates everything again server-side (token verified, SS values exist in `DefaSursaSector`, every code exists in `DefaClsfF`/`DefaClsfE` and is a leaf, An in range). Writes one row in a new table `AVACONT_COMUN.FX_Inregistrari`:

`IdCerere` PK AI · `Email` · `CF` · `DenumireAnaf` · `Denumire` · `An` · `Payload` JSON (SS list, F leaves, E leaves) · `Stare` (`InAsteptare` / `Aprobata` / `Respinsa` / `Esuata`) · `DbName` (filled at approval) · `Motiv` · `DataCerere` · `DataDecizie` · `Decis` (operator) · `IpAddress`.

Then e-mails the operator that a request is waiting (address from `config.py`).

Implementation and rationale: [0075-02](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-02), cerere.py and sql/0075_fx_inregistrari.sql. The full dictionaries are read again at submission, including whether xxyy00 is a leaf (650500/590100 are legitimate unsplit cases), so disappeared codes are refused during the 30-minute window. FX_Inregistrari deliberately has no foreign keys: failed/rejected correspondence must survive cleanup of a unit. Missing OPERATOR_EMAIL or SMTP failure keeps the request, logs a warning and returns operator_anuntat: false. MAX_ROWS = 50,000 is an implementation ceiling added beyond the initial plan. Success discards the registration note, preventing a second request on the same proof.

### 5.6 Approval and provisioning (job + poll, like `schema_sync`)

The table is the initial plan. The implementation deviations below and [0075-03/05](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-03) govern the confirmed name, ALTER USER, mail failure, and the GRANT procedure introduced after the live failure; §13.1 records its definer dependency.

Runs under a **separate provisioning account** in `config.py` (`CREATE`, `CREATE USER`, `GRANT OPTION` — never the normal service account). Every step registers its compensation; any failure unwinds in reverse order, sets `Stare = Esuata` + `Motiv`, and reports it.

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

BUILT IN 0075-03 (`routes/inregistrare/provizionare.py`, `randuri.py`, `parola.py`, `scripts/aproba_cerere.py`, `sql/0075_03_provizionare.sql`; worklog [0075-03](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-03)). Where it differs from the table above:

- **Step 8 uses `ALTER USER`, not `SET PASSWORD`**, through the provisioning account —
  nobody knows the old (random) password, so the user cannot change their own. The link's
  token sits after `#` (never in a URL the server logs); only its SHA-256 is stored, on the
  request row (`ParolaHash`, `ParolaExpira` — two new columns).
- **The mail of step 8 unwinds nothing** when it fails: the unit is complete. The answer says
  `link_trimis: false`; `link_nou()` / `--link-nou` sends another.
- **Checks before step 1** (nothing created, request marked `Esuata` with the reason): e-mail
  usable as an account name and not taken, CF not taken (D11), name ≥ 4 letters, template
  without the `_w` columns and without triggers/routines/events (they are not cloned), every
  row passes the dictionaries (§6.3), ≤ `MAX_ROWS`, `CodProgram` valid (D25).
- **One run at a time server-wide** (`GET_LOCK('kbot_provizionare')`), which also keeps the
  command line and the future approval page from racing.
- The provisioning account's grants are written from the documentation, **not tried**: see
  the worklog for the two that are least certain.

---

## 6. `Clasificatii` rows

### 6.1 Which rows

Every checked F leaf × every checked E leaf × every chosen SS (D5). The count is shown on the summary screen before «Trimite cererea», and on the approval page — it can be large (e.g. 20 × 40 × 2 = 1 600 rows). Inserted with `executemany` in one transaction.

### 6.2 `Capitol` (D5)

`Left(ClsfF,2) & "." & Left(SS,2)`, except `SS = "02E"` → `Left(ClsfF,2) & ".10"`. After §11 `Sursa` is written explicitly (`Right(SS,1)`), so any of the 14 SS values round-trips.

### 6.3 `Subcapitol`, `Articol`, `Alineat`, `Denumire`

Split per §2a. Before the INSERT, every derived row is checked in memory against `DefaArticol`, `DefaClsfF`, `DefaTitlu`, `DefaSursaSector` (the same gate as the Migrator's `Verifier.CheckDictionary`), so a mismatch fails with a Romanian message, not a raw `1452`. `Denumire` = `DefaClsfE.Denumire`, trimmed, cut to 255. `IdClsfAcc` = 0.

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

~~UNVERIFIED: how operator login is gated~~ — DECIDED by the operator, 23.09.2026: the K-BOT password (a MariaDB login as the operator, like `LoginForm`), then a six-digit code mailed to the same address, and the address must be in `OPERATORI` in `config.py`. The session is a `STORE` note with its own header (`X-Operator-Token`), not the K-BOT bearer session.

BUILT IN 0075-05 (`routes/inregistrare/operator.py`, `static/operator.html`, `static/js/operator/`; worklog [0075-05](worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-05)). Beyond the list above: **the operator can also edit the database name** before approving (operator, 23.09.2026), held to the computed name's rules — shape `1nn_SSSS`, a number no other database uses, a database that does not exist. The page sends the name on screen, so the job builds exactly what was confirmed. The approval runs as a thread polled by the page; an in-memory registry, safe only under the single gunicorn worker.

## 8. Passes

| Pass | Contents |
|---|---|
| 0075-00 | `Clasificatii.Sursa` migration script (§11) + Migrator flow (§12) — before anything else |
| 0075-01 | Pre-auth notes, ANAF proxy, `/cod`, `/verifica` — **DONE (code only)**. ~~pytest~~: no test files, by operator decision of 22.09.2026. The same goes for every pass after this one |
| 0075-02 | Nomenclator endpoints, name algorithm, `FX_Inregistrari` DDL, `/cerere`; refresh `sql/avacont_comun_login.sql` — **DONE (code only)** |
| 0075-03 | Provisioning job with compensation — **DONE (code only), 23.09.2026**, plus the password link page (step 8) and a command line to approve until 0075-05 exists. ~~one pytest per failing step proving the unwind~~ — dropped with the rest of the test files. ⚠ Worth knowing what that costs: this is the pass that CREATEs and DROPs databases and MariaDB accounts, and the unwind is the only thing between a half-failed run and a half-built unit. With no test behind it, every compensation path is first exercised on the live server |
| 0075-04 | Public page, components vendored to `PYTHON/static/js/` (tree checkbox mode) — **DONE (code only)**, taken ahead of 0075-03 by the operator's choice of 22.09.2026. Verified in a browser against a stub, never against live Flask/MariaDB/ANAF. §4 now describes what was built: six screens, Denumire on screen 1, sursă-sector list (D29), three-level trees with leaf-only boxes (D28), name characters (D27) |
| 0075-05 | Operator approval UI (incl. the editable `CodProgram` fields, D25, and the editable database name) — **DONE (code only), 23.09.2026**, verified in a browser against a stub. ~~runbook for the provisioning account + SMTP~~: the provisioning account was set up live during 0075-03, recorded in its worklog |
| 0075-06 | **Least privilege for the accounts that already exist** (§13, D26) — separate, one account at a time, after the grants are read — **DONE by the operator on the server, 23.09.2026** (what was changed is not recorded here) |

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
8. Build clean. ~~pytest all green or cleanly skipped~~ — there are no test files (operator
   decision, 22.09.2026), so every line of this checklist is a **hand** check on a live
   server. This was initially an unexecuted checklist. The 23.09 operator confirmation in §0.0 explicitly closes deployment and check 7; outcomes of the other checks are not recorded individually.

## 10. Closed questions

Q1–Q11 closed 22.09.2026 → D14–D19 above.

## 11. Pass 0075-00 — `Sector`, `Sursa` and `SS` become written columns (runs FIRST)

> **Reworked 22.09.2026, after the server refused the first design.** It kept `SS` generated
> as `concat(<sector CASE>, Sursa)`. MariaDB 10.11 rejects that with **error 1901** —
> reproduced in a FRESH table, so it is the expression shape, not the `ALTER`: *«Function or
> expression 'concat(case right(coalesce(`Capitol`,''),2) … ,`Sursa`)' cannot be used in the
> GENERATED ALWAYS AS clause of `SS`».* A stored generated column here cannot be built out of
> another column, so `SS` must be written — and `Sector`/`Sursa`, its two halves, follow.

**Which columns, and which stay.** The eight generated columns are two different kinds of thing. Six (`Clsf`, `Titlu`, `ClsfSal`, `ClsfF`, `ClsfE`, `ClsfX`) are pure functions of `Capitol/Subcapitol/Articol/Alineat`, which every writer already supplies, and two of them carry foreign keys of their own. Generated, they **cannot** disagree with the base columns; written, an `UPDATE` that changes `Capitol` and forgets `ClsfF` yields a row that passes every foreign key and still lies. They stay generated. Only the three that genuinely cannot be computed — the source letter was never in the capitol — become written (decision 22.09.2026).

**Why not a plain `MODIFY`:** MySQL documents that a STORED generated column turned into a normal one keeps its values; for MariaDB 10.11 this is **UNVERIFIED**. The pass does not rely on it: it copies the values into plain columns first, so nothing depends on how the engine treats the conversion.

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

**`SS` has no default, on purpose.** A writer that omits it fails with `1364`; one that invents a value fails with `1452`. Both loud. A default would let a wrong sector-source through quietly, which is the failure this pass exists to prevent.

**The deployment window.** After step 4 an `INSERT` without `SS` fails; before it, an `INSERT` *with* `SS` fails, because writing a generated column is an error. So the updated writers and this migration go out in the same maintenance window; between them, writes to `Clasificatii` fail and reads are unaffected. Those routes serve the Access/VBA sync, not continuous traffic — pick a quiet moment.

Delivered as a standalone script with a dry-run mode that prints the DB list, row counts and per-database state, **not** through `proc_SchemaDiff_DDL`. `--clean-leftovers` clears what a failed run left behind, but only where all three columns are still generated — i.e. where nothing structural happened. `sql/AVACONT_SURSA.sql` updated to match.

### 11.1 The writers

`SS` written means six existing write sites must pass it or break. They all have only the capitol, so `PYTHON/routes/clasificatii_ss.py` holds the one rule they share — `ss_values(capitol, sursa=None) → (Sector, Sursa, SS)` — which **reproduces the old generated expression exactly** when called with a capitol alone. Those routes therefore behave as they always did; only new code (this slice's provisioning, the Migrator reading Access `Sursa`) passes a real source letter and reaches the other eleven `DefaSursaSector` values.

Touched: `routes/clasificatii.py` (two INSERTs and the upsert — which also gains the three columns in its `ON DUPLICATE KEY UPDATE` list, because it can change `Capitol`) and `routes/nomenclatoare.py` (one INSERT).

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

## 13. Existing-account privileges — initial report, corrected in §13.1

The initial report of SU rights on every account was disproved by the grant reading in §13.1. Provisioning must still apply D18 to new accounts, never copy existing grants. Existing-account repair was separate 0075-06, one account at a time with grants recorded before replacement and a real login checked before the next account; §0.0 records the operator's completion on 23.09, without individual results.

Historical read-only inputs: SELECT user, host, plugin FROM mysql.user ORDER BY user; SHOW GRANTS FOR each account@%; SELECT UN, DC, Rol FROM AVACONT_COMUN.Unitati_Utilizatori ORDER BY UN. The findings and the definer dependency below remain relevant; no server command is authorized or executed by this editorial change.

### 13.1 What the grants actually say (read 22.09.2026 — VERIFIED)

The e-mail accounts are **not** SU, contrary to the first report. Each has `USAGE ON *.*` globally — that is "may log in", no rights — plus privileges on exactly its own database: `scavatarsoft`▸`000_DEMO`, `gradipp35`▸`006_GR35`, `dorraaa1977`▸`014_SCSV`, `grigore_moisil2003`▸`027_SCGM`, `radubogdangeorge`▸`045_CTER`. That scoping is already right.

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

Shape of the fix: for each e-mail account, `REVOKE ALL PRIVILEGES, GRANT OPTION`, then `GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE` on each database that account has a `Unitati_Utilizatori` row for — plus whatever login itself needs, which `auth.py` decides (it reads `AVACONT_COMUN` through the SERVICE account, so probably nothing). The provisioning account of §5.6 is new and narrow. The legacy `NNN_XXXX_Contabil` / `_Administrator` accounts are dropped once nothing uses them (D26) — check `Jurnal` and the FOREXE fleet first.

The original repair procedure required each step to be recoverable from recorded grants and one account verified by real login before the next. Completion does not document the individual actions here.

⚠ **Do not break provisioning while doing this (added 23.09.2026).** Step 7 of §5.6 hands a new user their rights through `AVACONT_COMUN.proc_Provizionare_Grant`, a `SQL SECURITY DEFINER` procedure whose **definer is `Admin`@`%`** — it runs with Admin's rights, and needs a definer holding global privileges `WITH GRANT OPTION` (see `sql/0075_03_02_grant_procedura.sql` for why a narrower grant cannot work). If this pass drops `Admin` or trims its global grants, recreate the procedure first under an account that keeps them, or every approval fails at step 7.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- No swallowed exceptions — every catch/except surfaces or rethrows.
- Parameterized SQL only; identifiers (DB name) validated against the regex, then quoted.
- Code and comments in English; UI and operator messages in Romanian with literal diacritics.
- Commit each self-contained change on its own, with its worklog and `KBOT_STATUS.md` line.
- Never invent a fact. Mark VERIFIED vs UNVERIFIED.
- The developer has no access to the MariaDB server or the VPS; the operator runs every live test.
