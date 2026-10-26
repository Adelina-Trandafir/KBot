# SLICE-0110-01…02 — Pagina publică și cererile de detalii

Istoric din 05.10.2026. 01 conține și corecția barei după 02…04, cu probele la lățimile consemnate; aceasta precede listarea de mai jos și nu confirmă formularul pe telefon. 02 păstrează ordinea salvare → mail, condițiile anti-bot, probele cu înlocuitori și limitele bazei/SMTP reale. Niciun script SQL sau serviciu nu este executat prin consolidare.

- [0110-01 — tutoriale și corecția barei](#slice-0110-01)
- [0110-02 — cereri de detalii](#slice-0110-02)

## SLICE-0110-01 - landing: «K-BOT te invata singur» (operator request, 05.10.2026) <a id="slice-0110-01"></a>

### What changed and why
The public page had no word about the interactive tutorials (slice 000T). New section `tutorials` in `PYTHON/routes/landing/content.json` (kicker, title, image, blocks, three steps), rendered by a new block in `templates/landing/index.html` after «Pentru toti utilizatorii» (same markup as the «multi» section), a nav entry and a side dot (`tutoriale`). Image: existing `fereastra-principala.png`. Text says what is true today (the tutorial waits for the user, dims the rest, is opened from «?», list keeps growing); no promise about specific flows.

### Files touched
`PYTHON/routes/landing/content.json`, `PYTHON/routes/landing/templates/landing/index.html`.

### Checks / not done
content.json parses. Page not rendered, not seen. Server not deployed. No help change (the site is not part of the in-app help). A dedicated tutorial screenshot would suit better than the main window; not made.

### Corrective, 05.10.2026 - the top bar after 0110-02..0110-04 (operator: «arata rau», the pills grew tall, must work on a phone)
Cause: nine menu links («Tutoriale» and «Cere detalii» were new) plus two buttons in a 1240 px container, with nothing stopping the text from wrapping, so the pills grew vertically. Fix, in `static/site/site.css` (appended, so it wins) and the two templates:
- nothing in the bar wraps any more (`white-space: nowrap`), container up to 1560 px;
- «Cere detalii» leaves the section menu (it stays in the closing call to action);
- what does not fit is dropped in steps: the tagline under 1480 px, the section menu under 1320 px (the side dots and the page itself still lead everywhere), and under 520 px the bar keeps the logo, «Intră în cont» and a short «Înregistrează» (the « unitatea» part is `.btn__more`, hidden on a phone). «Intră în cont» was hidden under 520 px by an older rule; it is visible now.
- Measured in the browser pane at 1950, 1440, 1366, 1330, 1100, 375 and 320 px: no element taller than one line, nothing outside the bar, no horizontal scroll, menu and buttons do not overlap (28 px free at 1330). Looked at on screen at 375 px.
- Not looked at: tablet widths between 520 and 900 px by eye (measured at 1100 only), and the form page's bar on a phone by eye.

## SLICE-0110-02 - public site: «Cere mai multe detalii» (operator request, 05.10.2026) <a id="slice-0110-02"></a>

Part of the site plan (`SLICE-0110`, state file `KBOT_STATUS_0110-0119.md`).

### What changed and why
A visitor of the presentation page can now ask for more details without registering a unit.
- `GET /detalii`: new page, same look as the presentation page (`site.css`, topbar), plus `static/site/detalii.css`. Fields: name, institution, e-mail (required), phone, fiscal code, message (optional), data-processing consent (required). Server-rendered template `routes/detalii/templates/detalii/index.html` so the page can carry the signed timestamp; `static/js/detalii/detalii.js` is plain script (CSP `script-src 'self'`).
- `POST /api/detalii` (`routes/detalii/detalii.py`): JSON in, JSON out. The request is written to `AVACONT_COMUN.FX_CereriDetalii` FIRST (`sql/0110_02_fx_cereri_detalii.sql`), mailed second, so a mail that fails never loses the request (flag `Notificat`). Answer is success even when the mail fails.
- Mail: `mailer.send_details_notice` to `DETALII_EMAIL` (default `info@avatarsoft.ro`, also in `config.py`), visitor's address only as `Reply-To`; nothing is mailed to the visitor.
- Bot protection, no CAPTCHA: hidden `website` field (filled = success answered, nothing stored), HMAC-signed timestamp issued with the page (refused under 3 s or over 2 h; key is per process), the shared `LIMITER` (5 attempts / 15 min per address, every attempt counted), lengths capped, control and non-BMP characters dropped (utf8mb3).
- Landing: `links.contact = /detalii`, new `links.contact_label = «Cere detalii»`; the two hard-coded «Contact» in the template read the label, so the top bar and the closing call to action now lead to the form.
- Decision: the phone/national-id JS components were NOT used - a light server+client check is enough for an optional phone and an optional fiscal code, and the components would add two more files to a public page.

### Files touched
`sql/0110_02_fx_cereri_detalii.sql` (new), `PYTHON/routes/detalii/{__init__,detalii}.py` and `templates/detalii/index.html` (new), `PYTHON/static/site/detalii.css`, `PYTHON/static/js/detalii/detalii.js` (new), `PYTHON/routes/auth/mailer.py`, `PYTHON/main.py`, `PYTHON/config.py` (gitignored, local), `PYTHON/routes/landing/content.json`, `PYTHON/routes/landing/templates/landing/index.html`.

### Checked
Flask test client with the DB and SMTP replaced: page 200 with token; too fast, bad e-mail / CF / phone / name / consent / token each refused with its reason; honeypot stores nothing; a failing mail still answers success; 6th attempt 429; mail text and Reply-To built correctly; landing links to `/detalii`. In the browser pane (stub server): form renders, empty submit shows the first problem, a valid submit shows the thank-you card, no console errors.

### Not done / to do on deploy
- **DDL first** on the VPS: `sql/0110_02_fx_cereri_detalii.sql`; then check the service account can INSERT/UPDATE the table.
- Real SMTP and real database NOT exercised; `config.py` on the VPS needs nothing (default in the mailer), but `SMTP_*` must be set or requests are saved without a mail (warning in the log).
- Phone layout (narrow width) not looked at on screen; only the CSS rules exist.
- Retention of old requests (open question): none implemented, rows stay.
- No help change: the site is not part of the in-app help.
