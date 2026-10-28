# SLICE-D001-07…30 — Istoricul curățării documentației din 25.10.2026

Reunire documentară D001-93. Rezultatele, inventarele, bilanțurile și datele de mai jos rămân înregistrările loturilor originale; căile din inventarele istorice sunt proveniență, inclusiv când fișierul a fost retras ulterior. Numărătorile cumulative sunt cele consemnate atunci, fără recalcularea retroactivă a probelor.

Inventarele indică rânduri eliminate (în forma compactă, numărul dintre paranteze); minus înseamnă rânduri adăugate. Pentru D001-18…30, următoarele două declarații identice se aplică fiecărui lot și sunt centralizate aici:

- Test results: Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, ajutor runtime, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.
- Unverified / deferred: Confirmările tehnice/vizuale nu sunt extinse prin consolidare. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse.

Declarațiile și excepțiile distincte ale D001-07…17 rămân în secțiunile lor. Data comună D001-07…30: 25.10.2026; toate loturile sunt aplicate local, numai documentație. Continuările fiecărui lot sunt repere istorice; continuarea curentă este în registrul D001.

Registrele comune fac parte din inventarul și numărul de căi al fiecărui lot; rândurile lor identice sunt grupate aici, cu excepțiile numerice păstrate. Celelalte căi sunt în secțiunea lotului.

| Loturi D001 | `docs/worklog/state/KBOT_STATUS_D000-D009.md` | `docs/worklog/KBOT_STATUS.md` |
|---|---|---|
| 07…15 | -2 | 0 |
| 16 | 10 | 0 |
| 17…18 | -2 | 0 |
| 19 | 0 | 0 |
| 20…25 | -1 | 0 |
| 26 | -1 | 19 |
| 27…30 | -1 | 0 |

## SLICE-D001-07 — Hărțile istorice E-Factura <a id="slice-d001-07"></a>

Data: 25.10.2026. Consolidare documentară locală; nu modifică formulare Access, exporturile brute, SQL executabil sau generatoare.

### What changed and why

- În 21 de înregistrări de formular, convențiile de coordonate/Parent/LayoutCached și limitele analizei statice trimit la README. Prefixul identic de patru atribute al celor 20 de clase cu declarații este păstrat o dată în README; declarațiile suplimentare, evenimentele WithEvents și comentariile specifice rămân pe formular. Mesaj_ANAF nu primește declarații inventate.
- Șase valori RecordSource copiate identic în properties/Data sunt înlocuite în properties cu trimitere la SQL-ul integral din Data. SQL-urile distincte ale formularelor nu sunt unificate.
- navigation.md și matrix.md sunt absorbite în forms.md. Cele 22 de muchii ale grafului au fost comparate documentar cu Opens/Subforms din catalog și sunt aceleași. Punctele de intrare externe și interogările-parametru rămân integral.
- Matricea rară păstrează toate cele 25 de obiecte, ordinea celor 45 de coloane sursă și fiecare marker literal, inclusiv rwwW/rwW, fără normalizarea multiplicății/literelor. Celulele goale devin absența markerului, explicit definită; obiectele fără marker rămân.
- MF_PDF_EF.md și Note_PDF_EF.md intră în EFACTURA_PDF.md: proprietățile/secțiunile/controalele/declarările/harta handlerelor identice apar o dată. Pentru fiecare dintre cele trei forme se păstrează numele, lungimea exportului, calea brută, coordonatele (inclusiv negative), SQL-ul și apelantul distinct. Catalogul trimite la ancorele de formular.
- EF_F/EF_F_inlucru și cele două forme de asociere rămân distincte. README/overview trimit la noua structură; descrierea coloanelor catalogului este corectată (nu exista coloana Role).

### Files touched

Inventarul complet și bilanțul sunt mai jos.

### Test results

Numai lectură și comparație documentară sursă/destinație, inclusiv egalitatea secțiunilor comune ale celor trei fișe PDF. Fără teste, builduri, generator, aplicații, SQL sau acces MariaDB. Nu este reinterpretată precizia analizei statice. Stage selectiv al Markdown-ului propriu, fără commit/push.

### Unverified / deferred

Generatorul build-efactura.js rămâne neschimbat: regenerarea ar suprascrie consolidarea și ar recrea navigation.md/matrix.md, fișele PDF separate și boilerplate-ul. README consemnează limita; adaptarea generatorului nu este inclusă în lucrarea Markdown. Surse rămâne nemodificat. AVACONT, documentația activă AV, CLAUDE și ajutorul runtime sunt excluse.

| Cale | Rânduri eliminate (minus = adăugate) |
|---|---|
| `docs/access-map/efactura/forms/EFACTURA_2025.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_AA.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD_FACTURI.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD_SUB.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_CLIENTI.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_MSG.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_PDF.md` | -16 |
| `docs/access-map/efactura/forms/EFACTURA_PickIBAN.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_TMP.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_UM.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_VANZATOR.md` | 7 |
| `docs/access-map/efactura/forms/EF_F.md` | 4 |
| `docs/access-map/efactura/forms/EF_F_inlucru.md` | 4 |
| `docs/access-map/efactura/forms/EF_P.md` | 4 |
| `docs/access-map/efactura/forms/MF2019_Asoc_EF.md` | 4 |
| `docs/access-map/efactura/forms/MF_PDF_EF.md` | 75 |
| `docs/access-map/efactura/forms/Mesaj_ANAF.md` | 0 |
| `docs/access-map/efactura/forms/Note_EFactura.md` | 4 |
| `docs/access-map/efactura/forms/Note_PDF_EF.md` | 75 |
| `docs/access-map/efactura/forms/_MF2019_Asoc_EF.md` | 4 |
| `docs/access-map/efactura/forms.md` | -83 |
| `docs/access-map/efactura/navigation.md` | 74 |
| `docs/access-map/efactura/matrix.md` | 31 |
| `docs/access-map/efactura/README.md` | -25 |
| `docs/access-map/efactura/00-overview.md` | 0 |
| `docs/worklog/SLICE-D001-07-harti-istorice-efactura.md` | -58 |

