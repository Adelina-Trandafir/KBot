# SLICE-0048-06 — setul de culori al graficului + clic pe punct ▸ rândul din arbore

Continuarea directă a feliei `SLICE-0048-05` (graficul `KBotChartView` din `AsociereForm`), după
ce operatorul l-a văzut pe ecran. Două cereri, în două runde: mai întâi **culorile**, apoi
**legarea clicului de arbore**. Codul graficului stă tot în `src/KBot.Controls/Chart/`, deci tot
în engleză, inclusiv numele categoriilor din grila de proprietăți.

**Stare:** cod verde — `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori (doar
cele 6 `MSB3825` vechi de pe `.resx`), soluția întreagă 0 erori. `KBot.Controls.Tests`
**953 trecute / 0 picate** — roșul preexistent și străin de felia asta a fost închis aici, cu o
singură linie (vezi §5).
**Fără teste noi și fără randare** — operatorul a spus explicit că testarea și randarea sunt
treaba lui.

---

## 1. Pasul 06-01 — setul de culori

Trei observații ale operatorului, după ce a văzut graficul pe ecran:

> 1. cred că folosești culorile nodurilor sau ceva, fiindcă nodurile «blocate/dezactivate» apar
>    toate gri în grafic. schimbă
> 2. diferența de culoare trebuie să fie mai mare, am multe R și unele arată aproape la fel
> 3. NICIODATĂ roșu în grafic, decât dacă e ceva rău. la afișarea normală — roșu NU se folosește

### 1.1 Gri în grafic (1)

Nu venea din arbore, ci din grafic: `CuloarePunct` întorcea `DisabledTextColor` pentru un
instantaneu blocat sau «fără schimbare», iar arborele copia apoi culoarea punctului. Pe un lanț
unde majoritatea legăturilor sunt blocate, linia ieșea gri de la un capăt la altul — graficul nu
mai spunea nimic, iar rândul cu care trebuia împerecheat nu mai avea cu ce.

`CuloarePunct` a dispărut; punctul ia direct `grafic.AutoColor(i)`. Informația nu se pierde:
«scos din joc» e scris pe rând de două ori oricum, prin lacăt și prin marcajul adăugat la
denumire. Culoarea rămâne liberă pentru singura treabă pe care nimic altceva n-o poate face —
să lege punctul de rândul lui.

Gri-ul de bază (`CuloareDeBaza`) rămâne acolo unde graficul chiar n-are nicio părere: rândurile
din arborele liber și, în vederea pe tot angajamentul, frunzele (acolo linia numește recepția,
deci culoarea se duce pe rădăcină).

### 1.2 Diferența de culoare (2)

`AutoColor` era o plimbare prin accentele paletei, repetate apoi cu un pas mai deschis. La patru
linii merge; la câte are un angajament real, «un pas mai deschis» nu e o diferență pe care ochiul
s-o poată ține, și două recepții arătau la fel.

Acum se mișcă **nuanța**, cu fracția de aur (0,618) din interval la fiecare pas: indici
consecutivi cad cât de departe îi poate pune un șir, și rămân depărtați oricât de lungă e lista —
exact proprietatea pentru care se alege numărul ăsta. Peste el, luminozitatea alternează în trei
trepte, deci și doi indici care ajung din urmă la o nuanță apropiată diferă ca greutate.

Nimic nu e culoare scrisă în cod: nuanța de pornire, saturația și luminozitatea de mijloc se
citesc din accentul schemei ACTIVE, deci indicele 0 CHIAR e accentul, iar tot setul se rotește cu
tema. Numerele din cod sunt limite (cât de închis mai e citibil pe fundalul ăsta) — același fel
de valoare pe care `ThemeShapes.Lighten` îl primea deja. Pe schemele întunecate întregul set e
ridicat, altfel liniile intră în fundal.

Conversia înapoi în culoare se face în HSL (`FromHsl`, privat în control), adică exact spațiul în
care raportează `Color.GetHue/GetSaturation/GetBrightness` — o culoare din paletă desfăcută cu ele
și strânsă la loc aici iese neschimbată. Convertorul NU s-a adăugat în `KBot.Theming`: n-are alt
client azi, iar mutarea acolo ar fi atins proiectul de teme și `FileVersion`-ul lui degeaba.

`AutoColor` e **public** dinadins: o gazdă care colorează rânduri lângă grafic (exact ce face
`AsociereForm`) trebuie să poată cere ACELEAȘI culori, nu unele care seamănă.

### 1.3 Fără roșu (3)

Roșul e ce cheltuie aplicația asta pe «ceva e greșit»; un grafic care-l împarte pe rând învață
operatorul să nu-l mai citească. Nuanțele stau strict între `HueFirst` (30°) și `HueLast` (330°):
pana roșie din jurul lui zero nu e «sărită», e în afara intervalului, deci nimic nu poate cădea
acolo indiferent de accentul schemei active. `ErrorColor` a ieșit din set. O gazdă care chiar
vrea să spună «asta e rea» scrie mai departe ea `LineColor`/`PointColor` — singura cale pe care
roșul ar trebui să ajungă vreodată pe control.

Primele 14 intrări, pe un accent albastru obișnuit (calculate, nu desenate — vezi §5):

```
#2273C3 #4E981B #A93CDD #22C39E #98861B #3C44DD #22C329
#981B87 #3CB9DD #91C322 #4D1B98 #3CDD8B #C38022 #1B4498
```

Cea mai caldă e `#C38022`, portocaliu.

