# ADECHIT — punct de reluare

12.10.2026, SLICE-ADE6-24/25. Rezumat pentru oprirea momentană a lucrului solicitată
de utilizator. Confirmat de operator, 23.10.2026: tot ce este descris mai jos este TESTAT, RULAT, PUBLICAT.

## Ultima intervenție

- **ADE6-25:** după predare, utilizatorul a raportat pe server deadlock 1213
  la deschiderea grupei. Blocarea comună AD_Lock folosea INSERT IGNORE urmat
  de FOR UPDATE pe același rând, inclusiv pentru citirile paralele. Corecția
  locală folosește blocare exclusivă de la inserare; fără DDL suplimentar.
  Publicat (`PYTHON/routes/adechit/{repository.py,__init__.py}` și HELP JSON) și probat
  („Plătitori → Modifică grupa”): TESTAT, RULAT, PUBLICAT.

- Email și CNP opționale pentru fiecare educator, în formularul grupei și în
  pregătirea noului an. Se păstrează ca text; golirea se salvează NULL.
- Validarea emailului și CNP-ului se aplică numai când sunt completate.
  Salvarea grupei/perioadelor rămâne atomică și verifică versiunile.
- Contactele existente se preiau în planul anual și se păstrează dacă se schimbă
  lista educatorilor. Schimbarea numai a contactelor nu înlocuiește perioadele.
- HELP-ul din aplicație, imprimarea și PDF-ul folosesc conținutul JSON actualizat;
  ghidul web, planul și README-ul migratorului descriu aceeași regulă.
- **Access nu conține aceste coloane. Migratorul nu le așteaptă și nu le importă.**
  Inserările existente omit câmpurile noi, care rămân NULL. Fără remigrare necesară.

## Ce se aplică acum

1. Rulezi [AD_09_educatori_contact.sql](../sql/AD_09_educatori_contact.sql) numai
   în **AVACONT_SURSA**. Adaugă două coloane nullable; nu modifică datele existente.
2. **KBotAdmin** sincronizează schemele celorlalte baze, conform instrucțiunii
   utilizatorului. Acest DDL nu se rulează manual pe fiecare DC.
3. Publici fișierele acestei intervenții și repornești backendul, prin fluxul existent:
   `PYTHON/routes/adechit/{schema.json,service.py,catalog_forms.py,annual.py}`,
   `PYTHON/static/js/adechit/{payers.js,annual.js}`, `PYTHON/static/css/adechit.css`,
   `PYTHON/static/help/adechit.json`.
4. Probezi educator nou/existent cu câmpuri goale, completare, golire, CNP/email
   invalid, reîncărcare și închidere anuală cu păstrarea contactelor. Nu trebuie
   relansată migrarea pentru a completa aceste câmpuri.

DDL-ul, sincronizarea și publicarea: executate (confirmat de operator, 23.10.2026).

## Unde a rămas proiectul

| Zonă | Consemnat în registru | Confirmare |
|---|---|---|
| Grupe, copii, părinți, taxe, prezență | Implementate; ultima corecție ADE6-24 | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| Încasări, chitanțe, anulări, restituiri | Implementate; listare/PDF disponibile | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| Calcul și ciclu lunar/anual | Implementat; comparator și paritate cu rezultatele Access | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| Rapoarte | ADE9-01: opt rapoarte și dispoziție de plată | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| Subunități și migrare | API/schema/selector și migrator | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| Portal părinți | AD11-01–05 | TESTAT, RULAT, PUBLICAT (23.10.2026) |
| HELP | AD12-01; conținut actualizat în ADE6-24 | TESTAT, RULAT, PUBLICAT (23.10.2026) |

**Acceptare finală confirmată de operator, 23.10.2026.**

## Restanțe documentate pentru reluare

- ADE9-02: proba completă a fluxului; ADE9-03: predarea și consemnarea rezultatelor: făcute (confirmat de operator, 23.10.2026).
- Paritatea financiară și M10: verificate cu rezultatele Access pe faze.
- M03: anularea restituirii cu motiv implementată; anularea
  chitanțelor/altor documente are intervenția distinctă AD11-01.
- Compensarea prin consiliu și funcțiile amânate (SIIR, mesaje de business,
  exporturi condiționate, rapoarte consolidate): fără puncte deschise.
- Trimiterea chitanțelor pe email rămâne inactivă în meniul de încasări;
  aceasta este distinctă de emailurile de autentificare ale portalului părinților.
- Registrul istoric reutilizează AD10/AD11 pentru obiective distincte și mai are
  referințe vechi; worklogurile se identifică și după titlu, fără renumerotare tacită.

Următoarele numere: **ADE6-26** pentru catalog, **AD11-06** pentru portal,
**AD12-02** pentru ajutor, **AD13** pentru o felie principală nouă.
