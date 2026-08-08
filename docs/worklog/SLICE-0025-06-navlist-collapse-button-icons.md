# SLICE-0025-06 — `KBotNavList`: pictograme pe butonul de strângere + unghiul care se întoarce

Pasul 6 al feliei 0025 (vezi `SLICE-0025-designer-surfaces.md` pentru pașii 1 și 7,
`SLICE-0025-02-navitem-icon.md` pentru pictograma elementelor, `SLICE-0025-05-navlist-collapse-padding.md`
pentru butonul de strângere însuși). A treia cerere de operator în afara planului:

> «in kbotnavlist, the expander button must have an exposed property to allow for puting a
> expanded icon and an collapsed icon in the designer (and in code). if none is supplied use the
> current `DrawCollapseButton`, but with the mention that when collapsed it should show the other
> arrow `>` instead of the same `<`»

---

## 1. Ce s-a schimbat și de ce

### 1.1 Cele două pictograme

```vb
<Category("K-BOT")>
Public Property CollapseExpandedImage As Image     ' desfășurată => butonul strânge
<Category("K-BOT")>
Public Property CollapseCollapsedImage As Image    ' strânsă     => butonul desfășoară
```

Tipul e `Image`, nu `Icon`/`Bitmap`, și numele conține `Image`, exact ca `KBotNavItem.Image` de la
pasul 02 — asta e ce face grila de proprietăți să deschidă **editorul de imagine stoc** și
designer-ul să depună imaginea în `.resx`-ul formularului. Ambele merg și din cod, evident
(setter public, nicio validare de stare).

**Sunt INDEPENDENTE.** Cine dă doar una primește unghiul desenat pe cealaltă stare; cererea a fost
«să pot pune o pictogramă», nu «un set complet sau nimic». Rezolvarea se face pe starea curentă,
într-un singur loc (`CollapseButtonImage`), folosit și la pictare și de cârligul de test — aceeași
regulă ca la geometria pictogramei de la 0025-02: o singură funcție, ca cele două să nu se
despartă.

**O singură pictogramă pentru amândouă stările strânse** (`Icons` și `Complete`): din amândouă
butonul face același lucru — desfășoară. Trei proprietăți ar fi cerut operatorului să aleagă o
imagine pentru o distincție pe care butonul n-o face.

**Bara NU deține imaginea** și nu o eliberează niciodată (`Dispose` rămâne neatins) — ca
`KBotNavItem.Image` și `KBotCaptionBar.IconImage`. E a apelantului sau a resurselor formularului;
o pictogramă partajată de două bare n-are voie să moară odată cu prima. Test dedicat.

`ShouldSerialize*` / `Reset*` **private** pentru amândouă, ca la `KBotNavItem.Image` (0025-03): un
tip referință n-are `<DefaultValue>` utilizabil în VB, iar fără ele designer-ul ar fi scris
`navViews.CollapseExpandedImage = Nothing` în fiecare formular care nu s-a atins de proprietate.
`TypeDescriptor` le găsește după nume inclusiv nepublice.

Imaginea se desenează scalată în pătratul butonului (18 px logici, minus 2 px de aer), prin
`DrawItemImage` — funcția care exista deja pentru pictogramele elementelor. Fundalul de hover
rămâne desenat și sub pictogramă: el spune «se poate apăsa», nu «uite o săgeată», deci n-are de ce
să depindă de felul glifei.

### 1.2 Unghiul care se întoarce (defect, nu preferință)

Înainte:

```vb
Dim forward As Boolean = (_collapseState = KBotNavCollapseState.Complete)
```

Adică unghiul se întorcea **doar** din `Complete`. Pe `Icons` rămânea identic cu cel din starea
desfășurată (`<` pe o bară verticală), deci o bară strânsă la pictograme arăta ca una desfășurată
și butonul părea că nu mai face nimic. Acum:

```vb
Private Function ChevronPointsToExpand() As Boolean
    Return _collapseState <> KBotNavCollapseState.Expanded
End Function
```

Asta contează cu atât mai mult de când operatorul a **scurtat ciclul** (vezi §2): `Complete` nu se
mai atinge din buton cât timp `Icons` e disponibil, deci `<` era literalmente singurul unghi pe
care apuca să-l vadă cineva pe o bară cu pictograme.

