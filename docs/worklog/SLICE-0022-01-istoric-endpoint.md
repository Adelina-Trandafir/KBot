# SLICE-0022 — Istoricul Istoric

Reunire documentară D001-26. Revizuirea din 24.07.2026 din 02 elimină cele șapte coloane de valori și totalurile din grila principală, mutând valorile nenule în detaliu; DTO-ul și endpoint-ul rămân la contractul lor de etapă. Cheile și riscurile de nomenclator din iulie sunt istorice, anterioare contractului 0080. Rezultatele și restanțele sunt păstrate pe etapă, fără rulări noi.

[0022-01](#slice-0022-01) · [0022-02](#slice-0022-02)

<a id="slice-0022-01"></a>

## SLICE-0022-01 — server + client: `GET /api/forexe/istoric`

Pass 1 of slice 0022 (vederea Istoric), per `PLAN_IstoricView.md` §2–§3. Ships the endpoint, its host-only tests, the `KBot.Domain` POCOs and `GetIstoricAsync` on the API client. **No UI in this pass** — `MainForm.CreateView("istoric")` still returned a `PlaceholderView` when this pass was written; the view lands in 0022-02.

> **Worklog naming.** `CODE_WORKFLOW.md` §3.2 requires multi-pass slices to sub-number their
> worklogs (`SLICE-0007-01-…`), which is also what 0015/0017/0020 did. Followed the standing rule.

### What changed and why

One round trip returns `{cod, randuri[], clasificatii[]}`. `IstoricView` builds the grid, three column menus and filters locally; filter changes issue no requests (0020-01 §7).

- **`randuri`** — `SELECT … FROM FX_Istoric WHERE CodAngajament = %s ORDER BY DataFX, Clsf`. The `ORDER BY` is the Access `RecordSource`'s, verbatim. Nineteen wire fields (§2.1). `cod` is an **exact** match, parametrized (Access used `Like` — deviation, see §9 in 0022-02).
- **`data_fx` keeps its time component** (§2.3). `FX_Istoric.DataFX` is a `datetime` and istoric rows are timestamped events. A dedicated `_iso_dt` returns the full ISO value **with** the time — deliberately NOT the `_iso` from `ddf.py`, which truncates to the day (right there, wrong here). Pinned by `test_data_fx_carries_time_component`.
- **`Clsf` needs no join** (§2.4). `FX_Istoric.Clsf` is a stored TEXT column, written at parse time by `FX_Istoric_Prelucreaza_Observatii` from `FX_Indicatori`. Read it directly. The view is therefore **not** exposed to the `Clasificatii` duplicate problem for display — only the filter hierarchy touches the nomenclator.
- **The classification key points the opposite way from DDF** (§2.5). `FX_Istoric_Prelucreaza_` `Observatii` writes `Rs!IdClsf = rcInd!IdClsf` straight from `FX_Indicatori`, which holds the **Access** id. So `FX_Istoric.IdClsf` matches `Clasificatii.IdClsfAcc` (NOT `IDClsf`, as DDF's `FX_DDF_REV_SA` does), and needs the `IdUnitate` predicate — which `FX_Istoric` has no column for, so it comes through `FX_Indicatori` on `CodAngajament`, exactly as the Access filter (`bFilter_Click`) did. Getting this backwards yields an **empty filter menu, not an error** — pinned by `test_key_resolves_via_idclsfacc_not_idclsf` (a row where `IDClsf` ≠ `IdClsfAcc`).
- **`clasificatii` is deduped on `IdClsfAcc`** (§2.2). The nomenclator has real duplicates (0011-03 measured pairs at `(75,79/84/90/92/93)`). This is a **list**, not a scalar lookup, so `LIMIT 1` does not apply — `GROUP BY c.IdClsfAcc`, one entry per distinct value. Both predicates (present-in-`FX_Istoric` **and** the angajament's `IdUnitate` via `FX_Indicatori`) are from the Access `bFilter_Click` query and are mandatory. `IdUnitate` is **kept** — `Clasificatii` is a shared nomenclator, not an `FX_` table, so the drop-`IdUnitate` rule (0011-03) does not apply.

#### ⚠️ Host-verification risk: the two caption tables (`DefaClsfF` / `DefaArticol`)

§2.2 assigns the two parent-menu captions to `AVACONT_COMUN.DefaClsfF.Denumire` (keyed on `Clasificatii.ClsfF`) and `AVACONT_COMUN.DefaArticol.Denumire` (keyed on `Clasificatii.Articol`). **The only ground truth in the repo — the Access query `qFX_DDF_SA_CLSF` — disagrees on the names:** it reads `DefaClsfF.Explicatie` (not `.Denumire`) and `DefaTitlu2.Denumire` (there is no `DefaArticol` table in the Access export). The MariaDB `AVACONT_COMUN` schema for these nomenclator tables is **not in the repo** (only `CAI`/`FX_LoginLog` are in `sql/avacont_comun_login.sql`), so I could not verify whether the migration renamed `Explicatie→Denumire` / `DefaTitlu2→DefaArticol`.

Decision: **implemented to the plan** (it is the approved spec, and §2.2 explicitly frames these as a decided caption source), using `LEFT JOIN` so a *missing caption* degrades to `NULL` rather than dropping the row — but a *missing table/column* makes the whole endpoint fail loudly (500), which is the behaviour §2.2 asks for ("fail loudly rather than return empty captions"). **This is the #1 thing the host run must check.** If the real column is `Explicatie` / the real table is `DefaTitlu2`, this one SQL string is the only change.

- **Alineat caption = `Clasificatii.Denumire`** (per-unit, no join) — an **operator decision** over `AVACONT_COMUN.DefaClsfE.Denumire` (national standard). `qFX_Clsf2026_Structura`'s definition is lost, so neither could be proven faithful; recorded as a decision, not a finding.

#### Deliberately NOT on the wire (§2.1)

`Utilizator`, `HASH`, `Prelucrat`, `DTQ`, `Val_Receptie_T`, `Rez_Ord` — none appears on the Access form. `Val_Receptie_T`/`Rez_Ord` exist in the DDL but on no Access control; adding them would be inventing a view. `test_row_shape_matches_contract` asserts none of them leak.

### Client (`KBot.Domain` + `KBot.Api`)

- `KBot.Domain/IstoricInfo.vb` — `IstoricInfo` (`Cod`, `Randuri`, `Clasificatii`), `IstoricRand` (`DataFx As Date?` keeps the time; `Idrev As Integer?` — many rows have no revision), `IstoricClasificatie`. snake_case stops at the wire DTO. `FileVersion` 1.0.1.0 → 1.0.2.0.
- `KBot.Api` — wire DTOs `GetIstoricResponse` / `GetIstoricRandRow` / `GetIstoricClasificatieRow` (snake_case verbatim), `GetIstoricAsync(cod)` matching `GetDdfAsync`/`GetPlatiAsync` (no `db_name` on the wire, bearer from session, empty arrays for an unknown cod, hard-fail on non-2xx). The five `FakeApiClient`s in `tests/KBot.App.Tests` got a `GetIstoricAsync` stub. `FileVersion` 1.0.1.0 → 1.0.2.0.

### Tests

- **Python** `tests/test_forexe_istoric.py` — 12 host-only tests (skip cleanly off-host): 401 guard, missing/blank cod → 400, unknown cod → 200 with empty arrays, row shape (every §2.1 field present, none of the excluded ones), `ORDER BY DataFX, Clsf`, negative values keep sign, `data_fx` carries time, `clasificatii` scoped to present `IdClsf` and the angajament's unit, **no fan-out** (one entry per `IdClsfAcc` with a duplicate nomenclator row), **key direction** (`IdClsfAcc` not `IDClsf`), literal diacritics, and the **finding test** (§8.12) that counts `(IdUnitate, IdClsfAcc)` groups with >1 distinct `Clsf` — never fails, always reports, closing the `Status_migrare_5 §9` open item on every host run.
- **.NET** `ApiClientTests` — `GetIstoricAsync`: url/bearer/escaping, blank-cod-throws-before- request, both arrays + time-preserving + nullables, empty-is-empty, 401 → `ApiException` with reason. Api 63 → 68.
- Offline: **.NET 436 passed / 0 failed**, build 0 errors / 0 new warnings (16 pre-existing NU1701); **Python 75 passed / 15 skipped / 0 failed** (`test_forexe_istoric.py` = the +1 skip).

### ⚠️ Open

- The endpoint has **never run against a live database** — all 12 Python tests are host-only and were seen only skipping.
- The `DefaClsfF`/`DefaArticol` name risk above is unresolved until the host run.
- The finding test's answer (`Status_migrare_5 §9`) is unknown until it runs on `000_DEMO`.

---

<a id="slice-0022-02"></a>

## SLICE-0022-02 — `IstoricView` + `IstoricFilter` (the fifth real hosted view)

Pass 2 of slice 0022, per `PLAN_IstoricView.md` §4–§7. `MainForm.CreateView("istoric")` no longer returns a `PlaceholderView`. Read-only, no writes anywhere.

Access source was a **popup dialog** (`frmFX_ISTORIC`); here it is a **hosted view** in the shell, like Sumar/Rezervări/Recepții/Plăți/DDF. It shows every `FX_Istoric` row for the selected angajament, with the three column filters Access had, a three-column totals row, and a detail pane for the two long text fields.

### Layout (§4.1)

`pnlFiltre` (Dock=Top, filter strip) over a horizontal `SplitContainer`: grid (`KBotDataView`, read-only, totals row) on top, `pnlDetaliu` (two read-only multiline textboxes side by side) at the bottom. `lblEmpty` is topmost (Fill) and covers everything in the empty/loading state. All controls declared in `IstoricView.Designer.vb` (house rule). Dock-order trap respected: `split` added before `pnlFiltre`.

### Grid (§4.2)

Twelve columns in Access `Left` order: `Clsf`, `TipRand`, `Data` (`dd.MM.yyyy`), the seven money columns, `Descriere`, `Observații`. Money columns store a `Double`, `FormatString = "N2"`, right-aligned — same idiom as `PlatiView`, read from it rather than invented. **Totals row on EXACTLY the three columns Access totalled** (`tot_Val_Rezervare_Dif`, `tot_Val_Receptie`, `tot_Val_Plata` → `vrdif`/`vrec`/`vpl`, `Aggregate = Sum`); totalling the other four would show a figure the source never showed. `Descriere`/`Observatii` carry `AutoHide = True` (0016) — they auto-hide first because the detail pane carries them in full.

### Detail pane (§4.3)

Selection fills read-only `txtDescriere`/`txtObservatii` from the row Tag (`IstoricRand`); no selection clears both. The pane carries the second Access continuous-form band, keeping `KBotDataView` at one line per row.

### The three filters

**`IstoricFilter`** (`KBot.App/Views/IstoricFilter.vb`) — a **pure class, no UI references**, a faithful port of `mdl_FX_Popups.ApplyColumnFilter`: three independent segments (`Clsf`, `TipRand`, `DataFx`); setting a segment **replaces** it; "TOATE" **clears only that segment**; the effective filter is the AND of the non-empty segments; `Apply(randuri)` returns the filtered list. `Clsf` = a set of `id_clsf`; `TipRand` = exact value or a `Rez_`/`Plata_` prefix group, matched **case-insensitively**; `DataFx` = month+year or an exact day.

The three menus are themed `ContextMenuStrip`s built from the loaded data (this replaces Access's hover-following `bFilter` button — deviation §6 below):

- **Clasificații** — hierarchical Subcapitol → Articol → Alineat from `clasificatii[]`. Root "TOATE"; a per-level "TOATE" only where that level has **more than one** child (the Access `dictArtPerSub`/`dictAlinPerArt` rule). Captions from the three `den_*` fields.
- **TipRand** — distinct `tip_rand` from the loaded rows. Submenu "REZERVĂRI" for `Rez_`, "PLĂȚI" for `Plata_` (both case-insensitive), rest flat. Root "TOATE"; a per-group "TOATE" only when the group has more than one entry (Access `rezCount > 1`); group "TOATE" filters by prefix. The `+` variants (`Rez_Initiala+`, …) are distinct values and get their own entries — correct, they mean something different from the unsuffixed ones.
- **DataFX** — `AnLuna` (`MM-YYYY`) → `Ziua`. Root "TOATE"; a per-month "TOATE" only when the month has more than one day (Access `dictZiPerAL`).

`lblFiltruActiv` shows a short summary of the active segments; `btnReset` clears all three.

### Loading (§6)

`SetContext(info)`: `Nothing`/blank cod clears grid, pane and menus without a request. Otherwise `_requestedCod` guards `WithReauth(Of IstoricInfo)` responses against context changes, as in `DdfView`. Success fills the grid, rebuilds all three menus and unconditionally resets all filter segments, preventing carryover between angajamente.

### `RandSchimbat` — wired, dormant, deliberate (§7)

Access stored row `ID` in hidden `CL` on `Form_Current`; its `Public Event RowChanged(key)` had the `RaiseEvent` commented out. Here selection raises `RandSchimbat(Id)` with **no subscriber**, deliberately retained like `AdaugaDdfCerut` in Rezervări.

### Theming

`IstoricView` implements `IThemedControl`; `ApplyTheme` styles the strip/detail/empty state, `ButtonStyles.ApplySecondary` on the four buttons, and themes the three `ContextMenuStrip`s (which are **not** in the control tree, so `ThemeManager.Apply` cannot reach them) via a palette-derived `KBotMenuColorTable : ProfessionalColorTable`. The grid self-themes (it implements `IThemedControl`).

### The nine deviations from Access (§9 of the plan)

1. **TipRand and DataFX menus are scoped to the current angajament.** Access queried `FX_Istoric` unscoped, listing values from every angajament in the database — a bug, not a feature.
2. **TipRand matched case-insensitively** — Access got this free from `Option Compare Database`; the writer emits both `Rez_Initiala` and `PLATA_PLATA`.
3. **`cod` is an exact match.** Access used `Like`.
4. **The two-band Access row is flattened** to one grid row plus a detail pane.
5. **Alineat caption = `Clasificatii.Denumire`** (operator decision; `DefaClsfE.Denumire` was the alternative).
6. **The hover-following `bFilter`** is replaced by a filter strip with three menu buttons — an Office `CommandBars` idiom with no WinForms equivalent worth reproducing.
7. **Six `FX_Istoric` columns are deliberately not on the wire** (§2.1 — see 0022-01).
8. **`clasificatii` is deduped on `IdClsfAcc`** (see 0022-01).
9. **`data_fx` keeps its time component**, unlike DDF's `_iso` (see 0022-01).

#### Two Access bugs NOT ported (§5.3)

- `bFilter_Click` builds `"SELECT * qFX_Clsf2026_Structura …"` — **no `FROM`**; the classification filter has never run in Access as exported. Nothing to be faithful to.
- `Popup_DataFx_Function` applies to `Forms!frmFX_MAIN.fxIstoric` while the other two apply to `Forms!frmfx_Istoric` — two different form objects. Here all three act on one grid.

### Tests

- **`IstoricFilterTests`** (pure, no STA) — replace-within-segment, accumulate-across-segments, "TOATE" clears one and leaves two, `Rez_`/`Plata_` case-insensitive grouping (`PLATA_PLATA` under `Plata_`), `Rez_Initiala` vs `Rez_Initiala+` distinct, month vs day segment, `ClearAll`.
- **`IstoricViewTests`** (STA thread + `Application.DoEvents()`, as Sumar/Rezervări/DDF) — blank context makes no call + clears grid/pane; response fills the grid with all rows; a stale response is discarded; the totals row runs on **exactly** the three Access columns; a new context rebuilds the menus and **resets all three filter segments**; the detail pane follows selection (no selection → empty); `RandSchimbat` carries the selected row's `Id`.
- 16 new `KBot.App.Tests` (App 120 → 136). **Test bug caught, not product:** the `Friend WithEvents` menu backing field is `_menuTipRand`, not `menuTipRand` — the reflection helper now reads `_<name>` first. `KBot.App` `FileVersion` 1.0.2.0 → 1.0.3.0.
- Full solution: **.NET 436 passed / 0 failed**, build 0 errors / 0 new warnings; **Python 75 passed / 15 skipped / 0 failed** offline.

### ⚠️ Open

- **No visual verdict** — the view has never been seen on screen; the harness stays the only way to get one. Menu layout, the three `ContextMenuStrip`s' theming, the flattened two-band row and the auto-hide of Descriere/Observatii at real width are all unverified.
- Never run against a live database — the view has only seen hand-built `IstoricInfo` objects.
- The endpoint's `DefaClsfF`/`DefaArticol` caption risk (0022-01) means the classification **menu captions** are the least-certain part until the host run; filtering itself keys on `id_clsf` and is unaffected.
- `RandSchimbat` is raised but unsubscribed by design.

### Revizuire operator (2026-07-24)

Four grid/detail changes requested after first review:

1. **The seven value columns were removed from the main grid.** It now shows five columns: `Clasificație`, `Tip rând`, `Data`, `Descriere`, `Observații`.
2. **`Observații` is last and stretches** (`ColumnFillMode.LastColumn`, already set).
3. **The values moved into a small grid in the detail pane.** The bottom-left pane is now `Descriere` (left) + a two-column `KBotDataView` `gridValori` (`Tip` | `Valoare`, right) that shows **one row per non-zero value** of the selected row — an on-the-fly unpivot/crosstab over the seven money fields, in Access order (Rezervare inițială/definitivă/anterioară/diferență, Angajament legal, Recepție, Plată). No non-zero value ⇒ empty grid.
4. **The totals row was removed** (`ShowTotalsRow = False`; the Sum aggregates are gone with the columns). The three-column totals of the original design no longer apply.

The `IstoricRand` DTO and the endpoint are **unchanged** — the seven money fields are still on the wire and in the POCO; only the *presentation* moved from grid columns + totals to the detail grid. Tests updated: `GridColumns_FiveNoValueColumns_NoTotals` (was the totals-columns test) and `DetailPane_FollowsSelection_DescriereAndNonZeroValues` (was txtDescriere/txtObservatii). Full .NET suite still 436 green.

## Repere unice din registrul 0022

Rândul registrului numea Istoric a cincea vedere găzduită, iar focusul «seventh real view (the sixth was DDF)»; sunt două formulări istorice păstrate, fără recenzarea ordinii implementărilor. Registrul desfăcea cele 16 teste App în 8 filter + 8 view; bilanțul de 436 era App 136, Api 68, Controls 134, Theming 27, Xfa 39, Domain 17, Common 14, LocalStore 1. Mențiunea scurtă «0 warnings» din focus era detaliată ca zero BC și 16 NU1701 preexistente în probele de etapă.

---

Consolidare documentară D002-20: completarea din 29.09.2026 a aceleiași vederi Istoric este păstrată după etapele inițiale din iulie. Nodul «Tot istoricul» elimină inclusiv intervalul FOREXE, fără a adăuga o confirmare vizuală sau live.

<a id="slice-0095"></a>

## SLICE-0095 - Istoric: root node «Tot istoricul»

Operator request, 29.09.2026: the Istoric tree gets a root level «Tot istoricul», like the other trees (Extrase has «Toate extrasele»). Clicking it does what the view does on load: every FX_Istoric row of the angajament.

### Change

`src/KBot.App/Views/IstoricView.vb` only.

- `BuildTree`: a bold, expanded root `"all"` with caption `Tot istoricul~~~<row count>` (count = ALL rows, dated or not, the same number the grid shows on load). Months hang under it (collapsed, as before); days under months (unchanged).
- `NodPerioada.Tot()` factory + `EsteTot` flag for the root's `Tag`.
- `Tree_NodeMouseUp`: root -> `_filter.ClearAll()` + refill (the same clearing `LoadAsync` does). Month / day nodes unchanged.

### Notes

- On load the view still behaves as before; only an interval requested by FOREXE (slice 0073) narrows it. Clicking the root drops that interval too - it is «everything», like «Reset».
- Undated rows had no tree node before; the root now reaches them.

### Status

Build of KBot.App clean (0 errors). Not run, not seen on screen, no tests (operator's rule). KBot.App FileVersion not bumped here - `push-update.ps1` asks for it.
