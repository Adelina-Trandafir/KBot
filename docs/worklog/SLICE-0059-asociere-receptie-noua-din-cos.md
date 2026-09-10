# SLICE 0059 — «Începe o recepție nouă», din coșul instantaneelor neașezate

Cererea operatorului: în `AsociereForm`, clic dreapta pe un instantaneu din arborele
**Neașezate** trebuie să ofere și opțiunea de a porni de la el o **recepție nouă (R)**.

---

## 1. Ce lipsea, de fapt

Coșul din dreapta avea până acum exact două ieșiri: **pune instantaneul pe o recepție de
aici** (tragere) sau **nu consemnează nicio schimbare** (F17, din meniu). Amândouă
presupun că recepția lui există undeva.

F26 spune că nu întotdeauna există. O recepție creată **și** ștearsă pe site înainte ca
K-BOT să fi descărcat vreodată angajamentul nu are rând în `ListaReceptii`, deci nu are
rând în `FX_Receptii_R` — dar instantaneele ei sunt toate în istoric, fiindcă istoricul
vine întreg. Pentru acele instantanee coșul era o fundătură: nu aveau pe ce fi puse, iar
«fără schimbare» ar fi fost o minciună.

**Serverul știa deja să le primească.** Acțiunea `reconstituire` și câmpul
`receptie_noua` există în `routes/forexe/asociere.py` și în
`routes/forexe/prelucrare_asociere.py` (§4c-bis, `verifica_etichetele` +
`materializeaza_receptiile_reconstituite`), sunt duse până la capăt prin `ApiClient` și
prin `ComandaAsociere.ReceptieNoua` / `DecizieAsociere.ReceptieNoua`, și au teste.
**Nimic din tot lanțul ăsta nu era chemat de nicăieri** — formularul nu avea niciun gest
care să producă o `reconstituire`. Felia asta nu adaugă un contract, ci pune capătul de
client pe unul care aștepta.

Nicio linie de Python nu s-a atins.

---

## 2. Cum e ținută o recepție nouă în formular

**Un IDRR NEGATIV în `_pozitie`, și atât.** `_pozitie` rămâne singura evidență a
așezărilor; o recepție pornită aici e mulțimea instantaneelor care poartă același număr
negativ. Lista `_receptiiNoi` e o **proiecție**, refăcută din `_pozitie` la fiecare
`Reconstruieste` (`ActualizeazaReceptiileNoi`), nu o a doua evidență care s-ar putea
contrazice cu prima.

Trei lucruri ies gratis din alegerea asta:

- **Recepția dispare singură** când pleacă de pe ea și ultimul instantaneu — nu «rămâne
  goală» și nu trebuie ștearsă de nimeni.
- **Toate suprafețele o văd fără să știe de ea**: arborele din stânga, graficul, benzile
  și țintele de tragere citesc acum `Receptiile()` (server + pornite aici) în loc de
  `_stare.Receptii`. Cheia de nod e `R:-1`, cheia de serie `R-1`, iar `IdrrDinCheie` le
  citește ca pe oricare altele.
- **`_stare` rămâne neatins**, adică rămâne adevărul de pe server, cu care se compară ce
  s-a schimbat.

Cifrele rândului se **recalculează** din lanț la fiecare mutare, fiindcă exact așa le va
scrie serverul: `DataR` = momentul celui mai vechi instantaneu, `SumaAntet` = totalul
rândului de ștergere. Un rând care ar arăta altceva decât ce se va scrie ar fi singura
minciună de pe ecran.

`EtichetaTrimisa(-1)` = `"R1"`. Numerele nu se reciclează cât ține fereastra: două
recepții pornite una după alta pe același număr ar ajunge la server cu aceeași etichetă,
iar acolo o etichetă se declară **o singură dată**.

---

## 3. Ce pleacă pe fir

Serverul cere, pentru fiecare lanț reconstituit: exact o `reconstituire` (care declară
eticheta), exact o `stergere`, iar ștergerea să fie **ultima** din lanț.

`Comenzi()` (modul de oricând) și `DeciziiDin` (modul propunere) traduc la fel, prin
aceeași pereche de funcții — `Declarantii…` alege **o dată** cine declară (primul
instantaneu după `DataH`), iar `ActiuneaPeReceptieNoua` dă acțiunea rândului:

