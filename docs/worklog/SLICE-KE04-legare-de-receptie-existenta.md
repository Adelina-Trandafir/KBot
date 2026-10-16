# SLICE-KE04 - Link an invoice to an existing reception (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Window `KeLegaForm` (one window, three modes). Mode "existenta": the receptions of the angajament with their value, their rest and an editable "De alocat" per classification; the proposed amount is the smaller of the invoice rest and the reception rest (sign aware), which the accountant may lower for a partial payment.
- Server: `receptii_angajament` (indicators + receptions, value per classification from the last counting snapshot, what invoices already cover, rest), `verifica` (the indicator exists, the sum fits the rest of the invoice on that classification, same sign; a warning when the receptions would pass the legal commitment) and `leaga` (an existing reception must have the rest, else 409 `REST_RECEPTIE` with the advice to modify or make a new one). Nothing changes in FOREXE.

## Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb` + designer, `PYTHON/routes/efactura/primite_ke.py` (`receptii_angajament`, `verifica`, `leaga`), `PrimiteView.Receptii.vb` (`LinkAsync`).

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- How the rest of a reception behaves when a modified reception grows after links were written on it was reasoned, not tried.
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).
