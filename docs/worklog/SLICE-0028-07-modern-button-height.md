# SLICE-0028-07 — Schema «Modern»: butonul crește cât să încapă și umplutura, și textul

Raport de operator, după ce a autorat singur meniul de filtrare în designer (felia 0028-06):

> i adjusted the design and it looks ok, but on modern theme, it adds a lot of padding to the
> buttons, so the text is not visible anymore. when the modern theme is applied, it should adjust
> the height of the buttons to fit the new padding AND the text

---

## 1. Ce se întâmpla

`BuiltInSchemes.Modern()` cere `ControlPadding = (12, 8, 12, 8)`, iar `ModernRenderer.ApplyButton`
o scrie în `Button.Padding`. Umplutura NU e desenată în afara butonului — ea micșorează suprafața
în care încape textul. Pe un buton autorat la 32px rămâneau `32 − 8 − 8 = 16px` pentru un rând de
text care cere ~15–20px: textul se tăia sau dispărea de tot.

Al doilea capăt al aceleiași greșeli, invizibil până acum: **umplutura nu se dădea niciodată
înapoi**. `DetachButton` (chemat la ieșirea din schema modernă, din amândouă ramurile lui
`ThemeManager` — `StylePalette` și `StyleSystem`) scotea handler-ele și regiunea rotunjită, dar lăsa
`Padding = (12,8,12,8)` pe buton. Adică schema modernă rescria PERMANENT designul: după o singură
trecere prin ea, butonul rămânea altfel și în Classic.

## 2. Regula pusă în loc

În `ModernRenderer`, aceeași formă ca lățimea autorată a unei coloane (felia 0028-05): **starea
autorată se reține înainte de prima scriere a temei, iar tema calculează peste ea, nu în locul ei.**

`ButtonState` (deja exista, pentru handler-e) ține acum și `AuthoredPadding` + `AuthoredHeight`,
luate o singură dată, înaintea oricărei scrieri. Apoi:

- **`ApplyButton`** — după ce pune umplutura schemei, cheamă `FitHeightToPaddingAndText`:

  ```
  nevoie   = Padding.Vertical + înălțimea unui rând de text (fontul butonului) + cele două chenare
  înălțime = Math.Max(înălțimea AUTORATĂ, nevoie)
  ```

  Se ia MAXIMUL, deci **o înălțime autorată mai mare rămâne neatinsă**: tema completează ce lipsește,
  nu rescrie ce a ales operatorul. Și, fiindcă baza e mereu valoarea autorată, comutarea temei de
  zece ori dă același rezultat ca prima — trecerea e o funcție de (autorat, schemă), nu de istoric.

- **`DetachButton`** — dă înapoi și marginea, și înălțimea autorate.

Două cazuri sunt sărite dinadins, fiindcă acolo înălțimea nu e a butonului:

- `AutoSize = True` — se ocupă WinForms;
- `Dock = Left / Right / Fill` — înălțimea e a PĂRINTELUI, iar o scriere aici ar fi ștearsă de
  următorul layout (în meniul de filtrare, exact cazul lui OK / Anulează, andocate în bara lor).

Un buton fără text primește totuși loc de un rând (se măsoară „Wg”): o pictogramă pusă mai târziu
n-are cum să încapă într-un buton cât umplutura.

## 3. Fereastra care le ține

Butoanele de comandă ale meniului sunt andocate SUS, deci creșterea lor mușcă din lista de valori,
care e `Fill` — meniul ar fi arătat, după comutare, mai puține valori decât înainte.
`KBotFilterPopup.OnThemeChanged` face acum `PerformLayout()` + re-măsurarea înălțimii ferestrei, deci
fereastra crește cu exact cât au crescut butoanele și numărul de rânduri arătate rămâne cel dinainte.

## Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Theming/ModernRenderer.vb` | `AuthoredPadding`/`AuthoredHeight` în `ButtonState`, `FitHeightToPaddingAndText`, restaurarea din `DetachButton` |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | `OnThemeChanged` re-măsoară fereastra după ce butoanele au crescut |
| `tests/KBot.Theming.Tests/ModernButtonHeightTests.vb` | **nou**, 6 teste |
| `tests/KBot.Controls.Tests/KBotFilterPopupDesignerTests.vb` | +1 test (fereastra crește odată cu butonul) |

**Versiuni**: `KBot.Theming` 1.6.0.0 → **1.7.0.0**; `KBot.Controls` 1.17.0.0 → **1.18.0.0**.

## Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Theming.Tests       →  71 /  71   (65 înainte, +6)
KBot.Controls.Tests      → 709 / 709   (708 înainte, +1)
KBot.DevHarness.Tests    → 170 / 170
```

Cele 6 teste noi: butonul scund crește cât umplutura + textul, cel destul de înalt e lăsat în pace,
ieșirea din schemă dă înapoi marginea ȘI înălțimea, re-aplicarea nu compune creșterea, un buton
andocat lateral e lăsat părintelui, iar unul fără text tot primește loc de un rând.

## Rămase neverificate / amânate

- **Nevăzut pe ecran**, ca toată felia 0028. Numerele vin din măsurători headless
  (`TextRenderer.MeasureText`), care sunt cele folosite și de pictare — dar verdictul «se citește
  bine» se dă privind.
- **Alte forme din aplicație vor arăta altfel pe Modern.** Regula e în MOTOR, nu în meniu, deci
  orice buton scund cu `Dock = Top/Bottom/None` va crește acum sub schema modernă — ceea ce e
  reparația cerută, dar e o schimbare vizibilă peste tot (LoginForm, MainForm, vederi). Nimic nu se
  micșorează, deci nimic nu se poate ascunde din cauza asta.
- **Umplutura orizontală n-a fost atinsă.** Cei 12px stânga/dreapta pot încă trunchia un text lung
  pe un buton îngust; lățimea nu s-a ajustat, fiindcă acolo layout-ul (andocare, ancore) o
  stăpânește mult mai des decât înălțimea, iar o creștere automată de lățime ar strica aranjamente
  întregi. Dacă apare cazul, se rezolvă cu `AutoSize` sau cu o lățime autorată mai mare.
