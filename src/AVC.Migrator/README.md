# AVC.Migrator - utilizare

<!-- slice: AV01-05 -->

Utilitar Windows separat, pe modelul `ADE.Migrator`, pentru baza Access Avacont (`baza2026.accdb`) -> tabelele `TZ_` din baza MariaDB a unitatii.
`KBot.Migrator` nu se modifica si nu se foloseste pentru Avacont. Ordinea migrarilor: `KBot.Migrator`, apoi E-Factura, apoi `AVC.Migrator`.

## Inainte de prima rulare

1. Scriptele `sql/AVACONT/Configurari/PlanCont/AV01_04_conturi_sursa.sql`, `AV01_04b_plancont_sursasector.sql`, `AV01_04c_soldinitial_rulaje.sql` si `AV01_04d_subunitati_unitate_parinte.sql` sunt rulate pe `AVACONT_SURSA`, apoi «Sincronizare schema» le copiaza in baza unitatii. Coloanele de rulaje trebuie sa existe in MariaDB (le foloseste fereastra «Plan de conturi»), desi valorile lor nu se migreaza; verificarea le cere.
2. Unitatile se portheaza ca subunitati chiar din migrator, butonul **Subunitati...** (capitolul nu exista in Access); daca verificarea gaseste o unitate fara subunitate, migratorul propune singur deschiderea ferestrei.
3. Bazele de date ale unitatii si cea comuna (`AVACONT_COMUN`) exista pe server.

## Pasii (ca la KBot.Migrator)

1. **Registrul**: alegeti `cale.accdb` (de obicei `C:\AVACONT\cale.accdb`), completati parola fisierelor daca sunt protejate, apoi butonul cu pictograma baza de date. Din registru vin DC-urile, unitatile si caile fisierelor fiecarei unitati.
2. **DC**: alegeti DC-ul. Este si baza MariaDB in care se scrie (se vede la «Baza-tinta»). Lista de unitati arata unitatile DC-ului; **bifati-le pe cele de transferat**. Codul fiscal este propus din registrul Windows si il puteti corecta (se scrie in antetul jurnalului SQL).
3. **Server**: gazda, port, utilizator, parola, baza comuna (`AVACONT_COMUN`).
4. **Citeste unitatile bifate**: citeste fisierele Access (`baza2026.accdb`) ale unitatilor bifate. Doar citire, nu atinge serverul.
5. **Subunitati...**: o linie pe fiecare unitate a DC-ului. Bifati unitatile de portat (se pot mai multe odata); *Denumirea* vine din `cai.AlteDetalii`, *Capitolul* din `cai.SURSA` (01A = 65.01, 02A = 65.02, 02E = 65.10; un al doilea 01A, de exemplu 65.11, se completeaza de mana), *Zecimale salarii* implicit 0 - toate se corecteaza in grila. Aceleasi reguli ca in fereastra «Plan de conturi». O unitate cu `IdUnitateParinte` pastreaza legatura (citeste clasificatiile si bugetul parintelui): parintele trebuie portat inainte sau odata cu ea. Prima subunitate scrisa in baza este cea implicita (nu se alege).
6. **Verifica**: verifica parola, baza si structura, apoi compara ce s-a citit cu nomenclatoarele. Nu scrie nimic. Constatarile sunt grupate: *blocaje* (opresc migrarea), *atentionari* (de citit), *informatii*.
7. **Migreaza** se activeaza doar dupa o verificare fara blocaje. Orice modificare (DC, bife de unitati, server, bife de pasi) cere o noua verificare. Totul se scrie intr-o singura tranzactie; **Opreste** cere rollback inainte de COMMIT.

Anul este fix 2026 (aceeasi decizie ca la KBot.Migrator: anul curent plus soldurile initiale).

Jurnalele SQL sunt in `Jurnale\AVC\<baza>\<data_ora>\` (langa executabil), ca la `ADE.Migrator`: instructiunile se scriu inainte de executie, `_99_final.txt` spune COMMIT sau rollback.
Erorile merg in `Logs\harness_errors.log`; la fiecare lansare jurnalul anterior se arhiveaza cu prefixul `AVC`.

## Ce migreaza acum

| Pas | Din Access | In MariaDB |
|---|---|---|
| Plan de conturi (cu solduri initiale) | `PlanCont` din fisierul fiecarei unitati bifate | `TZ_PlanCont` (subunitatea fiecarei unitati) + `TZ_SoldInitial` (anul 2026, debit inainte de credit; rulajele NU se migreaza) |
| Solduri initiale pe parteneri (AV01-08) | `ParteneriSI` din fisierul fiecarei unitati bifate | `TZ_SoldInitial` cu `IdPartener` (anul 2026; doar SID / SIC; contul se gaseste prin simbolul compus din `PlanCont`, partenerul prin `Parteneri` pe unitate + cod) |
| Jurnale (AV01-09, AV01-10) | `Jurnale` (`CodJurnal`, `DenumireJurnal`) din fisierul fiecarei unitati bifate | `TZ_Jurnale` (un singur tabel pe baza unitatii; fiecare jurnal ia sectorul si sursa subunitatii unitatii lui, deci pasul cere «Subunitati»; se reunesc dupa sector+sursa si cod, litere mari; la acelasi cod cu denumiri diferite ramane prima denumire) |

Pasul «Solduri initiale pe parteneri» are nevoie de «Plan de conturi» (aceeasi rulare sau una anterioara) si de partenerii mutati de KBot.Migrator. Un sold al carui cont sau partener lipseste din tinta NU se scrie si este numit in verificare (ATENTIE); celelalte se scriu.

Conturile fiecarei unitati intra in subunitatea ei (gasita prin `TZ_Subunitati.IdUnitateAccess`), deci se pot transfera mai multe unitati odata; verificarea **blocheaza** pentru o unitate care nu este inca subunitate.

Un cont cu acelasi simbol ca unul existent ramane asa cum l-ati editat in fereastra; o a doua rulare adauga doar ce lipseste.

## Cum se adauga o migrare noua

O subclasa `AvcStep` in `Configurari/<Modul>/` (citeste tabelul Access, verifica tinta, scrie tabelele `TZ_`) si un rand in `AvcCatalog.Create()`, in ordinea rularii.
