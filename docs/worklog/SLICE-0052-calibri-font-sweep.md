# SLICE-0052 — Calibri peste tot + comutatorul «Font din temă»

**Stare:** ÎN LUCRU. Pașii 2, 3 și 5 sunt gata; pasul 4 (măturarea lui
`AutoScaleDimensions`) și pasul 6 (versiunile) sunt OPRITE, în așteptarea
verificării în Visual Studio pe `OrdEditForm` — vezi §2.4.

**Fără git** (operatorul se ocupă). **Fără teste noi** (cerut explicit); patru
teste EXISTENTE au fost corectate, fiindcă fixau exact valorile pe care felia le
schimbă deliberat — vezi §5.

---

## 0. Ce s-a cerut, și ce s-a decis

Plângerea operatorului: «de ce sistemul de teme face formularele mai mari sau mai
mici? proiectez ceva și apoi îl văd altfel la rulare».

Deciziile lui, luate în timpul feliei:

1. Toate cele patru scheme trec pe Calibri, nu doar Modern.
2. Mărimea de bază 9pt. Bold / Italic / 10pt / 12pt puse pe controale rămân cum
   au fost autorite.
3. Calibri lipsă ⇒ mesaj în română, apoi cădere pe fontul de sistem și mai
   departe.
4. Valorile fontului se scriu pe toate patru schemele, cu un comentariu care
   spune pe față care două le citesc. `StyleSystem` **nu** primește o scriere de
   font: definiția schemei Classic e că nu pictează nimic.
5. `KBotThemedUserControl` stă în `KBot.Theming`, lângă celelalte două tipuri de
   bază; `CLAUDE.md` s-a lărgit la «base forms and base user controls».
6. `KBot.Migrator` e în felie (moștenește `KBotThemedForm`, deci ia Calibri
   oricum — nemăturat ar fi rămas singurul formular greșit).
7. `ShowTextScaleSlider` **rămâne** proprietate a operatorului; rândul se
   ascunde pe `_showTextScaleSlider AndAlso ThemeManager.WritesFormFont`. Prima
   formă a planului — «se derivă» — era greșită: sunt nouă apelanți, dintre care
   șapte `.Designer.vb` și un test de serializare.
8. `FrmNodeDebug` și `WicketMonitorForm` NU sunt în felie.
   `CertificateSelectionForm`, `HistoryForm` și `ForexeConsoleForm` sunt.

---

## 1. Defectul, măsurat — nu dedus

`ThemeManager.ApplyBaseFont` scrie fontul schemei pe formular. Îl cheamă doar
`StylePalette`, doar pe ramura `TypeOf ctrl Is Form`. Până la felia asta doar
**Modern** avea `BaseFontSize <> 0`, și cerea `Segoe UI Variable Text` 9.

Între timp, niciun `.Designer.vb` nu scria fontul formularului (o singură
excepție, `FrmNodeDebug.Designer.vb:127`), deci suprafața de proiectare din
Visual Studio măsura cu fontul implicit WinForms, `Segoe UI` 9.

Toate formularele sunt `AutoScaleMode.Font`. Asta înseamnă că WinForms compară
fontul de pe formular cu perechea `AutoScaleDimensions` scrisă în designer și
înmulțește CU RAPORTUL fiecare dreptunghi de copil și `ClientSize`-ul
formularului. Fontul nu e o podoabă: e o jumătate dintr-o măsurătoare, cealaltă
jumătate fiind fișierul de designer.

Cele două jumătăți nu se potriveau. Măsurat pe ecranul operatorului (150%):

| Font 9pt | se măsoară |
|---|---|
| `Segoe UI` (cel din designer) | **(10, 25)** |
| `Segoe UI Variable Text` (cel scris de Modern) | **(10, 24)** |

**Fiecare fereastră a aplicației se turtea pe verticală cu 4% la deschidere**,
lățimea neatinsă, fără ca nimic din designer s-o arate. De aceea se citea ca
«ușor greșit peste tot» și nu ca o stricăciune evidentă.

