# VC09-04 — Help și capturi după publicarea interfeței

Data: 2026-10-21. Utilizatorul a confirmat publicarea modificărilor VC09-03.

Capturi reale refăcute în browserul in-app, din VERCON DEMO:

- 07: Gestionari cu Denumire și Locatie, în context Mijloace Fixe;
- 09: Balanța produselor, opțiunile specifice și selecția datelor, în Local;
- 10: selectorul perioadei pe același ecran Rapoarte;
- 22: consultarea bonului de predare OI BP 13 și sumele numerice din copac.

Contul conectat, denumirile persoanelor și primitorul sunt mascate opac.
Originalele temporare au fost șterse. Nu s-au salvat sau modificat date de
gestiune și nu s-au trimis mesaje. Fila a rămas pe consultarea bonului OI.

Helpul VERCON descrie Denumire/Locatie, explică sumele grupurilor lunare și
nu mai menționează checkboxul de afișare a rezultatului în pagină. Opțiunile
raportului sunt descrise ca fiind disponibile în funcție de raportul ales.

Fișiere: `PYTHON/static/help/vercon.json`, patru JPEG-uri din
`PYTHON/static/help/images/vercon/`, inventarul `manifest.json`, `README.md`
și `implementare_vercon_v2.md`.

Stare: conținutul și capturile actualizate local, încă nepublicate.
Publicarea acestei tranșe cere JSON-ul și cele patru imagini actualizate
împreună. Nu cere modificări Python sau repornirea serviciului.
Examinare vizuală a capturilor în cadrul sarcinii cerute; fără teste,
verificări automate sau randări PDF. Fără SQL sau modificări de schemă.
