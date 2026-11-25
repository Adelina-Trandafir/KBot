# SLICE-VC20-03 — Numerotare și selecție produse la Bonuri Retur

24.11.2026 — Adaptarea numerotării a fost confirmată pentru retururile noi. În aceeași machetă a fost raportată selecția de produs fără efect la click.

## Schimbări

- **Număr BX** se completează la alegerea bonului de origine și se recalculează când se schimbă data unui retur nou. Propunerea folosește primul număr întreg liber care respectă ordinea cronologică din seria BX. Câmpul rămâne editabil. Serverul verifică din nou numărul la salvarea unui retur nou, în tranzacția existentă. Dacă data nu are niciun număr liber, formularul afișează motivul.
- În selectorul de produse, variantele disponibile apar direct la primul nivel. Editorul grilei folosește opțiunea existentă `selectParents` ca să accepte aceste rânduri. Selectarea completează datele produsului din bonul de origine și mută cursorul la **Cantitate**.
- Selectorul bonului de origine ordonează bonurile din fiecare grup după dată și apoi numeric după numărul bonului. Produsele se ordonează alfabetic după denumire și numeric după preț la denumiri egale.

## Fișiere

- `PYTHON/static/js/vercon/docs.js`, `document-number.js`, `return-editor.js`
- `PYTHON/routes/vercon/doc_routes.py`, `documents.py`, `number_change.py`
- `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md` și acest worklog

Implementat local. Nu s-au rulat teste și nu s-a făcut probă în aplicație sau publicare.
