# SLICE-0018 — KBotNavList: separatori + orientare + aliniere Near/Far + vizibilitate; gating MainForm prin ascundere

Patru capabilități noi pe `KBotNavList` (`KBot.Theming`) plus rescrierea `MainForm` ca DDF/ORD
să stea desprinse la baza barei și ca un flag `Are*=FALSE` să **ascundă** butonul, nu doar să-l
dezactiveze.

## Ce s-a schimbat și de ce

### 1. Separatori (`AddSeparator`)
`AddSeparator(Optional align As KBotNavAlign = Near)` — se pot adăuga oricâți. Un separator e un
`NavItem` cu `IsSeparator = True`, neselectabil, desenat ca linie fină (`Palette.BorderColor`) pe
mijlocul slotului, perpendiculară pe axa principală (linie orizontală în vertical, verticală în
orizontal). Cheia lui e internă și auto-generată (`__sep_1`, `__sep_2`, …) tocmai ca să NU
coincidă cu o cheie de buton și să nu strice `FindIndex`/`RequireIndex`. E sărit de mouse
(`IndexAt` ignoră separatorii), de selecție (`SelectIndex` aruncă pe un separator) și de navigarea
cu tastatura.

### 2. Orientare (`Orientation`)
`Orientation As KBotNavOrientation` (`Vertical` implicit / `Horizontal`). Enum public nou
`KBotNavOrientation`. În orizontal:
- lățimea unui buton = textul măsurat (`TextRenderer.MeasureText`) + padding (+ loc de badge),
  cu un minim; în vertical rămâne înălțimea fixă `ItemThickness` (36 DPI);
- textul se centrează (`HorizontalCenter`) în loc de stânga;
- navigarea cu tastatura se remapează pe **Stânga/Dreapta** (în vertical rămâne **Sus/Jos**);
  `IsInputKey` acceptă acum toate patru.
Schimbarea orientării invalidează layout-ul și repictează.

### 3. Aliniere Near/Far (`KBotNavAlign`)
Enum public nou `KBotNavAlign { Near, Far }` pe `AddItem(key, text, align)` (overload nou; vechiul
`AddItem(key, text)` rămâne = `Near`) și pe `AddSeparator(align)`. `Near` ancorează la început
(sus/stânga), `Far` la capăt (jos/dreapta). Grupul Far se calculează separat: se însumează
extinderea tuturor elementelor Far vizibile și grupul se așază de la `capăt - extindereTotală`, în
ordinea listei — deci butoanele Far declarate ultimele apar exact la bază, în ordinea în care au
fost adăugate. Asta „desprinde” DDF/ORD de restul.

### 4. Vizibilitate (`SetItemVisible`)
`SetItemVisible(key, visible)` — rezolvă firul deschis „`KBotNavList` has no `SetItemVisible`”.
Un buton ascuns (`Visible = False`):
- primește `Rectangle.Empty` în layout, deci **nu ocupă spațiu**;
- nu se pictează;
- nu se poate selecta (sărit de `IndexAt` și `SelectIndex`);
- e sărit de navigarea cu tastatura.
Cheie necunoscută => `ArgumentException` (regula casei — fără no-op tăcut), ca la
`SetItemEnabled`/`SetBadge`.

### Layout: un singur `RecalcLayout`, nu `index * height`
Vechea `ItemRect(index)` presupunea sloturi egale, contigue, mereu vizibile — incompatibil cu
sloturi ascunse (spațiu zero), separatori (altă extindere), grupuri Near/Far și orientare. Acum:
- fiecare `NavItem` poartă `Bounds As Rectangle` (slotul calculat);
- `RecalcLayout` face un singur pas: resetează sloturile, calculează extinderea fiecărui element
  vizibil (`ItemExtent`: separator = extindere fixă; buton = grosime fixă în vertical / lățime
  măsurată în orizontal), curge grupul Near de la început și grupul Far de la capăt;
- `_layoutValid` marchează layout-ul „murdar”; `EnsureLayout()` recalculează leneș din `OnPaint`,
  `IndexAt` și navigarea cu tastatura. Se invalidează la `AddItem`/`AddSeparator`/`SetItemVisible`/
  schimbare de `Orientation`/`OnSizeChanged`/`OnFontChanged`.

### MainForm
- DDF/ORD mutate în grupul `Far` cu un `AddSeparator(KBotNavAlign.Far)` deasupra lor — vizual la
  baza barei laterale, despărțite de lista de vederi.
- **`ApplyViewGating` trecut de la `SetItemEnabled` la `SetItemVisible`**: un flag `Are*=FALSE`
  acum **ascunde** intrarea, nu o mai lasă gri-inactivă (cererea operatorului). Comentariul
  vederii a fost actualizat. Fallback-ul la `sumar` când vederea activă dispare rămâne (`sumar` e
  mereu vizibil, deci `SelectedKey = "sumar"` nu aruncă niciodată pe cheie ascunsă).

## Fișiere atinse

- `src/KBot.Theming/KBotNavList.vb` — enum-uri noi `KBotNavOrientation`/`KBotNavAlign`; câmpuri
  `Visible`/`IsSeparator`/`Align`/`Bounds` pe `NavItem`; `Orientation`, `AddItem(…, align)`,
  `AddSeparator`, `SetItemVisible`; `RecalcLayout`/`EnsureLayout`/`InvalidateLayout`/`ItemExtent`/
  `SeparatorExtent`/`DrawSeparator`; `OnSizeChanged` nou; remapare tastatură pe orientare.
- `src/KBot.App/MainForm.vb` — `AddSeparator(Far)` + DDF/ORD pe `Far`; `ApplyViewGating` pe
  `SetItemVisible`.
- `docs/worklog/KBOT_STATUS.md` — rândul slice 0018, firul deschis marcat rezolvat, „Next free
  slice number” → 0019.

## Rezultate teste

- `dotnet build src/KBot.Theming` — **0 warnings / 0 errors**.
- `dotnet build src/KBot.App` — **0 warnings / 0 errors**.

## Rămas neverificat / amânat

- **Fără verdict vizual uman.** Verificat DOAR prin compilare. Bara nu a fost văzută pe ecran —
  nici separatorul, nici grupul Far la bază, nici ascunderea la gating, nici modul orizontal. Ca
  la 0010/0013/0016, harness-ul controalelor UI rămâne nerulat.
- **Fără teste xUnit pentru `KBotNavList`.** Nu existau înainte de această felie și nu au fost
  adăugate: separator/vizibilitate/aliniere Near-Far/orientare (inclusiv matematica din
  `RecalcLayout` și remaparea tastaturii) NU sunt acoperite de teste. Candidat clar pentru o
  felie de urmărire dacă se dorește acoperire.
- **Mod orizontal — cale secundară.** `MainForm` folosește bara verticală; orizontalul e o
  capabilitate generală implementată rezonabil (lățime măsurată, text centrat, Stânga/Dreapta),
  dar niciun consumator real nu-l exercită încă.
- **Ordinea vizuală în navigarea cu tastatura** urmează ordinea din listă, nu poziția pe ecran.
  Pentru MainForm coincide (elementele Far sunt declarate ultimele => și vizual jos). Dacă un
  viitor apelant adaugă butoane Far ÎNAINTE de cele Near, Sus/Jos ar sări „înapoi” la ele — de
  sortat după poziție doar dacă apare cazul.
