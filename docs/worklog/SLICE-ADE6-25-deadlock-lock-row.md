# SLICE-ADE6-25 — deadlock la deschiderea grupei

12.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Eroare raportată de utilizator pe server (deadlock AD_Lock).

## Ce s-a schimbat și de ce

Logul primit arată MariaDB 1213 la `SELECT ID FROM AD_Lock WHERE ID=0 FOR UPDATE`,
înaintea citirii anilor/educatorilor și a verificării dreptului operației.
Cererea are contextul utilizator/DC; nu este un refuz al certificatului FOREXE.
`editGroup` trimite în paralel `/years` și `/rows/Grupe_Educator`; ambele trec
prin același context tranzacțional și același rând de blocare globală.

Cauză probabilă din cod: `INSERT IGNORE` pe cheia existentă, apoi conversia
blocării la exclusiv prin `FOR UPDATE`. Două tranzacții pot ajunge să aștepte
reciproc această conversie. Mecanismul blocării shared la duplicate insert și
exclusive la ON DUPLICATE KEY UPDATE este descris în
[documentația InnoDB](https://dev.mysql.com/doc/refman/8.0/en/innodb-locks-set.html).
Raportul `LATEST DETECTED DEADLOCK` nu a fost furnizat; perechea exactă de
tranzacții de pe server nu este confirmată doar prin stack trace.

- `Repository.acquire_lock`: pe MariaDB folosește INSERT ... ON DUPLICATE KEY
  UPDATE ID=ID, care solicită blocarea exclusivă direct; păstrează și FOR UPDATE.
  Ordinea rămâne AD_Lock(0), apoi rândul subunității, apoi operațiile business.
  SQLite păstrează INSERT OR IGNORE în BEGIN IMMEDIATE.
- Nu se reexecută automat comenzile business; tranzacția păstrează rollbackul
  și propagarea excepției existente. Migratorul are deja lock 0 înaintea
  operațiilor pe subunitate; nu a fost modificat.
- Guard: logul include metoda/ruta cererii pentru a distinge citirile paralele.
  Succesul se loghează după construirea răspunsului; linia de succes din logul
  primit este compatibilă cu cealaltă cerere, nu dovedește succesul formularului.
- Deadlock 1213 / timeout 1205 primesc HTTP 503 DATABASE_BUSY, cu mesaj pentru
  încărcare sau salvare. Citirile nu mai afișează „aceeași cheie”; scrierile
  păstrează instrucțiunea de reîncercare cu aceeași cheie.
- HELP și ghidul explică reluarea încărcării și păstrarea formularului la salvare.
  Fără schimbări de certificat, autentificare, drepturi, DDL sau migrare Access.

## Fișiere atinse

`PYTHON/routes/adechit/{repository.py,__init__.py}`,
`PYTHON/static/help/adechit.json`, `ADECHIT/{UTILIZARE_WEB.md,STARE_LA_PAUZA.md}`,
`ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`, acest worklog.

## Verificări efective

- Citite exporturile locale AVACONT_SURSA/000_DEMO: AD_Lock are PK pe ID, InnoDB.
- Python AST pentru cele două module și JSON HELP: fără erori de sintaxă.
- `git diff --check` pentru fișierele intervenției: fără erori.
- Fără teste automate sau browser, conform preferinței proiectului; fără
  conectare MariaDB/SSH/publicare/restart. SQLite nu ar reproduce acest deadlock.

## Ce rămâne de probat

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

Utilizatorul publică cele două module backend și HELP, repornește serviciul și
repetă deschiderea grupei, inclusiv cu mai multe cereri/file simultane.
Dacă reapare, furnizează blocul `LATEST DETECTED DEADLOCK` din
`SHOW ENGINE INNODB STATUS;`, capturat imediat după incident, și ruta din log.
Raportul indică exact cealaltă tranzacție și blocările implicate.

Nu este necesar SQL suplimentar pentru această corecție. DDL-ul educatorilor
din ADE6-24 rămâne separat: AD_09 numai pe sursă, apoi KBotAdmin.

Următoarea subfelie: **ADE6-26**.
