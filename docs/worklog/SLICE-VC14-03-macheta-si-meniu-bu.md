# SLICE-VC14-03 — Macheta și meniul BU

09.11.2026 — **Fluxuri → Transferă între subunități** apare la OI/MF când există cel puțin două subunități active (intrarea este adăugată în lista grupului Fluxuri din `paintMenu`, `app.js`, cu iconița ei).

11.11.2026 — Macheta arată ca **Bon Transfer** (aceeași pagină: bara de titlu cu starea Ciornă/Salvat/Anulat, antetul, grila de produse cu rândul nou mereu deschis și alegerea produsului din arbore, bara de acțiuni, arborele de bonuri salvate în dreapta cu grupare pe luni sau pe unitate și butonul de adăugare). Față de Bon Transfer are în plus:

- **Unitate destinație** (obligatorie) și **Gestiune destinație** (apare numai când unitatea are mai multe gestiuni eligibile; una singură se alege singură);
- **Primitor (opțional)**: lăsat gol, bonul face doar NIR în magazia destinației;
- conturile de magazie / folosință din destinație, numai când sunt mai multe de ales;
- **Predător**: Magazie sau un gestionar al sursei.

Un bon salvat se deschide din arbore în aceeași machetă, doar pentru consultare; din meniul ⋮ al rândului din arbore și din bară: **Previzualizează** și **Anulează BU**. Folosește ruta dedicată din [VC14-02](SLICE-VC14-02-documente-si-evidenta-bu.md); răspunsul acesteia primește și numărul de zecimale al gestiunii (`zec`), pentru valoarea rândurilor.

Implementat local. Fără probe sau publicare executate în această etapă.

## Fișiere

- `PYTHON/routes/vercon/__init__.py`
- `PYTHON/routes/vercon/bu_routes.py`
- `PYTHON/routes/vercon/capabilities.py`
- `PYTHON/static/css/vercon-oi.css`
- `PYTHON/static/js/vercon/app.js`
- `PYTHON/static/js/vercon/bu.js`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
- `docs/worklog/SLICE-VC14-03-macheta-si-meniu-bu.md`
