# SLICE-AD21-02 — Extrase: doar plătitori cu CNP și Referința în cheia de import

Data: 13.11.2026. Număr de felie dat de Adelina («Continui ultimul AD» → următorul din AD21).

## Cerere

1. În ADECHIT, lista extraselor să arate **numai** încasările al căror cod fiscal de plătitor este un CNP (lungime 13).
2. În extrasul din imagine, ultimele două rânduri (ELENA LOREDANA IVAN, 04.11.2026, nr. 1, 420,00) sunt preluate o singură dată. Se deosebesc prin **Referință**.

Deciziile Adelinei la întrebări: deosebirea se face după `FX_Extrase.Referinta` (textul dinaintea primei liniuțe din explicație: `TZ521103381350` față de `TZ521103381352`); filtrul CNP se aplică **doar la afișare**, nu la import.

## Cauza celui de-al doilea punct

`importa_extrase` decide dublura pe cheia naturală `(DataDoc, NrDoc, platitor_cui, suma_debit, suma_credit)`. Cele două rânduri au cheia identică (același document `1`, același CNP, aceeași sumă), deci al doilea era tratat ca dublură și nu se scria.

## Ce s-a schimbat

- `routes/forexe/extrase.py`: cheia naturală primește și `Referinta` (a șasea componentă). Modificat în patru locuri, ca să rămână aceeași cheie peste tot: citirea rândurilor existente (`_E_EXISTENTE_SQL`), `_cheie_operatiune` (verificarea «toate operațiunile există deja»), `_scrie_operatiune`, plus helperul nou `_ref_cheie` (None și «» sunt aceeași valoare). Formula HASH nu s-a atins. Importul FOREXE folosește aceeași funcție, deci primește aceeași cheie; rândurile deja existente au aceeași Referință și rămân dubluri.
- `routes/adechit/bank_associations.py`: constanta `CNP_CLAUSE` (`e.platitor_cui REGEXP '^[0-9]{13}$'`) în `available()` (lista) și în `available_periods()` (arborele de luni), ca arborele să nu arate luni fără rânduri.
- Ajutor `adechit.json`: paragraf nou în secțiunea asocierii manuale, cu eticheta `AD21-02`.

## Limite

- Fișierul din imagine, dacă a fost deja încărcat, este refuzat ca «același fișier» (nume + dată): al doilea rând nu apare retroactiv. Pentru el trebuie șters extrasul încărcat (antet, rânduri și rândul din istoricul încărcărilor) și reîncărcat; **nu s-a făcut nimic în bază**.
- Un rând cu CUI gol, cu litere sau altă lungime nu mai apare în lista ADECHIT (rămâne în `FX_Extrase`). Restul ecranelor care citesc `FX_Extrase` nu sunt filtrate.

## Verificări

Efectuat: parsare AST a celor două fișiere Python. **Neverificat:** nicio rulare pe MariaDB sau în browser; `REGEXP` pe coloana reală; reîncărcarea extrasului din imagine; fără teste.

## Fișiere

`PYTHON/routes/forexe/extrase.py`, `PYTHON/routes/adechit/bank_associations.py`, `PYTHON/static/help/adechit.json`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`, acest worklog.
