# SLICE-0030 — Meniul de coloană: trei file, rânduri pe măsura temei, și bifele care se pierduseră

**Cerere operator** (patru puncte, după ce a rearanjat singur `KBotFilterPopup` cu un
`TableLayoutPanel`):

> a. Filtrarea nu mai merge — arată o grilă goală. Resetarea încă merge.
> b. Fereastra nu se redimensionează corect.
> c. Pe tema modernă, din cauza umpluturii în plus, controalele arată prost — nu e destul spațiu
>    pe verticală (uneori nici pe orizontală). Când se aplică tema, celula care ține un element
>    a cărui umplutură crește ar trebui să crească cu exact atâta. La fel când scade. Designerul
>    nu știe de sistemul nostru de teme, deci elementele sunt toate „pe tema clasică”.
> d. Meniul are nevoie de o navigație mică sus, cu trei opțiuni: Sortare, Filtrare și Grupare.
>    Fila de grupare arată opțiunile de grupare, create dinamic din ce e în grilă. Fila se ascunde
>    dacă un comutator nou (`EnableGrouping`) e stins.

**O hotărâre confirmată înainte de a scrie cod:** fila de grupare arată opțiunile pentru
**coloana aceasta** (plus ierarhia grilei, ca operatorul să vadă pe al câtelea etaj a nimerit), nu
un selector peste toate coloanele. Și `EnableGrouping` stă pe **grilă**, nu pe coloană.

---

## a. Filtrarea: o clauză `Handles` pierdută la o redenumire

`LstValori_ItemCheck` rămăsese fără `Handles lstValori.ItemCheck`. Consecința nu e „filtrul nu
merge”, ci ceva mai rău, fiindcă arată corect pe ecran: bifele puse cu mouse-ul nu ajungeau
niciodată în model, deci la OK se preda mulțimea de dinaintea oricărui clic. Pe drumul obișnuit —
«(Selectează tot)» stinge tot, apoi se bifează una singură — mulțimea aia e **goală**, adică o
grilă goală. «Șterge filtrul» continua să meargă fiindcă el nu trece prin bife deloc.

Clauza e la loc, cu un comentariu care spune ce se rupe fără ea, și cu un test care merge pe drumul
operatorului (`UncheckAllThenTickOne_FiltersToThatOneValue`), nu prin porțile `Debug*`.

## b. Corpul nu urma fereastra

`pnlCorp` nu era andocat: stătea la `Location = 1,1` cu `Size = 310×581`, scris de designer. Orice
re-măsurare pe înălțime schimba `ClientSize` și lăsa corpul unde era — banda goală de sub controale.
Acum e `Dock = Fill`, iar rama de 1px vine din `Padding`-ul formularului, cum spunea deja
rezumatul fișierului.

A doua jumătate: `AjusteazaInaltimea` citea `lstValori.Height` **înainte** de vreun layout, deci
măsura ce scrisese designerul, nu ce era pe ecran. Acum formula e una singură pentru toate cele
trei file — *rama corpului* (navigație + linie + bara de butoane + margini) plus *cât cere fila
activă* — și se aplică după un `PerformLayout`.

Și: fereastra se re-verifică **o dată după `Show()`**, fiindcă tema se aplică abia în
`KBotThemedForm.OnLoad` — pe Modern meniul poate ieși mai înalt decât cel așezat cu două rânduri
mai sus, iar un meniu care iese pe sub marginea ecranului nu se mai poate citi până la capăt.

## c. `ThemeTableFit` — rândurile fixe pe măsura schemei

Fișier nou în `KBot.Theming`, lângă `ModernRenderer`.

**Regula, într-o propoziție:** un rând fix crește cu EXACT surplusul pe care conținutul lui îl cere
peste măsura AUTORATĂ, și se întoarce la măsura autorată când surplusul dispare. Nu „rândul devine
cât conținutul” — atunci aerul ales de operator între controale s-ar pierde la prima comutare de
schemă.

Trei lucruri au ieșit din implementare și merită reținute:

1. **Baza e un instantaneu, nu mărimea de acum.** Fără `Capture`, a doua comutare de schemă ar
   măsura peste rezultatul primeia și rândurile ar crește la fiecare trecere prin Modern, fără să
   se mai întoarcă niciodată. Același tipar ca înălțimea autorată a unui buton din `ModernRenderer`
   și ca lățimea autorată a unei coloane de grilă.

2. **Se întreabă `GetPreferredSize`, NU `Height`** — și asta a fost miezul. `ModernRenderer` chiar
   crește butonul ca să-i încapă umplutura schemei, dar înăuntrul unui `TableLayoutPanel` creșterea
   aia nu se vede niciodată: motorul de așezare taie orice control andocat la dreptunghiul celulei
   la primul layout de după. Un buton de 56px într-o celulă de 40 **raportează 40**, deci o
   măsurare care s-ar uita la mărimea lui ar afla mereu că totul încape perfect. Prima versiune a
   testului a picat exact pe asta.
   Corolar: `KBotTextField` a primit un `GetPreferredSize` care spune cinstit cât îi trebuie pe
   fontul curent — cadrul nu se redimensionează singur (înălțimea lui e a designerului), deci ăsta
   era singurul loc unde putea răspunde la «mai încape textul?».

