# SLICE-AD20-01 — Butoane asociere extrase și număr obligatoriu în Alte plăți

Data: 05.11.2026.

## Cerere și implementare

În **Asociere extrase cu copii**, butonul **Închide** este mutat din antet în stânga subsolului. **Previzualizare automată** este mutat lângă **Asociază și înregistrează în Alte plăți**, în dreapta jos. Se păstrează identificatorii și evenimentele existente, modurile manual/automat și confirmarea abandonării repartizărilor. CSS-ul rămâne separat, limitat la această fereastră.

În **Prezență → Alte plăți**, prima coloană primește un câmp Număr editabil, înainte de Data/Valoare/Explicație. Este eliminat numărul automat «Fără număr». Câmpul admite identificatori text de maximum 255 de caractere, conform contractului existent. Numărul este curățat de spațiile marginale; lipsa sa și vechiul text «Fără număr» sunt refuzate în client și pe server.

Conform clarificării explicite a Adelinei, unicitatea este pe subunitatea curentă și **an calendaristic**, după `DataDoc`: intervalul [01.01.an, 01.01.anul următor). Același număr poate reapărea de la 1 ianuarie al anului următor. Controlul include documentele altor copii, documentele anulate și cele provenite din extrase; nu filtrează după luna sau copilul afișat. Compararea ignoră literele mari/mici și spațiile marginale. Nu se schimbă numerotarea chitanțelor pe an școlar.

Controlul serverului este înaintea inserării plății/documentului, sub lacătul existent al subunității, cu citire `FOR UPDATE` pentru a vedea și operația care a precedat așteptarea lacătului. Nu este necesară o modificare de schemă. Fluxul de asociere/repartizare a extraselor nu primește restricții noi și nu sunt rescrise documente istorice.

Erorile salvării manuale în Alte plăți folosesc jurnalul de mesaje existent și `messageBox` din sistemul comun: dialog blocant cu fundalul întregului ecran estompat. Nu se afișează suplimentar tooltip. Ciorna se păstrează; după închiderea mesajului despre număr lipsă/duplicat, focusul revine la Număr.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Mutarea butoanelor din asocierea extraselor și introducerea obligatorie a numărului în Alte plăți, unic pe subunitate și an calendaristic, cu refuzul salvării în dialogul comun.»

- `PYTHON/static/adechit.html`: poziția butoanelor din asociere.
- `PYTHON/static/css/adechit-bank-associations.css`: alinierea butoanelor în subsol.
- `PYTHON/static/js/adechit/app.js`: câmp Număr, transmiterea numărului introdus și conectarea mesajelor de eroare.
- `PYTHON/static/js/adechit/other-payment.js` (nou): citirea/validarea numărului și utilizarea jurnalului/dialogului comun pentru refuzul salvării.
- `PYTHON/routes/adechit/service.py`: număr obligatoriu, refuzul duplicatelor pe an calendaristic înaintea inserărilor.
- `PYTHON/static/help/adechit.json`: utilizarea butoanelor și regulile numărului.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul actualizat cu referința AD20-01.
- `ADECHIT_STATUS.md`: indexul feliei și numărul principal următor liber.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md` (nou): registrul detaliat, starea și probele rămase.
- `docs/worklog/SLICE-AD20-01-extrase-butoane-numar-alte-plati.md` (nou): implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 10 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-01 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: poziția butoanelor în mod manual/automat și pe ecran îngust; introducerea numărului cu tastatura; lipsă, spații sau «Fără număr»; duplicat la același/alt copil, document anulat și cereri concurente; refuz în același an și acceptarea numărului la 01.01 din anul următor; anul determinat de data documentului; mesajul blocant și păstrarea ciornei. Se publică împreună `app.js`, modulul nou și `service.py`, plus HTML/CSS/ajutor. Nu există SQL de aplicat.

Capturile existente din ajutor pentru Alte plăți și asociere sunt anterioare schimbării; nu s-au creat sau modificat capturi. Refacerea lor rămâne documentată în registrul detaliat.
