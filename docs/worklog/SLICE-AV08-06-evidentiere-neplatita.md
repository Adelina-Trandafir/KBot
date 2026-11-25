# SLICE-AV08-06 — Evidențierea neplătită: numărul, felul și data facturii

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): «sigur», ca în Access (`Note.Form_Load`: `NumarDC`, `FelDocument`, `DataDoc` active pentru `Fel = E` fără plăți).

## What changed and why

- `NoteC_editor.detalii`: `EditareDocument` este adevărat și pentru o evidențiere neplătită (fără plăți pe ea, inclusiv din anul următor, fără plată pe e-factură, documentul nemarcat plătit, cel mult o legătură cu e-factura).
- `NoteC_editor.aplica_nota`, la «Modifică documentul»: verificările celorlalte operații ale documentului exclud operația editată (legătura ei cu e-factura este permisă); data nouă se verifică după regulile AV08-02; numărul se verifică pentru dublură (AV08-05).
- Clientul nu s-a schimbat pentru asta: bifa «Modifică documentul» apare când `EditareDocument` e adevărat.

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC_editor.py`.

## Test results

`py_compile` fără erori. Nimic rulat.

## Unverified / deferred

Legătura cu e-factura nu se schimbă (alt număr de factură pe aceeași e-factură este lăsat, ca în Access).
