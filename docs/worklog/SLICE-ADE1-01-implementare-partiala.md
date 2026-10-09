# SLICE-ADE1-01 — checkpoint implementare parțială ADE1–ADE8

Data: 08.10.2026. Stare: **ÎN LUCRU, NEPREDAT, NETESTAT FUNCȚIONAL**.

## Motiv și întindere

Implementarea integrală cerută a început, apoi utilizatorul a cerut oprirea și predarea
unui checkpoint pentru continuare într-un thread nou. Acest document consemnează exact
codul rămas în lucru. Rapoartele, PDF-urile și tipăririle bazate pe rapoarte au fost excluse.

## Scris parțial

- ADE1: blueprintul `routes/adechit`, înregistrarea în `main.py`, pagina HTML/CSS și un
  launcher local SQLite la 5050. Fișierul obligatoriu `static/js/adechit/app.js` lipsește;
  pagina nu este funcțională.
- ADE2: editare opt-in în DataGrid, cu text/număr/dată/listă, Enter/Tab/Escape, salvare
  asincronă, eroare păstrată în editor, EventBus, ListenerTracker și registry. Nu a fost
  probată în DOM și poate conține regresii.
- ADE3: generator de schemă, `schema.json`, propuneri `AD_01_schema.sql` și
  `AD_02_chitante.sql`; `schema_sync` ignoră la DROP tabelele `AD_` și
  `Unitati_Chitante`. SQL-ul nu a fost rulat sau validat pe MariaDB.
- ADE4: guard bazat pe sesiunea portalului, verificare unitate și tabel propus de operații.
  Matricea reală de roluri și granturile SQL nu sunt finalizate.
- ADE6: servicii parțiale pentru cataloage și prezență. Transferurile sunt refuzate explicit
  până la decizia M06; nu există UI funcțional.
- ADE7: traducere parțială a fazelor `mdl_Situatie`, rezultate intermediare și închidere.
  Paritatea Access nu este demonstrată. Redeschiderea este încă refuzată de endpoint.
- ADE8: emitere parțială chitanță/alt document/restituire, numerotare blocată tranzacțional,
  idempotency și rescrierea snapshotului. Anularea restituirii este refuzată până la M03.

Fișiere ADE create/modificate în intervenție:

- `PYTHON/routes/adechit/{__init__,calculations,domain,repository,service}.py`
- `PYTHON/routes/adechit/schema.json`
- `PYTHON/static/adechit.html`, `PYTHON/static/css/adechit.css`
- `PYTHON/static/js/dgv/editing.js`, `datagrid.js`, `PYTHON/static/css/dgv.css`
- `PYTHON/static/js/portal/messages.js`, `PYTHON/utils/logger.py`
- `PYTHON/main.py`, `PYTHON/routes/schema_sync/schema_diff.py`
- `ADECHIT/tools/build_schema.py`, `ADECHIT/tools/preview.py`
- `sql/AD_01_schema.sql`, `sql/AD_02_chitante.sql`

Worktree-ul conține și schimbări e-Factura anterioare/nelegate; nu au fost modificate sau
curățate pentru acest checkpoint.

## Decizii primite la 08.10.2026

- M02: orice modificare asupra documentelor unei luni închise, inclusiv anularea, rescrie
  situația salvată a copilului; prezența rămâne blocată.
- M04: se păstrează condiția VBA `luna diferă AND anul diferă`.
- M05: numai ultima lună închisă se poate redeschide; luna deschisă următoare se elimină.
  Mișcările ei rămân orfane și păstrează separat luna/anul de proveniență, fără deducere din
  data documentului, pentru reatașare la recreare. Setarea per bază poate bloca redeschiderea
  dacă există mișcări și verifică atât luna redeschisă, cât și luna eliminată.

Încă lipsesc: M03, dacă motivul anulării unei restituiri se salvează obligatoriu, și M06,
regulile exacte pentru efectul schimbării grupei/transferului asupra catalogului, prezenței
curente și istoricului.

## Verificări efective

- `git diff --check`: fără erori de whitespace; numai avertismente CRLF.
- `node --check` pentru `editing.js`, `datagrid.js` și `messages.js`: trecut.
- Citire MDB doar-citire: Grupe 11, Platitori 653, LunaD 2, Prezenta 1214, Plati 380,
  Retur 69, SS_Buget 1214, CFGs 30. CFGs/CH a returnat seria GR40, numărul 783 și șablonul
  `C/Val. luna [LA] conf. contract`; acestea sunt date reale citite, nu seed-uri.
- Proba punctuală ACE `CLng`: 0.49→0, 0.50→0, 0.51→1, 1.50→2, -0.50→0, -1.50→-2.
  Comanda s-a terminat ulterior cu eroare nativă a driverului; rezultatul nu validează motorul.
- Nu exista proces MSACCESS la verificările efectuate; nu s-a încercat oprirea vreunuia.

## Neverificat și riscuri

- Mediul `PYTHON/.venv` indică `D:\PYTHON313\python.exe`, care nu există. `py -3.13`
  raportează „No installed Python found”; compilarea și testele Python nu au rulat.
- Preview-ul `localhost:5050` nu a pornit. Nu s-au făcut teste HTTP, browser, persistență,
  sesiune, drepturi, concurență, idempotency sau regresii DataGrid.
- `static/js/adechit/app.js` lipsește; HTML-ul îl solicită și interfața este nefuncțională.
- Schema și SQL-ul sunt propuneri nevalidate. Importul/reconcilierea ADE5 nu sunt implementate.
- Calculele nu sunt validate 1 la 1 cu rezultatele Access. Tratarea NULL, joinurile, tipurile,
  ordinea conversiilor și snapshoturile trebuie auditate înainte de continuare.
- Nu s-a folosit Linux/MariaDB, nu s-a publicat și nu s-a testat serverul.

## Ordinea exactă de reluare

1. Auditează toate fișierele de mai sus înainte de a continua; nu presupune că schița este corectă.
2. Repară sau recreează mediul Python al proiectului și rulează `py_compile`/testele unitare.
3. Finalizează ADE1/ADE2: creează `static/js/adechit/app.js`, pornește 5050 și probează
   DataGrid editabil plus regresiile read-only.
4. Revizuiește schema/DDL față de exportul MariaDB și implementează importul/reconcilierea ADE5.
5. Aplică M05 în model și serviciu; obține M03 și M06 înaintea funcțiilor dependente.
6. Construiește fixture-uri din MDB și comparatorul pe faze; nu declara ADE7 valid până la
   zero diferențe numerice neexplicate față de Access.
7. Finalizează operațiile ADE6/ADE8 și testele locale. Rapoartele rămân pentru ultima etapă.
