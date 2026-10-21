# SLICE-00EF-46 - The combo list with FOLDABLE GROUPS: the accounts of Vercon under their gestiune (EV02, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV02 "arbore in celula".

## What changed and why
The accountant picks the Vercon account of each invoice line from a tree: gestiune -> accounts, across all subunits (decision D1). There was no tree-in-a-cell control in
`KBot.Controls`. The plan named a new `KBotTreeComboBox` with an `AdvancedTreeControl` in a popup. **What was built instead (a decision of the implementation, to confirm):**
the list window of `KBotComboBox` (`KBotComboList`, which paints its own rows) learned to show **foldable groups**. It is the same two-level tree, with three advantages over a new
control: every integration of the grid (the cell editor, the combo on a group header `GroupHeaderCombo`, `CommitOnSelect`) takes it **without any change**; the typed search
keeps working (and finds an account by its gestiune's name too); and there is one control family to theme, not two.

- **`IKBotComboGroupedItem`** (new): `GroupCaption` (the header) and `RowCaption` (the row's shorter text under it). **`KBotComboGroupedItem`** (new): the ready-made item (`text`, `group`, `rowText`, `tag`).
- **`KBotComboList`**: rows can be group headers (fold triangle + bold text, not indented) or rows of a group (indented); a click or Enter on a header folds / unfolds it (the header stays
  highlighted, the window takes the new height); a row highlighted on opening is never hidden in a folded group (it is unfolded).
- **`KBotComboBox`**: `BuildRows` lists the items under one header per group (first appearance), ungrouped items first and plain; with no grouped item the rows are exactly the old ones.
  Enter on a header toggles it (before the "pick" branch). Which groups are folded is forgotten when `Items` changes.
- The headers exist only in the list window: `Items`, `SelectedIndex`, `FindStringExact`, `CommitText`, the keys with the list closed work on items only.
- Documented in `KBotComboBox.md` ("Grouped items").

## Files touched
`KBot.Controls/Combo/IKBotComboGroupedItem.vb` (new), `KBotComboGroupedItem.vb` (new), `KBotComboList.vb`, `KBotComboBox.DropDown.vb`, `KBotComboBox.vb`, `KBotComboBox.md`,
`KBot.Controls.vbproj` (FileVersion 1.66.0.0 -> 1.67.0.0).

## Test results
`dotnet build src\KBot.Controls\KBot.Controls.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule). Nothing seen on screen.

## Unverified / deferred
- **Nothing seen on screen**: the triangle (size/position), the indent (16 logical px), the bold header, the window height after a fold, hover on a header, the scroll after a fold.
- Behaviour in the grid's cell editor and on a group header was NOT exercised; it is expected to work because the combo's public behaviour on items is unchanged.
- No keyboard key for fold / unfold other than Enter on the highlighted header (Left/Right stay the text box's caret keys).
- If a real `AdvancedTreeControl` in a popup is wanted instead (the plan's wording), the items and the host code (EV03) stay the same; only the list window would change.
- No help: nothing is visible to the operator until EV03 uses it.
