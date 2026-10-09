# SLICE-ADE5-05 — oprire, jurnal SQL și invalidarea verificării în ADE.Migrator

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; nerulat, netestat pe MariaDB sau în UI.

## Ce s-a schimbat și de ce

- Buton Oprește în Designer, CancellationToken între rânduri și înainte de COMMIT;
  rollback la oprirea scrierii, fără terminarea procesului în timpul conexiunilor active.
  FormClosing așteaptă terminarea operației; citirea/testarea se așteaptă fără anulare.
- Progres pe tabel la fiecare 100 de rânduri și la final; rândurile pregătite nu sunt
  prezentate ca persistate înainte de COMMIT.
- Reutilizarea SqlDumpWriter și ValueConverter.ToLiteral din KBot.Migrator:
  jurnal pe tabel, configurația chitanțelor și AD_Imports, cu rezultat final și hash sursă.
  Înlocuirea parametrilor se face într-o singură trecere pentru a nu modifica textele
  care conțin numele altor parametri. Parola conexiunii nu intră în jurnal.
- Schimbarea fișierului/conexiunii/educatorilor invalidează verificarea; refacerea
  eșuată a planului nu păstrează planul vechi activ. Controalele sunt blocate în operații.
- Writer-ul reverifică destinația și hash-ul sursei; verifică AD_Imports și InnoDB
  inclusiv pentru configurația din baza comună. Nu schimbă schema/mapările business.
- UI distinge succesul confirmat de eroarea afișării și de rezultatul necunoscut.
  Eșecul confirmării COMMIT sau al rollback-ului nu promite o bază neschimbată.
- Erorile UI sunt afișate și înregistrate; salvarea setărilor propagă eroarea.
  Writer-ul propagă cu Throw, iar limita UI îl înregistrează. Rollback-ul eșuat
  păstrează ambele excepții. Comentarii noi în engleză; mesaje românești cu diacritice.

## Fișiere atinse

- ADECHIT/ADE.Migrator/AdeMigratorForm.vb
- ADECHIT/ADE.Migrator/AdeMigratorForm.Designer.vb
- ADECHIT/ADE.Migrator/AdeWriter.vb
- ADECHIT/ADE.Migrator/README.md — utilizare și interpretarea jurnalelor
- ADECHIT/PLAN_IMPLEMENTARE.md, ADECHIT_STATUS.md,
  docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog

## Verificări efective

- Citire statică a regulilor ADE, CODE_WORKFLOW și convenției WinForms.
- Consultate punctual exporturile MariaDB_Schema/000_DEMO.sql și AVACONT_COMUN.sql;
  AD_Imports are coloanele folosite și InnoDB în exportul DEMO. Destinația efectivă
  se verifică de utilitar, la rularea utilizatorului; exportul nu dovedește schema fiecărei unități.
- `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal`:
  build final reușit, 0 avertismente, 0 erori. O eroare intermediară de sintaxă tuple
  VB.NET a fost corectată înainte de buildul reușit.
- Fără teste automate, probe vizuale, rulare MDB sau conexiuni MariaDB, conform regulii
  de testare. Sursele Access nu au fost modificate. Fără commit, push sau DDL executat.

## Neverificat și amânat

- Utilizatorul probează citirea MDB, educatorii, blocajele, invalidarea verificării,
  oprirea înainte de COMMIT, închiderea în lucru și interpretarea jurnalelor.
- Scrierea efectivă, rollback-ul, pierderea conexiunii în COMMIT și reconcilierea pe
  MariaDB rămân neverificate. Verificarea tabelelor goale nu este o blocare concurentă
  a destinației; nu se rulează două migratoare sau scrieri web simultan pe aceeași bază.
- Citirea MDB și verificarea conexiunii nu sunt anulabile în această subfelie.
- Mapările și calculele ADE5-04 rămân de validat pe date, inclusiv lungimile textelor,
  AD_SS_Buget.Luna și conversia Plata; reconstrucția istoricului nu a fost probată.
- Numele proiectului și executabilului rămâne ADE.Migrator. Nu s-a schimbat distribuția.
