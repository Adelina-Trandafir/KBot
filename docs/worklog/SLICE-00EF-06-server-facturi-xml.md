# SLICE-00EF-06…07 — Serverul facturilor emise și fluxul ANAF

Istoric din 06.10.2026. 07 înlocuiește corecția prin PUT cu retrimiterea tip 384 prin trimite; validarea, încărcarea și citirea stării rămân pași distincți. Modelul inițial EF_Furnizor și DDL-ul 00EF-06 sunt înlocuite ulterior în [13/14](SLICE-00EF-13-date-unitate.md). [28](SLICE-00EF-28-mediu-vps.md) confirmă mediul, fără facturi reale ANAF. Răspunsurile, ipotezele și limitele sunt păstrate pe etapă.

- [00EF-06](#slice-00EF-06)
- [00EF-07](#slice-00EF-07)

## SLICE-00EF-06 - Server: issued invoices (CRUD), UBL XML, validation (code written, py_compile only, NEVER RUN) <a id="slice-00EF-06"></a>

Slice 00EF (operator, 06.10.2026). The server half of the issued invoices: what the screens of 00EF-08 / 00EF-09 will call. Upload, status and download from ANAF are 00EF-07 (not here). No client code, no help: nothing the operator sees changed.

### What changed and why
New files in `PYTHON/routes/efactura/` (all registered through `__init__.py`; `main.py` already registers the blueprint):
- `ubl.py` - the UBL 2.1 / CIUS-RO XML, pure functions. Ported from `mdl_EFactura_Add.GENEREAZA_XML_EFACTURA` + `qFacturi_Vanzare` (read in full): same elements, same order, same constants (RON, VAT category Z at 0 %, `0.0`, DueDate = date + 10 days, empty supplier `TaxScheme`, PaymentMeansCode 42, bank name from `BIC`). Differences from Access are listed in the file header.
- `validare.py` - (1) our own checks (`verifica`: issuer / customer / header / lines; levels `eroare` blocks, `avertisment` does not); (2) the call to ANAF's public validation service (`anaf_valideaza`, `interpret_answer`).
- `facturi_store.py` - SQL for `EF_Furnizor`, `EF_Clienti`, `EF_Facturi`, `EF_FacturiLinii` (unit DB) and the read-only `EF_UM`, `BIC` (common DB).
- `facturi.py` - the rules: states, numbering, edit / correction / delete / storno, XML, validation.
- `factura_routes.py` - the HTTP routes (list at the top of the file). All `@require_session`; unit = session; audit journal for every write that touches a fiscal document or the issuer (`EF_FACTURA_ADAUGA / _MODIFICA / _CORECTEAZA / _STERGE / _STORNO`, `EF_FURNIZOR_MODIFICA`).
- `sql/00EF_06_efactura_numar_initial.sql` (+ the same column added to `00EF_02_efactura_unitate.sql`): `EF_Furnizor.NumarInitial`.
- `tools/efactura/compare_xml.py` - compares the XML Access wrote with the one the server writes (whitespace and attribute order ignored).

#### Answers found in the Access code (they close open questions of the plan)
- **`ValideazaXML_Local` is not local.** It POSTs the XML (without `xsi:schemaLocation`) to `https://webservicesp.anaf.ro/prod/FCTEL/rest/validare/FACT1`, no token, and accepts on `"stare":"ok"`. The new server does the same call (`valideaza` with `{"anaf": true}`) and ADDS its own offline checks, which Access did not have.
- **`Factura.Corectata` / `TipFactura`**: an uploaded-and-accepted invoice (numeric `id_descarcare`) is corrected by resending it with type 384; only the comment and the order reference change and `Corectata` becomes true. (Written first as `PUT /facturi/<id>` on an accepted invoice; **moved in 00EF-07**: the correction is now made by `POST /facturi/<id>/trimite` with a `corectie`, and that PUT answers `SE_CORECTEAZA_PRIN_TRIMITERE`.)
- **`IdFacturaA`**: set on the cancelling invoice (storno) = the invoice it cancels; the storno repeats the original's lines with `Cant` and `Valoare` negated, is uploaded first, and is made in the SAME save that creates the replacement invoice. Access did not fill `SerieFacturaA` / `NumarFacturaA` in that code; the server fills them (series and number of the original, informational). Access did NOT write a `cac:BillingReference` in the storno XML; neither does this.
- **Edit rules** (`bMod_Click`): no `id_incarcare` = editable; uploaded without `id_descarcare` = «do not touch until it is known»; `id_descarcare` = `Err` = never edited again; numeric = may be corrected or cancelled; an invoice that has `IdFacturaA` is never modified.
- **Numbering** (`DMAX_EFACTURA`): highest `NumarFactura` + 1, or `Scheme.C5` when there is none. 00EF-02 dropped C5; it is back as `NumarInitial`. The number and the series are always the server's (the form made the number read-only); the series is `EF_Furnizor.SerieFactura`.

### Decisions taken here (state them, change if wrong)
- **Wire names**: the table columns as they are (ASCII, PascalCase / the Access words); computed fields are lower-case Romanian snake_case (`stare`, `total`, `linii`, `client`, `poate_modifica`, `poate_corecta`, `poate_storna`, `poate_sterge`, `este_storno`). Money and quantities are JSON numbers.
- **`Valoare` of a line is always computed by the server** (`Cant x PU`, half-up, 2 decimals); a `Valoare` sent by the client is ignored. A stored value that differs (migrated data) gives the warning `LINIE_VALOARE` at validation, not an error.
- **Delete**: only a draft that has the LAST number of its series (a gap in the numbering is a fiscal problem). Access had no delete at all.
- **A refused invoice (`Err`) is a dead end**, as in Access: not editable, not deletable. What the operator does with it is an open thread (below).
- **A second storno of the same invoice is refused** (`DEJA_STORNATA`); Access did not check it.
- **Correction accepts only `Comentarii` and `BT_13`**; any other key is refused with `CAMP_NEPERMIS` (no silent ignoring). (Now enforced in `trimitere.py`, 00EF-07.)
- **`anaf: true` is explicit** on `valideaza` (default off): the invoice data go to ANAF only when asked. It is not called when our own checks found errors.
- **Local checks that rest on rules I know but did not verify against ANAF's documents** (so they are WARNINGS or limited to what is certain): county code list ISO 3166-2:RO (error when the code is not in the list), Bucharest city = `SECTOR1..6` (warning only), IBAN shape (warning only).
- Customer `IndFiscal`, `Cont`, `Banca`, `Sector` are stored but, as in Access, do NOT reach the XML.

### Files touched
New: `PYTHON/routes/efactura/{ubl,validare,facturi_store,facturi,factura_routes}.py`, `sql/00EF_06_efactura_numar_initial.sql`, `tools/efactura/compare_xml.py`, this file. Edited: `PYTHON/routes/efactura/__init__.py` (docstring + one import), `sql/00EF_02_efactura_unitate.sql` (column `NumarInitial`), `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/PLAN_00EF_EFactura.md`.

### Test results
`py_compile` of the six `routes/efactura` modules and of `compare_xml.py` with `PYTHON\.venv`: clean. **No test code written and no test run** (standing rule), no scratch run of the XML builder, nothing started, nothing deployed, nothing called at ANAF. **Never run against a database.**

### Unverified / deferred
- **The whole thing on a real unit**: the DDL (00EF-02 + this slice's column) has not been run anywhere; the SQL of `facturi_store.py` was never executed (an error in a statement would show up on the first call).
- **The XML against Access's**: not compared. First step for the operator: write the same invoice in Access and in K-BOT (or take an Access `UPL\*.xml` and recreate its data), then `python tools/efactura/compare_xml.py <access.xml> <server.xml>`. Expected differences are only the ones in the header of `ubl.py`. Whether the supplier `CodFiscal` carries `RO` the way `UNIT.cui` did is a data question: `EF_Furnizor.CodFiscal`.
- **ANAF's validation service**: URL, request shape and the `"stare":"ok"` answer come from the Access code; never called from here. The shape of the failure answer (`Messages[].message`) is an assumption; unknown shapes fall back to the cut text of the answer.
- `User-Agent: K-BOT` instead of Access's fake MSIE string: unverified that ANAF accepts it.
- The Migrator tab «E-Factura» does NOT import `Scheme.C5` into `NumarInitial` (the operator sets it with `PUT /furnizor`); the first number of a series with no invoice is 1 until then. It matters only for such a series.
- Open question 3 of the plan is ANSWERED by the operator (06.10.2026): the Access invoices ARE imported, through the Migrator tab of 00EF-03 (read: it imports `Factura`/`FacturaC` with their own `SerieFactura`/`NumarFactura`, and the series of `EF_Furnizor` from `Scheme.T3`). So the numbering continues after the highest imported number of the series. Unverified: an old invoice whose series differs from T3 would make the current series restart at `NumarInitial`.
- The client side (`IEFacturaApi` methods, Domain POCOs) for these routes is 00EF-08.
- Rounding: VBA rounds half to even on doubles, the server half-up on exact decimals; identical for every amount the tables can hold (two decimals).

## SLICE-00EF-07 - Server: send to ANAF, state, download, messages (code written, py_compile only, NEVER RUN) <a id="slice-00EF-07"></a>

Slice 00EF (operator, 06.10.2026). The server half that talks to ANAF with the unit's stored token. No client code, no help: nothing the operator sees changed. The screens that use these routes are 00EF-08 / 00EF-09.

### Operator decisions (06.10.2026)
1. **Production addresses, exactly as the VBA flow** (`mdl_EFactura.bas.txt`, `mdl_EFactura_Add.bas.txt`): the Access upload "is always right". No test environment. (`StareMesaj` of the VBA points at `/test/`, but it is not the one `TrimiteFactura` uses: `StatusFactura` is, on `/prod/`.)
2. Received invoices stay in 00EF-10: here only the ANAF CALLS for messages (list, download of one). The XML of a received invoice is not read here, and `TrimiteMesajFactura` (a message to a supplier) is not built.
3. **Send, then check 2-3 seconds later; if still «in prelucrare», a re-check option.** Two separate routes; the pause is the screen's (00EF-09).

### What changed and why
New in `PYTHON/routes/efactura/`:
- `anaf_api.py` - the four calls (`upload`, `stare`, `descarca`, `lista_mesaje`) + the reasons inside an ANAF error zip. Shapes ported from `IncarcaFacturaXML`, `StatusFactura`, `DescarcaFactura` / `DescarcaMesaje`, `ListaMesaje`, `ParseXML_ERR`. Token only in the `Authorization` header.
- `trimitere.py` - the rules: `trimite`, `verifica`, `descarca`, `mesaje`, `descarca_mesaj`.
- `trimitere_routes.py` - `POST /facturi/<id>/trimite`, `POST /facturi/<id>/verifica`, `GET /facturi/<id>/descarca`, `GET /mesaje`, `GET /mesaje/<id>/descarca`.
- `sql/00EF_07_efactura_eroare_anaf.sql` (+ same column in `00EF_02_efactura_unitate.sql`): `EF_Facturi.EroareAnaf`.
Edited: `facturi.py` (flags `poate_trimite`, `poate_verifica`, `poate_descarca`; correction through PUT removed, see below), `facturi_store.py` (`EroareAnaf`, `factura_mark_uploaded / _accepted / _refused`, `messages_known`; `factura_correct` removed), `factura_routes.py` (extra fields in refusals), `__init__.py`.

#### The flow
`trimite`: lock the invoice row -> state must allow sending -> our checks (00EF-06) -> ANAF validation service (as Access did before every upload, now enforced by the server) -> token -> upload. **Nothing is written unless ANAF took the file**; then `id_incarcare` stored, `id_descarcare` cleared, `Trimisa` = 1, old `EroareAnaf` cleared. `verifica`: ANAF state -> «ok» + `id_descarcare` = acceptata; an error text or «nok» (also «XML cu erori nepreluat») = `id_descarcare` = `Err` + the reason in `EroareAnaf` (read from ANAF's error zip when it gives one) = refuzata; «in prelucrare» or an unknown state changes nothing, so `verifica` can be repeated.

### Decisions taken here (state them, change if wrong)
- **Correction (type 384) is done BY `trimite`** with `{"corectie": {"Comentarii", "BT_13"}}` on an accepted invoice; the change and type 384 are written only after ANAF took the new file. Why: the 00EF-06 design (store 384 with PUT, send later) cannot tell «correction waiting to be sent» from «correction already accepted» (both: 384 + numeric `id_descarcare`), and Access never wrote `Trimisa`, so imported rows cannot help. **This replaces the 00EF-06 PUT-on-accepted**, which now answers `SE_CORECTEAZA_PRIN_TRIMITERE` (409).
- A key other than `Comentarii` / `BT_13` in `corectie`, or a `corectie` on a draft, is refused (`CAMP_NEPERMIS`).
- **`Trimisa` = 1** when ANAF took the file (new meaning: Access never wrote the column). `state` does not use it.
- **A validator that cannot be reached blocks sending** (`VALIDARE_INDISPONIBILA`), as in Access, where a failed validation call returned False.
- **Refused invoices stay a dead end** (no re-send, no edit, no delete), as in Access. Open thread from 00EF-06, unchanged.
- **The row lock is held during the ANAF calls** (validator + upload: up to ~1.5 minutes in the worst case), so a double click cannot upload twice.
- **If the database write fails after ANAF took the file**, the `index_incarcare` is written to the server log for manual reconciliation (not a secret).
- `descarca` only for an accepted invoice; the zip is streamed as is (the signed XML), named like the XML file with `.zip`. Converting XML to PDF (Access `Export_XML_TO_PDF`) is not here.
- `mesaje`: «FACTURA TRIMISA» is always left out (as Access); `primite=1` (default) keeps only «FACTURA PRIMITA». `deja_in_baza` is computed from `EF_Mesaje`. `zile` 1..60. ANAF's paginated list is not used (Access used the plain one). The supplier name lookup (`InformatiiFirmaOnline`) is not here.

### Files touched
New: `PYTHON/routes/efactura/{anaf_api,trimitere,trimitere_routes}.py`, `sql/00EF_07_efactura_eroare_anaf.sql`, this file. Edited: `PYTHON/routes/efactura/{facturi,facturi_store,factura_routes,__init__}.py`, `sql/00EF_02_efactura_unitate.sql`, `docs/worklog/SLICE-00EF-06-server-facturi-xml.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/PLAN_00EF_EFactura.md`.

### Test results
`py_compile` of the modules, clean. **No test code written and no test run** (standing rule), nothing started, nothing deployed, **nothing called at ANAF**, no database touched. The operator said a test unit is needed: it does not exist here.

### Unverified / deferred
- **Everything against the real ANAF**: the exact XML of the upload and state answers (root attributes `index_incarcare`, `stare`, `id_descarcare`, an error child whose FIRST attribute is the text) is taken from how the VBA read them with MSXML, not from a captured answer. Namespaces in the answer are ignored (attributes are read by name).
- The names of the message keys (`id_solicitare`, `cif_emitent`, `tip`, ...) come from `clsEF_element.Add` and the VBA filters; the real JSON was not seen.
- What an ANAF error zip contains: assumed to be an XML whose root children carry the text in their first attribute (as `ParseXML_ERR` reads); unreadable -> the refusal is still recorded, without the reasons.
- `User-Agent: K-BOT` instead of Access's fake MSIE string (also in 00EF-06): unverified that ANAF accepts it.
- The token step (00EF-04/05) has never been run either: `tokens.access_token` is the first thing every call here depends on.
- Whether an upload of the SAME number as type 384 is accepted by ANAF is what Access relied on (`TipFactura = 384`); not tested here.
- The pause of 2-3 seconds and the re-check button are the screen's job (00EF-09); no automatic polling on the server (operator decision, 3).
- DDL: `EroareAnaf` must exist (00EF-02 as now, or the ALTER script) before `verifica` can run; without it every call on `EF_Facturi` answers `TABELE_LIPSA`.