---

## 2. Pasul 06-02 — clic pe punct ▸ rândul din arbore

> când utilizatorul dă clic pe un nod din grafic, nodul corespunzător din arbore trebuie
> selectat. ține referința nodurilor în proprietatea tag (părere, poți face cum vrei)

Cealaltă jumătate a colorării. Punctul și rândul au deja aceeași culoare, deci operatorul le
împerechează din ochi — dar pe un lanț lung rândul pe care tocmai l-a găsit în grafic poate fi
ieșit din vizor sau sub o recepție strânsă. Selectarea e ce transformă «văd care e» în «pot lucra
pe el».

### 2.1 Ce era deja acolo

`PointClicked(seriesKey, pointIndex)` exista din felia 0048-05; `FindSeries(key)` la fel. Iar
`punct.Tag` ține instantaneul de când se construiește punctul (`AdaugaPunct`), deci sugestia
operatorului era deja pusă în practică — nu se re-deduce nimic din etichetă și nu se caută după
un moment pe care două instantanee îl pot împărți.

### 2.2 `AdvancedTreeControl.SelectAndReveal(node)` — nou, public

`SelectedNode = nod` NU e de ajuns, și ăsta e tot rostul metodei: un nod aflat sub un părinte
strâns nu e deloc în lista vizibilă, deci selecția e reală dar invizibilă — operatorul vede un
clic care n-a făcut nimic. Metoda desface toți ascendenții, cheamă `RefreshScrollVisibility()`
(desfacerea a schimbat înălțimea conținutului, iar `EnsureNodeVisible` măsoară față de bară, deci
intervalul trebuie corect ÎNAINTE să fie întrebat) și abia apoi derulează minimul necesar.

**Nu ia focusul, dinadins.** Operatorul a dat clic în altă parte; tras aici, cursorul ar face ca
următoarea săgeată să miște un arbore la care nu se uita. (`KBotChartView.OnMouseDown` cheamă
`Focus()` pe el însuși, deci focusul rămâne unde a dat operatorul clic — pe grafic.)

Un nod care nu e din arborele ăsta e no-op, nu eroare: apelantul potrivește două liste, iar «aici
nu e» e un răspuns obișnuit.

Metoda a intrat în `AdvancedTreeControl.API.vb`, nu în formular, fiindcă orice apelant din AFARA
arborelui — un punct pe un grafic, un rezultat de căutare, un rând dintr-o listă alăturată — vrea
exact aceleași două jumătăți, iar fiecare scriind aceleași trei linii e felul în care jumătățile
ajung să se despartă.

### 2.3 Ce s-a schimbat în formular

`_nodInstantaneu` ținea `TreeItem`. Un `TreeItem` nu-și cunoaște controlul, iar instantaneele sunt
împărțite pe DOI arbori — legatele în stânga, cele libere în dreapta. Selectarea cere amândouă
jumătățile, deci dicționarul ține acum `RandDeArbore` (arbore + nod), singura variantă care nu
poate ajunge să ceară unui arbore să selecteze un rând al celuilalt. Recepțiile n-au nevoie de
așa ceva: o rădăcină există doar în arborele din stânga, deci `_nodReceptie` rămâne pe `TreeItem`.

`Grafic_PointClicked` citește instantaneul din `Tag`, îl caută în dicționar și cheamă
`SelectAndReveal`. Un punct FĂRĂ instantaneu e linia de total — o însumare a mai multor recepții,
deci nu există un singur rând în spate și clicul e lăsat dinadins să nu facă nimic.

Nu se reconstruiește nimic aici: selectarea unui rând nu e o schimbare a tabloului, iar
reconstruirea graficului din chiar clicul graficului ar fi o cale bună de inventat o buclă.

---

## 3. Fișiere atinse

**Adăugate**

- `docs/worklog/SLICE-0048-06-culorile-graficului-si-clic-pe-punct.md` (fișierul ăsta)

**Modificate**

