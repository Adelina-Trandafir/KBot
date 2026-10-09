# SLICE-00EF-21 -- the tab «E-Factura» only for angajamente with received invoices

Operator request, 09.10.2026.

## Done
- `KbotForm.Views.vb`: `ApplyViewGating` shows the tab only when the DDF exists (`AreDDF`) AND the last check for THIS angajament found invoices; it starts `VerificaFacturiPrimite` on every gating.
- `KbotForm.EFactura.vb`: `VerificaFacturiPrimite` (async Sub, logs and swallows): reads the DDF of the angajament, then `GET /primite?iddf=` (linked invoices, automatic or manual); shows or hides the tab; a newer question cancels the older answer; if the operator is on the tab and it goes, back to «Sumar».
- Cost: two requests per change of angajament with a DDF (the DDF read is the one the DDF view does too). A failure leaves the tab as it was and is logged.
- Help 0000-64.

## Verified / not
- Build of `KBot.App`: 0 errors. Nothing ran. The tab is hidden until the first answer arrives (a short moment after a selection); an angajament changed while on the tab drops to «Sumar» first, as with any view that closes.
