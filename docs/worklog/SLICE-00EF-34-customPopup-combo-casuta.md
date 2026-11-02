# SLICE-00EF-34 / 37 — Popup-ul comun: intrări, pictograme și rază

34 extinde CustomPopup cu combo/casetă, apoi cu bara colorată; runda de poziționare din aceeași etapă înlocuiește ShowBelow cu ShowAboveRightAligned. 37 mută intrările pe un singur rând, adaugă recitirea listei K-BOT și centralizează raza colțului; editorii din DataView rămân fără rază. Acestea sunt amendamentele interfeței inițiale, cu probele și limitele originale.

- [00EF-34 — intrări și bara de pictograme](#slice-00EF-34)
- [00EF-37 — recitire, rânduri și rază](#slice-00EF-37)

## SLICE-00EF-34 - CustomPopup: combo and text rows; the footer menu of the received invoices uses them (15.10.2026) <a id="slice-00EF-34"></a>

### What changed and why
Operator: the popup of «Actualizare…» was not where it should be, and a new window for it was not wanted -- the existing `CustomPopup` should learn combo and text box rows.
1. **`CustomPopupItem.Combo(key, text, rows)`** / **`CustomPopupItem.TextInput(key, text, placeholder, maxLength, digitsOnly)`**: caption above a real noneditable `KBotComboBox` / `KBotTextBox`. Combo selection or box Enter closes the popup and raises `ItemClicked` with `ComboSelectedIndex` / `InputText`. Flags: `IsCombo`, `IsTextInput`, `IsInput`; input rows have no highlight, selection or access letter.
2. **`CustomPopup.Inputs.vb`** (partial): creates controls once at runtime, asks their height for caption+control sizing, follows scroll, paints captions/themes controls. Opens focused on first input (text box first); focused input owns all keys except popup Esc.
3. **Placement**: standard `ShowBelow` anchors to footer BAND (new `AdvancedTreeControl.FooterBandRect`), below or above if space is lacking, without covering it. The first version overlapped by anchoring to the icon rectangle inside the band.
4. `KBotComboTextPopup` (the window of 00EF-33) is removed.
5. **Placement, second round** (operator: menu still far left, away from right-end button): new `CustomPopup.ShowAboveRightAligned(anchor, bandRect, rightEdge)` opens ABOVE the band, right edge aligned to button. Position is computed after layout gives the size, within working area.

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
1. **separator**, then **«Sincronizeaza cu serverul»** (`refresh`): rereads K-BOT's list via `LoadAsync(_idDdf)`, retaining the shown invoice; inactive during ANAF update. Interpreted as K-BOT server list because ANAF days update already existed and the operator earlier excluded ANAF-side changes.
2. **Icons**: the two ANAF rows carry the `anaf` icon, the server row `refresh`. For that, `CustomPopup.DrawInputRow` now draws the item's image on the caption line and moves the text by the icon band (before, an input row ignored its image).

3. **One row per element** (operator, same day): captions removed. Empty combo shows pale «Perioada de adus» (new `KBotComboBox.PlaceholderText`, outside list); empty box shows pale «Intre 1 si 60 de zile» even focused (new `KBotTextBox.PlaceholderWhileFocused`, WM_PAINT overlay). Opening focuses the box, hiding the native placeholder. `CustomPopup.Inputs.vb` centres each control right of the icon band; `DrawInputRow` paints only the icon.

4. **Footer popup on the right** (operator, after seeing it): the popup of the footer opens with `IconBarSide = Right` (icon bar and text on the right, next to the button).
5. **Common corner radius** (operator: combo rounder than text box): `ThemeShapes.CornerRadiusPx(ctrl, pinnedLogical)` resolves a pin >= 0 first, otherwise scheme `Style.CornerRadius` (BuiltInSchemes 0 / 0 / 8 / 6), scaled to DPI. Callers: `KBotComboBox`, `KBotTextBox` (old fixed default 4 becomes -1 = theme), `KBotTextField`, `KBotDatePicker`, `CustomPopup`, `KBotButton`. Removed twelve `cbo* / dtp*.CornerRadius = 4` pins from six designers (Login, OrdEdit, AdobeGazduire, SetariAplicatie / Extrase / Tema); memo boxes pinned to 0 stay square. **DataView editors always have radius 0:** `editText` is a plain TextBox; the already-frameless `editCombo` now explicitly returns 0 from `EffectiveRadius` in `CellEditorMode`. Scheme-wide changes belong to `BuiltInSchemes.vb`. Untouched: named radii (`ItemCornerRadius`, `ChipCornerRadius`, tree, tooltip, chart, lane) and plain-button regions in `ModernRenderer`, which already read the scheme.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb` (OpenSyncPopup, SyncMenuAnswered, ReloadFromServer), `src/KBot.Theming/ThemeShapes.vb`, `KBotButton.vb`, `KBotTextField.vb`, `KBotDatePicker.vb`, `CustomPopup.vb`, six `*.Designer.vb` (radius pins removed), `src/KBot.Controls/Popup/CustomPopup.Inputs.vb`, `CustomPopupItem.vb`, `CustomPopup.md`, `src/KBot.Controls/Combo/KBotComboBox.vb`, `src/KBot.Controls/TextField/KBotTextBox.vb`, `contabil/efactura/index.md` (0000-82).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

### Unverified / deferred
- Not seen: the icon band on the combo / box rows (the popup gets a little wider), the look of the separator before a plain row.
- Not seen: the pale text drawn over the focused box (its position comes from EM_GETRECT; the caret sits at its first letter), and that it disappears cleanly at the first digit.
- Not seen: the new look of the text boxes (theme radius instead of 4) and of the ex-pinned combos in Login / Setari / OrdEdit, in the four themes (Classic and Dark have radius 0: square fields there).
- The meaning of «Sincronizeaza cu serverul» (re-read the list from the K-BOT server) is my reading of the request.
