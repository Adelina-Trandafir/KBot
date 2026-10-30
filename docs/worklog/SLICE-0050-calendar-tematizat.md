# SLICE-0050 — calendarul tematizat (`KBotCalendar`) și câmpul de dată care se poate întinde (`KBotDatePicker`)

Familie nouă de control în `KBot.Controls`: `Calendar/`. Cerută de operator în trei propoziții — un control de calendar care ascultă de sistemul de teme, un «textbox» care arată ca el dar care **se poate întinde pe verticală**, și un calendar propriu-zis (zile / luni / ani) tot tematizat.

**Stare:** cod verde. `dotnet build src/KBot.Controls/KBot.Controls.vbproj` → **0 erori / 0 avertismente**. `dotnet build KBot.sln` → **0 erori / 6 avertismente**, toate cele 6 fiind `MSB3825` PREEXISTENTE pe `.resx`-urile din `KBot.App` (`image_list.ImageStream` prin `BinaryFormatter`) — niciunul nou, niciunul din felia asta.

**NIMIC NU S-A VĂZUT PE ECRAN.** Cele două controale n-au fost deschise nici în designerul Visual Studio, nici randate cu `DrawToBitmap`, nici puse pe vreun formular. Nicio schemă n-a fost probată pe ele. Verificarea vizuală rămâne a operatorului.

**Fără teste.** Nu s-a scris niciun test și nu s-a rulat nicio suită — cerut explicit.

**Fără git.** Nimic nu s-a comis și nimic nu s-a împins — cerut explicit. Worklogul, actualizarea STATUS și codul rămân în arborele de lucru.

---

## 0. Ce s-a citit înainte de orice

`docs/worklog/CODE_WORKFLOW.md` · `docs/worklog/KBOT_STATUS.md` (registrul de felii + linia «Next free») · `CLAUDE.md` · `src/KBot.Controls/CONTROLS.md` (convențiile C1..C9) · `src/KBot.Controls/README.md` · `src/KBot.Controls/KBot.Controls.vbproj` · `src/KBot.Controls/Combo/KBotComboBox.vb` + `.md` (tiparul feței desenate + steagurile de fixare) · `src/KBot.Controls/TextField/KBotTextField.vb` + `.md` (tiparul cadrului cu `TextBox` intern) · `src/KBot.Controls/Popup/CustomPopup.vb` (mecanica ferestrei: `CreateParams`, `ShowBelow`, `FitToWorkArea`, `OnDeactivate`, `ClosedJustNow`) · `src/KBot.Controls/Chart/KBotChartView.vb` (controlul cel mai nou, scris integral în engleză — tiparul de stil urmat aici) · `src/KBot.Theming/{ThemePalette,ThemeShapes,ThemeScheme,ThemeStyleOptions,IThemedControl, KBotDesignTime,AppScaling}.vb`.

---

## 1. Ce s-a scris

Șase fișiere noi în `src/KBot.Controls/Calendar/` (folder nou = familie nouă, regula din README):

| Fișier | Ce e |
|---|---|
| `KBotCalendarView.vb` | enum-ul celor trei niveluri: `Days` / `Months` / `Years` |
| `KBotDateSelectedEventArgs.vb` | ziua ALEASĂ (nu simpla plimbare cu săgețile) |
| `KBotCalendar.vb` | suprafața: stare, proprietăți, temă, mouse, tastatură |
| `KBotCalendar.Painting.vb` | partial-ul de geometrie și pictură (același tipar ca `Tree`/`Chart`) |
| `KBotCalendarPopup.vb` | fereastra pe care o desfășoară câmpul |
| `KBotDatePicker.vb` | câmpul de dată — «textbox»-ul cerut |

Plus documentația de familie: `Calendar/KBotCalendar.md` și `Calendar/KBotDatePicker.md`, în tiparul celorlalte (ce POATE controlul și unde se oprește, doar excepțiile de la C1..C9).

## 2. De ce nu s-au folosit `MonthCalendar` și `DateTimePicker`

MonthCalendar/DateTimePicker sunt pictate nativ de Windows, ca vechile ComboBox/meniuri înlocuite de KBotComboBox/CustomPopup: BackColor/ForeColor controlează doar parțial aspectul; antetul sistemului lasă suprafețe albe în tema întunecată.

Peste asta, fiecare are un defect propriu care contează aici:

