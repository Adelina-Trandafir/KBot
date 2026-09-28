# K-BOT — STATUS, slices 0010–0019

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0010

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0010 | `KBotDataView` — owner-drawn unbound grid (Access continuous-form) | DONE (code) / **NO VISUAL VERDICT YET** | `SLICE-0010-01-…skeleton.md`, `-02-…virtualizare.md`, `-03-…tipuri-coloana.md`, `-04-…formatare-disable.md`, `-05-…input-selectie.md`, `-06-…editare.md`, `-07-…scroll-by-column.md` | Plan: pasted in-session (continuation plan supersedes the original where they disagree). Multi-pass. **0010-01 (skeleton):** models + enum, double-buffered `Control` implementing `IThemedControl`, theming cache + `ApplyTheme`, header + empty body, 4 child controls in Designer. **0010-02 (render+virtualize):** split into partials (`.Theming`/`.Layout`/`.Painting`), frozen + scrolling column bands, integer virtualization math, two-pass scrollbar sizing, Text + CheckBox cell painting, `CellFormatting`/`RowFormatting` plumbing with **reused** args, full palette→role mapping. **Decision:** kept in `KBot.Controls` + a `KBot.Theming` ProjectReference (no cycle) so it self-themes — the plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade). **0010-03 (column types):** Combo / OptionButton / Button / ProgressBar painting + `OptionGroup` exclusivity via `SetOptionValue`. **0010-04 (formatting+disable):** three-level effective-enabled (`IsCellEnabled`/`IsRowEnabled`, separate "probe" args so queries can't clobber an in-flight paint), disabled rendering across all six types, conditional formatting. **0010-05 (input+selection):** current cell + `SelectionChanged`, Access-style keyboard nav, click/double-click, toggle + button activation gated on `IsCellEnabled`, header-edge column resize. **0010-06 (editing):** floating Text/Combo editor, `CellValidating` (veto **and** value coercion), Esc discard, auto-commit on move/scroll, and the corrected `IsDirty` contract — API writes are *loading*, only operator edits/toggles dirty a row (`ClearDirty` added; 2 pass-01 tests deliberately rewritten). Virtualization proven **headlessly** (same painted-row count at 5,000 and 50,000 rows). 158 tests green. **Editing caught 2 real VB case-insensitivity bugs** where a parameter shadowed a same-named property and an unqualified assignment was a silent no-op: `ProposedValue` (commit always wrote Nothing) and — live since 0010-01 — `HeaderText`, meaning **every column header was Nothing**; both fixed with `Me.` + regression tests. ⚠️ **NOBODY HAS RUN THE VISUAL HARNESS for any of the six passes.** Scroll smoothness, the WinForms key→handler path, resize drag, floating-editor placement/focus and actual colours are all unverified — the blank-header bug shows exactly why that matters. Run: DevHarness → Controls/UI → «KBotDataView — virtualizare + temă (5.000 × 20)». **0010-07 (later add-on):** `ScrollByColumn` property — horizontal scroll snaps to column edges (direction-aware: a small step still advances a full column), covering thumb/arrows/wheel via one snap in the ValueChanged path; harness got a «Derulare pe coloană» checkbox. 7 tests. Next: Sumar slice consumes it read-only |

### Current focus

- **Slice 0010 (`KBotDataView`) — all six passes landed, 158 tests green, 0 warnings.**
  **The one open item is a human one:** nobody has run the visual harness yet. Please run
  DevHarness (Debug start → «Nu») → Controls/UI → «KBotDataView — virtualizare + temă
  (5.000 × 20)» and give it a Pass/Fail. That probe is the only way to confirm scroll
  smoothness, keyboard/resize interaction, floating-editor placement and the actual colours —
  and pass 06 found a bug (all column headers were `Nothing` since 0010-01) that no headless
  test could have caught. After that: the **Sumar** slice, which consumes the grid read-only.

---

## Slice 0011

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0011 | Sumar — endpoint + `SumarView` (prima vedere reală) | DONE (code) / **partially run on a live DB (0011-03)** / **NO VISUAL VERDICT** | `SLICE-0011-01-sumar-endpoint.md`, `SLICE-0011-02-sumar-view.md`, `SLICE-0011-03-sumar-join-clasificatii.md` | Plan: pasted in-session. Port of `qFX_MAIN_SUMAR` v1 → `GET /api/forexe/sumar?cod=…` (header hoisted once + one row per indicator), plus `SumarView` replacing `PlaceholderView` for the `sumar` key — **the first of the nine views that is real**, and the first consumer of `KBotDataView` (read-only). **The `Clsf` blocker was resolved by the operator supplying the real DDL, not by guessing:** `Clasificatii.Clsf` EXISTS as a STORED generated column `concat_ws('.',Capitol,Subcapitol,Articol,Alineat)` (indexed), so Branch A — select it directly. `Titlu = left(Articol,2)` explains Access's `Mid(Clsf,13,2)`. Port decisions: no SS filter, `ClasificatiiG`→`Clasificatii` / `ParteneriG`→`Parteneri`, all `IdUnitate` join predicates dropped, **`LEFT JOIN Clasificatii`** (was INNER — an indicator with no classification must still appear), `TotalReceptii = SUM(DIF)` not `Valoare`, `'Angajament nou.'` with the trailing period. Deliberate deviations: `COALESCE(...,0)` on the five totals, `cod` pushed into every aggregate (Access full-scanned per aggregate), deterministic `ORDER BY` added, `ROUND(,2)` kept on revizii/ordonanțări only. Client: `WithReauth` passed in specialized on `SumarInfo` so the 401 policy stays in the shell; snake_case stops at the wire DTOs; **stale-response guard** discards a superseded `cod`. 164 tests green, 0 warnings. **0011-03 (after the operator's live run) fixed three stacked defects in the query:** the join key WAS wrong (`FX_Indicatori.IdClsf` holds the Access id → matches `C.IdClsfAcc`, giving 0 rows against `C.IDClsf`); `IdUnitate` had been dropped from a SHARED nomenclator (67-row fan-out); and `Clasificatii` has real duplicates on `(IdClsfAcc, IdUnitate)` (still 50 rows with both predicates). Fix: `LEFT JOIN Clasificatii` → **scalar subquery with `LIMIT 1`**, `ORDER BY` moved to the output alias. **The same defect was found and fixed in `aggRev`/`Parteneri`, which the brief did not ask for** — there the join sat BEFORE the `GROUP BY`, so a duplicate partener multiplied `SUM(SA.ValCur)` and inflated `TotalRevizii` (a wrong money figure, not a blank column). 3 new tests incl. a general `len(rows) == indicator count` sentinel. ⚠️ **Still open:** (a) nobody has run `SumarView` on screen, and `KBotDataView`'s visual harness is STILL unrun; (b) the `Clasificatii` duplicates need triage — if the rows differ by `Sursa`/`Sector` then a join dimension is MISSING and `LIMIT 1` picks arbitrarily between two different classifications; if identical, they are data to clean |

