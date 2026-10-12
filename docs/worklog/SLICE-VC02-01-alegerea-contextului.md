# SLICE-VC02-01 — alegerea DC/subunitate/gestiune/an

Stare: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

## Facut
- `routes/vercon/context.py`: `resolve` (functie pura) valideaza selectia pe arborele furnizat de un provider; autoselectie cand exista o singura optiune; anul implicit = cel mai recent; `version` se schimba cu selectia si cu optiunile. `EmptyProvider` in productie (pana la VC03).
- `GET /api/vercon/context` (selectie, optiuni, meniu, `type_locked`, `year_mode`); selectie invalida → 409 `SELECTION_CHANGED`.
- Pagina: trei selectoare in cascada + bara de context permanenta; raspunsurile sunt acceptate doar daca raspund ultimei cereri (numar de secventa).
- Draft: la schimbarea contextului cu modificari nesalvate se intreaba «Raman aici / Renunt»; `draft.js` retine contextul in care a pornit.
- Provider de fixture in `VERCON/tools/preview.py`; fixture extins (doua gestiuni Materiale in A1, una goala, una ocupata).

## Limite
Providerul real (tabele VC_*) vine cu VC03; ocuparea exclusiva reala cu VC04-04.
