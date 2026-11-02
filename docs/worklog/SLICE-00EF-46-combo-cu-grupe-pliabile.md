# SLICE-00EF-46 - The combo list with FOLDABLE GROUPS: the accounts of Vercon under their gestiune (EV02, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV02 "arbore in celula".

## What changed and why
Per-line Vercon accounts: gestiune > accounts across subunits (D1). The plan proposed `KBotTreeComboBox` with an `AdvancedTreeControl` popup. **Implementation decision, still to confirm:** foldable groups in the existing painted `KBotComboList`; its two-level tree preserves cell editors, `GroupHeaderCombo`, `CommitOnSelect` and one control family for theming. Search also matches gestiune names.

- **`IKBotComboGroupedItem`** (new): `GroupCaption` (the header) and `RowCaption` (the row's shorter text under it). **`KBotComboGroupedItem`** (new): the ready-made item (`text`, `group`, `rowText`, `tag`).
- **`KBotComboList`**: unindented bold headers with fold triangles, indented group rows. Click/Enter folds or unfolds a header, retaining its highlight and resizing the window. A row highlighted on opening unfolds its group.
- **`KBotComboBox`**: `BuildRows` lists the items under one header per group (first appearance), ungrouped items first and plain; with no grouped item the rows are exactly the old ones. Enter on a header toggles it (before the "pick" branch). Which groups are folded is forgotten when `Items` changes.
- The headers exist only in the list window: `Items`, `SelectedIndex`, `FindStringExact`, `CommitText`, the keys with the list closed work on items only.
- Documented in `KBotComboBox.md` ("Grouped items").

## Files touched
`KBot.Controls/Combo/IKBotComboGroupedItem.vb` (new), `KBotComboGroupedItem.vb` (new), `KBotComboList.vb`, `KBotComboBox.DropDown.vb`, `KBotComboBox.vb`, `KBotComboBox.md`, `KBot.Controls.vbproj` (FileVersion 1.66.0.0 -> 1.67.0.0).

## Test results
`dotnet build src\KBot.Controls\KBot.Controls.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule). Nothing seen on screen.

## Unverified / deferred
- **Nothing seen on screen**: the triangle (size/position), the indent (16 logical px), the bold header, the window height after a fold, hover on a header, the scroll after a fold.
- Behaviour in the grid's cell editor and on a group header was NOT exercised; it is expected to work because the combo's public behaviour on items is unchanged.
- No keyboard key for fold / unfold other than Enter on the highlighted header (Left/Right stay the text box's caret keys).
- If a real `AdvancedTreeControl` in a popup is wanted instead (the plan's wording), the items and the host code (EV03) stay the same; only the list window would change.
- No help: nothing is visible to the operator until EV03 uses it.
