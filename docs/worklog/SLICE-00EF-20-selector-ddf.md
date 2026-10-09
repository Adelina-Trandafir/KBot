# SLICE-00EF-20 -- DDF picker for the manual link of a received invoice

Operator request, 09.10.2026 («hai sa facem si selectorul»). Closes the open item of 00EF-19.

## Done
- Server: `primite.lista_ddf` + `GET /api/efactura/primite-ddf?q=` (the newest 300 `FX_DDF`: id, angajament, object, partner, tax code; `q` narrows).
  `py_compile` + the SQL shape checked with a fake cursor; NOT run against MariaDB.
- Client: `EFacturaDdfAlegere`, `IEFacturaApi.GetDdfAlegereAsync`, `ApiClient.EFactura.Primite`.
- `KBot.EFactura/Forms/AlegeDdfForm` (+ designer): search box, grid of fundamentari (double click picks), «Leaga» (right) / «Renunta» (left).
- `PrimiteView` without a DDF (the window of all the invoices): the invoice menu gets «Leaga de un DDF…» and, once the invoice has been shown
  (clicked), «Scoate legatura cu <angajament>» for every link the operator made. After a change the list and the invoice are read again.
- Help 0000-63.

## Verified / not
- Build of `KBot.Api`, `KBot.EFactura`, `KBot.App`: 0 errors. Nothing ran or was seen on screen. The server must be redeployed (new route).
- Choice made: the picker lists the newest 300; typing asks the SERVER (350 ms after the last key, a newer key cancels), so older fundamentari are found too.
