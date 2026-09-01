# SLICE-0048-09 — totalul recepțiilor scris chiar pe reperul plății

Continuarea directă a feliei `SLICE-0048-08`. O singură cerere a operatorului, pe 01.09.2026, după
ce a privit benzile de așezare în vederea «benzi»:

> «când trec cu mausul peste o bandă verticală (plățile) vreau să văd în etichetă Totalul
> Recepțiilor până la data aia (adică totalul ultimei stări a fiecărui R) și diferența dintre ele,
> ca să văd ușor dacă H-urile sunt bine puse sau nu.»

**Stare:** cod verde — `dotnet build KBot.sln` **0 erori / 0 avertismente**. `KBot.App.Tests`
**188 trecute / 13 picate** din 201; cele 13 roșii sunt **exact aceleași preexistente și străine**
de felia asta (`DdfViewTests` 3, `DdfXfaParserTests` 2, `IstoricViewTests` 6,
`MainFormNavItemsTests` 1, `XfaXmlPreviewTests` 1) — linia de plecare era 185/198, deci **cele 3
teste noi trec toate trei** și nimic nu s-a mișcat în rest. `KBot.Controls` neatins. Python neatins.

**S-A VĂZUT PE ECRAN**, parțial: eticheta a fost randată offline (sondă temporară în scratchpad,
ștearsă) prin chiar motorul care o desenează în aplicație — `KBotRichText.Parse` ▸ `Layout` ▸
`Draw`, adică drumul pe care îl ia `KBotToolTipWindow`. Se confirmă că `<b>` iese îngroșat (nu
literal), că cele patru linii de cifre se citesc ca o socoteală și că avertismentul de jos, despărțit
de un rând gol, nu se amestecă cu ele. Ce **NU** s-a văzut: eticheta ridicată de un mouse adevărat
peste o bandă adevărată, cu date de la server.

**Nimic nu a rulat pe MariaDB.** Nicio rută Python atinsă, nicio schemă schimbată, niciun contract
de fir schimbat — toată socoteala se face din ce era deja în `AsociereStare`.

---

## 1. Ce a fost și ce este

Reperul plății (`KBotChartGuide`, câte unul pe benzi și pe grafic) purta în corp o singură
propoziție, aceeași pentru toate plățile:

> «Totalul recepțiilor la data asta a intrat în ordonanțare.»

Propoziția aia spunea CE se întâmplă — lucru pe care operatorul îl știa oricum, e chiar meseria
lui. Ce nu avea era **cifra**. Acum corpul e socoteala §1.3 din
`docs/FUNDAMENT_Asociere_Receptii.md`, făcută pe tabloul LOCAL, la data plății:

```
OP 7                                        (doar dacă există număr de OP)
Total recepții la data plății: 200,00       (îngroșat — e cifra pentru care s-a deschis eticheta)
Plăți anterioare: 100,00                    (doar dacă există plăți înainte)
Plata asta: 50,00
Diferență (recepții - plăți): 50,00

1 instantanee neașezate până la data asta — totalul de mai sus nu le cuprinde.
```

**Totalul** = suma ultimului instantaneu al fiecărei recepții de la sau dinaintea datei plății —
exact `ValoareaLa`, funcția pe care o folosea deja linia «Total angajament» din grafic. Nu s-a
scris o a doua aritmetică: dacă linia din grafic și cifra din etichetă s-ar putea contrazice
vreodată, ele n-ar mai fi două înfățișări ale aceluiași fapt, iar operatorul ar avea de ales între
ele fără niciun temei.

**De ce apar și plățile anterioare, deși nu s-au cerut.** Fără ele diferența n-ar avea niciun prag
de citit: la un angajament plătit perfect, prima plată ar da zero și toate celelalte nu. Cu ele,
diferența e `Ramas` din tabelul §1.3 — ce mai are angajamentul de plătit la momentul ăla — deci un
număr care înseamnă ceva prin el însuși. E singura adăugire peste cerere; o linie, ștearsă ușor
dacă operatorul o găsește de prisos.

