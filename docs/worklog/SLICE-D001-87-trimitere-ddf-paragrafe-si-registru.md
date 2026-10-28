# SLICE-D001-87…98 — Istoricul curățării documentației (26.10.2026)

Reunire documentară D001-99. Toate loturile de mai jos au data 26.10.2026 și privesc numai documentația aplicată local. Rezultatele, inventarele, bilanțurile/cumulurile și continuările istorice sunt păstrate pe lot, cu ancore proprii; căile din inventare sunt proveniență.

Declarația comună Test results / unverified: numai lectură și comparație documentară/Git; fără teste, builduri, verificatoare, aplicații, comenzi operaționale, SQL, commit sau push. Confirmările istorice nu sunt extinse. AVACONT, claude*.md, documentația testelor, ajutorul runtime, sursele, exporturile și release-urile sunt excluse. Stage numai pentru Markdown propriu; fragmentele din indexul comun sunt delimitate mai jos.

Inventarele indică rânduri eliminate (în forma compactă, numărul dintre paranteze); minus înseamnă rânduri adăugate. Continuarea curentă este în registrul D000–D009.

Rândurile comune ale inventarelor sunt centralizate mai jos (SLICE-D003-12); valorile sunt istorice, cu aceeași convenție de semn ca inventarele. Celelalte căi rămân la fiecare lot.

| Loturi | `docs/worklog/state/KBOT_STATUS_D000-D009.md` | `docs/worklog/KBOT_STATUS.md` |
|---|---:|---:|
| D001-87, D001-88, D001-89, D001-90, D001-91, D001-92, D001-93, D001-94, D001-95, D001-97, D001-98 | -1 | 0 |
| D001-96 | -1 | 67 |

Stage istoric în indexul comun: numai rândul D001 pentru loturile 87…95; numai fragmente proprii pentru 96…98.

## SLICE-D001-87 — Trimiterea DDF: istoric compact și registru fără note identice <a id="slice-d001-87"></a>

### What changed and why

Istoricul 0081 și registrul 0080–0089 au fost citite integral. Continuările de paragraf/listă sunt reunite fără schimbarea cuvintelor, tabelelor, ancorelor sau ruperilor explicite. Nota identică din cele 13 rânduri de registru 0081 este enunțată o singură dată, lângă tabel; toate stările și linkurile pe etapă rămân. Se păstrează derivarea StareTrimitere, fluxul de reluare, semnăturile/PDF, distincția probă–reîncărcare, formulele și toate limitele SQL/publicare/Adobe/FOREXE. Contradicția istorică din finalul 12 rămâne delimitată de runda a treia; nu se deduce confirmarea live.

### Files touched

Inventar: `docs/worklog/SLICE-0081-01-starea-reviziei.md` (343); `docs/worklog/state/KBOT_STATUS_0080-0089.md` (-2); docs/worklog/SLICE-D001-87-trimitere-ddf-paragrafe-si-registru.md (-25).

Bilanț: **5 căi; un fișier adăugat; 315 rânduri mai puțin**. Cumul D001-01…87: **290 fișiere și 12709 rânduri mai puțin**. Continuare istorică D001-88.

## SLICE-D001-88 — Editorii ORD și DDF: istorice distincte, paragrafe compacte <a id="slice-d001-88"></a>

### What changed and why

Ambele istorice au fost citite integral. Continuările de paragrafe și liste sunt reunite fără schimbarea cuvintelor, ancorelor, citatelor, tabelelor, blocurilor de cod sau ruperilor explicite de rând. ORD păstrează cele trei runde BIC/comun-doar-citire/beneficiar unic și numărul probabil; DDF păstrează distinct rezervarea numerelor, inversarea cheilor, Desc_Lunga_ANSI, faza a doua a atașamentelor, probele rich-text și corecțiile 03. Rezultatele și neconcordanțele istorice nu sunt transformate în confirmări ale stării actuale; nu s-a deschis documentație din Tests.

### Files touched

Inventar: `docs/worklog/SLICE-0049-ord-edit-form.md` (137); `docs/worklog/SLICE-0051-ddf-edit-form.md` (71); docs/worklog/SLICE-D001-88-editoare-ord-ddf-istorice-compacte.md (-25).

