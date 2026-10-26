# SLICE-00EF-43…44 — Transferul facturilor primite cu Access și diagnosticarea preluării

43 introduce transferul și cheile naturale. 44 adaugă jurnalul pe lot și la final după o preluare incompletă: cauza incidentului rămâne necunoscută, iar propunerea butonului de unică folosință a fost anulată. Jurnalizarea nu dovedește remedierea.

- [00EF-43](#slice-00EF-43)
- [00EF-44](#slice-00EF-44)

## SLICE-00EF-43 - Received invoices: «Trimite in Access» and «Preia din Access» (17.10.2026) <a id="slice-00EF-43"></a>

### What changed and why
Operator request: in the received-invoices FORM (not in a view), in the ribbon's FORM group (the right-hand one), only for Access users, two buttons:
1. **Trimite in Access** - K-BOT -> Access, into EF, EFT, EFS, EFT_C, EFT_M (not EFT_O).
2. **Preia din Access** - what the Migrator's «E-Factura» tab does for the received invoices, directly, without the operator choosing files or ticks.

Decisions (operator, 17.10.2026): rows are recognised by their **natural key**, not by the Access number (both systems create rows, the numbers collide): message = `id_sol`; invoice = `id_sol` + `NrFact` + normalised CUI; the buttons are on the **received invoices window only** (EF/EFT/EFS are the received tables).

- **Server** (`PYTHON/routes/efactura/`): `primite_access.py` (rules), `primite_access_legare.py` (the automatic link to receptions, a port of the Migrator's `EfReceptionLinker`, 00EF-38), `primite_access_routes.py`, registered in `__init__.py`.
  - `GET /api/efactura/primite/access/stare` - the unit's tax code (`Unitati.CF`).
  - `POST .../access/preia` - a batch of Access messages with invoices, lines, notes, ANAF messages and booked operations (EFT_O); one transaction per message. A message / invoice already here is left as it is; the children of an invoice already here are added only when missing (notes by text, ANAF messages by id+text, operations by `IDEFTO`). The CPV of the new lines is read from the archive (`primite_ubl`). Operations resolve their classification on `(IdClsfAcc, IdUnitate)`; a pair that is not exactly one row is reported and that operation is skipped (a later preluare adds it).
  - `POST .../access/finalizeaza` - credit-note references between the added invoices, then the automatic link to receptions (only the added invoices; same rules as 00EF-38).
  - `POST .../access/exporta` - the messages of the year whose `IdSol` the file does not have, with everything under them (about 4 MB of archives per call).
- **Access side** (`src/KBot.Access/AccessEfStore.vb`, through the existing `IAccessBridge`, five new methods): file = `<folder of cale.accdb>\EFACTURA\ef_<year>.accdb` (the place the Migrator guesses first); reads by message in chunks; writes in one transaction with fresh AutoNumbers (`@@IDENTITY`), text cut to Access's 255 characters (counted and told), `NC` = -1 for a credit note that counts against the account (as the Access import writes it).
- **Client**: DTOs `KBot.Domain/AccessEfDtos.vb`; `IEFacturaApi` + `ApiClient.EFactura.Access.vb` (4 methods); the work in `PrimiteView.Access.vb` (ask -> count -> «Continuați?» -> batches with progress in the tree footer -> summary), thin wrappers in `PrimiteForm`; `KbotForm.RunPrimiteAccess`.
- **Ribbon**: two buttons in the FORM group (`ef_access_trimite`, `ef_access_preia`, made in `AvacontMain.Designer.vb`), visible only while the received invoices window is on screen AND `AccessFeature.Enabled` (the unit's `Setari.Access` on + the Access component installed) - `AvacontMain.UpdateFormGroup`.

### Files touched
`PYTHON/routes/efactura/{__init__,primite_access,primite_access_legare,primite_access_routes}.py`; `src/KBot.Domain/AccessEfDtos.vb`; `src/KBot.Common/AccessBridge.vb`; `src/KBot.Access/{AccessEfStore,AccessBridgeImpl,AccessBudgetSender}.vb`; `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura.Access}.vb`; `src/KBot.EFactura/{Forms/PrimiteForm,Views/Primite/PrimiteView.Access}.vb`; `src/KBot.App/{AvacontMain,AvacontMain.Designer,KbotForm.EFactura}.vb`. FileVersion: Domain 1.2.15 -> 1.2.16, Common 1.5.13 -> 1.5.14, Api 1.0.23 -> 1.0.24, EFactura 1.6.0 -> 1.7.0, Access 1.0.1 -> 1.0.2 (KBot.App left to `push-update.ps1`).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. The three new Python modules compile and import in the venv. **Nothing was run**: no test run (operator's rule), no Access file opened, no server call, nothing seen on screen.

### Unverified / deferred
- **Server files must be sent to the VPS** (`primite_access*.py`, `__init__.py`); without them the buttons answer with a server error. Nothing deployed.
- The Access side was written from the table layouts in `Surse/RawExport/EF_ACCDB/tables` and the import code in `mdl_EFactura.bas.txt`; never run against a real `ef_<year>.accdb` (OLE Object write of `EF.XML`, `@@IDENTITY` through ACE, the `IN (?)` lists, the schema lookup are all untried).
- The store is assumed at `<registry folder>\EFACTURA\ef_<year>.accdb` (the Migrator's guess); no setting to point at another place. A missing file is told to the operator with its full path.
- **Trimite** works per MESSAGE: a message the file already has is skipped whole, so notes / ANAF messages added in K-BOT to an invoice Access already has are not sent; nothing that links an invoice to receptions or classifications is sent either. `EF_PrimiteTVA` (rates) has no Access table.
- **Preia** does not set `Nou` / user notes on an existing invoice beyond adding missing ones; the CPV is read only for the invoices it adds. Operations whose classification does not resolve are skipped and listed (first 8 shown).
- The invoice key is `id_sol + NrFact + CUI`, the CUI only compared when both sides have it.
- Help and release notes: nothing written (operator's rule: nothing about the Access system goes into the help or NOUTATI).

## SLICE-00EF-44 - «Preia din Access»: details in the MariaDB journal (20.10.2026) <a id="slice-00EF-44"></a>

### What changed and why
A client's «Preia din Access» (00EF-43) brought only part of the received invoices (the operator says the first month, about 10 messages, while the unit has 100-200; the Migrator then brought the rest correctly). The journal showed only `mesaje=10 facturi=10 ...` and `mesaje=2 facturi=2 ...`, written only when a batch added something, so the cause could not be told from the database. The operator asked for more in the journal, to debug this and the next case. (An earlier attempt in the same thread, a «one-time button» with a mark in `Setari`, was **undone** at the operator's request while the cause is unknown; nothing of it remains.)

- **Every batch writes a row** now, also when nothing new was added: `EF_PRIMITE_ACCESS_PREIA`. `Rezultat` = `OK` | `PROBLEME` (operations left out) | `ERORI` (messages not written) | `EROARE` (the call failed; the row says the error type, reason code and text).
- **The end of a run writes a row** always: `EF_PRIMITE_ACCESS_FINAL` (references, automatic link to receptions, ms). A run with PREIA rows and no FINAL row was cut off on the PC.
- **`Detalii` is a line of `key=value`.** From the server, per batch: `mesaje_primite`, `prima` / `ultima` id, `luni` (first..last month of the messages), `fara_factura`, `fara_arhiva`, `mesaje_noi/sarite`, `facturi_noi/sarite`, `linii`, `note`, `mesaje_anaf`, `operatii(_sarite)`, `erori`, `probleme`, `erori_lista` (first 5, `id: motiv`), `ms`.
- **From the PC** (new optional `context` in the body of `preia` and `finalizeaza`; old clients send none): `an`, `fisier` (file name only), `cui` used to filter, `lot` / `loturi` (slice of 10 ids), `cerute` (ids asked), `citite_lot` (messages returned for them), `citite` (read so far), `acces_total` (ids Access listed for the unit), `necitite_total` and `necitite` (the first 20 ids asked and NOT returned). This is what tells «Access gave only 12 ids» from «Access gave 150, only 12 were read».
- A slice that returns nothing sends no batch, so it leaves no PREIA row; it shows in the FINAL row (`citite` < `acces_total`, `necitite=...`).

Query for a run: `SELECT Id, Moment, Actiune, Rezultat, Detalii FROM AVACONT_COMUN.Jurnal WHERE Actiune LIKE 'EF_PRIMITE_ACCESS%' AND DC = '<unit db>' ORDER BY Moment;`

### Files touched
`PYTHON/routes/efactura/primite_access_jurnal.py` (new: the text of the rows, the checked `context`); `primite_access_routes.py` (rows on every call, also on failure); `factura_routes.py` (`_audit` takes an optional `result`); `src/KBot.Domain/AccessEfDtos.vb` (`AccessPreluareContext`); `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura.Access}.vb` (`PreiaAccessAsync` and `FinalizeazaAccessAsync` take the context); `src/KBot.EFactura/Views/Primite/PrimiteView.Access.vb` (builds it per slice). FileVersion: Domain 1.2.17 -> 1.2.18, Api 1.0.26 -> 1.0.27, EFactura 1.8.0 -> 1.8.1 (KBot.App left to `push-update.ps1`).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `py_compile` of the three Python modules: ok. The journal text functions were called once with made-up data (sample lines looked as intended). **Nothing else was run**: no server call, no Access file, nothing on screen, no test run (operator's rule).

### Unverified / deferred
- **Server files must be sent to the VPS by hand**: `primite_access_jurnal.py` (new), `primite_access_routes.py`, `factura_routes.py` (+ restart). With the old server the new client's `context` is ignored and the rows stay as before.
- The cause of the short run is **still unknown**. Next run on a test copy, read the PREIA / FINAL rows; the PC's `Logs\mesaje_operator.log` has the boxes the operator read.
- `Jurnal.Detalii` is cut at 3000 characters; `Rezultat` is `varchar(16)`, the values fit.
- The new `Rezultat` values (`PROBLEME`, `ERORI`, `EROARE`) are used for the first time by this action; nothing reads `Rezultat` by value in the code searched (not checked on the Administrare pages).
- Help and release notes: nothing written (operator's rule: nothing about the Access system goes into the help or NOUTATI).
