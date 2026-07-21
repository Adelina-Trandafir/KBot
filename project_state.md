# K-BOT — project state (snapshot)

_Last updated: 2026-07-21._

High-level "where are we right now" snapshot for quick orientation. **The detailed,
authoritative record is [`docs/worklog/KBOT_STATUS.md`](docs/worklog/KBOT_STATUS.md)**
(slice registry + open threads) and the per-slice worklogs in `docs/worklog/`. When this
file and `KBOT_STATUS.md` disagree, `KBOT_STATUS.md` wins — fix it there first.

## What just landed (most recent first)

- **`KBotDataView` — owner-drawn unbound grid, slice 0010 complete** (`00b0cc9` → `3d69e2a`, 2026-07-18…21)
  - A reusable, **unbound**, **virtualized**, owner-drawn grid mimicking an Access continuous
    form — the shared list widget for the real views. Built in seven passes (worklogs
    `docs/worklog/SLICE-0010-0{1..7}-*.md`); see `KBOT_STATUS.md` slice 0010 for the full detail.
  - **01 skeleton** (models + `IThemedControl` + themed header/body) · **02 render+virtualize**
    (frozen + scrolling bands, integer virtualization, scrollbars, Text/CheckBox) · **03** the
    other four column types (Combo/OptionButton/Button/ProgressBar) + `OptionGroup` exclusivity ·
    **04** three-level effective-enabled (`IsCellEnabled`/`IsRowEnabled`) + disabled rendering +
    conditional formatting · **05** selection + Access-style keyboard nav + click/toggle + header
    resize · **06** in-place editing (floating Text/Combo editor, `CellValidating` veto+coercion,
    corrected `IsDirty` = operator-edits-only) · **07** `ScrollByColumn`.
  - **Placement decision:** lives in `KBot.Controls` (next to `AdvancedTreeControl`) but gained a
    `KBot.Theming` ProjectReference (no cycle) so it self-themes via `IThemedControl`/`ThemePalette`
    — the original plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade).
  - Virtualization proven **headlessly**: painted-row count is identical at 5,000 and 50,000 rows.
  - Editing pass caught **2 real VB case-insensitivity bugs** (a param shadowing a same-named
    property → silent no-op): `HeaderText` (every column header had been `Nothing` since pass 01,
    invisible to headless tests) and `ProposedValue` (commit always wrote `Nothing`). Both fixed
    with `Me.` + regression tests. See the [[vbnet-case-insensitive-shadowing]] pattern.
- **`KBotDataView.ScrollByColumn`** (`1ad2ff5`, fix `3d69e2a`, 2026-07-21)
  - New property: horizontal scroll snaps to column edges (a whole column at a time) instead of
    per-pixel. Vertical is untouched (already row-quantised by virtualization).
  - Arrows / trough / wheel snap **directionally** (a small step still advances one full column).
  - **Thumb drag** scrolls **freely** while dragging and snaps to the **nearest** edge only on
    release — the first version snapped on every `ValueChanged` mid-drag, which made the thumb
    jitter ("refreshes horribly, trying to decide which column"); fixed via the `Scroll` event's
    ThumbTrack/EndScroll distinction. DevHarness got a «Derulare pe coloană» checkbox.
