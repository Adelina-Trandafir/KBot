# SLICE-VC08-11 — Rapoarte OI: structura de referință și selecția MF

Data: 2026-10-23.

MAGAZIE rămâne rădăcină separată, cu toate rapoartele și grupările existente,
ca în celelalte gestiuni. Ramura OBIECTE DE INVENTAR păstrează cele două
niveluri de grupare Root și Niv1: OBIECTE DE INVENTAR → BONURI / PRODUSE /
PRIMITORI, urmate de frunzele rapoartelor. Numai această ramură este limitată
la cele 11 rapoarte din imaginea furnizată, în această ordine:

- BONURI: Bonuri casare; Bonuri predare; Bonuri transfer.
- PRODUSE: Situație produse; Stoc final produse; Balanță obiecte inventar;
  Lista inventar.
- PRIMITORI: Situație primitor; Stoc final primitor; Lista inventar;
  Fișă de evidență / spații O.I.

Fiecare raport OI reutilizează selecția ierarhică MF, inclusiv încărcarea
la deschiderea ramurii, bifarea ramurilor și „Selectează TOATE”. Pentru
bonuri se folosește primitor–bon–produs; pentru PRODUSE, cont–produs;
pentru PRIMITORI, primitor–cont–produs. Ramurile independente „Conturi
în folosință” și „Deținători” nu mai apar la aceste rapoarte. Protocolul
intern existent `mf_selection` transmite identitățile complete până la
generarea snapshotului comun PDF/Excel. Schimbarea raportului reîncarcă
selecția, ca la MF.

Interpretarea inițială eliminase MAGAZIE; a fost corectată local la solicitarea utilizatorului, păstrând limitarea celor 11 rapoarte numai pentru OBIECTE DE INVENTAR.

Calculele OI, sursele SELECT, machetele și aliasurile folosite direct de
documente se păstrează. Celelalte tipuri de gestiuni păstrează catalogul
și structura curente. Documentația și ajutorul textual descriu noua
structură; capturile existente nu au fost modificate.

Fișiere de aplicație:

- `PYTHON/routes/vercon/report_catalog.py`
- `PYTHON/routes/vercon/report_mf.py`
- `PYTHON/routes/vercon/report_mf_selection.py`
- `PYTHON/routes/vercon/report_service.py`
- `PYTHON/static/js/vercon/report-result.js`
- `PYTHON/static/js/vercon/reports.js`
- `PYTHON/static/help/vercon.json`

Documentație: `PYTHON/reports/vercon/README.md`, acest jurnal și
`implementare_vercon_v2.md`.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Rapoarte MF și perioade — 20.10.2026

Opt etape preluate din jurnalul general prin D001-05. Fiecare era marcată TESTAT, RULAT, PUBLICAT, confirmată de operator la 23.10.2026. Această confirmare istorică privește etapele de mai jos, nu alte restanțe. Structura OI de la începutul jurnalului este revizia ulterioară din 23.10.

### Catalog și selecția specifică MF

- Sub Mijloace Fixe, bonurile sunt direct la rădăcină, în ordinea Bon Predare/Transfer/Casare; dispare BONURI OI / MF. MF Balanta devine Balanta; MF Balanta SUBINV este ascuns. MF Lista Inventar devine Lista Inventar, direct la rădăcină, fără INVENTAR DETINATORI. Afișarea comună rămâne cu majuscule; această etapă nu schimbă gruparea OI.
- Rădăcinile generale Conturi în folosință/Primitori/Documente sunt înlocuite pentru MF: bonuri = primitor → bon (număr, dată, valoare) → produse, bonuri descrescător după dată/număr; Balanta = cont → produse; Lista Inventar = cont → primitor → produse, după conturile registrului de folosință.
- Amortizare = contul produsului → clasificare MF → produse. «Grupează după Primitori», inițial nebifat, schimbă în primitor → clasificare → produse. Deținerea vine din mișcări până la data raportului; fără deținere pozitivă = Fără primitor. Clasificarea folosește CC/catalogul comun, SELECT cu context. Produsul include nr. inventar și UM, fără nivel separat pentru variante.
- Ramuri pliate/lazy; bifarea ramurii, Selectează TOATE și căutarea folosesc încărcarea recursivă comună. Schimbarea raportului MF ori a grupării resetează arborele/selecțiile; celelalte rapoarte păstrează reutilizarea pe categorie.
- Identități compuse pentru afișare/PDF/Excel: cont/produs la balanță; cont/primitor/produs la inventar; primitor/bon/produs la bonuri; cont/primitor/variantă la amortizare. O variantă selectată în mai multe ramuri se calculează o singură dată la amortizare. Gruparea schimbă arborele, fără schimbarea machetei/formulelor exportului.

### Istoric comun și perioade

