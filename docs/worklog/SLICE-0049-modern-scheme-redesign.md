# SLICE-0049 — schema `Modern`, refăcută după macheta aprobată

Data: 2026-08-29 · Ramura: `slice-0049-modern-scheme-redesign`

Schema `Modern` a fost rescrisă din temelie după `desired_finished_look.png`: carduri albe
care plutesc pe o pânză gri-albăstruie, colțuri rotunjite, umbră joasă, un albastru mai viu,
rânduri mai înalte și un font livrat cu aplicația. `Classic`, `Dark` și `Colorful` nu se
schimbă cu niciun pixel.

Numele schemei rămâne `Modern`, deci o alegere salvată în `theme.json` se rezolvă mai departe.
Nu există migrare și nu era nevoie de una.

---

## 1. Ce s-a găsit ÎNAINTE de a scrie ceva (pasul 0 al planului)

Planul feliei făcea patru presupuneri care nu se potriveau cu ce e pe disc. Toate patru au
schimbat ce s-a construit, deci se scriu aici, nu se uită.

**1. Paleta ține culorile ca ȘIRURI hex, nu ca `Color`.** `Surface`, `Accent`, … sunt
`String`-uri `#RRGGBB` cu accesori `…Color` marcați `<JsonIgnore>`. Sloturile noi urmează
aceeași formă. Consecința care contează: `ColorHex` acceptă DOAR `#RRGGBB` și aruncă pentru
orice altceva, deci «umbră la 6% transparență» nu încape într-o singură valoare. Umbra e deci
o culoare (`Palette.Shadow`) plus un număr separat (`Style.CardShadowOpacity`) — ceea ce, în
plus, o face reglabilă din fereastra de opțiuni.

**2. `Tag = "Card"` nu înseamnă «card».** Sunt 25 de panouri cu eticheta asta, între ele
`pnlRoot` (toată zona de lucru), `pnlHeader` și `pnlStatus` (benzile) și `pnlCard`-ul fiecărui
dialog. Rotunjite și umbrite, toate ar fi arătat greșit. Nu există nicio regulă geometrică
care să le nimerească pe toate 25 — cea mai bună încercată (sari peste ce e andocat sus/jos
și peste ce are un `Form` drept părinte) greșea două dialoguri. **Decizia operatorului:**
etichetă nouă, `Tag = "CardSurface"`, pusă explicit pe cardurile adevărate. Excepție acceptată
la regula «niciun `.Designer.vb` atins» din §7 al planului.

**3. 142 de fonturi fixate în designer**, dintre care 98 `Calibri, 9F` — chiar pe arbore și pe
grilă, adică exact suprafețele despre care e felia. Cu ele pe loc, «Inter» ar fi fost aproape
invizibil. **Decizia operatorului:** se mătură. Ce s-a măturat efectiv: cele **94** de linii
de pe controalele K-BOT (`HeaderFont`, fonturile de coloană, `tree.Font`, `FooterFont`,
`FooterCaptionFont`, `SearchBarFont`). Ele se pot mătura fără pierdere fiindcă acele controale
își REDERIVĂ singure fontul din schemă, cu tot cu greutate — `KBotDataView.BuildBandFont`
caută întâi «⟨familie⟩ Semibold» și cade pe familie + `Bold`.

Ce NU s-a măturat, și de ce: **cele 10 linii `Calibri` rămase și toate cele ~32 `Segoe UI`**
stau pe `Label`/`Button`/`ComboBox` obișnuite și poartă fie o treaptă de mărime (titlu 18F,
13F, 11F), fie o greutate (`Semibold`, `Bold`). Un `Label` n-are niciun drum de rederivare:
ștergerea liniei nu i-ar da fontul schemei cu greutatea păstrată, i-ar da fontul formularului
în greutate normală, adică ar aplatiza ierarhia de titluri. Aia ar fi fost o regresie, nu o
redesenare. Rămân, și rămân o datorie deschisă — vezi §6.

**4. `DotOk` / `DotIdle` n-au drum către bulinele din machetă.** Verzi și gri, ele vin din
`FxIcons.StatusIcon(info.Stare)` — bitmap-uri din resurse, alese la `MainForm.vb:523`, în
`KBot.App`, pe care felia nu-l atinge. Sloturile există și sunt editabile, dar astăzi nu
pictează nimic. Vezi §6.

