# SLICE-VC06-05 — editare, anulare și traseul documentului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Editare** (`PUT /api/vercon/doc/<tip>/<id>`): doar cat perioada e deschisa (`PERIOD_CLOSED` la apel direct), doar cu revizia pe care a vazut-o pagina (`CONFLICT` la revizie veche), doar pe document neanulat. Liniile NIR se potrivesc dupa id (adaugate/pastrate/scoase); verificarea de stoc ruleaza dupa schimbare.
- **Anulare** cu motiv obligatoriu (`POST …/cancel`): documentul ramane in lista, marcat «anulat» (`Anulat`, motiv, cine, cand); liniile storno sunt marcate si ele. Nimic nu se sterge.
- **Dependente:** o recepție cu storno nu se modifica/anuleaza pana nu e anulat storno-ul; un BC cu retur sau asociat unei liste de alimente la fel; un activ deja in registrul OI/MF blocheaza recepția (`DOC_LINKED`). Ca atare stergerea unui BC legat nu lasa lista fara relatie.
- **Traseu** (`GET …/history`, buton «Istoric»): cine, cand, ce operatie, valorile dupa; din `VC_Audit`, scris in aceeasi tranzactie cu documentul; e-mailul este mascat.
- **Perioada inchisa:** butoanele de scriere dispar, serverul refuza.

## Fisiere
`PYTHON/routes/vercon/`: `documents.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Politica D05 completa (corectii istorice pe an inchis, redeschidere) nu e implementata: un an inchis ramane doar de consultat.
- Documentele importate nu au rand de audit (istoricul lor apare gol).
- Nimic rulat.
