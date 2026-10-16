# Facturi comune și validări NIR — implementare 16.10.2026

Slice: [SLICE-VC06-06 — facturi comune, corecții NIR și schemă unică](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md).

Stare: **IMPLEMENTAT ÎN SURSE, FĂRĂ EXECUȚIE**. Nu s-au rulat teste, compilări, verificări automate, browser sau interogări pe baze de date. Nu s-a aplicat DDL. Funcționarea în mediul utilizatorului nu este confirmată prin rulare.

## Activare

1. Utilizatorul execută `sql/VC_FACTURI_NIR_20261016.sql` pe baza de schemă `AVACONT_SURSA`, apoi sincronizează schema spre DC-urile folosite. Baza `AVACONT_SURSA` de pe server este sursa unică a structurii; dump-urile locale sunt fotografii pentru analiză. Scriptul complet generat `VC_AVACONT_SURSA.sql` a fost eliminat. Aplicația nu creează sau repară automat schema.
2. Se publică împreună modulele Python, JavaScript și CSS modificate. Scrierile NIR cer noua schemă și răspund explicit cu `INVOICE_SCHEMA_REQUIRED` dacă nu este disponibilă; consultarea documentelor vechi rămâne disponibilă.
3. Pentru importuri viitoare se folosește sursa nouă VRC.Migrator. Acesta reutilizează `KBot.Migrator.TargetSchema.Load` pentru schema reală și `WriteOrder.Derive` pentru ordinea cheilor străine; verifică cerințele importului înainte de transfer și creează fragmentele de factură în tranzacția importului. `import_map.json` descrie doar conversia/remaparea datelor Access, fără tipuri SQL sau reguli de nulabilitate pentru țintă.
4. Pentru date deja importate: în pagina NIR → **Factură comună → Asociază istoricul gestiunii**. Operația este reluabilă și nu reimportă subunitatea. La editarea unui NIR istoric neasociat, asocierea provizorie a acelui document se face în tranzacția editării.

## Folosire

- Data NIR este limitată de datele numerelor mai mici/mari din aceeași serie anuală; se ține cont de subnumere și de pozițiile documentelor anulate. Data nu mută un NIR existent în altă serie anuală. O anomalie istorică neschimbată nu renumerotează documentele.
- La modificare, cota TVA din antet este derivată din rânduri. Pentru cote mixte sau necunoscute este ascunsă. Pentru o cotă comună este afișată; schimbarea propagă TVA și recalculează întreaga ciornă înainte de acceptare. Dacă există rânduri automate, propagarea este blocată până la revenirea corecțiilor.
- **Corectează la valoarea NIR** folosește rândul selectat sau primul rând eligibil, păstrează cota reală și acceptă o diferență de cel mult 1 leu. După corecție suma liniilor este exact valoarea declarată. Cantitatea totală a produsului se păstrează; se izolează cel mult o bucată pentru prețul ajustat. Existența rândului în `VC_NIR_CALCULE` indică calculul invers din totalul cu TVA; se păstrează totalul brut, prețul inițial și originea corecției. La citire, serverul derivă marcajul API `calc_mode=GROSS` din existența înregistrării; nu se stochează un mod de calcul separat.
- Rândurile marcate **AUTO** sunt nemodificabile în toate coloanele. **Șterge** returnează fragmentul în grupul original și recalculează; dacă grupul nu mai există, fragmentul original este reconstruit. Revenirea trece prin simularea stocului pentru ambele variante. Originea și permisiunea revenirii sunt validate și în server, nu numai prin blocarea celulelor.
- Previzualizările de ciornă nu scriu documente, variante sau numere în MariaDB. Identitățile pentru rândurile automate și reveniri folosesc note opace ale utilizatorului/DC în STORE-ul portalului, cu expirare de 8 ore. Documentele salvate primesc note noi la recitire. Nu se cere o cheie de configurare nouă.
- **Factură comună** permite introducerea facturii furnizorului și a liniilor/valorilor sale, alegerea ei pentru NIR-ul curent, consultarea recepțiilor din toate destinațiile și reconcilierea explicită.
- Pentru o factură repartizată, NIR-ul are **Valoare aferentă acestui NIR**; ValoareFC globală este afișată distinct. Fiecare rând NIR se leagă de o linie a facturii. Se păstrează recepțiile parțiale; nu se corectează artificial partea nerecepționată. Cantitățile și valorile unei linii noi de factură nu pot fi depășite de recepțiile active.
- Pentru cazul simplu, fără o factură comună aleasă, salvarea NIR creează factura locală și liniile aferente. O valoare declarată lipsă produce un total calculat al NIR-ului, dar nu un total global confirmat al facturii.
- **Finalizează reconcilierea** cere egalitate exactă pe cantități și valori ale liniilor și între totalul recepțiilor active și ValoareFC confirmată. Totalurile provizorii din migrare nu sunt prezentate drept totaluri globale certe.
- Datele facturii comune cu recepții nu se schimbă tacit prin editorul unui singur NIR. Factura locală a unui singur NIR se poate actualiza în aceeași tranzacție cu acesta. Fluxul economic este în RON; conversia valutară nu este introdusă implicit.
- Salvările de creare/editare/anulare păstrează cheia cererii pentru repetarea unui răspuns neconfirmat. În server, repetarea aceleiași comenzi nu aplică din nou documentul.