---

## 2. Ciclul scurtat de operator (schimbare găsită în arbore, nu făcută acum)

`CycleCollapse` avea, necomis, ramura `Icons → Complete` comentată de operator:

```vb
'eliminat de mine (project manager). nu imi place cum arata fara nicio iconita!
'Case KBotNavCollapseState.Icons
'    [next] = KBotNavCollapseState.Complete
```

Deci butonul face acum `Expanded ↔ Icons` cât timp `Icons` e disponibil, și `Expanded ↔ Complete`
când nu e (o bară fără nicio pictogramă). Starea `Complete` rămâne în API (setter-ul
`CollapseState`) și rămâne acoperită de teste, dar butonul nu mai duce pe nimeni acolo dintr-o
bară cu pictograme.

**Două teste din 0025-05 rămăseseră roșii pe schimbarea asta** (`ClickingTheButton_CyclesIcons…`
și `CollapseStateChanged_FiresOncePerStep…`, ambele fixau ciclul în trei trepte). Erau roșii
ÎNAINTE de pasul 06 — sunt aliniate acum la ciclul care e în cod, cu motivul scris în test.
Comportamentul de rulare **nu** a fost schimbat înapoi: decizia e a operatorului.

---

## 3. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapseExpandedImage` / `CollapseCollapsedImage` + `ShouldSerialize`/`Reset`, `CollapseButtonImage()`, `ChevronPointsToExpand()`, ramura de pictogramă din `DrawCollapseButton`, două cârlige `Friend` de test |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.4.0.0 → **1.5.0.0** (`AssemblyVersion` stă pe loc: doar adăugiri) |
| `tests/KBot.Controls.Tests/KBotNavListCollapseTests.vb` | 7 teste noi; 2 teste existente aliniate la ciclul scurtat (§2) |

Nimic în `MainForm`/`DdfView`: s-a cerut **proprietatea**, nu un set de pictograme. Bara
`MainForm.navViews` n-are nici măcar `Collapsible = True` (de la 0025-05).

---

## 4. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **819 passed / 0 failed / 0 skipped** (812 înainte; `KBot.Controls.Tests`
  335 → 342).
- Teste noi: implicitul `Nothing` (⇒ unghi), alegerea pictogramei după starea curentă, aceeași
  pictogramă pe `Icons` și pe `Complete`, independența celor două (jumătate de set merge),
  întoarcerea unghiului pe **orice** stare strânsă, `ShouldSerialize`/`Reset` prin
  `PropertyDescriptor`, faptul că bara nu eliberează imaginile, și pictare fără excepție în toate
  cele trei stări cu pictograme puse.

---

## 5. Ce rămâne neverificat / amânat

- **Nimic din pasul ăsta nu s-a văzut pe ecran.** Cum arată o pictogramă de 14 px efectivi în
  pătratul de 18, dacă `DrawItemImage` (stretch, fără păstrarea proporției) e destul pentru
  pictograme ne-pătrate, și dacă unghiul întors se citește corect din `Icons` — toate probate doar
  prin `DrawToBitmap` headless («nu aruncă»), ca tot restul feliei. Firele (a)–(j) din 0025 rămân
  deschise.
- **Editorul de imagine din grilă n-a fost deschis** pentru cele două proprietăți. E a doua
  rotundă de IMAGINE din felie care are nevoie de `.resx`-ul formularului (prima e
  `KBotNavItem.Image`, tot neprobată — firul (f)) — dar aici e o proprietate directă pe CONTROL,
  nu printr-un element de colecție, deci e calea simplă a celor două, nu cea grea.
- **Nicio proporție păstrată și nicio dimensiune reglabilă** pentru pictograma butonului: se
  întinde în pătratul de 18 px logici, ca pictogramele elementelor. Dacă operatorul vrea alt
  raport sau alt pătrat, e o proprietate în plus (`CollapseButtonSize`), nu s-a cerut.
- **Fără pictogramă separată pentru hover / apăsat** și fără estompare pe o bară dezactivată — nu
  s-au cerut; butonul din colț n-are nici azi stare de „apăsat".
