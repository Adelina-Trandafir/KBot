# Editor NIR, repartizare și corecții — 16.10.2026

Actualizare 23.10.2026: NIR-urile manuale se pot asocia pe poziții și cantități
cu eFactura, prin drag and drop, cu propuneri CPV/cantitate și memorarea
denumirilor confirmate per furnizor. Aceeași factură poate acoperi NIR-uri
din subunități diferite. Detalii, limite și instalare în
[jurnalul asocierii parțiale](../docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md).
Necesită și `sql/EV_02_vercon_asocieri_partiale.sql`, pregătit pentru execuție
manuală; nu a fost executat. Codul este local, nepublicat și nerulat.

Slice: [SLICE-VC06-06](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md).
Stare: **IMPLEMENTAT ÎN SURSE, NERULAT**. Nu s-au rulat teste, compilări,
browser sau SQL și nu s-au modificat baze de date.

## Introducere și salvare

Factura se introduce direct în Adăugare NIR. Butonul «Factură comună» și
macheta separată au fost eliminate, inclusiv modulul JS și stylesheetul ei.
Modelul intern al facturilor rămâne pentru legătura dintre recepții.

Categorie/cont este un ComboTree cu toate conturile active din DC. Rădăcinile
sunt destinațiile existente, identificate prin perechea SubunitId/GestiuneId;
în limbajul interfeței sunt subunitățile Alimente/Materiale/OI/MF. Rădăcina
curentă este prima și expandată, celelalte sunt restrânse. Numai conturile
au checkbox. Unicul cont din întregul DC este selectat automat și controlul
blocat; unicul cont local nu împiedică alegerea altor destinații.

Produs folosește ComboTree în celula DGV, cu conturile bifate ca rădăcini și
produsele lor dedesubt. Căutarea se face prin tastare. Popupul este separat de
zona decupată a grilei, are înălțime utilă minimă de 220 px și înălțime dorită
de 320 px; alege spațiul de sus când cel de jos este insuficient. Într-un
viewport mai mic se limitează la spațiul disponibil și folosește scroll intern.
Comisia este sub Data Facturii și ocupă coloanele facturii și NIR-ului.

Mai multe conturi ale aceleiași rădăcini rămân într-un singur NIR, cu număr
editabil. Numai conturi din rădăcini diferite impun «auto» inactiv. Serverul
grupează liniile după destinație și alocă numerele în seriile lor la salvare;
nu creează documente goale pentru conturile fără produse.

Salvarea ocupă destinațiile suplimentare numai pe durata tranzacției, în
ordine stabilă, și respectă ocuparea altui utilizator sau a altei file.
Verifică exercițiile, comisiile, numerele, cronologia, produsele, cotele TVA,
disponibilul și egalitatea exactă a totalului cu ValoareFC. O eroare anulează
întregul lot. Aceeași cheie de cerere permite reluarea unui răspuns neconfirmat
fără dublarea documentelor. După salvare se afișează destinațiile și numerele
generate; fiecare NIR se consultă și se modifică în contextul său.

Comisia aleasă trebuie să existe cu aceiași patru membri în fiecare
destinație selectată. Aplicația nu creează comisii sau exerciții automat.
Materiale/Alimente folosesc anul curent ales; MF/OI folosesc contextul
multianual existent. Precizia și cotele TVA sunt cele ale destinației produsului.

## Conturi, TVA și AUTO

Contul liniei se deduce prin Intrari → ProduseP → Produse → Masini. Nu se
adaugă o coloană redundantă. Antetul păstrează IdMasina pentru un singur cont;
pentru mai multe conturi este NULL. Consultarea listează conturile liniilor.

Contul produsului cu mișcări prin oricare variantă de preț, în orice an, este
blocat în nomenclator și în server. Verificarea include mișcările anulate și
variantele inactive. Schimbarea contului și prima mișcare folosesc aceeași
blocare a gestiunii. Fără mișcări contul rămâne editabil; celelalte câmpuri
păstrează propriile reguli.

La modificare, cota TVA din antet este vizibilă numai dacă toate liniile au
aceeași cotă cunoscută. Schimbarea ei propagă procentul către ID-ul TVA local
al fiecărui produs și recalculează. O cotă lipsă într-o destinație blochează
operația. Rândurile AUTO cer întâi revenirea corecției.

«Corectează la valoarea facturii» egalează exact totalul cu valoarea declarată
pentru o diferență de cel mult 1 leu, păstrând TVA real. Izolează cel mult o
bucată din rândul ales și aplică un calcul invers din brut. Existența
înregistrării în VC_NIR_CALCULE indică acest calcul; metadatele rețin totalul
brut, prețul inițial și originea. API derivă calc_mode=GROSS la citire.

