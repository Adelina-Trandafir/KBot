# SLICE-AD20-04 — Selectarea textului și eticheta din stânga căutării copilului

Data: 05.11.2026.

## Cerere și implementare

În căutarea copilului din **Asociere extrase cu copii**, la intrarea în combobox se selectează întregul text pentru înlocuire directă. Evenimentul de focus acoperă tastatura și focusul programatic; primul clic de intrare reface selecția după poziționarea implicită a cursorului de către browser. Clicurile ulterioare în caseta deja focalizată permit corectarea textului. Ascultătorii folosesc același ciclu de viață al controlului.

Eticheta **Caută copil în toate grupele vizibile** este poziționată în stânga comboboxului prin CSS-ul specific ferestrei. Pe ecrane înguste, eticheta poate ocupa mai multe rânduri în jumătatea din stânga, iar caseta folosește spațiul rămas în dreapta. Asocierea accesibilă label/input existentă se păstrează.

Intervenția este limitată la căutarea copilului din această fereastră; căutarea extraselor și controlul Combobox comun nu sunt modificate. Regula AD20-03 care exclude grupele ascunse rămâne în vigoare.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Selectarea întregului text la intrarea în căutarea copilului din asocierea extraselor și poziționarea etichetei în stânga comboboxului.»

- `PYTHON/static/js/adechit/bank-child-search.js`: selecția textului la intrarea în control.
- `PYTHON/static/css/adechit-bank-associations.css`: eticheta în stânga și dimensionarea comboboxului.
- `PYTHON/static/help/adechit.json`: folosirea selecției automate și poziția etichetei.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-04.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-04-cautare-selectare-text-eticheta-stanga.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 7 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-04 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: intrare cu Tab, mouse și atingere, scriere peste selecția integrală, clic ulterior pentru corectare, alegerea altui copil și aspectul etichetei/casetei pe ecrane înguste. Nu s-au creat sau modificat capturi; captura 13 din ajutor rămâne de refăcut cu blur puternic pe datele sensibile.
