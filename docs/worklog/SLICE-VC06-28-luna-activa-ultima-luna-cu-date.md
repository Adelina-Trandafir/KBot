# SLICE-VC06-28 — Luna activă este ultima lună cu date

Data: 02.11.2026.

Cerere: în VERCON ONLINE, „luna activă” înseamnă ultima lună pentru care există date, independent de luna calendaristică.

Analiza codului: `consumption_period.active_month` nu folosea calendarul, dar pentru Alimente returna imediat ultima lună cu liste neanulate. Astfel, o listă mai veche putea ascunde o lună mai recentă cu bonuri de consum neanulate. Nu s-a consultat baza de date și nu s-a stabilit luna efectivă a gestiunii din captura utilizatorului.

Modificare în `PYTHON/routes/vercon/consumption_period.py`:

- Pentru Alimente, luna activă este luna datei maxime dintre listele de alimente și bonurile de consum neanulate; listele nu mai au prioritate față de bonuri.
- Pentru Materiale, rămâne ultima lună cu bonuri de consum neanulate. Retururile nu stabilesc luna de consum.
- Căutarea rămâne limitată la subunitatea și gestiunea curente, pe toate exercițiile. Fără date eligibile, luna rămâne nedefinită; nu există fallback la calendar.
- Mesajul de refuz și eroarea datei explică „ultima lună cu date (luna activă)”. Același calcul existent este folosit pentru permisiunile din ecran și pentru controalele de la salvare.

Limita intervenției: numai determinarea lunii pentru modificarea BC și mesajele ei. NIR, retururile, regulile de anulare, corecțiile listelor, protecția stocului și restricțiile exercițiului nu sunt modificate. Fără schimbări de schemă.

Documentație de continuitate: `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.** Analiză de design din cod și captura furnizată. Nu s-au rulat teste, verificări automate, build, browser, HTTP, SQL sau publicare, conform AGENTS.md.

## Verificare solicitată ulterior — 02.11.2026

Utilizatorul a cerut explicit verificarea erorii de stoc afișate după schimbarea lunii active.

- Comparația Git confirmă că singura modificare funcțională a slice-ului este în `consumption_period.py`. `protocol.py`, `documents.py`, `stock.py` și `stock_source.py` nu au diferențe locale față de HEAD. Mesajul și controlul `STOCK_NEGATIVE` există în istoricul Git din 12.10.2026.
- Cererea a fost găsită în jurnalul serverului cu identificatorul complet `47cedf024e3d453cb1e37245f7b4fcbf`, motiv `STOCK_NEGATIVE`, mesaj APA 19L, −4 la 19.05.2026. Codul copiat în mesajul utilizatorului era prefixul acestui identificator.
- Verificarea bazei s-a limitat la citirea variantelor APA 19L și a mișcărilor lor, în gestiunea identificată, prin `START TRANSACTION READ ONLY`, încheiat cu rollback. Datele deja salvate conțin deficitul pentru varianta cu prețul 23 lei: intrare 10 la 08.05, BC 75 = 5 la 11.05, BC 78 = 4 la 14.05, BC 81 = 5 la 19.05; soldul devine −4. Varianta cu prețul 21 lei nu prezintă deficit în istoricul citit. Nu s-a executat editarea refuzată și nu s-au modificat date.
- O probă izolată, pe date sintetice, a executat funcțiile reale `check_timeline` și `_by_day`, fără importarea aplicației ori acces la bază: deficitul istoric blochează atât o cantitate neschimbată, cât și reducerea consumului unui BC ulterior. Controlul nu compară deficitul anterior cu cel rezultat din editare.
- Amprentele AST ale funcțiilor `check_timeline`, `_by_day`, `edit_bc` și `active_month` coincid între fișierele locale și server. Modificarea lunii active este deci prezentă pe server; această sesiune nu a publicat-o.

Concluzie: eroarea corespunde unui deficit deja prezent în datele salvate, nu unui control nou introdus în acest slice. Verificarea completă a istoricului la editarea BC poate împiedica inclusiv o modificare care nu agravează acel deficit. Nu s-a modificat această regulă, deoarece cererea curentă a fost de verificare.

Stare actualizată: **COD PREZENT PE SERVER, DIAGNOSTIC CONFIRMAT PRIN CITIRE ȘI PROBĂ IZOLATĂ.** Fără modificări ale codului aplicației, scrieri în baza de date, DDL, publicare, restart sau probă de salvare în aplicație în etapa de verificare. Jurnalele locale erau vechi; incidentul a fost confirmat în jurnalul serverului.

Confirmare ulterioară a utilizatorului, 02.11.2026: cauza concretă a deficitului APA 19L este mutarea datei NIR după un consum deja înregistrat, verificată manual. Corectarea datei efective de stoc din DataFc în Data NIR este tratată separat în [SLICE-VC06-29](SLICE-VC06-29-stoc-dupa-data-nir.md). Diagnosticul și amprentele de mai sus descriu codul anterior acelui slice.

Înregistrare 03.11.2026: la cererea explicită „înregistrează fișierele care ajung pe server”, `tools/Register-DeploymentTask.ps1` a înregistrat cu succes `PYTHON/routes/vercon/consumption_period.py` pentru VC06-28. S-au înregistrat numai fișierele publicabile din PYTHON; documentația din afara acestui arbore nu a fost inclusă, conform limitării cerute. Înregistrarea nu a publicat fișiere și nu a executat Git, teste ori SQL.