Bilanț: **29 căi; 4 fișiere retrase, 1 jurnal nou; 3 fișiere și 151 rânduri eliminate net**. Cumul D001-01…07: **30 fișiere și 4127 rânduri mai puțin**.

## SLICE-D001-08 — Jurnale istorice ale controalelor desktop <a id="slice-d001-08"></a>

### What changed and why

- Familia 0010-01…07 este reunită în worklogul 0010-01, cu ancore distincte. Conținutul tehnic, fișierele, probele și limitele fiecărui pas sunt păstrate. Buildul identic al primelor șase etape și actualizarea indexului sunt consemnate o dată; rezultatele de test rămân specifice etapei.
- Contractul inițial IsDirty nu devine regulă curentă: 06 îl înlocuiește explicit. Distincția thumb-track/release din 07 și cele două defecte de proprietăți VB rămân. Reguli și excepții citate în istoric nu înlocuiesc AGENTS actual.
- Registrul 0010 păstrează verdictul istoric fără acceptare vizuală și trimite la detalii; acceptările din alte felii nu sunt extinse automat.

- Cele șapte fișiere 0025 sunt reunite în jurnalul Designer existent. Migrarea MainForm (pasul 7 din plan) și flyoutul 0025-07 rămân identificabile separat; scurtarea ciclului, mărimea 0, DPI/serialization, proprietatea imaginilor și limitele de test/VS sunt păstrate. Nota unică a registrului despre pașii 03/04 fără worklog este transferată, nu eliminată.

### Files touched

Inventar complet; rânduri eliminate (minus = adăugate). Corpul tehnic este păstrat aproape integral în această regrupare, astfel că reducerea de fișiere adaugă separatoare/ancore și evidență.

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0010-02-kbotdataview-virtualizare.md` | 98 |
| `docs/worklog/SLICE-0010-03-kbotdataview-tipuri-coloana.md` | 64 |
| `docs/worklog/SLICE-0010-04-kbotdataview-formatare-disable.md` | 71 |
| `docs/worklog/SLICE-0010-05-kbotdataview-input-selectie.md` | 83 |
| `docs/worklog/SLICE-0010-06-kbotdataview-editare.md` | 99 |
| `docs/worklog/SLICE-0010-07-scroll-by-column.md` | 67 |
| `docs/worklog/SLICE-0010-01-kbotdataview-skeleton.md` | -505 |
| `docs/worklog/state/KBOT_STATUS_0010-0019.md` | 6 |
| `docs/worklog/SLICE-D001-08-jurnale-controale-desktop.md` | -47 |
| `docs/worklog/SLICE-0025-02-navitem-icon.md` | 127 |
| `docs/worklog/SLICE-0025-05-navlist-collapse-padding.md` | 181 |
| `docs/worklog/SLICE-0025-06-navlist-collapse-button-icons.md` | 141 |
| `docs/worklog/SLICE-0025-07-navlist-collapsed-flyout.md` | 150 |
| `docs/worklog/SLICE-0025-08-navlist-corner-radius-gradient.md` | 129 |
| `docs/worklog/SLICE-0025-09-navlist-collapse-button-size.md` | 107 |
| `docs/worklog/SLICE-0025-designer-surfaces.md` | -875 |
| `docs/worklog/state/KBOT_STATUS_0020-0029.md` | 0 |

Bilanț: **19 căi; 12 fișiere retrase, un jurnal nou; 11 fișiere mai puțin și 106 rânduri în plus**. Cumul D001-01…08: **41 fișiere și 4021 rânduri mai puțin**.

### Test results

Numai lectură și analiză documentară. Fără teste, builduri, verificatoare, aplicații, SQL sau acces MariaDB. Probe și comenzi din worklogurile istorice nu sunt executate acum. Fără commit/push; stage numai pentru Markdown propriu.

### Unverified / deferred

Gruparea reduce fragmentarea; proba vizuală și rotunda designerului nu sunt certificate de această lucrare. AVACONT, documentația activă AV, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Familia 0025 este absorbită; probele descrise rămân istorice. Continuarea documentară este D001-09.

## SLICE-D001-09 — Context, recepții MF și bonuri OI/MF <a id="slice-d001-09"></a>

### What changed and why

Cele 20 de însemnări generale din 18.10 sunt absorbite în cinci capitole tematice existente. Corpurile tehnice sunt păstrate cu paragrafe reunite; stările repetate sunt consemnate o dată pe capitol, iar fișierele sunt într-un tabel pe etapă. Titlurile jurnalului general rămân puncte de navigare. Regruparea reduce rânduri, dar adaugă text de trasabilitate și trimiteri.

Reviziile din aceeași zi rămân cronologice: DNF vizibilă/editabilă, eliminarea coloanelor MF doar din grilă, activele cu ieșire protejate la editare, gruparea opțională a bonurilor, arbore BP vizibil/dezactivat și mini-fișele mutate ulterior în dreapta. Se păstrează inclusiv formula/rotunjirea, numerotarea per tip, diferența stocului BP/TC, identitatea loturilor, limitele clasificărilor și stările ciornelor.

Regulile inițiale diferite din 12/17.10 sunt marcate ca istorice. Nicio probă de funcționare nu este dedusă din implementarea în surse. Referința istorică la AVACONT_COMUN rămâne în contractul MF, fără accesarea documentației AVACONT.

### Files touched

Rânduri eliminate; minus înseamnă rânduri adăugate.

Inventar: `docs/worklog/SLICE-VC02-01-alegerea-contextului.md` (-31); `docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md` (-38); `docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md` (-81); `docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md` (-67); `VERCON/CONTROALE_CUSTOM_THREAD.md` (-18); `implementare_vercon_v2.md` (312); `docs/worklog/SLICE-D001-09-context-receptii-mf-si-bonuri.md` (-37).

Bilanț: **9 căi; 38 rânduri mai puțin; un jurnal nou**. Cumul D001-01…09: **40 fișiere și 4059 rânduri mai puțin**.

### Test results

Numai lectură și analiză documentară; fără teste, builduri, verificatoare, aplicații, SQL sau MariaDB. Fără commit/push; stage doar pentru Markdown propriu.

### Unverified / deferred

Rămân excluse AVACONT, documentația activă AV, CLAUDE, ajutorul runtime, codul și exporturile brute. Continuarea: D001-10.

## SLICE-D001-10 — Jurnale Adobe și DataView <a id="slice-d001-10"></a>

### What changed and why

Cele cinci jurnale 0023 sunt reunite în jurnalul RHP existent; cele nouă 0028, în jurnalul DataView existent. Corpurile tehnice/probele sunt păstrate cu titluri subordonate și ancore. Registrul 0023, cele nouă rânduri 0028 și focusul repetitiv trimit la detaliile consolidate.

Se păstrează invalidarea rulărilor Adobe, schimbarea priorității userPrefs/restaurării/bazei curate, mecanismul de mutare înlocuit și limitele probelor. La DataView se păstrează tipurile/agregatele/rotunjirea, harta vedere–model, variantele de meniu, lățimile autorate, intervențiile operatorului și lipsa verdictului vizual. Informația unică și contradicțiile din registre sunt explicitate în anexele celor două jurnale, inclusiv versiunea DevHarness și SumarView/FilterIconReserve.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0023-adobe-harness-config-layout.md` | 213 |
| `docs/worklog/SLICE-0023-adobe-harness-fixes.md` | 164 |
| `docs/worklog/SLICE-0023-adobe-harness-truthful-panel.md` | 124 |
| `docs/worklog/SLICE-0023-adobe-harness-move-window.md` | 115 |
| `docs/worklog/SLICE-0023-adobe-rhp-harness.md` | -630 |
| `docs/worklog/SLICE-0028-02-dataview-caption-icons-format.md` | 128 |
| `docs/worklog/SLICE-0028-03-dataview-column-filter-darkmode.md` | 302 |
| `docs/worklog/SLICE-0028-04-dataview-column-autosize.md` | 128 |
| `docs/worklog/SLICE-0028-05-dataview-authored-width.md` | 125 |
| `docs/worklog/SLICE-0028-06-filter-popup-designer.md` | 113 |
| `docs/worklog/SLICE-0028-07-modern-button-height.md` | 97 |
| `docs/worklog/SLICE-0028-08-filter-forms-operator-pass.md` | 111 |
| `docs/worklog/SLICE-0028-09-captionbar-theme-button.md` | 123 |
| `docs/worklog/SLICE-0028-dataview-footer-collapse-tooltip.md` | -1147 |
| `docs/worklog/state/KBOT_STATUS_0020-0029.md` | 113 |
| `docs/worklog/SLICE-D001-10-jurnale-adobe-si-dataview.md` | -44 |