Peste asta, `Segoe UI Variable Text` e un font de Windows 11: pe Windows 10 GDI
cade tăcut pe altceva, deci abaterea era ALTA pe alte mașini, fără nimic în
jurnal.

**Nu era defect, contrar primei impresii:** cele trei valori diferite de
`AutoScaleDimensions` din soluție. Fiecare e corectă pentru spațiul în care i-au
fost scrise coordonatele — vezi §2.3.

**Sprijin pe care planul nu-l avea:** Calibri era DEJA fontul dominant în
designere, la nivel de control — **242** `New Font("Calibri"…)` față de 28
`"Segoe UI"`, 11 `"Consolas"` și 8 `"Segoe UI Semibold"`. Fontul ambiental era
singurul care nu se potrivea cu restul. Felia nu introduce Calibri, ci îl aduce
și pe cel ambiental acolo unde era deja tot restul.

---

## 2. Măsurătoarea

### 2.1 Cum s-a măsurat

Două drumuri independente, tocmai ca să nu depindă răspunsul de amintirea cuiva
despre ce face WinForms înăuntru:

* **(A)** `ContainerControl.CurrentAutoScaleDimensions` pe un `Form` adevărat cu
  `AutoScaleMode.Font` — chiar proprietatea pe care o citește serializatorul
  designerului.
* **(B)** o reimplementare GDI (`CreateCompatibleDC` + `GetTextMetricsW`),
  parametrizată după DPI, ca să poată da numere pentru 96 / 120 / 144 fără să se
  mute cineva pe alt monitor.

**O capcană, meritată de reținut:** `CurrentAutoScaleDimensions` se calculează și
se REȚINE în clipa în care se atribuie `AutoScaleMode`, iar o schimbare
ulterioară de font NU o invalidează. Prima trecere a măsurat fontul implicit de
trei ori și a dat `(10, 25)` pentru trei fonturi diferite. **Fontul întâi, modul
după.**

Pentru numerele de la 96 dpi, procesul a fost pornit cu
`HighDpiMode.DpiUnaware`, ceea ce îl face să creadă că e la 96 dpi și dă valori
de 100% adevărate pe un ecran la 150%. Verificarea e încorporată: `Segoe UI` 9
TREBUIE să iasă `(7F, 15F)`, valoarea deja aflată în unsprezece fișiere. A ieșit.

> **NU se deschide un fișier de 96 dpi în Visual Studio pe un ecran la 150% ca
> să se «verifice».** `AutoScaleDimensions` se ștampilează după DPI-ul suprafeței
> de proiectare, nu după ce scria fișierul: VS ar rescala coordonatele copiilor
> în spațiul de 150% și ar ștampila corespunzător. Ai primi `(9F, 22F)`, iar
> coordonatele fișierului ar fi fost rescrise tăcut. Exact așa au devenit cele 40
> de fișiere `(10, 25)`.

### 2.2 Rezultatul

Cele două drumuri sunt de acord, la 144 dpi și la 96 dpi:

| Font 9pt | @96 dpi (100%) | @144 dpi (150%) |
|---|---|---|
| `Segoe UI` | `(7F, 15F)` | `(10F, 25F)` |
| **`Calibri`** | **`(6F, 14F)`** | **`(9F, 22F)`** |
| `Segoe UI Variable Text` | `(7F, 16F)` | `(10F, 24F)` |

`Calibri` s-a rezolvat la `"Calibri"` pe mașina operatorului (nu substituit).

**Reimplementarea GDI e greșită la lățime** și nu se folosește pentru nimic din
ce contează: la 96 dpi dă 6 pentru `Segoe UI` acolo unde API-ul (și convenția
universală WinForms) dă 7. Cauza e care din două formule de lățime folosește
WinForms — `tmAveCharWidth`, sau media rotunjită a unui șir de 52 de litere.
Pentru Calibri cele două formule dau același număr la orice DPI, deci nu
schimbă nimic; dar numerele reținute mai sus sunt cele de la drumul (A), cel
autoritar.

