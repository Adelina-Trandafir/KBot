# K-BOT — project state (snapshot)

_Last updated: 2026-07-17._

High-level "where are we right now" snapshot for quick orientation. **The detailed,
authoritative record is [`docs/worklog/KBOT_STATUS.md`](docs/worklog/KBOT_STATUS.md)**
(slice registry + open threads) and the per-slice worklogs in `docs/worklog/`. When this
file and `KBOT_STATUS.md` disagree, `KBOT_STATUS.md` wins — fix it there first.

## What just landed (most recent first)

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
- Missing MariaDB DDL for four `FX_*` flag tables; the two `FX_Angajamente` DDLs disagree
  on `ASCUNS` (see `KBOT_STATUS.md` open threads).

## Build / test

```powershell
dotnet build KBot.sln
dotnet test KBot.sln          # 84 green: Api 30, App 18, Theming 27, Common 7, Domain 1, LocalStore 1
# Python server tests via PYTHON\.venv; live-DB tests skip off-host
```
