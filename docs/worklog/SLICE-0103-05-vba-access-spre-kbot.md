# SLICE-0103-05 / 06 — Puntea de bugete între Access și K-BOT

Reunire documentară D001-24. Sunt păstrate separat sensurile Access → K-BOT (VBA/rută) și K-BOT → Access (bugetul în vigoare și rectificări, fără ștergere). Căile componentei și presupunerile despre cale.accdb/parolă descriu reperul din 02.10.2026; registrul 0104 consemnează mutarea paralelă în KBot.Access și ulterior setarea căii. Nu este declarată integrarea lor verificată. La ambele checkpointuri: Branch: `SLICE-0100-Multithreading` (nothing committed).

[0103-05](#slice-0103-05) · [0103-06](#slice-0103-06)

## SLICE-0103-05 — VBA function: Access → the NEW MariaDB (operator request, 02.10.2026) <a id="slice-0103-05"></a>

### What changed and why

A VBA function so that, from the Access «Clasificatii» form, the changes of the budget / rectifications are sent to the NEW MariaDB database (the K-BOT server), not to the one the VBA already works with. Correlation: Access `IdUnitate` + `IDClsf` == MariaDB `Clasificatii.IdUnitate` + `Clasificatii.IdClsfAcc`.

- **Server** `routes/clasificatii.py`: `POST /api/clasificatii/sync_acc_kbot` (`X-Api-Key`, the VBA guard), writes through `get_kbot_connection` = DB_CONFIG_NEW. Body: `db_name, id_unitate, an, [data_inceput], clasificatii[{IdClsfAcc, Trim1..4, rectificari[]}]`. Budget → `Clasificatii_Buget` version `(IdClsf, An, DataInceput)` (upsert; without `data_inceput` the latest version of the year is edited, 01.01 if none); rectifications → `Clasificatii_Rectificari` upsert on `(IdClsf, Data, Document)`. A classification that resolves to zero or several MariaDB rows is returned in `unmatched` / `ambiguous` and skipped. One transaction.
- **VBA** `docs/vba/mdl_FX_CLS_SYNC_TO_KBOT.bas` (importable text module): `FX_CLS_Sync_To_KBot(IdUnitate, An, [IDClsf], [DataInceput])`, same style as `mdl_FX_DDF_SYNC_TO_MARIADB` (Dictionary, `ConvertToJson`, WinHttp, `X-API-Key`).

### Files touched

`PYTHON/routes/clasificatii.py`, `docs/vba/mdl_FX_CLS_SYNC_TO_KBOT.bas` (new).

### Test results

No tests written or run. Python compiles (`py_compile`). The VBA was NOT compiled or run (no Access here).

### Left unverified / deferred

- The operator must IMPORT the module into Access and add a call on the Clasificatii form (an example is in the module header).
- **URL fixed (operator reported `API_BASE_URL = http://adcredit.avatarsoft.ro:5008/api`):** `KbotUrl()` keeps everything up to and including `/api` and appends `/clasificatii/sync_acc_kbot`; the first draft looked for `/api/` and would have produced `/api/api/...` (404). That host resolves to 89.33.25.34 (the new server); what listens on port 5008 (the repo's `gunicorn.conf.py` binds 5009) and which `API_KEY` it holds were NOT verified.
- The module assumes `API_BASE_URL` (the part up to `/api`), `API_KEY`, `DC()`, `ConvertToJson`, `Dictionary` exist in the project, as the DDF sync module uses them; their definitions are not in the export. The VBA names `Clasificatii` / `Rectificari` columns as documented in `docs/MAPARE_NOMENCLATOARE.md`, not as read from the live Access file.
- Which budget version an Access edit means is decided by `data_inceput` (VBA argument); with none, the latest version of the year.
- Server file not deployed.

---

## SLICE-0103-06 — «Trimite în Access»: K-BOT budget / rectifications → the unit's Access file (operator request, 02.10.2026) <a id="slice-0103-06"></a>

### What changed and why

A button in K-BOT's «Clasificații bugetare» window that sends the K-BOT changes of the budget and the rectifications to Access, to the correct database (the unit's, found through the registry).

- **`Views/Nomenclatoare/AccessBudgetSender.vb`** (new, `System.Data.OleDb` 8.0.0 added to `KBot.App`; ACE 64-bit only — operator, 02.10.2026: users have 64-bit Access only):
  - `ResolveUnitFile`: `C:\AVACONT\cale.accdb` ▸ `cai.FullPath` of the classification's `IdUnitate` (relative to the registry folder);
  - the files open without a password first, then with the shared one;
  - `Send`: in one transaction `UPDATE Clasificatii SET Trim1..4 WHERE IDClsf = <IdClsfAcc>` with the budget version in force today, and
    every rectification of the year into `Rectificari` (found on IdClsf + Data + Document → updated, else inserted with Capitol..Alineat
    copied from the Access classification row). Nothing is deleted from Access.
- **`ClasificatiiForm`**: button «Trimite în Access» (enabled when a classification is chosen and nothing is unsaved), asks first, re-reads the SAVED budget from the server, then sends; the result and every failure are shown (KBotMessage → operator log).
- `Clasificatie` (Domain) now carries `IdUnitate` and `IdClsfAcc` (the tree route returns them).

### Files touched

`src/KBot.App/Views/Nomenclatoare/AccessBudgetSender.vb` (new), `ClasificatiiForm.vb` + `.Designer.vb`, `KBot.App.vbproj`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `PYTHON/routes/forexe/clasificatii_edit.py`.

### Test results

No tests written or run. `dotnet build src/KBot.App/KBot.App.vbproj` → 0 warnings, 0 errors. Never run against a real Access file.

### Left unverified / deferred

- **«din registru» was read as the AVACONT registry `cale.accdb` (`cai`)**, the same one the Migrator calls «registrul». If the operator meant the Windows registry (`HKCU\Software\VB and VBA Program Settings\AVACONT\<DC>`), the lookup in `ResolveUnitFile` changes; nothing in the export says that key holds a path. The registry path is the constant `AccessBudgetSender.DefaultRegistryPath`, not a setting.
- **Password (operator, 02.10.2026):** one password shared by all the Access files, hard-coded in `AccessBudgetSender` in XOR-obfuscated form (it only hides the text from a look at the binary; it is not protection). A file is opened without it first, then with it (`Jet OLEDB:Database Password`). If ACE rejects the password on an UNprotected file, the first attempt already covers that case. The password now lives in `KBot.Common/AccessFilePassword.vb` and the Migrator uses it too (operator, 02.10.2026): `AccessProvider.Open` tries the typed password (when any), then the shared one, then none. KBot.Common 1.5.11.0, KBot.Migrator 1.17.1.0.
- `dotnet build src/KBot.App` currently fails in `Update/AppUpdateService.vb` (`UpdateProgressForm` constructor now asks for `access`): that is someone else's in-progress edit of the update system, not part of this slice; the Migrator builds clean.
- Which budget goes to Access: the version in force today into `Clasificatii.Trim1..4` (Access keeps one budget row per classification).
- Access columns come from `docs/MAPARE_NOMENCLATOARE.md`, not from the live file; `Esinc` / `DTQ` are left to Access.
- Writing while the Access application has the file open exclusively fails; the message says so.