### 2.3 Ce s-a aflat despre ștampilele EXISTENTE

Drumul (B), rulat pe mai multe DPI, explică fiecare valoare din soluție. Toate
sunt `Segoe UI` 9, măsurat la DPI-uri diferite:

| Ștampila de azi | Este | Fișiere |
|---|---|---|
| `(7F, 15F)` / `(7.0F, 15.0F)` | `Segoe UI` 9 @ **96 dpi (100%)** | 11 |
| `(8.0F, 20.0F)` | `Segoe UI` 9 @ **120 dpi (125%)** | 1 (`CertificateSelectionForm`) |
| `(10F, 25F)` / `(10.0F, 25.0F)` | `Segoe UI` 9 @ **144 dpi (150%)** | 40 |

Ștampila spune ÎN CE SPAȚIU sunt scrise coordonatele fișierului. Toate trei
grupurile sunt corecte azi, fiecare pentru spațiul lui.

### 2.4 Un defect în planul inițial, prins de măsurătoare

Planul cerea ca pasul 4 să înlocuiască FIECARE `AutoScaleDimensions` cu
`CALIBRI_9_AT_150`. Aplicat orbește, asta le-ar fi spus celor 11 fișiere scrise
la 96 dpi că sunt în spațiul de 150%: pe ecranul operatorului ar fi fost
desenate cu factorul 1 în loc de ~1,5, adică **micșorate la circa două treimi**.
Aceeași problemă, mai mică, pentru `CertificateSelectionForm` la 125%.

Corectarea aprobată — o hartă pe GRUP, după valoarea de acum:

* `(10, 25)` în toate cele patru scrieri → `(9F, 22F)`
* `(7, 15)` în ambele scrieri → `(6F, 14F)`
* `(8.0F, 20.0F)` → `(8F, 18F)`
* orice altceva → se RAPORTEAZĂ, nu se rescrie

### 2.5 Sonda

Aruncată după folosire (nu s-a lăsat nimic în `KBot.DevHarness` — dacă fontul de
bază se schimbă vreodată, e un copy-paste înapoi, nu merită un formular
permanent). Miezul, pentru cine trebuie s-o refacă:

```vb
' Font ÎNTÂI, mod DUPĂ — altfel se măsoară fontul implicit (vezi §2.1).
Using f As New Form()
    f.Font = New Font(face, 9.0F)
    f.AutoScaleMode = AutoScaleMode.Font
    Dim s As SizeF = f.CurrentAutoScaleDimensions
    Console.WriteLine($"{face} -> New SizeF({s.Width}F, {s.Height}F)  dpi={f.DeviceDpi}")
End Using
```

Pornit cu `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)` pentru numerele
de la 150%, și cu `HighDpiMode.DpiUnaware` pentru cele de la 96 dpi. Verificarea
că modul a funcționat: `Segoe UI` 9 iese `(7F, 15F)`.

Ieșirea adevărată, pe mașina operatorului:

```
Screen DC LOGPIXELSY .......... 144  (150%)
New Font("Calibri", 9) resolves to ... "Calibri" (present)

--- PerMonitorV2, 144 dpi ---
  Segoe UI                 -> New SizeF(10F, 25F)
  Calibri                  -> New SizeF(9F, 22F)
  Segoe UI Variable Text   -> New SizeF(10F, 24F)

--- DpiUnaware, 96 dpi ---
  Segoe UI                 -> New SizeF(7F, 15F)     <- verificarea încorporată
  Calibri                  -> New SizeF(6F, 14F)
  Segoe UI Variable Text   -> New SizeF(7F, 16F)
```

---

## 3. Ce s-a construit

### 3.1 `KBot.Theming/KBotFonts.vb` (nou)

Locul UNIC în care se decide fontul de bază. `BaseFontName = "Calibri"`,
`BaseFontSize = 9.0F`, plus `Base As Font` și `IsFallback As Boolean`.

