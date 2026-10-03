# SLICE-0103-05 — VBA function: Access → the NEW MariaDB (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

A VBA function so that, from the Access «Clasificatii» form, the changes of the budget / rectifications are sent to the NEW
MariaDB database (the K-BOT server), not to the one the VBA already works with. Correlation: Access `IdUnitate` + `IDClsf` ==
MariaDB `Clasificatii.IdUnitate` + `Clasificatii.IdClsfAcc`.

- **Server** `routes/clasificatii.py`: `POST /api/clasificatii/sync_acc_kbot` (`X-Api-Key`, the VBA guard), writes through
  `get_kbot_connection` = DB_CONFIG_NEW. Body: `db_name, id_unitate, an, [data_inceput], clasificatii[{IdClsfAcc, Trim1..4, rectificari[]}]`.
  Budget → `Clasificatii_Buget` version `(IdClsf, An, DataInceput)` (upsert; without `data_inceput` the latest version of the year
  is edited, 01.01 if none); rectifications → `Clasificatii_Rectificari` upsert on `(IdClsf, Data, Document)`. A classification that
  resolves to zero or several MariaDB rows is returned in `unmatched` / `ambiguous` and skipped. One transaction.
- **VBA** `docs/vba/mdl_FX_CLS_SYNC_TO_KBOT.bas` (importable text module): `FX_CLS_Sync_To_KBot(IdUnitate, An, [IDClsf], [DataInceput])`,
  same style as `mdl_FX_DDF_SYNC_TO_MARIADB` (Dictionary, `ConvertToJson`, WinHttp, `X-API-Key`).

## Files touched

`PYTHON/routes/clasificatii.py`, `docs/vba/mdl_FX_CLS_SYNC_TO_KBOT.bas` (new).

## Test results

No tests written or run. Python compiles (`py_compile`). The VBA was NOT compiled or run (no Access here).

## Left unverified / deferred

- The operator must IMPORT the module into Access and add a call on the Clasificatii form (an example is in the module header).
- **URL fixed (operator reported `API_BASE_URL = http://adcredit.avatarsoft.ro:5008/api`):** `KbotUrl()` keeps everything up to and including `/api` and appends
  `/clasificatii/sync_acc_kbot`; the first draft looked for `/api/` and would have produced `/api/api/...` (404). That host resolves to 89.33.25.34 (the
  new server); what listens on port 5008 (the repo's `gunicorn.conf.py` binds 5009) and which `API_KEY` it holds were NOT verified.
- The module assumes `API_BASE_URL` (the part up to `/api`), `API_KEY`, `DC()`, `ConvertToJson`, `Dictionary` exist in the project, as the
  DDF sync module uses them; their definitions are not in the export. The VBA names `Clasificatii` / `Rectificari` columns as
  documented in `docs/MAPARE_NOMENCLATOARE.md`, not as read from the live Access file.
- Which budget version an Access edit means is decided by `data_inceput` (VBA argument); with none, the latest version of the year.
- Server file not deployed.
