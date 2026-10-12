# SLICE-VC01-02 — pagina si ciclul de viata

Stare: **IMPLEMENTAT_LOCAL**. Nimic rulat, testat sau previzualizat (regula: fara teste/randari decat la cerere); nimic validat pe server.

## Facut
- `PYTHON/static/vercon.html`, `css/vercon.css`, `js/vercon/{gate,api,app}.js`; ruta `GET /vercon` (anteturi de securitate, no-store).
- Stari: se incarca / gol / eroare (cu cod diagnostic) / sesiune expirata (iesire la portal).
- Reutilizat: meniul de aplicatii, cronometrul de sesiune, mesajele portalului. Intrarea VERCON activata in meniurile existente si in selectorul `portal.js`.
- Curatare: un singur `AbortController` legat de `pagehide`; nu exista timere proprii.

## Limite
Fara pagina de lucru (VC02). Randarea nu a fost privita. Matricea sistem→curatare: `VERCON/INTEGRARE_SISTEME.md`.
