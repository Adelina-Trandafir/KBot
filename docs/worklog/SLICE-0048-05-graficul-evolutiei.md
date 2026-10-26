# SLICE-0048-05…09 — Graficul și benzile editorului de asociere

Consolidare editorială D001-04, 24.10.2026. Identificatorii și probele istorice rămân distincte; starea din registru nu devine validare live. API-urile sunt în documentele controalelor; aici rămân integrarea, motivele, evoluția și probele. Secțiunile numerotate din fiecare intervenție păstrează contextul documentului original.

<a id="slice-0048-05"></a>

## SLICE-0048-05 — mesajul care nu se stingea + graficul evoluției (`KBotChartView`)

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

**Stare:** cod verde — `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` build 0 erori (doar avertismentele `MSB3825` preexistente pe `.resx`-uri). `KBot.Controls.Tests` **952 trecute / 1 picată**, iar cea picată e **preexistentă și străină de felia asta** (vezi §6). **Formularul NU a fost deschis nici pe ecran, nici în designerul Visual Studio.** Controlul, în schimb, **a fost desenat și privit** — vezi §5.

---

## 1. Mesajul care nu se stingea

`ntfMesaj` e o casetă de notificare tranzitorie: o pune `ReincarcaAsync` (nu există instantanee / n-am putut citi), o pune `AratMeniul` (legătura e blocată, cu motivele), o pune `AplicaComandaDeMeniu` (comanda a eșuat) și o pune `btnSalveaza_Click` (rezultatul salvării). Nimeni nu o stingea în afară de `ReincarcaAsync`.

Efectul pe ecran: refuzul unui instantaneu rămânea sub cele două liste în timp ce operatorul lucra pe următorul, și **se citea ca și cum ar fi fost despre acela**.

Reparația e într-un singur loc, `Tree_NodeMouseUp`, și e prima instrucțiune din el:

```vb
ntfMesaj.Clear()
```

Se stinge **înainte** de orice altceva, deci `AratMeniul` (clic-dreapta pe o legătură blocată) poate pune imediat mesajul lui la loc — ordinea contează, iar asta e ordinea care o dă.

Alegerea unui alt rând înseamnă că operatorul a trecut mai departe, deci se sting **toate** felurile de mesaj, nu doar erorile — inclusiv «s-a salvat» și «angajamentul nu are niciun instantaneu». Un mesaj de succes care supraviețuiește următorului clic e la fel de mincinos ca unul de eroare.

---

## 2. Controlul nou — contract canonic și particularitățile livrării

API-ul, modelele, enumurile, colecțiile, proprietățile IDE și limitele sunt în [KBotChartView](../../src/KBot.Controls/Chart/KBotChartView.md); regulile comune de temă, DPI, serializare, erori, limbă și tooltip sunt în [CONTROLS C1–C9](../../src/KBot.Controls/CONTROLS.md). Familia inițială: KBotChartEnums/Point/Series/Tab/View/View.Painting.vb, cu colecțiile Point/Series/Tab, în Chart/. Toate categoriile și explicațiile IDE sunt în engleză; gazda furnizează textele românești.

Banda desenată în interior este single-select, distinctă de ChipBar; butoanele reale ar fi cerut temă/DPI/serializare proprii. Gazda interpretează TabSelected(cheia): atribuirea SelectedTabKey tace, SelectTab emite. Categoriile sunt Chart Appearance/Axes/Data/Header/Legend/Plot/Tabs/Tooltip, plus Point/Series/Tab. Colecțiile, banda/gradientul, alinierea/raza/spațierea butoanelor, liniile/marcatorii/suprafața, axele/grila/treptele/formatele, legenda/EmptyText și PointTooltip.Style sunt configurabile în IDE.

Particularități istorice: ApplyTheme scrie BackColor/ForeColor prin MyBase și reface resursele; culorile Empty urmau inițial accent/success/warning/error/tab-accent, apoi trepte deschise (set înlocuit în 0048-06). ShouldSerialize/Reset acoperă BackColor/ForeColor/Font cu steaguri de fixare, HeaderFont/AxisFont/TabIconSize și nouă culori; DefaultSize evită serializarea Size. Zero proprietăți suplimentare la un control nou au fost măsurate prin TypeDescriptor. Handle/DPI/font/size invalidează layoutul; scalarea rămâne ThemeShapes.ScaleDpi, DeviceDpi/96, fără rescrierea valorilor logice.

Regiunile pictate folosesc KBotToolTip.ShowAt cu cheia conținutului, fără reprogramare la fiecare pixel. Frontierele OnPaint/mouse/tastatură loghează și tratează eroarea, mutatorii colecțiilor loghează și re-aruncă, helperii au acoperire tranzitivă, fără log în designer. Cheile se validează la EndInit/rulare, cu chenar roșu în designer: dialogul colecției trebuie să poată introduce întâi un element gol.

## 3. Ce desenează, concret

- **Axa orizontală e timp REAL**, nu un index de sloturi: două instantanee la un minut distanță stau la un minut distanță. Contează exact aici — mai multe salvări ale aceleiași recepții pot cădea în aceeași zi, iar spațierea egală ar inventa un ritm pe care datele nu-l au.
- **Axa valorii se rotunjește la trepte întregi** (`NiceNumber`: 1/2/5/10 × o putere a lui zece), deci o axă de 0 / 3.271 / 6.542 devine 0 / 1.000 / 2.000 / 3.000 / 4.000 — a doua se citește fără aritmetică.
- **Un singur punct, sau mai multe în aceeași clipă**, dau o întindere zero. Nu e o eroare și nu se umflă: punctul se desenează în mijloc.
- **Etichetele de timp sunt doar cele două capete.** O axă de timp reală nu e regulată, deci etichete egal spațiate în interior ar numi momente în care nu s-a întâmplat nimic.
- **Seria accentuată se desenează ULTIMA** și mai gros, și tot ea câștigă la egalitate în vânătoarea de marcator: e linia de deasupra, deci e cea după care a întins operatorul mâna.

---

## 4. Legarea în `AsociereForm`

`splitDreapta.Panel1` era gol de când cele două liste au trecut amândouă în stânga. Acolo intră `grafic`, andocat `Fill`, declarat în `.Designer.vb` cu tot cu cele două butoane ale benzii (`receptie` / `angajament`, cu etichete în română și tooltip-uri multilinie).

- **Fila «Recepția»** — o singură linie: lanțul recepției alese, `Emphasis` + `FillArea`. Fără recepție aleasă, `EmptyText` spune ce să facă operatorul.
- **Fila «Tot angajamentul»** — câte o linie per recepție cu lanț nevid, plus **linia de total**, îngroșată. Recepția SELECTATĂ nu e cea accentuată aici, ci cea cu suprafața tentată: două linii accentuate ar face totalul doar încă un lanț, adică exact ce nu e.
- **Totalul**, un punct per moment distinct: fiecare recepție contribuie cu valoarea ultimului ei instantaneu de până atunci (o valoare ține până o schimbă următorul instantaneu). **O recepție al cărei ultim instantaneu e rândul de ȘTERGERE contribuie cu valoarea aceea LA acel moment și cu zero după** — rândul de ștergere spune cât valora când a plecat, nu cât valorează în continuare. Cu un singur lanț, totalul nu se mai desenează: ar fi aceeași linie de două ori.
- Graficul se reconstruiește **întreg**, din `Reconstruieste`, din același motiv pentru care arborii se reconstruiesc întregi: un grafic peticit punct cu punct și un arbore refăcut de la zero ajung, după câteva trageri, să spună lucruri diferite.
- Eticheta unui punct e **același text** ca tooltip-ul rândului din arbore — punctul și rândul sunt același fapt văzut de două ori.

---

## 5. Ce s-a VĂZUT (și ce nu)

Controlul a fost desenat cu `DrawToBitmap` pe **toate cele trei scheme** (Classic, Dark, Modern), cu date reprezentative: două lanțuri de recepție (unul cu suprafață tentată) plus linia de total accentuată, banda cu două butoane și unul curent, legenda, ambele axe.

**Verificat pe imagine:** banda desenează titlul la un capăt și butoanele la celălalt, butonul curent e pe accent; treptele valorii ies rotunde (0 / 10.000 / 20.000 / 30.000); linia de total e vizibil mai groasă; suprafața tentată nu îneacă liniile; pe Dark totul își schimbă culorile fără să rămână vreo pată deschisă, deci nimic nu e scris în cod.

**NEVĂZUT:** `AsociereForm` însuși — nici pe ecran, nici în designerul Visual Studio. Deci **nu sunt verificate**: cum arată graficul la dimensiunea reală a panoului, dus-întorsul prin dialogul de colecție al designerului, liniile pe care le-ar rescrie VS în `AsociereForm.Designer.vb` și chenarul roșu de cheie greșită pe suprafața de design.

---

## 6. Rezultatele testelor