- `MonthCalendar` **refuză mărimile**: se aliniază la dale întregi de lună, deci nu se poate nici andoca, nici întinde în spațiul primit.
- `DateTimePicker.SetBoundsCore` readuce `Height` la înălțimea combo-ului de sistem. `KBotDatePicker`, `Control` obișnuit, nu atinge `SetBoundsCore`: înălțime liberă, `DefaultSize` numai inițial, contur întins, text centrat vertical și buton care crește cu câmpul.

## 3. Deciziile care nu se citesc din cod

1. **ValueChanged** la orice mișcare, inclusiv săgeți; **DateSelected** numai la clic pe zi/Enter/Astăzi. Popupul se închide la DateSelected, evitând închiderea la prima săgeată.
2. **`CultureName = ro-RO` implicit**, pentru luni/zile românești pe orice stație, independent de cultura mașinii. Proprietatea rămâne configurabilă. Numai textele ecranului («Astăzi: …») și excepțiile sunt românești; restul familiei este ASCII engleză (C9, RULE 0).
3. **Culoarea de weekend se DERIVĂ din paletă**, nu se inventează: `Blend(ErrorColor, InputBack, 0.25)`. O coloană întreagă în roșul de eroare s-ar citi ca o coloană de erori.
4. **Poziționare reutilizată:** `KBotCalendarPopup` apelează `CustomPopup.FitToWorkArea` (`Friend Shared`, același assembly), cu aceeași întoarcere deasupra/aliniere dreapta ca meniul. Garda `ClosedJustNow` de 250 ms împiedică redeschiderea la al doilea clic care tocmai închisese fereastra.
5. **Fereastra se activează**, fără `WS_EX_NOACTIVATE`, pentru focus și navigare cu săgeți. Bara gazdei apare inactivă cât calendarul este deschis.
6. **Text invalid → ultima valoare bună**, fără salvarea unei date parțiale. Ordinea citirii: `Format`, apoi `2.9.26`/`02092026`/`2/9/2026`/`2026-09-02`, apoi cultura.
7. **Câmpul gol există doar cu `AllowEmpty`** — coloana de dată neumplută din Access. `ClearValue()` fără permisiunea asta ARUNCĂ `InvalidOperationException`, nu tace (C3).
8. **DefaultSize suprascris, fără Size în constructor.** Control.ShouldSerializeSize compară cu DefaultSize; atribuirea Size ar serializa o linie în fiecare gazdă nouă (C4).

## 4. Convențiile casei, punct cu punct

- **C1** — fiecare culoare are `Color.Empty` = «din temă» + perechea `Effective*`.
- **C2** — toate măsurile publice sunt px logici @96dpi, scalate la pictare prin `ThemeShapes.ScaleDpi` (care sare singur peste scalare sub `KBotDesignTime`).
- **C3** — cultură necunoscută, format gol, `MinDate > MaxDate`, nivel de calendar necunoscut → `ArgumentException`; `ClearValue()` fără `AllowEmpty` → `InvalidOperationException`; valorile numerice și datele în afara intervalului se LIMITEAZĂ.
- **C4** — `ShouldSerialize*`/`Reset*` pe fiecare culoare proprie ȘI pe `BackColor`/`ForeColor`/ `Font` moștenite, răspunse din steagul «operatorul a fixat asta», nu din punga de proprietăți a lui `Control`. `Value` nu se serializează (e stare, nu autorare).
- **C5** — amândouă controalele implementează `IThemedControl`; `KBotDatePicker` E OBLIGAT s-o facă, fiindcă ține un `TextBox` copil.
- **C7** — fiecare frontieră de UI (`OnPaint`, mouse, tastatură, temă) loghează în `GlobalErrorLog` și înghite; `ShowBelow`/`ShowDropDown` (creare de fereastră) loghează și RE-ARUNCĂ.

## 5. Fișiere atinse

**Noi:** cele șase fișiere și cele două documente din familia `src/KBot.Controls/Calendar/` enumerate în §1, plus acest jurnal (`docs/worklog/SLICE-0050-calendar-tematizat.md`).

**Modificate:**
`src/KBot.Controls/CONTROLS.md` — două rânduri noi în index; `src/KBot.Controls/README.md` — rândul `Calendar/` în tabelul de foldere; `src/KBot.Controls/KBot.Controls.vbproj` — familia nouă în comentariul de organizare + `FileVersion` 1.39.0.0 → **1.40.0.0** (regula: se urcă doar când se schimbă chiar proiectul ăsta; `AssemblyVersion` rămâne 1.0.0.0); `docs/worklog/KBOT_STATUS.md` — rândul feliei 0050 + «Next free» mutat pe 0051