Bilanț: **18 căi; 12 fișiere retrase, un jurnal nou; 33 rânduri mai puțin**. Cumul D001-01…10: **51 fișiere și 4092 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară. Fără teste, builduri, verificatoare, aplicații, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Probe/limite istorice rămân istorice; nu sunt executate acum. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-11.

## SLICE-D001-11 — Selecții și machete de rapoarte <a id="slice-d001-11"></a>

### What changed and why

20 de însemnări 17–19.10 sunt reunite în VC08-03: antet/export, perioade, încărcare lazy, bife, amortizare/balanțe, mesaje/totaluri, reutilizare/reset, catalog și valoare estimată. Fișierele sunt păstrate pe etapă, titlurile generale trimit la contractul consolidat. Etapele 0–7 nerulate/fără probe runtime și 8–19 Confirmat 23.10 rămân distincte.

Succesiunile sunt explicite: eliminarea inițială a furnizorilor față de structurile Intrări, păstrarea inițială pe categorie/structură față de regula finală de nivel 1, comutările care distrug selecția față de schimbările care o păstrează, activarea MF și perioadele revizuite ulterior pe 20.10. Reguli exacte de calcul, semne, identități, paginare și machete configurate rămân.

Nota ME01-03 despre explicația din RightsForm și incertitudinea salvării a fost scoasă din secțiunea rapoartelor și legată de contractul existent în jurnalul drepturilor; numai mențiunea unică a spațiului rezervat UI a fost adăugată acolo. ADE9-05 trimite la regulile comune de export/totaluri, fără o copie nouă.

### Files touched

Inventar: `docs/worklog/SLICE-ME01-03-privilegii-mariadb.md` (-2); `docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md` (-89); `implementare_vercon_v2.md` (342); `docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md` (-2); `docs/worklog/SLICE-D001-11-selectii-si-machete-rapoarte.md` (-35).

Bilanț: **7 căi, 212 rânduri mai puțin; un jurnal nou**. Cumul D001-01…11: **50 fișiere și 4304 rânduri mai puțin**.

### Test results

Numai lectură și analiză documentară; fără teste, builduri, verificatoare, aplicații, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Nu se extind confirmările istorice la etape nerulate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Continuare D001-12.

## SLICE-D001-12 — Sesiuni, editori și eFactura <a id="slice-d001-12"></a>

### What changed and why

22 de însemnări din 17–23.10 sunt absorbite în contractele/jurnalele existente. Fișierele și probele rămân identificate pe etapă. Proba locală a expirării sesiunii nu este extinsă la reînnoirea configurabilă ori logoutul de producție; 5052 este stare istorică, nu un proces pornit acum.

Sunt păstrate ajustarea economică NIR față de activarea explicativă a butonului, protecția activelor, stocul/ciorna BC și regulile ultimului BC/listă, variantele antetelor responsive, ordonarea/căutarea arborilor și diagnosticul alias/versiune OI. Reviziile eFactura explicită excepția CPV și rezolvarea fiscală CodPartener/Parteneri/CodFiscalSursa, fără a declara migrată toată aplicația sau investigată baza utilizatorului.

