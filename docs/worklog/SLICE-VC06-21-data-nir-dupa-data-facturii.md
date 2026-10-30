# SLICE-VC06-21 — Data NIR preluată la schimbarea datei facturii

30.10.2026 — **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

## Cerere și schimbare

La schimbarea datei facturii în editorul NIR, data NIR-ului trebuie să primească aceeași valoare.

`PYTHON/static/js/vercon/docs.js`: evenimentul schimbării câmpului `DataFc`, numai pentru familia `nir`, actualizează atât `header.Data`, cât și controlul DatePicker al datei NIR. Actualizarea controlului nu emite un al doilea eveniment de schimbare. Se aplică la adăugare și modificare, pentru câmpurile editabile. Data NIR poate fi apoi modificată separat.

Schimbarea parcurge și controlul numărului NIR pentru noua dată, apoi validarea ciornei existentă. În cazul unui refuz/unei erori, ambele date sunt restaurate împreună și antetul este redesenat. Salvarea folosește cele două date din ciornă; nu se impune egalitatea permanentă a datelor pe server și nu se rescriu documentele istorice.

`PYTHON/static/help/vercon.json`: explicație în secțiunea de înregistrare NIR. Storno și celelalte familii de documente nu sunt modificate.

## Stare și limite

Numai analiză de proiectare pe surse. Fără teste, compilări, verificări automate, browser, HTTP sau SQL, conform instrucțiunilor proiectului. Comportamentul nu este confirmat prin execuție. Fără publicare sau modificări de schemă/date în MariaDB.
