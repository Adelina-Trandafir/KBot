# Implementare VERCON v2

Convenție editorială (D001-04): marcajul **Confirmat 23.10** abreviază exclusiv «TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026)». Se aplică numai intrării care poartă marcajul; nu confirmă alte intervenții, limitări sau cazuri individuale. Stările locale/nerulate și confirmările ulterioare din workloguri rămân distincte.

## Actualizare 19.10.2026 — loading imediat și blocarea generărilor simultane (ADE9-07)

Loading-ul, blocarea per pagină, erorile, fișierele și publicarea comună sunt în [ADE9-07](docs/worklog/SLICE-ADE9-07-rapoarte-loading-blocare.md). Stare consemnată: TESTAT, RULAT, PUBLICAT, confirmat de operator la 23.10.2026.

## Actualizare 19.10.2026 — feedback comun al butoanelor VERCON/ADECHIT (ADE9-06)

Contractul efectelor, excepțiile pentru controalele integrate și fișierele de publicat împreună sunt în [ADE9-06](docs/worklog/SLICE-ADE9-06-butoane-stil-vercon.md). Stare consemnată: TESTAT, RULAT, PUBLICAT, confirmat de operator la 23.10.2026.

## Actualizare 19.10.2026 — motor și previzualizare de rapoarte comune cu ADECHIT (ADE9-05)

Motorul, adaptoarele VERCON/ADECHIT, limitele configurărilor și predarea sunt în [ADE9-05](docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md); API-ul comun este în [reporting/README](PYTHON/reporting/README.md). Stare consemnată: TESTAT, RULAT, PUBLICAT, confirmat de operator la 23.10.2026.

## Actualizare 19.10.2026 — Rapoarte: conturi direct în selecție și Selectează TOATE

