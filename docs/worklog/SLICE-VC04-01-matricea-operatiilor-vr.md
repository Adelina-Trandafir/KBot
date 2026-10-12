# SLICE-VC04-01 — matricea operatiilor VR

Stare: **IMPLEMENTAT_LOCAL**. Verificata doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau aplicat; nimic validat pe server.

## Facut
- `routes/vercon/operations.py`: 16 operatii (read, nomenclatoare, receptie, storno, consum, retur, alimente, oi_predare/transfer/casare, mf, raport, import, configure, close, reopen), cu tipurile de gestiune pentru care sunt valide; `effective` = drept SI tip. Formularele din `capabilities.py` cer acum operatia lor, nu un generic `edit`.
- `sql/VC04_01_roluri_vr.sql` (AVACONT_COMUN): cate un rol per operatie (`VR_*`), rerulabil, fara acordare implicita; operatiile se citesc din `Roluri_Operatii` (la fel ca AD).

## Limite
Matricea operatie→ruta se completeaza cu rutele business (VC06+). Nimic rulat; rolurile nu sunt inserate pana nu rulezi scriptul.
