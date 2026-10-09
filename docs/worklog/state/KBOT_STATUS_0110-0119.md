# K-BOT — STATUS, slices 0110–0119

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0110 — public site (sub-slices 0110-01 .. 0110-12) (landing, request form, read-only user area)

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
| 0110-09 | Portal: pagina «Administrare» (doar conturile din `PORTAL_ADMIN_EMAILS`, implicit scavatarsoft@gmail.com; altii primesc 404): file «Baze de date» (marime, tabele, activitate, erori din jurnale, schema), «Jurnale» (toate fisierele serverului, filtre pe fisier si pe baza + «fara baza de date», DGV) si «Vizitatori» (numaratoare pe pagina de prezentare: IP, steag din fisier DB-IP offline, mobil, secunde, sectiuni citite; tabel `Vizite_Site`, `POST /api/vizita`) | GATA pe cod, probat in browser cu date inventate (SitePreview); **nevazut pe ecran, SQL-ul nerulat pe MariaDB, GDPR (IP pastrat 180 zile) de hotarat; DDL + `pip install maxminddb` + fisierul .mmdb + server nedeployate** | `SLICE-0110-09-portal-administrare.md` |
| 0110-10 | Portal: pe calculator fiecare card al angajamentului are arborele lui (Istoric, Rezervari, Receptii, Extrase, Plati, Fundamentari, Ordonantari: «Toate...» > luna > ziua / documentul, cu totaluri, ca in K-BOT; Sumar fara arbore, ca in K-BOT); grupările pe data scoase din toate cardurile in ambele moduri (Istoric neschimbat); cardul «Documente» inlocuit de «Fundamentari» si «Ordonantari» (liniile documentului + PDF-ul semnat; notele CAB in dosarul lor din Ordonantari) | GATA pe cod, probat in browser cu date inventate (calculator + telefon); **nevazut pe ecran, baza reala neincercata, server nedeployat** | `SLICE-0110-10-portal-carduri-arbori.md` |
| 0110-11 | Portal, Administrare: cardul «Coloane» (coloane vizibile, pozitie, latime, o coloana care se extinde, pentru TOATE grilele sitului; se pastreaza in browser, se aplica pe grilele reale pentru administrator; fisier JSON de dat dezvoltatorului, care il pune in `grid-layouts.defaults.js`) | GATA pe cod, probat in browser cu date inventate; **nevazut pe ecran; Clasificatii/Parteneri neprobate in preview; asteapta fisierul cu alegerile administratorului**; server nedeployat | `SLICE-0110-11-portal-coloane-grile.md` |
| 0110-12 | Portal, DOAR calculator: detalii sub grila (Istoric: descriere + valori; Plati: extrasul bancar), meniul de vizualizare din antetul arborelui Extrase (antet + operatii / operatii + detalii), Fundamentari si Ordonantari cu navbar Vizualizare | Document, informatiile din header (DDF: antet; ORD: filtru beneficiar + subsol); reparat arborele fara casuta de cautare | GATA pe cod, probat in browser cu date inventate; **nevazut pe ecran, baza reala neincercata, server nedeployat** | `SLICE-0110-12-portal-carduri-detalii.md` |

---

## Slice 0111 — value correction of a reception snapshot

Operator, 06.10.2026. FOREXE's own history sometimes writes a wrong total on a reception header
(the 12.06.2026 reception of `AAB3MEF2MG2`: header row «valoare: 0», line row «Suma receptie: 1635
RON»). K-BOT cannot trust that total 100%, so the operator may correct it from the association
window. Reasoning and rules: `docs/FUNDAMENT_Asociere_Receptii.md` §1.8 and Part 6 (F35).

Locked decisions (operator, 06.10.2026):
- the corrected figure goes in the WORKING columns (`FX_Receptii_H.Total`, `FX_Receptii.Valoare`),
  which every reader already uses; the existing `TotalOrig` / `ValoareOrig` keep what FOREXE said (no
  new `ValoareReala` column);
