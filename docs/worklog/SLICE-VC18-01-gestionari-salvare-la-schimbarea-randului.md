# SLICE-VC18-01 — Gestionari: salvare doar la schimbarea rândului, fără mesaj

13.11.2026 — În Configurări → Clienți / Gestionari, fiecare celulă editată se salva imediat, iar în bara de stare apărea «Se salvează…» / «Salvat.». Cerința: rândul se salvează DOAR când se schimbă rândul, fără mesaj.

**Cauza.** `mountCrud` din `nomenclator-grid.js` are modul `rowSave` (celula doar marchează rândul ca nesalvat; salvarea o face `onRowLeave` / `finishRow`), dar îl aveau numai comisiile NIR / Inventariere și produsele. `clienti` rămânea pe salvare la fiecare celulă (`saveCell` → `persist`), cu mesajele de stare.

**Reparația.** În `mountCrud`:
- `clienti` intră în `rowSave` (alături de comisii);
- `persist` nu mai scrie «Se salvează…» / «Salvat.» pentru `clienti` (golește bara de stare); erorile rămân vizibile;
- textele rândului incomplet (Denumire este obligatorie): «Gestionarul nu poate fi salvat.» și titlul «Gestionar incomplet», în loc de cele pentru produse.

Comisiile și produsele nu se schimbă. Serverul (`nomenclatoare.py`) nu se atinge.

**Ajutor.** `PYTHON/static/help/vercon.json`, secțiunea «Baza de date, conturi, TVA și feluri» (`nomenclatoare-baza`), punctul **Gestionari**: descrie salvarea la schimbarea rândului, etichetă VC18-01.

## Fișiere

- `PYTHON/static/js/vercon/nomenclator-grid.js`
- `PYTHON/static/help/vercon.json`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
- `docs/worklog/SLICE-VC18-01-gestionari-salvare-la-schimbarea-randului.md`

## Neverificat

Verificat doar sintaxa (`node --check`, JSON valid). Nu s-a încercat în aplicație: schimbarea rândului cu o celulă editată, rând nou cu Denumire goală (apare întrebarea «Continuați editarea?»), ⋮ → Salvează rândul. Fișierele nu sunt pe server; proba de către Adelina este în așteptare, iar înregistrarea pentru publicare se face abia după ea.
