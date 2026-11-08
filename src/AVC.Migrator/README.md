# AVC.Migrator - utilizare

<!-- slice: AV01-05 -->
<!-- slice: AV02-03 -->

Utilitar Windows separat, pe modelul `ADE.Migrator`, pentru baza Access Avacont (`baza2026.accdb`) -> tabelele `TZ_` din baza MariaDB a unitatii.
`KBot.Migrator` nu se modifica si nu se foloseste pentru Avacont. Ordinea migrarilor: `KBot.Migrator`, apoi E-Factura, apoi `AVC.Migrator`.

## Inainte de prima rulare

1. Scriptele `sql/AVACONT/Configurari/PlanCont/AV01_04_conturi_sursa.sql`, `AV01_04b_plancont_sursasector.sql`, `AV01_04c_soldinitial_rulaje.sql` si `AV01_04d_subunitati_unitate_parinte.sql` sunt rulate pe `AVACONT_SURSA`, apoi «Sincronizare schema» le copiaza in baza unitatii. Coloanele de rulaje trebuie sa existe in MariaDB (le foloseste fereastra «Plan de conturi»), desi valorile lor nu se migreaza; verificarea le cere.
2. Completați datele subunităților în grila de unități și bifați pasul **Subunități**. Pasul creează subunitățile lipsă în aceeași migrare.
3. Bazele de date ale unitatii si cea comuna (`AVACONT_COMUN`) exista pe server.

## Pasii (ca la KBot.Migrator)

1. **Registrul**: alegeti `cale.accdb` (de obicei `C:\AVACONT\cale.accdb`), completati parola fisierelor daca sunt protejate, apoi butonul cu pictograma baza de date. Din registru vin DC-urile, unitatile si caile fisierelor fiecarei unitati.
2. **DC**: alegeti DC-ul. Este si baza MariaDB in care se scrie (se vede la «Baza-tinta»). Lista de unitati arata unitatile DC-ului; **bifati-le pe cele de transferat**. Codul fiscal este propus din registrul Windows si il puteti corecta (se scrie in antetul jurnalului SQL).
3. **Server**: gazda, port, utilizator, parola, baza comuna (`AVACONT_COMUN`).
4. **Citeste unitatile bifate**: citeste fisierele Access (`baza2026.accdb`) ale unitatilor bifate. Doar citire, nu atinge serverul.
5. **Subunități**. Grila are o linie pentru fiecare unitate a DC-ului. Bifati unitatile de portat (se pot mai multe odata); *Denumirea* vine din `cai.AlteDetalii`, *Capitolul* din `cai.SURSA` (01A = 65.01, 02A = 65.02, 02E = 65.10; un al doilea 01A, de exemplu 65.11, se completeaza de mana), *Zecimale salarii* implicit 0 - toate se corecteaza in grila. Aceleasi reguli ca in fereastra «Plan de conturi». O unitate cu `IdUnitateParinte` pastreaza legatura (citeste clasificatiile si bugetul parintelui): parintele trebuie portat inainte sau odata cu ea. Prima subunitate scrisa in baza este cea implicita (nu se alege).
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
| Operatii contabile (AV02-01) | `NoteC`, `Documente`, `Oper`, `Debit`, `Credit` din fisierul fiecarei unitati bifate | `TZ_NoteC`, `TZ_Documente`, `TZ_Oper`, `TZ_Debit`, `TZ_Credit` (anul 2026; `CreatedAt` = `DTQ` din Access; id-urile Access in coloanele `*Access`, deci o repetare adauga doar ce lipseste; sablonul `IDA` nu se duce; `AngajamentLegal` / `DataAngLeg` nu se duc) |

Pentru o migrare completă, bifați și pașii **Subunități**, **Plan de conturi** și **Jurnale** împreună cu **Operații**. Verificarea ține cont atât de datele deja existente, cât și de cele pregătite de pașii bifați. La scriere, «Operații» folosește ID-urile din aceeași tranzacție. Puteți rula pașii și separat.

