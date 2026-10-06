# SLICE-0000-52 - help: the online access (portal), operator request 06.10.2026

The in-app help now describes the web area where users view their data from a browser (slices 0110-03/04/06/08).
Text written by the operator (Romanian), put in as given; the PDF note was added at their request.

## What changed
- New topic `contabil.online` («Accesul online la date», `contabil/online.md`, order 95): what can be seen (read only),
  how to sign in (e-mail + password + mailed code), the digital certificate (enrolment, one active, computers only),
  the protections (2FA, own units only, session 20 min / 8 h, password not kept, audit trail, limited attempts, GDPR note),
  and «Ieși». PDFs online are a flattened copy of the originals and are shown ONLY if they were first signed in the local app.
- `contabil/index.md` («Despre K-BOT»): one paragraph «Și din browser» with a link to the new topic.
- `help-version.txt` = 2026-10-06.
- Site (`PYTHON/routes/landing/content.json`, «Ce urmează»): new node «Online» (editing from the browser; FOREXE through the online
  system shown as a direction under evaluation, not a promise) and two blocks added to «Direcția».

## Not done
- No screenshots (no capture tags in the topic). Site not previewed. Server not deployed.
