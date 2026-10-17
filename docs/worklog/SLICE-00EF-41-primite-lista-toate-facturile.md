# SLICE-00EF-41 - Received invoices: the root «Toate facturile» shows a grid with ticks, «Listeaza» and «Salveaza XML» (17.10.2026)

## What changed and why
Operator request: in the E-Factura view, choosing «Toate facturile» must do what the DDF and ORD views do for their roots: show a grid with all the invoices (of the supplier / angajament the view holds), with a tick column like the DDF view and two actions on the ticked rows, «Listeaza» and «Salveaza XML»; the last column of every row is an options button with the same two actions (for the selected row only).

Built as the DDF view's `PrintListPage` (slice 0099), in the shape of this project:
- New page `PrimiteListaPage` (`Views/Primite`, own designer, all controls declared there): `KBotDataView` with columns tick (header icon ticks / unticks all), Data, Numar, Furnizor, Scadenta, Total (footer sum) and the button column «…»; footer with a status line and the two buttons. The page only collects: it raises `ListRequested` / `SaveXmlRequested` with the invoices (the ticked ones, or the one of the row whose «…» menu was used).
- `PrimiteView` (new partial `PrimiteView.Lista.vb`): choosing the root node (`RootKey`) calls `ShowListSurface` (the bar of views and the pages are hidden, the list takes their place, filled from `ShownItems()` newest first); choosing an invoice brings them back (`ShowDetailSurface`); a rebuild of the tree refills the list and keeps the root chosen; the supplier-grouped tree (no root) never shows it; changing the DDF or `ClearAll` hides it.
- «Listeaza» = makes each invoice ready (the classic invoice K-BOT draws: `FacturaPrimitaPdf.Write`, the default view of «Factura PDF») and sends it to the printer WITHOUT opening it, like «Genereaza si imprima» of the DDF list: the printer and the number of copies are asked once for the whole batch. «Salveaza XML»: one invoice = the usual save window; several = one folder is asked and every XML goes in it (name as the single save suggests, «(2)», «(3)»… when taken). An invoice that fails is logged and named in a message at the end; the others go on. Messages go through `KBotMessage`.
- `PdfPrinter` / `PrinterChoice` (the printto helper of the print list) moved from `KBot.App/Views/Print` to `KBot.Controls/Print`, now Public, so this project can use it; the print list of the DDF / ORD views is unchanged (it already imports `KBot.Controls`).
- New resource `check_boxes` (copy of the App's tick-box icon) in `KBot.EFactura`.

Reading of the request (assumptions): «pentru furnizorul respectiv» = the invoices the view holds (in the view of an angajament, those of its DDF's suppliers; in the window «Cumparare», the year's invoices, where the same root exists); «doar pentru randul selectat» = the options button of a row acts on that row only, whatever is ticked (the button is drawn on every row, not only the selected one).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteListaPage.vb` + `.Designer.vb` (new), `PrimiteView.Lista.vb` (new), `PrimiteView.vb`, `PrimiteView.Designer.vb` (`pgLista`), `src/KBot.EFactura/My Project/Resources.resx` + `Resources.Designer.vb` + `Resources/check_boxes.png`, `src/KBot.Controls/Print/PdfPrinter.vb` (moved, Public), deleted `src/KBot.App/Views/Print/PdfPrinter.vb`; FileVersion KBot.EFactura 1.4.0.0 -> 1.5.0.0, KBot.Controls 1.65.0.0 -> 1.66.0.0. Help: SLICE-0000-97.

## Test results
`dotnet build src/KBot.App` (builds all its projects): 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

## Unverified / deferred
- Never seen: the layout of the list, the button column (text «…» on the full cell), the tick icon in the header, the pop-up of a row at the mouse position.
- The printing goes through the shell's «printto» verb (needs Adobe Reader, like the DDF print list); not tried on a printer, nor with a credit note («NC» rows print the same classic invoice).
- A month node still has no page of its own (the DDF view lists the revisions of a month too; here only the root was asked).
- Printed invoices are not counted anywhere (the DDF list counts prints on the server; received invoices have no such counter).
- The classic invoice is read from the server one by one (one call per invoice); a large batch is slow, there is no cancel.
