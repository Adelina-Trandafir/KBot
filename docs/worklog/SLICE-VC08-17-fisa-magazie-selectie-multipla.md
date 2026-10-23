# SLICE-VC08-17 — Fișa de magazie — mai multe produse și prețuri

Data: 2026-10-23. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Eliminată restricția variant_required pentru Fisa_Magazie și variantele
sale de raport FM1-u, FM2 și Fisa_Magazie_BF. Selectarea multiplă folosește
filtrul variant_ids deja existent în interfață și pe server. Bifarea unei
ramuri produs încarcă/selectează variantele sale prin mecanismul existent.

Noul report_statement.build separă rândurile după identitatea variantei,
ordonează produsele alfabetic și prețurile numeric și construiește fiecare
fișă prin motorul existent. Înaintea fiecărei fișe după prima introduce un
bloc break, interpretat de exportul comun drept PageBreak în PDF și
întrerupere de pagină la imprimarea Excelului. Nu adaugă pagină finală goală.

Fiecare fișă păstrează propriile solduri, mișcări cronologice, totaluri lunare
și semnături. Variantele cu același preț rămân identități separate, fără
amestecarea stocurilor. Machetele personalizate păstrează coloanele și
formatările; gruparea pe variantă este impusă în modelul din memorie.
Datele salvate și configurațiile de pe disc nu sunt rescrise.

Fișiere: PYTHON/routes/vercon/{report_catalog.py,report_statement.py (nou),
report_service.py}; PYTHON/static/help/vercon.json;
PYTHON/reports/vercon/README.md; implementare_vercon_v2.md și acest jurnal.

Numai analiză de design pe surse. Fără teste, verificări automate/de sintaxă,
browser sau generări PDF/Excel, conform instrucțiunilor proiectului.
Fără acces MariaDB, SQL executat ori schimbări de schemă. Capturile existente
nu au fost modificate. Publicare: modulele Python și helpul, apoi repornirea
serviciului web și reîncărcarea paginii pentru catalogul nou al rapoartelor.
