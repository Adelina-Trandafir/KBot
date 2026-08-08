# SLICE-0025-05 — `KBotNavList`: bară colapsabilă + `ItemPadding`

Pasul 5 al feliei 0025 (vezi `SLICE-0025-designer-surfaces.md` pentru pasul 1 și pasul 7,
`SLICE-0025-02-navitem-icon.md` pentru pictogramă). Cerere de operator, în afara planului:

> «navlist-ul să aibă o proprietate vizibilă în designer "Colapsable". Când e true, o pictogramă
> mică apare într-unul din colțuri (proprietatea `CollapseCorner {TopLeft, TopRight, BottomLeft,
> BottomRight}`). Strâns are două opțiuni: "Complete" și "Icons". La "Complete" lățimea barei (sau
> înălțimea, dacă e orizontală) devine puțin mai mare decât butonul de strângere. La "Icons", doar
> pe verticală, se redimensionează și bara și butoanele la lățimea pictogramei plus niște padding;
> dacă niciun buton nu are pictogramă, opțiunea nu e disponibilă. Merg din același buton: 1 =
> icons, 2 = complet, 3 = mărimea inițială. În plus, navlist-ul trebuie să expună o proprietate de
> padding, ca să pot controla aerul din jurul butoanelor.»

---

## 1. Ce s-a schimbat și de ce

### 1.1 `ItemPadding` — aerul din jurul butoanelor

```vb
<Category("K-BOT")>
<DefaultValue(GetType(System.Windows.Forms.Padding), "6, 6, 6, 6")>
Public Property ItemPadding As System.Windows.Forms.Padding
```

Înainte, `RecalcLayout` folosea o **margine fixă** `ThemeShapes.ScaleDpi(Me, 6)` pe toate cele
patru laturi. Acum cele patru laturi vin din proprietate; implicit tot 6, deci o bară care nu
atinge proprietatea arată **exact** ca înainte (fixat de un test dedicat, nu doar afirmat).

