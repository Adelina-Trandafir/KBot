# SLICE-0048-05 — mesajul care nu se stingea + graficul evoluției (`KBotChartView`)

**Cerut de operator, 31.08.2026, două lucruri într-o singură cerere:**

> «1. apare corect mesajul de eroare, dar dacă selectez alt nod nu mai dispare - fă-l să dispară»
>
> «2. a rămas un loc liber (dreapta-sus). acolo aș vrea un grafic care să arate evoluția recepției
> respective - cu popup pe hover per noduri din grafic sau, dacă se apasă un buton într-un custom
> panel deasupra (cel care ține loc de tabcontrol) să apară graficul per tot angajamentul.
> Controlul TREBUIE să fie custom, trebuie să fie abonat la theming, trebuie să expună toate
> propr configurabile în IDE (în properties tab) și numele grupelor de proprietăți, ca și numele
> variabilelor și explicațiile inline TREBUIE să fie în engleză.»

**Două întrebări puse înainte de a scrie o linie, și răspunsurile operatorului:**

| Întrebare | Răspuns |
|---|---|
| Ce desenează graficul «per tot angajamentul»? | **Linii per recepție + linie de total** (nu doar suma, nu doar liniile) |
| Banda de butoane e control separat sau bandă în interiorul graficului? | **Bandă în interiorul graficului** |

**Stare:** cod verde — `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` build 0 erori
(doar avertismentele `MSB3825` preexistente pe `.resx`-uri). `KBot.Controls.Tests` **952 trecute /
1 picată**, iar cea picată e **preexistentă și străină de felia asta** (vezi §6).
**Formularul NU a fost deschis nici pe ecran, nici în designerul Visual Studio.** Controlul, în
schimb, **a fost desenat și privit** — vezi §5.

---

## 1. Mesajul care nu se stingea

`ntfMesaj` e o casetă de notificare tranzitorie: o pune `ReincarcaAsync` (nu există instantanee /
n-am putut citi), o pune `AratMeniul` (legătura e blocată, cu motivele), o pune
`AplicaComandaDeMeniu` (comanda a eșuat) și o pune `btnSalveaza_Click` (rezultatul salvării).
Nimeni nu o stingea în afară de `ReincarcaAsync`.

Efectul pe ecran: refuzul unui instantaneu rămânea sub cele două liste în timp ce operatorul
lucra pe următorul, și **se citea ca și cum ar fi fost despre acela**.

Reparația e într-un singur loc, `Tree_NodeMouseUp`, și e prima instrucțiune din el:

```vb
ntfMesaj.Clear()
```

Se stinge **înainte** de orice altceva, deci `AratMeniul` (clic-dreapta pe o legătură blocată)
poate pune imediat mesajul lui la loc — ordinea contează, iar asta e ordinea care o dă.

Alegerea unui alt rând înseamnă că operatorul a trecut mai departe, deci se sting **toate**
felurile de mesaj, nu doar erorile — inclusiv «s-a salvat» și «angajamentul nu are niciun
instantaneu». Un mesaj de succes care supraviețuiește următorului clic e la fel de mincinos ca
unul de eroare.

---

## 2. Controlul nou: `src/KBot.Controls/Chart/`

O familie nouă, într-un folder al ei, ca orice control K-BOT:

| Fișier | Ce e |
|---|---|
| `KBotChartEnums.vb` | `KBotChartMarkerStyle`, `KBotChartTabAlign`, `KBotChartValueAxisMode` |
| `KBotChartPoint.vb` | punctul (`Moment`, `Value`, cele trei texte de etichetă) + colecția lui |
| `KBotChartSeries.vb` | seria (`Key`, `Text`, `LineColor`, `Visible`, `Emphasis`, `FillArea`, `Points`) + colecția ei |
| `KBotChartTab.vb` | butonul benzii (`Key`, `Text`, `Icon`, `Enabled`, `Visible`, `Tooltip`) + colecția lui |
| `KBotChartView.vb` | proprietățile, API-ul public, `ISupportInitialize`, tema |
| `KBotChartView.Painting.vb` | așezarea, pictura, mouse-ul, tastatura, eticheta plutitoare |