Nota despre butonul panoului de font rămâne în jurnalul general pentru lotul tipografiei; nu este pierdută la absorbția antetului BC.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-VC01-03-sesiune-log-conexiuni.md` | -20 |
| `docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md` | -14 |
| `VERCON/FACTURI_NIR_IMPLEMENTARE.md` | -17 |
| `docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md` | -21 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -16 |
| `docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md` | -11 |
| `docs/worklog/SLICE-0110-04-portal-certificat.md` | -10 |
| `docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md` | -15 |
| `docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md` | -23 |
| `implementare_vercon_v2.md` | 380 |
| `docs/worklog/SLICE-D001-12-sesiuni-editori-si-efactura.md` | -41 |

Bilanț: **13 căi; 190 rânduri mai puțin; un jurnal nou**. Cumul D001-01…12: **49 fișiere și 4494 rânduri mai puțin**.

### Test results

Numai lectură/analiză documentară. Fără teste, builduri, verificatoare, pornire/oprire aplicații, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Confirmările istorice rămân limitate la etapele lor. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Continuare D001-13.

## SLICE-D001-13 — Controale, editor și tipografie <a id="slice-d001-13"></a>

### What changed and why

Opt secțiuni generale și nota finală despre font sunt absorbite în documentele existente. API-urile TextBox/CheckBox/Combobox/TreeView/Menu/ModalFrame/debug nu mai sunt copiate în jurnalul general; clauzele unice de integrare, etapele NIR și riscurile/probele sunt păstrate separat. Introducerea rescrierii VERCON rămâne în jurnalul general.

Variantele succesive sunt explicite: roz inițial → validare la părăsire, furnizori după ultima recepție → alfabet, modal/TVA blocată față de reviziile ulterioare, Adăugare din meniu față de Intrări/consultare. Reperele CSS păstrează ordinea, dimensiunile și probele efectuate atunci.

Inventarul tipografic are 167 de intrări, 131 mapări distincte. Cele 36 de rânduri identice sunt grupate, cu toate pozițiile originale: nu se pierde nici multiplicitatea, nici ordinea reconstruibilă. Baza 14px rămâne reperul migrării, iar formula/panoul din 23.10 și scoaterea finală a butonului Aa sunt consemnate ca revizii separate.

### Files touched

Inventar: `docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md` (-25); `VERCON/CONTROALE_CUSTOM_THREAD.md` (-11); `docs/worklog/SLICE-VC02-02-meniuri-capabilitati-coloane.md` (-8); `docs/worklog/SLICE-VC02-01-alegerea-contextului.md` (-4); `docs/worklog/SLICE-VC08-01-motor-stoc-si-balante.md` (-8); `VERCON/TIPOGRAFIE_MIGRARE.md` (24); `implementare_vercon_v2.md` (106); `docs/worklog/SLICE-D001-13-controale-si-tipografie.md` (-38).

Bilanț: **10 căi, 34 rânduri mai puțin; un jurnal nou**. Cumul D001-01…13: **48 fișiere și 4528 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară. Fără teste, builduri, verificatoare, aplicații, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Probe și riscuri istorice nu sunt recertificate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-14.

## SLICE-D001-14 — Gazda Adobe și arborele desktop <a id="slice-d001-14"></a>

### What changed and why

Trei jurnale 0024 și trei 0027 sunt reunite în câte un fișier existent, cu ancore și corpuri păstrate. Notele/focusul registrului trimit la aceleași surse, păstrând verdictul istoric în coloana Status.

0024 are repere explicite pentru concluziile corectate: titlu obligatoriu/PID preferință, ascundere versus colapsare, WakeAcroLayout și probele ActiveX. Datele unice ale registrului (parametrii profilurilor, checkpointurile 693/694, dimensiunea panoului și LoadFile) sunt transferate. La 0027, acceptarea bazei nu este extinsă la subsol/corecții/vederi; înlocuirea TreeFont și implementarea gazdei rămân cronologice.

### Files touched

Inventar: `docs/worklog/SLICE-0024-02-adobe-profiles-runtime.md` (123); `docs/worklog/SLICE-0024-03-adobe-host-teardown.md` (422); `docs/worklog/SLICE-0024-01-adobe-shared-host.md` (-565); `docs/worklog/SLICE-0027-02-tree-footer-collapse.md` (373); `docs/worklog/SLICE-0027-03-tree-designer-values-in-views.md` (91); `docs/worklog/SLICE-0027-tree-designer-surface.md` (-472); `docs/worklog/state/KBOT_STATUS_0020-0029.md` (86); `docs/worklog/SLICE-D001-14-gazda-adobe-si-arbore-desktop.md` (-36).

Bilanț: **10 căi; patru fișiere retrase, un jurnal nou; 20 rânduri mai puțin**. Cumul D001-01…14: **51 fișiere și 4548 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, registry/SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Nicio concluzie despre starea actuală a Adobe ori funcționarea aplicației. Probe/reguli sunt istorice. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-15.

## SLICE-D001-15 — Jurnalele istorice ale migratorului <a id="slice-d001-15"></a>

### What changed and why

Cele patru jurnale 0044 sunt reunite în jurnalul HTTP inițial; cele șapte 0045, în jurnalul descoperirii migratorului direct. Corpurile tehnice sunt păstrate integral, cu titluri subordonate/ancore. Regruparea elimină nouă fișiere vechi, însă adaugă rânduri de trasabilitate. Registrele/focusul repetitiv trimit la aceleași surse.

Se păstrează trecerea JSON/HTTP → direct, cele două situații ale fișierelor sursă, corelațiile/NULL/cheile/ORD, ordinea, arhitectura jurnalului SQL, porțile și limitele citirii Access față de scrierea MariaDB. Istoricul creării/sincronizării schemei nu este autorizare de execuție acum. Notele unice de focus sunt transferate; existența AVACONT_COMUN, metadatele dumpului, LastInsertedId și acceptarea formularului rămân explicit datate.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0044-02-selectie-unitate-upsert-bife.md` | 132 |
| `docs/worklog/SLICE-0044-03-corelatii-coloane-grile-kbot.md` | 165 |
| `docs/worklog/SLICE-0044-04-sql-dump-coloane-obligatorii.md` | 713 |
| `docs/worklog/SLICE-0044-migrare-accdb-impins.md` | -1023 |
| `docs/worklog/SLICE-0045-02-motor-access-mariadb.md` | 152 |
| `docs/worklog/SLICE-0045-03-corelatii-si-porti.md` | 153 |
| `docs/worklog/SLICE-0045-04-scriere-si-jurnal-sql.md` | 130 |
| `docs/worklog/SLICE-0045-05-formular-si-stergerea-lantului-http.md` | 185 |
| `docs/worklog/SLICE-0045-06-schema-sync-progres-si-constatari.md` | 176 |
| `docs/worklog/SLICE-0045-07-unitatea-randului-nu-a-buclei.md` | 136 |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | -952 |
| `docs/worklog/state/KBOT_STATUS_0040-0049.md` | 8 |
| `docs/worklog/SLICE-D001-15-jurnale-migrator.md` | -41 |

