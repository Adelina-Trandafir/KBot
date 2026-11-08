# SLICE-AD20-08 — Număr document anual și numere distincte la repartizarea extrasului

Data: 05.11.2026.

## Cerere și implementare

În panoul din dreapta al asocierii manuale se adaugă caseta **Număr document**, automată și doar-citire, sub Luna înregistrării. Propune următorul număr integral din `AD_AlteDoc`, pe subunitatea curentă și anul calendaristic relevant: maximum numeric + 1, sau 1 dacă nu există numere. Sunt incluse documentele anulate; numerele nenumerice sunt ignorate la determinarea maximului. Numerele sunt transferate ca text și incrementate cu BigInt în client, pentru a evita pierderea cifrelor numerelor mari.

Înainte de selectarea extrasului, propunerea folosește anul lunii înregistrării. După alegerea extrasului, folosește anul datei documentului care se păstrează la salvare (`DataDoc`), în aceeași regulă de unicitate anuală AD20-01. Anul apare lângă casetă. Nu se modifică data bancară, data documentului sau rândul sursă al extrasului.

Fiecare copil din repartizare primește un număr distinct, consecutiv. Lista este afișată alfabetic și calculează numerele în aceeași ordine folosită în confirmare și cererea de salvare. Caseta arată numărul următor pentru un copil nou; pentru salvarea directă a unui singur copil se folosește numărul de bază. Numerele propuse pot fi reașezate când se adaugă sau se șterg copii. Nu se rezervă sau consumă numere în bază înainte de salvare.

La salvare, sub lacătele existente și în aceeași tranzacție, se recitește maximul anual printr-o citire curentă `FOR UPDATE` și se verifică exact anul și numerele revizuite. Dacă propunerea este depășită de altă salvare, întreaga operație este refuzată înaintea scrierii documentelor, iar răspunsul conține noua propunere. Clientul reface numerele și cere revizuirea prin confirmarea existentă la următoarea încercare. Cheia operației se păstrează pentru reîncercarea aceluiași conținut și se înlocuiește când conținutul se schimbă.

Pentru repartizare se transmite către `service.emit_document` numărul individual calculat, în locul aceluiași NrDoc din extras pentru toți copiii. Verificarea unicității din Alte plăți și salvarea atomică rămân active. Cererile vechi de repartizare fără câmpurile propunerii primesc și ele numere distincte pe server. Fluxul de previzualizare automată cu editarea datelor pe rând nu este extins la noua casetă sau la altă regulă de numerotare.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Propunerea următorului număr anual din AlteDoc în asocierea extraselor și salvarea unui număr distinct pentru fiecare copil repartizat, cu reverificare atomică.»

- `PYTHON/routes/adechit/bank_numbers.py` (nou): propunerea anuală și verificarea numerelor repartizării.
- `PYTHON/routes/adechit/__init__.py`: ruta de citire pentru propunere.
- `PYTHON/routes/adechit/bank_associations.py`: numere individuale și verificarea propunerii înaintea salvării.
- `PYTHON/static/adechit.html`: caseta Număr document.
- `PYTHON/static/js/adechit/bank-associations.js`: propunere, numere în listă/confirmare, transmitere și refacerea propunerii depășite.
- `PYTHON/static/help/adechit.json`: utilizarea casetei și numere distincte.
- `ADECHIT/UTILIZARE_WEB.md`: ghidul cu referința AD20-08.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-08-numere-documente-asociere-copii.md`: cerere, implementare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 10 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-08 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: an gol, documente anulate/numere nenumerice/numere mari, schimbarea anului și extrasului, un copil, mai mulți copii cu numere distincte, adăugare/ștergere și confirmarea numerelor, număr ocupat între propunere și salvare, reîncercare după răspuns pierdut, eșec intermediar cu rollback și afișarea casetei. Se publică împreună modulul Python nou, ruta, salvarea, HTML/JS și ajutorul. Fără SQL de aplicat.

Captura 13 din ajutor rămâne de refăcut cu blur puternic pe datele sensibile; nu s-au creat sau modificat capturi.
