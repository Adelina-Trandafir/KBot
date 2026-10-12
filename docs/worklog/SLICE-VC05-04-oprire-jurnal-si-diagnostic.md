# SLICE-VC05-04 — oprire, jurnal si diagnostic

Stare: **IMPLEMENTAT_LOCAL**. Compilat; nerulat.

## Facut
- Stari: NECITIT, PLAN_CITIT, BLOCAT, TESTAT, IN_IMPORT, OPRIRE_SOLICITATA, ROLLBACK_CONFIRMAT, COMMIT_CONFIRMAT, REZULTAT_NECUNOSCUT, RECONCILIAT (afisate in formular).
- Opreste anuleaza inainte de COMMIT (verificat la tabele si la fiecare 200 de randuri); dupa trimiterea COMMIT-ului nu se promite anulare.
- COMMIT fara raspuns => REZULTAT_NECUNOSCUT, cu indicatia de a folosi Diagnostic; transferul nu se reia singur.
- Diagnostic: citeste markerul VC_Imports, sursele si numarul de randuri pe tabel pentru subunitate; nu insereaza nimic.
- Jurnal: fereastra + fisier `Logs\vrc_migrare_<data>.log` si raportul `.md` alaturat; fara parole.
- O migrare confirmata dezactiveaza definitiv Migreaza in sesiune; pe server, VC_Imports unic pe subunitate o interzice si la o sesiune noua (Testeaza o respinge).
- Formularul nu se inchide cat timp ruleaza o operatie.
