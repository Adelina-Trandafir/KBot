# SLICE-0110-03 - web area of registered users: sign-in, session, unit (operator request, 05.10.2026)

Part of the site plan (`SLICE-0110`). Read-only viewing only: nothing here writes business data. The certificate
sign-in (nginx mTLS) is 0110-04 and is NOT done.

## What changed and why
- `GET /portal` (`static/portal.html`, `static/css/portal.css`, `static/js/portal/portal.js`): one page with three states -
  e-mail + password, the mailed code, signed in (unit picker + the unit's years/sources as a placeholder; the data views
  are slice 0110-06).
- `PYTHON/routes/portal/portal.py`:
  `POST /api/portal/login` (password proven by a MariaDB login as the user via `auth.verify_operator`, discarded at once;
  the user must have at least one unit in `Unitati_Utilizatori`; then a 6-digit code mailed), `POST /api/portal/verifica`
  (code, 5 tries, 10 min, hash only, `hmac.compare_digest`), `POST /api/portal/logout`, `GET /api/portal/me`,
  `POST /api/portal/unit` (the unit is re-read from `Unitati_Utilizatori`, never trusted from the client),
  `GET /api/portal/session/info`, `GET /api/portal/session/check`, `POST /api/portal/session/extend`.
  Any role may enter (operator decision).
- Session: a note in the same `STORE` under its own name (`portal_session`), own header `X-Portal-Token`, token in
  `sessionStorage` (no cookie, so no CSRF). Idle window 20 min sliding, absolute cap 8 h, at most 20 extensions. A K-BOT
  (desktop) bearer token is not a portal token and the reverse. `require_portal_session` is the guard for the data routes
  of 0110-06: sets `g.portal = {email, db_name, role}`, refuses 409 `UNITATE_NEDESCHISA` before a unit is opened.
- Limits and audit: shared `LIMITER` per (address, e-mail); a right password with no unit counts as a failure; `Jurnal`
  rows `PORTAL_LOGIN / PORTAL_LOGOUT / PORTAL_UNIT / PORTAL_AUTH_FAIL / PORTAL_DENIED`.
- Mail: `mailer.send_portal_code`.
- The session timer is the existing `JS_COMPONENTS/session` ADAPTED (operator asked), copied to `static/js/session/`: the
  three server calls now go to `/api/portal/session/*` with the portal header; the page hands the token and the sign-in
  address through `init({getToken, loginUrl, onExpired})`; the server's extension limit replaces the hard-coded 2; on expiry
  it clears the token and goes to `/portal` (no `/logout`, no footer); debug log off; fixed a bug of the original where the
  activity listeners were never removed (`bind()` made a new function each time). New `static/css/monitoring.css` (the
  original had none in the repository). `event-bus`, `listener-tracker`, `instances-registry` already existed in `static/js`.
- Landing: «Intră în cont» button in the top bar (`links.portal`).

## Files touched
`PYTHON/routes/portal/{__init__,portal}.py`, `PYTHON/static/{portal.html,css/portal.css,css/monitoring.css}`,
`PYTHON/static/js/portal/portal.js`, `PYTHON/static/js/session/*` (5 files), `PYTHON/static/site/detalii.css` (password
fields), `PYTHON/routes/auth/mailer.py`, `PYTHON/main.py`, `PYTHON/routes/landing/{content.json,templates/landing/index.html}`.

## Checked
Flask test client, DB/mail replaced: wrong password, no unit, wrong code, a code works once, no token, a unit that is not the
user's (403), unit switch, guard (409 before a unit, 200 after), info/check/extend, extension limit, absolute cap (401),
logout, a Bearer token is refused, 6th wrong password 429. In the browser pane (stub server, 75 s idle window): full sign-in,
unit switch, the warning modal at the right moment, an activity click auto-extends and shows the notice, a bogus stored token
falls back to the sign-in card, no unexpected console errors. Found and fixed on screen: a hidden section staying visible
(`display:flex` over `hidden`), password inputs without the field style.

## Not done / to do
- Real database, real SMTP and a real MariaDB login NOT exercised. `Jurnal` rows written by the service account as elsewhere.
- Expiry redirect (timer reaching zero) and the narrow-phone layout not looked at.
- Server not deployed. No DDL in this slice.
- 0110-04 mTLS: nginx on the VPS has no `ssl_verify_client` today (checked by the operator); needs a CA decision (open
  question) and `Utilizatori_Certificate`.
- The gunicorn worker is single and the limiter in-process, as for the other pages; a restart ends open portal sessions only if
  the STORE is memory (production uses Redis).
- No help change (the site is not part of the in-app help).