**Tot ce e în fișierele astea e în engleză** — identificatori, comentarii, `<summary>`, numele
categoriilor din grila de proprietăți și textele `<Description>`. Singurele șiruri în română sunt
cele pe care le scrie GAZDA (captions de buton, `EmptyText`, etichetele punctelor), fiindcă alea
le citește operatorul. Asta e exact cererea, și e și regula 0 din `CLAUDE.md`.

### 2.1 Banda de deasupra («ține loc de tabcontrol»)

E desenată de control, nu compusă din controale-copil, din același motiv pentru care arborele își
desenează antetul: o bandă de butoane reale ar fi avut nevoie de tematizarea ei, de pasul ei de
DPI și de serializarea ei în designer, și tot n-ar fi fost aliniată cu graficul de sub ea.

E **single-select** — asta o separă de `KBotChipBar`, care e multi-select: sub bandă încape un
singur grafic odată. Controlul **nu decide ce ÎNSEAMNĂ un buton**: ridică `TabSelected(cheia)` și
gazda reumple seriile.

`SelectedTabKey` (atribuire) **nu** ridică evenimentul; `SelectTab(cheie)` **da**. Prima e gazda
care constată un fapt, a doua e drumul pe care merge și clicul — o gazdă care și-ar fi ridicat
singură evenimentul ar fi intrat în propriul tratator.

### 2.2 Ce se vede în grila de proprietăți

Șapte categorii, toate în engleză:

`K-BOT Chart Appearance` · `K-BOT Chart Axes` · `K-BOT Chart Data` · `K-BOT Chart Header` ·
`K-BOT Chart Legend` · `K-BOT Chart Plot` · `K-BOT Chart Tabs` · `K-BOT Chart Tooltip`
(plus `K-BOT Chart Point/Series/Tab` pe elementele din dialogurile de colecție).

Sunt configurabile din IDE, printre altele: colecțiile `Series` și `Tabs` (dialogul standard de
colecție), înălțimea și gradientul benzii, alinierea/raza/aerul butoanelor, grosimea liniei
obișnuite și a celei accentuate, forma și mărimea marcatorului, opacitatea suprafeței,
vizibilitatea și culorile axelor și ale grilei, numărul de trepte, formatele de număr și de dată,
legenda, textul de gol și eticheta plutitoare (`PointTooltip.Style.…`).

### 2.3 Regulile casei, respectate punct cu punct

- **`IThemedControl`** — `ApplyTheme` scrie `BackColor`/`ForeColor` prin `MyBase`, reface
  creioanele memorate și repictează. Nicio culoare nu e scrisă în cod: **fiecare proprietate de
  culoare e `Color.Empty` = «din temă»**, iar o serie fără culoare primește una derivată din
  paletă (accent → success → warning → error → tab-accent, apoi aceleași cu o treaptă mai
  deschisă), deci un grafic neatins urmează schema.
- **`ShouldSerialize*` / `Reset*`** pe `BackColor`, `ForeColor`, `Font` (cu steaguri «operatorul a
  fixat asta»), pe `HeaderFont`, `AxisFont`, `TabIconSize` și pe toate cele nouă culori proprii.
  `Size` e ținut afară prin `DefaultSize`. **Un grafic proaspăt pus pe un formular scrie ZERO
  linii de proprietate** — dovedit prin `TypeDescriptor`, drumul pe care merge chiar Visual
  Studio, nu prin apelarea propriilor `ShouldSerializeX` (aia n-ar dovedi nimic).
- **DPI** — toate măsurile publice sunt **px logici la 96 dpi** și se scalează la pictare prin
  `ThemeShapes.ScaleDpi`, adică `DeviceDpi / 96`, **o singură sursă**. `OnHandleCreated`,
  `OnDpiChangedAfterParent`, `OnFontChanged` și `OnSizeChanged` aruncă așezarea.
