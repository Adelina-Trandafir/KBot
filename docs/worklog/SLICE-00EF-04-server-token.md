# SLICE-00EF-04…05 — Tokenul serverului și autorizarea pe calculator

Istoric din 06.10.2026. 04 păstrează secretele/tokenurile pe server și reînnoirea la cerere; 05 alege certificatul înainte de start, înlocuind ordinea de proiectare inițială. Starea „nerulat / nedeployat” precede [confirmarea operatorului din 13.10](SLICE-00EF-28-mediu-vps.md), ale cărei limite rămân explicite. Nota de 7 zile la pornire nu este construită, iar o factură reală la ANAF nu este confirmată prin aceste etape.

- [00EF-04](#slice-00EF-04)
- [00EF-05](#slice-00EF-05)

## SLICE-00EF-04 - E-Factura token on the server (code written, NOT deployed, NOT run) <a id="slice-00EF-04"></a>

Slice 00EF (operator, 06.10.2026). Python only; no VB, no SQL change, nothing run.

### What changed and why
New package `PYTHON/routes/efactura/` (blueprint `efactura_bp`, registered in `main.py`). It is the server half of the «authorise step» of the plan: the PC does the certificate step, Python holds every secret and every token.

| File | Role |
|---|---|
| `ef_config.py` | Reads the client data from the server ENVIRONMENT (`EF_CLIENT_ID`, `EF_CLIENT_SECRET`, `EF_REDIRECT_URI`, `EF_AUTH_URL`, `EF_TOKEN_URL`, `EF_TOKEN_KEY`, `EF_REFRESH_DAYS`), lazily per call. Missing / non-https -> `EfNotConfigured` carrying variable NAMES only; `repr` hides every field. |
| `cripto.py` | Fernet (package `cryptography`) with `EF_TOKEN_KEY`; ASCII output fits the `text utf8mb3` columns. A key that does not match -> «authorise again». |
| `oauth.py` | The two calls to ANAF's token address with stdlib `urllib`: code exchange and refresh; builds the authorise address (not called from here: the certificate is on the PC). Logs status + ANAF's short `error` code only. |
| `token_store.py` | SQL on `EF_Token` / `EF_TokenStart` (AVACONT_COMUN). Single-use state = one atomic `DELETE ... WHERE State, DC, UN, age < 15 min`. |
| `tokens.py` | The steps: `start`, `finish`, `stare`/`describe`, and `access_token(dc)` (renews first; row locked `FOR UPDATE` so two workers cannot spend the same refresh token). |
| `token_routes.py` | `POST /api/efactura/token/start`, `POST /api/efactura/token/cod`, `GET /api/efactura/token/stare`; bearer (`require_session`), unit = `g.session.db_name`; each authorisation written in `Jurnal` (`EF_TOKEN_AUTORIZARE`). |

Route bodies and error codes are in the docstring of `token_routes.py` (the contract for `KBot.EFactura` in 00EF-05).

What the PC learns (`stare`): `configurat`, `exista`, `cui`, `valabil_pana`, `avertizeaza_de_la`, `zile_ramase`, `avertizeaza`, `trebuie_reinnoit`, `certificat`, `autorizat_de`, `autorizat_la`, `ultima_eroare`, `ultima_eroare_la` (dates UTC ISO ending in `Z`). It never receives a token, the client secret, or the key. `access_token(dc)` is not reachable from any route: slices 00EF-06/07 call it.

### Files touched
New: `PYTHON/routes/efactura/{__init__,ef_config,cripto,oauth,token_store,tokens,token_routes}.py`, this file. Edited: `PYTHON/main.py` (import + `register_blueprint`), `docs/worklog/PLAN_00EF_EFactura.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Decisions taken here (state them, change if wrong)
- **No refresh job.** Renewal is lazy, inside `access_token(dc)`: the access token is renewed when it ends within 5 minutes. The refresh token's own life is the only thing that forces the certificate step, and a cron would not change it.
- **`RefreshExpiraLa` = moment of the certificate step + `EF_REFRESH_DAYS` and a renewal does NOT move it.** ANAF's answer as `EF.EXE` reads it has no refresh lifetime, and whether a new refresh token lives a whole new period is not verified. A warning that comes early is the harmless mistake.
- **A refusal by ANAF (HTTP 4xx other than 408/429) on a refresh** sets `RefreshExpiraLa = now` (the state then says «trebuie reinnoit»); a network failure / 5xx only writes `UltimaEroare` and leaves the tokens alone.
- **`state` is NOT put in the authorise address.** `EF.EXE` worked without it and whether ANAF's login service passes it through is not verified. The server hands it back separately and checks it came to the same unit and the same user within 15 minutes; it is spent even if the exchange then fails (the code is single-use at ANAF too).
- **Any user of the unit may authorise.** No role check: the role values of `Unitati_Utilizatori.Rol` are not known to me. Say if only some roles may.
- The certificate label / thumbprint come from the PC and are DISPLAY ONLY (control characters stripped, cut to the column).
- Both ANAF calls are `application/x-www-form-urlencoded`, encoded. `EF.EXE` sent the first one as `text/plain` un-encoded (see the header of `oauth.py`).

### Test results
`py_compile` of the seven new files and `main.py`: clean. **No test code written and no test run** (standing rule of this repo: no tests unless asked). Nothing imported on a server, no database touched, no call made to ANAF.

### Unverified / deferred
- **Server prerequisites (operator):**
  1. `pip install cryptography` in the server's virtual environment (it is not in the local venv either).
  2. `sql/00EF_02_efactura_comun.sql` run on `AVACONT_COMUN` (`EF_Token`, `EF_TokenStart`, `EF_UM`); the service account needs SELECT/INSERT/UPDATE/DELETE on them.
  3. `/etc/avacont/efactura.env` (root, 600) from `tools/efactura/efactura.env.example` with the five values of the old `token.txt` + a new `EF_TOKEN_KEY`; `tools/efactura/efactura.conf` into `/etc/systemd/system/avacont.service.d/`; `daemon-reload` + restart. Without them `start`/`cod` answer 503 `EF_NECONFIGURAT` / `TABELE_LIPSA` naming what is missing.
  4. Keep a copy of `EF_TOKEN_KEY`: if it is lost every unit does the certificate step again.
- Whether ANAF accepts the form-encoded code exchange; whether the token answer's `expires_in` is the access token's (assumed, as in `EF.EXE`); the real refresh-token life (365 days is an assumption); whether a renewal extends it.
- The route contract is not exercised by any client yet (the VB side is 00EF-05) and not by a request.
- No help change: nothing the operator sees changed (server only).

## SLICE-00EF-05 - KBot.EFactura project + the certificate step + menu entry «E-Factura» (code written, builds clean, NEVER RUN) <a id="slice-00EF-05"></a>

Slice 00EF (operator, 06.10.2026). Client side of the token: the project, the step with the hardware certificate, the first window, the menu entry. The server half is 00EF-04 (`SLICE-00EF-04-server-token.md`).

### What changed and why
- **New project `src/KBot.EFactura`** (net8.0-windows, WinForms, in `KBot.sln` under the «UI» folder, referenced by `KBot.App`; it references Domain, Common, Api, Theming, Controls and nothing from App). FileVersion 1.0.0.0.
  - `Certificates/CertificateRules.vb` + `CertificateFinder.vb` + `AnafCertificate.vb`: the rules of the old `EF.EXE` (`LoadCertificates`) ported rule for rule: certificate with private key and not expired, key NOT exportable, not a Microsoft software store, a known hardware maker OR Client Authentication, CN without `test` / `localhost`. Only reads the store and the key's properties; never exports, copies or signs.
  - `Authorization/AnafAuthorizeClient.vb`: the call to ANAF's authorise address over TLS with the client certificate (`HttpClientHandler`, redirects followed, 3 attempts as in `EF.EXE`), `code` read from the FINAL address. Never logs the address or the code.
  - `Authorization/TokenAuthorizer.vb`: the order list certificates -> operator picks -> `POST token/start` -> call with certificate -> `POST token/cod`. Nothing reaches the server before a certificate is chosen. The PC never holds a token or a secret.
  - `Views/TokenForm` («E-Factura — tokenul ANAF»: state sentence, 7-day notice, unit / CUI / certificate / who / last error, buttons «Reîmprospătează», «Autorizează» / «Reînnoiește tokenul», «Închide») and `Views/CertificatePickerForm` («E-Factura — alegerea certificatului»). Both `KBotThemedForm`, borderless, all controls in `.Designer.vb`, `KBotDataView` / `KBotNotice` / `KBotBusyBar` / `KBotCaptionBar` / `KBotToolTip`, `KBotMessage` for messages.
- **`KBot.Api`**: `IEFacturaApi` + `ApiClient.EFactura.vb` (`GetTokenStateAsync`, `StartTokenAuthorizationAsync`, `SubmitTokenCodeAsync`); wire names exactly the server's (ASCII). FileVersion 1.0.20.0.
- **`KBot.Domain`**: `EFacturaToken.vb` (`EFacturaTokenStart`, `EFacturaTokenState`). FileVersion 1.2.12.0.
- **`KBot.Common`**: `ReauthGate` MOVED here from `KBot.App\Views\Nomenclatoare\` (it depends on nothing in the shell; `KBot.EFactura` cannot reference App). The five existing users already `Imports KBot.Common`, so nothing else changed. FileVersion 1.5.13.0.
- **`KBot.App`**: reference to `KBot.EFactura`; header menu row «E-Factura» (key `efactura`, icon `binvoice`, after «Extrase», in `KbotForm.Designer.vb`); `Case "efactura"` in `MenuNou_ItemClicked`; `KbotForm.EFactura.vb` (`DeschideEFactura`: needs an open unit, modeless, one at a time). FileVersion of App NOT bumped here: `push-update.ps1` asks which part to bump.
- Help: topic `contabil.efactura` + menu row in `contabil.fereastra` + the main-window tour step (worklog `SLICE-0000-54-ajutor-efactura.md`).

### Files touched
New: `src/KBot.EFactura/**` (vbproj + 9 .vb + 2 .Designer.vb), `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura}.vb`, `src/KBot.Domain/EFacturaToken.vb`, `src/KBot.Common/ReauthGate.vb` (moved), `src/KBot.App/KbotForm.EFactura.vb`, `src/KBot.App/HelpContent/contabil/efactura/index.md`, this file. Edited: `KBot.sln`, `src/KBot.App/{KBot.App.vbproj,KbotForm.Designer.vb,KbotForm.Nomenclatoare.vb}`, the three `.vbproj` FileVersions above, help files (`contabil/fereastra.md`, `tours/tur-fereastra.md`, `HelpContent/README.md`). Deleted: `src/KBot.App/Views/Nomenclatoare/ReauthGate.vb` (moved).

### Decisions taken here (state them, change if wrong)
- **The server certificate of ANAF is CHECKED** in the authorise call. `EF.EXE` accepted any server certificate (`ServerCertificateCustomValidationCallback = True`); that was not copied. If a PC does not trust ANAF's server certificate the failure says the connection failed: first thing to look at on the first real try.
- **The address must be `https`; no host check.** The server hands the address over (the operator's `EF_AUTH_URL`).
- **Order: pick the certificate BEFORE asking the server to start** (the state lives 15 minutes; nothing is stored for a certificate nobody chose).
- **ReauthGate moved to KBot.Common** instead of copying it or giving the window one closure per response type.
- **No role check**: any user of the unit may open the window and authorise (same as the server, 00EF-04).
- The notice «expiră în N zile» is shown **in the E-Factura window when it is opened**. A notice on the main window at start-up is NOT built (open thread).
- Menu icon: `binvoice` (the invoice icon already used by «Extrase» and «Parteneri»); no new picture.
- Designer coordinates are written in 96 dpi (`AutoScaleDimensions = 96,96`); Visual Studio rewrites them with the first real save. The forms were never opened in the designer.

### Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura` and `KBot.App` (Debug): 0 errors, 0 warnings. **No test code written and no test run** (standing rule: no tests unless asked). **Never run, never seen on screen** — no hardware certificate here.

### Unverified / deferred
- **Everything on a real certificate**: the store listing on a real token (RSACng vs CAPI providers, the export-policy test), the PIN prompt, the TLS handshake with `logincert.anaf.ro`, whether ANAF's redirect chain ends on the registered redirect address answering 2xx and carrying `code=` (as `EF.EXE` relied on), and the exchange on the server (00EF-04 caveats: form-encoded body, the real life of the refresh token).
- How the two forms look (sizes, wrapping of the long labels, the picker's grid); the theme / DPI behaviour; the designer was never opened.
- A start-up notice on the main window 7 days before the refresh token ends; `KBot.EFactura` has no tests project.
- The picture `efactura-token` (capture tag in the help) does not exist yet.
- The server (00EF-04) must be deployed with `cryptography`, the DDL and `/etc/avacont/efactura.env` before a real try.
