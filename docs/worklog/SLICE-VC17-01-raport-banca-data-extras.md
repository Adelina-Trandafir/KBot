# SLICE-VC17-01 — Raport bancă: data extrasului în loc de data plății

13.11.2026 — Cerere: în **Raport bancă** trebuie să se vadă data extrasului (ca la asocierea plăților), nu data băncii sau data documentului.

## Ce s-a schimbat și de ce

Raportul lua coloana «Data» din `AD_Plati.Data` (data plății). Acum `_bank_entries` din `PYTHON/routes/adechit/reports.py` ia data extrasului la care plata a fost asociată: `AD_EXTRASE_ASOCIERI` → `FX_Extrase` → `FX_Extrase_H` → `FX_Extrase_F.DataExtras`, aceeași cale ca în lista de asocieri (`bank_associations.py`, `ASSOCIATION_SELECT`).

- Data din coloană, **intervalul ales**, sortarea și subtotalul pe zi merg toate după data extrasului (filtrul se aplică pe data afișată, ca raportul să nu pară incoerent).
- O plată fără asociere cu un extras își păstrează data plății (presupunere, neconfirmată de Adelina).
- Dacă o plată are mai multe asocieri, se ia cea mai veche dată de extras (`MIN`), ca rândul să nu se dubleze.
- Registru de casă nu se schimbă.

## Fișiere

- `PYTHON/routes/adechit/reports.py` (`_bank_entries`)
- `PYTHON/static/help/adechit.json` (rândul «Raport bancă» din tabelul rapoartelor, etichetă VC17-01)
- `VERCON/VERCON_STATUS.md` (rând de status)

## Teste

Doar sintaxa (`py_compile` cu `PYTHON/.venv`). Nu s-a rulat raportul și nu s-a văzut PDF-ul.

## Neverificat

- Interogarea SQL nu a rulat pe MariaDB (subinterogarea corelată pe `AD_EXTRASE_ASOCIERI` și numele coloanelor luate din codul existent, nu din `MariaDB_Schema/`).
- Alegerea «fără asociere = data plății» și `MIN` la mai multe asocieri sunt presupuneri.
- Fișierul nu e pe server; înregistrarea pentru publicare se face abia după proba Adelinei.