Rezolvarea VERIFICĂ numele întors, nu doar că nu s-a aruncat: GDI substituie
tăcut un font lipsă și întoarce un obiect perfect valid cu numele altcuiva, deci
constructorul care reușește nu dovedește nimic. La substituire: se cade pe
`SystemFonts.DefaultFont`, se aprinde `IsFallback` și se scrie motivul în
`GlobalErrorLog`.

Fontul NU se eliberează niciodată, deliberat — copiii care moștenesc fontul
ambiental împart aceeași instanță cu formularul, deci un `Dispose` la momentul
greșit ar arunca dintr-un `OnPaint` (aceeași alegere ca în `FontBaseline`).

Mesajul către operator, arătat o singură dată la pornire din `Program.Main`
(`KBot.App` și `KBot.Migrator`), niciodată din inițializatorul de câmp:

> Fontul Calibri nu este instalat pe acest calculator. Aplicația va folosi
> fontul implicit al sistemului, iar unele ferestre pot arăta ușor diferit.

### 3.2 Fontul, pus într-un singur loc per tip de bază

* `KBotThemedForm` — constructor nou, `Font = KBotFonts.Base`.
* `KBotShellForm` — **fără constructor propriu**, moștenește de la
  `KBotThemedForm`. Planul cerea unul pe fiecare; a doua scriere ar fi spus
  același lucru de două ori și ar fi invitat la derivă.
* `KBotThemedUserControl` (nou) — un `UserControl` e `ContainerControl`, deci are
  ștampila LUI și rulează `PerformAutoScale` singur, independent de formularul
  care-l găzduiește. Fără fontul din constructor se măsura de două ori: o dată la
  construire, pe fontul implicit, și încă o dată când era adăugat pe un formular
  cu alt font.

Momentul e toată povestea: constructorul clasei de bază rulează ÎNAINTEA celui
derivat, iar acela cheamă `InitializeComponent`, unde se atribuie
`AutoScaleDimensions` și `AutoScaleMode.Font`. Numai așa formularul poartă deja
fontul corect când e măsurat. Bonus: Visual Studio instanțiază tipul de BAZĂ ca
să deseneze un formular derivat, deci și suprafața de proiectare vede acum
Calibri.

**26 de `.Designer.vb`** au trecut de la `UserControl` la
`KBotThemedUserControl` — 25 în `KBot.App` (`ForexeFooterView`, cele opt vederi,
cele unsprezece pagini DDF, cele cinci pagini ORD) și `KBotRichTextEditor` în
`KBot.Controls`.

`CertificateSelectionForm` și `HistoryForm` (`KBot.Forexe`) au trecut de la
`Form` la `KBotThemedForm`, iar apelurile lor de mână `KBotTheme.ApplyTheme(Me)`
au fost șterse — exact migrarea pe care o descrie rezumatul lui `KBotThemedForm`.

### 3.3 Cele patru scheme

Toate poartă acum `KBotFonts.BaseFontName` / `BaseFontSize` (constantele, nu
literali — un literal aici ar lăsa o schemă să se despartă tăcut de restul
aplicației, adică exact defectul reparat).

Un bloc de comentariu în capul fișierului spune pe față care le citesc:

| Schemă | Drum | Citește fontul? |
|---|---|---|
| Dark, Modern | `StylePalette` → `ApplyBaseFont` | **da** |
| Classic | `StyleSystem` (n-are cod de font deloc) | nu |
| Colorful | `PreserveDesigner` (repune fontul din designer) | nu |

Valorile se scriu oricum, ca intenție declarată a schemei; comportamentul e
identic în ambele feluri, fiindcă formularul poartă deja Calibri 9 din
constructor înainte să se aplice vreo schemă. `StyleSystem` NU a primit o
scriere de font (decizia 4).

### 3.4 Comutatorul «Font din temă»

* `ThemeStore` — câmp nou `themeWritesFormFont` în `theme.json`, implicit `True`;
  `SaveThemeWritesFormFont` / `LoadThemeWritesFormFont`, cu aceeași
  citire-modificare-scriere ca `SaveActive` și `SaveScaling`, ca cele trei să nu
  se calce. Un fișier vechi, fără câmp, primește implicitul.