Bilanț: **5 căi; un fișier adăugat; 182 rânduri mai puțin**. Cumul D001-01…88: **289 fișiere și 12891 rânduri mai puțin**. Continuare istorică D001-89.

## SLICE-D001-89 — Înregistrarea și aprobarea: cronologie păstrată în paragrafe compacte <a id="slice-d001-89"></a>

### What changed and why

Istoricul 0075-01…05 a fost citit integral și compactat prin reunirea continuărilor, fără schimbarea cuvintelor, tabelelor, codului, ancorelor sau citatelor. Se păstrează ordinea efectivă 01–02–04–03–05, hash-urile/TTL-urile, regulile F/E și D27…D29, compensările, cele două eșecuri ale pasului 7 și a treia aprobare reușită. Mențiunile inițiale «nimic viu» rămân la reperul lor, delimitate de confirmările operatorului din 23.09; confirmarea parolei/LoginForm/drepturilor nu este extinsă la toate anulările. Pagina operatorului rămâne documentată istoric, înainte de ME01. Nicio operație descrisă nu a fost executată.

### Files touched

Inventar: `docs/worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md` (161); docs/worklog/SLICE-D001-89-inregistrare-aprobare-istoric-compact.md (-24).

Bilanț: **4 căi; un fișier adăugat; 136 rânduri mai puțin**. Cumul D001-01…89: **288 fișiere și 13027 rânduri mai puțin**. Continuare istorică D001-90.

## SLICE-D001-90 — Ingestie: premise compacte și completarea integrată în istoricul 03 <a id="slice-d001-90"></a>

### What changed and why

Cele patru jurnale 0048-01, 02, 03 și completarea 03 au fost citite integral. Premisele și alegerea unității rămân separate; continuările se reunesc fără schimbarea cuvintelor, codului, tabelelor sau citatelor. Completarea corectează aceeași tranșă 03 și este integrată după ea, cu numerotarea proprie și ancore explicite completare- pentru secțiuni. Trimiterile din plan și registru indică noile ancore; lista istorică de fișiere din D001-04 rămâne proveniență. Sunt păstrate propunerea cu rollback, amprenta, 409-urile distincte, oprirea inițială și activarea ulterioară a pasului 8, retragerea F20/O3, D-N/D-O, F28 și limitele probelor. Niciun pas istoric, SQL sau test nu a fost executat.

### Files touched

Inventar: `docs/worklog/SLICE-0048-01-premise-si-ajutoare.md` (85); `docs/worklog/SLICE-0048-02-alegerea-unitatii.md` (114); `docs/worklog/SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md` (-233); `docs/worklog/SLICE-0048-03-completare.md` (512); `docs/PLAN_ForexeIngest.md` (0); `docs/worklog/state/KBOT_STATUS_0040-0049.md` (0); docs/worklog/SLICE-D001-90-ingestie-premise-alegere-si-completare.md (-29).

Bilanț: **9 căi; 1 fișiere retrase și un jurnal adăugat; 448 rânduri mai puțin**. Cumul D001-01…90: **288 fișiere și 13475 rânduri mai puțin**. Continuare istorică D001-91.

## SLICE-D001-91 — Asociere: editor, grafice și registru fără reluări <a id="slice-d001-91"></a>

### What changed and why

Istoricele editorului și graficelor au fost citite integral; continuările sunt reunite fără schimbarea cuvintelor, tabelelor, codului, citatelor sau ancorelor. Descrierile tehnice și restanțele 0048-01/02/04 repetate în registru devin trimiteri la jurnale, păstrând stările de etapă. Reperele unice (6/12 cifre, frecvența ambiguității, numărătoarea istorică neconcordantă și îngustarea blocajului din 18.09) sunt transferate explicit. Corecția din 20.09, retragerea F13, F30 nereparat, formulele totalului și probele statice 08/09 rămân distincte de mouse/designer/SQL neprobate. Nicio afirmație istorică despre regula «de azi» nu este extinsă la starea actuală.

### Files touched