- **Etichetă plutitoare = `KBotToolTip`**, niciodată `System.Windows.Forms.ToolTip`: marcatoarele
  sunt zone pictate, nu controale, deci nu există ce să fie extins. Se folosește `ShowAt`, exact
  ca butoanele din antetul arborelui, cu o cheie de «ce e în etichetă acum» ca fiecare pixel de
  mișcare peste același marcator să nu reprogrameze apariția.
- **Try/Catch** — `OnPaint`, mouse-ul și tastatura sunt frontiere de UI: **loghează și înghit**.
  Mutatoarele celor trei colecții sunt puncte de intrare: **loghează și RE-ARUNCĂ**. Ajutoarele
  ajunse doar prin frontiere nu poartă `Try` propriu (acoperire tranzitivă). Nu se loghează nimic
  din procesul designerului (`KBotDesignTime`).
- **Chei greșite** — validarea NU stă în colecție (dialogul inserează un element gol în clipa în
  care se apasă «Add»); stă în `EndInit` și în metodele de rulare. În designer validarea se sare
  și defectul se arată cu chenar roșu, ca la `KBotChipBar`.

---

## 3. Ce desenează, concret

- **Axa orizontală e timp REAL**, nu un index de sloturi: două instantanee la un minut distanță
  stau la un minut distanță. Contează exact aici — mai multe salvări ale aceleiași recepții pot
  cădea în aceeași zi, iar spațierea egală ar inventa un ritm pe care datele nu-l au.
- **Axa valorii se rotunjește la trepte întregi** (`NiceNumber`: 1/2/5/10 × o putere a lui zece),
  deci o axă de 0 / 3.271 / 6.542 devine 0 / 1.000 / 2.000 / 3.000 / 4.000 — a doua se citește
  fără aritmetică.
- **Un singur punct, sau mai multe în aceeași clipă**, dau o întindere zero. Nu e o eroare și nu
  se umflă: punctul se desenează în mijloc.
- **Etichetele de timp sunt doar cele două capete.** O axă de timp reală nu e regulată, deci
  etichete egal spațiate în interior ar numi momente în care nu s-a întâmplat nimic.
- **Seria accentuată se desenează ULTIMA** și mai gros, și tot ea câștigă la egalitate în
  vânătoarea de marcator: e linia de deasupra, deci e cea după care a întins operatorul mâna.

---

## 4. Legarea în `AsociereForm`

`splitDreapta.Panel1` era gol de când cele două liste au trecut amândouă în stânga. Acolo intră
`grafic`, andocat `Fill`, declarat în `.Designer.vb` cu tot cu cele două butoane ale benzii
(`receptie` / `angajament`, cu etichete în română și tooltip-uri multilinie).

- **Fila «Recepția»** — o singură linie: lanțul recepției alese, `Emphasis` + `FillArea`.
  Fără recepție aleasă, `EmptyText` spune ce să facă operatorul.
- **Fila «Tot angajamentul»** — câte o linie per recepție cu lanț nevid, plus **linia de total**,
  îngroșată. Recepția SELECTATĂ nu e cea accentuată aici, ci cea cu suprafața tentată: două linii
  accentuate ar face totalul doar încă un lanț, adică exact ce nu e.
- **Totalul**, un punct per moment distinct: fiecare recepție contribuie cu valoarea ultimului ei
  instantaneu de până atunci (o valoare ține până o schimbă următorul instantaneu). **O recepție
  al cărei ultim instantaneu e rândul de ȘTERGERE contribuie cu valoarea aceea LA acel moment și
  cu zero după** — rândul de ștergere spune cât valora când a plecat, nu cât valorează în
  continuare. Cu un singur lanț, totalul nu se mai desenează: ar fi aceeași linie de două ori.
- Graficul se reconstruiește **întreg**, din `Reconstruieste`, din același motiv pentru care
  arborii se reconstruiesc întregi: un grafic peticit punct cu punct și un arbore refăcut de la
  zero ajung, după câteva trageri, să spună lucruri diferite.
- Eticheta unui punct e **același text** ca tooltip-ul rândului din arbore — punctul și rândul
  sunt același fapt văzut de două ori.

---

## 5. Ce s-a VĂZUT (și ce nu)

