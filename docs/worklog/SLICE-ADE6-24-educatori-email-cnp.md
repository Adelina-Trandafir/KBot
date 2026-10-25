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
  Punctul de reluare a fost absorbit în acest jurnal și în registrul ADE6, prin D001-04.
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
  Acestea sunt limitele predării din 12.10.2026, închise prin confirmarea de mai sus.

Următoarea subfelie de catalog: **ADE6-25**.

## Predarea absorbită din punctul de reluare — D001-04

Reper inițial: 12.10.2026, ADE6-24/25; acceptare finală confirmată de operator la 23.10.2026, inclusiv DDL, sincronizare, publicare, paritate financiară/M10 pe faze Access și ADE9-02/03 (proba completă și predarea). Nu este o probă nouă efectuată în D001-04. Emailul chitanțelor rămâne inactiv și este distinct de emailurile de autentificare ale portalului. Compensarea prin consiliu, SIIR, mesajele de business, exporturile condiționate și rapoartele consolidate nu aveau puncte deschise conform aceleiași confirmări; aceasta nu inventează detalii de implementare sau rezultate individuale.

Ordinea predării: AD_09 numai în AVACONT_SURSA → sincronizarea celorlalte baze prin KBotAdmin → publicarea fișierelor din lista de mai sus și restart backend → probe educator nou/existent cu câmpuri goale, completare/golire, email/CNP invalid, reîncărcare și închidere anuală cu păstrarea contactelor. Fără remigrare; schimbarea numai a contactelor nu înlocuiește perioadele. Pentru deadlock se publică separat cele două module și HELP din [ADE6-25](SLICE-ADE6-25-deadlock-lock-row.md); proba consemnată în punctul de reluare era «Plătitori → Modifică grupa». Cauza exactă rămâne calificată în jurnalul tehnic.

Punctul de reluare acoperea grupele/copiii/părinții/taxele/prezența (ADE6-24), încasările/anulările/restituirile și listare/PDF, ciclul lunar/anual, opt rapoarte și dispoziția de plată (ADE9-01), subunitățile/migrarea, portalul AD11-01…05 și ajutorul AD12-01. Toate aveau confirmarea de mai sus. Numerotarea AD10/AD11 a fost reutilizată istoric pentru obiective distincte: identificarea se face și după titlu, fără renumerotare. Numerele propuse atunci (ADE6-26, AD11-06, AD12-02, AD13) sunt istorice; continuarea folosește registrul actual.
