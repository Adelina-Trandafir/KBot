# SLICE-VC06-24 — Furnizori: coloane DGV proporționale

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

La cererea utilizatorului, Furnizori folosește proprietatea CUSTOM `proportionalWidths: true`, ca nomenclatoarele: Denumire / Cod fiscal / Cod partener au ponderi din spațiul disponibil, cu `minWidth: 0`; opțiunile rămân fixe la 36. `layout: {}` înlocuiește umplerea exclusivă a Denumirii. Controlul ține cont de scrollbarul vertical, fără mascarea overflow-ului prin CSS sau modificarea DGV-ului comun.

Fișier de aplicație: `PYTHON/static/js/vercon/partners.js`.

Fără teste, build, verificări automate/de sintaxă, browser, HTTP sau SQL. Au fost citite contractul de dimensionare al controlului și utilizarea din nomenclatoare pentru proiectare. Rezultatul nu a fost probat vizual. Publicarea necesită numai fișierul JS, presupunând tranșele precedente deja publicate.
