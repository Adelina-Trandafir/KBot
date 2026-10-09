# SLICE-00EF-16 — «Cont emitent» din lista unitatii, corectia 384, anul din K-BOT + filtru pe luni, PDF atasat

Operator request, 09.10.2026, answering the open threads of 00EF-13.

## What changed and why
1. **«Cont emitent»** (`cmbContPlata`) now offers FIRST the unit's own IBANs (`GET /furnizor/conturi`, kept in `_conturi`), then the
   accounts of earlier invoices; a new invoice starts from the only own account when the unit has one. The list is read once with the
   invoices and again after «Conturi Unitate» closes.
2. **«Corectează factura (tip 384)»** is a new menu item for an ACCEPTED invoice that is not a storno (`poate_corecta`, already on
   the server). New dialog `CorectieForm` (comment + order reference), then the send goes through the existing
   `POST /facturi/<id>/trimite` with `corectie` and the usual wait + state read. «Modifică factura» is unchanged: drafts only, never
   a 384 (the server already refuses anything else).
3. **Year and months**: `FacturiForm` takes the year of the session (`SessionContext.An`) and asks the server for that year only
   (`an=`; the server now defaults the limit to 2000 when `an` is given, so a year is not cut at 500). Above the tree a combo
   «Toate lunile» + 12 months filters the tree on the client. No search box (not wanted).
4. **«Atașează factura originală»** is now honoured: `FacturaPdf.WriteInvoice` draws the classic PDF on the PC, the bytes travel in
   the send body (`atasament_pdf`, base64), `trimitere._attachment` checks them (flag on the invoice, `%PDF`, <= 3 MB) and
   `ubl.build(attachment_pdf=...)` writes BG-24 (`cac:AdditionalDocumentReference` / `cac:Attachment` /
   `cbc:EmbeddedDocumentBinaryObject`, `mimeCode="application/pdf"`) right after `cac:OrderReference`. An invoice with the flag
   cannot be sent without the PDF (error `ATASAMENT_LIPSA`). Applies to a first send, a storno send and a correction.
5. County + city from ANAF («Preia de la ANAF») were already done in 00EF-14 (`adresa_anaf.py`: ISO county code, Bucharest -> `B` +
   `SECTOR1..6`); nothing new here. Note: the form is `SECTOR1`, no dash, as CIUS-RO asks and `validare.py` checks.
6. Received invoices left for a new slice (operator).

## Files touched
`PYTHON/routes/efactura/{ubl,trimitere,trimitere_routes,factura_routes}.py`,
`src/KBot.Domain/EFacturaFacturi.vb` (`PoateCorecta`), `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura.Facturi,ApiClient.EFactura.Trimitere}.vb`
(`SendFacturaAsync` gained the PDF parameter, new `SendCorectieAsync`), `src/KBot.EFactura/{FacturiForm.vb,.Designer.vb,.Arbore.vb,.Furnizor.vb,.Trimitere.vb}`,
`Views/Vanzare/VanzareGeneralePage.Designer.vb` (tooltip), new `Forms/CorectieForm.{vb,Designer.vb}`,
`src/KBot.App/KbotForm.EFactura.vb` (passes the year), help `contabil/efactura/index.md` (0000-60).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. One scratch run of `ubl.build` with an attachment and of
`trimitere._attachment` in the venv (XML element order and the three outcomes looked right) — a scratch check, no test written.
Nothing else run: no server deployed, no route called, no ANAF call, no window opened.

## Unverified / deferred
- **Never seen on screen**: the month combo above the tree (`pnlArbore`/`cmbLuna`, 37 px), `CorectieForm` layout.
- **Server not deployed**: `ubl.py`, `trimitere.py`, `trimitere_routes.py`, `factura_routes.py` must go to the VPS + restart; a new
  client against the old server sends `atasament_pdf` and gets «CAMP_NEPERMIS».
- ANAF may refuse the attachment element (a real upload was never tried); the validator route `facturi.py` (`xml`/`valideaza`)
  still builds the XML WITHOUT the PDF.
- The classic PDF of a correction is drawn from the stored comment, not the new one.
- The invoice list is by year only; a new invoice dated outside the K-BOT year is not in the tree after saving.
- A refused invoice still has no way out (waiting for the operator's decision).
- Received invoices: new slice.
