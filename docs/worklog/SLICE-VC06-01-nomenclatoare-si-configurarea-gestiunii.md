# SLICE-VC06-01 — nomenclatoare si configurarea gestiunii

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`); **nimic rulat**, nicio proba, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («toate odata»).

## Ce s-a facut si de ce
- **Provider real** (`routes/vercon/provider.py`, `SqlProvider`): pana acum productia folosea `EmptyProvider`, deci nicio gestiune nu aparea. Arborele subunitate -> gestiune -> ani vine acum din `VC_Subunitati` / `VC_Gestiuni` / `VC_Exercitii` (doar active; `Zec`/`Scara` incluse). Pe un DC fara tabelele `VC_*` raspunsul ramane «in pregatire» (eroarea 1146 -> arbore gol). `set_type` real, doar cat `Populated=0`.
- **Nomenclatoare** (`nomenclatoare.py`, tabel declarativ): categorii/conturi (`VC_Masini`), produse (`VC_Produse`) cu variante de pret (`VC_ProduseP`), clienti/gestionari (`VC_Clienti`; «Gestionari» din ribbon deschide Clienti, nu exista alt tabel), cote TVA (`VC_TVA`), comisie receptie / inventariere. Furnizorii raman in `Parteneri` (partajat cu K-BOT, fara editare aici). `FeluriMancare`/`Retetar` raman la VC07-01.
- **Reguli comune:** randul apartine unei singure gestiuni (`ScopedRepository`); nimic nu se sterge, doar inactiveaza; revizie `Version` la fiecare scriere (revizie veche = CONFLICT, nu suprascriere); `RequestKey` la creare (`VC_Operations`, raspuns reluat, aceeasi cheie cu alt corp refuzata); rand de audit (`VC_Audit`) in aceeasi tranzactie, CNP mascat; validari pe server (lungimi, obligatoriu, TVA 0-100 %, pret >= 0 pe INT/scara gestiunii, refuz — nu rotunjire — la prea multe zecimale). Primul pret scris seteaza `Populated=1` (inghiata tipul si scara).
- **Rute** (`nom_routes.py`): `GET/POST /api/vercon/nom/<kind>`, `PUT .../<id>`, `POST .../<id>/active`, `GET/POST /api/vercon/settings` (zecimale + scara, doar cat gestiunea nu are date; contoarele din `VC_Settings` doar afisate). Scrierile trec prin `scoped` (context re-verificat, tip, revizia contextului, jetonul curent de ocupare) si o singura tranzactie.
- **Meniu:** formulare noi `tva`, `comisie_nir`, `comisie_inventar` (drept `nomenclatoare`); raspunsul `forms/<f>/open` poarta `nom` (nomenclatoarele formularului).
- **Pagina** (`js/vercon/nomenclatoare.js`, `app.js`, `api.js`, `vercon.css`): lista/arbore + formular, butoane Adauga / Modifica / Renunta / Salveaza in footer (ca in schita Access `F_Produse` si in mockupurile NIR/BC: grila compacta, borduri subtiri); produsele au arbore categorie -> produs, variantele de pret apar sub produsul ales. Modificarile nesalvate cer confirmare la schimbarea randului/contextului/inchiderea paginii.

## Fisiere
`PYTHON/routes/vercon/`: `provider.py`, `nomenclatoare.py`, `nom_routes.py` (noi); `__init__.py`, `capabilities.py`, `repository.py` (`update(..., bump_version)`). `PYTHON/static/`: `js/vercon/nomenclatoare.js` (nou), `app.js`, `api.js`, `css/vercon.css`.

## Neverificat / amanat
- Nimic rulat: nici rutele, nici SQL-ul pe `000_DEMO`, nici pagina pe ecran. In previzualizarea locala (`VERCON/tools/preview.py`) nomenclatoarele raspund 503 (nu are baza de date).
- Presupuneri de verificat: `Version`/`Activ` exista pe toate tabelele folosite (DDL citit, nu si serverul); `VC_Audit.UserEmail` primeste e-mailul in clar (e in baza, nu in logger); nu exista verificare de unicitate pe denumire (istoricul migrat poate repeta nume).
- Neacoperit: ajutorul VERCON (nescris, ca la VC02-05), probele din `PROBE_BROWSER`, ecranul de furnizori, importul/exportul nomenclatoarelor, afisarea pretului la `Zec` (se afiseaza la scara de stocare).
- Mockupurile din `2026-10-12-corectate-v2` nu contin ecrane de nomenclatoare; layout-ul urmeaza regulile lor generale si schita `F_Produse`.

## Evoluția nomenclatoarelor — 15.10.2026, preluată prin D001-05

Etapele de mai jos sunt ulterioare implementării din 12.10 de mai sus: surse modificate, fără teste, compilări, browser, importuri, SQL sau alte verificări executate. Aspectul, rezultatele SQL și performanța nu au fost confirmate prin rulare. Modelele/schemele numite în listele istorice sunt cele de atunci; structura de import a fost revizuită ulterior în VC06-06. Stocarea INT/scară din introducerea de la 12.10 a fost înlocuită cu DECIMAL la 14.10, conform [contractului numeric](../../VERCON/CONTRACT_CALCULE.md).

### Grile, Baza de date, Comisii și TIP

- Formularul lateral este înlocuit de DGV editabile, rând gol, salvare pe celulă/rând complet și ⋮ pentru activare/dezactivare/reîncercare. Produse/variante au inițial două grile părinte–copil. Ciornele incomplete și salvările neconfirmate sunt păstrate separat; creare cu RequestKey, actualizare cu Version.
- Cele cinci intrări Configurări devin **Baza de date** (categorii stânga, TVA dreapta sus, feluri dreapta jos) și **Comisii** (recepție NIR/inventariere suprapuse pe întregul spațiu disponibil, cu limitele existente). Înălțimi relative, fără VH propriu; datele au ponderi egale, Stare/⋮ lățimi fixe, ⋮ de 36 px. `proportionalWidths` scade scrollbarul/bordurile; border-box include paddingul, iar antetul ⋮ nu impune vechiul padding de 44 px. Preferințele vechi nu reintroduc lățimi/coloane ascunse.
- Sub categorii: mini fișă valorică pentru anul selectat, SI/Intrări/Ieșiri/SF la două zecimale, prin serviciul existent de stoc și prețul curent al variantei. STOC_INITIAL din interval intră în SI; TVA nu are mini fișă.
- Dezactivarea categoriei este refuzată pentru mișcări în orice exercițiu, inclusiv produse inactive. TVA utilizat prin produse/linii VC_Intrari este protejat și când importul păstrează doar valoarea numerică TVA. Aceleași refuzuri sunt în server, indiferent de meniul clientului.
- VRC construiește VC_FeluriMancare din TIP distinct din MicDejun/Dejun/Cina pe toți anii gestiunii și completează IdFel țintă. IdFel Access nu este referință către feluri; explicațiile nu devin denumiri. TIP și Explicatie rămân pe linii. «Felul n» este normalizat, TIP neconform blochează cu X13, TIP gol rămâne fără referință. VC_IdMap/N11 și versiunea regulilor planului păstrează maparea. Lista alege TIP prin nomenclator și IdFel, fără rescrierea textelor istorice.
- `sql/VC_Feluri_din_TIP.sql` este pregătit pentru execuție manuală pe gestiunea migrată după completarea ID-urilor subunității/gestiunii; fără modificare de schemă, ștergere de nomenclatoare sau execuție automată.

### Produse, variante și fișa de magazie

- Geometrie inițială: stânga 70% din lățimea interioară, între coloane 20 px, titluri pe aceeași linie; Produse ocupă două rânduri egale, Istoric prețuri pe primul din dreapta și fișa pe al doilea. Fără VH sau măsurători JS. Ulterior grila prețurilor este eliminată complet, iar arborele/fișa ocupă toată dreapta.
- Coloane: Categorie/cont, Denumire, UM, CPV, extensiile MF și tehnice. Categorie trece de la 300 la 240 px, UM rămâne 100 px, CPV 200 px, Denumire umple restul peste minim. Stare este ulterior eliminată. Lățimile fixe nu devin ponderi; pe ecrane înguste există scroll orizontal. Filtrele Denumire/CPV/Cont folosesc denumirea categoriei inclusiv inactive; editorul oferă numai categorii active. Căutarea generală inițială pe aceste trei câmpuri este ulterior scoasă din pagină.
- Istoricul inițial include toate variantele active/inactive; rândul de preț și nodul arborelui deschid aceeași fișă. Variantele cu preț egal rămân identități separate, cu cod/ID disponibile. După eliminarea grilei, variantele din nomenclator devin readonly: create/update/active pentru `kind=variante` sunt refuzate în serviciu, dar scrierea automată a prețului prin repository-ul NIR rămâne.
- Arborele plat afișează prețul cu TVA (patru zecimale) și prima dată NIR neanulat din toți anii; fără document afișează «fără dată». Fișa: Data/Document/Cantitate/Sold și footer Stoc la două zecimale. Selecția și fundalul pornesc de la marginea părintelui: `--tv-node-start` și paddingul vertical de 4 px se aplică textului, cu poziția/înălțimea păstrate; excepția privește numai arborele plat VERCON, nu indentarea arborilor comuni.
- GET `/api/vercon/nom/produse/<id>/statement` este readonly, cu contextul serverului, exercițiul ales, warehouse și algoritmul existent de sold. Ultimele 20 de mișcări per variantă sunt descrescătoare; soldurile/Stoc includ toate mișcările eligibile și pe cele dinaintea intervalului OI/MF. Include variante fără mișcări; calculul cantitativ nu cere preț de evaluare, acceptând istoricul negativ/NULL. Schimbarea produsului ignoră răspunsurile întârziate și blochează editarea variantelor precedente în timpul încărcării.
- Implicit sunt produsele active folosite în anul selectat, calculate în server din documentele de stoc valide și, pentru OI/MF, predări/transferuri/casări din acel an. **Afișare** folosește openMenu cu «Afișează TOATE produsele» / «Afișează produse INACTIVE»; realegerea opțiunii bifate revine la folosite în an. Se păstrează selecția eligibilă sau se golește fișa produsului ascuns.

### CPV: citire lazy, alegere și modal

- Butonul «...» apare numai în celula CPV selectată. Prima pagină internă avea căutări Denumire/Cod, fonturile site-ului și Înapoi la produse, păstrând rândul/ciorna. «➕» la hover/focus este disponibil pe fiecare nod real, inclusiv superior, cu tastatură. Alegerea scrie în rândul inițial și salvează cu Version/audit; la eroare selectorul rămâne pentru reîncercare, iar la produs nou incomplet codul rămâne în draft. Un CPV nou este validat în server în AVACONT_COMUN.CPV.
- GET `/api/vercon/cpv` folosește schema CPV/Expl/L1–L7 din fotografia MariaDB_Schema/AVACONT_COMUN.sql. Identitate CPV.CPV; nivelurile/prefixele determină ramurile, inclusiv cel mai apropiat părinte existent la nivel sărit. Citește doar rădăcinile, apoi copiii direcți la expandare; TreeView reutilizează placeholder/pending/cache din NIR, fără toate cele ~10.000 de rânduri în client.
- Cache per pagină și în server 5 minute, maximum 512 intrări, fără date specifice utilizatorului. Căutare externă cu debounce 300 ms, rezultate flat, total și cel mult 200 de înregistrări reale. «Plus încă N alte înregistrări. Ajustați căutarea pentru un rezultat mai restrâns.» nu are icon/selecție/acțiune la mouse sau Enter. Rezultatele întârziate după altă căutare sunt ignorate.
- Utilizatorul confirmase deja PK CPV.CPV; nu s-a pregătit DDL și nu s-a executat SQL sau schimbare de schemă.
- Ulterior CPV trece în [ModalFrame comun](../../VERCON/CONTROALE_CUSTOM_THREAD.md#modalframe-și-unelte-de-debug), 1000×600 prin API și maxime CSS, redus pe ecrane mici, backdrop blurat, Tab în modal și Produse inactiv în fundal. Închide/Escape respectă ciclul comun și popupurile interioare; nu mai există captură Escape separată. Alegerea distruge modalul și întoarce focusul la controlul inițial; lazy/căutările rămân.
- Caption/title/showClose și cleanupul sunt ale componentei: documentele reutilizează headerul existent, CPV caption generat fără buton local Închide. La deschidere, descrierea CPV curent apare sub produs prin GET `/api/vercon/cpv/<code>`, guard/scoped și SELECT parametrizat pe CPV.CPV, fără încărcare de catalog. Stări încărcare/lipsă/eroare; răspunsurile după închidere sunt ignorate.

### Salvare pe rând și butonul comun

Regulile DataGrid sunt în [contractul comun](../../VERCON/CONTROALE_CUSTOM_THREAD.md#datagrid--filtrare-și-salvare-pe-rând-15102026), iar API/stil/cleanup în [ActionAddButton](../../PYTHON/static/js/components/action-add-button/ActionAddButton.md). Aici rămân integrarea și evoluția:

- Inițial «➕» Produse de 44×40 px, icon 24 px, colțuri rotunjite, centrare flex și stările pointer/hover/focus/disabled. Clickul izolează rândul nou și editează Denumire. Valorile rămân locale până la finalizare, inclusiv în CPV; Denumire/UM/Cont obligatorii și în server, fără CPV se poate continua numai după avertizare/alegere explicită prin showMessage/confirmBox.
- Finalizare la părăsirea rândului, Enter în ultima celulă editabilă sau ⋮ «Salvează rândul»; Escape anulează prin comportamentul DGV. După creare se recitește lista completă păstrând filtrele/sortarea/lățimile; produsul nou este selectat și adus în ecran, temporar la final dacă filtrul îl exclude, până la următoarea schimbare a filtrării.
- Componenta comună și redenumirea explicită folosesc `ActionAddButton`, directorul `static/js/components/action-add-button`, fișierele JS/CSS/MD, clase `k-action-add-button` și variabile `--action-add-button-*` în Produse/NIR, linkul CSS din vercon.html și inventar. CSS-ul local al butoanelor este eliminat; regula generică a barelor exclude componenta. Instanțele se distrug cu pagina/scope-ul arborelui; acțiunile existente rămân.
- NIR păstrează mărimile text/icon anterioare prin variabile; ulterior se elimină textul «Adaugă», rămânând ➕, tooltip/nume accesibil «Adaugă NIR», stil/acțiune. Exemplul componentei a fost actualizat.
- `.vc-doc__tree h4:has(button)` primește inițial padding 4 px sus/jos/stânga, apoi corecția explicită îl mută la **sus/jos/dreapta**, cu stânga anterioară păstrată. Titlurile fără buton nu sunt schimbate.

### Fișiere și proveniență pe etape

Etape (în ordinea inițială): 1. Nomenclatoare în DGV, Baza de date, Comisii și feluri din TIP; 2. Produse: grilă pe 70%, istoric prețuri și arbore de fișe de magazie; 3. Produse: prețuri numai din NIR, produse folosite și selector CPV lazy; 4. Selecție până la marginea arborelui și CPV în ModalFrame; 5. Caption comun ModalFrame și descrierea CPV curent; 6. Filtrare DGV comună și adăugare produse pe rând; 7. Aspect buton Adaugă produs; 8. Componentă comună ActionAddButton pentru Produse și NIR; 9. Denumire specifică ActionAddButton; 10. Buton NIR doar cu icon; 11. Padding condiționat pentru titlul arborelui; 12. Corecție latură padding titlu cu buton.

| Fișier/cale istorică | Etape |
|---|---|
| PYTHON/static/js/vercon/nomenclatoare.js | 1 |
| PYTHON/static/js/vercon/nomenclator-grid.js | 1, 2, 3, 6, 8 |
| PYTHON/static/js/vercon/app.js | 1 |
| PYTHON/static/js/vercon/lista.js | 1 |
| PYTHON/static/css/vercon-nomenclatoare.css | 1 |
| PYTHON/routes/vercon/capabilities.py | 1 |
| PYTHON/routes/vercon/nomenclatoare.py | 1, 3, 6 |
| PYTHON/routes/vercon/nomenclator_usage.py | 1 |
| PYTHON/routes/vercon/nom_routes.py | 1, 2, 3, 5 |
| PYTHON/routes/vercon/lista.py | 1 |
| VERCON/VRC.Migrator/VrcRunner.vb | 1 |
| VERCON/VRC.Migrator/VrcRunner.Dishes.vb | 1 |
| VERCON/VRC.Migrator/VrcTargetSchema.vb | 1 |
| VERCON/VRC.Migrator/target_schema.json | 1 |
| VERCON/VRC.Migrator/VrcPlan.vb | 1 |
| VERCON/VRC.Migrator/README.md | 1 |
| VERCON/tools/ddl.js (descrierea regulii meal_type; generatorul nu a fost rulat) | 1 |
| sql/VC_Feluri_din_TIP.sql | 1 |
| implementare_vercon_v2.md | 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12 |
| PYTHON/static/js/vercon/product-nomenclator.js | 2, 3, 4, 6 |
| PYTHON/static/js/vercon/product-stock.js | 2, 3 |
| PYTHON/static/css/vercon-products.css | 2, 3, 4, 6, 7, 8 |
| PYTHON/static/css/vercon.css | 2, 3 |
| PYTHON/routes/vercon/nomenclator_stock.py | 2, 3 |
| PYTHON/static/js/dgv/datagrid.js | 2, 3, 6 |
| PYTHON/static/js/dgv/engine.js | 2 |
| PYTHON/routes/vercon/cpv_catalog.py | 3, 5 |
| PYTHON/routes/vercon/nomenclator_products.py | 3 |
| PYTHON/static/js/vercon/cpv-picker.js | 3, 4, 5 |
| PYTHON/static/css/dgv.css | 3, 6 |
| PYTHON/static/js/components/treeview/treeview-render.js | 3 |
| PYTHON/static/js/components/treeview/treeview-interactions.js | 3 |
| PYTHON/static/css/vercon-cpv.css | 3, 4, 5 |
| PYTHON/static/js/components/modal-frame/modal-frame.js | 5 |
| PYTHON/static/css/modal-frame.css | 5 |
| PYTHON/static/js/vercon/docs.js | 5, 8, 10 |
| VERCON/CONTROALE_CUSTOM_THREAD.md | 5, 6, 8 |
| PYTHON/static/js/dgv/editing.js | 6 |
| PYTHON/static/js/dgv/filtering.js | 6 |
| PYTHON/static/js/components/action-add-button/action-add-button.js | 8 |
| PYTHON/static/js/components/action-add-button/action-add-button.css | 8 |
| PYTHON/static/js/components/action-add-button/ActionAddButton.md | 8, 10 |
| PYTHON/static/css/vercon-receipt-context.css | 8 |
| PYTHON/static/css/vercon-integration.css | 8 |
| PYTHON/static/vercon.html | 8 |
| PYTHON/static/css/vercon-documente-panels.css | 11, 12 |

La redenumirea ActionAddButton, lista compactă originală mai numea directorul componentei (JS/CSS/MD), vercon/docs.js, vercon/nomenclator-grid.js, css/vercon-integration.css, css/vercon-receipt-context.css, static/vercon.html și cele două documente de evidență; sunt aceleași responsabilități, nu alte fișiere noi.

## Produse, clasificări MF și traseu OI — 18.10.2026

D001-09, istoric: **IMPLEMENTAT ÎN SURSE, NERULAT**. Numai design din surse; fără teste/build/verificări automate, browser/HTTP, MariaDB/SQL/DDL, schimbări de schemă/date, migrator, deploy/commit/push. Aspectul nu a fost probat în aplicație.

Catalogul comun nu a fost interogat; coloanele sunt cele din referințele și contractele existente. Nu s-a creat sau modificat structura catalogului, iar valorile TIP vechi nu au fost migrate automat.

<a id="d001-09-etapa-4"></a>

### Produse MF și clasificări din catalogul comun

La MF, grila produselor ocupă toată lățimea disponibilă; cele două panouri din dreapta nu mai sunt montate. Celula CC are selector cu aceeași fereastră, căutare și arbore ca CPV, parametrizat pentru Clasificare MF. Catalogul este citit exclusiv din AVACONT_COMUN.VC_ClasificariMF (Cod, Denumire, DNF_MIN, DNF_MAX); intervalul de durată este prezentat în ani. Se salvează codul CC prin protocolul produselor existent, cu validarea existenței în catalog. Nu se creează și nu se modifică structura catalogului comun.

<a id="d001-09-etapa-5"></a>

### Produse OI: fișa traseului și deținerea curentă

Fișa inferioară din dreapta arată traseul variantei de produs: recepție, storno, predare, transfer între gestionari/deținători și casare, cu date, documente, cantități, cont și gestiunea curentă. Utilizatorul a clarificat că solicitarea privește gestionarii/deținătorii, nu transferuri între alte gestiuni sau subunități. Sunt folosite identitățile produsului și variatelor din context, fără asocieri după denumire.

Istoricul este multianual și păstrează toate documentele active. BC-ul generat de BP nu dublează predarea în fișă. Transferul este afișat o dată, cu sursa și destinația; soldurile curente folosesc separat registrul magaziei și cel al deținătorilor, până la data curentă. Cantitățile rămase sunt afișate în magazie și la fiecare deținător/cont. Documentele viitoare pot apărea în istoric, dar nu intră în poziția curentă.

<a id="d001-09-etapa-6"></a>

### Baza de date: Feluri numai la Alimente și TIP controlat

Feluri de mâncare este oferit numai gestiunilor ALIMENTE, inclusiv prin filtrarea nomenclatoarelor la deschiderea formularului și refuzul API-urilor directe de listare/scriere pentru celelalte tipuri. În lipsa panoului Feluri, TVA ocupă întreaga înălțime a coloanei din dreapta.

TIP este un combo comun în grila categoriilor: NULL este permis peste tot; MAGAZIE/FOLOSINȚĂ reprezintă OIM/OIF la OI și MFM/MFF la MF; ALIMENTE apare numai la Alimente; Materiale permite numai NULL. Alte valori sunt respinse la salvare pe server. Valorile vechi nu sunt modificate automat.

### Fișiere pe etapă

Căi istorice; numele fără prefix continuă directorul precedent.

| Etapă | Fișiere |
|---|---|
| 4 | PYTHON/static/js/vercon/product-nomenclator.js, cpv-picker.js, nomenclator-grid.js; PYTHON/static/css/vercon-products.css; PYTHON/routes/vercon/mf_classifications.py, nom_routes.py, nomenclatoare.py. |
| 5 | PYTHON/routes/vercon/product_movements.py, nomenclator_stock.py; PYTHON/static/js/vercon/product-stock.js; PYTHON/static/css/vercon-products.css. |
| 6 | PYTHON/routes/vercon/nomenclatoare.py, nom_routes.py, __init__.py; PYTHON/static/js/vercon/nomenclator-grid.js; PYTHON/static/css/vercon-nomenclatoare.css. |

## SLICE-VC06-10 — Salvarea comisiilor la ieșirea de pe rând <a id="slice-vc06-10"></a>

Data: 2026-10-23.

### Ce s-a schimbat

Ambele nomenclatoare, comisie_nir și comisie_inventar, activează mecanismul existent rowSave. Confirmarea unei celule păstrează modificarea în rândul local; persistarea rândului este făcută prin onRowLeave, când se trece pe alt rând sau se părăsește tabelul. Comanda explicită Salvează rândul rămâne disponibilă pentru finalizare/reîncercare. Validarea câmpurilor obligatorii este făcută la nivel de rând și folosește mesaje specifice comisiei.

Finalizarea unui rând nou permite trecerea la rândul următor din prima; blocarea navigării după adăugarea explicită rămâne specifică produselor. Helpul precizează că trecerea între celulele aceleiași comisii nu salvează.

### Stare

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Fișiere: PYTHON/static/js/vercon/nomenclator-grid.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md și acest jurnal.
