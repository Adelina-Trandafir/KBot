# SLICE-0037 — Grila: coloana care se întinde poate fi ORICARE, nu doar prima sau ultima

Cerere de operator:

> «pentru dgv: vreau să pot alege și altă coloană (în afară de prima sau ultima) pentru autofill»

## De ce lipsea

`ColumnFillMode` (felia 0013) numea ținta prin POZIȚIE: `FirstColumn` / `LastColumn`, plus
`Proportional`, care nu numește pe nimeni fiindcă le mișcă pe toate. Într-o grilă cu un cod îngust
la stânga și sume la dreapta, coloana care trebuie să crească e descrierea din MIJLOC — și nu
exista niciun fel de a o cere. `Proportional` nu e răspunsul: ar lăți și coloanele de sume, care
au deja exact lățimea cifrelor lor.

## Ce s-a schimbat

**`KBotFillMode.SpecificColumn` (=4)** și, lângă el, **`KBotDataView.FillColumnKey`** — cheia
coloanei-țintă. Trei alegeri merită scrise, fiindcă niciuna nu e evidentă:

1. **Cheie, nu poziție.** Un index ar arăta către altceva după prima reordonare sau ascundere de
   coloană; `Key` e deja moneda cu care vorbesc gruparea (`KBotGroupLevel.ColumnKey`), filtrarea și
   sortarea. Aceeași monedă peste tot înseamnă că o coloană mutată nu mută întinderea.
2. **Cheie greșită = zgomot la `EndInit`, nu la fiecare pictare.** `ValidateFillColumn` stă lângă
   `ValidateGroupLevels`, se sare în designer (o excepție din `InitializeComponent` înseamnă un
   formular care nu se mai deschide) și reclamă atât cheia GOALĂ, cât și una necunoscută: un mod de
   umplere care n-are ce umple e o greșeală de model, nu o stare. O trecere de layout n-are voie să
   arunce pentru asta — s-ar loga la fiecare redimensionare a ferestrei.
3. **Coloana ascunsă ACUM nu e o greșeală, e o stare.** Ținta se caută printre coloanele vizibile;
   dacă lipsește, nu crește nimeni și spațiul rămâne gol, ca la `None`. **Nu** se cade pe ultima
   coloană — ar întinde taman coloana pe care operatorul a ocolit-o alegând alta.

Ținta trece prin `FillTargetColumn`, deci moștenește gratis și **protecția față de auto-ascundere**
(felia 0016): coloana aleasă nu poate fi ascunsă ca să încapă restul, exact cum se poartă azi ținta
`FirstColumn`/`LastColumn`. Întinderea bate ascunderea, ca și până acum.

Pe drum: setter-ul lui `ColumnFillMode` **validează acum enum-ul**. Înainte accepta tăcut orice
`CType(99, KBotFillMode)` și grila se purta ca la `None` — exact no-op-ul tăcut pe care regula
casei îl interzice. `AutoSizeColumnsMode` avea garda dintotdeauna; sora lui nu.

## Cum se folosește

```vb
grid.ColumnFillMode = KBotFillMode.SpecificColumn
grid.FillColumnKey = "Descriere"
```

Ambele sunt proprietăți de designer (categoria «K-BOT»), deci se pot autora în `.Designer.vb` —
`FillColumnKey` poate fi scrisă și înainte de a comuta modul, fiindcă în celelalte moduri e
metadata inertă.

## Ce NU s-a făcut

- **Fără verdict vizual** — modul nu a fost văzut pe o grilă adevărată; e verificat prin compilare.
- **Fără teste** (cerute explicit sărite la această trecere). Rămân de scris cel puțin: ținta din
  mijloc primește tot restul, ținta ascunsă nu întinde pe nimeni, cheia necunoscută aruncă la
  `EndInit`, ținta e protejată de auto-ascundere.
- **Nicio vedere existentă nu a fost trecută pe noul mod** — `SumarView`/`IstoricView`/`DdfView`
  rămân cum erau.
