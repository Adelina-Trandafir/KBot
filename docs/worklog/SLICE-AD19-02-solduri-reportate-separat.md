# SLICE-AD19-02 — Solduri reportate din anii anteriori, separat de SI existent

04.11.2026. Adelina a precizat că există deja un rând de sold inițial și a cerut un tabel AD_ separat pentru soldul din alți ani. Această continuare înlocuiește varianta AD19-01 care ar fi rescris AD_Platitori.SI. Nu se reia migrarea și nu se modifică soldul inițial existent.

## Tabel și proveniență

DDL-ul este numai pregătit în `sql/AD_19_02_solduri_reportate.sql`, fără execuție. Tabelul nou `AD_SOLDURI_REPORTATE` are cheia (`SUBUNIT_ID`, `IDP`, `AN_START`), `SOLD` semnat BIGINT, proveniență (`SOURCE_IDP`, `SOURCE_HASH`, `SOURCE_FILE`), `VERSION` și momentele creării/actualizării. Are FK către copilul existent din aceeași subunitate și controlul intervalului anului. Nu există seed, DDL în aplicație sau modificări ale exportului MariaDB_Schema.

Utilizatorul aplică manual SQL-ul pe AVACONT_SURSA și propagă schema prin mecanismul existent către bazele unităților. Migratorul cere tabela InnoDB înainte de transfer. În unitățile unde noua tabelă nu este încă instalată, citirea Python întoarce explicit lipsa componentei opționale; numai eroarea de tabel inexistent este tratată astfel. Erorile de permisiuni, coloane sau conexiune se propagă la limita comună. Nu se creează tabele automat.

## Soldul și migratorul

Eligibilitatea, asocierea prin AD_IdMap, citirea MDB-ului complet și validările de identitate/perioadă din AD19-01 se păstrează. Se determină întâi soldul complet înainte de 2021. Din el se scade SI original de bază din MDB; doar diferența devine `SOLD` în tabela nouă, astfel încât SI să nu fie numărat de două ori. Pentru copilul dispărut din Platitori în MDB se poate folosi prima situație salvată în 2021, cu scăderea mișcărilor deja aferente lui 2021; această recuperare este permisă numai dacă SI existent este zero, cum a fost reconstruit la migrare.

Dacă SI existent diferă de baza din MDB, rândul este blocat pentru clarificare. Nu se presupune că reportul lipsește și nu se încearcă automat corectarea/reversarea unui SI deja schimbat. Situațiile salvate neconcordante și mișcările relevante fără lună rămân blocaje explicate pe rând.

Previzualizarea arată SI existent (păstrat), reportat existent, reportat propus, diferența reportării, SI + reportat, originea și starea. O înregistrare lipsă rămâne vizibilă ca celulă goală pentru Reportat existent; un report propus zero fără rând existent nu generează o înregistrare inutilă. SI și reportarea se afișează numeric, copiii alfabetic în limba română.

Scrierea folosește numai INSERT/UPDATE în AD_SOLDURI_REPORTATE. Cheia unică evită dublarea la repetare; valoarea existentă se înlocuiește după verificarea soldului/versiunii, fără incrementarea cumulativă a sumei. Pentru un report identic nu se scrie din nou. Sunt păstrate verificările SHA-256 ale sursei, recitirea previzualizării sub blocările existente și amprenta datelor aprobate. Versiunea și SI-ul copilului sunt doar citite, fără UPDATE în AD_Platitori.

Lotul eligibil este o tranzacție, cu jurnal SQL prin SqlDumpWriter. Erorile anterioare COMMIT provoacă rollback integral. COMMIT/rollback neconfirmat este raportat explicit. Nu sunt modificate SS_Buget, prezența, încasările, restituirile, documentele, harta ID-urilor, AD_Imports sau fluxul migrării complete.

## Consum în ADECHIT

Repository citește componenta separat, limitată la subunitate și la copil când este cazul. Datele complete, cele pentru grupa aleasă și cele ale copilului din portal primesc `CarriedBalances`; tabela nu este adăugată în catalogul generic editabil sau în tabelele importate din MDB.

Calculul lunar folosește `SI existent + suma reportărilor aplicabile + istoricul contractelor − plăți + restituiri`. O reportare este aplicabilă când AN_START este cel mult anul lunii calculate; se adaugă o singură dată la soldul inițial cumulativ. În lipsa reportărilor, rezultatul anterior se păstrează. Închiderea lunii și rapoartele/ecranele care folosesc același calcul primesc componenta prin același flux. Situațiile deja salvate ale lunilor închise continuă să fie citite, fără rescriere în acest task.

