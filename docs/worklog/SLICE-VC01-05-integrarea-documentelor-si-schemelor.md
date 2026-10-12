# SLICE-VC01-05 — documente si scheme

Stare: **IMPLEMENTAT_LOCAL**. Nimic rulat, testat sau previzualizat (regula: fara teste/randari decat la cerere); nimic validat pe server.

## Facut
- `static/js/vercon/documents.js`: print/PDF peste helperul ADE, xlsx simplu (fara sabloane), context DC/subunitate/gestiune in antete; preferinte cu prefix `vercon.`.
- `VERCON/INTEGRARE_SISTEME.md`: matrice de integrare, plan de publicare (AvacontPush, `PYTHON/.pushignore`) si propagare schema (AVACONT_SURSA → DC; AVACONT_COMUN nu e tinta).

## Limite
Rutele `/api/vercon/reports/*` nu exista inca (VC08-03). Regresia ADE/portal nu a fost rulata.
