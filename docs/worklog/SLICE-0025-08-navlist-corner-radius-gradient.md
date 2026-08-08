# SLICE-0025-08 — `KBotNavList`: rază de colț per bară + gradient „modern", din motorul de teme

Pasul 8 al feliei 0025. Continuă direct `SLICE-0025-07-navlist-collapsed-flyout.md` (eticheta
plutitoare). Cerere de operator:

> «the appearing button must have rounded corners (also the buttons in the navlist) similar to the
> button. that property should be exposed in the designer per navlist (not per button) and also it
> should have a modern gradient. also it should use the theming system»

---

## 1. Ce s-a schimbat și de ce

### 1.1 Gradientul stă în MOTOR, nu în control

Cererea „also it should use the theming system" s-a citit ca o cerință de arhitectură, nu doar de
culori: umplerea nouă e o funcție PUBLICĂ în `KBot.Theming`, nu cod de pictură ascuns în bară.

```vb
' src/KBot.Theming/ThemeShapes.vb
Public Function Lighten(c As Color, amount As Double) As Color
Public Function Darken(c As Color, amount As Double) As Color
Public Sub FillModern(g, path, bounds, baseColor As Color, strength As Integer)
```

`FillModern` **nu introduce nicio culoare nouă**: primește culoarea de bază din paletă (fundalul de
selecție derivat din accent, `ButtonHoverColor`, …) și îi calculează cele două capete. Deci
gradientul se schimbă odată cu schema, exact ca restul barei, și merge la fel pe schemele deschise
și pe cele întunecate — nimic nu e o constantă.

Două decizii de aspect, amândouă cu motiv:

- **Capetele nu sunt simetrice** — partea de jos se închide cu 3/4 din cât se deschide partea de
  sus. Un gradient simetric arată a buton Windows XP; deschis-sus/abia-închis-jos citește ca lumină
  căzând de sus, care e tot ce înseamnă „modern" aici.
- **`strength = 0` ⇒ umplere PLATĂ**, pe drumul `SolidBrush`, fără alocare de gradient — deci
  «bit cu bit ca înainte» rămâne disponibil și e ce se picta până acum.

Zona gradientului se umflă cu un pixel sus și jos: `LinearGradientBrush` pictează primul/ultimul
rând cu culoarea capătului opus (artefact vechi de rotunjire), iar așa rândurile stricate cad în
afara căii.

**E singura bucată din familia asta de felii care se poate proba HEADLESS pe bune**: un bitmap
spune dacă sus e mai deschis decât jos. Patru teste fac exact asta, inclusiv pe o culoare de schemă
întunecată.

### 1.2 Forma, dată per BARĂ

```vb
<DefaultValue(-1)>  Public Property ItemCornerRadius As Integer    ' -1 = raza schemei active
<DefaultValue(14)>  Public Property ItemGradient As Integer        ' 0..100; 0 = plat
```

**Per bară, nu per element** — cerut explicit, și e și singura variantă corectă: într-o navigație
butoanele trebuie să aibă aceeași formă, altfel coloana arată ruptă (același raționament ca la
`IconSize`, care e tot pe bară). Există un test care cade dacă cineva adaugă vreodată o rază pe
`KBotNavItem`, ca decizia să se reia conștient, nu din inerție.

**`-1` = raza schemei active** (`ThemeStyleOptions.CornerRadius`: 0 pe Classic și Dark, 8 pe
Modern). Ăsta e implicitul, deci: (a) o bară care nu atinge proprietatea arată exact ca înainte, și
(b) urmează tema când operatorul schimbă schema. O valoare de la 0 în sus o înlocuiește — `0`
înseamnă „colțuri drepte chiar și pe o schemă rotunjită", nu „nesetat". De aici și `-1` ca sentinel
în loc de `0`: pe o proprietate de rază, `0` e o valoare reală.

Sub `-1` se limitează la `-1`, iar gradientul la `[0,100]`: un setter de dimensiune care aruncă ar
rupe `InitializeComponent` la o valoare greșită din designer (aceeași regulă ca la `IconSize` /
`ItemWidth` / `ItemPadding`).

