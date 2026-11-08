# SLICE-03EF-12 - The tree of the received invoices: month > day > invoice «Furnizor - Număr» (08.11.2026)

## What changed and why
Operator (08.11.2026): the default grouping (by date) of the E-Factura tree must be: root «Toate facturile» (as now); level 1 the month by name (as now); level 2 the day as dd.mm.yyyy (NOT the supplier any more); leaf «Furnizor - NumarFactura ~~~ Valoare».
- `FacturiCumparareView.BuildMonthTree`: under a month the days (newest first, each with its sum, `D_yyyymmdd`, «fără dată» for invoices with no date), under a day the invoices ordered by supplier name then number. A day opens when it holds the invoice shown.
- `AddInvoiceNodes(..., k_underDay)`: under a day the row is «Furnizor - [NC ]Număr» with the value; `InvoiceSupplierName` = the name written on the invoice, else its tax code. Elsewhere the row keeps «dd.mm.yyyy · Număr».
- **Kept as before (decision, to confirm):** the view of an angajament (month > invoices, 0114-22) and the view «Recepții» of the window (month > supplier > invoices: the button on a supplier row proposes the links, KE21) — both need their own shape. «Grupare după furnizor» is unchanged.
- The tick boxes of «Contări» and the ticking by supplier work on the leaves as before; the search of the tree finds the supplier in the row.

## Files touched
`KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.vb`. Help 0000-173.

## Test results
KBot.EFactura builds clean (0 warnings, 0 errors). No test run, nothing seen on screen.

## Unverified / deferred
Not seen on screen. The Vercon page has its own tree and is not changed.
