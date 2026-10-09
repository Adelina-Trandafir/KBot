# SLICE-00EF-18 -- Received e-invoices, client half (domain, API, view in the shell, sync dialog)

Decisions of the operator are in `SLICE-00EF-17-primite-server.md`. Server: 00EF-17.

## Done
- `KBot.Domain/EFacturaPrimite.vb` (POCOs), `IEFacturaApi` + `ApiClient.EFactura.Primite.vb` (sync, list, detail, mark read, xml, zip, pdf,
  embedded file, link / unlink). Wire names are the server's.
- `KBot.EFactura/Views/Primite/PrimiteView` (+ designer): tree (root «Toate facturile» > months > invoices; orange dot = never opened),
  horizontal bar Linii / Factura PDF / Atasamente / Mesaje (the last two only when the invoice has some), right click «Salveaza ca ZIP / XML»
  and, with a DDF, «Leaga de acest DDF» / «Scoate legatura». Pages: `PrimiteLiniiPage` (lines + VAT per rate), `PrimiteAtasamentePage`
  (double click saves a file), `PrimiteMesajePage` (XML notes + messages); the PDF page is `VanzarePdfPage` (the embedded classic PDF if the XML
  carries one, else ANAF's drawing). Made for reuse: the separate screen (00EF-19) puts the same control in a window.
- `KBot.App/Views/EFactura/EFacturaView` (+ designer): the shell view «E-Factura» (nav item, Far side, shown with `AreDDF`); finds the DDF of the
  angajament (`AntetDeLucru(0).Iddf`; more than one header -> the first, logged) and calls `PrimiteView.LoadAsync(iddf)`.
- `KBot.EFactura/Forms/SincronizarePrimiteForm`: period 7/15/30/45/60 days, repeats the sync call (20 per call) with a progress line until
  nothing is left or a call brings nothing; no figures box on success; problems in a notice. Opened from the title-bar menu of `FacturiForm`
  («Sincronizeaza facturi primite»).
- Help 0000-61 (`contabil/efactura/index.md`: «Facturile primite», «Sincronizarea facturilor primite»).

## Verified / not
- Build of `KBot.Api`, `KBot.EFactura`, `KBot.App`: 0 errors, 0 warnings. NOTHING ran: not on screen, not in the VS designer, not against the server
  (server not deployed). No tests written (not asked).
- Assumptions to check on first run: the DDF view's «rev 0» is just «a DDF exists» (operator); `AdvancedTreeControl.NodeMouseUp` raises for the
  right button too (the invoice window relies on it); `KBotComboBox.Items.AddRange` in the dialog's designer.
- Server prerequisites: deploy 00EF-17 files, run `sql/00EF_17_primite_tva.sql` (the asocieri DDL is already run) + schema sync.

## Not done (next)
- 00EF-19: the separate window with all received invoices (same control, no DDF), opened from the menu.
- Manual link only from the DDF view's «all» list; a picker of DDFs from the separate screen is not built.
- Message to the supplier (`TrimiteMesajFactura`): later (operator).
