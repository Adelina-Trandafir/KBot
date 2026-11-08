# SLICE-AD20-15 — Ștergere manuală a plăților preluate din extrase

Data: 05.11.2026.

## Cerere și limită

Adelina a cerut o interogare SQL pentru baza curentă, ca plățile preluate greșit din extrase să poată fi preluate din nou, păstrând asocierile corecte cu plătitorii. La clarificarea despre lunile închise a precizat: «sunt doar din luna in curs».

Scriptul este fixat explicit pe **noiembrie 2026**, luna curentă la data cererii, pentru toate subunitățile bazei selectate. Selecția folosește luna contabilă `AD_LunaD` legată prin `AD_EXTRASE_ASOCIERI.IDL`, fără a substitui data operațiunii sau a antetului extrasului. Nu se execută de agent și nu se integrează automat în aplicație.

## Livrabil și comportament

`sql/AD_20_15_reset_plati_extrase_luna_curenta.sql` este un fișier complet, de rulat manual într-o sesiune nouă de client MariaDB/HeidiSQL cu suport `DELIMITER`, după selectarea bazei unității. Ferestrele ADECHIT pentru asociere și plăți trebuie închise înainte și redeschise după reușită, pentru reîncărcarea datelor și cereri noi.

Într-o singură tranzacție, sub lacătul comun `AD_Lock.ID=0`, reține exact înregistrările de proveniență din luna cerută, apoi șterge:

1. Repartizările din `AD_EXTRASE_ASOCIERI`, pentru eliberarea încasărilor la repreluare.
2. Documentele lor din `AD_AlteDoc`.
3. Plățile lor din `AD_Plati`.

Păstrează legăturile copil–plătitor, istoricul și datele bancare ale plătitorilor, prezența, plățile introduse manual, chitanțele, extrasele originale și istoricul încărcărilor. Nu modifică `AD_Operations` sau situațiile salvate. Nu resetează contoarele și nu dezactivează cheile străine. Nu adaugă schemă persistentă; folosește doar o tabelă temporară de selecție.

Se oprește înaintea ștergerilor dacă luna este închisă, există situații salvate afectate în luna vizată ori ulterior, lipsesc legături sau o plată are alte documente atașate. Numărul ștergerilor trebuie să coincidă pentru toate cele trei tabele; orice excepție după pornirea tranzacției produce rollback. La reușită execută **COMMIT**, apoi afișează baza, perioada și numerele înregistrărilor șterse/eliberate. Rularea nu este o simplă previzualizare.

## Analiză de proiectare și limite

Surse consultate: exportul local `MariaDB_Schema/AVACONT_SURSA.sql`, fluxul `bank_associations.py`, emiterea documentelor, calculul lunilor și lacătele repository-ului ADECHIT. În luna deschisă, situația curentă se calculează din plățile existente; rescrierea situațiilor închise depășește cererea și este refuzată de script.

Sintaxa blocului anonim și comportamentul tabelelor temporare au fost consultate în documentația oficială MariaDB: [blocuri în afara procedurilor](https://mariadb.com/docs/server/reference/sql-statements/programmatic-compound-statements/using-compound-statements-outside-of-stored-programs), [instrucțiuni cu commit implicit și excepția tabelelor temporare](https://mariadb.com/docs/server/reference/sql-statements/transactions/sql-statements-that-cause-an-implicit-commit). Consultarea surselor nu reprezintă probă de execuție pe baza utilizatoarei.

**SCRIS LOCAL; SQL NERULAT; NEÎNREGISTRAT.** Nu s-au rulat teste, compilare, verificări automate/vizuale, SQL sau acces direct la baze, publicare ori Git. Nu există confirmare a utilizatoarei pentru această intervenție; înregistrarea așteaptă confirmarea explicită. Validările anterioare AD20-01–14 rămân distincte.

## Fișiere proprii și motive pentru înregistrarea ulterioară

- `sql/AD_20_15_reset_plati_extrase_luna_curenta.sql`: ștergere manuală atomică a plăților provenite din extrase, numai în noiembrie 2026, cu păstrarea plătitorilor și eliberarea repreluării.
- `ADECHIT/UTILIZARE_WEB.md`: instrucțiuni de utilizare a scriptului, perioadă și date păstrate.
- `ADECHIT_STATUS.md`: starea noii intervenții, separată de versiunile validate anterior.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul AD20-15 și următoarea subfelie liberă AD20-16.
- `docs/worklog/SLICE-AD20-15-reset-plati-extrase-luna-curenta.md`: cererea, selecția, tranzacția, sursele și limitele predării.
