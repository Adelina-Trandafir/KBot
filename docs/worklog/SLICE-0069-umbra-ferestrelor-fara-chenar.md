# SLICE 0069 — Umbra ferestrelor fără chenar (cu chenar de rezervă)

**Data:** 19.09.2026
**Cererea operatorului:** «on windows 10 (and maybe on 11 - i didn't test) there is no shadow
under the forms so when a new form is opened over another they look bad. either implement a
shadow or, if no shadow detected, and one can't be implemented, then show a border around the
form. this should be done from the KbotThemedForm»

## Ce s-a găsit

Toate formularele K-BOT sunt `FormBorderStyle.None` (21 de `.Designer.vb`). Windows desenează
umbră doar ferestrelor cu ramă; o fereastră fără ramă n-are nici umbră, nici margine. Pe
Windows 11 `ThemeManager.Apply` cere colțuri rotunjite (`DWMWA_WINDOW_CORNER_PREFERENCE`), iar
DWM aduce umbra odată cu ele; pe Windows 10 atributul nu există (eșec logat o dată, documentat
în `NativeMethods.SetRoundedCorners`), deci fereastra rămânea pătrată ȘI fără umbră — exact ce a
văzut operatorul.

## Ce s-a schimbat și de ce

- **`KBot.Theming/Interop/NativeMethods.vb`** — `TryEnableBorderlessShadow(f) As Boolean`:
  `DwmIsCompositionEnabled`, apoi `DwmSetWindowAttribute(DWMWA_NCRENDERING_POLICY = 2,
  DWMNCRP_ENABLED)`, apoi `DwmExtendFrameIntoClientArea` cu margini de 1 px pe toate laturile —
  rețeta documentată prin care o fereastră `WS_POPUP` primește umbra ramei fără să primească
  rama (pixelul întins e acoperit de zona client). Întoarce **True numai** când compoziția e
  pornită și ambele apeluri au răspuns `S_OK`; orice altceva = False, logat o dată, fără throw
  (o umbră lipsă nu are voie să oprească deschiderea unui formular).
- **`KBot.Theming/KBotThemedForm.vb`** — totul stă în bază, cum a cerut operatorul:
  - proprietatea de designer `BorderlessShadow` (categoria K-BOT, `DefaultValue(True)`, deci zero
    linii serializate pe un formular proaspăt); `False` = formularul rămâne cum îl face WinForms
    (pentru meniurile cu umbra lor `CS_DROPSHADOW`). Formularele cu ramă ignoră comutatorul.
  - `OnHandleCreated` (nou; `KBotShellForm` îl suprascria deja și cheamă `MyBase`) cere umbra la
    FIECARE handle — schimbarea `FormBorderStyle`/`ShowInTaskbar` recreează fereastra și DWM
    uită tot.
  - `ApplyBorderlessDecoration(wanted, shadowAvailable)` (Friend, cusătura de test) așază
    rezultatul: umbră cerută dar nedată → **chenar de rezervă**: `Padding` crește cu 1 px pe
    fiecare latură (copiii andocați se opresc cu un pixel înaintea marginii; `ThemeFormFit`
    numără padding-ul, deci formularul crește cu 2 px în loc să-și strângă conținutul) și
    `OnPaint` desenează dreptunghiul de 1 px în `Palette.Border`. Ieșirea din rezervă dă
    pixelul înapoi. Idempotent.
  - `BorderlessShadowShown` / `FallbackBorderShown` (Browsable(False)) spun ce s-a întâmplat pe
    handle-ul curent; pe un formular fără chenar cu comutatorul pornit exact una e True.
- **`KBot.Controls/DataView/Filter/KBotFilterPopup.vb`** — `BorderlessShadow = False` în
  constructor: e meniu, are deja `CS_DROPSHADOW`; umbra de fereastră ar fi dublat-o.
- **`tests/KBot.Theming.Tests/BorderlessShadowTests.vb`** (nou, 7 teste): exact un rezultat pe
  handle real, comutatorul oprit / rama lasă formularul neatins, rezerva adaugă și dă înapoi
  pixelul (și peste un `Padding` autorat), idempotență, `ShouldSerializeValue` False implicit.

## Rezultate

- `dotnet build` pe `src\KBot.Controls`, `src\KBot.App`, `src\KBot.DevHarness`, `src\KBot.Forexe`,
  `src\KBot.Migrator`: **0 erori, 0 avertismente** (în afara `MSB3825` preexistent pe App).
- **Verificat pe ecran (Windows 11, DPI 96):** o sondă de unică folosință a arătat două
  `KBotThemedForm` fără chenar una peste alta și a fotografiat ecranul (`CopyFromScreen`, fiindcă
  `DrawToBitmap` nu vede umbra). Calea reală: `BorderlessShadowShown = True`, umbră vizibilă sub
  fereastra de sus, fără linie parazită pe marginea de 1 px întinsă. Rezerva (forțată prin
  reflexie pe aceeași sondă): `FallbackBorderShown = True`, `Padding = 1`, chenarul `#B4B4B4`
  vizibil pe toată circumferința, conținutul mutat cu 1 px.
- **Testele NU au fost rulate și NU au fost construite** (regula operatorului).

## Neverificat / amânat

- **Windows 10 nu a fost atins** — mașina de lucru e Windows 11, unde umbra vine oricum odată cu
  colțurile rotunjite. Rețeta `NCRENDERING_POLICY + DwmExtendFrameIntoClientArea(1,1,1,1)` e
  cea folosită de MetroFramework («AeroShadow») și de răspunsul clasic pentru formulare
  borderless pe Windows 7–10; de confirmat pe o mașină cu Windows 10 la prima instalare. Dacă pe
  Windows 10 apare o linie subțire deschisă pe marginea de 1 px (zona întinsă amestecă aditiv
  culoarea clientului cu rama DWM), e de așteptat mai ales pe schema Dark — se rezolvă vopsind
  acel pixel în negru în `OnPaint`, nescris fiindcă nu s-a văzut.
- Chenarul de rezervă e de 1 px DISPOZITIV (hairline), nescalat la DPI, intenționat.
- Calea în care DWM refuză de-adevăratelea (compoziție oprită) nu există pe Windows 8+; a fost
  exercitată doar prin cusătură.