Controlul a fost desenat cu `DrawToBitmap` pe **toate cele trei scheme** (Classic, Dark, Modern),
cu date reprezentative: două lanțuri de recepție (unul cu suprafață tentată) plus linia de total
accentuată, banda cu două butoane și unul curent, legenda, ambele axe.

**Verificat pe imagine:** banda desenează titlul la un capăt și butoanele la celălalt, butonul
curent e pe accent; treptele valorii ies rotunde (0 / 10.000 / 20.000 / 30.000); linia de total e
vizibil mai groasă; suprafața tentată nu îneacă liniile; pe Dark totul își schimbă culorile fără
să rămână vreo pată deschisă, deci nimic nu e scris în cod.

**NEVĂZUT:** `AsociereForm` însuși — nici pe ecran, nici în designerul Visual Studio. Deci **nu
sunt verificate**: cum arată graficul la dimensiunea reală a panoului, dus-întorsul prin dialogul
de colecție al designerului, liniile pe care le-ar rescrie VS în `AsociereForm.Designer.vb` și
chenarul roșu de cheie greșită pe suprafața de design.

---

## 6. Rezultatele testelor

- `dotnet build src/KBot.Controls` — **0 erori, 0 avertismente**.
- `dotnet build src/KBot.App` — **0 erori**; cele 6 avertismente sunt `MSB3825` (BinaryFormatter
  pe `.resx`-uri cu `ImageStream`), preexistente și pe alte cinci fișiere neatinse aici.
- `dotnet test tests/KBot.Controls.Tests` — **953 total: 952 trecute, 1 picată**.
  - **+16 teste noi** (`KBotChartViewTests.vb`), toate verzi: serializare zero pe un control
    proaspăt; `ApplyTheme` nu fixează nimic; axa se rotunjește; o serie plată tot primește o
    întindere; proiecția și ordinea în timp; punctul unic în mijloc; clicul pe bandă ridică o
    dată și nu ridică pentru butonul curent; `SelectedTabKey` tace, `SelectTab` vorbește; butonul
    dezactivat nu comută; cheile duplicate/goale se refuză (și la `EndInit`); colecțiile refuză
    `Nothing`; vânătoarea de marcator găsește cel mai apropiat și nimic departe de unul;
    accentuarea câștigă la egalitate; seria ascunsă nu e nici proiectată, nici vânată; pictează
    și cu date, și goală.
  - **Cea picată: `AdvancedTreePaddingsTests.Toate_marginile_sunt_proprietati_vizibile_in_categoria_Paddings`.**
    Așteaptă categoria `"K-BOT Tree - Paddings"`, primește `"K-BOT: Paddings"`. **Preexistentă și
    fără legătură cu felia asta:** amândouă fișierele (`AdvancedTreeControl.Paddings.vb` și testul)
    erau deja modificate în arborele de lucru la începutul sesiunii, dintr-o redenumire de
    categorii încă nedusă până la capăt. Nu am atins niciunul. **Rămâne deschisă.**

---

## 7. Fișiere atinse

**Adăugate**

- `src/KBot.Controls/Chart/KBotChartEnums.vb`
- `src/KBot.Controls/Chart/KBotChartPoint.vb`
- `src/KBot.Controls/Chart/KBotChartSeries.vb`
- `src/KBot.Controls/Chart/KBotChartTab.vb`
- `src/KBot.Controls/Chart/KBotChartView.vb`
- `src/KBot.Controls/Chart/KBotChartView.Painting.vb`
- `tests/KBot.Controls.Tests/KBotChartViewTests.vb`
- `docs/worklog/SLICE-0048-05-graficul-evolutiei.md` (fișierul ăsta)

**Modificate**

- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `grafic` în `splitDreapta.Panel1`, cele două
  butoane ale benzii, `Panel1.SuspendLayout`/`ResumeLayout`, `BeginInit`/`EndInit`, antetul de
  comentariu (trei zone → patru).
