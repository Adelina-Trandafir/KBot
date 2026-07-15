# HANDOFF — next step: the tree data API (`qFX_MAIN_TREE`)

> # ⚠️ SUPERSEDED — DO NOT BUILD FROM THIS FILE
>
> **Superseded 2026-07-15 by `docs/worklog/PLAN_TreeDataApi.md`; the work shipped as
> Slice 0008 (`docs/worklog/SLICE-0008-tree-data-api.md`).** Kept only as a record of how
> the contract was reached — several claims below are wrong. Specifically:
>
> - **"`Salarii` … is deprecated. Do not reintroduce it."** — wrong for the tree. It is a
>   real `FX_Angajamente` column and a real row-source column in `qFX_MAIN_TREE_DESCRIERE`.
>   The deprecation applies to the **angajamente list path** only (commit `22a2ec4`).
>   Slice 0008 restored it on the tree.
> - **`IDORD` / `First(FX_ORD.IDORD)`** — dropped entirely. The tree needs only `AreOrd`,
>   and the arbitrary-pick problem disappears with the column.
> - **The whole "scope by `db_name` / `id_unitate`" open question** — void. One MariaDB
>   database = one unit, so the connection is the scope; the endpoint takes neither. The
>   base comes from the session (`g.session.db_name`).
> - **`ArePartener` via `CODPARTENER`** — replaced by `FX_DDF.PartAng = 1`.
>
> The one thing below still worth reading is **"The trap that already cost one session"**:
> the three similarly-named queries are still not interchangeable. The correction is that
> `AngajamentTreeInfo` mirrors **two** of them — `_DESCRIERE` for the row-source columns
> and `qFX_MAIN_TREE` for the flags — not `qFX_MAIN_TREE` alone, which is exactly the
> mistake this file led the POCO into in Slice 0007.

> Written 2026-07-15 at the end of the Tier 1 / MainForm verification session.
> Everything below is marked **VERIFIED** (read from the real file this session) or
> **UNVERIFIED** (assumed, needs confirming). Do not promote an UNVERIFIED line to fact
> without reading the source.

## Where things stand

**VERIFIED — Phase A (Tier 1) is complete and pushed.** All of `KBOT_Tier1_Plan.md` is done:
the rate limiter is wired in both pre-auth endpoints, the gunicorn `on_starting` guard
refuses `workers > 1`, the server address is hardcoded HTTPS-only in `ApiOptions`, the
`KBOT_API_BASE_URL` override is gone, `AppConfig` is retired, `DownloadAction.ApiUrl` is
deleted, and `KBot.Forexe` still does not reference `KBot.Api`.

**VERIFIED — Phase B (`PLAN_MainForm_Scaffolding.md`) is already built.** Checked against
its own 11-item checklist: `KBotShellForm` base, `KBotNavList` with the six view keys,
lazy view creation in `CreateView`, `AngajamentTreeInfo` carrying selection state,
`cboAn`/`cboSs` from `/api/auth/periods`, `btnSinc` running ListaAngajamente through
`WithReauth`. No silent sample-tree fallback — `MainForm_Load` leaves the list empty and
logs a warning instead. The DI signature is `forexeRunner, session, apiClient, authApi,
loginFactory` (five params — the plan expected four; the real one wins).

**VERIFIED — baselines.** Solution builds clean, `Option Strict On`, zero warnings.
.NET: 80 tests green (`KBot.Api.Tests` = 26, `KBot.App.Tests` = 18, `KBot.Theming.Tests` =
27, `KBot.Common.Tests` = 7, `KBot.Domain.Tests` = 1, `KBot.LocalStore.Tests` = 1).
Python offline: 75 passed, 7 skipped, **zero fail/error** — keep it that way.

**VERIFIED — still in-process, do not raise the worker count.** Both `_upload_sessions`
(`routes/ftp.py`) and the rate limiter's counters (`routes/auth/ratelimit.py`) live in
process memory. `gunicorn.conf.py` stays at `workers = 1` until BOTH move to Redis.

## The next slice

Build the tree data endpoint + `MainForm.LoadTree` against real data. Today `LoadTree` has
no real source, so the six views have nothing to display. This is the unblocker.

### Step 0 — read before touching (mandatory)

- `src/KBot.Domain/AngajamentTreeInfo.vb` — the header now records per-field provenance.
  Read it first; it will save you the mistake below.
- `PYTHON/routes/forexe/angajamente.py` — the closest existing endpoint. Copy its shape:
  `@require_session`, `_validate_db_name`, `%s` placeholders, main + orphan branch, no
  swallowed exceptions.
- `src/KBot.App/MainForm.vb` — `LoadAngajamenteAsync` / `BuildTreeInfo` / `WithReauth`.
- `C:\AVACONT\FX_System_Export\QUERIES\qFX_MAIN_TREE.md` — the authoritative SQL.

### The trap that already cost one session

**VERIFIED:** three similarly named queries exist and they are NOT interchangeable.

| Query | Feeds | Has | Lacks |
|---|---|---|---|
| `qFX_MAIN_TREE` | `rcAngInd` in `frmFX_MAIN.RefreshTreeQuery` (`frmFX_MAIN.md:1200`) — **this POCO's contract** | `IDORD`, all nine `Are*` | `Salarii` |
| `qFX_MAIN_TREE_DESCRIERE` | tree NODE population, built by `mdl_FX_PopulareTree.Angajamente_SQL` | `Salarii`, `Surse`, `O` | `IDORD`, `Are*` |
| `qFX_MAIN_TREE_DATA` | same, "sort by date" variant | `Salarii`, `Surse`, `AlteDetalii` | `IDORD`, `Are*` |