### Current focus

- **Next:** the remaining seven views are still `PlaceholderView` (Sumar landed in 0011,
  Rezervări in 0014); Slice 0004's remaining Tier 1 items and the Slice 0003 VPS config
  are still open and short.

- **Slice 0011 (Sumar) — all three passes landed, 164 .NET tests green, 0 warnings.**
  The join-key question is now ANSWERED on real data (0011-03): the key was wrong,
  `IdUnitate` was missing, and the nomenclator has duplicates — all three fixed by
  moving to scalar subqueries. Two things still need a human:
  (1) rerun `test_forexe_sumar.py` on the host (18 tests now; they skip off-host) to
  confirm the fix end-to-end and that all eleven touched tables exist;
  (2) look at `SumarView` on screen — **it has still never been rendered**.

### Open threads

- **`Clasificatii` DDL is not in the repo.** Slice 0011 was blocked on it until the
  operator pasted it by hand. It documents real contracts the code now depends on
  (`Clsf`/`Titlu`/`SS` as STORED generated columns; FKs into `AVACONT_COMUN.Defa*`).
  Add it to the repo so the next slice does not have to ask again.

- ~~**MariaDB inverts Access's classification-id naming**~~ — SETTLED in 0011-03 on
  real data: `FX_Indicatori.IdClsf` holds the **Access** id (matches `C.IdClsfAcc`).
  Promoted to Locked decisions below.

---

