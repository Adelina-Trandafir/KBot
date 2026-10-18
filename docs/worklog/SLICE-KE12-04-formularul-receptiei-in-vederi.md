# SLICE-KE12-04 - The reception form inside the views of the received invoices (18.10.2026)

Operator (18.10.2026): in the views (not in the form) -- instead of the grid that shows the associated receptions, show a variant of the form «Recepții»: without the choice of the invoice when it is
opened from «Facturi Cumpărare» (the invoice is the chosen one), and without the choice of the angajament too when it is opened from the angajamente of K-BOT's main window (the invoice and the angajament are given).

## What changed and why
- **KeLegaForm** gets an embedded variant: `Embed()` (no frame, no header, docked by the host, never closed), `ShowInvoice(invoice, state, angajament)` (works on THAT invoice, already ticked; the tree is not shown, nothing is read for a list;
  everything kept about a former invoice is dropped; `angajament` not empty = the view of an angajament: the table of angajamente is not shown and that angajament is the one in use), `ClearInvoice()`, event `Committed`.
  The invoice stays in the tree even when it is covered (it is the one on screen). After a reception the form does not read anything itself: it raises `Committed` and waits to be given the invoice again.
  The window of the ribbon works as before (the new members do nothing there).
- **PrimiteReceptiiPage**: the table of links (`gridLegaturi`) is removed; under the summary line there is a panel (`pnlForma`) that hosts the form (`Embed`). The summary (total, allocated, rest, state) and the notes stay.
- **PrimiteView**: `AngajamentCode` (set by `EFacturaView.SetContext` for the view of an angajament, cleared with `ClearAll`); `ShowKeForm` (makes the form on the first invoice, feeds it in `ApplyKe`), `KeForm_Committed`
  (the list and the invoice are read again, which gives the invoice back to the form), `ClearInvoice` when the invoice goes away or its links cannot be read.

## Second part (same day): corrections after the first look
- Embedded variant: the titles «Date recepție …» and «Angajamente» are hidden, so the title band over the lower zones is gone; the rows of «Angajamente» show the description only (no code).
- The navbar of the reception: «Recepție nouă» on the left (icon `add_fact`), «Recepții existente» on the right (icon `table`) -- also in the page of the ribbon.
- The empty note label of `PrimiteReceptiiPage` (Bottom, 36 px) left a blank band under the form: it is shown only when it has text.
- `KBotDateEditBox`: the field of the date showed only a few characters (text clipped at about «04.0»). Cause (read from the code, not seen): the formatting rectangle is set on WM_SIZE, which arrives BEFORE WinForms has updated
  `ClientRectangle`, so after a resize (the embedded form is resized after its first layout) the rectangle kept the old narrow width. It is now also set on `OnClientSizeChanged`. Not seen on screen.

## Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `Views/Primite/PrimiteReceptiiPage.vb`, `PrimiteReceptiiPage.Designer.vb`, `PrimiteView.vb`, `PrimiteView.Receptii.vb`; `KBot.App/Views/EFactura/EFacturaView.vb`; help `contabil/efactura/index.md` (0000-105).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written.

## Unverified / deferred
- Not seen: whether the form fits the area of the views (about 757 x 450 px: the products and the data/amounts bands share it), the hidden columns (tree, angajamente) in the table layouts, the tick boxes in that size.
- A child form inside a view (TopLevel False, as AvacontMain does for pages) was assumed to take the theme and to be disposed with the view.
- The list of the links already made (angajament, classification, reception, sum) is no longer shown anywhere in the view; «Recepții existente» lists the receptions of the angajament and the summary line gives the allocated / rest. To say if the operator wants the links back.
- In the view of an angajament the invoice may be linked to receptions of OTHER angajamente (its old links): they are not shown any more.
