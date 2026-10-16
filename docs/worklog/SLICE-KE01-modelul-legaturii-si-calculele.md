# SLICE-KE01 - The link invoice <-> reception: data model, amounts with VAT, server and client contract (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- New table `EF_PrimiteReceptii` (many-to-many, per classification): `IdPrimita`, `CodAngajament`, `IdClsf`, `IDRR` / `IDRH` / `IDR`, `Suma` (with VAT, negative for a credit note), `Tip` (existenta / noua / modificata), `UN`. Two new columns on `EF_PrimiteLinii`: `IdClsf` (the accountant's choice) and `CotaTVA` (the line's VAT rate from the XML). One script: `sql/KE_01_primite_receptii.sql`.
- `primite_ubl.py` reads `Item/ClassifiedTaxCategory/Percent` per line; `primite.py` stores it at sync. A line stored before KE01 gets its rate from the kept XML the first time its links are opened (`_completeaza_cote`, same order of lines).
- `primite_ke.py` (rules): `valori_cu_tva` gives every line its value WITH VAT (rate of the line, else the rate of the biggest taxable amount; the cents between the lines of one rate and the VAT the invoice states for it go on the LAST line of the rate; checked by hand on the Digi XML: 64.51 + 12.89 = 77.40); `starea` (lines, totals per classification, links, the angajamente an invoice can go to, state nelegata / partiala / acoperita / depasita); the part of the total the lines do not explain is shown (`nealocat_liniilor`), never spread. Every write is for the role `Contabil`, read from the session (`g.session.ctx["Role"]`, set at login from `Unitati_Utilizatori.Rol`).
- `primite_ke_routes.py`: GET/PUT/POST/DELETE under `/api/efactura/primite/<id>/ke...` and `/api/efactura/ke/...`; registered in `routes/efactura/__init__.py`.
- Client: `KBot.Domain/EFacturaKe.vb` (the data), `KBot.Api/ApiClient.EFactura.Ke.vb` + 7 members of `IEFacturaApi` (only `ApiClient` implements it).

## Files touched
`sql/KE_01_primite_receptii.sql`, `PYTHON/routes/efactura/primite_ubl.py`, `primite.py`, `primite_ke.py`, `primite_ke_routes.py`, `__init__.py`, `src/KBot.Domain/EFacturaKe.vb`, `src/KBot.Api/ApiClient.EFactura.Ke.vb`, `IEFacturaApi.vb`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- The rate path `ClassifiedTaxCategory/Percent` is confirmed on ONE real XML (Digi, 21%); the other real XMLs of 00EF-17 were not re-read. A document-level allowance or charge (UBL `AllowanceCharge`) is not read: it shows as "nealocat din liniile" when it makes the lines differ from the total.
- The reception value per classification (sum of the lines of the last counting snapshot) copies the rule of the receptions view (slice 0065) and was written from reading, not compared with real data.
- KE08 (payments, payment orders) is NOT started: the plan leaves it for after KE01-KE07.
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).
