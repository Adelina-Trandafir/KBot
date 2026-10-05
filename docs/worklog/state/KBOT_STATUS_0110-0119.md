# K-BOT — STATUS, slices 0110–0119

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0110 — public site (sub-slices 0110-01 .. 0110-08) (landing, request form, read-only user area)

Plan: `C:UsersAdelina Trandafir.claudeplanspasted-content-id-62ae-directii-sait-robust-mccarthy.md`.
Locked decisions (operator, 05.10.2026): read-only viewing of MariaDB data, no editing, no PDF; no FOREXE;
login = email + password + e-mail code, or client certificate (nginx mTLS, no code; qualified certificates, fingerprint
bound by enrolment, password + code kept as the alternative — 0110-04); requests go to
info@avatarsoft.ro; all roles may enter; the JS session monitor in `JS_COMPONENTS/session` is adapted, not dropped.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0110-01 | Landing: «K-BOT te învață singur» | GATA pe cod, nevăzut, server nedeployat | `SLICE-0110-01-landing-tutoriale.md` |
| 0110-02 | Pagina «Cere mai multe detalii» (`/detalii`, `POST /api/detalii`, tabel `FX_CereriDetalii`, mail la info@avatarsoft.ro) | GATA pe cod, probat cu stub, **DDL + server nedeployate** | `SLICE-0110-02-cere-detalii.md` |
| 0110-03 | Portal: autentificare web (parola + cod pe e-mail), sesiune, alegerea unității, monitorul de sesiune adaptat | GATA pe cod, probat cu server de probă, **server nedeployat** | `SLICE-0110-03-portal-autentificare.md` |
| 0110-04 | Portal: certificat digital calificat (nginx mTLS, fără cod; amprenta SHA-256 legată prin înrolare; parola + cod rămâne alternativă) | decizii luate 05.10.2026; **backend GATA pe cod** (`routes/portal/certificat.py`, `sql/0110_04_utilizatori_certificate.sql`), doar `py_compile`, nimic rulat; pagina cu butoane (doar pe calculator, scoasă pe telefon); **DEPLOYAT pe VPS 05.10.2026 (host separat `cert.k-bot.ro`) și probat real: înrolare + intrare cu un certificat de pe token (certSIGN; DigiSign și Trans Sped neîncercate)**; deschise: amprentele rădăcinilor, Edge/Firefox, expirare, telefon real | `SLICE-0110-04-portal-certificat.md` |
| 0110-05 | Control JS DGV doar-citire (`static/js/dgv/`: motor pur + `DataGrid`, filtrare, grupare, agregate, 3 teme) | GATA pe cod, motorul verificat, comportamentul probat în browser, **aspectul nevăzut pe ecran** | `SLICE-0110-05-control-js-dgv.md` |
| 0110-06 | Pagini de vizualizare (Sumar, Rezervări, Recepții brute, Plăți) cu lista de angajamente; rutele `/api/portal/date/*` refolosesc funcțiile din `routes/forexe` | GATA pe cod, probat în browser cu date fictive, **baza reală neîncercată, server nedeployat**; Recepții = lista brută | `SLICE-0110-06-portal-vizualizare.md` |
| 0110-07 | Cercetare: PDF lifecycle (XFA) în browser | GATA (doar verdict, fără cod): fezabil cu pdf.js `enableXfa` (~2 MB vendored); semnate în Adobe se văd și într-un vizualizator simplu; limite în worklog | `SLICE-0110-07-pdf-lifecycle-in-browser.md` |
| 0110-08 | Portal: fila «Documente» (DDF, ORD, note) cu vizualizator PDF doar-citire pe pdf.js vendored (urmarea cercetării 0115) | GATA pe cod, probat în browser cu PDF-urile-exemplu reale, **DDF semnat din fluxul K-BOT neavut la dispoziție, server nedeployat** | `SLICE-0110-08-portal-documente-pdf.md` |
