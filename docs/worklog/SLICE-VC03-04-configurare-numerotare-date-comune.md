# SLICE-VC03-04 — configurare, numerotare, date comune

Stare: **IMPLEMENTAT_LOCAL**. Scripturile sunt scrise si generatorul a rulat (a produs fisierul); **nimic nu a fost aplicat pe MariaDB si nicio proba nu a fost rulata**.

## Facut
- `VC_Settings` (Setari/NR tipizate pe gestiune), `VC_Numbering` (contor per gestiune+exercitiu+fel de document), zecimalele in `VC_Gestiuni.Zec` (nu pe subunitate si nu global).
- Fara copie a identitatii DC/unitatii: raman in AVACONT_COMUN; fara scriere automata acolo.

## Limite
Maparea camp cu camp UNIT/cale → date comune se face in migrator (VC05-02). Nerulat.