## Slice 0012

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0012 | Migrare Access → MariaDB — introspecție coloane pentru seed | 0012-01 DONE (code) / **NEVER RUN ON A LIVE DB** | `SLICE-0012-01-seed-columns.md` | `GET /api/forexe/seed/columns?db_name=&table=` în `routes/forexe/seed.py`, al treilea endpoint al fișierului. Gardă **`@require_api_key` (X-Api-Key, NU bearer)** ca celelalte două — seed-ul e condus de VBA/FOREXE legacy. `db_name` prin `_DBNAME_RE`, `table` prin aceeași `ALLOWED_TABLES`; niciun identificator din client nu ajunge în SQL fără allow-list. Întoarce **doar numele coloanelor, în ordinea din tabel** (`SHOW COLUMNS`), ca apelantul să construiască INSERT-uri pe poziție. **Tabel inexistent → `200` cu `columns: []`, NU 404** — apelantul trebuie să distingă «zero coloane» (migrarea nu a rulat) de o eroare de rețea/cheie. Ca să nu se adulmece errno 1146 dintr-o excepție, existența se testează întâi cu `SHOW TABLES LIKE %s` (parametrizat, nu aruncă) și abia apoi `SHOW COLUMNS`. Strict read-only: nicio scriere, niciun DDL, `conn.close()` în `finally`. 11 teste host-only. **Decizie blocată (varianta A, nedistructivă): `seed/schema` rămâne în cod dar utilitarul de migrare NU îl apelează** — tabelele `FX_` există deja în MariaDB cu DDL curat, iar `/schema` face `DROP TABLE IF EXISTS` înainte de `CREATE` din tipurile DAO (`LONGTEXT` pentru orice necunoscut), deci le-ar recrea mai slabe peste date reale. `/columns` există exact ca să nu fie nevoie de `/schema`. ⚠️ Neverificat: nimic nu a atins o bază reală; presupunerea că `FX_Rezervarii_IMG` NU e migrat pe `000_DEMO` (folosit ca tabel-lipsă în test) e a mea — testul se sare explicit dacă apare; contul de seed are nevoie de privilegiul `SHOW` pe baza țintă |

---

## Slice 0013

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0013 | `KBotDataView` — auto-sizing coloane + moduri de umplere | DONE / **VISUALLY ACCEPTED** in harness playground | `SLICE-0013-column-sizing.md` | Plan: pasted in-session. Two grid-wide knobs run as ONE pass in `UpdateLayout` (before offsets/scrollbars): `AutoSizeColumnsMode` (`None`/`ToContent`, default `ToContent`) measures each visible column to `max(header, sampled cells) + padding` clamped to `[MinWidth, MaxWidth]`; `ColumnFillMode` (`None`/`FirstColumn`/`LastColumn`/`Proportional`, default `None`) then spends the leftover or absorbs the overflow so a fill mode never shows a horizontal scrollbar (except the honest `sum(MinWidth) > available` fallback). Model gains `MaxWidth` (default uncapped) + `Width` clamped `[Min,Max]` on every write + internal `UserSized` (drag pins a column; `ToContent` skips it, fill/shrink still applies; `ResetColumnSizing()` clears it). **New hard rule: all code comments added/touched this slice are in ENGLISH** (marked «English (slice 0013)»); the rest of `KBot.Controls` stays Romanian — no mass conversion. Measuring uses `TextRenderer.MeasureText` with the painter's fonts and the FORMATTED value (so `N2` money measures wide). Available width mirrors `UpdateScrollBars` exactly, vScroll visibility decided first (row-count only) → no circular dependency; `_inAutoLayout` re-entrancy guard. Limitations (in worklog): sampling (`AutoSizeSampleRows`, default 200, 0=all — a wider value further down ellipsizes), `CellFormatting` NOT raised while measuring (a handler that widens `Text` ellipsizes), the `MinWidth`-overflow fallback, and no column→grid back-reference (post-load column-property edits need an explicit `AutoSizeColumns()`; `AddColumn`/drag/resize/theme/`EndUpdate` cover the rest). 93 `KBot.Controls.Tests` green (15 new auto-size + 4 model), full solution green, 0 warnings. ⚠️ **Still unrun on screen** — like all of 0010/0011. Follow-up (separate): `SumarView` should adopt `ToContent` + `LastColumn`/`Proportional` and drop its hardcoded widths (Partener column removed separately) |

---

