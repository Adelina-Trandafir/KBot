# K-BOT — STATUS (single source of truth)

This file is the one place that says what is done, what is in progress, and what is next.
Three people rely on it: the developer, Claude (chat), and Code (Claude Code / Codex).
Keep it current. If reality and this file disagree, fix this file in the same commit.

**Rule:** every task also produces a worklog in `docs/worklog/` (see
`docs/worklog/CODE_WORKFLOW.md`). This STATUS file is the summary; the worklog is the detail.

---

## Slice registry

Numbers are permanent once assigned. A slice done in several passes keeps one number and
gets sub-numbered worklogs (`SLICE-0007-01-…`, `SLICE-0007-02-…`). The **next free slice
number** is recorded at the bottom of this section — bump it when you assign a new one.

> The numbers below are a starting proposal. Confirm or renumber them once, then treat
> them as fixed. Felia 1 (auth) is the only one anchored by history; the rest are assigned
> here for the first time.

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0001 | Auth — bearer tokens, session store (Felia 1) | DONE | (pre-worklog-rule) | Static API key eliminated for K-BOT; legacy FOREXE still uses X-Api-Key |
| 0002 | Split-brain 401 fix + reason codes | DONE | (pre-worklog-rule) | login mints via STORE; every 401 carries a reason code |
| 0003 | Redis session backend | DONE (code) / config PENDING on VPS | — | `SESSION_BACKEND="redis"`, `SESSION_KEY_PREFIX`, `REDIS_DB=2` must be set in host config.py; verify DB 2 is free |
| 0004 | Tier 1 hardening | IN PROGRESS | — | rate limiter DONE+pushed; remaining: ApiOptions address, retire AppConfig, verify gunicorn guard. Plan: `KBOT_Tier1_Plan.md` |
| 0005 | Phase A cleanup (lying tests, https guard) | DONE | — | Python 75 passed / 7 skipped, 0 fail/error; .NET 80 green |
| 0006 | MainForm scaffolding | DONE | (pre-worklog-rule) | Plan: `PLAN_MainForm_Scaffolding.md`. Built against its own 11-item checklist; `WithReauth(Of T)` + ListaAngajamente vertical preserved. Real DI signature is 5 params (`forexeRunner, session, apiClient, authApi, loginFactory`), not the 4 the plan expected |
| 0007 | AngajamentTreeInfo POCO correction | SUPERSEDED by 0008 | `SLICE-0008-tree-data-api.md` | Was done WRONG: built against `qFX_MAIN_TREE` alone, so `Salarii` was dropped and `IDORD` kept. 0008 rewrote the POCO against the real contract (row-source `_DESCRIERE` + flags `qFX_MAIN_TREE`): `Salarii` restored, `IDORD` dropped |
| 0008 | Tree data API + `MainForm.LoadTree` | DONE (code) / UNVERIFIED on a live DB | `SLICE-0008-tree-data-api.md`, `SLICE-0009-maintree-loadtree.md` (Part A amendment) | Plan: `PLAN_TreeDataApi.md`. `GET /api/forexe/tree` (an/ss/include_hidden, base from session), nine `EXISTS` flags, POCO rewrite, tree load + nav gating. **Amended by 0009:** the SS filter now has an orphan escape (`EXISTS SS OR NOT EXISTS any indicators`) so zero-indicator angajamente stay visible. **No part of it has touched a real database** — all route tests are host-only and skip off-host |
| 0009 | `MainForm.LoadTree` (client half) + tree orphan escape | DONE (code) / UNVERIFIED on a live DB | `SLICE-0009-maintree-loadtree.md` | The brief's Parts B/C/D (DTOs, `GetTreeAsync`, `LoadTreeAsync` + gating) were **already shipped by 0008**; the real deltas are Part A (orphan escape on the server, see 0008 row) + its 2 host-only tests, and the 4 `GetTreeAsync` client tests 0008 never added (Api 26 → 30). Kept 0008's choices: mapping in the client (no `BuildTreeInfo`), token from session (no param), `IDDF As Long?` throughout. LoadTree is period-driven (runs on load + every An/SS change = the `SetPeriod` precondition) |
| 0010 | `KBotDataView` — owner-drawn unbound grid (Access continuous-form) | DONE (code) / **NO VISUAL VERDICT YET** | `SLICE-0010-01-…skeleton.md`, `-02-…virtualizare.md`, `-03-…tipuri-coloana.md`, `-04-…formatare-disable.md`, `-05-…input-selectie.md`, `-06-…editare.md`, `-07-…scroll-by-column.md` | Plan: pasted in-session (continuation plan supersedes the original where they disagree). Multi-pass. **0010-01 (skeleton):** models + enum, double-buffered `Control` implementing `IThemedControl`, theming cache + `ApplyTheme`, header + empty body, 4 child controls in Designer. **0010-02 (render+virtualize):** split into partials (`.Theming`/`.Layout`/`.Painting`), frozen + scrolling column bands, integer virtualization math, two-pass scrollbar sizing, Text + CheckBox cell painting, `CellFormatting`/`RowFormatting` plumbing with **reused** args, full palette→role mapping. **Decision:** kept in `KBot.Controls` + a `KBot.Theming` ProjectReference (no cycle) so it self-themes — the plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade). **0010-03 (column types):** Combo / OptionButton / Button / ProgressBar painting + `OptionGroup` exclusivity via `SetOptionValue`. **0010-04 (formatting+disable):** three-level effective-enabled (`IsCellEnabled`/`IsRowEnabled`, separate "probe" args so queries can't clobber an in-flight paint), disabled rendering across all six types, conditional formatting. **0010-05 (input+selection):** current cell + `SelectionChanged`, Access-style keyboard nav, click/double-click, toggle + button activation gated on `IsCellEnabled`, header-edge column resize. **0010-06 (editing):** floating Text/Combo editor, `CellValidating` (veto **and** value coercion), Esc discard, auto-commit on move/scroll, and the corrected `IsDirty` contract — API writes are *loading*, only operator edits/toggles dirty a row (`ClearDirty` added; 2 pass-01 tests deliberately rewritten). Virtualization proven **headlessly** (same painted-row count at 5,000 and 50,000 rows). 158 tests green. **Editing caught 2 real VB case-insensitivity bugs** where a parameter shadowed a same-named property and an unqualified assignment was a silent no-op: `ProposedValue` (commit always wrote Nothing) and — live since 0010-01 — `HeaderText`, meaning **every column header was Nothing**; both fixed with `Me.` + regression tests. ⚠️ **NOBODY HAS RUN THE VISUAL HARNESS for any of the six passes.** Scroll smoothness, the WinForms key→handler path, resize drag, floating-editor placement/focus and actual colours are all unverified — the blank-header bug shows exactly why that matters. Run: DevHarness → Controls/UI → «KBotDataView — virtualizare + temă (5.000 × 20)». **0010-07 (later add-on):** `ScrollByColumn` property — horizontal scroll snaps to column edges (direction-aware: a small step still advances a full column), covering thumb/arrows/wheel via one snap in the ValueChanged path; harness got a «Derulare pe coloană» checkbox. 7 tests. Next: Sumar slice consumes it read-only |

| 0011 | Sumar — endpoint + `SumarView` (prima vedere reală) | DONE (code) / **partially run on a live DB (0011-03)** / **NO VISUAL VERDICT** | `SLICE-0011-01-sumar-endpoint.md`, `SLICE-0011-02-sumar-view.md`, `SLICE-0011-03-sumar-join-clasificatii.md` | Plan: pasted in-session. Port of `qFX_MAIN_SUMAR` v1 → `GET /api/forexe/sumar?cod=…` (header hoisted once + one row per indicator), plus `SumarView` replacing `PlaceholderView` for the `sumar` key — **the first of the nine views that is real**, and the first consumer of `KBotDataView` (read-only). **The `Clsf` blocker was resolved by the operator supplying the real DDL, not by guessing:** `Clasificatii.Clsf` EXISTS as a STORED generated column `concat_ws('.',Capitol,Subcapitol,Articol,Alineat)` (indexed), so Branch A — select it directly. `Titlu = left(Articol,2)` explains Access's `Mid(Clsf,13,2)`. Port decisions: no SS filter, `ClasificatiiG`→`Clasificatii` / `ParteneriG`→`Parteneri`, all `IdUnitate` join predicates dropped, **`LEFT JOIN Clasificatii`** (was INNER — an indicator with no classification must still appear), `TotalReceptii = SUM(DIF)` not `Valoare`, `'Angajament nou.'` with the trailing period. Deliberate deviations: `COALESCE(...,0)` on the five totals, `cod` pushed into every aggregate (Access full-scanned per aggregate), deterministic `ORDER BY` added, `ROUND(,2)` kept on revizii/ordonanțări only. Client: `WithReauth` passed in specialized on `SumarInfo` so the 401 policy stays in the shell; snake_case stops at the wire DTOs; **stale-response guard** discards a superseded `cod`. 164 tests green, 0 warnings. **0011-03 (after the operator's live run) fixed three stacked defects in the query:** the join key WAS wrong (`FX_Indicatori.IdClsf` holds the Access id → matches `C.IdClsfAcc`, giving 0 rows against `C.IDClsf`); `IdUnitate` had been dropped from a SHARED nomenclator (67-row fan-out); and `Clasificatii` has real duplicates on `(IdClsfAcc, IdUnitate)` (still 50 rows with both predicates). Fix: `LEFT JOIN Clasificatii` → **scalar subquery with `LIMIT 1`**, `ORDER BY` moved to the output alias. **The same defect was found and fixed in `aggRev`/`Parteneri`, which the brief did not ask for** — there the join sat BEFORE the `GROUP BY`, so a duplicate partener multiplied `SUM(SA.ValCur)` and inflated `TotalRevizii` (a wrong money figure, not a blank column). 3 new tests incl. a general `len(rows) == indicator count` sentinel. ⚠️ **Still open:** (a) nobody has run `SumarView` on screen, and `KBotDataView`'s visual harness is STILL unrun; (b) the `Clasificatii` duplicates need triage — if the rows differ by `Sursa`/`Sector` then a join dimension is MISSING and `LIMIT 1` picks arbitrarily between two different classifications; if identical, they are data to clean |
| 0012 | Migrare Access → MariaDB — introspecție coloane pentru seed | 0012-01 DONE (code) / **NEVER RUN ON A LIVE DB** | `SLICE-0012-01-seed-columns.md` | `GET /api/forexe/seed/columns?db_name=&table=` în `routes/forexe/seed.py`, al treilea endpoint al fișierului. Gardă **`@require_api_key` (X-Api-Key, NU bearer)** ca celelalte două — seed-ul e condus de VBA/FOREXE legacy. `db_name` prin `_DBNAME_RE`, `table` prin aceeași `ALLOWED_TABLES`; niciun identificator din client nu ajunge în SQL fără allow-list. Întoarce **doar numele coloanelor, în ordinea din tabel** (`SHOW COLUMNS`), ca apelantul să construiască INSERT-uri pe poziție. **Tabel inexistent → `200` cu `columns: []`, NU 404** — apelantul trebuie să distingă «zero coloane» (migrarea nu a rulat) de o eroare de rețea/cheie. Ca să nu se adulmece errno 1146 dintr-o excepție, existența se testează întâi cu `SHOW TABLES LIKE %s` (parametrizat, nu aruncă) și abia apoi `SHOW COLUMNS`. Strict read-only: nicio scriere, niciun DDL, `conn.close()` în `finally`. 11 teste host-only. **Decizie blocată (varianta A, nedistructivă): `seed/schema` rămâne în cod dar utilitarul de migrare NU îl apelează** — tabelele `FX_` există deja în MariaDB cu DDL curat, iar `/schema` face `DROP TABLE IF EXISTS` înainte de `CREATE` din tipurile DAO (`LONGTEXT` pentru orice necunoscut), deci le-ar recrea mai slabe peste date reale. `/columns` există exact ca să nu fie nevoie de `/schema`. ⚠️ Neverificat: nimic nu a atins o bază reală; presupunerea că `FX_Rezervarii_IMG` NU e migrat pe `000_DEMO` (folosit ca tabel-lipsă în test) e a mea — testul se sare explicit dacă apare; contul de seed are nevoie de privilegiul `SHOW` pe baza țintă |