* `ThemeManager.WritesFormFont` — proprietate publică, citită de `ApplyBaseFont`
  care iese devreme când e stinsă. Setterul persistă ȘI repune pe loc fontul din
  designer (`DesignerBaseline.Restore` pe fiecare fereastră deschisă, apoi
  `Apply`, care rescrie culorile schemei și lasă mărirea textului la urmă, în
  ordinea documentată). Fără `Restore`, comutatorul ar fi părut că nu face nimic
  până la repornire.
* Bara de titlu — rând bifabil «Font din temă», sub cursorul «Mărime text» și
  deasupra separatorului care precedă schemele. Bifa e desenată (16×16, în
  culoarea textului din schema activă) fiindcă `CustomPopupItem` n-are stare de
  bifare: are exact trei roluri, normal / separator / cursor. Stins, rândul n-are
  imagine, iar slotul rămâne gol — alinierea coloanei de pictograme nu se strică.
* Cursorul de mărime se ascunde pe `_showTextScaleSlider AndAlso
  ThemeManager.WritesFormFont`: mărirea textului trece tocmai prin scrierea
  fontului pe formular.

**La ce e bun un comutator care nu se vede.** Fontul fiind același în ambele
locuri, scrierea temei e un no-op și stinsul nu mișcă nimic. Asta E treaba lui:
îi dă operatorului cum să PROBEZE că nu mișcă nimic, și o ieșire dintr-un clic
dacă vreo mașină ajunge totuși să măsoare Calibri altfel — caz în care fereastra
s-ar redimensiona la comutarea temei, iar cauza ar fi altfel imposibil de arătat
cu degetul.

---

## 4. Pasul 4 — măturarea ștampilelor, și de ce n-a mai așteptat verificarea din VS

Verificarea cerută în VS (`OrdEditForm`, font pus pe Calibri 9 cu mâna, citită
ștampila) urma să răspundă la o singură întrebare: **e (9, 22) perechea corectă
pentru un fișier autorat la 144 dpi?** Răspunsul a venit altfel, mai tare, și
fără să se atingă niciun fișier — o sondă care construiește un formular cu
`ClientSize = 1641×1000` și îi dă pe rând cele două ștampile:

```
zona de lucru: 2560×1528       font de bază: Calibri 9

ștampila VECHE (10,25)   mărime=1477×880    poziție=(541,324)   centrat: DA
ștampila NOUĂ  (9,22)    mărime=1641×1000   poziție=(459,264)   centrat: DA
```

`1641×1000` e EXACT mărimea autorată. Deci (9, 22) e perechea, dovedită pe
mărimea care contează, nu pe un număr citit dintr-un fișier.

Și tot de aici se vede ce făcea ștampila veche: **fereastra se deschidea la 90%
pe lățime și 88% pe înălțime** — 1477 în loc de 1641, 880 în loc de 1000. Asta
era «prea îngust» și «prea lat» raportat de operator; nu venea din temă, venea
de aici, și lovea la fel toate cele patru scheme.

### 4.1 Ce s-a măturat

Toate cele 52 de fișiere, după harta din §2.4 — grupurile au ieșit exact cum
fuseseră numărate (40 + 11 + 1):

| Din | În | Fișiere |
|---|---|---|
| `SizeF(10F, 25F)` / `SizeF(10.0F, 25.0F)` / cele două cu `System.Drawing.` | `SizeF(9F, 22F)` | 40 |
| `SizeF(7F, 15F)` / `SizeF(7.0F, 15.0F)` | `SizeF(6F, 14F)` | 11 |
| `System.Drawing.SizeF(8.0F, 20.0F)` | `System.Drawing.SizeF(8F, 18F)` | 1 |

### 4.2 Comentariile care MINȚEAU

Unsprezece fișiere purtau linia «Coordonatele sunt scrise la 96 dpi și
`AutoScaleDimensions` le însoțește». Pe șapte dintre ele era FALSĂ: ștampila era
(10, 25), adică 144 dpi. Nu era o scăpare de redactare — istoria o arată:

