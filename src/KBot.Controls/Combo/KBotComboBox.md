# KBotComboBox

Themed drop-down: the CLOSED face is painted by us (rounded rect, 1 px outline, GDI+
arrow) and the list rows are owner-drawn. A stock `ComboBox` ignores `BackColor` on the
closed face — Windows themes it, so it stayed white on a dark scheme.

`Combo/KBotComboBox.vb` · `ComboBox` · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

## Why it inherits `ComboBox`
Deliberate: hosts already bind `DataSource`, `Items`, `SelectedItem`, `DisplayMember` and
`SelectedIndexChanged` (e.g. the An/SS combos on `MainForm`). Rewriting from `Control`
would mean reimplementing data binding to arrive at the same place.

## API
Everything `ComboBox` offers, plus:
- `HoverColor`, `BorderColor`, `ArrowColor`, `SelectionBackColor`, `SelectionForeColor` —
  Empty = theme (C1), each with a `Effective*` read-only counterpart = what is painted.
- `CornerRadius: Integer = -1` — -1 = the scheme's radius, 0 = square (logical px).
- `BackColor` / `ForeColor` / `Font` — overridden with ShouldSerialize/Reset (C4).
- `Editable: Boolean = False` — allow typing (see below).
- `LimitToList: Boolean = True` — accept list values only (see below).
- `TextOffsetY: Integer = 0` — optical nudge of the typed text, logical px. 0 = the exact
  vertical centre, computed from the font's own line height. Positive = down, negative = up.
- `CommitText()` — give the verdict on the typed text now, without waiting for the field
  to be left.
- `ApplyTheme(scheme)`

## Typing: `Editable` + `LimitToList`
`Editable = True` switches the control to `DropDownStyle.DropDown`, so Windows puts a
native EDIT child inside it. The text is then drawn by that child, not by us — and it is
**not** left unthemed: `WM_CTLCOLOREDIT` comes back reflected to the control, so the EDIT
paints with our `BackColor` / `ForeColor` / `Font`, while we keep painting the rounded
background, the outline and the arrow around it. Its inner margins are set from the box's
real rectangle (`NativeMethods.GetComboEditBounds` / `SetComboEditMargins`), so the typed
text starts at the same 8 logical px as the list rows at any DPI.

### Vertical centring
A single-line native EDIT does **not** centre its line: it draws it at the top of its own
client rectangle, so the glyphs land at `EDIT.Top + delta` and the EDIT's *height plays no
part*. Both terms are measured rather than guessed — the font's `tmHeight` from
`GetTextMetrics` on the EDIT's own DC (`NativeMethods.GetComboEditLineHeight`) and `delta`
from `EM_POSFROMCHAR` on character 0 (`GetComboEditTextTop`) — and the child is then moved
so that `EDIT.Top + delta` is the centred line. `AlignEditText` runs a second pass: `delta`
can only be read back after the child has moved once, so if the measured value differs from
the one assumed, the bounds are set once more. Never in a loop.

This is why there is no per-font, per-DPI padding constant any more: `tmHeight` changes with
the typeface and rounds independently of the box height at each scale, which is exactly what
made the old hand-tuned table wrong as soon as either changed.

`LimitToList` decides what happens to text that is not in the list:

| | text matches an item | text matches nothing |
|---|---|---|
| `LimitToList = True` (default) | `SelectedIndex` moves to it, spelling taken from the list | the field goes back to the last accepted value |
| `LimitToList = False` | same | the text is **kept** and `SelectedIndex` becomes -1 |

The verdict is given when the field is left (`OnLeave`), on Enter with the list closed,
and whenever a host calls `CommitText()` — the last one is for an OK/Save button the
operator can reach without moving the focus. Matching is `FindStringExact`, so it is
case-insensitive and honours `DisplayMember` when data-bound.

The last accepted value is whatever was last chosen from the list (or the last free text
accepted while `LimitToList` was off). With `LimitToList = False`, read `.Text` — not
`.SelectedItem`, which is `Nothing` for a value that is not in the list.

## Limits
- **`DropDownStyle.Simple` THROWS** (C3): there the list is a permanent panel we neither
  draw nor theme.
- In editable mode the **hover wash moves to the outline**. The EDIT child repaints its own
  rectangle with its own background, so a filled hover would only show as a thick frame.
- The **text selection highlight inside the edit box is the system's** (blue), not the
  scheme's — that colour belongs to the native EDIT.
- `DrawMode` (owner-draw), `FlatStyle` and `ItemHeight` are control-owned: pinned by the
  constructor or derived from the font, and never serialized. `DropDownStyle` is not
  serialized either — `Editable` is the single source of truth for it.
- The drop-down list window is still a native list: it gets the uxtheme dark variant plus
  our owner-drawn rows, but no rounded corners and no custom shadow.
- The EDIT can only be grown **downwards** from the centred text position (its top is what
  fixes where the glyphs are), so the click target is slightly asymmetric. Invisible: the
  EDIT paints with our own `BackColor`.
- At a large `CornerRadius` the rounded corner intrudes on the EDIT's left edge. Not handled
  — the horizontal geometry is Windows'.
- If `tmHeight + 2 logical px` does not fit inside `ClientSize.Height - 2`, the height is
  clamped and the text can be clipped at the bottom. The control does not resize itself.
- `delta` cannot be measured while the box is empty; until the first character arrives the
  last known value (0 on a fresh handle) is used. Invisible in practice — it is 0 or 1 px.
- No multi-column list, no per-item icons, no checkbox items.
- Auto-complete is the inherited `AutoCompleteMode` / `AutoCompleteSource`; this control
  neither sets nor fights them.
