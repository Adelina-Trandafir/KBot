# SLICE-AV02-06 - «Operatii contabile»: arbore, grila de facturi noi la cerere, «Sursa/Sector» in bara ferestrei

Request of the operator, 19.11.2026 (three screenshots of the window «Operatii contabile»). Help: `SLICE-0000-180-ajutor-operatii-sursa-arbore.md`.

## What changed and why

1. **«Operatii diverse»: the list of templates is a tree.** The grid `lstSabloane` and the manual search box `txtCauta` are gone; in their place the
   `AdvancedTreeControl` `treeSabloane` with ITS OWN search (`SearchShow`), one folder per journal (`Cod - Denumire`), **collapsed at the start**, the
   operations (type A templates of the source) under it. The tree is built ONCE per read (`BuildTemplates`); a pick does not rebuild it, so the folders the
   operator opened stay open. A click on an operation chooses it (`NodeMouseUp`, left button only); with unsaved work the same question as before; the
   selection follows the operation in work (`SyncSelection`, `SelectAndReveal`). The grouping by journal is MY choice (the operator asked for «a tree,
   collapsed»; type A templates have no classification) - see unverified.
2. **«Facturi si plati»: the grid of new invoices is filled only by choosing an evidence template.** `gridNoi` starts EMPTY (no spare row in the
   constructor nor after `ClearEdits`); choosing a template in «Documente evidentiere» gives it its spare row (`SyncInvoiceGrid`); taking the template off
   empties it when nothing was typed (typed rows are never thrown away silently; saving still demands a template). `KeepBlankRowLast` does nothing without a
   template. The grids of the current and of the previous year were ALREADY filled when a partner is chosen (`LoadPartnerAsync`) - unchanged. The payment grid is
   unchanged (the operator's sentence said «only for new invoices»).
3. **Duplicated partners.** Cause (read in the code, `MariaDB_Schema/AVACONT_SURSA.sql`): `Parteneri` holds every partner once per unit
   (`IdUnitate`, UNIQUE with `CodPartener`) and a unit is one row of `Unitati` = sector+source and year; both server routes read ALL partners without a filter, so
   one partner showed up as many times as there are units. Fix:
   - server: `postare.SQL_UNITATI_SURSEI` (`SELECT IdUnitate FROM Unitati WHERE SursaSector = %s AND An = %s AND COALESCE(Ascuns,0) = 0`); the context of
     «Facturi si plati» filters `Parteneri` by it; the context of «Operatii diverse» (`op9.py`) now ALSO returns `parteneri` (filtered the same way).
     The big catalog of the templates window is NOT touched (its fixed partners of template lines are looked up by id).
   - client: `OperatiiContext.Parteneri` (+ `MapOpPartners` in `ApiClient.Operatii.vb`, shared by the two contexts); the page «Operatii diverse» fills its partner
     combo from the context, no longer from the catalog.
   - **«Sursa/Sector» in the title bar of the window** (the existing painted selector `KBotCaptionBar.SelectorSector`), WITHOUT «Toate». It opens on the source
     of the main window, or on **02A** when the main window says «Toate» (first source if 02A is not offered). The sources are those of the periods of the year
     that have a subunit (`KbotForm.OperatiiSources`). The subunit of the pages = the main window's subunit when it belongs to the chosen source, else the
     default one, else the first one (`ResolveSubunit`); none = both pages show «Sursa/Sectorul ales nu are nicio subunitate...». A pick settles the page in
     front first (the unsaved-work question; Cancel keeps the selector where it was), then BOTH pages read again (the other one when it is shown:
     `EnsureBound`). `Bind` of both pages starts with `ResetAll`. The caption text no longer carries the source (`· toate sursele` / `· 02A` removed); the year stays.

## Second pass (19.11.2026, operator's remarks on three more screenshots, same slice)
- «Operatii diverse»: the **Jurnal** column is gone from the lines grid (designer column + the `k_row("jurnal")` write in `FillLines`).
- «Facturi si plati»: the partner combo is **typeable** (`cmbPartener.Editable`, like the note combo). The bar of the three views (`navFacturi`) is the same height
  as the header of the grids (45 at 144 dpi = 30 logical; row 51 with the 6 top margin, aligned with the top of `lstEvidentiere`), has **icons** (`add_fact`,
  `invoice`, `istoric_32`; `IconSize` 16) and a **frame**: `navFacturi` now sits in the Panel `pnlNav` (Padding 2) whose `BackColor` is
  `ThemeScheme.OuterBorder` (set in `ApplyTheme`, the SLICE-0130 rule). The vertical gap between the two groups of grids is the same on both sides:
  `tlyView.Margin` bottom 6, as the left lists have. **«Nota contabila» is the FIRST column** of both entry grids (`gridNoi`, `gridPlata`).
- The Facturi partner combo only; the partner combo of «Operatii diverse» stays a pick list (not asked).

## Third pass (19.11.2026, operator's rule for the payment grid)
- «Facturi si plati»: the bottom grid (`gridPlata`) shows ONLY the payment being made, and ONLY while a row of «Documente plata» is selected; otherwise it is EMPTY
  (`SyncPaymentGrid`, replaces `FillOrders`). The orders already issued are NOT listed any more, and the delete column (✕) + `GridPlata_ButtonClick` +
  `GridPlata_CellFormatting` are removed (nothing in it can be deleted now). The server still returns `plati` (untouched); the client ignores it.
  `ValidateRequest` works without a payment row (no payment template = no payment). **The feature «delete a payment» of AV02-04 is gone from this window.**
- E-Factura logic NOT touched (operator's warning): the only change in a shared file is a new constant `SQL_UNITATI_SURSEI` in `postare.py`; `plati.py`,
  `primite_*.py` and everything under `EFactura/` are unchanged by this slice.

## Fourth pass (19.11.2026, operator's rule: every date field of a DGV uses the custom calendar)
- «Facturi si plati»: the two editable date columns (new invoices «Data», payment «Data O.P.») were plain text; now `ValueType = DateTime` + `FormatString = "dd.MM.yyyy"` (the format the page parses), so they show the custom calendar. The page code is unchanged (the cells still hold the typed/chosen text).
- The rule is written, as permanent information, in `src/KBot.Controls/DataView/KBotDataView.md` (section «Date cells ALWAYS use the custom calendar»). A scan of every KBot.App designer found no other editable date column that was plain text; columns built in code were not scanned. The E-Factura grids were not touched. **The rule/md text has NO slice number yet (the operator did not give one).**

## Files touched
- `src/KBot.App/Avacont/IntroducereDate/Operatii/OperatiiDiversePage.vb` + `.Designer.vb`, `FacturiPlatiPage.vb`, `OperatiiSabloaneForm.vb`, `KbotForm.Operatii.vb`
- `src/KBot.Domain/Avacont/IntroducereDate/Operatii/OperatiiInfo.vb` (`OperatiiContext.Parteneri`), `src/KBot.Api/Avacont/IntroducereDate/Operatii/ApiClient.Operatii.vb`
- `PYTHON/routes/avacont/introducere_date/operatii/postare.py`, `op9.py`, `facturi_plati.py`
- `src/KBot.App/HelpContent/contabil/nomenclatoare/operatii.md` (0000-180)

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj` (output redirected, the running app locks `bin`): 0 warnings, 0 errors. Python: `py_compile` of the three files OK.
No tests run (operator rule). Nothing seen on screen.

## Unverified / deferred
- **Server NOT deployed** (operator does it by hand): the filter on partners is in `postare.py`, `op9.py`, `facturi_plati.py`. Until it is on the server the client
  works but partners stay duplicated (the new `parteneri` key of the diverse context is simply absent -> the combo would be EMPTY on the old server; deploy first).
- **Assumption not seen in data:** that a partner row exists for the `Unitati` row of (SS, year). If a source/year has no unit row or the partners were stored under
  another unit, the partner list is EMPTY (deliberately not a fallback to «all», that would bring the duplicates back). Check on the live DB after the deploy.
- The grouping of the tree by journal (instead of another grouping) is not confirmed by the operator.
- The page «Facturi si plati» was changed only for the new-invoices grid; the payment grid still shows its payment row at once.
- With the subunit list unknown (not read yet when the window opens) the window falls back to the main window's subunit and offers its source only.
- Not seen on screen; the painted selector in the title bar of this window (first use of `SelectorSector` outside the main window) is unchecked for width on small screens.
