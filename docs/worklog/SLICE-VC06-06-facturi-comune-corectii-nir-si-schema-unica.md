# SLICE-VC06-06 — facturi comune, corecții NIR și schemă unică

Data: 16.10.2026. Stare: **IMPLEMENTAT_LOCAL, FĂRĂ EXECUȚIE**.

Slice-ul include implementarea noului editor NIR aprobată ulterior în același
thread: [VC06-06-D01–D11](../../VERCON/DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06).
Documentația și sursele descriu acum același flux. Macheta separată de factură
respinsă de utilizator a fost eliminată.

## Comportament implementat în surse

- Introducerea facturii direct în Adăugare NIR, cu toate produsele în aceeași
  grilă; eliminate butonul și modulul UI «Factură comună».
- Arbore de conturi din întregul DC: rădăcina curentă prima și expandată,
  celelalte restrânse; checkbox numai pe conturi. Unicul cont global blochează
  selectorul; unicul cont local permite alegerea altor destinații.
- Mai multe conturi din aceeași destinație produc un NIR, cu număr editabil.
  Numai destinații diferite impun «auto». Liniile se grupează după contextul
  SubunitId/GestiuneId existent; numerele se alocă la salvare în seriile lor.
- ComboTree comun pentru produs în DGV: conturi → produse, căutare la tastare,
  înălțime utilă minimă, deschidere deasupra când spațiul inferior este
  insuficient, limitare la viewport și scroll intern. Grila recunoaște focusul
  din popup, inclusiv câmpul de căutare separat de celulă.
- Comisia ocupă două coloane sub Data Facturii, până la capătul Data NIR.
- Salvarea întregii facturi și a NIR-urilor sale este atomică. Contextele
  suplimentare se ocupă temporar în ordine stabilă; o destinație ocupată
  blochează tot lotul. Se validează toate produsele, perioadele, comisiile,
  numerele, cronologia, stocurile și totalurile înaintea scrierii. Repetarea
  aceleiași cereri nu dublează documentele.
- Contul liniei se deduce din ProduseP → Produse → Masini; nu există câmp nou
  redundant. Antetul păstrează IdMasina pentru cazul unic, altfel NULL.
  Consultarea și metadatele facturii folosesc conturile produselor liniilor.
- Contul produsului cu mișcări prin orice variantă, în orice an, este blocat
  în UI și server. Blocarea include variante inactive și mișcări anulate;
  serializarea cu scrierile documentelor protejează și prima mișcare.
- Cronologia numerelor/subnumerelor limitează schimbarea datei NIR. Modificarea
  și scoaterea liniilor/anularea păstrează regulile de dependențe și stoc.
- TVA comun în antet la modificare numai când toate liniile au aceeași cotă
  cunoscută; propagare a procentului către cota locală a fiecărui produs.
- Corecție de cel mult 1 leu pentru egalitate exactă cu ValoareFC, păstrând TVA
  real prin calcul invers. Existența în VC_NIR_CALCULE indică mecanismul.
  Rândurile AUTO nu se editează; ștergerea returnează fragmentul în grupul
  original și verifică stocul ambelor variante, cu origine validată în server.
- Editarea și revenirea AUTO sincronizează liniile interne de factură ale
  NIR-ului creat prin noul flux, identificate prin auditul existent. Nu ating
  liniile altor recepții sau țintele istorice. Totalul global confirmat rămâne
  exact ValoareFC; o modificare incompatibilă este refuzată atomic.
- Importul Access nu se adaptează noului selector: un NIR sursă are un singur
  cont local. Remaparea existentă produs/preț/document este suficientă.
- Migrarea păstrează fragmente provizorii și antetele originale separat;
  nu însumează antetele ca total global confirmat și nu regrupează istoricul.
- DNF MF folosește AVACONT_COMUN.VC_ClasificariMF, în ani convertiți în luni;
  durata activului rămâne în VC_ProduseM.
- VRC reutilizează KBot.Migrator.TargetSchema.Load și WriteOrder.Derive.
  AVACONT_SURSA de pe server rămâne sursa unică a structurii, iar import_map
  păstrează regulile sursei. Scriptul complet generat a fost eliminat anterior.

## Livrabile

- Backend nou: `receipt_destinations.py`, `receipt_distribution.py`.
- Integrare backend: `doc_routes.py`, `documents.py`, `receipts.py`, `invoices.py`,
  `nomenclatoare.py`, `nomenclator_usage.py`; mecanismele anterioare de calcul,
  AUTO, cronologie și facturi rămân reutilizate.
- Frontend: `receipt-allocation.js`, `docs.js`, `receipt-context.js`,
  `nomenclator-grid.js`, ComboTree comun, integrarea în `dgv/editing.js` și
  inițializarea TreeView în containere încă neatașate paginii.
- CSS: `combotree/combotree.css`, `vercon-receipt-destinations.css`, `vercon.css`.
  Eliminate `js/vercon/invoices.js` și `css/vercon-invoices.css`.
- Livrările anterioare ale slice-ului: adaptările VRC/Migrator pentru schema
  reală, ordinea FK și fragmentele provizorii; SQL incremental manual
  [VC_FACTURI_NIR_20261016.sql](../../sql/VC_FACTURI_NIR_20261016.sql).
- Documentație aliniată: DECIZII_DESCHISE, FLUXURI_UI, PLAN_IMPLEMENTARE,
  VERCON_STATUS, SCHEMA_VC, CONTRACT_CALCULE, ANALIZA_FLUX_NIR_2026-10-16,
  FACTURI_NIR_IMPLEMENTARE, README VRC.Migrator și implementare_vercon_v2.

## Activare și limitele consemnării

Utilizatorul aplică DDL-ul facturilor pe AVACONT_SURSA și sincronizează schema
în DC-uri. Noul editor și blocarea contului nu cer un DDL suplimentar.
Aplicația și VRC nu creează/repară schema automat. Nu s-a pregătit un script
pentru eliminarea coloanei redundante, deoarece utilizatorul a confirmat că
nu aplicase încă DDL-ul. VRC 0.2.1.0 include KBot.Migrator 1.19.0.0.

Fluxul este în RON. Comisia trebuie să existe cu aceiași membri în destinațiile
alese; aplicația nu creează nomenclatoare sau exerciții automat. Fiecare NIR
rezultat se consultă/modifică în contextul său. API-ul de facturi istorice și
reconciliere rămâne disponibil, fără vechiul ecran respins.

Nu s-au rulat teste, build, browser, verificări automate sau SQL. Nu s-au
modificat baze, nu s-a făcut publicare sau commit. Citirea surselor pentru
proiectare nu certifică funcționarea în mediul utilizatorului. Modificările
fără legătură din KBot.App și directoarele Surse nu fac parte din acest slice.
