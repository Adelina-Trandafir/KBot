# SLICE-0028-05 — DataView: cine deține lățimea unei coloane (designer vs trecere de layout)

Al cincilea pas al feliei 0028, pornit de la un raport de operator pe o vedere reală:

> look at the IstoricView. I tried to make the first three columns NO Autosize and only the last
> one to strech, but no matter what i do, the first column becomes huge. i want the first columns
> to be 300px, the second and third 200px and the last one to fill the difference

„No matter what I do” era exact adevărul: **două defecte diferite** îi anulau, pe rând, fiecare
încercare. Niciunul nu se vede citind vederea — se văd doar din numere.

---

## 1. Ce spuneau numerele

Starea din `IstoricView.Designer.vb` la momentul raportului:

```vb
grid.AutoSizeColumnsMode = KBotAutoSizeMode.None       ' fără măsurare
' grid.ColumnFillMode : ABSENT  => None, deci nimeni nu se întinde
KBotDataColumn1.MaxWidth = 123456                       ' încercare de plafon
KBotDataColumn1.MinWidth = 300
KBotDataColumn1.Width    = 747                          ' ← „coloana uriașă”
KBotDataColumn2.Width    = 40                           ' exact MinWidth
KBotDataColumn3.Width    = 55
KBotDataColumn4.Width    = 144
```

`747 + 40 + 55 + 144 = 986`, adică **fix `grid.Size.Width` de pe suprafața de designer**. Nimeni
nu tastează patru numere care să dea exact lățimea formularului: valorile alea sunt **ieșirea unei
treceri de umplere**, scrise în fișier ca și cum le-ar fi ales operatorul. (Verificat, nu presupus:
tiparul `40 = MinWidth` și restul până la suma exactă nu poate veni din altă parte.)

## 2. Defectul 1 — trecerea de layout rula ÎN DESIGNER și-i autora fișierul

`PerformAutoSize` scrie `Width`. Designerul serializează pe urmă ce găsește. Deci orice deschidere
a formularului în Visual Studio putea îngheța în `.Designer.vb` lățimea calculată pentru suprafața
aceea, la mărimea aceea — după care valoarea rămâne acolo pentru totdeauna, ca alegere a
operatorului, și supraviețuiește layout-ului care a produs-o.

E aceeași capcană pe care regula casei o descrie pentru `ShouldSerialize*`, cu un etaj mai jos:
acolo e vorba de valoarea implicită a unei proprietăți, aici de **ieșirea unei treceri de layout**,
iar o trecere de layout n-are ce căuta autorând formularul.

**Reparat:** `PerformAutoSize` iese imediat sub `KBotDesignTime.IsDesignTime(Me)`. În designer se
văd lățimile **așa cum au fost autorate** (ceea ce, pentru autorare, e și mai onest); măsurarea și
umplerea se întâmplă la rulare.

## 3. Defectul 2 — strâmtarea era DISTRUCTIVĂ

Al doilea, invizibil în fișier și cel care ar fi lovit imediat după prima reparație: `IstoricView`
se construiește într-un `SplitContainer` îngust (≈690 px) și abia pe urmă ajunge la mărimea
ferestrei. La 690 px, `ShrinkToFit` mușca din coloane (200 → 120), scriindu-le peste `Width`. Când
grila se lărgea, `DistributeLeftover` putea crește **doar ținta umplerii**, deci cele două coloane
rămâneau strâmtate **pentru tot restul sesiunii**. Operatorul cere 200 și vede 120: „proprietatea
nu merge”.

Cauza reală: trecerea își compunea propria ieșire — rezultatul depindea de cum se nimerise
redimensionată fereastra, nu de ce ceruse caller-ul.

**Reparat în model, nu în trecere:** `KBotDataColumn` ține acum, pe lângă `_width` (cea pictată),
și `_authoredWidth` — lățimea CERUTĂ, scrisă doar de setterul public `Width` (designer, cod, sau
tragerea operatorului). Trecerea scrie prin `SetLayoutWidth` (nu atinge lățimea cerută) și își
începe fiecare rulare cu `RestoreAuthoredWidths()`, exact ca `ClearAutoHiddenState()` de alături.
Deci **o trecere e o funcție de (lățimi cerute, spațiu disponibil)** și de nimic altceva.

