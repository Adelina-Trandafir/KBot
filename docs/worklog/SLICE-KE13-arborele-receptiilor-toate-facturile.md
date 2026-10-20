# SLICE-KE13 - The tree of the page «Recepții»: every invoice, covered ones locked, un-associate, options icon (18.10.2026)

Operator (18.10.2026), on the page «Recepții» and the view «Recepții» of an invoice:
1. text seems to exist under the tree;
2. the tree gets, as in K-BOT's main tree, a new button at the right of its header -- the options one -- with a popup holding «Arată facturile asociate», which shows ONLY the invoices that are associated with a reception;
3. the page shows ALL the invoices by itself, not only the ones that are not associated (same in the «Recepții» view of the E-Factura view);
4. the ones already associated can no longer be ticked (they are ticked by themselves); the whole association system has no point for them; they can be un-associated, nothing more. Suggestions asked on how they should look.

## What changed and why
- **Text under the tree (1)** -- read from the code, not seen: `AdvancedTreeControl.OnPaint` clips the rows to the area between header and footer, but the tree of this page had no footer, so the last row was cut in half
  by the bottom border and looked like text under the tree. The tree now has a footer (as the main tree has one): `TOTAL <sum of the listed invoices>`, which also takes the place of the cut row. The same sum is refreshed with every read.
- **Options icon (2)**: `tree.HeaderRightIcon` (new resource `settings`, the same picture as the main tree's) in `KeLegaForm`. A press opens a popup with ONE row, «Arată facturile asociate» (ticked while on).
  On: the tree lists only the invoices that have at least one link with a reception (a partly covered one counts); the header says «FACTURI ASOCIATE CU RECEPȚII» (else «FACTURI PRIMITE»). A second press turns it off.
  The tree is built again from the invoices already read (the suppliers that were open stay open); nothing is ticked afterwards.
- **All the invoices (3)**: `BuildTree` no longer leaves out the covered invoices (only the ones with value 0 stay out). After a reception `RefreshTree` (was `PruneTree`) keeps every row, refreshes the numbers and locks the ones that became covered.
  The embedded variant (the view of an invoice) already listed the invoice on screen whether covered or not (`ShowInvoice`, KE12-04) and has no tree on screen, so nothing changes there; it has no lock either (the host ticks its one invoice).
- **Covered invoices (4)**: an invoice whose receptions cover its whole total is ticked and LOCKED -- new `TreeItem.CheckLocked` in `KBot.Controls` (the box is drawn grey, a press on it does nothing, the row still selects); the row is written in the theme's dim
  text colour; the tooltip (`TreeItem.Tooltip`) says «Recepționată integral» / «Recepționată parțial: x din y» and lists date · amount of each reception, then «Clic dreapta: «Desface asocierea»». Locked rows are not in `CheckedIds`, so they never reach the lines grid.
  **Un-associate**: right click on any invoice that has links -> popup «Desface asocierea cu recepția» -> confirmation -> every link of the invoice is removed in K-BOT with the existing `RemoveKeLegaturaAsync` (the receptions in FOREXE are not touched; only the role «Contabil»,
  checked on the server too) -> the tree is read again. A partly covered invoice stays tickable (it still has a rest) and can also be un-associated.

## Suggestions for the look of the associated invoices (the operator asked)
Built now: dim text + grey locked tick + tooltip + right-click «Desface asocierea». Other ways, not built: a small «recepționată» icon at the right of the row; the reception date written after the number; a thin «Recepționate» sub-heading under each supplier; a real
«Desface» button on the row (the tree has no row buttons today); asking nothing and un-associating from the options popup for the ticked-and-locked rows.

## Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`; `src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `Resources/settings.png` (copy of the main app's picture);
`src/KBot.Controls/Tree/AdvancedTreeControl.TreeItem.vb`, `.Overrides.vb`, `.Painting.vb`; help `contabil/efactura/index.md` (0000-108).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

## Unverified / deferred
- Not seen: the half-row explanation (1) is from the paint code; the grey tick and dim text; the footer text fitting the width; the popup under the icon; the right click (assumes `NodeMouseUp` carries `e.Button = Right`, as the main window's tree does).
- `ThemeManager.Current.Palette.TextDimColor` is read when a row is built: a theme change in mid-session shows the old colour until the next read.
- The un-association was not run against the live server (`DELETE .../ke/legaturi/{id}` exists since KE01).
- In the embedded view of an invoice a covered invoice can still be linked again (no lock there, as before).