Bilanț: **15 căi; nouă fișiere retrase, un jurnal nou; 68 rânduri adăugate**. Cumul D001-01…15: **59 fișiere și 4480 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară, fără teste, builduri, verificatoare, procese, registry, SQL/MariaDB sau SSH. Fără commit/push; stage doar pentru Markdown propriu. Fișierele Access/exporturile citate în istoric nu au fost deschise acum.

### Unverified / deferred

Stările istorice ale uneltelor nu sunt recertificate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Continuare D001-16.

## SLICE-D001-16 — Jurnalele ferestrei Setări <a id="slice-d001-16"></a>

### What changed and why

Patru jurnale 0072 sunt reunite în fișierul de bază, cu corpuri, fișiere și probe păstrate. Numele/notes/focus/restanțe din registru nu mai repetă cererile și contractele. Stările inițiale și mențiunea istorică «comis» sunt păstrate pe etapă, fără a pretinde operații Git actuale. Tabelul registrului D001 este refăcut continuu, eliminând separatoarele goale dintre rânduri.

### Files touched

Inventar: `docs/worklog/SLICE-0072-01-setari-jurnal-si-meniuri.md` (174); `docs/worklog/SLICE-0072-02-optiuni-avansate.md` (33); `docs/worklog/SLICE-0072-03-setari-forexe-fara-cadere.md` (21); `docs/worklog/SLICE-0072-fereastra-setari.md` (-244); `docs/worklog/state/KBOT_STATUS_0070-0079.md` (19); `docs/worklog/SLICE-D001-16-jurnale-setari.md` (-32).

Bilanț: **8 căi; trei fișiere retrase, un jurnal nou; 19 rânduri adăugate**. Cumul D001-01…16: **61 fișiere și 4461 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, parole/SQL/MariaDB ori server. Fără commit/push; stage doar pentru Markdown propriu.

### Unverified / deferred

Randările statice nu devin probe manuale, iar testele doar scrise nu devin rulate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-17.

## SLICE-D001-17 — Jurnalele recorderului <a id="slice-d001-17"></a>

### What changed and why

Patru jurnale 0053 sunt reunite în fișierul de bază, cu conținutul tehnic/probele păstrate și ancore. Registrul și restanțele nu mai repetă corpurile. Variantele de andocare, CDP/NoViewport, cablarea și limitele CABWeb sunt delimitate cronologic; randarea statică nu devine probă live.

### Files touched

Inventar: `docs/worklog/SLICE-0053-02-recorder-cablare.md` (94); `docs/worklog/SLICE-0053-03-recorder-ergonomie.md` (111); `docs/worklog/SLICE-0053-04-andocare-reala.md` (218); `docs/worklog/SLICE-0053-recorder.md` (-435); `docs/worklog/state/KBOT_STATUS_0050-0059.md` (31); `docs/worklog/SLICE-D001-17-jurnale-recorder.md` (-32).

Bilanț: **8 căi; trei fișiere retrase, un jurnal nou; 15 rânduri adăugate**. Cumul D001-01…17: **63 fișiere și 4446 rânduri mai puțin**.

### Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, browser, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

### Unverified / deferred

Stările istorice nu sunt recertificate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-18.

## SLICE-D001-18 — Jurnalele asocierii partenerilor <a id="slice-d001-18"></a>

### What changed and why

Etapele 0084-02…04 sunt reunite în 02 cu corpurile păstrate. Se păstrează lista partenerilor față de antetul DDF, adăugarea fără dubluri, popupul, mutarea în ribbon și vizibilitatea/disponibilitatea succesivă. Registrul și trimiterea 0114-02 folosesc ancorele noi; reperul unic 0000-85 este transferat. 0084 și 0084-01 nu sunt absorbite.

### Files touched

Inventar: `docs/worklog/SLICE-0084-03-asociere-parteneri-popup-modal.md` (27); `docs/worklog/SLICE-0084-04-asociere-parteneri-pe-ribbon.md` (26); `docs/worklog/SLICE-0084-02-asociere-parteneri-sumar.md` (-65); `docs/worklog/state/KBOT_STATUS_0080-0089.md` (0); `docs/worklog/SLICE-0114-02-ribbon-separator-si-butoane-fixate-conditionate.md` (0); `docs/worklog/SLICE-D001-18-jurnale-asociere-parteneri.md` (-32).

Bilanț: **8 căi; două fișiere retrase, un jurnal nou; 46 rânduri adăugate**. Cumul D001-01…18: **64 fișiere și 4400 rânduri mai puțin**. Continuare D001-19.

## SLICE-D001-19 — Jurnalele Plăți și DDF <a id="slice-d001-19"></a>

### What changed and why

