# SLICE-0030 — Meniul de coloană: trei file, rânduri pe măsura temei, și bifele care se pierduseră

**Cerere operator** (patru puncte, după ce a rearanjat singur `KBotFilterPopup` cu un `TableLayoutPanel`):

> a. Filtrarea nu mai merge — arată o grilă goală. Resetarea încă merge.
> b. Fereastra nu se redimensionează corect.
> c. Pe tema modernă, din cauza umpluturii în plus, controalele arată prost — nu e destul spațiu pe verticală (uneori nici pe orizontală). Când se aplică tema, celula care ține un element a cărui umplutură crește ar trebui să crească cu exact atâta. La fel când scade. Designerul nu știe de sistemul nostru de teme, deci elementele sunt toate „pe tema clasică”.
> d. Meniul are nevoie de o navigație mică sus, cu trei opțiuni: Sortare, Filtrare și Grupare. Fila de grupare arată opțiunile de grupare, create dinamic din ce e în grilă. Fila se ascunde dacă un comutator nou (`EnableGrouping`) e stins.

**O hotărâre confirmată înainte de a scrie cod:** fila de grupare arată opțiunile pentru **coloana aceasta** (plus ierarhia grilei, ca operatorul să vadă pe al câtelea etaj a nimerit), nu un selector peste toate coloanele. Și `EnableGrouping` stă pe **grilă**, nu pe coloană.

---

## a. Filtrarea: o clauză `Handles` pierdută la o redenumire

`LstValori_ItemCheck` pierduse `Handles lstValori.ItemCheck`: bifele vizibile nu ajungeau în model, iar OK trimitea mulțimea veche. După debifarea «(Selectează tot)» și bifarea unei valori, aceasta rămânea goală, golind grila. «Șterge filtrul» funcționa deoarece nu folosește bifele.

Clauza e la loc, cu un comentariu care spune ce se rupe fără ea, și cu un test care merge pe drumul operatorului (`UncheckAllThenTickOne_FiltersToThatOneValue`), nu prin porțile `Debug*`.

## b. Corpul nu urma fereastra

pnlCorp avea Location=1,1/Size=310×581 din designer, fără andocare. Ajustarea ClientSize lăsa corpul fix și o bandă goală. Dock=Fill, cu rama 1px din Padding-ul formularului, respectă rezumatul existent.

AjusteazaInaltimea citea lstValori.Height înainte de layout, măsurând designerul. Acum, după PerformLayout, toate trei filele folosesc rama corpului (navigație+linie+butoane+margini) + cererea filei active.

Reîncadrare o dată după Show(): tema se aplică abia în KBotThemedForm.OnLoad; Modern poate mări meniul după așezarea inițială, depășind ecranul.

## c. `ThemeTableFit` — rândurile fixe pe măsura schemei

Fișier nou în `KBot.Theming`, lângă `ModernRenderer`.

**Regula:** rândul fix crește numai cu surplusul cerut de conținut peste măsura autorată și revine când surplusul dispare. Nu devine pur și simplu cât conținutul, ceea ce ar pierde spațiul ales de operator.

Trei lucruri au ieșit din implementare și merită reținute:

1. **Baza este instantaneul `Capture`**, nu dimensiunea curentă: altfel comutările Modern ar acumula creșterea fără revenire. Același tipar ca înălțimea autorată din `ModernRenderer` și lățimea autorată a coloanei.

2. **`GetPreferredSize`, nu `Height`:** layout-ul taie butonul mărit de `ModernRenderer` la celula andocată; 56px într-o celulă de 40 raportează 40, ascunzând deficitul. Prima probă a eșuat aici. `KBotTextField.GetPreferredSize` raportează necesarul fontului, deoarece cadrul nu se redimensionează singur, înălțimea fiind autorată.

3. **`Control.Visible` reflectă părinții:** înainte de afișarea formularului, tot tabelul pare ascuns și măsurarea ar ieși goală headless. Rândul realmente ascuns (condițiile pe coloană logică) se strânge explicit prin `SetRowCollapsed`; starea filei este în câmp propriu, nu `pnlX.Visible`.

