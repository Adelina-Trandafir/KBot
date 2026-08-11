# SLICE-0028-09 — Butonul de TEMĂ intră în bara de titlu

Cerere de operator, după ce selectorul de teme a mers o felie întreagă din `MainForm`:

> add a new button ThemeButton, right-to-left: just after the minimize/maximize buttons (if
> available, else after the close button) and will be built in the captionbar. it will do what the
> mainform option button does now, but with functions built in the captionbar system. […] there
> will be only one option (true/false) to enable this button. […] there should be another new
> option: "Show Theme Editor" which will show / hide the last button in the popup (the theme editor)

---

## 1. Ce se întâmpla

Meniul de teme era ÎN GAZDĂ. `MainForm` avea ~130 de rânduri care știau de `ThemeManager`, de
literele de acces, de pictogramele schemelor, de garda `CustomPopup.ClosedJustNow`, de
`ThemeEditorForm` și de `capBar.OptionButtonBounds` — iar bara de titlu nu ținea decât butonul gol
și evenimentul lui. Al doilea formular cu bară de titlu ar fi trebuit să COPIEZE tot.

Și chiar exista: `LoginForm` are `ShowOptionsButton = True` cu pictograma `switch_theme` și
**niciun handler** — un buton care arată exact ca selectorul de teme și nu face nimic la clic.

## 2. Ce s-a construit

**`KBotCaptionBar.ThemeButton.vb`** (partial nou, lângă `KBotCaptionBar.vb`) — butonul și tot ce
ține de el. Bara își face SINGURĂ meniul: `ShowThemeMenu()` construiește `CustomPopup`-ul, îl
ancorează pe `ThemeButtonBounds`, comută schema prin `ThemeManager.SetScheme` și deschide
`ThemeEditorForm` pentru rândul «Stiluri...». `ThemeEditorForm` e în `KBot.Controls`, deci nimic
nou nu s-a mutat pentru asta.

Proprietăți (toate cu `<DefaultValue>`, deci **niciun implicit nu ajunge scris în formularul
gazdă**):

| Proprietate | Implicit | Ce face |
|---|---|---|
| `ShowThemeButton` | `False` | **Singurul comutator cerut.** Îl aprinzi și vine cu tot: pictogramă, meniu, litere de acces, comutarea schemei. |
| `ShowThemeEditor` | `True` | Al doilea comutator cerut: stinge ULTIMUL rând al meniului («Stiluri...») — și, cu el, separatorul care nu mai are ce despărți. |
| `ThemeButtonImage` | `Nothing` | Gol = pictograma implicită din resursele K-BOT (`switch_theme`). Pereche `ShouldSerialize`/`Reset`, ca implicitul să nu treacă drept alegere. |
| `ThemeButtonPadding` | `2` | Cât se strânge glifa în slot (valoarea aleasă de operator pe butonul vechi). |
| `TintThemeButtonImage` | `True` | Recolorează glifa cu culoarea celorlalte — altfel, pe schema întunecată, siluetă neagră pe fundal negru. |
| `ThemeButtonBounds` / `ThemeButtonActive` | — | Doar rulare: `<Browsable(False)>` + serializare ascunsă. |
| Evenimentul `ThemeSchemeChanged` | — | Ridicat DUPĂ ce s-a aplicat schema. Gazda **nu** trebuie să reaplice nimic (`SetScheme` difuzează peste toate formularele deschise) — evenimentul e pentru ce e în PLUS. |

**Pictogramele meniului** vin acum din `KBot.Controls\Resources.resx` (`ThemeClassic` / `ThemeDark`
/ `ThemeModern` / `ThemeColorful` / `ThemeEditor` / `switch_theme`), nu din resursele lui
`KBot.App` — controlul nu poate depinde de gazdă.

**Cheia rândului de editor e `@ThemeEditor`, nu «Stiluri».** O schemă de utilizator chiar s-ar
putea numi «Stiluri», iar atunci alegerea ei ar fi deschis editorul în loc să comute tema (era
posibil în varianta din `MainForm`).

## 3. Cele trei lucruri care s-au rupt pe drum și au fost reparate

**(a) Sloturile.** Butonul de opțiuni își calcula slotul cu propria numărătoare (`1 + min + max`).
Al doilea buton pe aceeași formulă ar fi stat FIX PESTE el. Acum există o singură numărătoare —
`ThemeButtonSlot()` — iar opțiunile citesc `ThemeButtonSlot() + If(ShowThemeButton, 1, 0)`. Ordinea
dreapta→stânga e: închidere, maximizare, minimizare, **temă**, opțiuni. Exact ce cerea cererea:
«just after the minimize/maximize buttons (if available, else after the close button)».

**(b) Titlul curgea pe sub butoane.** `TitleRightLimit()` nu se uita deloc la butonul de opțiuni
(defect vechi, invizibil cât timp titlul era scurt). Acum limita e marginea celui mai din stânga
buton VIZIBIL, oricare ar fi el.