0017-02…04 și 0020-01…05 sunt reunite în primele lor jurnale, cu ancore pe etapă și corpurile păstrate; paragrafele sunt reașezate. 0017-01 rămâne contract separat pentru controlul general. Reluările din două registre sunt înlocuite cu navigare; reperele unice (cablare Istoric/DDF, MD5 complet, diferențe de formulare/probe) sunt transferate. Commiturile istorice 0020 rămân în registru, iar trimiterea D001-03 este actualizată.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0017-02-plati-endpoint.md` | -190 |
| `docs/worklog/SLICE-0020-01-ddf-endpoint.md` | -386 |
| `docs/worklog/SLICE-0017-03-plati-view.md` | 75 |
| `docs/worklog/SLICE-0017-04-plati-tree-doua-niveluri.md` | 128 |
| `docs/worklog/SLICE-0020-02-ddf-view.md` | 98 |
| `docs/worklog/SLICE-0020-03-ddf-preview.md` | 116 |
| `docs/worklog/SLICE-0020-04-ddf-file-browser.md` | 71 |
| `docs/worklog/SLICE-0020-05-ddf-generate.md` | 115 |
| `docs/worklog/state/KBOT_STATUS_0010-0019.md` | 17 |
| `docs/worklog/state/KBOT_STATUS_0020-0029.md` | 24 |
| `docs/worklog/SLICE-D001-03-absorbtie-planuri-si-predari.md` | 0 |
| `docs/worklog/SLICE-D001-19-jurnale-plati-si-ddf.md` | -38 |

Bilanț: **14 căi; net 5 fișiere mai puțin; 30 rânduri eliminate**. Cumul D001-01…19: **69 fișiere și 4430 rânduri mai puțin**. Continuare D001-20.

## SLICE-D001-20 — Istoricul editorului de ordonanțare <a id="slice-d001-20"></a>

### What changed and why

0049, 0049-01 și 0049-02 sunt reunite în jurnalul inițial. Bilanțul identic al celor trei runde din 02 este centralizat, cu trimiteri pe rundă; corpurile distincte și limitele rămân. Registry/Focus/Open threads repetate devin navigare, iar precizările unice sunt transferate. Corecțiile BIC, editării comune și numărului probabil sunt delimitate cronologic.

### Files touched

Inventar: `docs/worklog/SLICE-0049-ord-edit-form.md` (-517); `docs/worklog/SLICE-0049-01-plati-plus-ordonantare.md` (206); `docs/worklog/SLICE-0049-02-ord-edit-corectii-operator.md` (519); `docs/worklog/state/KBOT_STATUS_0040-0049.md` (122); `docs/worklog/SLICE-D001-20-istoric-editor-ordonantare.md` (-31).

Bilanț: **7 căi; net 1 fișiere mai puțin; 298 rânduri eliminate**. Cumul D001-01…20: **70 fișiere și 4728 rânduri mai puțin**. Continuare D001-21.

## SLICE-D001-21 — Istoricul trimiterii și editorului DDF <a id="slice-d001-21"></a>

### What changed and why

Cele 12 jurnale existente 0081 sunt reunite în 01, cu ancore și corpurile etapelor păstrate. Name/Notes/Focus/Open threads repetate devin trimiteri; întrebările și reperele unice sunt transferate. Cronologia separă starea trimiterii, modul probă, antetul, programul din indicatori și înlocuirea dialogului cu grila. Trimiterea din jurnalul Director 0000-06 este actualizată, fără modificarea ajutorului runtime.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0081-01-starea-reviziei.md` | -918 |
| `docs/worklog/SLICE-0081-02-editor-moduri-sectiunea-b.md` | 96 |
| `docs/worklog/SLICE-0081-03-workflow-uri.md` | 61 |
| `docs/worklog/SLICE-0081-04-trimiterea.md` | 76 |
| `docs/worklog/SLICE-0081-05-capturi-table4.md` | 38 |
| `docs/worklog/SLICE-0081-06-kbot-director.md` | 53 |
| `docs/worklog/SLICE-0081-07-proba-si-reincarcare.md` | 132 |
| `docs/worklog/SLICE-0081-08-rand-sectiunea-a.md` | 72 |
| `docs/worklog/SLICE-0081-09-antet-sursa-partener.md` | 91 |
| `docs/worklog/SLICE-0081-10-clsf-parteneri-antet.md` | 58 |
| `docs/worklog/SLICE-0081-12-indicatori-existenti-randuri-zero.md` | 162 |
| `docs/worklog/SLICE-0081-13-sectiunea-a-in-grila.md` | 66 |
| `docs/worklog/state/KBOT_STATUS_0080-0089.md` | 39 |
| `docs/worklog/SLICE-0000-06-partea-3-director.md` | 0 |
| `docs/worklog/SLICE-D001-21-istoric-trimitere-ddf.md` | -41 |

Bilanț: **17 căi; net 10 fișiere mai puțin; 16 rânduri adăugate**. Cumul D001-01…21: **80 fișiere și 4712 rânduri mai puțin**. Continuare D001-22.

## SLICE-D001-22 — Istoricul înregistrării și aprobării <a id="slice-d001-22"></a>

### What changed and why

