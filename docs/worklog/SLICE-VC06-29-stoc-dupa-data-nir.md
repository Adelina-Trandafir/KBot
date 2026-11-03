# SLICE-VC06-29 — Stocul folosește data NIR-ului

Data: 02.11.2026.

## Cererea și cauza confirmată

Utilizatorul confirmă prin verificare manuală că deficitul istoric APA 19L provine din mutarea datei unui NIR după data unui consum deja înregistrat. Cauza este consemnată ca **confirmată de utilizator**, nu ca ipoteză. Exemplul de referință: intrare +2 la 10.05, consum −2 la 11.05, apoi data NIR mutată la 12.05; soldul este −2 la 11.05 și revine la zero la 12.05.

Utilizatorul cere explicit folosirea **Data NIR peste tot** în cronologia VERCON ONLINE. Câmpul existent este `VC_NIR.Data`; nu se introduce o coloană nouă. Data facturii (`DataFc`) rămâne informația facturii, inclusiv pentru identificarea/asocierea eFactura și antetul tipărit. Acest slice corectează data folosită la stoc și selecțiile aferente; nu schimbă politica de acceptare a deficitelor istorice la editarea BC.

## Implementare

- `stock_source.py`: intrările și stornările folosesc `DATE(n.Data)`, fără fallback la data facturii. Disponibilul, cronologia, fișele, balanțele și recalculările care folosesc registrul comun preiau aceeași dată. Comentariul contractului din `protocol.py` este actualizat.
- `receipt_rules.py`: contribuția NIR salvat și cea a ciornei sunt comparate după data NIR. Mutarea intrării mai târziu rămâne o reducere temporară a contribuției, care cere validarea cronologiei.
- `receipts.py`: controlul înainte/după editare este declanșat de schimbarea datei NIR (`changed_day`), inclusiv când liniile sunt păstrate. Schimbarea izolată a datei facturii nu definește o deplasare de stoc.
- `documents.py`: cantitatea disponibilă pentru ajustare se calculează de la data NIR.
- `storno_stock.py`: sursele și disponibilul FCS sunt eligibile după data NIR sursă și data FCS; data facturii nu mai constituie o condiție suplimentară de stoc.
- `nomenclator_stock.py`: prima intrare a variantei folosește data NIR. `depreciation_source.py`: evenimentele de recepție/storno MF folosesc data documentului, coerent cu magazia.
- `receipt_context.py`, `static/js/vercon/receipt-context.js`: fișele contextuale sunt cerute, filtrate și afișate la data NIR/FCS, inclusiv fișa de furnizor din editor. Mesajele cer completarea datei documentului.
- `reports/vercon/queries/ledger.sql`, `receipts.sql`: sursele implicite folosesc data documentului pentru mișcare; data facturii are câmpul separat `invoice_date`.
- `report_receipt_dates.py` (nou), `report_store.py`: adaptare în memorie pentru sursele de registru/recepții salvate cu expresiile vechi. Fără suprascrierea configurărilor proprietarului. Pentru recepții, datele de document și de factură se leagă de antetul real printr-un SELECT limitat la subunitate/gestiune/exercițiu și documentele sursei; data NIR lipsă este refuzată explicit.
- `report_data.py`, `report_selection.py`, `report_incoming_selection.py`, `report_routes.py`: filtrarea documentelor și arborilor după data NIR. `report_body.py`, `report_reception.py`: data facturii rămâne distinctă în antetul generic și în macheta NIR/FCS.
- `report_service.py`, `report_routes.py`: versiune explicită a regulii datei NIR în snapshot; exporturile vechi dependente de registru/recepții și rapoartele de amortizare cer regenerare.
- Ajutorul `static/help/vercon.json` și `reports/vercon/README.md` descriu data efectivă de stoc.

Completarea automată a datei NIR când operatorul schimbă data facturii, cerută anterior în VC06-21, este păstrată; data NIR rezultată trece prin validarea ciornei. Informațiile facturii, regulile sale de identificare și asociere, nomenclatoarele și schemele MariaDB nu sunt rescrise. Datele istorice nu sunt reparate automat. Alte aplicații și migratoarele nu sunt modificate.

## Stare

**IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.** Numai analiză de design din cod. Fără teste, compilare, verificări automate de execuție, browser, HTTP, generări PDF/Excel, interogări sau modificări de date pe server, DDL, publicare ori restart în acest slice. Probele din VC06-28 au precedat această schimbare și nu validează codul nou.

Continuitate: `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

Înregistrare 03.11.2026: la cererea explicită a utilizatorului pentru fișierele care ajung pe server, scriptul `tools/Register-DeploymentTask.ps1` a înregistrat cu succes cele 23 de fișiere proprii publicabile din PYTHON descrise mai sus, inclusiv resursele SQL de raport, ajutorul și README-ul rapoartelor. Documentația din afara arborelui PYTHON a fost exclusă conform cererii. S-au păstrat slice-ul VC06-29 și motivul său real. Înregistrarea nu reprezintă publicare sau confirmare de test; fără Git, teste sau SQL executat în această etapă.
