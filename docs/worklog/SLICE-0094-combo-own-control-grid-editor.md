# SLICE-0094 — KBotComboBox rewritten on Control; it becomes the grid's combo editor

Operator request, 29.09.2026:

1. In the grid (slice 0085) the combo editor is not centred like the text editor.
2. Move `KBotComboBox` away from `Inherits ComboBox` — not versatile enough. The data-source
   machinery (`DataSource` and friends) is not used and not wanted.
3. Make it behave like `FindAsYouType`, except that the list is also shown when the drop-down
   arrow is clicked.
4. Once done, use it in the grid in place of the standard WinForms ComboBox.

## What changed and why

**`KBotComboBox` is now a `Control`** (`Combo/KBotComboBox.vb` + `KBotComboBox.DropDown.vb`).
Everything on screen is ours:

- **Items** — `KBotComboItemCollection` (new): a plain `IList` of objects with `Add` (returns the
  index), `AddRange`, `Insert`, `Remove(At)`, `Clear`, `IndexOf`, `Contains`, indexer. The
  selection follows its item through inserts/removals; clearing or removing the selected item
  raises `SelectedIndexChanged` with -1. Captions come from `CaptionSelector` (a `Func`) or
  `ToString()` — this replaces `DisplayMember`. No `DataSource`, `DisplayMember`, `ValueMember`,
  `DropDownStyle`, `DrawMode`, `FlatStyle`, `IntegralHeight`, settable `ItemHeight`.
- **The face** — painted as before (rounded fill, outline, "v" arrow). The typed text lives in a
  borderless `TextBox` child (`KBotComboEdit`), placed with the slice 0085 recipe: one line high,
  vertically centred like `TextFormatFlags.VerticalCenter`, glyph padding of `TextRenderer` set as
  the edit margins (`EM_SETMARGINS`). A non-editable or disabled combo paints its caption in the
  same rectangle, so typed and painted text start on the same pixel. The native-EDIT measuring
  (`AlignEditText`, `delta` probing, `WM_CTLCOLOREDIT`) is gone.
- **Height** — outside a grid it follows the font like the native control did:
  `PreferredHeight = ItemHeight + 6`, `ItemHeight = Font.Height + 6 logical px`. This reproduces
  the old serialized heights exactly (31→37, 28→34, 36→42).
- **One list** — `KBotComboList` (the former `KBotComboFindList`, renamed): the arrow (or F4 /
  Alt+Down, or a click on a non-editable face) opens it with EVERY item, the selected one
  highlighted and scrolled to the middle; typing with `FindAsYouType` re-fills it with the matching
  items (same matcher, same `FindAfterNChars`, same `FindFirstGroupCount`); with too little typed a
  list opened by the arrow shows everything again (first item starting with the text
  highlighted). It never takes the focus. New: a draggable scroll thumb and a pageable track, the
  wheel scrolls it also when turned over the box, and it closes on a click anywhere outside it and
  the box (an `IMessageFilter` watches mouse presses while it is open), besides the old triggers.