- `FX_Istoric` is never edited; a correction never causes a new download;
- on save, a total that is not the sum of the lines is a BLOCKING error;
- who / when / why: three columns on the header (`CorectatDe`, `CorectatLa`, `CorectatMotiv`);
- the DDL runs on `AVACONT_SURSA` only; the other databases are brought up by AvacontPush (schema
  sync, then the one-time query).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0111 | Value correction of a reception snapshot (menu «Corectează valoarea…», `CorectieValoareForm`, `POST /api/forexe/asociere/corectie`, `TotalOrig` at birth, DDL + one-time query) | GATA pe cod: `KBot.App` builds with 0 warnings / 0 errors, the Python files compile; **nothing ran against a live MariaDB, the form was never seen on screen; DDL + one-time query + server not deployed** | `SLICE-0111-01-corectie-valoare-instantaneu.md` |

### Current focus — 0111

Done in code (06.10.2026): `sql/0111_01_sursa.sql`, `sql/0111_02_interogare_unica.sql`;
`PYTHON/routes/forexe/prelucrare_pasi.py` (`TotalOrig` at birth), `PYTHON/routes/forexe/asociere.py`
(read of the new fields + the correction route); domain / API / form on the client; help 0000-53.

Deployment order (operator runs it): DDL on `AVACONT_SURSA` › AvacontPush schema sync (SAFE) › push the
Python files › AvacontPush one-time query `0111_total_orig_si_valoare_orig` › the K-BOT client.

### Open threads — 0111

- **To decide (operator):** does the ordonantare freeze also apply to a VALUE correction? Implemented as
  YES (same rule as for the link: an ordonantare read the total, §1.3); lifting it is the `blocks`
  check in `plan_correction`.
- **Second pass (same day): the download window.** The correction is also offered there, on the
  snapshots the download brought; it travels with the decision and the server applies it in phase two
  before the placements are checked. NOT run live; the phase-two path was never exercised (the first
  real download with a correction is its test). An older snapshot is not correctable from there.
- **Not run live:** the route, the one-time query, the `TotalOrig` ← `Total` value list in the header
  insert (MariaDB documents that a value list may read a column set earlier in it), the form on screen.
- Capture `asocieri-corectie-valoare` (help) is not taken.
- `tests/KBot.App.Tests` does not build for reasons that are NOT this slice (`ForexeAnswerStoreTests`
  indexes a `JobRequest`; `MainForm*Tests` miss the `capturiApi` argument). The 9 test fakes of
  `IApiClient` got the new member (`CorecteazaValoareaAsync`) so that this slice adds no error.
- The rebuild from history (`receptii_refacere`) of a header that was deleted is born again with the
  history's figure and without a correction.

---

## Slice 0112 — K-BOT message box + debug message catalog

Operator, 08.10.2026. A message window of our own (`KBot.ControlsMessageBox`: `KBotMessageBox`, `KBotMessageBoxForm`, `KBotMessageIcon`, `KBotMessageSpec`; kinds None/Info/Warning/Error/Question; optional extra button) replaces the native `MessageBox` behind every `KBotMessage.Show` through `KBotMessage.Presenter`, installed in `Program.Main` — no call site changed. Debug-only bench in DevHarness (grid of all 474 calls: type, buttons, extra button, caption, text, function; editor; preview) over `Config/mesaje_catalog.json`, produced by `tools/MessageCatalog/scan.js`.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0112 | K-BOT message box + debug message catalog (0112-02: catalog edits apply at run time, «Actualizează», MENIU › ADMIN) | GATA pe cod: Controls, DevHarness, App build 0 warnings / 0 errors; nimic rulat, **nevăzut pe ecran**; catalogul nu alimentează încă apelurile | [SLICE-0112](../SLICE-0112-caseta-mesaj-kbot-si-catalog.md) |

### Open threads — 0112

- First look on screen (Classic / Dark / Modern, 125% / 150%, long text, extra button, TopMost over a tutorial card).
- 0112-02 done in code: calls take their wording from the catalog (matched by file + member, confirmed by the code's template). Never run: first real edit on screen will prove the matcher.
- A changed button set is applied as written; code that tests the old set will misread the answer (the editor warns).
- Help: screenshots with a native message box are stale (listed in 0000-0009 Open threads).