Inventar: `docs/worklog/SLICE-0048-01-premise-si-ajutoare.md` (-4); `docs/worklog/SLICE-0048-02-alegerea-unitatii.md` (-4); `docs/worklog/SLICE-0048-04-asociere-oricand.md` (130); `docs/worklog/SLICE-0048-05-graficul-evolutiei.md` (349); `docs/worklog/state/KBOT_STATUS_0040-0049.md` (84); docs/worklog/SLICE-D001-91-asociere-grafice-si-registru.md (-28).

Bilanț: **8 căi; 0 fișiere retrase și un jurnal adăugat; 526 rânduri mai puțin**. Cumul D001-01…91: **287 fișiere și 14001 rânduri mai puțin**. Continuare istorică D001-92.

## SLICE-D001-92 — Refacerea recepțiilor și extrase: istorice compacte, probe delimitate <a id="slice-d001-92"></a>

### What changed and why

Ambele istorice au fost citite integral; paragrafele și continuările listelor sunt reunite fără schimbarea cuvintelor, ancorelor, citatelor, tabelelor sau codului. Recepțiile păstrează succesiunea refacerii după IDH, F31/F32, recalculării DIF și ancorei F33/F34; liniile zero și antetele goale nu sunt confundate. Extrasele păstrează separat proba live doar_noi și importul SNM nerulat, numărătoarea 7→10→11 a perechilor, cele trei răspunsuri despre IdClsfV și limitele RectificariV/gUnitati. Istoria cheii admin nu înlocuiește planul de securitate ulterior. Nicio comandă consemnată nu a fost executată.

### Files touched

Inventar: `docs/worklog/SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md` (151); `docs/worklog/SLICE-0057-forexe-lista-oprire-extrase.md` (125); docs/worklog/SLICE-D001-92-refacere-receptii-si-extrase-compactare.md (-25).

Bilanț: **5 căi; 0 fișiere retrase și un jurnal adăugat; 250 rânduri mai puțin**. Cumul D001-01…92: **286 fișiere și 14251 rânduri mai puțin**. Continuare istorică D001-93.

## SLICE-D001-93 — Istoricul curățării: loturile 07–30 într-un jurnal <a id="slice-d001-93"></a>

### What changed and why

