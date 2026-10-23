# VERCON — Rapoarte OI: structura de referință și selecția MF

Data: 2026-10-23.

MAGAZIE rămâne rădăcină separată, cu toate rapoartele și grupările existente,
ca în celelalte gestiuni. Ramura OBIECTE DE INVENTAR păstrează cele două
niveluri de grupare Root și Niv1: OBIECTE DE INVENTAR → BONURI / PRODUSE /
PRIMITORI, urmate de frunzele rapoartelor. Numai această ramură este limitată
la cele 11 rapoarte din imaginea furnizată, în această ordine:

- BONURI: Bonuri casare; Bonuri predare; Bonuri transfer.
- PRODUSE: Situație produse; Stoc final produse; Balanță obiecte inventar;
  Lista inventar.
- PRIMITORI: Situație primitor; Stoc final primitor; Lista inventar;
  Fișă de evidență / spații O.I.

Fiecare raport OI reutilizează selecția ierarhică MF, inclusiv încărcarea
la deschiderea ramurii, bifarea ramurilor și „Selectează TOATE”. Pentru
bonuri se folosește primitor–bon–produs; pentru PRODUSE, cont–produs;
pentru PRIMITORI, primitor–cont–produs. Ramurile independente „Conturi
în folosință” și „Deținători” nu mai apar la aceste rapoarte. Protocolul
intern existent `mf_selection` transmite identitățile complete până la
generarea snapshotului comun PDF/Excel. Schimbarea raportului reîncarcă
selecția, ca la MF.

Calculele OI, sursele SELECT, machetele și aliasurile folosite direct de
documente se păstrează. Celelalte tipuri de gestiuni păstrează catalogul
și structura curente. Documentația și ajutorul textual descriu noua
structură; capturile existente nu au fost modificate.

Fișiere de aplicație:

- `PYTHON/routes/vercon/report_catalog.py`
- `PYTHON/routes/vercon/report_mf.py`
- `PYTHON/routes/vercon/report_mf_selection.py`
- `PYTHON/routes/vercon/report_service.py`
- `PYTHON/static/js/vercon/report-result.js`
- `PYTHON/static/js/vercon/reports.js`
- `PYTHON/static/help/vercon.json`

Documentație: `PYTHON/reports/vercon/README.md`, acest jurnal și
`implementare_vercon_v2.md`.

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse; fără
teste, verificări automate, verificări de sintaxă, browser sau generări
PDF/Excel, conform instrucțiunilor proiectului. Fără execuție SQL sau
modificări de schemă. Publicare: fișierele de aplicație de mai sus împreună,
apoi repornirea serviciului web pentru codul Python.
