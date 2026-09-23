# SLICE-0777 — Main tree options: sort + CODANGAJAMENT / SURSE columns; «Aplicație» gets tabs

Operator request, 23.09.2026. Slice number **0777** given by the operator (the registry's
next free number is 0077 and was left untouched).

## What changed and why

1. **The right icon of the main tree's header opens a menu** (`CustomPopup`, three placeholder
   icons: `vertical`, `calendar`, `cells` — the operator will swap them):
   - «Sortare după nume» / «Sortare după data creării» (the one in force is checked);
   - separator;
   - «Afișare coloana CODANGAJAMENT» / «Afișare coloana SURSE» (checked = shown).
   The hidden `btnSort` (in the invisible `pnlTreeHead`) opens the same menu; its old
   three-way menu (server order / date asc / date desc) is gone.
2. **Sort.** By name = Descriere, Romanian culture, case ignored, code breaks ties. By date =
   DataCreare **oldest first**; rows without a downloaded DataCreare go **last, ordered by name**.
3. **Columns per sort.** Each sort keeps its own pair of switches. Defaults as asked:
   by name = CODANGAJAMENT on, SURSE off; by date = CODANGAJAMENT off, SURSE on. A column row in
   the menu flips the column for the sort in force only.
4. **One store.** Five new keys in `AppSettings` (`app_settings.json`): `TreeSort`
   (`Name`/`Date`), `TreeNameShowCod`, `TreeNameShowSurse`, `TreeDateShowCod`,
   `TreeDateShowSurse`. The shell follows `AppSettings.Changed` and re-lays the tree from the kept
   rows (`_treeRows`) — no new server request, selection kept. It acts only when the sort or a
   column actually changed.
5. **The tree shows columns now.** `ConfigureListMode` was commented out in `MainForm_Load`, so
   the `CodAngajament` cell was written but never displayed. Columns are now installed from the
   store (`ApplyTreeColumns`), and every row gets both cells (`CodAngajament`, `Surse`).
6. **«Aplicație» settings page has tabs.** A horizontal `KBotNavList` on top (same control as the
   settings window's vertical nav): **Generale** (the switches), **Documente** (the former bottom
   half: PDF engine + Excel ribbon), **KBOT** (sort combo + the four column checkboxes). The page
   follows `AppSettings.Changed`, so a menu choice shows there at once.
7. **Controls.** `CustomPopupItem.Checked` (new): a check mark drawn at the row's right end; the
   band is reserved only when some row is checked. `AdvancedTreeControl.HeaderRightIconRect`
   (new, public): the anchor for the menu, like the existing `FooterRightIconRect`.

## Second pass (same slice, operator 23.09.2026)

8. **No column title band in the tree.** New `AdvancedTreeControl.ColumnHeaderVisible`
   (default True, so every other tree keeps its band); the main tree sets it False in the
   designer. Hidden = the band is not drawn, takes no height and has no header click (so no
   column filter popup from it). The columns stay a real table in the rows — the `~~~`
   left-text~~~right-text fallback was not needed.
9. **Column widths on the KBOT tab.** Two `KBotTextField`s: CODANGAJAMENT (default 140) and
   SURSE (default 90), logical px at 100%, accepted 30–600, saved on Enter or on leaving the
   field (not per keystroke). Bad input: the band says why and the field goes back to the
   stored value. New keys `TreeCodColumnWidth` / `TreeSurseColumnWidth` (same under both sorts;
   an out-of-range value in the file is ignored at load). The shell re-lays the columns when
   a width changes. `COD_COLUMN_WIDTH` in `KbotForm.vb` removed.

## Files touched

- `src/KBot.App/KbotForm.TreeOptions.vb` — **new** partial: menu, handlers, sort, columns, store sync
- `src/KBot.App/KbotForm.vb` — old sort enum/field/menu/SortRows removed; Surse cell; `LeagaOptiunileArborelui()` in Load
- `src/KBot.App/KbotForm.ForexeWatch.vb` — `DezleagaOptiunileArborelui()` in `OnFormClosed`
- `src/KBot.App/KbotForm.Designer.vb` — header right icon tooltip text
- `src/KBot.App/Setari/SetariAplicatieView.vb` / `.Designer.vb` — tabs + KBOT page (`tlyBody` renamed `tlyGenerale`)
- `src/KBot.Common/AppSettings.vb` — five keys + `TreeSortIsDate` / `TreeShowCod` / `TreeShowSurse`
- `src/KBot.Controls/Popup/CustomPopupItem.vb`, `CustomPopup.vb`, `CustomPopup.Painting.vb` — `Checked`
- `src/KBot.Controls/Tree/AdvancedTreeControl.Header.vb` — `HeaderRightIconRect`
- `src/KBot.Controls/Tree/AdvancedTreeControl.Properties.vb`, `.vb`, `.Painting.vb` — `ColumnHeaderVisible` (pass 2)

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` — succeeded, 0 warnings, 0 errors.
- **No tests written or run** (operator: «NU TESTEZI»).

## Left unverified or deferred

- **Nothing seen on screen**: not the menu, not the check marks, not the tree's column header
  band, not the three tabs (nav height 60 px at 144 dpi is a guess), not in any theme.
- Date sort direction is **oldest first** — the request did not say; flip in `SortRows` if wanted.
- The width fields and the tree without its title band: not seen on screen. `Leave` on a
  `KBotTextField` (focus leaving its inner box) assumed to fire as on any container — not checked.
- `ConfigureListMode` sets `Indent = 0` and `RootExpander = False` on the main tree (flat list,
  as it already was by design) — visual effect not checked.
- A column filter left active on a column that the operator then hides is not cleared by this
  slice (behaviour of `SetTreeListView` unchanged) — not checked.
- FileVersion bumps for KBot.App / KBot.Common / KBot.Controls not done (left to `push-update.ps1`).
- No git (operator rule).