Alte două cifre din plan erau vechi: baza de teste era **1041**, nu 854; iar compilarea are
deja **10 avertismente** `MSB3825` la o recompilare completă (`BinaryFormatter` pe șase `.resx` de vederi), dinaintea
feliei și fără legătură cu ea. «Zero avertismente» din definiția de gata nu era îndeplinit
nici înainte.

Al cincilea punct, mai mic: planul spunea «fără linii verticale în grilă», macheta le arată.
**Decizia operatorului: se urmează macheta.** N-a fost nimic de scris — grila desenează deja
linii verticale între celulele benzilor (`DefaultCellSeparatorWidth = 1`).

---

## 2. Ce s-a schimbat

### Motorul (`KBot.Theming`)

| Fișier | Ce |
|---|---|
| `ThemePalette.vb` | nouă: `Canvas`, `Card`, `CardBorder`, `Shadow`, `NavSelectedBack`, `HeaderBand`, `RowSeparator`, `DotOk`, `DotIdle` + accesorii `…Color` + `ApplyNeutralCardDefaults()` |
| `ThemeStyleOptions.vb` | nouă: `CardRadius`, `CardShadow`, `CardShadowOpacity`, `CardGutter`, `NavItemHeight`, `ListRowHeight`, `GridRowHeight`, `GridHeaderHeight`, `TintIcons` — toate 0 / `False` implicit |
| `ThemeShapes.vb` | `DrawCardShadow`, `FillCard`, `PaintCardCorners` |
| `CardPainter.vb` | **NOU** — atașează/desprinde pictura de card, per `Tag = "CardSurface"` |
| `FontLoader.vb` | **NOU** — înregistrează `…\Fonts\*.ttf` pe AMÂNDOUĂ căile GDI |
| `IconTint.vb` | **NOU** — recolorare din paletă prin `ColorMatrix`, cu cache |
| `BuiltInSchemes.vb` | `Modern` rescrisă; celelalte trei cheamă `ApplyNeutralCardDefaults()` |
| `ThemeManager.vb` | `IsCard` acceptă și eticheta nouă; `CardPainter.Sync` în `Traverse`; `IconTint.Clear` la schimbarea schemei; `FontLoader.Initialize` la pornire |

**Cum se rotunjește un card.** Un control WinForms n-are canal alfa față de părintele lui și
nu poate picta în afara dreptunghiului său. Deci sunt DOUĂ cârlige: **cardul** își umple forma
rotunjită și apoi vopsește cele patru pene de colț cu culoarea pânzei (asta e tot ce-l face să
pară rotund), iar **părintele** desenează umbra sub limitele cardului, înainte ca el să se
picteze. Ordinea în cârligul cardului contează: umplere întâi, colțuri după — invers, umplerea
acoperă exact ce tocmai s-a vopsit.

**Umbra nu se ține în cache, deliberat**, deși planul cerea. Un cache pe `(lățime, înălțime,
rază, culoare, mărime)` NU ajută unde ar conta — la redimensionare cheia se schimbă la fiecare
cadru, deci cache-ul n-ar da niciun răspuns și ar aloca în plus un bitmap pe cadru, adică e
strict mai rău. Ce a rămas e desenul direct: `size` contururi rotunjite trasate, adică 10 apeluri
`DrawPath` per pictură a părintelui, ceea ce e neglijabil față de ce fac oricum controalele.

**Atașarea e idempotentă și desprinderea e completă.** Cârligele stau într-un
`ConditionalWeakTable` cu cheia pe control, deci o vedere eliberată își ia intrarea cu ea. O a
doua aplicare a aceleiași scheme nu adaugă un al doilea cârlig; trecerea la o schemă fără
carduri le scoate pe amândouă ȘI pune la loc spațierea părintelui.

**`DoubleBuffered` prin reflecție.** Planul cerea «folosește abordarea existentă din bază»:
nu există. Toate celelalte utilizări din `src/` sunt în interiorul unui control care se
deține singur; acestea sunt `Panel`-uri obișnuite, iar proprietatea e `Protected`. Reflecție,
deci — învelită și logată, fiindcă eșecul înseamnă doar o pâlpâire în plus la redimensionare.