| rândul | acțiune | ce poartă |
|---|---|---|
| primul din lanț | `reconstituire` | `receptie_noua` |
| marcat «rândul de ștergere» | `stergere` | `receptie_noua` |
| restul | `asociat` | `receptie_noua` |

`Idrr` rămâne **nescris** pe toate trei: serverul cere exact una dintre `idrr` și
`receptie_noua`, iar recepția nu are încă o cheie de numit.

Alegerea declarantului se face o singură dată, în afara buclei, tocmai fiindcă rând cu
rând s-ar putea nimeri două `reconstituiri` pentru aceeași etichetă — sau niciuna.

---

## 4. Refuzurile care nu ajung la server

**F14 nu se aplică unei recepții pornite aici.** O astfel de recepție nu are linii pe
indicator și nici nu poate avea: serverul i le scrie la salvare din chiar instantaneele
lanțului. Verificat oricum, F14 ar fi refuzat **primul instantaneu al fiecărei recepții
noi**, adică exact gestul care o creează. F16 (mulțimile doar cresc de-a lungul lanțului)
rămâne, fiindcă e o regulă între instantanee, nu între instantaneu și recepție.

**Cele trei condiții de la §4c-bis se verifică ÎNAINTE de salvare**
(`ProblemaReceptiilorNoi`), fiindcă serverul refuză salvarea **întreagă**, nu doar
recepția cu pricina: cel puțin două instantanee, exact un rând de ștergere, și acela
ultimul. Cât nu sunt îndeplinite, butonul de salvare e stins și banda de mesaje spune ce
lipsește — în amândouă modurile.

**Renunțarea** e în meniul rândului-recepție: pune înapoi în coș toate instantaneele, iar
recepția piere cu ele. Fără ea, o recepție pornită din greșeală s-ar desface doar
desprinzând instantaneele unul câte unul.

---

## 5. Două lucruri prinse abia pe ecran

Formularul a fost desenat cu `DrawToBitmap` peste un tablou nascocit (trei stări: recepție
nouă cu un instantaneu, cu două, și închisă cu rândul de ștergere). Amândouă defectele de
mai jos treceau de toate testele.

1. **Semnul lung de pe rând ștergea rândul.** Prima variantă scria pe rândul-recepție și
   ce îi mai lipsește («— lipsește rândul de ștergere»). Partea din dreapta a rândului e
   aliniată la dreapta, iar când nu încape, arborele o lasă **nedesenată cu totul**: a
   dispărut și suma, și numărul de instantanee. Rândul poartă acum doar
   `[recepție nouă]`; propoziția s-a mutat în tooltip și în banda de mesaje, care au loc
   pentru ea.
2. **Avertismentul rămânea pe ecran după ce problema se rezolva.** Banda se scria la
   fiecare reconstruire, dar nu se ștergea niciodată. Se ia jos acum doar dacă tot noi
   l-am pus (`_avertismentReceptiiNoi`) — un `Clear` necondiționat ar fi șters și
   «legăturile au fost salvate», care vine tocmai după o reîncărcare.

---

## 6. Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.App/Forexe/AsociereForm.vb` | tot ce e mai sus (+331 / −13) |
| `tests/KBot.App.Tests/AsociereFormTests.vb` | 8 teste noi + `StareCuDouaNeasezate` și trei ajutoare de reflexie |
| `tests/KBot.App.Tests/AsociereDeciziiTests.vb` | 3 teste noi, pe drumul propunerii |

Nimic în `KBot.Domain`, `KBot.Api` sau `PYTHON/` — contractul era deja acolo.

---

## 7. Rezultatele testelor

| suită | rezultat |
|---|---|
| `dotnet build src\KBot.App\KBot.App.vbproj` | **0 erori**, 7 avertismente MSB3825 (BinaryFormatter în `.resx`, preexistente pe tot proiectul) |
| `dotnet test tests\KBot.App.Tests --filter ~Asociere` | **41 trecute / 0 picate** |
| `tests\KBot.App.Tests` (întreg) | 238 trecute / **13 picate** |
| `tests\KBot.Api.Tests` | 103 trecute / **1 picat** |
| `tests\KBot.Domain.Tests` | 28 trecute / **3 picate** |

