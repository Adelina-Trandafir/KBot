# K-BOT — STATUS, slices 0110–0119

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0110 — public site (sub-slices 0110-01 .. 0110-13) (landing, request form, read-only user area)

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

## Slice 0110-13 — certificate login network message

12.10.2026: GATA local. `NETWORK` afișează o problemă de comunicare și recomandă
verificarea adresei paginii; numai `CERT_ABSENT` spune că serverul nu a primit
certificatul. Configurația CORS și memorarea certificatului în Chrome neschimbate.
Verificat doar sintactic; de publicat `PYTHON/static/js/portal/certificat.js`, apoi
Ctrl+F5. Fără DDL sau restart backend.
Worklog: `SLICE-0110-13-certificat-mesaj-retea.md`.

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
| 0113 | `AvacontMain` (caption bar + busy bar taken from `KbotForm`, ribbon, side panel, page area); `KbotForm` hosted and loaded behind the opening page; every macheta a page (`ShowPage` / `ShowMachetaAsync`), title in the main caption bar; own close buttons hidden (`HideCloseButtons`), Esc kept; start maximized offered once (`FullScreenOffered`); Adobe / Office find the top-level window (`TopLevelForm`) | GATA pe cod (build of the merge 0/0; 99 failing tests = the same as before), seen on screen only in `KBOT_TEST`; help 0000-69 | [SLICE-0113](../SLICE-0113-fereastra-principala-machete-pagini.md) |
| 0113-01 | **Pagini care rămân deschise + ferestre modale** (16.10.2026, cererea operatorului): paginile (Extrase, Clasificații, Parteneri, Setări, Jurnal, Istoric versiuni, Ajutor, E-Factura Vânzare / Cumpărare) nu se mai închid când se deschide alta, ci se pun deoparte (ascunse, deschise) și revin; editoarele DDF / ORD, legăturile recepțiilor, operațiunile necorelate, graficul rezervărilor, grupele (și catalogul de mesaje, Debug) sunt din nou **ferestre modale** cu bara lor de titlu, mai mici decât spațiul de lucru, centrate pe fereastra principală (`ShowModalAsync`); înainte ca ceva să acopere o pagină / shell-ul / o vedere cu PDF, `IPutAway` mută bara de file pe o filă fără PDF (bara rămâne) și închide documentul din Adobe; **«Machete deschise» e o stivă** (deschisele sus, în ordinea deschiderii și fixe; cele închise în italice la coadă, ies primele când se umple la 10; modalele și vederile shell-ului nu apar) | GATA pe cod (build `KBot.App` 0/0); **nimic rulat, nevăzut pe ecran**; ajutor 0000-86 | [SLICE-0113-01](../SLICE-0113-01-pagini-deschise-si-ferestre-modale.md) |

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
| 0114 | `KBotRibbon` (tabs Angajamente / Date / E-Factura / Configurări, groups, large/small buttons, fold by double click or chevron, `RibbonCollapsed`); replaces MENIU (`RunMenuEntry`, `MirrorMenuTo`, `MenuStateChanged`); pinned «Ajutor» (help page with no macheta, else the full «?» menu of the macheta) and «Închide» (closes the macheta; from the list back to Acasă; absent on Acasă; the app closes only from X); orange accents NOT brought | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST` (orange); help 0000-70 | [SLICE-0114](../SLICE-0114-ribbon.md) |
| 0114-01 | **Ribbon: câmpuri în grup + butoane condiționate** (15.10.2026): `KBotRibbonField` (Combo fără tastare / Text, `DigitsOnly`, `MaxLength`, tooltip) în `KBotRibbonGroup.Fields`, controalele KBot create de ribbon și puse în panou; `FindField`; fila E-Factura: «Cumpărare» înaintea lui «Vânzare», «Sincronizează facturi primite» + lista de zile + caseta de zile vizibile doar cât macheta «Cumpărare» e pe ecran | GATA pe cod (build curat KBot.App); **nimic rulat, nevăzut pe ecran** | `SLICE-0114-01-ribbon-campuri.md` |
| 0114-02 | **Ribbon: separator înaintea unui buton fixat + butoane fixate condiționate** (16.10.2026, cererea operatorului): `KBotRibbonButton.SeparatorBefore` (linie subțire la stânga butonului fixat, doar cât alt buton fixat e vizibil la stânga lui); `KBotRibbon` îi face loc și o desenează (panou și ribbon strâns); pe `AvacontMain`: butonul fixat «Ajutor» scos, butonul nou «Asociază parteneri» (`sumar_parteneri`) apare doar cât vederea «Sumar» e pe ecran, înaintea lui «Închide». Ajutor: 0000-85. | GATA pe cod (build curat, nerulat, nevăzut) | [SLICE-0114-02](../SLICE-0114-02-ribbon-separator-si-butoane-fixate-conditionate.md) |
| 0114-03 | **Ribbon: grupul din dreapta `RightGroup`** (16.10.2026, cererea operatorului): un grup cu butoane și câmpuri (ca cele ale filelor), în dreapta panoului, înaintea lui «Închide», pe orice filă, afișat doar cât are ceva vizibil; gazda (AvacontMain) îi arată elementele pentru vederea de pe ecran; «Asociază parteneri» mutat în el (grupul «Sumar»). Ajutor: 0000-85. | GATA pe cod (build curat, nerulat, nevăzut) | [SLICE-0114-03](../SLICE-0114-03-ribbon-grup-dreapta.md) |

### Open threads — 0114

- `KBotRibbon.md` (control documentation) and its line in `CONTROLS.md`: not written.
- The Setări › Aplicație switch still reads «Rândul «Jurnal activitate» în meniul MENIU al ferestrei principale»
  (`chkLogViewer`); it now drives the ribbon button. Stale on-screen text, code not changed.
- No keyboard navigation, no fly-out while folded, no drop-down buttons (on purpose, first version).

## Slice 0115 — opening page «Acasă» + side panel

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0115 | `AcasaPage` («Ce este nou» card, opens by itself once after an update — `NewsSeenVersion`, `HomeNewsCollapsed`; new logo bottom-right; no banner); side panel `KBotSidePanel` beside every page: Machete deschise, Acțiuni recente, Informații, Erori din sesiune (`GlobalErrorLog.RecentErrors` / `Written`); folds to an icon strip (`HomeSidebarCollapsed`); `ButtonStyles.ApplyTransparent` | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-71 | [SLICE-0115](../SLICE-0115-acasa-panou-lateral.md) |

## Slice 0116 — macheta registry

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0116 | `MachetaRef` / `MachetaRegistry` / `MachetaHistory` (KBot.Common); history only in the session; one opening = one entry; `OpenRegistered` = the single opening point from menu / ribbon; views noted by `NoteViewOpened`; reopening switches year, selects the angajament, opens the view (editors reopen in the DDF / ORD view); unsaved new angajament («!») not recorded | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-71 | [SLICE-0116](../SLICE-0116-registrul-de-machete.md) |

## Slice 0117 — recent actions

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0117 | Client of `GET /api/actiuni?n=10` (`RecentAction`, `IActionsApi`, `ApiClient.Actions.vb`); list in the side panel, read again 2 s after the last write of a burst (`ServerGateHandler.WriteSucceeded`); old server → 404 → empty list. Server half (`routes/actiuni.py`, `after_request` into `Jurnal` for the white-listed routes) exists ONLY in `KBOT_TEST\PYTHON` | GATA pe cod — client only (build of the merge 0/0); **server not delivered**; help 0000-71 | [SLICE-0117](../SLICE-0117-actiuni-recente.md) |

### Open threads — 0117

- Bring `KBOT_TEST\PYTHON\routes\actiuni.py` (+ 2 lines in `main.py`) into `KBOT\PYTHON` and deploy; until then
  «Acțiuni recente» is empty for everybody.

## Slice 0118 — «Jurnal activitate» and «Istoric versiuni» as machete

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0118 | Both pages left `SetariForm` (designer + `CreateView`); `JurnalPage` / `IstoricVersiuniPage` host `SetariJurnalView` / `SetariIstoricView` as pages (ribbon Configurări › Configurare); journal: today's day expanded (`KBotDataView.ExpandGroup`), adjustable splitter grid / detail (`splitJurnal`) | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-72 | [SLICE-0118](../SLICE-0118-jurnal-istoric-machete.md) |

## Slice 0119 — E-Factura on the ribbon

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0119 | Tab E-Factura: group E-Factura («Vânzare» = `FacturiForm`, «Cumpărare» = `PrimiteForm` as page), group ANAF («Token ANAF», «Sincronizează facturi primite», dialogs); the title-bar options of `FacturiForm` hidden (`ShowOptionsButton = False`); «Conturi Unitate» / «Date Unitate» in Configurări › Unitate, open without the E-Factura window (`RunUnitCommand`; an open `FacturiForm` hears the change) | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-73 | [SLICE-0119](../SLICE-0119-efactura-ribbon.md) |

### Open threads — 0119

- `FacturiForm.Trimitere.vb` still tells the operator «Reînnoiți tokenul cu butonul «Token ANAF» din subsolul
  ferestrei.»; the button is on the ribbon (E-Factura › ANAF). Stale on-screen text, code not changed.
