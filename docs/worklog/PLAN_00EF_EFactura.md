# PLAN 00EF - K-BOT E-Factura (project `KBot.EFactura`, inside `KBot.App`)

Slice number **00EF** (operator, 06.10.2026). Sub-slices are `00EF-01`, `00EF-02`, ... Starting point: the issued-invoice
screens of Access (`EFACTURA_ADD` and what hangs off it). Everything about Access comes from the raw export in
`Surse/RawExport` and the maps in `docs/access-map/efactura/` (read `00-overview.md` there first).

## Decisions locked by the operator (06.10.2026)

1. **`KBot.EFactura` is a project of the K-BOT solution, used directly by `KBot.App`** (third pass: it is NOT a standalone exe). Same
   process, same session, same theme, same logging: `KBot.App` references it, like it references `KBot.Xfa` and `KBot.Forexe`. Its
   windows open from the main app's menu, new entry **«E-FACTURA»**. No session hand-off, no second instance, no copied files. The
   operator typed `KBOT.EFACTURA`; the repo convention is `KBot.<Name>`, so the project is `src/KBot.EFactura` (assumption).
2. **It uses every K-BOT system** (theme, logger, custom controls, messages, tooltips, scaling, API client): see the table below.
3. **Start with `EFACTURA_ADD`** = issued invoices. Received invoices (`EFACTURA_2025`) come later, in their own sub-slices.
4. **The received-invoice tables are in `Surse/RawExport/EF_ACCDB`** (export of `EF_2025.accdb`): `EF`, `EFS`, `EFT`, `EFT_C`, `EFT_M`,
   `EFT_O`, `Ver`. The issued-invoice tables `Factura`, `FacturaC`, `ClientiEF` are linked from each unit's `baza2026.accdb`; only their
   field lists are in `Surse/RawExport/tables/`. All of them live in **the unit database** in MariaDB, like the other per-unit tables.
5. **ANAF token**: moves to MariaDB, encrypted, **one per unit (CUI)**. Obtaining it (first token, refresh) is **code inside
   `KBot.EFactura` and the Python server**; the external `EF.EXE` is not used and was only a reference (`Surse/EF_SURSA`).
6. **Secrets stay in Python.** The client secret (and everything else that is secret) is never sent to a PC; Python uses it and gives the
   app back only results (see "The authorise step").
7. **Ignore anything named `in_lucru`, `old` or `copy`** (also the `_` prefix: `_MF2019_Asoc_EF`, `_qNote_*`, `EF_F_inlucru`).
8. **The accounting part is NOT in this slice**: `SaveEFactura`, `Oper` / `Documente` / `PlatiFacturi`, `EFT_O`, the `QEF_*`
   queries that feed the booking lists, `EF_F` / `EF_P`, the `Note` / `MF2019` association forms.
9. **New view «E-Factura» in `KbotForm`**: e-invoices linked to the partner attached to the current DDF, joined on
   `FX_DDF_Parteneri.CodFiscal` (unit DB: `IdDdfPartener`, `IDDF`, `CodFiscal varchar(255)`, `NumePartener`, unique `(IDDF, CodFiscal)`).

## Shared K-BOT systems (all used directly: the project lives in the same process)

`KBot.EFactura` references `Domain`, `Common`, `Api`, `Theming`, `Controls` (and nothing from `KBot.App`: references go App ->
EFactura, never back). Names were checked to exist in `src/`.