| 0013 | `KBotDataView` — auto-sizing coloane + moduri de umplere | DONE / **VISUALLY ACCEPTED** in harness playground | `SLICE-0013-column-sizing.md` | Plan: pasted in-session. Two grid-wide knobs run as ONE pass in `UpdateLayout` (before offsets/scrollbars): `AutoSizeColumnsMode` (`None`/`ToContent`, default `ToContent`) measures each visible column to `max(header, sampled cells) + padding` clamped to `[MinWidth, MaxWidth]`; `ColumnFillMode` (`None`/`FirstColumn`/`LastColumn`/`Proportional`, default `None`) then spends the leftover or absorbs the overflow so a fill mode never shows a horizontal scrollbar (except the honest `sum(MinWidth) > available` fallback). Model gains `MaxWidth` (default uncapped) + `Width` clamped `[Min,Max]` on every write + internal `UserSized` (drag pins a column; `ToContent` skips it, fill/shrink still applies; `ResetColumnSizing()` clears it). **New hard rule: all code comments added/touched this slice are in ENGLISH** (marked «English (slice 0013)»); the rest of `KBot.Controls` stays Romanian — no mass conversion. Measuring uses `TextRenderer.MeasureText` with the painter's fonts and the FORMATTED value (so `N2` money measures wide). Available width mirrors `UpdateScrollBars` exactly, vScroll visibility decided first (row-count only) → no circular dependency; `_inAutoLayout` re-entrancy guard. Limitations (in worklog): sampling (`AutoSizeSampleRows`, default 200, 0=all — a wider value further down ellipsizes), `CellFormatting` NOT raised while measuring (a handler that widens `Text` ellipsizes), the `MinWidth`-overflow fallback, and no column→grid back-reference (post-load column-property edits need an explicit `AutoSizeColumns()`; `AddColumn`/drag/resize/theme/`EndUpdate` cover the rest). 93 `KBot.Controls.Tests` green (15 new auto-size + 4 model), full solution green, 0 warnings. ⚠️ **Still unrun on screen** — like all of 0010/0011. Follow-up (separate): `SumarView` should adopt `ToContent` + `LastColumn`/`Proportional` and drop its hardcoded widths (Partener column removed separately) |