**Cele 17 picate sunt preexistente și nu au legătură cu felia asta** — verificat prin
`git stash` al celor trei fișiere: pe arborele fără modificări suita `KBot.App.Tests` dă
**exact aceleași 13 picate** (227 trecute în loc de 238, adică toate cele 11 teste noi
trec). Toate sunt din familia DDF/revizii (`DdfXfaParserTests`, `DdfViewTests`,
`IstoricViewTests`, `XfaXmlPreviewTests`, `MainFormNavItemsTests`,
`ApiClientTests.GetDdf_FormatsRevisionLabel…`, `DdfInfoTests.EtichetaRevizie…`).

`dotnet test KBot.sln` **nu s-a rulat** — deschide ferestre Adobe reale pe ecranul
operatorului (regula casei).

---

## 8. Ce a rămas neverificat

- **Nu s-a salvat nimic pe o bază vie.** Serverul nu s-a pornit. Că lanțul
  `reconstituire` + `stergere` cu aceeași etichetă e ce așteaptă
  `verifica_etichetele` / `materializeaza_receptiile_reconstituite` e citit din codul
  Python, nu observat pe o rulare.
- **Nu s-a rulat suita PYTHON** — n-a fost atins niciun fișier Python.
- **Fereastra mare de benzi** (`AsociereBenziForm`) arată recepțiile noi și primește
  trageri pe ele (împrumută aceiași trei tratatori), dar **nu s-a văzut pe ecran** în
  sesiunea asta, și nu are niciun gest de creare — recepția se pornește doar din arbore.
- **Modul propunere nu s-a văzut pe ecran** deloc; drumul lui e acoperit numai de testele
  pe `DeciziiDin`. Capturile sunt toate din modul de oricând.
- `KBotLaneView` tot **nu are niciun test** (semnalat și de felia 0058).

---

# 0059-02 — corectură: recepția pornită aici e ȘTEARSĂ prin definiție

Observația operatorului, după prima citire pe ecran: *«recepția pornită e întotdeauna o
recepție ștearsă. NU există fel în care un R pornit de mână să nu fie șters. Deci asta
trebuie să se vadă și în mesaj, și în culoare. În rest e ca un R normal — intră în
calculul ordonanțărilor și e parte din grafic și din benzi.»*

Are dreptate, și taie o întrebare pe care prima variantă o punea degeaba.

## 1. Rândul de ștergere nu mai e o hotărâre, e o citire

O recepție ajunge să fie pornită de aici DOAR fiindcă a fost creată și ștearsă pe site
înainte de prima descărcare (F26). Dacă n-ar fi fost ștearsă, ar fi venit în
`ListaReceptii` ca toate celelalte și n-ar fi avut ce căuta în coș. Deci «e asta rândul
de ștergere?» n-are decât un răspuns: **ultimul instantaneu al lanțului**.

- `_stergereNoua` ține IDRH-urile care închid lanțurile pornite aici — tot o proiecție a
  lui `_pozitie`, refăcută în aceeași trecere cu `_receptiiNoi`.
- `EsteStergere` are acum două regimuri: pe o recepție de la server citește steagul
  operatorului (acolo recepția poate fi încă vie, iar mașina nu poate ghici), pe una
  pornită aici citește capătul lanțului și **ignoră** `_stergere`. Un steag rămas de pe o
  așezare anterioară ar fi arătat pe ecran un rând de ștergere pe care firul nu-l trimite.
- În meniul unui instantaneu de pe o recepție pornită aici nu se mai oferă «Este rândul de
  ștergere» — o intrare care nu schimbă nimic e mai rea decât niciuna.
- `Comenzi` și `DeciziiDin` iau amândouă capetele din aceeași ordonare (`CapeteLant`:
  `Declara` = primul, `Inchide` = ultimul), nu din dicționarul de ștergeri.
- Din cele patru verificări dinainte de salvare **a rămas una**: lanțul are nevoie de cel
  puțin două instantanee. Celelalte trei (zero ștergeri, două ștergeri, ștergerea la
  mijloc) nu se mai pot întâmpla de când capetele se citesc, nu se marchează.

## 2. «În mesaj»

- Rândul-recepție poartă `[nouă, ștearsă]` în loc de `[recepție nouă]`. Un singur semn,
  deși recepția are și `Sters`, și `Reconstituit`: trei scrise unul după altul ar spune
  același lucru de trei ori și ar umple partea din dreapta a rândului, care se pierde
  întreagă când nu încape (vezi §5 mai sus).
