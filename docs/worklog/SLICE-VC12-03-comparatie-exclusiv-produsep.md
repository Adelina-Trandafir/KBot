# SLICE-VC12-03 — Comparație inventar exclusiv cu ProduseP

Data: 06.11.2026.

## Cererea și corecția

Adelina a precizat că inventarul Excel trebuie comparat cu produsele existente în `VC_ProduseP`.

Interogarea candidaților pornește acum explicit din `VC_ProduseP`. `VC_Produse` este legată prin JOIN pe subunitate, gestiune și ID pentru denumire, UM și cont. Codul, prețul unitar și identitatea candidatului provin din rândul real `VC_ProduseP` (`COD`, `PUTVA`, `IdProdus`). Se păstrează limitarea la baza și Gestiune / Subgestiune / Cont selectate.

A fost eliminată interogarea suplimentară care adăuga produse din `VC_Produse` fără variantă în `VC_ProduseP` și le atribuia ID-uri negative artificiale. Aceste produse nu mai ajung în motorul de similitudine, în numărul de candidați sau în raport. Mai multe rânduri reale `VC_ProduseP` pentru aceeași denumire se compară separat, fiecare cu propriul preț și cod.

Corecția privește numai sursa candidaților. Nu introduce un filtru nou de activitate sau un calcul de stoc și nu modifică algoritmul de similitudine, interfața ori schema MariaDB. Corecția tranzacției MariaDB din VC12-02 este păstrată.

## Stare

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Sunt înregistrate toate cele cinci fișiere proprii, inclusiv documentația, cu motivul acestui slice. Agentul a făcut numai analiză de cod pentru proiectare; fără teste, verificări de execuție, compilare, SQL, conexiuni la baze de date/SSH, publicare sau Git.

KBOTADMIN nu necesită recompilare pentru această corecție din `PYTHON/routes/admin_cli/vercon_inventory.py`. Înregistrarea prin `tools/Register-DeploymentTask.ps1` nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate în acest task

- `PYTHON/routes/admin_cli/vercon_inventory.py`
- `docs/worklog/SLICE-VC12-01-comparatie-inventar-excel.md`
- `docs/worklog/SLICE-VC12-03-comparatie-exclusiv-produsep.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
