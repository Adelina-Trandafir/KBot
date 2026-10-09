# SLICE-ADE9-01 — ADECHIT doar cu logare + drepturi pe secțiuni și roluri

## Ce s-a schimbat și de ce

`https://k-bot.ro/adechit` se deschidea direct, fără logare. Datele erau protejate doar de sesiunea de
portal, dar pagina nu trimitea tokenul și nu avea drum spre login. În plus, drepturile ADECHIT trăiau
într-un tabel per unitate (`AD_Permissions`), fără ideea de «secțiune» a site-ului.

- **Model nou în `AVACONT_COMUN`** ([sql/ADE9_01_sectiuni_roluri.sql](../../sql/ADE9_01_sectiuni_roluri.sql)):
  `Sectiuni` (KB, AD, VR, AV), `Roluri` (un rol = o secțiune + o operație, nume românești fără diacritice),
  `Utilizatori_Roluri(UN, DC, IdRol)`. Dreptul se dă **pe unitate**; un utilizator poate avea oricâte roluri, în mai
  multe secțiuni. Roluri AD: AD_CITIRE, AD_PREZENTA, AD_PLATI, AD_ANULARE, AD_INCHIDERE, AD_REDESCHIDERE,
  AD_CATALOAGE, AD_TRANSFER, AD_IMPORT. VR_ și AV_ se adaugă când apar secțiunile.
- **`AD_Permissions` eliminat** (inventat de Codex, gol peste tot): scos din build_schema.py, sql/AD_01, sql/AD_03,
  preview, repository, test; [sql/ADE9_02_drop_ad_permissions.sql](../../sql/ADE9_02_drop_ad_permissions.sql) generează DROP-urile.
  `AD_Operations` NU s-a șters: nu ține drepturi, ci protecția la cereri repetate (`service.idempotent`).
- **Server:** `routes/portal/drepturi.py` citește secțiunile/operațiile din baza comună, la fiecare cerere.
  Rutele `/api/adechit/*` cer rolul operației lor din secțiunea AD (blueprint-ul primește `rights(context)`; preview-ul local își dă drepturile lui). `/api/portal/me` și
  `/api/portal/unit` întorc `sections`.
- **Pagină:** `static/js/adechit/gate.js` (script clasic, primul în `<head>`) trimite spre
  `/portal?next=/adechit` dacă nu există sesiune în tab. `app.js` trimite acum `X-Portal-Token`; la 401 sau
  «unitate nedeschisă» întoarce utilizatorul în portal. Portalul, după logare (sau după alegerea unității),
  revine în `/adechit` (doar adrese din lista albă `NEXT_PAGES`).
- Administrare: doar prin SQL (scavatarsoft), vezi comentariile din scriptul SQL. Ecran de admin: neacoperit.

## Fișiere atinse

`sql/ADE9_01_sectiuni_roluri.sql` (nou) · `PYTHON/routes/portal/drepturi.py` (nou) ·
`sql/ADE9_02_drop_ad_permissions.sql` (nou) · `PYTHON/routes/portal/portal.py` · `PYTHON/routes/adechit/{catalog_forms,repository}.py` · `ADECHIT/tools/{preview,build_schema}.py` · `PYTHON/tests/test_adechit.py` · `PYTHON/routes/adechit/__init__.py` · `PYTHON/static/adechit.html` ·
`PYTHON/static/js/adechit/gate.js` (nou) · `PYTHON/static/js/adechit/app.js` ·
`PYTHON/static/js/portal/portal.js` · `PYTHON/static/js/portal/app.js`.

## Rezultate teste

Nu s-au rulat teste (regula proiectului). Verificat doar: `py_compile` pe cele 3 fișiere Python, `node --check`
pe cele 4 fișiere JS. **Nu** s-a rulat nimic pe server și scriptul SQL **nu** a fost aplicat.

## Nerezolvat / amânat

- Scriptul SQL trebuie aplicat pe MariaDB de operator; apoi dat dreptul, de ex. pentru contul demo:
  `INSERT IGNORE INTO Utilizatori_Roluri (UN, DC, IdRol) SELECT 'scavatarsoft@gmail.com','000_DEMO',IdRol FROM Roluri WHERE Sectiune='AD';`
  **Până la aplicare, orice utilizator primește 403 FORBIDDEN în ADECHIT.**
- Rulat `ADE9_02` pe server pentru a șterge `AD_Permissions` din bazele de unitate.
- Fiecare ecran ADE are nevoie de AD_CITIRE; celelalte roluri se dau împreună cu ea.
- Scheletul HTML al `/adechit` tot se servește la un GET fără sesiune (sesiunea e în `sessionStorage`, fără cookie);
  se blochează datele (API) și pagina redirecționează imediat. Blocarea HTML ar cere cookie de sesiune (schimbă modelul portalului).
- Link spre ADECHIT în meniul portalului, doar pentru cei cu `sections` ∋ AD: neadăugat (`/me` întoarce deja `sections`).
- Nu s-a verificat în browser fluxul complet (login → întoarcere → pagină).
- Ajutorul din aplicație (HelpContent) acoperă aplicația desktop; ADECHIT web nu are secțiune de ajutor, deci nu s-a modificat.
- Număr de felie: ADE9 e deja menționat în plan pentru rapoartele Access (ADE1-04 «rămân în ADE9»); de confirmat/renumerotat.
