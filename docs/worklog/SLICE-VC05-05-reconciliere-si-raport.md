# SLICE-VC05-05 — reconciliere si raport

Stare: **IMPLEMENTAT_LOCAL**. Compilat; **nicio reconciliere reala efectuata**.

## Facut
- Inainte de COMMIT, scriitorul citeste inapoi din MariaDB numarul de randuri si sumele coloanelor de valoare pe fiecare (gestiune, exercitiu, tabel) si le compara cu ce a convertit; orice diferenta sau constatare BLOCAJ => rollback.
- Raport Markdown: numaratori sursa/netransferate/tinta/sume, constatari grupate pe regula (cu exemple), durata, ImportId.
- Rezumat JSON in VC_Imports.Summary si constatarile in VC_MigrationIssues (valoarea originala ramane langa cea tinta).
- Raportul de profilare numara acum ProduseP cu folosire si semnele cantitate/pret pe liniile documentelor.

## Neacoperit
- Comparatia pe cheie si pe NULL/FK camp cu camp (FK le verifica MariaDB la inserare); soldurile anuale nu sunt recalculate independent.
- Proba cu diferenta introdusa deliberat, intreruperea lotului si refuzul dupa marker cu fisier redenumit nu au fost rulate.