Raza efectivă se rezolvă într-o SINGURĂ funcție (`ItemRadius()`), folosită de butoane, de butonul
din colț ȘI de etichetă — altfel cele trei ar putea să se rotunjească diferit. La fel gradientul:
aceeași `ThemeShapes.FillModern` în toate trei.

### 1.3 Eticheta primește exact aceeași formă

`KBotNavFlyoutStyle` capătă `GradientStrength`, iar `Radius` vine acum din `ItemRadius()`. Eticheta
trebuie să fie **butonul care se desfășoară** (vezi 0025-07): o rază sau un gradient care nu se
potrivesc ar strica iluzia mai rău decât lipsa lor. Fixat de test. `Region`-ul ferestrei folosea
deja `_style.Radius`, deci colțurile ferestrei urmează automat.

---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Theming/ThemeShapes.vb` | **NOU:** `Lighten`, `Darken`, `FillModern` |
| `src/KBot.Theming/KBot.Theming.vbproj` | `FileVersion` 1.3.0.0 → **1.4.0.0** |
| `src/KBot.Controls/NavList/KBotNavList.vb` | `ItemCornerRadius`, `ItemGradient`, `ItemRadius()`; umplerea butoanelor și a butonului din colț trece prin `FillModern`; `BuildFlyoutStyle` duce raza + gradientul; 2 cârlige `Friend` de test |
| `src/KBot.Controls/NavList/KBotNavFlyout.vb` | `KBotNavFlyoutStyle.GradientStrength`; umplerea prin `FillModern` |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.6.0.0 → **1.7.0.0** |
| `tests/KBot.Controls.Tests/KBotNavListShapeTests.vb` | **NOU** — 11 teste |

`AssemblyVersion` stă pe loc peste tot: totul e aditiv.

---

## 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **854 passed / 0 failed / 0 skipped** (843 înainte; `KBot.Controls.Tests`
  366 → 377).
- Ce fixează testele: implicitele (`-1` / `14`); `-1` chiar ia raza schemei (Classic 0, Modern 8);
  o rază explicită bate schema în AMBELE sensuri (rotunjit peste o schemă pătrată și drept peste
  una rotunjită) și `-1` o redă schemei; valorile din afara intervalului se limitează, nu aruncă;
  proprietățile sunt pe BARĂ și lipsesc de pe element; eticheta primește exact raza și gradientul
  barei; `FillModern(strength := 0)` e identic cu umplerea plată **pe pixeli**; gradientul chiar
  deschide sus și închide jos, **asimetric**, inclusiv pe o culoare de schemă întunecată; și un
  dreptunghi degenerat (1 px înălțime, lățime 0) nu aruncă.

---

## 4. Ce rămâne neverificat / amânat

- **Tot nimic pe ecran.** Testele de pixeli probează că `FillModern` face ce spune, NU că 14 e
  intensitatea potrivită, că 8 px de rază arată bine pe un rând de 36, sau că gradientul se citește
  la fel pe Dark și pe Modern. Alegerea implicitelor e o judecată, nu o măsurătoare.
- **`Region`-ul ferestrei taie colțurile rotunjite drept, fără antialiasing** — pe o rază mare
  colțurile etichetei pot ieși zimțate. Nu se poate vedea headless; alternativa (fereastră stratificată
  cu alfa) e mult mai scumpă și nu s-a cerut.
- **Restul schemelor nu au fost atinse.** `FillModern` e disponibilă acum pentru orice control, dar
  o folosește DOAR `KBotNavList` (+ eticheta). `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`,
  `KBotNotice`, `KBotTextField` și butoanele din `ModernRenderer` rămân pe umplere plată — s-a
  cerut bara de navigație, iar o trecere generală la gradient e o decizie de aspect pe toată
  aplicația, nu un efect colateral al acestei felii.
- **`ThemeStyleOptions` NU a primit un flag de gradient.** Intensitatea stă pe control, nu pe
  schemă. Dacă gradientul se generalizează (punctul de mai sus), locul lui firesc e o proprietate
  nouă în `ThemeStyleOptions`, serializată în JSON ca restul — moment în care implicitul
  `ItemGradient` ar trebui să devină un sentinel «ia din schemă», ca `ItemCornerRadius` azi.
- Firele (a)–(p) ale feliilor 0025 / 0026 rămân toate deschise.
