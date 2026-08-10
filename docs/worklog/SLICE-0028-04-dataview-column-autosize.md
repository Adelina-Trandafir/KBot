# SLICE-0028-04 — DataView: `AutoSizeMode` PE COLOANĂ, care bate modul grilei

Al patrulea pas al feliei 0028 (`KBotDataView`). Cerere de operator, începută de el în cod și
lăsată neterminată, cu TODO-ul scris chiar în capul fișierului de auto-dimensionare:

> i started a new property per column (AutoSize) which takes precedence over the View's own
> AutoSize. you need to finish the implementation in KBotDataView.AutoSize

Ce exista deja în arborele de lucru: `KBotDataColumn.AutoSizeMode` (câmp + proprietate +
`Owner?.OnColumnAutoSizeModeChanged()`, cu `KBotDataView.OnColumnAutoSizeModeChanged` care cere
re-layout) și TODO-ul din `KBotDataView.AutoSize.vb`. Pasul de măsurare NU citea proprietatea:
`PerformAutoSize` întreba în continuare doar `_autoSizeMode`, deci proprietatea era, până aici,
un buton care nu era legat la nimic.

---

## 1. Decizia care trebuia luată înainte de cod: ce înseamnă „precedență”

Proprietatea nouă pornea de la `KBotAutoSizeMode.None` ca implicit. **Precedență + implicit
`None` = nicio coloană nu se mai măsoară**, oricât ar spune grila: `AutoSizeColumnsMode` este
`ToContent` din construcție, iar cele cinci vederi/gazde care îl setează explicit
(`DdfFileBrowser`, `XfaXmlPreview`, `Plati`, `Rezervari`, `Receptii`) și-ar fi pierdut
dimensionarea în tăcere, în ziua în care cineva ar fi construit prima coloană. Un implicit care
răstoarnă comportamentul întregii aplicații nu e un implicit, e o regresie cu întârziere.

Așa că vocabularul a primit **al treilea membru, `KBotAutoSizeMode.Inherit = -1`**, care e acum
implicitul COLOANEI: «n-am nicio părere, hotărăște grila». E același tipar cu `Color.Empty` = «din
temă» de la arbore (felia 0027) — o valoare-santinelă pentru „nespus”, ca „setat explicit” să
poată câștiga fără ca „neatins” să însemne ceva.

Consecințe, toate deliberate:

- **`Inherit` e doar al coloanei.** Pe `AutoSizeColumnsMode` (grila) valoarea **ARUNCĂ**
  `ArgumentException`: deasupra grilei nu e nimeni de la cine să moștenească, deci a o accepta ar
  însemna să o traducem tăcut în altceva — exact no-op-ul tăcut interzis de regula casei. Tot
  acolo se refuză și o valoare care nu e în enum (`[Enum].IsDefined`), pe amândouă proprietățile.
- **Numerotarea veche a rămas neatinsă** (`None = 0`, `ToContent = 1`), fiindcă bancul de probă
  leagă `cboAutoSize.SelectedIndex` direct de valoarea enum-ului; de aceea `Inherit` e `-1` și nu
  `0`. Lista de pe COLOANĂ, care începe cu `Inherit`, are de aceea traducere proprie
  (`ColumnModeAt` / `IndexOfColumnMode`) — indexul ei nu mai e valoarea enum-ului.
- `<DefaultValue(KBotAutoSizeMode.Inherit)>` pe proprietatea coloanei: designerul nu scrie nimic
  pentru o coloană neatinsă.

## 2. Pasul de măsurare (`KBotDataView.AutoSize.vb`)

Un singur loc citește precedența, și e numit ca atare:

```vb
Friend Function EffectiveAutoSizeMode(col As KBotDataColumn) As KBotAutoSizeMode
    If col Is Nothing Then Return _autoSizeMode
    If col.AutoSizeMode = KBotAutoSizeMode.Inherit Then Return _autoSizeMode
    Return col.AutoSizeMode
End Function
```

Două schimbări în pas:

- **Pasul 1 (măsurarea) întreabă per coloană**, nu o dată pe grilă. `If _autoSizeMode = ToContent`
  de dinaintea buclei a devenit un `If EffectiveAutoSizeMode(c) <> ToContent Then Continue For`
  ÎN buclă, lângă garda de `UserSized`.
- **Poarta de la intrarea în `PerformAutoSize` s-a mutat de pe grilă pe coloane.** Înainte era
  «grila e pe None ȘI n-avem umplere ȘI n-avem auto-hide ⇒ ieși». Acum se întreabă și
  `AnyColumnSizesToContent()`, altfel o coloană care cere singură măsurarea, pe o grilă manuală,
  n-ar fi fost măsurată niciodată — adică fix aranjamentul pentru care există proprietatea. (Poarta
  ignoră coloanele ascunse și pe cele trase de operator: pe acelea pasul oricum nu le atinge, deci
  o pornire pentru ele ar fi muncă degeaba.)

