# PLAN — Tree data API (`qFX_MAIN_TREE`) — Slice 0008

> **STATUS: IMPLEMENTED (2026-07-15).** See `SLICE-0008-tree-data-api.md` for what was
> actually built, and for the two points where this plan was **wrong** and got corrected
> (the "query 1 `WHERE`" instruction, and gating via `.Visible`). This file is kept as the
> contract of record; where it and the worklog disagree, **the worklog wins**.
>
> This plan supersedes `docs/HANDOFF_TreeDataApi.md`. It was written in chat and had no
> on-disk copy until slice 0008 saved it here — the same losing-the-doc problem the
> "Definition of done" section below warns about.

## What this slice delivers

A Flask endpoint that returns the angajamente tree for the connected database, and
`MainForm.LoadTree` that binds it into `AdvancedTreeControl` — the data that unblocks all
six/nine views. Plus the corrected `AngajamentTreeInfo` POCO the tree binds to.

## The data model (settled — do not relitigate)

- **No unit dimension.** In MariaDB one database = one unit. The Access `IdUnitate`,
  the `tCAI` join, and `GROUP BY IdUnitate` are dropped entirely. There is no unit filter
  and no DC filter — the connection IS the scope.
- The only dimensions that vary inside the database are **SS** (sector-source) and **An**
  (year). Both are MainForm runtime selectors, and the tree refreshes when either changes.
- One tree row per `CodAngajament`.
- **`IDDF` is one-to-at-most-one with `CodAngajament`.** An angajament CAN exist with no
  `FX_DDF` row (before its DDF is created), so `IDDF` is nullable → `LEFT JOIN FX_DDF`.
- **`IDORD` (the value) is dropped.** The tree never surfaces a specific IDORD. Only
  `CodAngajament` and `IDDF` are needed downstream.

## Columns returned per row

`CodAngajament` (key), `IDDF` (nullable), `Descriere`, `Stare`, `DataCreare`,
`DataDefinitivare`, `Incarcat`, `Preluat`, `Salarii`, `ASCUNS`, `Surse` (the concatenated
SS list for that angajament, display only).

## Tab-visibility flags (nine booleans, each an EXISTS)

Each flag answers "does this angajament have any …" and gates exactly one MainForm tab.
Implement each as a correlated `EXISTS (SELECT 1 FROM … WHERE CodAngajament =
a.CodAngajament)` — MariaDB best practice, NOT the Access `COUNT(*)>0` / `CBool` / `Nz`
pattern.

| Flag | Source | Gates tab |
|------|--------|-----------|
| `AreIndicatori` | EXISTS FX_Indicatori | Indicatori |
| `AreIstoric` | EXISTS FX_Istoric | Istoric |
| `AreRevizii` | EXISTS FX_DDF_REV_SA | Revizii |
| `AreRezervari` | EXISTS FX_Rezervari | Rezervări |
| `ArePartener` | `FX_DDF.PartAng = 1` (from the LEFT JOINed FX_DDF row; false/NULL when no DDF) | Partener |
| `AreDDF` | `IDDF IS NOT NULL` (already have it — no separate subquery) | DDF |
| `AreOrd` | EXISTS FX_ORD via the angajament's DDF (FX_ORD.IDDF = FX_DDF.IDDF) | Ordonanțări |
| `AreReceptii` | EXISTS FX_Receptii_H | Recepții |
| `ArePlati` | EXISTS FX_Plati | Plăți |

`ArePartener` is read from `FX_DDF.PartAng` (1 = has partener, 0 = none), NOT from any
`CODPARTENER` column — `CODPARTENER` was an Access-era field and does not apply. The tree
flag is only the yes/no. Resolving *which* partener (via `IdPartener` in the `Parteneri`
table, matched on `FX_DDF.CodFiscal` = `Parteneri.CodFiscal` when `CodFiscal` is not null)
belongs to the Partener tab's detail load in a later slice — it is NOT part of this tree
endpoint.

## Filters (the WHERE clause)

- **An:** `(YEAR(a.DataCreare) = :an OR a.DataCreare IS NULL)`. Rows with NULL `DataCreare`
  are ALWAYS shown regardless of `:an` — they aren't downloaded from FOREXE yet.
- **SS:** `EXISTS (SELECT 1 FROM FX_Indicatori i WHERE i.CodAngajament = a.CodAngajament
  AND i.SS = :ss)`. (`Surse` stays the full SS list for display; this filter narrows which
  angajamente appear.)
- **Hidden:** `(:include_hidden = 1 OR a.ASCUNS = 0)`. Default excludes `ASCUNS <> 0`;
  a `btnOpt` option on MainForm flips `:include_hidden` to show them.
- **State:** carry forward the Access exclusion — angajamente whose `Stare` contains
  'Anulat' or 'Suspendat' are excluded. Confirm the exact predicate against query 1's
  `WHERE` in Step 0 and translate it (`Stare NOT LIKE '%Anulat%' AND Stare NOT LIKE
  '%Suspendat%'`, case-sensitivity per the collation).

## Step 0 — read before writing (mandatory)

- The real `qFX_MAIN_TREE` row-source SQL and the flags SQL (both are in this chat's
  history; confirm they match the live Access export, which the operator says they do).
