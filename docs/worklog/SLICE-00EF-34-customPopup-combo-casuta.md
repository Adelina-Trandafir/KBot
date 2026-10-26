# SLICE-00EF-34 / 37 — Popup-ul comun: intrări, pictograme și rază

34 extinde CustomPopup cu combo/casetă, apoi cu bara colorată; runda de poziționare din aceeași etapă înlocuiește ShowBelow cu ShowAboveRightAligned. 37 mută intrările pe un singur rând, adaugă recitirea listei K-BOT și centralizează raza colțului; editorii din DataView rămân fără rază. Acestea sunt amendamentele interfeței inițiale, cu probele și limitele originale.

- [00EF-34 — intrări și bara de pictograme](#slice-00EF-34)
- [00EF-37 — recitire, rânduri și rază](#slice-00EF-37)

## SLICE-00EF-34 - CustomPopup: combo and text rows; the footer menu of the received invoices uses them (15.10.2026) <a id="slice-00EF-34"></a>

### What changed and why
Operator: the popup of «Actualizare…» was not where it should be, and a new window for it was not wanted -- the existing `CustomPopup` should learn combo and text box rows.
1. **`CustomPopupItem.Combo(key, text, rows)`** and **`CustomPopupItem.TextInput(key, text, placeholder, maxLength, digitsOnly)`**: a row = a caption line and under it the real KBot control (`KBotComboBox`, not editable / `KBotTextBox`). Picking a row of the combo, or Enter in the box, closes the popup and raises the usual `ItemClicked` for that item, which carries the answer (`ComboSelectedIndex` / `InputText`). New flags `IsCombo`, `IsTextInput`, `IsInput`; an input row is never highlighted, is not selectable and has no access letter.
2. **`CustomPopup.Inputs.vb`** (partial): creates the controls (once, not at design time), sizes the row (caption + control height asked from the control), places them (follows the scroll), paints the caption, themes the controls, opens with the focus in the first input (a text box first), and while an input has the keyboard every key is the input's own -- only Esc is the popup's.
3. **Placement**: the footer menu now opens with the standard `ShowBelow` from the footer BAND (`AdvancedTreeControl.FooterBandRect`, new), so it never covers the footer: below it, or above when there is no room, like the other menus. The first version anchored to the icon's rectangle, which sits inside the band, so it overlapped the footer.
4. `KBotComboTextPopup` (the window of 00EF-33) is removed.
5. **Placement, second round** (operator: still wrong -- the menu stood at the far left of the footer, away from the button at its right end): new `CustomPopup.ShowAboveRightAligned(anchor, bandRect, rightEdge)`: opens ABOVE the band with its right edge on the button's right edge (the position is worked out after the layout, when the size is known; kept inside the working area).

### Files touched
`src/KBot.Controls/Popup/CustomPopup.Inputs.vb` (new), `CustomPopup.vb`, `CustomPopup.Painting.vb`, `CustomPopup.Input.vb`, `CustomPopupItem.vb`, `CustomPopup.md`, `src/KBot.Controls/Tree/AdvancedTreeControl.Footer.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`; deleted `KBotComboTextPopup.vb` / `.Designer.vb`.

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run (operator rule); the existing `CustomPopupTests` were not run. Nothing seen on screen.

### Unverified / deferred
- Where the popup shows (below / above the footer), the focus in the text box, that picking a row closes it and starts the update, Enter in the box, Esc, a popup taller than the screen with an input row in it.
- The invalid-number warning is shown while the popup is still closing (it is raised from `ItemClicked`).
- `CustomPopupTests` has no case for the new rows.

### Follow-up (16.10.2026): the coloured ICON BAR (operator: «in slice-ul de popup»)
Asked: «iconitele din meniu pe o bara colorata verticala, ca in Windows 7; din designer.vb stanga sau dreapta; daca e dreapta, textul aliniat la dreapta». It did not exist yet (icons sat in a band on the plain background).
- New `PopupIconBarSide` (Left / Right) and `CustomPopup.IconBar.vb`: properties `IconBarVisible` (True), `IconBarSide` (Left), `IconBarColor` (Empty = theme: `Blend(SurfaceAlt, Accent, 0.14)`), geometry helpers; the bar is painted inside the rounded frame, with a fine line on its inner edge.
- Painting / sliders / input rows / capture redaction use the helpers; with the bar on the right the text is right-aligned, the check mark and chevron are at the left end, the input controls (`KBotComboBox`, `KBotTextBox`) get `TextAlign = Right` (also the placeholder drawn over a focused box).
- The bar is ON by default, so EVERY popup that has an icon changes its look.

#### Files touched
`src/KBot.Controls/Popup/PopupIconBarSide.vb`, `CustomPopup.IconBar.vb` (new), `CustomPopup.vb`, `CustomPopup.Painting.vb`, `CustomPopup.Slider.vb`, `CustomPopup.Inputs.vb`, `CustomPopup.CaptureRedaction.vb`, `CustomPopup.md`, `KBotTextBox.vb`.

#### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

#### Unverified / deferred
- Not seen: the bar in the three themes (colour 14% accent), at other DPIs, with the rounded frame; the right-bar layout (text, check mark, chevron, slider rows, input rows); a popup with a mix of rows with and without icons.
- Slider rows keep their label on the left (only the icon band moves). No sample on the DevHarness.
- Help: every capture that shows a popup menu with icons is stale (listed in the 0000 Open threads).

## SLICE-00EF-37 - Received invoices: «Sincronizeaza cu serverul» and icons in the footer popup (16.10.2026) <a id="slice-00EF-37"></a>

### What changed and why
The popup of the tree footer («Actualizare...», 00EF-33/34) got, under the two ANAF rows (period list, days box):
1. a **separator**, then the row **«Sincronizeaza cu serverul»** (icon `refresh`): reads the list again from the K-BOT server (`LoadAsync(_idDdf)`; the shown invoice stays shown; does nothing while an ANAF update runs). Read as «the list from the server»: the ANAF update (days) already existed, and the operator said earlier that changes on the ANAF side do not matter.
2. **Icons**: the two ANAF rows carry the `anaf` icon, the server row `refresh`. For that, `CustomPopup.DrawInputRow` now draws the item's image on the caption line and moves the text by the icon band (before, an input row ignored its image).

3. **One row per element** (operator, same day): the input rows lose their caption line. The combo shows the pale text «Perioada de adus» while nothing is picked (new `KBotComboBox.PlaceholderText`, drawn pale, not in the list); the box shows «Intre 1 si 60 de zile» as pale text, kept while the box is empty even when it has the focus (new `KBotTextBox.PlaceholderWhileFocused`, drawn over the edit control on WM_PAINT; the focus goes to the box when the popup opens, so the native placeholder would never be seen). `CustomPopup.Inputs.vb`: a row = the control on the row, right of the icon band, centred; `DrawInputRow` paints only the icon.

4. **Footer popup on the right** (operator, after seeing it): the popup of the footer opens with `IconBarSide = Right` (icon bar and text on the right, next to the button).
5. **One source for the corner radius** (operator: the combo had rounder corners than the text box): the radius of every themed control is `Style.CornerRadius` of the scheme (`BuiltInSchemes`: 0 / 0 / 8 / 6) unless the control pins its own. The text box (`KBotTextBox`) was the outlier with its own fixed 4 (`DefaultValue(4)`). Now: `ThemeShapes.CornerRadiusPx(ctrl, pinnedLogical)` is the ONE resolver (pinned >= 0 wins, else the theme's, scaled to DPI) and `KBotComboBox`, `KBotTextBox` (default now -1 = theme), `KBotTextField`, `KBotDatePicker`, `CustomPopup` and `KBotButton` call it. The 12 `cbo* / dtp*.CornerRadius = 4` pins in six designer files (Login, OrdEdit, AdobeGazduire, SetariAplicatie / Extrase / Tema) are removed, so those combos follow the theme too (before they were 4 next to a theme radius of 8 / 6 elsewhere). The text boxes pinned to 0 (memo boxes) stay square. **Rule (operator): controls embedded in the data view get NO corner radius, whatever the theme says.** The only ones are the cell editors of `KBotDataView`: `editText` (a plain `TextBox`, no radius) and `editCombo` (`KBotComboBox` in `CellEditorMode`: it already painted no frame; `EffectiveRadius` now returns 0 there explicitly, so a later change cannot round it). To change the roundness of everything: the `CornerRadius` of the scheme in `BuiltInSchemes.vb`. Not touched: controls with their own named radius (`ItemCornerRadius` of the nav lists, `ChipCornerRadius`, tree, tooltip, chart, lane) and `ModernRenderer`'s region of the plain buttons (it already reads the scheme).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb` (OpenSyncPopup, SyncMenuAnswered, ReloadFromServer), `src/KBot.Theming/ThemeShapes.vb`, `KBotButton.vb`, `KBotTextField.vb`, `KBotDatePicker.vb`, `CustomPopup.vb`, six `*.Designer.vb` (radius pins removed), `src/KBot.Controls/Popup/CustomPopup.Inputs.vb`, `CustomPopupItem.vb`, `CustomPopup.md`, `src/KBot.Controls/Combo/KBotComboBox.vb`, `src/KBot.Controls/TextField/KBotTextBox.vb`, `contabil/efactura/index.md` (0000-82).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

### Unverified / deferred
- Not seen: the icon band on the combo / box rows (the popup gets a little wider), the look of the separator before a plain row.
- Not seen: the pale text drawn over the focused box (its position comes from EM_GETRECT; the caret sits at its first letter), and that it disappears cleanly at the first digit.
- Not seen: the new look of the text boxes (theme radius instead of 4) and of the ex-pinned combos in Login / Setari / OrdEdit, in the four themes (Classic and Dark have radius 0: square fields there).
- The meaning of «Sincronizeaza cu serverul» (re-read the list from the K-BOT server) is my reading of the request.