- Proiecția poartă acum `Sters = True` și `Reconstituit = True`, fiindcă exact așa o scrie
  `_R_INSERT_RECONST_SQL`. Rândul de pe ecran arată ce se va scrie.
- Tooltipul spune deschis «se va scrie ca recepție ȘTEARSĂ și reconstituită — o recepție
  de aici există tocmai fiindcă a fost ștearsă», și că ultimul instantaneu e ștergerea ei.
- Suma se citește «Valoarea la ștergere», nu «Valoare acum»: «acum» ar fi o minciună
  pentru o recepție care nu mai există.

## 3. «Și în culoare»

**Pe FOND, nu pe textul rândului.** Textul rândului-recepție e deja luat: pe fila «Tot
angajamentul» el poartă culoarea liniei din grafic, adică lucrul care leagă rândul de
linia lui. Scrisă tot acolo, ștergerea ar fi ori ștearsă de identitate, ori ar fi șters-o
pe ea — și tocmai la recepția pornită aici operatorul are mai multă nevoie să vadă care
linie e a ei, în timp ce trage. Fondul nu-l cere nimeni altcineva, deci cele două fapte
încap amândouă, pe amândouă filele.

Nuanța se AMESTECĂ din paletă (`SurfaceAlt` + 14% `TextDim`), fiindcă `SurfaceAlt` singur
e chiar alb în temele deschise, adică fondul arborelui — invizibil, verificat pe ecran. Se
întoarce singură pe dos în tema întunecată. Nicio culoare scrisă în cod.

Aceeași nuanță pentru ORICE recepție ștearsă, nu doar pentru cele pornite aici: e același
fapt, iar două recepții amândouă șterse, dintre care numai una se vede că e, ar fi mai rău
decât nicio culoare. Și e o nuanță, nu o culoare tare — ștearsă rămâne o recepție
întreagă, cu ordonanțările care-i citesc totalul așa cum stătea atunci (§1.3).

## 4. «În rest e ca un R normal»

Două locuri unde nu era, amândouă din același `> 0` care nu lăsa niciodată un IDRR negativ
să treacă:

- `SincronizeazaCulorile` colora rândul-recepție cu culoarea liniei din grafic doar pentru
  `idrr > 0` — recepția pornită aici era singura al cărei rând nu se lega de linia lui.
- `AplicaCulorileBenzii` la fel, pentru bandă.

Amândouă sunt acum `<> 0`. `ValoareaLa` (deci și totalul din grafic, și reperele
plăților) cădea deja corect la zero după ștergere, fiindcă citește `EsteStergere`.

## 5. Un defect prins iar numai pe ecran

`Amesteca` arunca `OverflowException` la prima recepție ștearsă: `Color.R` e `Byte`, iar în
VB scăderea a doi `Byte` se face tot pe `Byte`, deci o diferență negativă crapă. Arborele
își înghite excepția (graniță de UI, regula casei) și **se oprea din desenat la jumătate**
— rândul-recepție scris, niciun instantaneu sub el, coșul lipsă cu totul. Toate testele
treceau. Canalele se urcă acum la `Double` înainte de scădere.

## 6. Alt defect, din felia 0059

`AratMeniul` întreba `PozitiaLui(inst) > 0` ca să știe dacă instantaneul e așezat. Un
instantaneu de pe o recepție pornită aici are IDRR negativ, deci cădea pe ramura coșului:
i se ofereau «nu consemnează nicio schimbare» și «începe o recepție nouă» pe ceva care
stătea deja pe una, și NU i se oferea desprinderea. Acum `<> 0`.

## 7. Rezultate

| suită | rezultat |
|---|---|
| `dotnet build src\KBot.App\KBot.App.vbproj` | **0 erori, 0 avertismente** |
| `--filter ~Asociere` | **47 trecute / 0 picate** |
| `tests\KBot.App.Tests` (întreg) | 241 trecute / **13 picate** — exact cele preexistente (DDF/revizii/Istoric) |

Văzut pe ecran (`DrawToBitmap`, 1500×950, modul de oricând): recepție nouă cu un
instantaneu (bandă de avertisment, salvare stinsă), cu două, cu trei și închisă, plus fila
«Tot angajamentul» — acolo se vede și rândul verde legat de linia lui verde, cu fondul de
recepție ștearsă peste. **Modul propunere tot nu s-a văzut pe ecran**, și tot **nu s-a
salvat nimic pe o bază vie**.