```
AlegereUnitateForm.Designer.vb
  80a46cb  (7F, 15F)   SLICE-0048-02 ...
  ab69142  (10F, 25F)  @ AlegereUnitateForm: designer re-save at 150% DPI
```

…iar coordonatele s-au dus cu ea: `pnlCard.Size` a trecut de la `618×576` la
`884×729`, adică lățimea exact ×10/7. **Visual Studio rescrie ÎNTOTDEAUNA
coordonatele și ștampila împreună**, deci fișierul era coerent; doar comentariul
rămăsese din altă viață. Toate unsprezece spun acum adevărul, cu dpi-ul real și
cu perechea Calibri care îi corespunde.

---

## 5. Pasul 6 — versiunile

`FileVersion` incrementat pe cele ȘASE proiecte atinse; `AssemblyVersion` neatins
peste tot:

| Proiect | Din | În |
|---|---|---|
| `KBot.Theming` | 1.11.0.0 | **1.12.0.0** |
| `KBot.Controls` | 1.44.0.0 | **1.45.0.0** |
| `KBot.App` | 1.0.23.0 | **1.0.24.0** |
| `KBot.DevHarness` | 1.0.23.0 | **1.0.24.0** |
| `KBot.Forexe` | 1.0.3.0 | **1.0.4.0** |
| `KBot.Migrator` | 1.6.0.0 | **1.7.0.0** |

`KBot.DevHarness` nu era în lista planului, dar patru din ștampilele măturate
sunt ale lui — deci s-a schimbat, deci se incrementează.

---

## 6. Ferestrele de editare — lățime și centrare

Cerut de operator: editoarele DDF și ORD pornesc **cât fereastra principală și în
centrul ecranului**.

`StartPosition = CenterScreen` era deja pe toate trei, și e corect — ce lipsea era
mărimea. `ClientSize` a trecut de la `1300×1000` la `1641×1000` pe
`DdfEditForm` și `OrdEditForm`, adică fix mărimea lui `MainForm`.

**Lărgirea e sigură prin construcție, nu prin noroc.** Ambele au `tlyMain` cu o
singură coloană `Percent 100` andocată `Fill`, iar antetul (`tlyAntet`) are șapte
coloane `Absolute` urmate de o a opta `Percent 100`. Cei 341px în plus se duc
integral în acea ultimă coloană — niciun câmp nu se mută și niciunul nu se
întinde.

---

## 7. Verificat PE ECRAN (nu doar măsurat)

Prima felie din proiect verificată rulând aplicația, nu doar citind numere.
Operatorul a dat mașina și a autentificat; capturile s-au luat cu `PrintWindow`
pe fereastra K-BOT (deci fără să fure focusul și fără să prindă nimic altceva de
pe ecran).

`MainForm`, pe rând pe toate cele patru scheme, comutate din meniul barei de titlu:

| Schemă | Dreptunghi | Mărime |
|---|---|---|
| Clasic | (459,264)–(2100,1264) | **1641×1000** |
| Întunecat | (459,264)–(2100,1264) | **1641×1000** |
| Modern | (459,264)–(2100,1264) | **1641×1000** |
| Colorat | (459,264)–(2100,1264) | **1641×1000** |

Mărimea autorată, poziția centrată calculată (`(2560−1641)/2 = 459`,
`(1528−1000)/2 = 264`), și **niciun pixel de diferență între scheme** — nu doar
fereastra, ci fiecare control din ea, în aceeași poziție în toate patru capturile.

Comutatorul «Font din temă» s-a văzut și el în meniu, cu bifa lui, și pe fundal
deschis, și pe întuneric.

**Ce NU s-a apucat să se vadă:** editoarele DDF și ORD la lățimea nouă. Sesiunea
a expirat în timpul navigării, a apărut dialogul de re-autentificare, iar o parolă
nu se poate tasta din partea asta. Ștampila, `ClientSize` și `StartPosition` sunt
identice cu ale lui `MainForm`, care tocmai s-a dovedit pe ecran, deci mecanismul
e probat — dar cele două ferestre în sine rămân NEVĂZUTE. Consemnat ca atare.