Detalii: [reviziile rapoartelor, etapa 0](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

## Actualizare 14.10.2026 — macheta NIR: antet obligatoriu înainte de grid, meniu «Factură Cumpărare (NIR)», `dropDownStyle`

Detalii: [istoric consolidat](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-13-editor-nir), D001-13. Componente și mod de test: [contracte comune](VERCON/CONTROALE_CUSTOM_THREAD.md); [tipografie](VERCON/TIPOGRAFIE_MIGRARE.md).

## Actualizare 13.10.2026 — TextBox și CheckBox comune; arborele NU se mai reconstruiește la click pe un document

Detalii: [istoric consolidat](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-13-editor-nir), D001-13. API-uri și limite: [contractele controalelor](VERCON/CONTROALE_CUSTOM_THREAD.md#d001-13-integrare).

## Actualizare 13.10.2026 — arbore NIR încărcat la cerere, bifă «Stoc inițial», listă Combobox

Detalii: [istoric consolidat](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-13-editor-nir), D001-13.

## Actualizare 13.10.2026 — eroare la încărcarea produselor NIR

Utilizatorul a raportat `Cannot read properties of undefined (reading 'filter')` în `productOptions` și a precizat că a publicat modificările și repornit serverul. Ipoteza anterioară a asistentului privind un server nerepornit nu este susținută și a fost retrasă. În sursele locale, `documents.lookups(..., 'nir')` include deja `products`; cauza diferenței față de răspunsul serverului nu a fost stabilită prin examinarea surselor. Nu s-a inspectat sau executat aplicația de pe server.

Corecții scrise: `PYTHON/static/js/vercon/docs.js` folosește un acces protejat comun la catalog și validează răspunsul înainte de montarea documentelor; un răspuns fără catalog este refuzat explicit, fără a prezenta o listă goală ca rezultat valid. Consola înregistrează ruta și numele proprietăților primite, fără valori de produs/furnizor. `PYTHON/routes/vercon/doc_routes.py` garantează lista pentru NIR la nivelul rutei: dacă modulul nu o furnizează, o citește direct din `VC_Produse` prin `receipt_choices.products`, cu subunitatea și gestiunea verificate. Nu se revine la selectarea variantelor de preț.

Stare: **CORECȚII_LOCALE_SCRISE, CAUZĂ_PE_SERVER_NECONFIRMATĂ**. Fără teste, porniri/reporniri, cereri către server sau operații SQL, conform cererii utilizatorului. Nu se declară problema de pe server rezolvată prin execuție.

## Actualizare 13.10.2026 — feedback recepție: fundal, selecții și ciornă

Detalii: [istoric consolidat](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-13-editor-nir), D001-13.

## Actualizare 13.10.2026 — corecții de dimensiuni și încadrare în fereastră

Cerere: cele cinci capturi de feedback pentru recepție; săgețile desenate indicau mărirea câmpurilor, nu butoane de copiere. Eliminat butoanele introduse greșit și textele informative de sub câmpuri; explicațiile rămân în tooltip, iar erorile reale rămân vizibile. Antet cu lățime egală de 260px pentru toate cele patru coloane, distanță orizontală de 12px și verticală de 8px, fără rânduri minime de 70px. Coloana Cantitate NIR folosește ponderea 64 în loc de 128; macheta grilei are cheie nouă pentru ca vechile lățimi salvate să nu prevaleze.

Pagina este limitată la 1440px. Limita de 2048px citită din ADECHIT a fost respinsă explicit de utilizator și înlocuită. Plafonul vertical adoptat este 1080px. Sub aceste limite folosește spațiul disponibil al ferestrei; peste ele este centrată pe ambele axe, cu fundal alb exterior. Headerul, navigarea și paddingurile sunt incluse în înălțime. Documentul și tabelul folosesc înălțimea rămasă prin flex/grid, înlocuind înălțimea NIR impusă prin clamp. Scrollul documentului HTML este eliminat; tabelele și panourile își gestionează conținutul în interior. Adaptate înălțimile listei de alimente și ale paginii de rapoarte la același spațiu disponibil.

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/css/vercon-receptie.css`, `PYTHON/static/css/vercon-workspace.css`, `PYTHON/static/css/vercon.css`. Stare: **IMPLEMENTAT_LOCAL, NERULAT**. Au fost citite sursele și captura de proiectare; fără teste, build, verificări în browser, interogări sau modificări MariaDB.

## Actualizare 13.10.2026 — remedierea accesului localhost

Probleme identificate: pagina launcherului SSH nu avea marcajul separat cerut pentru evitarea redirecționării client spre loginul portalului; `preview.py`, comanda folosită de utilizator, continua să pornească demonstrația cu date fictive. Mesajul inițial de pornire a fost insuficient pentru confirmarea accesului real. Sonda PowerShell `Invoke-RestMethod localhost` nu ajungea la server în acest mediu.

Corecții: `PYTHON/static/js/vercon/gate.js` recunoaște marcajul separat `data-vc-local-ssh`; `VERCON/tools/local_ssh.py` îl adaugă numai în pagina locală, păstrând formularele reale și autentificarea locală prin cookie. Expune o stare locală cu identitatea launcherului și baza fixă. Directorul configurărilor de raport este aplicat explicit și când modulele au fost deja importate. `VERCON/tools/preview.py` pornește implicit `local_ssh`; demonstrația fără MariaDB cere `--fixtures`. `Start-Localhost.ps1` raportează succesul numai după răspunsul launcherului corect, prin HttpClient fără proxy pe `127.0.0.1`. Completate `VERCON/tools/README.md` și `LOCALHOST.md`.

Stare actuală: **SERVER_LOCAL_PORNIT, CITIRE_DATE_CONFIRMATĂ_PRIN_API**. La diagnosticarea problemei au fost folosite exclusiv cereri de citire: pagina locală poartă marcajul corect, `/api/portal/me` răspunde `000_DEMO`, contextul real și listele NIR răspund cu succes. Pentru 2026 sunt 509 recepții în Alimente și 46 în Materiale. Nu s-au salvat/anulat documente și nu s-au modificat scheme. Nu au fost rulate teste automate; inspecția vizuală automată a browserului a fost blocată de politica instrumentului. Nu se afirmă verificarea vizuală a formularelor sau verificarea tuturor exporturilor.

## Actualizare 13.10.2026 — Recepție nouă și localhost cu 000_DEMO

Cerere: `VERCON/CAPTURI_SCHIMBARI_V1/Receptie_noua_modificari.png`, apoi posibilitatea testării manuale a tuturor modificărilor curente pe localhost. Utilizatorul a ales explicit baza MariaDB existentă `000_DEMO` prin SSH, cu configurația folosită de KBotAdmin.

### Recepție nouă

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/css/vercon-receptie.css`, `PYTHON/static/css/vercon.css`, `PYTHON/routes/vercon/receipt_number.py`, `PYTHON/routes/vercon/doc_routes.py`. Antet în patru coloane cu câmpuri de aceeași lățime: furnizor/cont/comisie, factură, NIR, valoare/cotă TVA. Săgeți pentru copierea numărului și datei facturii spre NIR. Număr NIR propus automat la adăugare și verificat la introducere; endpointul propunerii execută doar SELECT, nu rezervă numere. Salvarea verifică din nou numărul în tranzacția existentă. Cotă TVA în antet pentru toate liniile ciornei și ca valoare implicită la rândurile noi; fiecare linie păstrează și selectorul propriu. Cele cinci coloane valorice/TVA au lățimi proporționale egale. Grilă mai înaltă și zona de tabel/acțiuni limitată la 1450px.

Controale comune: `PYTHON/static/js/components/combobox/combobox-ui.js`, `combobox-events.js`, `PYTHON/static/js/dgv/editing.js`: opțiune explicită `showArrow` pentru liste căutabile; activată în recepție. Stilurile sunt numai în CSS. `PYTHON/routes/vercon/__init__.py`: CSP permite resursele blob necesare imaginilor PDF, workerului și iframe-ului de tipărire.

### Localhost real prin SSH

Fișiere noi: `VERCON/tools/local_ssh.py`, `Start-Localhost.ps1`, `Stop-Localhost.ps1`, `LOCALHOST.md`, `.gitignore`; completat `VERCON/tools/README.md`. Server local `http://localhost:5053/vercon`, legat exclusiv de `000_DEMO`. SSH folosește cheia/configurația existentă a proprietarului, citită din `_Admin/admin_settings.json`; conexiunea deja deschisă în KBotAdmin nu este preluată. Credencialele MariaDB sunt citite din configurația serverului și păstrate numai în memoria procesului. Conectorul permite numai `000_DEMO`, cu citire calificată a lățimilor Excel existente din `AVACONT_COMUN`. Salvarea/anularea documentelor produce modificări reale în datele `000_DEMO`. Nu se modifică scheme, nu se publică fișiere și nu se repornește serviciul public.

Autentificarea locală se bazează pe accesul SSH al administratorului, cu toate operațiile VERCON; nu reproduce rolurile și autentificarea portalului. Actor audit implicit `localhost-ssh@local.test`, configurabil prin `-Actor`. Folosește blocarea gestiunii SQL reală. Machetele personalizate ale rapoartelor locale au director separat `.local/report-config`; implicit folosește variantele noi din cod. HTML/CSS/JS se recitesc la reload; modificările Python cer restart.

Stare: **IMPLEMENTAT_LOCAL, SERVER_LOCAL_PORNIT**. Pornirea a reușit: configurația SSH citită, tunel deschis, conexiune MariaDB și citire de pornire din `000_DEMO.VC_Subunitati` reușite; Flask ascultă pe `127.0.0.1:5053`. Mediul local conține Flask, mysql-connector, ReportLab și openpyxl. Fila localhost a fost solicitată în Codex. Nu au fost rulate teste automate, build, operații de salvare sau verificări funcționale în browser. Interfața și exporturile rămân de testat manual de utilizator. Captura originală a rămas nemodificată.

## Actualizare 13.10.2026 — corecții CAPTURI_SCHIMBARI_V1

Scop aprobat: toate marcajele din cele 11 capturi, cu precizarea utilizatorului că pagina Rapoarte preia structura Access și păstrează stilurile VERCON. Implementare locală scrisă; aplicația, exporturile, tipărirea și selecțiile nu au fost executate sau verificate. Fără teste, build, browser, interogări SQL ori modificări de schemă, conform AGENTS.md. Capturile sursă au rămas nemodificate.

### Context și navigare

Fișiere: `PYTHON/static/vercon.html`, `PYTHON/static/js/vercon/app.js`, `PYTHON/static/css/vercon-layout.css`, `PYTHON/static/css/vercon-responsive.css`. Instituția, gestiunea și anul sunt în header. Eliminate căutarea globală, schimbarea unității, tipul duplicat, indicatorul Consultare și bara cu context repetat; fila Stocuri este ascunsă. Etichetele gestiunilor nu repetă tipul în paranteze. Ocuparea gestiunii, drepturile și protecția ciornelor rămân active pe fluxurile existente.

### NIR, BC, storno și retur

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/css/vercon-documente-layout.css`, `PYTHON/static/css/vercon-documente-panels.css`, `PYTHON/static/css/vercon-integration.css`. Antete compacte, coloane proporționale în tabel, fără spațiul gol din dreapta; prețul și cota TVA NIR sunt îngustate. Panoul Produse disponibile BC a fost eliminat; alegerea produsului și disponibilul pe data bonului rămân în grilă. Arborii salvărilor pornesc restrânși la rădăcină, fără căutare, și își păstrează ramurile deschise când se schimbă documentul. Înălțimea lor urmărește panoul principal.

Totalul grilei este suma liniilor. O valoare istorică diferită din antet se afișează separat, fără rescrierea documentului. Numărul salvat este inclus explicit în antetul pentru previzualizare; cota TVA este afișată ca procent.

### Lista de alimente

Fișiere: `PYTHON/static/js/vercon/lista.js`, `PYTHON/static/css/vercon-lista.css`, `PYTHON/static/css/vercon-responsive.css`. Antet vertical în prima coloană; bonurile asociate sunt sus, în coloanele următoare. Panourile meselor sunt mai înalte. Coloana vizibilă IdFel a fost eliminată; Tip se afișează ca Fel de mâncare, iar Explicații rămâne editabilă. IdFel existent rămâne în date și payload; rândurile noi pot avea IdFel nul, conform contractului existent. Grilele folosesc lățimea disponibilă fără scroll orizontal. Arborele listelor are același comportament restrâns, fără căutare.

### Previzualizare și acțiuni PDF

Fișiere: `PYTHON/static/js/vercon/document-report.js`, `PYTHON/static/js/vercon/pdf-preview.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/js/vercon/editor.js`, `PYTHON/static/js/portal/pdfview.js`, `PYTHON/static/css/vercon-preview.css`, plus apelurile din docs/lista. Documentele salvate Materiale/Alimente folosesc direct rapoartele oficiale și snapshotul serverului. Ciornele, documentele anulate și familiile amânate folosesc PDF-ul local; un cititor fără dreptul separat de raport păstrează previzualizarea datelor deja încărcate. Fereastra intermediară a documentului nu mai apare. Dialogul PDF ocupă spațiul disponibil și include descărcare, tipărire/alegere imprimantă prin dialogul browserului, zoom, potrivirea lățimii și navigarea paginilor. Ajutorul și istoricul păstrează prezentarea lor tabelară.

### Pagina Rapoarte și selecții multiple

Fișiere: `PYTHON/static/js/vercon/reports.js`, `PYTHON/static/js/vercon/report-selection.js`, `PYTHON/static/js/vercon/report-result.js`, `PYTHON/static/css/vercon-rapoarte.css`, `PYTHON/routes/vercon/report_routes.py`, `PYTHON/routes/vercon/report_data.py`, `PYTHON/routes/vercon/report_service.py`. Perioada și opțiunile sunt în stânga, tipurile grupate Produse/Intrări/Ieșiri în centru, datele cu bife în dreapta. Listare și Export Excel generează raportul cu selecția curentă. Rezultatul tabelar în pagină este opțional. Produsele sunt grupate cont → produs → variante; se pot selecta ramuri întregi. Filtrele opționale acceptă selecții multiple, filtrate și pe server; rapoartele care cer document/produs/furnizor unic păstrează această cerință. Nu se extind drepturile sau limitele unitate/subunitate/gestiune/exercițiu. Exportul Excel păstrează modulul comun de lățimi configurate.

### Macheta NIR

Fișiere: `PYTHON/routes/vercon/report_layouts.py`, `PYTHON/routes/vercon/report_body.py`. Macheta implicită include coloana și totalul TVA, codul fiscal al furnizorului în antet, numărul storno afișat pozitiv, categoria și tabelul comisiei/semnăturilor după modelul furnizat. Diferențele antet–linii sunt indicate și în raport. Configurările personalizate salvate nu sunt suprascrise; noile valori implicite se aplică unde nu există machetă personalizată.

Stare finală a acestui slice: IMPLEMENTAT_LOCAL, NERULAT. Fidelitatea vizuală și funcționarea efectivă a descărcării, tipăririi și selecțiilor rămân neconfirmate în execuție, deoarece utilizatorul nu a cerut verificări.

## Actualizare 13.10.2026 — rapoarte Magazie–Alimente și infrastructura Excel comună

Catalogul, cele 16 referințe, implementarea, administrarea din KBotAdmin, modulele și limitele sunt în [VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md); contractul comun Excel este în [reporting/README](PYTHON/reporting/README.md). Stare istorică la 13.10.2026: IMPLEMENTAT_LOCAL, NERULAT, fără paritate reală sau acceptare vizuală; VC08-04/05 și rapoartele OI/MF nu sunt declarate livrate de acest slice.

## Actualizare 12.10.2026 — VC08-02, amortizare

Formulele, datele sursă, accesul MF, interfața, cele 11 probe sintetice pregătite și limitele sunt în [VC08-02](docs/worklog/SLICE-VC08-02-motor-amortizare.md). Stare istorică la 12.10.2026: IMPLEMENTAT_LOCAL, NERULAT; fără probe pe date reale sau acces MariaDB. Continuarea de atunci este VC08-03, cu followup-ul UI separat.

---

## Actualizare 12.10.2026 — CSS modular și VC08-01

Detalii: [istoric consolidat](docs/worklog/SLICE-VC08-01-motor-stoc-si-balante.md#d001-13-css), D001-13.


---

Început: 12.10.2026. Cerință: rescrierea tuturor ecranelor VERCON deja existente în `PYTHON/static`, după mockupurile v2 și folosind controalele custom comune. Referințe: `VERCON/FLUXURI_UI.md`, `VERCON/CONTROALE_CUSTOM_THREAD.md`, `VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md`.

Rescrierea inițială încheiată local: 12.10.2026. Toate ecranele existente din tabel au fost rescrise fără teste, build sau rularea aplicației în acea etapă. Verificările solicitate ulterior pentru CSS și VC08-01 sunt consemnate mai sus. IMPLEMENTAT_LOCAL nu certifică integrarea pe date reale.


---

Început: 12.10.2026. Cerință: rescrierea tuturor ecranelor VERCON deja existente în `PYTHON/static`, după mockupurile v2 și folosind controalele custom comune. Referințe: `VERCON/FLUXURI_UI.md`, `VERCON/CONTROALE_CUSTOM_THREAD.md`, `VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md`.

Rescrierea inițială încheiată local: 12.10.2026. Toate ecranele existente din tabel au fost rescrise fără teste, build sau rularea aplicației în acea etapă. Verificările solicitate ulterior pentru CSS și VC08-01 sunt consemnate mai sus. IMPLEMENTAT_LOCAL nu certifică integrarea pe date reale.

## Reguli pentru continuarea în alt chat

- Se lucrează în checkoutul curent. Nu se execută teste, build, verificări de sintaxă sau aplicația fără cererea explicită a utilizatorului. Citirea codului și proiectarea sunt permise.
- Nu se execută modificări de scheme MariaDB. Această rescriere este de interfață; API-urile, drepturile, contextul și regulile de salvare se păstrează.
- Grile DataGrid compacte ca NIR, selecții Combobox, date DatePicker, arbori TreeView; meniuri/dialoguri/mesaje/PDF comune. Draftul se salvează la comanda documentului, nu prin HTTP la fiecare celulă.
- Arborele documentelor este în dreapta în consultare, ascuns în editor. Lista de alimente are mai multe BC-uri și nu repetă consumul.
- Se completează acest fișier imediat după fiecare ecran/familie, apoi se continuă cu următorul. „IMPLEMENTAT_LOCAL” înseamnă cod scris, nu verificat sau acceptat.

## Progres

| Ecran / familie | Stare |
|---|---|
| Structură comună, context și navigare | IMPLEMENTAT_LOCAL |
| NIR, inclusiv extensia MF | IMPLEMENTAT_LOCAL |
| Bon de consum | IMPLEMENTAT_LOCAL |
| Storno și retur | IMPLEMENTAT_LOCAL |
| Predare, transfer, casare | IMPLEMENTAT_LOCAL |
| Lista de alimente și alegere BC | IMPLEMENTAT_LOCAL |
| Registru MF | IMPLEMENTAT_LOCAL |
| Nomenclatoare și variante de preț | IMPLEMENTAT_LOCAL |
| Setări gestiune | IMPLEMENTAT_LOCAL |
| Editorul de preview existent | IMPLEMENTAT_LOCAL |

Stocuri, registrul OI și rapoartele complete nu au în prezent un ecran funcțional în această pagină; existența lor în mockupuri nu înseamnă că API-urile sunt implementate. Se consemnează separat la final, fără date fictive în fluxul real.

## Integrarea finală și predarea

- Toate familiile comune sunt utilizate: DataGrid, TreeView, Combobox, DatePicker, openMenu/closeMenu, confirmBox/reasonBox/pickBox, viewer PDF, mesaje/jurnal și timer de sesiune. Layouturile grilelor sunt conectate prin providerul comun și ID-uri proprii VERCON.
- `js/vercon/ui.js` centralizează montarea și cleanupul. `js/vercon/snapshot-pdf.js` produce local PDF-uri paginate ale valorilor din previzualizare, cu diacritice, în viewerul comun; se pot descărca și deschide pentru tipărire. PDF-ul este rasterizat și nu înlocuiește rapoartele oficiale încă neimplementate.
- Antetele și celulele păstrează valori canonice, sumele monetare fiind șiruri. Calculele locale sunt informative; serverul păstrează autoritatea asupra valorilor, disponibilului, drepturilor și reviziilor.
- POST-urile incerte păstrează corpul exact și cheia; câmpurile sunt blocate până la reluarea salvării sau renunțare. Editorul preview folosește reconcilierea existentă. O salvare confirmată este afișată înainte de reîncărcarea arborelui, astfel încât eșecul reîncărcării să nu transforme salvarea într-un răspuns incert.
- Meniurile și overlayurile Combobox se montează în dialogul gazdă; dropdownurile se poziționează la deschidere și scroll/resize. Footerul DataGrid respectă disabled/span și permite totaluri explicite independente de filtrare.
- BC cere disponibilul pentru data documentului și produsul selectat, excluzând bonul editat. Prețurile produselor existente pe BC/BP sunt păstrate din document. BT/BS păstrează lotul și disponibilul predătorului.
- Navigarea ține cont de ciorne, serializarea deschiderii formularelor și context; accesul exclusiv se actualizează după ocupare/pierdere. Timerul existent are cleanup; nu pornește un al doilea monitor.
- Fișiere comune adaptate: `js/dgv/datagrid.js`, `js/dgv/editing.js`, `js/components/combobox/combobox-ui.js`, `js/adechit/menu.js`, `js/adechit/session.js`. Restul modificărilor sunt în `vercon.html`, `css/vercon.css` și `js/vercon`.
- Harta componentelor pentru alt thread este actualizată în `VERCON/CONTROALE_CUSTOM_THREAD.md`. Nu s-au schimbat scheme, nu s-a executat SQL și nu s-au adăugat API-uri fictive.

Pentru o continuare: citiți acest jurnal și harta componentelor, apoi mockupurile/FLUXURI_UI. Validarea în browser și testele necesită o cerere explicită a utilizatorului conform AGENTS.md. Implementarea ecranelor lipsă (stocuri, registru OI, rapoarte oficiale) este o etapă separată; rescrierea ecranelor existente este încheiată.

### 1. Structură comună, context și navigare

Scrise: `PYTHON/static/js/vercon/ui.js`, modificări în `vercon.html`, `app.js`, `css/vercon.css`. Header cu căutare în ecranul curent, ajutor, temă și schimbarea unității; contextul folosește Combobox și pentru tip. Navigarea folosește operațiile primite de la server și meniul comun pentru nomenclatoare. Spațiu de lucru pe lățimea paginii și stil compact NIR; CSS-urile tuturor controalelor sunt încărcate.

Adaptoare comune pentru câmpuri, grile, arbori, meniuri, modale și preview/PDF; resursele au scope și cleanup. DataGrid a primit totaluri explicite independente de filtrare și footerAction dezactivabil. Editorul de listă comun a primit opțiuni per rând, căutare și eticheta `label` corectă. Timerul comun de sesiune întoarce cleanup; politica sesiunii nu se schimbă și nu se pornește un al doilea monitor. Nerulat/netestat.

### 2. NIR, inclusiv MF

Rescris `js/vercon/docs.js`: antet în trei coloane (furnizor/cont/comisie, factură cu valoare activă, NIR), o singură grilă DataGrid. La MF aceeași grilă se extinde cu inventar/data intrării/DNF/DF/valoare lunară. Combobox cu căutare pentru produse/TVA, DatePicker pentru date. Arbore TreeView furnizor → dată → factură/total în dreapta. Totalurile documentului sunt explicite; calculele locale sunt informative, regulile de salvare rămân pe server. Nerulat/netestat.

### 3. Bon de consum

Același modul: panou TreeView pentru produse/categorii în stânga, detalii produs și disponibil pentru liniile draftului, grilă DataGrid în centru, arbore bonuri în dreapta la consultare. Produsele din editor sunt limitate la cont. Disponibilul se cere pentru data bonului, excluzând BC editat; răspunsurile vechi sunt ignorate. Renunță păstrează documentul anterior. Nerulat/netestat.

### 4. Storno și retur

Selector sursă Combobox, cantități sursă/deja/rămas în DataGrid, prețurile sursei păstrate și DatePicker. Numai liniile cu cantitate completată intră în payload. Arborele documentelor și stările consultare/editor sunt comune. Nerulat/netestat.

### 10. Editorul de preview existent

Rescris `js/vercon/editor.js`: antet cu câmpurile comune, produse în DataGrid, footer Adaugă rând, meniu ⋮ și previzualizarea comună. Draftul extern și protocolul preview existent se păstrează. După răspuns incert, editarea este oprită până la reconciliere; conflictul păstrează ciorna și revizia originală. Renunță restaurează ultima stare acceptată. Nerulat/netestat.

### 8. Nomenclatoare și variante de preț

Rescris `js/vercon/nomenclatoare.js`: liste DataGrid pentru nomenclatoarele descrise de server și TreeView categorie → produs. Formulare comune cu Combobox pentru referințe, câmpuri compacte și draft separat; variantele urmează produsul selectat. Schimbarea produsului ține cont și de ciorna variantei. Filtrul active/inactive/toate folosește pickBox, meniurile ⋮ și confirmările sunt comune; dezactivarea păstrează documentele salvate. Salvarea, request_key și reviziile rămân în protocolul existent; conflictele păstrează datele introduse. Nerulat/netestat.

### 9. Setări gestiune

Același modul: câmpuri comune pentru zecimale/scară și DataGrid readonly pentru contoare. Editabilitatea depinde de drepturi și de absența datelor; limita 0–4 este afișată și aplicată în formular. Mesajele serverului rămân lângă câmpuri. Nerulat/netestat.

### 7. Registru MF

Rescris `js/vercon/mf.js` cu DataGrid, DatePicker în celulă și meniul ⋮. Modificarea datei/DNF/DF se păstrează într-o ciornă pentru mijlocul fix selectat, apoi se salvează împreună cu revizia originală. Identitatea, valoarea, deținătorii și casările sunt readonly; conflictul nu aruncă ciorna. Căutare din header și preview comune. Nerulat/netestat.

### 6. Lista de alimente și alegere BC

Rescris `js/vercon/lista.js`: trei panouri de mese alăturate, fiecare cu un singur rând P/VMD/V/D în DataGrid și grila de feluri. Porțiile felurilor reflectă masa; câmpul Tip existent rămâne separat. Antet cu DatePicker, totaluri BC/listă/diferență, footer Adaugă rând și meniuri comune. Popup modal cu TreeView primitor → dată → BC, selecție urmată de Adaugă BC; eliminarea scoate doar legătura din ciornă. Arbore liste an → lună → dată → număr/valoare ascuns în editor. Salvare atomică, revizie păstrată, cerere POST identică la răspuns incert, preview și reasonBox comune. Nerulat/netestat.

### 5. Predare, transfer și casare

Predare: cont de intrare → primitor/cont primitor și produse din cont. Transfer/casare: Combobox produse din loturile eligibile ale predătorului, cu protecție împotriva dublării aceluiași lot în draft. Casarea nu are primitor. Fără panouri Perioada/Deținători; arborele familiei în dreapta în consultare. Schimbarea predătorului cere confirmarea eliminării liniilor existente. Footer Adaugă rând, total și meniul ⋮ comune. Anularea folosește reasonBox; istoricul și previzualizarea ciornei/documentului folosesc dialogul și DataGrid comune. Payloadurile, reviziile și cheile de creare păstrate; nicio schemă schimbată. Nerulat/netestat.

## 14.10.2026 — valori uniforme DECIMAL(20,4), reset demo și reimport

Reguli, fișiere și limite istorice: [revizia migratorului](docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md#revizii-numerice-și-de-copiere--14102026).

## 14.10.2026 — VRC copiază valorile sursei fără recalculări

Reguli, fișiere și limite istorice: [revizia migratorului](docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md#revizii-numerice-și-de-copiere--14102026).

## 14.10.2026 — popup HTML pentru rezultatele VRC.Migrator

Reguli, fișiere și limite istorice: [revizia migratorului](docs/worklog/SLICE-VC05-09-rezultat-colorat-si-ordinea-butoanelor.md#popup-html-al-raportului--14102026).

## 15.10.2026 — selectoarele din antetul VERCON

Detalii: [istoric consolidat](docs/worklog/SLICE-VC02-01-alegerea-contextului.md#d001-13-context), D001-13.

## 15.10.2026 — aspectul și deschiderea meniului comun

Aspect, hover și succesiunea 180 → 320 → 420 ms: [contractul Menu](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--dimensiuni-animații-și-evoluție-15102026). Stare istorică: surse modificate, nerulat.

### Corecție — dropdown pe toată lățimea butonului și dezvăluire vizibilă

Lățimea butonului, textul pe rânduri și dezvăluirea în două cadre: [Menu](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--dimensiuni-animații-și-evoluție-15102026). Stare istorică: surse modificate, nerulat.

### Corecție — retragere laterală configurabilă și pornirea tranziției

Retragerea configurabilă, cauza tranziției și excepția reduced-motion: [Menu](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--dimensiuni-animații-și-evoluție-15102026). Stare istorică: surse modificate, nerulat.

### Lățime minimă a dropdown-ului configurabilă pentru fiecare buton

API dropdownMinWidth și minima inițială Configurări: [Menu](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--dimensiuni-animații-și-evoluție-15102026). Stare istorică: surse modificate, nerulat.

### Configurări mai îngust și deplasare între dropdown-uri

Minima Configurări 224 px și transferul geometriei în 280 ms: [Menu](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--dimensiuni-animații-și-evoluție-15102026). Stare istorică: surse modificate, nerulat.

### Meniu INTRARI / IESIRI pentru alimente și materiale

Detalii: [gruparea istorică](docs/worklog/SLICE-VC02-02-meniuri-capabilitati-coloane.md#d001-13-meniu), consolidată prin D001-13.

### NIR — editare în pagină și fișe în locul arborelui de furnizori

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#fișe-nir-și-editare-în-pagină--15102026).

### Fișe NIR — spațiul de sus recuperat și afișare cu două zecimale

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#fișe-nir-și-editare-în-pagină--15102026).

### Fișe NIR — eliminarea notelor de subsol

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#fișe-nir-și-editare-în-pagină--15102026).

### Fișe NIR — etichetă total și cele mai noi înregistrări primele

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#fișe-nir-și-editare-în-pagină--15102026).

### Fișe NIR — totalurile în benzile albastre de jos

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#fișe-nir-și-editare-în-pagină--15102026).

### TreeView comun — evidențiere de la începutul vizibil al nodului

Comportament, evoluție și limite: [detalii](VERCON/CONTROALE_CUSTOM_THREAD.md#treeview).

### 2026-10-15 — Nomenclatoare în DGV, Baza de date, Comisii și feluri din TIP

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Produse: grilă pe 70%, istoric prețuri și arbore de fișe de magazie

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Produse: prețuri numai din NIR, produse folosite și selector CPV lazy

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Selecție până la marginea arborelui și CPV în ModalFrame

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Caption comun ModalFrame și descrierea CPV curent

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Filtrare DGV comună și adăugare produse pe rând

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Aspect buton Adaugă produs

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Componentă comună ActionAddButton pentru Produse și NIR

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Denumire specifică ActionAddButton

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Buton NIR doar cu icon

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Padding condiționat pentru titlul arborelui

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Corecție latură padding titlu cu buton

Evoluție și limite: [nomenclatoare, 15.10](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#evoluția-nomenclatoarelor--15102026-preluată-prin-d001-05).

### 2026-10-15 — Bandă colorată uniformă în submeniuri

Contract și fișiere: [banda de 30 px](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu-popup--bandă-compactă-pentru-iconuri-15102026). Stare istorică: surse modificate, nerulat.

### 2026-10-15 — Butonul de meniu deschis continuă chenarul popupului

Joncțiune, condiții și fișiere: [chenarul comun](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--chenar-comun-pentru-butonul-deschis-și-popup-15102026). Stare istorică: surse modificate, nerulat.

### 2026-10-15 — Umbră dropdown doar jos și dreapta

Valori și fișier CSS: [umbra comună](VERCON/CONTROALE_CUSTOM_THREAD.md#meniu--chenar-comun-pentru-butonul-deschis-și-popup-15102026). Stare istorică: surse modificate, nerulat.

### 2026-10-16 — Factură comună, validări NIR și corecții automate nemodificabile

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — Separarea clasificărilor MF de schema facturilor

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — Sursă unică pentru schema MariaDB și infrastructură comună de import

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — Calcul invers NIR identificat prin existența metadatelor

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — Grupare în SLICE-VC06-06 și pregătire staging

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — SQL manual pentru repetarea migrării pe 000_DEMO

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — VC06-06, decizii pentru NIR cu mai multe conturi

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — VC06-06, implementarea editorului NIR pe mai multe conturi

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — ComboTree în DGV: căutare suprapusă și selecție NIR

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — Pagină locală de probă pentru DGV + ComboTree

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — ComboTree: tastatură și focus automat în Cantitate

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — ComboTree: corecții după proba utilizatorului în browser

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — DGV: aspectul și formatul celulei păstrate la editare

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — DGV: eliminarea conturului negru de focus

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — NIR: ValoareFC și butonul de corecție în antet

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — NIR: rânduri corectate, antet și bară de acțiuni

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — NIR: Stoc inițial în titlu și butoane cu lățimi egale

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — VERCON: efect unitar pentru butoane

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — ComboTree: Tab din căutare către controlul următor

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — NIR: acțiuni integrate, selecție furnizor și total corectat

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — VERCON: meniuri fără conturul comun la hover

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-17 — NIR: factură cumulată și păstrarea secțiunii la schimbarea gestiunii

Comportament, evoluție și limite: [detalii](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#factură-cumulată-și-schimbarea-secțiunii--17102026).

### 2026-10-16 — NIR: Comisie sub Categorie și salvare după editarea celulei

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-16 — NIR: lista Comisie la lățimea casetei

Detaliile și starea etapei sunt în [VC06-06 — istoric din jurnalul general](docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026).

### 2026-10-17 — Sesiune: avertizări custom, ieșire fără confirmare și probă locală

Detalii: [revizia 6](docs/worklog/SLICE-VC01-03-sesiune-log-conexiuni.md#d001-12-sesiune), consolidate prin D001-12.

### 2026-10-17 — Previzualizare: resurse de raport omise la publicare

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — Previzualizare: aliasuri în sursele SQL ale rapoartelor

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — Previzualizare PDF pe întreg spațiul disponibil

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — Macheta NIR după referința vizuală a utilizatorului

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — Corecție orientare NIR: A4 portret și zoom inițial 100%

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — FCS folosește exact macheta NIR

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — Cantitate și valori negative la afișarea FCS

Contractul rezultat: [Resursele rapoartelor](PYTHON/reports/vercon/README.md). Diagnosticul, variantele înlocuite, fișierele și limitele etapei: [istoric VC08-03](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#istoric-rapoarte-din-17102026--absorbit-prin-d001-04).

### 2026-10-17 — FCS: formular în ecranul normal și stoc la data documentului

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-04-br-si-fcs.md#fcs-în-pagină-și-numerotare-comună--17102026).

### 2026-10-17 — Sesiune web configurabilă, reînnoită la refresh

Detalii: [revizia 7](docs/worklog/SLICE-VC01-03-sesiune-log-conexiuni.md#d001-12-sesiune), consolidate prin D001-12.

### 2026-10-17 — FCS fără selecții repetate, cache mini fișe și eliminare Listare NIR

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-04-br-si-fcs.md#fcs-în-pagină-și-numerotare-comună--17102026).

### 2026-10-17 — Cache mini fișe de magazie configurabil în config.py

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-04-br-si-fcs.md#fcs-în-pagină-și-numerotare-comună--17102026).

### 2026-10-17 — FCS fără comparație între valoarea din antet și totalul liniilor

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-04-br-si-fcs.md#fcs-în-pagină-și-numerotare-comună--17102026).

### 2026-10-17 — Bonuri de consum: arbore, editare în pagină și mini fișe

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#editor-bc-și-mini-fișe--17102026).

### 2026-10-17 — BC: disponibil consecvent, fonturi și spațiul mini fișelor

Reguli, fișiere, corecții și limite: [istoricul editorului](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#editor-bc-și-mini-fișe--17102026).

### 2026-10-17 — Lista de alimente: antet, mese și arbore

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: derulare orizontală și bara de editare

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: totaluri compacte și tipografie

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: Număr și Data mai înguste

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: economii și depășiri anterioare

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: recapitulare pe cinci coloane

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: recapitularea în interiorul antetului

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Lista de alimente: editori FELUL și Explicații

Evoluție, reguli, fișiere și limite: [Lista de alimente, 17.10](docs/worklog/SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026).

### 2026-10-17 — Raport Lista de alimente: antet și recapitulare

Machetă, fișiere, proveniență și probe distincte: [Lista de alimente, 17.10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).

### 2026-10-17 — Server local VERCON cu date demonstrative

Machetă, fișiere, proveniență și probe distincte: [Lista de alimente, 17.10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).

### 2026-10-17 — Raport Lista de alimente: tabel compact și semnături la bază

Machetă, fișiere, proveniență și probe distincte: [Lista de alimente, 17.10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).

### 2026-10-17 — Raport Lista de alimente: grilă pentru pix și delimitări antet

Machetă, fișiere, proveniență și probe distincte: [Lista de alimente, 17.10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).

### 2026-10-17 — Raport Lista de alimente: linii uniforme și footer în prima secțiune

Machetă, fișiere, proveniență și probe distincte: [Lista de alimente, 17.10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).

### 2026-10-17 — Rapoarte: antet VERCON cu meniu, document și dată

Detalii: [reviziile rapoartelor, etapa 1](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-17 — NIR, BC, retur și FCS: ordonare alfabetică în arbore

Detalii: [revizia 8](VERCON/CONTROALE_CUSTOM_THREAD.md#d001-12-arbori), consolidate prin D001-12.

### 2026-10-17 — Obiecte de inventar: conturi multiple, BP/BT/BS și rapoarte

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md).

### 2026-10-18 — Rapoarte: ComboTree perioadă, limite DI/DSF și ramuri inițiale

Detalii: [reviziile rapoartelor, etapa 2](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-18 — Rapoarte: arbore tipuri mai îngust cu 40%

Detalii: [reviziile rapoartelor, etapa 3](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-18 — Rapoarte: Selectare date închis la rădăcini și încărcare lazy

Detalii: [reviziile rapoartelor, etapa 4](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-18 — OI: corecție alias SQL la listarea BP/BT/BS

Detalii: [revizia 9](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-12-diagnostic-oi), consolidate prin D001-12.

### 2026-10-18 — VC06-04: retururi si linii storno migrate fara sursa

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](docs/worklog/SLICE-VC06-04-br-si-fcs.md).

### 2026-10-18 — OI: versiunea listei ca text și jurnalizare după serializare

Detalii: [revizia 10](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-12-diagnostic-oi), consolidate prin D001-12.

### 2026-10-18 — Meniu: selecție progresivă Subunitate / Gestiune / An

Detalii, fișiere și limite de probă: [etapa 1](docs/worklog/SLICE-VC02-01-alegerea-contextului.md#d001-09-etapa-1), consolidate prin D001-09.

### 2026-10-18 — Meniu comun și Fluxuri / Mișcări pentru OI/MF

Detalii, fișiere și limite de probă: [etapa 2](docs/worklog/SLICE-VC02-01-alegerea-contextului.md#d001-09-etapa-2), consolidate prin D001-09.

### 2026-10-18 — Bonuri OI/MF: arbore direct și conturi BP

Detalii, fișiere și limite de probă: [etapa 3](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-3), consolidate prin D001-09.

### 2026-10-18 — Produse MF și clasificări din catalogul comun

Detalii, fișiere și limite de probă: [etapa 4](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#d001-09-etapa-4), consolidate prin D001-09.

### 2026-10-18 — Produse OI: fișa traseului și deținerea curentă

Detalii, fișiere și limite de probă: [etapa 5](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#d001-09-etapa-5), consolidate prin D001-09.

### 2026-10-18 — Baza de date: Feluri numai la Alimente și TIP controlat

Detalii, fișiere și limite de probă: [etapa 6](docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#d001-09-etapa-6), consolidate prin D001-09.

### 2026-10-18 — Intrări: antet rearanjat și consultare/editare MF

Detalii, fișiere și limite de probă: [etapa 7](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-7), consolidate prin D001-09.

### 2026-10-18 — Intrări: conturi MAGAZIE, produse noi în DGV și reguli MF

Detalii, fișiere și limite de probă: [etapa 8](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-8), consolidate prin D001-09.

### 2026-10-18 — Combouri: denumirea selecției unice

Detalii, fișiere și limite de probă: [etapa 9](VERCON/CONTROALE_CUSTOM_THREAD.md#d001-09-etapa-9), consolidate prin D001-09.

### 2026-10-18 — Intrări MF: DGV fără Cantitate și Cotă TVA

Detalii, fișiere și limite de probă: [etapa 10](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-10), consolidate prin D001-09.

### 2026-10-18 — MF: mesaj DNF cu clasificare și limite; status centrat

Detalii, fișiere și limite de probă: [etapa 11](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-11), consolidate prin D001-09.

### 2026-10-18 — Clasificări MF: selecție numai pentru elemente cu DNF

Detalii, fișiere și limite de probă: [etapa 12](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-12), consolidate prin D001-09.

### 2026-10-18 — DGV: ștergerea erorii după validarea/anularea rândului

Detalii, fișiere și limite de probă: [etapa 13](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-13), consolidate prin D001-09.

### 2026-10-18 — Adăugare intrări MF: padding antet DGV de 2px

Detalii, fișiere și limite de probă: [etapa 14](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-14), consolidate prin D001-09.

### 2026-10-18 — Modificare NIR MF: numai active fără ieșire; alinieri fișă MF

Detalii, fișiere și limite de probă: [etapa 15](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-09-etapa-15), consolidate prin D001-09.

### 2026-10-18 — Bonuri MF/OI: grupare opțională după dată

Detalii, fișiere și limite de probă: [etapa 16](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-16), consolidate prin D001-09.

### 2026-10-18 — Bonuri MF/OI: număr implicit separat P/T/C și produse cu disponibil pozitiv

Detalii, fișiere și limite de probă: [etapa 17](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-17), consolidate prin D001-09.

### 2026-10-18 — Identitate comună P/T/C și semantica registrului OI/MF

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Alinierea completă a aplicației la registrul OI/MF

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Bonuri PTC: editare ca la BC și mini-fișe TC

Detalii, fișiere și limite de probă: [etapa 18](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-18), consolidate prin D001-09.

### 2026-10-18 — Migrator: valoare compactă pentru VC_Imports.ToolVersion

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Documentarea contractului OI/MF și a corecției ToolVersion

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Bon transfer: Primitor și Predător diferiți

Detalii, fișiere și limite de probă: [etapa 19](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-19), consolidate prin D001-09.

### 2026-10-18 — Mini-fișele transfer/casare în dreapta

Detalii, fișiere și limite de probă: [etapa 20](docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md#d001-09-etapa-20), consolidate prin D001-09.

### 2026-10-18 — Diagnostic disponibil BT pentru produs predat și casat

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Cauza confirmată a disponibilului BP 27 / BS 5

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-18 — Cont OI/MF moștenit din BP până la casare, în aplicație și migrator

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-19 — Amortizare în arborele Rapoarte / Mijloace fixe

Detalii: [reviziile rapoartelor, etapa 5](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Migrator: recuperare univocă IDBO după refacerea unui bon

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/REGISTRU_OI_MF.md).

### 2026-10-19 — Migrator: IntrariStorno fără NIR storno părinte, ignorat NON BLOCKING

Regulile, evoluția, fișierele și limitele acestei etape: [documentul tematic](VERCON/VRC.Migrator/README.md).

### 2026-10-19 — ME01-03: drepturi reale MariaDB din KBotAdmin

Istoricul, fișierele și starea consemnată: [ME01-03](docs/worklog/SLICE-ME01-03-privilegii-mariadb.md).

### 2026-10-19 — Balanță: produse alfabetic, pagină pe cont și filtrare fără mișcări

Detalii: [reviziile rapoartelor, etapa 6](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Rapoarte: eliminarea zonei de mesaje de sub selecție

Detalii: [reviziile rapoartelor, etapa 7](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Intrări: ajustare NIR salvat pe o unitate rămasă în magazie

Detalii: [revizia 11](VERCON/FACTURI_NIR_IMPLEMENTARE.md#d001-12-ajustare), consolidate prin D001-12.

### 2026-10-19 — Intrări: buton de ajustare activ la diferențe, cu explicații

Detalii: [revizia 12](VERCON/FACTURI_NIR_IMPLEMENTARE.md#d001-12-ajustare), consolidate prin D001-12.

### 2026-10-19 — Zoom comun în bara de meniu VERCON / ADECHIT

Istoricul, fișierele și starea consemnată: [VC08-10](docs/worklog/SLICE-VC08-10-zoom-meniu-vercon-adechit.md).

### 2026-10-19 — NIR: valoare factură introdusă din numpad pe tastatură română

Istoricul, fișierele și starea consemnată: [VC08-06](docs/worklog/SLICE-VC08-06-nir-valoare-factura-numpad.md).

### 2026-10-19 — Toate rapoartele: antet unitate/subunitate, fără DC

Istoricul, fișierele și starea consemnată: [VC08-07](docs/worklog/SLICE-VC08-07-antet-comun-rapoarte-fara-dc.md).

### 2026-10-19 — Meniu VERCON: ADECHIT ca referință vizuală

Istoricul, fișierele și starea consemnată: [VC08-08](docs/worklog/SLICE-VC08-08-meniu-aspect-adechit.md).

### 2026-10-19 — Intrări/Ieșiri: grupare în două niveluri și lazy fără redraw global

Istoricul, fișierele și starea consemnată: [VC08-09](docs/worklog/SLICE-VC08-09-arbori-documente-doua-niveluri-lazy.md).

### 2026-10-19 — Aspectul bifelor din TreeView / ComboTree

Casetele sunt centrate vertical pe rând, cu bordură de 1px. Semnul bifat
folosește un SVG centrat, în locul bordurilor rotite cu poziții fixe;
și semnul stării parțiale este centrat vertical.
Fișier: PYTHON/static/css/treeview/treeview_checkbox.css (control comun).
Stare: **Confirmat 23.10**.

### 2026-10-19 — TreeView fără efect de apăsare pe rând

Eliminată regula `.treeview-item:active` care micșora rândul la 98% și
adăuga umbră interioară la click. Se păstrează stilurile de selecție.
Fișiere: PYTHON/static/css/treeview/treeview_items.css;
JS_COMPONENTS/css/treeview/treeview_items.css; implementare_vercon_v2.md.
Stare: **Confirmat 23.10**.

### 2026-10-19 — Butoanele rapoartelor în antet

Detalii: [revizia 13](docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md#d001-12-antet-export), consolidate prin D001-12.

### 2026-10-19 — Tipografie uniformă în antetul rapoartelor

Titlul, comenzile și indicatorii de pagină/zoom folosesc aceeași familie
și mărime, din variabilele existente --font-ui și --fs-button.
Titlul nu mai preia mărimea mai mare a titlurilor de dialog.
Fișiere: PYTHON/static/css/report-preview.css; implementare_vercon_v2.md.
Stare: **Confirmat 23.10**.

### 2026-10-19 — Acțiuni cu emoticoane și meniu Export Raport

Detalii: [reviziile rapoartelor, etapa 8](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Aliniere și aspect meniu Export Raport

Detalii: [revizia 14](docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md#d001-12-antet-export), consolidate prin D001-12.

### 2026-10-19 — Total general integrat în tabel, comun tuturor rapoartelor

Detalii: [reviziile rapoartelor, etapa 9](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Aspect Total general și eliminare pentru un singur grup

Detalii: [reviziile rapoartelor, etapa 10](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Păstrarea selecțiilor între rapoarte din aceeași categorie

Detalii: [reviziile rapoartelor, etapa 11](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Ordine rapoarte și selectarea documentelor pe furnizor

Detalii: [reviziile rapoartelor, etapa 12](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Reset complet la comutarea Afișează Documente

Detalii: [reviziile rapoartelor, etapa 13](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Corecție mod exclusiv Documente, cu încărcare lazy

Detalii: [reviziile rapoartelor, etapa 14](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Valoare estimată în referate rotunjită în sus la zeci de lei

Detalii: [reviziile rapoartelor, etapa 15](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Magazie / Intrări: selecție pe furnizor și NIR cu stornări

Detalii: [reviziile rapoartelor, etapa 16](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Subtotaluri integrate și denumiri rapoarte furnizori

Detalii: [reviziile rapoartelor, etapa 17](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Arbore rapoarte cu majuscule și selecție Ieșiri

Detalii: [reviziile rapoartelor, etapa 18](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-19 — Lățimi coloane și păstrarea arborelui pe categoria de nivel 1

Detalii: [reviziile rapoartelor, etapa 19](docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte), consolidate prin D001-11.

### 2026-10-20 — Simplificare meniu rapoarte Mijloace Fixe

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Arbori Selectare date specifici rapoartelor MF

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — MF/OI fără selector de an și perioade de raportare până azi

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Ani descrescători și expandare după ultimul an cu date

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Corecție compatibilitate expandare an curent

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — BC: număr propus, produse cu stoc și sumar produs

Detalii: [revizia 15](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#d001-12-bc), consolidate prin D001-12.

### 2026-10-20 — ComboTree: căutarea păstrează descendenții produsului găsit

Detalii: [revizia 16](VERCON/CONTROALE_CUSTOM_THREAD.md#d001-12-arbori), consolidate prin D001-12.

### 2026-10-20 — BC asociat unei liste: editare și recalculare; ordine inversă cronologică

Detalii: [revizia 17](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#d001-12-bc), consolidate prin D001-12.

### 2026-10-20 — BC: primitor unic selectat automat

Detalii: [revizia 18](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#d001-12-bc), consolidate prin D001-12.

### 2026-10-20 — VERCON: schimbarea perioadei direct peste raport

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Perioade rapoarte: luni descrescătoare până la ultima mișcare

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Dropdown perioadă egal cu lățimea casetei combo

Reguli, fișiere și confirmarea istorică: [MF și perioade, 20.10](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026).

### 2026-10-20 — Tooltip comun configurabil pentru ADECHIT, K-BOT și VERCON

Contractul, evoluția celor trei etape, fișierele și confirmarea operatorului: [Tooltip comun](docs/web-tooltip.md#istoric-și-proveniență).

### 2026-10-20 — Tooltipuri din title; proba butoanelor RAPOARTE și oprire pe mobil

Contractul, evoluția celor trei etape, fișierele și confirmarea operatorului: [Tooltip comun](docs/web-tooltip.md#istoric-și-proveniență).

### 2026-10-20 — Tooltip compact, antet opțional și oprire pe nodurile Selectare date

Contractul, evoluția celor trei etape, fișierele și confirmarea operatorului: [Tooltip comun](docs/web-tooltip.md#istoric-și-proveniență).

### 2026-10-20 — UTC pentru momentele tehnice ale serverului AVACONT

Salvările și anulările VERCON generate în Python folosesc acum utc_now(),
independent de fusul sistemului. Conexiunile MariaDB ale aplicației impun UTC
pe sesiune, astfel încât NOW(), valorile implicite și expirările comparate în
SQL folosesc aceeași convenție. Aceeași regulă se aplică serverului FOREXE,
e-Factura, jurnalelor, administrării și scripturilor auxiliare. Procesul web
și comenzile administratorului folosesc UTC și pentru biblioteci/loguri.

Data documentului, anul/perioada și implicitul „azi” rămân date calendaristice
românești, prin bucharest_today(). Chatul păstrează salvarea UTC și afișarea
existentă în browser. Istoricul și valorile importate nu sunt deplasate automat.

Fișiere VERCON: routes/vercon/doc_routes.py, documents.py, lista.py,
migration_links.py, nomenclatoare.py, oi.py, oi_context.py, receipt_products.py,
receipts.py, report_service.py (toate sub PYTHON); comun: PYTHON/utils/clock.py
(nou), database.py, logger.py, timing.py, asociere_log.py, main.py,
gunicorn.conf.py. Politica și publicarea: PYTHON/docs/TIMEZONES.md (nou).
Adaptate și modulele serverului pentru expirări FOREXE, semnături PDF,
metadate/loguri/copii, conexiunile directe și datele implicite de activitate.
Testul existent al semnăturilor PDF a fost adaptat conversiei UTC explicite.

Stare: **Confirmat 23.10**.

### 2026-10-21 — VC09-01: Ajutor VERCON după modelul ADECHIT și chat în ambele ghiduri

Istoricul, fișierele și starea consemnată: [VC09-01](docs/worklog/SLICE-VC09-01-ajutor-vercon-si-chat.md).

### 2026-10-21 — VC09-02: Capturi anonimizate în help, CPV/clasificări MF și timer

Istoricul, fișierele și starea consemnată: [VC09-02](docs/worklog/SLICE-VC09-02-capturi-ajutor.md).

### 2026-10-21 — VC09-03: Simplificare Gestionari, Rapoarte și totaluri în copaci

Istoricul, fișierele și starea consemnată: [VC09-03](docs/worklog/SLICE-VC09-03-simplificare-gestionari-rapoarte-copaci.md).

### 2026-10-21 — VC09-04: Help și capturi după publicarea interfeței

Istoricul, fișierele și starea consemnată: [VC09-04](docs/worklog/SLICE-VC09-04-help-dupa-publicarea-interfetei.md).

### 2026-10-21 — VC06-07 / VC06-08: NIR din facturile primite prin e-Factura

Istoricul, fișierele și starea consemnată: [VC06-07](docs/worklog/SLICE-VC06-07-nir-din-efactura-lista-si-completare.md); [VC06-08](docs/worklog/SLICE-VC06-08-nir-din-efactura-inchiderea-trimiterii.md).

### 2026-10-21 — Portal: conectare cu certificat lângă Continuă

Detalii: [revizia 19](docs/worklog/SLICE-0110-04-portal-certificat.md#d001-12-certificat), consolidate prin D001-12.

### 2026-10-21 — SLICE-VC09-05: Reglare registru OI (VERCON) și produs completat la migrare

Istoricul, fișierele și starea consemnată: [VC09-05](docs/worklog/SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md).

### 2026-10-23 — VC08-11: Rapoarte OI: două niveluri și selecție ca la MF

Istoricul, fișierele și starea consemnată: [VC08-11](docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md).

### 2026-10-23 — VC08-12: Antet comun bonuri P/T/C și Consum

Istoricul, fișierele și starea consemnată: [VC08-12](docs/worklog/SLICE-VC08-12-antet-comun-bonuri.md).

### 2026-10-23 — VC08-13: Corecții balanță OI, bonuri și selecție pe primitor

Istoricul, fișierele și starea consemnată: [VC08-13](docs/worklog/SLICE-VC08-13-corectii-balanta-oi-bonuri-primitori.md).

### 2026-10-23 — VC08-14: HTTP 500 la generare: parametrii cererii suprascriși

Istoricul, fișierele și starea consemnată: [VC08-14](docs/worklog/SLICE-VC08-14-generare-http-500-parametri.md).

### 2026-10-23 — VC07-06: Numărul copiilor în orice listă din ultima lună

Istoricul, fișierele și starea consemnată: [VC07-06](docs/worklog/SLICE-VC07-06-copii-liste-ultima-luna.md).

### 2026-10-23 — VC06-09: BC din luna activă, stoc protejat și istoric cu motiv

Istoricul, fișierele și starea consemnată: [VC06-09](docs/worklog/SLICE-VC06-09-bc-luna-activa-stoc-si-istoric.md).

### 2026-10-23 — VC07-07: Lista Alimente: ordinea și alinierea butoanelor

Istoricul, fișierele și starea consemnată: [VC07-07](docs/worklog/SLICE-VC07-07-lista-butoane-actiuni.md).

### 2026-10-23 — VC08-15 / VC06-10: Tooltipuri arbori și salvarea comisiilor la ieșirea de pe rând

Istoricul, fișierele și starea consemnată: [VC08-15](docs/worklog/SLICE-VC08-15-tooltip-arbori.md); [VC06-10](docs/worklog/SLICE-VC06-10-comisii-salvare-rand.md).

### 2026-10-23 — VC08-16: Ordonare după tip și evaluarea prețurilor istorice

Istoricul, fișierele și starea consemnată: [VC08-16](docs/worklog/SLICE-VC08-16-ordonare-tipuri-si-preturi-istorice.md).

### 2026-10-23 — VC08-17: Fișa de magazie: selecție multiplă și pagini separate

Istoricul, fișierele și starea consemnată: [VC08-17](docs/worklog/SLICE-VC08-17-fisa-magazie-selectie-multipla.md).

### 2026-10-23 — VC06-11: Asociere NIR manual cu eFactura, parțial și drag and drop

Istoricul, fișierele și starea consemnată: [VC06-11](docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md).

### 2026-10-23 — VC06-12: NIR: asociere în popup și grupare inițială pe furnizori

Istoricul, fișierele și starea consemnată: [VC06-12](docs/worklog/SLICE-VC06-12-nir-popup-asociere-si-grupare-furnizori.md).

### 2026-10-23 — VC06-13 / VC07-08: Asociere eFactura în DGV, cantități automate și listă compactă

Istoricul, fișierele și starea consemnată: [VC06-13](docs/worklog/SLICE-VC06-13-efactura-dgv-cantitati-automate.md); [VC07-08](docs/worklog/SLICE-VC07-08-lista-alimente-fara-mesaj-permanent.md).

### 2026-10-23 — VC06-14: eFactura: DGV-uri sursă și memorare implicită manuală

Istoricul, fișierele și starea consemnată: [VC06-14](docs/worklog/SLICE-VC06-14-efactura-grile-surse-drop-si-memorare-implicita.md).

### 2026-10-23 — VC08-18: status între acțiuni, elipsis și tooltip custom

Istoricul, fișierele și starea consemnată: [VC08-18](docs/worklog/SLICE-VC08-18-status-intre-actiuni.md).

### 2026-10-23 — VC06-15: facturi eFactura inactive global și informare în gestiune

Istoricul, fișierele și starea consemnată: [VC06-15](docs/worklog/SLICE-VC06-15-efactura-informare-inactive.md).

### 2026-10-23 — eFactura: mesajul de stare între butoanele footerului

Detalii: [revizia 20](docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md#d001-12-efactura), consolidate prin D001-12.

### 2026-10-23 — VC06-16: eFactura: scroll corectat, NIR protejat și afișare factură

Istoricul, fișierele și starea consemnată: [VC06-16](docs/worklog/SLICE-VC06-16-efactura-scroll-arbore-blocare-si-pdf.md).

### 2026-10-23 — VC06-17: eFactura: ComboBox custom și încărcare automată

Istoricul, fișierele și starea consemnată: [VC06-17](docs/worklog/SLICE-VC06-17-efactura-combo-custom-incarcare-automata.md).

### 2026-10-23 — Luni în arbori: ordonare calendaristică explicită

Detalii: [revizia 21](VERCON/CONTROALE_CUSTOM_THREAD.md#d001-12-arbori), consolidate prin D001-12.

### 2026-10-23 — eFactura: suprascrierea CPV-ului produsului la salvarea asocierilor

Detalii: [revizia 22](docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md#d001-12-efactura), consolidate prin D001-12.

### 2026-10-23 — VC08-19: mesaje în dialogul comun; eliminare vc-message

Istoricul, fișierele și starea consemnată: [VC08-19](docs/worklog/SLICE-VC08-19-mesaje-dialog-comun.md).

### 2026-10-23 — eFactura: diagnostic furnizor/dată și propuneri KG/KGM

Detalii: [revizia 23](docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md#d001-12-efactura), consolidate prin D001-12.

### 2026-10-23 — eFactura: furnizor fiscal al NIR-ului și listă strict filtrată

Detalii: [revizia 24](docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md#d001-12-efactura), consolidate prin D001-12.

### 2026-10-23 — VC06-18: VERCON ONLINE: o singură identitate fiscală a furnizorului

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC06-18](docs/worklog/SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md).

### 2026-10-23 — VC03-07: VERCON: migrare fiscală din KBotAdmin / Interogări unice

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC03-07](docs/worklog/SLICE-VC03-07-migrare-fiscala-kbotadmin-interogari-unice.md).

### 2026-10-23 — VC03-08: diagnostic NIR_FARA_CF după rularea utilizatorului

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC03-08](docs/worklog/SLICE-VC03-08-diagnostic-nir-fara-cf.md).

### 2026-10-23 — VC03-09: migrare VERCON CF v2: documentele istorice fără CF nu blochează baza

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC03-09](docs/worklog/SLICE-VC03-09-migrare-cf-v2-documente-istorice-fara-cf.md).

### 2026-10-23 — VC03-10: migrare CF v3: ignoră partenerii Ascuns=1

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC03-10](docs/worklog/SLICE-VC03-10-migrare-cf-v3-parteneri-ascunsi.md).

### 2026-10-23 — VC05-11: VRC.Migrator: aceeași identitate fiscală ca VERCON online

Detaliile implementării, fișierele, ordinea aplicării și starea consemnată sunt păstrate în [worklogul VC05-11](docs/worklog/SLICE-VC05-11-vrc-migrator-identitate-fiscala.md).

## 23.10.2026 — text care urmează lățimea ferestrei (panou de test)

Detalii: [istoric consolidat](VERCON/TIPOGRAFIE_MIGRARE.md#d001-13-tipografie), D001-13.

## 23.10.2026 — antet NIR / BC: etichetele lângă control când gridul are sub 10 rânduri

Detalii: [revizia 26](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-12-antet-nir), consolidate prin D001-12.

## 23.10.2026 — antet NIR pe o linie: Cotă TVA deasupra, Furnizor și Cont egale

Detalii: [revizia 27](docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md#d001-12-antet-nir), consolidate prin D001-12.

## 23.10.2026 — antet BC pe trei rânduri

Detalii: [revizia 28](docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md#d001-12-bc), consolidate prin D001-12.

## 23.10.2026 — Rapoarte: butoanele de jos prea sus

Grila paginii Rapoarte avea patru rânduri declarate (minmax(0,1fr) auto auto auto); cu doar panouri și butoane, ultimele două rânduri goale își adăugau fiecare câte 12px de spațiu sub butoane. Acum un singur rând fix pentru panouri și rânduri automate dedesubt (grid-auto-rows). Fișier: css/vercon-workspace.css. Neîncercat în browser; ipoteza vine din CSS, nu dintr-o măsurătoare.

Rapoarte: .vc-reports__actions are acum și padding-bottom: 10px (css/vercon-rapoarte.css).
