# ADECHIT — subunitatea pe rapoarte și configurarea zilelor reale

Data: 2026-10-20. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Starea din antet este consemnată pentru implementare și corecturile de mai jos. Limitele punctuale ale verificării sunt păstrate separat.

## Rapoarte

Modelele generate prin catalogul de rapoarte primesc subunitatea selectată când denumirea ei diferă de cea a unității. Comparația ignoră diferențele de majuscule și spațiere. Subunitatea se adaugă la antetul existent sau deasupra titlului când raportul nu are antet, inclusiv pe secțiunile separate explicit prin pagină nouă. Modificarea este în modelul comun pentru HTML, PDF și Excel, inclusiv Situație de plată. Rapoartele deja generate trebuie regenerate.

## Meniu → Zile lucrătoare pe luni

- Formular anual cu 12 luni, separat pe subunitate, accesibil cu dreptul existent `catalog`.
- Afișează zile calendaristice, luni–vineri, sărbători lucrătoare descărcate, zile reale editabile și starea lunii.
- Butonul Descarcă zilele libere citește explicit API-ul fix `https://zilelibere.webventure.ro/api/<an>` și completează propuneri. Nu salvează automat și nu apelează serviciul la calculul prezenței.
- Datele sunt validate, deduplicate și comparate cu anul cerut. Ziua săptămânii este calculată din dată; sărbătorile din weekend nu se scad, iar sărbătorile suprapuse se scad o singură dată. Lista datelor și sursa sunt vizibile.
- Utilizatorul poate corecta manual numărul de zile reale între 0 și numărul zilelor calendaristice ale lunii, inclusiv fără disponibilitatea API-ului.
- Salvarea este explicită, atomică și folosește mecanismul existent de idempotency. Versiunea include subunitatea, anul, configurarea și versiunile lunilor, pentru a refuza suprascrieri pe date depășite.
- Lunile închise sunt read-only și sunt păstrate inclusiv la import. O lună deschisă nu poate fi redusă sub prezențele deja înregistrate.

Valorile se păstrează compact în `AD_Settings`, pe cheie `WorkingDays:<an>` și subunitate, în limita câmpului existent `SettingValue` de 255 caractere. Lunile existente deschise primesc valoarea în `AD_LunaD.ZileLuna`; rapoartele și validarea prezenței folosesc deja acest câmp. La deschiderea lunii următoare, `create_next_month` folosește configurarea salvată pentru acea lună. Fără configurare, fallback-ul existent este numărul zilelor luni–vineri. Prezențele individuale, încasările și situațiile financiare închise nu sunt rescrise de această machetă.

Nu sunt necesare modificări de schemă sau scripturi DDL/seed.

## Fișiere

- `PYTHON/routes/adechit/report_identity.py` (nou), `reports.py`.
- `PYTHON/routes/adechit/working_days.py`, `working_days_holidays.py`, `working_days_routes.py` (noi), `__init__.py`, `service.py`.
- `PYTHON/static/js/adechit/working-days.js` (nou), `app.js`.
- `PYTHON/static/css/adechit-working-days.css` (nou), `adechit.css`.

## Limitele verificării

Documentația furnizorului și sursa publică PHP descriu structura JSON folosită; endpoint-ul live `/api/2026` nu a putut fi citit prin instrumentul de browsing. Nu este confirmată funcționarea cererii HTTP de pe serverul aplicației.

Surse consultate:
- https://zilelibere.webventure.ro/
- https://github.com/Webventure-Interactive/zile-libere/blob/master/calcul_zile.php

Necesită publicarea Python/JS/CSS, repornirea serviciului, reîncărcarea paginii și regenerarea rapoartelor.

## 404 raportat la încărcarea machetei

Logul furnizat de utilizator arată că noul JS este încărcat, dar ruta `GET /api/adechit/working-days` primește 404 pe server. În codul local, `working_days_routes.register(bp, guard, command)` este apelat în `routes/adechit/__init__.py`, înainte de întoarcerea blueprint-ului; acesta este înregistrat în `main.py`. Filtrele locale de publicare includ `.py` și nu exclud `__init__.py`.

Este necesară publicarea `routes/adechit/__init__.py`, `working_days_routes.py`, `working_days.py`, `working_days_holidays.py` și `service.py`, precum și a modificărilor pentru rapoarte, apoi repornirea serviciului `avacont`. Un 404 nu confirmă singur dacă fișierele lipsesc de pe disc sau procesul rulează încă versiunea anterioară. Nu s-a citit sau modificat starea serverului.

Clientul API păstrează acum codul HTTP în eroare, iar macheta afișează un mesaj explicit despre actualizarea componentei Python în cazul 404. Fișiere suplimentare modificate: `PYTHON/static/js/adechit/app.js`, `PYTHON/static/js/adechit/working-days.js`.

## Eroare la afișarea coloanei Stare

Formatterul DataGrid primește doar valoarea celulei. Coloana Stare presupunea incorect că primește și rândul, provocând eroarea la accesarea `row.closed`. În `working-days.js`, textul stării este acum calculat pe fiecare rând înainte de construirea grilei și afișat direct, fără formatter.

## Coloane și calcul din zile libere suplimentare

- Numai macheta Zile lucrătoare: padding vertical de 4px pentru antetele DGV, text pe două rânduri și denumirile Zile Luna, Zile lucrătoare, Zile Libere Legale, Zile Libere suplimentare, Zile prezență / Luna.
- Zile Libere suplimentare este editabilă pentru lunile neînchise, cu același fundal `--sky-1` și font ca în coloana Prezență a grilei principale. Zile prezență / Luna este needitabilă și se recalculează după editare: luni–vineri minus libere legale în timpul săptămânii minus libere suplimentare.
- Importul sărbătorilor păstrează liberele suplimentare introduse; refuză propunerea dacă acestea ar produce zile de prezență negative. Lunile închise păstrează zilele de prezență și liberele suplimentare.
- Totalul și componentele legal/extra se salvează compact în aceeași setare anuală AD_Settings existentă, fără modificări de schemă. Serverul validează formula, limitele și protecția lunilor închise. Configurările vechi rămân citibile; componentele necunoscute pornesc de la zero, fără a clasifica automat diferențele istorice drept libere suplimentare. Totalurile vechi rămân păstrate până la editarea componentelor/import pentru luna respectivă.
- Salvarea actualizează ZileLuna pentru lunile deschise existente; lunile noi primesc totalul configurat. Raportul Situație de plată folosește deja `month_days(month)` în antetul NR. MAX. ZILE LUNĂ și în calculul taxei lunare, astfel încât primește totalul de prezență salvat prin același câmp ZileLuna.

Fișiere: `PYTHON/routes/adechit/working_days.py`, `PYTHON/static/js/adechit/working-days.js`, `PYTHON/static/css/adechit-working-days.css`.

## Fundalul coloanei Zile Libere suplimentare

Fundalul `--sky-1` și fontul de la DGV Prezență se aplică acum întregii coloane, inclusiv pe rândurile închise, ca în grila principală. Regula inițială colora doar celulele cu clasa `is-editable`. Editorul numeric păstrează același fundal la introducerea valorii. Dreptul de editare al lunilor nu se schimbă. Fișier: `PYTHON/static/css/adechit-working-days.css`.
