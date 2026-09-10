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
