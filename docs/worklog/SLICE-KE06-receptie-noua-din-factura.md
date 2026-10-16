# SLICE-KE06 - "Receptie noua" from an invoice (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- The flow of the button: the window (`KeLegaForm`, mode "noua": type Avans / Partial / Final, default "Final" like the site; date, default the invoice date; description, default "plata factura nr/data"; the value of each classification, default the rest of the invoice) > the server check (`verifica`: indicator exists, amounts fit; warnings asked as a question) > a K-BOT marker `(IDRH: n; IDR: m)` is reserved through the existing route and written at the end of the description > the robot adds the reception > ONLY when it is saved the links are written with those numbers (`EF_PrimiteReceptii.IDRH/IDR`), so the download that follows knows whose reception it is.
- If the save cannot be confirmed (no success element and the read-back does not find the marker) nothing is linked and the accountant is told to look in FOREXE before trying again.

- **Optional one-reception-per-day (operator, 16.10.2026).** Setting `AppSettings.KeBlockSameDayReception`, OFF by default, in «Setari > Aplicatie» (`chkKeZi`). When on, `KeLegaForm` (mode "noua") refuses the date when the angajament already has a reception on it and tells the accountant to change that one or pick another date. Why off by default: a reception K-BOT writes carries its own numbers (the marker `(IDRH; IDR)`), so the day does not identify it; the check is a client-side wish, not a server rule. Only receptions K-BOT already knows are checked (a reception made on the site since the last download is not seen).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.Receptii.vb` (`NewReceptionAsync`, `ReserveMarkerAsync`), `Forms/KeLegaForm.vb`, `src/KBot.Common/AppSettings.vb`, `src/KBot.App/Setari/SetariAplicatieView(.Designer).vb`; help `contabil/efactura/index.md` (tag KE06).

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- The reserved marker lives 2 days (`FX_NumberLock`): a reception saved and read much later may lose it. The flow reads back at once.
- The link is written after the robot and not in the same transaction: if only the link write fails the reception exists in FOREXE and the accountant must link it with "Leaga de receptie...".
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).
