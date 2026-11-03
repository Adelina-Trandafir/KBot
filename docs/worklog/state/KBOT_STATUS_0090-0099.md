# K-BOT — STATUS, slices 0090–0099

Everything recorded about each slice: its registry row, its «Current focus» notes and its «Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0090

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0090 | **`KBOT_STATUS.md` împărțit: index + fișiere de câte zece felii (cererea operatorului, 28.09.2026)** | GATA (doar documente, fără cod) | [0090](../SLICE-0090-status-split.md) | Dimensiunile inițiale/finale, repartizarea pe registre, retragerea project_state.md și comparația celor 1.570 de linii cu cele două excepții: în jurnal. |

### Open threads

- **0090 — două fișiere încă mari:** `KBOT_STATUS_0020-0029.md` (~184 KB) și `KBOT_STATUS_0040-0049.md` (~174 KB). Operatorul a refuzat deocamdată împărțirea lor pe felii.

---

## Slice 0091

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0091 | **Detaliul recepției citit tăiat din FOREXE + viteză/așteptări în Setări → FOREXE (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | [0091](../SLICE-0091-detaliu-taiat-viteza-asteptari.md) | Cele trei semne de detaliu incomplet, recitirea țintită, ritmul și pauza F14 sunt descrise în jurnal; ipotezele rămân istorice. |

### Open threads

- **0091 — nimic rulat.** Testul de viteză depinde de pagina fast.com (`#speed-value.succeeded`, `#speed-units`); dacă își schimbă pagina, butonul spune că măsurarea a eșuat.
- **0091 — presupunere:** semnul 3 («mai puțini indicatori decât angajamentul») se sprijină pe un singur angajament (017_SCNB / AAB2DH3X6SK), unde fiecare recepție completă lista toți cei 7 indicatori. Dacă FOREXE listează altundeva doar o parte, recepțiile acelea vor fi semnalate mereu și RHR-ul lor nu se va mai actualiza — atunci semnul 3 trebuie scos.
- **0091 — F14 e încă în pauză** (`F14_PAUSED` în `prelucrare_asociere.py`, `F14Paused` în `AsociereForm.vb`). Scutirea țintită pe recepțiile tăiate o face inutilă; operatorul decide când se repune.
- **0091 — RHR-ul deja stricat** (017_SCNB: recepțiile 84, 86, 90) se repară la prima recitire în care detaliul lor vine întreg: pasul 4b adaugă liniile lipsă și corectează valorile.

---

## Slice 0092

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0092-01 | **Rezervări greșite: butonul «Repară», rândul roșu în arborele principal și ștergerea ultimei luni din istoric (cererea operatorului, 12.10.2026)** | GATA pe cod (build curat KBot.App; nimic rulat pe MariaDB sau pe ecran; **serverul trebuie redeployat**) | `SLICE-0092-01-rezervari-gresite-sterge-luna.md` | August 2026 al unui utilizator pornește de la valorile lui iunie (14 rânduri; verificat pe `Rezervari.txt`). `tree.py` coloana `RezervariGresite` → rând roșu + hover; iconița dreapta-jos din «Rezervări» = «Repară» (reanalizarea existentă); Setări › FOREXE › «Istoric (avansat)» șterge ultima lună din istoric (fără DDF/ORD; cascadă + recepțiile lunii, fără orfane). Ajutor 0000-68; capturile `rezervari*.png` de refăcut. |
| 0092 | **Rezervarea inițială = un singur eveniment, pe ultima zi inițială (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat) | `SLICE-0092-rezervare-initiala-o-singura-zi.md` | AAB5H2CHDGD: AA2 adăugat «În definitivare» pe 27.08, AAB definitivat pe 28.08 → două rânduri `EInitiala` pe zile diferite → două frunze «Inițială». Varianta A (doar cititorii, fără rescriere în bază): `RezervariView.BuildTree` pune toate rândurile inițiale pe ultima zi inițială; `_SQL_GEN_REZERVARI` folosește aceeași zi (`ZiRez`) pentru regula «cea mai veche zi, cel mai mic tip» și ca dată a reviziei. |

### Open threads

- **0092 — AAB5H2CHDGD are DDF doar pe AAB:** AA2 e legat de revizia 262 (`AreDDF = 1`). Dacă 262 conține și AAB, cauza e pasul 3e cazul 2 (exclude revizia «deja folosită», deși o revizie acoperă mai multe rânduri). Aștept rezultatul interogării pe `FX_DDF_REV` / `FX_DDF_REV_SA`.
- **0092 — revizii 0 în plus?** Două «lock NUMARREV=0» în jurnal (12:35, 12:40).

---

## Slice 0093

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0093 | **Parteneri: filtrare, ANAF, banca din IBAN, cod fiscal unic (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | `SLICE-0093-parteneri-anaf-cf-unic.md` | `GET /parteneri` întoarce doar Tip = '1', CodFiscal completat și ≠ `AVACONT_COMUN.Unitati.CF` (DC-ul sesiunii), `Ascuns = 0`, un singur partener pe cod fiscal (primul după IdPartener; codurile comparate doar pe cifre). Banca goală → din IBAN (car. 5-8) prin `AVACONT_COMUN.BIC`, pe server (GET + POST) și în fereastră la tastarea IBAN-ului. Rută nouă `GET /parteneri/anaf/<cf>` (refolosește `routes/inregistrare/anaf.py`, v9). Fereastra: Tip și «Arată partenerii ascunși» scoase, «Alte detalii» → «Adresa», «Cod fiscal *» obligatoriu; la ieșirea din câmp / Enter se caută la ANAF și se completează denumirea + adresa. Salvarea refuză codul unității și un cod fiscal deja folosit (verificare în Python, NU constrângere MariaDB — datele din Access au dubluri; un partener vechi cu cod dublat rămâne editabil cât timp nu-și schimbă codul). Tip scris mereu '1'. |

| 0093-01 | **Parteneri: butonul «Preia date din Access» in ribbon (grupul formularului) - partenerii noi din fisierele Access ale tuturor unitatilor bazei curente intra in `Parteneri`, lista se reincarca singura (cererea operatorului, 03.11.2026)** | INCHIS 03.11.2026 (build curat; ruta pusa pe server si butonul incercat de operator: «a mers»; fara ajutor - regula «nimic Access in ajutor») | `SLICE-0093-01-parteneri-preia-date-din-access.md` | Citire in KBot.Access (`AccessPartnersReader`, `IAccessBridge.ReadPartners`), scriere prin `POST /parteneri/preia-access` (`parteneri_access.py`): doar cheia `(IdUnitate, CodPartener)` care lipseste, nimic nu se actualizeaza, `Provenienta='avacont'`. Butonul apare cat «Parteneri» e pe ecran si `AccessFeature.Enabled`. |
| 0093-02 | **Parteneri: «Selecteaza Parteneri Sincronizare Anaf» / «Sincronizeaza ANAF» in ribbon (grupul formularului) - lista arata toti partenerii (si ascunsii) cu casute de bifat, trei alegeri rapide in meniul capului listei, apoi denumirea si adresa celor bifati se citesc de la ANAF, unul cate unul (cererea operatorului, 03.11.2026)** | GATA pe cod (build curat KBot.App; `py_compile` OK; nimic rulat, nevazut pe ecran; **server nedeployat, tabela `Parteneri_Anaf` neinfiintata**) | `SLICE-0093-02-parteneri-sincronizare-anaf.md` | Server: `parteneri_anaf_sync.py` (`POST .../parteneri/<id>/anaf-sync`, un apel = o cerere ANAF, 1,1 s intre cereri), `parteneri_edit.py` (`anaf_sincronizat` in lista), `sql/0093_02_Parteneri_Anaf.sql` (tabela noua, `Parteneri_Anaf`: marcajul «sincronizat»). Client: `ParteneriForm.Anaf.vb`, `ParteneriForm.vb`, `AvacontMain` (doua butoane, schimbate intre ele), `KbotForm.Nomenclatoare.vb`, `INomenclatoareApi`/`ApiClient.Nomenclatoare`, `Nomenclatoare.vb`. Doar coduri fiscale numerice de 6-12 cifre. Ajutor: `0000-147`. |
| 0093-03 | **Parteneri: cautare si dupa codul fiscal in copac; salvarea (si sincronizarea ANAF) scrie denumirea/adresa si la celelalte randuri cu acelasi cod fiscal, ca sa nu revina datele vechi (cererea operatorului, 03.11.2026)** | GATA pe cod (build curat; `py_compile` OK; nimic rulat; **server nedeployat**; ajutor 0000-148) | `SLICE-0093-03-parteneri-cautare-cf-si-actualizare-furnizor.md` | `TreeItem.SearchExtra` (KBot.Controls), `propagate_to_same_cf` in `parteneri_edit.py`. Cauza datelor vechi dedusa din cod (editorul DDF grupeaza dupa CodFiscal peste toate randurile), nevazuta pe ecran. |

### Open threads

- **0093-03 — cauza "datelor vechi" nedovedita pe ecran:** daca operatorul vede date vechi in alta fereastra deja deschisa (care isi citeste lista la deschidere), propagarea nu o ajuta; randurile deja diferite se uniformizeaza la urmatoarea salvare/sincronizare.
- **0093-02 — nimic rulat; server nedeployat:** ruta si tabela `Parteneri_Anaf` (script `sql/0093_02_Parteneri_Anaf.sql`, de rulat pe fiecare baza prin aplicatia admin) cer deploy/rulare de mana; pana atunci lista merge (toti «nesincronizati»), iar «Sincronizeaza ANAF» primeste 404/500 cu mesaj. Nu exista iesire din modul fara sincronizare (cererea: fara bife nu face nimic). Limita ANAF (1 cerere/s) nu e verificata.
- **0093-01 — se aduc toti partenerii noi** (si fara cod fiscal / alt Tip); mesajul final spune cati nu apar in lista. Partenerii existenti nu se actualizeaza din Access. Testul operatorului a fost fara detalii («a mers»).
- **0093 — nimic rulat.** Nici ruta ANAF, nici fereastra nu au fost încercate pe server/ecran.
- **0093 — partenerii ascunși / cu alt Tip / fără CF nu mai apar deloc** în fereastră; un partener marcat «Ascuns» la salvare dispare din listă. Cererea operatorului, dar nu mai există cale din K-BOT de a-l reafișa.
- **0093 — cod dublat ascuns:** verificarea de unicitate pe server include și partenerii ascunși sau cu alt Tip (mesajul spune «partener ascuns»); fereastra nu-i vede, deci operatorul află doar la salvare.

---

## Slice 0094

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0094 | **KBotComboBox pe Control și editor combo în KBotDataView** (cererea operatorului, 29.09.2026) | GATA pe cod (build Controls/App/Migrator/Theming/Updater **0 / 0**) / **nevăzut pe ecran, fără teste** | [0094](../SLICE-0094-combo-own-control-grid-editor.md) | Colecția/afișarea elementelor, lista unică, tastatura, alinierea, migrarea celor 14 designere și versiunile: în jurnal. |
| 0094-02 | Editor DDF — partener principal și listă asociată | GATA pe cod (build curat, nerulat, nevăzut; SQL-ul și serverul nedeployate) | [0094-02](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-02) | Tabela/rutele/controlul: 0084-02; ajutor 0000-28. Contracte și ordine în jurnal. |
| 0094-03 | Editor DDF — fila Parteneri urmează bifa | GATA pe cod (build curat, nevăzut) | [0094-03](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-03) | Partenerii non-principali ascunși rămân salvați; decizia este deschisă. |

### Current focus

- **Slice 0094 — combo-ul propriu (29.09.2026). Next:** operatorul îl vede în aplicație (grila DDF Secțiunea A, Parteneri, combo-urile din Setări / login / MainForm).

### Open threads

- **0094:** [limitele și diferențele de comportament](../SLICE-0094-combo-own-control-grid-editor.md#left-unverified-or-deferred) păstrează alinierea nemăsurată pentru combo, AssemblyVersion 1.0.0.0, ajutoarele EDIT-ului nativ din `KBot.Theming/Interop/NativeMethods.vb` rămase nefolosite, rotița și Items.Clear(); fără probă nouă pe ecran.
- **0094-02:** [ordine de instalare și probe UI](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-02); nimic văzut/deployat la checkpoint.

---

## Slice 0095

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0095 | **Istoric: nod rădăcină «Tot istoricul»** (cererea operatorului, 29.09.2026) | GATA pe cod (build KBot.App **0 erori**) / **nevăzut pe ecran, fără teste** | [0095](../SLICE-0022-01-istoric-endpoint.md#slice-0095) | Ierarhia, filtrele/intervalul FOREXE, numărătoarea cu rânduri fără dată și FileVersion: în jurnalul 0095. |
| 0095-02 | Extrase — subsolul, fereastra și lățimea meniului | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0095-02](../SLICE-0096-extrase-display-menu.md#slice-0095-02) | Subpas separat de Istoric 0095; ipoteze, limite și FileVersion în jurnal. |

### Open threads

- **0095-02:** [butonul strâns, alinierea și tooltipul](../SLICE-0096-extrase-display-menu.md#slice-0095-02); nevăzute.

---

## Slice 0096

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0096 | Extrase — antet/operații sau operații/detalii | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0096](../SLICE-0096-extrase-display-menu.md#slice-0096) | Ipotezele privind data, gruparea și păstrarea alegerii sunt în etapă. |

### Open threads

- **0096:** [ipotezele de afișare și alegere a modului](../SLICE-0096-extrase-display-menu.md#slice-0096); de confirmat cu operatorul.

---

## Slice 0097

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0097 | Corecturi inițiale: meniuri, unități și autentificare | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste; server nedeployat** | [0097](../SLICE-0097-corective.md#slice-0097) | Regulile inițiale pentru avansate sunt amendate în 0097-04/05; probele și limitele rămân pe etapă. |
| 0097-02 | A doua trecere: tur, browser și recepție pe aceeași dată | GATA pe cod (build KBot.App **0 avertismente, 0 erori**; `node --check` pe script) / **nerulat, nevăzut pe ecran, fără teste** | [0097-02](../SLICE-0097-corective.md#slice-0097-02) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-03 | Anul și Sursă/Sector în bara de titlu | GATA pe cod (build **0 avertismente, 0 erori**) / **nevăzut pe ecran**, nimic rulat | [0097-03](../SLICE-0097-corective.md#slice-0097-03) | Regula de ascundere la sortare este amendată în 0097-06; ajutor 0000-33, captura restantă în jurnal. |
| 0097-04 | Parola memorată: aplicare independentă de avansate | GATA pe cod (build curat, netestat, nevăzut) | [0097-04](../SLICE-0097-corective.md#slice-0097-04) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-05 | Avansatele permit schimbarea setărilor salvate | GATA pe cod (build curat, netestat, nevăzut) | [0097-05](../SLICE-0097-corective.md#slice-0097-05) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-06 | Sursa «Toate» și evenimentul global de perioadă | GATA pe cod (build curat, netestat, nevăzut) | [0097-06](../SLICE-0097-corective.md#slice-0097-06) | Contracte, ajutor, probe și limite în etapa legată. |

### Open threads

- **0097:** [server, testul vechi, eticheta DPI și schimbarea unității](../SLICE-0097-corective.md#slice-0097); încă neîncercate la checkpoint.
- **0097-02:** [probele vizuale, confirmările și recepția pe aceeași dată](../SLICE-0097-corective.md#slice-0097-02); workflow-ul nou nerulat și apelul direct din afara cozii sunt consemnate acolo.

---

## Slice 0098

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0098 | Coada robotului și poarta serverului — inițial | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0098](../SLICE-0098-robot-queue-server-gate.md#slice-0098) | Blocarea citirilor este înlocuită în 0098-02; ajutorul și versiunile la checkpoint sunt în jurnal. |

### Open threads

- **0098/02:** [poarta, ordinea, fereastra, modalele și apelanții direcți](../SLICE-0098-robot-queue-server-gate.md); nimic rulat/văzut la checkpoint. Bancul 02 rămâne nerulat; numai scrierile sunt ținute după amendament.

### 0098-02

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0098-02 | Citirile trec, banc simulat și deschidere peste o acțiune | GATA pe cod (build Debug + Release **0 avertismente, 0 erori**) / **bancul nerulat** | [0098-02](../SLICE-0098-robot-queue-server-gate.md#slice-0098-02) | Bancul scrie fișiere PROBA-* și mesaje [SIMULARE]; nu a fost rulat. |

---

## Slice 0099

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0099 | **PrintCount și lista de tipărire ORD/DDF** (cererea operatorului, 01.10.2026): contorul din partea 1, lista și comenzile din partea 2. Contractele, câmpurile și limitele rămân în jurnal. | GATA pe cod (build KBot.App Debug + Release **0 avertismente, 0 erori**) / **nimic rulat: nicio tipărire, nicio rută chemată, SQL neaplicat; fără teste** | [0099](../SLICE-0099-print-count.md) | Partea 1 nu are nimic vizibil; partea 2 e vizibilă și e în ajutor (`contabil.vederi.ord`, `contabil.ddf`, etichetă 0099). KBot.Controls 1.59.0.0, KBot.Api 1.0.17.0, KBot.Domain 1.2.9.0; KBot.App la `push-update.ps1`. Ordinea de deploy: SQL-ul pe fiecare bază + `AVACONT_SURSA`, apoi `print_count.py` + `__init__.py` pe VPS, apoi clientul. `PrintCount` nu e afișat nicăieri. |

### Open threads

- **0099 — nimic rulat.** De probat pe banc (DevHarness → semnarea PDF): Ctrl+P în Adobe, apoi liniile «[Adobe] Tipărire…» din jurnal.
- **0099 — observarea tipăririi:** [ipotezele și limitele](../SLICE-0099-print-count.md#left-unverified-or-deferred) păstrează numele lucrării, titlul/clasa ferestrei, ritmurile 2 s/250 ms, riscul nemăsurat Microsoft Print to PDF/Salvare ca (capcana neatinsă), rețeaua, sesiunea expirată fără reautentificare și nota CAB fără PDF. Nicio probă nouă.
- **0099 — lista de tipărire, nerulată și nevăzută:** [probele UI și limitele părții 2](../SLICE-0099-print-count.md#left-unverified-or-deferred-part-2) păstrează comenzile, dependența `printto`, copiile/numărarea la terminarea programului (nu confirmarea hârtiei), PDF-ul semnat luat de pe server și generarea celui nesemnat în TempPdf. Fără handler `printto`, fiecare document raportează mesajul shellului.
- **0099 — de deployat:** `sql/0099_print_count.sql` (în `AVACONT_SURSA` întâi `sql/0088_04`, tabela `FX_NoteCAB_Recipisa` lipsește acolo în dump-ul din 22.09.2026), `print_count.py`, `routes/forexe/__init__.py`. `sql/AVACONT_SURSA.sql` și `MariaDB_Schema/` neactualizate.
