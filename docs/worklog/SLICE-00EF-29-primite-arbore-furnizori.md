# SLICE-00EF-29 - E-Factura received invoices: tree by supplier, search in the header, «Continut» view reworked (15.10.2026)

Slice number: next free after 00EF-28 (checked with grep: 00EF-29 was not used anywhere). The help for it is recorded as
**0000-75** in this same file (sub-slice of 0000, as the rules ask).

## What changed and why
Operator request of 15.10.2026, eight points, all in the received-invoices control `PrimiteView` (used by the «E-Factura»
view of the DDF and by the window «Facturi primite»):

1. **Search box removed, search button in the tree** (like the main window): `txtCauta` and its handler / `_filter` /
   `ShownItems` are gone; the tree's own header search (`HeaderSearchIcon`, band «... tastează minim 3 caractere ...»,
   the control's rules: 3 characters, Esc clears) does the narrowing. It searches the node captions (supplier name,
   date, number).
2. **Right header button for filter options**: `HeaderRightIcon` (filter picture) + `Tree_HeaderRightIconClicked`.
   Nothing to put in it yet, so the menu has ONE disabled line («Opțiunile de filtrare vor fi adăugate aici.») instead of a
   silent no-op.
3. **Grouping**: Toate facturile > MONTH > SUPPLIER > invoice. Supplier key = `CuiNormalizat` (the tax code, which the
   server's list SQL already returns, `EF_Primite.CuiNormalizat`); an invoice without a code falls back to the name written on
   it. The name shown is the one of the supplier's newest invoice. Suppliers in name order, invoices newest first. Leaf caption:
   `dd.MM.yyyy · [NC ]number ~~~ total`. Supplier row has a tooltip (count and total).
4. **Text no longer cut**: `LeftTextWidth = 100` (fixed 100 px for the left text, which cut «Toate facturile (3 ...») is
   removed; `RightTextWidth = 96` reserves the right part instead, so the value (2 decimals, `~~~`, no columns) is always
   whole and right-aligned and the left text takes all the rest.
5. **Opening an invoice** no longer rebuilds the tree (no repositioning, no dot change). Operator, same day: the read / new
   state is NOT shown at all and the colour of the dot «will mean something else» -> `MarkReadAsync` and `_markedRead` are
   removed (the invoice is no longer marked read on the server either; `MarkPrimitaCititaAsync` stays in the API, unused here,
   and `EF_Mesaje.Nou` stays 1), the «Noua» line is gone from the tooltip, and `DotOf` gives a NEUTRAL dot
   (`FacturaIcons.StareNeutra`, theme's dim colour); only in the DDF view an invoice not linked to the DDF keeps the grey ring.
6. **Taxable-base grid removed**: `gridCote` (and its columns, footer «TOTAL TVA») gone from `PrimiteLiniiPage`, and its filling
   from `PrimiteView.FillLines`. The model (`EFacturaPrimitaDetaliu.Cote`) and the classic PDF keep the VAT per rate.
7. **Lines grid = 60% of the height**: `PrimiteLiniiPage` is now a `KBotTableLayoutPanel` (`tlyPagina`), rows 40% / 60%.
8. **Supplier and notes on top** (the 40%): `lblFurnizor` (`KBotHtmlLabel`: name in bold, cod fiscal, adresa, scadenta, referinta
   for a credit note, accounts) and, under it, `rtbNote` (`KBotRichTextEditor`, read-only, header/footer hidden) with the notes
   of the XML joined. All text is HTML-encoded. «Observatii» view stays as it was.

Pictures: `src/KBot.EFactura/Resources/search.png` (copy of the main window's magnifier) and `filter.png` (the filter picture
of KBot.App), added to `Resources.resx` and `Resources.Designer.vb` by hand.

Help (0000-75): `contabil/efactura/index.md`, sections «Facturile primite» and «E-Factura Cumparare: «Facturi primite»»,
tags `00EF-29, 0000-75`; `help-version.txt` = `2026-10-15`.

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.EFactura/Views/Vanzare/FacturaIcons.vb`, `PrimiteView.Designer.vb`, `PrimiteLiniiPage.vb`, `PrimiteLiniiPage.Designer.vb`,
`src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `src/KBot.EFactura/Resources/search.png`, `filter.png`,
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/help-version.txt`,
`docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results
- `dotnet build src/KBot.EFactura` and `src/KBot.App`: 0 warnings, 0 errors.
- `Check-Help.ps1 -Coverage`: nothing wrong in `efactura/index.md` once the slice `0000-75` is in the status; the remaining
  errors are old ones (tours / tutorials: `tur-setari`, `efactura-factura-noua`, `ordonantare-din-plata`).
- No tests added or run (operator rule). The window was NOT opened: nothing here was seen on screen.

## Unverified / deferred
- **Not seen on screen**: the 40 / 60 split, the height of `lblFurnizor` (fixed 96 px logical, a long address wraps and may be
  clipped), the width of the reserved value (96) at 150% scale, the search band inside the tree of this control.
- The filter menu is empty on purpose; the filter options themselves are not defined yet.
- The dot has no meaning yet (neutral). What its colour will say is a decision of the operator; `StateDot` / `DotOf` are
  the only places to change. The server's «new» flag is untouched and no longer read by this screen.
- Grouping is done in the client over the list the server returns (up to 2000 rows, `MAX_LIST`); no SQL was changed.
  If «SQL grouping» meant a server-side `GROUP BY`, say so.
- The «Observatii» view repeats the notes now shown above the lines; kept, not asked to remove.

## Captures to redo
- `efactura-cumparare` (tag in `contabil/efactura/index.md` has `redo:` set).

## Follow-up (15.10.2026, same day)
- Supplier block font 12 pt Calibri (was the default, too small); `lblFurnizor` 120 px logical, `rtbNote` the rest.
- **Empty lines are dropped when notes are shown**: `PrimiteObservatiiPage.CleanNote` / `CleanNotes` (split on CR / LF, trim every
  line, drop the empty ones, drop a note with nothing left). Used by the notes box above the lines (notes also joined with one
  line break, no blank line between two notes) and by the «Observatii» view (rows and the full-text box); the view's «has notes»
  test counts only notes that show something. The stored XML text is not changed.
- Build KBot.App: 0 warnings, 0 errors; not seen on screen.
- Second look (same day): supplier block 11 pt (was 12), notes text 10 pt (`rtbNote.EditorFont`, the editor's default was 11), the two
  side by side in `tlySus` (supplier 42% | 6 px gap | notes 58%, replaces `pnlFurnizor`); the lines grid font 10 pt (was the ambient
  Calibri 9; row height 28 px unchanged). Build KBot.App: 0 warnings, 0 errors; not seen on screen.