Rândurile AUTO nu se editează. Ștergerea readuce fragmentul în grupul de origine
și recalculează, cu simularea stocului ambelor variante înaintea scrierii.
Serverul validează originea prin tokenuri opace cu expirare, inclusiv pentru
ciorne; nu se bazează numai pe blocarea celulelor. Previzualizările nu
creează documente, numere ori variante de preț în MariaDB.

La editarea recepțiilor create de acest editor, liniile interne ale facturii
deținute exclusiv de NIR urmează modificarea și revenirea AUTO în aceeași
tranzacție. Auditul existent identifică facturile create prin repartizare;
nu se rescriu astfel țintele istorice ori liniile împărțite cu alte NIR-uri.
Pentru o factură repartizată cu ValoareFC confirmată, editarea trebuie să
păstreze exact totalul global; valoarea altor NIR-uri nu este schimbată tacit.

Cronologia numerelor și subnumerelor rămâne obligatorie la schimbarea datei;
NIR-ul existent nu este mutat în altă serie anuală. Documentele anulate își
păstrează poziția. Valorile istorice neschimbate sunt păstrate exact.

## Migrare și schemă

Importul Access nu se schimbă pentru noul selector: sursa are un NIR cu un
singur cont local, iar remaparea produs/preț/document este suficientă. Nu se
copiază artificial contul în linii și nu se regrupează automat istoricul.

Mecanismul existent creează fragmente provizorii de factură per NIR importat;
TOTAL_SURSA păstrează antetul original, inclusiv NULL, zero și diferențe.
Antetele nu se însumează ca total global confirmat. Asocierea/reconcilierea
istoricului rămâne în API; vechea machetă a fost eliminată. La modificarea
unui NIR istoric neasociat, asocierea provizorie se face în tranzacția editării.
Stocurile inițiale rămân fără factură. Corecțiile Access la 0% nu primesc
retroactiv marcaj AUTO, deoarece sursa nu păstrează o origine sigură.

Nu este necesar DDL suplimentar pentru noul editor sau blocarea contului.
Rămâne necesară schema facturilor din [VC_FACTURI_NIR_20261016.sql](../sql/VC_FACTURI_NIR_20261016.sql),
aplicată manual de utilizator pe AVACONT_SURSA și sincronizată în DC-uri:
VC_FACTURI, VC_FACTURI_LINII, VC_NIR_FACTURI, VC_FACTURI_REPARTIZARI și
VC_NIR_CALCULE; NumarSub se adaugă dacă lipsește. Scrierea răspunde cu
INVOICE_SCHEMA_REQUIRED dacă schema lipsește. Aplicația nu creează schema.

AVACONT_SURSA de pe server este sursa unică de structură. Dump-urile sunt
fotografii pentru analiză. VRC reutilizează KBot.Migrator.TargetSchema.Load
și WriteOrder.Derive; import_map.json descrie conversia/remaparea sursei.
Scriptul complet generat de schemă a fost eliminat anterior.

Clasificările MF folosesc AVACONT_COMUN.VC_ClasificariMF, administrat separat.
DNF_MIN/DNF_MAX sunt ani convertiți în luni la validarea DNF; durata activului
rămâne în VC_ProduseM. Nu există catalog MF local nou.

## Integrare în surse

- `receipt_destinations.py`: catalog DC, contexte, comisii și ocupări temporare.
- `receipt_distribution.py`: GET `/api/vercon/doc/nir/destinations`, POST
  `/api/vercon/doc/nir/distribution/validate` și `/distribution`, calcul și
  salvare atomică pentru editorul de adăugare.
- `receipts.py`, `receipt_rules.py`, `receipt_auto.py`, `receipt_number.py`:
  validarea/scrierea locală, cronologie, calcul și AUTO; editarea folosește
  în continuare PUT `/api/vercon/doc/nir/<id>` și POST `/nir/validate`.
- `documents.py`, `invoices.py`, `doc_routes.py`: conturile din produse,
  facturile interne și integrarea rutelor. API-ul de facturi/loturi rămâne
  disponibil pentru consumatorii existenți, fără ecranul respins.
- `docs.js`, `receipt-allocation.js`, `receipt-context.js`: antetul, grila,
  repartizarea și fișa produsului în contextul său.
- `components/combotree/combotree.js`, `dgv/editing.js`,
  `components/treeview/treeview.js`: selectorul comun și integrarea în celulă.
- `nomenclatoare.py`, `nomenclator_usage.py`, `nomenclator-grid.js`: blocarea
  contului produsului cu mișcări în UI și API.
- `css/combotree/combotree.css`, `vercon-receipt-destinations.css`, `vercon.css`:
  stilurile comune și poziționarea antetului.

Contract autoritar: [VC06-06-D01–D11](DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06).
