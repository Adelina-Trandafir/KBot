# SLICE-ADE10-01…06 — Subunități: schemă, API, migrator, interfață și predare

Etapele ADE10-01…05 și completările lor ADE10-06 sunt reunite în ordine, cu ancore per etapă. Fișierele sursă foloseau prefixul AD10, iar titlurile și registrul ADE10; identificatorii istorici nu sunt renumerotați. AD10-01 al drepturilor pe secțiuni este un istoric distinct.

Pentru rubricile inițiale de nevalidat/amânat din cele cinci etape: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Confirmările ulterioare și limitele inițiale se păstrează separat. Restricțiile individuale pe subunitate, descrise întâi ca viitoare, sunt completate în ADE10-06. Predarea descrie pașii operatorului, fără a autoriza executarea lor în cadrul consolidării.

- [ADE10-01](#slice-ADE10-01)
- [ADE10-02](#slice-ADE10-02)
- [ADE10-03](#slice-ADE10-03)
- [ADE10-04](#slice-ADE10-04)
- [ADE10-05](#slice-ADE10-05)

## ADE10-01 — Schema pentru subunități (sql/AD_05_subunitati.sql) <a id="slice-ADE10-01"></a>

Implementează secțiunile 3, 4 și 8 din [planul subunităților](../../ADECHIT/plan_subunitati.md).

### Ce s-a schimbat și de ce

Un singur script nou, `sql/AD_05_subunitati.sql`, de rulat o dată pe fiecare bază ADE (și pe șablonul `000_DEMO`, ca unitățile noi să pornească cu schema nouă). Scriptul a fost generat cu un program de unică folosință; textul final este cel din repo.

- **Tabele noi:** `AD_Subunits` (id, denumire unică, activă), `AD_ReceiptConfig` (serie, contor, explicație, versiune — una pe subunitate) și `AD_IdMap` (import + tabel + ID Access → ID server).
- **`SubunitId` obligatoriu** în cele 17 tabele ADE: Grupe, ValoriTaxe, LunaD, Platitori, Platitori_sub, Prezenta, Plati, Chitante, AlteDoc, Retur, SS_Buget, Grupe_Educator, Platitori_Istoric, Compensare, Settings, Operations, Imports.
- **Chei care erau pe tot DC-ul devin pe subunitate:** `(SubunitId, Luna, Anul)`, `(SubunitId, Serie, Numar)`, setare `(SubunitId, SettingKey)`, cheie de reîncercare `(SubunitId, RequestKey)`. `AD_Imports` păstrează `SourceHash` drept cheie: același MDB nu se importă de două ori în același DC.
- **Relații compuse:** cele 29 de chei străine dintre tabelele ADE sunt refăcute ca `(SubunitId, ColoanaLegată)` către `(SubunitId, Cheie)` — baza refuză o legătură între subunități (inclusiv compensările dintre doi copii și ambele capete ale istoricului). Plus câte o cheie străină spre `AD_Subunits` pe fiecare tabel (46 de `ADD CONSTRAINT` în total).
- **Strategia de ștergere pe relație:** cele 6 chei `ON DELETE SET NULL` (Plati/Retur `IDZ`,`IDL`; Chitante/AlteDoc `IDL`) devin `RESTRICT`, pentru că un SET NULL compus ar încerca să golească și `SubunitId`. Codul le golește singur înainte de ștergere (`reopen_month`: Plati/Retur erau deja tratate; s-a adăugat pentru Chitante și AlteDoc). `SS_Buget` rămâne CASCADE.
- **`AD_LunaD.Ordine`:** ordinea locală a lunilor (plan §5). La conversie `Ordine = IDL`; la import `Ordine` = IDL-ul din Access (cu eventualele goluri); o lună nouă primește `max(Ordine)+1` în subunitate.
- **Conversia DC-urilor deja populate:** subunitate inițială cu numele din `@ade_subunit_name` (de editat la începutul scriptului); toate rândurile îi sunt atribuite; seria și contorul se copiază din `AVACONT_COMUN.Unitati_Chitante` (rândul cu `DC = DATABASE()`); rândul de blocare `AD_Lock` are ID = SubunitId. Un DC fără date nu primește subunitate (o creează migratorul). Dacă `AD_Imports` are mai multe loturi, scriptul se oprește cu mesaj (date din mai multe surse → reconciliere pe proveniență, nu atribuire automată). Scriptul refuză și o a doua rulare.
- `AVACONT_COMUN.Unitati_Chitante` rămâne neschimbat și nu mai este citit de ADE.

### Fișiere

`sql/AD_05_subunitati.sql` (nou).

### Teste

Niciunul: DDL rulat și verificat pe MariaDB (confirmat de operator, 23.10.2026). Numele cheilor străine sunt luate din `MariaDB_Schema/000_DEMO.sql` (export 09.10.2026); alte baze pot avea nume diferite. Dacă un `DROP FOREIGN KEY` eșuează, scriptul se oprește (DDL nu e tranzacțional; revenirea = restaurarea copiei de siguranță).

### Rămâne neverificat / amânat

Rularea reală; compatibilitatea numelor de chei în baze neexportate; timpul pe baze mari. După rularea scriptului aplicația veche și migratorul vechi nu mai sunt compatibile (plan §9).

## ADE10-02 — API și operații cu context obligatoriu DC + subunitate <a id="slice-ADE10-02"></a>

Implementează secțiunile 4, 5, 6 și 8 (partea web) din [planul subunităților](../../ADECHIT/plan_subunitati.md).

### Ce s-a schimbat și de ce

- **Context.** Antetul `X-Ade-Subunit` se validează pe server la fiecare cerere față de subunitățile active ale DC-ului (`Repository.select_subunit`). Un id necunoscut sau inactiv → 409 `SUBUNIT_CONTEXT`. Lipsa antetului este acceptată numai când DC-ul are **exact o** subunitate activă (pagini vechi, unități cu o singură evidență); altfel 409 `SUBUNIT_REQUIRED` / `SUBUNIT_NONE`. `/api/adechit/context` poate rula fără subunitate și întoarce lista (`subunits`) și subunitatea rezolvată (`subunit`).
- **Repository.** Toate accesările sunt limitate la subunitate: `rows`, `get`, `insert` (adaugă `SubunitId`), `update`, `delete`, `years`, `situation_data`, `setting`, `data`. Fără subunitate, `scope()` refuză. Un ID din altă subunitate nu se poate citi, modifica sau șterge (404 `NOT_FOUND`).
- **Blocare.** `AD_Lock` are un rând pe subunitate (ID = SubunitId, creat la nevoie); mutațiile unei subunități se serializează, subunitățile diferite nu. Ordinea rămâne: lock → drepturi → rânduri → configurația chitanțelor.
- **Chitanțe.** Seria și contorul se citesc și se incrementează din `AD_ReceiptConfig` al subunității, în aceeași tranzacție cu documentul; duplicatul se caută pe `(SubunitId, Serie, Numar)`. PDF-ul și listarea afișează „Subunitate: …".
- **Reîncercări.** `AD_Operations` este pe `(SubunitId, RequestKey)` și amprenta include subunitatea.
- **Regula temporală.** `temporal()` compară `Ordine + 1` cu cea mai nouă lună din Prezență (nu IDL); echivalentă cu sursa deoarece `Ordine` = IDL-ul din Access, inclusiv golurile. Calculele de solduri păstrează comparația pe `IDL`, valabilă pentru că importul inserează lunile în ordinea Access, deci id-urile serverului cresc în aceeași ordine (formulele de business nu s-au schimbat).
- **Import web** (`/api/adechit/import`, `/reconcile`). Intră în subunitatea curentă (trebuie să fie goală). ID-urile nu se mai păstrează: serverul le alocă și referințele se rescriu prin hartă, salvată în `AD_IdMap`. Reconcilierea compară prin hartă. Același extras într-o altă subunitate → 409 `IMPORT_OTHER_SUBUNIT`. Seria se scrie în `AD_ReceiptConfig`.
- Câmp nou `Ordine` în `schema.json` (LunaD).

### Fișiere

`PYTHON/routes/adechit/`: `repository.py`, `__init__.py`, `service.py`, `importer.py`, `receipt_output.py`, `schema.json`; `PYTHON/templates/adechit/receipt.html`. Unelte locale: `ADECHIT/tools/preview.py` (schema SQLite cu `SubunitId`; opțiunea `--second-subunit` (acum implicit; `--single-subunit` o oprește) adaugă „Evidenta B" cu serie proprie DEMOB), `ADECHIT/tools/verify_import.py`.

### Teste

Rulate: nimic (nici automate, nici manuale). Doar `ast.parse` pe modulele Python. Teste existente aliniate la schema nouă: `PYTHON/tests/test_adechit.py` (lista tabelelor păstrate în `test_import_repeat_and_reconcile`) și `PYTHON/tests/adechit_lazy.test.mjs` (contextul simulat întoarce subunitatea). Nu s-au adăugat teste noi (regula proiectului).

### Rămâne neverificat / amânat

Toate fluxurile pe MariaDB; izolarea între subunități (scenariile 2–6 și 8 din plan) de probat de utilizator. Subunitățile se creează numai prin migrator sau SQL (nu există ecran de administrare, redenumire sau dezactivare). Restricții pe subunitate per utilizator — extensie viitoare (drepturile rămân pe DC).

### Acces pe subunitate (completare ADE10-06) <a id="ade10-06-acces"></a>

`AD_SubunitAccess (SubunitId, Email)`: o subunitate fara randuri este deschisa tuturor utilizatorilor cu drepturi ADE in DC; una cu randuri este deschisa numai e-mailurilor listate. Selectorul arata doar subunitatile permise si apare numai daca utilizatorul are mai mult de una; una singura se alege automat. O subunitate ceruta fara acces -> 403 SUBUNIT_FORBIDDEN. Administrarea listei se face deocamdata prin SQL (INSERT INTO AD_SubunitAccess); nu exista ecran. Neprobat.

### Corecturi după review (ADE10-06)

- **Snapshot înainte de blocare:** `transaction()` citește subunitățile într-o citire scurtă pe care o încheie cu `commit`, apoi pornește tranzacția reală și blochează rândul din `AD_Lock`. Altfel, sub REPEATABLE READ prima citire fixa snapshot-ul și o cerere care aștepta blocarea continua cu date vechi (inclusiv la verificarea `AD_Operations`). Izolarea efectivă a serverului rămâne neverificată.
- **Reîncercări de dinainte de conversie:** `idempotent()` acceptă și amprenta veche, fără subunitate; căutarea rămâne limitată la subunitate.
- **Reconciliere:** harta `AD_IdMap` se caută după subunitate (o subunitate primește un singur import), nu după hash: migratorul VB salvează hash-ul fișierului MDB, importul web hash-ul extractului normalizat. Limitare: același MDB importat pe cele două căi, în subunități diferite, nu este detectat ca import repetat.
- **Acces pe utilizator:** regula completă `AD_SubunitAccess` este în [completarea ADE10-06](#ade10-06-acces) de mai sus.
- **Sincronizarea schemei (`schema_sync`):** o schimbare de cheie primară pe o coloană încă lipsă din țintă rula (prioritate 3) înaintea `ADD COLUMN` (7) și eșua cu errno 1072 pe `AD_Operations`. În `schema_diff.py` o astfel de schimbare se emite acum ca `PK CREATE` (prioritate 9).

## ADE10-03 — ADE.Migrator pe subunități <a id="slice-ADE10-03"></a>

Implementează secțiunea 8 (migrator) din [planul subunităților](../../ADECHIT/plan_subunitati.md).

### Ce s-a schimbat și de ce

- Câmp nou **Subunitate** lângă DC destinație (propus din `Unitati.Denumire`, modificabil). Un nume nou creează subunitatea; numele unei subunități existente o alege (trebuie să fie activă și goală). Identitatea nu vine din calea MDB-ului.
- **Testează** verifică: tabelele și coloanele noi din AD_05, subunitatea (activă, fără rânduri, fără import, fără serie), faptul că hash-ul MDB nu a mai fost importat în DC, InnoDB. Înlocuiește verificarea „tabele goale pe tot DC-ul" cu cea pe subunitate: se poate importa B după A.
- **Migrează** (o singură tranzacție): creează sau ia subunitatea și îi blochează rândul din `AD_Lock` (același mecanism ca aplicația web), scrie fiecare tabel **fără cheile Access** (le alocă serverul) și rescrie toate referințele prin hartă — inclusiv rândurile construite de migrator (educatori, istoric copii). Lunile se scriu în ordinea IDL din Access; `LunaD.Ordine` = IDL Access. Harta se salvează în `AD_IdMap` (inserturi în loturi de 400). Seria și contorul intră în `AD_ReceiptConfig` al subunității; `Unitati_Chitante` nu mai este atinsă. `AD_Imports` primește subunitatea. Jurnalul SQL și confirmarea arată DC + subunitate.
- Rollback / COMMIT necunoscut se tratează ca înainte.

### Fișiere

`ADECHIT/ADE.Migrator/`: `AdeWriter.vb` (rescris), `AdeSchema.vb` (+Ordine, +BuiltRelations), `AdePlan.vb` (+Ordine), `AdeSource.vb` (+UnitName), `AdeMigratorForm.vb`, `AdeMigratorForm.Designer.vb`, `README.md`.

### Teste

`dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: 0 erori, 0 avertismente. Nicio migrare rulată.

### Rămâne neverificat / amânat

Migrarea efectivă a `baza_40.mdb` și `baza_47.mdb` (scenariile 1 și 9 și completările din secțiunea 12 a planului); completarea sau înlocuirea unei subunități populate rămâne în afara primei versiuni.

### Corectură după review (ADE10-06)

Sub blocarea subunității, `EnsureSubunit` reverifică acum **toată** destinația (toate tabelele planului, `AD_Imports`, `AD_ReceiptConfig`), în aceeași tranzacție, nu doar `AD_Imports`. Rânduri adăugate de aplicație între Testează și Migrează opresc migrarea cu mesaj. Build: 0 erori, 0 avertismente; nerulat.

## ADE10-04 — Selectorul „Subunitate" și comutarea <a id="slice-ADE10-04"></a>

Implementează secțiunea 7 din [planul subunităților](../../ADECHIT/plan_subunitati.md).

### Ce s-a schimbat și de ce

- În antet, lângă unitate, combobox-ul comun **Subunitate** (denumirea rămâne vizibilă). Selecția se ține în `sessionStorage` (independentă în fiecare filă, pe cheia DC-ului: schimbarea DC-ului o invalidează) și se trimite pe fiecare cerere în `X-Ade-Subunit`.
- La deschidere: subunitatea reținută dacă serverul o acceptă; singura subunitate se alege automat; cu mai multe și fără una reținută, ecranul cere alegerea și nu citește nimic din evidență; fără nicio subunitate, mesaj care trimite la migrator. Dacă serverul refuză subunitatea reținută (`SUBUNIT_CONTEXT`), selecția se uită și se cere din nou.
- **Comutarea reîncarcă pagina**: anii, lunile și grupele selectate, grilele, cataloagele, dialogurile, cache-urile și orice răspuns întârziat al subunității vechi dispar odată cu ea. Dacă există o fereastră deschisă sau un rând nou de document completat, comutarea este refuzată cu mesaj (nu se pierde nimic în tăcere). Cererile de listare și PDF trimit și ele subunitatea.

### Fișiere

`PYTHON/static/js/adechit/app.js`, `PYTHON/static/adechit.html` (selector și paragraf de ajutor), `PYTHON/static/css/adechit.css`.

### Teste

Niciun test rulat; nicio verificare vizuală. Ajutorul din pagină și `ADECHIT/UTILIZARE_WEB.md` actualizate cu referința ADE10-04.

### Rămâne neverificat / amânat

Aspectul în antet pe PC și telefon, comutarea cu două file deschise, tema închisă — de probat de utilizator.

### Corecturi după review (ADE10-06)

- Selectorul apare numai dacă utilizatorul are acces la mai mult de o subunitate; una singură se alege automat.
- Comutarea salvează întâi celula de prezență aflată în editare (ca Enter) și este refuzată dacă salvarea eșuează sau dacă o salvare este în curs (`state.pending` în `api()`).

### Revizie de interfață și sesiune (10.10.2026, același sub-slice AD10)

- **Interfață:** Subunitate mutat deasupra LUNA / ANUL, fără etichetă; Plătitori, Taxe și tema într-un meniu ☰ în antet (aceeași fereastră popup ca filtrul din subsolul grilei, cu emoticoane); „A” deschide aplicațiile contului (K-BOT activ, Vercon/Avacont în curând); căutarea este centrată pe grila principală (antetul are coloanele panoului); 🔄 în bara LUNA / ANUL; 🔒/🔓 ca iconiță dreapta pe rândurile lunilor (nou: `rightIcon` în TreeView); toate antetele de 32 px; coloanele numerice egale; panoul din stânga se restrânge; butonul panoului Rapoarte rămâne centrat; la alegerea unui copil se deschide fila documentelor lui; „Grupa ” dispare din nume (majuscule, fără spații); filtre 🔍 în subsolul grilei; banda de mesaje afișează numai erori.
- **Sesiune:** o oră de la cod, fără prelungire și fără alunecare (`PORTAL_MAX = PORTAL_IDLE = 3600`); cronometru în ultimele 5 minute (`adechit/session.js`); la expirare se face deconectare chiar cu modificări nesalvate. Codul de pe e-mail rămâne valabil o oră **în același browser** (`kbot-portal-trust` în localStorage + notă `portal_trust` pe server): după deconectare, e-mailul și parola ajung. Pagina de autentificare are combobox „Aplicația” (K-BOT / ADECHIT active); cu ADECHIT ales, portalul nu mai este desenat deloc.
- **Date locale:** `ADECHIT/tools/preview.py` pornește implicit cu două subunități și cu o lună închisă (septembrie 2026) și una deschisă (octombrie 2026) în fiecare (`--single-subunit` oprește a doua subunitate).
- Ajutorul din pagină actualizat. Teste: `adechit_lazy.test.mjs` adaptat la modulele noi, `test_adechit.py` neschimbat.

## ADE10-05 — Predare: subunități în același DC <a id="slice-ADE10-05"></a>

Pachet pentru [planul subunităților](../../ADECHIT/plan_subunitati.md). Nimic nu a fost publicat, rulat pe server sau testat din acest chat; commit și push nu s-au făcut.

### Ordinea de punere în funcțiune (coordonată, cu scrierile suspendate)

1. Copie de siguranță a fiecărei baze ADE (și a bazei-șablon `000_DEMO`). Nimeni nu lucrează în ADECHIT și nu rulează ADE.Migrator.
2. `sql/AD_05_subunitati.sql` pe fiecare bază, după ce se editează `@ade_subunit_name` (numele subunității inițiale a datelor existente). Se citesc apoi interogările de control de la finalul scriptului.
3. Publicare prin AvacontPush: `PYTHON/routes/adechit/*` (repository, __init__, service, importer, receipt_output, schema.json), `PYTHON/templates/adechit/receipt.html`, `PYTHON/static/adechit.html`, `PYTHON/static/js/adechit/app.js`, `PYTHON/static/css/adechit.css`; restart backend.
4. ADE.Migrator nou (build din `ADECHIT/ADE.Migrator`) pentru importurile următoare.

După pasul 2 aplicația veche și migratorul vechi nu mai sunt compatibile; revenirea cere restaurarea coordonată a bazei și a aplicației (plan §9).

### Ce probează utilizatorul (scenariile din plan §10 și completările §12)

1. Importul `baza_40.mdb` și `baza_47.mdb` în două subunități ale aceluiași DC (alegând DC-ul părinte și denumirile): ID-uri Access comune, luni comune, legături rămase în sursa lor. 40 = zero chitanțe, contor 6715; 47 = seria G.R., contor 10199.
2. Comutarea subunității (și două file, A și B); anii, lunile, grupele, copiii, taxele și soldurile se schimbă.
3. Citire și modificare cu un ID din cealaltă subunitate → 404; legătură între subunități → refuzată și de bază.
4. Emitere chitanță în A și în B: serie/contor independente; anulare și reîncercare nu mută contorul celeilalte.
5. Emitere concurentă în aceeași subunitate fără numere duble; aceeași cheie de reîncercare în A și B.
6. Închidere lunară/anuală, redeschidere, recalculare — numai în subunitatea curentă.
7. Regula temporală a documentelor, înainte și după remapare, inclusiv istoric `SS_Buget`/`Retur` cu luni lipsă.
8. Conversia unui DC existent: date, documente, solduri, serie; PDF-ul numește subunitatea.
9. Probă locală: `python ADECHIT/tools/preview.py --second-subunit` (localhost:5050) pentru comutare fără server.

### Decizii luate aici (pe linia planului; de infirmat dacă nu convin)

- Antet lipsă = acceptat numai cu o singură subunitate activă în DC (pagini vechi; unități cu o evidență).
- `ON DELETE SET NULL` → `RESTRICT` cu golirea legăturilor în cod (compus nu poate SET NULL fără a goli `SubunitId`).
- `AD_Lock.ID` = SubunitId; `AD_Imports` rămâne unic pe `SourceHash` la nivel de DC.
- `Ordine` păstrează golurile de IDL din sursă; comparațiile de solduri rămân pe IDL, cu importul în ordinea Access.
- Subunitatea nu se creează din interfața web; migratorul sau SQL.

### Teste

Niciunul rulat. `dotnet build` ADE.Migrator curat; `ast.parse` pe Python. Teste existente adaptate fără rulare: `test_adechit.py`, `adechit_lazy.test.mjs`.

### Neverificat / amânat

Tot ce depinde de MariaDB, de server și de ecran. Amânat explicit (plan §11): importuri istorice într-o subunitate populată, restricții individuale pe subunitate, rapoarte consolidate, administrarea subunităților din web.

### Actualizare după review (ADE10-06)

Fișiere de publicat suplimentar față de lista de mai sus: `PYTHON/routes/schema_sync/schema_diff.py` (corectură de sincronizare) și versiunea nouă a `PYTHON/routes/adechit/*` și `app.js`. `sql/AD_05_subunitati.sql` conține acum și `AD_SubunitAccess`; dacă varianta veche nu a fost rulată pe nicio bază, se rulează doar cea nouă. Restart backend după publicare. Acces pe subunitate: `INSERT INTO AD_SubunitAccess (SubunitId, Email) VALUES (...)`; atenție, primul rând pe o subunitate o închide pentru toți ceilalți.