## d. Trei file, și fila de grupare

KBotNavList orizontal Sortare/Filtrare/Grupare; panouri Fill suprapuse. În designer se lucrează la fila dorită cu Visible=True din proprietăți, ca pagini TabControl, evitând controlul netematizabil.

**Bara OK / Anulează e a FILTRULUI** și se ascunde pe celelalte două file. Asta nu e cosmetică, e consecința celor trei feluri de a preda o hotărâre:

| ce | când se aplică | ce face meniul |
|---|---|---|
| filtrul | la OK, dintr-o copie de lucru | se închide |
| sortarea | imediat | se închide (e o comandă, ca în Access) |
| **gruparea** | **imediat** | **rămâne deschis** |

Gruparea rămâne deschisă fiindcă are șapte opțiuni: o filă care s-ar închide la prima bifă ar trebui redeschisă de șase ori. Operatorul vede grila rearanjându-se în spate.

**Dinamic** înseamnă controale declarate în Designer, conținut din KBotDataView.Groups la deschidere: titlul «Grupează după «Luna»», cele șapte opțiuni și ierarhia cu nivelul curent marcat. Același model ca lista de valori 0028-06.

**Nivelul existent se refolosește**, păstrând culorile/fonturile autorate (`HeaderBackColor`, `FooterFont`…). `SetColumnGroupLevel` înlocuiește la aceeași poziție: schimbarea opțiunilor nu mută nivelul 1 la final și nu schimbă ierarhia.

`EnableGrouping = False` implicit ascunde numai fila. Gruparea autorată în designer poate rămâne activă și indisponibilă operatorului; cele șase vederi livrate nu primesc automat fila.

---

## Ce s-a mai schimbat pe drum

- `btnSortClear` (adăugat de operator în designer) **nu avea niciun handler** — era un buton mort. Acum resetează sortarea și e stins cât timp coloana nu e sortată.
- Separatoarele Label cu SystemColors.ControlLight deveneau invizibile în schemele cu paletă: regula generică Label seta BackColor=Transparent pe linia de 1px. Înlocuite cu Panel colorat din p.BorderColor.
- Fonturile Segoe UI 9 Bold/Italic fixate pe comenzile meniului sunt șterse, fără înlocuire, ca în 0027-02: explicitul bate ApplyBaseFont. Consolas rămâne pe lista valorilor, alegere deliberată pentru alinierea cifrelor.
- Ancora submeniului de condiții se traduce acum prin ecran: butonul stă cu trei părinți mai jos decât `pnlCorp`, deci `Bounds`-ul lui nu mai era în coordonatele corpului.
- Bancul de probă (`DataViewPlaygroundForm`) are `EnableGrouping = True`, ca fila să se poată vedea.

---

## Stare

Build 0 erori. `Controls.Tests` **808** (800 înainte, **+8**); `Theming.Tests` 71 și `DevHarness.Tests` 170 neatinse. Cele 14 picate din `Domain` / `Api` / `App` (etichete de revizie DDF, `IstoricView`, diacritice în `MainForm.Designer`) sunt **anterioare feliei** — verificate pe arborele curat, aceleași nume și același număr.

⚠️ **Deschis — nimic din felia asta n-a fost pe ecran**, iar tocmai lucrurile cerute se judecă privind:

- cum arată bara de file orizontală (înălțime, aer, culoarea elementului selectat) în cele patru scheme, mai ales strâmtă cât meniul;
- dacă rândurile crescute de `ThemeTableFit` sub Modern chiar rezolvă ce se vedea, sau doar mută problema (măsurarea e corectă, proporțiile nu se pot demonstra headless);
- fila de grupare pe o grilă adevărată: sare fereastra între file într-un fel supărător?
- dusul-întorsul prin designerul Visual Studio pe noua ierarhie de panouri (nefăcut).

Rămâne și datoria de la 0029: niciun ecran real nu grupează încă (`IstoricView` / `PlatiView`).
