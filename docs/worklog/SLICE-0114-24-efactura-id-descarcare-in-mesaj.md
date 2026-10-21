# SLICE-0114-24 - E-Factura: the download id is named in the ANAF download error (17.10.2026 request, 7.3 still open)

## What changed and why
Operator item 7.3 (view «Factura ANAF» of an issued invoice): «nu cred ca foloseste ID-ul corect pentru descarcare, de obicei incepe cu 4xxx»; later: the error text differs from error to error, so there is no single text to send. The path was read end to end: the client sends only the invoice id (`GET /facturi/{id}/pdf-anaf`) -> `trimitere.pdf_anaf` -> `trimitere.descarca` -> `anaf_api.descarca(token, row["id_descarcare"])`; `id_descarcare` is written only by `verifica` (`stareMesaj` -> attribute `id_descarcare`, the 4xxx number) when ANAF says «ok», and the state «acceptata» needs it numeric. Nothing in the code picks `id_incarcare` (5xxx) for the download. So a wrong id can only be a WRONG STORED VALUE (older rows, or written outside this code). To make the next occurrence readable without asking for a database read, `anaf_api.descarca` now ends its two errors with «(id descarcare folosit: N)».

## Files touched
`PYTHON/routes/efactura/anaf_api.py` (`descarca`). Not deployed (the operator does that by hand).

## Test results
Syntax checked only; no test run. Nothing seen on screen.

## Unverified / deferred
- 7.3 CLOSED by the operator (21.10.2026, «considera inchisa directia asta»); what follows is the note kept for the record: when the error shows, the id in the brackets tells whether the stored value is the 4xxx download id or the 5xxx upload id; if it is the upload id, the fix is data (re-run «Verifica» / the state check for that invoice) or a repair routine, to be decided with that evidence.
- The text is also shown for the other ANAF downloads (messages, received invoices), which use the same function.