- `src/KBot.App/Forexe/AsociereForm.vb` — `ntfMesaj.Clear()` pe selecție; `_receptieSelectata` +
  `ReceptiaNodului`; secțiunea graficului (`Grafic_TabSelected`, `ReconstruiesteGrafic`,
  `ConstruiesteGraficReceptie`, `ConstruiesteGraficAngajament`, `ConstruiesteSeriaTotal`,
  `ValoareaLa`, `LantulReceptiei`, `CheiaSeriei`, `EtichetaReceptiei`, `AdaugaPunct`);
  `Reconstruieste` folosește acum `LantulReceptiei` în loc de interogarea repetată.
- `docs/worklog/KBOT_STATUS.md` — rândul 0048-05.

---

## 8. Nerulat / amânat

- **`AsociereForm` nedeschis**, nici pe ecran, nici în designerul VS (vezi §5). Prima verificare
  vizuală rămâne de făcut de operator.
- **`FileVersion` pentru `KBot.Controls` trece de la `1.35.0.0` la `1.37.0.0`** — familia
  `Chart/` e o adăugare de interfață. `AssemblyVersion` rămâne `1.0.0.0`.
- **Fără pagină de banc de probă**: graficul nu are încă echivalentul lui `TreePlaygroundForm` în
  `KBot.DevHarness`. N-a fost cerut și nu s-a inventat.
- `AdvancedTreePaddingsTests` rămâne roșu, din cauza străină descrisă în §6.

---

## 9. Pasul 05-01 — culorile: temă respectată și legătura grafic ▸ arbore

Operatorul a văzut felia pe ecran și a raportat trei lucruri (31.08.2026):

> 1. arborii din Asociere nu urmează culorile temei (doar corpul — antetul și subsolul sunt bune)
> 2. banda din interiorul controlului nou (graficul) nu urmează culorile temei (corpul da)
> 3. când am selectat în grafic doar H-ul curent, vreau ca fiecare nod al arborelui să fie
>    colorat cu culoarea punctului lui din grafic. Linia ia culoarea nodului din stânga. Dacă e
>    ales «tot angajamentul», atunci fiecare nod-rădăcină din arbore (asociat) primește culoarea
>    dată de punctele graficului. La fel și linia.

### 9.1 Corpul arborilor (1)

Cauza nu era în control, ci în fișierul de designer al formularului: `treeLant.BackColor =
Color.White` și `treeLibere.BackColor = Color.White`. Exact capcana descrisă în CLAUDE.md —
o culoare scrisă în designer NU e `Color.Empty`, deci `_backColorPinned` e True și `ApplyTheme`
o ocolește pentru totdeauna. Amândouă liniile s-au ȘTERS. Nu se mai întorc: `ShouldSerializeBackColor`
răspunde din steagul de fixare, iar acum nimeni nu mai scrie proprietatea.

De ce antetul și subsolul erau bune, deși `HeaderBackColor = SystemColors.Control` e tot acolo:
arborele are deja *excepția schemei întunecate* (`BandColorsFromThemeOnly`) — pe întuneric își
ignoră culorile de bandă din designer. Corpul n-avea așa ceva, fiindcă e `Control.BackColor`.
Liniile de antet s-au LĂSAT pe loc: sunt alegeri reale ale operatorului și excepția le ține deja
în frâu acolo unde ar deranja.

### 9.2 Banda graficului (2)

Două cauze, amândouă rezolvate:

- Designerul scrisese `grafic.HeaderBackColor = SystemColors.Control` și
  `grafic.HeaderSeparatorColor = SystemColors.ActiveBorder` — șterse, ca la arbori.
- Controlul n-avea excepția schemei întunecate. Are acum: `_isDarkScheme` +
  `EffectiveHeaderBackColor` / `EffectiveHeaderTextColor` / `EffectiveHeaderSeparatorColor`,
  copiate ca regulă (nu ca text) după `AdvancedTreeControl.Theming`. Ce supraviețuiește pe
  întuneric e FORMA benzii (`HeaderGradient`), nu culoarea ei.

Butoanele n-au avut nevoie de nimic: își luau deja fondul și textul din paletă
(`AccentColor` / `ButtonBackColor` / `ButtonHoverColor` / `DisabledTextColor`).

### 9.3 Culoare comună grafic ▸ arbore (3)

