# SLICE-AD11-03 — conflicte istorice și query AvacontPush

10.10.2026. SCRIS LOCAL; MariaDB și interfața nu au fost probate.

La cererea utilizatorului, conflictele CNP/email existente nu opresc importul.
Contactele sursă rămân intacte; identitățile ambigue au Email NULL în registrul
portalului. Fiecare cerere de portal reverifică ambiguitatea în întregul DC,
inclusiv sesiunile deja conectate și trimiterea codului permanent.

Plătitori afișează o avertizare separată de mesajele tranzitorii, cu lista
înregistrărilor afectate din subunitatea curentă. Click selectează grupa,
copilul și părintele și deschide editorul. La salvare lista se actualizează.
Introducerea unor conflicte noi rămâne refuzată; un conflict existent și
nemodificat nu împiedică salvarea celorlalte date ale părintelui.
Corectarea emailului unui CNP sincronizează asocierile sale și reface registrul.

`sql/AD_08_portal_parinti.sql` conține numai DDL pentru AVACONT_SURSA, apoi
AvacontPush sincronizează schema. `sql/AD_08_02_interogare_unica.sql` conține
instrucțiuni simple pentru Interogări unice, nume AD_08_portal_parinti_date:
tabel temporar de agregare, dezactivarea contactelor ambigue, coduri aleatorii
numai unde lipsesc și există copil fără Plecat, propagare pe asocieri, lock 0.
Fără DELIMITER/proceduri, fără trimitere email. Queryul păstrează codurile existente.
Scrierile ADE se suspendă pe durata queryului. Niciun SQL remote nu a fost executat.

Migratorul VB și importerul Python aplică aceeași regulă de tolerare a conflictelor.
Actualizate parent_identity/parent_portal/catalog_forms, API, payers.js, HTML/CSS,
AdeWriter și ghidurile portalului, migratorului și utilizării web.

Verificări efective: py_compile pe modulele modificate, node --check payers.js,
dotnet build ADE.Migrator --no-restore: 0 erori și 0 avertismente.
Preview-ul local a fost repornit; GET /adechit/preview/parents răspunde HTTP 200.
Fără teste automate funcționale sau verificări vizuale, conform preferinței utilizatorului.
Rămân proba localhost, sincronizarea schemei, queryul, importul și loginul pe MariaDB.

Următoarea subfelie liberă a portalului: AD11-04.
