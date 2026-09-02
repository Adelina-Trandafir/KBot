# K-BOT

WinForms (.NET 8, VB.NET, `Option Strict On`) rewrite of the Access FX/Forexe system,
plus a Flask API server (`PYTHON/`). The Access source of truth is exported at
`C:\AVACONT\FX_System_Export` (FORMS / QUERIES / TABLES / MODULES as .md).

## RULE 0 — PRIORITY 0 — no diacritics in code

**No diacritics (a-breve, a-circumflex, i-circumflex, s-comma, t-comma) ANYWHERE
in code. The only exception is text the operator actually reads on screen.**
This rule outranks every other rule in this file, and it outranks the style of
any existing file: where old code disagrees, the old code is wrong and gets
swept, never copied.

The test before writing any name or comment: **does the operator SEE this
string?** If not, it is ASCII. It applies on BOTH sides of the wire — the Flask
routes and the VB clients must agree on ASCII field names.

## Solution layout (`src/`)

| Project | Role |
|---|---|
| `KBot.App` | WinForms shell: `Program` (DI via Microsoft.Extensions.DependencyInjection), `LoginForm`, `MainForm`, `Views/` |
| `KBot.Api` | HTTP clients: `ApiClient` (`IApiClient`), `AuthApi` (`IAuthApi`), `ApiOptions` (https-only, hostname constant) |
| `KBot.Common` | `SessionContext` (singleton, replaces VBA globals), `GlobalErrorLog`, `WorkflowCatalog`, `ExcelJob` |
| `KBot.Domain` | POCOs + mapping: `Angajament`, `AngajamentMapper`, `AngajamentTreeInfo`, `Auth/` (LoginResult, PeriodInfo, UnitInfo…) |
| `KBot.Forexe` | Playwright-driven FOREXE robot: `ForexeRunner` (`IForexeRunner`), `JobBuilder`, `RichTextBoxLogger`, `KBotTheme` facade |
| `KBot.Theming` | Theme **engine only**: `ThemeManager` + `ThemePalette`/`ThemeStyleOptions`, `BuiltInSchemes` (Classic/Dark/Modern), `IThemedControl`, `ThemeShapes`, `ButtonStyles`, `ModernRenderer`, `KBotDesignTime`, `Interop/NativeMethods`, plus the two base **forms** `KBotThemedForm` / `KBotShellForm`. No controls live here |
| `KBot.Controls` | **Every K-BOT custom control**, one logical folder per control family: `Tree/` (`AdvancedTreeControl` — owner-drawn tree; theme-aware since slice 0027 via `IThemedControl`, with `Color.Empty` = "from the theme" and any designer-set color winning), `DataView/` (+ `DataView/Events/`), `Adobe/`, `NavList/`, `Popup/` (`CustomPopup` — meniu contextual tematizat: pictogramă pe rând, litera de acces `&Salvează`, selecție dată din constructor, tastatură egală cu mouse-ul), `CaptionBar/`, `BusyBar/`, `Notice/`, `TextField/`, `ToolTip/` (`KBotToolTip` — eticheta plutitoare cu antet/corp/subsol + `KBotRichText`, motorul de text îmbogățit; vezi regula de mai jos) |
| `KBot.DevHarness` | Debug-only test bench (`DevHarnessForm`), referenced only on `Debug` |
| `KBot.LocalStore` / `KBot.Xfa` / `KBot.Forexe.Editor` | SQLite temp store / XFA / workflow editor |

Tests: `tests/KBot.{Api,App,Common,Controls,DevHarness,Domain,LocalStore,Theming,Xfa}.Tests` (xUnit).
`Surse/` = imported source snapshots (not part of the solution builds — edit `src/` copies).

## Commands

```powershell
dotnet build KBot.sln
dotnet test KBot.sln
dotnet publish src\KBot.App\KBot.App.vbproj -c Debug    # or .\publish-debug.ps1
# PYTHON server tests: use PYTHON\.venv (global py has no pytest/flask); live-DB tests skip off-host
```

Debug start shows a Yes/No dialog: Da = Login flow, Nu = DevHarness. Release always goes Login → MainForm. Runtime errors land in `<AppDir>\Logs\harness_errors.log`.

## House rules

