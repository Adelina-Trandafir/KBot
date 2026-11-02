# SLICE-0000-52 - help: the online access (portal), operator request 06.10.2026

In-app help for browser access (0110-03/04/06/08): the operator's Romanian text, inserted as given, with the PDF note they requested.

## What changed
- New topic `contabil.online` («Accesul online la date», `contabil/online.md`, order 95): read-only data; sign-in (e-mail, password, mailed code); certificate (enrolment, one active, computers only); protections (2FA, own units only, session 20 min / 8 h, password not kept, audit trail, limited attempts, GDPR note); «Ieși». Online PDFs are flattened copies of originals, shown ONLY if first signed in the local app.
- `contabil/index.md` («Despre K-BOT»): one paragraph «Și din browser» with a link to the new topic.
- `help-version.txt` = 2026-10-06.
- Site (`PYTHON/routes/landing/content.json`, «Ce urmează»): new node «Online» (editing from the browser; FOREXE through the online system shown as a direction under evaluation, not a promise) and two blocks added to «Direcția».

## Not done
- No screenshots (no capture tags in the topic). Site not previewed. Server not deployed.
