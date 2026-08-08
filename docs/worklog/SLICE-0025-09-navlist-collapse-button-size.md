# SLICE-0025-09 — `KBotNavList`: latura butonului de strângere, expusă

Pasul 9 al feliei 0025. Închide firul **(l)** deschis la 0025-06 («fără `CollapseButtonSize`»).
Cerere de operator:

> «i also need an exposed property for the size of the expander button for navlist»

---

## 1. Ce s-a schimbat și de ce

```vb
<Category("K-BOT")>
<DefaultValue(18)>
Public Property CollapseButtonSize As Integer      ' px logici; 0 = fără buton
```

Implicit **18** — exact valoarea fixă care stătea până acum în `CollapseButtonSide()`, deci o bară
care nu atinge proprietatea arată identic (fixat de test).

### 1.1 Nu e doar desen — e layout

Cele trei locuri care depindeau de constanta 18 se mută automat pe proprietate, fiindcă toate treceau
deja prin `CollapseButtonSide()`:

| Ce | Formulă | Efect al schimbării |
|---|---|---|
| Pătratul butonului | `latură` | se desenează mai mare/mic |
| Banda rezervată pe axa principală | `latură + 2*6` | primul (sau ultimul) element se mută |
| Lățimea în `Complete` | `latură + 2*6` | bara complet strânsă e mai lată |
| Lățimea în `Icons` | `Max(Complete, pad + pictogramă)` | urmează, dacă butonul o depășește |

Din cauza ultimelor două, setter-ul **reaplică dimensiunea pe loc** dacă bara e STRÂNSĂ chiar
atunci (`ApplyCollapseExtent`) — altfel o bară pe `Complete` ar fi rămas la lățimea butonului vechi,
adică proprietatea ar fi părut că nu face nimic exact în starea în care contează cel mai mult. Test
dedicat, inclusiv că desfășurarea readuce **mărimea inițială a operatorului**, nu una derivată din
buton.

### 1.2 Ce e DESENAT în buton se scalează cu el

Unghiul era deja proporțional (brațul = `latură/5`, grosimea liniei = `latură/10`), dar aerul din
jurul pictogramei autorate (0025-06) era **fix, 2 px scalați la DPI**. La o latură de 44 pictograma
ar fi stat lipită de margini; la una de 8 n-ar mai fi rămas nimic din ea. Devine `latură \ 9`, cu
minim 1 — ceea ce dă exact 2 la latura implicită de 18, deci nimic nu se schimbă la valoarea din
oficiu, iar cele două glife se scalează acum la fel.

### 1.3 `0` = fără buton

Aceeași convenție ca `IconSize = 0` («fără pictograme», nu «pictograme de zero pixeli care lasă o
gaură»): la 0 butonul nu se desenează, `CollapseButtonRect()` întoarce `Rectangle.Empty` — deci nu
poate fi nici survolat, nici apăsat — și, important, **nu mai rezervă bandă** (`CollapseBandExtent`
întoarce 0), altfel ar fi rămas o gaură de 12 px în capătul barei pentru un buton inexistent.

E o stare validă, nu o greșeală: o aplicație care strânge bara din propriul ei buton de bară de
unelte, prin `ToggleCollapse()` / `CollapseState`, nu vrea și unghiul din colț. **Dar e și o
capcană, scrisă ca atare în documentația proprietății:** cu `Collapsible = True` și latura 0,
operatorul nu mai poate strânge sau desfășura bara cu mouse-ul — strângerea rămâne exclusiv din cod.
Testul verifică ambele jumătăți: click în colț nu mai face nimic, `ToggleCollapse()` da.

Negativele se aduc la 0, ca la fiecare măsură a barei (un setter de dimensiune care aruncă ar rupe
`InitializeComponent` la o valoare greșită din designer).

---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapseButtonSize`; `CollapseButtonSide()` citește proprietatea; `CollapseButtonRect()` și `CollapseBandExtent()` tratează latura 0; aerul pictogramei devine proporțional |
| `tests/KBot.Controls.Tests/KBotNavListCollapseTests.vb` | 6 teste noi în secțiunea butonului din colț |

`KBot.Controls` `FileVersion` era **deja** pe 1.8.0.0 în arborele de lucru (HEAD e pe 1.7.0.0) —
bump-ul necomis acoperă și felia asta, nu s-a mai adăugat unul. `AssemblyVersion` stă pe loc:
schimbarea e aditivă.

---

## 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **868 passed / 0 failed / 0 skipped**, `KBot.Controls.Tests` la 391.
  Cifra pe soluție NU e comparabilă cu cele 854 de la 0025-08: între timp operatorul a comis
  feliile anterioare și a adăugat WIP propriu pe `AdvancedTreeControl` (inclusiv
  `AdvancedTreeSearchBarTests.vb`, netrecut prin mine). Contribuția feliei ăsteia, măsurată pe
  filtrul `KBotNavList`: **116 → 122**.
- Ce fixează testele: implicitul 18 = vechea valoare fixă; butonul ȘI banda se redimensionează
  împreună, iar primul element coboară cu ele; lățimea din `Complete` urmează **pe loc** o schimbare
  făcută cât bara e strânsă, iar desfășurarea readuce mărimea operatorului; `0` = niciun
  dreptunghi, nicio bandă, click-ul în colț inert, dar `ToggleCollapse()` funcțional; negativele se
  limitează la 0; pictarea nu aruncă la 0 / 6 / 18 / 44, cu pictogramă și fără.

---

## 4. Ce rămâne neverificat / amânat

- **Tot nimic pe ecran** (a cincea felie la rând). Nu se știe cum arată un buton de 30 față de unul
  de 18 lângă rândurile de 36 px, nici dacă unghiul rămâne lizibil la laturi mici.
- **Marginea din jurul butonului (6 px logici) NU e expusă.** S-a cerut latura; marginea intră în
  bandă și în lățimea strânsă la fel de mult, deci dacă se dorește control complet asupra colțului
  e o proprietate în plus (`CollapseButtonMargin`), nu o ajustare a acesteia.
- **Nicio gardă pentru un buton mai lat decât bara.** Pe o bară DESFĂȘURATĂ îngustă, o latură mare
  împinge dreptunghiul la coordonate negative (butonul iese pe stânga). În stările strânse nu se
  poate întâmpla — acolo lățimea barei se calculează DIN latura butonului. Limitare consemnată,
  aceeași filozofie ca la `ItemWidth`: valoarea e a operatorului.
- Firele (a)–(k) și (m)–(t) ale feliilor 0025 / 0026 rămân deschise; **(l) se închide pe jumătate**
  — `CollapseButtonSize` există acum; păstrarea proporției pictogramei și o glifă separată de
  hover/apăsat tot nu.