| 0014 | Rezervări — endpoint + `RezervariView` (a doua vedere reală) | DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | `SLICE-0014-rezervari-view.md` | Plan: pasted in-session. `GET /api/forexe/rezervari?cod=` (cititor brut, un rând per `FX_Rezervari`) + `RezervariView` (master/detail: arbore lună→(dată,tip) la stânga, grilă Clsf/CreditBug/Inițiale/Curentă/Definitive la dreapta) înlocuind `PlaceholderView` pentru `rezervari`. **Întrebarea deschisă a planului (§7.1, totalul lunar) e REZOLVATĂ din sursa Access, nu ghicită:** `qFX_REZERVARI_TREE` calculează `TOTALL = SUM(R_Valoare)` corelat pe (CodAngajament, lună, an) — candidatul (a). Valoarea frunzei = `SUM(IIf(EInitiala,R_Initiala,R_Valoare))` (Suma din `QFX_DDF_REZERVARI`); tipul derivat client-side (Inițială>Mărire>Micșorare). Clasificația: aceleași decizii ca Sumar 0011-03 — prin `FX_Indicatori` (join CodAI, `IdClsf`=id Access), subinterogare scalară `LIMIT 1` + predicat `IdUnitate` păstrat la nomenclator. `LEFT JOIN FX_Indicatori` (o rezervare nu dispare fără indicator). Iconițe GDI tematizabile (`RezervariIcons`), nu resurse binare. «+» = display-only (ridică `AdaugaDdfCerut`, fără abonat) — workflow-ul DDF (migration-plan item 7) e felie ulterioară. Plasa 401 + stale-guard ca la Sumar. Api 36→42 (+6 GetRezervari), App 22→30 (+8 view), 16 teste Python host-only (skip off-host). ⚠️ **Trei lucruri deschise:** (a) nimeni nu a văzut `RezervariView` pe ecran (+ harness-ul `KBotDataView` STILL unrun); (b) `test_forexe_rezervari.py` nerulat pe host — confirmă că cele opt tabele/coloane există și că join-ul prin `FX_Indicatori` dă Clsf populat; (c) cifrele exacte din screenshot (Ian 1.091.940 / …) NU au fost reproduse numeric (formula e din sursă, dar datele angajamentului lipsesc) |

