# SLICE-VC09-09 — Recuperarea prețurilor retururilor și coloanele de rulaj ale balanței

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Corecție ulterioară cerută de utilizator: [VC09-10](SLICE-VC09-10-balanta-fara-randuri-net-zero.md) elimină păstrarea rândurilor doar pe baza existenței documentelor compensate. Dacă SI, Intrări nete, Ieșiri nete și SF sunt toate zero, rândul este ascuns implicit. Descrierea `has_movements` de mai jos consemnează implementarea inițială, înlocuită prin VC09-10.

## Scop autorizat

Utilizatorul a cerut repararea aceleiași pierderi de zecimale și la bonurile de retur, în continuarea VC09-06/07/08. A stabilit și regula coloanelor balanței: **storno diminuează Intrările; returul diminuează Ieșirile**, atât cantitativ, cât și valoric. Operațiile își păstrează familia documentului, în loc să fie trecute în coloana opusă pentru semnul lor asupra stocului.

## Migrator și KBotAdmin

VRC.Migrator **0.3.15.0**, reguli `VC09-STORNO-RETURN-PRICE-2026-10-30`: `PrepareCorrectionPriceRow` este apelată pentru `IntrariStorno` și `IesiriRetur`. În ambele cazuri, coloana Access PUTVA este Long. Se recuperează prețul din `ProduseP` al aceleiași surse și partiții/gestiuni numai dacă întregul Access (`Int32`) corespunde rotunjirii ToEven a prețului variantei. Prețurile zecimale, nule sau incompatibile nu sunt înlocuite automat; diferențele incompatibile sunt semnalate N21. N20 consemnează separat tabela, ID-ul și prețurile vechi/noi. Recuperarea are loc înaintea conversiei și acumulării sumelor pentru reconcilierea existentă; Access rămâne nemodificat.

Comanda KBotAdmin devine **Preturi storno si retur VERCON**, cu **Repară** pentru toate cazurile eligibile din baza selectată. Numele interne CLI `storno_preturi` / `storno_preturi_repara` sunt păstrate. Sumarul este pe subunitate/gestiune/tip, iar detaliile disting Storno și Retur, numărul documentului și prețul actual/propus. Numerele compuse BX sunt sortate după numărul extras, nu lexicografic după etichetă.

Același modul citește două familii fixe, fără identificatori SQL primiți din client:

- Storno: `VC_IntrariStorno` și `VC_NIR`, cu `IdIntrare`, `Storno=1`, anularea liniei și a documentului.
- Retur: `VC_IesiriRetur` și `VC_Iesiri_H`, cu `IdBonConsum`, `Retur=1`, anularea din antet. Nu se presupune o coloană Anulat pe linia returului.

Legăturile folosesc subunitatea, gestiunea și, la documente, exercițiul. Se păstrează condițiile de proveniență `VC_IdMap` / `VC_ImportSources`, versiunea 1 a liniei, rotunjirea compatibilă și existența auditului. Cazurile neexplicate, modificate, anulate sau fără proveniență rămân afișate pentru analiză.

Repararea ambelor familii se face într-o singură tranzacție, după recitirea datelor sub blocările gestiunilor. UPDATE-urile reverifică cheile, prețul, cantitatea, starea și reviziile. Pentru fiecare document se crește revizia antetului o singură dată; cheia de grupare include familia, astfel încât un NIR și un bon cu același ID intern nu se confundă. Există audit vechi/nou pentru linii și revizii de document, cu operații `admin.storno.price.repair[.document]` și `admin.retur.price.repair[.document]`, plus rezumat în Jurnal. Tabelele de linii/documente ale ambelor familii, auditul și Jurnalul trebuie să fie InnoDB. Cantitățile, valorile din antete și prețurile variantelor nu se rescriu. Nu se aplică TVA încă o dată.

## Balanța

`stock.warehouse_turnover` derivă coloanele din mișcările existente:

- **Intrări = NIR − storno** (plus transferurile primite, când există).
- **Ieșiri = consum − retur** (plus transferuri predate/casări, când există).
- **SF = SI + Intrări − Ieșiri**.

În exemplul comunicat: NIR 69, storno 35, consum 34, fără SI/retur → **Intrări 34, Ieșiri 34, SF 0**. Un storno de 35 singur în perioadă produce Intrări −35; un retur de 5 singur produce Ieșiri −5. Sunt exemple de proiectare, nu probe executate.

Semnele fizice din `stock_source`, acumularea SI/SF, disponibilul și validarea cronologică rămân cele existente. Nu se inversează cantitățile înregistrate și nu se folosesc valori absolute; corecțiile istorice semnate își păstrează sensul. Rapoartele din `report_data.balances` folosesc noua agregare la cantități și evaluarea valorică aferentă; și Centralizator Conturi folosește aceste rulaje. Documentele individuale și centralizatorul de intrări păstrează valorile proprii ale documentelor și semnarea existentă a stornărilor.

API-ul de stoc adaugă cantități/valori agregate `incoming` și `outgoing`, inclusiv în totaluri, fără eliminarea câmpurilor existente pe familii. Pagina Stocuri și previzualizarea balanței consumă aceste agregate, cu etichetele Intrări / Ieșiri. Calculul rulajelor OI/MF din registrul deținătorilor nu este schimbat; în acele mișcări nu există storno/retur de magazie. Fișele cronologice și sensul mișcărilor lor rămân existente.

**Produsele cu rulaje compensate complet rămân în Balanță.** NIR + storno poate da net zero chiar dacă au existat documente în perioadă. `has_movements` este derivat din mișcările nenule din interval și păstrat de filtrarea raportului; opțiunea fără mișcări nu le elimină doar pentru că SI/Intrări/Ieșiri/SF sunt toate zero.

Evaluarea balanței rămâne la prețul curent al variantei, conform regulii existente; nu s-a schimbat baza de evaluare în valori istorice ale documentelor. Reîncadrarea în coloane elimină diferența de clasificare a stornărilor/retururilor, fără a declara reconcilierea pe date reale sau paritatea oricăror prețuri istorice cu prețurile curente.

Modelul raportului include `warehouse_balance_version`. Exporturile vechi ale balanței și rapoartelor de rulaje ale magaziei cer regenerare; PDF și Excel folosesc același model actualizat ca ecranul.

## Fișiere

- Migrator: `VERCON/VRC.Migrator/VrcRunner.Storno.vb`, `VrcRunner.vb`, `VrcRun.vb`, `VrcPlan.vb`, `VRC.Migrator.vbproj`, `README.md`.
- KBotAdmin: `KBotAdmin/Views/CommandsView.vb`, `PYTHON/routes/admin_cli/admin_cli.py`, `vercon_storno_prices.py`.
- Balanță: `PYTHON/routes/vercon/stock.py`, `report_data.py`, `report_balance.py`, `report_service.py`, `report_routes.py`, `PYTHON/static/js/vercon/stock.js`.
- Documentare: acest worklog, trimiterile din VC09-07/08, `VERCON/VERCON_STATUS.md`, indexul `implementare_vercon_v2.md`.

## Stare și limite

Doar citire de cod și analiză de proiectare. **Fără build, teste, verificări automate de sintaxă, rulare KBotAdmin/migrator/VERCON, date reale, SQL executat, modificări MariaDB sau publicare.** Nu s-a măsurat câte retururi sunt afectate. Sunt necesare actualizarea serverului/resurselor web, recompilarea KBotAdmin și a migratorului, apoi regenerarea balanțelor. Datele deja migrate sunt reparabile prin comanda generală; nu au fost schimbate de asistent.
