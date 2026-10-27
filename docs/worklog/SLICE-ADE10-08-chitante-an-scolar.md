# SLICE-ADE10-08 — Chitante pe ani scolari, jurnal rapid in ADE.Migrator, resetare la inchiderea anuala

<a id="slice-ADE10-08"></a>

Starea: **cod facut, build ADE.Migrator curat; NETESTAT, NERULAT, AD_13 NEAPLICAT.**

## Ce s-a schimbat si de ce

Unele unitati au acelasi numar de chitanta in mai multi ani (numerotarea reincepe in septembrie). Cheia `uq_Sub_Serie_Numar` (`SubunitId, Serie, Numar`) le respingea, iar migratorul scria o constatare de blocaj pentru fiecare numar dublat: cu mii de dubluri jurnalul din formular devenea foarte lent.

1. **Cheia.** `sql/AD_13_chitante_an_scolar.sql`: coloana calculata `AD_Chitante.AnScolar` (STORED, din `Data`: septembrie = an nou) si cheia `uq_Sub_Serie_An_Numar` (`SubunitId, Serie, AnScolar, Numar`) in locul celei vechi. Coloana nu se scrie nicaieri.
2. **Migrator.** Dublurile se cauta numai in acelasi an scolar (`AdeReceiptNumbers.GroupKey` include anul; `AdeReceipt.SchoolYear`). O singura constatare pentru toate dublurile (numar de numere, de chitante, ani, 5 exemple); chitantele fara data sunt o nota.
3. **Jurnal rapid.** `AdeMigratorForm.AppendLog`: intrare taiata la 4000 caractere, maximum 20 constatari scrise una cate una, caseta limitata la 400000 caractere (se scot liniile vechi); logul comun ramane necenzurat.
4. **Server.** `service.py`: `school_year_bounds` / `receipt_number_taken` — verificarea „numar deja folosit” se face pe anul scolar al datei chitantei (interval pe `Data`, deci nu depinde de coloana noua).
5. **Resetare la inchiderea anuala.** `annual.py`: `preview` intoarce `receipt_config`; `validate`/`apply` accepta `annual.receipts = {reset, next_number}` si, daca `reset`, pun `AD_ReceiptConfig.Numar` pe numarul ales (seria ramane), refuzand un numar deja folosit in noul an. UI: caseta „Reia numerotarea chitantelor in noul an scolar” (nebifata implicit) in `adechit.html`, `adechit-annual.css`, `annual.js`.

## Completari 26.10.2026 (dupa prima rulare pe date reale)

Pe MDB-ul real au ramas 272 de numere dublate in ACELASI an scolar (545 chitante; exemplu GRSM 751, IDC 1 si 821). Incercari respinse de operator: «taie cifra miilor» (nu ajuta la numere sub 1000), interval rezervat 900001+ (urat si nesigur pentru afisare). Solutia finala:

- **sub-numere ca la NIR-urile VERCON** (`VC_NIR.NumarSub`, sutimi: 751,1 = 751 + sub 10), date AUTOMAT de plan (`AdePlan.ApplySubNumbers`, `AdeReceiptNumbers.AssignSubNumbers`): cea mai noua chitanta din grup ramane fara sub-numar, cele mai vechi primesc ,1 ,2 …; numarul din Access nu se schimba (deci nu mai e nevoie de `NumarOriginal`); situatia nu mai blocheaza (o singura nota in plan; blocaj doar peste 100 chitante cu acelasi numar);
- fereastra «Chitante dublate» si regula «taie cifra miilor» ELIMINATE (`AdeReceiptsForm` sters);
- cheia (SubunitId, Serie, AnScolar, Numar, NumarSub), coloana `AD_Chitante.NumarSub SMALLINT NOT NULL DEFAULT 0` (`sql/AD_13_02_chitante_numar_original.sql`, numele fisierului ramas din faza anterioara);
- butonul «Golește date» in `AdeMigratorForm` (dupa «Golește lista» din VRC.Migrator);
- aplicatia web emite numai sub 0 (`receipt_number_taken` verifica `NumarSub=0`) si arata sub-numarul DOAR in Registrul de casa (`_cash_entries`) si Fisa debitor (`debtor_report`), prin `domain.receipt_number_text`; restul nu s-a schimbat. Codul web cere coloana `NumarSub` in baza: se publica dupa sincronizare.

Nevalidat: nimic rulat (nici migratorul pe MDB, nici AD_13_02). Constatarea «mai multe grupe de plecati» este regula veche (ADE5-05/08), neschimbata: serverul accepta cel mult o grupa `PLECATI`.

## Fisiere

`sql/AD_13_chitante_an_scolar.sql` (nou) · `ADECHIT/ADE.Migrator/{AdeReceiptNumbers,AdePlan,AdeMigratorForm,AdeReceiptsForm}.vb`, `README.md` · `PYTHON/routes/adechit/{service,annual}.py` · `PYTHON/static/{adechit.html,css/adechit-annual.css,js/adechit/annual.js}` · `ADECHIT/UTILIZARE_WEB.md`.

## Rezultate

- `dotnet build ADE.Migrator.vbproj`: 0 erori, 0 avertismente.
- `py_compile` service.py/annual.py si `node --check` annual.js: fara erori de sintaxa.
- Nu s-a rulat nimic altceva (nici migratorul, nici serverul, nici SQL-ul, nici fereastra anuala in browser, nici teste).

## Nevalidat / amanat

- **Incident 26.10.2026 (greseala mea de analiza):** am presupus ca sincronizarea de structura adauga coloane calculate in tabele existente; `schema_diff.py` le sare ("generated columns skipped"). La rularea FORCE pe 24 de baze: cheia veche `uq_Sub_Serie_Numar` a fost STEARSA peste tot (24 OK), `CREATE UNIQUE INDEX uq_Sub_Serie_An_Numar` a esuat pe prima baza (000_DEMO, errno 1072, coloana `AnScolar` nu exista), restul de 23 neatinse. Starea: coloana lipseste in cele 24 de baze, nicio cheie unica pe (Serie, Numar); datele intacte (copii de siguranta facute de sincronizare). Remediu: `schema_diff._columns` adauga acum coloana calculata lipsa (ADD COLUMN, prioritatea 7, inainte de cheia din prioritatea 10); dupa ce `schema_diff.py` ajunge pe server, se reruleaza sincronizarea (adauga `AnScolar`, apoi creeaza cheia). Pana atunci unicitatea o apara doar verificarea din server. Corectura din `schema_diff.py` este neverificata: nu s-a rulat sincronizarea si nici testele de schema.
- AD_13 aplicat in AVACONT_SURSA (rezulta din planul sincronizarii, care a vrut sa creeze cheia noua).
- Fereastra de inchidere anuala cu caseta noua: neverificata pe ecran.
- Testele existente `test_adechit_annual.py` / `test_adechit.py`: neverificate dupa schimbare (cheia `receipt_config` noua in `preview`).
- Ajutorul din aplicatia web ADECHIT nu are pagina dedicata actualizata; doar `UTILIZARE_WEB.md` si README-ul migratorului.