Partenerii și clasificațiile trebuie migrate în prealabil cu `KBot.Migrator`. Raportul arată operațiile omise din cauza unui cont, jurnal, partener sau clasificații indisponibile. Fiecare operație eligibilă se scrie cu toate liniile ei. Legăturile facturilor: `EF_PrimiteOperatii.IdOperatie` este mereu `TZ_Oper.IdOper`; numărul din Access stă în coloana `IdOperatieAccess` (importul E-Factura din Access îl scrie acolo). Ultima instrucțiune a pasului «Operații» pune `IdOperatie` pentru toate legăturile care au număr Access (operația se găsește prin număr Access + unitate), la fiecare rulare, în orice ordine față de importul facturilor; legăturile făcute în K-BOT (`IdOperatieAccess` gol) nu se ating. Înainte de prima migrare, aplicați scripturile `sql/AVACONT/IntroducereDate/Operatii/AV02_01_operatii.sql` și `sql/00EF_66_primite_operatii_id_access.sql` conform instrucțiunilor din fișiere.

## Migrare repetată și golire

Migrarea se poate rula de mai multe ori: fiecare pas adaugă doar ce lipsește (după cheile Access păstrate în coloanele `*Access`, după simbolul contului, după codul jurnalului, după explicația șablonului) și nu modifică ce există deja. Ce s-a adăugat în Access după o rulare intră la următoarea; ce s-a modificat sau șters în Access după aceea NU se actualizează în MariaDB. Pentru a începe de la zero: KBotAdmin ▸ Comenzi ▸ AVACONT ▸ «Golire date contabile» (golește tabelele `TZ_` și `EF_PrimiteOperatii` ale bazei alese), apoi migrarea și importul E-Factura din Access.

## Pasul «E-Factura» (mutat din fila E-Factura a KBot.Migrator)

Ultimul pas al listei. Bifat **singur** inseamna «doar e-facturi»: se poate rula oricand, ca sa aduca ce e nou, fara sa atinga pasii contabili. Muta din Access in baza unitatii: datele furnizorului (`Unitati_Date`), clientii, facturile emise cu liniile lor, facturile primite cu liniile, notele si mesajele lor (doar mesajele cu codul fiscal al unitatii), legaturile facturilor primite cu operatiile contabile (`EFT_O`) si legaturile cu receptiile. Lucrul il face `EfImporter` (ramas in KBot.Migrator ca biblioteca), pe conexiunea si tranzactia migrarii: o eroare anuleaza tot.

- **Randurile existente raman cum sunt** (ce s-a lucrat in K-BOT nu se suprascrie); se adauga doar ce e nou in Access.
- **Exceptie, legaturile cu operatiile (`EFT_O`)**: cele venite din Access se sterg si se citesc din nou la fiecare rulare, ca sa fie toate (o operatie stearsa in Access nu lasa legatura). Cele facute in K-BOT raman. Fiecare legatura primeste `IdOperatie` = `TZ_Oper.IdOper` (numarul din Access sta in `IdOperatieAccess`); daca operatiile nu sunt migrate inca, se leaga la pasul «Operatii».
- **Fisiere**: furnizorul, clientii si facturile emise vin din fisierul primei unitati bifate care are fisier; cele primite din `<dosarul lui cale.accdb>\EFACTURA\ef_<an>.accdb` sau din calea scrisa in `avc-migrator-settings.json` la `EfReceivedFile`. Anul este cel al migrarii (2026): pentru alt an se scrie calea.
- Are nevoie de **codul fiscal** completat pe formular, de scriptul `sql/00EF_66_primite_operatii_id_access.sql` si de clasificatiile mutate de KBot.Migrator.
- Fila «E-Factura» din KBot.Migrator nu mai exista.

Pasul «Solduri initiale pe parteneri» are nevoie de «Plan de conturi» (aceeasi rulare sau una anterioara) si de partenerii mutati de KBot.Migrator. Un sold al carui cont sau partener lipseste din tinta NU se scrie si este numit in verificare (ATENTIE); celelalte se scriu.

Conturile fiecărei unități intră în subunitatea ei, identificată prin `TZ_Subunitati.IdUnitateAccess`. Puteți transfera mai multe unități odată. Dacă o subunitate lipsește din țintă și pasul «Subunități» nu o poate crea în rularea curentă, verificarea raportează un blocaj.

Un cont cu acelasi simbol ca unul existent ramane asa cum l-ati editat in fereastra; o a doua rulare adauga doar ce lipseste.

## Cum se adauga o migrare noua

O subclasa `AvcStep` in `Configurari/<Modul>/` (citeste tabelul Access, verifica tinta, scrie tabelele `TZ_`) si un rand in `AvcCatalog.Create()`, in ordinea rularii.
