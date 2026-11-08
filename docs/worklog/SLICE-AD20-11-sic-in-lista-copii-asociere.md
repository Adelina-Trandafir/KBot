# SLICE-AD20-11 — SIC în lista Copii din asocierea extraselor

Data: 05.11.2026.

La cererea Adelinei, grila **Copii** din **Asociere extrase cu copii** afișează și **SIC**, lângă SID. SIC se citește pentru aceeași Lună a înregistrării: din situația salvată când există într-o lună închisă, respectiv din motorul de calcul deja folosit de ruta copiilor. Se păstrează acoperirea copiilor fără prezență prin calcul numai în memorie.

Ruta întoarce ambele solduri și nu transformă valorile lipsă în zero. Coloana SIC are tip numeric, aliniere la dreapta și formatare românească în lei, ca SID. Se adaptează lățimile interne ale coloanelor pentru CNP și ambele solduri, fără schimbarea proporției de lățime Copii/Plătitori stabilite în AD20-07. SID și SIC se actualizează împreună la schimbarea lunii, pe toate căile existente de încărcare/selectare.

Completarea automată a Sumei pentru copil continuă să folosească SID-ul; dacă SID este zero, preia întreaga sumă a extrasului. Nu se modifică formulele soldurilor, numerele documentelor, salvarea sau schema MariaDB.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Afișarea SIC lângă SID în lista Copii din asocierea extraselor, pentru aceeași lună a înregistrării.»

- `PYTHON/routes/adechit/bank_children.py`: citirea și returnarea ambelor solduri.
- `PYTHON/static/js/adechit/bank-associations.js`: coloana SIC și lățimile interne ale grilei Copii.
- `PYTHON/static/help/adechit.json`: explicarea coloanei SIC.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-11.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-11-sic-in-lista-copii-asociere.md`: cerere, modificare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 7 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-11 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: SIC zero/pozitiv, lună deschisă/închisă, copil fără prezență, schimbarea lunii, sortarea numerică și încadrarea CNP/SID/SIC la zoomul folosit. Nu există SQL de aplicat sau capturi noi.
