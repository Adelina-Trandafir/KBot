# SLICE-AV08-03 — `NrAng` scos din toate fluxurile

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): `NrAng` și câmpurile Ordonanțare / Propunere / Angajament / NrOpe sunt din ALOP vechi (2005); nu mai sunt necesare.

## What changed and why

- `NoteC.numar_angajament_urmator` șters; `creeaza_operatie` nu mai scrie `NrAng` (rămâne NULL) și întoarce doar `IdOper`; `document_si_operatie` întoarce `(IdDocument, IdOper)`.
- Apelanții: `op9.py` (răspunsul nu mai are `nrang`), `facturi_plati.py`, `NoteC_plati.posteaza_ordin`, `NoteC_contari*.py` (răspunsul nu mai are `NrAng`), `NoteC_editor.py`.
- Client: `OpDiverseOperatieWire.nrang` scos (era `Integer` nenul și nimeni nu-l citea).
- Coloana `TZ_Oper.NrAng` și valorile migrate rămân neatinse (AVC.Migrator le scrie din Access).
- Flagurile Angajament / Propunere / Ordonantare scrise din șabloane rămân cum erau (nimic cerut despre ele); editorul nu le arată.

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC`, `op9.py`, `facturi_plati.py`, `NoteC_plati.py`, `NoteC_editor.py`; `PYTHON/routes/avacont/introducere_date/operatii/NoteC_contari.py`; `src/KBot.Api/Avacont/IntroducereDate/Operatii/ApiClient.Operatii.vb`.

## Test results

Build App 0/0; `py_compile` fără erori. Nimic rulat.

## Unverified / deferred

Niciun cititor al `NrAng` găsit în KBOT (grep `PYTHON`, `src`); rapoartele viitoare nu trebuie să se bazeze pe el pentru operațiile noi.
