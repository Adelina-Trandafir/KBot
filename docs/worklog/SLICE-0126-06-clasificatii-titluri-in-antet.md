# SLICE-0126-06 - Clasificatii: titlurile tabelelor in antetul primei coloane (07.11.2026)

Operator, 07.11.2026 (captura cu sumarul unui nod): etichetele de deasupra tabelelor se muta in antetul coloanei «Clasificatie» (textul «Clsf» se poate elimina) si se scot din formular. Numar de felie: urmatorul liber din 0126; de confirmat.

## What changed and why
1. **Scoase din formular:** `lblBuget`, `lblRectificari`, `lblAngajamente` (declaratii, blocuri, `Friend`, culorile din `ApplyTheme`). `tlyRight` are 4 randuri (buget, rectificari, total, angajamente) in loc de 7; intre tabele ramane un gol de 12 (la 144 dpi) prin `Margin` pe tabelele de rectificari si angajamente, iar `ApplyGridHeights` le da inaltimile (`SetRowHeight(0, …)`, `SetRowHeight(1, … + 8)`; 8 = golul, in px logici).
2. **Titlul e antetul primei coloane** (`SetTitle`, scris la rulare cu anul si clasificatia): buget = coloana `clsf` («Buget 2026 — 65.02.04.02.20.01.04», pe nod «Buget 2026 — 65.10 (02E): 20 clasificatii»); rectificari = `clsf` pe nod («Rectificari bugetare 2026 — total pe clasificatie»), iar pe o clasificatie, unde `clsf` e ascunsa, coloana `document` («Rectificari bugetare 2026 — Nr. doc.», ca sa nu se piarda numele coloanei); angajamente = coloana `angajament` («Angajamente — …: n», «se incarca…» cat citeste). Textul «Clsf» a disparut din antetul coloanei.
3. **Denumirea clasificatiei** (era tooltipul etichetei de buget) e acum `HeaderTooltip` al antetului de buget (tooltip de coloana, permis; tabelul in sine nu are tooltip propriu).
4. **Pagina Venituri** (la cererea operatorului, dupa prima parte): la fel — `lblBuget` si `lblRectificari` scoase din `ClasificatiiVenituriView`, titlurile in antetul coloanei `clsf` (buget; rectificari pe nod) sau `document` (rectificari pe o clasificatie), denumirea in `HeaderTooltip`; `tlyRight` are 3 randuri (buget 40%, rectificari 60%, total), cu golul de 12 prin `Margin` pe tabelul de rectificari.
5. **Ajutor** (`0126-06`): `clasificatii.md`.

## Files touched
`src/KBot.App/KBotForm/Views/Nomenclatoare/ClasificatiiBugetView.vb`, `.Designer.vb`; `src/KBot.App/Avacont/Configurari/ClasificatiiVenituri/Views/ClasificatiiVenituriView.vb`, `.Designer.vb`; ajutor `HelpContent/contabil/nomenclatoare/clasificatii.md`; status. FileVersion nebumpat.

## Test results
Compilare `KBot.App` (Debug, alt director de iesire): 0 erori, 0 avertismente. Nimic rulat, nici un test.

## Unverified / deferred
- **Nevazut pe ecran.** Nu s-a verificat cum se taie un titlu lung in antet (pe nod, in coloana care se intinde, incape; pe o clasificatie cu «Nr. doc.» titlul poate fi taiat daca fereastra e ingusta) si nici ca golul de 12 si inaltimile cad exact (scazut 3 etichete din coloana: tabelul «Angajamente» capata ~150 px).
- Pagina Venituri: aceleasi lucruri nevazute pe ecran; nu are tabel de angajamente.
- Pasii de tutorial/tur nu citeaza etichetele (verificat prin cautare).
