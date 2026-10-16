# SLICE-KE02 - The angajament and the classification of each invoice line (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Window `KeClasificariForm`: the accountant chooses the angajament (the ones of the supplier's DDFs, or "Alt angajament..." from the list of all) and, for each line, the classification from the angajament's INDICATORS (`FX_Indicatori`): the law says the indicator must exist before the reception. "Ca la prima linie" copies the first choice. Saved on the server (`PUT .../ke/clasificatii`).
- Window `KeAngajamentForm` (picker with server-side search, like `AlegeDdfForm`): used when the supplier has no DDF or the invoice goes to another angajament. A supplier with several angajamente: every angajament of its DDFs is in the list.
- The lines grid of the "Continut" view got two read-only columns: "Valoare cu TVA" and "Clasificatie".
- A classification that is not an indicator of the angajament is refused with a message that names the missing step (a new reservation revision through the DDF), by the dialog and by the server check.

## Files touched
`src/KBot.EFactura/Forms/KeClasificariForm.vb` + designer, `KeAngajamentForm.vb` + designer, `Views/Primite/PrimiteLiniiPage.Designer.vb`, `PYTHON/routes/efactura/primite_ke.py` (`angajamente`, `seteaza_clasificatii`).

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- The grid combo column is fed with `Column(key).ComboItems` (the way `FacturiForm.Linii` does it); how it behaves with a long list was not seen.
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).

## Corrective 16.10.2026 (found on screen by the operator)
- The angajament chosen last (`_keCod`) was kept across invoices: the window of another invoice opened on a wrong angajament and offered ITS indicators (e.g. 65.10.04.02.20.01.30 instead of 65.02.04.02.20.01.04). Now it is cleared when another invoice is shown (`ShowInvoiceAsync`), and an already linked invoice starts on the angajament of its first link (`ApplyKe`).
- `KeClasificariForm.ApplyIndicators`: a cell holding a choice from the previous angajament goes back to "(nealeasa)" when the angajament changes (it used to look chosen and save as not chosen).