| 0015 | Recepții — endpoint + `ReceptiiView` (a treia vedere reală) | 0015-01/02/03 DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | `SLICE-0015-01-receptii-view.md`, `SLICE-0015-02-receptii-root-tooltip.md`, `SLICE-0015-03-receptii-month-tree-revision.md` | Plan: `PLAN_ReceptiiView.md`. **0015-03 = revizuire operator (2026-07-22):** arbore pe **3 niveluri** (folder lună/an → recepție → antet, grupat pe DataR); LISTA **agregată la orice nivel** (click pe lună/recepție/antet → total Sum(DIF) + rând per clsf Sum(Valoare)); coloana „Descriere" = `Clasificatii.Denumire` (nou pe endpoint, drumul 0011-03) fiindcă agregă peste anteturi; **tooltip pe lună ȘI recepție** cu fereastra de plăți extinsă până la **prima recepție a lunii următoare** (nu ≤ MaxDataH). Api 48, App 39. `GET /api/forexe/receptii?cod=` (cititor brut: un rând per linie `FX_Receptii`, cu antet `H` + recepție `R` părinte, + array `plati` în același răspuns) + `ReceptiiView` (master/detail: arbore **2 niveluri** recepție→antet la stânga, grilă LISTA NrCrt/Descriere/Clsf/Valoare la dreapta) înlocuind `PlaceholderView` pentru `receptii`. **Cele trei furci ale planului sunt blocate din sursa Access, nu ghicite:** (1) arbore 2 niveluri (frunzele pe clsf din `Show_Receptii` sunt cod comentat — `AggregateRow`/`valAsoc` dormante); (2) LISTA condusă doar de antet (rând-total `Sum(DIF)` + rânduri per clsf `Sum(Valoare)`, din `qFX_MAIN_REC_LISTA_IND`); (3) un singur endpoint (nu tree/lista/tooltip separate). Clasificația: același drum ca Sumar 0011-03 (`FX_Indicatori`, `IdClsf`=id Access, subinterogare scalară `LIMIT 1` + `IdUnitate` păstrat la nomenclator, `LEFT JOIN` ca o linie să nu dispară). Iconițe GDI de stare (sus/jos/neutru, finding 5) tematizabile. Plasa 401 + stale-guard ca la Sumar/Rezervări. Api 42→48 (+6 GetReceptii), App 30→37 (+7 view), 15 teste Python host-only (skip off-host). **Devieri documentate:** chei de fir snake_case (nu PascalCase-ul schiței); coloana grilei „Descriere" = Descrierea ANTETULUI (`FX_Receptii_H.Descriere`, din `Nz([HH]![Descriere],…)`), nu a indicatorului; `sters_h` adăugat în rând pentru cumulul DIFH din tooltip (0015-02). ⚠️ **Deschis:** (a) `test_forexe_receptii.py` nerulat pe host; (b) `ReceptiiView` niciodată pe ecran; (c) **pass 0015-02 (tooltip de recepție `NewRootPlatiTooltip`) nefăcut** |