- **Keys** — the text box's KeyDown/KeyPress/KeyUp are raised as the COMBO's (the grid listens to
  the combo). While the list is open, Up/Down/PageUp/PageDown/Enter/Escape drive it and are input
  keys (so a dialog's Accept/Cancel buttons do not take them). With the list closed, Up/Down step
  the selection (non-editable: also Home/End and a typed letter jumps to the next item starting
  with it). Enter and leaving the field give the verdict on the typed text (`CommitText`) BEFORE
  the host's KeyDown / Leave handlers run.
- **Mouse enter/leave** are reported for the whole combo, text box included, and a press in the
  text box is raised as the combo's MouseDown — `KBotToolTip` hangs off those three.
- Kept: colours/pinned flags/ShouldSerialize pairs, `CornerRadius`, `Editable`, `LimitToList`,
  `TextOffsetY`, `CommitText`, `FindAsYouType`, `FindAfterNChars`, `FindFirstGroupCount`,
  `InputMask`, `UnmaskedText`, `FindMatches`, `OfferNewItem`, `OfferNewItemText`,
  `NewItemRequested`, `MaxDropDownItems`, `BeginUpdate/EndUpdate`, `FindStringExact`,
  `DroppedDown`. New: `TextAlign`, `DropDownWidth` (logical px), `DropDown`/`DropDownClosed`,
  `SelectionStart/Length`, `TextLength`, `SelectAll`, `EditBounds`.

**The grid's combo editor** (`KBotDataView`): `editCombo` is now a `KBotComboBox` in cell editor
mode (Friend `CellEditorMode`: no frame, no arrow, free height, text flush left), `Editable`,
`FindAsYouType`, `LimitToList = False` (free text still reaches `CellValidating`, as with the old
native combo). `PlaceEditor` places it exactly like the text editor — one line high, same vertical
centring — over the combo cell's text rectangle (content minus the chevron strip), with the cell's
font, colours and alignment. The grid keeps painting the chevron; a click on it (when starting the
edit or during it) opens/closes the list, which opens under the whole cell (Friend
`DropDownAnchorProvider`). The painter skips the edited combo cell's text. `CommitEdit` lets the
combo give its verdict first. Left/Right leave a combo cell only from the edge of the text, like a
text cell. Cursor over the chevron is the arrow, not the I-beam. The chevron width is one constant
(`ComboChevronZone`) used by painting, auto-size, tooltip and hit-testing.

**Hosts.** The ComboBox-only designer lines (`DrawMode`, `DropDownStyle`, `FlatStyle`,
`IntegralHeight`, `ItemHeight`, `FormattingEnabled`) were removed from 14 designer files (only
lines on KBotComboBox instances). `KbotForm.Periods` (An/SS) and `LoginForm` (unit) no longer use
`DataSource`: they fill `Items`; the unit combo shows `UnitInfo.Display` through
`CaptionSelector`. DevHarness combo bench: no `DataSource`, reads `EditBounds` instead of the
native EDIT rectangle.

## Files touched

- `src/KBot.Controls/Combo/KBotComboBox.vb` — rewritten on `Control`
- `src/KBot.Controls/Combo/KBotComboBox.DropDown.vb` (new) — the list, outside-click filter
- `src/KBot.Controls/Combo/KBotComboItemCollection.vb` (new)
- `src/KBot.Controls/Combo/KBotComboList.vb` (new; replaces `KBotComboFindList.vb`, deleted)
- `src/KBot.Controls/Combo/KBotComboBox.md` — rewritten
- `src/KBot.Controls/DataView/KBotDataView.Designer.vb` — `editCombo As KBotComboBox`
- `src/KBot.Controls/DataView/KBotDataView.Editing.vb` — combo placement, chevron helpers,
  commit, caret edge
- `src/KBot.Controls/DataView/KBotDataView.Input.vb` — chevron click, cursor
- `src/KBot.Controls/DataView/KBotDataView.Painting.vb` — edited combo text skipped, constant
- `src/KBot.Controls/DataView/KBotDataView.Theming.vb` — `FlatStyle` line removed
- `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb`, `.Tooltip.vb` — chevron constant
- `src/KBot.Controls/DataView/KBotDataView.md` — «The combo editor (slice 0094)»
- `src/KBot.Controls/RichText/KBotRichTextEditor.Designer.vb` — designer lines
- `src/KBot.App/KbotForm.Periods.vb`, `src/KBot.App/LoginForm.vb` — `Items` instead of `DataSource`
- Designer lines removed: `KbotForm`, `LoginForm`, `DdfEditForm`, `DdfEditLinieAForm`,
  `OrdBeneficiariPage`, `AdobeGazduireForm`, `SetariAplicatieView`, `SetariExtraseView`,
  `SetariForexeView`, `SetariJurnalView`, `SetariTemaView`, `CabNoteForm`, `OrdVizualizarePage`
  (all `.Designer.vb`)
- `src/KBot.DevHarness/Internal/ComboPlaygroundForm.vb` / `.Designer.vb`
- `src/KBot.Controls/KBot.Controls.vbproj` FileVersion 1.56.0.0 → 1.57.0.0;
  `src/KBot.DevHarness/KBot.DevHarness.vbproj` 1.0.28.0 → 1.0.29.0

## Test results

- `dotnet build` of `KBot.Controls`, `KBot.App` (with DevHarness), `KBot.Migrator`,
  `KBot.Theming`, `KBot.Updater`: **0 warnings, 0 errors** each.
- No tests written, run or built (standing rule). No off-screen render, not run on screen.
- Reading the tests that touch the combo (`KBotInputMaskTests`: `FindMatches`, `FindAfterNChars`,
  `Editable`, `InputMask`, `Text`, `UnmaskedText`, the designer-serialization check of the three
  slice-0082 properties): every member they use still exists with the same shape. Not compiled.

## Left unverified or deferred

- **Nothing seen on screen.** Pixel alignment of the grid combo editor with the painted cell text
  relies on the same recipe that was measured for the text editor in slice 0085; not measured
  for the combo.
- `AssemblyVersion` of KBot.Controls left at 1.0.0.0 although the public surface of
  `KBotComboBox` changed (base type). Safe only because the update ships the full set.
- Forms laid out around the old combo: the height rule reproduces the serialized heights, but a
  combo docked in a taller cell is top-aligned as before — not checked form by form.
- The `NativeMethods` combo-EDIT helpers in KBot.Theming (`GetComboEditBounds`,
  `SetComboEditMargins`, `SetComboEditBounds`, `GetComboEditTextTop`, `GetComboEditLineHeight`,
  `ApplyControlColors`) are no longer called by anything; left in place (removing them means
  touching Theming).
- Behaviour differences to watch: the mouse wheel over a CLOSED combo no longer changes the
  selection; `Items.Clear()` on an editable combo keeps the typed text (the selection goes to -1);
  a non-editable combo's `Text` setter selects the item with that caption or none.