O coloană trasă de operator e sărită de la restaurare: tragerea aceea E lățimea cerută de acum
(setterul public), și ține până la `ResetColumnSizing()`.

## 4. Vederea

`IstoricView.Designer.vb`: `ColumnFillMode = LastColumn` (lipsea — de asta nu se întindea nimic),
`MaxWidth = 123456` șters, iar lățimile puse pe ce a cerut operatorul: **300 / 200 / 200 / 250**
(ultima e doar punctul de plecare — ea ia diferența). Cele patru `AutoSizeMode = None` autorate de
operator rămân: acum chiar înseamnă ceva (felia 0028-04).

Rezultatul, măsurat pe vederea reală (diagnostic temporar, șters după):

```
la 690 px  (cât e la construcție): 300 / 120 / 120 / 150   → strâmtate, fără bară orizontală
la 1600 px (fereastra adevărată):  300 / 200 / 200 / 900   → cerute înapoi + ultima umple
```

## Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/KBotDataColumn.vb` | `_authoredWidth`, `SetLayoutWidth`, `RestoreAuthoredWidth`; `Width`/`MinWidth`/`MaxWidth` țin baseline-ul la zi |
| `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` | ieșire sub `IsDesignTime`, `RestoreAuthoredWidths()` la începutul trecerii, toate scrierile trecerii pe `SetLayoutWidth` |
| `src/KBot.App/Views/IstoricView.Designer.vb` | `ColumnFillMode = LastColumn`, `MaxWidth = 123456` scos, lățimi 300/200/200/250 |
| `tests/KBot.Controls.Tests/KBotDataViewAuthoredWidthTests.vb` | **nou**, 5 teste |

**Versiuni**: `KBot.Controls` 1.15.0.0 → **1.16.0.0**; `KBot.App` 1.0.13.0 → **1.0.14.0**.

## Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Controls.Tests      → 699 / 699   (694 înainte, +5)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
Common 14/14 · Xfa 39/39 · LocalStore 1/1
```

Cele 5 teste noi: în designer nu se strâmtează și nu se măsoară nimic (site fals cu
`DesignMode = True`), strâmtarea nu mai e distructivă (îngust → lat readuce 300/200/200), trecerea
e idempotentă la aceeași lățime disponibilă, iar o coloană trasă rămâne a operatorului.

Eșecurile din `Domain` (3), `Api` (1) și `App` (10) sunt aceleași dinainte de pas — preexistente,
verificate la HEAD într-un worktree curat la comiterea precedentă.

## Rămase neverificate / amânate

- **Tot nevăzut pe ecran.** Numerele de mai sus vin dintr-un test headless pe `IstoricView`, nu
  dintr-o rulare a aplicației.
- **Celelalte vederi pot avea aceeași murdărie în `.Designer.vb`.** Defectul 1 a lucrat pentru
  oricare formular deschis în designer, deci `SumarView` / `PlatiView` / `Rezervari` / `Receptii` /
  `DdfView` pot conține lățimi calculate, înghețate. N-au fost atinse aici: fiecare cere privit ce
  voia operatorul de la coloanele ei, nu o corectură în masă. De acum înainte nu se mai pot murdări.
- **Strâmtarea rămâne grid-wide**: la spațiu insuficient se ia din TOATE coloanele vizibile,
  inclusiv din cele cu `AutoSizeMode = None` (regula scrisă în 0028-04: precedența acoperă doar
  măsurarea). Cine vrea o coloană complet nemișcată o prinde cu `MinWidth = MaxWidth`. Dacă
  operatorul vrea altă regulă — «strâmtarea ia doar din ținta umplerii» — e o schimbare de
  contract, cu două teste existente care o fixează pe cea de azi (`Overflow_WithFillMode_…`,
  `Overflow_MinWidthsExceedViewport_…`).
