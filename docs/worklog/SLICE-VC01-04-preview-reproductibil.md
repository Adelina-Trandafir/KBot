# SLICE-VC01-04 — preview reproductibil

Stare: **IMPLEMENTAT_LOCAL**. Nimic rulat, testat sau previzualizat (regula: fara teste/randari decat la cerere); nimic validat pe server.

## Facut
- `VERCON/tools/preview.py` (launcher local, doar 127.0.0.1) + `preview_fixture.json`: doua DC-uri fictive, doua subunitati in primul, gestiuni MATERIALE/ALIMENTE/OI/MF, ani deschisi/inchisi. Intrare `vercon-preview` (port 5053) in `.claude/launch.json`.
- Schimbare DC: `/vercon/preview/unit/preview` sau `preview2`. Reset: nu exista stare pe disc. Port ocupat: iese cu mesaj clar.
- Fara conexiuni externe; productia ramane protejata de sesiune (pagina nu are marcajul de preview decat in launcher).

## Limite
Fixture-ul nu alimenteaza inca pagina (nu exista endpoint de gestiuni pana la VC02/VC03); nu certifica FK/INT/ocuparea exclusiva MariaDB. Nu a fost pornit.
