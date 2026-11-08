# SLICE-AD20-09 — Ultimul număr exclusiv din anul calendaristic în curs

Data: 05.11.2026.

La corecția explicită a Adelinei, ultimul număr din AlteDoc pentru asocierea manuală se calculează numai pe **anul calendaristic în curs**, conform datei curente în România (`bucharest_today().year`). La data acestei intervenții: **01.01.2026–31.12.2026**. Alegerea extrasului sau a lunii înregistrării nu schimbă anul numerotării.

Propunerea și reverificarea atomică a numerelor folosesc aceeași sursă de an. Funcția de alocare nu mai primește anul de la apelant; îl determină din data curentă. O propunere din anul precedent este refuzată și actualizată la salvare dacă între timp a început un an nou. Numerele distincte pe copil, controlul concurenței, datele documentelor și celelalte fluxuri rămân păstrate. Nu se modifică regula de verificare a numerelor introduse manual în Prezență → Alte plăți.

Această corecție înlocuiește alegerea anului după luna/documentul descrisă istoric în AD20-08. Ghidul și ajutorul descriu regula curentă.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Calcularea ultimului număr AlteDoc exclusiv pentru anul calendaristic în curs, după data curentă din România, atât la propunere, cât și la salvarea repartizării.»

- `PYTHON/routes/adechit/bank_numbers.py`: anul curent comun propunerii și alocării.
- `PYTHON/routes/adechit/bank_associations.py`: apelul alocării fără anul datei extrasului.
- `PYTHON/static/help/adechit.json`: regula anului calendaristic în curs.
- `ADECHIT/UTILIZARE_WEB.md`: corectarea explicației numerotării cu referința AD20-09.
- `ADECHIT_STATUS.md`: starea curentă în index.
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`: registrul detaliat și următoarea subfelie liberă.
- `docs/worklog/SLICE-AD20-09-numar-an-calendaristic-in-curs.md`: cerere, modificare, fișiere și limite.

## Stare și limite

**TESTAT ȘI VALIDAT DE ADELINA — VERSIUNEA FINALĂ, ÎNREGISTRAT PENTRU SERVER.**

La 05.11.2026, Adelina a confirmat explicit: «am verificat si validat: inregistreaza fisierele». Validarea privește versiunea finală cumulată AD20-01–13, cu toate corecțiile ulterioare; descrierile intermediare din workloguri păstrează istoricul cererilor. Confirmarea nu detaliază separat acoperirea fiecărui scenariu enumerat anterior.

Toate cele 7 fișiere proprii enumerate mai sus, inclusiv ajutorul și documentația, sunt înregistrate pentru SLICE-AD20-09 prin tools/Register-DeploymentTask.ps1, cu motivul acestei intervenții și versiunile finale curente. Agentul nu a executat teste automate sau vizuale, compilare, browser, SQL, SSH, publicare, restart ori Git.

Scenarii consemnate anterior pentru operator: documente/extrase și luni din alți ani, ultimul număr căutat numai în anul curent, mai mulți copii și o propunere care traversează 31.12/01.01. Nu există SQL de aplicat.