- `dotnet build src/KBot.Controls` — **0 erori, 0 avertismente**.
- `dotnet build src/KBot.App` — **0 erori**; cele 6 avertismente sunt `MSB3825` (BinaryFormatter pe `.resx`-uri cu `ImageStream`), preexistente și pe alte cinci fișiere neatinse aici.
- `dotnet test tests/KBot.Controls.Tests` — **953 total: 952 trecute, 1 picată**.
  - **+16 teste noi** (`KBotChartViewTests.vb`), toate verzi: serializare zero pe un control proaspăt; `ApplyTheme` nu fixează nimic; axa se rotunjește; o serie plată tot primește o întindere; proiecția și ordinea în timp; punctul unic în mijloc; clicul pe bandă ridică o dată și nu ridică pentru butonul curent; `SelectedTabKey` tace, `SelectTab` vorbește; butonul dezactivat nu comută; cheile duplicate/goale se refuză (și la `EndInit`); colecțiile refuză `Nothing`; vânătoarea de marcator găsește cel mai apropiat și nimic departe de unul; accentuarea câștigă la egalitate; seria ascunsă nu e nici proiectată, nici vânată; pictează și cu date, și goală.
  - **Cea picată: `AdvancedTreePaddingsTests.Toate_marginile_sunt_proprietati_vizibile_in_categoria_Paddings`.** Așteaptă categoria `"K-BOT Tree - Paddings"`, primește `"K-BOT: Paddings"`. **Preexistentă și fără legătură cu felia asta:** amândouă fișierele (`AdvancedTreeControl.Paddings.vb` și testul) erau deja modificate în arborele de lucru la începutul sesiunii, dintr-o redenumire de categorii încă nedusă până la capăt. Nu am atins niciunul. **Rămâne deschisă.**

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

- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `grafic` în `splitDreapta.Panel1`, cele două butoane ale benzii, `Panel1.SuspendLayout`/`ResumeLayout`, `BeginInit`/`EndInit`, antetul de comentariu (trei zone → patru).
- `src/KBot.App/Forexe/AsociereForm.vb` — `ntfMesaj.Clear()` pe selecție; `_receptieSelectata` + `ReceptiaNodului`; secțiunea graficului (`Grafic_TabSelected`, `ReconstruiesteGrafic`, `ConstruiesteGraficReceptie`, `ConstruiesteGraficAngajament`, `ConstruiesteSeriaTotal`, `ValoareaLa`, `LantulReceptiei`, `CheiaSeriei`, `EtichetaReceptiei`, `AdaugaPunct`); `Reconstruieste` folosește acum `LantulReceptiei` în loc de interogarea repetată.
- `docs/worklog/KBOT_STATUS.md` — rândul 0048-05.

---

## 8. Nerulat / amânat

- **`AsociereForm` nedeschis**, nici pe ecran, nici în designerul VS (vezi §5). Prima verificare vizuală rămâne de făcut de operator.
- **`FileVersion` pentru `KBot.Controls` trece de la `1.35.0.0` la `1.37.0.0`** — familia `Chart/` e o adăugare de interfață. `AssemblyVersion` rămâne `1.0.0.0`.
- **Fără pagină de banc de probă**: graficul nu are încă echivalentul lui `TreePlaygroundForm` în `KBot.DevHarness`. N-a fost cerut și nu s-a inventat.
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

Cauza nu era în control, ci în fișierul de designer al formularului: `treeLant.BackColor = Color.White` și `treeLibere.BackColor = Color.White`. Exact capcana descrisă în CLAUDE.md — o culoare scrisă în designer NU e `Color.Empty`, deci `_backColorPinned` e True și `ApplyTheme` o ocolește pentru totdeauna. Amândouă liniile s-au ȘTERS. Nu se mai întorc: `ShouldSerializeBackColor` răspunde din steagul de fixare, iar acum nimeni nu mai scrie proprietatea.

De ce antetul și subsolul erau bune, deși `HeaderBackColor = SystemColors.Control` e tot acolo: arborele are deja *excepția schemei întunecate* (`BandColorsFromThemeOnly`) — pe întuneric își ignoră culorile de bandă din designer. Corpul n-avea așa ceva, fiindcă e `Control.BackColor`. Liniile de antet s-au LĂSAT pe loc: sunt alegeri reale ale operatorului și excepția le ține deja în frâu acolo unde ar deranja.

### 9.2 Banda graficului (2)

Două cauze, amândouă rezolvate:

- Designerul scrisese `grafic.HeaderBackColor = SystemColors.Control` și `grafic.HeaderSeparatorColor = SystemColors.ActiveBorder` — șterse, ca la arbori.
- Controlul n-avea excepția schemei întunecate. Are acum: `_isDarkScheme` + `EffectiveHeaderBackColor` / `EffectiveHeaderTextColor` / `EffectiveHeaderSeparatorColor`, copiate ca regulă (nu ca text) după `AdvancedTreeControl.Theming`. Ce supraviețuiește pe întuneric e FORMA benzii (`HeaderGradient`), nu culoarea ei.

Butoanele n-au avut nevoie de nimic: își luau deja fondul și textul din paletă (`AccentColor` / `ButtonBackColor` / `ButtonHoverColor` / `DisabledTextColor`).

### 9.3 Culoare comună grafic ▸ arbore (3)

**În control** — `KBotChartPoint.PointColor` (`Color.Empty` = culoarea seriei), cu perechea `ShouldSerialize`/`Reset`. Numele NU e `Color`: VB e insensibil la majuscule și un membru umbrește un tip omonim, deci `Color.Empty` scris în clasă ar fi ajuns să însemne proprietatea. `DrawOneSeries` desenează acum segment cu segment: **segmentul ia culoarea punctului din STÂNGA lui** — cel din care pleacă, nu cel în care ajunge, fiindcă porțiunea de după un instantaneu e cât a valorat instantaneul acela până la următorul. Zona colorată de sub linie se taie la fel, o fâșie pe segment. Inelul de survolare urmează și el culoarea punctului. Când niciun punct nu-și numește culoarea, se desenează exact ce desena un singur `DrawLines`.

`SeriesColor` s-a rupt în două, iar jumătatea care alege din paletă a devenit **publică**: `KBotChartView.AutoColor(index)`. Asta e cheia: gazda care colorează ceva din AFARA graficului trebuie să poată cere ACELEAȘI culori, nu să inventeze un set paralel care se rupe la prima schimbare de temă.

**În formular** — două dicționare (`_nodReceptie`, `_nodInstantaneu`) umplute în timp ce se construiesc arborii, și o singură trecere `SincronizeazaCulorile()`, chemată din `Finally`-ul lui `ReconstruiesteGrafic` — adică DUPĂ grafic, mereu, și nicăieri altundeva.

- **Recepția**: fiecare punct primește `CuloarePunct(inst, i)`, iar rândul lui de instantaneu ia aceeași valoare.
- **Tot angajamentul**: fiecare serie primește `AutoColor(index)` scris explicit (o culoare pe care n-o știe nimeni e o culoare despre care arborele nu poate fi anunțat), iar rândul-RĂDĂCINĂ al recepției o ia. Frunzele revin la text simplu — a le colora și pe ele ar pretinde o distincție pe care graficul n-o desenează.
- Se resetează TOT înainte de a se picta, nu se peticește: cele două vederi colorează rânduri diferite, deci orice comutare lasă rânduri în urmă.
- Un instantaneu **blocat sau «fără schimbare»** păstrează griul lui și pe grafic, și în arbore (`CuloareDeBaza`, desprins din `ColoreazaInstantaneu`). Corespondența rămâne exactă, iar un rând scos din joc nu devine cel mai colorat lucru de pe ecran.
- **Linia de total** ia `Palette.TextColor`, DINADINS nu o culoare din `AutoColor`: setul acela aparține recepțiilor, iar totalul nu e încă o recepție.
- `AsociereForm.OnThemeChanged` reconstruiește graficul. Culorile copiate din paletă în serii, puncte și rânduri sunt VALORI, nu legături — nimeni nu se întoarce să le corecteze.

### 9.4 Ce nu s-a atins

- `grid.BackColor = SystemColors.Window` are **exact același defect**, dar `KBotDataView` n-are `ShouldSerializeBackColor` cu steag de fixare, deci ștergerea liniei ar fi fost rescrisă la prima salvare din designer. E o reparație de CONTROL, în altă felie.
- Fonturile fixate în designer (`Calibri 9` pe arbori și pe grafic) s-au lăsat pe loc: raportul era despre culori, iar antetele bold sunt vizibil o alegere.
- Fără teste noi și fără randare — operatorul a cerut explicit să rămână amândouă la el.

**Verificat**: `KBot.Controls` și `KBot.App` compilează cu 0 erori; `KBot.Controls.Tests` 952/953, singurul roșu fiind cel străin din §6.

---

## 10. Ce a urmat

