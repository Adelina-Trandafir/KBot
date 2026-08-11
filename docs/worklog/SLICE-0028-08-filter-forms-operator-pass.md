# SLICE-0028-08 — Trecerea OPERATORULUI peste cele două ferestre de filtrare (+ ce a rupt designerul pe drum)

Felie de **livrare**, nu de proiectare: operatorul a lucrat singur în designerul Visual Studio peste
ce a lăsat 0028-06/07 și a cerut ca tot ce era necomis să intre, «even the work you didn't do».
Fișierul acesta scrie ce a intrat, ca peste o lună să se poată citi de ce arată așa.

---

## 1. Meniul de filtrare — rândurile de meniu (autor: operatorul)

`KBotFilterPopup.vb` a primit `AplicaRandDeMeniu`, chemată din `OnThemeChanged` pentru cele patru
comenzi de sus. Ele nu mai sunt butoane, ci **rânduri**: plate, fără chenar, în culoarea suprafeței
pe care stau, scoase în relief doar de hover — ca într-un meniu de sistem.

Motivul e scris în cod și merită repetat aici, fiindcă e o interacțiune fină între două lucruri
corecte fiecare în parte: schema **Modern** randează orice `Button` owner-drawn, adică îi taie
colțurile cu un `Region` de rază 8 și-i pune fundalul de buton. Pe un rând lat cât meniul, prin cele
patru decupaje se vedea suprafața de dedesubt — meniul arăta ca patru pastile gri lipite pe o foaie
albă. `ModernRenderer.DetachButton` scoate `Region`-ul și, pe drumul deschis în 0028-07, redă și
marginea și înălțimea AUTORATE. Apelul e idempotent și pe celelalte scheme (care oricum nu rotunjesc
nimic), deci rândul iese la fel peste tot.

Tot aici: roșul lui «Șterge filtrul» vine acum din **paletă** (`ErrorColor`), nu din `Firebrick`-ul
scris o dată în designer — adică se schimbă odată cu schema, ca orice altă culoare a casei.

**Două defecte reparate în același drum**, amândouă în deschiderea submeniului de condiții:

1. `CustomPopup` ridică `ItemClicked` ÎNAINTE de `Close`, iar dialogul de condiție e modal — deci
   submeniul rămânea pe ecran, viu și inutil, până se închidea dialogul. Acum se cheamă `Hide()`
   pe el înainte de `AplicaConditia`.
2. `_suppressDeactivate = True` se pune ACUM ÎNAINTE de `Hide()`: ascunderea ferestrei active mută
   activarea pe altcineva, adică ridică `OnDeactivate` — care, cu garda pusă după, închidea chiar
   meniul pe care tocmai îl foloseam.

## 2. Dialogul de condiție — rescris pe `TableLayoutPanel` (autor: operatorul)

`KBotFilterConditionDialog` stătea pe `Location`/`Size` absolute. Acum totul e într-un `tlyMAIN`
(două coloane, șase rânduri), deci se poate rearanja în designer fără să se recalculeze poziții.

Consecința în cod: ascunderea celei de-a doua casete (există doar la «Între…») nu se mai face
mutând butoanele cu mâna, ci **strângând la zero rândurile** pe care stau cele două controale, iar
fereastra se scurtează cu exact cât s-a strâns. `StrangeRandul` **citește** rândul din tabel
(`tlyMAIN.GetRow`) în loc să scrie un indice fix — altfel prima reordonare în designer, adică exact
rostul pentru care s-a trecut la tabel, ar fi stricat socoteala în tăcere. Constructorul a primit și
`Try/Catch` cu re-aruncare (regula casei pentru un punct de intrare): un dialog pe jumătate așezat e
mai rău decât unul care nu s-a deschis.

## 3. Ce a rupt designerul pe drum: `PlatiView` (reparat aici)

Deschiderea vederii în designerul VS a ȘTERS bucla care adăuga cele nouă rânduri `AutoSize` din
`detailTable` — designerul nu poate reciti cod pe care nu l-a scris el, așa că l-a rescris fără ea.
A rămas o singură linie, `RowStyles.Add(Percent 100)`, iar **stilurile sunt POZIȚIONALE**: ea
nimerea pe rândul 0, deci «Nr. document» absorbea tot spațiul în locul lui «Explicații», care e
rândul 9. Simptomul ar fi fost un panou de detaliu cu primul rând uriaș și restul înghesuite.