- `src/KBot.Controls/Chart/KBotChartView.Painting.vb` — `SeriesColor` simplificat; `AutoColor`
  rescris (public, nuanță pe fracția de aur între `HueFirst`/`HueLast`, ancorat în accentul
  schemei active, luminozitate în trei trepte, ridicat pe schemele întunecate); ajutoarele private
  noi `FromHsl` și `Channel`.
- `src/KBot.Controls/Tree/AdvancedTreeControl.API.vb` — metoda publică nouă `SelectAndReveal`.
- `src/KBot.App/Forexe/AsociereForm.vb` — `CuloarePunct` ȘTEARSĂ, punctele iau
  `grafic.AutoColor(i)`; `_nodInstantaneu` retipat pe `RandDeArbore`; `SincronizeazaCulorile`
  citește prin el; `Grafic_PointClicked` nou; clasa cuibărită `RandDeArbore` nouă.
- `docs/worklog/SLICE-0048-05-graficul-evolutiei.md` — pașii ăștia doi erau descriși acolo ca §10
  și §11; au fost scoși și mutați aici, cu o trimitere lăsată în locul lor.
- `docs/worklog/KBOT_STATUS.md` — rândul 0048-06.

---

## 4. Ce NU s-a atins, dinadins

- **`KBot.Theming` n-a fost atins deloc** — `FromHsl` a rămas privat în grafic (vezi §1.2), deci
  `FileVersion`-ul proiectului de teme nu se mișcă.
- **`CuloareDeBaza` a rămas** — o folosesc mai departe `ColoreazaInstantaneu` și resetul din
  `SincronizeazaCulorile`. A dispărut doar `CuloarePunct`, cea care aducea gri-ul ÎN grafic.

---

## 5. Rezultatele testelor

| Comandă | Rezultat |
|---|---|
| `dotnet build src\KBot.Controls\KBot.Controls.vbproj` | 0 erori, 0 avertismente |
| `dotnet build src\KBot.App\KBot.App.vbproj` | 0 erori (6 `MSB3825` preexistente pe `.resx`) |
| `dotnet build KBot.sln` | 0 erori |
| `dotnet test tests\KBot.Controls.Tests` | **953 trecute / 0 picate**, 0 regresii |

Roșul care a însoțit felia 0048-05 de la un capăt la altul —
`AdvancedTreePaddingsTests.Toate_marginile_sunt_proprietati_vizibile_in_categoria_Paddings`,
aștepta `"K-BOT Tree - Paddings"` și primea `"K-BOT: Paddings"` — **s-a închis la livrare**. Era o
redenumire de categorii lăsată pe jumătate în arborele de lucru: codul arborelui folosește
`K-BOT: <grup>` peste tot (Footer, Header, Search, Buttons, Colors, Columns, Tooltip, DND, Nodes),
deci testul era singurul rămas pe numele vechi. Reparația e ACEA singură linie, în test — codul nu
s-a atins, fiindcă el e deja consecvent.

Setul de culori a fost verificat **prin calcul** (o simulare a aritmeticii din `AutoColor`, fără
să se instanțieze controlul și fără să se deseneze nimic) — de acolo vin cele 14 intrări din §1.3.

---

## 6. Nerulat / amânat

- **`AsociereForm` tot NEVĂZUT** — nici pe ecran, nici în designerul VS. Operatorul a cerut
  explicit să nu se randeze nimic: testarea și randarea sunt ale lui. Prima verificare vizuală a
  culorilor și a clicului rămâne de făcut de el.
- **Fără teste noi**, din același motiv.
- `grid.BackColor = SystemColors.Window` din `AsociereForm.Designer.vb` — același defect de
  «proprietate fixată» ca la arbori, dar repararea cere întâi `ShouldSerializeBackColor` + un
  fanion de fixare pe `KBotDataView`, altfel linia ștearsă se rescrie la următoarea salvare din
  designer. Semnalat, necerut, neatins.
- Fonturile `Calibri 9` fixate din designer pe arbori și pe grafic — lăsate așa.
- **Livrarea a luat cu ea și lucrul din arborele de lucru pe `AdvancedTreeControl`** (chei de
  pictogramă pe bandă, `TreeImageKeyConverter`, redenumirea categoriilor, `TooltipShowOnlyOnRightIcon`,
  exportatorul din `KBot.DevHarness`): nu e felia asta, dar `AsociereForm.Designer.vb` îl FOLOSEȘTE
  (`TooltipShowOnlyOnRightIcon` nu există fără el), deci un commit care l-ar fi lăsat afară n-ar fi
  compilat. A plecat într-un commit al lui, ÎNAINTEA acestuia, și **nu are worklog** — nu s-a putut
  stabili cărei felii îi aparține.