**În control** — `KBotChartPoint.PointColor` (`Color.Empty` = culoarea seriei), cu perechea
`ShouldSerialize`/`Reset`. Numele NU e `Color`: VB e insensibil la majuscule și un membru umbrește
un tip omonim, deci `Color.Empty` scris în clasă ar fi ajuns să însemne proprietatea. `DrawOneSeries`
desenează acum segment cu segment: **segmentul ia culoarea punctului din STÂNGA lui** — cel din care
pleacă, nu cel în care ajunge, fiindcă porțiunea de după un instantaneu e cât a valorat instantaneul
acela până la următorul. Zona colorată de sub linie se taie la fel, o fâșie pe segment. Inelul de
survolare urmează și el culoarea punctului. Când niciun punct nu-și numește culoarea, se desenează
exact ce desena un singur `DrawLines`.

`SeriesColor` s-a rupt în două, iar jumătatea care alege din paletă a devenit **publică**:
`KBotChartView.AutoColor(index)`. Asta e cheia: gazda care colorează ceva din AFARA graficului
trebuie să poată cere ACELEAȘI culori, nu să inventeze un set paralel care se rupe la prima
schimbare de temă.

**În formular** — două dicționare (`_nodReceptie`, `_nodInstantaneu`) umplute în timp ce se
construiesc arborii, și o singură trecere `SincronizeazaCulorile()`, chemată din `Finally`-ul lui
`ReconstruiesteGrafic` — adică DUPĂ grafic, mereu, și nicăieri altundeva.

- **Recepția**: fiecare punct primește `CuloarePunct(inst, i)`, iar rândul lui de instantaneu ia
  aceeași valoare.
- **Tot angajamentul**: fiecare serie primește `AutoColor(index)` scris explicit (o culoare pe care
  n-o știe nimeni e o culoare despre care arborele nu poate fi anunțat), iar rândul-RĂDĂCINĂ al
  recepției o ia. Frunzele revin la text simplu — a le colora și pe ele ar pretinde o distincție
  pe care graficul n-o desenează.
- Se resetează TOT înainte de a se picta, nu se peticește: cele două vederi colorează rânduri
  diferite, deci orice comutare lasă rânduri în urmă.
- Un instantaneu **blocat sau «fără schimbare»** păstrează griul lui și pe grafic, și în arbore
  (`CuloareDeBaza`, desprins din `ColoreazaInstantaneu`). Corespondența rămâne exactă, iar un
  rând scos din joc nu devine cel mai colorat lucru de pe ecran.
- **Linia de total** ia `Palette.TextColor`, DINADINS nu o culoare din `AutoColor`: setul acela
  aparține recepțiilor, iar totalul nu e încă o recepție.
- `AsociereForm.OnThemeChanged` reconstruiește graficul. Culorile copiate din paletă în serii,
  puncte și rânduri sunt VALORI, nu legături — nimeni nu se întoarce să le corecteze.

### 9.4 Ce nu s-a atins

- `grid.BackColor = SystemColors.Window` are **exact același defect**, dar `KBotDataView` n-are
  `ShouldSerializeBackColor` cu steag de fixare, deci ștergerea liniei ar fi fost rescrisă la
  prima salvare din designer. E o reparație de CONTROL, în altă felie.
- Fonturile fixate în designer (`Calibri 9` pe arbori și pe grafic) s-au lăsat pe loc: raportul era
  despre culori, iar antetele bold sunt vizibil o alegere.
- Fără teste noi și fără randare — operatorul a cerut explicit să rămână amândouă la el.

**Verificat**: `KBot.Controls` și `KBot.App` compilează cu 0 erori;
`KBot.Controls.Tests` 952/953, singurul roșu fiind cel străin din §6.

---

## 10. Ce a urmat

Pașii 05-02 (setul de culori: gri scos din grafic, nuanțe mult mai depărtate, fără roșu) și 05-03
(clic pe un punct ▸ selectarea rândului din arbore) au plecat de aici într-o felie a lor:
`SLICE-0048-06-culorile-graficului-si-clic-pe-punct.md`.