Fișa cont operator (web/K-BOT) păstrează rândul Sold Inițial și adaugă distinct Sold reportat din anii anteriori la 01.01.AN_START. Fișa părinților folosește același principiu; un interval care începe ulterior include reportarea în soldul de deschidere, iar un interval care o conține o afișează pe propriul rând. PDF și Excel folosesc modelele existente. Nu s-au schimbat formulele documentelor sau alte aplicații.

## Fișiere proprii create/modificate

1. `sql/AD_19_02_solduri_reportate.sql` — DDL nou, numai pregătit.
2. `PYTHON/routes/adechit/carried_balances.py` — helper nou pentru componentă și rândurile fișelor.
3. `PYTHON/routes/adechit/repository.py` — citire opțională/scopată în datele calculului.
4. `PYTHON/routes/adechit/calculations.py` — componenta suplimentară a soldului inițial.
5. `PYTHON/routes/adechit/reports.py` — rând separat în fișa cont operator.
6. `PYTHON/routes/adechit/parent_statement.py` — citire și rând separat în fișa copilului.
7. `ADECHIT/ADE.Migrator/AdeOpeningBalanceSource.vb` — reținerea SI original pentru separarea componentei.
8. `ADECHIT/ADE.Migrator/AdeOpeningBalanceTransfer.vb` — transfer exclusiv în tabela nouă, SI păstrat.
9. `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.vb` — previzualizare și mesaje pentru report separat.
10. `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.Designer.vb` — coloanele SI/report/total.
11. `ADECHIT/ADE.Migrator/AdeMigratorForm.vb` — mesajul de eroare al comenzii.
12. `ADECHIT/ADE.Migrator/AdeMigratorForm.Designer.vb` — denumirea comenzii.
13. `ADECHIT/ADE.Migrator/README.md` — fluxul actual, DDL și limite.
14. `ADECHIT/CONTRACT_CALCULE.md` — extensia explicită a componentei inițiale.
15. `ADECHIT/UTILIZARE_WEB.md` — prezentarea reportului în fișă și calcul.
16. `ADECHIT/PORTAL_PARINTI.md` — reportul în fișa părinților și intervale.
17. `PYTHON/static/help/adechit.json` — instrucțiuni pentru rândul separat.
18. `src/KBot.App/HelpContent/contabil/adechit.md` — instrucțiuni pentru fișa cont K-BOT.
19. `ADECHIT_STATUS.md` — starea curentă AD19-02.
20. `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` — înlocuirea proiectării AD19-01 și starea AD19-02.
21. `docs/worklog/SLICE-AD19-02-solduri-reportate-separat.md` — acest worklog.

Motiv concret pentru înregistrarea celor 21 de fișiere: «Păstrarea SI existent și stocarea distinctă a soldurilor reportate din anii anteriori în AD_SOLDURI_REPORTATE, cu DDL manual, transfer separat în ADE.Migrator și includere unică în calculele și fișele cont ADECHIT.»

## Stare și limite

SCRIS LOCAL, NECOMPILAT, NETESTAT, NEPUBLICAT. S-au citit sursele și coloanele/cheile existente din exportul local MariaDB_Schema/000_DEMO.sql pentru proiectarea relației noi. Nu s-au rulat teste, compilări, verificări automate sau vizuale, aplicația, SQL, conexiuni la server, citiri ale datelor reale MDB, Git, publicare sau restart. Nu se declară că schema sau datele au fost modificate pe server.

Ordinea pentru operator: DDL pe AVACONT_SURSA → sincronizarea schemei către unități → publicarea modulelor Python → compilarea/distribuirea migratorului → previzualizare și salvare în tabela nouă → regenerarea rapoartelor. Proba trebuie să confirme SI și versiunea păstrate, report de debit/credit/zero, SI de bază nenul fără dublare, repetare, asociere greșită/blocată, SI deja modificat, concurență, rollback, calcul pe o grupă/toate și rândurile/intervalele fișelor cont. Nu s-au realizat capturi.

## Înregistrare

Adelina a confirmat explicit înregistrarea la 04.11.2026: «inregisreaza daca trebuie». Confirmarea autorizează înregistrarea celor 21 de fișiere enumerate în SLICE-AD19-02, cu motivul concret indicat, fără fișiere ale altor taskuri ori ale registrului. Nu reprezintă o confirmare de testare sau publicare; starea NECOMPILAT, NETESTAT și NEPUBLICAT se păstrează. Scriptul de înregistrare este executat după această actualizare documentară, iar rezultatul efectiv este raportat în chat și, la succes, în registrul imuabil docs/deployment/pending. Închiderea din perspectiva registrului depinde de succesul scriptului.
