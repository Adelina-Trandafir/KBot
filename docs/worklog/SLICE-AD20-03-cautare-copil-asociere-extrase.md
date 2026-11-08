# SLICE-AD20-03 — Căutare copil în grupele vizibile la asocierea extraselor

Data: 05.11.2026.

## Cerere și implementare

Deasupra listelor Grupe / Copii din **Asociere extrase cu copii**, în modul manual, este adăugat controlul comun Combobox pentru căutarea copilului după minimum două caractere din nume sau CNP. Rezultatele afișează numele, grupa și ultimele patru cifre din CNP pentru diferențiere. Se ordonează prin mecanismul comun în română, cu chei explicite nume → grupă → sufix CNP, înainte de limitarea la primele 100 de alegeri; un mesaj cere restrângerea când sunt mai multe rezultate.

Conform corecției explicite a Adelinei «FĂRĂ GRUPE ASCUNSE», rezultatele includ numai grupele cu `Ascunsa=0`, indiferent de bifa existentă «Arată ascunse». Nu se activează automat bifa. Căutarea este limitată la subunitatea autentificată, fără filtrare după luna extrasului sau prezență; folosește grupa curentă din catalogul copiilor. Căutarea din antetul paginii Prezență, limitată la luna aleasă, nu este modificată.

La selectare se recitesc catalogul grupelor și copiii grupei pentru a controla rezultatul devenit vechi. Dacă grupa nu mai este vizibilă sau copilul nu mai aparține grupei, se raportează eroarea și se cere refacerea căutării. Altfel se selectează grupa și copilul, se încarcă istoricul plătitorilor cu alegerea plătitorului activ prin fluxul existent și se aduc rândurile în vedere prin `selectRow(..., { reveal: true })`. Filtrele și sensul de sortare al grilelor sunt păstrate de API-ul controlului; rezultatul ales este dezvăluit explicit.

Extrasul, luna, explicația și repartizările deja pregătite nu sunt resetate de căutarea/selectarea copilului. Nu se înregistrează plăți prin simpla selectare. Cererile de căutare sunt anulate la schimbarea textului, închidere, schimbarea modului și distrugerea controlului; rezultatele vechi nu se aplică după schimbarea contextului. Se reutilizează dreptul de citire, tranzacția și raportarea erorilor existente. Nu se modifică schema MariaDB.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Căutarea copilului în toate grupele vizibile la asocierea extraselor, cu selectarea și poziționarea automată pe grupa și copilul ales.»

- `PYTHON/routes/adechit/bank_child_search.py` (nou): căutarea numai în grupele vizibile, în subunitatea curentă.
- `PYTHON/routes/adechit/__init__.py`: rută de citire pentru căutarea din asociere.
- `PYTHON/static/js/adechit/bank-child-search.js` (nou): controlul comun, căutare, sortare și invalidarea rezultatelor vechi.
- `PYTHON/static/js/adechit/bank-associations.js`: integrarea selecției grupă/copil și ciclului de viață.
- `PYTHON/static/adechit.html`: caseta de căutare deasupra listelor.
- `PYTHON/static/css/adechit-bank-associations.css`: structură și dimensionare în zona listelor.
- `PYTHON/static/help/adechit.json`: folosirea căutării și excluderea grupelor ascunse.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-03.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-03-cautare-copil-asociere-extrase.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 11 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-03 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: căutare după nume/CNP cu mouse și tastatură, copii cu același nume, copil din altă grupă vizibilă, copil fără prezență în luna aleasă, excluderea grupelor ascunse indiferent de bifă, rezultat devenit vechi, cereri lente și închidere/redeschidere, selecție aflată în afara zonei vizibile sau a unui filtru din grilă, păstrarea repartizărilor și aspectul pe ecrane înguste. Se publică împreună noile module Python/JS, ruta, integrarea, HTML/CSS și ajutorul; fără SQL de aplicat.

Captura 13 din ajutor este anterioară noii căutări și rămâne de refăcut cu blur puternic pe datele sensibile; nu s-au creat sau modificat capturi.