3. **`Control.Visible` nu poate fi întrebat de nimeni.** Getter-ul răspunde despre lanțul de
   părinți, deci pe un formular încă nearătat TOT ce e în tabel raportează `False` — o măsurare
   care ar sări controalele „ascunse” ar ieși goală headless. De aceea un rând care chiar trebuie
   să dispară (butonul de condiții pe o coloană logică) se strânge **explicit**, prin
   `SetRowCollapsed`. Aceeași capcană a mutat starea filei din `pnlX.Visible` într-un câmp.

## d. Trei file, și fila de grupare

Bara e un `KBotNavList` orizontal (`Sortare` / `Filtrare` / `Grupare`); fiecare filă e un `Panel`
andocat `Fill` peste același loc. Pe suprafața de proiectare stau una peste alta — ca să lucrezi la
una îi pui `Visible = True` din grila de proprietăți, exact ca la paginile unui `TabControl`, doar
că fără `TabControl`-ul netematizabil.

**Bara OK / Anulează e a FILTRULUI** și se ascunde pe celelalte două file. Asta nu e cosmetică, e
consecința celor trei feluri de a preda o hotărâre:

| ce | când se aplică | ce face meniul |
|---|---|---|
| filtrul | la OK, dintr-o copie de lucru | se închide |
| sortarea | imediat | se închide (e o comandă, ca în Access) |
| **gruparea** | **imediat** | **rămâne deschis** |

Gruparea rămâne deschisă fiindcă are șapte opțiuni: o filă care s-ar închide la prima bifă ar
trebui redeschisă de șase ori. Operatorul vede grila rearanjându-se în spate.

**„Dinamic din ce e în grilă”** înseamnă: controalele sunt ale designerului (regula casei), dar
NIMIC din ce scrie pe ele nu e — titlul («Grupează după «Luna»»), starea celor șapte opțiuni,
ierarhia de niveluri cu coloana curentă marcată, toate se citesc din `KBotDataView.Groups` la
deschidere. E aceeași împărțire ca la lista de valori de la 0028-06.

**Nivelul EXISTENT se refolosește, nu se înlocuiește:** pe el pot sta culori și fonturi puse din
designer (`HeaderBackColor`, `FooterFont`…), iar o bifă din meniu n-are voie să le șteargă. Și
`SetColumnGroupLevel` înlocuiește pe LOCUL LUI din ierarhie — o schimbare de opțiuni n-are voie să
mute coloana de pe nivelul 1 pe ultimul, fiindcă ordinea nivelurilor E ierarhia.

`EnableGrouping` e **implicit stins**, și gatează DOAR fila: o grilă poate porni grupată din
designer cu steagul stins, și atunci gruparea e a machetei, nu una pe care operatorul o poate
desface. Cele șase vederi livrate nu capătă nicio filă nouă peste noapte.

---

## Ce s-a mai schimbat pe drum

- `btnSortClear` (adăugat de operator în designer) **nu avea niciun handler** — era un buton mort.
  Acum resetează sortarea și e stins cât timp coloana nu e sortată.
- Cele două linii despărțitoare erau `Label`-uri cu `SystemColors.ControlLight` scris în designer.
  Sub o schemă cu paletă, regula generică de `Label` pune `BackColor = Transparent` — adică o linie
  de 1px care nu se mai vede deloc. Sunt `Panel`-uri acum, colorate din `p.BorderColor`.
- Fonturile «Segoe UI, 9 Bold/Italic» pinuite pe rândurile de meniu au fost **șterse, nu
  rescrise** — aceeași hotărâre ca la arbore (0027-02): un font scris explicit bate `ApplyBaseFont`,
  deci schema n-ar mai avea ce schimba. `Consolas` de pe lista de valori a rămas: acolo e o alegere
  deliberată (cifrele se aliniază), nu o inerție.
- Ancora submeniului de condiții se traduce acum prin ecran: butonul stă cu trei părinți mai jos
  decât `pnlCorp`, deci `Bounds`-ul lui nu mai era în coordonatele corpului.
- Bancul de probă (`DataViewPlaygroundForm`) are `EnableGrouping = True`, ca fila să se poată vedea.

---

## Stare

Build 0 erori. `Controls.Tests` **808** (800 înainte, **+8**); `Theming.Tests` 71 și
`DevHarness.Tests` 170 neatinse. Cele 14 picate din `Domain` / `Api` / `App` (etichete de revizie
DDF, `IstoricView`, diacritice în `MainForm.Designer`) sunt **anterioare feliei** — verificate pe
arborele curat, aceleași nume și același număr.

⚠️ **Deschis — nimic din felia asta n-a fost pe ecran**, iar tocmai lucrurile cerute se judecă
privind:

- cum arată bara de file orizontală (înălțime, aer, culoarea elementului selectat) în cele patru
  scheme, mai ales strâmtă cât meniul;
- dacă rândurile crescute de `ThemeTableFit` sub Modern chiar rezolvă ce se vedea, sau doar mută
  problema (măsurarea e corectă, proporțiile nu se pot demonstra headless);
- fila de grupare pe o grilă adevărată: sare fereastra între file într-un fel supărător?
- dusul-întorsul prin designerul Visual Studio pe noua ierarhie de panouri (nefăcut).

Rămâne și datoria de la 0029: niciun ecran real nu grupează încă (`IstoricView` / `PlatiView`).