| System | Where | Rule |
|---|---|---|
| Theme engine | `KBot.Theming` (`ThemeManager`, `ThemeStore`, `KBotTheme`) | colours only from `ThemeManager.Current.Palette`; the app already initialised it; the windows follow theme changes (`IThemedControl`) |
| Base forms | `KBotShellForm` (borderless), `KBotThemedForm` (dialogs), `KBotThemedUserControl` | every window inherits one of them; all controls declared in `.Designer.vb` |
| Scaling / DPI | `AppScaling` | `AutoScaleMode.Dpi` with the designer dpi stamp |
| Custom controls | `KBot.Controls` (`KBotDataView`, text fields, combos, tables, `CaptionBar`, `BusyBar`, `Notice`, `Popup`, menus, `NavList`, ...) | no stock control where a K-BOT one exists; a missing control is added to `KBot.Controls/<Family>/` (never inside `KBot.EFactura`) and implements `IThemedControl` |
| Tooltips | `KBotToolTip` | never `System.Windows.Forms.ToolTip`; texts in Romanian |
| Operator messages | `KBotMessage.Show` (also `Logs\mesaje_operator.log` via `OperatorLog`) | never `MessageBox.Show` / `MsgBox` |
| Error log | `GlobalErrorLog.Write("Type.Method", ex)` | the Try/Catch policy of CLAUDE.md (boundaries: log + rethrow; UI handlers: log + swallow) |
| API + session | `KBot.Api` (`ApiClient`, https-only `ApiOptions`), `SessionContext` | the same client and session as the rest of the app; e-invoice calls are new methods / a new part of `KBot.Api` talking to new server routes |
| Help | `src/KBot.App/HelpContent/` (F1, tours) | the new screens get their topics there (help slice `0000-NN`, tag `00EF`), same engine; nothing new to build |
| Menu, window hosting, updater, single instance | `KBot.App` | `KBot.EFactura` ships inside the same release and updater; the menu entry and the window opening are in `KBot.App` |

Consequences:
- Nothing from `KBot.App` is copied or moved: `KBot.EFactura` is reached from it. Anything the e-invoice windows need from the main
  window (the selected unit, year, source, the current DDF) is passed as plain arguments by `KBot.App`.
- The Access screens are redrawn with K-BOT controls and the K-BOT look, not reproduced pixel for pixel; the Access positions
  (`docs/access-map/efactura/forms/`) are a layout guide.
- The project is added to `KBot.sln` and `KBot.App.vbproj`. A new class library in the solution does not change any existing project
  except the one reference and the menu / view code in `KBot.App`.

## What the Access side does for issued invoices (verified from the export)

`EFACTURA_ADD` is a list (`EFACTURA_ADD_FACTURI`: `Factura` + `ClientiEF`) next to a tabbed editor: Generale, Cumparator
(`EFACTURA_CLIENTI`), Vanzator (`EFACTURA_VANZATOR` + `EFACTURA_PickIBAN`), Atasamente, Continut (`EFACTURA_ADD_SUB` over
`tmpFacturaC`, unit picker `EFACTURA_UM` over `EF_UM`). Save writes `Factura` + `FacturaC`; `TrimiteFactura` then runs
`GENEREAZA_XML_EFACTURA` (UBL XML to `<CALEEF>\OUT`), `ValideazaXML_Local`, `IncarcaFacturaXML` (POST to
`https://api.anaf.ro/prod/FCTEL/rest/upload?standard=UBL&cif=<CUI>`, stores `id_incarcare`), `StatusFactura` (GET `stareMesaj`,
stores `id_descarcare` or `Err`). Unit settings are read from `Scheme` (`NumeForm='DPIFV'`, columns `C2`, `T3`...).
Details: `docs/access-map/efactura/`.

## Target shape

```
KBot.App (menu «E-FACTURA», view in KbotForm)
   |  in-process
KBot.EFactura (windows + certificate step) --K-BOT bearer--> Flask server (PYTHON/routes/efactura/) --ANAF OAuth bearer--> ANAF
                                                              MariaDB: invoices, customers, encrypted tokens         upload / status / download
```

- **The server makes every ANAF call** with the stored token; the PC never holds an access or refresh token, and never sees a secret.
- **Data**: new MariaDB tables for issued invoices, lines, customers, units of measure and the unit's e-invoice settings (replacing
  `Factura`, `FacturaC`, `ClientiEF`, `EF_UM`, `Scheme` row `DPIFV`), plus the received-invoice tables the DDF view reads, all in the unit
  database (`000_DEMO` layout); the encrypted token table is per unit as well (one token per CUI). `utf8mb3_general_ci` (house rule).
- **XML**: generated on the server (port of `GENEREAZA_XML_EFACTURA` and its `Add*` helpers): one implementation, the app never
  builds UBL. Local validation (`ValideazaXML_Local`) is read first to see what it checks.

