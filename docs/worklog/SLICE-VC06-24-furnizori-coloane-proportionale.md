# SLICE-VC06-24 — Furnizori: coloane DGV proporționale

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

La cererea utilizatorului, grila Furnizori folosește proprietatea CUSTOM `proportionalWidths: true`, ca nomenclatoarele existente. Lățimile Denumire / Cod fiscal / Cod partener sunt ponderi ale spațiului disponibil, cu `minWidth: 0`; coloana de opțiuni păstrează lățimea fixă de 36. `layout: {}` înlocuiește umplerea exclusivă a coloanei Denumire. Controlul repartizează lățimea disponibilă ținând cont de scrollbarul vertical. Nu se maschează overflow-ul prin CSS și nu se modifică DGV-ul comun.

Fișier de aplicație: `PYTHON/static/js/vercon/partners.js`.

Fără teste, build, verificări automate/de sintaxă, browser, HTTP sau SQL. Au fost citite contractul de dimensionare al controlului și utilizarea din nomenclatoare pentru proiectare. Rezultatul nu a fost probat vizual. Publicarea necesită numai fișierul JS, presupunând tranșele precedente deja publicate.
