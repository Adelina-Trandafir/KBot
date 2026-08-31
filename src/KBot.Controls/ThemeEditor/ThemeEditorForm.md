# ThemeEditorForm — "Stiluri…"

A tool WINDOW (not a control) that takes an already-open surface, enumerates its controls
and lets the operator change one control's background, text, hover, border, accent,
selection colours and font, with immediate effect on screen. Choices are saved as JSON.

`ThemeEditor/` — `ThemeEditorForm.vb` (+ `.Designer.vb`), `ControlStyleProxy.vb`,
`ThemeScope.vb`. Opened from the caption bar's theme menu
([KBotCaptionBar](../CaptionBar/KBotCaptionBar.md), `ShowThemeEditor`).
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

Not to be confused with [ThemeOptionsForm](../ThemeOptions/ThemeOptionsForm.md): this one
puts EXCEPTIONS on named controls of one window; that one changes what a scheme MEANS for
every window.

## API
- `New(host As Form)` — `Nothing` → `ArgumentNullException`.
- `ShowFor(host) As ThemeEditorForm` (Shared) — opens it modeless, owned by the host; an
  already-open editor for the same host is brought to the front.
- `ThemeScope` — one editable surface: `Root`, `IsForm`, `ScopeName` (= the root's TYPE
  name, the key written into `ThemeOverrideSet.Scope`), `Collect(host) As List(Of ThemeScope)`.
- `ControlStyleProxy` — the property-grid row for one control:
  `Nume`, `Tip`, `Cale` (hierarchy path = the JSON key), `SloturiAplicabile` (which extra
  slots actually have an effect on THIS control), `Fundal`, `Text`, `Hover`, `Contur`,
  `Accent`, `SelectieFundal`, `SelectieText`, `Font`, `ResetAll()`.

## Rules
- **Modeless on purpose.** You must see the window behind change while you pick a colour; a
  modal dialog would cover exactly what you are adjusting.
- **Edits live instances**, not the designer file.
- **One set per surface**: choices are kept in a dictionary keyed by surface name, so
  switching between `MainForm` and a view loses nothing.
- The control tree STOPS at `IThemedControl` controls, exactly like `ThemeManager.Traverse`:
  their children are internal (the tree's search band, the grid's boxes), were never
  authored in the designer, and are not the operator's to style. Views appear as leaves of
  the window tree and as separate surfaces in the top list — they are hosts in their own
  right.

## Limits
- **Nothing is read back at startup.** The saved JSON is written but not loaded — that was
  explicitly deferred to a later slice, so edits are lost on restart.
- Slots outside `SloturiAplicabile` are still saved but have no visible effect on that
  control type.
- Only the listed slots plus `Font`; no per-state (pressed/disabled) colours, no metrics,
  no images.