## How the token is obtained today (verified: `Surse/EF_SURSA/Program.vb`, read in full 06.10.2026; reference only)

1. `EF.exe <DC>` (first run) or `EF.exe <DC> /r` (refresh). `DC` is exactly 8 characters = the unit.
2. It downloads the application's client data from the K-BOT server: `GET .../api/mfp/get_ef_token` (header `X-API-KEY`), answered with a
   password-protected zip (`token.zip`, a file in the server's project root; route `get_ef_token` in `PYTHON/routes/mfp.py`) that holds
   `token.txt` with five lines: `URI|` redirect address, `CID|` client id, `SID|` client secret, `ATH|` authorise address, `TKN|` token
   address. **The values are not in the repo** (a server file) and were NOT read.
3. First run only: lists the valid certificates of the Windows store (current user): private key present, not expired, NOT exportable,
   not a Microsoft software provider, a known hardware provider (SafeNet, Athena, eToken, Gemalto, Certum, ...) or with the Client
   Authentication usage, name not containing `test` / `localhost`. The operator picks one.
4. **No browser.** With that certificate attached to an `HttpClient` (TLS client certificate) it calls
   `GET https://logincert.anaf.ro/anaf-oauth2/v1/authorize?response_type=code&client_id=<CID>&redirect_uri=<URI>&token_content_type=jwt`,
   follows the redirects (up to 3 tries) and reads `code=` from the query of the FINAL address (the redirect target must answer 2xx).
5. It exchanges the code: `POST <TKN>` with `code`, `token_content_type=jwt`, `client_id`, `client_secret`, `redirect_uri`,
   `grant_type=authorization_code`. The answer has `access_token`, `refresh_token`, `expires_in` (seconds); expiry kept as UTC
   `yyyy-MM-dd HH:mm:ss`.
6. Saved in the Windows registry (`HKCU\Software\VB and VBA Program Settings\AVACONT\<DC>\Tokens`: `Token`, `RefreshKey`, `TokenExpiry`).
7. **Refresh needs no certificate**: `POST <TKN>` with `refresh_token`, `grant_type=refresh_token`, `client_id`, `client_secret`;
   it returns a new access AND refresh token, both saved again.

Weak points the new design removes: the client secret is shipped to every PC (inside a zip whose password, and the API key, are written
in the EF source) and the token lives in each user's registry. The key and the zip password must NOT be copied into any doc, test or
code of K-BOT. Once the new route exists, `get_ef_token` and `token.zip` stop being needed by anything in K-BOT (the Access side's
`EF.EXE` keeps using it until Access is retired; not touched here).

## The authorise step (design, from the operator's third pass)

The qualified certificate is on a hardware token on the operator's PC, so the TLS handshake with `logincert.anaf.ro` can only happen
there. So the step is split by what is secret:

| Step | Runs in | Secret involved |
|---|---|---|
| 1. ask the server to start (`POST /api/efactura/token/start`, unit known from the session) | `KBot.EFactura` -> Python | none |
| 2. Python answers with the **authorise address** it built (`ATH` + `client_id` + `redirect_uri` + `token_content_type=jwt`) and a one-time `state` | Python -> app | `client_id` and the redirect address are NOT secret (they travel in the URL ANAF sees); the **client secret never leaves Python** |
| 3. list eligible certificates (rules of step 3 above), operator picks one, mTLS `GET` to the authorise address, read `code` from the final address | `KBot.EFactura` (VB) | the certificate's private key stays on the token; it is never exported |
| 4. send `code` + `state` (`POST /api/efactura/token/cod`) | app -> Python | the one-time code |
| 5. Python exchanges the code with the client secret, encrypts and stores access + refresh token and expiry for the unit | Python | **client secret, tokens: server only** |
| 6. Python answers with results only: ok, expiry, certificate/unit label | Python -> app | none |
| later: every ANAF call and every refresh | Python alone | refresh needs no certificate |

Where the client data live on the server: in the server's own configuration / environment (the three secret values from today's
`token.zip` copied once by the operator), not in the repo. The app asks `GET /api/efactura/token/stare` for «token valid until ...» and
offers «Autorizează din nou» when it is missing, expired or the refresh failed.

