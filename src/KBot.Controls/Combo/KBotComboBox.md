# KBotComboBox

Themed drop-down list: the CLOSED face is painted by us (rounded rect, 1 px outline, GDI+
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
- `ApplyTheme(scheme)`

## Limits
- **`DropDownStyle` is always `DropDownList`. Any other value THROWS** (C3). An editable
  box has a native child EDIT that our painting cannot reach; accepting it would produce a
  half-themed control.
- `DrawMode` (owner-draw), `FlatStyle` and `ItemHeight` are control-owned: pinned by the
  constructor or derived from the font, and never serialized.
- The drop-down list window is still a native list: it gets the uxtheme dark variant plus
  our owner-drawn rows, but no rounded corners and no custom shadow.
- No multi-column list, no per-item icons, no checkbox items.