- `Option Strict On`; no silent no-ops (unknown key → `ArgumentException`).
- **Try/Catch everywhere except the very safe methods.** Every method that can realistically throw is wrapped in `Try/Catch`; the catch always calls `GlobalErrorLog.Write("Type.Method", ex)`. Then:
  - **Risky / boundary methods** (I/O, HTTP/`ApiClient`, Forexe/Playwright automation, SQLite/`LocalStore`, JSON/Excel/PDF/XML parsing, `Process`/interop, reflection): **log + `Throw`** (re-throw — never swallow; callers still see the failure). Use a bare `Throw` to preserve the stack.
  - **UI boundaries** (WinForms event handlers, `OnPaint`/paint bodies, timer ticks, async `void`-style handlers): **log + swallow** — these physically cannot re-throw (a throw from a paint/event body crashes the process), so log and return.
  - **"Very safe" methods that may be left unwrapped:** auto-property getters/setters and one-line pass-throughs, pure POCOs / DTOs / model classes, `.Designer.vb` generated files, interface declarations (no body), constructors that only do null-guards + field assignment, trivial event handlers that only `Invalidate()`/`Focus()`/set `DialogResult` + `Close()`, and code inside `_reference/` (imported snapshots, not built).
  - **Transitive coverage:** a private helper reached ONLY through an already-wrapped boundary in the same class (e.g. a `DrawX`/`CollectX` called only from a wrapped `OnPaint`/handler) needn't carry its own `Try` — the boundary is the log/swallow point. This keeps recursion from logging once per stack level. Wrap the boundary (event override, `On*`, `*_Click`, `*_Tick`, paint) and the public entry points; leave the transitively-covered internals.
  - `GlobalErrorLog.Write` is itself a terminal sink (never throws) — do not wrap calls to it.
  - **Sink reachability:** the catch logs via `GlobalErrorLog.Write` (in `KBot.Common`). `KBot.Controls` references Common (added for this) and exposes it through a project-wide `<Import Include="KBot.Common"/>`. `KBot.Domain` CANNOT reference Common (Common→Domain already, would cycle), so its one risky method (`AngajamentMapper`) throws precisely by design and is logged at the App/Forexe caller boundary instead. `WndProc` overrides are left to the global `Application.ThreadException` net (wrapping risks breaking the window-message contract).
