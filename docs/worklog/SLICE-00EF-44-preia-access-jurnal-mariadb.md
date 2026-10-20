# SLICE-00EF-44 - «Preia din Access»: details in the MariaDB journal (20.10.2026)

## What changed and why
A client's «Preia din Access» (00EF-43) brought only part of the received invoices (the operator says the first month, about 10 messages, while the unit has 100-200; the Migrator then brought the rest correctly). The journal showed only `mesaje=10 facturi=10 ...` and `mesaje=2 facturi=2 ...`, written only when a batch added something, so the cause could not be told from the database. The operator asked for more in the journal, to debug this and the next case. (An earlier attempt in the same thread, a «one-time button» with a mark in `Setari`, was **undone** at the operator's request while the cause is unknown; nothing of it remains.)

- **Every batch writes a row** now, also when nothing new was added: `EF_PRIMITE_ACCESS_PREIA`. `Rezultat` = `OK` | `PROBLEME` (operations left out) | `ERORI` (messages not written) | `EROARE` (the call failed; the row says the error type, reason code and text).
- **The end of a run writes a row** always: `EF_PRIMITE_ACCESS_FINAL` (references, automatic link to receptions, ms). A run with PREIA rows and no FINAL row was cut off on the PC.
- **`Detalii` is a line of `key=value`.** From the server, per batch: `mesaje_primite`, `prima` / `ultima` id, `luni` (first..last month of the messages), `fara_factura`, `fara_arhiva`, `mesaje_noi/sarite`, `facturi_noi/sarite`, `linii`, `note`, `mesaje_anaf`, `operatii(_sarite)`, `erori`, `probleme`, `erori_lista` (first 5, `id: motiv`), `ms`.
- **From the PC** (new optional `context` in the body of `preia` and `finalizeaza`; old clients send none): `an`, `fisier` (file name only), `cui` used to filter, `lot` / `loturi` (slice of 10 ids), `cerute` (ids asked), `citite_lot` (messages returned for them), `citite` (read so far), `acces_total` (ids Access listed for the unit), `necitite_total` and `necitite` (the first 20 ids asked and NOT returned). This is what tells «Access gave only 12 ids» from «Access gave 150, only 12 were read».
- A slice that returns nothing sends no batch, so it leaves no PREIA row; it shows in the FINAL row (`citite` < `acces_total`, `necitite=...`).

Query for a run:
`SELECT Id, Moment, Actiune, Rezultat, Detalii FROM AVACONT_COMUN.Jurnal WHERE Actiune LIKE 'EF_PRIMITE_ACCESS%' AND DC = '<unit db>' ORDER BY Moment;`

## Files touched
`PYTHON/routes/efactura/primite_access_jurnal.py` (new: the text of the rows, the checked `context`); `primite_access_routes.py` (rows on every call, also on failure); `factura_routes.py` (`_audit` takes an optional `result`); `src/KBot.Domain/AccessEfDtos.vb` (`AccessPreluareContext`); `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura.Access}.vb` (`PreiaAccessAsync` and `FinalizeazaAccessAsync` take the context); `src/KBot.EFactura/Views/Primite/PrimiteView.Access.vb` (builds it per slice). FileVersion: Domain 1.2.17 -> 1.2.18, Api 1.0.26 -> 1.0.27, EFactura 1.8.0 -> 1.8.1 (KBot.App left to `push-update.ps1`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `py_compile` of the three Python modules: ok. The journal text functions were called once with made-up data (sample lines looked as intended). **Nothing else was run**: no server call, no Access file, nothing on screen, no test run (operator's rule).

## Unverified / deferred
- **Server files must be sent to the VPS by hand**: `primite_access_jurnal.py` (new), `primite_access_routes.py`, `factura_routes.py` (+ restart). With the old server the new client's `context` is ignored and the rows stay as before.
- The cause of the short run is **still unknown**. Next run on a test copy, read the PREIA / FINAL rows; the PC's `Logs\mesaje_operator.log` has the boxes the operator read.
- `Jurnal.Detalii` is cut at 3000 characters; `Rezultat` is `varchar(16)`, the values fit.
- The new `Rezultat` values (`PROBLEME`, `ERORI`, `EROARE`) are used for the first time by this action; nothing reads `Rezultat` by value in the code searched (not checked on the Administrare pages).
- Help and release notes: nothing written (operator's rule: nothing about the Access system goes into the help or NOUTATI).