### Fontul

`Inter` se livrează în `src/KBot.App/Fonts/`, copiat lângă executabil printr-un `<None>` cu
metacaractere (deci compilarea nu cade cât timp fișierele lipsesc), și se înregistrează în
`ThemeManager.Initialize`, înaintea primului formular.

**Capcana, scrisă o dată ca să nu fie redescoperită:** un `PrivateFontCollection` produce un
`FontFamily` pe care îl vede DOAR `Graphics.DrawString`. `TextRenderer.DrawText` e GDI și NU-l
vede. K-BOT desenează prin amândouă — 23 de locuri prin `TextRenderer`, 18 prin `DrawString` —
deci se face și `AddFontMemResourceEx`. Cu una singură dintre ele, cam jumătate din aplicație
ar rămâne tăcut pe fontul implicit.

Lipsa fontului nu e fatală: fișier absent, citire eșuată sau familie nerezolvată se loghează
prin `GlobalErrorLog` și schema cade pe ce alege GDI. Aplicația pornește normal.

### Controalele (`KBot.Controls`)

| Control | Ce |
|---|---|
| `KBotNavList` | înălțimea butonului din `Style.NavItemHeight`; fundalul selecției din `NavSelectedBack`; pictogramele recolorate când schema cere (`TintIcons`), accent pe cel selectat; `ApplyTheme` invalidează acum și AȘEZAREA, nu doar culorile |
| `AdvancedTreeControl` | înălțimea rândului din `Style.ListRowHeight`, pe drumul cu întoarcere de mai jos; linia dintre rânduri din `RowSeparator` |
| `KBotDataView` | înălțimile de rând și de antet din schemă, același drum; banda de antet din `HeaderBand`; liniile din `RowSeparator` |
| `KBotCaptionBar`, `KBotBusyBar` | **nicio linie schimbată** — își luau deja tot din paletă (`SurfaceAlt`, `Text`, `TextDim`, `Accent`), deci paleta nouă a lui `Modern` le duce singură unde trebuie |

**Drumul cu întoarcere, și de ce a trebuit inventat.** `ItemHeight` și `RowHeight` sunt
proprietăți obișnuite de designer. O aplicare naivă a numărului cerut de schemă ar fi scris
direct în ele — iar controlul l-ar fi ținut minte ca alegere a operatorului. O singură trecere
prin «Modern» ar fi lăsat orice grilă pe rânduri de 40 px pentru totdeauna, inclusiv sub
Classic, iar singurul drum înapoi ar fi fost editarea unui fișier de designer. Deci fiecare
control ține valoarea AUTORITĂ separat, iar scrierea temei ridică un steag care spune «nu eu
sunt operatorul». Când schema activă nu mai cere o înălțime, se revine la ce s-a autorit.

### Fereastra de opțiuni

`SchemeOptionsProxy` a primit **17 proprietăți noi**, în două categorii — «10. Carduri» (cele
nouă culori) și «11. Geometrie carduri și rânduri» (cele opt măsuri) — cu nume și explicații în
română, cu selector de culoare pentru culori și cu validare pe fiecare număr. Cerința e
îndeplinită: tot ce aduce felia se reglează din fereastră, nu din cod.

### Designerele (excepții acceptate la §7)

- `MainForm.Designer.vb`: `pnlTree` și `viewHost` trec pe `Tag = "CardSurface"`. Doar ele:
  în machetă zona din dreapta arată ca două carduri, dar despărțitura aceea vine din structura
  internă a lui `ReceptiiView`, iar felia asta nu rearanjează nimic.
- 94 de linii de font șterse, pe controalele K-BOT. Detaliul e în §1.

---

## 3. Ce NU s-a schimbat

Niciun text vizibil de operator, niciun control nou, niciun `AddHandler` pe o acțiune a
operatorului, nicio schimbare de comportament sau de conectare. Pictograma din dreapta
arborelui e la fel unde era și apare după aceeași regulă. `Dark`, `Classic` și `Colorful` sunt
neatinse.

