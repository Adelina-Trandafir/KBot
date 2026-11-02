# SLICE-VC06-05 — editare, anulare și traseul documentului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Editare** (`PUT /api/vercon/doc/<tip>/<id>`): perioadă deschisă (altfel PERIOD_CLOSED), revizia văzută de pagină (altfel CONFLICT), document neanulat. Linii NIR potrivite după id: adăugate/păstrate/scoase; stoc verificat după schimbare.
- **Anulare**, POST …/cancel cu motiv obligatoriu: document păstrat în listă, Anulat/motiv/cine/când; și liniile storno marcate. Fără ștergere.
- **Dependențe:** recepția cu storno cere anularea storno-ului înaintea modificării/anulării; BC cu retur ori legat de o listă alimentară la fel. Activ în registrul OI/MF → recepție blocată (DOC_LINKED); un BC legat nu poate lăsa lista fără relație prin ștergere.
- **Traseu** (`GET …/history`, buton «Istoric»): cine, cand, ce operatie, valorile dupa; din `VC_Audit`, scris in aceeasi tranzactie cu documentul; e-mailul este mascat.
- **Perioada inchisa:** butoanele de scriere dispar, serverul refuza.

## Fisiere
`PYTHON/routes/vercon/`: `documents.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Politica D05 completa (corectii istorice pe an inchis, redeschidere) nu e implementata: un an inchis ramane doar de consultat.
- Documentele importate nu au rand de audit (istoricul lor apare gol).