- An este ascuns/dezactivat în meniul principal MF/OI, dar contextul intern multianual rămâne pentru rute. Implicit: 1 ianuarie din anul curent–azi, Europe/Bucharest cu fallback UTC dacă lipsesc fusurile orare. Anul curent este disponibil și când ultimele date sunt mai vechi. Exercițiile anuale ale celorlalte gestiuni păstrează comportamentul.
- TOTI ANII este primul la rădăcina perioadelor MF/OI: prima dată efectivă a mișcărilor–azi. Intrări/storno/ieșiri/retur și BP/BT/BS neanulate, cu linii, sunt filtrate pe gestiune; intrările folosesc COALESCE(DataFc,Data). Fără mișcări: 1 ianuarie curent–azi; numai documente viitoare: azi–azi. Calendarul permite manual și anii intermediari fără mișcări.
- Anii sunt descrescători, TOTI anii primul. În MF/OI anul curent se expandează numai dacă este ultimul cu date efective; altfel toate ramurile sunt pliate. Serverul calculează `latest_data_year` înainte de adăugarea anului curent sintetic. Dacă proprietatea lipsește în răspunsul vechi, clientul folosește `latest_year`, cu normalizare numerică. Un `latest_data_year: null` explicit nu expandează anul curent. Distincția an real/sintetic cere și versiunea Python nouă. `expanded` se aplică prin ComboTree după setData și se sincronizează cu expandedNodes pentru afișare/redeschidere.
- Lunile fiecărui an sunt descrescătoare, de la ultima mișcare până la ianuarie, inclusiv lunile intermediare goale. TOTI anii/Tot anul rămân. Catalogul `last_movement_month` este separat de limitele introducerii manuale: toate gestiunile folosesc liniile mișcărilor reale și datele efective ale motorului, inclusiv storno/retur; MF/OI includ BP/BT/BS active. Anulatele, antetele fără linii și data listei de alimente nu extind lunile. An sintetic fără mișcări = 0, fără luni fictive; implicitul și datele manuale rămân.

### Schimbarea perioadei peste PDF

- Numai VERCON oferă acțiunea calendar «Schimbă perioada» în meniul PDF. Popupul reutilizează ComboTree, casete/etichete, catalog, TOTI anii, ordinea și limitele din RAPOARTE; Amortizare are o singură dată.
- Pornește cu perioada raportului și aplică numai la confirmare: o copie a parametrilor originali schimbă exclusiv start/end. Produse/primitori/documente, gestiune/exercițiu și grupări rămân; nu reîncarcă arborele, nu elimină bife după noul interval și nu modifică formularul RAPOARTE din fundal.
- PDF-ul rămâne în aceeași fereastră; titlu/perioadă, tipărire, descărcare și Excel folosesc snapshotul nou. La generare eșuată rămân draftul popupului și raportul anterior; la încărcarea PDF nou eșuată se reîncarcă anteriorul. Renunță abandonează înainte de aplicare; închiderea raportului/paginii eliberează popupul.
- Și previzualizările oficiale VERCON primesc acțiunea; ciornele locale nu au sursă de regenerare. Viewerul comun primește doar extensii opționale de acțiuni/înlocuire PDF, alte aplicații nu primesc calendar. Fără rută sau schemă nouă.
- În ambele locuri selectorul perioadei setează `minWidth=0`, astfel dropdownul are lățimea casetei. ComboTree folosește fallback nullish, nu `||`, respectând zero; fără configurare rămâne implicitul 300 px. Geometria și limita viewportului se păstrează.

### Fișiere pe etapă (căile relative sunt în grupul indicat)

| Etapă | Fișiere |
|---|---|
| Catalog MF | PYTHON/routes/vercon/report_catalog.py; PYTHON/static/js/vercon/report-result.js |
| Selecție MF | PYTHON/routes/vercon/: report_mf.py și report_mf_selection.py noi, report_catalog.py, report_selection.py, report_routes.py, report_service.py, report_data.py; PYTHON/static/js/vercon/: reports.js, report-selection.js |
| MF/OI fără an; ani/compatibilitate/luni | PYTHON/static/js/vercon/app.js (ascunderea An), report-period.js; PYTHON/routes/vercon/report_period.py (istoric/catalog); compatibilitatea răspunsului vechi modifica numai report-period.js |
| Perioada peste PDF | PYTHON/static/js/vercon/: report-period-action.js nou, reports.js, document-report.js, pdf-preview.js; PYTHON/static/js/reporting/: pdf-preview.js, report-dialog.js; PYTHON/static/css/: vercon-report-period.css nou, vercon.css |
| Minima zero | PYTHON/static/js/components/combotree/combotree.js; PYTHON/static/js/vercon/report-period.js |

Toate cele opt etape actualizau și `implementare_vercon_v2.md`. Consolidarea schimbă numai documentația.