**Neatins:** `KBot.App`, `KBot.Theming`, `PYTHON/`, orice formular existent. `OrdEditForm` și `OrdZiuaForm` folosesc în continuare `DateTimePicker`-ul de sistem — înlocuirea lor n-a fost cerută în felia asta și nu s-a făcut.

## 6. Rezultatele probelor

- `dotnet build src/KBot.Controls/KBot.Controls.vbproj` → **Build succeeded, 0 Warning(s), 0 Error(s)**.
- `dotnet build KBot.sln` → **Build succeeded, 6 Warning(s), 0 Error(s)** — cele 6 sunt `MSB3825` preexistente pe `.resx`-urile din `KBot.App`.
- **Nicio suită de teste nu s-a rulat** (cerut explicit). Nu s-a scris niciun test nou.

## 7. Ce rămâne neverificat / amânat

1. **Aspectul pe ecran, pe toate cele trei scheme.** Nimic n-a fost randat. Culorile, geometria și comportamentul la DPI sunt argumentate din cod, nu dintr-o captură.
2. **Comportamentul în designerul Visual Studio.** Regula «un control proaspăt pus pe formular scrie ZERO linii de proprietăți» e respectată prin construcție (steaguri de fixare + `DefaultSize`), dar **nu a fost verificată** cu `TypeDescriptor.GetProperties(c)(nume).ShouldSerializeValue(c)`.
3. **Teste.** Zero teste în `KBot.Controls.Tests`. Candidații evidenți, toți funcții pure sau aproape: `KBotDatePicker.TryParseDate` (`Friend Shared`, deja scrisă ca funcție pură tocmai pentru asta), `CellRect`, `FirstVisibleDay`, construirea celor trei pagini de celule, limitarea la `MinDate`/`MaxDate`.
4. **Bancul de probă.** Nu s-a adăugat nicio probă în `KBot.DevHarness`.
5. **Niciun formular nu folosește încă familia asta.** Trecerea celor două `DateTimePicker`-uri din `Views/Ord/` pe `KBotDatePicker` e o felie separată, necerută aici.
6. **`KBotToolTip`** nu e legat pe niciuna dintre zonele desenate (săgeți, titlu, rândul «Astăzi»). C8 spune cum s-ar face când se va cere.

---

# 0050 — rundă corectivă (cerută de operator după prima montare pe `OrdEditForm`)

Trei lucruri cerute, după ce câmpul a intrat în `Views/Ord/OrdEditForm`: **(1)** aerul (paddingurile) să fie editabil din designer, tot; **(2)** în tema întunecată desenul de calendar de pe buton era negru pe negru; **(3)** formatul să permită valori cu ORĂ, nu doar zi.

## 8. Aerul, expus pe toate fețele

Regula de bază: fiecare padding e un `Padding` întreg (stânga/sus/dreapta/jos), nu un singur număr, e px logici la 96 dpi (C2), negativul se limitează la 0 (C3, se limitează, nu se aruncă), și fiecare are perechea `ShouldSerialize*`/`Reset*` (C4).

`KBotDatePicker` — patru reglaje:

| Proprietate | Implicit | Ce mișcă |
|---|---|---|
| `Padding` (moștenit, la *Layout*) | `0` | tot ce e în interiorul conturului: și textul, și butonul |
| `TextPadding` | `8,0,8,0` | textul; sus/jos strâng fâșia în care linia de text stă centrată — așa se ridică sau se coboară textul într-un câmp înalt |
| `ButtonPadding` | `6` | desenul de calendar în interiorul benzii-buton |
| `GlyphSize` | `14` (`0` = umple) | cât de mare e desenul; `0` îl lasă să crească odată cu câmpul |

`KBotCalendar` — cinci, imbricate dinspre exterior:

| Proprietate | Implicit | Ce mișcă |
|---|---|---|
| `Padding` (moștenit) | `0` | tot: antet, numele zilelor, grila, rândul «Astăzi» |
| `HeaderPadding` | `0` | săgețile și titlul DIN antet; banda pictată rămâne pe toată lățimea |
| `GridPadding` | `0` | fâșia cu numele zilelor ȘI celulele, împreună |
| `CellPadding` | `2` | distanța dintre celulă și dala ei colorată (selecție, hover, inelul zilei de azi) |
| `FooterPadding` | `0` | textul din rândul «Astăzi», nu banda și nici umplerea de hover |

Două decizii care nu se citesc din cod:

