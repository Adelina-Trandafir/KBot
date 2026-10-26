# K-BOT — STATUS, slices 0110–0119

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0110 — public site (sub-slices 0110-01 .. 0110-13) (landing, request form, read-only user area)

Plan: `C:UsersAdelina Trandafir.claudeplanspasted-content-id-62ae-directii-sait-robust-mccarthy.md`.
Locked decisions at the initial checkpoint (operator, 05.10.2026): read-only viewing of MariaDB data, no editing, no PDF; no FOREXE;
PDF viewing is added later in [0110-08](../SLICE-0110-06-portal-vizualizare.md#slice-0110-08), without signature verification.
login = email + password + e-mail code, or client certificate (nginx mTLS, no code; qualified certificates, fingerprint
bound by enrolment, password + code kept as the alternative — 0110-04); requests go to
info@avatarsoft.ro; all roles may enter; the JS session monitor in `JS_COMPONENTS/session` is adapted, not dropped.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0110-01 | Tutoriale și corecția barei publice — checkpointuri | GATA pe cod, nevăzut, server nedeployat | [0110-01](../SLICE-0110-01-landing-tutoriale.md#slice-0110-01) |
| 0110-02 | Cere detalii — formular, salvare și notificare | GATA pe cod, probat cu stub, **DDL + server nedeployate** | [0110-02](../SLICE-0110-01-landing-tutoriale.md#slice-0110-02) |
| 0110-03 | Portal — parolă/cod, sesiune și unitate | GATA pe cod, probat cu server de probă, **server nedeployat** | [0110-03](../SLICE-0110-04-portal-certificat.md#slice-0110-03) |
| 0110-04 | Portal — certificat calificat și probele operatorului | decizii luate 05.10.2026; **backend GATA pe cod** (`routes/portal/certificat.py`, `sql/0110_04_utilizatori_certificate.sql`), doar `py_compile`, nimic rulat; pagina cu butoane (doar pe calculator, scoasă pe telefon); **DEPLOYAT pe VPS 05.10.2026 (host separat `cert.k-bot.ro`) și probat real: înrolare + intrare cu un certificat de pe token (certSIGN; DigiSign și Trans Sped neîncercate)**; deschise: amprentele rădăcinilor, Edge/Firefox, expirare, telefon real | [0110-04](../SLICE-0110-04-portal-certificat.md#slice-0110-04) |
| 0110-05 | Grila JS — bază și extensii ulterioare | GATA pe cod, motorul verificat, comportamentul probat în browser, **aspectul nevăzut pe ecran** | [0110-05](../SLICE-0110-05-control-js-dgv.md) |
| 0110-06 | Vizualizare — primele citiri și carduri | GATA pe cod, probat în browser cu date fictive, **baza reală neîncercată, server nedeployat**; Recepții = lista brută | [0110-06](../SLICE-0110-06-portal-vizualizare.md#slice-0110-06) |
| 0110-07 | Cercetare PDF/XFA — verdict și limite | GATA (doar verdict, fără cod): fezabil cu pdf.js `enableXfa` (~2 MB vendored); semnate în Adobe se văd și într-un vizualizator simplu; limite în worklog | [0110-07](../SLICE-0110-07-pdf-lifecycle-in-browser.md) |
| 0110-08 | Documente semnate și vizualizator PDF | GATA pe cod, probat în browser cu PDF-urile-exemplu reale, **DDF semnat din fluxul K-BOT neavut la dispoziție, server nedeployat** | [0110-08](../SLICE-0110-06-portal-vizualizare.md#slice-0110-08) |
| 0110-09 | Administrare — baze, jurnale și vizitatori | GATA pe cod, probat in browser cu date inventate (SitePreview); **nevazut pe ecran, SQL-ul nerulat pe MariaDB, GDPR (IP pastrat 180 zile) de hotarat; DDL + `pip install maxminddb` + fisierul .mmdb + server nedeployate** | [0110-09](../SLICE-0110-09-portal-administrare.md#slice-0110-09) |
| 0110-10 | Arbori pe carduri și separarea documentelor | GATA pe cod, probat in browser cu date inventate (calculator + telefon); **nevazut pe ecran, baza reala neincercata, server nedeployat** | [0110-10](../SLICE-0110-06-portal-vizualizare.md#slice-0110-10) |
| 0110-11 | Administrare — coloane și exporturi succesive | GATA pe cod, probat in browser cu date inventate; **nevazut pe ecran; Clasificatii/Parteneri neprobate in preview; asteapta fisierul cu alegerile administratorului** (checkpoint inițial; follow-up-ul consemnează trei exporturi, ultimul fără Administrare); server nedeployat | [0110-11](../SLICE-0110-09-portal-administrare.md#slice-0110-11) |
| 0110-12 | Detalii, moduri Extrase și pagini Vizualizare/Document | GATA pe cod, probat in browser cu date inventate; **nevazut pe ecran, baza reala neincercata, server nedeployat** | [0110-12](../SLICE-0110-06-portal-vizualizare.md#slice-0110-12) |

---

## Slice 0110-13 — certificate login network message

12.10.2026: GATA local, numai sintaxă verificată. [Mesajele NETWORK/CERT_ABSENT, publicarea și limitele](../SLICE-0110-04-portal-certificat.md#slice-0110-13); CORS și comportamentul Chrome rămân în afara schimbării.

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
| 0112 | K-BOT message box + debug message catalog (0112-02: catalog edits apply at run time, «Actualizează», MENIU › ADMIN; 0112-04: buton «Trimite eroarea» în bara casetei de eroare → `FX_RaportErori`) | GATA pe cod: Controls, DevHarness, App build 0 warnings / 0 errors; nimic rulat, **nevăzut pe ecran**; catalogul nu alimentează încă apelurile | [SLICE-0112](../SLICE-0112-caseta-mesaj-kbot-si-catalog.md) |

### Open threads — 0112

- First look on screen (Classic / Dark / Modern, 125% / 150%, long text, extra button, TopMost over a tutorial card).
- 0112-02 done in code: calls take their wording from the catalog (matched by file + member, confirmed by the code's template). Never run: first real edit on screen will prove the matcher.
- A changed button set is applied as written; code that tests the old set will misread the answer (the editor warns).
- 0112-04: apply `sql/0112_04_fx_raport_erori.sql` on the server and deploy `PYTHON/routes/error_report.py` + `main.py` BEFORE publishing the client; never run, button/tooltip unseen; body-size limit of the proxy unchecked.
- Help: screenshots with a native message box are stale (listed in 0000-0009 Open threads).

---

## Slices 0113–0120 — the new main window (AvacontMain), recorded 13.10.2026

Built and tried on screen in the copy `KBOT_TEST`, brought into `KBOT\src` on 13.10.2026 (~17:15); round-by-round
history in `KBOT_TEST\MERGE_NOTES.md`. Recorded after the merge as work already done. 0120 lives in
[KBOT_STATUS_0120-0129.md](KBOT_STATUS_0120-0129.md).

**Locked decisions (operator, 13.10.2026):**
- every macheta opens as a **page** of the main window, one at a time (was MDI / modal); small dialogs, monitoring
  windows, login and the director window stay separate windows; machete lose their own close buttons (ribbon
  «Închide», Esc);
- the history of machete («Machete deschise») is kept **only for the session** (`MemoryMachetaHistoryStore`);
- recent actions are rows of the server table `Jurnal`, written for a **white list of routes** (`_LABELS`), no schema change;
- the orange accents of `KBOT_TEST` were **for the test only**; they were not brought into `KBOT`.

## Slice 0113 — main window AvacontMain + machete as pages

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0113 | Fereastra gazdă și paginile — inițial | GATA pe cod (build of the merge 0/0; 99 failing tests = the same as before), seen on screen only in `KBOT_TEST`; help 0000-69 | [0113](../SLICE-0113-fereastra-principala-machete-pagini.md#slice-0113) |
| 0113-01 | Pagini păstrate deschise, modale și stivă | GATA pe cod (build `KBot.App` 0/0); **nimic rulat, nevăzut pe ecran**; ajutor 0000-86 | [0113-01](../SLICE-0113-fereastra-principala-machete-pagini.md#slice-0113-01) |

### Open threads — 0113-01

- **Nevăzut pe ecran** (comutarea între pagini, titlul, butoanele ribbonului, mărimea / centrarea ferestrelor modale, Adobe care chiar se închide).
- **Schimbarea anului / unității cu pagini deschise**: paginile ascunse nu se reîncarcă și nu se închid (ex. Clasificațiile păstrează anul vechi). De decis.
- Presupuneri de confirmat: `GrupeForm` modal; Setări și Ajutor rămân pagini; la închiderea unei pagini revine cea folosită înainte (nu baza).
- Panoul «Machete deschise» e tot istoricul (nu «deschise acum»); un rând de modală din istoric redeschide vederea angajamentului, nu fereastra.
- FileVersion al `KBot.Common`, `KBot.EFactura`, `KBot.App` nebumpat (ca la 0113, de făcut separat).

### Open threads — 0113

- **The initial help tour no longer starts**: `PornesteTurulInitial` checks `Visible`, the shell is hidden at start
  (Acasă on screen), and the tour is anchored on `btnMeniu` (hidden). To be fixed separately (operator, 13.10.2026).
- **FileVersion** of `KBot.Api`, `KBot.Common`, `KBot.Controls`, `KBot.Theming`, `KBot.EFactura` (and `KBot.App`)
  not bumped for 0113–0120. To be done separately (operator, 13.10.2026).
- `menuNou` / `btnMeniu` still in `KbotForm`, hidden; the ribbon mirrors and runs them. Removed only when asked.
- Help navigation (`goto:` of captures / tours / `open:`) after the move: see the 0000-69 worklog («Navigarea
  ajutorului») — `view:` and `menu:` do not bring the list of angajamente to the front, `setari:jurnal` is gone, no
  `goto:` for the opening page, the ribbon tabs or «Istoric versiuni». Code change proposed, waiting for the operator.

## Slice 0114 — KBotRibbon + pinned «Ajutor» / «Închide»

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0114 | Ribbonul ferestrei principale (KBotRibbon) si butoanele fixate «Ajutor» / «Inchide» (13.10.2026) | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST` (orange); help 0000-70 | [0114](../SLICE-0114-ribbon.md#slice-0114) |
| 0114-01 | Ribbon: fields (combo / text box) in a group, conditional buttons, order (15.10.2026) | GATA pe cod (build curat KBot.App); **nimic rulat, nevăzut pe ecran** | [0114-01](../SLICE-0114-ribbon.md#slice-0114-01) |
| 0114-02 | Ribbon: separator before a pinned button, page buttons, «Ajutor» removed (16.10.2026) | GATA pe cod (build curat, nerulat, nevăzut) | [0114-02](../SLICE-0114-ribbon.md#slice-0114-02) |
| 0114-03 | Ribbon: the right group (16.10.2026) | GATA pe cod (build curat, nerulat, nevăzut) | [0114-03](../SLICE-0114-ribbon.md#slice-0114-03) |
| 0114-04 | Ribbon: tab order, "Date" folded into Configurari, new Extrase tab (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-04](../SLICE-0114-ribbon.md#slice-0114-04) |
| 0114-05 | «Descarca extrase»: only the ribbon button, tied to the Extrase window / the shell (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-05](../SLICE-0114-ribbon.md#slice-0114-05) |
| 0114-06 | Ribbon: a group for the FORM, next to the view group | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-06](../SLICE-0114-ribbon.md#slice-0114-06) |
| 0114-07 | Form group tied to the whole form (KbotMain), not to a view | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-07](../SLICE-0114-ribbon.md#slice-0114-07) |
| 0114-08 | «Extrase de cont» window: its own download button removed | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-08](../SLICE-0114-ribbon.md#slice-0114-08) |
| 0114-09 | «Descarca extrase» in the right group of the ribbon | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-09](../SLICE-0114-ribbon.md#slice-0114-09) |
| 0114-10 | E-Factura: «Sincronizeaza facturi primite» and its inputs in the form group | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-10](../SLICE-0114-ribbon.md#slice-0114-10) |
| 0114-11 | KbotMain: «Editeaza grupe» out of the popup menu, into the ribbon form group | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-11](../SLICE-0114-ribbon.md#slice-0114-11) |
| 0114-12 | KbotMain: a popup on the footer «Actualizeaza» button | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-12](../SLICE-0114-ribbon.md#slice-0114-12) |
| 0114-13 | KbotMain: no «Grupe» row when there is no custom group | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-13](../SLICE-0114-ribbon.md#slice-0114-13) |
| 0114-14 | KbotMain: the E-Factura view button under Extrase, aligned top | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-14](../SLICE-0114-ribbon.md#slice-0114-14) |
| 0114-15 | Rezervari view: Grafice / document / Repara in the ribbon, tree header and footer emptied | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-15](../SLICE-0114-ribbon.md#slice-0114-15) |
| 0114-16 | Receptii view: Asociaza receptii / Reface din istoric in the ribbon, only «Actualizeaza» left in the tree | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-16](../SLICE-0114-ribbon.md#slice-0114-16) |
| 0114-17 | E-Factura view: no «show all» tick, only the «without operations» filter, footer «Actualizeaza» with the list's icon | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-17](../SLICE-0114-ribbon.md#slice-0114-17) |
| 0114-18 | Ribbon fields of «Sincronizeaza facturi primite»: the list opens and has the rows, the second one is a text box | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-18](../SLICE-0114-ribbon.md#slice-0114-18) |
| 0114-19 | E-Factura Vanzare: the month list removed, the tree rows no longer cut (operator item 7.1, 7.2) (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-19](../SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md#slice-0114-19) |
| 0114-20 | Modal windows maximise inside the area of the pages, not at the screen corner (operator report) (17.10.2026) | Build curat; nerulat, nevăzut; ajutor 0000-92 | [0114-20](../SLICE-0113-fereastra-principala-machete-pagini.md#slice-0114-20) |
| 0114-21 | «Facturi primite» window: the empty strip at the bottom is gone (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-21](../SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md#slice-0114-21) |
| 0114-22 | E-Factura VIEW: no options button in the tree header, no supplier level (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-22](../SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md#slice-0114-22) |
| 0114-23 | E-Factura: no bottom padding on the sub-views, narrower trees, «Atasamente» folded into «Continut» (17.10.2026) | Build curat; nerulat, nevăzut pe ecran (checkpoint 17.10.2026) | [0114-23](../SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md#slice-0114-23) |
| 0114-24 | E-Factura: the download id is named in the ANAF download error (17.10.2026 request, 7.3 still open) | Direcția 7.3 închisă de operator la 21.10.2026; server nedeployat la checkpoint | [0114-24 — E-Factura](../SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md#slice-0114-24-efactura) |
| 0114-24 | Ribbon: compact mode and a temporary panel while it is folded (18.10.2026) | Build curat; nerulat, nevăzut; ajutor 0000-100 | [0114-24 — ribbon](../SLICE-0114-ribbon.md#slice-0114-24-ribbon) |
| 0114-25 | Previzualizarea reparării rezervărilor (19.10.2026) | GATA: build curat; operatorul confirmă că merge (21.10.2026). | `SLICE-0114-25-repara-rezervari-fereastra-previzualizare.md#slice-0114-25` |
| 0114-26 | Legături greșite, poarta rezervării și tooltip (23.10.2026) | GATA pe cod: build KBot.App 0 avertismente / 0 erori, py_compile curat; server NEDEPLOYAT (fișier nou + 4 modificate, de pus de mână), nimic rulat pe MariaDB, nimic văzut pe ecran, poarta JS nerulată, regula de detectare gândită doar pe AAB2FTFNE46, teste neatinse. | `SLICE-0114-25-repara-rezervari-fereastra-previzualizare.md#slice-0114-26` |
| 0114-27 | Programul DDF/ORD după SS și repararea antetului (23.10.2026) | Build curat; nerulat, nevăzut. | `SLICE-0114-27-program-ddf-dupa-ss-indicatori.md` |

### Open threads — 0114

- `KBotRibbon.md` (control documentation) and its line in `CONTROLS.md`: not written.
- The Setări › Aplicație switch still reads «Rândul «Jurnal activitate» în meniul MENIU al ferestrei principale»
  (`chkLogViewer`); it now drives the ribbon button. Stale on-screen text, code not changed.
- No keyboard navigation, no drop-down buttons (on purpose, first version). The fly-out while folded came with 0114-24 (never run).

## Slice 0115 — opening page «Acasă» + side panel

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0115 | Acasă și panoul lateral — baza inițială | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-71 | [0115](../SLICE-0115-acasa-panou-lateral.md#slice-0115) |
| 0115-01 | Panou — buton jos, imagini și acțiuni doar azi | GATA pe cod (build 0/0), nevăzut pe ecran; picture of hosted Adobe / browser parts may be blank; help 0000-87 | [0115-01](../SLICE-0115-acasa-panou-lateral.md#slice-0115-01) |
| 0115-02 | Panou — în spatele machetelor, implicit | GATA pe cod (build 0/0), nevăzut pe ecran; help 0000-106 | [0115-02](../SLICE-0115-acasa-panou-lateral.md#slice-0115-02) |

## Slice 0116 — macheta registry

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0116 | Registrul și istoricul machetelor — baza inițială | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-71 | [0116](../SLICE-0116-registrul-de-machete.md) |

## Slice 0117 — recent actions

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0117 | Acțiuni recente — clientul și serverul nelivrat | GATA pe cod — client only (build of the merge 0/0); **server not delivered**; help 0000-71 | [0117](../SLICE-0117-actiuni-recente.md) |

### Open threads — 0117

- [Serverul 0117 nelivrat](../SLICE-0117-actiuni-recente.md): lista rămâne goală până la livrare.

## Slice 0118 — «Jurnal activitate» and «Istoric versiuni» as machete

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0118 | Jurnal și Istoric versiuni ca pagini proprii | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-72 | [0118](../SLICE-0118-jurnal-istoric-machete.md) |

## Slice 0119 — E-Factura on the ribbon

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0119 | E-Factura și comenzile unității în ribbon — inițial | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-73 | [0119](../SLICE-0119-efactura-ribbon.md) |

### Open threads — 0119

- [Mesajul de token rămas în cod](../SLICE-0119-efactura-ribbon.md) este învechit; necorectat aici.