## Migrare și proveniență

VRC creează câte un fragment provizoriu pentru fiecare NIR obișnuit. `VC_NIR_FACTURI.TOTAL_SURSA` păstrează antetul original, inclusiv NULL, zero și diferențe față de linii. `SOURCE_ID`/`SOURCE_KEY` reutilizează mapările existente. Liniile reconstruite rămân distincte de liniile confirmate ale unei facturi introduse în web.

Reunirea se face explicit din meniul fragmentului în lista de facturi, către factura deschisă. Se cere același furnizor, număr, dată și monedă; după reunire totalul global rămâne neconfirmat până la introducerea lui de operator. Antetele fragmentelor nu sunt însumate automat. NIR-urile și mișcările lor nu se modifică prin reunire. Liniile istorice reconstruite nemodificate, inclusiv cantități sau valori speciale, sunt păstrate exact.

Stocurile inițiale rămân fără factură. Nu se deduce retroactiv că un rând Access la 0% a fost creat de corecție: sursa nu are o marcă sigură pentru această concluzie. Blocarea AUTO se aplică rândurilor generate de noul mecanism.

Catalogul de clasificări MF este comun: `AVACONT_COMUN.VC_ClasificariMF`, administrat separat de migrarea gestiunilor. Validarea DNF din NIR leagă `Cod` de `VC_Produse.CC` și convertește limitele `DNF_MIN`/`DNF_MAX` din ani în luni. Limitele NULL nu impun o restricție pe acea parte; când nu există o clasificare asociată, rămâne regula duratei pozitive. Migratorul nu mai copiază limitele din Access în cataloage locale și nu scrie în nomenclatorul comun. `VC_ProduseM` păstrează durata aleasă pentru fiecare activ. Structura și datele de referință au fost citite din exportul actualizat `MariaDB_Schema/AVACONT_COMUN.sql`; nu s-au executat interogări pe server.

## Contract API

- `POST /api/vercon/doc/nir/validate`: ciornă completă sau numai rânduri calculabile; `action=validate|adjust|undo_adjustment`, `line_key`, `id`, `version`. Ultimele două acțiuni întorc liniile rezultate; revenirea întoarce un token care se păstrează în `undo_tokens` până la salvare.
- `GET /api/vercon/invoices`: lista/candidații facturilor din DC, cu filtre opționale `partner`, `number`, `date`.
- `GET /api/vercon/invoices/<id>`: antet, linii, cantități/valori recepționate și NIR-uri din toate subunitățile/gestiunile asociate.
- `POST /api/vercon/invoices/save|merge|backfill|finalize`: comenzi cu `request_key`, context verificat și ocuparea gestiunii curente. Reunirea/asocierea istoricului nu modifică mișcări sau valori istorice.
- `POST /api/vercon/invoices/receive-batch`: salvare atomică a unui lot explicit. Cere `invoice_id`, `version`, `request_key` și `receipts`. Fiecare recepție are contextul, `context_version`, `lock_token`, antetul și liniile sale; la editare include `id` și `version`. Toate gestiunile se validează și se blochează în ordine stabilă. Se repetă verificarea ocupărilor înainte de commit. Interfața NIR permite recepția în etape; endpointul de lot oferă operația atomică pentru clienții care trimit mai multe destinații simultan.

Tabele noi în DC: `VC_FACTURI`, `VC_FACTURI_LINII`, `VC_NIR_FACTURI`, `VC_FACTURI_REPARTIZARI`, `VC_NIR_CALCULE`. Catalogul comun MF nu este inclus în acest DDL și nu condiționează disponibilitatea schemei de facturi. Nu se modifică identificatorii documentelor existente. `NumarSub` este adăugată de fișierul SQL numai dacă schema veche nu o are.

## Fișiere principale

- `PYTHON/routes/vercon/receipts.py`, `receipt_rules.py`, `receipt_auto.py`, `receipt_number.py`: validări, calcul, originea corecțiilor, simulare și scriere NIR.
- `PYTHON/routes/vercon/invoices.py`, `receipt_routes.py`: facturi comune, repartizări, reconciliere, asociere istoric și loturi.
- `documents.py`, `doc_routes.py`, `__init__.py`, `protocol.py`: integrare, dependențe active, anulare și repetarea comenzilor.
- `PYTHON/static/js/vercon/docs.js`, `invoices.js`, `PYTHON/static/css/vercon-invoices.css`, `vercon.css`: editor, blocarea AUTO și interfața facturii comune.
- `VERCON/VRC.Migrator/VrcRunner.Invoices.vb`, `VrcRunner.vb`, `VrcWriter.vb`: schema cerută și asocierea provizorie la import.
- `sql/VC_FACTURI_NIR_20261016.sql`: DDL incremental manual; `VERCON/tools/import_map.js`: mapare Access și domenii de acces ale aplicației, fără generare SQL.

Nu s-au făcut commit, publicare sau modificări de baze de date din această conversație.
