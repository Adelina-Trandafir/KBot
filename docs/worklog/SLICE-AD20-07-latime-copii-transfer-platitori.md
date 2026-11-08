# SLICE-AD20-07 — Transfer de 25% din lățimea Copii către Plătitori

Data: 05.11.2026.

La cererea Adelinei, lista **Copii** din asocierea extraselor păstrează 75% din lățimea configurației AD20-06, iar cei 25% eliberați sunt transferați integral listei **Plătitori**. Se adaptează atât pragurile minime, cât și proporțiile spațiului flexibil, în ambele configurații pentru calculator:

- Configurația largă: Copii `minmax(480px, 1fr)` → `minmax(360px, .75fr)`; Plătitori `minmax(160px, .75fr)` → `minmax(280px, 1fr)`.
- Configurația intermediară: Copii `minmax(430px, 1fr)` → `minmax(322.5px, .75fr)`; Plătitori `minmax(100px, .6fr)` → `minmax(207.5px, .85fr)`.

Lățimea listei Grupe, spațiile dintre grile, coloanele din grile, SID-ul, completarea sumei și logica asocierii nu sunt modificate. Pe ecrane înguste se păstrează configurația existentă cu liste succesive.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Transferul a 25% din lățimea listei Copii către lista Plătitori în fereastra de asociere a extraselor.»

- `PYTHON/static/css/adechit-bank-associations.css`: redistribuirea lățimilor.
- `PYTHON/static/help/adechit.json`: descrierea listei Copii fără referirea la lărgirea anterioară.
- `ADECHIT/UTILIZARE_WEB.md`: precizarea redistribuirii, cu referința AD20-07.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-07-latime-copii-transfer-platitori.md`: cerere, modificare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 6 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-07 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: proporțiile Copii/Plătitori la dimensiunea și zoomul folosite. Nu s-au creat sau modificat capturi. Nu există SQL de aplicat.
