# SLICE-VC03-02 — tabelele business, tipurile, FK

Stare: **IMPLEMENTAT_LOCAL**. Scripturile sunt scrise si generatorul a rulat (a produs fisierul); **nimic nu a fost aplicat pe MariaDB si nicio proba nu a fost rulata**.

## Facut
- `VERCON/tools/ddl.js` (citeste doar exporturile VERCON_ACCESS; foloseste acelasi cititor/clasificare ca dictionarul, mutat in `lib_tz.js`) genereaza `sql/VC_AVACONT_SURSA.sql`: 26 tabele, FK compuse pe context, INT scalat, `CHECK pret >= 0`, RESTRICT, anulare cu motiv, `TipDoc`/`IdPartener` pe NIR.
- Diagrama relatiilor: `VERCON/SCHEMA_VC_RELATII.mmd`.
- Deviatii deliberate listate in `VERCON/SCHEMA_VC.md` (Furnizori → Parteneri, legaturi logice fara FK, fara unicitate pe numere).

## Limite
Tratamentul exact pe fiecare relatie (fosta D05) este cel din generator: RESTRICT peste tot; ajustari dupa profilarea din migrator. `ProduseM` nu are FK spre ProduseP pana la profilare. Nerulat.