Pe verticală `Left`/`Right` strâng coloana butoanelor (deci și lățimea lor, cât sunt pe „umple
bara"), iar `Top`/`Bottom` depărtează primul buton de sus și ultimul buton `Far` de jos. Pe
orizontală rolurile se inversează. Valorile negative se aduc la 0, ca la `IconSize` / `ItemWidth`
— un setter de dimensiune care aruncă ar rupe `InitializeComponent` la o valoare greșită din
designer.

**De ce `ItemPadding` și nu `Padding`.** `Control.Padding` există deja pe orice control și e
**scalat automat de WinForms** la autoscalarea formularului (`Control.ScaleControl` ajustează
`Padding` și `Margin`). Bara asta își scalează singură fiecare măsură logică prin
`ThemeShapes.ScaleDpi` — înălțimea rândului (36), padding-ul intern al textului (12), latura
separatorului (11), `IconSize`, `ItemWidth`. Dacă aerul din jurul butoanelor ar sta pe
`Control.Padding`, aceeași valoare ar fi ajustată de **două** ori pe un ecran la 150%. Așa că:

- `ItemPadding` = px **logici**, scalat de noi, consecvent cu tot restul barei;
- `Control.Padding` moștenit e **ascuns din grilă** (`Browsable(False)` +
  `DesignerSerializationVisibility.Hidden`, prin `Shadows`), fiindcă pe un layout owner-drawn nu
  face nimic. O proprietate care nu face nimic, lăsată în grila de proprietăți exact lângă una
  care face, este prima pe care ai încerca-o.

### 1.2 Strângerea barei

Trei proprietăți + un eveniment + o metodă:

| Membru | Rol |
|---|---|
| `Collapsible As Boolean` (implicit `False`) | desenează butonul mic din colț și rezervă banda lui |
| `CollapseCorner As KBotNavCorner` (implicit `TopRight`) | `TopLeft` / `TopRight` / `BottomLeft` / `BottomRight` |
| `CollapseState As KBotNavCollapseState` | `Expanded` / `Icons` / `Complete`; **nu se serializează** |
| `IconsCollapseAvailable As Boolean` (ReadOnly) | dacă „Icons" are sens ACUM |
| `ToggleCollapse()` | trece la starea următoare, ca un click pe buton |
| `CollapseStateChanged(state)` | ridicat la fiecare trecere reală |

**Numele.** Cererea zicea `"Colapsable"`; proprietatea se numește `Collapsible` — ortografia
engleză corectă, consecventă cu `CollapseCorner` / `CollapseState`, care au venit în engleză din
aceeași cerere. Restul API-ului barei e tot în engleză (`Items`, `Orientation`, `IconSize`,
`ItemWidth`); descrierile din grilă rămân în română.

**Ciclul** e cel cerut: `Expanded → Icons → Complete → Expanded`. Butonul **sare** starea `Icons`
când nu e disponibilă (bară orizontală, `IconSize = 0`, sau niciun buton **vizibil** cu
pictogramă) — un buton care aruncă în fața operatorului fiindcă nicio intrare n-are pictogramă ar
fi o pedeapsă pentru o alegere de autorare. Setter-ul `CollapseState`, în schimb, **aruncă**
`InvalidOperationException` pe aceeași stare: e API, iar regula casei e „fără no-op-uri tăcute".

**Dimensiunile** (px logici, scalați):

| Stare | Latura pe axa care se strânge |
|---|---|
| `Complete` | `18 + 2*6 = 30` — «puțin mai mult decât butonul» |
| `Icons` | `ItemPadding.Left + ItemPadding.Right + IconSize + 2*8` (implicit `6+6+20+16 = 48`), niciodată sub `Complete` |
| `Expanded` | ultima mărime avută **desfășurată** |

Lățimea butoanelor în `Icons` = `IconSize + 2*8` (implicit 36) și bate **și** `AutoSize` pe
element **și** `ItemWidth` pe bară: bara s-a strâns la lățimea unei pictograme, nicio cerere de
lățime nu mai are ce cere.

**„Mărimea inițială" e a operatorului, nu a constructorului.** `OnSizeChanged` reține mărimea
curentă ca `_expandedExtent` cât timp starea e `Expanded`, deci dacă operatorul lățește bara la
240 și apoi o strânge, al treilea click îi dă înapoi 240, nu 170. Steagul
`_applyingCollapseExtent` oprește propriile noastre redimensionări să fie confundate cu ale lui —
fără el, prima strângere ar deveni noua „mărime inițială" și bara nu s-ar mai putea desfășura
niciodată.

**Banda rezervată.** Butonul din colț își ia o bandă de `18 + 2*6` la capătul axei principale
dinspre colțul ales (sus/jos pe verticală, stânga/dreapta pe orizontală), ca să nu stea peste
primul sau ultimul buton. Se pictează ultimul, peste tot, fiindcă pe o bară foarte îngustă un
buton lat tot i-ar putea intra pe dedesubt.

**`Complete` nu lasă niciun slot.** `RecalcLayout` iese devreme, toate `Bounds` rămân
`Rectangle.Empty` — deci nici pictarea, nici `IndexAt`, nici hover-ul nu mai ating vreun buton.
Un buton pictat „tăiat" într-o bară de 30px ar fi fost gunoi vizual, iar unul apăsabil, invizibil,
ar fi fost mai rău.

**Pictarea în `Icons`:** pictograma se **centrează** (nu mai are text lângă care să stea), textul
dispare, iar pastila badge-ului devine un **punct** în colțul din dreapta-sus — informația «are
ceva de arătat» nu are voie să dispară complet. Un buton **fără** pictogramă primește **inițiala**
textului, centrată: starea se poate cere de îndată ce UN singur buton are pictogramă, iar restul
trebuie totuși să se distingă între ele.

**Butonul** e un unghi („chevron") desenat cu `GraphicsPath`-ul obișnuit, în `TextDimColor`
(accent la hover, cu fundal `ButtonHoverColor`). Arată spre începutul axei cât mai sunt trepte de
strâns și înapoi din `Complete`. Zero culori hardcodate.

**În designer NU se strânge:** click-ul pe buton e ignorat la design time — ar redimensiona
controlul și ar murdări formularul cuiva. Butonul se **desenează** totuși, ca să se vadă unde
cade colțul ales.

**Schimbarea axei cât bara e strânsă** (caz marginal, dar tratat): se desfășoară întâi pe axa
veche — altfel lățimea strânsă ar rămâne agățată de o axă care nu mai e cea care se strânge —,
apoi mărimea curentă de pe axa nouă devine „mărimea inițială", `Icons` se retrogradează la
`Complete`, și se restrânge.

---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Theming/KBotNavList.vb` | enum-urile `KBotNavCorner` / `KBotNavCollapseState`; `ItemPadding`, `Padding` ascuns, `Collapsible`, `CollapseCorner`, `CollapseState`, `IconsCollapseAvailable`, `ToggleCollapse`, evenimentul `CollapseStateChanged`; geometria strângerii; `RecalcLayout` / `IconRect` / `CrossWidthFor` / `OnPaint` / `OnMouseMove` / `OnMouseClick` / `OnMouseLeave` / `OnSizeChanged` / setter-ul `Orientation`; două cârlige `Friend` noi pentru teste |
| `src/KBot.Theming/KBot.Theming.vbproj` | `FileVersion` 1.2.0.0 → **1.3.0.0** (`AssemblyVersion` stă pe loc — nu se rupe nimic la compilare) |
| `tests/KBot.Theming.Tests/KBotNavListCollapseTests.vb` | **nou**, 25 de teste |

Nimic altceva nu s-a atins. `MainForm.navViews` **nu** a primit `Collapsible = True`: s-a cerut
capacitatea, nu activarea ei. Cu `Collapsible = False` (implicit) bara se comportă bit cu bit ca
înainte — o linie în designer o pornește când se decide.

---

## 3. Rezultate teste

```
dotnet build KBot.sln     → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet test  KBot.sln     → 812 passed / 0 failed / 0 skipped
                            (KBot.Theming.Tests 76 → 101)
```

Cele 25 de teste noi acoperă: implicitul `ItemPadding = 6` **este** vechea margine fixă; cele
patru laturi pe verticală și inversarea lor pe orizontală; clamparea negativelor; `Padding`
moștenit ascuns din grilă (prin `TypeDescriptor`, nu prin reflexie brută); butonul în fiecare din
cele patru colțuri; banda rezervată la capătul corect, pe ambele axe; ciclul complet prin **click
real** (`OnMouseClick`, nu prin setter); saltul peste `Icons` fără pictograme; lățimile din
`Icons` (bară + butoane) și centrarea pictogramei; `Icons` peste `AutoSize`/`ItemWidth`;
`Complete` fără sloturi și fără lovire cu mouse-ul; întoarcerea la mărimea pe care o pusese
operatorul; bara orizontală care își strânge înălțimea, nu lățimea; cele trei stări imposibile
care aruncă; pictograma unui buton **ascuns** care NU deblochează `Icons`; retrogradarea la
schimbarea axei; `Collapsible = False` care desfășoară imediat; evenimentul, o dată pe treaptă și
niciodată pe un no-op; și pictarea headless a fiecărei combinații stare × colț.

---

## 4. Neverificat / amânat

1. **Niciun verdict vizual.** Ca la tot ce s-a livrat din felia 0010 încoace: nimeni nu a văzut
   bara strângându-se pe ecran. Chevron-ul, fundalul de hover, pictograma centrată, punctul în
   locul pastilei și inițiala pe butoanele fără pictogramă sunt probate doar prin `DrawToBitmap`
   headless — adică «nu aruncă», nu «arată bine». `PaintingEveryCollapsedState_DoesNotThrow`
   execută fiecare ramură de pictare, atât.
2. **Nedeschis în Visual Studio.** Cele patru proprietăți noi n-au fost văzute în grila de
   proprietăți; în particular, editorul standard de `Padding` pe `ItemPadding` (cel care desface
   Left/Top/Right/Bottom) și felul în care designer-ul serializează `ItemPadding` sunt
   neverificate. Firul deschis (b) al feliei 0025 rămâne deschis.
3. **`Dock` presupus, nu impus.** Strângerea schimbă `Width` (respectiv `Height`). Asta merge cu
   `Dock.Left`/`Right` (cum e `MainForm.navViews`), `Dock.Top`/`Bottom` sau ancorare. Cu
   `Dock.Fill` containerul rescrie imediat dimensiunea și bara nu se va strânge — nu e o gardă în
   cod (n-am inventat o excepție pentru o configurație pe care nimeni n-a cerut-o), e o limitare
   consemnată aici.
4. **Fără tooltip în `Icons`.** Cu textul ascuns, un tooltip cu numele vederii ar fi util. Nu s-a
   cerut, nu s-a adăugat.
5. **Fără navigare la tastatură pe butonul din colț.** Se acționează doar cu mouse-ul (sau din cod,
   prin `ToggleCollapse`). `Tab` intră în bară ca înainte, iar săgețile schimbă selecția —
   inclusiv cât bara e `Complete`, unde nu se vede nimic. Selecția rămâne o stare validă a
   controlului, deci n-am blocat-o; dacă deranjează, e o linie în `OnKeyDown`.