**Settled (operator, 06.10.2026, fourth pass):** the app never gets a token. "Return the values" means the RESULTS of what Python
downloads from the e-invoice API (invoices, statuses, messages), not token values. The only token fact the PC knows is the
**expiration**: Python renews the access token alone; the date that matters is when the **refresh token** stops working (about once a
year), because then the certificate step must be done again. From **7 days before** that date the app shows a notice and offers
«Reînnoiește tokenul» (= the certificate step above). The server gives the app, per unit: `valid until`, `warn from`, `must renew`,
certificate label, who authorised, last error text (`GET /api/efactura/token/stare`).
The server computes the refresh expiry (ANAF's token answer carries only `expires_in` for the access token as far as `EF.EXE` reads
it): authorisation time + `EF_REFRESH_DAYS` (365, to be confirmed in 00EF-03).

## Where the secrets go on the server (operator asked for the path)

`/etc/avacont/efactura.env` (owner `root`, mode `600`), read by systemd through the drop-in
`/etc/systemd/system/avacont.service.d/efactura.conf` (`EnvironmentFile=`; the repo has the template
`tools/efactura/efactura.env.example` and the drop-in `tools/efactura/efactura.conf`). Variables: `EF_CLIENT_ID`, `EF_CLIENT_SECRET`,
`EF_REDIRECT_URI`, `EF_AUTH_URL`, `EF_TOKEN_URL`, `EF_TOKEN_KEY` (encrypts the tokens in the database), `EF_REFRESH_DAYS`. An
`EnvironmentFile` and not `Environment=` lines, because `systemctl show` prints `Environment=` values to any user.

## 00EF-02 data model (written, NOT run)

`sql/00EF_02_efactura_unitate.sql` (run on `AVACONT_SURSA`, then schema sync to every unit database; 9 tables) and `sql/00EF_02_efactura_comun.sql` (`AVACONT_COMUN`: `EF_Token`,
`EF_TokenStart`, `EF_UM`). Why the token table is common and not per unit: the server's refresh job walks one table; `Unitati(DC, CF)` is there.
Mapping and decisions are in the headers of the two files.

## Revised sub-slices