Reading `_DESCRIERE`/`_DATA` and concluding things about `qFX_MAIN_TREE` produces a field
list that looks wrong in exactly the confusing way. `AngajamentTreeInfo` mirrors
`qFX_MAIN_TREE` and nothing else.

### `qFX_MAIN_TREE` columns (VERIFIED, read from the export)

`CodAngajament` (`A.CodAngajament`) · `IDDF` (`FX_DDF.IDDF`, joined) · `IDORD`
(`First(FX_ORD.IDORD)`, joined) · `DataCreare` · `DataDefinitivare` · `Descriere` ·
`Stare` · `Incarcat` · `Preluat` — the last six straight off `FX_Angajamente`.

Computed flags: `AreIndicatori`/`AreIstoric`/`AreRevizii`/`AreRezervari`/`AreReceptii`/
`ArePlati` = `CBool((SELECT Count(*) FROM <table> WHERE CodAngajament = A.CodAngajament)>0)`
over `FX_Indicatori` / `FX_Istoric` / `FX_DDF_REV_SA` / `FX_Rezervari` / `FX_Receptii_H` /
`FX_Plati`. `ArePartener` = `Not IsNull([CODPARTENER])`. `AreOrd` = `Nz([FX_ORD]![IDDF],0)<>0`.
`AreDDF` = `Nz([FX_DDF]![IDDF],0)<>0`.

Joins: `(FX_Angajamente A LEFT JOIN FX_DDF ON A.CodAngajament = FX_DDF.CodAngajament)
LEFT JOIN FX_ORD ON FX_DDF.IDDF = FX_ORD.IDDF`.

`Salarii` is **not** in this query and is **deprecated** — it was removed from the
angajamente list path entirely (commit `22a2ec4`). Do not reintroduce it.

### Access → MariaDB translation (needed; `angajamente.py` already shows the pattern)

- `CBool((SELECT Count(*) ...)>0)` → `EXISTS (SELECT 1 ...)` or `COUNT(*) > 0`.
- `Nz(x, 0) <> 0` → `COALESCE(x, 0) <> 0`.
- `Not IsNull(x)` → `x IS NOT NULL`.
- `First(FX_ORD.IDORD)` → **UNVERIFIED how to render.** Access `First` is arbitrary-pick
  under a `GROUP BY`, not "smallest". `MIN(FX_ORD.IDORD)` is the obvious analogue but is a
  behaviour change when an IDDF has several ORDs. Decide deliberately and write down why.
- `ConcatRelated(...)` → `GROUP_CONCAT(...)` (only if you also need `Surse`; `qFX_MAIN_TREE`
  does not have it).

### Open questions — resolve before/while building

1. **Scoping. VERIFIED: `qFX_MAIN_TREE` has no `WHERE` clause at all** — in Access it
   returns every angajament in the open database. `FX_Angajamente` **does** have a `DC`
   column (VERIFIED). The endpoint must scope by `db_name` the way `angajamente.py`'s
   orphan branch does (`FA.DC = %s`). **UNVERIFIED:** whether `DC` alone is right, or
   whether `id_unitate` should scope it too (the list endpoint takes both).
2. **`CODPARTENER` is unqualified in the query SQL. UNVERIFIED which table it binds to.**
   `FX_DDF` has a `CodPartener` column (VERIFIED); `FX_Angajamente` does not (VERIFIED).
   `FX_ORD` was not checked. Confirm before writing `ArePartener`.
3. **`AreOrd` reads `Nz([FX_ORD]![IDDF],0)<>0` — the ORD's IDDF, not its IDORD.** VERIFIED
   as what the SQL says. Looks intentional (the LEFT JOIN either matched or didn't), but
   worth a second look.
4. **Tree shape. UNVERIFIED.** `AngajamentTreeInfo` has `NodeKey`/`ParentKey`/`Caption` and
   `TipNod`/`CodIndicator`/`CodAi`/`IdPartener`, which in Access were set by `mcTree_Click`
   from the node level — not by the query. Read `mdl_FX_PopulareTree` for how nodes nest
   before deciding what the endpoint returns vs. what the client assembles.
5. **Is the export current? UNVERIFIED and untested.** Everything above describes
   `C:\AVACONT\FX_System_Export` as it stands today. Nobody has confirmed it matches the
   live Access file.

### Known gap worth folding in (optional)

`MainForm.vb` — `btnSort_Click` and `btnOpt_Click` are TODO stubs, and `btnIstoric_Click`
is a placeholder. Tree sort is a natural companion to the tree data, if scope allows.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- **Never invent a fact. Mark VERIFIED vs UNVERIFIED. Ask only below 75% confidence.**
  If a plan and the code disagree, the code wins — say so rather than following the plan
  into a false claim.
- No swallowed exceptions. Per `CLAUDE.md`: risky/boundary methods log + re-throw; UI
  boundaries log + swallow (they physically cannot re-throw).
- VB.NET: `Option Strict On`, no `Namespace` blocks, all controls in `*.Designer.vb`,
  colours only via `ThemeManager`/`KBotTheme`.
- Romanian comments and operator-facing strings with literal diacritics (ă â î ș ț), never
  `\uXXXX`; use «» inside string literals.
- Commit each self-contained change on its own. Do not sweep unrelated WIP into one blob.
- Python offline `pytest` must stay all-green-or-skipped, zero fail/error. Host-only tests
  must skip cleanly off-host (`config.py` is not on dev machines — and note `import config`
  can still *succeed* off-host because `test_redis_session_store.py` stubs a fake `config`
  into `sys.modules`, so gate on the actual attributes, not the module's presence).