Cele 24 de jurnale D001-07…30 au fost citite integral și reunite în fișierul 07 existent. Fiecare lot își păstrează titlul, data, rezultatul, inventarul complet, bilanțul/cumulul și continuarea istorică. Cele două declarații identice de probe/limite ale D001-18…30 și legenda tabelelor sunt centralizate cu domeniul explicit; declarațiile și excepțiile distincte ale 07…17 rămân. Registrul trimite la ancora fiecărui lot. Căile din tabelele istorice rămân dovezi ale intervențiilor, nu destinații curente. Nu sunt modificate documentele tehnice sau limitele adoptate de loturile originale; stările locale de atunci nu certifică starea Git actuală.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-D001-07-harti-istorice-efactura.md` | -746 |
| `docs/worklog/SLICE-D001-08-jurnale-controale-desktop.md` | 47 |
| `docs/worklog/SLICE-D001-09-context-receptii-mf-si-bonuri.md` | 37 |
| `docs/worklog/SLICE-D001-10-jurnale-adobe-si-dataview.md` | 44 |
| `docs/worklog/SLICE-D001-11-selectii-si-machete-rapoarte.md` | 35 |
| `docs/worklog/SLICE-D001-12-sesiuni-editori-si-efactura.md` | 41 |
| `docs/worklog/SLICE-D001-13-controale-si-tipografie.md` | 38 |
| `docs/worklog/SLICE-D001-14-gazda-adobe-si-arbore-desktop.md` | 36 |
| `docs/worklog/SLICE-D001-15-jurnale-migrator.md` | 41 |
| `docs/worklog/SLICE-D001-16-jurnale-setari.md` | 32 |
| `docs/worklog/SLICE-D001-17-jurnale-recorder.md` | 32 |
| `docs/worklog/SLICE-D001-18-jurnale-asociere-parteneri.md` | 32 |
| `docs/worklog/SLICE-D001-19-jurnale-plati-si-ddf.md` | 38 |
| `docs/worklog/SLICE-D001-20-istoric-editor-ordonantare.md` | 31 |
| `docs/worklog/SLICE-D001-21-istoric-trimitere-ddf.md` | 41 |
| `docs/worklog/SLICE-D001-22-istoric-inregistrare-si-aprobare.md` | 35 |
| `docs/worklog/SLICE-D001-23-plan-inregistrare-fara-reluari.md` | 28 |
| `docs/worklog/SLICE-D001-24-descarcari-unelte-si-bugete.md` | 40 |
| `docs/worklog/SLICE-D001-25-actualizare-extrase-si-jurnale.md` | 37 |
| `docs/worklog/SLICE-D001-26-sumar-receptii-istoric.md` | 37 |
| `docs/worklog/SLICE-D001-27-editor-ddf-si-corectii-ingestie.md` | 33 |
| `docs/worklog/SLICE-D001-28-nucleu-si-vizualizator-jurnale.md` | 31 |
| `docs/worklog/SLICE-D001-29-note-de-versiune-si-restante-istorice.md` | 33 |
| `docs/worklog/SLICE-D001-30-istoric-copii-locale-me02.md` | 33 |
| docs/worklog/SLICE-D001-93-istoric-curatare-loturile-07-30.md | -47 |

Bilanț: **27 căi; 23 fișiere retrase și un jurnal adăugat; 38 rânduri mai puțin**. Cumul D001-01…93: **308 fișiere și 14289 rânduri mai puțin**. Continuare istorică D001-94.

## SLICE-D001-94 — Istoricul curățării: loturile 31–60 într-un jurnal <a id="slice-d001-94"></a>

### What changed and why

Cele 30 de jurnale D001-31…60 au fost citite integral și reunite în fișierul 31 existent. Declarațiile identice despre probe, limite și legenda tabelelor sunt centralizate cu domeniul explicit. Titlurile, datele 25/26.10, rezultatele, inventarele și numărătorile de etapă rămân integrale; fiecare lot are ancoră proprie în registru. Mențiunile despre fișiere de release, SQL sau ajutor sunt dovezi ale lucrului istoric; acele artefacte nu sunt modificate acum. Confirmările și excepțiile nu sunt recertificate.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-D001-31-istoric-dataview-si-setari-adobe.md` | -799 |
| `docs/worklog/SLICE-D001-32-retragere-vba-si-exceptia-operatorului.md` | 29 |
| `docs/worklog/SLICE-D001-33-istoric-asistent-ajutor.md` | 35 |
| `docs/worklog/SLICE-D001-34-plan-ingestie-si-mapare-istorica.md` | 29 |
| `docs/worklog/SLICE-D001-35-istoric-tururi-capturi-si-export.md` | 38 |
| `docs/worklog/SLICE-D001-36-reguli-receptii-fara-explicatii-dublate.md` | 28 |
| `docs/worklog/SLICE-D001-37-receptii-refacere-totaluri-si-ancore.md` | 33 |
| `docs/worklog/SLICE-D001-38-desktop-dpi-umbra-si-login.md` | 33 |
| `docs/worklog/SLICE-D001-39-browser-gazde-urmarire-si-marcaje.md` | 34 |
| `docs/worklog/SLICE-D001-40-diagnostic-ingestie-si-jurnal-asociere.md` | 33 |
| `docs/worklog/SLICE-D001-41-reconstituire-refresh-si-snm.md` | 35 |
| `docs/worklog/SLICE-D001-42-plan-si-fundament-trimitere-ddf.md` | 29 |
| `docs/worklog/SLICE-D001-43-combo-si-istorice-desktop-cab.md` | 33 |
| `docs/worklog/SLICE-D001-44-paragrafe-continue-in-planuri.md` | 29 |
| `docs/worklog/SLICE-D001-45-bugete-versiuni-si-actualizare-in-coada.md` | 34 |
| `docs/worklog/SLICE-D001-46-istoric-bugete-arbore-si-access.md` | 33 |
| `docs/worklog/SLICE-D001-47-corecturi-autentificare-si-perioade.md` | 36 |
| `docs/worklog/SLICE-D001-48-coada-extrase-si-parteneri-ddf.md` | 34 |
| `docs/worklog/SLICE-D001-49-istoric-ajutor-bugete.md` | 34 |
| `docs/worklog/SLICE-D001-50-ribbon-control-grupuri-si-comenzi.md` | 50 |
| `docs/worklog/SLICE-D001-51-pagini-modale-si-corectii-efactura.md` | 39 |
| `docs/worklog/SLICE-D001-52-ajutor-ribbon-pagini-si-ferestre.md` | 39 |
| `docs/worklog/SLICE-D001-53-panou-lateral-si-machete-auxiliare.md` | 41 |
| `docs/worklog/SLICE-D001-54-pagina-publica-si-autentificare-portal.md` | 33 |
| `docs/worklog/SLICE-D001-55-portal-vizualizare-si-administrare.md` | 36 |
| `docs/worklog/SLICE-D001-56-furnizori-angajamente-dupa-codfiscal.md` | 35 |
| `docs/worklog/SLICE-D001-57-identitate-furnizor-si-conversie-coduri.md` | 34 |
| `docs/worklog/SLICE-D001-58-tema-scalare-si-controale.md` | 35 |
| `docs/worklog/SLICE-D001-59-pdf-migrare-si-poarta-parintelui.md` | 33 |
| `docs/worklog/SLICE-D001-60-efactura-model-import-si-token.md` | 34 |
| docs/worklog/SLICE-D001-94-istoric-curatare-loturile-31-60.md | -53 |