Jurnalele 0075-01…05 sunt reunite în 01; 00 rămâne contract separat, cu reperele unice transferate din registru. Registrele/focusul repetate sunt înlocuite cu trimiteri, stările de etapă sunt păstrate, iar navigarea planului este actualizată. Chronologia separă varianta inițială, schimbările D27…D29 și confirmările operatorului din 23.09.2026.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0075-01-inregistrare-preauth-anaf-cod.md` | -506 |
| `docs/worklog/SLICE-0075-02-nomenclatoare-nume-cerere.md` | 181 |
| `docs/worklog/SLICE-0075-03-provizionare.md` | 202 |
| `docs/worklog/SLICE-0075-04-pagina-publica.md` | 202 |
| `docs/worklog/SLICE-0075-05-pagina-operator.md` | 110 |
| `docs/worklog/SLICE-0075-00-clasificatii-sursa-scrisa.md` | -6 |
| `docs/worklog/state/KBOT_STATUS_0070-0079.md` | 76 |
| `docs/PLAN_AutoProvisioning.md` | 0 |
| `docs/worklog/SLICE-D001-22-istoric-inregistrare-si-aprobare.md` | -35 |

Bilanț: **11 căi; net 3 fișiere mai puțin; 223 rânduri eliminate**. Cumul D001-01…22: **83 fișiere și 4935 rânduri mai puțin**. Continuare D001-23.

## SLICE-D001-23 — Planul înregistrării fără reluări <a id="slice-d001-23"></a>

### What changed and why

Predarea lungă din §0.0 a planului este înlocuită cu repere și trimiteri către istoricul reunit; datele unice despre dumpuri rămân. §5.1/5.3/5.4/5.5 reunesc regulile repetate, păstrând TTL, verificări, forma numelui, motivele și excepțiile. Tabelul inițial §5.6 este delimitat de deviațiile implementate ulterior. Nu sunt modificate protocoalele SQL/migratorului sau stările de probă.

### Files touched

Inventar: `docs/PLAN_AutoProvisioning.md` (164); `docs/worklog/SLICE-D001-23-plan-inregistrare-fara-reluari.md` (-28).

Bilanț: **4 căi; un jurnal nou; 135 rânduri eliminate**. Cumul D001-01…23: **82 fișiere și 5070 rânduri mai puțin**. Continuare D001-24.

## SLICE-D001-24 — Descărcări multiple, unelte și bugete <a id="slice-d001-24"></a>

### What changed and why

0100 este reunit cu reviziile de server/progres/clic/capturi. Familia 0103 păstrează trei documente tematice: interogări/SSH, buget de deschidere/verificare și puntea Access în ambele sensuri. Repetările din registrul comun devin trimiteri numai pentru 0100/0103; celelalte felii rămân intacte. Precizările unice și trimiterea din jurnalul de ajutor sunt păstrate; fără schimbare runtime.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0100-multithreading-descarcari.md` | -253 |
| `docs/worklog/SLICE-0100-02-setari-server-multithread.md` | 54 |
| `docs/worklog/SLICE-0100-03-descarcari-multiple-rand-pe-angajament.md` | 162 |
| `docs/worklog/SLICE-0100-04-click-acoperit-de-bara-fixa.md` | 37 |
| `docs/worklog/SLICE-0100-05-fara-zgomot-de-capturi-la-actualizare-multipla.md` | 31 |
| `docs/worklog/SLICE-0103-interogari-unice.md` | -43 |
| `docs/worklog/SLICE-0103-03-buget-deschidere-migrator.md` | -47 |
| `docs/worklog/SLICE-0103-05-vba-access-spre-kbot.md` | -53 |
| `docs/worklog/SLICE-0103-02-ssh-cheie-si-baze-bifate.md` | 41 |
| `docs/worklog/SLICE-0103-04-verificare-buget-forexe.md` | 38 |
| `docs/worklog/SLICE-0103-06-trimite-in-access.md` | 44 |
| `docs/worklog/state/KBOT_STATUS_0100-0109.md` | 4 |
| `docs/worklog/SLICE-0000-36-ajutor-setari-multithread.md` | 0 |
| `docs/worklog/SLICE-D001-24-descarcari-unelte-si-bugete.md` | -40 |

Bilanț: **16 căi; net 6 fișiere mai puțin; 26 rânduri adăugate**. Cumul D001-01…24: **88 fișiere și 5044 rânduri mai puțin**. Continuare D001-25.

## SLICE-D001-25 — Actualizare, extrase și jurnale server <a id="slice-d001-25"></a>

### What changed and why

0067/01, cele două perechi tematice 0080 și 0089/01 sunt reunite în documente existente; 0067-02 rămâne separat. Reluările din registre sunt scurtate cu stările păstrate, iar reperele unice sunt transferate: predare/comitere, condiții de punere în funcțiune, confirmări ale operatorului și ajutor. Nu sunt executate actualizări, migrări sau acces server.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0067-actualizare-automata.md` | -89 |
| `docs/worklog/SLICE-0067-01-instalator-doar-actualizari.md` | 127 |
| `docs/worklog/SLICE-0080-01-idclsf-pereche-datadoc.md` | -101 |
| `docs/worklog/SLICE-0080-04-idclsfacc-doar-in-clasificatii.md` | 92 |
| `docs/worklog/SLICE-0080-02-vederea-extrase.md` | -32 |
| `docs/worklog/SLICE-0080-03-fereastra-extrase-de-cont.md` | 39 |
| `docs/worklog/SLICE-0089-jurnale-server-per-utilizator.md` | -35 |
| `docs/worklog/SLICE-0089-01-jurnale-server-doar-avansat.md` | 25 |
| `docs/worklog/state/KBOT_STATUS_0060-0069.md` | 14 |
| `docs/worklog/state/KBOT_STATUS_0080-0089.md` | 15 |
| `docs/worklog/SLICE-D001-25-actualizare-extrase-si-jurnale.md` | -37 |

Bilanț: **13 căi; net 3 fișiere mai puțin; 17 rânduri eliminate**. Cumul D001-01…25: **91 fișiere și 5061 rânduri mai puțin**. Continuare D001-26.

## SLICE-D001-26 — Sumar, Recepții și Istoric <a id="slice-d001-26"></a>

### What changed and why

Opt jurnale sunt reunite în trei istorice, cu corpurile și ancorele etapelor păstrate. Registrele repetate sunt scurtate, precizările unice transferate. În index, măsurătorile/explicațiile vechi despre chei trimit la 0011-03 și sunt delimitate cronologic față de contractul 0080; regula de cardinalitate și cea pentru nomenclatoare rămân. Nicio migrare nu este executată sau declarată confirmată.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0011-01-sumar-endpoint.md` | -171 |
| `docs/worklog/SLICE-0011-02-sumar-view.md` | 109 |
| `docs/worklog/SLICE-0011-03-sumar-join-clasificatii.md` | 138 |
| `docs/worklog/SLICE-0015-01-receptii-view.md` | -143 |
| `docs/worklog/SLICE-0015-02-receptii-root-tooltip.md` | 76 |
| `docs/worklog/SLICE-0015-03-receptii-month-tree-revision.md` | 76 |
| `docs/worklog/SLICE-0022-01-istoric-endpoint.md` | -117 |
| `docs/worklog/SLICE-0022-02-istoric-view.md` | 154 |
| `docs/worklog/state/KBOT_STATUS_0010-0019.md` | 24 |
| `docs/worklog/state/KBOT_STATUS_0020-0029.md` | 9 |
| `docs/worklog/SLICE-D001-26-sumar-receptii-istoric.md` | -37 |