**(c) Sinkul `IPopupAnchor` nu mai are un singur client.** Interfața primește un bit — «s-a
deschis un meniu» — și nu spune CARE buton l-a desfășurat. Bara ridică `_themeMenuOpening` chiar
înainte de `ShowBelow` și îl consumă în `SetPopupOpen`: meniul de temă aprinde butonul de temă,
orice altă deschidere (venită de la gazdă, prin butonul de opțiuni) aprinde butonul de opțiuni.
Închiderea le stinge pe amândouă — un buton rămas aprins ar arăta un meniu care nu mai există.

Desenul celor două butoane a fost pus pe aceeași funcție (`DrawImageButton`): sunt același obiect
vizual, iar două desene paralele s-ar fi despărțit la prima reglare de umplutură.

## 4. `MainForm` a rămas cu 9 rânduri

`capBar.ShowThemeButton = True` în designer, în locul celor trei rânduri de buton de opțiuni. Au
DISPĂRUT din `MainForm.vb`: `CapBar_OptionButtonClick`, `OnThemePicked`, `CuLiteraDeAcces`,
`GetThemeIcon` (~130 de rânduri) și câmpul `_suppressThemeEvents` — care, verificat, nu era ridicat
NICĂIERI, deci garda lui era o minciună. A rămas `CapBar_ThemeSchemeChanged`, care doar scrie în
jurnal: restul shell-ului se re-tematizează singur.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` | **nou** — butonul, meniul, comutarea schemei, «Stiluri...» |
| `src/KBot.Controls/CaptionBar/ThemeSchemeChangedEventArgs.vb` | **nou** — argumentele evenimentului |
| `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb` | `Partial`; sloturi dintr-o singură numărătoare; `TitleRightLimit` vede toate butoanele; desen comun `DrawImageButton`/`DrawGlyphImage`; hover + clic + `SetPopupOpen` pe două butoane |
| `src/KBot.App/MainForm.vb` | ~130 de rânduri de meniu de temă scoase; handler nou de 9 rânduri |
| `src/KBot.App/MainForm.Designer.vb` | `ShowThemeButton = True` în locul butonului de opțiuni |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.19 → **1.20** |
| `src/KBot.App/KBot.App.vbproj` | `FileVersion` 1.0.15 → **1.0.16** |
| `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb` | **nou** — 16 probe |

## Rezultate de test

- `dotnet build KBot.sln` — **succeeded, 0 erori, 0 avertismente** (rămâne avertismentul vechi
  `MSB3825` de la `DdfView.resx`, fără legătură).
- `KBot.Controls.Tests` — **725/725 verzi** (cele 16 noi + cele 9 vechi ale butonului de opțiuni,
  nemodificate).
- `KBot.Theming.Tests` — 71/71 verzi.
- Probele noi țin: geometria sloturilor (tema lângă cutia de control, opțiunile lipite la stânga
  ei, ambele urcă un slot când se stinge minimizarea), conținutul meniului (schema activă lipsește,
  fiecare rând are literă de acces TASTABILĂ și unică, fiecare rând are pictogramă),
  comutatorul `ShowThemeEditor` (scoate ultimul rând ȘI separatorul), aprinderea pe butonul corect
  și **zero rânduri de designer** pentru toate cele cinci proprietăți noi — verificate prin
  `TypeDescriptor`, calea pe care merge chiar Visual Studio.

## Nevăzut / amânat

- **Nimic din felia asta n-a fost văzut pe ecran.** Butonul, meniul, pictograma implicită și
  aprinderea sunt probate headless (`DrawToBitmap`, numărare de pixeli). Prima probă de operator
  trebuie să se uite la: poziția butonului față de minimizare, mărimea glifei la umplutura 2, și
  dacă meniul iese aliniat sub buton.
- **`KBot.App.Tests` are 10 probe roșii, TOATE dinainte** (`DdfViewTests` 7, `IstoricViewTests` 2,
  `MainFormNavItemsTests.Designer_WroteLiteralDiacritics` 1). Ultima e verificabilă direct:
  `git show HEAD:src/KBot.App/MainForm.Designer.vb` scrie deja `"Fundamentare"`, iar proba cere
  `"Doc. Fundamentare"`. Niciuna nu atinge bara de titlu sau tema.
- **`LoginForm` a rămas neatins**, deși acolo e chiar butonul mort descris la punctul 1: are
  `ShowOptionsButton = True` cu pictograma de temă și niciun handler. Un rând
  (`ShowThemeButton = True` în locul celor trei de opțiuni) îl face să meargă — dar `LoginForm` are
  lucru necomis în arbore, deci decizia rămâne a operatorului.
- Butonul de opțiuni NU a fost scos din control: rămâne liber pentru alt meniu, cu contractul lui
  neschimbat (`OptionButtonClick` + `OptionButtonBounds`).
