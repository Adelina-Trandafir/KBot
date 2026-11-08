# SLICE-VC12-06 — Citire Excel cu omiterea rândurilor invalide

Data: 06.11.2026.

## Cererea și comportamentul

Adelina a cerut eliminarea textului de eroare din pagină, raportarea prin sistemul existent de mesaje, cu roșu/nivel eroare, și continuarea citirii pentru celelalte date. Exemplul raportat este «Cantitate, UM sau preț lipsă/invalid la rândul 182».

Readerul colectează acum erorile de date pe rând, în loc să oprească citirea la prima poziție invalidă. Sunt omise pozițiile cu secțiune/denumire nerecunoscută sau lipsă, cantitate/UM/preț lipsă ori invalid, câmpuri prea lungi, precizie/capacitate numerică neacceptată și număr fizic de rând invalid/duplicat. Fiecare poziție omisă are în diagnostic rândul Excel, NrCrt și motivul. Celelalte poziții sunt procesate normal, fără înlocuirea valorilor lipsă cu zero sau cu valori inventate.

API-ul de citire întoarce datele valide și lista `errors`. Numai pozițiile valide intră în comparația temporară și pot ajunge ulterior în asocieri sau stoc inițial. Dacă toate pozițiile OI sunt invalide, răspunsul conține grile goale, `workspace: null` și diagnosticele; trimiterea rămâne indisponibilă. MF continuă să fie omise separat.

Erorile structurale ale fișierului, limitele generale de citire și erorile operațiilor server nu sunt transformate în poziții importabile sau ignorate. Corecția privește erorile de date pe rând.

## Mesaje

Grilele sunt populate înaintea afișării mesajului. Toate erorile de citire sunt reunite într-un singur mesaj prin `notify(..., 'error')` din sistemul VERCON existent, cu jurnalul comun, dialogul custom și stilul de eroare. Mesajul nu este așteptat pentru continuarea încărcării datelor.

Textul detaliat de eroare nu mai este scris în banda de stare a paginii; aceasta păstrează doar informațiile normale despre încărcare și numărul pozițiilor valide/omise. Și celelalte apeluri de eroare ale miniaplicației trec prin sistemul comun de mesaje. Corpul mesajului de eroare este roșu și derulabil când lista este lungă, prin CSS limitat la această pagină; controalele și sistemul comun de mesaje nu sunt modificate.

## Stare și livrare

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Agentul a făcut numai analiză de cod pentru proiectare; fără teste, verificări de execuție, browser, SQL, conexiuni la baze de date/SSH, publicare sau Git.

Cele opt fișiere proprii, inclusiv documentația, sunt înregistrate prin `tools/Register-DeploymentTask.ps1`, cu motivul acestui slice. Înregistrarea nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate

- `PYTHON/routes/vercon/inventory_excel.py`
- `PYTHON/routes/vercon/inventory_routes.py`
- `PYTHON/static/js/vercon-inventar/controls.js`
- `PYTHON/static/js/vercon-inventar/app.js`
- `PYTHON/static/css/vercon-inventar-controls.css`
- `docs/worklog/SLICE-VC12-06-inventar-citire-erori-pe-rand.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