---

## 8. Ce s-a construit și ce s-a rulat

`dotnet build KBot.sln --no-incremental`, RULAT DIN NOU după măturarea ștampilelor —
**0 erori / 14 avertismente**, toate
`MSB3825` PREEXISTENTE pe `.resx`-uri (aceeași linie de bază ca felia 0051-02).

| Suită | Rezultat |
|---|---|
| `KBot.Theming.Tests` | **110 / 110** |
| `KBot.Controls.Tests` | **964 / 964** |
| `KBot.App.Tests` | 194 trecute / **13 picate — ACELEAȘI 13** ca înainte de felie, toate din alte felii (`DdfView`, `IstoricView`, `XfaXmlPreview`, parserul XFA, iconițele `MainForm`) |

**Zero teste noi** (cerut). Patru teste EXISTENTE au fost corectate, fiindcă
fixau valorile pe care felia le schimbă deliberat — nu regresii, ci contractul
vechi:

1. `BuiltInSchemesTests.Modern_HasRoundedOwnerDrawnButtons_AndVariableFont` →
   `…_AndBaseFont`. Cerea `"Segoe UI Variable Text"`. Verifică acum față de
   `KBotFonts.BaseFontName`, nu față de un literal.
2. `BuiltInSchemesTests.Colorful_…` cerea `BaseFontSize = 0`. Apărarea fontului
   nu mai vine din VALOARE, ci din DRUM (`PreserveDesignerColors`, verificat în
   același test).
3. `CustomPopupSliderTests.Meniul_de_tema_incepe_cu_cursorul_de_marime` și
   `KBotCaptionBarThemeButtonTests.Meniul_nu_arata_schema_activa` /
   `Fara_nicio_unealta_pleaca_si_separatorul` — rândul nou de meniu. Ultimul s-a
   redenumit `Fara_nicio_unealta_pleaca_separatorul_uneltelor`: meniul nu mai
   poate rămâne fără NICIUN separator, dar poate rămâne cu exact unul.
4. `KBotDataViewBandThemingTests.BandFont_FollowsTheActiveSchemesFont_…` compara
   Classic cu Modern ca să arate că fontul benzii vine din schemă. Premisa a
   dispărut — toate patru schemele poartă acum același font. Proba se face acum
   pe o schemă mutată intenționat, ceea ce e și mai bine: nu mai depinde de ce
   fonturi sunt instalate pe mașina de test și nu se strică la următoarea schemă
   aliniată.

**NIMIC NU S-A VĂZUT PE ECRAN.** Niciun formular nu a fost deschis, nici în
aplicație, nici în designerul VS, nici prin `DrawToBitmap`. Singurul lucru
măsurat pe mașina adevărată e sonda din §2.5.

---

## 9. Urmări cunoscute și fire deschise

1. **Visual Studio va începe să scrie `Font = New Font("Calibri", 9F)` în
   `.Designer.vb`.** Constructorul de bază atribuie fontul, deci
   `ShouldSerializeValue(Font)` întoarce `True`, deci serializatorul îl scrie la
   prima salvare din VS. NU s-a pus un `ShouldSerializeFont` care să-l ascundă,
   pentru că acela ar fi rupt mărirea textului pe controalele de utilizator:
   `FontBaseline.ApplyScale` scalează individual doar controalele cu font
   PROPRIU, iar un font ascuns de serializare, dar totuși fixat, n-ar mai fi nici
   scalat, nici moștenit. Linia scrisă e corectă și stabilă; e o alegere, nu o
   scăpare, și se poate întoarce dacă deranjează (cere și o excepție în
   `FontBaseline`).