- No `Namespace` blocks.
- **All code is English — identifiers, comments, docstrings, XML doc comments, test names.
  Romanian ONLY in strings the operator actually sees** (message boxes, labels, UI log lines,
  the API's `error` field). Diacritics in those stay literal (ă â î ș ț), and use «» in string
  literals — VB treats typographic quotes „ " as string delimiters. Parts of `KBot.Migrator`
  and `PYTHON/routes/migrare` still carry Romanian identifiers from before this rule; write new
  code in English there anyway rather than matching the old style.
- **Every custom control lives in `KBot.Controls`, in its own logical folder.** No exceptions — a new control goes into `src/KBot.Controls/<Family>/` together with everything that belongs to it (enums, collections, `EventArgs`, models, helper forms). Group by control family, not by file kind: DataView things in `DataView/`, tree things in `Tree/`, Adobe-viewer things in `Adobe/`. The two base *forms* (`KBotThemedForm`/`KBotShellForm`) are not controls and stay in `KBot.Theming`. The reference direction is `KBot.Controls → KBot.Theming` and only that: anything a control needs from the theme engine must be `Public` there (`ThemeShapes`, `NativeMethods.DragMove`), never the other way round.
- Views that talk to the API (`SumarView`, `IstoricView`, `DdfView`, …) are app screens, not controls — they stay in `KBot.App\Views\`.
- Zero hardcoded colors: everything from `ThemeManager.Current.Palette`; new controls implement `IThemedControl`; theme-aware accents go in `OnThemeChanged` overrides (see `KBotThemedForm`).
- **A control that owns internal child controls MUST implement `IThemedControl`** — `ThemeManager.Traverse` recurses into the children of anything that doesn't, and the generic per-type rules then repaint them (`TextBox` → `InputBackColor`, `Label` → `TextColor`/Transparent). This has bitten twice: `KBotTextField`, then the tree's search `TextBox` (slice 0027).
- **Designer-settable colors/fonts/images need `ShouldSerialize<Prop>()`/`Reset<Prop>()`.** Without them Visual Studio writes the *resolved* default into `.Designer.vb`, and that frozen value then reads as a deliberate operator choice forever — how the light palette got hard-written into five designer files before 0027. Convention: `Color.Empty` / `Nothing` = "auto, from the theme"; anything set explicitly wins.
- **This applies to inherited `BackColor`/`ForeColor`/`Font` too.** `Control.ShouldSerializeX` answers True as soon as the property has ever been *written* — including by your own constructor default or `ApplyTheme` — so the designer freezes a value nobody chose, and on reload that line runs through your setter and pins it forever. If a control writes its own `BackColor`/`ForeColor`/`Font`, override the matching `ShouldSerialize*` (+ `Reset*`) and answer from your own "operator pinned it" flag, assigning via `MyBase` internally. `Shadows` properties need `<Browsable(False)>` + `DesignerSerializationVisibility.Hidden` for the same reason — shadowing does NOT inherit the base's serialization attributes.
- **The check for all of the above:** `Font` and `Size` cannot carry `<DefaultValue>` (the attribute needs a constant), so they need the `ShouldSerialize`/`Reset` pair or the designer writes them into every host form. Verify empirically — a freshly dropped control must produce **zero** property lines — and assert it with `TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)`, the path Visual Studio actually takes (calling your own `ShouldSerializeX` directly proves nothing).
- **Pixel metrics that an operator sets are LOGICAL pixels (96 dpi) and must be scaled at runtime.** A font is in points and scales itself; a number like `RowHeight = 28` does not — that mismatch is exactly what made the grid and the tree look wrong at 100% and "better" at 150% (slice 0035). **The public property always stays logical** (the designer serialises 22, not 33 — otherwise the next load scales it again, the `ShouldSerialize` trap in numeric form). Two shapes, pick by who writes the value back:
  - nobody writes it back → keep a `_xLogic` / `_x` pair and recompute the scaled one on every DPI change (`AdvancedTreeControl.Dpi.vb`, `KBotDataView` row/header/footer heights);
  - a layout pass writes it back in device pixels (column widths) → **store logical, scale at use**: expose `…Px` accessors for painting/layout and unscale on the way in (`SetLayoutWidth`, the column-edge drag). Storing the scaled value there silently changes what `Width` *means* — 29 tests caught exactly that.
  - Scale from **`DeviceDpi / 96`**, one source for the whole control. `ScaleControl` / `OnHandleCreated` / `OnDpiChangedAfterParent` are the triggers, not the source: `AutoScaleMode.Font` hands you ~1.45 at 150%, while paint constants go through `ThemeShapes.ScaleDpi` (DeviceDpi) — two sources make a control's floor and its drawing disagree. `OnHandleCreated` matters on its own: before the handle, `DeviceDpi` reports 96 even at 150%.
  - Skip scaling under `KBotDesignTime.IsDesignTime` — the VS surface draws at 96 dpi, so **it is fine to author at 150%**.
- **Hover tooltips use `KBotToolTip`, never `System.Windows.Forms.ToolTip`** — the latter cannot be themed, rounded, or given a header/footer/rich body. For real controls, drop a `KBotToolTip` in the form's `components` and set `SetToolTipHeader`/`SetToolTipText` in `InitializeComponent` (Romanian, like every user-facing string). For the buttons WE draw (tree/grid header icons — painted regions, not controls), call `KBotToolTip.ShowAt`/`HideNow` from the hover-tracking method that already exists. Different looks on one form come from `SetStyleFor(ctrl, Style.Clone())` — the style is a value, not a component; do not widen a control's internal tooltip object.
- Every form declares ALL controls in `.Designer.vb` (see `docs/kbot-forms-ui-convention.md`; note: its theming section predates ThemeManager — the Designer/layout rules still stand).
- Borderless shells inherit `KBotShellForm`; dialogs inherit `KBotThemedForm` (see LoginForm as reference).
- Inside a card panel add children in REVERSE dock order (Fill first, then Bottom/Top; last-added Top docks topmost).
- Versioning: per-project `FileVersion` bumped manually only when that project changes; `AssemblyVersion` stays stable (publish manifest contract).

## Slice status (2026-08-15)

Single source of truth: `docs/worklog/KBOT_STATUS.md` (slice registry + open threads).
Read `docs/worklog/CODE_WORKFLOW.md` before starting any task.

Done:
- **Auth/login** — bearer token; DC+periods model (UN=email, runtime An/SS via `/api/auth/periods` + last-SS persist); `WithReauth` in MainForm = 401 → re-login → retry once.
- **ListaAngajamente vertical** — Forexe scrape → `AngajamentMapper` → `/api/forexe/angajamente/upsert`; column-map seam + tests. Live round-trip still needs FX_Angajamente table (000_DEMO) + API env config.
- **Theming** — engine + 3 schemes, live switching, theme gallery in harness.
- **Tree designer surface** (slice 0027, **screen-verified 2026-08-08** — the first control slice
  with a visual sign-off) — `SearchShow` now actually opens the search band (it was only acted on
  by the XML/FOREXE path), the band renders inside the VS designer, `AdvancedTreeControl` became
  `IThemedControl` so designer colors survive a theme switch, and the header/search bar gained
  fonts, `HeaderTextAlign`, a luminance-driven gradient, designer-pickable icons, clear-button
  padding/image and ESC-to-clear. Two designer collections: `NodeImages` (an `ImageList`) and
  `Nodes` (flat `TreeNodeDefinition` records linked by `ParentKey`). `TreePlaygroundForm` in the
  harness is the tree's equivalent of the `KBotDataView` playground.
- **Tree footer + collapse** (slice 0027-02, code green, **never seen on screen**) — the tree has
  ONE font now: `TreeFont` (and its `FontName`/`FontSize` accessors) is gone, `Font` draws the
  nodes as well as sizing the row. Callers that pinned "Segoe UI, 9" were deleted, not rewritten —
  that is the default, and writing it explicitly pins the font against `ApplyBaseFont`. New
  `FooterVisible`/`Footer*` band (`AdvancedTreeControl.Footer.vb`), the header's sibling — including
  left/right end icons (`FooterRightIconClicked` mirrors `HeaderRightIconClicked`), where **the side
  the collapse button sits on belongs to it and that end icon is neither drawn nor clickable** — plus a
  collapse button: `MinimumCollapsedWidth` (100), `Collapsed`/`ToggleCollapse()`/`CollapsedChanged`,
  and `CollapsedFlyout` — while collapsed, hovering a row floats it out to the right via
  `TreeNodeFlyout`, `KBotNavFlyout`'s sibling — except over the SELECTED row, which stays quiet
  unless `FlyoutSelectedNode` is True (the operator picked that row; the label tells them nothing
  and covers the view next to it). **A host that docks/anchors the tree owns its width,
  so the control does not write `Width` at all when `HostOwnsWidth` is True** — it flips state and
  raises `CollapsedChanged`, and the host moves its own splitter (see `MainForm.tree_CollapsedChanged`,
  which also has to drop `Panel1MinSize` for the duration: a min-size set for dragging otherwise
  vetoes the programmatic collapse too). Writing `Width` against a docking parent just flickers.
  Also: a **hover-only right icon no longer reserves caption width** — `RightIconGutter(node)` takes
  the space only while the icon is actually on screen, so the text is full width and narrows on hover;
  `ReserveRightIconSpace = True` buys the old fixed gutter back. The TreeListView column band stays
  hover-blind on purpose (`ReservedRightIconWidth`) — a full-control geometry must not re-lay out
  under the cursor.
- **MainForm shell scaffolding** (equivalent of Access `frmFX_MAIN` only; Meniu is a separate future concept) — borderless resizable `KBotShellForm` (WM_NCHITTEST band via per-child HTTRANSPARENT subclassing + WM_GETMINMAXINFO taskbar clamp), `KBotCaptionBar` min/max/close + double-click maximize, header strip (unit, An/SS combos, Forexe status dot), `KBotNavList` sidebar (ten entries since 0008: sumar/indicatori/istoric/revizii/rezervari/partener/receptii/plati/ddf/ord), SplitContainer tree card (`AdvancedTreeControl`) | lazy view host, status bar (operator/program, Istoric, **Sincronizare** = connect-if-needed + preserved ListaAngajamente flow, file-only `RichTextBoxLogger` — `EnableUI=False` with a real unshown RichTextBox, never `Nothing`). `AngajamentTreeInfo` = one `/api/forexe/tree` row + tree-click nav state. Views are `PlaceholderView`s behind `IAngajamentView`.

- **Tree data API** (slice 0008) — `GET /api/forexe/tree` (`an`/`ss`/`include_hidden`; the
  base comes from the session, since one MariaDB database = one unit, so there is no
  `id_unitate`/`db_name` parameter). Nine `Are*` flags as correlated `EXISTS`. The contract
  composes TWO Access queries: row-source `qFX_MAIN_TREE_DESCRIERE` (display columns, incl.
  `Salarii`) + `qFX_MAIN_TREE` (the flags) — reading only one of them is the trap that has
  bitten this POCO once already. `MainForm.LoadTreeAsync` binds it via `WithReauth`, An/SS
  changes re-query, `btnOpt` toggles hidden rows, and each flag gates one nav entry via
  `SetItemEnabled`. **Written and green offline, but never run against a real database.**

- **Real views** — Sumar (0011), Rezervări (0014), Recepții (0015), Plăți (0017), **DDF
  (0020)** and **Istoric (0022)** are shipped (code green, never rendered on screen / never run
  live). Istoric adds a pure `IstoricFilter` engine (port of `ApplyColumnFilter`) driving three
  themed `ContextMenuStrip` filters (Clasificații / TipRand / DataFX), a 12-column grid with a
  three-column totals row, and a detail pane — a hosted view (Access `frmFX_ISTORIC` was a popup).
  Its one host-verification risk is the classification-caption tables (`DefaClsfF`/`DefaArticol`
  — plan names vs Access-export names, see `SLICE-0022-01`). DDF adds two
  preview surfaces (`XfaXmlPreview` default / `ReaderHostPreview` backup, switched by a
  compile-time constant), a `KBotPaths`-backed file browser, and in-process PDF generation
  (`DdfXmlBuilder` → `XfaWriter.Genereaza` on a background thread). DDF is **not** a separate
  editor form — it lives in the shell like the other views.

- **`KBotToolTip` + DPI + etichete** (slice 0035, code green, **never seen on screen**) — a real
  tooltip control (`KBot.Controls/ToolTip/`): extender component, header (left icon + title, own font /
  align / backcolor, transparent by default), rich-text body, footer, and a separator drawn only
  between two sections that are both visible. Per-control styles via `SetStyleFor(ctrl, Style.Clone())`.
  The tree and the grid gained multiline tooltip properties for every drawn header/footer button, and
  twelve forms/views gained Romanian tooltips authored in `.Designer.vb`. Same slice fixed the DPI
  half-scaling, column widths included (pass 0035-01), and collapsed the scale down to one source,
  `DeviceDpi / 96` — see the house rule above. `Controls.Tests` 831 green. Still without tooltips:
  `KBotNavList` items and `KBotCaptionBar` buttons.

Deferred / next slices:
- The remaining real views are still `PlaceholderView`: Indicatori, Revizii, Partener.
  (ORD shipped read-only in slice 0033 — tree + lines grid + real PDF, on `GET /api/forexe/ord`.
  **Slice 0049 added the WRITE path**: `OrdEditForm` (modal, `KBotShellForm`) with three pages
  — Beneficiari / Documente justificative / Atașamente — plus eight routes in
  `PYTHON/routes/forexe/ord_edit.py` (generate on the server, save in ONE transaction, delete
  through the cascades, attachment bytes in the new `FX_ORD_ATT_IMG`). `OrdView` stays read-only
  and gained only four entry points. **Code green, never run against MariaDB, never opened on
  screen.** ORD **PDF generation** is still deferred and stays the DDF sibling: it will reuse
  `KBot.Xfa`, like DDF's. Still deferred there: the Fișiere page and the multi-select
  checkboxes.)
- DDF pass 06 (signing) — deferred to slice 0021: `XfaWriter.GenereazaSiSemneaza` on a
  background thread + a `FX_DDF_REV.Semnatura` write-back route + the "never sign while
  `ReaderHostPreview` holds a window" rule.
- DDF step 05-00 live check + a real generated PDF opened in Adobe (see `KBOT_STATUS.md`).
- Istoric + tree sort buttons (placeholder MsgBox today).
- Meniu equivalent (owns Forexe connection lifecycle).
- ComboBox theming retrofit. (The `AdvancedTreeControl` half shipped in slice 0027.)
