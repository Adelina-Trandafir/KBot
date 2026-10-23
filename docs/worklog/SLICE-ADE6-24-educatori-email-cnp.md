# SLICE-ADE6-24 — Email și CNP opționale pentru educatori

12.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Ultima intervenție înaintea opririi momentane
a lucrului la ADECHIT solicitate de utilizator.

## Ce s-a schimbat și de ce

- Fiecare rând/perioadă din `AD_Grupe_Educator` are `EMail` și `CNP`, opționale.
  Editorul grupei are două coloane noi de text și lățime maximă 1120px (96vw).
  Datele sunt incluse în payloadul salvării atomice, cu verificarea versiunilor.
- API-ul verifică formatul emailului și validarea CNP existentă numai dacă sunt
  completate. JS folosește aceeași regulă CNP; CNP rămâne text. Golirea salvează NULL.
  Datele educatorilor nu sunt înregistrate ca identități de părinte în portal.
- Închiderea anuală preia și permite editarea contactelor; păstrează datele când
  înlocuiește perioadele. Modificarea numai a contactelor actualizează rândurile
  existente aplicabile la 1 septembrie. Contractul vechi cu nume simple rămâne
  acceptat și păstrează contactele educatorilor cu același nume.
- **Instrucțiune explicită:** Access NU conține coloanele; nu se așteaptă migrarea
  lor. `AdeSchema.GrupeEducator` are doar IDGE/IDG/Educator/DeLa/PanaLa;
  `AdePlan.AddEducatorRow` și writerul cu lista explicită de coloane rămân
  neschimbate. Câmpurile noi rămân NULL implicit la import; fără remigrare.
- DDL separat: `sql/AD_09_educatori_contact.sql`, numai în **AVACONT_SURSA**.
  **KBotAdmin** face sincronizarea celorlalte baze, conform cererii utilizatorului.
  Scriptul este repetabil pentru adăugarea coloanelor și nu modifică datele existente.
- HELP: conținutul JSON actualizat pentru catalog și închidere anuală, cu referință
  `slice: ADE6-24` în metadatele blocurilor; imprimarea/PDF folosesc aceeași sursă.
- Ghidul web, planul, README migratorului, indexul și stările au fost actualizate.
  `STARE_LA_PAUZA.md` păstrează livrabilele, DDL-ul, fișierele de publicat și restanțele.
  AD12 avea deja AD12-01; următorul număr principal liber este corectat la AD13.

## Fișiere atinse

- `PYTHON/routes/adechit/{schema.json,service.py,catalog_forms.py,annual.py}`.
- `PYTHON/static/js/adechit/{payers.js,annual.js}`, `PYTHON/static/css/adechit.css`,
  `PYTHON/static/help/adechit.json`.
- `sql/AD_09_educatori_contact.sql` (nou).
- `ADECHIT/{UTILIZARE_WEB.md,PLAN_IMPLEMENTARE.md,STARE_LA_PAUZA.md (nou)}`,
  `ADECHIT/ADE.Migrator/README.md`, `ADECHIT_STATUS.md`, cele două fișiere de stare.
- Acest worklog.

## Verificări efective

- Schema locală reală citită: `MariaDB_Schema/AVACONT_SURSA.sql`, tabelul
  `AD_Grupe_Educator` are Educator/DeLa/PanaLa și nu are EMail/CNP în export.
- Parse Python AST: service/catalog_forms/annual fără erori; JSON schema/HELP valide.
- `node --check` pentru payers.js și annual.js fără erori; `git diff --check` curat
  pentru fișierele intervenției.
- Citire punctuală a migratorului: coloanele de inserare nu includ contactele.
- Nu s-au rulat teste automate, probe UI sau conectări MariaDB; testarea funcțională
  rămâne la utilizator, conform preferinței proiectului. Sintaxa nu validează fluxul.

## Neverificat / amânat

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- DDL nerulat, KBotAdmin/publicare/restart neexecutate din chat.
- Salvare/golire, erori de validare, conflict, grila PC/mobil și planul anual neprobate.
- HELP/imprimare/PDF nu au fost deschise pentru verificare vizuală.
- Acceptarea finală, paritatea Access și probele server rămân neconfirmate;
  detaliile sunt în [punctul de reluare](../../ADECHIT/STARE_LA_PAUZA.md).

Următoarea subfelie de catalog: **ADE6-25**.
