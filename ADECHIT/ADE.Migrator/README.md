# ADE.Migrator — utilizare

<!-- slice: ADE5-05 -->

Utilitar Windows separat, în stilul KBot.Migrator, pentru Access ADECHIT → MariaDB.
Executabilul păstrează numele existent `ADE.Migrator.exe`.

1. Alegeți MDB-ul și apăsați **Citește**. Această operație citește numai fișierul local.
2. Verificați numerele de rânduri, conversiile, blocajele și educatorii. Corectați educatorii
   în ultima coloană; modificarea reface planul și cere o nouă verificare a serverului.
3. Completați conexiunea și apăsați **Testează**. Destinația este baza identificată prin
   `Unitati.DC`. Tabelele de destinație trebuie să existe, să fie goale și să folosească
   InnoDB. Seria existentă de chitanțe nu se suprascrie. Se verifică și coloanele AD_Imports.
4. Apăsați **Migrează** după rezolvarea blocajelor. Utilitarul verifică din nou hash-ul
   MDB și destinația înainte de scriere. Schimbarea fișierului sau a datelor conexiunii
   invalidează verificarea precedentă. Nu folosiți simultan două utilitare pe aceeași bază.

**Oprește** solicită oprirea migrării înainte de COMMIT. Instrucțiunea SQL curentă
trebuie să se termine înainte ca solicitarea să fie observată; apoi se încearcă rollback.
După trimiterea COMMIT, oprirea nu poate anula o tranzacție confirmată.
La închiderea ferestrei în timpul unei operații, utilitarul așteaptă eliberarea conexiunilor.
Citirea și verificarea conexiunii se așteaptă până la terminare; butonul Oprește este
activ numai în timpul migrării. Nu se închid procese MSACCESS.

Progresul afișează tabelul și rândurile pregătite, la fiecare 100 de rânduri și la finalul
tabelului. Aceste rânduri sunt confirmate numai după COMMIT.

Jurnalele sunt în `Jurnale/ADE/<DC>/<data_ora>/`, lângă executabil:

- `_00_info.txt`: identificarea sursei și a bazei, fără parola conexiunii;
- fișiere `.sql`: reconstrucția comenzilor parametrizate, înregistrate înainte de execuție;
- `_99_final.txt`: COMMIT sau rollback confirmat;
- `_outcome.sql`: rezultat necunoscut dacă serverul nu a confirmat COMMIT/rollback.

Fișierele SQL pot descrie rânduri anulate prin rollback. Dacă rezultatul este necunoscut,
verificați AD_Imports și tabelele pe server înainte de reluare. O eroare de scriere a
jurnalului este înregistrată și afișată prin mecanismul comun; jurnalizarea se dezactivează,
iar tranzacția continuă conform comportamentului SqlDumpWriter din KBot.Migrator.

Schema se pregătește separat de utilizator: AD_03 și, pentru perioadele taxelor, AD_04.
Utilitarul nu execută DDL și nu inventează perioade pentru datele istorice NULL.

ADE5-05: build verificat; utilitarul, oprirea, interfața și scrierea pe MariaDB rămân
de probat de utilizator. Nu s-au rulat teste automate sau verificări vizuale.
