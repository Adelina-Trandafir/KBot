# SLICE-VC03-06 — golirea tabelelor VC_ pentru refacerea unei migrări

Stare: **IMPLEMENTAT_LOCAL**. SQL generat; **nerulat de mine**. Cererea operatorului, 12.10.2026.

## Facut
- `tools/ddl.js` generează acum și `sql/VC_Golire.sql`: `TRUNCATE` pe toate cele 39 de tabele `VC_*` (cu `FOREIGN_KEY_CHECKS` 0 / 1 în jur) și, la sfârșit, o singură interogare cu patru numere (`subunitati`, `importuri`, `nir`, `harta_chei`), care trebuie să fie 0.
- Golește inclusiv `VC_Imports` (markerul de migrare), deci după el aceeași subunitate poate fi migrată din nou. Fără golire, refuzul celei de-a doua migrări în aceeași subunitate rămâne în vigoare (confirmat de operator pe `000_DEMO`).
- Nu atinge `Parteneri` (nici partenerii adăugați din ANAF) și nicio altă tabelă din afara prefixului `VC_`.
- Se rulează pe o bază aleasă, de exemplu `USE 000_DEMO;`, apoi fișierul. Se regenerează din `ddl.js` dacă se adaugă tabele.

## Neacoperit
- Nu are rol în producție: golirea pe o bază cu date reale șterge tot ce a produs VERCON acolo.
- Rularea scriptului pe server nu a fost făcută de mine; ultima interogare (patru zerouri) nu a fost văzută.