## Slice 0014

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0014 | Rezervări — endpoint + `RezervariView` (a doua vedere reală) | DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | `SLICE-0014-rezervari-view.md` | Plan: pasted in-session. `GET /api/forexe/rezervari?cod=` (cititor brut, un rând per `FX_Rezervari`) + `RezervariView` (master/detail: arbore lună→(dată,tip) la stânga, grilă Clsf/CreditBug/Inițiale/Curentă/Definitive la dreapta) înlocuind `PlaceholderView` pentru `rezervari`. **Întrebarea deschisă a planului (§7.1, totalul lunar) e REZOLVATĂ din sursa Access, nu ghicită:** `qFX_REZERVARI_TREE` calculează `TOTALL = SUM(R_Valoare)` corelat pe (CodAngajament, lună, an) — candidatul (a). Valoarea frunzei = `SUM(IIf(EInitiala,R_Initiala,R_Valoare))` (Suma din `QFX_DDF_REZERVARI`); tipul derivat client-side (Inițială>Mărire>Micșorare). Clasificația: aceleași decizii ca Sumar 0011-03 — prin `FX_Indicatori` (join CodAI, `IdClsf`=id Access), subinterogare scalară `LIMIT 1` + predicat `IdUnitate` păstrat la nomenclator. `LEFT JOIN FX_Indicatori` (o rezervare nu dispare fără indicator). Iconițe GDI tematizabile (`RezervariIcons`), nu resurse binare. «+» = display-only (ridică `AdaugaDdfCerut`, fără abonat) — workflow-ul DDF (migration-plan item 7) e felie ulterioară. Plasa 401 + stale-guard ca la Sumar. Api 36→42 (+6 GetRezervari), App 22→30 (+8 view), 16 teste Python host-only (skip off-host). ⚠️ **Trei lucruri deschise:** (a) nimeni nu a văzut `RezervariView` pe ecran (+ harness-ul `KBotDataView` STILL unrun); (b) `test_forexe_rezervari.py` nerulat pe host — confirmă că cele opt tabele/coloane există și că join-ul prin `FX_Indicatori` dă Clsf populat; (c) cifrele exacte din screenshot (Ian 1.091.940 / …) NU au fost reproduse numeric (formula e din sursă, dar datele angajamentului lipsesc) |

### Current focus

- **Slice 0014 (Rezervări) — code green, 0 warnings; the second real view.** The plan's
  open month-total question was answered from the Access source (`qFX_REZERVARI_TREE.TOTALL
  = SUM(R_Valoare)`), not guessed. Three human items: (1) run `test_forexe_rezervari.py`
  on the host (16 tests, skip off-host) — confirms the eight tables/columns exist and the
  `FX_Indicatori` join yields a populated `Clsf` (blank on every row ⇒ join key, not the
  view — same trap as 0011-03); (2) `RezervariView` has never been rendered on screen;
  (3) the exact screenshot month totals were not reproduced numerically (no data for that
  angajament) — confirm visually.

---

## Slice 0015

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0015 | Recepții — endpoint + `ReceptiiView` (a treia vedere reală) | 0015-01/02/03 DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | `SLICE-0015-01-receptii-view.md`, `SLICE-0015-02-receptii-root-tooltip.md`, `SLICE-0015-03-receptii-month-tree-revision.md` | Plan: `PLAN_ReceptiiView.md`. **0015-03 = revizuire operator (2026-07-22):** arbore pe **3 niveluri** (folder lună/an → recepție → antet, grupat pe DataR); LISTA **agregată la orice nivel** (click pe lună/recepție/antet → total Sum(DIF) + rând per clsf Sum(Valoare)); coloana „Descriere" = `Clasificatii.Denumire` (nou pe endpoint, drumul 0011-03) fiindcă agregă peste anteturi; **tooltip pe lună ȘI recepție** cu fereastra de plăți extinsă până la **prima recepție a lunii următoare** (nu ≤ MaxDataH). Api 48, App 39. `GET /api/forexe/receptii?cod=` (cititor brut: un rând per linie `FX_Receptii`, cu antet `H` + recepție `R` părinte, + array `plati` în același răspuns) + `ReceptiiView` (master/detail: arbore **2 niveluri** recepție→antet la stânga, grilă LISTA NrCrt/Descriere/Clsf/Valoare la dreapta) înlocuind `PlaceholderView` pentru `receptii`. **Cele trei furci ale planului sunt blocate din sursa Access, nu ghicite:** (1) arbore 2 niveluri (frunzele pe clsf din `Show_Receptii` sunt cod comentat — `AggregateRow`/`valAsoc` dormante); (2) LISTA condusă doar de antet (rând-total `Sum(DIF)` + rânduri per clsf `Sum(Valoare)`, din `qFX_MAIN_REC_LISTA_IND`); (3) un singur endpoint (nu tree/lista/tooltip separate). Clasificația: același drum ca Sumar 0011-03 (`FX_Indicatori`, `IdClsf`=id Access, subinterogare scalară `LIMIT 1` + `IdUnitate` păstrat la nomenclator, `LEFT JOIN` ca o linie să nu dispară). Iconițe GDI de stare (sus/jos/neutru, finding 5) tematizabile. Plasa 401 + stale-guard ca la Sumar/Rezervări. Api 42→48 (+6 GetReceptii), App 30→37 (+7 view), 15 teste Python host-only (skip off-host). **Devieri documentate:** chei de fir snake_case (nu PascalCase-ul schiței); coloana grilei „Descriere" = Descrierea ANTETULUI (`FX_Receptii_H.Descriere`, din `Nz([HH]![Descriere],…)`), nu a indicatorului; `sters_h` adăugat în rând pentru cumulul DIFH din tooltip (0015-02). ⚠️ **Deschis:** (a) `test_forexe_receptii.py` nerulat pe host; (b) `ReceptiiView` niciodată pe ecran; (c) **pass 0015-02 (tooltip de recepție `NewRootPlatiTooltip`) nefăcut** |