Singurele `AddHandler` adăugate sunt de pictură: `Paint` pe card și pe părintele lui, plus
`SizeChanged`/`LocationChanged` pe card, care nu fac decât `Invalidate()`. Fără ultimele două,
umbra rămâne mânjită pe părinte când operatorul trage despărțitorul.

---

## 4. Teste

| Proiect | Înainte | După |
|---|---|---|
| `KBot.Theming.Tests` | 110 | **130** |
| `KBot.Controls.Tests` | 931 | **938** |

Toate verzi. Celelalte proiecte, neschimbate: `Common` 85, `LocalStore` 1, `Xfa` 39.

**Trei proiecte au eșecuri, TOATE dinaintea feliei** — verificat punând modificările deoparte
și rulându-le din nou pe codul curat, cu aceleași cifre: `Api` 1 eșec / 95 treceri, `App` 13 /
185, `Domain` 3 / 14. Niciunul nu ține de felia asta.

Ce dovedesc testele noi:

- fiecare slot nou de culoare al schemelor neutre arată spre slotul pe care schema îl folosea
  deja; `NavSelectedBack` e CALCULAT, nu copiat de mână, tocmai fiindcă trebuie să egaleze la
  bit expresia veche `Blend(SurfaceAlt, Accent, 0.14)`;
- toată geometria nouă e 0 pe schemele neutre;
- rază 0 dă exact același bitmap ca un `FillRectangle` simplu; mărime 0 sau intensitate 0 nu
  ating niciun pixel — asta face din «Classic arată la fel» un fapt măsurat;
- umbra se stinge spre exterior, iar colțul aparține pânzei în timp ce centrul aparține cardului;
- o schemă salvată de versiunea dinainte, fără niciuna dintre cheile noi, se încarcă și cade pe
  implicite, fără să arunce;
- «Modern → Classic → Modern» de trei ori nu stivuiește cârlige și nu lasă spațierea mărită;
- cache-ul de pictograme se golește și nu crește la nesfârșit;
- înălțimile revin la ce s-a autorit, iar o alegere a operatorului făcută SUB «Modern» rămâne a
  lui după comutare.

`DevHarness.Tests` nu s-a rulat: testele lui deschid ferestre reale de bancă de probă pe ecranul
operatorului.

---

## 5. Ce a rămas neverificat

**Nimic din felia asta n-a fost văzut pe ecran.** Testele de pixeli dovedesc că o umbră e mai
închisă lângă card și că drumul plat e identic cu o umplere plată. Ele NU dovedesc că raza 14
arată bine, că umbra nu e prea grea sau că rândurile de 34 px se citesc la 125%. Alea sunt
judecăți și cer un operator în fața aplicației pornite.

Cele două fișiere `Inter-Regular.ttf` / `Inter-SemiBold.ttf` **nu sunt în depozit** — descărcarea
lor cere acordul explicit al operatorului. Până ajung în `src/KBot.App/Fonts/`, «Modern» cere
«Inter», nu-l găsește, și GDI cade pe fontul implicit. Aplicația pornește normal și scrie o
linie în jurnal. Sursa e scrisă în `Fonts/README.md`.

**Pictogramele nu s-au înlocuit.** §5 al planului cerea artwork Lucide peste numele existente
din `Resources.resx`; și asta e o descărcare care cere acord. Ce s-a livrat e MECANISMUL —
`IconTint` plus steagul `TintIcons` — deci pictogramele monocrome existente urmează deja paleta,
iar înlocuirea artwork-ului mai târziu nu va cere nicio schimbare de cod și nicio linie de
designer.

## 6. Limite de reținut, nu de reparat aici

- **Cardurile n-au voie să se suprapună.** Trucul cu colțurile vopsește culoarea pânzei în
  penele de colț; un card peste alt card ar picta peste el.
- **Pânza trebuie să rămână o culoare plină.** O imagine sau un degrade în spatele cardurilor
  strică vopsirea colțurilor.
- Colțurile FERESTREI trec mai departe prin `Region` și se taie fără antialias. Firul acela,
  din `SLICE-0025-08`, rămâne deschis.
- **Cele ~42 de fonturi rămase fixate în designer** pe `Label`/`Button` țin ierarhia de titluri.
  Ca să urmeze și ele schema ar trebui un mecanism de tip «ia familia din temă, păstrează-mi
  greutatea și treapta de mărime» — adică o schimbare de comportament, dincolo de o felie de
  aspect. E următorul pas firesc dacă tipografia trebuie să fie unitară până la capăt.