Pașii 05-02 (setul de culori: gri scos din grafic, nuanțe mult mai depărtate, fără roșu) și 05-03 (clic pe un punct ▸ selectarea rândului din arbore) au plecat de aici într-o felie a lor: [0048-06](#slice-0048-06), absorbit în acest jurnal prin D001-04.

<a id="slice-0048-06"></a>

## SLICE-0048-06 — setul de culori al graficului + clic pe punct ▸ rândul din arbore

Continuarea directă a feliei `SLICE-0048-05` (graficul `KBotChartView` din `AsociereForm`), după ce operatorul l-a văzut pe ecran. Două cereri, în două runde: mai întâi **culorile**, apoi **legarea clicului de arbore**. Codul graficului stă tot în `src/KBot.Controls/Chart/`, deci tot în engleză, inclusiv numele categoriilor din grila de proprietăți.

**Stare:** cod verde — `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori (doar cele 6 `MSB3825` vechi de pe `.resx`), soluția întreagă 0 erori. `KBot.Controls.Tests` **953 trecute / 0 picate** — roșul preexistent și străin de felia asta a fost închis aici, cu o singură linie (vezi §5). **Fără teste noi și fără randare** — operatorul a spus explicit că testarea și randarea sunt treaba lui.

---

## 1. Pasul 06-01 — setul de culori

Operatorul a cerut eliminarea griului din grafic, distincție mai mare între multe recepții și folosirea roșului numai pentru erori. CuloarePunct a fost eliminată: punctele iau direct grafic.AutoColor(i), inclusiv la instantanee blocate/fără schimbare; lacătul și textul păstrează starea. CuloareDeBaza rămâne pentru arborele liber și frunzele din vederea întregului angajament, unde graficul nu exprimă o distincție de instantaneu.

AutoColor este public pentru aceeași culoare în grafic și gazdă. Setul vechi de accente repetate mai deschis a fost înlocuit cu nuanțe la pasul fracției de aur 0,618 și luminozitate în trei trepte. Pornirea/saturația/luminozitatea vin din accentul activ; indicele 0 este accentul în intervalul permis, iar pe Dark întregul set este ridicat. Limitele numerice sunt limite de lizibilitate, nu culori fixe. FromHsl/Channel sunt conversia privată în același spațiu ca GetHue/GetSaturation/GetBrightness; KBot.Theming nu a fost extins, neavând alt client.

HueFirst=30° și HueLast=330° exclud pana roșie indiferent de temă; ErrorColor a ieșit din set. Roșul rămâne posibil numai când gazda îl atribuie explicit prin LineColor/PointColor pentru o problemă. Primele 14 intrări, calculate pentru un accent albastru, fără randare: #2273C3 #4E981B #A93CDD #22C39E #98861B #3C44DD #22C329 #981B87 #3CB9DD #91C322 #4D1B98 #3CDD8B #C38022 #1B4498; cea mai caldă este portocaliu. Contractul ulterior și pragul de contrast adăugat în 0048-08 sunt în [Chart](../../src/KBot.Controls/Chart/KBotChartView.md).

## 2. Pasul 06-02 — clic pe punct ▸ rândul din arbore

> când utilizatorul dă clic pe un nod din grafic, nodul corespunzător din arbore trebuie
> selectat. ține referința nodurilor în proprietatea tag (părere, poți face cum vrei)

Cealaltă jumătate a colorării. Punctul și rândul au deja aceeași culoare, deci operatorul le împerechează din ochi — dar pe un lanț lung rândul pe care tocmai l-a găsit în grafic poate fi ieșit din vizor sau sub o recepție strânsă. Selectarea e ce transformă «văd care e» în «pot lucra pe el».

### 2.1 Ce era deja acolo

`PointClicked(seriesKey, pointIndex)` exista din felia 0048-05; `FindSeries(key)` la fel. Iar `punct.Tag` ține instantaneul de când se construiește punctul (`AdaugaPunct`), deci sugestia operatorului era deja pusă în practică — nu se re-deduce nimic din etichetă și nu se caută după un moment pe care două instantanee îl pot împărți.

### 2.2 `AdvancedTreeControl.SelectAndReveal(node)` — nou, public

`SelectedNode = nod` NU e de ajuns, și ăsta e tot rostul metodei: un nod aflat sub un părinte strâns nu e deloc în lista vizibilă, deci selecția e reală dar invizibilă — operatorul vede un clic care n-a făcut nimic. Metoda desface toți ascendenții, cheamă `RefreshScrollVisibility()` (desfacerea a schimbat înălțimea conținutului, iar `EnsureNodeVisible` măsoară față de bară, deci intervalul trebuie corect ÎNAINTE să fie întrebat) și abia apoi derulează minimul necesar.

**Nu ia focusul, dinadins.** Operatorul a dat clic în altă parte; tras aici, cursorul ar face ca următoarea săgeată să miște un arbore la care nu se uita. (`KBotChartView.OnMouseDown` cheamă `Focus()` pe el însuși, deci focusul rămâne unde a dat operatorul clic — pe grafic.)

Un nod care nu e din arborele ăsta e no-op, nu eroare: apelantul potrivește două liste, iar «aici nu e» e un răspuns obișnuit.

Metoda a intrat în `AdvancedTreeControl.API.vb`, nu în formular, fiindcă orice apelant din AFARA arborelui — un punct pe un grafic, un rezultat de căutare, un rând dintr-o listă alăturată — vrea exact aceleași două jumătăți, iar fiecare scriind aceleași trei linii e felul în care jumătățile ajung să se despartă.

### 2.3 Ce s-a schimbat în formular

`_nodInstantaneu` ținea `TreeItem`. Un `TreeItem` nu-și cunoaște controlul, iar instantaneele sunt împărțite pe DOI arbori — legatele în stânga, cele libere în dreapta. Selectarea cere amândouă jumătățile, deci dicționarul ține acum `RandDeArbore` (arbore + nod), singura variantă care nu poate ajunge să ceară unui arbore să selecteze un rând al celuilalt. Recepțiile n-au nevoie de așa ceva: o rădăcină există doar în arborele din stânga, deci `_nodReceptie` rămâne pe `TreeItem`.

`Grafic_PointClicked` citește instantaneul din `Tag`, îl caută în dicționar și cheamă `SelectAndReveal`. Un punct FĂRĂ instantaneu e linia de total — o însumare a mai multor recepții, deci nu există un singur rând în spate și clicul e lăsat dinadins să nu facă nimic.

Nu se reconstruiește nimic aici: selectarea unui rând nu e o schimbare a tabloului, iar reconstruirea graficului din chiar clicul graficului ar fi o cale bună de inventat o buclă.

---

## 3. Fișiere atinse

**Adăugate**

- `docs/worklog/SLICE-0048-06-culorile-graficului-si-clic-pe-punct.md` (fișierul ăsta)

**Modificate**

- `src/KBot.Controls/Chart/KBotChartView.Painting.vb` — `SeriesColor` simplificat; `AutoColor` rescris (public, nuanță pe fracția de aur între `HueFirst`/`HueLast`, ancorat în accentul schemei active, luminozitate în trei trepte, ridicat pe schemele întunecate); ajutoarele private noi `FromHsl` și `Channel`.
- `src/KBot.Controls/Tree/AdvancedTreeControl.API.vb` — metoda publică nouă `SelectAndReveal`.
- `src/KBot.App/Forexe/AsociereForm.vb` — `CuloarePunct` ȘTEARSĂ, punctele iau `grafic.AutoColor(i)`; `_nodInstantaneu` retipat pe `RandDeArbore`; `SincronizeazaCulorile` citește prin el; `Grafic_PointClicked` nou; clasa cuibărită `RandDeArbore` nouă.
- `docs/worklog/SLICE-0048-05-graficul-evolutiei.md` — pașii ăștia doi erau descriși acolo ca §10 și §11; au fost scoși și mutați aici, cu o trimitere lăsată în locul lor.
- `docs/worklog/KBOT_STATUS.md` — rândul 0048-06.

---

## 4. Ce NU s-a atins, dinadins

- **`KBot.Theming` n-a fost atins deloc** — `FromHsl` a rămas privat în grafic (vezi §1.2), deci `FileVersion`-ul proiectului de teme nu se mișcă.
- **`CuloareDeBaza` a rămas** — o folosesc mai departe `ColoreazaInstantaneu` și resetul din `SincronizeazaCulorile`. A dispărut doar `CuloarePunct`, cea care aducea gri-ul ÎN grafic.

---

## 5. Rezultatele testelor

| Comandă | Rezultat |
|---|---|
| `dotnet build src\KBot.Controls\KBot.Controls.vbproj` | 0 erori, 0 avertismente |
| `dotnet build src\KBot.App\KBot.App.vbproj` | 0 erori (6 `MSB3825` preexistente pe `.resx`) |
| `dotnet build KBot.sln` | 0 erori |
| `dotnet test tests\KBot.Controls.Tests` | **953 trecute / 0 picate**, 0 regresii |

Roșul care a însoțit felia 0048-05 de la un capăt la altul — `AdvancedTreePaddingsTests.Toate_marginile_sunt_proprietati_vizibile_in_categoria_Paddings`, aștepta `"K-BOT Tree - Paddings"` și primea `"K-BOT: Paddings"` — **s-a închis la livrare**. Era o redenumire de categorii lăsată pe jumătate în arborele de lucru: codul arborelui folosește `K-BOT: <grup>` peste tot (Footer, Header, Search, Buttons, Colors, Columns, Tooltip, DND, Nodes), deci testul era singurul rămas pe numele vechi. Reparația e ACEA singură linie, în test — codul nu s-a atins, fiindcă el e deja consecvent.

Setul de culori a fost verificat **prin calcul** (o simulare a aritmeticii din `AutoColor`, fără să se instanțieze controlul și fără să se deseneze nimic) — de acolo vin cele 14 intrări din §1.3.

---

## 6. Nerulat / amânat

- **`AsociereForm` tot NEVĂZUT** — nici pe ecran, nici în designerul VS. Operatorul a cerut explicit să nu se randeze nimic: testarea și randarea sunt ale lui. Prima verificare vizuală a culorilor și a clicului rămâne de făcut de el.
- **Fără teste noi**, din același motiv.
- `grid.BackColor = SystemColors.Window` din `AsociereForm.Designer.vb` — același defect de «proprietate fixată» ca la arbori, dar repararea cere întâi `ShouldSerializeBackColor` + un fanion de fixare pe `KBotDataView`, altfel linia ștearsă se rescrie la următoarea salvare din designer. Semnalat, necerut, neatins.
- Fonturile `Calibri 9` fixate din designer pe arbori și pe grafic — lăsate așa.
- **Livrarea a luat cu ea și lucrul din arborele de lucru pe `AdvancedTreeControl`** (chei de pictogramă pe bandă, `TreeImageKeyConverter`, redenumirea categoriilor, `TooltipShowOnlyOnRightIcon`, exportatorul din `KBot.DevHarness`): nu e felia asta, dar `AsociereForm.Designer.vb` îl FOLOSEȘTE (`TooltipShowOnlyOnRightIcon` nu există fără el), deci un commit care l-ar fi lăsat afară n-ar fi compilat. A plecat într-un commit al lui, ÎNAINTEA acestuia, și **nu are worklog** — nu s-a putut stabili cărei felii îi aparține.

<a id="slice-0048-07"></a>

## SLICE-0048-07 — benzile de așezare, liniile în trepte, reperele plăților și retragerea lui F13

Continuarea directă a feliilor `SLICE-0048-04` … `SLICE-0048-06` (editorul de legături R ▸ H și graficul lui). Plan: `docs/PLAN_AsociereBenzi.md`, scris de operator pe 31.08.2026.

**Stare:** cod verde — soluția întreagă build **0 erori**; `KBot.Controls` **0 erori / 0 avertismente**. Singurele avertismente pe soluție sunt cele **6 `MSB3825` preexistente** de pe `AsociereForm.resx` (`Il_Receptii.ImageStream` prin `BinaryFormatter`), care apar doar când se regenerează `.resx`-ul — aceleași pe care le consemna și worklogul feliei 0048-06. `KBot.Controls.Tests` **953 trecute / 0 picate**, exact linia de plecare. `KBot.App.Tests` **13 picate / 185 trecute**, exact linia de plecare (vezi §7). Python **465 trecute / 15 sărite**.

**NIMIC NU S-A VĂZUT PE ECRAN.** `AsociereForm` nu a fost niciodată randat, iar `KBotLaneView` și `AsociereBenziForm` sunt scrise în felia asta și nu au fost deschise nici măcar în designerul Visual Studio. Testarea și verificarea vizuală sunt ale operatorului, pe mașina lui, după publicare.

**Nimic nu a rulat pe MariaDB.** Fără migrare, fără schimbare de schemă, fără apel viu.

**Fără git.** Nimic nu s-a comis și nimic nu s-a împins — cerut explicit în plan. Worklogul rămâne în arborele de lucru.

---

## 0. Ce s-a citit înainte de orice

`CODE_WORKFLOW.md` · `KBOT_STATUS.md` (toată familia 0048) · `docs/FUNDAMENT_Asociere_Receptii.md` (părțile 1, 2, 4 integral) · worklogurile `0048-04`, `0048-05`, `0048-06` · `src/KBot.App/Forexe/AsociereForm.{vb,Designer.vb}` · toată familia `src/KBot.Controls/Chart/` + `KBotChartView.md` · `src/KBot.Controls/Tree/AdvancedTreeControl.Drag.vb` · `src/KBot.Controls/CONTROLS.md` (C1..C9) · `docs/kbot-forms-ui-convention.md` · `PYTHON/routes/forexe/asociere.py` și `prelucrare_asociere.py` · `src/KBot.Domain/AsociereStare.vb` · `src/KBot.Api/ApiClient.vb` (partea de asociere) · `src/KBot.Theming/ThemeShapes.vb` și `ThemePalette.vb`.

---

## 1. De ce există felia — premisa care s-a rupt

Operatorul a corectat pe 31.08.2026 un fapt pe care se sprijinea tot formularul:

> `FX_Receptii_R.DataR` **nu** e momentul creării. E un câmp obișnuit, pe care omul îl tastează pe
> site, și se poate schimba după creare.

Verificat în schemă: `FX_Receptii_R` e `IDRR, NRCRT, CodAngajament, Tip, DataR, SumaAntet, Descriere, HASH, TipReceptie, Incarcat, Preluat` (`MariaDB_Schema/000_DEMO.sql` și `FX_System_Export/TABLES/FX_Receptii_R.md`). **Nu există nicio coloană cu momentul creării în tabelul ăla.** Rândurile din eșantion poartă `dd.MM.yyyy` fără componentă de oră — adică exact cum arată o dată tastată.

Deci **F13 se sprijinea pe nimic** și a ieșit. Iar fiindcă F13 era cea mai tare constrângere a formularului, ce a rămas — F14, F16, o singură ștergere per lanț, F15 ca semn — nu mai poate duce designul. Consecința nu e o funcție mai mică, ci alta:

> Formularul nu mai poate **refuza** util o plasare. Trebuie în schimb să facă fiecare plasare
> **ieftin de desfăcut și imediat vizibilă în consecințe.**

Asta s-a construit.

---

## 2. Ce s-a livrat

### 2.1 Graficul în trepte și reperele

KBotChartLineMode=Straight|Step este pe SERIE (implicit Straight, ShouldSerialize/Reset, categoria Chart Series), fiind proprietate a datelor. AsociereForm folosește Step în ambele vederi, inclusiv totalul: orizontală apoi verticală, ambele în culoarea punctului stâng, FillArea dreptunghiular; colțul sintetic nu are marcaj. Valoarea ține până la următorul instantaneu, ca ValoareaLa/totalul, nu se interpolează continuu.

KBotChartGuide și colecția sa au introdus Moment, Text, Tooltip, LineColor Empty=text stins, DashStyle implicit Dot, Visible și Tag. Reperele erau subțiri/punctate, în spatele seriilor, fără roșu, hit-test numai orizontal, tooltip ShowAt, fără clic/eveniment și fără extinderea axei. Documentul [Chart](../../src/KBot.Controls/Chart/KBotChartView.md) păstrează contractul și optimizarea ulterioară la linii continue; istoricul nu o contrazice ca regulă actuală.

### 2.2 Benzile și integrarea

[KBotLaneView](../../src/KBot.Controls/Lane/KBotLaneView.md) este suprafața custom Control/sealed/partial/Toolbox/IThemedControl/ISupportInitialize: o bandă per recepție, marcaje de instantanee, separator și banda neașezatelor; inițial compactă fără text pentru scara 20×20. Familia Lane cu View/Painting/Drag, Lane/Marker și colecțiile, Enums și documentație; stiluri Normal/Deletion/NoChange/Locked/Loose, EndMark None/Ok/Warning. Locked are lacăt în culoare plină, fără gri. C1–C9 rămân comune: temă, serializare/pinning, 96 DPI logici, ShowAt, validare EndInit/rulare și chenar în designer, log/tratare la UI inclusiv scroll, log/re-aruncare în mutatori.

MarkerDragStarting/Over/Dropped folosesc Allow implicit False și DoDragDrop. Marcajul vine din obiectul de date, nu din câmpul controlului (necesar între două suprafețe); controlul nu mută niciodată datele și desenează chenar și la refuz. Derulare numai verticală: timpul trebuie văzut integral orizontal.

Inițial splitDreapta.Panel1 avea splitSus: grafic sus, benzi jos, Panel2MinSize=60, SplitterDistance=180, editat direct în Designer. ReconstruiesteBenzi și SincronizeazaCulorile leagă culoarea benzii de serie și marcajul de punct; aceleași trei tratatoare scriu în același _pozitie ca arborele. EnlargeRequested deschide fereastra mare; orice atingere stinge ntfMesaj. Ordinea și celelalte abateri motivate sunt în §5; afișarea simultană a fost înlocuită în 0048-08.

### 2.3 F13 coborât la semn

Clientul scoate vetoul din MotivulRefuzului; EsteInainteDeDataReceptiei produce marcaj în denumire/tooltip și numărul «N din M instantanee sunt mai vechi decât data recepției». Serverul valideaza_plasarile scrie avertismente în loc de DecizieInvalida pe AMBELE căi, editor și ingestie; aplica_decizii primește avertismente=warnings. Un veto fals pe ingestie ar încălca F10, blocând o recepție pe care operatorul n-o poate repara. F14/F16 și o singură ștergere per lanț rămân vetouri; F15 rămâne semn.

Comparația este pe ZI, abatere deliberată de la timestampul planului: DataR este tastată, cu miezul nopții, DataH are ceasul sistemului; cele două nu dovedesc ordinea creării. Motivul și hazardul datei sunt păstrate în §1/§4/§8, fără repararea sursei Access.

### 2.4 Fereastra mare

AsociereBenziForm.{vb,Designer.vb}, KBotThemedForm modală maximizată, Lane dock Fill, fără buton de salvare (D-H). Nu are date/regulă proprii: împrumută UmpleBenzile și LeagaBanda cu cei trei tratatori, deci la închidere nu există două tablouri de reconciliat. Configurația inițială avea denumiri, sume, axă și LaneHeight=26; sumele au fost retrase în 0048-08.

## 3. Fișiere atinse

**Noi**
- `src/KBot.Controls/Lane/KBotLaneEnums.vb`, `KBotLaneMarker.vb`, `KBotLane.vb`, `KBotLaneView.vb`, `KBotLaneView.Painting.vb`, `KBotLaneView.Drag.vb`, `KBotLaneView.md`
- `src/KBot.Controls/Chart/KBotChartGuide.vb`, `KBotAutoPalette.vb`
- `src/KBot.App/Forexe/AsociereBenziForm.vb`, `AsociereBenziForm.Designer.vb`
- `docs/worklog/SLICE-0048-07-benzi-si-retragerea-f13.md` (fișierul ăsta)

**Modificate**
- `src/KBot.Controls/Chart/KBotChartEnums.vb` — `KBotChartLineMode`
- `src/KBot.Controls/Chart/KBotChartSeries.vb` — `LineMode` + perechea lui
- `src/KBot.Controls/Chart/KBotChartView.vb` — `Guides`, `AddGuide`, `ClearGuides`, `IKBotGuideHost`, `_hoverGuideIndex`
- `src/KBot.Controls/Chart/KBotChartView.Painting.vb` — desen în trepte, `ProjectGuides`, `DrawGuides`, `HitTestGuide`, eticheta reperului, `AutoColor` delegat la `KBotAutoPalette`
- `src/KBot.Controls/Chart/KBotChartView.md`, `src/KBot.Controls/CONTROLS.md`
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.37.0.0 ▸ **1.38.0.0**, convenția de organizare actualizată cu `Lane\`
- `src/KBot.App/Forexe/AsociereForm.vb`, `AsociereForm.Designer.vb`
- `PYTHON/routes/forexe/prelucrare_asociere.py`, `asociere.py`
- `PYTHON/tests/test_forexe_prelucrare_asociere.py`, `test_forexe_asociere.py`
- `tests/KBot.App.Tests/AsociereFormTests.vb`
- `docs/FUNDAMENT_Asociere_Receptii.md`
- `docs/worklog/KBOT_STATUS.md`

`AssemblyVersion` rămâne `1.0.0.0`.

---

## 4. Corecturile din fundament (§2 din plan)

- **F13 — RETRAS.** Textul e tăiat, nu șters, cu motivul complet și cu nota că a rămas SEMN pe amândouă căile și că se măsoară pe zi.
- **F25 — AMENDAT, și cheia numită corect.** Documentul (și citirea din care venea) spunea că pasul 4b se potrivește pe hash-ul de antet. **Nu.** Verificat în `Receptii_Prelucrare` real: `rsTmpR_Snap.FindFirst "CLng(DataR)=" & CLng(dtDataR)`. `sHashIdent` se CALCULEAZĂ în `ObtineDateHeader` și se SCRIE în `tmpFX_Receptii_R!HASH` la inserare, dar **nu se citește niciodată pentru potrivire**. Regula lui F25 rămâne întreagă.
- **F29 — NOU.** `DataR` e editabil de operator, inclusiv după creare, și tabelul nu are momentul creării. Nicio regulă nu are voie să-l trateze ca pe un fapt despre când a apărut recepția.
- **F30 — NOU, consemnat ca HAZARD, NEREPARAT aici.** Fiindcă 4b se potrivește pe `CLng(DataR)` cu `FindFirst`, două recepții cu același `DataR` se ciocnesc la fiecare descărcare de după prima: a doua o suprascrie pe prima și niciun rând nou nu se inserează. La PRIMA descărcare amândouă se inserează — de-aia perechi din astea ajung să existe (`AAB2HFBEEAF`, rândurile 268 și 269, ambele 16.01.2026, cu `HASH` identic, deci nici hash-ul n-ar discrimina). Editarea lui `DataR` pe site (F29) dă defectul oglindă: cheia se mută, 4b nu mai recunoaște recepția, se inserează un duplicat. Operatorul spune că n-a văzut niciodată editarea în practică și că purtarea pe aceeași zi «nu e bună tot timpul, dar merge în mare». **Consemnat, deliberat nereparat.**
- **§1.5 rescris** — «regula datei» e tăiată și înlocuită cu semnul; propoziția-cheie a designului («nu mai poate refuza util, deci trebuie să facă plasarea ieftin de desfăcut») e scrisă acolo.
- **Partea 5** — **O2 închis** (F23 a fost retras pe 26.08 și F24 l-a înlocuit; contractul în două faze e construit pe F24, nu se datorează nimic). **O7 adăugat**, arătând spre F30 ca hazard acceptat, nu ca decizie deschisă.

---

## 5. Ce s-a hotărât altfel decât în literă, și de ce

**5.1 Ordinea lui `ReconstruiesteBenzi`.** Planul cerea «imediat după `ReconstruiesteGrafic()` și înainte de `SincronizeazaCulorile()`». Imposibil ca atare: `SincronizeazaCulorile` nu se cheamă din `Reconstruieste`, ci din coada lui `ReconstruiesteGrafic` — ca să prindă și schimbarea de filă, și cea de selecție, care nu ating benzile. Intenția (benzile există înainte ca pasul de culori să ruleze) s-a obținut punând benzile **înaintea** graficului. Comentat la fața locului.

**5.2 Reperele: două obiecte, nu unul împărțit.** Planul spune «aceeași colecție `Guides`». Tipul CHIAR e împărțit — `KBotChartGuide` e desenat și de grafic, și de benzi, prin `IKBotGuideHost` — dar prima variantă punea același OBIECT în amândouă colecțiile, și acolo e o capcană: un reper are un singur câmp de proprietar (cel care îl repictează la schimbarea culorii), iar al doilea adăugat îl fura pe primul. Acum se construiesc două repere în ACEEAȘI buclă din aceeași plată. Garanția e la fel de tare — amândouă primesc `plata.DataPlata`, aceeași valoare, în același pas — fără obiect cu doi stăpâni.

**5.3 `RangeStart`/`RangeEnd` există, dar `AsociereForm` NU le folosește.** Planul cere «margini comune cu graficul, ca un reper de plată să stea în aceeași poziție orizontală în amândouă controalele». Controlul are cu ce (`RangeStart`/`RangeEnd`, doar la rulare), dar formularul nu le fixează, **deliberat**: benzile arată și instantaneele NEAȘEZATE, iar graficul nu; fixate pe intervalul graficului, marcajele din afara lui n-ar mai fi desenate — adică suprafața al cărei rost e tocmai să arate instantaneele neașezate le-ar ascunde. Alinierea la pixel între cele două rămâne deci adevărată doar când amândouă arată același interval. **De verificat pe ecran de operator**; dacă vrea alinierea cu orice preț, unealta e deja acolo.

**5.4 `KBotAutoPalette`.** `AutoColor` era o metodă privată a graficului. Fiindcă acum și benzile trebuie să dea aceeași culoare aceluiași fapt, setul s-a mutat într-un modul `Friend` împărțit, iar `KBotChartView.AutoColor` doar deleagă. Două implementări s-ar fi potrivit în ziua în care au fost scrise și s-ar fi despărțit la prima schimbare de temă. Culorile ies bit cu bit aceleași — e aceeași aritmetică mutată, nu rescrisă.

**5.5 `AllowDrop` — capcana C4 pe o proprietate MOȘTENITĂ.** Prinsă măsurând, nu ghicind: un `KBotLaneView` proaspăt serializa `AllowDrop`, unde un `KBotChartView` proaspăt nu serializa nimic peste cele trei linii pe care Visual Studio le scrie pentru orice control (`Location`, `Name`, `TabIndex`). Constructorul îl aprinde fiindcă tot rostul controlului e să primească aruncări, dar `AllowDrop` poartă `DefaultValue(False)`, deci designerul ar fi scris `benzi.AllowDrop = True` în fiecare formular-gazdă. **Un `ShouldSerializeAllowDrop` NU merge** — și s-a încercat întâi: `TypeDescriptor` construiește descriptorul unei proprietăți moștenite pe tipul care o DECLARĂ, deci caută metoda pe `Control` și n-o vede niciodată pe a noastră. Soluția e cea deja consemnată în notele proiectului pentru cazul ăsta: se umbrește proprietatea și se marchează umbra, fiindcă umbrirea NU moștenește atributele de serializare ale bazei. Verificat empiric prin `TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)` — drumul pe care chiar merge Visual Studio — cu o sondă temporară, ștearsă după (§7).

**5.6 Testele care codificau F13 au fost rescrise, nu șterse.** Trei bucăți (două în Python, una în `KBot.App.Tests`) verificau vetoul. Regula n-a dispărut, a coborât — deci testele au coborât cu ea și verifică acum semnul. Nu e «test nou»: e același test, pe regula corectată. Un test lăsat pe regula retrasă ar fi însemnat livrarea unei suite roșii.

---

## 6. Punctele de «oprește-te și raportează» (§4 din plan) — ce a ieșit

**6.1 Plățile în sarcina utilă — NU s-a declanșat.** `GET /api/forexe/asociere` întoarce deja `plati: [{data_plata, suma, nr_op}]` (`asociere.py`, `_PLATI_SQL`), `AsociereStare.Plati` există în `KBot.Domain/AsociereStare.vb` ca `List(Of PlataAsociere)`, iar `ApiClient.CitesteAsociere` o umple deja. Nu s-a atins nicio rută și niciun tabel.

**6.2 Aspectul din designer — NU s-a declanșat.** `docs/kbot-forms-ui-convention.md` §5, §6 și §8 **nu** rezervă aspectul operatorului: rezervă aspectul **fișierului `.Designer.vb`**, spre deosebire de codul de rulare. §6 e explicită — «dacă o schimbare aparține Designerului — mutare/re-părintare/ re-docare, redimensionare, adăugare de control — fă-o **direct în `*.Designer.vb`**». Deci restructurarea s-a făcut acolo, direct. §7 respectată: **niciun comentariu nou** în `.Designer.vb` (cele existente în `AsociereForm.Designer.vb` sunt dinainte, neatinse).

**6.3 Canalul de avertismente al serverului — NU s-a declanșat.** Există deja: `aplica_comenzi` primește `avertismente: list`, `POST /api/forexe/asociere` îl întoarce ca `"avertismente"`, iar clientul îl citește deja în `AsociereRezultat.Avertismente`. Pe calea de ingestie echivalentul e `warnings`, pe care `aplica_decizii` îl primea deja pentru altceva. Nu s-a inventat nicio a doua formă de răspuns.

**6.4 Unde trăia F13 — NU s-a declanșat.** F13 era un bloc `if` distinct în `valideaza_plasarile`, nu împletit cu F14/F16. Coborârea lui nu atinge purtarea celorlalte două, iar testele lor trec neschimbate.

---

## 7. Teste

| Suită | Înainte | După |
|---|---|---|
| `tests/KBot.Controls.Tests` | 953 trecute / 0 picate | **953 trecute / 0 picate** |
| `tests/KBot.App.Tests` | 185 trecute / **13 picate** | 185 trecute / **13 picate** |
| `PYTHON/tests` (via `.venv`) | 465 trecute / 15 sărite | **465 trecute / 15 sărite** |

Linia de plecare pentru `KBot.App.Tests` s-a luat **prin `git stash` pe modificările feliei**, nu din memorie — tocmai fiindcă lucrul ăsta a mers prost o dată (`KBOT_STATUS` zicea șapte, erau șaptesprezece). Cele 13 picate sunt **preexistente și străine de felia asta**: `DdfViewTests` (3), `DdfXfaParserTests` (2), `IstoricViewTests` (6), `MainFormNavItemsTests` (1), `XfaXmlPreviewTests` (1). Cu modificările aplicate au fost o clipă 14 — a paisprezecea fiind `AsociereFormTests.F13_ORecepțieMaiNouaDecatInstantaneul_ERefuzata`, adică chiar testul regulii retrase; rescris ca `F13Retras_...ETotusiPrimita`, suita s-a întors la 13.

**Fără teste noi**, conform regulii permanente. Singura excepție a fost o **sondă temporară** (`ZZTempLaneSerializationProbe.vb`) folosită ca să măsor ce serializează un `KBotLaneView` proaspăt față de un `KBotChartView` proaspăt; a găsit `AllowDrop` (§5.5), și a fost **ștearsă** după ce măsurătoarea a ieșit `CHART: [Location, Name, TabIndex]` = `LANE: [Location, Name, TabIndex]`. Nu a rămas în suită.

**Build:** `dotnet build KBot.sln` — **0 erori**. `KBot.Controls` singur: **0 erori / 0 avertismente**. Pe soluție apar 6 avertismente `MSB3825` (`AsociereForm.resx`, `Il_Receptii.ImageStream` prin `BinaryFormatter`) — **preexistente**, consemnate ca atare și în worklogul feliei 0048-06, și vizibile doar când se regenerează `.resx`-ul. Nu s-a atins `.resx`-ul în felia asta pentru controalele noi: `benzi` nu are nicio imagine (butonul de mărire își desenează singur pictograma când gazda nu-i dă una).

---

## 8. Observații pe `Receptii_Prelucrare`, consemnate nereparate

Operatorul a dat VBA-ul real pe 31.08.2026 și l-a descris ca «nu e bun tot timpul, dar în mare merge». **Niciun VBA nu s-a modificat în felia asta** — VBA-ul e referința, portarea trăiește altundeva.

1. **`CLng(DataR)` e cheia de identitate** (§4). Granularitate de zi, `FindFirst`, prima potrivire câștigă. Consecințele sunt F30.
2. **`rsTmpR_Snap` e un instantaneu luat ÎNAINTE de buclă.** Recepțiile inserate în timpul rulării sunt invizibile iterațiilor următoare ale aceleiași rulări. De-aia două recepții din aceeași zi se inserează amândouă la prima descărcare și se ciocnesc la următoarele.
3. **Un `cReceptii` gol întoarce `False`.** `If cReceptii Is Nothing Then Exit Function` și `If cReceptii.Count <= 0 Then Exit Function` cad amândouă pe valoarea implicită a funcției. Dacă apelantul citește `False` ca eroare, un angajament fără recepții e raportat ca eșec. De verificat pe partea apelantului; nu e evident greșit, doar ambiguu.
4. **`Nz(DMax("NRCRT", "tmpFX_Receptii_R"), 0)`** — `Nz` e interzis în cod K-BOT nou. Consemnat pentru portare, nu pentru original.
5. **`NRCRT` e atribuit de K-BOT**, nu de FOREXE (`DMax + 1`). Confirmat de operator: nu înseamnă nimic dinspre site. Nicio regulă nu are voie să-l trateze ca ordine de creare — de-aia constrângerea de ordonare avută în vedere mai devreme în design a fost lăsată deoparte.

---

## 9. Ce NU s-a făcut, și de ce

Din §5 al planului, explicit în afara ariei:

- **Nicio propunere automată.** Formularul arată; nu ghicește cărei recepții îi aparține un orfan.
- **Niciun teanc de anulare.** Cu o singură salvare la sfârșit și cu `Comenzi()` trimițând doar ce diferă, o tragere greșită se desface trăgând înapoi. Aia e reversibilitatea.
- **Nicio plasare în masă.** Operatorul raportează ~10 instantanee neașezate obișnuit, 25 la cel mai rău văzut. Câte una pe rând e primitiva potrivită la scara aia.
- **F30 nu e reparat.** Consemnat în fundament, niciun rând de cod.
- **Fără teste noi** (vezi §7).
- **Fără randare și fără capturi de ecran.** Ale operatorului, pe mașina lui, după publicare.
- **Nimic nu a rulat pe MariaDB.**

## 10. Rămâne neverificat

- **Totul, vizual.** `KBotLaneView` nu a fost desenat niciodată — nici la rulare, nici în designerul Visual Studio. Mărimile compacte (bandă de 13 px logici, marcaj de 7) sunt alese pe hârtie din scara pe care a numit-o operatorul; dacă nu se citesc, `LaneHeight`/`MarkerSize` sunt exact manetele de învârtit.
- **Lacătul la mărimea compactă** e două-trei pixeli de detaliu. Consemnat ca limită în `KBotLaneView.md`, nu ca defect.
- **Alinierea orizontală bandă ▸ grafic** e adevărată doar când amândouă arată același interval (§5.3). De privit pe ecran.
- **Semnul F13 pe date reale.** Comparația pe zi e argumentată, nu măsurată: nimeni n-a rulat-o peste un angajament viu ca să vadă câte semne se aprind.
- **Ruta și clientul nu au rulat viu.** La fel ca toată familia 0048.

<a id="slice-0048-08"></a>

## SLICE-0048-08 — benzile citibile, eticheta care nu mai rămâne în urmă, și cele două vederi din dreapta sus

Continuarea directă a feliei `SLICE-0048-07`. Cerută punct cu punct de operator pe 01.09.2026, după ce a privit prima oară benzile de așezare.

**Stare:** cod verde — `dotnet build KBot.sln` **0 erori**, cele **6 `MSB3825` preexistente** pe `AsociereForm.resx` / `Views/*.resx` (`ImageStream` prin `BinaryFormatter`) fiind singurele avertismente. `KBot.Controls.Tests` **953 trecute / 0 picate** (exact linia de plecare). `KBot.App.Tests` **185 trecute / 13 picate** — aceleași 13 roșii **preexistente și străine** de felia asta (`DdfViewTests` 3, `DdfXfaParserTests` 2, `IstoricViewTests` 6, `MainFormNavItemsTests` 1, `XfaXmlPreviewTests` 1). **Fără teste noi.** Python neatins.

**S-A VĂZUT PE ECRAN** — prima felie din familia 0048 despre care se poate spune asta. `KBotLaneView` a fost randat cu `DrawToBitmap` pe toate trei schemele (Classic / Modern / Dark), iar `AsociereForm` a fost randat întreg, în amândouă vederile din dreapta sus, cu un tablou de probă (4 recepții × 4 instantanee + 3 orfani + 2 plăți). Sondele de randare au fost **temporare și au fost șterse** — nu a rămas niciun test nou în suită. Ce **NU** s-a văzut: designerul Visual Studio, un server viu, o tragere adevărată cu mouse-ul.

**Nimic nu a rulat pe MariaDB.** Nicio rută Python atinsă, nicio schemă schimbată.

---

## 1. Cerințe, corecții și motive

### Fără sume; segmente și spațiu final

MarkerLabelsVisible=True a fost scos din Designerul ferestrei mari (implicit False): sumele acopereau banda de dedesubt, unde se compară lanțurile; valorile rămân în arbore/grafic/tooltip. Motivul este în doc-ul clasei, fără comentarii noi în Designer.

SegmentedRail=True și SegmentWidth (0=LaneLineWidth): markerul colorează de la el la următorul, ultimul până la capăt. Banda simplă rămâne sub segmente ca țintă goală și pentru intervalul dinaintea primului; bucățile sunt sortate după X, nu după colecția nesortată; Loose nu pictează segment, nefiind un lanț. Fiecare marcaj ia AutoColor(j) după poziția în lanț, ca punctul din fila Recepția; pe Tot angajamentul marcajele rămân distincte. AplicaCulorileBenzii primește suprafața și reface culoarea poziției în loc de reset la Empty.

TrailingSpace (implicit 0, logici) face vizibilă bucata ultimului marker. MomentToX folosește _plotRect.Width−trailing: spațiul este scos din AXA timpului, nu din suprafață; datele/reperele și etichetele axei rămân corelate. Limita este un sfert din suprafață. În Designer: benzi 50, benziMari 60. Regulile complete sunt în [Lane](../../src/KBot.Controls/Lane/KBotLaneView.md).

### Tooltipul rămas pe primul punct

KBotToolTip.Schedule compara ReferenceEquals pe un singur KBotToolTipContent rescris de control; astfel al doilea conținut era ignorat până la ieșirea de pe tot controlul. Garda compară acum și amprenta antet ␁ corp ␁ subsol. Un tooltip deja deschis se schimbă imediat, fără încă o întârziere; ShowScheduled este comun timerului și schimbării la cald, păstrând verificarea cursorului pe ambele căi. HideNow curăță și amprentele, ca prima etichetă după închidere să nu fie înghițită. Corecție comună benzilor, graficului, grilei și arborelui.

### Contrastul și chenarele

Readable din KBotAutoPalette mută numai lightness în pași 0,04, maximum 12, mărginit la 0,16…0,88, până la 3:1 față de fundal (pragul invocat pentru grafică). Hue și saturația rămân; dacă pragul nu e atins rămâne ultima valoare, fără împingere nelimitată spre negru. HSL lightness singur făcea galbenul/albastrul inegal lizibile pe Classic/Modern; modulul comun aplică schimbarea graficului și benzilor.

BorderColor exista; BorderWidth nou pe Chart/Lane, logic, implicit 1/DefaultValue, 0=fără chenar ca BorderVisible=False. Setterul Chart.BorderColor reconstruiește acum penița cached, nu doar invalidează. OnHandleCreated și OnDpiChangedAfterParent refac peniile ambelor controale: înainte de handle DeviceDpi era 96 indiferent de ecran.

### Două vederi în loc de împărțirea panoului

splitSus a fost scos. În splitDreapta.Panel1: benzi Fill, grafic Fill, navGrafice Top, în ordinea inversă a docării; benzi.Visible=False și cheia «grafic» în Designer. Constructorul o aplică explicit fiindcă EndInit din procesul VS sare selecția/validarea. Două panouri lăsau ~150px fiecăruia, insuficient; acum fiecare folosește întregul loc. Reconstruieste le ține actuale și ascunse, deci navGrafice schimbă numai vizibilitatea, fără reconstrucție.

Configurația benzii din panou: LaneCaptionsVisible=True, LaneCaptionWidth=150, LaneHeight=18, LaneSpacing=3, MarkerSize=9, AxisVisible=True, SegmentWidth=4; MarkerLabelsVisible=False și aici. Limitele vizuale, alinierea numai pe interval comun, reperele din afara intervalului și probele sunt păstrate în §3–§5.

## 2. Fișiere atinse

**Modificate**
- `src/KBot.Controls/ToolTip/KBotToolTip.vb` — amprenta textului, `ShowScheduled`, schimbarea la cald; `KBotToolTip.md`
- `src/KBot.Controls/Chart/KBotAutoPalette.vb` — `Readable` + luminanța relativă WCAG
- `src/KBot.Controls/Chart/KBotChartView.vb` — `BorderWidth`, `BorderColor` reconstruiește penița, penițe rezidite la handle/DPI; `KBotChartView.Painting.vb` (`BorderWidth = 0`); `KBotChartView.md`
- `src/KBot.Controls/Lane/KBotLaneView.vb` — `SegmentedRail`, `SegmentWidth`, `TrailingSpace`, `BorderWidth`, penițe rezidite la handle/DPI; `KBotLaneView.Painting.vb` (`AxisRun`, `DrawLaneSegments`, axa pe intervalul timpului); `KBotLaneView.md`
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.38.0.0 ▸ **1.39.0.0**
- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `splitSus` scos, cele trei controale în `splitDreapta.Panel1`, `benzi` la mărimea panoului, `navGrafice.SelectedKey = "grafic"`
- `src/KBot.App/Forexe/AsociereForm.vb` — `NavGrafice_SelectionChanged` + `AplicaVedereaDinDreaptaSus`, culoarea de poziție pe marcaje, `AplicaCulorileBenzii` primește suprafața
- `src/KBot.App/Forexe/AsociereBenziForm.Designer.vb` — fără sume, `SegmentWidth`, `TrailingSpace`; `AsociereBenziForm.vb` (doc-ul de clasă)
- `src/KBot.App/KBot.App.vbproj` — `FileVersion` 1.0.22.0 ▸ **1.0.23.0**
- `docs/worklog/KBOT_STATUS.md`, fișierul ăsta

`AssemblyVersion` rămâne `1.0.0.0` peste tot.

**Fără fișiere noi.** **Fără teste noi** (regula permanentă). **Nimic Python.**

---

## 3. Ce s-a văzut pe ecran, și cum

Două sonde temporare, șterse după ce au răspuns:
1. `KBotLaneView` singur, 900×320, cu 5 benzi × 4..8 marcaje, o bandă de orfani sub separator și două repere de plată, randat prin `DrawToBitmap` pe Classic, Modern și Dark. Confirmat: bucățile colorate, orfanii fără bucăți, spațiul din dreapta, datele de pe axă la capetele intervalului TIMPULUI (nu ale dreptunghiului), semnele de capăt, niciun ton spălăcit pe schemele deschise.
2. `AsociereForm` întreg, 1400×900, cu un `AsociereFakeApi` (4 recepții × 4 instantanee, 3 orfani, 2 plăți), randat în amândouă vederile prin `navGrafice.SelectedKey`. Confirmat: banda de navigare sus, graficul implicit, «Distribuție» care aduce benzile pe tot panoul, denumirile benzilor, reperul plății traversând toate benzile.

Prima sondă a scos și un defect al MĂSURĂTORII, nu al codului: `Form.DrawToBitmap` peste `ClientSize` desenează și rama și bara de titlu, deci partea de jos a controlului cădea în afara imaginii și axa părea că lipsește. Randarea CONTROLULUI, nu a formularului, e drumul corect.

---

## 4. Ce NU s-a făcut, și de ce

- **Reperele din afara intervalului tot nu se desenează.** O plată căzută în `TrailingSpace` (adică la puțin după ultimul instantaneu) e nedesenată, ca înainte. Regula «un reper nu întinde axa timpului» e din 0048-07 și e deliberată; se vede în captură, unde plata din 18.02 nu apare fiindcă ultimul instantaneu e din 14.02. **De hotărât de operator** dacă spațiul din dreapta ar trebui să primească și reperele care cad în el — argumentul PENTRU e că exact acolo întrebarea «a căzut instantaneul înainte sau după plată» e cea mai ascuțită.
- **Alinierea la pixel bandă ▸ grafic** rămâne cum a lăsat-o 0048-07 (adevărată doar când amândouă arată același interval), și acum contează mai puțin: cele două nu mai sunt niciodată pe ecran în același timp.
- **Nicio schimbare pe server, în fundament sau în reguli.** Felia asta e numai despre ce se vede.
- **Tragerea nu a fost încercată cu mouse-ul.** Codul ei e neatins de felia asta, dar niciun gest adevărat n-a trecut prin el, nici acum, nici în 0048-07.

## 5. Rămâne neverificat

- **Designerul Visual Studio.** Nici `AsociereForm`, nici `AsociereBenziForm`, nici `KBotLaneView` n-au fost deschise acolo. `benzi.Visible = False` scris în designer face suprafața invizibilă și pe suprafața de proiectare — de știut înainte de a o căuta.
- **Scara reală.** Toate capturile sunt cu 4-5 benzi. Cazul numit de operator — 20 de benzi × 20 de marcaje — n-a fost desenat niciodată, iar acolo `LaneHeight = 18` cere derulare.
- **150% DPI.** Peniile se reconstruiesc acum la handle și la schimbarea de DPI, dar asta e argumentat, nu măsurat pe un ecran adevărat.
- **Ruta și clientul nu au rulat viu**, ca toată familia 0048.

<a id="slice-0048-09"></a>

## SLICE-0048-09 — totalul recepțiilor scris chiar pe reperul plății

Continuarea directă a feliei `SLICE-0048-08`. O singură cerere a operatorului, pe 01.09.2026, după ce a privit benzile de așezare în vederea «benzi»:

> «când trec cu mausul peste o bandă verticală (plățile) vreau să văd în etichetă Totalul
> Recepțiilor până la data aia (adică totalul ultimei stări a fiecărui R) și diferența dintre ele,
> ca să văd ușor dacă H-urile sunt bine puse sau nu.»

**Stare:** cod verde — `dotnet build KBot.sln` **0 erori / 0 avertismente**. `KBot.App.Tests` **188 trecute / 13 picate** din 201; cele 13 roșii sunt **exact aceleași preexistente și străine** de felia asta (`DdfViewTests` 3, `DdfXfaParserTests` 2, `IstoricViewTests` 6, `MainFormNavItemsTests` 1, `XfaXmlPreviewTests` 1) — linia de plecare era 185/198, deci **cele 3 teste noi trec toate trei** și nimic nu s-a mișcat în rest. `KBot.Controls` neatins. Python neatins.

**S-A VĂZUT PE ECRAN**, parțial: eticheta a fost randată offline (sondă temporară în scratchpad, ștearsă) prin chiar motorul care o desenează în aplicație — `KBotRichText.Parse` ▸ `Layout` ▸ `Draw`, adică drumul pe care îl ia `KBotToolTipWindow`. Se confirmă că `<b>` iese îngroșat (nu literal), că cele patru linii de cifre se citesc ca o socoteală și că avertismentul de jos, despărțit de un rând gol, nu se amestecă cu ele. Ce **NU** s-a văzut: eticheta ridicată de un mouse adevărat peste o bandă adevărată, cu date de la server.

**Nimic nu a rulat pe MariaDB.** Nicio rută Python atinsă, nicio schemă schimbată, niciun contract de fir schimbat — toată socoteala se face din ce era deja în `AsociereStare`.

---

## 1. Socoteala pe tabloul local

Textul generic «Totalul recepțiilor la data asta a intrat în ordonanțare» a fost înlocuit cu calculul din fundament §1.3: OP n numai dacă există număr; total recepții la data plății îngroșat; plăți anterioare numai dacă există (fără rând 0,00); plata curentă; diferența; după un rând gol, numărul H neașezate până la acea dată și avertismentul că nu sunt cuprinse.

Totalul este suma ultimului H de la/sau dinaintea plății pentru fiecare R, folosind aceeași ValoareaLa ca linia Total angajament; fără a doua aritmetică. Plățile anterioare sunt singura completare peste cerere: diferența = recepții − plăți anterioare − plata curentă, adică Ramas, nu o comparație falsă doar cu ultima plată. Exemplu: 200−100−50=50.

Pozitivul este rest normal de plată; numai diferența negativă primește ErrorColor din paletă prin markup color=#RRGGBB, semnalând posibila așezare după plată sau pe alt R. H neașezate fac cifra provizorie și sunt numărate chiar lângă ea; nu intră în niciun lanț/total. Recalcularea după fiecare mutare și probele rămân în secțiunile următoare.

## 2. Unde se vede

Corpul e construit o singură dată pe plată și se pune pe AMÂNDOUĂ reperele — cel de pe benzi și cel din grafic — ca și până acum. Deci:

- banda strâmtă din `AsociereForm` (vederea «benzi»),
- fereastra mare `AsociereBenziForm`,
- graficul din `AsociereForm` (vederea «grafic»), care primește aceleași repere.

Și, esențial: **se reface la fiecare tragere.** `ConstruiesteReperelePlatilor` se cheamă din `ConstruiesteBenzi`, care se cheamă din `Reconstruieste` — adică după fiecare mutare, pe amândouă suprafețele. Fără asta cifra ar răspunde la întrebarea de acum două trageri, ceea ce e mai rău decât să nu răspundă deloc.

## 3. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Forexe/AsociereForm.vb` | `ConstruiesteReperelePlatilor` calculează lanțurile o SINGURĂ dată (nu unul pe plată) și adună `platiAnterioare` pe parcurs, în ordinea datelor — chiar definiția lui `PlatiAnt`. Două metode noi: `CorpulReperului` (socoteala + textul) și `LiniaDiferentei` (colorează doar diferența imposibilă). |
| `tests/KBot.App.Tests/AsociereFormTests.vb` | Trei teste noi + două ajutoare (`Banda`, `Reper`) și două tablouri de probă. |

## 4. Teste

Trei teste noi, toate pe firul STA existent, fără ecran:

1. `ReperulPlatii_ScrieTotalulReceptiilorAsaCumStateaLaDataAia` — chiar tabelul §1.3: la prima plată intră în total DOAR instantaneul de dinaintea ei, iar linia «Plăți anterioare» lipsește cu totul (nu scrie «0,00»).
2. `ReperulPlatii_ScadePlatileDeDinainteaLui` — a doua plată: total 200, anterioare 100, plata 50, diferență 50. Exact rândul 04/01 din tabelul fundamentului.
3. `ReperulPlatii_SeRefaceLaFiecareTragere_SiSpuneCateAuRamasPeJos` — cifra se schimbă (300 ▸ 350) când un instantaneu neașezat e tras pe o recepție, iar avertismentul despre neașezate dispare odată cu ultimul. **Ăsta e testul care apără rostul cifrei**, nu doar valoarea ei.

`dotnet test tests/KBot.App.Tests` ▸ 188 trecute / 13 picate (cele 13 preexistente, enumerate mai sus). `dotnet build KBot.sln` ▸ 0 erori, 0 avertismente.

## 5. Neverificat / amânat

- **Eticheta ridicată de un mouse adevărat**, peste o bandă adevărată, cu date de la server. S-a văzut textul randat prin motorul lui, nu fereastra plutitoare pe ecran.
- **Nicio dată reală.** Toate cifrele de mai sus vin din tablouri de probă scrise după tabelul §1.3. Prima confruntare cu un angajament viu poate arăta că `Plati` sosește altfel decât presupune ordonarea după dată (de pildă două plăți în aceeași zi, care aici se despart doar prin ordinea din listă, fiindcă `PlataAsociere` nu are un identificator propriu).
- **Culoarea diferenței negative** n-a fost văzută: pentru ea trebuie un tablou în care plățile depășesc recepțiile la o dată, iar testele nu o acoperă fiindcă `ThemeManager.Current` poate fi `Nothing` fără ecran și atunci linia iese necolorată, pe drept.
