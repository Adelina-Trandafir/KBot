# SLICE-0110-03 / 04 / 13 — Autentificarea portalului

Istoric 05–23.10.2026. 03 introduce parola/codul și sesiunea; „certificatul nu este făcut” este checkpointul dinaintea lui 04. În 04, propunerea și starea nedeployată preced confirmarea operatorului pentru certSIGN; rădăcinile, celelalte autorități/browsere și antetele proxy rămân cu limitele consemnate. 13 corectează doar mesajele NETWORK/CERT_ABSENT. Așezarea autentificării confirmată la 23.10 nu extinde probele de certificat și nu schimbă excluderea mobilă.

- [0110-03 — parolă, cod și sesiune](#slice-0110-03)
- [0110-04 — certificat, instalare și confirmări](#slice-0110-04)
- [0110-13 — mesajul erorii de rețea](#slice-0110-13)

## SLICE-0110-03 - web area of registered users: sign-in, session, unit (operator request, 05.10.2026) <a id="slice-0110-03"></a>

Part of the site plan (`SLICE-0110`). Read-only viewing only: nothing here writes business data. The certificate sign-in (nginx mTLS) is 0110-04 and is NOT done.

### What changed and why
- `GET /portal` (`static/portal.html`, `static/css/portal.css`, `static/js/portal/portal.js`): one page with three states - e-mail + password, the mailed code, signed in (unit picker + the unit's years/sources as a placeholder; the data views are slice 0110-06).
- `PYTHON/routes/portal/portal.py`: `POST /api/portal/login` (password proven by a MariaDB login as the user via `auth.verify_operator`, discarded at once; the user must have at least one unit in `Unitati_Utilizatori`; then a 6-digit code mailed), `POST /api/portal/verifica` (code, 5 tries, 10 min, hash only, `hmac.compare_digest`), `POST /api/portal/logout`, `GET /api/portal/me`, `POST /api/portal/unit` (the unit is re-read from `Unitati_Utilizatori`, never trusted from the client), `GET /api/portal/session/info`, `GET /api/portal/session/check`, `POST /api/portal/session/extend`. Any role may enter (operator decision).
- Session: a note in the same `STORE` under its own name (`portal_session`), own header `X-Portal-Token`, token in `sessionStorage` (no cookie, so no CSRF). Idle window 20 min sliding, absolute cap 8 h, at most 20 extensions. A K-BOT (desktop) bearer token is not a portal token and the reverse. `require_portal_session` is the guard for the data routes of 0110-06: sets `g.portal = {email, db_name, role}`, refuses 409 `UNITATE_NEDESCHISA` before a unit is opened.
- Limits and audit: shared `LIMITER` per (address, e-mail); a right password with no unit counts as a failure; `Jurnal` rows `PORTAL_LOGIN / PORTAL_LOGOUT / PORTAL_UNIT / PORTAL_AUTH_FAIL / PORTAL_DENIED`.
- Mail: `mailer.send_portal_code`.
- The session timer is the existing `JS_COMPONENTS/session` ADAPTED (operator asked), copied to `static/js/session/`: the three server calls now go to `/api/portal/session/*` with the portal header; the page hands the token and the sign-in address through `init({getToken, loginUrl, onExpired})`; the server's extension limit replaces the hard-coded 2; on expiry it clears the token and goes to `/portal` (no `/logout`, no footer); debug log off; fixed a bug of the original where the activity listeners were never removed (`bind()` made a new function each time). New `static/css/monitoring.css` (the original had none in the repository). `event-bus`, `listener-tracker`, `instances-registry` already existed in `static/js`.
- Landing: «Intră în cont» button in the top bar (`links.portal`).

### Files touched
`PYTHON/routes/portal/{__init__,portal}.py`, `PYTHON/static/{portal.html,css/portal.css,css/monitoring.css}`, `PYTHON/static/js/portal/portal.js`, `PYTHON/static/js/session/*` (5 files), `PYTHON/static/site/detalii.css` (password fields), `PYTHON/routes/auth/mailer.py`, `PYTHON/main.py`, `PYTHON/routes/landing/{content.json,templates/landing/index.html}`.

### Checked
Flask test client, DB/mail replaced: wrong password, no unit, wrong code, a code works once, no token, a unit that is not the user's (403), unit switch, guard (409 before a unit, 200 after), info/check/extend, extension limit, absolute cap (401), logout, a Bearer token is refused, 6th wrong password 429. In the browser pane (stub server, 75 s idle window): full sign-in, unit switch, the warning modal at the right moment, an activity click auto-extends and shows the notice, a bogus stored token falls back to the sign-in card, no unexpected console errors. Found and fixed on screen: a hidden section staying visible (`display:flex` over `hidden`), password inputs without the field style.

### Not done / to do
- Real database, real SMTP and a real MariaDB login NOT exercised. `Jurnal` rows written by the service account as elsewhere.
- Expiry redirect (timer reaching zero) and the narrow-phone layout not looked at.
- Server not deployed. No DDL in this slice.
- 0110-04 mTLS: nginx on the VPS has no `ssl_verify_client` today (checked by the operator); needs a CA decision (open question) and `Utilizatori_Certificate`.
- The gunicorn worker is single and the limiter in-process, as for the other pages; a restart ends open portal sessions only if the STORE is memory (production uses Redis).
- No help change (the site is not part of the in-app help).

## SLICE-0110-04 - portal: sign-in with a qualified digital certificate (operator decision, 05.10.2026) <a id="slice-0110-04"></a>

Part of the site plan (`SLICE-0110`). Follows `SLICE-0110-03` (password + mailed code). Read-only viewing, as before.

### Decisions (operator, 05.10.2026) - locked
1. **Authority: qualified certificates** (certSIGN, DigiSign, ... as for ANAF). No CA of our own, nothing to issue.
2. **The certificate is recognised by its SHA-256 fingerprint, bound to the account by enrolment.** The user signs in with password + code, presses «Înrolează», the server remembers the fingerprint. No field of the person (no CNP) is stored.
3. **Fallback: password + code stays** next to the certificate (the certificate is an alternative, never the only way).
4. Expiry: nginx refuses an expired certificate; the page falls back to password + code and invites a new enrolment.
5. Revocation: `Activ = 0` on the row (operator in the database, or the user from the portal). The server does NOT ask the authority's revocation lists (no OCSP/CRL).
6. A renewed certificate is a new row: enrolling it switches the user's older ones off (one active certificate per user).

Assumption to confirm: the e-mail inside the certificate (when the subject has one) is only COMPARED with the account's and the difference written in `Jurnal`; it does not block enrolment, because a certificate often carries a personal address.

### What changed
- `sql/0110_04_utilizatori_certificate.sql`: table `Utilizatori_Certificate` in `AVACONT_COMUN` (UN, Amprenta unique, Subiect, Emitent, ValidPana, Activ, DataInrolare, DataDezactivare, UltimaFolosire). All columns utf8mb3 / utf8mb3_general_ci (house rule). `Unitati_Utilizatori` and `Jurnal` are utf8mb4 in `MariaDB_Schema`; no query joins them to this table (`UN` is compared with a parameter only), so the difference is harmless as the code stands.
- `PYTHON/routes/portal/certificat.py` (registered in `main.py`): `GET /api/portal/certificat/stare`, `POST .../inroleaza` (needs the portal session), `POST .../login` (no session; the portal session without password or code), `POST .../dezactiveaza {id}` (only the user's own). Journal: `PORTAL_CERT_ENROL`, `PORTAL_CERT_OFF`, and `PORTAL_LOGIN` with detail `certificate`; refusals `PORTAL_AUTH_FAIL`/`PORTAL_DENIED`. The failed-attempt limit is per (address, fingerprint). No new library: the fingerprint is the SHA-256 of the PEM's DER bytes (stdlib); subject, issuer and end date come from nginx.
- `PYTHON/routes/portal/portal.py`: the session creation moved into `open_session(email, units, ip, how)` so the code route and the certificate route open the same session; behaviour of the code route unchanged.

### nginx (to be applied by the operator on the VPS - NOT applied)
**CORRECTION (same day):** `ssl_verify_client optional` makes nginx ask for a client certificate on EVERY connection to that host (TLS 1.3 cannot ask per location), so anyone holding a certificate would see the browser's picker on the public pages too. Recommended: put the block below on a host of its own (e.g. `cert.<domain>`, same certificate/proxy to 5009) and keep the main host WITHOUT `ssl_verify_client`; the portal page calls that host (see «Page»). Same host also works for a try-out.

In the `server` block of that host (TLS):

```nginx
ssl_client_certificate /etc/nginx/ca/calificate.pem;   # roots + intermediates of the qualified authorities (see below)
ssl_verify_client      optional;                       # optional: the landing and «Cere detalii» must stay open to everyone
ssl_verify_depth       3;

location /api/portal/certificat/ {
    proxy_pass http://127.0.0.1:5009;
    # SET on every request: a value sent by the client is replaced, an empty value is dropped.
    proxy_set_header X-SSL-Verify  $ssl_client_verify;
    proxy_set_header X-SSL-Cert    $ssl_client_escaped_cert;
    proxy_set_header X-SSL-Subject $ssl_client_s_dn;
    proxy_set_header X-SSL-Issuer  $ssl_client_i_dn;
    proxy_set_header X-SSL-End     $ssl_client_v_end;
    # ...the same proxy_set_header lines (Host, X-Forwarded-For, ...) as the other locations
}
```
Every OTHER location that reaches Flask must blank the five headers too (`proxy_set_header X-SSL-Verify "";` etc.), or a visitor could send them by hand to a route that trusts them. Today only `certificat.py` reads them, and gunicorn listens on 127.0.0.1 only. `calificate.pem`: the root and intermediate certificates of the authorities the users' certificates come from (downloaded from each authority's own site; verify against the authority's published fingerprint before trusting). With `optional`, a browser with no certificate (or an untrusted one) still reaches the host.

### Page (portal.html, portal.js, new certificat.js, portal.css) - COMPUTERS ONLY
- Login card: «Intră cu certificat digital» under a «sau» divider; header (signed in): «Certificat» -> dialog: is this browser's certificate enrolled, «Înrolează certificatul acesta», the list of the user's certificates with «Dezactivează». Any refusal on the sign-in shows the server's sentence + «Poți intra cu parola și codul»; the password form is always there.
- **Phone / tablet: the three elements are REMOVED from the DOM** (not hidden): `isPhoneOrTablet()` = `userAgentData.mobile`, or a mobile UA (Android, iPhone, iPad, iPod, Mobile, Windows Phone), or iPadOS (Mac UA + touch), or touch-only pointer (covers «Request desktop site» on a phone). By the device, never by window width. A tablet counts as a phone (assumption).
- The calls go to `<meta name="portal-cert-origin">` + `/api/portal/certificat/*`. portal.py fills the meta and the CSP `connect-src` from the env var `PORTAL_CERT_ORIGIN` (https only; empty = same host) and answers the cross-origin calls (CORS, one origin, headers Content-Type + X-Portal-Token) for `PORTAL_SITE_ORIGIN`. Both are environment variables (config.py is gitignored); set them on the VPS when the certificate host exists.
- Checked in the browser pane (static preview server, no API): desktop shows the button, the header button and the dialog; mobile emulation (Pixel UA) removes all three and leaves the password form; a refused sign-in shows the fallback sentence. NOT seen on screen (the pane did not draw: screenshots timed out), the dialog and the layout were never looked at; no real certificate, no real nginx.

### Not done / to do
- Nothing was run: only `py_compile` on the three Python files. No Flask test client, no live database, no real certificate.
- DDL not run on the VPS; nginx not changed; server not deployed. The service account's rights on the new table to check.
- The browser asks for the certificate on the first visit to `/api/portal/certificat/*`, so the «Intră cu certificat» button must call it by a normal `fetch` from the page; how Chrome/Edge/Firefox behave with a smart-card token on that call is unverified.
- `$ssl_client_v_end` format (`Oct  5 12:00:00 2027 GMT`) is from the nginx documentation, not seen on the server; an unreadable value is stored as empty and does not break anything.
- No help change (the site is not part of the in-app help).

### Deployed and tried for real (operator, 05.10.2026)
- VPS: `cert.k-bot.ro` (A record to the VPS, Let's Encrypt via certbot webroot, nginx block from `tools/nginx/cert-kbot-ro.conf`), `/etc/nginx/ca/calificate.pem` built by `tools/nginx/build-calificate.sh` from 8 files (certSIGN ROOT CA G2 + Qualified CA, certSIGN ROOT CA SIGN 2023 RSA + Qualified 2023 RSA CA, DigiSign Root + Qualified CA Class 3 2017, Trans Sped Root CA G2 + QCA G2). `Utilizatori_Certificate` created. systemd drop-in `tools/nginx/portal-cert.conf` (the two origins); `avacont` restarted. Main host: `ssl_verify_client` NOT set; the server really asks (checked with `openssl s_client`: the eight authorities listed).
- Real test, on a PC with a physical token: the certificate was enrolled and the sign-in with it (no code) worked.
- Found: the dialog said «Browserul nu a trimis niciun certificat» until the browser was closed and the host opened directly (`https://cert.k-bot.ro/api/portal/certificat/stare`); the operator also had a code-signing certificate from certum.pl connected and thinks it kept Chrome from showing the picker (a guess, not checked). Lesson for the help of whoever sets this up: close the browser, plug the token, then try.
- The proxy of the main host got the five empty `X-SSL-*` lines (step «Etapa 2»): the operator did NOT confirm it; check with `sudo grep -c 'X-SSL-' /etc/nginx/sites-enabled/kbot-ro` (must print 5).

### Still open
- Authenticity of the 8 authority certificates NOT verified against an independent source (only that each intermediate verifies against its root, for DigiSign and Trans Sped; certSIGN not checked even that).
- Not tried: Edge/Firefox, a certificate from DigiSign / Trans Sped (the one that worked was from certSIGN: confirmed by the operator; DigiSign and Trans Sped not tried), an expired one, the «Dezactivează» button, a user switched off, a phone (the removal of the buttons was only seen in emulation).
- Only the main host's `k-bot.ro` address is allowed to call the certificate host (CORS); `www.k-bot.ro` redirects to it.
- Renewal of the certificate of `cert.k-bot.ro` is certbot's timer; nginx is not reloaded by it unless a deploy hook exists (to check before 2027-01-03).

### Așezarea autentificării comune — 21.10.2026 <a id="d001-12-certificat"></a>

Preluare D001-12, **Confirmat 23.10**. În /portal, inclusiv /portal?next=%2Fvercon, Intră cu certificat digital este în stânga lui Continuă, pe același rând, aliniat la marginea stângă a selectorului Aplicația; separatorul sau este eliminat. Textele se pot împacheta la lățimi mici. Formularele de autentificare/cod permit derularea paginii sub 901px pentru acces la toate controalele când înălțimea este mică. Așezarea înlocuiește descrierea inițială «sub separator»; condițiile de autentificare și excluderea mobilă de mai sus nu sunt revizuite de această etapă.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 19 | PYTHON/static/portal.html; PYTHON/static/css/portal-certificate.css, portal-signin.css; implementare_vercon_v2.md. |

## Slice 0110-13 — Mesaj corect pentru eroarea de rețea la intrarea cu certificat <a id="slice-0110-13"></a>

12.10.2026. Cererea operatorului: corectat doar mesajul; pagina fusese deschisă la o adresă greșită. Configurația CORS și comportamentul certificatului în Chrome rămân în afara acestei schimbări.

În `PYTHON/static/js/portal/certificat.js`, `NETWORK` și `CERT_ABSENT` au mesaje separate. Un eșec fetch (inclusiv CORS) nu dovedește absența certificatului și nu permite identificarea sigură a cauzei din interfață.

- `NETWORK`: „Nu s-a putut comunica cu serviciul de autentificare. Verifică adresa paginii și conexiunea, apoi reîncearcă. Poți intra cu parola și codul.”
- `CERT_ABSENT`: „Serverul nu a primit niciun certificat. Verifică dacă tokenul este conectat și certificatul a fost selectat în browser. Poți intra cu parola și codul.”

Publicare: doar fișierul JavaScript, apoi reîncărcarea paginii cu Ctrl+F5. Fără DDL sau restart al backendului. Ajutorul aplicației desktop nu este afectat; mesajele portalului sunt documentate aici.

Verificare: `node --check` și `git diff --check`. Fără teste funcționale sau vizuale; verificarea în browser și publicarea rămân la operator.