| 0016 | `KBotDataView` — coloane auto-hide (responsive) | DONE (code) / **NO VISUAL VERDICT** | `SLICE-0016-kbotdataview-auto-hide-columns.md` | New per-column `Column.AutoHide`: a column may disappear when the grid would otherwise need a horizontal scrollbar. The fit pass (in `PerformAutoSize`, after measure, before fill) hides auto-hideable columns **rightmost-first** until the rest fit or none remain; if none remain, the scrollbar appears normally (with auto-hide engaged, unresolved overflow does **not** shrink the survivors — that overrides slice-0013 `ShrinkToFit`). Auto-hidden state is a separate `Friend AutoHidden` flag (never touches the caller's `Visible`), recomputed every layout so a widened grid brings columns back; `IsEffectivelyVisible = Visible AndAlso Not AutoHidden` replaces the two column-visibility reads (`RecalcColumnLayout`, `VisibleColumns`) so paint/hit-test/nav/auto-size all inherit it. **Interaction with the stretch column:** the `ColumnFillMode` First/Last target is the "expanding column" — it's **never** auto-hidden (a column that is both `AutoHide` and the fill target stays and expands: stretching wins), and once a column disappears the expander absorbs the freed gap (the leftover branch). `Proportional`/`None` have no single protected target. **Interpretation flagged:** there is no per-column stretch flag in the codebase — "the property to stretch a column" was read as the existing grid-wide `ColumnFillMode`; if the operator meant a per-column `Stretch`, only the target designation changes. Controls 120 tests (+9), full solution 243 green, 0 warnings. DevHarness got a «Ascunde coloane la nevoie» checkbox. ⚠️ **Unrun on screen** like all of 0010/0013; minor pre-existing edge (editor left floating if a resize hides the edited column) noted in the worklog |

| 0017 | Plăți — endpoint + `PlatiView` (a patra vedere reală) + `KBotDataView` totals row | 0017-01/02 DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | `SLICE-0017-01-kbotdataview-totals-row.md`, `SLICE-0017-02-plati-endpoint.md` | Plan: pasted in-session. Four passes: **01** `KBotDataView` pinned totals row; **02** server `GET /api/forexe/plati?cod=`; **03** `PlatiView`; **04** fix Rezervări «+» (one leaf, not all). **0017-02 (DONE):** `GET /api/forexe/plati?cod=` (cititor brut, un rând per `FX_Plati`, cu extrasul bancar `FX_Extrase` purtat pe rând + `are_ord`). **Deviere de la SCHIȚA planului:** clasificația prin `FX_Indicatori` (join CodAI, PK→1:1) + subinterogare scalară `Clasificatii` pe `IdClsfAcc=I.IdClsf` `LIMIT 1` (drumul 0011-03), NU `p.IdClsf` — rezolvat de nota Step 0 a planului («match receptii.py»); se întorc și `clsf`/`denumire` (nomenclator) și `clsf_plata` (brut, fallback). `LEFT JOIN FX_Extrase` (nu INNER pe H ca Access) — o plată fără extras nu dispare. `are_ord` contra unui derivat `DISTINCT` (o plată pe mai multe linii de ordonantare nu duplică rândul). `ORDER BY Data_plata, IdPlataFX`. `data_doc` e TEXT (string brut, nu ISO). 15 teste Python host-only (skip off-host), inclusiv garda anti-fan-out `count(plati)==count(FX_Plati)`. Suite offline 75 passed / 13 skipped, py_compile curat. **0017-01 (DONE):** reusable totals band in `KBot.Controls`, NOT special-cased for Plăți. New `KBotAggregate {None,Sum,Count,Average}`; `KBotDataColumn.Aggregate`/`AggregateFormatString`; `KBotDataView.ShowTotalsRow`/`TotalsRowHeight`. Pinned band between body and h-scroll, header band styling (reused `_bHeaderBack`/`_pHeaderSep`/`_pHeaderBaseline`/`HeaderFont`), frozen + h-scroll layering mirrored from `DrawHeader`. Aggregates over ALL rows: Sum/Average skip Nothing+non-numeric, Average of nothing = empty (not 0/NaN), Count = rows with `HasValue` (not non-empty cells). NOT a row (excluded from RowCount/nav/hit-test/dirty). `ViewportHeight`+`UpdateScrollBars`+`WillVScrollBeVisible` all subtract the band; totals text participates in auto-size measurement (chosen, so a wide total can't ellipsize). **Deviation:** `AddRow` recompute is one step behind (cells set after AddRow returns), so the control's per-cell `Item` setter recomputes too; bulk loads via `KBotDataRow` inside Begin/EndUpdate recompute once at EndUpdate. Controls 120→134 tests (+14), 0 warnings. DevHarness got a «Rând de totaluri» checkbox. ⚠️ **Band never rendered on screen; `KBotDataView` visual harness STILL unrun** (open since 0010) — frozen alignment only smoke-tested headless |

**Next free slice number: 0018.**

---

## Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).
- **Next:** the remaining seven views are still `PlaceholderView` (Sumar landed in 0011,
  Rezervări in 0014); Slice 0004's remaining Tier 1 items and the Slice 0003 VPS config
  are still open and short.
- **Slice 0015 (Recepții) — pass 0015-01 landed, code green, 0 warnings; the third real
  view.** The plan's three forks (tree depth, LISTA driver, single endpoint) were all
  settled from the Access source, not guessed. Three human items: (1) run
  `test_forexe_receptii.py` on the host (15 tests, skip off-host) — confirms the eight
  tables/columns exist and the `FX_Indicatori` join yields a populated `Clsf` (blank on
  every row ⇒ join key, same trap as 0011-03); (2) `ReceptiiView` has never been rendered
  on screen; (3) 0015-02 tooltip + **0015-03 operator revision landed** (3-level month tree,
  aggregated LISTA at any level, Descriere=Clasificatii.Denumire, month+receptie tooltip with
  the plăți window extended to the next month's first receptie). Nothing in 0015 has been
  rendered on screen or run on a live DB.
- **Slice 0014 (Rezervări) — code green, 0 warnings; the second real view.** The plan's
  open month-total question was answered from the Access source (`qFX_REZERVARI_TREE.TOTALL
  = SUM(R_Valoare)`), not guessed. Three human items: (1) run `test_forexe_rezervari.py`
  on the host (16 tests, skip off-host) — confirms the eight tables/columns exist and the
  `FX_Indicatori` join yields a populated `Clsf` (blank on every row ⇒ join key, not the
  view — same trap as 0011-03); (2) `RezervariView` has never been rendered on screen;
  (3) the exact screenshot month totals were not reproduced numerically (no data for that
  angajament) — confirm visually.
- **Slice 0011 (Sumar) — all three passes landed, 164 .NET tests green, 0 warnings.**
  The join-key question is now ANSWERED on real data (0011-03): the key was wrong,
  `IdUnitate` was missing, and the nomenclator has duplicates — all three fixed by
  moving to scalar subqueries. Two things still need a human:
  (1) rerun `test_forexe_sumar.py` on the host (18 tests now; they skip off-host) to
  confirm the fix end-to-end and that all eleven touched tables exist;
  (2) look at `SumarView` on screen — **it has still never been rendered**.
- **Slice 0010 (`KBotDataView`) — all six passes landed, 158 tests green, 0 warnings.**
  **The one open item is a human one:** nobody has run the visual harness yet. Please run
  DevHarness (Debug start → «Nu») → Controls/UI → «KBotDataView — virtualizare + temă
  (5.000 × 20)» and give it a Pass/Fail. That probe is the only way to confirm scroll
  smoothness, keyboard/resize interaction, floating-editor placement and the actual colours —
  and pass 06 found a bug (all column headers were `Nothing` since 0010-01) that no headless
  test could have caught. After that: the **Sumar** slice, which consumes the grid read-only.

## Open threads (not yet scheduled)

- `_upload_sessions` (routes/ftp.py) is still an in-process dict → blocks multi-worker.
- The rate limiter's counters are also in-process → restart clears lockouts; also blocks
  multi-worker. Both must move to Redis before `workers > 1` is possible.
- **No MariaDB DDL for `FX_DDF`, `FX_ORD`, `FX_DDF_REV_SA`, `FX_Receptii_H`** — five of
  the nine tree flags read them and `DDL_FX_ListaAngajamente.sql` creates none of them.
  The operator confirms all four exist live (2026-07-15); the endpoint hard-fails if one
  does not. Add their DDL so the repo stops disagreeing with production.
- **The two FX_Angajamente DDLs contradict each other**: `DDL_FX_ListaAngajamente.sql:42`
  has `ASCUNS` and `IdUnitate`, `docs/FX_Angajamente.sql` has neither. Operator confirms
  `ASCUNS` exists live. Pick one file as canonical and delete the other.
- The Access export used for the tree contract is the **repo copy**
  (`FX_System_Export/QUERIES/`); nobody has confirmed it matches the live Access file, or
  `C:\AVACONT\FX_System_Export`.
- `KBotNavList` has no `SetItemVisible` — 0008 gates views with `SetItemEnabled`
  (grey-out) instead of hiding. Add real visibility if the greyed items bother operators.
- `GET /api/forexe/angajamente` + `IApiClient.GetAngajamenteAsync` have **no caller left**
  after 0008 (the tree replaced them on MainForm's load path). Retire or find them a use.
- Server housekeeping: kernel reboot, systemd hardening, de-root the service user (blocked
  by AvacontPush SSH dependency), close stray ufw port 5010.
- Naming ambiguity: `Unitati` exists in both `AVACONT_COMUN` and each per-unit DB with
  different columns.
- **`Clasificatii` DDL is not in the repo.** Slice 0011 was blocked on it until the
  operator pasted it by hand. It documents real contracts the code now depends on
  (`Clsf`/`Titlu`/`SS` as STORED generated columns; FKs into `AVACONT_COMUN.Defa*`).
  Add it to the repo so the next slice does not have to ask again.
- ~~**MariaDB inverts Access's classification-id naming**~~ — SETTLED in 0011-03 on
  real data: `FX_Indicatori.IdClsf` holds the **Access** id (matches `C.IdClsfAcc`).
  Promoted to Locked decisions below.
- **`Clasificatii` has real duplicates on `(IdClsfAcc, IdUnitate)`** — on `000_DEMO`:
  `(75,79)`, `(75,84)`, `(75,90)`, `(75,92)`, `(75,93)`, each twice. Sumar is safe
  (scalar subquery + `LIMIT 1` = one row per indicator), so this does not block, but
  it needs triage: **if the duplicate rows differ by `Sursa`/`Sector`, a join
  dimension is MISSING** and `LIMIT 1` is silently choosing between two genuinely
  different classifications; if they are identical, they are data to clean. Any
  future view that needs the classification's `Sursa`/`Sector` must settle this first.

## Locked decisions (do not relitigate without a note here)

- Single-worker Gunicorn stays; `on_starting` guard refuses `workers > 1`.
- Under Redis: TTL counts down to the 30-min absolute cap (no slide); the 20-min idle
  window is enforced in-app from `expires_at`; operator password is never written to Redis;
  keys namespaced `kbot:sess:` in K-BOT's own DB; never FLUSHDB/FLUSHALL (shared Redis).
- Static API key kept ONLY for the legacy FOREXE fleet on shared routes; eliminated for
  K-BOT.
- **The "drop `IdUnitate`" rule applies to `FX_` tables ONLY.** Shared nomenclatoare
  (`Clasificatii`, `Parteneri`, and others of that kind) **keep** the `IdUnitate`
  predicate: the per-unit database holds them for SEVERAL units. On `000_DEMO`,
  `Clasificatii` carries 8 (48, 75, 76, 121, 123, 135, 136, 157). Measured cost of
  getting this wrong in slice 0011 (`FX_Indicatori` = 29 rows, 25 with `IdClsf <> 0`):
  `ON I.IdClsf = C.IDClsf` → **0** rows (wrong key); `ON I.IdClsf = C.IdClsfAcc` →
  **67** (multi-unit fan-out); `+ AND I.IdUnitate = C.IdUnitate` → **50** (duplicate
  fan-out). Full detail: `SLICE-0011-03-sumar-join-clasificatii.md`.
- **Key-naming conventions, both confirmed against the live schema:**
  - `Clasificatii`: `IDClsf` = MariaDB PK, `IdClsfAcc` = retained Access id.
    **BUT `FX_Indicatori.IdClsf` holds the ACCESS id — it does NOT follow the
    convention.** The column name is not evidence.
  - `FX_ORD` family: suffix "P" = MariaDB PK (`IDORDTBLP`), without "P" = Access id
    (`IDORDTBL`).
  - **Operational rule that beats both tables: never infer the key from the column
    name — COUNT rows before and after the join.** A join returning 0, or more rows
    than its left-hand table, is a defect, not a data quirk. Slice 0011 lost three
    live runs to this; one `COUNT(*)` would have caught all three.
- **Migrarea `FX_` NU recreează schema (varianta A, nedistructivă).** Tabelele există
  deja în MariaDB cu DDL curat; `POST /api/forexe/seed/schema` rămâne în cod pentru o
  bază complet goală, dar **utilitarul de migrare nu îl apelează** — face
  `DROP TABLE IF EXISTS` înainte de un `CREATE` derivat din tipurile DAO ale Access-ului
  (`LONGTEXT` pentru orice tip necunoscut, `VARCHAR(255)` implicit), deci ar înlocui o
  schemă bună cu una mai slabă, peste date reale. Lista de coloane se citește în schimb
  din destinație, cu `GET /api/forexe/seed/columns` (slice 0012-01).
- Shared nomenclatoare are read via **scalar subqueries with `LIMIT 1`**, not joins,
  wherever the value is display-only (`Clsf`, `Partener` in Sumar). `Clasificatii` has
  real duplicates on `(IdClsfAcc, IdUnitate)`, so even a fully-predicated join fans
  out; a scalar subquery guarantees one row per indicator.

---

## How to update this file

- When a slice changes state, edit its row and (if needed) the Current focus section.
- When you assign a new slice, add a row and bump "Next free slice number".
- Do it in the same commit as the work, alongside the worklog. Never let this drift.
