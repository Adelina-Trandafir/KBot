# SLICE-01EF-02 — «Contari»: the page and the server calculation (no write)

## What changed and why
The first view of the bar of `PrimiteView` (window with all the received invoices only) is now **«Contari»**. While it is shown the tree has tick boxes (invoices and suppliers); the invoices of ONE supplier can be ticked at a time (ticking another supplier takes the others off, with a note); every change of ticks, classification, template or value to pay asks the server to calculate again. The page shows: classification and the two templates (evidence, payment), the supplier with account and codes, the warnings, the evidence DGV (Access EF_F without the FOREXE part; only «De plătit» is edited), the payment DGV (EF_P: one row), and the bookings the chosen templates would produce. **Nothing is saved** (01EF-04/05). The default selected view stays «Conținut».

Server `POST /api/efactura/contari/calcul` (`primite_contari.py`, rules in its header): Suma = signed total minus `EF_PrimiteOperatii`; `NumarDocument` = digits of the last word of the number (reading of QEF_EV, inferred); templates `Tip` M, evidence `Fel` E / payment `Fel` P, fitting by `Cheie` = `Clasificatii.ClsfX` and journal sector+source = `Clasificatii.SS`, lines of the form (`Subcapitol`) or without form; classification suggested from `Parteneri_Coduri`, else the only one of article 20 that has a template; codes and bank account from `Parteneri_Coduri`, else `Parteneri.ContIBAN`; warnings: partner missing, invoice already complete, credit note, other year (the client sends `an`), duplicates in `EF_Primite`, codes missing, no bank account, template without a line for the form, explanation over 254. Explanation of the O.P.: «F <numbers> <FelOP>».

## Files touched
- Server: `PYTHON/routes/efactura/primite_contari.py`, `primite_contari_routes.py`, `__init__.py`.
- Domain/Api: `KBot.Domain/EFacturaContari.vb`, `KBot.Api/ApiClient.EFactura.Contari.vb`, `IEFacturaApi.vb`.
- Client: `Views/Primite/PrimiteContarePage.vb` + `.Designer.vb`, `PrimiteView.Contare.vb`, hooks in `PrimiteView.vb` and `PrimiteView.Designer.vb` (new nav item `contare` first, `pgContare`, tick boxes through `_contare`).
- Help: 0000-144 (`contabil/efactura/index.md`).

## Test results
`py_compile` of both Python files; `numar_document` checked on 4 strings; `dotnet build` KBot.Api, KBot.EFactura, KBot.App: 0 errors, 0 warnings. **Nothing run, nothing seen on screen, server not deployed** (the operator tests at the end).

## Unverified / deferred
- Left out because the tables do not exist yet (AV stage 2): next O.P. number, accounting-note (NC) numbers, closed-month rule, duplicate documents in `Documente`, the client code «C <cod>» of the O.P. (a column K-BOT does not have).
- The fallback of the codes through `Parteneri_Angajamente(_Indicatori)` (by tax code) is not done: it needs the link to `FX_Indicatori`.
- Editing «De plătit» in the DGV relies on `CellValueChanged` firing on commit: not seen working.
- Role: reading is open; `poate_scrie` only informs (no write route yet).
