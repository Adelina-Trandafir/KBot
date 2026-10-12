# SLICE-VC06-04 — BR și FCS

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Retur BC (BR)** (`create_br/edit_br`): se alege un BC existent; produsele lui apar cu cantitatea de pe bon, ce s-a returnat deja si ce a ramas; se returneaza partial si de mai multe ori. Antet nou in `VC_Iesiri_H` (`Retur=1`, `IdRetur` = bonul sursa, primitor/categorie mostenite, numar `BX<n>` ca in Access), linii in `VC_IesiriRetur` la **pretul bonului**. Cantitate peste cea ramasa sau data inaintea bonului → refuz. Mareste stocul.
- **Storno recepție (FCS)** (`create_storno/edit_storno`): se alege un NIR; produsele cu cantitatea primita, deja stornata si ramasa; storno partial/multiplu. Antet nou in `VC_NIR` (`Storno=1`, `IdStorno` = NIR-ul sursa, furnizor mostenit, numarul stocat **negativ**, contor separat), linii in `VC_IntrariStorno` la **pretul din recepție**. Scade stocul de la data documentului storno (un storno ulterior nu schimba disponibilul unei date anterioare).
- Pretul nu poate fi negativ (vine din documentul sursa, care are pret ≥ 0); eroarea nu lasa document partial (o singura tranzactie).
- Pagina: alegerea sursei, apoi grila cu «De stornat / De returnat»; produsele fara cantitate nu intra in document.

## Fisiere
`PYTHON/routes/vercon/`: `documents.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`, `app.js`.

## Presupuneri si neverificat
- Pretul de storno/retur este pretul **documentului sursa**, nu `ProduseP.PUTVA` actual (liniile isi pastreaza valorile proprii, Q9). Daca o recepție are doua linii ale aceleiasi variante la preturi diferite, se foloseste cel mai mic.
- Retururile/storno nu ating registrul OI/MF (VC07). Efectul unui BR/FCS pe MF/OI nu este definit (D04/D10 deschise): FCS este permis pe toate tipurile, BR doar pe Materiale/Alimente.
- Nimic rulat.
