# VC09-03 — Gestionari, rapoarte și sume în copaci

Data: 2026-10-21.

La cererea utilizatorului, înaintea actualizării helpului și capturilor:

- Gestionari expune numai Denumire și Locatie. CF, Nr. reg. com., CNP și Cont au fost eliminate din specificația nomenclatorului. Locatie păstrează cheia și coloana internă Adresa. Valorile istorice din câmpurile eliminate nu sunt șterse și nu mai sunt rescrise prin acest formular.
- Rapoarte nu mai oferă checkboxul „Afișează rezultatul în pagină” și nici zona aferentă. Listare și Export Excel păstrează fluxul existent. Secțiunea Opțiuni afișare apare numai când raportul are opțiuni specifice.
- În gruparea lunară a documentelor OI/MF, suma din dreapta nodului nu mai are prefixul TOTAL. Calculul și formatarea numerică rămân aceleași.

Fișiere modificate:

- `PYTHON/routes/vercon/nomenclatoare.py`
- `PYTHON/static/js/vercon/reports.js`
- `PYTHON/static/js/vercon/document-tree-order.js`
- `implementare_vercon_v2.md`

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Helpul și imaginile nu sunt actualizate în această tranșă: utilizatorul publică întâi modificările pe server și anunță când putem continua cu capturile reale și ajustarea documentației. Publicarea acestei tranșe cere cele trei fișiere de aplicație de mai sus și repornirea serviciului web pentru definiția Python a nomenclatorului.

## VC09-04 — Help și capturi după publicarea interfeței <a id="slice-vc09-04"></a>

Data: 2026-10-21. Utilizatorul a confirmat publicarea modificărilor VC09-03.

Capturi reale refăcute în browserul in-app, din VERCON DEMO:

- 07: Gestionari cu Denumire și Locatie, în context Mijloace Fixe;
- 09: Balanța produselor, opțiunile specifice și selecția datelor, în Local;
- 10: selectorul perioadei pe același ecran Rapoarte;
- 22: consultarea bonului de predare OI BP 13 și sumele numerice din copac.

Contul conectat, denumirile persoanelor și primitorul sunt mascate opac. Originalele temporare au fost șterse. Nu s-au salvat sau modificat date de gestiune și nu s-au trimis mesaje. Fila a rămas pe consultarea bonului OI.

Helpul VERCON descrie Denumire/Locatie, explică sumele grupurilor lunare și nu mai menționează checkboxul de afișare a rezultatului în pagină. Opțiunile raportului sunt descrise ca fiind disponibile în funcție de raportul ales.

Fișiere: `PYTHON/static/help/vercon.json`, patru JPEG-uri din `PYTHON/static/help/images/vercon/`, inventarul `manifest.json`, `README.md` și `implementare_vercon_v2.md`.

Stare: conținutul și capturile actualizate local, încă nepublicate. Publicarea acestei tranșe cere JSON-ul și cele patru imagini actualizate împreună. Nu cere modificări Python sau repornirea serviciului. Examinare vizuală a capturilor în cadrul sarcinii cerute; fără teste, verificări automate sau randări PDF. Fără SQL sau modificări de schemă.
