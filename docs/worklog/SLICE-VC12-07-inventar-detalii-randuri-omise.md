# SLICE-VC12-07 — Denumire, UM și preț pentru rândurile omise

Data: 06.11.2026.

## Cererea și corecția

Adelina a cerut ca mesajul erorilor de citire să arate, pe lângă rând și NrCrt, denumirea, UM și prețul poziției. UM și prețul pot fi `null` și nu trebuie să genereze erori de afișare.

Diagnosticul fiecărui rând omis include acum `name`, `um` și `price`, preluate din celulele B/D/I ale sursei, înaintea conversiei numerice. Astfel, un preț nevalid rămâne vizibil ca textul citit, iar un preț care a depășit precizia admisă nu este rotunjit pentru mesaj.

Mesajul comun, cu nivel eroare/roșu, prezintă fiecare poziție într-un bloc cu rândul Excel, NrCrt, motivul, denumirea, UM și prețul. Valorile `null`, `undefined` sau goale se afișează ca **lipsă**. Zero rămâne zero; nu se apelează formatarea numerică pe valorile de diagnostic.

Citirea continuă pentru celelalte poziții, rândurile invalide rămân excluse, iar detaliile erorii nu sunt adăugate în pagină. Nu se modifică asocierea sau trimiterea în VERCON.

## Stare și livrare

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Agentul a făcut numai analiză de cod pentru proiectare; fără teste, verificări de execuție, browser, SQL, publicare sau Git.

Cele cinci fișiere proprii, inclusiv documentația, sunt înregistrate prin `tools/Register-DeploymentTask.ps1`, cu motivul acestui slice. Înregistrarea nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate

- `PYTHON/routes/vercon/inventory_excel.py`
- `PYTHON/static/js/vercon-inventar/controls.js`
- `docs/worklog/SLICE-VC12-07-inventar-detalii-randuri-omise.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
