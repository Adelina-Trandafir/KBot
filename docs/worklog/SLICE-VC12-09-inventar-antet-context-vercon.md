# SLICE-VC12-09 — Antetul standard și contextul VERCON în inventar

Data: 06.11.2026.

## Modificări

La cererea Adelinei, miniaplicația `/vercon-inventar` păstrează numai bara cu fișierul Excel, foaia și butoanele «Alege Excel», «Citește și compară», «Trimite în VERCON». Rândul separat Unitate/Gestiune/Subgestiune/Cont și antetul propriu «Inventar OI Excel · VERCON» sunt eliminate.

Pagina folosește structura și stylesheeturile antetului standard VERCON: marca/meniul de aplicații, selectoarele contextului, utilizatorul, timpul sesiunii, zoomul, ajutorul/istoricul mesajelor și tema. Comboboxurile sunt controalele custom comune. Stilurile butoanelor locale inventarului sunt limitate la conținut, pentru a păstra aspectul standard al antetului.

Unitatea este cea deschisă în sesiunea portalului. Gestiunea/subgestiunea acceptate de server se transmit la navigarea între VERCON și inventar și se rețin în `sessionStorage`, separat pe unitate. URL-ul paginii reflectă contextul acceptat și la schimbarea selecției, pentru reîncărcare. Preferințele nu acordă acces: contextul este rezolvat din nou pe server; o selecție devenită indisponibilă se reîncarcă fără preferințe. În meniul miniaplicației există întoarcere la VERCON, cu protejarea asocierilor nesalvate și eliberarea blocării.

În antet apare suplimentar «Cont OIM» numai când subgestiunea OI are mai multe conturi OIM active. Unicul cont OIM activ este utilizat implicit, iar selectorul rămâne ascuns. Fără cont OIM activ se folosește mesajul comun de eroare și citirea inventarului rămâne indisponibilă. Lista este ordonată alfabetic în limba română de adaptorul existent. Nu se schimbă tipul sau contextul ales în VERCON în mod implicit: dacă este selectată o subgestiune de alt tip, antetul o păstrează și pagina cere alegerea uneia OI înainte de citire. Miniaplicația rămâne exclusiv OI.

Adaptorul API al miniaplicației acceptă și răspunsul binar folosit de exportul PDF al ajutorului standard. Grilele compacte, footerele și separatorul din VC12-08 sunt păstrate. Nu se schimbă Python, contractele de inventar, scrierea în VERCON sau baza de date.

## Stare

**VERIFICAT DE ADELINA ȘI ÎNREGISTRAT PENTRU SERVER (06.11.2026).** Adelina a confirmat explicit «inregistreaza fisierele. am verificat». Agentul a făcut numai analiză de cod pentru proiectare; fără teste, verificări de execuție, browser, build, SQL, publicare sau Git.

Pentru probă se publică fișierele HTML/CSS/JS din lista de mai jos, inclusiv modulele noi `header.js` și `context-link.js`, apoi se reîncarcă atât VERCON, cât și inventarul cu Ctrl+F5. Nu este necesară repornirea backendului pentru acest slice de interfață.

Cele 13 fișiere proprii de mai jos sunt înregistrate prin `tools/Register-DeploymentTask.ps1`, cu slice-ul și motivul acestei modificări, inclusiv documentația. Înregistrarea nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate

- `PYTHON/static/vercon-inventar.html`
- `PYTHON/static/css/vercon-inventar-layout.css`
- `PYTHON/static/css/vercon-inventar-controls.css`
- `PYTHON/static/js/vercon-inventar/app.js`
- `PYTHON/static/js/vercon-inventar/header.js`
- `PYTHON/static/js/vercon-inventar/controls.js`
- `PYTHON/static/js/vercon-inventar/api.js`
- `PYTHON/static/js/vercon/context-link.js`
- `PYTHON/static/js/vercon/app.js`
- `PYTHON/static/js/vercon/application-menu.js`
- `docs/worklog/SLICE-VC12-09-inventar-antet-context-vercon.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
