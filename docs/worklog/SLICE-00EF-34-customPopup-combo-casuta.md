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

## Follow-up (16.10.2026): the coloured ICON BAR (operator: «in slice-ul de popup»)
Asked: «iconitele din meniu pe o bara colorata verticala, ca in Windows 7; din designer.vb stanga sau dreapta; daca e dreapta, textul
aliniat la dreapta». It did not exist yet (icons sat in a band on the plain background).
- New `PopupIconBarSide` (Left / Right) and `CustomPopup.IconBar.vb`: properties `IconBarVisible` (True), `IconBarSide` (Left),
  `IconBarColor` (Empty = theme: `Blend(SurfaceAlt, Accent, 0.14)`), geometry helpers; the bar is painted inside the rounded frame,
  with a fine line on its inner edge.
- Painting / sliders / input rows / capture redaction use the helpers; with the bar on the right the text is right-aligned, the check
  mark and chevron are at the left end, the input controls (`KBotComboBox`, `KBotTextBox`) get `TextAlign = Right` (also the
  placeholder drawn over a focused box).
- The bar is ON by default, so EVERY popup that has an icon changes its look.

### Files touched
`src/KBot.Controls/Popup/PopupIconBarSide.vb`, `CustomPopup.IconBar.vb` (new), `CustomPopup.vb`, `CustomPopup.Painting.vb`,
`CustomPopup.Slider.vb`, `CustomPopup.Inputs.vb`, `CustomPopup.CaptureRedaction.vb`, `CustomPopup.md`, `KBotTextBox.vb`.

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

### Unverified / deferred
- Not seen: the bar in the three themes (colour 14% accent), at other DPIs, with the rounded frame; the right-bar layout (text, check
  mark, chevron, slider rows, input rows); a popup with a mix of rows with and without icons.
- Slider rows keep their label on the left (only the icon band moves). No sample on the DevHarness.
- Help: every capture that shows a popup menu with icons is stale (listed in the 0000 Open threads).
