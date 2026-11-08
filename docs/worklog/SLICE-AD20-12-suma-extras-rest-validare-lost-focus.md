# SLICE-AD20-12 — Sumă propusă din extras/rest și validare la lost focus

Data: 05.11.2026.

## Cerere și implementare

Conform corecției Adelinei, propunerea pentru **Suma pentru copil (lei)** este suma extrasului minus totalul copiilor deja adăugați în repartizare. Inițial se propune întreaga încasare; la selectarea altui copil, adăugarea sau ștergerea unei repartizări se recalculează restul. La repartizare integrală se afișează 0. SID/SIC rămân afișate, fără să determine propunerea sumei.

La corecția explicită «NU ÎN TIMPUL TASTĂRII — LA LOST FOCUS», validarea depășirii este legată numai de evenimentul `blur`, de adăugare și de salvare. Nu există ascultător de validare pe input/keydown. La ieșirea din casetă se afișează eroare prin sistemul existent dacă suma depășește încasarea sau restul disponibil; se marchează controlul invalid și se păstrează valoarea pentru corectare. Propunerea programatică resetează mesajul propriu de validare și limita maximă a controlului.

Înainte de **Asociază și înregistrează** se reverifică limita casetei. Controlul existent al egalității exacte dintre suma copilului unic/totalul repartizării și încasarea extrasului este păstrat înaintea confirmării și cererii de salvare. La diferență se afișează eroare și nu se trimite cererea de salvare. Verificarea atomică echivalentă de pe server, din `associate_allocations`, rămâne activă și nu necesită modificări: toate sumele pozitive întregi trebuie să totalizeze exact încasarea.

Nu se schimbă numerele documentelor, datele, formulele SID/SIC, alte ecrane sau schema MariaDB.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Propunerea sumei din încasarea extrasului minus repartizările pregătite, cu validarea limitei la lost focus și oprirea salvării când totalul diferă de extras.»

- `PYTHON/static/js/adechit/bank-associations.js`: propunerea restului, limita controlului, validarea blur/adăugare/salvare.
- `PYTHON/static/help/adechit.json`: folosirea sumei din extras/rest și momentul validării.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul corectat cu referința AD20-12.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-12-suma-extras-rest-validare-lost-focus.md`: cerere, modificare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 6 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-12 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: propunere integrală, primul copil cu sumă editată, următorul copil cu rest corect, ștergerea unei repartizări, rest 0, depășire de încasare/rest cu eroare numai la ieșirea din casetă, corectare și refuzul salvării când totalul este mai mic/mai mare decât extrasul. Fără SQL de aplicat sau capturi noi.