**Precedența acoperă DOAR măsurarea.** `ColumnFillMode` e alt buton și rămâne așa: umplerea /
strâmtarea trec în continuare peste toate coloanele vizibile, inclusiv peste una fixată pe `None`
— exact regula care se aplică deja unei coloane trase cu mouse-ul (`UserSized`), pe care măsurarea
o sare, dar umplerea o mișcă. O coloană care nu are voie să se miște deloc se prinde în cuie cu
`MinWidth = MaxWidth`, unde clamparea o ține. Regula asta e scrisă acum și în capul fișierului,
în locul TODO-ului.

**Precedența e față de GRILĂ, nu față de operator**: `UserSized` bate în continuare tot, iar
`ResetColumnSizing()` e cel care redă coloana măsurării.

## 3. Bancul de probă

Inspectorul de coloană din `DataViewPlaygroundForm` a primit lista `AutoSizeMode (bate modul
grilei)`, cu cele trei valori. Tot acolo s-a corectat o activare care de acum minte: „Width” era
editabil după steagul GRILEI (`toContent`), deși coloana selectată poate spune altceva — se
întreabă modul EFECTIV al coloanei selectate (`SelectedColumnSizesToContent`).

## Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/KBotAutoSizeMode.vb` | membrul `Inherit = -1` + de ce e santinelă doar a coloanei |
| `src/KBot.Controls/DataView/KBotDataColumn.vb` | implicitul proprietății pe `Inherit`, `<DefaultValue>`, documentația precedenței, refuzul valorilor din afara enum-ului |
| `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` | `EffectiveAutoSizeMode`, `AnyColumnSizesToContent`, poarta și bucla de măsurare, refuzul lui `Inherit` pe grilă, capul de fișier (TODO-ul operatorului → explicație) |
| `src/KBot.DevHarness/Internal/DataViewPlaygroundForm(.Designer).vb` | lista `AutoSizeMode` în inspectorul de coloană + activarea corectă a „Width” |
| `tests/KBot.Controls.Tests/KBotDataViewColumnAutoSizeTests.vb` | **nou**, 9 teste |

**Versiuni**: `KBot.Controls` 1.14.0.0 → **1.15.0.0**; `KBot.DevHarness` 1.0.17.0 → **1.0.18.0**.

## Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori, 0 avertismente noi (MSB3825 pe DdfView.resx, preexistent)
KBot.Controls.Tests      → 694 / 694   (685 înainte de pas, +9)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
```

Cele 9 teste noi fixează: implicitul `Inherit`, coloana `None` care bate grila `ToContent`,
coloana `ToContent` care bate grila `None`, pornirea pasului pe o grilă complet manuală, re-layout-ul
din setter, `UserSized` care rămâne deasupra tuturor (și cade la `ResetColumnSizing`), umplerea care
mișcă și o coloană `None`, plus cele două refuzuri (`Inherit` pe grilă, valoare din afara enum-ului).

### Eșecuri PREEXISTENTE, neatinse de pas

Aceleași ca la 0028-03, verificate din nou **într-un worktree curat la HEAD**, nu presupuse:
`KBot.Domain.Tests` 3 și `KBot.Api.Tests` 1 (`EtichetaRevizie*`) eșuează identic la HEAD;
`KBot.App.Tests` are 10 în arborele de lucru, dintre care 2 eșuează și la HEAD, iar restul de 8 vin
din rescrierea ÎN LUCRU a vederilor (`DdfView` — `GetMethod("tree_NodeMouseUp")` întoarce `Nothing`;
`IstoricView` — coloana `obs`; captionul de navigare «Doc. Fundamentare»). Niciunul nu atinge
auto-dimensionarea.

## Rămase neverificate / amânate

- **N-a fost văzut pe ecran**, ca tot restul feliei 0028. Locul unde se probează e lista nouă din
  inspectorul de coloană al bancului (`DataViewPlaygroundForm`).
- **Nicio vedere nu folosește încă proprietatea.** Cele cinci gazde care setează
  `AutoSizeColumnsMode = ToContent` au rămas exact cum erau; alegerea coloanelor care merită fixate
  (de obicei cele numerice, ca lățimea lor să nu sară de la o încărcare la alta) e a operatorului.
- **Umplerea rămâne grid-wide.** Un `FillWeight` pe coloană n-a fost cerut și nu s-a făcut; azi
  singurul mod de a scoate o coloană complet din mișcare e `MinWidth = MaxWidth`.