Bilanț: **33 căi; 29 fișiere retrase și un jurnal adăugat; 145 rânduri mai puțin**. Cumul D001-01…94: **336 fișiere și 14434 rânduri mai puțin**. Continuare istorică D001-95.

## SLICE-D001-95 — Istoricul curățării: loturile 61–86 într-un jurnal <a id="slice-d001-95"></a>

### What changed and why

Cele 26 de jurnale D001-61…86 au fost citite integral și reunite în fișierul 61 existent. Data comună 26.10.2026, starea locală și declarațiile identice de probe/limite sunt enunțate o singură dată. Fiecare rezultat, inventar, bilanț/cumul și continuare rămâne integral sub titlul și ancora lotului. Registrul indică secțiunile. Cumulul original D001-86 rămâne dovadă a predării; această absorbție nu îl rescrie și nu schimbă starea publicării sau probării funcționalităților.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-D001-61-efactura-server-emisie-si-date-unitate.md` | -718 |
| `docs/worklog/SLICE-D001-62-efactura-primite-server-si-client.md` | 37 |
| `docs/worklog/SLICE-D001-63-efactura-primite-evolutie-si-popup.md` | 44 |
| `docs/worklog/SLICE-D001-64-ajutor-efactura-emise-si-primite.md` | 50 |
| `docs/worklog/SLICE-D001-65-efactura-lista-access-si-trimitere-vercon.md` | 43 |
| `docs/worklog/SLICE-D001-66-ajutor-efactura-lista-si-vercon.md` | 39 |
| `docs/worklog/SLICE-D001-67-receptii-model-clasificatii-legare-si-robot.md` | 41 |
| `docs/worklog/SLICE-D001-68-receptii-pagina-propuneri-si-checkpointuri.md` | 41 |
| `docs/worklog/SLICE-D001-69-ajutor-receptii-pagina-si-propuneri.md` | 42 |
| `docs/worklog/SLICE-D001-70-reparare-rezervari-program-si-ajutor.md` | 38 |
| `docs/worklog/SLICE-D001-71-adobe-istorice-compactare-paragrafe.md` | 29 |
| `docs/worklog/SLICE-D001-72-designer-navlist-si-arbore-compactare.md` | 29 |
| `docs/worklog/SLICE-D001-73-migrare-http-si-directa-compactare.md` | 29 |
| `docs/worklog/SLICE-D001-74-recorder-gazde-browser-si-urmarire.md` | 31 |
| `docs/worklog/SLICE-D001-75-dataview-contracte-si-filtrare-compactare.md` | 29 |
| `docs/worklog/SLICE-D001-76-adechit-migrator-istoric-si-plan.md` | 45 |
| `docs/worklog/SLICE-D001-77-adechit-cataloage-prezenta-si-taxe.md` | 53 |
| `docs/worklog/SLICE-D001-78-adechit-ciclu-incasari-si-rapoarte.md` | 47 |
| `docs/worklog/SLICE-D001-79-portal-parinti.md` | 34 |
| `docs/worklog/SLICE-D001-80-pagina-preview-editare.md` | 37 |
| `docs/worklog/SLICE-D001-81-subunitati-schema-api-predare.md` | 35 |
| `docs/worklog/SLICE-D001-82-kbotadmin-interfata-si-stari.md` | 35 |
| `docs/worklog/SLICE-D001-83-adechit-plan-si-continuari.md` | 32 |
| `docs/worklog/SLICE-D001-84-adechit-analiza-si-decizii.md` | 38 |
| `docs/worklog/SLICE-D001-85-adechit-contracte-si-inventar.md` | 32 |
| `docs/worklog/SLICE-D001-86-copii-locale-istorice-compacte.md` | 31 |
| docs/worklog/SLICE-D001-95-istoric-curatare-loturile-61-86.md | -49 |

Bilanț: **29 căi; 25 fișiere retrase și un jurnal adăugat; 173 rânduri mai puțin**. Cumul D001-01…95: **360 fișiere și 14607 rânduri mai puțin**. Continuare istorică D001-96.

## SLICE-D001-96 — Indexul general: reguli comune în paragrafe compacte <a id="slice-d001-96"></a>

### What changed and why

Indexul general a fost citit integral în afara rândului AV exclus. Regulile de citire/întreținere, notele transversale și deciziile blocate sunt compactate prin reunirea continuărilor de paragraf/listă, fără schimbarea cuvintelor, ancorelor sau tabelelor. Numerele, rândurile individuale, stările, găurile 0038/0039 și reperele datate rămân; nu se grupează identificatorii indexului, pentru a păstra forma folosită de workflow. Rândurile cu modificări independente 00EF, AV și 0123 rămân nestagiate. Nu sunt recertificate regulile istorice despre server sau SQL.

### Files touched

Inventar: docs/worklog/SLICE-D001-96-index-general-paragrafe-compacte.md (-24).

Bilanț: **3 căi; 0 fișiere retrase și un jurnal adăugat; 42 rânduri mai puțin**. Cumul D001-01…96: **359 fișiere și 14649 rânduri mai puțin**. Continuare istorică D001-97.

## SLICE-D001-97 — Adobe găzduit: paragrafe compacte în istoricul 0078 <a id="slice-d001-97"></a>

### What changed and why

Paragrafele și continuările listelor din istoricul ferestrei Adobe găzduite au fost reunite fără schimbarea cuvintelor, titlurilor, ancorelor, tabelelor sau ordinii. Rămân distincte bancul simulat și rulările Adobe reale, alegerile A/B, răspunsul inițial Da și corecția ulterioară Nu, ipotezele despre procesele fantomă, monitorizarea pe Acrobat vechi și limitele confirmărilor de build/help.

### Files touched

Inventar: `docs/worklog/SLICE-0078-05-fereastra-gazduita-ctrl-h.md` (299); docs/worklog/SLICE-D001-97-adobe-gazduit-istoric-paragrafe-compacte.md (-24).

Bilanț: **4 căi; 0 fișiere retrase și un jurnal adăugat; 274 rânduri mai puțin**. Cumul D001-01…97: **358 fișiere și 14923 rânduri mai puțin**. Continuare istorică D001-98.

## SLICE-D001-98 — Vederea DDF: etape istorice în paragrafe compacte <a id="slice-d001-98"></a>

### What changed and why

Paragrafele și continuările listelor din istoricul vederii DDF sunt reunite fără schimbarea cuvintelor. Se păstrează separat cele cinci etape, revizuirea operatorului din 24.07.2026, formulele SUM/COALESCE și floor, defectul mostrei DDF identice cu ORD, alegerile namespace/UTF-8 și riscurile lor, numărătorile de teste și limitele offline/live/PDF. Titlurile, ancorele, tabelele și ordinea rămân intacte.

### Files touched

Inventar: `docs/worklog/SLICE-0020-01-ddf-endpoint.md` (197); docs/worklog/SLICE-D001-98-vedere-ddf-etape-paragrafe-compacte.md (-24).

Bilanț: **4 căi; 0 fișiere retrase și un jurnal adăugat; 172 rânduri mai puțin**. Cumul D001-01…98: **357 fișiere și 15095 rânduri mai puțin**. Continuare istorică D001-99.
