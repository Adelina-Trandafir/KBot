# SLICE-VC04-03 — ACL locale si contul migratorului

Stare: **IMPLEMENTAT_LOCAL**. Verificata doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau aplicat; nimic validat pe server.

## Facut
- Decizie aplicata: fara ACL pe subunitate/gestiune; ACL absenta = rolurile pe DC deschid toate subunitatile/gestiunile DC-ului, iar ocuparea exclusiva separa utilizatorii.
- `sql/VC04_03_privilegii_migrator.sql` (sablon cu <DC>, fara parole): contul migratorului scrie doar tabelele VC_ ale DC-ului tinta, citeste Parteneri si doar identitatea unitatii din AVACONT_COMUN; fara DELETE (exceptie ContextLock), DDL sau GRANT OPTION; verificare cu SHOW GRANTS si revocare.

## Limite
Nerulat; conturile reale se creeaza si se valideaza de administrator, separat de preview.
