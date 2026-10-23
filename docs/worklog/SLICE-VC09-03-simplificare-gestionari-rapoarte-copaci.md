# VC09-03 — Gestionari, rapoarte și sume în copaci

Data: 2026-10-21.

La cererea utilizatorului, înaintea actualizării helpului și capturilor:

- Gestionari expune numai Denumire și Locatie. CF, Nr. reg. com., CNP și
  Cont au fost eliminate din specificația nomenclatorului. Locatie păstrează
  cheia și coloana internă Adresa. Valorile istorice din câmpurile eliminate
  nu sunt șterse și nu mai sunt rescrise prin acest formular.
- Rapoarte nu mai oferă checkboxul „Afișează rezultatul în pagină” și nici
  zona aferentă. Listare și Export Excel păstrează fluxul existent.
  Secțiunea Opțiuni afișare apare numai când raportul are opțiuni specifice.
- În gruparea lunară a documentelor OI/MF, suma din dreapta nodului nu mai
  are prefixul TOTAL. Calculul și formatarea numerică rămân aceleași.

Fișiere modificate:

- `PYTHON/routes/vercon/nomenclatoare.py`
- `PYTHON/static/js/vercon/reports.js`
- `PYTHON/static/js/vercon/document-tree-order.js`
- `implementare_vercon_v2.md`

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Helpul și imaginile nu sunt actualizate în această tranșă: utilizatorul
publică întâi modificările pe server și anunță când putem continua cu
capturile reale și ajustarea documentației. Publicarea acestei tranșe cere
cele trei fișiere de aplicație de mai sus și repornirea serviciului web
pentru definiția Python a nomenclatorului.
