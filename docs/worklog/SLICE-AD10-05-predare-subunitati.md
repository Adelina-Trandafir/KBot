# ADE10-05 — Predare: subunități în același DC

Pachet pentru [planul subunităților](../../ADECHIT/plan_subunitati.md). Nimic nu a fost publicat, rulat pe
server sau testat din acest chat; commit și push nu s-au făcut.

## Ordinea de punere în funcțiune (coordonată, cu scrierile suspendate)

1. Copie de siguranță a fiecărei baze ADE (și a bazei-șablon `000_DEMO`). Nimeni nu lucrează în ADECHIT și nu
   rulează ADE.Migrator.
2. `sql/AD_05_subunitati.sql` pe fiecare bază, după ce se editează `@ade_subunit_name` (numele subunității
   inițiale a datelor existente). Se citesc apoi interogările de control de la finalul scriptului.
3. Publicare prin AvacontPush: `PYTHON/routes/adechit/*` (repository, __init__, service, importer, receipt_output,
   schema.json), `PYTHON/templates/adechit/receipt.html`, `PYTHON/static/adechit.html`,
   `PYTHON/static/js/adechit/app.js`, `PYTHON/static/css/adechit.css`; restart backend.
4. ADE.Migrator nou (build din `ADECHIT/ADE.Migrator`) pentru importurile următoare.

După pasul 2 aplicația veche și migratorul vechi nu mai sunt compatibile; revenirea cere restaurarea
coordonată a bazei și a aplicației (plan §9).

## Ce probează utilizatorul (scenariile din plan §10 și completările §12)

1. Importul `baza_40.mdb` și `baza_47.mdb` în două subunități ale aceluiași DC (alegând DC-ul părinte și
   denumirile): ID-uri Access comune, luni comune, legături rămase în sursa lor. 40 = zero chitanțe, contor 6715;
   47 = seria G.R., contor 10199.
2. Comutarea subunității (și două file, A și B); anii, lunile, grupele, copiii, taxele și soldurile se schimbă.
3. Citire și modificare cu un ID din cealaltă subunitate → 404; legătură între subunități → refuzată și de bază.
4. Emitere chitanță în A și în B: serie/contor independente; anulare și reîncercare nu mută contorul celeilalte.
5. Emitere concurentă în aceeași subunitate fără numere duble; aceeași cheie de reîncercare în A și B.
6. Închidere lunară/anuală, redeschidere, recalculare — numai în subunitatea curentă.
7. Regula temporală a documentelor, înainte și după remapare, inclusiv istoric `SS_Buget`/`Retur` cu luni lipsă.
8. Conversia unui DC existent: date, documente, solduri, serie; PDF-ul numește subunitatea.
9. Probă locală: `python ADECHIT/tools/preview.py --second-subunit` (localhost:5050) pentru comutare fără server.

## Decizii luate aici (pe linia planului; de infirmat dacă nu convin)

- Antet lipsă = acceptat numai cu o singură subunitate activă în DC (pagini vechi; unități cu o evidență).
- `ON DELETE SET NULL` → `RESTRICT` cu golirea legăturilor în cod (compus nu poate SET NULL fără a goli `SubunitId`).
- `AD_Lock.ID` = SubunitId; `AD_Imports` rămâne unic pe `SourceHash` la nivel de DC.
- `Ordine` păstrează golurile de IDL din sursă; comparațiile de solduri rămân pe IDL, cu importul în ordinea Access.
- Subunitatea nu se creează din interfața web; migratorul sau SQL.

## Teste

Niciunul rulat. `dotnet build` ADE.Migrator curat; `ast.parse` pe Python. Teste existente adaptate fără rulare:
`test_adechit.py`, `adechit_lazy.test.mjs`.

## Neverificat / amânat

Tot ce depinde de MariaDB, de server și de ecran. Amânat explicit (plan §11): importuri istorice într-o
subunitate populată, restricții individuale pe subunitate, rapoarte consolidate, administrarea subunităților din web.

## Actualizare după review (ADE10-06)

Fișiere de publicat suplimentar față de lista de mai sus: `PYTHON/routes/schema_sync/schema_diff.py` (corectură de sincronizare) și versiunea nouă a
`PYTHON/routes/adechit/*` și `app.js`. `sql/AD_05_subunitati.sql` conține acum și `AD_SubunitAccess`; dacă varianta veche nu a fost rulată pe nicio bază,
se rulează doar cea nouă. Restart backend după publicare. Acces pe subunitate: `INSERT INTO AD_SubunitAccess (SubunitId, Email) VALUES (...)`; atenție,
primul rând pe o subunitate o închide pentru toți ceilalți.
