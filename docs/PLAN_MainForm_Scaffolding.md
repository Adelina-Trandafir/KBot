# PLAN_MainForm_Scaffolding.md

> Rebuilt from the original design session (the working file was lost from the tree).
> The Designer tree, the `IAngajamentView` contract, the `AngajamentTreeInfo` gate, and the
> lazy-view mechanism below are reconstructed from that session and are faithful to it.
> Everything marked "confirm in Step 0" must be read from the real file before editing —
> do not treat it as verified.

## What this slice is (and is not)

MainForm is the equivalent of the Access form `frmFX_MAIN` **only**. The `Meniu` hub stays
a separate future concept — not built here. This slice builds the shell and wiring; the six
views are placeholders that later slices replace one at a time without touching the shell.

The sidebar lists **views of the current angajament** — Sumar, Rezervări, Recepții, Plăți,
DDF, ORD — not application modules.

Two finished pieces already in the test-shell MainForm are **preserved and rewired, not
deleted**: the `WithReauth(Of T)` 401-retry wrapper and the ListaAngajamente vertical. Both
get rewired to the "Sincronizare" status-bar button.

---

## Step 0 — read before touching (mandatory)

Open and read in full, do not edit yet:

- the current test-shell `MainForm.vb` + `MainForm.Designer.vb` — note the **exact**
  constructor DI signature (expected: `forexeRunner, session, apiClient, loginFactory` —
  keep whatever is actually there, unchanged), the `WithReauth(Of T)` wrapper, and the
  ListaAngajamente load path.
- `KBotCaptionBar`, `KBotBusyBar`, `KBotTextField`, `KBotNotice` — the controls the
  LoginForm slice produced, so the shell reuses them exactly.
- `ThemeManager` / `StylePalette` / `StyleSystem` — the `IThemedControl` short-circuit in
  `Traverse` and the `Tag = "Card"` → `SurfaceAltColor` convention. Both are locked; the
  new panels rely on them.
- `AdvancedTreeControl` (in `KBot.Controls`) — its public surface, since the tree hosts on
  the left of the split.
- In the **FX_System_Export** repo, `FORMS/frmFX_MAIN.md` — the `RefreshTreeQuery` /
  `qFX_MAIN_TREE` field list (fields assigned from `rcAngInd!...`). This is the gate for
  the POCO below.

Report what each read showed before writing any file.

---

## New base class — `KBotShellForm` (KBot.App or KBot.Theming, match where KBotCaptionBar lives)

A resizable borderless base class, so MainForm (and future shells) get the same
window behavior as the modern LoginForm:

- `WM_NCHITTEST` — edge/corner resize hit-testing on a borderless form.
- `WM_GETMINMAXINFO` — maximize that respects the taskbar (doesn't cover it).
- Aero snap + drag via `DragMove` on the caption area.

LoginForm already solved borderless drag/close; reuse that approach rather than inventing a
second one. Confirm in Step 0 whether LoginForm's borderless logic is already in a
shareable place — if so, lift it into `KBotShellForm` and have LoginForm inherit too (only
if that's low-risk; otherwise leave LoginForm alone this slice).

---

## New control — `KBotNavList` (owner-drawn sidebar)

Replaces a `TabControl`. An owner-drawn vertical list of the six view entries, themed via
`KBotTheme` (no hardcoded colours). Raises `SelectionChanged` with the selected `ViewKey`.
Selected-item accent uses the theme accent (`#185FA5` family via the theme, not literal).
All controls in the Designer file.

---

## New POCO — `AngajamentTreeInfo` (KBot.Domain or KBot.Common — match existing POCOs)

Replaces the ~15 hidden textboxes the Access form used to stash the selected angajament's
fields. Plain properties, one per field.

**Gate:** the field list comes from `RefreshTreeQuery` / `qFX_MAIN_TREE` in `frmFX_MAIN`
(fields assigned from `rcAngInd!...`). Before creating the POCO, open `FORMS/frmFX_MAIN.md`
in FX_System_Export and confirm the full field list. Add anything missed; drop nothing
present. Do not invent fields.

---

## New contract — `Views/IAngajamentView.vb` (KBot.App)

```vb
Public Interface IAngajamentView
    ReadOnly Property ViewKey As String                 ' "sumar", "rezervari", ...
    Sub SetContext(info As AngajamentTreeInfo)          ' selection changed (may be Nothing)
End Interface
```

`SetContext(Nothing)` means "no angajament selected" — the view shows its empty state.

## New UserControl — `Views/PlaceholderView.vb` (+ Designer)

One UserControl used for **all six** views in this slice: a centered `TextDimColor` label
showing `«{ViewName}» — în lucru`, plus, when context is set, the `CodAngajament`. Takes
`viewKey` and `displayName` constructor args. Real views replace instances one by one in
later slices without touching the shell.

## View hosting — `ViewHostPanel` (plain Panel) + lazy activation in MainForm

- `Dictionary(Of String, IAngajamentView)` — a view is created on **first activation
  only** (a never-visited view costs nothing). `Dock = Fill`, only the active one
  `Visible`.
- On `KBotNavList.SelectionChanged`: create-if-missing, show it, hide the previous, push
  the current `AngajamentTreeInfo` into it.
- On tree selection change: update the current `AngajamentTreeInfo`, push it to the
  **active** view only; the others receive it on their next activation.

---

## MainForm — rebuilt Designer + edited code-behind

Designer structure (form: `KBotShellForm`, borderless, 1280×760, Min 1100×640, Padding=1):

```
MainForm : KBotShellForm
└── pnlRoot : Panel                 Dock=Fill, Tag="Card"
    ├── capBar    : KBotCaptionBar  Dock=Top, 40   (ShowMinimize + ShowMaximize)
    ├── busyBar   : KBotBusyBar     Dock=Top, 3
    ├── pnlHeader : Panel           Dock=Top, 40, Tag="Card"
    │     lblUnit (unit name, semibold) · cboAn · cboSS · Forexe status dot+label (right)
    ├── pnlStatus : Panel           Dock=Bottom, 44, Tag="Card"
    │     lblOperator · lblProgram · (spacer) · btnIstoric · btnSinc
    └── pnlWork : Panel             Dock=Fill, Padding=8   (Surface — NO Card tag)
        ├── navViews : KBotNavList  Dock=Left, 170
        └── split : SplitContainer  Dock=Fill
            ├── Panel1: tree card — pnlTreeHead (title + btnSort + btnOpt) +
            │           tree (AdvancedTreeControl, Dock=Fill)
            └── Panel2: viewHost panel (Tag="Card")
```

**Dock-order warning** (same trap as LoginForm): inside `pnlRoot`, add children in
**reverse** dock order in the Designer — `pnlWork` first, then `pnlStatus`, `pnlHeader`,
`busyBar`, `capBar` last. Otherwise the docking stacks wrong.

Code-behind responsibilities (this slice only):

- Constructor: keep the existing DI signature **unchanged** (confirm in Step 0 and keep
  whatever is there: `forexeRunner, session, apiClient, loginFactory`).
- Populate `lblUnit` / `lblOperator` / `lblProgram` from `session` / `SessionContext`.
- `cboAn` / `cboSS`: fill from `/api/auth/periods`. An and SS are **runtime** selections
  here, not login-time facts. On change, they drive later data loads (not built this
  slice) — for now just hold the selection.
- Wire `navViews.SelectionChanged` → lazy view activation (above).
- Wire the tree selection → build/refresh `AngajamentTreeInfo`, push to active view.
- Rewire the preserved `WithReauth(Of T)` wrapper and the ListaAngajamente vertical to
  **`btnSinc` ("Sincronizare")**.

### Two things the auth fix changed — respect them here

1. `WithReauth` is now reason-code aware. A **second** 401 after a fresh login carrying
   `TOKEN_UNKNOWN` is a server-side defect: surface a clear Romanian message, don't loop
   back to the login form. Keep that; don't regress it while rewiring.
   *Note:* `CONTEXT_MISMATCH` comes back from the server as **403**, not 401, so it never
   enters the 401 retry path. If MainForm is meant to treat it as a server defect too,
   handle the 403 explicitly — don't assume `WithReauth` will catch it.
2. The silent `DebugSampleAngajamente()` fallback was removed on purpose (it hid the exact
   failure just fixed). **Do not** reintroduce a silent sample-tree fallback in MainForm.
   If a no-backend tree bench is still wanted, it belongs in `DevHarness`, behind an
   explicit operator choice.

---

## Verification checklist

1. Solution builds clean, `Option Strict On`, zero warnings.
2. All controls are in `*.Designer.vb` and visible at design time (no runtime-only
   controls).
3. No hardcoded colours — every colour comes through `KBotTheme` / the `Tag="Card"`
   convention.
4. Borderless resize, taskbar-respecting maximize, and Aero snap all work.
5. Switching sidebar entries shows the right placeholder; a never-visited view is never
   constructed.
6. Selecting a tree node updates the active view's shown `CodAngajament`; deselecting shows
   the empty state.
7. `btnSinc` runs the preserved ListaAngajamente load through `WithReauth`; a dead session
   surfaces the reason-coded message, not a silent empty tree.
8. `cboAn` / `cboSS` populate from `/api/auth/periods` for the logged-in database.
9. Constructor DI signature unchanged from the test shell.
10. `AngajamentTreeInfo` field list matches `qFX_MAIN_TREE` — nothing dropped.
11. .NET tests green (baseline `KBot.Api.Tests` = 16); test output to
    `AppDir\Logs\test_*.log`; solution published before tests run.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- No swallowed exceptions anywhere — every `catch` surfaces or rethrows.
- VB.NET: `Option Strict On`, no `Namespace` blocks, all controls in `*.Designer.vb`,
  colours only via `KBotTheme`.
- Code and comments in English; operator-facing messages in Romanian with literal
  diacritics (ă â î ș ț), never `\uXXXX`.
- Commit each self-contained change on its own.
- Never invent a fact. Mark verified vs. assumed. Ask only below 75% confidence.