**Ce e greșit și ce e doar rest.** O diferență pozitivă e obișnuită: recepții încă neplătite. O
diferență **negativă** nu poate fi rest — ar însemna că s-a plătit mai mult decât arătau recepțiile
atunci — deci ori un instantaneu care trebuia să cadă înaintea plății stă acum după ea, ori stă pe
recepția greșită. **Doar aia se colorează** (`ErrorColor` din paletă, ca marcaj `<color=#RRGGBB>`).
O culoare pusă și pe un rest normal ar învăța ochiul în două zile să nu se mai uite la culoare.

**Instantaneele neașezate se numără** fiindcă nu intră în niciun lanț, deci nu intră nici în total.
Cu ele pe jos, cifra de deasupra e provizorie — iar asta trebuie spus **acolo unde se citește
cifra**, nu ghicit privind banda de jos.

## 2. Unde se vede

Corpul e construit o singură dată pe plată și se pune pe AMÂNDOUĂ reperele — cel de pe benzi și cel
din grafic — ca și până acum. Deci:

- banda strâmtă din `AsociereForm` (vederea «benzi»),
- fereastra mare `AsociereBenziForm`,
- graficul din `AsociereForm` (vederea «grafic»), care primește aceleași repere.

Și, esențial: **se reface la fiecare tragere.** `ConstruiesteReperelePlatilor` se cheamă din
`ConstruiesteBenzi`, care se cheamă din `Reconstruieste` — adică după fiecare mutare, pe amândouă
suprafețele. Fără asta cifra ar răspunde la întrebarea de acum două trageri, ceea ce e mai rău
decât să nu răspundă deloc.

## 3. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Forexe/AsociereForm.vb` | `ConstruiesteReperelePlatilor` calculează lanțurile o SINGURĂ dată (nu unul pe plată) și adună `platiAnterioare` pe parcurs, în ordinea datelor — chiar definiția lui `PlatiAnt`. Două metode noi: `CorpulReperului` (socoteala + textul) și `LiniaDiferentei` (colorează doar diferența imposibilă). |
| `tests/KBot.App.Tests/AsociereFormTests.vb` | Trei teste noi + două ajutoare (`Banda`, `Reper`) și două tablouri de probă. |

## 4. Teste

Trei teste noi, toate pe firul STA existent, fără ecran:

1. `ReperulPlatii_ScrieTotalulReceptiilorAsaCumStateaLaDataAia` — chiar tabelul §1.3: la prima
   plată intră în total DOAR instantaneul de dinaintea ei, iar linia «Plăți anterioare» lipsește cu
   totul (nu scrie «0,00»).
2. `ReperulPlatii_ScadePlatileDeDinainteaLui` — a doua plată: total 200, anterioare 100, plata 50,
   diferență 50. Exact rândul 04/01 din tabelul fundamentului.
3. `ReperulPlatii_SeRefaceLaFiecareTragere_SiSpuneCateAuRamasPeJos` — cifra se schimbă (300 ▸ 350)
   când un instantaneu neașezat e tras pe o recepție, iar avertismentul despre neașezate dispare
   odată cu ultimul. **Ăsta e testul care apără rostul cifrei**, nu doar valoarea ei.

`dotnet test tests/KBot.App.Tests` ▸ 188 trecute / 13 picate (cele 13 preexistente, enumerate mai
sus). `dotnet build KBot.sln` ▸ 0 erori, 0 avertismente.

## 5. Neverificat / amânat

- **Eticheta ridicată de un mouse adevărat**, peste o bandă adevărată, cu date de la server. S-a
  văzut textul randat prin motorul lui, nu fereastra plutitoare pe ecran.
- **Nicio dată reală.** Toate cifrele de mai sus vin din tablouri de probă scrise după tabelul
  §1.3. Prima confruntare cu un angajament viu poate arăta că `Plati` sosește altfel decât
  presupune ordonarea după dată (de pildă două plăți în aceeași zi, care aici se despart doar prin
  ordinea din listă, fiindcă `PlataAsociere` nu are un identificator propriu).
- **Culoarea diferenței negative** n-a fost văzută: pentru ea trebuie un tablou în care plățile
  depășesc recepțiile la o dată, iar testele nu o acoperă fiindcă `ThemeManager.Current` poate fi
  `Nothing` fără ecran și atunci linia iese necolorată, pe drept.
