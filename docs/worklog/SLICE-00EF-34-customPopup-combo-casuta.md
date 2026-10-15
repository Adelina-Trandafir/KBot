# SLICE-00EF-34 - CustomPopup: combo and text rows; the footer menu of the received invoices uses them (15.10.2026)

## What changed and why
Operator: the popup of «Actualizare…» was not where it should be, and a new window for it was not wanted -- the existing
`CustomPopup` should learn combo and text box rows.
1. **`CustomPopupItem.Combo(key, text, rows)`** and **`CustomPopupItem.TextInput(key, text, placeholder, maxLength, digitsOnly)`**: a row =
   a caption line and under it the real KBot control (`KBotComboBox`, not editable / `KBotTextBox`). Picking a row of the combo, or
   Enter in the box, closes the popup and raises the usual `ItemClicked` for that item, which carries the answer
   (`ComboSelectedIndex` / `InputText`). New flags `IsCombo`, `IsTextInput`, `IsInput`; an input row is never highlighted, is not
   selectable and has no access letter.
2. **`CustomPopup.Inputs.vb`** (partial): creates the controls (once, not at design time), sizes the row (caption + control height
   asked from the control), places them (follows the scroll), paints the caption, themes the controls, opens with the focus in the
   first input (a text box first), and while an input has the keyboard every key is the input's own -- only Esc is the popup's.
3. **Placement**: the footer menu now opens with the standard `ShowBelow` from the footer BAND (`AdvancedTreeControl.FooterBandRect`,
   new), so it never covers the footer: below it, or above when there is no room, like the other menus. The first version anchored
   to the icon's rectangle, which sits inside the band, so it overlapped the footer.
4. `KBotComboTextPopup` (the window of 00EF-33) is removed.
5. **Placement, second round** (operator: still wrong -- the menu stood at the far left of the footer, away from the button at its
   right end): new `CustomPopup.ShowAboveRightAligned(anchor, bandRect, rightEdge)`: opens ABOVE the band with its right edge on the
   button's right edge (the position is worked out after the layout, when the size is known; kept inside the working area).

## Files touched
`src/KBot.Controls/Popup/CustomPopup.Inputs.vb` (new), `CustomPopup.vb`, `CustomPopup.Painting.vb`, `CustomPopup.Input.vb`,
`CustomPopupItem.vb`, `CustomPopup.md`, `src/KBot.Controls/Tree/AdvancedTreeControl.Footer.vb`,
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`; deleted `KBotComboTextPopup.vb` / `.Designer.vb`.

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run (operator rule); the existing `CustomPopupTests` were not run.
Nothing seen on screen.

## Unverified / deferred
- Where the popup shows (below / above the footer), the focus in the text box, that picking a row closes it and starts the update,
  Enter in the box, Esc, a popup taller than the screen with an input row in it.
- The invalid-number warning is shown while the popup is still closing (it is raised from `ItemClicked`).
- `CustomPopupTests` has no case for the new rows.