Bilanț: **13 căi; net 4 fișiere mai puțin; 136 rânduri eliminate**. Cumul D001-01…26: **95 fișiere și 5197 rânduri mai puțin**. Continuare D001-27.

## SLICE-D001-27 — Editorul DDF și corecțiile ingestiei <a id="slice-d001-27"></a>

### What changed and why

0051/03 și 0056/02 sunt reunite în jurnalele inițiale, cu ancore și corpurile păstrate. 0051-02 rămâne contract separat pentru combo; reluările din registru devin trimiteri, fără schimbarea stărilor. Intro-urile delimitează probele statice, corecția compartimentului și schimbarea regulii contextului. Trimiterea jurnalului de ajutor este actualizată, fără editarea ajutorului runtime.

### Files touched

Inventar: `docs/worklog/SLICE-0051-ddf-edit-form.md` (-2); `docs/worklog/SLICE-0051-03-ddf-edit-congruenta.md` (236); `docs/worklog/SLICE-0056-tabloul-intreg.md` (-62); `docs/worklog/SLICE-0056-02-corecturi-la-descarcare.md` (108); `docs/worklog/state/KBOT_STATUS_0050-0059.md` (0); `docs/worklog/SLICE-0000-26-ajutor-pentru-0056-02.md` (0); `docs/worklog/SLICE-D001-27-editor-ddf-si-corectii-ingestie.md` (-33).

Bilanț: **9 căi; net 1 fișiere mai puțin; 246 rânduri eliminate**. Cumul D001-01…27: **96 fișiere și 5443 rânduri mai puțin**. Continuare D001-28.

## SLICE-D001-28 — Nucleul și vizualizatorul jurnalelor <a id="slice-d001-28"></a>

### What changed and why

0031-01/04 sunt reunite, iar 03 rămâne document separat pentru controlul de cipuri. Reluările registrului sunt scurtate, rândul de navigare 04 lipsă este completat din jurnal, fără probe noi. Planul păstrează corpul original version-2 și primește navigare/încadrare istorică; indexul nu mai prezintă 04 ca încă nescris. Propunerea per-unitate nu este confundată cu fluxul ulterior 0089.

### Files touched

Inventar: `docs/worklog/SLICE-0031-01-log-core.md` (-95); `docs/worklog/SLICE-0031-04-log-viewer.md` (197); `docs/worklog/state/KBOT_STATUS_0030-0039.md` (-4); `docs/PLAN_LogViewer.md` (-2); `docs/worklog/SLICE-D001-28-nucleu-si-vizualizator-jurnale.md` (-31).

Bilanț: **7 căi; net 0 fișiere mai puțin; 64 rânduri eliminate**. Cumul D001-01…28: **96 fișiere și 5507 rânduri mai puțin**. Continuare D001-29.

## SLICE-D001-29 — Note de versiune și restanțe istorice <a id="slice-d001-29"></a>

### What changed and why

Jurnalul 0067-02 este absorbit în procedura release-notes: contractele comune au o singură sursă, iar detaliile scripturilor, baseline-ul, probele și limitele sunt păstrate în istoricul anexat. Navigarea și registrul sunt actualizate. Trei intrări vechi din possible_future_directions sunt delimitate de corecțiile ulterioare BIC/combo/refresh; propunerea de interfață comună și limitele live rămân. Discrepanța explicației vechi despre WinForms este consemnată în jurnalul controlului. NOUTATI rămâne neatins.

### Files touched

Inventar: `docs/release-notes/README.md` (-64); `docs/worklog/SLICE-0067-02-note-de-versiune.md` (119); `docs/worklog/SLICE-0067-actualizare-automata.md` (0); `docs/worklog/state/KBOT_STATUS_0060-0069.md` (6); `docs/possible_future_directions.md` (28); `docs/worklog/SLICE-0051-02-combo-editabil.md` (-4); `docs/worklog/SLICE-D001-29-note-de-versiune-si-restante-istorice.md` (-33).

Bilanț: **9 căi; net 0 fișiere mai puțin; 51 rânduri eliminate**. Cumul D001-01…29: **96 fișiere și 5558 rânduri mai puțin**. Continuare D001-30.

## SLICE-D001-30 — Istoricul copiilor locale ME02 <a id="slice-d001-30"></a>

### What changed and why

Etapele ME02-03…08 sunt reunite în 03; metadatele SQL 01 și dumpul text 02 rămân distincte. Planul folosește specificația de format ca sursă și delimitează reviziile programării/retenției. Registrul și indexul separă implementarea din 20.10 de confirmarea operatorului din 21.10 pentru două copii, fără extindere la verificare/restaurare și fără executarea lor. Ideea amânată pentru PC-urile utilizatorilor rămâne integral în registru.

### Files touched

Inventar: `docs/worklog/SLICE-ME02-03-format-si-motor.md` (-150); `docs/worklog/SLICE-ME02-04-verbul-snap-run.md` (41); `docs/worklog/SLICE-ME02-05-pagina-copii-locale-si-sarcina-zilnica.md` (43); `docs/worklog/SLICE-ME02-06-07-08-verificare-export-istoric-retentie.md` (42); `docs/ME02_PLAN.md` (5); `docs/worklog/state/KBOT_STATUS_ME02.md` (5); `docs/worklog/SLICE-D001-30-istoric-copii-locale-me02.md` (-33).

Bilanț: **9 căi; net 2 fișiere mai puțin; 48 rânduri adăugate**. Cumul D001-01…30: **98 fișiere și 5510 rânduri mai puțin**. Continuare D001-31.
