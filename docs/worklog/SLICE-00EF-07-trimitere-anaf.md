# SLICE-00EF-07 - Server: send to ANAF, state, download, messages (code written, py_compile only, NEVER RUN)

Slice 00EF (operator, 06.10.2026). The server half that talks to ANAF with the unit's stored token. No client code, no help:
nothing the operator sees changed. The screens that use these routes are 00EF-08 / 00EF-09.

## Operator decisions (06.10.2026)
1. **Production addresses, exactly as the VBA flow** (`mdl_EFactura.bas.txt`, `mdl_EFactura_Add.bas.txt`): the Access upload "is always right".
   No test environment. (`StareMesaj` of the VBA points at `/test/`, but it is not the one `TrimiteFactura` uses: `StatusFactura` is, on `/prod/`.)
2. Received invoices stay in 00EF-10: here only the ANAF CALLS for messages (list, download of one). The XML of a received invoice is not read here,
   and `TrimiteMesajFactura` (a message to a supplier) is not built.
3. **Send, then check 2-3 seconds later; if still «in prelucrare», a re-check option.** Two separate routes; the pause is the screen's (00EF-09).

## What changed and why
New in `PYTHON/routes/efactura/`:
- `anaf_api.py` - the four calls (`upload`, `stare`, `descarca`, `lista_mesaje`) + the reasons inside an ANAF error zip. Shapes ported from
  `IncarcaFacturaXML`, `StatusFactura`, `DescarcaFactura` / `DescarcaMesaje`, `ListaMesaje`, `ParseXML_ERR`. Token only in the `Authorization` header.
- `trimitere.py` - the rules: `trimite`, `verifica`, `descarca`, `mesaje`, `descarca_mesaj`.
- `trimitere_routes.py` - `POST /facturi/<id>/trimite`, `POST /facturi/<id>/verifica`, `GET /facturi/<id>/descarca`, `GET /mesaje`, `GET /mesaje/<id>/descarca`.
- `sql/00EF_07_efactura_eroare_anaf.sql` (+ same column in `00EF_02_efactura_unitate.sql`): `EF_Facturi.EroareAnaf`.
Edited: `facturi.py` (flags `poate_trimite`, `poate_verifica`, `poate_descarca`; correction through PUT removed, see below), `facturi_store.py`
(`EroareAnaf`, `factura_mark_uploaded / _accepted / _refused`, `messages_known`; `factura_correct` removed), `factura_routes.py` (extra fields in refusals),
`__init__.py`.

### The flow
`trimite`: lock the invoice row -> state must allow sending -> our checks (00EF-06) -> ANAF validation service (as Access did before every upload,
now enforced by the server) -> token -> upload. **Nothing is written unless ANAF took the file**; then `id_incarcare` stored, `id_descarcare` cleared,
`Trimisa` = 1, old `EroareAnaf` cleared. `verifica`: ANAF state -> «ok» + `id_descarcare` = acceptata; an error text or «nok» (also «XML cu erori nepreluat») =
`id_descarcare` = `Err` + the reason in `EroareAnaf` (read from ANAF's error zip when it gives one) = refuzata; «in prelucrare» or an unknown state changes
nothing, so `verifica` can be repeated.

## Decisions taken here (state them, change if wrong)
- **Correction (type 384) is done BY `trimite`** with `{"corectie": {"Comentarii", "BT_13"}}` on an accepted invoice; the change and type 384 are written only after ANAF
  took the new file. Why: the 00EF-06 design (store 384 with PUT, send later) cannot tell «correction waiting to be sent» from «correction already accepted»
  (both: 384 + numeric `id_descarcare`), and Access never wrote `Trimisa`, so imported rows cannot help. **This replaces the 00EF-06 PUT-on-accepted**, which now answers
  `SE_CORECTEAZA_PRIN_TRIMITERE` (409).
- A key other than `Comentarii` / `BT_13` in `corectie`, or a `corectie` on a draft, is refused (`CAMP_NEPERMIS`).
- **`Trimisa` = 1** when ANAF took the file (new meaning: Access never wrote the column). `state` does not use it.
- **A validator that cannot be reached blocks sending** (`VALIDARE_INDISPONIBILA`), as in Access, where a failed validation call returned False.
- **Refused invoices stay a dead end** (no re-send, no edit, no delete), as in Access. Open thread from 00EF-06, unchanged.
- **The row lock is held during the ANAF calls** (validator + upload: up to ~1.5 minutes in the worst case), so a double click cannot upload twice.
- **If the database write fails after ANAF took the file**, the `index_incarcare` is written to the server log for manual reconciliation (not a secret).
- `descarca` only for an accepted invoice; the zip is streamed as is (the signed XML), named like the XML file with `.zip`. Converting XML to PDF (Access `Export_XML_TO_PDF`) is not here.
- `mesaje`: «FACTURA TRIMISA» is always left out (as Access); `primite=1` (default) keeps only «FACTURA PRIMITA». `deja_in_baza` is computed from `EF_Mesaje`.
  `zile` 1..60. ANAF's paginated list is not used (Access used the plain one). The supplier name lookup (`InformatiiFirmaOnline`) is not here.

## Files touched
New: `PYTHON/routes/efactura/{anaf_api,trimitere,trimitere_routes}.py`, `sql/00EF_07_efactura_eroare_anaf.sql`, this file.
Edited: `PYTHON/routes/efactura/{facturi,facturi_store,factura_routes,__init__}.py`, `sql/00EF_02_efactura_unitate.sql`,
`docs/worklog/SLICE-00EF-06-server-facturi-xml.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/PLAN_00EF_EFactura.md`.

## Test results
`py_compile` of the modules, clean. **No test code written and no test run** (standing rule), nothing started, nothing deployed, **nothing called at ANAF**,
no database touched. The operator said a test unit is needed: it does not exist here.

## Unverified / deferred
- **Everything against the real ANAF**: the exact XML of the upload and state answers (root attributes `index_incarcare`, `stare`, `id_descarcare`, an error child whose
  FIRST attribute is the text) is taken from how the VBA read them with MSXML, not from a captured answer. Namespaces in the answer are ignored (attributes are read by name).
- The names of the message keys (`id_solicitare`, `cif_emitent`, `tip`, ...) come from `clsEF_element.Add` and the VBA filters; the real JSON was not seen.
- What an ANAF error zip contains: assumed to be an XML whose root children carry the text in their first attribute (as `ParseXML_ERR` reads); unreadable -> the refusal
  is still recorded, without the reasons.
- `User-Agent: K-BOT` instead of Access's fake MSIE string (also in 00EF-06): unverified that ANAF accepts it.
- The token step (00EF-04/05) has never been run either: `tokens.access_token` is the first thing every call here depends on.
- Whether an upload of the SAME number as type 384 is accepted by ANAF is what Access relied on (`TipFactura = 384`); not tested here.
- The pause of 2-3 seconds and the re-check button are the screen's job (00EF-09); no automatic polling on the server (operator decision, 3).
- DDL: `EroareAnaf` must exist (00EF-02 as now, or the ALTER script) before `verifica` can run; without it every call on `EF_Facturi` answers `TABELE_LIPSA`.
