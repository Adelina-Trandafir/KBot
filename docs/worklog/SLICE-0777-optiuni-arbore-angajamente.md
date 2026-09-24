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
   **`FX_Istoric.DataFX` of the row whose `Descriere` is «Angajament nou.»**, one per
   CodAngajament (pass 5 — NOT `FX_Angajamente.DataCreare`). DataFX is DATETIME and the sort
   uses the time part too. Ascending by default, direction in Settings (pass 4); all sources
   (pass 3). Rows with no such FX_Istoric row go **last, ordered by name**.
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

## Third pass (same slice, operator 23.09.2026)

10. **Date sort = newest first**, and it counts **every source (SS)** of the year, not only the
    SS in the combo. `GET /api/forexe/tree` now accepts `ss=*` (`ALL_SOURCES` in
    `PYTHON/routes/forexe/tree.py`): a leading `%s = 1 OR` lifts the SS block; bind tuple is
    now `(an, all_ss, ss, include_hidden)`; the year, hidden and Anulat/Suspendat filters are
    unchanged. Client: `ApiClient.TreeAllSources = "*"`; `LoadTreeAsync` sends it while the
    date sort is in force (the `IApiClient` signature did not change, so no test fake had to).
    A change of sort now RELOADS from the server (the two sorts show different rows); a
    change of column still only re-lays. Undated rows: still last, by name.

## Fourth pass (same slice, operator 23.09.2026)

11. **One row per angajament, sources joined by «,».** The server already returns one row per
    angajament (CodAngajament is the primary key; the sources are a GROUP_CONCAT joined by
    «;»). The client now also guarantees it (`OneRowPerAngajament`: a repeated code is merged
    into the first row, sources united) and shows the sources as `02A,02B` — each SS once,
    sorted (`FormatSurse`), in the SURSE cell and the row tooltip.
12. **Default widths** CODANGAJAMENT 100, SURSE 70 (were 140 / 90).
13. **Direction: ascending by default**, with «Crescătoare / Descrescătoare» on the KBOT tab
    (`cboOrdine`, key `TreeSortDescending`). Applies to both sorts; undated rows stay last
    either way. Pass 3's hard-coded «newest first» is gone. A change of direction re-lays
    from the kept rows (no server call).

## Fifth pass (same slice, operator 23.09.2026)

14. **The date sort orders on `FX_Istoric.DataFX`**, taken from the row whose `Descriere` is
    «Angajament nou.» (Access literal with the trailing dot; matched with or without it),
    one per CodAngajament (`MIN()` keeps the subquery scalar). DataFX is DATETIME and the
    time part is kept end to end: server field `DataAngajamentNou` (ISO datetime) →
    `GetTreeRow.DataAngajamentNou` → `AngajamentTreeInfo.DataAngajamentNou` → `SortRows`.
    The row tooltip shows it as «Angajament nou (FOREXE): dd.MM.yyyy HH:mm:ss».
    `FX_Angajamente.DataCreare` still drives the YEAR filter only. Not built, not run.

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
- `PYTHON/routes/forexe/tree.py` — `ss=*` = all sources (pass 3); `src/KBot.Api/ApiClient.vb`, `IApiClient.vb` — `TreeAllSources`

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` — succeeded, 0 warnings, 0 errors.
- Pass 4 built into a scratch output folder under %TEMP% (KBot.App was running from Visual Studio and locked its bin/Debug). 0 warnings, 0 errors.
- **No tests written or run** (operator: «NU TESTEZI»).

## Left unverified or deferred

- **Nothing seen on screen**: not the menu, not the check marks, not the tree's column header
  band, not the three tabs (nav height 60 px at 144 dpi is a guess), not in any theme.
- `ss=*` not run against the live server; the server file must be copied to the VPS and gunicorn
  restarted, otherwise the date sort gets rows of NO SS (the old server looks for SS = «*»),
  i.e. only the angajamente without indicators. `PYTHON/tests/test_forexe_tree.py` has no
  `ss=*` case (no tests, operator rule).
- While sorted by date the SS combo no longer narrows the tree; changing it still reloads.
- The width fields and the tree without its title band: not seen on screen. `Leave` on a
  `KBotTextField` (focus leaving its inner box) assumed to fire as on any container — not checked.
- `ConfigureListMode` sets `Indent = 0` and `RootExpander = False` on the main tree (flat list,
  as it already was by design) — visual effect not checked.
- A column filter left active on a column that the operator then hides is not cleared by this
  slice (behaviour of `SetTreeListView` unchanged) — not checked.
- FileVersion bumps for KBot.App / KBot.Common / KBot.Controls not done (left to `push-update.ps1`).
- No git (operator rule).