### Current focus

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

---

## Slice 0016

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0016 | `KBotDataView` — coloane auto-hide (responsive) | DONE (code) / **NO VISUAL VERDICT** | `SLICE-0016-kbotdataview-auto-hide-columns.md` | New per-column `Column.AutoHide`: a column may disappear when the grid would otherwise need a horizontal scrollbar. The fit pass (in `PerformAutoSize`, after measure, before fill) hides auto-hideable columns **rightmost-first** until the rest fit or none remain; if none remain, the scrollbar appears normally (with auto-hide engaged, unresolved overflow does **not** shrink the survivors — that overrides slice-0013 `ShrinkToFit`). Auto-hidden state is a separate `Friend AutoHidden` flag (never touches the caller's `Visible`), recomputed every layout so a widened grid brings columns back; `IsEffectivelyVisible = Visible AndAlso Not AutoHidden` replaces the two column-visibility reads (`RecalcColumnLayout`, `VisibleColumns`) so paint/hit-test/nav/auto-size all inherit it. **Interaction with the stretch column:** the `ColumnFillMode` First/Last target is the "expanding column" — it's **never** auto-hidden (a column that is both `AutoHide` and the fill target stays and expands: stretching wins), and once a column disappears the expander absorbs the freed gap (the leftover branch). `Proportional`/`None` have no single protected target. **Interpretation flagged:** there is no per-column stretch flag in the codebase — "the property to stretch a column" was read as the existing grid-wide `ColumnFillMode`; if the operator meant a per-column `Stretch`, only the target designation changes. Controls 120 tests (+9), full solution 243 green, 0 warnings. DevHarness got a «Ascunde coloane la nevoie» checkbox. ⚠️ **Unrun on screen** like all of 0010/0013; minor pre-existing edge (editor left floating if a resize hides the edited column) noted in the worklog |

---

## Slice 0017

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0017 | Plăți — endpoint + `PlatiView` (a patra vedere reală) + `KBotDataView` totals row | 0017-01..04 DONE (code) / **0017-04 VĂZUT PE ECRAN ȘI ACCEPTAT 2026-08-13** (prima acceptare vizuală a vederii Plăți) / **NEVER RUN ON A LIVE DB** | `SLICE-0017-01-kbotdataview-totals-row.md`, `SLICE-0017-02-plati-endpoint.md`, `SLICE-0017-03-plati-view.md`, `SLICE-0017-04-plati-tree-doua-niveluri.md` | Plan: pasted in-session. Four passes: **01** `KBotDataView` pinned totals row; **02** server `GET /api/forexe/plati?cod=`; **03** `PlatiView`; **04** fix Rezervări «+» (one leaf, not all). **0017-03 (DONE):** a patra vedere reală (master/detail: arbore 3 niveluri `« TOATE PLĂȚILE »`/lună/zi/plată la stânga, grilă LISTA sus-dreapta cu rând de totaluri, panou extras bancar jos-dreapta) înlocuind `PlaceholderView` pentru `plati`. LISTA e **filtru, nu agregat** (click pe nod → exact rândurile lui). Panoul de detaliu e condus de selecția din grilă, din datele deja pe rând (fără al doilea apel). «+» pe EXACT o zi (cea mai veche cu `AreOrd=False`) + luna ei + nodurile de nivel 2 ne-ordonantate; nimic altundeva. Reînvie codul dormant Level=2 din Access. **Asumpții notate (fără sursă Access pentru lună/zi merjate):** iconița stării = orice `Incarcat`→sus / orice `Preluat`→jos / neutru; verde INCASARE doar când TOATE rândurile zilei/lunii sunt INCASARE (per-rând la nivel 2). «+» single-tint (Access nu are variantă verde pentru plăți). `PlatiInfo` (`PlataRow`/`ExtrasBancar`) + `GetPlatiAsync` + `PlatiIcons` (up/down/neutru/«+»/«toate») + events dormante `AdaugaOrdonantari(Cerut)`. Api 48→53 (+5), App 39→50 (+11), soluție 0 warnings. **0017-02 (DONE):** `GET /api/forexe/plati?cod=` (cititor brut, un rând per `FX_Plati`, cu extrasul bancar `FX_Extrase` purtat pe rând + `are_ord`). **Deviere de la SCHIȚA planului:** clasificația prin `FX_Indicatori` (join CodAI, PK→1:1) + subinterogare scalară `Clasificatii` pe `IdClsfAcc=I.IdClsf` `LIMIT 1` (drumul 0011-03), NU `p.IdClsf` — rezolvat de nota Step 0 a planului («match receptii.py»); se întorc și `clsf`/`denumire` (nomenclator) și `clsf_plata` (brut, fallback). `LEFT JOIN FX_Extrase` (nu INNER pe H ca Access) — o plată fără extras nu dispare. `are_ord` contra unui derivat `DISTINCT` (o plată pe mai multe linii de ordonantare nu duplică rândul). `ORDER BY Data_plata, IdPlataFX`. `data_doc` e TEXT (string brut, nu ISO). 15 teste Python host-only (skip off-host), inclusiv garda anti-fan-out `count(plati)==count(FX_Plati)`. Suite offline 75 passed / 13 skipped, py_compile curat. **0017-01 (DONE):** reusable totals band in `KBot.Controls`, NOT special-cased for Plăți. New `KBotAggregate {None,Sum,Count,Average}`; `KBotDataColumn.Aggregate`/`AggregateFormatString`; `KBotDataView.ShowTotalsRow`/`TotalsRowHeight`. Pinned band between body and h-scroll, header band styling (reused `_bHeaderBack`/`_pHeaderSep`/`_pHeaderBaseline`/`HeaderFont`), frozen + h-scroll layering mirrored from `DrawHeader`. Aggregates over ALL rows: Sum/Average skip Nothing+non-numeric, Average of nothing = empty (not 0/NaN), Count = rows with `HasValue` (not non-empty cells). NOT a row (excluded from RowCount/nav/hit-test/dirty). `ViewportHeight`+`UpdateScrollBars`+`WillVScrollBeVisible` all subtract the band; totals text participates in auto-size measurement (chosen, so a wide total can't ellipsize). **Deviation:** `AddRow` recompute is one step behind (cells set after AddRow returns), so the control's per-cell `Item` setter recomputes too; bulk loads via `KBotDataRow` inside Begin/EndUpdate recompute once at EndUpdate. Controls 120→134 tests (+14), 0 warnings. DevHarness got a «Rând de totaluri» checkbox. ⚠️ **Band never rendered on screen; `KBotDataView` visual harness STILL unrun** (open since 0010) — frozen alignment only smoke-tested headless. **0017-04 (DONE, VĂZUT PE ECRAN):** activarea vederii arunca `ArgumentException: Cheie de coloană duplicată: 'clsf'` — o trecere prin designer autorase toate cele cinci coloane, iar `BuildColumns()` le adăuga a doua oară; `BuildColumns` șters, designerul deține coloanele. Arborele a coborât de la patru straturi la **DOUĂ**: rădăcină lună → frunză ZI (toate plățile zilei într-un nod); `« TOATE PLĂȚILE »` și nodul per `IdPlataFX` au dispărut, LISTA rămâne filtru. Iconițele vin din `image_list` (`month` pe lună, `up`/`down` pe starea merjată a zilei); luna nu mai poartă stare și nu mai e verde (ca în Access: folder, nu stare). Lunile pornesc strânse **în afară de cea cu «+»** (abatere deliberată de la Access, cerință operator). ⚠️ **Defect găsit pe drum:** o regenerare a lui `InitializeComponent` prin designer ștersese cele zece apeluri `InitDetailPair`, deci panoul de detaliu ieșea GOL, tăcut — cablarea s-a mutat în `BuildDetailRows()` din constructor. `image_list` n-are încă cheile `neutru`/`plus` (cad pe GDI); `nrdoc`/`data` sunt `ValueType.DateTime` în designer dar `FillGrid` scrie șiruri în ele (probabil scăpare, lăsat neatins) |

### Current focus

- **Slice 0017-04 (Plăți: arborele pe două niveluri) — DONE și VĂZUT PE ECRAN 2026-08-13.**
  Prima acceptare vizuală a vederii Plăți. Trei lucruri merită scoase din ea. (1) **Designerul
  Visual Studio a devenit a doua sursă de adevăr, iar codul nu știa asta** — trecerea operatorului
  autorase cele cinci coloane ale grilei, `BuildColumns()` le adăuga a doua oară, și regula „fără
  no-op tăcut" a transformat asta într-o excepție care dobora activarea vederii. Când designerul
  autorează ceva, codul trebuie să înceteze să o facă. (2) **Ce scrii de mână în
  `InitializeComponent` moare la primul dute-vino prin designer** — cele zece apeluri
  `InitDetailPair` fuseseră șterse de o regenerare, iar panoul de detaliu ieșea gol fără nicio
  eroare; cablarea stă acum în constructor. Aceeași capcană e de verificat în `IstoricView` și
  `DdfView`, ale căror designere au fost regenerate în aceeași rundă (vezi Open threads).
  (3) **Trei capcane de test, toate generale:** `Type.GetMethod` e sensibil la majuscule deși VB
  nu e (o redenumire de handler a arătat exact ca un defect de vedere); `Throw failure` resetează
  stiva și ascunde locul real al eșecului (`ExceptionDispatchInfo.Capture(...).Throw()` nu);
  `ImageList.Images(i)` întoarce un `Bitmap` NOU la fiecare acces, deci `Assert.Same` nu poate
  trece niciodată pe iconițe — comparația se face pe pixeli.

### Open threads

- **„Fix Rezervări «+» (one leaf, not all)" a rămas fără număr.** Era planificat ca 0017-04, dar
  operatorul a realocat 04 reveniri asupra arborelui de Plăți. Elementul **nu s-a făcut** și are
  nevoie de un număr nou când se programează.

---

## Slice 0018

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0018 | `KBotNavList` — separatori + orientare + aliniere Near/Far + vizibilitate; gating MainForm prin ascundere | DONE (code) / **NO VISUAL VERDICT** | `SLICE-0018-navlist-separators-orientation-visibility.md` | Four capabilities added to `KBotNavList` (`KBot.Theming`): (1) **separatori** — `AddSeparator(Optional align)`, oricâți, desenați ca linie fină (`Palette.BorderColor`), neselectabili, săriți de mouse/tastatură; cheie internă auto (`__sep_N`) ca să nu colideze cu `FindIndex`; (2) **orientare** — `Orientation` (`KBotNavOrientation.Vertical` implicit / `Horizontal`); orizontal măsoară lățimea butonului din text, centrează textul și remapează navigarea pe Stânga/Dreapta; (3) **aliniere Near/Far** — enum nou `KBotNavAlign` pe `AddItem(key,text,align)` și `AddSeparator(align)`; `Far` ancorează grupul **jos** (vertical) / **dreapta** (orizontal); (4) **vizibilitate** — `SetItemVisible(key,visible)`, un buton ascuns nu ocupă spațiu, nu se pictează, nu se selectează, e sărit de tastatură (rezolvă firul deschis „`KBotNavList` has no `SetItemVisible`"). Layout-ul e acum un singur `RecalcLayout` (invalidat la add/vizibilitate/orientare/resize/font), nu vechiul `index * height`; grupul Far se așază de la `capăt - extindereTotală`. **`MainForm`:** DDF/ORD mutate în grupul `Far` cu `AddSeparator(Far)` deasupra (desprinse la baza barei); **`ApplyViewGating` trecut de la `SetItemEnabled` la `SetItemVisible`** — un flag `Are*=FALSE` acum **ASCUNDE** butonul, nu doar îl dezactivează (cererea operatorului). Fallback-ul la `sumar` când vederea activă dispare rămâne. `KBot.Theming` + `KBot.App` compilează curat, 0 warnings. ⚠️ **Fără verdict vizual** — verificat doar prin compilare; harness-ul controalelor UI rămâne nerulat (ca 0010+); ~~**fără teste xUnit pentru `KBotNavList`** (nu existau înainte) — separator/vizibilitate/aliniere/orientare neacoperite de teste~~ **REZOLVAT în felia 0025** (`KBotNavListTests.vb`, 20 de teste: separatori + chei `__sep_N`, ordinea Near/Far verificată geometric, «ascuns ⇒ `Rectangle.Empty`, sărit de `IndexAt` și de navigarea cu tastatura»). Rămâne fără verdict vizual |

---

## Slice 0019

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0019 | XFA_WRITTER → `KBot.Xfa` (librărie în proces, nu exe separat) | DONE (code) / **NEVER RUN LIVE** / **NO VISUAL/PDF VERDICT** | `SLICE-0019-xfa-writter-library.md` | Portat exe-ul `XFA_WRITTER` din `Surse/SURSA_XFA_WRITTER` în `src/KBot.Xfa` ca **librărie apelată în proces** — K-BOT nu mai lansează un `.exe` separat cum făcea Access prin `WScript.Shell.Run` + cod de ieșire. API public = **`XfaWriter`** (fațadă ce înlocuiește `Program.Main`): `Genereaza` / `GenereazaSiSemneaza` / `Semneaza` / `ExtrageAtasamente`, întorcând un **`XfaResult`** (`Reusit`, `Semnat`, `Masca`, `SemnatAB/CD/Ordonator`, `CaleOutputPdf`, + `CodIesireLegacy` 0/2/11..17 pentru compatibilitate). `docType` ∈ {DDF, ORD}. Motorul `AdobeUtils` (iTextSharp 5.5.13.3 `PdfReader`/`PdfStamper`/`XfaForm`): completează XFA din XML, embedează atașamente base64, deschide Adobe și **așteaptă** semnarea (fără timeout, `WaitForExit`), calculează masca semnatarilor din handler-ul `.pdf` din registry (+ validare că e Adobe). `TemplateDownloader` = cache DDF/ORD de pe API-ul **legacy** (`Configs`: adcredit.avatarsoft.ro:5008 + `X-API-KEY`, `c:\avacont\cache`) — separat de API-ul K-BOT bearer. **Adaptări la regulile casei:** fără bloc `Namespace` (`AdobeUtilsNS` scos); `Logger`→modul `XfaLog` (fără `AllocConsole`, trasare în `C:\AVACONT\logs`); granițele loghează în `GlobalErrorLog` + rearuncă, deci `KBot.Xfa` referă acum `KBot.Common` (fără ciclu — Common referă doar Domain); comentarii/șiruri românești. **Două capcane rezolvate:** (1) `Option Strict On` rupe `PdfStamper(…, "\0", True)` original → `ChrW(0)` (sentinela iTextSharp „păstrează versiunea machetei"; exe-ul pe Option Strict Off trimitea de fapt un backslash); (2) shadowing VB case-insensitive — Sub `DeschidePdf` vs param `deschidePdf` → Sub redenumit (aceeași capcană ca la 0010/`ProposedValue`/`HeaderText`). **Modurile cu semnare BLOCHEAZĂ** (deschid Adobe) → din UI rulează-le pe thread de fundal. iTextSharp/BouncyCastle sunt pachete .NET Framework pe net8.0-windows → NU1701 e așteptat, rulează corect (aceeași versiune ca exe-ul). `FileVersion` 1.0.0.0→1.1.0.0. **Cum se rulează din K-BOT:** DevHarness → categoria **XFA** (`XfaWriterHarnessTest` în `KBot.App/HarnessTests`, doar Debug, descoperit prin scanarea assembly-ului de intrare) — DOAR logica pură offline (separare atașamente base64 + clasificare mască ORD/DDF). Proiect `tests/KBot.Xfa.Tests` xUnit adăugat (**39 teste**: `ExtrageAtasamente` + `MascaSemnatari` ORD/DDF + `XfaResult` coduri 0/2/11..17), adăugat în `KBot.sln`; soluție întreagă verde (0 erori). ⚠️ **Deschis:** (a) generarea completă a PDF-ului cere macheta de pe serverul legacy — niciodată rulată live; (b) semnarea cere Adobe ca handler implicit `.pdf` — neverificată; (c) niciun PDF real nu a fost produs/inspectat; (d) testele acoperă DOAR logica pură (separare atașamente + mască) — motorul XFA propriu-zis (`ModifyXfaFromXml`, clonare `SubformInf`, embed) cere o machetă PDF reală, neacoperit; (e) vederile editor DDF/ORD rămân amânate (vor apela `XfaWriter`) |

### Open threads

- **No real DDF PDF produced or opened** (0019 open items a/c, still open after 0020-05). The
  generation flow is wired and unit-tested, but `XfaWriter.Genereaza` needs the machete + Adobe;
  the "Adobe opens the generated PDF correctly" verdict is outstanding.