- **AdvancedTreeControl → design-time control** (uncommitted, 2026-07-21)
  - Same treatment `KBotDataView` got: class marked `<ToolboxItem(True)>` +
    `<DefaultProperty("HeaderCaption")>`, so it drops from the VS Toolbox onto a form.
  - Constructor is now design-time safe — the `TooltipPopup` (a real `Form`) is no longer
    created / handle-forced under `LicenseManager.UsageMode = Designtime`; it's built lazily
    at runtime instead (all callers already null-guard it).
  - Property grid organized via `<Category>`/`<Description>`/`<DefaultValue>` into
    _K-BOT Arbore_ + _· Culori / Antet / Căutare / Tooltip / Coloane_. Runtime-only members
    hidden (`SelectedNode`, `OldSelectedNode`, `Items`, resolved header `Image`s,
    `TooltipPopupHandle`, redundant `FontName`/`FontSize`).
  - Pure metadata + a designer-only guard — no runtime behavior change. Full suite green.
  - **Not verified in the actual VS designer** (can't drive it headless); mirrors the
    known-good DGV setup. Note: the control still has **no `Dispose` override** (timers,
    `_vScroll`, tooltip `Form` never torn down) — pre-existing, left as-is.
- **Tree UI polish + Internal Info popup** (`b8c558c`, 2026-07-17)
  - Left status icons now render in the flat list (flat mode forced `Expanded=True`, so the
    control drew the never-set `LeftIconOpen`; it now falls back to `LeftIconClosed`).
  - The hover refresh icon no longer overlaps the `CodAngajament` column — opt-in
    `AdvancedTreeControl.ReserveRightIconSpace` reserves the icon's width; MainForm opts in.
  - New **non-modal `InternalInfoForm`** (themed, borderless): shows every
    `AngajamentTreeInfo` field incl. all nine `Are*` flags for the selected node. Opened
    from the `ⓘ` button in the tree header; auto-refreshes on tree selection + has its own
    Reîmprospătează button.
- **Slice 0009 — tree orphan escape + client tests** (`14770ea`, 2026-07-17)
  - Server: `GET /api/forexe/tree` SS filter now keeps zero-indicator (orphan) angajamente
    visible (`EXISTS SS OR NOT EXISTS any indicators`).
  - Client: the `.NET` `GetTreeAsync` tests that slice 0008 never wrote (Api 26 → 30).
  - Slice 0009's Parts B/C/D were already shipped by slice 0008; see the worklog.

## What works today

- Auth / login (bearer token; DC + An/SS periods; 401 → re-login retry once).
- ListaAngajamente scrape → upsert (offline round-trip; live still needs the table + env).
- Theming engine (Classic / Dark / Modern, live switching).
- MainForm shell: header (unit, An/SS, Forexe dot), nav sidebar, tree card, view host,
  status bar (Istoric / **Sincronizare**).
- **Angajamente tree**: `GET /api/forexe/tree` bound in MainForm, An/SS-driven reload,
  `btnOpt` hidden toggle, per-node `Are*` gating of nav entries, status icons, and the
  Internal Info popup.
- **`KBotDataView`**: reusable owner-drawn grid (six column types, virtualized, themed,
  editable, `ScrollByColumn`). Consumed by the real views that have landed (Sumar, Rezervări).

## What's next / deferred

- **The nine real views are still `PlaceholderView`** (Sumar, Indicatori, Istoric, Revizii,
  Rezervări, Partener, Recepții, Plăți, DDF, ORD). This is the "tabs aren't wired to data
  yet" gap — the Internal Info popup is the current way to see live per-node data.
- `btnSort` / `btnIstoric` are placeholder `MsgBox` stubs.
- `KBotNavList.SetItemVisible` (real hide vs. grey-out gating).
- The tree is a flat list, not a nested tree.
- `GET /api/forexe/angajamente` + `GetAngajamenteAsync` are now tree-unused (kept, no caller
  removed).

## Known-unverified (needs a real environment)

- **No part of the tree endpoint has run against a live database** — all route tests
  (incl. the two new orphan tests) are host-only and skip off-station.
- **The tree UI fixes above were not eyeballed in a running MainForm** (needs login +
  server + DB); they build clean and the full `.NET` suite is green, but confirm visually.
- **`KBotDataView`'s visual harness has never been run** — for any of the seven passes. Scroll
  smoothness (incl. `ScrollByColumn` thumb release), the WinForms key→handler path, resize drag,
  floating-editor placement and the actual colours are all unconfirmed. The tests cover the
  logic; the pixels don't. The blank-header bug (headers `Nothing` for five passes, caught only
  in pass 06) is the argument for running it: DevHarness (Debug start → «Nu») → Controls/UI →
  «KBotDataView — virtualizare + temă (5.000 × 20)».
- Missing MariaDB DDL for four `FX_*` flag tables; the two `FX_Angajamente` DDLs disagree
  on `ASCUNS` (see `KBOT_STATUS.md` open threads).

## Build / test

```powershell
dotnet build KBot.sln
dotnet test KBot.sln          # 219 green: Api 42, App 30, Controls 111, Theming 27, Common 7, Domain 1, LocalStore 1
# Python server tests via PYTHON\.venv; live-DB tests skip off-host
```