- The MariaDB schema of every table referenced: `FX_Angajamente` (esp. `DataCreare`,
  `DataDefinitivare`, `Salarii`, `ASCUNS`, `Stare`, and where `CODPARTENER` lives),
  `FX_DDF`, `FX_ORD`, `FX_Indicatori` (esp. `SS`), `FX_Istoric`, `FX_DDF_REV_SA`,
  `FX_Rezervari`, `FX_Receptii_H`, `FX_Plati`.
- The existing `angajamente.py` route — it required `id_unitate`; that parameter is
  removed here. See what else it did so nothing needed is lost.
- The current `AngajamentTreeInfo.vb` — it has the wrong shape (see Corrections). Read it
  before rewriting.
- `MainForm` as built in Slice 0006 — the tree host, the SS/An selectors, and the
  preserved `WithReauth(Of T)` wrapper the tree load goes through.
- How `Surse` is currently produced server-side (the Access `ConcatRelated('SS',
  'FX_Indicatori', …)`). In MariaDB use `GROUP_CONCAT(DISTINCT i.SS SEPARATOR ';')` — but
  confirm whether it's already implemented somewhere before rewriting it.

Report what each read showed before editing.

## Server side (Python / Flask)

- New or reworked endpoint (e.g. `GET /api/forexe/tree`) guarded by `@require_session`
  (`g.session`). Parameters: `ss`, `an`, `include_hidden`. No `id_unitate`.
- Parameterized query only — never string-concatenate `ss`/`an`/`include_hidden` into SQL.
- Build the row list with the columns above + the nine `EXISTS` flags. Return JSON with
  `ensure_ascii=False` (diacritics in `Descriere`/`Stare` must be literal UTF-8).
- No swallowed exceptions — surface/rethrow. A DB error returns a reason-coded error body,
  not an empty tree.
- Query cost is not a concern: the tree is at most a few hundred rows, so nine correlated
  `EXISTS` per row is fine. Use `EXISTS` for clarity; no need for the LEFT JOIN/GROUP BY
  alternative.

## Client side (VB.NET)

- **Rewrite `AngajamentTreeInfo`** to the settled shape: the columns above + the nine
  flags. Drop `IdUnitate`. Restore `Salarii`. Remove `DataDefinitivare` only if Step 0
  shows it isn't returned — it IS in the column list, so keep it. Rewrite the provenance
  comment to state, per field, "from qFX_MAIN_TREE row-source", "from flags query", or
  "confirmed on MariaDB schema" — nothing labelled confirmed without a source.
- `MainForm.LoadTree(ss, an, includeHidden)`: calls the endpoint through the preserved
  `WithReauth(Of T)` wrapper, maps rows into `AngajamentTreeInfo`, binds the tree. A dead
  session surfaces the reason-coded Romanian message — NEVER a silent empty tree, NEVER
  the removed `DebugSampleAngajamente` fallback.
- Wire SS-change and An-change on MainForm to re-call `LoadTree` with the new values.
- Wire the `btnOpt` "show hidden" option to the `includeHidden` argument.
- On node selection, the bound `AngajamentTreeInfo` drives tab visibility: each `Are*`
  flag shows/hides its tab per the table above. `AreDDF`/the tab set already scaffolded in
  Slice 0006 — wire the flags to `.Visible`, don't rebuild the tabs.

## Corrections to earlier assumptions (so nobody re-introduces them)

- `Salarii` was wrongly REMOVED from the POCO in an earlier slice. It is a real
  row-source column. Restore it.
- The POCO previously carried `IDORD` and a `MIN/First` ORD pick. Drop `IDORD`; it's not
  needed and the arbitrary-pick problem disappears with it.
- The tree is NOT unit-scoped or DC-scoped in any parameter sense — there is no such
  dimension in MariaDB. Do not add an `id_unitate` or `db_name` filter argument.

## Verification checklist

1. Endpoint returns correct rows for a known angajament set, filtered by SS and An, with
   NULL-`DataCreare` rows always present.
2. `include_hidden` toggles `ASCUNS <> 0` rows in/out.
3. Anulat/Suspendat angajamente are excluded.
4. Each `Are*` flag is correct against a hand-checked angajament (one with DDF+ORD, one
   with neither, one with receptii but no plati, etc.).
5. Diacritics in `Descriere`/`Stare` render as literal UTF-8 end to end.
6. `MainForm` re-queries the tree when SS or An changes.
7. Tab visibility follows the flags on node selection.
8. A dead session shows the reason-coded message, not an empty tree.
9. Build clean, `Option Strict On`, zero warnings; .NET tests green (baseline 80);
   Python offline suite green or cleanly skipped, zero fail/error.
10. `AngajamentTreeInfo` provenance comment has a real source per field.

## Definition of done (mandatory — no exceptions)

Not complete until: code builds/tests clean AND a worklog exists at
`docs/worklog/SLICE-0008-tree-data-api.md` (what changed and why · files touched · test
results · anything left unverified or deferred) AND every existing on-disk `.md` doc is
reconciled with reality AND all are committed and pushed together. If any is missing,
report the task as unfinished.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- No swallowed exceptions anywhere — every catch/except surfaces or rethrows.
- VB.NET: `Option Strict On`, no `Namespace` blocks, all controls in `*.Designer.vb`,
  colours only via `KBotTheme`.
- Parameterized SQL only. Code and comments in English; operator messages in Romanian with
  literal diacritics.
- Commit each self-contained change on its own.
- Never invent a fact. Mark verified vs. assumed. Ask only below 75% confidence.
