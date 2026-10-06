# SLICE-00EF-04 - E-Factura token on the server (code written, NOT deployed, NOT run)

Slice 00EF (operator, 06.10.2026). Python only; no VB, no SQL change, nothing run.

## What changed and why
New package `PYTHON/routes/efactura/` (blueprint `efactura_bp`, registered in `main.py`). It is the server half of the
«authorise step» of the plan: the PC does the certificate step, Python holds every secret and every token.

| File | Role |
|---|---|
| `ef_config.py` | Reads the client data from the server ENVIRONMENT (`EF_CLIENT_ID`, `EF_CLIENT_SECRET`, `EF_REDIRECT_URI`, `EF_AUTH_URL`, `EF_TOKEN_URL`, `EF_TOKEN_KEY`, `EF_REFRESH_DAYS`), lazily per call. Missing / non-https -> `EfNotConfigured` carrying variable NAMES only; `repr` hides every field. |
| `cripto.py` | Fernet (package `cryptography`) with `EF_TOKEN_KEY`; ASCII output fits the `text utf8mb3` columns. A key that does not match -> «authorise again». |
| `oauth.py` | The two calls to ANAF's token address with stdlib `urllib`: code exchange and refresh; builds the authorise address (not called from here: the certificate is on the PC). Logs status + ANAF's short `error` code only. |
| `token_store.py` | SQL on `EF_Token` / `EF_TokenStart` (AVACONT_COMUN). Single-use state = one atomic `DELETE ... WHERE State, DC, UN, age < 15 min`. |
| `tokens.py` | The steps: `start`, `finish`, `stare`/`describe`, and `access_token(dc)` (renews first; row locked `FOR UPDATE` so two workers cannot spend the same refresh token). |
| `token_routes.py` | `POST /api/efactura/token/start`, `POST /api/efactura/token/cod`, `GET /api/efactura/token/stare`; bearer (`require_session`), unit = `g.session.db_name`; each authorisation written in `Jurnal` (`EF_TOKEN_AUTORIZARE`). |

Route bodies and error codes are in the docstring of `token_routes.py` (the contract for `KBot.EFactura` in 00EF-05).

What the PC learns (`stare`): `configurat`, `exista`, `cui`, `valabil_pana`, `avertizeaza_de_la`, `zile_ramase`, `avertizeaza`,
`trebuie_reinnoit`, `certificat`, `autorizat_de`, `autorizat_la`, `ultima_eroare`, `ultima_eroare_la` (dates UTC ISO ending in `Z`).
It never receives a token, the client secret, or the key. `access_token(dc)` is not reachable from any route: slices 00EF-06/07 call it.

## Files touched
New: `PYTHON/routes/efactura/{__init__,ef_config,cripto,oauth,token_store,tokens,token_routes}.py`, this file.
Edited: `PYTHON/main.py` (import + `register_blueprint`), `docs/worklog/PLAN_00EF_EFactura.md`, `docs/worklog/KBOT_STATUS.md`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Decisions taken here (state them, change if wrong)
- **No refresh job.** Renewal is lazy, inside `access_token(dc)`: the access token is renewed when it ends within 5 minutes. The refresh token's
  own life is the only thing that forces the certificate step, and a cron would not change it.
- **`RefreshExpiraLa` = moment of the certificate step + `EF_REFRESH_DAYS` and a renewal does NOT move it.** ANAF's answer as `EF.EXE` reads it has
  no refresh lifetime, and whether a new refresh token lives a whole new period is not verified. A warning that comes early is the harmless mistake.
- **A refusal by ANAF (HTTP 4xx other than 408/429) on a refresh** sets `RefreshExpiraLa = now` (the state then says «trebuie reinnoit»); a network
  failure / 5xx only writes `UltimaEroare` and leaves the tokens alone.
- **`state` is NOT put in the authorise address.** `EF.EXE` worked without it and whether ANAF's login service passes it through is not verified. The
  server hands it back separately and checks it came to the same unit and the same user within 15 minutes; it is spent even if the exchange then fails
  (the code is single-use at ANAF too).
- **Any user of the unit may authorise.** No role check: the role values of `Unitati_Utilizatori.Rol` are not known to me. Say if only some roles may.
- The certificate label / thumbprint come from the PC and are DISPLAY ONLY (control characters stripped, cut to the column).
- Both ANAF calls are `application/x-www-form-urlencoded`, encoded. `EF.EXE` sent the first one as `text/plain` un-encoded (see the header of `oauth.py`).

## Test results
`py_compile` of the seven new files and `main.py`: clean. **No test code written and no test run** (standing rule of this repo: no tests unless asked). Nothing
imported on a server, no database touched, no call made to ANAF.

## Unverified / deferred
- **Server prerequisites (operator):**
  1. `pip install cryptography` in the server's virtual environment (it is not in the local venv either).
  2. `sql/00EF_02_efactura_comun.sql` run on `AVACONT_COMUN` (`EF_Token`, `EF_TokenStart`, `EF_UM`); the service account needs SELECT/INSERT/UPDATE/DELETE on them.
  3. `/etc/avacont/efactura.env` (root, 600) from `tools/efactura/efactura.env.example` with the five values of the old `token.txt` + a new `EF_TOKEN_KEY`;
     `tools/efactura/efactura.conf` into `/etc/systemd/system/avacont.service.d/`; `daemon-reload` + restart. Without them `start`/`cod` answer 503
     `EF_NECONFIGURAT` / `TABELE_LIPSA` naming what is missing.
  4. Keep a copy of `EF_TOKEN_KEY`: if it is lost every unit does the certificate step again.
- Whether ANAF accepts the form-encoded code exchange; whether the token answer's `expires_in` is the access token's (assumed, as in `EF.EXE`);
  the real refresh-token life (365 days is an assumption); whether a renewal extends it.
- The route contract is not exercised by any client yet (the VB side is 00EF-05) and not by a request.
- No help change: nothing the operator sees changed (server only).
