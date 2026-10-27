# SLICELESS - EF2 (EF.EXE): the token file route comes back as a bridge (22.10.2026)

## What changed and why
Operator (22.10.2026): the EF2 project (`C:\Users\Adelina Trandafir\source\repos\EF2`, the old `EF.EXE`, a console tool of the Access side) downloads `GET /api/mfp/get_ef_token` (header `X-API-KEY`) -> password-protected `token.zip` with the client data. That route was retired in SLICELESS-securitate-api (S2), so EF2 gets 404 and cannot renew or obtain a token. The operator asked to use the old path again.

- `PYTHON/routes/legacy_updater.py`: `GET /api/mfp/get_ef_token` added to the temporary bridge, same `_guard()` (key read from the server's `config.py` `API_KEY` at request time, `hmac.compare_digest`) and same `LEGACY_UPDATER_ENABLED` switch: `False` -> 404 again. It only SENDS `token.zip` from the server's working directory (as the old route did); the file is never opened, read or logged, and nothing about the key or the zip password is copied into the repo.
- `PYTHON/tests/test_legacy_retired.py`: the path removed from `RETIRED_PATHS` (it is back on purpose, like check_update).
- EF2 itself is NOT changed: it already calls this exact path with `ApiKey` and its own zip password.

## Files touched
`PYTHON/routes/legacy_updater.py`, `PYTHON/tests/test_legacy_retired.py`, this file, `state/KBOT_STATUS_SLICELESS.md`.

## Test results
`py_compile` of `legacy_updater.py` only. No test run (operator rule), nothing deployed, `config.py` untouched.

## Unverified / deferred
- The server must still have `API_KEY` in its `config.py` (the bridge answers 503 without it) and `token.zip` in its working directory: not checkable from here. If `API_KEY` was already removed from the server's `config.py` (securitate-api step 1), the old key must be put back by the operator, by hand.
- The secret travels to every PC again for as long as the bridge is on: set `LEGACY_UPDATER_ENABLED = False` (by hand, on the server) once Access/EF.EXE is retired. The new path (`/api/efactura/token/*`, server-side secrets) is unchanged.
- `docs/security/legacy-route-inventory.json` still lists the old `mfp.py` declaration (historical snapshot, not edited).
- Upload of `routes/legacy_updater.py` to the server is the operator's, by hand.

## Part 2 (22.10.2026): POST /api/tools/upload gets its own key
Why: `API_KEY` is compiled into every client exe, and with it `POST /api/tools/upload` lets anyone put a file into the update cache that every old client installs. The shared key must not open that route.

- `PYTHON/routes/legacy_updater.py`: new `_guard_upload()`; `upload_file` uses it. Key = `LEGACY_UPLOAD_KEY` in the server's `config.py`, header `X-Upload-Key`, `hmac.compare_digest`. **No fallback to `API_KEY`**; missing key = 503; the `LEGACY_UPDATER_ENABLED` switch still gives 404. `py_compile` only.
- Publisher found: `C:\Users\Adelina Trandafir\source\repos\AvacontUpdater` (`MainForm.UploadOne`), NOT `Avacont_Uploader`. It now sends `X-Upload-Key` (value `Secrets.UploadKey` in its git-ignored `secrets.vb`, generated random, 32 bytes). Builds clean (0 warnings, 0 errors). Not run against a server.
- `Avacont_Uploader` (Access-side tool) uses `/api/ftp/init_upload|upload_chunk|finalize_upload` (backups to the FTPS store) and is run by users, so those three routes stay on the shared key: a new key in that exe would be public again.

### Operator, by hand, BEFORE the next publish
1. On the server add to `config.py` the line `LEGACY_UPLOAD_KEY = "<the value of Secrets.UploadKey from AvacontUpdater\secrets.vb>"` and restart Flask. Until then `/api/tools/upload` answers 503 and nothing can be published.
2. Upload `routes/legacy_updater.py` to the server.

### Unverified / deferred
- No call to the real server. The old `Avacont_Uploader`/updaters of other repos (`UpdaterAva`, `Updater25.NET`) only READ (`check_update(s)`); not changed.
- `http://` clients still send the shared key in clear; `AvacontUpdater` itself already uses https.
- `/api/ftp/*` upload routes still write with the shared key (storage, not client installs): candidates for an nginx IP limit.

## Part 3 (23.10.2026): EF2 NOT moved to the new flow (decision: stays as is until Access is retired)
Looked at moving EF2 to `/api/efactura/token/*` (login + certificate step only). Rejected by the operator: the Access side reads the token from the registry (`mdl_2025.Token_Key` / `Token_Expiry` / `Token_RefreshKey`) and calls ANAF itself in many functions (`CautaFacturiNoi`, `DescarcaFactura`, `IncarcaFacturaXML`, `StatusFactura`, `ListaMesaje`, `TrimiteMesajFactura`, `VerificaAnaf`, ...), and the new server never hands a token to the PC. So EF2 keeps the old flow and `GET /api/mfp/get_ef_token` (part 1) stays open as a bridge. Nothing in EF2 changed (a draft `ServerClient.vb` was written and deleted; `git status` of EF2 is clean). Open: the secret keeps travelling to PCs; limit `/api/mfp/get_ef_token` by IP in nginx, and switch the bridge off with `LEGACY_UPDATER_ENABLED = False` once Access/EF.EXE is retired. Unknown: whether Access refreshes the token itself with `RefreshKey`.