| Sub-slice | Content | Needs from the operator |
|---|---|---|
| 00EF-01 | Analysis: access-map docs + this plan; status registered | done |
| 00EF-02 | Data model: `sql/00EF_02_efactura_unitate.sql` + `sql/00EF_02_efactura_comun.sql` (invoices, lines, customers, UM, issuer settings, received-invoice tables without accounting, encrypted token tables); `Scheme` `T3` = invoice series, `C2` = tick new received invoices before download (read from the code) | **written, not run**; DDL to be run by the operator on every unit DB + AVACONT_COMUN |
| 00EF-03 | **Migrator, tab «E-Factura»** (`src/KBot.Migrator/EFactura/`, `MigratorForm.EFactura.vb`): imports the issuer (UNIT + Scheme DPIFV), customers, issued invoices + lines, and the received invoices + lines/notes/messages (no accounting) from the unit's Access files; Verify, then Import in one transaction. **EF_UM is NOT migrated** (the operator writes `AVACONT_COMUN.EF_UM` on the server; the tab only reads it to flag unknown codes on lines) | **written, builds clean, never run** |
| 00EF-04 | Server token: configuration of the client data, encrypted store, `start` / `cod` / `stare`, exchange, refresh | **written** (`PYTHON/routes/efactura/`, `py_compile` only, not deployed, no tests): needs `pip install cryptography`, the 00EF-02 DDL and the values in `/etc/avacont/efactura.env` placed by the operator (never in chat or repo). Worklog `SLICE-00EF-04-server-token.md` |
| 00EF-05 | `KBot.EFactura` project (library, in `KBot.sln`, referenced by `KBot.App`) + the authorise step (certificate list, mTLS call, `start` / `cod` / `stare`) + menu entry «E-FACTURA» opening the first window | **written, builds clean, never run** (`SLICE-00EF-05-proiect-efactura-autorizare.md`; help 0000-54). Needs a real hardware certificate to test on |
| 00EF-06 | Server: invoice CRUD, UBL XML generation, validation | **written** (`PYTHON/routes/efactura/{ubl,validare,facturi_store,facturi,factura_routes}.py`, `py_compile` only, not deployed, never run; `SLICE-00EF-06-server-facturi-xml.md`). Needs the operator to compare the XML with Access's (`tools/efactura/compare_xml.py`). `ValideazaXML_Local` turned out to call ANAF's public validation service, not to validate locally |
| 00EF-07 | Server: upload, status, download, messages (ANAF calls with the stored token) | **written** (`PYTHON/routes/efactura/{anaf_api,trimitere,trimitere_routes}.py`, `py_compile` only, not deployed, nothing called at ANAF; `SLICE-00EF-07-trimitere-anaf.md`). PRODUCTION addresses as the VBA (operator decision); send, then check 2-3 s later with a re-check (the pause is the screen's, 00EF-09). Needs a test unit with a real token |
| 00EF-08 | Screen `EFACTURA_ADD` in K-BOT: list, tabs, customers, seller + IBAN, lines + UM | |
| 00EF-09 | Send flow in the UI (generate, validate, send, status, retry) | |
| 00EF-10 | View «E-Factura» in `KbotForm` (join by `CodFiscal`, normalising `RO` / spaces) + the received-invoice list it reads (WITHOUT accounting) | confirm what the view shows |
| 00EF-11 | Help (`0000-NN`, tag `00EF`) for the menu entry, the view and the screens | |

## Open questions now

1. **Server prerequisites for 00EF-03** (not mine to decide): the library for encrypting the tokens (`cryptography` is NOT in the project's venv; it must be installed on the VPS too) and the three secret values placed in `/etc/avacont/efactura.env` by the operator.
2. **Rows of `EF_UM`**: written by the operator on the server in `AVACONT_COMUN` (not migrated); the file is `Surse/RawExport/tables/TABLE_VALUES/EF_UM.txt`.
3. **Invoice numbering**: Access made `NumarFactura` unique on its own; the new key is (series, number). ANSWERED (operator, 06.10.2026): the existing Access invoices ARE imported, by the Migrator tab «E-Factura» (00EF-03, already written: `Factura` + `FacturaC` with their own series and number). The server numbers a new invoice as highest number of its series + 1, so after the import the sequence continues by itself; `NumarInitial` only matters for a series with no invoice. Caveat: if old invoices carry a series different from `EF_Furnizor.SerieFactura` (T3), the current series restarts at `NumarInitial`.
4. **`Factura.Corectata`** and **`IdFacturaA/SerieFacturaA/NumarFacturaA`**: ANSWERED in 00EF-06 from `EFACTURA_ADD.bSAV_Click`: `Corectata` = an accepted invoice resent as type 384; `IdFacturaA` = the invoice a storno cancels (the storno repeats its lines negated and is saved together with the replacement). See `SLICE-00EF-06-server-facturi-xml.md`.
5. **Refused invoices** (`id_descarcare` = `Err`): Access never edited them again and the server does not either (no edit, no delete). What the operator should do with one is open.
6. **First invoice number**: `Scheme.C5` is now `EF_Furnizor.NumarInitial` (used only for a series with no invoice); the Migrator does not import it, and after the invoice import it is not needed.

## Verified vs assumed

- Verified (read): the Access form/tab/subform structure, the table fields, the upload/status call shapes, the whole token flow in
  `Surse/EF_SURSA/Program.vb`, the `FX_DDF_Parteneri` columns in `MariaDB_Schema/000_DEMO.sql`, the `get_ef_token` route.
- Not verified: the values in `token.zip`; the lifetimes of the tokens (`expires_in` is whatever ANAF answers); the registered redirect
  address (the redirect target must answer 2xx for step 3 to work: unknown how it behaves outside `EF.exe`); the contents of `Scheme`
  rows; the XML produced by `GENEREAZA_XML_EFACTURA` (not run or compared); nothing in the portal or server was run.
