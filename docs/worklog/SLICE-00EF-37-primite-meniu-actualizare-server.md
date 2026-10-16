# SLICE-00EF-37 - Received invoices: «Sincronizeaza cu serverul» and icons in the footer popup (16.10.2026)

## What changed and why
The popup of the tree footer («Actualizare...», 00EF-33/34) got, under the two ANAF rows (period list, days box):
1. a **separator**, then the row **«Sincronizeaza cu serverul»** (icon `refresh`): reads the list again from the K-BOT server
   (`LoadAsync(_idDdf)`; the shown invoice stays shown; does nothing while an ANAF update runs). Read as «the list from the server»:
   the ANAF update (days) already existed, and the operator said earlier that changes on the ANAF side do not matter.
2. **Icons**: the two ANAF rows carry the `anaf` icon, the server row `refresh`. For that, `CustomPopup.DrawInputRow` now draws the
   item's image on the caption line and moves the text by the icon band (before, an input row ignored its image).

3. **One row per element** (operator, same day): the input rows lose their caption line. The combo shows the pale text «Perioada de adus»
   while nothing is picked (new `KBotComboBox.PlaceholderText`, drawn pale, not in the list); the box shows «Intre 1 si 60 de zile»
   as pale text, kept while the box is empty even when it has the focus (new `KBotTextBox.PlaceholderWhileFocused`, drawn over the
   edit control on WM_PAINT; the focus goes to the box when the popup opens, so the native placeholder would never be seen).
   `CustomPopup.Inputs.vb`: a row = the control on the row, right of the icon band, centred; `DrawInputRow` paints only the icon.

4. **Footer popup on the right** (operator, after seeing it): the popup of the footer opens with `IconBarSide = Right` (icon bar and
   text on the right, next to the button).
5. **One source for the corner radius** (operator: the combo had rounder corners than the text box): the radius of every themed
   control is `Style.CornerRadius` of the scheme (`BuiltInSchemes`: 0 / 0 / 8 / 6) unless the control pins its own. The text box
   (`KBotTextBox`) was the outlier with its own fixed 4 (`DefaultValue(4)`). Now: `ThemeShapes.CornerRadiusPx(ctrl, pinnedLogical)` is the
   ONE resolver (pinned >= 0 wins, else the theme's, scaled to DPI) and `KBotComboBox`, `KBotTextBox` (default now -1 = theme),
   `KBotTextField`, `KBotDatePicker`, `CustomPopup` and `KBotButton` call it. The 12 `cbo* / dtp*.CornerRadius = 4` pins in six
   designer files (Login, OrdEdit, AdobeGazduire, SetariAplicatie / Extrase / Tema) are removed, so those combos follow the theme too
   (before they were 4 next to a theme radius of 8 / 6 elsewhere). The text boxes pinned to 0 (memo boxes) stay square.
   **Rule (operator): controls embedded in the data view get NO corner radius, whatever the theme says.** The only ones are the cell
   editors of `KBotDataView`: `editText` (a plain `TextBox`, no radius) and `editCombo` (`KBotComboBox` in `CellEditorMode`: it already
   painted no frame; `EffectiveRadius` now returns 0 there explicitly, so a later change cannot round it).
   To change the roundness of everything: the `CornerRadius` of the scheme in `BuiltInSchemes.vb`. Not touched: controls with their own
   named radius (`ItemCornerRadius` of the nav lists, `ChipCornerRadius`, tree, tooltip, chart, lane) and `ModernRenderer`'s region of
   the plain buttons (it already reads the scheme).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb` (OpenSyncPopup, SyncMenuAnswered, ReloadFromServer), `src/KBot.Theming/ThemeShapes.vb`, `KBotButton.vb`, `KBotTextField.vb`, `KBotDatePicker.vb`, `CustomPopup.vb`, six `*.Designer.vb` (radius pins removed),
`src/KBot.Controls/Popup/CustomPopup.Inputs.vb`, `CustomPopupItem.vb`, `CustomPopup.md`, `src/KBot.Controls/Combo/KBotComboBox.vb`, `src/KBot.Controls/TextField/KBotTextBox.vb`, `contabil/efactura/index.md` (0000-82).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

## Unverified / deferred
- Not seen: the icon band on the combo / box rows (the popup gets a little wider), the look of the separator before a plain row.
- Not seen: the pale text drawn over the focused box (its position comes from EM_GETRECT; the caret sits at its first letter), and
  that it disappears cleanly at the first digit.
- Not seen: the new look of the text boxes (theme radius instead of 4) and of the ex-pinned combos in Login / Setari / OrdEdit, in the four themes (Classic and Dark have radius 0: square fields there).
- The meaning of «Sincronizeaza cu serverul» (re-read the list from the K-BOT server) is my reading of the request.