2. **Aspectul, formular cu formular, e în afara feliei.** Calibri 9 e mai îngust
   și mai scund decât Segoe UI 9 (22 față de 25 la 150%), deci etichetele vor sta
   mai lejer în spațiul lor și unele așezări vor părea răsfirate. Nimic nu se
   taie — un text mai îngust nu poate da pe dinafară. Operatorul umblă prin
   formulare după aceea.
3. **Rândul «Font din temă» nu se poate ascunde.** Cele șapte dialoguri care sting
   cursorul de mărime (`OrdEditForm`, `DdfEditForm`, `OrdTextForm`, `OrdZiuaForm`,
   `AlegereUnitateForm`, `AsociereForm`, `AsociereBenziForm`) îl vor arăta
   totuși. Argumentul pentru: un comutator care poate fi ascuns tocmai de
   formularul pe care s-ar investiga problema nu e bun la nimic. Argumentul
   contra: e o unealtă de diagnostic pe fiecare dialog modal. **De hotărât de
   operator.**
4. **`FrmNodeDebug`** (`KBot.Controls/Tree`) rămâne pe `Segoe UI` 9 — scris
   explicit în designerul lui, singurul formular din soluție care-și fixează
   fontul — și **`WicketMonitorForm`** (`KBot.Forexe`) rămâne `Form` simplu. Ambele
   sunt în afara feliei prin decizia 8 și rămân consecvente cu ele însele.
5. **Cele șase formulare din `KBot.DevHarness` fără linie de
   `AutoScaleDimensions`** (`DevHarnessForm`, `DataViewHarnessForm`,
   `PopupPlaygroundForm`, `ThemeGalleryForm`, `TreePlaygroundForm`,
   `TreeVisualForm`) nu se ating în pasul 4, conform planului.
6. **`CLAUDE.md` e în `.gitignore`** în depozitul ăsta, deci lărgirea regulii
   («base forms and base user controls») e o schimbare LOCALĂ și nu pleacă odată
   cu commit-ul.
7. **Garda de compilare** care ar pica build-ul pe o ștampilă greșită rămâne
   nescrisă — contează prima dată când cineva deschide un formular în VS pe un
   monitor la 96 dpi, fiindcă atunci se re-ștampilează tăcut. De trecut în
   `docs/possible_future_directions.md`.
8. **Editoarele DDF și ORD n-au fost VĂZUTE la lățimea nouă.** Sesiunea a expirat
   în timpul navigării spre ele. Mecanismul e probat pe `MainForm` (aceeași
   ștampilă, același `ClientSize`, același `StartPosition`), dar cele două
   ferestre în sine rămân neverificate pe ecran. De privit la prima ocazie.
9. **Există un fișier de schemă de utilizator care ÎNLOCUIEȘTE «Modern»
   compilat**: `%AppData%\AVACONT\Themes\Modern.json`. Descoperit în timpul
   diagnosticării. Consecința: **editările din `BuiltInSchemes.Modern()` nu au
   niciun efect pe mașina asta** — `MergeSchemes` pune fișierul peste built-in,
   iar de la pornirea următoare acela E «Modern». Aici n-a stricat nimic (fișierul
   poartă deja `Calibri` 9 și `ControlPadding` 0), dar e o capcană: o schimbare de
   schemă făcută în cod și „probată” pe mașina asta poate părea că n-a funcționat.
   Ștergerea fișierului readuce schema compilată. Lângă el mai stau două
   instantanee, `Modern.json.pre-revert.bak` și `Modern.json.pre-slice-0049.bak`;
   nu se încarcă (nu se termină în `.json`), dar nici nu servesc la nimic.
10. **`ThemeStyleOptions.ControlPadding` nu e citit de nimeni** în motorul de teme
    — singurul lui consumator e grila de proprietăți din fereastra de opțiuni
    (`SchemeOptionsProxy`). Verificat în timpul diagnosticării, fiindcă era
    principalul suspect pentru «Modern prea lat» (are `12,8,12,8` față de `0` la
    celelalte trei). Nu era el. Rămâne o opțiune pe care operatorul o poate muta
    din fereastra de opțiuni fără ca ea să facă ceva.
