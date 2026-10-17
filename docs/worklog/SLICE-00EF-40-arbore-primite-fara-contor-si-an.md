# SLICE-00EF-40 - Received invoices tree: no counter on the root, no year on the months (17.10.2026)

## What changed and why
Operator, from a screenshot of the tree «Facturi primite, pe luni»: (1) the root says «Toate facturile» without the count («Toate facturile (10)» before); (2) the month rows say «Septembrie», not «Septembrie 2026».

`PrimiteView.BuildMonthTree`: the root caption no longer carries `({k_shown.Count})`; `MonthLabel` takes a flag and leaves the year out. My addition: the year comes back on every month when the list holds invoices of MORE than one year (the view of an angajament is not limited to one year), so two Septembers can never look the same. With one year (the usual case, and the one in the screenshot) the months are bare names. Not changed: the supplier-grouped tree, the totals on the right, the node keys (they still use year*100+month).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj` (FileVersion 1.3.0.0 -> 1.4.0.0). Help: SLICE-0000-96.

## Test results
`dotnet build src/KBot.EFactura`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
The multi-year behaviour is my guard, not something the operator asked for; if the months must never show a year, delete the `k_severalYears` flag.
