# SLICE-VC06-06 — facturi comune, corecții NIR și schemă unică

Data: 16.10.2026. Stare: **IMPLEMENTAT_LOCAL, FĂRĂ EXECUȚIE**.

Extensie autorizată de utilizator la VC06, cu integrare în infrastructura VC03/VC05.
Reunește lucrul din acest thread: regulile NIR rezultate din analiza Access FC,
facturile repartizate, corecțiile AUTO și reutilizarea infrastructurii KBot.Migrator.
Nu certifică funcționarea prin build, teste sau rulare pe server.

## Comportament implementat în surse

- Cronologia numerelor și subnumerelor NIR limitează modificarea datei în seria
  anuală. Modificarea liniilor, scoaterea lor și anularea folosesc regulile de
  dependențe și disponibil, cu simularea stocului înaintea scrierii.
- TVA în antetul de modificare este derivat din rânduri și vizibil pentru o cotă
  comună. Schimbarea propagă cota și recalculează; rândurile AUTO cer mai întâi
  revenirea corecției.
- Corecția de cel mult 1 leu egalează exact suma liniilor cu valoarea declarată,
  păstrând TVA real prin calcul invers din totalul brut. Rândul AUTO nu se editează;
  ștergerea lui returnează fragmentul în grupul original și verifică stocul ambelor
  variante. Originea și permisiunea revenirii sunt validate și în server.
- Existența înregistrării în `VC_NIR_CALCULE` identifică calculul invers. Tabelul
  păstrează totalul brut, prețul inițial și originea, fără un mod de calcul redundant.
- Factura comună are linii și repartizări către NIR-uri din conturi, gestiuni și
  subunități diferite ale aceluiași DC. Recepțiile pot fi parțiale; reconcilierea
  completă cere egalități exacte. Endpointul de lot acceptă mai multe contexte
  explicit, într-o singură tranzacție.
- Migrarea păstrează câte un fragment provizoriu pentru fiecare NIR istoric și
  valoarea sursă separat; nu însumează antetele ca total global și nu regrupează
  automat facturile. Asocierea istoricului existent este reluabilă, fără reimport.
- Validarea DNF la recepția MF citește nomenclatorul comun
  `AVACONT_COMUN.VC_ClasificariMF`; durata aleasă rămâne în `VC_ProduseM`.
- VRC reutilizează `KBot.Migrator.TargetSchema.Load` și `WriteOrder.Derive` pentru
  schema reală a DC-ului și ordinea după FK plus dependențele logice Access.
  `import_map.json` păstrează doar regulile sursei. Eliminat scriptul complet
  generat de creare și generatorul lui; AVACONT_SURSA de pe server rămâne unica
  referință de structură, sincronizată separat în DC-uri.

## Livrabile

- Analiza și contractul: [analiza NIR](../../VERCON/ANALIZA_FLUX_NIR_2026-10-16.md),
  [contractul calculelor](../../VERCON/CONTRACT_CALCULE.md),
  [implementare și activare](../../VERCON/FACTURI_NIR_IMPLEMENTARE.md).
- Backend: modulele `receipts`, `receipt_rules`, `receipt_auto`, `receipt_number`,
  `invoices`, `receipt_routes` și integrarea în rutele/documentele VERCON.
- Interfață: editorul NIR, modulul facturilor comune și stylesheet separat.
- Migrator: `VrcRunner.Invoices`, `VrcImportMap`, `VrcWriter.Schema`, adaptările
  scriitorului și suprasarcina comună `KBot.Migrator.WriteOrder.Derive`.
- SQL manual: [VC_FACTURI_NIR_20261016.sql](../../sql/VC_FACTURI_NIR_20261016.sql).
  Cele cinci tabele sunt `VC_FACTURI`, `VC_FACTURI_LINII`, `VC_NIR_FACTURI`,
  `VC_FACTURI_REPARTIZARI`, `VC_NIR_CALCULE`; se adaugă și `NumarSub` dacă lipsește.
- Registrul implementării: [implementare_vercon_v2](../../implementare_vercon_v2.md).

## Activare și limite

Utilizatorul aplică DDL-ul pe AVACONT_SURSA și sincronizează schema în DC-uri.
Nu există creare sau reparare automată în VRC ori aplicație. Utilizatorul a
confirmat că nu a executat încă acest DDL; nu este necesar un script de eliminare
a coloanei redundante. Publicarea VRC 0.2.1.0 include KBot.Migrator 1.19.0.0.

Fluxul economic este în RON. Interfața recepționează în etape; loturile cu mai multe
destinații sunt disponibile prin endpoint. Corecțiile Access vechi la 0% nu sunt
marcate retroactiv AUTO. Detaliile și celelalte limite sunt în documentul de
implementare legat mai sus.

Nu s-au rulat teste, compilări, verificări automate, browser sau SQL. Nu s-au
modificat baze de date și nu s-a făcut publicare. Se pregătește numai staging-ul
modificărilor din acest thread; directoarele sursă fără legătură rămân în afară.
