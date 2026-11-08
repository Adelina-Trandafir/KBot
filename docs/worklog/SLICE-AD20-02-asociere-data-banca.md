# SLICE-AD20-02 — Data bancă în tabelul asocierii extraselor

Data: 05.11.2026.

La cererea Adelinei, prima coloană a tabelului încasărilor din **Asociere extrase cu copii** afișează `DataBanca`, cu titlul **Data bancă**, în loc de `DataDoc` / **Data document**. Tipul coloanei rămâne dată calendaristică, inclusiv pentru sortare. Câmpul este deja furnizat de API; nu este necesară schimbarea serverului.

Intervenția este limitată la coloana afișată în lista de încasări. Filtrarea perioadelor, data documentului ADECHIT și revizuirea documentelor în previzualizarea automată nu sunt modificate.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Afișarea Datei bancă din extras în prima coloană a tabelului de asociere extrase cu copii.»

- `PYTHON/static/js/adechit/bank-associations.js`: sursa și eticheta coloanei de dată.
- `PYTHON/static/help/adechit.json`: explicația coloanei Data bancă.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul actualizat, cu referința AD20-02.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-02-asociere-data-banca.md`: cerere, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 6 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-02 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: un extras cu `DataBanca` diferită de `DataDoc`, afișarea datei bancare și sortarea cronologică a coloanei. Captura 13 din ajutor este anterioară modificării și rămâne de refăcut cu blur puternic; nu s-au creat sau modificat capturi.
