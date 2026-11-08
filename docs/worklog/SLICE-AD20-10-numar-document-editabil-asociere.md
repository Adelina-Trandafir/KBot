# SLICE-AD20-10 — Număr document editabil în asocierea extraselor

Data: 05.11.2026.

## Cerere și implementare

La cererea explicită a Adelinei, caseta **Număr document** devine editabilă. Propunerea inițială din AlteDoc se păstrează, calculată exclusiv pe anul calendaristic în curs din România, conform AD20-09. Valoarea tastată este folosită la salvare dacă este un întreg pozitiv, de maximum 255 de cifre, disponibil în anul curent. Nu se mai impune egalitatea cu următorul număr maxim + 1.

La adăugarea copilului în repartizare, numărul din casetă este memorat pe acel copil. Următorul număr propus este cel introdus + 1, ocolind numerele deja atribuite în ciornă. Sortarea afișării și ștergerea unui copil păstrează numerele celorlalți. Numărul repetat în repartizare este refuzat în client; pentru corectarea numărului deja memorat se elimină copilul și se adaugă din nou. Salvarea directă a unui singur copil citește caseta editată.

Serverul acceptă numerele explicite și verifică pozitivitatea, formatul, unicitatea în lot și disponibilitatea în AlteDoc pe subunitate/an calendaristic în curs, sub lacătele și tranzacția existente, prin citire curentă. Numerele cu zerouri inițiale sunt normalizate numeric; un număr existent ca «001» rezervă și valoarea 1 în această numerotare. Numărul ocupat între introducere și salvare oprește întregul lot. Ciorna și valorile explicite sunt păstrate pentru corectare. Schimbarea anului declanșează în continuare actualizarea anului/propunerii și revizuirea înainte de salvare.

Se păstrează compatibilitatea cererilor vechi fără câmpurile numerotării. Nu se modifică datele documentelor, formulele sumelor, alte ecrane sau schema MariaDB.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Editarea numărului de document în asocierea extraselor, păstrarea numărului introdus pentru fiecare copil și validarea disponibilității în anul calendaristic în curs.»

- `PYTHON/static/adechit.html`: eliminarea readonly, câmp obligatoriu și limită de lungime.
- `PYTHON/static/js/adechit/bank-associations.js`: citirea valorii editate, memorarea numerelor pe copii și refuzul repetării în ciornă.
- `PYTHON/routes/adechit/bank_numbers.py`: acceptarea numerelor explicite libere și verificarea unicității în anul curent.
- `PYTHON/static/help/adechit.json`: folosirea numărului editabil și corectarea numerelor din repartizare.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul actualizat cu referința AD20-10.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-10-numar-document-editabil-asociere.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 8 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-10 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: tastarea unui alt număr liber, salvarea directă, numere distincte pentru mai mulți copii, număr gol/zero/negativ/nenumeric, duplicat în ciornă sau bază, zerouri inițiale, ștergere/reintroducere, sortare fără renumerotare, număr ocupat între timp și trecerea în anul următor. Se publică împreună HTML/JS/Python și ajutorul. Fără SQL de aplicat sau capturi noi.
