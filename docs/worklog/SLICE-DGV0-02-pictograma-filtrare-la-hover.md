# SLICE-DGV0-02 - the filter funnel only on hover (OnHoverOrFiltered)

Operator, 24.11.2026: «DGV0-02, fa-l cu OnHoverOrFiltered» (question: can the filter icon of the custom DGV be shown only on hover?). Slice number given by the operator.

## What changed and why
- New enum `KBotFilterIconVisibility` = `Always` (default) / `OnHover` / `OnHoverOrFiltered` (`KBotFilterIconVisibility.vb`).
- New grid property `KBotDataView.FilterIconVisibility` (category «K-BOT: Filtrare», `DefaultValue(Always)`, unknown value -> `ArgumentException`).
  - `OnHover`: the funnel is drawn only on the column whose HEADER is under the mouse, or whose filter menu is open.
  - `OnHoverOrFiltered`: same, and the funnel also stays drawn on every column that has a filter set (`HasColumnFilter`), so what is filtered stays visible.
- The room of the funnel stays reserved in every mode (layout untouched): the title does not move when the funnel appears or disappears. Hidden = not drawn (not even the hover plate).
- Hover is now tracked per header cell (`_hotHeaderCellKey`, `UpdateHeaderCellHover` from `OnMouseMove`, cleared in `OnMouseLeave`); repaint only when the mode is not `Always`.
- The funnel of the column whose menu is open stays drawn (`_openFilterMenuKey`, cleared on the popup's `FormClosed`), otherwise it would vanish under its own menu (the mouse is in the popup, not on the header).
- Hit-test unchanged: a hidden funnel sits inside the header cell, and reaching it with the mouse first hovers the header, which shows it.
- Default `Always` = every existing grid looks and behaves as before.
- Switched on, by the operator's choice (24.11.2026, «Introducere Date  Modificare operatii»): `gridOperatii` in `ModificareOperatiiForm.Designer.vb` = `OnHoverOrFiltered`. `gridFacturi` / `gridSold` of the same form are NOT changed.
- `KBot.Controls` FileVersion 1.85.3.0 -> 1.85.4.0. `KBotDataView.md` updated.

## Files touched
`src/KBot.Controls/DataView/KBotFilterIconVisibility.vb` (new), `KBotDataView.FilterIcon.vb`, `KBotDataView.Input.vb`, `KBotDataView.md`, `src/KBot.Controls/KBot.Controls.vbproj`.

## Test results
`dotnet build src/KBot.Controls/KBot.Controls.vbproj`: 0 warnings, 0 errors. No tests, nothing run or seen on screen.

## Unverified / deferred
- Not seen on screen: hover show/hide, the funnel of a filtered column staying on, the funnel staying while the menu is open.
- Help: section «Cautare si ordonare» of `contabil.nomenclatoare.modificare-operatii` rewritten, tag `AV02-05, 0000-177, DGV0-02, 0000-181` (see `SLICE-0000-181-ajutor-filtru-la-hover.md`).

## Second pass (24.11.2026, «Modificare operatii», operator list of five)
1. Columns **CH, CF, Ord, R** (`gridOperatii`): no filter button and no room for it (`ShowColumnFilter` and `ColumnFilterIcon` removed in the designer; `FilterEnabled` is then False, so `ComputeHeaderCellLayout` reserves nothing).
2. The same four columns narrower: `MinWidth = 30` (the default floor of 40 would have undone it) was written; the Width change did not land here, see the third pass (28 each).
3. Grid font **+1 pt**: `gridOperatii.Font` Calibri 9 -> 10 (the header and footer font is fixed by the DGV bands rule and was not touched).
4. The grid covers the whole vertical space: `tlyBody` has ONE row (100 %), the 270 px row of the info panel is gone; the card keeps the 9 px top and bottom padding, so the margin below equals the one above.
5. **Info section removed**: `tlyInfo`, `lblInfo`, `gridFacturi`, `gridSold` out of the designer; `ModificareOperatiiForm.Info.vb` deleted (and its `<Compile Update>` in `KBot.App.vbproj`); `LoadInfo` calls out of `.Lista.vb`; the ribbon button «Arata informatiile» (`btnModOpInfo`, key `modificare_operatii_info`) removed from `AvacontMain.Designer.vb` / `AvacontMain.vb` / `.Ribbon.vb`. NOT touched: the API `GetOperatieInfoAsync` and the `OperatieInfo` model (the data stays available for the new way of showing it), and the setting `AppSettings.ModificareOperatiiInfo` (now unused). The old panel code is in git (`ModificareOperatiiForm.Info.vb`).
- Help: `modificare-operatii.md` (section «Informatiile operatiei» removed, filter paragraph says CH/CF/Ord/R have no filter), `nomenclatoare/index.md` (line of the page); tags `DGV0-02, 0000-181`.
- Build `dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Not run, not seen on screen.
- Unverified: that «Ord» / «CH» headers fit at 30-35 logical px (bold 9pt; the title is elided when it does not fit); the font +1 pt may need a larger `RowHeight`.

## Third pass (24.11.2026, screenshot of CH / CF / Ord / R)
Operator: «coloanele astea mai inguste; aceeasi distanta intre iconita stanga si dreapta ca sus si jos». The icon / tick box is centred in its cell, so the gap left and right equals the gap above and below exactly when the column is as wide as the row is high: Width = MinWidth = 28 logical (= the default `RowHeight` 28) on CH, CF, Ord and R (were 33 / 33 / 35 / 30, `MinWidth` 30). If `gridOperatii.RowHeight` is ever changed, these four widths follow it.
Correction: the width change of the SECOND pass (33 / 33 / 35 / 30) was reported as done but was NOT written to the designer (only `MinWidth` was); the columns kept 66 / 66 / 70 / 60, which is why the operator screenshot still showed them wide. Written now (28 each) and checked in the file. Build App 0/0, not run.

## Fourth pass (24.11.2026)
Headers renamed in the grid: CH -> Ch, CF -> Cf, Ord -> O (R unchanged). Help text of `modificare-operatii.md` follows. The Excel export keeps its own full headers (CH, CF, Ordonantare, Receptie). Build App 0/0, not run.

## Fifth pass (24.11.2026)
Column R (reception) is now a `CheckBox` column like Ch and Cf (same tick, same look): `colRec.ColumnType = CheckBox`; the value is True when the operation has reception for all or some of its invoices (`Receptie <> Niciuna`). The «partly» corner-filled square is gone (a tick box has two states); the «R» icon branch was removed from `GridOperatii_CellFormatting`. `ModificareOperatiiIcons.ReceptieIcon` / `DrawReceptie` are now unused (left in place); `ReceptieText` still feeds the Excel export. Help: the «R» line of `modificare-operatii.md`. Build App 0/0, not run.

## Sixth pass (24.11.2026)
Column R is three-state now: ticked = all invoices have a reception, the «some» box (box with a filled square) = only some, empty box = none. New opt-in column property `KBotDataColumn.NullShowsMixedCheck` (default False, so every other grid keeps drawing Nothing as an empty box): on a `CheckBox` column a Nothing / DBNull cell is drawn with the existing `DrawMixedCheck` (`KBotDataView.Painting.vb`). The form sets it on `colRec`; `ModificareOperatiiForm.ReceptieValue` maps Toate -> True, Partiala -> Nothing, Niciuna -> False. `KBot.Controls` FileVersion 1.85.4.0 -> 1.85.5.0. Help: the «R» line of `modificare-operatii.md`. Build App (to a scratch OutDir, because the running KBot.App locks bin) 0/0, not run, not seen on screen. Note: a tick box is not editable here (column read-only); if a column with this flag were editable, a click on a Nothing cell would make it True.

## Seventh pass (24.11.2026)
Operator rule for the whole accounting part: no date in any DGV shows the time (saved as a memory note). Checked every `KBotDataView` of `src/KBot.App/Avacont`: all date columns already had `Format = ShortDate` or `FormatString = "dd.MM.yyyy"` (or hold text dates), except `colData` of `gridOperatii`, which had neither and so printed `dd.MM.yyyy HH:mm:ss`; now `Format = ShortDate`. Not checked: grids outside `Avacont` (E-Factura, Forexe, ...), grids whose columns are built in code, and the Excel export of this page (its date cell is a day number with a date style). Build App 0/0 (scratch OutDir), not seen on screen.

## Eighth pass (24.11.2026)
Grouping in `gridOperatii`: `EnableGrouping = True` (the «Grupare» tab appears in the column menu) and `AllowGrouping = False` on Tip, Doc., Explicatie, Cont debit, Cont credit, Suma, so only NC, Clasificatie, Fel, Jurnal and Data can be grouped; on Data the menu offers «Pe zi / Pe luna / Pe an» (existing control feature, `KBotGroupLevel.DatePart`). Nothing is remembered between sessions (the sort is; the grouping is not), and «Curata filtrele» does not clear the grouping. Help: new paragraph in «Cautare si ordonare». Build App 0/0 (scratch OutDir), not run, not seen on screen: group bands with the page's «selected operation» (delete, Excel of the list as shown) are untested.

## Ninth pass (24.11.2026)
A column that is a grouping level and has NO filter now shows a drawn «group» glyph (a header bar with two indented rows, accent colour) in the place of the funnel; a filtered column keeps the funnel (the filter wins, also when it is grouped too). With `FilterIconVisibility = OnHoverOrFiltered` the glyph stays on grouped columns like the funnel stays on filtered ones. `KBotDataView.FilterIcon.vb` (`IsColumnGrouped`, `DrawGroupGlyph`); `KBot.Controls` FileVersion 1.85.5.0 -> 1.85.6.0. NOTE: this is in the shared control and is NOT limited to this page: any grid that groups (any `FilterIconVisibility`) now shows the glyph on a grouped, unfiltered column. A column with an own `ColumnFilterIcon` image also gets the glyph while grouped and unfiltered. Help: paragraph on grouping in `modificare-operatii.md`. Build App 0/0 (scratch OutDir), not run, not seen on screen: the look of the glyph at 16 px is untested.