Reparat scriind cele nouă rânduri **unul câte unul** — pe acelea designerul le poate reciti, deci nu
le mai pierde.

E același tipar cu defectul din 0028-05 (trecerea de auto-dimensionare care autora `.Designer.vb`)
văzut din partea cealaltă: **ce nu poate designerul reciti, dispare la prima deschidere.**

## 4. Restul care aștepta comiterea

- `src/KBot.Controls/My Project/Resources.resx` + `Resources.Designer.vb` — proiectul `KBot.Controls`
  a primit propriul fișier de resurse (gol deocamdată, doar șablonul VS) plus legarea lui în
  `.vbproj`. **Trebuie comis**: fără el, `.vbproj` trimite către un fișier care nu există pentru
  oricine altcineva clonează.
- `PlatiView.resx`, `KBotFilterConditionDialog.resx` (+ `$this.Icon`) — resurse generate de VS la
  salvarea formularelor.
- `KBotFilterPopup.Designer.vb` — ajustările vizuale ale operatorului: cursor de mână pe cele două
  rânduri de comandă, «Operatori filtru» în loc de «Filtre...», înălțimi și umpluturi refăcute.

## Fișiere atinse

| Fișier | Autor | Ce s-a schimbat |
|---|---|---|
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | operator | `AplicaRandDeMeniu`, roșul din paletă, cele două defecte din deschiderea submeniului |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Designer.vb` | operator | ajustări vizuale (cursor, texte, înălțimi) |
| `src/KBot.Controls/DataView/Filter/KBotFilterConditionDialog(.Designer/.resx).vb` | operator | rescris pe `tlyMAIN`, `StrangeRandul`, `Try/Catch` în constructor |
| `src/KBot.Controls/My Project/Resources.resx` + `.Designer.vb`, `KBot.Controls.vbproj` | operator | fișierul de resurse al proiectului de controale |
| `src/KBot.App/Views/PlatiView.Designer.vb` | **reparat aici** | cele nouă rânduri `AutoSize`, scrise unul câte unul |
| `src/KBot.App/Views/PlatiView.resx` | VS | resursă generată la salvare |

**Versiuni**: `KBot.Controls` 1.18.0.0 → **1.19.0.0**; `KBot.App` 1.0.14.0 → **1.0.15.0**.

## Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori, 0 avertismente
KBot.Controls.Tests      → 709 / 709
KBot.Theming.Tests       →  71 /  71
KBot.DevHarness.Tests    → 170 / 170
Common 14/14 · Xfa 39/39 · LocalStore 1/1
```

`App` (10), `Domain` (3) și `Api` (1) — aceleași eșecuri preexistente, cu ACELAȘI număr ca înainte
de felie (verificate la HEAD într-un worktree curat la comiterea din 10.08.2026).

## Rămase neverificate / amânate

- **Nimic din felia asta n-a fost probat de mine pe ecran.** Partea operatorului a fost VĂZUTĂ de el
  în designer și la rulare — de acolo vin cererile — dar rândurile de meniu, dialogul pe tabel și
  panoul `PlatiView` reparat aici n-au fost confirmate vizual după ultima modificare.
- **`PlatiView` e reparat pe hârtie, nu pe ecran.** Niciun test nu prinde ordinea stilurilor de rând
  (ele n-au consecință verificabilă headless, fiindcă panoul e `Visible = False` până la o selecție).
  Merită o privire la prima rulare a vederii Plăți.
- **Aceeași ștergere de buclă poate lovi orice `.Designer.vb` deschis în VS.** `PlatiView` a fost
  prins fiindcă diferența era în arborele de lucru; alte vederi cu cod scris de mână în designer
  (bucle, `For Each`, apeluri ajutătoare ca `InitDetailPair`) pot pierde același fel de linii la
  prima deschidere. `InitDetailPair` a supraviețuit aici, dar e o chestiune de noroc, nu de regulă.
- `My Project\Resources.resx` din `KBot.Controls` e GOL: legarea există, resursele nu.
