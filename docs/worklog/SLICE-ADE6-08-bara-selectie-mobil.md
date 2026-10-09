# SLICE-ADE6-08 — bara de selecție numai pe mobil

09.10.2026. SCRIS LOCAL.

## Schimbare

Pe PC, ade-toolbar este ascunsă complet, iar grila centrală nu mai rezervă rândul
de 40px. Tabelul începe de sus, la nivelul panoului LUNA / ANUL din aceeași zonă.
Pe mobil (max-width 980px), rândul barei există pentru cele două comboboxuri.
Textul lunii/grupei este ascuns în ambele moduri; identificatorii rămân în DOM
pentru actualizările existente din app.js. Nu există text vizibil suplimentar.

## Fișiere și verificare

PYTHON/static/css/adechit.css, PYTHON/static/adechit.html, indexul și statusul ADE.
Ajutorul paginii descrie afișarea pe PC/mobil cu referință ADE6-08.
Nu am rulat teste sau verificări vizuale, conform instrucțiunilor utilizatorului;
autorizarea precedentă pentru testare a fost limitată la pornirea aplicației.
Nu s-a modificat backendul sau schema. Fișierele statice se preiau după reîncărcare.

## Predare

Modificări locale, fără commit/publicare. Utilizatorul publică prin AvacontPush.
Nu există SQL nou. Următoarea subfelie: ADE6-09.