- **`GridPadding` se ia O SINGURĂ DATĂ**, înainte de desprinderea fâșiei cu numele zilelor. Dacă s-ar lua separat pe fiecare, capetele de coloană și coloanele de dedesubt s-ar tăia din lățimi diferite și s-ar decala între ele.
- **Padding rămâne moștenit**, la Layout: Shadows pentru recategorizare ar rupe ShouldSerializePadding și ar serializa implicitul neales în gazdă (C4). EnsureLayout/OnPaddingChanged îl respectă. CellPadding=2 reproduce retragerea fixă anterioară, fără schimbarea aspectului implicit.

`NaturalSize` numără acum aerul exterior și `GridPadding`: altfel fereastra desfășurată ar fi ieșit strâmtată exact cu cât s-a cerut să rămână gol.

## 9. Desenul de pe buton, negru pe negru

**Cauza reală, găsită în `OrdEditForm.Designer.vb`, nu în control:** două culori fixate din grila de proprietăți —

```
dtpData.GlyphColor = SystemColors.ActiveCaptionText   ' negru
dtpData.HoverColor = SystemColors.ButtonFace
```

Culorile explicite prevalează la fiecare schemă (C1). ActiveCaptionText negru pe input întunecat #1C1C1C ascundea glifa. Ștergerea celor două linii restabilește tema.

EffectiveGlyphColor se derivă din ForeColor tras o treime spre BackColor, fără slot separat ales pentru alt fundal. Exemplu întunecat: #D2D2D2 peste #1C1C1C la 0,3 → #9B9B9B. Vizibilitate argumentată aritmetic, fără captură.

`GlyphColor` fixat de operator câștigă în continuare.

## 10. Valori cu oră

`Value` era tăiat la zi în `SetValueCore` (`candidate.Date`). Nu mai e: e un `Date` întreg.

- `Format` primește parte de oră — `dd.MM.yyyy HH:mm`, `dd.MM.yyyy HH:mm:ss` — și câmpul devine câmp de dată-oră: ora se scrie, se tastează și se citește înapoi.
- Lista de scurtături are acum și tipare cu oră, puse ÎNAINTEA celor fără: un tipar doar-dată refuză oricum un text cu oră în el.
- `MinDate`/`MaxDate` nu mai taie ora; implicitul de sus e **ultima clipă** a zilei (`31.12.2100 23:59:59`), nu miezul nopții — altfel s-ar refuza fiecare oră a ultimei zile.
- **Păstrează partea neafișată:** format fără oră → zi aleasă/retastată păstrează ora veche; format cu oră → valoarea tastată, inclusiv miezul nopții. FormatHasTime(format), pură, ignoră literalii ghilimelați și escape-ul \, astfel dd.MM.yyyy 'ora' nu indică timp.
- Calendarul din fereastra desfășurată **rămâne o suprafață de ZI** — n-are ceas. Dă înapoi o zi, iar câmpul îi pune ora la loc.

Adăugat și `DropDownCalendar` (read-only, ne-serializat): calendarul viu din fereastra deschisă, pentru gazda care vrea să-i regleze aerul sau culorile din `DropDownOpened`.

## 11. Fișiere atinse în runda asta

`src/KBot.Controls/Calendar/KBotDatePicker.vb` — paddinguri, `GlyphSize`, culoarea derivată a desenului, valoarea cu oră, `FormatHasTime`, `DropDownCalendar`; `src/KBot.Controls/Calendar/KBotCalendar.vb` — cele patru paddinguri proprii + `OnPaddingChanged`; `src/KBot.Controls/Calendar/KBotCalendar.Painting.vb` — `ScalePad`/`Shrink`, așezarea pe benzi, `NaturalSize`, dala celulei, textul din subsol; `src/KBot.App/Views/Ord/OrdEditForm.Designer.vb` — **șterse** `dtpData.GlyphColor` și `dtpData.HoverColor`; `src/KBot.Controls/Calendar/KBotCalendar.md`, `KBotDatePicker.md` — secțiunile noi; `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.40.0.0 → **1.41.0.0**

## 12. Probe

- `dotnet build src/KBot.Controls/KBot.Controls.vbproj` → **0 erori / 0 avertismente**.
- `dotnet build KBot.sln` → **0 erori / 6 avertismente**, cele 6 `MSB3825` preexistente.
- **Zero teste rulate, zero teste scrise** (cerut explicit). **Fără git.**
- **NIMIC NU S-A VĂZUT PE ECRAN** nici de data asta. Vizibilitatea desenului e argumentată prin aritmetica de amestec de mai sus, nu prin randare. `FormatHasTime` și regula de păstrare a orei sunt scrise anume ca funcții pure, ca să poată fi probate fără ecran când se vor scrie teste.