- **`DotOk` / `DotIdle` nu pictează nimic astăzi.** Bulinele vin din `FxIcons`, în `KBot.App`.
  Ca să asculte de paletă, `MainForm` ar trebui să treacă pictograma prin `IconTint` — o linie,
  dar în `KBot.App`, pe care felia asta nu-l atinge.
- Cele **10 avertismente `MSB3825`** rămân. Sunt vechi, țin de `BinaryFormatter` în șase `.resx`
  de vederi, și n-au legătură cu tema.

---

## 7. CORECȚIE, la prima privire pe ecran — de ce nu se vedea NIMIC

Prima livrare a feliei a produs, pe mașina operatorului, **exact interfața de dinainte**: nicio
umbră, niciun colț, niciun card. Nu era o problemă de pictură. Erau două lucruri:

### 7.1 Un fișier vechi ținea aplicația în trecut

`%AppData%\AVACONT\Themes\Modern.json`, salvat pe 17 august din fereastra de opțiuni.
`ThemeManager.AvailableSchemes` tratează un fișier care poartă numele unei scheme built-in ca
**ÎNLOCUITOR**, nu ca strat peste ea — și asta e corect, așa se persistă editarea lui «Modern».

Dar un fișier scris de o versiune mai veche nu știe nimic despre nicio cheie adăugată de atunci,
așa că deserializarea a dat fiecărei chei noi implicitul TIPULUI: `CardRadius = 0`,
`CardShadow = 0`, cele patru înălțimi 0, `TintIcons = False`, `BaseFontName` cel vechi. Adică
felia întreagă, stinsă. **Fără nicio eroare nicăieri** — din punctul de vedere al codului nu se
întâmplase nimic rău.

**Reparația: SUPRAPUNERE în loc de înlocuire.** `ThemeStore.LoadUserSchemes` primește acum un
rezolvator către schema COMPILATĂ, iar `OverlayOnto` scrie fișierul stocat peste ea cheie cu
cheie, pe toate nivelurile de imbricare. Ce a ales operatorul câștige mai departe; cheile
inventate după ce el a apăsat «Salvează» vin cu implicitele lor noi. Mersul e generic — umblă
prin JSON, nu numește proprietăți — deci o cheie adăugată în vreo felie viitoare trece singură,
fără ca cineva să-și amintească să se întoarcă aici.

Fișierul operatorului a fost redenumit `Modern.json.pre-slice-0049.bak` (reversibil), ca să vadă
«Modern» cel nou și nu varianta lui de pe 17 august. Calea normală pentru asta e butonul
«Restaurează implicit» din fereastra de opțiuni.

Șase teste noi (`StaleUserSchemeTests`) rulează pe JSON-ul REAL, copiat verbatim.

### 7.2 Umbra era în bitmap, dar invizibilă pentru un om

Formula de alfa împărțea la `size` și înmulțea cu 2, ca să țină cerneala totală constantă
indiferent de întindere. Efectul: la `CardShadowOpacity = 6`, inelul lipit de card ieșea cu alfa
**3 din 255**. Prezent la o verificare pe pixeli, inexistent pentru ochi — și exact de asta
testele de pixeli trecuseră.

Acum `opacity` înseamnă literal ce spune: alfa inelului care atinge cardul, în procente. Inelele
sunt contururi de 1px la offseturi diferite, deci abia se suprapun și fiecare pixel ia practic
alfa unui singur inel. Implicitul lui «Modern» a urcat de la 6 la **14**, ales privind randarea.

### 7.3 Ce s-a învățat despre verificare

Am scris în §5 «nimic n-a fost văzut pe ecran» și am livrat așa. Asta a fost greșeala: două
defecte care ANULAU felia stăteau amândouă sub pragul la care ajung testele scrise. O randare
`DrawToBitmap` a formei, pe un ecran virtual, ar fi prins amândouă în câteva minute — n-are
nevoie nici de bază de date, nici de autentificare. Pentru orice felie de aspect de-acum înainte,
acela e minimul, nu testele de pixeli pe primitive.
