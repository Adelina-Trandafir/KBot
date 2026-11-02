# SLICE-VC06-21 — Data NIR preluată la schimbarea datei facturii

30.10.2026 — **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

## Cerere și schimbare

La schimbarea datei facturii în editorul NIR, data NIR-ului trebuie să primească aceeași valoare.

În `PYTHON/static/js/vercon/docs.js`, schimbarea `DataFc` actualizează `header.Data` și DatePicker-ul NIR fără al doilea eveniment. Numai familia `nir`, la adăugare/modificare, pe câmpuri editabile; data NIR rămâne apoi modificabilă separat.

Se controlează numărul NIR pentru noua dată, apoi se validează ciorna ca înainte. Refuzul/eroarea restaurează ambele date și redesenează antetul. Salvarea folosește datele ciornei, fără egalitate permanentă impusă pe server sau rescrierea documentelor istorice.

`PYTHON/static/help/vercon.json`: explicație în secțiunea de înregistrare NIR. Storno și celelalte familii de documente nu sunt modificate.

## Stare și limite

Numai analiză de proiectare pe surse. Fără teste, compilări, verificări automate, browser, HTTP sau SQL, conform instrucțiunilor proiectului. Comportamentul nu este confirmat prin execuție. Fără publicare sau modificări de schemă/date în MariaDB.
