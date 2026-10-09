# SLICE-00EF-19 -- Received e-invoices, the separate window

Continues 00EF-17 (server) and 00EF-18 (client). Operator decision: a separate screen with all the received invoices.

## Done
- `KBot.EFactura/Forms/PrimiteForm` (+ designer): `KBotShellForm` with the same `PrimiteView` as the DDF view, without a DDF: every received invoice of
  the year chosen in K-BOT (`SessionContext.An`), months in the tree, views on the right. Bottom bar: Iesire (left), Reimprospateaza and
  Sincronizeaza cu ANAF (right; the latter opens `SincronizarePrimiteForm` and reloads when something was added).
- `PrimiteView`: search box `txtCauta` above the tree (visible only without a DDF): narrows the tree on supplier, tax code or number, client side.
- Opened from the title-bar menu of `FacturiForm` («Facturi primite»), one at a time (asking again brings it forward). `FacturiForm` now keeps the
  PDF viewer factory it was given.
- Help 0000-62 (`contabil/efactura/index.md`, «Fereastra Facturi primite»).

## Verified / not
- Build of `KBot.EFactura` and `KBot.App`: 0 errors. Nothing ran, nothing seen on screen, not opened in the VS designer.

## Not done
- Linking an invoice to a DDF from this window (needs a DDF picker); the link is made from the DDF view («Arata toate facturile primite»).
- Year choice inside the window (it follows K-BOT's year, as the issued-invoice window does).
- Message to the supplier (`TrimiteMesajFactura`): later.
